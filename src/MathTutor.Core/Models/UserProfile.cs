using System;
using System.Collections.Generic;

namespace MathTutor.Core.Models;

/// <summary>
/// Preferencias de accesibilidad configurables que garantizan el cumplimiento de WCAG 2.2
/// y la adaptación total al usuario según su condición.
/// </summary>
public class AccessibilityPreferences
{
    public const string ThemePreference = nameof(Theme);
    public const string FontSizePreference = nameof(FontSize);
    public const string VoiceSynthesisPreference = nameof(EnableVoiceSynthesis);
    public const string ScreenReaderPreference = nameof(ScreenReaderAnnouncements);
    public const string AudioCuesPreference = nameof(PlayAudioCues);
    public const string SimplifyLanguagePreference = nameof(SimplifyLanguage);
    public const string ReduceOptionsPreference = nameof(ReduceOptionsCount);
    public const string KeyboardNavigationPreference = nameof(SimplifiedKeyboardNavigation);
    public const string UnlimitedTimePreference = nameof(UnlimitedTime);
    public const string SpeechRatePreference = nameof(SpeechRate);

    /// <summary>Estado expandido o contraído de cada grupo de trofeos.</summary>
    public Dictionary<string, bool> BadgeGroupExpansion { get; set; } = new();

    /// <summary>
    /// Preferencias que el usuario ajustó manualmente. Las sugerencias automáticas
    /// nunca sobrescriben estos valores.
    /// </summary>
    public List<string> ManualPreferenceKeys { get; set; } = new();

    public bool IsManual(string preferenceKey) =>
        ManualPreferenceKeys.Contains(preferenceKey, StringComparer.Ordinal);

    public void MarkManual(string preferenceKey)
    {
        if (!IsManual(preferenceKey))
        {
            ManualPreferenceKeys.Add(preferenceKey);
        }
    }

    /// <summary>
    /// Habilita la síntesis de voz propia (TTS) que lee en voz alta cada instrucción, botón y evento.
    /// </summary>
    public bool EnableVoiceSynthesis { get; set; } = true;

    /// <summary>
    /// Habilita los anuncios compatibles con lectores de pantalla externos (NVDA, JAWS, Narrador)
    /// mediante UI Automation / LiveSetting.
    /// </summary>
    public bool ScreenReaderAnnouncements { get; set; } = true;

    /// <summary>
    /// Esquema visual de contraste actual.
    /// </summary>
    public HighContrastTheme Theme { get; set; } = HighContrastTheme.DefaultPlayful;

    /// <summary>
    /// Tamaño de fuente para todos los elementos de la interfaz.
    /// </summary>
    public FontSizeLevel FontSize { get; set; } = FontSizeLevel.Normal;

    /// <summary>
    /// Velocidad de la síntesis de voz (-10 a 10). -1 es ideal para niños pequeños.
    /// </summary>
    public int SpeechRate { get; set; } = -1;

    /// <summary>
    /// Volumen de la síntesis de voz (0 a 100).
    /// </summary>
    public int SpeechVolume { get; set; } = 100;

    /// <summary>
    /// Habilita pistas sonoras armónicas (notas musicales de conteo, campanas de acierto).
    /// </summary>
    public bool PlayAudioCues { get; set; } = true;

    /// <summary>
    /// Simplificación del lenguaje: oraciones cortas, metáforas directas, vocabulario concreto.
    /// </summary>
    public bool SimplifyLanguage { get; set; } = false;

    /// <summary>
    /// Reduce la cantidad de opciones en las preguntas (por ejemplo, 2 opciones en vez de 4)
    /// para evitar la sobrecarga cognitiva en discapacidad intelectual.
    /// </summary>
    public bool ReduceOptionsCount { get; set; } = false;

    /// <summary>
    /// Modo de entrada por teclado simplificado (selección directa con números 1, 2, 3 o Espacio/Enter).
    /// </summary>
    public bool SimplifiedKeyboardNavigation { get; set; } = true;

    /// <summary>
    /// Sin límite de tiempo para responder (WCAG Pauta 2.2 Tiempo Suficiente).
    /// </summary>
    public bool UnlimitedTime { get; set; } = true;
}

/// <summary>
/// Apoyos y descripciones funcionales específicas de cada condición.
/// No existe un grado universal de severidad.
/// </summary>
public class SupportProfile
{
    public AutismSupportLevel AutismSupport { get; set; } = AutismSupportLevel.NotApplicable;
    public AdhdPresentation AdhdPresentation { get; set; } = AdhdPresentation.NotApplicable;
    public IntellectualSupportLevel IntellectualSupport { get; set; } = IntellectualSupportLevel.NotApplicable;
    public VisualFunctionalLevel VisualFunction { get; set; } = VisualFunctionalLevel.NotApplicable;
    public AuditoryFunctionalLevel AuditoryFunction { get; set; } = AuditoryFunctionalLevel.NotApplicable;
    public MotorFunctionalLevel MotorFunction { get; set; } = MotorFunctionalLevel.NotApplicable;
}

/// <summary>
/// Perfil pedagógico y de accesibilidad del usuario / alumno.
/// </summary>
public class UserProfile
{
    public const int CurrentProfileVersion = 2;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Pequeño Matemático";
    public int Age { get; set; } = 5;

    /// <summary>
    /// Versión del esquema del perfil. Permite migrar perfiles antiguos sin
    /// convertir su severidad genérica en un nivel de apoyo autista.
    /// </summary>
    public int ProfileVersion { get; set; } = CurrentProfileVersion;

    private DisabilityType _disability = DisabilityType.None;

    /// <summary>
    /// Tipo de discapacidad primaria (mantiene retrocompatibilidad con código existente).
    /// Se sincroniza automáticamente con la lista de discapacidades múltiples.
    /// </summary>
    public DisabilityType Disability
    {
        get => _disability;
        set
        {
            _disability = value;
            if (value != DisabilityType.None)
            {
                if (!Disabilities.Contains(value))
                {
                    Disabilities.Add(value);
                }
            }
            else if (Disabilities.Count <= 1)
            {
                Disabilities.Clear();
            }
        }
    }

    private List<DisabilityType> _disabilities = new();

    /// <summary>
    /// Lista de discapacidades o condiciones seleccionadas para soporte de comorbilidad e interseccionalidad.
    /// </summary>
    public List<DisabilityType> Disabilities
    {
        get => _disabilities;
        set => _disabilities = value != null ? value.Distinct().ToList() : new List<DisabilityType>();
    }

    /// <summary>
    /// Determina si el alumno presenta una discapacidad o condición específica.
    /// </summary>
    public bool HasDisability(DisabilityType type)
    {
        if (type == DisabilityType.None)
        {
            return Disabilities.Count == 0 || (Disabilities.Count == 1 && Disabilities[0] == DisabilityType.None);
        }
        return Disabilities.Contains(type) || Disability == type;
    }

    private SupportProfile _support = new();

    /// <summary>
    /// Apoyos específicos por condición (autismo, TDAH, intelectual, visual,
    /// auditiva y motriz). Reemplaza el antiguo grado universal de severidad.
    /// </summary>
    public SupportProfile Support
    {
        get => _support;
        set => _support = value ?? new SupportProfile();
    }

    public AcademicLevel CurrentLevel { get; set; } = AcademicLevel.Kindergarten;
    public RegionalContext Region { get; set; } = RegionalContext.Mexico;

    public string CurrencyName => Region switch
    {
        RegionalContext.Peru => "soles",
        _ => "pesos"
    };

    public string CurrencyFractionName => Region switch
    {
        RegionalContext.Peru => "céntimos",
        _ => "centavos"
    };

    public string CurrencySymbol => Region switch
    {
        RegionalContext.Peru => "S/",
        _ => "$"
    };

    public string StoreName => Region switch
    {
        RegionalContext.Colombia => "tienda de barrio",
        RegionalContext.Argentina => "kiosco",
        RegionalContext.Peru => "bodega",
        RegionalContext.Chile => "almacén",
        _ => "tiendita"
    };

    public AccessibilityPreferences Preferences { get; set; } = new();

    // Gamificación y logros acumulados
    public int StarsTotal { get; set; } = 0;
    public int PointsTotal { get; set; } = 0;
    public int CurrentStreak { get; set; } = 0;
    public List<string> CompletedLessonIds { get; set; } = new();
    public List<string> CompletedExerciseIds { get; set; } = new();

    /// <summary>
    /// Sugiere apoyos de accesibilidad según las condiciones y los apoyos
    /// declarados. Nunca sobrescribe una preferencia que el usuario ajustó a mano.
    /// </summary>
    public void ApplyRecommendedAccessibilityDefaults()
    {
        var activeDisabilities = Disabilities.Where(d => d != DisabilityType.None).Distinct().ToList();
        if (activeDisabilities.Count == 0 && Disability != DisabilityType.None)
        {
            activeDisabilities.Add(Disability);
        }

        bool hasVisual = activeDisabilities.Contains(DisabilityType.Visual);
        bool hasHearing = activeDisabilities.Contains(DisabilityType.Hearing);

        if (activeDisabilities.Count == 0)
        {
            Suggest(Preferences, AccessibilityPreferences.ThemePreference,
                () => Preferences.Theme = HighContrastTheme.DefaultPlayful);
            Suggest(Preferences, AccessibilityPreferences.SimplifyLanguagePreference,
                () => Preferences.SimplifyLanguage = false);
            Suggest(Preferences, AccessibilityPreferences.ReduceOptionsPreference,
                () => Preferences.ReduceOptionsCount = false);
            Suggest(Preferences, AccessibilityPreferences.SpeechRatePreference,
                () => Preferences.SpeechRate = 0);

            Suggest(Preferences, AccessibilityPreferences.VoiceSynthesisPreference,
                () => Preferences.EnableVoiceSynthesis = Age <= 6);
            Suggest(Preferences, AccessibilityPreferences.ScreenReaderPreference,
                () => Preferences.ScreenReaderAnnouncements = true);
            Suggest(Preferences, AccessibilityPreferences.AudioCuesPreference,
                () => Preferences.PlayAudioCues = Age <= 6);
            Suggest(Preferences, AccessibilityPreferences.FontSizePreference,
                () => Preferences.FontSize = Age <= 6 ? FontSizeLevel.Large : FontSizeLevel.Normal);
            return;
        }

        foreach (var dis in activeDisabilities)
        {
            switch (dis)
            {
                case DisabilityType.Visual:
                    Suggest(Preferences, AccessibilityPreferences.VoiceSynthesisPreference,
                        () => Preferences.EnableVoiceSynthesis = true);
                    Suggest(Preferences, AccessibilityPreferences.ScreenReaderPreference,
                        () => Preferences.ScreenReaderAnnouncements = true);
                    Suggest(Preferences, AccessibilityPreferences.AudioCuesPreference,
                        () => Preferences.PlayAudioCues = true);

                    bool blindness = Support.VisualFunction == VisualFunctionalLevel.Blindness;
                    Suggest(Preferences, AccessibilityPreferences.ThemePreference,
                        () => Preferences.Theme = blindness
                            ? HighContrastTheme.HighContrastDark
                            : HighContrastTheme.LowSensory);
                    Suggest(Preferences, AccessibilityPreferences.FontSizePreference,
                        () => Preferences.FontSize = blindness || Support.VisualFunction == VisualFunctionalLevel.LowVision
                            ? FontSizeLevel.ExtraLarge
                            : FontSizeLevel.Large);
                    break;

                case DisabilityType.Hearing:
                    if (!hasVisual)
                    {
                        Suggest(Preferences, AccessibilityPreferences.VoiceSynthesisPreference,
                            () => Preferences.EnableVoiceSynthesis = false);
                        Suggest(Preferences, AccessibilityPreferences.ScreenReaderPreference,
                            () => Preferences.ScreenReaderAnnouncements = false);
                        Suggest(Preferences, AccessibilityPreferences.AudioCuesPreference,
                            () => Preferences.PlayAudioCues = false);
                    }
                    Suggest(Preferences, AccessibilityPreferences.FontSizePreference,
                        () => Preferences.FontSize = FontSizeLevel.Large);
                    break;

                case DisabilityType.Motor:
                    Suggest(Preferences, AccessibilityPreferences.KeyboardNavigationPreference,
                        () => Preferences.SimplifiedKeyboardNavigation = true);
                    Suggest(Preferences, AccessibilityPreferences.FontSizePreference,
                        () => Preferences.FontSize = FontSizeLevel.Large);
                    Suggest(Preferences, AccessibilityPreferences.UnlimitedTimePreference,
                        () => Preferences.UnlimitedTime = true);
                    break;

                case DisabilityType.Intellectual:
                    int intellectualSupport = Support.IntellectualSupport == IntellectualSupportLevel.NotApplicable
                        ? (int)IntellectualSupportLevel.Intermittent
                        : (int)Support.IntellectualSupport;
                    Suggest(Preferences, AccessibilityPreferences.SimplifyLanguagePreference,
                        () => Preferences.SimplifyLanguage = true);
                    Suggest(Preferences, AccessibilityPreferences.ReduceOptionsPreference,
                        () => Preferences.ReduceOptionsCount = true);
                    Suggest(Preferences, AccessibilityPreferences.SpeechRatePreference,
                        () => Preferences.SpeechRate = intellectualSupport >= (int)IntellectualSupportLevel.Limited ? -2 : -1);
                    Suggest(Preferences, AccessibilityPreferences.VoiceSynthesisPreference,
                        () => Preferences.EnableVoiceSynthesis = true);
                    Suggest(Preferences, AccessibilityPreferences.AudioCuesPreference,
                        () => Preferences.PlayAudioCues = true);
                    if (intellectualSupport >= (int)IntellectualSupportLevel.Limited)
                    {
                        Suggest(Preferences, AccessibilityPreferences.FontSizePreference,
                            () => Preferences.FontSize = FontSizeLevel.Large);
                    }
                    Suggest(Preferences, AccessibilityPreferences.UnlimitedTimePreference,
                        () => Preferences.UnlimitedTime = true);
                    break;

                case DisabilityType.Autism:
                    if (!hasVisual)
                    {
                        Suggest(Preferences, AccessibilityPreferences.ThemePreference,
                            () => Preferences.Theme = HighContrastTheme.LowSensory);
                    }
                    Suggest(Preferences, AccessibilityPreferences.UnlimitedTimePreference,
                        () => Preferences.UnlimitedTime = true);
                    Suggest(Preferences, AccessibilityPreferences.AudioCuesPreference,
                        () => Preferences.PlayAudioCues = true);
                    Suggest(Preferences, AccessibilityPreferences.SpeechRatePreference,
                        () => Preferences.SpeechRate = -1);
                    if (Support.AutismSupport >= AutismSupportLevel.Level2)
                    {
                        Suggest(Preferences, AccessibilityPreferences.ReduceOptionsPreference,
                            () => Preferences.ReduceOptionsCount = true);
                    }
                    break;

                case DisabilityType.ADHD:
                    if (!hasVisual && (Support.AdhdPresentation is AdhdPresentation.PredominantlyHyperactiveImpulsive
                        or AdhdPresentation.Combined))
                    {
                        Suggest(Preferences, AccessibilityPreferences.ThemePreference,
                            () => Preferences.Theme = HighContrastTheme.LowSensory);
                    }
                    Suggest(Preferences, AccessibilityPreferences.UnlimitedTimePreference,
                        () => Preferences.UnlimitedTime = true);
                    Suggest(Preferences, AccessibilityPreferences.AudioCuesPreference,
                        () => Preferences.PlayAudioCues = true);
                    Suggest(Preferences, AccessibilityPreferences.SpeechRatePreference,
                        () => Preferences.SpeechRate = -1);
                    Suggest(Preferences, AccessibilityPreferences.ReduceOptionsPreference,
                        () => Preferences.ReduceOptionsCount = true);
                    break;

                case DisabilityType.Neurodivergent:
                    // Perfil heredado que no pasó por migración.
                    if (!hasVisual)
                    {
                        Suggest(Preferences, AccessibilityPreferences.ThemePreference,
                            () => Preferences.Theme = HighContrastTheme.LowSensory);
                    }
                    Suggest(Preferences, AccessibilityPreferences.UnlimitedTimePreference,
                        () => Preferences.UnlimitedTime = true);
                    Suggest(Preferences, AccessibilityPreferences.AudioCuesPreference,
                        () => Preferences.PlayAudioCues = true);
                    Suggest(Preferences, AccessibilityPreferences.SpeechRatePreference,
                        () => Preferences.SpeechRate = -1);
                    break;
            }
        }
    }

    private static void Suggest(
        AccessibilityPreferences preferences,
        string preferenceKey,
        Action apply)
    {
        if (!preferences.IsManual(preferenceKey))
        {
            apply();
        }
    }
}
