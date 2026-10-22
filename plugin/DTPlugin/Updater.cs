using System;
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
    /// Aggiornamento automatico da GitHub (release del repository pubblico):
    ///  - controlla l'ultima release (tag "v1.36") all'avvio e a richiesta;
    ///  - "Installa" scarica DTPlugin.dll e DT Dashboard.simhubdash dalla release,
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
        string dllUrl, dashUrl;
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

        static WebClient Client()
        {
            try { ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; } catch { }
            var wc = new WebClient();
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
            if (Busy) return;
            new Thread(() => Check(currentVersion)) { IsBackground = true, Name = "DTPlugin updater" }.Start();
        }

        public void Check(string currentVersion)
        {
            Busy = true; Status = "Controllo aggiornamenti…"; Notify();
            try
            {
                string json;
                using (var wc = Client()) json = wc.DownloadString("https://api.github.com/repos/" + Repo + "/releases/latest");
                string tag = Field(json, "tag_name");
                LatestVersion = tag.TrimStart('v', 'V');
                Notes = Field(json, "body");
                dllUrl = AssetUrl(json, DllAsset);
                dashUrl = AssetUrl(json, DashAsset) ?? AssetUrl(json, "DT Dashboard.simhubdash");
                Available = IsNewer(LatestVersion, currentVersion) && dllUrl != null;
                Status = Available ? "Disponibile la versione " + LatestVersion + " (tu hai la " + currentVersion + ")."
                                   : "Hai l'ultima versione (" + currentVersion + ").";
            }
            catch (WebException ex)
            {
                var r = ex.Response as HttpWebResponse;
                Status = r != null && r.StatusCode == HttpStatusCode.NotFound ? "Nessuna versione pubblicata su GitHub per ora."
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
                    using (var wc = Client()) wc.DownloadFile(dashUrl, zip);
                    InstallDash(zip, tmp);
                }
                // 2) plugin
                Status = "Scarico il plugin…"; Notify();
                string newDll = Path.Combine(tmp, DllAsset);
                using (var wc = Client()) wc.DownloadFile(dllUrl, newDll);
                if (new FileInfo(newDll).Length < 20000) throw new IOException("file del plugin scaricato non valido");
                string own = typeof(Updater).Assembly.Location;
                string old = own + ".old";
                if (File.Exists(old)) { try { File.Delete(old); } catch { old = own + ".old" + DateTime.Now.Ticks; } }
                File.Move(own, old);                     // il file in uso si puo' rinominare
                try { File.Copy(newDll, own, true); }
                catch { File.Move(old, own); throw; }  // ripristina se la copia fallisce
                Installed = true; Available = false;
                Status = "Versione " + LatestVersion + " installata. Riavvia SimHub per usarla.";
                FedeLog.Write("Aggiornato alla versione " + LatestVersion);
            }
            catch (UnauthorizedAccessException ex)
            {
                Status = "Windows non permette di scrivere nella cartella di SimHub: avvia SimHub come amministratore e riprova.";
                FedeLog.Error("installazione aggiornamento", ex);
            }
            catch (Exception ex) { Status = "Installazione non riuscita: " + ex.Message; FedeLog.Error("installazione aggiornamento", ex); }
            finally { try { Directory.Delete(tmp, true); } catch { } Busy = false; Notify(); }
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
