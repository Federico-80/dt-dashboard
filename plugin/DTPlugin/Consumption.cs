using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace FedeDashboard
{
    /// <summary>
    /// Consumo per giro aggiornato in tempo reale (carburante o energia virtuale).
    /// Durante il giro salva quanto hai consumato ogni mezzo percento di pista; nel giro dopo confronta,
    /// nello stesso punto, con l'ultimo giro valido: stima = consumo dell'ultimo giro valido + quanto stai
    /// consumando in piu' (o in meno) fin qui. Cosi' la stima cambia gia' a meta' giro se risparmi o spingi.
    /// L'ultimo giro valido viene salvato per pista + auto: alla sessione dopo la stima c'e' gia' dal primo giro.
    /// </summary>
    public class ConsumptionTracker
    {
        const double Step = 0.005;   // un campione ogni 0.5% di giro

        readonly string kind;        // "carb" / "energia" (nome del file)
        string combo = "";
        readonly List<double[]> cur = new List<double[]>();   // (posizione 0-1, consumato fin li')
        double[][] last;             // ultimo giro valido
        double lastUsed = double.NaN;
        double usedNow, prevAmount = double.NaN, lastPos = -1;
        bool recording, wrapped;

        public ConsumptionTracker(string kind) { this.kind = kind; }

        /// <summary>Consumo dell'ultimo giro valido (NaN se non c'e').</summary>
        public double LastValid { get { return lastUsed; } }

        /// <summary>Consumo stimato per questo giro (ultimo valido + differenza nello stesso punto).</summary>
        public double Estimate
        {
            get
            {
                if (double.IsNaN(lastUsed)) return double.NaN;
                if (!recording || last == null || last.Length < 3 || lastPos <= 0.02) return lastUsed;
                double atSame = Interp(last, lastPos);
                if (double.IsNaN(atSame)) return lastUsed;
                double est = lastUsed + (usedNow - atSame);
                return est > lastUsed * 0.5 ? est : lastUsed;
            }
        }

        /// <summary>Quanto stai consumando in piu' (+) o in meno (-) rispetto all'ultimo giro valido, fin qui.</summary>
        public double Delta
        {
            get
            {
                if (!recording || last == null || last.Length < 3 || lastPos <= 0.02) return double.NaN;
                double atSame = Interp(last, lastPos);
                return double.IsNaN(atSame) ? double.NaN : usedNow - atSame;
            }
        }

        public void SetCombo(string c)
        {
            c = Clean(c);
            if (c == combo) return;
            combo = c;
            last = null; lastUsed = double.NaN;
            Load();
        }

        /// <summary>Da chiamare a ogni aggiornamento con la quantita' a bordo e la posizione in pista (0-1).</summary>
        public void Sample(double amount, double pos, bool moving)
        {
            if (double.IsNaN(amount) || double.IsNaN(pos)) return;
            if (!double.IsNaN(prevAmount))
            {
                double diff = prevAmount - amount;
                if (diff > 0 && diff < 5) usedNow += diff;         // rigenerazione / rifornimento: non contano
                else if (diff < 0 && !moving) { }                  // rifornimento ai box
            }
            prevAmount = amount;
            if (!recording) { lastPos = pos; return; }
            if (wrapped) return;                                   // traguardo passato: aspetta LapDone
            if (pos + 0.5 < lastPos) { wrapped = true; lastPos = pos; return; }
            if (pos > lastPos + 0.2) { recording = false; cur.Clear(); lastPos = pos; return; }   // salto (garage, teletrasporto)
            if (cur.Count == 0 || pos - cur[cur.Count - 1][0] >= Step) cur.Add(new[] { pos, usedNow });
            lastPos = pos;
        }

        /// <summary>Traguardo: valid = giro pulito (niente box, consumo plausibile). Ricomincia a registrare.</summary>
        public void LapDone(bool valid, double used)
        {
            if (valid && recording && cur.Count >= 20 && used > 0)
            {
                cur.Add(new[] { 1.0, used });
                last = cur.ToArray();
                lastUsed = used;
                Save();
            }
            else if (valid && used > 0 && double.IsNaN(lastUsed)) lastUsed = used;
            cur.Clear();
            usedNow = 0;
            lastPos = 0;
            wrapped = false;
            recording = true;
            cur.Add(new[] { 0.0, 0.0 });
        }

        /// <summary>Uscita dai box, garage, nuova sessione: il giro in corso non si confronta.</summary>
        public void Interrupt() { recording = false; wrapped = false; cur.Clear(); usedNow = 0; }

        static double Interp(double[][] a, double pos)
        {
            if (pos <= a[0][0]) return a[0][1];
            for (int i = 1; i < a.Length; i++)
            {
                if (a[i][0] >= pos)
                {
                    double x0 = a[i - 1][0], x1 = a[i][0];
                    if (x1 - x0 < 1e-9) return a[i][1];
                    return a[i - 1][1] + (a[i][1] - a[i - 1][1]) * (pos - x0) / (x1 - x0);
                }
            }
            return a[a.Length - 1][1];
        }

        static string Clean(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var sb = new StringBuilder();
            foreach (char ch in s) sb.Append(char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '_');
            return sb.ToString();
        }

        string FilePath()
        {
            if (combo.Length == 0) return null;
            string dir = Path.Combine(FedeLog.Folder, "DTConsumi");
            return Path.Combine(dir, combo + "." + kind + ".txt");
        }

        void Save()
        {
            try
            {
                string f = FilePath();
                if (f == null || last == null) return;
                Directory.CreateDirectory(Path.GetDirectoryName(f));
                var sb = new StringBuilder();
                sb.Append(lastUsed.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
                foreach (var p in last) sb.Append(p[0].ToString("0.####", CultureInfo.InvariantCulture)).Append(':').Append(p[1].ToString("0.#####", CultureInfo.InvariantCulture)).Append(' ');
                File.WriteAllText(f, sb.ToString());
            }
            catch (Exception ex) { FedeLog.Error("salvataggio consumi", ex); }
        }

        void Load()
        {
            try
            {
                string f = FilePath();
                if (f == null || !File.Exists(f)) return;
                var lines = File.ReadAllText(f).Split('\n');
                double u;
                if (!double.TryParse(lines[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out u) || !(u > 0)) return;
                var pts = new List<double[]>();
                if (lines.Length > 1)
                    foreach (var tok in lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var kv = tok.Split(':');
                        double a, b;
                        if (kv.Length == 2 && double.TryParse(kv[0], NumberStyles.Float, CultureInfo.InvariantCulture, out a)
                            && double.TryParse(kv[1], NumberStyles.Float, CultureInfo.InvariantCulture, out b)) pts.Add(new[] { a, b });
                    }
                lastUsed = u;
                last = pts.Count >= 3 ? pts.ToArray() : null;
            }
            catch (Exception ex) { FedeLog.Error("lettura consumi", ex); }
        }
    }
}
