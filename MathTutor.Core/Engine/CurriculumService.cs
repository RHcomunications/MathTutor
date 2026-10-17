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
        return new List<PedagogicalTopic>
        {
            BuildKindergartenCountingTopic(),
            BuildAdditionAndPropertiesTopic(),
            BuildSubtractionAndInverseTopic(),
            BuildMultiplicationAndArraysTopic(),
            BuildDivisionAndFairSharingTopic(),
            BuildFractionsTopic(),
            BuildDistributiveMultiplicationTopic(),
            BuildMixedFractionsTopic(),
            BuildDecimalsAndMoneyTopic(),
            BuildPowersAndExponentsTopic(),
            BuildDirectProportionTopic(),
            BuildAlgebraicBalanceTopic(),
            BuildSquareRootTopic(),
            BuildCartesianCoordinatesTopic(),
            BuildSignedNumbersTopic(),
            BuildOrderOfOperationsTopic(),
            BuildLinearEquationsTopic(),
            BuildPercentagesTopic(),
            BuildExponentLawsTopic(),
            BuildPolynomialLikeTermsTopic(),
            BuildLinearSystems2x2Topic(),
            BuildPythagoreanTheoremTopic(),
            BuildQuadraticEquationsTopic(),
            BuildThalesSimilarityTopic(),
            BuildTrigonometricRatiosTopic(),
            BuildQuadraticFunctionsTopic(),
            BuildAnalyticDistanceTopic(),
            BuildLineSlopeTopic(),
            BuildLinearEquationYMXBTopic(),
            BuildConicCircleTopic(),
            BuildSineWaveContinuousTopic(),
            BuildLogarithmBinaryTopic(),
            BuildCalculusDerivativeTopic(),
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
    }

    /// <summary>
    /// Devuelve las misiones pedagógicas permitidas según la edad del alumno (Tarea 6).
    /// </summary>
    public List<PedagogicalTopic> GetTopicsForAge(int age)
    {
        var allowedLevels = AdaptiveEngine.GetAllowedLevelsForAge(age);
        return GetAllTopics()
            .FindAll(t => allowedLevels.Contains(t.Level));
    }

    /// <summary>
    /// Obtiene la siguiente misión recomendada tras completar <paramref name="currentTopicId"/> (Tarea 7).
    /// Busca dentro de los niveles permitidos para la edad del alumno.
    /// </summary>
    public PedagogicalTopic? GetNextTopic(string currentTopicId, UserProfile? profile = null)
    {
        var topics = profile != null ? GetTopicsForAge(profile.Age) : GetAllTopics();
        if (topics.Count == 0)
        {
            topics = GetAllTopics();
        }

        int idx = topics.FindIndex(t => t.Id == currentTopicId);
        if (idx >= 0 && idx + 1 < topics.Count)
        {
            return topics[idx + 1];
        }

        if (profile != null)
        {
            var nextUncompleted = topics.Find(t => t.Id != currentTopicId && !profile.CompletedLessonIds.Contains(t.Id));
            if (nextUncompleted != null)
            {
                return nextUncompleted;
            }
        }

        return topics.Count > 0 ? topics[0] : null;
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
        if (profile.Disability == DisabilityType.Intellectual || (isMonetaryTopic && profile.Region != RegionalContext.Mexico))
        {
            return _generator.GenerateExercisesForTopic(topic.Id, profile, count);
        }

        var templateTopic = GetAllTopics().Find(t => t.Id == topic.Id);
        var result = new List<Exercise>();

        if (templateTopic != null && templateTopic.Exercises.Count > 0)
        {
            // Reto 1: Ejercicio base curado que ejemplifica directamente la explicación de la lección
            result.Add(templateTopic.Exercises[0]);
        }

        int remaining = count - result.Count;
        if (remaining > 0)
        {
            result.AddRange(_generator.GenerateExercisesForTopic(topic.Id, profile, remaining));
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
            Title = "El Bosque Mágico: Los 5 Secretos del Conteo",
            Level = AcademicLevel.Kindergarten,
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "¡Hola, valiente explorador! Soy Cosmo, el búho de los números. En nuestro Bosque Encantado, los números son llaves mágicas que abren cofres y ayudan a nuestros amigos animales. ¿Estás listo para aprender a contar jugando?",
            NarrativeIntroSimplified = "¡Hola! Soy Cosmo. Vamos a jugar con los números en el bosque. ¡Será muy divertido!",
            CoreConceptExplanation = "Contar no es solo decir palabras de memoria. Cuando contamos, seguimos reglas secretas: cada amigo recibe un número (correspondencia 1 a 1), los números nunca se cambian de lugar (orden estable), y el último número que decimos con fuerza nos dice cuántos amigos hay en total (cardinalidad).",
            CoreConceptSimplified = "Para contar, señalamos cada cosita una sola vez. El último número que dices es la cantidad total.",
            PrinciplesAddressed = new List<string>
            {
                "Correspondencia uno a uno (biunívoca)",
                "Orden estable de la serie numérica",
                "Cardinalidad del conjunto",
                "Abstracción",
                "Irrelevancia del orden"
            },
            CongratulationMessage = "¡Extraordinario trabajo! Has descubierto los 5 secretos mágicos del conteo. ¡Las estrellas brillan para ti!"
        };

        // Ejercicio 1: Principio de Correspondencia Uno a Uno y Cardinalidad
        var ex1 = new Exercise
        {
            Id = "count_ex_01",
            Title = "El Banquete de las Manzanas Mágicas",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 3,
            TargetAgeMax = 5,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 3,
            StandardInstruction = "En el bosque hay 3 simpáticos conejitos con mucha hambre. Toca o selecciona cada manzana para dársela a un conejito. ¡Recuerda: una manzana para cada uno! ¿Cuántas manzanas tenemos en total?",
            SimplifiedInstruction = "Toca cada manzana para contarla. Una, dos... ¿cuántas manzanas hay?",
            ScreenReaderDescription = "Hay 3 manzanas rojas jugosas alineadas en el huerto. Cada una tiene una campana musical. Debes contarlas una a una.",
            FirstHint = "Toca la primera manzana y escucha su campana. Luego la siguiente. ¡No saltes ninguna!",
            SecondHintDetailed = "Vamos juntos: una manzana (1), dos manzanas (2), tres manzanas (3). El último número fue el 3.",
            ExplanationOfSuccess = "¡Correcto! Hay 3 manzanas. Le diste exactamente una manzana a cada conejito. ¡Eso es correspondencia uno a uno y el 3 es el total de la colección!"
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
            Title = "El Puente de Piedras del Río Cantarín",
            GameType = GamePlayType.OrderSequence,
            TargetAgeMin = 3,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 4,
            StandardInstruction = "Para cruzar el río, debemos pisar las piedras musicales que flotan en el agua. Las piedras dicen: 1, 2, 3... ¿Qué piedra mágica debe ir a continuación para no caernos al agua?",
            SimplifiedInstruction = "Las piedras dicen: 1, 2, 3... ¿Qué número sigue ahora?",
            ScreenReaderDescription = "Hay un puente con 4 piedras sobre el río. La primera tiene el número 1, la segunda el 2, la tercera el 3, y la cuarta está esperando tu respuesta.",
            FirstHint = "Di los números en voz alta como una canción: uno, dos, tres...",
            SecondHintDetailed = "Después del número 3 siempre viene el número 4. Los números nunca cambian su orden.",
            ExplanationOfSuccess = "¡Bravo! El número 4 es la piedra correcta. Los números siempre viajan en el mismo trenecito ordenado: 1, 2, 3, 4, 5. ¡Eso es el orden estable!"
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
            Title = "El Cofre de los Objetos Curiosos (Abstracción e Irrelevancia)",
            GameType = GamePlayType.InteractiveCounting,
            TargetAgeMin = 4,
            TargetAgeMax = 6,
            Level = AcademicLevel.Kindergarten,
            ExpectedResult = 5,
            StandardInstruction = "Cosmo encontró en su cofre objetos muy diferentes: 2 estrellas doradas, 2 campanas musicales y 1 corazón rojo. ¿Se pueden contar todos juntos aunque sean diferentes? ¿Cuántos tesoros hay en total?",
            SimplifiedInstruction = "Hay estrellas, campanas y un corazón. ¡Todos son juguetes! ¿Cuántos juguetes hay en total?",
            ScreenReaderDescription = "Dentro del cofre hay 5 objetos variados: dos estrellas, dos campanas y un corazón. Puedes contarlos en cualquier orden.",
            FirstHint = "¡Sí podemos contarlos! No importa que unos sean estrellas y otros campanas, todos son tesoros.",
            SecondHintDetailed = "Cuenta uno por uno: 1, 2, 3, 4, 5. No importa si comienzas por el corazón o por las estrellas, el total siempre es 5.",
            ExplanationOfSuccess = "¡Genial! Hay 5 tesoros. Aprendiste el principio de abstracción: podemos contar cualquier clase de objetos, y da igual el orden en que los toques, ¡el resultado siempre es 5!"
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

        return topic;
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
            ScreenReaderDescription = "5 patitos nadando en el agua. Dos de ellos se van nadando hacia la orilla. Quedan 3 patitos en el centro.",
            FirstHint = "Teníamos 5. Cuenta hacia atrás dos pasitos: 4... 3...",
            SecondHintDetailed = "5 patitos menos 2 patitos es igual a 3 patitos.",
            ExplanationOfSuccess = "¡Brillante! 5 - 2 = 3. Al quitar o separar una parte de la colección, descubrimos cuántos quedan."
        };

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
            Title = "Los Saltos de la Ranita Robot",
            GameType = GamePlayType.PatternAndLoopSeed,
            TargetAgeMin = 6,
            TargetAgeMax = 8,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 6,
            StandardInstruction = "Nuestra ranita robot está programada con la instrucción: 'REPETIR 3 VECES { AVANZAR 2 PASOS }'. Es decir: 2 pasos en el primer salto + 2 en el segundo + 2 en el tercero. ¿Cuántos pasos avanzó en total? ¡Calcula 2 + 2 + 2!",
            SimplifiedInstruction = "La ranita da 3 saltos de 2 pasos cada uno: 2 + 2 + 2. ¿Cuántos pasos dio en total?",
            ScreenReaderDescription = "Una ranita mecánica da 3 saltos iguales sobre la recta numérica. Cada salto mide 2 unidades. Aterriza en el 6.",
            FirstHint = "Suma primero los dos primeros saltos: 2 + 2 = 4. Luego suma el tercer salto: 4 + 2...",
            SecondHintDetailed = "2 + 2 + 2 es igual a 6. Tres veces el número 2 son 6.",
            ExplanationOfSuccess = "¡Impresionante! 2 + 2 + 2 = 6, lo que es igual a 3 veces 2 (3 x 2 = 6). ¡Acabas de resolver tu primer algoritmo matemático con bucles!"
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
            ScreenReaderDescription = "Una cuadrícula de 2 filas con 3 manzanas cada una. Total: 6 manzanas.",
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
            ScreenReaderDescription = "Un pastel redondo dividido en 4 rebanadas idénticas. 3 rebanadas han sido tomadas y queda 1 rebanada en el plato.",
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
            ScreenReaderDescription = "Un waffle entero partido en 2 porciones y medio waffle más partido en 1 porción. Total: 3 medios waffles.",
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
            ScreenReaderDescription = "1 moneda dorada de un peso (100 centavos) y 1 moneda plateada de cincuenta centavos. Total: 150 centavos o 1 peso con 50.",
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
            ScreenReaderDescription = "Cuadrado de 9 losas organizado en 3 filas por 3 columnas. Raíz cuadrada de 9.",
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
            NarrativeIntro = "¡Bienvenido a 1º de Secundaria! En la vida cotidiana no todos los números son positivos: en la tiendita cuando pedimos fiado tenemos una deuda (saldo negativo), y en invierno la temperatura en la sierra baja a grados bajo cero. Los números enteros con signo (+ y -) nos permiten calcular ganancias, pérdidas y balances exactos.",
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
            ScreenReaderDescription = "Suma de enteros: positivo 5 más negativo 8. Da negativo 3.",
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
            ScreenReaderDescription = "Expresión: 4 más 3 por 2. Por jerarquía de operaciones, 3 por 2 es 6, más 4 es igual a 10.",
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
            ScreenReaderDescription = "Cálculo de 20% de 200 pesos. 20 por cada 100 pesos da 40 pesos de rebaja.",
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
            ScreenReaderDescription = "Ecuación de la recta y = 4x + 10. Para x = 4 kilómetros: 4 por 4 es 16, más 10 pesos da 26 pesos.",
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
            ScreenReaderDescription = "Logaritmo en base 2 de 64. Potencia de 2 que da 64. 2 elevado a la 6 es 64. Respuesta: 6 pasos.",
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
            ScreenReaderDescription = "Estadística descriptiva: datos 7, 8 y 9. Suma total 24. Dividido entre 3 observaciones da media 8.",
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
            CharacterName = "Cosmo el Búho Sabio",
            NarrativeIntro = "En Secundaria y Preparatoria aprendiste que una función matemática f(x) = 2x + 3 transforma una entrada x en una salida. ¡En programación una 'función' es exactamente eso! Es un bloque de código con nombre propio que recibe datos de entrada (parámetros), los procesa y devuelve un resultado con la instrucción RETORNAR (return).",
            NarrativeIntroSimplified = "Una función de código es como una máquina con nombre: le das un número de entrada (parámetro), hace el cálculo y te devuelve (return) el resultado.",
            CoreConceptExplanation = "Las funciones evitan repetir código y hacen los programas limpios y modulares. Al definir 'FUNCION calcularVolumen(nivel) { RETORNAR (3 * nivel) + 2 }', 'nivel' es el parámetro de entrada. Cuando llamamos a 'calcularVolumen(4)', el parámetro toma el valor 4, calcula (3 × 4) + 2 y retorna 14.",
            CoreConceptSimplified = "En 'FUNCION calcularVolumen(nivel) { RETORNAR 3 * nivel + 2 }', si llamamos a calcularVolumen(4), calcula 3 × 4 + 2 = 14.",
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
            Title = "Llamando a la Función: calcularVolumen(4)",
            GameType = GamePlayType.ProgrammingFunctionsReturn,
            TargetAgeMin = 10,
            TargetAgeMax = 18,
            Level = AcademicLevel.ProgrammingFoundations,
            ExpectedResult = 14,
            StandardInstruction = "En el módulo de audio accesible programamos esta función reutilizable:\n• FUNCION calcularVolumen(nivel) { RETORNAR (3 * nivel) + 2 }\nSi el programa principal invoca la función pasándole el argumento 4: calcularVolumen(4), ¿qué valor numérico retorna (return) la función?",
            SimplifiedInstruction = "La función 'calcularVolumen' multiplica el número de entrada por 3 y luego suma 2. Si entra el 4, ¿cuánto es (3 × 4) + 2?",
            ScreenReaderDescription = "Función calcularVolumen con parámetro nivel: retorna 3 por nivel más 2. Evaluar calcularVolumen con argumento 4.",
            FirstHint = "Sustituye el parámetro 'nivel' por el argumento 4: primero multiplica 3 × 4 = 12.",
            SecondHintDetailed = "Ahora súmale 2 al producto: 12 + 2 = 14 decibeles. Ese es el valor que retorna la función.",
            ExplanationOfSuccess = "¡Magnífico! Al llamar a calcularVolumen(4), el parámetro 'nivel' recibió 4, evaluó (3 × 4) + 2 = 14 y retornó 14 decibeles."
        };

        ex1.ItemsGroupA.Add(new ManipulableItem
        {
            Id = "func_call_item",
            Name = "calcularVolumen(4) → return 14",
            EmojiSymbol = "⚙️",
            AccessibleDescription = "Función calcularVolumen: recibe entrada 4, calcula 3 por 4 más 2 y retorna 14",
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
}
