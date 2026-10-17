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
        Assert.Equal(39, allTopics.Count);

        foreach (var topic in allTopics)
        {
            var profile = new UserProfile { CurrentLevel = topic.Level, Age = 12 };
            var exercises = curriculum.GenerateAdaptiveExercises(topic, profile);

            Assert.Equal(3, exercises.Count);
            Assert.All(exercises, ex =>
            {
                Assert.Contains(ex.Options, o => o.IsCorrect && o.Value == ex.ExpectedResult);
                if (topic.Id != "kinder_counting_01")
                {
                    Assert.NotEqual(GamePlayType.InteractiveCounting, ex.GameType);
                }
            });
        }
    }

    [Fact]
    public void GetAdaptedOptions_PreservesRelativeOrderWithoutAlwaysPuttingCorrectFirst()
    {
        var adaptive = new AdaptiveEngine();
        var profile = new UserProfile
        {
            Disability = DisabilityType.Intellectual,
            Severity = DisabilitySeverity.Moderate
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

        Assert.Equal(39, allTopics.Count);
        Assert.Equal(39, stations.Count);

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
}
