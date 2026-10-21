using System;
using System.Collections.Generic;

namespace MathTutor.Core.Models;

/// <summary>
/// Preferencias de accesibilidad configurables que garantizan el cumplimiento de WCAG 2.2
/// y la adaptación total al usuario según su condición.
/// </summary>
public class AccessibilityPreferences
{
    /// <summary>Estado expandido o contraído de cada grupo de trofeos.</summary>
    public Dictionary<string, bool> BadgeGroupExpansion { get; set; } = new();

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
/// Perfil pedagógico y de accesibilidad del usuario / alumno.
/// </summary>
public class UserProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Pequeño Matemático";
    public int Age { get; set; } = 5;

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

    public DisabilitySeverity Severity { get; set; } = DisabilitySeverity.Mild;
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
    /// Aplica automáticamente la configuración inicial recomendada de accesibilidad
    /// de acuerdo con la caracterización de discapacidad (incluyendo selecciones múltiples / comorbilidad) y edad.
    /// </summary>
    public void ApplyRecommendedAccessibilityDefaults()
    {
        var activeDisabilities = Disabilities.Where(d => d != DisabilityType.None).Distinct().ToList();
        if (activeDisabilities.Count == 0 && Disability != DisabilityType.None)
        {
            activeDisabilities.Add(Disability);
        }

        if (activeDisabilities.Count == 0)
        {
            // Neurotípico
            Preferences.Theme = HighContrastTheme.DefaultPlayful;
            Preferences.SimplifyLanguage = false;
            Preferences.ReduceOptionsCount = false;
            Preferences.SpeechRate = 0;
            if (Age <= 6)
            {
                Preferences.EnableVoiceSynthesis = true;
                Preferences.PlayAudioCues = true;
                Preferences.FontSize = FontSizeLevel.Large;
            }
            return;
        }

        foreach (var dis in activeDisabilities)
        {
            switch (dis)
            {
                case DisabilityType.Visual:
                    Preferences.EnableVoiceSynthesis = true;
                    Preferences.ScreenReaderAnnouncements = true;
                    Preferences.PlayAudioCues = true;
                    if (Severity >= DisabilitySeverity.Moderate)
                    {
                        Preferences.Theme = HighContrastTheme.HighContrastDark;
                        Preferences.FontSize = FontSizeLevel.ExtraLarge;
                    }
                    else
                    {
                        Preferences.FontSize = FontSizeLevel.Large;
                    }
                    break;

                case DisabilityType.Hearing:
                    if (!activeDisabilities.Contains(DisabilityType.Visual))
                    {
                        Preferences.EnableVoiceSynthesis = false;
                        Preferences.ScreenReaderAnnouncements = false;
                        Preferences.PlayAudioCues = false;
                    }
                    Preferences.FontSize = FontSizeLevel.Large;
                    break;

                case DisabilityType.Motor:
                    Preferences.SimplifiedKeyboardNavigation = true;
                    Preferences.FontSize = FontSizeLevel.Large;
                    Preferences.UnlimitedTime = true;
                    break;

                case DisabilityType.Intellectual:
                    Preferences.SimplifyLanguage = true;
                    Preferences.ReduceOptionsCount = true;
                    Preferences.SpeechRate = -2;
                    Preferences.EnableVoiceSynthesis = true;
                    Preferences.PlayAudioCues = true;
                    Preferences.FontSize = FontSizeLevel.Large;
                    Preferences.UnlimitedTime = true;
                    break;

                case DisabilityType.Neurodivergent:
                    if (!activeDisabilities.Contains(DisabilityType.Visual) || Severity < DisabilitySeverity.Moderate)
                    {
                        Preferences.Theme = HighContrastTheme.LowSensory;
                    }
                    Preferences.PlayAudioCues = true;
                    Preferences.SpeechRate = -1;
                    Preferences.UnlimitedTime = true;
                    break;
            }
        }
    }
}
