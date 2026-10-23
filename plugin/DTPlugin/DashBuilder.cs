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
        public bool Working { get { return working; } }
        /// <summary>True quando la dash e' stata riscritta dopo l'avvio: SimHub la vede solo riaprendola.</summary>
        public volatile bool NeedsReload;
        /// <summary>Da mettere a true dopo la prima composizione all'avvio (prima che SimHub carichi la dash).</summary>
        public volatile bool Started;
        /// <summary>Chiamata (sul thread del builder) quando la dash e' stata riscritta dopo l'avvio.</summary>
        public Func<bool> ReloadDash;

        public static string DashFolder
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DashTemplates", DashName); }
        }

        static string Signature(FedeSettings s)
        {
            var sb = new StringBuilder();
            foreach (var p in s.Pages)
                sb.Append(p.EffectiveFrame).Append(',').Append(p.GridLeft).Append(',').Append(p.GridRight).Append(',').Append(p.Toolbar).Append(';');
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
                try
                {
                    if (Build(pages, font)) lastSignature = sig;
                    if (NeedsReload && ReloadDash != null)
                    {
                        Thread.Sleep(300);
                        if (ReloadDash()) { NeedsReload = false; Status = "Dashboard aggiornata e ricaricata in SimHub."; }
                    }
                }
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
                int f = Clamp(p.EffectiveFrame, Catalog.Frames.Length), gl = Clamp(p.GridLeft, Catalog.Grids.Length),
                    gr = Clamp(p.GridRight, Catalog.Grids.Length), tb = Clamp(p.Toolbar, Catalog.Toolbars.Length);
                var kids = new List<string>();
                string single, stD, stS;
                int fs;
                if (pieces.TryGetValue("SINGOLA_" + f, out single) && int.TryParse(single.Trim(), out fs)
                    && pieces.TryGetValue("STRATO_D", out stD) && pieces.TryGetValue("STRATO_S", out stS))
                {
                    // barra doppia solo quando serve rifornire (DTPlugin.BarraDoppia), altrimenti la cornice con la barra singola
                    kids.Add(stD.Replace("\"@@FIGLI@@\"", string.Join(",", Layout(pieces, f, gl, gr, tb))));
                    kids.Add(stS.Replace("\"@@FIGLI@@\"", string.Join(",", Layout(pieces, fs, gl, gr, tb))));
                }
                else kids = Layout(pieces, f, gl, gr, tb);
                string idx = (i + 1).ToString(CultureInfo.InvariantCulture);
                layers.Add(pageTpl.Replace("\"@@FIGLI@@\"", string.Join(",", kids)).Replace("@@IDX@@", idx));
            }
            string tpl = File.ReadAllText(tplPath, Encoding.UTF8);
            if (!tpl.Contains("\"@@PAGINE@@\"")) throw new InvalidDataException("modello senza @@PAGINE@@");
            string json = ApplyStyle(tpl.Replace("\"@@PAGINE@@\"", string.Join(",", layers)), font, true, false);
            try { if (ApplyFontToOtherFiles(folder, outPath, font) > 0 && Started) NeedsReload = true; } catch (Exception ex) { FedeLog.Error("carattere schede", ex); }

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
            if (Started) NeedsReload = true;
            return true;
        }

        // ------------------------------------------------------------------ carattere
        static readonly string[] WeightLadder = { "Light", "Normal", "Medium", "Bold", "Black" };
        // DT Montserrat: Black diventa ExtraBold (il Black e' troppo pesante), il resto un gradino piu' marcato
        static readonly string[] MontserratWeights = { "Normal", "Medium", "SemiBold", "Bold", "ExtraBold" };
        const double MontserratScale = 0.88;
        static readonly Regex FontRx = new Regex("\"Font\":\"Roboto\"", RegexOptions.Compiled);
        static readonly Regex WeightRx = new Regex("\"FontWeight\":\"(Light|Normal|Medium|Bold|Black)\"", RegexOptions.Compiled);
        static readonly Regex SizeRx = new Regex("\"FontSize\":(\\d+(?:\\.\\d+)?)", RegexOptions.Compiled);

        static readonly Regex BorderRx = new Regex("\"BorderStyle\":\\{\"BorderTop\":\\d+,\"BorderBottom\":\\d+,\"BorderLeft\":\\d+,\"BorderRight\":\\d+", RegexOptions.Compiled);

        /// <summary>Schede delle righe centrali: la misura viene dal catalogo (Schede_LxA.djson).</summary>
        public static bool IsCenterCardFile(string name)
        {
            foreach (var fr in Catalog.CenterRects)
                for (int i = 3; i < fr.Length; i++)
                    if (name.StartsWith("Schede_" + fr[i][2] + "x" + fr[i][3] + ".", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>Carattere + bordi. centerFile = scheda delle righe centrali.</summary>
        public static string ApplyStyle(string json, FontSettings f, bool mainFile, bool centerFile)
        {
            if (f == null) return json;
            json = ApplyFont(json, f);
            int center = f.CenterBorder, card = f.CardBorder;
            if (center == 0 && card == 2) return json;   // come generata
            return BorderRx.Replace(json, m =>
            {
                int at = json.LastIndexOf("\"Name\":\"", m.Index, StringComparison.Ordinal);
                if (at < 0) return m.Value;
                int st = at + 8, en = json.IndexOf('"', st);
                string name = en > st ? json.Substring(st, en - st) : "";
                int px;
                if (mainFile && (name == "Marcia fondo" || name == "Velocità fondo")) px = center;
                else if (!mainFile && name == "Cornice") px = centerFile ? center : card;
                else return m.Value;
                string v = px.ToString(CultureInfo.InvariantCulture);
                return "\"BorderStyle\":{\"BorderTop\":" + v + ",\"BorderBottom\":" + v + ",\"BorderLeft\":" + v + ",\"BorderRight\":" + v;
            });
        }

        /// <summary>Cambia famiglia, spessore e dimensione dei testi nel json di una dashboard.</summary>
        public static string ApplyFont(string json, FontSettings f)
        {
            if (f == null || (f.Family == 0 && f.Weight == 0 && f.SizePct == 100)) return json;
            string fam = FontSettings.Families[f.Family < 0 || f.Family >= FontSettings.Families.Length ? 0 : f.Family];
            int step = FontSettings.WeightSteps[f.Weight < 0 || f.Weight >= FontSettings.WeightSteps.Length ? 0 : f.Weight];
            double k = f.SizePct / 100.0;
            bool mont = fam == FontInstaller.Family;
            if (mont)
            {
                json = FontRx.Replace(json, "\"Font\":\"" + FontInstaller.FontValue() + "\"");
                k *= MontserratScale;   // lettere piu' larghe di Roboto
            }
            else if (fam != "Roboto") json = FontRx.Replace(json, "\"Font\":\"" + fam + "\"");
            if (step != 0 || mont)
                json = WeightRx.Replace(json, m =>
                {
                    int i = Array.IndexOf(WeightLadder, m.Groups[1].Value) + step;
                    if (i < 0) i = 0; if (i >= WeightLadder.Length) i = WeightLadder.Length - 1;
                    return "\"FontWeight\":\"" + (mont ? MontserratWeights[i] : WeightLadder[i]) + "\"";
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
        static int ApplyFontToOtherFiles(string folder, string mainPath, FontSettings font)
        {
            string stampPath = Path.Combine(folder, DashName + ".carattere");
            var last = new FontSettings();
            if (File.Exists(stampPath))
            {
                var parts = File.ReadAllText(stampPath).Trim().Split('/');
                int a, b, c;
                int d, e;
                if (parts.Length >= 3 && int.TryParse(parts[0], out a) && int.TryParse(parts[1], out b) && int.TryParse(parts[2], out c))
                { last.Family = a; last.Weight = b; last.SizePct = c; }
                if (parts.Length >= 5 && int.TryParse(parts[3], out d) && int.TryParse(parts[4], out e)) { last.CenterBorder = d; last.CardBorder = e; }
                last.Normalize();
            }
            var now = font ?? new FontSettings();
            if (now.IsDefault && last.IsDefault && !File.Exists(stampPath)) return 0;   // mai cambiato: niente da fare
            int changed = 0;
            foreach (var file in Directory.GetFiles(folder, "*.djson"))
            {
                if (string.Equals(Path.GetFullPath(file), Path.GetFullPath(mainPath), StringComparison.OrdinalIgnoreCase)) continue;
                string cur = File.ReadAllText(file, Encoding.UTF8);
                string origPath = file + ".orig";
                string orig = File.Exists(origPath) ? File.ReadAllText(origPath, Encoding.UTF8) : null;
                bool centerFile = IsCenterCardFile(Path.GetFileName(file));
                // (anche col bordo opposto: le versioni prima della 1.92 riconoscevano le righe centrali dalla vecchia misura)
                if (orig == null || (ApplyStyle(orig, last, false, centerFile) != cur && ApplyStyle(orig, last, false, !centerFile) != cur))
                {
                    orig = cur;   // file nuovo o reimportato: questo e' l'originale
                    File.WriteAllText(origPath, orig, new UTF8Encoding(false));
                }
                string want = ApplyStyle(orig, now, false, centerFile);
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
            return changed;
        }

        static List<string> Layout(Dictionary<string, string> pieces, int f, int gl, int gr, int tb)
        {
            var kids = new List<string>();
            AddArray(kids, pieces, "CENTRO_" + f);
            AddArray(kids, pieces, "GRIGLIA_" + f + "_L_" + gl);
            AddArray(kids, pieces, "GRIGLIA_" + f + "_R_" + gr);
            AddArray(kids, pieces, "BARRA_" + f + "_" + tb);
            AddArray(kids, pieces, "LATERALI_" + f);
            return kids;
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
