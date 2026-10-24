using System.Collections.Generic;
using MathTutor.Core.Models;

namespace MathTutor.Core.Engine;

/// <summary>
/// Proveedor del currículo pedagógico adaptativo con base lúdica y principios matemáticos sólidos.
/// </summary>
public class CurriculumService
{
    // Escala musical pentatónica y diatónica (C4 a C5) para conteo sensorial
    private static readonly int[] Frequencies = { 262, 294, 330, 349, 392, 440, 494, 523 };
    private readonly ExerciseGenerator _generator = new();

    public List<PedagogicalTopic> GetAllTopics()
    {
        var topics = new List<PedagogicalTopic>
        {
            BuildKindergartenCountingTopic(),
            BuildAdditionAndPropertiesTopic(),
            BuildSubtractionAndInverseTopic(),
            BuildMultiplicationAndArraysTopic(),
            BuildDivisionAndFairSharingTopic(),
            BuildDivisionRemainderTopic(),
            BuildMoneyCurrencyTopic(),
            BuildMeasurementRulerTopic(),
            BuildTimeClockTopic(),
            BuildFractionsTopic(),
            BuildFractionsNumberLineTopic(),
            BuildDistributiveMultiplicationTopic(),
            BuildAnglesTopic(),
            BuildPerimeterTopic(),
            BuildMixedFractionsTopic(),
            BuildDecimalsAndMoneyTopic(),
            BuildDataBarChartsTopic(),
            BuildAreaTopic(),
            BuildPowersAndExponentsTopic(),
            BuildDirectProportionTopic(),
            BuildStatisticsCentralTendencyTopic(),
            BuildAlgebraicBalanceTopic(),
            BuildVolumeTopic(),
            BuildSquareRootTopic(),
            BuildFactorsMcmMcdTopic(),
            BuildCartesianCoordinatesTopic(),
            BuildCircleGeometryTopic(),
            BuildSignedNumbersTopic(),
            BuildNegativeRationalNumbersTopic(),
            BuildOrderOfOperationsTopic(),
            BuildLinearEquationsTopic(),
            BuildPercentagesTopic(),
            BuildExponentLawsTopic(),
            BuildPolynomialLikeTermsTopic(),
            BuildAlgebraFactoringTopic(),
            BuildParallelLinesAnglesTopic(),
            BuildLinearSystems2x2Topic(),
            BuildPythagoreanTheoremTopic(),
            BuildQuadraticEquationsTopic(),
            BuildThalesSimilarityTopic(),
            BuildTrigonometricRatiosTopic(),
            BuildProbabilityCompoundEventsTopic(),
            BuildQuadraticFunctionsTopic(),
            BuildAnalyticDistanceTopic(),
            BuildLineSlopeTopic(),
            BuildLinearEquationYMXBTopic(),
            BuildConicCircleTopic(),
            BuildConicsParabolaEllipseTopic(),
            BuildSineWaveContinuousTopic(),
            BuildLogarithmBinaryTopic(),
            BuildCalculusLimitsTopic(),
            BuildCalculusDerivativeTopic(),
            BuildCalculusDerivativeRulesTopic(),
            BuildCalculusIntegralTopic(),
            BuildProbabilityAndStatsTopic(),
            BuildRepeatedAdditionAndProgrammingSeedTopic(),
            BuildProgrammingVariablesTopic(),
            BuildProgrammingConditionalsTopic(),
            BuildProgrammingBooleanLogicTopic(),
            BuildProgrammingLoopsTopic(),
            BuildProgrammingArraysTopic(),
            BuildProgrammingFunctionsTopic(),
            BuildProgrammingAlgorithmsSearchTopic(),
            BuildProgrammingGameCoordinatesTopic(),
            BuildProgrammingAssistiveAITopic()
        };

        topics.InsertRange(1, KindergartenCurriculum.BuildTopics());
        return topics;
    }

    /// <summary>
    /// Devuelve las misiones pedagógicas permitidas según la edad y condición del alumno (Tarea 6 y Tarea 12),
    /// desbloqueando las misiones subsecuentes de la etapa cuando se completan las misiones del nivel actual.
    /// </summary>
    public List<PedagogicalTopic> GetTopicsForAge(int age, DisabilityType disability = DisabilityType.None, IEnumerable<string>? completedLessonIds = null)
    {
        var allTopics = GetAllTopics();
        var allowedLevels = new HashSet<AcademicLevel>(AdaptiveEngine.GetAllowedLevelsForAge(age, disability));

        // Niveles de refuerzo concedidos únicamente por discapacidad intelectual:
        // sus temas quedan exentos del filtro de rango de edad objetivo, de lo
        // contrario el filtro anularía el refuerzo (p. ej. Kínder para 6-15 años).
        var exemptLevels = new HashSet<AcademicLevel>();
        if (disability == DisabilityType.Intellectual && age >= 6 && age <= 15)
        {
            var baseLevels = new HashSet<AcademicLevel>(AdaptiveEngine.GetAllowedLevelsForAge(age, DisabilityType.None));
            foreach (var level in allowedLevels)
            {
                if (!baseLevels.Contains(level))
                {
                    exemptLevels.Add(level);
                }
            }
        }

        if (completedLessonIds != null)
        {
            var completedSet = new HashSet<string>(completedLessonIds);

            bool IsAllowed(PedagogicalTopic t) =>
                allowedLevels.Contains(t.Level) &&
                (age >= 16 || exemptLevels.Contains(t.Level) ||
                 (age >= t.TargetAgeMin && age <= t.TargetAgeMax));

            // Desbloqueo en cascada: si el alumno ya completó todas las misiones
            // permitidas, se abre el nivel siguiente del orden canónico; la cascada
            // se repite mientras continúe completado. Los niveles desbloqueados por
            // progreso quedan exentos del filtro de edad objetivo, porque un alumno
            // adelantado puede terminarlos antes de llegar a la edad canónica.
            while (age >= 5)
            {
                var allowedTopics = allTopics.FindAll(IsAllowed);
                if (allowedTopics.Count == 0 || !allowedTopics.TrueForAll(t => completedSet.Contains(t.Id)))
                {
                    break;
                }

                int lastIdx = allTopics.FindLastIndex(t => allowedLevels.Contains(t.Level));
                bool added = false;
                for (int i = lastIdx + 1; i < allTopics.Count; i++)
                {
                    if (allowedLevels.Add(allTopics[i].Level))
                    {
                        exemptLevels.Add(allTopics[i].Level);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    break;
                }
            }
        }

        return allTopics
            .FindAll(t =>
                allowedLevels.Contains(t.Level) &&
                (age >= 16 || exemptLevels.Contains(t.Level) ||
                 (age >= t.TargetAgeMin && age <= t.TargetAgeMax)));
    }

    /// <summary>
    /// Devuelve las misiones pedagógicas permitidas según la edad y condiciones múltiples del alumno (comorbilidad),
    /// desbloqueando las misiones subsecuentes de la etapa cuando se completan las misiones del nivel actual.
    /// </summary>
    public List<PedagogicalTopic> GetTopicsForAge(int age, IEnumerable<DisabilityType> disabilities, IEnumerable<string>? completedLessonIds = null)
    {
        bool hasIntellectual = disabilities != null && System.Linq.Enumerable.Contains(disabilities, DisabilityType.Intellectual);
        return GetTopicsForAge(age, hasIntellectual ? DisabilityType.Intellectual : DisabilityType.None, completedLessonIds);
    }

    /// <summary>
    /// Obtiene la siguiente misión recomendada tras completar <paramref name="currentTopicId"/> (Tarea 7 y Tarea 10).
    /// Busca dentro de los niveles permitidos para la edad del alumno y devuelve null si ya completó todas las misiones de su etapa.
    /// </summary>
    public PedagogicalTopic? GetNextTopic(string currentTopicId, UserProfile? profile = null)
    {
        var topics = profile != null
            ? (profile.Disabilities.Count > 0 
                ? GetTopicsForAge(profile.Age, profile.Disabilities, profile.CompletedLessonIds) 
                : GetTopicsForAge(profile.Age, profile.Disability, profile.CompletedLessonIds))
            : GetAllTopics();

        if (topics.Count == 0)
        {
            topics = GetAllTopics();
        }

        int idx = topics.FindIndex(t => t.Id == currentTopicId);
        if (idx >= 0)
        {
            var next = topics
                .Skip(idx + 1)
                .FirstOrDefault(t => profile == null || !profile.CompletedLessonIds.Contains(t.Id));
            if (next != null)
            {
                return next;
            }
        }

        if (profile != null)
        {
            // Fallback hacia adelante: si el tema actualidad no está visible para este
            // perfil (p. ej. quedó fuera de sus niveles permitidos), se ubica su posición
            // canónica y se prefiere el primer tema pendiente posterior a él en el orden
            // del currículo; solo si ya no hay pendientes adelante se retrocede.
            var allTopics = GetAllTopics();
            int canonicalIdx = allTopics.FindIndex(t => t.Id == currentTopicId);
            int start = canonicalIdx >= 0 ? canonicalIdx : -1;

            return topics.FirstOrDefault(t =>
                       allTopics.IndexOf(t) > start &&
                       !profile.CompletedLessonIds.Contains(t.Id)) ??
                   topics.Find(t => t.Id != currentTopicId && !profile.CompletedLessonIds.Contains(t.Id));
        }

        return null;
    }

    /// <summary>
    /// Genera una serie de ejercicios que conecta el reto base de la lección con ejercicios procedimentales adaptados.
    /// </summary>
    public List<Exercise> GenerateAdaptiveExercises(PedagogicalTopic topic, UserProfile profile, int count = 3)
    {
        if (count <= 0) return new List<Exercise>();

        bool isMonetaryTopic = topic.Id is "elem_decimals_01"
            or "elem_proportion_01"
            or "sec_signed_numbers_01"
            or "sec_linear_equations_01"
            or "sec_percentages_01"
            or "sec_linear_systems_2x2_01"
            or "prog_variables_state_01"
            or "prog_arrays_lists_01";

        // Si el perfil requiere rango reducido por discapacidad intelectual o moneda de otro país en temas monetarios,
        // generamos todos procedimentalmente para garantizar rango numérico y vocabulario regional exactos.
        if (profile.HasDisability(DisabilityType.Intellectual) || (isMonetaryTopic && profile.Region != RegionalContext.Mexico))
        {
            return _generator.GenerateExercisesForTopic(topic.Id, profile, count);
        }

        var templateTopic = GetAllTopics().Find(t => t.Id == topic.Id);
        var result = new List<Exercise>();

        if (templateTopic != null && templateTopic.Exercises.Count > 0)
        {
            IEnumerable<Exercise> curated = templateTopic.Exercises;

            // En jardín de niños, el tema contiene retos por banda de edad:
            // se eligen los que corresponden a la edad real del alumno.
            if (topic.Level == AcademicLevel.Kindergarten)
            {
                curated = curated
                    .Where(e => profile.Age >= e.TargetAgeMin && profile.Age <= e.TargetAgeMax)
                    .OrderByDescending(e => e.TargetAgeMin)
                    .ThenByDescending(e => e.TargetAgeMax);

                if (!curated.Any())
                {
                    curated = templateTopic.Exercises;
                }
            }

            result.AddRange(curated.Take(count));
        }

        int remaining = count - result.Count;
        if (remaining > 0)
        {
            result.AddRange(_generator.GenerateExercisesForTopic(topic.Id, profile, remaining));
        }

        // Aleatorizar las opciones para eliminar el sesgo de posición en cada reto cargado
        foreach (var ex in result)
        {
            ExerciseGenerator.ShuffleOptions(ex);
        }

        return result;
    }

    /// <summary>
    /// Tema 1: Los Principios del Conteo (Gelman & Gallistel) para Jardín de Niños (3 a 5 años)
    /// </summary>
    private PedagogicalTopic BuildKindergartenCountingTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "kinder_counting_01",
            Title = "El Bosque Mágico: Cuenta con Cosmo",
            Level = AcademicLevel.Kindergarten,
            TargetAgeMin = 3,
            TargetAgeMax = 5,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Hola! Soy Cosmo, el búho del bosque. Hoy vamos a contar manzanas, piedras y tesoros. ¿Me acompañas?",
            NarrativeIntroSimplified = "¡Hola! Soy Cosmo. Vamos a contar cosas del bosque. ¡Vamos a jugar!",
            CoreConceptExplanation = "Para contar, toca cada cosa una sola vez y di un número. No te saltes ninguna. El último número que dices es cuántas cosas hay.",
            CoreConceptSimplified = "Toca cada cosa una vez. Di un número. El último número es el total.",
            PrinciplesAddressed = new List<string>
            {
                "Correspondencia uno a uno (biunívoca)",
                "Orden estable de la serie numérica",
                "Cardinalidad del conjunto",
                "Abstracción",
                "Irrelevancia del orden"
            },
            CongratulationMessage = "¡Muy bien! Ya sabes contar tocando cada cosa una vez. ¡Las estrellas brillan para ti!"
        };

        // Ejercicio 1: Principio de Correspondencia Uno a Uno y Cardinalidad
        var ex1 = new Exercise
        {
            Id = "count_ex_01",
            Title = "El Banquete de las Manzanas",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 3,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 3,
            StandardInstruction = "Hay 3 conejitos con hambre. Toca cada manzana y dale una a cada conejito. ¿Cuántas manzanas hay?",
            SimplifiedInstruction = "Toca cada manzana para contarla. ¿Cuántas manzanas hay?",
            ScreenReaderDescription = "En el huerto hay varias manzanas. Explora y cuéntalas una a una.",
            FirstHint = "Toca la primera manzana. Luego la siguiente. No te saltes ninguna.",
            SecondHintDetailed = "Cuenta: una, dos, tres. El último número es 3.",
            ExplanationOfSuccess = "¡Muy bien! Hay 3 manzanas. Tocaste cada una una vez.",
            SimplifiedFirstHint = "Toca una manzana. Luego la siguiente. No te saltes ninguna.",
            SimplifiedSecondHint = "Cuenta: una, dos, tres.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Hay 3 manzanas."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "m1", Name = "Manzana Roja 1", EmojiSymbol = "🍎", AccessibleDescription = "Primera manzana roja", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "m2", Name = "Manzana Roja 2", EmojiSymbol = "🍎", AccessibleDescription = "Segunda manzana roja", OrderIndex = 2, MusicalPitchHz = Frequencies[1] });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "m3", Name = "Manzana Roja 3", EmojiSymbol = "🍎", AccessibleDescription = "Tercera manzana roja", OrderIndex = 3, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "2 manzanas", Value = 2, IsCorrect = false, SpeechFeedback = "Mmm, te falta una manzana por contar. ¡Escucha bien!", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "3 manzanas", Value = 3, IsCorrect = true, SpeechFeedback = "¡Magnífico! Son 3 manzanas. ¡Exacto!", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4 manzanas", Value = 4, IsCorrect = false, SpeechFeedback = "¡Uy, esa es una manzana de más! Contemos otra vez.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);

        // Ejercicio 2: Principio de Orden Estable
        var ex2 = new Exercise
        {
            Id = "count_ex_02",
            Title = "El Puente de las Piedras",
            GameType = GamePlayType.OrderSequence,
            TargetAgeMin = 3,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 4,
            StandardInstruction = "Las piedras del río dicen: 1, 2, 3... ¿Qué número sigue en la siguiente piedra?",
            SimplifiedInstruction = "Las piedras dicen: 1, 2, 3... ¿Qué número sigue?",
            ScreenReaderDescription = "Hay piedras con números en orden. Una piedra está vacía.",
            FirstHint = "Di los números como una canción: uno, dos, tres...",
            SecondHintDetailed = "Después del 3 viene el 4. Los números van en orden.",
            ExplanationOfSuccess = "¡Bien! El 4 es el número que sigue. Los números van en orden: 1, 2, 3, 4.",
            SimplifiedFirstHint = "Di los números: uno, dos, tres...",
            SimplifiedSecondHint = "Después del 3 viene el 4.",
            SimplifiedExplanationOfSuccess = "¡Bien! El 4 es el número que sigue."
        };

        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "p1", Name = "Piedra 1", EmojiSymbol = "🪨 1", AccessibleDescription = "Piedra con el número 1", OrderIndex = 1, MusicalPitchHz = Frequencies[0], IsCounted = true });
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "p2", Name = "Piedra 2", EmojiSymbol = "🪨 2", AccessibleDescription = "Piedra con el número 2", OrderIndex = 2, MusicalPitchHz = Frequencies[1], IsCounted = true });
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "p3", Name = "Piedra 3", EmojiSymbol = "🪨 3", AccessibleDescription = "Piedra con el número 3", OrderIndex = 3, MusicalPitchHz = Frequencies[2], IsCounted = true });
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "p4", Name = "Piedra ?", EmojiSymbol = "❓", AccessibleDescription = "Piedra misteriosa vacía esperando tu número", OrderIndex = 4, MusicalPitchHz = Frequencies[3] });

        ex2.Options.Add(new ExerciseOption { Id = "opt1", Label = "5", Value = 5, IsCorrect = false, SpeechFeedback = "¡Uy, te adelantaste un pasito! Antes del 5 viene otro número.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt2", Label = "4", Value = 4, IsCorrect = true, SpeechFeedback = "¡Excelente! Después del 3 siempre viene el 4.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt3", Label = "2", Value = 2, IsCorrect = false, SpeechFeedback = "El 2 ya lo pisamos atrás. ¡Vamos hacia adelante!", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex2);

        // Ejercicio 3: Principio de Abstracción e Irrelevancia del Orden
        var ex3 = new Exercise
        {
            Id = "count_ex_03",
            Title = "El Cofre de los Tesoros",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 4,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 5,
            StandardInstruction = "En el cofre hay estrellas, campanas y un corazón. ¿Cuántos tesoros hay en total?",
            SimplifiedInstruction = "Hay estrellas, campanas y un corazón. ¿Cuántos juguetes hay?",
            ScreenReaderDescription = "Dentro del cofre hay objetos diferentes. Cuéntalos todos.",
            FirstHint = "Puedes contarlos en cualquier orden. Toca cada uno una vez.",
            SecondHintDetailed = "Cuenta: uno, dos, tres, cuatro, cinco. Hay 5.",
            ExplanationOfSuccess = "¡Genial! Hay 5 tesoros. Los contaste todos, aunque fueran diferentes.",
            SimplifiedFirstHint = "Toca cada tesoro una vez.",
            SimplifiedSecondHint = "Cuenta: uno, dos, tres, cuatro, cinco.",
            SimplifiedExplanationOfSuccess = "¡Genial! Hay 5 tesoros."
        };

        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "item1", Name = "Estrella 1", EmojiSymbol = "⭐", AccessibleDescription = "Una estrella dorada", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "item2", Name = "Estrella 2", EmojiSymbol = "⭐", AccessibleDescription = "Otra estrella dorada", OrderIndex = 2, MusicalPitchHz = Frequencies[1] });
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "item3", Name = "Campana 1", EmojiSymbol = "🔔", AccessibleDescription = "Una campana sonora", OrderIndex = 3, MusicalPitchHz = Frequencies[2] });
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "item4", Name = "Campana 2", EmojiSymbol = "🔔", AccessibleDescription = "Otra campana sonora", OrderIndex = 4, MusicalPitchHz = Frequencies[3] });
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "item5", Name = "Corazón", EmojiSymbol = "💖", AccessibleDescription = "Un corazón brillante", OrderIndex = 5, MusicalPitchHz = Frequencies[4] });

        ex3.Options.Add(new ExerciseOption { Id = "opt1", Label = "4 tesoros", Value = 4, IsCorrect = false, SpeechFeedback = "Falta un tesoro por contar.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt2", Label = "5 tesoros", Value = 5, IsCorrect = true, SpeechFeedback = "¡Fantástico! Son 5 tesoros mágicos en total.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt3", Label = "3 tesoros", Value = 3, IsCorrect = false, SpeechFeedback = "Contaste sólo algunos. Contemos todos juntos.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex3);
        AddKindergartenAgeBands(topic);

        return topic;
    }

    /// <summary>
    /// Añade retos por banda de edad al tema de conteo de jardín de niños:
    /// 3-4 años (contar hasta 2 y orden 1-3), 4-5 años (contar hasta 4 y comparar)
    /// y 5-6 años (orden 1-5, juntar 2+3 y contar hasta 7).
    /// </summary>
    private static void AddKindergartenAgeBands(PedagogicalTopic topic)
    {
        var ex4 = new Exercise
        {
            Id = "count_ex_04",
            Title = "Dos Estrellas en el Cielo",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 3,
            TargetAgeMax = 4,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 2,
            StandardInstruction = "Mira las estrellas. Toca cada una y cuenta. ¿Cuántas estrellas hay?",
            SimplifiedInstruction = "Toca cada estrella. ¿Cuántas hay?",
            ScreenReaderDescription = "Hay estrellas en el cielo. Cuéntalas una a una.",
            FirstHint = "Toca la primera estrella. Luego la segunda.",
            SecondHintDetailed = "Cuenta: una, dos. Hay 2 estrellas.",
            ExplanationOfSuccess = "¡Muy bien! Hay 2 estrellas.",
            SimplifiedFirstHint = "Toca una estrella. Luego la otra.",
            SimplifiedSecondHint = "Cuenta: una, dos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Hay 2 estrellas."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "s1", Name = "Estrella 1", EmojiSymbol = "⭐", AccessibleDescription = "Primera estrella", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "s2", Name = "Estrella 2", EmojiSymbol = "⭐", AccessibleDescription = "Segunda estrella", OrderIndex = 2, MusicalPitchHz = Frequencies[1] });
        ex4.Options.Add(new ExerciseOption { Id = "opt1", Label = "1 estrella", Value = 1, IsCorrect = false, SpeechFeedback = "Hay otra estrella. Cuenta las dos.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt2", Label = "2 estrellas", Value = 2, IsCorrect = true, SpeechFeedback = "¡Sí! Hay 2 estrellas.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt3", Label = "3 estrellas", Value = 3, IsCorrect = false, SpeechFeedback = "Solo hay dos estrellas.", AccessibleKeyShortcut = "3" });
        topic.Exercises.Add(ex4);

        var ex5 = new Exercise
        {
            Id = "count_ex_05",
            Title = "El Camino de Tres Piedras",
            GameType = GamePlayType.OrderSequence,
            TargetAgeMin = 3,
            TargetAgeMax = 4,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 3,
            StandardInstruction = "Las piedras dicen: 1, 2... ¿Qué número sigue?",
            SimplifiedInstruction = "Las piedras dicen: 1, 2... ¿Qué número sigue?",
            ScreenReaderDescription = "Tres piedras en el camino. Una está vacía.",
            FirstHint = "Después del 2 viene el 3.",
            SecondHintDetailed = "Cuenta: uno, dos, tres.",
            ExplanationOfSuccess = "¡Bien! El 3 sigue después del 2.",
            SimplifiedFirstHint = "Después del 2 viene el 3.",
            SimplifiedSecondHint = "Cuenta: uno, dos, tres.",
            SimplifiedExplanationOfSuccess = "¡Bien! El 3 sigue después del 2."
        };
        ex5.ItemsGroupA.Add(new ManipulableItem { Id = "k1", Name = "Piedra 1", EmojiSymbol = "🪨 1", AccessibleDescription = "Piedra con el número 1", OrderIndex = 1, MusicalPitchHz = Frequencies[0], IsCounted = true });
        ex5.ItemsGroupA.Add(new ManipulableItem { Id = "k2", Name = "Piedra 2", EmojiSymbol = "🪨 2", AccessibleDescription = "Piedra con el número 2", OrderIndex = 2, MusicalPitchHz = Frequencies[1], IsCounted = true });
        ex5.ItemsGroupA.Add(new ManipulableItem { Id = "k3", Name = "Piedra ?", EmojiSymbol = "❓", AccessibleDescription = "Piedra vacía esperando un número", OrderIndex = 3, MusicalPitchHz = Frequencies[2] });
        ex5.Options.Add(new ExerciseOption { Id = "opt1", Label = "2", Value = 2, IsCorrect = false, SpeechFeedback = "El 2 ya está. Busca el que sigue.", AccessibleKeyShortcut = "1" });
        ex5.Options.Add(new ExerciseOption { Id = "opt2", Label = "3", Value = 3, IsCorrect = true, SpeechFeedback = "¡Sí! Después del 2 viene el 3.", AccessibleKeyShortcut = "2" });
        ex5.Options.Add(new ExerciseOption { Id = "opt3", Label = "4", Value = 4, IsCorrect = false, SpeechFeedback = "El 4 viene después del 3.", AccessibleKeyShortcut = "3" });
        topic.Exercises.Add(ex5);

        var ex6 = new Exercise
        {
            Id = "count_ex_06",
            Title = "Las Cuatro Campanas",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 4,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 4,
            StandardInstruction = "Toca cada campana y cuenta. ¿Cuántas campanas hay?",
            SimplifiedInstruction = "Toca cada campana. ¿Cuántas hay?",
            ScreenReaderDescription = "Hay campanas en la mesa. Cuéntalas una a una.",
            FirstHint = "Toca una campana y di un número. Sigue con la siguiente.",
            SecondHintDetailed = "Cuenta: una, dos, tres, cuatro. Hay 4 campanas.",
            ExplanationOfSuccess = "¡Muy bien! Hay 4 campanas.",
            SimplifiedFirstHint = "Toca cada campana una vez.",
            SimplifiedSecondHint = "Cuenta: una, dos, tres, cuatro.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Hay 4 campanas."
        };
        for (int i = 0; i < 4; i++)
        {
            ex6.ItemsGroupA.Add(new ManipulableItem { Id = $"b{i + 1}", Name = $"Campana {i + 1}", EmojiSymbol = "🔔", AccessibleDescription = $"Campana número {i + 1}", OrderIndex = i + 1, MusicalPitchHz = Frequencies[i] });
        }
        ex6.Options.Add(new ExerciseOption { Id = "opt1", Label = "3 campanas", Value = 3, IsCorrect = false, SpeechFeedback = "Falta una campana por contar.", AccessibleKeyShortcut = "1" });
        ex6.Options.Add(new ExerciseOption { Id = "opt2", Label = "4 campanas", Value = 4, IsCorrect = true, SpeechFeedback = "¡Sí! Hay 4 campanas.", AccessibleKeyShortcut = "2" });
        ex6.Options.Add(new ExerciseOption { Id = "opt3", Label = "5 campanas", Value = 5, IsCorrect = false, SpeechFeedback = "Solo hay cuatro campanas.", AccessibleKeyShortcut = "3" });
        topic.Exercises.Add(ex6);

        var ex7 = new Exercise
        {
            Id = "count_ex_07",
            Title = "¿Dónde Hay Más Manzanas?",
            GameType = GamePlayType.Comparison,
            TargetAgeMin = 4,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 4,
            StandardInstruction = "Mira los dos grupos. ¿En cuál hay más manzanas?",
            SimplifiedInstruction = "¿Dónde hay más manzanas?",
            ScreenReaderDescription = "Dos grupos de manzanas. Compara cuál tiene más.",
            FirstHint = "Cuenta las manzanas de cada grupo.",
            SecondHintDetailed = "Un grupo tiene 3 y el otro tiene 4. El de 4 tiene más.",
            ExplanationOfSuccess = "¡Bien! El grupo de 4 tiene más manzanas.",
            SimplifiedFirstHint = "Cuenta las manzanas de cada grupo.",
            SimplifiedSecondHint = "El grupo de 4 tiene más.",
            SimplifiedExplanationOfSuccess = "¡Bien! El grupo de 4 tiene más."
        };
        for (int i = 0; i < 3; i++)
        {
            ex7.ItemsGroupA.Add(new ManipulableItem { Id = $"a{i + 1}", Name = $"Manzana {i + 1}", EmojiSymbol = "🍎", AccessibleDescription = $"Manzana del grupo A número {i + 1}", OrderIndex = i + 1, MusicalPitchHz = Frequencies[i] });
        }
        for (int i = 0; i < 4; i++)
        {
            ex7.ItemsGroupB.Add(new ManipulableItem { Id = $"b{i + 1}", Name = $"Manzana {i + 1}", EmojiSymbol = "🍎", AccessibleDescription = $"Manzana del grupo B número {i + 1}", OrderIndex = i + 1, MusicalPitchHz = Frequencies[i] });
        }
        ex7.Options.Add(new ExerciseOption { Id = "opt1", Label = "3 manzanas", Value = 3, IsCorrect = false, SpeechFeedback = "Ese grupo tiene menos.", AccessibleKeyShortcut = "1" });
        ex7.Options.Add(new ExerciseOption { Id = "opt2", Label = "4 manzanas", Value = 4, IsCorrect = true, SpeechFeedback = "¡Sí! 4 es más que 3.", AccessibleKeyShortcut = "2" });
        ex7.Options.Add(new ExerciseOption { Id = "opt3", Label = "Igual", Value = 0, IsCorrect = false, SpeechFeedback = "Los grupos no tienen la misma cantidad.", AccessibleKeyShortcut = "3" });
        topic.Exercises.Add(ex7);

        var ex8 = new Exercise
        {
            Id = "count_ex_08",
            Title = "El Río de los Cinco Números",
            GameType = GamePlayType.OrderSequence,
            TargetAgeMin = 5,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 5,
            StandardInstruction = "Las piedras dicen: 1, 2, 3, 4... ¿Qué número sigue?",
            SimplifiedInstruction = "Las piedras dicen: 1, 2, 3, 4... ¿Qué número sigue?",
            ScreenReaderDescription = "Cinco piedras en el río. La última está vacía.",
            FirstHint = "Después del 4 viene el 5.",
            SecondHintDetailed = "Cuenta: uno, dos, tres, cuatro, cinco.",
            ExplanationOfSuccess = "¡Bien! El 5 sigue después del 4.",
            SimplifiedFirstHint = "Después del 4 viene el 5.",
            SimplifiedSecondHint = "Cuenta: uno, dos, tres, cuatro, cinco.",
            SimplifiedExplanationOfSuccess = "¡Bien! El 5 sigue después del 4."
        };
        for (int i = 1; i <= 4; i++)
        {
            ex8.ItemsGroupA.Add(new ManipulableItem { Id = $"r{i}", Name = $"Piedra {i}", EmojiSymbol = $"🪨 {i}", AccessibleDescription = $"Piedra con el número {i}", OrderIndex = i, MusicalPitchHz = Frequencies[i - 1], IsCounted = true });
        }
        ex8.ItemsGroupA.Add(new ManipulableItem { Id = "r5", Name = "Piedra ?", EmojiSymbol = "❓", AccessibleDescription = "Piedra vacía esperando un número", OrderIndex = 5, MusicalPitchHz = Frequencies[4] });
        ex8.Options.Add(new ExerciseOption { Id = "opt1", Label = "4", Value = 4, IsCorrect = false, SpeechFeedback = "El 4 ya está. Busca el que sigue.", AccessibleKeyShortcut = "1" });
        ex8.Options.Add(new ExerciseOption { Id = "opt2", Label = "5", Value = 5, IsCorrect = true, SpeechFeedback = "¡Sí! Después del 4 viene el 5.", AccessibleKeyShortcut = "2" });
        ex8.Options.Add(new ExerciseOption { Id = "opt3", Label = "6", Value = 6, IsCorrect = false, SpeechFeedback = "El 6 viene después del 5.", AccessibleKeyShortcut = "3" });
        topic.Exercises.Add(ex8);

        var ex9 = new Exercise
        {
            Id = "count_ex_09",
            Title = "La Cesta de Manzanas",
            GameType = GamePlayType.AdditionCombine,
            TargetAgeMin = 5,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 5,
            StandardInstruction = "Hay 2 manzanas en una cesta y 3 en otra. ¿Cuántas manzanas hay en total?",
            SimplifiedInstruction = "Hay 2 manzanas y 3 manzanas. ¿Cuántas hay en total?",
            ScreenReaderDescription = "Dos grupos de manzanas que se juntan en una cesta.",
            FirstHint = "Cuenta todas las manzanas: 1, 2, 3, 4, 5.",
            SecondHintDetailed = "2 manzanas más 3 manzanas son 5 manzanas.",
            ExplanationOfSuccess = "¡Muy bien! 2 y 3 son 5 manzanas.",
            SimplifiedFirstHint = "Cuenta todas las manzanas.",
            SimplifiedSecondHint = "2 y 3 son 5.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Hay 5 manzanas."
        };
        for (int i = 0; i < 2; i++)
        {
            ex9.ItemsGroupA.Add(new ManipulableItem { Id = $"c{i + 1}", Name = $"Manzana {i + 1}", EmojiSymbol = "🍎", AccessibleDescription = $"Manzana de la primera cesta número {i + 1}", OrderIndex = i + 1, MusicalPitchHz = Frequencies[i] });
        }
        for (int i = 0; i < 3; i++)
        {
            ex9.ItemsGroupB.Add(new ManipulableItem { Id = $"d{i + 1}", Name = $"Manzana {i + 1}", EmojiSymbol = "🍎", AccessibleDescription = $"Manzana de la segunda cesta número {i + 1}", OrderIndex = i + 1, MusicalPitchHz = Frequencies[i + 2] });
        }
        ex9.Options.Add(new ExerciseOption { Id = "opt1", Label = "4 manzanas", Value = 4, IsCorrect = false, SpeechFeedback = "Cuenta una manzana más.", AccessibleKeyShortcut = "1" });
        ex9.Options.Add(new ExerciseOption { Id = "opt2", Label = "5 manzanas", Value = 5, IsCorrect = true, SpeechFeedback = "¡Sí! 2 y 3 son 5.", AccessibleKeyShortcut = "2" });
        ex9.Options.Add(new ExerciseOption { Id = "opt3", Label = "6 manzanas", Value = 6, IsCorrect = false, SpeechFeedback = "Son menos de seis.", AccessibleKeyShortcut = "3" });
        topic.Exercises.Add(ex9);

        var ex10 = new Exercise
        {
            Id = "count_ex_10",
            Title = "Siete Tesoros del Bosque",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 5,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 7,
            StandardInstruction = "Toca cada tesoro y cuenta. ¿Cuántos tesoros hay?",
            SimplifiedInstruction = "Toca cada tesoro. ¿Cuántos hay?",
            ScreenReaderDescription = "Hay varios tesoros en el bosque. Cuéntalos todos.",
            FirstHint = "Toca cada tesoro una vez y di un número.",
            SecondHintDetailed = "Cuenta: 1, 2, 3, 4, 5, 6, 7. Hay 7 tesoros.",
            ExplanationOfSuccess = "¡Muy bien! Hay 7 tesoros.",
            SimplifiedFirstHint = "Toca cada tesoro una vez.",
            SimplifiedSecondHint = "Cuenta: 1, 2, 3, 4, 5, 6, 7.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Hay 7 tesoros."
        };
        for (int i = 0; i < 7; i++)
        {
            ex10.ItemsGroupA.Add(new ManipulableItem { Id = $"t{i + 1}", Name = $"Tesoro {i + 1}", EmojiSymbol = i % 2 == 0 ? "💎" : "⭐", AccessibleDescription = $"Tesoro número {i + 1}", OrderIndex = i + 1, MusicalPitchHz = Frequencies[i % Frequencies.Length] });
        }
        ex10.Options.Add(new ExerciseOption { Id = "opt1", Label = "6 tesoros", Value = 6, IsCorrect = false, SpeechFeedback = "Falta un tesoro por contar.", AccessibleKeyShortcut = "1" });
        ex10.Options.Add(new ExerciseOption { Id = "opt2", Label = "7 tesoros", Value = 7, IsCorrect = true, SpeechFeedback = "¡Sí! Hay 7 tesoros.", AccessibleKeyShortcut = "2" });
        ex10.Options.Add(new ExerciseOption { Id = "opt3", Label = "8 tesoros", Value = 8, IsCorrect = false, SpeechFeedback = "Solo hay siete tesoros.", AccessibleKeyShortcut = "3" });
        topic.Exercises.Add(ex10);
    }

    /// <summary>
    /// Tema 2: Fundamentos y Propiedades de la Suma (Primaria baja: 6 a 8 años)
    /// </summary>
    private PedagogicalTopic BuildAdditionAndPropertiesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_addition_01",
            Title = "La Gran Fiesta de la Suma y sus Superpoderes",
            Level = AcademicLevel.EarlyElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido a la Aldea de los Constructores! Aquí los números aman juntarse. Cuando dos grupos de amigos se unen en un solo gran equipo, ¡estamos sumando! Pero la suma tiene superpoderes secretos llamados propiedades. ¿Quieres conocerlos?",
            NarrativeIntroSimplified = "Sumar es juntar amigos en un solo grupo. Vamos a jugar y descubrir sus trucos mágicos.",
            CoreConceptExplanation = "Sumar significa agregar o juntar dos o más cantidades. Sus propiedades mágicas son:\n1. Conmutativa: Cambiar el orden de los sumandos no cambia la suma (3 + 2 = 2 + 3).\n2. Asociativa: Agrupar en parejas distintas no cambia el total ((1 + 2) + 3 = 1 + (2 + 3)).\n3. Elemento Neutro: Sumar cero deja el número exactamente igual (5 + 0 = 5).",
            CoreConceptSimplified = "Sumar es juntar cosas. Si cambias el orden de los números, el resultado sigue siendo el mismo.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto fundamental de adición (juntar colecciones)",
                "Propiedad Conmutativa: a + b = b + a",
                "Propiedad Asociativa: (a + b) + c = a + (b + c)",
                "Elemento Neutro de la suma: a + 0 = a"
            },
            CongratulationMessage = "¡Eres todo un maestro de la suma! Conoces la conmutativa, la asociativa y el poder del cero. ¡Brillante!"
        };

        // Ejercicio 1: Fundamento de la suma (Juntar colecciones)
        var ex1 = new Exercise
        {
            Id = "add_ex_01",
            Title = "La Cesta de Frutas de los Amigos",
            GameType = GamePlayType.AdditionCombine,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 5,
            StandardInstruction = "La ardilla Ada tiene 3 bellotas doradas en su cesta. El erizo Tito llega con 2 bellotas más y las echa a la misma cesta. Al juntarlas todas, ¿cuántas bellotas tienen ahora? ¡Estamos haciendo 3 + 2!",
            SimplifiedInstruction = "Tienes 3 bellotas y te regalan 2 más. Júntalas todas. ¿Cuántas hay en total?",
            ScreenReaderDescription = "Un grupo A con 3 bellotas y un grupo B con 2 bellotas se unen en una sola cesta grande.",
            FirstHint = "Empieza con las 3 bellotas de Ada y cuenta dos pasos hacia adelante: cuatro, cinco...",
            SecondHintDetailed = "3 bellotas más 2 bellotas es igual a 5 bellotas en total.",
            ExplanationOfSuccess = "¡Perfecto! 3 + 2 = 5. Cuando juntamos dos grupos para saber el gran total, ¡eso es sumar!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "a1", Name = "Bellota 1", EmojiSymbol = "🌰", AccessibleDescription = "Bellota 1 de Ada", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "a2", Name = "Bellota 2", EmojiSymbol = "🌰", AccessibleDescription = "Bellota 2 de Ada", OrderIndex = 2, MusicalPitchHz = Frequencies[1] });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "a3", Name = "Bellota 3", EmojiSymbol = "🌰", AccessibleDescription = "Bellota 3 de Ada", OrderIndex = 3, MusicalPitchHz = Frequencies[2] });

        ex1.ItemsGroupB.Add(new ManipulableItem { Id = "b1", Name = "Bellota 4", EmojiSymbol = "🌰", AccessibleDescription = "Bellota 1 de Tito", OrderIndex = 4, MusicalPitchHz = Frequencies[3] });
        ex1.ItemsGroupB.Add(new ManipulableItem { Id = "b2", Name = "Bellota 5", EmojiSymbol = "🌰", AccessibleDescription = "Bellota 2 de Tito", OrderIndex = 5, MusicalPitchHz = Frequencies[4] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "4 bellotas", Value = 4, IsCorrect = false, SpeechFeedback = "Falta una bellotita por sumar.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "5 bellotas", Value = 5, IsCorrect = true, SpeechFeedback = "¡Exacto! 3 más 2 es igual a 5.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "6 bellotas", Value = 6, IsCorrect = false, SpeechFeedback = "Contaste una de más.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);

        // Ejercicio 2: Propiedad Conmutativa
        var ex2 = new Exercise
        {
            Id = "add_ex_02",
            Title = "El Espejo Mágico: Propiedad Conmutativa",
            GameType = GamePlayType.CommutativePropertyDemo,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 7,
            StandardInstruction = "Observa este gran truco: si sumamos 4 mariposas azules + 3 mariposas amarillas obtenemos 7. Ahora las mariposas dan una vuelta en el aire y quedan 3 amarillas + 4 azules. ¿Cuánto da 3 + 4?",
            SimplifiedInstruction = "Si 4 + 3 son 7... al cambiar el orden a 3 + 4, ¿cuánto dará?",
            ScreenReaderDescription = "Comparación de dos sumas: 4 + 3 y 3 + 4. Muestra visualmente que el resultado no cambia aunque cambien de lugar.",
            FirstHint = "¡Recuerda el superpoder conmutativo! Cambiar de lugar a los amigos no cambia el resultado final.",
            SecondHintDetailed = "Tanto 4 + 3 como 3 + 4 dan el mismo número: 7.",
            ExplanationOfSuccess = "¡Maravilloso! 4 + 3 = 7 y 3 + 4 = 7. El orden de los sumandos no cambia la suma. ¡Esa es la famosa Propiedad Conmutativa!"
        };

        ex2.Options.Add(new ExerciseOption { Id = "opt1", Label = "7 (El mismo total)", Value = 7, IsCorrect = true, SpeechFeedback = "¡Magnífico! Siempre da 7 porque el orden no altera la suma.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt2", Label = "6", Value = 6, IsCorrect = false, SpeechFeedback = "Mmm, ¿seguro? Recuerda que son las mismas mariposas.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt3", Label = "8", Value = 8, IsCorrect = false, SpeechFeedback = "No agregamos ninguna mariposa nueva.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex2);

        // Ejercicio 3: Elemento Neutro (El Cero)
        var ex3 = new Exercise
        {
            Id = "add_ex_03",
            Title = "La Nube Transparente: El Cero en la Suma",
            GameType = GamePlayType.ZeroNeutralProperty,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 6,
            StandardInstruction = "En una canasta tienes 6 deliciosas fresas. Cosmo trae una caja mágica vacía con 0 fresas y las suma a la canasta. ¿Cuántas fresas tienes en total? ¡Es la operación 6 + 0!",
            SimplifiedInstruction = "Tienes 6 fresas. Si le sumas 0 fresas, ¿cuántas te quedan?",
            ScreenReaderDescription = "Una cesta con 6 fresas y un plato vacío con 0 fresas. Operación matemática: 6 + 0.",
            FirstHint = "El cero significa 'ninguno'. Si no agregas ninguna fresa, ¿cambian tus fresas?",
            SecondHintDetailed = "6 + 0 sigue siendo 6. El cero es el elemento neutro: deja al número tal como estaba.",
            ExplanationOfSuccess = "¡Excelente! 6 + 0 = 6. El cero es el elemento neutro de la suma: no suma nada y el número queda intacto."
        };

        ex3.Options.Add(new ExerciseOption { Id = "opt1", Label = "0 fresas", Value = 0, IsCorrect = false, SpeechFeedback = "¡Cuidado! Las 6 fresas que ya tenías no desaparecieron.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt2", Label = "6 fresas", Value = 6, IsCorrect = true, SpeechFeedback = "¡Exacto! Sigues teniendo 6 fresas porque el cero no altera el valor.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt3", Label = "7 fresas", Value = 7, IsCorrect = false, SpeechFeedback = "No agregamos una fresa, agregamos cero.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex3);

        // Ejercicio 4: Propiedad Asociativa
        var ex4 = new Exercise
        {
            Id = "add_ex_04",
            Title = "Los Vagones del Tren: Propiedad Asociativa",
            GameType = GamePlayType.AssociativePropertyDemo,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 6,
            StandardInstruction = "El tren de la Aldea transporta 6 tesoros repartidos en tres vagones: 2 en el vagón 1, 3 en el vagón 2 y 1 en el vagón 3. La locomotora prueba dos maneras de agrupar: primero une el vagón 1 con el 2 (2 + 3 = 5) y después agrega el vagón 3: 5 + 1 = 6. Luego prueba agrupando el vagón 2 con el 3 (3 + 1 = 4) y después une el vagón 1: 2 + 4 = 6. ¿Cuántos tesoros lleva el tren en total?",
            SimplifiedInstruction = "En el tren hay vagones con 2, 3 y 1 tesoros. Primero suma dos de los vagones y luego el tercero: 2 + 3 + 1. ¿Cuántos tesoros hay en total?",
            ScreenReaderDescription = "Tren con tres vagones: vagón 1 con 2 tesoros, vagón 2 con 3 tesoros y vagón 3 con 1 tesoro. Agrupa los tesoros de dos maneras distintas y compara los resultados.",
            FirstHint = "Elige una pareja de vagones para empezar: 2 + 3 = 5. Ahora suma el último vagón: 5 + 1.",
            SecondHintDetailed = "Las dos agrupaciones dan el mismo resultado: (2 + 3) + 1 = 6 y 2 + (3 + 1) = 6.",
            ExplanationOfSuccess = "¡Estupendo! Cambiar la manera de agrupar los sumandos no cambia el total: (2 + 3) + 1 = 2 + (3 + 1) = 6. ¡Esa es la Propiedad Asociativa, el superpoder de los vagones del tren!"
        };

        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "w1a", Name = "Tesoro 1 del vagón 1", EmojiSymbol = "💎", AccessibleDescription = "Tesoro 1 del vagón 1", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "w1b", Name = "Tesoro 2 del vagón 1", EmojiSymbol = "💎", AccessibleDescription = "Tesoro 2 del vagón 1", OrderIndex = 2, MusicalPitchHz = Frequencies[1] });
        ex4.ItemsGroupB.Add(new ManipulableItem { Id = "w2a", Name = "Tesoro 1 del vagón 2", EmojiSymbol = "💎", AccessibleDescription = "Tesoro 1 del vagón 2", OrderIndex = 3, MusicalPitchHz = Frequencies[2] });
        ex4.ItemsGroupB.Add(new ManipulableItem { Id = "w2b", Name = "Tesoro 2 del vagón 2", EmojiSymbol = "💎", AccessibleDescription = "Tesoro 2 del vagón 2", OrderIndex = 4, MusicalPitchHz = Frequencies[3] });
        ex4.ItemsGroupB.Add(new ManipulableItem { Id = "w2c", Name = "Tesoro 3 del vagón 2", EmojiSymbol = "💎", AccessibleDescription = "Tesoro 3 del vagón 2", OrderIndex = 5, MusicalPitchHz = Frequencies[4] });
        ex4.ItemsGroupB.Add(new ManipulableItem { Id = "w3a", Name = "Tesoro del vagón 3", EmojiSymbol = "💎", AccessibleDescription = "Tesoro del vagón 3", OrderIndex = 6, MusicalPitchHz = Frequencies[5] });

        ex4.Options.Add(new ExerciseOption { Id = "opt1", Label = "5 tesoros", Value = 5, IsCorrect = false, SpeechFeedback = "5 es solo la primera agrupación (2 + 3); falta sumar el vagón con 1 tesoro.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt2", Label = "6 tesoros", Value = 6, IsCorrect = true, SpeechFeedback = "¡Exacto! 2 + 3 + 1 son 6 tesoros, sin importar cómo agrupes.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt3", Label = "7 tesoros", Value = 7, IsCorrect = false, SpeechFeedback = "Contamos un tesoro de más; revisa los vagones: 2, 3 y 1.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex4);

        return topic;
    }

    /// <summary>
    /// Tema 3: Fundamentos de la Resta y la Relación Inversa
    /// </summary>
    private PedagogicalTopic BuildSubtractionAndInverseTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_subtraction_01",
            Title = "El Sendero del Retorno: La Resta y sus Misterios",
            Level = AcademicLevel.EarlyElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Si la suma es dar pasos hacia adelante o invitar amigos a la fiesta, la resta es dar pasos hacia atrás o despedir a los amigos que van a descansar. Pero lo más emocionante es que la resta es el espejo mágico de la suma: ¡puede deshacer cualquier paso!",
            NarrativeIntroSimplified = "Restar es quitar o dar pasos atrás. Si sumamos y luego restamos lo mismo, volvemos al principio.",
            CoreConceptExplanation = "Restar consiste en quitar una cantidad a otra o calcular la diferencia entre dos números. Además, la suma y la resta son operaciones inversas: si 5 + 3 = 8, entonces 8 - 3 = 5.",
            CoreConceptSimplified = "Restar es quitar cosas de un grupo y ver cuántas quedaron.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto fundamental de sustracción (quitar / separar)",
                "Retroceder en la recta numérica",
                "Relación inversa entre la suma y la resta"
            },
            CongratulationMessage = "¡Misión cumplida! Ahora sabes restar y utilizar el camino de regreso. ¡Eres imparable!"
        };

        // Ejercicio 1: Quitar elementos
        var ex1 = new Exercise
        {
            Id = "sub_ex_01",
            Title = "Los Patitos en el Estanque",
            GameType = GamePlayType.SubtractionTakeAway,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 3,
            StandardInstruction = "En el estanque nadaban alegremente 5 patitos dorados. De pronto, 2 patitos salieron a dormir una siesta en el pasto. ¿Cuántos patitos siguen nadando en el agua? ¡Calcula 5 - 2!",
            SimplifiedInstruction = "Había 5 patitos. 2 patitos se fueron. ¿Cuántos patitos quedaron en el agua?",
            ScreenReaderDescription = "5 patitos nadando en el agua. Dos de ellos se van nadando hacia la orilla. Observa cuántos siguen en el centro.",
            FirstHint = "Teníamos 5. Cuenta hacia atrás dos pasitos: 4... 3...",
            SecondHintDetailed = "5 patitos menos 2 patitos es igual a 3 patitos.",
            ExplanationOfSuccess = "¡Brillante! 5 - 2 = 3. Al quitar o separar una parte de la colección, descubrimos cuántos quedan."
        };

        for (int i = 0; i < 5; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem { Id = $"p{i + 1}", Name = $"Patito {i + 1}", EmojiSymbol = "🦆", AccessibleDescription = $"Patito nadando número {i + 1}", OrderIndex = i + 1, MusicalPitchHz = Frequencies[i] });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "2 patitos", Value = 2, IsCorrect = false, SpeechFeedback = "2 son los que se fueron a dormir.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "3 patitos", Value = 3, IsCorrect = true, SpeechFeedback = "¡Magnífico! Quedan 3 patitos nadando en el agua.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4 patitos", Value = 4, IsCorrect = false, SpeechFeedback = "Se fueron 2 patitos, no 1 solo.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);

        // Ejercicio 2: Relación Inversa
        var ex2 = new Exercise
        {
            Id = "sub_ex_02",
            Title = "La Máquina del Tiempo Numérica (Relación Inversa)",
            GameType = GamePlayType.InverseRelation,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 4,
            StandardInstruction = "Sabemos que 4 + 3 = 7. Si metemos el 7 en nuestra máquina del tiempo y le restamos 3 (7 - 3), ¿a qué número original regresaremos?",
            SimplifiedInstruction = "Si 4 más 3 son 7, entonces si a 7 le quitamos 3... ¿qué número nos queda?",
            ScreenReaderDescription = "Una flecha va de 4 a 7 sumando 3. Otra flecha va de 7 hacia atrás restando 3.",
            FirstHint = "La resta deshace lo que hizo la suma. ¡Vuelve al número donde empezó todo!",
            SecondHintDetailed = "Si 4 + 3 = 7, al restar 3 volvemos directamente al 4.",
            ExplanationOfSuccess = "¡Espectacular! Si 4 + 3 = 7, entonces 7 - 3 = 4. ¡La suma y la resta son dos caras de la misma moneda mágica!"
        };

        ex2.Options.Add(new ExerciseOption { Id = "opt1", Label = "4", Value = 4, IsCorrect = true, SpeechFeedback = "¡Exacto! Regresamos al 4, el número con el que comenzamos.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt2", Label = "3", Value = 3, IsCorrect = false, SpeechFeedback = "3 fue la cantidad que sumamos y restamos.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt3", Label = "5", Value = 5, IsCorrect = false, SpeechFeedback = "Ese no es el número inicial. ¡Revisa la suma original!", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex2);

        return topic;
    }

    /// <summary>
    /// Tema 4: Semilla a Multiplicación y Programación (Bucles / Repetición)
    /// </summary>
    private PedagogicalTopic BuildRepeatedAdditionAndProgrammingSeedTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_seed_01",
            Title = "La Ranita Robot: Sumas Repetidas y Bucles de Código",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¿Sabías que los programadores de computadoras usan matemáticas todo el tiempo? En programación, cuando queremos hacer la misma acción varias veces, usamos algo llamado 'Bucle' o 'Repetición'. ¡Y en matemáticas, sumar el mismo número varias veces es el secreto de la multiplicación!",
            NarrativeIntroSimplified = "En las computadoras repetimos pasos para crear juegos. Repetir sumas es multiplicar.",
            CoreConceptExplanation = "La multiplicación es una forma rápida y elegante de sumar el mismo número muchas veces. Por ejemplo, si una ranita robot da 3 saltos de 2 metros cada uno, hace: 2 + 2 + 2 = 6 metros. ¡En código de computadora decimos: 'Repetir 3 veces: dar 2 pasos'!",
            CoreConceptSimplified = "Si das 3 saltos y cada salto avanza 2 pasos, sumas 2 + 2 + 2 = 6.",
            PrinciplesAddressed = new List<string>
            {
                "Suma repetida de sumandos iguales",
                "Fundamento del producto (multiplicación)",
                "Pensamiento computacional: concepto de Bucle (Loop / Repeat)"
            },
            CongratulationMessage = "¡Felicidades, futuro programador! Has conectado las matemáticas con el código informático."
        };

        var ex1 = new Exercise
        {
            Id = "prog_ex_01",
            Title = "El Robot de Entrenamiento: tu Primer Bucle de Código",
            GameType = GamePlayType.PatternAndLoopSeed,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 6,
            StandardInstruction = "Antes de escribir código real, todo programador entrena el pensamiento computacional con diagramas. Nuestro robot de entrenamiento ejecuta la instrucción: 'REPETIR 3 VECES { AVANZAR 2 PASOS }'. Es decir: 2 pasos en el primer salto + 2 en el segundo + 2 en el tercero. Este patrón de repetición se llama bucle (loop). ¿Cuántos pasos avanzó en total? Calcula 2 + 2 + 2.",
            SimplifiedInstruction = "El robot repite 3 veces la orden de avanzar 2 pasos: 2 + 2 + 2. ¿Cuántos pasos dio en total?",
            ScreenReaderDescription = "Un robot de entrenamiento ejecuta 3 repeticiones idénticas sobre la recta numérica. Cada repetición mide 2 unidades. Observa dónde aterriza al terminar el bucle.",
            FirstHint = "Suma primero las dos primeras repeticiones: 2 + 2 = 4. Luego añade la tercera: 4 + 2...",
            SecondHintDetailed = "2 + 2 + 2 es igual a 6. Tres veces el número 2 son 6: en programación lo notarías como 3 × 2.",
            ExplanationOfSuccess = "¡Impresionante! 2 + 2 + 2 = 6, lo que equivale a 3 veces 2 (3 × 2 = 6). Acabas de resolver tu primer algoritmo matemático con un bucle: la base de toda la programación."
        };

        for (int i = 1; i <= 3; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"frog_jump_{i}",
                Name = $"Salto #{i} (+2 pasos)",
                EmojiSymbol = "🐸",
                AccessibleDescription = $"Salto número {i} del bucle: avanza 2 pasos (acumulado: {i * 2})",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "5 pasos", Value = 5, IsCorrect = false, SpeechFeedback = "Falta sumar el último paso del salto.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "6 pasos (3 veces 2)", Value = 6, IsCorrect = true, SpeechFeedback = "¡Sensacional! 2 + 2 + 2 = 6. ¡Pensamiento algorítmico al 100%!", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4 pasos", Value = 4, IsCorrect = false, SpeechFeedback = "Eso son solo 2 saltos. La ranita dio 3 saltos.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);

        return topic;
    }

    /// <summary>
    /// Tema 5: Multiplicación Temprana (Matrices rectangulares, grupos iguales y conmutatividad del producto)
    /// </summary>
    private PedagogicalTopic BuildMultiplicationAndArraysTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_multiplication_01",
            Title = "El Huerto de las Multiplicaciones: Filas, Columnas y Grupos Iguales",
            Level = AcademicLevel.EarlyElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido al Gran Huerto de los Patrones! Multiplicar no es memorizar listas aburridas: es descubrir la belleza de organizar cosas en filas y columnas rectangulares o armar grupos exactamente iguales. Cuando tienes 3 filas con 4 zanahorias, ¡la multiplicación te dice el total en un pestañeo!",
            NarrativeIntroSimplified = "Multiplicar es sumar grupos iguales muy rápido. Si tienes 3 filas con 2 manzanas, sumas 2 + 2 + 2.",
            CoreConceptExplanation = "Multiplicar significa calcular el total de varios grupos que tienen la misma cantidad de elementos. Se representa con matrices de filas por columnas. Su superpoder es la conmutatividad: 2 filas de 4 manzanas dan exactamente el mismo total que 4 filas de 2 manzanas (2 × 4 = 4 × 2 = 8).",
            CoreConceptSimplified = "Multiplicar es juntar filas que tienen la misma cantidad de cosas.",
            PrinciplesAddressed = new List<string>
            {
                "Multiplicación como suma repetida de sumandos iguales",
                "Disposición rectangular en matrices (filas × columnas)",
                "Propiedad conmutativa del producto: a × b = b × a"
            },
            CongratulationMessage = "¡Fabuloso! Ahora comprendes la multiplicación a través de filas y grupos iguales. ¡Eres un maestro de los patrones!"
        };

        // Ejercicio introductorio base
        var ex1 = new Exercise
        {
            Id = "mult_ex_01",
            Title = "Las Filas de Manzanas del Huerto",
            GameType = GamePlayType.MultiplicationArray,
            TargetAgeMin = 6,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 6,
            StandardInstruction = "Cosmo organizó sus manzanas en 2 filas bien ordenadas. Cada fila tiene 3 manzanas rojas. ¿Cuántas manzanas hay en total? ¡Es la multiplicación 2 × 3!",
            SimplifiedInstruction = "Hay 2 filas. Cada fila tiene 3 manzanas. ¿Cuántas hay en total?",
            ScreenReaderDescription = "Una cuadrícula de 2 filas con 3 manzanas cada una. Observa cómo las filas iguales se suman para multiplicar.",
            FirstHint = "Suma la primera fila más la segunda fila: 3 + 3...",
            SecondHintDetailed = "2 veces 3 es igual a 6. 2 × 3 = 6.",
            ExplanationOfSuccess = "¡Extraordinario! 2 × 3 = 6. Dos filas de tres manzanas nos dan seis manzanas en total."
        };

        for (int i = 1; i <= 6; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"m_item_{i}",
                Name = $"Manzana #{i}",
                EmojiSymbol = "🍎",
                AccessibleDescription = $"Manzana número {i} del huerto",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "5 manzanas", Value = 5, IsCorrect = false, SpeechFeedback = "Mmm, te falta una manzana. Recuerda que son 3 + 3.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "6 manzanas (2 × 3)", Value = 6, IsCorrect = true, SpeechFeedback = "¡Exacto! 2 veces 3 es igual a 6.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "7 manzanas", Value = 7, IsCorrect = false, SpeechFeedback = "Son solo 2 filas de 3, no agregamos de más.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 6: División Temprana (Reparto equitativo y relación inversa con el producto)
    /// </summary>
    private PedagogicalTopic BuildDivisionAndFairSharingTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_division_01",
            Title = "El Banquete de los Amigos: División y Reparto Equitativo",
            Level = AcademicLevel.EarlyElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "En el Reino Matemágico la regla de oro de la amistad es la equidad: cuando tenemos tesoros o deliciosas frutas para compartir, ¡todos los amigos reciben exactamente la misma cantidad! Dividir es repartir en partes iguales para que todos celebren contentos.",
            NarrativeIntroSimplified = "Dividir es repartir cosas entre amigos para que todos tengan lo mismo y nadie se quede sin nada.",
            CoreConceptExplanation = "Dividir significa repartir una cantidad total en partes exactamente iguales entre un número de receptores o grupos. Además, es el camino inverso de la multiplicación: si 2 amigos reciben 3 galletas cada uno (2 × 3 = 6), entonces 6 galletas repartidas entre 2 amigos da 3 a cada uno (6 ÷ 2 = 3).",
            CoreConceptSimplified = "Dividir es dar una porción igual a cada amigo sin que sobre nada.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto fundamental de división como reparto equitativo (Fair Sharing)",
                "Partición exacta en partes iguales",
                "Relación inversa entre la multiplicación y la división: (a × b = c) <=> (c ÷ b = a)"
            },
            CongratulationMessage = "¡Felicidades! Eres el repartidor más justo del reino. Comprendes cómo compartir y dividir con equidad."
        };

        // Ejercicio introductorio base
        var ex1 = new Exercise
        {
            Id = "div_ex_01",
            Title = "El Reparto de Bellotas entre Ardillas",
            GameType = GamePlayType.DivisionFairSharing,
            TargetAgeMin = 6,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 3,
            StandardInstruction = "Tenemos 6 ricas bellotas para repartir equitativamente entre 2 ardillitas amigas. ¿Cuántas bellotas le tocan a cada ardillita para que sea perfectamente justo? ¡Es 6 ÷ 2!",
            SimplifiedInstruction = "Tienes 6 bellotas para 2 ardillas. Repártelas en partes iguales. ¿Cuántas le tocan a cada una?",
            ScreenReaderDescription = "6 bellotas en el centro y 2 ardillitas esperando su parte igual.",
            FirstHint = "Si le das 1 a cada una gastas 2. Si le das otra a cada una gastas 4. Con otra más gastas las 6.",
            SecondHintDetailed = "6 repartido entre 2 ardillas da 3 bellotas a cada una.",
            ExplanationOfSuccess = "¡Magnífico! 6 ÷ 2 = 3. Cada ardillita recibe 3 bellotas y ninguna se queda triste."
        };

        for (int i = 1; i <= 6; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"div_m_{i}",
                Name = $"Bellota #{i}",
                EmojiSymbol = "🌰",
                AccessibleDescription = $"Bellota número {i} para repartir",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "2 bellotas a cada una", Value = 2, IsCorrect = false, SpeechFeedback = "Con 2 a cada una sólo repartes 4, todavía te sobran bellotas.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "3 bellotas a cada una", Value = 3, IsCorrect = true, SpeechFeedback = "¡Exacto! 6 dividido entre 2 es igual a 3.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4 bellotas a cada una", Value = 4, IsCorrect = false, SpeechFeedback = "No alcanzan para darle 4 a cada una, necesitarías 8 bellotas.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 6 (4º Grado): Fracciones y Partes Iguales (Numerador, Denominador y Equivalencias)
    /// </summary>
    private PedagogicalTopic BuildFractionsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_fractions_01",
            Title = "La Pastelería Mágica: El Secreto de las Fracciones y Partes Iguales",
            Level = AcademicLevel.MiddleElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenidos a la Pastelería Mágica! Cuando compartimos un delicioso pastel o una pizza con amigos, no siempre comemos piezas enteras: las cortamos en rebanadas exactamente iguales. ¡Eso son las fracciones!",
            NarrativeIntroSimplified = "En la pastelería cortamos pasteles en partes iguales. Las partes de un todo se llaman fracciones.",
            CoreConceptExplanation = "Una fracción expresa partes de un todo dividido en pedazos iguales. El número de abajo (denominador) indica en cuántas partes iguales se dividió el todo. El número de arriba (numerador) indica cuántas partes tomamos o comemos. Además, dos fracciones con números distintos pueden representar la misma cantidad: ¡1/2 es exactamente igual a 2/4!",
            CoreConceptSimplified = "El número de abajo dice el total de partes iguales. El de arriba dice cuántas partes tomaste.",
            PrinciplesAddressed = new List<string>
            {
                "Comprensión de la fracción como parte de un todo unitario",
                "Identificación del numerador (partes consideradas) y denominador (total de partes congruentes)",
                "Noción de equivalencia de fracciones (representación visual de 1/2 = 2/4)"
            },
            CongratulationMessage = "¡Formidable! Ahora eres un maestro pastelero de las fracciones. Sabes leer partes iguales y encontrar equivalencias mágicas."
        };

        // Ejercicio 1: Numerador y denominador en un pastel
        var ex1 = new Exercise
        {
            Id = "frac_ex_01",
            Title = "El Pastel de Fresas de la Pastelería",
            GameType = GamePlayType.FractionParts,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 3,
            StandardInstruction = "Un pastel se cortó en 4 rebanadas iguales. Los amigos comieron 3 de esas rebanadas. ¿Cuál es el numerador (la cantidad de rebanadas que se comieron)?",
            SimplifiedInstruction = "El pastel tiene 4 pedazos iguales. Se comieron 3. ¿Cuántos pedazos se comieron?",
            ScreenReaderDescription = "Un pastel redondo dividido en 4 rebanadas idénticas. Observa cuántas rebanadas han sido tomadas y cuántas siguen en el plato.",
            FirstHint = "El número de arriba (numerador) indica las partes tomadas: cuenta cuántas rebanadas se comieron.",
            SecondHintDetailed = "Se comieron 3 de 4 partes. El numerador es 3 y la fracción es 3/4.",
            ExplanationOfSuccess = "¡Correcto! El numerador es 3. La fracción es 3/4: tomaron 3 partes de un total de 4 partes iguales."
        };

        for (int i = 1; i <= 4; i++)
        {
            bool isTaken = i <= 3;
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"frac_slice_{i}",
                Name = $"Rebanada {i}",
                EmojiSymbol = "🍰",
                AccessibleDescription = isTaken ? $"Rebanada {i} comida" : $"Rebanada {i} disponible",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "2 rebanadas", Value = 2, IsCorrect = false, SpeechFeedback = "No, se tomaron más de 2 rebanadas. Cuenta las tomadas.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "3 rebanadas (3/4)", Value = 3, IsCorrect = true, SpeechFeedback = "¡Magnífico! 3 rebanadas de 4. El numerador es 3.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4 rebanadas", Value = 4, IsCorrect = false, SpeechFeedback = "4 es el total de pedazos (el denominador), pero solo se comieron 3.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);

        // Ejercicio 2: Equivalencia (1/2 = 2/4)
        var ex2 = new Exercise
        {
            Id = "frac_ex_02",
            Title = "El Espejo de las Fracciones Equivalentes",
            GameType = GamePlayType.FractionEquivalence,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 1,
            StandardInstruction = "Ada comió 1/2 de su barra de chocolate. Tito comió 2/4 de una barra exactamente igual. ¿Ambos comieron la misma cantidad de chocolate?",
            SimplifiedInstruction = "¿La mitad (1/2) es del mismo tamaño que 2 cuartos (2/4)?",
            ScreenReaderDescription = "Dos barras de chocolate iguales. Una partida a la mitad (1 de 2), la otra en 4 pedazos (2 de 4). Ambas cubren exactamente el mismo tamaño.",
            FirstHint = "Piensa: si divides 1 rebanada grande en 2 pequeñas, tienes 2 de 4. ¡Ocupan el mismo espacio!",
            SecondHintDetailed = "1/2 y 2/4 son fracciones equivalentes: representan la misma cantidad de chocolate.",
            ExplanationOfSuccess = "¡Exacto! 1/2 y 2/4 son fracciones equivalentes. Aunque los números cambien, el tamaño de chocolate comido es idéntico."
        };

        ex2.Options.Add(new ExerciseOption { Id = "opt_yes", Label = "¡Sí! Son equivalentes (1/2 = 2/4)", Value = 1, IsCorrect = true, SpeechFeedback = "¡Exacto! Son fracciones equivalentes.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_no", Label = "No, uno comió más que el otro", Value = 2, IsCorrect = false, SpeechFeedback = "Aunque 4 pedazos son más pequeños, 2 de ellos hacen exactamente media barra.", AccessibleKeyShortcut = "2" });

        topic.Exercises.Add(ex2);
        return topic;
    }

    /// <summary>
    /// Tema 7 (4º Grado): Multiplicación por Descomposición y Propiedad Distributiva
    /// </summary>
    private PedagogicalTopic BuildDistributiveMultiplicationTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_distributive_01",
            Title = "Multiplicar Descomponiendo en Bloques: Propiedad Distributiva",
            Level = AcademicLevel.MiddleElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Cuando los números se hacen grandes y retadores, ¡los matemáticos inteligentes no se asustan! Usamos un superpoder secreto: los desarmamos en bloques de decenas y unidades, multiplicamos cada bloque por separado y luego sumamos los resultados.",
            NarrativeIntroSimplified = "Para multiplicar números grandes, los rompemos en partes fáciles (decenas y unidades), multiplicamos y sumamos.",
            CoreConceptExplanation = "La propiedad distributiva nos enseña que multiplicar una suma por un número es igual a sumar las multiplicaciones individuales: (a + b) × c = (a × c) + (b × c). Por ejemplo, 14 × 3 se calcula desarmando 14 en (10 + 4), multiplicando (10 × 3 = 30) y (4 × 3 = 12), y sumando al final: 30 + 12 = 42.",
            CoreConceptSimplified = "Rompe un número como 14 en 10 y 4. Multiplica cada parte y suma los resultados.",
            PrinciplesAddressed = new List<string>
            {
                "Descomposición aditiva en potencias de diez (decenas y unidades)",
                "Propiedad distributiva de la multiplicación sobre la adición: (a + b) × c = (a × c) + (b × c)",
                "Estrategia de cálculo mental flexible para productos multidígito"
            },
            CongratulationMessage = "¡Extraordinario estratega! Has dominado la propiedad distributiva. Multiplicar números grandes ahora es pan comido para ti."
        };

        var ex1 = new Exercise
        {
            Id = "distrib_ex_01",
            Title = "Descomponiendo 14 × 3 en Bloques",
            GameType = GamePlayType.MultiplicationDistributive,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 42,
            StandardInstruction = "Para calcular 14 × 3, descomponemos 14 en (10 + 4). Primero calculamos (10 × 3 = 30) y luego (4 × 3 = 12). Al sumar 30 + 12, ¿cuál es el resultado final de 14 × 3?",
            SimplifiedInstruction = "Rompe 14 en 10 y 4. Diez por tres es 30, y cuatro por tres es 12. Suma 30 + 12.",
            ScreenReaderDescription = "Multiplicación distributiva de 14 por 3. Bloque de decenas: 10 por 3 igual a 30. Bloque de unidades: 4 por 3 igual a 12. Total sumado: 30 más 12.",
            FirstHint = "Suma el resultado de las decenas (30) más las unidades (12).",
            SecondHintDetailed = "30 + 12 = 42.",
            ExplanationOfSuccess = "¡Espectacular! 14 × 3 = 42. Descomponer números grandes en decenas y unidades hace que la multiplicación sea rápida y sin errores."
        };

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "38", Value = 38, IsCorrect = false, SpeechFeedback = "Cerca, pero revisa la suma de 30 más 12.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "42", Value = 42, IsCorrect = true, SpeechFeedback = "¡Magnífico! 30 + 12 = 42. 14 × 3 = 42.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "52", Value = 52, IsCorrect = false, SpeechFeedback = "Sumaste diez de más. 30 + 12 es 42.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 8 (5º Grado): Fracciones Mixtas y Más de un Entero
    /// </summary>
    private PedagogicalTopic BuildMixedFractionsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_mixed_fractions_01",
            Title = "La Cocina Mágica: Fracciones Mixtas y Más de un Entero",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡En la cocina mágica, a veces horneamos tanto que un solo molde no alcanza! Cuando tenemos uno o más enteros completos junto con una fracción, formamos un número mixto o una fracción impropia. ¡Acompáñame a contar deliciosas rebanadas!",
            NarrativeIntroSimplified = "Cuando tenemos cosas completas y un pedacito más, tenemos fracciones mixtas.",
            CoreConceptExplanation = "Un número mixto combina enteros y una fracción propia (ej. 1 1/2). Si cortamos el entero en pedazos iguales al denominador, podemos expresarlo como fracción impropia (3/2), donde el numerador es mayor que el denominador.",
            CoreConceptSimplified = "1 waffle entero cortado a la mitad son 2 pedazos. Con 1 pedazo extra tienes 3 pedazos de mitad (3/2).",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de número mixto (parte entera y parte fraccionaria propia)",
                "Conversión visual y concreta a fracción impropia: 1 1/2 = 3/2",
                "Comprensión de cantidades fraccionarias mayores a la unidad"
            },
            CongratulationMessage = "¡Fabuloso! Ahora eres un maestro cocinero de las fracciones mixtas. Entiendes lo que significa tener más de un entero completo."
        };

        var ex1 = new Exercise
        {
            Id = "mixed_ex_01",
            Title = "Waffles con Fruta: 1 Entero y 1/2",
            GameType = GamePlayType.MixedFractions,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 3,
            StandardInstruction = "Ada horneó 1 waffle entero y 1/2 de otro. Si cada waffle se divide en 2 pedazos iguales, ¿cuántos pedazos de 1/2 hay en total en 1 y 1/2?",
            SimplifiedInstruction = "Tienes 1 waffle entero partido en 2, más 1 pedazo extra. ¿Cuántos pedazos hay en total?",
            ScreenReaderDescription = "Un waffle entero partido en 2 porciones y medio waffle más partido en 1 porción. Cuenta cuántas porciones de waffle hay en total.",
            FirstHint = "1 waffle entero son 2 medios pedazos. Suma el pedazo suelto.",
            SecondHintDetailed = "2 medios + 1 medio = 3 medios pedazos (3/2).",
            ExplanationOfSuccess = "¡Correcto! 1 y 1/2 waffle equivalen a 3 medios pedazos (3/2). ¡Es una fracción impropia!"
        };

        for (int i = 1; i <= 3; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"mix_w_{i}",
                Name = $"Porción de waffle {i}",
                EmojiSymbol = "🧇",
                AccessibleDescription = i <= 2 ? $"Porción {i} del primer waffle" : "Porción extra del segundo waffle",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "2 partes", Value = 2, IsCorrect = false, SpeechFeedback = "Eso solo sería el primer waffle entero. Falta el medio waffle extra.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "3 partes (3/2)", Value = 3, IsCorrect = true, SpeechFeedback = "¡Magnífico! 2 mitades del entero más 1 mitad extra son 3 mitades (3/2).", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4 partes", Value = 4, IsCorrect = false, SpeechFeedback = "Para 4 mitades necesitarías 2 waffles enteros completos.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 9 (5º Grado): Números Decimales y Monedas
    /// </summary>
    private PedagogicalTopic BuildDecimalsAndMoneyTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_decimals_01",
            Title = "El Mercado de los Centavos: Números Decimales y Monedas",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenidos al Mercado de las Frutas y Monedas! Cuando compramos en la tiendita o en el mercado, los precios no siempre son números enteros: usamos monedas de centavos para pagar partes de un peso. El punto decimal separa los pesos enteros de los centavos.",
            NarrativeIntroSimplified = "Los números decimales nos sirven para contar dinero: pesos y monedas de centavos.",
            CoreConceptExplanation = "El sistema decimal extiende el valor posicional a partes más pequeñas que uno: décimos (0.1) y centésimos (0.01). Un peso entero tiene 100 centavos ($1.00), y media moneda son 50 centavos ($0.50). Juntos hacen $1.50.",
            CoreConceptSimplified = "El punto decimal separa los pesos enteros de los centavitos sueltos.",
            PrinciplesAddressed = new List<string>
            {
                "Comprensión de los decimales como fracciones decimales (décimos y centésimos)",
                "Aplicación funcional del sistema decimal en monedas y dinero cotidiano",
                "Interpretación del valor posicional a la derecha del punto decimal"
            },
            CongratulationMessage = "¡Excelente comprador! Has dominado el valor de las monedas y los números decimales en el mercado."
        };

        var ex1 = new Exercise
        {
            Id = "dec_ex_01",
            Title = "Comprando Frutas: $1.50 en Monedas",
            GameType = GamePlayType.DecimalsMoney,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 150,
            StandardInstruction = "En el mercado tienes 1 moneda de $1.00 (100 centavos) y 1 moneda de $0.50 (50 centavos). ¿Cuántos centavos tienes en total? (Equivale a $1.50).",
            SimplifiedInstruction = "Tienes 1 moneda de $1 (100 centavos) y 1 moneda de 50 centavos. ¿Cuántos centavos tienes en total?",
            ScreenReaderDescription = "1 moneda dorada de un peso (100 centavos) y 1 moneda plateada de cincuenta centavos. Añade el valor de todas las monedas.",
            FirstHint = "Suma 100 centavos más 50 centavos.",
            SecondHintDetailed = "100 + 50 = 150 centavos ($1.50).",
            ExplanationOfSuccess = "¡Exacto! Tienes 150 centavos, que se escribe $1.50 con un punto decimal. ¡Los decimales representan las partes sueltas de un entero!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "coin_1", Name = "Moneda de $1.00", EmojiSymbol = "🪙", AccessibleDescription = "Moneda dorada de 1 peso (100 centavos)", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex1.ItemsGroupB.Add(new ManipulableItem { Id = "coin_2", Name = "Moneda de $0.50", EmojiSymbol = "🥈", AccessibleDescription = "Moneda plateada de 50 centavos", OrderIndex = 2, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "120 centavos ($1.20)", Value = 120, IsCorrect = false, SpeechFeedback = "No, recuerda que la moneda plateada vale 50 centavos.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "150 centavos ($1.50)", Value = 150, IsCorrect = true, SpeechFeedback = "¡Magnífico! 100 + 50 = 150 centavos ($1.50).", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "200 centavos ($2.00)", Value = 200, IsCorrect = false, SpeechFeedback = "Para 200 centavos necesitarías dos monedas de un peso.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 10 (5º Grado): Primeras Potencias y Crecimiento Exponencial
    /// </summary>
    private PedagogicalTopic BuildPowersAndExponentsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_powers_01",
            Title = "El Cofre Duplicador: Primeras Potencias y Crecimiento Mágico",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Has llegado a la Cueva de los Tesoros Legendarios! Aquí encontramos el legendario Cofre Duplicador: cada vez que subes un nivel, el número de gemas no solo se suma, ¡se multiplica por sí mismo! Este es el asombroso poder de los exponentes.",
            NarrativeIntroSimplified = "El cofre duplica cosas: 2 al cubo es 2 por 2 por 2 = 8. Las potencias multiplican por el mismo número varias veces.",
            CoreConceptExplanation = "Una potencia representa una multiplicación repetida de la misma base: a^n. En base 2: 2^1 = 2, 2^2 = 2 × 2 = 4, 2^3 = 2 × 2 × 2 = 8. El exponente nos indica cuántas veces multiplicamos la base por sí misma.",
            CoreConceptSimplified = "El número chiquito de arriba (exponente) te dice cuántas veces multiplicar el número por sí mismo.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de potencia como multiplicación iterada de un factor constante",
                "Diferenciación entre producto (2 × 3 = 6) y potenciación (2^3 = 8)",
                "Modelado intuitivo del crecimiento exponencial"
            },
            CongratulationMessage = "¡Poder sin límites! Comprendes cómo crecen las potencias y los exponentes mágicos."
        };

        var ex1 = new Exercise
        {
            Id = "pow_ex_01",
            Title = "El Poder de 2 Elevado a la 3 (2³)",
            GameType = GamePlayType.PowersAndExponentGrowth,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 8,
            StandardInstruction = "En el cofre duplicador colocamos un cristal con potencia 3: 2³. Esto significa multiplicar 2 por sí mismo 3 veces: (2 × 2 × 2). Primero 2 × 2 = 4, y luego 4 × 2 = ... ¿cuántos cristales hay al final?",
            SimplifiedInstruction = "2 multiplicado por sí mismo 3 veces: 2 × 2 × 2. ¿Cuánto da?",
            ScreenReaderDescription = "Potencia de base 2 y exponente 3. Multiplicación repetida: 2 por 2 por 2.",
            FirstHint = "Multiplica paso a paso: 2 × 2 = 4. Ahora multiplica 4 × 2.",
            SecondHintDetailed = "4 × 2 = 8. Por tanto, 2³ = 8.",
            ExplanationOfSuccess = "¡Extraordinario! 2³ = 8. ¡Cuidado de no confundirlo con 2 × 3 que es 6! Las potencias crecen mucho más rápido."
        };

        for (int i = 1; i <= 8; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"gem_pow_{i}",
                Name = $"Cristal de Poder {i}",
                EmojiSymbol = "💎",
                AccessibleDescription = $"Cristal brillante duplicado número {i}",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "6 cristales (como 2 × 3)", Value = 6, IsCorrect = false, SpeechFeedback = "¡Cuidado con la trampa! 2 × 3 es sumar 2 tres veces. En potencia multiplicas: 2 × 2 = 4, y 4 × 2 = 8.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "8 cristales (2³ = 2 × 2 × 2)", Value = 8, IsCorrect = true, SpeechFeedback = "¡Magnífico! 2 × 2 × 2 = 8. 2³ = 8.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "9 cristales", Value = 9, IsCorrect = false, SpeechFeedback = "9 es 3 × 3. Aquí multiplicamos doses: 2 × 2 × 2 = 8.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 11 (6º Grado): Proporcionalidad Directa y Regla de Tres
    /// </summary>
    private PedagogicalTopic BuildDirectProportionTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_proportion_01",
            Title = "La Alquimia de las Pociones: Proporcionalidad Directa",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido al Laboratorio de Alquimia! Cuando duplicamos o triplicamos una receta mágica, todos los ingredientes deben crecer en la misma proporción para que la poción funcione a la perfección. ¡Eso es la proporcionalidad directa y la regla de tres!",
            NarrativeIntroSimplified = "En la cocina mágica, si haces el doble de comida necesitas el doble de ingredientes.",
            CoreConceptExplanation = "Dos magnitudes son directamente proporcionales cuando al multiplicar una por un número, la otra queda multiplicada por el mismo número: y = k · x. La constante de proporcionalidad k nos dice cuánto vale una sola unidad.",
            CoreConceptSimplified = "Averigua cuánto da 1 flor. Luego multiplica por las flores que tengas.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de razón y proporcionalidad directa",
                "Constante de proporcionalidad unitaria (k = y / x)",
                "Resolución intuitiva de problemas de regla de tres simple"
            },
            CongratulationMessage = "¡Maestro Alquimista! Sabes mantener la proporción perfecta en cualquier receta o misión."
        };

        var ex1 = new Exercise
        {
            Id = "prop_ex_01",
            Title = "Pociones de Curación: 2 Flores para 6 Frascos",
            GameType = GamePlayType.DirectProportion,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 12,
            StandardInstruction = "En el caldero, con 2 flores mágicas preparamos 6 frascos de poción. Si usamos 4 flores mágicas manteniendo la misma proporción, ¿cuántos frascos obtendremos?",
            SimplifiedInstruction = "2 flores hacen 6 pociones. ¿Cuántas pociones harás con 4 flores (el doble)?",
            ScreenReaderDescription = "Proporción directa: 2 flores dan 6 frascos. Razón: 3 frascos por flor. Con 4 flores, ¿cuántos frascos se obtienen?",
            FirstHint = "Como 4 flores es el doble de 2 flores, obtendrás el doble de pociones: 6 × 2.",
            SecondHintDetailed = "1 flor da 3 pociones (6 ÷ 2 = 3). Con 4 flores: 4 × 3 = 12 pociones.",
            ExplanationOfSuccess = "¡Excelente! La razón es de 3 frascos por flor. Con 4 flores preparas exactamente 12 frascos."
        };

        for (int i = 1; i <= 4; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"prop_fl_{i}",
                Name = $"Flor #{i}",
                EmojiSymbol = "🌸",
                AccessibleDescription = $"Flor mágica {i} en el caldero",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "8 frascos", Value = 8, IsCorrect = false, SpeechFeedback = "No se suma 2, se multiplica por la misma proporción.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "12 frascos", Value = 12, IsCorrect = true, SpeechFeedback = "¡Magnífico! 4 × 3 = 12 frascos de poción.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "18 frascos", Value = 18, IsCorrect = false, SpeechFeedback = "Eso sería con 6 flores. Con 4 son 12 frascos.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 12 (6º Grado): Pensamiento Algebraico y Balanza de Incógnitas
    /// </summary>
    private PedagogicalTopic BuildAlgebraicBalanceTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_algebra_balance_01",
            Title = "La Balanza Dorada: Ecuaciones y la Incógnita Secreta",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Has descubierto la legendaria Balanza Dorada de los Sabios! Una ecuación matemática es exactamente como una balanza: el signo igual (=) significa equilibrio absoluto. Para descubrir el valor de la caja secreta X, debemos hacer lo mismo en ambos lados sin perder el balance.",
            NarrativeIntroSimplified = "Una ecuación es como una balanza. Lo que quites de un lado, quítalo del otro para que no se caiga.",
            CoreConceptExplanation = "En una ecuación lineal x + a = b, la incógnita x representa el valor desconocido que hace cierta la igualdad. Aplicando la propiedad uniforme de la igualdad, restamos 'a' en ambos miembros para despejar x: x = b - a.",
            CoreConceptSimplified = "Quita las gemas visibles de ambos lados para saber cuánto pesa la caja misteriosa X.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de ecuación como igualdad y equilibrio algebraico",
                "Despeje de la incógnita mediante operaciones inversas",
                "Propiedad uniforme de la igualdad (balanza matemática)"
            },
            CongratulationMessage = "¡Mente brillante! Has dominado el arte del álgebra y el equilibrio de incógnitas."
        };

        var ex1 = new Exercise
        {
            Id = "alg_ex_01",
            Title = "El Misterio de la Caja X: X + 3 = 8",
            GameType = GamePlayType.AlgebraicBalance,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 5,
            StandardInstruction = "En la balanza de oro, el platillo izquierdo tiene una caja mágica X y 3 gemas. El platillo derecho tiene 8 gemas. Como la balanza está en equilibrio perfecto (X + 3 = 8), ¿cuántas gemas contiene la caja X?",
            SimplifiedInstruction = "Caja X más 3 gemas pesa igual a 8 gemas. ¿Cuánto vale X?",
            ScreenReaderDescription = "Balanza nivelada. Platillo izquierdo: caja misteriosa X y 3 gemas. Platillo derecho: 8 gemas. Ecuación: X + 3 = 8.",
            FirstHint = "Para despejar X, quita 3 gemas de ambos platillos: 8 - 3.",
            SecondHintDetailed = "X = 8 - 3 = 5 gemas.",
            ExplanationOfSuccess = "¡Extraordinario! Despejaste la incógnita: X = 8 - 3 = 5. Comprobamos: 5 + 3 = 8."
        };

        for (int i = 1; i <= 3; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem { Id = $"g_left_{i}", Name = $"Gema izquierda #{i}", EmojiSymbol = "💎", AccessibleDescription = $"Gema visible {i} con la caja X", OrderIndex = i, MusicalPitchHz = Frequencies[0] });
        }
        for (int i = 1; i <= 8; i++)
        {
            ex1.ItemsGroupB.Add(new ManipulableItem { Id = $"g_right_{i}", Name = $"Gema derecha #{i}", EmojiSymbol = "✨", AccessibleDescription = $"Gema derecha {i}", OrderIndex = i, MusicalPitchHz = Frequencies[2] });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "X = 4 gemas", Value = 4, IsCorrect = false, SpeechFeedback = "4 + 3 daría 7 gemas, faltaría una para equilibrar.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "X = 5 gemas", Value = 5, IsCorrect = true, SpeechFeedback = "¡Magnífico! X = 8 - 3 = 5. 5 + 3 = 8.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "X = 11 gemas", Value = 11, IsCorrect = false, SpeechFeedback = "Sumaste en vez de restar. Para despejar restamos 3 de 8.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 13 (6º Grado): Raíz Cuadrada y Cuadrados Perfectos
    /// </summary>
    private PedagogicalTopic BuildSquareRootTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_square_root_01",
            Title = "El Laberinto de los Cuadrados Perfectos: Raíz Cuadrada",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Adéntrate en el Laberinto Cuadrado! En este reino, todos los templos tienen pisos con el mismo número de baldosas a lo largo que a lo ancho (cuadrados perfectos). La raíz cuadrada es la llave mágica que deshace la potencia al cuadrado y nos revela la longitud exacta de cada lado.",
            NarrativeIntroSimplified = "La raíz cuadrada te dice qué número multiplicado por sí mismo da el total.",
            CoreConceptExplanation = "La raíz cuadrada de un número x es aquel valor no negativo 'r' que multiplicado por sí mismo produce x: √x = r <=> r² = x. Para cuadrados perfectos: √4 = 2, √9 = 3, √16 = 4, √25 = 5.",
            CoreConceptSimplified = "Si un piso tiene 9 cuadros, cada lado tiene 3 cuadros porque 3 por 3 es 9.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de raíz cuadrada como operación inversa de la potenciación cuadrática",
                "Identificación geométrica de cuadrados perfectos (lado de un área cuadrada)",
                "Resolución exacta de raíces de números fundamentales (4, 9, 16, 25)"
            },
            CongratulationMessage = "¡Fabuloso Geómetra! Conoces la raíz cuadrada y el misterio de los cuadrados perfectos."
        };

        var ex1 = new Exercise
        {
            Id = "sqrt_ex_01",
            Title = "El Templo de las 9 Baldosas: √9",
            GameType = GamePlayType.SquareRootPerfect,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 3,
            StandardInstruction = "El piso de la sala de meditación es un cuadrado perfecto formado por 9 baldosas mágicas en total (3 filas de 3 columnas). ¿Cuántas baldosas mide cada lado de la sala? (Calcula la raíz cuadrada: √9)",
            SimplifiedInstruction = "El piso cuadrado tiene 9 baldosas. ¿Cuántas baldosas hay en cada fila y columna?",
            ScreenReaderDescription = "Cuadrado perfecto de 9 losas. Observa cómo se organizan en filas y columnas iguales para hallar la raíz cuadrada.",
            FirstHint = "Piensa: ¿qué número multiplicado por sí mismo es igual a 9?",
            SecondHintDetailed = "3 × 3 = 9, por lo tanto √9 = 3.",
            ExplanationOfSuccess = "¡Brillante! √9 = 3 porque 3² = 3 × 3 = 9. La raíz cuadrada nos devuelve la medida del lado."
        };

        for (int i = 1; i <= 9; i++)
        {
            ex1.ItemsGroupA.Add(new ManipulableItem
            {
                Id = $"sq_tile_{i}",
                Name = $"Baldosa #{i}",
                EmojiSymbol = "🟩",
                AccessibleDescription = $"Baldosa número {i} del templo cuadrado",
                OrderIndex = i,
                MusicalPitchHz = Frequencies[(i - 1) % Frequencies.Length]
            });
        }

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "2 baldosas por lado", Value = 2, IsCorrect = false, SpeechFeedback = "2 × 2 es 4 baldosas, no 9.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "3 baldosas por lado (√9 = 3)", Value = 3, IsCorrect = true, SpeechFeedback = "¡Exacto! 3 × 3 = 9. √9 = 3.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4.5 baldosas", Value = 4, IsCorrect = false, SpeechFeedback = "No se divide entre 2. Buscamos un número que por sí mismo dé 9.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 14 (6º Grado): Coordenadas Cartesianas en el Primer Cuadrante
    /// </summary>
    private PedagogicalTopic BuildCartesianCoordinatesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_cartesian_coordinates_01",
            Title = "El Mapa Náutico: Coordenadas Cartesianas (X, Y)",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡A toda vela, navegante! En alta mar no existen calles con nombres: usamos una cuadrícula mágica inventada por los astrónomos. Primero contamos los pasos hacia el este en la recta horizontal (eje X) y luego hacia el norte en la recta vertical (eje Y). ¡Las coordenadas (X, Y) te guían al tesoro!",
            NarrativeIntroSimplified = "El mapa tiene dos ejes: primero caminas a la derecha (X) y luego subes (Y).",
            CoreConceptExplanation = "El plano cartesiano ubica puntos unívocos mediante un par ordenado (x, y). El primer valor (abscisa x) mide la distancia horizontal desde el origen (0,0), y el segundo valor (ordenada y) mide la distancia vertical.",
            CoreConceptSimplified = "El par (X, Y) es como una dirección secreta: X es horizontal, Y es vertical.",
            PrinciplesAddressed = new List<string>
            {
                "Lectura e interpretación de pares ordenados (x, y) en el primer cuadrante",
                "Diferenciación entre eje horizontal (abscisas X) y eje vertical (ordenadas Y)",
                "Ubicación y localización espacial accesible"
            },
            CongratulationMessage = "¡Capitán de los Mares! Has descifrado las coordenadas cartesianas y la orientación en el plano."
        };

        var ex1 = new Exercise
        {
            Id = "coord_ex_01",
            Title = "La Isla de la Calavera: Coordenada (4, 2)",
            GameType = GamePlayType.CartesianCoordinates,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 4,
            StandardInstruction = "En el mapa náutico, la Isla de la Calavera está marcada con las coordenadas (4, 2). Si el primer valor siempre representa el eje horizontal X (este), ¿cuántos pasos al este marca la coordenada X?",
            SimplifiedInstruction = "En el punto (4, 2), el primer número es X. ¿Cuánto vale X?",
            ScreenReaderDescription = "Plano cartesiano con un punto en coordenadas X=4, Y=2. Pregunta por el valor del eje X horizontal.",
            FirstHint = "En el par (X, Y), el primer número es X y el segundo es Y.",
            SecondHintDetailed = "El punto es (4, 2). El eje X horizontal vale 4.",
            ExplanationOfSuccess = "¡Extraordinario! En el par ordenado (4, 2), la coordenada horizontal X es 4 y la vertical Y es 2."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "point_island",
            Name = "Isla en (4, 2)",
            EmojiSymbol = "🏝️",
            AccessibleDescription = "Isla marcada en X=4 horizontal, Y=2 vertical",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[3]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "2 pasos (eje Y)", Value = 2, IsCorrect = false, SpeechFeedback = "2 es el segundo número (eje vertical Y). Buscamos el eje horizontal X.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "4 pasos (eje X)", Value = 4, IsCorrect = true, SpeechFeedback = "¡Correcto! El primer número es 4, correspondiente al eje horizontal X.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "6 pasos", Value = 6, IsCorrect = false, SpeechFeedback = "No sumamos los números; leemos el primer valor del par (4, 2).", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 15 (1º Secundaria): Números Enteros con Signo y Recta Numérica
    /// </summary>
    private PedagogicalTopic BuildSignedNumbersTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_signed_numbers_01",
            Title = "La Tiendita y la Recta Numérica: Números con Signo",
            Level = AcademicLevel.SecondaryFirstGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido a 1º de Secundaria! En la vida cotidiana no todos los números son positivos: en la tiendita cuando pedimos fiado tenemos una deuda (saldo negativo), y en invierno la temperatura en la sierra puede quedar en grados bajo cero. Los números enteros con signo (+ y -) nos permiten calcular ganancias, pérdidas y balances exactos.",
            NarrativeIntroSimplified = "Los números con signo nos ayudan a contar deudas (-) y dinero que tenemos (+).",
            CoreConceptExplanation = "El conjunto de los números enteros (Z) incluye los positivos, el cero y los negativos. En una suma (+a) + (-b), si el número negativo tiene mayor valor absoluto, el resultado final es negativo: representa una deuda o déficit.",
            CoreConceptSimplified = "Si tienes 5 pesos pero debes 8 pesos, pagas tus 5 y todavía quedas debiendo 3 (-3).",
            PrinciplesAddressed = new List<string>
            {
                "Comprensión intuitiva y formal de números enteros positivos y negativos",
                "Modelado de saldo y deudas en situaciones cotidianas",
                "Adición y sustracción de números con signo en la recta numérica"
            },
            CongratulationMessage = "¡Felicidades! Dominas los números positivos y negativos como un experto financiero."
        };

        var ex1 = new Exercise
        {
            Id = "sign_ex_01",
            Title = "El Saldo en la Tiendita: (+5) + (-8)",
            GameType = GamePlayType.SignedNumbersAddition,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = -3,
            StandardInstruction = "En la tiendita tenías $5 pesos a favor (+5). Compraste un jugo y unas papitas por $8 pesos fiados (-8). Al sumar (+5) + (-8), ¿cuál es tu saldo final?",
            SimplifiedInstruction = "Tienes 5 pesos y debes 8. ¿Cuánto quedas debiendo?",
            ScreenReaderDescription = "Suma de enteros: positivo 5 más negativo 8. Sigue la flecha sobre la recta numérica para conocer el saldo.",
            FirstHint = "Como debes más de lo que tienes, el resultado es negativo.",
            SecondHintDetailed = "5 - 8 = -3 pesos (debes 3 pesos).",
            ExplanationOfSuccess = "¡Exacto! (+5) + (-8) = -3. Quedas a deber 3 pesos en la tiendita."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "bal_1", Name = "Dinero propio (+5)", EmojiSymbol = "🪙", AccessibleDescription = "Monedas a favor de 5 pesos", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex1.ItemsGroupB.Add(new ManipulableItem { Id = "debt_1", Name = "Cuenta fiada (-8)", EmojiSymbol = "🧾", AccessibleDescription = "Nota de deuda de 8 pesos", OrderIndex = 2, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "-3 pesos (debes 3)", Value = -3, IsCorrect = true, SpeechFeedback = "¡Correcto! 5 menos 8 es igual a -3 pesos.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "+3 pesos (te sobran 3)", Value = 3, IsCorrect = false, SpeechFeedback = "No te sobra dinero, gastaste más de lo que tenías.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "-13 pesos", Value = -13, IsCorrect = false, SpeechFeedback = "No se sumaron dos deudas; tus 5 pesos pagaron parte de los 8.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 16 (1º Secundaria): Jerarquía de Operaciones y Paréntesis (PEMDAS)
    /// </summary>
    private PedagogicalTopic BuildOrderOfOperationsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_order_operations_01",
            Title = "El Código de la Jerarquía: Orden de Operaciones y Paréntesis (PEMDAS)",
            Level = AcademicLevel.SecondaryFirstGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Cuando los científicos y programadores escriben fórmulas matemáticas, todos deben entender el mismo resultado sin confusiones. Por eso existe una regla universal llamada Jerarquía de Operaciones (PEMDAS): primero se resuelven los paréntesis, luego potencias y raíces, después multiplicaciones y divisiones, y al final sumas y restas.",
            NarrativeIntroSimplified = "En matemáticas hay reglas de turno: la multiplicación siempre se resuelve antes que la suma.",
            CoreConceptExplanation = "La jerarquía de operaciones establece el orden estricto de resolución: 1º Paréntesis, 2º Exponentes y raíces, 3º Multiplicaciones y Divisiones (de izquierda a derecha), 4º Sumas y Restas (de izquierda a derecha).",
            CoreConceptSimplified = "Primero multiplica. Después suma el resultado.",
            PrinciplesAddressed = new List<string>
            {
                "Convención universal de precedencia de operadores aritméticos (PEMDAS)",
                "Prioridad de la multiplicación y división sobre la suma y resta",
                "Uso de paréntesis como modificadores de la prioridad de cálculo"
            },
            CongratulationMessage = "¡Extraordinario estratega! Resuelves operaciones complejas respetando la jerarquía matemática."
        };

        var ex1 = new Exercise
        {
            Id = "pemdas_ex_01",
            Title = "La Regla de Oro: 4 + 3 × 2",
            GameType = GamePlayType.OrderOfOperationsPEMDAS,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 10,
            StandardInstruction = "Calcula el valor de la expresión: 4 + 3 × 2. Recuerda que la multiplicación se resuelve antes que la suma. ¿Cuál es el resultado correcto?",
            SimplifiedInstruction = "Primero haz 3 × 2 = 6. Luego suma 4 + 6. ¿Cuánto da?",
            ScreenReaderDescription = "Expresión: 4 más 3 por 2. Por jerarquía de operaciones, resuelve primero la multiplicación y después la suma.",
            FirstHint = "Resuelve primero 3 × 2. Luego suma 4.",
            SecondHintDetailed = "3 × 2 = 6, y 4 + 6 = 10.",
            ExplanationOfSuccess = "¡Magnífico! 3 × 2 = 6, y 4 + 6 = 10. Si sumaras primero 4 + 3 = 7 y luego multiplicaras daría 14, ¡lo cual es un error común que tú evitaste!"
        };

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "10 (correcto por jerarquía)", Value = 10, IsCorrect = true, SpeechFeedback = "¡Exacto! 3 × 2 = 6, y 4 + 6 = 10.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "14 (sumando primero)", Value = 14, IsCorrect = false, SpeechFeedback = "¡Cuidado! No sumes primero. La multiplicación tiene prioridad.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "9", Value = 9, IsCorrect = false, SpeechFeedback = "Revisa los cálculos: primero 3 × 2 y luego suma 4.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 17 (1º Secundaria): Ecuaciones Lineales ax + b = c
    /// </summary>
    private PedagogicalTopic BuildLinearEquationsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_linear_equations_01",
            Title = "El Misterio de la Papelería: Ecuaciones Lineales ax + b = c",
            Level = AcademicLevel.SecondaryFirstGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "En la papelería del barrio, compraste varios cuadernos del mismo precio X más un lápiz, y pagaste con un billete. ¿Cómo saber el precio de cada cuaderno sin adivinar? ¡Con una ecuación lineal de primer grado de la forma ax + b = c! Despejamos la incógnita con operaciones inversas ordenadas.",
            NarrativeIntroSimplified = "Para saber el costo de cada libreta, primero resta lo que pagaste del lápiz y luego divide entre las libretas.",
            CoreConceptExplanation = "En una ecuación ax + b = c, despejamos x en dos pasos: primero restamos b en ambos lados (ax = c - b), y luego dividimos ambos miembros entre el coeficiente a: x = (c - b) / a.",
            CoreConceptSimplified = "Paso 1: Resta el lápiz. Paso 2: Divide entre el número de libretas.",
            PrinciplesAddressed = new List<string>
            {
                "Modelado algebraico de problemas de compra y costo unitario",
                "Despeje en dos pasos: restar el término independiente y dividir entre el coeficiente",
                "Comprobación formal de la solución"
            },
            CongratulationMessage = "¡Mente analítica! Despejas ecuaciones de primer grado con soltura y precisión."
        };

        var ex1 = new Exercise
        {
            Id = "eq_axb_ex_01",
            Title = "La Cuenta de la Papelería: 2X + 5 = 25",
            GameType = GamePlayType.LinearEquationAXB,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 10,
            StandardInstruction = "Compraste 2 libretas iguales de precio X y un lápiz de $5 pesos. La cuenta total fue de $25 pesos (2X + 5 = 25). ¿Cuánto costó cada libreta X?",
            SimplifiedInstruction = "2 libretas más 5 pesos da 25 pesos. ¿Cuánto costó cada libreta?",
            ScreenReaderDescription = "Ecuación: 2 por X más 5 igual a 25. Resta 5 para obtener 2X = 20, luego divide entre 2.",
            FirstHint = "Paso 1: 2X = 25 - 5 = 20 pesos.",
            SecondHintDetailed = "Paso 2: X = 20 ÷ 2 = 10 pesos.",
            ExplanationOfSuccess = "¡Espectacular! 2X = 20, por lo que X = 10 pesos. Comprobamos: 2(10) + 5 = 20 + 5 = 25 pesos."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "lib_1", Name = "Libreta 1", EmojiSymbol = "📓", AccessibleDescription = "Primera libreta de precio X", OrderIndex = 1, MusicalPitchHz = Frequencies[1] });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "lib_2", Name = "Libreta 2", EmojiSymbol = "📓", AccessibleDescription = "Segunda libreta de precio X", OrderIndex = 2, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "X = $8 pesos", Value = 8, IsCorrect = false, SpeechFeedback = "2 por 8 es 16, más 5 daría 21 pesos, no 25.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "X = $10 pesos", Value = 10, IsCorrect = true, SpeechFeedback = "¡Magnífico! 2 × 10 = 20, más 5 = 25 pesos.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "X = $15 pesos", Value = 15, IsCorrect = false, SpeechFeedback = "2 por 15 daría 30 pesos, se pasaría del total.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 18 (1º Secundaria): Cálculo de Porcentajes y Descuentos
    /// </summary>
    private PedagogicalTopic BuildPercentagesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_percentages_01",
            Title = "Temporada de Ofertas: Cálculo de Porcentajes y Descuentos",
            Level = AcademicLevel.SecondaryFirstGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Llegaron las ofertas a la tienda departamental y al tianguis! Un descuento del 20% significa que por cada 100 pesos de precio, te rebajan 20 pesos. Saber calcular porcentajes te convierte en un comprador inteligente que nunca se deja engañar.",
            NarrativeIntroSimplified = "El porcentaje (%) te dice cuánto te descuentan por cada 100 pesos de compra.",
            CoreConceptExplanation = "El porcentaje es una razón que indica una fracción de 100 partes iguales. Para calcular el P% de una cantidad C: Descuento = (C × P) / 100. El precio final con descuento es C - Descuento.",
            CoreConceptSimplified = "20% de 200 pesos: por los primeros 100 pesos son 20, por los segundos son otros 20 = 40 pesos de descuento.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de tanto por ciento como razón de base 100",
                "Cálculo directo del monto de descuento y precio final",
                "Estrategias de cálculo mental para 10%, 25% y 50%"
            },
            CongratulationMessage = "¡Comprador experto! Calculas descuentos y porcentajes con maestría en cualquier tienda."
        };

        var ex1 = new Exercise
        {
            Id = "pct_ex_01",
            Title = "Oferta en la Mochila: 20% de $200",
            GameType = GamePlayType.PercentageCalculation,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 40,
            StandardInstruction = "En la tienda una mochila cuesta $200 pesos y tiene un descuento especial del 20%. ¿Cuántos pesos te descuentan del precio original?",
            SimplifiedInstruction = "La mochila vale 200 pesos y tiene 20% de descuento. ¿Cuánto dinero te descuentan?",
            ScreenReaderDescription = "Cálculo de 20% de 200 pesos. Por cada 100 pesos se descuentan 20.",
            FirstHint = "Por cada $100 pesos te descuentan $20. Como son $200 pesos, te descuentan el doble: 20 × 2.",
            SecondHintDetailed = "(200 × 20) ÷ 100 = 40 pesos de descuento.",
            ExplanationOfSuccess = "¡Exacto! El 20% de $200 pesos son $40 pesos de descuento. La mochila te costará solo $160 pesos."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "bag_1", Name = "Mochila de $200", EmojiSymbol = "🎒", AccessibleDescription = "Mochila de 200 pesos con etiqueta de 20%", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "$20 pesos de descuento", Value = 20, IsCorrect = false, SpeechFeedback = "$20 pesos sería el descuento si costara $100. Como cuesta $200 es el doble.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "$40 pesos de descuento", Value = 40, IsCorrect = true, SpeechFeedback = "¡Magnífico! 20 de cada 100 pesos: 20 × 2 = $40 pesos.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "$50 pesos de descuento", Value = 50, IsCorrect = false, SpeechFeedback = "$50 pesos sería el 25% de descuento.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 19 (2º Secundaria): Leyes de los Exponentes
    /// </summary>
    private PedagogicalTopic BuildExponentLawsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_exponent_laws_01",
            Title = "El Reactor de Energía: Leyes de los Exponentes",
            Level = AcademicLevel.SecondarySecondGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido a 2º de Secundaria! En la era digital y espacial manejamos cantidades astronómicas de bytes y átomos. Las leyes de los exponentes son las reglas secretas que permiten simplificar multiplicaciones gigantescas sumando pequeños exponentes: a^m · a^n = a^(m+n).",
            NarrativeIntroSimplified = "Al multiplicar potencias de la misma base, no necesitas multiplicar todo: solo suma los exponentes.",
            CoreConceptExplanation = "Ley del producto de potencias con la misma base: a^m · a^n = a^(m+n). La base se conserva y los exponentes se suman algebraicamente.",
            CoreConceptSimplified = "Si multiplicas 2² por 2³, la base sigue siendo 2 y sumas 2 + 3 = 5 (2⁵).",
            PrinciplesAddressed = new List<string>
            {
                "Ley del producto de potencias de la misma base: a^m · a^n = a^(m+n)",
                "Potencia de una potencia: (a^m)^n = a^(m·n)",
                "Propiedad del exponente cero: a^0 = 1"
            },
            CongratulationMessage = "¡Poder Exponencial! Dominas las leyes de los exponentes para acelerar cualquier cálculo matemático."
        };

        var ex1 = new Exercise
        {
            Id = "exp_law_ex_01",
            Title = "El Producto de Potencias: 2³ × 2²",
            GameType = GamePlayType.ExponentLaws,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 5,
            StandardInstruction = "Aplica la ley de exponentes para simplificar: 2³ × 2². Como las bases son iguales (2), la regla indica que los exponentes se suman (3 + 2). ¿Cuál es el exponente resultante?",
            SimplifiedInstruction = "En 2³ × 2², suma los exponentes: 3 + 2. ¿Cuál es el resultado?",
            ScreenReaderDescription = "Producto de potencias con base 2: exponente 3 por exponente 2. Suma de exponentes 3 más 2.",
            FirstHint = "Conserva la base 2 y suma los exponentes: 3 + 2.",
            SecondHintDetailed = "3 + 2 = 5. El resultado simplificado es 2⁵ (exponente 5).",
            ExplanationOfSuccess = "¡Exacto! 2³ × 2² = 2^(3+2) = 2⁵. El exponente resultante es 5."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "exp_item_1", Name = "Potencia 2³", EmojiSymbol = "💎", AccessibleDescription = "Base 2 con exponente 3", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex1.ItemsGroupB.Add(new ManipulableItem { Id = "exp_item_2", Name = "Potencia 2²", EmojiSymbol = "✨", AccessibleDescription = "Base 2 con exponente 2", OrderIndex = 2, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "Exponente 5 (2⁵)", Value = 5, IsCorrect = true, SpeechFeedback = "¡Magnífico! 3 + 2 = 5. El exponente es 5.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "Exponente 6 (2⁶)", Value = 6, IsCorrect = false, SpeechFeedback = "¡Cuidado! Los exponentes se suman, no se multiplican (3 + 2 = 5, no 3 × 2).", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "Exponente 1", Value = 1, IsCorrect = false, SpeechFeedback = "No se restan los exponentes en la multiplicación; se suman.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 20 (2º Secundaria): Reducción de Términos Semejantes en Polinomios
    /// </summary>
    private PedagogicalTopic BuildPolynomialLikeTermsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_polynomials_like_terms_01",
            Title = "El Almacén de Términos: Reducción de Polinomios",
            Level = AcademicLevel.SecondarySecondGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "En el almacén escolar no puedes sumar cajas de cuadernos con cajas de lápices: juntas cuadernos con cuadernos y lápices con lápices. En álgebra ocurre exactamente lo mismo: solo se pueden sumar o restar aquellos términos que tienen la misma parte literal (términos semejantes).",
            NarrativeIntroSimplified = "En álgebra solo sumamos las letras iguales con las letras iguales.",
            CoreConceptExplanation = "Dos o más términos son semejantes si tienen la misma variable elevada al mismo exponente. Al reducirlos, sumamos o restamos algebraicamente sus coeficientes numéricos conservando la variable intacta: ax + bx = (a + b)x.",
            CoreConceptSimplified = "3x + 4x son 7x. No puedes sumar x con y.",
            PrinciplesAddressed = new List<string>
            {
                "Identificación de términos semejantes por su parte literal y exponente",
                "Reducción y simplificación de polinomios multivariables",
                "Aplicación al inventario y modelado algebraico"
            },
            CongratulationMessage = "¡Maestro del Álgebra! Organizas y reduces polinomios con total claridad y elegancia."
        };

        var ex1 = new Exercise
        {
            Id = "poly_ex_01",
            Title = "Clasificando Útiles: 3x + 2y + 4x + 5y",
            GameType = GamePlayType.PolynomialLikeTerms,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 7,
            StandardInstruction = "En el inventario tenemos la expresión: 3x + 2y + 4x + 5y, donde x representa cuadernos e y lápices. Reduciendo los términos semejantes, ¿cuál es el coeficiente total de cuadernos x?",
            SimplifiedInstruction = "Junta las x: 3x más 4x. ¿Cuántas x tienes en total?",
            ScreenReaderDescription = "Expresión: 3x más 2y más 4x más 5y. Pregunta por el coeficiente del término semejante x.",
            FirstHint = "Agrupa solo los términos que tienen x: 3x + 4x.",
            SecondHintDetailed = "(3 + 4)x = 7x. El coeficiente de x es 7.",
            ExplanationOfSuccess = "¡Excelente! 3x + 4x = 7x (y 2y + 5y = 7y). El coeficiente final de x es 7."
        };

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "5x", Value = 5, IsCorrect = false, SpeechFeedback = "Revisa la suma de 3 + 4.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "7x (coeficiente 7)", Value = 7, IsCorrect = true, SpeechFeedback = "¡Correcto! 3x + 4x = 7x.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "14xy", Value = 14, IsCorrect = false, SpeechFeedback = "¡No! No se juntan las letras x con y. Se mantienen separadas.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 21 (2º Secundaria): Sistemas de Ecuaciones Lineales 2×2
    /// </summary>
    private PedagogicalTopic BuildLinearSystems2x2Topic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_linear_systems_2x2_01",
            Title = "La Cuenta de la Taquería: Sistemas de Ecuaciones 2×2",
            Level = AcademicLevel.SecondarySecondGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Llegamos a la taquería del barrio! Si dos amigos van a cenar y piden órdenes combinadas de tacos y refrescos con cuentas globales, ¿cómo averiguar el precio individual de un taco? ¡Con un sistema de dos ecuaciones lineales con dos incógnitas (2×2)! La resta o eliminación nos da la solución exacta.",
            NarrativeIntroSimplified = "Comparamos dos cuentas de comida para descubrir cuánto cuesta cada cosa por separado.",
            CoreConceptExplanation = "Un sistema 2×2 consta de dos ecuaciones con dos incógnitas compartidas. Mediante el método de reducción (eliminación), restamos una ecuación de la otra para cancelar una variable y resolver la restante.",
            CoreConceptSimplified = "Si 2 tacos y 1 refresco cuestan $45, y 1 taco y 1 refresco cuestan $30, la diferencia de $15 es el precio de 1 taco.",
            PrinciplesAddressed = new List<string>
            {
                "Modelado de situaciones simultáneas con dos incógnitas (x, y)",
                "Método algebraico de reducción / eliminación",
                "Verificación de la solución en ambas ecuaciones"
            },
            CongratulationMessage = "¡Detective Algebraico! Resuelves sistemas de ecuaciones simultáneas sin titubear."
        };

        var ex1 = new Exercise
        {
            Id = "sys2x2_ex_01",
            Title = "La Orden de Tacos y Refrescos",
            GameType = GamePlayType.LinearSystem2x2,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 15,
            StandardInstruction = "En la taquería: 2 tacos (x) y 1 refresco (y) cuestan $45 pesos (2x + y = 45). En la mesa de al lado, 1 taco (x) y 1 refresco (y) cuestan $30 pesos (x + y = 30). Restando ambas cuentas, ¿cuánto cuesta exactamente 1 taco (x)?",
            SimplifiedInstruction = "2 tacos y 1 refresco valen $45. 1 taco y 1 refresco valen $30. Resta 45 menos 30 para saber el costo de 1 taco.",
            ScreenReaderDescription = "Sistema 2x2: ecuación uno 2x + y = 45; ecuación dos x + y = 30. Resta para hallar el valor de x.",
            FirstHint = "Resta la segunda orden de la primera: (2x + y) - (x + y) = 45 - 30.",
            SecondHintDetailed = "x = 45 - 30 = 15 pesos por taco.",
            ExplanationOfSuccess = "¡Brillante deducción! 1 taco cuesta $15 pesos (y el refresco cuesta $15 pesos). Comprobamos: 2(15) + 15 = 30 + 15 = 45 pesos."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "taco_ex", Name = "Taco (x)", EmojiSymbol = "🌮", AccessibleDescription = "Taco de precio incógnita x", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex1.ItemsGroupB.Add(new ManipulableItem { Id = "soda_ex", Name = "Refresco (y)", EmojiSymbol = "🥤", AccessibleDescription = "Refresco de precio incógnita y", OrderIndex = 2, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "$10 pesos", Value = 10, IsCorrect = false, SpeechFeedback = "Si el taco costara 10, la cuenta no coincidiría.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "$15 pesos por taco", Value = 15, IsCorrect = true, SpeechFeedback = "¡Magnífico! 45 - 30 = 15 pesos por taco.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "$20 pesos", Value = 20, IsCorrect = false, SpeechFeedback = "20 pesos es demasiado alto; 45 menos 30 es 15.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 22 (2º Secundaria): El Teorema de Pitágoras
    /// </summary>
    private PedagogicalTopic BuildPythagoreanTheoremTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_pythagorean_theorem_01",
            Title = "Arquitectura Accesible: El Teorema de Pitágoras",
            Level = AcademicLevel.SecondarySecondGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Para construir una escuela 100% accesible, diseñamos rampas para sillas de ruedas. La rampa forma la hipotenusa de un triángulo rectángulo perfecto. Pitágoras descubrió que la suma de los cuadrados de los catetos es igual al cuadrado de la hipotenusa: a² + b² = c².",
            NarrativeIntroSimplified = "En un triángulo con esquina recta, el lado largo al cuadrado es igual a la suma de los dos lados cortos al cuadrado.",
            CoreConceptExplanation = "En todo triángulo rectángulo, el cuadrado de la longitud de la hipotenusa (c) es igual a la suma de los cuadrados de las longitudes de los catetos (a y b): c² = a² + b² <=> c = √(a² + b²).",
            CoreConceptSimplified = "Eleva al cuadrado los dos lados rectos, súmalos y saca la raíz cuadrada para hallar la rampa.",
            PrinciplesAddressed = new List<string>
            {
                "Enunciado geométrico y algebraico del Teorema de Pitágoras: a² + b² = c²",
                "Cálculo de la hipotenusa a partir de los catetos conocidos",
                "Aplicación práctica a la accesibilidad y diseño universal"
            },
            CongratulationMessage = "¡Gran Arquitecto Pitagórico! Calculas hipotenusas y construyes rampas perfectas con el teorema fundamental de la geometría."
        };

        var ex1 = new Exercise
        {
            Id = "pyth_ex_01",
            Title = "La Rampa Clásica: Catetos 3m y 4m",
            GameType = GamePlayType.PythagoreanTheorem,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 5,
            StandardInstruction = "Una rampa accesible tiene una altura vertical de 3 metros (cateto a) y una base horizontal de 4 metros (cateto b). Si c² = 3² + 4² = 9 + 16 = 25, ¿cuánto mide la longitud de la rampa inclinada c (√25)?",
            SimplifiedInstruction = "La rampa mide: 3² + 4² = 9 + 16 = 25. La raíz de 25 es la respuesta. ¿Cuánto mide?",
            ScreenReaderDescription = "Teorema de Pitágoras: cateto 3 y cateto 4. Cuadrados: 9 más 16 igual a 25. Raíz cuadrada de 25.",
            FirstHint = "Calcula la raíz cuadrada de 25: ¿qué número multiplicado por sí mismo da 25?",
            SecondHintDetailed = "5 × 5 = 25, por lo tanto c = √25 = 5 metros.",
            ExplanationOfSuccess = "¡Espectacular! c = √(3² + 4²) = √(9 + 16) = √25 = 5 metros. ¡Es la terna pitagórica más famosa de la historia!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "ramp_icon", Name = "Rampa accesible", EmojiSymbol = "📐", AccessibleDescription = "Rampa de hipotenusa 5 metros", OrderIndex = 1, MusicalPitchHz = Frequencies[3] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "5 metros de rampa (c = √25)", Value = 5, IsCorrect = true, SpeechFeedback = "¡Exacto! c = 5 metros. 3² + 4² = 5².", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "7 metros (sumando lados)", Value = 7, IsCorrect = false, SpeechFeedback = "¡Cuidado! No se suman directamente 3 + 4; se suman sus cuadrados y se saca raíz.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "12 metros", Value = 12, IsCorrect = false, SpeechFeedback = "12 sería 3 × 4, pero buscamos la hipotenusa que es 5.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 23 (3º Secundaria): Ecuaciones Cuadráticas
    /// </summary>
    private PedagogicalTopic BuildQuadraticEquationsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_quadratic_equations_01",
            Title = "El Misterio de la Incógnita al Cuadrado: Ecuaciones Cuadráticas",
            Level = AcademicLevel.SecondaryThirdGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido a 3º de Secundaria! Nos adentramos en el reino de las áreas y las curvas parabólicas. Cuando la incógnita está elevada al cuadrado (x²), la ecuación puede tener hasta dos soluciones reales. Resolver ecuaciones cuadráticas puras (ax² + c = 0) o completas (ax² + bx + c = 0) nos permite calcular dimensiones de terrenos, trayectorias espaciales y estructuras.",
            NarrativeIntroSimplified = "En una ecuación cuadrática, la letra x está multiplicada por sí misma (x²). Despejamos sacando la raíz cuadrada.",
            CoreConceptExplanation = "Una ecuación de segundo grado tiene la forma general ax² + bx + c = 0 (con a ≠ 0). En las formas puras x² - k = 0, despejamos directamente x² = k, por lo que x = ±√k. En problemas de áreas y medidas físicas, la longitud del lado es la raíz positiva.",
            CoreConceptSimplified = "Si x² = 25, entonces x es el número que multiplicado por sí mismo da 25, es decir, 5.",
            PrinciplesAddressed = new List<string>
            {
                "Estructura general de la ecuación de segundo grado: ax² + bx + c = 0",
                "Resolución por despeje directo y extracción de raíz cuadrada: x = ±√k",
                "Interpretación geométrica de las raíces en problemas de áreas"
            },
            CongratulationMessage = "¡Gran Maestro del Segundo Grado! Dominas las ecuaciones cuadráticas y descubres las raíces exactas de cualquier problema."
        };

        var ex1 = new Exercise
        {
            Id = "quad_ex_01",
            Title = "El Huerto Comunitario: x² - 36 = 0",
            GameType = GamePlayType.QuadraticEquations,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 6,
            StandardInstruction = "En el ejido escolar se delimita un huerto cuadrado de área total x² metros cuadrados. La ecuación que describe el terreno es x² - 36 = 0. Despejando x² = 36, ¿cuántos metros mide cada lado x del huerto (√36)?",
            SimplifiedInstruction = "Si x² = 36, ¿cuánto vale x? ¿Qué número por sí mismo da 36?",
            ScreenReaderDescription = "Ecuación cuadrática x cuadrada menos 36 igual a cero. Se despeja x al cuadrado igual a 36. Raíz cuadrada de 36.",
            FirstHint = "Despeja x²: x² = 36. Luego calcula la raíz cuadrada de 36.",
            SecondHintDetailed = "6 × 6 = 36, por lo tanto el lado positivo es x = 6 metros.",
            ExplanationOfSuccess = "¡Extraordinario! x² - 36 = 0 => x² = 36 => x = √36 = 6 metros. El huerto mide 6 metros por lado."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "garden_quad_item", Name = "Huerto 36m²", EmojiSymbol = "🟩", AccessibleDescription = "Huerto cuadrado de 36 metros cuadrados", OrderIndex = 1, MusicalPitchHz = Frequencies[1] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "6 metros (x = √36)", Value = 6, IsCorrect = true, SpeechFeedback = "¡Magnífico! 6 × 6 = 36. El lado mide 6 metros.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "18 metros (la mitad)", Value = 18, IsCorrect = false, SpeechFeedback = "No se divide entre 2; se busca la raíz cuadrada.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "12 metros", Value = 12, IsCorrect = false, SpeechFeedback = "12 × 12 sería 144, no 36. La respuesta correcta es 6.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 24 (3º Secundaria): Teorema de Tales y Semejanza de Triángulos
    /// </summary>
    private PedagogicalTopic BuildThalesSimilarityTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_thales_similarity_01",
            Title = "La Geometría del Sol: Teorema de Tales y Semejanza",
            Level = AcademicLevel.SecondaryThirdGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Cuentan las crónicas que Tales de Mileto midió la altura de las pirámides de Egipto comparando la sombra de su bastón con la de la pirámide a la misma hora del día. Dos figuras son semejantes cuando tienen la misma forma, ángulos idénticos y lados proporcionales.",
            NarrativeIntroSimplified = "El sol crea sombras con la misma inclinación. Usando sombras podemos medir cosas gigantescas sin escalarlas.",
            CoreConceptExplanation = "El Teorema de Tales establece que si en un triángulo se traza una línea paralela a cualquiera de sus lados, se obtiene un triángulo semejante. La razón de semejanza r entre lados homólogos es constante: Altura₁ / Sombra₁ = Altura₂ / Sombra₂.",
            CoreConceptSimplified = "Si un poste mide 2m y da 1m de sombra, un árbol que da 5m de sombra mide 10m de altura.",
            PrinciplesAddressed = new List<string>
            {
                "Criterios de semejanza de triángulos (Ángulo-Ángulo, Lado-Lado-Lado)",
                "Teorema de Tales sobre rectas paralelas y segmentos proporcionales",
                "Resolución de problemas de medición indirecta de alturas mediante sombras"
            },
            CongratulationMessage = "¡Fabuloso Geómetra! Mides monumentos y árboles gigantes con la elegancia matemática de Tales de Mileto."
        };

        var ex1 = new Exercise
        {
            Id = "thales_ex_01",
            Title = "La Sombra del Pirul del Parque",
            GameType = GamePlayType.TriangleSimilarityThales,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 8,
            StandardInstruction = "Al atardecer en el parque, un estudiante de 2 metros de estatura proyecta una sombra de 3 metros. A su lado, el gran árbol pirul proyecta una sombra de 12 metros. Como 12 ÷ 3 = 4 (el árbol es 4 veces más grande que la sombra de referencia), ¿cuántos metros de altura mide el árbol pirul (2 × 4)?",
            SimplifiedInstruction = "La sombra del árbol (12m) es 4 veces la sombra del estudiante (3m). Multiplica la altura del estudiante (2m) por 4.",
            ScreenReaderDescription = "Teorema de Tales: sombra de 12 metros entre sombra de 3 metros da factor 4. Altura del estudiante 2 metros por 4.",
            FirstHint = "Calcula la proporción: 12 / 3 = 4 veces más grande. Multiplica 2 × 4.",
            SecondHintDetailed = "Altura = 2 × 4 = 8 metros.",
            ExplanationOfSuccess = "¡Excelente! Por semejanza de triángulos: Altura / 12 = 2 / 3 => Altura = (2 × 12) / 3 = 24 / 3 = 8 metros."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "tree_thales_item", Name = "Árbol Pirul", EmojiSymbol = "🌳", AccessibleDescription = "Árbol pirul proyectando 12 metros de sombra", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "6 metros", Value = 6, IsCorrect = false, SpeechFeedback = "Revisa la proporción: 12 entre 3 es 4, y 2 por 4 es 8.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "8 metros de altura", Value = 8, IsCorrect = true, SpeechFeedback = "¡Correcto! 2 × 4 = 8 metros de altura.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "10 metros", Value = 10, IsCorrect = false, SpeechFeedback = "10 no coincide con la razón de semejanza de 4.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 25 (3º Secundaria): Razones Trigonométricas Básicas
    /// </summary>
    private PedagogicalTopic BuildTrigonometricRatiosTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_trigonometry_ratios_01",
            Title = "La Brújula de los Ángulos: Razones Trigonométricas",
            Level = AcademicLevel.SecondaryThirdGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "La trigonometría une los lados rectos de un triángulo con sus ángulos internos. En todo triángulo rectángulo, las razones entre sus lados reciben nombres universales: seno (sen), coseno (cos) y tangente (tan). Con ellas navegamos mares, medimos distancias estelares y orientamos satélites.",
            NarrativeIntroSimplified = "La trigonometría compara los lados de un triángulo para calcular ángulos y alturas inaccesibles.",
            CoreConceptExplanation = "Para un ángulo agudo θ en un triángulo rectángulo: sen(θ) = Cateto Opuesto / Hipotenusa, cos(θ) = Cateto Adyacente / Hipotenusa, y tan(θ) = Cateto Opuesto / Cateto Adyacente.",
            CoreConceptSimplified = "SOH-CAH-TOA: Seno = Opuesto/Hipotenusa, Coseno = Adyacente/Hipotenusa, Tangente = Opuesto/Adyacente.",
            PrinciplesAddressed = new List<string>
            {
                "Definición de las razones trigonométricas directas: seno, coseno y tangente",
                "Identificación del cateto opuesto, cateto adyacente e hipotenusa según el ángulo de referencia",
                "Aplicación al cálculo de distancias y orientaciones espaciales"
            },
            CongratulationMessage = "¡Navegante Trigonométrico! Dominas el seno, coseno y la tangente para orientar cualquier trayecto en el plano."
        };

        var ex1 = new Exercise
        {
            Id = "trig_ex_01",
            Title = "La Antena y el Cateto Opuesto",
            GameType = GamePlayType.TrigonometricRatios,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 3,
            StandardInstruction = "En un soporte triangular con hipotenusa de 5 metros y cateto adyacente de 4 metros, la razón seno del ángulo es sen(θ) = cateto opuesto / 5. Con la terna pitagórica op² + 4² = 5² (op² + 16 = 25 => op² = 9), ¿cuántos metros mide el cateto opuesto (√9)?",
            SimplifiedInstruction = "Si la hipotenusa mide 5 y la base mide 4, el lado opuesto cumple: op² = 25 - 16 = 9. ¿Cuánto es la raíz de 9?",
            ScreenReaderDescription = "Triángulo rectángulo: hipotenusa 5 metros, base 4 metros. Cateto opuesto al cuadrado es 9. Raíz de 9.",
            FirstHint = "Calcula la raíz cuadrada de 9: ¿qué número multiplicado por sí mismo da 9?",
            SecondHintDetailed = "3 × 3 = 9, por lo tanto el cateto opuesto mide 3 metros.",
            ExplanationOfSuccess = "¡Perfecto! El cateto opuesto mide 3 metros. Así: sen(θ) = 3/5 = 0.6, cos(θ) = 4/5 = 0.8, tan(θ) = 3/4 = 0.75."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "trig_antenna_item", Name = "Antena Soporte", EmojiSymbol = "📡", AccessibleDescription = "Soporte de antena con cateto opuesto de 3 metros", OrderIndex = 1, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "3 metros (sen θ = 3/5)", Value = 3, IsCorrect = true, SpeechFeedback = "¡Excelente! El cateto opuesto mide 3 metros.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "2 metros", Value = 2, IsCorrect = false, SpeechFeedback = "2 al cuadrado es 4, pero necesitamos 9.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "4 metros", Value = 4, IsCorrect = false, SpeechFeedback = "4 es el cateto adyacente, no el opuesto.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 26 (3º Secundaria): Funciones Cuadráticas y Parábolas
    /// </summary>
    private PedagogicalTopic BuildQuadraticFunctionsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_quadratic_functions_01",
            Title = "El Vuelo de la Parábola: Funciones Cuadráticas",
            Level = AcademicLevel.SecondaryThirdGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Cuando lanzas un balón de básquetbol o miras el arco de un puente colgante, la curva que se dibuja no es una línea recta: ¡es una parábola perfecta! Las funciones cuadráticas de la forma y = ax² modelan la aceleración por gravedad, la acústica de los teatros y las trayectorias de cohetes.",
            NarrativeIntroSimplified = "Una función cuadrática dibuja una curva suave llamada parábola. A medida que x crece, y crece mucho más rápido.",
            CoreConceptExplanation = "La función cuadrática básica es y = ax². A diferencia de la variación lineal donde la razón de cambio es constante, en la función cuadrática las segundas diferencias son constantes, produciendo una curva simétrica con vértice en el origen.",
            CoreConceptSimplified = "Si y = 2x² y x vale 3, calculas 3² = 9, y luego 2 × 9 = 18.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto y gráfica de la función cuadrática: y = ax²",
                "Identificación del vértice, eje de simetría y concavidad",
                "Modelado de trayectorias físicas y parábolas en el entorno"
            },
            CongratulationMessage = "¡Asombroso Modelador Matemático! Comprendes las funciones cuadráticas y la belleza geométrica de las parábolas."
        };

        var ex1 = new Exercise
        {
            Id = "quad_func_ex_01",
            Title = "La Fuente de Agua Danzante: y = 2x²",
            GameType = GamePlayType.QuadraticFunctionCurve,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 18,
            StandardInstruction = "En la fuente del zócalo, la parábola del agua sigue la función cuadrática y = 2x². Si avanzamos a una distancia horizontal de x = 3 metros, ¿cuál es la altura y del chorro de agua? (Calcula 3² = 9 y luego multiplica por 2).",
            SimplifiedInstruction = "Para x = 3, haz: 3 × 3 = 9. Luego multiplica 9 × 2. ¿Cuánto vale y?",
            ScreenReaderDescription = "Función cuadrática y igual a 2x al cuadrado. Para x igual a 3 metros, 3 al cuadrado es 9, por 2 da 18 metros.",
            FirstHint = "Primero eleva 3 al cuadrado: 3 × 3 = 9. Luego multiplica: 2 × 9.",
            SecondHintDetailed = "y = 2 × (3²) = 2 × 9 = 18 metros de altura.",
            ExplanationOfSuccess = "¡Brillante! Para x = 3, y = 2 × (3²) = 2 × 9 = 18 metros. ¡La parábola se eleva con gracia y precisión matemática!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "fountain_parabola_item", Name = "Chorro de Agua", EmojiSymbol = "⛲", AccessibleDescription = "Chorro parabólico que alcanza 18 metros", OrderIndex = 1, MusicalPitchHz = Frequencies[4] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "12 metros (2 × 6)", Value = 12, IsCorrect = false, SpeechFeedback = "Recuerda elevar 3 al cuadrado primero (3² = 9), no 3 × 2.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "18 metros (y = 2 × 9)", Value = 18, IsCorrect = true, SpeechFeedback = "¡Magnífico! 3² = 9, y 2 × 9 = 18 metros.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "36 metros", Value = 36, IsCorrect = false, SpeechFeedback = "36 es demasiado alto; 2 × 9 = 18.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 27 (1º Preparatoria): Distancia entre Puntos en el Plano Cartesiano
    /// </summary>
    private PedagogicalTopic BuildAnalyticDistanceTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_analytic_distance_01",
            Title = "La Red de Telecomunicaciones: Distancia entre Puntos",
            Level = AcademicLevel.HighSchoolFirstGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido a la Educación Media Superior! En 1º de Preparatoria las matemáticas se fusionan con la geometría analítica de René Descartes. Cada antena celular, satélite o pixel en una pantalla tiene coordenadas exactas (x, y). Conectar dos puntos con fibra óptica requiere calcular la distancia euclidiana d = √((x₂ - x₁)² + (y₂ - y₁)²), que es la generalización de Pitágoras en el plano.",
            NarrativeIntroSimplified = "Para saber la distancia entre dos puntos en un mapa, calculamos los pasos horizontales y verticales y aplicamos la raíz cuadrada.",
            CoreConceptExplanation = "La distancia entre dos puntos A(x₁, y₁) y B(x₂, y₂) en el plano cartesiano se deduce del Teorema de Pitágoras: d = √((Δx)² + (Δy)²), donde Δx = x₂ - x₁ y Δy = y₂ - y₁.",
            CoreConceptSimplified = "Si de A a B avanzas 3 en horizontal y 4 en vertical, la distancia directa es √(3² + 4²) = 5.",
            PrinciplesAddressed = new List<string>
            {
                "Definición y cálculo de la distancia euclidiana entre dos puntos en ℝ²",
                "Relación fundamental entre la geometría analítica y el Teorema de Pitágoras",
                "Modelado de redes de telecomunicación y rutas óptimas"
            },
            CongratulationMessage = "¡Gran Analista de Redes! Calculas distancias euclidianas con precisión matemática fundamental para el desarrollo de mapas y gráficos."
        };

        var ex1 = new Exercise
        {
            Id = "dist2d_ex_01",
            Title = "Conexión de Antenas A(1, 2) y B(4, 6)",
            GameType = GamePlayType.AnalyticDistance2D,
            TargetAgeMin = 14,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolFirstGrade,
            ExpectedResult = 5,
            StandardInstruction = "En la red metropolitana conectamos la antena A(1, 2) con la antena B(4, 6). Las diferencias de coordenadas son dx = 4 - 1 = 3 km y dy = 6 - 2 = 4 km. Aplicando d = √(3² + 4²) = √(9 + 16) = √25, ¿cuántos kilómetros de fibra óptica se necesitan?",
            SimplifiedInstruction = "La antena A y B tienen diferencia horizontal de 3km y vertical de 4km. Si d² = 3² + 4² = 25, ¿cuánto es la raíz de 25?",
            ScreenReaderDescription = "Distancia analítica: A(1, 2) a B(4, 6). dx es 3, dy es 4. Raíz cuadrada de 25.",
            FirstHint = "Calcula la suma de los cuadrados: 3² + 4² = 9 + 16 = 25.",
            SecondHintDetailed = "d = √25 = 5 kilómetros.",
            ExplanationOfSuccess = "¡Excelente! d = √((4 - 1)² + (6 - 2)²) = √(3² + 4²) = √25 = 5 km. ¡Es la distancia cartesiana exacta!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "ant_a_item", Name = "Antena A(1,2)", EmojiSymbol = "📍", AccessibleDescription = "Antena origen A", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });
        ex1.ItemsGroupB.Add(new ManipulableItem { Id = "ant_b_item", Name = "Antena B(4,6)", EmojiSymbol = "📡", AccessibleDescription = "Antena destino B", OrderIndex = 2, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "5 km de fibra (d = √25)", Value = 5, IsCorrect = true, SpeechFeedback = "¡Magnífico! d = 5 km.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "7 km (sumando 3 + 4)", Value = 7, IsCorrect = false, SpeechFeedback = "No se suman directamente los catetos; la distancia directa es la hipotenusa (5 km).", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "12 km", Value = 12, IsCorrect = false, SpeechFeedback = "12 no corresponde a la raíz de 25.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 28 (1º Preparatoria): Pendiente de la Recta m = Δy / Δx
    /// </summary>
    private PedagogicalTopic BuildLineSlopeTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_line_slope_01",
            Title = "La Rampa del Viaducto: La Pendiente m",
            Level = AcademicLevel.HighSchoolFirstGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "La inclinación de una carretera o la velocidad con que sube un elevador se describe con un único número mágico: la pendiente m. La pendiente nos dice cuánto cambia la altura (y) por cada paso horizontal (x) que damos. Es el concepto central que abre las puertas al cálculo diferencial.",
            NarrativeIntroSimplified = "La pendiente mide qué tan inclinada está una recta: es el cambio vertical dividido entre el cambio horizontal.",
            CoreConceptExplanation = "La pendiente de una recta no vertical que pasa por (x₁, y₁) y (x₂, y₂) es m = (y₂ - y₁) / (x₂ - x₁) = Δy / Δx. Una pendiente positiva asciende de izquierda a derecha; una negativa desciende; una pendiente cero es totalmente horizontal.",
            CoreConceptSimplified = "Si subes 6 metros al avanzar 2 metros en horizontal, la pendiente es 6 ÷ 2 = 3.",
            PrinciplesAddressed = new List<string>
            {
                "Definición geométrica y analítica de la pendiente de una recta: m = Δy / Δx",
                "Interpretación de la pendiente como razón de cambio constante",
                "Relación entre pendiente, ángulo de inclinación y accesibilidad vial"
            },
            CongratulationMessage = "¡Gran Ingeniero Vial! Dominas la pendiente y la razón de cambio que gobierna las pendientes del mundo físico."
        };

        var ex1 = new Exercise
        {
            Id = "slope_ex_01",
            Title = "El Viaducto Metropolitano: P(1, 3) y Q(3, 9)",
            GameType = GamePlayType.LineSlopeM,
            TargetAgeMin = 14,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolFirstGrade,
            ExpectedResult = 3,
            StandardInstruction = "El trazo de un viaducto pasa por los puntos P(1, 3) y Q(3, 9). Calculando el incremento vertical Δy = 9 - 3 = 6 y el incremento horizontal Δx = 3 - 1 = 2, ¿cuál es el valor de la pendiente m = 6 / 2?",
            SimplifiedInstruction = "Divide el cambio en vertical (6) entre el cambio en horizontal (2): 6 ÷ 2. ¿Cuánto vale la pendiente?",
            ScreenReaderDescription = "Pendiente de la recta: punto P(1, 3) y Q(3, 9). Δy es 6, Δx es 2. Pendiente 6 entre 2.",
            FirstHint = "Divide el cambio vertical entre el horizontal: m = (9 - 3) / (3 - 1) = 6 / 2.",
            SecondHintDetailed = "m = 6 ÷ 2 = 3.",
            ExplanationOfSuccess = "¡Excelente! m = (9 - 3) / (3 - 1) = 6 / 2 = 3. Por cada kilómetro horizontal, la vía asciende 3 kilómetros."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "bridge_slope_item", Name = "Viaducto m=3", EmojiSymbol = "🌉", AccessibleDescription = "Viaducto con pendiente de 3", OrderIndex = 1, MusicalPitchHz = Frequencies[1] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "m = 2", Value = 2, IsCorrect = false, SpeechFeedback = "6 entre 2 no es 2; realiza la división 6 ÷ 2.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "m = 3 de pendiente", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! m = 6 / 2 = 3.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "m = 4", Value = 4, IsCorrect = false, SpeechFeedback = "4 × 2 sería 8, pero buscamos 6.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 29 (1º Preparatoria): Ecuación de la Recta y = mx + b
    /// </summary>
    private PedagogicalTopic BuildLinearEquationYMXBTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_line_equation_ymxb_01",
            Title = "La Tarifa de Transporte: Ecuación y = mx + b",
            Level = AcademicLevel.HighSchoolFirstGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Casi todo en la economía cotidiana se modela con una recta: un costo fijo inicial (b) más un costo variable por cada unidad o kilómetro recorrido (m). La ecuación en su forma pendiente-ordenada al origen y = mx + b es la reina del modelado lineal y el primer paso hacia los algoritmos de predicción.",
            NarrativeIntroSimplified = "y = mx + b suma un valor inicial b más una cantidad m por cada paso x.",
            CoreConceptExplanation = "En la ecuación de la recta y = mx + b, m representa la pendiente (ritmo de cambio) y b representa la ordenada al origen (el valor de y cuando x = 0, punto de corte con el eje vertical).",
            CoreConceptSimplified = "Si cobras $10 de entrada y $5 por cada juego, en 3 juegos pagas: (5 × 3) + 10 = 25.",
            PrinciplesAddressed = new List<string>
            {
                "Forma pendiente-ordenada al origen de la ecuación lineal: y = mx + b",
                "Interpretación de la ordenada al origen b como condición o costo inicial",
                "Modelado de tarifas y funciones lineales en contextos socioeconómicos"
            },
            CongratulationMessage = "¡Fabuloso Modelador Lineal! Con y = mx + b comprendes cómo predecir costos y trazar trayectorias en cualquier sistema."
        };

        var ex1 = new Exercise
        {
            Id = "ymxb_ex_01",
            Title = "El Mototaxi del Barrio: y = 4x + 10",
            GameType = GamePlayType.LinearEquationYMXB,
            TargetAgeMin = 14,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolFirstGrade,
            ExpectedResult = 26,
            StandardInstruction = "En el servicio de mototaxi, la tarifa en pesos es y = 4x + 10, donde $4 pesos es el costo por kilómetro (m), $10 pesos es el banderazo inicial (b) y x son los kilómetros recorridos. Si una persona viaja x = 4 kilómetros, ¿cuánto paga en total y = 4(4) + 10?",
            SimplifiedInstruction = "Calcula 4 × 4 + 10. Primero multiplica 4 por 4 (= 16) y luego suma 10. ¿Cuánto pagas?",
            ScreenReaderDescription = "Ecuación de la recta y = 4x + 10. Para x = 4 kilómetros: multiplica la pendiente por x y suma el banderazo.",
            FirstHint = "Multiplica 4 por los 4 km: 4 × 4 = 16. Luego suma el banderazo: 16 + 10.",
            SecondHintDetailed = "y = 16 + 10 = 26 pesos.",
            ExplanationOfSuccess = "¡Brillante! y = 4(4) + 10 = 16 + 10 = $26 pesos. ¡La ecuación y = mx + b modela a la perfección los costos de transporte!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "taxi_ymxb_item", Name = "Mototaxi", EmojiSymbol = "🛺", AccessibleDescription = "Mototaxi con tarifa y = 4x + 10", OrderIndex = 1, MusicalPitchHz = Frequencies[3] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "$24 pesos", Value = 24, IsCorrect = false, SpeechFeedback = "Revisa la suma: 16 + 10 es 26.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "$26 pesos en total", Value = 26, IsCorrect = true, SpeechFeedback = "¡Magnífico! 4 × 4 + 10 = 26 pesos.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "$30 pesos", Value = 30, IsCorrect = false, SpeechFeedback = "30 pesos es demasiado alto; 16 más 10 es 26.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 30 (2º Preparatoria): Circunferencia Canónica x² + y² = r²
    /// </summary>
    private PedagogicalTopic BuildConicCircleTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_circle_conic_01",
            Title = "El Radar Perimetral: La Circunferencia x² + y² = r²",
            Level = AcademicLevel.HighSchoolSecondGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido a 2º de Preparatoria! En el mundo de los videojuegos y la robótica accesible, saber si un obstáculo o amigo está dentro de un radio de proximidad se calcula con la ecuación de la circunferencia: x² + y² = r². Todos los puntos cuya distancia al centro es exactamente r forman esta curva cónica perfecta.",
            NarrativeIntroSimplified = "Un círculo perfecto tiene la regla x² + y² = r². La raíz del número de la derecha es el radio del círculo.",
            CoreConceptExplanation = "La ecuación ordinaria de la circunferencia con centro en el origen C(0, 0) y radio r es x² + y² = r². Si un punto (x, y) cumple x² + y² < r², está en el interior; si es igual, está sobre el perímetro.",
            CoreConceptSimplified = "Si x² + y² = 49, el radio es la raíz de 49, que es 7.",
            PrinciplesAddressed = new List<string>
            {
                "Ecuación canónica de la circunferencia con centro en el origen: x² + y² = r²",
                "Cálculo del radio r a partir de la constante r²",
                "Aplicación al campo de visión y zonas de interacción en robótica"
            },
            CongratulationMessage = "¡Gran Diseñador de Radares! Dominas la circunferencia cartesiana y el radio de cobertura en el plano."
        };

        var ex1 = new Exercise
        {
            Id = "circle_ex_01",
            Title = "El Perímetro del Sensor de Proximidad",
            GameType = GamePlayType.ConicCircleRadius,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = 5,
            StandardInstruction = "El radar de un vehículo accesible barre un área circular descrita por la ecuación x² + y² = 25. Sabiendo que x² + y² = r², ¿cuántos metros mide el radio de alcance r (√25)?",
            SimplifiedInstruction = "Si x² + y² = 25, ¿cuánto vale el radio (la raíz de 25)?",
            ScreenReaderDescription = "Ecuación de la circunferencia x cuadrada más y cuadrada igual a 25. Radio al cuadrado es 25. Raíz cuadrada de 25.",
            FirstHint = "Compara la ecuación con x² + y² = r²: r² = 25. Calcula la raíz cuadrada de 25.",
            SecondHintDetailed = "r = √25 = 5 metros.",
            ExplanationOfSuccess = "¡Excelente! r² = 25 => r = √25 = 5 metros. El sensor tiene un alcance radial de 5 metros en todas las direcciones."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "radar_circle_item", Name = "Radar 5m", EmojiSymbol = "🎯", AccessibleDescription = "Radar circular de 5 metros de radio", OrderIndex = 1, MusicalPitchHz = Frequencies[0] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "5 metros de radio (r = √25)", Value = 5, IsCorrect = true, SpeechFeedback = "¡Correcto! r = 5 metros.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "12.5 metros (la mitad)", Value = 12, IsCorrect = false, SpeechFeedback = "No se divide entre 2; se extrae la raíz cuadrada.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "10 metros", Value = 10, IsCorrect = false, SpeechFeedback = "10 al cuadrado es 100, no 25.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 31 (2º Preparatoria): Ondas Senoidales y Armónicas y = A · sen(x)
    /// </summary>
    private PedagogicalTopic BuildSineWaveContinuousTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_sine_wave_01",
            Title = "El Oscilador Armónico: La Onda y = A · sen(x)",
            Level = AcademicLevel.HighSchoolSecondGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "La música, la voz humana y la corriente eléctrica viajan por el aire en forma de ondas continuas. La función trigonométrica seno y = A · sen(Bx) describe cómo sube y baja un tono musical. La amplitud A nos dice qué tan fuerte suena una nota o cuál es su voltaje pico.",
            NarrativeIntroSimplified = "Una onda musical sube y baja suavemente como una ola. La amplitud A es qué tan alto sube la ola.",
            CoreConceptExplanation = "En la función periódica y = A · sen(Bx), A representa la amplitud (la distancia desde el eje central hasta la cresta máxima) y B modula la frecuencia angular.",
            CoreConceptSimplified = "Si la función es y = 6 · sen(x), como el seno máximo vale 1, la cresta más alta es 6.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto y propiedades de la onda senoidal continua: y = A · sen(x)",
                "Identificación de la amplitud, periodo y cresta máxima",
                "Modelado de señales acústicas y procesamiento digital de sonido"
            },
            CongratulationMessage = "¡Maestro de las Ondas! Comprendes cómo la trigonometría genera audio y armonía musical continua."
        };

        var ex1 = new Exercise
        {
            Id = "sine_wave_ex_01",
            Title = "El Sintetizador de Audio: y = 5 · sen(x)",
            GameType = GamePlayType.SineWaveContinuous,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = 5,
            StandardInstruction = "En el ecualizador de audio, una nota de flauta genera la onda periódica y = 5 · sen(x). Dado que el valor máximo que alcanza la función sen(x) es 1, ¿cuál es la amplitud o cresta máxima alcanzada por la señal de audio (5 × 1)?",
            SimplifiedInstruction = "Para y = 5 × sen(x), cuando el seno vale 1, ¿cuál es la altura máxima alcanzada?",
            ScreenReaderDescription = "Función senoidal continua y = 5 por seno de x. Cresta máxima cuando seno de x es 1. Amplitud pico de 5 unidades.",
            FirstHint = "El valor máximo de sen(x) es 1. Multiplica la amplitud 5 por 1.",
            SecondHintDetailed = "Cresta máxima = 5 × 1 = 5 unidades.",
            ExplanationOfSuccess = "¡Magnífico! La amplitud de la onda es A = 5. ¡Las funciones senoidales son el pilar matemático del procesamiento de audio!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "sine_wave_item", Name = "Onda Sonora", EmojiSymbol = "〰️", AccessibleDescription = "Onda senoidal con amplitud pico de 5", OrderIndex = 1, MusicalPitchHz = Frequencies[2] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "3 unidades", Value = 3, IsCorrect = false, SpeechFeedback = "La amplitud indicada en la fórmula es 5.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "5 unidades de amplitud (cresta máxima)", Value = 5, IsCorrect = true, SpeechFeedback = "¡Exacto! Amplitud A = 5.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "10 unidades", Value = 10, IsCorrect = false, SpeechFeedback = "10 sería el rango pico a pico (de -5 a +5), pero la amplitud respecto al centro es 5.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 32 (2º Preparatoria): Logaritmos en Base 2 y Complejidad Binaria
    /// </summary>
    private PedagogicalTopic BuildLogarithmBinaryTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_logarithm_binary_01",
            Title = "El Código Binario: Logaritmos log₂(n)",
            Level = AcademicLevel.HighSchoolSecondGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "El logaritmo es la operación inversa de la exponenciación: responde a la pregunta '¿a qué exponente debo elevar la base para obtener este número?'. En la computación, la base por excelencia es el 2. El logaritmo binario log₂(n) determina cuántas veces podemos partir a la mitad un conjunto de datos, siendo el secreto de los algoritmos ultra veloces de búsqueda.",
            NarrativeIntroSimplified = "El logaritmo log₂(n) cuenta cuántas veces multiplicas el 2 por sí mismo para llegar a un número.",
            CoreConceptExplanation = "Por definición: log_b(a) = c <=> b^c = a. En base 2: log₂(8) = 3 porque 2³ = 8. En ciencias computacionales, una búsqueda binaria en n elementos toma en el peor caso O(log₂ n) pasos.",
            CoreConceptSimplified = "Si tienes 16 cosas y divides a la mitad: 16 -> 8 -> 4 -> 2 -> 1 (4 pasos). Por eso log₂(16) = 4.",
            PrinciplesAddressed = new List<string>
            {
                "Definición formal del logaritmo como inversa de la función exponencial: log_b(x) = y <=> b^y = x",
                "Propiedades y cálculo del logaritmo en base 2: log₂(2^k) = k",
                "Aplicación fundamental al análisis de algoritmos y árboles binarios"
            },
            CongratulationMessage = "¡Cerebro Computacional! Dominas los logaritmos binarios y comprendes por qué las computadoras resuelven problemas inmensos en microsegundos."
        };

        var ex1 = new Exercise
        {
            Id = "log2_ex_01",
            Title = "Búsqueda en un Catálogo de 64 Elementos",
            GameType = GamePlayType.LogarithmBinary,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = 6,
            StandardInstruction = "En una biblioteca digital accesible de 64 libros ordenados alfabéticamente, un algoritmo de búsqueda binaria descarta la mitad de opciones en cada intento. Sabiendo que 2⁶ = 64, ¿cuántos pasos comparativos como máximo se necesitan (log₂(64))?",
            SimplifiedInstruction = "¿A qué potencia elevas el 2 para que dé 64? (2 × 2 × 2 × 2 × 2 × 2). ¿Cuántas veces multiplicas el 2?",
            ScreenReaderDescription = "Logaritmo en base 2 de 64. Cuenta cuántas veces multiplicas el 2 por sí mismo para llegar a 64.",
            FirstHint = "Multiplica el 2 sucesivamente: 2¹=2, 2²=4, 2³=8, 2⁴=16, 2⁵=32, 2⁶=64.",
            SecondHintDetailed = "Como 2⁶ = 64, entonces log₂(64) = 6 pasos.",
            ExplanationOfSuccess = "¡Brillante! log₂(64) = 6. ¡Con solo 6 preguntas puedes encontrar cualquier libro entre 64! El logaritmo convierte listas enormes en búsquedas instantáneas."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "tree_log2_item", Name = "Árbol de 64 Nodos", EmojiSymbol = "🌲", AccessibleDescription = "Estructura binaria con 64 elementos", OrderIndex = 1, MusicalPitchHz = Frequencies[4] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "5 pasos (2⁵ = 32)", Value = 5, IsCorrect = false, SpeechFeedback = "2 a la 5 es 32; para 64 se requiere un paso más.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "6 pasos (log₂ 64 = 6)", Value = 6, IsCorrect = true, SpeechFeedback = "¡Magnífico! 2⁶ = 64. log₂(64) = 6.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "8 pasos", Value = 8, IsCorrect = false, SpeechFeedback = "8 × 8 es 64, pero en base 2 buscamos la potencia 2^k, que es 6.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 33 (3º Preparatoria): Cálculo Diferencial — La Derivada y Razón de Cambio Instantánea
    /// </summary>
    private PedagogicalTopic BuildCalculusDerivativeTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_calculus_derivative_01",
            Title = "El Velocímetro del Futuro: Cálculo Diferencial y Derivadas",
            Level = AcademicLevel.HighSchoolThirdGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido a 3º de Preparatoria! Hasta ahora medíamos la pendiente entre dos puntos separados. Pero ¿qué pasa cuando acercamos esos dos puntos hasta que la distancia entre ellos tiende a cero? ¡Nace la derivada del Cálculo Diferencial! La derivada nos revela la velocidad exacta en un instante preciso y es el motor del descenso de gradiente con el que aprenden las inteligencias artificiales.",
            NarrativeIntroSimplified = "La derivada nos dice qué tan rápido cambia algo en un instante exacto, como el velocímetro de un auto.",
            CoreConceptExplanation = "La derivada f'(x) representa la pendiente de la recta tangente y la razón de cambio instantánea de f(x). Por la regla de la potencia, si f(t) = a·t^n, su derivada es f'(t) = n·a·t^(n-1). Para f(t) = a·t², la velocidad instantánea es v(t) = f'(t) = 2a·t.",
            CoreConceptSimplified = "Para derivar t², bajas el 2 a multiplicar: la derivada de 3t² es 6t. Luego multiplicas 6 por el segundo t.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de límite y razón de cambio instantánea (pendiente de la recta tangente)",
                "Regla de derivación de potencias: d/dt (a·t²) = 2a·t",
                "Interpretación física (velocidad instantánea) y computacional (gradiente)"
            },
            CongratulationMessage = "¡Maestro del Cálculo Diferencial! Dominas las derivadas y la razón de cambio instantáneo."
        };

        var ex1 = new Exercise
        {
            Id = "deriv_ex_01",
            Title = "La Velocidad Instantánea del Tren Eléctrico: f(t) = 3t²",
            GameType = GamePlayType.CalculusDerivativeRate,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 12,
            StandardInstruction = "Un tren eléctrico autónomo arranca desde la estación siguiendo la función de posición f(t) = 3t² metros. Su velocidad instantánea está dada por la derivada f'(t) = 6t. ¿Cuál es su velocidad exacta en m/s al pasar por el segundo t = 2?",
            SimplifiedInstruction = "La velocidad es f'(t) = 6 × t. Cuando t = 2 segundos, ¿cuánto vale 6 × 2?",
            ScreenReaderDescription = "Cálculo diferencial: función de posición f de t igual a 3t al cuadrado. Derivada f prima de t igual a 6t. Evaluar en t = 2 segundos.",
            FirstHint = "Al derivar 3t², bajamos el exponente 2 multiplicando al 3: 2 × 3t = 6t. Ahora sustituye t = 2.",
            SecondHintDetailed = "f'(2) = 6 × 2 = 12 m/s.",
            ExplanationOfSuccess = "¡Brillante! En el instante t = 2 s, el tren viaja exactamente a 12 m/s. La derivada transforma una curva de posición en una fórmula de velocidad instantánea."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "train_deriv_item", Name = "Velocímetro f'(2) = 12 m/s", EmojiSymbol = "🚄", AccessibleDescription = "Tren eléctrico con derivada f'(t) = 6t", OrderIndex = 1, MusicalPitchHz = Frequencies[5] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "6 m/s", Value = 6, IsCorrect = false, SpeechFeedback = "6 m/s es en el segundo t = 1. En t = 2 es el doble.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "12 m/s (f'(2) = 6 × 2)", Value = 12, IsCorrect = true, SpeechFeedback = "¡Exacto! f'(2) = 6 × 2 = 12 m/s.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "18 m/s", Value = 18, IsCorrect = false, SpeechFeedback = "18 m/s sería en el segundo t = 3.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 34 (3º Preparatoria): Cálculo Integral — Área Bajo la Curva y Acumulación
    /// </summary>
    private PedagogicalTopic BuildCalculusIntegralTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_calculus_integral_01",
            Title = "El Acumulador Solar: Cálculo Integral y Área Bajo la Curva",
            Level = AcademicLevel.HighSchoolThirdGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Si la derivada nos dice la velocidad en cada instante, ¿cómo descubrimos la cantidad total acumulada a lo largo del tiempo? ¡Con la operación inversa: la Integral! En Cálculo Integral, sumar infinitos instantes equivale a calcular el área exacta bajo la curva de una gráfica.",
            NarrativeIntroSimplified = "Integrar significa juntar todos los pedacitos para saber el total acumulado, midiendo el área debajo de una gráfica.",
            CoreConceptExplanation = "Por el Teorema Fundamental del Cálculo, la integral definida ∫₀ᵇ f(t) dt representa el área neta bajo la curva f(t) en el intervalo [0, b] y mide la acumulación total de una razón de cambio.",
            CoreConceptSimplified = "Si entran 5 litros cada minuto durante 4 minutos, el área del rectángulo bajo la línea es 5 × 4 = 20 litros.",
            PrinciplesAddressed = new List<string>
            {
                "Interpretación geométrica de la integral definida como área bajo la curva",
                "Teorema Fundamental del Cálculo: relación inversa entre derivada e integral",
                "Simulación numérica de acumulación en física y motores de videojuegos"
            },
            CongratulationMessage = "¡Arquitecto de la Integración! Comprendes cómo el área bajo la curva acumula energía, distancia y flujo."
        };

        var ex1 = new Exercise
        {
            Id = "integral_ex_01",
            Title = "Cosecha de Energía Solar: ∫₀⁴ 5 dt",
            GameType = GamePlayType.CalculusIntegralArea,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 20,
            StandardInstruction = "Un panel solar comunitario genera una potencia constante de f(t) = 5 kilovatios durante un lapso de t = 0 a t = 4 horas. La energía total acumulada es el área bajo la curva dada por la integral definida ∫₀⁴ 5 dt. ¿Cuántos kilovatios-hora (kWh) se acumularon en total?",
            SimplifiedInstruction = "Calcula el área bajo la línea: 5 kilovatios constantes durante 4 horas (5 × 4). ¿Cuánta energía se acumuló?",
            ScreenReaderDescription = "Cálculo integral: integral definida de 0 a 4 de 5 diferencial de t. Área rectangular de altura 5 y base 4.",
            FirstHint = "La región bajo f(t) = 5 entre t = 0 y t = 4 es un rectángulo de altura 5 y base 4.",
            SecondHintDetailed = "∫₀⁴ 5 dt = 5 × 4 = 20 kWh.",
            ExplanationOfSuccess = "¡Extraordinario! La integral definida es 20 kWh. ¡Acabas de aplicar el Teorema Fundamental del Cálculo para medir la energía acumulada!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "solar_integral_item", Name = "Área Bajo la Curva (20 kWh)", EmojiSymbol = "🔋", AccessibleDescription = "Panel solar con área acumulada de 20 kWh", OrderIndex = 1, MusicalPitchHz = Frequencies[6] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "9 kWh (sumando 5 + 4)", Value = 9, IsCorrect = false, SpeechFeedback = "El área de un rectángulo se calcula multiplicando base por altura, no sumando.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "20 kWh (∫₀⁴ 5 dt = 20)", Value = 20, IsCorrect = true, SpeechFeedback = "¡Magnífico! 5 × 4 = 20 kWh acumulados.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "25 kWh", Value = 25, IsCorrect = false, SpeechFeedback = "25 sería durante 5 horas; aquí fueron 4 horas.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 35 (3º Preparatoria): Probabilidad y Estadística para Ciencia de Datos
    /// </summary>
    private PedagogicalTopic BuildProbabilityAndStatsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_probability_stats_01",
            Title = "El Oráculo de los Datos: Probabilidad y Estadística",
            Level = AcademicLevel.HighSchoolThirdGrade,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Hemos llegado a la cima de la Educación Media Superior! En el mundo real y en la ciencia de datos, analizamos muestras de información para encontrar patrones y tomar decisiones inteligentes. Las medidas de tendencia central como la media aritmética (promedio) resumen conjuntos complejos en un valor representativo.",
            NarrativeIntroSimplified = "El promedio o media suma todos los datos y los reparte en partes iguales para resumir la información.",
            CoreConceptExplanation = "La media aritmética (x̄) de un conjunto de n datos se obtiene sumando todos los valores y dividiendo entre el número total de observaciones: x̄ = (x₁ + x₂ + ... + xₙ) / n.",
            CoreConceptSimplified = "Si en tres ensayos sacaste 7, 8 y 9, sumas 7 + 8 + 9 = 24 y divides 24 ÷ 3 = 8 de promedio.",
            PrinciplesAddressed = new List<string>
            {
                "Medidas de tendencia central: cálculo e interpretación de la media aritmética (x̄)",
                "Análisis de muestras de datos para toma de decisiones basada en evidencia",
                "Fundamentos estadísticos para ciencia de datos e inteligencia artificial"
            },
            CongratulationMessage = "¡Científico de Datos Graduado! Has completado toda la Preparatoria y estás 100% listo para conquistar la Programación Inclusiva."
        };

        var ex1 = new Exercise
        {
            Id = "stats_ex_01",
            Title = "Precisión del Lector de Pantalla: Media Aritmética x̄",
            GameType = GamePlayType.ProbabilityAndStats,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 8,
            StandardInstruction = "En un laboratorio de tecnología asistiva se evaluó la velocidad de respuesta de un lector de pantalla en 3 pruebas, obteniendo calificaciones de 7, 8 y 9 puntos. ¿Cuál es la media aritmética o promedio x̄ = (7 + 8 + 9) / 3?",
            SimplifiedInstruction = "Suma 7 + 8 + 9 (= 24) y divide entre 3 para conocer el promedio. ¿Cuánto es 24 ÷ 3?",
            ScreenReaderDescription = "Estadística descriptiva: datos 7, 8 y 9. Suma todos los datos y divide entre la cantidad de observaciones.",
            FirstHint = "Suma los tres valores: 7 + 8 + 9 = 24. Ahora divide 24 entre 3.",
            SecondHintDetailed = "x̄ = 24 ÷ 3 = 8 puntos.",
            ExplanationOfSuccess = "¡Excelente! La media aritmética es x̄ = 8 puntos. ¡Con este dominio de la estadística y el cálculo estás listo para programar el futuro!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "stats_chart_item", Name = "Muestra (7, 8, 9) -> x̄ = 8", EmojiSymbol = "📊", AccessibleDescription = "Gráfica estadística con media igual a 8", OrderIndex = 1, MusicalPitchHz = Frequencies[7] });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "7 puntos", Value = 7, IsCorrect = false, SpeechFeedback = "7 es el valor mínimo de la muestra, pero el promedio está en el centro.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "8 puntos de media (x̄ = 8)", Value = 8, IsCorrect = true, SpeechFeedback = "¡Magnífico! 24 ÷ 3 = 8 puntos.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "24 puntos", Value = 24, IsCorrect = false, SpeechFeedback = "24 es la suma total; falta dividirla entre los 3 datos.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 37 (Objetivo 14 - Programación Inclusiva 1): Variables, Asignación y Estado en Memoria
    /// </summary>
    private PedagogicalTopic BuildProgrammingVariablesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_variables_state_01",
            Title = "Cajas de Memoria: Variables, Asignación y Estado (x = x + a)",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido al Módulo 1 de Programación Inclusiva! Ya dominas las incógnitas del álgebra; ahora descubrirás cómo las computadoras guardan información. En programación, una 'variable' es como una caja etiquetada en la memoria donde guardamos un dato (como el saldo de la tiendita o las vidas de un juego) y lo actualizamos línea por línea.",
            NarrativeIntroSimplified = "Una variable es como una cajita con nombre donde la computadora guarda un número y lo puede cambiar sumando o restando.",
            CoreConceptExplanation = "En programación, el signo '=' significa 'asignar o guardar'. La instrucción 'saldo = 10' guarda el número 10 en la caja 'saldo'. Si en la siguiente línea escribimos 'saldo = saldo + 5', la computadora toma el 10 que había adentro, le suma 5 y guarda el nuevo valor 15 en la misma caja.",
            CoreConceptSimplified = "Si la caja 'saldo' tiene 10 y el código dice 'saldo = saldo + 5', ahora la caja guarda 15.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de variable como espacio con nombre en la memoria del programa",
                "Diferencia entre igualdad algebraica y asignación computacional (x = x + a)",
                "Seguimiento secuencial (traza) del estado de una variable línea por línea"
            },
            CongratulationMessage = "¡Programador de Memoria! Dominas las variables y el seguimiento de estado paso a paso."
        };

        var ex1 = new Exercise
        {
            Id = "prog_var_ex_01",
            Title = "Rastreando la Variable 'saldo' en la Tiendita",
            GameType = GamePlayType.ProgrammingVariablesState,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 12,
            StandardInstruction = "En el sistema accesible de la tiendita ejecutamos estas 3 instrucciones en orden:\n1) saldo = 10\n2) saldo = saldo + 5\n3) saldo = saldo - 3\n¿Qué valor queda almacenado en la variable 'saldo' al finalizar la línea 3?",
            SimplifiedInstruction = "La caja 'saldo' empieza en 10. En el paso 2 le sumamos 5 (10 + 5 = 15) y en el paso 3 le restamos 3 (15 - 3). ¿Cuánto vale 'saldo' al final?",
            ScreenReaderDescription = "Traza de variable: línea 1, saldo igual a 10. Línea 2, saldo igual a saldo más 5. Línea 3, saldo igual a saldo menos 3.",
            FirstHint = "Calcula primero la línea 2: como 'saldo' valía 10, al hacer saldo + 5 ahora vale 15.",
            SecondHintDetailed = "En la línea 3 restamos 3 al valor 15 que tenía la variable: 15 - 3 = 12 pesos.",
            ExplanationOfSuccess = "¡Excelente traza de código! La variable 'saldo' pasó de 10 a 15 y finalmente a 12 pesos. Así es como todos los programas actualizan su estado en memoria."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "var_trace_item",
            Name = "Variable saldo (10 → 15 → 12)",
            EmojiSymbol = "📦",
            AccessibleDescription = "Caja de memoria saldo: inicia en 10, sube a 15 y termina en 12",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[2]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "10 pesos (el valor inicial)", Value = 10, IsCorrect = false, SpeechFeedback = "10 era el valor en la línea 1, pero las líneas 2 y 3 modificaron la variable.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "12 pesos (10 + 5 - 3)", Value = 12, IsCorrect = true, SpeechFeedback = "¡Exacto! 10 + 5 = 15, y 15 - 3 = 12 pesos.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "15 pesos (sin restar la línea 3)", Value = 15, IsCorrect = false, SpeechFeedback = "15 es el valor al terminar la línea 2; falta aplicar la resta de la línea 3 (- 3).", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 38 (Objetivo 14 - Programación Inclusiva 1): Condicionales SI / SI NO (if / else)
    /// </summary>
    private PedagogicalTopic BuildProgrammingConditionalsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_conditionals_ifelse_01",
            Title = "El Semáforo Inteligente: Condicionales SI / SI NO (if / else)",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¿Cómo sabe un lector de pantalla o un bastón inteligente cuándo activar una alerta sonora? ¡Gracias a las estructuras condicionales! Usando las desigualdades matemáticas (mayor que >, menor que <, mayor o igual >=), el código evalúa una pregunta lógica y elige el camino verdadero (SI / if) o el camino alternativo (SI NO / else).",
            NarrativeIntroSimplified = "Con la instrucción SI (if), el programa hace una pregunta: si se cumple sigue un camino, y SI NO (else) sigue el otro.",
            CoreConceptExplanation = "Una estructura condicional evalúa una condición booleana (Verdadera o Falsa): 'SI (condición) ENTONCES { acción A } SI NO { acción B }'. Nunca se ejecutan ambos bloques a la vez: si la condición se cumple se ejecuta únicamente la rama SI; en caso contrario se ejecuta la rama SI NO.",
            CoreConceptSimplified = "En 'SI puntos >= 10 suma 5, SI NO suma 2': si tienes 12 puntos, como 12 es mayor que 10, sumas 5 (12 + 5 = 17).",
            PrinciplesAddressed = new List<string>
            {
                "Evaluación de operadores relacionales (>, <, >=, <=, ==) en código",
                "Bifurcación del flujo de ejecución mediante bloques SI (if) y SI NO (else)",
                "Diseño de reglas de decisión en aplicaciones y tecnología asistiva"
            },
            CongratulationMessage = "¡Arquitecto de Decisiones! Sabes programar condicionales if/else para que el software tome decisiones inteligentes."
        };

        var ex1 = new Exercise
        {
            Id = "prog_if_ex_01",
            Title = "Bono en el Lector Inteligente: SI puntos >= 10",
            GameType = GamePlayType.ProgrammingConditionalsIfElse,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 17,
            StandardInstruction = "Observa el siguiente bloque condicional de nuestro tutor accesible:\n• puntos = 12\n• SI (puntos >= 10) ENTONCES { bono = 5 } SI NO { bono = 2 }\n• total = puntos + bono\n¿Qué valor numérico tendrá la variable 'total' al ejecutarse el programa?",
            SimplifiedInstruction = "Tienes puntos = 12. La regla dice: SI puntos es 10 o más, bono = 5; SI NO, bono = 2. ¿Cuánto vale puntos + bono?",
            ScreenReaderDescription = "Código condicional: puntos igual a 12. Si puntos mayor o igual a 10 entonces bono igual a 5; si no, bono igual a 2. Total igual a puntos más bono.",
            FirstHint = "Compara: ¿12 es mayor o igual a 10? ¡Sí! Por lo tanto se cumple la condición SI y bono = 5.",
            SecondHintDetailed = "Ahora suma puntos + bono: 12 + 5 = 17 puntos.",
            ExplanationOfSuccess = "¡Brillante! Como 12 >= 10 es Verdadero, el programa ejecutó la rama SI (bono = 5) e ignoró la rama SI NO, dando total = 12 + 5 = 17."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "cond_if_item",
            Name = "SI (12 >= 10) → bono = 5",
            EmojiSymbol = "🚦",
            AccessibleDescription = "Condicional verdadero: 12 es mayor o igual a 10, asigna bono 5 para un total de 17",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[4]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "14 puntos (usando la rama SI NO: 12 + 2)", Value = 14, IsCorrect = false, SpeechFeedback = "14 resultaría de sumar bono = 2, pero como 12 >= 10 sí se cumple, debemos usar bono = 5.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "17 puntos (rama SI: 12 + 5)", Value = 17, IsCorrect = true, SpeechFeedback = "¡Exacto! Como 12 >= 10 es verdadero, bono vale 5 y 12 + 5 = 17.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "19 puntos (sumando ambos bonos)", Value = 19, IsCorrect = false, SpeechFeedback = "En un condicional SI / SI NO nunca se ejecutan ambas ramas a la vez; solo se suma el bono de la rama elegida.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 39 (Objetivo 14 - Programación Inclusiva 1): Lógica Booleana y Compuertas Lógicas (AND, OR, NOT)
    /// </summary>
    private PedagogicalTopic BuildProgrammingBooleanLogicTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_boolean_logic_01",
            Title = "Las Compuertas del Robot: Lógica Booleana (Y / AND, O / OR, NO / NOT)",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Llegamos al corazón lógico de todas las computadoras! El matemático George Boole descubrió que toda decisión compleja puede construirse combinando valores Verdaderos (1) y Falsos (0) mediante tres operadores lógicos: Y (AND), O (OR) y NO (NOT). ¡Con ellos programamos sensores de seguridad y sistemas de asistencia!",
            NarrativeIntroSimplified = "El operador lógico Y (AND) exige que las dos condiciones sean verdaderas (1 y 1) al mismo tiempo.",
            CoreConceptExplanation = "En Lógica Booleana (donde 1 = Verdadero y 0 = Falso): 1) El operador Y (AND) solo da Verdadero si AMBAS condiciones son verdaderas (1 Y 1 = 1; si alguna es 0, da 0). 2) El operador O (OR) da Verdadero si AL MENOS UNA es verdadera. 3) El operador NO (NOT) invierte el valor (NO 1 = 0, NO 0 = 1).",
            CoreConceptSimplified = "En una compuerta Y (AND), 1 Y 1 es Verdadero, pero 1 Y 0 es Falso porque falta uno de los dos.",
            PrinciplesAddressed = new List<string>
            {
                "Álgebra de Boole: valores de verdad binarios (1 = Verdadero, 0 = Falso)",
                "Tablas de verdad de los operadores conjunción Y (AND), disyunción O (OR) y negación NO (NOT)",
                "Evaluación de condiciones compuestas en robótica y accesibilidad"
            },
            CongratulationMessage = "¡Maestro de la Lógica Booleana! Has completado el Objetivo 14 de Programación Inclusiva."
        };

        var ex1 = new Exercise
        {
            Id = "prog_bool_ex_01",
            Title = "Rampa Robótica Accesible: Compuerta Y (AND)",
            GameType = GamePlayType.ProgrammingBooleanLogic,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 18,
            StandardInstruction = "Una rampa robótica accesible tiene energía base = 10 y evalúa dos sensores de seguridad con el operador lógico Y (AND), donde 1 = Activo y 0 = Inactivo:\n• sensor_boton = 1, sensor_rampa = 1\n• SI (sensor_boton == 1 Y sensor_rampa == 1) ENTONCES { energia = 10 + 8 } SI NO { energia = 10 + 2 }\n¿Cuál es el valor final de 'energia'?",
            SimplifiedInstruction = "Para sumar +8, los dos sensores deben valer 1. Como sensor_boton = 1 y sensor_rampa = 1, ambos están activos. ¿Cuánto es 10 + 8?",
            ScreenReaderDescription = "Compuerta booleana AND: sensor botón igual a 1 y sensor rampa igual a 1. Si ambos son 1, energía es 10 más 8; si no, 10 más 2.",
            FirstHint = "Evalúa la compuerta Y (AND): ¿sensor_boton vale 1 Y sensor_rampa vale 1? ¡Sí, ambos valen 1!",
            SecondHintDetailed = "Como (1 Y 1) es Verdadero, se ejecuta la primera rama: energia = 10 + 8 = 18 unidades.",
            ExplanationOfSuccess = "¡Extraordinario! En la compuerta lógica Y (AND), 1 Y 1 = Verdadero, por lo que la rampa activó su potencia completa: 10 + 8 = 18 unidades."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "bool_and_item",
            Name = "Compuerta Y (1 AND 1 = Verdadero → +8)",
            EmojiSymbol = "🔌",
            AccessibleDescription = "Compuerta lógica AND con ambas entradas en 1, activando el impulso de 8 para un total de 18",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[5]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "12 unidades (rama SI NO: 10 + 2)", Value = 12, IsCorrect = false, SpeechFeedback = "12 sería si alguno de los sensores fuera 0, pero aquí ambos sensores valen 1.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "18 unidades (1 AND 1 es Verdadero: 10 + 8)", Value = 18, IsCorrect = true, SpeechFeedback = "¡Magnífico! Como ambos sensores valen 1, la compuerta Y es verdadera y 10 + 8 = 18.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "20 unidades (sumando 10 + 8 + 2)", Value = 20, IsCorrect = false, SpeechFeedback = "Recuerda que solo se suma el impulso de la rama verdadera (+8), no ambos.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 40 (Objetivo 15 - Programación Inclusiva 2): Ciclos PARA / MIENTRAS (for / while) y Variables Acumuladoras
    /// </summary>
    private PedagogicalTopic BuildProgrammingLoopsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_loops_for_while_01",
            Title = "El Motor de Repetición: Ciclos PARA (for) y Variables Acumuladoras",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido al Módulo 2 de Programación Inclusiva! ¿Recuerdas cómo la multiplicación resume sumas repetidas? En programación, cuando necesitamos que el código repita una tarea decenas o miles de veces (como leer cada renglón de un libro digital accesible), usamos un ciclo PARA (for) o MIENTRAS (while) junto con una variable acumuladora.",
            NarrativeIntroSimplified = "Un ciclo PARA (for) repite una orden varias veces automáticamente y va guardando la suma en una variable acumuladora.",
            CoreConceptExplanation = "En un ciclo 'PARA i = 1 HASTA N HACER { total = total + paso }', la variable contadora 'i' lleva la cuenta de las vueltas (iteraciones) desde 1 hasta N. En cada iteración, la variable acumuladora 'total' suma el valor de 'paso'. Al terminar las N vueltas, el total final es igual al valor inicial más (N × paso).",
            CoreConceptSimplified = "Si 'palabras' empieza en 4 y un ciclo repite 3 veces 'palabras = palabras + 5', sumamos 3 × 5 = 15 al 4 inicial, llegando a 19.",
            PrinciplesAddressed = new List<string>
            {
                "Estructura de control iterativa PARA (for) con contador de iteraciones",
                "Patrón de variable acumuladora (total = total + incremento) a lo largo de un ciclo",
                "Equivalencia entre iteración computacional y progresiones aritméticas"
            },
            CongratulationMessage = "¡Ingeniero de Ciclos! Dominas los bucles for/while y las variables acumuladoras para automatizar cualquier proceso."
        };

        var ex1 = new Exercise
        {
            Id = "prog_loop_ex_01",
            Title = "El Sintetizador de Voz: Ciclo PARA de 3 Iteraciones",
            GameType = GamePlayType.ProgrammingLoopsForWhile,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 19,
            StandardInstruction = "Nuestro lector de pantalla procesa párrafos usando un ciclo PARA (for) con una variable acumuladora:\n1) palabras = 4\n2) PARA i = 1 HASTA 3 HACER { palabras = palabras + 5 }\n¿Qué valor tendrá la variable 'palabras' al terminar las 3 iteraciones del ciclo?",
            SimplifiedInstruction = "La variable 'palabras' empieza en 4. El ciclo repite 3 veces la orden de sumar +5 (3 × 5 = 15). ¿Cuánto es 4 + 15?",
            ScreenReaderDescription = "Ciclo for: palabras inicia en 4. Para i de 1 hasta 3, palabras igual a palabras más 5. Calcular el valor final de palabras.",
            FirstHint = "Calcula cuánto se acumula en las 3 vueltas del ciclo: en cada vuelta se suman 5 palabras, así que 3 × 5 = 15 palabras nuevas.",
            SecondHintDetailed = "Suma el valor inicial de 'palabras' (4) más las 15 palabras acumuladas en el ciclo: 4 + 15 = 19 palabras.",
            ExplanationOfSuccess = "¡Iteración perfecta! Vuelta 1: 4 + 5 = 9; Vuelta 2: 9 + 5 = 14; Vuelta 3: 14 + 5 = 19 palabras en total."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "loop_for_item",
            Name = "Ciclo for (4 → 9 → 14 → 19)",
            EmojiSymbol = "🔄",
            AccessibleDescription = "Ciclo for de 3 iteraciones que suma 5 en cada vuelta: pasa de 4 a 9, luego a 14 y termina en 19",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[3]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "9 palabras (solo 1 vuelta del ciclo)", Value = 9, IsCorrect = false, SpeechFeedback = "9 es el valor tras la primera vuelta (4 + 5), pero el ciclo repite la suma 3 veces.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "15 palabras (olvidando el valor inicial 4)", Value = 15, IsCorrect = false, SpeechFeedback = "15 es lo que suma el ciclo (3 × 5), pero falta sumarle las 4 palabras con las que inició la variable.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "19 palabras (4 + 3 × 5)", Value = 19, IsCorrect = true, SpeechFeedback = "¡Exacto! 4 iniciales + 15 acumuladas en las 3 iteraciones = 19 palabras.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 41 (Objetivo 15 - Programación Inclusiva 2): Arreglos (Arrays), Listas e Índices desde Cero [0, 1, 2...]
    /// </summary>
    private PedagogicalTopic BuildProgrammingArraysTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_arrays_lists_01",
            Title = "El Estante Numerado desde Cero: Arreglos (Arrays) e Índices [0, 1, 2...]",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "Cuando queremos guardar toda una colección de datos en orden (como las notas musicales de una melodía accesible o la lista de precios de la tiendita), en lugar de crear cien variables sueltas usamos un Arreglo (Array) o Lista. ¡El secreto más famoso de la programación es que la primera posición de un arreglo siempre tiene el índice 0!",
            NarrativeIntroSimplified = "Un arreglo es una fila de cajitas numeradas empezando desde el 0: la primera cajita es [0], la segunda es [1] y la tercera es [2].",
            CoreConceptExplanation = "Un arreglo almacena elementos contiguos en memoria a los que accedemos por su índice entre corchetes. Como el índice mide cuántos pasos nos desplazamos desde el inicio, el primer elemento está en el índice [0], el segundo en [1], el tercero en [2] y el cuarto en [3]. Por ejemplo, en precios = [4, 7, 10, 12], precios[0] vale 4 y precios[2] vale 10.",
            CoreConceptSimplified = "En la lista precios = [4, 7, 10, 12]: precios[0] es 4 (el primero) y precios[2] es 10 (el tercero).",
            PrinciplesAddressed = new List<string>
            {
                "Estructura de datos lineal: Arreglos (Arrays) y colecciones ordenadas",
                "Indexación basada en cero (Zero-Based Indexing): desplazamientos desde la posición [0]",
                "Acceso directo por índice y operaciones aritméticas con elementos de un arreglo"
            },
            CongratulationMessage = "¡Maestro de las Estructuras de Datos! Dominas los arreglos y la indexación desde cero como todo un profesional."
        };

        var ex1 = new Exercise
        {
            Id = "prog_arr_ex_01",
            Title = "Catálogo en Memoria: precios[0] + precios[2]",
            GameType = GamePlayType.ProgrammingArraysZeroIndex,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 14,
            StandardInstruction = "En el catálogo digital de la tiendita tenemos el siguiente arreglo de precios en pesos, indexado desde 0:\n• precios = [4, 7, 10, 12]\n(Donde precios[0] = 4, precios[1] = 7, precios[2] = 10 y precios[3] = 12).\nSi el carrito de compras calcula total = precios[0] + precios[2], ¿cuántos pesos vale 'total'?",
            SimplifiedInstruction = "En la lista precios = [4, 7, 10, 12], la posición [0] vale 4 y la posición [2] vale 10. ¿Cuánto es 4 + 10 pesos?",
            ScreenReaderDescription = "Arreglo precios igual a 4, 7, 10, 12. Índice 0 vale 4, índice 1 vale 7, índice 2 vale 10, índice 3 vale 12. Calcular precios en 0 más precios en 2.",
            FirstHint = "Identifica los elementos por su índice empezando en 0: precios[0] es el primer número (4) y precios[2] es el tercer número (10).",
            SecondHintDetailed = "Suma ambos elementos del arreglo: precios[0] + precios[2] = 4 + 10 = 14 pesos.",
            ExplanationOfSuccess = "¡Excelente manejo de índices! En programación el conteo de posiciones inicia en [0]: precios[0] = 4 y precios[2] = 10, sumando 14 pesos."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "array_index_item",
            Name = "precios = [4, 7, 10, 12] ([0]=4, [2]=10)",
            EmojiSymbol = "🗂️",
            AccessibleDescription = "Arreglo de 4 elementos: índice 0 vale 4, índice 1 vale 7, índice 2 vale 10 e índice 3 vale 12",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[6]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "11 pesos (sumando precios[0] + precios[1]: 4 + 7)", Value = 11, IsCorrect = false, SpeechFeedback = "7 está en el índice [1], pero el código pidió precios[2], que vale 10.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "14 pesos (precios[0]=4 + precios[2]=10)", Value = 14, IsCorrect = true, SpeechFeedback = "¡Perfecto! precios[0] es 4 y precios[2] es 10; 4 + 10 = 14 pesos.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "17 pesos (confundiendo índices con 7 + 10)", Value = 17, IsCorrect = false, SpeechFeedback = "Recuerda que el primer elemento siempre es el índice [0] (que vale 4), no el [1].", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 42 (Objetivo 15 - Programación Inclusiva 2): Funciones Modulares, Parámetros y Valor de Retorno (return)
    /// </summary>
    private PedagogicalTopic BuildProgrammingFunctionsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_functions_params_01",
            Title = "Máquinas Reutilizables de Código: Funciones, Parámetros y Return",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "En Secundaria y Preparatoria aprendiste que una función matemática f(x) = 2x + 3 transforma una entrada x en una salida. ¡En programación una 'función' es exactamente eso! Es un bloque de código con nombre propio que recibe datos de entrada (parámetros), los procesa y devuelve un resultado con la instrucción RETORNAR (return).",
            NarrativeIntroSimplified = "Una función de código es como una máquina con nombre: le das un número de entrada (parámetro), hace el cálculo y te devuelve (return) el resultado.",
            CoreConceptExplanation = "Las funciones evitan repetir código y hacen los programas limpios y modulares. Al definir 'FUNCION calcularDecibeles(nivel) { RETORNAR (3 * nivel) + 2 }', 'nivel' es el parámetro de entrada. Cuando llamamos a 'calcularDecibeles(4)', el parámetro toma el valor 4, calcula (3 × 4) + 2 y retorna 14.",
            CoreConceptSimplified = "En 'FUNCION calcularDecibeles(nivel) { RETORNAR 3 * nivel + 2 }', si llamamos a calcularDecibeles(4), calcula 3 × 4 + 2 = 14.",
            PrinciplesAddressed = new List<string>
            {
                "Conexión directa entre funciones algebraicas f(x) y funciones en programación",
                "Paso de argumentos a parámetros de entrada y evaluación del cuerpo de la función",
                "Valor de retorno (return) y reutilización modular de código accesible"
            },
            CongratulationMessage = "¡Arquitecto de Software Modular! Has completado el Objetivo 15 dominando ciclos, arreglos y funciones."
        };

        var ex1 = new Exercise
        {
            Id = "prog_func_ex_01",
            Title = "Llamando a la Función: calcularDecibeles(4)",
            GameType = GamePlayType.ProgrammingFunctionsReturn,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 14,
            StandardInstruction = "En el módulo de audio accesible programamos esta función reutilizable:\n• FUNCION calcularDecibeles(nivel) { RETORNAR (3 * nivel) + 2 }\nSi el programa principal invoca la función pasándole el argumento 4: calcularDecibeles(4), ¿qué valor numérico retorna (return) la función?",
            SimplifiedInstruction = "La función 'calcularDecibeles' multiplica el número de entrada por 3 y luego suma 2. Si entra el 4, ¿cuánto es (3 × 4) + 2?",
            ScreenReaderDescription = "Función calcularDecibeles con parámetro nivel: retorna 3 por nivel más 2. Evaluar calcularDecibeles con argumento 4.",
            FirstHint = "Sustituye el parámetro 'nivel' por el argumento 4: primero multiplica 3 × 4 = 12.",
            SecondHintDetailed = "Ahora súmale 2 al producto: 12 + 2 = 14 decibeles. Ese es el valor que retorna la función.",
            ExplanationOfSuccess = "¡Magnífico! Al llamar a calcularDecibeles(4), el parámetro 'nivel' recibió 4, evaluó (3 × 4) + 2 = 14 y retornó 14 decibeles."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "func_call_item",
            Name = "calcularDecibeles(4) → return 14",
            EmojiSymbol = "⚙️",
            AccessibleDescription = "Función calcularDecibeles: recibe entrada 4, calcula 3 por 4 más 2 y retorna 14",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[7]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "9 decibeles (sumando 3 + 4 + 2 en vez de multiplicar)", Value = 9, IsCorrect = false, SpeechFeedback = "Recuerda que '3 * nivel' indica multiplicación (3 × 4 = 12), no suma.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "12 decibeles (olvidando sumar + 2 antes del return)", Value = 12, IsCorrect = false, SpeechFeedback = "12 es el resultado de 3 × 4, pero falta sumar + 2 como indica la expresión de retorno.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "14 decibeles (return 3 × 4 + 2 = 14)", Value = 14, IsCorrect = true, SpeechFeedback = "¡Exacto! (3 × 4) + 2 = 14 decibeles retornados.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 43 (Objetivo 16 - Programación Inclusiva 3): Algoritmos Paso a Paso de Búsqueda y Máximos/Mínimos
    /// </summary>
    private PedagogicalTopic BuildProgrammingAlgorithmsSearchTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_algorithms_search_01",
            Title = "Recetas Paso a Paso: Algoritmos de Búsqueda y Valor Máximo",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenido al Módulo 3 de Programación Inclusiva! Un 'algoritmo' (palabra que viene del matemático Al-Juarismi) es una secuencia finita y ordenada de pasos sin ambigüedad para resolver un problema. Combinando un arreglo, un ciclo PARA y un condicional SI, podemos programar un algoritmo que recorra cualquier colección de datos y encuentre el valor máximo o mínimo.",
            NarrativeIntroSimplified = "Un algoritmo es una receta paso a paso. Para hallar el número más grande de una lista, revisamos uno por uno y guardamos siempre el mayor.",
            CoreConceptExplanation = "El algoritmo clásico de búsqueda del máximo funciona en dos pasos: 1) Inicializamos la variable 'maximo' con el primer elemento de la lista (maximo = lista[0]). 2) Recorremos la lista con un ciclo y, en cada elemento, preguntamos: 'SI (valor > maximo) ENTONCES { maximo = valor }'. Al finalizar el recorrido, 'maximo' contiene con certeza el mayor valor de toda la colección.",
            CoreConceptSimplified = "En la lista [6, 14, 9, 11], empezamos con máximo = 6; al ver el 14 lo cambiamos a 14, y como 9 y 11 son menores, el máximo final es 14.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto formal de algoritmo: secuencia finita, definida y ordenada de instrucciones",
                "Integración de arreglos, ciclos (for) y condicionales (if) en un algoritmo de búsqueda",
                "Traza paso a paso del algoritmo de búsqueda del valor máximo / mínimo"
            },
            CongratulationMessage = "¡Estratega Algorítmico! Sabes diseñar y rastrear algoritmos de búsqueda combinando ciclos, arreglos y condicionales."
        };

        var ex1 = new Exercise
        {
            Id = "prog_algo_ex_01",
            Title = "Buscando la Señal Más Clara: Algoritmo del Máximo",
            GameType = GamePlayType.ProgrammingAlgorithmsSearch,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 14,
            StandardInstruction = "Nuestro bastón electrónico inteligente analiza 4 lecturas de sensores = [6, 14, 9, 11] ejecutando este algoritmo:\n1) maximo = sensores[0] (inicia en 6)\n2) PARA CADA valor EN [6, 14, 9, 11]: SI (valor > maximo) ENTONCES { maximo = valor }\n¿Qué valor queda almacenado en la variable 'maximo' al terminar el algoritmo?",
            SimplifiedInstruction = "El algoritmo revisa uno por uno los números de la lista [6, 14, 9, 11] y guarda siempre el más grande en 'maximo'. ¿Cuál es el valor final de 'maximo'?",
            ScreenReaderDescription = "Algoritmo de búsqueda del máximo en la lista 6, 14, 9 y 11. Inicia máximo en 6 y actualiza cuando encuentra un valor mayor.",
            FirstHint = "Empieza con maximo = 6. Al comparar con el segundo elemento (14), como 14 > 6, ahora maximo = 14.",
            SecondHintDetailed = "Luego compara 14 con 9 y con 11. Como ninguno es mayor que 14, la variable 'maximo' termina con el valor 14.",
            ExplanationOfSuccess = "¡Traza algorítmica perfecta! El algoritmo inició en 6, se actualizó a 14 en el segundo paso y conservó 14 hasta el final."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "algo_search_item",
            Name = "sensores = [6, 14, 9, 11] → máx = 14",
            EmojiSymbol = "🔍",
            AccessibleDescription = "Algoritmo de búsqueda en la lista 6, 14, 9, 11 que identifica el valor máximo 14",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[5]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "6 puntos (el primer elemento sensores[0])", Value = 6, IsCorrect = false, SpeechFeedback = "6 es solo el valor inicial antes de comparar con el resto de la lista.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "11 puntos (el último elemento de la lista)", Value = 11, IsCorrect = false, SpeechFeedback = "11 es el último elemento, pero como 11 no es mayor que 14, no reemplaza al máximo.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "14 puntos (el valor máximo encontrado)", Value = 14, IsCorrect = true, SpeechFeedback = "¡Exacto! 14 es el mayor valor de la lista y queda guardado en la variable 'maximo'.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 44 (Objetivo 16 - Programación Inclusiva 3): Física y Coordenadas 2D en Videojuegos Accesibles (x = x + v·t)
    /// </summary>
    private PedagogicalTopic BuildProgrammingGameCoordinatesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_game_coordinates_01",
            Title = "Física y Coordenadas en Videojuegos Accesibles: Vectores (x = x + v·t)",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¿Cómo se mueven los personajes en un videojuego accesible con sonido espacial 3D? ¡Uniendo el plano cartesiano y las ecuaciones lineales que aprendiste en Secundaria y Preparatoria con el bucle principal del juego (Game Loop)! En cada segundo, el programa calcula la nueva posición sumando a la coordenada inicial el producto de la velocidad por el tiempo.",
            NarrativeIntroSimplified = "En un videojuego, para mover un personaje calculamos su nueva posición sumando dónde empezó más su velocidad multiplicada por el tiempo.",
            CoreConceptExplanation = "En programación de videojuegos 2D, cada objeto tiene coordenadas (x, y). Cuando se mueve con velocidad constante v_x durante un tiempo t, el motor de física ejecuta la ecuación cinemática: 'x_final = x_inicial + (velocidad_x * tiempo)'. En juegos accesibles para personas con discapacidad visual, esta coordenada X controla además el balance estéreo (izquierda-centro-derecha) del sonido.",
            CoreConceptSimplified = "Si la nave inicia en X = 5 y avanza a velocidad 4 durante 3 segundos, recorre 4 × 3 = 12 y llega a X = 5 + 12 = 17.",
            PrinciplesAddressed = new List<string>
            {
                "Representación de entidades mediante coordenadas (x, y) y vectores de velocidad en motores de juego",
                "Actualización de estado físico en el Game Loop: x_final = x_inicial + (v_x · t)",
                "Mapeo de coordenadas espaciales a audio binaural/estéreo en videojuegos accesibles"
            },
            CongratulationMessage = "¡Desarrollador de Videojuegos Accesibles! Sabes programar movimiento vectorial y coordenadas en el plano."
        };

        var ex1 = new Exercise
        {
            Id = "prog_game_ex_01",
            Title = "La Nave Sonora de Cosmo: Movimiento en el Eje X",
            GameType = GamePlayType.ProgrammingGameCoordinates2D,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 17,
            StandardInstruction = "En nuestro videojuego espacial accesible, la nave de Cosmo actualiza su coordenada horizontal X con esta instrucción del motor de física:\n• x_inicial = 5\n• velocidad_x = 4 casillas por segundo, tiempo = 3 segundos\n• x_final = x_inicial + (velocidad_x * tiempo)\n¿En qué coordenada X se ubicará la nave tras los 3 segundos?",
            SimplifiedInstruction = "La nave empieza en la posición X = 5 y avanza 4 casillas cada segundo durante 3 segundos (4 × 3 = 12). ¿A qué posición llega (5 + 12)?",
            ScreenReaderDescription = "Motor de videojuego: x inicial igual a 5, velocidad 4 casillas por segundo durante 3 segundos. Calcular x final igual a 5 más 4 por 3.",
            FirstHint = "Por jerarquía de operaciones, multiplica primero la velocidad por el tiempo: 4 × 3 = 12 casillas de avance.",
            SecondHintDetailed = "Suma las 12 casillas de avance a la posición inicial X = 5: 5 + 12 = 17 en el eje X.",
            ExplanationOfSuccess = "¡Física y código en perfecta armonía! La nave se desplazó 4 × 3 = 12 unidades desde X = 5 hasta llegar a X = 17."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "game_physics_item",
            Name = "Nave X: 5 + (4 × 3) = 17",
            EmojiSymbol = "🎮",
            AccessibleDescription = "Coordenada de la nave en el videojuego: inicia en 5, avanza 4 por 3 igual a 12, llegando a 17",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[6]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "12 en X (olvidando sumar la posición inicial 5)", Value = 12, IsCorrect = false, SpeechFeedback = "12 es la distancia avanzada (4 × 3), pero falta sumarle la coordenada inicial x_inicial = 5.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "17 en X (5 + 4 × 3 = 17)", Value = 17, IsCorrect = true, SpeechFeedback = "¡Exacto! 5 iniciales + 12 de desplazamiento = coordenada 17 en X.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "27 en X (sumando 5 + 4 antes de multiplicar por 3)", Value = 27, IsCorrect = false, SpeechFeedback = "Recuerda resolver primero la multiplicación velocidad × tiempo (4 × 3 = 12) y al final sumar 5.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    /// <summary>
    /// Tema 45 (Objetivo 16 - Programación Inclusiva 3): Programando Tecnología Inclusiva e IA Asistiva con Sensores y Datos
    /// </summary>
    private PedagogicalTopic BuildProgrammingAssistiveAITopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prog_assistive_ai_data_01",
            Title = "Programando Tecnología Inclusiva: Sensores, Promedios e IA Asistiva",
            Level = AcademicLevel.ProgrammingFoundations,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Llegaste a la cumbre del Reino Matemágico y de la Programación Inclusiva! Desde contar tus primeros objetos en Preescolar hasta dominar el Cálculo, la Estadística y el Código, ahora tienes todas las herramientas para crear tecnología que cambie vidas: lectores de pantalla, guantes traductores de Lengua de Señas, sillas de ruedas autónomas e Inteligencia Artificial accesible.",
            NarrativeIntroSimplified = "Combinando promedios estadísticos y funciones de código podemos programar Inteligencia Artificial que traduce señas y ayuda a las personas.",
            CoreConceptExplanation = "Los sistemas de Inteligencia Artificial y tecnología asistiva leen datos del mundo real mediante sensores. Como los sensores tienen pequeñas variaciones, el software primero filtra la señal calculando la media aritmética de varias lecturas: 'promedio = (s1 + s2 + s3) / 3', y luego aplica un modelo o calibración ('senal_limpia = promedio + ajuste') para tomar decisiones precisas y confiables.",
            CoreConceptSimplified = "Si 3 sensores miden [7, 9, 11], su promedio es (7 + 9 + 11) ÷ 3 = 9. Si sumamos +4 de calibración, la señal final limpia es 13.",
            PrinciplesAddressed = new List<string>
            {
                "Filtrado de ruido en sensores mediante la media aritmética muestral x̄ = (s₁ + s₂ + s₃) / 3",
                "Pipeline de procesamiento de datos en IA asistiva: lectura → promedio → calibración → salida",
                "Integración de matemáticas y programación para el diseño de tecnología inclusiva"
            },
            CongratulationMessage = "¡Graduado de Honor en Matemáticas y Programación Inclusiva! Has completado las 45 estaciones del Reino Matemágico y estás listo para programar un mundo más accesible para todas y todos."
        };

        var ex1 = new Exercise
        {
            Id = "prog_ai_ex_01",
            Title = "Guante Traductor de Lengua de Señas con IA",
            GameType = GamePlayType.ProgrammingAssistiveAIData,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 13,
            StandardInstruction = "Programamos un guante inteligente que traduce Lengua de Señas a voz. Para filtrar el ruido del sensor de flexión, el código ejecuta estos 3 pasos:\n1) lecturas = [7, 9, 11]\n2) promedio = (7 + 9 + 11) / 3\n3) senal_limpia = promedio + 4\n¿Cuál es el valor final de 'senal_limpia' que recibe el sintetizador de voz?",
            SimplifiedInstruction = "Primero saca el promedio de [7, 9, 11]: suma 7 + 9 + 11 = 27 y divide entre 3 (27 ÷ 3 = 9). Luego súmale 4 de calibración (9 + 4). ¿Cuánto da?",
            ScreenReaderDescription = "IA asistiva: lecturas de sensores 7, 9 y 11. Promedio igual a la suma entre 3. Señal limpia igual a promedio más 4.",
            FirstHint = "Calcula primero el promedio en la línea 2: (7 + 9 + 11) ÷ 3 = 27 ÷ 3 = 9.",
            SecondHintDetailed = "En la línea 3 suma la calibración (+4) al promedio obtenido (9): 9 + 4 = 13 puntos de señal.",
            ExplanationOfSuccess = "¡Extraordinario! El algoritmo promedió las 3 lecturas (27 ÷ 3 = 9) y sumó la calibración (+4) para entregar una señal limpia de 13. ¡Ya eres programador de tecnología inclusiva!"
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "ai_glove_item",
            Name = "IA Guante: x̄(7,9,11)=9 + 4 = 13",
            EmojiSymbol = "🤖",
            AccessibleDescription = "Módulo de IA del guante traductor: promedio de 7, 9 y 11 es 9, más 4 de calibración igual a 13",
            OrderIndex = 1,
            MusicalPitchHz = Frequencies[7]
        });

        ex1.Options.Add(new ExerciseOption { Id = "opt1", Label = "9 señal (solo el promedio sin sumar la calibración +4)", Value = 9, IsCorrect = false, SpeechFeedback = "9 es el valor de 'promedio' en la línea 2; falta ejecutar la línea 3 sumando + 4.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt2", Label = "13 señal (promedio 9 + calibración 4 = 13)", Value = 13, IsCorrect = true, SpeechFeedback = "¡Magnífico! Promedio 9 + calibración 4 = 13 puntos de señal limpia.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt3", Label = "31 señal (sumando 27 + 4 sin dividir entre 3)", Value = 31, IsCorrect = false, SpeechFeedback = "27 es la suma de los 3 sensores; recuerda dividirla entre 3 para hallar el promedio antes de sumar 4.", AccessibleKeyShortcut = "3" });

        topic.Exercises.Add(ex1);
        return topic;
    }

    private static PedagogicalTopic BuildAnglesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_geometry_angles_01",
            Title = "El Radar de los Giros y los Ángulos",
            Level = AcademicLevel.MiddleElementary,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Bienvenidos al Radar de los Giros! En la naturaleza y en las construcciones, las líneas se cruzan formando esquinas y aberturas llamadas ángulos. Un ángulo recto mide exactamente 90 grados, como la esquina de una ventana o un libro. Si la abertura es más cerrada y afilada es un ángulo agudo, y si es más abierta y amplia es un ángulo obtuso.",
            NarrativeIntroSimplified = "¡Mira las esquinas! Las esquinas se llaman ángulos. Una esquina recta mide 90 grados. Si es chiquita y cerrada es aguda. Si es abierta y ancha es obtusa.",
            CoreConceptExplanation = "Un ángulo mide la cantidad de giro entre dos líneas que se unen en un punto común llamado vértice. Los ángulos se miden en grados (°). Un cuarto de vuelta completa es un ángulo recto de 90°. Los ángulos menores a 90° son agudos (afilados). Los ángulos mayores a 90° pero menores a 180° son obtusos (abiertos). Media vuelta completa forma una línea recta llamada ángulo llano de 180°.",
            CoreConceptSimplified = "El ángulo es cuánto gira una línea. 90° es recto como una esquina. Menos de 90° es agudo. Más de 90° es obtuso. 180° es una línea recta.",
            PrinciplesAddressed = new List<string>
            {
                "Identificación del ángulo recto (90°) en el entorno cotidiano y figuras geométricas",
                "Clasificación cualitativa y cuantitativa de ángulos agudos (< 90°) y obtusos (> 90°)",
                "Comprensión del giro y la abertura angular (ángulo llano de 180°)"
            },
            CongratulationMessage = "¡Excelente astrónomo de los giros! Ahora reconoces y clasificas cualquier esquina o ángulo del universo."
        };

        var ex1 = new Exercise
        {
            Id = "angle_ex_01",
            Title = "La Esquina Recta de la Ventana (90°)",
            GameType = GamePlayType.GeometryAnglesClassification,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 90,
            StandardInstruction = "Cosmo observa el marco de la ventana del observatorio. Los dos marcos forman una esquina perpendicular en L perfecta (un cuarto de giro). ¿Cuántos grados mide este ángulo recto?",
            SimplifiedInstruction = "¿Cuántos grados mide una esquina recta cuadrada?",
            ScreenReaderDescription = "Una ventana con dos marcos perpendiculares formando una esquina en L de 90 grados.",
            FirstHint = "Un ángulo recto es un cuarto de giro completo y forma una letra L cuadrada.",
            SecondHintDetailed = "La esquina de un cuadrado o una hoja de papel siempre mide exactamente 90 grados (90°).",
            ExplanationOfSuccess = "¡Exacto! El ángulo recto mide exactamente 90 grados (90°).",
            SimplifiedFirstHint = "Busca la esquina cuadrada.",
            SimplifiedSecondHint = "Mide 90 grados.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 90 grados."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "angle_v_1", Name = "Vértice", EmojiSymbol = "📐", AccessibleDescription = "Vértice del ángulo recto", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "angle_v_2", Name = "Esquina de 90°", EmojiSymbol = "🟦", AccessibleDescription = "Marco cuadrado que forma 90 grados", OrderIndex = 2, MusicalPitchHz = 392 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_a1", Label = "45° (agudo)", Value = 45, IsCorrect = false, SpeechFeedback = "45 grados es un ángulo agudo, la mitad de uno recto." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_a2", Label = "90° (recto)", Value = 90, IsCorrect = true, SpeechFeedback = "¡Correcto! Un ángulo recto mide exactamente 90 grados." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_a3", Label = "120° (obtuso)", Value = 120, IsCorrect = false, SpeechFeedback = "120 grados es más abierto que una esquina recta." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_a4", Label = "180° (llano)", Value = 180, IsCorrect = false, SpeechFeedback = "180 grados es una línea recta completa." });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "angle_ex_02",
            Title = "El Pico Afilado de la Nave (Ángulo Agudo)",
            GameType = GamePlayType.GeometryAnglesClassification,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 45,
            StandardInstruction = "La proa de la nave exploradora tiene una abertura afilada que mide 45 grados. Como es menor a 90 grados, ¿qué tipo de ángulo es?",
            SimplifiedInstruction = "Un ángulo que mide 45 grados (menos de 90 grados), ¿es agudo, recto u obtuso?",
            ScreenReaderDescription = "La punta de una nave con abertura afilada de 45 grados, menor a una esquina recta.",
            FirstHint = "Los ángulos menores a 90° se llaman agudos porque terminan en punta.",
            SecondHintDetailed = "Como 45° es menor que 90°, se clasifica como ángulo agudo.",
            ExplanationOfSuccess = "¡Correcto! Los ángulos que miden menos de 90° son ángulos agudos.",
            SimplifiedFirstHint = "Es afilado como una aguja.",
            SimplifiedSecondHint = "Es un ángulo agudo.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Es un ángulo agudo."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "angle_acute_1", Name = "Proa afilada", EmojiSymbol = "🔺", AccessibleDescription = "Punta triangular afilada de 45 grados", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_b1", Label = "Ángulo agudo (menor a 90°)", Value = 45, IsCorrect = true, SpeechFeedback = "¡Excelente! Los ángulos menores a 90° son agudos." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_b2", Label = "Ángulo recto (90°)", Value = 90, IsCorrect = false, SpeechFeedback = "El ángulo recto mide 90 grados exactos." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_b3", Label = "Ángulo obtuso (mayor a 90°)", Value = 120, IsCorrect = false, SpeechFeedback = "El ángulo obtuso es más abierto que 90 grados." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_b4", Label = "Ángulo llano (180°)", Value = 180, IsCorrect = false, SpeechFeedback = "El ángulo llano mide 180 grados." });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "angle_ex_03",
            Title = "El Abanico Solar (Ángulo Obtuso)",
            GameType = GamePlayType.GeometryAnglesClassification,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 120,
            StandardInstruction = "Un panel solar se desplegó ampliamente formando una abertura de 120 grados. Como 120° es mayor a 90° y menor a 180°, ¿cómo se llama este ángulo?",
            SimplifiedInstruction = "Un ángulo de 120 grados (más abierto que una esquina recta), ¿qué tipo de ángulo es?",
            ScreenReaderDescription = "Un panel solar abierto ampliamente a 120 grados.",
            FirstHint = "Los ángulos mayores a 90° pero menores a 180° son obtusos.",
            SecondHintDetailed = "Como 120° es mayor a 90°, se clasifica como ángulo obtuso.",
            ExplanationOfSuccess = "¡Brillante! Un ángulo mayor a 90° pero menor a 180° es un ángulo obtuso.",
            SimplifiedFirstHint = "Está muy abierto.",
            SimplifiedSecondHint = "Es un ángulo obtuso.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Es un ángulo obtuso."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "angle_obtuse_1", Name = "Panel extendido", EmojiSymbol = "🪭", AccessibleDescription = "Panel solar abierto a 120 grados", OrderIndex = 1, MusicalPitchHz = 440 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c1", Label = "Ángulo agudo (menor a 90°)", Value = 45, IsCorrect = false, SpeechFeedback = "Un ángulo agudo es cerrado y menor a 90 grados." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c2", Label = "Ángulo obtuso (mayor a 90°)", Value = 120, IsCorrect = true, SpeechFeedback = "¡Correcto! 120 grados es un ángulo obtuso." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c3", Label = "Ángulo recto (90°)", Value = 90, IsCorrect = false, SpeechFeedback = "Un ángulo recto mide exactamente 90 grados." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c4", Label = "Ángulo completo (360°)", Value = 360, IsCorrect = false, SpeechFeedback = "Un ángulo completo es una vuelta entera de 360 grados." });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "angle_ex_04",
            Title = "La Media Vuelta del Telescopio (Ángulo Llano)",
            GameType = GamePlayType.GeometryAnglesClassification,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 180,
            StandardInstruction = "El telescopio astronómico giró desde el este hasta el oeste formando una línea recta horizontal completa (media vuelta). ¿Cuántos grados mide este ángulo llano?",
            SimplifiedInstruction = "¿Cuántos grados mide media vuelta en línea recta?",
            ScreenReaderDescription = "Una línea recta horizontal con un arco que indica media vuelta de 180 grados.",
            FirstHint = "Si dos ángulos rectos de 90° se juntan, suman media vuelta.",
            SecondHintDetailed = "90° + 90° = 180°. Un ángulo llano mide exactamente 180 grados.",
            ExplanationOfSuccess = "¡Extraordinario! Media vuelta completa forma una línea recta de 180 grados (ángulo llano).",
            SimplifiedFirstHint = "Suma 90 + 90.",
            SimplifiedSecondHint = "Mide 180 grados.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 180 grados."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "angle_straight_1", Name = "Línea de 180°", EmojiSymbol = "🔭", AccessibleDescription = "Telescopio apuntando en media vuelta recta", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_d1", Label = "90° (un cuarto de vuelta)", Value = 90, IsCorrect = false, SpeechFeedback = "90 grados es un cuarto de vuelta." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_d2", Label = "180° (media vuelta)", Value = 180, IsCorrect = true, SpeechFeedback = "¡Exacto! Un ángulo llano mide 180 grados." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_d3", Label = "270° (tres cuartos de vuelta)", Value = 270, IsCorrect = false, SpeechFeedback = "270 grados son tres cuartos de vuelta." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_d4", Label = "360° (vuelta entera)", Value = 360, IsCorrect = false, SpeechFeedback = "360 grados es una vuelta entera completa." });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildAreaTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_geometry_area_01",
            Title = "El Taller de los Mosaicos y Superficies: Áreas 2D",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Constructor",
            NarrativeIntro = "¡Bienvenidos al Taller de los Mosaicos! Cuando cubrimos un piso o pintamos una pared, no solo nos interesan los bordes: medimos el espacio plano de adentro llamado Área. El área se calcula contando cuántas baldosas o cuadritos caben. Para un rectángulo o cuadrado, multiplicamos la base por la altura. Y para un triángulo, ¡calculamos la mitad de un rectángulo!",
            NarrativeIntroSimplified = "¡Vamos a llenar el piso de baldosas! El área es cuántos cuadritos caben adentro. Multiplicamos base por altura. En un triángulo dividimos entre dos.",
            CoreConceptExplanation = "El área es la medida de la región o superficie encerrada dentro de los límites de una figura geométrica plana. Se expresa en unidades cuadradas (cm², m²). En un rectángulo de base b y altura h, el Área = b × h. En un cuadrado de lado l, el Área = l × l = l². En un triángulo, dado que cualquier triángulo es exactamente la mitad de un rectángulo con su misma base y altura, el Área = (b × h) / 2.",
            CoreConceptSimplified = "Área = Base × Altura. En un rectángulo multiplicas los dos lados. En un triángulo multiplicas base por altura y divides entre dos.",
            PrinciplesAddressed = new List<string>
            {
                "Concepto de superficie plana y conteo de unidades cuadradas (m²)",
                "Fórmula del área del rectángulo y cuadrado: Área = base × altura",
                "Deducción geométrica del área del triángulo como mitad de un rectángulo: Área = (b × h) / 2"
            },
            CongratulationMessage = "¡Genio de las superficies! Ahora calculas con precisión el área de rectángulos, cuadrados y triángulos."
        };

        var ex1 = new Exercise
        {
            Id = "area_ex_01",
            Title = "La Cuadrícula de Baldosas (3 × 4)",
            GameType = GamePlayType.GeometryArea2D,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 12,
            StandardInstruction = "En el piso del taller colocamos una cuadrícula de 3 filas de baldosas por 4 columnas de baldosas. Cada baldosa mide 1 metro cuadrado (1 m²). ¿Cuál es el área total del piso?",
            SimplifiedInstruction = "Hay 3 filas con 4 baldosas cada una. Multiplica 3 × 4. ¿Cuántos cuadritos hay?",
            ScreenReaderDescription = "Una cuadrícula de mosaicos con 3 filas y 4 columnas, dando 12 baldosas en total.",
            FirstHint = "Multiplica el número de filas por el número de columnas: 3 × 4.",
            SecondHintDetailed = "3 × 4 = 12 baldosas de 1 m². El área total es 12 m².",
            ExplanationOfSuccess = "¡Excelente! 3 filas × 4 columnas = 12 metros cuadrados (12 m²).",
            SimplifiedFirstHint = "Multiplica 3 por 4.",
            SimplifiedSecondHint = "Son 12 cuadritos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 12 m²."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "tile_row1", Name = "Fila 1 (4 baldosas)", EmojiSymbol = "🟩", AccessibleDescription = "Fila superior de 4 baldosas", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "tile_row2", Name = "Fila 2 (4 baldosas)", EmojiSymbol = "🟩", AccessibleDescription = "Fila central de 4 baldosas", OrderIndex = 2, MusicalPitchHz = 330 });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "tile_row3", Name = "Fila 3 (4 baldosas)", EmojiSymbol = "🟩", AccessibleDescription = "Fila inferior de 4 baldosas", OrderIndex = 3, MusicalPitchHz = 392 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_ar1", Label = "7 m²", Value = 7, IsCorrect = false, SpeechFeedback = "7 es sumarlos (3 + 4), pero para el área debemos multiplicar las filas por las columnas." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_ar2", Label = "12 m²", Value = 12, IsCorrect = true, SpeechFeedback = "¡Correcto! 3 filas por 4 columnas son 12 metros cuadrados." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_ar3", Label = "14 m²", Value = 14, IsCorrect = false, SpeechFeedback = "14 no es el producto de 3 por 4." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_ar4", Label = "16 m²", Value = 16, IsCorrect = false, SpeechFeedback = "16 sería un cuadrado de 4 por 4." });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "area_ex_02",
            Title = "El Patio de Aterrizaje Rectangular (6 × 5)",
            GameType = GamePlayType.GeometryArea2D,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 30,
            StandardInstruction = "Un patio rectangular de aterrizaje mide 6 metros de base y 5 metros de altura. Aplicando la fórmula Área = Base × Altura, ¿cuál es el área del patio?",
            SimplifiedInstruction = "El patio mide 6 de base y 5 de altura. Multiplica 6 × 5.",
            ScreenReaderDescription = "Un rectángulo de 6 metros de base y 5 metros de altura.",
            FirstHint = "Multiplica la base por la altura: 6 × 5.",
            SecondHintDetailed = "6 × 5 = 30. El área del rectángulo es 30 m².",
            ExplanationOfSuccess = "¡Exacto! Base (6) × Altura (5) = 30 metros cuadrados (30 m²).",
            SimplifiedFirstHint = "Multiplica 6 por 5.",
            SimplifiedSecondHint = "6 por 5 da 30.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 30 m²."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "area_rect_item", Name = "Patio Rectangular", EmojiSymbol = "🟨", AccessibleDescription = "Rectángulo de 6 metros de largo por 5 de ancho", OrderIndex = 1, MusicalPitchHz = 349 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_rec1", Label = "22 m²", Value = 22, IsCorrect = false, SpeechFeedback = "22 metros sería el perímetro (6+5+6+5), pero el área es la multiplicación." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_rec2", Label = "30 m²", Value = 30, IsCorrect = true, SpeechFeedback = "¡Correcto! 6 por 5 son 30 metros cuadrados." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_rec3", Label = "35 m²", Value = 35, IsCorrect = false, SpeechFeedback = "35 sería 7 por 5." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_rec4", Label = "60 m²", Value = 60, IsCorrect = false, SpeechFeedback = "60 es el doble del área real." });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "area_ex_03",
            Title = "La Plataforma Cuadrada (7 × 7)",
            GameType = GamePlayType.GeometryArea2D,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 49,
            StandardInstruction = "Una plataforma solar es un cuadrado perfecto con lados de 7 metros. Aplicando la fórmula del cuadrado (Lado × Lado = 7 × 7), ¿cuál es su área total?",
            SimplifiedInstruction = "Un cuadrado de 7 por 7. Multiplica 7 × 7.",
            ScreenReaderDescription = "Un cuadrado regular con los cuatro lados de 7 metros cada uno.",
            FirstHint = "Multiplica el lado por sí mismo: 7 × 7.",
            SecondHintDetailed = "7 × 7 = 49. El área de la plataforma cuadrada es 49 m².",
            ExplanationOfSuccess = "¡Brillante! 7 × 7 = 49 metros cuadrados (49 m²).",
            SimplifiedFirstHint = "Multiplica 7 por 7.",
            SimplifiedSecondHint = "7 veces 7 es 49.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 49 m²."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "area_sq_item", Name = "Plataforma Cuadrada", EmojiSymbol = "🟦", AccessibleDescription = "Plataforma cuadrada de 7 por 7 metros", OrderIndex = 1, MusicalPitchHz = 440 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sq1", Label = "28 m²", Value = 28, IsCorrect = false, SpeechFeedback = "28 metros es el perímetro exterior (4 lados de 7), no el área." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sq2", Label = "42 m²", Value = 42, IsCorrect = false, SpeechFeedback = "42 es 7 por 6." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sq3", Label = "49 m²", Value = 49, IsCorrect = true, SpeechFeedback = "¡Correcto! 7 por 7 son 49 metros cuadrados." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sq4", Label = "56 m²", Value = 56, IsCorrect = false, SpeechFeedback = "56 es 7 por 8." });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "area_ex_04",
            Title = "La Vela Solar Triangular ((Base × Altura) / 2)",
            GameType = GamePlayType.GeometryArea2D,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 15,
            StandardInstruction = "Una vela espacial triangular tiene una base de 6 metros y una altura de 5 metros. Recuerda que el área de un triángulo es la mitad de un rectángulo: (Base × Altura) / 2. ¿Cuál es el área de la vela?",
            SimplifiedInstruction = "Multiplica 6 × 5 = 30, y luego divide entre 2. ¿Cuánto da la mitad de 30?",
            ScreenReaderDescription = "Un triángulo de 6 metros de base y 5 metros de altura.",
            FirstHint = "Primero calcula el rectángulo completo (6 × 5 = 30) y luego sácale la mitad.",
            SecondHintDetailed = "30 dividido entre 2 = 15. El área del triángulo es 15 m².",
            ExplanationOfSuccess = "¡Extraordinario! (6 × 5) / 2 = 30 / 2 = 15 metros cuadrados (15 m²).",
            SimplifiedFirstHint = "Saca la mitad de 30.",
            SimplifiedSecondHint = "La mitad de 30 es 15.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 15 m²."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "area_tri_item", Name = "Vela Triangular", EmojiSymbol = "🔺", AccessibleDescription = "Triángulo con base de 6 metros y altura de 5 metros", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_tri1", Label = "11 m²", Value = 11, IsCorrect = false, SpeechFeedback = "11 es la suma de 6 y 5, pero debemos multiplicar y dividir entre dos." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_tri2", Label = "15 m²", Value = 15, IsCorrect = true, SpeechFeedback = "¡Exacto! (6 por 5) entre 2 son 15 metros cuadrados." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_tri3", Label = "30 m²", Value = 30, IsCorrect = false, SpeechFeedback = "30 m² sería el rectángulo entero. ¡El triángulo es solo la mitad!" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_tri4", Label = "60 m²", Value = 60, IsCorrect = false, SpeechFeedback = "60 es cuatro veces el área del triángulo." });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildVolumeTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_geometry_volume_01",
            Title = "La Fábrica de Cubos y Prismas: Volúmenes 3D",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Maya la Ingeniera Espacial",
            NarrativeIntro = "¡Bienvenidos a la Fábrica de Contenedores Espaciales! En nuestro mundo de tres dimensiones, las cosas no solo son planas: tienen cuerpo, profundidad y altura. El volumen mide cuánto espacio ocupa un objeto o cuántos cubitos de 1×1×1 caben adentro. Para una caja rectangular, multiplicamos largo por ancho por alto.",
            NarrativeIntroSimplified = "¡Mira las cajas 3D! El volumen es el espacio que hay adentro de una caja. Multiplicamos largo por ancho por alto: cuántos cubos hay en la base por cuántos pisos tiene.",
            CoreConceptExplanation = "El volumen es la medida del espacio tridimensional ocupado por un cuerpo geométrico y se expresa en unidades cúbicas (cm³, m³). En un prisma rectangular de dimensiones largo (l), ancho (w) y alto (h), el Volumen = l × w × h. Equivale a calcular el área de la base y multiplicarla por la altura. En un cubo con aristas iguales (l), el Volumen = l³ = l × l × l.",
            CoreConceptSimplified = "Volumen = Largo × Ancho × Alto. Cuenta los cubos de la base y multiplícalos por los pisos de alto.",
            PrinciplesAddressed = new List<string>
            {
                "Comprensión del espacio tridimensional y unidades cúbicas (m³)",
                "Fórmula del volumen del prisma rectangular: Volumen = largo × ancho × alto",
                "Cálculo del volumen del cubo perfecto: Volumen = lado³"
            },
            CongratulationMessage = "¡Extraordinario arquitecto tridimensional! Ahora calculas el volumen y capacidad de cualquier prisma o cubo cósmico."
        };

        var ex1 = new Exercise
        {
            Id = "vol_ex_01",
            Title = "La Caja de Cubos Unitarios (Piso por Piso)",
            GameType = GamePlayType.GeometryVolume3D,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 24,
            StandardInstruction = "En una caja transparente se apilan cubos de 1 metro cúbico (1 m³). La base tiene 4 cubos de largo por 3 cubos de ancho (12 cubos por piso), y la caja tiene 2 pisos de altura. ¿Cuál es el volumen total de la caja?",
            SimplifiedInstruction = "Hay 12 cubos en la base y 2 pisos de alto. Multiplica 12 × 2.",
            ScreenReaderDescription = "Una caja con base de 4 por 3 cubos (12 cubos) apilada en 2 pisos de altura.",
            FirstHint = "Multiplica los cubos de la base (12) por la cantidad de pisos (2).",
            SecondHintDetailed = "12 × 2 = 24 cubos de 1 m³. El volumen total es 24 m³.",
            ExplanationOfSuccess = "¡Excelente! 4 de largo × 3 de ancho × 2 de alto = 24 metros cúbicos (24 m³).",
            SimplifiedFirstHint = "Multiplica 12 por 2.",
            SimplifiedSecondHint = "12 por 2 es 24.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 24 m³."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "vol_floor1", Name = "Piso 1 (12 cubos)", EmojiSymbol = "📦", AccessibleDescription = "Base inferior con 12 cubos", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "vol_floor2", Name = "Piso 2 (12 cubos)", EmojiSymbol = "📦", AccessibleDescription = "Segundo piso superior con 12 cubos", OrderIndex = 2, MusicalPitchHz = 392 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_v1", Label = "14 m³", Value = 14, IsCorrect = false, SpeechFeedback = "14 es sumar 12 más 2, pero para el volumen debemos multiplicar por los pisos." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_v2", Label = "18 m³", Value = 18, IsCorrect = false, SpeechFeedback = "18 no es el producto de 12 por 2." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_v3", Label = "24 m³", Value = 24, IsCorrect = true, SpeechFeedback = "¡Correcto! 12 cubos por 2 pisos son 24 metros cúbicos." });
        ex1.Options.Add(new ExerciseOption { Id = "opt_v4", Label = "36 m³", Value = 36, IsCorrect = false, SpeechFeedback = "36 sería si hubiera 3 pisos de cubos." });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "vol_ex_02",
            Title = "El Contenedor de Carga Espacial (5 × 4 × 3)",
            GameType = GamePlayType.GeometryVolume3D,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 60,
            StandardInstruction = "Un contenedor de carga mide 5 metros de largo, 4 metros de ancho y 3 metros de alto. Aplicando la fórmula Volumen = largo × ancho × alto (5 × 4 × 3), ¿cuál es su volumen total?",
            SimplifiedInstruction = "Multiplica 5 × 4 = 20, y luego 20 × 3. ¿Cuánto da el volumen?",
            ScreenReaderDescription = "Un prisma rectangular con dimensiones 5 metros de largo, 4 de ancho y 3 de alto.",
            FirstHint = "Multiplica las tres dimensiones: primero 5 × 4 = 20, y luego 20 × 3.",
            SecondHintDetailed = "20 × 3 = 60. El volumen total es 60 m³.",
            ExplanationOfSuccess = "¡Exacto! 5 × 4 × 3 = 60 metros cúbicos (60 m³).",
            SimplifiedFirstHint = "Multiplica 20 por 3.",
            SimplifiedSecondHint = "20 veces 3 es 60.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 60 m³."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "vol_prism_item", Name = "Contenedor Prisma", EmojiSymbol = "🚢", AccessibleDescription = "Contenedor rectangular de 5 por 4 por 3 metros", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_v2_1", Label = "20 m³", Value = 20, IsCorrect = false, SpeechFeedback = "20 m² es solo el área de la base (5 por 4), falta multiplicar por la altura (3)." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_v2_2", Label = "45 m³", Value = 45, IsCorrect = false, SpeechFeedback = "45 no es 20 por 3." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_v2_3", Label = "60 m³", Value = 60, IsCorrect = true, SpeechFeedback = "¡Correcto! 5 por 4 por 3 son 60 metros cúbicos." });
        ex2.Options.Add(new ExerciseOption { Id = "opt_v2_4", Label = "120 m³", Value = 120, IsCorrect = false, SpeechFeedback = "120 es el doble del volumen real." });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "vol_ex_03",
            Title = "El Cubo de Energía (3 × 3 × 3)",
            GameType = GamePlayType.GeometryVolume3D,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 27,
            StandardInstruction = "Un reactor cúbico tiene todos sus lados iguales que miden 3 metros. Aplicando la fórmula del cubo (Lado³ = 3 × 3 × 3), ¿cuál es su volumen total?",
            SimplifiedInstruction = "Multiplica 3 × 3 = 9, y luego 9 × 3. ¿Cuánto da?",
            ScreenReaderDescription = "Un cubo regular con aristas de 3 metros de largo, ancho y alto.",
            FirstHint = "Multiplica 3 por sí mismo tres veces: 3 × 3 × 3.",
            SecondHintDetailed = "3 × 3 = 9, y 9 × 3 = 27. El volumen del cubo es 27 m³.",
            ExplanationOfSuccess = "¡Brillante! 3 × 3 × 3 = 27 metros cúbicos (27 m³).",
            SimplifiedFirstHint = "Multiplica 9 por 3.",
            SimplifiedSecondHint = "9 por 3 es 27.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 27 m³."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "vol_cube_item", Name = "Cubo de Energía", EmojiSymbol = "🧊", AccessibleDescription = "Cubo perfecto de 3 por 3 por 3 metros", OrderIndex = 1, MusicalPitchHz = 440 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_v3_1", Label = "9 m³", Value = 9, IsCorrect = false, SpeechFeedback = "9 m² es el área de una sola cara (3 por 3), falta multiplicar por la profundidad." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_v3_2", Label = "18 m³", Value = 18, IsCorrect = false, SpeechFeedback = "18 no es 9 por 3." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_v3_3", Label = "27 m³", Value = 27, IsCorrect = true, SpeechFeedback = "¡Correcto! 3 por 3 por 3 son 27 metros cúbicos." });
        ex3.Options.Add(new ExerciseOption { Id = "opt_v3_4", Label = "81 m³", Value = 81, IsCorrect = false, SpeechFeedback = "81 sería 3 elevado a la cuatro." });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "vol_ex_04",
            Title = "El Estanque Espacial (Área Base × Altura)",
            GameType = GamePlayType.GeometryVolume3D,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 40,
            StandardInstruction = "Un estanque de agua para el huerto espacial tiene un área en su base de 10 metros cuadrados (10 m²) y una altura de 4 metros. ¿Cuál es su capacidad volumétrica (Área Base × Altura = 10 × 4)?",
            SimplifiedInstruction = "La base mide 10 y la altura es 4. Multiplica 10 × 4. ¿Cuál es el volumen?",
            ScreenReaderDescription = "Un estanque cilíndrico o prismático con base de 10 m² y altura de 4 metros.",
            FirstHint = "Multiplica el área de la base por la altura: 10 × 4.",
            SecondHintDetailed = "10 × 4 = 40. La capacidad del estanque es 40 m³.",
            ExplanationOfSuccess = "¡Extraordinario! 10 m² de base × 4 m de altura = 40 metros cúbicos (40 m³).",
            SimplifiedFirstHint = "Multiplica 10 por 4.",
            SimplifiedSecondHint = "10 por 4 es 40.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 40 m³."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "vol_tank_item", Name = "Estanque de Agua", EmojiSymbol = "💧", AccessibleDescription = "Estanque con 10 m² de base y 4 metros de profundidad", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_v4_1", Label = "14 m³", Value = 14, IsCorrect = false, SpeechFeedback = "14 es sumar 10 y 4, pero para el volumen debemos multiplicarlos." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_v4_2", Label = "24 m³", Value = 24, IsCorrect = false, SpeechFeedback = "24 no es el producto de 10 por 4." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_v4_3", Label = "40 m³", Value = 40, IsCorrect = true, SpeechFeedback = "¡Exacto! 10 por 4 son 40 metros cúbicos." });
        ex4.Options.Add(new ExerciseOption { Id = "opt_v4_4", Label = "100 m³", Value = 100, IsCorrect = false, SpeechFeedback = "100 sería si la altura fuera 10 metros." });
        topic.Exercises.Add(ex4);

        return topic;
    }


    private static PedagogicalTopic BuildMoneyCurrencyTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_money_currency_01",
            Title = "El Mercado del Reino: Monedas y Billetes",
            Level = AcademicLevel.EarlyElementary,
            CharacterName = "Nico en el Mercado Amigo",
            NarrativeIntro = "¡Bienvenidos al Mercado del Reino! Cuando queremos comprar una rica manzana o un juguete espacial, usamos monedas y billetes. Cada moneda tiene un valor: hay de $1, $2, $5 y $10 pesos. Para pagar, juntamos las monedas necesarias, y si entregamos más de la cuenta, ¡el comerciante nos devuelve el cambio!",
            NarrativeIntroSimplified = "¡Vamos a comprar al mercado! Juntamos monedas para pagar. Si pagas con una moneda más grande, te devuelven el cambio.",
            CoreConceptExplanation = "El dinero nos permite comprar cosas asignando un precio a cada objeto. Usamos monedas comunes ($1, $2, $5, $10) y billetes ($20, $50). Para comprar algo sumamos el valor de las monedas. Si pagamos con más dinero del necesario, el cambio se calcula restando: Cambio = Dinero Entregado - Precio.",
            CoreConceptSimplified = "Para comprar sumas tus monedas. Si das $10 para algo de $8, te devuelven $2 de cambio (10 menos 8 = 2).",
            PrinciplesAddressed = new List<string>
            {
                "Reconocimiento y suma de denominaciones monetarias ($1, $2, $5, $10)",
                "Composición aditiva para realizar pagos justos",
                "Cálculo del cambio mediante sustracción (Pago - Costo = Cambio)"
            },
            CongratulationMessage = "¡Magnífico comerciante! Sabes contar tu dinero, comprar y calcular el cambio exacto."
        };

        var ex1 = new Exercise
        {
            Id = "money_ex_01",
            Title = "Contando Monedas para la Fruta",
            GameType = GamePlayType.MoneyCurrencyCounting,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 13,
            StandardInstruction = "Nico tiene dos monedas de $5 y tres monedas de $1 en su bolsillo. ¿Cuánto dinero tiene en total en pesos?",
            SimplifiedInstruction = "Tienes dos monedas de 5 y tres de 1. Suma: 5 + 5 + 1 + 1 + 1. ¿Cuánto dinero es?",
            ScreenReaderDescription = "Dos monedas de 5 pesos y tres monedas de 1 peso en una bolsita.",
            FirstHint = "Primero suma las dos monedas de $5: 5 + 5 = 10.",
            SecondHintDetailed = "Luego suma las tres monedas de $1: 10 + 1 + 1 + 1 = 13 pesos.",
            ExplanationOfSuccess = "¡Correcto! 5 + 5 + 1 + 1 + 1 = 13 pesos en total.",
            SimplifiedFirstHint = "5 + 5 son 10.",
            SimplifiedSecondHint = "10 + 3 monedas de 1 son 13 pesos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Tienes 13 pesos."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "coin_5_1", Name = "Moneda de $5", EmojiSymbol = "🪙", AccessibleDescription = "Moneda dorada de 5 pesos", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "coin_5_2", Name = "Moneda de $5", EmojiSymbol = "🪙", AccessibleDescription = "Segunda moneda de 5 pesos", OrderIndex = 2, MusicalPitchHz = 330 });
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "coin_1_1", Name = "Moneda de $1", EmojiSymbol = "⚪", AccessibleDescription = "Moneda de 1 peso", OrderIndex = 3, MusicalPitchHz = 392 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m1", Label = "$10 pesos", Value = 10, IsCorrect = false, SpeechFeedback = "10 pesos solo serían las dos monedas de 5, faltan las de 1.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m2", Label = "$13 pesos", Value = 13, IsCorrect = true, SpeechFeedback = "¡Correcto! 5 más 5 son 10, más 3 de a peso son 13 pesos.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m3", Label = "$15 pesos", Value = 15, IsCorrect = false, SpeechFeedback = "15 pesos serían tres monedas de 5.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m4", Label = "$20 pesos", Value = 20, IsCorrect = false, SpeechFeedback = "20 pesos es demasiado.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "money_ex_02",
            Title = "Calculando el Cambio de la Manzana",
            GameType = GamePlayType.MoneyCurrencyCounting,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 2,
            StandardInstruction = "Compras una manzana que cuesta $8 pesos y pagas entregando una moneda de $10 pesos. ¿Cuánto dinero te deben devolver de cambio?",
            SimplifiedInstruction = "Cuesta $8 y pagas con $10. Resta 10 menos 8. ¿Cuánto te devuelven?",
            ScreenReaderDescription = "Una manzana con precio de 8 pesos y una moneda de pago de 10 pesos.",
            FirstHint = "Resta el precio del dinero que entregaste: 10 - 8.",
            SecondHintDetailed = "10 menos 8 es igual a 2. Te deben devolver $2 pesos de cambio.",
            ExplanationOfSuccess = "¡Excelente! 10 - 8 = 2 pesos de cambio.",
            SimplifiedFirstHint = "Resta 10 menos 8.",
            SimplifiedSecondHint = "10 menos 8 son 2 pesos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Te devuelven 2 pesos."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "apple_item", Name = "Manzana $8", EmojiSymbol = "🍎", AccessibleDescription = "Manzana de 8 pesos", OrderIndex = 1, MusicalPitchHz = 349 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_mc1", Label = "$1 peso", Value = 1, IsCorrect = false, SpeechFeedback = "1 peso no completa los 10 pesos.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_mc2", Label = "$2 pesos", Value = 2, IsCorrect = true, SpeechFeedback = "¡Correcto! 10 menos 8 son 2 pesos de cambio.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_mc3", Label = "$3 pesos", Value = 3, IsCorrect = false, SpeechFeedback = "8 más 3 serían 11.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_mc4", Label = "$5 pesos", Value = 5, IsCorrect = false, SpeechFeedback = "5 pesos sería si la manzana costara 5.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "money_ex_03",
            Title = "¿Cuánto Falta para el Juguete?",
            GameType = GamePlayType.MoneyCurrencyCounting,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 5,
            StandardInstruction = "Un carrito espacial cuesta $15 pesos. Tienes una moneda de $10 en tu alcancía. ¿Cuántos pesos te faltan para completar los $15?",
            SimplifiedInstruction = "El juguete cuesta 15 y tienes 10. Resta 15 menos 10. ¿Cuánto te falta?",
            ScreenReaderDescription = "Un carrito de juguete con etiqueta de 15 pesos y una moneda de 10 pesos.",
            FirstHint = "Resta lo que tienes de lo que cuesta el juguete: 15 - 10.",
            SecondHintDetailed = "15 menos 10 son 5. Te faltan 5 pesos.",
            ExplanationOfSuccess = "¡Brillante! 15 - 10 = 5 pesos para completar el carrito.",
            SimplifiedFirstHint = "Resta 15 menos 10.",
            SimplifiedSecondHint = "15 menos 10 son 5.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Te faltan 5 pesos."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "toy_car", Name = "Carrito $15", EmojiSymbol = "🚗", AccessibleDescription = "Carrito de juguete de 15 pesos", OrderIndex = 1, MusicalPitchHz = 440 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_mf1", Label = "$3 pesos", Value = 3, IsCorrect = false, SpeechFeedback = "10 más 3 son 13, todavía te falta.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_mf2", Label = "$5 pesos", Value = 5, IsCorrect = true, SpeechFeedback = "¡Correcto! 10 más 5 son 15 pesos exactos.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_mf3", Label = "$7 pesos", Value = 7, IsCorrect = false, SpeechFeedback = "10 más 7 serían 17.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_mf4", Label = "$10 pesos", Value = 10, IsCorrect = false, SpeechFeedback = "10 más 10 son 20.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "money_ex_04",
            Title = "El Billete de $20 en la Papelería",
            GameType = GamePlayType.MoneyCurrencyCounting,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 8,
            StandardInstruction = "Compras un cuaderno de dibujo de $12 pesos y pagas con un billete de $20 pesos. ¿Cuánto dinero recibes de cambio?",
            SimplifiedInstruction = "Cuesta $12 y pagas con un billete de $20. Resta 20 menos 12. ¿Cuánto te devuelven?",
            ScreenReaderDescription = "Un cuaderno de 12 pesos y un billete azul de 20 pesos.",
            FirstHint = "Resta 12 de 20: 20 - 12.",
            SecondHintDetailed = "20 menos 12 es igual a 8. Tu cambio son 8 pesos.",
            ExplanationOfSuccess = "¡Extraordinario! 20 - 12 = 8 pesos de cambio.",
            SimplifiedFirstHint = "Resta 20 menos 12.",
            SimplifiedSecondHint = "20 menos 12 da 8.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Tu cambio son 8 pesos."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "bill_20", Name = "Billete de $20", EmojiSymbol = "💵", AccessibleDescription = "Billete de 20 pesos", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_b20_1", Label = "$6 pesos", Value = 6, IsCorrect = false, SpeechFeedback = "12 más 6 son 18.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_b20_2", Label = "$8 pesos", Value = 8, IsCorrect = true, SpeechFeedback = "¡Correcto! 20 menos 12 son 8 pesos.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_b20_3", Label = "$10 pesos", Value = 10, IsCorrect = false, SpeechFeedback = "20 menos 10 serían 10.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_b20_4", Label = "$12 pesos", Value = 12, IsCorrect = false, SpeechFeedback = "12 pesos es el precio del cuaderno.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildMeasurementRulerTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_measurement_ruler_01",
            Title = "El Taller de Medición: Longitud y Regla",
            Level = AcademicLevel.EarlyElementary,
            CharacterName = "Lalo el Lagarto Carpintero",
            NarrativeIntro = "¡Bienvenidos al Taller de Carpintería! Para cortar listones de madera o construir mesas derechitas, no podemos adivinar: tenemos que medir con mucho cuidado. La regla graduada tiene marcas numeradas llamadas Centímetros (cm). El secreto de un gran carpintero es colocar siempre el inicio del objeto exactamente en el número CERO (0).",
            NarrativeIntroSimplified = "¡Vamos a medir con la regla! Pon la punta del lápiz en el 0. Mira hasta qué número llega: ese número son los centímetros.",
            CoreConceptExplanation = "La longitud mide qué tan largo es un objeto. En la escuela medimos con centímetros (cm) y metros (1 metro = 100 centímetros). Para medir con una regla, alineamos el inicio del objeto con la marca del 0 (cero). El número donde termina el objeto indica su medida exacta.",
            CoreConceptSimplified = "Medir es ver el largo. Pon la punta en el 0 de la regla. El número donde termina son los centímetros.",
            PrinciplesAddressed = new List<string>
            {
                "Alineación obligatoria del objeto con la marca 0 de la regla",
                "Lectura de longitudes en centímetros (cm)",
                "Relación de escala métrica: 1 metro = 100 centímetros"
            },
            CongratulationMessage = "¡Excelente maestro de la regla! Ahora mides con precisión cualquier objeto del taller."
        };

        var ex1 = new Exercise
        {
            Id = "ruler_ex_01",
            Title = "Midiendo el Lápiz Mágico",
            GameType = GamePlayType.MeasurementRulerLength,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 8,
            StandardInstruction = "Lalo coloca un lápiz amarillo sobre la regla. La punta trasera empieza en el número 0 y la punta delantera llega exactamente al número 8. ¿Cuántos centímetros (cm) mide el lápiz?",
            SimplifiedInstruction = "El lápiz empieza en el 0 y llega al 8. ¿Cuántos centímetros mide?",
            ScreenReaderDescription = "Un lápiz sobre una regla graduada en centímetros: empieza en el 0 y termina en el 8.",
            FirstHint = "El lápiz empieza en 0 y termina en 8. Su medida es el número donde termina.",
            SecondHintDetailed = "Como llega al número 8, mide exactamente 8 centímetros.",
            ExplanationOfSuccess = "¡Exacto! El lápiz mide 8 centímetros (8 cm).",
            SimplifiedFirstHint = "Mira el número donde termina la punta.",
            SimplifiedSecondHint = "Termina en el 8.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 8 cm."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "pencil_item", Name = "Lápiz de 8 cm", EmojiSymbol = "✏️", AccessibleDescription = "Lápiz alineado de 0 a 8 cm", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_r1", Label = "6 cm", Value = 6, IsCorrect = false, SpeechFeedback = "6 cm sería más corto.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_r2", Label = "8 cm", Value = 8, IsCorrect = true, SpeechFeedback = "¡Correcto! El lápiz mide 8 centímetros.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_r3", Label = "10 cm", Value = 10, IsCorrect = false, SpeechFeedback = "10 cm sería más largo.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_r4", Label = "12 cm", Value = 12, IsCorrect = false, SpeechFeedback = "12 cm es demasiado largo.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "ruler_ex_02",
            Title = "La Goma de Borrar Pequeña",
            GameType = GamePlayType.MeasurementRulerLength,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 4,
            StandardInstruction = "Colocas una goma de borrar sobre la regla. Empieza en el 0 y termina en la marca del 4. ¿Cuántos centímetros mide de largo?",
            SimplifiedInstruction = "La goma empieza en el 0 y termina en el 4. ¿Cuánto mide?",
            ScreenReaderDescription = "Una goma de borrar alineada de 0 a 4 cm sobre la regla.",
            FirstHint = "Revisa el número final donde termina la goma.",
            SecondHintDetailed = "La goma termina en la marca del 4, por lo que mide 4 cm.",
            ExplanationOfSuccess = "¡Excelente! La goma mide 4 centímetros.",
            SimplifiedFirstHint = "Mira hasta dónde llega.",
            SimplifiedSecondHint = "Llega al número 4.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 4 cm."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "eraser_item", Name = "Goma de 4 cm", EmojiSymbol = "🧼", AccessibleDescription = "Goma de borrar de 4 cm", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_er1", Label = "2 cm", Value = 2, IsCorrect = false, SpeechFeedback = "2 cm es la mitad.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_er2", Label = "4 cm", Value = 4, IsCorrect = true, SpeechFeedback = "¡Correcto! Mide 4 centímetros.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_er3", Label = "5 cm", Value = 5, IsCorrect = false, SpeechFeedback = "5 cm se pasa por un centímetro.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_er4", Label = "8 cm", Value = 8, IsCorrect = false, SpeechFeedback = "8 cm es el doble.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "ruler_ex_03",
            Title = "Comparando Dos Listones de Madera",
            GameType = GamePlayType.MeasurementRulerLength,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 3,
            StandardInstruction = "Un listón de madera rojo mide 10 cm y un listón azul mide 7 cm. ¿Cuántos centímetros más largo es el listón rojo que el azul (10 - 7)?",
            SimplifiedInstruction = "El rojo mide 10 y el azul mide 7. Resta 10 menos 7. ¿Por cuántos centímetros le gana?",
            ScreenReaderDescription = "Un listón rojo de 10 cm y un listón azul de 7 cm puestos uno junto al otro.",
            FirstHint = "Resta la medida del más corto a la del más largo: 10 - 7.",
            SecondHintDetailed = "10 menos 7 es igual a 3 cm de diferencia.",
            ExplanationOfSuccess = "¡Brillante! El listón rojo es 3 centímetros más largo que el azul.",
            SimplifiedFirstHint = "Resta 10 menos 7.",
            SimplifiedSecondHint = "10 menos 7 da 3.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Le gana por 3 cm."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "stick_red", Name = "Listón Rojo 10 cm", EmojiSymbol = "🟥", AccessibleDescription = "Listón rojo de 10 cm", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "stick_blue", Name = "Listón Azul 7 cm", EmojiSymbol = "🟦", AccessibleDescription = "Listón azul de 7 cm", OrderIndex = 2, MusicalPitchHz = 440 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_diff1", Label = "2 cm", Value = 2, IsCorrect = false, SpeechFeedback = "10 menos 7 no es 2.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_diff2", Label = "3 cm", Value = 3, IsCorrect = true, SpeechFeedback = "¡Exacto! 10 menos 7 son 3 centímetros.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_diff3", Label = "5 cm", Value = 5, IsCorrect = false, SpeechFeedback = "5 cm sería si el azul midiera 5.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_diff4", Label = "7 cm", Value = 7, IsCorrect = false, SpeechFeedback = "7 cm es el largo del azul.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "ruler_ex_04",
            Title = "¿Cuántos Centímetros Hay en un Metro?",
            GameType = GamePlayType.MeasurementRulerLength,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 100,
            StandardInstruction = "La regla del taller es larga y mide 1 metro completo. ¿Cuántos centímetros caben en 1 metro?",
            SimplifiedInstruction = "Un metro entero, ¿cuántos centímetros tiene?",
            ScreenReaderDescription = "Una cinta métrica de 1 metro completo dividida en 100 centímetros.",
            FirstHint = "La palabra centímetro empieza con 'centi', que viene de cien (100).",
            SecondHintDetailed = "En 1 metro caben exactamente 100 centímetros.",
            ExplanationOfSuccess = "¡Extraordinario! 1 metro equivale a exactamente 100 centímetros (1 m = 100 cm).",
            SimplifiedFirstHint = "Ciento... viene de cien.",
            SimplifiedSecondHint = "Son 100 centímetros.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! 1 metro son 100 cm."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "meter_tape", Name = "Metro de 100 cm", EmojiSymbol = "📏", AccessibleDescription = "Cinta métrica de 1 metro", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m100_1", Label = "10 cm", Value = 10, IsCorrect = false, SpeechFeedback = "10 cm es un decímetro, muy cortito.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m100_2", Label = "50 cm", Value = 50, IsCorrect = false, SpeechFeedback = "50 cm es solo medio metro.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m100_3", Label = "100 cm", Value = 100, IsCorrect = true, SpeechFeedback = "¡Correcto! Un metro tiene exactamente 100 centímetros.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m100_4", Label = "1000 cm", Value = 1000, IsCorrect = false, SpeechFeedback = "1000 cm serían 10 metros.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildTimeClockTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_time_clock_01",
            Title = "La Torre del Gran Reloj: Horas y Minutos",
            Level = AcademicLevel.EarlyElementary,
            CharacterName = "Tictac el Búho Cronometrador",
            NarrativeIntro = "¡Bienvenidos a la Torre del Gran Reloj! Para no llegar tarde a las misiones espaciales ni a la escuela, necesitamos aprender el secreto del tiempo. En los relojes de manecillas, la aguja corta y gruesa avanza despacito marcando la Hora, mientras que la aguja larga y delgada corre más rápido marcando los Minutos.",
            NarrativeIntroSimplified = "¡Mira las manecillas del reloj! La aguja corta dice la hora. La aguja larga dice los minutos. Una hora tiene 60 minutos.",
            CoreConceptExplanation = "Un reloj de manecillas tiene dos agujas principales: la horaria (corta) que señala la hora del 1 al 12, y el minutero (larga) que cuenta los minutos (la vuelta entera son 60 minutos, y cada número avanza 5 minutos). Cuando la aguja larga apunta al 12 es la hora en punto (:00). Cuando apunta al 6 es media hora (:30). Cuando apunta al 3 es un cuarto de hora (:15).",
            CoreConceptSimplified = "Manecilla corta = hora. Manecilla larga = minutos. Al 12 es en punto (:00). Al 6 es y media (:30). Al 3 es y cuarto (:15).",
            PrinciplesAddressed = new List<string>
            {
                "Distinción funcional: manecilla corta (horas) vs. manecilla larga (minutos)",
                "Lectura de hora en punto (:00), media hora (:30) y cuarto de hora (:15)",
                "Equivalencia temporal: 1 hora = 60 minutos, media hora = 30 minutos"
            },
            CongratulationMessage = "¡Fantástico cronometrador! Ahora sabes leer el reloj y manejar el tiempo como un verdadero astronauta."
        };

        var ex1 = new Exercise
        {
            Id = "clock_ex_01",
            Title = "La Hora en Punto del Despegue (3:00)",
            GameType = GamePlayType.TimeClockReading,
            TargetAgeMin = 7,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 3,
            StandardInstruction = "Tictac observa el reloj de la torre. La manecilla corta apunta exactamente al número 3 y la manecilla larga apunta al número 12 (:00 en punto). ¿Qué hora marca el reloj?",
            SimplifiedInstruction = "La aguja corta apunta al 3 y la larga al 12. ¿Qué hora es?",
            ScreenReaderDescription = "Un reloj de pared con manecilla corta en el 3 y minutero en el 12.",
            FirstHint = "La manecilla corta indica la hora. Mira qué número está señalando.",
            SecondHintDetailed = "La manecilla corta apunta al 3 y la larga al 12, por lo que son exactamente las 3 en punto.",
            ExplanationOfSuccess = "¡Exacto! Son las 3 en punto (3:00).",
            SimplifiedFirstHint = "Mira la aguja corta.",
            SimplifiedSecondHint = "Apunta al 3.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son las 3 en punto."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "clock_3pm", Name = "Reloj 3:00", EmojiSymbol = "🕒", AccessibleDescription = "Reloj marcando las 3 en punto", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_clk1", Label = "Las 12 en punto", Value = 12, IsCorrect = false, SpeechFeedback = "El 12 lo marca el minutero, pero la aguja corta dice la hora.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_clk2", Label = "Las 3 en punto", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! Son las 3 en punto.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_clk3", Label = "Las 6 en punto", Value = 6, IsCorrect = false, SpeechFeedback = "Las 6 sería abajo.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_clk4", Label = "Las 9 en punto", Value = 9, IsCorrect = false, SpeechFeedback = "Las 9 sería hacia la izquierda.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "clock_ex_02",
            Title = "Media Hora en el Espacio (:30)",
            GameType = GamePlayType.TimeClockReading,
            TargetAgeMin = 7,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 30,
            StandardInstruction = "La manecilla corta está entre el 4 y el 5, y el minutero largo apunta hacia abajo al número 6 (media vuelta). ¿Cuántos minutos han pasado después de las 4?",
            SimplifiedInstruction = "El minutero largo apunta al 6 (media hora). ¿Cuántos minutos son?",
            ScreenReaderDescription = "Un reloj con la aguja larga apuntando al número 6 en la parte inferior.",
            FirstHint = "Cada número en el reloj vale 5 minutos. El 6 es la mitad de 60.",
            SecondHintDetailed = "6 × 5 = 30 minutos. Son las 4 y media (4:30).",
            ExplanationOfSuccess = "¡Brillante! Apuntar al 6 indica 30 minutos (media hora).",
            SimplifiedFirstHint = "Media hora.",
            SimplifiedSecondHint = "Son 30 minutos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 30 minutos."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "clock_430", Name = "Reloj 4:30", EmojiSymbol = "🕟", AccessibleDescription = "Reloj marcando las 4 y media", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m30_1", Label = "15 minutos", Value = 15, IsCorrect = false, SpeechFeedback = "15 minutos es apuntar al 3.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m30_2", Label = "30 minutos", Value = 30, IsCorrect = true, SpeechFeedback = "¡Correcto! Apuntar al 6 son 30 minutos.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m30_3", Label = "45 minutos", Value = 45, IsCorrect = false, SpeechFeedback = "45 minutos es apuntar al 9.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m30_4", Label = "60 minutos", Value = 60, IsCorrect = false, SpeechFeedback = "60 minutos es la vuelta entera al 12.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "clock_ex_03",
            Title = "Los Minutos de una Hora Completa",
            GameType = GamePlayType.TimeClockReading,
            TargetAgeMin = 7,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 60,
            StandardInstruction = "El minutero largo dio una vuelta redonda completa a toda la esfera del reloj. ¿Cuántos minutos dura exactamente 1 hora completa?",
            SimplifiedInstruction = "Una hora entera, ¿cuántos minutos tiene?",
            ScreenReaderDescription = "La esfera de un reloj mostrando la vuelta completa de 60 minutos.",
            FirstHint = "Cuenta de 5 en 5 los doce números del reloj: 12 × 5.",
            SecondHintDetailed = "Una hora completa tiene exactamente 60 minutos.",
            ExplanationOfSuccess = "¡Extraordinario! 1 hora equivale a 60 minutos completos.",
            SimplifiedFirstHint = "Una hora entera.",
            SimplifiedSecondHint = "Son 60 minutos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! 1 hora = 60 minutos."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "clock_full", Name = "1 Hora = 60 min", EmojiSymbol = "⏰", AccessibleDescription = "Reloj de una hora completa", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_h60_1", Label = "24 minutos", Value = 24, IsCorrect = false, SpeechFeedback = "24 son las horas de un día, no los minutos de una hora.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_h60_2", Label = "30 minutos", Value = 30, IsCorrect = false, SpeechFeedback = "30 minutos es solo media hora.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_h60_3", Label = "60 minutos", Value = 60, IsCorrect = true, SpeechFeedback = "¡Correcto! Una hora tiene 60 minutos.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_h60_4", Label = "100 minutos", Value = 100, IsCorrect = false, SpeechFeedback = "Las horas no se dividen en 100, sino en 60.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "clock_ex_04",
            Title = "Un Cuarto de Hora (:15)",
            GameType = GamePlayType.TimeClockReading,
            TargetAgeMin = 7,
            TargetAgeMax = 9,
            Level = AcademicLevel.EarlyElementary,
            ExpectedResult = 15,
            StandardInstruction = "El minutero largo señala hacia la derecha el número 3 (un cuarto de vuelta del reloj). ¿Cuántos minutos representa el número 3?",
            SimplifiedInstruction = "El minutero apunta al 3. Cada número vale 5 minutos (5 + 5 + 5). ¿Cuántos minutos son?",
            ScreenReaderDescription = "Un reloj con la manecilla minutera señalando al número 3.",
            FirstHint = "Multiplica el número 3 por 5 minutos: 3 × 5.",
            SecondHintDetailed = "3 × 5 = 15 minutos. Un cuarto de hora son 15 minutos.",
            ExplanationOfSuccess = "¡Excelente! Al marcar el 3 son 15 minutos (un cuarto de hora).",
            SimplifiedFirstHint = "Multiplica 3 por 5.",
            SimplifiedSecondHint = "Son 15 minutos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 15 minutos."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "clock_15min", Name = "15 minutos", EmojiSymbol = "🕐", AccessibleDescription = "Reloj marcando 15 minutos", OrderIndex = 1, MusicalPitchHz = 440 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_q15_1", Label = "5 minutos", Value = 5, IsCorrect = false, SpeechFeedback = "5 minutos es apuntar al 1.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_q15_2", Label = "10 minutos", Value = 10, IsCorrect = false, SpeechFeedback = "10 minutos es apuntar al 2.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_q15_3", Label = "15 minutos", Value = 15, IsCorrect = true, SpeechFeedback = "¡Correcto! Apuntar al 3 son 15 minutos.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_q15_4", Label = "20 minutos", Value = 20, IsCorrect = false, SpeechFeedback = "20 minutos es apuntar al 4.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildPerimeterTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_geometry_perimeter_01",
            Title = "El Sendero de las Fronteras: Perímetro de Polígonos",
            Level = AcademicLevel.MiddleElementary,
            CharacterName = "Cosmo el Explorador de Senderos",
            NarrativeIntro = "¡Bienvenidos al Sendero de las Fronteras! Cuando construimos una cerca o caminamos alrededor de un parque, no estamos contando los cuadritos de adentro (área), sino la distancia que recorremos por el borde exterior: el Perímetro. El perímetro es la suma de todos los lados exteriores de una figura plana.",
            NarrativeIntroSimplified = "¡Vamos a caminar por la orilla! El perímetro es la cerca de afuera. Se calcula sumando la medida de todos los lados.",
            CoreConceptExplanation = "El perímetro (P) es la longitud total del contorno o frontera de una figura geométrica plana. Se calcula sumando las longitudes de todos sus lados: P = l₁ + l₂ + l₃ + ... Se expresa en unidades lineales simples (cm, m). En un rectángulo de base b y altura h, P = 2b + 2h. En un cuadrado de lado l, P = 4l.",
            CoreConceptSimplified = "Perímetro = suma de todos los lados exteriores. En un cuadrado sumas sus 4 lados iguales. En un rectángulo sumas los dos largos y los dos anchos.",
            PrinciplesAddressed = new List<string>
            {
                "Diferenciación conceptual: perímetro (contorno exterior lineal) vs. área (superficie interior cuadrada)",
                "Cálculo del perímetro en polígonos regulares e irregulares por adición de lados",
                "Fórmulas prácticas: Cuadrado P = 4l, Rectángulo P = 2(base + altura)"
            },
            CongratulationMessage = "¡Excelente guardián de las fronteras! Ahora puedes medir el contorno de cualquier territorio o polígono."
        };

        var ex1 = new Exercise
        {
            Id = "perim_ex_01",
            Title = "El Sendero Triangular (5 + 5 + 5)",
            GameType = GamePlayType.GeometryPerimeter,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 15,
            StandardInstruction = "Un sendero tiene forma de triángulo con 3 lados iguales que miden 5 metros cada uno. Si caminas alrededor de todo el sendero, ¿cuál es el perímetro total que recorriste?",
            SimplifiedInstruction = "Un triángulo con 3 lados de 5 metros. Suma 5 + 5 + 5. ¿Cuál es el perímetro?",
            ScreenReaderDescription = "Un triángulo equilátero con sus tres lados de 5 metros cada uno.",
            FirstHint = "Suma los tres lados del triángulo: 5 + 5 + 5.",
            SecondHintDetailed = "5 + 5 + 5 = 15 metros. El perímetro es 15 m.",
            ExplanationOfSuccess = "¡Exacto! 5 + 5 + 5 = 15 metros de perímetro.",
            SimplifiedFirstHint = "Suma 5 tres veces.",
            SimplifiedSecondHint = "5 + 5 + 5 son 15 metros.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El perímetro es 15 m."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "perim_tri_item", Name = "Sendero Triangular", EmojiSymbol = "🔺", AccessibleDescription = "Triángulo con 3 lados de 5 m", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_p1_1", Label = "10 m", Value = 10, IsCorrect = false, SpeechFeedback = "10 m solo serían 2 lados, falta sumar el tercer lado.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_p1_2", Label = "15 m", Value = 15, IsCorrect = true, SpeechFeedback = "¡Correcto! 5 más 5 más 5 son 15 metros.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_p1_3", Label = "20 m", Value = 20, IsCorrect = false, SpeechFeedback = "20 m sería si tuviera 4 lados.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_p1_4", Label = "25 m", Value = 25, IsCorrect = false, SpeechFeedback = "25 m es 5 por 5.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "perim_ex_02",
            Title = "El Perímetro del Terreno Rectangular (8 × 3)",
            GameType = GamePlayType.GeometryPerimeter,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 22,
            StandardInstruction = "Un terreno rectangular mide 8 metros de largo y 3 metros de ancho. Recuerda que tiene dos lados de 8 m y dos lados de 3 m. ¿Cuál es su perímetro exterior total (8 + 3 + 8 + 3)?",
            SimplifiedInstruction = "Suma los 4 lados del rectángulo: 8 + 3 + 8 + 3. ¿Cuánto da?",
            ScreenReaderDescription = "Un rectángulo con dos lados largos de 8 metros y dos lados cortos de 3 metros.",
            FirstHint = "Suma los 4 lados exteriores: 8 + 3 + 8 + 3.",
            SecondHintDetailed = "8 + 3 = 11, y 11 + 11 = 22 metros de perímetro.",
            ExplanationOfSuccess = "¡Excelente! 8 + 3 + 8 + 3 = 22 metros de contorno exterior.",
            SimplifiedFirstHint = "Suma 8 + 3 + 8 + 3.",
            SimplifiedSecondHint = "Da 22 metros.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El perímetro es 22 m."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "perim_rect_item", Name = "Terreno Rectangular", EmojiSymbol = "🟩", AccessibleDescription = "Rectángulo de 8 m por 3 m", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_p2_1", Label = "11 m", Value = 11, IsCorrect = false, SpeechFeedback = "11 m solo es un largo y un ancho, falta el otro lado.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_p2_2", Label = "22 m", Value = 22, IsCorrect = true, SpeechFeedback = "¡Correcto! 8 + 3 + 8 + 3 son 22 metros de cerca.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_p2_3", Label = "24 m", Value = 24, IsCorrect = false, SpeechFeedback = "24 m² sería el área (8 por 3), pero el perímetro es la suma de los lados.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_p2_4", Label = "32 m", Value = 32, IsCorrect = false, SpeechFeedback = "32 m sería 4 lados de 8.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "perim_ex_03",
            Title = "La Cerca del Jardín Cuadrado (6 m por lado)",
            GameType = GamePlayType.GeometryPerimeter,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 24,
            StandardInstruction = "Un jardín cuadrado tiene 4 lados iguales de 6 metros cada uno. ¿Cuántos metros de cerca de madera se necesitan para rodear todo su perímetro (6 × 4)?",
            SimplifiedInstruction = "Un cuadrado tiene 4 lados de 6 metros. Multiplica 6 × 4. ¿Cuál es el perímetro?",
            ScreenReaderDescription = "Un jardín cuadrado con los cuatro lados de 6 metros cada uno.",
            FirstHint = "Multiplica los 4 lados iguales por la medida de cada uno: 4 × 6.",
            SecondHintDetailed = "4 × 6 = 24 metros de cerca.",
            ExplanationOfSuccess = "¡Brillante! 4 lados × 6 m = 24 metros de perímetro.",
            SimplifiedFirstHint = "Multiplica 6 por 4.",
            SimplifiedSecondHint = "6 por 4 da 24 metros.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 24 m de perímetro."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "perim_sq_item", Name = "Jardín Cuadrado", EmojiSymbol = "🏡", AccessibleDescription = "Jardín cuadrado de 6 m de lado", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_p3_1", Label = "12 m", Value = 12, IsCorrect = false, SpeechFeedback = "12 m solo serían 2 lados de 6.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_p3_2", Label = "18 m", Value = 18, IsCorrect = false, SpeechFeedback = "18 m serían 3 lados.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_p3_3", Label = "24 m", Value = 24, IsCorrect = true, SpeechFeedback = "¡Correcto! 4 lados de 6 m son 24 metros.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_p3_4", Label = "36 m", Value = 36, IsCorrect = false, SpeechFeedback = "36 m² es el área (6 por 6), no el perímetro.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "perim_ex_04",
            Title = "El Pentágono de la Base Estelar (5 lados iguales)",
            GameType = GamePlayType.GeometryPerimeter,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 20,
            StandardInstruction = "La base estelar tiene forma de pentágono regular con 5 lados iguales que miden 4 metros cada uno. ¿Cuál es el perímetro total de la base (5 × 4)?",
            SimplifiedInstruction = "Una figura con 5 lados de 4 metros cada uno. Multiplica 5 × 4.",
            ScreenReaderDescription = "Un pentágono regular con cinco lados iguales de 4 metros cada uno.",
            FirstHint = "Multiplica el número de lados (5) por la longitud de cada lado (4): 5 × 4.",
            SecondHintDetailed = "5 × 4 = 20 metros de perímetro.",
            ExplanationOfSuccess = "¡Extraordinario! 5 lados × 4 m = 20 metros de perímetro total.",
            SimplifiedFirstHint = "Multiplica 5 por 4.",
            SimplifiedSecondHint = "5 por 4 da 20.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 20 m de perímetro."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "perim_penta_item", Name = "Base Pentagonal", EmojiSymbol = "⭐", AccessibleDescription = "Pentágono con 5 lados de 4 m", OrderIndex = 1, MusicalPitchHz = 440 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_p4_1", Label = "15 m", Value = 15, IsCorrect = false, SpeechFeedback = "15 m no es 5 por 4.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_p4_2", Label = "20 m", Value = 20, IsCorrect = true, SpeechFeedback = "¡Exacto! 5 lados por 4 metros son 20 metros de perímetro.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_p4_3", Label = "25 m", Value = 25, IsCorrect = false, SpeechFeedback = "25 m sería si cada lado midiera 5.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_p4_4", Label = "30 m", Value = 30, IsCorrect = false, SpeechFeedback = "30 m es demasiado grande.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }


    private static PedagogicalTopic BuildDivisionRemainderTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_division_remainder_01",
            Title = "El Banquete de los Sobrantes: División con Residuo",
            Level = AcademicLevel.MiddleElementary,
            CharacterName = "Cosmo el Repartidor Justo",
            NarrativeIntro = "¡Bienvenidos al Banquete de los Sobrantes! Cuando repartimos galletas o manzanas entre amigos, no siempre alcanza para que todos reciban lo mismo de forma exacta. Lo que no se puede repartir sin romperlo se llama Residuo o Sobrante. La regla de oro es: Dividendo = (Divisor × Cociente) + Residuo.",
            NarrativeIntroSimplified = "¡Vamos a repartir galletas! A veces sobra alguna porque no alcanza para todos. Lo que sobra se llama residuo.",
            CoreConceptExplanation = "La división con residuo (inexacta) ocurre cuando una cantidad no se puede dividir en partes enteras iguales sin que sobre nada. El número a repartir es el Dividendo (D), entre cuántos se reparte es el Divisor (d), lo que le toca a cada uno es el Cociente (q), y lo que sobra es el Residuo (r). El residuo siempre debe ser menor que el divisor: D = (d × q) + r.",
            CoreConceptSimplified = "Dividir con residuo: Si repartes 14 galletas entre 3 amigos, a cada uno le tocan 4 galletas (3 × 4 = 12) y sobran 2 galletas (residuo).",
            PrinciplesAddressed = new List<string>
            {
                "Concepto y significado del residuo en el reparto cotidiano",
                "Relación fundamental de la división: Dividendo = (Divisor × Cociente) + Residuo",
                "Condición de validez del residuo: siempre estrictamente menor que el divisor (r < d)"
            },
            CongratulationMessage = "¡Maestro del reparto justo! Ahora sabes calcular con exactitud cuánto le toca a cada uno y cuánto queda de residuo."
        };

        var ex1 = new Exercise
        {
            Id = "divrem_ex_01",
            Title = "Repartiendo 14 Galletas entre 3 Amigos",
            GameType = GamePlayType.DivisionWithRemainder,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 2,
            StandardInstruction = "Cosmo tiene 14 galletas y las reparte en partes iguales entre 3 amigos. A cada amigo le tocan 4 galletas (3 × 4 = 12). ¿Cuántas galletas sobran de residuo?",
            SimplifiedInstruction = "Tienes 14 galletas y das 4 a cada uno de 3 amigos (12 galletas). Resta 14 menos 12. ¿Cuántas sobran?",
            ScreenReaderDescription = "14 galletas en una bandeja para repartir a 3 amigos; 12 se entregan (4 a cada uno) y quedan galletas sobrantes.",
            FirstHint = "Multiplica lo que diste: 3 amigos × 4 galletas = 12.",
            SecondHintDetailed = "Resta 12 del total de 14: 14 - 12 = 2 galletas sobran de residuo.",
            ExplanationOfSuccess = "¡Correcto! A cada uno le tocan 4 galletas y sobran 2 galletas (residuo).",
            SimplifiedFirstHint = "Resta 14 menos 12.",
            SimplifiedSecondHint = "14 menos 12 son 2.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Sobran 2 galletas."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "cookie_rem_item", Name = "Galletas Sobrantes", EmojiSymbol = "🍪", AccessibleDescription = "Galletas que sobran tras el reparto", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr1", Label = "1 galleta", Value = 1, IsCorrect = false, SpeechFeedback = "12 más 1 son 13, pero teníamos 14.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr2", Label = "2 galletas", Value = 2, IsCorrect = true, SpeechFeedback = "¡Correcto! 14 - 12 = 2 galletas de residuo.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr3", Label = "3 galletas", Value = 3, IsCorrect = false, SpeechFeedback = "Si sobraran 3, ¡alcanzaría para darle una más a cada uno!", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr4", Label = "4 galletas", Value = 4, IsCorrect = false, SpeechFeedback = "4 galletas es lo que le tocó a cada amigo, no lo que sobró.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "divrem_ex_02",
            Title = "Reparto de 17 Manzanas entre 4 Cestas",
            GameType = GamePlayType.DivisionWithRemainder,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 1,
            StandardInstruction = "En el huerto hay 17 manzanas y se colocan equitativamente en 4 cestas. Caben 4 manzanas en cada cesta (4 × 4 = 16). ¿Cuántas manzanas quedan sueltas (residuo)?",
            SimplifiedInstruction = "17 manzanas repartidas en 4 cestas (4 en cada una = 16). Resta 17 menos 16.",
            ScreenReaderDescription = "4 cestas con 4 manzanas cada una (16 manzanas) y manzanas sueltas de un total de 17.",
            FirstHint = "4 cestas × 4 manzanas = 16 manzanas guardadas.",
            SecondHintDetailed = "Resta 16 de 17: 17 - 16 = 1 manzana sobrante.",
            ExplanationOfSuccess = "¡Exacto! 17 dividido entre 4 da 4 por cesta con residuo de 1 manzana.",
            SimplifiedFirstHint = "Resta 17 menos 16.",
            SimplifiedSecondHint = "17 menos 16 da 1.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Sobra 1 manzana."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "apple_rem_item", Name = "Manzana Suelta", EmojiSymbol = "🍎", AccessibleDescription = "Manzana que quedó sin cesta", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_1", Label = "1 manzana", Value = 1, IsCorrect = true, SpeechFeedback = "¡Correcto! Queda 1 manzana de residuo.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_2", Label = "2 manzanas", Value = 2, IsCorrect = false, SpeechFeedback = "16 más 2 serían 18.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_3", Label = "3 manzanas", Value = 3, IsCorrect = false, SpeechFeedback = "16 más 3 serían 19.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_4", Label = "4 manzanas", Value = 4, IsCorrect = false, SpeechFeedback = "Si sobraran 4 alcanzaría para meter una más en cada cesta.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "divrem_ex_03",
            Title = "El Cociente del Reparto (23 entre 5)",
            GameType = GamePlayType.DivisionWithRemainder,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 4,
            StandardInstruction = "Repartes 23 monedas de oro entre 5 exploradores espaciales de forma justa. ¿Cuántas monedas completas le tocan a cada explorador (cociente) sabiendo que 5 × 4 = 20 y sobran 3?",
            SimplifiedInstruction = "23 monedas entre 5 amigos. ¿Cuántas le tocan a cada uno? (5 por 4 es 20).",
            ScreenReaderDescription = "23 monedas repartidas entre 5 exploradores; sobran 3 monedas.",
            FirstHint = "Busca el número que multiplicado por 5 se acerque a 23 sin pasarse: 5 × 4 = 20.",
            SecondHintDetailed = "A cada uno le tocan 4 monedas completas y sobran 3 monedas.",
            ExplanationOfSuccess = "¡Brillante! El cociente es 4 (a cada uno le tocan 4 monedas).",
            SimplifiedFirstHint = "5 por 4 es 20.",
            SimplifiedSecondHint = "A cada amigo le tocan 4.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Le tocan 4 monedas."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "gold_coins_item", Name = "Monedas de Oro", EmojiSymbol = "🪙", AccessibleDescription = "Monedas repartidas equitativamente", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_1", Label = "3 monedas", Value = 3, IsCorrect = false, SpeechFeedback = "5 por 3 es 15, sobrarían 8 monedas (demasiadas).", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_2", Label = "4 monedas", Value = 4, IsCorrect = true, SpeechFeedback = "¡Correcto! Tocan 4 monedas completas a cada uno.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_3", Label = "5 monedas", Value = 5, IsCorrect = false, SpeechFeedback = "5 por 5 es 25, no alcanza porque solo tenemos 23.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_4", Label = "6 monedas", Value = 6, IsCorrect = false, SpeechFeedback = "5 por 6 es 30, no alcanza.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "divrem_ex_04",
            Title = "Comprobando con la Fórmula Mágica (D = d × q + r)",
            GameType = GamePlayType.DivisionWithRemainder,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 26,
            StandardInstruction = "Al dividir cierto número entre 6, el cociente fue 4 y sobraron 2 de residuo. Usando la fórmula Dividendo = (Divisor × Cociente) + Residuo: (6 × 4) + 2, ¿cuál era el número original?",
            SimplifiedInstruction = "Multiplica 6 × 4 = 24, y súmale el sobrante (+2). ¿Cuánto da?",
            ScreenReaderDescription = "Una comprobación de división: divisor 6, cociente 4, residuo 2.",
            FirstHint = "Primero multiplica el divisor por el cociente: 6 × 4 = 24.",
            SecondHintDetailed = "Luego suma el residuo: 24 + 2 = 26. El número original era 26.",
            ExplanationOfSuccess = "¡Extraordinario! (6 × 4) + 2 = 24 + 2 = 26.",
            SimplifiedFirstHint = "Suma 24 + 2.",
            SimplifiedSecondHint = "24 + 2 es 26.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 26."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "formula_item", Name = "Comprobación", EmojiSymbol = "✨", AccessibleDescription = "Fórmula mágica de la división", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_1", Label = "20", Value = 20, IsCorrect = false, SpeechFeedback = "20 no es (6 por 4) más 2.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_2", Label = "24", Value = 24, IsCorrect = false, SpeechFeedback = "24 es solo 6 por 4, falta sumar los 2 que sobraron.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_3", Label = "26", Value = 26, IsCorrect = true, SpeechFeedback = "¡Correcto! 24 más 2 son 26.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_4", Label = "30", Value = 30, IsCorrect = false, SpeechFeedback = "30 es 6 por 5.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildFractionsNumberLineTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_fractions_numberline_01",
            Title = "El Sendero de la Recta Numérica: Fracciones en el Camino",
            Level = AcademicLevel.MiddleElementary,
            CharacterName = "Maya la Exploradora de la Recta",
            NarrativeIntro = "¡Bienvenidos al Sendero de la Recta Numérica! Las fracciones no solo existen en rebanadas de pizza o pasteles: son números reales que tienen su propia casa en la recta numérica. Entre el número 0 y el número 1 caben infinidad de pasos fraccionarios. El denominador te dice en cuántas partes iguales cortar el camino, y el numerador cuántos pasos avanzar.",
            NarrativeIntroSimplified = "¡Vamos a caminar por la recta! Entre el 0 y el 1 cortamos el camino en partes iguales. La mitad (1/2) queda justo en el centro.",
            CoreConceptExplanation = "En la recta numérica, la distancia entre dos números enteros consecutivos (como de 0 a 1) representa la unidad. Para ubicar una fracción a/b: se divide el segmento unitario en 'b' partes de igual longitud (denominador), y desde el 0 se cuentan 'a' partes hacia la derecha (numerador). Si el numerador es menor que el denominador, la fracción propia queda entre 0 y 1. Si es mayor (impropia), supera el 1.",
            CoreConceptSimplified = "Para ubicar una fracción: El número de abajo corta el camino. El número de arriba dice cuántos pasos das desde el 0.",
            PrinciplesAddressed = new List<string>
            {
                "Representación geométrica de fracciones como puntos en la recta numérica",
                "División equitativa del segmento unitario según el denominador",
                "Comparación de magnitudes fraccionarias por posición espacial (más a la derecha = mayor)"
            },
            CongratulationMessage = "¡Excelente exploradora de la recta! Ahora ubicas y comparas cualquier fracción con precisión milimétrica."
        };

        var ex1 = new Exercise
        {
            Id = "fracline_ex_01",
            Title = "El Centro Exacto del Camino (1/2)",
            GameType = GamePlayType.FractionsNumberLine,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 1,
            StandardInstruction = "En una pista recta que va del kilómetro 0 al kilómetro 1, un robot se detiene exactamente a la mitad del trayecto. ¿Qué fracción representa el punto medio exacto de la recta?",
            SimplifiedInstruction = "¿Qué fracción está en medio del 0 y del 1?",
            ScreenReaderDescription = "Una recta numérica graduada de 0 a 1 con una marca justo en la mitad.",
            FirstHint = "El camino se cortó en 2 partes iguales y avanzó 1 parte.",
            SecondHintDetailed = "1 parte de 2 es la fracción un medio (1/2).",
            ExplanationOfSuccess = "¡Exacto! 1/2 se ubica justo en el punto medio entre 0 y 1.",
            SimplifiedFirstHint = "Es la mitad.",
            SimplifiedSecondHint = "Un medio (1/2).",
            SimplifiedExplanationOfSuccess = "¡Muy bien! 1/2 es la mitad."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "half_marker", Name = "Marca de 1/2", EmojiSymbol = "📍", AccessibleDescription = "Marca roja en la mitad entre 0 y 1", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fl1", Label = "1/2 (un medio)", Value = 1, IsCorrect = true, SpeechFeedback = "¡Correcto! 1/2 es exactamente la mitad.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fl2", Label = "1/4 (un cuarto)", Value = 2, IsCorrect = false, SpeechFeedback = "1/4 está más cerca del 0, es la mitad de la mitad.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fl3", Label = "3/4 (tres cuartos)", Value = 3, IsCorrect = false, SpeechFeedback = "3/4 está más cerca del 1.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fl4", Label = "2/1 (dos enteros)", Value = 4, IsCorrect = false, SpeechFeedback = "2/1 son dos enteros, está mucho más adelante.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "fracline_ex_02",
            Title = "Avanzando en Cuartos (3/4)",
            GameType = GamePlayType.FractionsNumberLine,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 3,
            StandardInstruction = "El segmento de 0 a 1 está dividido en 4 partes iguales de un cuarto cada una. Si das 3 saltos desde el 0, ¿a qué fracción llegas?",
            SimplifiedInstruction = "El camino tiene 4 pedacitos iguales. Das 3 saltos desde el cero. ¿Cuántos cuartos son?",
            ScreenReaderDescription = "Una recta dividida en 4 partes iguales: 1/4, 2/4, 3/4 y 1 entero.",
            FirstHint = "Cada salto equivale a 1/4. Tres saltos son 3 veces 1/4.",
            SecondHintDetailed = "Llegas a la marca de tres cuartos (3/4).",
            ExplanationOfSuccess = "¡Brillante! 3 saltos de un cuarto te colocan en 3/4.",
            SimplifiedFirstHint = "Son 3 saltos de a cuarto.",
            SimplifiedSecondHint = "Son 3/4.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Llegas a 3/4."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "three_fourths", Name = "Marca de 3/4", EmojiSymbol = "🚩", AccessibleDescription = "Bandera en la marca de tres cuartos", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fl2_1", Label = "1/4", Value = 1, IsCorrect = false, SpeechFeedback = "1/4 solo es un salto.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fl2_2", Label = "2/4 (la mitad)", Value = 2, IsCorrect = false, SpeechFeedback = "2/4 son solo dos saltos.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fl2_3", Label = "3/4 (tres cuartos)", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! Tres saltos de cuarto son 3/4.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fl2_4", Label = "4/4 (el entero)", Value = 4, IsCorrect = false, SpeechFeedback = "4/4 es el número 1 completo.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "fracline_ex_03",
            Title = "¿Quién Está Más Cerca del 1? (Comparación)",
            GameType = GamePlayType.FractionsNumberLine,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 1,
            StandardInstruction = "En la recta numérica de 0 a 1, comparamos dos fracciones: 1/4 y 3/4. ¿Cuál de las dos fracciones es mayor y está más a la derecha (más cerca del 1)?",
            SimplifiedInstruction = "¿Cuál es más grande y está más cerca del 1: 3/4 o 1/4?",
            ScreenReaderDescription = "Una recta con dos puntos marcados: 1/4 a la izquierda y 3/4 a la derecha cerca del 1.",
            FirstHint = "En la recta numérica, el número que está más a la derecha siempre es mayor.",
            SecondHintDetailed = "3/4 está más a la derecha que 1/4, por lo que 3/4 es mayor.",
            ExplanationOfSuccess = "¡Excelente! 3/4 es mayor que 1/4 porque está más cerca del 1.",
            SimplifiedFirstHint = "El que está más a la derecha es mayor.",
            SimplifiedSecondHint = "3/4 es más grande.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! 3/4 es mayor."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "compare_fractions", Name = "Comparación 3/4 vs 1/4", EmojiSymbol = "⚖️", AccessibleDescription = "Balanza de fracciones en la recta", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_cf1", Label = "3/4 es mayor", Value = 1, IsCorrect = true, SpeechFeedback = "¡Correcto! 3 cuartos es más grande que 1 cuarto.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_cf2", Label = "1/4 es mayor", Value = 2, IsCorrect = false, SpeechFeedback = "1/4 es más pequeño, está más cerca del 0.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_cf3", Label = "Son iguales", Value = 3, IsCorrect = false, SpeechFeedback = "No son iguales, 3/4 tiene más partes que 1/4.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_cf4", Label = "Ninguna", Value = 4, IsCorrect = false, SpeechFeedback = "Una siempre es mayor.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "fracline_ex_04",
            Title = "Cruzando el Entero (Fracción Impropia 5/4)",
            GameType = GamePlayType.FractionsNumberLine,
            TargetAgeMin = 9,
            TargetAgeMax = 12,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 5,
            StandardInstruction = "Si das 5 saltos de 1/4 desde el 0, pasas el número 1 (4/4) y llegas a 5/4. ¿A cuántos enteros y cuartos equivale 5/4?",
            SimplifiedInstruction = "5 cuartos es más de un entero (4 cuartos). ¿A qué equivale: 1 entero y 1 cuarto?",
            ScreenReaderDescription = "Una recta que pasa del número 1 y llega a la marca de 5/4 (un entero y un cuarto).",
            FirstHint = "4 cuartos (4/4) hacen un entero completo (1).",
            SecondHintDetailed = "5 cuartos es igual a 1 entero más 1 cuarto (1 1/4).",
            ExplanationOfSuccess = "¡Extraordinario! 5/4 = 1 entero y 1/4 (fracción impropia mayor que 1).",
            SimplifiedFirstHint = "4/4 es 1 entero.",
            SimplifiedSecondHint = "Sobra 1 cuarto: 1 entero y 1/4.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 1 entero y 1/4."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "improper_frac", Name = "Fracción Impropia 5/4", EmojiSymbol = "🚀", AccessibleDescription = "Cohete pasando la marca del 1", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_imp1", Label = "Menos de 1 entero", Value = 1, IsCorrect = false, SpeechFeedback = "5/4 supera el 4/4, por lo que es más de 1 entero.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_imp2", Label = "Exactamente 1 entero", Value = 4, IsCorrect = false, SpeechFeedback = "1 entero exacto es 4/4.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_imp3", Label = "1 entero y 1/4 (más de 1)", Value = 5, IsCorrect = true, SpeechFeedback = "¡Correcto! 5/4 es 1 entero y 1 cuarto.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_imp4", Label = "2 enteros completos", Value = 8, IsCorrect = false, SpeechFeedback = "2 enteros serían 8/4.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildDataBarChartsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_data_barcharts_01",
            Title = "El Observatorio de Datos: Tablas y Gráficas de Barras",
            Level = AcademicLevel.MiddleElementary,
            CharacterName = "Leo el Astrónomo Estadístico",
            NarrativeIntro = "¡Bienvenidos al Observatorio de Datos! Cuando los científicos exploran el universo o los estudiantes eligen su fruta favorita, recogen muchos datos sueltos. Para entenderlos con claridad y tomar buenas decisiones, los organizamos en Tablas de Frecuencia y los dibujamos en Gráficas de Barras: la barra más alta muestra lo más popular.",
            NarrativeIntroSimplified = "¡Mira la gráfica de barras! La barra más alta es la que tiene más cantidad. La más bajita es la que tiene menos.",
            CoreConceptExplanation = "Una tabla de datos organiza la información contando cuántas veces ocurre cada elemento (frecuencia). Una gráfica de barras representa estos datos visualmente mediante columnas rectangulares: el eje horizontal muestra las categorías y el eje vertical muestra la cantidad numérica (escala). La altura de cada barra es directamente proporcional a su frecuencia.",
            CoreConceptSimplified = "Gráfica de barras: Cada columna es una categoría. La altura de la barra te dice el número exacto.",
            PrinciplesAddressed = new List<string>
            {
                "Lectura e interpretación de tablas de frecuencia y conteo",
                "Asociación entre la altura de una barra y el valor numérico en el eje vertical",
                "Comparación de categorías: identificación de valor máximo, mínimo y diferencias"
            },
            CongratulationMessage = "¡Excelente analista de datos! Ahora sabes interpretar tablas y gráficas para descubrir información oculta."
        };

        var ex1 = new Exercise
        {
            Id = "barchart_ex_01",
            Title = "La Fruta Más Popular en el Salón",
            GameType = GamePlayType.DataBarChartsAndTables,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 8,
            StandardInstruction = "En una encuesta sobre la fruta preferida: Manzanas = 5 votos, Plátanos = 8 votos, Naranjas = 3 votos. En la gráfica de barras, ¿cuál es la altura máxima alcanzada por los plátanos?",
            SimplifiedInstruction = "Los plátanos tuvieron 8 votos. ¿Hasta qué número llega su barra?",
            ScreenReaderDescription = "Una gráfica de barras: barra de Manzanas llega al 5, Plátanos llega al 8, Naranjas llega al 3.",
            FirstHint = "Busca la barra de los plátanos y lee el número en el eje vertical.",
            SecondHintDetailed = "La barra de plátanos tiene 8 votos, por lo que llega al número 8.",
            ExplanationOfSuccess = "¡Correcto! Los plátanos alcanzaron la altura máxima de 8 votos.",
            SimplifiedFirstHint = "Mira el número de los plátanos.",
            SimplifiedSecondHint = "Es el número 8.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Llega al 8."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "chart_bar_item", Name = "Barra de Plátanos", EmojiSymbol = "📊", AccessibleDescription = "Gráfica de barras con columna de plátanos en 8", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_bc1", Label = "3 votos", Value = 3, IsCorrect = false, SpeechFeedback = "3 votos fue la barra más baja (naranjas).", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_bc2", Label = "5 votos", Value = 5, IsCorrect = false, SpeechFeedback = "5 votos fue la barra de manzanas.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_bc3", Label = "8 votos", Value = 8, IsCorrect = true, SpeechFeedback = "¡Correcto! Los plátanos tuvieron 8 votos.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_bc4", Label = "16 votos", Value = 16, IsCorrect = false, SpeechFeedback = "16 votos es la suma de todas las frutas juntas.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "barchart_ex_02",
            Title = "Diferencia Entre la Mayor y Menor Categoría",
            GameType = GamePlayType.DataBarChartsAndTables,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 5,
            StandardInstruction = "La barra más alta tiene 8 votos (plátanos) y la más baja tiene 3 votos (naranjas). ¿Cuántos votos más tuvo la fruta más popular que la menos popular (8 - 3)?",
            SimplifiedInstruction = "Resta la barra alta (8) menos la barra baja (3). ¿Cuánto da 8 - 3?",
            ScreenReaderDescription = "Comparación de alturas: barra de 8 contra barra de 3.",
            FirstHint = "Resta la barra menor de la mayor: 8 - 3.",
            SecondHintDetailed = "8 menos 3 es igual a 5 votos de diferencia.",
            ExplanationOfSuccess = "¡Exacto! 8 - 3 = 5 votos de ventaja para la fruta más popular.",
            SimplifiedFirstHint = "Resta 8 menos 3.",
            SimplifiedSecondHint = "8 menos 3 da 5.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Le gana por 5 votos."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "diff_bars_item", Name = "Diferencia de Barras", EmojiSymbol = "📈", AccessibleDescription = "Diferencia de 5 unidades entre barras", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_bc2_1", Label = "3 votos", Value = 3, IsCorrect = false, SpeechFeedback = "3 votos es el valor de las naranjas.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_bc2_2", Label = "5 votos", Value = 5, IsCorrect = true, SpeechFeedback = "¡Correcto! 8 menos 3 son 5 votos.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_bc2_3", Label = "8 votos", Value = 8, IsCorrect = false, SpeechFeedback = "8 votos es el valor de los plátanos.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_bc2_4", Label = "11 votos", Value = 11, IsCorrect = false, SpeechFeedback = "11 es sumarlas (8 + 3).", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "barchart_ex_03",
            Title = "Total de Personas Encuestadas",
            GameType = GamePlayType.DataBarChartsAndTables,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 16,
            StandardInstruction = "En la tabla de datos registramos: 5 personas eligieron Manzana, 8 eligieron Plátano y 3 eligieron Naranja. ¿Cuántas personas fueron encuestadas en total (5 + 8 + 3)?",
            SimplifiedInstruction = "Suma todos los votos: 5 + 8 + 3. ¿Cuántas personas son en total?",
            ScreenReaderDescription = "Una tabla con tres filas de datos sumando 5, 8 y 3.",
            FirstHint = "Suma los tres números: primero 5 + 8 = 13, y luego 13 + 3.",
            SecondHintDetailed = "13 + 3 = 16 personas encuestadas en total.",
            ExplanationOfSuccess = "¡Brillante! 5 + 8 + 3 = 16 personas en la encuesta.",
            SimplifiedFirstHint = "Suma 5 + 8 + 3.",
            SimplifiedSecondHint = "Da 16 en total.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 16 personas."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "total_survey_item", Name = "Total Encuestados", EmojiSymbol = "📋", AccessibleDescription = "Tabla con el conteo total de 16 personas", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_bc3_1", Label = "12 personas", Value = 12, IsCorrect = false, SpeechFeedback = "12 es menor al total.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_bc3_2", Label = "14 personas", Value = 14, IsCorrect = false, SpeechFeedback = "Faltaron sumar 2 personas.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_bc3_3", Label = "16 personas", Value = 16, IsCorrect = true, SpeechFeedback = "¡Correcto! 5 + 8 + 3 = 16 personas en total.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_bc3_4", Label = "20 personas", Value = 20, IsCorrect = false, SpeechFeedback = "20 es demasiado alto.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "barchart_ex_04",
            Title = "Leyendo la Categoría con Menor Frecuencia",
            GameType = GamePlayType.DataBarChartsAndTables,
            TargetAgeMin = 8,
            TargetAgeMax = 11,
            Level = AcademicLevel.MiddleElementary,
            ExpectedResult = 3,
            StandardInstruction = "En la gráfica de libros leídos: Cuentos = 7 libros, Ciencia = 9 libros, Poesía = 3 libros. ¿Cuántos libros leyó la categoría menos leída (la barra más baja)?",
            SimplifiedInstruction = "¿Cuál es el número de la barra más baja (poesía)?",
            ScreenReaderDescription = "Gráfica con barra de Cuentos en 7, Ciencia en 9, y Poesía en 3.",
            FirstHint = "Busca la barra de menor altura en la gráfica.",
            SecondHintDetailed = "La barra más baja corresponde a Poesía con 3 libros.",
            ExplanationOfSuccess = "¡Excelente! La categoría con menor frecuencia tiene 3 libros.",
            SimplifiedFirstHint = "Busca la barra más chiquita.",
            SimplifiedSecondHint = "Mide 3.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 3 libros."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "min_bar_item", Name = "Barra Más Baja", EmojiSymbol = "📉", AccessibleDescription = "Barra de poesía en el valor mínimo de 3", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_bc4_1", Label = "3 libros", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! 3 libros es la frecuencia mínima.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_bc4_2", Label = "7 libros", Value = 7, IsCorrect = false, SpeechFeedback = "7 es el valor medio (cuentos).", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_bc4_3", Label = "9 libros", Value = 9, IsCorrect = false, SpeechFeedback = "9 es la barra más alta (ciencia).", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_bc4_4", Label = "19 libros", Value = 19, IsCorrect = false, SpeechFeedback = "19 es la suma total.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildStatisticsCentralTendencyTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "elem_stats_central_tendency_01",
            Title = "El Centro de Mando: Media, Mediana y Moda",
            Level = AcademicLevel.UpperElementary,
            CharacterName = "Cosmo el Científico de Datos",
            NarrativeIntro = "¡Bienvenidos al Centro de Mando Estadístico! Cuando tenemos muchos números (como las calificaciones de un grupo o las temperaturas de una semana), necesitamos resumirlos en un solo valor representativo: el centro de los datos. Para eso usamos tres herramientas maestras: la Media (promedio), la Mediana (el dato del medio) y la Moda (el más repetido).",
            NarrativeIntroSimplified = "¡Vamos a resumir los números! La Moda es el número que más se repite. La Media (promedio) es sumar todo y dividir entre cuántos son.",
            CoreConceptExplanation = "Las medidas de tendencia central resumen un conjunto numérico: 1) Media aritmética (promedio): se suman todos los valores y se divide entre la cantidad total de datos (x̄ = Σx / n). 2) Moda: es el valor que aparece con mayor frecuencia en la muestra. 3) Mediana: es el valor que queda en la posición central cuando los datos se ordenan de menor a mayor.",
            CoreConceptSimplified = "Media = suma todos los números y divide entre cuántos son. Moda = el número que más veces se repite. Mediana = el número que queda en medio al formarlos en fila.",
            PrinciplesAddressed = new List<string>
            {
                "Cálculo e interpretación de la media aritmética (promedio)",
                "Identificación directa de la moda (valor con máxima frecuencia)",
                "Determinación de la mediana en conjuntos impares ordenados"
            },
            CongratulationMessage = "¡Extraordinario científico de datos! Ahora dominas la media, mediana y moda para analizar cualquier experimento del cosmos."
        };

        var ex1 = new Exercise
        {
            Id = "statscen_ex_01",
            Title = "Calculando el Promedio (Media Aritmética)",
            GameType = GamePlayType.StatisticsCentralTendency,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 8,
            StandardInstruction = "En 3 tareas espaciales obtuviste las calificaciones: 7, 8 y 9. Para calcular la Media (promedio), suma las 3 calificaciones (7 + 8 + 9 = 24) y divide entre 3. ¿Cuál es el promedio?",
            SimplifiedInstruction = "Suma 7 + 8 + 9 = 24. Luego divide 24 entre 3. ¿Cuánto da el promedio?",
            ScreenReaderDescription = "Tres calificaciones: 7, 8 y 9. La suma es 24 y se divide entre 3.",
            FirstHint = "Suma las tres notas: 7 + 8 + 9 = 24.",
            SecondHintDetailed = "Divide 24 entre 3: 24 / 3 = 8. El promedio es 8.",
            ExplanationOfSuccess = "¡Excelente! (7 + 8 + 9) / 3 = 24 / 3 = 8 de promedio.",
            SimplifiedFirstHint = "Divide 24 entre 3.",
            SimplifiedSecondHint = "24 entre 3 da 8.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El promedio es 8."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "mean_calc_item", Name = "Promedio de 8", EmojiSymbol = "🎯", AccessibleDescription = "Diana centrada en el promedio 8", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_sc1", Label = "7", Value = 7, IsCorrect = false, SpeechFeedback = "7 es la calificación más baja.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_sc2", Label = "8", Value = 8, IsCorrect = true, SpeechFeedback = "¡Correcto! 24 entre 3 da un promedio de 8.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_sc3", Label = "9", Value = 9, IsCorrect = false, SpeechFeedback = "9 es la calificación más alta.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_sc4", Label = "24", Value = 24, IsCorrect = false, SpeechFeedback = "24 es la suma total, falta dividir entre 3.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "statscen_ex_02",
            Title = "Encontrando la Moda (El Número Más Repetido)",
            GameType = GamePlayType.StatisticsCentralTendency,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 5,
            StandardInstruction = "En una carrera se registraron los tiempos: [3, 5, 5, 5, 8, 9]. La Moda es el número que aparece más veces en la lista. ¿Cuál es la moda?",
            SimplifiedInstruction = "En la lista [3, 5, 5, 5, 8, 9], ¿cuál es el número que más veces se repite?",
            ScreenReaderDescription = "Una lista con los números 3, 5, 5, 5, 8 y 9; el número 5 aparece tres veces.",
            FirstHint = "Cuenta cuántas veces aparece cada número. El 5 aparece 3 veces.",
            SecondHintDetailed = "Como el 5 es el que más se repite, la moda es 5.",
            ExplanationOfSuccess = "¡Brillante! El 5 aparece tres veces, por lo que es la moda de este conjunto.",
            SimplifiedFirstHint = "Mira el que se repite más.",
            SimplifiedSecondHint = "El 5 está 3 veces.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! La moda es 5."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "mode_item", Name = "Moda 5", EmojiSymbol = "⭐", AccessibleDescription = "Estrella destacando la moda 5", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_sc2_1", Label = "3", Value = 3, IsCorrect = false, SpeechFeedback = "El 3 solo aparece una vez.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_sc2_2", Label = "5 (se repite 3 veces)", Value = 5, IsCorrect = true, SpeechFeedback = "¡Correcto! El 5 es la moda porque es el más repetido.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_sc2_3", Label = "8", Value = 8, IsCorrect = false, SpeechFeedback = "El 8 solo aparece una vez.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_sc2_4", Label = "9", Value = 9, IsCorrect = false, SpeechFeedback = "El 9 solo aparece una vez.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "statscen_ex_03",
            Title = "Localizando la Mediana (El Dato del Centro)",
            GameType = GamePlayType.StatisticsCentralTendency,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 6,
            StandardInstruction = "Tenemos 5 números ya ordenados de menor a mayor: [2, 4, 6, 8, 10]. La Mediana es el número que queda exactamente en el medio de la fila. ¿Cuál es la mediana?",
            SimplifiedInstruction = "En la fila [2, 4, 6, 8, 10], ¿cuál número está justo en el centro?",
            ScreenReaderDescription = "Cinco números en fila: 2, 4, 6, 8 y 10. El 6 tiene dos números a la izquierda y dos a la derecha.",
            FirstHint = "Hay 5 números. El número del centro es la posición 3.",
            SecondHintDetailed = "El número 6 deja dos números a la izquierda (2 y 4) y dos a la derecha (8 y 10). La mediana es 6.",
            ExplanationOfSuccess = "¡Exacto! El 6 está exactamente en el centro, por lo que es la mediana.",
            SimplifiedFirstHint = "Busca el que está en medio.",
            SimplifiedSecondHint = "En medio está el 6.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! La mediana es 6."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "median_item", Name = "Mediana 6", EmojiSymbol = "⚖️", AccessibleDescription = "Balanza equilibrada en el valor central 6", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sc3_1", Label = "2", Value = 2, IsCorrect = false, SpeechFeedback = "2 es el primer número, el extremo izquierdo.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sc3_2", Label = "4", Value = 4, IsCorrect = false, SpeechFeedback = "4 no está en el centro.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sc3_3", Label = "6 (en el medio)", Value = 6, IsCorrect = true, SpeechFeedback = "¡Correcto! El 6 es la mediana central.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_sc3_4", Label = "10", Value = 10, IsCorrect = false, SpeechFeedback = "10 es el último número, el extremo derecho.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "statscen_ex_04",
            Title = "Promedio de Cuatro Temperaturas Espaciales",
            GameType = GamePlayType.StatisticsCentralTendency,
            TargetAgeMin = 10,
            TargetAgeMax = 13,
            Level = AcademicLevel.UpperElementary,
            ExpectedResult = 10,
            StandardInstruction = "Las temperaturas de 4 módulos espaciales son: 10°, 12°, 8° y 10°. Sumamos todas las temperaturas (10 + 12 + 8 + 10 = 40) y dividimos entre 4 módulos. ¿Cuál es la temperatura media?",
            SimplifiedInstruction = "Suma 10 + 12 + 8 + 10 = 40. Luego divide 40 entre 4. ¿Cuánto da?",
            ScreenReaderDescription = "Cuatro temperaturas sumando 40 grados para repartir entre 4 módulos.",
            FirstHint = "Divide la suma total (40) entre los 4 módulos: 40 / 4.",
            SecondHintDetailed = "40 dividido entre 4 es igual a 10 grados.",
            ExplanationOfSuccess = "¡Extraordinario! 40 / 4 = 10 grados de temperatura media.",
            SimplifiedFirstHint = "Divide 40 entre 4.",
            SimplifiedSecondHint = "40 entre 4 da 10.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 10 grados."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "temp_mean_item", Name = "Media 10°", EmojiSymbol = "🌡️", AccessibleDescription = "Termómetro marcando la media de 10 grados", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_sc4_1", Label = "8 grados", Value = 8, IsCorrect = false, SpeechFeedback = "8 es la temperatura más baja registrada.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_sc4_2", Label = "10 grados", Value = 10, IsCorrect = true, SpeechFeedback = "¡Correcto! 40 entre 4 son 10 grados de media.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_sc4_3", Label = "12 grados", Value = 12, IsCorrect = false, SpeechFeedback = "12 es la temperatura más alta.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_sc4_4", Label = "40 grados", Value = 40, IsCorrect = false, SpeechFeedback = "40 es la suma total, falta dividir entre 4.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }


    private static PedagogicalTopic BuildCircleGeometryTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_circle_geometry_01",
            Title = "El Templo del Círculo y Pi: Circunferencia, Radio y Diámetro",
            Level = AcademicLevel.SecondaryFirstGrade,
            CharacterName = "Arquímedes el Geómetra Cósmico",
            NarrativeIntro = "¡Bienvenidos al Templo del Círculo! La curva perfecta ha fascinado a la humanidad desde el principio de los tiempos. Todo círculo guarda una constante mágica e infinita llamada Pi (π ≈ 3.1416), que es la cantidad exacta de veces que el diámetro cabe alrededor de su contorno (circunferencia). Además, el radio es la distancia del centro al borde, y el diámetro cruza de lado a lado.",
            NarrativeIntroSimplified = "¡Vamos a medir círculos! El radio va del centro a la orilla. El diámetro cruza completo y mide el doble del radio.",
            CoreConceptExplanation = "Propiedades fundamentales del círculo: 1) Radio (r): segmento del centro a cualquier punto del perímetro. 2) Diámetro (d): cuerda que pasa por el centro, d = 2r. 3) Circunferencia (perímetro C): contorno del círculo, C = π × d = 2πr. 4) Área (A): superficie interior encerrada, A = π × r².",
            CoreConceptSimplified = "Círculo: El diámetro es el doble del radio (d = 2 × r). El perímetro del contorno se calcula multiplicando pi por el diámetro (C = π × d).",
            PrinciplesAddressed = new List<string>
            {
                "Relación geométrica fundamental entre radio y diámetro: d = 2r",
                "Cálculo de la circunferencia (perímetro del círculo) mediante C = π × d",
                "Comprensión de la constante pi (π ≈ 3.1416) como razón perimetral"
            },
            CongratulationMessage = "¡Maestro de las curvas circulares! Ahora dominas el radio, diámetro, pi y la circunferencia con precisión milimétrica."
        };

        var ex1 = new Exercise
        {
            Id = "circle_ex_01",
            Title = "Del Radio al Diámetro (d = 2r)",
            GameType = GamePlayType.CircleGeometryPi,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 12,
            StandardInstruction = "La rueda de una nave espacial tiene un radio de 6 metros desde el centro hasta la orilla. Sabiendo que el diámetro cruza completo de lado a lado pasando por el centro (d = 2 × r), ¿cuánto mide el diámetro?",
            SimplifiedInstruction = "El radio mide 6 metros. El diámetro es el doble (2 × 6). ¿Cuánto mide el diámetro?",
            ScreenReaderDescription = "Un círculo con una línea de radio de 6 metros y una línea de diámetro completa.",
            FirstHint = "El diámetro es el doble del radio: d = 2 × r.",
            SecondHintDetailed = "Multiplica 2 por 6: 2 × 6 = 12 metros de diámetro.",
            ExplanationOfSuccess = "¡Correcto! Diámetro = 2 × 6 m = 12 metros.",
            SimplifiedFirstHint = "Multiplica 2 por 6.",
            SimplifiedSecondHint = "2 por 6 es 12.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 12 metros."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "circle_rad_item", Name = "Radio de 6 m", EmojiSymbol = "⭕", AccessibleDescription = "Círculo con radio de 6 m", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_c1_1", Label = "6 m", Value = 6, IsCorrect = false, SpeechFeedback = "6 m es solo el radio, no el diámetro completo.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_c1_2", Label = "12 m", Value = 12, IsCorrect = true, SpeechFeedback = "¡Correcto! El diámetro es el doble: 2 por 6 son 12 m.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_c1_3", Label = "18 m", Value = 18, IsCorrect = false, SpeechFeedback = "18 m es el triple.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_c1_4", Label = "36 m", Value = 36, IsCorrect = false, SpeechFeedback = "36 m es el radio al cuadrado (6 por 6).", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "circle_ex_02",
            Title = "Calculando la Circunferencia con Pi (C = π × d)",
            GameType = GamePlayType.CircleGeometryPi,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 314,
            StandardInstruction = "Una rueda de precisión tiene un diámetro de 100 centímetros. Usando la constante π ≈ 3.14 en la fórmula de la circunferencia C = π × d, ¿cuántos centímetros mide todo el contorno de la rueda (3.14 × 100 cm)?",
            SimplifiedInstruction = "Multiplica 3.14 por 100 para medir el contorno de la rueda en centímetros. ¿Cuánto da?",
            ScreenReaderDescription = "Una rueda con diámetro de 100 cm y su perímetro iluminado.",
            FirstHint = "Multiplica pi por el diámetro en centímetros: 3.14 × 100.",
            SecondHintDetailed = "Al multiplicar por 100, el punto decimal se recorre dos lugares: 314 cm.",
            ExplanationOfSuccess = "¡Exacto! C = 3.14 × 100 = 314 centímetros de contorno.",
            SimplifiedFirstHint = "Mueve el punto dos lugares: 3.14 por 100.",
            SimplifiedSecondHint = "Da 314.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 314 centímetros."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "circle_circ_item", Name = "Circunferencia de 314 cm", EmojiSymbol = "🌀", AccessibleDescription = "Contorno del círculo de 314 cm", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_c2_1", Label = "200 cm", Value = 200, IsCorrect = false, SpeechFeedback = "200 cm es solo 2 por 100.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_c2_2", Label = "314 cm", Value = 314, IsCorrect = true, SpeechFeedback = "¡Correcto! 3.14 por 100 son 314 centímetros.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_c2_3", Label = "400 cm", Value = 400, IsCorrect = false, SpeechFeedback = "400 cm es demasiado grande.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_c2_4", Label = "3140 cm", Value = 3140, IsCorrect = false, SpeechFeedback = "3140 cm sería multiplicando por 1000.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "circle_ex_03",
            Title = "Del Diámetro al Radio (r = d / 2)",
            GameType = GamePlayType.CircleGeometryPi,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 8,
            StandardInstruction = "El domo de una estación lunar tiene un diámetro exterior de 16 metros. Sabiendo que el radio va del centro a la orilla y mide exactamente la mitad del diámetro (r = d / 2), ¿cuánto mide el radio?",
            SimplifiedInstruction = "El diámetro mide 16 metros. Divide 16 entre 2 para hallar el radio. ¿Cuánto da?",
            ScreenReaderDescription = "Un domo circular con diámetro de 16 metros; se pide la mitad (el radio).",
            FirstHint = "El radio es la mitad del diámetro: r = 16 / 2.",
            SecondHintDetailed = "16 dividido entre 2 es igual a 8 metros de radio.",
            ExplanationOfSuccess = "¡Brillante! Radio = 16 / 2 = 8 metros.",
            SimplifiedFirstHint = "La mitad de 16.",
            SimplifiedSecondHint = "16 entre 2 da 8.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El radio mide 8 metros."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "circle_half_item", Name = "Mitad del Domo", EmojiSymbol = "🪐", AccessibleDescription = "Radio de 8 metros", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c3_1", Label = "4 m", Value = 4, IsCorrect = false, SpeechFeedback = "4 m es la cuarta parte.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c3_2", Label = "8 m", Value = 8, IsCorrect = true, SpeechFeedback = "¡Correcto! La mitad de 16 metros son 8 metros.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c3_3", Label = "14 m", Value = 14, IsCorrect = false, SpeechFeedback = "14 m no es la mitad.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_c3_4", Label = "32 m", Value = 32, IsCorrect = false, SpeechFeedback = "32 m es el doble, no la mitad.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "circle_ex_04",
            Title = "Área del Círculo (A = π × r²)",
            GameType = GamePlayType.CircleGeometryPi,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 314,
            StandardInstruction = "Una compuerta redonda tiene un radio de 10 metros. Calculamos el área interior usando A = π × r² con π ≈ 3.14. Primero elevamos el radio al cuadrado (10² = 100) y multiplicamos: 3.14 × 100. ¿Cuál es el área en metros cuadrados?",
            SimplifiedInstruction = "Multiplica 3.14 por 100 (que es 10 × 10). ¿Cuánto da el área?",
            ScreenReaderDescription = "Una compuerta circular con radio de 10 metros y superficie iluminada.",
            FirstHint = "Calcula r² = 10 × 10 = 100.",
            SecondHintDetailed = "Multiplica pi por 100: 3.14 × 100 = 314 m².",
            ExplanationOfSuccess = "¡Extraordinario! A = 3.14 × 10² = 3.14 × 100 = 314 m².",
            SimplifiedFirstHint = "Multiplica 3.14 por 100.",
            SimplifiedSecondHint = "Da 314.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 314 m²."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "circle_area_item", Name = "Área Circular", EmojiSymbol = "🟡", AccessibleDescription = "Superficie de 314 m²", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_c4_1", Label = "200 m²", Value = 200, IsCorrect = false, SpeechFeedback = "200 no es 3.14 por 100.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_c4_2", Label = "314 m²", Value = 314, IsCorrect = true, SpeechFeedback = "¡Correcto! 3.14 por 100 = 314 metros cuadrados.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_c4_3", Label = "500 m²", Value = 500, IsCorrect = false, SpeechFeedback = "500 es demasiado grande.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_c4_4", Label = "1000 m²", Value = 1000, IsCorrect = false, SpeechFeedback = "1000 sería 10 al cubo.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildFactorsMcmMcdTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_factors_multiples_mcm_mcd_01",
            Title = "El Reloj de los Encuentros: MCM y MCD",
            Level = AcademicLevel.SecondaryFirstGrade,
            CharacterName = "Eratóstenes el Relojero de los Números",
            NarrativeIntro = "¡Bienvenidos al Reloj de los Encuentros! Dos faros espaciales parpadean a ritmos distintos: uno cada 4 minutos y otro cada 6 minutos. ¿En qué minuto exacto coincidirán ambos por primera vez? Para descubrirlo usamos el Mínimo Común Múltiplo (MCM). Y cuando queremos cortar tablones de distintas longitudes en pedazos iguales sin que sobre madera, ¡invocamos al Máximo Común Divisor (MCD)!",
            NarrativeIntroSimplified = "¡Vamos a encontrar coincidencias! El MCM te dice cuándo vuelven a coincidir dos cosas. El MCD te dice el trozo más grande en que puedes cortar sin que sobre nada.",
            CoreConceptExplanation = "1) Múltiplos y MCM: Los múltiplos de un número se obtienen multiplicándolo por los enteros naturales. El Mínimo Común Múltiplo (MCM) de dos números es el menor múltiplo positivo común a ambos (ideal para sincronización y periodos). 2) Divisores y MCD: Los divisores dividen exactamente a un número. El Máximo Común Divisor (MCD) es el mayor número entero que divide a ambos sin dejar residuo (ideal para particiones y simplificación).",
            CoreConceptSimplified = "MCM: Busca la primera vez que se cruzan las tablas de multiplicar. MCD: Busca el número más grande que divide a ambos en partes iguales.",
            PrinciplesAddressed = new List<string>
            {
                "Identificación de múltiplos y cálculo del Mínimo Común Múltiplo (MCM)",
                "Identificación de divisores y cálculo del Máximo Común Divisor (MCD)",
                "Resolución de problemas contextuales de periodos de coincidencia y partición óptima"
            },
            CongratulationMessage = "¡Maestro de las frecuencias y factores! Ahora resuelves sincronizaciones y repartos perfectos con MCM y MCD."
        };

        var ex1 = new Exercise
        {
            Id = "mcm_mcd_ex_01",
            Title = "Mínimo Común Múltiplo de 4 y 6 (MCM)",
            GameType = GamePlayType.FactorsMcmMcd,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 12,
            StandardInstruction = "Dos luces parpadean periódicamente: una cada 4 segundos (4, 8, 12, 16...) y otra cada 6 segundos (6, 12, 18...). ¿Cuál es el primer segundo en que ambas parpadean juntas (MCM de 4 y 6)?",
            SimplifiedInstruction = "La luz A parpadea cada 4 s. La luz B cada 6 s. ¿En qué número coinciden primero: 12?",
            ScreenReaderDescription = "Dos secuencias numéricas: múltiplos de 4 (4, 8, 12) y de 6 (6, 12) coincidiendo en 12.",
            FirstHint = "Escribe los múltiplos de 4: 4, 8, 12, 16, 20...",
            SecondHintDetailed = "Escribe los múltiplos de 6: 6, 12, 18... El menor común es 12.",
            ExplanationOfSuccess = "¡Exacto! El MCM(4, 6) = 12 segundos.",
            SimplifiedFirstHint = "Mira cuándo coinciden las tablas del 4 y del 6.",
            SimplifiedSecondHint = "4 × 3 = 12 y 6 × 2 = 12.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Coinciden en el 12."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "mcm_pulse_item", Name = "Coincidencia MCM", EmojiSymbol = "💡", AccessibleDescription = "Parpadeo conjunto a los 12 segundos", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m1_1", Label = "8 s", Value = 8, IsCorrect = false, SpeechFeedback = "8 es múltiplo de 4 pero no de 6.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m1_2", Label = "12 s", Value = 12, IsCorrect = true, SpeechFeedback = "¡Correcto! 12 es el menor múltiplo común a 4 y 6.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m1_3", Label = "16 s", Value = 16, IsCorrect = false, SpeechFeedback = "16 no se divide entre 6.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_m1_4", Label = "24 s", Value = 24, IsCorrect = false, SpeechFeedback = "24 es múltiplo común, ¡pero 12 es menor!", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "mcm_mcd_ex_02",
            Title = "Máximo Común Divisor de 12 y 18 (MCD)",
            GameType = GamePlayType.FactorsMcmMcd,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 6,
            StandardInstruction = "Queremos cortar dos listones de 12 cm y 18 cm en trozos iguales lo más largos posible sin que sobre nada. Los divisores de 12 son [1, 2, 3, 4, 6, 12] y de 18 son [1, 2, 3, 6, 9, 18]. ¿Cuál es el Máximo Común Divisor (MCD)?",
            SimplifiedInstruction = "¿Cuál es el número más grande que divide exactamente al 12 y al 18 sin que sobre nada?",
            ScreenReaderDescription = "Lista de divisores de 12 y 18 destacando el número 6 como el mayor común.",
            FirstHint = "Los divisores comunes a ambos son 1, 2, 3 y 6.",
            SecondHintDetailed = "El mayor de todos los divisores comunes es 6.",
            ExplanationOfSuccess = "¡Brillante! El MCD(12, 18) = 6 cm.",
            SimplifiedFirstHint = "El 6 divide al 12 (12/6=2) y al 18 (18/6=3).",
            SimplifiedSecondHint = "Es el número 6.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El trozo mide 6 cm."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "mcd_cut_item", Name = "Corte MCD de 6 cm", EmojiSymbol = "✂️", AccessibleDescription = "Trozos de 6 cm sin desperdicio", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m2_1", Label = "3 cm", Value = 3, IsCorrect = false, SpeechFeedback = "3 es divisor común, pero podemos cortar trozos más grandes.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m2_2", Label = "4 cm", Value = 4, IsCorrect = false, SpeechFeedback = "4 divide a 12 pero no a 18.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m2_3", Label = "6 cm", Value = 6, IsCorrect = true, SpeechFeedback = "¡Correcto! 6 es el divisor más grande común a 12 y 18.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_m2_4", Label = "9 cm", Value = 9, IsCorrect = false, SpeechFeedback = "9 divide a 18 pero no a 12.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "mcm_mcd_ex_03",
            Title = "Sincronización de Cohetes (MCM de 3 y 5)",
            GameType = GamePlayType.FactorsMcmMcd,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 15,
            StandardInstruction = "La nave Alfa despega cada 3 días y la nave Beta despega cada 5 días. Si hoy despegaron juntas, ¿dentro de cuántos días volverán a despegar exactamente el mismo día (MCM de 3 y 5)?",
            SimplifiedInstruction = "Alfa sale cada 3 días y Beta cada 5 días. Multiplica 3 × 5 para hallar el día en que vuelven a salir juntas.",
            ScreenReaderDescription = "Calendario con saltos de 3 días y saltos de 5 días coincidiendo en el día 15.",
            FirstHint = "Como 3 y 5 son números primos, su MCM es su producto directo: 3 × 5.",
            SecondHintDetailed = "3 × 5 = 15 días. Volverán a coincidir en 15 días.",
            ExplanationOfSuccess = "¡Excelente! Despegarán juntas cada 15 días.",
            SimplifiedFirstHint = "Multiplica 3 por 5.",
            SimplifiedSecondHint = "3 por 5 son 15.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 15 días."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "mcm_rocket_item", Name = "Despegue Simultáneo", EmojiSymbol = "🚀", AccessibleDescription = "Naves despegando juntas en el día 15", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_m3_1", Label = "8 días", Value = 8, IsCorrect = false, SpeechFeedback = "8 es solo sumarlos (3 + 5), no su múltiplo común.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_m3_2", Label = "10 días", Value = 10, IsCorrect = false, SpeechFeedback = "10 no es múltiplo de 3.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_m3_3", Label = "15 días", Value = 15, IsCorrect = true, SpeechFeedback = "¡Correcto! MCM(3, 5) = 15 días.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_m3_4", Label = "30 días", Value = 30, IsCorrect = false, SpeechFeedback = "30 es común, pero la primera coincidencia es a los 15.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "mcm_mcd_ex_04",
            Title = "Empacando Frascos de Experimentos (MCD de 20 y 30)",
            GameType = GamePlayType.FactorsMcmMcd,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 10,
            StandardInstruction = "Un laboratorio tiene 20 tubos de ensayo azules y 30 tubos rojos. Se quieren empaquetar en cajas con la misma cantidad máxima de tubos de cada color sin mezclar y sin que sobre ninguno. ¿Cuántos tubos van en cada caja (MCD de 20 y 30)?",
            SimplifiedInstruction = "¿Cuál es el número más grande que divide a 20 y a 30: 5 o 10?",
            ScreenReaderDescription = "Cajas idénticas organizando 20 tubos azules y 30 rojos en paquetes de 10.",
            FirstHint = "Los divisores comunes a 20 y 30 son 1, 2, 5 y 10.",
            SecondHintDetailed = "El máximo divisor común es 10 tubos por caja.",
            ExplanationOfSuccess = "¡Extraordinario! MCD(20, 30) = 10 tubos por caja.",
            SimplifiedFirstHint = "El 10 cabe en 20 (2 veces) y en 30 (3 veces).",
            SimplifiedSecondHint = "Son 10 tubos.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Caben 10 tubos en cada caja."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "mcd_box_item", Name = "Caja de 10 Tubos", EmojiSymbol = "🧪", AccessibleDescription = "Caja con 10 tubos de ensayo", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m4_1", Label = "5 tubos", Value = 5, IsCorrect = false, SpeechFeedback = "5 divide a ambos, pero 10 es más grande.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m4_2", Label = "10 tubos", Value = 10, IsCorrect = true, SpeechFeedback = "¡Correcto! 10 es el máximo divisor común.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m4_3", Label = "15 tubos", Value = 15, IsCorrect = false, SpeechFeedback = "15 no divide a 20.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_m4_4", Label = "20 tubos", Value = 20, IsCorrect = false, SpeechFeedback = "20 no divide a 30.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildNegativeRationalNumbersTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_negative_rational_numbers_01",
            Title = "El Sendero Bajo Cero: Números Racionales Negativos",
            Level = AcademicLevel.SecondaryFirstGrade,
            CharacterName = "Polaris el Explorador de los Glaciares",
            NarrativeIntro = "¡Bienvenidos al Sendero Bajo Cero! En el universo existen profundidades y temperaturas congelantes que caen por debajo del cero. Los números racionales (fracciones y decimales) también viven en el territorio negativo. En la recta numérica, cuanto más a la izquierda se encuentra un número negativo, ¡menor es su valor! Aprenderás a ubicar, comparar y operar con fracciones y decimales con signo.",
            NarrativeIntroSimplified = "¡Vamos bajo cero! A la izquierda del 0 están los números negativos con signo menos (-). Por ejemplo -1/2 o -50 centésimos.",
            CoreConceptExplanation = "El conjunto de los números racionales (Q) incluye a todas las razones a/b (b ≠ 0), tanto positivas como negativas. En la recta numérica: los negativos se ubican a la izquierda del origen (0). Si dos números son negativos, el que tiene mayor valor absoluto está más a la izquierda y es menor (-3/4 < -1/4). En la suma de racionales con signo se respetan las leyes de los signos algebraicos.",
            CoreConceptSimplified = "Racionales negativos: Son fracciones o decimales con signo menos. En la recta, el que está más a la izquierda vale menos.",
            PrinciplesAddressed = new List<string>
            {
                "Representación de fracciones y decimales negativos a la izquierda del cero en la recta numérica",
                "Comparación y ordenación de números racionales con signo",
                "Operaciones básicas con números racionales y aplicación de leyes de los signos"
            },
            CongratulationMessage = "¡Extraordinario explorador polar! Ahora dominas los números racionales negativos con total seguridad analítica."
        };

        var ex1 = new Exercise
        {
            Id = "negrat_ex_01",
            Title = "Ubicando Menos un Medio (-1/2) en la Recta",
            GameType = GamePlayType.NegativeRationalNumbers,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = -50,
            StandardInstruction = "En el mercado espacial medimos temperaturas en centésimos de grado bajo cero. La temperatura bajó a menos un medio de grado (-1/2). Sabiendo que 1 grado entero son 100 centésimos, ¿a cuántos centésimos bajo cero (-0.50) equivale la fracción -1/2?",
            SimplifiedInstruction = "¿A cuántos centésimos bajo cero equivale la mitad de 100 con signo menos: -50?",
            ScreenReaderDescription = "Una recta numérica marcando el punto medio entre 0 y -1 con la etiqueta -50 centésimos (-0.50).",
            FirstHint = "Divide 100 entre 2: 100 / 2 = 50 centésimos.",
            SecondHintDetailed = "Como está bajo cero, el valor es -50 centésimos (-0.50).",
            ExplanationOfSuccess = "¡Exacto! -1/2 equivale a -50 centésimos (-0.50) en la escala.",
            SimplifiedFirstHint = "La mitad de 100 es 50.",
            SimplifiedSecondHint = "Con signo menos es -50.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Equivale a -50 centésimos."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "neg_half_item", Name = "Marca de -50 centésimos", EmojiSymbol = "❄️", AccessibleDescription = "Cristal de hielo en la posición -50", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_nr1_1", Label = "-100 centésimos (-1.0)", Value = -100, IsCorrect = false, SpeechFeedback = "-100 centésimos es un entero completo negativo.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_nr1_2", Label = "-50 centésimos (-0.50)", Value = -50, IsCorrect = true, SpeechFeedback = "¡Correcto! -1/2 es igual a -50 centésimos (-0.50).", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_nr1_3", Label = "+50 centésimos (positivo)", Value = 50, IsCorrect = false, SpeechFeedback = "+50 positivo está a la derecha del cero.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_nr1_4", Label = "-25 centésimos (-0.25)", Value = -25, IsCorrect = false, SpeechFeedback = "-25 centésimos es un cuarto negativo.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "negrat_ex_02",
            Title = "Comparación: ¿Quién es Mayor, -1/4 o -3/4?",
            GameType = GamePlayType.NegativeRationalNumbers,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 1,
            StandardInstruction = "En el mundo bajo cero comparamos dos temperaturas: -1/4 °C (-0.25) y -3/4 °C (-0.75). En la recta numérica, el número que está más a la derecha (más cercano al cero) es el MAYOR. ¿Cuál temperatura es mayor?",
            SimplifiedInstruction = "Bajo cero, el número más cercano al cero es más caliente y mayor. ¿Cuál es mayor: -1/4 o -3/4?",
            ScreenReaderDescription = "Una recta numérica mostrando -0.75 a la izquierda y -0.25 a la derecha cerca del cero.",
            FirstHint = "El número que está más a la derecha en la recta siempre es el mayor.",
            SecondHintDetailed = "-0.25 (-1/4) está más cerca del cero que -0.75, por lo tanto -1/4 > -3/4.",
            ExplanationOfSuccess = "¡Brillante! -1/4 es mayor que -3/4 porque está más a la derecha en la recta.",
            SimplifiedFirstHint = "-1/4 está más cerca del 0.",
            SimplifiedSecondHint = "-1/4 es mayor.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! -1/4 es mayor."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "neg_comp_item", Name = "Termómetro Bajo Cero", EmojiSymbol = "🌡️", AccessibleDescription = "Comparación térmica bajo cero", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_nr2_1", Label = "-1/4 es mayor", Value = 1, IsCorrect = true, SpeechFeedback = "¡Correcto! -1/4 está más a la derecha, por lo que es mayor.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_nr2_2", Label = "-3/4 es mayor", Value = 2, IsCorrect = false, SpeechFeedback = "-3/4 está más a la izquierda en el frío extremo, es menor.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_nr2_3", Label = "Son iguales", Value = 3, IsCorrect = false, SpeechFeedback = "No son iguales, tienen distinta distancia al cero.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_nr2_4", Label = "Ninguno", Value = 4, IsCorrect = false, SpeechFeedback = "Siempre hay uno mayor.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "negrat_ex_03",
            Title = "Suma con Signos Opuestos: (-1/2) + (3/2)",
            GameType = GamePlayType.NegativeRationalNumbers,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 1,
            StandardInstruction = "Calcula la suma de fracciones con signos opuestos: (-1/2) + (3/2). Como tienen el mismo denominador (2), sumamos los numeradores (-1 + 3 = 2). La fracción resultante es 2/2. ¿A cuántos enteros equivale 2/2?",
            SimplifiedInstruction = "Debes 1 medio y pagas 3 medios. Te quedan 2 medios (2/2). ¿Cuántos enteros son 2/2?",
            ScreenReaderDescription = "Operación fraccionaria: (-1/2) + (3/2) = 2/2 = 1 entero.",
            FirstHint = "Suma los numeradores: -1 + 3 = 2.",
            SecondHintDetailed = "Divide numerador entre denominador: 2 / 2 = 1 entero positivo.",
            ExplanationOfSuccess = "¡Excelente! (-1/2) + (3/2) = 2/2 = 1 entero.",
            SimplifiedFirstHint = "2 entre 2 da 1.",
            SimplifiedSecondHint = "Es 1 entero.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Equivale a 1 entero."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "neg_sum_item", Name = "Equilibrio Térmico", EmojiSymbol = "⚖️", AccessibleDescription = "Balanza nivelada en 1 entero", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_nr3_1", Label = "-1", Value = -1, IsCorrect = false, SpeechFeedback = "-1 sería si el negativo fuera mayor.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_nr3_2", Label = "0", Value = 0, IsCorrect = false, SpeechFeedback = "No se cancelan por completo.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_nr3_3", Label = "1 entero", Value = 1, IsCorrect = true, SpeechFeedback = "¡Correcto! 2/2 es igual a 1 entero.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_nr3_4", Label = "2 enteros", Value = 2, IsCorrect = false, SpeechFeedback = "2 enteros serían 4/2.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "negrat_ex_04",
            Title = "Multiplicación de Signos: (-2) × (-5)",
            GameType = GamePlayType.NegativeRationalNumbers,
            TargetAgeMin = 11,
            TargetAgeMax = 14,
            Level = AcademicLevel.SecondaryFirstGrade,
            ExpectedResult = 10,
            StandardInstruction = "Aplica la ley de los signos: Menos por menos da más ((-) × (-) = (+)). ¿Cuál es el resultado de multiplicar (-2) × (-5)?",
            SimplifiedInstruction = "Menos por menos da más. Multiplica 2 × 5. ¿Cuánto da positivo?",
            ScreenReaderDescription = "Ecuación: (-2) × (-5) = +10 con los signos menos cancelándose.",
            FirstHint = "El signo final será positivo porque menos por menos da más.",
            SecondHintDetailed = "Multiplica los valores: 2 × 5 = 10 positivo.",
            ExplanationOfSuccess = "¡Extraordinario! (-2) × (-5) = +10.",
            SimplifiedFirstHint = "2 veces 5 da 10.",
            SimplifiedSecondHint = "Menos por menos es más (+10).",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El resultado es 10."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "neg_mul_item", Name = "Ley de Signos", EmojiSymbol = "✨", AccessibleDescription = "Signos negativos transformándose en positivo", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_nr4_1", Label = "-10", Value = -10, IsCorrect = false, SpeechFeedback = "Recuerda que menos por menos da más.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_nr4_2", Label = "+10 (positivo)", Value = 10, IsCorrect = true, SpeechFeedback = "¡Correcto! Menos por menos da más: (+10).", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_nr4_3", Label = "-7", Value = -7, IsCorrect = false, SpeechFeedback = "-7 es sumarlos (-2 + -5).", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_nr4_4", Label = "+7", Value = 7, IsCorrect = false, SpeechFeedback = "No se suman; se multiplican 2 × 5.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

private static PedagogicalTopic BuildAlgebraFactoringTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_algebra_factoring_01",
            Title = "El Taller de los Factores: Factorización Algebraica",
            Level = AcademicLevel.SecondarySecondGrade,
            CharacterName = "Al-Juarismi el Arquitecto de Ecuaciones",
            NarrativeIntro = "¡Bienvenidos al Taller de los Factores! Así como podemos desarmar una nave espacial en sus piezas modulares, en álgebra podemos convertir una suma de términos en una multiplicación equivalente de factores: esto es Factorizar. Si todos los términos comparten una pieza común, la extraemos afuera del paréntesis. Y si reconoces una resta de cuadrados como x² - 16, ¡se descompone mágicamente en binomios conjugados (x - 4)(x + 4)!",
            NarrativeIntroSimplified = "¡Vamos a desarmar expresiones! Factorizar es escribir una suma como una multiplicación entre paréntesis.",
            CoreConceptExplanation = "Métodos esenciales de factorización: 1) Factor común: se extrae el MCD numérico y la variable de menor exponente: ax + ay = a(x + y). 2) Diferencia de cuadrados: la resta de dos cuadrados perfectos se factoriza como producto de binomios conjugados: a² - b² = (a - b)(a + b). 3) Trinomio cuadrado perfecto: a² + 2ab + b² = (a + b)².",
            CoreConceptSimplified = "Factorizar: Saca el número que divide a todos fuera del paréntesis. Por ejemplo 3x + 6 = 3(x + 2).",
            PrinciplesAddressed = new List<string>
            {
                "Extracción del máximo factor común monomio",
                "Factorización de diferencias de cuadrados como binomios conjugados",
                "Identificación y factorización de trinomios cuadrados perfectos"
            },
            CongratulationMessage = "¡Gran maestro de la factorización! Ahora simplificas cualquier polinomio descomponiéndolo en factores fundamentales."
        };

        var ex1 = new Exercise
        {
            Id = "factor_ex_01",
            Title = "Factor Común Monomio: 3x + 6",
            GameType = GamePlayType.AlgebraFactoring,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 3,
            StandardInstruction = "En la expresión 3x + 6, ambos términos se pueden dividir exactamente entre el mismo número. ¿Cuál es el factor común numérico que se coloca fuera del paréntesis para multiplicar a (x + 2)?",
            SimplifiedInstruction = "¿Qué número divide al 3 y al 6 para ponerlo afuera del paréntesis?",
            ScreenReaderDescription = "Expresión algebraica 3x + 6 factorizada como factor común que multiplica a (x + 2).",
            FirstHint = "Busca el Máximo Común Divisor de 3 y 6.",
            SecondHintDetailed = "3 divide a 3 (da 1) y a 6 (da 2). El factor común es 3.",
            ExplanationOfSuccess = "¡Correcto! 3x + 6 = 3(x + 2). El factor común es 3.",
            SimplifiedFirstHint = "El 3 divide a ambos.",
            SimplifiedSecondHint = "Es el número 3.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El factor común es 3."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "fact_common_item", Name = "Factor 3", EmojiSymbol = "🧩", AccessibleDescription = "Pieza común de valor 3", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fc1_1", Label = "2", Value = 2, IsCorrect = false, SpeechFeedback = "2 no divide a 3.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fc1_2", Label = "3", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! 3 divide a 3x y a 6.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fc1_3", Label = "6", Value = 6, IsCorrect = false, SpeechFeedback = "6 no divide a 3.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_fc1_4", Label = "9", Value = 9, IsCorrect = false, SpeechFeedback = "9 es la suma (3 + 6).", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "factor_ex_02",
            Title = "Diferencia de Cuadrados: x² - 16",
            GameType = GamePlayType.AlgebraFactoring,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 4,
            StandardInstruction = "La expresión x² - 16 es una diferencia de cuadrados perfectos que se factoriza como (x - ?)(x + ?). Sabiendo que la raíz cuadrada de 16 es √16, ¿qué número completa los binomios conjugados?",
            SimplifiedInstruction = "Calcula la raíz cuadrada de 16 (¿qué número multiplicado por sí mismo da 16?).",
            ScreenReaderDescription = "Diferencia de cuadrados x² - 16 = (x - ?)(x + ?).",
            FirstHint = "Calcula la raíz cuadrada de 16: √16.",
            SecondHintDetailed = "4 × 4 = 16, por lo que x² - 16 = (x - 4)(x + 4).",
            ExplanationOfSuccess = "¡Exacto! x² - 16 = (x - 4)(x + 4). El término es 4.",
            SimplifiedFirstHint = "4 por 4 es 16.",
            SimplifiedSecondHint = "La raíz es 4.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Se completa con 4."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "diff_sq_item", Name = "Raíz 4", EmojiSymbol = "⚖️", AccessibleDescription = "Binomios conjugados con 4", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fc2_1", Label = "2", Value = 2, IsCorrect = false, SpeechFeedback = "2 al cuadrado es 4, no 16.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fc2_2", Label = "4", Value = 4, IsCorrect = true, SpeechFeedback = "¡Correcto! √16 = 4, da (x - 4)(x + 4).", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fc2_3", Label = "8", Value = 8, IsCorrect = false, SpeechFeedback = "8 es la mitad de 16, no su raíz cuadrada.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_fc2_4", Label = "16", Value = 16, IsCorrect = false, SpeechFeedback = "16 es el número completo al cuadrado.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "factor_ex_03",
            Title = "Término Interior del Paréntesis: 5x + 15 = 5(x + ?)",
            GameType = GamePlayType.AlgebraFactoring,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 3,
            StandardInstruction = "Al extraer el factor común 5 de la expresión 5x + 15, dividimos 15 entre 5 (15 / 5). ¿Qué número queda dentro del paréntesis en la forma 5(x + término faltante)?",
            SimplifiedInstruction = "Divide 15 entre 5. ¿Qué número queda dentro del paréntesis?",
            ScreenReaderDescription = "Expresión 5x + 15 factorizada extrayendo 5 para obtener 5 que multiplica a x más un término.",
            FirstHint = "Divide el segundo término entre el factor común: 15 / 5.",
            SecondHintDetailed = "15 / 5 = 3. La expresión factorizada es 5(x + 3).",
            ExplanationOfSuccess = "¡Brillante! 5x + 15 = 5(x + 3). Queda el 3.",
            SimplifiedFirstHint = "15 entre 5.",
            SimplifiedSecondHint = "15 entre 5 da 3.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Queda el 3."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "fact_inner_item", Name = "Término 3", EmojiSymbol = "📦", AccessibleDescription = "Contenido interior de valor 3", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_fc3_1", Label = "1", Value = 1, IsCorrect = false, SpeechFeedback = "5 por 1 daría 5, pero teníamos 15.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_fc3_2", Label = "3", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! 15 / 5 = 3.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_fc3_3", Label = "5", Value = 5, IsCorrect = false, SpeechFeedback = "5 por 5 daría 25.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_fc3_4", Label = "10", Value = 10, IsCorrect = false, SpeechFeedback = "10 es 15 menos 5.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "factor_ex_04",
            Title = "Trinomio Cuadrado Perfecto: x² + 6x + 9",
            GameType = GamePlayType.AlgebraFactoring,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 3,
            StandardInstruction = "El trinomio x² + 6x + 9 es un trinomio cuadrado perfecto que se factoriza como (x + a)². Sabiendo que el doble del término es 2 × a = 6 y su cuadrado es a² = 9, ¿qué número completa el binomio al cuadrado?",
            SimplifiedInstruction = "Busca un número que al multiplicarlo por sí mismo dé 9 (¿raíz de 9?).",
            ScreenReaderDescription = "Trinomio x² + 6x + 9 factorizado como un binomio al cuadrado.",
            FirstHint = "Calcula la raíz cuadrada del último término: √9.",
            SecondHintDetailed = "3 × 3 = 9 y 2 × 3 = 6. La factorización es (x + 3)².",
            ExplanationOfSuccess = "¡Extraordinario! x² + 6x + 9 = (x + 3)². El término es 3.",
            SimplifiedFirstHint = "3 por 3 da 9.",
            SimplifiedSecondHint = "La raíz es 3.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Se completa con 3."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "fact_trinom_item", Name = "Binomio Cuadrado", EmojiSymbol = "⭐", AccessibleDescription = "Binomio al cuadrado con 3", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_fc4_1", Label = "2", Value = 2, IsCorrect = false, SpeechFeedback = "2 al cuadrado es 4, no 9.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_fc4_2", Label = "3", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! (x + 3)² = x² + 6x + 9.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_fc4_3", Label = "6", Value = 6, IsCorrect = false, SpeechFeedback = "6 es el coeficiente del término lineal.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_fc4_4", Label = "9", Value = 9, IsCorrect = false, SpeechFeedback = "9 es el término independiente sin raíz.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildParallelLinesAnglesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_parallel_lines_angles_01",
            Title = "El Cruce de las Rutas Cósmicas: Paralelas y Secante",
            Level = AcademicLevel.SecondarySecondGrade,
            CharacterName = "Tales el Cartógrafo de las Estrellas",
            NarrativeIntro = "¡Bienvenidos al Cruce de las Rutas Cósmicas! En la navegación interestelar, dos rutas paralelas nunca se tocan, pero cuando una nave traza una ruta transversal (secante) que corta a ambas, ¡se produce una sinfonía de 8 ángulos relacionados! Los ángulos que están al revés pero dentro de las paralelas se llaman 'alternos internos' y son exactamente iguales entre sí. Los que se miran frente a frente en la misma esquina se llaman 'correspondientes'.",
            NarrativeIntroSimplified = "¡Mira el cruce de líneas! Dos líneas derechas cortadas por una línea inclinada forman ángulos gemelos que miden exactamente lo mismo.",
            CoreConceptExplanation = "Relaciones angulares entre dos rectas paralelas cortadas por una secante transversal: 1) Ángulos alternos internos: están a lados opuestos de la secante y dentro de las paralelas; son congruentes (iguales). 2) Ángulos alternos externos: a lados opuestos de la secante y fuera de las paralelas; congruentes. 3) Ángulos correspondientes: en la misma posición relativa en cada cruce; congruentes. 4) Ángulos conjugados (internos o externos): al mismo lado de la secante; son suplementarios (suman 180°).",
            CoreConceptSimplified = "Paralelas cortadas por secante: Los ángulos alternos (en zigzag) son iguales. Dos ángulos en la misma línea recta suman 180 grados.",
            PrinciplesAddressed = new List<string>
            {
                "Congruencia de ángulos alternos internos y alternos externos",
                "Identificación de ángulos correspondientes y opuestos por el vértice",
                "Cálculo de ángulos suplementarios conjugados que suman 180°"
            },
            CongratulationMessage = "¡Maestro de la geometría euclidiana! Ahora identificas y calculas cualquier ángulo en cruces de líneas con precisión milimétrica."
        };

        var ex1 = new Exercise
        {
            Id = "parall_ex_01",
            Title = "Ángulos Alternos Internos en Zigzag",
            GameType = GamePlayType.ParallelLinesAngles,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 65,
            StandardInstruction = "Dos vías espaciales paralelas son cortadas por una secante. El ángulo interno superior izquierdo mide 65°. Por el teorema de ángulos alternos internos, ¿cuánto mide el ángulo alterno interno en la parte inferior derecha?",
            SimplifiedInstruction = "Si un ángulo alterno interno mide 65 grados, ¿cuánto mide su ángulo gemelo alterno?",
            ScreenReaderDescription = "Dos líneas horizontales paralelas cortadas por una recta diagonal con un ángulo de 65 grados.",
            FirstHint = "Los ángulos alternos internos son exactamente iguales entre sí.",
            SecondHintDetailed = "Como son congruentes, el ángulo alterno mide exactamente 65°.",
            ExplanationOfSuccess = "¡Correcto! Los ángulos alternos internos son congruentes y miden 65°.",
            SimplifiedFirstHint = "Miden exactamente lo mismo.",
            SimplifiedSecondHint = "Mide 65 grados.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 65 grados."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "angle_65_item", Name = "Ángulo de 65°", EmojiSymbol = "📐", AccessibleDescription = "Ángulo alterno de 65 grados", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pl1_1", Label = "25°", Value = 25, IsCorrect = false, SpeechFeedback = "25° es el complemento (90 - 65).", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pl1_2", Label = "65°", Value = 65, IsCorrect = true, SpeechFeedback = "¡Correcto! Son gemelos: miden 65°.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pl1_3", Label = "115°", Value = 115, IsCorrect = false, SpeechFeedback = "115° es el ángulo suplementario que suma 180°.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pl1_4", Label = "130°", Value = 130, IsCorrect = false, SpeechFeedback = "130° es el doble (2 × 65).", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "parall_ex_02",
            Title = "Ángulos Conjugados Suplementarios (Suman 180°)",
            GameType = GamePlayType.ParallelLinesAngles,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 110,
            StandardInstruction = "En un cruce de paralelas, un ángulo interno mide 70°. El ángulo conjugado interno que está de su mismo lado es suplementario, lo que significa que ambos deben sumar 180° (70° + ? = 180°). ¿Cuánto mide el ángulo suplementario?",
            SimplifiedInstruction = "Resta 180 menos 70 para hallar el ángulo que falta. ¿Cuánto da?",
            ScreenReaderDescription = "Dos ángulos suplementarios sobre una recta sumando 180 grados: uno de 70 y otro desconocido.",
            FirstHint = "Resta el ángulo conocido de 180°: 180 - 70.",
            SecondHintDetailed = "180 - 70 = 110°. El ángulo mide 110 grados.",
            ExplanationOfSuccess = "¡Exacto! 70° + 110° = 180° de suma suplementaria.",
            SimplifiedFirstHint = "180 menos 70.",
            SimplifiedSecondHint = "Da 110 grados.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 110 grados."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "angle_110_item", Name = "Ángulo de 110°", EmojiSymbol = "🛡️", AccessibleDescription = "Ángulo suplementario de 110 grados", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pl2_1", Label = "70°", Value = 70, IsCorrect = false, SpeechFeedback = "70° es el ángulo agudo opuesto.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pl2_2", Label = "90°", Value = 90, IsCorrect = false, SpeechFeedback = "90° es un ángulo recto.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pl2_3", Label = "110°", Value = 110, IsCorrect = true, SpeechFeedback = "¡Correcto! 180 - 70 = 110 grados.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pl2_4", Label = "180°", Value = 180, IsCorrect = false, SpeechFeedback = "180° es la suma de los dos juntos.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "parall_ex_03",
            Title = "Ángulos Correspondientes en la Misma Posición",
            GameType = GamePlayType.ParallelLinesAngles,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 120,
            StandardInstruction = "En la intersección superior, el ángulo obtuso en la esquina superior derecha mide 120°. Por la propiedad de ángulos correspondientes (misma esquina en la intersección inferior), ¿cuánto mide el ángulo correspondiente?",
            SimplifiedInstruction = "El ángulo de la esquina superior mide 120 grados. El de la esquina correspondiente inferior mide lo mismo. ¿Cuánto mide?",
            ScreenReaderDescription = "Dos cruces idénticos con un ángulo correspondiente de 120 grados.",
            FirstHint = "Los ángulos correspondientes ocupan la misma posición en ambos cruces y son congruentes.",
            SecondHintDetailed = "Miden exactamente lo mismo: 120°.",
            ExplanationOfSuccess = "¡Brillante! Los ángulos correspondientes son iguales y miden 120°.",
            SimplifiedFirstHint = "Mide lo mismo.",
            SimplifiedSecondHint = "120 grados.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 120 grados."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "angle_120_item", Name = "Ángulo de 120°", EmojiSymbol = "✨", AccessibleDescription = "Ángulo correspondiente de 120 grados", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pl3_1", Label = "60°", Value = 60, IsCorrect = false, SpeechFeedback = "60° es el suplementario (180 - 120).", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pl3_2", Label = "120°", Value = 120, IsCorrect = true, SpeechFeedback = "¡Correcto! Los correspondientes son iguales: 120°.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pl3_3", Label = "140°", Value = 140, IsCorrect = false, SpeechFeedback = "140° no corresponde al cruce.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pl3_4", Label = "240°", Value = 240, IsCorrect = false, SpeechFeedback = "240° supera el ángulo llano.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "parall_ex_04",
            Title = "Ángulos Opuestos por el Vértice",
            GameType = GamePlayType.ParallelLinesAngles,
            TargetAgeMin = 12,
            TargetAgeMax = 15,
            Level = AcademicLevel.SecondarySecondGrade,
            ExpectedResult = 45,
            StandardInstruction = "Dos líneas se cruzan formando cuatro ángulos. Si uno de ellos mide 45°, el ángulo que está justo enfrente cruzando el punto central (opuesto por el vértice) mide exactamente lo mismo. ¿Cuánto mide el ángulo opuesto?",
            SimplifiedInstruction = "Los ángulos opuestos frente a frente miden lo mismo. Si uno mide 45 grados, ¿cuánto mide el de enfrente?",
            ScreenReaderDescription = "Dos rectas cruzadas como una X con dos ángulos opuestos por el vértice de 45 grados.",
            FirstHint = "Los ángulos opuestos por el vértice son siempre congruentes.",
            SecondHintDetailed = "Como miden lo mismo, el ángulo opuesto mide 45°.",
            ExplanationOfSuccess = "¡Extraordinario! Los ángulos opuestos por el vértice son congruentes: 45°.",
            SimplifiedFirstHint = "Mide lo mismo.",
            SimplifiedSecondHint = "45 grados.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 45 grados."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "angle_vert_item", Name = "Vértice 45°", EmojiSymbol = "✖️", AccessibleDescription = "Cruce en X de 45 grados", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pl4_1", Label = "45°", Value = 45, IsCorrect = true, SpeechFeedback = "¡Correcto! Los opuestos por el vértice miden 45°.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pl4_2", Label = "90°", Value = 90, IsCorrect = false, SpeechFeedback = "90° es perpendicular.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pl4_3", Label = "135°", Value = 135, IsCorrect = false, SpeechFeedback = "135° es el ángulo adyacente que suma 180°.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pl4_4", Label = "180°", Value = 180, IsCorrect = false, SpeechFeedback = "180° es la línea recta completa.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildProbabilityCompoundEventsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "sec_probability_compound_01",
            Title = "El Oráculo del Azar: Probabilidad Clásica y Compuesta",
            Level = AcademicLevel.SecondaryThirdGrade,
            CharacterName = "Laplace el Matemático del Futuro",
            NarrativeIntro = "¡Bienvenidos al Oráculo del Azar! Cuando tiramos un dado espacial, lanzamos una moneda o extraemos una gema de una urna, no sabemos con certeza qué saldrá, pero sí podemos predecir con exactitud matemática qué tan probable es que ocurra. Usando la Regla de Laplace: P = Casos Favorables / Casos Posibles Totales. Aprenderás a calcular probabilidades simples, porcentajes de azar y combinaciones compuestas.",
            NarrativeIntroSimplified = "¡Vamos a calcular probabilidades! La probabilidad es contar cuántos resultados te dan la victoria entre todos los resultados que pueden salir.",
            CoreConceptExplanation = "Fundamentos del cálculo de probabilidad: 1) Regla de Laplace: P(A) = número de casos favorables / número total de casos posibles en un espacio muestral equiprobable. 2) Escala de probabilidad: va de 0 (imposible, 0%) a 1 (seguro, 100%). 3) Eventos compuestos independientes: la probabilidad conjunta es el producto de sus probabilidades individuales: P(A y B) = P(A) × P(B).",
            CoreConceptSimplified = "Probabilidad = Casos que buscas dividido entre casos totales posibles. Por ejemplo, en un dado hay 3 números pares de 6 (3/6 = 50%).",
            PrinciplesAddressed = new List<string>
            {
                "Aplicación de la Regla de Laplace para eventos simples equiprobables",
                "Expresión de probabilidades en fracción, decimal y porcentaje",
                "Construcción del espacio muestral en eventos compuestos (monedas y dados)"
            },
            CongratulationMessage = "¡Gran maestro de la probabilidad y la estadística! Ahora calculas las leyes del azar con precisión científica."
        };

        var ex1 = new Exercise
        {
            Id = "prob_ex_01",
            Title = "Lanzando un Dado: Números Pares (3 de 6 = 50%)",
            GameType = GamePlayType.ProbabilityCompoundEvents,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 50,
            StandardInstruction = "Al lanzar un dado normal de 6 caras (1, 2, 3, 4, 5, 6), hay 3 caras que son números pares (2, 4 y 6). La probabilidad es 3/6 = 1/2. ¿A qué porcentaje de probabilidad equivale un medio (1/2 en %)?",
            SimplifiedInstruction = "La mitad de las caras del dado son pares (1 de cada 2). ¿Qué porcentaje es la mitad: 50%?",
            ScreenReaderDescription = "Un dado mostrando las caras pares 2, 4 y 6 de un total de 6.",
            FirstHint = "La fracción 1/2 multiplicada por 100 da el porcentaje: (1/2) × 100.",
            SecondHintDetailed = "100 / 2 = 50%. La probabilidad es del 50%.",
            ExplanationOfSuccess = "¡Correcto! 3/6 = 1/2 = 50% de probabilidad de obtener un número par.",
            SimplifiedFirstHint = "La mitad de 100 es 50.",
            SimplifiedSecondHint = "Es el 50%.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Equivale al 50%."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "prob_dice_item", Name = "Dado 50%", EmojiSymbol = "🎲", AccessibleDescription = "Dado con probabilidad del 50%", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pr1_1", Label = "25%", Value = 25, IsCorrect = false, SpeechFeedback = "25% es un cuarto.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pr1_2", Label = "33%", Value = 33, IsCorrect = false, SpeechFeedback = "33% es un tercio.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pr1_3", Label = "50%", Value = 50, IsCorrect = true, SpeechFeedback = "¡Correcto! 3 de 6 caras es el 50%.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pr1_4", Label = "100%", Value = 100, IsCorrect = false, SpeechFeedback = "100% sería si todas las caras fueran pares.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "prob_ex_02",
            Title = "La Urna de Gemas Espaciales (4 de 10 = 40%)",
            GameType = GamePlayType.ProbabilityCompoundEvents,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 40,
            StandardInstruction = "En una urna espacial hay 4 gemas rojas y 6 gemas azules (10 gemas en total). Si sacas una gema al azar sin mirar, ¿cuál es el porcentaje de probabilidad de que sea roja (4 / 10 en %)?",
            SimplifiedInstruction = "Hay 4 gemas rojas de 10 en total. Multiplica (4/10) × 100. ¿Qué porcentaje es: 40%?",
            ScreenReaderDescription = "Una urna con 4 gemas rojas y 6 azules sumando 10 gemas.",
            FirstHint = "Aplica la Regla de Laplace: Casos favorables (4) / Casos totales (10) = 4/10.",
            SecondHintDetailed = "(4/10) × 100 = 40% de probabilidad.",
            ExplanationOfSuccess = "¡Exacto! P(roja) = 4/10 = 40% de probabilidad.",
            SimplifiedFirstHint = "4 de cada 10 son rojas.",
            SimplifiedSecondHint = "Son el 40%.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Es el 40%."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "prob_gems_item", Name = "Urna de Gemas", EmojiSymbol = "🔮", AccessibleDescription = "Urna con 40% de probabilidad roja", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pr2_1", Label = "20%", Value = 20, IsCorrect = false, SpeechFeedback = "20% serían solo 2 gemas.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pr2_2", Label = "40%", Value = 40, IsCorrect = true, SpeechFeedback = "¡Correcto! 4 de 10 es el 40%.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pr2_3", Label = "60%", Value = 60, IsCorrect = false, SpeechFeedback = "60% es la probabilidad de sacar gema azul (6/10).", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pr2_4", Label = "80%", Value = 80, IsCorrect = false, SpeechFeedback = "80% es demasiado alto.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "prob_ex_03",
            Title = "Espacio Muestral: Lanzamiento de Dos Monedas",
            GameType = GamePlayType.ProbabilityCompoundEvents,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 4,
            StandardInstruction = "Al lanzar dos monedas al aire simultáneamente, los resultados posibles son: (Águila, Águila), (Águila, Sol), (Sol, Águila) y (Sol, Sol). ¿Cuántos resultados posibles forman el espacio muestral total (2 × 2)?",
            SimplifiedInstruction = "Lanzas dos monedas. Cada moneda tiene 2 caras (2 × 2). ¿Cuántas combinaciones posibles son?",
            ScreenReaderDescription = "Diagrama de árbol con 4 combinaciones posibles de dos monedas.",
            FirstHint = "Multiplica las opciones de la primera moneda (2) por las de la segunda (2): 2 × 2.",
            SecondHintDetailed = "2 × 2 = 4 resultados posibles en el espacio muestral.",
            ExplanationOfSuccess = "¡Brillante! El espacio muestral tiene 4 combinaciones posibles.",
            SimplifiedFirstHint = "Multiplica 2 por 2.",
            SimplifiedSecondHint = "Son 4 combinaciones.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Son 4 combinaciones."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "prob_coins_item", Name = "Dos Monedas", EmojiSymbol = "🪙", AccessibleDescription = "4 combinaciones de lanzamiento de monedas", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pr3_1", Label = "2 combinaciones", Value = 2, IsCorrect = false, SpeechFeedback = "2 es solo para una moneda.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pr3_2", Label = "4 combinaciones", Value = 4, IsCorrect = true, SpeechFeedback = "¡Correcto! Hay 4 resultados posibles (AA, AS, SA, SS).", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pr3_3", Label = "6 combinaciones", Value = 6, IsCorrect = false, SpeechFeedback = "6 no es potencia de 2.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pr3_4", Label = "8 combinaciones", Value = 8, IsCorrect = false, SpeechFeedback = "8 sería si lanzaras 3 monedas (2 al cubo).", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "prob_ex_04",
            Title = "Evento Imposible y Evento Seguro",
            GameType = GamePlayType.ProbabilityCompoundEvents,
            TargetAgeMin = 13,
            TargetAgeMax = 16,
            Level = AcademicLevel.SecondaryThirdGrade,
            ExpectedResult = 0,
            StandardInstruction = "En una bolsa sólo hay monedas de oro. La probabilidad de extraer una moneda de plata (un evento totalmente imposible) es numéricamente igual a: ¿qué porcentaje (0% o 100%)?",
            SimplifiedInstruction = "Si no hay ninguna moneda de plata, ¿cuál es el porcentaje de probabilidad de sacar una (0%)?",
            ScreenReaderDescription = "Bolsa sólo con monedas de oro; la probabilidad de sacar plata es imposible (cero).",
            FirstHint = "Un evento imposible tiene cero casos favorables.",
            SecondHintDetailed = "La probabilidad de un evento imposible es 0 (0%).",
            ExplanationOfSuccess = "¡Extraordinario! Los eventos imposibles tienen exactamente 0% de probabilidad.",
            SimplifiedFirstHint = "Es imposible.",
            SimplifiedSecondHint = "Mide 0%.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Es 0%."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "prob_zero_item", Name = "Probabilidad Cero", EmojiSymbol = "🚫", AccessibleDescription = "Evento imposible con 0%", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pr4_1", Label = "0% (imposible)", Value = 0, IsCorrect = true, SpeechFeedback = "¡Correcto! La probabilidad de lo imposible es 0%.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pr4_2", Label = "25%", Value = 25, IsCorrect = false, SpeechFeedback = "25% significaría que sí puede ocurrir.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pr4_3", Label = "50%", Value = 50, IsCorrect = false, SpeechFeedback = "50% es equiprobable.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pr4_4", Label = "100% (seguro)", Value = 100, IsCorrect = false, SpeechFeedback = "100% es para un evento seguro.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

private static PedagogicalTopic BuildConicsParabolaEllipseTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_conics_parabola_ellipse_01",
            Title = "El Observatorio de Antenas y Órbitas: Parábolas y Elipses",
            Level = AcademicLevel.HighSchoolSecondGrade,
            CharacterName = "Kepler el Navegante Planetario",
            NarrativeIntro = "¡Bienvenidos al Observatorio de Antenas y Órbitas! En el espacio profundo, los planetas no giran en círculos perfectos, sino en órbitas elípticas con el Sol en uno de sus focos. Y las antenas de comunicación concentran todas las señales en un punto exacto llamado foco gracias a la forma de la parábola. Aprenderás a dominar las ecuaciones canónicas de las cónicas analíticas: y² = 4px y x²/a² + y²/b² = 1.",
            NarrativeIntroSimplified = "¡Curvas del cosmos! Las antenas tienen forma de parábola para concentrar la señal en el foco. Los planetas viajan en elipses ovaladas.",
            CoreConceptExplanation = "Propiedades de las secciones cónicas: 1) Parábola con vértice en el origen: ecuación y² = 4px (horizontal) o x² = 4py (vertical), donde p es el parámetro o distancia focal del vértice al foco y a la directriz. 2) Elipse con centro en el origen: ecuación canónica x²/a² + y²/b² = 1, donde 'a' es la longitud del semieje mayor y 'b' es el semieje menor. La longitud total del eje mayor es 2a.",
            CoreConceptSimplified = "Parábola y² = 4px: divide el número entre 4 para hallar el foco p. Elipse x²/a² + y²/b² = 1: saca la raíz del número mayor para el semieje.",
            PrinciplesAddressed = new List<string>
            {
                "Ecuación canónica de la parábola con vértice en el origen: y² = 4px",
                "Cálculo del parámetro focal p y coordenadas del foco",
                "Ecuación canónica de la elipse y determinación de sus semiejes mayor y menor"
            },
            CongratulationMessage = "¡Maestro de la geometría analítica cónica! Ahora diseñas reflectores parabólicos y trazas órbitas elípticas interplanetarias."
        };

        var ex1 = new Exercise
        {
            Id = "conic_ex_01",
            Title = "El Foco de la Antena Parabólica (y² = 12x)",
            GameType = GamePlayType.ConicsParabolaEllipse,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = 3,
            StandardInstruction = "El reflector parabólico de una antena de comunicaciones espaciales sigue la ecuación y² = 12x. Sabiendo que la ecuación canónica es y² = 4px, donde 4p = 12, ¿cuál es la distancia focal p (12 / 4)?",
            SimplifiedInstruction = "Divide 12 entre 4 para hallar la distancia al foco. ¿Cuánto da 12 / 4?",
            ScreenReaderDescription = "Una parábola horizontal abierta a la derecha con ecuación y² = 12x y foco en p.",
            FirstHint = "Compara la ecuación con y² = 4px: 4p = 12.",
            SecondHintDetailed = "Divide 12 entre 4: p = 12 / 4 = 3 unidades.",
            ExplanationOfSuccess = "¡Correcto! 4p = 12 => p = 3. El foco receptor se coloca a 3 metros del vértice.",
            SimplifiedFirstHint = "Divide 12 entre 4.",
            SimplifiedSecondHint = "12 entre 4 da 3.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El foco está en 3."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "para_focus_item", Name = "Foco en p = 3", EmojiSymbol = "📡", AccessibleDescription = "Antena parabólica con foco en 3", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pe1_1", Label = "2 m", Value = 2, IsCorrect = false, SpeechFeedback = "4 por 2 daría 8.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pe1_2", Label = "3 m", Value = 3, IsCorrect = true, SpeechFeedback = "¡Correcto! 4p = 12 => p = 3.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pe1_3", Label = "4 m", Value = 4, IsCorrect = false, SpeechFeedback = "4 es el coeficiente divisor.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_pe1_4", Label = "6 m", Value = 6, IsCorrect = false, SpeechFeedback = "6 es la mitad de 12, pero se divide entre 4.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "conic_ex_02",
            Title = "Semieje Mayor de la Órbita Elíptica (x²/25 + y²/9 = 1)",
            GameType = GamePlayType.ConicsParabolaEllipse,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = 5,
            StandardInstruction = "La órbita de una sonda sigue la ecuación elíptica x²/25 + y²/9 = 1. Sabiendo que a² = 25 corresponde al semieje mayor horizontal, ¿cuánto mide el semieje mayor a (√25)?",
            SimplifiedInstruction = "Calcula la raíz cuadrada de 25 (el número que por sí mismo da 25).",
            ScreenReaderDescription = "Una elipse horizontal con a² = 25 y b² = 9.",
            FirstHint = "Calcula la raíz cuadrada del denominador mayor: √25.",
            SecondHintDetailed = "√25 = 5 unidades de longitud.",
            ExplanationOfSuccess = "¡Exacto! a² = 25 => a = 5 unidades de semieje mayor.",
            SimplifiedFirstHint = "5 por 5 es 25.",
            SimplifiedSecondHint = "La raíz es 5.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 5 unidades."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "elip_major_item", Name = "Semieje a = 5", EmojiSymbol = "🪐", AccessibleDescription = "Elipse con semieje mayor 5", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pe2_1", Label = "3", Value = 3, IsCorrect = false, SpeechFeedback = "3 es el semieje menor b (√9 = 3).", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pe2_2", Label = "5", Value = 5, IsCorrect = true, SpeechFeedback = "¡Correcto! √25 = 5.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pe2_3", Label = "12.5", Value = 12, IsCorrect = false, SpeechFeedback = "No se divide entre 2; se extrae raíz cuadrada.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_pe2_4", Label = "25", Value = 25, IsCorrect = false, SpeechFeedback = "25 es a al cuadrado, falta la raíz.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "conic_ex_03",
            Title = "Longitud Total del Eje Mayor (2a)",
            GameType = GamePlayType.ConicsParabolaEllipse,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = 10,
            StandardInstruction = "En la elipse de la sonda, el semieje mayor mide a = 5 metros. Sabiendo que el eje mayor completo cruza de un extremo al otro y mide el doble (2 × a), ¿cuál es su longitud total (2 × 5)?",
            SimplifiedInstruction = "Multiplica el semieje 5 por 2 para hallar el eje completo. ¿Cuánto da?",
            ScreenReaderDescription = "El eje mayor completo de la elipse midiendo el doble del semieje.",
            FirstHint = "Multiplica el semieje por 2: 2 × 5.",
            SecondHintDetailed = "2 × 5 = 10 metros de eje mayor.",
            ExplanationOfSuccess = "¡Brillante! Eje mayor = 2a = 2 × 5 = 10 metros.",
            SimplifiedFirstHint = "2 por 5.",
            SimplifiedSecondHint = "Da 10.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Mide 10 metros."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "elip_axis_item", Name = "Eje Mayor 10 m", EmojiSymbol = "🌌", AccessibleDescription = "Eje mayor completo de 10 metros", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pe3_1", Label = "5 m", Value = 5, IsCorrect = false, SpeechFeedback = "5 m es solo la mitad (el semieje).", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pe3_2", Label = "8 m", Value = 8, IsCorrect = false, SpeechFeedback = "8 no es 2 por 5.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pe3_3", Label = "10 m", Value = 10, IsCorrect = true, SpeechFeedback = "¡Correcto! El eje completo mide 2 × 5 = 10 metros.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_pe3_4", Label = "20 m", Value = 20, IsCorrect = false, SpeechFeedback = "20 m es el cuádruple.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "conic_ex_04",
            Title = "Parábola Vertical: Foco en el Eje Y (x² = 16y)",
            GameType = GamePlayType.ConicsParabolaEllipse,
            TargetAgeMin = 15,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolSecondGrade,
            ExpectedResult = 4,
            StandardInstruction = "Un telescopio espacial con reflector parabólico vertical tiene la ecuación x² = 16y. Con la forma x² = 4py, donde 4p = 16, ¿cuál es la coordenada y del foco receptor (16 / 4)?",
            SimplifiedInstruction = "Divide 16 entre 4 para hallar la coordenada del foco.",
            ScreenReaderDescription = "Parábola vertical abierta hacia arriba con foco sobre el eje y.",
            FirstHint = "Divide el coeficiente entre 4: 16 / 4.",
            SecondHintDetailed = "p = 16 / 4 = 4. El foco está en el punto (0, 4).",
            ExplanationOfSuccess = "¡Extraordinario! 4p = 16 => p = 4. El foco receptor se ubica en (0, 4).",
            SimplifiedFirstHint = "16 entre 4.",
            SimplifiedSecondHint = "Da 4.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! El foco está en 4."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "para_vert_item", Name = "Foco Vertical p = 4", EmojiSymbol = "🔭", AccessibleDescription = "Telescopio parabólico con foco en 4", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pe4_1", Label = "2", Value = 2, IsCorrect = false, SpeechFeedback = "4 por 2 daría 8.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pe4_2", Label = "4", Value = 4, IsCorrect = true, SpeechFeedback = "¡Correcto! 4p = 16 => p = 4.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pe4_3", Label = "8", Value = 8, IsCorrect = false, SpeechFeedback = "8 es la mitad de 16.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_pe4_4", Label = "16", Value = 16, IsCorrect = false, SpeechFeedback = "16 es 4p completo.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildCalculusLimitsTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_calculus_limits_01",
            Title = "La Puerta del Infinito: Límites y Continuidad",
            Level = AcademicLevel.HighSchoolThirdGrade,
            CharacterName = "Newton el Explorador del Infinito",
            NarrativeIntro = "¡Bienvenidos a la Puerta del Infinito! ¿Qué ocurre cuando nos acercamos infinitamente cerca a un punto sin llegar a tocarlo? Esta es la pregunta sublime que dio origen al Cálculo. El Límite describe el valor al que se aproxima una función cuando su variable x se acerca a un número 'a'. Si no hay saltos ni agujeros, sustituimos directamente. Y cuando aparece una trampa de 0/0, ¡el álgebra nos permite cancelarla y cruzar al otro lado!",
            NarrativeIntroSimplified = "¡Vamos a acercarnos al límite! Un límite es ver a qué número llega la gráfica cuando caminas muy cerca de un punto.",
            CoreConceptExplanation = "Concepto formal de límite y continuidad: 1) Límite: lim_{x -> a} f(x) = L si los valores de f(x) se aproximan a L cuando x se aproxima a 'a'. 2) Sustitución directa: si la función es continua en 'a', el límite es simplemente f(a). 3) Formas indeterminadas cancelables (0/0): al sustituir da 0/0; factorizamos el numerador o denominador para cancelar el factor que provoca la indeterminación (x - a) y luego re-evaluamos.",
            CoreConceptSimplified = "Límite: Reemplaza la 'x' por el número al que se acerca. Por ejemplo si x se acerca a 3 en 2x + 4: 2(3) + 4 = 10.",
            PrinciplesAddressed = new List<string>
            {
                "Evaluación de límites algebraicos por sustitución directa",
                "Resolución de indeterminaciones del tipo 0/0 mediante factorización algebraica",
                "Comprensión gráfica y analítica de la continuidad de funciones"
            },
            CongratulationMessage = "¡Gran maestro del cálculo infinitesimal! Has cruzado la puerta del infinito calculando límites con maestría analítica."
        };

        var ex1 = new Exercise
        {
            Id = "limit_ex_01",
            Title = "Límite por Sustitución Directa: lim_{x -> 3} (2x + 4)",
            GameType = GamePlayType.CalculusLimits,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 10,
            StandardInstruction = "Calcula el límite por sustitución directa cuando x tiende a 3: lim_{x -> 3} (2x + 4). Sustituyendo x = 3: 2(3) + 4. ¿Cuál es el valor del límite?",
            SimplifiedInstruction = "Cambia la x por el 3: multiplica 2 × 3 = 6, y suma 4 (+4). ¿Cuánto da?",
            ScreenReaderDescription = "Límite algebraico lim x tiende a 3 de la función 2x + 4.",
            FirstHint = "Sustituye x = 3 en la expresión: 2(3) + 4.",
            SecondHintDetailed = "2 × 3 = 6, y 6 + 4 = 10.",
            ExplanationOfSuccess = "¡Correcto! lim_{x -> 3} (2x + 4) = 2(3) + 4 = 10.",
            SimplifiedFirstHint = "Multiplica 2 por 3 y suma 4.",
            SimplifiedSecondHint = "6 + 4 = 10.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 10."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "limit_sub_item", Name = "Límite 10", EmojiSymbol = "🎯", AccessibleDescription = "Aproximación continua al valor 10", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_lim1_1", Label = "7", Value = 7, IsCorrect = false, SpeechFeedback = "7 es solo sumar 3 + 4 sin multiplicar por 2.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_lim1_2", Label = "10", Value = 10, IsCorrect = true, SpeechFeedback = "¡Correcto! 2(3) + 4 = 10.", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_lim1_3", Label = "14", Value = 14, IsCorrect = false, SpeechFeedback = "14 es demasiado alto.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_lim1_4", Label = "24", Value = 24, IsCorrect = false, SpeechFeedback = "24 no corresponde a la operación.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "limit_ex_02",
            Title = "Límite Cuadrático: lim_{x -> 2} (x² + 5)",
            GameType = GamePlayType.CalculusLimits,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 9,
            StandardInstruction = "Evalúa el límite cuadrático continuo cuando x se acerca a 2: lim_{x -> 2} (x² + 5). Elevando al cuadrado 2² = 4 y sumando 5 (4 + 5), ¿cuál es el resultado?",
            SimplifiedInstruction = "Eleva 2 al cuadrado (2 × 2 = 4) y súmale 5 (+5). ¿Cuánto da?",
            ScreenReaderDescription = "Límite de x cuadrada más 5 cuando x tiende a 2.",
            FirstHint = "Sustituye x = 2: 2² + 5.",
            SecondHintDetailed = "2² = 4, y 4 + 5 = 9.",
            ExplanationOfSuccess = "¡Exacto! lim_{x -> 2} (x² + 5) = 4 + 5 = 9.",
            SimplifiedFirstHint = "2 por 2 es 4. Suma 5.",
            SimplifiedSecondHint = "4 + 5 = 9.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 9."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "limit_quad_item", Name = "Límite 9", EmojiSymbol = "📈", AccessibleDescription = "Curva cuadrática alcanzando el valor 9", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_lim2_1", Label = "7", Value = 7, IsCorrect = false, SpeechFeedback = "7 sería 2 + 5 sin elevar al cuadrado.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_lim2_2", Label = "9", Value = 9, IsCorrect = true, SpeechFeedback = "¡Correcto! 2² + 5 = 4 + 5 = 9.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_lim2_3", Label = "11", Value = 11, IsCorrect = false, SpeechFeedback = "11 sería si 2 al cuadrado fuera 6.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_lim2_4", Label = "25", Value = 25, IsCorrect = false, SpeechFeedback = "25 es 5 al cuadrado.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "limit_ex_03",
            Title = "Cancelando la Indeterminación 0/0: lim_{x -> 4} (x² - 16) / (x - 4)",
            GameType = GamePlayType.CalculusLimits,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 8,
            StandardInstruction = "Al sustituir x = 4 directamente obtenemos 0/0. Factorizamos la diferencia de cuadrados: (x² - 16) = (x - 4)(x + 4). Cancelamos (x - 4) y nos queda lim_{x -> 4} (x + 4). Evaluando en x = 4 (4 + 4), ¿cuál es el verdadero límite?",
            SimplifiedInstruction = "Cancela la trampa y calcula 4 + 4. ¿Cuánto da el límite?",
            ScreenReaderDescription = "Límite indeterminado (x² - 16) entre (x - 4) factorizado como (x + 4) evaluado en 4.",
            FirstHint = "Factoriza: (x - 4)(x + 4) / (x - 4) = x + 4.",
            SecondHintDetailed = "Sustituye x = 4 en (x + 4): 4 + 4 = 8.",
            ExplanationOfSuccess = "¡Brillante! Cancelaste la indeterminación: lim_{x -> 4} (x + 4) = 4 + 4 = 8.",
            SimplifiedFirstHint = "Suma 4 + 4.",
            SimplifiedSecondHint = "4 + 4 son 8.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 8."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "limit_cancel_item", Name = "Límite Cancelado 8", EmojiSymbol = "✨", AccessibleDescription = "Indeterminación cancelada alcanzando el 8", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_lim3_1", Label = "0", Value = 0, IsCorrect = false, SpeechFeedback = "0/0 es indeterminado, no cero.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_lim3_2", Label = "4", Value = 4, IsCorrect = false, SpeechFeedback = "4 es solo el valor al que tiende x.", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_lim3_3", Label = "8", Value = 8, IsCorrect = true, SpeechFeedback = "¡Correcto! 4 + 4 = 8.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_lim3_4", Label = "16", Value = 16, IsCorrect = false, SpeechFeedback = "16 es el término cuadrático.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "limit_ex_04",
            Title = "Límite de una Constante: lim_{x -> 7} (15)",
            GameType = GamePlayType.CalculusLimits,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 15,
            StandardInstruction = "Por las propiedades de los límites, el límite de una función constante es igual a la constante misma (lim_{x -> a} c = c), ya que su valor no cambia sin importar a dónde tienda x. ¿Cuál es lim_{x -> 7} (15)?",
            SimplifiedInstruction = "Si la función siempre vale 15 fija y no tiene 'x', ¿cuánto vale su límite?",
            ScreenReaderDescription = "Límite de la constante 15 cuando x tiende a 7.",
            FirstHint = "El límite de un número constante siempre es ese mismo número.",
            SecondHintDetailed = "Como no hay 'x' que sustituir, el límite es exactamente 15.",
            ExplanationOfSuccess = "¡Extraordinario! lim_{x -> 7} (15) = 15. Una constante nunca cambia su valor.",
            SimplifiedFirstHint = "Es el mismo número 15.",
            SimplifiedSecondHint = "Vale 15.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Es 15."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "limit_const_item", Name = "Constante 15", EmojiSymbol = "🛡️", AccessibleDescription = "Línea horizontal constante en 15", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_lim4_1", Label = "0", Value = 0, IsCorrect = false, SpeechFeedback = "No es cero, la constante es 15.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_lim4_2", Label = "7", Value = 7, IsCorrect = false, SpeechFeedback = "7 es el punto al que se acerca x, no el valor de la función.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_lim4_3", Label = "15", Value = 15, IsCorrect = true, SpeechFeedback = "¡Correcto! El límite de una constante es la misma constante (15).", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_lim4_4", Label = "105", Value = 105, IsCorrect = false, SpeechFeedback = "105 sería multiplicarlos (7 × 15).", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }

    private static PedagogicalTopic BuildCalculusDerivativeRulesTopic()
    {
        var topic = new PedagogicalTopic
        {
            Id = "prep_calculus_derivative_rules_01",
            Title = "El Motor del Cambio: Reglas Operativas de Derivación",
            Level = AcademicLevel.HighSchoolThirdGrade,
            CharacterName = "Leibniz el Maestro de las Diferenciales",
            NarrativeIntro = "¡Bienvenidos al Motor del Cambio! Mientras que los límites nos abren la puerta al infinito, las Derivadas miden la velocidad instantánea exacta de cualquier fenómeno del universo. Para derivar funciones complejas sin tener que calcular el límite largo cada vez, usamos las Reglas de Derivación: la Regla de la Potencia baja el exponente multiplicando y le resta uno a la potencia (d/dx xⁿ = n·xⁿ⁻¹). ¡Es la herramienta maestra de la física y la ingeniería!",
            NarrativeIntroSimplified = "¡Vamos a derivar rápido! La regla de la potencia baja el exponente multiplicando y le resta 1. Por ejemplo la derivada de x³ es 3x².",
            CoreConceptExplanation = "Reglas fundamentales del cálculo diferencial: 1) Regla de la potencia: d/dx(xⁿ) = n·xⁿ⁻¹. 2) Derivada de una constante: d/dx(c) = 0 (lo estático no cambia). 3) Regla del múltiplo constante: d/dx(c·f(x)) = c·f'(x). 4) Regla de la suma: la derivada de una suma es la suma de las derivadas: d/dx(f + g) = f' + g'.",
            CoreConceptSimplified = "Regla de la potencia: multiplica por el exponente y bájale 1 a la potencia. La derivada de un número solo es 0.",
            PrinciplesAddressed = new List<string>
            {
                "Aplicación de la regla de la potencia para derivar monomios",
                "Derivada de funciones lineales y polinomios aplicando la regla de la suma",
                "Comprobación de que la derivada de una función constante es idénticamente cero"
            },
            CongratulationMessage = "¡Maestro de la diferenciación analítica! Ahora dominas las reglas de derivación que impulsan la ciencia aeroespacial y el cálculo superior."
        };

        var ex1 = new Exercise
        {
            Id = "deriv_ex_01",
            Title = "Regla de la Potencia: Derivada de x³ en x = 2",
            GameType = GamePlayType.CalculusDerivativeRules,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 12,
            StandardInstruction = "Aplica la regla de la potencia d/dx(xⁿ) = n·xⁿ⁻¹ para hallar la derivada de f(x) = x³. Su derivada es f'(x) = 3x². Al evaluarla en el instante x = 2: 3 × (2²), ¿cuál es la tasa de cambio instantánea (3 × 4)?",
            SimplifiedInstruction = "La derivada es 3x². Si x vale 2, multiplica 3 × 4 (que es 2 × 2). ¿Cuánto da?",
            ScreenReaderDescription = "Derivada de x cúbica evaluada en x = 2: 3 por 2 al cuadrado igual a 12.",
            FirstHint = "Aplica la regla de la potencia: d/dx(x³) = 3x².",
            SecondHintDetailed = "Sustituye x = 2: 3 × 2² = 3 × 4 = 12.",
            ExplanationOfSuccess = "¡Correcto! f'(x) = 3x² => f'(2) = 3(4) = 12 unidades por segundo.",
            SimplifiedFirstHint = "Multiplica 3 por 4.",
            SimplifiedSecondHint = "3 por 4 es 12.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 12."
        };
        ex1.ItemsGroupA.Add(new ManipulableItem { Id = "deriv_pow_item", Name = "Derivada 12", EmojiSymbol = "⚡", AccessibleDescription = "Velocidad instantánea de 12 u/s", OrderIndex = 1, MusicalPitchHz = 262 });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr1_1", Label = "6", Value = 6, IsCorrect = false, SpeechFeedback = "6 es solo 3 × 2 sin elevar al cuadrado.", AccessibleKeyShortcut = "1" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr1_2", Label = "8", Value = 8, IsCorrect = false, SpeechFeedback = "8 es 2 al cubo (la función original sin derivar).", AccessibleKeyShortcut = "2" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr1_3", Label = "12", Value = 12, IsCorrect = true, SpeechFeedback = "¡Correcto! 3(2²) = 3(4) = 12.", AccessibleKeyShortcut = "3" });
        ex1.Options.Add(new ExerciseOption { Id = "opt_dr1_4", Label = "18", Value = 18, IsCorrect = false, SpeechFeedback = "18 es demasiado alto.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex1);

        var ex2 = new Exercise
        {
            Id = "deriv_ex_02",
            Title = "Derivada de una Función Lineal: f(x) = 7x + 5",
            GameType = GamePlayType.CalculusDerivativeRules,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 7,
            StandardInstruction = "En la función lineal f(x) = 7x + 5, la derivada de 7x es 7 (la pendiente) y la derivada de la constante 5 es 0. ¿Cuál es el valor exacto de la derivada f'(x)?",
            SimplifiedInstruction = "La derivada de 7x + 5 es solo la pendiente del número que acompaña a la x. ¿Qué número es: 7?",
            ScreenReaderDescription = "Derivada de la función lineal f(x) = 7x + 5.",
            FirstHint = "La derivada de m·x + b es simplemente la constante m.",
            SecondHintDetailed = "d/dx(7x + 5) = 7 + 0 = 7.",
            ExplanationOfSuccess = "¡Exacto! La derivada de cualquier recta es su pendiente constante: f'(x) = 7.",
            SimplifiedFirstHint = "Es el número que acompaña a la x.",
            SimplifiedSecondHint = "Es el 7.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! La derivada es 7."
        };
        ex2.ItemsGroupA.Add(new ManipulableItem { Id = "deriv_slope_item", Name = "Pendiente 7", EmojiSymbol = "📐", AccessibleDescription = "Pendiente constante de 7", OrderIndex = 1, MusicalPitchHz = 330 });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_1", Label = "0", Value = 0, IsCorrect = false, SpeechFeedback = "Cero es la derivada de la constante 5 sola, pero tenemos 7x.", AccessibleKeyShortcut = "1" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_2", Label = "5", Value = 5, IsCorrect = false, SpeechFeedback = "5 es la constante que se hace cero al derivar.", AccessibleKeyShortcut = "2" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_3", Label = "7", Value = 7, IsCorrect = true, SpeechFeedback = "¡Correcto! La derivada de 7x es 7.", AccessibleKeyShortcut = "3" });
        ex2.Options.Add(new ExerciseOption { Id = "opt_dr2_4", Label = "12", Value = 12, IsCorrect = false, SpeechFeedback = "12 es la suma (7 + 5).", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex2);

        var ex3 = new Exercise
        {
            Id = "deriv_ex_03",
            Title = "Derivada de un Polinomio: f(x) = 2x² + 3x evaluada en x = 1",
            GameType = GamePlayType.CalculusDerivativeRules,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 7,
            StandardInstruction = "Derivamos término a término: la derivada de 2x² es 2(2x) = 4x, y la de 3x es 3. La función derivada es f'(x) = 4x + 3. Al evaluarla en el instante x = 1 (4(1) + 3), ¿cuánto da la tasa de cambio?",
            SimplifiedInstruction = "Multiplica 4 × 1 = 4, y súmale 3 (+3). ¿Cuánto da?",
            ScreenReaderDescription = "Derivada de 2x² + 3x evaluada en x = 1: 4(1) + 3 = 7.",
            FirstHint = "Calcula la derivada: f'(x) = 4x + 3.",
            SecondHintDetailed = "Sustituye x = 1: 4(1) + 3 = 4 + 3 = 7.",
            ExplanationOfSuccess = "¡Brillante! f'(1) = 4(1) + 3 = 7.",
            SimplifiedFirstHint = "Suma 4 + 3.",
            SimplifiedSecondHint = "4 + 3 son 7.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! Da 7."
        };
        ex3.ItemsGroupA.Add(new ManipulableItem { Id = "deriv_poly_item", Name = "Tasa de Cambio 7", EmojiSymbol = "🚀", AccessibleDescription = "Tasa de cambio de 7 unidades", OrderIndex = 1, MusicalPitchHz = 392 });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_1", Label = "4", Value = 4, IsCorrect = false, SpeechFeedback = "4 es solo 4 × 1, falta sumar 3.", AccessibleKeyShortcut = "1" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_2", Label = "5", Value = 5, IsCorrect = false, SpeechFeedback = "5 es la función original evaluada en 1 (2 + 3).", AccessibleKeyShortcut = "2" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_3", Label = "7", Value = 7, IsCorrect = true, SpeechFeedback = "¡Correcto! 4(1) + 3 = 7.", AccessibleKeyShortcut = "3" });
        ex3.Options.Add(new ExerciseOption { Id = "opt_dr3_4", Label = "10", Value = 10, IsCorrect = false, SpeechFeedback = "10 no corresponde a la evaluación.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex3);

        var ex4 = new Exercise
        {
            Id = "deriv_ex_04",
            Title = "Derivada de una Constante: f(x) = 42",
            GameType = GamePlayType.CalculusDerivativeRules,
            TargetAgeMin = 16,
            TargetAgeMax = 18,
            Level = AcademicLevel.HighSchoolThirdGrade,
            ExpectedResult = 0,
            StandardInstruction = "La derivada mide la rapidez de cambio instantáneo. Si una magnitud permanece completamente constante sin cambiar nunca, como f(x) = 42, ¿cuál es su derivada f'(x)?",
            SimplifiedInstruction = "Si algo no cambia nada de nada, ¿cuál es su velocidad de cambio: 0?",
            ScreenReaderDescription = "Derivada de la función constante f(x) = 42 igual a 0.",
            FirstHint = "Una constante no tiene tasa de cambio.",
            SecondHintDetailed = "d/dx(c) = 0 para cualquier constante c.",
            ExplanationOfSuccess = "¡Extraordinario! d/dx(42) = 0. Lo que no cambia tiene derivada cero.",
            SimplifiedFirstHint = "No cambia nada, es cero.",
            SimplifiedSecondHint = "Da 0.",
            SimplifiedExplanationOfSuccess = "¡Muy bien! La derivada es 0."
        };
        ex4.ItemsGroupA.Add(new ManipulableItem { Id = "deriv_zero_item", Name = "Derivada Cero", EmojiSymbol = "⏸️", AccessibleDescription = "Estado estático con derivada 0", OrderIndex = 1, MusicalPitchHz = 523 });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_1", Label = "0", Value = 0, IsCorrect = true, SpeechFeedback = "¡Correcto! La derivada de una constante es 0.", AccessibleKeyShortcut = "1" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_2", Label = "1", Value = 1, IsCorrect = false, SpeechFeedback = "1 es la derivada de x, no de una constante.", AccessibleKeyShortcut = "2" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_3", Label = "42", Value = 42, IsCorrect = false, SpeechFeedback = "42 es el valor de la función, no su derivada.", AccessibleKeyShortcut = "3" });
        ex4.Options.Add(new ExerciseOption { Id = "opt_dr4_4", Label = "-42", Value = -42, IsCorrect = false, SpeechFeedback = "-42 no es la derivada.", AccessibleKeyShortcut = "4" });
        topic.Exercises.Add(ex4);

        return topic;
    }
}
