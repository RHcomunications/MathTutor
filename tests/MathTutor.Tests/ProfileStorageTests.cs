using System;
using System.IO;
using System.Linq;
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
    public void Load_CorruptJson_QuarantinesFileInsteadOfSilentlyLosingIt()
    {
        File.WriteAllText(_testFilePath, "esto no es json {{{");

        Assert.Null(_storage.Load());
        Assert.False(File.Exists(_testFilePath));

        string? quarantined = Directory
            .EnumerateFiles(Path.GetTempPath(), Path.GetFileName(_testFilePath) + ".corrupt-*")
            .FirstOrDefault();
        Assert.True(quarantined is not null, "El archivo corrupto debió ponerse en cuarentena");

        string quarantinedText = File.ReadAllText(quarantined);
        Assert.Equal("esto no es json {{{", quarantinedText);
    }

    [Fact]
    public void Load_TruncatedJson_SimulatingCrashDuringWrite_ReturnsNullAndQuarantines()
    {
        // Simula el resultado de un corte de luz a mitad de una escritura no atómica.
        File.WriteAllText(_testFilePath, "{\"Name\": \"SofÍa Ex");

        Assert.Null(_storage.Load());
        Assert.False(File.Exists(_testFilePath));
        Assert.Contains(
            Directory.EnumerateFiles(Path.GetTempPath()),
            path => Path.GetFileName(path).StartsWith(Path.GetFileName(_testFilePath) + ".corrupt-"));
    }

    [Fact]
    public void Load_ValidJsonThatIsNotAProfileObject_QuarantinesAndReturnsNull()
    {
        File.WriteAllText(_testFilePath, "[1, 2, 3]");

        Assert.Null(_storage.Load());
        Assert.False(File.Exists(_testFilePath));
        Assert.Contains(
            Directory.EnumerateFiles(Path.GetTempPath()),
            path => Path.GetFileName(path).StartsWith(Path.GetFileName(_testFilePath) + ".corrupt-"));
    }

    [Fact]
    public void Save_CleansUpTempFile_AfterSuccessfulSave()
    {
        _storage.Save(new UserProfile { Name = "Atómico" });
        bool tempExists = File.Exists(_testFilePath + ".tmp");

        Assert.False(tempExists, "El archivo temporal debe desaparecer tras el rename");
        Assert.Equal("Atómico", _storage.Load()?.Name);
    }

    [Fact]
    public void SaveAndLoad_PersistsLessonStarRatings_PerLesson()
    {
        var profile = new UserProfile { Name = "Calificada" };
        profile.CompletedLessonIds.Add("elem_addition_01");
        profile.LessonStarRatings["elem_addition_01"] = 3;
        profile.LessonStarRatings["kinder_counting_01"] = 2;

        _storage.Save(profile);
        var loaded = _storage.Load();

        Assert.NotNull(loaded);
        Assert.Equal(3, loaded.LessonStarRatings["elem_addition_01"]);
        Assert.Equal(2, loaded.LessonStarRatings["kinder_counting_01"]);
        Assert.Equal(2, loaded.LessonStarRatings.Count);
    }

    [Fact]
    public void Load_LegacyProfile_PersistsMigratedResultOnDisk()
    {
        File.WriteAllText(
            _testFilePath,
            "{\"Age\":10,\"Disability\":\"Visual\",\"Disabilities\":[\"Visual\"],\"Severity\":\"Severe\"}");

        var loaded = _storage.Load();
        Assert.NotNull(loaded);
        Assert.Equal(VisualFunctionalLevel.Blindness, loaded.Support.VisualFunction);

        // El archivo en disco ya migrado no debe volver a migrar en cada arranque.
        string jsonOnDisk = File.ReadAllText(_testFilePath);
        Assert.Contains("ProfileVersion", jsonOnDisk);
        Assert.DoesNotContain("Severity", jsonOnDisk);

        var reloaded = _storage.Load();
        Assert.NotNull(reloaded);
        Assert.Equal(loaded.Support.VisualFunction, reloaded.Support.VisualFunction);
    }

    [Fact]
    public void Load_MigrationPersistFailsTransiently_ProfileIsReturnedNotQuarantined()
    {
        File.WriteAllText(
            _testFilePath,
            "{\"Age\":8,\"Disability\":\"Neurodivergent\",\"Disabilities\":[\"Neurodivergent\"]}");

        // El archivo temporal se sustituye por un directorio: Save no puede
        // escribir y la re-persistencia de la migración falla.
        Directory.CreateDirectory(_testFilePath + ".tmp");
        try
        {
            var loaded = _storage.Load();

            // El JSON era válido: se devuelve el perfil migrado y JAMÁS se
            // cuarentena por una falla de escritura.
            Assert.NotNull(loaded);
            Assert.Equal(UserProfile.CurrentProfileVersion, loaded.ProfileVersion);
            Assert.Contains(DisabilityType.Autism, loaded.Disabilities);
            Assert.Contains(DisabilityType.ADHD, loaded.Disabilities);
        }
        finally
        {
            if (Directory.Exists(_testFilePath + ".tmp"))
            {
                Directory.Delete(_testFilePath + ".tmp");
            }
        }

        Assert.Empty(Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(_testFilePath) + ".corrupt-*"));
        Assert.True(_storage.Exists());

        // Con el camino despejado, el siguiente arranque persiste la migración.
        var reloaded = _storage.Load();
        Assert.NotNull(reloaded);
        string jsonOnDisk = File.ReadAllText(_testFilePath);
        Assert.Contains("ProfileVersion", jsonOnDisk);
    }

    [Fact]
    public void ConcurrentSavesAndLoadsAndDelete_NeverQuarantineAValidProfile()
    {
        var storage = new ProfileStorageService(_testFilePath);
        const int workers = 4;
        const int iterations = 25;
        var failures = new System.Collections.Concurrent.ConcurrentQueue<Exception>();

        void SaveLoop(int workerIndex)
        {
            try
            {
                for (int i = 0; i < iterations; i++)
                {
                    storage.Save(new UserProfile
                    {
                        Name = $"Paola {workerIndex}-{i}",
                        Age = 6 + (i % 5)
                    });
                }
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
            }
        }

        void LoadLoop()
        {
            try
            {
                for (int i = 0; i < iterations; i++)
                {
                    if (storage.Exists())
                    {
                        storage.Load();
                    }
                }
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
            }
        }

        void DeleteLoop()
        {
            try
            {
                for (int i = 0; i < iterations; i++)
                {
                    storage.Delete();
                }
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
            }
        }

        var threads = new List<Thread>();
        for (int w = 0; w < workers; w++)
        {
            threads.Add(new Thread(() => SaveLoop(w)));
        }
        threads.Add(new Thread(LoadLoop));
        threads.Add(new Thread(DeleteLoop));

        foreach (var thread in threads)
        {
            thread.Start();
        }
        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.True(failures.IsEmpty, "Las operaciones concurrentes nunca deben lanzar: " +
            string.Join("; ", failures.Select(f => f.Message)));
        Assert.Empty(Directory.GetFiles(Path.GetTempPath(), Path.GetFileName(_testFilePath) + ".corrupt-*"));

        // El estado final es consistente: si hay archivo, es un perfil
        // válido deserializable (nunca JSON corrupto).
        if (storage.Exists())
        {
            var finalProfile = storage.Load();
            Assert.NotNull(finalProfile);
        }
    }

    [Fact]
    public void MigrateLegacyProfile_IsIdempotent_SecondCallLeavesProfileUnchanged()
    {
        var profile = new UserProfile
        {
            Disability = DisabilityType.Neurodivergent,
            Disabilities = { DisabilityType.Neurodivergent },
            Support = { VisualFunction = VisualFunctionalLevel.NotApplicable }
        };

        ProfileStorageService.MigrateLegacyProfile(profile, legacySeverity: null, force: true);
        var afterFirst = snapshotSupportState(profile);

        ProfileStorageService.MigrateLegacyProfile(profile, legacySeverity: null, force: true);
        var afterSecond = snapshotSupportState(profile);

        Assert.Equal(afterFirst, afterSecond);
        Assert.True(profile.HasDisability(DisabilityType.Autism));
        Assert.True(profile.HasDisability(DisabilityType.ADHD));
    }

    private static string snapshotSupportState(UserProfile profile)
    {
        return string.Join(
            "|",
            profile.Disabilities.OrderBy(d => d.ToString()),
            profile.Disability,
            profile.Support.VisualFunction,
            profile.Support.AuditoryFunction,
            profile.Support.MotorFunction,
            profile.Support.IntellectualSupport,
            profile.Support.AutismSupport,
            profile.Support.AdhdPresentation);
    }

    [Fact]
    public void SaveAndLoad_PersistsSupportDescriptorsForAutismAdhdHearingAndMotor()
    {
        var profile = new UserProfile
        {
            Name = "Perfil con apoyos completos",
            Age = 12,
            Disabilities = { DisabilityType.Autism, DisabilityType.ADHD, DisabilityType.Hearing, DisabilityType.Motor },
            Support =
            {
                AutismSupport = AutismSupportLevel.Level3,
                AdhdPresentation = AdhdPresentation.PredominantlyInattentive,
                AuditoryFunction = AuditoryFunctionalLevel.HardOfHearing,
                MotorFunction = MotorFunctionalLevel.FineMotorDifficulties
            }
        };

        _storage.Save(profile);
        var loaded = _storage.Load();

        Assert.NotNull(loaded);
        Assert.Equal(4, loaded.Disabilities.Count);
        Assert.True(loaded.HasDisability(DisabilityType.Autism));
        Assert.True(loaded.HasDisability(DisabilityType.ADHD));
        Assert.True(loaded.HasDisability(DisabilityType.Hearing));
        Assert.True(loaded.HasDisability(DisabilityType.Motor));
        Assert.Equal(AutismSupportLevel.Level3, loaded.Support.AutismSupport);
        Assert.Equal(AdhdPresentation.PredominantlyInattentive, loaded.Support.AdhdPresentation);
        Assert.Equal(AuditoryFunctionalLevel.HardOfHearing, loaded.Support.AuditoryFunction);
        Assert.Equal(MotorFunctionalLevel.FineMotorDifficulties, loaded.Support.MotorFunction);
    }

    [Fact]
    public void Delete_ReturnsFalse_WhenFileIsLockedByAnotherHandle()
    {
        _storage.Save(new UserProfile { Name = "Bloqueado" });
        FileStream? locked = null;

        try
        {
            locked = File.Open(
                _testFilePath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);

            // Borrado con el archivo bloqueado debe reportar false, no lanzar.
            bool deleted = _storage.Delete();

            Assert.False(deleted);
            Assert.True(_storage.Exists());
        }
        finally
        {
            locked?.Dispose();
        }

        // Al liberar el bloqueo el borrado debe funcionar con normalidad.
        Assert.True(_storage.Delete());
        Assert.False(_storage.Exists());
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
            string prefix = Path.GetFileName(_testFilePath);
            foreach (string file in Directory.EnumerateFiles(Path.GetTempPath(), prefix + "*"))
            {
                File.Delete(file);
            }
        }
        catch
        {
            // Limpieza segura
        }
    }
}
