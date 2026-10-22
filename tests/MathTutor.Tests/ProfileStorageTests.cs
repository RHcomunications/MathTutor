using System;
using System.IO;
using MathTutor.Core.Models;
using MathTutor.Core.Services;
using Xunit;

namespace MathTutor.Tests;

public class ProfileStorageTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly ProfileStorageService _storage;

    public ProfileStorageTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"mathtutor_test_{Guid.NewGuid():N}.json");
        _storage = new ProfileStorageService(_testFilePath);
    }

    [Fact]
    public void Exists_ReturnsFalse_WhenNoProfileSaved()
    {
        Assert.False(_storage.Exists());
        Assert.Null(_storage.Load());
    }

    [Fact]
    public void SaveAndLoad_PersistsAllProfileFieldsAccurately()
    {
        var profile = new UserProfile
        {
            Name = "Sofía Exploradora",
            Age = 6,
            Disability = DisabilityType.Intellectual,
            Support = { IntellectualSupport = IntellectualSupportLevel.Limited },
            CurrentLevel = AcademicLevel.EarlyElementary,
            StarsTotal = 15,
            PointsTotal = 150,
            CurrentStreak = 4
        };
        profile.Preferences.Theme = HighContrastTheme.HighContrastDark;
        profile.Preferences.FontSize = FontSizeLevel.ExtraLarge;
        profile.Preferences.SimplifyLanguage = true;
        profile.Preferences.EnableVoiceSynthesis = true;
        profile.CompletedLessonIds.Add("kinder_counting_01");
        profile.Disabilities.Add(DisabilityType.Visual);

        _storage.Save(profile);

        Assert.True(_storage.Exists());

        var loaded = _storage.Load();
        Assert.NotNull(loaded);
        Assert.Equal("Sofía Exploradora", loaded.Name);
        Assert.Equal(6, loaded.Age);
        Assert.Equal(DisabilityType.Intellectual, loaded.Disability);
        Assert.Equal(2, loaded.Disabilities.Count);
        Assert.Contains(DisabilityType.Intellectual, loaded.Disabilities);
        Assert.Contains(DisabilityType.Visual, loaded.Disabilities);
        Assert.Equal(UserProfile.CurrentProfileVersion, loaded.ProfileVersion);
        Assert.Equal(IntellectualSupportLevel.Limited, loaded.Support.IntellectualSupport);
        Assert.Equal(AcademicLevel.EarlyElementary, loaded.CurrentLevel);
        Assert.Equal(15, loaded.StarsTotal);
        Assert.Equal(150, loaded.PointsTotal);
        Assert.Equal(4, loaded.CurrentStreak);
        Assert.Equal(HighContrastTheme.HighContrastDark, loaded.Preferences.Theme);
        Assert.Equal(FontSizeLevel.ExtraLarge, loaded.Preferences.FontSize);
        Assert.True(loaded.Preferences.SimplifyLanguage);
        Assert.True(loaded.Preferences.EnableVoiceSynthesis);
        Assert.Contains("kinder_counting_01", loaded.CompletedLessonIds);
    }

    [Fact]
    public void SaveAndLoad_PreservesExpandedAndCollapsedBadgeGroups()
    {
        var profile = new UserProfile();
        profile.Preferences.BadgeGroupExpansion["Kindergarten"] = true;
        profile.Preferences.BadgeGroupExpansion["general"] = false;

        _storage.Save(profile);
        var loaded = _storage.Load();

        Assert.NotNull(loaded);
        Assert.True(loaded.Preferences.BadgeGroupExpansion["Kindergarten"]);
        Assert.False(loaded.Preferences.BadgeGroupExpansion["general"]);
    }

    [Fact]
    public void Load_OlderProfileWithoutBadgeGroups_UsesEmptyExpansionState()
    {
        File.WriteAllText(_testFilePath, "{\"Preferences\":{\"FontSize\":\"ExtraLarge\"}}");

        var loaded = _storage.Load();

        Assert.NotNull(loaded);
        Assert.Empty(loaded.Preferences.BadgeGroupExpansion);
        Assert.Equal(FontSizeLevel.ExtraLarge, loaded.Preferences.FontSize);
    }

    [Fact]
    public void Load_LegacyNeurodivergent_SplitsIntoAutismAndAdhd_WithoutInventingSupportLevel()
    {
        File.WriteAllText(
            _testFilePath,
            "{\"Age\":10,\"Disability\":\"Neurodivergent\",\"Disabilities\":[\"Neurodivergent\"],\"Severity\":\"Severe\"}");

        var loaded = _storage.Load();

        Assert.NotNull(loaded);
        Assert.True(loaded.HasDisability(DisabilityType.Autism));
        Assert.True(loaded.HasDisability(DisabilityType.ADHD));
        Assert.False(loaded.HasDisability(DisabilityType.Neurodivergent));
        Assert.Equal(AutismSupportLevel.NotApplicable, loaded.Support.AutismSupport);
        Assert.Equal(UserProfile.CurrentProfileVersion, loaded.ProfileVersion);
    }

    [Fact]
    public void Load_LegacySeverity_MapsToFunctionalDescriptors_NotAutismLevel()
    {
        File.WriteAllText(
            _testFilePath,
            "{\"Age\":10,\"Disability\":\"Visual\",\"Disabilities\":[\"Visual\"],\"Severity\":\"Severe\"}");

        var loaded = _storage.Load();

        Assert.NotNull(loaded);
        Assert.Equal(VisualFunctionalLevel.Blindness, loaded.Support.VisualFunction);
        Assert.Equal(AutismSupportLevel.NotApplicable, loaded.Support.AutismSupport);
        Assert.Equal(UserProfile.CurrentProfileVersion, loaded.ProfileVersion);
    }

    [Fact]
    public void ApplyRecommendedDefaults_DoesNotOverwriteManualPreferences()
    {
        var profile = new UserProfile
        {
            Disability = DisabilityType.Visual,
            Support = { VisualFunction = VisualFunctionalLevel.Blindness }
        };
        profile.Preferences.Theme = HighContrastTheme.HighContrastLight;
        profile.Preferences.ScreenReaderAnnouncements = false;
        profile.Preferences.MarkManual(AccessibilityPreferences.ThemePreference);
        profile.Preferences.MarkManual(AccessibilityPreferences.ScreenReaderPreference);

        profile.ApplyRecommendedAccessibilityDefaults();

        Assert.Equal(HighContrastTheme.HighContrastLight, profile.Preferences.Theme);
        Assert.False(profile.Preferences.ScreenReaderAnnouncements);
        Assert.True(profile.Preferences.EnableVoiceSynthesis);
    }

    [Fact]
    public void Export_WritesProfileAndProgressToChosenPath()
    {
        string exportPath = Path.Combine(Path.GetTempPath(), $"mathtutor_export_{Guid.NewGuid():N}.json");
        var profile = new UserProfile
        {
            Name = "Mateo",
            Age = 7,
            Support = { AutismSupport = AutismSupportLevel.Level1 }
        };
        profile.CompletedLessonIds.Add("kinder_counting_01");

        try
        {
            _storage.Export(profile, exportPath);

            Assert.True(File.Exists(exportPath));
            string json = File.ReadAllText(exportPath);
            Assert.Contains("\"Name\": \"Mateo\"", json);
            Assert.Contains("kinder_counting_01", json);
            Assert.Contains("Level1", json);
        }
        finally
        {
            if (File.Exists(exportPath))
            {
                File.Delete(exportPath);
            }
        }
    }

    [Fact]
    public void Delete_RemovesSavedProfileAndReportsWhetherItExisted()
    {
        _storage.Save(new UserProfile { Name = "Para borrar" });

        Assert.True(_storage.Exists());
        Assert.True(_storage.Delete());
        Assert.False(_storage.Exists());
        Assert.False(_storage.Delete());
        Assert.Null(_storage.Load());
    }

    [Fact]
    public void Profile_DefaultsToCurrentVersionAndKeepsAutismAndAdhdSeparate()
    {
        var profile = new UserProfile
        {
            Disabilities = { DisabilityType.Autism, DisabilityType.ADHD },
            Support =
            {
                AutismSupport = AutismSupportLevel.Level2,
                AdhdPresentation = AdhdPresentation.Combined
            }
        };

        Assert.Equal(UserProfile.CurrentProfileVersion, profile.ProfileVersion);
        Assert.True(profile.HasDisability(DisabilityType.Autism));
        Assert.True(profile.HasDisability(DisabilityType.ADHD));
        Assert.Equal(AutismSupportLevel.Level2, profile.Support.AutismSupport);
        Assert.Equal(AdhdPresentation.Combined, profile.Support.AdhdPresentation);
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
        catch
        {
            // Limpieza segura
        }
    }
}
