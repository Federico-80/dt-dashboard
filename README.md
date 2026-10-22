# DT Dashboard + DT Plugin per SimHub

Dashboard per SimHub (850×480) con il suo plugin, **DT Plugin**, che decide cosa mostrare e come.
Funziona con **Le Mans Ultimate** (dati completi del gioco) e con **iRacing**.

## Cosa fa

- Pagine e layout scelti dal plugin, cambiano in tempo reale. Puoi trascinare i riquadri con il mouse e ci sono le pagine automatiche per prove, qualifica e gara.
- Schede per:
  - gomme (temperatura corretta per LMU, pressione in kPa, freni, usura, gomma forata o persa);
  - carburante ed energia virtuale, con i giri che puoi ancora fare;
  - meteo e motore, con icone;
  - distacchi con nome e categoria, doppiati (+1L);
  - schema danni;
  - stato dell'auto (luci, bandiera, tergicristalli, motore).
- Delta con i 3 settori colorati: verde = tuo migliore, fucsia = migliore della classe, giallo = più lento.
- Barra strategia gara come quella di RBR32: soste, finestra box, ratio, stint, giri alla fine, energia necessaria, "riempi a". Si attiva subito, come in LMU.
- Avvisi configurabili:
  - box / pit limiter, con la schermata completa;
  - danni;
  - bandiere (sopra la velocità);
  - cambio TC, ABS, BB, mappa;
  - fine giro.
- Riquadri TC e ABS che si illuminano quando intervengono. Marcia rossa ai giri da cambiata. Avviso di motore spento.
- Schermata iniziale con numero, nome, sfondo animato e stemma (preset DT Racing Team).
- Doppia barra in basso: riquadri sopra e strategia gara sotto.

## Installazione (la prima volta)

1. Scarica l'ultima versione dalla cartella [`release`](release):
   - [`DTPlugin.dll`](release/DTPlugin.dll) (apri il file e premi il pulsante di download)
   - [`DT.Dashboard.simhubdash`](release/DT.Dashboard.simhubdash)
2. Chiudi SimHub e copia `DTPlugin.dll` nella cartella di SimHub, quella dove c'è `SimHubWPF.exe`.
   Se Windows lo blocca: tasto destro sul file → Proprietà → **Sblocca**.
3. Apri SimHub e attiva **DT Plugin** quando te lo chiede.
4. Fai doppio clic su `DT.Dashboard.simhubdash` per importare la dashboard.

## Aggiornamenti

Il plugin controlla da solo se su GitHub c'è una versione nuova (file `release/latest.json`).
Quando c'è, in **SimHub → DT Plugin** compare in alto **"Installa aggiornamento"**.
Il pulsante scarica plugin e dashboard e li mette a posto da solo: dopo basta **riavviare SimHub**.
Le tue impostazioni restano, perché sono salvate in `PluginsData\Common\DTPlugin.json`.

Puoi anche controllare a mano con **"Controlla aggiornamenti"** nella pagina del plugin.

Se SimHub è installato in `C:\Program Files (x86)`, per scrivere i file potrebbe servire avviarlo **come amministratore**.

## Contenuto del repository

| Cartella | Cosa c'è |
|---|---|
| `plugin/` | sorgenti del DT Plugin (C#, .NET Framework 4.8). Si apre con `DTPlugin.sln` in Visual Studio. |
| `generatore/` | script Python che generano la dashboard (`gen_dash.py`) e il catalogo del plugin (`gen_cs.py`). |
| `ISTRUZIONI.txt` | guida completa in italiano a tutte le opzioni. |

Per rigenerare la dashboard: `cd generatore && python gen_dash.py`. Serve Pillow.
Il risultato va nella cartella `build/`.

## Problemi

Apri una [issue](../../issues) e allega il file `PluginsData\Common\DTPlugin.log` che trovi nella cartella di SimHub.
