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
