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

        var ex = new Exercise
        {
            Id = $"gen_signed_{Guid.NewGuid():N}",
            Title = "La Libreta de la Tiendita: Saldo con Números Enteros",
            GameType = GamePlayType.SignedNumbersAddition,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = finalBalance,
            StandardInstruction = $"En la tiendita de la esquina tenías ${initialBalance} {currency} a favor (+{initialBalance}). Compraste mercancía por ${debt} {currency} que quedaste a deber (-{debt}). Al registrar (+{initialBalance}) + (-{debt}), ¿cuál es tu saldo final?",
            SimplifiedInstruction = $"Tenías {initialBalance} {currency} y gastaste {debt} {currency}. Quedaste debiendo. ¿Cuál es tu saldo final?",
            ScreenReaderDescription = $"Operación con números enteros con signo: positivo {initialBalance} más negativo {debt}. Resultado esperado: {finalBalance}.",
            FirstHint = $"Como la deuda ({debt}) es mayor que tu saldo inicial ({initialBalance}), tu resultado será un número negativo.",
            SecondHintDetailed = $"(+{initialBalance}) - {debt} = {finalBalance}. Tienes una deuda de {Math.Abs(finalBalance)} {currency}.",
            ExplanationOfSuccess = $"¡Excelente! (+{initialBalance}) + (-{debt}) = {finalBalance}. Un saldo de {finalBalance} significa que debes {Math.Abs(finalBalance)} {currency} en la tiendita."
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

        var ex = new Exercise
        {
            Id = $"gen_eq_axb_{Guid.NewGuid():N}",
            Title = $"Ecuación Lineal: {a}X + {b} = {c}",
            GameType = GamePlayType.LinearEquationAXB,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = x,
            StandardInstruction = $"En la papelería compraste {a} cuadernos iguales que costaron X pesos cada uno, más una goma de ${b} pesos. La cuenta total fue de ${c} pesos ({a}X + {b} = {c}). Despejando paso a paso, ¿cuánto costó cada cuaderno X?",
            SimplifiedInstruction = $"{a} libretas valen {a}X. Más {b} pesos da {c} en total. ¿Cuánto vale cada libreta X?",
            ScreenReaderDescription = $"Ecuación lineal: {a} por X más {b} igual a {c}. Encuentra el valor de X.",
            FirstHint = $"Paso 1: Resta {b} de ambos lados: {a}X = {c} - {b} = {c - b}.",
            SecondHintDetailed = $"Paso 2: Divide entre {a}: X = {c - b} ÷ {a} = {x}.",
            ExplanationOfSuccess = $"¡Espectacular! {a}X = {c - b}, luego X = {c - b} ÷ {a} = {x}. Cada libreta costó ${x} pesos."
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

        BuildOptions(ex, x, min: Math.Max(1, x - 2), max: x + 3, unitLabel: "pesos", rng);
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

        var ex = new Exercise
        {
            Id = $"gen_pct_{Guid.NewGuid():N}",
            Title = $"Oferta en la Tienda: {percent}% de Descuento",
            GameType = GamePlayType.PercentageCalculation,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = discountAmount,
            StandardInstruction = $"En la tienda de ropa una mochila cuesta ${basePrice} {currency} y tiene un letrero de oferta con el {percent}% de descuento. ¿Cuánto dinero te van a descontar del precio original?",
            SimplifiedInstruction = $"Una mochila de ${basePrice} {currency} tiene {percent}% de descuento. ¿Cuánto dinero te descuentan?",
            ScreenReaderDescription = $"Cálculo de porcentaje: {percent} por ciento de {basePrice} pesos. Descuento en pesos.",
            FirstHint = $"El {percent}% significa {percent} de cada 100 pesos. Multiplica {basePrice} × {percent} y divide entre 100.",
            SecondHintDetailed = $"({basePrice} × {percent}) ÷ 100 = {discountAmount} {currency}.",
            ExplanationOfSuccess = $"¡Exacto! El {percent}% de ${basePrice} {currency} son ${discountAmount} {currency} de descuento. ¡Pagarás solo ${finalPrice} {currency}!"
        };

        ex.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "item_backpack",
            Name = $"Mochila de ${basePrice}",
            EmojiSymbol = "🎒",
            AccessibleDescription = $"Mochila de {basePrice} pesos con etiqueta de {percent}%",
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

        var ex = new Exercise
        {
            Id = $"gen_sys2x2_{Guid.NewGuid():N}",
            Title = "La Taquería: Sistema de Ecuaciones 2×2",
            GameType = GamePlayType.LinearSystem2x2,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = tacoPrice,
            StandardInstruction = $"En la taquería del barrio: por 2 tacos y 1 refresco pagas ${p1} {currency} (2x + y = {p1}). Por 1 taco y 1 refresco pagas ${p2} {currency} (x + y = {p2}). Comparando ambas cuentas, ¿cuánto cuesta exactamente 1 taco (x)?",
            SimplifiedInstruction = $"2 tacos y 1 refresco cuestan ${p1}. 1 taco y 1 refresco cuestan ${p2}. La diferencia es el precio de 1 taco. ¿Cuánto cuesta?",
            ScreenReaderDescription = $"Sistema 2x2: ecuación uno es 2x + y = {p1}. Ecuación dos es x + y = {p2}. Restando ambas ecuaciones se obtiene el precio de x.",
            FirstHint = $"Observa que la única diferencia entre las dos órdenes es 1 taco adicional. Resta: ${p1} - ${p2}.",
            SecondHintDetailed = $"{p1} - {p2} = {tacoPrice} {currency}. Por tanto, 1 taco cuesta ${tacoPrice} {currency}.",
            ExplanationOfSuccess = $"¡Excelente deducción algebraica! Al restar las dos ecuaciones eliminamos el refresco (y - y = 0) y obtenemos: x = {p1} - {p2} = ${tacoPrice} {currency} por taco."
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
