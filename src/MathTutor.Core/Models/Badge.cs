using System;

namespace MathTutor.Core.Models;

/// <summary>
/// Insignia o trofeo lúdico que premia el esfuerzo y perseverancia del alumno en el Reino Matemágico.
/// </summary>
public class Badge
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = "🏅";
    public string AssociatedTopicId { get; set; } = string.Empty;
    public int RequiredStars { get; set; } = 3;
    public bool IsUnlocked { get; set; } = false;
    public DateTime? UnlockedAt { get; set; }
}

/// <summary>
/// Estación o monumento en el Mapa del Reino Matemágico.
/// </summary>
public class RealmStation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LandmarkEmoji { get; set; } = "🏰";
    public string TopicId { get; set; } = string.Empty;
    public int OrderIndex { get; set; } = 1;
    public bool IsCompleted { get; set; } = false;
    public bool IsCurrentStation { get; set; } = false;
    public int StarsEarned { get; set; } = 0;
}
