using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace FedeDashboard
{
    /// <summary>
    /// Codice di accesso: plugin e dashboard funzionano solo con un codice valido.
    /// I codici validi sono salvati solo come impronta (PBKDF2), in release/accessi.json su GitHub
    /// (si possono aggiungere o togliere senza una nuova versione) e, come riserva, dentro il plugin (AccessData).
    /// </summary>
    public class Access
    {
        const string Url = "https://raw.githubusercontent.com/" + Updater.Repo + "/main/release/accessi.json";
        const string ApiUrl = "https://api.github.com/repos/" + Updater.Repo + "/contents/release/accessi.json";

        public volatile bool Unlocked;
        public volatile string Status = "";
        public volatile string Holder = "";
        public event Action Changed;
        readonly AccessSettings settings;
        readonly Action save;
        readonly object sync = new object();

        public Access(AccessSettings s, Action save) { settings = s; this.save = save; }

        void Notify() { try { var c = Changed; if (c != null) c(); } catch { } }

        class CodeList { public string Salt = ""; public int Iter; public Dictionary<string, string> Codes = new Dictionary<string, string>(); public bool Empty { get { return Codes.Count == 0; } } }

        /// <summary>Impronta del codice (maiuscole, spazi tolti).</summary>
        public static string Hash(string code, string salt, int iter)
        {
            string c = Regex.Replace((code ?? "").ToUpperInvariant(), "\\s+", "");
            using (var kdf = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(c), Encoding.UTF8.GetBytes(salt), iter))
            {
                var b = kdf.GetBytes(32);
                var sb = new StringBuilder();
                foreach (var x in b) sb.Append(x.ToString("x2"));
                return sb.ToString();
            }
        }

        static CodeList Parse(string json)
        {
            var l = new CodeList();
            if (string.IsNullOrWhiteSpace(json)) return l;
            var m = Regex.Match(json, "\"salt\"\\s*:\\s*\"([^\"]*)\"");
            if (m.Success) l.Salt = m.Groups[1].Value;
            m = Regex.Match(json, "\"iter\"\\s*:\\s*(\\d+)");
            l.Iter = m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
            foreach (Match c in Regex.Matches(json, "\\{\\s*\"nome\"\\s*:\\s*\"([^\"]*)\"\\s*,\\s*\"hash\"\\s*:\\s*\"([0-9a-f]{64})\"\\s*\\}"))
                l.Codes[c.Groups[2].Value] = c.Groups[1].Value;
            if (l.Iter <= 0 || l.Salt.Length == 0) l.Codes.Clear();
            return l;
        }

        /// <summary>Elenco valido: GitHub se raggiungibile, altrimenti l'ultima copia scaricata, altrimenti quella dentro il plugin.</summary>
        CodeList Current(bool online)
        {
            if (online)
            {
                string json = TestDownload != null ? TestDownload() : Download();
                if (json != null && json.Length > 0)
                {
                    var l = Parse(json);
                    settings.CachedList = json;
                    return l;
                }
                // nessun elenco su GitHub: vale quello dentro il plugin (se vuoto, accesso libero)
                if (json != null) return Parse(AccessData.Json);
            }
            var cached = Parse(settings.CachedList);
            if (!cached.Empty) return cached;
            return Parse(AccessData.Json);
        }

        /// <summary>Solo per i test: sostituisce lo scaricamento da GitHub (null = rete assente, "" = nessun elenco).</summary>
        public static Func<string> TestDownload;

        static string Download()
        {
            foreach (var u in new[] { Url + "?t=" + DateTime.UtcNow.Ticks, ApiUrl })
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        try { ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; } catch { }
                        wc.Headers[HttpRequestHeader.UserAgent] = "DTPlugin";
                        if (u.StartsWith("https://api.")) wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github.raw";
                        wc.Encoding = Encoding.UTF8;
                        return wc.DownloadString(u);
                    }
                }
                catch (WebException ex)
                {
                    var r = ex.Response as HttpWebResponse;
                    if (r != null && r.StatusCode == HttpStatusCode.NotFound) return "";   // nessun elenco pubblicato
                }
                catch { }
            }
            return null;
        }

        void Evaluate(CodeList l)
        {
            if (l.Empty) { Unlocked = true; Holder = ""; Status = ""; return; }   // nessun codice impostato: libero
            string mine = settings.Hash ?? "";
            string name;
            if (mine.Length > 0 && settings.Salt == l.Salt && l.Codes.TryGetValue(mine, out name))
            {
                Unlocked = true; Holder = name; Status = "Sbloccato" + (name.Length > 0 ? " (" + name + ")" : "") + ".";
            }
            else
            {
                bool had = mine.Length > 0;
                Unlocked = false; Holder = "";
                Status = had ? "Il codice di questo PC non è più valido: inseriscine uno nuovo." : "Inserisci il codice di accesso per usare DT Plugin e DT Dashboard.";
            }
        }

        /// <summary>All'avvio: subito con i dati salvati, poi verifica su GitHub in background.</summary>
        public void Start()
        {
            lock (sync) Evaluate(Current(false));
            Notify();
            CheckAsync();
        }

        public void CheckAsync()
        {
            new Thread(Check) { IsBackground = true, Name = "DT accesso" }.Start();
        }

        /// <summary>Verifica l'elenco su GitHub (sul thread chiamante).</summary>
        public void Check()
        {
            try
            {
                bool before = Unlocked;
                lock (sync) Evaluate(Current(true));
                if (before != Unlocked) FedeLog.Write("Accesso: " + (Unlocked ? "sbloccato" : "bloccato") + " (" + Status + ")");
                try { save(); } catch { }
            }
            catch (Exception ex) { FedeLog.Error("verifica accesso", ex); }
            Notify();
        }

        /// <summary>Prova un codice (sul thread chiamante). Restituisce true se valido.</summary>
        public bool TryUnlock(string code)
        {
            bool ok;
            lock (sync)
            {
                var l = Current(true);
                if (l.Empty) { Evaluate(l); Notify(); return true; }
                string h = Hash(code, l.Salt, l.Iter);
                ok = l.Codes.ContainsKey(h);
                if (ok) { settings.Hash = h; settings.Salt = l.Salt; }
                Evaluate(l);
                if (!ok) Status = "Codice non valido.";
            }
            FedeLog.Write("Accesso: tentativo " + (ok ? "riuscito" : "non valido"));
            try { save(); } catch { }
            Notify();
            return ok;
        }
    }
}
