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
    public void QuadraticEquationsTopic_CoversSecondDegreeAndRoots()
    {
        var topics = _curriculum.GetAllTopics();
        var quadTopic = topics.FirstOrDefault(t => t.Id == "sec_quadratic_equations_01");

        Assert.NotNull(quadTopic);
        Assert.Equal(AcademicLevel.SecondaryThirdGrade, quadTopic.Level);
        Assert.Contains(quadTopic.PrinciplesAddressed, p => p.Contains("segundo grado"));
        Assert.Contains(quadTopic.PrinciplesAddressed, p => p.Contains("raíz cuadrada"));
        Assert.True(quadTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void ThalesSimilarityTopic_CoversShadowsAndSimilarityCriteria()
    {
        var topics = _curriculum.GetAllTopics();
        var thalesTopic = topics.FirstOrDefault(t => t.Id == "sec_thales_similarity_01");

        Assert.NotNull(thalesTopic);
        Assert.Equal(AcademicLevel.SecondaryThirdGrade, thalesTopic.Level);
        Assert.Contains(thalesTopic.PrinciplesAddressed, p => p.Contains("semejanza"));
        Assert.Contains(thalesTopic.PrinciplesAddressed, p => p.Contains("Tales"));
        Assert.True(thalesTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void TrigonometricRatiosTopic_CoversSineCosineTangent()
    {
        var topics = _curriculum.GetAllTopics();
        var trigTopic = topics.FirstOrDefault(t => t.Id == "sec_trigonometry_ratios_01");

        Assert.NotNull(trigTopic);
        Assert.Equal(AcademicLevel.SecondaryThirdGrade, trigTopic.Level);
        Assert.Contains(trigTopic.PrinciplesAddressed, p => p.Contains("seno") || p.Contains("trigonométricas"));
        Assert.Contains(trigTopic.PrinciplesAddressed, p => p.Contains("cateto"));
        Assert.True(trigTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void QuadraticFunctionsTopic_CoversParabolaAndVertex()
    {
        var topics = _curriculum.GetAllTopics();
        var funcTopic = topics.FirstOrDefault(t => t.Id == "sec_quadratic_functions_01");

        Assert.NotNull(funcTopic);
        Assert.Equal(AcademicLevel.SecondaryThirdGrade, funcTopic.Level);
        Assert.Contains(funcTopic.PrinciplesAddressed, p => p.Contains("cuadrática"));
        Assert.Contains(funcTopic.PrinciplesAddressed, p => p.Contains("parábolas") || p.Contains("vértice"));
        Assert.True(funcTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void AnalyticDistanceTopic_CoversEuclideanDistanceAndCoordinates()
    {
        var topics = _curriculum.GetAllTopics();
        var distTopic = topics.FirstOrDefault(t => t.Id == "prep_analytic_distance_01");

        Assert.NotNull(distTopic);
        Assert.Equal(AcademicLevel.HighSchoolFirstGrade, distTopic.Level);
        Assert.Contains(distTopic.PrinciplesAddressed, p => p.Contains("distancia euclidiana"));
        Assert.Contains(distTopic.PrinciplesAddressed, p => p.Contains("Pitágoras"));
        Assert.True(distTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void LineSlopeTopic_CoversSlopeMAndRateOfChange()
    {
        var topics = _curriculum.GetAllTopics();
        var slopeTopic = topics.FirstOrDefault(t => t.Id == "prep_line_slope_01");

        Assert.NotNull(slopeTopic);
        Assert.Equal(AcademicLevel.HighSchoolFirstGrade, slopeTopic.Level);
        Assert.Contains(slopeTopic.PrinciplesAddressed, p => p.Contains("pendiente"));
        Assert.Contains(slopeTopic.PrinciplesAddressed, p => p.Contains("razón de cambio"));
        Assert.True(slopeTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void LinearEquationYMXBTopic_CoversSlopeInterceptFormAndTariffs()
    {
        var topics = _curriculum.GetAllTopics();
        var ymxbTopic = topics.FirstOrDefault(t => t.Id == "prep_line_equation_ymxb_01");

        Assert.NotNull(ymxbTopic);
        Assert.Equal(AcademicLevel.HighSchoolFirstGrade, ymxbTopic.Level);
        Assert.Contains(ymxbTopic.PrinciplesAddressed, p => p.Contains("y = mx + b"));
        Assert.Contains(ymxbTopic.PrinciplesAddressed, p => p.Contains("ordenada al origen"));
        Assert.True(ymxbTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void ConicCircleTopic_CoversCanonicalCircleAndRadius()
    {
        var topics = _curriculum.GetAllTopics();
        var circleTopic = topics.FirstOrDefault(t => t.Id == "prep_circle_conic_01");

        Assert.NotNull(circleTopic);
        Assert.Equal(AcademicLevel.HighSchoolSecondGrade, circleTopic.Level);
        Assert.Contains(circleTopic.PrinciplesAddressed, p => p.Contains("circunferencia"));
        Assert.Contains(circleTopic.PrinciplesAddressed, p => p.Contains("radio"));
        Assert.True(circleTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void SineWaveContinuousTopic_CoversHarmonicsAndAmplitude()
    {
        var topics = _curriculum.GetAllTopics();
        var waveTopic = topics.FirstOrDefault(t => t.Id == "prep_sine_wave_01");

        Assert.NotNull(waveTopic);
        Assert.Equal(AcademicLevel.HighSchoolSecondGrade, waveTopic.Level);
        Assert.Contains(waveTopic.PrinciplesAddressed, p => p.Contains("senoidal"));
        Assert.Contains(waveTopic.PrinciplesAddressed, p => p.Contains("amplitud"));
        Assert.True(waveTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void LogarithmBinaryTopic_CoversBinaryLogarithmAndAlgorithms()
    {
        var topics = _curriculum.GetAllTopics();
        var logTopic = topics.FirstOrDefault(t => t.Id == "prep_logarithm_binary_01");

        Assert.NotNull(logTopic);
        Assert.Equal(AcademicLevel.HighSchoolSecondGrade, logTopic.Level);
        Assert.Contains(logTopic.PrinciplesAddressed, p => p.Contains("logaritmo"));
        Assert.Contains(logTopic.PrinciplesAddressed, p => p.Contains("base 2") || p.Contains("binarios"));
        Assert.True(logTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void CalculusDerivativeTopic_CoversInstantaneousRateAndPowerRule()
    {
        var topics = _curriculum.GetAllTopics();
        var derivTopic = topics.FirstOrDefault(t => t.Id == "prep_calculus_derivative_01");

        Assert.NotNull(derivTopic);
        Assert.Equal(AcademicLevel.HighSchoolThirdGrade, derivTopic.Level);
        Assert.Contains(derivTopic.PrinciplesAddressed, p => p.Contains("razón de cambio instantánea") || p.Contains("recta tangente"));
        Assert.Contains(derivTopic.PrinciplesAddressed, p => p.Contains("derivación"));
        Assert.True(derivTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void CalculusIntegralTopic_CoversAreaUnderCurveAndFundamentalTheorem()
    {
        var topics = _curriculum.GetAllTopics();
        var intTopic = topics.FirstOrDefault(t => t.Id == "prep_calculus_integral_01");

        Assert.NotNull(intTopic);
        Assert.Equal(AcademicLevel.HighSchoolThirdGrade, intTopic.Level);
        Assert.Contains(intTopic.PrinciplesAddressed, p => p.Contains("área bajo la curva"));
        Assert.Contains(intTopic.PrinciplesAddressed, p => p.Contains("Teorema Fundamental"));
        Assert.True(intTopic.Exercises.Count >= 1);
    }

    [Fact]
    public void ProbabilityAndStatsTopic_CoversArithmeticMeanAndDataScience()
    {
        var topics = _curriculum.GetAllTopics();
        var statsTopic = topics.FirstOrDefault(t => t.Id == "prep_probability_stats_01");

        Assert.NotNull(statsTopic);
        Assert.Equal(AcademicLevel.HighSchoolThirdGrade, statsTopic.Level);
        Assert.Contains(statsTopic.PrinciplesAddressed, p => p.Contains("media aritmética"));
        Assert.Contains(statsTopic.PrinciplesAddressed, p => p.Contains("ciencia de datos"));
        Assert.True(statsTopic.Exercises.Count >= 1);
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
    public void AdaptiveEngine_UsesAgeLanguageRegister_EvenWithoutDisability()
    {
        var young = new UserProfile { Age = 4 };
        var older = new UserProfile { Age = 9 };
        var topic = _curriculum.GetAllTopics().First(t => t.Id == "kinder_counting_01");
        var exercise = topic.Exercises.First();

        Assert.Equal(LanguageRegister.PlayfulEarly, AdaptiveEngine.GetLanguageRegister(young));
        Assert.Equal(LanguageRegister.Standard, AdaptiveEngine.GetLanguageRegister(older));
        Assert.Equal(exercise.SimplifiedInstruction, _adaptive.GetAdaptedInstruction(exercise, young));
        Assert.Equal(exercise.StandardInstruction, _adaptive.GetAdaptedInstruction(exercise, older));
        Assert.Equal(exercise.SimplifiedExplanationOfSuccess, _adaptive.GetAdaptedSuccess(exercise, young));
        Assert.Equal(exercise.SimplifiedFirstHint, _adaptive.GetAdaptedHint(exercise, young, detailed: false));
    }

    [Fact]
    public void AdaptiveEngine_DoesNotInfantilizeOlderLearner_WithIntellectualDisability()
    {
        var profile = new UserProfile
        {
            Age = 15,
            Disability = DisabilityType.Intellectual
        };

        Assert.Equal(LanguageRegister.Simple, AdaptiveEngine.GetLanguageRegister(profile));
    }

    [Fact]
    public void Kindergarten_ChildFacingText_HasNoTechnicalTerms()
    {
        var topic = _curriculum.GetAllTopics().First(t => t.Id == "kinder_counting_01");
        var banned = new[]
        {
            "cardinalidad",
            "correspondencia",
            "orden estable",
            "abstracción",
            "abstraccion",
            "irrelevancia",
            "colección",
            "coleccion"
        };

        var texts = new List<string>
        {
            topic.Title,
            topic.NarrativeIntro,
            topic.NarrativeIntroSimplified,
            topic.CoreConceptExplanation,
            topic.CoreConceptSimplified,
            topic.CongratulationMessage
        };

        foreach (var exercise in topic.Exercises)
        {
            texts.Add(exercise.Title);
            texts.Add(exercise.StandardInstruction);
            texts.Add(exercise.SimplifiedInstruction);
            texts.Add(exercise.FirstHint);
            texts.Add(exercise.SecondHintDetailed);
            texts.Add(exercise.ExplanationOfSuccess);
            texts.Add(exercise.SimplifiedFirstHint);
            texts.Add(exercise.SimplifiedSecondHint);
            texts.Add(exercise.SimplifiedExplanationOfSuccess);
            texts.AddRange(exercise.Options.Select(o => o.Label));
            texts.AddRange(exercise.Options.Select(o => o.SpeechFeedback));
        }

        foreach (var text in texts.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            foreach (var term in banned)
            {
                Assert.False(
                    text.Contains(term, StringComparison.OrdinalIgnoreCase),
                    $"El texto de jardín contiene el término técnico '{term}': {text}");
            }
        }
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

    [Fact]
    public void AdaptiveEngine_GetAllowedLevelsForAge_EnforcesAgeBracketRules()
    {
        // 3 a 5 años: únicamente Preescolar / Kínder
        var kinderLevels = AdaptiveEngine.GetAllowedLevelsForAge(4);
        Assert.Single(kinderLevels);
        Assert.Contains(AcademicLevel.Kindergarten, kinderLevels);

        // 6 a 12 años: Primaria Inicial, Media y Superior
        var elementaryLevels = AdaptiveEngine.GetAllowedLevelsForAge(9);
        Assert.Equal(3, elementaryLevels.Count);
        Assert.Contains(AcademicLevel.EarlyElementary, elementaryLevels);
        Assert.Contains(AcademicLevel.MiddleElementary, elementaryLevels);
        Assert.Contains(AcademicLevel.UpperElementary, elementaryLevels);
        Assert.DoesNotContain(AcademicLevel.SecondaryFirstGrade, elementaryLevels);

        // 13 a 15 años: Secundaria 1º, 2º y 3º
        var secondaryLevels = AdaptiveEngine.GetAllowedLevelsForAge(14);
        Assert.Equal(3, secondaryLevels.Count);
        Assert.Contains(AcademicLevel.SecondaryFirstGrade, secondaryLevels);
        Assert.Contains(AcademicLevel.SecondarySecondGrade, secondaryLevels);
        Assert.Contains(AcademicLevel.SecondaryThirdGrade, secondaryLevels);
        Assert.DoesNotContain(AcademicLevel.HighSchoolFirstGrade, secondaryLevels);

        // 16 años o más: todos los niveles incluyendo Preparatoria y Semilla de Programación
        var highSchoolLevels = AdaptiveEngine.GetAllowedLevelsForAge(17);
        Assert.Contains(AcademicLevel.HighSchoolFirstGrade, highSchoolLevels);
        Assert.Contains(AcademicLevel.HighSchoolSecondGrade, highSchoolLevels);
        Assert.Contains(AcademicLevel.HighSchoolThirdGrade, highSchoolLevels);
        Assert.Contains(AcademicLevel.ProgrammingFoundations, highSchoolLevels);
    }

    [Fact]
    public void AdaptiveEngine_GetRecommendedLevelForAge_ReturnsAppropriateDefaultLevel()
    {
        Assert.Equal(AcademicLevel.Kindergarten, AdaptiveEngine.GetRecommendedLevelForAge(4));
        Assert.Equal(AcademicLevel.EarlyElementary, AdaptiveEngine.GetRecommendedLevelForAge(7));
        Assert.Equal(AcademicLevel.MiddleElementary, AdaptiveEngine.GetRecommendedLevelForAge(10));
        Assert.Equal(AcademicLevel.UpperElementary, AdaptiveEngine.GetRecommendedLevelForAge(12));
        Assert.Equal(AcademicLevel.SecondaryFirstGrade, AdaptiveEngine.GetRecommendedLevelForAge(13));
        Assert.Equal(AcademicLevel.SecondarySecondGrade, AdaptiveEngine.GetRecommendedLevelForAge(14));
        Assert.Equal(AcademicLevel.SecondaryThirdGrade, AdaptiveEngine.GetRecommendedLevelForAge(15));
        Assert.Equal(AcademicLevel.HighSchoolFirstGrade, AdaptiveEngine.GetRecommendedLevelForAge(16));
        Assert.Equal(AcademicLevel.HighSchoolSecondGrade, AdaptiveEngine.GetRecommendedLevelForAge(17));
        Assert.Equal(AcademicLevel.HighSchoolThirdGrade, AdaptiveEngine.GetRecommendedLevelForAge(18));
    }

    [Fact]
    public void CurriculumService_GetTopicsForAge_FiltersOutInappropriateLevels()
    {
        var age5Topics = _curriculum.GetTopicsForAge(5);
        Assert.NotEmpty(age5Topics);
        Assert.All(age5Topics, t => Assert.Equal(AcademicLevel.Kindergarten, t.Level));

        var age8Topics = _curriculum.GetTopicsForAge(8);
        Assert.NotEmpty(age8Topics);
        Assert.All(age8Topics, t =>
            Assert.True(t.Level is AcademicLevel.EarlyElementary or AcademicLevel.MiddleElementary or AcademicLevel.UpperElementary));

        var age14Topics = _curriculum.GetTopicsForAge(14);
        Assert.NotEmpty(age14Topics);
        Assert.All(age14Topics, t =>
            Assert.True(t.Level is AcademicLevel.SecondaryFirstGrade or AcademicLevel.SecondarySecondGrade or AcademicLevel.SecondaryThirdGrade));

        var age17Topics = _curriculum.GetTopicsForAge(17);
        Assert.Equal(_curriculum.GetAllTopics().Count, age17Topics.Count);
    }

    [Fact]
    public void CurriculumService_GetNextTopic_ResolvesNextSequentialMissionWithinAgeFilter()
    {
        var profile = new UserProfile { Age = 8 };
        var nextFromAddition = _curriculum.GetNextTopic("elem_addition_01", profile);

        Assert.NotNull(nextFromAddition);
        Assert.Equal("elem_subtraction_01", nextFromAddition.Id);
    }

    [Fact]
    public void CurriculumService_GetNextTopic_ReturnsNullWhenAllStageMissionsCompleted()
    {
        var profile = new UserProfile
        {
            Age = 4,
            CompletedLessonIds = { "kinder_counting_01" }
        };

        var nextAfterKinder = _curriculum.GetNextTopic("kinder_counting_01", profile);
        Assert.Null(nextAfterKinder);
    }

    [Fact]
    public void AdaptiveEngine_GetAllowedLevelsForAge_IncludesSecondaryFirstGradeAtAge12AndFoundationalForIntellectualDisability()
    {
        // A los 12 años se permite tanto Primaria como 1º de Secundaria (Tarea 12)
        var age12Levels = AdaptiveEngine.GetAllowedLevelsForAge(12);
        Assert.Contains(AcademicLevel.UpperElementary, age12Levels);
        Assert.Contains(AcademicLevel.SecondaryFirstGrade, age12Levels);

        // Un adolescente de 14 años con Discapacidad Intelectual conserva acceso a niveles base de refuerzo
        var age14Intellectual = AdaptiveEngine.GetAllowedLevelsForAge(14, DisabilityType.Intellectual);
        Assert.Contains(AcademicLevel.Kindergarten, age14Intellectual);
        Assert.Contains(AcademicLevel.EarlyElementary, age14Intellectual);
        Assert.Contains(AcademicLevel.MiddleElementary, age14Intellectual);
        Assert.Contains(AcademicLevel.UpperElementary, age14Intellectual);
        Assert.Contains(AcademicLevel.SecondarySecondGrade, age14Intellectual);
    }

    [Fact]
    public void UserProfile_SupportsMultipleDisabilities_AndCombinesAdaptationsSimultaneously()
    {
        var profile = new UserProfile
        {
            Age = 10,
            Disabilities = { DisabilityType.Visual, DisabilityType.Neurodivergent },
            Severity = DisabilitySeverity.Moderate
        };

        profile.ApplyRecommendedAccessibilityDefaults();

        // Ambas adaptaciones deben activarse simultáneamente
        Assert.True(profile.HasDisability(DisabilityType.Visual));
        Assert.True(profile.HasDisability(DisabilityType.Neurodivergent));
        Assert.False(profile.HasDisability(DisabilityType.Hearing));

        // Adaptaciones de ceguera/baja visión:
        Assert.True(profile.Preferences.ScreenReaderAnnouncements);
        Assert.True(profile.Preferences.EnableVoiceSynthesis);
        Assert.Equal(FontSizeLevel.ExtraLarge, profile.Preferences.FontSize);

        // Adaptaciones de TEA / neurodivergencia:
        Assert.Equal(-1, profile.Preferences.SpeechRate);
        Assert.True(profile.Preferences.UnlimitedTime);

        // Caso de comorbilidad Intelectual + Motriz
        var profile2 = new UserProfile
        {
            Age = 14,
            Disabilities = { DisabilityType.Intellectual, DisabilityType.Motor }
        };
        profile2.ApplyRecommendedAccessibilityDefaults();

        Assert.True(profile2.Preferences.SimplifyLanguage);
        Assert.True(profile2.Preferences.ReduceOptionsCount);
        Assert.True(profile2.Preferences.SimplifiedKeyboardNavigation);

        // Los niveles escolares deben incluir refuerzo pedagógico
        var allowed = AdaptiveEngine.GetAllowedLevelsForAge(profile2.Age, profile2.Disabilities);
        Assert.Contains(AcademicLevel.Kindergarten, allowed);
        Assert.Contains(AcademicLevel.EarlyElementary, allowed);
    }

    [Fact]
    public void BuildOptionsAnnouncement_FormatsExplicitOptionsAndKeyInstructionsAccurately()
    {
        var options = new List<ExerciseOption>
        {
            new ExerciseOption { Id = "1", Label = "3 manzanas", Value = 3, IsCorrect = false },
            new ExerciseOption { Id = "2", Label = "4 manzanas", Value = 4, IsCorrect = true },
            new ExerciseOption { Id = "3", Label = "5 manzanas", Value = 5, IsCorrect = false }
        };

        string announcement = AdaptiveEngine.BuildOptionsAnnouncement(options);
        Assert.Equal("Opción 1: 3 manzanas. Opción 2: 4 manzanas. Opción 3: 5 manzanas. Presiona 1, 2 o 3 para responder.", announcement);

        var twoOptions = options.Take(2).ToList();
        string twoAnnounce = AdaptiveEngine.BuildOptionsAnnouncement(twoOptions);
        Assert.Equal("Opción 1: 3 manzanas. Opción 2: 4 manzanas. Presiona 1 o 2 para responder.", twoAnnounce);
    }

    [Fact]
    public void All45Topics_SupportStepByStepInteractiveLessonStructure_WithNarrativeConceptAndGuidedExample()
    {
        var topics = _curriculum.GetAllTopics();
        Assert.Equal(45, topics.Count);

        foreach (var topic in topics)
        {
            // Paso 1: Historia y narrativa
            Assert.False(string.IsNullOrWhiteSpace(topic.NarrativeIntro), $"El tema {topic.Id} carece de narrativa introductoria.");

            // Paso 2: Concepto clave y principios
            Assert.False(string.IsNullOrWhiteSpace(topic.CoreConceptExplanation), $"El tema {topic.Id} carece de explicación del concepto.");
            Assert.NotEmpty(topic.PrinciplesAddressed);

            // Paso 3: Reto y pistas guiadas
            if (topic.Exercises.Count > 0)
            {
                var sample = topic.Exercises[0];
                Assert.False(string.IsNullOrWhiteSpace(sample.FirstHint), $"El ejercicio base de {topic.Id} carece de FirstHint.");
                Assert.False(string.IsNullOrWhiteSpace(sample.SecondHintDetailed), $"El ejercicio base de {topic.Id} carece de SecondHintDetailed.");
                Assert.False(string.IsNullOrWhiteSpace(sample.ExplanationOfSuccess), $"El ejercicio base de {topic.Id} carece de ExplanationOfSuccess.");
            }
        }
    }

    [Fact]
    public void CurriculumService_UnlocksSubsequentStageMissions_WhenBaseStageCompleted()
    {
        var profile = new UserProfile
        {
            Age = 5,
            CompletedLessonIds = { "kinder_counting_01" }
        };

        // Antes de completar, solo incluye Kínder
        var initialTopics = _curriculum.GetTopicsForAge(5);
        Assert.Single(initialTopics);
        Assert.Equal("kinder_counting_01", initialTopics[0].Id);

        // Tras completar, incluye misiones de la siguiente etapa (Primaria Inicial)
        var unlockedTopics = _curriculum.GetTopicsForAge(5, DisabilityType.None, profile.CompletedLessonIds);
        Assert.True(unlockedTopics.Count > 1);
        Assert.Contains(unlockedTopics, t => t.Level == AcademicLevel.EarlyElementary);

        // GetNextTopic avanza hacia la primera misión de primaria
        var nextTopic = _curriculum.GetNextTopic("kinder_counting_01", profile);
        Assert.NotNull(nextTopic);
        Assert.Equal("elem_addition_01", nextTopic.Id);
    }

    [Fact]
    public void CurriculumService_GenerateAdaptiveExercises_ShufflesCuratedFirstExerciseOptions()
    {
        var profile = new UserProfile { Age = 7, CurrentLevel = AcademicLevel.EarlyElementary };
        var topic = _curriculum.GetAllTopics().Find(t => t.Id == "elem_addition_01")!;
        int option1Count = 0;
        int option2Count = 0;
        int option3Count = 0;

        for (int i = 0; i < 60; i++)
        {
            var exercises = _curriculum.GenerateAdaptiveExercises(topic, profile, count: 1);
            Assert.NotEmpty(exercises);
            var firstEx = exercises[0];
            int correctIdx = firstEx.Options.FindIndex(o => o.IsCorrect);
            Assert.True(correctIdx >= 0 && correctIdx < firstEx.Options.Count);

            if (correctIdx == 0) option1Count++;
            else if (correctIdx == 1) option2Count++;
            else if (correctIdx == 2) option3Count++;
        }

        // La respuesta correcta debe distribuirse entre las posiciones en vez de estar siempre fija en la 2
        Assert.True(option1Count > 0, "La opción correcta nunca cayó en la posición 1");
        Assert.True(option2Count > 0, "La opción correcta nunca cayó en la posición 2");
        Assert.True(option3Count > 0, "La opción correcta nunca cayó en la posición 3");
    }
}


