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
