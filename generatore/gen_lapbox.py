"""DT LapBox: dashboard per la RaceSense LapBox 4" (1050x684).
In alto (0-100) le due luci dietro le finestrelle, comandate dal DT Plugin (DTPlugin.LB_Luce1 / LB_Luce2).
Sotto (100-580) la parte visibile, divisa in 2 colonne x 4 righe: le griglie sono quelle dei lati della DT Dashboard
e le schede sono le stesse (file Schede_LxA.djson alle misure della LapBox).
Ogni blocco possibile e' un widget: il plugin dice quali si vedono (LB_CellaVis_n) e che scheda mostrano (LB_Cella_n).
Cambio pagina e schermata di attesa sono sotto-dashboard della parte visibile (Avvisi.djson, Attesa.djson): niente avvisi a schermo."""
import copy, json, os, shutil, sys, zipfile
import catalog as C
import gen_dash as G

NAME = "DT LapBox"
W, H = C.LB_W, C.LB_H
TOP, BOT = C.LB_TOP, C.LB_BOTTOM
SX, SY, SW, SH = C.LB_X0, C.LB_TOP, C.LB_X1 - C.LB_X0, C.LB_BOTTOM - C.LB_TOP   # parte visibile nella finestra (avvisi di pagina, attesa)
P = "DTPlugin."


def js(code):
    return {"Formula": {"JSExt": 0, "Interpreter": 1, "Expression": code}, "Mode": 2}


def at_size(w, h, fn):
    """Genera elementi pensati per la DT Dashboard (850x480) alla misura w x h."""
    saved = (C.DASH_W, C.DASH_H, G.DASH_W, G.DASH_H, G.ALERT_VARIANTS)
    try:
        C.DASH_W, C.DASH_H = w, h
        G.DASH_W, G.DASH_H = w, h
        G.ALERT_VARIANTS = C.alert_variants()
        return fn()
    finally:
        C.DASH_W, C.DASH_H, G.DASH_W, G.DASH_H, G.ALERT_VARIANTS = saved


def lights():
    """Le due luci: una zona piena dietro ogni foro (colore dal plugin). Spente = nero."""
    it = []
    for k, (x, y, w, h) in enumerate(C.LB_LIGHTS):
        prop = "$prop('%sLB_Luce%d')" % (P, k + 1)
        col = js("var c = %s;\r\nreturn (!c || c.length < 9) ? '#FF000000' : c;" % prop)
        r = G.rect("Luce %d" % (k + 1), x, y, w, h, bg="#FF000000", radius=0, bindings={"BackgroundColor": col})
        r["_pv"]["bg"] = "#FF00E676"
        it.append(r)
    return it


def cells():
    """Tutti i blocchi possibili (21): visibili solo quelli della griglia della pagina attiva."""
    it = []
    for k, cell in enumerate(C.LB_CELLS):
        x, y, w, h = C.lb_rect(cell)
        wd = G.widget("Blocco %d" % (k + 1), x, y, w, h, G.card_file(w, h), "LB_Cella%d" % (k + 1))
        it.append(G.layer("Blocco %d" % (k + 1), [wd], None, ("LB_CellaVis%d" % (k + 1), 1),
                          vis_js="return $prop('%sLB_CellaVis%d') === true;" % (P, k + 1)))
    return it


def overlay_doc():
    """Cambio pagina, motore spento, plugin mancante / bloccato: della parte visibile (gli avvisi a schermo sulla LapBox non servono)."""
    def make():
        toast = G.layer("Avviso pagina", [
            G.rect("Avviso fondo", SW / 2 - 170, SH / 2 - 45, 340, 90, bg="#F0101010", border=G.WHITE, bw=2, radius=14),
            G.text("Avviso testo", SW / 2 - 170, SH / 2 - 45, 340, 90, None, "Gara", 38, G.WHITE,
                   bindings={"Text": js("var n = $prop('%sLB_PaginaNome');\r\nreturn n ? n : '';" % P)}),
        ], None, ("MostraPagina", 1), vis_js="return $prop('%sMostraPagina') === true;" % P)
        return [toast, G.engine_off_layer(), G.plugin_warning(), G.lock_layer()]
    items = at_size(SW, SH, make)
    scr = [G.screen("Avvisi", items, ingame=True)]
    return G.dash_doc(SW, SH, scr, G.metadata(SW, SH, scr))


def idle_doc():
    def make():
        it = G.idle_items()
        it.append(G.plugin_warning(big=True))
        it.append(G.lock_layer())
        return it
    items = at_size(SW, SH, make)
    scr = [G.screen("Attesa", items, ingame=False, idle=True, bg="#FF000000")]
    return G.dash_doc(SW, SH, scr, G.metadata(SW, SH, scr, idle=[0]))


def no_borders(doc):
    """Schede senza cornice: il bordo della "Cornice" a 0 (il nome della scheda resta)."""
    def walk(items):
        for it in items:
            if isinstance(it, dict):
                if it.get("Name") == "Cornice" and "BorderStyle" in it:
                    bs = it["BorderStyle"]
                    for k in ("BorderTop", "BorderBottom", "BorderLeft", "BorderRight"):
                        if k in bs:
                            bs[k] = 0
                    if "_pv" in it:
                        it["_pv"]["border"] = None
                if it.get("Childrens"):
                    walk(it["Childrens"])
    for sc in doc["Screens"]:
        walk(sc["Items"])
    return doc


def overlay_widget(name, fn):
    d = G.widget(name, SX, SY, SW, SH, fn, "_nessuno")
    d["Bindings"] = {}
    d["_pv"] = dict(kind="widget", file=fn, slot="_nessuno")
    return d


def build():
    bg = G.rect("Sfondo", 0, 0, W, H, bg="#FF000000", radius=0)
    race = [bg] + lights() + cells() + [overlay_widget("Avvisi", "Avvisi.djson")]
    idle = [copy.deepcopy(bg), overlay_widget("Attesa", "Attesa.djson")]
    scr = [G.screen("Attesa", idle, ingame=False, idle=True, bg="#FF000000"), G.screen("Gara", race, ingame=True, bg="#FF000000")]
    meta = G.metadata(W, H, scr, title=NAME, ingame=[1], idle=[0], preview=1)
    meta["Description"] = "DT Dashboard per RaceSense LapBox 4\" (layout e schede dal DT Plugin)"
    return G.dash_doc(W, H, scr, meta, controls=True), meta


def preview_ctx(page):
    ctx = {"MostraPagina": 0}
    for k in range(len(C.LB_CELLS)):
        ctx["LB_CellaVis%d" % (k + 1)] = 0
    for i, ci in enumerate(C.LB_GRID_CELLS[page["grid"]]):
        ctx["LB_CellaVis%d" % (ci + 1)] = 1
        ctx["LB_Cella%d" % (ci + 1)] = page["L"][i] if i < len(page["L"]) else 0
    return ctx


def write(out_dir):
    folder = os.path.join(out_dir, NAME)
    if os.path.exists(folder):
        shutil.rmtree(folder)
    os.makedirs(os.path.join(folder, "_SHFonts"))
    before = set(G.CARD_FILES)
    doc, meta = build()
    files = {NAME + ".djson": doc, "Avvisi.djson": overlay_doc(), "Attesa.djson": idle_doc()}
    sizes = {G.card_file(*C.lb_rect(c)[2:]) for c in C.LB_CELLS}
    for fn in sizes:
        files[fn] = no_borders(copy.deepcopy(G.CARD_FILES[fn]))
    import icons
    G.write_docs(folder, files, icons.all_icons())
    with open(os.path.join(folder, NAME + ".djson.metadata"), "w", encoding="utf-8") as f:
        json.dump(meta, f, ensure_ascii=False, indent=2)
    os.makedirs(os.path.join(folder, "JavascriptExtensions"))
    with open(os.path.join(folder, "JavascriptExtensions", "DT-Dashboard.js"), "w", encoding="utf-8") as f:
        f.write(G.js_extension())
    src = os.path.join(os.path.dirname(os.path.abspath(__file__)), "fonts")
    for ttf in ("Roboto-Light.ttf", "Roboto-Regular.ttf", "Roboto-Medium.ttf", "Roboto-Bold.ttf", "Roboto-Black.ttf",
                "RobotoCondensed-Light.ttf", "RobotoCondensed-Regular.ttf", "RobotoCondensed-Bold.ttf"):
        shutil.copy(os.path.join(src, ttf), os.path.join(folder, "_SHFonts", ttf))
    try:
        G.write_thumbs(folder, NAME, doc, preview_ctx(C.DEFAULT_LB_PAGES[0]), files)
    except Exception as ex:
        print("miniature non create:", ex)
    zpath = os.path.join(out_dir, NAME + ".simhubdash")
    with zipfile.ZipFile(zpath, "w", zipfile.ZIP_DEFLATED) as z:
        for dd, _, fs in os.walk(folder):
            for fn in fs:
                full = os.path.join(dd, fn)
                z.write(full, os.path.relpath(full, out_dir).replace("/", "\\"))
    return doc, files, zpath


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(os.path.abspath(__file__)), "out_lapbox")
    os.makedirs(out, exist_ok=True)
    doc, files, z = write(out)
    print(z, len(files), "file")
