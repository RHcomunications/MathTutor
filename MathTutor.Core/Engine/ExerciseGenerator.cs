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
        if (profile.Disability == DisabilityType.Intellectual)
        {
            return profile.Severity switch
            {
                DisabilitySeverity.Severe => (1, 3),
                DisabilitySeverity.Moderate => (1, 5),
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
            ScreenReaderDescription = $"Hay {count} {visual.Name.ToLower()}s alineadas en fila. Debes contar cada una.",
            FirstHint = $"Toca la primera {visual.Name.ToLower()} y escucha su tono musical. Luego sigue con la siguiente.",
            SecondHintDetailed = $"Cuenta despacito: el último número que digas te dirá cuántas {visual.Name.ToLower()}s hay en total.",
            ExplanationOfSuccess = $"¡Extraordinario! Hay {count} {visual.Name.ToLower()}s en total. ¡El último número contado representa la cantidad completa de la colección!"
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
            SecondHintDetailed = $"Después del número {start + 2} siempre viene el número {missing}. ¡El orden nunca cambia!",
            ExplanationOfSuccess = $"¡Magnífico! El número {missing} es el correcto. Los números siempre viajan en el mismo orden invariable: ¡eso es el orden estable!"
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 6);

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
            ScreenReaderDescription = $"Una cuadrícula de {rows} filas por {cols} columnas con {visual.Name.ToLower()}s. Total: {total}.",
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 6);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            FirstHint = $"Observa que {eq.n2} es el doble de {eq.n1}, y {eq.d2} es el doble de {eq.d1}. ¡Ocupan el mismo espacio!",
            SecondHintDetailed = $"{eq.n1}/{eq.d1} y {eq.n2}/{eq.d2} son fracciones equivalentes: representan la misma porción.",
            ExplanationOfSuccess = $"¡Exacto! {eq.n1}/{eq.d1} es equivalente a {eq.n2}/{eq.d2}. Aunque usemos números distintos, ¡representan exactamente la misma cantidad!"
        };

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
            SpeechFeedback = "Mmm, observa con cuidado: aunque las rebanadas son más pequeñas, son más en cantidad y suman lo mismo."
        });

        return ex;
    }

    /// <summary>
    /// Genera un ejercicio de multiplicación por descomposición aditiva / propiedad distributiva: (20 + 4) x 3 = 60 + 12 = 72.
    /// </summary>
    public Exercise GenerateMultiplicationDistributiveExercise(UserProfile profile, Random? rng = null)
    {
        rng ??= Random.Shared;
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            ScreenReaderDescription = $"{wholeUnits} monedas de 1.00 {currency} y {halfUnits} monedas de cincuenta {fraction}. Total: {totalSubUnits} {fraction}.",
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            ScreenReaderDescription = $"Cuadrado perfecto de {squareArea} losas en una cuadrícula de {root} por {root}. Raíz cuadrada de {squareArea}.",
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            ScreenReaderDescription = $"Plano cartesiano con un punto en coordenada X igual a {xCoord} e Y igual a {yCoord}. Pregunta por el eje {axisName}.",
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            ScreenReaderDescription = $"Operación con números enteros con signo: positivo {initialBalance} más negativo {debt}. Resultado esperado: {finalBalance}.",
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            ScreenReaderDescription = $"Ecuación de la recta y = {m}x + {b}. Para x = {x} kilómetros: {m} por {x} es {m * x}, más {b} {currency} da {expectedY} {currency}.",
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            ScreenReaderDescription = $"Función trigonométrica continua y = {amplitude} por seno de x. Amplitud máxima en el pico donde sen(x) es 1. Altura máxima {amplitude}.",
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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
        bool isSimplified = (profile.Disability == DisabilityType.Intellectual || profile.Age <= 8);

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
            ScreenReaderDescription = $"Bucle de programación: repetir {iterations} veces avanzar {stepsPerIteration} pasos. Suma repetida {sumExpression} igual a {totalSteps} pasos.",
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

                "elem_addition_01" => (i % 3) switch
                {
                    0 => GenerateAdditionCombineExercise(profile, rng),
                    1 => GenerateCommutativeExercise(profile, rng),
                    _ => GenerateNeutralZeroExercise(profile, rng)
                },

                "elem_subtraction_01" => GenerateSubtractionExercise(profile, rng),

                "elem_multiplication_01" => GenerateMultiplicationArrayExercise(profile, rng),

                "elem_division_01" => GenerateDivisionFairSharingExercise(profile, rng),

                "elem_fractions_01" => (i % 2 == 0)
                    ? GenerateFractionPartsExercise(profile, rng)
                    : GenerateFractionEquivalenceExercise(profile, rng),

                "elem_distributive_01" => GenerateMultiplicationDistributiveExercise(profile, rng),

                "elem_mixed_fractions_01" => GenerateMixedFractionsExercise(profile, rng),

                "elem_decimals_01" => GenerateDecimalsMoneyExercise(profile, rng),

                "elem_powers_01" => GeneratePowersExercise(profile, rng),

                "elem_proportion_01" => GenerateDirectProportionExercise(profile, rng),

                "elem_algebra_balance_01" => GenerateAlgebraicBalanceExercise(profile, rng),

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

                _ => GenerateCountingExercise(profile, rng)
            };

            list.Add(ex);
        }

        return list;
    }

    private static void BuildOptions(Exercise ex, int correctValue, int min, int max, string unitLabel, Random rng)
    {
        var values = new HashSet<int> { correctValue };

        // Añadir distractores cercanos
        int candidate1 = correctValue + 1;
        int candidate2 = Math.Max(min, correctValue - 1);
        int candidate3 = correctValue + 2;

        values.Add(candidate1);
        if (candidate2 != correctValue) values.Add(candidate2);
        if (values.Count < 3) values.Add(candidate3);

        var sortedValues = values.OrderBy(v => v).ToList();

        for (int i = 0; i < sortedValues.Count; i++)
        {
            int val = sortedValues[i];
            bool isCorrect = (val == correctValue);
            string label = string.IsNullOrWhiteSpace(unitLabel) ? val.ToString() : $"{val} {unitLabel}";
            string shortcut = (i + 1).ToString();

            ex.Options.Add(new ExerciseOption
            {
                Id = $"opt_{val}",
                Value = val,
                Label = label,
                IsCorrect = isCorrect,
                AccessibleKeyShortcut = shortcut,
                SpeechFeedback = isCorrect
                    ? $"¡Muy bien! {label} es la respuesta correcta."
                    : $"Mmm, {label} no es. ¡Escucha la pista e inténtalo de nuevo!"
            });
        }
    }
}
