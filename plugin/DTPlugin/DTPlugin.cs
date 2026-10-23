using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace FedeDashboard
{
    /// <summary>Stato a runtime di un canale dati (testo formattato + valore numerico).</summary>
    public class ChannelRuntime
    {
        public ChannelDef Def;
        public ChannelSettings Settings;
        public volatile string Text = ValueFormatter.Empty;
        public double Value;
    }

    [PluginDescription("DT Dashboard: scegli pagine, layout, schede e i valori da mostrare")]
    [PluginAuthor("DT Racing Team")]
    [PluginName("DT Plugin")]
    public class DTPlugin : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public const string VersionText = "1.83";

        public FedeSettings Settings;
        public readonly Updater Updater = new Updater();
        public readonly DashBuilder DashBuilder = new DashBuilder();
        public Access Access;
        LmuRest lmuRest;
        volatile bool lmuActive;
        volatile string sessionTypeCache = "", playerNameCache = "";

        static readonly string[] SkyNames = { "Sereno", "Poco nuvoloso", "Nuvoloso", "Coperto", "Molto nuvoloso", "Pioviggine",
                                              "Pioggia leggera", "Coperto, pioggia", "Pioggia", "Pioggia forte", "Temporale" };
        static readonly string[] SkyShort = { "Sereno", "Poche nubi", "Nuvoloso", "Coperto", "Nuvoloso", "Pioviggine", "Pioggia", "Pioggia", "Pioggia", "Pioggia forte", "Temporale" };
        static readonly string[] FcLabels = { "INIZIO", "25%", "50%", "75%", "FINE" };

        static int SkyIcon(int id)
        {
            if (id < 0) return -1;
            if (id == 0) return 0;
            if (id <= 2) return 1;
            if (id <= 4) return 2;
            if (id >= 10) return 4;
            return 3;
        }

        /// <summary>Stato della pista: 0 = testo, 1 = colore di fondo, 2 = colore del testo.</summary>
        string TrackState(int what)
        {
            var tw = Find("TrackWet"); var rn = Find("Rain");
            double wet = tw != null && tw.Text != ValueFormatter.Empty ? tw.Value : double.NaN;
            double rain = rn != null && rn.Text != ValueFormatter.Empty ? rn.Value : double.NaN;
            if (lmuRest != null && lmuRest.FreshInfo && !double.IsNaN(lmuRest.Wetness) && double.IsNaN(wet)) wet = lmuRest.Wetness * 100;
            string t, bg, fg;
            if (double.IsNaN(wet) && double.IsNaN(rain)) { t = ""; bg = "#FF2A2A2A"; fg = "#FF9E9E9E"; }
            else if (rain > 1) { t = "PIO"; bg = "#FF0D47A1"; fg = "#FFFFFFFF"; }
            else if (wet >= 30) { t = "BAG"; bg = "#FF1E88E5"; fg = "#FFFFFFFF"; }
            else if (wet >= 5) { t = "UMI"; bg = "#FF26C6DA"; fg = "#FF111111"; }
            else { t = "ASC"; bg = "#FFE0E0E0"; fg = "#FF111111"; }
            return what == 0 ? t : (what == 1 ? bg : fg);
        }

        /// <summary>Danno sospensione (0-100%): dati condivisi di LMU, altrimenti server LMU.</summary>
        double SuspDamage(int w)
        {
            double v = Num(Raw("mSuspensionDamage0" + (w + 1)));
            if (double.IsNaN(v) && lmuRest != null && lmuRest.FreshWear) v = lmuRest.Susp[w];
            if (double.IsNaN(v)) return double.NaN;
            return Math.Max(0, Math.Min(100, v <= 1.0001 ? v * 100.0 : v));
        }

        object LmuSpecial(string src)
        {
            var r = lmuRest;
            switch (src)
            {
                case "@rip_danni": return r != null && r.FreshRep && !double.IsNaN(r.RepDamage) ? (object)r.RepDamage : null;
                case "@rip_freni": return r != null && r.FreshRep && !double.IsNaN(r.RepBrakes) ? (object)r.RepBrakes : null;
                case "@rip_gomme": return r != null && r.FreshRep && !double.IsNaN(r.RepTyres) ? (object)r.RepTyres : null;
                case "@rip_carb": return r != null && r.FreshRep && !double.IsNaN(r.RepFuel) ? (object)r.RepFuel : null;
                case "@rip_totale": return r != null && r.FreshRep && !double.IsNaN(r.RepTotal) ? (object)r.RepTotal : null;
                case "@danno_aero":
                    {
                        double v = Num(Raw("mAeroDamage"));
                        if (double.IsNaN(v) && r != null && r.FreshWear) v = r.Aero;
                        return double.IsNaN(v) ? null : (object)Math.Max(0, Math.Min(100, v <= 1.0001 ? v * 100 : v));
                    }
                case "@danno_motore":
                    {
                        double v = Num(Raw("mEngineDamage"));
                        return double.IsNaN(v) ? null : (object)Math.Max(0, Math.Min(100, v <= 1.0001 ? v * 100 : v));
                    }
                case "@gomme_nuove": return r != null && r.FreshWear && !double.IsNaN(r.NewTyres) ? (object)r.NewTyres : null;
                case "@batteria":
                    {
                        double b = r != null ? r.BatteryPct : double.NaN;
                        if (double.IsNaN(b)) { double f = Num(Raw("CurrentPlayerTelemetry.mBatteryChargeFraction")); if (!double.IsNaN(f)) b = f * 100; }
                        return double.IsNaN(b) ? null : (object)b;
                    }
                case "@track_limits":
                    {
                        double pts = Num(Raw("PlayerNativeTelemetry.mTrackLimitsSteps"));
                        double allowed = r != null ? r.CutsAllowed : double.NaN;
                        if (double.IsNaN(pts) && double.IsNaN(allowed)) return null;
                        string a = double.IsNaN(pts) ? "-" : Math.Round(pts).ToString(CultureInfo.InvariantCulture);
                        return double.IsNaN(allowed) || allowed <= 0 ? a : a + "/" + Math.Round(allowed).ToString(CultureInfo.InvariantCulture);
                    }
                case "@cielo":
                    {
                        if (r == null || !r.FreshForecast) return null;
                        int i = ForecastNow();
                        int id = r.FcSky[i];
                        if (id >= 0 && id < SkyNames.Length) return SkyNames[id];
                        return string.IsNullOrEmpty(r.FcSkyText[i]) ? null : r.FcSkyText[i];
                    }
            }
            if (src.StartsWith("@sosp_"))
            {
                int w = Array.IndexOf(Wheels, src.Substring(6));
                if (w < 0) return null;
                double v = SuspDamage(w);
                return double.IsNaN(v) ? null : (object)v;
            }
            if (src.StartsWith("@usura_freno_"))
            {
                int w = Array.IndexOf(Wheels, src.Substring(13));
                if (w < 0 || r == null || !r.FreshCond || double.IsNaN(r.Brakes[w])) return null;
                double v = r.Brakes[w];
                return Math.Max(0, Math.Min(100, v <= 1.0001 ? v * 100 : v));
            }
            return null;
        }

        /// <summary>Punto delle previsioni piu' vicino all'avanzamento della sessione (0 = inizio ... 4 = fine).</summary>
        int ForecastNow()
        {
            var r = lmuRest;
            double c = r != null && r.FreshInfo ? (!double.IsNaN(r.TimeCompletion) && r.TimeCompletion > 0 ? r.TimeCompletion : r.RaceCompletion) : double.NaN;
            if (double.IsNaN(c)) return 0;
            if (c > 1.0001) c /= 100.0;
            return Math.Max(0, Math.Min(4, (int)Math.Round(c * 4)));
        }
        public PluginManager PluginManager { get; set; }
        public string LeftMenuTitle { get { return "DT Plugin"; } }

        internal ChannelRuntime[] Runtime = new ChannelRuntime[0];

        /// <summary>Chiamato quando la pagina attiva cambia (anche da un tasto del volante).</summary>
        public event Action PageChanged;

        SimHubBridge bridge;
        DateTime toastUntil = DateTime.MinValue;
        DateTime nextGapUpdate = DateTime.MinValue;
        double gapAhead = double.NaN, gapBehind = double.NaN;
        // schermata box: auto davanti [0] e dietro [1]
        readonly string[] boxGapCard = { "-", "-" }, boxLast = { "-", "-" }, boxNameCard = { "", "" };
        readonly string[] boxName = { "", "" }, boxNum = { "", "" }, boxClass = { "", "" }, boxClassColor = { "#FFC62828", "#FFC62828" }, boxGap = { "", "" };
        volatile bool alertBox;
        DateTime pitLaneSince = DateTime.MaxValue;
        static readonly PageSettings FallbackPage = new PageSettings();
        readonly Dictionary<string, MemberInfo> memberCache = new Dictionary<string, MemberInfo>();
        bool dataErrorLogged, alertErrorLogged;
        int updates;
        string sessionKind = "";

        // ---- avvisi
        volatile bool alertActive;
        int alertVariant, alertN;
        string alertTitle = "", alertBg = "#FF000000", alertFg = "#FFFFFFFF";
        readonly string[] alertV = { "", "", "", "" }, alertL = { "", "", "", "" };
        readonly Dictionary<string, string> lastTexts = new Dictionary<string, string>();
        DateTime changeUntil = DateTime.MinValue, lapUntil = DateTime.MinValue, testUntil = DateTime.MinValue, runningSince = DateTime.MaxValue;
        string changeId = "", testKind = "";
        double lastLapNumber = -1, fuelAtLapStart = double.NaN, fuelLastLap = double.NaN;
        double veAtLapStart = double.NaN, veLastLap = double.NaN;
        readonly List<double> veLaps = new List<double>();

        // ---- classe, danni, bandiere
        int playerClassPos, classCount;
        readonly int[] dent = new int[8];
        readonly int[] prevDent = new int[8];
        readonly string[] prevWheelState = { "", "", "", "" };
        DateTime damageUntil = DateTime.MinValue, flagTransientUntil = DateTime.MinValue;
        string transientFlag = "";
        bool lastGreen, lastChequered;
        volatile bool alertDamage;
        volatile bool engineOff, overRev;
        DateTime tcActiveUntil = DateTime.MinValue, absActiveUntil = DateTime.MinValue;
        volatile int engineState, lightsState, wiperState;
        volatile string currentFlag = "";
        volatile bool boxFlagOn;
        volatile string boxFlagText = "", boxFlagBg = "#FFFFD600", boxFlagFg = "#FF000000";
        SectorTracker sectors;
        DateTime engineStoppedSince = DateTime.MaxValue;

        // ---- gomme e freni: colori
        static readonly string[] Wheels = { "FL", "FR", "RL", "RR" };
        static readonly string[] WheelNames = { "FrontLeft", "FrontRight", "RearLeft", "RearRight" };
        readonly Dictionary<string, string> colors = new Dictionary<string, string>();
        string rawPrefix;

        // ------------------------------------------------------------------ avvio / chiusura
        public void Init(PluginManager pluginManager)
        {
            FedeLog.Start();
            Updater.CleanupOld();
            try
            {
                Settings = SettingsStore.Load();
                Access = new Access(Settings.Access, SaveSettings);
                lmuRest = new LmuRest(() => lmuActive);
                lmuRest.SessionType = () => sessionTypeCache;
                lmuRest.PlayerName = () => playerNameCache;
                lmuRest.Start();
                Access.Start();
                Updater.Changed += OnUpdaterChanged;
                Updater.CheckAsync(VersionText);   // controllo aggiornamenti su GitHub in background
                // e poi ogni 30 minuti, cosi' l'avviso arriva anche con SimHub sempre aperto
                updateTimer = new System.Threading.Timer(_ => { try { Access.CheckAsync(); if (!Updater.Busy && !Updater.Installed) Updater.CheckAsync(VersionText); } catch { } },
                    null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
                try { DashBuilder.Build(new List<PageSettings>(Settings.Pages), Settings.Font); } catch (Exception ex) { FedeLog.Error("componi dashboard", ex); }
                DashBuilder.Started = true;
                object pmObj = pluginManager;
                DashBuilder.ReloadDash = () => DashReloader.Reload(pmObj);
                // cerco in SimHub un modo per ricaricare la dashboard senza riavviarlo (scritto nel log)
                new System.Threading.Thread(() => { System.Threading.Thread.Sleep(15000); DashProbe.Run(); }) { IsBackground = true, Name = "DT sonda" }.Start();
                BuildRuntime();
                bridge = new SimHubBridge(this, pluginManager);
                RegisterProperties();
                EnsureDtLogo();
                FedeLog.Write("Init completato: " + Runtime.Length + " canali, " + Settings.Pages.Count + " pagine");
            }
            catch (Exception ex)
            {
                FedeLog.Error("Init", ex);
                if (Settings == null) Settings = FedeSettings.CreateDefault();
            }
        }

        void RegisterProperties()
        {
            // Canali dati:  DTPlugin.<Id>  /  DTPlugin.<Id>_Valore  /  DTPlugin.<Id>_Nome
            foreach (var item in Runtime)
            {
                var rt = item;
                bridge.Attach(rt.Def.Id, () => rt.Text);
                bridge.Attach(rt.Def.Id + "_Valore", () => rt.Value);
                bridge.Attach(rt.Def.Id + "_Nome", () => rt.Settings.Label);
            }

            // Pagine e layout
            bridge.Attach("Pagina", () => Settings.ActivePage + 1);
            bridge.Attach("Sbloccato", () => Access != null && Access.Unlocked);
            bridge.Attach("PaginaNome", () => Page.Name);
            bridge.Attach("Pagine", () => Settings.Pages.Count);
            bridge.Attach("MostraPagina", () => DateTime.Now < toastUntil);
            bridge.Attach("Cornice", () => Page.Frame);
            bridge.Attach("Griglia_L", () => Page.GridLeft);
            bridge.Attach("Griglia_R", () => Page.GridRight);
            bridge.Attach("BarraInferiore", () => Page.Toolbar);
            bridge.Attach("Slot_SL", () => Page.SidebarLeft);
            bridge.Attach("Slot_SR", () => Page.SidebarRight);
            bridge.Attach("BarrePedali", () => Page.ShowInputs);
            bridge.Attach("Sessione", () => sessionKind);
            foreach (var w in Wheels)
            {
                foreach (var kind in new[] { "TyreT", "TyreP", "BrakeT", "TyreW" })
                {
                    string key = kind + w;
                    colors[key] = "#FF9E9E9E";
                    bridge.Attach(key + "_Colore", () => ColorOf(key));
                }
                {
                    string key = "Gomma_" + w + "_Stato";
                    colors[key] = "";
                    bridge.Attach(key, () => ColorOf(key));
                }
                {
                    string key = "Gomma_" + w + "_Mescola", keyC = key + "Colore";
                    colors[key] = ""; colors[keyC] = "#FFFFFFFF";
                    bridge.Attach(key, () => ColorOf(key));
                    bridge.Attach(keyC, () => ColorOf(keyC));
                }
                for (int j = 1; j <= 3; j++)
                {
                    string key = "Strisce_" + w + "_" + j;
                    colors[key] = "#FF3A3A3A";
                    bridge.Attach(key, () => ColorOf(key));
                }
            }
            // schermata di attesa
            // con lo stemma attivo numero e nome grandi si nascondono (finirebbero sopra il cerchio)
            bridge.Attach("Attesa_NumeroVisibile", () => Settings.Idle.NumberEnabled && !Settings.Idle.BadgeEnabled);
            bridge.Attach("Attesa_Numero", () => Settings.Idle.Number);
            bridge.Attach("Attesa_NumeroColore", () => NormalizeColor(Settings.Idle.NumberColor, "#FFFFFFFF"));
            bridge.Attach("Attesa_NomeVisibile", () => Settings.Idle.NameEnabled && !Settings.Idle.BadgeEnabled);
            bridge.Attach("Attesa_Nome", () => Settings.Idle.Name);
            bridge.Attach("Attesa_NomeColore", () => NormalizeColor(Settings.Idle.NameColor, "#FFFFFFFF"));
            bridge.Attach("Attesa_Modo", () => Settings.Idle.Mode);
            sectors = new SectorTracker(p => Raw(p), p => bridge.GetPropertyValue(p));
            for (int i = 0; i < 3; i++) { int si = i; bridge.Attach("Settore_" + (si + 1) + "_Colore", () => sectors.Color(si)); bridge.Attach("Settore_" + (si + 1) + "_Striscia", () => sectors.Strip(si)); }
            foreach (var key in new[] { "Attiva", "Soste", "Finestra", "FinestraColore", "Ratio", "Stint", "Giri", "Necessaria", "Litri", "EnergiaNec", "Questo", "Prossimo", "Chiusura" })
            {
                string kk = key;
                bridge.Attach("Strat_" + kk, () => { string v; lock (strat) return strat.TryGetValue(kk, out v) ? v : (kk == "Attiva" ? "0" : "-"); });
            }
            bridge.Attach("Bandiera_Riquadro", () => boxFlagOn);
            bridge.Attach("Bandiera_Testo", () => boxFlagText);
            bridge.Attach("Bandiera_Sfondo", () => boxFlagBg);
            bridge.Attach("Bandiera_TestoColore", () => boxFlagFg);
            bridge.Attach("Stato_Luci", () => lightsState);
            bridge.Attach("Stato_Tergi", () => wiperState);
            bridge.Attach("Stato_Motore", () => engineState);
            bridge.Attach("Stato_Bandiera", () => currentFlag);
            bridge.Attach("Stato_BandieraColore", () => FlagTileColor(currentFlag));
            bridge.Attach("MotoreSpento", () => engineOff && !Settings.Idle.EngineOffDisabled);
            bridge.Attach("MotoreSpento_Stile", () => Settings.Idle.EngineOffStyle);
            bridge.Attach("Attivo_TC", () => DateTime.Now < tcActiveUntil);
            bridge.Attach("Attivo_ABS", () => DateTime.Now < absActiveUntil);
            bridge.Attach("FuoriGiri", () => overRev);
            bridge.Attach("MotoreSpento_Testo", () => Settings.Idle.EngineOffText ?? "MOTORE SPENTO");
            bridge.Attach("MotoreSpento_Colore", () => Settings.Idle.EngineOffColor ?? "#FF2E2E2E");
            bridge.Attach("Attesa_Colore1", () => NormalizeColor(Settings.Idle.Color1, "#FF000000"));
            bridge.Attach("Attesa_Colore2", () => NormalizeColor(Settings.Idle.Color2, "#FF00E676"));
            bridge.Attach("Attesa_Sfumatura", () => Settings.Idle.Gradient);
            bridge.Attach("Attesa_Animazione", () => Settings.Idle.Animation);
            bridge.Attach("Attesa_Immagine", () => Settings.Idle.ImagePath);
            bridge.Attach("Attesa_Orologio", () => Settings.Idle.ShowClock);
            bridge.Attach("Attesa_Scritta", () => Settings.Idle.ShowWaiting);
            bridge.Attach("Attesa_Stemma", () => Settings.Idle.BadgeEnabled);
            bridge.Attach("Attesa_StemmaColore", () => NormalizeColor(Settings.Idle.BadgeFill, "#FF1A1A1A"));
            bridge.Attach("Attesa_StemmaBordo", () => NormalizeColor(Settings.Idle.BadgeBorder, "#FFFF8C00"));
            bridge.Attach("Attesa_StemmaBordoSpessore", () => Settings.Idle.BadgeBorderWidth);
            bridge.Attach("Attesa_StemmaDim", () => Settings.Idle.BadgeSize);
            bridge.Attach("Attesa_StemmaLogo", () => Settings.Idle.BadgeLogoPath);
            bridge.Attach("Attesa_StemmaLogoRapporto", () => Settings.Idle.BadgeLogoAspect);
            bridge.Attach("Attesa_StemmaTesto", () => Settings.Idle.BadgeText);
            bridge.Attach("Attesa_StemmaTestoColore", () => NormalizeColor(Settings.Idle.BadgeTextColor, "#FFFFFFFF"));
            bridge.Attach("Attesa_StemmaNumero", () => Settings.Idle.BadgeShowNumber && !string.IsNullOrEmpty(Settings.Idle.Number));
            bridge.Attach("Avviso_Danni", () => alertDamage);
            for (int i = 0; i < 8; i++) { int zi = i; bridge.Attach("Danno_Z" + (zi + 1) + "_Livello", () => dent[zi]); }
            foreach (var key in new[] { "FL", "FR", "RL", "RR", "Aero" })
            {
                string kk = key;
                bridge.Attach("Danno_" + kk + "_Testo", () => { string v; lock (colors) return colors.TryGetValue("DannoTesto_" + kk, out v) ? v : "OK"; });
                bridge.Attach("Danno_" + kk + "_Colore", () => { string v; lock (colors) return colors.TryGetValue("DannoColore_" + kk, out v) ? v : "#FFFFFFFF"; });
            }
            bridge.Attach("Avviso_Box", () => alertBox);
            // previsioni meteo (LMU): 5 punti della sessione
            for (int fi = 0; fi < 5; fi++)
            {
                int q = fi;
                string pre = "Prev_" + (fi + 1) + "_";
                bridge.Attach(pre + "Ora", () => lmuRest != null && lmuRest.FreshForecast ? FcLabels[q] : "");
                bridge.Attach(pre + "Icona", () => lmuRest != null && lmuRest.FreshForecast && lmuRest.FcSky[q] >= 0 ? SkyIcon(lmuRest.FcSky[q]).ToString(CultureInfo.InvariantCulture) : "");
                bridge.Attach(pre + "Cielo", () => lmuRest != null && lmuRest.FreshForecast && lmuRest.FcSky[q] >= 0 && lmuRest.FcSky[q] < SkyShort.Length ? SkyShort[lmuRest.FcSky[q]] : "");
                bridge.Attach(pre + "Temp", () => lmuRest != null && lmuRest.FreshForecast && !double.IsNaN(lmuRest.FcTemp[q]) ? Math.Round(lmuRest.FcTemp[q]).ToString(CultureInfo.InvariantCulture) + "°" : "");
                bridge.Attach(pre + "Pioggia", () => lmuRest != null && lmuRest.FreshForecast && !double.IsNaN(lmuRest.FcRain[q]) ? Math.Round(lmuRest.FcRain[q]).ToString(CultureInfo.InvariantCulture) + "%" : "");
            }
            // classifiche (generale = 0, di classe = 1)
            for (int sc = 0; sc < 2; sc++)
            {
                int q = sc;
                string pre = "Std_" + (sc == 0 ? "G" : "C") + "_";
                bridge.Attach(pre + "N", () => stdN[q]);
                bridge.Attach(pre + "Io", () => stdIo[q]);
                for (int r = 0; r < Catalog.StandingsMax; r++)
                {
                    int rr = r;
                    string rp = pre + (r + 1) + "_";
                    bridge.Attach(rp + "Pos", () => stdRows[q][rr].Pos);
                    bridge.Attach(rp + "Nome", () => stdRows[q][rr].Name);
                    bridge.Attach(rp + "Classe", () => stdRows[q][rr].Cls);
                    bridge.Attach(rp + "ClasseColore", () => stdRows[q][rr].ClsColor);
                    bridge.Attach(rp + "Distacco", () => stdRows[q][rr].Gap);
                    bridge.Attach(rp + "Io", () => stdRows[q][rr].Me);
                }
            }
            bridge.Attach("PosGain_Colore", () => posGainColor);
            bridge.Attach("Pista_Stato", () => TrackState(0));
            bridge.Attach("Pista_StatoColore", () => TrackState(1));
            bridge.Attach("Pista_StatoTesto", () => TrackState(2));
            bridge.Attach("PosGain_Dir", () => posGainDir);
            bridge.Attach("Box_TempoCorsia", () => pitLaneSince == DateTime.MaxValue ? "" : ValueFormatter.LapTime(Math.Max(0, (DateTime.Now - pitLaneSince).TotalSeconds)));
            string[] sides = { "Avanti", "Dietro" };
            for (int i = 0; i < 2; i++)
            {
                int k = i;
                bridge.Attach("Box_" + sides[k] + "_Nome", () => boxName[k]);
                bridge.Attach("Box_" + sides[k] + "_Numero", () => boxNum[k]);
                bridge.Attach("Box_" + sides[k] + "_Classe", () => boxClass[k]);
                bridge.Attach("Box_" + sides[k] + "_ClasseColore", () => boxClassColor[k]);
                bridge.Attach("Box_" + sides[k] + "_Distacco", () => boxGap[k]);
                bridge.Attach("Box_" + sides[k] + "_DistaccoScheda", () => boxGapCard[k]);
                bridge.Attach("Box_" + sides[k] + "_UltimoGiro", () => boxLast[k]);
                bridge.Attach("Box_" + sides[k] + "_NomeScheda", () => boxNameCard[k]);
            }
            for (int z = 1; z <= 8; z++)
            {
                string key = "Danno_Z" + z + "_Colore";
                colors[key] = "#FF3A3A3A";
                bridge.Attach(key, () => ColorOf(key));
            }
            foreach (var w in Wheels)
            {
                string key = "Gomma_" + w + "_StatoColore";
                colors[key] = "#FF5A5A5A";
                bridge.Attach(key, () => ColorOf(key));
            }
            bridge.Attach("PostItAttivo", () => HasPostIt());
            bridge.Attach("Avviso_Attivo", () => alertActive);
            bridge.Attach("Avviso_Variante", () => alertVariant);
            bridge.Attach("Avviso_N", () => alertN);
            bridge.Attach("Avviso_Titolo", () => alertTitle);
            bridge.Attach("Avviso_Sfondo", () => alertBg);
            bridge.Attach("Avviso_Testo", () => alertFg);
            for (int i = 0; i < AlertSettings.MaxValues; i++)
            {
                int idx = i;
                bridge.Attach("Avviso_V" + (idx + 1), () => alertV[idx]);
                bridge.Attach("Avviso_L" + (idx + 1), () => alertL[idx]);
            }
            for (int i = 0; i < Catalog.MaxSideSlots; i++)
            {
                int idx = i;
                bridge.Attach("Slot_L" + (idx + 1), () => SlotValue(Page.Left, idx));
                bridge.Attach("Slot_R" + (idx + 1), () => SlotValue(Page.Right, idx));
            }
            for (int i = 0; i < Catalog.MaxToolSlots; i++)
            {
                int idx = i;
                bridge.Attach("Slot_T" + (idx + 1), () => SlotValue(Page.Bottom, idx));
            }
            for (int i = 0; i < Catalog.CenterRows; i++)
            {
                int idx = i;
                bridge.Attach("Slot_C" + (idx + 1), () => SlotValue(Page.Center, idx));
            }

            // Azioni da collegare ai tasti (SimHub > Controlli ed eventi)
            bridge.AddAction("PaginaSuccessiva", () => ChangePage(1));
            bridge.AddAction("PaginaPrecedente", () => ChangePage(-1));
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                bridge.AddAction("VaiAPagina" + (idx + 1), () => SetActivePage(idx, true));
            }
        }

        /// <summary>Scrive su disco il logo incorporato "DT Racing Team" (PluginsData\Common\DTLoghi) e ne restituisce il percorso.</summary>
        public static string EnsureDtLogo()
        {
            try
            {
                string dir = System.IO.Path.Combine(FedeLog.Folder, "DTLoghi");
                System.IO.Directory.CreateDirectory(dir);
                string path = System.IO.Path.Combine(dir, "DT Racing Team.png");
                byte[] bytes = Convert.FromBase64String(Logos.DtRacingTeamPng);
                if (!System.IO.File.Exists(path) || new System.IO.FileInfo(path).Length != bytes.Length)
                    System.IO.File.WriteAllBytes(path, bytes);
                return path;
            }
            catch (Exception ex)
            {
                FedeLog.Error("logo DT Racing Team", ex);
                return "";
            }
        }

        System.Threading.Timer updateTimer;
        string popupShownFor = "";
        dynamic updatePopup;

        /// <summary>Nuova versione trovata: finestra di avviso in SimHub (una volta per versione).</summary>
        void OnUpdaterChanged()
        {
            try
            {
                if (!Updater.Available || Updater.Busy || Updater.Installed) return;
                if (Settings == null || !Settings.UpdatePopup) return;
                string ver = Updater.LatestVersion;
                if (string.IsNullOrEmpty(ver) || ver == popupShownFor) return;
                popupShownFor = ver;
                var appType = Wpf.T("System.Windows.Application");
                object app = appType.GetProperty("Current").GetValue(null, null);
                if (app == null) return;
                object disp = appType.GetProperty("Dispatcher").GetValue(app, null);
                Action show = () => ShowUpdatePopup(ver);
                disp.GetType().GetMethod("BeginInvoke", new[] { typeof(Delegate), typeof(object[]) }).Invoke(disp, new object[] { show, new object[0] });
            }
            catch (Exception ex) { FedeLog.Error("avviso aggiornamento", ex); }
        }

        void ShowUpdatePopup(string ver)
        {
            try
            {
                if (updatePopup != null) { try { updatePopup.Close(); } catch { } updatePopup = null; }
                string notes = Updater.Notes ?? "";
                dynamic w = Wpf.Xaml(
                    "<Window {NS} Title='DT Plugin - aggiornamento' Width='460' SizeToContent='Height' WindowStartupLocation='CenterScreen' " +
                    "ResizeMode='NoResize' Topmost='True' ShowInTaskbar='True' Background='#FF1E1E1E'>" +
                    "<StackPanel Margin='20,18,20,16'>" +
                    "<TextBlock Text='È disponibile DT Plugin " + Wpf.Esc(ver) + "' FontSize='17' FontWeight='Bold' Foreground='White'/>" +
                    "<TextBlock Text='Hai la versione " + Wpf.Esc(VersionText) + ". Plugin e dashboard vengono scaricati da GitHub, poi SimHub si riavvia. Le tue impostazioni restano.' " +
                    "TextWrapping='Wrap' FontSize='12' Foreground='#FFBDBDBD' Margin='0,8,0,0'/>" +
                    (notes.Length > 0 ? "<TextBlock Text='" + Wpf.Esc("Novità: " + notes) + "' TextWrapping='Wrap' FontSize='12' Foreground='#FF64B5F6' Margin='0,10,0,0'/>" : "") +
                    "<StackPanel Orientation='Horizontal' HorizontalAlignment='Right' Margin='0,18,0,0'>" +
                    "<Button x:Name='BtnLater' Content='Più tardi' Padding='14,6' Margin='0,0,10,0'/>" +
                    "<Button x:Name='BtnInstall' Content='Installa e riavvia SimHub' Padding='14,6' FontWeight='Bold' Background='#FF1565C0' Foreground='White'/>" +
                    "</StackPanel></StackPanel></Window>");
                object later = Wpf.Find(w, "BtnLater"), inst = Wpf.Find(w, "BtnInstall");
                object wo = w;
                Wpf.On(later, "Click", (a, b) => { try { ((dynamic)wo).Close(); } catch { } });
                Wpf.On(inst, "Click", (a, b) => { try { ((dynamic)wo).Close(); } catch { } Updater.InstallAsync(); });
                updatePopup = w;
                w.Show();
                FedeLog.Write("Avviso aggiornamento mostrato: " + ver);
            }
            catch (Exception ex) { FedeLog.Error("finestra aggiornamento", ex); }
        }

        public void End(PluginManager pluginManager)
        {
            try { if (updateTimer != null) updateTimer.Dispose(); } catch { }
            try { if (lmuRest != null) lmuRest.Stop(); } catch { }
            SaveSettings();
            FedeLog.Write("Chiusura");
        }

        public void SaveSettings()
        {
            SettingsStore.Save(Settings);
        }

        public Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            try
            {
                var ui = new SettingsUi(this);
                return (Control)ui.Root;
            }
            catch (Exception ex)
            {
                FedeLog.Error("pannello impostazioni", ex);
                return null;
            }
        }

        static ImageSource icon;
        public ImageSource PictureIcon
        {
            get
            {
                if (icon == null)
                {
                    try
                    {
                        // menu di SimHub: solo il logo DT, in bianco
                        // riquadro trasparente 128x128 con il logo (largo 100) al centro: SimHub stira l'icona in un quadrato
                        dynamic img = Wpf.Xaml("<DrawingImage {NS}><DrawingImage.Drawing><DrawingGroup>" +
                                               "<GeometryDrawing Brush='Transparent'><GeometryDrawing.Geometry><RectangleGeometry Rect='0,0,128,128'/></GeometryDrawing.Geometry></GeometryDrawing>" +
                                               "<DrawingGroup><DrawingGroup.Transform><TranslateTransform X='14' Y='38.85'/></DrawingGroup.Transform>" +
                                               "<GeometryDrawing Brush='White' Geometry='" + Logo.Geometry + "'/></DrawingGroup>" +
                                               "</DrawingGroup></DrawingImage.Drawing></DrawingImage>");
                        img.Freeze();
                        icon = (ImageSource)(object)img;
                    }
                    catch (Exception ex) { FedeLog.Error("icona", ex); }
                }
                return icon;
            }
        }

        // ------------------------------------------------------------------ pagine
        public PageSettings Page
        {
            get
            {
                try
                {
                    var s = Settings;
                    if (s == null) return FallbackPage;
                    var pages = s.Pages;
                    if (pages == null || pages.Count == 0) return FallbackPage;
                    int i = s.ActivePage;
                    if (i < 0 || i >= pages.Count) i = 0;
                    return pages[i] ?? FallbackPage;
                }
                catch
                {
                    return FallbackPage; // la lista pagine e' stata modificata proprio ora dalle impostazioni
                }
            }
        }

        static int SlotValue(List<int> list, int index)
        {
            if (list == null || index < 0 || index >= list.Count) return 0;
            return list[index];
        }

        public void ChangePage(int delta)
        {
            int n = Settings.Pages.Count;
            if (n == 0) return;
            SetActivePage(((Settings.ActivePage + delta) % n + n) % n, true);
        }

        public void SetActivePage(int index, bool showToast)
        {
            if (index < 0 || index >= Settings.Pages.Count) return;
            Settings.ActivePage = index;
            if (showToast && Settings.PageToastSeconds > 0)
                toastUntil = DateTime.Now.AddSeconds(Settings.PageToastSeconds);
            var h = PageChanged;
            if (h != null) h();
        }

        // ------------------------------------------------------------------ canali dati
        void BuildRuntime()
        {
            var list = new List<ChannelRuntime>();
            foreach (var def in Catalog.Channels)
            {
                ChannelSettings cs = null;
                foreach (var c in Settings.Channels)
                    if (c.Id == def.Id) { cs = c; break; }
                if (cs == null) { cs = ChannelSettings.FromDef(def); Settings.Channels.Add(cs); }
                list.Add(new ChannelRuntime { Def = def, Settings = cs });
            }
            Runtime = list.ToArray();
        }

        /// <summary>Sostituisce tutte le impostazioni (importa / ripristina) tenendo collegati i valori gia' pubblicati.</summary>
        public void ApplySettings(FedeSettings ns)
        {
            ns.Normalize();
            foreach (var rt in Runtime)
            {
                foreach (var c in ns.Channels)
                    if (c.Id == rt.Def.Id)
                    {
                        rt.Settings.Label = c.Label; rt.Settings.Source = c.Source;
                        rt.Settings.Decimals = c.Decimals; rt.Settings.Unit = c.Unit;
                        break;
                    }
            }
            var chans = new List<ChannelSettings>();
            foreach (var rt in Runtime) chans.Add(rt.Settings);
            ns.Channels = chans;
            ns.Access = Settings.Access;   // il codice di accesso resta quello di questo PC (non si importa)
            Settings = ns;
            SaveSettings();
            SetActivePage(Math.Max(0, Math.Min(ns.ActivePage, ns.Pages.Count - 1)), false);
        }

        /// <summary>Riporta un canale ai valori originali (senza cambiare l'oggetto, cosi' le proprieta' restano collegate).</summary>
        public void ResetChannel(ChannelRuntime rt)
        {
            rt.Settings.Label = rt.Def.Label;
            rt.Settings.Source = rt.Def.Source;
            rt.Settings.Decimals = rt.Def.Decimals;
            rt.Settings.Unit = rt.Def.Unit;
        }

        bool isIRacing;
        string currentGame = "";

        /// <summary>Valori di iRacing per i canali che con LMU arrivano da dati specifici del gioco.</summary>
        object IRacingFallback(string id)
        {
            switch (id)
            {
                case "TCCut": return Raw("Telemetry.dcTractionControl2");
                case "Map": return Raw("Telemetry.dcFuelMixture") ?? Raw("Telemetry.dcThrottleShape");
                case "TC": return Raw("Telemetry.dcTractionControl");
                case "ABS": return Raw("Telemetry.dcABS");
                case "BB": return Raw("Telemetry.dcBrakeBias");
                default: return null;
            }
        }

        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (bridge == null) return;
            if (Access != null && !Access.Unlocked) return;   // senza codice valido il plugin non calcola niente
            try
            {
                var now = DateTime.Now;
                currentGame = Convert.ToString(bridge.GetPropertyValue("DataCorePlugin.CurrentGame") ?? "");
                isIRacing = currentGame.Equals("IRacing", StringComparison.OrdinalIgnoreCase);
                sessionTypeCache = Convert.ToString(bridge.GetPropertyValue("DataCorePlugin.GameData.SessionTypeName") ?? "");
                if (playerNameCache.Length == 0 || now.Second == 0) playerNameCache = SectorTracker.Str(Raw("CurrentPlayer.mDriverName"));
                lmuActive = AsBool(GetMember(data, "GameRunning")) && (currentGame.IndexOf("LMU", StringComparison.OrdinalIgnoreCase) >= 0 || Raw("Scoring.mVehicles") != null);
                if (now >= nextGapUpdate)
                {
                    nextGapUpdate = now.AddMilliseconds(250);
                    object gd = data;
                    object newData = null;
                    if (gd != null && AsBool(GetMember(gd, "GameRunning"))) newData = GetMember(gd, "NewData");
                    ComputeGaps(newData);
                }

                foreach (var rt in Runtime)
                {
                    object v;
                    string src = rt.Settings.Source;
                    if (string.IsNullOrWhiteSpace(src))
                    {
                        v = null;
                    }
                    else
                    {
                        src = src.Trim().TrimStart('[').TrimEnd(']').Trim();
                        v = src.StartsWith("@") ? Special(src, now) : bridge.GetPropertyValue(src);
                        if (v == null && isIRacing) v = IRacingFallback(rt.Def.Id);
                    }
                    double number;
                    string text;
                    try { text = ValueFormatter.Format(v, rt.Def.Format, rt.Settings.Decimals, rt.Settings.Unit, out number); }
                    catch { text = ValueFormatter.Empty; number = 0; }
                    rt.Value = number;
                    rt.Text = text;
                }
                try { UpdateTyreColors(); } catch { }
                try { UpdateEngineOff(now, AsBool(GetMember(data, "GameRunning"))); } catch { }
                try { sectors.Update(now, AsBool(GetMember(data, "GameRunning"))); } catch { }
                try { UpdateAids(now); } catch { }
                try { DashBuilder.Tick(Settings, now); } catch { }
                if (now >= nextStrategy)
                {
                    nextStrategy = now.AddSeconds(1);
                    try { UpdateStrategy(AsBool(GetMember(data, "GameRunning"))); } catch (Exception ex) { if (!stratErrorLogged) { FedeLog.Error("strategia", ex); stratErrorLogged = true; } }
                }
                CheckSession();
                try { UpdateAlerts(now, AsBool(GetMember(data, "GameRunning"))); }
                catch (Exception ex) { if (!alertErrorLogged) { FedeLog.Error("avvisi", ex); alertErrorLogged = true; } }
                bridge.UpdateFallbackProperties();
                if (++updates == 30) bridge.ReportCheck();
            }
            catch (Exception ex)
            {
                if (!dataErrorLogged) { FedeLog.Error("DataUpdate", ex); dataErrorLogged = true; }
            }
        }

        /// <summary>Quando cambia il tipo di sessione (prove/qualifica/gara) passa alla pagina segnata per quella sessione.</summary>
        void CheckSession()
        {
            string kind = "";
            var v = bridge.GetPropertyValue("DataCorePlugin.GameData.SessionTypeName");
            string t = v == null ? "" : v.ToString().ToUpperInvariant();
            if (t.Contains("QUALI")) kind = "Q";
            else if (t.Contains("RACE") || t.Contains("GARA")) kind = "R";
            else if (t.Length > 0) kind = "P";
            if (kind == "R" && simStartPos == 0 && AsInt(bridge.GetPropertyValue("DataCorePlugin.GameData.CurrentLap")) <= 1)
            {
                int sp = AsInt(TimingPosition());
                if (sp > 0) simStartPos = sp;     // altri giochi: posizione al via
            }
            if (kind == sessionKind) return;
            sessionKind = kind;
            simStartPos = 0; startPosAll = 0; startPosClass = 0;
            if (kind.Length == 0) return;
            var pages = Settings.Pages;
            for (int i = 0; i < pages.Count; i++)
            {
                var p = pages[i];
                bool match = (kind == "P" && p.AutoPractice) || (kind == "Q" && p.AutoQualify) || (kind == "R" && p.AutoRace);
                if (match)
                {
                    if (i != Settings.ActivePage) SetActivePage(i, true);
                    FedeLog.Write("Sessione " + t + ": pagina " + p.Name);
                    return;
                }
            }
        }

        // ------------------------------------------------------------------ tempi e posizioni
        public bool HasPostIt()
        {
            try { return bridge != null && bridge.GetPropertyValue("PostItNoteRacing.Version") != null; } catch { return false; }
        }

        /// <summary>Proprieta' Post-It Note Racing del team del giocatore (es. EstimatedLapTime, LivePositionInClass).</summary>
        object PostItTeam(string prop)
        {
            int pos = AsInt(bridge.GetPropertyValue("DataCorePlugin.GameData.Position"));
            if (pos <= 0) return null;
            var team = bridge.GetPropertyValue("PostItNoteRacing.LeaderboardPosition_" + pos.ToString("00") + "_Team");
            int t = AsInt(team);
            if (team == null) return null;
            return bridge.GetPropertyValue("PostItNoteRacing.Team_" + t.ToString("00") + "_" + prop);
        }

        static double Seconds(object v)
        {
            if (v == null) return double.NaN;
            if (v is TimeSpan) return ((TimeSpan)v).TotalSeconds;
            return Num(v);
        }

        double BestLapSeconds()
        {
            var b = Find("BestLap");
            if (b != null && b.Value > 0) return b.Value;
            return Seconds(bridge.GetPropertyValue("DataCorePlugin.GameData.BestLapTime"));
        }

        object TimingDelta()
        {
            switch (Settings.Timing.DeltaSource)
            {
                case 0:
                    {
                        double d = Num(Raw("CurrentPlayerTelemetry.mDeltaBest"));
                        if (!double.IsNaN(d)) return d;
                        break;
                    }
                case 2:
                    {
                        double est = Seconds(PostItTeam("EstimatedLapTime")), best = BestLapSeconds();
                        if (!double.IsNaN(est) && est > 0 && best > 0) return est - best;
                        break;
                    }
            }
            return bridge.GetPropertyValue("PersistantTrackerPlugin.SessionBestLiveDeltaSeconds");
        }

        object TimingEstimated()
        {
            switch (Settings.Timing.DeltaSource)
            {
                case 0:
                    {
                        double d = Num(Raw("CurrentPlayerTelemetry.mDeltaBest")), best = BestLapSeconds();
                        if (!double.IsNaN(d) && best > 0) return best + d;
                        break;
                    }
                case 2:
                    {
                        double est = Seconds(PostItTeam("EstimatedLapTime"));
                        if (!double.IsNaN(est) && est > 0) return est;
                        break;
                    }
            }
            return bridge.GetPropertyValue("PersistantTrackerPlugin.EstimatedLapTime_SessionBestBased");
        }

        object TimingPosition()
        {
            switch (Settings.Timing.PositionMode)
            {
                case 1:
                    if (playerClassPos > 0) return playerClassPos;
                    break;
                case 2:
                    {
                        var v = PostItTeam("LivePositionInClass");
                        if (v != null && AsInt(v) > 0) return v;
                        if (playerClassPos > 0) return playerClassPos;
                        break;
                    }
            }
            var gp = bridge.GetPropertyValue("DataCorePlugin.GameData.Position");
            if (AsInt(gp) <= 0 && lmuPlace > 0) return lmuPlace;
            return gp;
        }

        /// <summary>
        /// TC / ABS che intervengono (SimHub TCActive / ABSActive, tenuti accesi 0,25 s per non sfarfallare)
        /// e fuori giri (giri >= linea rossa della marcia di SimHub, altrimenti >= 97%).
        /// </summary>
        void UpdateAids(DateTime now)
        {
            if (Num(bridge.GetPropertyValue("DataCorePlugin.GameData.TCActive")) > 0) tcActiveUntil = now.AddMilliseconds(250);
            if (Num(bridge.GetPropertyValue("DataCorePlugin.GameData.ABSActive")) > 0) absActiveUntil = now.AddMilliseconds(250);
            double rpm = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Rpms"));
            double red = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.CarSettings_CurrentGearRedLineRPM"));
            double pct = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.CarSettings_CurrentDisplayedRPMPercent"));
            if (!double.IsNaN(rpm) && !double.IsNaN(red) && red > 500) overRev = rpm >= red;
            else overRev = !double.IsNaN(pct) && pct >= 97;
            if (engineOff) overRev = false;
        }

        /// <summary>Motore spento: giri motore a zero (o accensione spenta) per almeno 0,7 secondi, con il gioco in corso.</summary>
        void UpdateEngineOff(DateTime now, bool running)
        {
            if (!running) { engineOff = false; engineStoppedSince = DateTime.MaxValue; engineState = 0; lightsState = 0; wiperState = 0; return; }
            double rpm = Num(Raw("CurrentPlayerTelemetry.mEngineRPM"));
            if (double.IsNaN(rpm)) rpm = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Rpms"));
            double ign = Num(Raw("CurrentPlayerTelemetry.mIgnitionStarter"));
            if (double.IsNaN(ign)) ign = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.EngineIgnitionOn"));
            engineState = !double.IsNaN(rpm) && rpm >= 50 ? 2 : (!double.IsNaN(ign) && ign > 0 ? 1 : 0);
            double hl = Num(Raw("CurrentPlayerTelemetry.mHeadlights"));
            lightsState = !double.IsNaN(hl) && (((int)hl) & 1) == 1 ? 1 : 0;
            double wp = Num(Raw("PlayerNativeTelemetry.mWiperState"));
            wiperState = double.IsNaN(wp) ? 0 : Math.Max(0, Math.Min(3, (int)wp));
            bool stopped = (!double.IsNaN(rpm) && rpm < 50) || (!double.IsNaN(ign) && ign <= 0 && (double.IsNaN(rpm) || rpm < 300));
            if (!stopped) { engineOff = false; engineStoppedSince = DateTime.MaxValue; return; }
            if (engineStoppedSince == DateTime.MaxValue) engineStoppedSince = now;
            engineOff = (now - engineStoppedSince).TotalSeconds >= 0.7;
        }

        /// <summary>Testo per la scheda "Tempi e posizioni" delle impostazioni.</summary>
        public string TimingSummary()
        {
            Func<string, string> txt = id => { var c = Find(id); return c == null ? "-" : c.Text; };
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Delta          " + txt("Delta"));
            sb.AppendLine("Giro stimato   " + txt("EstLap"));
            sb.AppendLine("Posizione      " + txt("Pos") + "   (auto: " + txt("Cars") + ")");
            sb.AppendLine("Nella classe   " + (playerClassPos > 0 ? playerClassPos + " di " + classCount : "-"));
            sb.AppendLine("Avanti/dietro  " + txt("GapAhead") + "  /  " + txt("GapBehind"));
            string vs, fs;
            VeLapsLeft(out vs); FuelLapsLeft(out fs);
            sb.AppendLine("Giri energia   " + txt("VELaps") + "   (fonte: " + vs + ")");
            sb.AppendLine("Giri carburante " + txt("FuelLaps") + "  (fonte: " + fs + ")");
            string rsr; double rr = FuelRatio(out rsr);
            sb.AppendLine("Ratio carb./VE " + (double.IsNaN(rr) ? "-" : rr.ToString("0.00", CultureInfo.InvariantCulture) + "  (" + rsr + ")"));
            sb.Append("Post-It        " + (HasPostIt() ? "trovato" : "non trovato"));
            return sb.ToString();
        }

        // ------------------------------------------------------------------ danni e bandiere
        static string DentText(int sev)
        {
            switch (sev) { case 1: return "LIEVE"; case 2: return "MEDIO"; case 3: return "GRAVE"; default: return "OK"; }
        }

        static string DentTextColor(int sev)
        {
            return sev <= 0 ? "#FFFFFFFF" : DentColor(sev);
        }

        static string DentColor(int sev)
        {
            switch (sev) { case 1: return "#FFFFD600"; case 2: return "#FFFF8C00"; case 3: return "#FFFF1744"; default: return "#FF3A3A3A"; }
        }

        /// <summary>Legge le 8 zone danno e lo stato delle gomme; true se e' appena arrivato un danno nuovo.</summary>
        bool UpdateDamage()
        {
            bool fresh = false;
            var next = new Dictionary<string, string>();
            for (int z = 0; z < 8; z++)
            {
                double v = Num(Raw("CurrentPlayerTelemetry.mDentSeverity0" + (z + 1)));
                int sev = double.IsNaN(v) ? 0 : Math.Max(0, Math.Min(3, (int)Math.Round(v)));
                dent[z] = sev;
                if (sev > prevDent[z]) fresh = true;
                prevDent[z] = sev;
                next["Danno_Z" + (z + 1) + "_Colore"] = DentColor(sev);
            }
            for (int w = 0; w < 4; w++)
            {
                string st = ColorOf("Gomma_" + Wheels[w] + "_Stato");
                if ((st == "PERSA" || st == "FORATA") && st != prevWheelState[w]) fresh = true;
                prevWheelState[w] = st ?? "";
                next["Gomma_" + Wheels[w] + "_StatoColore"] = st == "PERSA" ? "#FFFF1744" : (st == "FORATA" ? "#FFFF8C00" : "#FF5A5A5A");
            }
            // stato degli angoli (ammaccatura dell'angolo + gomma) e dell'aerodinamica (muso + alettone)
            int[] cornerZone = { 1, 7, 3, 5 };   // indici dent[] di FL(zona2) FR(zona8) RL(zona4) RR(zona6)
            for (int w = 0; w < 4; w++)
            {
                string st = prevWheelState[w];
                string txt, col;
                if (st == "PERSA") { txt = "PERSA"; col = "#FFFF1744"; }
                else if (st == "FORATA") { txt = "FORATA"; col = "#FFFF8C00"; }
                else { txt = DentText(dent[cornerZone[w]]); col = DentTextColor(dent[cornerZone[w]]); }
                next["DannoTesto_" + Wheels[w]] = txt; next["DannoColore_" + Wheels[w]] = col;
            }
            int aero = Math.Max(dent[0], dent[4]);
            next["DannoTesto_Aero"] = DentText(aero); next["DannoColore_Aero"] = DentTextColor(aero);
            lock (colors) { foreach (var kv in next) colors[kv.Key] = kv.Value; }
            return fresh;
        }

        bool Flag(string name)
        {
            return Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Flag_" + name)) > 0;
        }

        /// <summary>Bandiera da mostrare ora ("" = nessuna). Verde e scacchi solo per qualche secondo quando compaiono.</summary>
        string CurrentFlag(DateTime now, double seconds)
        {
            bool green = Flag("Green"), cheq = Flag("Checkered");
            if (cheq && !lastChequered) { transientFlag = "Checkered"; flagTransientUntil = now.AddSeconds(Math.Max(1, seconds)); }
            else if (green && !lastGreen) { transientFlag = "Green"; flagTransientUntil = now.AddSeconds(Math.Max(1, seconds)); }
            lastGreen = green; lastChequered = cheq;
            if (Flag("Black")) return "Black";
            if (Flag("Orange")) return "Orange";
            bool known, lmuBlue;
            bool lmuYellow = LmuYellow(out known, out lmuBlue);
            if (known ? lmuYellow : Flag("Yellow")) return "Yellow";
            if (known ? lmuBlue : Flag("Blue")) return "Blue";
            if (Flag("White")) return "White";
            if (now < flagTransientUntil) return transientFlag;
            return "";
        }

        int flagLogLines;
        bool lastLoggedYellow;
        string lastFlagSnap = "";

        /// <summary>
        /// Gialla e blu lette dal punteggio di LMU (SimHub con LMU puo' lasciare Flag_Yellow sempre accesa):
        /// gialla = auto sotto gialla (mUnderYellow), full course yellow (mGamePhase 6) o gialla nel settore in cui sei;
        /// blu = mFlag 6 dell'auto del giocatore.
        /// </summary>
        bool LmuYellow(out bool known, out bool blue)
        {
            blue = false;
            object uy = Raw("CurrentPlayer.mUnderYellow"), ph = Raw("Scoring.mScoringInfo.mGamePhase");
            known = uy != null || ph != null;
            if (!known) return false;
            bool under = Num(uy) > 0;
            bool fcy = Num(ph) == 6;
            bool sectorY = false;
            string sf = "";
            try
            {
                var arr = Raw("Scoring.mScoringInfo.mSectorFlag") as Array;
                if (arr == null || arr.Length == 0)
                {
                    // LMU tramite SimHub: le gialle di settore stanno in Data.mSectorFlag01..03 (come nella dash TK)
                    object f1 = Raw("Data.mSectorFlag01"), f2 = Raw("Data.mSectorFlag02"), f3 = Raw("Data.mSectorFlag03");
                    if (f1 != null || f2 != null || f3 != null) arr = new object[] { f1 ?? 0, f2 ?? 0, f3 ?? 0 };
                }
                int sec = AsInt(Raw("CurrentPlayer.mSector"));
                // LMU/rF2: mSector 0 = settore 3. Non e' certo se mSectorFlag[0] sia il settore 1 o il 3: controllo entrambe
                int idxA = sec == 0 ? 2 : sec - 1, idxB = sec;
                if (arr != null)
                {
                    foreach (var v in arr) sf += Convert.ToString(v, CultureInfo.InvariantCulture) + " ";
                    if (idxA >= 0 && idxA < arr.Length && Num(arr.GetValue(idxA)) == 1) sectorY = true;
                    if (idxB >= 0 && idxB < arr.Length && Num(arr.GetValue(idxB)) == 1) sectorY = true;
                }
            }
            catch { }
            object fl = Raw("CurrentPlayer.mFlag"), yfs = Raw("Scoring.mScoringInfo.mYellowFlagState");
            blue = Num(fl) == 6;
            // LMU spesso non passa le gialle di settore (mSectorFlag vuoto): allora vale la gialla di SimHub
            bool simY = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Flag_Yellow")) > 0;
            bool yellow = under || fcy || sectorY || (sf.Trim().Length == 0 && simY);
            // diario: ogni volta che cambia uno dei dati bandiera (max 300 righe) -> DTPlugin.log
            string snap = "SimHub Y=" + bridge.GetPropertyValue("DataCorePlugin.GameData.Flag_Yellow") + " B=" + bridge.GetPropertyValue("DataCorePlugin.GameData.Flag_Blue") +
                          " | LMU mUnderYellow=" + uy + " mGamePhase=" + ph + " mYellowFlagState=" + yfs + " mSectorFlag=" + sf.Trim() +
                          " mSector=" + Raw("CurrentPlayer.mSector") + " mFlag=" + fl + " -> gialla=" + yellow + " blu=" + blue;
            if (snap != lastFlagSnap && flagLogLines < 300) { flagLogLines++; lastFlagSnap = snap; FedeLog.Write("Bandiere: " + snap); }
            lastLoggedYellow = yellow;
            return yellow;
        }

        static string FlagTileColor(string flag)
        {
            switch (flag)
            {
                case "Black": return "#FF000000";
                case "Orange": return "#FFFF8C00";
                case "Yellow": return "#FFFFD600";
                case "Blue": return "#FF2979FF";
                case "White": return "#FFFFFFFF";
                case "Green": return "#FF00C853";
                case "Checkered": return "#FF202020";
                default: return "#FF242424";
            }
        }

        static void FlagInfo(string flag, out string text, out string bg, out string fg)
        {
            switch (flag)
            {
                case "Black": text = "NERA"; bg = "#FF000000"; fg = "#FFFFFFFF"; break;
                case "Orange": text = "ARANCIONE"; bg = "#FFFF8C00"; fg = "#FF000000"; break;
                case "Yellow": text = "GIALLA"; bg = "#FFFFD600"; fg = "#FF000000"; break;
                case "Blue": text = "BLU"; bg = "#FF2979FF"; fg = "#FFFFFFFF"; break;
                case "White": text = "BIANCA"; bg = "#FFFFFFFF"; fg = "#FF000000"; break;
                case "Green": text = "VERDE"; bg = "#FF00C853"; fg = "#FFFFFFFF"; break;
                default: text = "SCACCHI"; bg = "#FF202020"; fg = "#FFFFFFFF"; break;
            }
        }

        void ShowFlag(AlertSettings a, string flag)
        {
            string text, bg, fg;
            switch (flag)
            {
                case "Black": text = "NERA"; bg = "#FF000000"; fg = "#FFFFFFFF"; break;
                case "Orange": text = "ARANCIONE"; bg = "#FFFF8C00"; fg = "#FF000000"; break;
                case "Yellow": text = "GIALLA"; bg = "#FFFFD600"; fg = "#FF000000"; break;
                case "Blue": text = "BLU"; bg = "#FF2979FF"; fg = "#FFFFFFFF"; break;
                case "White": text = "BIANCA"; bg = "#FFFFFFFF"; fg = "#FF000000"; break;
                case "Green": text = "VERDE"; bg = "#FF00C853"; fg = "#FFFFFFFF"; break;
                default: text = "SCACCHI"; bg = "#FF202020"; fg = "#FFFFFFFF"; break;
            }
            alertTitle = a.Title ?? "";
            alertV[0] = text; alertL[0] = "";
            for (int i = 1; i < AlertSettings.MaxValues; i++) { alertV[i] = ""; alertL[i] = ""; }
            alertN = 1;
            alertVariant = a.Variant;
            alertBg = a.UseFlagColor ? WithOpacity(bg, a.Opacity) : WithOpacity(a.Background, a.Opacity);
            alertFg = a.UseFlagColor ? fg : NormalizeColor(a.TextColor, "#FFFFFFFF");
            alertDamage = false;
            alertActive = true;
        }

        void ShowDamage(AlertSettings a)
        {
            alertTitle = a.Title ?? "";
            for (int i = 0; i < AlertSettings.MaxValues; i++) { alertV[i] = ""; alertL[i] = ""; }
            alertN = 1;
            alertVariant = a.Variant;
            alertBg = WithOpacity(a.Background, a.Opacity);
            alertFg = NormalizeColor(a.TextColor, "#FFFFFFFF");
            alertDamage = true;
            alertActive = true;
        }

        /// <summary>Energia virtuale LMU in % (telemetria: frazione 0-1).</summary>
        double VePercent()
        {
            double v = Num(Raw("PlayerNativeTelemetry.mVirtualEnergy"));
            return double.IsNaN(v) ? double.NaN : v * 100.0;
        }

        bool lapFromLine, lapClean;
        readonly List<double> fuelLaps = new List<double>();

        // Come la dash RBR32: consumo e giri di energia virtuale presi dai plugin LMU se installati
        // (TweaksOnTrack, LMU NeoRed, doX SessionData), altrimenti calcolati dal DT Plugin sui giri puliti.
        static readonly string[][] VeSources =
        {
            new[] { "TweaksOnTrack", "TweaksOnTrack.Virtual_Energy.AvgPerLap_%", "TweaksOnTrack.Virtual_Energy_Estimated.Laps" },
            new[] { "LMU NeoRed", "LMU_NeoRedPlugin.Energy.VE.PerLast5ClearLap_%", "LMU_NeoRedPlugin.Energy.VE.Computed_RemainLaps" },
            new[] { "doX SessionData", "doX_LMU_SessionDataPlugin.FuelCompanionVirtualEnergyAverage", "doX_LMU_SessionDataPlugin.FuelCompanionVirtualEnergyRemainingLaps" },
        };

        double VePerLap(out string source)
        {
            foreach (var vs in VeSources)
            {
                var o = bridge.GetPropertyValue(vs[1]);
                if (o == null) continue;
                double v = Num(o);
                if (!double.IsNaN(v) && v > 0.01) { source = vs[0]; return v; }
            }
            if (lmuRest != null) { double rp = lmuRest.VePerLapPct; if (!double.IsNaN(rp)) { source = "LMU (consumo previsto dal gioco)"; return rp; } }
            source = "DT Plugin";
            double a = VeAverage();
            if (double.IsNaN(a) || a <= 0.01) a = veLastLap;
            return a;
        }

        double VeLapsLeft(out string source)
        {
            foreach (var vs in VeSources)
            {
                var o = bridge.GetPropertyValue(vs[2]);
                if (o == null) continue;
                double v = Num(o);
                if (!double.IsNaN(v) && v > 0) { source = vs[0]; return v; }
            }
            if (lmuRest != null) { double rv = lmuRest.VeLaps; if (!double.IsNaN(rv) && rv > 0) { source = "LMU (consumo previsto dal gioco)"; return rv; } }
            double ve = VePercent(), per = VePerLap(out source);
            if (double.IsNaN(ve) || double.IsNaN(per) || per <= 0.01) return double.NaN;
            return ve / per;
        }

        // ------------------------------------------------------------------ strategia gara (come la barra LMU di RBR32)
        readonly Dictionary<string, string> strat = new Dictionary<string, string>();
        DateTime nextStrategy = DateTime.MinValue;
        bool stratErrorLogged;

        double FuelPerLap()
        {
            if (lmuRest != null) { double rf = lmuRest.FuelPerLapFresh; if (!double.IsNaN(rf)) return rf; }
            {
                string vs, rs;
                double ve = VePercent(), vePer = VePerLap(out vs), ratio = FuelRatio(out rs);
                if (!double.IsNaN(ve) && ve > 0 && vePer > 0.01 && !double.IsNaN(ratio)) return vePer * ratio;
            }
            if (fuelLaps.Count > 0) { double sm = 0; foreach (var x in fuelLaps) sm += x; return sm / fuelLaps.Count; }
            double v = Num(bridge.GetPropertyValue("DataCorePlugin.Computed.Fuel_LastLapConsumption"));
            if (!double.IsNaN(v) && v > 0.01) return v;
            v = Num(bridge.GetPropertyValue("DataCorePlugin.Computed.Fuel_LitersPerLap"));
            return !double.IsNaN(v) && v > 0.01 ? v : double.NaN;
        }

        /// <summary>Tempo sul giro stimato dai migliori giri delle altre auto della tua classe (per calcolare prima del tuo primo giro).</summary>
        double ClassLapEstimate()
        {
            try
            {
                var arr = Raw("Scoring.mVehicles") as IEnumerable;
                if (arr == null) return double.IsNaN(simhubClassBest) ? double.NaN : simhubClassBest + 0.5;
                int num = AsInt(Raw("Scoring.mScoringInfo.mNumVehicles"));
                string myCls = SectorTracker.Str(Raw("CurrentPlayer.mVehicleClass"));
                var bests = new List<double>();
                int i = 0;
                foreach (var v in arr)
                {
                    if (num > 0 && i++ >= num) break;
                    if (v == null) continue;
                    if (myCls.Length > 0 && SectorTracker.Str(SectorTracker.Get(v, "mVehicleClass")) != myCls) continue;
                    double b = Num(SectorTracker.Get(v, "mBestLapTime"));
                    if (b > 0) bests.Add(b);
                }
                double m = Median(bests);
                return double.IsNaN(m) ? double.NaN : m + 0.5;
            }
            catch { return double.NaN; }
        }

        void SetStrat(Dictionary<string, string> d)
        {
            lock (strat) { strat.Clear(); foreach (var kv in d) strat[kv.Key] = kv.Value; }
        }

        /// <summary>
        /// Soste, finestra box, ratio carburante/energia, stint, giri alla fine e "riempi a" (questo giro, prossimo, chiusura),
        /// con lo stesso metodo della barra LMU della dash RBR32. Solo in gara, dal 3o giro.
        /// </summary>
        void UpdateStrategy(bool running)
        {
            var d = new Dictionary<string, string>();
            string st = Convert.ToString(bridge.GetPropertyValue("DataCorePlugin.GameData.SessionTypeName") ?? "").ToUpperInvariant();
            bool race = st.Contains("RACE") || st.Contains("GARA");
            int lap = AsInt(bridge.GetPropertyValue("DataCorePlugin.GameData.CurrentLap"));
            double pos = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.TrackPositionPercent"));
            if (double.IsNaN(pos)) pos = 0;
            pos = Math.Max(0, Math.Min(1, pos));
            double best = Seconds(bridge.GetPropertyValue("DataCorePlugin.GameData.BestLapTime"));
            double last = Seconds(bridge.GetPropertyValue("DataCorePlugin.GameData.LastLapTime"));
            double fuelPer = FuelPerLap();
            string vsrc;
            double ve = VePercent(), vePer = VePerLap(out vsrc);
            // come il menu di LMU: consumo previsto e quantita' a bordo letti dal gioco, se disponibili
            if (lmuRest != null)
            {
                double rp = lmuRest.VePerLapPct;
                if (!double.IsNaN(rp) && rp > 0.01) vePer = rp;
                double rf = lmuRest.FuelPerLapFresh;
                if (!double.IsNaN(rf) && rf > 0) fuelPer = rf;
            }
            bool nrg = !double.IsNaN(ve) && ve > 0 && !double.IsNaN(vePer) && vePer > 0.01;

            if (!running || !race) { d["Attiva"] = "0"; SetStrat(d); return; }
            d["Attiva"] = "1";
            // Come LMU: calcola subito, appena c'e' un consumo per giro (stima dei plugin LMU o di SimHub) e un tempo sul giro.
            // Tempo sul giro: ultimo giro valido, altrimenti migliore + 0.5 s, altrimenti la media dei migliori giri della tua classe.
            // tempo sul giro: prima quello stimato da LMU (lo stesso del menu strategia), poi ultimo giro / migliore / media della classe
            double avgLap = lmuEstLap > 0 && lmuEstLap < 1800 ? lmuEstLap
                : ((best > 0 && last > 0 && last <= best * 1.15) ? last : (best > 0 ? best + 0.5 : ClassLapEstimate()));
            bool haveCons = nrg || !double.IsNaN(fuelPer);
            if (!haveCons || !(avgLap > 0))
            {
                foreach (var k in new[] { "Soste", "Finestra", "Ratio", "Stint", "Giri", "Necessaria", "Litri", "EnergiaNec", "Questo", "Prossimo", "Chiusura" }) d[k] = "-";
                d["FinestraColore"] = "#FF404040";
                SetStrat(d); return;
            }

            // giri alla fine (esclusa la corrente)
            double timeLeft = Seconds(bridge.GetPropertyValue("DataCorePlugin.GameData.SessionTimeLeft"));
            int remLaps = AsInt(bridge.GetPropertyValue("DataCorePlugin.GameData.RemainingLaps"));
            int lapsToEnd;
            if (!double.IsNaN(timeLeft) && timeLeft > 0 && timeLeft < 360000)
            {
                double after = timeLeft - (1 - pos) * avgLap;
                lapsToEnd = after > 0 ? (int)Math.Ceiling(after / avgLap) : 0;
            }
            else lapsToEnd = Math.Max(0, remLaps - 1);
            // giri alla fine con i decimali, come LMU ("36.3 giri alla fine")
            // come LMU: giri dopo quello in corso (il giro corrente non e' contato)
            double lapsFrac = (!double.IsNaN(timeLeft) && timeLeft > 0 && timeLeft < 360000)
                ? Math.Max(0, (timeLeft - (1 - pos) * avgLap) / avgLap) : Math.Max(0, remLaps - 1);
            // giri da guidare davvero: resto del giro corrente + giri interi dopo (allo scadere del tempo il giro va finito)
            double lapsDrive = (1 - pos) + Math.Ceiling(lapsFrac - 1e-6);
            int fillLaps = lapsToEnd == 0 ? 0 : lapsToEnd + 1;

            // risorsa: energia (auto con VE) o carburante
            double fuel = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Fuel"));
            double maxFuel = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.MaxFuel"));
            Func<double, double, double, int, double[]> plan = (tank, pl, cp, lp) =>
            {
                double end = tank - (1 - pos) * pl;                   // a fine giro corrente
                double lapsEnd = end / pl;
                double finish = pl * fillLaps - end;                 // quanto serve per finire
                bool need = finish > 0 && lapsEnd < lapsToEnd;
                double close = lp + Math.Floor(lapsEnd);
                return new[] { end, lapsEnd, need ? finish : 0, close };
            };
            double[] pv = nrg ? plan(ve, vePer, 100, lap) : null;
            double[] pf = (!double.IsNaN(fuel) && !double.IsNaN(fuelPer) && fuelPer > 0) ? plan(fuel, fuelPer, maxFuel, lap) : null;

            // conta la risorsa che finisce prima
            bool useVe = nrg && (pf == null || pv[3] <= pf[3]);
            double[] P = useVe ? pv : pf;
            double per = useVe ? vePer : fuelPer;
            double cap = useVe ? 100 : (maxFuel > 0 ? maxFuel : 100);
            string unit = useVe ? "%" : "l";
            if (P == null) { d["Soste"] = "-"; SetStrat(d); return; }

            int stops = 0;
            if (P[2] > 0) stops = (int)Math.Ceiling(P[2] / (useVe ? 100 : Math.Max(1, cap - 0.8 * per)));
            if (nrg && pf != null && pf[2] > 0 && maxFuel > 0) stops = Math.Max(stops, (int)Math.Ceiling(pf[2] / Math.Max(1, maxFuel - 0.8 * fuelPer)));
            int closeLap = (int)P[3];
            int endLap = lap + lapsToEnd;
            int openLap = closeLap;
            if (stops == 1)
            {
                double add = Math.Min(P[2], cap);
                openLap = lap + Math.Max(0, (int)Math.Ceiling((P[0] + add - cap) / per));
            }
            bool inLane = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.IsInPitLane")) > 0;

            string win, wcol;
            if (stops == 0 || closeLap > endLap) { win = "NIENTE RIFORNIMENTO"; wcol = "#FF404040"; }
            else if (closeLap == endLap) { win = "RISPARMIA"; wcol = "#FFB8860B"; }
            else if (lap == closeLap - 1) { win = "BOX PROSSIMO GIRO"; wcol = "#FF2E7D32"; }
            else if (closeLap <= lap) { win = "BOX ORA!"; wcol = "#FFD50000"; }
            else if (inLane) { win = "IN CORSIA BOX"; wcol = "#FF1565C0"; }
            else if (lap >= openLap) { win = "CHIUDE GIRO " + closeLap; wcol = "#FF2E7D32"; }
            else if (stops > 1 || openLap >= closeLap) { win = "BOX GIRO " + closeLap; wcol = "#FFD50000"; }
            else { win = "GIRO " + openLap + " - GIRO " + closeLap; wcol = "#FFD50000"; }

            d["Soste"] = stops.ToString(CultureInfo.InvariantCulture);
            d["Finestra"] = win;
            d["FinestraColore"] = wcol;
            string rsrc; double lmuRatio = FuelRatio(out rsrc);
            d["Ratio"] = nrg ? (!double.IsNaN(lmuRatio) ? lmuRatio : (fuelPer > 0 ? fuelPer / vePer : double.NaN)).ToString("0.00", CultureInfo.InvariantCulture) : "-";
            if (d["Ratio"] == "NaN") d["Ratio"] = "-";
            int stintLaps = stops > 0 ? Math.Max(0, closeLap - lap) : lapsToEnd;
            var stTs = TimeSpan.FromSeconds(stintLaps * avgLap);
            d["Stint"] = stTs.TotalHours >= 1 ? ((int)stTs.TotalHours + ":" + stTs.Minutes.ToString("00") + ":" + stTs.Seconds.ToString("00")) : (stTs.Minutes + ":" + stTs.Seconds.ToString("00"));
            d["Giri"] = lapsFrac.ToString("0.0", CultureInfo.InvariantCulture);
            // quanto serve ancora per arrivare alla fine (come "61% VE necessaria" di LMU)
            double needTot = per * lapsDrive - (useVe ? ve : fuel);
            d["Necessaria"] = needTot > 0 ? Math.Ceiling(needTot).ToString("0", CultureInfo.InvariantCulture) + (useVe ? " %" : " l") : "0";
            // litri ed energia che servono ancora per arrivare alla fine (0 = bastano quelli a bordo)
            if (!double.IsNaN(fuel) && fuelPer > 0)
            {
                // come LMU ("VE necessaria"): quanto serve per i giri alla fine, senza togliere quello a bordo
                double nl = fuelPer * lapsFrac;
                d["Litri"] = Math.Ceiling(nl).ToString("0", CultureInfo.InvariantCulture) + " l";
            }
            else d["Litri"] = "-";
            if (nrg)
            {
                double ne = vePer * lapsFrac;
                d["EnergiaNec"] = Math.Ceiling(ne).ToString("0", CultureInfo.InvariantCulture) + " %";
            }
            else d["EnergiaNec"] = "-";

            Func<int, string> fillTo = pitLap =>
            {
                if (stops == 0 || P[2] <= 0) return "-";
                if (pitLap < openLap) return "-";
                double atPit = P[0] - (pitLap - lap) * per;
                double to = Math.Min(Math.Ceiling(atPit + P[2]), cap);
                return to.ToString("0", CultureInfo.InvariantCulture) + (unit == "%" ? " %" : " l");
            };
            d["Questo"] = fillTo(lap);
            d["Prossimo"] = fillTo(lap + 1);
            d["Chiusura"] = fillTo(closeLap);
            SetStrat(d);
        }

        /// <summary>Giri di carburante come RBR32: stima di SimHub, altrimenti litri / consumo dell'ultimo giro.</summary>
        /// <summary>
        /// Rapporto carburante / energia virtuale (litri per 1% di VE), come "Ratio carburante/EV" di LMU:
        /// dal plugin LMU NeoRed (menu box), altrimenti misurato sui giri puliti (litri/giro : %VE/giro).
        /// </summary>
        public double FuelRatio(out string source)
        {
            foreach (var key in new[] { "LMU_NeoRedPlugin.PitMenu.FuelRatio", "TweaksOnTrack.Fuel.Ratio", "TweaksOnTrack.PitMenu.FuelRatio" })
            {
                double r = Num(bridge.GetPropertyValue(key));
                if (!double.IsNaN(r) && r > 0.05 && r < 5) { source = key.StartsWith("LMU_Neo") ? "LMU NeoRed" : "TweaksOnTrack"; return r; }
            }
            source = "";
            if (fuelLaps.Count > 0 && veLaps.Count > 0)
            {
                double f = 0, v = 0;
                foreach (var x in fuelLaps) f += x;
                foreach (var x in veLaps) v += x;
                f /= fuelLaps.Count; v /= veLaps.Count;
                if (v > 0.01 && f > 0) { source = "misurato"; return f / v; }
            }
            return double.NaN;
        }

        double FuelLapsLeft(out string source)
        {
            // LMU: carburante a bordo / consumo previsto dal gioco (gli stessi numeri del menu)
            if (lmuRest != null)
            {
                double rl = lmuRest.FuelLaps;
                if (!double.IsNaN(rl) && rl > 0) { source = "LMU (consumo previsto dal gioco)"; return rl; }
            }
            // auto con energia virtuale (LMU): carburante per giro = energia per giro x ratio -> giusto gia' dal primo giro
            {
                string vs, rs;
                double ve = VePercent(), vePer = VePerLap(out vs), ratio = FuelRatio(out rs);
                double fuelNowV = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Fuel"));
                if (!double.IsNaN(ve) && ve > 0 && vePer > 0.01 && !double.IsNaN(ratio) && !double.IsNaN(fuelNowV) && fuelNowV > 0)
                {
                    source = "energia x ratio (" + vs + ", ratio " + ratio.ToString("0.00", CultureInfo.InvariantCulture) + " " + rs + ")";
                    return fuelNowV / (vePer * ratio);
                }
            }
            // prima la media dei giri puliti (come per l'energia), poi SimHub
            if (fuelLaps.Count > 0)
            {
                double fuelNow = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Fuel"));
                double sum = 0; foreach (var x in fuelLaps) sum += x;
                double avg = sum / fuelLaps.Count;
                if (!double.IsNaN(fuelNow) && avg > 0.01) { source = "DT Plugin (media " + fuelLaps.Count + " giri puliti)"; return fuelNow / avg; }
            }
            double g = Num(bridge.GetPropertyValue("DataCorePlugin.Computed.Fuel_RemainingLaps"));
            if (!double.IsNaN(g) && g > 0) { source = "SimHub"; return g; }
            double fuel = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.Fuel"));
            double per = Num(bridge.GetPropertyValue("DataCorePlugin.Computed.Fuel_LastLapConsumption"));
            source = "SimHub (ultimo giro)";
            if (double.IsNaN(per) || per <= 0.01) { per = fuelLastLap; source = "DT Plugin"; }
            if (double.IsNaN(fuel) || double.IsNaN(per) || per <= 0.01) return double.NaN;
            return fuel / per;
        }

        static double Median(List<double> l)
        {
            if (l == null || l.Count == 0) return double.NaN;
            var c = new List<double>(l); c.Sort();
            return c.Count % 2 == 1 ? c[c.Count / 2] : (c[c.Count / 2 - 1] + c[c.Count / 2]) / 2;
        }

        /// <summary>Consumo medio di energia virtuale per giro (ultimi 5 giri validi).</summary>
        double VeAverage()
        {
            if (veLaps.Count == 0) return double.NaN;
            double sum = 0;
            foreach (var x in veLaps) sum += x;
            return sum / veLaps.Count;
        }

        ChannelRuntime Find(string id)
        {
            foreach (var rt in Runtime) if (rt.Def.Id == id) return rt;
            return null;
        }

        AlertSettings Alert(string id)
        {
            foreach (var a in Settings.Alerts) if (a.Id == id) return a;
            return AlertSettings.Default(id);
        }

        /// <summary>Mostra subito un avviso di prova sulla dashboard (pulsante "Prova" nelle impostazioni).</summary>
        public void TestAlert(string kind)
        {
            testKind = kind;
            testUntil = DateTime.Now.AddSeconds(3);
        }

        void UpdateAlerts(DateTime now, bool running)
        {
            if (running) { if (runningSince == DateTime.MaxValue) runningSince = now; }
            else runningSince = DateTime.MaxValue;
            bool settled = running && (now - runningSince).TotalSeconds > 3;

            var pit = Alert("Pit"); var chg = Alert("Change"); var lap = Alert("Lap");

            // fine giro + consumo dell'ultimo giro.
            // Conta solo giri "puliti": iniziati sul traguardo (non all'uscita dai box / garage o a meta' giro),
            // senza passare dalla corsia box e senza rifornimento; scarta i giri molto diversi dalla media (FCY, errori).
            var lapRt = Find("Lap"); var fuelRt = Find("Fuel"); var pitRt = Find("InPit");
            double rawLaps = Num(Raw("CurrentPlayer.mTotalLaps"));
            double lapNo = !double.IsNaN(rawLaps) ? rawLaps : (lapRt != null ? lapRt.Value : 0);
            double fuel = fuelRt != null ? fuelRt.Value : double.NaN;
            bool laneNow = pitRt != null && pitRt.Value > 0;
            if (laneNow || !running) lapClean = false;
            if (lastLapNumber >= 0 && lapNo == lastLapNumber + 1)
            {
                double ve = VePercent();
                if (lapFromLine && lapClean)
                {
                    if (!double.IsNaN(fuelAtLapStart) && fuel > 0 && fuelAtLapStart > fuel)
                    {
                        double usedF = fuelAtLapStart - fuel;
                        double medF = Median(fuelLaps);
                        if (double.IsNaN(medF) || fuelLaps.Count < 2 || (usedF > medF * 0.6 && usedF < medF * 1.6))
                        {
                            fuelLastLap = usedF;
                            fuelLaps.Add(usedF);
                            if (fuelLaps.Count > 5) fuelLaps.RemoveAt(0);
                        }
                    }
                    if (!double.IsNaN(veAtLapStart) && !double.IsNaN(ve) && veAtLapStart > ve && veAtLapStart - ve < 50)
                    {
                        double used = veAtLapStart - ve;
                        double med = Median(veLaps);
                        if (double.IsNaN(med) || veLaps.Count < 2 || (used > med * 0.6 && used < med * 1.6))
                        {
                            veLastLap = used;
                            veLaps.Add(used);
                            if (veLaps.Count > 5) veLaps.RemoveAt(0);
                        }
                    }
                }
                fuelAtLapStart = fuel;
                veAtLapStart = ve;
                lapFromLine = true;
                lapClean = !laneNow;
                if (lap.Enabled && settled) lapUntil = now.AddSeconds(Math.Max(0.5, lap.Seconds));
            }
            else if (lapNo != lastLapNumber)
            {
                fuelAtLapStart = fuel;
                veAtLapStart = VePercent();
                lapFromLine = false;
                if (lapNo < lastLapNumber) { veLaps.Clear(); fuelLaps.Clear(); veLastLap = double.NaN; fuelLastLap = double.NaN; } // nuova sessione
            }
            lastLapNumber = lapNo;

            // cambio valori (BB, TC, ABS...)
            foreach (var id in chg.Channels)
            {
                var rt = Find(id);
                if (rt == null) continue;
                string t = rt.Text, old;
                if (lastTexts.TryGetValue(id, out old) && old != t && old != ValueFormatter.Empty && t != ValueFormatter.Empty && settled && chg.Enabled)
                {
                    changeId = id;
                    changeUntil = now.AddSeconds(Math.Max(0.3, chg.Seconds));
                }
                lastTexts[id] = t;
            }

            var lim = Find("PitLimiter"); var inPit = Find("InPit");
            // limitatore: prima la telemetria LMU (mSpeedLimiter), altrimenti il valore di SimHub
            bool limiterOn = lim != null && lim.Value > 0;
            bool inLane = inPit != null && inPit.Value > 0;
            bool pitOn = pit.Enabled && (limiterOn || (pit.PitLaneToo && inLane));
            if (inLane || limiterOn) { if (pitLaneSince == DateTime.MaxValue) pitLaneSince = now; }
            else pitLaneSince = DateTime.MaxValue;

            var dmg = Alert("Damage"); var flg = Alert("Flag");
            if (UpdateDamage() && dmg.Enabled && settled) damageUntil = now.AddSeconds(Math.Max(0.5, dmg.Seconds));
            string flag = CurrentFlag(now, flg.Seconds);
            currentFlag = running ? flag : "";

            if (now < testUntil)
            {
                if (testKind == "Damage") ShowDamage(dmg);
                else if (testKind == "Flag") { if (flg.FlagAsPopup) ShowFlag(flg, "Yellow"); else { alertActive = false; alertDamage = false; } }
                else ShowAlert(Alert(testKind), testKind == "Change" ? FirstOr(chg.Channels, "BB") : null, testKind == "Pit" ? (pit.Title ?? "") : null);
            }
            else if (pitOn) ShowAlert(pit, null, limiterOn ? pit.Title : "CORSIA BOX");
            else if (now < damageUntil) ShowDamage(dmg);
            else if (flg.Enabled && flg.FlagAsPopup && flag.Length > 0 && running) ShowFlag(flg, flag);
            else if (now < changeUntil) ShowAlert(chg, changeId, null);
            else if (now < lapUntil) ShowAlert(lap, null, null);
            else { alertActive = false; alertDamage = false; }
            // bandiera nel riquadro in alto a destra
            string bf = (now < testUntil && testKind == "Flag") ? "Yellow" : (running ? flag : "");
            if (flg.Enabled && !flg.FlagAsPopup && bf.Length > 0)
            {
                string t, b, f;
                FlagInfo(bf, out t, out b, out f);
                boxFlagText = t; boxFlagBg = b; boxFlagFg = f; boxFlagOn = true;
            }
            else boxFlagOn = false;
            bool showingPit = (now < testUntil && testKind == "Pit") || (now >= testUntil && pitOn);
            alertBox = alertActive && showingPit && !pit.SimpleLayout;
        }

        static string FirstOr(List<string> list, string def)
        {
            return list != null && list.Count > 0 ? list[0] : def;
        }

        /// <summary>Prepara i valori dell'avviso. singleId = mostra solo quel valore (cambio valori).</summary>
        void ShowAlert(AlertSettings a, string singleId, string titleOverride)
        {
            var ids = new List<string>();
            if (singleId != null) ids.Add(singleId);
            else foreach (var c in a.Channels) { if (ids.Count < AlertSettings.MaxValues && Find(c) != null) ids.Add(c); }
            int n = Math.Max(1, ids.Count);
            for (int i = 0; i < AlertSettings.MaxValues; i++)
            {
                var rt = i < ids.Count ? Find(ids[i]) : null;
                alertV[i] = rt != null ? rt.Text : "";
                alertL[i] = rt != null ? rt.Settings.Label : "";
            }
            if (singleId != null)
            {
                var rt = Find(singleId);
                alertTitle = rt != null ? rt.Settings.Label : "";
                alertL[0] = "";   // cambio valori: solo il valore, nessuna scritta sotto
            }
            else alertTitle = titleOverride ?? a.Title ?? "";
            alertN = n;
            alertVariant = a.Variant;
            alertBg = WithOpacity(a.Background, a.Opacity);
            alertFg = NormalizeColor(a.TextColor, "#FFFFFFFF");
            alertDamage = false;
            alertActive = true;
        }

        public static string NormalizeColor(string c, string def)
        {
            if (string.IsNullOrWhiteSpace(c)) return def;
            c = c.Trim();
            if (!c.StartsWith("#")) c = "#" + c;
            if (c.Length == 7) c = "#FF" + c.Substring(1);
            return c.Length == 9 ? c.ToUpperInvariant() : def;
        }

        static string WithOpacity(string c, int opacity)
        {
            c = NormalizeColor(c, "#FF000000");
            int a = (int)Math.Round(Math.Max(0, Math.Min(100, opacity)) * 2.55);
            return "#" + a.ToString("X2") + c.Substring(3);
        }

        string ColorOf(string key)
        {
            string c;
            lock (colors) { return colors.TryGetValue(key, out c) ? c : "#FF9E9E9E"; }
        }

        /// <summary>Legge una proprieta' grezza del gioco (GameRawData...), provando i due modi di scriverla.</summary>
        object Raw(string path)
        {
            if (rawPrefix != null) return bridge.GetPropertyValue(rawPrefix + path);
            foreach (var pre in new[] { "DataCorePlugin.GameRawData.", "GameRawData." })
            {
                var v = bridge.GetPropertyValue(pre + path);
                if (v != null) { rawPrefix = pre; return v; }
            }
            return null;
        }

        static double Num(object v)
        {
            if (v == null) return double.NaN;
            try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { return double.NaN; }
        }

        /// <summary>Kelvin (telemetria LMU) -> gradi C; NaN se il dato non c'e'.</summary>
        double RawKelvin(string wheelIdx, string field)
        {
            double k = Num(Raw("CurrentPlayerTelemetry.mWheels" + wheelIdx + "." + field));
            return (double.IsNaN(k) || k <= 0) ? double.NaN : k - 273.15;
        }

        /// <summary>Temperatura gomma secondo la fonte scelta (correzione per LMU).</summary>
        object TyreTemp(string wheel)
        {
            int w = Array.IndexOf(Wheels, wheel);
            if (w < 0) return null;
            var t = Settings.Tyres;
            string idx = "0" + (w + 1);
            double c = double.NaN;
            switch (t.TempSource)
            {
                case 1:
                    c = (RawKelvin(idx, "mTemperature01") + RawKelvin(idx, "mTemperature02") + RawKelvin(idx, "mTemperature03")) / 3; break;
                case 2:
                    c = RawKelvin(idx, "mTemperature02"); break;
                case 3:
                    c = (RawKelvin(idx, "mTireInnerLayerTemperature01") + RawKelvin(idx, "mTireInnerLayerTemperature02") + RawKelvin(idx, "mTireInnerLayerTemperature03")) / 3; break;
                case 4:
                    c = RawKelvin(idx, "mTireCarcassTemperature"); break;
            }
            if (double.IsNaN(c)) c = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.TyreTemperature" + WheelNames[w]));
            if (double.IsNaN(c)) return null;
            return c + t.TempOffset;
        }

        /// <summary>Pressione gomma: telemetria LMU in kPa (come l'HUD del gioco), altrimenti SimHub; convertita nell'unita' scelta.</summary>
        object TyrePressure(string wheel)
        {
            int w = Array.IndexOf(Wheels, wheel);
            if (w < 0) return null;
            var t = Settings.Tyres;
            double kpa = Num(Raw("CurrentPlayerTelemetry.mWheels0" + (w + 1) + ".mPressure"));
            if (double.IsNaN(kpa))
            {
                double v = Num(bridge.GetPropertyValue("DataCorePlugin.GameData.TyrePressure" + WheelNames[w]));
                if (double.IsNaN(v)) return null;
                var u = bridge.GetPropertyValue("DataCorePlugin.GameData.TyrePressureUnit");
                string unit = u == null ? "psi" : u.ToString().ToLowerInvariant();
                kpa = unit.Contains("kpa") ? v : (unit.Contains("bar") ? v * 100.0 : v * 6.894757);
            }
            return t.FromKpa(kpa);
        }

        /// <summary>"" se la gomma e' a posto, altrimenti "PERSA" o "FORATA".</summary>
        string TyreState(int w)
        {
            string b = "CurrentPlayerTelemetry.mWheels0" + (w + 1) + ".";
            if (AsBool(Raw(b + "mDetached")) || Num(Raw(b + "mDetached")) > 0) return "PERSA";
            if (AsBool(Raw(b + "mFlat")) || Num(Raw(b + "mFlat")) > 0) return "FORATA";
            var p = Find("TyreP" + Wheels[w]);
            if (p != null && p.Text != ValueFormatter.Empty && p.Value <= 0.5)
            {
                // pressione a zero mentre le altre gomme sono gonfie: gomma andata
                foreach (var o in Wheels)
                {
                    var op = Find("TyreP" + o);
                    if (op != null && op.Value > 5) return "FORATA";
                }
            }
            return "";
        }

        /// <summary>Temperature di superficie sinistra/centro/destra (per le 3 strisce).</summary>
        double[] SurfaceTemps(int w)
        {
            string idx = "0" + (w + 1);
            var r = new[] { RawKelvin(idx, "mTemperature01"), RawKelvin(idx, "mTemperature02"), RawKelvin(idx, "mTemperature03") };
            if (double.IsNaN(r[0]) || double.IsNaN(r[1]) || double.IsNaN(r[2]))
            {
                string b = "DataCorePlugin.GameData.TyreTemperature" + WheelNames[w];
                double inner = Num(bridge.GetPropertyValue(b + "Inner")), mid = Num(bridge.GetPropertyValue(b + "Middle")), outer = Num(bridge.GetPropertyValue(b + "Outer"));
                bool leftSide = w == 0 || w == 2;
                r = leftSide ? new[] { outer, mid, inner } : new[] { inner, mid, outer };
            }
            return r;
        }

        static string Lerp(uint a, uint b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));
            Func<int, int> ch = sh => (int)Math.Round(((a >> sh) & 0xFF) + ((((b >> sh) & 0xFF) - (double)((a >> sh) & 0xFF)) * t));
            return "#FF" + ch(16).ToString("X2") + ch(8).ToString("X2") + ch(0).ToString("X2");
        }

        /// <summary>Blu sotto "freddo", verde a "ottimale", rosso sopra "caldo" (sfumato in mezzo).</summary>
        public static string ThreeColors(double v, double cold, double opt, double hot)
        {
            const uint Blue = 0x2979FF, Green = 0x00E676, Red = 0xFF1744;
            if (double.IsNaN(v) || v <= 0) return "#FF9E9E9E";
            if (v <= cold) return Lerp(Blue, Blue, 0);
            if (v < opt) return Lerp(Blue, Green, (v - cold) / Math.Max(0.001, opt - cold));
            if (v < hot) return Lerp(Green, Red, (v - opt) / Math.Max(0.001, hot - opt));
            return Lerp(Red, Red, 0);
        }

        /// <summary>
        /// Mescola della gomma: lettera e colore (S rossa, M gialla, H bianca, W blu).
        /// LMU: nome della mescola anteriore / posteriore dalla telemetria; iRacing: asciutto/bagnato; altrimenti SimHub.
        /// </summary>
        string Compound(int w, out string color)
        {
            color = "#FFFFFFFF";
            string name = "";
            try
            {
                if (lmuRest != null) name = lmuRest.Compound(w);   // LMU: menu gomme del gioco (come TK)
                if (string.IsNullOrWhiteSpace(name)) name = RawCompoundName(w < 2);
                if (string.IsNullOrWhiteSpace(name))
                {
                    object ir = Raw("Telemetry.PlayerTireCompound");
                    if (ir != null && isIRacing) name = AsInt(ir) >= 1 ? "Wet" : "";
                }
                if (string.IsNullOrWhiteSpace(name))
                    name = Convert.ToString(bridge.GetPropertyValue("DataCorePlugin.GameData.TyreCompound") ?? "");
            }
            catch { }
            string n = (name ?? "").Trim().ToUpperInvariant();
            if (n.Length == 0) return "";
            if (n.Contains("WET") || n.Contains("RAIN") || n.Contains("INTER") || n == "W") { color = "#FF2979FF"; return "W"; }
            if (n.Contains("SOFT") || n == "S") { color = "#FFFF1744"; return "S"; }
            if (n.Contains("MED") || n == "M") { color = "#FFFFD600"; return "M"; }
            if (n.Contains("HARD") || n == "H") { color = "#FFFFFFFF"; return "H"; }
            color = "#FF9E9E9E";
            return n.Substring(0, 1);
        }

        static readonly string[] CompoundRoots = { "CurrentPlayerTelemetry", "PlayerNativeTelemetry", "CurrentPlayer", "Telemetry", "PlayerTelemetry" };
        readonly Dictionary<Type, List<MemberInfo>> compoundMembers = new Dictionary<Type, List<MemberInfo>>();
        bool compoundLogged;

        /// <summary>
        /// Nome della mescola dai dati grezzi del gioco: cerca un campo con "Compound" (e Front/Rear) nei dati del giocatore.
        /// La prima volta scrive nel log cosa ha trovato.
        /// </summary>
        string RawCompoundName(bool front)
        {
            var log = new List<string>();
            string found = "";
            foreach (var root in CompoundRoots)
            {
                object o = Raw(root);
                if (o == null) continue;
                List<MemberInfo> mem;
                var t = o.GetType();
                if (!compoundMembers.TryGetValue(t, out mem))
                {
                    mem = new List<MemberInfo>();
                    foreach (var m in t.GetMembers(BindingFlags.Public | BindingFlags.Instance))
                        if ((m is FieldInfo || m is PropertyInfo) && m.Name.IndexOf("Compound", StringComparison.OrdinalIgnoreCase) >= 0) mem.Add(m);
                    compoundMembers[t] = mem;
                }
                foreach (var m in mem)
                {
                    object v = null;
                    try { v = m is FieldInfo ? ((FieldInfo)m).GetValue(o) : ((PropertyInfo)m).GetValue(o, null); } catch { }
                    string sv = SectorTracker.Str(v);
                    if (!compoundLogged) log.Add(root + "." + m.Name + "='" + sv + "'");
                    string nm = m.Name.ToLowerInvariant();
                    bool side = front ? (nm.Contains("front") || nm.Contains("fl") || nm.Contains("fr")) : (nm.Contains("rear") || nm.Contains("rl") || nm.Contains("rr"));
                    if (found.Length == 0 && side && sv.Length > 0 && !(v is IConvertible && !(v is string)))
                        found = sv;
                }
            }
            if (!compoundLogged && Raw("CurrentPlayerTelemetry") != null)
            {
                compoundLogged = true;
                FedeLog.Write("Mescole: " + (log.Count == 0 ? "nessun campo 'Compound' nei dati del gioco" : string.Join(" | ", log)));
            }
            return found;
        }

        void UpdateTyreColors()
        {
            var t = Settings.Tyres;
            var next = new Dictionary<string, string>();
            for (int w = 0; w < 4; w++)
            {
                string k = Wheels[w];
                var tt = Find("TyreT" + k); var tp = Find("TyreP" + k); var bt = Find("BrakeT" + k);
                next["TyreT" + k] = ThreeColors(tt != null ? tt.Value : double.NaN, t.TyreCold, t.TyreOptimal, t.TyreHot);
                next["TyreP" + k] = ThreeColors(tp != null ? tp.Value : double.NaN, t.PressLow, t.PressOptimal, t.PressHigh);
                next["Gomma_" + k + "_Stato"] = TyreState(w);
                string mc, ml = Compound(w, out mc);
                next["Gomma_" + k + "_Mescola"] = ml;
                next["Gomma_" + k + "_MescolaColore"] = mc;
                var tw = Find("TyreW" + k);
                double wear = tw != null && tw.Text != ValueFormatter.Empty ? tw.Value : double.NaN;
                next["TyreW" + k] = double.IsNaN(wear) || wear <= 0 ? "#FF9E9E9E" : (wear >= 60 ? "#FF00E676" : (wear >= 30 ? "#FFFFD600" : "#FFFF1744"));
                next["BrakeT" + k] = ThreeColors(bt != null ? bt.Value : double.NaN, t.BrakeCold, t.BrakeOptimal, t.BrakeHot);
                var surf = SurfaceTemps(w);
                for (int j = 0; j < 3; j++)
                    next["Strisce_" + k + "_" + (j + 1)] = double.IsNaN(surf[j]) ? "#FF3A3A3A" : ThreeColors(surf[j], t.TyreCold, t.TyreOptimal, t.TyreHot);
            }
            lock (colors) { foreach (var kv in next) colors[kv.Key] = kv.Value; }
        }

        object Special(string src, DateTime now)
        {
            if (src.StartsWith("@gomma_", StringComparison.OrdinalIgnoreCase)) return TyreTemp(src.Substring(7).ToUpperInvariant());
            if (src.StartsWith("@press_", StringComparison.OrdinalIgnoreCase)) return TyrePressure(src.Substring(7).ToUpperInvariant());
            if (src.StartsWith("@raw:", StringComparison.OrdinalIgnoreCase)) return Raw(src.Substring(5).Trim());
            switch (src.ToLowerInvariant())
            {
                case "@delta": return TimingDelta();
                case "@stimato": return TimingEstimated();
                case "@pos": return TimingPosition();
                case "@pos_guadagno":
                    {
                        int now0 = AsInt(TimingPosition());
                        int start = Settings.Timing.PositionMode != 0 ? startPosClass : startPosAll;
                        if (start <= 0 && simStartPos > 0) start = simStartPos;
                        if (now0 <= 0 || start <= 0) return null;
                        int g = start - now0;
                        posGainColor = g > 0 ? "#FF00E676" : (g < 0 ? "#FFFF1744" : "#FF9E9E9E");
                        posGainDir = Math.Sign(g);
                        return Math.Abs(g).ToString(CultureInfo.InvariantCulture);
                    }
                case "@auto":
                    if (Settings.Timing.PositionMode != 0 && classCount > 0) return classCount;
                    if (lmuCars > 0 && Raw("Scoring.mVehicles") != null) return lmuCars;   // LMU: auto in sessione
                    return bridge.GetPropertyValue("DataCorePlugin.GameData.OpponentsCount");
            }
            switch (src.ToLowerInvariant())
            {
                case "@ve": { double v = VePercent(); return double.IsNaN(v) ? null : (object)v; }
                case "@bagnato":
                    {
                        double v = Num(Raw("Scoring.mScoringInfo.mAvgPathWetness"));
                        if (!double.IsNaN(v)) return v * 100;
                        // iRacing: TrackWetness 1 = asciutta ... 7 = allagata (0 = sconosciuto)
                        double w = Num(Raw("Telemetry.TrackWetness"));
                        return double.IsNaN(w) || w < 1 ? null : (object)Math.Round((w - 1) / 6.0 * 100);
                    }
                case "@pioggia":
                    {
                        double v = Num(Raw("Scoring.mScoringInfo.mRaining"));
                        if (!double.IsNaN(v)) return v * 100;
                        double pr = Num(Raw("Telemetry.Precipitation"));   // iRacing 0..1
                        return double.IsNaN(pr) ? null : (object)(pr * 100);
                    }
                case "@ve_ultimo": return double.IsNaN(veLastLap) ? null : (object)veLastLap;
                case "@carb_media": { if (fuelLaps.Count == 0) return null; double sm = 0; foreach (var x in fuelLaps) sm += x; return sm / fuelLaps.Count; }
                case "@ve_media": { string vsrc; double a = VePerLap(out vsrc); return double.IsNaN(a) ? null : (object)a; }
                case "@ve_giri": { string vsrc; double g = VeLapsLeft(out vsrc); return double.IsNaN(g) ? null : (object)g; }
                case "@giri_carb": { string fsrc; double g = FuelLapsLeft(out fsrc); return double.IsNaN(g) ? null : (object)g; }
                case "@ers":
                    {
                        double b = Num(Raw("CurrentPlayerTelemetry.mBatteryChargeFraction"));
                        if (!double.IsNaN(b)) return b * 100.0;
                        return bridge.GetPropertyValue("DataCorePlugin.GameData.ERSPercent");
                    }
            }
            if (src.Equals("@limitatore", StringComparison.OrdinalIgnoreCase))
            {
                double rl = Num(Raw("CurrentPlayerTelemetry.mSpeedLimiter"));
                if (!double.IsNaN(rl)) return rl > 0 ? 1.0 : 0.0;
                return bridge.GetPropertyValue("DataCorePlugin.GameData.PitLimiterOn");
            }
            if (src.StartsWith("@rip_") || src.StartsWith("@sosp_") || src.StartsWith("@usura_freno_") || src == "@danno_aero" || src == "@danno_motore" ||
                src == "@gomme_nuove" || src == "@batteria" || src == "@track_limits" || src == "@cielo")
                return LmuSpecial(src);
            switch (src.ToLowerInvariant())
            {
                case "@ora": return now;
                case "@gap_avanti": return double.IsNaN(gapAhead) ? null : (object)gapAhead;
                case "@gap_dietro": return double.IsNaN(gapBehind) ? null : (object)gapBehind;
                case "@delta_ultimo":
                    {
                        var last = Find("LastLap"); var best = Find("BestLap");
                        if (last == null || best == null || last.Value <= 0 || best.Value <= 0) return null;
                        return last.Value - best.Value;
                    }
                case "@consumo_ultimo": return double.IsNaN(fuelLastLap) ? null : (object)fuelLastLap;
                default: return null;
            }
        }

        /// <summary>Distacco (s) dalla macchina davanti / dietro in classifica, letto dalla lista avversari di SimHub.</summary>
        void ComputeGaps(object newData)
        {
            gapAhead = double.NaN;
            gapBehind = double.NaN;
            if (newData == null) { playerClassPos = 0; classCount = 0; return; }
            try { if (ComputeGapsLmu(newData)) return; } catch { }
            try
            {
                var opponents = GetMember(newData, "Opponents") as IEnumerable;
                if (opponents == null) return;
                var all = new List<object>();
                object player = null;
                foreach (var o in opponents)
                {
                    if (o == null) continue;
                    all.Add(o);
                    if (AsBool(GetMember(o, "IsPlayer"))) player = o;
                }
                if (player == null) return;
                int playerPos = AsInt(GetMember(player, "Position"));
                string playerClass = (GetMember(player, "CarClass") ?? "").ToString();

                // auto della stessa classe ordinate per posizione
                var sameClass = new List<object>();
                foreach (var o in all)
                    if ((GetMember(o, "CarClass") ?? "").ToString() == playerClass && AsInt(GetMember(o, "Position")) > 0) sameClass.Add(o);
                sameClass.Sort((a, b) => AsInt(GetMember(a, "Position")).CompareTo(AsInt(GetMember(b, "Position"))));
                classCount = sameClass.Count;
                playerClassPos = sameClass.IndexOf(player) + 1;

                bool byClass = Settings.Timing.GapMode == 1;
                var list = byClass ? sameClass : all;
                if (!byClass) list.Sort((a, b) => AsInt(GetMember(a, "Position")).CompareTo(AsInt(GetMember(b, "Position"))));
                int idx = list.IndexOf(player);
                object oa = idx > 0 ? list[idx - 1] : null, ob = idx >= 0 && idx < list.Count - 1 ? list[idx + 1] : null;
                string stn = Convert.ToString(bridge.GetPropertyValue("DataCorePlugin.GameData.SessionTypeName") ?? "").ToUpperInvariant();
                bool raceS = stn.Contains("RACE") || stn.Contains("GARA");
                int lapsA = 0, lapsB = 0;
                if (oa != null) gapAhead = SimHubGap(oa, player, raceS, out lapsA);
                if (ob != null) gapBehind = SimHubGap(player, ob, raceS, out lapsB);
                if (!double.IsNaN(gapAhead)) gapAhead = -Math.Abs(gapAhead);
                if (!double.IsNaN(gapBehind)) gapBehind = Math.Abs(gapBehind);
                SetBoxCar(0, oa, gapAhead, lapsA);
                SetBoxCar(1, ob, gapBehind, lapsB);
                try { StandingsSimHub(all, player, raceS); } catch { ClearStandings(); }
                // migliori giri della classe (per la strategia prima del primo giro)
                var cb = new List<double>();
                foreach (var o in sameClass) { double b = Seconds(GetMember(o, "BestLapTime")); if (b > 0) cb.Add(b); }
                simhubClassBest = Median(cb);
                if (playerPos <= 0) { gapAhead = double.NaN; gapBehind = double.NaN; }
            }
            catch
            {
                gapAhead = double.NaN;
                gapBehind = double.NaN;
            }
        }

        volatile int lmuPlace, lmuCars;

        // ---- classifiche
        class StdRow { public string Pos = "", Name = "", Cls = "", ClsColor = "#FFC62828", Gap = ""; public bool Me; }
        StdRow[][] stdRows = NewStd();
        readonly int[] stdN = { 0, 0 }, stdIo = { 0, 0 };

        static StdRow[][] NewStd()
        {
            var a = new StdRow[2][];
            for (int i = 0; i < 2; i++) { a[i] = new StdRow[Catalog.StandingsMax]; for (int r = 0; r < a[i].Length; r++) a[i][r] = new StdRow(); }
            return a;
        }

        static string FmtRel(double g, int laps)
        {
            if (laps != 0) return (laps > 0 ? "+" : "-") + Math.Abs(laps) + "L";
            if (double.IsNaN(g)) return "-";
            double a = Math.Abs(g);
            return (g < 0 ? "-" : "+") + a.ToString(a < 100 ? "0.0" : "0", CultureInfo.InvariantCulture);
        }

        /// <summary>Riempie una classifica: rows gia' in ordine, me = indice del giocatore, gap/laps rispetto a me, pos = testo posizione.</summary>
        void FillStandings(int sc, int count, int me, Func<int, StdRow> make)
        {
            var rows = new StdRow[Catalog.StandingsMax];
            int n = Math.Min(count, rows.Length);
            for (int r = 0; r < rows.Length; r++) rows[r] = r < n ? (make(r) ?? new StdRow()) : new StdRow();
            stdRows[sc] = rows;
            stdN[sc] = n;
            stdIo[sc] = Math.Max(0, Math.Min(me, n - 1));
        }

        void ClearStandings() { for (int sc = 0; sc < 2; sc++) { stdN[sc] = 0; stdIo[sc] = 0; } }

        void StandingsLmu(List<LmuCar> cars, LmuCar me, bool race, Dictionary<string, object> extra)
        {
            for (int sc = 0; sc < 2; sc++)
            {
                var list = sc == 0 ? cars : cars.FindAll(c => c.Cls == me.Cls);
                int mi = list.IndexOf(me);
                FillStandings(sc, list.Count, mi, r =>
                {
                    var c = list[r];
                    var row = new StdRow { Pos = (sc == 0 ? c.Place : r + 1).ToString(CultureInfo.InvariantCulture), Name = ShortName(c.Name), Cls = c.Cls, Me = c == me };
                    object o;
                    if (extra.TryGetValue(c.Name, out o)) row.ClsColor = NormalizeColor(Convert.ToString(GetMember(o, "CarClassColor") ?? ""), "#FFC62828");
                    if (c == me) { row.Gap = ""; return row; }
                    bool ahead = c.Place < me.Place;
                    double g; int laps = 0;
                    if (race)
                    {
                        g = ahead ? ChainGap(cars, c, me, out laps) : ChainGap(cars, me, c, out laps);
                        if (double.IsNaN(g)) { g = ahead ? GapBetween(c, me, true) : GapBetween(me, c, true); laps = ahead ? LapsBetween(c, me) : LapsBetween(me, c); }
                        g = ahead ? -Math.Abs(g) : Math.Abs(g);
                        if (ahead) laps = -laps;
                    }
                    else g = (c.Best > 0 && me.Best > 0) ? c.Best - me.Best : double.NaN;
                    row.Gap = FmtRel(g, laps);
                    return row;
                });
            }
        }

        void StandingsSimHub(List<object> all, object player, bool race)
        {
            all = new List<object>(all);
            all.RemoveAll(o => AsInt(GetMember(o, "Position")) <= 0);
            all.Sort((a, b) => AsInt(GetMember(a, "Position")).CompareTo(AsInt(GetMember(b, "Position"))));
            string pc = (GetMember(player, "CarClass") ?? "").ToString();
            double myBest = Seconds(GetMember(player, "BestLapTime"));
            double myProg = AsDouble(GetMember(player, "CurrentLapHighPrecision"));
            for (int sc = 0; sc < 2; sc++)
            {
                var list = sc == 0 ? all : all.FindAll(o => (GetMember(o, "CarClass") ?? "").ToString() == pc);
                int mi = list.IndexOf(player);
                FillStandings(sc, list.Count, mi, r =>
                {
                    var o = list[r];
                    bool me = ReferenceEquals(o, player);
                    var row = new StdRow
                    {
                        Pos = (sc == 0 ? AsInt(GetMember(o, "Position")) : r + 1).ToString(CultureInfo.InvariantCulture),
                        Name = ShortName(Convert.ToString(GetMember(o, "Name") ?? "")),
                        Cls = Convert.ToString(GetMember(o, "CarClass") ?? ""),
                        ClsColor = NormalizeColor(Convert.ToString(GetMember(o, "CarClassColor") ?? ""), "#FFC62828"),
                        Me = me
                    };
                    if (me) return row;
                    bool ahead = r < mi;
                    double g; int laps = 0;
                    if (race)
                    {
                        g = AsDouble(GetMember(o, "GaptoPlayer"));
                        double pr = AsDouble(GetMember(o, "CurrentLapHighPrecision"));
                        if (!double.IsNaN(pr) && !double.IsNaN(myProg) && Math.Abs(pr - myProg) >= 1) laps = (int)Math.Floor(Math.Abs(pr - myProg)) * (ahead ? -1 : 1);
                        if (!double.IsNaN(g)) g = ahead ? -Math.Abs(g) : Math.Abs(g);
                    }
                    else
                    {
                        double b = Seconds(GetMember(o, "BestLapTime"));
                        g = (b > 0 && myBest > 0) ? b - myBest : double.NaN;
                    }
                    row.Gap = FmtRel(g, laps);
                    return row;
                });
            }
        }

        class LmuCar { public int Place, Qual; public double Tbl, Tbn, Lbn, Best, Last, Progress, LapsBehindLeader, Speed, EstLap; public string Cls = "", Name = ""; public bool Player; }
        volatile int startPosClass, startPosAll, simStartPos;
        volatile string posGainColor = "#FF9E9E9E";
        volatile int posGainDir;     // posizione di partenza (griglia) nella classe / assoluta
        double lmuEstLap = double.NaN;               // tempo sul giro stimato da LMU per il giocatore
        double lmuTrackLen = double.NaN;

        static double VecLen(object vec)
        {
            if (vec == null) return double.NaN;
            try
            {
                double x = Convert.ToDouble(SectorTracker.Get(vec, "x") ?? 0.0), y = Convert.ToDouble(SectorTracker.Get(vec, "y") ?? 0.0), z = Convert.ToDouble(SectorTracker.Get(vec, "z") ?? 0.0);
                return Math.Sqrt(x * x + y * y + z * z);
            }
            catch { return double.NaN; }
        }
        readonly int[] boxLaps = { 0, 0 };

        /// <summary>Distacchi, nomi e categorie dal punteggio di LMU (sempre presenti, anche quando SimHub non ha il distacco).</summary>
        bool ComputeGapsLmu(object newData)
        {
            var arr = Raw("Scoring.mVehicles") as IEnumerable;
            if (arr == null) return false;
            int num = AsInt(Raw("Scoring.mScoringInfo.mNumVehicles"));
            var cars = new List<LmuCar>();
            LmuCar me = null;
            double trackLen = Num(Raw("Scoring.mScoringInfo.mLapDist"));
            lmuTrackLen = trackLen;
            int i = 0;
            foreach (var v in arr)
            {
                if (num > 0 && i++ >= num) break;
                if (v == null) continue;
                var c = new LmuCar
                {
                    Place = AsInt(SectorTracker.Get(v, "mPlace")),
                    Tbl = Num(SectorTracker.Get(v, "mTimeBehindLeader")),
                    Cls = SectorTracker.Str(SectorTracker.Get(v, "mVehicleClass")),
                    Name = SectorTracker.Str(SectorTracker.Get(v, "mDriverName")),
                    Player = Num(SectorTracker.Get(v, "mIsPlayer")) > 0,
                    Best = Num(SectorTracker.Get(v, "mBestLapTime")),
                    Last = Num(SectorTracker.Get(v, "mLastLapTime")),
                    LapsBehindLeader = Num(SectorTracker.Get(v, "mLapsBehindLeader")),
                    Tbn = Num(SectorTracker.Get(v, "mTimeBehindNext")),
                    Lbn = Num(SectorTracker.Get(v, "mLapsBehindNext")),
                    Speed = VecLen(SectorTracker.Get(v, "mLocalVel")),
                    EstLap = Num(SectorTracker.Get(v, "mEstimatedLapTime")),
                    Qual = AsInt(SectorTracker.Get(v, "mQualification"))
                };
                double laps = Num(SectorTracker.Get(v, "mTotalLaps")), dist = Num(SectorTracker.Get(v, "mLapDist"));
                c.Progress = (double.IsNaN(laps) ? 0 : laps) + (trackLen > 0 && !double.IsNaN(dist) ? Math.Max(0, Math.Min(1, dist / trackLen)) : 0);
                if (c.Place <= 0) continue;
                cars.Add(c);
                if (c.Player) me = c;
            }
            if (me == null || cars.Count == 0) return false;
            cars.Sort((a, b) => a.Place.CompareTo(b.Place));
            lmuPlace = me.Place; lmuCars = cars.Count;
            lmuEstLap = me.EstLap > 0 ? me.EstLap : double.NaN;
            // posizione di partenza: griglia di LMU (mQualification), assoluta e nella classe
            if (me.Qual > 0)
            {
                startPosAll = me.Qual;
                int cp = 1;
                foreach (var c in cars) if (c != me && c.Cls == me.Cls && c.Qual > 0 && c.Qual < me.Qual) cp++;
                startPosClass = cp;
            }
            var same = cars.FindAll(c => c.Cls == me.Cls);
            classCount = same.Count;
            playerClassPos = same.IndexOf(me) + 1;
            var list = Settings.Timing.GapMode == 1 ? same : cars;
            int idx = list.IndexOf(me);
            LmuCar ahead = idx > 0 ? list[idx - 1] : null, behind = idx >= 0 && idx < list.Count - 1 ? list[idx + 1] : null;
            string st = Convert.ToString(bridge.GetPropertyValue("DataCorePlugin.GameData.SessionTypeName") ?? "").ToUpperInvariant();
            bool race = st.Contains("RACE") || st.Contains("GARA");
            boxLaps[0] = 0; boxLaps[1] = 0;
            if (race)
            {
                // gara: distacchi calcolati da LMU (tempo dalla macchina davanti, sommato sulle auto in mezzo)
                int la = 0, lb = 0;
                double ga = ahead != null ? ChainGap(cars, ahead, me, out la) : double.NaN;
                double gb = behind != null ? ChainGap(cars, me, behind, out lb) : double.NaN;
                if (ahead != null) { if (double.IsNaN(ga)) { ga = GapBetween(ahead, me, true); la = LapsBetween(ahead, me); } boxLaps[0] = la; gapAhead = -Math.Abs(ga); }
                if (behind != null) { if (double.IsNaN(gb)) { gb = GapBetween(me, behind, true); lb = LapsBetween(me, behind); } boxLaps[1] = lb; gapBehind = Math.Abs(gb); }
            }
            else
            {
                // prove e qualifica: come la tabella di LMU, differenza tra i giri migliori
                if (ahead != null) gapAhead = -Math.Abs(GapBetween(ahead, me, false));
                if (behind != null) gapBehind = Math.Abs(GapBetween(me, behind, false));
            }

            // colore classe e numero dalla lista di SimHub (stesso nome), se c'e'
            var extra = new Dictionary<string, object>();
            var opponents = GetMember(newData, "Opponents") as IEnumerable;
            if (opponents != null)
                foreach (var o in opponents)
                    if (o != null) { string n = Convert.ToString(GetMember(o, "Name") ?? ""); if (n.Length > 0) extra[n] = o; }
            SetBoxLmu(0, ahead, gapAhead, extra);
            SetBoxLmu(1, behind, gapBehind, extra);
            try { StandingsLmu(cars, me, race, extra); } catch (Exception ex) { FedeLog.Error("classifica LMU", ex); }
            return true;
        }

        /// <summary>
        /// Distacco in secondi tra due auto in classifica (a davanti a b).
        /// Gara: tempo dal leader di LMU; se non c'e', come RBR32: differenza di giri percorsi x tempo sul giro.
        /// Prove/qualifica: differenza tra i giri migliori.
        /// </summary>
        /// <summary>
        /// Distacco di LMU tra a (davanti) e b: somma di mTimeBehindNext delle auto da a (esclusa) a b (inclusa), in ordine di posizione.
        /// NaN se LMU non lo fornisce (tutti zero).
        /// </summary>
        static double ChainGap(List<LmuCar> overall, LmuCar a, LmuCar b, out int laps)
        {
            laps = 0;
            double t = 0, l = 0;
            bool any = false;
            foreach (var c in overall)
            {
                if (c.Place <= a.Place || c.Place > b.Place) continue;
                if (double.IsNaN(c.Tbn) || c.Tbn < 0) return double.NaN;
                if (c.Tbn > 0.0005) any = true;
                t += c.Tbn;
                if (!double.IsNaN(c.Lbn) && c.Lbn > 0) l += c.Lbn;
            }
            if (!any) return double.NaN;
            laps = l >= 1 ? (int)Math.Round(l) : 0;
            return t;
        }

        /// <summary>Giri interi di differenza (a davanti a b): 0 se sono nello stesso giro.</summary>
        static int LapsBetween(LmuCar a, LmuCar b)
        {
            if (!double.IsNaN(a.LapsBehindLeader) && !double.IsNaN(b.LapsBehindLeader) && b.LapsBehindLeader - a.LapsBehindLeader >= 1)
                return (int)Math.Round(b.LapsBehindLeader - a.LapsBehindLeader);
            double d = a.Progress - b.Progress;
            return d >= 1 ? (int)Math.Floor(d) : 0;
        }

        double GapBetween(LmuCar a, LmuCar b, bool race)
        {
            if (!race)
            {
                if (a.Best > 0 && b.Best > 0) return b.Best - a.Best;
                return double.NaN;
            }
            if (!double.IsNaN(a.Tbl) && !double.IsNaN(b.Tbl) && b.Tbl - a.Tbl > 0.001) return b.Tbl - a.Tbl;
            double laps = a.Progress - b.Progress;
            if (laps <= 0) return double.NaN;
            double refLap = b.Last > 0 ? b.Last : (b.Best > 0 ? b.Best : (a.Best > 0 ? a.Best : double.NaN));
            if (!double.IsNaN(refLap)) return laps * refLap;
            // inizio gara, nessun giro ancora: distanza in pista diviso la velocita' di chi sta dietro
            double sp = !double.IsNaN(b.Speed) && b.Speed > 5 ? b.Speed : (!double.IsNaN(a.Speed) && a.Speed > 5 ? a.Speed : double.NaN);
            if (lmuTrackLen > 0 && !double.IsNaN(sp)) return laps * lmuTrackLen / sp;
            return double.NaN;
        }

        void SetBoxLmu(int k, LmuCar c, double gap, Dictionary<string, object> extra)
        {
            if (c == null) { boxName[k] = ""; boxNum[k] = ""; boxClass[k] = ""; boxGap[k] = ""; boxGapCard[k] = "-"; boxLast[k] = "-"; boxNameCard[k] = ""; return; }
            boxName[k] = ShortName(c.Name);
            boxNameCard[k] = Trunc(boxName[k], 12);
            boxLast[k] = c.Last > 0 ? ValueFormatter.LapTime(c.Last) : "-";
            boxClass[k] = c.Cls;
            object o;
            if (extra.TryGetValue(c.Name, out o))
            {
                boxNum[k] = Convert.ToString(GetMember(o, "CarNumber") ?? "");
                boxClassColor[k] = NormalizeColor(Convert.ToString(GetMember(o, "CarClassColor") ?? ""), "#FFC62828");
            }
            else { boxNum[k] = ""; boxClassColor[k] = "#FFC62828"; }
            if (boxLaps[k] >= 1) { boxGap[k] = "+" + boxLaps[k] + "L"; boxGapCard[k] = boxGap[k]; return; }
            if (double.IsNaN(gap)) { boxGap[k] = ""; boxGapCard[k] = "-"; return; }
            double a = Math.Abs(gap);
            boxGap[k] = (k == 0 ? "-" : "+") + a.ToString(a < 10 ? "0.00" : "0.0", CultureInfo.InvariantCulture);
            boxGapCard[k] = (k == 0 ? "-" : "+") + a.ToString("0.0", CultureInfo.InvariantCulture);
        }

        double simhubClassBest = double.NaN;

        /// <summary>
        /// Distacco tra due auto dalla lista di SimHub (a davanti a b), per iRacing e gli altri giochi:
        /// gara = distacco di SimHub, se manca giri percorsi x tempo sul giro; doppiati = giri interi; prove/qualifica = differenza giri migliori.
        /// </summary>
        double SimHubGap(object a, object b, bool race, out int laps)
        {
            laps = 0;
            if (!race)
            {
                double ba = Seconds(GetMember(a, "BestLapTime")), bb = Seconds(GetMember(b, "BestLapTime"));
                if (ba > 0 && bb > 0) return bb - ba;
                // nessun giro migliore ancora: distacco di SimHub
                double g0 = AsDouble(GetMember(AsBool(GetMember(a, "IsPlayer")) ? b : a, "GaptoPlayer"));
                return g0;
            }
            double pa = AsDouble(GetMember(a, "CurrentLapHighPrecision")), pb = AsDouble(GetMember(b, "CurrentLapHighPrecision"));
            double d = (!double.IsNaN(pa) && !double.IsNaN(pb)) ? pa - pb : double.NaN;
            if (!double.IsNaN(d) && d >= 1) laps = (int)Math.Floor(d);
            // GaptoPlayer e' relativo al giocatore: per la coppia (davanti, giocatore) o (giocatore, dietro) e' gia' il distacco giusto
            double g = AsDouble(GetMember(AsBool(GetMember(a, "IsPlayer")) ? b : a, "GaptoPlayer"));
            if (!double.IsNaN(g) && Math.Abs(g) > 0.001) return g;
            double refLap = Seconds(GetMember(b, "LastLapTime"));
            if (!(refLap > 0)) refLap = Seconds(GetMember(b, "BestLapTime"));
            if (double.IsNaN(d) || d <= 0 || !(refLap > 0)) return double.NaN;
            return d * refLap;
        }

        void SetBoxCar(int k, object o, double gap, int laps = 0)
        {
            if (o == null) { boxName[k] = ""; boxNum[k] = ""; boxClass[k] = ""; boxGap[k] = ""; boxGapCard[k] = "-"; boxLast[k] = "-"; boxNameCard[k] = ""; return; }
            boxLaps[k] = laps;
            boxGapCard[k] = laps >= 1 ? "+" + laps + "L" : (double.IsNaN(gap) ? "-" : (k == 0 ? "-" : "+") + Math.Abs(gap).ToString("0.0", CultureInfo.InvariantCulture));
            boxName[k] = ShortName(Convert.ToString(GetMember(o, "Name") ?? ""));
            boxNameCard[k] = Trunc(boxName[k], 12);
            { double ll = Seconds(GetMember(o, "LastLapTime")); boxLast[k] = ll > 0 ? ValueFormatter.LapTime(ll) : "-"; }
            boxNum[k] = Convert.ToString(GetMember(o, "CarNumber") ?? "");
            boxClass[k] = Convert.ToString(GetMember(o, "CarClass") ?? "");
            string cc = Convert.ToString(GetMember(o, "CarClassColor") ?? "");
            boxClassColor[k] = NormalizeColor(cc, "#FFC62828");
            if (laps >= 1) { boxGap[k] = "+" + laps + "L"; return; }
            if (double.IsNaN(gap)) { boxGap[k] = ""; return; }
            double a = Math.Abs(gap);
            boxGap[k] = (k == 0 ? "-" : "+") + a.ToString(a < 10 ? "0.00" : "0.0", CultureInfo.InvariantCulture);
        }

        /// <summary>"Sebastiano Gattuso" -> "S. Gattuso"</summary>
        /// <summary>Accorcia un testo troppo lungo per il riquadro (con il punto finale).</summary>
        static string Trunc(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s ?? "";
            return s.Substring(0, max - 1).TrimEnd() + ".";
        }

        static string ShortName(string n)
        {
            n = (n ?? "").Trim();
            int sp = n.IndexOf(' ');
            if (sp <= 0) return n;
            return n.Substring(0, 1).ToUpperInvariant() + ". " + n.Substring(sp + 1).Trim();
        }

        /// <summary>Legge una proprieta' o un campo pubblico per nome (con cache).</summary>
        object GetMember(object obj, string name)
        {
            var t = obj.GetType();
            string key = t.FullName + "|" + name;
            MemberInfo mi;
            if (!memberCache.TryGetValue(key, out mi))
            {
                mi = null;
                // cerca dalla classe piu' specifica verso le basi (evita AmbiguousMatchException con proprieta' "new")
                for (var cur = t; cur != null && mi == null; cur = cur.BaseType)
                {
                    const BindingFlags f = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                    foreach (var pi0 in cur.GetProperties(f))
                        if (pi0.Name == name && pi0.GetIndexParameters().Length == 0) { mi = pi0; break; }
                    if (mi == null)
                        foreach (var fi0 in cur.GetFields(f))
                            if (fi0.Name == name) { mi = fi0; break; }
                }
                memberCache[key] = mi;
            }
            var pi = mi as PropertyInfo;
            if (pi != null) return pi.GetValue(obj, null);
            var fi = mi as FieldInfo;
            if (fi != null) return fi.GetValue(obj);
            return null;
        }

        static bool AsBool(object o)
        {
            return o is bool && (bool)o;
        }

        static int AsInt(object o)
        {
            if (o == null) return 0;
            try { return Convert.ToInt32(o, CultureInfo.InvariantCulture); } catch { return 0; }
        }

        static double AsDouble(object o)
        {
            if (o == null) return double.NaN;
            try { return Convert.ToDouble(o, CultureInfo.InvariantCulture); } catch { return double.NaN; }
        }
    }
}
