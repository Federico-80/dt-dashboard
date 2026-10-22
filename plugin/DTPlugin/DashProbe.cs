using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace FedeDashboard
{
    /// <summary>
    /// Elenca nel log i tipi di SimHub che gestiscono le dashboard e i loro metodi di ricarica,
    /// per poter ricaricare la dash dopo un cambio di layout senza riavviare SimHub.
    /// </summary>
    public static class DashProbe
    {
        static readonly string[] Words = { "Reload", "Refresh", "LoadDash", "OpenDash", "SetDash", "Restart", "ChangeDash", "SelectDash", "Invalidate" };

        public static void Run()
        {
            try
            {
                int lines = 0;
                var sb = new StringBuilder();
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string an = asm.GetName().Name ?? "";
                    if (!an.StartsWith("SimHub", StringComparison.OrdinalIgnoreCase)) continue;
                    Type[] types;
                    try { types = asm.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types; } catch { continue; }
                    foreach (var t in types)
                    {
                        if (t == null || t.FullName == null) continue;
                        string tn = t.FullName;
                        if (tn.IndexOf("Dash", StringComparison.OrdinalIgnoreCase) < 0 && tn.IndexOf("Screen", StringComparison.OrdinalIgnoreCase) < 0) continue;
                        if (tn.Contains("<") || tn.Contains("+<")) continue;
                        var found = new List<string>();
                        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                        {
                            if (m.Name.Contains("<")) continue;
                            bool hit = false;
                            foreach (var w in Words) if (m.Name.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0) { hit = true; break; }
                            if (!hit) continue;
                            var ps = new List<string>();
                            foreach (var p in m.GetParameters()) ps.Add(p.ParameterType.Name);
                            found.Add((m.IsStatic ? "static " : "") + (m.IsPublic ? "" : "~") + m.Name + "(" + string.Join(",", ps) + ")");
                        }
                        if (found.Count == 0) continue;
                        sb.Append(an).Append(": ").Append(tn).Append(" -> ").Append(string.Join("; ", found)).Append(Environment.NewLine);
                        if (++lines >= 120) break;
                    }
                    if (lines >= 120) break;
                }
                FedeLog.Write("Sonda dashboard (" + lines + " tipi):" + Environment.NewLine + sb);
            }
            catch (Exception ex) { FedeLog.Error("sonda dashboard", ex); }
        }
    }
}
