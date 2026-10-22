using System;
using System.Globalization;
using System.Reflection;
using System.Security;

namespace FedeDashboard
{
    /// <summary>
    /// Piccoli aiuti per costruire l'interfaccia WPF a runtime (XAML + reflection),
    /// senza legare il plugin alle DLL di WPF in fase di compilazione.
    /// </summary>
    public static class Wpf
    {
        const string Suffix = ", Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        public const string NS = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";
        static MethodInfo parse;

        public static Type T(string fullName)
        {
            foreach (var asm in new[] { "PresentationFramework", "PresentationCore", "WindowsBase" })
            {
                var t = Type.GetType(fullName + ", " + asm + Suffix, false);
                if (t != null) return t;
            }
            throw new TypeLoadException("Tipo WPF non trovato: " + fullName);
        }

        /// <summary>System.Windows.Visibility.Visible / Collapsed (senza riferimento a WPF in compilazione).</summary>
        public static object Visibility(bool visible)
        {
            return Enum.Parse(T("System.Windows.Visibility"), visible ? "Visible" : "Collapsed");
        }

        /// <summary>Crea oggetti WPF da testo XAML. Usa {NS} per gli xmlns.</summary>
        public static dynamic Xaml(string xaml)
        {
            if (parse == null) parse = T("System.Windows.Markup.XamlReader").GetMethod("Parse", new[] { typeof(string) });
            try
            {
                return parse.Invoke(null, new object[] { xaml.Replace("{NS}", NS) });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>Testo sicuro per un attributo XAML.</summary>
        public static string Esc(string s)
        {
            return (SecurityElement.Escape(s ?? "") ?? "").Replace("\n", "&#10;");
        }

        public static string N(double v)
        {
            return v.ToString("0.##", CultureInfo.InvariantCulture);
        }

        public static dynamic Find(object root, string name)
        {
            var m = root.GetType().GetMethod("FindName", new[] { typeof(string) });
            return m.Invoke(root, new object[] { name });
        }

        /// <summary>Collega un evento WPF (Click, SelectionChanged, ...) a una funzione.</summary>
        public static void On(object target, string eventName, Action<object, object> handler)
        {
            var ei = target.GetType().GetEvent(eventName);
            if (ei == null) throw new MissingMemberException(target.GetType().Name, eventName);
            var thunk = new Thunk(handler, eventName);
            var d = Delegate.CreateDelegate(ei.EventHandlerType, thunk, typeof(Thunk).GetMethod("Invoke"));
            ei.AddEventHandler(target, d);
        }

        public static void Alert(string text)
        {
            var mb = T("System.Windows.MessageBox");
            mb.GetMethod("Show", new[] { typeof(string), typeof(string) }).Invoke(null, new object[] { text, "DT Plugin" });
        }

        public static bool Confirm(string text)
        {
            var mb = T("System.Windows.MessageBox");
            var btn = T("System.Windows.MessageBoxButton");
            var res = mb.GetMethod("Show", new[] { typeof(string), typeof(string), btn })
                        .Invoke(null, new object[] { text, "DT Plugin", Enum.Parse(btn, "YesNo") });
            return res != null && res.ToString() == "Yes";
        }

        public class Thunk
        {
            readonly Action<object, object> handler;
            readonly string name;

            public Thunk(Action<object, object> handler, string name)
            {
                this.handler = handler;
                this.name = name;
            }

            public void Invoke(object sender, EventArgs e)
            {
                try { handler(sender, e); }
                catch (Exception ex) { FedeLog.Error("evento " + name, ex); }
            }
        }
    }
}
