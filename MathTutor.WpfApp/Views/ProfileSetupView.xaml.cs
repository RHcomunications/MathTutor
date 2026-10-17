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
        // Edades de 3 a 12
        CmbAge.Items.Clear();
        for (int i = 3; i <= 12; i++)
        {
            CmbAge.Items.Add(new ComboBoxItem { Content = $"{i} años", Tag = i });
        }

        // Niveles académicos
        CmbAcademicLevel.Items.Clear();
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Jardín de Niños (3 a 5 años)", Tag = AcademicLevel.Kindergarten });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Primaria Baja (6 a 8 años - 1º y 2º)", Tag = AcademicLevel.EarlyElementary });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Primaria Media (9 a 10 años - 3º y 4º)", Tag = AcademicLevel.MiddleElementary });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Primaria Alta (11 a 12 años - 5º y 6º)", Tag = AcademicLevel.UpperElementary });
        CmbAcademicLevel.Items.Add(new ComboBoxItem { Content = "Fundamentos de Programación y Algoritmos", Tag = AcademicLevel.ProgrammingFoundations });

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

    private void CmbDisabilityType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbDisabilityType.SelectedItem is ComboBoxItem item && item.Tag is DisabilityType dis)
        {
            _acc.CurrentProfile.Disability = dis;
            _acc.CurrentProfile.ApplyRecommendedAccessibilityDefaults();

            // Sincronizar UI con las recomendaciones
            _isInitializing = true;
            ChkVoiceSynthesis.IsChecked = _acc.CurrentProfile.Preferences.EnableVoiceSynthesis;
            ChkAudioCues.IsChecked = _acc.CurrentProfile.Preferences.PlayAudioCues;
            ChkSimplifyLanguage.IsChecked = _acc.CurrentProfile.Preferences.SimplifyLanguage;
            SelectComboByTag(CmbTheme, _acc.CurrentProfile.Preferences.Theme);
            SelectComboByTag(CmbFontSize, _acc.CurrentProfile.Preferences.FontSize);
            _isInitializing = false;

            _acc.Announce($"Seleccionado: {item.Content}. Opciones de accesibilidad adaptadas automáticamente.");
        }
    }

    private void CmbDisabilitySeverity_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbDisabilitySeverity.SelectedItem is ComboBoxItem item && item.Tag is DisabilitySeverity sev)
        {
            _acc.CurrentProfile.Severity = sev;
            _acc.Announce($"Severidad: {item.Content}");
        }
    }

    private void CmbAge_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbAge.SelectedItem is ComboBoxItem item && item.Tag is int age)
        {
            _acc.CurrentProfile.Age = age;
            if (age <= 5)
            {
                SelectComboByTag(CmbAcademicLevel, AcademicLevel.Kindergarten);
            }
            else if (age <= 8)
            {
                SelectComboByTag(CmbAcademicLevel, AcademicLevel.EarlyElementary);
            }
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
