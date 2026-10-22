using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FedeDashboard
{
    /// <summary>
    /// Aggiornamento automatico da GitHub (cartella "release" del repository pubblico):
    ///  - controlla release/latest.json ({"version":"1.37","notes":"..."}) all'avvio e a richiesta;
    ///    se non c'e', prova l'ultima "release" di GitHub;
    ///  - "Installa" scarica release/DTPlugin.dll e release/DT.Dashboard.simhubdash,
    ///    rinomina il plugin in uso in DTPlugin.dll.old (Windows lo permette anche se e' caricato),
    ///    mette il nuovo al suo posto e copia la dashboard in SimHub\DashTemplates;
    ///  - poi basta riavviare SimHub. Le impostazioni (DTPlugin.json) non vengono toccate.
    /// </summary>
    public class Updater
    {
        public const string Repo = "Federico-80/dt-dashboard";
        const string DllAsset = "DTPlugin.dll";
        const string DashAsset = "DT.Dashboard.simhubdash";   // GitHub sostituisce gli spazi con punti nei nomi degli allegati
        const string DashFolder = "DT Dashboard";

        public volatile string Status = "";
        public volatile string LatestVersion = "";
        public volatile string Notes = "";
        public volatile bool Available, Busy, Installed;
        string dllUrl, dashUrl, rawError;
        public event Action Changed;

        static string SimHubDir { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        void Notify() { try { var c = Changed; if (c != null) c(); } catch { } }

        /// <summary>Al primo avvio dopo un aggiornamento cancella il vecchio plugin rinominato.</summary>
        public static void CleanupOld()
        {
            try
            {
                string own = typeof(Updater).Assembly.Location;
                if (string.IsNullOrEmpty(own)) return;
                foreach (var f in Directory.GetFiles(Path.GetDirectoryName(own), Path.GetFileName(own) + ".old*"))
                    try { File.Delete(f); } catch { }
            }
            catch { }
        }

        /// <summary>WebClient con timeout (30 s) e TLS 1.2 (richiesto da GitHub).</summary>
        class TimedWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                var r = base.GetWebRequest(address);
                if (r != null) r.Timeout = 30000;
                return r;
            }
        }

        static WebClient Client()
        {
            try { ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; } catch { }
            var wc = new TimedWebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "DTPlugin-Updater";
            wc.Encoding = Encoding.UTF8;
            return wc;
        }

        public static int[] ParseVersion(string v)
        {
            var m = Regex.Match(v ?? "", @"(\d+)(?:\.(\d+))?(?:\.(\d+))?");
            if (!m.Success) return new[] { 0, 0, 0 };
            Func<int, int> g = i => m.Groups[i].Success ? int.Parse(m.Groups[i].Value, CultureInfo.InvariantCulture) : 0;
            return new[] { g(1), g(2), g(3) };
        }

        public static bool IsNewer(string remote, string local)
        {
            var a = ParseVersion(remote); var b = ParseVersion(local);
            for (int i = 0; i < 3; i++) { if (a[i] != b[i]) return a[i] > b[i]; }
            return false;
        }

        /// <summary>Controlla l'ultima release su GitHub (in background).</summary>
        public void CheckAsync(string currentVersion)
        {
            if (Busy) { Notify(); return; }
            Status = "Controllo aggiornamenti…"; Notify();
            new Thread(() => Check(currentVersion)) { IsBackground = true, Name = "DTPlugin updater" }.Start();
        }

        public void Check(string currentVersion)
        {
            Busy = true; Status = "Controllo aggiornamenti…"; Notify();
            try
            {
                string json = null; rawError = null;
                // 1) file nel repository, letti dalle API di GitHub (sempre aggiornate: raw.githubusercontent tiene copie per minuti)
                string api = "https://api.github.com/repos/" + Repo + "/contents/release/";
                try
                {
                    using (var wc = Client()) { wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github.raw"; json = wc.DownloadString(api + "latest.json"); }
                    LatestVersion = Field(json, "version").TrimStart('v', 'V');
                    Notes = Field(json, "notes");
                    dllUrl = api + DllAsset;
                    dashUrl = api + DashAsset;
                }
                catch (WebException wex) { json = null; rawError = wex.Message; FedeLog.Write("Aggiornamenti (release/latest.json): " + wex.Message); }
                // 2) in alternativa la "release" di GitHub
                if (json == null)
                {
                    using (var wc = Client()) json = wc.DownloadString("https://api.github.com/repos/" + Repo + "/releases/latest");
                    LatestVersion = Field(json, "tag_name").TrimStart('v', 'V');
                    Notes = Field(json, "body");
                    dllUrl = AssetUrl(json, DllAsset);
                    dashUrl = AssetUrl(json, DashAsset) ?? AssetUrl(json, "DT Dashboard.simhubdash");
                }
                Available = IsNewer(LatestVersion, currentVersion) && dllUrl != null;
                Status = Available ? "Disponibile la versione " + LatestVersion + " (tu hai la " + currentVersion + ")."
                                   : "Hai l'ultima versione (" + currentVersion + ").";
                FedeLog.Write("Aggiornamenti: ultima " + LatestVersion + ", installata " + currentVersion + " -> " + (Available ? "da aggiornare" : "ok"));
            }
            catch (WebException ex)
            {
                var r = ex.Response as HttpWebResponse;
                Status = rawError != null ? "Impossibile leggere gli aggiornamenti da GitHub (" + rawError + ")."
                       : r != null && r.StatusCode == HttpStatusCode.NotFound ? "Nessuna versione pubblicata su GitHub per ora."
                       : "Impossibile controllare gli aggiornamenti (connessione?).";
                FedeLog.Write("Aggiornamenti: " + ex.Message);
            }
            catch (Exception ex) { Status = "Errore nel controllo aggiornamenti."; FedeLog.Error("aggiornamenti", ex); }
            Busy = false; Notify();
        }

        /// <summary>Scarica e installa plugin e dashboard. Restituisce true se serve riavviare SimHub.</summary>
        public void InstallAsync()
        {
            if (Busy || !Available) return;
            new Thread(Install) { IsBackground = true, Name = "DTPlugin install" }.Start();
        }

        void Install()
        {
            Busy = true; Notify();
            string tmp = Path.Combine(Path.GetTempPath(), "DTPlugin_update_" + DateTime.Now.Ticks);
            try
            {
                Directory.CreateDirectory(tmp);
                // 1) dashboard
                if (dashUrl != null)
                {
                    Status = "Scarico la dashboard…"; Notify();
                    string zip = Path.Combine(tmp, "dash.simhubdash");
                    using (var wc = Client()) { wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github.raw"; wc.DownloadFile(dashUrl, zip); }
                    InstallDash(zip, tmp);
                }
                // 2) plugin
                Status = "Scarico il plugin…"; Notify();
                string newDll = Path.Combine(tmp, DllAsset);
                using (var wc = Client()) { wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github.raw"; wc.DownloadFile(dllUrl, newDll); }
                if (new FileInfo(newDll).Length < 20000) throw new IOException("file del plugin scaricato non valido");
                string own = typeof(Updater).Assembly.Location;
                string old = own + ".old";
                if (File.Exists(old)) { try { File.Delete(old); } catch { old = own + ".old" + DateTime.Now.Ticks; } }
                File.Move(own, old);                     // il file in uso si puo' rinominare
                try { File.Copy(newDll, own, true); }
                catch { File.Move(old, own); throw; }  // ripristina se la copia fallisce
                Installed = true; Available = false;
                Status = "Versione " + LatestVersion + " installata. Riavvio SimHub…";
                FedeLog.Write("Aggiornato alla versione " + LatestVersion + ": riavvio SimHub");
                Busy = false; Notify();
                Thread.Sleep(1500);
                RestartSimHub();
            }
            catch (UnauthorizedAccessException ex)
            {
                Status = "Windows non permette di scrivere nella cartella di SimHub: avvia SimHub come amministratore e riprova.";
                FedeLog.Error("installazione aggiornamento", ex);
            }
            catch (Exception ex) { Status = "Installazione non riuscita: " + ex.Message; FedeLog.Error("installazione aggiornamento", ex); }
            finally { try { Directory.Delete(tmp, true); } catch { } Busy = false; Notify(); }
        }

        /// <summary>
        /// Riavvia SimHub: un piccolo comando PowerShell aspetta che SimHub si chiuda e lo riapre,
        /// poi SimHub viene chiuso normalmente (salva le sue impostazioni). Se non si chiude entro 20 s, viene terminato.
        /// </summary>
        public void RestartSimHub()
        {
            try
            {
                var me = Process.GetCurrentProcess();
                string exe = me.MainModule.FileName;
                string dir = Path.GetDirectoryName(exe);
                string tmp = Path.Combine(Path.GetTempPath(), "DTPlugin-riavvio");
                Directory.CreateDirectory(tmp);
                string rlog = Path.Combine(FedeLog.Folder, "DTPlugin-riavvio.log");
                Func<string, string> q = x => "'" + x.Replace("'", "''") + "'";
                // script che aspetta la chiusura di SimHub e lo riapre
                string waiter = Path.Combine(tmp, "attendi.ps1");
                File.WriteAllText(waiter,
                    "$ErrorActionPreference = 'SilentlyContinue'\r\n" +
                    "$log = " + q(rlog) + "\r\n" +
                    "function L($m) { Add-Content -Path $log -Value ((Get-Date -Format 'HH:mm:ss') + '  Riavvio: ' + $m) }\r\n" +
                    "L 'attendo la chiusura di SimHub'\r\n" +
                    "Wait-Process -Id " + me.Id + " -Timeout 90\r\n" +
                    "if (Get-Process -Id " + me.Id + ") { L 'SimHub ancora aperto dopo 90 s, lo chiudo'; Stop-Process -Id " + me.Id + " -Force; Start-Sleep -Seconds 2 }\r\n" +
                    "Start-Sleep -Seconds 2\r\n" +
                    "try { Start-Process -FilePath " + q(exe) + " -WorkingDirectory " + q(dir) + " -ErrorAction Stop; L 'SimHub riaperto' } catch { L ('errore: ' + $_.Exception.Message) }\r\n",
                    new UTF8Encoding(true));
                // lancio lo script fuori dal processo di SimHub (tramite WMI), cosi' non viene chiuso insieme a SimHub
                string launcher = Path.Combine(tmp, "lancia.ps1");
                string cmd = "powershell.exe -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" + waiter + "\"";
                File.WriteAllText(launcher,
                    "$ErrorActionPreference = 'SilentlyContinue'\r\n" +
                    "$r = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{ CommandLine = " + q(cmd) + " }\r\n" +
                    "if (-not $r -or $r.ReturnValue -ne 0) { Add-Content -Path " + q(rlog) + " -Value ((Get-Date -Format 'HH:mm:ss') + '  Riavvio: WMI non disponibile, avvio diretto'); " +
                    "Start-Process -FilePath powershell.exe -ArgumentList '-NoProfile','-ExecutionPolicy','Bypass','-WindowStyle','Hidden','-File'," + q("\"" + waiter + "\"") + " -WindowStyle Hidden }\r\n",
                    new UTF8Encoding(true));
                var psi = new ProcessStartInfo("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"" + launcher + "\"")
                { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden };
                var lp = Process.Start(psi);
                // aspetto che il lanciatore abbia fatto partire lo script prima di chiudere SimHub
                try { if (lp != null) lp.WaitForExit(15000); } catch { }
                FedeLog.Write("Riavvio: script avviato, chiudo SimHub");
                // chiusura normale dell'applicazione WPF di SimHub
                var appType = Wpf.T("System.Windows.Application");
                object app = appType.GetProperty("Current").GetValue(null, null);
                if (app != null)
                {
                    object disp = appType.GetProperty("Dispatcher").GetValue(app, null);
                    Action shut = () => appType.GetMethod("Shutdown", Type.EmptyTypes).Invoke(app, null);
                    disp.GetType().GetMethod("BeginInvoke", new[] { typeof(Delegate), typeof(object[]) }).Invoke(disp, new object[] { shut, new object[0] });
                }
                new Thread(() => { Thread.Sleep(20000); Environment.Exit(0); }) { IsBackground = true }.Start();
            }
            catch (Exception ex)
            {
                Status = "Aggiornamento installato: chiudi e riapri SimHub.";
                FedeLog.Error("riavvio SimHub", ex);
                Notify();
            }
        }

        /// <summary>Estrae il .simhubdash (percorsi con "\") in SimHub\DashTemplates\DT Dashboard.</summary>
        static void InstallDash(string zipPath, string tmp)
        {
            string target = Path.Combine(SimHubDir, "DashTemplates", DashFolder);
            string stage = Path.Combine(tmp, "dash");
            using (var z = ZipFile.OpenRead(zipPath))
            {
                foreach (var e in z.Entries)
                {
                    string rel = e.FullName.Replace('\\', '/');
                    if (rel.StartsWith(DashFolder + "/", StringComparison.OrdinalIgnoreCase)) rel = rel.Substring(DashFolder.Length + 1);
                    if (rel.Length == 0 || rel.EndsWith("/") || rel.Contains("..")) continue;
                    string dest = Path.Combine(stage, rel.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    e.ExtractToFile(dest, true);
                }
            }
            if (!File.Exists(Path.Combine(stage, DashFolder + ".djson"))) throw new IOException("dashboard scaricata non valida");
            Directory.CreateDirectory(target);
            // tolgo i file vecchi (le schede possono cambiare nome), poi copio i nuovi
            foreach (var f in Directory.GetFiles(target, "*", SearchOption.AllDirectories))
                try { File.Delete(f); } catch { }
            foreach (var f in Directory.GetFiles(stage, "*", SearchOption.AllDirectories))
            {
                string dest = Path.Combine(target, f.Substring(stage.Length + 1));
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(f, dest, true);
            }
        }

        // -------- lettura minima del JSON di GitHub (senza librerie esterne)
        static string Field(string json, string name)
        {
            var m = Regex.Match(json, "\"" + Regex.Escape(name) + "\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)\"");
            return m.Success ? Regex.Unescape(m.Groups[1].Value) : "";
        }

        static string AssetUrl(string json, string assetName)
        {
            foreach (Match m in Regex.Matches(json, "\"browser_download_url\"\\s*:\\s*\"([^\"]+)\""))
            {
                string url = m.Groups[1].Value;
                string file = Uri.UnescapeDataString(url.Substring(url.LastIndexOf('/') + 1));
                if (string.Equals(file, assetName, StringComparison.OrdinalIgnoreCase)) return url;
            }
            return null;
        }
    }
}
