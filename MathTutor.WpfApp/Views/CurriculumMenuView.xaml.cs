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
    }

    public void RefreshData()
    {
        var profile = _acc.CurrentProfile;

        var allowedLevels = profile.Disabilities.Count > 0
            ? AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disabilities)
            : AdaptiveEngine.GetAllowedLevelsForAge(profile.Age, profile.Disability);
        if (!allowedLevels.Contains(profile.CurrentLevel))
        {
            profile.CurrentLevel = AdaptiveEngine.GetRecommendedLevelForAge(profile.Age);
        }

        TxtWelcomeUser.Text = $"¡Hola, {profile.Name}! Elige tu aventura";
        TxtUserSummary.Text = $"Edad: {profile.Age} años | Nivel: {GetAcademicLevelName(profile.CurrentLevel)} | Condición: {GetDisabilitySummary(profile)}";
        TxtStarsCount.Text = profile.StarsTotal.ToString();
        TxtStreakCount.Text = profile.CurrentStreak.ToString();

        // Tarea 6 y Tarea 12: Filtrar las misiones permitidas para la edad y condición del alumno y desbloquear subsecuentes
        var allowedTopics = profile.Disabilities.Count > 0
            ? _curriculum.GetTopicsForAge(profile.Age, profile.Disabilities, profile.CompletedLessonIds)
            : _curriculum.GetTopicsForAge(profile.Age, profile.Disability, profile.CompletedLessonIds);
        // Priorizar las misiones del nivel actual del alumno arriba, manteniendo el orden pedagógico en el resto
        var orderedTopics = allowedTopics
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

            // Tarea 3: Etiqueta accesible limpia sin emojis ni redundancia: "{Estado}: {NombreMision}"
            string cleanState = isCompleted
                ? "Misión completada"
                : isCurrentLevel
                    ? "Misión recomendada"
                    : "Misión disponible";

            displayList.Add(new TopicDisplayItem
            {
                TopicId = t.Id,
                Title = t.Title,
                LevelDescription = GetAcademicLevelName(t.Level),
                ConceptSnippet = concept,
                PrinciplesSnippet = principles,
                StatusBadgeText = statusBadge,
                ActionButtonLabel = btnLabel,
                AccessibleButtonName = $"{cleanState}: {t.Title}"
            });
        }

        ItemsTopics.ItemsSource = displayList;

        if (displayList.Count > 0)
        {
            ItemsTopics.SelectedIndex = 0;
        }

        // Tarea 1 y Tarea 2: Posicionar el foco real de teclado en el primer item de la lista de misiones
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
        {
            FocusFirstMissionItem();
        });

        int completedCount = allowedTopics.Count(t => profile.CompletedLessonIds.Contains(t.Id));
        _acc.Announce($"Bienvenido a MathTutor, {profile.Name}. Menú de misiones para {GetAcademicLevelName(profile.CurrentLevel)}. Tienes {profile.StarsTotal} estrellas y {completedCount} de {allowedTopics.Count} misiones completadas. Usa las flechas arriba y abajo para elegir misión y presiona Enter para comenzar.", speak: true, playTone: true);
    }

    public void FocusInitialElement() => FocusFirstMissionItem();

    public void FocusFirstMissionItem()
    {
        if (ItemsTopics.Items.Count > 0)
        {
            if (ItemsTopics.SelectedIndex < 0)
            {
                ItemsTopics.SelectedIndex = 0;
            }
            ItemsTopics.UpdateLayout();
            if (ItemsTopics.ItemContainerGenerator.ContainerFromIndex(ItemsTopics.SelectedIndex) is ListBoxItem firstItem)
            {
                firstItem.Focus();
                System.Windows.Input.Keyboard.Focus(firstItem);
                return;
            }
            else
            {
                EventHandler statusHandler = null!;
                statusHandler = (s, e) =>
                {
                    if (ItemsTopics.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        ItemsTopics.ItemContainerGenerator.StatusChanged -= statusHandler;
                        if (ItemsTopics.ItemContainerGenerator.ContainerFromIndex(ItemsTopics.SelectedIndex) is ListBoxItem generatedItem)
                        {
                            generatedItem.Focus();
                            System.Windows.Input.Keyboard.Focus(generatedItem);
                        }
                    }
                };
                ItemsTopics.ItemContainerGenerator.StatusChanged += statusHandler;
                return;
            }
        }

        BtnOpenRealmMap.Focus();
        System.Windows.Input.Keyboard.Focus(BtnOpenRealmMap);
    }

    private void ItemsTopics_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Space)
        {
            if (ItemsTopics.SelectedItem is TopicDisplayItem selected && !string.IsNullOrEmpty(selected.TopicId))
            {
                e.Handled = true;
                _acc.Audio.PlayNavigationClick();
                TopicSelected?.Invoke(selected.TopicId);
            }
        }
    }

    private void ItemsTopics_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && ItemsTopics.SelectedItem is TopicDisplayItem)
        {
            _acc.Audio.PlayNavigationClick();
        }
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

    private static string GetDisabilitySummary(UserProfile profile)
    {
        if (profile.Disabilities.Count > 0)
        {
            return string.Join(" + ", profile.Disabilities.Select(GetDisabilityName));
        }
        return GetDisabilityName(profile.Disability);
    }

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
