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
            Severity = DisabilitySeverity.Moderate,
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
        Assert.Equal(DisabilitySeverity.Moderate, loaded.Severity);
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
