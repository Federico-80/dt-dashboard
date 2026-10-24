using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FedeDashboard
{
    /// <summary>
    /// Anteprima vera nella scheda "Pagine e layout": disegna la pagina leggendo gli stessi file della dashboard
    /// (pezzi del centro + schede), con i valori d'esempio scritti dal generatore nei campi fissi.
    /// I file letti restano in memoria; si ridisegna solo quando cambia qualcosa.
    /// </summary>
    public class PreviewRenderer
    {
        class Doc { public DateTime Stamp; public List<object> Screens; }

        readonly Dictionary<string, Doc> docs = new Dictionary<string, Doc>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, object> images = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, object> brushes = new Dictionary<string, object>();
        Dictionary<string, List<object>> pieces;
        DateTime piecesStamp;
        string piecesFontKey = "";

        Type tBorder, tGrid, tText, tImage, tCanvas, tCorner, tThick, tTextAlign, tVAlign, tHAlign, tFontFamily, tStretch;
        object weightConv, brushConv;

        static string Folder { get { return DashBuilder.DashFolder; } }

        void InitTypes()
        {
            if (tBorder != null) return;
            tBorder = Wpf.T("System.Windows.Controls.Border");
            tGrid = Wpf.T("System.Windows.Controls.Grid");
            tText = Wpf.T("System.Windows.Controls.TextBlock");
            tImage = Wpf.T("System.Windows.Controls.Image");
            tCanvas = Wpf.T("System.Windows.Controls.Canvas");
            tCorner = Wpf.T("System.Windows.CornerRadius");
            tThick = Wpf.T("System.Windows.Thickness");
            tTextAlign = Wpf.T("System.Windows.TextAlignment");
            tVAlign = Wpf.T("System.Windows.VerticalAlignment");
            tHAlign = Wpf.T("System.Windows.HorizontalAlignment");
            tFontFamily = Wpf.T("System.Windows.Media.FontFamily");
            tStretch = Wpf.T("System.Windows.Media.Stretch");
            weightConv = Activator.CreateInstance(Wpf.T("System.Windows.FontWeightConverter"));
            brushConv = Activator.CreateInstance(Wpf.T("System.Windows.Media.BrushConverter"));
        }

        // anteprima avvisi: livelli forzati, testi, colori e sfondi sostituiti (per nome dell'elemento)
        readonly Dictionary<string, bool> layerVis = new Dictionary<string, bool>();
        readonly Dictionary<string, string> textOver = new Dictionary<string, string>();
        readonly Dictionary<string, string> colorOver = new Dictionary<string, string>();
        readonly Dictionary<string, string> bgOver = new Dictionary<string, string>();
        List<object> alertTree;
        DateTime alertStamp;

        readonly HashSet<string> hidden = new HashSet<string>();
        void ClearOverrides() { layerVis.Clear(); textOver.Clear(); colorOver.Clear(); bgOver.Clear(); hidden.Clear(); }

        static Dictionary<string, object> FindByName(List<object> items, string name, int depth = 0)
        {
            if (items == null || depth > 8) return null;
            foreach (var o in items)
            {
                var d = o as Dictionary<string, object>;
                if (d == null) continue;
                if (Str(d, "Name") == name) return d;
                var r = FindByName(Get(d, "Childrens") as List<object>, name, depth + 1);
                if (r != null) return r;
            }
            return null;
        }

        static string Sample(string id)
        {
            foreach (var c in Catalog.Channels) if (c.Id == id) return string.IsNullOrEmpty(c.Sample) ? "-" : c.Sample;
            return "-";
        }

        /// <summary>Anteprima di un avviso sopra la pagina, con le impostazioni attuali (colori, titolo, valori, grandezza, posizione).</summary>
        public bool DrawAlert(object canvas, PageSettings p, FontSettings font, AlertSettings a, Func<string, string> label)
        {
            InitTypes();
            ClearOverrides();
            try
            {
                bool flagTile = a.Id == "Flag" && !a.FlagAsPopup;
                if (flagTile) { layerVis["Bandiera"] = true; textOver["Bandiera settore"] = "SETTORE 2"; }
                if (!Draw(canvas, p, font)) return false;
                if (flagTile) return true;
                string path = Path.Combine(Folder, DashBuilder.DashName + ".djson.modello");
                if (!File.Exists(path)) return true;
                var stamp = File.GetLastWriteTimeUtc(path);
                if (alertTree == null || stamp != alertStamp)
                {
                    var root = MiniJson.Parse(File.ReadAllText(path, Encoding.UTF8).TrimStart('\uFEFF')) as Dictionary<string, object>;
                    var sc = root != null ? Get(root, "Screens") as List<object> : null;
                    var all = new List<object>();
                    if (sc != null) foreach (var x in sc) { var sd = x as Dictionary<string, object>; if (sd != null && Get(sd, "Items") is List<object>) all.AddRange((List<object>)Get(sd, "Items")); }
                    alertTree = all; alertStamp = stamp;
                }
                string mainFile = Path.Combine(Folder, DashBuilder.DashName + ".djson");
                if (a.Id == "Pit" && !a.SimpleLayout)
                {
                    var box = FindByName(alertTree, "Schermata box");
                    if (box != null)
                    {
                        bgOver["Box sfondo"] = WithOpacity(a.Background, a.Opacity);
                        DrawItems(canvas, Get(box, "Childrens") as List<object>, 0, 0, p, mainFile, 0);
                    }
                    return true;
                }
                var v = FindByName(alertTree, "Variante " + a.Variant);
                if (v == null) return true;
                string fg = DTPlugin.NormalizeColor(a.TextColor, "#FFFFFFFF");
                string bg = WithOpacity(a.Background, a.Opacity);
                if (a.Id == "Flag" && a.UseFlagColor) { bg = "#FFFFD600"; fg = "#FF000000"; }
                bgOver["Avviso sfondo"] = bg;
                for (int n = 1; n <= 4; n++) layerVis["Valori " + n] = false;
                layerVis["Valore solo"] = false;
                layerVis["Schema danni"] = a.Id == "Damage";
                string title = a.Title ?? "";
                var ids = new List<string>();
                foreach (var c in a.Channels) if (ids.Count < 4 && !string.IsNullOrEmpty(c)) ids.Add(c);
                if (a.Id == "Change")
                {
                    string id = ids.Count > 0 ? ids[0] : "BB";
                    title = label(id);
                    layerVis["Valore solo"] = true;
                    textOver["Avviso valore solo"] = Sample(id);
                    colorOver["Avviso titolo"] = "#FF8EC5FF";
                    colorOver["Avviso valore solo"] = fg;
                }
                else if (a.Id == "Flag")
                {
                    // come in pista: titolo dell'avviso e, grande, il nome della bandiera
                    layerVis["Valore solo"] = true;
                    textOver["Avviso valore solo"] = "GIALLA";
                    colorOver["Avviso valore solo"] = fg;
                    hidden.Add("Avviso freccia sinistra"); hidden.Add("Avviso freccia destra");
                }
                else if (a.Id != "Damage" && ids.Count > 0)
                {
                    layerVis["Valori " + ids.Count] = true;
                    for (int i = 0; i < ids.Count; i++)
                    {
                        textOver["Avviso valore " + (i + 1)] = Sample(ids[i]);
                        textOver["Avviso nome " + (i + 1)] = label(ids[i]);
                        colorOver["Avviso valore " + (i + 1)] = fg;
                        colorOver["Avviso nome " + (i + 1)] = fg;
                    }
                }
                textOver["Avviso titolo"] = title;
                if (!colorOver.ContainsKey("Avviso titolo")) colorOver["Avviso titolo"] = fg;
                DrawItems(canvas, Get(v, "Childrens") as List<object>, 0, 0, p, mainFile, 0);
                return true;
            }
            finally { ClearOverrides(); }
        }

        static string WithOpacity(string c, int opacity)
        {
            c = DTPlugin.NormalizeColor(c, "#FF000000");
            int al = (int)Math.Round(Math.Max(0, Math.Min(100, opacity)) * 2.55);
            return "#" + al.ToString("X2") + c.Substring(3);
        }

        /// <summary>Disegna la pagina nel Canvas (850x480). false se i file della dash non ci sono.</summary>
        public bool Draw(object canvas, PageSettings p, FontSettings font)
        {
            InitTypes();
            if (!LoadPieces(font)) return false;
            int f = p.EffectiveFrame;
            var keys = new[] { "CENTRO_" + f + "_" + p.CenterLayout, "GRIGLIA_" + f + "_L_" + p.GridLeft, "GRIGLIA_" + f + "_R_" + p.GridRight,
                               "BARRA_" + f + "_" + p.Toolbar, "LATERALI_" + f };
            string mainFile = Path.Combine(Folder, DashBuilder.DashName + ".djson");
            foreach (var k in keys)
            {
                List<object> items;
                if (pieces.TryGetValue(k, out items)) DrawItems(canvas, items, 0, 0, p, mainFile, 0);
            }
            return true;
        }

        bool LoadPieces(FontSettings font)
        {
            string path = Path.Combine(Folder, DashBuilder.DashName + ".pezzi");
            if (!File.Exists(path)) return false;
            var stamp = File.GetLastWriteTimeUtc(path);
            string fk = font != null ? font.Key : "";
            if (pieces != null && stamp == piecesStamp && fk == piecesFontKey) return true;
            var d = new Dictionary<string, List<object>>();
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
            {
                int t = line.IndexOf('\t');
                if (t <= 0) continue;
                string key = line.Substring(0, t);
                if (!(key.StartsWith("CENTRO_") || key.StartsWith("GRIGLIA_") || key.StartsWith("BARRA_") || key.StartsWith("LATERALI_"))) continue;
                string json = line.Substring(t + 1);
                // stesso carattere / bordi della dash (le schede sui file sono gia' riscritte dal plugin)
                try { if (font != null) json = DashBuilder.ApplyStyle(json, font, true, false); } catch { }
                var arr = MiniJson.Parse(json) as List<object>;
                if (arr != null) d[key] = arr;
            }
            pieces = d; piecesStamp = stamp; piecesFontKey = fk;
            return true;
        }

        List<object> Screens(string file)
        {
            string path = Path.Combine(Folder, file);
            if (!File.Exists(path)) return null;
            var stamp = File.GetLastWriteTimeUtc(path);
            Doc doc;
            if (docs.TryGetValue(path, out doc) && doc.Stamp == stamp) return doc.Screens;
            var root = MiniJson.Parse(File.ReadAllText(path, Encoding.UTF8).TrimStart('﻿')) as Dictionary<string, object>;
            var sc = root != null ? Get(root, "Screens") as List<object> : null;
            docs[path] = new Doc { Stamp = stamp, Screens = sc };
            // le immagini di quel file potrebbero essere cambiate
            var gone = new List<string>();
            foreach (var k in images.Keys) if (k.StartsWith(path + "|", StringComparison.OrdinalIgnoreCase)) gone.Add(k);
            foreach (var k in gone) images.Remove(k);
            return sc;
        }

        static object Get(Dictionary<string, object> d, string k) { object v; return d.TryGetValue(k, out v) ? v : null; }
        static double Num(Dictionary<string, object> d, string k, double def = 0)
        {
            object v = Get(d, k);
            if (v is double) return (double)v;
            double r;
            return v != null && double.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture, out r) ? r : def;
        }
        static string Str(Dictionary<string, object> d, string k) { return Convert.ToString(Get(d, k) ?? "", CultureInfo.InvariantCulture); }

        void DrawItems(object canvas, List<object> items, double ox, double oy, PageSettings p, string file, int depth)
        {
            if (items == null || depth > 6) return;
            foreach (var o in items)
            {
                var it = o as Dictionary<string, object>;
                if (it == null) continue;
                string type = Str(it, "$type");
                string nm = Str(it, "Name");
                // riquadro delta: quello predefinito o la scheda scelta, secondo la pagina
                bool forced;
                if (hidden.Contains(nm)) continue;
                if (layerVis.TryGetValue(nm, out forced))
                {
                    if (!forced) continue;     // anteprima avvisi: livelli accesi/spenti a mano
                }
                else if (nm == "Delta predefinito" || nm == "Delta scelto")
                {
                    if ((nm == "Delta predefinito") != (p.CenterTop == 0)) continue;
                }
                else if (nm == "Barra giri")
                {
                    if (p.ShowInputs || p.HideSideBars) continue;     // opzioni della pagina: barre ai lati della marcia
                }
                else if (nm == "Barre acceleratore e freno")
                {
                    if (!p.ShowInputs || p.HideSideBars) continue;
                }
                else if (Get(it, "Visible") is bool && !(bool)Get(it, "Visible")) continue;
                try
                {
                    if (type.Contains(".Layer,")) DrawItems(canvas, Get(it, "Childrens") as List<object>, ox, oy, p, file, depth + 1);
                    else if (type.Contains(".WidgetItem,")) DrawWidget(canvas, it, ox, oy, p, depth);
                    else if (type.Contains(".RectangleItem,")) Add(canvas, MakeRect(it), ox + Num(it, "Left"), oy + Num(it, "Top"));
                    else if (type.Contains(".TextItem,")) Add(canvas, MakeText(it), ox + Num(it, "Left"), oy + Num(it, "Top"));
                    else if (type.Contains(".ImageItem,"))
                    {
                        var img = MakeImage(it, file);
                        if (img != null) Add(canvas, img, ox + Num(it, "Left"), oy + Num(it, "Top"));
                    }
                }
                catch { }
            }
        }

        void DrawWidget(object canvas, Dictionary<string, object> it, double ox, double oy, PageSettings p, int depth)
        {
            int screen = SlotOf(Str(it, "Name"), p);
            if (screen < 0) return;
            string fn = Str(it, "FileName");
            var sc = Screens(fn);
            if (sc == null || screen >= sc.Count) return;
            var s = sc[screen] as Dictionary<string, object>;
            if (s == null) return;
            DrawItems(canvas, Get(s, "Items") as List<object>, ox + Num(it, "Left"), oy + Num(it, "Top"), p, Path.Combine(Folder, fn), depth + 1);
        }

        /// <summary>Scheda scelta per il riquadro: "Slot L1", "Slot T3", "Riga centrale 2", "Barra SL".</summary>
        static int SlotOf(string name, PageSettings p)
        {
            try
            {
                if (name.StartsWith("Riga centrale ")) return At(p.Center, int.Parse(name.Substring(14)) - 1);
                if (name == "Barra SL") return p.SidebarLeft;
                if (name == "Riquadro delta") return p.CenterTop;
                if (name == "Barra SR") return p.SidebarRight;
                if (name.StartsWith("Slot "))
                {
                    char k = name[5];
                    int i = int.Parse(name.Substring(6)) - 1;
                    if (k == 'L') return At(p.Left, i);
                    if (k == 'R') return At(p.Right, i);
                    if (k == 'T') return At(p.Bottom, i);
                    if (k == 'C') return At(p.Center, i);
                }
            }
            catch { }
            return -1;
        }

        static int At(List<int> l, int i) { return l != null && i >= 0 && i < l.Count ? l[i] : 0; }

        object Brush(string c)
        {
            if (string.IsNullOrEmpty(c)) return null;
            object b;
            if (brushes.TryGetValue(c, out b)) return b;
            try { b = brushConv.GetType().GetMethod("ConvertFromString", new[] { typeof(string) }).Invoke(brushConv, new object[] { c }); }
            catch { b = null; }
            if (b != null) { try { b.GetType().GetMethod("Freeze").Invoke(b, null); } catch { } }
            brushes[c] = b;
            return b;
        }

        object MakeRect(Dictionary<string, object> it)
        {
            dynamic b = Activator.CreateInstance(tBorder);
            b.Width = Math.Max(0, Num(it, "Width"));
            b.Height = Math.Max(0, Num(it, "Height"));
            string bgs;
            if (!bgOver.TryGetValue(Str(it, "Name"), out bgs)) bgs = Str(it, "BackgroundColor");
            object bg = Brush(bgs);
            if (bg != null) b.Background = (dynamic)bg;
            var bs = Get(it, "BorderStyle") as Dictionary<string, object>;
            if (bs != null)
            {
                b.CornerRadius = (dynamic)Activator.CreateInstance(tCorner, Num(bs, "RadiusTopLeft"), Num(bs, "RadiusTopRight"), Num(bs, "RadiusBottomRight"), Num(bs, "RadiusBottomLeft"));
                double l = Num(bs, "BorderLeft"), t = Num(bs, "BorderTop"), r = Num(bs, "BorderRight"), bo = Num(bs, "BorderBottom");
                if (l + t + r + bo > 0)
                {
                    b.BorderThickness = (dynamic)Activator.CreateInstance(tThick, l, t, r, bo);
                    object bc = Brush(Str(bs, "BorderColor"));
                    if (bc != null) b.BorderBrush = (dynamic)bc;
                }
            }
            b.IsHitTestVisible = false;
            return b;
        }

        object MakeText(Dictionary<string, object> it)
        {
            string txt = Str(it, "Text");
            string ov;
            if (textOver.TryGetValue(Str(it, "Name"), out ov)) txt = ov;
            dynamic g = Activator.CreateInstance(tGrid);
            g.Width = Math.Max(0, Num(it, "Width"));
            g.Height = Math.Max(0, Num(it, "Height"));
            g.IsHitTestVisible = false;
            if (txt.Length == 0) return g;
            dynamic t = Activator.CreateInstance(tText);
            t.Text = txt;
            t.FontSize = Math.Max(4, Num(it, "FontSize", 14));
            string fam = Str(it, "Font");
            try { t.FontFamily = (dynamic)Activator.CreateInstance(tFontFamily, (fam.Length > 0 ? fam + ", " : "") + "Roboto, Segoe UI"); } catch { }
            try { t.FontWeight = (dynamic)weightConv.GetType().GetMethod("ConvertFromString", new[] { typeof(string) }).Invoke(weightConv, new object[] { Str(it, "FontWeight").Length > 0 ? Str(it, "FontWeight") : "Normal" }); } catch { }
            string fgs;
            if (!colorOver.TryGetValue(Str(it, "Name"), out fgs)) fgs = Str(it, "TextColor");
            object fg = Brush(fgs);
            if (fg != null) t.Foreground = (dynamic)fg;
            int ha = (int)Num(it, "HorizontalAlignment", 1), va = (int)Num(it, "VerticalAlignment", 1);
            t.TextAlignment = (dynamic)Enum.Parse(tTextAlign, ha == 0 ? "Left" : (ha == 2 ? "Right" : "Center"));
            t.HorizontalAlignment = (dynamic)Enum.Parse(tHAlign, "Stretch");
            t.VerticalAlignment = (dynamic)Enum.Parse(tVAlign, va == 0 ? "Top" : (va == 2 ? "Bottom" : "Center"));
            t.IsHitTestVisible = false;
            g.Children.Add(t);
            return g;
        }

        object MakeImage(Dictionary<string, object> it, string file)
        {
            string name = Str(it, "Image");
            if (name.Length == 0) return null;
            object src = ImageSource(file, name);
            if (src == null) return null;
            dynamic im = Activator.CreateInstance(tImage);
            im.Width = Math.Max(0, Num(it, "Width"));
            im.Height = Math.Max(0, Num(it, "Height"));
            im.Stretch = (dynamic)Enum.Parse(tStretch, "Fill");
            im.Source = (dynamic)src;
            im.IsHitTestVisible = false;
            return im;
        }

        object ImageSource(string file, string name)
        {
            string key = file + "|" + name;
            object v;
            if (images.TryGetValue(key, out v)) return v;
            v = null;
            try
            {
                string res = file + ".ressources";
                if (File.Exists(res))
                    using (var z = ZipFile.OpenRead(res))
                    {
                        var e = z.GetEntry(name + ".png");
                        if (e != null)
                        {
                            var ms = new MemoryStream();
                            using (var st = e.Open()) st.CopyTo(ms);
                            ms.Position = 0;
                            dynamic bmp = Activator.CreateInstance(Wpf.T("System.Windows.Media.Imaging.BitmapImage"));
                            bmp.BeginInit();
                            bmp.CacheOption = (dynamic)Enum.Parse(Wpf.T("System.Windows.Media.Imaging.BitmapCacheOption"), "OnLoad");
                            bmp.StreamSource = ms;
                            bmp.EndInit();
                            bmp.Freeze();
                            v = bmp;
                        }
                    }
            }
            catch { v = null; }
            images[key] = v;
            return v;
        }

        void Add(object canvas, object el, double x, double y)
        {
            tCanvas.GetMethod("SetLeft").Invoke(null, new object[] { el, x });
            tCanvas.GetMethod("SetTop").Invoke(null, new object[] { el, y });
            ((dynamic)canvas).Children.Add((dynamic)el);
        }
    }
}
