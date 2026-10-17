namespace MathTutor.Core.Models;

/// <summary>
/// Tipos de discapacidad contemplados en la caracterización del usuario.
/// Permite adaptar la interfaz, el lenguaje pedagógico, los estímulos sensoriales y la dificultad.
/// </summary>
public enum DisabilityType
{
    None = 0,               // Sin discapacidad (Neurotípico)
    Visual = 1,             // Discapacidad visual (Ceguera total, Baja visión)
    Hearing = 2,            // Discapacidad auditiva (Hipoacusia, Sordera)
    Motor = 3,              // Discapacidad motriz (Dificultades motoras finas, movilidad reducida)
    Intellectual = 4,       // Discapacidad intelectual / cognitiva (requiere lenguaje simplificado y menor carga)
    Neurodivergent = 5      // Neurodivergencia (TDAH, Trastorno del Espectro Autista - baja sobrecarga sensorial)
}

/// <summary>
/// Grado o severidad de la discapacidad presentada.
/// </summary>
public enum DisabilitySeverity
{
    Mild = 1,       // Leve
    Moderate = 2,   // Moderada
    Severe = 3      // Severa
}

/// <summary>
/// Nivel o grado escolar del alumno.
/// Diseñado para abarcar desde Jardín de niños hasta niveles avanzados y programación.
/// </summary>
public enum AcademicLevel
{
    Kindergarten = 1,       // Jardín de niños (3 a 5 años): Principios del conteo, noción de cantidad
    EarlyElementary = 2,    // Primaria baja (6 a 8 años - 1º y 2º grado): Suma, resta, propiedades, recta numérica
    MiddleElementary = 3,   // Primaria media (9 a 10 años - 3º y 4º): Multiplicación, división, fracciones
    UpperElementary = 4,    // Primaria alta (11 a 12 años - 5º y 6º): Proporciones, geometría, potencias
    SecondaryFirstGrade = 6, // 1º de secundaria (12 a 13 años): Números con signo, jerarquía, ecuaciones ax+b=c, porcentajes
    SecondarySecondGrade = 7, // 2º de secundaria (13 a 14 años): Leyes de exponentes, Pitágoras, sistemas 2x2
    SecondaryThirdGrade = 8,  // 3º de secundaria (14 a 15 años): Ecuaciones cuadráticas, trigonometría básica, semejanza
    HighSchoolFirstGrade = 9, // 1º de preparatoria / bachillerato (15 a 16 años): Geometría analítica, pendiente m, recta y=mx+b, distancia
    HighSchoolSecondGrade = 10, // 2º de preparatoria / bachillerato (16 a 17 años): Precálculo, funciones trigonométricas continuas, logaritmos
    HighSchoolThirdGrade = 11,  // 3º de preparatoria / bachillerato (17 a 18 años): Introducción al cálculo diferencial/integral y probabilidad
    ProgrammingFoundations = 5 // Fundamentos lógicos y algoritmos matemáticos (pensamiento computacional)
}

/// <summary>
/// Contexto regional y cultural del alumno en América Latina para adaptar vocabulario, monedas y metáforas cotidianas.
/// </summary>
public enum RegionalContext
{
    Mexico = 1,       // Pesos ($) y centavos; tiendita, mercado
    Colombia = 2,     // Pesos ($); tienda de barrio
    Argentina = 3,    // Pesos ($) y centavos; kiosco
    Peru = 4,         // Soles (S/) y céntimos; bodega
    Chile = 5         // Pesos ($); almacén
}

/// <summary>
/// Esquemas de contraste y visualización para cumplimiento de directrices de accesibilidad (WCAG 2.2).
/// </summary>
public enum HighContrastTheme
{
    DefaultPlayful = 0,     // Colorido, lúdico y amigable con contraste verificado AA (> 4.5:1)
    HighContrastDark = 1,   // Fondo negro con texto y elementos en amarillo brillante y blanco (> 15:1)
    HighContrastLight = 2,  // Fondo blanco puro con texto y bordes en negro absoluto (> 21:1)
    LowSensory = 3          // Tonos suaves pastel, sin parpadeos ni saturaciones (ideal para TEA / TDAH)
}

/// <summary>
/// Escala de tamaños de fuente para legibilidad y baja visión.
/// </summary>
public enum FontSizeLevel
{
    Normal = 0,     // 18pt base (ya amplio para niños)
    Large = 1,      // 24pt base
    ExtraLarge = 2, // 30pt base
    Huge = 3        // 36pt base
}

/// <summary>
/// Estado del resultado en un ejercicio pedagógico.
/// </summary>
public enum AnswerResult
{
    None = 0,
    Correct = 1,
    NeedsGuidance = 2,
    Assisted = 3
}
