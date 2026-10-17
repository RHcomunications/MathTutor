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

        int dollars = isSimplified ? 1 : rng.Next(1, 4);
        int halfDollars = isSimplified ? 1 : (rng.Next(1, 3) * 1);
        int totalCents = (dollars * 100) + (halfDollars * 50);
        decimal decimalAmount = totalCents / 100m;

        var ex = new Exercise
        {
            Id = $"gen_dec_{Guid.NewGuid():N}",
            Title = $"El Monedero Mágico: ${decimalAmount:F2}",
            GameType = GamePlayType.DecimalsMoney,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = totalCents,
            StandardInstruction = $"En el mercado compras frutas mágicas. Tienes {dollars} moneda(s) de $1.00 (100¢) y {halfDollars} moneda(s) de $0.50 (50¢). ¿Cuántos centavos tienes en total? (Equivale a ${decimalAmount:F2})",
            SimplifiedInstruction = $"Tienes {dollars} moneda de $1 y {halfDollars} de 50 centavos. ¿Cuántos centavos tienes en total?",
            ScreenReaderDescription = $"{dollars} monedas de un peso y {halfDollars} monedas de cincuenta centavos. Total: {totalCents} centavos.",
            FirstHint = $"Cada $1.00 son 100 centavos. Suma: ({dollars * 100}) + ({halfDollars * 50}).",
            SecondHintDetailed = $"{dollars * 100} centavos más {halfDollars * 50} centavos es igual a {totalCents} centavos (${decimalAmount:F2}).",
            ExplanationOfSuccess = $"¡Excelente! Tienes {totalCents} centavos, que se escribe matemáticamente como ${decimalAmount:F2}. ¡Los decimales son las partes de un peso entero!"
        };

        for (int i = 1; i <= dollars; i++)
        {
            ex.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"coin_d_{i}",
                Name = $"Moneda de $1.00 #{i}",
                EmojiSymbol = "🪙",
                AccessibleDescription = $"Moneda de 1 peso (100 centavos)",
                OrderIndex = i,
                MusicalPitchHz = PentatonicScaleHz[0]
            });
        }

        for (int i = 1; i <= halfDollars; i++)
        {
            ex.ItemsGroupB.Add(new ManipulableItem
            {
                Id = $"coin_c_{i}",
                Name = $"Moneda de $0.50 #{i}",
                EmojiSymbol = "🥈",
                AccessibleDescription = $"Moneda de 50 centavos (0.50 pesos)",
                OrderIndex = dollars + i,
                MusicalPitchHz = PentatonicScaleHz[2]
            });
        }

        BuildOptions(ex, totalCents, min: Math.Max(50, totalCents - 100), max: totalCents + 100, unitLabel: "centavos", rng);
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
