using System;
using System.Linq;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using Xunit;

namespace MathTutor.Tests;

public class ExerciseGeneratorTests
{
    private readonly ExerciseGenerator _generator = new();

    [Fact]
    public void GetTargetRange_RestrictsNumbers_ForIntellectualDisability()
    {
        var severeProfile = new UserProfile
        {
            Disability = DisabilityType.Intellectual,
            Severity = DisabilitySeverity.Severe
        };
        var (minSev, maxSev) = _generator.GetTargetRange(severeProfile);
        Assert.Equal(1, minSev);
        Assert.Equal(3, maxSev);

        var moderateProfile = new UserProfile
        {
            Disability = DisabilityType.Intellectual,
            Severity = DisabilitySeverity.Moderate
        };
        var (minMod, maxMod) = _generator.GetTargetRange(moderateProfile);
        Assert.Equal(1, minMod);
        Assert.Equal(5, maxMod);
    }

    [Fact]
    public void GenerateCountingExercise_MatchesItemCountToExpectedResult()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.Kindergarten, Age = 5 };
        var rng = new Random(42);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateCountingExercise(profile, rng);
            Assert.Equal(ex.ExpectedResult, ex.ItemsGroupA.Count);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1 && ex.ExpectedResult <= 5);
        }
    }

    [Fact]
    public void GenerateAdditionCombineExercise_SumMatchesTotalItems()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.EarlyElementary, Age = 7 };
        var rng = new Random(100);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateAdditionCombineExercise(profile, rng);
            int totalItems = ex.ItemsGroupA.Count + ex.ItemsGroupB.Count;
            Assert.Equal(ex.ExpectedResult, totalItems);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateNeutralZeroExercise_ExpectedResultPreservesOriginalNumber()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.EarlyElementary, Age = 7 };
        var rng = new Random(200);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateNeutralZeroExercise(profile, rng);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateSubtractionExercise_AlwaysResultsInPositiveDifference()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.EarlyElementary, Age = 7 };
        var rng = new Random(300);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateSubtractionExercise(profile, rng);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_ReturnsRequestedCountWithValidContent()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.Kindergarten, Age = 4 };
        var list = _generator.GenerateExercisesForTopic("kinder_counting_01", profile, count: 5);

        Assert.Equal(5, list.Count);
        foreach (var ex in list)
        {
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.SimplifiedInstruction));
            Assert.Contains(ex.Options, o => o.IsCorrect);
        }
    }

    [Fact]
    public void GenerateMultiplicationArrayExercise_MatchesProductAndGrid()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.EarlyElementary, Age = 8 };
        var rng = new Random(400);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateMultiplicationArrayExercise(profile, rng);
            Assert.Equal(ex.ExpectedResult, ex.ItemsGroupA.Count);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateDivisionFairSharingExercise_DividesEqually()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.EarlyElementary, Age = 8 };
        var rng = new Random(500);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateDivisionFairSharingExercise(profile, rng);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_MultiplicationAndDivision_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.EarlyElementary, Age = 8 };
        
        var multList = _generator.GenerateExercisesForTopic("elem_multiplication_01", profile, count: 4);
        Assert.Equal(4, multList.Count);
        Assert.All(multList, ex => Assert.Equal(GamePlayType.MultiplicationArray, ex.GameType));

        var divList = _generator.GenerateExercisesForTopic("elem_division_01", profile, count: 4);
        Assert.Equal(4, divList.Count);
        Assert.All(divList, ex => Assert.Equal(GamePlayType.DivisionFairSharing, ex.GameType));
    }
}
