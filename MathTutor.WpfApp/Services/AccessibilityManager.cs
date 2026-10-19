using System;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Media;
using MathTutor.Core.Models;
using MathTutor.Core.Services;

namespace MathTutor.WpfApp.Services;

/// <summary>
/// Administrador central de accesibilidad y cumplimiento WCAG 2.2 para la aplicación WPF.
/// Maneja paletas de alto contraste, escala dinámica de fuentes, síntesis de voz y anuncios accesibles.
/// </summary>
public class AccessibilityManager
{
    private static AccessibilityManager? _instance;
    public static AccessibilityManager Instance => _instance ??= new AccessibilityManager();

    public UserProfile CurrentProfile { get; set; } = new();
    public SpeechService Speech { get; } = new();
    public AccessibleAudioCueService Audio { get; } = new();
    public ProfileStorageService Storage { get; } = new();

    /// <summary>
    /// Canal A (Controlado por F2): Audio y efectos acústicos internos del juego.
    /// </summary>
    public bool IsInternalAudioEnabled { get; set; } = true;

    /// <summary>
    /// Canal B (Siempre activo por defecto): Comunicación nativa hacia lectores de pantalla externos (NVDA, JAWS, Narrador).
    /// </summary>
    public bool IsScreenReaderIntegrationEnabled
    {
        get => Speech.IsScreenReaderIntegrationEnabled;
        set => Speech.IsScreenReaderIntegrationEnabled = value;
    }

    public event Action? ThemeChanged;
    public event Action? FontSizeChanged;
    public event Action<string>? AnnouncementMade;

    private AccessibilityManager()
    {
    }

    /// <summary>
    /// Aplica el perfil y sincroniza todos los subsistemas de accesibilidad.
    /// </summary>
    public void ApplyProfile(UserProfile profile)
    {
        CurrentProfile = profile;

        // Canal B: Siempre activo para garantizar que NVDA, JAWS y Narrador nunca queden mudos
        IsScreenReaderIntegrationEnabled = true;

        // Canal A: Audio interno del juego (controlado por F2 / preferencias)
        IsInternalAudioEnabled = profile.Preferences.PlayAudioCues && profile.Preferences.EnableVoiceSynthesis;
        Audio.Enabled = IsInternalAudioEnabled;

        Speech.SetRate(profile.Preferences.SpeechRate);
        Speech.SetVolume(profile.Preferences.SpeechVolume);

        ApplyTheme(profile.Preferences.Theme);
        ApplyFontSize(profile.Preferences.FontSize);
    }

    /// <summary>
    /// Guarda el perfil actual y sus preferencias en disco.
    /// </summary>
    public void SaveCurrentProfile()
    {
        Storage.Save(CurrentProfile);
    }

    /// <summary>
    /// Intenta cargar un perfil previamente guardado. Retorna true si existía y se cargó exitosamente.
    /// </summary>
    public bool LoadSavedProfile()
    {
        var saved = Storage.Load();
        if (saved != null)
        {
            ApplyProfile(saved);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Envía un anuncio separando Canal B (lector de pantalla del sistema, siempre activo)
    /// y Canal A (efectos de audio internos del juego, controlados por F2).
    /// </summary>
    public void Announce(string message, bool speak = true, bool playTone = false)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        AnnouncementMade?.Invoke(message);

        // Canal A: Solo reproduce pistas sonoras internas si F2 no las ha desactivado
        if (playTone && IsInternalAudioEnabled && Audio.Enabled)
        {
            Audio.PlayHintTone();
        }

        // Canal B: SIEMPRE envía al lector de pantalla externo (NVDA / JAWS / Narrador / UIA)
        if (speak && IsScreenReaderIntegrationEnabled)
        {
            Speech.Speak(message, interruptPrevious: true, immediateLiveRegion: false);
        }
    }

    /// <summary>
    /// Emite un anuncio asertivo prioritario e inmediato hacia el lector de pantalla (Tarea 4).
    /// </summary>
    public void AnnounceAssertive(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        AnnouncementMade?.Invoke(message);

        if (IsScreenReaderIntegrationEnabled)
        {
            Speech.SpeakAssertive(message);
        }
    }

    /// <summary>
    /// Cambia cíclicamente de esquema de contraste con la tecla rápida F3.
    /// </summary>
    public void CycleTheme()
    {
        var nextTheme = CurrentProfile.Preferences.Theme switch
        {
            HighContrastTheme.DefaultPlayful => HighContrastTheme.HighContrastDark,
            HighContrastTheme.HighContrastDark => HighContrastTheme.HighContrastLight,
            HighContrastTheme.HighContrastLight => HighContrastTheme.LowSensory,
            HighContrastTheme.LowSensory => HighContrastTheme.DefaultPlayful,
            _ => HighContrastTheme.DefaultPlayful
        };

        ApplyTheme(nextTheme);
        AnnounceAssertive($"Esquema de contraste cambiado a: {GetThemeDisplayName(nextTheme)}");
    }

    /// <summary>
    /// Cambia cíclicamente el tamaño de fuente con la tecla rápida F4.
    /// </summary>
    public void CycleFontSize()
    {
        var nextSize = CurrentProfile.Preferences.FontSize switch
        {
            FontSizeLevel.Normal => FontSizeLevel.Large,
            FontSizeLevel.Large => FontSizeLevel.ExtraLarge,
            FontSizeLevel.ExtraLarge => FontSizeLevel.Huge,
            FontSizeLevel.Huge => FontSizeLevel.Normal,
            _ => FontSizeLevel.Normal
        };

        ApplyFontSize(nextSize);
        AnnounceAssertive($"Tamaño de texto ajustado a: {GetFontSizeDisplayName(nextSize)}");
    }

    /// <summary>
    /// Conmuta únicamente el audio interno del juego (Canal A - Tecla F2).
    /// El canal del lector de pantalla del sistema (Canal B) permanece siempre activo (Tarea 5).
    /// </summary>
    public void ToggleSpeech()
    {
        IsInternalAudioEnabled = !IsInternalAudioEnabled;
        CurrentProfile.Preferences.EnableVoiceSynthesis = IsInternalAudioEnabled;
        CurrentProfile.Preferences.PlayAudioCues = IsInternalAudioEnabled;
        Audio.Enabled = IsInternalAudioEnabled;

        string status = IsInternalAudioEnabled ? "activado" : "desactivado";
        AnnounceAssertive($"Audio interno del juego {status}. El lector de pantalla permanece activo.");
    }

    public void ApplyTheme(HighContrastTheme theme)
    {
        CurrentProfile.Preferences.Theme = theme;
        var app = Application.Current;
        if (app == null) return;

        switch (theme)
        {
            case HighContrastTheme.HighContrastDark:
                // Negro absoluto con amarillo brillante y blanco (WCAG AAA > 15:1)
                SetResource("AppBackgroundBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppSurfaceBrush", new SolidColorBrush(Color.FromRgb(18, 18, 18)));
                SetResource("AppCardBrush", new SolidColorBrush(Color.FromRgb(28, 28, 28)));
                SetResource("AppTextPrimaryBrush", new SolidColorBrush(Color.FromRgb(255, 255, 0))); // Amarillo brillante
                SetResource("AppTextSecondaryBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255))); // Blanco
                SetResource("AppAccentBrush", new SolidColorBrush(Color.FromRgb(255, 215, 0)));
                SetResource("AppFocusBorderBrush", new SolidColorBrush(Color.FromRgb(0, 255, 255))); // Cyan brillante
                SetResource("AppBorderBrush", new SolidColorBrush(Color.FromRgb(255, 255, 0)));
                SetResource("AppSuccessBrush", new SolidColorBrush(Color.FromRgb(50, 255, 50)));
                SetResource("AppOnAccentBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppOnSuccessBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppErrorBrush", new SolidColorBrush(Color.FromRgb(255, 90, 90)));
                break;

            case HighContrastTheme.HighContrastLight:
                // Blanco puro con negro absoluto (WCAG AAA > 21:1)
                SetResource("AppBackgroundBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                SetResource("AppSurfaceBrush", new SolidColorBrush(Color.FromRgb(245, 245, 245)));
                SetResource("AppCardBrush", new SolidColorBrush(Color.FromRgb(235, 235, 235)));
                SetResource("AppTextPrimaryBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppTextSecondaryBrush", new SolidColorBrush(Color.FromRgb(20, 20, 20)));
                SetResource("AppAccentBrush", new SolidColorBrush(Color.FromRgb(0, 50, 200)));
                SetResource("AppFocusBorderBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppBorderBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppSuccessBrush", new SolidColorBrush(Color.FromRgb(0, 140, 0)));
                SetResource("AppOnAccentBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                SetResource("AppOnSuccessBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                SetResource("AppErrorBrush", new SolidColorBrush(Color.FromRgb(180, 0, 0)));
                break;

            case HighContrastTheme.LowSensory:
                // Tonos suaves y armónicos para TEA y TDAH (evita fatiga visual)
                SetResource("AppBackgroundBrush", new SolidColorBrush(Color.FromRgb(240, 244, 248)));
                SetResource("AppSurfaceBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                SetResource("AppCardBrush", new SolidColorBrush(Color.FromRgb(230, 238, 245)));
                SetResource("AppTextPrimaryBrush", new SolidColorBrush(Color.FromRgb(28, 43, 62)));
                SetResource("AppTextSecondaryBrush", new SolidColorBrush(Color.FromRgb(55, 75, 100)));
                SetResource("AppAccentBrush", new SolidColorBrush(Color.FromRgb(70, 130, 180)));
                SetResource("AppFocusBorderBrush", new SolidColorBrush(Color.FromRgb(255, 140, 0)));
                SetResource("AppBorderBrush", new SolidColorBrush(Color.FromRgb(180, 200, 220)));
                SetResource("AppSuccessBrush", new SolidColorBrush(Color.FromRgb(46, 139, 87)));
                SetResource("AppOnAccentBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppOnSuccessBrush", new SolidColorBrush(Color.FromRgb(0, 0, 0)));
                SetResource("AppErrorBrush", new SolidColorBrush(Color.FromRgb(150, 0, 0)));
                break;

            case HighContrastTheme.DefaultPlayful:
            default:
                // Diseño alegre, infantil pero con alto contraste verificado
                SetResource("AppBackgroundBrush", new SolidColorBrush(Color.FromRgb(248, 249, 255)));
                SetResource("AppSurfaceBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                SetResource("AppCardBrush", new SolidColorBrush(Color.FromRgb(240, 243, 250)));
                SetResource("AppTextPrimaryBrush", new SolidColorBrush(Color.FromRgb(20, 25, 45)));
                SetResource("AppTextSecondaryBrush", new SolidColorBrush(Color.FromRgb(60, 70, 95)));
                SetResource("AppAccentBrush", new SolidColorBrush(Color.FromRgb(90, 60, 220)));
                SetResource("AppFocusBorderBrush", new SolidColorBrush(Color.FromRgb(255, 120, 0)));
                SetResource("AppBorderBrush", new SolidColorBrush(Color.FromRgb(210, 220, 240)));
                SetResource("AppSuccessBrush", new SolidColorBrush(Color.FromRgb(15, 140, 70)));
                SetResource("AppOnAccentBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                SetResource("AppOnSuccessBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                SetResource("AppErrorBrush", new SolidColorBrush(Color.FromRgb(176, 0, 32)));
                break;
        }

        ThemeChanged?.Invoke();
    }

    public void ApplyFontSize(FontSizeLevel level)
    {
        CurrentProfile.Preferences.FontSize = level;
        var app = Application.Current;
        if (app == null) return;

        double baseSize = level switch
        {
            FontSizeLevel.Normal => 18.0,
            FontSizeLevel.Large => 23.0,
            FontSizeLevel.ExtraLarge => 28.0,
            FontSizeLevel.Huge => 34.0,
            _ => 18.0
        };

        SetResource("AppBaseFontSize", baseSize);
        SetResource("AppSubHeaderFontSize", baseSize * 1.25);
        SetResource("AppHeaderFontSize", baseSize * 1.55);
        SetResource("AppTitleFontSize", baseSize * 1.95);

        FontSizeChanged?.Invoke();
    }

    private static void SetResource(string key, object value)
    {
        if (Application.Current != null)
        {
            Application.Current.Resources[key] = value;
        }
    }

    private static string GetThemeDisplayName(HighContrastTheme theme) => theme switch
    {
        HighContrastTheme.DefaultPlayful => "Colorido Lúdico",
        HighContrastTheme.HighContrastDark => "Alto Contraste Negro y Amarillo",
        HighContrastTheme.HighContrastLight => "Alto Contraste Blanco y Negro",
        HighContrastTheme.LowSensory => "Baja Estimulación Sensorial",
        _ => "Estándar"
    };

    private static string GetFontSizeDisplayName(FontSizeLevel level) => level switch
    {
        FontSizeLevel.Normal => "Estándar",
        FontSizeLevel.Large => "Grande",
        FontSizeLevel.ExtraLarge => "Muy Grande",
        FontSizeLevel.Huge => "Gigante",
        _ => "Estándar"
    };
}
