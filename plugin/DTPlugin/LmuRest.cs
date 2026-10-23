using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace FedeDashboard
{
    /// <summary>JSON minimo: oggetti -> Dictionary, array -> List, numeri -> double.</summary>
    public static class MiniJson
    {
        public static object Parse(string s)
        {
            int i = 0;
            var v = Value(s, ref i);
            return v;
        }

        static void Ws(string s, ref int i) { while (i < s.Length && char.IsWhiteSpace(s[i])) i++; }

        static object Value(string s, ref int i)
        {
            Ws(s, ref i);
            if (i >= s.Length) return null;
            char c = s[i];
            if (c == '{')
            {
                var d = new Dictionary<string, object>();
                i++; Ws(s, ref i);
                if (i < s.Length && s[i] == '}') { i++; return d; }
                while (i < s.Length)
                {
                    Ws(s, ref i);
                    string k = Str(s, ref i);
                    Ws(s, ref i); if (i < s.Length && s[i] == ':') i++;
                    d[k] = Value(s, ref i);
                    Ws(s, ref i);
                    if (i < s.Length && s[i] == ',') { i++; continue; }
                    if (i < s.Length && s[i] == '}') { i++; break; }
                    break;
                }
                return d;
            }
            if (c == '[')
            {
                var l = new List<object>();
                i++; Ws(s, ref i);
                if (i < s.Length && s[i] == ']') { i++; return l; }
                while (i < s.Length)
                {
                    l.Add(Value(s, ref i));
                    Ws(s, ref i);
                    if (i < s.Length && s[i] == ',') { i++; continue; }
                    if (i < s.Length && s[i] == ']') { i++; break; }
                    break;
                }
                return l;
            }
            if (c == '"') return Str(s, ref i);
            if (s.Length - i >= 4 && string.CompareOrdinal(s, i, "true", 0, 4) == 0) { i += 4; return true; }
            if (s.Length - i >= 5 && string.CompareOrdinal(s, i, "false", 0, 5) == 0) { i += 5; return false; }
            if (s.Length - i >= 4 && string.CompareOrdinal(s, i, "null", 0, 4) == 0) { i += 4; return null; }
            int st = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            double d0;
            if (double.TryParse(s.Substring(st, i - st), NumberStyles.Float, CultureInfo.InvariantCulture, out d0)) return d0;
            i = Math.Max(i, st + 1);
            return null;
        }

        static string Str(string s, ref int i)
        {
            if (i >= s.Length || s[i] != '"') return "";
            i++;
            var sb = new StringBuilder();
            while (i < s.Length && s[i] != '"')
            {
                char c = s[i++];
                if (c == '\\' && i < s.Length)
                {
                    char e = s[i++];
                    switch (e)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'u':
                            if (i + 4 <= s.Length) { sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16)); i += 4; }
                            break;
                        default: sb.Append(e); break;
                    }
                }
                else sb.Append(c);
            }
            i++;
            return sb.ToString();
        }

        /// <summary>Percorso tipo "a.b.0.c" dentro l'albero.</summary>
        public static object Get(object o, string path)
        {
            foreach (var part in path.Split('.'))
            {
                if (o == null) return null;
                var d = o as Dictionary<string, object>;
                if (d != null) { object v; o = d.TryGetValue(part, out v) ? v : null; continue; }
                var l = o as List<object>;
                int idx;
                if (l != null && int.TryParse(part, out idx)) { o = idx >= 0 && idx < l.Count ? l[idx] : null; continue; }
                return null;
            }
            return o;
        }

        public static double Num(object o, string path)
        {
            var v = Get(o, path);
            if (v is double) return (double)v;
            if (v is bool) return (bool)v ? 1 : 0;
            double d;
            if (v is string && double.TryParse((string)v, NumberStyles.Float, CultureInfo.InvariantCulture, out d)) return d;
            return double.NaN;
        }

        public static bool Bool(object o, string path)
        {
            var v = Get(o, path);
            if (v is bool) return (bool)v;
            if (v is double) return (double)v != 0;
            return false;
        }
    }

    /// <summary>
    /// Dati dal server interno di LMU (http://localhost:6397), gli stessi dei menu del gioco:
    /// mescole delle 4 gomme, consumo previsto di energia e carburante per giro, energia e carburante a bordo.
    /// Interroga solo con l'auto caricata, non nei replay e non nella schermata monitor (come TK LMU Plugin).
    /// </summary>
    public class LmuRest
    {
        const string Base = "http://localhost:6397";
        readonly Func<bool> active;
        volatile bool stop;
        Thread thread;
        int tick;
        DateTime retryAt = DateTime.MinValue;

        public volatile bool UiSafe;
        public readonly string[] Compounds = { "", "", "", "" };
        public double VeFractionPerLap = double.NaN, FuelPerLap = double.NaN;
        public double VeNow = double.NaN, VeMax = double.NaN, FuelNow = double.NaN;
        public DateTime UsageTime = DateTime.MinValue, TankTime = DateTime.MinValue, CompoundTime = DateTime.MinValue;
        public volatile string Status = "";
        bool loggedCompounds;

        public LmuRest(Func<bool> active) { this.active = active; }

        public void Start()
        {
            thread = new Thread(Loop) { IsBackground = true, Name = "DT LMU REST" };
            thread.Start();
        }

        public void Stop() { stop = true; }

        static bool Fresh(DateTime t, int seconds) { return (DateTime.Now - t).TotalSeconds < seconds; }

        /// <summary>Giri di energia virtuale secondo LMU (energia a bordo / consumo previsto). NaN se non disponibile.</summary>
        public double VeLaps
        {
            get
            {
                if (!Fresh(UsageTime, 20) || !Fresh(TankTime, 20)) return double.NaN;
                if (!(VeMax > 0) || !(VeFractionPerLap > 0) || double.IsNaN(VeNow)) return double.NaN;
                return VeNow / VeMax / VeFractionPerLap;
            }
        }

        public double FuelLaps
        {
            get
            {
                if (!Fresh(UsageTime, 20) || !Fresh(TankTime, 20)) return double.NaN;
                if (!(FuelPerLap > 0) || double.IsNaN(FuelNow)) return double.NaN;
                return FuelNow / FuelPerLap;
            }
        }

        /// <summary>Energia per giro in % (consumo previsto da LMU).</summary>
        public double VePerLapPct { get { return Fresh(UsageTime, 20) && VeFractionPerLap > 0 ? VeFractionPerLap * 100.0 : double.NaN; } }
        public double FuelPerLapFresh { get { return Fresh(UsageTime, 20) && FuelPerLap > 0 ? FuelPerLap : double.NaN; } }

        public string Compound(int w)
        {
            if (w < 0 || w > 3 || !Fresh(CompoundTime, 30)) return "";
            return Compounds[w] ?? "";
        }

        void Loop()
        {
            while (!stop)
            {
                try
                {
                    Thread.Sleep(500);
                    if (!active() || DateTime.Now < retryAt) { UiSafe = false; continue; }
                    tick++;
                    if (tick % 6 != 0) continue;          // ogni 3 secondi
                    PollAll();
                }
                catch (Exception ex) { Status = "errore: " + ex.Message; }
            }
        }

        object GetJson(string path)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(Base + path);
                req.Method = "GET";
                req.Timeout = 3000;
                req.ReadWriteTimeout = 3000;
                req.Accept = "application/json";
                req.Proxy = null;
                req.KeepAlive = false;
                using (var resp = (HttpWebResponse)req.GetResponse())
                {
                    if (resp.StatusCode != HttpStatusCode.OK) return null;
                    using (var sr = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string txt = sr.ReadToEnd();
                        if (string.IsNullOrEmpty(txt)) return null;
                        return MiniJson.Parse(txt);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ConnectFailure || ex.Status == WebExceptionStatus.Timeout)
                {
                    retryAt = DateTime.Now.AddSeconds(10);   // LMU non risponde: riprovo tra poco
                    Status = "LMU non raggiungibile (" + ex.Status + ")";
                }
                return null;
            }
            catch { return null; }
        }

        void PollAll()
        {
            var gs = GetJson("/rest/sessions/GetGameState");
            if (gs == null) { UiSafe = false; return; }
            UiSafe = MiniJson.Bool(gs, "playerVehicleLoaded") && !MiniJson.Bool(gs, "isReplayActive") && !MiniJson.Bool(gs, "inMonitor");
            if (!UiSafe) { Status = "in attesa (menu / replay / monitor)"; return; }

            if (tick % 12 == 0)   // ogni 6 secondi, come TK
            {
                var tm = GetJson("/rest/garage/UIScreen/TireManagement");
                if (tm != null)
                {
                    var comps = MiniJson.Get(tm, "optimalCompoundConditions.compounds") as List<object>;
                    var wheels = MiniJson.Get(tm, "wheelInfo.wheelLocs") as List<object>;
                    if (comps != null && wheels != null && wheels.Count >= 4)
                    {
                        for (int w = 0; w < 4; w++)
                        {
                            double idx = MiniJson.Num(wheels[w], "compound");
                            string name = "";
                            if (!double.IsNaN(idx) && idx >= 0 && idx < comps.Count)
                            {
                                var t = MiniJson.Get(comps[(int)idx], "type");
                                name = t == null ? "" : Convert.ToString(t, CultureInfo.InvariantCulture);
                            }
                            Compounds[w] = name;
                        }
                        CompoundTime = DateTime.Now;
                        if (!loggedCompounds) { loggedCompounds = true; FedeLog.Write("LMU mescole: " + string.Join(", ", Compounds)); }
                    }
                    double ve = MiniJson.Num(tm, "expectedUsage.virtualEnergyFractionPerLap");
                    double fu = MiniJson.Num(tm, "expectedUsage.fuelConsumption");
                    if (ve > 0) VeFractionPerLap = ve;
                    if (fu > 0) FuelPerLap = fu;
                    if (ve > 0 || fu > 0) UsageTime = DateTime.Now;
                }
            }

            var rr = GetJson("/rest/garage/UIScreen/RepairAndRefuel");
            if (rr != null)
            {
                double cv = MiniJson.Num(rr, "fuelInfo.currentVirtualEnergy"), mv = MiniJson.Num(rr, "fuelInfo.maxVirtualEnergy");
                double cf = MiniJson.Num(rr, "fuelInfo.currentFuel");
                if (!double.IsNaN(cv)) VeNow = cv;
                if (!double.IsNaN(mv)) VeMax = mv;
                if (!double.IsNaN(cf)) FuelNow = cf;
                if (!double.IsNaN(cv) || !double.IsNaN(cf)) TankTime = DateTime.Now;
            }
            Status = "ok";
        }
    }
}
