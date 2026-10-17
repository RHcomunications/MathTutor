using System.Linq;
using MathTutor.Core.Models;
using MathTutor.Core.Services;
using Xunit;

namespace MathTutor.Tests;

public class RewardServiceTests
{
    private readonly RewardService _rewards = new();

    [Fact]
    public void GetAllBadges_LocksAndUnlocksAppropriately()
    {
        var profile = new UserProfile();
        var initialBadges = _rewards.GetAllBadges(profile);
        Assert.All(initialBadges, b => Assert.False(b.IsUnlocked));

        // Completar lección de conteo y alcanzar 15 estrellas
        profile.CompletedLessonIds.Add("kinder_counting_01");
        profile.StarsTotal = 15;

        var updatedBadges = _rewards.GetAllBadges(profile);
        var countingBadge = updatedBadges.First(b => b.Id == "badge_counting");
        var starBadge = updatedBadges.First(b => b.Id == "badge_star_collector");
        var additionBadge = updatedBadges.First(b => b.Id == "badge_addition");

        Assert.True(countingBadge.IsUnlocked);
        Assert.True(starBadge.IsUnlocked);
        Assert.False(additionBadge.IsUnlocked);
    }

    [Fact]
    public void GetRealmStations_OrdersAndFlagsStationsCorrectly()
    {
        var profile = new UserProfile();
        profile.CompletedLessonIds.Add("kinder_counting_01");

        var stations = _rewards.GetRealmStations(profile);
        Assert.Equal(39, stations.Count);
        Assert.Equal("station_1", stations[0].Id);
        Assert.True(stations[0].IsCompleted);
        Assert.False(stations[0].IsCurrentStation);

        // La estación 2 debe ser la estación activa actual
        Assert.Equal("station_2", stations[1].Id);
        Assert.False(stations[1].IsCompleted);
        Assert.True(stations[1].IsCurrentStation);

        // Estaciones añadidas para 4º grado
        Assert.Equal("station_6", stations[5].Id);
        Assert.Equal("elem_fractions_01", stations[5].TopicId);
        Assert.Equal("station_7", stations[6].Id);
        Assert.Equal("elem_distributive_01", stations[6].TopicId);

        // Estaciones añadidas para 5º grado
        Assert.Equal("station_8", stations[7].Id);
        Assert.Equal("elem_mixed_fractions_01", stations[7].TopicId);
        Assert.Equal("station_9", stations[8].Id);
        Assert.Equal("elem_decimals_01", stations[8].TopicId);
        Assert.Equal("station_10", stations[9].Id);
        Assert.Equal("elem_powers_01", stations[9].TopicId);

        // Estaciones añadidas para 6º grado
        Assert.Equal("station_11", stations[10].Id);
        Assert.Equal("elem_proportion_01", stations[10].TopicId);
        Assert.Equal("station_12", stations[11].Id);
        Assert.Equal("elem_algebra_balance_01", stations[11].TopicId);
        Assert.Equal("station_13", stations[12].Id);
        Assert.Equal("elem_square_root_01", stations[12].TopicId);
        Assert.Equal("station_14", stations[13].Id);
        Assert.Equal("elem_cartesian_coordinates_01", stations[13].TopicId);

        // Estaciones añadidas para 1º de secundaria
        Assert.Equal("station_15", stations[14].Id);
        Assert.Equal("sec_signed_numbers_01", stations[14].TopicId);
        Assert.Equal("station_16", stations[15].Id);
        Assert.Equal("sec_order_operations_01", stations[15].TopicId);
        Assert.Equal("station_17", stations[16].Id);
        Assert.Equal("sec_linear_equations_01", stations[16].TopicId);
        Assert.Equal("station_18", stations[17].Id);
        Assert.Equal("sec_percentages_01", stations[17].TopicId);

        // Estaciones añadidas para 2º de secundaria
        Assert.Equal("station_19", stations[18].Id);
        Assert.Equal("sec_exponent_laws_01", stations[18].TopicId);
        Assert.Equal("station_20", stations[19].Id);
        Assert.Equal("sec_polynomials_like_terms_01", stations[19].TopicId);
        Assert.Equal("station_21", stations[20].Id);
        Assert.Equal("sec_linear_systems_2x2_01", stations[20].TopicId);
        Assert.Equal("station_22", stations[21].Id);
        Assert.Equal("sec_pythagorean_theorem_01", stations[21].TopicId);

        // Estaciones añadidas para 3º de secundaria
        Assert.Equal("station_23", stations[22].Id);
        Assert.Equal("sec_quadratic_equations_01", stations[22].TopicId);
        Assert.Equal("station_24", stations[23].Id);
        Assert.Equal("sec_thales_similarity_01", stations[23].TopicId);
        Assert.Equal("station_25", stations[24].Id);
        Assert.Equal("sec_trigonometry_ratios_01", stations[24].TopicId);
        Assert.Equal("station_26", stations[25].Id);
        Assert.Equal("sec_quadratic_functions_01", stations[25].TopicId);

        // Estaciones añadidas para 1º de preparatoria
        Assert.Equal("station_27", stations[26].Id);
        Assert.Equal("prep_analytic_distance_01", stations[26].TopicId);
        Assert.Equal("station_28", stations[27].Id);
        Assert.Equal("prep_line_slope_01", stations[27].TopicId);
        Assert.Equal("station_29", stations[28].Id);
        Assert.Equal("prep_line_equation_ymxb_01", stations[28].TopicId);

        // Estaciones añadidas para 2º de preparatoria
        Assert.Equal("station_30", stations[29].Id);
        Assert.Equal("prep_circle_conic_01", stations[29].TopicId);
        Assert.Equal("station_31", stations[30].Id);
        Assert.Equal("prep_sine_wave_01", stations[30].TopicId);
        Assert.Equal("station_32", stations[31].Id);
        Assert.Equal("prep_logarithm_binary_01", stations[31].TopicId);

        // Estaciones añadidas para 3º de preparatoria
        Assert.Equal("station_33", stations[32].Id);
        Assert.Equal("prep_calculus_derivative_01", stations[32].TopicId);
        Assert.Equal("station_34", stations[33].Id);
        Assert.Equal("prep_calculus_integral_01", stations[33].TopicId);
        Assert.Equal("station_35", stations[34].Id);
        Assert.Equal("prep_probability_stats_01", stations[34].TopicId);

        // Estación Semilla de Programación y Objetivo 14 (Programación Inclusiva Módulo 1)
        Assert.Equal("station_36", stations[35].Id);
        Assert.Equal("prog_seed_01", stations[35].TopicId);
        Assert.Equal("station_37", stations[36].Id);
        Assert.Equal("prog_variables_state_01", stations[36].TopicId);
        Assert.Equal("station_38", stations[37].Id);
        Assert.Equal("prog_conditionals_ifelse_01", stations[37].TopicId);
        Assert.Equal("station_39", stations[38].Id);
        Assert.Equal("prog_boolean_logic_01", stations[38].TopicId);
    }
}
