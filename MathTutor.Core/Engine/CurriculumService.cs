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
            BuildRepeatedAdditionAndProgrammingSeedTopic()
        };
    }

    /// <summary>
    /// Genera una serie fresca de ejercicios adaptados dinámicamente según la edad y condición del alumno.
    /// </summary>
    public List<Exercise> GenerateAdaptiveExercises(PedagogicalTopic topic, UserProfile profile, int count = 3)
    {
        return _generator.GenerateExercisesForTopic(topic.Id, profile, count);
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
}
