using System;
using System.IO;
using Microsoft.Win32;

namespace FedeDashboard
{
    /// <summary>
    /// Carattere "DT Montserrat" (Montserrat con le cifre a larghezza fissa) dentro il plugin.
    /// Quando viene scelto lo scrive in %LOCALAPPDATA%\DTPlugin\Fonts e prova a installarlo in Windows
    /// (per tutti gli utenti se SimHub ha i permessi, altrimenti solo per questo utente).
    /// Nella dash il carattere e' scritto come elenco: nome installato, poi la cartella del plugin, poi Roboto.
    /// </summary>
    public static class FontInstaller
    {
        public const string Family = "DT Montserrat";
        static readonly object sync = new object();
        static bool done;
        public static string Status = "";

        public static string Folder
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DTPlugin", "Fonts"); }
        }

        /// <summary>Valore da scrivere in "Font" nella dash.</summary>
        public static string FontValue()
        {
            Ensure();
            string uri;
            try { uri = new Uri(Folder.TrimEnd('\\', '/') + Path.DirectorySeparatorChar).AbsoluteUri; }
            catch { uri = ""; }
            return Family + (uri.Length > 0 ? ", " + uri + "#" + Family : "") + ", Roboto";
        }

        public static void Ensure()
        {
            lock (sync)
            {
                if (done) return;
                done = true;
                try
                {
                    Directory.CreateDirectory(Folder);
                    foreach (var f in FontData.Files)
                    {
                        byte[] data = Convert.FromBase64String(f[1]);
                        string p = Path.Combine(Folder, f[0]);
                        if (!File.Exists(p) || new FileInfo(p).Length != data.Length) File.WriteAllBytes(p, data);
                    }
                }
                catch (Exception ex) { FedeLog.Error("scrittura carattere DT Montserrat", ex); Status = "Non sono riuscito a copiare il carattere."; return; }
                string how = InstallAllUsers() ? "per tutti gli utenti" : (InstallUser() ? "per questo utente" : "");
                Status = how.Length > 0 ? "Carattere DT Montserrat installato " + how + "." : "Carattere DT Montserrat usato dalla cartella del plugin.";
                FedeLog.Write(Status);
            }
        }

        static string Style(string file)
        {
            string s = Path.GetFileNameWithoutExtension(file);
            int i = s.IndexOf('-');
            return i < 0 ? "" : s.Substring(i + 1);
        }

        static string RegName(string file)
        {
            string st = Style(file);
            return Family + (st == "Regular" ? "" : " " + st) + " (TrueType)";
        }

        static bool InstallAllUsers()
        {
            try
            {
                string win = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
                if (!Directory.Exists(win)) return false;
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", true))
                {
                    if (key == null) return false;
                    foreach (var f in FontData.Files)
                    {
                        string dst = Path.Combine(win, f[0]);
                        string src = Path.Combine(Folder, f[0]);
                        if (!File.Exists(dst) || new FileInfo(dst).Length != new FileInfo(src).Length) File.Copy(src, dst, true);
                        key.SetValue(RegName(f[0]), f[0]);
                    }
                }
                return true;
            }
            catch (Exception ex) { FedeLog.Write("Carattere: installazione per tutti gli utenti non possibile (" + ex.GetType().Name + ")"); return false; }
        }

        static bool InstallUser()
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts");
                Directory.CreateDirectory(dir);
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts"))
                {
                    if (key == null) return false;
                    foreach (var f in FontData.Files)
                    {
                        string dst = Path.Combine(dir, f[0]);
                        string src = Path.Combine(Folder, f[0]);
                        if (!File.Exists(dst) || new FileInfo(dst).Length != new FileInfo(src).Length) File.Copy(src, dst, true);
                        key.SetValue(RegName(f[0]), dst);
                    }
                }
                return true;
            }
            catch (Exception ex) { FedeLog.Write("Carattere: installazione per l'utente non possibile (" + ex.GetType().Name + ")"); return false; }
        }
    }
}
