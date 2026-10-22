using System;
using System.Globalization;

namespace FedeDashboard
{
    /// <summary>Trasforma qualunque valore SimHub (numero, TimeSpan, bool, testo) nel testo da mostrare + valore numerico.</summary>
    public static class ValueFormatter
    {
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        public const string Empty = "-";

        public static string Format(object v, string format, int decimals, string unit, out double number)
        {
            number = 0;
            if (v == null) return Empty;
            if (decimals < 0) decimals = 0;
            if (decimals > 6) decimals = 6;
            unit = unit ?? "";

            if (v is TimeSpan)
            {
                var ts = (TimeSpan)v;
                number = ts.TotalSeconds;
                return FormatSeconds(number, format, decimals, unit);
            }
            if (v is bool)
            {
                bool b = (bool)v;
                number = b ? 1 : 0;
                return b ? "ON" : "OFF";
            }
            if (v is DateTime)
            {
                var dt = (DateTime)v;
                number = dt.TimeOfDay.TotalSeconds;
                return dt.ToString("HH:mm", Inv);
            }

            var s = v as string;
            if (s != null)
            {
                double parsed;
                bool isNum = double.TryParse(s, NumberStyles.Float, Inv, out parsed);
                if (isNum) number = parsed;
                if (format == "text" || !isNum) return s.Length == 0 ? Empty : s;
                return FormatSeconds(parsed, format, decimals, unit);
            }

            if (v is IConvertible)
            {
                try { number = Convert.ToDouble(v, Inv); }
                catch { return v.ToString(); }
                return FormatSeconds(number, format, decimals, unit);
            }
            return v.ToString();
        }

        /// <summary>Formatta un numero secondo il formato del canale (per i tempi il numero e' in secondi).</summary>
        static string FormatSeconds(double n, string format, int decimals, string unit)
        {
            if (double.IsNaN(n) || double.IsInfinity(n)) return Empty;
            switch (format)
            {
                case "laptime":
                    if (n <= 0) return "-:--.---";
                    return LapTime(n);
                case "duration":
                    if (n <= 0) return "--:--";
                    return Duration(n);
                case "clock":
                    return DateTime.Today.AddSeconds(n).ToString("HH:mm", Inv);
                case "gap":
                    return Math.Abs(n).ToString("F" + decimals, Inv) + unit;
                case "signed":
                    return (n > 0 ? "+" : "") + n.ToString("F" + decimals, Inv) + unit;
                case "text":
                    return (Math.Abs(n - Math.Round(n)) < 1e-9 ? Math.Round(n).ToString("0", Inv) : n.ToString("F" + decimals, Inv)) + unit;
                default:
                    return n.ToString("F" + decimals, Inv) + unit;
            }
        }

        public static string LapTime(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            int minutes = (int)ts.TotalMinutes;
            return minutes.ToString(Inv) + ":" + ts.Seconds.ToString("00", Inv) + "." + ts.Milliseconds.ToString("000", Inv);
        }

        public static string Duration(double seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.TotalHours >= 1)
                return ((int)ts.TotalHours).ToString(Inv) + ":" + ts.Minutes.ToString("00", Inv) + ":" + ts.Seconds.ToString("00", Inv);
            return ts.Minutes.ToString("00", Inv) + ":" + ts.Seconds.ToString("00", Inv);
        }
    }
}
