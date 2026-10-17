using System.Collections.Generic;

namespace MathTutor.Core.Models;

/// <summary>
/// Tipo de actividad o mecánica lúdica del ejercicio.
/// </summary>
public enum GamePlayType
{
    InteractiveCounting,      // Conteo interactivo paso a paso con sonidos ascendentes y confirmación de cardinalidad
    OrderSequence,            // Camino de piedras: completar la secuencia en orden estable
    Comparison,               // Balanza mágica: más que, menos que, igual que
    AdditionCombine,          // Juntar elementos en la cesta mágica y descubrir la suma
    CommutativePropertyDemo,  // Los gemelos traviesos: intercambiar el orden y ver que da lo mismo (a+b = b+a)
    AssociativePropertyDemo,  // Vagones de tren: agrupar de diferentes maneras ((a+b)+c = a+(b+c))
    ZeroNeutralProperty,      // La nube transparente: sumar o restar cero no cambia nada (a+0 = a)
    SubtractionTakeAway,      // Quitar estrellas o despedir amiguitos y contar los que quedan
    InverseRelation,          // Deshacer el camino: si 3+2=5, entonces 5-2=3
    PatternAndLoopSeed,       // Semilla a programación: repetir patrones y saltos idénticos
    MultiplicationArray,      // Matrices rectangulares de filas x columnas y grupos iguales
    DivisionFairSharing,      // Reparto equitativo entre amigos sin que sobre nada
    FractionParts,            // Fracción como parte de un todo: numerador (partes tomadas) y denominador (total de partes)
    FractionEquivalence,      // Fracciones equivalentes (1/2 = 2/4)
    MultiplicationDistributive, // Descomposición aditiva y propiedad distributiva: (20 + 4) x 3 = 60 + 12
    MixedFractions,           // Fracciones impropias y números mixtos (ej. 1 1/2 = 3/2)
    DecimalsMoney,            // Decimales cotidianos: décimos y centésimos con monedas
    PowersAndExponentGrowth,  // Potencias iniciales y crecimiento exponencial (2^1, 2^2, 2^3)
    DirectProportion,         // Proporcionalidad directa y regla de tres (a/b = c/d)
    AlgebraicBalance,         // Balanza de incógnitas y ecuaciones lineales simples (x + a = b)
    SquareRootPerfect,        // Raíz cuadrada de cuadrados perfectos (lado de un área cuadrada)
    CartesianCoordinates,     // Coordenadas cartesianas en el plano (X, Y)
    SignedNumbersAddition,    // Números enteros con signo (positivos y negativos, deudas y saldo)
    OrderOfOperationsPEMDAS,  // Jerarquía de operaciones y signos de agrupación (PEMDAS)
    LinearEquationAXB,        // Ecuaciones lineales de la forma ax + b = c
    PercentageCalculation,    // Cálculo de porcentajes y descuentos en la tiendita
    ExponentLaws,             // Leyes de los exponentes (producto de potencias, potencia de potencia, a^0=1)
    PolynomialLikeTerms,      // Reducción de términos semejantes y polinomios
    LinearSystem2x2,          // Sistemas de dos ecuaciones lineales con dos incógnitas (2x2)
    PythagoreanTheorem        // Teorema de Pitágoras (a^2 + b^2 = c^2 en triángulos rectángulos y rampas)
}

/// <summary>
/// Representa un objeto concreto contable o manipulable en el minijuego.
/// Proporciona accesibilidad multimodal: imagen/icono, etiqueta de texto, color y nota musical.
/// </summary>
public class ManipulableItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = "Objeto";
    public string EmojiSymbol { get; set; } = "⭐";
    public string AccessibleDescription { get; set; } = "Una estrella brillante";
    public int OrderIndex { get; set; } = 1;
    public bool IsCounted { get; set; } = false;
    public int MusicalPitchHz { get; set; } = 440; // Frecuencia para retroalimentación sonora
}

/// <summary>
/// Opción de respuesta accesible con retroalimentación inmediata formativa.
/// </summary>
public class ExerciseOption
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool IsCorrect { get; set; }
    public string SpeechFeedback { get; set; } = string.Empty;
    public string AccessibleKeyShortcut { get; set; } = string.Empty; // Ej. "1", "2", "3"
}

/// <summary>
/// Reto o ejercicio matemático adaptativo.
/// </summary>
public class Exercise
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public GamePlayType GameType { get; set; }

    // Enunciados adaptativos
    public string StandardInstruction { get; set; } = string.Empty;
    public string SimplifiedInstruction { get; set; } = string.Empty;
    public string ScreenReaderDescription { get; set; } = string.Empty;

    // Elementos concretos del juego
    public List<ManipulableItem> ItemsGroupA { get; set; } = new();
    public List<ManipulableItem> ItemsGroupB { get; set; } = new();

    // Opciones de respuesta
    public List<ExerciseOption> Options { get; set; } = new();

    // Pistas escalonadas
    public string FirstHint { get; set; } = string.Empty;
    public string SecondHintDetailed { get; set; } = string.Empty;
    public string ExplanationOfSuccess { get; set; } = string.Empty;

    // Metadatos pedagógicos
    public int ExpectedResult { get; set; }
    public int TargetAgeMin { get; set; } = 3;
    public int TargetAgeMax { get; set; } = 8;
    public AcademicLevel Level { get; set; } = AcademicLevel.Kindergarten;
}
