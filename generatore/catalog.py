# -*- coding: utf-8 -*-
"""Catalogo unico condiviso fra la dashboard (djson) e il plugin (C#).
Tutto cio' che la dash mostra passa da proprieta' DTPlugin.*  ->  nessun altro plugin necessario.
"""

# ---------------------------------------------------------------- canali dati
# id, etichetta, proprieta' SimHub sorgente (default), formato, decimali, unita', larghezza max (caratteri), valore esempio
# formati: num | signed | laptime | duration | clock | gap | text
# sorgenti speciali calcolate dal plugin: @ora  @gap_avanti  @gap_dietro
GD = "DataCorePlugin.GameData."
CHANNELS = [
    ("Gear",      "MARCIA",     GD + "Gear",                      "text",     0, "",  2, "4"),
    ("Rpm",       "GIRI",       GD + "Rpms",                      "num",      0, "",  5, "7250"),
    ("RpmPct",    "GIRI %",     GD + "CarSettings_CurrentDisplayedRPMPercent", "num", 0, "", 3, "78"),
    ("Speed",     "VELOCITÀ",   GD + "SpeedLocal",                "num",      0, "",  3, "214"),
    ("Throttle",  "GAS",        GD + "Throttle",                  "num",      0, "",  3, "86"),
    ("Brake",     "FRENO",      GD + "Brake",                     "num",      0, "",  3, "12"),
    ("Clutch",    "FRIZIONE",   GD + "Clutch",                    "num",      0, "",  3, "0"),
    ("TC",        "TC",         GD + "TCLevel",                   "num",      0, "",  2, "3"),
    ("TCCut",     "TC CUT",     "@raw:PlayerNativeTelemetry.mTCCut", "num", 0, "",  2, "0"),
    ("ABS",       "ABS",        GD + "ABSLevel",                  "num",      0, "",  2, "2"),
    ("BB",        "BB",         GD + "BrakeBias",                 "num",      1, "",  4, "56.2"),
    ("Map",       "MAP",        GD + "EngineMap",                 "num",      0, "",  2, "1"),
    ("Fuel",      "LITRI",      GD + "Fuel",                      "num",      1, "",  5, "14.0"),
    ("FuelPct",   "CARB. %",    GD + "FuelPercent",               "num",      0, "%", 4, "38%"),
    ("FuelPerLap","MEDIA",      "DataCorePlugin.Computed.Fuel_LitersPerLap",  "num", 2, "", 4, "2.69"),
    ("FuelLaps",  "GIRI CARB.", "@giri_carb",                 "num", 1, "", 4, "5.4"),
    ("Pos",       "POS",        "@pos",                           "num",      0, "",  5, "11/25"),
    ("Cars",      "AUTO",       "@auto",                          "num",      0, "",  3, "25"),
    ("PosGain",   "POSIZIONI",  "@pos_guadagno",                  "text",     0, "",  3, "3"),
    ("Lap",       "GIRO",       GD + "CurrentLap",                "num",      0, "",  3, "7"),
    ("TotalLaps", "GIRI TOT.",  GD + "TotalLaps",                 "num",      0, "",  3, "24"),
    ("TimeLeft",  "TEMPO",      GD + "SessionTimeLeft",           "duration", 0, "",  7, "07:52"),
    ("Clock",     "ORA",        "@ora",                           "clock",    0, "",  5, "12:13"),
    ("LastLap",   "ULTIMO",     GD + "LastLapTime",               "laptime",  0, "",  8, "2:04.572"),
    ("BestLap",   "MIGLIORE",   GD + "BestLapTime",               "laptime",  0, "",  8, "1:44.605"),
    ("EstLap",    "PREVISTO",   "@stimato",                       "laptime",  0, "",  8, "1:44.535"),
    ("CurLap",    "ATTUALE",    GD + "CurrentLapTime",            "laptime",  0, "",  8, "1:12.408"),
    ("Delta",     "DELTA",      "@delta",                         "signed",   3, "",  6, "-0.070"),
    ("GapAhead",  "AVANTI",     "@gap_avanti",                    "gap",      1, "",  6, "1.4"),
    ("GapBehind", "DIETRO",     "@gap_dietro",                    "gap",      1, "",  6, "0.8"),
    ("AirTemp",   "ARIA",       GD + "AirTemperature",            "num",      0, "°", 4, "30°"),
    ("TrackTemp", "PISTA",      GD + "RoadTemperature",           "num",      0, "°", 4, "38°"),
    ("WaterTemp", "ACQUA",      GD + "WaterTemperature",          "num",      0, "°", 4, "88°"),
    ("OilTemp",   "OLIO",       GD + "OilTemperature",            "num",      0, "°", 4, "102°"),
    ("ERS",       "ERS",        "@ers",                           "num",      0, "%", 4, "64%"),
]
WHEELS = [("FL", "FrontLeft", "AS"), ("FR", "FrontRight", "AD"), ("RL", "RearLeft", "PS"), ("RR", "RearRight", "PD")]
_tp = {"FL": "161.7", "FR": "163.2", "RL": "167.1", "RR": "166.0"}
_tt = {"FL": "84°", "FR": "88°", "RL": "83°", "RR": "87°"}
_tw = {"FL": "97%", "FR": "95%", "RL": "98%", "RR": "96%"}
_bt = {"FL": "425°", "FR": "418°", "RL": "310°", "RR": "305°"}
for k, prop, it in WHEELS:
    CHANNELS.append(("TyreP" + k, "PRESS. " + it, "@press_" + k,    "num", 1, "",  5, _tp[k]))
for k, prop, it in WHEELS:
    CHANNELS.append(("TyreT" + k, "TEMP. " + it,  "@gomma_" + k, "num", 0, "°", 4, _tt[k]))
for k, prop, it in WHEELS:
    CHANNELS.append(("TyreW" + k, "USURA " + it,  GD + "TyreWear" + prop,        "num", 0, "%", 4, _tw[k]))
for k, prop, it in WHEELS:
    CHANNELS.append(("BrakeT" + k, "FRENO " + it, GD + "BrakeTemperature" + prop,"num", 0, "°", 4, _bt[k]))
CHANNELS += [
    ("FlagYellow", "GIALLA",     GD + "Flag_Yellow",  "num", 0, "", 1, "0"),
    ("FlagBlue",   "BLU",        GD + "Flag_Blue",    "num", 0, "", 1, "0"),
    ("PitLimiter", "LIMITATORE", "@limitatore",       "num", 0, "", 1, "0"),
    ("InPit",      "CORSIA BOX", GD + "IsInPitLane",  "num", 0, "", 1, "0"),
    ("LastLapDiff","DIFF. GIRO", "@delta_ultimo",     "signed", 3, "", 7, "+0.155"),
    ("FuelLastLap","CONSUMO GIRO", "@consumo_ultimo", "num", 2, "", 5, "2.71"),
    ("VE",         "ENERGIA V.",  "@ve",              "num", 1, "%", 5, "64.2%"),
    ("VELaps",     "GIRI VE",     "@ve_giri",         "num", 1, "",  4, "5.2"),
    ("VEPerLap",   "VE / GIRO",   "@ve_media",        "num", 2, "%", 5, "12.31%"),
    ("VELastLap",  "VE ULT. GIRO","@ve_ultimo",       "num", 2, "%", 5, "12.40%"),
    ("TCSlip",     "TC SLIP",     "@raw:PlayerNativeTelemetry.mTCSlip", "num", 0, "", 2, "4"),
    ("TrackWet",   "PISTA BAGN.", "@bagnato",         "num", 0, "%", 4, "0%"),
    ("Rain",       "PIOGGIA",     "@pioggia",         "num", 0, "%", 4, "0%"),
]
# dati del server interno di LMU (sosta, danni, freni, gomme nuove, track limits, batteria, cielo)
CHANNELS += [
    ("RepDamage", "DANNI",  "@rip_danni",   "num", 1, "s", 6, "12.5s"),
    ("RepBrakes", "FRENI",        "@rip_freni",   "num", 1, "s", 6, "0.0s"),
    ("RepTyres",  "GOMME",        "@rip_gomme",   "num", 1, "s", 6, "8.0s"),
    ("RepFuel",   "CARB.", "@rip_carb",    "num", 1, "s", 6, "21.4s"),
    ("RepTotal",  "SOSTA",        "@rip_totale",  "num", 1, "s", 6, "41.9s"),
]
_su = {"FL": "0%", "FR": "0%", "RL": "12%", "RR": "0%"}
for k, prop, it in WHEELS:
    CHANNELS.append(("Susp" + k, "SOSP. " + it, "@sosp_" + k, "num", 0, "%", 4, _su[k]))
_bw = {"FL": "92%", "FR": "93%", "RL": "96%", "RR": "96%"}
for k, prop, it in WHEELS:
    CHANNELS.append(("BrakeW" + k, "FRENO " + it, "@usura_freno_" + k, "num", 0, "%", 4, _bw[k]))
CHANNELS += [
    ("AeroDmg",   "AERO",          "@danno_aero",   "num", 0, "%", 4, "0%"),
    ("EngineDmg", "MOTORE",        "@danno_motore", "num", 0, "%", 4, "0%"),
    ("NewTyres",  "GOMME NUOVE",   "@gomme_nuove",  "num", 0, "",  2, "8"),
    ("TrackLim",  "TRACK LIMITS",  "@track_limits", "text", 0, "", 5, "1/4"),
    ("Battery",   "BATTERIA",      "@batteria",     "num", 0, "%", 4, "78%"),
    ("Sky",       "CIELO",         "@cielo",        "text", 0, "", 13, "Nuvoloso"),
]
# calcoli come TinyPedal: consumo in tempo reale, risparmio, rifornimento impostato, gomme, posizione dopo il box
CHANNELS += [
    ("FuelLive",    "CONSUMO STIMATO", "@carb_stima",   "num",    2, "",  5, "2.64"),
    ("FuelDelta",   "CONS. VS ULTIMO", "@carb_delta",   "signed", 2, "",  6, "-0.12"),
    ("VELive",      "VE STIMATA",      "@ve_stima",     "num",    2, "%", 6, "11.85%"),
    ("SavePlus1",   "PER +1 GIRO",     "@giro_piu",     "text",   0, "",  7, "2.52 l"),
    ("SaveStop",    "1 SOSTA IN MENO", "@sosta_meno",   "text",   0, "",  7, "2.41 l"),
    ("Refill",      "RIFORNIMENTO",    "@rifornimento", "text",   0, "",  6, "65 %"),
    ("TyreWearLap", "USURA / GIRO",    "@usura_giro",   "num",    1, "%", 5, "1.8%"),
    ("TyreLaps",    "GIRI GOMME",      "@giri_gomme",   "num",    0, "",  3, "24"),
    ("PitRejoin",   "DOPO IL BOX",     "@pos_dopo_box", "text",   0, "",  4, "P7"),
]
for i in range(1, 7):
    CHANNELS.append(("Custom%d" % i, "PERS. %d" % i, "", "num", 1, "", 6, "-"))

CH = {c[0]: c for c in CHANNELS}

# ---------------------------------------------------------------- colori
WHITE = "#FFFFFFFF"
GREY = "#FF9E9E9E"
FRAME = "#FFBDBDBD"
CYAN = "#FF00B0FF"
YELLOW = "#FFFFD600"
RED = "#FFFF1744"
GREEN = "#FF00E676"
PURPLE = "#FFD500F9"
ORANGE = "#FFFF9100"
BLUE = "#FF2979FF"

# ---------------------------------------------------------------- schede (riquadri laterali, barra inferiore, righe centrali)
# kind: empty | single | group | tyres | bars | delta
def single(name, ch, color=FRAME):
    return dict(name=name, kind="single", ch=ch, color=color)

def group(name, title, entries, color=FRAME):
    return dict(name=name, kind="group", title=title, entries=entries, color=color)

CARDS = [
    dict(name="Vuoto", kind="empty"),
    dict(name="Gomme (pressione + temperatura)", kind="tyres", title="GOMME",
         main=["TyreP" + w[0] for w in WHEELS], sub=["TyreT" + w[0] for w in WHEELS], color=FRAME),
    group("Gomme (temperature)", "TEMP. GOMME", [("TyreT" + w[0], WHITE) for w in WHEELS]),
    group("Usura gomme", "USURA GOMME", [("TyreW" + w[0], WHITE) for w in WHEELS]),
    group("Temperature freni", "FRENI", [("BrakeT" + w[0], ORANGE) for w in WHEELS]),
    group("Tempi giro", "TEMPI GIRO", [("EstLap", BLUE), ("LastLap", GREEN), ("BestLap", PURPLE)]),
    group("Carburante", "CARBURANTE", [("Fuel", WHITE), ("FuelPerLap", WHITE), ("FuelLaps", WHITE)]),
    dict(name="Sessione", kind="session", title="", color=FRAME),
    dict(name="Distacchi", kind="gaps", title="DISTACCHI", sides=["Avanti", "Dietro"], color=FRAME),
    dict(name="Pedali", kind="bars", title="PEDALI",
         entries=[("Throttle", GREEN), ("Brake", RED), ("Clutch", CYAN)], color=FRAME),
    dict(name="Meteo", kind="iconrow", title="METEO", color=FRAME,
         entries=[("dt-ico-pista", "TrackTemp"), ("dt-ico-aria", "AirTemp"), ("dt-ico-bagnato", "TrackWet"), ("dt-ico-pioggia", "Rain")]),
    dict(name="Motore (acqua/olio)", kind="iconrow", title="MOTORE", color=FRAME,
         entries=[("dt-ico-acqua", "WaterTemp"), ("dt-ico-olio", "OilTemp")]),
    group("Assetto (TC/ABS/BB/MAP)", "ASSETTO", [("TC", CYAN), ("ABS", YELLOW), ("BB", RED), ("Map", GREEN)]),
    dict(name="Delta", kind="delta", ch="Delta", color=FRAME),
    single("TC", "TC", CYAN),
    single("TC Cut", "TCCut", CYAN),
    single("ABS", "ABS", YELLOW),
    single("Ripartizione freno (BB)", "BB", RED),
    single("Mappa motore", "Map", GREEN),
    single("Posizione", "Pos"),
    single("Giro", "Lap"),
    single("Tempo rimasto", "TimeLeft"),
    single("Ultimo giro", "LastLap", YELLOW),
    single("Miglior giro", "BestLap", GREEN),
    single("Giro stimato", "EstLap", PURPLE),
    single("Velocità", "Speed"),
    single("Giri motore", "Rpm"),
    single("Carburante (litri)", "Fuel"),
    single("Carburante (giri)", "FuelLaps"),
    single("Consumo medio", "FuelPerLap"),
    single("ERS / batteria", "ERS", CYAN),
    single("Ora", "Clock"),
    dict(name="Distacco avanti", kind="gaps", title="", sides=["Avanti"], color=GREEN),
    dict(name="Distacco dietro", kind="gaps", title="", sides=["Dietro"], color=RED),
    single("Temperatura aria", "AirTemp"),
    single("Temperatura pista", "TrackTemp"),
]
for i in range(1, 7):
    CARDS.append(single("Personalizzato %d" % i, "Custom%d" % i, ORANGE))
# schede aggiunte dopo: sempre in fondo, cosi' gli indici delle pagine salvate restano validi
CARDS.append(dict(name="Gomme complete (temp. carcassa, pressione, freni)", kind="tyres2", color=FRAME))

CARDS.append(single("Energia virtuale (VE)", "VE", CYAN))
CARDS.append(single("Giri energia virtuale", "VELaps", CYAN))
CARDS.append(single("Consumo energia virtuale", "VEPerLap", CYAN))
CARDS.append(group("Energia virtuale (completa)", "ENERGIA VIRTUALE", [("VE", CYAN), ("VEPerLap", WHITE), ("VELaps", WHITE)], CYAN))
CARDS.append(single("TC Slip", "TCSlip", CYAN))
CARDS.append(dict(name="Danni (schema auto)", kind="damage", title="DANNI", color=FRAME))
CARDS.append(dict(name="Stato auto (luci, bandiera, tergi, motore)", kind="status", title="", color=FRAME))
CARDS.append(dict(name="Carburante (barra + giri)", kind="gauge", title="CARBURANTE (GIRI)", ch="Fuel", laps="FuelLaps", pct="FuelPct",
                  suffix="l", fill="#FF7A2A35", color=FRAME))
CARDS.append(dict(name="Energia virtuale (barra + giri)", kind="gauge", title="ENERGIA (GIRI)", ch="VE", laps="VELaps", pct="VE",
                  suffix="", fill="#FF1E3BE0", color=FRAME))
CARDS.append(dict(name="Strategia gara (soste, finestra, rifornimento)", kind="strategy", title="", color=FRAME))
CARDS.append(dict(name="Classifica generale", kind="standings", scope="G", title="CLASSIFICA", color=FRAME))
CARDS.append(dict(name="Classifica di classe", kind="standings", scope="C", title="CLASSIFICA CLASSE", color=FRAME))
CARDS.append(group("Sosta (tempi stimati da LMU)", "SOSTA (S)", [("RepDamage", WHITE), ("RepTyres", WHITE), ("RepFuel", WHITE), ("RepBrakes", WHITE), ("RepTotal", YELLOW)]))
CARDS.append(single("Tempo sosta totale", "RepTotal", YELLOW))
CARDS.append(group("Danni sospensioni", "SOSPENSIONI", [("Susp" + k, WHITE) for k, _, _ in WHEELS]))
CARDS.append(group("Danni (aero e motore)", "DANNI", [("AeroDmg", WHITE), ("EngineDmg", WHITE)]))
CARDS.append(group("Usura freni", "USURA FRENI", [("BrakeW" + k, WHITE) for k, _, _ in WHEELS]))
CARDS.append(single("Gomme nuove rimaste", "NewTyres", GREEN))
CARDS.append(single("Track limits", "TrackLim", ORANGE))
CARDS.append(single("Batteria", "Battery", CYAN))
CARDS.append(dict(name="Previsioni meteo", kind="forecast", title="PREVISIONI", color=FRAME))
CARDS.append(dict(name="Gomme (tutto in uno: temperatura, pressione, usura, freni)", kind="tyres3", title="GOMME", color=FRAME))
STANDINGS_MAX = 60   # righe esportate dal plugin per ogni classifica
CARDS += [
    single("Consumo stimato (in tempo reale)", "FuelLive"),
    single("Consumo rispetto all'ultimo giro", "FuelDelta"),
    single("Energia stimata (in tempo reale)", "VELive", CYAN),
    single("Consumo per fare 1 giro in più", "SavePlus1", GREEN),
    single("Consumo per 1 sosta in meno", "SaveStop", YELLOW),
    group("Risparmio carburante", "RISPARMIO", [("FuelLive", WHITE), ("SavePlus1", GREEN), ("SaveStop", YELLOW)]),
    single("Rifornimento impostato (menu box LMU)", "Refill"),
    single("Usura gomme per giro", "TyreWearLap"),
    single("Giri che durano le gomme", "TyreLaps"),
    group("Gomme (usura e durata)", "GOMME", [("TyreWearLap", WHITE), ("TyreLaps", WHITE)]),
    single("Posizione dopo il box", "PitRejoin"),
]

CARD_INDEX = {c["name"]: i for i, c in enumerate(CARDS)}

# zone danno LMU (mDentSeverity01..08), macchina vista dall'alto con il muso in alto:
# 1 anteriore centro, 2 anteriore sx, 3 fianco sx, 4 posteriore sx, 5 posteriore (ala), 6 posteriore dx, 7 fianco dx, 8 anteriore dx
DAMAGE_ZONES = 8

# canali con colore calcolato dal plugin (blu freddo -> verde ok -> rosso caldo): DTPlugin.<Id>_Colore
COLOR_CHANNELS = ["PosGain"] + ["TyreT" + w[0] for w in WHEELS] + ["TyreP" + w[0] for w in WHEELS] + ["BrakeT" + w[0] for w in WHEELS] + ["TyreW" + w[0] for w in WHEELS] + ["FuelDelta", "Refill", "FuelLaps", "VELaps"]

# categorie per la tavolozza del plugin (ordine = ordine di visualizzazione)
CATEGORIES = [
    ("Tempi", ["Tempi giro", "Delta", "Ultimo giro", "Miglior giro", "Giro stimato"]),
    ("Carburante ed energia", ["Carburante", "Carburante (litri)", "Carburante (giri)", "Consumo medio", "Energia virtuale (VE)", "Energia virtuale (completa)", "Giri energia virtuale", "Consumo energia virtuale", "ERS / batteria", "Carburante (barra + giri)", "Energia virtuale (barra + giri)", "Strategia gara (soste, finestra, rifornimento)",
                               "Consumo stimato (in tempo reale)", "Consumo rispetto all'ultimo giro", "Energia stimata (in tempo reale)",
                               "Consumo per fare 1 giro in più", "Consumo per 1 sosta in meno", "Risparmio carburante", "Rifornimento impostato (menu box LMU)"]),
    ("Comandi auto", ["Assetto (TC/ABS/BB/MAP)", "TC", "TC Cut", "TC Slip", "ABS", "Ripartizione freno (BB)", "Mappa motore"]),
    ("Gomme e freni", ["Gomme (tutto in uno: temperatura, pressione, usura, freni)", "Gomme complete (temp. carcassa, pressione, freni)", "Gomme (pressione + temperatura)", "Gomme (temperature)", "Usura gomme", "Temperature freni", "Danni (schema auto)",
                        "Usura gomme per giro", "Giri che durano le gomme", "Gomme (usura e durata)"]),
    ("Sessione e gara", ["Sessione", "Posizione", "Giro", "Tempo rimasto", "Ora", "Distacchi", "Distacco avanti", "Distacco dietro", "Classifica generale", "Classifica di classe", "Stato auto (luci, bandiera, tergi, motore)"]),
    ("Motore e pedali", ["Pedali", "Velocità", "Giri motore", "Motore (acqua/olio)"]),
    ("Meteo", ["Meteo", "Temperatura aria", "Temperatura pista", "Previsioni meteo"]),
    ("Sosta e danni (LMU)", ["Sosta (tempi stimati da LMU)", "Tempo sosta totale", "Danni sospensioni", "Danni (aero e motore)", "Usura freni", "Gomme nuove rimaste", "Track limits", "Batteria", "Posizione dopo il box"]),
    ("Personalizzati", ["Personalizzato %d" % i for i in range(1, 7)]),
]
_cat_all = [n for _, ns in CATEGORIES for n in ns]
assert sorted(_cat_all) == sorted(c["name"] for c in CARDS if c["kind"] != "empty"), set(c["name"] for c in CARDS) ^ set(_cat_all)

# schede per le barre laterali verticali
BARS = [
    ("Vuoto", None, None),
    ("Giri motore", "RpmPct", GREEN),
    ("Gas", "Throttle", GREEN),
    ("Freno", "Brake", RED),
    ("Frizione", "Clutch", CYAN),
    ("Carburante", "FuelPct", YELLOW),
    ("ERS / batteria", "ERS", CYAN),
]
BAR_INDEX = {b[0]: i for i, b in enumerate(BARS)}

# ---------------------------------------------------------------- geometria
DASH_W, DASH_H = 850, 480
M = 5        # margine
G = 6        # spazio fra riquadri
SB = 20      # larghezza barra laterale
TB = 78      # altezza barra inferiore
WB = 78      # altezza della riga larga sotto (barra doppia)
TB2 = 62     # altezza della riga di riquadri sopra, quando la barra e' doppia (piu' spazio alle schede in alto)
CW = 200     # larghezza colonna centrale
CX = (DASH_W - CW) // 2  # 339

# cornici: (nome, barre laterali, barra inferiore)
FRAMES = [
    ("Completo (barre laterali + barra inferiore)", True, True),
    ("Solo barra inferiore", False, True),
    ("Solo barre laterali", True, False),
    ("Pulito (solo riquadri)", False, False),
    # barra inferiore doppia (toolbar = 2): riga di riquadri + sotto un riquadro largo (es. strategia gara)
    ("Completo con barra doppia", True, 2),
    ("Barra doppia senza barre laterali", False, 2),
    # usate dalla barra doppia quando non serve rifornire (non compaiono tra le scelte): una sola riga, bassa come quella della doppia
    ("Barra bassa con barre laterali", True, 3),
    ("Barra bassa", False, 3),
]
# barra doppia "intelligente": quando non serve rifornire la pagina usa la cornice con la sola riga di riquadri
# stesse cornici con la fila di LED dei giri in alto (tutto il resto piu' in basso): indice + LED_OFFSET
LED_OFFSET = len(FRAMES)
LED_H = 26
FRAMES += [(f[0] + " + LED", f[1], f[2], True) for f in FRAMES]
SINGLE_OF = {4: 6, 5: 7, 4 + LED_OFFSET: 6 + LED_OFFSET, 5 + LED_OFFSET: 7 + LED_OFFSET}


def frame_led(fi):
    return len(FRAMES[fi]) > 3 and bool(FRAMES[fi][3])

# griglie dei lati: unita' 2 colonne x 4 righe  (col, riga, larghezza, altezza)
GRIDS = [
    ("Riga + grande",        [(0, 0, 2, 1), (0, 1, 2, 3)]),
    ("2 grandi",             [(0, 0, 2, 2), (0, 2, 2, 2)]),
    ("Grande + 2 medi",      [(0, 0, 2, 2), (0, 2, 1, 2), (1, 2, 1, 2)]),
    ("4 medi",               [(0, 0, 1, 2), (1, 0, 1, 2), (0, 2, 1, 2), (1, 2, 1, 2)]),
    ("Grande + 4 piccoli",   [(0, 0, 2, 2), (0, 2, 1, 1), (1, 2, 1, 1), (0, 3, 1, 1), (1, 3, 1, 1)]),
    ("4 righe",              [(0, 0, 2, 1), (0, 1, 2, 1), (0, 2, 2, 1), (0, 3, 2, 1)]),
    ("8 piccoli",            [(c, r, 1, 1) for r in range(4) for c in range(2)]),
    ("2 righe + grande",     [(0, 0, 2, 1), (0, 1, 2, 1), (0, 2, 2, 2)]),
    ("Grande + 2 righe",     [(0, 0, 2, 2), (0, 2, 2, 1), (0, 3, 2, 1)]),
    ("Grande + riga",        [(0, 0, 2, 3), (0, 3, 2, 1)]),
    ("Unico",                [(0, 0, 2, 4)]),
]
MAX_SIDE_SLOTS = max(len(g[1]) for g in GRIDS)  # 8
TOOLBARS = [("5 riquadri", 5), ("6 riquadri", 6), ("4 riquadri", 4), ("3 riquadri", 3), ("1 riquadro largo", 1)]
MAX_TOOL_SLOTS = max(t[1] for t in TOOLBARS) + 1   # +1: riquadro largo della barra doppia
CENTER_ROWS = 3


def frame_geom(fi):
    _, sidebars, toolbar = FRAMES[fi][:3]
    bottom = DASH_H - M - (((TB2 + G + WB + G) if toolbar == 2 else ((TB2 + G) if toolbar == 3 else (TB + G))) if toolbar else 0)
    xl0 = M + (SB + G if sidebars else 0)
    xl1 = CX - G
    xr0 = CX + CW + G
    xr1 = DASH_W - M - (SB + G if sidebars else 0)
    # altezza della riga in alto nella griglia "Riga + grande": come i riquadri della barra in basso
    row_h = TB2 if toolbar in (2, 3) else TB
    top = M + (LED_H + G if frame_led(fi) else 0)
    return dict(sidebars=sidebars, toolbar=toolbar, top=top, bottom=bottom,
                left=(xl0, xl1), right=(xr0, xr1), row_h=row_h)


def grid_cells(region_x0, region_x1, top, bottom, gi, row_h=None):
    W = region_x1 - region_x0
    H = bottom - top
    cw = (W - G) / 2.0
    rh = (H - 3 * G) / 4.0
    if gi == 0 and row_h:
        # "Riga + grande": la riga alta come la barra in basso, il resto alla scheda grande
        return [(region_x0, top, round(W), row_h), (region_x0, top + row_h + G, round(W), bottom - (top + row_h + G))]
    out = []
    for (c, r, cs, rs) in GRIDS[gi][1]:
        x = region_x0 + round(c * (cw + G))
        y = top + round(r * (rh + G))
        w = round(cs * cw + (cs - 1) * G)
        h = round(rs * rh + (rs - 1) * G)
        out.append((x, y, w, h))
    return out


def toolbar_cells(fi, ti):
    g = frame_geom(fi)
    n = TOOLBARS[ti][1]
    x0, x1 = g["left"][0], g["right"][1]
    W = x1 - x0
    w = (W - (n - 1) * G) // n
    lead = (W - (n * w + (n - 1) * G)) // 2
    double = FRAMES[fi][2] == 2
    th = TB2 if (double or FRAMES[fi][2] == 3) else TB
    y = DASH_H - M - th - ((WB + G) if double else 0)
    cells = [(x0 + lead + i * (w + G), y, w, th) for i in range(n)]
    if double:
        cells.append((x0, DASH_H - M - WB, W, WB))   # riquadro largo sotto (ultimo riquadro della barra)
    return cells


def sidebar_cells(fi):
    g = frame_geom(fi)
    h = g["bottom"] - g["top"]
    return [(M, g["top"], SB, h), (DASH_W - M - SB, g["top"], SB, h)]


# centro: quanti blocchi (delta + marcia + righe sotto la marcia); con meno righe la marcia diventa piu' alta
CENTER_LAYOUTS = [("5 blocchi", 3), ("4 blocchi", 2), ("3 blocchi", 1), ("2 blocchi", 0)]


def center_geom(fi, ci=0):
    g = frame_geom(fi)
    top, bottom = g["top"], g["bottom"]
    delta_h, speed_h, row_h = 50, 40, 36   # delta piu' alto (spazio preso dal riquadro della marcia)
    n = CENTER_LAYOUTS[ci][1]
    rows_block = n * (row_h + G)
    gear_top = top + delta_h + G
    gear_h = bottom - rows_block - speed_h - G - gear_top
    speed_top = gear_top + gear_h + G
    rows_top = speed_top + speed_h + G
    rows = [(CX, rows_top + i * (row_h + G), CW, row_h) for i in range(n)]
    # la velocita' sta dentro il riquadro della marcia (un solo riquadro); "speed" e' la sua zona interna
    box_h = gear_h + G + speed_h
    sp_y = gear_top + int(box_h * 0.56)
    return dict(delta=(CX, top, CW, delta_h), gear=(CX, gear_top, CW, box_h),
                speed=(CX + 8, sp_y, CW - 16, speed_h), rows=rows)


# ---------------------------------------------------------------- pagine predefinite
def _slots(names):
    return [CARD_INDEX[n] for n in names]

DEFAULT_PAGES = [
    dict(name="Gara", frame=1, grid_l=0, grid_r=0, toolbar=0,
         L=_slots(["Sessione", "Gomme (pressione + temperatura)"]),
         R=_slots(["Distacchi", "Tempi giro"]),
         T=_slots(["TC", "TC Cut", "ABS", "Ripartizione freno (BB)", "Mappa motore"]),
         C=_slots(["Carburante", "Meteo", "Ora"]),
         SL=BAR_INDEX["Giri motore"], SR=BAR_INDEX["Carburante"]),
    dict(name="Qualifica", frame=0, grid_l=2, grid_r=8, toolbar=1,
         L=_slots(["Gomme (pressione + temperatura)", "Pedali", "Delta"]),
         R=_slots(["Tempi giro", "Posizione", "Tempo rimasto"]),
         T=_slots(["TC", "TC Cut", "ABS", "Ripartizione freno (BB)", "Mappa motore", "Carburante (giri)"]),
         C=_slots(["Giro", "Meteo", "Ora"]),
         SL=BAR_INDEX["Gas"], SR=BAR_INDEX["Freno"]),
    dict(name="Endurance", frame=3, grid_l=6, grid_r=4, toolbar=0,
         L=_slots(["TC", "TC Cut", "ABS", "Ripartizione freno (BB)", "Mappa motore", "ERS / batteria", "Temperatura aria", "Temperatura pista"]),
         R=_slots(["Carburante", "Carburante (litri)", "Carburante (giri)", "Consumo medio", "Tempo rimasto"]),
         T=_slots(["TC", "ABS", "Ripartizione freno (BB)", "Mappa motore", "Carburante (giri)"]),
         C=_slots(["Distacco avanti", "Distacco dietro", "Ora"]),
         SL=BAR_INDEX["Vuoto"], SR=BAR_INDEX["Vuoto"]),
]


# ---------------------------------------------------------------- avvisi (overlay)
# varianti: 0 pieno schermo; 1-3 meta' (centro/alto/basso); 4-6 piccolo (centro/alto/basso); 7-8 meta' di lato (sx/dx)
ALERT_SIZES = ["Pieno schermo", "Metà", "Piccolo", "Metà di lato"]
ALERT_POSITIONS = ["Centro", "Alto", "Basso"]
def alert_variants():
    out = [(0, 0, DASH_W, DASH_H)]
    hw, hh = 600, 290
    for top in ((DASH_H - hh) // 2, 10, DASH_H - hh - 10):
        out.append(((DASH_W - hw) // 2, top, hw, hh))
    sw, sh = 400, 130
    for top in ((DASH_H - sh) // 2, 10, DASH_H - sh - 10):
        out.append(((DASH_W - sw) // 2, top, sw, sh))
    # 7-8: meta' schermo di lato (entra da sinistra / da destra)
    sw2, sh2 = 320, DASH_H                    # di lato: stretto ma alto tutto lo schermo
    sy2 = (DASH_H - sh2) // 2
    out.append((0, sy2, sw2, sh2))
    out.append((DASH_W - sw2, sy2, sw2, sh2))
    return out
ALERT_VARIANTS = alert_variants()
ALERT_MAX_VALUES = 4


# ---------------------------------------------------------------- DT LapBox (RaceSense LapBox 4")
# in alto le due luci dietro i fori, sotto la parte visibile nella finestra.
# La parte visibile e' divisa in 2 colonne x 4 righe (8 blocchi): le griglie sono le stesse dei lati della DT Dashboard.
# misure prese dalle foto: con 800x630 restavano bande ai lati, con 1050x630 una banda in alto (luci tagliate).
LB_W, LB_H = 1050, 700            # proporzioni dello schermo della LapBox misurate dalle foto
LB_X0, LB_X1 = 6, 1040
LB_TOP, LB_BOTTOM = 110, 598
# le due luci: una zona piena (rettangolo) dietro ogni foro, cosi' il foro e' tutto acceso anche se non e' preciso
LB_LIGHTS = [(253, 0, 140, 94), (635, 0, 140, 94)]     # x, y, larghezza, altezza
LB_G = 6
LB_CW = (LB_X1 - LB_X0 - LB_G) // 2
LB_RH = (LB_BOTTOM - LB_TOP - 3 * LB_G) // 4


def lb_rect(cell):
    c, r, w, h = cell
    x = LB_X0 + c * (LB_CW + LB_G)
    y = LB_TOP + r * (LB_RH + LB_G)
    return (x, y, w * LB_CW + (w - 1) * LB_G, h * LB_RH + (h - 1) * LB_G)


# griglie della LapBox: 8, 4, 2 (sopra e sotto o affiancati) e 1 blocco
LB_GRIDS = [
    ("8 blocchi",                [(c, r, 1, 1) for r in range(4) for c in range(2)]),
    ("4 blocchi",                [(0, 0, 1, 2), (1, 0, 1, 2), (0, 2, 1, 2), (1, 2, 1, 2)]),
    ("2 blocchi sopra e sotto",  [(0, 0, 2, 2), (0, 2, 2, 2)]),
    ("2 blocchi affiancati",     [(0, 0, 1, 4), (1, 0, 1, 4)]),
    ("1 blocco",                 [(0, 0, 2, 4)]),
]
LB_CELLS = []
for _g in LB_GRIDS:
    for _c in _g[1]:
        if tuple(_c) not in LB_CELLS:
            LB_CELLS.append(tuple(_c))
LB_GRID_CELLS = [[LB_CELLS.index(tuple(c)) for c in g[1]] for g in LB_GRIDS]

DEFAULT_LB_PAGES = [
    dict(name="Gara", grid=1, L=_slots(["Delta", "Tempi giro", "Carburante (barra + giri)", "Distacchi"])),
    dict(name="Prove", grid=0, L=_slots(["Ultimo giro", "Miglior giro", "Delta", "Giro stimato", "TC", "ABS", "Ripartizione freno (BB)", "Mappa motore"])),
]
