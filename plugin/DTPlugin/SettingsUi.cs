using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace FedeDashboard
{
    /// <summary>Pannello impostazioni del plugin: XAML caricato a runtime + logica qui sotto.</summary>
    public class SettingsUi
    {
        const string Accent = "#FF00A8E0";

        const string MainXaml = @"
<UserControl {NS} Background='#FF1C1C1C' Foreground='#FFE8E8E8' FontSize='12.5'>
  <DockPanel>
    <!-- intestazione -->
    <Grid DockPanel.Dock='Top' Margin='22,16,22,10'>
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width='Auto'/>
        <ColumnDefinition Width='*'/>
      </Grid.ColumnDefinitions>
      <StackPanel Orientation='Horizontal'>
        <Border Width='42' Height='42' CornerRadius='10' Margin='0,0,12,0'>
          <Border.Background>
            <SolidColorBrush Color='#FF00FED1'/>
          </Border.Background>
          <Path Data='{LOGO}' Fill='Black' Stretch='Uniform' Margin='6,10'/>
        </Border>
        <StackPanel VerticalAlignment='Center'>
          <TextBlock Text='DT DASHBOARD' FontSize='22' FontWeight='Black' FontStyle='Italic' Foreground='White'/>
          <TextBlock x:Name='VersionText' Text='DT Plugin' FontSize='11.5' Foreground='#FF9A9A9A'/>
          <TextBlock x:Name='UpdateStatus' Text='' FontSize='11.5' Foreground='#FF64B5F6' TextWrapping='Wrap' MaxWidth='520'/>
        </StackPanel>
      </StackPanel>
      <StackPanel Grid.Column='1' Orientation='Horizontal' HorizontalAlignment='Right' VerticalAlignment='Bottom'>
        <Button x:Name='BtnCheckUpdate' Content='Controlla aggiornamenti' Padding='12,4' Margin='0,0,14,0'/>
        <Button x:Name='BtnExport' Content='Esporta impostazioni' Style='{DynamicResource LinkButton}'/>
        <Button x:Name='BtnImport' Content='Importa impostazioni' Style='{DynamicResource LinkButton}'/>
        <Button x:Name='BtnReset' Content='Ripristina tutto' Style='{DynamicResource LinkButton}'/>
      </StackPanel>
    </Grid>

    <!-- aggiornamento da GitHub -->
    <Border x:Name='UpdateBar' DockPanel.Dock='Top' Margin='22,0,22,10' Padding='14,8' CornerRadius='6' Background='#FF15304A' BorderBrush='#FF2196F3' BorderThickness='1' Visibility='Collapsed'>
      <DockPanel>
        <Button x:Name='BtnUpdate' DockPanel.Dock='Right' Content='Installa e riavvia SimHub' Padding='14,4' Margin='12,0,0,0'/>
        <TextBlock x:Name='UpdateText' VerticalAlignment='Center' TextWrapping='Wrap' Foreground='White' FontSize='13'/>
      </DockPanel>
    </Border>

    <!-- codice di accesso -->
    <Border x:Name='LockBar' DockPanel.Dock='Top' Margin='22,0,22,10' Padding='16,12' CornerRadius='6' Background='#FF3A1010' BorderBrush='#FFE53935' BorderThickness='1' Visibility='Collapsed'>
      <StackPanel>
        <TextBlock Text='DT Plugin bloccato' FontSize='16' FontWeight='Bold' Foreground='White'/>
        <TextBlock x:Name='LockText' Margin='0,4,0,10' TextWrapping='Wrap' Foreground='#FFE0E0E0' FontSize='13'/>
        <StackPanel Orientation='Horizontal'>
          <TextBlock Text='Codice:' VerticalAlignment='Center' Foreground='White' Margin='0,0,10,0'/>
          <PasswordBox x:Name='LockCode' Width='240' Padding='4,3'/>
          <Button x:Name='BtnUnlock' Content='Sblocca' Padding='16,4' Margin='10,0,0,0'/>
        </StackPanel>
      </StackPanel>
    </Border>

    <!-- layout cambiato: SimHub deve ricaricare la dashboard -->
    <Border x:Name='ReloadBar' DockPanel.Dock='Top' Margin='22,0,22,10' Padding='14,8' CornerRadius='6' Background='#FF3D3500' BorderBrush='#FFFFD600' BorderThickness='1' Visibility='Collapsed'>
      <DockPanel>
        <Button x:Name='BtnReload' DockPanel.Dock='Right' Content='Riavvia SimHub ora' Padding='14,4' Margin='12,0,0,0'/>
        <TextBlock x:Name='ReloadText' VerticalAlignment='Center' TextWrapping='Wrap' Foreground='White' FontSize='13'
          Text='Hai cambiato la disposizione o il carattere, ma SimHub non è riuscito a ricaricare la dashboard da solo. Riapri la dash nello schermo oppure riavvia SimHub.'/>
      </DockPanel>
    </Border>

    <TabControl x:Name='Tabs' Margin='16,0,16,0'>
      <TabItem Header='Schermata iniziale'>
        <ScrollViewer VerticalScrollBarVisibility='Auto'>
          <Grid Margin='22,18,22,22'>
            <Grid.ColumnDefinitions>
              <ColumnDefinition Width='*' MinWidth='420'/>
              <ColumnDefinition Width='470'/>
            </Grid.ColumnDefinitions>
            <StackPanel x:Name='IdlePanel' Grid.Column='0' Margin='0,0,24,0'/>
            <StackPanel Grid.Column='1'>
              <TextBlock Text='Anteprima' FontWeight='Bold' FontSize='14' Foreground='White' Margin='0,0,0,10'/>
              <ContentControl x:Name='IdlePreview'/>
              <TextBlock TextWrapping='Wrap' Foreground='#FF9A9A9A' FontSize='11.5' Margin='0,10,0,0'
                         Text='Questa schermata compare sulla dashboard quando il gioco non è in esecuzione.'/>
            </StackPanel>
          </Grid>
        </ScrollViewer>
      </TabItem>

      <TabItem Header='Pagine e layout'>
        <ScrollViewer VerticalScrollBarVisibility='Auto'>
        <DockPanel Margin='22,18,22,16'>
          <StackPanel DockPanel.Dock='Top'>
            <Grid Margin='0,4,0,12'><Grid.ColumnDefinitions><ColumnDefinition Width='Auto'/><ColumnDefinition Width='*'/></Grid.ColumnDefinitions><TextBlock Text='GIOCO' FontWeight='Bold' FontSize='14' Foreground='White'/><Border Grid.Column='1' Height='1' Background='#FF4A4A4A' VerticalAlignment='Center' Margin='12,0,0,0'/></Grid>
            <WrapPanel Margin='24,0,0,4'>
              <ComboBox x:Name='CbGame' Width='240' Margin='0,2,10,2' VerticalContentAlignment='Center'/>
              <Button x:Name='BtnGameCreate' Content='Crea pagine per questo gioco' Margin='0,2,6,2'/>
              <Button x:Name='BtnGameDelete' Content='Usa le pagine predefinite' Margin='0,2,6,2'/>
            </WrapPanel>
            <TextBlock x:Name='GameInfo' Margin='24,2,0,4' TextWrapping='Wrap' Foreground='#FF9A9A9A' FontSize='12'/>
            <CheckBox x:Name='ChkAutoGame' Margin='24,2,0,8' Content='Quando parte un gioco, usa da solo le sue pagine'/>
            <Grid Margin='0,10,0,12'><Grid.ColumnDefinitions><ColumnDefinition Width='Auto'/><ColumnDefinition Width='*'/></Grid.ColumnDefinitions><TextBlock Text='PAGINA' FontWeight='Bold' FontSize='14' Foreground='White'/><Border Grid.Column='1' Height='1' Background='#FF4A4A4A' VerticalAlignment='Center' Margin='12,0,0,0'/></Grid>
            <WrapPanel Margin='24,0,0,4'>
              <ComboBox x:Name='CbPage' Width='180' Margin='0,2,10,2' VerticalContentAlignment='Center'/>
              <TextBox x:Name='TbName' Width='160' Margin='0,2,14,2' ToolTip='Nome della pagina'/>
              <Button x:Name='BtnNew' Content='+ Nuova' Margin='0,2,6,2'/>
              <Button x:Name='BtnDup' Content='Duplica' Margin='0,2,6,2'/>
              <Button x:Name='BtnDel' Content='Elimina' Margin='0,2,6,2'/>
              <Button x:Name='BtnLeft' Content='◀' Margin='0,2,4,2' ToolTip='Sposta a sinistra'/>
              <Button x:Name='BtnRight' Content='▶' Margin='0,2,6,2' ToolTip='Sposta a destra'/>
            </WrapPanel>
            <WrapPanel Margin='24,6,0,0'>
              <TextBlock Text='Si apre da sola in' VerticalAlignment='Center' Foreground='#FF9A9A9A' Margin='0,0,12,0'/>
              <CheckBox x:Name='ChkP' Content='Prove' Margin='0,0,18,0'/>
              <CheckBox x:Name='ChkQ' Content='Qualifica' Margin='0,0,18,0'/>
              <CheckBox x:Name='ChkR' Content='Gara' Margin='0,0,18,0'/>
            </WrapPanel>
            <StackPanel x:Name='PageKeys' Margin='24,8,0,0'/>

            <Grid Margin='0,18,0,0'><Grid.ColumnDefinitions><ColumnDefinition Width='Auto'/><ColumnDefinition Width='*'/></Grid.ColumnDefinitions><TextBlock Text='LAYOUT' FontWeight='Bold' FontSize='14' Foreground='White'/><Border Grid.Column='1' Height='1' Background='#FF4A4A4A' VerticalAlignment='Center' Margin='12,0,0,0'/></Grid>
            <StackPanel Margin='24,0,0,0'>
              <!-- in fila: barre, riquadri in basso, centro e LED -->
              <WrapPanel>
                <StackPanel Margin='0,0,26,8'>
                  <TextBlock Text='Barre' FontSize='11.5' Foreground='#FF9A9A9A' Margin='0,0,0,4'/>
                  <WrapPanel x:Name='FrameButtons' MaxWidth='560'/>
                </StackPanel>
                <StackPanel Margin='0,0,26,8'>
                  <TextBlock Text='Riquadri in basso' FontSize='11.5' Foreground='#FF9A9A9A' Margin='0,0,0,4'/>
                  <WrapPanel x:Name='ToolbarButtons' MaxWidth='420'/>
                </StackPanel>
                <StackPanel Margin='0,0,26,8'>
                  <TextBlock Text='Centro' FontSize='11.5' Foreground='#FF9A9A9A' Margin='0,0,0,4'/>
                  <WrapPanel x:Name='CenterButtons'/>
                </StackPanel>
                <StackPanel Margin='0,0,26,8'>
                  <TextBlock Text='LED dei giri' FontSize='11.5' Foreground='#FF9A9A9A' Margin='0,0,0,4'/>
                  <WrapPanel x:Name='LedButtons'/>
                </StackPanel>
              </WrapPanel>
              <!-- sotto: griglie dei lati e le altre opzioni -->
              <WrapPanel Margin='0,6,0,0'>
                <StackPanel Margin='0,0,28,8'>
                  <TextBlock Text='Griglia lato sinistro' FontSize='11.5' Foreground='#FF9A9A9A' Margin='0,0,0,4'/>
                  <WrapPanel x:Name='GridLButtons' MaxWidth='560'/>
                </StackPanel>
                <StackPanel Margin='0,0,28,8'>
                  <TextBlock Text='Griglia lato destro' FontSize='11.5' Foreground='#FF9A9A9A' Margin='0,0,0,4'/>
                  <WrapPanel x:Name='GridRButtons' MaxWidth='560'/>
                </StackPanel>
              </WrapPanel>
              <StackPanel Margin='0,4,0,0'>
                <CheckBox x:Name='ChkInputs' Content='Ai lati della marcia: acceleratore e freno (invece della barra dei giri)'/>
                <CheckBox x:Name='ChkNoBars' Content='Nascondi le barre ai lati della marcia' Margin='0,6,0,0'/>
              </StackPanel>
            </StackPanel>
            <Grid Margin='0,18,0,0'><Grid.ColumnDefinitions><ColumnDefinition Width='Auto'/><ColumnDefinition Width='*'/></Grid.ColumnDefinitions><TextBlock Text='ANTEPRIMA E SCHEDE' FontWeight='Bold' FontSize='14' Foreground='White'/><Border Grid.Column='1' Height='1' Background='#FF4A4A4A' VerticalAlignment='Center' Margin='12,0,0,0'/></Grid>
          </StackPanel>
          <Grid>
            <Grid.ColumnDefinitions>
              <ColumnDefinition Width='*'/>
              <ColumnDefinition Width='360'/>
            </Grid.ColumnDefinitions>
            <StackPanel Grid.Column='0' Margin='24,0,16,0'>
              <Border CornerRadius='10' Background='#FF000000' BorderBrush='#FF3E3E3E' BorderThickness='1' Padding='6' VerticalAlignment='Top' MaxWidth='1000'>
                <Viewbox Stretch='Uniform'>
                  <Canvas x:Name='Preview' Width='850' Height='480' Background='Black' ClipToBounds='True'/>
                </Viewbox>
              </Border>
              <TextBlock TextWrapping='Wrap' Foreground='#FF9A9A9A' FontSize='11.5' Margin='0,8,0,0'
                         Text=""Clic su un riquadro per sceglierne la scheda a destra  ·  trascina un riquadro su un altro per scambiarli  ·  tasto destro per svuotarlo. Le modifiche si vedono subito sulla dashboard.""/>
            </StackPanel>
            <Border Grid.Column='1' Background='#FF262626' CornerRadius='10' Padding='14' VerticalAlignment='Top' MaxHeight='560'>
              <ScrollViewer VerticalScrollBarVisibility='Auto'>
                <StackPanel x:Name='PickerPanel'/>
              </ScrollViewer>
            </Border>
          </Grid>
        </DockPanel>
        </ScrollViewer>
      </TabItem>

      <TabItem Header='Avvisi'>
        <ScrollViewer VerticalScrollBarVisibility='Auto'>
          <StackPanel x:Name='AlertsPanel' Margin='22,18,22,22' MaxWidth='1100' HorizontalAlignment='Left'/>
        </ScrollViewer>
      </TabItem>

      <TabItem Header='Gomme e freni'>
        <ScrollViewer VerticalScrollBarVisibility='Auto'>
          <StackPanel x:Name='TyresPanel' Margin='22,18,22,22' MaxWidth='1100' HorizontalAlignment='Left'/>
        </ScrollViewer>
      </TabItem>

      <TabItem Header='Tempi e posizioni'>
        <ScrollViewer VerticalScrollBarVisibility='Auto'>
          <StackPanel x:Name='TimingPanel' Margin='22,18,22,22' MaxWidth='1100' HorizontalAlignment='Left'/>
        </ScrollViewer>
      </TabItem>

      <TabItem Header='Aspetto e aggiornamenti'>
        <ScrollViewer VerticalScrollBarVisibility='Auto'>
          <StackPanel x:Name='FontPanel' Margin='22,18,22,22' MaxWidth='1100' HorizontalAlignment='Left'/>
        </ScrollViewer>
      </TabItem>

      <TabItem Header='Valori (dati)'>
        <DockPanel Margin='22,18,22,16'>
          <StackPanel DockPanel.Dock='Top'>
            <Grid Margin='0,4,0,12'><Grid.ColumnDefinitions><ColumnDefinition Width='Auto'/><ColumnDefinition Width='*'/></Grid.ColumnDefinitions><TextBlock Text='VALORI E PROPRIETÀ' FontWeight='Bold' FontSize='14' Foreground='White'/><Border Grid.Column='1' Height='1' Background='#FF4A4A4A' VerticalAlignment='Center' Margin='12,0,0,0'/></Grid>
            <TextBlock x:Name='ValuesInfo' TextWrapping='Wrap' Foreground='#FF9A9A9A' FontSize='11.5' Margin='0,0,0,10'/>
          </StackPanel>
          <WrapPanel DockPanel.Dock='Bottom' Margin='0,10,0,0'>
            <Button x:Name='BtnResetRow' Content='Ripristina riga selezionata' Margin='0,2,8,2'/>
            <Button x:Name='BtnResetAll' Content='Ripristina tutti i valori' Margin='0,2,8,2'/>
          </WrapPanel>
          <DataGrid x:Name='Grid' AutoGenerateColumns='False' CanUserAddRows='False' CanUserDeleteRows='False'
                    CanUserReorderColumns='False' SelectionMode='Single' HeadersVisibility='Column' RowHeight='26'
                    GridLinesVisibility='Horizontal'>
            <DataGrid.Columns>
              <DataGridTextColumn Header='Canale' Binding='{Binding Id, Mode=OneWay}' IsReadOnly='True' Width='100'/>
              <DataGridTextColumn Header='Etichetta' Binding='{Binding Label}' Width='120'/>
              <DataGridTextColumn Header='Proprietà SimHub (sorgente)' Binding='{Binding Source}' Width='*'/>
              <DataGridTextColumn Header='Decimali' Binding='{Binding Decimals}' Width='70'/>
              <DataGridTextColumn Header='Unità' Binding='{Binding Unit}' Width='60'/>
              <DataGridTextColumn Header='Valore attuale' Binding='{Binding Current, Mode=OneWay}' IsReadOnly='True' Width='130'/>
            </DataGrid.Columns>
          </DataGrid>
        </DockPanel>
      </TabItem>

      <TabItem Header='Guida'>
        <ScrollViewer VerticalScrollBarVisibility='Auto'>
          <Border Background='#FF262626' CornerRadius='10' Padding='22' Margin='22,18,22,22' MaxWidth='900' HorizontalAlignment='Left'>
            <TextBlock x:Name='HelpText' TextWrapping='Wrap' FontSize='13' LineHeight='20' Foreground='#FFE0E0E0'/>
          </Border>
        </ScrollViewer>
      </TabItem>
    </TabControl>
  </DockPanel>
</UserControl>";

        static readonly string[] FrameShort = { "Entrambe", "Solo in basso", "Solo laterali", "Nessuna" };

        const string ValuesInfoText =
            "Ogni riga è un valore che le schede possono mostrare. Puoi cambiare l'etichetta (il testo sopra il numero), " +
            "la proprietà SimHub da cui leggere il dato, i decimali e l'unità. La colonna 'Valore attuale' si aggiorna dal vivo: " +
            "usala per controllare che la proprietà sia giusta.\n" +
            "Il nome di una proprietà lo trovi in SimHub (Proprietà disponibili, o Dash Studio > Bindings): copialo e incollalo qui, " +
            "le parentesi [ ] vengono tolte da sole. Sorgenti speciali: @ora, @gap_avanti, @gap_dietro. " +
            "Le righe 'Custom1..6' sono libere e corrispondono alle schede 'Personalizzato 1..6'.";

        const string HelpTextString =
            "PAGINE\n" +
            "Una pagina è una disposizione completa della dashboard (cornice, griglie, schede). Puoi crearne quante vuoi e " +
            "passare dall'una all'altra in gara con un tasto del volante.\n\n" +
            "COME SI USA L'EDITOR\n" +
            "Clic su un riquadro dell'anteprima: a destra compare la tavolozza delle schede divise per categoria; un clic sulla scheda la mette nel riquadro. " +
            "Trascina un riquadro su un altro per scambiarli. Tasto destro su un riquadro per svuotarlo. " +
            "Sopra scegli le barre (laterali/in basso), quanti riquadri in basso e la griglia di ciascun lato con le miniature.\n\n" +
            "ATTIVA DA SOLA IN\n" +
            "Spunta Prove / Qualifica / Gara e quella pagina si apre automaticamente quando entri in quel tipo di sessione.\n\n" +
            "TASTI DEL VOLANTE\n" +
            "Sotto il nome della pagina: clic su \"Pagina dopo\" o \"Pagina prima\" e premi il tasto sul volante o sulla button box. " +
            "Oppure da SimHub > Controlli ed eventi > Nuova mappatura > scegli l'azione:\n" +
            "   DTPlugin.PaginaSuccessiva / DTPlugin.PaginaPrecedente\n" +
            "   DTPlugin.VaiAPagina1 ... VaiAPagina6\n" +
            "   DTPlugin.MostraStrategia (avviso strategia gara)\n" +
            "e premi il tasto da usare. Al cambio pagina la dashboard mostra per un attimo il nome della pagina.\n\n" +
            "CORNICE E GRIGLIE\n" +
            "Cornice = se ci sono le barre laterali verticali e/o la barra in basso. Ogni lato (sinistro/destro) ha la sua griglia " +
            "(riquadri grandi, medi, piccoli, righe). Al centro restano sempre delta, marcia, giri e velocità, più 3 righe a scelta.\n\n" +
            "SCHEDE\n" +
            "Ogni riquadro può mostrare una qualunque scheda; la scheda si adatta da sola alla misura del riquadro. " +
            "Le schede leggono i valori della scheda 'Valori (dati)': se un numero non compare, controlla lì la proprietà.\n\n" +
            "PROPRIETÀ PER LE TUE DASH\n" +
            "Ogni valore è disponibile anche come proprietà: DTPlugin.<Canale> (testo pronto), DTPlugin.<Canale>_Valore (numero), " +
            "DTPlugin.<Canale>_Nome (etichetta). Esempio: [DTPlugin.BB], [DTPlugin.Fuel_Valore].\n\n" +
            "PROBLEMI\n" +
            "Il plugin scrive un registro in PluginsData\\Common\\DTPlugin.log (cartella di SimHub). " +
            "Le impostazioni sono in PluginsData\\Common\\DTPlugin.json.";

        readonly DTPlugin plugin;
        readonly ObservableCollection<ChannelRow> rows = new ObservableCollection<ChannelRow>();
        public readonly object Root;
        string uiGame = "";
        readonly PreviewRenderer previewRenderer = new PreviewRenderer();
        bool realPreview, previewErrorLogged;
        readonly dynamic root, tabs, cbPage, tbName, chkP, chkQ, chkR, chkInputs, chkNoBars, cbGame, gameInfo, chkAutoGame, centerButtons, ledButtons, frameButtons, toolbarButtons,
                         gridLButtons, gridRButtons, preview, pickerPanel, grid, timer, alertsPanel, pageKeys, tyresPanel, idlePanel, idlePreview;
        dynamic tyreLive;
        bool updating;

        // riquadro selezionato (L, R, C, T, SL, SR) e trascinamento
        string selKind = "";
        int selIndex = -1;
        bool pressed;
        string pressKind;
        int pressIndex;
        double pressX, pressY;

        FedeSettings S { get { return plugin.Settings; } }
        PageSettings Page { get { return plugin.Page; } }

        /// <summary>
        /// SimHub senza licenza mette il pannello in uno spazio senza limite di altezza: le barre di scorrimento
        /// interne non servono e non si scorre. Limito l'altezza del pannello a quella della finestra di SimHub.
        /// </summary>
        void FitToWindow()
        {
            try
            {
                dynamic r = Root;
                Action fit = () =>
                {
                    try
                    {
                        var wt = Wpf.T("System.Windows.Window");
                        dynamic win = wt.GetMethod("GetWindow").Invoke(null, new object[] { Root });
                        if (win == null) return;
                        double top = 150;
                        try
                        {
                            var pt = Wpf.T("System.Windows.Point");
                            object zero = Activator.CreateInstance(pt, 0.0, 0.0);
                            dynamic p = Root.GetType().GetMethod("TranslatePoint").Invoke(Root, new object[] { zero, (object)win.Content });
                            double y = (double)p.Y;
                            if (y >= 0 && y < (double)win.ActualHeight) top = y;
                        }
                        catch { }
                        double h = (double)win.ActualHeight - top - 48;
                        if (h > 300 && !double.IsNaN(h)) r.MaxHeight = h;
                    }
                    catch { }
                };
                Wpf.On(Root, "Loaded", (s, e) =>
                {
                    fit();
                    try
                    {
                        var wt = Wpf.T("System.Windows.Window");
                        object win = wt.GetMethod("GetWindow").Invoke(null, new object[] { Root });
                        if (win != null) Wpf.On(win, "SizeChanged", (s2, e2) => fit());
                    }
                    catch { }
                });
            }
            catch (Exception ex) { FedeLog.Error("altezza pannello", ex); }
        }

        public SettingsUi(DTPlugin plugin)
        {
            this.plugin = plugin;
            // pannello con lo stile grafico; se lo stile non si carica, pannello con l'aspetto standard
            string mainXaml = MainXaml.Replace("{LOGO}", Logo.Geometry);
            try
            {
                int at = mainXaml.IndexOf('>', mainXaml.IndexOf("<UserControl")) + 1;
                Root = (object)Wpf.Xaml(mainXaml.Substring(0, at) + "<UserControl.Resources>" + UiStyles.Dictionary + "</UserControl.Resources>" + mainXaml.Substring(at));
            }
            catch (Exception ex)
            {
                FedeLog.Error("stile grafico (uso l'aspetto standard)", ex);
                Root = (object)Wpf.Xaml(mainXaml);
            }
            root = Root;
            FitToWindow();
            F("VersionText").Text = "DT Plugin  v" + DTPlugin.VersionText;
            Click("BtnExport", ExportSettings);
            Click("BtnCheckUpdate", () => { plugin.Updater.CheckAsync(DTPlugin.VersionText, true); ShowUpdate(true); });
            Click("BtnUpdate", () => plugin.Updater.InstallAsync());
            Click("BtnUnlock", TryUnlock);
            Wpf.On((object)F("LockCode"), "KeyDown", (s2, e2) => { try { if (((dynamic)e2).Key.ToString() == "Return") TryUnlock(); } catch { } });
            Click("BtnReload", () => { Save(); FedeLog.Write("Riavvio richiesto per ricaricare la dashboard"); plugin.Updater.RestartSimHub(); });
            Click("BtnImport", ImportSettings);
            Click("BtnReset", ResetEverything);
            tabs = F("Tabs");
            cbPage = F("CbPage");
            tbName = F("TbName");
            chkP = F("ChkP");
            chkQ = F("ChkQ");
            chkR = F("ChkR");
            chkInputs = F("ChkInputs");
            chkNoBars = F("ChkNoBars");
            cbGame = F("CbGame"); gameInfo = F("GameInfo"); chkAutoGame = F("ChkAutoGame");
            frameButtons = F("FrameButtons");
            toolbarButtons = F("ToolbarButtons");
            centerButtons = F("CenterButtons");
            ledButtons = F("LedButtons");
            gridLButtons = F("GridLButtons");
            gridRButtons = F("GridRButtons");
            preview = F("Preview");
            pickerPanel = F("PickerPanel");
            grid = F("Grid");
            alertsPanel = F("AlertsPanel");
            pageKeys = F("PageKeys");
            tyresPanel = F("TyresPanel");
            idlePanel = F("IdlePanel");
            idlePreview = F("IdlePreview");
            F("ValuesInfo").Text = ValuesInfoText;
            F("HelpText").Text = HelpTextString;

            Wpf.On((object)cbPage, "SelectionChanged", (s, e) =>
            {
                if (updating) return;
                int i = cbPage.SelectedIndex;
                if (i < 0) return;
                selKind = ""; selIndex = -1;
                plugin.SetActivePage(i, true);
                Changed();
            });
            Wpf.On((object)tbName, "LostFocus", (s, e) => RenamePage());
            Wpf.On((object)tbName, "KeyDown", (s, e) =>
            {
                dynamic de = e;
                if ((int)de.Key == 6) RenamePage(); // Invio
            });
            OnCheck((object)chkP, v => Page.AutoPractice = v);
            OnCheck((object)chkQ, v => Page.AutoQualify = v);
            OnCheck((object)chkR, v => Page.AutoRace = v);
            OnCheck((object)chkInputs, v => Page.ShowInputs = v);
            OnCheck((object)chkNoBars, v => Page.HideSideBars = v);
            OnCheck((object)chkAutoGame, v => S.AutoGamePages = v);
            foreach (var n in GameProfile.Names) cbGame.Items.Add(n);
            uiGame = S.ProfileGame;
            Wpf.On((object)cbGame, "SelectionChanged", (s, e) =>
            {
                if (updating) return;
                int gi = (int)cbGame.SelectedIndex;
                if (gi < 0) return;
                uiGame = GameProfile.Keys[gi];
                S.SwitchProfile(uiGame);   // se il gioco non ha pagine proprie restano quelle predefinite
                plugin.SetActivePage(S.ActivePage, false);
                Changed();
            });
            Click("BtnGameCreate", () =>
            {
                if (uiGame.Length == 0) return;
                S.CreateProfile(uiGame);
                plugin.SetActivePage(S.ActivePage, false);
                Changed();
            });
            Click("BtnGameDelete", () =>
            {
                if (uiGame.Length == 0 || !S.HasProfile(uiGame)) return;
                S.DeleteProfile(uiGame);
                plugin.SetActivePage(S.ActivePage, false);
                Changed();
            });

            Click("BtnNew", () => AddPage(false));
            Click("BtnDup", () => AddPage(true));
            Click("BtnDel", DeletePage);
            Click("BtnLeft", () => MovePage(-1));
            Click("BtnRight", () => MovePage(1));
            Click("BtnResetRow", ResetSelected);
            Click("BtnResetAll", ResetAll);

            foreach (var rt in plugin.Runtime) rows.Add(new ChannelRow(rt));
            grid.ItemsSource = rows;
            Wpf.On((object)grid, "CellEditEnding", (s, e) => root.Dispatcher.BeginInvoke(new Action(Save)));

            timer = Activator.CreateInstance(Wpf.T("System.Windows.Threading.DispatcherTimer"));
            timer.Interval = TimeSpan.FromMilliseconds(300);
            Wpf.On((object)timer, "Tick", (s, e) =>
            {
                string tab = "";
                try { object si = tabs.SelectedItem; if (si != null) tab = Convert.ToString(((dynamic)si).Header); } catch { }
                ShowLock();
                try { F("ReloadBar").Visibility = Wpf.Visibility(plugin.DashBuilder.NeedsReload && !plugin.DashBuilder.Working); } catch { }
                if (tab == "Gomme e freni") UpdateTyreLive();
                if (tab == "Tempi e posizioni") UpdateTimingLive();
                if (tab != "Valori (dati)") return;
                foreach (var r in rows) r.Refresh();
            });
            Action upd = () => root.Dispatcher.BeginInvoke(new Action(() => ShowUpdate(false)));
            Wpf.On(Root, "Loaded", (s, e) => { plugin.Updater.Changed += upd; ShowUpdate(false); });
            Wpf.On(Root, "Unloaded", (s, e) => { plugin.Updater.Changed -= upd; });
            Wpf.On(Root, "Loaded", (s, e) => { ShowLock(); timer.Start(); plugin.PageChanged += OnPageChanged; RefreshAll(); BuildAlerts(); BuildTyres(); BuildIdle(); BuildTiming(); BuildFont(); });
            Wpf.On(Root, "Unloaded", (s, e) => { timer.Stop(); plugin.PageChanged -= OnPageChanged; });
        }

        bool unlocking;

        void TryUnlock()
        {
            if (unlocking) return;
            string code = "";
            try { code = (string)F("LockCode").Password; } catch { }
            if (code.Trim().Length == 0) return;
            unlocking = true;
            try { F("LockText").Text = "Verifico il codice…"; F("BtnUnlock").IsEnabled = false; } catch { }
            new System.Threading.Thread(() =>
            {
                bool ok = false;
                try { ok = plugin.Access.TryUnlock(code); } catch (Exception ex) { FedeLog.Error("sblocco", ex); }
                root.Dispatcher.BeginInvoke(new Action(() =>
                {
                    unlocking = false;
                    try { F("BtnUnlock").IsEnabled = true; if (ok) F("LockCode").Password = ""; } catch { }
                    ShowLock();
                }));
            }) { IsBackground = true }.Start();
        }

        bool lastLocked;

        /// <summary>Senza codice valido: barra rossa con la casella del codice e schede disattivate.</summary>
        void ShowLock()
        {
            try
            {
                var a = plugin.Access;
                bool locked = a != null && !a.Unlocked;
                F("LockBar").Visibility = Wpf.Visibility(locked);
                if (locked && !unlocking) F("LockText").Text = a.Status;
                if (locked != lastLocked || locked) F("Tabs").IsEnabled = !locked;
                lastLocked = locked;
            }
            catch (Exception ex) { FedeLog.Error("barra accesso", ex); }
        }

        bool updateShowAlways;

        /// <summary>Barra in alto: aggiornamento disponibile / in corso / installato (o esito del controllo manuale).</summary>
        void ShowUpdate(bool manual)
        {
            try
            {
                if (manual) updateShowAlways = true;
                var u = plugin.Updater;
                // prima la riga di stato (cosi' si vede sempre, anche se il resto non va)
                try { F("UpdateStatus").Text = u.Status.Length > 0 ? "Aggiornamenti: " + u.Status : ""; } catch (Exception ex) { FedeLog.Error("stato aggiornamenti", ex); }
                dynamic bar = F("UpdateBar"), txt = F("UpdateText"), btn = F("BtnUpdate");
                bool show = u.Available || u.Installed || u.Busy || (updateShowAlways && u.Status.Length > 0);
                bar.Visibility = Wpf.Visibility(show);
                txt.Text = u.Available && !u.Busy ? "Nuova versione " + u.LatestVersion + " disponibile su GitHub (tu hai la " + DTPlugin.VersionText + ")." : u.Status;
                btn.Visibility = Wpf.Visibility(u.Available && !u.Busy);
            }
            catch (Exception ex) { FedeLog.Error("barra aggiornamenti", ex); }
        }

        dynamic F(string name)
        {
            object el = Wpf.Find(Root, name);
            if (el == null) throw new MissingMemberException("XAML", name);
            return el;
        }

        void Click(string name, Action action)
        {
            Wpf.On((object)F(name), "Click", (s, e) => action());
        }

        void OnCheck(object box, Action<bool> setter)
        {
            Action<object, object> h = (s, e) =>
            {
                if (updating) return;
                bool? v = ((dynamic)box).IsChecked;
                setter(v == true);
                Changed();
            };
            Wpf.On(box, "Checked", h);
            Wpf.On(box, "Unchecked", h);
        }

        void OnPageChanged()
        {
            try { root.Dispatcher.BeginInvoke(new Action(RefreshAll)); } catch { }
        }

        void Save()
        {
            plugin.SaveSettings();
        }

        void Changed()
        {
            Save();
            root.Dispatcher.BeginInvoke(new Action(RefreshAll));
        }

        string learningKey;   // quale tasto pagina si sta assegnando ("next" / "prev"), null = nessuno

        void BuildPageKeys()
        {
            try
            {
                pageKeys.Children.Clear();
                dynamic r = Wpf.Xaml("<WrapPanel {NS} Margin='0,0,0,4'/>");
                r.Children.Add(Wpf.Xaml("<TextBlock {NS} Text='Tasti volante per cambiare pagina' VerticalAlignment='Center' Foreground='#FF9A9A9A' Margin='0,0,12,0'/>"));
                AddKey((object)r, "next", "Pagina dopo", () => S.PageNextKey, k => S.PageNextKey = k);
                AddKey((object)r, "prev", "Pagina prima", () => S.PagePrevKey, k => S.PagePrevKey = k);
                pageKeys.Children.Add(r);
            }
            catch (Exception ex) { FedeLog.Error("tasti pagina", ex); }
        }

        void AddKey(object rowObj, string id, string what, Func<KeyBinding> get, Action<KeyBinding> set)
        {
            dynamic row = rowObj;
            var k = get();
            bool learning = learningKey == id && plugin.LearnButton != null;
            string txt = learning ? what + ": premi il tasto..." : what + ": " + (k == null || string.IsNullOrEmpty(k.Device) ? "nessuno" : "tasto " + k.Button);
            dynamic b = Seg(txt, learning, true, () =>
            {
                if (learning) { plugin.LearnButton = null; learningKey = null; }
                else
                {
                    learningKey = id;
                    plugin.LearnButton = (dev, name, btn) =>
                    {
                        set(new KeyBinding { Device = dev, Name = name, Button = btn });
                        learningKey = null;
                        Save();
                        root.Dispatcher.BeginInvoke(new Action(BuildPageKeys));
                    };
                }
                BuildPageKeys();
            });
            if (k != null && !string.IsNullOrEmpty(k.Device)) b.ToolTip = (k.Name ?? "") + " - tasto " + k.Button + " (clic per cambiarlo)";
            row.Children.Add(b);
            if (k != null && !string.IsNullOrEmpty(k.Device) && !learning)
                row.Children.Add(Seg("x", false, true, () => { set(null); Save(); BuildPageKeys(); }));
        }

        void RefreshAll()
        {
            updating = true;
            try
            {
                BuildPageKeys();
                var p = Page;
                // gioco: quello scelto qui, oppure (se le pagine sono cambiate da sole) quello in uso
                if (S.ProfileGame.Length > 0) uiGame = S.ProfileGame;
                else if (S.HasProfile(uiGame) && uiGame.Length > 0) uiGame = "";
                cbGame.SelectedIndex = Math.Max(0, Array.IndexOf(GameProfile.Keys, uiGame));
                bool own = uiGame.Length > 0 && S.ProfileGame == uiGame;
                gameInfo.Text = uiGame.Length == 0 ? "Pagine usate da tutti i giochi che non hanno pagine proprie."
                              : (own ? "Stai modificando le pagine di " + GameProfile.NameOf(uiGame) + "."
                                     : GameProfile.NameOf(uiGame) + " usa le pagine predefinite (quelle sotto). Premi \"Crea pagine per questo gioco\" per fargli le sue.");
                ((dynamic)F("BtnGameCreate")).IsEnabled = uiGame.Length > 0 && !own;
                ((dynamic)F("BtnGameDelete")).IsEnabled = own;
                chkAutoGame.IsChecked = S.AutoGamePages;
                cbPage.Items.Clear();
                foreach (var pg in S.Pages) cbPage.Items.Add(pg.Name);
                cbPage.SelectedIndex = S.ActivePage;
                if (!(bool)tbName.IsKeyboardFocusWithin) tbName.Text = p.Name;
                chkP.IsChecked = p.AutoPractice;
                chkQ.IsChecked = p.AutoQualify;
                chkR.IsChecked = p.AutoRace;
                chkInputs.IsChecked = p.ShowInputs;
                chkNoBars.IsChecked = p.HideSideBars;
                BuildLayoutButtons();
                DrawPreview();
                BuildPicker();
            }
            catch (Exception ex)
            {
                FedeLog.Error("aggiornamento impostazioni", ex);
            }
            finally
            {
                updating = false;
            }
        }

        // ---------------------------------------------------------------- pulsanti layout
        static string BtnColors(bool selected)
        {
            return selected ? " Background='" + Accent + "' Foreground='White' BorderBrush='" + Accent + "'" : "";
        }

        void BuildLayoutButtons()
        {
            var p = Page;
            frameButtons.Children.Clear();
            for (int i = 0; i < FrameShort.Length; i++)
            {
                int idx = i;
                // come le griglie dei lati: solo i blocchi (barre laterali e barra in basso)
                dynamic b = Wpf.Xaml(IconButton(Catalog.Frames[i], p.Frame == i, 68, 40, FrameBlocks(i)));
                Wpf.On((object)b, "Click", (s, e) => { Page.Frame = idx; Changed(); });
                frameButtons.Children.Add(b);
            }

            toolbarButtons.Children.Clear();
            bool hasToolbar = Catalog.FrameHasToolbar[p.Frame];
            for (int i = 0; i < Catalog.Toolbars.Length; i++)
            {
                int idx = i;
                bool sel = hasToolbar && p.Toolbar == i;
                dynamic b = Wpf.Xaml(IconButton(Catalog.Toolbars[i], sel, 68, 16, ToolbarBlocks(p.EffectiveFrame, i)));
                b.IsEnabled = hasToolbar;
                Wpf.On((object)b, "Click", (s, e) => { Page.Toolbar = idx; Changed(); });
                toolbarButtons.Children.Add(b);
            }

            ledButtons.Children.Clear();
            for (int i = 0; i < 2; i++)
            {
                bool on = i == 1;
                dynamic b = Wpf.Xaml(IconButton(on ? "Con i LED dei giri in alto (tutta la pagina scende un po')" : "Senza LED", p.Leds == on, 68, 40, LedBlocks(on)));
                Wpf.On((object)b, "Click", (s, e) => { Page.Leds = on; Changed(); });
                ledButtons.Children.Add(b);
            }

            centerButtons.Children.Clear();
            for (int i = 0; i < Catalog.CenterLayouts.Length; i++)
            {
                int idx = i;
                dynamic b = Wpf.Xaml(IconButton(Catalog.CenterLayouts[i], p.CenterLayout == i, 20, 48, CenterBlocks(p.EffectiveFrame, i)));
                Wpf.On((object)b, "Click", (s, e) => { Page.CenterLayout = idx; selKind = ""; selIndex = -1; Changed(); });
                centerButtons.Children.Add(b);
            }

            FillGridButtons((object)gridLButtons, p.GridLeft, v => Page.GridLeft = v);
            FillGridButtons((object)gridRButtons, p.GridRight, v => Page.GridRight = v);
        }

        /// <summary>Pulsante con il disegnino dei blocchi (stile delle griglie dei lati). blocks = rettangoli in 0..1.</summary>
        static string IconButton(string tip, bool selected, double w, double h, List<double[]> blocks)
        {
            var sb = new StringBuilder();
            sb.Append("<Button {NS} Padding='4' Margin='0,0,4,4' ToolTip='" + Wpf.Esc(tip) + "'");
            sb.Append(selected ? " Background='" + Accent + "' BorderBrush='" + Accent + "'>" : ">");
            sb.Append("<Canvas Width='" + Wpf.N(w) + "' Height='" + Wpf.N(h) + "'>");
            foreach (var r in blocks)
            {
                double x = r[0] * w, y = r[1] * h, bw = Math.Max(1.5, r[2] * w - 1.5), bh = Math.Max(1.5, r[3] * h - 1.5);
                sb.Append("<Border Canvas.Left='" + Wpf.N(x) + "' Canvas.Top='" + Wpf.N(y) + "' Width='" + Wpf.N(bw) + "' Height='" + Wpf.N(bh) +
                          "' CornerRadius='2' Background='" + (selected ? "#FFFFFFFF" : "#FFB0B0B0") + "'/>");
            }
            sb.Append("</Canvas></Button>");
            return sb.ToString();
        }

        static List<double[]> Scale(IEnumerable<int[]> rects, double x0, double y0, double w, double h)
        {
            var l = new List<double[]>();
            foreach (var r in rects) l.Add(new[] { (r[0] - x0) / w, (r[1] - y0) / h, r[2] / w, r[3] / h });
            return l;
        }

        /// <summary>Barre: barre laterali + barra in basso (una o due righe), su tutta la pagina.</summary>
        static List<double[]> FrameBlocks(int f)
        {
            var l = new List<int[]>();
            if (Catalog.FrameHasSidebars[f]) l.AddRange(Catalog.SidebarRects[f]);
            if (Catalog.FrameHasToolbar[f])
            {
                var t = Catalog.ToolbarRects[f][Catalog.Toolbars.Length - 1];   // "1 riquadro largo": la barra intera
                l.AddRange(t);
            }
            return Scale(l, 0, 0, 850, 480);
        }

        /// <summary>LED: fila di pallini in alto e la pagina che scende; senza LED solo la pagina.</summary>
        static List<double[]> LedBlocks(bool on)
        {
            var l = new List<double[]>();
            double top = 0;
            if (on)
            {
                const int n = 8;
                double d = 0.075, gap = (1.0 - n * d) / (n - 1);
                for (int i = 0; i < n; i++) l.Add(new[] { i * (d + gap), 0.0, d, 0.13 });
                top = 0.24;
            }
            l.Add(new[] { 0.0, top, 1.0, 1.0 - top });
            return l;
        }

        /// <summary>Riquadri in basso: solo la fila dei riquadri.</summary>
        static List<double[]> ToolbarBlocks(int f, int t)
        {
            int ff = Catalog.FrameHasToolbar[f] ? f : 1;
            var rs = Catalog.ToolbarRects[ff][t];
            if (rs.Length == 0) return new List<double[]>();
            int y0 = int.MaxValue, y1 = 0;
            foreach (var r in rs) { y0 = Math.Min(y0, r[1]); y1 = Math.Max(y1, r[1] + r[3]); }
            var row = new List<int[]>();
            foreach (var r in rs) if (r[1] == rs[0][1]) row.Add(r);   // solo la riga di riquadri (non il largo della doppia)
            int h0 = row[0][3];
            return Scale(row, 0, row[0][1], 850, h0);
        }

        /// <summary>Centro: delta, marcia e le righe sotto.</summary>
        static List<double[]> CenterBlocks(int f, int c)
        {
            var r = Catalog.CenterRects[f][c];
            var l = new List<int[]>();
            for (int i = 0; i < r.Length; i++) if (i != 2) l.Add(r[i]);
            int y0 = r[0][1], y1 = 0;
            foreach (var x in l) y1 = Math.Max(y1, x[1] + x[3]);
            return Scale(l, r[0][0], y0, r[0][2], y1 - y0);
        }

        /// <summary>Disegnino della pagina (64x36): centro, lati, barre laterali e riquadri in basso. onlyBottom = evidenzia la barra in basso.</summary>
        static string MiniLayout(int f, int tb, bool selected, bool onlyBottom)
        {
            const double k = 64.0 / 850.0;
            string on = selected ? "#FFFFFFFF" : "#FFB0B0B0", dim = selected ? "#80FFFFFF" : "#FF5A5A5A";
            var sb = new StringBuilder("<Canvas Width='64' Height='36' HorizontalAlignment='Center' Background='#FF101010'>");
            Action<int[], string> box = (r, col) =>
            {
                double x = r[0] * k, y = r[1] * k, w = Math.Max(1, r[2] * k - 0.6), h = Math.Max(1, r[3] * k - 0.6);
                sb.Append("<Border Canvas.Left='" + Wpf.N(x) + "' Canvas.Top='" + Wpf.N(y) + "' Width='" + Wpf.N(w) + "' Height='" + Wpf.N(h) +
                          "' CornerRadius='1' Background='" + col + "'/>");
            };
            try
            {
                string side = onlyBottom ? dim : on;
                var c = Catalog.CenterRects[f][0];
                for (int i = 0; i < c.Length; i++) if (i != 2) box(c[i], side);
                var g = Catalog.GridRects[f][Catalog.Grids.Length - 1];   // "Unico": tutta la zona dei lati
                foreach (var r in g[0]) box(r, side);
                foreach (var r in g[1]) box(r, side);
                if (Catalog.FrameHasSidebars[f]) foreach (var r in Catalog.SidebarRects[f]) box(r, side);
                if (Catalog.FrameHasToolbar[f] && tb >= 0 && tb < Catalog.ToolbarRects[f].Length)
                    foreach (var r in Catalog.ToolbarRects[f][tb]) box(r, on);
                if (f >= Catalog.LedOffset) box(new[] { 9, 5, 832, 26 }, side);
            }
            catch { }
            sb.Append("</Canvas>");
            return sb.ToString();
        }

        void FillGridButtons(object panelObj, int selected, Action<int> setter)
        {
            dynamic panel = panelObj;
            panel.Children.Clear();
            for (int g = 0; g < Catalog.Grids.Length; g++)
            {
                int idx = g;
                var sb = new StringBuilder();
                sb.Append("<Button {NS} Width='46' Height='60' Padding='4' Margin='0,0,4,4' ToolTip='" + Wpf.Esc(Catalog.Grids[g]) + "'");
                sb.Append(g == selected ? " Background='" + Accent + "' BorderBrush='" + Accent + "'>" : ">");
                sb.Append("<Canvas Width='34' Height='48'>");
                foreach (var u in Catalog.GridUnits[g])
                {
                    double x = u[0] * 17 + 1, y = u[1] * 12 + 1, w = u[2] * 17 - 2, h = u[3] * 12 - 2;
                    sb.Append("<Border Canvas.Left='" + Wpf.N(x) + "' Canvas.Top='" + Wpf.N(y) + "' Width='" + Wpf.N(w) + "' Height='" + Wpf.N(h) +
                              "' CornerRadius='2' Background='" + (g == selected ? "#FFFFFFFF" : "#FFB0B0B0") + "'/>");
                }
                sb.Append("</Canvas></Button>");
                dynamic b = Wpf.Xaml(sb.ToString());
                Wpf.On((object)b, "Click", (s, e) => { setter(idx); selKind = ""; selIndex = -1; Changed(); });
                panel.Children.Add(b);
            }
        }

        // ---------------------------------------------------------------- riquadri (lettura/scrittura)
        List<int> ListFor(string kind)
        {
            var p = Page;
            switch (kind)
            {
                case "L": return p.Left;
                case "R": return p.Right;
                case "C": return p.Center;
                case "T": return p.Bottom;
                default: return null;
            }
        }

        static bool IsBar(string kind)
        {
            return kind == "SL" || kind == "SR";
        }

        int GetSlot(string kind, int index)
        {
            if (kind == "SL") return Page.SidebarLeft;
            if (kind == "SR") return Page.SidebarRight;
            if (kind == "D") return Page.CenterTop;
            var l = ListFor(kind);
            return (l != null && index >= 0 && index < l.Count) ? l[index] : 0;
        }

        void SetSlot(string kind, int index, int value)
        {
            if (kind == "SL") { Page.SidebarLeft = value; return; }
            if (kind == "D") { Page.CenterTop = value; return; }
            if (kind == "SR") { Page.SidebarRight = value; return; }
            var l = ListFor(kind);
            if (l != null && index >= 0 && index < l.Count) l[index] = value;
        }

        static string Tag(string kind, int index)
        {
            switch (kind)
            {
                case "L": return "S" + (index + 1);
                case "R": return "D" + (index + 1);
                case "C": return "C" + (index + 1);
                case "D": return "Delta";
                case "T": return "B" + (index + 1);
                case "SL": return "Barra sx";
                default: return "Barra dx";
            }
        }

        static string AreaName(string kind)
        {
            switch (kind)
            {
                case "L": return "Lato sinistro";
                case "R": return "Lato destro";
                case "C": return "Centro";
                case "D": return "Centro in alto (vuoto = delta)";
                case "T": return "Barra inferiore";
                default: return "Barra laterale";
            }
        }

        // ---------------------------------------------------------------- anteprima
        void DrawPreview()
        {
            preview.Children.Clear();
            var p = Page;
            int f = p.EffectiveFrame;
            // anteprima vera (schede disegnate come sulla dash); se non riesce, i riquadri con il nome
            realPreview = false;
            try { realPreview = previewRenderer.Draw((object)preview, p, S.Font); }
            catch (Exception ex) { if (!previewErrorLogged) { previewErrorLogged = true; FedeLog.Error("anteprima vera", ex); } preview.Children.Clear(); realPreview = false; }
            if (!realPreview)
            {
                if (p.Leds) FixedRect(new[] { 9, 5, 832, 26 }, "LED GIRI");
            }
            var c = Catalog.CenterRects[f][Math.Max(0, Math.Min(p.CenterLayout, Catalog.CenterLayouts.Length - 1))];
            if (!realPreview)
            {
                FixedRect(c[1], "MARCIA");
                FixedRect(c[2], "VELOCITÀ");
            }
            SlotRect(c[0], "D", 0);
            for (int i = 0; i + 3 < c.Length; i++) SlotRect(c[3 + i], "C", i);

            var left = Catalog.GridRects[f][p.GridLeft][0];
            for (int i = 0; i < left.Length; i++) SlotRect(left[i], "L", i);
            var right = Catalog.GridRects[f][p.GridRight][1];
            for (int i = 0; i < right.Length; i++) SlotRect(right[i], "R", i);

            if (Catalog.FrameHasToolbar[f])
            {
                var t = Catalog.ToolbarRects[f][p.Toolbar];
                for (int i = 0; i < t.Length; i++) SlotRect(t[i], "T", i);
            }
            if (Catalog.FrameHasSidebars[f])
            {
                var sb = Catalog.SidebarRects[f];
                SlotRect(sb[0], "SL", 0);
                SlotRect(sb[1], "SR", 0);
            }
        }

        static string Pos(int[] r)
        {
            return "Width='" + r[2] + "' Height='" + r[3] + "' Canvas.Left='" + r[0] + "' Canvas.Top='" + r[1] + "'";
        }

        void FixedRect(int[] r, string label)
        {
            dynamic b = Wpf.Xaml("<Border {NS} " + Pos(r) + " Background='#FF282828' CornerRadius='10'>" +
                                 "<TextBlock Text='" + Wpf.Esc(label) + "' Foreground='Gray' FontSize='18' FontWeight='Bold' " +
                                 "HorizontalAlignment='Center' VerticalAlignment='Center'/></Border>");
            preview.Children.Add(b);
        }

        void SlotRect(int[] r, string kind, int index)
        {
            bool bar = IsBar(kind);
            int value = GetSlot(kind, index);
            string[] names = bar ? Catalog.Bars : Catalog.Cards;
            if (value < 0 || value >= names.Length) value = 0;
            bool selected = kind == selKind && index == selIndex;
            string border = selected ? Accent : (value == 0 ? "#FF464646" : "#FFBDBDBD");
            string bg = bar ? (value == 0 ? "#FF121212" : "#FF00E676") : (value == 0 ? "#FF0E0E0E" : "#FF1E1E1E");
            string inner = "";
            if (realPreview)
            {
                // sopra la scheda disegnata: solo il bordo di selezione (trasparente ma cliccabile); "+" sui riquadri vuoti
                border = selected ? Accent : "#00000000";
                bg = value == 0 ? "#FF0E0E0E" : "#01000000";
                if (kind == "D" && value == 0) bg = "#01000000";   // delta predefinito: e' gia' disegnato sotto
                else if (value == 0 && !bar)
                    inner = "<TextBlock Text='+' Foreground='Gray' HorizontalAlignment='Center' VerticalAlignment='Center' FontSize='22' IsHitTestVisible='False'/>";
            }
            else if (!bar)
            {
                string text = value == 0 ? (kind == "D" ? "DELTA" : "+") : names[value];
                double fs = Math.Max(11, Math.Min(22, r[3] / 4.0));
                inner = "<TextBlock Text='" + Wpf.Esc(text) + "' Foreground='" + (value == 0 ? "Gray" : "White") +
                        "' TextWrapping='Wrap' TextAlignment='Center' HorizontalAlignment='Center' VerticalAlignment='Center' FontSize='" +
                        Wpf.N(fs) + "' Margin='4' IsHitTestVisible='False'/>";
            }
            dynamic b = Wpf.Xaml("<Border {NS} " + Pos(r) + " Background='" + bg + "' BorderBrush='" + border +
                                 "' BorderThickness='" + (selected ? "4" : "2") + "' CornerRadius='" + Wpf.N(Math.Min(12, r[3] / 4.0)) +
                                 "' Cursor='Hand' AllowDrop='True' ToolTip='" +
                                 Wpf.Esc(Tag(kind, index) + ": " + names[value]) + "'>" + inner + "</Border>");
            object target = b;

            Wpf.On(target, "MouseLeftButtonDown", (s, e) =>
            {
                dynamic de = e;
                dynamic pt = de.GetPosition(preview);
                pressed = true; pressKind = kind; pressIndex = index;
                pressX = (double)pt.X; pressY = (double)pt.Y;
            });
            Wpf.On(target, "MouseMove", (s, e) =>
            {
                if (!pressed || pressKind != kind || pressIndex != index) return;
                dynamic de = e;
                if (de.LeftButton.ToString() != "Pressed") { pressed = false; return; }
                dynamic pt = de.GetPosition(preview);
                if (Math.Abs((double)pt.X - pressX) < 6 && Math.Abs((double)pt.Y - pressY) < 6) return;
                pressed = false;
                StartDrag(target, kind + "|" + index);
            });
            Wpf.On(target, "MouseLeftButtonUp", (s, e) =>
            {
                if (!pressed) return;
                pressed = false;
                selKind = kind; selIndex = index;   // clic semplice: seleziona, mostra la tavolozza e il menu li' dove hai cliccato
                root.Dispatcher.BeginInvoke(new Action(RefreshAll));
                root.Dispatcher.BeginInvoke(new Action(() => OpenSlotMenu(kind, index)));
            });
            Wpf.On(target, "MouseRightButtonUp", (s, e) =>
            {
                SetSlot(kind, index, 0);
                Changed();
            });
            Wpf.On(target, "Drop", (s, e) =>
            {
                dynamic de = e;
                string data = null;
                try { data = (string)de.Data.GetData(typeof(string)); } catch { }
                if (string.IsNullOrEmpty(data)) return;
                var parts = data.Split('|');
                if (parts.Length != 2) return;
                string fromKind = parts[0];
                int fromIndex;
                if (!int.TryParse(parts[1], out fromIndex)) return;
                if (fromKind == kind && fromIndex == index) return;
                if (IsBar(fromKind) != IsBar(kind)) return; // barre e schede non si mescolano
                int a = GetSlot(fromKind, fromIndex), bv = GetSlot(kind, index);
                SetSlot(fromKind, fromIndex, bv);
                SetSlot(kind, index, a);
                selKind = kind; selIndex = index;
                Changed();
            });
            preview.Children.Add(b);
        }

        /// <summary>Menu sul riquadro cliccato: categorie con le schede (o le barre), la scheda attuale spuntata.</summary>
        void OpenSlotMenu(string kind, int index)
        {
            try
            {
                bool bar = IsBar(kind);
                int current = GetSlot(kind, index);
                string[] names = bar ? Catalog.Bars : Catalog.Cards;
                dynamic menu = Wpf.Xaml("<ContextMenu {NS} MaxHeight='560'/>");
                dynamic head = Wpf.Xaml("<MenuItem {NS} IsEnabled='False' FontWeight='Bold'/>");
                head.Header = Tag(kind, index) + " · " + AreaName(kind);
                menu.Items.Add(head);
                dynamic clear = Wpf.Xaml("<MenuItem {NS}/>");
                clear.Header = kind == "D" ? "Torna al delta" : "Svuota riquadro";
                Wpf.On((object)clear, "Click", (s, e) => { SetSlot(kind, index, 0); Changed(); });
                menu.Items.Add(clear);
                menu.Items.Add(Wpf.Xaml("<Separator {NS}/>"));
                Func<int, object> item = v =>
                {
                    dynamic mi = Wpf.Xaml("<MenuItem {NS}/>");
                    mi.Header = names[v];
                    mi.IsChecked = v == current;
                    Wpf.On((object)mi, "Click", (s, e) => { SetSlot(kind, index, v); Changed(); });
                    return mi;
                };
                if (bar)
                {
                    for (int v = 1; v < names.Length; v++) menu.Items.Add((dynamic)item(v));
                }
                else
                {
                    for (int c = 0; c < Catalog.Categories.Length; c++)
                    {
                        dynamic sub = Wpf.Xaml("<MenuItem {NS}/>");
                        sub.Header = Catalog.Categories[c];
                        bool has = false;
                        foreach (var v in Catalog.CategoryCards[c]) { if (v == current) has = true; sub.Items.Add((dynamic)item(v)); }
                        if (has) sub.FontWeight = Wpf.Xaml("<TextBlock {NS} FontWeight='Bold'/>").FontWeight;
                        menu.Items.Add(sub);
                    }
                }
                menu.PlacementTarget = preview;
                menu.Placement = (dynamic)Enum.Parse(Wpf.T("System.Windows.Controls.Primitives.PlacementMode"), "MousePoint");
                menu.IsOpen = true;
            }
            catch (Exception ex) { FedeLog.Error("menu del riquadro", ex); }
        }

        static void StartDrag(object source, string data)
        {
            try
            {
                var dd = Wpf.T("System.Windows.DragDrop");
                var effects = Wpf.T("System.Windows.DragDropEffects");
                MethodInfo m = null;
                foreach (var x in dd.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    if (x.Name == "DoDragDrop" && x.GetParameters().Length == 3) { m = x; break; }
                if (m != null) m.Invoke(null, new object[] { source, data, Enum.Parse(effects, "Move") });
            }
            catch (Exception ex) { FedeLog.Error("trascinamento", ex); }
        }

        // ---------------------------------------------------------------- tavolozza schede
        void BuildPicker()
        {
            pickerPanel.Children.Clear();
            if (selKind.Length == 0)
            {
                pickerPanel.Children.Add(Wpf.Xaml("<TextBlock {NS} TextWrapping='Wrap' Foreground='#FFD0D0D0' FontSize='13' Text='" +
                    Wpf.Esc("Clicca un riquadro nell'anteprima per scegliere cosa mostrare.\n\nPuoi anche trascinare un riquadro sopra un altro per scambiarli, " +
                            "e usare il tasto destro per svuotarlo.") + "'/>"));
                return;
            }
            bool bar = IsBar(selKind);
            int current = GetSlot(selKind, selIndex);
            string[] names = bar ? Catalog.Bars : Catalog.Cards;
            if (current < 0 || current >= names.Length) current = 0;

            pickerPanel.Children.Add(Wpf.Xaml("<TextBlock {NS} FontSize='16' FontWeight='Bold' Foreground='White' Text='" +
                                              Wpf.Esc(Tag(selKind, selIndex) + " · " + AreaName(selKind)) + "'/>"));
            pickerPanel.Children.Add(Wpf.Xaml("<TextBlock {NS} Foreground='#FFBDBDBD' Margin='0,0,0,8' Text='" +
                                              Wpf.Esc("Attuale: " + (selKind == "D" && current == 0 ? "Delta (predefinito)" : names[current])) + "'/>"));
            dynamic clear = Wpf.Xaml("<Button {NS} Content='" + (selKind == "D" ? "Torna al delta" : "Svuota riquadro") + "' HorizontalAlignment='Left' Padding='10,3' Margin='0,0,0,8'/>");
            Wpf.On((object)clear, "Click", (s, e) => { SetSlot(selKind, selIndex, 0); Changed(); });
            pickerPanel.Children.Add(clear);

            // cerca: filtra le schede mentre scrivi (nome o categoria)
            dynamic search = Wpf.Xaml("<TextBox {NS} Margin='0,0,0,6' Padding='4,3' ToolTip='Cerca una scheda'/>");
            search.Text = pickerSearch;
            dynamic hint = Wpf.Xaml("<TextBlock {NS} Text='Cerca...' Foreground='#FF7A7A7A' Margin='7,-27,0,10' IsHitTestVisible='False'/>");
            hint.Visibility = Wpf.Visibility(pickerSearch.Length == 0);
            pickerItems.Clear(); pickerGroups.Clear();
            Wpf.On((object)search, "TextChanged", (s, e) =>
            {
                pickerSearch = (string)search.Text ?? "";
                hint.Visibility = Wpf.Visibility(pickerSearch.Length == 0);
                FilterPicker();
            });
            pickerPanel.Children.Add(search);
            pickerPanel.Children.Add(hint);

            if (bar)
            {
                AddCategory("Barre", Range(1, Catalog.Bars.Length), Catalog.Bars, current);
                FilterPicker();
                return;
            }
            for (int c = 0; c < Catalog.Categories.Length; c++)
                AddCategory(Catalog.Categories[c], Catalog.CategoryCards[c], Catalog.Cards, current);
            FilterPicker();
        }

        string pickerSearch = "";
        readonly List<object[]> pickerItems = new List<object[]>();    // { pulsante, testo da cercare, gruppo }
        readonly List<object[]> pickerGroups = new List<object[]>();   // { titolo, contenitore, titolo categoria }

        static string Fold(string t)
        {
            t = (t ?? "").ToLowerInvariant();
            return t.Replace("à", "a").Replace("è", "e").Replace("é", "e").Replace("ì", "i").Replace("ò", "o").Replace("ù", "u");
        }

        void FilterPicker()
        {
            string q = Fold(pickerSearch.Trim());
            var shown = new Dictionary<object, int>();
            foreach (var it in pickerItems)
            {
                bool ok = q.Length == 0;
                if (!ok)
                {
                    ok = true;
                    foreach (var w in q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        if (((string)it[1]).IndexOf(w, StringComparison.Ordinal) < 0) { ok = false; break; }
                }
                ((dynamic)it[0]).Visibility = Wpf.Visibility(ok);
                int n; shown.TryGetValue(it[2], out n); shown[it[2]] = n + (ok ? 1 : 0);
            }
            foreach (var g in pickerGroups)
            {
                int n; shown.TryGetValue(g[1], out n);
                bool vis = n > 0;
                ((dynamic)g[0]).Visibility = Wpf.Visibility(vis);
                ((dynamic)g[1]).Visibility = Wpf.Visibility(vis);
            }
        }

        static int[] Range(int from, int to)
        {
            var r = new int[Math.Max(0, to - from)];
            for (int i = 0; i < r.Length; i++) r[i] = from + i;
            return r;
        }

        void AddCategory(string title, int[] items, string[] names, int current)
        {
            dynamic head = Wpf.Xaml("<TextBlock {NS} Text='" + Wpf.Esc(title) + "' Foreground='White' FontWeight='SemiBold' Margin='0,10,0,4'/>");
            pickerPanel.Children.Add(head);
            dynamic wrap = Wpf.Xaml("<WrapPanel {NS}/>");
            object wrapObj = wrap;
            foreach (var item in items)
            {
                int v = item;
                dynamic b = Wpf.Xaml("<Button {NS} Content='" + Wpf.Esc(names[v]) + "' Padding='8,3' Margin='0,0,4,4'" +
                                     BtnColors(v == current) + "/>");
                Wpf.On((object)b, "Click", (s, e) => { SetSlot(selKind, selIndex, v); Changed(); });
                wrap.Children.Add(b);
                pickerItems.Add(new object[] { (object)b, Fold(names[v] + " " + title), wrapObj });
            }
            pickerPanel.Children.Add(wrap);
            pickerGroups.Add(new object[] { (object)head, wrapObj });
        }

        // ---------------------------------------------------------------- mattoncini grafici
        static readonly string[][] Palette =
        {
            new[] { "Nero", "#FF000000" }, new[] { "Grigio scuro", "#FF1A1A1A" }, new[] { "Grigio", "#FF616161" }, new[] { "Bianco", "#FFFFFFFF" },
            new[] { "Verde", "#FF00E676" }, new[] { "Verde scuro", "#FF1B5E20" }, new[] { "Turchese DT", "#FF00FED1" }, new[] { "Azzurro", "#FF00B0FF" },
            new[] { "Blu", "#FF2979FF" }, new[] { "Blu scuro", "#FF0D47A1" }, new[] { "Viola", "#FFD500F9" }, new[] { "Viola scuro", "#FF4A148C" },
            new[] { "Rosso", "#FFFF1744" }, new[] { "Rosso scuro", "#FFB71C1C" }, new[] { "Arancione", "#FFFF8C00" }, new[] { "Giallo", "#FFFFD600" }
        };
        static readonly string[] ChangeCandidates = { "TC", "TCCut", "TCSlip", "ABS", "BB", "Map", "ERS", "Custom1", "Custom2", "Custom3", "Custom4", "Custom5", "Custom6" };
        const double LabelWidth = 140;

        dynamic Section(string title)
        {
            return Wpf.Xaml("<Grid {NS} Margin='0,6,0,12'><Grid.ColumnDefinitions><ColumnDefinition Width='Auto'/><ColumnDefinition Width='*'/></Grid.ColumnDefinitions>" +
                            "<TextBlock Text='" + Wpf.Esc(title.ToUpperInvariant()) + "' FontWeight='Bold' FontSize='14' Foreground='White'/>" +
                            "<Border Grid.Column='1' Height='1' Background='#FF4A4A4A' VerticalAlignment='Center' Margin='12,0,0,0'/></Grid>");
        }

        dynamic Help(string text, string margin = "0,4,0,0")
        {
            return Wpf.Xaml("<TextBlock {NS} TextWrapping='Wrap' Foreground='#FF9A9A9A' FontSize='11.5' Margin='" + margin + "' Text='" + Wpf.Esc(text) + "'/>");
        }

        dynamic Warn(string text)
        {
            return Wpf.Xaml("<TextBlock {NS} TextWrapping='Wrap' Foreground='#FFFFC107' FontSize='11.5' Margin='0,4,0,0' Text='" + Wpf.Esc(text) + "'/>");
        }

        dynamic Field()
        {
            return Wpf.Xaml("<StackPanel {NS} Margin='24,0,0,18'/>");
        }

        dynamic Row()
        {
            return Wpf.Xaml("<WrapPanel {NS} Margin='0,4,0,4'/>");
        }

        dynamic Label(string text, double width)
        {
            return Wpf.Xaml("<TextBlock {NS} Text='" + Wpf.Esc(text) + "' VerticalAlignment='Center' Foreground='#FFB8B8B8' Width='" + Wpf.N(width) + "'/>");
        }

        dynamic Small(string text)
        {
            return Wpf.Xaml("<TextBlock {NS} Text='" + Wpf.Esc(text) + "' VerticalAlignment='Center' Foreground='#FF9A9A9A' FontSize='11' Margin='0,0,6,0'/>");
        }

        dynamic Seg(string text, bool selected, bool enabled, Action click)
        {
            dynamic b = Wpf.Xaml("<Button {NS} Content='" + Wpf.Esc(text) + "' Padding='12,5' Margin='0,0,5,0'" + BtnColors(selected) + "/>");
            b.IsEnabled = enabled;
            Wpf.On((object)b, "Click", (s, e) => click());
            return b;
        }

        dynamic TextInput(string value, double width, Action<string> commit)
        {
            dynamic tb = Wpf.Xaml("<TextBox {NS} Width='" + Wpf.N(width) + "' Margin='0,0,10,0' VerticalContentAlignment='Center'/>");
            tb.Text = value ?? "";
            object o = tb;
            Action doCommit = () => { string v = (string)((dynamic)o).Text; if (v != value) commit(v); };
            Wpf.On(o, "LostFocus", (s, e) => doCommit());
            Wpf.On(o, "KeyDown", (s, e) => { dynamic de = e; if ((int)de.Key == 6) doCommit(); });
            return tb;
        }

        dynamic NumInput(double value, Action<double> set, Action after)
        {
            return TextInput(F1(value), 64, v =>
            {
                double d;
                if (double.TryParse((v ?? "").Replace(',', '.'), System.Globalization.NumberStyles.Float,
                                    System.Globalization.CultureInfo.InvariantCulture, out d)) set(d);
                after();
            });
        }

        dynamic Check(string text, bool value, Action<bool> set, Action after, bool bold = false, string margin = "0,0,18,0")
        {
            dynamic cb = Wpf.Xaml("<CheckBox {NS} Content='" + Wpf.Esc(text) + "' VerticalAlignment='Center' Margin='" + margin + "'" +
                                  (bold ? " FontWeight='SemiBold' FontSize='13.5'" : "") + "/>");
            cb.IsChecked = value;
            object o = cb;
            Action<object, object> h = (s, e) => { bool? v = ((dynamic)o).IsChecked; set(v == true); after(); };
            Wpf.On(o, "Checked", h);
            Wpf.On(o, "Unchecked", h);
            return cb;
        }

        /// <summary>Riquadro con titolo, sottotitolo, controlli a destra del titolo e corpo.</summary>
        dynamic Card(string title, string subtitle, out dynamic right, out dynamic body)
        {
            dynamic card = Wpf.Xaml("<Border {NS} Background='#FF262626' CornerRadius='10' Padding='18,14' Margin='0,0,0,14'><StackPanel>" +
                "<Grid><Grid.ColumnDefinitions><ColumnDefinition Width='*'/><ColumnDefinition Width='Auto'/></Grid.ColumnDefinitions>" +
                "<StackPanel><TextBlock Text='" + Wpf.Esc(title) + "' FontWeight='Bold' FontSize='15' Foreground='White'/>" +
                (string.IsNullOrEmpty(subtitle) ? "" : "<TextBlock Text='" + Wpf.Esc(subtitle) + "' TextWrapping='Wrap' Foreground='#FF9A9A9A' FontSize='11.5' Margin='0,3,0,0'/>") +
                "</StackPanel><StackPanel Grid.Column='1' Orientation='Horizontal' VerticalAlignment='Center' Margin='12,0,0,0'/></Grid>" +
                "<Border Height='1' Background='#FF3A3A3A' Margin='0,12,0,10'/><StackPanel/></StackPanel></Border>");
            dynamic stack = card.Child;
            right = stack.Children[0].Children[1];
            body = stack.Children[2];
            return card;
        }

        /// <summary>Riga con etichetta a sinistra (colonna fissa) e controlli a destra.</summary>
        dynamic LRow(string label)
        {
            dynamic r = Row();
            r.Children.Add(Label(label, LabelWidth));
            return r;
        }

        /// <summary>Quadratino del colore: clic = tavolozza con codice esadecimale.</summary>
        dynamic ColorField(string current, Action<string> set, Action after)
        {
            string c = DTPlugin.NormalizeColor(current, "#FF000000");
            dynamic panel = Wpf.Xaml("<StackPanel {NS} Orientation='Horizontal' VerticalAlignment='Center' Margin='0,0,14,0'/>");
            dynamic sw = Wpf.Xaml("<Border {NS} Width='48' Height='26' CornerRadius='5' BorderBrush='#FF5A5A5A' BorderThickness='1' Cursor='Hand' " +
                                  "Background='#FF000000' Padding='4' ToolTip='Clicca per scegliere il colore'><Border CornerRadius='3' Background='" + c + "'/></Border>");
            object swO = sw;
            Wpf.On(swO, "MouseLeftButtonUp", (s, e) => OpenColorPopup(swO, c, v => { set(v); after(); }));
            panel.Children.Add(sw);
            panel.Children.Add(Wpf.Xaml("<TextBlock {NS} Text='" + c + "' Foreground='#FF8A8A8A' FontSize='11' VerticalAlignment='Center' Margin='8,0,0,0'/>"));
            return panel;
        }

        void OpenColorPopup(object target, string current, Action<string> pick)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("<Popup {NS} StaysOpen='False' Placement='Bottom' AllowsTransparency='True'>");
                sb.Append("<Border Background='#FF2A2A2A' BorderBrush='#FF555555' BorderThickness='1' CornerRadius='8' Padding='10' Margin='0,4,0,0'><StackPanel>");
                sb.Append("<WrapPanel Width='236'>");
                foreach (var p in Palette)
                {
                    bool sel = SameColor(p[1], current);
                    sb.Append("<Border Width='26' Height='26' Margin='0,0,3,3' CornerRadius='4' Cursor='Hand' Background='" + p[1] + "' BorderBrush='" +
                              (sel ? "#FF1E9BD7" : "#FF555555") + "' BorderThickness='" + (sel ? "3" : "1") + "' ToolTip='" + Wpf.Esc(p[0]) + "' Tag='" + p[1] + "'/>");
                }
                sb.Append("</WrapPanel>");
                sb.Append("<DockPanel Margin='0,8,0,0'><Button DockPanel.Dock='Right' Content='OK' Padding='12,4' Margin='6,0,0,0' Background='#FF1E9BD7' Foreground='White' BorderThickness='0'/>");
                sb.Append("<TextBox Text='" + Wpf.Esc(current) + "' Padding='6,4' Background='#FF141414' Foreground='White' BorderBrush='#FF444444' CaretBrush='White'/></DockPanel>");
                sb.Append("<TextBlock Text='Codice: #RRGGBB oppure #AARRGGBB' Foreground='#FF9A9A9A' FontSize='10.5' Margin='0,4,0,0'/>");
                sb.Append("</StackPanel></Border></Popup>");
                dynamic popup = Wpf.Xaml(sb.ToString());
                popup.PlacementTarget = (dynamic)target;
                object popO = popup;
                dynamic stack = popup.Child.Child;
                dynamic wrap = stack.Children[0];
                int n = (int)wrap.Children.Count;
                for (int i = 0; i < n; i++)
                {
                    object it = wrap.Children[i];
                    string col = (string)((dynamic)it).Tag;
                    Wpf.On(it, "MouseLeftButtonUp", (s, e) => { ((dynamic)popO).IsOpen = false; pick(col); });
                }
                dynamic dock = stack.Children[1];
                object okO = dock.Children[0];
                object tbO = dock.Children[1];
                Action commit = () => { string v = (string)((dynamic)tbO).Text; ((dynamic)popO).IsOpen = false; pick(v); };
                Wpf.On(okO, "Click", (s, e) => commit());
                Wpf.On(tbO, "KeyDown", (s, e) => { dynamic de = e; if ((int)de.Key == 6) commit(); });
                popup.IsOpen = true;
            }
            catch (Exception ex) { FedeLog.Error("tavolozza colori", ex); }
        }

        // ---------------------------------------------------------------- avvisi
        AlertSettings learningAlert;

        void AlertChanged()
        {
            Save();
            root.Dispatcher.BeginInvoke(new Action(BuildAlerts));
        }

        void BuildAlerts()
        {
            try
            {
                alertsPanel.Children.Clear();
                alertsPanel.Children.Add(Section("Avvisi a schermo"));
                alertsPanel.Children.Add(Help("Gli avvisi compaiono sopra la dashboard. Se ne scattano due insieme vince il box, poi danni, bandiere, cambio valori e fine giro. " +
                                     "Premi \"Prova\" per vederli subito sulla dash.", "0,-4,0,14"));
                foreach (var a in S.Alerts) alertsPanel.Children.Add(AlertCard(a));
            }
            catch (Exception ex) { FedeLog.Error("scheda avvisi", ex); }
        }

        dynamic AlertCard(AlertSettings a)
        {
            string title = a.Id == "Pit" ? "Box / limitatore" : a.Id == "Damage" ? "Danni" : a.Id == "Flag" ? "Bandiere"
                         : a.Id == "Change" ? "Cambio valori" : a.Id == "Strategy" ? "Strategia gara" : "Fine giro";
            string sub = a.Id == "Pit" ? "Resta acceso finché il limitatore è inserito."
                       : a.Id == "Damage" ? "Quando subisci un danno mostra lo schema della macchina con le zone colpite (giallo lieve, arancio medio, rosso grave) e le gomme forate o perse."
                       : a.Id == "Flag" ? "Bandiera gialla, blu, nera, bianca, arancio (meatball); verde e scacchi solo per qualche secondo. Resta accesa finché la bandiera è esposta."
                       : a.Id == "Change" ? "Quando cambi BB, TC, ABS, mappa... mostra il nuovo valore per qualche secondo."
                       : a.Id == "Strategy" ? "Solo in gara: a ogni giro, dopo l'avviso di fine giro, mostra soste, finestra box, ratio, giri alla fine, litri ed energia necessari."
                       : "A ogni giro completato mostra i valori che scegli.";
            dynamic right, body;
            dynamic card = Card(title, sub, out right, out body);
            right.Children.Add(Check("Attivo", a.Enabled, v => a.Enabled = v, AlertChanged));
            right.Children.Add(Seg("Prova", false, true, () => plugin.TestAlert(a.Id)));

            dynamic r = LRow("Dimensione");
            string[] sizes = { "Pieno schermo", "Metà", "Piccolo", "Metà di lato" };
            for (int i = 0; i < 4; i++) { int v = i; r.Children.Add(Seg(sizes[i], a.Size == i, true, () => { a.Size = v; if (v == 3 && a.Position > 1) a.Position = 1; AlertChanged(); })); }
            body.Children.Add(r);

            r = LRow("Posizione");
            string[] pos = a.Size == 3 ? new[] { "Da sinistra", "Da destra" } : new[] { "Centro", "Alto", "Basso" };
            for (int i = 0; i < pos.Length; i++) { int v = i; r.Children.Add(Seg(pos[i], a.Position == i && a.Size != 0, a.Size != 0, () => { a.Position = v; AlertChanged(); })); }
            if (a.Size == 0) r.Children.Add(Small("(a pieno schermo non serve)"));
            body.Children.Add(r);

            if (a.Id == "Flag")
            {
                r = LRow("Dove");
                r.Children.Add(Seg("Sopra la velocità", !a.FlagAsPopup, true, () => { a.FlagAsPopup = false; AlertChanged(); }));
                r.Children.Add(Seg("Avviso a schermo", a.FlagAsPopup, true, () => { a.FlagAsPopup = true; AlertChanged(); }));
                body.Children.Add(r);
                if (!a.FlagAsPopup)
                    body.Children.Add(Help("La bandiera compare sopra il riquadro della velocità (al centro), con il suo colore e il nome. Dimensione, posizione e colori qui sotto valgono solo per l'avviso a schermo.", "0,-2,0,8"));
                r = LRow("Sfondo");
                r.Children.Add(Check("Del colore della bandiera (gialla, blu, nera...)", a.UseFlagColor, v => a.UseFlagColor = v, AlertChanged));
                body.Children.Add(r);
            }

            r = LRow("Colori");
            r.Children.Add(Small(a.Id == "Flag" && a.UseFlagColor ? "sfondo (se non usi il colore bandiera)" : "sfondo"));
            r.Children.Add(ColorField(a.Background, v => a.Background = v, AlertChanged));
            r.Children.Add(Small("testo"));
            r.Children.Add(ColorField(a.TextColor, v => a.TextColor = v, AlertChanged));
            body.Children.Add(r);

            r = LRow("Trasparenza sfondo");
            foreach (var o in new[] { 100, 85, 60 }) { int v = o; r.Children.Add(Seg(v == 100 ? "Pieno" : v + "%", a.Opacity == v, true, () => { a.Opacity = v; AlertChanged(); })); }
            body.Children.Add(r);

            if (a.Id == "Pit")
            {
                r = LRow("Schermata");
                r.Children.Add(Seg("Completa", !a.SimpleLayout, true, () => { a.SimpleLayout = false; AlertChanged(); }));
                r.Children.Add(Seg("Semplice", a.SimpleLayout, true, () => { a.SimpleLayout = true; AlertChanged(); }));
                body.Children.Add(r);
                body.Children.Add(Help(a.SimpleLayout ? "Titolo e i valori che scegli sotto, con la dimensione scelta."
                    : "Pieno schermo: auto davanti e dietro (numero, classe, nome, distacco), tempo in corsia box, velocità, marcia, giri motore e posizione. Dimensione, posizione e valori qui sotto valgono solo per la schermata semplice.", "0,-2,0,8"));
                r = LRow("Quando");
                r.Children.Add(Check("Anche in corsia box senza limitatore", a.PitLaneToo, v => a.PitLaneToo = v, AlertChanged));
                body.Children.Add(r);
            }
            else
            {
                r = LRow("Durata (secondi)");
                r.Children.Add(TextInput(a.Seconds.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture), 64, v =>
                {
                    double d;
                    if (double.TryParse(v.Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d))
                        a.Seconds = Math.Max(0.3, Math.Min(60, d));
                    AlertChanged();
                }));
                body.Children.Add(r);
            }

            if (a.Id != "Change")   // il cambio valori mostra solo nome e valore
            {
                r = LRow("Titolo");
                r.Children.Add(TextInput(a.Title, 220, v => { a.Title = v; AlertChanged(); }));
                body.Children.Add(r);
            }

            if (a.Id == "Strategy")
            {
                r = LRow("Quando");
                r.Children.Add(Seg("A ogni giro + tasto", !a.NoAutoLap, true, () => { a.NoAutoLap = false; AlertChanged(); }));
                r.Children.Add(Seg("Solo col tasto", a.NoAutoLap, true, () => { a.NoAutoLap = true; AlertChanged(); }));
                body.Children.Add(r);
                r = LRow("Tasto volante");
                bool learning = plugin.LearnButton != null && learningAlert == a;
                string keyTxt = learning ? "Premi ora il tasto sul volante o sulla button box..."
                              : string.IsNullOrEmpty(a.KeyDevice) ? "Nessun tasto" : (a.KeyDeviceName ?? "Volante") + " - tasto " + a.KeyButton;
                r.Children.Add(Small(keyTxt));
                r.Children.Add(Seg(learning ? "Annulla" : "Assegna tasto", learning, true, () =>
                {
                    if (plugin.LearnButton != null) { plugin.LearnButton = null; learningAlert = null; AlertChanged(); return; }
                    learningAlert = a;
                    plugin.LearnButton = (dev, name, btn) => { a.KeyDevice = dev; a.KeyDeviceName = name; a.KeyButton = btn; learningAlert = null; AlertChanged(); };
                    AlertChanged();
                }));
                if (!string.IsNullOrEmpty(a.KeyDevice) && !learning)
                    r.Children.Add(Seg("Togli", false, true, () => { a.KeyDevice = null; a.KeyDeviceName = null; a.KeyButton = 0; AlertChanged(); }));
                body.Children.Add(r);
                body.Children.Add(Help("Premi \"Assegna tasto\" e poi il tasto che vuoi usare: la strategia esce subito (anche fuori gara), ripremendolo si chiude. " +
                    "In alternativa: SimHub > Controlli ed eventi > azione DTPlugin.MostraStrategia.", "0,-2,0,8"));
            }
            else if (a.Id == "Damage" || a.Id == "Flag")
            {
                if (a.Id == "Damage")
                    body.Children.Add(Help("Lo schema danni si può anche mettere fisso in un riquadro: scheda \"Danni (schema auto)\" in Pagine e layout.", "0,4,0,0"));
            }
            else if (a.Id == "Change")
            {
                r = LRow("Avvisa quando cambia");
                dynamic boxes = Wpf.Xaml("<WrapPanel {NS} MaxWidth='720'/>");
                foreach (var id in ChangeCandidates)
                {
                    string cid = id;
                    boxes.Children.Add(Check(ChannelLabel(cid), a.Channels.Contains(cid), v => { a.Channels.Remove(cid); if (v) a.Channels.Add(cid); }, AlertChanged, false, "0,0,18,8"));
                }
                r.Children.Add(boxes);
                body.Children.Add(r);
            }
            else
            {
                r = LRow("Valori da mostrare");
                for (int i = 0; i < AlertSettings.MaxValues; i++)
                {
                    int slot = i;
                    dynamic cb = Wpf.Xaml("<ComboBox {NS} Width='180' Margin='0,0,6,4' VerticalContentAlignment='Center'/>");
                    cb.Items.Add("(nessuno)");
                    int sel = 0, k = 1;
                    string current = i < a.Channels.Count ? a.Channels[i] : "";
                    foreach (var rt in plugin.Runtime)
                    {
                        cb.Items.Add(rt.Settings.Label + "  (" + rt.Def.Id + ")");
                        if (rt.Def.Id == current) sel = k;
                        k++;
                    }
                    cb.SelectedIndex = sel;
                    object cbO = cb;
                    Wpf.On(cbO, "SelectionChanged", (s, e) =>
                    {
                        int idx = ((dynamic)cbO).SelectedIndex;
                        var list = new List<string>(a.Channels);
                        while (list.Count < AlertSettings.MaxValues) list.Add("");
                        list[slot] = idx <= 0 ? "" : plugin.Runtime[idx - 1].Def.Id;
                        a.Channels = list.FindAll(x => !string.IsNullOrEmpty(x));
                        AlertChanged();
                    });
                    r.Children.Add(cb);
                }
                body.Children.Add(r);
            }
            AddAlertPreview(card, body, a);
            return card;
        }

        /// <summary>Anteprima dell'avviso a destra delle sue impostazioni (sopra la pagina attiva).</summary>
        void AddAlertPreview(dynamic card, dynamic body, AlertSettings a)
        {
            try
            {
                dynamic stack = card.Child;
                stack.Children.Remove(body);
                dynamic g = Wpf.Xaml("<Grid {NS}><Grid.ColumnDefinitions><ColumnDefinition Width='*'/><ColumnDefinition Width='Auto'/></Grid.ColumnDefinitions></Grid>");
                g.Children.Add(body);
                dynamic pb = Wpf.Xaml("<Border {NS} Grid.Column='1' Margin='16,0,0,0' VerticalAlignment='Top' CornerRadius='6' Background='#FF000000' BorderBrush='#FF3E3E3E' BorderThickness='1' Padding='3'>" +
                                      "<StackPanel><Viewbox Width='340' Stretch='Uniform'><Canvas Width='850' Height='480' Background='Black' ClipToBounds='True'/></Viewbox>" +
                                      "<TextBlock Text='Anteprima (pagina attiva, valori di esempio)' FontSize='10.5' Foreground='#FF7A7A7A' HorizontalAlignment='Center' Margin='0,3,0,0'/></StackPanel></Border>");
                dynamic canvas = pb.Child.Children[0].Child;
                Func<string, string> lbl = id =>
                {
                    foreach (var rt in plugin.Runtime) if (rt.Def.Id == id) return rt.Settings.Label;
                    return id;
                };
                bool ok = previewRenderer.DrawAlert((object)canvas, Page, S.Font, a, lbl);
                if (!ok) { g.Children.Remove(body); stack.Children.Add(body); return; }   // file della dash non trovati: niente anteprima
                g.Children.Add(pb);
                stack.Children.Add(g);
            }
            catch (Exception ex)
            {
                FedeLog.Error("anteprima avviso", ex);
                try { dynamic stack = card.Child; if (!stack.Children.Contains(body)) stack.Children.Add(body); } catch { }
            }
        }

        // ---------------------------------------------------------------- tempi e posizioni
        dynamic timingPanel, timingLive;

        void TimingChanged()
        {
            Save();
            root.Dispatcher.BeginInvoke(new Action(BuildTiming));
        }

        void BuildTiming()
        {
            try
            {
                if (timingPanel == null) timingPanel = F("TimingPanel");
                timingPanel.Children.Clear();
                var t = S.Timing;
                bool postIt = plugin.HasPostIt();
                timingPanel.Children.Add(Section("Opzioni tempi"));
                timingPanel.Children.Add(Help("Scegli da dove prendere giro stimato, delta, distacchi e posizione. Valgono per tutte le schede " +
                    "(Delta, Giro stimato, Posizione, Distacchi) e per gli avvisi.", "0,-4,0,14"));
                timingPanel.Children.Add(Warn(postIt ? "Post-It Note Racing trovato: puoi usarlo come fonte."
                    : "Post-It Note Racing non trovato in SimHub: le sue opzioni sono disattivate (se lo scegli si usa SimHub)."));

                string[] dSub = { "Delta calcolato da LMU sul tuo giro migliore (il più preciso). Il giro stimato = miglior giro + delta.",
                                  "Delta e stima di SimHub (Persistant Tracker), basati sul miglior giro della sessione.",
                                  "Stima del plugin Post-It Note Racing." };
                timingPanel.Children.Add(Tiles("Giro stimato e delta", TimingSettings.DeltaSources, dSub, t.DeltaSource, postIt ? -1 : 2, v => t.DeltaSource = v));

                string[] gSub = { "Distacco dalla macchina davanti e dietro in classifica generale.",
                                  "Distacco dalla macchina davanti e dietro della tua classe (Hypercar, LMP2, GT3...)." };
                timingPanel.Children.Add(Tiles("Distacchi", TimingSettings.GapModes, gSub, t.GapMode, -1, v => t.GapMode = v));

                string[] pSub = { "Posizione in classifica generale (SimHub).", "Posizione nella tua classe, calcolata da SimHub.",
                                  "Posizione live nella classe da Post-It Note Racing." };
                timingPanel.Children.Add(Tiles("Posizione", TimingSettings.PositionModes, pSub, t.PositionMode, postIt ? -1 : 2, v => t.PositionMode = v));

                timingPanel.Children.Add(Section("Valori attuali"));
                timingLive = Wpf.Xaml("<TextBlock {NS} FontFamily='Consolas' FontSize='13' Foreground='#FFE0E0E0' Margin='0,0,0,8' TextWrapping='Wrap'/>");
                timingPanel.Children.Add(timingLive);
                UpdateTimingLive();
            }
            catch (Exception ex) { FedeLog.Error("scheda tempi", ex); }
        }

        dynamic Tiles(string title, string[] names, string[] subs, int selected, int disabled, Action<int> set, Action after = null, string[] families = null)
        {
            dynamic box = Wpf.Xaml("<StackPanel {NS} Margin='0,0,0,18'/>");
            box.Children.Add(Wpf.Xaml("<TextBlock {NS} Text='" + Wpf.Esc(title) + "' FontWeight='Bold' FontSize='13' Foreground='White' Margin='0,0,0,8'/>"));
            dynamic wrap = Wpf.Xaml("<WrapPanel {NS}/>");
            for (int i = 0; i < names.Length; i++)
            {
                int v = i;
                bool sel = i == selected, off = i == disabled;
                string border = sel ? "#FF2196F3" : "#FF3A3A3A";
                string bg = sel ? "#FF15304A" : "#FF262626";
                dynamic tile = Wpf.Xaml("<Border {NS} Width='250' MinHeight='86' Margin='0,0,10,10' Padding='12,10' CornerRadius='6' BorderThickness='" + (sel ? 2 : 1) +
                    "' BorderBrush='" + border + "' Background='" + bg + "' Cursor='Hand' Opacity='" + (off ? "0.4" : "1") + "'>" +
                    "<StackPanel><TextBlock Text='" + Wpf.Esc(names[i]) + "' FontWeight='Bold' FontSize='13' Foreground='" + (sel ? "#FF64B5F6" : "White") + "'" +
                    (families != null && i < families.Length ? " FontFamily='" + Wpf.Esc(families[i]) + ", Segoe UI'" : "") + "/>" +
                    "<TextBlock Text='" + Wpf.Esc(i < subs.Length ? subs[i] : "") + "' TextWrapping='Wrap' FontSize='11' Foreground='#FF9A9A9A' Margin='0,4,0,0'/></StackPanel></Border>");
                if (!off)
                {
                    object tO = tile;
                    Wpf.On(tO, "MouseLeftButtonUp", (s, e) => { set(v); if (after != null) after(); else TimingChanged(); });
                }
                wrap.Children.Add(tile);
            }
            box.Children.Add(wrap);
            return box;
        }

        // ---------------------------------------------------------------- carattere e aggiornamenti
        dynamic fontPanel;

        void FontChanged()
        {
            Save();
            root.Dispatcher.BeginInvoke(new Action(BuildFont));
        }

        void BuildFont()
        {
            try
            {
                if (fontPanel == null) fontPanel = F("FontPanel");
                fontPanel.Children.Clear();
                var f = S.Font;
                fontPanel.Children.Add(Section("Carattere della dashboard"));
                fontPanel.Children.Add(Help("Cambia tipo, spessore e grandezza di tutti i testi della dashboard. Il plugin riscrive i file della dash: " +
                    "se SimHub non la aggiorna subito, riapri la dashboard (o riavvia SimHub).", "0,-4,0,14"));
                string[] famSub = { "Quello di sempre.", "Più stretto: i numeri occupano meno spazio.",
                                    "Font da strumentazione, già presente in Windows 10/11.", "Il font di Windows, più morbido.", "Classico, molto leggibile.",
                                    "Largo e marcato, numeri a larghezza fissa (non ballano). Incluso nel plugin. La prima volta può servire riavviare SimHub." };
                fontPanel.Children.Add(Tiles("Tipo di carattere", FontSettings.FamilyNames, famSub, f.Family, -1, v => f.Family = v, FontChanged, FontSettings.Families));
                string[] wSub = { "Spessori come sono stati disegnati.", "Ogni testo di uno spessore più sottile (grassetto → medio).",
                                  "Due spessori più sottile (grassetto → normale).", "Ogni testo un po' più spesso." };
                fontPanel.Children.Add(Tiles("Spessore", FontSettings.Weights, wSub, f.Weight, -1, v => f.Weight = v, FontChanged));
                var sizeNames = new string[FontSettings.Sizes.Length];
                var sizeSubs = new string[FontSettings.Sizes.Length];
                int selSize = 0;
                for (int i = 0; i < sizeNames.Length; i++)
                {
                    int pct = FontSettings.Sizes[i];
                    sizeNames[i] = pct + "%";
                    sizeSubs[i] = pct == 100 ? "Grandezza originale." : (pct < 100 ? "Testi più piccoli del " + (100 - pct) + "%." : "Testi più grandi del " + (pct - 100) + "%.");
                    if (pct == f.SizePct) selSize = i;
                }
                fontPanel.Children.Add(Tiles("Grandezza", sizeNames, sizeSubs, selSize, -1, v => f.SizePct = FontSettings.Sizes[v], FontChanged));

                fontPanel.Children.Add(Section("Bordi dei riquadri"));
                fontPanel.Children.Add(Help("Spessore in pixel della cornice dei riquadri. 0 = nessun bordo.", "0,-4,0,14"));
                var bNames = new string[FontSettings.BorderSizes.Length];
                var bSubs = new string[FontSettings.BorderSizes.Length];
                int selC = 0, selK = 0;
                for (int i = 0; i < bNames.Length; i++)
                {
                    int px = FontSettings.BorderSizes[i];
                    bNames[i] = px == 0 ? "Nessuno" : px + " px";
                    bSubs[i] = px == 0 ? "Senza cornice." : (px == 1 ? "Sottile." : (px == 2 ? "Normale." : "Spesso."));
                    if (px == f.CenterBorder) selC = i;
                    if (px == f.CardBorder) selK = i;
                }
                fontPanel.Children.Add(Tiles("Parte centrale (marcia, velocità, righe al centro)", bNames, bSubs, selC, -1, v => f.CenterBorder = FontSettings.BorderSizes[v], FontChanged));
                fontPanel.Children.Add(Tiles("Schede ai lati e in basso", bNames, bSubs, selK, -1, v => f.CardBorder = FontSettings.BorderSizes[v], FontChanged));

                fontPanel.Children.Add(Section("Aggiornamenti"));
                string[] uNames = { "Avvisami con una finestra", "Solo qui nella pagina del plugin" };
                string[] uSub = { "Quando su GitHub esce una versione nuova compare una finestra in SimHub (controllo all'avvio e ogni 30 minuti).",
                                  "Nessuna finestra: l'avviso si vede solo in alto in questa pagina." };
                fontPanel.Children.Add(Tiles("Nuove versioni", uNames, uSub, S.UpdatePopup ? 0 : 1, -1, v => S.UpdatePopup = v == 0, FontChanged));
            }
            catch (Exception ex) { FedeLog.Error("scheda carattere", ex); }
        }

        void UpdateTimingLive()
        {
            try
            {
                if (timingLive == null) return;
                timingLive.Text = plugin.TimingSummary();
            }
            catch { }
        }

        // ---------------------------------------------------------------- gomme e freni
        void TyresChanged()
        {
            Save();
            root.Dispatcher.BeginInvoke(new Action(BuildTyres));
        }

        void ThresholdRow(object bodyObj, string title, string a, double va, Action<double> sa, string b, double vb, Action<double> sb,
                          string c, double vc, Action<double> sc)
        {
            dynamic r = LRow(title);
            r.Children.Add(Wpf.Xaml("<Ellipse {NS} Width='11' Height='11' Fill='#FF2979FF' Margin='0,0,6,0' VerticalAlignment='Center'/>"));
            r.Children.Add(Small(a)); r.Children.Add(NumInput(va, sa, TyresChanged));
            r.Children.Add(Wpf.Xaml("<Ellipse {NS} Width='11' Height='11' Fill='#FF00E676' Margin='6,0,6,0' VerticalAlignment='Center'/>"));
            r.Children.Add(Small(b)); r.Children.Add(NumInput(vb, sb, TyresChanged));
            r.Children.Add(Wpf.Xaml("<Ellipse {NS} Width='11' Height='11' Fill='#FFFF1744' Margin='6,0,6,0' VerticalAlignment='Center'/>"));
            r.Children.Add(Small(c)); r.Children.Add(NumInput(vc, sc, TyresChanged));
            ((dynamic)bodyObj).Children.Add(r);
        }

        void BuildTyres()
        {
            try
            {
                var t = S.Tyres;
                tyresPanel.Children.Clear();
                tyresPanel.Children.Add(Section("Gomme e freni"));

                dynamic right, body;
                dynamic c1 = Card("Temperatura gomme", "In LMU il valore standard di SimHub non coincide con il gioco: qui si legge la telemetria di LMU. " +
                                  "\"Strato interno\" è quello che coincide con l'HUD del gioco.", out right, out body);
                dynamic r = LRow("Fonte del numero");
                for (int i = 0; i < TyreSettings.Sources.Length; i++)
                {
                    int v = i;
                    r.Children.Add(Seg(TyreSettings.Sources[i], t.TempSource == i, true, () => { S.Tyres.TempSource = v; TyresChanged(); }));
                }
                body.Children.Add(r);
                r = LRow("Correzione (°C)");
                r.Children.Add(NumInput(t.TempOffset, d => S.Tyres.TempOffset = Math.Max(-50, Math.Min(50, d)), TyresChanged));
                r.Children.Add(Small("da aggiungere o togliere, di solito 0"));
                body.Children.Add(r);
                tyresPanel.Children.Add(c1);

                dynamic c2 = Card("Colori e soglie", "Blu sotto il primo valore, verde al valore ottimale, rosso sopra il terzo; in mezzo sfuma.", out right, out body);
                r = LRow("Pressione in");
                for (int i = 0; i < TyreSettings.PressureUnits.Length; i++)
                {
                    int v = i;
                    r.Children.Add(Seg(TyreSettings.PressureUnits[i], t.PressureUnit == i, true, () =>
                    {
                        var ty = S.Tyres;
                        double lo = ty.ToKpa(ty.PressLow), op = ty.ToKpa(ty.PressOptimal), hi = ty.ToKpa(ty.PressHigh);
                        ty.PressureUnit = v;
                        ty.PressLow = Math.Round(ty.FromKpa(lo), 2); ty.PressOptimal = Math.Round(ty.FromKpa(op), 2); ty.PressHigh = Math.Round(ty.FromKpa(hi), 2);
                        foreach (var rt in plugin.Runtime)
                            if (rt.Def.Id.StartsWith("TyreP")) rt.Settings.Decimals = v == 2 ? 2 : 1;
                        TyresChanged();
                    }));
                }
                body.Children.Add(r);
                ThresholdRow((object)body, "Gomme (°C)", "freddo", t.TyreCold, d => S.Tyres.TyreCold = d, "ottimale", t.TyreOptimal, d => S.Tyres.TyreOptimal = d,
                             "caldo", t.TyreHot, d => S.Tyres.TyreHot = d);
                ThresholdRow((object)body, "Freni (°C)", "freddo", t.BrakeCold, d => S.Tyres.BrakeCold = d, "ottimale", t.BrakeOptimal, d => S.Tyres.BrakeOptimal = d,
                             "caldo", t.BrakeHot, d => S.Tyres.BrakeHot = d);
                ThresholdRow((object)body, "Pressione", "bassa", t.PressLow, d => S.Tyres.PressLow = d, "ottimale", t.PressOptimal, d => S.Tyres.PressOptimal = d,
                             "alta", t.PressHigh, d => S.Tyres.PressHigh = d);
                tyresPanel.Children.Add(c2);

                dynamic c3 = Card("Valori in questo momento", "Per confrontarli con l'HUD del gioco.", out right, out body);
                tyreLive = Wpf.Xaml("<TextBlock {NS} FontFamily='Consolas' FontSize='13' LineHeight='20' Foreground='#FFE0E0E0'/>");
                body.Children.Add(tyreLive);
                tyresPanel.Children.Add(c3);
                UpdateTyreLive();
            }
            catch (Exception ex) { FedeLog.Error("scheda gomme", ex); }
        }

        // ---------------------------------------------------------------- schermata iniziale
        void IdleChanged()
        {
            Save();
            root.Dispatcher.BeginInvoke(new Action(BuildIdle));
        }

        void BuildIdle()
        {
            try
            {
                var d = S.Idle;
                idlePanel.Children.Clear();
                dynamic sp = idlePanel;

                // ---- pilota
                sp.Children.Add(Section("Pilota"));
                dynamic f = Field();
                f.Children.Add(Check("Numero pilota", d.NumberEnabled, v => S.Idle.NumberEnabled = v, IdleChanged, true));
                dynamic r = Row();
                r.Children.Add(TextInput(d.Number, 90, v => { S.Idle.Number = v; IdleChanged(); }));
                r.Children.Add(ColorField(d.NumberColor, v => S.Idle.NumberColor = v, IdleChanged));
                f.Children.Add(r);
                f.Children.Add(Help("Il tuo numero e il suo colore."));
                sp.Children.Add(f);

                f = Field();
                f.Children.Add(Check("Nome pilota", d.NameEnabled, v => S.Idle.NameEnabled = v, IdleChanged, true));
                r = Row();
                r.Children.Add(TextInput(d.Name, 260, v => { S.Idle.Name = v; IdleChanged(); }));
                r.Children.Add(ColorField(d.NameColor, v => S.Idle.NameColor = v, IdleChanged));
                f.Children.Add(r);
                f.Children.Add(Help("Il nome come vuoi vederlo sulla schermata."));
                if (d.BadgeEnabled) f.Children.Add(Warn("Lo stemma è attivo: numero e nome grandi sono nascosti (il numero può stare dentro lo stemma)."));
                sp.Children.Add(f);

                // ---- sfondo
                sp.Children.Add(Section("Sfondo"));
                f = Field();
                r = Row();
                for (int i = 0; i < IdleSettings.Modes.Length; i++)
                {
                    int v = i;
                    r.Children.Add(Seg(IdleSettings.Modes[i], d.Mode == i, true, () => { S.Idle.Mode = v; IdleChanged(); }));
                }
                f.Children.Add(r);
                if (d.Mode == 1)
                {
                    r = Row();
                    r.Children.Add(TextInput(d.ImagePath, 300, v => { S.Idle.ImagePath = (v ?? "").Trim().Trim('"'); IdleChanged(); }));
                    r.Children.Add(Seg("Scegli immagine…", false, true, PickImage));
                    f.Children.Add(r);
                    r = Row();
                    r.Children.Add(Small("colore sotto"));
                    r.Children.Add(ColorField(d.Color1, v => S.Idle.Color1 = v, IdleChanged));
                    f.Children.Add(r);
                    f.Children.Add(Help("Meglio un'immagine 850x480 (o con le stesse proporzioni) che resti in quella cartella del PC."));
                }
                else
                {
                    r = Row();
                    r.Children.Add(Small(d.Mode == 2 ? "sfondo" : "colore 1"));
                    r.Children.Add(ColorField(d.Color1, v => S.Idle.Color1 = v, IdleChanged));
                    r.Children.Add(Small(d.Mode == 2 ? "effetto" : "colore 2"));
                    r.Children.Add(ColorField(d.Color2, v => S.Idle.Color2 = v, IdleChanged));
                    f.Children.Add(r);
                    r = Row();
                    if (d.Mode == 0)
                    {
                        r.Children.Add(Small("sfumatura"));
                        for (int i = 0; i < IdleSettings.Gradients.Length; i++)
                        {
                            int v = i;
                            r.Children.Add(Seg(IdleSettings.Gradients[i], d.Gradient == i, true, () => { S.Idle.Gradient = v; IdleChanged(); }));
                        }
                    }
                    else
                    {
                        r.Children.Add(Small("animazione"));
                        for (int i = 0; i < IdleSettings.Animations.Length; i++)
                        {
                            int v = i;
                            r.Children.Add(Seg(IdleSettings.Animations[i], d.Animation == i, true, () => { S.Idle.Animation = v; IdleChanged(); }));
                        }
                    }
                    f.Children.Add(r);
                    f.Children.Add(Help(d.Mode == 2 ? "Le animazioni funzionano su qualunque schermo della dashboard." : "Con \"Nessuna\" si usa solo il colore 1."));
                }
                sp.Children.Add(f);

                // ---- stemma
                sp.Children.Add(Section("Stemma"));
                f = Field();
                r = Row();
                r.Children.Add(Check("Mostra stemma", d.BadgeEnabled, v => S.Idle.BadgeEnabled = v, IdleChanged, true));
                dynamic preset = Seg("Preset " + Logos.DtRacingTeamName, false, true, ApplyDtPreset);
                preset.Background = Wpf.Xaml("<SolidColorBrush {NS} Color='#FF00A88A'/>");
                preset.Foreground = Wpf.Xaml("<SolidColorBrush {NS} Color='White'/>");
                r.Children.Add(preset);
                f.Children.Add(r);
                f.Children.Add(Help("Un cerchio con il suo colore, un bordo, il logo e il nome dentro. Il preset imposta tutto con il logo del team."));
                if (d.BadgeEnabled)
                {
                    dynamic box = Wpf.Xaml("<Border {NS} Background='#FF262626' CornerRadius='10' Padding='16,12' Margin='0,10,0,0'><StackPanel/></Border>");
                    dynamic b = box.Child;
                    r = LRow("Dimensione");
                    int[] sizes = { 180, 250, 330, 420 };
                    string[] sizeNames = { "Piccolo", "Medio", "Grande", "Enorme" };
                    for (int i = 0; i < sizes.Length; i++)
                    {
                        int v = sizes[i];
                        r.Children.Add(Seg(sizeNames[i], Math.Abs(d.BadgeSize - v) < 1, true, () => { S.Idle.BadgeSize = v; IdleChanged(); }));
                    }
                    b.Children.Add(r);
                    r = LRow("Colori");
                    r.Children.Add(Small("cerchio"));
                    r.Children.Add(ColorField(d.BadgeFill, v => S.Idle.BadgeFill = v, IdleChanged));
                    r.Children.Add(Small("bordo"));
                    r.Children.Add(ColorField(d.BadgeBorder, v => S.Idle.BadgeBorder = v, IdleChanged));
                    b.Children.Add(r);
                    r = LRow("Spessore bordo");
                    foreach (var bw in new[] { 0, 4, 8, 12, 16 })
                    {
                        int v = bw;
                        r.Children.Add(Seg(v == 0 ? "Nessuno" : v + " px", Math.Abs(d.BadgeBorderWidth - v) < 0.5, true, () => { S.Idle.BadgeBorderWidth = v; IdleChanged(); }));
                    }
                    b.Children.Add(r);
                    r = LRow("Logo");
                    r.Children.Add(TextInput(d.BadgeLogoPath, 230, v => { SetBadgeLogo((v ?? "").Trim().Trim('"')); }));
                    r.Children.Add(Seg("Scegli logo…", false, true, PickBadgeLogo));
                    r.Children.Add(Seg("Logo DT", false, true, () => SetBadgeLogo(DTPlugin.EnsureDtLogo())));
                    b.Children.Add(r);
                    r = LRow("Nome nel cerchio");
                    r.Children.Add(TextInput(d.BadgeText, 200, v => { S.Idle.BadgeText = v ?? ""; IdleChanged(); }));
                    r.Children.Add(ColorField(d.BadgeTextColor, v => S.Idle.BadgeTextColor = v, IdleChanged));
                    b.Children.Add(r);
                    r = LRow("Numero sotto il nome");
                    r.Children.Add(Check("", d.BadgeShowNumber, v => S.Idle.BadgeShowNumber = v, IdleChanged));
                    r.Children.Add(TextInput(d.Number, 70, v => { S.Idle.Number = v; IdleChanged(); }));
                    r.Children.Add(ColorField(d.NumberColor, v => S.Idle.NumberColor = v, IdleChanged));
                    b.Children.Add(r);
                    f.Children.Add(box);
                }
                sp.Children.Add(f);

                // ---- altro
                sp.Children.Add(Section("Altro"));
                f = Field();
                r = Row();
                r.Children.Add(Check("Orologio in alto", d.ShowClock, v => S.Idle.ShowClock = v, IdleChanged));
                r.Children.Add(Check("Scritta \"In attesa del gioco\"", d.ShowWaiting, v => S.Idle.ShowWaiting = v, IdleChanged));
                f.Children.Add(r);
                sp.Children.Add(f);

                // ---- motore spento
                sp.Children.Add(Section("Motore spento"));
                f = Field();
                f.Children.Add(Check("Avviso quando il motore è spento", !d.EngineOffDisabled, v => S.Idle.EngineOffDisabled = !v, IdleChanged, true));
                r = Row();
                r.Children.Add(Seg("Riquadro marcia giallo", d.EngineOffStyle != 1, true, () => { S.Idle.EngineOffStyle = 0; IdleChanged(); }));
                r.Children.Add(Seg("Schermo nero", d.EngineOffStyle == 1, true, () => { S.Idle.EngineOffStyle = 1; IdleChanged(); }));
                f.Children.Add(r);
                r = Row();
                r.Children.Add(TextInput(d.EngineOffText, 260, v => { S.Idle.EngineOffText = v; IdleChanged(); }));
                r.Children.Add(ColorField(d.EngineOffColor, v => S.Idle.EngineOffColor = v, IdleChanged));
                f.Children.Add(r);
                f.Children.Add(Help("In gioco, con il motore fermo (giri a zero o accensione spenta), la dashboard diventa nera con questa scritta. " +
                                    "Torna normale appena accendi il motore."));
                sp.Children.Add(f);

                DrawIdlePreview();
            }
            catch (Exception ex) { FedeLog.Error("schermata iniziale", ex); }
        }

        string ChannelLabel(string id)
        {
            foreach (var rt in plugin.Runtime) if (rt.Def.Id == id) return rt.Settings.Label + "  (" + id + ")";
            return id;
        }

        static bool SameColor(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        static string F1(double v)
        {
            return v.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }

        void UpdateTyreLive()
        {
            if (tyreLive == null) return;
            var sb = new StringBuilder();
            string[] w = { "FL", "FR", "RL", "RR" };
            string[] it = { "AS", "AD", "PS", "PD" };
            for (int i = 0; i < 4; i++)
            {
                sb.Append(it[i]).Append("   gomma ").Append(Val("TyreT" + w[i]).PadLeft(6))
                  .Append("   pressione ").Append(Val("TyreP" + w[i]).PadLeft(6))
                  .Append("   freno ").Append(Val("BrakeT" + w[i]).PadLeft(6));
                if (i < 3) sb.Append("\n");
            }
            tyreLive.Text = sb.ToString();
        }

        string Val(string id)
        {
            foreach (var rt in plugin.Runtime) if (rt.Def.Id == id) return rt.Text;
            return "-";
        }

        void PickImage()
        {
            try
            {
                dynamic dlg = Activator.CreateInstance(Wpf.T("Microsoft.Win32.OpenFileDialog"));
                dlg.Filter = "Immagini|*.png;*.jpg;*.jpeg;*.bmp|Tutti i file|*.*";
                dlg.Title = "Scegli l'immagine di sfondo";
                object r = dlg.ShowDialog();
                if (r is bool && (bool)r)
                {
                    S.Idle.ImagePath = (string)dlg.FileName;
                    S.Idle.Mode = 1;
                    IdleChanged();
                }
            }
            catch (Exception ex) { FedeLog.Error("scelta immagine", ex); }
        }

        /// <summary>Preset "DT Racing Team": stemma scuro con bordo arancione, logo DT, sfondo animato.</summary>
        void ApplyDtPreset()
        {
            var d = S.Idle;
            d.BadgeEnabled = true;
            d.BadgeLogoPath = DTPlugin.EnsureDtLogo();
            d.BadgeLogoAspect = Logos.DtRacingTeamAspect;
            d.BadgeText = "DT RACING TEAM";
            d.BadgeFill = "#FF1A1A1A";
            d.BadgeBorder = "#FFFF8C00";
            d.BadgeBorderWidth = 8;
            d.BadgeSize = 250;
            d.BadgeTextColor = "#FFFFFFFF";
            d.BadgeShowNumber = true;
            d.NumberEnabled = false;
            d.NameEnabled = false;
            d.Mode = 2;
            d.Animation = 0;
            d.Color1 = "#FF000000";
            d.Color2 = "#FF00FED1";
            IdleChanged();
        }

        static double ImageAspect(string path)
        {
            try
            {
                var create = Wpf.T("System.Windows.Media.Imaging.BitmapFrame").GetMethod("Create", new[] { typeof(Uri) });
                dynamic frame = create.Invoke(null, new object[] { new Uri(path) });
                double w = (double)(int)frame.PixelWidth, h = (double)(int)frame.PixelHeight;
                return h > 0 ? w / h : 2;
            }
            catch { return 2; }
        }

        void SetBadgeLogo(string path)
        {
            S.Idle.BadgeLogoPath = path ?? "";
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path)) S.Idle.BadgeLogoAspect = ImageAspect(path);
            IdleChanged();
        }

        void PickBadgeLogo()
        {
            try
            {
                dynamic dlg = Activator.CreateInstance(Wpf.T("Microsoft.Win32.OpenFileDialog"));
                dlg.Filter = "Immagini|*.png;*.jpg;*.jpeg;*.bmp|Tutti i file|*.*";
                dlg.Title = "Scegli il logo (meglio PNG con sfondo trasparente)";
                object r = dlg.ShowDialog();
                if (r is bool && (bool)r) SetBadgeLogo((string)dlg.FileName);
            }
            catch (Exception ex) { FedeLog.Error("scelta logo", ex); }
        }

        void DrawIdlePreview()
        {
            var d = S.Idle;
            string c1 = DTPlugin.NormalizeColor(d.Color1, "#FF000000"), c2 = DTPlugin.NormalizeColor(d.Color2, "#FF00E676");
            var sb = new StringBuilder();
            sb.Append("<Border {NS} Width='427' Height='240' CornerRadius='18' ClipToBounds='True' BorderBrush='#FF444444' BorderThickness='1'>");
            sb.Append("<Border.Background>");
            if (d.Mode == 0 && d.Gradient == 1)
                sb.Append("<LinearGradientBrush StartPoint='0,0' EndPoint='0,1'><GradientStop Color='" + c1 + "' Offset='0'/><GradientStop Color='" + c2 + "' Offset='1'/></LinearGradientBrush>");
            else if (d.Mode == 0 && d.Gradient == 2)
                sb.Append("<LinearGradientBrush StartPoint='0,0' EndPoint='1,0'><GradientStop Color='" + c1 + "' Offset='0'/><GradientStop Color='" + c2 + "' Offset='1'/></LinearGradientBrush>");
            else if (d.Mode == 0 && d.Gradient == 3)
                sb.Append("<RadialGradientBrush><GradientStop Color='" + c2 + "' Offset='0'/><GradientStop Color='" + c1 + "' Offset='1'/></RadialGradientBrush>");
            else
                sb.Append("<SolidColorBrush Color='" + c1 + "'/>");
            sb.Append("</Border.Background><Grid>");
            if (d.Mode == 1 && !string.IsNullOrEmpty(d.ImagePath) && System.IO.File.Exists(d.ImagePath))
                sb.Append("<Image Source='" + Wpf.Esc(d.ImagePath) + "' Stretch='UniformToFill'/>");
            if (d.Mode == 2 && d.Animation == 0)
            {
                sb.Append("<Canvas>");
                int[][] st = { new[] { 20, 160, 2 }, new[] { 38, 260, 3 }, new[] { 55, 130, 1 }, new[] { 75, 300, 5 }, new[] { 92, 190, 2 }, new[] { 110, 350, 6 },
                               new[] { 125, 150, 1 }, new[] { 142, 240, 3 }, new[] { 158, 130, 1 }, new[] { 175, 280, 5 }, new[] { 192, 170, 2 }, new[] { 210, 320, 4 } };
                for (int i = 0; i < st.Length; i++)
                    sb.Append("<Rectangle Canvas.Left='" + ((i * 97) % 260 - 60) + "' Canvas.Top='" + st[i][0] + "' Width='" + st[i][1] + "' Height='" + st[i][2] +
                              "' RadiusX='2' RadiusY='2' Fill='" + c2 + "' Opacity='" + Wpf.N(0.25 + (i % 4) * 0.15) + "'/>");
                sb.Append("</Canvas>");
            }
            if (d.Mode == 2 && d.Animation == 1)
                sb.Append("<Rectangle Fill='" + c2 + "' Opacity='0.3'/>");
            if (d.BadgeEnabled)
            {
                double size = d.BadgeSize / 2, bwp = d.BadgeBorderWidth / 2;
                double lw = size * 0.6, lh = lw / d.BadgeLogoAspect;
                if (lh > size * 0.42) { lh = size * 0.42; lw = lh * d.BadgeLogoAspect; }
                sb.Append("<Grid Width='" + Wpf.N(size) + "' Height='" + Wpf.N(size) + "' HorizontalAlignment='Center' VerticalAlignment='Center'>");
                sb.Append("<Ellipse Fill='" + DTPlugin.NormalizeColor(d.BadgeBorder, "#FFFF8C00") + "'/>");
                sb.Append("<Ellipse Margin='" + Wpf.N(bwp) + "' Fill='" + DTPlugin.NormalizeColor(d.BadgeFill, "#FF1A1A1A") + "'/>");
                sb.Append("<StackPanel VerticalAlignment='Center' HorizontalAlignment='Center'>");
                if (!string.IsNullOrEmpty(d.BadgeLogoPath) && System.IO.File.Exists(d.BadgeLogoPath))
                    sb.Append("<Image Source='" + Wpf.Esc(d.BadgeLogoPath) + "' Width='" + Wpf.N(lw) + "' Height='" + Wpf.N(lh) + "' Stretch='Uniform'/>");
                if (!string.IsNullOrEmpty(d.BadgeText))
                    sb.Append("<TextBlock Text='" + Wpf.Esc(d.BadgeText) + "' Foreground='" + DTPlugin.NormalizeColor(d.BadgeTextColor, "#FFFFFFFF") +
                              "' FontWeight='Black' FontSize='" + Wpf.N(Math.Max(6, size * 0.085)) + "' HorizontalAlignment='Center' Margin='0," + Wpf.N(size * 0.05) + ",0,0'/>");
                if (d.BadgeShowNumber && !string.IsNullOrEmpty(d.Number))
                    sb.Append("<TextBlock Text='" + Wpf.Esc(d.Number) + "' Foreground='" + DTPlugin.NormalizeColor(d.NumberColor, "#FFFFFFFF") +
                              "' FontWeight='Black' FontSize='" + Wpf.N(Math.Max(8, size * 0.14)) + "' HorizontalAlignment='Center' Margin='0,0,0,0'/>");
                sb.Append("</StackPanel></Grid>");
            }
            if (d.NumberEnabled && !d.BadgeEnabled)
                sb.Append("<TextBlock Text='" + Wpf.Esc(d.Number) + "' Foreground='" + DTPlugin.NormalizeColor(d.NumberColor, "#FFFFFFFF") +
                          "' FontSize='105' FontWeight='Black' HorizontalAlignment='Center' VerticalAlignment='Top' Margin='0,22,0,0'/>");
            if (d.NameEnabled && !d.BadgeEnabled)
                sb.Append("<TextBlock Text='" + Wpf.Esc(d.Name) + "' Foreground='" + DTPlugin.NormalizeColor(d.NameColor, "#FFFFFFFF") +
                          "' FontSize='22' FontWeight='Black' HorizontalAlignment='Center' VerticalAlignment='Top' Margin='0,150,0,0'/>");
            if (d.ShowClock)
                sb.Append("<TextBlock Text='" + DateTime.Now.ToString("HH:mm") + "' Foreground='White' FontSize='13' FontWeight='Bold' HorizontalAlignment='Right' VerticalAlignment='Top' Margin='0,8,14,0'/>");
            if (d.ShowWaiting)
                sb.Append("<TextBlock Text='In attesa del gioco…' Foreground='Gray' FontSize='9' HorizontalAlignment='Center' VerticalAlignment='Bottom' Margin='0,0,0,12'/>");
            sb.Append("</Grid></Border>");
            try { idlePreview.Content = Wpf.Xaml(sb.ToString()); }
            catch (Exception ex)
            {
                FedeLog.Error("anteprima schermata iniziale", ex);
                idlePreview.Content = Wpf.Xaml("<TextBlock {NS} Foreground='#FFE0E0E0' Text='Anteprima non disponibile (controlla i colori o il percorso immagine)'/>");
            }
        }

        // ---------------------------------------------------------------- esporta / importa / ripristina
        void RebuildAll()
        {
            selKind = ""; selIndex = -1;
            RefreshAll(); BuildAlerts(); BuildTyres(); BuildIdle();
            foreach (var r in rows) r.NotifyAll();
        }

        void ExportSettings()
        {
            try
            {
                dynamic dlg = Activator.CreateInstance(Wpf.T("Microsoft.Win32.SaveFileDialog"));
                dlg.Filter = "Impostazioni DT Plugin|*.json";
                dlg.FileName = "DTPlugin-impostazioni.json";
                object r = dlg.ShowDialog();
                if (r is bool && (bool)r)
                {
                    var acc = S.Access;
                    S.Access = new AccessSettings();   // il codice di accesso non va nel file esportato
                    try { SettingsStore.SaveTo(S, (string)dlg.FileName); } finally { S.Access = acc; }
                    Wpf.Alert("Impostazioni esportate.");
                }
            }
            catch (Exception ex) { FedeLog.Error("esporta", ex); }
        }

        void ImportSettings()
        {
            try
            {
                dynamic dlg = Activator.CreateInstance(Wpf.T("Microsoft.Win32.OpenFileDialog"));
                dlg.Filter = "Impostazioni DT Plugin|*.json|Tutti i file|*.*";
                object r = dlg.ShowDialog();
                if (!(r is bool && (bool)r)) return;
                var ns = SettingsStore.LoadFrom((string)dlg.FileName);
                if (ns == null) { Wpf.Alert("Il file non contiene impostazioni valide del DT Plugin."); return; }
                plugin.ApplySettings(ns);
                RebuildAll();
            }
            catch (Exception ex) { FedeLog.Error("importa", ex); }
        }

        void ResetEverything()
        {
            if (!Wpf.Confirm("Riportare TUTTE le impostazioni (pagine, avvisi, gomme, schermata iniziale, valori) a quelle di partenza?")) return;
            plugin.ApplySettings(FedeSettings.CreateDefault());
            RebuildAll();
        }

        // ---------------------------------------------------------------- gestione pagine
        void AddPage(bool duplicate)
        {
            PageSettings np = duplicate
                ? Page.Clone(Page.Name + " (copia)")
                : Catalog.DefaultPages()[0].Clone("Pagina " + (S.Pages.Count + 1));
            np.AutoPractice = np.AutoQualify = np.AutoRace = false;
            np.Normalize();
            S.Pages.Add(np);
            selKind = ""; selIndex = -1;
            plugin.SetActivePage(S.Pages.Count - 1, true);
            Changed();
        }

        void DeletePage()
        {
            if (S.Pages.Count <= 1)
            {
                Wpf.Alert("Serve almeno una pagina.");
                return;
            }
            if (!Wpf.Confirm("Eliminare la pagina \"" + Page.Name + "\"?")) return;
            int i = S.ActivePage;
            S.ActivePage = 0;
            S.Pages.RemoveAt(i);
            selKind = ""; selIndex = -1;
            plugin.SetActivePage(Math.Max(0, i - 1), false);
            Changed();
        }

        void MovePage(int delta)
        {
            int i = S.ActivePage, j = i + delta;
            if (j < 0 || j >= S.Pages.Count) return;
            var tmp = S.Pages[i];
            S.Pages[i] = S.Pages[j];
            S.Pages[j] = tmp;
            plugin.SetActivePage(j, false);
            Changed();
        }

        void RenamePage()
        {
            if (updating) return;
            string n = ((string)tbName.Text ?? "").Trim();
            if (n.Length == 0 || n == Page.Name) return;
            Page.Name = n;
            Changed();
        }

        // ---------------------------------------------------------------- valori
        void ResetSelected()
        {
            var r = ((object)grid.SelectedItem) as ChannelRow;
            if (r == null) return;
            grid.CancelEdit();
            plugin.ResetChannel(r.Runtime);
            r.NotifyAll();
            Save();
        }

        void ResetAll()
        {
            if (!Wpf.Confirm("Riportare tutti i valori alle impostazioni originali?")) return;
            grid.CancelEdit();
            foreach (var r in rows)
            {
                plugin.ResetChannel(r.Runtime);
                r.NotifyAll();
            }
            Save();
        }
    }

    /// <summary>Riga della tabella "Valori": modifica direttamente le impostazioni del canale.</summary>
    public class ChannelRow : INotifyPropertyChanged
    {
        public readonly ChannelRuntime Runtime;

        public ChannelRow(ChannelRuntime rt)
        {
            Runtime = rt;
        }

        public string Id { get { return Runtime.Def.Id; } }

        public string Label
        {
            get { return Runtime.Settings.Label; }
            set { Runtime.Settings.Label = value ?? ""; Raise("Label"); }
        }

        public string Source
        {
            get { return Runtime.Settings.Source; }
            set { Runtime.Settings.Source = (value ?? "").Trim(); Raise("Source"); }
        }

        public int Decimals
        {
            get { return Runtime.Settings.Decimals; }
            set { Runtime.Settings.Decimals = Math.Max(0, Math.Min(6, value)); Raise("Decimals"); }
        }

        public string Unit
        {
            get { return Runtime.Settings.Unit; }
            set { Runtime.Settings.Unit = value ?? ""; Raise("Unit"); }
        }

        public string Current { get { return Runtime.Text; } }

        public void Refresh()
        {
            Raise("Current");
        }

        public void NotifyAll()
        {
            Raise(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void Raise(string name)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(name));
        }
    }
}
