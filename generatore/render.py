# -*- coding: utf-8 -*-
"""Anteprima PNG della dashboard generata (legge gli stessi oggetti scritti nei .djson)."""
import os, sys
from PIL import Image, ImageDraw, ImageFont
import gen_dash as GEN
from catalog import *

HERE = os.path.dirname(os.path.abspath(__file__))
FONTS = {w: os.path.join(HERE, "fonts", "Roboto-%s.ttf" % w) for w in ("Regular", "Medium", "Bold", "Black")}
_fc = {}


def font(weight, size):
    key = (weight, size)
    if key not in _fc:
        _fc[key] = ImageFont.truetype(FONTS.get(weight, FONTS["Bold"]), size)
    return _fc[key]


def col(c):
    c = c.lstrip("#")
    a, r, g, b = (int(c[i:i + 2], 16) for i in (0, 2, 4, 6))
    return (r, g, b, a)


def page_ctx(p):
    d = {"Cornice": p["frame"], "Griglia_L": p["grid_l"], "Griglia_R": p["grid_r"], "BarraInferiore": p["toolbar"],
         "Slot_SL": p["SL"], "Slot_SR": p["SR"], "MostraPagina": 0, "Pagina": 1}
    for k in ("L", "R", "T", "C"):
        for i, v in enumerate(p[k]):
            d["Slot_%s%d" % (k, i + 1)] = v
    return d


def draw(dr, items, ox, oy, ctx, s, files):
    for it in items:
        pv = it.get("_pv", {})
        k = pv.get("kind")
        if k == "none":
            continue
        if k == "layer":
            if pv.get("vis") and ctx.get(pv["vis"][0], 0) != pv["vis"][1]:
                continue
            draw(dr, it["Childrens"], ox, oy, ctx, s, files)
            continue
        if not it.get("Visible", True):
            continue
        x, y, w, h = ox + it["Left"], oy + it["Top"], it["Width"], it["Height"]
        if k == "rect":
            if "vfrac" in pv:
                nh = h * pv["vfrac"]; y, h = y + h - nh, nh
            if "hfrac" in pv:
                w = w * pv["hfrac"]
            if w <= 0 or h <= 0:
                continue
            r = min(pv["radius"], w / 2, h / 2)
            box = [x * s, y * s, (x + w) * s - 1, (y + h) * s - 1]
            fill = col(pv["bg"]) if col(pv["bg"])[3] > 0 else None
            if pv.get("border"):
                dr.rounded_rectangle(box, radius=r * s, fill=fill, outline=col(pv["border"]), width=int(pv["bw"] * s))
            elif fill:
                dr.rounded_rectangle(box, radius=r * s, fill=fill)
        elif k == "text":
            t = pv["text"]
            if not t:
                continue
            f = font(pv["weight"], int(pv["size"] * s))
            ha, va = it["HorizontalAlignment"], it["VerticalAlignment"]
            ax = {0: x * s + 2 * s, 1: (x + w / 2) * s, 2: (x + w) * s - 2 * s}[ha]
            ay = {0: y * s, 1: (y + h / 2) * s, 2: (y + h) * s}[va]
            anchor = {0: "l", 1: "m", 2: "r"}[ha] + {0: "a", 1: "m", 2: "d"}[va]
            dr.text((ax, ay), t, font=f, fill=col(pv["color"]), anchor=anchor)
        elif k == "image":
            import io, icons
            png = icons.all_icons().get(pv["img"])
            if png:
                im = Image.open(io.BytesIO(png)).resize((max(1, int(w * s)), max(1, int(h * s))), Image.LANCZOS)
                dr._image.alpha_composite(im, (int(x * s), int(y * s)))
        elif k == "widget":
            doc = files[pv["file"]]
            idx = ctx.get(pv["slot"], 0)
            draw(dr, doc["Screens"][idx]["Items"], x, y, ctx, s, files)


def render(page, path, s=2, screen_index=1):
    main, _ = GEN.main_dash([page])
    img = Image.new("RGBA", (GEN.DASH_W * s, GEN.DASH_H * s), (0, 0, 0, 255))
    dr = ImageDraw.Draw(img)
    draw(dr, main["Screens"][screen_index]["Items"], 0, 0, page_ctx(page), s, GEN.CARD_FILES)
    img.convert("RGB").save(path)


if __name__ == "__main__":
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(HERE, "preview")
    os.makedirs(out, exist_ok=True)
    for i, p in enumerate(DEFAULT_PAGES):
        render(p, os.path.join(out, "pagina_%d_%s.png" % (i + 1, p["name"].lower())))
    print("ok")
