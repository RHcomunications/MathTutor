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

    [Fact]
    public void GenerateFractionPartsExercise_MatchesNumeratorAndTotalItems()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.MiddleElementary, Age = 9 };
        var rng = new Random(600);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateFractionPartsExercise(profile, rng);
            Assert.Equal(GamePlayType.FractionParts, ex.GameType);
            Assert.True(ex.ExpectedResult >= 1 && ex.ExpectedResult <= ex.ItemsGroupA.Count);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateFractionEquivalenceExercise_HasValidCorrectOption()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.MiddleElementary, Age = 9 };
        var rng = new Random(700);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateFractionEquivalenceExercise(profile, rng);
            Assert.Equal(GamePlayType.FractionEquivalence, ex.GameType);
            Assert.Equal(1, ex.ExpectedResult);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == 1);
        }
    }

    [Fact]
    public void GenerateMultiplicationDistributiveExercise_CalculatesCorrectProduct()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.MiddleElementary, Age = 9 };
        var rng = new Random(800);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateMultiplicationDistributiveExercise(profile, rng);
            Assert.Equal(GamePlayType.MultiplicationDistributive, ex.GameType);
            Assert.True(ex.ExpectedResult >= 20);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_FractionsAndDistributive_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.MiddleElementary, Age = 9 };

        var fracList = _generator.GenerateExercisesForTopic("elem_fractions_01", profile, count: 4);
        Assert.Equal(4, fracList.Count);
        Assert.Contains(fracList, ex => ex.GameType == GamePlayType.FractionParts);
        Assert.Contains(fracList, ex => ex.GameType == GamePlayType.FractionEquivalence);

        var distList = _generator.GenerateExercisesForTopic("elem_distributive_01", profile, count: 4);
        Assert.Equal(4, distList.Count);
        Assert.All(distList, ex => Assert.Equal(GamePlayType.MultiplicationDistributive, ex.GameType));
    }

    [Fact]
    public void GenerateMixedFractionsExercise_CalculatesTotalPartsCorrectly()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 10 };
        var rng = new Random(900);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateMixedFractionsExercise(profile, rng);
            Assert.Equal(GamePlayType.MixedFractions, ex.GameType);
            Assert.True(ex.ExpectedResult >= 3);
            Assert.Equal(ex.ExpectedResult, ex.ItemsGroupA.Count);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateDecimalsMoneyExercise_ProducesValidMonetaryValues()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 10 };
        var rng = new Random(1000);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateDecimalsMoneyExercise(profile, rng);
            Assert.Equal(GamePlayType.DecimalsMoney, ex.GameType);
            Assert.True(ex.ExpectedResult >= 100); // Al menos $1.00
            Assert.True(ex.ExpectedResult % 50 == 0); // Múltiplo de 50 centavos
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GeneratePowersExercise_ProducesAccurateExponentialResult()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 10 };
        var rng = new Random(1100);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GeneratePowersExercise(profile, rng);
            Assert.Equal(GamePlayType.PowersAndExponentGrowth, ex.GameType);
            Assert.True(ex.ExpectedResult is 2 or 4 or 8 or 16);
            Assert.Equal(ex.ExpectedResult, ex.ItemsGroupA.Count);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_FifthGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 10 };

        var mixedList = _generator.GenerateExercisesForTopic("elem_mixed_fractions_01", profile, count: 3);
        Assert.Equal(3, mixedList.Count);
        Assert.All(mixedList, ex => Assert.Equal(GamePlayType.MixedFractions, ex.GameType));

        var decList = _generator.GenerateExercisesForTopic("elem_decimals_01", profile, count: 3);
        Assert.Equal(3, decList.Count);
        Assert.All(decList, ex => Assert.Equal(GamePlayType.DecimalsMoney, ex.GameType));

        var powList = _generator.GenerateExercisesForTopic("elem_powers_01", profile, count: 3);
        Assert.Equal(3, powList.Count);
        Assert.All(powList, ex => Assert.Equal(GamePlayType.PowersAndExponentGrowth, ex.GameType));
    }

    [Fact]
    public void GenerateDirectProportionExercise_CalculatesProportionalExpectedResult()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 11 };
        var rng = new Random(1200);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateDirectProportionExercise(profile, rng);
            Assert.Equal(GamePlayType.DirectProportion, ex.GameType);
            Assert.True(ex.ExpectedResult >= 4);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateAlgebraicBalanceExercise_SolvesForXCorrectly()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 11 };
        var rng = new Random(1300);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateAlgebraicBalanceExercise(profile, rng);
            Assert.Equal(GamePlayType.AlgebraicBalance, ex.GameType);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateSquareRootExercise_ReturnsExactRootOfSquare()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 11 };
        var rng = new Random(1400);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateSquareRootExercise(profile, rng);
            Assert.Equal(GamePlayType.SquareRootPerfect, ex.GameType);
            Assert.True(ex.ExpectedResult is 2 or 3 or 4 or 5);
            Assert.Equal(ex.ExpectedResult * ex.ExpectedResult, ex.ItemsGroupA.Count);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateCartesianCoordinatesExercise_ReturnsCorrectCoordinate()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 11 };
        var rng = new Random(1500);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateCartesianCoordinatesExercise(profile, rng);
            Assert.Equal(GamePlayType.CartesianCoordinates, ex.GameType);
            Assert.True(ex.ExpectedResult >= 1 && ex.ExpectedResult <= 5);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_SixthGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.UpperElementary, Age = 11 };

        var propList = _generator.GenerateExercisesForTopic("elem_proportion_01", profile, count: 3);
        Assert.Equal(3, propList.Count);
        Assert.All(propList, ex => Assert.Equal(GamePlayType.DirectProportion, ex.GameType));

        var algList = _generator.GenerateExercisesForTopic("elem_algebra_balance_01", profile, count: 3);
        Assert.Equal(3, algList.Count);
        Assert.All(algList, ex => Assert.Equal(GamePlayType.AlgebraicBalance, ex.GameType));

        var sqrtList = _generator.GenerateExercisesForTopic("elem_square_root_01", profile, count: 3);
        Assert.Equal(3, sqrtList.Count);
        Assert.All(sqrtList, ex => Assert.Equal(GamePlayType.SquareRootPerfect, ex.GameType));

        var coordList = _generator.GenerateExercisesForTopic("elem_cartesian_coordinates_01", profile, count: 3);
        Assert.Equal(3, coordList.Count);
        Assert.All(coordList, ex => Assert.Equal(GamePlayType.CartesianCoordinates, ex.GameType));
    }
}
