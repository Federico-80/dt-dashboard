using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
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
            new Thread(() =>
            {
                try { if (Build(pages)) lastSignature = sig; }
                catch (Exception ex) { Status = "Errore nel comporre la dashboard: " + ex.Message; FedeLog.Error("componi dashboard", ex); }
                finally { working = false; }
            }) { IsBackground = true, Name = "DT dash builder" }.Start();
        }

        /// <summary>Forza la ricomposizione al prossimo controllo (es. dopo un aggiornamento della dashboard).</summary>
        public void Invalidate() { lastSignature = ""; nextCheck = DateTime.MinValue; }

        /// <summary>Scrive la dash per queste pagine. Restituisce true se e' a posto (scritta o gia' uguale).</summary>
        public bool Build(List<PageSettings> pages)
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
            string json = tpl.Replace("\"@@PAGINE@@\"", string.Join(",", layers));

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
