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
    /// Obtiene la instrucción adaptada a la edad y discapacidad del alumno.
    /// </summary>
    public string GetAdaptedInstruction(Exercise exercise, UserProfile profile)
    {
        if (profile.Preferences.SimplifyLanguage || profile.Disability == DisabilityType.Intellectual)
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
        if (profile.Preferences.SimplifyLanguage || profile.Disability == DisabilityType.Intellectual)
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
        if (profile.Preferences.SimplifyLanguage || profile.Disability == DisabilityType.Intellectual)
        {
            return !string.IsNullOrWhiteSpace(topic.CoreConceptSimplified)
                ? topic.CoreConceptSimplified
                : topic.CoreConceptExplanation;
        }

        return topic.CoreConceptExplanation;
    }

    /// <summary>
    /// Filtra y adapta las opciones de respuesta.
    /// Para discapacidad intelectual o dificultades de procesamiento, reduce el número de distractores
    /// para evitar la sobrecarga cognitiva sin eliminar el reto (WCAG 3.1 y DUA: Diseño Universal para el Aprendizaje).
    /// </summary>
    public List<ExerciseOption> GetAdaptedOptions(Exercise exercise, UserProfile profile)
    {
        if (profile.Preferences.ReduceOptionsCount || profile.Disability == DisabilityType.Intellectual)
        {
            var correct = exercise.Options.FirstOrDefault(o => o.IsCorrect);
            var incorrect = exercise.Options.Where(o => !o.IsCorrect).Take(1).ToList();

            var reduced = new List<ExerciseOption>();
            if (correct != null) reduced.Add(correct);
            reduced.AddRange(incorrect);

            // Reasignar atajos de teclado numéricos limpios (1 y 2)
            for (int i = 0; i < reduced.Count; i++)
            {
                reduced[i].AccessibleKeyShortcut = (i + 1).ToString();
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
            int starsEarned = attemptCount switch
            {
                1 => 3, // 3 estrellas si lo logra al primer intento
                2 => 2, // 2 estrellas con una pista
                _ => 1  // 1 estrella asistida (siempre premia el esfuerzo y la perseverancia)
            };

            profile.StarsTotal += starsEarned;
            profile.PointsTotal += starsEarned * 10;

            if (!profile.CompletedExerciseIds.Contains(exercise.Id))
            {
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
}
