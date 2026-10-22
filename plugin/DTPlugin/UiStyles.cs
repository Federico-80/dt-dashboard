namespace FedeDashboard
{
    /// <summary>
    /// Aspetto grafico del pannello (interruttori, pulsanti, campi, schede).
    /// Caricato a parte: se per qualche motivo non si carica, il pannello funziona lo stesso con l'aspetto standard.
    /// </summary>
    public static class UiStyles
    {
        public const string Accent = "#FF1E9BD7";
        public const string Toggle = "#FF34C759";
        public const string Panel = "#FF1C1C1C";
        public const string Card = "#FF262626";
        public const string Line = "#FF3E3E3E";
        public const string Muted = "#FF9A9A9A";
        public const string Text = "#FFE8E8E8";

        public const string Dictionary = @"
<ResourceDictionary {NS}>

  <!-- pulsanti: arrotondati, si illuminano al passaggio del mouse -->
  <Style TargetType='Button'>
    <Setter Property='Background' Value='#FF2F2F2F'/>
    <Setter Property='Foreground' Value='#FFE8E8E8'/>
    <Setter Property='BorderBrush' Value='#FF444444'/>
    <Setter Property='BorderThickness' Value='1'/>
    <Setter Property='Padding' Value='12,5'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='FontSize' Value='12.5'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='Button'>
          <Border x:Name='Bd' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                  BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='6' Padding='{TemplateBinding Padding}'>
            <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center' RecognizesAccessKey='False'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'>
              <Setter TargetName='Bd' Property='BorderBrush' Value='#FF1E9BD7'/>
            </Trigger>
            <Trigger Property='IsPressed' Value='True'>
              <Setter TargetName='Bd' Property='Opacity' Value='0.75'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='False'>
              <Setter TargetName='Bd' Property='Opacity' Value='0.35'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <!-- link testuali in alto (Esporta / Importa / Ripristina) -->
  <Style x:Key='LinkButton' TargetType='Button'>
    <Setter Property='Foreground' Value='#FF1E9BD7'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Margin' Value='14,0,0,0'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='Button'>
          <TextBlock x:Name='Tx' Text='{Binding Content, RelativeSource={RelativeSource TemplatedParent}}' Foreground='{TemplateBinding Foreground}' FontSize='12.5'/>
          <ControlTemplate.Triggers>
            <Trigger Property='IsMouseOver' Value='True'>
              <Setter TargetName='Tx' Property='TextDecorations' Value='Underline'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <!-- caselle di spunta come interruttori verdi -->
  <Style TargetType='CheckBox'>
    <Setter Property='Foreground' Value='#FFE8E8E8'/>
    <Setter Property='Cursor' Value='Hand'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='CheckBox'>
          <StackPanel Orientation='Horizontal' Background='Transparent'>
            <Border x:Name='Track' Width='38' Height='21' CornerRadius='10.5' Background='#FF4A4A4A' VerticalAlignment='Center'>
              <Ellipse x:Name='Knob' Width='17' Height='17' Fill='White' HorizontalAlignment='Left' Margin='2,0,0,0'/>
            </Border>
            <ContentPresenter Margin='9,0,0,0' VerticalAlignment='Center' RecognizesAccessKey='False'/>
          </StackPanel>
          <ControlTemplate.Triggers>
            <Trigger Property='IsChecked' Value='True'>
              <Setter TargetName='Track' Property='Background' Value='#FF34C759'/>
              <Setter TargetName='Knob' Property='HorizontalAlignment' Value='Right'/>
              <Setter TargetName='Knob' Property='Margin' Value='0,0,2,0'/>
            </Trigger>
            <Trigger Property='IsEnabled' Value='False'>
              <Setter Property='Opacity' Value='0.4'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <!-- campi di testo scuri -->
  <Style TargetType='TextBox'>
    <Setter Property='Background' Value='#FF141414'/>
    <Setter Property='Foreground' Value='#FFEDEDED'/>
    <Setter Property='BorderBrush' Value='#FF3E3E3E'/>
    <Setter Property='BorderThickness' Value='1'/>
    <Setter Property='CaretBrush' Value='White'/>
    <Setter Property='Padding' Value='7,5'/>
    <Setter Property='FontSize' Value='12.5'/>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='TextBox'>
          <Border x:Name='Bd' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}'
                  BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='5'>
            <ScrollViewer x:Name='PART_ContentHost' Margin='{TemplateBinding Padding}' VerticalAlignment='Center'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsKeyboardFocused' Value='True'>
              <Setter TargetName='Bd' Property='BorderBrush' Value='#FF1E9BD7'/>
            </Trigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <!-- schede in alto -->
  <Style TargetType='TabControl'>
    <Setter Property='Background' Value='Transparent'/>
    <Setter Property='BorderBrush' Value='#FF1E9BD7'/>
    <Setter Property='BorderThickness' Value='0,2,0,0'/>
    <Setter Property='Padding' Value='0'/>
  </Style>
  <Style TargetType='TabItem'>
    <Setter Property='Template'>
      <Setter.Value>
        <ControlTemplate TargetType='TabItem'>
          <Border x:Name='Bd' Background='#FF2C2C2C' CornerRadius='7,7,0,0' Padding='16,8' Margin='0,0,4,0' Cursor='Hand'>
            <ContentPresenter x:Name='Hd' ContentSource='Header' TextElement.Foreground='#FFE0E0E0' TextElement.FontWeight='SemiBold'
                              TextElement.FontSize='13' RecognizesAccessKey='False'/>
          </Border>
          <ControlTemplate.Triggers>
            <Trigger Property='IsSelected' Value='True'>
              <Setter TargetName='Bd' Property='Background' Value='#FF1E9BD7'/>
              <Setter TargetName='Hd' Property='TextElement.Foreground' Value='White'/>
            </Trigger>
            <MultiTrigger>
              <MultiTrigger.Conditions>
                <Condition Property='IsMouseOver' Value='True'/>
                <Condition Property='IsSelected' Value='False'/>
              </MultiTrigger.Conditions>
              <Setter TargetName='Bd' Property='Background' Value='#FF3A3A3A'/>
            </MultiTrigger>
          </ControlTemplate.Triggers>
        </ControlTemplate>
      </Setter.Value>
    </Setter>
  </Style>

  <Style TargetType='ToolTip'>
    <Setter Property='Background' Value='#FF2A2A2A'/>
    <Setter Property='Foreground' Value='#FFE8E8E8'/>
    <Setter Property='BorderBrush' Value='#FF555555'/>
  </Style>
</ResourceDictionary>";
    }
}
