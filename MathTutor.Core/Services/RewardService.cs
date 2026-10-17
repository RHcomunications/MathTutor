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
                Name = "La Torre de los Robots y Bucles",
                Description = "La cúspide tecnológica donde las matemáticas programan el futuro.",
                LandmarkEmoji = "🚀",
                TopicId = "prog_seed_01",
                OrderIndex = 8
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
