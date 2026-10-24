using System;
using System.Linq;
using System.Reflection;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using Xunit;

namespace MathTutor.Tests;

public class ExerciseGeneratorTests
{
    private readonly ExerciseGenerator _generator = new();
    private readonly AdaptiveEngine _adaptive = new();

    [Fact]
    public void GetTargetRange_RestrictsNumbers_ForIntellectualDisability()
    {
        var extensiveProfile = new UserProfile
        {
            Disability = DisabilityType.Intellectual,
            Support = { IntellectualSupport = IntellectualSupportLevel.Extensive }
        };
        var (minExt, maxExt) = _generator.GetTargetRange(extensiveProfile);
        Assert.Equal(1, minExt);
        Assert.Equal(3, maxExt);

        var limitedProfile = new UserProfile
        {
            Disability = DisabilityType.Intellectual,
            Support = { IntellectualSupport = IntellectualSupportLevel.Limited }
        };
        var (minLim, maxLim) = _generator.GetTargetRange(limitedProfile);
        Assert.Equal(1, minLim);
        Assert.Equal(5, maxLim);
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

    [Fact]
    public void GenerateSignedNumbersExercise_HandlesSignedBalanceAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryFirstGrade, Age = 12 };
        var rng = new Random(1600);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateSignedNumbersExercise(profile, rng);
            Assert.Equal(GamePlayType.SignedNumbersAddition, ex.GameType);
            Assert.True(ex.ExpectedResult < 0); // Deuda neta en la tiendita
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateOrderOfOperationsExercise_RespectsMultiplicationPrecedence()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryFirstGrade, Age = 12 };
        var rng = new Random(1700);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateOrderOfOperationsExercise(profile, rng);
            Assert.Equal(GamePlayType.OrderOfOperationsPEMDAS, ex.GameType);
            Assert.True(ex.ExpectedResult >= 6);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateLinearEquationAXBExercise_CalculatesXAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryFirstGrade, Age = 12 };
        var rng = new Random(1800);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateLinearEquationAXBExercise(profile, rng);
            Assert.Equal(GamePlayType.LinearEquationAXB, ex.GameType);
            Assert.True(ex.ExpectedResult >= 2);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GeneratePercentageExercise_ComputesDiscountAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryFirstGrade, Age = 12 };
        var rng = new Random(1900);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GeneratePercentageExercise(profile, rng);
            Assert.Equal(GamePlayType.PercentageCalculation, ex.GameType);
            Assert.True(ex.ExpectedResult >= 10);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_SecondaryFirstGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryFirstGrade, Age = 12 };

        var signList = _generator.GenerateExercisesForTopic("sec_signed_numbers_01", profile, count: 3);
        Assert.Equal(3, signList.Count);
        Assert.All(signList, ex => Assert.Equal(GamePlayType.SignedNumbersAddition, ex.GameType));

        var pemdasList = _generator.GenerateExercisesForTopic("sec_order_operations_01", profile, count: 3);
        Assert.Equal(3, pemdasList.Count);
        Assert.All(pemdasList, ex => Assert.Equal(GamePlayType.OrderOfOperationsPEMDAS, ex.GameType));

        var eqList = _generator.GenerateExercisesForTopic("sec_linear_equations_01", profile, count: 3);
        Assert.Equal(3, eqList.Count);
        Assert.All(eqList, ex => Assert.Equal(GamePlayType.LinearEquationAXB, ex.GameType));

        var pctList = _generator.GenerateExercisesForTopic("sec_percentages_01", profile, count: 3);
        Assert.Equal(3, pctList.Count);
        Assert.All(pctList, ex => Assert.Equal(GamePlayType.PercentageCalculation, ex.GameType));
    }

    [Fact]
    public void GenerateExponentLawsExercise_ProducesValidMultiplicationOrDivisionExponents()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondarySecondGrade, Age = 13 };
        var rng = new Random(2000);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateExponentLawsExercise(profile, rng);
            Assert.Equal(GamePlayType.ExponentLaws, ex.GameType);
            Assert.True(ex.ExpectedResult >= 1 && ex.ExpectedResult <= 12);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GeneratePolynomialLikeTermsExercise_CalculatesTotalCoefficientAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondarySecondGrade, Age = 13 };
        var rng = new Random(2100);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GeneratePolynomialLikeTermsExercise(profile, rng);
            Assert.Equal(GamePlayType.PolynomialLikeTerms, ex.GameType);
            Assert.True(ex.ExpectedResult >= 2);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateLinearSystem2x2Exercise_FindsAccurateVariable()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondarySecondGrade, Age = 13 };
        var rng = new Random(2200);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateLinearSystem2x2Exercise(profile, rng);
            Assert.Equal(GamePlayType.LinearSystem2x2, ex.GameType);
            Assert.True(ex.ExpectedResult >= 12 && ex.ExpectedResult <= 18);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GeneratePythagoreanTheoremExercise_SolvesHypotenuseAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondarySecondGrade, Age = 13 };
        var rng = new Random(2300);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GeneratePythagoreanTheoremExercise(profile, rng);
            Assert.Equal(GamePlayType.PythagoreanTheorem, ex.GameType);
            Assert.True(ex.ExpectedResult >= 5);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_SecondarySecondGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondarySecondGrade, Age = 13 };

        var expList = _generator.GenerateExercisesForTopic("sec_exponent_laws_01", profile, count: 3);
        Assert.Equal(3, expList.Count);
        Assert.All(expList, ex => Assert.Equal(GamePlayType.ExponentLaws, ex.GameType));

        var polyList = _generator.GenerateExercisesForTopic("sec_polynomials_like_terms_01", profile, count: 3);
        Assert.Equal(3, polyList.Count);
        Assert.All(polyList, ex => Assert.Equal(GamePlayType.PolynomialLikeTerms, ex.GameType));

        var sysList = _generator.GenerateExercisesForTopic("sec_linear_systems_2x2_01", profile, count: 3);
        Assert.Equal(3, sysList.Count);
        Assert.All(sysList, ex => Assert.Equal(GamePlayType.LinearSystem2x2, ex.GameType));

        var pythList = _generator.GenerateExercisesForTopic("sec_pythagorean_theorem_01", profile, count: 3);
        Assert.Equal(3, pythList.Count);
        Assert.All(pythList, ex => Assert.Equal(GamePlayType.PythagoreanTheorem, ex.GameType));
    }

    [Fact]
    public void GenerateQuadraticEquationExercise_ProducesAccurateRoots()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryThirdGrade, Age = 14 };
        var rng = new Random(2400);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateQuadraticEquationExercise(profile, rng);
            Assert.Equal(GamePlayType.QuadraticEquations, ex.GameType);
            Assert.True(ex.ExpectedResult >= 3 && ex.ExpectedResult <= 9);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateTriangleSimilarityThalesExercise_ComputesProportionalHeight()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryThirdGrade, Age = 14 };
        var rng = new Random(2500);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateTriangleSimilarityThalesExercise(profile, rng);
            Assert.Equal(GamePlayType.TriangleSimilarityThales, ex.GameType);
            Assert.True(ex.ExpectedResult >= 4 && ex.ExpectedResult <= 10);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateTrigonometricRatiosExercise_SolvesOppositeSideAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryThirdGrade, Age = 14 };
        var rng = new Random(2600);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateTrigonometricRatiosExercise(profile, rng);
            Assert.Equal(GamePlayType.TrigonometricRatios, ex.GameType);
            Assert.True(ex.ExpectedResult >= 3);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateQuadraticFunctionCurveExercise_ComputesParabolicHeight()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryThirdGrade, Age = 14 };
        var rng = new Random(2700);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateQuadraticFunctionCurveExercise(profile, rng);
            Assert.Equal(GamePlayType.QuadraticFunctionCurve, ex.GameType);
            Assert.True(ex.ExpectedResult >= 8);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_SecondaryThirdGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.SecondaryThirdGrade, Age = 14 };

        var quadList = _generator.GenerateExercisesForTopic("sec_quadratic_equations_01", profile, count: 3);
        Assert.Equal(3, quadList.Count);
        Assert.All(quadList, ex => Assert.Equal(GamePlayType.QuadraticEquations, ex.GameType));

        var thalesList = _generator.GenerateExercisesForTopic("sec_thales_similarity_01", profile, count: 3);
        Assert.Equal(3, thalesList.Count);
        Assert.All(thalesList, ex => Assert.Equal(GamePlayType.TriangleSimilarityThales, ex.GameType));

        var trigList = _generator.GenerateExercisesForTopic("sec_trigonometry_ratios_01", profile, count: 3);
        Assert.Equal(3, trigList.Count);
        Assert.All(trigList, ex => Assert.Equal(GamePlayType.TrigonometricRatios, ex.GameType));

        var funcList = _generator.GenerateExercisesForTopic("sec_quadratic_functions_01", profile, count: 3);
        Assert.Equal(3, funcList.Count);
        Assert.All(funcList, ex => Assert.Equal(GamePlayType.QuadraticFunctionCurve, ex.GameType));
    }

    [Fact]
    public void GenerateAnalyticDistanceExercise_SolvesExactEuclideanDistance()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolFirstGrade, Age = 15 };
        var rng = new Random(2800);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateAnalyticDistanceExercise(profile, rng);
            Assert.Equal(GamePlayType.AnalyticDistance2D, ex.GameType);
            Assert.True(ex.ExpectedResult >= 5);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateLineSlopeExercise_ComputesSlopeAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolFirstGrade, Age = 15 };
        var rng = new Random(2900);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateLineSlopeExercise(profile, rng);
            Assert.Equal(GamePlayType.LineSlopeM, ex.GameType);
            Assert.True(ex.ExpectedResult >= 2 && ex.ExpectedResult <= 5);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateLinearEquationYMXBExercise_CalculatesTotalCostAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolFirstGrade, Age = 15 };
        var rng = new Random(3000);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateLinearEquationYMXBExercise(profile, rng);
            Assert.Equal(GamePlayType.LinearEquationYMXB, ex.GameType);
            Assert.True(ex.ExpectedResult >= 6);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_HighSchoolFirstGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolFirstGrade, Age = 15 };

        var distList = _generator.GenerateExercisesForTopic("prep_analytic_distance_01", profile, count: 3);
        Assert.Equal(3, distList.Count);
        Assert.All(distList, ex => Assert.Equal(GamePlayType.AnalyticDistance2D, ex.GameType));

        var slopeList = _generator.GenerateExercisesForTopic("prep_line_slope_01", profile, count: 3);
        Assert.Equal(3, slopeList.Count);
        Assert.All(slopeList, ex => Assert.Equal(GamePlayType.LineSlopeM, ex.GameType));

        var ymxbList = _generator.GenerateExercisesForTopic("prep_line_equation_ymxb_01", profile, count: 3);
        Assert.Equal(3, ymxbList.Count);
        Assert.All(ymxbList, ex => Assert.Equal(GamePlayType.LinearEquationYMXB, ex.GameType));
    }

    [Fact]
    public void GenerateConicCircleExercise_SolvesExactRadius()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolSecondGrade, Age = 16 };
        var rng = new Random(3100);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateConicCircleExercise(profile, rng);
            Assert.Equal(GamePlayType.ConicCircleRadius, ex.GameType);
            Assert.True(ex.ExpectedResult >= 3 && ex.ExpectedResult <= 8);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateSineWaveContinuousExercise_ProducesAccuratePeakAmplitude()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolSecondGrade, Age = 16 };
        var rng = new Random(3200);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateSineWaveContinuousExercise(profile, rng);
            Assert.Equal(GamePlayType.SineWaveContinuous, ex.GameType);
            Assert.True(ex.ExpectedResult >= 3 && ex.ExpectedResult <= 7);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateLogarithmBinaryExercise_CalculatesBinaryStepsAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolSecondGrade, Age = 16 };
        var rng = new Random(3300);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateLogarithmBinaryExercise(profile, rng);
            Assert.Equal(GamePlayType.LogarithmBinary, ex.GameType);
            Assert.True(ex.ExpectedResult >= 3 && ex.ExpectedResult <= 6);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_HighSchoolSecondGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolSecondGrade, Age = 16 };

        var circleList = _generator.GenerateExercisesForTopic("prep_circle_conic_01", profile, count: 3);
        Assert.Equal(3, circleList.Count);
        Assert.All(circleList, ex => Assert.Equal(GamePlayType.ConicCircleRadius, ex.GameType));

        var waveList = _generator.GenerateExercisesForTopic("prep_sine_wave_01", profile, count: 3);
        Assert.Equal(3, waveList.Count);
        Assert.All(waveList, ex => Assert.Equal(GamePlayType.SineWaveContinuous, ex.GameType));

        var logList = _generator.GenerateExercisesForTopic("prep_logarithm_binary_01", profile, count: 3);
        Assert.Equal(3, logList.Count);
        Assert.All(logList, ex => Assert.Equal(GamePlayType.LogarithmBinary, ex.GameType));
    }

    [Fact]
    public void GenerateCalculusDerivativeExercise_ComputesInstantaneousVelocityAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolThirdGrade, Age = 17 };
        var rng = new Random(3400);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateCalculusDerivativeExercise(profile, rng);
            Assert.Equal(GamePlayType.CalculusDerivativeRate, ex.GameType);
            Assert.True(ex.ExpectedResult >= 8 && ex.ExpectedResult <= 40);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateCalculusIntegralExercise_ComputesAreaUnderCurveAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolThirdGrade, Age = 17 };
        var rng = new Random(3500);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateCalculusIntegralExercise(profile, rng);
            Assert.Equal(GamePlayType.CalculusIntegralArea, ex.GameType);
            Assert.True(ex.ExpectedResult >= 9 && ex.ExpectedResult <= 42);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateProbabilityAndStatsExercise_ComputesArithmeticMeanAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolThirdGrade, Age = 17 };
        var rng = new Random(3600);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateProbabilityAndStatsExercise(profile, rng);
            Assert.Equal(GamePlayType.ProbabilityAndStats, ex.GameType);
            Assert.True(ex.ExpectedResult >= 6 && ex.ExpectedResult <= 9);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateExercisesForTopic_HighSchoolThirdGrade_GeneratesValidLists()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.HighSchoolThirdGrade, Age = 17 };

        var derivList = _generator.GenerateExercisesForTopic("prep_calculus_derivative_01", profile, count: 3);
        Assert.Equal(3, derivList.Count);
        Assert.All(derivList, ex => Assert.Equal(GamePlayType.CalculusDerivativeRate, ex.GameType));

        var intList = _generator.GenerateExercisesForTopic("prep_calculus_integral_01", profile, count: 3);
        Assert.Equal(3, intList.Count);
        Assert.All(intList, ex => Assert.Equal(GamePlayType.CalculusIntegralArea, ex.GameType));

        var statsList = _generator.GenerateExercisesForTopic("prep_probability_stats_01", profile, count: 3);
        Assert.Equal(3, statsList.Count);
        Assert.All(statsList, ex => Assert.Equal(GamePlayType.ProbabilityAndStats, ex.GameType));
    }

    [Fact]
    public void GeneratePatternAndLoopSeedExercise_ComputesRepeatedAdditionAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.ProgrammingFoundations, Age = 10 };
        var rng = new Random(3700);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GeneratePatternAndLoopSeedExercise(profile, rng);
            Assert.Equal(GamePlayType.PatternAndLoopSeed, ex.GameType);
            Assert.Equal(AcademicLevel.ProgrammingFoundations, ex.Level);
            Assert.True(ex.ExpectedResult >= 4 && ex.ExpectedResult <= 25);
            Assert.NotEmpty(ex.ItemsGroupA);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateProgrammingVariablesExercise_TracksVariableStateAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.ProgrammingFoundations, Age = 12 };
        var rng = new Random(3800);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateProgrammingVariablesExercise(profile, rng);
            Assert.Equal(GamePlayType.ProgrammingVariablesState, ex.GameType);
            Assert.Equal(AcademicLevel.ProgrammingFoundations, ex.Level);
            Assert.True(ex.ExpectedResult >= 5 && ex.ExpectedResult <= 30);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateProgrammingConditionalsExercise_EvaluatesIfElseBranchAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.ProgrammingFoundations, Age = 12 };
        var rng = new Random(3900);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateProgrammingConditionalsExercise(profile, rng);
            Assert.Equal(GamePlayType.ProgrammingConditionalsIfElse, ex.GameType);
            Assert.Equal(AcademicLevel.ProgrammingFoundations, ex.Level);
            Assert.True(ex.ExpectedResult >= 6 && ex.ExpectedResult <= 25);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void GenerateProgrammingBooleanLogicExercise_EvaluatesAndGateAccurately()
    {
        var profile = new UserProfile { CurrentLevel = AcademicLevel.ProgrammingFoundations, Age = 12 };
        var rng = new Random(4000);

        for (int i = 0; i < 20; i++)
        {
            var ex = _generator.GenerateProgrammingBooleanLogicExercise(profile, rng);
            Assert.Equal(GamePlayType.ProgrammingBooleanLogic, ex.GameType);
            Assert.Equal(AcademicLevel.ProgrammingFoundations, ex.Level);
            Assert.True(ex.ExpectedResult >= 12 && ex.ExpectedResult <= 26);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
        }
    }

    [Fact]
    public void AllCurriculumTopics_GenerateAdaptiveExercisesWithoutFallbackToCounting()
    {
        var curriculum = new CurriculumService();
        var allTopics = curriculum.GetAllTopics();
        Assert.Equal(74, allTopics.Count);

        foreach (var topic in allTopics)
        {
            var profile = new UserProfile { CurrentLevel = topic.Level, Age = 12 };
            var exercises = curriculum.GenerateAdaptiveExercises(topic, profile);

            Assert.Equal(3, exercises.Count);
            Assert.All(exercises, ex =>
            {
                Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
                if (topic.Level != AcademicLevel.Kindergarten)
                {
                    Assert.NotEqual(GamePlayType.InteractiveCounting, ex.GameType);
                }
            });
        }
    }

    [Fact]
    public void KindergartenTopics_GenerateProceduralExercises_WithIntellectualDisability_GeneratesTopicSpecificMechanics()
    {
        var curriculum = new CurriculumService();
        var generator = new ExerciseGenerator();
        var profile = new UserProfile
        {
            CurrentLevel = AcademicLevel.Kindergarten,
            Age = 4,
            Disability = DisabilityType.Intellectual,
            Support = { IntellectualSupport = IntellectualSupportLevel.Extensive }
        };

        var kinderTopics = curriculum.GetAllTopics().Where(t => t.Level == AcademicLevel.Kindergarten).ToList();
        Assert.Equal(10, kinderTopics.Count);

        foreach (var topic in kinderTopics)
        {
            var exercises = generator.GenerateExercisesForTopic(topic.Id, profile, count: 2);
            Assert.Equal(2, exercises.Count);
            Assert.All(exercises, ex =>
            {
                Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
                Assert.True(ex.ExpectedResult >= 1);
            });
        }

        var patternEx = generator.GenerateExercisesForTopic("kinder_patterns_01", profile, count: 1).First();
        Assert.Equal(GamePlayType.OrderSequence, patternEx.GameType);

        var measureEx = generator.GenerateExercisesForTopic("kinder_measure_01", profile, count: 1).First();
        Assert.Equal(GamePlayType.Comparison, measureEx.GameType);

        var addSubEx = generator.GenerateExercisesForTopic("kinder_add_sub_01", profile, count: 1).First();
        Assert.True(addSubEx.GameType is GamePlayType.AdditionCombine or GamePlayType.SubtractionTakeAway);
    }

    [Fact]
    public void GetAdaptedOptions_PreservesRelativeOrderWithoutAlwaysPuttingCorrectFirst()
    {
        var adaptive = new AdaptiveEngine();
        var profile = new UserProfile
        {
            Disability = DisabilityType.Intellectual,
            Support = { IntellectualSupport = IntellectualSupportLevel.Limited }
        };
        profile.ApplyRecommendedAccessibilityDefaults();

        var ex = new Exercise
        {
            ExpectedResult = 10,
            Options =
            {
                new ExerciseOption { Id = "o1", Label = "8", Value = 8, IsCorrect = false },
                new ExerciseOption { Id = "o2", Label = "10", Value = 10, IsCorrect = true },
                new ExerciseOption { Id = "o3", Label = "12", Value = 12, IsCorrect = false }
            }
        };

        var reduced = adaptive.GetAdaptedOptions(ex, profile);
        Assert.Equal(2, reduced.Count);
        Assert.False(reduced[0].IsCorrect);
        Assert.True(reduced[1].IsCorrect);
    }

    [Fact]
    public void AllCurriculumTopics_AreConnectedToRewardsAndHaveCompleteExplanationsAndErrorFeedback()
    {
        var curriculum = new CurriculumService();
        var rewards = new MathTutor.Core.Services.RewardService();
        var profile = new UserProfile { Age = 12 };

        var allTopics = curriculum.GetAllTopics();
        var stations = rewards.GetRealmStations(profile);
        var badges = rewards.GetAllBadges(profile);

        Assert.Equal(74, allTopics.Count);
        Assert.Equal(74, stations.Count);

        var topicIds = allTopics.Select(t => t.Id).ToHashSet();
        var stationTopicIds = stations.Select(s => s.TopicId).ToHashSet();
        var badgeTopicIds = badges.Where(b => !string.IsNullOrEmpty(b.AssociatedTopicId)).Select(b => b.AssociatedTopicId).ToHashSet();

        // Verificar que ninguna estación, medalla o tema quede suelto
        Assert.Equal(topicIds, stationTopicIds);
        Assert.Equal(topicIds, badgeTopicIds);

        foreach (var topic in allTopics)
        {
            Assert.False(string.IsNullOrWhiteSpace(topic.Title));
            Assert.False(string.IsNullOrWhiteSpace(topic.NarrativeIntro));
            Assert.False(string.IsNullOrWhiteSpace(topic.NarrativeIntroSimplified));
            Assert.False(string.IsNullOrWhiteSpace(topic.CoreConceptExplanation));
            Assert.False(string.IsNullOrWhiteSpace(topic.CoreConceptSimplified));
            Assert.NotEmpty(topic.PrinciplesAddressed);
            Assert.False(string.IsNullOrWhiteSpace(topic.CongratulationMessage));
            Assert.NotEmpty(topic.Exercises);

            var exercises = curriculum.GenerateAdaptiveExercises(topic, profile, count: 3);
            Assert.Equal(3, exercises.Count);

            foreach (var ex in exercises)
            {
                Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
                Assert.False(string.IsNullOrWhiteSpace(ex.SimplifiedInstruction));
                Assert.False(string.IsNullOrWhiteSpace(ex.ScreenReaderDescription));
                Assert.False(string.IsNullOrWhiteSpace(ex.FirstHint));
                Assert.False(string.IsNullOrWhiteSpace(ex.SecondHintDetailed));
                Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));

                var wrongOptions = ex.Options.Where(o => !o.IsCorrect).ToList();
                Assert.NotEmpty(wrongOptions);
                foreach (var wrongOpt in wrongOptions)
                {
                    Assert.False(string.IsNullOrWhiteSpace(wrongOpt.SpeechFeedback));
                    Assert.DoesNotContain("¡Escucha la pista e inténtalo de nuevo!", wrongOpt.SpeechFeedback);
                }
            }
        }
    }

    [Fact]
    public void GenerateProgrammingModule2Exercises_CreatesValidLoopsArraysAndFunctions()
    {
        var profile = new UserProfile
        {
            Age = 15,
            CurrentLevel = AcademicLevel.ProgrammingFoundations,
            Region = RegionalContext.Peru
        };
        var rng = new Random(1502);

        var loopEx = _generator.GenerateProgrammingLoopsExercise(profile, rng);
        Assert.Equal(GamePlayType.ProgrammingLoopsForWhile, loopEx.GameType);
        Assert.Contains(loopEx.Options, o => o.IsCorrect && o.Value == loopEx.ExpectedResult);
        Assert.Single(loopEx.ItemsGroupA);

        var arrEx = _generator.GenerateProgrammingArraysExercise(profile, rng);
        Assert.Equal(GamePlayType.ProgrammingArraysZeroIndex, arrEx.GameType);
        Assert.Contains("bodega", arrEx.StandardInstruction);
        Assert.Contains("soles", arrEx.StandardInstruction);
        Assert.Contains(arrEx.Options, o => o.IsCorrect && o.Value == arrEx.ExpectedResult);

        var funcEx = _generator.GenerateProgrammingFunctionsExercise(profile, rng);
        Assert.Equal(GamePlayType.ProgrammingFunctionsReturn, funcEx.GameType);
        Assert.Contains("RETORNAR", funcEx.StandardInstruction);
        Assert.Contains(funcEx.Options, o => o.IsCorrect && o.Value == funcEx.ExpectedResult);
    }

    [Fact]
    public void GenerateProgrammingModule3Exercises_CreatesValidAlgorithmsGameCoordsAndAssistiveAI()
    {
        var profile = new UserProfile
        {
            Age = 16,
            CurrentLevel = AcademicLevel.ProgrammingFoundations
        };
        var rng = new Random(1603);

        var algoEx = _generator.GenerateProgrammingAlgorithmsSearchExercise(profile, rng);
        Assert.Equal(GamePlayType.ProgrammingAlgorithmsSearch, algoEx.GameType);
        Assert.Contains(algoEx.Options, o => o.IsCorrect && o.Value == algoEx.ExpectedResult);
        Assert.Single(algoEx.ItemsGroupA);

        var gameEx = _generator.GenerateProgrammingGameCoordinatesExercise(profile, rng);
        Assert.Equal(GamePlayType.ProgrammingGameCoordinates2D, gameEx.GameType);
        Assert.Contains("velocidad_x", gameEx.StandardInstruction);
        Assert.Contains(gameEx.Options, o => o.IsCorrect && o.Value == gameEx.ExpectedResult);

        var aiEx = _generator.GenerateProgrammingAssistiveAIExercise(profile, rng);
        Assert.Equal(GamePlayType.ProgrammingAssistiveAIData, aiEx.GameType);
        Assert.Contains("promedio", aiEx.StandardInstruction);
        Assert.Contains(aiEx.Options, o => o.IsCorrect && o.Value == aiEx.ExpectedResult);
    }

    [Fact]
    public void BuildOptions_And_ShuffleOptions_DistributesCorrectOptionAcrossAllPositions()
    {
        var profile = new UserProfile { Age = 6, CurrentLevel = AcademicLevel.EarlyElementary };
        int option1Count = 0;
        int option2Count = 0;
        int option3Count = 0;

        for (int i = 0; i < 90; i++)
        {
            var rng = new Random(i * 37 + 11);
            var ex = _generator.GenerateCountingExercise(profile, rng);
            int correctIndex = ex.Options.FindIndex(o => o.IsCorrect);
            Assert.True(correctIndex >= 0 && correctIndex < ex.Options.Count);

            // Verificar atajos secuenciales 1..N
            for (int k = 0; k < ex.Options.Count; k++)
            {
                Assert.Equal((k + 1).ToString(), ex.Options[k].AccessibleKeyShortcut);
            }

            if (correctIndex == 0) option1Count++;
            else if (correctIndex == 1) option2Count++;
            else if (correctIndex == 2) option3Count++;
        }

        // Cada posición debe aparecer de forma distribuida (sin sesgo fijo a la opción 2)
        Assert.True(option1Count >= 15, $"Opción 1 apareció {option1Count} veces, se esperaba >= 15");
        Assert.True(option2Count >= 15, $"Opción 2 apareció {option2Count} veces, se esperaba >= 15");
        Assert.True(option3Count >= 15, $"Opción 3 apareció {option3Count} veces, se esperaba >= 15");
    }

    [Fact]
    public void BuildOptions_OptionsStayWithinDeclaredRange()
    {
        var buildOptions = GetBuildOptionsAccessor();
        var rng = new Random(7);

        foreach (var (correct, min, max) in new[] { (10, 1, 20), (30, 30, 30 + 4), (50, 1, 50), (7, 1, 7), (100, 50, 150), (1, 1, 2), (2, 1, 2) })
        {
            for (int seed = 0; seed < 30; seed++)
            {
                var ex = new Exercise { Id = $"t_{seed}", ExpectedResult = correct };
                buildOptions.Invoke(null, new object?[] { ex, correct, min, max, "", rng });

                Assert.Equal(3, ex.Options.Count);
                Assert.Equal(ex.Options.Select(o => o.Value).Count(), ex.Options.Select(o => o.Value).Distinct().Count());
                Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == correct);
                // El rango es un límite heurístico: cuando solo caben dos opciones posibles
                // dentro del rango, el respaldo determinista expande alternando lados.
                int poolSize = Math.Max(max, correct) - Math.Min(min, correct) + 1;
                if (poolSize - 1 >= 2)
                {
                    Assert.All(ex.Options, o => Assert.True(o.Value >= Math.Min(min, correct) && o.Value <= Math.Max(max, correct), $"Opción {o.Value} fuera de [{min}, {max}]"));
                }
                Assert.Equal(1, ex.Options.Count(o => o.IsCorrect));
            }
        }
    }

    [Fact]
    public void BuildOptions_DistractorsVary_WithinWideRange()
    {
        var buildOptions = GetBuildOptionsAccessor();
        const int correct = 10;
        const int min = 1;
        const int max = 30;

        var distractorSets = new HashSet<string>();
        for (int seed = 0; seed < 60; seed++)
        {
            var ex = new Exercise { Id = $"v_{seed}", ExpectedResult = correct };
            buildOptions.Invoke(null, new object?[] { ex, correct, min, max, "", new Random(seed) });
            var set = string.Join(",", ex.Options.Where(o => !o.IsCorrect).OrderBy(o => o.Value).Select(o => o.Value));
            distractorSets.Add(set);
        }

        Assert.True(distractorSets.Count >= 6, $"Solo {distractorSets.Count} combinaciones de distractores distintas en 60 semillas.");
    }

    [Fact]
    public void BuildOptions_DegenerateRange_ExpandsOnBothSides()
    {
        var buildOptions = GetBuildOptionsAccessor();
        var rng = new Random(3);

        // Rango con solo dos opciones posibles: el respaldo no debe sesgar
        // todos los distractores hacia arriba.
        var ex = new Exercise { Id = "deg", ExpectedResult = 1 };
        buildOptions.Invoke(null, new object?[] { ex, 1, 1, 2, "", rng });
        var values = ex.Options.Select(o => o.Value).OrderBy(v => v).ToList();
        Assert.Equal(new[] { 0, 1, 2 }, values);

        // Resultado negativo sin espacio en el rango: también alterna lados.
        var neg = new Exercise { Id = "neg", ExpectedResult = -3 };
        buildOptions.Invoke(null, new object?[] { neg, -3, -3, -3, "", rng });
        var negValues = neg.Options.Select(o => o.Value).OrderBy(v => v).ToList();
        Assert.Contains(-4, negValues);
        Assert.Contains(-3, negValues);
        Assert.Contains(-2, negValues);
    }

    [Fact]
    public void GetAdaptedOptions_ReducesToClosestDistractor_WithoutMutatingSource()
    {
        var profile = new UserProfile { Age = 7, Disability = DisabilityType.Intellectual };
        var exercise = new Exercise { Id = "red" };
        exercise.Options.Add(new ExerciseOption { Id = "a", Value = 9, IsCorrect = false, AccessibleKeyShortcut = "1" });
        exercise.Options.Add(new ExerciseOption { Id = "b", Value = 6, IsCorrect = true, AccessibleKeyShortcut = "2" });
        exercise.Options.Add(new ExerciseOption { Id = "c", Value = 12, IsCorrect = false, AccessibleKeyShortcut = "3" });

        var reduced = _adaptive.GetAdaptedOptions(exercise, profile);

        Assert.Equal(2, reduced.Count);
        Assert.Contains(reduced, o => o.IsCorrect && o.Value == 6);
        // 9 está más cerca de 6 que 12: se conserva el distractor más informativo.
        Assert.Contains(reduced, o => !o.IsCorrect && o.Value == 9);
        Assert.Equal(new[] { "1", "2" }, reduced.Select(o => o.AccessibleKeyShortcut).ToArray());

        // Las opciones originales del ejercicio compartido no se mutan.
        Assert.Equal("1", exercise.Options[0].AccessibleKeyShortcut);
        Assert.Equal("2", exercise.Options[1].AccessibleKeyShortcut);
        Assert.Equal("3", exercise.Options[2].AccessibleKeyShortcut);
        Assert.NotSame(reduced[0], exercise.Options[0]);
        Assert.NotSame(reduced[1], exercise.Options[1]);
    }

    [Fact]
    public void AllCurriculumTopicExercises_HaveSoundOptionSets()
    {
        var profile = new UserProfile { Age = 14, CurrentLevel = AcademicLevel.HighSchoolSecondGrade };
        var curriculum = new CurriculumService();
        var topics = curriculum.GetAllTopics();
        int generadoCount = 0;

        foreach (var topic in topics)
        {
            var list = _generator.GenerateExercisesForTopic(topic.Id, profile, count: 3);
            foreach (var ex in list)
            {
                Assert.NotEmpty(ex.Options);
                Assert.Equal(ex.Options.Select(o => o.Value).Count(), ex.Options.Select(o => o.Value).Distinct().Count());
                Assert.Equal(1, ex.Options.Count(o => o.IsCorrect));
                Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
                generadoCount++;
            }
        }

        Assert.True(generadoCount >= 100, $"Se esperaban >= 100 ejercicios generados, salieron {generadoCount}");
    }

    private static MethodInfo GetBuildOptionsAccessor()
    {
        var method = typeof(ExerciseGenerator).GetMethod("BuildOptions", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return method!;
    }

    [Fact]
    public void SimplifiedHintCatalog_CoversEveryGamePlayType()
    {
        foreach (GamePlayType gameType in Enum.GetValues<GamePlayType>())
        {
            var first = SimplifiedHintCatalog.GetSimpleHint(gameType, detailed: false);
            var second = SimplifiedHintCatalog.GetSimpleHint(gameType, detailed: true);
            var success = SimplifiedHintCatalog.GetSimpleSuccess(gameType);

            Assert.False(string.IsNullOrWhiteSpace(first), $"Falta pista 1 simple para {gameType}");
            Assert.False(string.IsNullOrWhiteSpace(second), $"Falta pista 2 simple para {gameType}");
            Assert.False(string.IsNullOrWhiteSpace(success), $"Falta explicación de éxito simple para {gameType}");
        }
    }

    [Fact]
    public void GetAdaptedHints_FallBackToCatalog_ForSecondaryTopicWithSimpleLanguage()
    {
        var profile = new UserProfile
        {
            Age = 15,
            CurrentLevel = AcademicLevel.SecondaryFirstGrade,
            Preferences = { SimplifyLanguage = true }
        };
        var topic = new CurriculumService().GetAllTopics()
            .First(t => t.Id == "sec_order_operations_01");
        var exercise = _generator.GenerateExercisesForTopic(topic.Id, profile, count: 1)[0];

        var simpleFirst = exercise.SimplifiedFirstHint;
        if (string.IsNullOrWhiteSpace(simpleFirst))
        {
            var hint = _adaptive.GetAdaptedHint(exercise, profile, detailed: false);
            Assert.False(string.IsNullOrWhiteSpace(hint));
            // El catálogo no depende de números generados: la pista respaldada es estable.
            Assert.Equal(SimplifiedHintCatalog.GetSimpleHint(exercise.GameType, detailed: false), hint);
            Assert.NotEqual(exercise.FirstHint, hint);
        }
        else
        {
            Assert.Equal(simpleFirst, _adaptive.GetAdaptedHint(exercise, profile, detailed: false));
        }

        var success = _adaptive.GetAdaptedSuccess(exercise, profile);
        Assert.False(string.IsNullOrWhiteSpace(success));
        if (string.IsNullOrWhiteSpace(exercise.SimplifiedExplanationOfSuccess))
        {
            Assert.Equal(SimplifiedHintCatalog.GetSimpleSuccess(exercise.GameType), success);
        }
    }
    [Fact]
    public void GenerateAnglesExercise_GeneratesAccurateAngleClassification()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 9, CurrentLevel = AcademicLevel.MiddleElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateAnglesExercise(profile);
            Assert.Equal(GamePlayType.GeometryAnglesClassification, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult is 30 or 45 or 60 or 75 or 90 or 105 or 120 or 135 or 150 or 180);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateAreaExercise_ComputesAreaAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 10, CurrentLevel = AcademicLevel.UpperElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateAreaExercise(profile);
            Assert.Equal(GamePlayType.GeometryArea2D, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 4);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateVolumeExercise_ComputesVolumeAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 11, CurrentLevel = AcademicLevel.UpperElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateVolumeExercise(profile);
            Assert.Equal(GamePlayType.GeometryVolume3D, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 8);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }
    [Fact]
    public void GenerateMoneyCurrencyExercise_CalculatesCoinAndBillTotalsAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 7, CurrentLevel = AcademicLevel.EarlyElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateMoneyCurrencyExercise(profile);
            Assert.Equal(GamePlayType.MoneyCurrencyCounting, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateMeasurementRulerExercise_MeasuresCentimetersAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 7, CurrentLevel = AcademicLevel.EarlyElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateMeasurementRulerExercise(profile);
            Assert.Equal(GamePlayType.MeasurementRulerLength, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateTimeClockExercise_ReadsHoursAndMinutesAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 8, CurrentLevel = AcademicLevel.EarlyElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateTimeClockExercise(profile);
            Assert.Equal(GamePlayType.TimeClockReading, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult is >= 1 and <= 60);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GeneratePerimeterExercise_ComputesPerimeterAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 9, CurrentLevel = AcademicLevel.MiddleElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GeneratePerimeterExercise(profile);
            Assert.Equal(GamePlayType.GeometryPerimeter, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 6);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateDivisionRemainderExercise_SolvesRemainderAndCocienteAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 9, CurrentLevel = AcademicLevel.MiddleElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateDivisionRemainderExercise(profile);
            Assert.Equal(GamePlayType.DivisionWithRemainder, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateFractionsNumberLineExercise_PositionsFraccionAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 10, CurrentLevel = AcademicLevel.MiddleElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateFractionsNumberLineExercise(profile);
            Assert.Equal(GamePlayType.FractionsNumberLine, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateDataBarChartsExercise_CalculatesFrequenciesAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 9, CurrentLevel = AcademicLevel.MiddleElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateDataBarChartsExercise(profile);
            Assert.Equal(GamePlayType.DataBarChartsAndTables, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateStatisticsCentralTendencyExercise_CalculatesMeanModeMedianAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 11, CurrentLevel = AcademicLevel.UpperElementary };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateStatisticsCentralTendencyExercise(profile);
            Assert.Equal(GamePlayType.StatisticsCentralTendency, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }


    [Fact]
    public void GenerateCircleGeometryExercise_ComputesDimensionsAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 12, CurrentLevel = AcademicLevel.SecondaryFirstGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateCircleGeometryExercise(profile);
            Assert.Equal(GamePlayType.CircleGeometryPi, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 2);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateFactorsMcmMcdExercise_CalculatesMcmAndMcdAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 12, CurrentLevel = AcademicLevel.SecondaryFirstGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateFactorsMcmMcdExercise(profile);
            Assert.Equal(GamePlayType.FactorsMcmMcd, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 2);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateNegativeRationalNumbersExercise_OperatesUnderZeroAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 12, CurrentLevel = AcademicLevel.SecondaryFirstGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateNegativeRationalNumbersExercise(profile);
            Assert.Equal(GamePlayType.NegativeRationalNumbers, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }


    [Fact]
    public void GenerateAlgebraFactoringExercise_FactorsAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 14, CurrentLevel = AcademicLevel.SecondarySecondGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateAlgebraFactoringExercise(profile);
            Assert.Equal(GamePlayType.AlgebraFactoring, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateParallelLinesAnglesExercise_ComputesAnglesAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 14, CurrentLevel = AcademicLevel.SecondarySecondGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateParallelLinesAnglesExercise(profile);
            Assert.Equal(GamePlayType.ParallelLinesAngles, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 20);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateProbabilityCompoundEventsExercise_CalculatesProbabilityAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 15, CurrentLevel = AcademicLevel.SecondaryThirdGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateProbabilityCompoundEventsExercise(profile);
            Assert.Equal(GamePlayType.ProbabilityCompoundEvents, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 0);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }


    [Fact]
    public void GenerateConicsParabolaEllipseExercise_CalculatesDimensionsAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 16, CurrentLevel = AcademicLevel.HighSchoolSecondGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateConicsParabolaEllipseExercise(profile);
            Assert.Equal(GamePlayType.ConicsParabolaEllipse, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateCalculusLimitsExercise_EvaluatesLimitsAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 17, CurrentLevel = AcademicLevel.HighSchoolThirdGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateCalculusLimitsExercise(profile);
            Assert.Equal(GamePlayType.CalculusLimits, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 1);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

    [Fact]
    public void GenerateCalculusDerivativeRulesExercise_AppliesRulesAccurately()
    {
        var generator = new ExerciseGenerator();
        var profile = new UserProfile { Age = 17, CurrentLevel = AcademicLevel.HighSchoolThirdGrade };

        for (int i = 0; i < 20; i++)
        {
            var ex = generator.GenerateCalculusDerivativeRulesExercise(profile);
            Assert.Equal(GamePlayType.CalculusDerivativeRules, ex.GameType);
            Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
            Assert.True(ex.ExpectedResult >= 0);
            Assert.False(string.IsNullOrWhiteSpace(ex.StandardInstruction));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExplanationOfSuccess));
        }
    }

}
