using System;
using System.Collections.Generic;
using System.Linq;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;

namespace MathTutor.WinUI.Views;

public sealed partial class ProfilePage : Page
{
    private const string PrivacyNoticeText =
        "MathTutor funciona sin conexión y no recopila datos para publicidad ni análisis. " +
        "El nombre, la edad, las condiciones o apoyos que decidas indicar y el progreso se guardan " +
        "solo en este equipo, en %LocalAppData%\\MathTutor\\user_profile.json. " +
        "No es obligatorio declarar un diagnóstico: puedes dejar todo sin marcar o elegir 'Sin especificar'. " +
        "La aplicación no envía esta información a internet. " +
        "Puedes exportar una copia en un archivo JSON o borrar el perfil desde esta pantalla. " +
        "Al borrar el perfil se elimina el archivo local y se reinicia la caracterización; " +
        "las copias que hayas exportado no se borran automáticamente. " +
        "Si compartes el archivo exportado, revisa que no incluya datos que prefieras mantener privados.";

    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private bool _isInitializing = true;
    private bool _supportSelectionDirty;
    private UserProfile? _snapshot;

    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _isInitializing = true;
        PopulateDropdowns();
        LoadCurrentProfileData();
        _isInitializing = false;

        _snapshot = _acc.Storage.Load();
        BtnCancel.Visibility = _snapshot is not null ? Visibility.Visible : Visibility.Collapsed;

        DispatcherQueue.TryEnqueue(() => TxtName.Focus(FocusState.Programmatic));

        _acc.Announce(
            "Bienvenido a MathTutor. Pantalla de caracterización del alumno. Presiona Tab para navegar por las opciones.",
            speak: true,
            playTone: true);
    }

    private void PopulateDropdowns()
    {
        CmbAge.Items.Clear();
        for (int i = 3; i <= 18; i++)
        {
            CmbAge.Items.Add(new ComboBoxItem { Content = $"{i} años", Tag = i });
        }

        PopulateAcademicLevelsForAge(_acc.CurrentProfile.Age);

        CmbRegion.Items.Clear();
        CmbRegion.Items.Add(new ComboBoxItem { Content = "México (Pesos $ y Centavos - Tiendita / Mercado)", Tag = RegionalContext.Mexico });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Colombia (Pesos $ - Tienda de Barrio)", Tag = RegionalContext.Colombia });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Argentina (Pesos $ y Centavos - Kiosco)", Tag = RegionalContext.Argentina });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Perú (Soles S/ y Céntimos - Bodega)", Tag = RegionalContext.Peru });
        CmbRegion.Items.Add(new ComboBoxItem { Content = "Chile (Pesos $ - Almacén)", Tag = RegionalContext.Chile });

        PopulateSupportDropdowns();

        CmbTheme.Items.Clear();
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Colorido Lúdico (Estándar Infantil)", Tag = HighContrastTheme.DefaultPlayful });
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Alto Contraste: Negro y Amarillo", Tag = HighContrastTheme.HighContrastDark });
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Alto Contraste: Blanco y Negro", Tag = HighContrastTheme.HighContrastLight });
        CmbTheme.Items.Add(new ComboBoxItem { Content = "Baja Estimulación Sensorial (Pastel)", Tag = HighContrastTheme.LowSensory });

        CmbFontSize.Items.Clear();
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Estándar (18 pt)", Tag = FontSizeLevel.Normal });
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Grande (23 pt)", Tag = FontSizeLevel.Large });
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Muy Grande (28 pt)", Tag = FontSizeLevel.ExtraLarge });
        CmbFontSize.Items.Add(new ComboBoxItem { Content = "Gigante (34 pt)", Tag = FontSizeLevel.Huge });
    }

    private void PopulateSupportDropdowns()
    {
        AddSupportItems(CmbAutismSupport,
            ("Sin especificar", AutismSupportLevel.NotApplicable),
            ("Nivel 1: requiere apoyo", AutismSupportLevel.Level1),
            ("Nivel 2: requiere apoyo sustancial", AutismSupportLevel.Level2),
            ("Nivel 3: requiere apoyo muy sustancial", AutismSupportLevel.Level3));

        AddSupportItems(CmbAdhdPresentation,
            ("Sin especificar", AdhdPresentation.NotApplicable),
            ("Predominio inatento", AdhdPresentation.PredominantlyInattentive),
            ("Predominio hiperactivo-impulsivo", AdhdPresentation.PredominantlyHyperactiveImpulsive),
            ("Presentación combinada", AdhdPresentation.Combined));

        AddSupportItems(CmbIntellectualSupport,
            ("Sin especificar", IntellectualSupportLevel.NotApplicable),
            ("Apoyo intermitente", IntellectualSupportLevel.Intermittent),
            ("Apoyo limitado", IntellectualSupportLevel.Limited),
            ("Apoyo extenso", IntellectualSupportLevel.Extensive),
            ("Apoyo generalizado", IntellectualSupportLevel.Pervasive));

        AddSupportItems(CmbVisualFunction,
            ("Sin especificar", VisualFunctionalLevel.NotApplicable),
            ("Baja visión", VisualFunctionalLevel.LowVision),
            ("Ceguera", VisualFunctionalLevel.Blindness));

        AddSupportItems(CmbAuditoryFunction,
            ("Sin especificar", AuditoryFunctionalLevel.NotApplicable),
            ("Hipoacusia", AuditoryFunctionalLevel.HardOfHearing),
            ("Sordera", AuditoryFunctionalLevel.Deaf));

        AddSupportItems(CmbMotorFunction,
            ("Sin especificar", MotorFunctionalLevel.NotApplicable),
            ("Dificultades motoras finas", MotorFunctionalLevel.FineMotorDifficulties),
            ("Movilidad reducida", MotorFunctionalLevel.ReducedMobility),
            ("Sin uso de extremidades superiores", MotorFunctionalLevel.NoUpperLimbUse));
    }

    private static void AddSupportItems<T>(ComboBox combo, params (string Label, T Value)[] items)
    {
        combo.Items.Clear();
        foreach (var (label, value) in items)
        {
            combo.Items.Add(new ComboBoxItem { Content = label, Tag = value! });
        }
    }

    private static T GetSelectedSupport<T>(ComboBox combo, T fallback)
    {
        return combo.SelectedItem is ComboBoxItem item && item.Tag is T value ? value : fallback;
    }

    private void PopulateAcademicLevelsForAge(int age)
    {
        var profile = _acc.CurrentProfile;
        var allowedLevels = profile.Disabilities.Count > 0
            ? AdaptiveEngine.GetAllowedLevelsForAge(age, profile.Disabilities)
            : AdaptiveEngine.GetAllowedLevelsForAge(age, profile.Disability);

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
            ? AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disabilities)
            : AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disability);
        if (!allowed.Contains(profile.CurrentLevel))
        {
            profile.CurrentLevel = AdaptiveEngine.GetRecommendedLevelForAge(profile.Age);
        }

        SelectComboByTag(CmbAcademicLevel, profile.CurrentLevel);
        SelectComboByTag(CmbRegion, profile.Region);

        ChkDisabilityVisual.IsChecked = profile.HasDisability(DisabilityType.Visual);
        ChkDisabilityHearing.IsChecked = profile.HasDisability(DisabilityType.Hearing);
        ChkDisabilityMotor.IsChecked = profile.HasDisability(DisabilityType.Motor);
        ChkDisabilityIntellectual.IsChecked = profile.HasDisability(DisabilityType.Intellectual);
        ChkDisabilityAutism.IsChecked = profile.HasDisability(DisabilityType.Autism);
        ChkDisabilityAdhd.IsChecked = profile.HasDisability(DisabilityType.ADHD);

        UpdateSupportPanelsVisibility();
        SelectComboByTag(CmbAutismSupport, profile.Support.AutismSupport);
        SelectComboByTag(CmbAdhdPresentation, profile.Support.AdhdPresentation);
        SelectComboByTag(CmbIntellectualSupport, profile.Support.IntellectualSupport);
        SelectComboByTag(CmbVisualFunction, profile.Support.VisualFunction);
        SelectComboByTag(CmbAuditoryFunction, profile.Support.AuditoryFunction);
        SelectComboByTag(CmbMotorFunction, profile.Support.MotorFunction);
        SelectComboByTag(CmbTheme, profile.Preferences.Theme);
        SelectComboByTag(CmbFontSize, profile.Preferences.FontSize);

        ChkInternalAudio.IsChecked = profile.Preferences.EnableVoiceSynthesis;
        ChkScreenReaderAnnouncements.IsChecked = profile.Preferences.ScreenReaderAnnouncements;
        ChkAudioCues.IsChecked = profile.Preferences.PlayAudioCues;
        ChkSimplifyLanguage.IsChecked = profile.Preferences.SimplifyLanguage;
    }

    private void UpdateSupportPanelsVisibility()
    {
        PnlAutismSupport.Visibility = ChkDisabilityAutism.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PnlAdhdPresentation.Visibility = ChkDisabilityAdhd.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PnlIntellectualSupport.Visibility = ChkDisabilityIntellectual.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PnlVisualFunction.Visibility = ChkDisabilityVisual.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PnlAuditoryFunction.Visibility = ChkDisabilityHearing.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PnlMotorFunction.Visibility = ChkDisabilityMotor.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void SelectComboByTag(ComboBox combo, object tagValue)
    {
        foreach (var item in combo.Items)
        {
            if (item is ComboBoxItem comboItem && Equals(comboItem.Tag, tagValue))
            {
                combo.SelectedItem = comboItem;
                break;
            }
        }

        if (combo.SelectedItem is null && combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }
    }

    private void SyncAccessibilityControlsFromProfile()
    {
        _isInitializing = true;
        var profile = _acc.CurrentProfile;
        ChkInternalAudio.IsChecked = profile.Preferences.EnableVoiceSynthesis;
        ChkScreenReaderAnnouncements.IsChecked = profile.Preferences.ScreenReaderAnnouncements;
        ChkAudioCues.IsChecked = profile.Preferences.PlayAudioCues;
        ChkSimplifyLanguage.IsChecked = profile.Preferences.SimplifyLanguage;
        SelectComboByTag(CmbTheme, profile.Preferences.Theme);
        SelectComboByTag(CmbFontSize, profile.Preferences.FontSize);
        SelectComboByTag(CmbAutismSupport, profile.Support.AutismSupport);
        SelectComboByTag(CmbAdhdPresentation, profile.Support.AdhdPresentation);
        SelectComboByTag(CmbIntellectualSupport, profile.Support.IntellectualSupport);
        SelectComboByTag(CmbVisualFunction, profile.Support.VisualFunction);
        SelectComboByTag(CmbAuditoryFunction, profile.Support.AuditoryFunction);
        SelectComboByTag(CmbMotorFunction, profile.Support.MotorFunction);
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
        if (ChkDisabilityAutism.IsChecked == true) profile.Disabilities.Add(DisabilityType.Autism);
        if (ChkDisabilityAdhd.IsChecked == true) profile.Disabilities.Add(DisabilityType.ADHD);

        profile.Disability = profile.Disabilities.Count > 0 ? profile.Disabilities[0] : DisabilityType.None;
        UpdateSupportPanelsVisibility();
        profile.ApplyRecommendedAccessibilityDefaults();

        _isInitializing = true;
        PopulateAcademicLevelsForAge(profile.Age);
        var allowed = AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disabilities);
        if (!allowed.Contains(profile.CurrentLevel))
        {
            profile.CurrentLevel = AdaptiveEngine.GetRecommendedLevelForAge(profile.Age);
        }
        SelectComboByTag(CmbAcademicLevel, profile.CurrentLevel);
        _isInitializing = false;

        SyncAccessibilityControlsFromProfile();
        _acc.ApplyProfile(profile);

        string summary = profile.Disabilities.Count > 0
            ? string.Join(", ", profile.Disabilities.Select(d => d.ToString()))
            : "Sin discapacidad";
        _acc.Announce($"Condiciones actualizadas: {summary}. Opciones de accesibilidad adaptadas simultáneamente.");
    }

    private void SupportDescription_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;

        var profile = _acc.CurrentProfile;
        profile.Support.AutismSupport = GetSelectedSupport(CmbAutismSupport, AutismSupportLevel.NotApplicable);
        profile.Support.AdhdPresentation = GetSelectedSupport(CmbAdhdPresentation, AdhdPresentation.NotApplicable);
        profile.Support.IntellectualSupport = GetSelectedSupport(CmbIntellectualSupport, IntellectualSupportLevel.NotApplicable);
        profile.Support.VisualFunction = GetSelectedSupport(CmbVisualFunction, VisualFunctionalLevel.NotApplicable);
        profile.Support.AuditoryFunction = GetSelectedSupport(CmbAuditoryFunction, AuditoryFunctionalLevel.NotApplicable);
        profile.Support.MotorFunction = GetSelectedSupport(CmbMotorFunction, MotorFunctionalLevel.NotApplicable);

        profile.ApplyRecommendedAccessibilityDefaults();
        SyncAccessibilityControlsFromProfile();
        _acc.ApplyProfile(profile);
        _supportSelectionDirty = true;
    }

    private void SupportCombo_DropDownClosed(object sender, object e)
    {
        if (_isInitializing || !_supportSelectionDirty)
            return;

        _supportSelectionDirty = false;
        string selection = (sender as ComboBox)?.SelectedItem is ComboBoxItem item
            ? item.Content?.ToString() ?? string.Empty
            : string.Empty;
        if (!string.IsNullOrWhiteSpace(selection))
        {
            _acc.Announce(selection);
        }
    }

    private void CmbAge_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbAge.SelectedItem is ComboBoxItem item && item.Tag is int age)
        {
            _acc.CurrentProfile.Age = age;
            var suggestedLevel = AdaptiveEngine.GetRecommendedLevelForAge(age);

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
            _acc.CurrentProfile.Preferences.MarkManual(AccessibilityPreferences.ThemePreference);
            _acc.ApplyTheme(theme);
        }
    }

    private void CmbFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        if (CmbFontSize.SelectedItem is ComboBoxItem item && item.Tag is FontSizeLevel font)
        {
            _acc.CurrentProfile.Preferences.MarkManual(AccessibilityPreferences.FontSizePreference);
            _acc.ApplyFontSize(font);
        }
    }

    private void ChkInternalAudio_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        bool enabled = ChkInternalAudio.IsChecked == true;
        _acc.CurrentProfile.Preferences.EnableVoiceSynthesis = enabled;
        _acc.CurrentProfile.Preferences.MarkManual(AccessibilityPreferences.VoiceSynthesisPreference);
        _acc.IsInternalAudioEnabled = enabled;
        _acc.Audio.Enabled = enabled;
        _acc.Announce(enabled ? "Audio interno habilitado." : "Audio interno deshabilitado.");
    }

    private void ChkScreenReaderAnnouncements_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        bool enabled = ChkScreenReaderAnnouncements.IsChecked == true;
        _acc.CurrentProfile.Preferences.ScreenReaderAnnouncements = enabled;
        _acc.CurrentProfile.Preferences.MarkManual(AccessibilityPreferences.ScreenReaderPreference);
        _acc.IsScreenReaderIntegrationEnabled = enabled;
        _acc.Announce(enabled ? "Anuncios para el lector de pantalla habilitados." : "Anuncios para el lector de pantalla deshabilitados.");
    }

    private void ChkAudioCues_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        bool enabled = ChkAudioCues.IsChecked == true;
        _acc.CurrentProfile.Preferences.PlayAudioCues = enabled;
        _acc.CurrentProfile.Preferences.MarkManual(AccessibilityPreferences.AudioCuesPreference);
        _acc.Audio.MusicEnabled = enabled;
        if (enabled && _acc.Audio.Enabled)
        {
            _acc.Audio.PlaySuccessChord();
        }
    }

    private void ChkSimplifyLanguage_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        bool enabled = ChkSimplifyLanguage.IsChecked == true;
        _acc.CurrentProfile.Preferences.SimplifyLanguage = enabled;
        _acc.CurrentProfile.Preferences.ReduceOptionsCount = enabled;
        _acc.CurrentProfile.Preferences.MarkManual(AccessibilityPreferences.SimplifyLanguagePreference);
        _acc.CurrentProfile.Preferences.MarkManual(AccessibilityPreferences.ReduceOptionsPreference);
        _acc.Announce(enabled ? "Lenguaje simplificado activado" : "Lenguaje estándar activado");
    }

    private void Control_GotFocus(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
    }

    private async void BtnExportProfile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _acc.CurrentProfile.Name = string.IsNullOrWhiteSpace(TxtName.Text)
                ? "Aventurero"
                : TxtName.Text.Trim();

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = $"mathtutor_perfil_{DateTime.Now:yyyyMMdd}"
            };
            picker.FileTypeChoices.Add("Perfil MathTutor (JSON)", new List<string> { ".json" });

            if (App.MainAppWindow is not null)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainAppWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            }

            var file = await picker.PickSaveFileAsync();
            if (file is null)
            {
                _acc.Announce("Exportación cancelada.");
                return;
            }

            _acc.Storage.Export(_acc.CurrentProfile, file.Path);
            TxtPrivacyStatus.Text = $"Perfil exportado en: {file.Path}";
            _acc.Audio.PlaySuccessChord();
            _acc.Announce($"Perfil exportado correctamente en {file.Name}.", speak: true);
        }
        catch (Exception)
        {
            TxtPrivacyStatus.Text = "No se pudo exportar el perfil. Intenta guardarlo en Documentos.";
            _acc.AnnounceAssertive("No se pudo exportar el perfil. Intenta guardarlo en la carpeta Documentos.");
        }
    }

    private async void BtnDeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "¿Borrar el perfil de este equipo?",
            Content = "Se borrarán el nombre, la edad, los apoyos y el progreso guardados localmente. " +
                      "Las copias exportadas no se borran. Esta acción no se puede deshacer.",
            PrimaryButtonText = "Sí, borrar perfil",
            CloseButtonText = "Cancelar",
            DefaultButton = ContentDialogButton.Close
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            _acc.Announce("Borrado del perfil cancelado.");
            return;
        }

        bool deleted = _acc.Storage.Delete();
        _snapshot = null;
        _acc.ApplyProfile(new UserProfile());

        _isInitializing = true;
        PopulateDropdowns();
        LoadCurrentProfileData();
        _isInitializing = false;

        BtnCancel.Visibility = Visibility.Collapsed;
        TxtPrivacyStatus.Text = deleted
            ? "Perfil borrado de este equipo. Puedes crear uno nuevo cuando quieras."
            : "No había un perfil guardado; se reinició la caracterización.";
        _acc.Audio.PlaySuccessChord();
        _acc.Announce(TxtPrivacyStatus.Text, speak: true);
    }

    private async void BtnPrivacyNotice_Click(object sender, RoutedEventArgs e)
    {
        var content = new ScrollViewer
        {
            Content = new TextBlock
            {
                Text = PrivacyNoticeText,
                TextWrapping = TextWrapping.Wrap
            },
            MaxHeight = 420,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Aviso de privacidad para familias",
            Content = content,
            CloseButtonText = "Entendido",
            DefaultButton = ContentDialogButton.Close
        };

        await dialog.ShowAsync();
    }

    private void BtnSaveAndStart_Click(object sender, RoutedEventArgs e)
    {
        _acc.CurrentProfile.Name = string.IsNullOrWhiteSpace(TxtName.Text) ? "Aventurero" : TxtName.Text.Trim();
        _acc.SaveCurrentProfile();
        _acc.Audio.PlaySuccessChord();
        _acc.Announce($"¡Perfil guardado, {_acc.CurrentProfile.Name}! Entrando al Reino Matemágico...", speak: true);
        App.Navigation.CompleteProfile();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (_snapshot is not null)
        {
            _acc.ApplyProfile(_snapshot);
            _acc.SaveCurrentProfile();
        }

        _acc.Audio.PlayNavigationClick();
        App.Navigation.CancelProfile();
    }
}
