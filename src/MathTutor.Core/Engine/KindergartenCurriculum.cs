using System.Collections.Generic;
using MathTutor.Core.Models;

namespace MathTutor.Core.Engine;

/// <summary>
/// Contenido curado de jardín de niños por banda de edad (3-4, 4-5 y 5-6 años).
/// </summary>
internal static class KindergartenCurriculum
{
    private static readonly int[] Frequencies = { 262, 294, 330, 349, 392, 440, 494, 523 };

    public static List<PedagogicalTopic> BuildTopics() => new()
    {
        BuildShapesTopic(),
        BuildPositionsTopic(),
        BuildStoriesTopic(),
        BuildPatternsTopic(),
        BuildSortingTopic(),
        BuildMusicTopic(),
        BuildCountToTenTopic(),
        BuildAddSubtractTopic(),
        BuildMeasureTopic()
    };

    private static PedagogicalTopic BuildShapesTopic()
    {
        var topic = CreateTopic(
            "kinder_shapes_01",
            "El Jardín de las Formas",
            3,
            4,
            "Tita la Tortuga",
            "¡Hola! Soy Tita la Tortuga. Vamos a mirar formas y tamaños en el jardín.",
            "¡Hola! Soy Tita. Vamos a mirar formas.",
            "Las cosas tienen forma y tamaño. Un círculo es redondo. Un cuadrado tiene cuatro lados. Un triángulo tiene tres lados.",
            "Las cosas tienen forma. Mira el círculo, el cuadrado y el triángulo.",
            "¡Muy bien! Ya reconoces formas y tamaños.",
            "Reconocer formas", "Comparar tamaños");

        var ex1 = BaseExercise(
            "shape_ex_01", "Los Círculos del Estanque", GamePlayType.InteractiveCounting, 3, 4,
            "Toca solo los círculos. ¿Cuántos círculos hay?",
            "Toca los círculos. ¿Cuántos hay?",
            "Hay dos círculos en el estanque.",
            "Toca cada círculo una vez.",
            "Cuenta: uno, dos. Hay 2 círculos.",
            "¡Muy bien! Hay 2 círculos.");
        AddItem(ex1, false, "shape_c1", "Círculo 1", "🔵", "Un círculo azul");
        AddItem(ex1, false, "shape_c2", "Círculo 2", "🔵", "Otro círculo azul");
        AddOptions(ex1, 2, ("1 círculo", 1), ("2 círculos", 2), ("3 círculos", 3));
        topic.Exercises.Add(ex1);

        var ex2 = BaseExercise(
            "shape_ex_02", "El Cuadrado de la Ventana", GamePlayType.InteractiveCounting, 3, 4,
            "Toca solo el cuadrado. ¿Cuántos cuadrados hay?",
            "Toca el cuadrado. ¿Cuántos hay?",
            "Hay un cuadrado en la ventana.",
            "Busca la forma de cuatro lados.",
            "Cuenta: uno. Hay 1 cuadrado.",
            "¡Muy bien! Hay 1 cuadrado.");
        AddItem(ex2, false, "shape_s2", "Cuadrado", "🟦", "Un cuadrado azul");
        AddOptions(ex2, 1, ("0 cuadrados", 0), ("1 cuadrado", 1), ("2 cuadrados", 2));
        topic.Exercises.Add(ex2);

        var ex3 = BaseExercise(
            "shape_ex_03", "El Triángulo del Cerro", GamePlayType.InteractiveCounting, 3, 4,
            "Toca solo los triángulos. ¿Cuántos triángulos hay?",
            "Toca los triángulos. ¿Cuántos hay?",
            "Hay tres triángulos en el cerro.",
            "Busca las formas de tres lados.",
            "Cuenta: uno, dos, tres. Hay 3 triángulos.",
            "¡Muy bien! Hay 3 triángulos.");
        AddItem(ex3, false, "shape_t1", "Triángulo 1", "🔺", "Un triángulo rojo");
        AddItem(ex3, false, "shape_t2", "Triángulo 2", "🔺", "Otro triángulo rojo");
        AddItem(ex3, false, "shape_t3", "Triángulo 3", "🔺", "Un tercer triángulo rojo");
        AddOptions(ex3, 3, ("2 triángulos", 2), ("3 triángulos", 3), ("4 triángulos", 4));
        topic.Exercises.Add(ex3);

        var ex4 = BaseExercise(
            "shape_ex_04", "Grande y Pequeño", GamePlayType.Comparison, 3, 4,
            "¿Dónde hay más estrellas?",
            "¿Dónde hay más estrellas?",
            "Un grupo tiene estrellas grandes. El otro tiene estrellas pequeñas.",
            "Cuenta las estrellas de cada grupo.",
            "Las grandes son 3. Las pequeñas son 2. 3 es más.",
            "¡Bien! Las grandes son más.");
        AddItem(ex4, false, "shape_big1", "Estrella grande 1", "🌟", "Una estrella grande");
        AddItem(ex4, false, "shape_big2", "Estrella grande 2", "🌟", "Otra estrella grande");
        AddItem(ex4, false, "shape_big3", "Estrella grande 3", "🌟", "Una tercera estrella grande");
        AddItem(ex4, true, "shape_small1", "Estrella pequeña 1", "⭐", "Una estrella pequeña");
        AddItem(ex4, true, "shape_small2", "Estrella pequeña 2", "⭐", "Otra estrella pequeña");
        AddOptions(ex4, 1, ("Las grandes", 1), ("Las pequeñas", 2), ("Igual", 0));
        topic.Exercises.Add(ex4);

        var ex5 = BaseExercise(
            "shape_ex_05", "La Casa de las Formas", GamePlayType.InteractiveCounting, 3, 4,
            "¿Cuántas formas hay en la casa?",
            "¿Cuántas formas hay?",
            "La casa tiene círculos, un cuadrado y un triángulo.",
            "Toca cada forma una vez.",
            "Cuenta: uno, dos, tres, cuatro. Hay 4 formas.",
            "¡Muy bien! Hay 4 formas.");
        AddItem(ex5, false, "shape_h1", "Círculo 1", "🔵", "Un círculo azul");
        AddItem(ex5, false, "shape_h2", "Círculo 2", "🔵", "Otro círculo azul");
        AddItem(ex5, false, "shape_h3", "Cuadrado", "🟦", "Un cuadrado azul");
        AddItem(ex5, false, "shape_h4", "Triángulo", "🔺", "Un triángulo rojo");
        AddOptions(ex5, 4, ("3 formas", 3), ("4 formas", 4), ("5 formas", 5));
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildPositionsTopic()
    {
        var topic = CreateTopic(
            "kinder_positions_01",
            "¿Dónde Está?",
            3,
            4,
            "Nico el Conejo",
            "Soy Nico el Conejo. Vamos a mirar dónde están las cosas.",
            "Soy Nico. Vamos a mirar dónde están las cosas.",
            "Puedes decir arriba, abajo, dentro y fuera. Mira con atención.",
            "Puedes decir arriba, abajo, dentro y fuera.",
            "¡Muy bien! Ya sabes dónde están las cosas.",
            "Arriba y abajo", "Dentro y fuera", "Cerca y lejos");

        var ex1 = BaseExercise(
            "position_ex_01", "Arriba en el Árbol", GamePlayType.InteractiveCounting, 3, 4,
            "Los pájaros están arriba. ¿Cuántos pájaros hay arriba?",
            "¿Cuántos pájaros hay arriba?",
            "Dos pájaros están arriba en el árbol.",
            "Mira la parte de arriba.",
            "Cuenta: uno, dos. Hay 2 pájaros arriba.",
            "¡Muy bien! Hay 2 pájaros arriba.");
        AddItem(ex1, false, "position_bird1", "Pájaro 1", "🐦", "Un pájaro arriba en el árbol");
        AddItem(ex1, false, "position_bird2", "Pájaro 2", "🐦", "Otro pájaro arriba en el árbol");
        AddOptions(ex1, 2, ("1 pájaro", 1), ("2 pájaros", 2), ("3 pájaros", 3));
        topic.Exercises.Add(ex1);

        var ex2 = BaseExercise(
            "position_ex_02", "Abajo en el Nido", GamePlayType.InteractiveCounting, 3, 4,
            "El huevo está abajo. ¿Cuántos huevos hay abajo?",
            "¿Cuántos huevos hay abajo?",
            "Un huevo está abajo en el nido.",
            "Mira la parte de abajo.",
            "Cuenta: uno. Hay 1 huevo abajo.",
            "¡Muy bien! Hay 1 huevo abajo.");
        AddItem(ex2, false, "position_egg1", "Huevo", "🥚", "Un huevo abajo en el nido");
        AddOptions(ex2, 1, ("0 huevos", 0), ("1 huevo", 1), ("2 huevos", 2));
        topic.Exercises.Add(ex2);

        var ex3 = BaseExercise(
            "position_ex_03", "Dentro de la Caja", GamePlayType.InteractiveCounting, 3, 4,
            "Los juguetes están dentro de la caja. ¿Cuántos hay dentro?",
            "¿Cuántos juguetes hay dentro?",
            "Tres juguetes están dentro de la caja.",
            "Mira dentro de la caja.",
            "Cuenta: uno, dos, tres. Hay 3 juguetes dentro.",
            "¡Muy bien! Hay 3 juguetes dentro.");
        AddItem(ex3, false, "position_toy1", "Juguete 1", "🧸", "Un juguete dentro de la caja");
        AddItem(ex3, false, "position_toy2", "Juguete 2", "🧸", "Otro juguete dentro de la caja");
        AddItem(ex3, false, "position_toy3", "Juguete 3", "🧸", "Un tercer juguete dentro de la caja");
        AddOptions(ex3, 3, ("2 juguetes", 2), ("3 juguetes", 3), ("4 juguetes", 4));
        topic.Exercises.Add(ex3);

        var ex4 = BaseExercise(
            "position_ex_04", "Fuera de la Caja", GamePlayType.InteractiveCounting, 3, 4,
            "La pelota está fuera. ¿Cuántas pelotas hay fuera?",
            "¿Cuántas pelotas hay fuera?",
            "Una pelota está fuera de la caja.",
            "Mira fuera de la caja.",
            "Cuenta: una. Hay 1 pelota fuera.",
            "¡Muy bien! Hay 1 pelota fuera.");
        AddItem(ex4, false, "position_ball2", "Pelota", "⚽", "Una pelota fuera de la caja");
        AddOptions(ex4, 1, ("0 pelotas", 0), ("1 pelota", 1), ("2 pelotas", 2));
        topic.Exercises.Add(ex4);

        var ex5 = BaseExercise(
            "position_ex_05", "Cerca y Lejos", GamePlayType.Comparison, 3, 4,
            "El conejo está cerca. ¿Qué está cerca?",
            "¿Qué está cerca?",
            "El conejo está cerca. La casa y el árbol están lejos.",
            "Mira lo que está a tu lado.",
            "El conejo está cerca de ti.",
            "¡Bien! El conejo está cerca.");
        AddItem(ex5, false, "position_rabbit", "Conejo", "🐰", "Un conejo cerca de ti");
        AddItem(ex5, false, "position_house", "Casa", "🏠", "Una casa lejos");
        AddItem(ex5, false, "position_tree", "Árbol", "🌳", "Un árbol lejos");
        AddOptions(ex5, 1, ("El conejo", 1), ("La casa", 2), ("El árbol", 3));
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildStoriesTopic()
    {
        var topic = CreateTopic(
            "kinder_stories_01",
            "Cuentos con Números",
            3,
            5,
            "Luna la Cuentacuentos",
            "Soy Luna la Cuentacuentos. Vamos a contar cosas de los cuentos.",
            "Soy Luna. Vamos a contar cosas de cuentos.",
            "Los cuentos tienen números. Cuenta lo que pasa en la historia.",
            "Los cuentos tienen números. Cuenta lo que pasa.",
            "¡Muy bien! Ya cuentas cosas de los cuentos.",
            "Contar en historias", "Juntar y quitar");

        var ex1 = Counting(
            "story_ex_01", "Los Tres Cerditos", 3, 5, 3, "🐷", "cerdito", "cerditos",
            "Tres cerditos van a su casa. ¿Cuántos cerditos hay?",
            "¿Cuántos cerditos hay?",
            "Hay tres cerditos caminando a su casa.",
            "Toca cada cerdito una vez.",
            "Cuenta: uno, dos, tres. Hay 3 cerditos.",
            "¡Muy bien! Hay 3 cerditos.");
        topic.Exercises.Add(ex1);

        var ex2 = Counting(
            "story_ex_02", "Caperucita y las Flores", 3, 5, 4, "🌼", "flor", "flores",
            "Caperucita recoge flores. ¿Cuántas flores hay?",
            "¿Cuántas flores hay?",
            "Hay cuatro flores en el camino.",
            "Toca cada flor una vez.",
            "Cuenta: uno, dos, tres, cuatro. Hay 4 flores.",
            "¡Muy bien! Hay 4 flores.");
        topic.Exercises.Add(ex2);

        var ex3 = BaseExercise(
            "story_ex_03", "El Patito Perdido", GamePlayType.InteractiveCounting, 3, 5,
            "Dos patitos buscan a su mamá. ¿Cuántos patitos hay?",
            "¿Cuántos patitos hay?",
            "Hay dos patitos en el estanque.",
            "Toca cada patito una vez.",
            "Cuenta: uno, dos. Hay 2 patitos.",
            "¡Muy bien! Hay 2 patitos.");
        AddItem(ex3, false, "story_duck1", "Patito 1", "🦆", "Un patito");
        AddItem(ex3, false, "story_duck2", "Patito 2", "🦆", "Otro patito");
        AddOptions(ex3, 2, ("1 patito", 1), ("2 patitos", 2), ("3 patitos", 3));
        topic.Exercises.Add(ex3);

        var ex4 = Combine(
            "story_ex_04", "La Cesta de la Abuela", 3, 5, 2, 3, "🍎", "manzana", "manzanas",
            "La abuela pone 2 manzanas y 3 manzanas. ¿Cuántas manzanas hay?",
            "Hay 2 manzanas y 3 manzanas. ¿Cuántas hay?",
            "Dos manzanas en una cesta y tres manzanas en otra.",
            "Cuenta todas las manzanas.",
            "2 y 3 son 5. Hay 5 manzanas.",
            "¡Muy bien! Hay 5 manzanas.");
        topic.Exercises.Add(ex4);

        var ex5 = TakeAway(
            "story_ex_05", "El Lobo y las Ovejas", 3, 5, 5, 2, "🐑", "oveja", "ovejas",
            "Había 5 ovejas y 2 se fueron. ¿Cuántas ovejas quedan?",
            "Había 5 ovejas. 2 se fueron. ¿Cuántas quedan?",
            "Quedan tres ovejas y dos se van.",
            "Cuenta las ovejas que quedan.",
            "5 menos 2 son 3. Quedan 3 ovejas.",
            "¡Muy bien! Quedan 3 ovejas.");
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildPatternsTopic()
    {
        var topic = CreateTopic(
            "kinder_patterns_01",
            "Sigue el Ritmo",
            4,
            5,
            "Rita la Rana",
            "Soy Rita la Rana. Vamos a seguir el ritmo y los colores.",
            "Soy Rita. Vamos a seguir el ritmo.",
            "Mira lo que se repite. ¿Qué viene después?",
            "Mira lo que se repite. ¿Qué viene después?",
            "¡Muy bien! Ya sigues el ritmo.",
            "Seguir patrones", "Predecir lo que sigue");

        var ex1 = Choice(
            "pattern_ex_01", "Rojo y Azul", GamePlayType.OrderSequence, 4, 5,
            new (string Id, string Name, string Emoji, string Desc)[]
            {
                ("pattern_r1", "Rojo 1", "🔴", "Un círculo rojo"),
                ("pattern_b1", "Azul 1", "🔵", "Un círculo azul"),
                ("pattern_r2", "Rojo 2", "🔴", "Otro círculo rojo"),
                ("pattern_b2", "Azul 2", "🔵", "Otro círculo azul")
            },
            "Mira: rojo, azul, rojo, azul... ¿Qué sigue?",
            "Rojo, azul, rojo, azul... ¿Qué sigue?",
            "Una fila de círculos rojos y azules.",
            "Mira el color que se repite.",
            "La fila termina en azul. Después viene rojo.",
            "¡Bien! Después del azul viene el rojo.",
            1, ("Rojo", 1), ("Azul", 2), ("Verde", 3));
        topic.Exercises.Add(ex1);

        var ex2 = Choice(
            "pattern_ex_02", "Círculo y Cuadrado", GamePlayType.OrderSequence, 4, 5,
            new (string Id, string Name, string Emoji, string Desc)[]
            {
                ("pattern_c1", "Círculo 1", "🔵", "Un círculo azul"),
                ("pattern_s1", "Cuadrado 1", "🟦", "Un cuadrado azul"),
                ("pattern_c2", "Círculo 2", "🔵", "Otro círculo azul"),
                ("pattern_s2", "Cuadrado 2", "🟦", "Otro cuadrado azul")
            },
            "Mira: círculo, cuadrado, círculo, cuadrado... ¿Qué sigue?",
            "Círculo, cuadrado, círculo, cuadrado... ¿Qué sigue?",
            "Una fila de círculos y cuadrados.",
            "Mira la forma que se repite.",
            "La fila termina en cuadrado. Después viene círculo.",
            "¡Bien! Después del cuadrado viene el círculo.",
            1, ("Círculo", 1), ("Cuadrado", 2), ("Triángulo", 3));
        topic.Exercises.Add(ex2);

        var ex3 = Choice(
            "pattern_ex_03", "Grande y Pequeño", GamePlayType.OrderSequence, 4, 5,
            new (string Id, string Name, string Emoji, string Desc)[]
            {
                ("pattern_big1", "Estrella grande 1", "🌟", "Una estrella grande"),
                ("pattern_small1", "Estrella pequeña 1", "⭐", "Una estrella pequeña"),
                ("pattern_big2", "Estrella grande 2", "🌟", "Otra estrella grande"),
                ("pattern_small2", "Estrella pequeña 2", "⭐", "Otra estrella pequeña")
            },
            "Mira: grande, pequeño, grande, pequeño... ¿Qué sigue?",
            "Grande, pequeño, grande, pequeño... ¿Qué sigue?",
            "Una fila de estrellas grandes y pequeñas.",
            "Mira el tamaño que se repite.",
            "La fila termina en pequeña. Después viene grande.",
            "¡Bien! Después de la pequeña viene la grande.",
            1, ("Grande", 1), ("Pequeña", 2), ("Mediana", 3));
        topic.Exercises.Add(ex3);

        var ex4 = Choice(
            "pattern_ex_04", "Tambor y Palmas", GamePlayType.OrderSequence, 4, 5,
            new (string Id, string Name, string Emoji, string Desc)[]
            {
                ("pattern_drum1", "Tambor 1", "🥁", "Un tambor"),
                ("pattern_clap1", "Palmas 1", "👏", "Una palmada"),
                ("pattern_drum2", "Tambor 2", "🥁", "Otro tambor"),
                ("pattern_clap2", "Palmas 2", "👏", "Otra palmada")
            },
            "Mira: tambor, palmas, tambor, palmas... ¿Qué sigue?",
            "Tambor, palmas, tambor, palmas... ¿Qué sigue?",
            "Una fila de tambores y palmadas.",
            "Mira el sonido que se repite.",
            "La fila termina en palmas. Después viene tambor.",
            "¡Bien! Después de las palmas viene el tambor.",
            1, ("Tambor", 1), ("Palmas", 2), ("Silencio", 3));
        topic.Exercises.Add(ex4);

        var ex5 = Choice(
            "pattern_ex_05", "La Fila de Colores", GamePlayType.OrderSequence, 4, 5,
            new (string Id, string Name, string Emoji, string Desc)[]
            {
                ("pattern_red1", "Rojo 1", "🔴", "Un círculo rojo"),
                ("pattern_yellow1", "Amarillo 1", "🟡", "Un círculo amarillo"),
                ("pattern_red2", "Rojo 2", "🔴", "Otro círculo rojo"),
                ("pattern_yellow2", "Amarillo 2", "🟡", "Otro círculo amarillo")
            },
            "Mira: rojo, amarillo, rojo, amarillo... ¿Qué sigue?",
            "Rojo, amarillo, rojo, amarillo... ¿Qué sigue?",
            "Una fila de círculos rojos y amarillos.",
            "Mira el color que se repite.",
            "La fila termina en amarillo. Después viene rojo.",
            "¡Bien! Después del amarillo viene el rojo.",
            1, ("Rojo", 1), ("Azul", 2), ("Amarillo", 3));
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildSortingTopic()
    {
        var topic = CreateTopic(
            "kinder_sorting_01",
            "Pon Cada Cosa en su Lugar",
            4,
            5,
            "Pancho el Panda",
            "Soy Pancho el Panda. Vamos a poner cada cosa en su lugar.",
            "Soy Pancho. Vamos a poner cada cosa en su lugar.",
            "Puedes agrupar cosas por color, forma o tamaño.",
            "Puedes agrupar cosas por color, forma o tamaño.",
            "¡Muy bien! Ya agrupas cosas por color, forma y tamaño.",
            "Agrupar por color", "Agrupar por forma", "Agrupar por tamaño");

        var ex1 = BaseExercise(
            "sorting_ex_01", "Manzanas Rojas y Verdes", GamePlayType.Comparison, 4, 5,
            "¿Dónde hay más manzanas, en el grupo rojo o en el grupo verde?",
            "¿Dónde hay más manzanas?",
            "El grupo rojo tiene tres manzanas. El grupo verde tiene dos manzanas.",
            "Cuenta las manzanas de cada grupo.",
            "Las rojas son 3. Las verdes son 2. 3 es más.",
            "¡Bien! Las rojas son más.");
        AddItem(ex1, false, "sorting_red1", "Manzana roja 1", "🍎", "Una manzana roja");
        AddItem(ex1, false, "sorting_red2", "Manzana roja 2", "🍎", "Otra manzana roja");
        AddItem(ex1, false, "sorting_red3", "Manzana roja 3", "🍎", "Una tercera manzana roja");
        AddItem(ex1, true, "sorting_green1", "Manzana verde 1", "🍏", "Una manzana verde");
        AddItem(ex1, true, "sorting_green2", "Manzana verde 2", "🍏", "Otra manzana verde");
        AddOptions(ex1, 1, ("Las rojas", 1), ("Las verdes", 2), ("Igual", 0));
        topic.Exercises.Add(ex1);

        var ex2 = BaseExercise(
            "sorting_ex_02", "Estrellas Amarillas y Azules", GamePlayType.Comparison, 4, 5,
            "¿Dónde hay más estrellas, en el grupo amarillo o en el grupo azul?",
            "¿Dónde hay más estrellas?",
            "El grupo amarillo tiene cuatro estrellas. El grupo azul tiene una estrella.",
            "Cuenta las estrellas de cada grupo.",
            "Las amarillas son 4. La azul es 1. 4 es más.",
            "¡Bien! Las amarillas son más.");
        AddItem(ex2, false, "sorting_yellow1", "Estrella amarilla 1", "⭐", "Una estrella amarilla");
        AddItem(ex2, false, "sorting_yellow2", "Estrella amarilla 2", "⭐", "Otra estrella amarilla");
        AddItem(ex2, false, "sorting_yellow3", "Estrella amarilla 3", "⭐", "Una tercera estrella amarilla");
        AddItem(ex2, false, "sorting_yellow4", "Estrella amarilla 4", "⭐", "Una cuarta estrella amarilla");
        AddItem(ex2, true, "sorting_blue1", "Estrella azul", "💠", "Una estrella azul");
        AddOptions(ex2, 1, ("Las amarillas", 1), ("La azul", 2), ("Igual", 0));
        topic.Exercises.Add(ex2);

        var ex3 = BaseExercise(
            "sorting_ex_03", "Círculos y Cuadrados", GamePlayType.Comparison, 4, 5,
            "¿Dónde hay más formas, en el grupo de círculos o en el grupo de cuadrados?",
            "¿Dónde hay más formas?",
            "Hay tres círculos en un grupo y dos cuadrados en el otro grupo.",
            "Cuenta las formas de cada grupo.",
            "Los círculos son 3. Los cuadrados son 2. 3 es más.",
            "¡Bien! Hay más círculos.");
        AddItem(ex3, false, "sorting_c1", "Círculo 1", "🔵", "Un círculo azul");
        AddItem(ex3, false, "sorting_c2", "Círculo 2", "🔵", "Otro círculo azul");
        AddItem(ex3, false, "sorting_c3", "Círculo 3", "🔵", "Un tercer círculo azul");
        AddItem(ex3, true, "sorting_s1", "Cuadrado 1", "🟦", "Un cuadrado azul");
        AddItem(ex3, true, "sorting_s2", "Cuadrado 2", "🟦", "Otro cuadrado azul");
        AddOptions(ex3, 1, ("Los círculos", 1), ("Los cuadrados", 2), ("Igual", 0));
        topic.Exercises.Add(ex3);

        var ex4 = BaseExercise(
            "sorting_ex_04", "Grandes y Pequeños", GamePlayType.Comparison, 4, 5,
            "¿Dónde hay más bloques?",
            "¿Dónde hay más bloques?",
            "Un grupo tiene bloques grandes. El otro tiene bloques pequeños.",
            "Cuenta los bloques de cada grupo.",
            "Los grandes son 4. Los pequeños son 2. 4 es más.",
            "¡Bien! Los grandes son más.");
        AddItem(ex4, false, "sorting_big1", "Bloque grande 1", "🟥", "Un bloque grande");
        AddItem(ex4, false, "sorting_big2", "Bloque grande 2", "🟥", "Otro bloque grande");
        AddItem(ex4, false, "sorting_big3", "Bloque grande 3", "🟥", "Un tercer bloque grande");
        AddItem(ex4, false, "sorting_big4", "Bloque grande 4", "🟥", "Un cuarto bloque grande");
        AddItem(ex4, true, "sorting_small1", "Bloque pequeño 1", "🟥", "Un bloque pequeño");
        AddItem(ex4, true, "sorting_small2", "Bloque pequeño 2", "🟥", "Otro bloque pequeño");
        AddOptions(ex4, 1, ("Los grandes", 1), ("Los pequeños", 2), ("Igual", 0));
        topic.Exercises.Add(ex4);

        var ex5 = BaseExercise(
            "sorting_ex_05", "La Mesa de las Frutas", GamePlayType.InteractiveCounting, 4, 5,
            "¿Cuántas frutas hay en total?",
            "¿Cuántas frutas hay?",
            "Hay dos manzanas, dos plátanos y una pera.",
            "Cuenta todas las frutas.",
            "Cuenta: uno, dos, tres, cuatro, cinco. Hay 5 frutas.",
            "¡Muy bien! Hay 5 frutas.");
        AddItem(ex5, false, "sorting_apple1", "Manzana 1", "🍎", "Una manzana roja");
        AddItem(ex5, false, "sorting_apple2", "Manzana 2", "🍎", "Otra manzana roja");
        AddItem(ex5, false, "sorting_banana1", "Plátano 1", "🍌", "Un plátano");
        AddItem(ex5, false, "sorting_banana2", "Plátano 2", "🍌", "Otro plátano");
        AddItem(ex5, false, "sorting_pear1", "Pera", "🍐", "Una pera");
        AddOptions(ex5, 5, ("4 frutas", 4), ("5 frutas", 5), ("6 frutas", 6));
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildMusicTopic()
    {
        var topic = CreateTopic(
            "kinder_music_01",
            "Música y Conteo",
            4,
            5,
            "Tito el Tambor",
            "Soy Tito el Tambor. Vamos a contar sonidos y seguir el ritmo.",
            "Soy Tito. Vamos a contar sonidos.",
            "La música tiene ritmo. Cuenta los sonidos y sigue el compás.",
            "La música tiene ritmo. Cuenta los sonidos.",
            "¡Muy bien! Ya cuentas sonidos y sigues el ritmo.",
            "Contar sonidos", "Seguir ritmos");

        var ex1 = Counting(
            "music_ex_01", "Los Tambores", 4, 5, 3, "🥁", "tambor", "tambores",
            "Toca cada tambor y cuenta. ¿Cuántos tambores hay?",
            "¿Cuántos tambores hay?",
            "Hay tres tambores en la banda.",
            "Toca cada tambor una vez.",
            "Cuenta: uno, dos, tres. Hay 3 tambores.",
            "¡Muy bien! Hay 3 tambores.");
        topic.Exercises.Add(ex1);

        var ex2 = Counting(
            "music_ex_02", "Las Campanas", 4, 5, 4, "🔔", "campana", "campanas",
            "Toca cada campana y cuenta. ¿Cuántas campanas hay?",
            "¿Cuántas campanas hay?",
            "Hay cuatro campanas en la banda.",
            "Toca cada campana una vez.",
            "Cuenta: uno, dos, tres, cuatro. Hay 4 campanas.",
            "¡Muy bien! Hay 4 campanas.");
        topic.Exercises.Add(ex2);

        var ex3 = Choice(
            "music_ex_03", "Palmas y Tambor", GamePlayType.OrderSequence, 4, 5,
            new (string Id, string Name, string Emoji, string Desc)[]
            {
                ("music_clap1", "Palmas 1", "👏", "Una palmada"),
                ("music_drum1", "Tambor 1", "🥁", "Un tambor"),
                ("music_clap2", "Palmas 2", "👏", "Otra palmada"),
                ("music_drum2", "Tambor 2", "🥁", "Otro tambor")
            },
            "Mira: palmas, tambor, palmas, tambor... ¿Qué sigue?",
            "Palmas, tambor, palmas, tambor... ¿Qué sigue?",
            "Una fila de palmadas y tambores.",
            "Mira el sonido que se repite.",
            "La fila termina en tambor. Después vienen las palmas.",
            "¡Bien! Después del tambor vienen las palmas.",
            1, ("Palmas", 1), ("Tambor", 2), ("Pausa", 3));
        topic.Exercises.Add(ex3);

        var ex4 = Choice(
            "music_ex_04", "Fuerte y Suave", GamePlayType.OrderSequence, 4, 5,
            new (string Id, string Name, string Emoji, string Desc)[]
            {
                ("music_loud1", "Fuerte 1", "🔊", "Un sonido fuerte"),
                ("music_soft1", "Suave 1", "🔈", "Un sonido suave"),
                ("music_loud2", "Fuerte 2", "🔊", "Otro sonido fuerte"),
                ("music_soft2", "Suave 2", "🔈", "Otro sonido suave")
            },
            "Mira: fuerte, suave, fuerte, suave... ¿Qué sigue?",
            "Fuerte, suave, fuerte, suave... ¿Qué sigue?",
            "Una fila de sonidos fuertes y suaves.",
            "Mira el sonido que se repite.",
            "La fila termina en suave. Después viene fuerte.",
            "¡Bien! Después del suave viene el fuerte.",
            1, ("Fuerte", 1), ("Suave", 2), ("Silencio", 3));
        topic.Exercises.Add(ex4);

        var ex5 = BaseExercise(
            "music_ex_05", "La Banda de Cosmo", GamePlayType.InteractiveCounting, 4, 5,
            "¿Cuántos instrumentos hay en la banda?",
            "¿Cuántos instrumentos hay?",
            "Hay dos tambores, dos campanas y una flauta.",
            "Cuenta todos los instrumentos.",
            "Cuenta: uno, dos, tres, cuatro, cinco. Hay 5 instrumentos.",
            "¡Muy bien! Hay 5 instrumentos.");
        AddItem(ex5, false, "music_drum3", "Tambor 1", "🥁", "Un tambor");
        AddItem(ex5, false, "music_drum4", "Tambor 2", "🥁", "Otro tambor");
        AddItem(ex5, false, "music_bell1", "Campana 1", "🔔", "Una campana");
        AddItem(ex5, false, "music_bell2", "Campana 2", "🔔", "Otra campana");
        AddItem(ex5, false, "music_flute1", "Flauta", "🎺", "Una flauta");
        AddOptions(ex5, 5, ("4 instrumentos", 4), ("5 instrumentos", 5), ("6 instrumentos", 6));
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildCountToTenTopic()
    {
        var topic = CreateTopic(
            "kinder_count_to_10_01",
            "Cuenta hasta Diez",
            5,
            6,
            "Cosmo el Búho Sabio",
            "Soy Cosmo. Vamos a contar hasta diez con los peces y las flores.",
            "Soy Cosmo. Vamos a contar hasta diez.",
            "Puedes contar hasta 10. Toca cada cosa una vez y di un número.",
            "Puedes contar hasta 10. Toca cada cosa una vez.",
            "¡Muy bien! Ya cuentas hasta diez.",
            "Contar hasta 10", "Comparar cantidades", "Componer 10");

        var ex1 = Counting(
            "count10_ex_01", "Los Peces del Estanque", 5, 6, 8, "🐟", "pez", "peces",
            "Toca cada pez y cuenta. ¿Cuántos peces hay?",
            "¿Cuántos peces hay?",
            "Hay ocho peces en el estanque.",
            "Toca cada pez una vez.",
            "Cuenta: 1, 2, 3, 4, 5, 6, 7, 8. Hay 8 peces.",
            "¡Muy bien! Hay 8 peces.");
        topic.Exercises.Add(ex1);

        var ex2 = Counting(
            "count10_ex_02", "Las Flores del Jardín", 5, 6, 10, "🌻", "flor", "flores",
            "Toca cada flor y cuenta. ¿Cuántas flores hay?",
            "¿Cuántas flores hay?",
            "Hay diez flores en el jardín.",
            "Toca cada flor una vez.",
            "Cuenta: 1, 2, 3, 4, 5, 6, 7, 8, 9, 10. Hay 10 flores.",
            "¡Muy bien! Hay 10 flores.");
        topic.Exercises.Add(ex2);

        var ex3 = BaseExercise(
            "count10_ex_03", "La Recta de los Números", GamePlayType.OrderSequence, 5, 6,
            "Las piedras dicen: 1, 2, 3, 4, 5... ¿Qué número sigue?",
            "1, 2, 3, 4, 5... ¿Qué sigue?",
            "Cinco piedras con números. Una piedra está vacía.",
            "Después del 5 viene el 6.",
            "Cuenta: uno, dos, tres, cuatro, cinco, seis.",
            "¡Bien! Después del 5 viene el 6.");
        for (int i = 1; i <= 5; i++)
        {
            AddItem(ex3, false, $"count10_stone{i}", $"Piedra {i}", $"🪨 {i}", $"Piedra con el número {i}");
        }
        AddItem(ex3, false, "count10_stone6", "Piedra vacía", "❓", "Piedra vacía esperando un número");
        AddOptions(ex3, 6, ("5", 5), ("6", 6), ("7", 7));
        topic.Exercises.Add(ex3);

        var ex4 = BaseExercise(
            "count10_ex_04", "Más o Menos", GamePlayType.Comparison, 5, 6,
            "¿Dónde hay más manzanas?",
            "¿Dónde hay más manzanas?",
            "Un grupo tiene siete manzanas. El otro tiene cinco.",
            "Cuenta las manzanas de cada grupo.",
            "El grupo A tiene 7. El grupo B tiene 5. 7 es más.",
            "¡Bien! El grupo A tiene más manzanas.");
        for (int i = 1; i <= 7; i++)
        {
            AddItem(ex4, false, $"count10_apple_a{i}", $"Manzana A{i}", "🍎", $"Manzana del grupo A número {i}");
        }
        for (int i = 1; i <= 5; i++)
        {
            AddItem(ex4, true, $"count10_apple_b{i}", $"Manzana B{i}", "🍎", $"Manzana del grupo B número {i}");
        }
        AddOptions(ex4, 1, ("El grupo A", 1), ("El grupo B", 2), ("Igual", 0));
        topic.Exercises.Add(ex4);

        var ex5 = Combine(
            "count10_ex_05", "Cinco y Cinco", 5, 6, 5, 5, "⭐", "estrella", "estrellas",
            "Hay 5 estrellas y 5 estrellas. ¿Cuántas estrellas hay?",
            "5 estrellas y 5 estrellas. ¿Cuántas hay?",
            "Dos grupos de cinco estrellas.",
            "Cuenta todas las estrellas.",
            "5 y 5 son 10. Hay 10 estrellas.",
            "¡Muy bien! Hay 10 estrellas.");
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildAddSubtractTopic()
    {
        var topic = CreateTopic(
            "kinder_add_sub_01",
            "Junto y Quito",
            5,
            6,
            "Ada la Ardilla",
            "Soy Ada la Ardilla. Vamos a juntar y quitar cosas.",
            "Soy Ada. Vamos a juntar y quitar cosas.",
            "Juntar es sumar. Quitar es restar. Cuenta para saber cuántos hay.",
            "Juntar es sumar. Quitar es restar.",
            "¡Muy bien! Ya juntas y quitas cosas.",
            "Juntar cantidades", "Quitar cantidades");

        var ex1 = Combine(
            "addsub_ex_01", "La Cesta de Peras", 5, 6, 2, 2, "🍐", "pera", "peras",
            "Hay 2 peras y 2 peras. ¿Cuántas peras hay?",
            "2 peras y 2 peras. ¿Cuántas hay?",
            "Dos peras en una cesta y dos peras en otra.",
            "Cuenta todas las peras.",
            "2 y 2 son 4. Hay 4 peras.",
            "¡Muy bien! Hay 4 peras.");
        topic.Exercises.Add(ex1);

        var ex2 = Combine(
            "addsub_ex_02", "Los Globos", 5, 6, 3, 3, "🎈", "globo", "globos",
            "Hay 3 globos y 3 globos. ¿Cuántos globos hay?",
            "3 globos y 3 globos. ¿Cuántos hay?",
            "Dos grupos de tres globos.",
            "Cuenta todos los globos.",
            "3 y 3 son 6. Hay 6 globos.",
            "¡Muy bien! Hay 6 globos.");
        topic.Exercises.Add(ex2);

        var ex3 = TakeAway(
            "addsub_ex_03", "Las Galletas", 5, 6, 5, 2, "🍪", "galleta", "galletas",
            "Hay 5 galletas. Se comen 2. ¿Cuántas galletas quedan?",
            "Hay 5 galletas. Se comen 2. ¿Cuántas quedan?",
            "Quedan tres galletas y dos se van.",
            "Cuenta las galletas que quedan.",
            "5 menos 2 son 3. Quedan 3 galletas.",
            "¡Muy bien! Quedan 3 galletas.");
        topic.Exercises.Add(ex3);

        var ex4 = TakeAway(
            "addsub_ex_04", "Los Pájaros que Se Van", 5, 6, 4, 2, "🐦", "pájaro", "pájaros",
            "Hay 4 pájaros. 2 se van. ¿Cuántos pájaros quedan?",
            "Hay 4 pájaros. 2 se van. ¿Cuántos quedan?",
            "Quedan dos pájaros y dos se van.",
            "Cuenta los pájaros que quedan.",
            "4 menos 2 son 2. Quedan 2 pájaros.",
            "¡Muy bien! Quedan 2 pájaros.");
        topic.Exercises.Add(ex4);

        var ex5 = Combine(
            "addsub_ex_05", "La Tienda de Jugos", 5, 6, 3, 4, "🧃", "jugo", "jugos",
            "Hay 3 jugos y 4 jugos. ¿Cuántos jugos hay?",
            "3 jugos y 4 jugos. ¿Cuántos hay?",
            "Tres jugos en una caja y cuatro en otra.",
            "Cuenta todos los jugos.",
            "3 y 4 son 7. Hay 7 jugos.",
            "¡Muy bien! Hay 7 jugos.");
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic BuildMeasureTopic()
    {
        var topic = CreateTopic(
            "kinder_measure_01",
            "Mido y Ordeno",
            5,
            6,
            "Milo el Ratón",
            "Soy Milo el Ratón. Vamos a medir y ordenar cosas.",
            "Soy Milo. Vamos a medir y ordenar.",
            "Puedes comparar largo, corto, pesado y ligero. También puedes ordenar del primero al último.",
            "Puedes comparar largo, corto, pesado y ligero.",
            "¡Muy bien! Ya comparas y ordenas cosas.",
            "Comparar longitudes", "Comparar pesos", "Ordenar del primero al último");

        var ex1 = BaseExercise(
            "measure_ex_01", "La Serpiente Larga", GamePlayType.Comparison, 5, 6,
            "¿Cuál es más largo?",
            "¿Cuál es más largo?",
            "Una serpiente larga y un gusano corto.",
            "Mira cuál es más largo.",
            "La serpiente es más larga que el gusano.",
            "¡Bien! La serpiente es más larga.");
        AddItem(ex1, false, "measure_snake", "Serpiente", "🐍", "Una serpiente larga");
        AddItem(ex1, true, "measure_worm", "Gusano", "🪱", "Un gusano corto");
        AddOptions(ex1, 1, ("La serpiente", 1), ("El gusano", 2), ("Igual", 0));
        topic.Exercises.Add(ex1);

        var ex2 = BaseExercise(
            "measure_ex_02", "La Pluma y la Piedra", GamePlayType.Comparison, 5, 6,
            "¿Qué es más ligero?",
            "¿Qué es más ligero?",
            "Una pluma ligera y una piedra pesada.",
            "Lo ligero pesa poco.",
            "La pluma pesa poco. La piedra pesa más.",
            "¡Bien! La pluma es más ligera.");
        AddItem(ex2, false, "measure_feather", "Pluma", "🪶", "Una pluma ligera");
        AddItem(ex2, true, "measure_stone", "Piedra", "🪨", "Una piedra pesada");
        AddOptions(ex2, 1, ("La pluma", 1), ("La piedra", 2), ("Igual", 0));
        topic.Exercises.Add(ex2);

        var ex3 = BaseExercise(
            "measure_ex_03", "Los Tres Vasos", GamePlayType.OrderSequence, 5, 6,
            "Si ordenas de pequeño a grande, ¿cuál va primero?",
            "De pequeño a grande, ¿cuál va primero?",
            "Hay un vaso pequeño, uno mediano y uno grande.",
            "El primero es el más pequeño.",
            "El vaso pequeño va primero.",
            "¡Bien! El vaso pequeño va primero.");
        AddItem(ex3, false, "measure_cup_small", "Vaso pequeño", "🥛", "Un vaso pequeño");
        AddItem(ex3, false, "measure_cup_medium", "Vaso mediano", "🥛", "Un vaso mediano");
        AddItem(ex3, false, "measure_cup_large", "Vaso grande", "🥛", "Un vaso grande");
        AddOptions(ex3, 1, ("El vaso pequeño", 1), ("El vaso mediano", 2), ("El vaso grande", 3));
        topic.Exercises.Add(ex3);

        var ex4 = BaseExercise(
            "measure_ex_04", "Antes y Después", GamePlayType.OrderSequence, 5, 6,
            "¿Qué pasa después de la mañana?",
            "¿Qué pasa después de la mañana?",
            "Primero sale el sol. Luego llega el mediodía. Al final llega la noche.",
            "Después de la mañana llega el mediodía.",
            "Primero la mañana. Luego el mediodía.",
            "¡Bien! Después de la mañana viene el mediodía.");
        AddItem(ex4, false, "measure_morning", "Mañana", "🌅", "El sol de la mañana");
        AddItem(ex4, false, "measure_noon", "Mediodía", "☀️", "El sol del mediodía");
        AddItem(ex4, false, "measure_night", "Noche", "🌙", "La luna de la noche");
        AddOptions(ex4, 2, ("La noche", 1), ("El mediodía", 2), ("La mañana", 3));
        topic.Exercises.Add(ex4);

        var ex5 = BaseExercise(
            "measure_ex_05", "Los Tres Lápices", GamePlayType.OrderSequence, 5, 6,
            "Si ordenas de corto a largo, ¿cuál va al final?",
            "De corto a largo, ¿cuál va al final?",
            "Hay un lápiz corto, uno mediano y uno largo.",
            "El último es el más largo.",
            "El lápiz largo va al final.",
            "¡Bien! El lápiz largo va al final.");
        AddItem(ex5, false, "measure_pencil_short", "Lápiz corto", "✏️", "Un lápiz corto");
        AddItem(ex5, false, "measure_pencil_medium", "Lápiz mediano", "✏️", "Un lápiz mediano");
        AddItem(ex5, false, "measure_pencil_long", "Lápiz largo", "✏️", "Un lápiz largo");
        AddOptions(ex5, 3, ("El corto", 1), ("El mediano", 2), ("El largo", 3));
        topic.Exercises.Add(ex5);

        return topic;
    }

    private static PedagogicalTopic CreateTopic(
        string id,
        string title,
        int ageMin,
        int ageMax,
        string character,
        string intro,
        string introSimplified,
        string concept,
        string conceptSimplified,
        string congratulation,
        params string[] principles) => new()
    {
        Id = id,
        Title = title,
        Level = AcademicLevel.Kindergarten,
        TargetAgeMin = ageMin,
        TargetAgeMax = ageMax,
        CharacterName = character,
        NarrativeIntro = intro,
        NarrativeIntroSimplified = introSimplified,
        CoreConceptExplanation = concept,
        CoreConceptSimplified = conceptSimplified,
        PrinciplesAddressed = new List<string>(principles),
        CongratulationMessage = congratulation
    };

    private static Exercise BaseExercise(
        string id,
        string title,
        GamePlayType gameType,
        int ageMin,
        int ageMax,
        string instruction,
        string simplifiedInstruction,
        string screenReaderDescription,
        string firstHint,
        string secondHint,
        string success) => new()
    {
        Id = id,
        Title = title,
        GameType = gameType,
        TargetAgeMin = ageMin,
        TargetAgeMax = ageMax,
        Level = AcademicLevel.Kindergarten,
        StandardInstruction = instruction,
        SimplifiedInstruction = simplifiedInstruction,
        ScreenReaderDescription = screenReaderDescription,
        FirstHint = firstHint,
        SecondHintDetailed = secondHint,
        ExplanationOfSuccess = success,
        SimplifiedFirstHint = firstHint,
        SimplifiedSecondHint = secondHint,
        SimplifiedExplanationOfSuccess = success
    };

    private static Exercise Counting(
        string id,
        string title,
        int ageMin,
        int ageMax,
        int count,
        string emoji,
        string singular,
        string plural,
        string instruction,
        string simplifiedInstruction,
        string screenReaderDescription,
        string firstHint,
        string secondHint,
        string success)
    {
        var exercise = BaseExercise(
            id, title, GamePlayType.InteractiveCounting, ageMin, ageMax,
            instruction, simplifiedInstruction, screenReaderDescription,
            firstHint, secondHint, success);

        for (int i = 1; i <= count; i++)
        {
            AddItem(exercise, false, $"{id}_item{i}", $"{singular} {i}", emoji, $"{singular} número {i}");
        }

        AddOptions(
            exercise,
            count,
            (NumberLabel(count - 1, singular, plural), count - 1),
            (NumberLabel(count, singular, plural), count),
            (NumberLabel(count + 1, singular, plural), count + 1));

        return exercise;
    }

    private static Exercise Combine(
        string id,
        string title,
        int ageMin,
        int ageMax,
        int groupA,
        int groupB,
        string emoji,
        string singular,
        string plural,
        string instruction,
        string simplifiedInstruction,
        string screenReaderDescription,
        string firstHint,
        string secondHint,
        string success)
    {
        var exercise = BaseExercise(
            id, title, GamePlayType.AdditionCombine, ageMin, ageMax,
            instruction, simplifiedInstruction, screenReaderDescription,
            firstHint, secondHint, success);

        for (int i = 1; i <= groupA; i++)
        {
            AddItem(exercise, false, $"{id}_a{i}", $"{singular} A{i}", emoji, $"{singular} del primer grupo número {i}");
        }
        for (int i = 1; i <= groupB; i++)
        {
            AddItem(exercise, true, $"{id}_b{i}", $"{singular} B{i}", emoji, $"{singular} del segundo grupo número {i}");
        }

        int total = groupA + groupB;
        AddOptions(
            exercise,
            total,
            (NumberLabel(total - 1, singular, plural), total - 1),
            (NumberLabel(total, singular, plural), total),
            (NumberLabel(total + 1, singular, plural), total + 1));

        return exercise;
    }

    private static Exercise TakeAway(
        string id,
        string title,
        int ageMin,
        int ageMax,
        int total,
        int removed,
        string emoji,
        string singular,
        string plural,
        string instruction,
        string simplifiedInstruction,
        string screenReaderDescription,
        string firstHint,
        string secondHint,
        string success)
    {
        var exercise = BaseExercise(
            id, title, GamePlayType.SubtractionTakeAway, ageMin, ageMax,
            instruction, simplifiedInstruction, screenReaderDescription,
            firstHint, secondHint, success);

        int remaining = total - removed;
        for (int i = 1; i <= remaining; i++)
        {
            AddItem(exercise, false, $"{id}_remain{i}", $"{singular} {i}", emoji, $"{singular} que queda número {i}");
        }
        for (int i = 1; i <= removed; i++)
        {
            AddItem(exercise, true, $"{id}_gone{i}", $"{singular} que se va {i}", emoji, $"{singular} que se va número {i}");
        }

        AddOptions(
            exercise,
            remaining,
            (NumberLabel(remaining - 1, singular, plural), remaining - 1),
            (NumberLabel(remaining, singular, plural), remaining),
            (NumberLabel(remaining + 1, singular, plural), remaining + 1));

        return exercise;
    }

    private static Exercise Choice(
        string id,
        string title,
        GamePlayType gameType,
        int ageMin,
        int ageMax,
        (string Id, string Name, string Emoji, string Desc)[] items,
        string instruction,
        string simplifiedInstruction,
        string screenReaderDescription,
        string firstHint,
        string secondHint,
        string success,
        int correctValue,
        params (string Label, int Value)[] options)
    {
        var exercise = BaseExercise(
            id, title, gameType, ageMin, ageMax,
            instruction, simplifiedInstruction, screenReaderDescription,
            firstHint, secondHint, success);

        foreach (var item in items)
        {
            AddItem(exercise, false, item.Id, item.Name, item.Emoji, item.Desc);
        }

        AddOptions(exercise, correctValue, options);
        return exercise;
    }

    private static void AddItem(
        Exercise exercise,
        bool groupB,
        string id,
        string name,
        string emoji,
        string description)
    {
        var items = groupB ? exercise.ItemsGroupB : exercise.ItemsGroupA;
        items.Add(new ManipulableItem
        {
            Id = id,
            Name = name,
            EmojiSymbol = emoji,
            AccessibleDescription = description,
            OrderIndex = items.Count + 1,
            MusicalPitchHz = Frequencies[items.Count % Frequencies.Length]
        });
    }

    private static void AddOptions(
        Exercise exercise,
        int correctValue,
        params (string Label, int Value)[] options)
    {
        for (int i = 0; i < options.Length; i++)
        {
            bool isCorrect = options[i].Value == correctValue;
            exercise.Options.Add(new ExerciseOption
            {
                Id = $"opt{i + 1}",
                Label = options[i].Label,
                Value = options[i].Value,
                IsCorrect = isCorrect,
                SpeechFeedback = isCorrect ? "¡Muy bien!" : "Prueba otra vez. Mira bien.",
                AccessibleKeyShortcut = (i + 1).ToString()
            });
        }

        exercise.ExpectedResult = correctValue;
    }

    private static string NumberLabel(int value, string singular, string plural) =>
        $"{value} {(value == 1 ? singular : plural)}";
}
