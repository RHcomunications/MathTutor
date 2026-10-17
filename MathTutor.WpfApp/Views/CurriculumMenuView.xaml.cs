using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WpfApp.Services;

namespace MathTutor.WpfApp.Views;

public class TopicDisplayItem
{
    public string TopicId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LevelDescription { get; set; } = string.Empty;
    public string ConceptSnippet { get; set; } = string.Empty;
    public string PrinciplesSnippet { get; set; } = string.Empty;
    public string StatusBadgeText { get; set; } = string.Empty;
    public string ActionButtonLabel { get; set; } = "🚀 Entrar a Jugar";
    public string AccessibleButtonName { get; set; } = string.Empty;
}

public partial class CurriculumMenuView : UserControl
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly CurriculumService _curriculum = new();
    private readonly AdaptiveEngine _adaptive = new();

    public event Action<string>? TopicSelected;
    public event Action? RealmMapRequested;

    public CurriculumMenuView()
    {
        InitializeComponent();
        Loaded += CurriculumMenuView_Loaded;
    }

    private void CurriculumMenuView_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshData();
    }

    public void RefreshData()
    {
        var profile = _acc.CurrentProfile;

        TxtWelcomeUser.Text = $"¡Hola, {profile.Name}! Elige tu aventura";
        TxtUserSummary.Text = $"Nivel: {GetAcademicLevelName(profile.CurrentLevel)} | Condición: {GetDisabilityName(profile.Disability)}";
        TxtStarsCount.Text = profile.StarsTotal.ToString();
        TxtStreakCount.Text = profile.CurrentStreak.ToString();

        var allTopics = _curriculum.GetAllTopics();
        // Priorizar las misiones del nivel actual del alumno arriba, manteniendo el orden pedagógico en el resto
        var orderedTopics = allTopics
            .OrderByDescending(t => t.Level == profile.CurrentLevel)
            .ToList();

        var displayList = new List<TopicDisplayItem>();

        foreach (var t in orderedTopics)
        {
            string concept = _adaptive.GetAdaptedConceptExplanation(t, profile);
            if (concept.Length > 140) concept = concept.Substring(0, 137) + "...";

            string principles = "Enseña: " + string.Join(" • ", t.PrinciplesAddressed.Take(3));
            bool isCompleted = profile.CompletedLessonIds.Contains(t.Id);
            bool isCurrentLevel = t.Level == profile.CurrentLevel;

            string statusBadge = isCompleted
                ? "✅ ¡Misión Completada con Éxito! (⭐)"
                : isCurrentLevel
                    ? "🎯 Recomendada para tu Grado Actual"
                    : "🧭 Misión Disponible para Explorar";

            string btnLabel = isCompleted ? "🔄 Volver a Jugar" : "🚀 Entrar a Jugar";

            displayList.Add(new TopicDisplayItem
            {
                TopicId = t.Id,
                Title = t.Title,
                LevelDescription = GetAcademicLevelName(t.Level),
                ConceptSnippet = concept,
                PrinciplesSnippet = principles,
                StatusBadgeText = statusBadge,
                ActionButtonLabel = btnLabel,
                AccessibleButtonName = $"{btnLabel}: {t.Title}. {statusBadge}"
            });
        }

        ItemsTopics.ItemsSource = displayList;

        int completedCount = profile.CompletedLessonIds.Count;
        _acc.Announce($"Menú principal de aventuras. Nivel {GetAcademicLevelName(profile.CurrentLevel)}. Tienes {profile.StarsTotal} estrellas, {completedCount} de {allTopics.Count} misiones completadas. Las misiones de tu grado aparecen primero.", speak: true, playTone: true);
    }

    private void BtnEnterTopic_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string topicId)
        {
            _acc.Audio.PlayNavigationClick();
            TopicSelected?.Invoke(topicId);
        }
    }

    private void BtnOpenRealmMap_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        RealmMapRequested?.Invoke();
    }

    private void Button_GotFocus(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
    }

    private static string GetAcademicLevelName(AcademicLevel level) => level switch
    {
        AcademicLevel.Kindergarten => "Jardín de Niños (3 a 5 años)",
        AcademicLevel.EarlyElementary => "Primaria Baja (1º y 2º - 6 a 8 años)",
        AcademicLevel.MiddleElementary => "Primaria Media (3º y 4º - 9 a 10 años)",
        AcademicLevel.UpperElementary => "Primaria Alta (5º y 6º - 11 a 12 años)",
        AcademicLevel.SecondaryFirstGrade => "1º de Secundaria (12 a 13 años)",
        AcademicLevel.SecondarySecondGrade => "2º de Secundaria (13 a 14 años)",
        AcademicLevel.SecondaryThirdGrade => "3º de Secundaria (14 a 15 años)",
        AcademicLevel.HighSchoolFirstGrade => "1º de Preparatoria / Bachillerato (15 a 16 años)",
        AcademicLevel.HighSchoolSecondGrade => "2º de Preparatoria / Bachillerato (16 a 17 años)",
        AcademicLevel.HighSchoolThirdGrade => "3º de Preparatoria / Bachillerato (17 a 18 años)",
        AcademicLevel.ProgrammingFoundations => "Fundamentos de Programación y Algoritmos",
        _ => "General"
    };

    private static string GetDisabilityName(DisabilityType type) => type switch
    {
        DisabilityType.None => "Neurotípico",
        DisabilityType.Visual => "Visual",
        DisabilityType.Hearing => "Auditiva",
        DisabilityType.Motor => "Motriz",
        DisabilityType.Intellectual => "Intelectual",
        DisabilityType.Neurodivergent => "Neurodivergente",
        _ => "Estándar"
    };
}
