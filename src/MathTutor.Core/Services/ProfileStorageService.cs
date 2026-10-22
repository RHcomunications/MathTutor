using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MathTutor.Core.Models;

namespace MathTutor.Core.Services;

/// <summary>
/// Servicio de persistencia local para el perfil del alumno y sus preferencias de accesibilidad.
/// </summary>
public class ProfileStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string DefaultFilePath { get; }

    public ProfileStorageService(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            DefaultFilePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = Path.Combine(appData, "MathTutor");
            DefaultFilePath = Path.Combine(folder, "user_profile.json");
        }
    }

    /// <summary>
    /// Guarda el perfil del usuario en disco.
    /// </summary>
    public void Save(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        string? directory = Path.GetDirectoryName(DefaultFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(profile, JsonOptions);
        File.WriteAllText(DefaultFilePath, json);
    }

    /// <summary>
    /// Carga el perfil del usuario desde el disco. Retorna null si no existe o si ocurre un error de lectura.
    /// </summary>
    public UserProfile? Load()
    {
        if (!File.Exists(DefaultFilePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(DefaultFilePath);
            string? legacySeverity = null;
            bool hasProfileVersion = false;

            using (var document = JsonDocument.Parse(json))
            {
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                {
                    hasProfileVersion = document.RootElement.TryGetProperty(nameof(UserProfile.ProfileVersion), out _);
                    if (document.RootElement.TryGetProperty("Severity", out var severityElement) &&
                        severityElement.ValueKind == JsonValueKind.String)
                    {
                        legacySeverity = severityElement.GetString();
                    }
                }
            }

            var profile = JsonSerializer.Deserialize<UserProfile>(json, JsonOptions);
            if (profile != null)
            {
                MigrateLegacyProfile(profile, legacySeverity, force: !hasProfileVersion);
            }

            return profile;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Migra perfiles anteriores a la versión 2. La severidad genérica nunca se
    /// convierte en un nivel de apoyo autista; solo se traduce a descriptores
    /// funcionales o apoyos intelectuales cuando corresponde.
    /// </summary>
    public static void MigrateLegacyProfile(UserProfile profile, string? legacySeverity, bool force = false)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (!force && profile.ProfileVersion >= UserProfile.CurrentProfileVersion)
        {
            return;
        }

        bool hadLegacyNeurodivergence =
            profile.Disability == DisabilityType.Neurodivergent ||
            profile.Disabilities.Contains(DisabilityType.Neurodivergent);

        if (hadLegacyNeurodivergence)
        {
            profile.Disabilities.RemoveAll(d => d == DisabilityType.Neurodivergent);
            if (!profile.Disabilities.Contains(DisabilityType.Autism))
            {
                profile.Disabilities.Add(DisabilityType.Autism);
            }
            if (!profile.Disabilities.Contains(DisabilityType.ADHD))
            {
                profile.Disabilities.Add(DisabilityType.ADHD);
            }

            if (profile.Disability == DisabilityType.Neurodivergent)
            {
                profile.Disability = DisabilityType.Autism;
            }
        }

        if (profile.Disabilities.Count == 0 && profile.Disability != DisabilityType.None)
        {
            profile.Disabilities.Add(profile.Disability);
        }

        bool severe = string.Equals(legacySeverity, "Severe", StringComparison.OrdinalIgnoreCase);
        bool moderate = string.Equals(legacySeverity, "Moderate", StringComparison.OrdinalIgnoreCase);

        if (profile.HasDisability(DisabilityType.Visual) &&
            profile.Support.VisualFunction == VisualFunctionalLevel.NotApplicable)
        {
            profile.Support.VisualFunction = severe
                ? VisualFunctionalLevel.Blindness
                : VisualFunctionalLevel.LowVision;
        }

        if (profile.HasDisability(DisabilityType.Hearing) &&
            profile.Support.AuditoryFunction == AuditoryFunctionalLevel.NotApplicable)
        {
            profile.Support.AuditoryFunction = severe
                ? AuditoryFunctionalLevel.Deaf
                : AuditoryFunctionalLevel.HardOfHearing;
        }

        if (profile.HasDisability(DisabilityType.Motor) &&
            profile.Support.MotorFunction == MotorFunctionalLevel.NotApplicable)
        {
            profile.Support.MotorFunction = severe
                ? MotorFunctionalLevel.ReducedMobility
                : MotorFunctionalLevel.FineMotorDifficulties;
        }

        if (profile.HasDisability(DisabilityType.Intellectual) &&
            profile.Support.IntellectualSupport == IntellectualSupportLevel.NotApplicable)
        {
            profile.Support.IntellectualSupport = severe
                ? IntellectualSupportLevel.Extensive
                : moderate
                    ? IntellectualSupportLevel.Limited
                    : IntellectualSupportLevel.Intermittent;
        }

        profile.ProfileVersion = UserProfile.CurrentProfileVersion;
    }

    /// <summary>
    /// Exporta una copia legible del perfil y su progreso a una ruta elegida por la familia.
    /// </summary>
    public void Export(UserProfile profile, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        string? directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(profile, JsonOptions);
        File.WriteAllText(destinationPath, json);
    }

    /// <summary>
    /// Borra el perfil local. Devuelve true si existía un archivo que eliminar.
    /// </summary>
    public bool Delete()
    {
        if (!File.Exists(DefaultFilePath))
        {
            return false;
        }

        File.Delete(DefaultFilePath);
        return true;
    }

    /// <summary>
    /// Indica si ya existe un perfil guardado previamente en el equipo.
    /// </summary>
    public bool Exists()
    {
        return File.Exists(DefaultFilePath);
    }
}
