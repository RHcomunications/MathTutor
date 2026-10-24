using System.Collections.Generic;
using MathTutor.Core.Models;

namespace MathTutor.Core.Engine;

/// <summary>
/// Catálogo de pistas y explicaciones en lenguaje sencillo (sin tecnicismos)
/// por tipo de juego. Sirve como respaldo para los temas de secundaria,
/// preparatoria y programación cuando el ejercicio no trae variantes
/// simplificadas escritas a mano, garantizando que los alumnos que prefieren
/// lenguaje simple (discapacidad intelectual o preferencia explícita)
/// reciban orientación comprensible (DUA y WCAG 3.1).
/// </summary>
public static class SimplifiedHintCatalog
{
    private static readonly IReadOnlyDictionary<GamePlayType, SimpleHints> Catalog =
        new Dictionary<GamePlayType, SimpleHints>
    {
        [GamePlayType.InteractiveCounting] = new(
            "Toca cada objeto una sola vez, en orden, hasta llegar al final.",
            "Cuenta una vez cada objeto y no dejes ninguno fuera: el último número que digas es el total.",
            "¡Muy bien! Contaste todos los objetos y ninguno dos veces. ¡Así se sabe exactamente cuántos hay!"),
        [GamePlayType.OrderSequence] = new(
            "Sigue los números en orden: uno tras otro, sin brincarte ninguno.",
            "Los números van de uno en uno. Encuentra el que falta para completar la serie.",
            "¡Genial! Siguiendo el orden, la serie quedó completa de principio a fin."),
        [GamePlayType.Comparison] = new(
            "Mira a cada lado de la balanza: cuenta lo que hay en cada platillo y compara.",
            "El lado que tiene más objetos baja abajo. El lado con menos se queda arriba.",
            "¡Bien! Cuando comparas, primero cuentas cada lado y después decides cuál tiene más."),
        [GamePlayType.AdditionCombine] = new(
            "Junta los dos grupos en una sola cesta y cuenta todo.",
            "Primero cuenta un grupo y luego empieza a contar desde ese número con el otro grupo.",
            "¡Excelente! Al juntar dos grupos, el total es lo mismo que sumar los números de cada grupo."),
        [GamePlayType.CommutativePropertyDemo] = new(
            "Cambia el orden de los dos grupos y vuelve a contar: dará lo mismo.",
            "3 más 2 da lo mismo que 2 más 3. Cambiar el orden no cambia el total.",
            "¡Estupendo! Cambiar el orden al sumar siempre da el mismo resultado."),
        [GamePlayType.AssociativePropertyDemo] = new(
            "Agrupa primero dos vagones y después agrega el tercero. Prueba otro orden y compara.",
            "Cuando juntas los mismos tres grupos, no importa con cuál empieces: el total no cambia.",
            "¡Bravo! Al agrupar de distintas maneras, la suma se mantiene igual."),
        [GamePlayType.ZeroNeutralProperty] = new(
            "Mira la caja vacía: no tiene nada para agregar ni para quitar.",
            "Sumar cero no agrega nada y restar cero no quita nada: el número queda igualito.",
            "¡Genial! Cero no cambia nada. El número se quedó exactamente como estaba."),
        [GamePlayType.SubtractionTakeAway] = new(
            "Cuenta los objetos que se van. Lo que queda es la respuesta.",
            "Empieza en el número grande y regresa uno a uno las veces que se salieron.",
            "¡Muy bien! Al quitar, lo que queda es menos que al principio."),
        [GamePlayType.InverseRelation] = new(
            "La resta deshace lo que hizo la suma. Vuelve al número donde todo empezó.",
            "Si 4 más 3 son 7, entonces de 7 quitas 3 y regresas al 4. Son caminos opuestos.",
            "¡Perfecto! La suma y la resta se deshacen entre sí y vuelven al punto de partida."),
        [GamePlayType.PatternAndLoopSeed] = new(
            "El robot repite la misma orden varias veces. Cuenta los saltos iguales.",
            "Suma el mismo número las veces que ordena el bucle: 2 + 2 + 2 son tres saltos iguales.",
            "¡Excelente! Repetir el mismo salto varias veces es una suma rápida."),
        [GamePlayType.MultiplicationArray] = new(
            "Hay filas iguales con varios objetos cada una. Cuenta una fila y multiplícala por el número de filas.",
            "Por ejemplo, 2 filas de 3 manzanas: 3 + 3 = 6. La multiplicación es una suma repetida.",
            "¡Espectacular! Las filas iguales se resuelven rápido multiplicando: no hay que contar una por una."),
        [GamePlayType.DivisionFairSharing] = new(
            "Reparte sobrantes uno a uno hasta que nadie sobre.",
            "Da una pieza a cada amigo, luego otra y otra más, hasta que las piezas se acaben.",
            "¡Justo! Todos quedaron con la misma cantidad y no sobró nada. Eso es dividir en partes iguales."),
        [GamePlayType.FractionParts] = new(
            "La de abajo dice en cuántas partes se cortó; la de arriba dice cuántas se usaron.",
            "La fracción se lee: arriba (tomadas) sobre abajo (totales). Cuenta las tomadas.",
            "¡Genial! El numerador es lo que tomaste y el denominador es el total del corte."),
        [GamePlayType.FractionEquivalence] = new(
            "Compara las dos barras: si cubren el mismo espacio, las fracciones son iguales.",
            "Al multiplicar de arriba y de abajo por el mismo número, la fracción NO cambia de tamaño.",
            "¡Exacto! Son la misma cantidad aunque se escriban con números distintos."),
        [GamePlayType.MultiplicationDistributive] = new(
            "Parte el número grande en decenas y unidades. Multiplica cada parte y después suma.",
            "Primero multiplica la parte de las decenas, después la de las unidades, y al final suma las dos partes.",
            "¡Perfecto! Partir el número en partes pequeñas hace la multiplicación más fácil: después se junta todo."),
        [GamePlayType.MixedFractions] = new(
            "Cada entero son pedazos completos y además hay un pedacito suelto.",
            "Divide cada entero en las mismas partes que indica el número de abajo de la fracción y cuenta todas las partes iguales: enteros por el número de abajo, más las partes extra.",
            "¡Excelente! El número mixto significa un entero más un pedacito: junta todas las partes iguales en una sola fracción."),
        [GamePlayType.DecimalsMoney] = new(
            "Piensa en monedas: la de peso entero vale 100 centavos y la de medio vale 50 centavos.",
            "Empieza por los pesos enteros y luego suma las monedas pequeñas.",
            "¡Muy bien! El punto decimal separa los pesos completos de los centavos."),
        [GamePlayType.PowersAndExponentGrowth] = new(
            "El número pequeño dice cuántas veces multiplicas la base por sí misma.",
            "2² significa 2 por 2. Se multiplica la base el número de veces que marca el exponente.",
            "¡Estupendo! El exponente es un atajo de repetición: multiplicar la base por sí misma."),
        [GamePlayType.DirectProportion] = new(
            "Si una cantidad crece, la otra crece en la misma proporción.",
            "Primero descubre cuánto da cada unidad y después multiplica por la cantidad final.",
            "¡Bravo! La proporción directa significa: si uno crece, el otro crece igual de rápido."),
        [GamePlayType.AlgebraicBalance] = new(
            "La balanza está equilibrada; lo mismo debe haber en cada lado.",
            "Quítale lo mismo a ambos lados hasta que la caja X quede sola. La balanza sigue equilibrada.",
            "¡Genial! Al hacer lo mismo en los dos lados, la balanza no se rompe y X se revela."),
        [GamePlayType.SquareRootPerfect] = new(
            "Busca un número que multiplicado por sí mismo dé el área total.",
            "El área son los cuadritos de un cuadrado. La respuesta es el lado del cuadrado.",
            "¡Perfecto! La raíz cuadrada regresa el lado del cuadrado perfecto."),
        [GamePlayType.GeometryAnglesClassification] = new(
            "90 grados es una esquina recta como un libro. Menos de 90 es agudo y más de 90 es obtuso.",
            "Una esquina cuadrada mide 90 grados. Si es afilada es aguda; si es abierta es obtusa.",
            "¡Exacto! Reconociste la abertura del ángulo: 90° es recto, menos es agudo y más es obtuso."),
        [GamePlayType.GeometryArea2D] = new(
            "El área es cuántos cuadritos llenan el piso. Multiplica base por altura.",
            "En un rectángulo multiplica base por altura. En un triángulo calcula la mitad de ese resultado.",
            "¡Excelente! Calculaste el área contando las unidades cuadradas de la superficie."),
        [GamePlayType.GeometryVolume3D] = new(
            "El volumen es cuánto espacio hay adentro de una caja 3D. Multiplica largo por ancho por alto.",
            "Cuenta los cubitos de la base y multiplícalos por la cantidad de pisos de alto.",
            "¡Brillante! Calculaste el volumen multiplicando las tres dimensiones en metros cúbicos."),
        [GamePlayType.GeometryPerimeter] = new(
            "El perímetro es la cerca de afuera. Suma todos los lados del borde.",
            "Camina alrededor de la figura y suma la medida de cada lado: lado + lado + lado.",
            "¡Excelente! El perímetro es la suma de todas las orillas exteriores de la figura."),
        [GamePlayType.TimeClockReading] = new(
            "La manecilla corta marca la hora; la manecilla larga marca los minutos.",
            "Mira la aguja corta para saber la hora. La aguja larga cuenta minutos de 5 en 5.",
            "¡Genial! Leíste la hora y los minutos con gran precisión."),
        [GamePlayType.MoneyCurrencyCounting] = new(
            "Cuenta el valor de las monedas y billetes para saber cuánto dinero tienes.",
            "Suma primero los billetes grandes y luego las monedas para pagar o dar cambio.",
            "¡Muy bien! Calculaste el total de dinero exacto para la compra."),
        [GamePlayType.MeasurementRulerLength] = new(
            "Pon la regla empezando en el cero (0) y mira hasta qué número llega el objeto.",
            "Asegúrate de que la punta empiece en el 0. El número final son los centímetros.",
            "¡Exacto! Mediste la longitud en centímetros desde el cero de la regla."),
        [GamePlayType.DivisionWithRemainder] = new(
            "Reparte en partes iguales. Lo que no alcanza para todos es lo que sobra (residuo).",
            "Divide hasta donde se pueda de forma exacta. Cuenta cuántas piezas quedaron sin repartir.",
            "¡Excelente! Encontraste cuántas tocan a cada uno y cuánto sobra de residuo."),
        [GamePlayType.FractionsNumberLine] = new(
            "Corta el camino de 0 a 1 en partes iguales y da los saltos que dice el número de arriba.",
            "El número de abajo dice los cortes entre enteros; el de arriba dice cuántos saltos avanzas.",
            "¡Brillante! Ubicaste la fracción en su lugar exacto de la recta numérica."),
        [GamePlayType.DataBarChartsAndTables] = new(
            "La barra más alta es la que tiene más cantidad. Mira el número donde llega el tope.",
            "Compara las alturas de las barras o lee la tabla para encontrar la categoría mayor o menor.",
            "¡Muy bien! Leíste los datos de la gráfica de barras correctamente."),
        [GamePlayType.StatisticsCentralTendency] = new(
            "El promedio se saca sumando todo y dividiendo entre cuántos son. La moda es la que más se repite.",
            "Suma los números y divide entre el total de datos para la media; la mediana es el dato del centro.",
            "¡Extraordinario! Calculaste la medida estadística exacta del conjunto de datos."),
        [GamePlayType.CircleGeometryPi] = new(
            "El diámetro es el doble del radio (d = 2r). El perímetro es pi por diámetro (C = π × d).",
            "Multiplica el diámetro por pi (3.14) para el contorno del círculo, o pi por radio al cuadrado para el área.",
            "¡Excelente! Calculaste las medidas circulares con el poder de pi."),
        [GamePlayType.FactorsMcmMcd] = new(
            "MCM es el múltiplo común más pequeño donde coinciden; MCD es el divisor común más grande que cabe en ambos.",
            "Para coincidencias de tiempo usa el MCM; para cortar en trozos iguales sin que sobre usa el MCD.",
            "¡Brillante! Encontraste el factor común exacto entre los números."),
        [GamePlayType.NegativeRationalNumbers] = new(
            "En la recta numérica, los números negativos van a la izquierda del cero. Menos por menos da más.",
            "Ubica la fracción o decimal a la izquierda del 0 si es negativo; más a la izquierda significa menor valor.",
            "¡Extraordinario! Dominas los números racionales positivos y negativos en la recta."),
        [GamePlayType.AlgebraFactoring] = new(
            "Factorizar es desarmar en una multiplicación. Saca lo que se repite afuera del paréntesis.",
            "Para factor común busca el término compartido: 3x + 6 = 3(x + 2). Para diferencia de cuadrados: x² - 9 = (x - 3)(x + 3).",
            "¡Brillante! Descompusiste la expresión algebraica en sus factores primos exactos."),
        [GamePlayType.ParallelLinesAngles] = new(
            "Las paralelas cortadas por una secante forman ángulos iguales: los opuestos y los alternos son gemelos.",
            "Si dos líneas son paralelas, los ángulos alternos internos son iguales; los suplementarios suman 180°.",
            "¡Excelente! Descifraste la geometría de las paralelas y secantes cósmicas."),
        [GamePlayType.ProbabilityCompoundEvents] = new(
            "La probabilidad es casos favorables entre casos posibles totales (Regla de Laplace).",
            "Cuenta cuántos resultados cumplen la condición y divide entre todos los resultados que pueden salir.",
            "¡Extraordinario! Calculaste la probabilidad matemática exacta del suceso."),
        [GamePlayType.ConicsParabolaEllipse] = new(
            "En la parábola y² = 4px, la distancia al foco es p. En la elipse x²/a² + y²/b² = 1, a es el semieje mayor.",
            "Para la parábola divide el coeficiente entre 4 (4p => p). Para la elipse extrae la raíz cuadrada de a².",
            "¡Extraordinario! Dominas las curvas cónicas analíticas de la astronomía y el diseño."),
        [GamePlayType.CalculusLimits] = new(
            "Un límite es el valor al que se acerca la función cuando x se aproxima a un número.",
            "Sustituye el valor de x en la función; si da un número directo, ese es el valor del límite.",
            "¡Brillante! Calculaste el límite y demostraste la continuidad matemática."),
        [GamePlayType.CalculusDerivativeRules] = new(
            "Regla de la potencia: baja el exponente multiplicando y réstale 1 al exponente (d/dx xⁿ = n·xⁿ⁻¹).",
            "Multiplica el coeficiente por el exponente y baja en uno la potencia; la derivada de un número suelto es 0.",
            "¡Extraordinario! Dominas las reglas operativas de derivación del cálculo diferencial."),
        [GamePlayType.CartesianCoordinates] = new(
            "En el par (X, Y): primero horizontal (X), después vertical (Y).",
            "El primer número del par señala X y el segundo señala Y. Localiza la coordenada pedida.",
            "¡Exacto! En el par (X, Y), localizar el orden da el lugar: derecha según X y arriba según Y."),
        [GamePlayType.SignedNumbersAddition] = new(
            "Los positivos son dinero tuyo y los negativos deudas. Quita la deuda del dinero.",
            "Empieza en la marca positiva y camina hacia la izquierda tantas veces como la deuda.",
            "¡Bien! Al sumar deuda, caminas la recta a la izquierda. Saldo negativo significa que debes algo."),
        [GamePlayType.OrderOfOperationsPEMDAS] = new(
            "Primero la multiplicación, después la suma. Es la regla del orden de operaciones.",
            "Resuelve primero las operaciones fuertes (paréntesis, multiplicación, división) y luego las sencillas.",
            "¡Correcto! El orden de las operaciones es MUY importante: puede cambiar el resultado."),
        [GamePlayType.LinearEquationAXB] = new(
            "La caja X está sola: quita a los dos lados lo que sobra hasta dejarla sola.",
            "Resta lo que suma y divide lo que multiplica, siempre en los dos lados. X queda sola y su valor aparece.",
            "¡Excelente! Despejar X significa operaciones inversas en ambos lados hasta dejar la X sola."),
        [GamePlayType.PercentageCalculation] = new(
            "El porcentaje es una porción de cada cien piezas. Encuentra cuánto vale por cada 100.",
            "Considera 20% como 20 de cada 100. Multiplica la porción por la cantidad de piezas.",
            "¡Genial! Un porcentaje es la parte proporcional del todo."),
        [GamePlayType.ExponentLaws] = new(
            "Con la misma base, multiplicar suma los exponentes y dividir los resta.",
            "Cuenta cuántas veces aparece la base en cada factor: suma las repeticiones y ese es el nuevo exponente.",
            "¡Exacto! La ley de los exponentes simplifica operaciones repetidas de la misma base."),
        [GamePlayType.PolynomialLikeTerms] = new(
            "Solo se pueden juntar piezas idénticas: mismas letras y mismo exponente.",
            "Las iguales se agrupan: 3x y 4x suman 7x. Las piezas distintas quedan separadas.",
            "¡Bravo! Al sumar términos semejantes, la expresión queda más compacta."),
        [GamePlayType.LinearSystem2x2] = new(
            "Tenemos dos ecuaciones y dos letras desconocidas: haz que una letra se quite para resolver la otra.",
            "Resta una ecuación de la otra y una letra se cancela sola. Después calcula la restante.",
            "¡Bien! Con dos ecuaciones y dos desconocidos, por sustitución o resta se descubre todo."),
        [GamePlayType.PythagoreanTheorem] = new(
            "El lado más largo del triángulo rectángulo se calcula con la fórmula a² + b² = c².",
            "Eleva al cuadrado los dos lados menores, súmalos y saca la raíz cuadrada para obtener el lado mayor.",
            "¡Correcto! El teorema de Pitágoras conecta los tres lados de un triángulo rectángulo."),
        [GamePlayType.QuadraticEquations] = new(
            "Si la X² está sola, saca la raíz cuadrada de lo que queda en el otro lado.",
            "Despeja X² primero y después aplica la raíz cuadrada en ambos lados. Ojo: hay dos respuestas posibles.",
            "¡Estupendo! Cuadrática significa X al cuadrado: hay dos caminos que la resuelven."),
        [GamePlayType.TriangleSimilarityThales] = new(
            "Si el triángulo pequeño se hace grande, todos sus lados crecen por el mismo factor.",
            "Divide la sombra del árbol entre la sombra de la persona y multiplica por la altura de la persona.",
            "¡Excelente! La proporción entre lado y sombra se repite en el triángulo grande."),
        [GamePlayType.TrigonometricRatios] = new(
            "El seno divide el lado frente al ángulo entre el lado más largo.",
            "Primero identifica el lado que pide la pregunta (frente al ángulo o pegado al ángulo) y al lado más largo.",
            "¡Genial! Las razones trigonométricas son proporciones entre dos lados de un triángulo rectángulo."),
        [GamePlayType.QuadraticFunctionCurve] = new(
            "Sustituye X en la fórmula de la parábola y calcula paso a paso.",
            "Primero resuelve X², luego multiplica por el coeficiente a y después suma el número suelto.",
            "¡Perfecto! La parábola se evalúa reemplazando X con el valor dado y siguiendo la fórmula."),
        [GamePlayType.AnalyticDistance2D] = new(
            "Camina como si fuera un triángulo rectángulo: mide las diferencias horizontal y vertical por separado.",
            "Resta X de ambos puntos, resta Y de ambos puntos, eleva cada diferencia al cuadrado y saca la raíz.",
            "¡Bravo! La distancia en el plano viene de la fórmula de Pitágoras aplicada a las diferencias."),
        [GamePlayType.LineSlopeM] = new(
            "La pendiente mide cuánto sube la recta por cada paso hacia la derecha.",
            "Divide el cambio vertical entre el cambio horizontal entre dos puntos de la recta.",
            "¡Genial! La pendiente mide cuánto creció Y por cada paso que creció X."),
        [GamePlayType.LinearEquationYMXB] = new(
            "Multiplica la pendiente por los kilómetros y suma el cobro fijo.",
            "En y = mx + b, primero se multiplica m por x y después se suma b (el costo fijo).",
            "¡Exacto! La recta une un cobro por unidad (m) más un precio inicial (b)."),
        [GamePlayType.ConicCircleRadius] = new(
            "En x² + y² = r², el número del lado derecho es el cuadrado del radio.",
            "La ecuación muestra el radio al cuadrado. Para el radio, saca la raíz cuadrada del número.",
            "¡Bien! El radio del círculo es la raíz del número de la ecuación."),
        [GamePlayType.SineWaveContinuous] = new(
            "La altura máxima de la onda es el número que multiplica al seno.",
            "El seno alcanza su pico (1), por eso la amplitud es el coeficiente de la onda.",
            "¡Estupendo! La amplitud de una onda senoidal es su cresta: el valor máximo alcanzable."),
        [GamePlayType.LogarithmBinary] = new(
            "Cuenta cuántas veces multiplicas el 2 por sí mismo para llegar al número.",
            "El logaritmo base 2 responde: ¿2 elevado a qué número da este valor?",
            "¡Correcto! El logaritmo cuenta los pasos de doble en doble hasta llegar."),
        [GamePlayType.CalculusDerivativeRate] = new(
            "La derivada da la velocidad exacta en cada instante, no el promedio.",
            "Para la derivada de t², baja el exponente al frente: queda 2t. Después evalúa en t dado.",
            "¡Genial! La derivada dice exactamente cómo cambia el movimiento en un instante."),
        [GamePlayType.CalculusIntegralArea] = new(
            "La integral acumula: suma pequeñas porciones hasta llegar al área total.",
            "Con una velocidad constante, el área es un rectángulo: base por altura.",
            "¡Excelente! La integral suma todo el cambio acumulado desde el inicio hasta el final."),
        [GamePlayType.ProbabilityAndStats] = new(
            "Para la media, suma todos los datos y divide entre el número de datos.",
            "El promedio reparte las piezas equitativamente entre todas las observaciones.",
            "¡Muy bien! La media es el equilibrio justo de los datos: suma total dividida entre la cantidad."),
        [GamePlayType.ProgrammingVariablesState] = new(
            "La variable es una caja con número: cada línea la actualiza.",
            "Lee el programa línea por línea y reemplaza el valor viejo por el nuevo cada vez.",
            "¡Perfecto! Seguir los pasos de una variable es como salir de una caja en cada línea."),
        [GamePlayType.ProgrammingConditionalsIfElse] = new(
            "Prueba la condición del SI: si es verdadera, se sigue un camino; si no, el otro.",
            "Compara el número con el umbral para elegir el bono correcto y saca la suma final.",
            "¡Exacto! El if/else es una bifurcación: la condición determina qué camino activar."),
        [GamePlayType.ProgrammingBooleanLogic] = new(
            "El operador Y solo es verdadero si ambas entradas valen 1.",
            "El Y (AND) exige que TODO sea verdadero: dos interruptores encendidos al mismo tiempo.",
            "¡Genial! La compuerta lógica Y se activa únicamente con todas las señales activas."),
        [GamePlayType.ProgrammingLoopsForWhile] = new(
            "El ciclo repite lo mismo varias veces: suma cada vuelta al acumulador.",
            "Multiplica el paso por las veces que se repite el ciclo y así sale el valor final.",
            "¡Excelente! El bucle permite repetir y acumular sin escribir muchas líneas."),
        [GamePlayType.ProgrammingArraysZeroIndex] = new(
            "En un arreglo, la primera caja es la 0, la segunda es la 1, la tercera es la 2...",
            "El índice comienza en 0. Para pedir un elemento, usa su número de índice corregido.",
            "¡Exacto! La numeración de cero es la convención de computación: el primer elemento es el índice 0."),
        [GamePlayType.ProgrammingFunctionsReturn] = new(
            "La función es una receta: entra un número, sale otro número según la regla.",
            "Sustituye el argumento en la fórmula de la función y calcula el retorno.",
            "¡Estupendo! La función toma el argumento, lo pasa por la receta y devuelve el valor."),
        [GamePlayType.ProgrammingAlgorithmsSearch] = new(
            "El algoritmo revisa la lista una pieza a la vez y guarda el mayor visto hasta ahora.",
            "Compara cada elemento de la lista con el máximo actual y lo reemplaza si es mejor.",
            "¡Bravo! El algoritmo de búsqueda del máximo solo conserva el más grande visto."),
        [GamePlayType.ProgrammingGameCoordinates2D] = new(
            "La posición nueva es la posición inicial más la velocidad multiplicada por los segundos.",
            "Velocidad y tiempo multiplicados dan cuánto caminó, y eso se agrega a la posición inicial.",
            "¡Genial! La física del juego es una recta: x final = x inicial + velocidad × tiempo."),
        [GamePlayType.ProgrammingAssistiveAIData] = new(
            "Suma las lecturas, divide entre cuántas hay y ajusta con la calibración.",
            "El promedio de sensores es la suma total dividida entre la cantidad de lecturas.",
            "¡Correcto! La IA asistiva promedia lecturas para conocer el valor limpio de la señal."),
    };

    private sealed record SimpleHints(string FirstHint, string SecondHint, string Success);

    /// <summary>
    /// Devuelve la pista simple para el tipo de juego, o null si el tipo no tiene respaldo.
    /// </summary>
    public static string? GetSimpleHint(GamePlayType gameType, bool detailed) =>
        Catalog.TryGetValue(gameType, out var hints)
            ? (detailed ? hints.SecondHint : hints.FirstHint)
            : null;

    /// <summary>
    /// Devuelve la explicación de éxito simplificada, o null si no existe respaldo.
    /// </summary>
    public static string? GetSimpleSuccess(GamePlayType gameType) =>
        Catalog.TryGetValue(gameType, out var hints) ? hints.Success : null;
}
