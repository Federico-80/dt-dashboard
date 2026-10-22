"""DT Rally: dashboard semplificata per i rally (AC Rally, EA WRC, DiRT Rally, RBR).
Contagiri a barra in alto, marcia grande al centro, tempo prova e gomme a sinistra, motore e aiuti a destra,
avanzamento della prova in basso. Usa le proprieta' standard di SimHub; serve il DT Plugin sbloccato."""
import json, os, shutil, sys, zipfile
import gen_dash as G

NAME = "DT Rally"
W, H = 850, 480
GD = "DataCorePlugin.GameData."
WHITE, GREY, DIM = "#FFFFFFFF", "#FF9E9E9E", "#FF5A5A5A"
GREEN, YELLOW, ORANGE, RED, BLUE, TEAL = "#FF00E676", "#FFFFD600", "#FFFF9100", "#FFFF1744", "#FF2979FF", "#FF00FED1"
PANEL = "#FF0E0E0E"


def js(code):
    return {"Formula": {"JSExt": 0, "Interpreter": 1, "Expression": code}, "Mode": 2}


def p(name):
    return "$prop('%s%s')" % (GD, name)


def rect(name, x, y, w, h, bg=PANEL, radius=8, border=None, bw=2, bind=None):
    d = G.rect(name, x, y, w, h, bg=bg, border=border, bw=bw, radius=radius)
    if bind:
        d["Bindings"] = bind
    return d


def text(name, x, y, w, h, sample, size, color=WHITE, weight="Bold", ha=1, bind=None):
    d = G.text(name, x, y, w, h, None, sample, size, color, weight=weight, ha=ha)
    if bind:
        d["Bindings"] = bind
    return d


# --- frammenti JS
RPM_PCT = ("var r = %s; var m = %s; var pc = %s;\r\n"
           "if (pc == null || pc <= 0) { pc = (m > 0 && r != null) ? r / m * 100 : 0; }\r\n") % (p("Rpms"), p("MaxRpm"), p("CarSettings_CurrentDisplayedRPMPercent"))
SHIFT = "var blink = (Math.floor(Date.now() / 90) % 2 == 0);\r\n"


def lap_js(prop):
    return ("var v = %s;\r\nvar s = null;\r\n"
            "if (typeof v === 'number') { s = v; } else if (v) { try { s = timespantoseconds(v); } catch (e) { s = null; } }\r\n"
            "if (s == null || isNaN(s) || s <= 0) { return '-:--.---'; }\r\n"
            "var ms = Math.round(s * 1000);\r\nvar sec = Math.floor(ms / 1000) %% 60, mil = ms %% 1000;\r\n"
            "return Math.floor(ms / 60000) + ':' + (sec < 10 ? '0' : '') + sec + '.' + (mil < 100 ? (mil < 10 ? '00' : '0') : '') + mil;") % prop


def num_js(prop, dec=0, suffix=""):
    return ("var v = %s;\r\nif (v == null || isNaN(v)) { return '-'; }\r\nreturn (+v).toFixed(%d)%s;"
            % (prop, dec, (" + '%s'" % suffix) if suffix else ""))


def temp_color(prop, cold, hot):
    return js("var t = %s;\r\nif (t == null || t <= 0) { return '#FF242424'; }\r\nif (t < %d) { return '#FF0D47A1'; }\r\n"
              "if (t > %d) { return '#FFB71C1C'; }\r\nreturn '#FF1B5E20';" % (prop, cold, hot))


# ------------------------------------------------------------------ contagiri a barra
def rpm_bar():
    """Contagiri a barra che parte dai due lati e si chiude verso il centro."""
    it = []
    n, x0, y0, gap, hh = 15, 14, 10, 4, 46      # segmenti per lato
    half = (W - 2 * x0 - 12) / 2.0               # 12 px di spazio al centro
    seg = (half - (n - 1) * gap) / n
    off = "#FF1A1A1A"
    for i in range(n):
        thr = (i + 1) / n * 100.0
        frac = i / (n - 1)
        col = GREEN if frac < 0.55 else (YELLOW if frac < 0.75 else (ORANGE if frac < 0.88 else RED))
        code = (RPM_PCT + SHIFT +
                "if (pc >= 97) { return blink ? '%s' : '%s'; }\r\nreturn pc >= %.1f ? '%s' : '%s';" % (BLUE, off, thr - 100.0 / n * 0.5, col, off))
        h = hh * (0.62 + 0.38 * frac)            # piu' alti verso il centro
        xl = x0 + i * (seg + gap)                # da sinistra verso il centro
        xr = W - x0 - (i + 1) * seg - i * gap    # da destra verso il centro
        for side, x in (("S", xl), ("D", xr)):
            it.append(rect("Giri %s%d" % (side, i + 1), x, y0 + hh - h, seg, h, bg=col, radius=3, bind={"BackgroundColor": js(code)}))
    it.append(text("Giri valore", W / 2 - 100, 60, 200, 24, "7450 RPM", 16, GREY, "Medium",
                   bind={"Text": js("var r = %s;\r\nreturn (r == null) ? '' : Math.round(r) + ' RPM';" % p("Rpms"))}))
    return it


# ------------------------------------------------------------------ centro: marcia, velocita', pedali
def center():
    cx, cw = 300, 250
    it = [rect("Marcia fondo", cx, 88, cw, 262, bg="#FF101010", radius=18, border="#FF2A2A2A", bw=2,
               bind={"BorderColor": js(RPM_PCT + SHIFT + "return (pc >= 97 && blink) ? '%s' : '#FF2A2A2A';" % BLUE)}),
          text("Marcia", cx, 92, cw, 250, "3", 230, WHITE, "Black",
               bind={"Text": js("var g = %s;\r\nif (g == null || g === '') { return '-'; }\r\nreturn ('' + g) == '0' ? 'N' : '' + g;" % p("Gear")),
                     "TextColor": js(RPM_PCT + SHIFT + "return (pc >= 97 && blink) ? '%s' : '%s';" % (BLUE, WHITE))}),
          text("Velocita", cx, 352, cw, 74, "142", 66, WHITE, "Black",
               bind={"Text": js("var v = %s;\r\nreturn (v == null) ? '0' : Math.round(v);" % p("SpeedLocal"))}),
          text("Velocita unita", cx, 418, cw, 18, "KM/H", 13, GREY, "Medium",
               bind={"Text": js("var u = %s;\r\nreturn u ? ('' + u).toUpperCase() : 'KM/H';" % p("SpeedLocalUnit"))})]
    # barre pedali ai lati della marcia: freno a sinistra, gas a destra
    for name, x, prop, col in (("Freno", cx - 22, "Brake", RED), ("Gas", cx + cw + 6, "Throttle", GREEN)):
        it.append(rect(name + " fondo", x, 88, 16, 262, bg="#FF1A1A1A", radius=6))
        it.append(rect(name, x, 88, 16, 262, bg=col, radius=6, bind={
            "Height": js("var v = %s;\r\nif (v == null) { v = 0; }\r\nreturn Math.max(0, Math.min(262, v * 2.62));" % p(prop)),
            "Top": js("var v = %s;\r\nif (v == null) { v = 0; }\r\nreturn 88 + 262 - Math.max(0, Math.min(262, v * 2.62));" % p(prop))}))
    # freno a mano
    hb = rect("Freno a mano", cx, 88, cw, 34, bg="#FFB71C1C", radius=10)
    hb["Bindings"] = {"Visible": js("var v = %s;\r\nreturn v != null && v > 5;" % p("Handbrake"))}
    hb["Visible"] = False
    it.append(hb)
    t = text("Freno a mano testo", cx, 88, cw, 34, "FRENO A MANO", 17, WHITE, "Black")
    t["Bindings"] = {"Visible": js("var v = %s;\r\nreturn v != null && v > 5;" % p("Handbrake"))}
    t["Visible"] = False
    it.append(t)
    return it


# ------------------------------------------------------------------ sinistra: tempi e gomme
def left():
    x, w = 14, 258
    it = [rect("Tempo fondo", x, 88, w, 116),
          text("Tempo etichetta", x + 14, 94, w - 28, 18, "TEMPO PROVA", 13, GREY, "Medium", ha=0),
          text("Tempo", x + 6, 112, w - 12, 56, "2:14.873", 46, WHITE, "Black", bind={"Text": js(lap_js(p("CurrentLapTime")))}),
          text("Ultimo etichetta", x + 14, 172, 60, 22, "ULTIMO", 11, GREY, "Medium", ha=0),
          text("Ultimo", x + 70, 170, w - 84, 26, "4:02.115", 19, WHITE, "Bold", ha=2, bind={"Text": js(lap_js(p("LastLapTime")))}),
          rect("Migliore fondo", x, 210, w, 44),
          text("Migliore etichetta", x + 14, 210, 80, 44, "MIGLIORE", 11, GREY, "Medium", ha=0),
          text("Migliore", x + 90, 210, w - 104, 44, "3:58.402", 22, "#FFD500F9", "Bold", ha=2, bind={"Text": js(lap_js(p("BestLapTime")))}),
          rect("Gomme fondo", x, 260, w, 166),
          text("Gomme etichetta", x + 14, 264, w - 28, 18, "GOMME °C", 13, GREY, "Medium", ha=0)]
    cw_, ch_ = (w - 30) / 2, 62
    for i, (k, lab) in enumerate((("FrontLeft", "AS"), ("FrontRight", "AD"), ("RearLeft", "PS"), ("RearRight", "PD"))):
        cx_ = x + 10 + (i % 2) * (cw_ + 10)
        cy_ = 288 + (i // 2) * (ch_ + 8)
        prop = p("TyreTemperature" + k)
        it.append(rect("Gomma " + lab, cx_, cy_, cw_, ch_, bg="#FF1B5E20", radius=8, bind={"BackgroundColor": temp_color(prop, 50, 105)}))
        it.append(text("Gomma %s valore" % lab, cx_, cy_ + 4, cw_, 40, "78", 32, WHITE, "Black", bind={"Text": js(num_js(prop))}))
        it.append(text("Gomma %s nome" % lab, cx_, cy_ + 42, cw_, 16, lab, 11, "#CCFFFFFF", "Medium"))
    return it


# ------------------------------------------------------------------ destra: motore e aiuti
def right():
    x, w = 578, 258
    rows = [("ACQUA", p("WaterTemperature"), 0, "°", (60, 105), "88°"),
            ("OLIO", p("OilTemperature"), 0, "°", (60, 120), "96°"),
            ("CARBURANTE", p("Fuel"), 1, " L", None, "38.5 L")]
    it = [rect("Motore fondo", x, 88, w, 166)]
    for i, (lab, prop, dec, suf, thr, sample) in enumerate(rows):
        y = 96 + i * 52
        it.append(text("%s etichetta" % lab, x + 14, y, 130, 46, lab, 14, GREY, "Medium", ha=0))
        b = {"Text": js(num_js(prop, dec, suf))}
        if thr:
            b["TextColor"] = js("var t = %s;\r\nif (t == null || t <= 0) { return '#FFFFFFFF'; }\r\nif (t < %d) { return '#FF64B5F6'; }\r\n"
                                "if (t > %d) { return '#FFFF5252'; }\r\nreturn '#FFFFFFFF';" % (prop, thr[0], thr[1]))
        it.append(text("%s valore" % lab, x + 120, y, w - 134, 46, sample, 30, WHITE, "Black", ha=2, bind=b))
    # aiuti: TC, ABS, ripartitore
    bx, bw_, gap = x, (w - 16) / 3, 8
    for i, (lab, prop, dec) in enumerate((("TC", p("TCLevel"), 0), ("ABS", p("ABSLevel"), 0), ("BB", p("BrakeBias"), 1))):
        cx_ = bx + i * (bw_ + gap)
        it.append(rect("%s fondo" % lab, cx_, 260, bw_, 80))
        it.append(text("%s etichetta" % lab, cx_, 264, bw_, 18, lab, 13, GREY, "Medium"))
        it.append(text("%s valore" % lab, cx_, 282, bw_, 52, "56.0" if lab == "BB" else "3", 36 if lab != "BB" else 28, YELLOW if lab != "BB" else WHITE, "Black",
                       bind={"Text": js(num_js(prop, dec))}))
    # avanzamento prova (percentuale)
    it.append(rect("Prova fondo", x, 346, w, 80))
    it.append(text("Prova etichetta", x + 14, 350, 140, 18, "PROVA", 13, GREY, "Medium", ha=0))
    it.append(text("Prova valore", x + 14, 366, w - 28, 56, "64%", 44, TEAL, "Black", ha=2,
                   bind={"Text": js("var v = %s;\r\nif (v == null || isNaN(v)) { return '-'; }\r\nif (v <= 1) { v = v * 100; }\r\nreturn Math.round(Math.max(0, Math.min(100, v))) + '%%';" % p("TrackPositionPercent"))}))
    return it


# ------------------------------------------------------------------ barra di avanzamento della prova
def progress():
    x, y, w, h = 14, 440, W - 28, 26
    pos = ("var v = %s;\r\nif (v == null || isNaN(v)) { v = 0; }\r\nif (v > 1) { v = v / 100; }\r\nv = Math.max(0, Math.min(1, v));\r\n" % p("TrackPositionPercent"))
    it = [rect("Avanzamento fondo", x, y, w, h, bg="#FF161616", radius=13),
          rect("Avanzamento", x, y, w, h, bg="#FF00897B", radius=13, bind={"Width": js(pos + "return Math.max(%d, %d * v);" % (h, w))})]
    for i in range(1, 4):  # tacche a 25/50/75%
        it.append(rect("Tacca %d" % i, x + w * i / 4 - 1, y + 4, 2, h - 8, bg="#55FFFFFF", radius=1))
    it.append(text("Arrivo", x + w - 70, y, 62, h, "ARRIVO", 11, "#AAFFFFFF", "Bold", ha=2))
    return it


def lock_layer():
    lay = G.lock_layer()
    lay["Bindings"] = {"Visible": js("return $prop('DTPlugin.Sbloccato') !== true;")}
    return lay


def idle():
    return [rect("Attesa fondo", 0, 0, W, H, bg="#FF000000", radius=0),
            text("Attesa titolo", 0, 170, W, 90, "DT RALLY", 80, WHITE, "Black"),
            text("Attesa sotto", 0, 262, W, 30, "In attesa della prova…", 20, GREY, "Medium"),
            rect("Attesa linea", W / 2 - 60, 160, 120, 5, bg=TEAL, radius=2)]


def build():
    race = [rect("Sfondo", 0, 0, W, H, bg="#FF000000", radius=0)] + rpm_bar() + center() + left() + right() + progress()
    race.append(lock_layer())
    idl = idle() + [lock_layer()]
    scr = [G.screen("Attesa", idl, ingame=False, idle=True, bg="#FF000000"), G.screen("Prova", race, ingame=True, bg="#FF000000")]
    meta = G.metadata(W, H, scr, title=NAME, ingame=[1], idle=[0], preview=1)
    meta["Description"] = "Dashboard semplificata per i rally (DT Racing Team)"
    return G.dash_doc(W, H, scr, meta, controls=True), meta


def write(out_dir):
    folder = os.path.join(out_dir, NAME)
    if os.path.exists(folder):
        shutil.rmtree(folder)
    os.makedirs(os.path.join(folder, "_SHFonts"))
    doc, meta = build()
    with open(os.path.join(folder, NAME + ".djson"), "w", encoding="utf-8") as f:
        f.write(json.dumps(G.strip(doc), ensure_ascii=False, separators=(",", ":")))
    with open(os.path.join(folder, NAME + ".djson.metadata"), "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)
    G.write_thumbs(folder, NAME, doc, {})
    src = os.path.join(os.path.dirname(os.path.abspath(__file__)), "fonts")
    for ttf in ("Roboto-Regular.ttf", "Roboto-Medium.ttf", "Roboto-Bold.ttf", "Roboto-Black.ttf"):
        shutil.copy(os.path.join(src, ttf), os.path.join(folder, "_SHFonts", ttf))
    zpath = os.path.join(out_dir, NAME + ".simhubdash")
    with zipfile.ZipFile(zpath, "w", zipfile.ZIP_DEFLATED) as z:
        for dd, _, fs in os.walk(folder):
            for fn in fs:
                full = os.path.join(dd, fn)
                z.write(full, os.path.relpath(full, out_dir).replace("/", "\\"))
    return doc, zpath


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(os.path.abspath(__file__)), "out_rally")
    os.makedirs(out, exist_ok=True)
    doc, z = write(out)
    print(z)
