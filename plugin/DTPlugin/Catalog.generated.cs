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
            new ChannelDef("Gear", "MARCIA", "DataCorePlugin.GameData.Gear", "text", 0, "", "4"),
            new ChannelDef("Rpm", "GIRI", "DataCorePlugin.GameData.Rpms", "num", 0, "", "7250"),
            new ChannelDef("RpmPct", "GIRI %", "DataCorePlugin.GameData.CarSettings_CurrentDisplayedRPMPercent", "num", 0, "", "78"),
            new ChannelDef("Speed", "VELOCITÀ", "DataCorePlugin.GameData.SpeedLocal", "num", 0, "", "214"),
            new ChannelDef("Throttle", "GAS", "DataCorePlugin.GameData.Throttle", "num", 0, "", "86"),
            new ChannelDef("Brake", "FRENO", "DataCorePlugin.GameData.Brake", "num", 0, "", "12"),
            new ChannelDef("Clutch", "FRIZIONE", "DataCorePlugin.GameData.Clutch", "num", 0, "", "0"),
            new ChannelDef("TC", "TC", "DataCorePlugin.GameData.TCLevel", "num", 0, "", "3"),
            new ChannelDef("TCCut", "TC CUT", "@raw:PlayerNativeTelemetry.mTCCut", "num", 0, "", "0"),
            new ChannelDef("ABS", "ABS", "DataCorePlugin.GameData.ABSLevel", "num", 0, "", "2"),
            new ChannelDef("BB", "BB", "DataCorePlugin.GameData.BrakeBias", "num", 1, "", "56.2"),
            new ChannelDef("Map", "MAP", "DataCorePlugin.GameData.EngineMap", "num", 0, "", "1"),
            new ChannelDef("Fuel", "LITRI", "DataCorePlugin.GameData.Fuel", "num", 1, "", "14.0"),
            new ChannelDef("FuelPct", "CARB. %", "DataCorePlugin.GameData.FuelPercent", "num", 0, "%", "38%"),
            new ChannelDef("FuelPerLap", "MEDIA", "DataCorePlugin.Computed.Fuel_LitersPerLap", "num", 2, "", "2.69"),
            new ChannelDef("FuelLaps", "GIRI CARB.", "@giri_carb", "num", 1, "", "5.4"),
            new ChannelDef("Pos", "POS", "@pos", "num", 0, "", "11/25"),
            new ChannelDef("Cars", "AUTO", "@auto", "num", 0, "", "25"),
            new ChannelDef("PosGain", "POSIZIONI", "@pos_guadagno", "text", 0, "", "3"),
            new ChannelDef("Lap", "GIRO", "DataCorePlugin.GameData.CurrentLap", "num", 0, "", "7"),
            new ChannelDef("TotalLaps", "GIRI TOT.", "DataCorePlugin.GameData.TotalLaps", "num", 0, "", "24"),
            new ChannelDef("TimeLeft", "TEMPO", "DataCorePlugin.GameData.SessionTimeLeft", "duration", 0, "", "07:52"),
            new ChannelDef("Clock", "ORA", "@ora", "clock", 0, "", "12:13"),
            new ChannelDef("LastLap", "ULTIMO", "DataCorePlugin.GameData.LastLapTime", "laptime", 0, "", "2:04.572"),
            new ChannelDef("BestLap", "MIGLIORE", "DataCorePlugin.GameData.BestLapTime", "laptime", 0, "", "1:44.605"),
            new ChannelDef("EstLap", "PREVISTO", "@stimato", "laptime", 0, "", "1:44.535"),
            new ChannelDef("CurLap", "ATTUALE", "DataCorePlugin.GameData.CurrentLapTime", "laptime", 0, "", "1:12.408"),
            new ChannelDef("Delta", "DELTA", "@delta", "signed", 3, "", "-0.070"),
            new ChannelDef("GapAhead", "AVANTI", "@gap_avanti", "gap", 1, "", "1.4"),
            new ChannelDef("GapBehind", "DIETRO", "@gap_dietro", "gap", 1, "", "0.8"),
            new ChannelDef("AirTemp", "ARIA", "DataCorePlugin.GameData.AirTemperature", "num", 0, "°", "30°"),
            new ChannelDef("TrackTemp", "PISTA", "DataCorePlugin.GameData.RoadTemperature", "num", 0, "°", "38°"),
            new ChannelDef("WaterTemp", "ACQUA", "DataCorePlugin.GameData.WaterTemperature", "num", 0, "°", "88°"),
            new ChannelDef("OilTemp", "OLIO", "DataCorePlugin.GameData.OilTemperature", "num", 0, "°", "102°"),
            new ChannelDef("ERS", "ERS", "@ers", "num", 0, "%", "64%"),
            new ChannelDef("TyrePFL", "PRESS. AS", "@press_FL", "num", 1, "", "161.7"),
            new ChannelDef("TyrePFR", "PRESS. AD", "@press_FR", "num", 1, "", "163.2"),
            new ChannelDef("TyrePRL", "PRESS. PS", "@press_RL", "num", 1, "", "167.1"),
            new ChannelDef("TyrePRR", "PRESS. PD", "@press_RR", "num", 1, "", "166.0"),
            new ChannelDef("TyreTFL", "TEMP. AS", "@gomma_FL", "num", 0, "°", "84°"),
            new ChannelDef("TyreTFR", "TEMP. AD", "@gomma_FR", "num", 0, "°", "88°"),
            new ChannelDef("TyreTRL", "TEMP. PS", "@gomma_RL", "num", 0, "°", "83°"),
            new ChannelDef("TyreTRR", "TEMP. PD", "@gomma_RR", "num", 0, "°", "87°"),
            new ChannelDef("TyreWFL", "USURA AS", "DataCorePlugin.GameData.TyreWearFrontLeft", "num", 0, "%", "97%"),
            new ChannelDef("TyreWFR", "USURA AD", "DataCorePlugin.GameData.TyreWearFrontRight", "num", 0, "%", "95%"),
            new ChannelDef("TyreWRL", "USURA PS", "DataCorePlugin.GameData.TyreWearRearLeft", "num", 0, "%", "98%"),
            new ChannelDef("TyreWRR", "USURA PD", "DataCorePlugin.GameData.TyreWearRearRight", "num", 0, "%", "96%"),
            new ChannelDef("BrakeTFL", "FRENO AS", "DataCorePlugin.GameData.BrakeTemperatureFrontLeft", "num", 0, "°", "425°"),
            new ChannelDef("BrakeTFR", "FRENO AD", "DataCorePlugin.GameData.BrakeTemperatureFrontRight", "num", 0, "°", "418°"),
            new ChannelDef("BrakeTRL", "FRENO PS", "DataCorePlugin.GameData.BrakeTemperatureRearLeft", "num", 0, "°", "310°"),
            new ChannelDef("BrakeTRR", "FRENO PD", "DataCorePlugin.GameData.BrakeTemperatureRearRight", "num", 0, "°", "305°"),
            new ChannelDef("FlagYellow", "GIALLA", "DataCorePlugin.GameData.Flag_Yellow", "num", 0, "", "0"),
            new ChannelDef("FlagBlue", "BLU", "DataCorePlugin.GameData.Flag_Blue", "num", 0, "", "0"),
            new ChannelDef("PitLimiter", "LIMITATORE", "@limitatore", "num", 0, "", "0"),
            new ChannelDef("InPit", "CORSIA BOX", "DataCorePlugin.GameData.IsInPitLane", "num", 0, "", "0"),
            new ChannelDef("LastLapDiff", "DIFF. GIRO", "@delta_ultimo", "signed", 3, "", "+0.155"),
            new ChannelDef("FuelLastLap", "CONSUMO GIRO", "@consumo_ultimo", "num", 2, "", "2.71"),
            new ChannelDef("VE", "ENERGIA V.", "@ve", "num", 1, "%", "64.2%"),
            new ChannelDef("VELaps", "GIRI VE", "@ve_giri", "num", 1, "", "5.2"),
            new ChannelDef("VEPerLap", "VE / GIRO", "@ve_media", "num", 2, "%", "12.31%"),
            new ChannelDef("VELastLap", "VE ULT. GIRO", "@ve_ultimo", "num", 2, "%", "12.40%"),
            new ChannelDef("TCSlip", "TC SLIP", "@raw:PlayerNativeTelemetry.mTCSlip", "num", 0, "", "4"),
            new ChannelDef("TrackWet", "PISTA BAGN.", "@bagnato", "num", 0, "%", "0%"),
            new ChannelDef("Rain", "PIOGGIA", "@pioggia", "num", 0, "%", "0%"),
            new ChannelDef("RepDamage", "DANNI", "@rip_danni", "num", 1, "s", "12.5s"),
            new ChannelDef("RepBrakes", "FRENI", "@rip_freni", "num", 1, "s", "0.0s"),
            new ChannelDef("RepTyres", "GOMME", "@rip_gomme", "num", 1, "s", "8.0s"),
            new ChannelDef("RepFuel", "CARB.", "@rip_carb", "num", 1, "s", "21.4s"),
            new ChannelDef("RepTotal", "SOSTA", "@rip_totale", "num", 1, "s", "41.9s"),
            new ChannelDef("SuspFL", "SOSP. AS", "@sosp_FL", "num", 0, "%", "0%"),
            new ChannelDef("SuspFR", "SOSP. AD", "@sosp_FR", "num", 0, "%", "0%"),
            new ChannelDef("SuspRL", "SOSP. PS", "@sosp_RL", "num", 0, "%", "12%"),
            new ChannelDef("SuspRR", "SOSP. PD", "@sosp_RR", "num", 0, "%", "0%"),
            new ChannelDef("BrakeWFL", "FRENO AS", "@usura_freno_FL", "num", 0, "%", "92%"),
            new ChannelDef("BrakeWFR", "FRENO AD", "@usura_freno_FR", "num", 0, "%", "93%"),
            new ChannelDef("BrakeWRL", "FRENO PS", "@usura_freno_RL", "num", 0, "%", "96%"),
            new ChannelDef("BrakeWRR", "FRENO PD", "@usura_freno_RR", "num", 0, "%", "96%"),
            new ChannelDef("AeroDmg", "AERO", "@danno_aero", "num", 0, "%", "0%"),
            new ChannelDef("EngineDmg", "MOTORE", "@danno_motore", "num", 0, "%", "0%"),
            new ChannelDef("NewTyres", "GOMME NUOVE", "@gomme_nuove", "num", 0, "", "8"),
            new ChannelDef("TrackLim", "TRACK LIMITS", "@track_limits", "text", 0, "", "1/4"),
            new ChannelDef("Battery", "BATTERIA", "@batteria", "num", 0, "%", "78%"),
            new ChannelDef("Sky", "CIELO", "@cielo", "text", 0, "", "Nuvoloso"),
            new ChannelDef("FuelLive", "CONSUMO STIMATO", "@carb_stima", "num", 2, "", "2.64"),
            new ChannelDef("FuelDelta", "CONS. VS ULTIMO", "@carb_delta", "signed", 2, "", "-0.12"),
            new ChannelDef("VELive", "VE STIMATA", "@ve_stima", "num", 2, "%", "11.85%"),
            new ChannelDef("SavePlus1", "PER +1 GIRO", "@giro_piu", "text", 0, "", "2.52 l"),
            new ChannelDef("SaveStop", "1 SOSTA IN MENO", "@sosta_meno", "text", 0, "", "2.41 l"),
            new ChannelDef("Refill", "RIFORNIMENTO", "@rifornimento", "text", 0, "", "65 %"),
            new ChannelDef("TyreWearLap", "USURA / GIRO", "@usura_giro", "num", 1, "%", "1.8%"),
            new ChannelDef("TyreLaps", "GIRI GOMME", "@giri_gomme", "num", 0, "", "24"),
            new ChannelDef("PitRejoin", "DOPO IL BOX", "@pos_dopo_box", "text", 0, "", "P7"),
            new ChannelDef("Custom1", "PERS. 1", "", "num", 1, "", "-"),
            new ChannelDef("Custom2", "PERS. 2", "", "num", 1, "", "-"),
            new ChannelDef("Custom3", "PERS. 3", "", "num", 1, "", "-"),
            new ChannelDef("Custom4", "PERS. 4", "", "num", 1, "", "-"),
            new ChannelDef("Custom5", "PERS. 5", "", "num", 1, "", "-"),
            new ChannelDef("Custom6", "PERS. 6", "", "num", 1, "", "-"),
        };

        public static readonly string[] Cards = new string[] { "Vuoto", "Gomme (pressione + temperatura)", "Gomme (temperature)", "Usura gomme", "Temperature freni", "Tempi giro", "Carburante", "Sessione", "Distacchi", "Pedali", "Meteo", "Motore (acqua/olio)", "Assetto (TC/ABS/BB/MAP)", "Delta", "TC", "TC Cut", "ABS", "Ripartizione freno (BB)", "Mappa motore", "Posizione", "Giro", "Tempo rimasto", "Ultimo giro", "Miglior giro", "Giro stimato", "Velocità", "Giri motore", "Carburante (litri)", "Carburante (giri)", "Consumo medio", "ERS / batteria", "Ora", "Distacco avanti", "Distacco dietro", "Temperatura aria", "Temperatura pista", "Personalizzato 1", "Personalizzato 2", "Personalizzato 3", "Personalizzato 4", "Personalizzato 5", "Personalizzato 6", "Gomme complete (temp. carcassa, pressione, freni)", "Energia virtuale (VE)", "Giri energia virtuale", "Consumo energia virtuale", "Energia virtuale (completa)", "TC Slip", "Danni (schema auto)", "Stato auto (luci, bandiera, tergi, motore)", "Carburante (barra + giri)", "Energia virtuale (barra + giri)", "Strategia gara (soste, finestra, rifornimento)", "Classifica generale", "Classifica di classe", "Sosta (tempi stimati da LMU)", "Tempo sosta totale", "Danni sospensioni", "Danni (aero e motore)", "Usura freni", "Gomme nuove rimaste", "Track limits", "Batteria", "Previsioni meteo", "Gomme (tutto in uno: temperatura, pressione, usura, freni)", "Consumo stimato (in tempo reale)", "Consumo rispetto all'ultimo giro", "Energia stimata (in tempo reale)", "Consumo per fare 1 giro in più", "Consumo per 1 sosta in meno", "Risparmio carburante", "Rifornimento impostato (menu box LMU)", "Usura gomme per giro", "Giri che durano le gomme", "Gomme (usura e durata)", "Posizione dopo il box" };
        public static readonly string[] Bars = new string[] { "Vuoto", "Giri motore", "Gas", "Freno", "Frizione", "Carburante", "ERS / batteria" };
        public static readonly string[] Frames = new string[] { "Completo (barre laterali + barra inferiore)", "Solo barra inferiore", "Solo barre laterali", "Pulito (solo riquadri)", "Completo con barra doppia", "Barra doppia senza barre laterali", "Barra bassa con barre laterali", "Barra bassa", "Completo (barre laterali + barra inferiore) + LED", "Solo barra inferiore + LED", "Solo barre laterali + LED", "Pulito (solo riquadri) + LED", "Completo con barra doppia + LED", "Barra doppia senza barre laterali + LED", "Barra bassa con barre laterali + LED", "Barra bassa + LED" };
        // cornici da LedOffset in poi: le stesse con la fila di LED in alto
        public const int LedOffset = 8;
        public static readonly bool[] FrameHasSidebars = new bool[] { true, false, true, false, true, false, true, false, true, false, true, false, true, false, true, false };
        public static readonly bool[] FrameHasToolbar = new bool[] { true, true, false, false, true, true, true, true, true, true, false, false, true, true, true, true };
        public static readonly string[] Grids = new string[] { "Riga + grande", "2 grandi", "Grande + 2 medi", "4 medi", "Grande + 4 piccoli", "4 righe", "8 piccoli", "2 righe + grande", "Grande + 2 righe", "Grande + riga", "Unico" };
        public static readonly string[] Categories = new string[] { "Tempi", "Carburante ed energia", "Comandi auto", "Gomme e freni", "Sessione e gara", "Motore e pedali", "Meteo", "Sosta e danni (LMU)", "Personalizzati" };
        // CategoryCards[categoria] = indici delle schede
        public static readonly int[][] CategoryCards = new int[][] { new[] { 5, 13, 22, 23, 24 }, new[] { 6, 27, 28, 29, 43, 46, 44, 45, 30, 50, 51, 52, 65, 66, 67, 68, 69, 70, 71 }, new[] { 12, 14, 15, 47, 16, 17, 18 }, new[] { 64, 42, 1, 2, 3, 4, 48, 72, 73, 74 }, new[] { 7, 19, 20, 21, 31, 8, 32, 33, 53, 54, 49 }, new[] { 9, 25, 26, 11 }, new[] { 10, 34, 35, 63 }, new[] { 55, 56, 57, 58, 59, 60, 61, 62, 75 }, new[] { 36, 37, 38, 39, 40, 41 } };
        // GridUnits[griglia] = celle in unita' 2 colonne x 4 righe {col, riga, larghezza, altezza}
        public static readonly int[][][] GridUnits = new int[][][] { new int[][] { new[] { 0, 0, 2, 1 }, new[] { 0, 1, 2, 3 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 2, 2 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 1, 2 }, new[] { 1, 2, 1, 2 } }, new int[][] { new[] { 0, 0, 1, 2 }, new[] { 1, 0, 1, 2 }, new[] { 0, 2, 1, 2 }, new[] { 1, 2, 1, 2 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 1, 1 }, new[] { 1, 2, 1, 1 }, new[] { 0, 3, 1, 1 }, new[] { 1, 3, 1, 1 } }, new int[][] { new[] { 0, 0, 2, 1 }, new[] { 0, 1, 2, 1 }, new[] { 0, 2, 2, 1 }, new[] { 0, 3, 2, 1 } }, new int[][] { new[] { 0, 0, 1, 1 }, new[] { 1, 0, 1, 1 }, new[] { 0, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, new[] { 0, 2, 1, 1 }, new[] { 1, 2, 1, 1 }, new[] { 0, 3, 1, 1 }, new[] { 1, 3, 1, 1 } }, new int[][] { new[] { 0, 0, 2, 1 }, new[] { 0, 1, 2, 1 }, new[] { 0, 2, 2, 2 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 2, 1 }, new[] { 0, 3, 2, 1 } }, new int[][] { new[] { 0, 0, 2, 3 }, new[] { 0, 3, 2, 1 } }, new int[][] { new[] { 0, 0, 2, 4 } } };
        public static readonly string[] Toolbars = new string[] { "5 riquadri", "6 riquadri", "4 riquadri", "3 riquadri", "1 riquadro largo" };

        // Geometria (pixel della dashboard 850x480) per l'anteprima nelle impostazioni
        // GridRects[cornice][griglia][lato 0=sx 1=dx] -> rettangoli {x, y, w, h}
        public static readonly int[][][][][] GridRects = new int[][][][][]
        {
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 5, 288, 78 }, new[] { 31, 89, 288, 302 } }, new int[][] { new[] { 531, 5, 288, 78 }, new[] { 531, 89, 288, 302 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 190 }, new[] { 31, 201, 288, 190 } }, new int[][] { new[] { 531, 5, 288, 190 }, new[] { 531, 201, 288, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 190 }, new[] { 31, 201, 141, 190 }, new[] { 178, 201, 141, 190 } }, new int[][] { new[] { 531, 5, 288, 190 }, new[] { 531, 201, 141, 190 }, new[] { 678, 201, 141, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 190 }, new[] { 178, 5, 141, 190 }, new[] { 31, 201, 141, 190 }, new[] { 178, 201, 141, 190 } }, new int[][] { new[] { 531, 5, 141, 190 }, new[] { 678, 5, 141, 190 }, new[] { 531, 201, 141, 190 }, new[] { 678, 201, 141, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 190 }, new[] { 31, 201, 141, 92 }, new[] { 178, 201, 141, 92 }, new[] { 31, 299, 141, 92 }, new[] { 178, 299, 141, 92 } }, new int[][] { new[] { 531, 5, 288, 190 }, new[] { 531, 201, 141, 92 }, new[] { 678, 201, 141, 92 }, new[] { 531, 299, 141, 92 }, new[] { 678, 299, 141, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 92 }, new[] { 31, 103, 288, 92 }, new[] { 31, 201, 288, 92 }, new[] { 31, 299, 288, 92 } }, new int[][] { new[] { 531, 5, 288, 92 }, new[] { 531, 103, 288, 92 }, new[] { 531, 201, 288, 92 }, new[] { 531, 299, 288, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 92 }, new[] { 178, 5, 141, 92 }, new[] { 31, 103, 141, 92 }, new[] { 178, 103, 141, 92 }, new[] { 31, 201, 141, 92 }, new[] { 178, 201, 141, 92 }, new[] { 31, 299, 141, 92 }, new[] { 178, 299, 141, 92 } }, new int[][] { new[] { 531, 5, 141, 92 }, new[] { 678, 5, 141, 92 }, new[] { 531, 103, 141, 92 }, new[] { 678, 103, 141, 92 }, new[] { 531, 201, 141, 92 }, new[] { 678, 201, 141, 92 }, new[] { 531, 299, 141, 92 }, new[] { 678, 299, 141, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 92 }, new[] { 31, 103, 288, 92 }, new[] { 31, 201, 288, 190 } }, new int[][] { new[] { 531, 5, 288, 92 }, new[] { 531, 103, 288, 92 }, new[] { 531, 201, 288, 190 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 190 }, new[] { 31, 201, 288, 92 }, new[] { 31, 299, 288, 92 } }, new int[][] { new[] { 531, 5, 288, 190 }, new[] { 531, 201, 288, 92 }, new[] { 531, 299, 288, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 288 }, new[] { 31, 299, 288, 92 } }, new int[][] { new[] { 531, 5, 288, 288 }, new[] { 531, 299, 288, 92 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 386 } }, new int[][] { new[] { 531, 5, 288, 386 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 5, 314, 78 }, new[] { 5, 89, 314, 302 } }, new int[][] { new[] { 531, 5, 314, 78 }, new[] { 531, 89, 314, 302 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 190 }, new[] { 5, 201, 314, 190 } }, new int[][] { new[] { 531, 5, 314, 190 }, new[] { 531, 201, 314, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 190 }, new[] { 5, 201, 154, 190 }, new[] { 165, 201, 154, 190 } }, new int[][] { new[] { 531, 5, 314, 190 }, new[] { 531, 201, 154, 190 }, new[] { 691, 201, 154, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 190 }, new[] { 165, 5, 154, 190 }, new[] { 5, 201, 154, 190 }, new[] { 165, 201, 154, 190 } }, new int[][] { new[] { 531, 5, 154, 190 }, new[] { 691, 5, 154, 190 }, new[] { 531, 201, 154, 190 }, new[] { 691, 201, 154, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 190 }, new[] { 5, 201, 154, 92 }, new[] { 165, 201, 154, 92 }, new[] { 5, 299, 154, 92 }, new[] { 165, 299, 154, 92 } }, new int[][] { new[] { 531, 5, 314, 190 }, new[] { 531, 201, 154, 92 }, new[] { 691, 201, 154, 92 }, new[] { 531, 299, 154, 92 }, new[] { 691, 299, 154, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 92 }, new[] { 5, 103, 314, 92 }, new[] { 5, 201, 314, 92 }, new[] { 5, 299, 314, 92 } }, new int[][] { new[] { 531, 5, 314, 92 }, new[] { 531, 103, 314, 92 }, new[] { 531, 201, 314, 92 }, new[] { 531, 299, 314, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 92 }, new[] { 165, 5, 154, 92 }, new[] { 5, 103, 154, 92 }, new[] { 165, 103, 154, 92 }, new[] { 5, 201, 154, 92 }, new[] { 165, 201, 154, 92 }, new[] { 5, 299, 154, 92 }, new[] { 165, 299, 154, 92 } }, new int[][] { new[] { 531, 5, 154, 92 }, new[] { 691, 5, 154, 92 }, new[] { 531, 103, 154, 92 }, new[] { 691, 103, 154, 92 }, new[] { 531, 201, 154, 92 }, new[] { 691, 201, 154, 92 }, new[] { 531, 299, 154, 92 }, new[] { 691, 299, 154, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 92 }, new[] { 5, 103, 314, 92 }, new[] { 5, 201, 314, 190 } }, new int[][] { new[] { 531, 5, 314, 92 }, new[] { 531, 103, 314, 92 }, new[] { 531, 201, 314, 190 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 190 }, new[] { 5, 201, 314, 92 }, new[] { 5, 299, 314, 92 } }, new int[][] { new[] { 531, 5, 314, 190 }, new[] { 531, 201, 314, 92 }, new[] { 531, 299, 314, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 288 }, new[] { 5, 299, 314, 92 } }, new int[][] { new[] { 531, 5, 314, 288 }, new[] { 531, 299, 314, 92 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 386 } }, new int[][] { new[] { 531, 5, 314, 386 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 5, 288, 78 }, new[] { 31, 89, 288, 386 } }, new int[][] { new[] { 531, 5, 288, 78 }, new[] { 531, 89, 288, 386 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 232 }, new[] { 31, 243, 288, 232 } }, new int[][] { new[] { 531, 5, 288, 232 }, new[] { 531, 243, 288, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 232 }, new[] { 31, 243, 141, 232 }, new[] { 178, 243, 141, 232 } }, new int[][] { new[] { 531, 5, 288, 232 }, new[] { 531, 243, 141, 232 }, new[] { 678, 243, 141, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 232 }, new[] { 178, 5, 141, 232 }, new[] { 31, 243, 141, 232 }, new[] { 178, 243, 141, 232 } }, new int[][] { new[] { 531, 5, 141, 232 }, new[] { 678, 5, 141, 232 }, new[] { 531, 243, 141, 232 }, new[] { 678, 243, 141, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 232 }, new[] { 31, 243, 141, 113 }, new[] { 178, 243, 141, 113 }, new[] { 31, 362, 141, 113 }, new[] { 178, 362, 141, 113 } }, new int[][] { new[] { 531, 5, 288, 232 }, new[] { 531, 243, 141, 113 }, new[] { 678, 243, 141, 113 }, new[] { 531, 362, 141, 113 }, new[] { 678, 362, 141, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 113 }, new[] { 31, 124, 288, 113 }, new[] { 31, 243, 288, 113 }, new[] { 31, 362, 288, 113 } }, new int[][] { new[] { 531, 5, 288, 113 }, new[] { 531, 124, 288, 113 }, new[] { 531, 243, 288, 113 }, new[] { 531, 362, 288, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 113 }, new[] { 178, 5, 141, 113 }, new[] { 31, 124, 141, 113 }, new[] { 178, 124, 141, 113 }, new[] { 31, 243, 141, 113 }, new[] { 178, 243, 141, 113 }, new[] { 31, 362, 141, 113 }, new[] { 178, 362, 141, 113 } }, new int[][] { new[] { 531, 5, 141, 113 }, new[] { 678, 5, 141, 113 }, new[] { 531, 124, 141, 113 }, new[] { 678, 124, 141, 113 }, new[] { 531, 243, 141, 113 }, new[] { 678, 243, 141, 113 }, new[] { 531, 362, 141, 113 }, new[] { 678, 362, 141, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 113 }, new[] { 31, 124, 288, 113 }, new[] { 31, 243, 288, 232 } }, new int[][] { new[] { 531, 5, 288, 113 }, new[] { 531, 124, 288, 113 }, new[] { 531, 243, 288, 232 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 232 }, new[] { 31, 243, 288, 113 }, new[] { 31, 362, 288, 113 } }, new int[][] { new[] { 531, 5, 288, 232 }, new[] { 531, 243, 288, 113 }, new[] { 531, 362, 288, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 351 }, new[] { 31, 362, 288, 113 } }, new int[][] { new[] { 531, 5, 288, 351 }, new[] { 531, 362, 288, 113 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 470 } }, new int[][] { new[] { 531, 5, 288, 470 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 5, 314, 78 }, new[] { 5, 89, 314, 386 } }, new int[][] { new[] { 531, 5, 314, 78 }, new[] { 531, 89, 314, 386 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 232 }, new[] { 5, 243, 314, 232 } }, new int[][] { new[] { 531, 5, 314, 232 }, new[] { 531, 243, 314, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 232 }, new[] { 5, 243, 154, 232 }, new[] { 165, 243, 154, 232 } }, new int[][] { new[] { 531, 5, 314, 232 }, new[] { 531, 243, 154, 232 }, new[] { 691, 243, 154, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 232 }, new[] { 165, 5, 154, 232 }, new[] { 5, 243, 154, 232 }, new[] { 165, 243, 154, 232 } }, new int[][] { new[] { 531, 5, 154, 232 }, new[] { 691, 5, 154, 232 }, new[] { 531, 243, 154, 232 }, new[] { 691, 243, 154, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 232 }, new[] { 5, 243, 154, 113 }, new[] { 165, 243, 154, 113 }, new[] { 5, 362, 154, 113 }, new[] { 165, 362, 154, 113 } }, new int[][] { new[] { 531, 5, 314, 232 }, new[] { 531, 243, 154, 113 }, new[] { 691, 243, 154, 113 }, new[] { 531, 362, 154, 113 }, new[] { 691, 362, 154, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 113 }, new[] { 5, 124, 314, 113 }, new[] { 5, 243, 314, 113 }, new[] { 5, 362, 314, 113 } }, new int[][] { new[] { 531, 5, 314, 113 }, new[] { 531, 124, 314, 113 }, new[] { 531, 243, 314, 113 }, new[] { 531, 362, 314, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 113 }, new[] { 165, 5, 154, 113 }, new[] { 5, 124, 154, 113 }, new[] { 165, 124, 154, 113 }, new[] { 5, 243, 154, 113 }, new[] { 165, 243, 154, 113 }, new[] { 5, 362, 154, 113 }, new[] { 165, 362, 154, 113 } }, new int[][] { new[] { 531, 5, 154, 113 }, new[] { 691, 5, 154, 113 }, new[] { 531, 124, 154, 113 }, new[] { 691, 124, 154, 113 }, new[] { 531, 243, 154, 113 }, new[] { 691, 243, 154, 113 }, new[] { 531, 362, 154, 113 }, new[] { 691, 362, 154, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 113 }, new[] { 5, 124, 314, 113 }, new[] { 5, 243, 314, 232 } }, new int[][] { new[] { 531, 5, 314, 113 }, new[] { 531, 124, 314, 113 }, new[] { 531, 243, 314, 232 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 232 }, new[] { 5, 243, 314, 113 }, new[] { 5, 362, 314, 113 } }, new int[][] { new[] { 531, 5, 314, 232 }, new[] { 531, 243, 314, 113 }, new[] { 531, 362, 314, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 351 }, new[] { 5, 362, 314, 113 } }, new int[][] { new[] { 531, 5, 314, 351 }, new[] { 531, 362, 314, 113 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 470 } }, new int[][] { new[] { 531, 5, 314, 470 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 5, 288, 62 }, new[] { 31, 73, 288, 250 } }, new int[][] { new[] { 531, 5, 288, 62 }, new[] { 531, 73, 288, 250 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 156 }, new[] { 31, 167, 288, 156 } }, new int[][] { new[] { 531, 5, 288, 156 }, new[] { 531, 167, 288, 156 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 156 }, new[] { 31, 167, 141, 156 }, new[] { 178, 167, 141, 156 } }, new int[][] { new[] { 531, 5, 288, 156 }, new[] { 531, 167, 141, 156 }, new[] { 678, 167, 141, 156 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 156 }, new[] { 178, 5, 141, 156 }, new[] { 31, 167, 141, 156 }, new[] { 178, 167, 141, 156 } }, new int[][] { new[] { 531, 5, 141, 156 }, new[] { 678, 5, 141, 156 }, new[] { 531, 167, 141, 156 }, new[] { 678, 167, 141, 156 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 156 }, new[] { 31, 167, 141, 75 }, new[] { 178, 167, 141, 75 }, new[] { 31, 248, 141, 75 }, new[] { 178, 248, 141, 75 } }, new int[][] { new[] { 531, 5, 288, 156 }, new[] { 531, 167, 141, 75 }, new[] { 678, 167, 141, 75 }, new[] { 531, 248, 141, 75 }, new[] { 678, 248, 141, 75 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 75 }, new[] { 31, 86, 288, 75 }, new[] { 31, 167, 288, 75 }, new[] { 31, 248, 288, 75 } }, new int[][] { new[] { 531, 5, 288, 75 }, new[] { 531, 86, 288, 75 }, new[] { 531, 167, 288, 75 }, new[] { 531, 248, 288, 75 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 75 }, new[] { 178, 5, 141, 75 }, new[] { 31, 86, 141, 75 }, new[] { 178, 86, 141, 75 }, new[] { 31, 167, 141, 75 }, new[] { 178, 167, 141, 75 }, new[] { 31, 248, 141, 75 }, new[] { 178, 248, 141, 75 } }, new int[][] { new[] { 531, 5, 141, 75 }, new[] { 678, 5, 141, 75 }, new[] { 531, 86, 141, 75 }, new[] { 678, 86, 141, 75 }, new[] { 531, 167, 141, 75 }, new[] { 678, 167, 141, 75 }, new[] { 531, 248, 141, 75 }, new[] { 678, 248, 141, 75 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 75 }, new[] { 31, 86, 288, 75 }, new[] { 31, 167, 288, 156 } }, new int[][] { new[] { 531, 5, 288, 75 }, new[] { 531, 86, 288, 75 }, new[] { 531, 167, 288, 156 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 156 }, new[] { 31, 167, 288, 75 }, new[] { 31, 248, 288, 75 } }, new int[][] { new[] { 531, 5, 288, 156 }, new[] { 531, 167, 288, 75 }, new[] { 531, 248, 288, 75 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 237 }, new[] { 31, 248, 288, 75 } }, new int[][] { new[] { 531, 5, 288, 237 }, new[] { 531, 248, 288, 75 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 318 } }, new int[][] { new[] { 531, 5, 288, 318 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 5, 314, 62 }, new[] { 5, 73, 314, 250 } }, new int[][] { new[] { 531, 5, 314, 62 }, new[] { 531, 73, 314, 250 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 156 }, new[] { 5, 167, 314, 156 } }, new int[][] { new[] { 531, 5, 314, 156 }, new[] { 531, 167, 314, 156 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 156 }, new[] { 5, 167, 154, 156 }, new[] { 165, 167, 154, 156 } }, new int[][] { new[] { 531, 5, 314, 156 }, new[] { 531, 167, 154, 156 }, new[] { 691, 167, 154, 156 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 156 }, new[] { 165, 5, 154, 156 }, new[] { 5, 167, 154, 156 }, new[] { 165, 167, 154, 156 } }, new int[][] { new[] { 531, 5, 154, 156 }, new[] { 691, 5, 154, 156 }, new[] { 531, 167, 154, 156 }, new[] { 691, 167, 154, 156 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 156 }, new[] { 5, 167, 154, 75 }, new[] { 165, 167, 154, 75 }, new[] { 5, 248, 154, 75 }, new[] { 165, 248, 154, 75 } }, new int[][] { new[] { 531, 5, 314, 156 }, new[] { 531, 167, 154, 75 }, new[] { 691, 167, 154, 75 }, new[] { 531, 248, 154, 75 }, new[] { 691, 248, 154, 75 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 75 }, new[] { 5, 86, 314, 75 }, new[] { 5, 167, 314, 75 }, new[] { 5, 248, 314, 75 } }, new int[][] { new[] { 531, 5, 314, 75 }, new[] { 531, 86, 314, 75 }, new[] { 531, 167, 314, 75 }, new[] { 531, 248, 314, 75 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 75 }, new[] { 165, 5, 154, 75 }, new[] { 5, 86, 154, 75 }, new[] { 165, 86, 154, 75 }, new[] { 5, 167, 154, 75 }, new[] { 165, 167, 154, 75 }, new[] { 5, 248, 154, 75 }, new[] { 165, 248, 154, 75 } }, new int[][] { new[] { 531, 5, 154, 75 }, new[] { 691, 5, 154, 75 }, new[] { 531, 86, 154, 75 }, new[] { 691, 86, 154, 75 }, new[] { 531, 167, 154, 75 }, new[] { 691, 167, 154, 75 }, new[] { 531, 248, 154, 75 }, new[] { 691, 248, 154, 75 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 75 }, new[] { 5, 86, 314, 75 }, new[] { 5, 167, 314, 156 } }, new int[][] { new[] { 531, 5, 314, 75 }, new[] { 531, 86, 314, 75 }, new[] { 531, 167, 314, 156 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 156 }, new[] { 5, 167, 314, 75 }, new[] { 5, 248, 314, 75 } }, new int[][] { new[] { 531, 5, 314, 156 }, new[] { 531, 167, 314, 75 }, new[] { 531, 248, 314, 75 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 237 }, new[] { 5, 248, 314, 75 } }, new int[][] { new[] { 531, 5, 314, 237 }, new[] { 531, 248, 314, 75 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 318 } }, new int[][] { new[] { 531, 5, 314, 318 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 5, 288, 62 }, new[] { 31, 73, 288, 334 } }, new int[][] { new[] { 531, 5, 288, 62 }, new[] { 531, 73, 288, 334 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 198 }, new[] { 31, 209, 288, 198 } }, new int[][] { new[] { 531, 5, 288, 198 }, new[] { 531, 209, 288, 198 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 198 }, new[] { 31, 209, 141, 198 }, new[] { 178, 209, 141, 198 } }, new int[][] { new[] { 531, 5, 288, 198 }, new[] { 531, 209, 141, 198 }, new[] { 678, 209, 141, 198 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 198 }, new[] { 178, 5, 141, 198 }, new[] { 31, 209, 141, 198 }, new[] { 178, 209, 141, 198 } }, new int[][] { new[] { 531, 5, 141, 198 }, new[] { 678, 5, 141, 198 }, new[] { 531, 209, 141, 198 }, new[] { 678, 209, 141, 198 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 198 }, new[] { 31, 209, 141, 96 }, new[] { 178, 209, 141, 96 }, new[] { 31, 311, 141, 96 }, new[] { 178, 311, 141, 96 } }, new int[][] { new[] { 531, 5, 288, 198 }, new[] { 531, 209, 141, 96 }, new[] { 678, 209, 141, 96 }, new[] { 531, 311, 141, 96 }, new[] { 678, 311, 141, 96 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 96 }, new[] { 31, 107, 288, 96 }, new[] { 31, 209, 288, 96 }, new[] { 31, 311, 288, 96 } }, new int[][] { new[] { 531, 5, 288, 96 }, new[] { 531, 107, 288, 96 }, new[] { 531, 209, 288, 96 }, new[] { 531, 311, 288, 96 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 141, 96 }, new[] { 178, 5, 141, 96 }, new[] { 31, 107, 141, 96 }, new[] { 178, 107, 141, 96 }, new[] { 31, 209, 141, 96 }, new[] { 178, 209, 141, 96 }, new[] { 31, 311, 141, 96 }, new[] { 178, 311, 141, 96 } }, new int[][] { new[] { 531, 5, 141, 96 }, new[] { 678, 5, 141, 96 }, new[] { 531, 107, 141, 96 }, new[] { 678, 107, 141, 96 }, new[] { 531, 209, 141, 96 }, new[] { 678, 209, 141, 96 }, new[] { 531, 311, 141, 96 }, new[] { 678, 311, 141, 96 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 96 }, new[] { 31, 107, 288, 96 }, new[] { 31, 209, 288, 198 } }, new int[][] { new[] { 531, 5, 288, 96 }, new[] { 531, 107, 288, 96 }, new[] { 531, 209, 288, 198 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 198 }, new[] { 31, 209, 288, 96 }, new[] { 31, 311, 288, 96 } }, new int[][] { new[] { 531, 5, 288, 198 }, new[] { 531, 209, 288, 96 }, new[] { 531, 311, 288, 96 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 300 }, new[] { 31, 311, 288, 96 } }, new int[][] { new[] { 531, 5, 288, 300 }, new[] { 531, 311, 288, 96 } } },
                new int[][][] { new int[][] { new[] { 31, 5, 288, 402 } }, new int[][] { new[] { 531, 5, 288, 402 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 5, 314, 62 }, new[] { 5, 73, 314, 334 } }, new int[][] { new[] { 531, 5, 314, 62 }, new[] { 531, 73, 314, 334 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 198 }, new[] { 5, 209, 314, 198 } }, new int[][] { new[] { 531, 5, 314, 198 }, new[] { 531, 209, 314, 198 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 198 }, new[] { 5, 209, 154, 198 }, new[] { 165, 209, 154, 198 } }, new int[][] { new[] { 531, 5, 314, 198 }, new[] { 531, 209, 154, 198 }, new[] { 691, 209, 154, 198 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 198 }, new[] { 165, 5, 154, 198 }, new[] { 5, 209, 154, 198 }, new[] { 165, 209, 154, 198 } }, new int[][] { new[] { 531, 5, 154, 198 }, new[] { 691, 5, 154, 198 }, new[] { 531, 209, 154, 198 }, new[] { 691, 209, 154, 198 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 198 }, new[] { 5, 209, 154, 96 }, new[] { 165, 209, 154, 96 }, new[] { 5, 311, 154, 96 }, new[] { 165, 311, 154, 96 } }, new int[][] { new[] { 531, 5, 314, 198 }, new[] { 531, 209, 154, 96 }, new[] { 691, 209, 154, 96 }, new[] { 531, 311, 154, 96 }, new[] { 691, 311, 154, 96 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 96 }, new[] { 5, 107, 314, 96 }, new[] { 5, 209, 314, 96 }, new[] { 5, 311, 314, 96 } }, new int[][] { new[] { 531, 5, 314, 96 }, new[] { 531, 107, 314, 96 }, new[] { 531, 209, 314, 96 }, new[] { 531, 311, 314, 96 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 154, 96 }, new[] { 165, 5, 154, 96 }, new[] { 5, 107, 154, 96 }, new[] { 165, 107, 154, 96 }, new[] { 5, 209, 154, 96 }, new[] { 165, 209, 154, 96 }, new[] { 5, 311, 154, 96 }, new[] { 165, 311, 154, 96 } }, new int[][] { new[] { 531, 5, 154, 96 }, new[] { 691, 5, 154, 96 }, new[] { 531, 107, 154, 96 }, new[] { 691, 107, 154, 96 }, new[] { 531, 209, 154, 96 }, new[] { 691, 209, 154, 96 }, new[] { 531, 311, 154, 96 }, new[] { 691, 311, 154, 96 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 96 }, new[] { 5, 107, 314, 96 }, new[] { 5, 209, 314, 198 } }, new int[][] { new[] { 531, 5, 314, 96 }, new[] { 531, 107, 314, 96 }, new[] { 531, 209, 314, 198 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 198 }, new[] { 5, 209, 314, 96 }, new[] { 5, 311, 314, 96 } }, new int[][] { new[] { 531, 5, 314, 198 }, new[] { 531, 209, 314, 96 }, new[] { 531, 311, 314, 96 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 300 }, new[] { 5, 311, 314, 96 } }, new int[][] { new[] { 531, 5, 314, 300 }, new[] { 531, 311, 314, 96 } } },
                new int[][][] { new int[][] { new[] { 5, 5, 314, 402 } }, new int[][] { new[] { 531, 5, 314, 402 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 37, 288, 78 }, new[] { 31, 121, 288, 270 } }, new int[][] { new[] { 531, 37, 288, 78 }, new[] { 531, 121, 288, 270 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 174 }, new[] { 31, 217, 288, 174 } }, new int[][] { new[] { 531, 37, 288, 174 }, new[] { 531, 217, 288, 174 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 174 }, new[] { 31, 217, 141, 174 }, new[] { 178, 217, 141, 174 } }, new int[][] { new[] { 531, 37, 288, 174 }, new[] { 531, 217, 141, 174 }, new[] { 678, 217, 141, 174 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 174 }, new[] { 178, 37, 141, 174 }, new[] { 31, 217, 141, 174 }, new[] { 178, 217, 141, 174 } }, new int[][] { new[] { 531, 37, 141, 174 }, new[] { 678, 37, 141, 174 }, new[] { 531, 217, 141, 174 }, new[] { 678, 217, 141, 174 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 174 }, new[] { 31, 217, 141, 84 }, new[] { 178, 217, 141, 84 }, new[] { 31, 307, 141, 84 }, new[] { 178, 307, 141, 84 } }, new int[][] { new[] { 531, 37, 288, 174 }, new[] { 531, 217, 141, 84 }, new[] { 678, 217, 141, 84 }, new[] { 531, 307, 141, 84 }, new[] { 678, 307, 141, 84 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 84 }, new[] { 31, 127, 288, 84 }, new[] { 31, 217, 288, 84 }, new[] { 31, 307, 288, 84 } }, new int[][] { new[] { 531, 37, 288, 84 }, new[] { 531, 127, 288, 84 }, new[] { 531, 217, 288, 84 }, new[] { 531, 307, 288, 84 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 84 }, new[] { 178, 37, 141, 84 }, new[] { 31, 127, 141, 84 }, new[] { 178, 127, 141, 84 }, new[] { 31, 217, 141, 84 }, new[] { 178, 217, 141, 84 }, new[] { 31, 307, 141, 84 }, new[] { 178, 307, 141, 84 } }, new int[][] { new[] { 531, 37, 141, 84 }, new[] { 678, 37, 141, 84 }, new[] { 531, 127, 141, 84 }, new[] { 678, 127, 141, 84 }, new[] { 531, 217, 141, 84 }, new[] { 678, 217, 141, 84 }, new[] { 531, 307, 141, 84 }, new[] { 678, 307, 141, 84 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 84 }, new[] { 31, 127, 288, 84 }, new[] { 31, 217, 288, 174 } }, new int[][] { new[] { 531, 37, 288, 84 }, new[] { 531, 127, 288, 84 }, new[] { 531, 217, 288, 174 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 174 }, new[] { 31, 217, 288, 84 }, new[] { 31, 307, 288, 84 } }, new int[][] { new[] { 531, 37, 288, 174 }, new[] { 531, 217, 288, 84 }, new[] { 531, 307, 288, 84 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 264 }, new[] { 31, 307, 288, 84 } }, new int[][] { new[] { 531, 37, 288, 264 }, new[] { 531, 307, 288, 84 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 354 } }, new int[][] { new[] { 531, 37, 288, 354 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 37, 314, 78 }, new[] { 5, 121, 314, 270 } }, new int[][] { new[] { 531, 37, 314, 78 }, new[] { 531, 121, 314, 270 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 174 }, new[] { 5, 217, 314, 174 } }, new int[][] { new[] { 531, 37, 314, 174 }, new[] { 531, 217, 314, 174 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 174 }, new[] { 5, 217, 154, 174 }, new[] { 165, 217, 154, 174 } }, new int[][] { new[] { 531, 37, 314, 174 }, new[] { 531, 217, 154, 174 }, new[] { 691, 217, 154, 174 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 174 }, new[] { 165, 37, 154, 174 }, new[] { 5, 217, 154, 174 }, new[] { 165, 217, 154, 174 } }, new int[][] { new[] { 531, 37, 154, 174 }, new[] { 691, 37, 154, 174 }, new[] { 531, 217, 154, 174 }, new[] { 691, 217, 154, 174 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 174 }, new[] { 5, 217, 154, 84 }, new[] { 165, 217, 154, 84 }, new[] { 5, 307, 154, 84 }, new[] { 165, 307, 154, 84 } }, new int[][] { new[] { 531, 37, 314, 174 }, new[] { 531, 217, 154, 84 }, new[] { 691, 217, 154, 84 }, new[] { 531, 307, 154, 84 }, new[] { 691, 307, 154, 84 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 84 }, new[] { 5, 127, 314, 84 }, new[] { 5, 217, 314, 84 }, new[] { 5, 307, 314, 84 } }, new int[][] { new[] { 531, 37, 314, 84 }, new[] { 531, 127, 314, 84 }, new[] { 531, 217, 314, 84 }, new[] { 531, 307, 314, 84 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 84 }, new[] { 165, 37, 154, 84 }, new[] { 5, 127, 154, 84 }, new[] { 165, 127, 154, 84 }, new[] { 5, 217, 154, 84 }, new[] { 165, 217, 154, 84 }, new[] { 5, 307, 154, 84 }, new[] { 165, 307, 154, 84 } }, new int[][] { new[] { 531, 37, 154, 84 }, new[] { 691, 37, 154, 84 }, new[] { 531, 127, 154, 84 }, new[] { 691, 127, 154, 84 }, new[] { 531, 217, 154, 84 }, new[] { 691, 217, 154, 84 }, new[] { 531, 307, 154, 84 }, new[] { 691, 307, 154, 84 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 84 }, new[] { 5, 127, 314, 84 }, new[] { 5, 217, 314, 174 } }, new int[][] { new[] { 531, 37, 314, 84 }, new[] { 531, 127, 314, 84 }, new[] { 531, 217, 314, 174 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 174 }, new[] { 5, 217, 314, 84 }, new[] { 5, 307, 314, 84 } }, new int[][] { new[] { 531, 37, 314, 174 }, new[] { 531, 217, 314, 84 }, new[] { 531, 307, 314, 84 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 264 }, new[] { 5, 307, 314, 84 } }, new int[][] { new[] { 531, 37, 314, 264 }, new[] { 531, 307, 314, 84 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 354 } }, new int[][] { new[] { 531, 37, 314, 354 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 37, 288, 78 }, new[] { 31, 121, 288, 354 } }, new int[][] { new[] { 531, 37, 288, 78 }, new[] { 531, 121, 288, 354 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 216 }, new[] { 31, 259, 288, 216 } }, new int[][] { new[] { 531, 37, 288, 216 }, new[] { 531, 259, 288, 216 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 216 }, new[] { 31, 259, 141, 216 }, new[] { 178, 259, 141, 216 } }, new int[][] { new[] { 531, 37, 288, 216 }, new[] { 531, 259, 141, 216 }, new[] { 678, 259, 141, 216 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 216 }, new[] { 178, 37, 141, 216 }, new[] { 31, 259, 141, 216 }, new[] { 178, 259, 141, 216 } }, new int[][] { new[] { 531, 37, 141, 216 }, new[] { 678, 37, 141, 216 }, new[] { 531, 259, 141, 216 }, new[] { 678, 259, 141, 216 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 216 }, new[] { 31, 259, 141, 105 }, new[] { 178, 259, 141, 105 }, new[] { 31, 370, 141, 105 }, new[] { 178, 370, 141, 105 } }, new int[][] { new[] { 531, 37, 288, 216 }, new[] { 531, 259, 141, 105 }, new[] { 678, 259, 141, 105 }, new[] { 531, 370, 141, 105 }, new[] { 678, 370, 141, 105 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 105 }, new[] { 31, 148, 288, 105 }, new[] { 31, 259, 288, 105 }, new[] { 31, 370, 288, 105 } }, new int[][] { new[] { 531, 37, 288, 105 }, new[] { 531, 148, 288, 105 }, new[] { 531, 259, 288, 105 }, new[] { 531, 370, 288, 105 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 105 }, new[] { 178, 37, 141, 105 }, new[] { 31, 148, 141, 105 }, new[] { 178, 148, 141, 105 }, new[] { 31, 259, 141, 105 }, new[] { 178, 259, 141, 105 }, new[] { 31, 370, 141, 105 }, new[] { 178, 370, 141, 105 } }, new int[][] { new[] { 531, 37, 141, 105 }, new[] { 678, 37, 141, 105 }, new[] { 531, 148, 141, 105 }, new[] { 678, 148, 141, 105 }, new[] { 531, 259, 141, 105 }, new[] { 678, 259, 141, 105 }, new[] { 531, 370, 141, 105 }, new[] { 678, 370, 141, 105 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 105 }, new[] { 31, 148, 288, 105 }, new[] { 31, 259, 288, 216 } }, new int[][] { new[] { 531, 37, 288, 105 }, new[] { 531, 148, 288, 105 }, new[] { 531, 259, 288, 216 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 216 }, new[] { 31, 259, 288, 105 }, new[] { 31, 370, 288, 105 } }, new int[][] { new[] { 531, 37, 288, 216 }, new[] { 531, 259, 288, 105 }, new[] { 531, 370, 288, 105 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 327 }, new[] { 31, 370, 288, 105 } }, new int[][] { new[] { 531, 37, 288, 327 }, new[] { 531, 370, 288, 105 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 438 } }, new int[][] { new[] { 531, 37, 288, 438 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 37, 314, 78 }, new[] { 5, 121, 314, 354 } }, new int[][] { new[] { 531, 37, 314, 78 }, new[] { 531, 121, 314, 354 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 216 }, new[] { 5, 259, 314, 216 } }, new int[][] { new[] { 531, 37, 314, 216 }, new[] { 531, 259, 314, 216 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 216 }, new[] { 5, 259, 154, 216 }, new[] { 165, 259, 154, 216 } }, new int[][] { new[] { 531, 37, 314, 216 }, new[] { 531, 259, 154, 216 }, new[] { 691, 259, 154, 216 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 216 }, new[] { 165, 37, 154, 216 }, new[] { 5, 259, 154, 216 }, new[] { 165, 259, 154, 216 } }, new int[][] { new[] { 531, 37, 154, 216 }, new[] { 691, 37, 154, 216 }, new[] { 531, 259, 154, 216 }, new[] { 691, 259, 154, 216 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 216 }, new[] { 5, 259, 154, 105 }, new[] { 165, 259, 154, 105 }, new[] { 5, 370, 154, 105 }, new[] { 165, 370, 154, 105 } }, new int[][] { new[] { 531, 37, 314, 216 }, new[] { 531, 259, 154, 105 }, new[] { 691, 259, 154, 105 }, new[] { 531, 370, 154, 105 }, new[] { 691, 370, 154, 105 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 105 }, new[] { 5, 148, 314, 105 }, new[] { 5, 259, 314, 105 }, new[] { 5, 370, 314, 105 } }, new int[][] { new[] { 531, 37, 314, 105 }, new[] { 531, 148, 314, 105 }, new[] { 531, 259, 314, 105 }, new[] { 531, 370, 314, 105 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 105 }, new[] { 165, 37, 154, 105 }, new[] { 5, 148, 154, 105 }, new[] { 165, 148, 154, 105 }, new[] { 5, 259, 154, 105 }, new[] { 165, 259, 154, 105 }, new[] { 5, 370, 154, 105 }, new[] { 165, 370, 154, 105 } }, new int[][] { new[] { 531, 37, 154, 105 }, new[] { 691, 37, 154, 105 }, new[] { 531, 148, 154, 105 }, new[] { 691, 148, 154, 105 }, new[] { 531, 259, 154, 105 }, new[] { 691, 259, 154, 105 }, new[] { 531, 370, 154, 105 }, new[] { 691, 370, 154, 105 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 105 }, new[] { 5, 148, 314, 105 }, new[] { 5, 259, 314, 216 } }, new int[][] { new[] { 531, 37, 314, 105 }, new[] { 531, 148, 314, 105 }, new[] { 531, 259, 314, 216 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 216 }, new[] { 5, 259, 314, 105 }, new[] { 5, 370, 314, 105 } }, new int[][] { new[] { 531, 37, 314, 216 }, new[] { 531, 259, 314, 105 }, new[] { 531, 370, 314, 105 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 327 }, new[] { 5, 370, 314, 105 } }, new int[][] { new[] { 531, 37, 314, 327 }, new[] { 531, 370, 314, 105 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 438 } }, new int[][] { new[] { 531, 37, 314, 438 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 37, 288, 62 }, new[] { 31, 105, 288, 218 } }, new int[][] { new[] { 531, 37, 288, 62 }, new[] { 531, 105, 288, 218 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 140 }, new[] { 31, 183, 288, 140 } }, new int[][] { new[] { 531, 37, 288, 140 }, new[] { 531, 183, 288, 140 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 140 }, new[] { 31, 183, 141, 140 }, new[] { 178, 183, 141, 140 } }, new int[][] { new[] { 531, 37, 288, 140 }, new[] { 531, 183, 141, 140 }, new[] { 678, 183, 141, 140 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 140 }, new[] { 178, 37, 141, 140 }, new[] { 31, 183, 141, 140 }, new[] { 178, 183, 141, 140 } }, new int[][] { new[] { 531, 37, 141, 140 }, new[] { 678, 37, 141, 140 }, new[] { 531, 183, 141, 140 }, new[] { 678, 183, 141, 140 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 140 }, new[] { 31, 183, 141, 67 }, new[] { 178, 183, 141, 67 }, new[] { 31, 256, 141, 67 }, new[] { 178, 256, 141, 67 } }, new int[][] { new[] { 531, 37, 288, 140 }, new[] { 531, 183, 141, 67 }, new[] { 678, 183, 141, 67 }, new[] { 531, 256, 141, 67 }, new[] { 678, 256, 141, 67 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 67 }, new[] { 31, 110, 288, 67 }, new[] { 31, 183, 288, 67 }, new[] { 31, 256, 288, 67 } }, new int[][] { new[] { 531, 37, 288, 67 }, new[] { 531, 110, 288, 67 }, new[] { 531, 183, 288, 67 }, new[] { 531, 256, 288, 67 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 67 }, new[] { 178, 37, 141, 67 }, new[] { 31, 110, 141, 67 }, new[] { 178, 110, 141, 67 }, new[] { 31, 183, 141, 67 }, new[] { 178, 183, 141, 67 }, new[] { 31, 256, 141, 67 }, new[] { 178, 256, 141, 67 } }, new int[][] { new[] { 531, 37, 141, 67 }, new[] { 678, 37, 141, 67 }, new[] { 531, 110, 141, 67 }, new[] { 678, 110, 141, 67 }, new[] { 531, 183, 141, 67 }, new[] { 678, 183, 141, 67 }, new[] { 531, 256, 141, 67 }, new[] { 678, 256, 141, 67 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 67 }, new[] { 31, 110, 288, 67 }, new[] { 31, 183, 288, 140 } }, new int[][] { new[] { 531, 37, 288, 67 }, new[] { 531, 110, 288, 67 }, new[] { 531, 183, 288, 140 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 140 }, new[] { 31, 183, 288, 67 }, new[] { 31, 256, 288, 67 } }, new int[][] { new[] { 531, 37, 288, 140 }, new[] { 531, 183, 288, 67 }, new[] { 531, 256, 288, 67 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 213 }, new[] { 31, 256, 288, 67 } }, new int[][] { new[] { 531, 37, 288, 213 }, new[] { 531, 256, 288, 67 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 286 } }, new int[][] { new[] { 531, 37, 288, 286 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 37, 314, 62 }, new[] { 5, 105, 314, 218 } }, new int[][] { new[] { 531, 37, 314, 62 }, new[] { 531, 105, 314, 218 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 140 }, new[] { 5, 183, 314, 140 } }, new int[][] { new[] { 531, 37, 314, 140 }, new[] { 531, 183, 314, 140 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 140 }, new[] { 5, 183, 154, 140 }, new[] { 165, 183, 154, 140 } }, new int[][] { new[] { 531, 37, 314, 140 }, new[] { 531, 183, 154, 140 }, new[] { 691, 183, 154, 140 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 140 }, new[] { 165, 37, 154, 140 }, new[] { 5, 183, 154, 140 }, new[] { 165, 183, 154, 140 } }, new int[][] { new[] { 531, 37, 154, 140 }, new[] { 691, 37, 154, 140 }, new[] { 531, 183, 154, 140 }, new[] { 691, 183, 154, 140 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 140 }, new[] { 5, 183, 154, 67 }, new[] { 165, 183, 154, 67 }, new[] { 5, 256, 154, 67 }, new[] { 165, 256, 154, 67 } }, new int[][] { new[] { 531, 37, 314, 140 }, new[] { 531, 183, 154, 67 }, new[] { 691, 183, 154, 67 }, new[] { 531, 256, 154, 67 }, new[] { 691, 256, 154, 67 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 67 }, new[] { 5, 110, 314, 67 }, new[] { 5, 183, 314, 67 }, new[] { 5, 256, 314, 67 } }, new int[][] { new[] { 531, 37, 314, 67 }, new[] { 531, 110, 314, 67 }, new[] { 531, 183, 314, 67 }, new[] { 531, 256, 314, 67 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 67 }, new[] { 165, 37, 154, 67 }, new[] { 5, 110, 154, 67 }, new[] { 165, 110, 154, 67 }, new[] { 5, 183, 154, 67 }, new[] { 165, 183, 154, 67 }, new[] { 5, 256, 154, 67 }, new[] { 165, 256, 154, 67 } }, new int[][] { new[] { 531, 37, 154, 67 }, new[] { 691, 37, 154, 67 }, new[] { 531, 110, 154, 67 }, new[] { 691, 110, 154, 67 }, new[] { 531, 183, 154, 67 }, new[] { 691, 183, 154, 67 }, new[] { 531, 256, 154, 67 }, new[] { 691, 256, 154, 67 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 67 }, new[] { 5, 110, 314, 67 }, new[] { 5, 183, 314, 140 } }, new int[][] { new[] { 531, 37, 314, 67 }, new[] { 531, 110, 314, 67 }, new[] { 531, 183, 314, 140 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 140 }, new[] { 5, 183, 314, 67 }, new[] { 5, 256, 314, 67 } }, new int[][] { new[] { 531, 37, 314, 140 }, new[] { 531, 183, 314, 67 }, new[] { 531, 256, 314, 67 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 213 }, new[] { 5, 256, 314, 67 } }, new int[][] { new[] { 531, 37, 314, 213 }, new[] { 531, 256, 314, 67 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 286 } }, new int[][] { new[] { 531, 37, 314, 286 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 31, 37, 288, 62 }, new[] { 31, 105, 288, 302 } }, new int[][] { new[] { 531, 37, 288, 62 }, new[] { 531, 105, 288, 302 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 182 }, new[] { 31, 225, 288, 182 } }, new int[][] { new[] { 531, 37, 288, 182 }, new[] { 531, 225, 288, 182 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 182 }, new[] { 31, 225, 141, 182 }, new[] { 178, 225, 141, 182 } }, new int[][] { new[] { 531, 37, 288, 182 }, new[] { 531, 225, 141, 182 }, new[] { 678, 225, 141, 182 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 182 }, new[] { 178, 37, 141, 182 }, new[] { 31, 225, 141, 182 }, new[] { 178, 225, 141, 182 } }, new int[][] { new[] { 531, 37, 141, 182 }, new[] { 678, 37, 141, 182 }, new[] { 531, 225, 141, 182 }, new[] { 678, 225, 141, 182 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 182 }, new[] { 31, 225, 141, 88 }, new[] { 178, 225, 141, 88 }, new[] { 31, 319, 141, 88 }, new[] { 178, 319, 141, 88 } }, new int[][] { new[] { 531, 37, 288, 182 }, new[] { 531, 225, 141, 88 }, new[] { 678, 225, 141, 88 }, new[] { 531, 319, 141, 88 }, new[] { 678, 319, 141, 88 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 88 }, new[] { 31, 131, 288, 88 }, new[] { 31, 225, 288, 88 }, new[] { 31, 319, 288, 88 } }, new int[][] { new[] { 531, 37, 288, 88 }, new[] { 531, 131, 288, 88 }, new[] { 531, 225, 288, 88 }, new[] { 531, 319, 288, 88 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 141, 88 }, new[] { 178, 37, 141, 88 }, new[] { 31, 131, 141, 88 }, new[] { 178, 131, 141, 88 }, new[] { 31, 225, 141, 88 }, new[] { 178, 225, 141, 88 }, new[] { 31, 319, 141, 88 }, new[] { 178, 319, 141, 88 } }, new int[][] { new[] { 531, 37, 141, 88 }, new[] { 678, 37, 141, 88 }, new[] { 531, 131, 141, 88 }, new[] { 678, 131, 141, 88 }, new[] { 531, 225, 141, 88 }, new[] { 678, 225, 141, 88 }, new[] { 531, 319, 141, 88 }, new[] { 678, 319, 141, 88 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 88 }, new[] { 31, 131, 288, 88 }, new[] { 31, 225, 288, 182 } }, new int[][] { new[] { 531, 37, 288, 88 }, new[] { 531, 131, 288, 88 }, new[] { 531, 225, 288, 182 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 182 }, new[] { 31, 225, 288, 88 }, new[] { 31, 319, 288, 88 } }, new int[][] { new[] { 531, 37, 288, 182 }, new[] { 531, 225, 288, 88 }, new[] { 531, 319, 288, 88 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 276 }, new[] { 31, 319, 288, 88 } }, new int[][] { new[] { 531, 37, 288, 276 }, new[] { 531, 319, 288, 88 } } },
                new int[][][] { new int[][] { new[] { 31, 37, 288, 370 } }, new int[][] { new[] { 531, 37, 288, 370 } } },
            },
            new int[][][][] {
                new int[][][] { new int[][] { new[] { 5, 37, 314, 62 }, new[] { 5, 105, 314, 302 } }, new int[][] { new[] { 531, 37, 314, 62 }, new[] { 531, 105, 314, 302 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 182 }, new[] { 5, 225, 314, 182 } }, new int[][] { new[] { 531, 37, 314, 182 }, new[] { 531, 225, 314, 182 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 182 }, new[] { 5, 225, 154, 182 }, new[] { 165, 225, 154, 182 } }, new int[][] { new[] { 531, 37, 314, 182 }, new[] { 531, 225, 154, 182 }, new[] { 691, 225, 154, 182 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 182 }, new[] { 165, 37, 154, 182 }, new[] { 5, 225, 154, 182 }, new[] { 165, 225, 154, 182 } }, new int[][] { new[] { 531, 37, 154, 182 }, new[] { 691, 37, 154, 182 }, new[] { 531, 225, 154, 182 }, new[] { 691, 225, 154, 182 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 182 }, new[] { 5, 225, 154, 88 }, new[] { 165, 225, 154, 88 }, new[] { 5, 319, 154, 88 }, new[] { 165, 319, 154, 88 } }, new int[][] { new[] { 531, 37, 314, 182 }, new[] { 531, 225, 154, 88 }, new[] { 691, 225, 154, 88 }, new[] { 531, 319, 154, 88 }, new[] { 691, 319, 154, 88 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 88 }, new[] { 5, 131, 314, 88 }, new[] { 5, 225, 314, 88 }, new[] { 5, 319, 314, 88 } }, new int[][] { new[] { 531, 37, 314, 88 }, new[] { 531, 131, 314, 88 }, new[] { 531, 225, 314, 88 }, new[] { 531, 319, 314, 88 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 154, 88 }, new[] { 165, 37, 154, 88 }, new[] { 5, 131, 154, 88 }, new[] { 165, 131, 154, 88 }, new[] { 5, 225, 154, 88 }, new[] { 165, 225, 154, 88 }, new[] { 5, 319, 154, 88 }, new[] { 165, 319, 154, 88 } }, new int[][] { new[] { 531, 37, 154, 88 }, new[] { 691, 37, 154, 88 }, new[] { 531, 131, 154, 88 }, new[] { 691, 131, 154, 88 }, new[] { 531, 225, 154, 88 }, new[] { 691, 225, 154, 88 }, new[] { 531, 319, 154, 88 }, new[] { 691, 319, 154, 88 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 88 }, new[] { 5, 131, 314, 88 }, new[] { 5, 225, 314, 182 } }, new int[][] { new[] { 531, 37, 314, 88 }, new[] { 531, 131, 314, 88 }, new[] { 531, 225, 314, 182 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 182 }, new[] { 5, 225, 314, 88 }, new[] { 5, 319, 314, 88 } }, new int[][] { new[] { 531, 37, 314, 182 }, new[] { 531, 225, 314, 88 }, new[] { 531, 319, 314, 88 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 276 }, new[] { 5, 319, 314, 88 } }, new int[][] { new[] { 531, 37, 314, 276 }, new[] { 531, 319, 314, 88 } } },
                new int[][][] { new int[][] { new[] { 5, 37, 314, 370 } }, new int[][] { new[] { 531, 37, 314, 370 } } },
            },
        };
        // ToolbarRects[cornice][modello] (vuoto se la cornice non ha barra inferiore)
        public static readonly int[][][][] ToolbarRects = new int[][][][]
        {
            new int[][][] { new int[][] { new[] { 33, 397, 152, 78 }, new[] { 191, 397, 152, 78 }, new[] { 349, 397, 152, 78 }, new[] { 507, 397, 152, 78 }, new[] { 665, 397, 152, 78 } }, new int[][] { new[] { 32, 397, 126, 78 }, new[] { 164, 397, 126, 78 }, new[] { 296, 397, 126, 78 }, new[] { 428, 397, 126, 78 }, new[] { 560, 397, 126, 78 }, new[] { 692, 397, 126, 78 } }, new int[][] { new[] { 32, 397, 192, 78 }, new[] { 230, 397, 192, 78 }, new[] { 428, 397, 192, 78 }, new[] { 626, 397, 192, 78 } }, new int[][] { new[] { 32, 397, 258, 78 }, new[] { 296, 397, 258, 78 }, new[] { 560, 397, 258, 78 } }, new int[][] { new[] { 31, 397, 788, 78 } } },
            new int[][][] { new int[][] { new[] { 5, 397, 163, 78 }, new[] { 174, 397, 163, 78 }, new[] { 343, 397, 163, 78 }, new[] { 512, 397, 163, 78 }, new[] { 681, 397, 163, 78 } }, new int[][] { new[] { 5, 397, 135, 78 }, new[] { 146, 397, 135, 78 }, new[] { 287, 397, 135, 78 }, new[] { 428, 397, 135, 78 }, new[] { 569, 397, 135, 78 }, new[] { 710, 397, 135, 78 } }, new int[][] { new[] { 6, 397, 205, 78 }, new[] { 217, 397, 205, 78 }, new[] { 428, 397, 205, 78 }, new[] { 639, 397, 205, 78 } }, new int[][] { new[] { 5, 397, 276, 78 }, new[] { 287, 397, 276, 78 }, new[] { 569, 397, 276, 78 } }, new int[][] { new[] { 5, 397, 840, 78 } } },
            new int[][][] { new int[0][], new int[0][], new int[0][], new int[0][], new int[0][] },
            new int[][][] { new int[0][], new int[0][], new int[0][], new int[0][], new int[0][] },
            new int[][][] { new int[][] { new[] { 33, 329, 152, 62 }, new[] { 191, 329, 152, 62 }, new[] { 349, 329, 152, 62 }, new[] { 507, 329, 152, 62 }, new[] { 665, 329, 152, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 329, 126, 62 }, new[] { 164, 329, 126, 62 }, new[] { 296, 329, 126, 62 }, new[] { 428, 329, 126, 62 }, new[] { 560, 329, 126, 62 }, new[] { 692, 329, 126, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 329, 192, 62 }, new[] { 230, 329, 192, 62 }, new[] { 428, 329, 192, 62 }, new[] { 626, 329, 192, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 329, 258, 62 }, new[] { 296, 329, 258, 62 }, new[] { 560, 329, 258, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 31, 329, 788, 62 }, new[] { 31, 397, 788, 78 } } },
            new int[][][] { new int[][] { new[] { 5, 329, 163, 62 }, new[] { 174, 329, 163, 62 }, new[] { 343, 329, 163, 62 }, new[] { 512, 329, 163, 62 }, new[] { 681, 329, 163, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 329, 135, 62 }, new[] { 146, 329, 135, 62 }, new[] { 287, 329, 135, 62 }, new[] { 428, 329, 135, 62 }, new[] { 569, 329, 135, 62 }, new[] { 710, 329, 135, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 6, 329, 205, 62 }, new[] { 217, 329, 205, 62 }, new[] { 428, 329, 205, 62 }, new[] { 639, 329, 205, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 329, 276, 62 }, new[] { 287, 329, 276, 62 }, new[] { 569, 329, 276, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 329, 840, 62 }, new[] { 5, 397, 840, 78 } } },
            new int[][][] { new int[][] { new[] { 33, 413, 152, 62 }, new[] { 191, 413, 152, 62 }, new[] { 349, 413, 152, 62 }, new[] { 507, 413, 152, 62 }, new[] { 665, 413, 152, 62 } }, new int[][] { new[] { 32, 413, 126, 62 }, new[] { 164, 413, 126, 62 }, new[] { 296, 413, 126, 62 }, new[] { 428, 413, 126, 62 }, new[] { 560, 413, 126, 62 }, new[] { 692, 413, 126, 62 } }, new int[][] { new[] { 32, 413, 192, 62 }, new[] { 230, 413, 192, 62 }, new[] { 428, 413, 192, 62 }, new[] { 626, 413, 192, 62 } }, new int[][] { new[] { 32, 413, 258, 62 }, new[] { 296, 413, 258, 62 }, new[] { 560, 413, 258, 62 } }, new int[][] { new[] { 31, 413, 788, 62 } } },
            new int[][][] { new int[][] { new[] { 5, 413, 163, 62 }, new[] { 174, 413, 163, 62 }, new[] { 343, 413, 163, 62 }, new[] { 512, 413, 163, 62 }, new[] { 681, 413, 163, 62 } }, new int[][] { new[] { 5, 413, 135, 62 }, new[] { 146, 413, 135, 62 }, new[] { 287, 413, 135, 62 }, new[] { 428, 413, 135, 62 }, new[] { 569, 413, 135, 62 }, new[] { 710, 413, 135, 62 } }, new int[][] { new[] { 6, 413, 205, 62 }, new[] { 217, 413, 205, 62 }, new[] { 428, 413, 205, 62 }, new[] { 639, 413, 205, 62 } }, new int[][] { new[] { 5, 413, 276, 62 }, new[] { 287, 413, 276, 62 }, new[] { 569, 413, 276, 62 } }, new int[][] { new[] { 5, 413, 840, 62 } } },
            new int[][][] { new int[][] { new[] { 33, 397, 152, 78 }, new[] { 191, 397, 152, 78 }, new[] { 349, 397, 152, 78 }, new[] { 507, 397, 152, 78 }, new[] { 665, 397, 152, 78 } }, new int[][] { new[] { 32, 397, 126, 78 }, new[] { 164, 397, 126, 78 }, new[] { 296, 397, 126, 78 }, new[] { 428, 397, 126, 78 }, new[] { 560, 397, 126, 78 }, new[] { 692, 397, 126, 78 } }, new int[][] { new[] { 32, 397, 192, 78 }, new[] { 230, 397, 192, 78 }, new[] { 428, 397, 192, 78 }, new[] { 626, 397, 192, 78 } }, new int[][] { new[] { 32, 397, 258, 78 }, new[] { 296, 397, 258, 78 }, new[] { 560, 397, 258, 78 } }, new int[][] { new[] { 31, 397, 788, 78 } } },
            new int[][][] { new int[][] { new[] { 5, 397, 163, 78 }, new[] { 174, 397, 163, 78 }, new[] { 343, 397, 163, 78 }, new[] { 512, 397, 163, 78 }, new[] { 681, 397, 163, 78 } }, new int[][] { new[] { 5, 397, 135, 78 }, new[] { 146, 397, 135, 78 }, new[] { 287, 397, 135, 78 }, new[] { 428, 397, 135, 78 }, new[] { 569, 397, 135, 78 }, new[] { 710, 397, 135, 78 } }, new int[][] { new[] { 6, 397, 205, 78 }, new[] { 217, 397, 205, 78 }, new[] { 428, 397, 205, 78 }, new[] { 639, 397, 205, 78 } }, new int[][] { new[] { 5, 397, 276, 78 }, new[] { 287, 397, 276, 78 }, new[] { 569, 397, 276, 78 } }, new int[][] { new[] { 5, 397, 840, 78 } } },
            new int[][][] { new int[0][], new int[0][], new int[0][], new int[0][], new int[0][] },
            new int[][][] { new int[0][], new int[0][], new int[0][], new int[0][], new int[0][] },
            new int[][][] { new int[][] { new[] { 33, 329, 152, 62 }, new[] { 191, 329, 152, 62 }, new[] { 349, 329, 152, 62 }, new[] { 507, 329, 152, 62 }, new[] { 665, 329, 152, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 329, 126, 62 }, new[] { 164, 329, 126, 62 }, new[] { 296, 329, 126, 62 }, new[] { 428, 329, 126, 62 }, new[] { 560, 329, 126, 62 }, new[] { 692, 329, 126, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 329, 192, 62 }, new[] { 230, 329, 192, 62 }, new[] { 428, 329, 192, 62 }, new[] { 626, 329, 192, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 32, 329, 258, 62 }, new[] { 296, 329, 258, 62 }, new[] { 560, 329, 258, 62 }, new[] { 31, 397, 788, 78 } }, new int[][] { new[] { 31, 329, 788, 62 }, new[] { 31, 397, 788, 78 } } },
            new int[][][] { new int[][] { new[] { 5, 329, 163, 62 }, new[] { 174, 329, 163, 62 }, new[] { 343, 329, 163, 62 }, new[] { 512, 329, 163, 62 }, new[] { 681, 329, 163, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 329, 135, 62 }, new[] { 146, 329, 135, 62 }, new[] { 287, 329, 135, 62 }, new[] { 428, 329, 135, 62 }, new[] { 569, 329, 135, 62 }, new[] { 710, 329, 135, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 6, 329, 205, 62 }, new[] { 217, 329, 205, 62 }, new[] { 428, 329, 205, 62 }, new[] { 639, 329, 205, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 329, 276, 62 }, new[] { 287, 329, 276, 62 }, new[] { 569, 329, 276, 62 }, new[] { 5, 397, 840, 78 } }, new int[][] { new[] { 5, 329, 840, 62 }, new[] { 5, 397, 840, 78 } } },
            new int[][][] { new int[][] { new[] { 33, 413, 152, 62 }, new[] { 191, 413, 152, 62 }, new[] { 349, 413, 152, 62 }, new[] { 507, 413, 152, 62 }, new[] { 665, 413, 152, 62 } }, new int[][] { new[] { 32, 413, 126, 62 }, new[] { 164, 413, 126, 62 }, new[] { 296, 413, 126, 62 }, new[] { 428, 413, 126, 62 }, new[] { 560, 413, 126, 62 }, new[] { 692, 413, 126, 62 } }, new int[][] { new[] { 32, 413, 192, 62 }, new[] { 230, 413, 192, 62 }, new[] { 428, 413, 192, 62 }, new[] { 626, 413, 192, 62 } }, new int[][] { new[] { 32, 413, 258, 62 }, new[] { 296, 413, 258, 62 }, new[] { 560, 413, 258, 62 } }, new int[][] { new[] { 31, 413, 788, 62 } } },
            new int[][][] { new int[][] { new[] { 5, 413, 163, 62 }, new[] { 174, 413, 163, 62 }, new[] { 343, 413, 163, 62 }, new[] { 512, 413, 163, 62 }, new[] { 681, 413, 163, 62 } }, new int[][] { new[] { 5, 413, 135, 62 }, new[] { 146, 413, 135, 62 }, new[] { 287, 413, 135, 62 }, new[] { 428, 413, 135, 62 }, new[] { 569, 413, 135, 62 }, new[] { 710, 413, 135, 62 } }, new int[][] { new[] { 6, 413, 205, 62 }, new[] { 217, 413, 205, 62 }, new[] { 428, 413, 205, 62 }, new[] { 639, 413, 205, 62 } }, new int[][] { new[] { 5, 413, 276, 62 }, new[] { 287, 413, 276, 62 }, new[] { 569, 413, 276, 62 } }, new int[][] { new[] { 5, 413, 840, 62 } } },
        };
        // SidebarRects[cornice] (vuoto se senza barre laterali)
        public static readonly int[][][] SidebarRects = new int[][][]
        {
            new int[][] { new[] { 5, 5, 20, 386 }, new[] { 825, 5, 20, 386 } },
            new int[0][],
            new int[][] { new[] { 5, 5, 20, 470 }, new[] { 825, 5, 20, 470 } },
            new int[0][],
            new int[][] { new[] { 5, 5, 20, 318 }, new[] { 825, 5, 20, 318 } },
            new int[0][],
            new int[][] { new[] { 5, 5, 20, 402 }, new[] { 825, 5, 20, 402 } },
            new int[0][],
            new int[][] { new[] { 5, 37, 20, 354 }, new[] { 825, 37, 20, 354 } },
            new int[0][],
            new int[][] { new[] { 5, 37, 20, 438 }, new[] { 825, 37, 20, 438 } },
            new int[0][],
            new int[][] { new[] { 5, 37, 20, 286 }, new[] { 825, 37, 20, 286 } },
            new int[0][],
            new int[][] { new[] { 5, 37, 20, 370 }, new[] { 825, 37, 20, 370 } },
            new int[0][],
        };
        // CenterRects[cornice][centro] = { delta, marcia, velocita', riga1, riga2, ... }
        public static readonly int[][][][] CenterRects = new int[][][][]
        {
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 204 }, new[] { 333, 175, 184, 40 }, new[] { 325, 271, 200, 36 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 246 }, new[] { 333, 198, 184, 40 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 288 }, new[] { 333, 222, 184, 40 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 330 }, new[] { 333, 245, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 204 }, new[] { 333, 175, 184, 40 }, new[] { 325, 271, 200, 36 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 246 }, new[] { 333, 198, 184, 40 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 288 }, new[] { 333, 222, 184, 40 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 330 }, new[] { 333, 245, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 288 }, new[] { 333, 222, 184, 40 }, new[] { 325, 355, 200, 36 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 330 }, new[] { 333, 245, 184, 40 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 372 }, new[] { 333, 269, 184, 40 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 414 }, new[] { 333, 292, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 288 }, new[] { 333, 222, 184, 40 }, new[] { 325, 355, 200, 36 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 330 }, new[] { 333, 245, 184, 40 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 372 }, new[] { 333, 269, 184, 40 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 414 }, new[] { 333, 292, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 136 }, new[] { 333, 137, 184, 40 }, new[] { 325, 203, 200, 36 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 178 }, new[] { 333, 160, 184, 40 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 220 }, new[] { 333, 184, 184, 40 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 262 }, new[] { 333, 207, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 136 }, new[] { 333, 137, 184, 40 }, new[] { 325, 203, 200, 36 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 178 }, new[] { 333, 160, 184, 40 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 220 }, new[] { 333, 184, 184, 40 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 262 }, new[] { 333, 207, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 220 }, new[] { 333, 184, 184, 40 }, new[] { 325, 287, 200, 36 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 262 }, new[] { 333, 207, 184, 40 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 304 }, new[] { 333, 231, 184, 40 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 346 }, new[] { 333, 254, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 220 }, new[] { 333, 184, 184, 40 }, new[] { 325, 287, 200, 36 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 262 }, new[] { 333, 207, 184, 40 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 304 }, new[] { 333, 231, 184, 40 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 5, 200, 50 }, new[] { 325, 61, 200, 346 }, new[] { 333, 254, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 172 }, new[] { 333, 189, 184, 40 }, new[] { 325, 271, 200, 36 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 214 }, new[] { 333, 212, 184, 40 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 256 }, new[] { 333, 236, 184, 40 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 298 }, new[] { 333, 259, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 172 }, new[] { 333, 189, 184, 40 }, new[] { 325, 271, 200, 36 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 214 }, new[] { 333, 212, 184, 40 }, new[] { 325, 313, 200, 36 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 256 }, new[] { 333, 236, 184, 40 }, new[] { 325, 355, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 298 }, new[] { 333, 259, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 256 }, new[] { 333, 236, 184, 40 }, new[] { 325, 355, 200, 36 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 298 }, new[] { 333, 259, 184, 40 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 340 }, new[] { 333, 283, 184, 40 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 382 }, new[] { 333, 306, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 256 }, new[] { 333, 236, 184, 40 }, new[] { 325, 355, 200, 36 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 298 }, new[] { 333, 259, 184, 40 }, new[] { 325, 397, 200, 36 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 340 }, new[] { 333, 283, 184, 40 }, new[] { 325, 439, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 382 }, new[] { 333, 306, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 104 }, new[] { 333, 151, 184, 40 }, new[] { 325, 203, 200, 36 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 146 }, new[] { 333, 174, 184, 40 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 188 }, new[] { 333, 198, 184, 40 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 230 }, new[] { 333, 221, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 104 }, new[] { 333, 151, 184, 40 }, new[] { 325, 203, 200, 36 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 146 }, new[] { 333, 174, 184, 40 }, new[] { 325, 245, 200, 36 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 188 }, new[] { 333, 198, 184, 40 }, new[] { 325, 287, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 230 }, new[] { 333, 221, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 188 }, new[] { 333, 198, 184, 40 }, new[] { 325, 287, 200, 36 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 230 }, new[] { 333, 221, 184, 40 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 272 }, new[] { 333, 245, 184, 40 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 314 }, new[] { 333, 268, 184, 40 } } },
            new int[][][] { new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 188 }, new[] { 333, 198, 184, 40 }, new[] { 325, 287, 200, 36 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 230 }, new[] { 333, 221, 184, 40 }, new[] { 325, 329, 200, 36 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 272 }, new[] { 333, 245, 184, 40 }, new[] { 325, 371, 200, 36 } }, new int[][] { new[] { 325, 37, 200, 50 }, new[] { 325, 93, 200, 314 }, new[] { 333, 268, 184, 40 } } },
        };
        public static readonly string[] CenterLayouts = new string[] { "5 blocchi", "4 blocchi", "3 blocchi", "2 blocchi" };

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

        // DT LapBox: schermo 1050x656, blocchi possibili {x, y, w, h} e per ogni griglia i blocchi usati (in ordine di riquadro)
        public const int LapBoxWidth = 1050;
        public const int LapBoxHeight = 656;
        public static readonly int[][] LapBoxCells = new int[][] { new[] { 14, 88, 508, 117 }, new[] { 528, 88, 508, 117 }, new[] { 14, 211, 508, 117 }, new[] { 528, 211, 508, 117 }, new[] { 14, 334, 508, 117 }, new[] { 528, 334, 508, 117 }, new[] { 14, 457, 508, 117 }, new[] { 528, 457, 508, 117 }, new[] { 14, 88, 508, 240 }, new[] { 528, 88, 508, 240 }, new[] { 14, 334, 508, 240 }, new[] { 528, 334, 508, 240 }, new[] { 14, 88, 1022, 240 }, new[] { 14, 334, 1022, 240 }, new[] { 14, 88, 508, 486 }, new[] { 528, 88, 508, 486 }, new[] { 14, 88, 1022, 486 } };
        public static readonly int[][] LapBoxGridCells = new int[][] { new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new[] { 8, 9, 10, 11 }, new[] { 12, 13 }, new[] { 14, 15 }, new[] { 16 } };
        public static readonly string[] LapBoxGrids = new string[] { "8 blocchi", "4 blocchi", "2 blocchi sopra e sotto", "2 blocchi affiancati", "1 blocco" };
        public static readonly int[][][] LapBoxGridUnits = new int[][][] { new int[][] { new[] { 0, 0, 1, 1 }, new[] { 1, 0, 1, 1 }, new[] { 0, 1, 1, 1 }, new[] { 1, 1, 1, 1 }, new[] { 0, 2, 1, 1 }, new[] { 1, 2, 1, 1 }, new[] { 0, 3, 1, 1 }, new[] { 1, 3, 1, 1 } }, new int[][] { new[] { 0, 0, 1, 2 }, new[] { 1, 0, 1, 2 }, new[] { 0, 2, 1, 2 }, new[] { 1, 2, 1, 2 } }, new int[][] { new[] { 0, 0, 2, 2 }, new[] { 0, 2, 2, 2 } }, new int[][] { new[] { 0, 0, 1, 4 }, new[] { 1, 0, 1, 4 } }, new int[][] { new[] { 0, 0, 2, 4 } } };
        public static readonly int[][] LapBoxLights = new int[][] { new[] { 253, 0, 140, 70 }, new[] { 635, 0, 140, 70 } };

        public static List<PageSettings> DefaultLapBoxPages()
        {
            var list = new List<PageSettings>();
            list.Add(new PageSettings { Name = "Gara", GridLeft = 1, Left = new List<int> { 13, 5, 50, 8, 0, 0, 0, 0 } });
            list.Add(new PageSettings { Name = "Prove", GridLeft = 0, Left = new List<int> { 22, 23, 13, 24, 14, 16, 17, 18 } });
            return list;
        }
    }

    public class ChannelDef
    {
        public readonly string Id, Label, Source, Format, Unit, Sample;
        public readonly int Decimals;
        public ChannelDef(string id, string label, string source, string format, int decimals, string unit, string sample = "")
        {
            Id = id; Label = label; Source = source; Format = format; Decimals = decimals; Unit = unit; Sample = sample;
        }
    }
}
