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
        Assert.Equal(54, stations.Count);
        Assert.Equal(stations.Count, stations.Select(s => s.Id).Distinct().Count());
        Assert.All(stations, s => Assert.Equal(s.OrderIndex, stations.IndexOf(s) + 1));

        Assert.Equal("station_1", stations[0].Id);
        Assert.True(stations[0].IsCompleted);
        Assert.False(stations[0].IsCurrentStation);

        // El siguiente tema de jardín queda como estación activa.
        Assert.Equal("station_kinder_shapes", stations[1].Id);
        Assert.Equal("kinder_shapes_01", stations[1].TopicId);
        Assert.False(stations[1].IsCompleted);
        Assert.True(stations[1].IsCurrentStation);

        // Las estaciones de primaria y secundaria conservan su tema asociado.
        Assert.Equal("elem_addition_01", stations.First(s => s.TopicId == "elem_addition_01").TopicId);
        Assert.Equal("sec_quadratic_functions_01", stations.First(s => s.TopicId == "sec_quadratic_functions_01").TopicId);
        Assert.Equal("prog_assistive_ai_data_01", stations[^1].TopicId);
    }
}
