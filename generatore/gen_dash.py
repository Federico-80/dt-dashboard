# -*- coding: utf-8 -*-
"""Genera la 'DT Dashboard' per SimHub (Dash Studio) + anteprime PNG."""
import json, math, os, shutil, sys, uuid, zipfile
from catalog import *

OUT = os.environ.get("DT_OUT") or os.path.join(os.path.dirname(os.path.abspath(__file__)), "build")
DASH_NAME = os.environ.get("DT_NOME") or "DT Dashboard"
P = "DTPlugin."
T_RECT = "SimHub.Plugins.OutputPlugins.GraphicalDash.Models.RectangleItem, SimHub.Plugins"
T_TEXT = "SimHub.Plugins.OutputPlugins.GraphicalDash.Models.TextItem, SimHub.Plugins"
T_WIDGET = "SimHub.Plugins.OutputPlugins.GraphicalDash.Models.WidgetItem, SimHub.Plugins"
T_LAYER = "SimHub.Plugins.OutputPlugins.GraphicalDash.Models.Layer, SimHub.Plugins"
T_IMAGE = "SimHub.Plugins.OutputPlugins.GraphicalDash.Models.ImageItem, SimHub.Plugins"
CARD_BG = "#FF0B0B0B"
TRACK = "#FF262626"


def ncalc(expr):
    return {"Formula": {"Expression": expr}, "Mode": 2}


import re as _re


def js(code):
    code = _re.sub(r"\$prop\('DTPlugin\.(\w+)_Valore'\)", r"fdv('\1')", code)
    return {"Formula": {"JSExt": 1, "Interpreter": 1, "Expression": code}, "Mode": 2}


def base(name, x, y, w, h, bindings=None, visible=True):
    d = {"BackgroundColor": "#00FFFFFF", "Height": float(round(h)), "Left": float(round(x)), "Top": float(round(y)),
         "Visible": visible, "BlinkPhasisInverted": False, "Width": float(round(w)), "IsFreezed": False,
         "Name": name, "RenderingSkip": 0, "MinimumRefreshIntervalMS": 0.0}
    if bindings:
        d["Bindings"] = bindings
    return d


def rect(name, x, y, w, h, bg=CARD_BG, border=None, bw=2, radius=10, bindings=None, border_bind=None, pv=None):
    radius = int(round(radius))     # SimHub accetta solo numeri interi per raggio e spessore del bordo
    bw = int(round(bw))
    bs = {"RadiusTopLeft": radius, "RadiusTopRight": radius, "RadiusBottomLeft": radius, "RadiusBottomRight": radius}
    if border:
        bs = {"BorderTop": bw, "BorderBottom": bw, "BorderLeft": bw, "BorderRight": bw, "BorderColor": border, **bs}
    if border_bind:
        bs["Bindings"] = {"BorderColor": border_bind}
    d = {"$type": T_RECT, "IsRectangleItem": True}
    d.update(base(name, x, y, w, h, bindings))
    d["BackgroundColor"] = bg
    d["BorderStyle"] = bs
    d["_pv"] = dict(kind="rect", bg=bg, border=border, bw=bw, radius=radius, **(pv or {}))
    return d


def text(name, x, y, w, h, expr=None, sample="", size=14, color=WHITE, weight="Bold", ha=1, va=1, bindings=None, pv_color=None):
    b = dict(bindings or {})
    if expr:
        m = _re.fullmatch(r"\[DTPlugin\.(\w+?)(_Nome)?\]", expr)
        if m and m.group(1) in CH:
            b["Text"] = js("return %s('%s');" % ("fdn" if m.group(2) else "fd", m.group(1)))
            if m.group(1) == "Pos" and not m.group(2):
                # posizione con il totale delle auto: 11/25
                b["Text"] = js("var p = fd('Pos'); var n = fd('Cars');\r\n"
                               "return (p && p !== '-' && n && n !== '-' && n != '0') ? p + '/' + n : p;")
            if not m.group(2) and m.group(1) in COLOR_CHANNELS and "TextColor" not in b:
                # colore blu/verde/rosso calcolato dal plugin in base alle soglie
                b["TextColor"] = js("var c = $prop('DTPlugin.%s_Colore');\r\nreturn c ? c : '%s';" % (m.group(1), color))
        else:
            b["Text"] = ncalc(expr)
    d = {"$type": T_TEXT, "IsTextItem": True, "Font": "Roboto", "FontWeight": weight, "FontSize": float(max(7, int(size))),
         "Text": sample if not expr else "", "TextColor": color, "HorizontalAlignment": ha, "VerticalAlignment": va}
    d.update(base(name, x, y, w, h, b))
    d["_pv"] = dict(kind="text", text=sample, size=max(7, int(size)), color=pv_color or color, ha=ha, weight=weight)
    return d


def image(name, x, y, w, h, img, bind_js=None):
    """Immagine incorporata nella dashboard (icone di icons.py)."""
    d = {"$type": T_IMAGE, "Image": img, "AutoSize": False, "BackgroundColor": "#00FFFFFF", "Height": float(h), "Left": float(x),
         "Top": float(y), "Visible": True, "BlinkPhasisInverted": False, "Width": float(w), "Name": name, "RenderingSkip": 0,
         "MinimumRefreshIntervalMS": 0.0, "_pv": {"kind": "image", "img": img}}
    if bind_js:
        d["Bindings"] = {"Image": js(bind_js)}
    return d


SECTOR_GREY = "#FF4A4A4A"
DIV_COL = "#FF4A4A4A"     # divisori leggeri dentro le schede


def vdiv(name, x, y, h):
    return rect(name, round(x) - 1, y, 2, h, bg=DIV_COL, radius=0)


def hdiv(name, x, y, w):
    return rect(name, x, round(y) - 1, w, 2, bg=DIV_COL, radius=0)


def sector_bars(x, y, w, h, gap=4):
    """3 barre dei settori: verde = tuo migliore (striscia viola = migliore della classe), viola = migliore di tutti, giallo = piu' lento."""
    bw = (w - 2 * gap) / 3.0
    samples = ["#FFD500F9", "#FFD500F9", SECTOR_GREY]
    it = []
    for i in range(3):
        it.append(rect("Settore %d" % (i + 1), x + i * (bw + gap), y, bw, h, bg=samples[i], radius=min(3, h / 2), bindings={
            "BackgroundColor": js("var c = $prop('DTPlugin.Settore_%d_Colore');\r\nreturn c ? c : '%s';" % (i + 1, SECTOR_GREY))}))
    # striscia viola sotto: tuo migliore ed e' anche il migliore della tua classe
    sh = max(2, int(round(h * 0.28)))
    for i in range(3):
        it.append(rect("Settore %d classe" % (i + 1), x + i * (bw + gap), y + h - sh, bw, sh, bg="#FFD500F9", radius=min(3, sh // 2), bindings={
            "Visible": js("return $prop('DTPlugin.Settore_%d_Striscia') == true;" % (i + 1))}))
    return it


def status_items(ix, iy, iw, ih):
    """Blocco stato: luci, bandiera, tergicristalli, motore."""
    row = iw >= ih * 2.2
    cols, rows = (4, 1) if row else (2, 2)
    g = 0                                   # riquadri attaccati, senza spazi
    tw, th = (iw - (cols - 1) * g) / cols, (ih - (rows - 1) * g) / rows
    tiles = [
        ("Luci", "return $prop('DTPlugin.Stato_Luci') > 0 ? '#FF0D3A5A' : '#FF242424';",
         "return $prop('DTPlugin.Stato_Luci') > 0 ? 'dt-luci-on' : 'dt-luci-off';", "dt-luci-off", "#FF242424"),
        ("Bandiera", "var c = $prop('DTPlugin.Stato_BandieraColore');\r\nreturn c ? c : '#FF242424';",
         "var f = $prop('DTPlugin.Stato_Bandiera');\r\nif (!f) { return 'dt-bandiera-off'; }\r\n"
         "return (f == 'Yellow' || f == 'White' || f == 'Orange' || f == 'Green') ? 'dt-bandiera-nera' : 'dt-bandiera-bianca';",
         "dt-bandiera-off", "#FF242424"),
        ("Tergi", "return $prop('DTPlugin.Stato_Tergi') > 0 ? '#FF0E3B3E' : '#FF242424';",
         "return $prop('DTPlugin.Stato_Tergi') > 0 ? 'dt-tergi-on' : 'dt-tergi-off';", "dt-tergi-on", "#FF0E3B3E"),
        ("Motore", "var m = $prop('DTPlugin.Stato_Motore');\r\nreturn m == 2 ? '#FF0F3D1A' : (m == 1 ? '#FF3D3500' : '#FF242424');",
         "var m = $prop('DTPlugin.Stato_Motore');\r\nreturn m == 2 ? 'dt-motore-on' : (m == 1 ? 'dt-motore-acc' : 'dt-motore-off');",
         "dt-motore-on", "#FF0F3D1A"),
    ]
    it = []
    for i, (nm, bgjs, imgjs, sample_img, sample_bg) in enumerate(tiles):
        r, k = divmod(i, cols)
        x, y = ix + k * (tw + g), iy + r * (th + g)
        it.append(rect("Stato %s fondo" % nm, x, y, tw, th, bg=sample_bg, radius=0, bindings={"BackgroundColor": js(bgjs)}))
        sz = min(tw, th) * 0.7
        it.append(image("Stato %s icona" % nm, x + (tw - sz) / 2, y + (th - sz) / 2, sz, sz, sample_img, imgjs))
        if nm == "Tergi":
            fs = clamp(int(sz * 0.32), 8, 30)
            it.append(text("Stato tergi modo", x + tw - sz * 0.45 - 2, y + th - sz * 0.42 - 2, sz * 0.45, sz * 0.42, None, "A", fs, "#FF26C6DA",
                           weight="Black", bindings={"Text": js("var t = $prop('DTPlugin.Stato_Tergi');\r\nreturn t == 1 ? 'A' : (t == 2 ? '1' : (t == 3 ? '2' : ''));")}))
    return it


def layer(name, children, vis_expr=None, pv_vis=None, vis_js=None):
    d = {"$type": T_LAYER, "Group": False, "Repetitions": 0, "Childrens": children, "Visible": True,
         "BlinkPhasisInverted": False, "IsFreezed": False, "Name": name, "RenderingSkip": 0, "MinimumRefreshIntervalMS": 0.0}
    if vis_js:
        d["Bindings"] = {"Visible": js(vis_js)}
    elif vis_expr:
        d["Visible"] = False  # nascosto se il plugin non risponde
        d["Bindings"] = {"Visible": ncalc(vis_expr)}
    if pv_vis and "Bindings" not in d:
        raise ValueError("livello '%s' senza regola di visibilita'" % name)
    d["_pv"] = dict(kind="layer", vis=pv_vis)
    return d


def widget(name, x, y, w, h, filename, slot_prop):
    d = {"$type": T_WIDGET, "NextScreenCommand": 0, "PreviousScreenCommand": 0, "AutoSize": True, "FileName": filename,
         "InitialScreenIndex": 0, "FreezePageChanges": False, "EnableScreenRolesAndActivation": True,
         "IgnoreSavedScreensEx": True}
    d.update(base(name, x, y, w, h, {"InitialScreenIndex": js(
        "var v = $prop('%s%s');\r\nreturn (v == null) ? %d : v;" % (P, slot_prop, DEFAULT_SLOT.get(slot_prop, 0)))}))
    d["_pv"] = dict(kind="widget", file=filename, slot=slot_prop)
    return d


D0 = DEFAULT_PAGES[0]
DEFAULT_SLOT = {"Cornice": D0["frame"], "Griglia_L": D0["grid_l"], "Griglia_R": D0["grid_r"], "BarraInferiore": D0["toolbar"],
                "Slot_SL": D0["SL"], "Slot_SR": D0["SR"]}
for _k in ("L", "R", "T", "C"):
    for _i, _v in enumerate(D0[_k]):
        DEFAULT_SLOT["Slot_%s%d" % (_k, _i + 1)] = _v


def vis_eq(prop, val):
    return ("var v = $prop('%s%s');\r\nif (v == null) { v = %d; }\r\nreturn v == %d;" % (P, prop, DEFAULT_SLOT[prop], val))


def missing_banner():
    return layer("Plugin non attivo", [
        rect("Avviso plugin fondo", 175, 395, 500, 70, bg="#F0400000", border=RED, bw=2, radius=12),
        text("Avviso plugin", 175, 395, 500, 38, None, "FEDE PLUGIN NON ATTIVO", 22, WHITE, weight="Black"),
        text("Avviso plugin 2", 175, 428, 500, 30, None, "Copia DTPlugin.dll nella cartella di SimHub e riavvia", 14, GREY, weight="Medium"),
    ], vis_js="return $prop('DTPlugin.Pagine') == null;", pv_vis=("PluginMancante", 1))


def screen(name, items, ingame=True, idle=False, bg="#00FFFFFF"):
    return {"RenderingSkip": 0, "Name": name, "InGameScreen": ingame, "IdleScreen": idle, "PitScreen": False,
            "ScreenId": str(uuid.uuid4()), "AllowOverlays": True, "IsForegroundLayer": False, "IsOverlayLayer": False,
            "OverlayTriggerExpression": {"Expression": ""}, "ScreenEnabledExpression": {"Expression": ""},
            "OverlayMaxDuration": 0, "OverlayMinDuration": 0, "IsBackgroundLayer": False, "BackgroundColor": bg,
            "Items": items, "MinimumRefreshIntervalMS": 0.0}


def metadata(w, h, screens, title=None, ingame=None, idle=None, preview=0):
    m = {"SettingsBuilder": {"Settings": [], "IsEditMode": False, "Prefix": None},
         "ScreenCount": float(len(screens)),
         "InGameScreensIndexs": ingame if ingame is not None else list(range(len(screens))),
         "IdleScreensIndexs": idle or [], "MainPreviewIndex": preview, "IsOverlay": False, "OverlaySizeWarning": False,
         "MetadataVersion": 2.0, "EnableOnDashboardMessaging": True, "PitScreensIndexs": [], "PreferredTouchMode": 0,
         "SimHubVersion": "9.12.9"}
    if title:
        m.update({"Category": "", "Title": title,
                  "Description": "Dashboard personale con layout e schede scelti dal DT Plugin",
                  "Author": "DT Racing Team"})
    m.update({"Width": float(w), "Height": float(h), "DashboardVersion": "1.0"})
    return m


def dash_doc(w, h, screens, meta, controls=False):
    return {"Variables": {"DashboardVariables": []},
            "DashboardDebugManager": {"_dashSettingsStores": {}, "WindowPositionSettings": {
                "IsFullScreen": False, "Position": "100,100,%d,%d" % (w, h), "TopMost": False, "AllowTransparency": False,
                "CloseOnMonitorLoss": False, "NoWindowActivate": False}},
            "Version": 2, "Id": str(uuid.uuid4()), "BaseHeight": h, "BaseWidth": w, "BackgroundColor": "#00000000" if not controls else "#FF000000",
            "Screens": screens, "SnapToGrid": False, "HideLabels": True, "ShowForeground": True, "ForegroundOpacity": 100.0,
            "ShowBackground": True, "BackgroundOpacity": 100.0, "ShowBoundingRectangles": False, "GridSize": 10,
            "Images": [], "Metadata": meta, "ShowOnScreenControls": controls, "IsOverlay": False,
            "EnableClickThroughOverlay": True, "EnableOnDashboardMessaging": True, "UseStrictJSIsolation": False,
            "UseStrictJSIsolationWarning": True}


# ------------------------------------------------------------------ dimensioni testo
def fit(w, h, chars, hfrac=0.8, wfrac=0.575, cap=200):
    return int(max(7, min(h * hfrac, w / (max(chars, 1) * wfrac), cap)))


def clamp(v, a, b):
    return max(a, min(b, v))


def ch_val(c):
    return "[%s%s]" % (P, c)


def ch_name(c):
    return "[%s%s_Nome]" % (P, c)


def label_color(c):
    return GREY if c in (FRAME, WHITE) else c


DELTA_COLOR_JS = ("var d = $prop('DTPlugin.%s_Valore');\r\n"
                  "if (d == null || d == 0) { return '#FFFFFFFF'; }\r\n"
                  "return d < 0 ? '#FF00E676' : '#FFFF1744';")


# ------------------------------------------------------------------ schede
def gauge_items(card, w, h, items_ref=None):
    """Barra di riempimento (carburante / energia): al centro i giri che puoi ancora fare, sotto tra parentesi la percentuale."""
    c, lp, pc, suf = card["ch"], card["laps"], card["pct"], card.get("suffix", "")
    it = []
    lh = 0
    if h >= 44 and items_ref is not None:
        lfs = clamp(int(min(h * 0.22, (w - 24) / (len(card["title"]) * 0.62))), 8, 19)
        lh = int(legend(items_ref, w, h, card["title"], lfs, GREY) * 0.8)
    bx, by, bw_, bh_ = 5, lh + 2, w - 10, h - lh - 6
    r = int(min(8, bh_ / 4))
    it.append(rect("Barra fondo", bx, by, bw_, bh_, bg="#FF1A1A1A", radius=r))
    pv = "Math.max(0, Math.min(100, $prop('DTPlugin.%s_Valore')))" % pc
    it.append(rect("Barra", bx, by, bw_ * 0.74, bh_, bg=card["fill"], radius=r, bindings={"Width": js("return %s * %d / 100;" % (pv, bw_))},
                   pv=dict(hfrac=0.74)))
    # al centro in grande i giri che puoi ancora fare, sotto tra parentesi la percentuale
    gtxt = "var g = fd('%s');\r\nreturn (g && g !== '') ? g : '-';" % lp
    ptxt = "var t = fd('%s');\r\nif (!t || t === '-') { return '(-)'; }\r\nt = '' + t; if (t.indexOf('%%') < 0) { t = t + '%%'; }\r\nreturn '(' + t + ')';" % pc
    gh_ = bh_ * 0.66
    vfs = fit(bw_ - 10, gh_, 4, 0.95, wfrac=0.58)
    it.append(text("Giri", bx, by, bw_, gh_ + 2, None, CH[lp][7], vfs, WHITE, va=2, bindings={"Text": js(gtxt)}))
    qfs = clamp(int(min((bh_ - gh_) * 0.85, (bw_ - 8) / (7 * 0.56))), 8, 40)
    it.append(text("Percentuale", bx, by + gh_, bw_, bh_ - gh_, None, "(" + CH[pc][7].replace("%", "") + "%)", qfs, WHITE, va=0, weight="Medium",
                   bindings={"Text": js(ptxt)}))
    return it


def iconrow_items(card, w, h):
    """Icona + valore per ogni dato (meteo: pista, aria, pista bagnata, pioggia; motore: acqua, olio)."""
    ents = list(card["entries"])
    if len(ents) == 4 and (w < 200 or (h < 50 and w < 260)):
        ents = ents[:2]          # riquadro troppo basso e stretto: solo temperature pista e aria
    n = len(ents)
    iw, ih = w - 8, h - 6
    it = []
    meteo = card.get("name") == "Meteo"
    row = ih < 50 or iw / max(ih, 1) > 3.5
    if meteo:
        # stato della pista (ASCIUTTO / UMIDO / BAGNATO / PIOGGIA): a destra se in riga, sotto se a griglia
        if row:
            bw = clamp(w * 0.16, 34, 56); bh = min(ih - 8, 24)
            bx, by = w - 4 - bw, 3 + (ih - bh) / 2
            iw -= bw + 4
        else:
            bh = clamp(ih * 0.18, 14, 22); bw = clamp(w * 0.3, 40, 80)
            bx, by = (w - bw) / 2, h - 4 - bh
            ih -= bh + 4
        it.append(rect("Pista stato fondo", bx, by, bw, bh, bg="#FFE0E0E0", radius=int(min(6, bh / 3)),
                       bindings={"BackgroundColor": js("var c = $prop('DTPlugin.Pista_StatoColore');\r\nreturn c ? c : '#FFE0E0E0';")}))
        it.append(text("Pista stato", bx, by, bw, bh, None, "ASC", min(15, fit(bw - 6, bh, 3, 0.7, 0.66)), "#FF111111", weight="Black",
                       bindings={"Text": js("var t = $prop('DTPlugin.Pista_Stato');\r\nreturn t ? t : '-';"),
                                 "TextColor": js("var c = $prop('DTPlugin.Pista_StatoTesto');\r\nreturn c ? c : '#FF111111';")}))
    if row:
        cols, rows = n, 1
    else:
        cols, rows = best_grid(n, iw, ih, 6)
    cw, rh = iw / cols, ih / rows
    # stessa grandezza per tutti i valori (prima "0%" veniva piu' grande di "38°")
    chars = max(max(len(CH[c][7]) for _, c in ents), 3)
    sz = min(rh * 0.5, cw * 0.26)
    vfs = fit(cw - sz - 14, rh, chars, 0.62 if row else 0.74, wfrac=0.58)
    sz = min(sz, vfs * 0.95)
    for i, (ico, c) in enumerate(ents):
        r, k = divmod(i, cols)
        x, y = 4 + k * cw, 3 + r * rh
        # icona e valore affiancati e centrati nella cella
        est = sz + 4 + vfs * 0.6 * len(CH[c][7])
        x0 = x + max(4, (cw - est) / 2)
        it.append(image("Icona %s" % c, x0, y + (rh - sz) / 2, sz, sz, ico))
        it.append(text("Valore %s" % c, x0 + sz + 4, y, max(10, x + cw - (x0 + sz + 4) - 2), rh, ch_val(c), CH[c][7], vfs, WHITE, ha=0))
    # divisori tra i dati (e prima dello stato della pista): bassi, centrati
    dh = min(ih * 0.5, vfs * 1.25) if row else min(rh * 0.6, vfs * 1.3)
    for k in range(1, cols):
        for r in range(rows):
            it.append(vdiv("Divisore %d %d" % (k, r), 4 + k * cw, 3 + r * rh + (rh - dh) / 2, dh))
    for r in range(1, rows):
        it.append(hdiv("Divisore riga %d" % r, 4 + iw * 0.06, 3 + r * rh, iw * 0.88))
    if meteo and row:
        it.append(vdiv("Divisore stato", bx - 3, 3 + (ih - dh) / 2, dh))
    return it


GAP_INFO = {"Avanti": ("GapAhead", "AVANTI", GREEN, "S. Gattuso", "14", "GT3"),
            "Dietro": ("GapBehind", "DIETRO", RED, "J. Adam", "15", "GT3")}


def gap_side_items(side, x, y, w, h):
    """Distacco + nome del pilota + categoria (e numero) dell'auto davanti o dietro."""
    ch, label, col, nm, num, cls = GAP_INFO[side]
    P_ = "DTPlugin.Box_%s_" % side
    it = []
    name_b = {"Text": js("var n = $prop('%sNome');\r\nreturn n ? n : '';" % P_)}
    gap_b = {"Text": js("var g = $prop('%sDistaccoScheda');\r\nif (g == null) { return fd('%s'); }\r\nreturn g;" % (P_, ch))}

    def badges(bx, by, bh, maxw):
        bh = min(bh, 24)
        fs = clamp(int(bh * 0.7), 7, 16)
        cw = min(maxw, fs * 4.2)
        fs = max(7, min(fs, int((cw - 4) / (3 * 0.66))))   # la classe (es. GT3, LMP2) deve stare nel riquadrino
        vis = {"Visible": js("var n = $prop('%sClasse');\r\nreturn n != null && n !== '';" % P_)}
        g = [rect("%s classe fondo" % side, bx, by, cw, bh, bg="#FFC62828", radius=3,
                  bindings={"BackgroundColor": js("var c = $prop('%sClasseColore');\r\nreturn c ? c : '#FFC62828';" % P_)}),
             text("%s classe" % side, bx, by, cw, bh, "[%sClasse]" % P_, cls, fs, WHITE, weight="Black")]
        lay = layer("%s categoria" % side, g)
        lay["Bindings"] = vis
        return [lay]

    # distacco grande in alto; sotto nome + categoria affiancati; in fondo il suo ultimo giro
    if h < 44 and w >= 120:
        # riquadro basso (righe centrali): distacco a sinistra, nome e categoria a destra, su una riga
        gw = w * 0.38
        it.append(text("%s distacco" % side, x + 4, y, gw - 4, h, None, CH[ch][7], fit(gw - 8, h, 5, 0.78), col, ha=0, bindings=gap_b))
        bhh = min(h * 0.5, 16)
        bfs = clamp(int(bhh * 0.7), 7, 12)
        bwd = min(bfs * 4.0, w * 0.2)
        nx, nw = x + gw, w - gw - bwd - 10
        it.append(text("%s nome" % side, nx, y, nw, h, None, nm, max(7, int(min(h * 0.42, nw / (11 * 0.56)))), WHITE, weight="Medium", ha=2,
                       bindings={"Text": js("var n = $prop('%sNomeScheda');\r\nif (!n) { n = $prop('%sNome'); }\r\nreturn n ? n : '';" % (P_, P_))}))
        it += badges(x + w - bwd - 4, y + (h - bhh) / 2, bhh, bwd)
        return it
    lh = clamp(round(h * 0.13), 9, 15) if h >= 90 else 0
    if lh:
        it.append(text("%s etichetta" % side, x, y, w, lh, None, label, int(lh * 0.8), GREY, weight="Medium"))
    ah = h - lh
    vh, nh = ah * 0.48, ah * 0.28
    th_ = ah - vh - nh
    it.append(text("%s distacco" % side, x, y + lh, w, vh, None, CH[ch][7], fit(w - 6, vh, 5, 0.9), col, bindings=gap_b))
    ny = y + lh + vh
    nfs = fit(w * 0.62, nh, 11, 0.8)
    bhh = max(9, min(nh * 0.78, 18))
    bw_ = clamp(int(bhh * 0.7), 7, 13) * 4.0
    split = x + w * 0.66                     # nome a destra fino qui, categoria subito dopo
    it.append(text("%s nome" % side, x + 2, ny, split - x - 4, nh, None, nm, nfs, WHITE, weight="Medium", ha=2,
                   bindings={"Text": js("var n = $prop('%sNomeScheda');\r\nif (!n) { n = $prop('%sNome'); }\r\nreturn n ? n : '';" % (P_, P_))}))
    it += badges(split + 2, ny + (nh - bhh) / 2, bhh, min(bw_, x + w - split - 4))
    it.append(text("%s ultimo giro" % side, x, ny + nh, w, th_, None, "1:44.812", fit(w - 10, th_, 8, 0.8), GREY, weight="Medium",
                   bindings={"Text": js("var t = $prop('%sUltimoGiro');\r\nreturn (t == null || t === '') ? '-' : t;" % P_)}))
    return it


STD_SAMPLE = [("5", "GT3", "S. Bovy", "-19.4", False), ("6", "GT3", "M. Berry", "-13.9", False), ("7", "GT3", "F. Pegoraro", "", True),
              ("8", "GT3", "M. Birch", "+1.4", False), ("9", "GT3", "R. Agostini", "+13.6", False), ("10", "GT3", "M. Cressoni", "+45.2", False),
              ("11", "LMP2", "A. Rossi", "+1L", False), ("12", "GT3", "L. Verdi", "+1L", False)]


FC_SAMPLE = [("INIZIO", 1, "24°", "10%"), ("25%", 2, "23°", "30%"), ("50%", 3, "21°", "70%"), ("75%", 3, "20°", "80%"), ("FINE", 1, "22°", "20%")]


SES_GREY = "#FF8A8A8A"


SES_W_JS = ("function W(s, f) { var n = 0; s = '' + s;\r\n"
            "for (var i = 0; i < s.length; i++) { var c = s.charAt(i); n += (c == ':' || c == '.') ? 0.32 : (c == '/' ? 0.42 : 0.62); }\r\n"
            "return n * f; }\r\n")


def _ses_w(sv, f):
    return sum(0.32 if c in ":." else (0.42 if c == "/" else 0.62) for c in sv) * f


def centered_pair(prefix, bx, bw, y, h, a_js, b_js, a_sample, b_sample, fa, fb, col_b, gap=0, hb=None):
    """Due testi affiancati (bianco + grigio) sempre centrati insieme nel blocco, qualunque sia la lunghezza.
    a_js / b_js: istruzioni che assegnano le variabili a e b."""
    hb = hb or h
    c = bx + bw / 2.0
    calc = SES_W_JS + a_js + "\r\n" + b_js + "\r\nvar s = %f - (W(a, %f) + %f + W(b, %f)) / 2;\r\n" % (c, fa, gap, fb)
    wa, wb = _ses_w(a_sample, fa), _ses_w(b_sample, fb)
    st = c - (wa + gap + wb) / 2
    # il bianco parte sempre dall'inizio del blocco (allineato a destra): cambia solo la larghezza
    A = text(prefix + " principale", bx, y, max(1, st + wa - bx), h, None, a_sample, fa, WHITE, weight="Black", ha=2, va=2,
             bindings={"Text": js(a_js + "\r\n" + b_js + "\r\nreturn a;"),
                       "Width": js(calc + "return Math.max(1, s + W(a, %f) - %f);" % (fa, bx))})
    B = text(prefix + " secondo", st + wa + gap, y, bw, hb, None, b_sample, fb, col_b, weight="Bold", ha=0, va=2,
             bindings={"Text": js(a_js + "\r\n" + b_js + "\r\nreturn b;"), "Left": js(calc + "return s + W(a, %f) + %f;" % (fa, gap))})
    return [A, B]


def session_items(w, h):
    """Sessione come le dash da gara: 18:39 (minuti bianchi, secondi grigi), 25/25 (posizione bianca, totale grigio), giri.
    Sotto in piccolo le etichette; accanto a POS le posizioni guadagnate (verde) o perse (rossa). Ogni blocco sempre centrato."""
    if h >= 100 and w / float(h) < 1.6:
        return session_items_vertical(w, h)       # riquadro alto e stretto: tempo, posizione e giro uno sotto l'altro
    it = []
    ix, iw = 6, w - 12
    lh = clamp(round(h * 0.23), 10, 15)
    vh = min(h - lh - 8, max(52, h * 0.45))       # riquadri alti: numeri piu' grandi
    vy = max(2, (h - vh - lh) / 2 - 3)        # numeri + etichette centrati, un po' staccati dal bordo in basso
    ly = vy + vh
    wts = [1.05, 1.25, 1.05]
    ws = [iw * t / sum(wts) for t in wts]
    pad = min(14, min(ws) * 0.12)
    vfs = min(fit(ws[0] - pad, vh, 5, 0.8, 0.62), fit(ws[1] - pad, vh, 5, 0.8, 0.62), fit(ws[2] - pad, vh, 4.2, 0.8, 0.62))
    # scritte sotto: ci devono stare nel loro blocco (QUALIFICA, "(3) POS")
    lfs = clamp(int(min(lh * 0.82, min(ws) / (9 * 0.62))), 7, 12)
    # numeri + scritte centrati in altezza sulla parte visibile (cifre alte ~0.72 del carattere)
    gh = vfs * 0.72
    top_vis = (h - (gh + 6 + lfs)) / 2.0
    vy = top_vis + gh + vfs * 0.21 - vh
    ly = top_vis + gh + 6 - lfs * 0.12
    x = ix
    # tempo: parte prima dei secondi bianca, ":ss" grigio; con le ore (1:29:30) solo "1h29"
    w0 = ws[0]
    t_pre = ("var t = fd('TimeLeft'); var a = '-', b = '';\r\n"
             "if (t && t !== '-') { var p = ('' + t).split(':');\r\n"
             "  if (p.length >= 3 && p[0] != '0' && p[0] != '00') { a = parseInt(p[0], 10) + 'h' + p[1]; }\r\n"
             "  else { var i = t.lastIndexOf(':'); if (i < 0) { a = t; } else { a = t.substring(0, i); b = t.substring(i); } } }")
    it += centered_pair("Tempo", x, w0, vy, vh, t_pre, "", "18", ":39", vfs, vfs, SES_GREY)
    # sotto il tempo: il tipo di sessione (PROVE, QUALIFICA, GARA...)
    it.append(text("Tempo etichetta", x, ly, w0, lh, None, "GARA", lfs, WHITE, weight="Bold",
                   bindings={"Text": js("var t = $prop('DTPlugin.Sessione_Nome');\r\nreturn t ? t : 'TEMPO';")}))
    x += w0
    # posizione: "25" bianco, "/25" grigio; sotto freccia + numero posizioni e "POS"
    w1 = ws[1]; sp = x + w1 * 0.5
    p_a = "var p = fd('Pos'); var a = '-';\r\nif (p && p !== '-') { var i = ('' + p).indexOf('/'); a = i < 0 ? p : ('' + p).substring(0, i); }"
    p_b = "var n = fd('Cars'); var b = (n && n !== '-' && n != '0') ? '/' + n : '';"
    it += centered_pair("Pos", x, w1, vy, vh, p_a, p_b, "25", "/25", vfs, vfs, SES_GREY)
    # sotto: "(freccia 3) POS"  /  "(0) POS"
    ad = lfs * 0.85
    pl = sp - 2                                   # le parentesi finiscono qui, poi "POS"
    gtxt = js("var d = $prop('DTPlugin.PosGain_Dir');\r\nvar g = fd('PosGain');\r\nif (!g || g === '-') { g = '0'; }\r\nreturn g + ')';")
    gcol = js("var c = $prop('DTPlugin.PosGain_Colore');\r\nreturn c ? c : '#FF9E9E9E';")
    nw = lfs * 1.2
    it.append(text("Pos guadagno", pl - nw, ly, nw, lh, None, "3)", lfs, GREEN, weight="Bold", ha=2, bindings={"Text": gtxt, "TextColor": gcol}))
    ax = pl - nw - ad - 1
    it.append(pos_arrow("Pos", ax, ly + (lh - ad) / 2, ad))
    it.append(text("Pos parentesi", ax - lfs * 0.45, ly, lfs * 0.45, lh, None, "(", lfs, GREY, weight="Medium", ha=2,
                   bindings={"Left": js("var d = $prop('DTPlugin.PosGain_Dir');\r\nreturn (d != null && d != 0) ? %d : %d;" % (round(ax - lfs * 0.45), round(ax + ad - lfs * 0.45)))}))
    it.append(text("Pos etichetta", sp + 3, ly, x + w1 - sp - 3, lh, None, "POS", lfs, GREY, weight="Medium", ha=0))
    x += w1
    # giro: numero bianco e, piu' piccoli in grigio, i giri totali stimati come LMU ("/~27.1")
    w2 = ws[2]
    tfs = max(8, int(vfs * 0.55))
    g_a = "var a = fd('Lap'); if (!a) { a = '-'; }"
    g_b = "var t = $prop('DTPlugin.GiriTotali'); var b = (t == null || t === '') ? '' : '/' + t;"
    it += centered_pair("Giro", x, w2, vy, vh, g_a, g_b, "4", "/~27.1", vfs, tfs, SES_GREY, gap=vfs * 0.12, hb=vh - vfs * 0.08)
    it.append(text("Giro etichetta", x, ly, w2, lh, None, "GIRO", lfs, GREY, weight="Medium"))
    # divisori tra tempo, posizione e giro
    dy0, dy1 = top_vis - 5, top_vis + gh + 6 + lfs + 5
    it.append(vdiv("Divisore 1", ix + ws[0], dy0, dy1 - dy0))
    it.append(vdiv("Divisore 2", ix + ws[0] + ws[1], dy0, dy1 - dy0))
    return it


def session_items_vertical(w, h):
    """Sessione in verticale: tempo (con PROVE/QUALIFICA/GARA), posizione (con posizioni guadagnate) e giro, uno sotto l'altro."""
    it = []
    ix, iw = 8, w - 16
    rh = (h - 8) / 3.0
    vfs = int(min(fit(iw - 10, rh * 0.62, 6, 0.85, 0.62), 60))
    lfs = clamp(int(min(rh * 0.16, iw / (9 * 0.62))), 8, 14)
    gh = vfs * 0.72
    gap = max(4, rh * 0.05)
    blk = gh + gap + lfs * 0.8
    vbox = vfs * 1.25
    rows = []
    for r in range(3):
        top = 4 + r * rh + (rh - blk) / 2
        rows.append((top + gh + vfs * 0.21 - vbox, top + gh + gap - lfs * 0.15))
    t_pre = ("var t = fd('TimeLeft'); var a = '-', b = '';\r\n"
             "if (t && t !== '-') { var p = ('' + t).split(':');\r\n"
             "  if (p.length >= 3 && p[0] != '0' && p[0] != '00') { a = parseInt(p[0], 10) + 'h' + p[1]; }\r\n"
             "  else { var i = t.lastIndexOf(':'); if (i < 0) { a = t; } else { a = t.substring(0, i); b = t.substring(i); } } }")
    vy, ly = rows[0]
    it += centered_pair("Tempo", ix, iw, vy, vbox, t_pre, "", "18", ":39", vfs, vfs, SES_GREY)
    it.append(text("Tempo etichetta", ix, ly, iw, lfs * 1.4, None, "GARA", lfs, WHITE, weight="Bold", va=0,
                   bindings={"Text": js("var t = $prop('DTPlugin.Sessione_Nome');\r\nreturn t ? t : 'TEMPO';")}))
    vy, ly = rows[1]
    p_a = "var p = fd('Pos'); var a = '-';\r\nif (p && p !== '-') { var i = ('' + p).indexOf('/'); a = i < 0 ? p : ('' + p).substring(0, i); }"
    p_b = "var n = fd('Cars'); var b = (n && n !== '-' && n != '0') ? '/' + n : '';"
    it += centered_pair("Pos", ix, iw, vy, vbox, p_a, p_b, "25", "/25", vfs, vfs, SES_GREY)
    # sotto: "(freccia 3)" a sinistra del centro, "POS" a destra
    cxm = ix + iw / 2
    lhh = lfs * 1.4
    ad = lfs * 0.85
    gtxt = js("var g = fd('PosGain');\r\nif (!g || g === '-') { g = '0'; }\r\nreturn g + ')';")
    gcol = js("var c = $prop('DTPlugin.PosGain_Colore');\r\nreturn c ? c : '#FF9E9E9E';")
    nw = lfs * 1.3
    it.append(text("Pos guadagno", cxm - 3 - nw, ly, nw, lhh, None, "3)", lfs, GREEN, weight="Bold", ha=2, va=0, bindings={"Text": gtxt, "TextColor": gcol}))
    ax = cxm - 3 - nw - ad - 1
    it.append(pos_arrow("Pos", ax, ly + lfs * 0.1, ad))
    it.append(text("Pos parentesi", ax - lfs * 0.45, ly, lfs * 0.45, lhh, None, "(", lfs, GREY, weight="Medium", ha=2, va=0,
                   bindings={"Left": js("var d = $prop('DTPlugin.PosGain_Dir');\r\nreturn (d != null && d != 0) ? %d : %d;" % (round(ax - lfs * 0.45), round(ax + ad - lfs * 0.45)))}))
    it.append(text("Pos etichetta", cxm + 3, ly, iw / 2 - 3, lhh, None, "POS", lfs, GREY, weight="Medium", ha=0, va=0))
    vy, ly = rows[2]
    g_a = "var a = fd('Lap'); if (!a) { a = '-'; }"
    g_b = "var t = $prop('DTPlugin.GiriTotali'); var b = (t == null || t === '') ? '' : '/' + t;"
    tfs = max(8, int(vfs * 0.55))
    it += centered_pair("Giro", ix, iw, vy, vbox, g_a, g_b, "4", "/~27.1", vfs, tfs, SES_GREY, gap=vfs * 0.12, hb=vbox - vfs * 0.08)
    it.append(text("Giro etichetta", ix, ly, iw, lhh, None, "GIRO", lfs, GREY, weight="Medium", va=0))
    for r in (1, 2):
        it.append(hdiv("Divisore %d" % r, ix + iw * 0.08, 4 + r * rh, iw * 0.84))
    return it


def forecast_items(card, w, h):
    """Previsioni meteo di LMU per la sessione: inizio, 25%, 50%, 75%, fine (cielo, temperatura, probabilita' di pioggia)."""
    show_title = h >= 110
    th = clamp(round(h * 0.09), 12, 15) if show_title else 0
    it = []
    if show_title:
        it.append(text("Titolo", 6, 3, w - 12, th, None, card["title"], int(th * 0.75), GREY, weight="Medium"))
    idx = [1, 2, 3, 4, 5] if w >= 230 else [1, 3, 5]
    n = len(idx)
    iy, ih = th + 4, h - th - 8
    cw = (w - 8) / n
    # riquadro alto: le colonne restano compatte e centrate in altezza (non sparse fino in fondo)
    hb = min(ih, cw * 2.7 + 16)
    iy, ih = iy + (ih - hb) / 2, hb
    lh = clamp(ih * 0.16, 9, 16)
    rest = ih - lh
    full = rest >= 70
    ic = min(cw - 8, rest * (0.5 if full else 0.62))
    for j, k in enumerate(idx):
        x = 4 + j * cw
        P_ = "DTPlugin.Prev_%d_" % k
        lab, kind, t, r = FC_SAMPLE[k - 1]
        it.append(text("Prev %d ora" % k, x, iy, cw, lh, None, lab, int(lh * 0.8), GREY, weight="Medium",
                       bindings={"Text": js("var t = $prop('%sOra');\r\nreturn t ? t : '';" % P_)}))
        img = image("Prev %d cielo" % k, x + (cw - ic) / 2, iy + lh + 1, ic, ic, "dt-cielo-%d" % kind,
                    bind_js="var c = $prop('%sIcona');\r\nreturn 'dt-cielo-' + ((c == null || c === '') ? 2 : c);" % P_)
        img["Bindings"]["Visible"] = js("var c = $prop('%sIcona');\r\nreturn c != null && c !== '';" % P_)
        it.append(img)
        ty = iy + lh + ic + 2
        left = iy + ih - ty
        if full:
            th2 = left * 0.55
            it.append(text("Prev %d temp" % k, x, ty, cw, th2, None, t, fit(cw - 4, th2, 4, 0.8), WHITE,
                           bindings={"Text": js("var t = $prop('%sTemp');\r\nreturn t ? t : '-';" % P_)}))
            it.append(text("Prev %d pioggia" % k, x, ty + th2, cw, left - th2, None, r, fit(cw - 4, left - th2, 4, 0.75), "#FF64B5F6",
                           weight="Medium", bindings={"Text": js("var t = $prop('%sPioggia');\r\nreturn t ? t : '';" % P_)}))
        else:
            it.append(text("Prev %d temp" % k, x, ty, cw, left, None, t, fit(cw - 4, left, 4, 0.8), WHITE,
                           bindings={"Text": js("var t = $prop('%sTemp');\r\nreturn t ? t : '-';" % P_)}))
    return it


def standings_items(card, w, h):
    """Classifica intorno a te (generale o di classe): posizione, categoria, nome, distacco da te. La tua riga e' evidenziata."""
    sc = card["scope"]
    P_ = "DTPlugin.Std_%s_" % sc
    show_title = h >= 120
    th = clamp(round(h * 0.07), 12, 15) if show_title else 0
    it = []
    if show_title:
        it.append(text("Titolo", 6, 3, w - 12, th, None, card["title"], int(th * 0.75), GREY, weight="Medium"))
    px = clamp(round(w * 0.045), 10, 18)          # margine ai lati (prima era troppo attaccato al bordo)
    iy, ih = th + 7, h - th - 14
    k = int(clamp(ih // 22, 1, 15))
    rh = ih / k
    fs = clamp(int(rh * 0.62), 8, 18)
    pw = clamp(w * 0.12, 18, 34)
    show_cls = sc == "G" and w >= 200
    cw_ = clamp(w * 0.15, 26, 46) if show_cls else 0
    gw = clamp(w * 0.24, 36, 80)
    nx = px + pw + (cw_ + 6 if show_cls else 4)
    nw = w - nx - gw - px - 4
    for r in range(k):
        y = iy + r * rh
        pre = ("var n = $prop('%sN'); if (!n) { n = 0; }\r\nvar io = $prop('%sIo'); if (!io) { io = 0; }\r\n"
               "var s = Math.max(0, Math.min(io - %d, n - %d));\r\nvar a = s + %d + 1;\r\n" % (P_, P_, k // 2, k, r))
        fld = lambda f: pre + "if (a > n) { return ''; }\r\nvar v = $prop('%s' + a + '_%s');\r\nreturn v == null ? '' : v;" % (P_, f)
        sp = STD_SAMPLE[r] if r < len(STD_SAMPLE) else (str(5 + r), "GT3", "Pilota %d" % (r + 1), "+1L", False)
        me_js = pre + "return a <= n && $prop('%s' + a + '_Io') === true;" % P_
        hl = rect("Riga %d evidenza" % (r + 1), px - 5, y + 1, w - 2 * (px - 5), rh - 2, bg="#FF2E3B3A", radius=4)
        hl["Bindings"] = {"Visible": js(me_js)}
        hl["Visible"] = sp[4]
        it.append(hl)
        it.append(text("Riga %d posizione" % (r + 1), px, y, pw, rh, None, sp[0], fs, WHITE, weight="Black", ha=0,
                       bindings={"Text": js(fld("Pos"))}))
        if show_cls:
            bh = min(rh - 6, 18)
            g = [rect("Riga %d classe fondo" % (r + 1), px + pw, y + (rh - bh) / 2, cw_, bh, bg="#FFC62828", radius=3,
                      bindings={"BackgroundColor": js(pre + "var c = (a <= n) ? $prop('%s' + a + '_ClasseColore') : null;\r\nreturn c ? c : '#FFC62828';" % P_)}),
                 text("Riga %d classe" % (r + 1), px + pw, y + (rh - bh) / 2, cw_, bh, None, sp[1], clamp(int(bh * 0.62), 7, 12), WHITE,
                      weight="Black", bindings={"Text": js(fld("Classe"))})]
            cl = layer("Riga %d categoria" % (r + 1), g)
            cl["Bindings"] = {"Visible": js(pre + "return a <= n && ('' + $prop('%s' + a + '_Classe')) !== '';" % P_)}
            it.append(cl)
        nfs = int(min(fs, nw / (12 * 0.56)))          # nome: ci deve stare (il plugin lo accorcia a 12 lettere)
        it.append(text("Riga %d nome" % (r + 1), nx, y, nw, rh, None, sp[2], max(7, nfs), WHITE, weight="Medium", ha=0,
                       bindings={"Text": js(fld("Nome"))}))
        dfs = int(min(fs, gw / (6 * 0.6)))
        it.append(text("Riga %d distacco" % (r + 1), w - gw - px, y, gw, rh, None, sp[3], max(7, dfs), WHITE, ha=2,
                       bindings={"Text": js(fld("Distacco")),
                                 "TextColor": js(pre + "var g = (a <= n) ? ('' + $prop('%s' + a + '_Distacco')) : '';\r\n"
                                                 "return g.charAt(0) == '-' ? '#FF00E676' : (g.charAt(0) == '+' ? '#FFFF5252' : '#FFFFFFFF');" % P_)}))
    return it


def gaps_items(card, w, h):
    sides = card["sides"]
    show_title = bool(card.get("title")) and h >= 110
    th = clamp(round(h * 0.09), 12, 16) if show_title else 0
    it = []
    if show_title:
        it.append(text("Titolo", 6, 3, w - 12, th, None, card["title"], int(th * 0.75), GREY, weight="Medium"))
    iy, ih, iw = th + 3, h - th - 6, w - 8
    n = len(sides)
    side_by_side = n == 1 or iw / max(ih, 1) >= 1.25 or ih < 128
    for i, sd in enumerate(sides):
        if side_by_side:
            cw = iw / n
            it += gap_side_items(sd, 4 + i * cw, iy, cw, ih)
        else:
            rh = ih / n
            it += gap_side_items(sd, 4, iy + i * rh, iw, rh)
    if n == 2:
        if side_by_side:
            it.append(vdiv("Divisore", 4 + iw / 2, iy + ih * 0.1, ih * 0.8))
        else:
            it.append(hdiv("Divisore", 4 + iw * 0.06, iy + ih / 2, iw * 0.88))
    return it


STRAT_COLS = [("SOSTE", "Soste", 0.8, "1"), ("FINESTRA", "Finestra", 2.6, "GIRO 7 - GIRO 27"), ("RATIO", "Ratio", 0.9, "0.73"),
              ("GIRI ALLA FINE", "Giri", 1.25, "36.3"), ("LITRI NECESSARI", "Litri", 1.3, "42 l"), ("ENERGIA NECESSARIA", "EnergiaNec", 1.45, "61 %")]


def strategy_items(w, h):
    """Barra strategia gara (come quella LMU di RBR32): soste, finestra box, ratio, stint, giri alla fine, riempi a."""
    if w >= 600:
        cols = STRAT_COLS
    elif w >= 280:
        cols = [c for c in STRAT_COLS if c[1] in ("Soste", "Finestra", "Giri", "EnergiaNec")]
    else:
        cols = [c for c in STRAT_COLS if c[1] in ("Soste", "Finestra")]
    tot = sum(c[2] for c in cols)
    ix, iw = 4, w - 8
    hh = clamp(round(h * 0.3), 11, 22)
    vy, vh = 3 + hh + 2, h - hh - 8
    it = []
    x = ix
    SHORT = {"GIRI ALLA FINE": "GIRI FINE", "LITRI NECESSARI": "LITRI", "ENERGIA NECESSARIA": "ENERGIA"}

    def lab_of(lab, cw):
        return SHORT[lab] if (len(lab) * 7 * 0.62 > cw - 6 and lab in SHORT) else lab
    # stessa grandezza per tutte le intestazioni (la piu' piccola che ci sta)
    lab_fs = 15
    for (lab, key, wt, sample) in cols:
        if key is None:
            continue
        cw = iw * wt / tot
        l2 = lab_of(lab, cw)
        lab_fs = min(lab_fs, clamp(int(min(hh * 0.68, (cw - 6) / (len(l2) * 0.62))), 7, 15))
    for (lab, key, wt, sample) in cols:
        cw = iw * wt / tot
        if key is None:   # etichetta verticale "RIEMPI A"
            it.append(rect("Riempi fondo", x + 1, 3, cw - 2, h - 6, bg="#FF2A2A2A", radius=3))
            fs = clamp(int(min((cw - 4) / (6 * 0.62), (h - 10) / 3)), 7, 14)
            it.append(text("Riempi a", x, 3, cw, h - 6, None, "RIEMPI\nA", fs, GREY, weight="Bold"))
            x += cw
            continue
        lab = lab_of(lab, cw)                   # colonna stretta: scritta corta
        it.append(rect("%s intestazione" % lab, x + 1, 3, cw - 2, hh, bg="#FF2A2A2A", radius=3))
        it.append(text("%s etichetta" % lab, x + 1, 3, cw - 2, hh, None, lab, lab_fs,
                       "#FFBDBDBD", weight="Bold"))
        if key == "Finestra":
            it.append(rect("Finestra fondo", x + 1, vy, cw - 2, vh, bg="#FFD50000", radius=3, bindings={
                "BackgroundColor": js("var c = $prop('DTPlugin.Strat_FinestraColore');\r\nreturn c ? c : '#FF404040';")}))
            vfs = fit(cw - 12, vh, 18, 0.62)   # "BOX PROSSIMO GIRO" deve stare dentro
        else:
            vfs = fit(cw - 6, vh, max(4, len(sample)), 0.72)
        it.append(text("%s valore" % lab, x + 1, vy, cw - 2, vh, None, sample, vfs, WHITE, weight="Bold",
                       bindings={"Text": js("var v = $prop('DTPlugin.Strat_%s');\r\nreturn (v == null || v === '') ? '-' : v;" % key)}))
        x += cw
    # fuori gara: avviso sopra
    msg = [rect("Strategia fuori gara fondo", 2, 2, w - 4, h - 4, bg="#F0101010", radius=int(min(10, h / 4))),
           text("Strategia fuori gara", 2, 2, w - 4, h - 4, None, "STRATEGIA: SOLO IN GARA", fit(w - 20, h - 4, 24, 0.4), GREY, weight="Medium")]
    it.append(layer("Fuori gara", msg, None, ("StratFuori", 1), vis_js="return $prop('DTPlugin.Strat_Attiva') != '1';"))
    return it


CENTER_BW = 0      # bordo dei riquadri della parte centrale (il DT Plugin lo puo' cambiare)
CARD_BW = 2        # bordo delle schede ai lati e in basso


def center_card_sizes():
    return {(int(round(w)), int(round(h))) for fi in range(len(FRAMES)) for (x, y, w, h) in center_geom(fi)["rows"] + [center_geom(fi)["delta"]]}


def legend(items, w, h, label, lfs, color, bind_text=None):
    """Nome della scheda sul bordo superiore (come una "legenda"): il bordo passa a meta' della scritta.
    Sposta la cornice (items[0]) in basso di meta' scritta e mette un fondo nero dietro al nome. Restituisce dove inizia il contenuto."""
    lh = int(round(lfs * 1.25))
    top = lh // 2
    fr = items[0]
    fr["Top"] = float(top)
    fr["Height"] = float(round(h - top))
    tw = min(w - 16, len(label) * lfs * 0.64 + 10)
    items.append(rect("Nome fondo", (w - tw) / 2, 0, tw, lh, bg="#FF000000", radius=0))
    b = {"Text": bind_text} if bind_text else None
    items.append(text("Nome", (w - tw) / 2, 0, tw, lh, None, label, lfs, color, weight="Medium", bindings=b))
    return lh


def frame_rect(w, h, color):
    bw = CENTER_BW if (int(round(w)), int(round(h))) in center_card_sizes() else CARD_BW
    return rect("Cornice", 0, 0, w, h, bg=CARD_BG, border=color, bw=bw, radius=int(min(12, h / 4)))


def pos_arrow(prefix, x, y, d):
    """Freccia verde in su / rossa in giu' per le posizioni guadagnate o perse (DTPlugin.PosGain_Dir)."""
    img = image(prefix + " freccia", x, y, d, d, "dt-freccia-su",
                bind_js="return $prop('DTPlugin.PosGain_Dir') < 0 ? 'dt-freccia-giu' : 'dt-freccia-su';")
    img["Bindings"]["Visible"] = js("var v = $prop('DTPlugin.PosGain_Dir');\r\nreturn v != null && v != 0;")
    return img


def pos_with_gain(color, x, y, w, h, prefix, chars=None, vfs=None):
    """Posizione (11/25) con accanto, tra parentesi e della stessa grandezza, le posizioni guadagnate/perse: (freccia 3)."""
    items = []
    tall = h >= 40 and w >= 40
    vh = h * 0.72 if tall else h
    vfs = vfs or fit(w - 6, vh, 10, 0.95 if tall else 0.72, 0.56)
    valw = 5 * 0.56 * vfs                               # "11/25"
    gainw = (0.4 + 0.8 + 1.25) * vfs                    # "(" freccia "3)"
    gap = vfs * 0.25
    start = x + max(2, (w - valw - gap - gainw) / 2)
    items.append(text(prefix + " valore", start - vfs, y, valw + vfs, vh, ch_val("Pos"), CH["Pos"][7], vfs, color, ha=2, va=2 if tall else 1))
    gx = start + valw + gap
    gh = vh
    ad = vfs * 0.72
    ay = y + (vh - ad * 1.22) if tall else y + (h - ad) / 2 - vfs * 0.05
    vva = 2 if tall else 1
    items.append(text(prefix + " par1", gx, y, vfs * 0.45, gh, None, "(", vfs, GREY, weight="Medium", ha=0, va=vva))
    items.append(pos_arrow(prefix, gx + vfs * 0.38, ay, ad))
    nx_arrow, nx_none = gx + vfs * 0.38 + ad + 2, gx + vfs * 0.4
    items.append(text(prefix + " guadagno", nx_arrow, y, max(10, x + w - nx_none), gh, None, "3)", vfs, WHITE, ha=0, va=vva,
                      bindings={"Text": js("var g = fd('PosGain');\r\nreturn (g && g !== '-') ? g + ')' : '-)';"),
                                "TextColor": js("var c = $prop('DTPlugin.PosGain_Colore');\r\nreturn c ? c : '#FF9E9E9E';"),
                                "Left": js("var d = $prop('DTPlugin.PosGain_Dir');\r\nreturn (d != null && d != 0) ? %d : %d;" % (round(nx_arrow), round(nx_none)))}))
    if tall:
        lfs = clamp(int(min((h - vh) * 0.5, (w - 4) / (len(CH["Pos"][1]) * 0.6))), 7, 12)
        items.append(text(prefix + " nome", x, y + vh, w, h - vh, ch_name("Pos"), CH["Pos"][1], lfs, GREY, weight="Normal", va=0))
    return items


def lap_times_items(ents, ix, iy, iw, ih):
    """Tempi giro uno sotto l'altro: ogni tempo con la scritta sotto, i tre blocchi distribuiti in modo uguale
    (stesso spazio sopra, in mezzo e sotto). Le linee passano a meta' delle scritte PREVISTO e ULTIMO."""
    n = len(ents)
    rh = ih / n
    chars = max(CH[c][6] for c, _ in ents)
    vfs = int(fit(iw - 5, rh * 0.745, chars, 0.97, 0.56) * 0.86)
    lfs = clamp(int(min(rh * 0.255 * 0.55, (iw - 4) / (8 * 0.6))), 7, 11)
    gh = vfs * 0.72                     # altezza delle cifre
    blk = gh + 8 + lfs
    space = max(0, (ih - n * blk) / (n + 1))
    it = []
    for i, (c, col) in enumerate(ents):
        top = iy + space * (i + 1) + blk * i
        vh = vfs * 1.25
        it.append(text("%s %d valore" % (c, i), ix, top + gh + vfs * 0.21 - vh, iw, vh, ch_val(c), CH[c][7], vfs, col, va=2))
        ly = top + gh + 8 - lfs * 0.15
        it.append(text("%s %d nome" % (c, i), ix, ly, iw, lfs * 1.4, ch_name(c), CH[c][1], lfs, GREY, weight="Normal", va=0))
        if i < n - 1:
            cyl = ly + lfs * 0.62
            half = len(CH[c][1]) * lfs * 0.38 + 8
            cxm = ix + iw / 2
            x0, x1 = ix + iw * 0.06, ix + iw * 0.94
            it.append(hdiv("Divisore riga %d sx" % (i + 1), x0, cyl, cxm - half - x0))
            it.append(hdiv("Divisore riga %d dx" % (i + 1), cxm + half, cyl, x1 - cxm - half))
    return it


def entry_items(c, color, x, y, w, h, prefix, sub=None, chars=None, vfs=None, lfs=None):
    """Una voce (etichetta + valore) dentro un riquadro di gruppo."""
    forced = vfs
    same = chars is not None          # riga unica: stessa grandezza per tutti i numeri
    sample = CH[c][7]
    chars = chars or max(CH[c][6], len(sample))
    items = []
    if c == "PosGain":
        # numero di posizioni con la freccia accanto
        if h >= 40 and w >= 40:
            vh = h * 0.72
            vfs = fit(w - 6, vh, chars, 0.95, 0.56) if same else fit(w * 0.5, vh, 2, 0.95, 0.56)
            ad = min(vh * 0.6, w * 0.3, vfs * 0.75)
            items.append(pos_arrow(prefix, x + w / 2 - ad - 2, y + vh - ad - vh * 0.08, ad))
            items.append(text(prefix + " valore", x + w / 2 + 2, y, w / 2 - 2, vh, ch_val(c), sample, vfs, color, ha=0, va=2))
            lfs = clamp(int(min((h - vh) * 0.5, (w - 4) / (len(CH[c][1]) * 0.6))), 7, 12)
            items.append(text(prefix + " nome", x, y + vh, w, h - vh, ch_name(c), CH[c][1], lfs, GREY, weight="Normal", va=0))
        else:
            lw = w * 0.42
            lfs = clamp(int(min(h * 0.42, lw / (max(len(CH[c][1]), 3) * 0.62))), 7, 13)
            if w >= 60:
                items.append(text(prefix + " nome", x + 4, y, lw, h, ch_name(c), CH[c][1], lfs, GREY, weight="Medium", ha=0))
            ad = min(h * 0.62, (w - lw) * 0.35)
            vx = x + (lw if w >= 60 else 0)
            vw = x + w - 4 - vx
            items.append(pos_arrow(prefix, vx + vw - ad - fit(vw - ad, h, 2, 0.72) * 1.3 - 4, y + (h - ad) / 2, ad))
            items.append(text(prefix + " valore", vx, y, vw, h, ch_val(c), sample, fit(vw - ad, h, 2, 0.72), color, ha=2))
        return items
    if h >= 40 and w >= 40:
        # numeri grandi, scritta sotto piccola e sottile
        vh = h * (0.745 if sub is None else 0.61)
        vfs = forced or (fit(w - 5, vh, chars, 0.97, 0.56) if sub is None else fit(w - 5, vh, chars, 0.87))
        if sub is None:
            # numero + scritta sotto, centrati insieme in altezza
            lfs = lfs or entry_label_size(c, w, h)
            gh = vfs * 0.72
            gap = max(3, h * 0.04)
            blk = gh + gap + lfs * 0.8
            top = y + max(1, (h - blk) / 2)
            vbox = vfs * 1.25
            items.append(text(prefix + " valore", x, top + gh + vfs * 0.21 - vbox, w, vbox, ch_val(c), sample, vfs, color, va=2))
            items.append(text(prefix + " nome", x, top + gh + gap - lfs * 0.15, w, lfs * 1.4, ch_name(c), CH[c][1], lfs, GREY, weight="Normal", va=0))
        else:
            # numero grande + valore sotto (es. pressione e temperatura), centrati insieme in altezza
            lh = h - vh
            sfs = clamp(int(min(lh * 0.6, (w - 4) / (CH[sub][6] * 0.6))), 8, 22)
            gh, sgh = vfs * 0.72, sfs * 0.72
            gap = max(4, h * 0.05)
            top = y + max(1, (h - (gh + gap + sgh)) / 2)
            vbox = vfs * 1.25
            items.append(text(prefix + " valore", x, top + gh + vfs * 0.21 - vbox, w, vbox, ch_val(c), sample, vfs, color, va=2))
            items.append(text(prefix + " sotto", x, top + gh + gap - sfs * 0.15, w, sfs * 1.4, ch_val(sub), CH[sub][7], sfs, ORANGE, weight="Medium", va=0))
    else:
        lw = w * 0.39
        lfs = lfs or clamp(int(min(h * 0.41, lw / (max(len(CH[c][1]), 3) * 0.61))), 7, 13)
        if w >= 84:
            items.append(text(prefix + " nome", x + 4, y, lw, h, ch_name(c), CH[c][1], lfs, GREY, weight="Medium", ha=0))
            vfs = forced or fit(w - lw - 5, h, chars, 0.78)
            items.append(text(prefix + " valore", x + lw, y, w - lw - 4, h, ch_val(c), sample, vfs, color, ha=2))
        else:
            vfs = forced or fit(w - 8, h, chars, 0.78)
            items.append(text(prefix + " valore", x, y, w, h, ch_val(c), sample, vfs, color))
    return items


def entry_label_size(c, w, h):
    lh = h * 0.255
    return clamp(int(min(lh * 0.55, (w - 4) / (len(CH[c][1]) * 0.6))), 7, 11)


def best_grid(k, w, h, chars):
    if h < 44 and k <= 5:
        return k, 1          # riquadro basso (righe centrali): tutti i valori in fila, si leggono meglio
    best = None
    for cols in range(1, k + 1):
        rows = math.ceil(k / cols)
        if (cols - 1) * rows >= k:  # colonne inutili
            continue
        cw, rh = w / cols, h / rows
        if rh >= 40 and cw >= 40:
            score = min(rh * 0.62 * 0.85, (cw - 6) / (chars * 0.6))
        else:
            score = min(rh * 0.72, (cw * 0.58 - 6) / (chars * 0.6)) * 0.9
        if cols * rows > k:  # celle vuote: preferisci disposizioni piene
            score *= 0.7
        if best is None or score > best[0]:
            best = (score, cols, rows)
    return best[1], best[2]


BLINK = "(Math.floor(Date.now() / 90) % 2 == 0)"   # lampeggio veloce (90 ms acceso / 90 ms spento)


def card_items(card, w, h):
    kind = card["kind"]
    if kind == "empty":
        return []
    color = card.get("color", FRAME)
    items = [frame_rect(w, h, color)]

    if kind == "gauge":
        g = gauge_items(card, w, h, items)
        return items + g

    if kind == "iconrow":
        return items + iconrow_items(card, w, h)

    if kind == "strategy":
        return items + strategy_items(w, h)

    if kind == "gaps":
        return items + gaps_items(card, w, h)

    if kind == "standings":
        return items + standings_items(card, w, h)

    if kind == "forecast":
        return items + forecast_items(card, w, h)

    if kind == "session":
        return items + session_items(w, h)

    if kind in ("single", "delta"):
        c = card["ch"]
        aid = {"TC": "TC", "ABS": "ABS"}.get(c)   # solo TC e ABS si accendono (TC Cut e TC Slip no)
        if aid:
            glow = "#FF0091EA" if aid == "TC" else "#FFFFAB00"
            items.append(layer("%s attivo" % aid, [rect("%s attivo fondo" % aid, 2, 2, w - 4, h - 4, bg=glow, radius=int(min(10, h / 4)))],
                               None, ("Attivo_" + aid, 1), vis_js="return $prop('DTPlugin.Attivo_%s') === true && %s;" % (aid, BLINK)))
        chars, sample = max(CH[c][6], len(CH[c][7])), CH[c][7]
        vb = {"TextColor": js(DELTA_COLOR_JS % c)} if kind == "delta" else None
        vcol = WHITE if kind != "delta" else GREEN
        if h >= 52:
            lfs = clamp(int(min(h * 0.22, (w - 24) / (len(CH[c][1]) * 0.62))), 9, 19)   # titolo sul bordo piu' grande
            lg = legend(items, w, h, CH[c][1], lfs, label_color(color), bind_text=js("return fdn('%s');" % c))
            lh = int(lg * 0.6)                    # il nome sta sul bordo: il valore usa quasi tutta l'altezza
            vfs = fit(w - 11, h - lh - 6, chars, 0.86)
            if kind == "delta":
                sb = clamp(round(h * 0.07), 4, 9)
                vfs = fit(w - 14, h - lh - sb - 14, chars, 0.8)
                items.append(text("Valore", 4, lh + 2, w - 8, h - lh - sb - 12, ch_val(c), sample, vfs, vcol, bindings=vb))
                items += sector_bars(8, h - sb - 7, w - 16, sb)
                return items
            items.append(text("Valore", 3, lh + 2, w - 6, h - lh - 5, ch_val(c), sample, vfs, vcol, bindings=vb))
        else:
            lw = w * 0.39
            lfs = clamp(int(min(h * 0.39, lw / (max(len(CH[c][1]), 3) * 0.61))), 7, 13)
            items.append(text("Nome", 8, 0, lw, h, ch_name(c), CH[c][1], lfs, label_color(color), weight="Medium", ha=0))
            vfs = fit(w - lw - 13, h, chars, 0.76)
            if kind == "delta":
                vfs = fit(w - lw - 16, h - 8, chars, 0.7)
                items.append(text("Valore", lw, 0, w - lw - 9, h - 6, ch_val(c), sample, vfs, vcol, ha=2, bindings=vb))
                items += sector_bars(9, h - 6, w - 18, 3, gap=3)
                return items
            items.append(text("Valore", lw, 0, w - lw - 6, h, ch_val(c), sample, vfs, vcol, ha=2, bindings=vb))
        if aid:
            for itx in items:
                if itx.get("Name") == "Nome":
                    itx.setdefault("Bindings", {})["TextColor"] = js("return ($prop('DTPlugin.Attivo_%s') === true && %s) ? '#FFFFFFFF' : '%s';"
                                                                     % (aid, BLINK, label_color(color)))
        return items

    title = card.get("title", "")
    show_title = h >= 70 and w >= 90 and bool(card.get("title"))
    th = 0
    if show_title:
        tfs = clamp(int(min(h * 0.08, (w - 24) / (len(title) * 0.62))), 9, 15)
        th = int(legend(items, w, h, title, tfs, label_color(color)) * 0.75)
    ix, iy = 4, (th + 2 if show_title else 2)
    iw, ih = w - 8, h - iy - 3

    if kind == "group":
        ents = card["entries"]
        chars = max(CH[c][6] for c, _ in ents)
        cols, rows = best_grid(len(ents), iw, ih, chars)
        if card.get("one_row") and iw / len(ents) >= 36:
            cols, rows = len(ents), 1          # tutto su una riga
        cw, rh = iw / cols, ih / rows
        one = card.get("one_row") and rows == 1
        if one:
            chars = max(len(CH[c][7]) for c, _ in ents)   # stessa grandezza, sul valore piu' lungo
        if one and card.get("pos_gain"):
            # colonne su misura (la posizione ha accanto le posizioni guadagnate) e numeri tutti della stessa grandezza
            wts = [2.1 if c == "Pos" else (0.7 if c == "Lap" else 1.0) for c, _ in ents]
            ws = [iw * t / sum(wts) for t in wts]
            vh = rh * 0.72 if rh >= 40 else rh
            hf = 0.95 if rh >= 40 else 0.72
            same = min(fit(wd - 6, vh, 10 if c == "Pos" else len(CH[c][7]), hf, 0.56) for (c, _), wd in zip(ents, ws))
            same = max(8, int(same * 0.88))                # un po' piu' piccoli
            xx = ix
            for i, ((c, col), wd) in enumerate(zip(ents, ws)):
                if c == "Pos":
                    items += pos_with_gain(col, xx, iy, wd, rh, "%s %d" % (c, i), vfs=same)
                else:
                    items += entry_items(c, col, xx, iy, wd, rh, "%s %d" % (c, i), vfs=same)
                xx += wd
            return items
        if card["name"] == "Tempi giro" and cols == 1 and rh >= 40 and cw >= 40:
            return items + lap_times_items(ents, ix, iy, iw, ih)
        # stessa grandezza per tutti i numeri e per tutte le scritte della scheda
        same_v = same_l = None
        if rh < 40 and cw < 84 and not one:
            # solo valori in fila: tutti della stessa grandezza
            same_v = min(fit(cw - 8, rh, max(CH[c][6], len(CH[c][7])), 0.78) for c, _ in ents)
        if rh < 40 and cw >= 84 and not one:
            # righe basse (nome a sinistra, valore a destra): stessa grandezza per tutti i valori
            same_v = min(fit(cw - cw * 0.39 - 5, rh, max(CH[c][6], len(CH[c][7])), 0.78) for c, _ in ents)
            same_l = min(clamp(int(min(rh * 0.41, cw * 0.39 / (max(len(CH[c][1]), 3) * 0.61))), 7, 13) for c, _ in ents)
        if rh >= 40 and cw >= 40:
            same_v = min(fit(cw - 5, rh * 0.745, max(CH[c][6], len(CH[c][7])) if not one else chars, 0.97, 0.56) for c, _ in ents)
            same_l = min(entry_label_size(c, cw, rh) for c, _ in ents)
        for i, (c, col) in enumerate(ents):
            r, k = divmod(i, cols)
            items += entry_items(c, col, ix + k * cw, iy + r * rh, cw, rh, "%s %d" % (c, i), chars=chars if one else None,
                                 vfs=same_v, lfs=same_l)
        # divisori leggeri tra le righe e tra le colonne
        for r in range(1, rows):
            # numeri e scritte sono centrati nella loro riga: il confine e' a meta' dello spazio libero
            items.append(hdiv("Divisore riga %d" % r, ix + iw * 0.06, iy + r * rh, iw * 0.88))
        for k in range(1, cols):
            m = min(rh * 0.15, 6)
            items.append(vdiv("Divisore colonna %d" % k, ix + k * cw, iy + m, ih - 2 * m))
        return items

    if kind == "damage":
        return items + damage_items(ix, iy, iw, ih)

    if kind == "status":
        return items + status_items(ix, iy, iw, ih)

    if kind == "tyres2":
        return items + tyres2_items(ix, iy, iw, ih)

    if kind == "tyres3":
        return items + tyres3_items(ix, iy, iw, ih)

    if kind == "tyres":
        main, sub = card["main"], card["sub"]
        if iw / max(ih, 1) > 2.8:
            cols, rows = 4, 1
        elif ih / max(iw, 1) > 2.8:
            cols, rows = 1, 4
        else:
            cols, rows = 2, 2
        cw, rh = iw / cols, ih / rows
        for i in range(4):
            r, k = divmod(i, cols)
            items += entry_items(main[i], WHITE, ix + k * cw, iy + r * rh, cw, rh, "Gomma %d" % i,
                                 sub=sub[i] if rh >= 40 else None)
        for i, (wk, prop, it) in enumerate(WHEELS):
            r, k = divmod(i, cols)
            if rh >= 40 and cw >= 60:
                # mescola nell'angolo in alto, verso l'esterno della macchina
                # in basso, all'esterno, accanto alla temperatura (in alto c'e' il numero grande)
                d = clamp(rh * 0.2, 12, 20)
                bx = ix + k * cw + 3 if wk in ("FL", "RL") else ix + (k + 1) * cw - d - 3
                items.append(compound_badge(wk, bx, iy + (r + 1) * rh - d - 4, d))
            items.append(tyre_state_overlay(wk, ix + k * cw + 2, iy + r * rh + 2, cw - 4, rh - 4))
        return items

    if kind == "bars":
        ents = card["entries"]
        n = len(ents)
        if ih >= 60:
            cw = iw / n
            bw = clamp(cw * 0.45, 6, 30)
            lab_h = 14 if ih >= 80 else 0
            y0, y1 = iy + 2, iy + ih - lab_h - 2
            H = y1 - y0
            for i, (c, col) in enumerate(ents):
                bx = ix + i * cw + (cw - bw) / 2
                items.append(rect("Barra %s fondo" % c, bx, y0, bw, H, bg=TRACK, radius=3))
                frac = float(CH[c][7].rstrip("%") or 0) / 100
                v = "Math.max(0, Math.min(100, $prop('DTPlugin.%s_Valore')))" % c
                items.append(rect("Barra %s" % c, bx, y0, bw, H, bg=col, radius=3, bindings={
                    "Height": js("return %s * %d / 100;" % (v, round(H))),
                    "Top": js("return %d - %s * %d / 100;" % (round(y1), v, round(H)))}, pv=dict(vfrac=frac)))
                if lab_h:
                    items.append(text("Nome %s" % c, ix + i * cw, y1 + 1, cw, lab_h, ch_name(c), CH[c][1],
                                      clamp(int(min(10, cw / (len(CH[c][1]) * 0.62))), 7, 10), GREY, weight="Medium"))
        else:
            rh = ih / n
            lw = iw * 0.32 if iw >= 120 else 0
            for i, (c, col) in enumerate(ents):
                ry = iy + i * rh
                if lw:
                    items.append(text("Nome %s" % c, ix + 2, ry, lw, rh, ch_name(c), CH[c][1],
                                      clamp(int(min(rh * 0.6, lw / (len(CH[c][1]) * 0.62))), 7, 11), GREY, weight="Medium", ha=0))
                bx, bh = ix + lw + 2, max(4, rh * 0.5)
                BW = iw - lw - 6
                by = ry + (rh - bh) / 2
                items.append(rect("Barra %s fondo" % c, bx, by, BW, bh, bg=TRACK, radius=2))
                frac = float(CH[c][7].rstrip("%") or 0) / 100
                v = "Math.max(0, Math.min(100, $prop('DTPlugin.%s_Valore')))" % c
                items.append(rect("Barra %s" % c, bx, by, BW, bh, bg=col, radius=2, bindings={
                    "Width": js("return %s * %d / 100;" % (v, round(BW)))}, pv=dict(hfrac=frac)))
        return items
    raise ValueError(kind)


TYRE_SAMPLE_COL = {"FL": GREEN, "FR": "#FF8BE04E", "RL": GREEN, "RR": YELLOW}


# zone danno: (zona, x, y, larghezza, altezza) in frazioni del rettangolo della macchina (muso in alto)
DAMAGE_RECTS = [(1, 0.28, 0.00, 0.44, 0.07), (2, 0.00, 0.00, 0.26, 0.17), (8, 0.74, 0.00, 0.26, 0.17),
                (3, 0.00, 0.30, 0.10, 0.40), (7, 0.90, 0.30, 0.10, 0.40),
                (4, 0.00, 0.79, 0.26, 0.13), (6, 0.74, 0.79, 0.26, 0.13), (5, 0.08, 0.93, 0.84, 0.07)]


# ruote attorno al profilo (frazioni del riquadro auto, larghezza:altezza = 1:2)
WHEEL_RECTS = [("FL", 0.00, 0.10), ("FR", 0.92, 0.10), ("RL", 0.00, 0.66), ("RR", 0.92, 0.66)]
CORNER_ZONE = {"FL": 2, "FR": 8, "RL": 4, "RR": 6}
CORNER_NAME = {"FL": "ANT. SX", "FR": "ANT. DX", "RL": "POST. SX", "RR": "POST. DX"}


DMG_OK = "#FF6E6E6E"
DMG_LV_JS = "var C = ['%s', '#FFFFD600', '#FFFF8C00', '#FFFF1744'];\r\n" % DMG_OK


def _zone_js(*zones):
    """Colore della parte di carrozzeria: il danno piu' grave tra le zone LMU indicate."""
    parts = " , ".join("($prop('DTPlugin.Danno_Z%d_Livello') || 0)" % z for z in zones)
    return DMG_LV_JS + "var l = Math.max(%s);\r\nl = Math.max(0, Math.min(3, Math.round(l)));\r\nreturn C[l];" % parts


def damage_items(bx, by, bw, bh):
    """Danni come l'icona di LMU: auto vista dall'alto (muso in alto) disegnata a linee, 4 gomme azzurre con accanto
    la striscia del freno (verde -> rosso con l'usura), frecce delle sospensioni, musetto e alettone colorati col danno.
    Se c'e' spazio, ai lati lo stato di ogni angolo (OK / LIEVE / ... / PERSA) e dell'aerodinamica."""
    # margine intorno e macchina un po' piu' piccola (le scritte non toccano i bordi)
    px_, py_ = bw * 0.06, bh * 0.07
    bx, by, bw, bh = bx + px_, by + py_, bw - 2 * px_, bh - 2 * py_
    ch = bh * 0.84
    cw = ch * 0.62
    if cw > bw * 0.9:
        cw = bw * 0.9
        ch = cw / 0.62
    lw = (bw - cw) / 2.0 - 10
    labels = lw >= 46 and ch >= 95
    x0, y0 = bx + (bw - cw) / 2.0, by + (bh - ch) / 2.0
    lw_ = max(2, round(ch * 0.018))                      # spessore delle linee
    tw, th = cw * 0.2, ch * 0.2                          # gomme
    sw_ = max(3, tw * 0.28)                              # striscia freno
    gap = max(2, tw * 0.12)
    fy, ry = y0 + ch * 0.17, y0 + ch * 0.58              # cima delle gomme davanti / dietro
    it = []
    # scocca: due fianchi (colorati con i danni laterali) chiusi davanti e dietro
    sx0, sx1 = x0 + tw * 0.55, x0 + cw - tw * 0.55 - lw_
    top, bot = y0 + ch * 0.13, y0 + ch * 0.85
    it.append(rect("Scocca sinistra", sx0, top, lw_, bot - top, bg=DMG_OK, radius=0, bindings={"BackgroundColor": js(_zone_js(3))}))
    it.append(rect("Scocca destra", sx1, top, lw_, bot - top, bg=DMG_OK, radius=0, bindings={"BackgroundColor": js(_zone_js(7))}))
    # davanti: parentesi (angoli anteriori) e ala (muso)
    it.append(rect("Davanti linea", sx0, top, sx1 + lw_ - sx0, lw_, bg=DMG_OK, radius=0, bindings={"BackgroundColor": js(_zone_js(2, 8))}))
    fw = cw * 0.62
    it.append(rect("Ala anteriore", x0 + (cw - fw) / 2, y0 + ch * 0.03, fw, ch * 0.055, bg=DMG_OK, radius=int(ch * 0.02),
                   bindings={"BackgroundColor": js(_zone_js(1))}))
    # dietro: parentesi (angoli posteriori) e alettone
    by0 = y0 + ch * 0.83
    bl = cw * 0.5
    bx0 = x0 + (cw - bl) / 2
    br = [rect("Dietro fondo", bx0, by0 + ch * 0.035, bl, lw_ + 1, bg=DMG_OK, radius=0),
          rect("Dietro sinistra", bx0, by0, lw_ + 1, ch * 0.035 + lw_ + 1, bg=DMG_OK, radius=0),
          rect("Dietro destra", bx0 + bl - lw_ - 1, by0, lw_ + 1, ch * 0.035 + lw_ + 1, bg=DMG_OK, radius=0)]
    for r_ in br:
        r_["Bindings"] = {"BackgroundColor": js(_zone_js(4, 6))}
        it.append(r_)
    aw = cw * 0.46
    it.append(rect("Alettone", x0 + (cw - aw) / 2, y0 + ch * 0.905, aw, ch * 0.07, bg="#FFE57373", radius=int(ch * 0.025),
                   bindings={"BackgroundColor": js(_zone_js(5))}))
    it[-1]["_pv"]["bg"] = DMG_OK
    # gomme + freni + sospensioni
    sfs = clamp(int(th * 0.55), 8, 40)
    for k in ("FL", "FR", "RL", "RR"):
        left = k[1] == "L"
        ty = fy if k[0] == "F" else ry
        tx = x0 if left else x0 + cw - tw
        tcol = js("var s = $prop('DTPlugin.Gomma_%s_Stato');\r\nreturn s == 'PERSA' ? '#FFFF1744' : (s == 'FORATA' ? '#FFFF8C00' : '#FF4FC3F7');" % k)
        it.append(rect("Gomma %s" % k, tx, ty, tw, th, bg="#FF4FC3F7", radius=int(min(tw, th) * 0.22), bindings={"BackgroundColor": tcol}))
        bxs = tx + tw + gap if left else tx - gap - sw_
        bcol = js("var v = $prop('DTPlugin.BrakeW%s_Valore');\r\nif (v == null || isNaN(v)) { return '#FF00E676'; }\r\n"
                  "return v >= 60 ? '#FF00E676' : (v >= 30 ? '#FFFFD600' : '#FFFF1744');" % k)
        it.append(rect("Freno %s" % k, bxs, ty + th * 0.18, sw_, th * 0.64, bg=GREEN, radius=int(sw_ * 0.3), bindings={"BackgroundColor": bcol}))
        scol = js("var v = $prop('DTPlugin.Susp%s_Valore');\r\nif (v == null || isNaN(v) || v <= 0) { return '%s'; }\r\n"
                  "return v < 25 ? '#FFFFD600' : (v < 60 ? '#FFFF8C00' : '#FFFF1744');" % (k, DMG_OK))
        cx_ = x0 + cw / 2
        chw = cw * 0.16
        chx = cx_ - chw - 1 if left else cx_ + 1
        it.append(text("Sospensione %s" % k, chx, ty, chw, th, None, "<" if left else ">", sfs, DMG_OK, weight="Black",
                       ha=2 if left else 0, bindings={"TextColor": scol}))
    if not labels:
        return it
    vfs = clamp(int(min(ch * 0.085, lw / 3.6)), 11, 44)
    sfs = clamp(int(vfs * 0.42), 7, 16)
    bh_ = vfs * 1.15 + sfs * 1.3

    def label(name, key, cx, cy, left, sample_v, sample_l):
        xx = cx - lw if left else cx
        ty = cy - bh_ / 2
        tb = {"Text": js("var t = $prop('DTPlugin.Danno_%s_Testo');\r\nreturn t ? t : 'OK';" % key),
              "TextColor": js("var c = $prop('DTPlugin.Danno_%s_Colore');\r\nreturn c ? c : '#FFFFFFFF';" % key)}
        return [text("Danno %s valore" % name, xx, ty, lw, vfs * 1.15, None, sample_v, vfs, WHITE, ha=2 if left else 0, va=2, bindings=tb),
                text("Danno %s nome" % name, xx, ty + vfs * 1.15, lw, sfs * 1.3, None, sample_l, sfs, GREY, weight="Medium",
                     ha=2 if left else 0, va=0)]

    for k in ("FL", "FR", "RL", "RR"):
        left = k[1] == "L"
        cy = (fy if k[0] == "F" else ry) + th / 2
        lx = (x0 - 8) if left else (x0 + cw + 8)
        it += label(k, k, lx - 4 if left else lx + 4, cy, left, "OK", CORNER_NAME[k])
    it += label("Aero", "Aero", x0 + cw + 12, y0 + 0.93 * ch, False, "OK", "AERO")
    return it


TYRE_SAMPLE_COMP = {"FL": ("M", "#FFFFD600"), "FR": ("M", "#FFFFD600"), "RL": ("S", "#FFFF1744"), "RR": ("S", "#FFFF1744")}


def compound_badge(k, x, y, d):
    """Mescola della gomma: cerchio colorato con la lettera (S rossa, M gialla, H bianca, W blu)."""
    d = max(10, int(round(d)))
    col = js("var c = $prop('DTPlugin.Gomma_%s_MescolaColore');\r\nreturn c ? c : '#FFFFFFFF';" % k)
    letter, sc = TYRE_SAMPLE_COMP[k]
    bw = 2 if d >= 14 else 1
    g = [rect("Mescola %s cerchio" % k, x, y, d, d, bg="#00000000", border=sc, bw=bw, radius=d // 2, border_bind=col),
         text("Mescola %s" % k, x, y, d, d, "[DTPlugin.Gomma_%s_Mescola]" % k, letter, max(7, int(d * 0.62)), sc, weight="Black",
              bindings={"TextColor": col})]
    lay = layer("Mescola %s" % k, g)
    lay["Bindings"] = {"Visible": js("var m = $prop('DTPlugin.Gomma_%s_Mescola');\r\nreturn m != null && m !== '';" % k)}
    return lay


def tyre_state_overlay(k, x, y, w, h):
    """Riquadro rosso "PERSA"/"FORATA" sopra la gomma quando la telemetria dice che e' andata."""
    fs = fit(w - 8, h * 0.5, 6, 0.8, cap=40)
    return layer("Gomma %s stato" % k, [
        rect("Gomma %s stato fondo" % k, x, y, w, h, bg="#E6400000", border=RED, bw=3, radius=int(min(8, h / 5))),
        text("Gomma %s stato testo" % k, x, y, w, h, "[DTPlugin.Gomma_%s_Stato]" % k, "PERSA", fs, WHITE, weight="Black"),
    ], None, ("Gomma_%s_Stato" % k, 1),
        vis_js="var s = $prop('DTPlugin.Gomma_%s_Stato');\r\nreturn s != null && s != '';" % k)


def tyres3_items(ix, iy, iw, ih):
    """Gomme "tutto in uno" (2x2 separati da una croce): temperatura grande, accanto al disegno del battistrada
    (3 strisce colorate) pressione e usura verso l'esterno, temperatura freno verso l'interno; mescola al centro."""
    if iw / max(ih, 1) > 2.8 or ih / max(iw, 1) > 2.8:
        return tyres2_items(ix, iy, iw, ih)
    items = []
    cw, rh = iw / 2.0, ih / 2.0
    cx, cy = ix + cw, iy + rh
    line = "#FF4A4A4A"
    items.append(rect("Croce verticale", cx - 1, iy + 2, 2, ih - 4, bg=line))
    items.append(rect("Croce orizzontale", ix + 2, cy - 1, iw - 4, 2, bg=line))
    ph = clamp(rh * 0.17, 12, 24)                      # pastiglia della mescola al centro
    gap = ph / 2 + 3
    pfs_all, sfs_all = [], []
    for i, (k, prop, it) in enumerate(WHEELS):
        r, c = divmod(i, 2)
        x0, y0 = ix + c * cw, iy + r * rh
        left = k in ("FL", "RL")
        front = r == 0
        # zona dettagli (striscia + numeri piccoli) vicino alla croce, pressione verso il bordo
        dh = rh * 0.50
        if front:
            dy = cy - gap - dh
            py, pH = y0 + 2, dy - y0 - 2
        else:
            dy = cy + gap
            py, pH = dy + dh, y0 + rh - (dy + dh) - 2
        pfs = fit(cw - 30, pH, 4, 0.8, 0.58)
        items.append(text("Gomma %s temp" % k, x0 + 4, py, cw - 8, pH, ch_val("TyreT" + k), CH["TyreT" + k][7], pfs, WHITE, weight="Black"))
        # battistrada: 3 strisce
        tw = clamp(cw * 0.21, 16, 40)               # gomma larga: 3 fasce con un piccolo spazio
        th = dh * 0.92
        tx = x0 + (cw - tw) / 2
        ty = dy + (dh - th) / 2
        gp = 3 if tw >= 24 else 2
        stw = (tw - 2 * gp) / 3.0
        for j in range(3):
            rr = 5 if j != 1 else 2
            items.append(rect("Gomma %s striscia %d" % (k, j + 1), tx + j * (stw + gp), ty, stw, th, bg=GREEN, radius=rr,
                              bindings={"BackgroundColor": js("var c = $prop('DTPlugin.Strisce_%s_%d');\r\nreturn c ? c : '#FF3A3A3A';" % (k, j + 1))}))
            items[-1]["_pv"]["bg"] = TYRE_SAMPLE_COL[k]
        sw = (cw - tw) / 2 - 8                         # spazio ai lati della striscia
        sfs = int(fit(sw - 2, dh * 0.5, 5, 0.9, 0.62) * 0.95)
        ox = x0 + 4 if left else tx + tw + 4           # lato esterno della macchina
        nx = tx + tw + 4 if left else x0 + 4           # lato interno
        oha, nha = (2, 0) if left else (0, 2)          # tutto allineato verso la striscia
        items.append(text("Gomma %s pressione" % k, ox, dy, sw, dh / 2, ch_val("TyreP" + k), CH["TyreP" + k][7], sfs, WHITE, weight="Bold", ha=oha, va=2))
        wfs = min(int(sfs * 1.35), int(fit(sw - 2, dh * 0.52, 3.6, 1.0, 0.58)))   # percentuale piu' grande
        items.append(text("Gomma %s usura" % k, ox, dy + dh / 2, sw, dh / 2, ch_val("TyreW" + k), CH["TyreW" + k][7], wfs, WHITE, weight="Bold", ha=oha, va=0))
        # temperatura freno: icona del freno sopra, numero sotto (come pressione/usura dall'altra parte)
        bfs = min(int(sfs * 1.35), int(fit(sw - 2, dh * 0.55, 4, 1.0, 0.58)))   # temperatura freno piu' grande
        isz = min(dh * 0.4, sfs * 1.15)
        ix_ = nx + 2 if left else nx + sw - isz - 2
        items.append(image("Gomma %s icona freno" % k, ix_, dy + dh / 2 - isz - 2, isz, isz, "dt-ico-freno"))
        items.append(text("Gomma %s freno" % k, nx, dy + dh / 2 - 2, sw, dh / 2 + 4, ch_val("BrakeT" + k), CH["BrakeT" + k][7], bfs, WHITE, weight="Bold", ha=nha, va=0))
    # mescola al centro della croce
    pw = clamp(iw * 0.36, 50, 150)
    pfs = clamp(int(ph * 0.66), 8, 16)
    pill = [rect("Mescola nome fondo", cx - pw / 2, cy - ph / 2, pw, ph, bg="#FFFF1744", radius=int(ph // 2),
                 bindings={"BackgroundColor": js("var c = $prop('DTPlugin.Gomme_MescolaSfondo');\r\nreturn c ? c : '#FF5A5A5A';")}),
            text("Mescola nome", cx - pw / 2, cy - ph / 2, pw, ph, "[DTPlugin.Gomme_MescolaNome]", "SOFT", pfs, WHITE, weight="Black",
                 bindings={"TextColor": js("var c = $prop('DTPlugin.Gomme_MescolaTesto');\r\nreturn c ? c : '#FFFFFFFF';")})]
    lay = layer("Mescola nome", pill)
    lay["Bindings"] = {"Visible": js("var m = $prop('DTPlugin.Gomme_MescolaNome');\r\nreturn m != null && m !== '';")}
    items.append(lay)
    for i, (k, prop, it) in enumerate(WHEELS):
        r, c = divmod(i, 2)
        items.append(tyre_state_overlay(k, ix + c * cw + 2, iy + r * rh + 2, cw - 4, rh - 4))
    return items


def tyres2_items(ix, iy, iw, ih):
    """Gomme complete: per ruota temp. carcassa grande + pressione + temp. freno + 3 strisce di superficie colorate."""
    items = []
    if iw / max(ih, 1) > 2.8:
        cols, rows = 4, 1
    elif ih / max(iw, 1) > 2.8:
        cols, rows = 1, 4
    else:
        cols, rows = 2, 2
    gw, gh = iw / cols, ih / rows
    for i, (k, prop, it) in enumerate(WHEELS):
        r, c = divmod(i, cols)
        x, y, w, h = ix + c * gw + 2, iy + r * gh + 2, gw - 4, gh - 4
        left_wheel = k in ("FL", "RL")
        items.append(rect("Gomma %s riquadro" % k, x, y, w, h, bg=CARD_BG, border="#FF5A5A5A", bw=1, radius=int(min(8, h / 5))))
        small = h < 70 or w < 90
        lh = 0 if small else clamp(round(h * 0.15), 10, 18)
        if lh:
            items.append(text("Gomma %s nome" % k, x, y + 2, w, lh, None, it, int(lh * 0.8), GREY, weight="Medium"))
            # mescola accanto al nome (AS, AD...)
            items.append(compound_badge(k, x + w / 2 + lh * 0.75, y + 2 + lh * 0.05, lh * 0.95))
        # strisce superficie (lato interno della macchina)
        sw = clamp(w * 0.26, 12, 44)
        sh = h - lh - 10
        sx = x + w - sw - 6 if left_wheel else x + 6
        sy = y + lh + 5
        stw = (sw - 4) / 3.0
        for j in range(3):
            items.append(rect("Gomma %s striscia %d" % (k, j + 1), sx + j * (stw + 2), sy, stw, sh, bg=GREEN, radius=2,
                              bindings={"BackgroundColor": js("var c = $prop('DTPlugin.Strisce_%s_%d');\r\nreturn c ? c : '#FF3A3A3A';" % (k, j + 1))}))
            items[-1]["_pv"]["bg"] = TYRE_SAMPLE_COL[k]
        # percentuale di gomma rimasta, dentro la gomma (solo nel riquadro completo)
        if not small and sw >= 22:
            # percentuale di gomma rimasta: grande, su fondo nero pieno
            ph = clamp(sh * 0.3, 16, 34)
            py = sy + (sh - ph) / 2
            items.append(rect("Gomma %s usura fondo" % k, sx - 4, py, sw + 8, ph, bg="#F0000000", radius=4))
            items.append(text("Gomma %s usura" % k, sx - 4, py, sw + 8, ph, ch_val("TyreW" + k), CH["TyreW" + k][7],
                              fit(sw + 6, ph, 3.4, 0.92, 0.56), WHITE, weight="Black", pv_color=GREEN))
        tx = x + 4 if left_wheel else sx + sw + 4
        tw = w - sw - 14
        ah = h - lh - 6
        ay = y + lh + 3
        if small:
            items.append(text("Gomma %s temp" % k, tx, ay, tw, ah, ch_val("TyreT" + k), CH["TyreT" + k][7],
                              fit(tw, ah, 4, 0.8), WHITE, pv_color=TYRE_SAMPLE_COL[k]))
            continue
        th = ah * 0.48
        items.append(text("Gomma %s temp" % k, tx, ay, tw, th, ch_val("TyreT" + k), CH["TyreT" + k][7],
                          fit(tw - 4, th, 4, 0.85), WHITE, ha=2 if left_wheel else 0, pv_color=TYRE_SAMPLE_COL[k]))
        # pressione (P) e freno (F): piu' grandi, allineati verso la gomma
        rh2 = (ah - th) / 2.0
        for j, (cid, tag) in enumerate((("TyreP" + k, "P"), ("BrakeT" + k, "F"))):
            ly = ay + th + j * rh2
            tagw = clamp(tw * 0.16, 9, 16)
            vfs2 = fit(tw - tagw - 2, rh2, 5, 0.88, 0.56)
            items.append(text("Gomma %s %s etichetta" % (k, tag), tx, ly, tagw, rh2, None, tag,
                              clamp(int(vfs2 * 0.6), 7, 14), GREY, weight="Medium", ha=0))
            items.append(text("Gomma %s %s" % (k, tag), tx + tagw, ly, tw - tagw, rh2, ch_val(cid), CH[cid][7],
                              vfs2, WHITE, ha=2, pv_color=GREEN if tag == "P" else YELLOW))
    for i, (k, prop, it) in enumerate(WHEELS):
        r, c = divmod(i, cols)
        items.append(tyre_state_overlay(k, ix + c * gw + 2, iy + r * gh + 2, gw - 4, gh - 4))
    return items


def bar_items(entry, w, h):
    name, c, col = entry
    if c is None:
        return []
    items = [rect("Fondo", 0, 0, w, h, bg="#FF161616", radius=6)]
    v = "Math.max(0, Math.min(100, $prop('DTPlugin.%s_Valore')))" % c
    b = {"Height": js("return %s * %d / 100;" % (v, h)), "Top": js("return %d - %s * %d / 100;" % (h, v, h))}
    if c == "RpmPct":
        b["BackgroundColor"] = js("var p = $prop('DTPlugin.RpmPct_Valore');\r\n"
                                  "return p >= 97 ? '#FFFF1744' : (p >= 85 ? '#FFFFD600' : '#FF00E676');")
    frac = float(CH[c][7].rstrip("%") or 0) / 100
    items.append(rect("Livello", 0, 0, w, h, bg=col, radius=6, bindings=b, pv=dict(vfrac=frac)))
    return items


# ------------------------------------------------------------------ file schede
CARD_FILES = {}   # filename -> doc


def card_file(w, h):
    fn = "Schede_%dx%d.djson" % (w, h)
    if fn not in CARD_FILES:
        scr = [screen(c["name"], card_items(c, w, h)) for c in CARDS]
        CARD_FILES[fn] = dash_doc(w, h, scr, metadata(w, h, scr))
    return fn


def bar_file(w, h):
    fn = "Barre_%dx%d.djson" % (w, h)
    if fn not in CARD_FILES:
        scr = [screen(b[0], bar_items(b, w, h)) for b in BARS]
        CARD_FILES[fn] = dash_doc(w, h, scr, metadata(w, h, scr))
    return fn


# ------------------------------------------------------------------ schermata principale
LED_N = 16


def led_strip(x, y, w, h):
    """Fila di LED dei giri: verdi, gialli, rossi da meta' giri in su; al limitatore tutti blu lampeggianti."""
    it = []
    d = min(h, (w - (LED_N - 1) * 8) / LED_N)
    gap = (w - LED_N * d) / (LED_N - 1)
    for i in range(LED_N):
        col = "#FF00E676" if i < 6 else ("#FFFFD600" if i < 11 else "#FFFF1744")
        thr = 60 + i * (95 - 60) / (LED_N - 1)   # % giri a cui si accende
        cj = ("var p = $prop('DTPlugin.RpmPct_Valore');\r\nif (p == null) { return '#FF1E1E1E'; }\r\n"
              "if (p >= 97) { return %s ? '#FF2979FF' : '#FF1E1E1E'; }\r\n"
              "return p >= %.1f ? '%s' : '#FF1E1E1E';" % (BLINK, thr, col))
        it.append(rect("LED %d" % (i + 1), x + i * (d + gap), y + (h - d) / 2, d, d, bg=col if i < 10 else "#FF1E1E1E",
                       radius=int(d // 2), bindings={"BackgroundColor": js(cj)}))
    return it


def center_items(fi, ci=0):
    BLINK_ = BLINK
    cg = center_geom(fi, ci)
    it = []
    if frame_led(fi):
        it.append(layer("LED giri", led_strip(M + 4, M, DASH_W - 2 * M - 8, LED_H)))
    x, y, w, h = cg["delta"]
    # riquadro in alto al centro: il delta (predefinito) oppure una scheda scelta nel plugin (DTPlugin.Slot_D)
    dl = [rect("Delta fondo", x, y, w, h, bg="#FF161616", radius=8)]
    sbh = 5
    dl.append(text("Delta", x, y + 1, w, h - sbh - 5, ch_val("Delta"), CH["Delta"][7], fit(w - 16, h - sbh - 6, 6, 0.92, 0.6), GREEN,
                   bindings={"TextColor": js(DELTA_COLOR_JS % "Delta")}))
    dl += sector_bars(x + 6, y + h - sbh - 4, w - 12, sbh, gap=3)
    it.append(layer("Delta predefinito", dl, None, ("Slot_D", 0), vis_js="var v = $prop('DTPlugin.Slot_D');\r\nreturn v == null || v == 0;"))
    it.append(layer("Delta scelto", [widget("Riquadro delta", x, y, w, h, card_file(w, h), "Slot_D")], None, None,
                    vis_js="var v = $prop('DTPlugin.Slot_D');\r\nreturn v != null && v != 0;"))
    x, y, w, h = cg["gear"]
    it.append(rect("Marcia fondo", x, y, w, h, bg=CARD_BG, border=FRAME, bw=CENTER_BW, radius=12, bindings={
        "BackgroundColor": js("if ($prop('DTPlugin.PitLimiter_Valore') > 0) { return '#FF0D47A1'; }\r\n"
                              "if ($prop('DTPlugin.FuoriGiri') === true && %s) { return '#FFFF1744'; }\r\nreturn '#FF0B0B0B';" % BLINK)}))
    sx, sy, sw, sh = cg["speed"]
    gfs = int(min((sy - y) * 1.12, 200))       # marcia grande: usa quasi tutta l'altezza sopra la velocita'
    it.append(text("Marcia", x, y - 16, w, sy - y + 30, ch_val("Gear"), CH["Gear"][7], gfs, WHITE, weight="Black"))
    it.append(rect("Divisore marcia velocità", x + w * 0.14, round(sy) - 1, w * 0.72, 1, bg=DIV_COL, radius=0))   # linea sottile
    sh2 = sh + 12                              # velocita' piu' grande
    it.append(text("Velocità", sx, sy, sw, sh2, ch_val("Speed"), CH["Speed"][7], int(sh2 * 0.88), WHITE, weight="Bold"))
    rty = sy + sh2
    rth = max(14, (y + h - 6) - rty)
    # giri motore piu' grandi: in basso non ci sono piu' le barre
    it.append(text("Giri", x, rty, w, rth, ch_val("Rpm"), CH["Rpm"][7], clamp(int(rth * 0.62), 12, 28), GREY, weight="Medium", va=0))
    # barre verticali ai lati della marcia: giri (tutte e due) oppure acceleratore (sinistra) e freno (destra)
    BWD = 5
    bt, bb = y + 12, y + h - 12
    BH = bb - bt
    lx, rx = x + 8, x + w - 8 - BWD

    def vbar(name, bx, val, col, frac, col_js=None):
        b = {"Height": js("return %s * %d / 100;" % (val, BH)), "Top": js("return %d - %s * %d / 100;" % (bb, val, BH))}
        if col_js:
            b["BackgroundColor"] = js(col_js)
        return [rect(name + " fondo", bx, bt, BWD, BH, bg=TRACK, radius=2),
                rect(name, bx, bt, BWD, BH, bg=col, radius=2, bindings=b, pv=dict(vfrac=frac))]
    v = "Math.max(0, Math.min(100, $prop('DTPlugin.RpmPct_Valore')))"
    rcol = "var p = $prop('DTPlugin.RpmPct_Valore');\r\nreturn p >= 97 ? '#FFFF1744' : (p >= 85 ? '#FFFFD600' : '#FF00E676');"
    rpm_bar = vbar("Giri barra sinistra", lx, v, GREEN, 0.78, rcol) + vbar("Giri barra destra", rx, v, GREEN, 0.78, rcol)
    it.append(layer("Barra giri", rpm_bar, None, ("BarrePedali", 0), vis_js="return $prop('DTPlugin.BarrePedali') !== true && $prop('DTPlugin.BarreMarcia') !== false;"))
    # in alternativa (opzione della pagina): acceleratore (verde) a sinistra e freno (rosso) a destra
    ped = []
    for (c, col, bx, fr) in (("Throttle", GREEN, lx, 0.85), ("Brake", RED, rx, 0.15)):
        vv = "Math.max(0, Math.min(100, $prop('DTPlugin.%s_Valore')))" % c
        ped += vbar("%s barra" % c, bx, vv, col, fr)
    it.append(layer("Barre acceleratore e freno", ped, None, ("BarrePedali", 1),
                    vis_js="return $prop('DTPlugin.BarrePedali') === true && $prop('DTPlugin.BarreMarcia') !== false;"))
    # motore spento: riquadro giallo con "MOTORE SPENTO" e la marcia (stile 0)
    x, y, w, h = cg["gear"]
    eo_fs = fit(w - 16, h * 0.24, 7, 0.95)
    eo = [rect("Motore spento centro fondo", x, y, w, h, bg="#FFFFDA4A", radius=12),
          text("Motore spento centro testo", x + 4, y + h * 0.06, w - 8, h * 0.58, None, "MOTORE\nSPENTO", eo_fs, "#FF000000", weight="Black",
               bindings={"Text": js("var t = $prop('DTPlugin.MotoreSpento_Testo');\r\nif (!t) { t = 'MOTORE SPENTO'; }\r\nreturn t.replace(' ', '\\n');")}),
          text("Motore spento centro marcia", x, y + h * 0.64, w, h * 0.3, ch_val("Gear"), "N", int(min(h * 0.24, 60)), "#FF000000", weight="Black")]
    it.append(layer("Motore spento (centro)", eo, None, ("MotoreSpentoCentro", 1),
                    vis_js="return $prop('DTPlugin.MotoreSpento') === true && $prop('DTPlugin.MotoreSpento_Stile') != 1;"))
    # bandiera: copre velocita' e giri (dalla linea al fondo del riquadro della marcia)
    gx_, gy_, gw_, gh_ = cg["gear"]
    sy_ = cg["speed"][1]
    # dentro le barre laterali (barre a 8 px dal bordo, larghe 5) e fino al fondo delle barre
    it.append(flag_box(gx_ + 17, sy_ - 8, gw_ - 34, gy_ + gh_ - 12 - (sy_ - 8), square=True))   # copre anche la linea   # bandiere: sopra la velocita', sempre nello stesso posto
    for i, (x, y, w, h) in enumerate(cg["rows"]):
        it.append(widget("Riga centrale %d" % (i + 1), x, y, w, h, card_file(w, h), "Slot_C%d" % (i + 1)))
    return it


def flag_box(x, y, w, h, square=False):
    """Bandiera esposta (DTPlugin.Bandiera_Riquadro) mostrata sopra velocita' e giri."""
    fg = js("var c = $prop('DTPlugin.Bandiera_TestoColore');\r\nreturn c ? c : '#FF000000';")
    bgb = {"BackgroundColor": js("var c = $prop('DTPlugin.Bandiera_Sfondo');\r\nreturn c ? c : '#FFFFD600';")}
    if square:
        kids = [rect("Bandiera fondo", x, y, w, h, bg="#FFFFD600", radius=0, bindings=bgb)]
    else:
        kids = [rect("Bandiera fondo", x, y, w, h, bg="#FFFFD600", border="#FFFFFFFF", bw=2, radius=int(min(10, h / 3)), bindings=bgb)]
    sz = min(h - 8, 44)
    tx = x + 6
    if square:
        # icona sopra, nome sotto, tutto centrato
        # icona, nome e (con la gialla) il settore, tutto centrato
        isz = min(h * 0.32, 44)
        kids.append(image("Bandiera icona", x + (w - isz) / 2, y + h * 0.07, isz, isz, "dt-bandiera-nera",
                          "var c = $prop('DTPlugin.Bandiera_TestoColore');\r\nreturn (c == '#FFFFFFFF') ? 'dt-bandiera-bianca' : 'dt-bandiera-nera';"))
        ty = y + h * 0.07 + isz + 2
        nh = h * 0.30
        kids.append(text("Bandiera nome", x + 4, ty, w - 8, nh, "[DTPlugin.Bandiera_Testo]", "GIALLA",
                         fit(w - 12, nh, 8, 0.85), "#FF000000", weight="Black", bindings={"TextColor": fg}))
        sy2 = ty + nh
        kids.append(text("Bandiera settore", x + 4, sy2, w - 8, y + h - sy2 - 3, None, "SETTORE 2",
                         fit(w - 12, y + h - sy2 - 3, 10, 0.75), "#FF000000", weight="Bold",
                         bindings={"Text": js("var t = $prop('DTPlugin.Bandiera_Settore');\r\nreturn t ? t : '';"), "TextColor": fg}))
        return layer("Bandiera", kids, None, ("Bandiera_Riquadro", 1), vis_js="return $prop('DTPlugin.Bandiera_Riquadro') === true;")
    if sz >= 18:
        kids.append(image("Bandiera icona", x + 6, y + 4, sz, sz, "dt-bandiera-nera",
                          "var c = $prop('DTPlugin.Bandiera_TestoColore');\r\nreturn (c == '#FFFFFFFF') ? 'dt-bandiera-bianca' : 'dt-bandiera-nera';"))
        tx = x + 10 + sz
    tw = x + w - 6 - tx
    kids.append(text("Bandiera nome", tx, y, tw, h, "[DTPlugin.Bandiera_Testo]", "GIALLA", fit(tw, h, 9, 0.7), "#FF000000",
                     weight="Black", bindings={"TextColor": fg}))
    return layer("Bandiera", kids, None, ("Bandiera_Riquadro", 1), vis_js="return $prop('DTPlugin.Bandiera_Riquadro') === true;")


def frame_layer(fi):
    g = frame_geom(fi)
    kids = [layer("Centro", center_items(fi))]
    for side, key in (("left", "L"), ("right", "R")):
        x0, x1 = g[side]
        gl = []
        for gi, (gname, _) in enumerate(GRIDS):
            cells = grid_cells(x0, x1, g["top"], g["bottom"], gi, g["row_h"])
            ws = [widget("Slot %s%d" % (key, i + 1), x, y, w, h, card_file(w, h), "Slot_%s%d" % (key, i + 1))
                  for i, (x, y, w, h) in enumerate(cells)]
            gl.append(layer("Griglia %s" % gname, ws, None, ("Griglia_" + key, gi), vis_js=vis_eq("Griglia_" + key, gi)))
        kids.append(layer("Lato sinistro" if key == "L" else "Lato destro", gl))
    if g["toolbar"]:
        tl = []
        for ti, (tname, n) in enumerate(TOOLBARS):
            ws = [widget("Slot T%d" % (i + 1), x, y, w, h, card_file(w, h), "Slot_T%d" % (i + 1))
                  for i, (x, y, w, h) in enumerate(toolbar_cells(fi, ti))]
            tl.append(layer("Barra %s" % tname, ws, None, ("BarraInferiore", ti), vis_js=vis_eq("BarraInferiore", ti)))
        kids.append(layer("Barra inferiore", tl))
    if g["sidebars"]:
        ws = [widget("Barra %s" % k, x, y, w, h, bar_file(w, h), "Slot_" + k)
              for k, (x, y, w, h) in zip(("SL", "SR"), sidebar_cells(fi))]
        kids.append(layer("Barre laterali", ws))
    return layer("Cornice %d - %s" % (fi, FRAMES[fi][0]), kids, None, ("Cornice", fi), vis_js=vis_eq("Cornice", fi))


# ------------------------------------------------------------------ pagine (dashboard leggera)
# La dash contiene solo i layout delle pagine configurate (non tutte le combinazioni cornice x griglia x barra):
# il DT Plugin la ricompone dai "pezzi" quando cambi cornice, griglia o barra di una pagina.
def piece_grid(fi, key, gi):
    g = frame_geom(fi)
    x0, x1 = g["left" if key == "L" else "right"]
    return [widget("Slot %s%d" % (key, i + 1), x, y, w, h, card_file(w, h), "Slot_%s%d" % (key, i + 1))
            for i, (x, y, w, h) in enumerate(grid_cells(x0, x1, g["top"], g["bottom"], gi, g["row_h"]))]


def piece_toolbar(fi, ti):
    if not frame_geom(fi)["toolbar"]:
        return []
    return [widget("Slot T%d" % (i + 1), x, y, w, h, card_file(w, h), "Slot_T%d" % (i + 1))
            for i, (x, y, w, h) in enumerate(toolbar_cells(fi, ti))]


def piece_sidebars(fi):
    if not frame_geom(fi)["sidebars"]:
        return []
    return [widget("Barra %s" % k, x, y, w, h, bar_file(w, h), "Slot_" + k)
            for k, (x, y, w, h) in zip(("SL", "SR"), sidebar_cells(fi))]


def page_vis(idx):
    return "var p = $prop('DTPlugin.Pagina');\r\nif (p == null) { p = 1; }\r\nreturn p == %s;" % idx


DOUBLE_ON = "return $prop('DTPlugin.BarraDoppia') === true;"
DOUBLE_OFF = "return $prop('DTPlugin.BarraDoppia') !== true;"


def layout_items(fi, page):
    return center_items(fi, page.get("center", 0)) + piece_grid(fi, "L", page["grid_l"]) + piece_grid(fi, "R", page["grid_r"]) \
        + piece_toolbar(fi, page["toolbar"]) + piece_sidebars(fi)


def page_layer(pi, page):
    fi = page["frame"]
    if fi in SINGLE_OF:
        # barra doppia solo quando serve rifornire, altrimenti barra singola e schede piu' grandi
        kids = [layer("Barra doppia", layout_items(fi, page), None, ("BarraDoppia", 1), vis_js=DOUBLE_ON),
                layer("Barra singola", layout_items(SINGLE_OF[fi], page), None, ("BarraDoppia", 0), vis_js=DOUBLE_OFF)]
    else:
        kids = layout_items(fi, page)
    return layer("Pagina %d" % (pi + 1), kids, None, ("Pagina", pi + 1), vis_js=page_vis(pi + 1))


def plugin_warning(big=False):
    """Avviso visibile solo se le proprieta' del DT Plugin non esistono (la dash usa i valori di SimHub)."""
    W, H = DASH_W, DASH_H
    if big:
        kids = [text("Avviso plugin", 0, 300, W, 30, None, "DT Plugin non attivo: valori letti direttamente da SimHub", 15, RED, weight="Medium")]
    else:
        kids = [text("Avviso plugin", CX + 3, M + 2, 34, 30, None, "NO\nPLUGIN", 8, RED, weight="Bold", ha=0)]
    lay = layer("Avviso plugin mancante", kids)
    lay["Bindings"] = {"Visible": js("return $prop('DTPlugin.Pagina') == null;")}
    lay["_pv"]["vis"] = ("PluginMancante", 1)
    return lay


def lock_layer():
    """Dashboard bloccata: copre tutto finche' il DT Plugin non ha un codice di accesso valido (o non c'e')."""
    W, H = DASH_W, DASH_H
    kids = [rect("Bloccata fondo", 0, 0, W, H, bg="#FF000000", radius=0),
            rect("Bloccata riquadro", W / 2 - 300, H / 2 - 90, 600, 180, bg="#FF141414", border=RED, bw=3, radius=16),
            text("Bloccata titolo", W / 2 - 300, H / 2 - 75, 600, 60, None, "DT DASHBOARD BLOCCATA", 34, WHITE, weight="Black"),
            text("Bloccata testo", W / 2 - 290, H / 2 - 5, 580, 80, None,
                 "Serve il DT Plugin con un codice di accesso valido.\nInseriscilo in SimHub > DT Plugin.", 17, "#FFBDBDBD", weight="Medium")]
    lay = layer("Bloccata", kids)
    lay["Bindings"] = {"Visible": js("return $prop('DTPlugin.Sbloccato') !== true;")}
    lay["_pv"]["vis"] = ("Bloccata", 1)
    return lay


def engine_off_layer():
    """Schermo nero con "MOTORE SPENTO" quando in gioco il motore e' fermo (DTPlugin.MotoreSpento)."""
    W, H = DASH_W, DASH_H
    kids = [rect("Motore spento fondo", 0, 0, W, H, bg="#FF000000", radius=0),
            text("Motore spento testo", 0, H / 2 - 50, W, 100, None, "MOTORE SPENTO", 72, "#FF2E2E2E", weight="Bold",
                 bindings={"Text": js("var t = $prop('DTPlugin.MotoreSpento_Testo');\r\nreturn t ? t : 'MOTORE SPENTO';"),
                           "TextColor": js("var c = $prop('DTPlugin.MotoreSpento_Colore');\r\nreturn c ? c : '#FF2E2E2E';")})]
    return layer("Motore spento", kids, None, ("MotoreSpentoNero", 1), vis_js="return $prop('DTPlugin.MotoreSpento') === true && $prop('DTPlugin.MotoreSpento_Stile') == 1;")


def pit_box_items():
    """Schermata box completa (pieno schermo): auto davanti/dietro, tempo in corsia, velocita', marcia, giri, posizione."""
    W, H = DASH_W, DASH_H
    tcol = js("var c = $prop('DTPlugin.Avviso_Testo');\r\nreturn c ? c : '#FFFFFFFF';")
    bgcol = js("var c = $prop('DTPlugin.Avviso_Sfondo');\r\nreturn c ? c : '#FF0000FF';")
    tb = {"TextColor": tcol}
    it = [rect("Box sfondo", 0, 0, W, H, bg="#FF0000FF", radius=0, bindings={"BackgroundColor": bgcol}),
          text("Box titolo", 0, 16, W, 64, "[DTPlugin.Avviso_Titolo]", "PIT LIMITER", 30, WHITE, weight="Medium", bindings=dict(tb))]

    def car(side, badge_y, name_y, gap_y, sample):
        nm, num, cls = sample
        P_ = "DTPlugin.Box_%s_" % side
        vis = {"Visible": js("var n = $prop('%sNome');\r\nreturn n != null && n !== '';" % P_)}
        g = [rect("Box %s numero fondo" % side, 16, badge_y, 30, 22, bg="#FFC62828", radius=4),
             text("Box %s numero" % side, 16, badge_y, 30, 22, "[%sNumero]" % P_, num, 15, WHITE, weight="Black"),
             rect("Box %s classe fondo" % side, 49, badge_y, 46, 22, bg="#FFC62828", radius=4,
                  bindings={"BackgroundColor": js("var c = $prop('%sClasseColore');\r\nreturn c ? c : '#FFC62828';" % P_)}),
             text("Box %s classe" % side, 49, badge_y, 46, 22, "[%sClasse]" % P_, cls, 14, WHITE, weight="Black"),
             text("Box %s nome" % side, 16, name_y, 300, 26, "[%sNome]" % P_, nm, 18, WHITE, ha=0, bindings=dict(tb))]
        lay = layer("Box %s" % side, g)
        lay["Bindings"] = vis
        return [lay, text("Box %s distacco" % side, 16, gap_y, 320, 66, "[%sDistacco]" % P_,
                          "-30.3" if side == "Avanti" else "+0.89", 54, WHITE, ha=0, bindings=dict(tb))]

    it += car("Avanti", 104, 133, 158, ("S. Gattuso", "14", "GT3"))
    it += car("Dietro", 330, 300, 233, ("J. Adam", "15", "GT3"))
    # centro
    it.append(text("Box tempo corsia", W / 2 - 120, 130, 240, 30, "[DTPlugin.Box_TempoCorsia]", "0:15.328", 20, WHITE, bindings=dict(tb)))
    it.append(text("Box velocita", W / 2 - 160, 162, 320, 126, "[DTPlugin.Speed]", "0", 112, WHITE, bindings=dict(tb)))
    it.append(rect("Box marcia fondo", W / 2 - 27, 296, 54, 54, bg=WHITE, radius=6, bindings={"BackgroundColor": tcol}))
    it.append(text("Box marcia", W / 2 - 27, 296, 54, 54, "[DTPlugin.Gear]", "1", 36, "#FF0000FF", weight="Black",
                   bindings={"TextColor": bgcol}))
    it.append(text("Box giri", W / 2 - 120, 368, 240, 46, "[DTPlugin.Rpm]", "1991", 32, WHITE, bindings=dict(tb)))
    # posizione a destra
    it.append(text("Box posizione", W - 230, 228, 165, 64, None, "8", 50, WHITE, ha=2, va=2,
                   bindings={"Text": js("return fd('Pos');"), "TextColor": tcol}))
    it.append(text("Box totale", W - 64, 246, 60, 40, None, "/25", 22, WHITE, ha=0, va=2,
                   bindings={"Text": js("return '/' + fd('Cars');"), "TextColor": tcol}))
    return it


def _slide(items, sign):
    """Avviso di lato: ogni elemento entra dal bordo (DTPlugin.Avviso_Scorrimento px, che scende a 0)."""
    for it in items:
        if it.get("Childrens") is not None and it.get("$type") == T_LAYER:
            _slide(it["Childrens"], sign)
            continue
        b = it.setdefault("Bindings", {})
        if "Left" in b:
            continue
        b["Left"] = js("return %s %s ($prop('DTPlugin.Avviso_Scorrimento') || 0);" % (round(it["Left"], 1), "-" if sign < 0 else "+"))


def alert_layer():
    """Avviso configurabile dal plugin (box/limitatore, cambio valori, fine giro)."""
    tcol = js("var c = $prop('DTPlugin.Avviso_Testo');\r\nreturn c ? c : '#FFFFFFFF';")
    variants = []
    for vi, (x, y, w, h) in enumerate(ALERT_VARIANTS):
        full = vi == 0
        bg = rect("Avviso sfondo", x, y, w, h, bg="#FF0D47A1", border=None, bw=0, radius=0,   # squadrato, senza bordo
                  bindings={"BackgroundColor": js("var c = $prop('DTPlugin.Avviso_Sfondo');\r\nreturn c ? c : '#FF000000';")},
                  border_bind=None if full else tcol)
        th = round(h * (0.17 if not full else 0.15))
        kids = [bg, text("Avviso titolo", x + 10, y + h * 0.03, w - 20, th, "[DTPlugin.Avviso_Titolo]", "PIT LIMITER",
                         int(fit(w - 20, th, 12, 0.8) * 0.62), WHITE, weight="Medium",
                         bindings={"TextColor": js("var c = $prop('DTPlugin.Avviso_TitoloColore');\r\nreturn c ? c : '#FFFFFFFF';")})]
        iy = y + h * 0.03 + th
        ih = y + h - iy - h * 0.05
        ix, iw = x + w * 0.04, w * 0.92
        nl = []
        for n in range(1, ALERT_MAX_VALUES + 1):
            cols, rows = best_grid(n, iw, ih, 8)
            if n == 4 and iw / max(ih, 1) > 2.6:
                cols, rows = 4, 1
            cw, rh = iw / cols, ih / rows
            items = []
            for i in range(n):
                r, k = divmod(i, cols)
                cx, cy = ix + k * cw, iy + r * rh
                vh = rh * 0.70
                vfs = int(fit(cw - 10, vh, 7 if n > 1 else 6, 0.85, cap=260) * 0.85)   # numeri meno grossi
                items.append(text("Avviso valore %d" % (i + 1), cx, cy, cw, vh, "[DTPlugin.Avviso_V%d]" % (i + 1),
                                  ["86", "1:44.760", "2.71", "5.4"][i], vfs, WHITE, weight="Medium", va=2, bindings={"TextColor": tcol}))
                lfs = clamp(int(min(rh * 0.22, (cw - 6) / (12 * 0.62))), 8, 34)
                items.append(text("Avviso nome %d" % (i + 1), cx, cy + vh, cw, rh - vh, "[DTPlugin.Avviso_L%d]" % (i + 1),
                                  ["KM/H", "ULTIMO", "CONSUMO", "GIRI CARB."][i], lfs, WHITE, weight="Normal", va=0,
                                  bindings={"TextColor": tcol}))
            if n == 1:
                # un solo valore senza scritta sotto (cambio valori): il valore usa tutto lo spazio
                # ai lati la freccia: verde su se il valore e' aumentato, rossa giu' se e' diminuito
                ad = min(ih * 0.42, iw * 0.13)
                vw = iw - 2 * (ad + iw * 0.05)
                solo = [text("Avviso valore solo", ix + (iw - vw) / 2, iy, vw, ih, "[DTPlugin.Avviso_V1]", "52.5",
                             int(fit(vw - 10, ih * 0.85, 5, 0.85, cap=260) * 0.85), WHITE, weight="Medium", bindings={"TextColor": tcol})]
                img_js = "return $prop('DTPlugin.Avviso_Freccia') < 0 ? 'dt-cambio-giu' : 'dt-cambio-su';"
                for side, ax in (("sinistra", ix + iw * 0.03), ("destra", ix + iw - iw * 0.03 - ad)):
                    arr = image("Avviso freccia %s" % side, ax, iy + (ih - ad) / 2, ad, ad, "dt-cambio-su", img_js)
                    arr.setdefault("Bindings", {})["Visible"] = js("var d = $prop('DTPlugin.Avviso_Freccia');\r\nreturn d != null && d != 0;")
                    solo.append(arr)
                nl.append(layer("Valore solo", solo, None, ("Avviso_Solo", 1),
                                vis_js="var l = $prop('DTPlugin.Avviso_L1');\r\nreturn $prop('DTPlugin.Avviso_N') == 1 && (l == null || l === '') && $prop('DTPlugin.Avviso_Danni') !== true;"))
                vis = ("var l = $prop('DTPlugin.Avviso_L1');\r\nreturn $prop('DTPlugin.Avviso_N') == 1 && l != null && l !== '' && $prop('DTPlugin.Avviso_Danni') !== true;")
            else:
                vis = "return $prop('DTPlugin.Avviso_N') == %d && $prop('DTPlugin.Avviso_Danni') !== true;" % n
            nl.append(layer("Valori %d" % n, items, None, ("Avviso_N", n), vis_js=vis))
        kids += nl
        kids.append(layer("Schema danni", damage_items(ix, iy, iw, ih), None, ("Avviso_Danni", 1),
                          vis_js="return $prop('DTPlugin.Avviso_Danni') === true;"))
        if vi in (7, 8):
            _slide(kids, -1 if vi == 7 else 1)
        variants.append(layer("Variante %d" % vi, kids, None, ("Avviso_Variante", vi),
                              vis_js="return $prop('DTPlugin.Avviso_Variante') == %d && $prop('DTPlugin.Avviso_Box') !== true;" % vi))
    variants.append(layer("Schermata box", pit_box_items(), None, ("Avviso_Box", 1),
                          vis_js="return $prop('DTPlugin.Avviso_Box') === true;"))
    return layer("Avvisi", variants, None, ("Avviso_Attivo", 1), vis_js="return $prop('DTPlugin.Avviso_Attivo') === true;")


T_IMGFILE = "SimHub.Plugins.OutputPlugins.GraphicalDash.Models.ImageFromFileItem, SimHub.Plugins"
IDLE_BANDS = 48
# scie animate: (y, lunghezza, spessore, velocita' px/s, ritardo, opacita')
STREAKS = [(40, 320, 3, 260, 0, 0.55), (75, 520, 6, 180, 300, 0.35), (110, 260, 2, 420, 120, 0.7), (150, 600, 9, 140, 700, 0.25),
           (185, 380, 4, 320, 50, 0.5), (220, 700, 12, 110, 400, 0.2), (250, 300, 3, 380, 650, 0.65), (285, 480, 7, 200, 220, 0.4),
           (315, 260, 2, 460, 900, 0.7), (350, 560, 10, 150, 80, 0.25), (385, 340, 4, 300, 500, 0.5), (420, 640, 8, 170, 260, 0.3),
           (450, 280, 3, 400, 760, 0.6), (95, 220, 2, 520, 380, 0.75), (265, 240, 2, 560, 140, 0.8), (400, 200, 2, 600, 820, 0.8)]


def badge_layer():
    W, H = DASH_W, DASH_H
    cx, cy = W / 2.0, H / 2.0
    S = "fd_idle('StemmaDim', 250)"
    BW = "fd_idle('StemmaBordoSpessore', 8)"

    def circle(name, size_expr, color_expr, default_size, color):
        r = rect(name, cx - default_size / 2, cy - default_size / 2, default_size, default_size, bg=color, radius=int(default_size / 2),
                 bindings={"Left": js("return %s - (%s) / 2;" % (cx, size_expr)),
                           "Top": js("return %s - (%s) / 2;" % (cy, size_expr)),
                           "Width": js("return %s;" % size_expr),
                           "Height": js("return %s;" % size_expr),
                           "BackgroundColor": js("return %s;" % color_expr)})
        rad = js("return (%s) / 2;" % size_expr)
        r["BorderStyle"]["Bindings"] = {"RadiusTopLeft": rad, "RadiusTopRight": rad, "RadiusBottomLeft": rad, "RadiusBottomRight": rad}
        return r

    outer = circle("Stemma bordo", S, "fd_idle('StemmaBordo', '#FFFF8C00')", 250, "#FFFF8C00")
    inner = circle("Stemma cerchio", "%s - 2 * %s" % (S, BW), "fd_idle('StemmaColore', '#FF1A1A1A')", 234, "#FF1A1A1A")

    # logo: largo il 60% del cerchio, rispettando le proporzioni dell'immagine; piu' in alto se c'e' il nome
    logo_w = ("var s = %s, a = fd_idle('StemmaLogoRapporto', 2), w = s * 0.6, h = w / a;\r\n"
              "if (h > s * 0.42) { h = s * 0.42; w = h * a; }\r\n") % S
    has_text = "(('' + fd_idle('StemmaTesto', '')).length > 0)"
    has_num = "(fd_idle('StemmaNumero', false) === true)"
    img = {"$type": T_IMGFILE, "ImagePath": ""}
    img.update(base("Stemma logo", cx - 75, cy - 50, 150, 76, {
        "ImagePath": js("return fd_idle('StemmaLogo', '');"),
        "Width": js(logo_w + "return w;"),
        "Height": js(logo_w + "return h;"),
        "Left": js(logo_w + "return %s - w / 2;" % cx),
        "Top": js(logo_w + "var up = (%s ? 0.08 : 0) + (%s ? 0.09 : 0);\r\nreturn %s - h / 2 - s * up;" % (has_text, has_num, cy)),
        "Visible": js("var p = fd_idle('StemmaLogo', '');\r\nreturn p != null && p != '';")}))
    img["_pv"] = dict(kind="none")

    txt = text("Stemma nome", cx - 100, cy + 42, 200, 34, "[DTPlugin.Attesa_StemmaTesto]", "DT RACING TEAM", 21, WHITE, weight="Black", bindings={
        "TextColor": js("return fd_idle('StemmaTestoColore', '#FFFFFFFF');"),
        "Left": js("return %s - (%s) * 0.42;" % (cx, S)),
        "Width": js("return (%s) * 0.84;" % S),
        "Top": js("return %s + (%s) * (%s ? 0.06 : 0.15);" % (cy, S, has_num)),
        "Height": js("return (%s) * 0.16;" % S),
        "FontSize": js("return Math.max(8, (%s) * 0.085);" % S)})
    num = text("Stemma numero", cx - 100, cy + 60, 200, 50, "[DTPlugin.Attesa_Numero]", "80", 34, WHITE, weight="Black", bindings={
        "TextColor": js("return fd_idle('NumeroColore', '#FFFFFFFF');"),
        "Visible": js("return %s;" % has_num),
        "Left": js("return %s - (%s) * 0.42;" % (cx, S)),
        "Width": js("return (%s) * 0.84;" % S),
        "Top": js("return %s + (%s) * (%s ? 0.20 : 0.10);" % (cy, S, has_text)),
        "Height": js("return (%s) * 0.20;" % S),
        "FontSize": js("return Math.max(10, (%s) * 0.14);" % S)})
    return layer("Stemma", [outer, inner, img, txt, num], None, ("Attesa_Stemma", 1),
                 vis_js="return fd_idle('Stemma', false) === true;")


def idle_items():
    """Schermata di attesa (gioco chiuso): numero, nome, sfondo a tinta/sfumatura/immagine/animato, tutto dal plugin."""
    W, H = DASH_W, DASH_H
    P_ = "DTPlugin.Attesa_"
    c1 = "fd_idle('Colore1', '#FF000000')"
    c2 = "fd_idle('Colore2', '#FF00E676')"
    mode = "fd_idle('Modo', 0)"
    grad = "fd_idle('Sfumatura', 2)"
    it = [rect("Sfondo", 0, 0, W, H, bg="#FF000000", radius=0, bindings={"BackgroundColor": js("return %s;" % c1)})]

    # sfumature fatte a bande
    def bands(kind, gi):
        kids = []
        for i in range(IDLE_BANDS):
            t = i / (IDLE_BANDS - 1.0)
            if kind == "v":
                bh = H / IDLE_BANDS
                x, y, w, h = 0, i * bh, W, bh + 1
            elif kind == "h":
                bw = W / IDLE_BANDS
                x, y, w, h = i * bw, 0, bw + 1, H
            else:  # centro: rettangoli concentrici, al centro il colore 2
                f = 1 - i / float(IDLE_BANDS)
                w, h = W * f, H * f
                x, y = (W - w) / 2, (H - h) / 2
            kids.append(rect("Banda %d" % i, x, y, w, h, bg="#FF000000", radius=int(40 * (1 - t)) if kind == "c" else 0,
                             bindings={"BackgroundColor": js("return fd_mix(%s, %s, %.3f);" % (c1, c2, t))}))
        return layer("Sfumatura %s" % kind, kids, None, ("Attesa_Sfumatura", gi),
                     vis_js="return %s == 0 && %s == %d;" % (mode, grad, gi))
    it += [bands("v", 1), bands("h", 2), bands("c", 3)]

    # immagine a tutto schermo (file scelto nel plugin)
    img = {"$type": T_IMGFILE, "ImagePath": ""}
    img.update(base("Immagine", 0, 0, W, H, {"ImagePath": js("return $prop('%sImmagine');" % P_)}))
    img["_pv"] = dict(kind="none")
    it.append(layer("Sfondo immagine", [img], None, ("Attesa_Modo", 1),
                    vis_js="var p = $prop('%sImmagine');\r\nreturn %s == 1 && p != null && p != '';" % (P_, mode)))

    # animazioni
    sk = []
    for i, (y, ln, th, sp, dl, op) in enumerate(STREAKS):
        span = W + ln
        sk.append(rect("Scia %d" % i, -ln, y, ln, th, bg="#FF00E676", radius=max(1, th // 2), bindings={
            "Left": js("return ((Date.now() / 1000 * %d + %d) %% %d) - %d;" % (sp, dl, span, ln)),
            "BackgroundColor": js("return fd_alpha(%s, %.2f);" % (c2, op))}))
    it.append(layer("Animazione scie", sk, None, ("Attesa_Animazione", 0),
                    vis_js="return %s == 2 && fd_idle('Animazione', 0) == 0;" % mode))
    it.append(layer("Animazione respiro", [
        rect("Respiro", 0, 0, W, H, bg="#FF00E676", radius=0, bindings={
            "BackgroundColor": js("return %s;" % c2),
            "Opacity": js("return 10 + 35 * (1 + Math.sin(Date.now() / 1000 * 1.3)) / 2;")})],
        None, ("Attesa_Animazione", 1), vis_js="return %s == 2 && fd_idle('Animazione', 0) == 1;" % mode))

    # stemma: cerchio con bordo, logo e nome dentro
    it.append(badge_layer())

    # numero e nome
    it.append(text("Numero", 0, 70, W, 230, "[%sNumero]" % P_, "80", 210, WHITE, weight="Black",
                   bindings={"TextColor": js("return fd_idle('NumeroColore', '#FFFFFFFF');"),
                             "Visible": js("return fd_idle('NumeroVisibile', false) === true;")}))
    it.append(text("Nome", 0, 295, W, 60, "[%sNome]" % P_, "Federico Pegoraro", 44, WHITE, weight="Black",
                   bindings={"TextColor": js("return fd_idle('NomeColore', '#FFFFFFFF');"),
                             "Visible": js("return fd_idle('NomeVisibile', false) === true;")}))
    # titolo di riserva se il plugin non c'e'
    it.append(layer("Titolo di riserva", [
        text("Titolo", 0, 150, W, 110, None, "DT", 96, WHITE, weight="Black"),
        text("Sottotitolo", 0, 255, W, 40, None, "DASHBOARD", 28, GREY, weight="Medium")],
        None, ("PluginMancante", 1), vis_js="return $prop('DTPlugin.Attesa_Modo') == null;"))
    it.append(text("Ora", W - 160, 10, 150, 40, ch_val("Clock"), "12:13", 26, WHITE, ha=2,
                   bindings={"Visible": js("return fd_idle('Orologio', true) === true;")}))
    it.append(text("Attesa", 0, H - 50, W, 30, None, "In attesa del gioco…", 16, GREY, weight="Medium",
                   bindings={"Visible": js("return fd_idle('Scritta', true) === true;")}))
    return it


def main_dash(pages=None):
    W, H = DASH_W, DASH_H
    idle = idle_items()
    race = [rect("Sfondo", 0, 0, W, H, bg="#FF000000", radius=0)]
    race += [page_layer(i, pg) for i, pg in enumerate(pages if pages is not None else DEFAULT_PAGES)]
    flag_js = ("if ($prop('DTPlugin.FlagBlue_Valore') > 0) { return '#FF2979FF'; }\r\n"
               "return '#FFFFD600';")
    race.append(rect("Bandiera", 0, 0, W, H, bg="#00000000", border=YELLOW, bw=5, radius=0,
                     border_bind=js(flag_js),
                     bindings={"Visible": js("return $prop('DTPlugin.FlagYellow_Valore') > 0 || $prop('DTPlugin.FlagBlue_Valore') > 0;")}))
    race[-1]["Visible"] = False
    race.pop()   # niente bordo a tutto schermo: le bandiere compaiono nel riquadro in alto a destra
    toast = layer("Avviso pagina", [
        rect("Avviso fondo", W / 2 - 170, H / 2 - 45, 340, 90, bg="#F0101010", border=WHITE, bw=2, radius=14),
        text("Avviso testo", W / 2 - 170, H / 2 - 45, 340, 90, "[%sPaginaNome]" % P, "Gara", 38, WHITE),
    ], None, ("MostraPagina", 1), vis_js="return $prop('DTPlugin.MostraPagina') === true;")
    race.append(alert_layer())
    race.append(toast)
    race.append(engine_off_layer())
    race.append(plugin_warning())
    idle.append(plugin_warning(big=True))
    race.append(lock_layer())
    idle.append(lock_layer())
    scr = [screen("Attesa", idle, ingame=False, idle=True, bg="#FF000000"),
           screen("Gara", race, ingame=True, bg="#FF000000")]
    meta = metadata(W, H, scr, title=DASH_NAME, ingame=[1], idle=[0], preview=1)
    return dash_doc(W, H, scr, meta, controls=True), meta


# ------------------------------------------------------------------ JS di riserva
def js_extension():
    rows = []
    for (cid, label, src, fmt, dec, unit, chars, sample) in CHANNELS:
        rows.append("    %s: [%s, %s, %d, %s, %s]" % (cid, json.dumps(src), json.dumps(fmt), dec, json.dumps(unit, ensure_ascii=False), json.dumps(label, ensure_ascii=False)))
    return JS_TEMPLATE.replace("/*CANALI*/", ",\n".join(rows))


JS_TEMPLATE = r"""// DT Dashboard - funzioni della dashboard.
// fd('X')  = testo del valore X       fdv('X') = numero       fdn('X') = etichetta
// Se il DT Plugin e' attivo usa i suoi valori (DTPlugin.X); altrimenti legge
// direttamente le proprieta' di SimHub (funziona con LMU e con gli altri giochi).
var FD_CH = {
/*CANALI*/
};

function fd_ok(v) { return v !== null && v !== undefined; }

function fd_pad(n, l) { var s = '' + n; while (s.length < l) { s = '0' + s; } return s; }

function fd_num(v) {
    if (!fd_ok(v) || v === '') { return null; }
    if (typeof v === 'number') { return v; }
    if (typeof v === 'boolean') { return v ? 1 : 0; }
    var n = parseFloat(v);
    return isNaN(n) ? null : n;
}

function fd_secs(v) {
    if (!fd_ok(v) || v === '') { return null; }
    if (typeof v === 'number') { return v; }
    try {
        var t = timespantoseconds(v);
        if (fd_ok(t) && !isNaN(t)) { return t; }
    } catch (e) { }
    var s = '' + v;
    var m = s.match(/^(-)?(?:(\d+)\.)?(\d+):(\d+):(\d+(?:\.\d+)?)$/);
    if (m) {
        var r = (parseInt(m[2] || '0', 10) * 86400) + parseInt(m[3], 10) * 3600 + parseInt(m[4], 10) * 60 + parseFloat(m[5]);
        return m[1] ? -r : r;
    }
    var n = parseFloat(s);
    return isNaN(n) ? null : n;
}

function fd_lap(s) {
    if (!fd_ok(s) || s <= 0) { return '-:--.---'; }
    var ms = Math.round(s * 1000);
    return Math.floor(ms / 60000) + ':' + fd_pad(Math.floor(ms / 1000) % 60, 2) + '.' + fd_pad(ms % 1000, 3);
}

function fd_dur(s) {
    if (!fd_ok(s) || s <= 0) { return '--:--'; }
    s = Math.floor(s);
    var h = Math.floor(s / 3600), m = Math.floor(s / 60) % 60, x = s % 60;
    return h > 0 ? h + ':' + fd_pad(m, 2) + ':' + fd_pad(x, 2) : fd_pad(m, 2) + ':' + fd_pad(x, 2);
}

function fd_gap(offset) {
    try {
        var pos = getopponentleaderboardposition_aheadbehind(offset);
        if (!fd_ok(pos) || pos <= 0) { return null; }
        return fd_secs(drivergaptoplayer(pos));
    } catch (e) { return null; }
}

function fd_raw(id) {
    var c = FD_CH[id];
    if (!c || c[0] === '') { return null; }
    var src = c[0];
    if (src === '@ora') { var d = new Date(); return d.getHours() * 3600 + d.getMinutes() * 60 + d.getSeconds(); }
    if (src === '@gap_avanti') { return fd_gap(-1); }
    if (src === '@gap_dietro') { return fd_gap(1); }
    if (src.indexOf('@raw:') === 0) { return $prop('GameRawData.' + src.substring(5)); }
    if (src === '@ve') {
        var ve = $prop('GameRawData.PlayerNativeTelemetry.mVirtualEnergy');
        return fd_ok(ve) ? ve * 100 : null;
    }
    if (src === '@ers') {
        var bt = $prop('GameRawData.CurrentPlayerTelemetry.mBatteryChargeFraction');
        return fd_ok(bt) ? bt * 100 : $prop('DataCorePlugin.GameData.ERSPercent');
    }
    if (src === '@limitatore') {
        var rl = $prop('GameRawData.CurrentPlayerTelemetry.mSpeedLimiter');
        if (fd_ok(rl)) { return rl > 0 ? 1 : 0; }
        return $prop('DataCorePlugin.GameData.PitLimiterOn');
    }
    if (src.indexOf('@press_') === 0) {
        var pw = src.substring(7), pidx = { FL: '01', FR: '02', RL: '03', RR: '04' }[pw];
        var pn = { FL: 'FrontLeft', FR: 'FrontRight', RL: 'RearLeft', RR: 'RearRight' };
        var kp = $prop('GameRawData.CurrentPlayerTelemetry.mWheels' + pidx + '.mPressure');
        if (fd_ok(kp)) { return kp; }
        var ps = $prop('DataCorePlugin.GameData.TyrePressure' + pn[pw]);
        return fd_ok(ps) ? ps * 6.894757 : null;
    }
    if (src.indexOf('@gomma_') === 0) {
        var w = src.substring(7), idx = { FL: '01', FR: '02', RL: '03', RR: '04' }[w];
        var names = { FL: 'FrontLeft', FR: 'FrontRight', RL: 'RearLeft', RR: 'RearRight' };
        var a = $prop('GameRawData.CurrentPlayerTelemetry.mWheels' + idx + '.mTireInnerLayerTemperature01');
        var b = $prop('GameRawData.CurrentPlayerTelemetry.mWheels' + idx + '.mTireInnerLayerTemperature02');
        var c = $prop('GameRawData.CurrentPlayerTelemetry.mWheels' + idx + '.mTireInnerLayerTemperature03');
        if (fd_ok(a) && fd_ok(b) && fd_ok(c) && a > 0) { return (a + b + c) / 3 - 273.15; }
        return $prop('DataCorePlugin.GameData.TyreTemperature' + names[w]);
    }
    if (src === '@delta_ultimo') {
        var l = fd_secs($prop('DataCorePlugin.GameData.LastLapTime')), b = fd_secs($prop('DataCorePlugin.GameData.BestLapTime'));
        return (fd_ok(l) && fd_ok(b) && l > 0 && b > 0) ? l - b : null;
    }
    if (src === '@delta') {
        var gd = $prop('GameRawData.CurrentPlayerTelemetry.mDeltaBest');
        return fd_ok(gd) ? gd : $prop('PersistantTrackerPlugin.SessionBestLiveDeltaSeconds');
    }
    if (src === '@stimato') {
        var bl = fd_secs($prop('DataCorePlugin.GameData.BestLapTime')), dd = $prop('GameRawData.CurrentPlayerTelemetry.mDeltaBest');
        if (fd_ok(bl) && bl > 0 && fd_ok(dd)) { return bl + dd; }
        return $prop('PersistantTrackerPlugin.EstimatedLapTime_SessionBestBased');
    }
    if (src === '@bagnato') { var bw = $prop('GameRawData.Scoring.mScoringInfo.mAvgPathWetness'); return fd_ok(bw) ? bw * 100 : null; }
    if (src === '@pioggia') { var rn = $prop('GameRawData.Scoring.mScoringInfo.mRaining'); return fd_ok(rn) ? rn * 100 : null; }
    if (src === '@giri_carb') {
        var gl = $prop('DataCorePlugin.Computed.Fuel_RemainingLaps');
        if (fd_ok(gl) && gl > 0) { return gl; }
        var fl = $prop('DataCorePlugin.GameData.Fuel'), pl = $prop('DataCorePlugin.Computed.Fuel_LastLapConsumption');
        return (fd_ok(fl) && fd_ok(pl) && pl > 0.01) ? fl / pl : null;
    }
    if (src === '@pos') { return $prop('DataCorePlugin.GameData.Position'); }
    if (src === '@auto') { return $prop('DataCorePlugin.GameData.OpponentsCount'); }
    try { return $prop(src); } catch (e) { return null; }
}

function fd_text(id) {
    var c = FD_CH[id];
    if (!c) { return '-'; }
    var v = fd_raw(id);
    if (!fd_ok(v) || v === '') { return '-'; }
    var f = c[1], dec = c[2], unit = c[3];
    if (f === 'laptime') { return fd_lap(fd_secs(v)); }
    if (f === 'duration') { return fd_dur(fd_secs(v)); }
    if (f === 'clock') { var s = fd_secs(v); return fd_pad(Math.floor(s / 3600) % 24, 2) + ':' + fd_pad(Math.floor(s / 60) % 60, 2); }
    var n = fd_num(v);
    if (f === 'text') {
        if (n === null || typeof v === 'string') { return '' + v; }
        return (Math.abs(n - Math.round(n)) < 1e-9 ? '' + Math.round(n) : n.toFixed(dec)) + unit;
    }
    if (n === null) { return '' + v; }
    if (f === 'gap') { return Math.abs(n).toFixed(dec) + unit; }
    if (f === 'signed') { return (n > 0 ? '+' : '') + n.toFixed(dec) + unit; }
    return n.toFixed(dec) + unit;
}

function fd_idle(name, def) {
    var v = $prop('DTPlugin.Attesa_' + name);
    return fd_ok(v) ? v : def;
}

function fd_hex(c) {
    c = ('' + (c || '')).replace('#', '');
    if (c.length === 6) { c = 'FF' + c; }
    if (c.length !== 8) { c = 'FF000000'; }
    return [parseInt(c.substr(0, 2), 16), parseInt(c.substr(2, 2), 16), parseInt(c.substr(4, 2), 16), parseInt(c.substr(6, 2), 16)];
}

function fd_tohex(n) { var s = Math.max(0, Math.min(255, Math.round(n))).toString(16).toUpperCase(); return s.length < 2 ? '0' + s : s; }

function fd_mix(a, b, t) {
    var x = fd_hex(a), y = fd_hex(b);
    return '#' + fd_tohex(x[0] + (y[0] - x[0]) * t) + fd_tohex(x[1] + (y[1] - x[1]) * t) + fd_tohex(x[2] + (y[2] - x[2]) * t) + fd_tohex(x[3] + (y[3] - x[3]) * t);
}

function fd_alpha(c, a) {
    var x = fd_hex(c);
    return '#' + fd_tohex(255 * a) + fd_tohex(x[1]) + fd_tohex(x[2]) + fd_tohex(x[3]);
}

function fd(id) {
    var v = $prop('DTPlugin.' + id);
    if (fd_ok(v)) { return v; }
    return fd_text(id);
}

function fdv(id) {
    var v = $prop('DTPlugin.' + id + '_Valore');
    if (fd_ok(v)) { return v; }
    var c = FD_CH[id];
    if (!c) { return 0; }
    var r = fd_raw(id);
    var n = (c[1] === 'laptime' || c[1] === 'duration' || c[1] === 'clock') ? fd_secs(r) : fd_num(r);
    return n === null ? 0 : n;
}

function fdn(id) {
    var v = $prop('DTPlugin.' + id + '_Nome');
    if (fd_ok(v)) { return v; }
    var c = FD_CH[id];
    return c ? c[4] : id;
}
"""


# ------------------------------------------------------------------ salvataggio
def _static_preview(o):
    """Valori fissi = quelli d'esempio dell'anteprima (testi, colori, livello delle barre, livelli nascosti).
    A dash attiva contano i collegamenti (Bindings), quindi servono solo all'anteprima del plugin e all'editor di SimHub."""
    pv = o.get("_pv")
    if not pv:
        return o
    o = dict(o)
    b = o.get("Bindings") or {}
    k = pv.get("kind")
    if k == "text":
        if not o.get("Text") and pv.get("text"):
            o["Text"] = pv["text"]
        if pv.get("color") and "TextColor" in b:
            o["TextColor"] = pv["color"]
    elif k == "rect":
        if pv.get("bg") and "BackgroundColor" in b:
            o["BackgroundColor"] = pv["bg"]
        if "vfrac" in pv and "Height" in b:
            nh = o["Height"] * pv["vfrac"]
            o["Top"] = float(round(o["Top"] + o["Height"] - nh)); o["Height"] = float(round(nh))
        if "hfrac" in pv and "Width" in b:
            o["Width"] = float(round(o["Width"] * pv["hfrac"]))
    elif k == "layer":
        vis = pv.get("vis")
        if vis and vis[1] != 0 and vis[0] != "Pagina" and "Visible" in b:
            o["Visible"] = False
    return o


def strip(o):
    if isinstance(o, dict):
        o = _static_preview(o)
        return {k: strip(v) for k, v in o.items() if k != "_pv"}
    if isinstance(o, list):
        return [strip(v) for v in o]
    return o


def write_thumbs(folder, name, doc, ctx, card_files=None):
    """Miniature per l'elenco di SimHub: nome.djson.png (531x300) e una per schermata (nome.djson.NN.png, alte 100)."""
    import render as R
    from PIL import Image, ImageDraw
    w, h = int(doc["BaseWidth"]), int(doc["BaseHeight"])
    shots = []
    for i, sc in enumerate(doc["Screens"]):
        img = Image.new("RGBA", (w * 2, h * 2), (0, 0, 0, 255))
        R.draw(ImageDraw.Draw(img), sc["Items"], 0, 0, ctx, 2, card_files or {})
        shots.append(img)
        img.resize((int(round(w * 100.0 / h)), 100), Image.LANCZOS).save(os.path.join(folder, "%s.djson.%02d.png" % (name, i)))
    main = shots[doc["Metadata"].get("MainPreviewIndex", 0) if doc.get("Metadata") else 0]
    main.resize((int(round(w * 300.0 / h)), 300), Image.LANCZOS).save(os.path.join(folder, name + ".djson.png"))


def write_all():
    fonts_src = os.path.join(os.path.dirname(os.path.abspath(__file__)), "fonts")
    folder = os.path.join(OUT, DASH_NAME)
    if os.path.exists(OUT):
        shutil.rmtree(OUT)
    os.makedirs(os.path.join(folder, "_SHFonts"))
    main, meta = main_dash()
    # pezzi per il DT Plugin: centro, griglie, barre per ogni cornice (crea anche tutti i file delle schede)
    pieces = {}
    for fi in range(len(FRAMES)):
        for ci in range(len(CENTER_LAYOUTS)):
            pieces["CENTRO_%d_%d" % (fi, ci)] = center_items(fi, ci)
        pieces["LATERALI_%d" % fi] = piece_sidebars(fi)
        for gi in range(len(GRIDS)):
            pieces["GRIGLIA_%d_L_%d" % (fi, gi)] = piece_grid(fi, "L", gi)
            pieces["GRIGLIA_%d_R_%d" % (fi, gi)] = piece_grid(fi, "R", gi)
        for ti in range(len(TOOLBARS)):
            pieces["BARRA_%d_%d" % (fi, ti)] = piece_toolbar(fi, ti)
    page_tpl = layer("Pagina @@IDX@@", ["@@FIGLI@@"], None, ("Pagina", 0), vis_js=page_vis("@@IDX@@"))
    files = {DASH_NAME + ".djson": main}
    files.update(CARD_FILES)
    import hashlib, icons
    ICONS = icons.all_icons()
    if os.environ.get("DT_SENZA_ICONE"):
        # versione di prova: niente immagini (serve a capire se il problema sono le icone)
        def drop(o):
            if isinstance(o, dict):
                if "Childrens" in o:
                    o["Childrens"] = [drop(c) for c in o["Childrens"] if not (isinstance(c, dict) and c.get("$type") == T_IMAGE)]
                for k, v in o.items():
                    if k != "Childrens":
                        drop(v)
            elif isinstance(o, list):
                for v in o:
                    drop(v)
            return o
        for doc in files.values():
            for sc in doc["Screens"]:
                sc["Items"] = [drop(i) for i in sc["Items"] if not (isinstance(i, dict) and i.get("$type") == T_IMAGE)]
        ICONS = {}
    for fn, doc in files.items():
        body = json.dumps(strip(doc), ensure_ascii=False, separators=(",", ":"))
        used = [n for n in ICONS if ('"%s"' % n) in body or ("'%s'" % n) in body
                 or (n.startswith("dt-dz") and "'dt-dz" in body) or (n.startswith("dt-cielo") and "'dt-cielo-'" in body) or (n.startswith("dt-freccia") and "'dt-freccia-" in body)]      # zone danno scelte a runtime ('dt-dz3-' + livello)
        if used:
            import struct
            def png_size(b):
                return struct.unpack(">II", b[16:24])
            doc["Images"] = [{"Name": n, "Extension": ".png", "Modified": False, "Optimized": True, "Width": png_size(ICONS[n])[0],
                              "Height": png_size(ICONS[n])[1], "Length": len(ICONS[n]), "MD5": hashlib.md5(ICONS[n]).hexdigest()} for n in used]
            with zipfile.ZipFile(os.path.join(folder, fn + ".ressources"), "w", zipfile.ZIP_DEFLATED) as z:
                for n in used:
                    z.writestr(n + ".png", ICONS[n])
            body = json.dumps(strip(doc), ensure_ascii=False, separators=(",", ":"))
        with open(os.path.join(folder, fn), "w", encoding="utf-8") as f:
            f.write(body)
    with open(os.path.join(folder, DASH_NAME + ".djson.metadata"), "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)
    # modello della dash (le pagine al posto di "@@PAGINE@@") + pezzi, per il DT Plugin
    import copy
    tpl = copy.deepcopy(main)
    items = tpl["Screens"][1]["Items"]
    first = next(i for i, it in enumerate(items) if isinstance(it, dict) and str(it.get("Name", "")).startswith("Pagina "))
    tpl["Screens"][1]["Items"] = [it for it in items if not (isinstance(it, dict) and str(it.get("Name", "")).startswith("Pagina "))]
    tpl["Screens"][1]["Items"].insert(first, "@@PAGINE@@")
    with open(os.path.join(folder, DASH_NAME + ".djson.modello"), "w", encoding="utf-8") as f:
        f.write(json.dumps(strip(tpl), ensure_ascii=False, separators=(",", ":")))
    with open(os.path.join(folder, DASH_NAME + ".pezzi"), "w", encoding="utf-8", newline="\n") as f:
        f.write("PAGINA\t" + json.dumps(strip(page_tpl), ensure_ascii=False, separators=(",", ":")) + "\n")
        for nm, key, cond, pv in (("Barra doppia", "STRATO_D", DOUBLE_ON, 1), ("Barra singola", "STRATO_S", DOUBLE_OFF, 0)):
            f.write(key + "\t" + json.dumps(strip(layer(nm, ["@@FIGLI@@"], None, ("BarraDoppia", pv), vis_js=cond)), ensure_ascii=False, separators=(",", ":")) + "\n")
        for fd_, fs_ in SINGLE_OF.items():
            f.write("SINGOLA_%d\t%d\n" % (fd_, fs_))
        for k, v in pieces.items():
            f.write(k + "\t" + json.dumps(strip(v), ensure_ascii=False, separators=(",", ":")) + "\n")
    os.makedirs(os.path.join(folder, "JavascriptExtensions"))
    with open(os.path.join(folder, "JavascriptExtensions", "DT-Dashboard.js"), "w", encoding="utf-8") as f:
        f.write(js_extension())
    for ttf in ("Roboto-Light.ttf", "Roboto-Regular.ttf", "Roboto-Medium.ttf", "Roboto-Bold.ttf", "Roboto-Black.ttf",
                "RobotoCondensed-Light.ttf", "RobotoCondensed-Regular.ttf", "RobotoCondensed-Bold.ttf"):
        shutil.copy(os.path.join(fonts_src, ttf), os.path.join(folder, "_SHFonts", ttf))
    try:
        import render as R
        write_thumbs(folder, DASH_NAME, main, R.page_ctx(DEFAULT_PAGES[0]), CARD_FILES)
    except Exception as ex:
        print("miniature non create:", ex)
    return main, files


if __name__ == "__main__":
    main, files = write_all()
    print("file schede:", len(CARD_FILES))
    for k in sorted(CARD_FILES):
        print("  ", k)
