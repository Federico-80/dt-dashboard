# -*- coding: utf-8 -*-
"""Genera Catalog.generated.cs per il DT Plugin dallo stesso catalogo della dashboard."""
import os, sys
from catalog import *

def s(v):
    return '"' + v.replace("\\", "\\\\").replace('"', '\\"') + '"'

def arr(vals, fn=str):
    return "{ " + ", ".join(fn(v) for v in vals) + " }"

def rects(lst):
    return "new int[][] { " + ", ".join("new[] { %d, %d, %d, %d }" % tuple(r) for r in lst) + " }"

L = []
w = L.append
w("// FILE GENERATO AUTOMATICAMENTE da gen_cs.py - non modificare a mano.")
w("// Deve restare allineato alla dashboard: gli indici delle schede = indici delle schermate nei file Schede_*.djson")
w("using System.Collections.Generic;")
w("")
w("namespace FedeDashboard")
w("{")
w("    public static class Catalog")
w("    {")
w("        public const int MaxSideSlots = %d;" % MAX_SIDE_SLOTS)
w("        public const int MaxToolSlots = %d;" % MAX_TOOL_SLOTS)
w("        public const int CenterRows = %d;" % CENTER_ROWS)
w("        public const int StandingsMax = %d;" % STANDINGS_MAX)
w("        public const int DashWidth = %d;" % DASH_W)
w("        public const int DashHeight = %d;" % DASH_H)
w("")
w("        public static readonly ChannelDef[] Channels = new ChannelDef[]")
w("        {")
for (cid, label, src, fmt, dec, unit, chars, sample) in CHANNELS:
    w("            new ChannelDef(%s, %s, %s, %s, %d, %s)," % (s(cid), s(label), s(src), s(fmt), dec, s(unit)))
w("        };")
w("")
w("        public static readonly string[] Cards = new string[] " + arr([c["name"] for c in CARDS], s) + ";")
w("        public static readonly string[] Bars = new string[] " + arr([b[0] for b in BARS], s) + ";")
w("        public static readonly string[] Frames = new string[] " + arr([f[0] for f in FRAMES], s) + ";")
w("        public static readonly bool[] FrameHasSidebars = new bool[] " + arr([f[1] for f in FRAMES], lambda b: "true" if b else "false") + ";")
w("        public static readonly bool[] FrameHasToolbar = new bool[] " + arr([f[2] for f in FRAMES], lambda b: "true" if b else "false") + ";")
w("        public static readonly string[] Grids = new string[] " + arr([g[0] for g in GRIDS], s) + ";")
w("        public static readonly string[] Categories = new string[] " + arr([c[0] for c in CATEGORIES], s) + ";")
w("        // CategoryCards[categoria] = indici delle schede")
w("        public static readonly int[][] CategoryCards = new int[][] { " + ", ".join("new[] { " + ", ".join(str(CARD_INDEX[n]) for n in ns) + " }" for _, ns in CATEGORIES) + " };")
w("        // GridUnits[griglia] = celle in unita' 2 colonne x 4 righe {col, riga, larghezza, altezza}")
w("        public static readonly int[][][] GridUnits = new int[][][] { " + ", ".join(rects(g[1]) for g in GRIDS) + " };")
w("        public static readonly string[] Toolbars = new string[] " + arr([t[0] for t in TOOLBARS], s) + ";")
w("")
w("        // Geometria (pixel della dashboard 850x480) per l'anteprima nelle impostazioni")
w("        // GridRects[cornice][griglia][lato 0=sx 1=dx] -> rettangoli {x, y, w, h}")
w("        public static readonly int[][][][][] GridRects = new int[][][][][]")
w("        {")
for fi in range(len(FRAMES)):
    g = frame_geom(fi)
    w("            new int[][][][] {")
    for gi in range(len(GRIDS)):
        l = grid_cells(g["left"][0], g["left"][1], g["top"], g["bottom"], gi, g["row_h"])
        r = grid_cells(g["right"][0], g["right"][1], g["top"], g["bottom"], gi, g["row_h"])
        w("                new int[][][] { %s, %s }," % (rects(l), rects(r)))
    w("            },")
w("        };")
w("        // ToolbarRects[cornice][modello] (vuoto se la cornice non ha barra inferiore)")
w("        public static readonly int[][][][] ToolbarRects = new int[][][][]")
w("        {")
for fi in range(len(FRAMES)):
    if FRAMES[fi][2]:
        w("            new int[][][] { " + ", ".join(rects(toolbar_cells(fi, ti)) for ti in range(len(TOOLBARS))) + " },")
    else:
        w("            new int[][][] { " + ", ".join("new int[0][]" for _ in TOOLBARS) + " },")
w("        };")
w("        // SidebarRects[cornice] (vuoto se senza barre laterali)")
w("        public static readonly int[][][] SidebarRects = new int[][][]")
w("        {")
for fi in range(len(FRAMES)):
    w("            " + (rects(sidebar_cells(fi)) if FRAMES[fi][1] else "new int[0][]") + ",")
w("        };")
w("        // CenterRects[cornice] = { delta, marcia, velocita', riga1, riga2, riga3 }")
w("        public static readonly int[][][] CenterRects = new int[][][]")
w("        {")
for fi in range(len(FRAMES)):
    c = center_geom(fi)
    w("            " + rects([c["delta"], c["gear"], c["speed"]] + c["rows"]) + ",")
w("        };")
w("")
w("        public static List<PageSettings> DefaultPages()")
w("        {")
w("            var list = new List<PageSettings>();")
for p in DEFAULT_PAGES:
    def il(v, n):
        v = list(v) + [0] * (n - len(v))
        return "new List<int> " + arr(v)
    w("            list.Add(new PageSettings { Name = %s, Frame = %d, GridLeft = %d, GridRight = %d, Toolbar = %d, SidebarLeft = %d, SidebarRight = %d," % (
        s(p["name"]), p["frame"], p["grid_l"], p["grid_r"], p["toolbar"], p["SL"], p["SR"]))
    w("                Left = %s, Right = %s, Bottom = %s, Center = %s });" % (
        il(p["L"], MAX_SIDE_SLOTS), il(p["R"], MAX_SIDE_SLOTS), il(p["T"], MAX_TOOL_SLOTS), il(p["C"], CENTER_ROWS)))
w("            return list;")
w("        }")
w("    }")
w("")
w("    public class ChannelDef")
w("    {")
w("        public readonly string Id, Label, Source, Format, Unit;")
w("        public readonly int Decimals;")
w("        public ChannelDef(string id, string label, string source, string format, int decimals, string unit)")
w("        {")
w("            Id = id; Label = label; Source = source; Format = format; Decimals = decimals; Unit = unit;")
w("        }")
w("    }")
w("}")

out = sys.argv[1]
with open(out, "w", encoding="utf-8-sig", newline="\r\n") as f:
    f.write("\n".join(L) + "\n")
print("scritto", out)
