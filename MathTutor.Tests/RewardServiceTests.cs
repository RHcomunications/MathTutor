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
        Assert.Equal(11, stations.Count);
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
        Assert.Equal("station_11", stations[10].Id);
        Assert.Equal("prog_seed_01", stations[10].TopicId);
    }
}
