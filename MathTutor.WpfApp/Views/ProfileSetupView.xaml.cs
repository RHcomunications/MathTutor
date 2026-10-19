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
    private UserProfile? _snapshot;
    public event Action? ProfileSavedAndReady;
    public event Action? CancelRequested;

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

        // Snapshot del último perfil guardado para poder descartar cambios al volver.
        _snapshot = _acc.Storage.Load();
        BtnCancel.Visibility = _snapshot != null ? Visibility.Visible : Visibility.Collapsed;

        // Tarea 1: Poner el foco real de teclado en el primer control interactivo antes de anunciar
        TxtName.Focus();
        System.Windows.Input.Keyboard.Focus(TxtName);

        // Anunciar la pantalla actual al lector de pantalla del sistema (NVDA, JAWS o Narrador)
        _acc.Announce("Bienvenido a MathTutor. Pantalla de caracterización del alumno. Presiona Tab para navegar por las opciones.", speak: true, playTone: true);
    }

    private void PopulateDropdowns()
    {
        // Edades de 3 a 18
        CmbAge.Items.Clear();
        for (int i = 3; i <= 18; i++)
        {
            CmbAge.Items.Add(new ComboBoxItem { Content = $"{i} años", Tag = i });
        }

        // Tarea 6: Poblar dinámicamente solo los niveles escolares permitidos según la edad del alumno
        PopulateAcademicLevelsForAge(_acc.CurrentProfile.Age);

        // Región / País de América Latina
        CmbRegion.Items.Clear();
        CmbRegion.Items.Add(new ComboBoxItem { Content = "México (Pesos $ y Centavos - Tiendita / Mercado)", Tag = RegionalContext.Mexico });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Colombia (Pesos $ - Tienda de Barrio)", Tag = RegionalContext.Colombia });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Argentina (Pesos $ y Centavos - Kiosco)", Tag = RegionalContext.Argentina });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Perú (Soles S/ y Céntimos - Bodega)", Tag = RegionalContext.Peru });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Chile (Pesos $ - Almacén)", Tag = RegionalContext.Chile });

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

    private void PopulateAcademicLevelsForAge(int age)
    {
        var profile = _acc.CurrentProfile;
        var allowedLevels = profile.Disabilities.Count > 0
            ? MathTutor.Core.Engine.AdaptiveEngine.GetAllowedLevelsForAge(age, profile.Disabilities)
            : MathTutor.Core.Engine.AdaptiveEngine.GetAllowedLevelsForAge(age, profile.Disability);
        CmbAcademicLevel.Items.Clear();

        foreach (var level in allowedLevels)
        {
            CmbAcademicLevel.Items.Add(new ComboBoxItem
            {
                Content = GetAcademicLevelDisplayLabel(level),
                Tag = level
            });
        }
    }

    private static string GetAcademicLevelDisplayLabel(AcademicLevel level) => level switch
    {
        AcademicLevel.Kindergarten => "Jardín de Niños (3 a 5 años)",
        AcademicLevel.EarlyElementary => "Primaria Baja (6 a 8 años - 1º y 2º)",
        AcademicLevel.MiddleElementary => "Primaria Media (9 a 10 años - 3º y 4º)",
        AcademicLevel.UpperElementary => "Primaria Alta (11 a 12 años - 5º y 6º)",
        AcademicLevel.SecondaryFirstGrade => "1º de Secundaria (12 a 13 años)",
        AcademicLevel.SecondarySecondGrade => "2º de Secundaria (13 a 14 años)",
        AcademicLevel.SecondaryThirdGrade => "3º de Secundaria (14 a 15 años)",
        AcademicLevel.HighSchoolFirstGrade => "1º de Preparatoria / Bachillerato (15 a 16 años)",
        AcademicLevel.HighSchoolSecondGrade => "2º de Preparatoria / Bachillerato (16 a 17 años)",
        AcademicLevel.HighSchoolThirdGrade => "3º de Preparatoria / Bachillerato (17 a 18 años)",
        AcademicLevel.ProgrammingFoundations => "Fundamentos de Programación y Algoritmos",
        _ => "Jardín de Niños (3 a 5 años)"
    };

    private void LoadCurrentProfileData()
    {
        var profile = _acc.CurrentProfile;
        TxtName.Text = profile.Name;

        SelectComboByTag(CmbAge, profile.Age);
        PopulateAcademicLevelsForAge(profile.Age);

        var allowed = profile.Disabilities.Count > 0
            ? MathTutor.Core.Engine.AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disabilities)
            : MathTutor.Core.Engine.AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disability);
        if (!System.Linq.Enumerable.Contains(allowed, profile.CurrentLevel))
        {
            profile.CurrentLevel = MathTutor.Core.Engine.AdaptiveEngine.GetRecommendedLevelForAge(profile.Age);
        }

        SelectComboByTag(CmbAcademicLevel, profile.CurrentLevel);
        SelectComboByTag(CmbRegion, profile.Region);

        _isInitializing = true;
        ChkDisabilityVisual.IsChecked = profile.HasDisability(DisabilityType.Visual);
        ChkDisabilityHearing.IsChecked = profile.HasDisability(DisabilityType.Hearing);
        ChkDisabilityMotor.IsChecked = profile.HasDisability(DisabilityType.Motor);
        ChkDisabilityIntellectual.IsChecked = profile.HasDisability(DisabilityType.Intellectual);
        ChkDisabilityNeurodivergent.IsChecked = profile.HasDisability(DisabilityType.Neurodivergent);
        _isInitializing = false;

        CmbDisabilitySeverity.IsEnabled = profile.Disabilities.Count > 0;
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

    private void ChkDisability_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;

        var profile = _acc.CurrentProfile;
        profile.Disabilities.Clear();

        if (ChkDisabilityVisual.IsChecked == true) profile.Disabilities.Add(DisabilityType.Visual);
        if (ChkDisabilityHearing.IsChecked == true) profile.Disabilities.Add(DisabilityType.Hearing);
        if (ChkDisabilityMotor.IsChecked == true) profile.Disabilities.Add(DisabilityType.Motor);
        if (ChkDisabilityIntellectual.IsChecked == true) profile.Disabilities.Add(DisabilityType.Intellectual);
        if (ChkDisabilityNeurodivergent.IsChecked == true) profile.Disabilities.Add(DisabilityType.Neurodivergent);

        profile.Disability = profile.Disabilities.Count > 0 ? profile.Disabilities[0] : DisabilityType.None;
        CmbDisabilitySeverity.IsEnabled = profile.Disabilities.Count > 0;
        profile.ApplyRecommendedAccessibilityDefaults();

        _isInitializing = true;
        PopulateAcademicLevelsForAge(profile.Age);
        var allowed = MathTutor.Core.Engine.AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disabilities);
        if (!System.Linq.Enumerable.Contains(allowed, profile.CurrentLevel))
        {
            profile.CurrentLevel = MathTutor.Core.Engine.AdaptiveEngine.GetRecommendedLevelForAge(profile.Age);
        }
        SelectComboByTag(CmbAcademicLevel, profile.CurrentLevel);
        _isInitializing = false;

        SyncAccessibilityControlsFromProfile();
        _acc.ApplyTheme(profile.Preferences.Theme);
        _acc.ApplyFontSize(profile.Preferences.FontSize);

        string summary = profile.Disabilities.Count > 0
            ? string.Join(", ", profile.Disabilities.Select(d => d.ToString()))
            : "Sin discapacidad";
        _acc.Announce($"Condiciones actualizadas: {summary}. Opciones de accesibilidad adaptadas simultáneamente.");
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
            var suggestedLevel = MathTutor.Core.Engine.AdaptiveEngine.GetRecommendedLevelForAge(age);

            _isInitializing = true;
            PopulateAcademicLevelsForAge(age);
            SelectComboByTag(CmbAcademicLevel, suggestedLevel);
            _isInitializing = false;

            _acc.CurrentProfile.CurrentLevel = suggestedLevel;
            _acc.Announce($"Edad: {age} años. Nivel escolar ajustado a {GetAcademicLevelDisplayLabel(suggestedLevel)}.");
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
        bool enabled = ChkVoiceSynthesis.IsChecked == true;
        _acc.CurrentProfile.Preferences.EnableVoiceSynthesis = enabled;
        _acc.IsInternalAudioEnabled = enabled && _acc.CurrentProfile.Preferences.PlayAudioCues;
        _acc.Audio.Enabled = _acc.IsInternalAudioEnabled;
        _acc.IsScreenReaderIntegrationEnabled = true;
        _acc.Announce(enabled ? "Audio interno habilitado. El lector de pantalla permanece activo." : "Audio interno deshabilitado. El lector de pantalla permanece activo.");
    }

    private void ChkAudioCues_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        bool enabled = ChkAudioCues.IsChecked == true;
        _acc.CurrentProfile.Preferences.PlayAudioCues = enabled;
        _acc.IsInternalAudioEnabled = enabled;
        _acc.Audio.Enabled = enabled;
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

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (_snapshot != null)
        {
            _acc.ApplyProfile(_snapshot);
            _acc.SaveCurrentProfile();
        }

        _acc.Audio.PlayNavigationClick();
        CancelRequested?.Invoke();
    }
}
