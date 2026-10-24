using System;
using System.Collections.Generic;
using System.Linq;
using MathTutor.Core.Models;

namespace MathTutor.Core.Engine;

/// <summary>
/// Generador dinámico de ejercicios matemáticos adaptados según la edad,
/// nivel escolar y tipo de discapacidad del alumno.
/// </summary>
public class ExerciseGenerator
{
    private static readonly int[] PentatonicScaleHz = { 262, 294, 330, 349, 392, 440, 494, 523, 587, 659 };

    private static readonly (string Name, string Emoji, string Description)[] ItemVisualTypes =
    {
        ("Manzana", "🍎", "Una manzana roja jugosa"),
        ("Estrella", "⭐", "Una estrella dorada brillante"),
        ("Bellota", "🌰", "Una bellota del bosque"),
        ("Flor", "🌸", "Una flor rosa mágica"),
        ("Mariposa", "🦋", "Una mariposa de colores"),
        ("Campana", "🔔", "Una campana sonora dorada"),
        ("Corazón", "💖", "Un corazón brillante")
    };

    /// <summary>
    /// Determina el rango máximo numérico adecuado para el alumno.
    /// Para discapacidad intelectual o edades tempranas (3-4 años), reduce el límite superior.
    /// </summary>
    public (int Min, int Max) GetTargetRange(UserProfile profile)
    {
        if (profile.HasDisability(DisabilityType.Intellectual))
        {
            return profile.Support.IntellectualSupport switch
            {
                IntellectualSupportLevel.Extensive or IntellectualSupportLevel.Pervasive => (1, 3),
                _ => (1, 5)
            };
        }

        return profile.CurrentLevel switch
        {
            AcademicLevel.Kindergarten => profile.Age <= 4 ? (1, 4) : (1, 5),
            AcademicLevel.EarlyElementary => (1, 10),
            _ => (1, 10)
        };
    }

    /// <summary>
    /// Genera un ejercicio interactivo de conteo (Gelman & Gallistel: Correspondencia 1 a 1 y Cardinalidad).
    /// </summary>
    public Exercise GenerateCountingExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int count = rng.Next(min, max + 1);

        var visual = ItemVisualTypes[rng.Next(ItemVisualTypes.Length)];

        var ex = new Exercise
        {
            Id = $"gen_count_{Guid.NewGuid():N}",
            Title = $"El Tesoro de las {visual.Name}s Mágicas",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 3,
            TargetAgeMax = 6,
            Level = profile.CurrentLevel,
            ExpectedResult = count,
            StandardInstruction = $"En el jardín aparecieron {visual.Name.ToLower()}s mágicas. Toca o selecciona cada una para contarla al ritmo de la música. ¿Cuántas hay en total?",
            SimplifiedInstruction = $"Toca cada {visual.Name.ToLower()} para contar. Una, dos... ¿cuántas hay en total?",
            ScreenReaderDescription = $"Hay varias {visual.Name.ToLower()}s en el jardín. Explora y cuenta una a una.",
            FirstHint = $"Toca la primera {visual.Name.ToLower()} y escucha su tono musical. Luego sigue con la siguiente.",
            SecondHintDetailed = $"Cuenta despacito: el último número que digas te dirá cuántas {visual.Name.ToLower()}s hay en total.",
            ExplanationOfSuccess = $"¡Muy bien! Hay {count} {visual.Name.ToLower()}s en total.",
            SimplifiedFirstHint = $"Toca la primera {visual.Name.ToLower()} y luego la siguiente.",
            SimplifiedSecondHint = $"Cuenta una por una y di el último número.",
            SimplifiedExplanationOfSuccess = $"¡Muy bien! Hay {count} {visual.Name.ToLower()}s."
        };

        for (int i = 0; i < count; i++)
        {
            int pitch = PentatonicScaleHz[i % PentatonicScaleHz.Length];
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_{i + 1}",
                Name = $"{visual.Name} {i + 1}",
                EmojiSymbol = visual.Emoji,
                AccessibleDescription = $"{visual.Description} número {i + 1}",
                OrderIndex = i + 1,
                MusicalPitchHz = pitch
            });
        }

        BuildOptions(ex, count, min: 1, max: max + 2, unitLabel: visual.Name.ToLower() + "s", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio del Principio de Orden Estable (completar la secuencia numérica).
    /// </summary>
    public Exercise GenerateStableOrderExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int start = Math.Clamp(rng.Next(min, Math.Max(min, max - 2)), 1, 7);
        int missing = start + 3; // Ejemplo: start=1 -> 1, 2, 3, [4]

        var ex = new Exercise
        {
            Id = $"gen_order_{Guid.NewGuid():N}",
            Title = "El Puente de Piedras Musicales",
            GameType = GamePlayType.OrderSequence,
            TargetAgeMin = 3,
            TargetAgeMax = 6,
            Level = profile.CurrentLevel,
            ExpectedResult = missing,
            StandardInstruction = $"Para cruzar el río, pisamos las piedras que van en orden: {start}, {start + 1}, {start + 2}... ¿Qué número sigue en la siguiente piedra?",
            SimplifiedInstruction = $"Las piedras dicen: {start}, {start + 1}, {start + 2}... ¿Qué número sigue ahora?",
            ScreenReaderDescription = $"Una fila de piedras sobre el río con los números {start}, {start + 1}, {start + 2}. Falta la última piedra.",
            FirstHint = $"Di los números en voz alta como una canción: {start}, {start + 1}, {start + 2}...",
            SecondHintDetailed = $"Después del número {start + 2} viene el número {missing}. Los números van en orden.",
            ExplanationOfSuccess = $"¡Bien! El número {missing} es el correcto. Los números van en orden: {start}, {start + 1}, {start + 2}, {missing}.",
            SimplifiedFirstHint = $"Di los números: {start}, {start + 1}, {start + 2}...",
            SimplifiedSecondHint = $"Después del {start + 2} viene el {missing}.",
            SimplifiedExplanationOfSuccess = $"¡Bien! El número {missing} es el que sigue."
        };

        for (int i = 0; i < 3; i++)
        {
            int num = start + i;
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"stone_{num}",
                Name = $"Piedra {num}",
                EmojiSymbol = $"🪨 {num}",
                AccessibleDescription = $"Piedra con el número {num}",
                OrderIndex = num,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length],
                IsCounted = true
            });
        }

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "stone_missing",
            Name = "Piedra Misteriosa",
            EmojiSymbol = "❓",
            AccessibleDescription = "Piedra misteriosa con signo de interrogación esperando tu respuesta",
            OrderIndex = missing,
            MusicalPitchHz = PentatonicScaleHz[3 % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, missing, min: Math.Max(1, missing - 2), max: missing + 2, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de adición (juntar colecciones A y B).
    /// </summary>
    public Exercise GenerateAdditionCombineExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (_, max) = GetTargetRange(profile);
        int halfMax = Math.Max(2, max / 2);
        int a = rng.Next(1, halfMax + 1);
        int b = rng.Next(1, halfMax + 1);
        int sum = a + b;

        var visual = ItemVisualTypes[rng.Next(ItemVisualTypes.Length)];

        var ex = new Exercise
        {
            Id = $"gen_add_{Guid.NewGuid():N}",
            Title = $"La Gran Cesta de {visual.Name}s",
            GameType = GamePlayType.AdditionCombine,
            TargetAgeMin = 5,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = sum,
            StandardInstruction = $"En una cesta pusimos {a} {visual.Name.ToLower()}s. Luego trajimos {b} más y las juntamos todas. ¿Cuántas hay en total? ¡Calcula {a} + {b}!",
            SimplifiedInstruction = $"Tienes {a} y te dan {b} más. Júntalas todas. ¿Cuántas son?",
            ScreenReaderDescription = $"Grupo 1 con {a} {visual.Name.ToLower()}s y Grupo 2 con {b} {visual.Name.ToLower()}s que se juntan.",
            FirstHint = $"Empieza en el número {a} y avanza {b} pasitos hacia adelante.",
            SecondHintDetailed = $"{a} más {b} es igual a {sum}.",
            ExplanationOfSuccess = $"¡Excelente! {a} + {b} = {sum}. Cuando juntamos dos grupos para saber el total, ¡estamos sumando!"
        };

        for (int i = 0; i < a; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"a_{i + 1}",
                Name = $"{visual.Name} A{i + 1}",
                EmojiSymbol = visual.Emoji,
                AccessibleDescription = $"{visual.Name} del primer grupo",
                OrderIndex = i + 1,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
            });
        }

        for (int i = 0; i < b; i++)
        {
            ex.ItemsGroupB.Add(new ManipulableItem
            {
                Id = $"b_{i + 1}",
                Name = $"{visual.Name} B{i + 1}",
                EmojiSymbol = visual.Emoji,
                AccessibleDescription = $"{visual.Name} agregada al segundo grupo",
                OrderIndex = a + i + 1,
                MusicalPitchHz = PentatonicScaleHz[(a + i) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, sum, min: Math.Max(1, sum - 2), max: sum + 2, unitLabel: visual.Name.ToLower() + "s", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de la Propiedad Conmutativa de la Suma (a + b = b + a).
    /// </summary>
    public Exercise GenerateCommutativeExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (_, max) = GetTargetRange(profile);
        int halfMax = Math.Max(2, max / 2);
        int a = rng.Next(1, halfMax + 1);
        int b = rng.Next(1, halfMax + 1);
        while (b == a) b = rng.Next(1, halfMax + 1); // Asegurar sumandos distintos para ilustrar conmutatividad
        int sum = a + b;

        var ex = new Exercise
        {
            Id = $"gen_comm_{Guid.NewGuid():N}",
            Title = "El Espejo Mágico: Propiedad Conmutativa",
            GameType = GamePlayType.CommutativePropertyDemo,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = sum,
            StandardInstruction = $"Sabemos que {a} + {b} = {sum}. Si ahora cambiamos el orden de los amigos y sumamos {b} + {a}, ¿cuál será el resultado?",
            SimplifiedInstruction = $"Si {a} + {b} son {sum}... al cambiar el orden a {b} + {a}, ¿cuánto da?",
            ScreenReaderDescription = $"Comprobación de la propiedad conmutativa: comparar {a} + {b} con {b} + {a}.",
            FirstHint = "¡Recuerda el superpoder conmutativo! Cambiar de lugar los números no cambia el resultado final.",
            SecondHintDetailed = $"Tanto {a} + {b} como {b} + {a} dan el mismo número: {sum}.",
            ExplanationOfSuccess = $"¡Brillante! {a} + {b} = {sum} y {b} + {a} = {sum}. El orden de los sumandos no cambia la suma."
        };

        BuildOptions(ex, sum, min: Math.Max(1, sum - 2), max: sum + 2, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio del Elemento Neutro de la Suma (a + 0 = a).
    /// </summary>
    public Exercise GenerateNeutralZeroExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int a = rng.Next(Math.Max(2, min), max + 1);

        var ex = new Exercise
        {
            Id = $"gen_zero_{Guid.NewGuid():N}",
            Title = "La Nube Transparente: El Cero en la Suma",
            GameType = GamePlayType.ZeroNeutralProperty,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = a,
            StandardInstruction = $"En tu cofre tienes {a} gemas relucientes. Cosmo te regala una cajita mágica con 0 gemas y las suma a tu cofre. ¿Cuántas gemas tienes ahora? ¡Es {a} + 0!",
            SimplifiedInstruction = $"Tienes {a} gemas. Si le sumas 0 gemas, ¿cuántas tienes?",
            ScreenReaderDescription = $"Un cofre con {a} gemas y una caja vacía con 0 gemas. Operación {a} + 0.",
            FirstHint = "Sumar cero significa no agregar nada. ¿Cambió la cantidad de gemas que ya tenías?",
            SecondHintDetailed = $"{a} + 0 sigue siendo {a}. El cero deja al número exactamente igual.",
            ExplanationOfSuccess = $"¡Exacto! {a} + 0 = {a}. El cero es el elemento neutro de la suma: no agrega nada y mantiene el valor intacto."
        };

        BuildOptions(ex, a, min: Math.Max(0, a - 2), max: a + 2, unitLabel: "gemas", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de sustracción (quitar elementos).
    /// </summary>
    public Exercise GenerateSubtractionExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int total = rng.Next(Math.Max(3, min + 1), max + 2);
        int remove = rng.Next(1, total);
        int remaining = total - remove;

        var visual = ItemVisualTypes[rng.Next(ItemVisualTypes.Length)];

        var ex = new Exercise
        {
            Id = $"gen_sub_{Guid.NewGuid():N}",
            Title = $"La Despedida de las {visual.Name}s",
            GameType = GamePlayType.SubtractionTakeAway,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = remaining,
            StandardInstruction = $"Había {total} {visual.Name.ToLower()}s en la rama. El viento sopló y se llevó {remove} de ellas. ¿Cuántas quedaron en la rama? ¡Calcula {total} - {remove}!",
            SimplifiedInstruction = $"Había {total} {visual.Name.ToLower()}s y se fueron {remove}. ¿Cuántas quedaron?",
            ScreenReaderDescription = $"Había {total} {visual.Name.ToLower()}s y se restan {remove}.",
            FirstHint = $"Teníamos {total}. Cuenta hacia atrás {remove} pasitos.",
            SecondHintDetailed = $"{total} menos {remove} es igual a {remaining}.",
            ExplanationOfSuccess = $"¡Sensacional! {total} - {remove} = {remaining}. Restar es quitar o separar elementos para saber los que quedan."
        };

        BuildOptions(ex, remaining, min: Math.Max(1, remaining - 2), max: remaining + 2, unitLabel: visual.Name.ToLower() + "s", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de multiplicación temprana mediante matrices rectangulares (filas x columnas) y grupos iguales.
    /// </summary>
    public Exercise GenerateMultiplicationArrayExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 6);

        int rows = isSimplified ? rng.Next(2, 4) : rng.Next(2, 5);
        int cols = isSimplified ? rng.Next(1, 3) : rng.Next(2, 5);
        int total = rows * cols;

        var visual = ItemVisualTypes[rng.Next(ItemVisualTypes.Length)];

        var ex = new Exercise
        {
            Id = $"gen_mult_{Guid.NewGuid():N}",
            Title = $"El Huerto Mágico: Filas de {visual.Name}s",
            GameType = GamePlayType.MultiplicationArray,
            TargetAgeMin = 6,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = total,
            StandardInstruction = $"En el huerto de Cosmo plantamos {rows} filas con {cols} {visual.Name.ToLower()}s en cada fila. ¿Cuántas hay en total? ¡Calcula {rows} × {cols}!",
            SimplifiedInstruction = $"Hay {rows} grupitos. Cada grupito tiene {cols} {visual.Name.ToLower()}s. ¿Cuántas hay en total?",
            ScreenReaderDescription = $"Una cuadrícula de {rows} filas por {cols} columnas con {visual.Name.ToLower()}s. Cuenta las filas iguales para descubrir el total.",
            FirstHint = $"Suma cada fila: {string.Join(" + ", Enumerable.Repeat(cols.ToString(), rows))}...",
            SecondHintDetailed = $"{rows} veces {cols} es igual a {total}.",
            ExplanationOfSuccess = $"¡Espectacular! {rows} × {cols} = {total}. Multiplicar es una forma rápida de sumar grupos iguales: ¡{rows} veces {cols} da {total}!"
        };

        int itemOrder = 1;
        for (int r = 1; r <= rows; r++)
        {
            for (int c = 1; c <= cols; c++)
            {
                ex.ItemsGroupA.Add(new ManipulableItem
                {
                    Id = $"item_{r}_{c}",
                    Name = $"{visual.Name} (Fila {r}, Col {c})",
                    EmojiSymbol = visual.Emoji,
                    AccessibleDescription = $"{visual.Name} en fila {r}, columna {c}",
                    OrderIndex = itemOrder,
                    MusicalPitchHz = PentatonicScaleHz[(itemOrder - 1) % PentatonicScaleHz.Length]
                });
                itemOrder++;
            }
        }

        BuildOptions(ex, total, min: Math.Max(1, total - 3), max: total + 3, unitLabel: visual.Name.ToLower() + "s", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de división temprana mediante reparto equitativo (Fair Sharing).
    /// </summary>
    public Exercise GenerateDivisionFairSharingExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 6);

        int friends = rng.Next(2, 4); // 2 o 3 amigos
        int sharePerFriend = isSimplified ? rng.Next(1, 4) : rng.Next(2, 5); // 1 a 3 (simplificado) o 2 a 4
        int total = friends * sharePerFriend;

        var visual = ItemVisualTypes[rng.Next(ItemVisualTypes.Length)];

        var ex = new Exercise
        {
            Id = $"gen_div_{Guid.NewGuid():N}",
            Title = "El Banquete Justo: Reparto Equitativo",
            GameType = GamePlayType.DivisionFairSharing,
            TargetAgeMin = 6,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = sharePerFriend,
            StandardInstruction = $"Tenemos {total} {visual.Name.ToLower()}s deliciosas para repartir en partes exactamente iguales entre {friends} amiguitos. ¿Cuántas le tocan a cada amiguito? ¡Calcula {total} ÷ {friends}!",
            SimplifiedInstruction = $"Reparte {total} {visual.Name.ToLower()}s entre {friends} amiguitos. A todos les toca lo mismo. ¿Cuántas le tocan a cada uno?",
            ScreenReaderDescription = $"Hay {total} {visual.Name.ToLower()}s en la mesa y {friends} amigos esperando su porción equitativa.",
            FirstHint = $"Si le das una a cada amigo, gastas {friends}. Si repites hasta que no quede ninguna, ¿cuántas tendrá cada uno?",
            SecondHintDetailed = $"{total} repartido entre {friends} es igual a {sharePerFriend} para cada uno.",
            ExplanationOfSuccess = $"¡Brillante y equitativo! {total} ÷ {friends} = {sharePerFriend}. Dividir consiste en repartir en partes exactamente iguales sin que nadie quede triste."
        };

        for (int i = 0; i < total; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"div_item_{i + 1}",
                Name = $"{visual.Name} {i + 1}",
                EmojiSymbol = visual.Emoji,
                AccessibleDescription = $"{visual.Name} para repartir número {i + 1}",
                OrderIndex = i + 1,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, sharePerFriend, min: 1, max: sharePerFriend + 3, unitLabel: $"{visual.Name.ToLower()}s a cada uno", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de fracciones como partes de un todo (numerador y denominador).
    /// </summary>
    public Exercise GenerateFractionPartsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        // Denominador (partes totales): 2, 4 para simplificado; 2, 3, 4, 6 para regular
        int[] possibleDenoms = isSimplified ? new[] { 2, 4 } : new[] { 2, 3, 4, 6 };
        int denominator = possibleDenoms[rng.Next(possibleDenoms.Length)];
        int numerator = rng.Next(1, denominator);

        var (foodName, emoji) = rng.Next(2) == 0 ? ("Pizza", "🍕") : ("Barra de Chocolate", "🍫");

        var ex = new Exercise
        {
            Id = $"gen_frac_{Guid.NewGuid():N}",
            Title = $"La Repostería Mágica: Partes de la {foodName}",
            GameType = GamePlayType.FractionParts,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = numerator,
            StandardInstruction = $"Cosmo horneó una deliciosa {foodName.ToLower()} y la cortó en {denominator} partes exactamente iguales. Si te comes {numerator} de esas partes, ¿cuál es el numerador (las partes que comiste)? ¡Estamos representando la fracción {numerator}/{denominator}!",
            SimplifiedInstruction = $"La {foodName.ToLower()} se cortó en {denominator} rebanadas iguales. Si te comes {numerator}, ¿cuántas rebanadas tomaste?",
            ScreenReaderDescription = $"Una {foodName.ToLower()} dividida en {denominator} partes iguales. Se han seleccionado {numerator} partes.",
            FirstHint = $"En una fracción, el número de arriba (numerador) indica las partes tomadas ({numerator}), y el de abajo (denominador) el total ({denominator}).",
            SecondHintDetailed = $"Tomaste {numerator} partes de un total de {denominator}.",
            ExplanationOfSuccess = $"¡Excelente! El numerador es {numerator}. La fracción es {numerator}/{denominator}: tomaste {numerator} partes de un todo dividido en {denominator} partes iguales."
        };

        for (int i = 1; i <= denominator; i++)
        {
            bool isTaken = i <= numerator;
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"frac_slice_{i}",
                Name = $"Porción {i} de {denominator}",
                EmojiSymbol = emoji,
                AccessibleDescription = isTaken
                    ? $"Rebanada {i} de {denominator} (Comida/Tomada)"
                    : $"Rebanada {i} de {denominator} (Disponible)",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[(i - 1) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, numerator, min: 1, max: denominator, unitLabel: "partes tomadas", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de equivalencia de fracciones (por ejemplo, 1/2 = 2/4).
    /// </summary>
    public Exercise GenerateFractionEquivalenceExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        // 1/2 = 2/4 o 1/3 = 2/6 o 2/4 = 4/8
        var equivalences = new (int n1, int d1, int n2, int d2)[]
        {
            (1, 2, 2, 4),
            (1, 2, 3, 6),
            (2, 4, 4, 8),
            (1, 3, 2, 6)
        };
        var eq = equivalences[rng.Next(equivalences.Length)];
        int scaleFactor = eq.n2 / eq.n1;
        string factorWord = scaleFactor == 2 ? "el doble" : "el triple";

        var ex = new Exercise
        {
            Id = $"gen_equiv_{Guid.NewGuid():N}",
            Title = "El Espejo de las Fracciones Equivalentes",
            GameType = GamePlayType.FractionEquivalence,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 1, // 1 = "Sí, representan exactamente la misma cantidad"
            StandardInstruction = $"Ada comió {eq.n1}/{eq.d1} de su barra de chocolate. Tito comió {eq.n2}/{eq.d2} de una barra idéntica. ¿Ambos comieron la misma cantidad total de chocolate?",
            SimplifiedInstruction = $"¿La fracción {eq.n1}/{eq.d1} es igual en tamaño que {eq.n2}/{eq.d2}?",
            ScreenReaderDescription = $"Dos barras iguales. La primera dividida en {eq.d1} partes con {eq.n1} tomadas. La segunda dividida en {eq.d2} partes con {eq.n2} tomadas.",
            FirstHint = $"Observa que {eq.n2} es {factorWord} de {eq.n1}, y {eq.d2} es {factorWord} de {eq.d1}. ¡Ocupan el mismo espacio!",
            SecondHintDetailed = $"{eq.n1}/{eq.d1} y {eq.n2}/{eq.d2} son fracciones equivalentes: al multiplicar numerador y denominador por {scaleFactor}, representan la misma porción.",
            ExplanationOfSuccess = $"¡Exacto! {eq.n1}/{eq.d1} es equivalente a {eq.n2}/{eq.d2}. Aunque usemos números distintos, ¡representan exactamente la misma cantidad!"
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "equiv_bar_a",
            Name = $"Barra de Ada ({eq.n1}/{eq.d1})",
            EmojiSymbol = "🍫",
            AccessibleDescription = $"Barra de chocolate de Ada mostrando {eq.n1} de {eq.d1} partes iguales",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        ex.ItemsGroupB.Add(new ManipulableItem
        {
            Id = "equiv_bar_b",
            Name = $"Barra de Tito ({eq.n2}/{eq.d2})",
            EmojiSymbol = "🍫",
            AccessibleDescription = $"Barra de chocolate de Tito mostrando {eq.n2} de {eq.d2} partes iguales (equivalente a {eq.n1}/{eq.d1})",
            OrderIndex = 2,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        ex.Options.Add(new ExerciseOption
        {
            Id = "opt_yes",
            Value = 1,
            Label = $"¡Sí! Son equivalentes ({eq.n1}/{eq.d1} = {eq.n2}/{eq.d2})",
            IsCorrect = true,
            AccessibleKeyShortcut = "1",
            SpeechFeedback = $"¡Maravilloso! Son equivalentes porque representan la misma porción del chocolate."
        });

        ex.Options.Add(new ExerciseOption
        {
            Id = "opt_no",
            Value = 2,
            Label = "No, uno comió más que el otro",
            IsCorrect = false,
            AccessibleKeyShortcut = "2",
            SpeechFeedback = $"Aunque {eq.d2} pedazos son más pequeños que {eq.d1}, tomar {eq.n2} de ellos cubre exactamente el mismo tamaño que {eq.n1}/{eq.d1}."
        });

        ShuffleOptions(ex, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de multiplicación por descomposición aditiva / propiedad distributiva: (20 + 4) x 3 = 60 + 12 = 72.
    /// </summary>
    public Exercise GenerateMultiplicationDistributiveExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int tens = isSimplified ? 10 : (rng.Next(1, 4) * 10); // 10, 20 o 30
        int units = isSimplified ? rng.Next(1, 4) : rng.Next(2, 6); // 1 a 5
        int multiplier = isSimplified ? 2 : rng.Next(2, 5); // 2, 3 o 4

        int totalNumber = tens + units;
        int tensProduct = tens * multiplier;
        int unitsProduct = units * multiplier;
        int finalResult = totalNumber * multiplier;

        var ex = new Exercise
        {
            Id = $"gen_distrib_{Guid.NewGuid():N}",
            Title = $"Multiplicar Descomponiendo: {totalNumber} × {multiplier}",
            GameType = GamePlayType.MultiplicationDistributive,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = finalResult,
            StandardInstruction = $"Para calcular {totalNumber} × {multiplier}, descomponemos {totalNumber} en ({tens} + {units}). Multiplicamos por separado: ({tens} × {multiplier} = {tensProduct}) y ({units} × {multiplier} = {unitsProduct}). Al sumar {tensProduct} + {unitsProduct}, ¿cuál es el gran total?",
            SimplifiedInstruction = $"Rompe {totalNumber} en {tens} y {units}. {tens} × {multiplier} = {tensProduct}, y {units} × {multiplier} = {unitsProduct}. Suma {tensProduct} + {unitsProduct}.",
            ScreenReaderDescription = $"Multiplicación por descomposición: ({tens} + {units}) por {multiplier}. {tens} por {multiplier} es {tensProduct}, más {units} por {multiplier} que es {unitsProduct}.",
            FirstHint = $"Suma los dos bloques: {tensProduct} + {unitsProduct}.",
            SecondHintDetailed = $"{tensProduct} más {unitsProduct} es igual a {finalResult}.",
            ExplanationOfSuccess = $"¡Espectacular! {totalNumber} × {multiplier} = {finalResult}. Romper números grandes en decenas y unidades hace que multiplicar sea facilísimo."
        };

        BuildOptions(ex, finalResult, min: Math.Max(10, finalResult - 8), max: finalResult + 8, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de fracciones mixtas e impropias (5º Grado).
    /// </summary>
    public Exercise GenerateMixedFractionsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int whole = 1;
        int denom = isSimplified ? 2 : (rng.Next(2, 5)); // 2, 3 o 4
        int remNumerator = 1;

        int totalParts = (whole * denom) + remNumerator;

        var ex = new Exercise
        {
            Id = $"gen_mixed_{Guid.NewGuid():N}",
            Title = $"Fracciones Mixtas: {whole} entero y {remNumerator}/{denom}",
            GameType = GamePlayType.MixedFractions,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = totalParts,
            StandardInstruction = $"Ada horneó {whole} waffle entero y {remNumerator}/{denom} de otro. Si cada waffle se divide en {denom} pedazos iguales, ¿cuántos pedazos ({remNumerator}/{denom}) hay en total en {whole} y {remNumerator}/{denom}?",
            SimplifiedInstruction = $"Tienes 1 waffle entero y {remNumerator} pedazo de {denom}. Si partes el entero en {denom} pedazos, ¿cuántos pedazos tienes en total?",
            ScreenReaderDescription = $"Un waffle completo partido en {denom} porciones más otro waffle con {remNumerator} porción de {denom}.",
            FirstHint = $"1 waffle entero son {denom} pedazos. Súmale el pedazo extra.",
            SecondHintDetailed = $"{denom} pedazos del entero + {remNumerator} pedazo extra = {totalParts} pedazos en total ({totalParts}/{denom}).",
            ExplanationOfSuccess = $"¡Fantástico! {whole} entero con {remNumerator}/{denom} equivale a la fracción impropia {totalParts}/{denom}. ¡Tienes {totalParts} partes iguales!"
        };

        for (int i = 1; i <= totalParts; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"mixed_slice_{i}",
                Name = $"Porción de waffle {i}",
                EmojiSymbol = "🧇",
                AccessibleDescription = i <= denom
                    ? $"Porción {i} del primer waffle entero"
                    : $"Porción extra {i - denom} del segundo waffle",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[(i - 1) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, totalParts, min: Math.Max(2, totalParts - 3), max: totalParts + 3, unitLabel: "partes", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de decimales y sistema monetario (5º Grado).
    /// </summary>
    public Exercise GenerateDecimalsMoneyExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int wholeUnits = isSimplified ? 1 : rng.Next(1, 4);
        int halfUnits = isSimplified ? 1 : rng.Next(1, 3);
        int totalSubUnits = (wholeUnits * 100) + (halfUnits * 50);
        decimal decimalAmount = totalSubUnits / 100m;
        string currency = profile.CurrencyName;
        string fraction = profile.CurrencyFractionName;
        string symbol = profile.CurrencySymbol;

        var ex = new Exercise
        {
            Id = $"gen_dec_{Guid.NewGuid():N}",
            Title = $"El Monedero Mágico: {symbol}{decimalAmount:F2}",
            GameType = GamePlayType.DecimalsMoney,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = totalSubUnits,
            StandardInstruction = $"En el mercado compras frutas mágicas. Tienes {wholeUnits} moneda(s) de {symbol}1.00 (100 {fraction}) y {halfUnits} moneda(s) de {symbol}0.50 (50 {fraction}). ¿Cuántos {fraction} tienes en total? (Equivale a {symbol}{decimalAmount:F2})",
            SimplifiedInstruction = $"Tienes {wholeUnits} moneda de {symbol}1 y {halfUnits} de 50 {fraction}. ¿Cuántos {fraction} tienes en total?",
            ScreenReaderDescription = $"{wholeUnits} monedas de 1.00 {currency} y {halfUnits} monedas de cincuenta {fraction}. Añade el valor de todas las monedas.",
            FirstHint = $"Cada {symbol}1.00 son 100 {fraction}. Suma: ({wholeUnits * 100}) + ({halfUnits * 50}).",
            SecondHintDetailed = $"{wholeUnits * 100} {fraction} más {halfUnits * 50} {fraction} es igual a {totalSubUnits} {fraction} ({symbol}{decimalAmount:F2}).",
            ExplanationOfSuccess = $"¡Excelente! Tienes {totalSubUnits} {fraction}, que se escribe matemáticamente como {symbol}{decimalAmount:F2} {currency}. ¡Los decimales son las partes de una unidad entera!"
        };

        for (int i = 1; i <= wholeUnits; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"coin_d_{i}",
                Name = $"Moneda de {symbol}1.00 #{i}",
                EmojiSymbol = "🪙",
                AccessibleDescription = $"Moneda de {symbol}1.00 (100 {fraction})",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[0]
            });
        }

        for (int i = 1; i <= halfUnits; i++)
        {
            ex.ItemsGroupB.Add(new ManipulableItem
            {
                Id = $"coin_c_{i}",
                Name = $"Moneda de {symbol}0.50 #{i}",
                EmojiSymbol = "🥈",
                AccessibleDescription = $"Moneda de 50 {fraction} (0.50 {currency})",
                OrderIndex = wholeUnits + i,
                MusicalPitchHz = PentatonicScaleHz[2]
            });
        }

        BuildOptions(ex, totalSubUnits, min: Math.Max(50, totalSubUnits - 100), max: totalSubUnits + 100, unitLabel: fraction, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de primeras potencias y crecimiento exponencial: 2^n (5º Grado).
    /// </summary>
    public Exercise GeneratePowersExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int exponent = isSimplified ? rng.Next(1, 3) : rng.Next(2, 4);
        int powerResult = (int)Math.Pow(2, exponent);

        var formulaStr = exponent switch
        {
            1 => "2",
            2 => "2 × 2",
            3 => "2 × 2 × 2",
            _ => "2 × 2 × 2 × 2"
        };

        var ex = new Exercise
        {
            Id = $"gen_pow_{Guid.NewGuid():N}",
            Title = $"El Cofre Duplicador: 2 con exponente {exponent} (2^{exponent})",
            GameType = GamePlayType.PowersAndExponentGrowth,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = powerResult,
            StandardInstruction = $"En el cofre de gemas mágicas, cada nivel duplica los cristales. Para 2 elevado a la {exponent} (2^{exponent}), multiplicamos la base (2) por sí misma {exponent} veces: {formulaStr}. ¿Cuántos cristales se forman?",
            SimplifiedInstruction = $"El cofre duplica cristales: 2 con potencia {exponent} es {formulaStr}. ¿Cuánto es?",
            ScreenReaderDescription = $"Potencia 2 elevado a la {exponent}. Multiplicación repetida: {formulaStr}.",
            FirstHint = $"Calcula la multiplicación: {formulaStr}.",
            SecondHintDetailed = $"{formulaStr} = {powerResult}.",
            ExplanationOfSuccess = $"¡Increíble! 2^{exponent} = {powerResult}. ¡Las potencias representan multiplicación repetida y crecimiento acelerado!"
        };

        for (int i = 1; i <= powerResult; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"gem_pow_{i}",
                Name = $"Cristal de Poder #{i}",
                EmojiSymbol = "💎",
                AccessibleDescription = $"Cristal número {i} duplicado",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[(i - 1) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, powerResult, min: Math.Max(2, powerResult - 4), max: powerResult + 4, unitLabel: "cristales", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de proporcionalidad directa y regla de tres (6º Grado).
    /// </summary>
    public Exercise GenerateDirectProportionExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int ratio = isSimplified ? 2 : rng.Next(2, 5);
        int knownUnits = isSimplified ? 1 : rng.Next(1, 3);
        int knownResult = knownUnits * ratio;
        int targetUnits = isSimplified ? 3 : rng.Next(3, 6);
        int expected = targetUnits * ratio;

        var ex = new Exercise
        {
            Id = $"gen_prop_{Guid.NewGuid():N}",
            Title = "La Poción Mágica: Proporcionalidad Directa",
            GameType = GamePlayType.DirectProportion,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = expected,
            StandardInstruction = $"Para preparar pociones mágicas, sabemos que con {knownUnits} flor(es) se preparan {knownResult} frascos. Manteniendo la misma proporción, ¿cuántos frascos se pueden preparar con {targetUnits} flores?",
            SimplifiedInstruction = $"Si {knownUnits} flor hace {knownResult} frascos, ¿cuántos frascos harás con {targetUnits} flores?",
            ScreenReaderDescription = $"Proporción directa: {knownUnits} flores equivalen a {knownResult} frascos. Razón: {ratio} frascos por cada flor. Flores disponibles: {targetUnits}.",
            FirstHint = $"Averigua cuántos frascos da 1 sola flor: {knownResult} ÷ {knownUnits} = {ratio}. Luego multiplica por {targetUnits}.",
            SecondHintDetailed = $"{targetUnits} flores × {ratio} frascos por flor = {expected} frascos.",
            ExplanationOfSuccess = $"¡Exacto! Hay una proporción constante de {ratio} a 1. Con {targetUnits} flores preparas exactamente {expected} frascos."
        };

        for (int i = 1; i <= targetUnits; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"prop_flower_{i}",
                Name = $"Flor Mágica #{i}",
                EmojiSymbol = "🌸",
                AccessibleDescription = $"Flor mágica número {i}",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[(i - 1) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, expected, min: Math.Max(2, expected - 6), max: expected + 6, unitLabel: "frascos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de pensamiento algebraico y balanza de incógnitas: x + a = b (6º Grado).
    /// </summary>
    public Exercise GenerateAlgebraicBalanceExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int a = isSimplified ? 2 : rng.Next(2, 6);
        int x = isSimplified ? 3 : rng.Next(2, 7);
        int b = x + a;

        var ex = new Exercise
        {
            Id = $"gen_alg_{Guid.NewGuid():N}",
            Title = $"La Balanza Equilibrada: X + {a} = {b}",
            GameType = GamePlayType.AlgebraicBalance,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = x,
            StandardInstruction = $"En la balanza mágica, el platillo izquierdo tiene una caja secreta con valor X y {a} gemas. El platillo derecho tiene {b} gemas. Si ambos platillos están perfectamente equilibrados (X + {a} = {b}), ¿cuántas gemas hay dentro de la caja X?",
            SimplifiedInstruction = $"Una caja con X más {a} gemas pesa igual a {b} gemas. ¿Cuánto vale X?",
            ScreenReaderDescription = $"Balanza equilibrada. Lado izquierdo: caja X más {a} gemas. Lado derecho: {b} gemas. Ecuación: X + {a} = {b}.",
            FirstHint = $"Para saber cuánto pesa solo la caja X, quita las {a} gemas de ambos platillos.",
            SecondHintDetailed = $"X = {b} - {a} = {x}.",
            ExplanationOfSuccess = $"¡Magnífico! Despejaste la incógnita con maestría: X = {b} - {a} = {x}. Si pones {x} dentro de la caja, ambos lados pesan {b}."
        };

        for (int i = 1; i <= a; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"gem_a_{i}",
                Name = $"Gema en platillo izquierdo #{i}",
                EmojiSymbol = "💎",
                AccessibleDescription = $"Gema visible {i} junto a la caja X",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[0]
            });
        }

        for (int i = 1; i <= b; i++)
        {
            ex.ItemsGroupB.Add(new ManipulableItem
            {
                Id = $"gem_b_{i}",
                Name = $"Gema en platillo derecho #{i}",
                EmojiSymbol = "✨",
                AccessibleDescription = $"Gema del platillo derecho número {i}",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[2]
            });
        }

        BuildOptions(ex, x, min: Math.Max(1, x - 3), max: x + 3, unitLabel: "gemas", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de raíz cuadrada de cuadrados perfectos (6º Grado).
    /// </summary>
    public Exercise GenerateSquareRootExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        var roots = isSimplified ? new[] { 2, 3 } : new[] { 2, 3, 4, 5 };
        int root = roots[rng.Next(roots.Length)];
        int squareArea = root * root;

        var ex = new Exercise
        {
            Id = $"gen_sqrt_{Guid.NewGuid():N}",
            Title = $"El Jardín Cuadrado: √{squareArea}",
            GameType = GamePlayType.SquareRootPerfect,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = root,
            StandardInstruction = $"El jardín mágico de Cosmo tiene forma de cuadrado perfecto y está formado por {squareArea} baldosas en total. Como tiene el mismo número de filas que de columnas, ¿cuántas baldosas mide cada lado del jardín? (Calcula la raíz cuadrada: √{squareArea})",
            SimplifiedInstruction = $"Un piso cuadrado tiene {squareArea} baldosas en total. ¿Cuántas baldosas tiene de ancho y de largo?",
            ScreenReaderDescription = $"Cuadrado perfecto de {squareArea} losas. Observa cómo se organizan en filas y columnas iguales para hallar la raíz cuadrada.",
            FirstHint = $"Busca un número que multiplicado por sí mismo dé {squareArea}: ¿qué número por sí mismo da {squareArea}?",
            SecondHintDetailed = $"{root} × {root} = {squareArea}, por lo tanto √{squareArea} = {root}.",
            ExplanationOfSuccess = $"¡Espectacular! √{squareArea} = {root}, porque {root} × {root} = {squareArea}. La raíz cuadrada es la operación inversa de elevar al cuadrado."
        };

        for (int i = 1; i <= squareArea; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"tile_{i}",
                Name = $"Baldosa #{i}",
                EmojiSymbol = "🟩",
                AccessibleDescription = $"Baldosa número {i} de la cuadrícula de {squareArea}",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[(i - 1) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, root, min: Math.Max(1, root - 2), max: root + 3, unitLabel: "baldosas por lado", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de coordenadas cartesianas en el primer cuadrante (6º Grado).
    /// </summary>
    public Exercise GenerateCartesianCoordinatesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int xCoord = isSimplified ? rng.Next(1, 4) : rng.Next(1, 6);
        int yCoord = isSimplified ? rng.Next(1, 4) : rng.Next(1, 6);

        bool askX = (rng.Next(2) == 0);
        int expected = askX ? xCoord : yCoord;
        string axisName = askX ? "X (horizontal, este)" : "Y (vertical, norte)";

        var ex = new Exercise
        {
            Id = $"gen_coord_{Guid.NewGuid():N}",
            Title = $"El Mapa Pirata: Coordenadas ({xCoord}, {yCoord})",
            GameType = GamePlayType.CartesianCoordinates,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = expected,
            StandardInstruction = $"El cofre del tesoro está marcado en el mapa en el punto ({xCoord}, {yCoord}). Recuerda que el primer número siempre indica el eje X horizontal (este) y el segundo el eje Y vertical (norte). ¿Cuál es la coordenada sobre el eje {axisName}?",
            SimplifiedInstruction = $"El punto es ({xCoord}, {yCoord}). El primer número es X y el segundo es Y. ¿Cuánto vale {(askX ? "X" : "Y")}?",
            ScreenReaderDescription = $"Plano cartesiano con un punto marcado en ({xCoord}, {yCoord}). Localiza su posición sobre el eje {axisName}.",
            FirstHint = "En el par ordenado (X, Y), el primer valor es X y el segundo es Y.",
            SecondHintDetailed = $"Para ({xCoord}, {yCoord}), la coordenada en el eje {axisName} es {expected}.",
            ExplanationOfSuccess = $"¡Navegante experto! En el punto ({xCoord}, {yCoord}), el valor sobre el eje {axisName} es {expected}."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "point_target",
            Name = $"Tesoro en ({xCoord}, {yCoord})",
            EmojiSymbol = "📍",
            AccessibleDescription = $"Marcador en coordenada X={xCoord}, Y={yCoord}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        BuildOptions(ex, expected, min: 1, max: 5, unitLabel: "pasos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de números enteros con signo (1º Secundaria): deudas, saldo y recta numérica.
    /// </summary>
    public Exercise GenerateSignedNumbersExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int initialBalance = isSimplified ? 4 : rng.Next(3, 8);
        int debt = initialBalance + (isSimplified ? 2 : rng.Next(2, 6));
        int finalBalance = initialBalance - debt;
        string currency = profile.CurrencyName;
        string symbol = profile.CurrencySymbol;
        string store = profile.StoreName;

        var ex = new Exercise
        {
            Id = $"gen_signed_{Guid.NewGuid():N}",
            Title = $"La Libreta en {store}: Saldo con Números Enteros",
            GameType = GamePlayType.SignedNumbersAddition,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = finalBalance,
            StandardInstruction = $"En el/la {store} de la esquina tenías {symbol}{initialBalance} {currency} a favor (+{initialBalance}). Compraste mercancía por {symbol}{debt} {currency} que quedaste a deber (-{debt}). Al registrar (+{initialBalance}) + (-{debt}), ¿cuál es tu saldo final?",
            SimplifiedInstruction = $"Tenías {initialBalance} {currency} y gastaste {debt} {currency}. Quedaste debiendo. ¿Cuál es tu saldo final?",
            ScreenReaderDescription = $"Operación con números enteros con signo: positivo {initialBalance} más negativo {debt}. Sigue la flecha sobre la recta numérica para hallar el saldo.",
            FirstHint = $"Como la deuda ({debt}) es mayor que tu saldo inicial ({initialBalance}), tu resultado será un número negativo.",
            SecondHintDetailed = $"(+{initialBalance}) - {debt} = {finalBalance}. Tienes una deuda de {Math.Abs(finalBalance)} {currency}.",
            ExplanationOfSuccess = $"¡Excelente! (+{initialBalance}) + (-{debt}) = {finalBalance}. Un saldo de {finalBalance} significa que debes {Math.Abs(finalBalance)} {currency} en el/la {store}."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_balance",
            Name = $"Saldo inicial (+{initialBalance})",
            EmojiSymbol = "🪙",
            AccessibleDescription = $"Saldo a favor de {initialBalance} {currency}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        ex.ItemsGroupB.Add(new ManipulableItem
        {
            Id = "item_debt",
            Name = $"Deuda fiada (-{debt})",
            EmojiSymbol = "🧾",
            AccessibleDescription = $"Nota de deuda de {debt} {currency}",
            OrderIndex = 2,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        BuildOptions(ex, finalBalance, min: finalBalance - 3, max: finalBalance + 3, unitLabel: currency, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de jerarquía de operaciones y signos de agrupación (PEMDAS - 1º Secundaria).
    /// </summary>
    public Exercise GenerateOrderOfOperationsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int a = isSimplified ? 2 : rng.Next(2, 6);
        int b = isSimplified ? 3 : rng.Next(2, 5);
        int c = isSimplified ? 2 : rng.Next(2, 4);

        int multiplication = b * c;
        int correctResult = a + multiplication;
        int wrongPrecedence = (a + b) * c;

        var ex = new Exercise
        {
            Id = $"gen_pemdas_{Guid.NewGuid():N}",
            Title = $"La Jerarquía de Operaciones: {a} + {b} × {c}",
            GameType = GamePlayType.OrderOfOperationsPEMDAS,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = correctResult,
            StandardInstruction = $"Calcula el resultado de la expresión: {a} + {b} × {c}. Recuerda la regla de oro de la jerarquía de operaciones: las multiplicaciones siempre se resuelven antes que las sumas.",
            SimplifiedInstruction = $"En {a} + {b} × {c}, primero multiplica {b} × {c}. Luego suma {a}.",
            ScreenReaderDescription = $"Expresión aritmética: {a} más {b} por {c}. La multiplicación tiene prioridad sobre la suma.",
            FirstHint = $"Primero resuelve la multiplicación: {b} × {c} = {multiplication}. Luego suma {a}.",
            SecondHintDetailed = $"{a} + {multiplication} = {correctResult}.",
            ExplanationOfSuccess = $"¡Magnífico! Por jerarquía de operaciones, {b} × {c} = {multiplication}, y sumando {a} obtenemos {correctResult}. ¡Cuidado con sumar primero ({a}+{b}={a+b}) que daría {wrongPrecedence}!"
        };

        for (int i = 1; i <= c; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_pemdas_{i}",
                Name = $"Grupo de multiplicación #{i}",
                EmojiSymbol = "⭐",
                AccessibleDescription = $"Grupo {i} de {b} elementos",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[(i - 1) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, correctResult, min: Math.Min(correctResult, wrongPrecedence) - 2, max: Math.Max(correctResult, wrongPrecedence) + 2, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de ecuación lineal de la forma ax + b = c (1º Secundaria).
    /// </summary>
    public Exercise GenerateLinearEquationAXBExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int a = isSimplified ? 2 : rng.Next(2, 4);
        int x = isSimplified ? 3 : rng.Next(2, 6);
        int b = isSimplified ? 4 : rng.Next(2, 8);
        int c = (a * x) + b;
        string currency = profile.CurrencyName;
        string symbol = profile.CurrencySymbol;

        var ex = new Exercise
        {
            Id = $"gen_eq_axb_{Guid.NewGuid():N}",
            Title = $"Ecuación Lineal: {a}X + {b} = {c}",
            GameType = GamePlayType.LinearEquationAXB,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = x,
            StandardInstruction = $"En la papelería compraste {a} cuadernos iguales que costaron X {currency} cada uno, más una goma de {symbol}{b} {currency}. La cuenta total fue de {symbol}{c} {currency} ({a}X + {b} = {c}). Despejando paso a paso, ¿cuánto costó cada cuaderno X?",
            SimplifiedInstruction = $"{a} libretas valen {a}X. Más {b} {currency} da {c} en total. ¿Cuánto vale cada libreta X?",
            ScreenReaderDescription = $"Ecuación lineal: {a} por X más {b} igual a {c}. Encuentra el valor de X en {currency}.",
            FirstHint = $"Paso 1: Resta {b} de ambos lados: {a}X = {c} - {b} = {c - b}.",
            SecondHintDetailed = $"Paso 2: Divide entre {a}: X = {c - b} ÷ {a} = {x} {currency}.",
            ExplanationOfSuccess = $"¡Espectacular! {a}X = {c - b}, luego X = {c - b} ÷ {a} = {x}. Cada libreta costó {symbol}{x} {currency}."
        };

        for (int i = 1; i <= a; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_nb_{i}",
                Name = $"Libreta #{i} (costo X)",
                EmojiSymbol = "📓",
                AccessibleDescription = $"Libreta {i} de costo incógnita X",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[1]
            });
        }

        BuildOptions(ex, x, min: Math.Max(1, x - 2), max: x + 3, unitLabel: currency, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de porcentajes aplicados a compras y descuentos (1º Secundaria).
    /// </summary>
    public Exercise GeneratePercentageExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int basePrice = isSimplified ? 100 : (rng.Next(1, 4) * 100);
        int[] allowedPercents = isSimplified ? new[] { 10, 50 } : new[] { 10, 20, 25, 50 };
        int percent = allowedPercents[rng.Next(allowedPercents.Length)];

        int discountAmount = (basePrice * percent) / 100;
        int finalPrice = basePrice - discountAmount;
        string currency = profile.CurrencyName;
        string symbol = profile.CurrencySymbol;

        var ex = new Exercise
        {
            Id = $"gen_pct_{Guid.NewGuid():N}",
            Title = $"Oferta en la Tienda: {percent}% de Descuento",
            GameType = GamePlayType.PercentageCalculation,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = discountAmount,
            StandardInstruction = $"En la tienda de ropa una mochila cuesta {symbol}{basePrice} {currency} y tiene un letrero de oferta con el {percent}% de descuento. ¿Cuánto dinero te van a descontar del precio original?",
            SimplifiedInstruction = $"Una mochila de {symbol}{basePrice} {currency} tiene {percent}% de descuento. ¿Cuánto dinero te descuentan?",
            ScreenReaderDescription = $"Cálculo de porcentaje: {percent} por ciento de {basePrice} {currency}. Descuento en {currency}.",
            FirstHint = $"El {percent}% significa {percent} de cada 100 {currency}. Multiplica {basePrice} × {percent} y divide entre 100.",
            SecondHintDetailed = $"({basePrice} × {percent}) ÷ 100 = {discountAmount} {currency}.",
            ExplanationOfSuccess = $"¡Exacto! El {percent}% de {symbol}{basePrice} {currency} son {symbol}{discountAmount} {currency} de descuento. ¡Pagarás solo {symbol}{finalPrice} {currency}!"
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_backpack",
            Name = $"Mochila de {symbol}{basePrice}",
            EmojiSymbol = "🎒",
            AccessibleDescription = $"Mochila de {basePrice} {currency} con etiqueta de {percent}%",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        BuildOptions(ex, discountAmount, min: Math.Max(5, discountAmount - 15), max: discountAmount + 20, unitLabel: currency, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de leyes de los exponentes: producto de potencias de la misma base a^m * a^n = a^(m+n) (2º Secundaria).
    /// </summary>
    public Exercise GenerateExponentLawsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int baseNum = isSimplified ? 2 : rng.Next(2, 4);
        int m = isSimplified ? 1 : rng.Next(2, 4);
        int n = isSimplified ? 2 : rng.Next(2, 4);
        int expectedExponent = m + n;

        var ex = new Exercise
        {
            Id = $"gen_exp_law_{Guid.NewGuid():N}",
            Title = $"Leyes de Exponentes: {baseNum}^{m} × {baseNum}^{n}",
            GameType = GamePlayType.ExponentLaws,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = expectedExponent,
            StandardInstruction = $"Aplica la ley del producto de potencias con la misma base: {baseNum}^{m} × {baseNum}^{n}. La regla nos dice que los exponentes se suman (a^m × a^n = a^(m+n)). ¿Cuál es el exponente final de {baseNum}?",
            SimplifiedInstruction = $"En {baseNum}^{m} × {baseNum}^{n}, los exponentes se suman: {m} + {n}. ¿Cuál es el exponente?",
            ScreenReaderDescription = $"Multiplicación de potencias con base {baseNum}: exponente {m} por exponente {n}. Suma de exponentes.",
            FirstHint = $"La base se conserva ({baseNum}) y los exponentes se suman: {m} + {n}.",
            SecondHintDetailed = $"{m} + {n} = {expectedExponent}. La expresión simplificada es {baseNum}^{expectedExponent}.",
            ExplanationOfSuccess = $"¡Magnífico! {baseNum}^{m} × {baseNum}^{n} = {baseNum}^{expectedExponent}. Al multiplicar potencias de la misma base, se mantiene la base y se suman los exponentes."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "exp_m",
            Name = $"Potencia {baseNum}^{m}",
            EmojiSymbol = "💎",
            AccessibleDescription = $"Base {baseNum} con exponente {m}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        ex.ItemsGroupB.Add(new ManipulableItem
        {
            Id = "exp_n",
            Name = $"Potencia {baseNum}^{n}",
            EmojiSymbol = "✨",
            AccessibleDescription = $"Base {baseNum} con exponente {n}",
            OrderIndex = 2,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        BuildOptions(ex, expectedExponent, min: Math.Max(2, expectedExponent - 2), max: expectedExponent + 3, unitLabel: "(exponente)", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de reducción de términos semejantes en polinomios (2º Secundaria).
    /// </summary>
    public Exercise GeneratePolynomialLikeTermsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int a = isSimplified ? 2 : rng.Next(2, 6);
        int c = isSimplified ? 3 : rng.Next(2, 6);
        int expectedX = a + c;

        int b = isSimplified ? 1 : rng.Next(2, 5);
        int d = isSimplified ? 2 : rng.Next(2, 5);
        int totalY = b + d;

        var ex = new Exercise
        {
            Id = $"gen_poly_{Guid.NewGuid():N}",
            Title = $"Reducción de Términos: {a}x + {b}y + {c}x + {d}y",
            GameType = GamePlayType.PolynomialLikeTerms,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = expectedX,
            StandardInstruction = $"En el almacén escolar clasifican materiales: {a}x libretas + {b}y lápices + {c}x libretas + {d}y lápices. Reduciendo términos semejantes, ¿cuál es el coeficiente total de libretas x?",
            SimplifiedInstruction = $"Junta las x con las x: tienes {a}x y le sumas {c}x. ¿Cuántas x tienes en total?",
            ScreenReaderDescription = $"Expresión algebraica: {a}x más {b}y más {c}x más {d}y. Suma los coeficientes del término semejante x.",
            FirstHint = $"Solo puedes sumar términos con la misma letra: suma los números de x ({a} + {c}).",
            SecondHintDetailed = $"{a}x + {c}x = {expectedX}x. El coeficiente de x es {expectedX}.",
            ExplanationOfSuccess = $"¡Exacto! {a}x + {c}x = {expectedX}x (y {b}y + {d}y = {totalY}y). Los términos semejantes solo se suman entre familias con la misma letra."
        };

        for (int i = 1; i <= a; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_x1_{i}",
                Name = $"Libreta x #{i}",
                EmojiSymbol = "📘",
                AccessibleDescription = "Primer grupo de libreta x",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[1]
            });
        }
        for (int i = 1; i <= c; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_x2_{i}",
                Name = $"Libreta x #{a + i}",
                EmojiSymbol = "📘",
                AccessibleDescription = "Segundo grupo de libreta x",
                OrderIndex = a + i,
                MusicalPitchHz = PentatonicScaleHz[1]
            });
        }

        BuildOptions(ex, expectedX, min: Math.Max(3, expectedX - 3), max: expectedX + 3, unitLabel: "x", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de sistema de ecuaciones lineales 2x2 en la taquería (2º Secundaria).
    /// </summary>
    public Exercise GenerateLinearSystem2x2Exercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int tacoPrice = isSimplified ? 12 : rng.Next(12, 19);
        int drinkPrice = isSimplified ? 15 : rng.Next(14, 22);
        int p2 = tacoPrice + drinkPrice;
        int p1 = (2 * tacoPrice) + drinkPrice;
        string currency = profile.CurrencyName;
        string symbol = profile.CurrencySymbol;

        var ex = new Exercise
        {
            Id = $"gen_sys2x2_{Guid.NewGuid():N}",
            Title = "La Taquería: Sistema de Ecuaciones 2×2",
            GameType = GamePlayType.LinearSystem2x2,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = tacoPrice,
            StandardInstruction = $"En la taquería del barrio: por 2 tacos y 1 refresco pagas {symbol}{p1} {currency} (2x + y = {p1}). Por 1 taco y 1 refresco pagas {symbol}{p2} {currency} (x + y = {p2}). Comparando ambas cuentas, ¿cuánto cuesta exactamente 1 taco (x)?",
            SimplifiedInstruction = $"2 tacos y 1 refresco cuestan {symbol}{p1}. 1 taco y 1 refresco cuestan {symbol}{p2}. La diferencia es el precio de 1 taco. ¿Cuánto cuesta?",
            ScreenReaderDescription = $"Sistema 2x2: ecuación uno es 2x + y = {p1}. Ecuación dos es x + y = {p2}. Restando ambas ecuaciones se obtiene el precio de x en {currency}.",
            FirstHint = $"Observa que la única diferencia entre las dos órdenes es 1 taco adicional. Resta: {symbol}{p1} - {symbol}{p2}.",
            SecondHintDetailed = $"{p1} - {p2} = {tacoPrice} {currency}. Por tanto, 1 taco cuesta {symbol}{tacoPrice} {currency}.",
            ExplanationOfSuccess = $"¡Excelente deducción algebraica! Al restar las dos ecuaciones eliminamos el refresco (y - y = 0) y obtenemos: x = {p1} - {p2} = {symbol}{tacoPrice} {currency} por taco."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_taco",
            Name = "Orden de tacos (x)",
            EmojiSymbol = "🌮",
            AccessibleDescription = "Taco de precio incógnita x",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        ex.ItemsGroupB.Add(new ManipulableItem
        {
            Id = "item_soda",
            Name = "Refresco (y)",
            EmojiSymbol = "🥤",
            AccessibleDescription = "Refresco de precio incógnita y",
            OrderIndex = 2,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        BuildOptions(ex, tacoPrice, min: Math.Max(5, tacoPrice - 4), max: tacoPrice + 5, unitLabel: currency, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio del Teorema de Pitágoras con ternas pitagóricas en rampas accesibles (2º Secundaria).
    /// </summary>
    public Exercise GeneratePythagoreanTheoremExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        var triplets = isSimplified
            ? new (int a, int b, int c)[] { (3, 4, 5) }
            : new (int a, int b, int c)[] { (3, 4, 5), (6, 8, 10), (5, 12, 13) };

        var t = triplets[rng.Next(triplets.Length)];
        int expectedHypotenuse = t.c;
        int a2 = t.a * t.a;
        int b2 = t.b * t.b;
        int sumSquares = a2 + b2;

        var ex = new Exercise
        {
            Id = $"gen_pyth_{Guid.NewGuid():N}",
            Title = "La Rampa Accesible: Teorema de Pitágoras",
            GameType = GamePlayType.PythagoreanTheorem,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = expectedHypotenuse,
            StandardInstruction = $"En la escuela diseñamos una rampa accesible para personas en silla de ruedas. La rampa forma un triángulo rectángulo con una base de {t.b} metros y una altura de {t.a} metros. Aplicando el Teorema de Pitágoras (c² = a² + b² = {a2} + {b2} = {sumSquares}), ¿cuántos metros de largo mide la rampa inclinada c?",
            SimplifiedInstruction = $"Una rampa tiene base de {t.b} metros y altura de {t.a} metros. Si c² = {a2} + {b2} = {sumSquares}, ¿cuánto mide la rampa (√{sumSquares})?",
            ScreenReaderDescription = $"Triángulo rectángulo: cateto altura {t.a} metros, cateto base {t.b} metros. Hipotenusa c al cuadrado es {sumSquares}. Raíz cuadrada de {sumSquares}.",
            FirstHint = $"Suma los cuadrados de los catetos: {a2} + {b2} = {sumSquares}. Luego saca la raíz cuadrada de {sumSquares}.",
            SecondHintDetailed = $"c = √{sumSquares} = {expectedHypotenuse} metros.",
            ExplanationOfSuccess = $"¡Brillante! Por el Teorema de Pitágoras: c = √({t.a}² + {t.b}²) = √({a2} + {b2}) = √{sumSquares} = {expectedHypotenuse} metros. ¡La rampa cumple perfectamente las medidas arquitectónicas!"
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "ramp_slope",
            Name = $"Rampa Inclinada ({expectedHypotenuse}m)",
            EmojiSymbol = "📐",
            AccessibleDescription = $"Rampa accesible de hipotenusa {expectedHypotenuse} metros",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[3]
        });

        BuildOptions(ex, expectedHypotenuse, min: Math.Max(3, expectedHypotenuse - 3), max: expectedHypotenuse + 3, unitLabel: "metros", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de resolución de ecuaciones cuadráticas (3º Secundaria).
    /// Forma pura x² - d = 0 (o x² = k) con raíces enteras positivas.
    /// </summary>
    public Exercise GenerateQuadraticEquationExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        // Raíz x elegida entre 3 y 9 (o 4 para simplificado)
        int xRoot = isSimplified ? 4 : rng.Next(3, 10);
        int constantD = xRoot * xRoot;

        var ex = new Exercise
        {
            Id = $"gen_quad_{Guid.NewGuid():N}",
            Title = $"El Terreno Cuadrado: x² - {constantD} = 0",
            GameType = GamePlayType.QuadraticEquations,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = xRoot,
            StandardInstruction = $"En el ejido del pueblo se delimita un huerto cuadrado de área x² m². Si la ecuación del terreno es x² - {constantD} = 0 (es decir, x² = {constantD}), ¿cuántos metros mide cada lado positivo x del huerto (√{constantD})?",
            SimplifiedInstruction = $"Si un huerto cuadrado tiene x² = {constantD}, ¿qué número multiplicado por sí mismo da {constantD}?",
            ScreenReaderDescription = $"Ecuación cuadrática: x cuadrada menos {constantD} igual a cero. Se despeja x al cuadrado igual a {constantD}, luego raíz cuadrada de {constantD}.",
            FirstHint = $"Despeja sumando {constantD} a ambos lados: x² = {constantD}. Luego calcula la raíz cuadrada de {constantD}.",
            SecondHintDetailed = $"x = √{constantD} = {xRoot} metros de lado.",
            ExplanationOfSuccess = $"¡Excelente resolución cuadrática! x² = {constantD} implica x = ±√{constantD}. Tomando la dimensión geométrica positiva, x = {xRoot} metros."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "garden_square",
            Name = $"Huerto Cuadrado ({constantD} m²)",
            EmojiSymbol = "🟩",
            AccessibleDescription = $"Huerto de área {constantD} metros cuadrados",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[1]
        });

        BuildOptions(ex, xRoot, min: Math.Max(2, xRoot - 3), max: xRoot + 3, unitLabel: "metros", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio del Teorema de Tales y semejanza de triángulos con sombras (3º Secundaria).
    /// Relación: h_arbol / s_arbol = h_persona / s_persona
    /// </summary>
    public Exercise GenerateTriangleSimilarityThalesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        // Estudiante de altura 1 o 2 metros, sombra proporcional
        // ratio: factor de sombra (ej. sombra = altura * 2)
        int personHeight = 2; // metros
        int personShadow = isSimplified ? 3 : rng.Next(2, 5); // metros
        int factorTree = isSimplified ? 3 : rng.Next(2, 5); // arbol es factorTree veces más alto
        int treeHeight = personHeight * factorTree;
        int treeShadow = personShadow * factorTree;

        var ex = new Exercise
        {
            Id = $"gen_thales_{Guid.NewGuid():N}",
            Title = "La Sombra del Ahuehuete: Teorema de Tales",
            GameType = GamePlayType.TriangleSimilarityThales,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = treeHeight,
            StandardInstruction = $"En la plaza del pueblo, a las 3 de la tarde, una persona de {personHeight} metros proyecta una sombra de {personShadow} metros. Al mismo instante, un gran ahuehuete proyecta una sombra de {treeShadow} metros. Por semejanza de triángulos (Teorema de Tales), ¿cuántos metros de altura mide el árbol?",
            SimplifiedInstruction = $"Si una persona de {personHeight}m da una sombra de {personShadow}m, y el árbol da {treeShadow}m de sombra ({factorTree} veces más), ¿cuánto mide el árbol?",
            ScreenReaderDescription = $"Teorema de Tales: persona de {personHeight}m proyecta sombra de {personShadow}m; árbol proyecta sombra de {treeShadow}m. Razón de semejanza {factorTree}. Altura del árbol.",
            FirstHint = $"Halla la razón de semejanza entre las sombras: {treeShadow} ÷ {personShadow} = {factorTree}. Multiplica la altura de la persona por esa razón.",
            SecondHintDetailed = $"Altura del árbol = {personHeight} × {factorTree} = {treeHeight} metros.",
            ExplanationOfSuccess = $"¡Magnífica aplicación de Tales! Los rayos solares forman triángulos semejantes: {treeHeight} / {treeShadow} = {personHeight} / {personShadow} = {personHeight.ToString()}/{personShadow.ToString()}. El ahuehuete mide {treeHeight} metros."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "tree_item",
            Name = $"Ahuehuete ({treeHeight}m)",
            EmojiSymbol = "🌳",
            AccessibleDescription = $"Árbol ahuehuete de altura incógnita proyectando {treeShadow} metros de sombra",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        BuildOptions(ex, treeHeight, min: Math.Max(3, treeHeight - 4), max: treeHeight + 4, unitLabel: "metros", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de razones trigonométricas básicas (3º Secundaria).
    /// Enfoque: Identificar el cateto opuesto, cateto adyacente o la razón exacta tan(θ) = op/ady.
    /// Para mantener resultados enteros accesibles, se evalúa el numerador del cateto opuesto o la tangente entera/razón directa.
    /// </summary>
    public Exercise GenerateTrigonometricRatiosExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        // Triángulo rectángulo clásico: opuesto, adyacente, hipotenusa (3, 4, 5) o (6, 8, 10)
        int multiplier = isSimplified ? 1 : rng.Next(1, 4);
        int op = 3 * multiplier;
        int ady = 4 * multiplier;
        int hip = 5 * multiplier;

        // Preguntar por el valor del cateto opuesto sabiendo que sen(θ) = op / hip o tan(θ) = op / ady
        int expectedValue = op;

        var ex = new Exercise
        {
            Id = $"gen_trig_{Guid.NewGuid():N}",
            Title = "El Radar y el Ángulo: Razones Trigonométricas",
            GameType = GamePlayType.TrigonometricRatios,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = expectedValue,
            StandardInstruction = $"En un triángulo rectángulo para orientar una antena parabólica, la hipotenusa mide {hip} metros y el cateto adyacente mide {ady} metros. Sabiendo que el seno del ángulo es cateto opuesto entre hipotenusa (sen θ = op/{hip}) y que la terna pitagórica cumple op² + {ady}² = {hip}², ¿cuántos metros mide el cateto opuesto?",
            SimplifiedInstruction = $"En un triángulo de hipotenusa {hip} y lado adyacente {ady}, ¿cuánto mide el cateto opuesto (la altura)?",
            ScreenReaderDescription = $"Razones trigonométricas en triángulo rectángulo: hipotenusa {hip} metros, cateto adyacente {ady} metros. Cateto opuesto.",
            FirstHint = $"Usa la relación trigonométrica y pitagórica: op² = {hip}² - {ady}² = {hip * hip} - {ady * ady}.",
            SecondHintDetailed = $"op² = {hip * hip - ady * ady}, entonces op = √{hip * hip - ady * ady} = {op} metros.",
            ExplanationOfSuccess = $"¡Exacto! El cateto opuesto mide {op} metros. Así, sen(θ) = {op}/{hip} y tan(θ) = {op}/{ady}."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "antenna_trig",
            Name = "Antena orientada",
            EmojiSymbol = "📡",
            AccessibleDescription = $"Triángulo trigonométrico con hipotenusa {hip}m",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        BuildOptions(ex, expectedValue, min: Math.Max(2, expectedValue - 3), max: expectedValue + 3, unitLabel: "metros", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de funciones cuadráticas y parábolas y = ax² (3º Secundaria).
    /// Evaluar el valor de y para una abscisa x dada (ej. y = a · x²).
    /// </summary>
    public Exercise GenerateQuadraticFunctionCurveExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int a = isSimplified ? 2 : rng.Next(2, 4);
        int x = isSimplified ? 3 : rng.Next(2, 5);
        int expectedY = a * (x * x);

        var ex = new Exercise
        {
            Id = $"gen_quad_func_{Guid.NewGuid():N}",
            Title = $"La Parábola del Chorro de Agua: y = {a}x²",
            GameType = GamePlayType.QuadraticFunctionCurve,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = expectedY,
            StandardInstruction = $"En la fuente del parque, el chorro de agua describe una curva parabólica que sigue la función cuadrática y = {a}x². Si avanzamos horizontalmente x = {x} metros desde el origen, ¿cuál es la altura y alcanzada por el agua en ese punto?",
            SimplifiedInstruction = $"Calcula y = {a} × x² cuando x = {x}. Primero eleva {x} al cuadrado ({x} × {x}) y luego multiplica por {a}.",
            ScreenReaderDescription = $"Función cuadrática y = {a}x al cuadrado. Evaluar en x = {x}. {x} al cuadrado es {x * x}, multiplicado por {a}.",
            FirstHint = $"Primero calcula x²: {x} × {x} = {x * x}. Luego multiplica el resultado por {a}.",
            SecondHintDetailed = $"y = {a} × ({x * x}) = {expectedY} metros.",
            ExplanationOfSuccess = $"¡Brillante cálculo parabólico! Cuando x = {x}, x² = {x * x}, y multiplicando por {a} obtenemos y = {expectedY}. Las parábolas modelan trayectorias curvas en la física y la arquitectura."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "fountain_parabola",
            Name = "Chorro Parabólico",
            EmojiSymbol = "⛲",
            AccessibleDescription = $"Fuente parabólica con trayectoria y = {a}x²",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[4]
        });

        BuildOptions(ex, expectedY, min: Math.Max(4, expectedY - 6), max: expectedY + 6, unitLabel: "metros", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de distancia entre dos puntos en el plano cartesiano (1º Preparatoria).
    /// d = √((x2 - x1)² + (y2 - y1)²). Se utilizan ternas enteras (ej. 3-4-5, 6-8-10).
    /// </summary>
    public Exercise GenerateAnalyticDistanceExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        // Generamos un segmento con catetos dx y dy que formen terna pitagórica exacta
        var triplets = isSimplified
            ? new (int dx, int dy, int d)[] { (3, 4, 5) }
            : new (int dx, int dy, int d)[] { (3, 4, 5), (6, 8, 10), (5, 12, 13) };

        var t = triplets[rng.Next(triplets.Length)];
        int x1 = rng.Next(1, 5);
        int y1 = rng.Next(1, 5);
        int x2 = x1 + t.dx;
        int y2 = y1 + t.dy;
        int expectedDistance = t.d;

        int dx2 = t.dx * t.dx;
        int dy2 = t.dy * t.dy;
        int sumSq = dx2 + dy2;

        var ex = new Exercise
        {
            Id = $"gen_dist2d_{Guid.NewGuid():N}",
            Title = $"La Red de Fibra Óptica: Distancia entre Puntos",
            GameType = GamePlayType.AnalyticDistance2D,
            TargetAgeMin = 14,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolFirstGrade,
            ExpectedResult = expectedDistance,
            StandardInstruction = $"En la ciudad se conectan dos antenas de telecomunicación ubicadas en los puntos A({x1}, {y1}) y B({x2}, {y2}). Aplicando la fórmula de distancia en el plano cartesiano d = √((x₂ - x₁)² + (y₂ - y₁)²), donde dx = {t.dx} y dy = {t.dy} (d² = {dx2} + {dy2} = {sumSq}), ¿cuántos kilómetros de cable de fibra óptica se requieren (√{sumSq})?",
            SimplifiedInstruction = $"Calcula la distancia entre el punto A({x1}, {y1}) y el punto B({x2}, {y2}). Si d² = {dx2} + {dy2} = {sumSq}, ¿cuánto mide la distancia directa (√{sumSq})?",
            ScreenReaderDescription = $"Geometría analítica: distancia entre A({x1}, {y1}) y B({x2}, {y2}). Diferencia en x es {t.dx}, diferencia en y es {t.dy}. Suma de cuadrados es {sumSq}. Raíz cuadrada de {sumSq}.",
            FirstHint = $"Resta las coordenadas para hallar los lados del triángulo rectángulo: dx = {x2} - {x1} = {t.dx}, dy = {y2} - {y1} = {t.dy}.",
            SecondHintDetailed = $"d = √({t.dx}² + {t.dy}²) = √({dx2} + {dy2}) = √{sumSq} = {expectedDistance} kilómetros.",
            ExplanationOfSuccess = $"¡Excelente cálculo analítico! d = √(({x2} - {x1})² + ({y2} - {y1})²) = √({sumSq}) = {expectedDistance} km. ¡La distancia euclidiana es el núcleo de los motores de gráficos y GPS!"
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "point_a",
            Name = $"Antena A({x1}, {y1})",
            EmojiSymbol = "📍",
            AccessibleDescription = $"Punto A en coordenadas ({x1}, {y1})",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        ex.ItemsGroupB.Add(new ManipulableItem
        {
            Id = "point_b",
            Name = $"Antena B({x2}, {y2})",
            EmojiSymbol = "📡",
            AccessibleDescription = $"Punto B en coordenadas ({x2}, {y2})",
            OrderIndex = 2,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        BuildOptions(ex, expectedDistance, min: Math.Max(3, expectedDistance - 3), max: expectedDistance + 3, unitLabel: "km", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de cálculo de pendiente de la recta m = (y2 - y1) / (x2 - x1) (1º Preparatoria).
    /// </summary>
    public Exercise GenerateLineSlopeExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int m = isSimplified ? 2 : rng.Next(2, 5); // pendiente entera positiva
        int x1 = rng.Next(1, 4);
        int dx = rng.Next(1, 4);
        int x2 = x1 + dx;
        int dy = m * dx;
        int y1 = rng.Next(1, 5);
        int y2 = y1 + dy;

        var ex = new Exercise
        {
            Id = $"gen_slope_{Guid.NewGuid():N}",
            Title = $"La Pendiente del Viaducto: m = Δy / Δx",
            GameType = GamePlayType.LineSlopeM,
            TargetAgeMin = 14,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolFirstGrade,
            ExpectedResult = m,
            StandardInstruction = $"En la construcción de un viaducto urbano, la vía pasa por los puntos A({x1}, {y1}) y B({x2}, {y2}). Calculando la razón de cambio o pendiente de inclinación m = (y₂ - y₁) / (x₂ - x₁) = ({y2} - {y1}) / ({x2} - {x1}) = {dy} / {dx}, ¿cuál es el valor de la pendiente m?",
            SimplifiedInstruction = $"Halla la pendiente: resta las alturas ({y2} - {y1} = {dy}) y divide entre la distancia horizontal ({x2} - {x1} = {dx}). ¿Cuánto da {dy} ÷ {dx}?",
            ScreenReaderDescription = $"Pendiente analítica m: punto uno ({x1}, {y1}), punto dos ({x2}, {y2}). Incremento en y es {dy}, incremento en x es {dx}. Pendiente igual a {dy} entre {dx}.",
            FirstHint = $"Calcula el cambio vertical Δy = {y2} - {y1} = {dy} y el horizontal Δx = {x2} - {x1} = {dx}.",
            SecondHintDetailed = $"m = Δy / Δx = {dy} / {dx} = {m}.",
            ExplanationOfSuccess = $"¡Brillante! La pendiente de la recta es m = {m}. La pendiente representa la inclinación y razón de cambio instantáneo que da inicio al cálculo."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "slope_road",
            Name = "Viaducto Inclinado",
            EmojiSymbol = "🌉",
            AccessibleDescription = $"Viaducto con pendiente m = {m}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[1]
        });

        BuildOptions(ex, m, min: 1, max: m + 3, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de la ecuación de la recta pendiente-ordenada al origen: y = mx + b (1º Preparatoria).
    /// Evaluar y para un valor de x dado, o determinar la ordenada al origen b.
    /// </summary>
    public Exercise GenerateLinearEquationYMXBExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int m = isSimplified ? 3 : rng.Next(2, 5);
        int b = isSimplified ? 5 : rng.Next(2, 8);
        int x = isSimplified ? 4 : rng.Next(2, 6);
        int expectedY = (m * x) + b;

        string currency = profile.CurrencyName;
        string symbol = profile.CurrencySymbol;

        var ex = new Exercise
        {
            Id = $"gen_ymxb_{Guid.NewGuid():N}",
            Title = $"Tarifa de Transporte: y = {m}x + {b}",
            GameType = GamePlayType.LinearEquationYMXB,
            TargetAgeMin = 14,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolFirstGrade,
            ExpectedResult = expectedY,
            StandardInstruction = $"En el servicio de mototaxi de la comunidad, la tarifa en {currency} sigue la recta lineal y = mx + b, donde m = {symbol}{m} {currency} por kilómetro (pendiente), b = {symbol}{b} {currency} de banderazo inicial (ordenada al origen), y x son los kilómetros recorridos. Si viajas x = {x} kilómetros, ¿cuánto pagas en total y = {m}({x}) + {b}?",
            SimplifiedInstruction = $"Calcula y = {m} × {x} + {b}. Multiplica {m} por {x} y luego súmale {b}. ¿Cuánto pagas en total?",
            ScreenReaderDescription = $"Ecuación de la recta y = {m}x + {b}. Para x = {x} kilómetros: multiplica la pendiente por x y suma el banderazo en {currency}.",
            FirstHint = $"Multiplica la pendiente por los kilómetros: {m} × {x} = {m * x}. Luego suma el banderazo: + {b}.",
            SecondHintDetailed = $"y = ({m * x}) + {b} = {expectedY} {currency}.",
            ExplanationOfSuccess = $"¡Exacto! y = {m}({x}) + {b} = {m * x} + {b} = {symbol}{expectedY} {currency}. La recta y = mx + b modela costos fijos y variables en la economía cotidiana."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "taxi_item",
            Name = "Mototaxi",
            EmojiSymbol = "🛺",
            AccessibleDescription = $"Mototaxi con tarifa y = {m}x + {b}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[3]
        });

        BuildOptions(ex, expectedY, min: Math.Max(5, expectedY - 5), max: expectedY + 5, unitLabel: currency, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de ecuación de la circunferencia x² + y² = r² y radio de proximidad (2º Preparatoria).
    /// </summary>
    public Exercise GenerateConicCircleExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int radius = isSimplified ? 5 : rng.Next(3, 9);
        int rSquared = radius * radius;

        var ex = new Exercise
        {
            Id = $"gen_circle_{Guid.NewGuid():N}",
            Title = $"El Radar Circular: x² + y² = {rSquared}",
            GameType = GamePlayType.ConicCircleRadius,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = radius,
            StandardInstruction = $"El sensor de un robot móvil detecta obstáculos en una zona circular modelada por la ecuación x² + y² = {rSquared}. Sabiendo que la ecuación canónica de la circunferencia con centro en el origen es x² + y² = r², ¿cuántos metros mide el radio de detección r = √{rSquared}?",
            SimplifiedInstruction = $"Si un círculo cumple x² + y² = {rSquared}, ¿cuánto mide su radio? ¿Qué número por sí mismo da {rSquared}?",
            ScreenReaderDescription = $"Ecuación canónica de la circunferencia: x al cuadrado más y al cuadrado igual a {rSquared}. Radio al cuadrado es {rSquared}. Raíz cuadrada de {rSquared}.",
            FirstHint = $"Compara con la ecuación canónica: r² = {rSquared}. Para hallar r, calcula la raíz cuadrada de {rSquared}.",
            SecondHintDetailed = $"r = √{rSquared} = {radius} metros.",
            ExplanationOfSuccess = $"¡Brillante! El radio de detección es r = {radius} metros (r² = {rSquared}). Las ecuaciones circulares se usan en robótica y videojuegos para delimitar zonas de interacción."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "radar_circle_item",
            Name = $"Círculo de Radio {radius}m",
            EmojiSymbol = "🎯",
            AccessibleDescription = $"Zona circular de radio {radius} metros",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[0]
        });

        BuildOptions(ex, radius, min: Math.Max(2, radius - 3), max: radius + 3, unitLabel: "metros", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de ondas senoidales y armónicas continuas y = A · sen(x) (2º Preparatoria).
    /// Evaluar la amplitud pico A de una onda sonora o de voltaje.
    /// </summary>
    public Exercise GenerateSineWaveContinuousExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int amplitude = isSimplified ? 4 : rng.Next(3, 8); // amplitud A
        int peakValue = amplitude; // sen(90°) = 1 => y_max = A

        var ex = new Exercise
        {
            Id = $"gen_sinewave_{Guid.NewGuid():N}",
            Title = $"La Onda Sonora: y = {amplitude} · sen(x)",
            GameType = GamePlayType.SineWaveContinuous,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = peakValue,
            StandardInstruction = $"En un sintetizador musical accesible, una nota pura genera una onda senoidal de voltaje dada por la función y = {amplitude} · sen(x). Como el valor máximo de la función seno es 1 (sen 90° = 1), ¿cuál es la amplitud máxima o cresta alcanzada por la onda sonora ({amplitude} × 1)?",
            SimplifiedInstruction = $"En la onda y = {amplitude} × sen(x), el seno máximo vale 1. ¿Cuál es la altura máxima alcanzada ({amplitude} × 1)?",
            ScreenReaderDescription = $"Función trigonométrica continua y = {amplitude} por seno de x. Amplitud máxima en el pico donde sen(x) es 1. Calcula {amplitude} multiplicado por 1.",
            FirstHint = $"El valor máximo que puede tener la función sen(x) es 1. Multiplica {amplitude} por 1.",
            SecondHintDetailed = $"Altura máxima = {amplitude} × 1 = {amplitude} voltios / unidades.",
            ExplanationOfSuccess = $"¡Espectacular! La amplitud de la onda es A = {amplitude}. Las funciones senoidales son el alma del audio digital, la síntesis de voz y las señales de radio."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "wave_oscillator",
            Name = "Onda Sonora",
            EmojiSymbol = "〰️",
            AccessibleDescription = $"Onda armónica continua con amplitud {amplitude}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        BuildOptions(ex, peakValue, min: Math.Max(2, peakValue - 3), max: peakValue + 3, unitLabel: "unidades", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de logaritmos en base 2 log₂(n) y complejidad computacional (2º Preparatoria).
    /// </summary>
    public Exercise GenerateLogarithmBinaryExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        // Potencias de 2: 2^3=8, 2^4=16, 2^5=32, 2^6=64, 2^7=128
        int exponent = isSimplified ? 4 : rng.Next(3, 7);
        int numberN = 1 << exponent; // 2^exponent

        var ex = new Exercise
        {
            Id = $"gen_log2_{Guid.NewGuid():N}",
            Title = $"Búsqueda Binaria: log₂({numberN})",
            GameType = GamePlayType.LogarithmBinary,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = exponent,
            StandardInstruction = $"En un diccionario digital de {numberN} palabras ordenadas alfabéticamente, un algoritmo de búsqueda binaria divide la lista a la mitad en cada paso. El número máximo de comparaciones requeridas es log₂({numberN}). Sabiendo que 2 elevado a k es igual a {numberN} (2^k = {numberN}), ¿cuántos pasos k toma encontrar cualquier palabra?",
            SimplifiedInstruction = $"Si vas partiendo a la mitad {numberN} cosas, ¿cuántas veces tienes que multiplicar el 2 (2 × 2 × 2...) para llegar a {numberN}?",
            ScreenReaderDescription = $"Logaritmo en base 2 de {numberN}. Exponente k tal que 2 a la k es igual a {numberN}. Resultado es {exponent} pasos.",
            FirstHint = $"Calcula las potencias de 2: 2¹=2, 2²=4, 2³=8, 2⁴=16, etc. ¿A qué exponente corresponde {numberN}?",
            SecondHintDetailed = $"2^{exponent} = {numberN}, por lo tanto log₂({numberN}) = {exponent}.",
            ExplanationOfSuccess = $"¡Extraordinario! log₂({numberN}) = {exponent}. ¡Un algoritmo binario solo necesita {exponent} comparaciones entre {numberN} elementos! Es el fundamento de la eficiencia en programación."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "binary_tree_item",
            Name = $"Árbol Binario ({numberN} nodos)",
            EmojiSymbol = "🌲",
            AccessibleDescription = $"Estructura binaria de {numberN} elementos",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[4]
        });

        BuildOptions(ex, exponent, min: Math.Max(2, exponent - 2), max: exponent + 3, unitLabel: "pasos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Cálculo Diferencial: derivada y razón de cambio instantánea f'(t) (3º Preparatoria).
    /// Si la posición es f(t) = a·t², su derivada (velocidad instantánea) es f'(t) = 2a·t.
    /// </summary>
    public Exercise GenerateCalculusDerivativeExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int a = isSimplified ? 3 : rng.Next(2, 6);
        int t = isSimplified ? 2 : rng.Next(2, 5);
        int coeffDeriv = 2 * a;
        int velocity = coeffDeriv * t;

        string coeffText = a == 1 ? "" : a.ToString();

        var ex = new Exercise
        {
            Id = $"gen_deriv_{Guid.NewGuid():N}",
            Title = $"Velocidad Instantánea: Derivada de f(t) = {coeffText}t²",
            GameType = GamePlayType.CalculusDerivativeRate,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = velocity,
            StandardInstruction = $"Un vehículo autónomo eléctrico acelera siguiendo la función de posición f(t) = {coeffText}t² metros. Aplicando la regla de la potencia del cálculo diferencial, la velocidad instantánea es la derivada f'(t) = 2·({a})t = {coeffDeriv}t. ¿Cuál es su velocidad instantánea en m/s exactamente en el segundo t = {t}?",
            SimplifiedInstruction = $"La velocidad instantánea es f'(t) = {coeffDeriv} × t. Si el tiempo es t = {t} segundos, ¿cuánto da {coeffDeriv} × {t}?",
            ScreenReaderDescription = $"Cálculo diferencial: función de posición f de t igual a {a} por t al cuadrado. Su derivada f prima de t es {coeffDeriv} por t. Evaluada en t igual a {t} segundos.",
            FirstHint = $"Para derivar {a}t², baja el exponente 2 multiplicando a {a}: obtienes {coeffDeriv}t. Ahora sustituye t = {t}.",
            SecondHintDetailed = $"f'({t}) = {coeffDeriv} × {t} = {velocity} m/s.",
            ExplanationOfSuccess = $"¡Magistral! La derivada evaluada en t = {t} es f'({t}) = {velocity} m/s. En programación e inteligencia artificial, la derivada (gradiente) indica hacia dónde ajustar los pesos de una red neuronal."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "deriv_speedometer",
            Name = $"Velocímetro f'({t}) = {velocity} m/s",
            EmojiSymbol = "🏎️",
            AccessibleDescription = $"Vehículo autónomo con derivada de posición f'(t) = {coeffDeriv}t",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[5]
        });

        BuildOptions(ex, velocity, min: Math.Max(4, velocity - 4), max: velocity + 4, unitLabel: "m/s", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Cálculo Integral: área bajo la curva e integral definida ∫₀ᵇ v dt (3º Preparatoria).
    /// Acumulación de flujo o distancia recorrida con rapidez constante o escalonada.
    /// </summary>
    public Exercise GenerateCalculusIntegralExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int rate = isSimplified ? 4 : rng.Next(3, 8); // litros por minuto o m/s
        int timeB = isSimplified ? 3 : rng.Next(3, 7); // intervalo [0, b]
        int areaAccumulated = rate * timeB;

        var ex = new Exercise
        {
            Id = $"gen_integral_{Guid.NewGuid():N}",
            Title = $"Acumulación Total: Integral ∫₀^{timeB} {rate} dt",
            GameType = GamePlayType.CalculusIntegralArea,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = areaAccumulated,
            StandardInstruction = $"Una bomba solar vierte agua en una cisterna comunitaria a una razón constante f(t) = {rate} litros por minuto desde t = 0 hasta t = {timeB} minutos. En cálculo integral, el volumen total acumulado es el área bajo la curva dada por la integral definida ∫₀^{timeB} {rate} dt = [{rate}t]₀^{timeB}. ¿Cuántos litros se acumulan en total?",
            SimplifiedInstruction = $"Calcula el área bajo la gráfica: si entran {rate} litros cada minuto durante {timeB} minutos, ¿cuántos litros se juntan en total ({rate} × {timeB})?",
            ScreenReaderDescription = $"Cálculo integral: integral definida de 0 a {timeB} de la función constante {rate} diferencial de t. Área del rectángulo bajo la curva de altura {rate} y base {timeB}.",
            FirstHint = $"El área bajo la línea horizontal y = {rate} entre 0 y {timeB} forma un rectángulo de base {timeB} y altura {rate}.",
            SecondHintDetailed = $"∫₀^{timeB} {rate} dt = {rate} × {timeB} = {areaAccumulated} litros.",
            ExplanationOfSuccess = $"¡Extraordinario! La integral definida vale {areaAccumulated} litros. Integrar significa sumar infinitos pedacitos para conocer la acumulación total, como los motores físicos de videojuegos que integran la velocidad en cada fotograma."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "integral_tank",
            Name = $"Área Bajo la Curva ({areaAccumulated} L)",
            EmojiSymbol = "🔋",
            AccessibleDescription = $"Acumulador con área bajo la curva de {rate} por {timeB} igual a {areaAccumulated}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[6]
        });

        BuildOptions(ex, areaAccumulated, min: Math.Max(6, areaAccumulated - 5), max: areaAccumulated + 5, unitLabel: "litros", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Probabilidad y Estadística Descriptiva (media aritmética exacta) (3º Preparatoria).
    /// </summary>
    public Exercise GenerateProbabilityAndStatsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int mean = isSimplified ? 8 : rng.Next(6, 10);
        int delta = isSimplified ? 1 : rng.Next(1, 3);
        // Tres datos simétricos alrededor de la media: (mean - delta), mean, (mean + delta)
        int d1 = mean - delta;
        int d2 = mean;
        int d3 = mean + delta;
        int sum = d1 + d2 + d3;

        var ex = new Exercise
        {
            Id = $"gen_stats_{Guid.NewGuid():N}",
            Title = $"Ciencia de Datos: Media Aritmética x̄",
            GameType = GamePlayType.ProbabilityAndStats,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = mean,
            StandardInstruction = $"Al entrenar un modelo de reconocimiento de voz inclusivo, se registraron los puntajes de precisión en 3 pruebas: {d1}, {d2} y {d3} puntos. Para evaluar el rendimiento estadístico, calcula el promedio o media aritmética x̄ = ({d1} + {d2} + {d3}) / 3 = {sum} / 3. ¿Cuál es la media x̄?",
            SimplifiedInstruction = $"Suma los 3 puntajes ({d1} + {d2} + {d3} = {sum}) y divide el total entre 3 para hallar el promedio. ¿Cuánto es {sum} ÷ 3?",
            ScreenReaderDescription = $"Estadística descriptiva: conjunto de tres datos: {d1}, {d2} y {d3}. Suma total igual a {sum}. Media aritmética igual a {sum} dividido entre 3.",
            FirstHint = $"Suma primero todos los datos: {d1} + {d2} + {d3} = {sum}. Luego divide ese resultado entre 3.",
            SecondHintDetailed = $"x̄ = {sum} ÷ 3 = {mean} puntos.",
            ExplanationOfSuccess = $"¡Excelente! La media aritmética es x̄ = {mean} puntos. La estadística y la probabilidad son el corazón de la ciencia de datos y el aprendizaje automático moderno."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "stats_chart",
            Name = $"Muestra de Datos ({d1}, {d2}, {d3})",
            EmojiSymbol = "📊",
            AccessibleDescription = $"Conjunto de datos estadísticos con suma {sum} y promedio {mean}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[7]
        });

        BuildOptions(ex, mean, min: Math.Max(3, mean - 3), max: mean + 3, unitLabel: "puntos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de suma repetida y bucles de código (Semilla de Programación: REPETIR N VECES).
    /// </summary>
    public Exercise GeneratePatternAndLoopSeedExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int iterations = isSimplified ? rng.Next(2, 4) : rng.Next(2, 6);
        int stepsPerIteration = isSimplified ? rng.Next(2, 4) : rng.Next(2, 6);
        int totalSteps = iterations * stepsPerIteration;

        string sumExpression = string.Join(" + ", Enumerable.Repeat(stepsPerIteration.ToString(), iterations));

        var ex = new Exercise
        {
            Id = $"gen_loop_seed_{Guid.NewGuid():N}",
            Title = $"La Ranita Robot: REPETIR {iterations} VECES {{ +{stepsPerIteration} }}",
            GameType = GamePlayType.PatternAndLoopSeed,
            TargetAgeMin = 6,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = totalSteps,
            StandardInstruction = $"Nuestra ranita robot ejecuta el bucle de código: 'REPETIR {iterations} VECES {{ AVANZAR {stepsPerIteration} PASOS }}'. Esto equivale a la suma repetida {sumExpression} (o {iterations} × {stepsPerIteration}). ¿Cuántos pasos avanza la ranita al terminar el bucle?",
            SimplifiedInstruction = $"La ranita da {iterations} saltos de {stepsPerIteration} pasos cada uno ({sumExpression}). ¿Cuántos pasos avanza en total?",
            ScreenReaderDescription = $"Bucle de programación: repetir {iterations} veces avanzar {stepsPerIteration} pasos. Cuentas la suma repetida {sumExpression} en cada vuelta del ciclo.",
            FirstHint = $"Suma {stepsPerIteration} un total de {iterations} veces: {sumExpression}.",
            SecondHintDetailed = $"{iterations} veces {stepsPerIteration} ({iterations} × {stepsPerIteration}) es igual a {totalSteps} pasos.",
            ExplanationOfSuccess = $"¡Excelente pensamiento algorítmico! El bucle REPETIR {iterations} VECES {{ AVANZAR {stepsPerIteration} }} suma {sumExpression} = {totalSteps} pasos."
        };

        for (int i = 1; i <= iterations; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"loop_iter_{i}",
                Name = $"Iteración #{i} (+{stepsPerIteration})",
                EmojiSymbol = "🐸",
                AccessibleDescription = $"Iteración {i} del bucle: avanza {stepsPerIteration} pasos, acumulado {i * stepsPerIteration}",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[(i - 1) % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, totalSteps, min: Math.Max(2, totalSteps - 3), max: totalSteps + 4, unitLabel: "pasos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Variables, Asignación y Actualización de Estado en código (Objetivo 14 - Programación 1).
    /// </summary>
    public Exercise GenerateProgrammingVariablesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int initial = isSimplified ? rng.Next(5, 10) : rng.Next(10, 21);
        int deposit = isSimplified ? rng.Next(2, 6) : rng.Next(4, 10);
        int expense = isSimplified ? rng.Next(1, 4) : rng.Next(2, 7);
        int afterDeposit = initial + deposit;
        int finalValue = afterDeposit - expense;

        string currency = profile.CurrencyName;
        string store = profile.StoreName;

        var ex = new Exercise
        {
            Id = $"gen_prog_var_{Guid.NewGuid():N}",
            Title = $"Cajas de Memoria: Variable 'saldo' en la {store}",
            GameType = GamePlayType.ProgrammingVariablesState,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = finalValue,
            StandardInstruction = $"En una aplicación accesible para la {store}, creamos la variable 'saldo' en {currency} y ejecutamos estas 3 líneas de código en orden:\n1) saldo = {initial}\n2) saldo = saldo + {deposit}\n3) saldo = saldo - {expense}\n¿Qué valor numérico queda guardado en la variable 'saldo' al terminar la línea 3?",
            SimplifiedInstruction = $"La caja llamada 'saldo' empieza con {initial}. Primero le sumamos {deposit} ({initial} + {deposit} = {afterDeposit}) y luego le restamos {expense}. ¿Cuánto queda en 'saldo'?",
            ScreenReaderDescription = $"Programa de tres líneas con la variable saldo: línea 1, saldo igual a {initial}. Línea 2, saldo igual a saldo más {deposit}. Línea 3, saldo igual a saldo menos {expense}.",
            FirstHint = $"Sigue el estado línea por línea: en la línea 2, 'saldo' pasa de {initial} a {initial} + {deposit} = {afterDeposit}.",
            SecondHintDetailed = $"En la línea 3 tomamos el valor actualizado ({afterDeposit}) y le restamos {expense}: {afterDeposit} - {expense} = {finalValue} {currency}.",
            ExplanationOfSuccess = $"¡Excelente programador! La variable 'saldo' comenzó en {initial}, subió a {afterDeposit} y terminó en {finalValue} {currency}. Una variable guarda y actualiza su estado paso a paso."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "var_box_state",
            Name = $"Variable saldo ({initial} → {afterDeposit} → {finalValue})",
            EmojiSymbol = "📦",
            AccessibleDescription = $"Caja de memoria de la variable saldo: inicia en {initial}, suma {deposit} dando {afterDeposit} y resta {expense} dando {finalValue}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[2]
        });

        BuildOptions(ex, finalValue, min: Math.Max(1, finalValue - 4), max: finalValue + 4, unitLabel: currency, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Condicionales SI / SI NO (if / else) (Objetivo 14 - Programación 1).
    /// </summary>
    public Exercise GenerateProgrammingConditionalsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int threshold = 10;
        bool conditionMet = rng.Next(0, 2) == 0;
        int basePoints = conditionMet
            ? (isSimplified ? 12 : rng.Next(10, 16))
            : (isSimplified ? 6 : rng.Next(5, 10));

        int highBonus = isSimplified ? 5 : rng.Next(4, 8);
        int lowBonus = isSimplified ? 2 : rng.Next(1, 4);
        int appliedBonus = (basePoints >= threshold) ? highBonus : lowBonus;
        int finalTotal = basePoints + appliedBonus;

        string branchTaken = conditionMet ? "SI (verdadera)" : "SI NO (falsa)";

        var ex = new Exercise
        {
            Id = $"gen_prog_if_{Guid.NewGuid():N}",
            Title = $"Decisiones del Código: SI puntos >= {threshold} ENTONCES...",
            GameType = GamePlayType.ProgrammingConditionalsIfElse,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = finalTotal,
            StandardInstruction = $"El lector de pantalla inteligente evalúa el avance del alumno con este condicional:\n• puntos = {basePoints}\n• SI (puntos >= {threshold}) ENTONCES {{ bono = {highBonus} }} SI NO {{ bono = {lowBonus} }}\n• total = puntos + bono\n¿Cuál es el valor final de la variable 'total'?",
            SimplifiedInstruction = $"Tienes puntos = {basePoints}. Si tus puntos son {threshold} o más, ganas un bono de {highBonus}; si son menos de {threshold}, ganas {lowBonus}. ¿Cuánto es puntos + bono?",
            ScreenReaderDescription = $"Condicional if else: variable puntos igual a {basePoints}. Si puntos es mayor o igual a {threshold}, bono vale {highBonus}; si no, bono vale {lowBonus}. Calcular total igual a puntos más bono.",
            FirstHint = $"Primero compara: ¿{basePoints} es mayor o igual a {threshold}? Como la condición es {(conditionMet ? "verdadera" : "falsa")}, el programa entra a la rama {branchTaken} y asigna bono = {appliedBonus}.",
            SecondHintDetailed = $"Con puntos = {basePoints} y bono = {appliedBonus}, sumamos total = {basePoints} + {appliedBonus} = {finalTotal} puntos.",
            ExplanationOfSuccess = $"¡Lógica impecable! Como {basePoints} {(conditionMet ? ">=" : "<")} {threshold}, el condicional eligió bono = {appliedBonus} y calculó total = {basePoints} + {appliedBonus} = {finalTotal}."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "cond_gate_item",
            Name = $"Condicional ({basePoints} >= {threshold} → +{appliedBonus})",
            EmojiSymbol = "🚦",
            AccessibleDescription = $"Semáforo condicional: puntos {basePoints} comparado con {threshold} asigna bono {appliedBonus}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[4]
        });

        BuildOptions(ex, finalTotal, min: Math.Max(3, finalTotal - 4), max: finalTotal + 4, unitLabel: "puntos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Lógica Booleana y Compuertas Lógicas (AND / Y, OR / O) (Objetivo 14 - Programación 1).
    /// </summary>
    public Exercise GenerateProgrammingBooleanLogicExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int baseEnergy = isSimplified ? 10 : rng.Next(10, 16);
        int turboBoost = isSimplified ? 5 : rng.Next(6, 10);
        int safeBoost = 2;

        bool bothTrue = rng.Next(0, 2) == 0;
        int sensorBoton = 1;
        int sensorRampa = bothTrue ? 1 : 0;

        int addedBoost = (sensorBoton == 1 && sensorRampa == 1) ? turboBoost : safeBoost;
        int totalEnergy = baseEnergy + addedBoost;

        var ex = new Exercise
        {
            Id = $"gen_prog_bool_{Guid.NewGuid():N}",
            Title = "Compuertas Lógicas del Robot: Operador Y (AND)",
            GameType = GamePlayType.ProgrammingBooleanLogic,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = totalEnergy,
            StandardInstruction = $"Una rampa robótica accesible tiene energía base = {baseEnergy} y evalúa dos sensores con el operador lógico Y (AND), donde 1 es Activo y 0 es Inactivo:\n• sensor_boton = {sensorBoton}, sensor_rampa = {sensorRampa}\n• SI (sensor_boton == 1 Y sensor_rampa == 1) ENTONCES {{ energia = {baseEnergy} + {turboBoost} }} SI NO {{ energia = {baseEnergy} + {safeBoost} }}\nRecuerda que 'A Y B' solo es verdadero si AMBOS valen 1. ¿Cuál es la energía final?",
            SimplifiedInstruction = $"Para sumar el impulso grande (+{turboBoost}), los dos sensores deben valer 1. Aquí sensor_boton = {sensorBoton} y sensor_rampa = {sensorRampa}. Si ambos son 1 suma {baseEnergy} + {turboBoost}; si no, suma {baseEnergy} + {safeBoost}.",
            ScreenReaderDescription = $"Compuerta lógica AND: sensor botón vale {sensorBoton} y sensor rampa vale {sensorRampa}. Si ambos valen 1, energía es {baseEnergy} más {turboBoost}; si no, es {baseEnergy} más {safeBoost}.",
            FirstHint = $"En la compuerta lógica Y (AND), ambos sensores deben ser 1. Aquí tenemos ({sensorBoton} Y {sensorRampa}), por lo que la condición es {(bothTrue ? "VERDADERA" : "FALSA")}.",
            SecondHintDetailed = $"Como la condición Y es {(bothTrue ? "verdadera" : "falsa")}, sumamos {baseEnergy} + {addedBoost} = {totalEnergy} unidades de energía.",
            ExplanationOfSuccess = $"¡Dominio total de la lógica de Boole! Con sensor_boton = {sensorBoton} y sensor_rampa = {sensorRampa}, la compuerta Y (AND) dio {(bothTrue ? "Verdadero" : "Falso")} y la energía fue {baseEnergy} + {addedBoost} = {totalEnergy}."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "bool_gate_item",
            Name = $"Compuerta Y ({sensorBoton} AND {sensorRampa} → +{addedBoost})",
            EmojiSymbol = "🔌",
            AccessibleDescription = $"Compuerta booleana AND con entradas {sensorBoton} y {sensorRampa}, sumando {addedBoost} a {baseEnergy}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[5]
        });

        BuildOptions(ex, totalEnergy, min: Math.Max(5, totalEnergy - 4), max: totalEnergy + 4, unitLabel: "unidades", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Ciclos PARA / MIENTRAS (for / while) y Variables Acumuladoras (Objetivo 15 - Programación 2).
    /// </summary>
    public Exercise GenerateProgrammingLoopsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int initial = isSimplified ? rng.Next(2, 6) : rng.Next(3, 9);
        int iterations = isSimplified ? rng.Next(2, 4) : rng.Next(3, 6);
        int stepValue = isSimplified ? rng.Next(2, 4) : rng.Next(2, 6);
        int addedByLoop = iterations * stepValue;
        int finalTotal = initial + addedByLoop;

        var ex = new Exercise
        {
            Id = $"gen_prog_loop_{Guid.NewGuid():N}",
            Title = $"Ciclo PARA (for): Acumulando en {iterations} Iteraciones",
            GameType = GamePlayType.ProgrammingLoopsForWhile,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = finalTotal,
            StandardInstruction = $"Un sintetizador de voz accesible ejecuta el siguiente ciclo PARA (for) con una variable acumuladora:\n1) palabras = {initial}\n2) PARA i = 1 HASTA {iterations} HACER {{ palabras = palabras + {stepValue} }}\nEn cada una de las {iterations} vueltas del ciclo, 'palabras' aumenta en {stepValue}. ¿Cuál es el valor final de 'palabras' al terminar el ciclo?",
            SimplifiedInstruction = $"La variable 'palabras' empieza en {initial}. El ciclo repite {iterations} veces la orden de sumar +{stepValue} ({iterations} × {stepValue} = {addedByLoop}). ¿Cuánto es {initial} + {addedByLoop}?",
            ScreenReaderDescription = $"Ciclo for: variable palabras inicia en {initial}. Para i desde 1 hasta {iterations}, palabras igual a palabras más {stepValue}. Calcular el valor final de palabras.",
            FirstHint = $"El ciclo se repite {iterations} veces y en cada vuelta suma {stepValue}, acumulando {iterations} × {stepValue} = {addedByLoop} palabras nuevas.",
            SecondHintDetailed = $"Suma el valor inicial ({initial}) más lo acumulado en el ciclo ({addedByLoop}): {initial} + {addedByLoop} = {finalTotal} palabras.",
            ExplanationOfSuccess = $"¡Excelente dominio de los ciclos! El ciclo PARA ejecutó {iterations} iteraciones sumando {stepValue} cada vez ({addedByLoop} en total), pasando de {initial} a {finalTotal} palabras."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "loop_accumulator_item",
            Name = $"Ciclo for ({initial} + {iterations}×{stepValue} = {finalTotal})",
            EmojiSymbol = "🔄",
            AccessibleDescription = $"Ciclo repetitivo for de {iterations} vueltas sumando {stepValue} al valor inicial {initial} para dar {finalTotal}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[3]
        });

        BuildOptions(ex, finalTotal, min: Math.Max(4, finalTotal - 4), max: finalTotal + 4, unitLabel: "palabras", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Arreglos (Arrays), Listas e Índices desde Cero [0, 1, 2...] (Objetivo 15 - Programación 2).
    /// </summary>
    public Exercise GenerateProgrammingArraysExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);
        string currency = profile.CurrencyName;
        string store = profile.StoreName;

        int v0 = isSimplified ? rng.Next(2, 5) : rng.Next(3, 7);
        int v1 = v0 + rng.Next(2, 4);
        int v2 = v1 + rng.Next(2, 4);
        int v3 = v2 + rng.Next(2, 4);
        int[] arr = [v0, v1, v2, v3];

        int idxA = 0;
        int idxB = isSimplified ? 1 : rng.Next(1, 4);
        int expectedSum = arr[idxA] + arr[idxB];

        var ex = new Exercise
        {
            Id = $"gen_prog_arr_{Guid.NewGuid():N}",
            Title = $"Arreglos desde el Índice Cero: precios[{idxA}] + precios[{idxB}]",
            GameType = GamePlayType.ProgrammingArraysZeroIndex,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = expectedSum,
            StandardInstruction = $"En el catálogo digital de la {store}, los precios se guardan en un arreglo (Array) cuyas posiciones se numeran empezando desde el índice 0:\n• precios = [{v0}, {v1}, {v2}, {v3}]\n(Recuerda: precios[0] = {v0}, precios[1] = {v1}, precios[2] = {v2}, precios[3] = {v3}).\nSi el programa calcula total = precios[{idxA}] + precios[{idxB}], ¿cuántos {currency} vale 'total'?",
            SimplifiedInstruction = $"En la lista precios = [{v0}, {v1}, {v2}, {v3}], la posición [0] es el primer número ({v0}) y la posición [{idxB}] vale {arr[idxB]}. ¿Cuánto es {v0} + {arr[idxB]} {currency}?",
            ScreenReaderDescription = $"Arreglo precios con elementos {v0}, {v1}, {v2} y {v3} indexados desde cero. Calcular precios en índice {idxA} más precios en índice {idxB}.",
            FirstHint = $"En programación el primer casillero siempre es el índice [0]. Por eso precios[{idxA}] = {arr[idxA]} y precios[{idxB}] = {arr[idxB]}.",
            SecondHintDetailed = $"Suma los valores de ambas posiciones del arreglo: {arr[idxA]} + {arr[idxB]} = {expectedSum} {currency}.",
            ExplanationOfSuccess = $"¡Indexación perfecta! Identificaste que precios[{idxA}] = {arr[idxA]} y precios[{idxB}] = {arr[idxB]}, obteniendo {arr[idxA]} + {arr[idxB]} = {expectedSum} {currency}."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "array_zero_item",
            Name = $"Arreglo [{v0}, {v1}, {v2}, {v3}] (índices 0..3)",
            EmojiSymbol = "🗂️",
            AccessibleDescription = $"Arreglo indexado desde cero: índice 0 vale {v0}, índice 1 vale {v1}, índice 2 vale {v2}, índice 3 vale {v3}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[6]
        });

        BuildOptions(ex, expectedSum, min: Math.Max(3, expectedSum - 4), max: expectedSum + 4, unitLabel: currency, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Funciones Modulares, Parámetros y Valor de Retorno (return) (Objetivo 15 - Programación 2).
    /// </summary>
    public Exercise GenerateProgrammingFunctionsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int factor = isSimplified ? 2 : rng.Next(2, 5);
        int baseOffset = isSimplified ? rng.Next(1, 4) : rng.Next(2, 6);
        int inputParam = isSimplified ? rng.Next(2, 5) : rng.Next(3, 7);
        int returnValue = (factor * inputParam) + baseOffset;

        var ex = new Exercise
        {
            Id = $"gen_prog_func_{Guid.NewGuid():N}",
            Title = $"Funciones Modulares: calcularDecibeles({inputParam})",
            GameType = GamePlayType.ProgrammingFunctionsReturn,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = returnValue,
            StandardInstruction = $"Igual que en matemáticas evaluamos f(x) = {factor}x + {baseOffset}, en programación definimos una función reutilizable con parámetro de entrada 'nivel' y valor de retorno (return):\n• FUNCION calcularDecibeles(nivel) {{ RETORNAR ({factor} * nivel) + {baseOffset} }}\nSi nuestro programa llama a la función con calcularDecibeles({inputParam}), ¿qué valor devuelve (return)?",
            SimplifiedInstruction = $"La máquina de código 'calcularDecibeles' multiplica el número de entrada por {factor} y luego le suma {baseOffset}. Si entra el número {inputParam}, ¿cuánto es ({factor} × {inputParam}) + {baseOffset}?",
            ScreenReaderDescription = $"Función calcularDecibeles con parámetro nivel: retorna {factor} por nivel más {baseOffset}. Evaluar la llamada calcularDecibeles con argumento {inputParam}.",
            FirstHint = $"Sustituye el parámetro 'nivel' por {inputParam} dentro de la función: primero multiplica {factor} × {inputParam} = {factor * inputParam}.",
            SecondHintDetailed = $"Al producto {factor * inputParam} súmale la constante {baseOffset}: {factor * inputParam} + {baseOffset} = {returnValue} decibeles.",
            ExplanationOfSuccess = $"¡Gran diseño modular! Al invocar calcularDecibeles({inputParam}), la función evaluó ({factor} × {inputParam}) + {baseOffset} y retornó {returnValue} decibeles."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "func_return_item",
            Name = $"calcularDecibeles({inputParam}) → return {returnValue}",
            EmojiSymbol = "⚙️",
            AccessibleDescription = $"Función modular: recibe parámetro {inputParam}, calcula {factor} por {inputParam} más {baseOffset} y retorna {returnValue} decibeles",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[7]
        });

        BuildOptions(ex, returnValue, min: Math.Max(3, returnValue - 4), max: returnValue + 4, unitLabel: "decibeles", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Algoritmos de Búsqueda y Valor Máximo/Mínimo (Objetivo 16 - Programación 3).
    /// </summary>
    public Exercise GenerateProgrammingAlgorithmsSearchExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int a = isSimplified ? rng.Next(3, 6) : rng.Next(4, 9);
        int b = a + rng.Next(4, 8); // valor máximo en medio
        int c = a + rng.Next(1, 4);
        int d = isSimplified ? a + 1 : c - 1;
        int maxVal = Math.Max(Math.Max(a, b), Math.Max(c, d));

        var ex = new Exercise
        {
            Id = $"gen_prog_algo_{Guid.NewGuid():N}",
            Title = "Algoritmo de Búsqueda: Encontrando el Valor Máximo",
            GameType = GamePlayType.ProgrammingAlgorithmsSearch,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = maxVal,
            StandardInstruction = $"Un algoritmo es una receta lógica paso a paso. Nuestro lector accesible busca la señal más clara recorriendo la lista sensores = [{a}, {b}, {c}, {d}] con este algoritmo:\n1) maximo = sensores[0] (inicia en {a})\n2) PARA CADA valor EN [{a}, {b}, {c}, {d}]: SI (valor > maximo) ENTONCES {{ maximo = valor }}\n¿Qué valor queda guardado en 'maximo' al terminar de recorrer toda la lista?",
            SimplifiedInstruction = $"El algoritmo recorre la lista [{a}, {b}, {c}, {d}] comparando uno por uno para quedarse con el número más grande (el máximo). ¿Cuál es el número mayor de la lista?",
            ScreenReaderDescription = $"Algoritmo de búsqueda del máximo en la lista {a}, {b}, {c} y {d}. Inicia máximo en {a} y actualiza si encuentra un número mayor.",
            FirstHint = $"Sigue el algoritmo paso a paso: empieza con maximo = {a}. Al comparar con {b}, como {b} > {a}, actualiza maximo = {b}.",
            SecondHintDetailed = $"Luego compara {b} con {c} y con {d}; como ninguno supera a {b}, la variable 'maximo' conserva el valor {maxVal}.",
            ExplanationOfSuccess = $"¡Pensamiento algorítmico impecable! El algoritmo recorrió los 4 elementos, actualizó el máximo al encontrar {maxVal} y descartó los menores."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "algo_max_item",
            Name = $"Búsqueda en [{a}, {b}, {c}, {d}] → máx = {maxVal}",
            EmojiSymbol = "🔍",
            AccessibleDescription = $"Algoritmo de búsqueda secuencial sobre {a}, {b}, {c}, {d} que identifica el máximo {maxVal}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[5]
        });

        BuildOptions(ex, maxVal, min: Math.Max(2, maxVal - 4), max: maxVal + 4, unitLabel: "puntos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de Física y Coordenadas 2D en Videojuegos Accesibles (x = x + v·t) (Objetivo 16 - Programación 3).
    /// </summary>
    public Exercise GenerateProgrammingGameCoordinatesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int x0 = isSimplified ? rng.Next(2, 5) : rng.Next(3, 8);
        int velocity = isSimplified ? rng.Next(2, 4) : rng.Next(3, 6);
        int seconds = isSimplified ? rng.Next(2, 4) : rng.Next(3, 5);
        int displacement = velocity * seconds;
        int finalX = x0 + displacement;

        var ex = new Exercise
        {
            Id = $"gen_prog_game_{Guid.NewGuid():N}",
            Title = $"Motor de Videojuego Accesible: Coordenada x = {x0} + ({velocity} × {seconds})",
            GameType = GamePlayType.ProgrammingGameCoordinates2D,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = finalX,
            StandardInstruction = $"En un videojuego espacial con audio 3D accesible, la nave de Cosmo actualiza su coordenada horizontal X usando cinemática vectorial:\n• x_inicial = {x0}\n• velocidad_x = {velocity} casillas por segundo, tiempo = {seconds} segundos\n• x_final = x_inicial + (velocidad_x * tiempo)\n¿En qué coordenada X se escuchará la nave después de {seconds} segundos?",
            SimplifiedInstruction = $"La nave empieza en la posición X = {x0} y avanza {velocity} casillas cada segundo durante {seconds} segundos ({velocity} × {seconds} = {displacement}). ¿A qué posición llega ({x0} + {displacement})?",
            ScreenReaderDescription = $"Coordenadas de videojuego: x inicial igual a {x0}, velocidad {velocity} por tiempo {seconds}. Calcular x final igual a {x0} más {velocity} por {seconds}.",
            FirstHint = $"Calcula primero el desplazamiento multiplicando velocidad por tiempo: {velocity} × {seconds} = {displacement} casillas.",
            SecondHintDetailed = $"Suma el desplazamiento a la coordenada inicial: x_final = {x0} + {displacement} = {finalX}.",
            ExplanationOfSuccess = $"¡Gran programador de videojuegos! La nave avanzó {velocity} × {seconds} = {displacement} casillas desde X = {x0}, llegando a la coordenada X = {finalX}."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "game_coord_item",
            Name = $"Nave X: {x0} + ({velocity}×{seconds}) = {finalX}",
            EmojiSymbol = "🎮",
            AccessibleDescription = $"Motor de física 2D: posición inicial {x0} más velocidad {velocity} por {seconds} segundos igual a coordenada {finalX}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[6]
        });

        BuildOptions(ex, finalX, min: Math.Max(4, finalX - 4), max: finalX + 4, unitLabel: "en X", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de IA Asistiva, Promedio de Sensores y Clasificación con Datos (Objetivo 16 - Programación 3).
    /// </summary>
    public Exercise GenerateProgrammingAssistiveAIExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int mean = isSimplified ? rng.Next(4, 8) : rng.Next(6, 12);
        int delta = isSimplified ? 1 : rng.Next(2, 4);
        int s1 = mean - delta;
        int s2 = mean;
        int s3 = mean + delta;
        int sum = s1 + s2 + s3;
        int calibrationBoost = isSimplified ? 2 : rng.Next(3, 6);
        int finalSignal = mean + calibrationBoost;

        var ex = new Exercise
        {
            Id = $"gen_prog_ai_{Guid.NewGuid():N}",
            Title = "IA Asistiva: Filtrado de Sensores por Promedio",
            GameType = GamePlayType.ProgrammingAssistiveAIData,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = finalSignal,
            StandardInstruction = $"Un guante traductor de Lengua de Señas con Inteligencia Artificial lee 3 sensores de movimiento y filtra el ruido calculando su media aritmética antes de sumar la calibración:\n1) lecturas = [{s1}, {s2}, {s3}]\n2) promedio = ({s1} + {s2} + {s3}) / 3\n3) senal_limpia = promedio + {calibrationBoost}\n¿Cuál es el valor final de 'senal_limpia'?",
            SimplifiedInstruction = $"Primero calcula el promedio de los 3 sensores ({s1} + {s2} + {s3} = {sum}, dividido entre 3 da {mean}). Luego súmale {calibrationBoost} de calibración ({mean} + {calibrationBoost}). ¿Cuánto da?",
            ScreenReaderDescription = $"IA asistiva: lecturas de sensores {s1}, {s2} y {s3}. Promedio igual a la suma entre 3. Señal limpia igual a promedio más {calibrationBoost}.",
            FirstHint = $"Suma las 3 lecturas de los sensores y divide entre 3: ({s1} + {s2} + {s3}) ÷ 3 = {sum} ÷ 3 = {mean}.",
            SecondHintDetailed = $"Ahora suma el ajuste de calibración ({calibrationBoost}) al promedio ({mean}): {mean} + {calibrationBoost} = {finalSignal} puntos de señal.",
            ExplanationOfSuccess = $"¡Ingeniería inclusiva en acción! El filtro estadístico obtuvo el promedio {mean} y generó una señal limpia de {mean} + {calibrationBoost} = {finalSignal} para el traductor."
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "assistive_ai_item",
            Name = $"IA Sensores x̄={mean} + {calibrationBoost} = {finalSignal}",
            EmojiSymbol = "🤖",
            AccessibleDescription = $"Módulo de IA asistiva: promedia {s1}, {s2} y {s3} dando {mean} y suma calibración {calibrationBoost} para obtener {finalSignal}",
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[7]
        });

        BuildOptions(ex, finalSignal, min: Math.Max(4, finalSignal - 4), max: finalSignal + 4, unitLabel: "señal", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de formas geométricas y tamaños para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenShapesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int count = rng.Next(min, max + 1);

        var shapes = new[]
        {
            ("círculo", "círculos", "🔵", "Un círculo azul redondo", 262),
            ("cuadrado", "cuadrados", "🟦", "Un cuadrado azul de 4 lados", 330),
            ("triángulo", "triángulos", "🔺", "Un triángulo rojo de 3 lados", 392)
        };

        var target = shapes[rng.Next(shapes.Length)];

        var ex = new Exercise
        {
            Id = $"gen_kshape_{Guid.NewGuid():N}",
            Title = $"El Jardín de las Formas: {target.Item2}",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 3,
            TargetAgeMax = 4,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = count,
            StandardInstruction = $"En el jardín aparecieron formas geométricas. Toca solo los {target.Item2} para contarlos. ¿Cuántos hay en total?",
            SimplifiedInstruction = $"Toca solo los {target.Item2}. ¿Cuántos hay?",
            ScreenReaderDescription = $"Hay varias formas geométricas en el jardín. Explora y cuenta solo los {target.Item2}.",
            FirstHint = $"Busca la forma que es un {target.Item1} y tócala.",
            SecondHintDetailed = $"Cuenta despacio: uno, dos... el último número es la cantidad de {target.Item2}.",
            ExplanationOfSuccess = $"¡Excelente! Hay {count} {target.Item2} en el jardín.",
            SimplifiedFirstHint = $"Toca cada {target.Item1}.",
            SimplifiedSecondHint = $"Cuenta: uno, dos... di el último número.",
            SimplifiedExplanationOfSuccess = $"¡Muy bien! Hay {count} {target.Item2}."
        };

        for (int i = 0; i < count; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_shape_{i + 1}",
                Name = $"{target.Item1} {i + 1}",
                EmojiSymbol = target.Item3,
                AccessibleDescription = $"{target.Item4} número {i + 1}",
                OrderIndex = i + 1,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, count, min: 1, max: max + 2, unitLabel: target.Item2, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de posiciones espaciales (arriba/abajo, dentro/fuera) para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenPositionsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int count = rng.Next(min, max + 1);

        bool isUp = rng.Next(2) == 0;
        string positionName = isUp ? "arriba" : "abajo";
        string locationName = isUp ? "arriba en el árbol" : "abajo en el pasto";
        string itemNoun = isUp ? "pájaro" : "conejo";
        string itemPlural = isUp ? "pájaros" : "conejos";
        string emoji = isUp ? "🐦" : "🐰";

        var ex = new Exercise
        {
            Id = $"gen_kpos_{Guid.NewGuid():N}",
            Title = $"¿Dónde Está?: {itemPlural.Substring(0, 1).ToUpper() + itemPlural.Substring(1)} {positionName}",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 3,
            TargetAgeMax = 4,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = count,
            StandardInstruction = $"Los {itemPlural} están jugando. Cuenta únicamente los que están {locationName}. ¿Cuántos hay?",
            SimplifiedInstruction = $"Cuenta los {itemPlural} que están {positionName}. ¿Cuántos hay?",
            ScreenReaderDescription = $"Nico el Conejo observa los {itemPlural} {locationName}. Explora y cuenta los de esa posición.",
            FirstHint = $"Mira con atención la parte de {positionName}.",
            SecondHintDetailed = $"Cuenta uno a uno los {itemPlural} que están {positionName}.",
            ExplanationOfSuccess = $"¡Muy bien! Hay {count} {itemPlural} {positionName}.",
            SimplifiedFirstHint = $"Mira {positionName}.",
            SimplifiedSecondHint = $"Cuenta: uno, dos... los que están {positionName}.",
            SimplifiedExplanationOfSuccess = $"¡Bien! Hay {count} {itemPlural} {positionName}."
        };

        for (int i = 0; i < count; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_pos_{i + 1}",
                Name = $"{itemNoun} {i + 1}",
                EmojiSymbol = emoji,
                AccessibleDescription = $"Un {itemNoun} {locationName} número {i + 1}",
                OrderIndex = i + 1,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, count, min: 1, max: max + 2, unitLabel: itemPlural, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de cuentos con números para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenStoriesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int count = rng.Next(min, max + 1);

        var stories = new[]
        {
            ("cerdito", "cerditos", "🐷", "cerditos caminando a su casa", "Los Cerditos del Bosque"),
            ("patito", "patitos", "🦆", "patitos nadando en el estanque", "Los Patitos Nadadores"),
            ("oveja", "ovejas", "🐑", "ovejitas en la colina verde", "Las Ovejitas Traviesas")
        };
        var st = stories[rng.Next(stories.Length)];

        var ex = new Exercise
        {
            Id = $"gen_kstory_{Guid.NewGuid():N}",
            Title = $"Cuentos con Números: {st.Item5}",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 3,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = count,
            StandardInstruction = $"En el cuento de Luna, hay {st.Item4}. Toca cada uno para contar. ¿Cuántos {st.Item2} hay?",
            SimplifiedInstruction = $"Cuenta los {st.Item2} del cuento. ¿Cuántos hay?",
            ScreenReaderDescription = $"Cuento de Luna: hay varios {st.Item4}. Explora y cuenta cada uno.",
            FirstHint = $"Toca cada {st.Item1} una sola vez.",
            SecondHintDetailed = $"Cuenta despacito: uno, dos... hasta el último {st.Item1}.",
            ExplanationOfSuccess = $"¡Grandioso! El cuento tiene {count} {st.Item2}.",
            SimplifiedFirstHint = $"Toca cada {st.Item1}.",
            SimplifiedSecondHint = $"Cuenta: uno, dos...",
            SimplifiedExplanationOfSuccess = $"¡Muy bien! Hay {count} {st.Item2}."
        };

        for (int i = 0; i < count; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_story_{i + 1}",
                Name = $"{st.Item1} {i + 1}",
                EmojiSymbol = st.Item3,
                AccessibleDescription = $"Un {st.Item1} del cuento número {i + 1}",
                OrderIndex = i + 1,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, count, min: 1, max: max + 2, unitLabel: st.Item2, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de secuencias y patrones rítmicos para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenPatternsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;

        var patterns = new[]
        {
            (
                "Rojo y Azul",
                new[] { ("Rojo 1", "🔴", "Un círculo rojo", 262), ("Azul 1", "🔵", "Un círculo azul", 330), ("Rojo 2", "🔴", "Otro círculo rojo", 262), ("Azul 2", "🔵", "Otro círculo azul", 330) },
                "Mira la serie: rojo, azul, rojo, azul... ¿Qué color sigue ahora?",
                "Rojo, azul, rojo, azul... ¿Qué sigue?",
                "Secuencia rítmica de círculos rojos y azules.",
                "Rojo", 1, ("Rojo", 1), ("Azul", 2), ("Verde", 3)
            ),
            (
                "Círculo y Cuadrado",
                new[] { ("Círculo 1", "🔵", "Un círculo azul", 262), ("Cuadrado 1", "🟦", "Un cuadrado azul", 392), ("Círculo 2", "🔵", "Otro círculo azul", 262), ("Cuadrado 2", "🟦", "Otro cuadrado azul", 392) },
                "Mira la serie: círculo, cuadrado, círculo, cuadrado... ¿Qué forma sigue?",
                "Círculo, cuadrado, círculo, cuadrado... ¿Qué sigue?",
                "Secuencia rítmica de círculos y cuadrados.",
                "Círculo", 1, ("Círculo", 1), ("Cuadrado", 2), ("Triángulo", 3)
            ),
            (
                "Tambor y Palmas",
                new[] { ("Tambor 1", "🥁", "Un tambor sonoro", 220), ("Palmas 1", "👏", "Una palmada", 440), ("Tambor 2", "🥁", "Otro tambor sonoro", 220), ("Palmas 2", "👏", "Otra palmada", 440) },
                "Escucha el ritmo: tambor, palmas, tambor, palmas... ¿Qué sonido viene después?",
                "Tambor, palmas, tambor, palmas... ¿Qué sigue?",
                "Secuencia rítmica sonora de tambores y palmadas.",
                "Tambor", 1, ("Tambor", 1), ("Palmas", 2), ("Campana", 3)
            )
        };

        var p = patterns[rng.Next(patterns.Length)];

        var ex = new Exercise
        {
            Id = $"gen_kpattern_{Guid.NewGuid():N}",
            Title = $"Sigue el Ritmo: {p.Item1}",
            GameType = GamePlayType.OrderSequence,
            TargetAgeMin = 4,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = p.Item7,
            StandardInstruction = p.Item3,
            SimplifiedInstruction = p.Item4,
            ScreenReaderDescription = p.Item5,
            FirstHint = "Mira o escucha el elemento que se va alternando en la fila.",
            SecondHintDetailed = $"La fila termina en el segundo elemento; después vuelve a empezar el {p.Item6.ToLower()}.",
            ExplanationOfSuccess = $"¡Fantástico! Después del ciclo viene el {p.Item6.ToLower()}.",
            SimplifiedFirstHint = "Mira lo que se repite.",
            SimplifiedSecondHint = $"Sigue el {p.Item6.ToLower()}.",
            SimplifiedExplanationOfSuccess = $"¡Bien! Sigue el {p.Item6.ToLower()}."
        };

        for (int i = 0; i < p.Item2.Length; i++)
        {
            var item = p.Item2[i];
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_pat_{i + 1}",
                Name = item.Item1,
                EmojiSymbol = item.Item2,
                AccessibleDescription = item.Item3,
                OrderIndex = i + 1,
                MusicalPitchHz = item.Item4
            });
        }

        var optDefs = new[] { p.Item8, p.Item9, p.Item10 };
        for (int i = 0; i < optDefs.Length; i++)
        {
            var def = optDefs[i];
            bool isCorrect = (def.Item2 == p.Item7);
            ex.Options.Add(new ExerciseOption
            {
                Id = $"opt_kpat_{i + 1}",
                Label = def.Item1,
                Value = def.Item2,
                IsCorrect = isCorrect,
                AccessibleKeyShortcut = (i + 1).ToString(),
                SpeechFeedback = isCorrect
                    ? $"¡Muy bien! {def.Item1} es la opción correcta."
                    : $"Elegiste {def.Item1}. Observa el orden repetido del patrón."
            });
        }

        ShuffleOptions(ex, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de clasificación por atributo para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenSortingExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int redCount = rng.Next(min, max + 1);
        int greenCount = rng.Next(min, max + 1);
        while (greenCount == redCount)
        {
            greenCount = rng.Next(min, max + 1);
        }

        bool askRed = rng.Next(2) == 0;
        int expected = askRed ? redCount : greenCount;
        string targetColor = askRed ? "rojas" : "verdes";
        string targetEmoji = askRed ? "🍎" : "🍏";

        var ex = new Exercise
        {
            Id = $"gen_ksort_{Guid.NewGuid():N}",
            Title = $"Pon Cada Cosa en su Lugar: Manzanas {targetColor}",
            GameType = GamePlayType.Comparison,
            TargetAgeMin = 4,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = expected,
            StandardInstruction = $"Mateo el Oso clasificó manzanas en dos canastas: rojas y verdes. Cuenta solo las manzanas {targetColor}. ¿Cuántas hay?",
            SimplifiedInstruction = $"Cuenta las manzanas {targetColor}. ¿Cuántas hay?",
            ScreenReaderDescription = $"Hay manzanas rojas y manzanas verdes separadas por color. Explora y cuenta las {targetColor}.",
            FirstHint = $"Busca las manzanas con el emoji {targetEmoji} y cuéntalas.",
            SecondHintDetailed = $"Cuenta despacio solo las manzanas {targetColor}.",
            ExplanationOfSuccess = $"¡Excelente! Hay {expected} manzanas {targetColor}.",
            SimplifiedFirstHint = $"Busca las {targetColor}.",
            SimplifiedSecondHint = $"Cuenta solo las {targetColor}.",
            SimplifiedExplanationOfSuccess = $"¡Bien! Hay {expected} manzanas {targetColor}."
        };

        int order = 1;
        for (int i = 0; i < redCount; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_red_{i + 1}",
                Name = $"Manzana roja {i + 1}",
                EmojiSymbol = "🍎",
                AccessibleDescription = $"Una manzana roja número {i + 1}",
                OrderIndex = order++,
                MusicalPitchHz = PentatonicScaleHz[0]
            });
        }
        for (int i = 0; i < greenCount; i++)
        {
            ex.ItemsGroupB.Add(new ManipulableItem
            {
                Id = $"item_green_{i + 1}",
                Name = $"Manzana verde {i + 1}",
                EmojiSymbol = "🍏",
                AccessibleDescription = $"Una manzana verde número {i + 1}",
                OrderIndex = order++,
                MusicalPitchHz = PentatonicScaleHz[2]
            });
        }

        BuildOptions(ex, expected, min: 1, max: max + 2, unitLabel: $"manzanas {targetColor}", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de música y conteo sonoro para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenMusicExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        var (min, max) = GetTargetRange(profile);
        int noteCount = rng.Next(Math.Max(2, min), max + 1);

        var ex = new Exercise
        {
            Id = $"gen_kmusic_{Guid.NewGuid():N}",
            Title = "Música y Conteo con Bipo",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 4,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = noteCount,
            StandardInstruction = $"Bipo el Pájaro Cantor toca campanas doradas. Toca cada campana para escuchar su tono musical y contarla. ¿Cuántas notas hay en total?",
            SimplifiedInstruction = $"Toca cada campana para contar. ¿Cuántas hay?",
            ScreenReaderDescription = $"Campanas musicales afinadas en notas pentatónicas. Explora y cuenta cada una.",
            FirstHint = "Toca la primera campana y escucha su sonido.",
            SecondHintDetailed = "Sigue tocando cada campana hacia la derecha y cuenta en voz alta.",
            ExplanationOfSuccess = $"¡Maravilloso! Sonaron {noteCount} notas musicales.",
            SimplifiedFirstHint = "Toca la primera campana.",
            SimplifiedSecondHint = "Cuenta: una, dos... campanas.",
            SimplifiedExplanationOfSuccess = $"¡Muy bien! Hay {noteCount} campanas."
        };

        for (int i = 0; i < noteCount; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_bell_{i + 1}",
                Name = $"Campana {i + 1}",
                EmojiSymbol = "🔔",
                AccessibleDescription = $"Campana sonora dorada número {i + 1}",
                OrderIndex = i + 1,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length],
                SoundKind = ManipulableSoundKind.Bell
            });
        }

        BuildOptions(ex, noteCount, min: 1, max: max + 2, unitLabel: "campanas", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de conteo extendido hasta diez para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenCountToTenExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        int min = profile.HasDisability(DisabilityType.Intellectual) ? 3 : 5;
        int max = profile.HasDisability(DisabilityType.Intellectual) ? 5 : 10;
        int count = rng.Next(min, max + 1);

        var ex = new Exercise
        {
            Id = $"gen_kcount10_{Guid.NewGuid():N}",
            Title = "Cuenta hasta Diez con Cosmo",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 5,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = count,
            StandardInstruction = $"Cosmo ha reunido estrellas mágicas en el cielo nocturno. Toca cada estrella para contarla. ¿Cuántas estrellas hay en total?",
            SimplifiedInstruction = $"Toca cada estrella para contar. ¿Cuántas hay?",
            ScreenReaderDescription = $"Cielo nocturno con estrellas brillantes. Explora y cuenta una por una.",
            FirstHint = "Empieza desde la primera estrella y sigue contando en orden.",
            SecondHintDetailed = "Cuenta despacio: uno, dos, tres... el último número que digas es el total.",
            ExplanationOfSuccess = $"¡Brillante! Hay {count} estrellas en el cielo.",
            SimplifiedFirstHint = "Toca la primera estrella.",
            SimplifiedSecondHint = "Cuenta despacito: uno, dos, tres...",
            SimplifiedExplanationOfSuccess = $"¡Bien hecho! Hay {count} estrellas."
        };

        for (int i = 0; i < count; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"item_star_{i + 1}",
                Name = $"Estrella {i + 1}",
                EmojiSymbol = "⭐",
                AccessibleDescription = $"Estrella dorada brillante número {i + 1}",
                OrderIndex = i + 1,
                MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
            });
        }

        BuildOptions(ex, count, min: Math.Max(1, count - 3), max: count + 3, unitLabel: "estrellas", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de adición o sustracción concreta para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenAddSubExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isAdd = rng.Next(2) == 0;

        if (isAdd)
        {
            int a = rng.Next(1, 3);
            int b = rng.Next(1, 4);
            int total = a + b;

            var ex = new Exercise
            {
                Id = $"gen_kadd_{Guid.NewGuid():N}",
                Title = "Junto y Quito: Tito Junta Amigos",
                GameType = GamePlayType.AdditionCombine,
                TargetAgeMin = 5,
                TargetAgeMax = 6,
                Level = AcademicLevel.Kindergarten,
                ExpectedResult = total,
                StandardInstruction = $"Tito el Castor tiene {a} peces en un charco y agrega {b} peces más. ¿Cuántos peces hay en total?",
                SimplifiedInstruction = $"Hay {a} peces y llegan {b} más. ¿Cuántos hay en total?",
                ScreenReaderDescription = $"Tito tiene dos grupos de peces: un grupo con {a} y otro con {b}. Júntalos y cuéntalos todos.",
                FirstHint = $"Cuenta primero los {a} peces y luego sigue con los otros {b}.",
                SecondHintDetailed = $"Junta ambos grupos: {a} más {b} es igual a {total}.",
                ExplanationOfSuccess = $"¡Excelente! {a} y {b} juntos dan {total} peces.",
                SimplifiedFirstHint = "Junta todos los peces.",
                SimplifiedSecondHint = $"{a} más {b} son {total}.",
                SimplifiedExplanationOfSuccess = $"¡Muy bien! Hay {total} peces."
            };

            int order = 1;
            for (int i = 0; i < a; i++)
            {
                ex.ItemsGroupA.Add(new ManipulableItem
                {
                    Id = $"fish_a_{i + 1}",
                    Name = $"Pez {i + 1}",
                    EmojiSymbol = "🐟",
                    AccessibleDescription = $"Un pez del primer grupo número {i + 1}",
                    OrderIndex = order++,
                    MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
                });
            }
            for (int i = 0; i < b; i++)
            {
                ex.ItemsGroupB.Add(new ManipulableItem
                {
                    Id = $"fish_b_{i + 1}",
                    Name = $"Pez {a + i + 1}",
                    EmojiSymbol = "🐠",
                    AccessibleDescription = $"Un pez del segundo grupo número {a + i + 1}",
                    OrderIndex = order++,
                    MusicalPitchHz = PentatonicScaleHz[(a + i) % PentatonicScaleHz.Length]
                });
            }

            BuildOptions(ex, total, min: 1, max: total + 3, unitLabel: "peces", rng);
            return ex;
        }
        else
        {
            int total = rng.Next(3, 6);
            int take = rng.Next(1, total - 1);
            int remainder = total - take;

            var ex = new Exercise
            {
                Id = $"gen_ksub_{Guid.NewGuid():N}",
                Title = "Junto y Quito: Peces que Nadan Lejos",
                GameType = GamePlayType.SubtractionTakeAway,
                TargetAgeMin = 5,
                TargetAgeMax = 6,
                Level = AcademicLevel.Kindergarten,
                ExpectedResult = remainder,
                StandardInstruction = $"Había {total} bellotas en el árbol. Una ardilla se llevó {take} bellotas. ¿Cuántas bellotas quedan?",
                SimplifiedInstruction = $"Había {total} bellotas y se llevaron {take}. ¿Cuántas quedan?",
                ScreenReaderDescription = $"Había {total} bellotas y se retiran {take}. Cuenta cuántas bellotas quedan.",
                FirstHint = $"Quita {take} bellotas del total de {total}.",
                SecondHintDetailed = $"Cuenta solo las que quedaron: {total} menos {take} es igual a {remainder}.",
                ExplanationOfSuccess = $"¡Perfecto! Quedan {remainder} bellotas.",
                SimplifiedFirstHint = "Quita las que se fueron.",
                SimplifiedSecondHint = $"{total} menos {take} son {remainder}.",
                SimplifiedExplanationOfSuccess = $"¡Bien! Quedan {remainder} bellotas."
            };

            for (int i = 0; i < remainder; i++)
            {
                ex.ItemsGroupA.Add(new ManipulableItem
                {
                    Id = $"acorn_rem_{i + 1}",
                    Name = $"Bellota {i + 1}",
                    EmojiSymbol = "🌰",
                    AccessibleDescription = $"Bellota que quedó número {i + 1}",
                    OrderIndex = i + 1,
                    MusicalPitchHz = PentatonicScaleHz[i % PentatonicScaleHz.Length]
                });
            }

            BuildOptions(ex, remainder, min: 1, max: total + 1, unitLabel: "bellotas", rng);
            return ex;
        }
    }

    /// <summary>
    /// Genera un ejercicio interactivo de medición y comparación de tamaño para jardín de niños.
    /// </summary>
    public Exercise GenerateKindergartenMeasureExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        int a = rng.Next(2, 5);
        int b = rng.Next(2, 5);
        while (b == a)
        {
            b = rng.Next(2, 5);
        }

        bool askTaller = rng.Next(2) == 0;
        int expectedOption = (askTaller ? (a > b ? 1 : 2) : (a < b ? 1 : 2));
        string questionAttr = askTaller ? "más alta" : "más baja";

        var ex = new Exercise
        {
            Id = $"gen_kmeasure_{Guid.NewGuid():N}",
            Title = $"Mido y Ordeno: Torres de Bloques",
            GameType = GamePlayType.Comparison,
            TargetAgeMin = 5,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = expectedOption,
            StandardInstruction = $"Dino el Dinosaurio construyó dos torres de bloques: la Torre Azul tiene {a} bloques y la Torre Roja tiene {b} bloques. ¿Cuál torre es {questionAttr}?",
            SimplifiedInstruction = $"¿Cuál torre es {questionAttr}? La azul de {a} o la roja de {b}.",
            ScreenReaderDescription = $"Dos torres de bloques: Torre Azul con {a} bloques y Torre Roja con {b} bloques. Compara cuál es {questionAttr}.",
            FirstHint = $"Cuenta los bloques de cada torre: {a} bloques y {b} bloques.",
            SecondHintDetailed = askTaller
                ? $"La torre con más bloques ({Math.Max(a, b)}) es la más alta."
                : $"La torre con menos bloques ({Math.Min(a, b)}) es la más baja.",
            ExplanationOfSuccess = $"¡Muy bien! La torre {(expectedOption == 1 ? "Azul" : "Roja")} es la {questionAttr}.",
            SimplifiedFirstHint = "Mira cuál tiene más o menos bloques.",
            SimplifiedSecondHint = $"La que tiene {(askTaller ? Math.Max(a, b) : Math.Min(a, b))} bloques es {questionAttr}.",
            SimplifiedExplanationOfSuccess = $"¡Bien! La torre {(expectedOption == 1 ? "Azul" : "Roja")} es {questionAttr}."
        };

        int order = 1;
        for (int i = 0; i < a; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"block_blue_{i + 1}",
                Name = $"Bloque azul {i + 1}",
                EmojiSymbol = "🟦",
                AccessibleDescription = $"Bloque azul número {i + 1} de la Torre Azul",
                OrderIndex = order++,
                MusicalPitchHz = PentatonicScaleHz[0]
            });
        }
        for (int i = 0; i < b; i++)
        {
            ex.ItemsGroupB.Add(new ManipulableItem
            {
                Id = $"block_red_{i + 1}",
                Name = $"Bloque rojo {i + 1}",
                EmojiSymbol = "🟥",
                AccessibleDescription = $"Bloque rojo número {i + 1} de la Torre Roja",
                OrderIndex = order++,
                MusicalPitchHz = PentatonicScaleHz[3]
            });
        }

        ex.Options.Add(new ExerciseOption
        {
            Id = "opt_kmeas_1",
            Label = $"Torre Azul ({a} bloques)",
            Value = 1,
            IsCorrect = (expectedOption == 1),
            AccessibleKeyShortcut = "1",
            SpeechFeedback = (expectedOption == 1)
                ? $"¡Correcto! La Torre Azul con {a} bloques es {questionAttr}."
                : $"La Torre Azul tiene {a} bloques. Compara con la Torre Roja de {b} bloques."
        });
        ex.Options.Add(new ExerciseOption
        {
            Id = "opt_kmeas_2",
            Label = $"Torre Roja ({b} bloques)",
            Value = 2,
            IsCorrect = (expectedOption == 2),
            AccessibleKeyShortcut = "2",
            SpeechFeedback = (expectedOption == 2)
                ? $"¡Correcto! La Torre Roja con {b} bloques es {questionAttr}."
                : $"La Torre Roja tiene {b} bloques. Compara con la Torre Azul de {a} bloques."
        });

        ShuffleOptions(ex, rng);
        return ex;
    }

    /// <summary>
    /// Genera una lista completa de ejercicios procedimentales adaptados para cualquier tema solicitado.
    /// </summary>
    public List<Exercise> GenerateExercisesForTopic(string topicId, UserProfile profile, int count = 3, Random? rng = null)
    {
        rng ??= Random.Shared;
        var list = new List<Exercise>();

        for (int i = 0; i < count; i++)
        {
            var ex = topicId switch
            {
                "kinder_counting_01" => (i % 2 == 0)
                    ? GenerateCountingExercise(profile, rng)
                    : GenerateStableOrderExercise(profile, rng),

                "kinder_shapes_01" => GenerateKindergartenShapesExercise(profile, rng),
                "kinder_positions_01" => GenerateKindergartenPositionsExercise(profile, rng),
                "kinder_stories_01" => GenerateKindergartenStoriesExercise(profile, rng),
                "kinder_patterns_01" => GenerateKindergartenPatternsExercise(profile, rng),
                "kinder_sorting_01" => GenerateKindergartenSortingExercise(profile, rng),
                "kinder_music_01" => GenerateKindergartenMusicExercise(profile, rng),
                "kinder_count_to_10_01" => GenerateKindergartenCountToTenExercise(profile, rng),
                "kinder_add_sub_01" => GenerateKindergartenAddSubExercise(profile, rng),
                "kinder_measure_01" => GenerateKindergartenMeasureExercise(profile, rng),

                "elem_addition_01" => (i % 3) switch
                {
                    0 => GenerateAdditionCombineExercise(profile, rng),
                    1 => GenerateCommutativeExercise(profile, rng),
                    _ => GenerateNeutralZeroExercise(profile, rng)
                },

                "elem_subtraction_01" => GenerateSubtractionExercise(profile, rng),

                "elem_multiplication_01" => GenerateMultiplicationArrayExercise(profile, rng),

                "elem_division_01" => GenerateDivisionFairSharingExercise(profile, rng),

                "elem_money_currency_01" => GenerateMoneyCurrencyExercise(profile, rng),

                "elem_measurement_ruler_01" => GenerateMeasurementRulerExercise(profile, rng),

                "elem_time_clock_01" => GenerateTimeClockExercise(profile, rng),

                "elem_fractions_01" => (i % 2 == 0)
                    ? GenerateFractionPartsExercise(profile, rng)
                    : GenerateFractionEquivalenceExercise(profile, rng),

                "elem_distributive_01" => GenerateMultiplicationDistributiveExercise(profile, rng),

                "elem_geometry_angles_01" => GenerateAnglesExercise(profile, rng),

                "elem_geometry_perimeter_01" => GeneratePerimeterExercise(profile, rng),

                "elem_division_remainder_01" => GenerateDivisionRemainderExercise(profile, rng),

                "elem_fractions_numberline_01" => GenerateFractionsNumberLineExercise(profile, rng),

                "elem_data_barcharts_01" => GenerateDataBarChartsExercise(profile, rng),

                "elem_stats_central_tendency_01" => GenerateStatisticsCentralTendencyExercise(profile, rng),

                "sec_circle_geometry_01" => GenerateCircleGeometryExercise(profile, rng),

                "sec_factors_multiples_mcm_mcd_01" => GenerateFactorsMcmMcdExercise(profile, rng),

                "sec_negative_rational_numbers_01" => GenerateNegativeRationalNumbersExercise(profile, rng),

                "sec_algebra_factoring_01" => GenerateAlgebraFactoringExercise(profile, rng),

                "sec_parallel_lines_angles_01" => GenerateParallelLinesAnglesExercise(profile, rng),

                "sec_probability_compound_01" => GenerateProbabilityCompoundEventsExercise(profile, rng),

                "prep_conics_parabola_ellipse_01" => GenerateConicsParabolaEllipseExercise(profile, rng),

                "prep_calculus_limits_01" => GenerateCalculusLimitsExercise(profile, rng),

                "prep_calculus_derivative_rules_01" => GenerateCalculusDerivativeRulesExercise(profile, rng),

                "elem_mixed_fractions_01" => GenerateMixedFractionsExercise(profile, rng),

                "elem_decimals_01" => GenerateDecimalsMoneyExercise(profile, rng),

                "elem_geometry_area_01" => GenerateAreaExercise(profile, rng),

                "elem_powers_01" => GeneratePowersExercise(profile, rng),

                "elem_proportion_01" => GenerateDirectProportionExercise(profile, rng),

                "elem_algebra_balance_01" => GenerateAlgebraicBalanceExercise(profile, rng),

                "elem_geometry_volume_01" => GenerateVolumeExercise(profile, rng),

                "elem_square_root_01" => GenerateSquareRootExercise(profile, rng),

                "elem_cartesian_coordinates_01" => GenerateCartesianCoordinatesExercise(profile, rng),

                "sec_signed_numbers_01" => GenerateSignedNumbersExercise(profile, rng),

                "sec_order_operations_01" => GenerateOrderOfOperationsExercise(profile, rng),

                "sec_linear_equations_01" => GenerateLinearEquationAXBExercise(profile, rng),

                "sec_percentages_01" => GeneratePercentageExercise(profile, rng),

                "sec_exponent_laws_01" => GenerateExponentLawsExercise(profile, rng),

                "sec_polynomials_like_terms_01" => GeneratePolynomialLikeTermsExercise(profile, rng),

                "sec_linear_systems_2x2_01" => GenerateLinearSystem2x2Exercise(profile, rng),

                "sec_pythagorean_theorem_01" => GeneratePythagoreanTheoremExercise(profile, rng),

                "sec_quadratic_equations_01" => GenerateQuadraticEquationExercise(profile, rng),

                "sec_thales_similarity_01" => GenerateTriangleSimilarityThalesExercise(profile, rng),

                "sec_trigonometry_ratios_01" => GenerateTrigonometricRatiosExercise(profile, rng),

                "sec_quadratic_functions_01" => GenerateQuadraticFunctionCurveExercise(profile, rng),

                "prep_analytic_distance_01" => GenerateAnalyticDistanceExercise(profile, rng),

                "prep_line_slope_01" => GenerateLineSlopeExercise(profile, rng),

                "prep_line_equation_ymxb_01" => GenerateLinearEquationYMXBExercise(profile, rng),

                "prep_circle_conic_01" => GenerateConicCircleExercise(profile, rng),

                "prep_sine_wave_01" => GenerateSineWaveContinuousExercise(profile, rng),

                "prep_logarithm_binary_01" => GenerateLogarithmBinaryExercise(profile, rng),

                "prep_calculus_derivative_01" => GenerateCalculusDerivativeExercise(profile, rng),

                "prep_calculus_integral_01" => GenerateCalculusIntegralExercise(profile, rng),

                "prep_probability_stats_01" => GenerateProbabilityAndStatsExercise(profile, rng),

                "prog_seed_01" => GeneratePatternAndLoopSeedExercise(profile, rng),

                "prog_variables_state_01" => GenerateProgrammingVariablesExercise(profile, rng),

                "prog_conditionals_ifelse_01" => GenerateProgrammingConditionalsExercise(profile, rng),

                "prog_boolean_logic_01" => GenerateProgrammingBooleanLogicExercise(profile, rng),

                "prog_loops_for_while_01" => GenerateProgrammingLoopsExercise(profile, rng),

                "prog_arrays_lists_01" => GenerateProgrammingArraysExercise(profile, rng),

                "prog_functions_params_01" => GenerateProgrammingFunctionsExercise(profile, rng),

                "prog_algorithms_search_01" => GenerateProgrammingAlgorithmsSearchExercise(profile, rng),

                "prog_game_coordinates_01" => GenerateProgrammingGameCoordinatesExercise(profile, rng),

                "prog_assistive_ai_data_01" => GenerateProgrammingAssistiveAIExercise(profile, rng),

                _ => GenerateCountingExercise(profile, rng)
            };

            ShuffleOptions(ex, rng);
            list.Add(ex);
        }

        return list;
    }

    /// <summary>
    /// Aleatoriza el orden de las opciones de respuesta utilizando el algoritmo Fisher-Yates
    /// para eliminar el sesgo de posición (la respuesta correcta ya no queda siempre fija en la opción 2),
    /// y reasigna los atajos de teclado del 1 al N de forma secuencial.
    /// </summary>
    public static void ShuffleOptions(Exercise ex, Random? rng = null)
    {
        if (ex.Options.Count <= 1) return;
        rng ??= Random.Shared;

        for (int i = ex.Options.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (ex.Options[i], ex.Options[j]) = (ex.Options[j], ex.Options[i]);
        }

        for (int i = 0; i < ex.Options.Count; i++)
        {
            ex.Options[i].AccessibleKeyShortcut = (i + 1).ToString();
        }
    }
    private static void BuildOptions(Exercise ex, int correctValue, int min, int max, string unitLabel, Random rng)
    {
        // Distractores cercanos al resultado pero elegidos aleatoriamente dentro del
        // rango definido por cada tipo de ejercicio, sin salirse de sus límites.
        int span = Math.Max(0, max - min);
        int jitter = Math.Max(1, span / 3);

        var candidates = new List<int>();
        int lower = Math.Min(min, correctValue);
        int upper = Math.Max(max, correctValue);
        for (int v = lower; v <= upper; v++)
        {
            if (v == correctValue) continue;
            candidates.Add(v);
        }

        var chosen = candidates
            .Select(v => (Value: v, Weight: Math.Abs(v - correctValue) + rng.Next(0, jitter + 1)))
            .OrderBy(c => c.Weight).ThenBy(c => c.Value)
            .Take(2)
            .Select(c => c.Value)
            .ToList();

        // Respaldo determinista (siempre terminante): si el rango no aporta dos
        // distractores, se expande alternando lados (arriba y abajo) para no
        // sesgar la dirección de la pista. Caso extremo: un rango con solo dos
        // opciones posibles (p. ej. fracción 1/2 con max = 2).
        int delta = 1;
        while (chosen.Count < 2)
        {
            // Si el rango no tiene espacio a un lado, se usa el otro; si tiene
            // espacio en ambos, se alterna el lado según la distancia.
            bool aboveFirst = correctValue <= min
                ? true
                : correctValue >= max
                    ? false
                    : delta % 2 == 1;

            int above = correctValue + delta;
            int below = correctValue - delta;

            if (aboveFirst)
            {
                if (above != correctValue && !chosen.Contains(above)) chosen.Add(above);
                if (chosen.Count < 2 && below != correctValue && !chosen.Contains(below)) chosen.Add(below);
            }
            else
            {
                if (below != correctValue && !chosen.Contains(below)) chosen.Add(below);
                if (chosen.Count < 2 && above != correctValue && !chosen.Contains(above)) chosen.Add(above);
            }
            delta++;
        }

        var listValues = new List<int>(chosen) { correctValue };

        foreach (int val in listValues)
        {
            bool isCorrect = (val == correctValue);
            string label = string.IsNullOrWhiteSpace(unitLabel) ? val.ToString() : $"{val} {unitLabel}";
            string shortcut = (listValues.IndexOf(val) + 1).ToString();

            ex.Options.Add(new ExerciseOption
            {
                Id = $"opt_{val}",
                Value = val,
                Label = label,
                IsCorrect = isCorrect,
                AccessibleKeyShortcut = shortcut,
                SpeechFeedback = isCorrect
                    ? $"¡Muy bien! {label} es la respuesta correcta."
                    : BuildDiagnosticWrongFeedback(ex, val, correctValue, label)
            });
        }

        // Aleatorizar opciones para eliminar el sesgo de posición
        ShuffleOptions(ex, rng);
    }

    private static string BuildDiagnosticWrongFeedback(Exercise ex, int selectedValue, int correctValue, string label)
    {
        string directionHint = selectedValue < correctValue
            ? $"Elegiste {label}, que es menor que el resultado correcto."
            : $"Elegiste {label}, que es mayor que el resultado correcto.";

        string conceptGuidance = ex.GameType switch
        {
            GamePlayType.Comparison => "Compara con atención las cantidades o tamaños de ambos grupos.",
            GamePlayType.InteractiveCounting => "Asegúrate de contar cada objeto una sola vez hasta llegar al último.",
            GamePlayType.OrderSequence => "Recuerda seguir el orden fijo de la serie numérica paso a paso.",
            GamePlayType.AdditionCombine => "Une los dos grupos y cuenta todos los elementos juntos.",
            GamePlayType.CommutativePropertyDemo => "Por la propiedad conmutativa, cambiar el orden de los sumandos mantiene exactamente la misma suma.",
            GamePlayType.AssociativePropertyDemo => "Agrupar los sumandos de distinta forma no altera el total de la suma.",
            GamePlayType.ZeroNeutralProperty => "Sumar 0 no agrega ni quita nada: la cantidad original se conserva igual.",
            GamePlayType.SubtractionTakeAway => "En la resta partimos del total inicial y descontamos los que se fueron.",
            GamePlayType.InverseRelation => "Recuerda que la resta deshace la suma y te regresa al número inicial.",
            GamePlayType.MultiplicationArray => "Suma los elementos de todas las filas iguales o multiplica filas por columnas.",
            GamePlayType.DivisionFairSharing => "Reparte el total en partes iguales dividiendo entre el número de grupos.",
            GamePlayType.FractionParts => "Cuenta únicamente las partes seleccionadas (numerador) del entero dividido.",
            GamePlayType.MultiplicationDistributive => "Multiplica por separado las decenas y las unidades, y al final suma ambos productos.",
            GamePlayType.MixedFractions => "Convierte el entero en partes iguales al denominador y súmale las partes adicionales.",
            GamePlayType.DecimalsMoney => "Suma primero las monedas enteras y luego agrega el valor de las monedas fraccionarias.",
            GamePlayType.PowersAndExponentGrowth => "En una potencia multiplicamos la base por sí misma tantas veces como indica el exponente (no multipliques base por exponente).",
            GamePlayType.DirectProportion => "Halla primero el valor de 1 unidad y luego multiplícalo por la cantidad pedida.",
            GamePlayType.AlgebraicBalance => "Para mantener la balanza en equilibrio, resta el peso conocido a ambos lados.",
            GamePlayType.SquareRootPerfect => "Busca el número que multiplicado por sí mismo (al cuadrado) dé el área total.",
            GamePlayType.CartesianCoordinates => "En el par ordenado (X, Y), el primer número siempre indica el avance horizontal X y el segundo el vertical Y.",
            GamePlayType.SignedNumbersAddition => "Compara tu dinero a favor (+) con la deuda (-): resta los valores y conserva el signo del mayor.",
            GamePlayType.OrderOfOperationsPEMDAS => "Por jerarquía de operaciones (PEMDAS), resuelve primero paréntesis y multiplicaciones antes de sumar.",
            GamePlayType.LinearEquationAXB => "En ax + b = c, primero resta b de c y luego divide el resultado entre a.",
            GamePlayType.PercentageCalculation => "Calcula el descuento multiplicando por el porcentaje y dividiendo entre 100.",
            GamePlayType.ExponentLaws => "Con la misma base, al multiplicar se suman los exponentes y al dividir se restan.",
            GamePlayType.PolynomialLikeTerms => "Suma o resta los coeficientes de los términos que tienen exactamente la misma letra y exponente.",
            GamePlayType.LinearSystem2x2 => "Sustituye o suma las dos ecuaciones para despejar primero una incógnita.",
            GamePlayType.PythagoreanTheorem => "Por el Teorema de Pitágoras, suma los cuadrados de los catetos (a² + b²) y saca la raíz cuadrada.",
            GamePlayType.QuadraticEquations => "En ax² + c = d, despeja x² y finalmente aplica la raíz cuadrada.",
            GamePlayType.TriangleSimilarityThales => "Por el Teorema de Tales, la razón entre altura y sombra se mantiene constante en ambos triángulos.",
            GamePlayType.TrigonometricRatios => "Identifica bien el cateto opuesto, el cateto adyacente y la hipotenusa respecto al ángulo.",
            GamePlayType.QuadraticFunctionCurve => "Evalúa primero x² (x por x), multiplícalo por el coeficiente a y al final suma c.",
            GamePlayType.AnalyticDistance2D => "Calcula los incrementos Δx y Δy, elévalos al cuadrado, súmalos y obtén la raíz cuadrada.",
            GamePlayType.LineSlopeM => "La pendiente m se obtiene dividiendo el cambio vertical (y₂ - y₁) entre el horizontal (x₂ - x₁).",
            GamePlayType.LinearEquationYMXB => "En la recta y = mx + b, multiplica primero la pendiente m por x y luego suma la ordenada al origen b.",
            GamePlayType.ConicCircleRadius => "En x² + y² = r², el número del lado derecho es r², saca su raíz cuadrada para hallar el radio r.",
            GamePlayType.SineWaveContinuous => "En y = A·sen(x), el valor máximo se alcanza cuando sen(x) = 1, dando la amplitud A.",
            GamePlayType.LogarithmBinary => "El logaritmo base 2 cuenta cuántas veces debes multiplicar 2 por sí mismo para obtener ese número.",
            GamePlayType.CalculusDerivativeRate => "Por la regla de la derivada d/dt(a·t²) = 2a·t, multiplica 2 por a y por el instante t.",
            GamePlayType.CalculusIntegralArea => "La integral definida de una tasa constante es el área del rectángulo: multiplica la altura por el tiempo.",
            GamePlayType.ProbabilityAndStats => "Para hallar la media aritmética x̄, suma todos los datos de la muestra y divide entre la cantidad de datos.",
            GamePlayType.PatternAndLoopSeed => "En el bucle REPETIR, suma los pasos de cada salto tantas veces como indica el contador.",
            GamePlayType.ProgrammingVariablesState => "Sigue el valor de la variable línea por línea: primero aplica la suma al valor inicial y al resultado réstale el gasto.",
            GamePlayType.ProgrammingConditionalsIfElse => "Evalúa primero si la condición del SI (>=) se cumple para elegir el bono correcto antes de sumarlo.",
            GamePlayType.ProgrammingBooleanLogic => "En el operador lógico Y (AND), la condición solo es verdadera si ambas entradas valen 1 al mismo tiempo.",
            GamePlayType.ProgrammingLoopsForWhile => "Multiplica el incremento por el número de repeticiones del ciclo PARA y súmalo al valor inicial de la variable acumuladora.",
            GamePlayType.ProgrammingArraysZeroIndex => "Recuerda que en un arreglo el primer elemento siempre ocupa el índice [0], el segundo el [1], el tercero el [2] y el cuarto el [3].",
            GamePlayType.ProgrammingFunctionsReturn => "Sustituye el parámetro de entrada en la regla de la función: multiplica primero por el factor y luego suma la constante antes de retornar.",
            GamePlayType.ProgrammingAlgorithmsSearch => "En el algoritmo de búsqueda del máximo, compara cada elemento de la lista y conserva únicamente el valor más grande encontrado.",
            GamePlayType.ProgrammingGameCoordinates2D => "Multiplica primero la velocidad por el tiempo (v × t) y luego suma ese desplazamiento a la coordenada X inicial.",
            GamePlayType.ProgrammingAssistiveAIData => "Suma las 3 lecturas de los sensores, divide entre 3 para hallar el promedio y finalmente suma el ajuste de calibración.",
            _ => "Revisa paso a paso los datos del problema."
        };

        return $"{directionHint} {conceptGuidance}";
    }

    /// <summary>
    /// Genera un ejercicio interactivo y adaptativo de ángulos (recto, agudo, obtuso, llano).
    /// </summary>
    public Exercise GenerateAnglesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        // Modos: 0 = Angulo Recto (90°), 1 = Angulo Agudo (<90°), 2 = Angulo Obtuso (>90°), 3 = Angulo Llano (180°)
        int mode = rng.Next(4);

        int expected;
        string typeLabel;
        string standardDesc;
        string simplifiedDesc;
        string emoji;
        string screenDesc;
        string firstHint;
        string secondHint;
        string successMsg;

        switch (mode)
        {
            case 0: // Recto
                expected = 90;
                typeLabel = "Ángulo Recto";
                emoji = "📐";
                standardDesc = "Observa la esquina del marco en forma de L perfecta (un cuarto de vuelta). ¿Cuántos grados mide un ángulo recto?";
                simplifiedDesc = "¿Cuántos grados mide una esquina recta en L?";
                screenDesc = "Una esquina cuadrada perpendicular que mide exactamente 90 grados.";
                firstHint = "Un ángulo recto es un cuarto de giro completo.";
                secondHint = "La esquina de un cuadrado o una hoja mide 90 grados.";
                successMsg = "¡Correcto! El ángulo recto mide exactamente 90 grados.";
                break;

            case 1: // Agudo
                int[] acuteValues = isSimplified ? new[] { 45, 60 } : new[] { 30, 45, 60, 75 };
                expected = acuteValues[rng.Next(acuteValues.Length)];
                typeLabel = "Ángulo Agudo";
                emoji = "🔺";
                standardDesc = $"Un haz de luz estelar forma una abertura cerrada y afilada de {expected} grados. Como mide menos de 90°, ¿cuántos grados mide esta abertura?";
                simplifiedDesc = $"El haz de luz mide {expected} grados (es menor a 90 grados). ¿Cuánto mide?";
                screenDesc = $"Una abertura aguda de {expected} grados en punta.";
                firstHint = "Es un ángulo agudo porque mide menos de 90 grados.";
                secondHint = $"La abertura afilada mide {expected} grados.";
                successMsg = $"¡Exacto! Mide {expected} grados y es un ángulo agudo menor a 90°.";
                break;

            case 2: // Obtuso
                int[] obtuseValues = isSimplified ? new[] { 120, 135 } : new[] { 105, 120, 135, 150 };
                expected = obtuseValues[rng.Next(obtuseValues.Length)];
                typeLabel = "Ángulo Obtuso";
                emoji = "🪭";
                standardDesc = $"Un alerón solar se abrió ampliamente formando una abertura de {expected} grados. Como es mayor a 90° pero menor a 180°, ¿cuántos grados mide este ángulo obtuso?";
                simplifiedDesc = $"El alerón está muy abierto y mide {expected} grados. ¿Cuánto mide?";
                screenDesc = $"Una abertura obtusa amplia de {expected} grados.";
                firstHint = "Es un ángulo obtuso porque supera los 90 grados de una esquina recta.";
                secondHint = $"El ángulo abierto mide {expected} grados.";
                successMsg = $"¡Muy bien! Mide {expected} grados y es un ángulo obtuso.";
                break;

            default: // Llano
                expected = 180;
                typeLabel = "Ángulo Llano";
                emoji = "🔭";
                standardDesc = "La antena espacial giró media vuelta completa formando una línea recta horizontal. ¿Cuántos grados mide este ángulo llano?";
                simplifiedDesc = "¿Cuántos grados mide media vuelta recta completa?";
                screenDesc = "Una línea recta horizontal que representa media vuelta de 180 grados.";
                firstHint = "Una vuelta entera son 360 grados; media vuelta es su mitad.";
                secondHint = "Dos ángulos rectos juntos (90° + 90°) suman 180 grados.";
                successMsg = "¡Brillante! Una línea recta completa de media vuelta mide 180 grados.";
                break;
        }

        var ex = new Exercise
        {
            Id = $"gen_angle_{Guid.NewGuid():N}",
            Title = $"Radar de Giros: {typeLabel}",
            GameType = GamePlayType.GeometryAnglesClassification,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = expected,
            StandardInstruction = standardDesc,
            SimplifiedInstruction = simplifiedDesc,
            ScreenReaderDescription = screenDesc,
            FirstHint = firstHint,
            SecondHintDetailed = secondHint,
            ExplanationOfSuccess = successMsg,
            SimplifiedFirstHint = firstHint,
            SimplifiedSecondHint = secondHint,
            SimplifiedExplanationOfSuccess = successMsg
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = $"item_ang_1",
            Name = typeLabel,
            EmojiSymbol = emoji,
            AccessibleDescription = screenDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(30, expected - 45), max: Math.Min(180, expected + 45), unitLabel: "°", rng);

        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de área en 2D (rectángulo, cuadrado, triángulo).
    /// </summary>
    public Exercise GenerateAreaExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 9);

        // Modos: 0 = Rectángulo (b × h), 1 = Cuadrado (l × l), 2 = Triángulo ((b × h) / 2)
        int mode = rng.Next(3);

        int expected;
        string title;
        string standardDesc;
        string simplifiedDesc;
        string screenDesc;
        string firstHint;
        string secondHint;
        string successMsg;
        string emoji;

        if (mode == 0) // Rectángulo
        {
            int b = isSimplified ? rng.Next(3, 6) : rng.Next(4, 9);
            int h = isSimplified ? rng.Next(2, 5) : rng.Next(3, 7);
            expected = b * h;
            title = $"Área del Rectángulo ({b} × {h})";
            emoji = "🟩";
            standardDesc = $"Un piso rectangular mide {b} metros de base y {h} metros de altura. Aplicando Área = Base × Altura ({b} × {h}), ¿cuál es el área en metros cuadrados (m²)?";
            simplifiedDesc = $"El piso mide {b} de base y {h} de alto. Multiplica {b} × {h}. ¿Cuál es el área?";
            screenDesc = $"Un rectángulo de {b} metros de base por {h} metros de altura con {expected} cuadritos interiores.";
            firstHint = $"Multiplica la base por la altura: {b} × {h}.";
            secondHint = $"{b} × {h} = {expected} metros cuadrados.";
            successMsg = $"¡Excelente! {b} × {h} = {expected} m² de área.";
        }
        else if (mode == 1) // Cuadrado
        {
            int l = isSimplified ? rng.Next(3, 6) : rng.Next(4, 8);
            expected = l * l;
            title = $"Área del Cuadrado ({l} × {l})";
            emoji = "🟦";
            standardDesc = $"Una plataforma cuadrada tiene lados de {l} metros. Aplicando Área = Lado × Lado ({l} × {l}), ¿cuál es su área total en m²?";
            simplifiedDesc = $"Un cuadrado de {l} por {l}. Multiplica {l} × {l}.";
            screenDesc = $"Un cuadrado de {l} metros de lado con {expected} unidades cuadradas.";
            firstHint = $"Multiplica el lado por sí mismo: {l} × {l}.";
            secondHint = $"{l} × {l} = {expected} metros cuadrados.";
            successMsg = $"¡Brillante! {l} × {l} = {expected} m² de área.";
        }
        else // Triángulo
        {
            int[] evenBases = isSimplified ? new[] { 4, 6 } : new[] { 4, 6, 8, 10 };
            int b = evenBases[rng.Next(evenBases.Length)];
            int h = isSimplified ? rng.Next(2, 5) : rng.Next(3, 7);
            expected = (b * h) / 2;
            title = $"Área del Triángulo (({b} × {h}) / 2)";
            emoji = "🔺";
            standardDesc = $"Una vela triangular tiene una base de {b} metros y una altura de {h} metros. Recuerda que el área de un triángulo es la mitad de un rectángulo: ({b} × {h}) / 2. ¿Cuál es su área en m²?";
            simplifiedDesc = $"Multiplica {b} × {h} = {b * h}, y luego sácale la mitad (divide entre 2). ¿Cuánto da?";
            screenDesc = $"Un triángulo de base {b} y altura {h}, equivalente a la mitad de un rectángulo de {b * h} m².";
            firstHint = $"Primero multiplica {b} × {h} = {b * h}, y luego divide entre 2.";
            secondHint = $"{b * h} dividido entre 2 = {expected} metros cuadrados.";
            successMsg = $"¡Exacto! ({b} × {h}) / 2 = {expected} m².";
        }

        var ex = new Exercise
        {
            Id = $"gen_area_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.GeometryArea2D,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = expected,
            StandardInstruction = standardDesc,
            SimplifiedInstruction = simplifiedDesc,
            ScreenReaderDescription = screenDesc,
            FirstHint = firstHint,
            SecondHintDetailed = secondHint,
            ExplanationOfSuccess = successMsg,
            SimplifiedFirstHint = firstHint,
            SimplifiedSecondHint = secondHint,
            SimplifiedExplanationOfSuccess = successMsg
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = $"item_area_1",
            Name = title,
            EmojiSymbol = emoji,
            AccessibleDescription = screenDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(4, expected - 10), max: expected + 15, unitLabel: "m²", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de volumen tridimensional (cubo y prisma rectangular).
    /// </summary>
    public Exercise GenerateVolumeExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 10);

        // Modos: 0 = Prisma rectangular (l × w × h), 1 = Cubo (l³), 2 = Base Area × Altura
        int mode = rng.Next(3);

        int expected;
        string title;
        string standardDesc;
        string simplifiedDesc;
        string screenDesc;
        string firstHint;
        string secondHint;
        string successMsg;
        string emoji;

        if (mode == 0) // Prisma
        {
            int l = isSimplified ? rng.Next(2, 5) : rng.Next(3, 6);
            int w = isSimplified ? rng.Next(2, 4) : rng.Next(2, 5);
            int h = isSimplified ? rng.Next(2, 4) : rng.Next(2, 4);
            expected = l * w * h;
            title = $"Volumen del Prisma ({l} × {w} × {h})";
            emoji = "📦";
            standardDesc = $"Una caja prismática tiene {l} metros de largo, {w} metros de ancho y {h} metros de alto. Aplicando Volumen = largo × ancho × alto ({l} × {w} × {h}), ¿cuál es su volumen en metros cúbicos (m³)?";
            simplifiedDesc = $"Multiplica largo por ancho por alto: {l} × {w} = {l * w}, y luego {l * w} × {h}. ¿Cuánto da el volumen?";
            screenDesc = $"Un prisma rectangular de {l} por {w} por {h} metros con {expected} cubos interiores.";
            firstHint = $"Multiplica las tres dimensiones: primero {l} × {w} = {l * w}, y luego {l * w} × {h}.";
            secondHint = $"{l * w} × {h} = {expected} metros cúbicos.";
            successMsg = $"¡Excelente! {l} × {w} × {h} = {expected} m³ de volumen.";
        }
        else if (mode == 1) // Cubo
        {
            int s = isSimplified ? rng.Next(2, 4) : rng.Next(2, 5);
            expected = s * s * s;
            title = $"Volumen del Cubo ({s} × {s} × {s})";
            emoji = "🧊";
            standardDesc = $"Un contenedor cúbico tiene aristas iguales de {s} metros. Aplicando la fórmula del cubo (Lado³ = {s} × {s} × {s}), ¿cuál es su volumen total en m³?";
            simplifiedDesc = $"Un cubo de {s} por {s} por {s}. Multiplica {s} × {s} = {s * s}, y luego {s * s} × {s}.";
            screenDesc = $"Un cubo perfecto de {s} metros de arista con {expected} unidades cúbicas.";
            firstHint = $"Multiplica {s} por sí mismo tres veces: {s} × {s} × {s}.";
            secondHint = $"{s * s} × {s} = {expected} metros cúbicos.";
            successMsg = $"¡Brillante! {s}³ = {expected} m³ de volumen.";
        }
        else // Base × Altura
        {
            int[] baseAreas = isSimplified ? new[] { 6, 8, 10 } : new[] { 8, 10, 12, 15 };
            int bArea = baseAreas[rng.Next(baseAreas.Length)];
            int h = isSimplified ? rng.Next(2, 4) : rng.Next(2, 5);
            expected = bArea * h;
            title = $"Volumen: Área Base × Altura ({bArea} m² × {h} m)";
            emoji = "💧";
            standardDesc = $"Un depósito de almacenamiento tiene un área de base de {bArea} metros cuadrados (m²) y una altura de {h} metros. ¿Cuál es su capacidad volumétrica total ({bArea} × {h})?";
            simplifiedDesc = $"El depósito tiene {bArea} en la base y {h} pisos de alto. Multiplica {bArea} × {h}.";
            screenDesc = $"Un depósito con base de {bArea} m² y {h} metros de altura.";
            firstHint = $"Multiplica el área de la base por la altura: {bArea} × {h}.";
            secondHint = $"{bArea} × {h} = {expected} metros cúbicos.";
            successMsg = $"¡Exacto! {bArea} m² × {h} m = {expected} m³ de volumen.";
        }

        var ex = new Exercise
        {
            Id = $"gen_vol_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.GeometryVolume3D,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = expected,
            StandardInstruction = standardDesc,
            SimplifiedInstruction = simplifiedDesc,
            ScreenReaderDescription = screenDesc,
            FirstHint = firstHint,
            SecondHintDetailed = secondHint,
            ExplanationOfSuccess = successMsg,
            SimplifiedFirstHint = firstHint,
            SimplifiedSecondHint = secondHint,
            SimplifiedExplanationOfSuccess = successMsg
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = $"item_vol_1",
            Name = title,
            EmojiSymbol = emoji,
            AccessibleDescription = screenDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(4, expected - 15), max: expected + 20, unitLabel: "m³", rng);
        return ex;
    }


    /// <summary>
    /// Genera un ejercicio interactivo de conteo de monedas y cálculo de cambio.
    /// </summary>
    public Exercise GenerateMoneyCurrencyExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 7);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Suma de monedas
        {
            int c5 = rng.Next(1, isSimplified ? 3 : 4);
            int c1 = rng.Next(1, isSimplified ? 4 : 6);
            expected = (c5 * 5) + c1;
            title = $"Contando Monedas: {c5} de $5 y {c1} de $1";
            stdDesc = $"En el monedero hay {c5} moneda(s) de $5 pesos y {c1} moneda(s) de $1 peso. ¿Cuánto dinero hay en total en pesos?";
            simDesc = $"Tienes {c5} moneda(s) de $5 y {c1} de $1. ¿Cuánto dinero suma en total?";
            scrDesc = $"{c5} monedas de 5 pesos y {c1} monedas de 1 peso sobre el mostrador.";
            hint1 = $"Primero multiplica las de $5: {c5} × 5 = {c5 * 5}.";
            hint2 = $"Luego suma las de $1: {c5 * 5} + {c1} = {expected} pesos.";
            success = $"¡Excelente! Tienes exactamente {expected} pesos.";
        }
        else if (mode == 1) // Cambio
        {
            int price = isSimplified ? rng.Next(4, 9) : rng.Next(6, 17);
            int payment = price <= 10 ? 10 : 20;
            expected = payment - price;
            title = $"Calculando Cambio de una Compra de ${price}";
            stdDesc = $"Compras un producto que cuesta ${price} pesos y pagas con una denominación de ${payment} pesos. ¿Cuánto te deben devolver de cambio?";
            simDesc = $"Cuesta ${price} y pagas con ${payment}. Resta {payment} menos {price}. ¿Cuánto sobra?";
            scrDesc = $"Un objeto con precio de {price} pesos y un pago de {payment} pesos.";
            hint1 = $"Resta el costo del dinero entregado: {payment} - {price}.";
            hint2 = $"{payment} menos {price} da {expected} pesos de cambio.";
            success = $"¡Muy bien! Te deben devolver {expected} pesos de cambio.";
        }
        else // Cuánto falta
        {
            int total = isSimplified ? rng.Next(8, 15) : rng.Next(12, 25);
            int have = isSimplified ? rng.Next(3, 7) : rng.Next(5, 12);
            expected = total - have;
            title = $"¿Cuánto Falta para Alcanzar ${total}?";
            stdDesc = $"Deseas comprar un artículo de ${total} pesos y tienes ahorrados ${have} pesos. ¿Cuántos pesos te faltan para poder comprarlo?";
            simDesc = $"Cuesta ${total} y tienes ${have}. Resta {total} menos {have}. ¿Cuánto te falta?";
            scrDesc = $"Meta de ahorro de {total} pesos con {have} pesos reunidos.";
            hint1 = $"Resta lo que tienes de lo que cuesta: {total} - {have}.";
            hint2 = $"{total} menos {have} son {expected} pesos.";
            success = $"¡Exacto! Te faltan {expected} pesos para completar la compra.";
        }

        var ex = new Exercise
        {
            Id = $"gen_money_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.MoneyCurrencyCounting,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_money_1",
            Name = title,
            EmojiSymbol = "🪙",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 5), max: expected + 8, unitLabel: "pesos", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de medición de longitud con regla métrica.
    /// </summary>
    public Exercise GenerateMeasurementRulerExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 7);

        int mode = rng.Next(2);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Medición directa desde el cero
        {
            int length = isSimplified ? rng.Next(3, 8) : rng.Next(4, 15);
            expected = length;
            title = $"Midiendo Longitud: {length} cm";
            stdDesc = $"Un objeto está alineado sobre la regla desde la marca 0 hasta la marca del {length}. ¿Cuántos centímetros (cm) mide el objeto?";
            simDesc = $"El objeto empieza en el 0 y termina en el {length}. ¿Cuántos centímetros mide?";
            scrDesc = $"Un objeto colocado sobre una regla milimétrica alineado de 0 a {length} cm.";
            hint1 = "Mira el número en el que termina el objeto.";
            hint2 = $"El objeto termina exactamente en el {length}, por lo que mide {length} cm.";
            success = $"¡Exacto! El objeto mide {length} centímetros (cm).";
        }
        else // Diferencia de longitud
        {
            int l1 = isSimplified ? rng.Next(6, 10) : rng.Next(8, 16);
            int l2 = isSimplified ? rng.Next(2, 5) : rng.Next(3, 7);
            expected = l1 - l2;
            title = $"Diferencia de Longitud ({l1} cm vs {l2} cm)";
            stdDesc = $"Una pieza de madera mide {l1} cm y otra mide {l2} cm. ¿Cuántos centímetros más larga es la primera pieza que la segunda?";
            simDesc = $"Una mide {l1} cm y la otra mide {l2} cm. Resta {l1} menos {l2}. ¿Por cuánto le gana?";
            scrDesc = $"Dos listones comparados: uno de {l1} cm y otro de {l2} cm.";
            hint1 = $"Resta la pieza corta de la pieza larga: {l1} - {l2}.";
            hint2 = $"{l1} menos {l2} da {expected} cm de diferencia.";
            success = $"¡Brillante! La diferencia es de {expected} centímetros.";
        }

        var ex = new Exercise
        {
            Id = $"gen_ruler_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.MeasurementRulerLength,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_ruler_1",
            Name = title,
            EmojiSymbol = "📏",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 4), max: expected + 6, unitLabel: "cm", rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de lectura del reloj (horas y minutos).
    /// </summary>
    public Exercise GenerateTimeClockExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;
        string unit;

        if (mode == 0) // Hora en punto
        {
            int hour = rng.Next(1, 13);
            expected = hour;
            unit = "en punto";
            title = $"Reloj: Las {hour}:00 en Punto";
            stdDesc = $"La manecilla corta señala el número {hour} y el minutero largo señala exactamente el 12 (:00). ¿Qué hora marca el reloj?";
            simDesc = $"La aguja corta marca el {hour} y la larga está en el 12. ¿Qué hora es?";
            scrDesc = $"Un reloj analógico con la aguja corta en el {hour} y el minutero en el 12.";
            hint1 = "La aguja corta indica la hora. Mira qué número señala.";
            hint2 = $"La aguja corta está en el {hour}, por lo que son las {hour} en punto.";
            success = $"¡Exacto! Son las {hour} en punto (las {hour}:00).";
        }
        else if (mode == 1) // Media hora (:30)
        {
            int hour = rng.Next(1, 13);
            expected = 30;
            unit = "minutos";
            title = $"Reloj: Media Hora Pasada las {hour}";
            stdDesc = $"La manecilla horaria ha pasado las {hour} y el minutero largo apunta hacia abajo al número 6. ¿Cuántos minutos han transcurrido en esa media hora?";
            simDesc = $"El minutero largo apunta al 6 (media hora). ¿Cuántos minutos son?";
            scrDesc = $"Un reloj marcando las {hour} y media con el minutero en el 6.";
            hint1 = "Cada número en el reloj equivale a 5 minutos. El 6 es la mitad de 60.";
            hint2 = "El minutero en el 6 indica exactamente 30 minutos.";
            success = "¡Brillante! Apuntar al 6 indica 30 minutos (media hora).";
        }
        else // Cuarto de hora (:15) o Minutos en 1 hora
        {
            if (isSimplified || rng.Next(2) == 0)
            {
                expected = 15;
                unit = "minutos";
                title = "Reloj: Un Cuarto de Hora";
                stdDesc = "El minutero largo apunta hacia la derecha al número 3 (un cuarto de vuelta). ¿Cuántos minutos representa el número 3?";
                simDesc = "El minutero apunta al 3. Multiplica 3 × 5 minutos. ¿Cuántos minutos son?";
                scrDesc = "Un reloj con la manecilla de minutos en el 3.";
                hint1 = "Multiplica el número 3 por 5 minutos: 3 × 5.";
                hint2 = "3 × 5 = 15 minutos (un cuarto de hora).";
                success = "¡Excelente! Al apuntar al 3 han pasado 15 minutos.";
            }
            else
            {
                expected = 60;
                unit = "minutos";
                title = "Reloj: Vuelta Completa de 1 Hora";
                stdDesc = "El minutero largo recorre los 12 números de la esfera dando una vuelta completa. ¿Cuántos minutos tiene 1 hora entera?";
                simDesc = "Una hora completa, ¿cuántos minutos tiene en total?";
                scrDesc = "Un reloj mostrando el recorrido de una hora completa de 60 minutos.";
                hint1 = "Multiplica los 12 números por 5 minutos: 12 × 5.";
                hint2 = "Una hora completa tiene 60 minutos.";
                success = "¡Extraordinario! 1 hora equivale a 60 minutos completos.";
            }
        }

        var ex = new Exercise
        {
            Id = $"gen_clock_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.TimeClockReading,
            TargetAgeMin = 7,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_clock_1",
            Name = title,
            EmojiSymbol = "⏰",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 10), max: expected + 15, unitLabel: unit, rng);
        return ex;
    }

    /// <summary>
    /// Genera un ejercicio interactivo de perímetro de polígonos (triángulo, rectángulo, cuadrado).
    /// </summary>
    public Exercise GeneratePerimeterExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Triángulo
        {
            int side = isSimplified ? rng.Next(3, 7) : rng.Next(4, 9);
            expected = side * 3;
            title = $"Perímetro de Triángulo Equilátero ({side} m)";
            stdDesc = $"Un parque triangular tiene 3 lados iguales que miden {side} metros cada uno. ¿Cuál es su perímetro total sumando los tres lados ({side} + {side} + {side})?";
            simDesc = $"Un triángulo con 3 lados de {side} metros. Suma {side} + {side} + {side}. ¿Cuánto da?";
            scrDesc = $"Un triángulo con tres lados iguales de {side} metros cada uno.";
            hint1 = $"Suma los tres lados: {side} + {side} + {side}.";
            hint2 = $"3 lados de {side} m suman {expected} metros de perímetro.";
            success = $"¡Excelente! El perímetro del triángulo es {expected} metros.";
        }
        else if (mode == 1) // Rectángulo
        {
            int b = isSimplified ? rng.Next(4, 8) : rng.Next(5, 10);
            int h = isSimplified ? rng.Next(2, 5) : rng.Next(3, 7);
            expected = (2 * b) + (2 * h);
            title = $"Perímetro del Rectángulo ({b} m × {h} m)";
            stdDesc = $"Un terreno rectangular tiene dos lados de {b} metros y dos lados de {h} metros. ¿Cuál es su perímetro exterior ({b} + {h} + {b} + {h})?";
            simDesc = $"Suma los 4 lados del rectángulo: {b} + {h} + {b} + {h}. ¿Cuánto da?";
            scrDesc = $"Un rectángulo con dos lados largos de {b} m y dos lados cortos de {h} m.";
            hint1 = $"Suma los 4 lados: {b} + {h} + {b} + {h}.";
            hint2 = $"2 lados de {b} ({2 * b}) más 2 lados de {h} ({2 * h}) suman {expected} m.";
            success = $"¡Exacto! El perímetro del rectángulo es {expected} metros.";
        }
        else // Cuadrado
        {
            int s = isSimplified ? rng.Next(3, 7) : rng.Next(4, 9);
            expected = s * 4;
            title = $"Perímetro del Cuadrado ({s} m por lado)";
            stdDesc = $"Un patio cuadrado tiene 4 lados iguales que miden {s} metros cada uno. ¿Cuántos metros de cerca se necesitan para rodear todo su perímetro (4 × {s})?";
            simDesc = $"Un cuadrado tiene 4 lados de {s} metros. Multiplica 4 × {s}. ¿Cuál es el perímetro?";
            scrDesc = $"Un cuadrado con cuatro lados iguales de {s} metros cada uno.";
            hint1 = $"Multiplica los 4 lados por {s}: 4 × {s}.";
            hint2 = $"4 lados de {s} metros suman {expected} metros de cerca.";
            success = $"¡Brillante! 4 × {s} = {expected} metros de perímetro.";
        }

        var ex = new Exercise
        {
            Id = $"gen_perim_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.GeometryPerimeter,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_perim_1",
            Name = title,
            EmojiSymbol = "📐",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(4, expected - 8), max: expected + 12, unitLabel: "m", rng);
        return ex;
    }


    /// <summary>
    /// Genera ejercicios de división con residuo (inexacta) D = d * q + r.
    /// </summary>
    public Exercise GenerateDivisionRemainderExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int divisor = isSimplified ? rng.Next(3, 6) : rng.Next(3, 8);
        int quotient = isSimplified ? rng.Next(2, 5) : rng.Next(2, 7);
        int remainder = rng.Next(1, divisor); // Siempre 1 <= r < divisor
        int dividend = (divisor * quotient) + remainder;

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Preguntar por el Residuo
        {
            expected = remainder;
            title = $"Reparto de {dividend} Galletas entre {divisor} Amigos";
            stdDesc = $"Cosmo tiene {dividend} galletas y las reparte en partes iguales entre {divisor} amigos. A cada amigo le tocan {quotient} galletas ({divisor} × {quotient} = {divisor * quotient}). ¿Cuántas galletas sobran de residuo?";
            simDesc = $"Tienes {dividend} galletas y diste {divisor * quotient}. Resta {dividend} menos {divisor * quotient}. ¿Cuántas sobran?";
            scrDesc = $"{dividend} galletas para repartir equitativamente entre {divisor} amigos; sobran galletas.";
            hint1 = $"Multiplica lo repartido: {divisor} × {quotient} = {divisor * quotient}.";
            hint2 = $"Resta del total: {dividend} - {divisor * quotient} = {remainder} galletas de residuo.";
            success = $"¡Correcto! Sobran {remainder} galletas de residuo.";
        }
        else if (mode == 1) // Preguntar por el Cociente
        {
            expected = quotient;
            title = $"Reparto Justo: {dividend} Monedas entre {divisor} Exploradores";
            stdDesc = $"Repartes {dividend} monedas entre {divisor} exploradores de forma justa y sobran {remainder} monedas. ¿Cuántas monedas completas le tocan a cada explorador ({divisor} × ? se acerca a {dividend - remainder})?";
            simDesc = $"Repartes {dividend} monedas entre {divisor} amigos y sobran {remainder}. ¿Cuántas monedas le tocan a cada amigo?";
            scrDesc = $"{dividend} monedas repartidas entre {divisor} exploradores; quedan {remainder} sobrantes.";
            hint1 = $"Resta lo que sobra: {dividend} - {remainder} = {dividend - remainder}.";
            hint2 = $"Divide lo que se repartió: {dividend - remainder} entre {divisor} = {quotient} monedas.";
            success = $"¡Exacto! A cada explorador le tocan {quotient} monedas completas.";
        }
        else // Comprobación: D = d * q + r
        {
            expected = dividend;
            title = $"Comprobando la División: (Divisor × Cociente) + Residuo";
            stdDesc = $"Al repartir manzanas entre {divisor} cestas, cupieron {quotient} en cada una y sobraron {remainder}. Usando la fórmula ({divisor} × {quotient}) + {remainder}, ¿cuántas manzanas había en total?";
            simDesc = $"Multiplica {divisor} × {quotient} = {divisor * quotient}, y súmale lo que sobró (+ {remainder}). ¿Cuánto da?";
            scrDesc = $"Comprobación de división: {divisor} por {quotient} más residuo de {remainder}.";
            hint1 = $"Multiplica {divisor} × {quotient} = {divisor * quotient}.";
            hint2 = $"Suma el residuo {remainder}: {divisor * quotient} + {remainder} = {dividend}.";
            success = $"¡Extraordinario! ({divisor} × {quotient}) + {remainder} = {dividend}.";
        }

        var ex = new Exercise
        {
            Id = $"gen_divrem_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.DivisionWithRemainder,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_divrem_1",
            Name = title,
            EmojiSymbol = "🍪",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 3), max: expected + 4, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de fracciones en la recta numérica.
    /// </summary>
    public Exercise GenerateFractionsNumberLineExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int[] possibleDenoms = isSimplified ? new[] { 2, 4 } : new[] { 2, 3, 4, 5, 6 };
        int denominator = possibleDenoms[rng.Next(possibleDenoms.Length)];
        int numerator = rng.Next(1, denominator);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Saltos en la recta
        {
            expected = numerator;
            title = $"Saltos en la Recta: Fracción {numerator}/{denominator}";
            stdDesc = $"El camino de 0 a 1 está dividido en {denominator} partes iguales de 1/{denominator} cada una. Si das {numerator} saltos desde el 0, ¿cuántos pasos fraccionarios avanzaste en el numerador de la fracción ?/{denominator}?";
            simDesc = $"El camino tiene {denominator} partes iguales. Das {numerator} saltos. ¿Cuál es el número de saltos dados?";
            scrDesc = $"Una recta numérica de 0 a 1 con divisiones de 1/{denominator} y una marca a los {numerator} saltos.";
            hint1 = $"Cada salto cuenta una parte del camino (1/{denominator}).";
            hint2 = $"Diste {numerator} saltos, por lo que avanzaste {numerator}/{denominator}.";
            success = $"¡Exacto! Llegaste a la marca de {numerator}/{denominator} dando {numerator} saltos.";
        }
        else if (mode == 1) // Comparación con la mitad (o posición)
        {
            expected = numerator;
            title = $"Ubicación de {numerator}/{denominator} en la Recta";
            stdDesc = $"En una pista graduada de 0 a 1 con marcas cada 1/{denominator}, una nave recorre {numerator} partes de {denominator}. ¿Qué numerador marca la posición del vehículo en la pista?";
            simDesc = $"La nave avanzó {numerator} de las {denominator} partes de la pista. ¿Cuál es el número de partes recorridas?";
            scrDesc = $"Pista numérica con {denominator} segmentos iguales y la nave en la marca {numerator}.";
            hint1 = $"Cuenta los segmentos recorridos desde la línea de salida (el 0).";
            hint2 = $"La nave se encuentra en la posición {numerator}/{denominator}.";
            success = $"¡Brillante! La posición correcta es {numerator}/{denominator}.";
        }
        else // Partes totales de la unidad
        {
            expected = denominator;
            title = $"Partes del Camino Completo (1/{denominator})";
            stdDesc = $"Un sendero completo de 0 a 1 se corta en partes iguales para colocar marcas de fracción. Si cada paso mide 1/{denominator}, ¿en cuántas partes iguales en total está dividida la unidad?";
            simDesc = $"Cada parte mide 1/{denominator}. ¿En cuántas partes iguales se cortó todo el camino?";
            scrDesc = $"Una recta unitaria cortada en {denominator} segmentos de igual longitud.";
            hint1 = $"El denominador (número inferior) indica las partes totales en que se divide el entero.";
            hint2 = $"Como la fracción es 1/{denominator}, el camino se dividió en {denominator} partes.";
            success = $"¡Excelente! El denominador {denominator} indica que la recta se dividió en {denominator} partes.";
        }

        var ex = new Exercise
        {
            Id = $"gen_fracline_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.FractionsNumberLine,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_fracline_1",
            Name = title,
            EmojiSymbol = "📍",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 3), max: expected + 3, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de lectura de tablas de frecuencia y gráficas de barras.
    /// </summary>
    public Exercise GenerateDataBarChartsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int v1 = isSimplified ? rng.Next(3, 7) : rng.Next(3, 9);
        int v2 = isSimplified ? rng.Next(6, 10) : rng.Next(7, 12);
        int v3 = isSimplified ? rng.Next(2, 5) : rng.Next(2, 6);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Lectura directa del valor mayor
        {
            expected = v2;
            title = $"Lectura de Barra: La Mayor Frecuencia ({v2} votos)";
            stdDesc = $"En una encuesta escolar: Robótica = {v1} votos, Astronomía = {v2} votos, Música = {v3} votos. En la gráfica de barras, ¿cuál es la altura (número de votos) alcanzada por la barra más alta (Astronomía)?";
            simDesc = $"Astronomía tiene {v2} votos y es la barra más alta. ¿Hasta qué número llega su barra?";
            scrDesc = $"Gráfica de barras: Robótica {v1}, Astronomía {v2}, Música {v3}.";
            hint1 = $"Observa la columna de Astronomía en el eje horizontal.";
            hint2 = $"La altura de la barra de Astronomía llega exactamente al número {v2}.";
            success = $"¡Correcto! La barra más alta alcanza una frecuencia de {v2} votos.";
        }
        else if (mode == 1) // Diferencia entre barra mayor y menor
        {
            expected = v2 - v3;
            title = $"Comparación de Barras: Diferencia entre Mayor y Menor";
            stdDesc = $"La barra más alta tiene {v2} votos (Astronomía) y la más baja tiene {v3} votos (Música). ¿Cuántos votos más obtuvo la actividad más popular que la menos popular ({v2} - {v3})?";
            simDesc = $"Resta la barra alta ({v2}) menos la barra baja ({v3}). ¿Cuánto da {v2} - {v3}?";
            scrDesc = $"Comparación de barras: columna de {v2} frente a columna de {v3}.";
            hint1 = $"Resta la menor de la mayor: {v2} - {v3}.";
            hint2 = $"{v2} menos {v3} da una diferencia de {expected} votos.";
            success = $"¡Exacto! {v2} - {v3} = {expected} votos de diferencia.";
        }
        else // Total de datos encuestados
        {
            expected = v1 + v2 + v3;
            title = $"Total de Participantes en la Tabla de Frecuencias";
            stdDesc = $"Una tabla registra las respuestas de los estudiantes: Robótica = {v1}, Astronomía = {v2}, Música = {v3}. ¿Cuántos estudiantes participaron en total sumando las tres categorías ({v1} + {v2} + {v3})?";
            simDesc = $"Suma todos los votos: {v1} + {v2} + {v3}. ¿Cuántos son en total?";
            scrDesc = $"Tabla de conteo con {v1}, {v2} y {v3} participantes por categoría.";
            hint1 = $"Suma los tres valores: primero {v1} + {v2} = {v1 + v2}, y luego añade {v3}.";
            hint2 = $"{v1} + {v2} + {v3} = {expected} participantes en total.";
            success = $"¡Extraordinario! Participaron {expected} estudiantes en la encuesta.";
        }

        var ex = new Exercise
        {
            Id = $"gen_barchart_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.DataBarChartsAndTables,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_barchart_1",
            Name = title,
            EmojiSymbol = "📊",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 4), max: expected + 5, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de medidas de tendencia central (media, mediana, moda).
    /// </summary>
    public Exercise GenerateStatisticsCentralTendencyExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 8);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Media aritmética (promedio de 3 números con suma múltiplo de 3)
        {
            int meanVal = isSimplified ? rng.Next(5, 9) : rng.Next(6, 12);
            int diff = rng.Next(1, 4);
            int n1 = meanVal - diff;
            int n2 = meanVal;
            int n3 = meanVal + diff;
            int sum = n1 + n2 + n3;

            expected = meanVal;
            title = $"Cálculo de la Media Aritmética (Promedio)";
            stdDesc = $"Las calificaciones obtenidas en tres misiones son: {n1}, {n2} y {n3}. La suma total es {sum} ({n1} + {n2} + {n3} = {sum}). Para encontrar la Media (promedio), divide {sum} entre 3. ¿Cuál es el promedio?";
            simDesc = $"Suma {n1} + {n2} + {n3} = {sum}. Divide {sum} entre 3. ¿Cuánto da el promedio?";
            scrDesc = $"Tres calificaciones: {n1}, {n2} y {n3} cuya suma es {sum}.";
            hint1 = $"Divide la suma total entre la cantidad de datos: {sum} / 3.";
            hint2 = $"{sum} dividido entre 3 es igual a {meanVal}.";
            success = $"¡Excelente! El promedio (media) es {meanVal}.";
        }
        else if (mode == 1) // Moda (el valor más repetido)
        {
            int modeVal = isSimplified ? rng.Next(3, 8) : rng.Next(4, 11);
            int other1 = (modeVal > 5) ? modeVal - 2 : modeVal + 2;
            int other2 = (modeVal > 5) ? modeVal - 1 : modeVal + 3;

            expected = modeVal;
            title = $"Identificación de la Moda en el Conjunto de Datos";
            stdDesc = $"En una competencia se registraron los siguientes puntajes: [{other1}, {modeVal}, {modeVal}, {modeVal}, {other2}]. La Moda es el valor que aparece con mayor frecuencia. ¿Cuál es la moda?";
            simDesc = $"En la lista [{other1}, {modeVal}, {modeVal}, {modeVal}, {other2}], ¿cuál es el número que más veces se repite?";
            scrDesc = $"Lista de números con {modeVal} repetido 3 veces y otros valores con 1 repetición.";
            hint1 = $"Cuenta cuántas veces aparece cada número en la lista.";
            hint2 = $"El número {modeVal} aparece 3 veces, superando a todos los demás.";
            success = $"¡Brillante! La moda es {modeVal} porque es el número más repetido.";
        }
        else // Mediana (el valor central de 5 datos ordenados)
        {
            int start = isSimplified ? rng.Next(2, 6) : rng.Next(2, 8);
            int step = rng.Next(1, 3);
            int m1 = start;
            int m2 = m1 + step;
            int m3 = m2 + step;
            int m4 = m3 + step;
            int m5 = m4 + step;

            expected = m3;
            title = $"Localización de la Mediana en Datos Ordenados";
            stdDesc = $"Cinco datos están ordenados de menor a mayor: [{m1}, {m2}, {m3}, {m4}, {m5}]. La Mediana es el número que ocupa exactamente la posición del medio. ¿Cuál es la mediana?";
            simDesc = $"En la fila [{m1}, {m2}, {m3}, {m4}, {m5}], ¿cuál número está exactamente en el centro?";
            scrDesc = $"Cinco datos ordenados en fila; el tercer valor está en la posición central.";
            hint1 = $"Hay 5 números ordenados. El dato central es el que ocupa el tercer lugar.";
            hint2 = $"El número en la tercera posición es {m3}, con dos datos a su izquierda y dos a su derecha.";
            success = $"¡Extraordinario! La mediana es {m3} porque está justo al centro.";
        }

        var ex = new Exercise
        {
            Id = $"gen_statscen_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.StatisticsCentralTendency,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_statscen_1",
            Name = title,
            EmojiSymbol = "🎯",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 3), max: expected + 4, unitLabel: "", rng);
        return ex;
    }


    /// <summary>
    /// Genera ejercicios de geometría del círculo, radio, diámetro y constante pi.
    /// </summary>
    public Exercise GenerateCircleGeometryExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 11);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Diámetro desde Radio: d = 2r
        {
            int r = isSimplified ? rng.Next(3, 8) : rng.Next(4, 12);
            expected = 2 * r;
            title = $"Diámetro de una Rueda Circular (Radio {r} m)";
            stdDesc = $"Una compuerta circular tiene un radio de {r} metros del centro al borde. Sabiendo que el diámetro cruza completo de lado a lado pasando por el centro (d = 2 × r), ¿cuánto mide el diámetro?";
            simDesc = $"El radio mide {r} metros. El diámetro es el doble (2 × {r}). ¿Cuánto mide el diámetro?";
            scrDesc = $"Círculo con radio de {r} metros y diámetro horizontal.";
            hint1 = $"El diámetro es el doble del radio: d = 2 × {r}.";
            hint2 = $"2 por {r} es igual a {expected} metros de diámetro.";
            success = $"¡Correcto! Diámetro = 2 × {r} m = {expected} metros.";
        }
        else if (mode == 1) // Circunferencia en cm: C = 3.14 × d_cm (d = 100 cm -> C = 314 cm, o d = 50 -> 157, etc.)
        {
            int factor = isSimplified ? rng.Next(1, 3) * 100 : rng.Next(1, 5) * 100; // 100, 200, 300 cm
            expected = (int)(3.14 * factor); // 314, 628, 942
            title = $"Circunferencia con Diámetro {factor} cm (C = π × d)";
            stdDesc = $"Una rueda circular tiene un diámetro de {factor} centímetros. Usando la aproximación π ≈ 3.14 en la fórmula de la circunferencia C = π × d, ¿cuántos centímetros mide todo el contorno (3.14 × {factor} cm)?";
            simDesc = $"Multiplica 3.14 por {factor} para medir el contorno de la rueda en centímetros. ¿Cuánto da?";
            scrDesc = $"Pista circular de {factor} centímetros de diámetro y perímetro resaltado.";
            hint1 = $"Multiplica pi por el diámetro: 3.14 × {factor}.";
            hint2 = $"3.14 × {factor} = {expected} centímetros de circunferencia.";
            success = $"¡Exacto! C = 3.14 × {factor} = {expected} centímetros de contorno.";
        }
        else // Radio desde Diámetro: r = d / 2
        {
            int r = isSimplified ? rng.Next(3, 8) : rng.Next(4, 14);
            int d = 2 * r;
            expected = r;
            title = $"Radio del Domo Circular (Diámetro {d} m)";
            stdDesc = $"Un domo espacial tiene un diámetro exterior de {d} metros. Sabiendo que el radio va del centro al borde y mide la mitad exacta (r = d / 2), ¿cuál es el radio?";
            simDesc = $"El diámetro mide {d} metros. Divide {d} entre 2 para hallar el radio. ¿Cuánto da?";
            scrDesc = $"Domo circular con diámetro de {d} metros; se pide la mitad.";
            hint1 = $"El radio es la mitad del diámetro: {d} / 2.";
            hint2 = $"{d} dividido entre 2 es igual a {expected} metros.";
            success = $"¡Brillante! Radio = {d} / 2 = {expected} metros.";
        }

        var ex = new Exercise
        {
            Id = $"gen_circle_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.CircleGeometryPi,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_circle_1",
            Name = title,
            EmojiSymbol = "⭕",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        if (mode == 1)
        {
            ex.Options.Add(new ExerciseOption { Id = "opt_gc_1", Label = $"{expected - 100} cm", Value = expected - 100, IsCorrect = false, SpeechFeedback = "Incorrecto.", AccessibleKeyShortcut = "1" });
            ex.Options.Add(new ExerciseOption { Id = "opt_gc_2", Label = $"{expected} cm", Value = expected, IsCorrect = true, SpeechFeedback = $"¡Correcto! {expected} centímetros.", AccessibleKeyShortcut = "2" });
            ex.Options.Add(new ExerciseOption { Id = "opt_gc_3", Label = $"{expected + 100} cm", Value = expected + 100, IsCorrect = false, SpeechFeedback = "Demasiado grande.", AccessibleKeyShortcut = "3" });
            ex.Options.Add(new ExerciseOption { Id = "opt_gc_4", Label = $"{expected * 2} cm", Value = expected * 2, IsCorrect = false, SpeechFeedback = "Ese es el doble.", AccessibleKeyShortcut = "4" });
        }
        else
        {
            BuildOptions(ex, expected, min: Math.Max(2, expected - 4), max: expected + 5, unitLabel: "m", rng);
        }

        return ex;
    }

    /// <summary>
    /// Genera ejercicios de Mínimo Común Múltiplo (MCM) y Máximo Común Divisor (MCD).
    /// </summary>
    public Exercise GenerateFactorsMcmMcdExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 11);

        int mode = rng.Next(2); // 0 = MCM, 1 = MCD
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // MCM
        {
            var pairs = new (int a, int b, int mcm)[]
            {
                (4, 6, 12),
                (3, 5, 15),
                (6, 8, 24),
                (4, 10, 20),
                (3, 4, 12),
                (6, 9, 18)
            };
            var p = pairs[rng.Next(pairs.Length)];
            expected = p.mcm;
            title = $"Mínimo Común Múltiplo (MCM) de {p.a} y {p.b}";
            stdDesc = $"Dos faros espaciales emiten destellos con periodos distintos: el faro A cada {p.a} minutos y el faro B cada {p.b} minutos. ¿En qué minuto exacto coincidirán ambos por primera vez (MCM de {p.a} y {p.b})?";
            simDesc = $"El faro A parpadea cada {p.a} min y el B cada {p.b} min. ¿En qué número coinciden primero las tablas de multiplicar?";
            scrDesc = $"Secuencias de múltiplos de {p.a} y {p.b} coincidiendo en {p.mcm}.";
            hint1 = $"Escribe los múltiplos de {p.a} y los de {p.b}.";
            hint2 = $"El menor múltiplo común a ambos es {expected}.";
            success = $"¡Exacto! El MCM({p.a}, {p.b}) es {expected}.";
        }
        else // MCD
        {
            var pairs = new (int a, int b, int mcd)[]
            {
                (12, 18, 6),
                (20, 30, 10),
                (16, 24, 8),
                (15, 25, 5),
                (14, 21, 7),
                (18, 27, 9)
            };
            var p = pairs[rng.Next(pairs.Length)];
            expected = p.mcd;
            title = $"Máximo Común Divisor (MCD) de {p.a} y {p.b}";
            stdDesc = $"Queremos empaquetar {p.a} cristales azules y {p.b} cristales rojos en cajas iguales con la mayor cantidad posible sin que sobre ninguno. ¿Cuántos cristales debe llevar cada caja (MCD de {p.a} y {p.b})?";
            simDesc = $"¿Cuál es el número más grande que divide exactamente a {p.a} y a {p.b} sin que sobre nada?";
            scrDesc = $"Divisores de {p.a} y {p.b} destacando el divisor común máximo {p.mcd}.";
            hint1 = $"Busca los divisores comunes de {p.a} y {p.b}.";
            hint2 = $"El divisor común más grande es {expected}.";
            success = $"¡Brillante! El MCD({p.a}, {p.b}) es {expected}.";
        }

        var ex = new Exercise
        {
            Id = $"gen_mcmmcd_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.FactorsMcmMcd,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_mcmmcd_1",
            Name = title,
            EmojiSymbol = "💡",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(2, expected - 4), max: expected + 6, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de números racionales con signo (fracciones y decimales negativos).
    /// </summary>
    public Exercise GenerateNegativeRationalNumbersExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 11);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Centésimos bajo cero: -1/2 = -50, -1/4 = -25, -3/4 = -75
        {
            var fracs = new (string frac, int cents, string dec)[]
            {
                ("-1/2", -50, "-0.50"),
                ("-1/4", -25, "-0.25"),
                ("-3/4", -75, "-0.75"),
                ("-1/5", -20, "-0.20")
            };
            var f = fracs[rng.Next(fracs.Length)];
            expected = f.cents;
            title = $"Equivalencia en Centésimos de {f.frac}";
            stdDesc = $"En la escala bajo cero, 1 grado son 100 centésimos. ¿A cuántos centésimos bajo cero ({f.dec}) equivale la fracción {f.frac}?";
            simDesc = $"¿A cuántos centésimos bajo cero con signo menos equivale {f.frac}?";
            scrDesc = $"Recta numérica con la fracción {f.frac} a la izquierda del cero marcando {f.dec}.";
            hint1 = $"Divide 100 entre el denominador y coloca signo negativo.";
            hint2 = $"{f.frac} equivale exactamente a {expected} centésimos ({f.dec}).";
            success = $"¡Exacto! {f.frac} = {expected} centésimos ({f.dec}).";
        }
        else if (mode == 1) // Ley de signos en multiplicación: (-2) × (-d) = +(2 × d)
        {
            int a = rng.Next(2, 6);
            int b = rng.Next(2, 6);
            expected = a * b;
            title = $"Multiplicación con Ley de Signos: (-{a}) × (-{b})";
            stdDesc = $"Aplica la ley de los signos: Menos por menos da más ((-) × (-) = (+)). ¿Cuál es el resultado de multiplicar (-{a}) × (-{b})?";
            simDesc = $"Menos por menos da más. Multiplica {a} × {b}. ¿Cuánto da positivo?";
            scrDesc = $"Operación (-{a}) × (-{b}) cancelando signos negativos.";
            hint1 = $"El signo final es positivo: menos por menos es más (+).";
            hint2 = $"{a} × {b} = {expected}.";
            success = $"¡Brillante! (-{a}) × (-{b}) = +{expected}.";
        }
        else // Suma con signos opuestos
        {
            int neg = rng.Next(1, 4);
            int pos = rng.Next(neg + 1, neg + 5);
            expected = pos - neg;
            title = $"Suma de Racionales con Signos Opuestos: (-{neg}/2) + ({pos}/2)";
            stdDesc = $"Calcula la suma de fracciones con signos opuestos: (-{neg}/2) + ({pos}/2). Restamos los numeradores ({pos} - {neg} = {pos - neg}). ¿A cuántos medios ({pos - neg}/2) equivale el resultado?";
            simDesc = $"Debes {neg} medios y pagas {pos} medios. ¿Cuántos medios te quedan a favor?";
            scrDesc = $"Operación (-{neg}/2) + ({pos}/2) = +{pos - neg}/2.";
            hint1 = $"Conserva el denominador 2 y suma los numeradores: -{neg} + {pos} = {pos - neg}.";
            hint2 = $"El resultado es {pos - neg} medios.";
            success = $"¡Extraordinario! (-{neg}/2) + ({pos}/2) = +{pos - neg}/2.";
        }

        var ex = new Exercise
        {
            Id = $"gen_negrat_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.NegativeRationalNumbers,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_negrat_1",
            Name = title,
            EmojiSymbol = "❄️",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        if (mode == 0)
        {
            ex.Options.Add(new ExerciseOption { Id = "opt_nr_1", Label = $"{expected - 25} centésimos", Value = expected - 25, IsCorrect = false, SpeechFeedback = "Demasiado negativo.", AccessibleKeyShortcut = "1" });
            ex.Options.Add(new ExerciseOption { Id = "opt_nr_2", Label = $"{expected} centésimos", Value = expected, IsCorrect = true, SpeechFeedback = $"¡Correcto! {expected} centésimos.", AccessibleKeyShortcut = "2" });
            ex.Options.Add(new ExerciseOption { Id = "opt_nr_3", Label = $"{Math.Abs(expected)} centésimos (+)", Value = Math.Abs(expected), IsCorrect = false, SpeechFeedback = "Ese es positivo, pero la fracción era negativa.", AccessibleKeyShortcut = "3" });
            ex.Options.Add(new ExerciseOption { Id = "opt_nr_4", Label = $"{expected + 25} centésimos", Value = expected + 25, IsCorrect = false, SpeechFeedback = "Incorrecto.", AccessibleKeyShortcut = "4" });
        }
        else
        {
            BuildOptions(ex, expected, min: Math.Max(1, expected - 3), max: expected + 4, unitLabel: "", rng);
        }

        return ex;
    }

/// <summary>
    /// Genera ejercicios de factorización algebraica (factor común y diferencia de cuadrados).
    /// </summary>
    public Exercise GenerateAlgebraFactoringExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 12);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Factor común numérico: k*x + k*b = k(x + b)
        {
            int k = isSimplified ? rng.Next(2, 5) : rng.Next(2, 8);
            int b = isSimplified ? rng.Next(2, 5) : rng.Next(2, 6);
            int kb = k * b;
            expected = k;
            title = $"Factor Común Monomio en {k}x + {kb}";
            stdDesc = $"En la expresión algebraica {k}x + {kb}, ambos términos se pueden dividir de forma exacta entre el mismo número. ¿Cuál es el factor común que se coloca fuera del paréntesis para multiplicar a (x + {b})?";
            simDesc = $"¿Qué número divide a {k} y a {kb} para ponerlo afuera del paréntesis?";
            scrDesc = $"Expresión algebraica {k}x + {kb} factorizada con factor común que multiplica a x + {b}.";
            hint1 = $"Busca el Máximo Común Divisor entre {k} y {kb}.";
            hint2 = $"{k} divide a {k}x y a {kb} ({kb} / {k} = {b}). El factor común es {k}.";
            success = $"¡Correcto! {k}x + {kb} = {k}(x + {b}). El factor común es {k}.";
        }
        else if (mode == 1) // Diferencia de cuadrados: x^2 - a^2 = (x - a)(x + a)
        {
            int a = isSimplified ? rng.Next(2, 6) : rng.Next(2, 10);
            int a2 = a * a;
            expected = a;
            title = $"Diferencia de Cuadrados: x² - {a2}";
            stdDesc = $"La expresión x² - {a2} es una diferencia de cuadrados que se factoriza en binomios conjugados (x - a)(x + a). Sabiendo que la raíz cuadrada de {a2} es √{a2}, ¿qué número completa ambos binomios?";
            simDesc = $"Calcula la raíz cuadrada de {a2} (¿qué número por sí mismo da {a2}?).";
            scrDesc = $"Diferencia de cuadrados x² - {a2} factorizada en binomios conjugados de x y término faltante.";
            hint1 = $"Calcula la raíz cuadrada de {a2}: √{a2}.";
            hint2 = $"{a} × {a} = {a2}, por lo que la factorización es (x - {a})(x + {a}).";
            success = $"¡Exacto! x² - {a2} = (x - {a})(x + {a}). La raíz es {a}.";
        }
        else // Término interior: k*x + k*b = k(x + ?)
        {
            int k = isSimplified ? rng.Next(2, 5) : rng.Next(3, 7);
            int b = isSimplified ? rng.Next(2, 5) : rng.Next(2, 7);
            int kb = k * b;
            expected = b;
            title = $"Término Interior del Paréntesis: {k}x + {kb} = {k}(x + ?)";
            stdDesc = $"Al extraer el factor común {k} de la expresión {k}x + {kb}, dividimos el segundo término entre {k} ({kb} / {k}). ¿Qué número queda dentro del paréntesis en la forma {k}(x + término faltante)?";
            simDesc = $"Divide {kb} entre {k}. ¿Qué número queda dentro del paréntesis?";
            scrDesc = $"Expresión {k}x + {kb} factorizada extrayendo {k} para obtener {k} que multiplica a x más un término.";
            hint1 = $"Divide el segundo término entre el factor común: {kb} / {k}.";
            hint2 = $"{kb} dividido entre {k} es igual a {b}.";
            success = $"¡Brillante! {k}x + {kb} = {k}(x + {b}). Queda el número {b}.";
        }

        var ex = new Exercise
        {
            Id = $"gen_fact_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.AlgebraFactoring,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_fact_1",
            Name = title,
            EmojiSymbol = "🧩",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 3), max: expected + 4, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de ángulos entre paralelas cortadas por una secante.
    /// </summary>
    public Exercise GenerateParallelLinesAnglesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 12);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Ángulos alternos internos (iguales)
        {
            int ang = isSimplified ? rng.Next(4, 8) * 10 : rng.Next(7, 15) * 5; // e.g. 40, 50, 65, 70...
            expected = ang;
            title = $"Ángulo Alterno Interno ({ang}°)";
            stdDesc = $"Dos rectas paralelas son cortadas por una secante transversal. Si uno de los ángulos alternos internos mide {ang}°, por propiedad de paralelismo, ¿cuánto mide el otro ángulo alterno interno?";
            simDesc = $"Los ángulos alternos son gemelos idénticos. Si uno mide {ang} grados, ¿cuánto mide el otro?";
            scrDesc = $"Líneas paralelas cortadas por una secante con ángulos alternos de {ang} grados.";
            hint1 = $"Los ángulos alternos internos son congruentes (miden exactamente lo mismo).";
            hint2 = $"Mide exactamente {ang}°.";
            success = $"¡Correcto! Los ángulos alternos internos son congruentes y miden {ang}°.";
        }
        else if (mode == 1) // Ángulos conjugados suplementarios (suman 180°)
        {
            int ang = isSimplified ? rng.Next(5, 9) * 10 : rng.Next(8, 15) * 5; // e.g. 60, 70, 75...
            expected = 180 - ang;
            title = $"Ángulo Suplementario Conjugado ({ang}° + ? = 180°)";
            stdDesc = $"En una intersección de paralelas, un ángulo interno mide {ang}°. El ángulo conjugado interno situado de su mismo lado es suplementario y debe sumar 180° ({ang}° + ? = 180°). ¿Cuánto mide?";
            simDesc = $"Resta 180 menos {ang} para hallar el ángulo que falta. ¿Cuánto da?";
            scrDesc = $"Dos ángulos conjugados sobre paralelas que suman 180 grados.";
            hint1 = $"Resta el ángulo conocido de 180°: 180 - {ang}.";
            hint2 = $"180 - {ang} = {expected}°.";
            success = $"¡Exacto! {ang}° + {expected}° = 180° de suma suplementaria.";
        }
        else // Opuestos por el vértice (iguales)
        {
            int ang = isSimplified ? rng.Next(3, 8) * 10 : rng.Next(6, 16) * 5; // e.g. 45, 55, 60...
            expected = ang;
            title = $"Ángulo Opuesto por el Vértice ({ang}°)";
            stdDesc = $"Dos rectas se cortan en un vértice formando cuatro ángulos. Si uno de los ángulos mide {ang}°, ¿cuánto mide el ángulo que se encuentra justo frente a él cruzando el vértice?";
            simDesc = $"Los ángulos opuestos frente a frente miden lo mismo. Si uno mide {ang}°, ¿cuánto mide el de enfrente?";
            scrDesc = $"Cruce de dos rectas con dos ángulos opuestos por el vértice de {ang} grados.";
            hint1 = $"Los ángulos opuestos por el vértice son congruentes.";
            hint2 = $"Mide exactamente lo mismo: {ang}°.";
            success = $"¡Brillante! Los ángulos opuestos por el vértice miden {ang}°.";
        }

        var ex = new Exercise
        {
            Id = $"gen_parall_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.ParallelLinesAngles,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_parall_1",
            Name = title,
            EmojiSymbol = "📐",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(15, expected - 25), max: expected + 30, unitLabel: "°", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de probabilidad clásica y eventos compuestos.
    /// </summary>
    public Exercise GenerateProbabilityCompoundEventsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 12);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Probabilidad clásica en porcentaje (Laplace): Urna con rojas y azules
        {
            var cases = new (int rojas, int azules, int pct)[]
            {
                (4, 6, 40),
                (2, 8, 20),
                (5, 5, 50),
                (3, 7, 30),
                (7, 3, 70),
                (1, 9, 10)
            };
            var c = cases[rng.Next(cases.Length)];
            int total = c.rojas + c.azules;
            expected = c.pct;
            title = $"Probabilidad de Extraer Gema Roja ({c.rojas} de {total})";
            stdDesc = $"En una urna hay {c.rojas} gemas rojas y {c.azules} gemas azules ({total} gemas en total). Si se extrae una gema al azar, ¿cuál es el porcentaje de probabilidad de que sea roja (({c.rojas} / {total}) × 100)?";
            simDesc = $"Hay {c.rojas} gemas rojas de {total} en total. ¿Qué porcentaje es: {c.pct}%?";
            scrDesc = $"Urna con {c.rojas} gemas rojas y {c.azules} azules sumando {total}.";
            hint1 = $"Aplica la Regla de Laplace: Casos favorables ({c.rojas}) / Casos posibles ({total}).";
            hint2 = $"({c.rojas} / {total}) × 100 = {expected}%.";
            success = $"¡Exacto! La probabilidad de sacar gema roja es del {expected}%.";
        }
        else if (mode == 1) // Espacio muestral de monedas: 2^n
        {
            int nCoins = isSimplified ? 2 : rng.Next(2, 4); // 2 o 3 monedas
            expected = (int)Math.Pow(2, nCoins); // 4 u 8
            title = $"Espacio Muestral: Lanzamiento de {nCoins} Monedas";
            stdDesc = $"Al lanzar {nCoins} monedas al aire al mismo tiempo, cada moneda tiene 2 resultados posibles. Sabiendo que los casos posibles se calculan como 2 elevado a la {nCoins} (2^{nCoins}), ¿cuántas combinaciones posibles forman el espacio muestral?";
            simDesc = $"Cada moneda tiene 2 caras. Multiplica {string.Join(" × ", Enumerable.Repeat("2", nCoins))}. ¿Cuántas combinaciones son?";
            scrDesc = $"Diagrama de combinaciones posibles para {nCoins} monedas.";
            hint1 = $"Eleva 2 al número de monedas ({nCoins}): 2^{nCoins}.";
            hint2 = $"Hay {expected} combinaciones posibles en total.";
            success = $"¡Brillante! El espacio muestral tiene {expected} combinaciones posibles.";
        }
        else // Evento imposible (0%) o evento seguro (100%)
        {
            bool isSure = rng.Next(2) == 0;
            expected = isSure ? 100 : 0;
            string eventName = isSure ? "evento seguro (certeza absoluta de que ocurrirá)" : "evento imposible (nunca puede ocurrir)";
            title = isSure ? "Probabilidad de un Evento Seguro" : "Probabilidad de un Evento Imposible";
            stdDesc = $"En la teoría clásica de la probabilidad, ¿cuál es el porcentaje exacto que representa a un {eventName}?";
            simDesc = isSure ? "¿Qué porcentaje representa algo que siempre ocurre (100%)?" : "¿Qué porcentaje representa algo imposible que nunca ocurre (0%)?";
            scrDesc = $"Escala de probabilidad destacando el valor extremo del {expected}%.";
            hint1 = isSure ? "Un evento seguro abarca el 100% de los casos." : "Un evento imposible tiene 0 casos favorables.";
            hint2 = $"La respuesta es {expected}%.";
            success = $"¡Extraordinario! La probabilidad es exactamente {expected}%.";
        }

        var ex = new Exercise
        {
            Id = $"gen_prob_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.ProbabilityCompoundEvents,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_prob_1",
            Name = title,
            EmojiSymbol = "🎲",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        if (mode == 2)
        {
            ex.Options.Add(new ExerciseOption { Id = "opt_p_1", Label = "0%", Value = 0, IsCorrect = (expected == 0), SpeechFeedback = (expected == 0) ? "¡Correcto!" : "Incorrecto.", AccessibleKeyShortcut = "1" });
            ex.Options.Add(new ExerciseOption { Id = "opt_p_2", Label = "25%", Value = 25, IsCorrect = false, SpeechFeedback = "Incorrecto.", AccessibleKeyShortcut = "2" });
            ex.Options.Add(new ExerciseOption { Id = "opt_p_3", Label = "50%", Value = 50, IsCorrect = false, SpeechFeedback = "Incorrecto.", AccessibleKeyShortcut = "3" });
            ex.Options.Add(new ExerciseOption { Id = "opt_p_4", Label = "100%", Value = 100, IsCorrect = (expected == 100), SpeechFeedback = (expected == 100) ? "¡Correcto!" : "Incorrecto.", AccessibleKeyShortcut = "4" });
        }
        else
        {
            BuildOptions(ex, expected, min: Math.Max(2, expected - 20), max: expected + 25, unitLabel: (mode == 0 ? "%" : ""), rng);
        }

        return ex;
    }

/// <summary>
    /// Genera ejercicios de cónicas analíticas (parábolas y elipses).
    /// </summary>
    public Exercise GenerateConicsParabolaEllipseExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 14);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Parábola y² = 4px => foco p
        {
            int p = isSimplified ? rng.Next(2, 6) : rng.Next(2, 9);
            int fourP = 4 * p;
            expected = p;
            title = $"Foco de la Parábola y² = {fourP}x";
            stdDesc = $"Una antena reflectora tiene la sección parabólica y² = {fourP}x. Con la forma ordinaria y² = 4px, donde 4p = {fourP}, ¿a qué distancia focal p ({fourP} / 4) se debe colocar el receptor?";
            simDesc = $"Divide {fourP} entre 4 para hallar la distancia al foco. ¿Cuánto da {fourP} / 4?";
            scrDesc = $"Parábola horizontal con ecuación y² = {fourP}x y foco en el eje x.";
            hint1 = $"Divide el coeficiente de x entre 4: {fourP} / 4.";
            hint2 = $"{fourP} / 4 = {expected}. La distancia focal es {expected} metros.";
            success = $"¡Correcto! 4p = {fourP} => p = {expected} unidades.";
        }
        else if (mode == 1) // Elipse x²/a² + y²/b² = 1 => semieje a
        {
            int a = isSimplified ? rng.Next(3, 7) : rng.Next(4, 10);
            int a2 = a * a;
            int b = a - 1;
            int b2 = b * b;
            expected = a;
            title = $"Semieje Mayor de la Elipse x²/{a2} + y²/{b2} = 1";
            stdDesc = $"Una trayectoria orbital elíptica cumple x²/{a2} + y²/{b2} = 1. Sabiendo que a² = {a2} define el semieje mayor, ¿cuánto mide el semieje mayor a (√{a2})?";
            simDesc = $"Calcula la raíz cuadrada de {a2} (¿qué número multiplicado por sí mismo da {a2}?).";
            scrDesc = $"Elipse horizontal con semieje mayor a² = {a2}.";
            hint1 = $"Calcula la raíz cuadrada de {a2}: √{a2}.";
            hint2 = $"{a} × {a} = {a2}, por lo que el semieje mayor es {expected}.";
            success = $"¡Exacto! a = √{a2} = {expected} unidades.";
        }
        else // Eje mayor completo = 2a
        {
            int a = isSimplified ? rng.Next(3, 7) : rng.Next(4, 9);
            expected = 2 * a;
            title = $"Eje Mayor Completo de la Órbita (2a)";
            stdDesc = $"En una elipse planetaria, la distancia del centro al vértice (semieje mayor) mide a = {a} unidades astronómicas. ¿Cuánto mide el eje mayor completo de extremo a extremo (2 × {a})?";
            simDesc = $"Multiplica el semieje {a} por 2 para hallar el eje completo. ¿Cuánto da?";
            scrDesc = $"Eje mayor de la elipse midiendo 2a con a = {a}.";
            hint1 = $"Multiplica el semieje por 2: 2 × {a}.";
            hint2 = $"2 × {a} = {expected} unidades.";
            success = $"¡Brillante! Longitud del eje mayor = 2a = {expected} unidades astronómicas.";
        }

        var ex = new Exercise
        {
            Id = $"gen_conic_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.ConicsParabolaEllipse,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_conic_1",
            Name = title,
            EmojiSymbol = "📡",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 3), max: expected + 4, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de límites y continuidad analítica.
    /// </summary>
    public Exercise GenerateCalculusLimitsExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 15);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Sustitución lineal lim_{x -> a} (m*x + c)
        {
            int m = isSimplified ? rng.Next(2, 4) : rng.Next(2, 6);
            int a = isSimplified ? rng.Next(2, 5) : rng.Next(2, 6);
            int c = rng.Next(1, 7);
            expected = (m * a) + c;
            title = $"Límite Lineal: lim_{{x -> {a}}} ({m}x + {c})";
            stdDesc = $"Evalúa el límite continuo por sustitución directa: lim_{{x -> {a}}} ({m}x + {c}). Sustituyendo x = {a} ({m} × {a} + {c}), ¿cuál es el valor del límite?";
            simDesc = $"Sustituye x por {a}: multiplica {m} × {a} = {m * a}, y suma {c}. ¿Cuánto da?";
            scrDesc = $"Límite de {m}x + {c} cuando x tiende a {a}.";
            hint1 = $"Sustituye x = {a}: {m}({a}) + {c}.";
            hint2 = $"{m * a} + {c} = {expected}.";
            success = $"¡Correcto! lim_{{x -> {a}}} ({m}x + {c}) = {expected}.";
        }
        else if (mode == 1) // Sustitución cuadrática lim_{x -> a} (x² + c)
        {
            int a = isSimplified ? rng.Next(2, 5) : rng.Next(2, 6);
            int c = rng.Next(1, 8);
            expected = (a * a) + c;
            title = $"Límite Cuadrático: lim_{{x -> {a}}} (x² + {c})";
            stdDesc = $"Calcula el límite por sustitución directa: lim_{{x -> {a}}} (x² + {c}). Elevando {a}² = {a * a} y sumando {c} ({a * a} + {c}), ¿cuánto vale el límite?";
            simDesc = $"Eleva {a} al cuadrado ({a} × {a} = {a * a}) y suma {c}. ¿Cuánto da?";
            scrDesc = $"Límite cuadrático de x² + {c} cuando x tiende a {a}.";
            hint1 = $"Sustituye x = {a}: {a}² + {c}.";
            hint2 = $"{a * a} + {c} = {expected}.";
            success = $"¡Exacto! lim_{{x -> {a}}} (x² + {c}) = {expected}.";
        }
        else // Límite de constante lim_{x -> a} (c) = c
        {
            int a = rng.Next(3, 9);
            int c = rng.Next(10, 25);
            expected = c;
            title = $"Límite de una Constante: lim_{{x -> {a}}} ({c})";
            stdDesc = $"Por el teorema del límite de una constante, lim_{{x -> a}} c = c. ¿Cuál es el resultado de lim_{{x -> {a}}} ({c})?";
            simDesc = $"La función siempre vale {c} fijo. ¿Cuánto vale su límite?";
            scrDesc = $"Límite de la constante {c} cuando x tiende a {a}.";
            hint1 = $"El límite de una constante es la misma constante.";
            hint2 = $"El valor no depende de x: el límite es {expected}.";
            success = $"¡Brillante! lim_{{x -> {a}}} ({c}) = {expected}. Las constantes nunca cambian.";
        }

        var ex = new Exercise
        {
            Id = $"gen_limit_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.CalculusLimits,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_limit_1",
            Name = title,
            EmojiSymbol = "🎯",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        BuildOptions(ex, expected, min: Math.Max(1, expected - 4), max: expected + 5, unitLabel: "", rng);
        return ex;
    }

    /// <summary>
    /// Genera ejercicios de reglas operativas de derivación (potencia, lineales, constantes).
    /// </summary>
    public Exercise GenerateCalculusDerivativeRulesExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.HasDisability(DisabilityType.Intellectual) || profile.Age <= 15);

        int mode = rng.Next(3);
        int expected;
        string title;
        string stdDesc;
        string simDesc;
        string scrDesc;
        string hint1;
        string hint2;
        string success;

        if (mode == 0) // Regla de potencia: d/dx(x^n) evaluada en x = a
        {
            int n = isSimplified ? 2 : 3;
            int a = isSimplified ? rng.Next(2, 5) : rng.Next(2, 4);
            int powVal = (int)Math.Pow(a, n - 1);
            expected = n * powVal;
            title = $"Regla de la Potencia: Derivada de x^{n} en x = {a}";
            stdDesc = $"Aplicando la regla de la potencia d/dx(xⁿ) = n·xⁿ⁻¹, la derivada de f(x) = x^{n} es f'(x) = {n}x^{n-1}. Al evaluarla en x = {a}: {n} × ({a}^{n-1} = {powVal}), ¿cuál es la tasa de cambio ({n} × {powVal})?";
            simDesc = $"La derivada es {n}x^{n-1}. Si x vale {a}, multiplica {n} × {powVal}. ¿Cuánto da?";
            scrDesc = $"Derivada de x^{n} evaluada en x = {a}.";
            hint1 = $"Aplica la regla de la potencia: f'(x) = {n}x^{n-1}.";
            hint2 = $"Sustituye x = {a}: {n} × {powVal} = {expected}.";
            success = $"¡Correcto! f'({a}) = {expected} unidades por segundo.";
        }
        else if (mode == 1) // Derivada de función lineal: f(x) = m*x + b => f'(x) = m
        {
            int m = rng.Next(3, 10);
            int b = rng.Next(1, 15);
            expected = m;
            title = $"Derivada Lineal: f(x) = {m}x + {b}";
            stdDesc = $"En la función de posición lineal f(x) = {m}x + {b}, la derivada de {m}x es la constante {m} y la de {b} es 0. ¿Cuál es el valor exacto de la derivada f'(x)?";
            simDesc = $"La derivada de {m}x + {b} es el número que acompaña a la x ({m}). ¿Cuánto vale?";
            scrDesc = $"Derivada de una función lineal f(x) = {m}x + {b}.";
            hint1 = $"La derivada de m·x + b es simplemente la constante m.";
            hint2 = $"f'(x) = {m}.";
            success = $"¡Exacto! La derivada de {m}x + {b} es su pendiente constante: {expected}.";
        }
        else // Derivada de una constante: d/dx(c) = 0
        {
            int c = rng.Next(15, 60);
            expected = 0;
            title = $"Derivada de una Magnitud Constante: f(x) = {c}";
            stdDesc = $"Una nave espacial se encuentra en reposo estacionario con posición fija f(x) = {c}. Sabiendo que la derivada mide la velocidad de cambio de posición, ¿cuál es su derivada f'(x)?";
            simDesc = $"Si la nave no se mueve nada y su posición es fija en {c}, ¿cuál es su velocidad (derivada)?";
            scrDesc = $"Función constante f(x) = {c} con tasa de cambio cero.";
            hint1 = $"La derivada de cualquier constante es siempre 0.";
            hint2 = $"f'(x) = 0.";
            success = $"¡Extraordinario! d/dx({c}) = 0. Lo que no cambia tiene derivada cero.";
        }

        var ex = new Exercise
        {
            Id = $"gen_deriv_{Guid.NewGuid():N}",
            Title = title,
            GameType = GamePlayType.CalculusDerivativeRules,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = expected,
            StandardInstruction = stdDesc,
            SimplifiedInstruction = simDesc,
            ScreenReaderDescription = scrDesc,
            FirstHint = hint1,
            SecondHintDetailed = hint2,
            ExplanationOfSuccess = success,
            SimplifiedFirstHint = hint1,
            SimplifiedSecondHint = hint2,
            SimplifiedExplanationOfSuccess = success
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_deriv_1",
            Name = title,
            EmojiSymbol = "⚡",
            AccessibleDescription = scrDesc,
            OrderIndex = 1,
            MusicalPitchHz = PentatonicScaleHz[mode % PentatonicScaleHz.Length]
        });

        if (mode == 2)
        {
            ex.Options.Add(new ExerciseOption { Id = "opt_dr_1", Label = "0", Value = 0, IsCorrect = true, SpeechFeedback = "¡Correcto! La derivada de una constante es 0.", AccessibleKeyShortcut = "1" });
            ex.Options.Add(new ExerciseOption { Id = "opt_dr_2", Label = "1", Value = 1, IsCorrect = false, SpeechFeedback = "Incorrecto.", AccessibleKeyShortcut = "2" });
            ex.Options.Add(new ExerciseOption { Id = "opt_dr_3", Label = "10", Value = 10, IsCorrect = false, SpeechFeedback = "Incorrecto.", AccessibleKeyShortcut = "3" });
            ex.Options.Add(new ExerciseOption { Id = "opt_dr_4", Label = "100", Value = 100, IsCorrect = false, SpeechFeedback = "Incorrecto.", AccessibleKeyShortcut = "4" });
        }
        else
        {
            BuildOptions(ex, expected, min: Math.Max(1, expected - 4), max: expected + 5, unitLabel: "", rng);
        }

        return ex;
    }
}
