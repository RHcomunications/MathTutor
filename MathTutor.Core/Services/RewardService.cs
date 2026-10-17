using System;
using System.Collections.Generic;
using System.Linq;
using MathTutor.Core.Models;

namespace MathTutor.Core.Services;

/// <summary>
/// Servicio de gamificación, medallas y mapa del Reino Matemágico.
/// </summary>
public class RewardService
{
    public List<Badge> GetAllBadges(UserProfile profile)
    {
        var badges = new List<Badge>
        {
            new()
            {
                Id = "badge_counting",
                Title = "Guardián del Conteo",
                Description = "Dominaste los 5 secretos mágicos del conteo en el bosque.",
                IconEmoji = "🦉",
                AssociatedTopicId = "kinder_counting_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_addition",
                Title = "Alquimista de la Suma",
                Description = "Descubriste la conmutatividad, asociatividad y el poder del cero.",
                IconEmoji = "✨",
                AssociatedTopicId = "elem_addition_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_subtraction",
                Title = "Explorador del Retorno",
                Description = "Aprendiste a restar y a utilizar el camino de regreso inverso.",
                IconEmoji = "🧭",
                AssociatedTopicId = "elem_subtraction_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_multiplication",
                Title = "Arquitecto de Matrices",
                Description = "Organizaste hileras, columnas y grupos iguales con multiplicación.",
                IconEmoji = "🌾",
                AssociatedTopicId = "elem_multiplication_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_division",
                Title = "Caballero de la Equidad",
                Description = "Repartiste alimentos y gemas en partes exactamente justas.",
                IconEmoji = "⚖️",
                AssociatedTopicId = "elem_division_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_fractions",
                Title = "Maestro de las Fracciones",
                Description = "Comprendiste partes iguales, numeradores y fracciones equivalentes.",
                IconEmoji = "🍰",
                AssociatedTopicId = "elem_fractions_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_distributive",
                Title = "Estratega de la Descomposición",
                Description = "Multiplicaste cantidades mayores rompiéndolas en decenas y unidades.",
                IconEmoji = "🧩",
                AssociatedTopicId = "elem_distributive_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_mixed_fractions",
                Title = "Chef de los Mixtos",
                Description = "Dominaste enteros y fracciones impropias mayores a la unidad.",
                IconEmoji = "🧇",
                AssociatedTopicId = "elem_mixed_fractions_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_decimals",
                Title = "Tesorero Decimal",
                Description = "Manejaste monedas de centavos y el valor de los números decimales.",
                IconEmoji = "🪙",
                AssociatedTopicId = "elem_decimals_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_powers",
                Title = "Mago del Exponente",
                Description = "Desataste el crecimiento acelerado y la duplicación de las potencias.",
                IconEmoji = "💎",
                AssociatedTopicId = "elem_powers_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_proportion",
                Title = "Alquimista Proporcional",
                Description = "Dominaste las razones constantes y la regla de tres en el laboratorio.",
                IconEmoji = "🌸",
                AssociatedTopicId = "elem_proportion_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_algebra",
                Title = "Guardián del Equilibrio",
                Description = "Despejaste incógnitas secretas manteniendo la balanza en balance.",
                IconEmoji = "⚖️",
                AssociatedTopicId = "elem_algebra_balance_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_square_root",
                Title = "Maestro de la Raíz Cuadrada",
                Description = "Descubriste el lado exacto de los templos de cuadrados perfectos.",
                IconEmoji = "🟩",
                AssociatedTopicId = "elem_square_root_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_coordinates",
                Title = "Cartógrafo del Plano",
                Description = "Ubicaste tesoros con pares ordenados en coordenadas (X, Y).",
                IconEmoji = "🗺️",
                AssociatedTopicId = "elem_cartesian_coordinates_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_signed_numbers",
                Title = "Financiero de la Recta",
                Description = "Dominaste los saldos, deudas y números enteros con signo.",
                IconEmoji = "🧾",
                AssociatedTopicId = "sec_signed_numbers_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_order_operations",
                Title = "Maestro de la Jerarquía",
                Description = "Resolviste operaciones complejas respetando PEMDAS y paréntesis.",
                IconEmoji = "🛡️",
                AssociatedTopicId = "sec_order_operations_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_linear_equations",
                Title = "Estratega Lineal",
                Description = "Despejaste ecuaciones de la forma ax + b = c como todo un profesional.",
                IconEmoji = "📐",
                AssociatedTopicId = "sec_linear_equations_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_percentages",
                Title = "Cazador de Ofertas",
                Description = "Calculaste porcentajes, descuentos e IVA con agilidad.",
                IconEmoji = "🏷️",
                AssociatedTopicId = "sec_percentages_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_exponent_laws",
                Title = "Señor de los Exponentes",
                Description = "Dominaste el producto y cociente de potencias con la misma base.",
                IconEmoji = "⚡",
                AssociatedTopicId = "sec_exponent_laws_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_polynomials",
                Title = "Domador de Polinomios",
                Description = "Agrupaste y redujiste términos semejantes en expresiones algebraicas.",
                IconEmoji = "🧩",
                AssociatedTopicId = "sec_polynomials_like_terms_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_linear_systems_2x2",
                Title = "Decodificador 2x2",
                Description = "Resolviste sistemas de dos ecuaciones lineales con dos incógnitas.",
                IconEmoji = "🔍",
                AssociatedTopicId = "sec_linear_systems_2x2_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_pythagoras",
                Title = "Héroe Pitagórico",
                Description = "Aplicaste a² + b² = c² en triángulos rectángulos y rampas del entorno.",
                IconEmoji = "📐",
                AssociatedTopicId = "sec_pythagorean_theorem_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_quadratic",
                Title = "Mago del Segundo Grado",
                Description = "Despejaste ecuaciones cuadráticas y calculaste raíces exactas en áreas.",
                IconEmoji = "🪞",
                AssociatedTopicId = "sec_quadratic_equations_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_thales",
                Title = "Explorador Solar de Tales",
                Description = "Mediste alturas monumentales utilizando sombras y semejanza de triángulos.",
                IconEmoji = "☀️",
                AssociatedTopicId = "sec_thales_similarity_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_trigonometry",
                Title = "Navegante del Seno y Coseno",
                Description = "Descubriste las razones trigonométricas y orientaste señales espaciales.",
                IconEmoji = "📡",
                AssociatedTopicId = "sec_trigonometry_ratios_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_parabola",
                Title = "Arquitecto de Parábolas",
                Description = "Modelaste curvas parabólicas y trayectorias con funciones cuadráticas y = ax².",
                IconEmoji = "⛲",
                AssociatedTopicId = "sec_quadratic_functions_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_analytic_distance",
                Title = "Topógrafo Cartesiano",
                Description = "Calculaste distancias euclidianas exactas en el plano cartesiano con d = √((Δx)² + (Δy)²).",
                IconEmoji = "📍",
                AssociatedTopicId = "prep_analytic_distance_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_line_slope",
                Title = "Ingeniero de Pendientes",
                Description = "Determinaste la razón de cambio e inclinación m = Δy / Δx en viaductos y caminos.",
                IconEmoji = "🌉",
                AssociatedTopicId = "prep_line_slope_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_line_ymxb",
                Title = "Estratega Lineal y = mx + b",
                Description = "Modelaste tarifas, costos fijos y variables con la ecuación fundamental de la recta.",
                IconEmoji = "🛺",
                AssociatedTopicId = "prep_line_equation_ymxb_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_circle_conic",
                Title = "Guardián de la Circunferencia",
                Description = "Calculaste el radio canónico r en círculos de proximidad con x² + y² = r².",
                IconEmoji = "🎯",
                AssociatedTopicId = "prep_circle_conic_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_sine_wave",
                Title = "Maestro de las Ondas",
                Description = "Modelaste señales acústicas y amplitud pico en funciones armónicas y = A · sen(x).",
                IconEmoji = "〰️",
                AssociatedTopicId = "prep_sine_wave_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_logarithm_binary",
                Title = "Explorador Logarítmico",
                Description = "Descifraste log₂(n) y la complejidad eficiente de la búsqueda binaria.",
                IconEmoji = "🌲",
                AssociatedTopicId = "prep_logarithm_binary_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_programming",
                Title = "Programador de Bucles",
                Description = "Uniste las matemáticas con los algoritmos y la computación.",
                IconEmoji = "🤖",
                AssociatedTopicId = "prog_seed_01",
                RequiredStars = 3
            },
            new()
            {
                Id = "badge_star_collector",
                Title = "Coleccionista de Estrellas",
                Description = "Acumulaste 15 o más estrellas doradas en tus misiones.",
                IconEmoji = "⭐",
                AssociatedTopicId = string.Empty,
                RequiredStars = 15
            }
        };

        foreach (var b in badges)
        {
            bool unlocked = false;
            if (!string.IsNullOrEmpty(b.AssociatedTopicId))
            {
                unlocked = profile.CompletedLessonIds.Contains(b.AssociatedTopicId);
            }
            else
            {
                unlocked = profile.StarsTotal >= b.RequiredStars;
            }

            b.IsUnlocked = unlocked;
        }

        return badges;
    }

    /// <summary>
    /// Construye las estaciones del Mapa del Reino Matemágico según el progreso del alumno.
    /// </summary>
    public List<RealmStation> GetRealmStations(UserProfile profile)
    {
        var stations = new List<RealmStation>
        {
            new()
            {
                Id = "station_1",
                Name = "El Bosque de los Números",
                Description = "La arboleda donde habitan los 5 secretos mágicos del conteo.",
                LandmarkEmoji = "🌳",
                TopicId = "kinder_counting_01",
                OrderIndex = 1
            },
            new()
            {
                Id = "station_2",
                Name = "El Castillo de la Suma",
                Description = "La fortaleza donde los números se unen y celebran sus propiedades.",
                LandmarkEmoji = "🏰",
                TopicId = "elem_addition_01",
                OrderIndex = 2
            },
            new()
            {
                Id = "station_3",
                Name = "El Sendero del Retorno",
                Description = "El camino mágico que deshace los pasos y revela la resta.",
                LandmarkEmoji = "🛤️",
                TopicId = "elem_subtraction_01",
                OrderIndex = 3
            },
            new()
            {
                Id = "station_4",
                Name = "El Gran Huerto de Matrices",
                Description = "Campos ordenados en filas y columnas donde brota la multiplicación.",
                LandmarkEmoji = "🌻",
                TopicId = "elem_multiplication_01",
                OrderIndex = 4
            },
            new()
            {
                Id = "station_5",
                Name = "El Molino de la Equidad",
                Description = "La colina donde se reparten las cosechas en partes perfectamente iguales.",
                LandmarkEmoji = "🥖",
                TopicId = "elem_division_01",
                OrderIndex = 5
            },
            new()
            {
                Id = "station_6",
                Name = "La Pastelería de las Fracciones",
                Description = "El taller donde los pasteles se dividen en partes mágicamente iguales.",
                LandmarkEmoji = "🍰",
                TopicId = "elem_fractions_01",
                OrderIndex = 6
            },
            new()
            {
                Id = "station_7",
                Name = "El Taller de los Bloques Multiplicadores",
                Description = "El laboratorio donde los números se descomponen para multiplicar con maestría.",
                LandmarkEmoji = "🧩",
                TopicId = "elem_distributive_01",
                OrderIndex = 7
            },
            new()
            {
                Id = "station_8",
                Name = "La Cocina de los Mixtos",
                Description = "El lugar donde horneamos waffles y comprendemos más de un entero.",
                LandmarkEmoji = "🧇",
                TopicId = "elem_mixed_fractions_01",
                OrderIndex = 8
            },
            new()
            {
                Id = "station_9",
                Name = "El Mercado de los Centavos",
                Description = "El bazar donde compramos con monedas y aprendemos el valor decimal.",
                LandmarkEmoji = "🪙",
                TopicId = "elem_decimals_01",
                OrderIndex = 9
            },
            new()
            {
                Id = "station_10",
                Name = "La Cueva de las Potencias",
                Description = "La cámara mágica donde los cofres duplican gemas exponencialmente.",
                LandmarkEmoji = "💎",
                TopicId = "elem_powers_01",
                OrderIndex = 10
            },
            new()
            {
                Id = "station_11",
                Name = "El Laboratorio de Alquimia",
                Description = "El taller donde los ingredientes crecen en proporción perfecta.",
                LandmarkEmoji = "🌸",
                TopicId = "elem_proportion_01",
                OrderIndex = 11
            },
            new()
            {
                Id = "station_12",
                Name = "El Templo de la Balanza Dorada",
                Description = "La balanza sagrada donde despejamos incógnitas y ecuaciones.",
                LandmarkEmoji = "⚖️",
                TopicId = "elem_algebra_balance_01",
                OrderIndex = 12
            },
            new()
            {
                Id = "station_13",
                Name = "El Laberinto de los Cuadrados Perfectos",
                Description = "El laberinto donde la raíz cuadrada revela el lado del templo.",
                LandmarkEmoji = "🟩",
                TopicId = "elem_square_root_01",
                OrderIndex = 13
            },
            new()
            {
                Id = "station_14",
                Name = "El Puerto de las Coordenadas",
                Description = "La carta de navegación que ubica tesoros mediante pares (X, Y).",
                LandmarkEmoji = "🗺️",
                TopicId = "elem_cartesian_coordinates_01",
                OrderIndex = 14
            },
            new()
            {
                Id = "station_15",
                Name = "La Tiendita de la Recta Numérica",
                Description = "El negocio del barrio donde comprendemos saldos a favor y deudas.",
                LandmarkEmoji = "🧾",
                TopicId = "sec_signed_numbers_01",
                OrderIndex = 15
            },
            new()
            {
                Id = "station_16",
                Name = "El Templo de la Jerarquía PEMDAS",
                Description = "El santuario donde los paréntesis y multiplicaciones definen la prioridad.",
                LandmarkEmoji = "🛡️",
                TopicId = "sec_order_operations_01",
                OrderIndex = 16
            },
            new()
            {
                Id = "station_17",
                Name = "La Papelería de las Ecuaciones",
                Description = "El comercio donde calculamos precios unitarios con ax + b = c.",
                LandmarkEmoji = "📐",
                TopicId = "sec_linear_equations_01",
                OrderIndex = 17
            },
            new()
            {
                Id = "station_18",
                Name = "El Tianguis de las Ofertas",
                Description = "El mercado de promociones donde calculamos el tanto por ciento y descuentos.",
                LandmarkEmoji = "🏷️",
                TopicId = "sec_percentages_01",
                OrderIndex = 18
            },
            new()
            {
                Id = "station_19",
                Name = "La Torre de las Potencias y Exponentes",
                Description = "La cumbre donde las bases iguales suman o restan sus exponentes.",
                LandmarkEmoji = "⚡",
                TopicId = "sec_exponent_laws_01",
                OrderIndex = 19
            },
            new()
            {
                Id = "station_20",
                Name = "El Valle de los Polinomios",
                Description = "La llanura donde agrupamos y simplificamos términos semejantes.",
                LandmarkEmoji = "🧩",
                TopicId = "sec_polynomials_like_terms_01",
                OrderIndex = 20
            },
            new()
            {
                Id = "station_21",
                Name = "El Centro de Mando 2x2",
                Description = "La estación donde dos radares resuelven pares ordenados (x, y).",
                LandmarkEmoji = "🔍",
                TopicId = "sec_linear_systems_2x2_01",
                OrderIndex = 21
            },
            new()
            {
                Id = "station_22",
                Name = "La Gran Rampa Pitagórica",
                Description = "El monumento inclinado donde la suma de catetos al cuadrado da la hipotenusa.",
                LandmarkEmoji = "📐",
                TopicId = "sec_pythagorean_theorem_01",
                OrderIndex = 22
            },
            new()
            {
                Id = "station_23",
                Name = "El Laberinto de las Cuadráticas",
                Description = "El jardín de áreas cuadradas donde descubrimos raíces x² = k.",
                LandmarkEmoji = "🪞",
                TopicId = "sec_quadratic_equations_01",
                OrderIndex = 23
            },
            new()
            {
                Id = "station_24",
                Name = "El Obelisco del Sol de Tales",
                Description = "El monumento donde las sombras y triángulos semejantes revelan alturas colosales.",
                LandmarkEmoji = "☀️",
                TopicId = "sec_thales_similarity_01",
                OrderIndex = 24
            },
            new()
            {
                Id = "station_25",
                Name = "La Cúpula Trigonométrica",
                Description = "El observatorio donde el seno, coseno y tangente orientan las estrellas.",
                LandmarkEmoji = "📡",
                TopicId = "sec_trigonometry_ratios_01",
                OrderIndex = 25
            },
            new()
            {
                Id = "station_26",
                Name = "La Gran Fuente Parabólica",
                Description = "El parque acuático donde los chorros de agua dibujan funciones cuadráticas y = ax².",
                LandmarkEmoji = "⛲",
                TopicId = "sec_quadratic_functions_01",
                OrderIndex = 26
            },
            new()
            {
                Id = "station_27",
                Name = "La Central de Fibra Óptica",
                Description = "El nodo de telecomunicaciones donde medimos distancias euclidianas exactas d = √((Δx)² + (Δy)²).",
                LandmarkEmoji = "📍",
                TopicId = "prep_analytic_distance_01",
                OrderIndex = 27
            },
            new()
            {
                Id = "station_28",
                Name = "El Viaducto de las Pendientes",
                Description = "La autopista elevada donde calculamos la inclinación y razón de cambio m = Δy / Δx.",
                LandmarkEmoji = "🌉",
                TopicId = "prep_line_slope_01",
                OrderIndex = 28
            },
            new()
            {
                Id = "station_29",
                Name = "La Terminal del Mototaxi Lineal",
                Description = "La estación de transporte donde modelamos costos fijos y variables con y = mx + b.",
                LandmarkEmoji = "🛺",
                TopicId = "prep_line_equation_ymxb_01",
                OrderIndex = 29
            },
            new()
            {
                Id = "station_30",
                Name = "La Cúpula del Radar Canónico",
                Description = "El perímetro de vigilancia circular donde medimos radios de cobertura con x² + y² = r².",
                LandmarkEmoji = "🎯",
                TopicId = "prep_circle_conic_01",
                OrderIndex = 30
            },
            new()
            {
                Id = "station_31",
                Name = "El Laboratorio de Ondas Sonoras",
                Description = "La sala de audio digital donde estudiamos la amplitud y cresta en y = A · sen(x).",
                LandmarkEmoji = "〰️",
                TopicId = "prep_sine_wave_01",
                OrderIndex = 31
            },
            new()
            {
                Id = "station_32",
                Name = "El Bosque de los Árboles Binarios",
                Description = "La reserva algorítmica donde los logaritmos log₂(n) dividen y vencen en tiempo récord.",
                LandmarkEmoji = "🌲",
                TopicId = "prep_logarithm_binary_01",
                OrderIndex = 32
            },
            new()
            {
                Id = "station_33",
                Name = "La Torre de los Robots y Bucles",
                Description = "La cúspide tecnológica donde las matemáticas programan el futuro.",
                LandmarkEmoji = "🚀",
                TopicId = "prog_seed_01",
                OrderIndex = 33
            }
        };

        bool foundCurrent = false;

        foreach (var s in stations)
        {
            s.IsCompleted = profile.CompletedLessonIds.Contains(s.TopicId);
            if (s.IsCompleted)
            {
                s.StarsEarned = 3;
            }
            else if (!foundCurrent)
            {
                s.IsCurrentStation = true;
                foundCurrent = true;
            }
        }

        // Si todas están completadas, la última es la activa
        if (!foundCurrent && stations.Count > 0)
        {
            stations[^1].IsCurrentStation = true;
        }

        return stations;
    }
}
