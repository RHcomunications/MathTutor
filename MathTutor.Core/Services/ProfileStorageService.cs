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
            return JsonSerializer.Deserialize<UserProfile>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Indica si ya existe un perfil guardado previamente en el equipo.
    /// </summary>
    public bool Exists()
    {
        return File.Exists(DefaultFilePath);
    }
}
