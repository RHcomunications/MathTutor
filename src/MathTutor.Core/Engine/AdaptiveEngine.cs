using System;
using System.Collections.Generic;
using System.Linq;
using MathTutor.Core.Models;

namespace MathTutor.Core.Engine;

/// <summary>
/// Motor de adaptación pedagógica. Modifica dinámicamente la dificultad,
/// la complejidad del lenguaje y los estímulos según la caracterización del usuario.
/// </summary>
public class AdaptiveEngine
{
    /// <summary>
    /// Devuelve los niveles escolares permitidos para la edad y condición del alumno (Tarea 6 y Tarea 12).
    /// Evita que niños pequeños vean o seleccionen por error niveles avanzados, permitiendo a los 12 años
    /// elegir también 1º de Secundaria y habilitando niveles base de refuerzo para alumnos con discapacidad intelectual.
    /// </summary>
    public static IReadOnlyList<AcademicLevel> GetAllowedLevelsForAge(int age, DisabilityType disability = DisabilityType.None)
    {
        var baseLevels = age switch
        {
            >= 3 and <= 5 => new[]
            {
                AcademicLevel.Kindergarten
            },
            >= 6 and <= 11 => new[]
            {
                AcademicLevel.EarlyElementary,
                AcademicLevel.MiddleElementary,
                AcademicLevel.UpperElementary
            },
            12 => new[]
            {
                AcademicLevel.EarlyElementary,
                AcademicLevel.MiddleElementary,
                AcademicLevel.UpperElementary,
                AcademicLevel.SecondaryFirstGrade
            },
            >= 13 and <= 15 => new[]
            {
                AcademicLevel.SecondaryFirstGrade,
                AcademicLevel.SecondarySecondGrade,
                AcademicLevel.SecondaryThirdGrade
            },
            >= 16 => new[]
            {
                AcademicLevel.Kindergarten,
                AcademicLevel.EarlyElementary,
                AcademicLevel.MiddleElementary,
                AcademicLevel.UpperElementary,
                AcademicLevel.SecondaryFirstGrade,
                AcademicLevel.SecondarySecondGrade,
                AcademicLevel.SecondaryThirdGrade,
                AcademicLevel.HighSchoolFirstGrade,
                AcademicLevel.HighSchoolSecondGrade,
                AcademicLevel.HighSchoolThirdGrade,
                AcademicLevel.ProgrammingFoundations
            },
            _ => new[]
            {
                AcademicLevel.Kindergarten
            }
        };

        if (disability == DisabilityType.Intellectual && age >= 6 && age <= 15)
        {
            var foundational = new[]
            {
                AcademicLevel.Kindergarten,
                AcademicLevel.EarlyElementary,
                AcademicLevel.MiddleElementary,
                AcademicLevel.UpperElementary
            };

            return foundational
                .Concat(baseLevels)
                .Distinct()
                .ToArray();
        }

        return baseLevels;
    }

    /// <summary>
    /// Devuelve los niveles escolares permitidos considerando una colección de discapacidades múltiples (comorbilidad).
    /// </summary>
    public static IReadOnlyList<AcademicLevel> GetAllowedLevelsForAge(int age, IEnumerable<DisabilityType> disabilities)
    {
        bool hasIntellectual = disabilities != null && disabilities.Contains(DisabilityType.Intellectual);
        return GetAllowedLevelsForAge(age, hasIntellectual ? DisabilityType.Intellectual : DisabilityType.None);
    }

    /// <summary>
    /// Sugiere el nivel escolar inicial óptimo dentro de los niveles permitidos para la edad indicada.
    /// </summary>
    public static AcademicLevel GetRecommendedLevelForAge(int age) => age switch
    {
        <= 5 => AcademicLevel.Kindergarten,
        <= 8 => AcademicLevel.EarlyElementary,
        <= 10 => AcademicLevel.MiddleElementary,
        <= 12 => AcademicLevel.UpperElementary,
        13 => AcademicLevel.SecondaryFirstGrade,
        14 => AcademicLevel.SecondarySecondGrade,
        15 => AcademicLevel.SecondaryThirdGrade,
        16 => AcademicLevel.HighSchoolFirstGrade,
        17 => AcademicLevel.HighSchoolSecondGrade,
        _ => AcademicLevel.HighSchoolThirdGrade
    };

    /// <summary>
    /// Determina el registro de lenguaje apropiado según la edad, el nivel y las
    /// preferencias del alumno. La edad manda: la discapacidad no debe infantilizar
    /// a un adolescente ni ocultar apoyos a un niño pequeño.
    /// </summary>
    public static LanguageRegister GetLanguageRegister(UserProfile profile)
    {
        if (profile.Preferences.SimplifyLanguage || profile.HasDisability(DisabilityType.Intellectual))
        {
            return profile.Age <= 8 ? LanguageRegister.PlayfulEarly : LanguageRegister.Simple;
        }

        return profile.Age switch
        {
            <= 5 => LanguageRegister.PlayfulEarly,
            <= 8 => LanguageRegister.Simple,
            <= 12 => LanguageRegister.Standard,
            _ => LanguageRegister.Technical
        };
    }

    private static bool PrefersSimpleLanguage(UserProfile profile) =>
        GetLanguageRegister(profile) is LanguageRegister.PlayfulEarly or LanguageRegister.Simple;

    /// <summary>
    /// Obtiene la instrucción adaptada a la edad y discapacidad del alumno.
    /// </summary>
    public string GetAdaptedInstruction(Exercise exercise, UserProfile profile)
    {
        if (PrefersSimpleLanguage(profile))
        {
            return !string.IsNullOrWhiteSpace(exercise.SimplifiedInstruction)
                ? exercise.SimplifiedInstruction
                : exercise.StandardInstruction;
        }

        return exercise.StandardInstruction;
    }

    /// <summary>
    /// Obtiene la explicación del tema adaptada.
    /// </summary>
    public string GetAdaptedTopicIntro(PedagogicalTopic topic, UserProfile profile)
    {
        if (PrefersSimpleLanguage(profile))
        {
            return !string.IsNullOrWhiteSpace(topic.NarrativeIntroSimplified)
                ? topic.NarrativeIntroSimplified
                : topic.NarrativeIntro;
        }

        return topic.NarrativeIntro;
    }

    /// <summary>
    /// Obtiene la explicación del concepto adaptada.
    /// </summary>
    public string GetAdaptedConceptExplanation(PedagogicalTopic topic, UserProfile profile)
    {
        if (PrefersSimpleLanguage(profile))
        {
            return !string.IsNullOrWhiteSpace(topic.CoreConceptSimplified)
                ? topic.CoreConceptSimplified
                : topic.CoreConceptExplanation;
        }

        return topic.CoreConceptExplanation;
    }

    /// <summary>
    /// Obtiene el mensaje de éxito adaptado al registro del alumno.
    /// </summary>
    public string GetAdaptedSuccess(Exercise exercise, UserProfile profile)
    {
        if (PrefersSimpleLanguage(profile))
        {
            if (!string.IsNullOrWhiteSpace(exercise.SimplifiedExplanationOfSuccess))
            {
                return exercise.SimplifiedExplanationOfSuccess;
            }

            return SimplifiedHintCatalog.GetSimpleSuccess(exercise.GameType) ?? exercise.ExplanationOfSuccess;
        }

        return exercise.ExplanationOfSuccess;
    }

    /// <summary>
    /// Obtiene la pista adaptada al registro del alumno.
    /// </summary>
    public string GetAdaptedHint(Exercise exercise, UserProfile profile, bool detailed)
    {
        if (PrefersSimpleLanguage(profile))
        {
            string simplified = detailed ? exercise.SimplifiedSecondHint : exercise.SimplifiedFirstHint;
            if (!string.IsNullOrWhiteSpace(simplified))
            {
                return simplified;
            }

            return SimplifiedHintCatalog.GetSimpleHint(exercise.GameType, detailed)
                ?? (detailed ? exercise.SecondHintDetailed : exercise.FirstHint);
        }

        return detailed ? exercise.SecondHintDetailed : exercise.FirstHint;
    }

    /// <summary>
    /// Filtra y adapta las opciones de respuesta.
    /// Para discapacidad intelectual o dificultades de procesamiento, reduce el número de distractores
    /// para evitar la sobrecarga cognitiva sin eliminar el reto (WCAG 3.1 y DUA: Diseño Universal para el Aprendizaje).
    /// </summary>
    public List<ExerciseOption> GetAdaptedOptions(Exercise exercise, UserProfile profile)
    {
        if (profile.Preferences.ReduceOptionsCount || profile.HasDisability(DisabilityType.Intellectual))
        {
            var correct = exercise.Options.FirstOrDefault(o => o.IsCorrect);
            if (correct is null)
            {
                return exercise.Options;
            }

            // El distractor más cercano al resultado es el más informativo
            // (obliga a discriminar) y evita que la opción reducida sea siempre
            // la primera de la lista original.
            var distractor = exercise.Options
                .Where(o => !o.IsCorrect)
                .OrderBy(o => Math.Abs(o.Value - correct.Value))
                .FirstOrDefault();

            var reduced = new List<ExerciseOption>(2);
            foreach (var source in exercise.Options
                .Where(o => ReferenceEquals(o, correct) || ReferenceEquals(o, distractor)))
            {
                // Copia: los atajos limpios (1 y 2) no deben mutar las opciones
                // del ejercicio compartido; se conserva el orden original para
                // no revelar la posición de la respuesta correcta.
                reduced.Add(new ExerciseOption
                {
                    Id = source.Id,
                    Label = source.Label,
                    Value = source.Value,
                    IsCorrect = source.IsCorrect,
                    SpeechFeedback = source.SpeechFeedback,
                    AccessibleDescription = source.AccessibleDescription,
                    AccessibleKeyShortcut = (reduced.Count + 1).ToString()
                });
            }

            return reduced;
        }

        return exercise.Options;
    }

    /// <summary>
    /// Registra el resultado del ejercicio, premiando el esfuerzo y adaptando la racha.
    /// </summary>
    public void RecordAttempt(UserProfile profile, Exercise exercise, bool isCorrect, int attemptCount)
    {
        if (isCorrect)
        {
            profile.CurrentStreak++;

            // Las estrellas y puntos se otorgan una sola vez por ejercicio:
            // re-jugar la misma misión ya no infla el total acumulado,
            // la mejor calificación histórica vive en LessonStarRatings.
            if (!profile.CompletedExerciseIds.Contains(exercise.Id))
            {
                int starsEarned = attemptCount switch
                {
                    1 => 3, // 3 estrellas si lo logra al primer intento
                    2 => 2, // 2 estrellas con una pista
                    _ => 1  // 1 estrella asistida (siempre premia el esfuerzo y la perseverancia)
                };

                profile.StarsTotal += starsEarned;
                profile.PointsTotal += starsEarned * 10;
                profile.CompletedExerciseIds.Add(exercise.Id);
            }
        }
        else
        {
            // No reiniciamos la racha de forma punitiva para no desmotivar a los niños
            if (profile.CurrentStreak > 0)
            {
                profile.CurrentStreak = Math.Max(0, profile.CurrentStreak - 1);
            }
        }
    }

    /// <summary>
    /// Construye la cadena del anuncio accesible detallando cada opción de respuesta para TTS y lector de pantalla (Mejora 3).
    /// Formato: "Opción 1: {Opcion1}. Opción 2: {Opcion2}. Opción 3: {Opcion3}. Presiona 1, 2 o 3 para responder."
    /// </summary>
    public static string BuildOptionsAnnouncement(IReadOnlyList<ExerciseOption> options)
    {
        if (options == null || options.Count == 0) return string.Empty;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < options.Count; i++)
        {
            var opt = options[i];
            string optionText = string.IsNullOrWhiteSpace(opt.AccessibleDescription)
                ? opt.Label
                : opt.AccessibleDescription;
            sb.Append($"Opción {i + 1}: {optionText}. ");
        }

        string instruction = options.Count switch
        {
            1 => "Presiona 1 para responder.",
            2 => "Presiona 1 o 2 para responder.",
            3 => "Presiona 1, 2 o 3 para responder.",
            _ => $"Presiona del 1 al {options.Count} para responder."
        };
        sb.Append(instruction);

        return sb.ToString().Trim();
    }
}
