using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace FedeDashboard
{
    /// <summary>Log semplice in PluginsData\Common\DTPlugin.log (utile se qualcosa non va).</summary>
    public static class FedeLog
    {
        static readonly object Sync = new object();
        static string path;

        public static string Folder
        {
            get
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginsData", "Common");
                try
                {
                    Directory.CreateDirectory(dir);
                    string probe = Path.Combine(dir, "DTPlugin.write-test");
                    File.WriteAllText(probe, "ok");
                    File.Delete(probe);
                    return dir;
                }
                catch
                {
                    dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DTPlugin");
                    Directory.CreateDirectory(dir);
                    return dir;
                }
            }
        }

        public static void Start()
        {
            try
            {
                path = Path.Combine(Folder, "DTPlugin.log");
                File.WriteAllText(path, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  DT Plugin avviato" + Environment.NewLine);
            }
            catch { path = null; }
        }

        public static void Write(string msg)
        {
            if (path == null) return;
            try
            {
                lock (Sync) File.AppendAllText(path, DateTime.Now.ToString("HH:mm:ss") + "  " + msg + Environment.NewLine);
            }
            catch { }
        }

        public static void Error(string where, Exception ex)
        {
            var inner = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
            Write("ERRORE " + where + ": " + inner.GetType().Name + ": " + inner.Message);
        }
    }

    /// <summary>
    /// Accesso alle funzioni di SimHub tramite reflection: cosi' il plugin non dipende dalla versione
    /// esatta delle DLL di SimHub con cui e' stato compilato. Prova piu' strade e scrive nel log quale usa.
    /// </summary>
    public class SimHubBridge
    {
        readonly IPlugin plugin;
        readonly PluginManager manager;
        readonly string prefix;
        readonly List<MethodInfo> attachCandidates = new List<MethodInfo>();   // statici (estensione) o d'istanza su PluginManager
        readonly List<MethodInfo> actionCandidates = new List<MethodInfo>();
        MethodInfo getPropertyValue, setPropertyValue, addProperty;
        MethodInfo attachChosen;           // metodo che ha funzionato
        bool useFallback;                  // AddProperty + SetPropertyValue
        bool decided;
        readonly List<KeyValuePair<string, Func<object>>> fallbackProps = new List<KeyValuePair<string, Func<object>>>();

        public SimHubBridge(IPlugin plugin, PluginManager manager)
        {
            this.plugin = plugin;
            this.manager = manager;
            prefix = plugin.GetType().Name + ".";
            var pmType = manager.GetType();
            FedeLog.Write("SimHub.Plugins: " + typeof(IPlugin).Assembly.FullName + "  PluginManager: " + pmType.FullName);

            foreach (var asm in SafeAssemblies())
            {
                string an;
                try { an = asm.GetName().Name; } catch { continue; }
                if (!an.StartsWith("SimHub", StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var m in StaticMethods(asm))
                {
                    if (m.Name != "AttachDelegate" && m.Name != "AddAction") continue;
                    var ps = m.GetParameters();
                    if (ps.Length < 3 || !ps[0].ParameterType.IsAssignableFrom(plugin.GetType()) || ps[1].ParameterType != typeof(string)) continue;
                    if (!typeof(Delegate).IsAssignableFrom(ps[2].ParameterType) && !ps[2].ParameterType.IsGenericParameter && !ps[2].ParameterType.ContainsGenericParameters) continue;
                    if (!ps.Skip(3).All(q => q.IsOptional)) continue;
                    (m.Name == "AttachDelegate" ? attachCandidates : actionCandidates).Add(m);
                    FedeLog.Write("  trovato " + Sig(m));
                }
            }
            foreach (var m in pmType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var ps = m.GetParameters();
                if (m.Name == "AttachDelegate" || m.Name == "AddAction" || m.Name == "AddProperty" || m.Name == "SetPropertyValue" || m.Name == "GetPropertyValue")
                    FedeLog.Write("  PluginManager." + Sig(m));
                if ((m.Name == "AttachDelegate" || m.Name == "AddAction") && ps.Length >= 3 && ps[0].ParameterType == typeof(string)
                    && ps[1].ParameterType == typeof(Type) && ps.Skip(3).All(q => q.IsOptional))
                    (m.Name == "AttachDelegate" ? attachCandidates : actionCandidates).Add(m);
            }
            // attenzione: SimHub ha anche versioni generiche con lo stesso nome -> niente GetMethod(nome, tipi)
            getPropertyValue = FindExact(pmType, "GetPropertyValue", typeof(string));
            setPropertyValue = FindExact(pmType, "SetPropertyValue", typeof(string), typeof(Type), typeof(object));
            addProperty = pmType.GetMethods().FirstOrDefault(x =>
            {
                if (x.Name != "AddProperty" || x.IsGenericMethodDefinition) return false;
                var p = x.GetParameters();
                return p.Length >= 3 && p[0].ParameterType == typeof(string) && p[1].ParameterType == typeof(Type)
                       && (p[2].ParameterType == typeof(object) || p[2].ParameterType == typeof(Type)) && p.Skip(3).All(q => q.IsOptional);
            });
            FedeLog.Write("SimHub: AttachDelegate candidati=" + attachCandidates.Count + " AddAction candidati=" + actionCandidates.Count +
                          " GetPropertyValue=" + (getPropertyValue != null) + " SetPropertyValue=" + (setPropertyValue != null) +
                          " AddProperty=" + (addProperty != null));
        }

        static MethodInfo FindExact(Type t, string name, params Type[] types)
        {
            foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                if (m.Name != name || m.IsGenericMethodDefinition) continue;
                var ps = m.GetParameters();
                if (ps.Length != types.Length) continue;
                bool ok = true;
                for (int i = 0; i < ps.Length; i++) if (ps[i].ParameterType != types[i]) { ok = false; break; }
                if (ok) return m;
            }
            return null;
        }

        static string Sig(MethodInfo m)
        {
            return (m.IsStatic ? m.DeclaringType.FullName + "." : "") + m.Name + (m.IsGenericMethodDefinition ? "<T>" : "") + "(" +
                   string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name + (p.IsOptional ? "=opt" : ""))) + ")";
        }

        static Assembly[] SafeAssemblies()
        {
            try { return AppDomain.CurrentDomain.GetAssemblies(); } catch { return new Assembly[0]; }
        }

        static IEnumerable<MethodInfo> StaticMethods(Assembly asm)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }
            catch { yield break; }
            foreach (var t in types)
            {
                bool visible;
                try { visible = t.IsVisible && t.IsAbstract && t.IsSealed; } catch { continue; }
                if (!visible) continue; // classi statiche pubbliche (metodi di estensione)
                MethodInfo[] ms;
                try { ms = t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly); }
                catch { continue; }
                foreach (var m in ms) yield return m;
            }
        }

        /// <summary>Prepara gli argomenti per chiamare un metodo AttachDelegate/AddAction qualunque sia la sua firma.</summary>
        object[] BuildArgs(MethodInfo m, string name, object del)
        {
            var ps = m.GetParameters();
            var args = new object[ps.Length];
            int i = 0;
            if (m.IsStatic) args[i++] = plugin;
            args[i++] = name;
            if (!m.IsStatic) args[i++] = plugin.GetType();
            args[i++] = del;
            for (; i < ps.Length; i++) args[i] = Type.Missing;
            return args;
        }

        static object AdaptDelegate<T>(Type target, Func<T> f)
        {
            if (target.IsInstanceOfType(f)) return f;
            Func<object> fo = () => f();
            if (target.IsInstanceOfType(fo)) return fo;
            // ultima prova: delegato senza parametri di un altro tipo
            return Delegate.CreateDelegate(target, fo.Target, fo.Method);
        }

        bool TryAttach<T>(MethodInfo m, string name, Func<T> f)
        {
            try
            {
                MethodInfo mm = m.IsGenericMethodDefinition ? m.MakeGenericMethod(typeof(T)) : m;
                int delIndex = mm.IsStatic ? 2 : 2;
                var del = AdaptDelegate(mm.GetParameters()[delIndex].ParameterType, f);
                mm.Invoke(mm.IsStatic ? null : (object)manager, BuildArgs(mm, name, del));
                return true;
            }
            catch (Exception ex)
            {
                FedeLog.Error("tentativo " + Sig(m) + " per " + name, ex);
                return false;
            }
        }

        bool Visible(string name)
        {
            return GetPropertyValue(prefix + name) != null;
        }

        /// <summary>Crea la proprieta' DTPlugin.name leggendo il valore da f.</summary>
        public void Attach<T>(string name, Func<T> f)
        {
            if (!decided)
            {
                // primo valore: prova le strade una alla volta e verifica che la proprieta' esista davvero
                decided = true;
                foreach (var m in attachCandidates)
                {
                    if (TryAttach(m, name, f) && Visible(name))
                    {
                        attachChosen = m;
                        FedeLog.Write("Proprieta' registrate con " + Sig(m));
                        return;
                    }
                }
                useFallback = true;
                FedeLog.Write("AttachDelegate non funziona: uso AddProperty/SetPropertyValue");
            }
            else if (attachChosen != null)
            {
                if (TryAttach(attachChosen, name, f)) return;
            }
            AddFallback(name, () => f());
        }

        void AddFallback(string name, Func<object> f)
        {
            try
            {
                if (addProperty != null)
                {
                    var p = addProperty.GetParameters();
                    var args = new object[p.Length];
                    object v = null;
                    try { v = f(); } catch { }
                    args[0] = name; args[1] = plugin.GetType();
                    args[2] = p[2].ParameterType == typeof(Type) ? (object)(v != null ? v.GetType() : typeof(object)) : v;
                    for (int i = 3; i < p.Length; i++) args[i] = Type.Missing;
                    addProperty.Invoke(manager, args);
                }
            }
            catch (Exception ex) { FedeLog.Error("AddProperty " + name, ex); }
            fallbackProps.Add(new KeyValuePair<string, Func<object>>(name, f));
        }

        public void UpdateFallbackProperties()
        {
            if (fallbackProps.Count == 0 || setPropertyValue == null) return;
            var t = plugin.GetType();
            foreach (var kv in fallbackProps)
            {
                try { setPropertyValue.Invoke(manager, new object[] { kv.Key, t, kv.Value() }); }
                catch { }
            }
        }

        /// <summary>Scrive nel log se la dashboard puo' leggere le proprieta' (chiamato dopo il primo aggiornamento).</summary>
        public void ReportCheck()
        {
            foreach (var n in new[] { prefix + "Pagina", prefix + "Gear", "DT Plugin.Pagina", "Pagina" })
                FedeLog.Write("verifica " + n + " = " + (GetPropertyValue(n) ?? "(null)"));
        }

        /// <summary>Registra un'azione (DTPlugin.name) da collegare a un tasto in "Controlli ed eventi".</summary>
        public void AddAction(string name, Action run)
        {
            foreach (var m in actionCandidates)
            {
                try
                {
                    var mm = m.IsGenericMethodDefinition ? m.MakeGenericMethod(typeof(object)) : m;
                    var delegateType = mm.GetParameters()[2].ParameterType;
                    var thunk = new ActionThunk(run);
                    int n = delegateType.GetMethod("Invoke").GetParameters().Length;
                    MethodInfo target = typeof(ActionThunk).GetMethods().First(x => x.Name == "Run" && x.GetParameters().Length == n);
                    var d = Delegate.CreateDelegate(delegateType, thunk, target);
                    mm.Invoke(mm.IsStatic ? null : (object)manager, BuildArgs(mm, name, d));
                    return;
                }
                catch (Exception ex) { FedeLog.Error("AddAction " + Sig(m) + " " + name, ex); }
            }
            FedeLog.Write("AddAction non riuscita: " + name);
        }

        public object GetPropertyValue(string name)
        {
            if (getPropertyValue == null) return null;
            try { return getPropertyValue.Invoke(manager, new object[] { name }); }
            catch { return null; }
        }

        public class ActionThunk
        {
            readonly Action run;
            public ActionThunk(Action run) { this.run = run; }
            public void Run() { Safe(); }
            public void Run(object a) { Safe(); }
            public void Run(object a, object b) { Safe(); }
            public void Run(object a, object b, object c) { Safe(); }
            void Safe()
            {
                try { run(); } catch (Exception ex) { FedeLog.Error("azione", ex); }
            }
        }
    }

    /// <summary>Salvataggio impostazioni in JSON (senza dipendere da SimHub).</summary>
    public static class SettingsStore
    {
        static string FilePath { get { return Path.Combine(FedeLog.Folder, "DTPlugin.json"); } }

        public static FedeSettings Load()
        {
            try
            {
                // prima versione del plugin (si chiamava "Fede Plugin"): riprendi le sue impostazioni
                string legacy = Path.Combine(FedeLog.Folder, "FedePlugin.json");
                string path = File.Exists(FilePath) ? FilePath : (File.Exists(legacy) ? legacy : null);
                if (path != null)
                {
                    if (path == legacy) FedeLog.Write("Impostazioni riprese dal vecchio FedePlugin.json");
                    var ser = new DataContractJsonSerializer(typeof(FedeSettings));
                    using (var fs = File.OpenRead(path))
                    {
                        var s = ser.ReadObject(fs) as FedeSettings;
                        if (s != null) { s.Normalize(); return s; }
                    }
                }
            }
            catch (Exception ex) { FedeLog.Error("lettura impostazioni", ex); }
            return FedeSettings.CreateDefault();
        }

        public static void SaveTo(FedeSettings s, string path)
        {
            var ser = new DataContractJsonSerializer(typeof(FedeSettings));
            using (var fs = File.Create(path)) ser.WriteObject(fs, s);
        }

        public static FedeSettings LoadFrom(string path)
        {
            try
            {
                var ser = new DataContractJsonSerializer(typeof(FedeSettings));
                using (var fs = File.OpenRead(path))
                {
                    var s = ser.ReadObject(fs) as FedeSettings;
                    if (s == null || s.Pages == null) return null;
                    s.Normalize();
                    return s;
                }
            }
            catch (Exception ex) { FedeLog.Error("lettura " + path, ex); return null; }
        }

        public static void Save(FedeSettings s)
        {
            if (s == null) return;
            try
            {
                var ser = new DataContractJsonSerializer(typeof(FedeSettings));
                string tmp = FilePath + ".tmp";
                using (var fs = File.Create(tmp))
                    ser.WriteObject(fs, s);
                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(tmp, FilePath);
            }
            catch (Exception ex) { FedeLog.Error("salvataggio impostazioni", ex); }
        }
    }
}
