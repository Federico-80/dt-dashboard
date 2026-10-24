using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace FedeDashboard
{
    /// <summary>
    /// Legge i tasti di volanti e button box direttamente da Windows (winmm, fino a 16 periferiche, 32 tasti ciascuna).
    /// Serve per assegnare un tasto dal plugin senza passare da "Controlli ed eventi" di SimHub.
    /// </summary>
    public static class WheelButtons
    {
        [StructLayout(LayoutKind.Sequential)]
        struct JOYINFOEX
        {
            public int dwSize, dwFlags, dwXpos, dwYpos, dwZpos, dwRpos, dwUpos, dwVpos, dwButtons, dwButtonNumber, dwPOV, dwReserved1, dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct JOYCAPS
        {
            public ushort wMid, wPid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szPname;
            public int wXmin, wXmax, wYmin, wYmax, wZmin, wZmax, wNumButtons, wPeriodMin, wPeriodMax,
                       wRmin, wRmax, wUmin, wUmax, wVmin, wVmax, wCaps, wMaxAxes, wNumAxes, wMaxButtons;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szRegKey;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szOEMVxD;
        }

        [DllImport("winmm.dll")] static extern int joyGetPosEx(int uJoyID, ref JOYINFOEX pji);
        [DllImport("winmm.dll", CharSet = CharSet.Auto)] static extern int joyGetDevCaps(IntPtr uJoyID, ref JOYCAPS pjc, int cbjc);

        const int JOY_RETURNBUTTONS = 0x80;

        class Dev { public bool On; public string Key, Name; public int Last; }

        /// <summary>Tasto premuto: chiave periferica (vid:pid), nome periferica, numero tasto (da 1).</summary>
        public static event Action<string, string, int> Pressed;

        static readonly Dev[] devs = new Dev[16];
        static Thread thread;
        static volatile bool running;
        static DateTime nextScan = DateTime.MinValue;

        public static void Start()
        {
            if (running) return;
            running = true;
            thread = new Thread(Loop) { IsBackground = true, Name = "DT tasti volante" };
            thread.Start();
        }

        public static void Stop() { running = false; }

        static void Scan()
        {
            for (int i = 0; i < 16; i++)
            {
                var d = devs[i] ?? (devs[i] = new Dev());
                var ji = new JOYINFOEX { dwSize = Marshal.SizeOf(typeof(JOYINFOEX)), dwFlags = JOY_RETURNBUTTONS };
                bool on = joyGetPosEx(i, ref ji) == 0;
                if (on && !d.On)
                {
                    var c = new JOYCAPS();
                    string name = "Periferica " + (i + 1), key = "id" + i;
                    if (joyGetDevCaps((IntPtr)i, ref c, Marshal.SizeOf(typeof(JOYCAPS))) == 0)
                    {
                        key = c.wMid.ToString("X4") + ":" + c.wPid.ToString("X4");
                        if (!string.IsNullOrWhiteSpace(c.szPname)) name = c.szPname.Trim();
                    }
                    d.Key = key; d.Name = name; d.Last = ji.dwButtons;
                }
                d.On = on;
            }
        }

        static void Loop()
        {
            while (running)
            {
                try
                {
                    var now = DateTime.Now;
                    if (now >= nextScan) { Scan(); nextScan = now.AddSeconds(5); }
                    for (int i = 0; i < 16; i++)
                    {
                        var d = devs[i];
                        if (d == null || !d.On) continue;
                        var ji = new JOYINFOEX { dwSize = Marshal.SizeOf(typeof(JOYINFOEX)), dwFlags = JOY_RETURNBUTTONS };
                        if (joyGetPosEx(i, ref ji) != 0) { d.On = false; continue; }
                        int down = ji.dwButtons & ~d.Last;
                        d.Last = ji.dwButtons;
                        if (down == 0) continue;
                        for (int b = 0; b < 32; b++)
                            if ((down & (1 << b)) != 0)
                            {
                                var h = Pressed;
                                if (h != null) try { h(d.Key, d.Name, b + 1); } catch (Exception ex) { FedeLog.Error("tasto volante", ex); }
                            }
                    }
                }
                catch (DllNotFoundException) { running = false; return; }   // non e' Windows
                catch (Exception ex) { FedeLog.Error("lettura tasti volante", ex); Thread.Sleep(2000); }
                Thread.Sleep(25);
            }
        }
    }
}
