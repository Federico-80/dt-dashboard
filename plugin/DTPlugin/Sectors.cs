using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FedeDashboard
{
    /// <summary>
    /// Colori dei 3 settori dal punteggio LMU (GameRawData.CurrentPlayer / Scoring.mVehicles).
    /// Viola = migliore della tua classe (come il gioco), verde = tuo miglior settore, giallo = piu' lento del tuo migliore.
    /// I migliori settori degli altri sono raccolti giro per giro (non solo quelli del loro giro migliore).
    /// Durante il settore 1 mostra i settori del giro appena finito.
    /// </summary>
    public class SectorTracker
    {
        public const string Grey = "#FF4A4A4A", Purple = "#FFD500F9", Green = "#FF00E676", Yellow = "#FFFFD600";

        readonly Func<string, object> raw;
        readonly double[] myBest = { double.NaN, double.NaN, double.NaN };
        readonly string[] cur = { Grey, Grey, Grey }, last = { Grey, Grey, Grey };
        readonly bool[] curStrip = new bool[3], lastStrip = new bool[3];
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

        /// <summary>Striscia viola sotto la barra: tuo migliore ed e' il migliore della tua classe.</summary>
        public bool Strip(int i)
        {
            if (i < 0 || i > 2) return false;
            return showLast ? lastStrip[i] : curStrip[i];
        }

        // migliori settori degli altri piloti (per nome): classe + 3 settori
        readonly System.Collections.Generic.Dictionary<string, object[]> others = new System.Collections.Generic.Dictionary<string, object[]>();
        DateTime nextOthers = DateTime.MinValue;

        static void Better(double[] b, int i, double t) { if (t > 0 && t < 3600 && (double.IsNaN(b[i]) || t < b[i])) b[i] = t; }

        void TrackOthers(DateTime now)
        {
            if (now < nextOthers) return;
            nextOthers = now.AddMilliseconds(400);
            try
            {
                var list = raw("Scoring.mVehicles") as IEnumerable;
                if (list == null) return;
                foreach (var v in list)
                {
                    if (v == null || N(Get(v, "mIsPlayer")) > 0) continue;
                    string name = Str(Get(v, "mDriverName"));
                    if (name.Length == 0) continue;
                    object[] e;
                    if (!others.TryGetValue(name, out e)) { e = new object[] { "", new[] { double.NaN, double.NaN, double.NaN } }; others[name] = e; }
                    e[0] = Str(Get(v, "mVehicleClass"));
                    var b = (double[])e[1];
                    double b1 = N(Get(v, "mBestSector1")), b2 = N(Get(v, "mBestSector2")), bl = N(Get(v, "mBestLapTime"));
                    double l1 = N(Get(v, "mLastSector1")), l2 = N(Get(v, "mLastSector2")), ll = N(Get(v, "mLastLapTime"));
                    Better(b, 0, b1); Better(b, 0, l1);
                    if (b1 > 0 && b2 > b1) Better(b, 1, b2 - b1);
                    if (l1 > 0 && l2 > l1) Better(b, 1, l2 - l1);
                    if (b2 > 0 && bl > b2) Better(b, 2, bl - b2);
                    if (l2 > 0 && ll > l2) Better(b, 2, ll - l2);
                }
            }
            catch { }
        }

        public void Reset()
        {
            others.Clear();
            for (int i = 0; i < 3; i++) { myBest[i] = double.NaN; cur[i] = Grey; last[i] = Grey; curStrip[i] = false; lastStrip[i] = false; }
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
            if (!genericMode) TrackOthers(now);

            // settore 1 completato
            if (cur1 > 0 && prevCur1 <= 0)
            {
                cur[0] = Judge(0, cur1, out curStrip[0]);
                cur[1] = Grey; cur[2] = Grey; curStrip[1] = false; curStrip[2] = false;
                showLast = false;
            }
            // settore 2 completato
            if (!double.IsNaN(cur2) && cur2 > 0 && prevCur2 <= 0 && cur1 > 0)
                cur[1] = Judge(1, cur2 - cur1, out curStrip[1]);

            // giro completato -> settore 3 (il tempo del giro puo' arrivare un attimo dopo)
            if (prevLaps >= 0 && lapsI == prevLaps + 1) { pendingS3 = true; pendingUntil = now.AddSeconds(3); }
            if (pendingS3)
            {
                double lastLap = N(src("CurrentPlayer.mLastLapTime")), last2 = N(src("CurrentPlayer.mLastSector2"));
                if (lastLap > 0 && last2 > 0 && lastLap > last2)
                {
                    cur[2] = Judge(2, lastLap - last2, out curStrip[2]);
                    Array.Copy(cur, last, 3); Array.Copy(curStrip, lastStrip, 3);
                    showLast = false;   // a fine giro le 3 barre si azzerano (grigie)
                    for (int i = 0; i < 3; i++) { cur[i] = Grey; curStrip[i] = false; }
                    pendingS3 = false;
                }
                else if (now > pendingUntil)
                {
                    // giro non valido / senza tempo: mostra quel che c'e' e riparti
                    Array.Copy(cur, last, 3);
                    showLast = false;   // a fine giro le 3 barre si azzerano (grigie)
                    for (int i = 0; i < 3; i++) { cur[i] = Grey; curStrip[i] = false; }
                    pendingS3 = false;
                }
            }
            prevCur1 = cur1; prevCur2 = double.IsNaN(cur2) ? -1 : cur2; prevLaps = lapsI;
        }

        string Judge(int i, double t, out bool strip)
        {
            strip = false;
            if (!(t > 0) || t > 3600) return Grey;
            bool personal = double.IsNaN(myBest[i]) || t <= myBest[i] + 0.0005;
            if (personal) myBest[i] = double.IsNaN(myBest[i]) ? t : Math.Min(myBest[i], t);
            if (!personal) return Yellow;
            // senza i settori degli altri (iRacing) il viola non si puo' calcolare: solo verde / giallo
            if (genericMode) return Green;
            // viola = migliore della tua classe (anche se nessun altro della classe ha ancora un tempo)
            double cls = ClassBest(i);
            return double.IsNaN(cls) || t <= cls + 0.0005 ? Purple : Green;
        }

        double ClassBest(int i)
        {
            nextOthers = DateTime.MinValue; TrackOthers(DateTime.Now);   // dati freschi al momento del confronto
            string myClass = Str(raw("CurrentPlayer.mVehicleClass"));
            double best = double.NaN;
            foreach (var e in others.Values)
            {
                if (myClass.Length > 0 && (string)e[0] != myClass) continue;
                double t = ((double[])e[1])[i];
                if (t > 0 && (double.IsNaN(best) || t < best)) best = t;
            }
            return best;
        }
        double OthersBest(int i, bool sameClass)
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
                    if (sameClass && myClass.Length > 0 && Str(Get(v, "mVehicleClass")) != myClass) continue;
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
            if (b == null)
            {
                var sb = v as sbyte[];
                if (sb != null) { b = new byte[sb.Length]; Buffer.BlockCopy(sb, 0, b, 0, sb.Length); }
            }
            if (b != null)
            {
                int n = Array.IndexOf(b, (byte)0);
                return Encoding.UTF8.GetString(b, 0, n < 0 ? b.Length : n).Trim();
            }
            var ca = v as char[];
            if (ca != null) { int n = Array.IndexOf(ca, '\0'); return new string(ca, 0, n < 0 ? ca.Length : n).Trim(); }
            if (v is Array) return "";
            return v.ToString().Trim();
        }
    }
}
