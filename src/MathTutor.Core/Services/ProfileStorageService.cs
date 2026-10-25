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
    private static readonly object FileGate = new();

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
    /// Guarda el perfil del usuario en disco mediante una escritura atómica:
    /// primero se escribe un archivo temporal en el mismo directorio y luego
    /// se reemplaza el archivo final con rename. Un corte de luz o un fallo
    /// a mitad de la escritura nunca trunca el perfil ya guardado.
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
        string tempPath = DefaultFilePath + ".tmp";

        lock (FileGate)
        {
            try
            {
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, DefaultFilePath, overwrite: true);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch (IOException)
                {
                    // El rename ya se aplicó; el temporal aislado se limpia con el SO.
                }
            }
        }
    }

    /// <summary>
    /// Carga el perfil del usuario desde el disco. Retorna null si no existe o si el
    /// archivo está dañado; un JSON corrupto se pone en cuarentena con sufijo
    /// <c>.corrupt</c> en lugar de perderse, y la migración resultante se persiste.
    /// </summary>
    public UserProfile? Load()
    {
        if (!File.Exists(DefaultFilePath))
        {
            return null;
        }

        lock (FileGate)
        {
            return LoadCore();
        }
    }

    private UserProfile? LoadCore()
    {
        string json;
        try
        {
            json = File.ReadAllText(DefaultFilePath);
        }
        catch (IOException)
        {
            return null;
        }

        string? legacySeverity = null;
        bool hasProfileVersion = false;
        bool validJson = true;

        try
        {
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
        }
        catch (Exception)
        {
            validJson = false;
        }

        if (!validJson)
        {
            QuarantineCorruptProfile(DefaultFilePath);
            return null;
        }

        UserProfile? profile;
        try
        {
            profile = JsonSerializer.Deserialize<UserProfile>(json, JsonOptions);
        }
        catch (JsonException)
        {
            QuarantineCorruptProfile(DefaultFilePath);
            return null;
        }

        // JSON válido que no representa un objeto de perfil (p. ej. un array
        // o el literal null).
        if (profile is null)
        {
            QuarantineCorruptProfile(DefaultFilePath);
            return null;
        }

        bool needsPersistence = !hasProfileVersion ||
            profile.ProfileVersion < UserProfile.CurrentProfileVersion;

        MigrateLegacyProfile(profile, legacySeverity, force: !hasProfileVersion);

        if (needsPersistence)
        {
            // Persistir la migración para que no se repita en cada arranque.
            // Un fallo transitorio de escritura nunca invalida el perfil: el
            // objeto en memoria es correcto y la próxima carga reintenta.
            try
            {
                Save(profile);
            }
            catch (IOException)
            {
                // Disco completo o archivo bloqueado temporalmente.
            }
            catch (UnauthorizedAccessException)
            {
                // Permiso denegado transitorio (p. ej. bloqueo antivirus).
            }
        }

        profile.RecalculateStars();
        return profile;
    }

    private static void QuarantineCorruptProfile(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss-fff");
        string quarantinePath = path + $".corrupt-{timestamp}";
        try
        {
            File.Move(path, quarantinePath);
        }
        catch (IOException)
        {
            // Archivo bloqueado: se deja en sitio; el próximo arranque reintenta.
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
    /// Borra el perfil local. Devuelve true si existía un archivo y se eliminó con éxito;
    /// false si no existía o si el archivo estaba bloqueado por otro proceso.
    /// </summary>
    public bool Delete()
    {
        lock (FileGate)
        {
            if (!File.Exists(DefaultFilePath))
            {
                return false;
            }

            try
            {
                File.Delete(DefaultFilePath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Indica si ya existe un perfil guardado previamente en el equipo.
    /// </summary>
    public bool Exists()
    {
        return File.Exists(DefaultFilePath);
    }

    /// <summary>
    /// Indica si ya existe un perfil guardado previamente en el equipo.
    /// </summary>
    public bool ProfileExists() => Exists();
}
