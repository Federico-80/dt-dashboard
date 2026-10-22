using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FedeDashboard
{
    /// <summary>
    /// Colori dei 3 settori dal punteggio LMU (GameRawData.CurrentPlayer / Scoring.mVehicles).
    /// Fucsia = miglior settore della classe, verde = tuo miglior settore, giallo = piu' lento del tuo migliore.
    /// Durante il settore 1 mostra i settori del giro appena finito.
    /// </summary>
    public class SectorTracker
    {
        public const string Grey = "#FF4A4A4A", Purple = "#FFD500F9", Green = "#FF00E676", Yellow = "#FFFFD600";

        readonly Func<string, object> raw;
        readonly double[] myBest = { double.NaN, double.NaN, double.NaN };
        readonly string[] cur = { Grey, Grey, Grey }, last = { Grey, Grey, Grey };
        bool showLast;
        double prevCur1 = -1, prevCur2 = -1;
        int prevLaps = -1;
        bool pendingS3;
        DateTime pendingUntil;
        public bool Available;

        readonly Func<string, object> prop;
        bool genericMode;

        public SectorTracker(Func<string, object> raw, Func<string, object> prop = null) { this.raw = raw; this.prop = prop; }

        static double Secs(object v)
        {
            if (v is TimeSpan) return ((TimeSpan)v).TotalSeconds;
            return N(v);
        }

        /// <summary>
        /// Per i giochi senza punteggio LMU (iRacing, ecc.): stessi dati presi dai tempi settore di SimHub
        /// (Sector1Time / Sector2Time del giro in corso, Sector1-2LastLapTime e LastLapTime del giro finito).
        /// </summary>
        object Generic(string key)
        {
            if (prop == null) return null;
            const string G = "DataCorePlugin.GameData.";
            switch (key)
            {
                case "CurrentPlayer.mTotalLaps": return prop(G + "CompletedLaps") ?? prop(G + "CurrentLap");
                case "CurrentPlayer.mCurSector1": { double a = Secs(prop(G + "Sector1Time")); return a > 0 ? a : -1.0; }
                case "CurrentPlayer.mCurSector2":
                    {
                        double a = Secs(prop(G + "Sector1Time")), b = Secs(prop(G + "Sector2Time"));
                        return a > 0 && b > 0 ? a + b : -1.0;
                    }
                case "CurrentPlayer.mLastLapTime": return Secs(prop(G + "LastLapTime"));
                case "CurrentPlayer.mLastSector2":
                    {
                        double a = Secs(prop(G + "Sector1LastLapTime")), b = Secs(prop(G + "Sector2LastLapTime"));
                        return a > 0 && b > 0 ? a + b : double.NaN;
                    }
                default: return null;
            }
        }

        public string Color(int i)
        {
            if (i < 0 || i > 2) return Grey;
            return showLast ? last[i] : cur[i];
        }

        public void Reset()
        {
            for (int i = 0; i < 3; i++) { myBest[i] = double.NaN; cur[i] = Grey; last[i] = Grey; }
            showLast = false; prevCur1 = -1; prevCur2 = -1; prevLaps = -1; pendingS3 = false;
        }

        public void Update(DateTime now, bool running)
        {
            if (!running) { Available = false; return; }
            genericMode = raw("CurrentPlayer.mCurSector1") == null;
            Func<string, object> src = genericMode ? (Func<string, object>)Generic : raw;
            double cur1 = N(src("CurrentPlayer.mCurSector1")), cur2 = N(src("CurrentPlayer.mCurSector2"));
            double laps = N(src("CurrentPlayer.mTotalLaps"));
            if (double.IsNaN(cur1) || double.IsNaN(laps)) { Available = false; return; }
            Available = true;
            int lapsI = (int)laps;

            if (prevLaps >= 0 && lapsI < prevLaps) Reset();          // nuova sessione

            // settore 1 completato
            if (cur1 > 0 && prevCur1 <= 0)
            {
                cur[0] = Judge(0, cur1);
                cur[1] = Grey; cur[2] = Grey;
                showLast = false;
            }
            // settore 2 completato
            if (!double.IsNaN(cur2) && cur2 > 0 && prevCur2 <= 0 && cur1 > 0)
                cur[1] = Judge(1, cur2 - cur1);

            // giro completato -> settore 3 (il tempo del giro puo' arrivare un attimo dopo)
            if (prevLaps >= 0 && lapsI == prevLaps + 1) { pendingS3 = true; pendingUntil = now.AddSeconds(3); }
            if (pendingS3)
            {
                double lastLap = N(src("CurrentPlayer.mLastLapTime")), last2 = N(src("CurrentPlayer.mLastSector2"));
                if (lastLap > 0 && last2 > 0 && lastLap > last2)
                {
                    cur[2] = Judge(2, lastLap - last2);
                    Array.Copy(cur, last, 3);
                    showLast = false;   // a fine giro le 3 barre si azzerano (grigie)
                    for (int i = 0; i < 3; i++) cur[i] = Grey;
                    pendingS3 = false;
                }
                else if (now > pendingUntil)
                {
                    // giro non valido / senza tempo: mostra quel che c'e' e riparti
                    Array.Copy(cur, last, 3);
                    showLast = false;   // a fine giro le 3 barre si azzerano (grigie)
                    for (int i = 0; i < 3; i++) cur[i] = Grey;
                    pendingS3 = false;
                }
            }
            prevCur1 = cur1; prevCur2 = double.IsNaN(cur2) ? -1 : cur2; prevLaps = lapsI;
        }

        string Judge(int i, double t)
        {
            if (!(t > 0) || t > 3600) return Grey;
            double classBest = genericMode ? double.NaN : ClassBest(i);
            bool personal = double.IsNaN(myBest[i]) || t <= myBest[i] + 0.0005;
            if (personal) myBest[i] = double.IsNaN(myBest[i]) ? t : Math.Min(myBest[i], t);
            // senza i settori degli altri (iRacing) il fucsia non si puo' calcolare: solo verde / giallo
            if (genericMode) return personal ? Green : Yellow;
            if (personal && (double.IsNaN(classBest) || t <= classBest + 0.0005)) return Purple;
            return personal ? Green : Yellow;
        }

        /// <summary>Miglior settore i delle altre auto della mia classe (NaN se non disponibile).</summary>
        double ClassBest(int i)
        {
            double best = double.NaN;
            try
            {
                string myClass = Str(raw("CurrentPlayer.mVehicleClass"));
                var list = raw("Scoring.mVehicles") as IEnumerable;
                if (list == null) return best;
                foreach (var v in list)
                {
                    if (v == null) continue;
                    if (N(Get(v, "mIsPlayer")) > 0) continue;
                    if (myClass.Length > 0 && Str(Get(v, "mVehicleClass")) != myClass) continue;
                    double b1 = N(Get(v, "mBestSector1")), b2 = N(Get(v, "mBestSector2")), bl = N(Get(v, "mBestLapTime"));
                    double t = i == 0 ? b1 : (i == 1 ? (b2 > 0 && b1 > 0 ? b2 - b1 : double.NaN) : (bl > 0 && b2 > 0 ? bl - b2 : double.NaN));
                    if (t > 0 && (double.IsNaN(best) || t < best)) best = t;
                }
            }
            catch { }
            return best;
        }

        internal static object Get(object o, string name)
        {
            var t = o.GetType();
            var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p != null) return p.GetValue(o, null);
            var f = t.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            return f != null ? f.GetValue(o) : null;
        }

        static double N(object v)
        {
            if (v == null) return double.NaN;
            if (v is bool) return (bool)v ? 1 : 0;
            try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { return double.NaN; }
        }

        internal static string Str(object v)
        {
            if (v == null) return "";
            var b = v as byte[];
            if (b != null)
            {
                int n = Array.IndexOf(b, (byte)0);
                return Encoding.UTF8.GetString(b, 0, n < 0 ? b.Length : n).Trim();
            }
            return v.ToString().Trim();
        }
    }
}
