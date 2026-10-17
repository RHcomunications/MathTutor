using System;
using System.Windows;
using System.Windows.Controls;
using MathTutor.Core.Models;
using MathTutor.WpfApp.Services;

namespace MathTutor.WpfApp.Views;

public partial class ProfileSetupView : UserControl
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private bool _isInitializing = true;
    public event Action? ProfileSavedAndReady;

    public ProfileSetupView()
    {
        InitializeComponent();
        Loaded += ProfileSetupView_Loaded;
    }

    private void ProfileSetupView_Loaded(object sender, RoutedEventArgs e)
    {
        _isInitializing = true;
        PopulateDropdowns();
        LoadCurrentProfileData();
        _isInitializing = false;

        // Anunciar por voz la pantalla actual
        _acc.Announce("Pantalla de caracterización del alumno. Configura tu nombre, edad, nivel y condición para personalizar tu experiencia de aprendizaje.", speak: true, playTone: true);

        // Poner el foco en el campo de nombre
        TxtName.Focus();
    }

    private void PopulateDropdowns()
    {
        // Edades de 3 a 18
        CmbAge.Items.Clear();
        for (int i = 3; i <= 18; i++)
        {
            CmbAge.Items.Add(new ComboBoxItem { Content = $"{i} años", Tag = i });
        }

        // Niveles académicos completos
        CmbAcademicLevel.Items.Clear();
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Jardín de Niños (3 a 5 años)", Tag = AcademicLevel.Kindergarten });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Primaria Baja (6 a 8 años - 1º y 2º)", Tag = AcademicLevel.EarlyElementary });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Primaria Media (9 a 10 años - 3º y 4º)", Tag = AcademicLevel.MiddleElementary });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Primaria Alta (11 a 12 años - 5º y 6º)", Tag = AcademicLevel.UpperElementary });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "1º de Secundaria (12 a 13 años)", Tag = AcademicLevel.SecondaryFirstGrade });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "2º de Secundaria (13 a 14 años)", Tag = AcademicLevel.SecondarySecondGrade });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "3º de Secundaria (14 a 15 años)", Tag = AcademicLevel.SecondaryThirdGrade });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "1º de Preparatoria / Bachillerato (15 a 16 años)", Tag = AcademicLevel.HighSchoolFirstGrade });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "2º de Preparatoria / Bachillerato (16 a 17 años)", Tag = AcademicLevel.HighSchoolSecondGrade });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "3º de Preparatoria / Bachillerato (17 a 18 años)", Tag = AcademicLevel.HighSchoolThirdGrade });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Fundamentos de Programación y Algoritmos", Tag = AcademicLevel.ProgrammingFoundations });

        // Región / País de América Latina
        CmbRegion.Items.Clear();
        CmbRegion.Items.Add(new ComboBoxItem { Content = "México (Pesos $ y Centavos - Tiendita / Mercado)", Tag = RegionalContext.Mexico });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Colombia (Pesos $ - Tienda de Barrio)", Tag = RegionalContext.Colombia });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Argentina (Pesos $ y Centavos - Kiosco)", Tag = RegionalContext.Argentina });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Perú (Soles S/ y Céntimos - Bodega)", Tag = RegionalContext.Peru });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Chile (Pesos $ - Almacén)", Tag = RegionalContext.Chile });

        // Tipos de discapacidad
        CmbDisabilityType.Items.Clear();
        CmbDisabilityType.Items.Add(new ComboBoxItem { Content = "Sin discapacidad (Neurotípico)", Tag = DisabilityType.None });
        CmbDisabilityType.Items.Add(new ComboBoxItem { Content = "Discapacidad Visual (Ceguera o Baja Visión)", Tag = DisabilityType.Visual });
        CmbDisabilityType.Items.Add(new ComboBoxItem { Content = "Discapacidad Auditiva (Hipoacusia o Sordera)", Tag = DisabilityType.Hearing });
        CmbDisabilityType.Items.Add(new ComboBoxItem { Content = "Discapacidad Motriz (Dificultad motora fina)", Tag = DisabilityType.Motor });
        CmbDisabilityType.Items.Add(new ComboBoxItem { Content = "Discapacidad Intelectual / Cognitiva", Tag = DisabilityType.Intellectual });
        CmbDisabilityType.Items.Add(new ComboBoxItem { Content = "Neurodivergente (TDAH o Espectro Autista)", Tag = DisabilityType.Neurodivergent });

        // Severidad
        CmbDisabilitySeverity.Items.Clear();
        CmbDisabilitySeverity.Items.Add(new ComboBoxItem { Content = "Leve", Tag = DisabilitySeverity.Mild });
        CmbDisabilitySeverity.Items.Add(new ComboBoxItem { Content = "Moderada", Tag = DisabilitySeverity.Moderate });
        CmbDisabilitySeverity.Items.Add(new ComboBoxItem { Content = "Severa", Tag = DisabilitySeverity.Severe });

        // Temas de contraste
        CmbTheme.Items.Clear();
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Colorido Lúdico (Estándar Infantil)", Tag = HighContrastTheme.DefaultPlayful });
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Alto Contraste: Negro y Amarillo", Tag = HighContrastTheme.HighContrastDark });
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Alto Contraste: Blanco y Negro", Tag = HighContrastTheme.HighContrastLight });
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Baja Estimulación Sensorial (Pastel)", Tag = HighContrastTheme.LowSensory });

        // Tamaños de fuente
        CmbFontSize.Items.Clear();
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Estándar (18 pt)", Tag = FontSizeLevel.Normal });
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Grande (23 pt)", Tag = FontSizeLevel.Large });
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Muy Grande (28 pt)", Tag = FontSizeLevel.ExtraLarge });
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Gigante (34 pt)", Tag = FontSizeLevel.Huge });
    }

    private void LoadCurrentProfileData()
    {
        var profile = _acc.CurrentProfile;
        TxtName.Text = profile.Name;

        SelectComboByTag(CmbAge, profile.Age);
        SelectComboByTag(CmbAcademicLevel, profile.CurrentLevel);
        SelectComboByTag(CmbRegion, profile.Region);
        SelectComboByTag(CmbDisabilityType, profile.Disability);
        SelectComboByTag(CmbDisabilitySeverity, profile.Severity);
        SelectComboByTag(CmbTheme, profile.Preferences.Theme);
        SelectComboByTag(CmbFontSize, profile.Preferences.FontSize);

        ChkVoiceSynthesis.IsChecked = profile.Preferences.EnableVoiceSynthesis;
        ChkAudioCues.IsChecked = profile.Preferences.PlayAudioCues;
        ChkSimplifyLanguage.IsChecked = profile.Preferences.SimplifyLanguage;
    }

    private static void SelectComboByTag(ComboBox combo, object tagValue)
    {
        foreach (ComboBoxItem item in combo.Items)
        {
            if (Equals(item.Tag, tagValue))
            {
                combo.SelectedItem = item;
                break;
            }
        }
        if (combo.SelectedItem == null && combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }
    }

    private void SyncAccessibilityControlsFromProfile()
    {
        _isInitializing = true;
        ChkVoiceSynthesis.IsChecked = _acc.CurrentProfile.Preferences.EnableVoiceSynthesis;
        ChkAudioCues.IsChecked = _acc.CurrentProfile.Preferences.PlayAudioCues;
        ChkSimplifyLanguage.IsChecked = _acc.CurrentProfile.Preferences.SimplifyLanguage;
        SelectComboByTag(CmbTheme, _acc.CurrentProfile.Preferences.Theme);
        SelectComboByTag(CmbFontSize, _acc.CurrentProfile.Preferences.FontSize);
        _isInitializing = false;
    }

    private void CmbDisabilityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbDisabilityType.SelectedItem is ComboBoxItem item && item.Tag is DisabilityType dis)
        {
            _acc.CurrentProfile.Disability = dis;
            _acc.CurrentProfile.ApplyRecommendedAccessibilityDefaults();
            SyncAccessibilityControlsFromProfile();

            _acc.Announce($"Seleccionado: {item.Content}. Opciones de accesibilidad adaptadas automáticamente.");
        }
    }

    private void CmbDisabilitySeverity_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbDisabilitySeverity.SelectedItem is ComboBoxItem item && item.Tag is DisabilitySeverity sev)
        {
            _acc.CurrentProfile.Severity = sev;
            _acc.CurrentProfile.ApplyRecommendedAccessibilityDefaults();
            SyncAccessibilityControlsFromProfile();

            _acc.Announce($"Severidad: {item.Content}. Ajustes de accesibilidad actualizados.");
        }
    }

    private void CmbAge_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbAge.SelectedItem is ComboBoxItem item && item.Tag is int age)
        {
            _acc.CurrentProfile.Age = age;
            var suggestedLevel = age switch
            {
                <= 5 => AcademicLevel.Kindergarten,
                <= 8 => AcademicLevel.EarlyElementary,
                <= 10 => AcademicLevel.MiddleElementary,
                11 => AcademicLevel.UpperElementary,
                12 => AcademicLevel.SecondaryFirstGrade,
                13 => AcademicLevel.SecondarySecondGrade,
                14 => AcademicLevel.SecondaryThirdGrade,
                15 => AcademicLevel.HighSchoolFirstGrade,
                16 => AcademicLevel.HighSchoolSecondGrade,
                _ => AcademicLevel.HighSchoolThirdGrade
            };
            SelectComboByTag(CmbAcademicLevel, suggestedLevel);
            _acc.Announce($"Edad: {age} años");
        }
    }

    private void CmbAcademicLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbAcademicLevel.SelectedItem is ComboBoxItem item && item.Tag is AcademicLevel level)
        {
            _acc.CurrentProfile.CurrentLevel = level;
            _acc.Announce($"Nivel académico: {item.Content}");
        }
    }

    private void CmbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbRegion.SelectedItem is ComboBoxItem item && item.Tag is RegionalContext region)
        {
            _acc.CurrentProfile.Region = region;
            _acc.Announce($"País y contexto cultural: {item.Content}");
        }
    }

    private void CmbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbTheme.SelectedItem is ComboBoxItem item && item.Tag is HighContrastTheme theme)
        {
            _acc.ApplyTheme(theme);
        }
    }

    private void CmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbFontSize.SelectedItem is ComboBoxItem item && item.Tag is FontSizeLevel font)
        {
            _acc.ApplyFontSize(font);
        }
    }

    private void ChkVoiceSynthesis_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        _acc.CurrentProfile.Preferences.EnableVoiceSynthesis = ChkVoiceSynthesis.IsChecked == true;
        _acc.Speech.Enabled = _acc.CurrentProfile.Preferences.EnableVoiceSynthesis;
        _acc.Announce(_acc.Speech.Enabled ? "Síntesis de voz habilitada" : "Síntesis de voz deshabilitada");
    }

    private void ChkAudioCues_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        _acc.CurrentProfile.Preferences.PlayAudioCues = ChkAudioCues.IsChecked == true;
        _acc.Audio.Enabled = _acc.CurrentProfile.Preferences.PlayAudioCues;
        if (_acc.Audio.Enabled)
        {
            _acc.Audio.PlaySuccessChord();
        }
    }

    private void ChkSimplifyLanguage_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        _acc.CurrentProfile.Preferences.SimplifyLanguage = ChkSimplifyLanguage.IsChecked == true;
        _acc.CurrentProfile.Preferences.ReduceOptionsCount = ChkSimplifyLanguage.IsChecked == true;
        _acc.Announce(_acc.CurrentProfile.Preferences.SimplifyLanguage ? "Lenguaje simplificado activado" : "Lenguaje estándar activado");
    }

    private void Control_GotFocus(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
    }

    private void BtnSaveAndStart_Click(object sender, RoutedEventArgs e)
    {
        _acc.CurrentProfile.Name = string.IsNullOrWhiteSpace(TxtName.Text) ? "Aventurero" : TxtName.Text.Trim();
        _acc.SaveCurrentProfile();
        _acc.Audio.PlaySuccessChord();
        _acc.Announce($"¡Perfil guardado, {_acc.CurrentProfile.Name}! Entrando al Reino Matemágico...", speak: true);
        ProfileSavedAndReady?.Invoke();
    }
}
