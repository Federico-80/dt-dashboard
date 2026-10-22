using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FedeDashboard
{
    /// <summary>
    /// Fa ricaricare a SimHub la dashboard dopo che il plugin l'ha riscritta (cambio di cornice, griglie, barra, carattere).
    /// SimHub non ha un'API pubblica per questo: cerco tra i suoi oggetti la dashboard "DT Dashboard"
    /// (GraphicalDashItem.ReloadEverywhere) o gli schermi che la mostrano (ForceDashReload).
    /// </summary>
    public static class DashReloader
    {
        const string Dash = "DT Dashboard";
        const int MaxNodes = 250000, MaxDepth = 9, MaxItems = 3000;

        class RefEq : IEqualityComparer<object>
        {
            public new bool Equals(object a, object b) { return ReferenceEquals(a, b); }
            public int GetHashCode(object o) { return RuntimeHelpers.GetHashCode(o); }
        }

        /// <summary>Restituisce true se almeno una ricarica e' stata chiesta a SimHub.</summary>
        public static bool Reload(object pluginManager)
        {
            var started = DateTime.Now;
            var dashItems = new List<object>();
            var screens = new List<object>();
            int nodes = 0;
            try
            {
                var seen = new HashSet<object>(new RefEq());
                var queue = new Queue<KeyValuePair<object, int>>();
                Action<object, int> push = (o, d) =>
                {
                    if (o == null || d > MaxDepth || !Interesting(o.GetType())) return;
                    if (seen.Add(o)) queue.Enqueue(new KeyValuePair<object, int>(o, d));
                };
                push(pluginManager, 0);
                // anche i campi statici dei tipi della parte dashboard di SimHub
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string an = asm.GetName().Name ?? "";
                    if (!an.StartsWith("SimHub", StringComparison.OrdinalIgnoreCase)) continue;
                    Type[] types;
                    try { types = asm.GetTypes(); } catch (ReflectionTypeLoadException ex) { types = ex.Types; } catch { continue; }
                    foreach (var t in types)
                    {
                        if (t == null || t.IsGenericTypeDefinition || t.FullName == null || t.FullName.IndexOf("GraphicalDash", StringComparison.Ordinal) < 0) continue;
                        foreach (var f in t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            if (f.FieldType.IsPrimitive || f.FieldType == typeof(string)) continue;
                            try { push(f.GetValue(null), 1); } catch { }
                        }
                    }
                }

                while (queue.Count > 0 && nodes < MaxNodes)
                {
                    var kv = queue.Dequeue();
                    object o = kv.Key; int d = kv.Value;
                    nodes++;
                    Type t = o.GetType();
                    string tn = t.FullName ?? "";
                    if (tn == "SimHub.Plugins.OutputPlugins.GraphicalDash.GraphicalDashItem")
                    {
                        if (HasName(o)) dashItems.Add(o);
                        continue;   // dentro ci sono i modelli della dash: non serve scendere
                    }
                    if (HasMethod(t, "ForceDashReload") && HasName(o)) screens.Add(o);

                    var en = o as IEnumerable;
                    if (en != null && !(o is string))
                    {
                        int n = 0;
                        try
                        {
                            foreach (var item in en)
                            {
                                if (++n > MaxItems) break;
                                if (item is DictionaryEntry) { push(((DictionaryEntry)item).Value, d + 1); continue; }
                                push(item, d + 1);
                            }
                        }
                        catch { }
                        if (t.IsArray) continue;
                    }
                    for (Type c = t; c != null && c != typeof(object); c = c.BaseType)
                    {
                        foreach (var f in c.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                        {
                            Type ft = f.FieldType;
                            if (ft.IsPrimitive || ft.IsEnum || ft == typeof(string) || typeof(Delegate).IsAssignableFrom(ft)) continue;
                            object v;
                            try { v = f.GetValue(o); } catch { continue; }
                            if (v == null) continue;
                            // KeyValuePair e altre struct generiche
                            if (v.GetType().IsValueType)
                            {
                                var vt = v.GetType();
                                if (vt.IsGenericType && vt.Name.StartsWith("KeyValuePair"))
                                {
                                    try { push(vt.GetProperty("Value").GetValue(v, null), d + 1); } catch { }
                                }
                                continue;
                            }
                            push(v, d + 1);
                        }
                    }
                }
            }
            catch (Exception ex) { FedeLog.Error("ricerca dashboard in SimHub", ex); }

            bool done = false;
            var ms = (int)(DateTime.Now - started).TotalMilliseconds;
            // 1) la dashboard stessa: la ricarica ovunque sia aperta
            foreach (var item in dashItems)
            {
                if (Invoke(item, "ReloadEverywhere")) { done = true; }
            }
            // 2) altrimenti gli schermi che la stanno mostrando
            if (!done)
                foreach (var s in screens)
                    if (Invoke(s, "ForceDashReload")) done = true;
            FedeLog.Write("Ricarica dashboard: " + (done ? "chiesta a SimHub" : "non riuscita") + " (dash trovate " + dashItems.Count +
                          ", schermi " + screens.Count + ", oggetti esaminati " + nodes + ", " + ms + " ms)");
            return done;
        }

        static bool Interesting(Type t)
        {
            if (t.IsPrimitive || t.IsEnum || t == typeof(string) || t.IsPointer) return false;
            if (t.IsArray) { var e = t.GetElementType(); return !(e.IsPrimitive || e == typeof(string) || e.IsEnum); }
            string ns = t.Namespace ?? "";
            if (ns.StartsWith("SimHub", StringComparison.Ordinal)) return !ns.Contains(".Models");   // gli elementi grafici delle dash: troppi e inutili
            if (ns.StartsWith("System.Collections", StringComparison.Ordinal)) return true;
            if (ns == "System" && t.IsGenericType && t.Name.StartsWith("Lazy", StringComparison.Ordinal)) return false;
            return false;
        }

        static bool HasMethod(Type t, string name)
        {
            try { return t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null; }
            catch { return false; }
        }

        /// <summary>Un campo stringa (o una proprieta' "Name") dell'oggetto vale "DT Dashboard".</summary>
        static bool HasName(object o)
        {
            try
            {
                for (Type c = o.GetType(); c != null && c != typeof(object); c = c.BaseType)
                {
                    foreach (var f in c.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (f.FieldType != typeof(string)) continue;
                        string v = f.GetValue(o) as string;
                        if (v != null && Matches(v)) return true;
                    }
                }
            }
            catch { }
            return false;
        }

        static bool Matches(string v)
        {
            v = v.Replace('\\', '/').TrimEnd('/');
            if (string.Equals(v, Dash, StringComparison.OrdinalIgnoreCase)) return true;
            return v.EndsWith("/" + Dash, StringComparison.OrdinalIgnoreCase) || v.EndsWith("/" + Dash + ".djson", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Chiama il metodo sul thread dell'interfaccia di SimHub.</summary>
        static bool Invoke(object target, string method)
        {
            try
            {
                var m = target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (m == null) return false;
                Action call = () =>
                {
                    try { m.Invoke(target, null); FedeLog.Write("Ricarica: " + target.GetType().Name + "." + method + " eseguito"); }
                    catch (Exception ex) { FedeLog.Error("ricarica " + target.GetType().Name + "." + method, ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex); }
                };
                Type appType = null;
                try { appType = Wpf.T("System.Windows.Application"); } catch { }
                object app = appType == null ? null : appType.GetProperty("Current").GetValue(null, null);
                if (app == null) { call(); return true; }
                object disp = appType.GetProperty("Dispatcher").GetValue(app, null);
                disp.GetType().GetMethod("BeginInvoke", new[] { typeof(Delegate), typeof(object[]) }).Invoke(disp, new object[] { call, new object[0] });
                return true;
            }
            catch (Exception ex) { FedeLog.Error("ricarica " + method, ex); return false; }
        }
    }
}
