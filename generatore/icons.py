# -*- coding: utf-8 -*-
"""Icone della DT Dashboard (disegnate qui, nessun file esterno): luci, tergicristalli, motore, bandiera."""
import io, math
from PIL import Image, ImageDraw

S = 96          # dimensione finale
K = 4           # supersampling
GREY = "#6E6E6E"


def _rgba(c):
    c = c.lstrip("#")
    if len(c) == 8:
        c = c[2:]
    return tuple(int(c[i:i + 2], 16) for i in (0, 2, 4)) + (255,)


def _canvas():
    im = Image.new("RGBA", (S * K, S * K), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im)


def _done(im):
    im = im.resize((S, S), Image.LANCZOS)
    b = io.BytesIO()
    im.save(b, "PNG", optimize=True)
    return b.getvalue()


def lights(color):
    im, d = _canvas(); c = _rgba(color); u = S * K / 96.0; w = int(7 * u)
    # faro a "D" rivolto a sinistra + fasci di luce
    d.chord([40 * u, 22 * u, 84 * u, 74 * u], 90, 270, outline=c, width=w)
    d.line([62 * u, 22 * u, 62 * u, 74 * u], fill=c, width=w)
    for i, yy in enumerate((28, 41, 54, 67)):
        d.line([12 * u, (yy + 5) * u, 34 * u, yy * u], fill=c, width=w)
    return _done(im)


def wipers(color):
    im, d = _canvas(); c = _rgba(color); u = S * K / 96.0; w = int(7 * u)
    # parabrezza (bordo superiore curvo, lati e base) + spazzola + gocce
    d.arc([6 * u, 14 * u, 90 * u, 74 * u], 195, 345, fill=c, width=w)
    d.line([12 * u, 34 * u, 20 * u, 80 * u], fill=c, width=w)
    d.line([84 * u, 34 * u, 76 * u, 80 * u], fill=c, width=w)
    d.line([20 * u, 80 * u, 76 * u, 80 * u], fill=c, width=w)
    d.line([48 * u, 76 * u, 26 * u, 34 * u], fill=c, width=w)
    for (x, y) in ((50, 30), (62, 34), (58, 48), (70, 50), (66, 64)):
        d.ellipse([(x - 3.5) * u, (y - 3.5) * u, (x + 3.5) * u, (y + 3.5) * u], fill=c)
    return _done(im)


def engine(color):
    im, d = _canvas(); c = _rgba(color); u = S * K / 96.0; w = int(6 * u)
    # blocco motore stilizzato con fulmine
    d.line([30 * u, 22 * u, 58 * u, 22 * u], fill=c, width=w)
    d.line([44 * u, 22 * u, 44 * u, 32 * u], fill=c, width=w)
    d.polygon([(22 * u, 32 * u), (66 * u, 32 * u), (74 * u, 40 * u), (74 * u, 70 * u), (34 * u, 70 * u), (22 * u, 60 * u)],
              outline=c, width=w)
    d.line([8 * u, 42 * u, 8 * u, 62 * u], fill=c, width=w)
    d.line([8 * u, 52 * u, 22 * u, 52 * u], fill=c, width=w)
    d.line([74 * u, 50 * u, 88 * u, 50 * u], fill=c, width=w)
    d.line([88 * u, 38 * u, 88 * u, 62 * u], fill=c, width=w)
    d.polygon([(52 * u, 38 * u), (38 * u, 54 * u), (48 * u, 54 * u), (44 * u, 66 * u), (58 * u, 48 * u), (48 * u, 48 * u)], fill=c)
    return _done(im)


def flag(color):
    im, d = _canvas(); c = _rgba(color); u = S * K / 96.0
    # bandiera a scacchi 4x3 su asta
    d.line([18 * u, 14 * u, 18 * u, 86 * u], fill=c, width=int(6 * u))
    x0, y0, cw, chh = 22, 16, 15, 13
    for r in range(3):
        for k in range(4):
            if (r + k) % 2 == 0:
                d.rectangle([(x0 + k * cw) * u, (y0 + r * chh) * u, (x0 + (k + 1) * cw) * u, (y0 + (r + 1) * chh) * u], fill=c)
    d.rectangle([x0 * u, y0 * u, (x0 + 4 * cw) * u, (y0 + 3 * chh) * u], outline=c, width=int(3 * u))
    return _done(im)


def _ico(fn):
    im, d = _canvas(); u = S * K / 96.0
    fn(d, u, _rgba("#FFD0D0D0"))
    return _done(im)


def ico_track(d, u, c):
    w = int(6 * u)
    d.line([30 * u, 86 * u, 42 * u, 10 * u], fill=c, width=w)
    d.line([66 * u, 86 * u, 54 * u, 10 * u], fill=c, width=w)
    for y0, y1 in ((76, 64), (54, 44), (34, 26), (17, 11)):
        d.line([48 * u, y0 * u, 48 * u, y1 * u], fill=c, width=int(5 * u))


def ico_air(d, u, c):
    w = int(7 * u)
    for yy in (28, 50, 72):
        pts = [((10 + i * 2) * u, (yy + 6 * math.sin(i * 2 * math.pi / 19.0)) * u) for i in range(39)]
        d.line(pts, fill=c, width=w, joint="curve")


def ico_wet(d, u, c):
    d.polygon([(48 * u, 8 * u), (30 * u, 40 * u), (66 * u, 40 * u)], fill=c)
    d.ellipse([28 * u, 26 * u, 68 * u, 66 * u], fill=c)
    d.line([12 * u, 84 * u, 84 * u, 84 * u], fill=c, width=int(6 * u))


def ico_rain(d, u, c):
    d.ellipse([14 * u, 26 * u, 46 * u, 58 * u], fill=c)
    d.ellipse([32 * u, 12 * u, 72 * u, 52 * u], fill=c)
    d.ellipse([56 * u, 30 * u, 84 * u, 58 * u], fill=c)
    d.rectangle([30 * u, 40 * u, 70 * u, 58 * u], fill=c)
    for x in (30, 48, 66):
        d.line([x * u, 68 * u, (x - 6) * u, 86 * u], fill=c, width=int(6 * u))


def ico_water(d, u, c):
    w = int(6 * u)
    d.rounded_rectangle([42 * u, 8 * u, 54 * u, 56 * u], radius=6 * u, outline=c, width=w)
    d.ellipse([38 * u, 50 * u, 58 * u, 70 * u], fill=c)
    for yy in (18, 30, 42):
        d.line([56 * u, yy * u, 66 * u, yy * u], fill=c, width=int(4 * u))
    for yy in (78, 90):
        pts = [((10 + i * 2) * u, (yy + 4 * math.sin(i * 2 * math.pi / 13.0)) * u) for i in range(39)]
        d.line(pts, fill=c, width=int(5 * u), joint="curve")


def ico_oil(d, u, c):
    w = int(6 * u)
    d.polygon([(22 * u, 44 * u), (66 * u, 44 * u), (82 * u, 34 * u), (88 * u, 40 * u), (70 * u, 70 * u), (22 * u, 70 * u)], outline=c, width=w)
    d.line([8 * u, 40 * u, 22 * u, 50 * u], fill=c, width=w)
    d.line([8 * u, 40 * u, 8 * u, 52 * u], fill=c, width=w)
    d.line([38 * u, 44 * u, 38 * u, 34 * u], fill=c, width=w)
    d.line([30 * u, 34 * u, 46 * u, 34 * u], fill=c, width=w)
    d.polygon([(88 * u, 58 * u), (84 * u, 68 * u), (92 * u, 68 * u)], fill=c)
    d.ellipse([84 * u, 64 * u, 92 * u, 72 * u], fill=c)


ON = {"luci": "#FF64B5F6", "tergi": "#FF26C6DA", "motore": "#FF00E676", "motore-acc": "#FFFFD600"}


_CACHE = {}


# ---- cielo (previsioni meteo): 0 sereno, 1 poco nuvoloso, 2 nuvoloso, 3 pioggia, 4 temporale
SUN, CLOUD, DROP, BOLT = "#FFFFC400", "#FFCFD8DC", "#FF42A5F5", "#FFFFEB3B"


def _sun(d, u, cx, cy, r):
    c = _rgba(SUN)
    d.ellipse([(cx - r) * u, (cy - r) * u, (cx + r) * u, (cy + r) * u], fill=c)
    import math
    for k in range(8):
        a = k * math.pi / 4
        d.line([(cx + math.cos(a) * (r + 6)) * u, (cy + math.sin(a) * (r + 6)) * u,
                (cx + math.cos(a) * (r + 16)) * u, (cy + math.sin(a) * (r + 16)) * u], fill=c, width=int(6 * u))


def _cloud(d, u, ox, oy, col=CLOUD):
    c = _rgba(col)
    d.ellipse([(ox + 0) * u, (oy + 20) * u, (ox + 34) * u, (oy + 54) * u], fill=c)
    d.ellipse([(ox + 18) * u, (oy + 4) * u, (ox + 60) * u, (oy + 46) * u], fill=c)
    d.ellipse([(ox + 44) * u, (oy + 20) * u, (ox + 76) * u, (oy + 54) * u], fill=c)
    d.rectangle([(ox + 16) * u, (oy + 34) * u, (ox + 60) * u, (oy + 54) * u], fill=c)


def sky(kind):
    im, d = _canvas(); u = S * K / 96.0
    if kind == 0:
        _sun(d, u, 48, 48, 20)
    elif kind == 1:
        _sun(d, u, 34, 32, 15)
        _cloud(d, u, 16, 34)
    elif kind == 2:
        _cloud(d, u, 10, 14, "#FFB0BEC5")
        _cloud(d, u, 18, 30)
    else:
        _cloud(d, u, 10, 6, "#FF90A4AE" if kind == 4 else CLOUD)
        if kind == 3:
            for x in (28, 46, 64):
                d.line([x * u, 70 * u, (x - 7) * u, 90 * u], fill=_rgba(DROP), width=int(7 * u))
        else:
            d.polygon([(52 * u, 58 * u), (36 * u, 78 * u), (48 * u, 78 * u), (40 * u, 94 * u), (62 * u, 70 * u), (50 * u, 70 * u), (58 * u, 58 * u)],
                      fill=_rgba(BOLT))
    return _done(im)


def arrow(up):
    im, d = _canvas(); u = S * K / 96.0
    c = _rgba("#FF00E676" if up else "#FFFF1744")
    if up:
        d.polygon([(48 * u, 10 * u), (88 * u, 56 * u), (64 * u, 56 * u), (64 * u, 88 * u), (32 * u, 88 * u), (32 * u, 56 * u), (8 * u, 56 * u)], fill=c)
    else:
        d.polygon([(48 * u, 86 * u), (88 * u, 40 * u), (64 * u, 40 * u), (64 * u, 8 * u), (32 * u, 8 * u), (32 * u, 40 * u), (8 * u, 40 * u)], fill=c)
    return _done(im)


def change_arrow(up):
    """Freccia del cambio valore (come le dash da gara): freccia + 3 trattini (verde su, rossa giu')."""
    im, d = _canvas(); u = S * K / 96.0
    c = _rgba("#FF00E676" if up else "#FFFF1744")
    if up:
        d.polygon([(48 * u, 4 * u), (84 * u, 40 * u), (62 * u, 40 * u), (62 * u, 56 * u), (34 * u, 56 * u), (34 * u, 40 * u), (12 * u, 40 * u)], fill=c)
        bars = (62, 74, 86)
    else:
        d.polygon([(48 * u, 92 * u), (84 * u, 56 * u), (62 * u, 56 * u), (62 * u, 40 * u), (34 * u, 40 * u), (34 * u, 56 * u), (12 * u, 56 * u)], fill=c)
        bars = (6, 18, 30)
    for y in bars:
        d.rectangle([34 * u, y * u, 62 * u, (y + 6) * u], fill=c)
    return _done(im)


def all_icons():
    """nome -> PNG"""
    if not _CACHE:
        _CACHE.update(_build())
        _CACHE.update(car_icons())
    return _CACHE


def _build():
    return {
        "dt-luci-off": lights(GREY), "dt-luci-on": lights(ON["luci"]),
        "dt-tergi-off": wipers(GREY), "dt-tergi-on": wipers(ON["tergi"]),
        "dt-motore-off": engine(GREY), "dt-motore-acc": engine(ON["motore-acc"]), "dt-motore-on": engine(ON["motore"]),
        "dt-ico-pista": _ico(ico_track), "dt-ico-aria": _ico(ico_air), "dt-ico-bagnato": _ico(ico_wet),
        "dt-ico-pioggia": _ico(ico_rain), "dt-ico-acqua": _ico(ico_water), "dt-ico-olio": _ico(ico_oil),
        "dt-bandiera-off": flag(GREY), "dt-bandiera-nera": flag("#FF000000"), "dt-bandiera-bianca": flag("#FFFFFFFF"),
        "dt-freccia-su": arrow(True), "dt-freccia-giu": arrow(False),
        "dt-cambio-su": change_arrow(True), "dt-cambio-giu": change_arrow(False),
        "dt-cielo-0": sky(0), "dt-cielo-1": sky(1), "dt-cielo-2": sky(2), "dt-cielo-3": sky(3), "dt-cielo-4": sky(4),
    }


if __name__ == "__main__":
    ic = all_icons()
    sheet = Image.new("RGBA", (S * len(ic), S), (30, 30, 30, 255))
    for i, (n, png) in enumerate(ic.items()):
        sheet.alpha_composite(Image.open(io.BytesIO(png)), (i * S, 0))
    sheet.save("preview/icone.png")
    print("ok")


# ------------------------------------------------------------------ schema danni (auto vista dall'alto)
CAR_W, CAR_H = 120, 240
DENT_COLORS = ["#FF5A5A5A", "#FFFFD600", "#FFFF8C00", "#FFFF1744"]
# meta' sinistra del profilo (unita' 100 x 200, muso in alto) e zona danno del tratto che parte da quel punto
_LEFT = [((50, 3), 1), ((42, 4), 1), ((34, 7), 2), ((26, 13), 2), ((19, 22), 2), ((15, 33), 2), ((16, 46), 3), ((22, 60), 3),
         ((25, 76), 3), ((25, 96), 3), ((22, 112), 3), ((16, 124), 4), ((12, 138), 4), ((11, 154), 4), ((13, 170), 4),
         ((17, 183), 4), ((25, 191), 5), ((37, 195), 5)]
_MIRROR_ZONE = {1: 1, 2: 8, 3: 7, 4: 6, 5: 5}


def _outline():
    """Profilo chiuso [(x, y, zona)] in senso orario partendo dal muso."""
    left = _LEFT
    right = [((100 - x, y), _MIRROR_ZONE[z]) for (x, y), z in left]
    # muso -> lato destro -> coda -> lato sinistro (al contrario)
    pts = [(p, z) for p, z in right] + [((50, 196), 5)]
    rev = list(reversed(left))
    for i, (p, z) in enumerate(rev):
        # la zona di un tratto che va da rev[i] verso rev[i+1] e' quella del punto rev[i+1] nel verso originale
        nz = rev[i + 1][1] if i + 1 < len(rev) else 1
        pts.append((p, nz if i + 1 < len(rev) else 1))
    return pts


def _spline(pts, steps=10):
    """Catmull-Rom chiusa: [(x, y, zona del tratto)]"""
    n = len(pts)
    out = []
    for i in range(n):
        p0, p1, p2, p3 = (pts[(i + k) % n][0] for k in (-1, 0, 1, 2))
        z = pts[i][1]
        for s in range(steps):
            t = s / float(steps)
            t2, t3 = t * t, t * t * t
            x = 0.5 * ((2 * p1[0]) + (-p0[0] + p2[0]) * t + (2 * p0[0] - 5 * p1[0] + 4 * p2[0] - p3[0]) * t2 + (-p0[0] + 3 * p1[0] - 3 * p2[0] + p3[0]) * t3)
            y = 0.5 * ((2 * p1[1]) + (-p0[1] + p2[1]) * t + (2 * p0[1] - 5 * p1[1] + 4 * p2[1] - p3[1]) * t2 + (-p0[1] + 3 * p1[1] - 3 * p2[1] + p3[1]) * t3)
            out.append((x, y, z))
    return out


def _car_canvas():
    im = Image.new("RGBA", (CAR_W * K, CAR_H * K), (0, 0, 0, 0))
    return im, ImageDraw.Draw(im), CAR_W * K / 100.0


def _car_done(im):
    im = im.resize((CAR_W, CAR_H), Image.LANCZOS)
    b = io.BytesIO()
    im.save(b, "PNG", optimize=True)
    return b.getvalue()


def car_base():
    im, d, u = _car_canvas()
    sp = _spline(_outline())
    d.polygon([(x * u, y * u) for x, y, _ in sp], fill=_rgba("#FF2A2A2A"))
    g = _rgba("#FF6A6A6A"); w = int(2.2 * u)
    # parabrezza, abitacolo, lunotto e linee del cofano
    d.polygon([(33 * u, 70 * u), (67 * u, 70 * u), (63 * u, 84 * u), (37 * u, 84 * u)], outline=g, width=w)
    d.rounded_rectangle([36 * u, 86 * u, 64 * u, 122 * u], radius=8 * u, outline=g, width=w)
    d.polygon([(37 * u, 124 * u), (63 * u, 124 * u), (66 * u, 136 * u), (34 * u, 136 * u)], outline=g, width=w)
    d.line([40 * u, 20 * u, 36 * u, 62 * u], fill=g, width=w)
    d.line([60 * u, 20 * u, 64 * u, 62 * u], fill=g, width=w)
    d.line([30 * u, 186 * u, 70 * u, 186 * u], fill=g, width=w)
    return _car_done(im)


def car_zone(zone, color):
    im, d, u = _car_canvas()
    sp = _spline(_outline())
    c = _rgba(color); w = int(5 * u)
    n = len(sp)
    for i in range(n):
        x1, y1, z = sp[i]
        x2, y2, _ = sp[(i + 1) % n]
        if z == zone:
            d.line([x1 * u, y1 * u, x2 * u, y2 * u], fill=c, width=w)
            r = w / 2.0
            d.ellipse([x1 * u - r, y1 * u - r, x1 * u + r, y1 * u + r], fill=c)
    return _car_done(im)


def car_icons():
    out = {"dt-auto-base": car_base()}
    for z in range(1, 9):
        for sev, c in enumerate(DENT_COLORS):
            out["dt-dz%d-%d" % (z, sev)] = car_zone(z, c)
    return out
