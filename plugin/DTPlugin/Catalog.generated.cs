// FILE GENERATO AUTOMATICAMENTE da gen_cs.py - non modificare a mano.
// Deve restare allineato alla dashboard: gli indici delle schede = indici delle schermate nei file Schede_*.djson
using System.Collections.Generic;

namespace FedeDashboard
{
    public static class Catalog
    {
        public const int MaxSideSlots = 8;
        public const int MaxToolSlots = 7;
        public const int CenterRows = 3;
        public const int StandingsMax = 60;
        public const int DashWidth = 850;
        public const int DashHeight = 480;

        public static readonly ChannelDef[] Channels = new ChannelDef[]
        {
            new ChannelDef("Gear", "MARCIA", "DataCorePlugin.GameData.Gear", "text", 0, ""),
            new ChannelDef("Rpm", "GIRI", "DataCorePlugin.GameData.Rpms", "num", 0, ""),
            new ChannelDef("RpmPct", "GIRI %", "DataCorePlugin.GameData.CarSettings_CurrentDisplayedRPMPercent", "num", 0, ""),
            new ChannelDef("Speed", "VELOCITÀ", "DataCorePlugin.GameData.SpeedLocal", "num", 0, ""),
            new ChannelDef("Throttle", "GAS", "DataCorePlugin.GameData.Throttle", "num", 0, ""),
            new ChannelDef("Brake", "FRENO", "DataCorePlugin.GameData.Brake", "num", 0, ""),
            new ChannelDef("Clutch", "FRIZIONE", "DataCorePlugin.GameData.Clutch", "num", 0, ""),
            new ChannelDef("TC", "TC", "DataCorePlugin.GameData.TCLevel", "num", 0, ""),
            new ChannelDef("TCCut", "TC CUT", "@raw:PlayerNativeTelemetry.mTCCut", "num", 0, ""),
            new ChannelDef("ABS", "ABS", "DataCorePlugin.GameData.ABSLevel", "num", 0, ""),
            new ChannelDef("BB", "BB", "DataCorePlugin.GameData.BrakeBias", "num", 1, ""),
            new ChannelDef("Map", "MAP", "DataCorePlugin.GameData.EngineMap", "num", 0, ""),
            new ChannelDef("Fuel", "LITRI", "DataCorePlugin.GameData.Fuel", "num", 1, ""),
            new ChannelDef("FuelPct", "CARB. %", "DataCorePlugin.GameData.FuelPercent", "num", 0, "%"),
            new ChannelDef("FuelPerLap", "MEDIA", "DataCorePlugin.Computed.Fuel_LitersPerLap", "num", 2, ""),
            new ChannelDef("FuelLaps", "GIRI CARB.", "@giri_carb", "num", 1, ""),
            new ChannelDef("Pos", "POS", "@pos", "num", 0, ""),
            new ChannelDef("Cars", "AUTO", "@auto", "num", 0, ""),
            new ChannelDef("Lap", "GIRO", "DataCorePlugin.GameData.CurrentLap", "num", 0, ""),
            new ChannelDef("TotalLaps", "GIRI TOT.", "DataCorePlugin.GameData.TotalLaps", "num", 0, ""),
            new ChannelDef("TimeLeft", "TEMPO", "DataCorePlugin.GameData.SessionTimeLeft", "duration", 0, ""),
            new ChannelDef("Clock", "ORA", "@ora", "clock", 0, ""),
            new ChannelDef("LastLap", "ULTIMO", "DataCorePlugin.GameData.LastLapTime", "laptime", 0, ""),
            new ChannelDef("BestLap", "MIGLIORE", "DataCorePlugin.GameData.BestLapTime", "laptime", 0, ""),
            new ChannelDef("EstLap", "STIMATO", "@stimato", "laptime", 0, ""),
            new ChannelDef("Delta", "DELTA", "@delta", "signed", 3, ""),
            new ChannelDef("GapAhead", "AVANTI", "@gap_avanti", "gap", 1, ""),
            new ChannelDef("GapBehind", "DIETRO", "@gap_dietro", "gap", 1, ""),
            new ChannelDef("AirTemp", "ARIA", "DataCorePlugin.GameData.AirTemperature", "num", 0, "°"),
            new ChannelDef("TrackTemp", "PISTA", "DataCorePlugin.GameData.RoadTemperature", "num", 0, "°"),
            new ChannelDef("WaterTemp", "ACQUA", "DataCorePlugin.GameData.WaterTemperature", "num", 0, "°"),
            new ChannelDef("OilTemp", "OLIO", "DataCorePlugin.GameData.OilTemperature", "num", 0, "°"),
            new ChannelDef("ERS", "ERS", "@ers", "num", 0, "%"),
            new ChannelDef("TyrePFL", "PRESS. AS", "@press_FL", "num", 1, ""),
            new ChannelDef("TyrePFR", "PRESS. AD", "@press_FR", "num", 1, ""),
            new ChannelDef("TyrePRL", "PRESS. PS", "@press_RL", "num", 1, ""),
            new ChannelDef("TyrePRR", "PRESS. PD", "@press_RR", "num", 1, ""),
            new ChannelDef("TyreTFL", "TEMP. AS", "@gomma_FL", "num", 0, "°"),
            new ChannelDef("TyreTFR", "TEMP. AD", "@gomma_FR", "num", 0, "°"),
            new ChannelDef("TyreTRL", "TEMP. PS", "@gomma_RL", "num", 0, "°"),
            new ChannelDef("TyreTRR", "TEMP. PD", "@gomma_RR", "num", 0, "°"),
            new ChannelDef("TyreWFL", "USURA AS", "DataCorePlugin.GameData.TyreWearFrontLeft", "num", 0, "%"),
            new ChannelDef("TyreWFR", "USURA AD", "DataCorePlugin.GameData.TyreWearFrontRight", "num", 0, "%"),
            new ChannelDef("TyreWRL", "USURA PS", "DataCorePlugin.GameData.TyreWearRearLeft", "num", 0, "%"),
            new ChannelDef("TyreWRR", "USURA PD", "DataCorePlugin.GameData.TyreWearRearRight", "num", 0, "%"),
            new ChannelDef("BrakeTFL", "FRENO AS", "DataCorePlugin.GameData.BrakeTemperatureFrontLeft", "num", 0, "°"),
            new ChannelDef("BrakeTFR", "FRENO AD", "DataCorePlugin.GameData.BrakeTemperatureFrontRight", "num", 0, "°"),
            new ChannelDef("BrakeTRL", "FRENO PS", "DataCorePlugin.GameData.BrakeTemperatureRearLeft", "num", 0, "°"),
            new ChannelDef("BrakeTRR", "FRENO PD", "DataCorePlugin.GameData.BrakeTemperatureRearRight", "num", 0, "°"),
            new ChannelDef("FlagYellow", "GIALLA", "DataCorePlugin.GameData.Flag_Yellow", "num", 0, ""),
            new ChannelDef("FlagBlue", "BLU", "DataCorePlugin.GameData.Flag_Blue", "num", 0, ""),
            new ChannelDef("PitLimiter", "LIMITATORE", "@limitatore", "num", 0, ""),
            new ChannelDef("InPit", "CORSIA BOX", "DataCorePlugin.GameData.IsInPitLane", "num", 0, ""),
            new ChannelDef("LastLapDiff", "DIFF. GIRO", "@delta_ultimo", "signed", 3, ""),
            new ChannelDef("FuelLastLap", "CONSUMO GIRO", "@consumo_ultimo", "num", 2, ""),
            new ChannelDef("VE", "ENERGIA V.", "@ve", "num", 1, "%"),
            new ChannelDef("VELaps", "GIRI VE", "@ve_giri", "num", 1, ""),
            new ChannelDef("VEPerLap", "VE / GIRO", "@ve_media", "num", 2, "%"),
            new ChannelDef("VELastLap", "VE ULT. GIRO", "@ve_ultimo", "num", 2, "%"),
            new ChannelDef("TCSlip", "TC SLIP", "@raw:PlayerNativeTelemetry.mTCSlip", "num", 0, ""),
            new ChannelDef("TrackWet", "PISTA BAGN.", "@bagnato", "num", 0, "%"),
            new ChannelDef("Rain", "PIOGGIA", "@pioggia", "num", 0, "%"),
            new ChannelDef("RepDamage", "RIPARAZIONE", "@rip_danni", "num", 1, "s"),
            new ChannelDef("RepBrakes", "FRENI", "@rip_freni", "num", 1, "s"),
            new ChannelDef("RepTyres", "GOMME", "@rip_gomme", "num", 1, "s"),
            new ChannelDef("RepFuel", "RIFORNIMENTO", "@rip_carb", "num", 1, "s"),
            new ChannelDef("RepTotal", "SOSTA", "@rip_totale", "num", 1, "s"),
            new ChannelDef("SuspFL", "SOSP. AS", "@sosp_FL", "num", 0, "%"),
            new ChannelDef("SuspFR", "SOSP. AD", "@sosp_FR", "num", 0, "%"),
            new ChannelDef("SuspRL", "SOSP. PS", "@sosp_RL", "num", 0, "%"),
            new ChannelDef("SuspRR", "SOSP. PD", "@sosp_RR", "num", 0, "%"),
            new ChannelDef("BrakeWFL", "FRENO AS", "@usura_freno_FL", "num", 0, "%"),
            new ChannelDef("BrakeWFR", "FRENO AD", "@usura_freno_FR", "num", 0, "%"),
            new ChannelDef("BrakeWRL", "FRENO PS", "@usura_freno_RL", "num", 0, "%"),
            new ChannelDef("BrakeWRR", "FRENO PD", "@usura_freno_RR", "num", 0, "%"),
            new ChannelDef("AeroDmg", "AERO", "@danno_aero", "num", 0, "%"),
            new ChannelDef("EngineDmg", "MOTORE", "@danno_motore", "num", 0, "%"),
            new ChannelDef("NewTyres", "GOMME NUOVE", "@gomme_nuove", "num", 0, ""),
            new ChannelDef("TrackLim", "TRACK LIMITS", "@track_limits", "text", 0, ""),
            new ChannelDef("Battery", "BATTERIA", "@batteria", "num", 0, "%"),
            new ChannelDef("Sky", "CIELO", "@cielo", "text", 0, ""),
            new ChannelDef("Custom1", "PERS. 1", "", "num", 1, ""),
            new ChannelDef("Custom2", "PERS. 2", "", "num", 1, ""),
            new ChannelDef("Custom3", "PERS. 3", "", "num", 1, ""),
            new ChannelDef("Custom4", "PERS. 4", "", "num", 1, ""),
            new ChannelDef("Custom5", "PERS. 5", "", "num", 1, ""),
            new ChannelDef("Custom6", "PERS. 6", "", "num", 1, ""),
        };

        public static readonly string[] Cards = new string[] { "Vuoto", "Gomme (pressione + temperatura)", "Gomme (temperature)", "Usura gomme", "Temperature freni", "Tempi giro", "Carburante", "Sessione", "Distacchi", "Pedali", "Meteo", "Motore (acqua/olio)", "Assetto (TC/ABS/BB/MAP)", "Delta", "TC", "TC Cut", "ABS", "Ripartizione freno (BB)", "Mappa motore", "Posizione", "Giro", "Tempo rimasto", "Ultimo giro", "Miglior giro", "Giro stimato", "Velocità", "Giri motore", "Carburante (litri)", "Carburante (giri)", "Consumo medio", "ERS / batteria", "Ora", "Distacco avanti", "Distacco dietro", "Temperatura aria", "Temperatura pista", "Personalizzato 1", "Personalizzato 2", "Personalizzato 3", "Personalizzato 4", "Personalizzato 5", "Personalizzato 6", "Gomme complete (temp. carcassa, pressione, freni)", "Energia virtuale (VE)", "Giri energia virtuale", "Consumo energia virtuale", "Energia virtuale (completa)", "TC Slip", "Danni (schema auto)", "Stato auto (luci, bandiera, tergi, motore)", "Carburante (barra + giri)", "Energia virtuale (barra + giri)", "Strategia gara (soste, finestra, rifornimento)", "Classifica generale", "Classifica di classe", "Sosta (tempi stimati da LMU)", "Tempo sosta totale", "Danni sospensioni", "Danni (aero e motore)", "Usura freni", "Gomme nuove rimaste", "Track limits", "Batteria", "Previsioni meteo" };
        public static readonly string[] Bars = new string[] { "Vuoto", "Giri motore", "Gas", "Freno", "Frizione", "Carburante", "ERS / batteria" };
        public static readonly string[] Frames = new string[] { "Completo (barre laterali + barra inferiore)", "Solo barra inferiore", "Solo barre laterali", "Pulito (solo riquadri)", "Completo con barra doppia", "Barra doppia senza barre laterali" };
        public static readonly bool[] FrameHasSidebars = new bool[] { true, false, true, false, true, false };
        public static readonly bool[] FrameHasToolbar = new bool[] { true, true, false, false, true, true };
        public static readonly string[] Grids = new string[] { "Riga + grande", "2 grandi", "Grande + 2 medi", "4 medi", "Grande + 4 piccoli", "4 righe", "8 piccoli", "2 righe + grande", "Grande + 2 righe", "Grande + riga", "Unico" };
        public static readonly string[] Categories = new string[] { "Tempi", "Carburante ed energia", "Comandi auto", "Gomme e freni", "Sessione e gara", "Motore e pedali", "Meteo", "Sosta e danni (LMU)", "Personalizzati" };
        // CategoryCards[categoria] = indici delle schede
        public static readonly int[][] CategoryCards = new int[][] { new[] { 5, 13, 22, 23, 24 }, new[] { 6, 27, 28, 29, 43, 46, 44, 45, 30, 50, 51, 52 }, new[] { 12, 14, 15, 47, 16, 17, 18 }, new[] { 42, 1, 2, 3, 4, 48 }, new[] { 7, 19, 20, 21, 31, 8, 32, 33, 53, 54, 49 }, new[] { 9, 25, 26, 11 }, new[] { 10, 34, 35, 63 }, new[] { 55, 56, 57, 58, 59, 60, 61, 62 }, new[] { 36, 37, 38, 39, 40, 41 } };
        // GridUnits[griglia] = celle in unita' 2 colonne x 4 righe {col, riga, larghezza, altezza}
        public static readonly int[][][] GridUnits = new int[][][] { new int[][] { new[] { 0, 0, 2, 1 }, new[] { 0, 1, 2, 3 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 2, 2 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 1, 2 }, new[] { 1, 2, 1, 2 } }, new int[][] { new[] { 0, 0, 1, 2 }, new[] { 1, 0, 1, 2 }, new[] { 0, 2, 1, 2 }, new[] { 1, 2, 1, 2 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 1, 1 }, new[] { 1, 2, 1, 1 }, new[] { 0, 3, 1, 1 }, new[] { 1, 3, 1, 1 } }, new int[][] { new[] { 0, 0, 2, 1 }, new[] { 0, 1, 2, 1 }, new[] { 0, 2, 2, 1 }, new[] { 0, 3, 2, 1 } }, new int[][] { new[] { 0, 0, 1, 1 }, new[] { 1, 0, 1, 1 }, new[] { 0, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, new[] { 0, 2, 1, 1 }, new[] { 1, 2, 1, 1 }, new[] { 0, 3, 1, 1 }, new[] { 1, 3, 1, 1 } }, new int[][] { new[] { 0, 0, 2, 1 }, new[] { 0, 1, 2, 1 }, new[] { 0, 2, 2, 2 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 2, 1 }, new[] { 0, 3, 2, 1 } }, new int[][] { new[] { 0, 0, 2, 3 }, new[] { 0, 3, 2, 1 } }, new int[][] { new[] { 0, 0, 2, 4 } } };
        public static readonly string[] Toolbars = new string[] { "5 riquadri", "6 riquadri", "4 riquadri", "3 riquadri", "1 riquadro largo" };

        // Geometria (pixel della dashboard 850x480) per l'anteprima nelle impostazioni
        // GridRects[cornice][griglia][lato 0=sx 1=dx] -> rettangoli {x, y, w, h}
        public static readonly int[][][][][] GridRects = new int[][][][][]
        {
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 5, 302, 92 }, new[] { 31, 103, 302, 288 } }, new int[][] { new[] { 517, 5, 302, 92 }, new[] { 517, 103, 302, 288 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 190 }, new[] { 31, 201, 302, 190 } }, new int[][] { new[] { 517, 5, 302, 190 }, new[] { 517, 201, 302, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 190 }, new[] { 31, 201, 148, 190 }, new[] { 185, 201, 148, 190 } }, new int[][] { new[] { 517, 5, 302, 190 }, new[] { 517, 201, 148, 190 }, new[] { 671, 201, 148, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 148, 190 }, new[] { 185, 5, 148, 190 }, new[] { 31, 201, 148, 190 }, new[] { 185, 201, 148, 190 } }, new int[][] { new[] { 517, 5, 148, 190 }, new[] { 671, 5, 148, 190 }, new[] { 517, 201, 148, 190 }, new[] { 671, 201, 148, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 190 }, new[] { 31, 201, 148, 92 }, new[] { 185, 201, 148, 92 }, new[] { 31, 299, 148, 92 }, new[] { 185, 299, 148, 92 } }, new int[][] { new[] { 517, 5, 302, 190 }, new[] { 517, 201, 148, 92 }, new[] { 671, 201, 148, 92 }, new[] { 517, 299, 148, 92 }, new[] { 671, 299, 148, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 92 }, new[] { 31, 103, 302, 92 }, new[] { 31, 201, 302, 92 }, new[] { 31, 299, 302, 92 } }, new int[][] { new[] { 517, 5, 302, 92 }, new[] { 517, 103, 302, 92 }, new[] { 517, 201, 302, 92 }, new[] { 517, 299, 302, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 148, 92 }, new[] { 185, 5, 148, 92 }, new[] { 31, 103, 148, 92 }, new[] { 185, 103, 148, 92 }, new[] { 31, 201, 148, 92 }, new[] { 185, 201, 148, 92 }, new[] { 31, 299, 148, 92 }, new[] { 185, 299, 148, 92 } }, new int[][] { new[] { 517, 5, 148, 92 }, new[] { 671, 5, 148, 92 }, new[] { 517, 103, 148, 92 }, new[] { 671, 103, 148, 92 }, new[] { 517, 201, 148, 92 }, new[] { 671, 201, 148, 92 }, new[] { 517, 299, 148, 92 }, new[] { 671, 299, 148, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 92 }, new[] { 31, 103, 302, 92 }, new[] { 31, 201, 302, 190 } }, new int[][] { new[] { 517, 5, 302, 92 }, new[] { 517, 103, 302, 92 }, new[] { 517, 201, 302, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 190 }, new[] { 31, 201, 302, 92 }, new[] { 31, 299, 302, 92 } }, new int[][] { new[] { 517, 5, 302, 190 }, new[] { 517, 201, 302, 92 }, new[] { 517, 299, 302, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 288 }, new[] { 31, 299, 302, 92 } }, new int[][] { new[] { 517, 5, 302, 288 }, new[] { 517, 299, 302, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 386 } }, new int[][] { new[] { 517, 5, 302, 386 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 5, 328, 92 }, new[] { 5, 103, 328, 288 } }, new int[][] { new[] { 517, 5, 328, 92 }, new[] { 517, 103, 328, 288 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 190 }, new[] { 5, 201, 328, 190 } }, new int[][] { new[] { 517, 5, 328, 190 }, new[] { 517, 201, 328, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 190 }, new[] { 5, 201, 161, 190 }, new[] { 172, 201, 161, 190 } }, new int[][] { new[] { 517, 5, 328, 190 }, new[] { 517, 201, 161, 190 }, new[] { 684, 201, 161, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 161, 190 }, new[] { 172, 5, 161, 190 }, new[] { 5, 201, 161, 190 }, new[] { 172, 201, 161, 190 } }, new int[][] { new[] { 517, 5, 161, 190 }, new[] { 684, 5, 161, 190 }, new[] { 517, 201, 161, 190 }, new[] { 684, 201, 161, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 190 }, new[] { 5, 201, 161, 92 }, new[] { 172, 201, 161, 92 }, new[] { 5, 299, 161, 92 }, new[] { 172, 299, 161, 92 } }, new int[][] { new[] { 517, 5, 328, 190 }, new[] { 517, 201, 161, 92 }, new[] { 684, 201, 161, 92 }, new[] { 517, 299, 161, 92 }, new[] { 684, 299, 161, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 92 }, new[] { 5, 103, 328, 92 }, new[] { 5, 201, 328, 92 }, new[] { 5, 299, 328, 92 } }, new int[][] { new[] { 517, 5, 328, 92 }, new[] { 517, 103, 328, 92 }, new[] { 517, 201, 328, 92 }, new[] { 517, 299, 328, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 161, 92 }, new[] { 172, 5, 161, 92 }, new[] { 5, 103, 161, 92 }, new[] { 172, 103, 161, 92 }, new[] { 5, 201, 161, 92 }, new[] { 172, 201, 161, 92 }, new[] { 5, 299, 161, 92 }, new[] { 172, 299, 161, 92 } }, new int[][] { new[] { 517, 5, 161, 92 }, new[] { 684, 5, 161, 92 }, new[] { 517, 103, 161, 92 }, new[] { 684, 103, 161, 92 }, new[] { 517, 201, 161, 92 }, new[] { 684, 201, 161, 92 }, new[] { 517, 299, 161, 92 }, new[] { 684, 299, 161, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 92 }, new[] { 5, 103, 328, 92 }, new[] { 5, 201, 328, 190 } }, new int[][] { new[] { 517, 5, 328, 92 }, new[] { 517, 103, 328, 92 }, new[] { 517, 201, 328, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 190 }, new[] { 5, 201, 328, 92 }, new[] { 5, 299, 328, 92 } }, new int[][] { new[] { 517, 5, 328, 190 }, new[] { 517, 201, 328, 92 }, new[] { 517, 299, 328, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 288 }, new[] { 5, 299, 328, 92 } }, new int[][] { new[] { 517, 5, 328, 288 }, new[] { 517, 299, 328, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 386 } }, new int[][] { new[] { 517, 5, 328, 386 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 5, 302, 113 }, new[] { 31, 124, 302, 351 } }, new int[][] { new[] { 517, 5, 302, 113 }, new[] { 517, 124, 302, 351 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 232 }, new[] { 31, 243, 302, 232 } }, new int[][] { new[] { 517, 5, 302, 232 }, new[] { 517, 243, 302, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 232 }, new[] { 31, 243, 148, 232 }, new[] { 185, 243, 148, 232 } }, new int[][] { new[] { 517, 5, 302, 232 }, new[] { 517, 243, 148, 232 }, new[] { 671, 243, 148, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 148, 232 }, new[] { 185, 5, 148, 232 }, new[] { 31, 243, 148, 232 }, new[] { 185, 243, 148, 232 } }, new int[][] { new[] { 517, 5, 148, 232 }, new[] { 671, 5, 148, 232 }, new[] { 517, 243, 148, 232 }, new[] { 671, 243, 148, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 232 }, new[] { 31, 243, 148, 113 }, new[] { 185, 243, 148, 113 }, new[] { 31, 362, 148, 113 }, new[] { 185, 362, 148, 113 } }, new int[][] { new[] { 517, 5, 302, 232 }, new[] { 517, 243, 148, 113 }, new[] { 671, 243, 148, 113 }, new[] { 517, 362, 148, 113 }, new[] { 671, 362, 148, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 113 }, new[] { 31, 124, 302, 113 }, new[] { 31, 243, 302, 113 }, new[] { 31, 362, 302, 113 } }, new int[][] { new[] { 517, 5, 302, 113 }, new[] { 517, 124, 302, 113 }, new[] { 517, 243, 302, 113 }, new[] { 517, 362, 302, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 148, 113 }, new[] { 185, 5, 148, 113 }, new[] { 31, 124, 148, 113 }, new[] { 185, 124, 148, 113 }, new[] { 31, 243, 148, 113 }, new[] { 185, 243, 148, 113 }, new[] { 31, 362, 148, 113 }, new[] { 185, 362, 148, 113 } }, new int[][] { new[] { 517, 5, 148, 113 }, new[] { 671, 5, 148, 113 }, new[] { 517, 124, 148, 113 }, new[] { 671, 124, 148, 113 }, new[] { 517, 243, 148, 113 }, new[] { 671, 243, 148, 113 }, new[] { 517, 362, 148, 113 }, new[] { 671, 362, 148, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 113 }, new[] { 31, 124, 302, 113 }, new[] { 31, 243, 302, 232 } }, new int[][] { new[] { 517, 5, 302, 113 }, new[] { 517, 124, 302, 113 }, new[] { 517, 243, 302, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 232 }, new[] { 31, 243, 302, 113 }, new[] { 31, 362, 302, 113 } }, new int[][] { new[] { 517, 5, 302, 232 }, new[] { 517, 243, 302, 113 }, new[] { 517, 362, 302, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 351 }, new[] { 31, 362, 302, 113 } }, new int[][] { new[] { 517, 5, 302, 351 }, new[] { 517, 362, 302, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 470 } }, new int[][] { new[] { 517, 5, 302, 470 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 5, 328, 113 }, new[] { 5, 124, 328, 351 } }, new int[][] { new[] { 517, 5, 328, 113 }, new[] { 517, 124, 328, 351 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 232 }, new[] { 5, 243, 328, 232 } }, new int[][] { new[] { 517, 5, 328, 232 }, new[] { 517, 243, 328, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 232 }, new[] { 5, 243, 161, 232 }, new[] { 172, 243, 161, 232 } }, new int[][] { new[] { 517, 5, 328, 232 }, new[] { 517, 243, 161, 232 }, new[] { 684, 243, 161, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 161, 232 }, new[] { 172, 5, 161, 232 }, new[] { 5, 243, 161, 232 }, new[] { 172, 243, 161, 232 } }, new int[][] { new[] { 517, 5, 161, 232 }, new[] { 684, 5, 161, 232 }, new[] { 517, 243, 161, 232 }, new[] { 684, 243, 161, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 232 }, new[] { 5, 243, 161, 113 }, new[] { 172, 243, 161, 113 }, new[] { 5, 362, 161, 113 }, new[] { 172, 362, 161, 113 } }, new int[][] { new[] { 517, 5, 328, 232 }, new[] { 517, 243, 161, 113 }, new[] { 684, 243, 161, 113 }, new[] { 517, 362, 161, 113 }, new[] { 684, 362, 161, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 113 }, new[] { 5, 124, 328, 113 }, new[] { 5, 243, 328, 113 }, new[] { 5, 362, 328, 113 } }, new int[][] { new[] { 517, 5, 328, 113 }, new[] { 517, 124, 328, 113 }, new[] { 517, 243, 328, 113 }, new[] { 517, 362, 328, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 161, 113 }, new[] { 172, 5, 161, 113 }, new[] { 5, 124, 161, 113 }, new[] { 172, 124, 161, 113 }, new[] { 5, 243, 161, 113 }, new[] { 172, 243, 161, 113 }, new[] { 5, 362, 161, 113 }, new[] { 172, 362, 161, 113 } }, new int[][] { new[] { 517, 5, 161, 113 }, new[] { 684, 5, 161, 113 }, new[] { 517, 124, 161, 113 }, new[] { 684, 124, 161, 113 }, new[] { 517, 243, 161, 113 }, new[] { 684, 243, 161, 113 }, new[] { 517, 362, 161, 113 }, new[] { 684, 362, 161, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 113 }, new[] { 5, 124, 328, 113 }, new[] { 5, 243, 328, 232 } }, new int[][] { new[] { 517, 5, 328, 113 }, new[] { 517, 124, 328, 113 }, new[] { 517, 243, 328, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 232 }, new[] { 5, 243, 328, 113 }, new[] { 5, 362, 328, 113 } }, new int[][] { new[] { 517, 5, 328, 232 }, new[] { 517, 243, 328, 113 }, new[] { 517, 362, 328, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 351 }, new[] { 5, 362, 328, 113 } }, new int[][] { new[] { 517, 5, 328, 351 }, new[] { 517, 362, 328, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 470 } }, new int[][] { new[] { 517, 5, 328, 470 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 5, 302, 71 }, new[] { 31, 82, 302, 225 } }, new int[][] { new[] { 517, 5, 302, 71 }, new[] { 517, 82, 302, 225 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 148 }, new[] { 31, 159, 302, 148 } }, new int[][] { new[] { 517, 5, 302, 148 }, new[] { 517, 159, 302, 148 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 148 }, new[] { 31, 159, 148, 148 }, new[] { 185, 159, 148, 148 } }, new int[][] { new[] { 517, 5, 302, 148 }, new[] { 517, 159, 148, 148 }, new[] { 671, 159, 148, 148 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 148, 148 }, new[] { 185, 5, 148, 148 }, new[] { 31, 159, 148, 148 }, new[] { 185, 159, 148, 148 } }, new int[][] { new[] { 517, 5, 148, 148 }, new[] { 671, 5, 148, 148 }, new[] { 517, 159, 148, 148 }, new[] { 671, 159, 148, 148 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 148 }, new[] { 31, 159, 148, 71 }, new[] { 185, 159, 148, 71 }, new[] { 31, 236, 148, 71 }, new[] { 185, 236, 148, 71 } }, new int[][] { new[] { 517, 5, 302, 148 }, new[] { 517, 159, 148, 71 }, new[] { 671, 159, 148, 71 }, new[] { 517, 236, 148, 71 }, new[] { 671, 236, 148, 71 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 71 }, new[] { 31, 82, 302, 71 }, new[] { 31, 159, 302, 71 }, new[] { 31, 236, 302, 71 } }, new int[][] { new[] { 517, 5, 302, 71 }, new[] { 517, 82, 302, 71 }, new[] { 517, 159, 302, 71 }, new[] { 517, 236, 302, 71 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 148, 71 }, new[] { 185, 5, 148, 71 }, new[] { 31, 82, 148, 71 }, new[] { 185, 82, 148, 71 }, new[] { 31, 159, 148, 71 }, new[] { 185, 159, 148, 71 }, new[] { 31, 236, 148, 71 }, new[] { 185, 236, 148, 71 } }, new int[][] { new[] { 517, 5, 148, 71 }, new[] { 671, 5, 148, 71 }, new[] { 517, 82, 148, 71 }, new[] { 671, 82, 148, 71 }, new[] { 517, 159, 148, 71 }, new[] { 671, 159, 148, 71 }, new[] { 517, 236, 148, 71 }, new[] { 671, 236, 148, 71 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 71 }, new[] { 31, 82, 302, 71 }, new[] { 31, 159, 302, 148 } }, new int[][] { new[] { 517, 5, 302, 71 }, new[] { 517, 82, 302, 71 }, new[] { 517, 159, 302, 148 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 148 }, new[] { 31, 159, 302, 71 }, new[] { 31, 236, 302, 71 } }, new int[][] { new[] { 517, 5, 302, 148 }, new[] { 517, 159, 302, 71 }, new[] { 517, 236, 302, 71 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 225 }, new[] { 31, 236, 302, 71 } }, new int[][] { new[] { 517, 5, 302, 225 }, new[] { 517, 236, 302, 71 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 302, 302 } }, new int[][] { new[] { 517, 5, 302, 302 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 5, 328, 71 }, new[] { 5, 82, 328, 225 } }, new int[][] { new[] { 517, 5, 328, 71 }, new[] { 517, 82, 328, 225 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 148 }, new[] { 5, 159, 328, 148 } }, new int[][] { new[] { 517, 5, 328, 148 }, new[] { 517, 159, 328, 148 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 148 }, new[] { 5, 159, 161, 148 }, new[] { 172, 159, 161, 148 } }, new int[][] { new[] { 517, 5, 328, 148 }, new[] { 517, 159, 161, 148 }, new[] { 684, 159, 161, 148 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 161, 148 }, new[] { 172, 5, 161, 148 }, new[] { 5, 159, 161, 148 }, new[] { 172, 159, 161, 148 } }, new int[][] { new[] { 517, 5, 161, 148 }, new[] { 684, 5, 161, 148 }, new[] { 517, 159, 161, 148 }, new[] { 684, 159, 161, 148 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 148 }, new[] { 5, 159, 161, 71 }, new[] { 172, 159, 161, 71 }, new[] { 5, 236, 161, 71 }, new[] { 172, 236, 161, 71 } }, new int[][] { new[] { 517, 5, 328, 148 }, new[] { 517, 159, 161, 71 }, new[] { 684, 159, 161, 71 }, new[] { 517, 236, 161, 71 }, new[] { 684, 236, 161, 71 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 71 }, new[] { 5, 82, 328, 71 }, new[] { 5, 159, 328, 71 }, new[] { 5, 236, 328, 71 } }, new int[][] { new[] { 517, 5, 328, 71 }, new[] { 517, 82, 328, 71 }, new[] { 517, 159, 328, 71 }, new[] { 517, 236, 328, 71 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 161, 71 }, new[] { 172, 5, 161, 71 }, new[] { 5, 82, 161, 71 }, new[] { 172, 82, 161, 71 }, new[] { 5, 159, 161, 71 }, new[] { 172, 159, 161, 71 }, new[] { 5, 236, 161, 71 }, new[] { 172, 236, 161, 71 } }, new int[][] { new[] { 517, 5, 161, 71 }, new[] { 684, 5, 161, 71 }, new[] { 517, 82, 161, 71 }, new[] { 684, 82, 161, 71 }, new[] { 517, 159, 161, 71 }, new[] { 684, 159, 161, 71 }, new[] { 517, 236, 161, 71 }, new[] { 684, 236, 161, 71 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 71 }, new[] { 5, 82, 328, 71 }, new[] { 5, 159, 328, 148 } }, new int[][] { new[] { 517, 5, 328, 71 }, new[] { 517, 82, 328, 71 }, new[] { 517, 159, 328, 148 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 148 }, new[] { 5, 159, 328, 71 }, new[] { 5, 236, 328, 71 } }, new int[][] { new[] { 517, 5, 328, 148 }, new[] { 517, 159, 328, 71 }, new[] { 517, 236, 328, 71 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 225 }, new[] { 5, 236, 328, 71 } }, new int[][] { new[] { 517, 5, 328, 225 }, new[] { 517, 236, 328, 71 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 328, 302 } }, new int[][] { new[] { 517, 5, 328, 302 } } },
            },
        };
        // ToolbarRects[cornice][modello] (vuoto se la cornice non ha barra inferiore)
        public static readonly int[][][][] ToolbarRects = new int[][][][]
        {
            new int[][][] { new int[][] { new[] { 33, 397, 152, 78 }, new[] { 191, 397, 152, 78 }, new[] { 349, 397, 152, 78 }, new[] { 507, 397, 152, 78 }, new[] { 665, 397, 152, 78 } }, new int[][] { new[] { 32, 397, 126, 78 }, new[] { 164, 397, 126, 78 }, new[] { 296, 397, 126, 78 }, new[] { 428, 397, 126, 78 }, new[] { 560, 397, 126, 78 }, new[] { 692, 397, 126, 78 } }, new int[][] { new[] { 32, 397, 192, 78 }, new[] { 230, 397, 192, 78 }, new[] { 428, 397, 192, 78 }, new[] { 626, 397, 192, 78 } }, new int[][] { new[] { 32, 397, 258, 78 }, new[] { 296, 397, 258, 78 }, new[] { 560, 397, 258, 78 } }, new int[][] { new[] { 31, 397, 788, 78 } } },
            new int[][][] { new int[][] { new[] { 5, 397, 163, 78 }, new[] { 174, 397, 163, 78 }, new[] { 343, 397, 163, 78 }, new[] { 512, 397, 163, 78 }, new[] { 681, 397, 163, 78 } }, new int[][] { new[] { 5, 397, 135, 78 }, new[] { 146, 397, 135, 78 }, new[] { 287, 397, 135, 78 }, new[] { 428, 397, 135, 78 }, new[] { 569, 397, 135, 78 }, new[] { 710, 397, 135, 78 } }, new int[][] { new[] { 6, 397, 205, 78 }, new[] { 217, 397, 205, 78 }, new[] { 428, 397, 205, 78 }, new[] { 639, 397, 205, 78 } }, new int[][] { new[] { 5, 397, 276, 78 }, new[] { 287, 397, 276, 78 }, new[] { 569, 397, 276, 78 } }, new int[][] { new[] { 5, 397, 840, 78 } } },
            new int[][][] { new int[0][], new int[0][], new int[0][], new int[0][], new int[0][] },
            new int[][][] { new int[0][], new int[0][], new int[0][], new int[0][], new int[0][] },
            new int[][][] { new int[][] { new[] { 33, 313, 152, 78 }, new[] { 191, 313, 152, 78 }, new[] { 349, 313, 152, 78 }, new[] { 507, 313, 152, 78 }, new[] { 665, 313, 152, 78 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 313, 126, 78 }, new[] { 164, 313, 126, 78 }, new[] { 296, 313, 126, 78 }, new[] { 428, 313, 126, 78 }, new[] { 560, 313, 126, 78 }, new[] { 692, 313, 126, 78 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 313, 192, 78 }, new[] { 230, 313, 192, 78 }, new[] { 428, 313, 192, 78 }, new[] { 626, 313, 192, 78 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 313, 258, 78 }, new[] { 296, 313, 258, 78 }, new[] { 560, 313, 258, 78 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 31, 313, 788, 78 }, new[] { 31, 397, 788, 78 } } },
            new int[][][] { new int[][] { new[] { 5, 313, 163, 78 }, new[] { 174, 313, 163, 78 }, new[] { 343, 313, 163, 78 }, new[] { 512, 313, 163, 78 }, new[] { 681, 313, 163, 78 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 313, 135, 78 }, new[] { 146, 313, 135, 78 }, new[] { 287, 313, 135, 78 }, new[] { 428, 313, 135, 78 }, new[] { 569, 313, 135, 78 }, new[] { 710, 313, 135, 78 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 6, 313, 205, 78 }, new[] { 217, 313, 205, 78 }, new[] { 428, 313, 205, 78 }, new[] { 639, 313, 205, 78 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 313, 276, 78 }, new[] { 287, 313, 276, 78 }, new[] { 569, 313, 276, 78 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 313, 840, 78 }, new[] { 5, 397, 840, 78 } } },
        };
        // SidebarRects[cornice] (vuoto se senza barre laterali)
        public static readonly int[][][] SidebarRects = new int[][][]
        {
            new int[][] { new[] { 5, 5, 20, 386 }, new[] { 825, 5, 20, 386 } },
            new int[0][],
            new int[][] { new[] { 5, 5, 20, 470 }, new[] { 825, 5, 20, 470 } },
            new int[0][],
            new int[][] { new[] { 5, 5, 20, 302 }, new[] { 825, 5, 20, 302 } },
            new int[0][],
        };
        // CenterRects[cornice] = { delta, marcia, velocita', riga1, riga2, riga3 }
        public static readonly int[][][] CenterRects = new int[][][]
        {
            new int[][] { new[] { 339, 5, 172, 34 }, new[] { 339, 45, 172, 174 }, new[] { 339, 225, 172, 40 }, new[] { 339, 271, 172, 36 }, new[] { 339, 313, 172, 36 }, new[] { 339, 355, 172, 36 } },
            new int[][] { new[] { 339, 5, 172, 34 }, new[] { 339, 45, 172, 174 }, new[] { 339, 225, 172, 40 }, new[] { 339, 271, 172, 36 }, new[] { 339, 313, 172, 36 }, new[] { 339, 355, 172, 36 } },
            new int[][] { new[] { 339, 5, 172, 34 }, new[] { 339, 45, 172, 258 }, new[] { 339, 309, 172, 40 }, new[] { 339, 355, 172, 36 }, new[] { 339, 397, 172, 36 }, new[] { 339, 439, 172, 36 } },
            new int[][] { new[] { 339, 5, 172, 34 }, new[] { 339, 45, 172, 258 }, new[] { 339, 309, 172, 40 }, new[] { 339, 355, 172, 36 }, new[] { 339, 397, 172, 36 }, new[] { 339, 439, 172, 36 } },
            new int[][] { new[] { 339, 5, 172, 34 }, new[] { 339, 45, 172, 90 }, new[] { 339, 141, 172, 40 }, new[] { 339, 187, 172, 36 }, new[] { 339, 229, 172, 36 }, new[] { 339, 271, 172, 36 } },
            new int[][] { new[] { 339, 5, 172, 34 }, new[] { 339, 45, 172, 90 }, new[] { 339, 141, 172, 40 }, new[] { 339, 187, 172, 36 }, new[] { 339, 229, 172, 36 }, new[] { 339, 271, 172, 36 } },
        };

        public static List<PageSettings> DefaultPages()
        {
            var list = new List<PageSettings>();
            list.Add(new PageSettings { Name = "Gara", Frame = 1, GridLeft = 0, GridRight = 0, Toolbar = 0, SidebarLeft = 1, SidebarRight = 5,
                Left = new List<int> { 7, 1, 0, 0, 0, 0, 0, 0 }, Right = new List<int> { 8, 5, 0, 0, 0, 0, 0, 0 }, Bottom = new List<int> { 14, 15, 16, 17, 18, 0, 0 }, Center = new List<int> { 6, 10, 31 } });
            list.Add(new PageSettings { Name = "Qualifica", Frame = 0, GridLeft = 2, GridRight = 8, Toolbar = 1, SidebarLeft = 2, SidebarRight = 3,
                Left = new List<int> { 1, 9, 13, 0, 0, 0, 0, 0 }, Right = new List<int> { 5, 19, 21, 0, 0, 0, 0, 0 }, Bottom = new List<int> { 14, 15, 16, 17, 18, 28, 0 }, Center = new List<int> { 20, 10, 31 } });
            list.Add(new PageSettings { Name = "Endurance", Frame = 3, GridLeft = 6, GridRight = 4, Toolbar = 0, SidebarLeft = 0, SidebarRight = 0,
                Left = new List<int> { 14, 15, 16, 17, 18, 30, 34, 35 }, Right = new List<int> { 6, 27, 28, 29, 21, 0, 0, 0 }, Bottom = new List<int> { 14, 16, 17, 18, 28, 0, 0 }, Center = new List<int> { 32, 33, 31 } });
            return list;
        }
    }

    public class ChannelDef
    {
        public readonly string Id, Label, Source, Format, Unit;
        public readonly int Decimals;
        public ChannelDef(string id, string label, string source, string format, int decimals, string unit)
        {
            Id = id; Label = label; Source = source; Format = format; Decimals = decimals; Unit = unit;
        }
    }
}
