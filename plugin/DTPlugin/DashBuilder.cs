using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FedeDashboard
{
    /// <summary>
    /// Ricompone "DT Dashboard.djson" con solo i layout delle pagine configurate.
    /// Una dash con tutte le combinazioni (cornice x griglia x barra) avrebbe centinaia di sotto-dashboard
    /// e su schermi come il VoCore non si carica: qui ne restano poche decine.
    /// Usa i file "DT Dashboard.djson.modello" e "DT Dashboard.pezzi" forniti con la dashboard.
    /// </summary>
    public class DashBuilder
    {
        const string DashName = "DT Dashboard";
        string lastSignature = "";
        DateTime nextCheck = DateTime.MinValue;
        volatile bool working;
        public volatile string Status = "";

        public static string DashFolder
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashTemplates", DashName); }
        }

        static string Signature(FedeSettings s)
        {
            var sb = new StringBuilder();
            foreach (var p in s.Pages)
                sb.Append(p.Frame).Append(',').Append(p.GridLeft).Append(',').Append(p.GridRight).Append(',').Append(p.Toolbar).Append(';');
            if (s.Font != null) sb.Append("F").Append(s.Font.Key);
            return sb.ToString();
        }

        /// <summary>Da chiamare spesso (DataUpdate): ogni 2 s controlla se i layout delle pagine sono cambiati.</summary>
        public void Tick(FedeSettings s, DateTime now)
        {
            if (working || now < nextCheck) return;
            nextCheck = now.AddSeconds(2);
            string sig = Signature(s);
            if (sig == lastSignature) return;
            working = true;
            var pages = new List<PageSettings>(s.Pages);
            var font = s.Font;
            new Thread(() =>
            {
                try { if (Build(pages, font)) lastSignature = sig; }
                catch (Exception ex) { Status = "Errore nel comporre la dashboard: " + ex.Message; FedeLog.Error("componi dashboard", ex); }
                finally { working = false; }
            }) { IsBackground = true, Name = "DT dash builder" }.Start();
        }

        /// <summary>Forza la ricomposizione al prossimo controllo (es. dopo un aggiornamento della dashboard).</summary>
        public void Invalidate() { lastSignature = ""; nextCheck = DateTime.MinValue; }

        /// <summary>Scrive la dash per queste pagine. Restituisce true se e' a posto (scritta o gia' uguale).</summary>
        public bool Build(List<PageSettings> pages, FontSettings font = null)
        {
            string folder = DashFolder;
            string tplPath = Path.Combine(folder, DashName + ".djson.modello");
            string piecesPath = Path.Combine(folder, DashName + ".pezzi");
            string outPath = Path.Combine(folder, DashName + ".djson");
            if (!File.Exists(tplPath) || !File.Exists(piecesPath))
            {
                Status = "Dashboard non trovata in DashTemplates (importa DT Dashboard).";
                return true;   // niente da fare: non riprovare in continuazione
            }
            var pieces = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(piecesPath, Encoding.UTF8))
            {
                int tab = line.IndexOf('\t');
                if (tab > 0) pieces[line.Substring(0, tab)] = line.Substring(tab + 1);
            }
            string pageTpl;
            if (!pieces.TryGetValue("PAGINA", out pageTpl)) throw new InvalidDataException("pezzi senza PAGINA");

            var layers = new List<string>();
            for (int i = 0; i < pages.Count; i++)
            {
                var p = pages[i];
                int f = Clamp(p.Frame, Catalog.Frames.Length), gl = Clamp(p.GridLeft, Catalog.Grids.Length),
                    gr = Clamp(p.GridRight, Catalog.Grids.Length), tb = Clamp(p.Toolbar, Catalog.Toolbars.Length);
                var kids = new List<string>();
                AddArray(kids, pieces, "CENTRO_" + f);
                AddArray(kids, pieces, "GRIGLIA_" + f + "_L_" + gl);
                AddArray(kids, pieces, "GRIGLIA_" + f + "_R_" + gr);
                AddArray(kids, pieces, "BARRA_" + f + "_" + tb);
                AddArray(kids, pieces, "LATERALI_" + f);
                string idx = (i + 1).ToString(CultureInfo.InvariantCulture);
                layers.Add(pageTpl.Replace("\"@@FIGLI@@\"", string.Join(",", kids)).Replace("@@IDX@@", idx));
            }
            string tpl = File.ReadAllText(tplPath, Encoding.UTF8);
            if (!tpl.Contains("\"@@PAGINE@@\"")) throw new InvalidDataException("modello senza @@PAGINE@@");
            string json = ApplyFont(tpl.Replace("\"@@PAGINE@@\"", string.Join(",", layers)), font);
            try { ApplyFontToOtherFiles(folder, outPath, font); } catch (Exception ex) { FedeLog.Error("carattere schede", ex); }

            if (File.Exists(outPath) && File.ReadAllText(outPath, Encoding.UTF8) == json)
            {
                Status = "Dashboard a posto (" + pages.Count + " pagine).";
                return true;
            }
            string tmp = outPath + ".tmp";
            File.WriteAllText(tmp, json, new UTF8Encoding(false));
            if (File.Exists(outPath)) File.Replace(tmp, outPath, null);
            else File.Move(tmp, outPath);
            Status = "Dashboard ricomposta per " + pages.Count + " pagine.";
            FedeLog.Write("Dashboard ricomposta: " + pages.Count + " pagine");
            return true;
        }

        // ------------------------------------------------------------------ carattere
        static readonly string[] WeightLadder = { "Light", "Normal", "Medium", "Bold", "Black" };
        static readonly Regex FontRx = new Regex("\"Font\":\"Roboto\"", RegexOptions.Compiled);
        static readonly Regex WeightRx = new Regex("\"FontWeight\":\"(Light|Normal|Medium|Bold|Black)\"", RegexOptions.Compiled);
        static readonly Regex SizeRx = new Regex("\"FontSize\":(\\d+(?:\\.\\d+)?)", RegexOptions.Compiled);

        /// <summary>Cambia famiglia, spessore e dimensione dei testi nel json di una dashboard.</summary>
        public static string ApplyFont(string json, FontSettings f)
        {
            if (f == null || f.IsDefault) return json;
            string fam = FontSettings.Families[f.Family < 0 || f.Family >= FontSettings.Families.Length ? 0 : f.Family];
            int step = FontSettings.WeightSteps[f.Weight < 0 || f.Weight >= FontSettings.WeightSteps.Length ? 0 : f.Weight];
            double k = f.SizePct / 100.0;
            if (fam != "Roboto") json = FontRx.Replace(json, "\"Font\":\"" + fam + "\"");
            if (step != 0)
                json = WeightRx.Replace(json, m =>
                {
                    int i = Array.IndexOf(WeightLadder, m.Groups[1].Value) + step;
                    if (i < 0) i = 0; if (i >= WeightLadder.Length) i = WeightLadder.Length - 1;
                    return "\"FontWeight\":\"" + WeightLadder[i] + "\"";
                });
            if (Math.Abs(k - 1) > 0.001)
                json = SizeRx.Replace(json, m =>
                {
                    double v = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) * k;
                    if (v < 6) v = 6;
                    return "\"FontSize\":" + Math.Round(v, 1).ToString("0.0", CultureInfo.InvariantCulture);
                });
            return json;
        }

        /// <summary>
        /// Applica il carattere anche alle schede (gli altri .djson della cartella).
        /// L'originale di ogni file resta in "nome.djson.orig"; se il file e' stato sostituito (dashboard reimportata)
        /// l'originale viene ripreso dal file nuovo.
        /// </summary>
        static void ApplyFontToOtherFiles(string folder, string mainPath, FontSettings font)
        {
            string stampPath = Path.Combine(folder, DashName + ".carattere");
            var last = new FontSettings();
            if (File.Exists(stampPath))
            {
                var parts = File.ReadAllText(stampPath).Trim().Split('/');
                int a, b, c;
                if (parts.Length == 3 && int.TryParse(parts[0], out a) && int.TryParse(parts[1], out b) && int.TryParse(parts[2], out c))
                { last.Family = a; last.Weight = b; last.SizePct = c; last.Normalize(); }
            }
            var now = font ?? new FontSettings();
            if (now.IsDefault && last.IsDefault && !File.Exists(stampPath)) return;   // mai cambiato: niente da fare
            int changed = 0;
            foreach (var file in Directory.GetFiles(folder, "*.djson"))
            {
                if (string.Equals(Path.GetFullPath(file), Path.GetFullPath(mainPath), StringComparison.OrdinalIgnoreCase)) continue;
                string cur = File.ReadAllText(file, Encoding.UTF8);
                string origPath = file + ".orig";
                string orig = File.Exists(origPath) ? File.ReadAllText(origPath, Encoding.UTF8) : null;
                if (orig == null || ApplyFont(orig, last) != cur)
                {
                    orig = cur;   // file nuovo o reimportato: questo e' l'originale
                    File.WriteAllText(origPath, orig, new UTF8Encoding(false));
                }
                string want = ApplyFont(orig, now);
                if (want != cur)
                {
                    string tmp = file + ".tmp";
                    File.WriteAllText(tmp, want, new UTF8Encoding(false));
                    File.Replace(tmp, file, null);
                    changed++;
                }
            }
            File.WriteAllText(stampPath, now.Key);
            if (changed > 0) FedeLog.Write("Carattere applicato a " + changed + " schede: " + now.Key);
        }

        static int Clamp(int v, int n) { return v < 0 ? 0 : (v >= n ? n - 1 : v); }

        static void AddArray(List<string> kids, Dictionary<string, string> pieces, string key)
        {
            string arr;
            if (!pieces.TryGetValue(key, out arr)) return;
            arr = arr.Trim();
            if (arr.Length < 2 || arr[0] != '[') return;
            string inner = arr.Substring(1, arr.Length - 2).Trim();
            if (inner.Length > 0) kids.Add(inner);
        }
    }
}
