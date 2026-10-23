using System;
using System.Collections.Generic;

namespace FedeDashboard
{
    /// <summary>Impostazioni salvate in PluginsData\Common\DTPlugin.json (cartella di SimHub).</summary>
    public class FedeSettings
    {
        public int Version { get; set; } = 1;
        public int ActivePage { get; set; } = 0;
        public double PageToastSeconds { get; set; } = 1.5;
        public List<PageSettings> Pages { get; set; } = new List<PageSettings>();
        public List<ChannelSettings> Channels { get; set; } = new List<ChannelSettings>();
        public List<AlertSettings> Alerts { get; set; } = new List<AlertSettings>();
        public TyreSettings Tyres { get; set; } = new TyreSettings();
        public IdleSettings Idle { get; set; } = new IdleSettings();
        public TimingSettings Timing { get; set; } = new TimingSettings();
        public FontSettings Font { get; set; } = new FontSettings();
        public AccessSettings Access { get; set; } = new AccessSettings();
        /// <summary>Finestra di avviso in SimHub quando esce una nuova versione.</summary>
        public bool UpdatePopup { get; set; } = true;

        /// <summary>Completa/ripara le impostazioni lette da file (canali nuovi, liste corte, indici fuori range).</summary>
        public void Normalize()
        {
            if (Version < 1)
            {
                // file nuovo o letto senza costruttore: valori di partenza
                Version = 1;
                PageToastSeconds = 1.5;
            }
            if (Timing == null) Timing = new TimingSettings();
            Timing.Normalize();
            if (Access == null) Access = new AccessSettings();
            if (Font == null) Font = new FontSettings();
            Font.Normalize();
            if (Idle == null) Idle = new IdleSettings();
            Idle.Normalize();
            if (Tyres == null) Tyres = new TyreSettings();
            Tyres.Normalize();
            if (Pages == null) Pages = new List<PageSettings>();
            if (Channels == null) Channels = new List<ChannelSettings>();
            if (Pages.Count == 0) Pages.AddRange(Catalog.DefaultPages());
            foreach (var p in Pages) p.Normalize();
            if (ActivePage < 0 || ActivePage >= Pages.Count) ActivePage = 0;
            if (PageToastSeconds < 0) PageToastSeconds = 0;

            var byId = new Dictionary<string, ChannelSettings>();
            foreach (var c in Channels)
                if (c != null && !string.IsNullOrEmpty(c.Id) && !byId.ContainsKey(c.Id)) byId[c.Id] = c;
            var ordered = new List<ChannelSettings>();
            foreach (var def in Catalog.Channels)
            {
                ChannelSettings cs;
                if (!byId.TryGetValue(def.Id, out cs)) cs = ChannelSettings.FromDef(def);
                if (cs.Label == null) cs.Label = def.Label;
                if (cs.Source == null) cs.Source = def.Source;
                if (cs.Unit == null) cs.Unit = def.Unit;
                if (cs.Decimals < 0 || cs.Decimals > 6) cs.Decimals = def.Decimals;
                ordered.Add(cs);
            }
            Channels = ordered;
            // avvisi: sempre i tre tipi, nell'ordine Pit / Cambio / Giro
            if (Alerts == null) Alerts = new List<AlertSettings>();
            var alerts = new List<AlertSettings>();
            foreach (var id in AlertSettings.Kinds)
            {
                AlertSettings a = null;
                foreach (var x in Alerts) if (x != null && x.Id == id) { a = x; break; }
                if (a == null) a = AlertSettings.Default(id);
                a.Normalize();
                alerts.Add(a);
            }
            Alerts = alerts;

            if (Version < 2)
            {
                // v2: temperatura gomme letta dalla telemetria LMU (carcassa) tramite @gomma_XX
                foreach (var c in Channels)
                    if (c.Id.StartsWith("TyreT") && c.Source != null && c.Source.StartsWith("DataCorePlugin.GameData.TyreTemperature"))
                        c.Source = "@gomma_" + c.Id.Substring(5);
                Version = 2;
            }
            if (Version < 3)
            {
                // v3: pressioni lette dalla telemetria LMU (kPa come il gioco), temperatura = strato interno
                foreach (var c in Channels)
                    if (c.Id.StartsWith("TyreP") && c.Source != null && c.Source.StartsWith("DataCorePlugin.GameData.TyrePressure"))
                        c.Source = "@press_" + c.Id.Substring(5);
                if (Tyres.TempSource == 4) Tyres.TempSource = 3;
                if (Tyres.PressLow == 24.5 && Tyres.PressOptimal == 26.5 && Tyres.PressHigh == 28.5)
                {
                    Tyres.PressLow = 155; Tyres.PressOptimal = 170; Tyres.PressHigh = 185;
                }
                Tyres.PressureUnit = 0;
                Version = 3;
            }
            if (Version < 6)
            {
                // v6: delta, giro stimato, posizione e numero auto passano dalle opzioni "Tempi e posizioni"
                foreach (var c in Channels)
                {
                    if (c.Id == "Delta" && c.Source == "PersistantTrackerPlugin.SessionBestLiveDeltaSeconds") c.Source = "@delta";
                    if (c.Id == "EstLap" && c.Source == "PersistantTrackerPlugin.EstimatedLapTime_SessionBestBased") c.Source = "@stimato";
                    if (c.Id == "Pos" && c.Source == "DataCorePlugin.GameData.Position") c.Source = "@pos";
                    if (c.Id == "Cars" && c.Source == "DataCorePlugin.GameData.OpponentsCount") c.Source = "@auto";
                }
            }
            if (Version < 4)
            {
                // v4: l'avviso box compare solo col limitatore davvero inserito
                foreach (var a in Alerts) if (a.Id == "Pit") a.PitLaneToo = false;
                foreach (var c in Channels)
                    if (c.Id == "PitLimiter" && c.Source == "DataCorePlugin.GameData.PitLimiterOn") c.Source = "@limitatore";
                Version = 4;
            }
            if (Version < 5)
            {
                // v5: dati LMU presi dalla telemetria (come la dash RBR32): TC Cut, energia virtuale, batteria
                foreach (var c in Channels)
                {
                    if (c.Id == "TCCut" && string.IsNullOrWhiteSpace(c.Source)) c.Source = "@raw:PlayerNativeTelemetry.mTCCut";
                    if (c.Id == "VE" && string.IsNullOrWhiteSpace(c.Source)) { c.Source = "@ve"; c.Decimals = 1; }
                    if (c.Id == "VELaps" && string.IsNullOrWhiteSpace(c.Source)) c.Source = "@ve_giri";
                    if (c.Id == "ERS" && c.Source == "DataCorePlugin.GameData.ERSPercent") c.Source = "@ers";
                }
                // dove c'era la scheda ERS ora c'e' l'energia virtuale
                int ers = Array.IndexOf(Catalog.Cards, "ERS / batteria"), ve = Array.IndexOf(Catalog.Cards, "Energia virtuale (VE)");
                if (ers > 0 && ve > 0)
                    foreach (var pg in Pages)
                        foreach (var list in new[] { pg.Left, pg.Right, pg.Bottom, pg.Center })
                            for (int i = 0; i < list.Count; i++) if (list[i] == ers) list[i] = ve;
                Version = 5;
            }
            if (Version < 6) Version = 6;
            if (Version < 7)
            {
                // nuova schermata box: blu pieno come quella di riferimento
                foreach (var a in Alerts) if (a.Id == "Pit" && string.Equals(a.Background, "#FF0D47A1", StringComparison.OrdinalIgnoreCase)) a.Background = "#FF0000FF";
                Version = 7;
            }
            if (Version < 8)
            {
                // giri di carburante: nuova sorgente con stima di riserva
                foreach (var c in Channels)
                    if (c.Id == "FuelLaps" && (c.Source ?? "").Trim().Trim('[', ']') == "DataCorePlugin.Computed.Fuel_RemainingLaps") c.Source = "@giri_carb";
                Version = 8;
            }
            if (Version < 9)
            {
                // posizione nella propria classe (es. 3/12 GT3) invece che assoluta
                if (Timing != null && Timing.PositionMode == 0) Timing.PositionMode = 1;
                if (Timing != null) Timing.GapMode = 1;   // davanti/dietro: auto della propria classe, come la tabella di LMU
                Version = 9;
            }

        }

        public static FedeSettings CreateDefault()
        {
            var s = new FedeSettings();
            s.Normalize();
            return s;
        }
    }

    public class PageSettings
    {
        public string Name { get; set; } = "Pagina";
        public int Frame { get; set; }
        public int GridLeft { get; set; }
        public int GridRight { get; set; }
        public int Toolbar { get; set; }
        public int SidebarLeft { get; set; }
        public int SidebarRight { get; set; }
        public List<int> Left { get; set; } = new List<int>();
        public List<int> Right { get; set; } = new List<int>();
        public List<int> Bottom { get; set; } = new List<int>();
        public List<int> Center { get; set; } = new List<int>();
        public bool AutoPractice { get; set; }
        public bool AutoQualify { get; set; }
        public bool AutoRace { get; set; }
        public bool ShowInputs { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(Name)) Name = "Pagina";
            Frame = Clamp(Frame, Catalog.Frames.Length);
            GridLeft = Clamp(GridLeft, Catalog.Grids.Length);
            GridRight = Clamp(GridRight, Catalog.Grids.Length);
            Toolbar = Clamp(Toolbar, Catalog.Toolbars.Length);
            SidebarLeft = Clamp(SidebarLeft, Catalog.Bars.Length);
            SidebarRight = Clamp(SidebarRight, Catalog.Bars.Length);
            Left = Fix(Left, Catalog.MaxSideSlots);
            Right = Fix(Right, Catalog.MaxSideSlots);
            Bottom = Fix(Bottom, Catalog.MaxToolSlots);
            Center = Fix(Center, Catalog.CenterRows);
        }

        public PageSettings Clone(string newName)
        {
            return new PageSettings
            {
                Name = newName, Frame = Frame, GridLeft = GridLeft, GridRight = GridRight, Toolbar = Toolbar,
                SidebarLeft = SidebarLeft, SidebarRight = SidebarRight,
                Left = new List<int>(Left), Right = new List<int>(Right),
                Bottom = new List<int>(Bottom), Center = new List<int>(Center),
                AutoPractice = AutoPractice, AutoQualify = AutoQualify, AutoRace = AutoRace, ShowInputs = ShowInputs
            };
        }

        static int Clamp(int v, int count)
        {
            return (v < 0 || v >= count) ? 0 : v;
        }

        static List<int> Fix(List<int> list, int size)
        {
            var res = new List<int>();
            for (int i = 0; i < size; i++)
            {
                int v = (list != null && i < list.Count) ? list[i] : 0;
                res.Add(Clamp(v, Catalog.Cards.Length));
            }
            return res;
        }
    }

    public class ChannelSettings
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Source { get; set; }
        public int Decimals { get; set; }
        public string Unit { get; set; }

        public static ChannelSettings FromDef(ChannelDef d)
        {
            return new ChannelSettings { Id = d.Id, Label = d.Label, Source = d.Source, Decimals = d.Decimals, Unit = d.Unit };
        }
    }

    /// <summary>Un avviso a schermo: box/limitatore, cambio valori o fine giro.</summary>
    public class AlertSettings
    {
        public static readonly string[] Kinds = { "Pit", "Damage", "Flag", "Change", "Lap" };
        public const int MaxValues = 4;

        public string Id { get; set; }
        public bool Enabled { get; set; }
        public int Size { get; set; }          // 0 pieno schermo, 1 meta', 2 piccolo
        public int Position { get; set; }      // 0 centro, 1 alto, 2 basso
        public string Background { get; set; }
        public int Opacity { get; set; }       // percentuale sfondo
        public string TextColor { get; set; }
        public double Seconds { get; set; }
        public bool PitLaneToo { get; set; }   // solo Pit: anche in corsia box senza limitatore
        public bool UseFlagColor { get; set; } // solo Flag: sfondo del colore della bandiera
        public bool FlagAsPopup { get; set; }  // solo Flag: false = nel riquadro in alto a destra, true = avviso a schermo
        public bool SimpleLayout { get; set; } // solo Pit: false = schermata box completa (avversari, tempo in corsia)
        public string Title { get; set; }
        public List<string> Channels { get; set; } = new List<string>();

        public static AlertSettings Default(string id)
        {
            switch (id)
            {
                case "Pit":
                    return new AlertSettings { Id = id, Enabled = true, Size = 0, Position = 0, Background = "#FF0000FF", Opacity = 100,
                        TextColor = "#FFFFFFFF", Seconds = 0, PitLaneToo = false, Title = "PIT LIMITER", Channels = new List<string> { "Speed" } };
                case "Damage":
                    return new AlertSettings { Id = id, Enabled = true, Size = 1, Position = 0, Background = "#FF000000", Opacity = 90,
                        TextColor = "#FFFFFFFF", Seconds = 4, Title = "DANNI", Channels = new List<string>() };
                case "Flag":
                    return new AlertSettings { Id = id, Enabled = true, Size = 2, Position = 1, Background = "#FF000000", Opacity = 100,
                        TextColor = "#FFFFFFFF", Seconds = 3, Title = "BANDIERA", UseFlagColor = true, Channels = new List<string>() };
                case "Change":
                    return new AlertSettings { Id = id, Enabled = true, Size = 1, Position = 0, Background = "#FF000000", Opacity = 90,
                        TextColor = "#FFFFFFFF", Seconds = 1.5, Title = "",
                        Channels = new List<string> { "TC", "TCCut", "ABS", "BB", "Map" } };
                default:
                    return new AlertSettings { Id = "Lap", Enabled = true, Size = 1, Position = 0, Background = "#FF000000", Opacity = 90,
                        TextColor = "#FFFFFFFF", Seconds = 4, Title = "GIRO",
                        Channels = new List<string> { "LastLap", "LastLapDiff", "FuelLastLap", "FuelLaps" } };
            }
        }

        public void Normalize()
        {
            var d = Default(Id);
            if (Size < 0 || Size > 2) Size = d.Size;
            if (Position < 0 || Position > 2) Position = d.Position;
            if (string.IsNullOrWhiteSpace(Background)) Background = d.Background;
            if (string.IsNullOrWhiteSpace(TextColor)) TextColor = d.TextColor;
            if (Opacity <= 0 || Opacity > 100) Opacity = d.Opacity;
            if (Seconds < 0 || Seconds > 60) Seconds = d.Seconds;
            if (Title == null) Title = d.Title;
            if (Channels == null) Channels = new List<string>(d.Channels);
            Channels.RemoveAll(c => string.IsNullOrEmpty(c));
            if (Id != "Change" && Channels.Count > MaxValues) Channels.RemoveRange(MaxValues, Channels.Count - MaxValues);
        }

        /// <summary>Indice della variante grafica nella dashboard (0 pieno, 1-3 meta', 4-6 piccolo).</summary>
        public int Variant
        {
            get { return Size == 0 ? 0 : (Size == 1 ? 1 + Position : 4 + Position); }
        }
    }

    /// <summary>Gomme e freni: da dove leggere la temperatura gomme e soglie dei colori.</summary>
    public class TyreSettings
    {
        public static readonly string[] Sources = { "SimHub (standard)", "Superficie (media)", "Superficie (centro)", "Strato interno (media)", "Carcassa" };
        public static readonly string[] PressureUnits = { "kPa", "psi", "bar" };

        public int TempSource { get; set; } = 3;
        public int PressureUnit { get; set; }
        public double TempOffset { get; set; }
        public double TyreCold { get; set; } = 60;
        public double TyreOptimal { get; set; } = 85;
        public double TyreHot { get; set; } = 110;
        public double BrakeCold { get; set; } = 150;
        public double BrakeOptimal { get; set; } = 450;
        public double BrakeHot { get; set; } = 850;
        public double PressLow { get; set; } = 155;
        public double PressOptimal { get; set; } = 170;
        public double PressHigh { get; set; } = 185;
        public bool Initialized { get; set; }

        /// <summary>kPa -> unita' scelta.</summary>
        public double FromKpa(double kpa)
        {
            switch (PressureUnit) { case 1: return kpa / 6.894757; case 2: return kpa / 100.0; default: return kpa; }
        }

        /// <summary>Unita' scelta -> kPa.</summary>
        public double ToKpa(double v)
        {
            switch (PressureUnit) { case 1: return v * 6.894757; case 2: return v * 100.0; default: return v; }
        }

        public void Normalize()
        {
            if (!Initialized)
            {
                // file vecchio senza questa sezione (letto senza costruttore): valori di partenza
                var d = new TyreSettings();
                TempSource = d.TempSource; TempOffset = 0; PressureUnit = 0;
                TyreCold = d.TyreCold; TyreOptimal = d.TyreOptimal; TyreHot = d.TyreHot;
                BrakeCold = d.BrakeCold; BrakeOptimal = d.BrakeOptimal; BrakeHot = d.BrakeHot;
                PressLow = d.PressLow; PressOptimal = d.PressOptimal; PressHigh = d.PressHigh;
                Initialized = true;
            }
            if (TempSource < 0 || TempSource >= Sources.Length) TempSource = 3;
            if (PressureUnit < 0 || PressureUnit >= PressureUnits.Length) PressureUnit = 0;
        }
    }

    /// <summary>Schermata di attesa (prima del gioco): numero, nome, sfondo.</summary>
    public class IdleSettings
    {
        public static readonly string[] Modes = { "Colore / sfumatura", "Immagine", "Animato" };
        public static readonly string[] Gradients = { "Nessuna", "Verticale", "Orizzontale", "Dal centro" };
        public static readonly string[] Animations = { "Scie", "Respiro" };

        public bool NumberEnabled { get; set; } = true;
        public string Number { get; set; } = "80";
        public string NumberColor { get; set; } = "#FFFFFFFF";
        public bool NameEnabled { get; set; } = true;
        public string Name { get; set; } = "Federico Pegoraro";
        public string NameColor { get; set; } = "#FFFFFFFF";
        public int Mode { get; set; } = 2;
        public string Color1 { get; set; } = "#FF000000";
        public string Color2 { get; set; } = "#FF00E676";
        public int Gradient { get; set; } = 2;
        public int Animation { get; set; }
        public string ImagePath { get; set; } = "";
        public bool ShowClock { get; set; } = true;
        public bool ShowWaiting { get; set; } = true;
        // stemma: cerchio colorato con bordo, logo e nome dentro
        public bool BadgeEnabled { get; set; }
        public string BadgeFill { get; set; } = "#FF1A1A1A";
        public string BadgeBorder { get; set; } = "#FFFF8C00";
        public double BadgeBorderWidth { get; set; } = 8;
        public double BadgeSize { get; set; } = 250;
        public string BadgeLogoPath { get; set; } = "";
        public double BadgeLogoAspect { get; set; } = 2;
        public string BadgeText { get; set; } = "";
        public string BadgeTextColor { get; set; } = "#FFFFFFFF";
        public bool BadgeShowNumber { get; set; }
        // schermo "motore spento" (in gioco, motore fermo): attivo se EngineOffDisabled = false
        public bool EngineOffDisabled { get; set; }
        public int EngineOffStyle { get; set; }      // 0 = riquadro della marcia giallo, 1 = schermo nero
        public string EngineOffText { get; set; }
        public string EngineOffColor { get; set; }
        public bool Initialized { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(EngineOffText)) EngineOffText = "MOTORE SPENTO";
            if (string.IsNullOrEmpty(EngineOffColor)) EngineOffColor = "#FF2E2E2E";
            if (!Initialized)
            {
                var d = new IdleSettings();
                NumberEnabled = d.NumberEnabled; Number = d.Number; NumberColor = d.NumberColor;
                NameEnabled = d.NameEnabled; Name = d.Name; NameColor = d.NameColor;
                Mode = d.Mode; Color1 = d.Color1; Color2 = d.Color2; Gradient = d.Gradient; Animation = d.Animation;
                ImagePath = ""; ShowClock = true; ShowWaiting = true;
                BadgeEnabled = false; BadgeSize = 0; BadgeBorderWidth = 8; BadgeLogoAspect = 2; BadgeText = ""; BadgeLogoPath = "";
                Initialized = true;
            }
            if (Mode < 0 || Mode >= Modes.Length) Mode = 0;
            if (Gradient < 0 || Gradient >= Gradients.Length) Gradient = 0;
            if (Animation < 0 || Animation >= Animations.Length) Animation = 0;
            if (Number == null) Number = "";
            if (Name == null) Name = "";
            if (ImagePath == null) ImagePath = "";
            if (string.IsNullOrWhiteSpace(Color1)) Color1 = "#FF000000";
            if (string.IsNullOrWhiteSpace(Color2)) Color2 = "#FF00E676";
            if (string.IsNullOrWhiteSpace(NumberColor)) NumberColor = "#FFFFFFFF";
            if (string.IsNullOrWhiteSpace(NameColor)) NameColor = "#FFFFFFFF";
            // campi dello stemma assenti nei file salvati prima di questa versione
            if (string.IsNullOrWhiteSpace(BadgeFill)) BadgeFill = "#FF1A1A1A";
            if (string.IsNullOrWhiteSpace(BadgeBorder)) BadgeBorder = "#FFFF8C00";
            if (string.IsNullOrWhiteSpace(BadgeTextColor)) BadgeTextColor = "#FFFFFFFF";
            if (BadgeSize < 80 || BadgeSize > 470) BadgeSize = 250;
            if (BadgeBorderWidth < 0 || BadgeBorderWidth > 40) BadgeBorderWidth = 8;
            if (BadgeLogoAspect <= 0.1 || BadgeLogoAspect > 10) BadgeLogoAspect = 2;
            if (BadgeLogoPath == null) BadgeLogoPath = "";
            if (BadgeText == null) BadgeText = "";
        }
    }

    /// <summary>Opzioni tempi e posizioni.</summary>
    public class TimingSettings
    {
        public static readonly string[] DeltaSources = { "Gioco", "SimHub", "Post-It Note Racing" };
        public static readonly string[] PositionModes = { "Assoluta", "Nella classe", "Post-It Note Racing (classe)" };
        public static readonly string[] GapModes = { "Assoluti", "Nella classe" };

        public int DeltaSource { get; set; } = 1;
        public int PositionMode { get; set; } = 1;
        public int GapMode { get; set; }
        public bool Initialized { get; set; }

        public void Normalize()
        {
            if (!Initialized) { DeltaSource = 1; PositionMode = 1; GapMode = 0; Initialized = true; }
            if (DeltaSource < 0 || DeltaSource >= DeltaSources.Length) DeltaSource = 1;
            if (PositionMode < 0 || PositionMode >= PositionModes.Length) PositionMode = 0;
            if (GapMode < 0 || GapMode >= GapModes.Length) GapMode = 0;
        }
    }

    /// <summary>Carattere dei testi della dashboard (applicato riscrivendo i file della dash).</summary>
    public class FontSettings
    {
        public static readonly string[] Families = { "Roboto", "Roboto Condensed", "Bahnschrift", "Segoe UI", "Arial", "DT Montserrat" };
        public static readonly string[] FamilyNames = { "Roboto (originale)", "Roboto Condensed (stretto)", "Bahnschrift (Windows)", "Segoe UI (Windows)", "Arial", "DT Montserrat (marcato, nitido)" };
        public static readonly string[] Weights = { "Come in origine", "Un po' più leggero", "Leggero", "Più marcato" };
        public static readonly int[] WeightSteps = { 0, -1, -2, 1 };
        public static readonly int[] Sizes = { 100, 95, 90, 85, 80, 110 };

        public int Family { get; set; }
        public int Weight { get; set; }
        public int SizePct { get; set; } = 100;
        /// <summary>Spessore del bordo (px) dei riquadri centrali: marcia, velocita', righe centrali. 0 = nessun bordo.</summary>
        public int CenterBorder { get; set; } = 0;
        /// <summary>Spessore del bordo (px) delle schede ai lati e in basso.</summary>
        public int CardBorder { get; set; } = 2;
        public static readonly int[] BorderSizes = { 0, 1, 2, 3, 4 };

        public void Normalize()
        {
            if (CenterBorder < 0 || CenterBorder > 6) CenterBorder = 0;
            if (CardBorder < 0 || CardBorder > 6) CardBorder = 2;
            if (Family < 0 || Family >= Families.Length) Family = 0;
            if (Weight < 0 || Weight >= Weights.Length) Weight = 0;
            if (SizePct < 70 || SizePct > 120) SizePct = 100;
        }

        public bool IsDefault { get { return Family == 0 && Weight == 0 && SizePct == 100 && CenterBorder == 0 && CardBorder == 2; } }
        public string Key { get { return Family + "/" + Weight + "/" + SizePct + "/" + CenterBorder + "/" + CardBorder; } }
    }

    /// <summary>Codice di accesso di questo PC (solo l'impronta) e ultima copia dell'elenco dei codici.</summary>
    public class AccessSettings
    {
        public string Hash { get; set; } = "";
        public string Salt { get; set; } = "";
        public string CachedList { get; set; } = "";
    }
}
