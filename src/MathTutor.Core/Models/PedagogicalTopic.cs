using System.Collections.Generic;

namespace MathTutor.Core.Models;

/// <summary>
/// Módulo o lección pedagógica que combina una narrativa lúdica,
/// explicación conceptual clara y minijuegos con retos adaptativos.
/// </summary>
public class PedagogicalTopic
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public AcademicLevel Level { get; set; }
    public int TargetAgeMin { get; set; } = 3;
    public int TargetAgeMax { get; set; } = 18;
    public string CharacterName { get; set; } = "Cosmo el Búho Sabio";
    
    // Narrativa y explicación
    public string NarrativeIntro { get; set; } = string.Empty;
    public string NarrativeIntroSimplified { get; set; } = string.Empty;
    public string CoreConceptExplanation { get; set; } = string.Empty;
    public string CoreConceptSimplified { get; set; } = string.Empty;

    // Principios matemáticos que enseña
    public List<string> PrinciplesAddressed { get; set; } = new();

    // Ejercicios y minijuegos correspondientes
    public List<Exercise> Exercises { get; set; } = new();

    // Resumen de cierre significativo
    public string CongratulationMessage { get; set; } = string.Empty;
}
