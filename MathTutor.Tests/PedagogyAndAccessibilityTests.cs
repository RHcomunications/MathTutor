using System.Linq;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using Xunit;

namespace MathTutor.Tests;

public class PedagogyAndAccessibilityTests
{
    private readonly CurriculumService _curriculum = new();
    private readonly AdaptiveEngine _adaptive = new();

    [Fact]
    public void KindergartenTopic_CoversAllFiveCountingPrinciples()
    {
        var topics = _curriculum.GetAllTopics();
        var kinderTopic = topics.FirstOrDefault(t => t.Level == AcademicLevel.Kindergarten);

        Assert.NotNull(kinderTopic);
        Assert.Contains(kinderTopic.PrinciplesAddressed, p => p.Contains("Correspondencia uno a uno"));
        Assert.Contains(kinderTopic.PrinciplesAddressed, p => p.Contains("Orden estable"));
        Assert.Contains(kinderTopic.PrinciplesAddressed, p => p.Contains("Cardinalidad"));
        Assert.Contains(kinderTopic.PrinciplesAddressed, p => p.Contains("Abstracción"));
        Assert.Contains(kinderTopic.PrinciplesAddressed, p => p.Contains("Irrelevancia del orden"));
    }

    [Fact]
    public void AdditionTopic_CoversCoreOperationAndProperties()
    {
        var topics = _curriculum.GetAllTopics();
        var addTopic = topics.FirstOrDefault(t => t.Id == "elem_addition_01");

        Assert.NotNull(addTopic);
        Assert.Contains(addTopic.PrinciplesAddressed, p => p.Contains("adición"));
        Assert.Contains(addTopic.PrinciplesAddressed, p => p.Contains("Conmutativa"));
        Assert.Contains(addTopic.PrinciplesAddressed, p => p.Contains("Elemento Neutro"));
    }

    [Fact]
    public void SubtractionTopic_CoversInverseRelation()
    {
        var topics = _curriculum.GetAllTopics();
        var subTopic = topics.FirstOrDefault(t => t.Id == "elem_subtraction_01");

        Assert.NotNull(subTopic);
        Assert.Contains(subTopic.PrinciplesAddressed, p => p.Contains("inversa"));
    }

    [Fact]
    public void ProgrammingSeedTopic_CoversRepeatedAdditionAndLoops()
    {
        var topics = _curriculum.GetAllTopics();
        var progTopic = topics.FirstOrDefault(t => t.Level == AcademicLevel.ProgrammingFoundations);

        Assert.NotNull(progTopic);
        Assert.Contains(progTopic.PrinciplesAddressed, p => p.Contains("Bucle") || p.Contains("multiplicación"));
    }

    [Fact]
    public void MultiplicationTopic_CoversArraysAndProperties()
    {
        var topics = _curriculum.GetAllTopics();
        var multTopic = topics.FirstOrDefault(t => t.Id == "elem_multiplication_01");

        Assert.NotNull(multTopic);
        Assert.Contains(multTopic.PrinciplesAddressed, p => p.Contains("suma repetida"));
        Assert.Contains(multTopic.PrinciplesAddressed, p => p.Contains("matrices"));
        Assert.Contains(multTopic.PrinciplesAddressed, p => p.Contains("conmutativa"));
    }

    [Fact]
    public void DivisionTopic_CoversFairSharingAndInverse()
    {
        var topics = _curriculum.GetAllTopics();
        var divTopic = topics.FirstOrDefault(t => t.Id == "elem_division_01");

        Assert.NotNull(divTopic);
        Assert.Contains(divTopic.PrinciplesAddressed, p => p.Contains("reparto equitativo"));
        Assert.Contains(divTopic.PrinciplesAddressed, p => p.Contains("inversa"));
    }

    [Fact]
    public void FractionsTopic_CoversNumeratorDenominatorAndEquivalence()
    {
        var topics = _curriculum.GetAllTopics();
        var fracTopic = topics.FirstOrDefault(t => t.Id == "elem_fractions_01");

        Assert.NotNull(fracTopic);
        Assert.Equal(AcademicLevel.MiddleElementary, fracTopic.Level);
        Assert.Contains(fracTopic.PrinciplesAddressed, p => p.Contains("numerador") || p.Contains("todo"));
        Assert.Contains(fracTopic.PrinciplesAddressed, p => p.Contains("equivalencia"));
        Assert.True(fracTopic.Exercises.Count >= 2);
    }

    [Fact]
    public void DistributiveTopic_CoversDecompositionAndProperties()
    {
        var topics = _curriculum.GetAllTopics();
        var distTopic = topics.FirstOrDefault(t => t.Id == "elem_distributive_01");

        Assert.NotNull(distTopic);
        Assert.Equal(AcademicLevel.MiddleElementary, distTopic.Level);
        Assert.Contains(distTopic.PrinciplesAddressed, p => p.Contains("distributiva"));
        Assert.Contains(distTopic.PrinciplesAddressed, p => p.Contains("Descomposición"));
        Assert.True(distTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void MixedFractionsTopic_CoversMixedNumbersAndImproperFractions()
    {
        var topics = _curriculum.GetAllTopics();
        var mixedTopic = topics.FirstOrDefault(t => t.Id == "elem_mixed_fractions_01");

        Assert.NotNull(mixedTopic);
        Assert.Equal(AcademicLevel.UpperElementary, mixedTopic.Level);
        Assert.Contains(mixedTopic.PrinciplesAddressed, p => p.Contains("mixto"));
        Assert.Contains(mixedTopic.PrinciplesAddressed, p => p.Contains("impropia"));
        Assert.True(mixedTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void DecimalsTopic_CoversMoneyAndDecimalPlaceValue()
    {
        var topics = _curriculum.GetAllTopics();
        var decTopic = topics.FirstOrDefault(t => t.Id == "elem_decimals_01");

        Assert.NotNull(decTopic);
        Assert.Equal(AcademicLevel.UpperElementary, decTopic.Level);
        Assert.Contains(decTopic.PrinciplesAddressed, p => p.Contains("decimal"));
        Assert.Contains(decTopic.PrinciplesAddressed, p => p.Contains("monedas"));
        Assert.True(decTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void PowersTopic_CoversExponentsAndMultiplicationGrowth()
    {
        var topics = _curriculum.GetAllTopics();
        var powTopic = topics.FirstOrDefault(t => t.Id == "elem_powers_01");

        Assert.NotNull(powTopic);
        Assert.Equal(AcademicLevel.UpperElementary, powTopic.Level);
        Assert.Contains(powTopic.PrinciplesAddressed, p => p.Contains("potencia") || p.Contains("Potencia"));
        Assert.Contains(powTopic.PrinciplesAddressed, p => p.Contains("exponencial"));
        Assert.True(powTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void ProportionTopic_CoversDirectRatioAndRuleOfThree()
    {
        var topics = _curriculum.GetAllTopics();
        var propTopic = topics.FirstOrDefault(t => t.Id == "elem_proportion_01");

        Assert.NotNull(propTopic);
        Assert.Equal(AcademicLevel.UpperElementary, propTopic.Level);
        Assert.Contains(propTopic.PrinciplesAddressed, p => p.Contains("proporcionalidad"));
        Assert.Contains(propTopic.PrinciplesAddressed, p => p.Contains("regla de tres"));
        Assert.True(propTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void AlgebraicBalanceTopic_CoversEquationsAndUnknowns()
    {
        var topics = _curriculum.GetAllTopics();
        var algTopic = topics.FirstOrDefault(t => t.Id == "elem_algebra_balance_01");

        Assert.NotNull(algTopic);
        Assert.Equal(AcademicLevel.UpperElementary, algTopic.Level);
        Assert.Contains(algTopic.PrinciplesAddressed, p => p.Contains("ecuación"));
        Assert.Contains(algTopic.PrinciplesAddressed, p => p.Contains("incógnita"));
        Assert.True(algTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void SquareRootTopic_CoversInversePowerAndPerfectSquares()
    {
        var topics = _curriculum.GetAllTopics();
        var sqrtTopic = topics.FirstOrDefault(t => t.Id == "elem_square_root_01");

        Assert.NotNull(sqrtTopic);
        Assert.Equal(AcademicLevel.UpperElementary, sqrtTopic.Level);
        Assert.Contains(sqrtTopic.PrinciplesAddressed, p => p.Contains("raíz cuadrada"));
        Assert.Contains(sqrtTopic.PrinciplesAddressed, p => p.Contains("inversa"));
        Assert.True(sqrtTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void CartesianCoordinatesTopic_CoversOrderedPairsAndAxes()
    {
        var topics = _curriculum.GetAllTopics();
        var coordTopic = topics.FirstOrDefault(t => t.Id == "elem_cartesian_coordinates_01");

        Assert.NotNull(coordTopic);
        Assert.Equal(AcademicLevel.UpperElementary, coordTopic.Level);
        Assert.Contains(coordTopic.PrinciplesAddressed, p => p.Contains("pares ordenados"));
        Assert.Contains(coordTopic.PrinciplesAddressed, p => p.Contains("eje"));
        Assert.True(coordTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void SignedNumbersTopic_CoversIntegersAndSignedOperations()
    {
        var topics = _curriculum.GetAllTopics();
        var signTopic = topics.FirstOrDefault(t => t.Id == "sec_signed_numbers_01");

        Assert.NotNull(signTopic);
        Assert.Equal(AcademicLevel.SecondaryFirstGrade, signTopic.Level);
        Assert.Contains(signTopic.PrinciplesAddressed, p => p.Contains("enteros"));
        Assert.Contains(signTopic.PrinciplesAddressed, p => p.Contains("recta numérica"));
        Assert.True(signTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void OrderOfOperationsTopic_CoversPEMDASAndParentheses()
    {
        var topics = _curriculum.GetAllTopics();
        var pemdasTopic = topics.FirstOrDefault(t => t.Id == "sec_order_operations_01");

        Assert.NotNull(pemdasTopic);
        Assert.Equal(AcademicLevel.SecondaryFirstGrade, pemdasTopic.Level);
        Assert.Contains(pemdasTopic.PrinciplesAddressed, p => p.Contains("PEMDAS"));
        Assert.Contains(pemdasTopic.PrinciplesAddressed, p => p.Contains("Prioridad"));
        Assert.True(pemdasTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void LinearEquationsTopic_CoversModelAndInverseOperations()
    {
        var topics = _curriculum.GetAllTopics();
        var eqTopic = topics.FirstOrDefault(t => t.Id == "sec_linear_equations_01");

        Assert.NotNull(eqTopic);
        Assert.Equal(AcademicLevel.SecondaryFirstGrade, eqTopic.Level);
        Assert.Contains(eqTopic.PrinciplesAddressed, p => p.Contains("algebraico"));
        Assert.Contains(eqTopic.PrinciplesAddressed, p => p.Contains("Despeje"));
        Assert.True(eqTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void PercentagesTopic_CoversBase100AndDiscounts()
    {
        var topics = _curriculum.GetAllTopics();
        var pctTopic = topics.FirstOrDefault(t => t.Id == "sec_percentages_01");

        Assert.NotNull(pctTopic);
        Assert.Equal(AcademicLevel.SecondaryFirstGrade, pctTopic.Level);
        Assert.Contains(pctTopic.PrinciplesAddressed, p => p.Contains("tanto por ciento"));
        Assert.Contains(pctTopic.PrinciplesAddressed, p => p.Contains("descuento"));
        Assert.True(pctTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void ExponentLawsTopic_CoversPowerProductAndQuotients()
    {
        var topics = _curriculum.GetAllTopics();
        var expTopic = topics.FirstOrDefault(t => t.Id == "sec_exponent_laws_01");

        Assert.NotNull(expTopic);
        Assert.Equal(AcademicLevel.SecondarySecondGrade, expTopic.Level);
        Assert.Contains(expTopic.PrinciplesAddressed, p => p.Contains("potencias"));
        Assert.Contains(expTopic.PrinciplesAddressed, p => p.Contains("exponente"));
        Assert.True(expTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void PolynomialLikeTermsTopic_CoversSimplificationAndAlgebraicLanguage()
    {
        var topics = _curriculum.GetAllTopics();
        var polyTopic = topics.FirstOrDefault(t => t.Id == "sec_polynomials_like_terms_01");

        Assert.NotNull(polyTopic);
        Assert.Equal(AcademicLevel.SecondarySecondGrade, polyTopic.Level);
        Assert.Contains(polyTopic.PrinciplesAddressed, p => p.Contains("términos semejantes"));
        Assert.Contains(polyTopic.PrinciplesAddressed, p => p.Contains("Reducción"));
        Assert.True(polyTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void LinearSystems2x2Topic_CoversSimultaneousEquationsAndSolutionPairs()
    {
        var topics = _curriculum.GetAllTopics();
        var sysTopic = topics.FirstOrDefault(t => t.Id == "sec_linear_systems_2x2_01");

        Assert.NotNull(sysTopic);
        Assert.Equal(AcademicLevel.SecondarySecondGrade, sysTopic.Level);
        Assert.Contains(sysTopic.PrinciplesAddressed, p => p.Contains("simultáneas"));
        Assert.Contains(sysTopic.PrinciplesAddressed, p => p.Contains("reducción"));
        Assert.True(sysTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void PythagoreanTheoremTopic_CoversRightTrianglesAndHypotenuse()
    {
        var topics = _curriculum.GetAllTopics();
        var pythTopic = topics.FirstOrDefault(t => t.Id == "sec_pythagorean_theorem_01");

        Assert.NotNull(pythTopic);
        Assert.Equal(AcademicLevel.SecondarySecondGrade, pythTopic.Level);
        Assert.Contains(pythTopic.PrinciplesAddressed, p => p.Contains("Pitágoras"));
        Assert.Contains(pythTopic.PrinciplesAddressed, p => p.Contains("hipotenusa"));
        Assert.True(pythTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void AdaptiveEngine_SimplifiesLanguage_ForIntellectualDisability()
    {
        var profile = new UserProfile
        {
            Disability = DisabilityType.Intellectual,
            Severity = DisabilitySeverity.Moderate
        };
        profile.ApplyRecommendedAccessibilityDefaults();

        Assert.True(profile.Preferences.SimplifyLanguage);
        Assert.True(profile.Preferences.ReduceOptionsCount);

        var topic = _curriculum.GetAllTopics().First();
        var ex = topic.Exercises.First();

        string adaptedInstruction = _adaptive.GetAdaptedInstruction(ex, profile);
        Assert.Equal(ex.SimplifiedInstruction, adaptedInstruction);

        var adaptedOptions = _adaptive.GetAdaptedOptions(ex, profile);
        Assert.True(adaptedOptions.Count <= 2);
        Assert.Contains(adaptedOptions, o => o.IsCorrect);
    }

    [Fact]
    public void UserProfile_VisualDisability_EnablesVoiceAndScreenReaderDefaults()
    {
        var profile = new UserProfile
        {
            Disability = DisabilityType.Visual,
            Severity = DisabilitySeverity.Severe
        };
        profile.ApplyRecommendedAccessibilityDefaults();

        Assert.True(profile.Preferences.EnableVoiceSynthesis);
        Assert.True(profile.Preferences.ScreenReaderAnnouncements);
        Assert.True(profile.Preferences.PlayAudioCues);
        Assert.Equal(HighContrastTheme.HighContrastDark, profile.Preferences.Theme);
        Assert.Equal(FontSizeLevel.ExtraLarge, profile.Preferences.FontSize);
    }

    [Fact]
    public void UserProfile_Neurodivergent_SetsLowSensoryTheme()
    {
        var profile = new UserProfile
        {
            Disability = DisabilityType.Neurodivergent
        };
        profile.ApplyRecommendedAccessibilityDefaults();

        Assert.Equal(HighContrastTheme.LowSensory, profile.Preferences.Theme);
        Assert.True(profile.Preferences.UnlimitedTime);
    }

    [Fact]
    public void AdaptiveEngine_RecordAttempt_AwardsStarsAndStreak()
    {
        var profile = new UserProfile();
        var topic = _curriculum.GetAllTopics().First();
        var ex = topic.Exercises.First();

        int initialStars = profile.StarsTotal;
        int initialStreak = profile.CurrentStreak;

        _adaptive.RecordAttempt(profile, ex, isCorrect: true, attemptCount: 1);

        Assert.Equal(initialStars + 3, profile.StarsTotal);
        Assert.Equal(initialStreak + 1, profile.CurrentStreak);
        Assert.Contains(ex.Id, profile.CompletedExerciseIds);
    }
}
