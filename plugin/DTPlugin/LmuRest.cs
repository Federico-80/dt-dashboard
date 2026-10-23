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
        bool loggedCompounds, loggedOnce;

        // sosta stimata (secondi), danni, freni, gomme nuove, batteria, track limits, meteo
        public double RepDamage = double.NaN, RepBrakes = double.NaN, RepTyres = double.NaN, RepFuel = double.NaN, RepTotal = double.NaN;
        public DateTime RepTime = DateTime.MinValue;
        public readonly double[] Susp = { double.NaN, double.NaN, double.NaN, double.NaN };
        public readonly double[] Brakes = { double.NaN, double.NaN, double.NaN, double.NaN };
        public double Aero = double.NaN, VehicleDmg = double.NaN, NewTyres = double.NaN;
        public DateTime WearTime = DateTime.MinValue, CondTime = DateTime.MinValue;
        public double BattNow = double.NaN, BattMax = double.NaN;
        public double CutsAllowed = double.NaN;
        public double AirC = double.NaN, TrackC = double.NaN, RainIntensity = double.NaN, Cloud = double.NaN, Wetness = double.NaN;
        public double TimeCompletion = double.NaN, RaceCompletion = double.NaN;
        public DateTime WeatherTime = DateTime.MinValue, InfoTime = DateTime.MinValue;
        /// <summary>Previsioni: 5 punti (inizio, 25%, 50%, 75%, fine): id cielo 0..10, testo, temperatura, pioggia %.</summary>
        public readonly int[] FcSky = { -1, -1, -1, -1, -1 };
        public readonly string[] FcSkyText = { "", "", "", "", "" };
        public readonly double[] FcTemp = { double.NaN, double.NaN, double.NaN, double.NaN, double.NaN };
        public readonly double[] FcRain = { double.NaN, double.NaN, double.NaN, double.NaN, double.NaN };
        public DateTime ForecastTime = DateTime.MinValue;
        public Func<string> SessionType;   // dal plugin: "PRACTICE" / "QUALIFY" / "RACE"
        bool cutsRead, probed;
        int strategyLogs;
        volatile string lastText;
        public Func<string> PlayerName;

        public bool FreshRep { get { return Fresh(RepTime, 20); } }
        public bool FreshWear { get { return Fresh(WearTime, 30); } }
        public bool FreshCond { get { return Fresh(CondTime, 30); } }
        public bool FreshForecast { get { return Fresh(ForecastTime, 60); } }
        public bool FreshInfo { get { return Fresh(InfoTime, 30); } }
        public double BatteryPct { get { return Fresh(TankTime, 20) && BattMax > 0 && !double.IsNaN(BattNow) ? BattNow / BattMax * 100.0 : double.NaN; } }

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
                        lastText = txt;
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

        static readonly string[] Nodes = { "START", "NODE_25", "NODE_50", "NODE_75", "FINISH" };

        void PollWeather()
        {
            var wj = GetJson("/rest/sessions/weather") as Dictionary<string, object>;
            if (wj == null) return;
            string st = SessionType != null ? (SessionType() ?? "") : "";
            st = st.ToUpperInvariant();
            string key = st.Contains("PRACTICE") || st.Contains("TRAINING") ? "PRACTICE" : (st.Contains("QUALIFY") ? "QUALIFY" : "RACE");
            object ses;
            if (!wj.TryGetValue(key, out ses) || !(ses is Dictionary<string, object>))
            {
                ses = null;
                foreach (var kv in wj) if (kv.Value is Dictionary<string, object>) { ses = kv.Value; break; }
            }
            if (ses == null) return;
            for (int i = 0; i < 5; i++)
            {
                var node = MiniJson.Get(ses, Nodes[i]);
                if (node == null) continue;
                double sky = MiniJson.Num(node, "WNV_SKY.currentValue");
                FcSky[i] = double.IsNaN(sky) ? -1 : (int)sky;
                var stv = MiniJson.Get(node, "WNV_SKY.stringValue");
                FcSkyText[i] = stv == null ? "" : Convert.ToString(stv, CultureInfo.InvariantCulture);
                FcTemp[i] = MiniJson.Num(node, "WNV_TEMPERATURE.currentValue");
                FcRain[i] = MiniJson.Num(node, "WNV_RAIN_CHANCE.currentValue");
            }
            ForecastTime = DateTime.Now;
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
                    double nt = MiniJson.Num(tm, "tireInventory.newTires");
                    if (!double.IsNaN(nt)) NewTyres = nt;
                    var sus = MiniJson.Get(tm, "wearables.suspension") as List<object>;
                    if (sus != null && sus.Count >= 4) for (int w = 0; w < 4; w++) { var o = sus[w]; Susp[w] = o is double ? (double)o : double.NaN; }
                    double ae = MiniJson.Num(tm, "wearables.body.aero");
                    if (!double.IsNaN(ae)) Aero = ae;
                    WearTime = DateTime.Now;
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
                double cb = MiniJson.Num(rr, "fuelInfo.currentBattery"), mb = MiniJson.Num(rr, "fuelInfo.maxBattery");
                if (!double.IsNaN(cb)) BattNow = cb;
                if (!double.IsNaN(mb)) BattMax = mb;
                if (!double.IsNaN(cv) || !double.IsNaN(cf)) TankTime = DateTime.Now;
                double ak = MiniJson.Num(rr, "currentWeather.ambientTempKelvin"), tk = MiniJson.Num(rr, "currentWeather.trackTempKelvin");
                if (ak > 0) AirC = ak - 273.15;
                if (tk > 0) TrackC = tk - 273.15;
                RainIntensity = MiniJson.Num(rr, "currentWeather.rainIntensity");
                Cloud = MiniJson.Num(rr, "currentWeather.cloudCoverage");
                if (ak > 0) WeatherTime = DateTime.Now;
            }

            int t3 = (tick / 6) % 3;
            if (t3 == 0)
            {
                var vc = GetJson("/rest/garage/getVehicleCondition");
                if (vc != null)
                {
                    double vd = MiniJson.Num(vc, "vehicleDamage");
                    if (!double.IsNaN(vd)) VehicleDmg = vd;
                    var bc = MiniJson.Get(vc, "brakeCondition") as List<object>;
                    if (bc != null && bc.Count >= 4) for (int w = 0; w < 4; w++) { var o = bc[w]; Brakes[w] = o is double ? (double)o : double.NaN; }
                    CondTime = DateTime.Now;
                }
            }
            else if (t3 == 1)
            {
                var pe = GetJson("/rest/strategy/pitstop-estimate");
                if (pe != null)
                {
                    RepDamage = MiniJson.Num(pe, "damage"); RepBrakes = MiniJson.Num(pe, "brakes"); RepTyres = MiniJson.Num(pe, "tires");
                    RepFuel = MiniJson.Num(pe, "fuel"); RepTotal = MiniJson.Num(pe, "total");
                    RepTime = DateTime.Now;
                }
            }
            else
            {
                PollWeather();
            }

            var si = GetJson("/rest/watch/sessionInfo");
            if (si != null)
            {
                RaceCompletion = MiniJson.Num(si, "raceCompletion");
                TimeCompletion = MiniJson.Num(si, "timeCompletion");
                Wetness = MiniJson.Num(si, "averagePathWetness");
                InfoTime = DateTime.Now;
            }
            if (!probed)
            {
                probed = true;
                // elenco dei servizi di LMU (per trovare la strategia del menu): scritto una volta nel log
                foreach (var doc in new[] { "/swagger/v1/swagger.json", "/swagger.json", "/openapi.json", "/rest/swagger.json" })
                {
                    var sw = GetJson(doc) as Dictionary<string, object>;
                    var paths = sw == null ? null : MiniJson.Get(sw, "paths") as Dictionary<string, object>;
                    if (paths == null) continue;
                    var keep = new List<string>();
                    foreach (var k in paths.Keys)
                        if (k.IndexOf("strateg", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("fuel", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            k.IndexOf("pit", StringComparison.OrdinalIgnoreCase) >= 0 || k.IndexOf("energy", StringComparison.OrdinalIgnoreCase) >= 0) keep.Add(k);
                    FedeLog.Write("LMU servizi (" + paths.Count + "): " + string.Join(" ", keep));
                    break;
                }
            }
            // strategia del menu di LMU: scrivo nel log com'e' fatta (alcune volte, per capirne i campi)
            if (strategyLogs < 0 && (tick / 6) % 20 == 3)   // disattivato: la strategia di LMU non ha i giri alla fine
            {
                strategyLogs++;
                GetJson("/rest/strategy/overall");
                string ov = lastText ?? "";
                FedeLog.Write("LMU strategia overall: " + (ov.Length > 1500 ? ov.Substring(0, 1500) + "…" : ov));
                GetJson("/rest/strategy/usage");
                string us = lastText ?? "";
                string me = PlayerName != null ? (PlayerName() ?? "") : "";
                int at = me.Length > 0 ? us.IndexOf("\"" + me + "\"", StringComparison.Ordinal) : -1;
                FedeLog.Write("LMU strategia usage (" + me + "): " + (at >= 0 ? us.Substring(at, Math.Min(900, us.Length - at)) : (us.Length > 600 ? us.Substring(0, 600) : us)));
            }
            if (!cutsRead)
            {
                var ss = GetJson("/rest/sessions/?");
                if (ss != null) { CutsAllowed = MiniJson.Num(ss, "SESSSET_cuts_allowed.currentValue"); cutsRead = true; }
            }
            if (!loggedOnce && tick / 6 >= 8)   // dopo una ventina di secondi, quando tutti i dati sono arrivati
            {
                loggedOnce = true;
                FedeLog.Write("LMU dati: sosta " + RepTotal + " s, gomme nuove " + NewTyres + ", batteria " + BatteryPct + "%, track limits ammessi " + CutsAllowed +
                              ", sospensioni " + string.Join("/", Susp) + ", freni " + string.Join("/", Brakes));
            }
            Status = "ok";
        }
    }
}
