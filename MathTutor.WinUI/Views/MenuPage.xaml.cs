using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace MathTutor.WinUI.Views;

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
    public string AccessibleActionName { get; set; } = string.Empty;
}

public sealed partial class MenuPage : Page
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly CurriculumService _curriculum = new();
    private readonly AdaptiveEngine _adaptive = new();

    public MenuPage()
    {
        InitializeComponent();
        ItemsTopics.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(ItemsTopics_KeyDown), handledEventsToo: true);
        ItemsTopics.ContainerContentChanging += ItemsTopics_ContainerContentChanging;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        RefreshData();
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
        AutomationProperties.SetName(
            TxtStarsCount,
            profile.StarsTotal == 1 ? "1 estrella" : $"{profile.StarsTotal} estrellas");
        AutomationProperties.SetName(
            TxtStreakCount,
            profile.CurrentStreak == 1 ? "1 día de racha" : $"{profile.CurrentStreak} días de racha");

        var allowedTopics = profile.Disabilities.Count > 0
            ? _curriculum.GetTopicsForAge(profile.Age, profile.Disabilities, profile.CompletedLessonIds)
            : _curriculum.GetTopicsForAge(profile.Age, profile.Disability, profile.CompletedLessonIds);

        var orderedTopics = allowedTopics
            .OrderByDescending(t => t.Level == profile.CurrentLevel)
            .ToList();

        var displayList = new List<TopicDisplayItem>();

        foreach (var t in orderedTopics)
        {
            string concept = _adaptive.GetAdaptedConceptExplanation(t, profile);
            if (concept.Length > 140)
            {
                concept = concept.Substring(0, 137) + "...";
            }

            string principles = "Enseña: " + string.Join(" • ", t.PrinciplesAddressed.Take(3));
            bool isCompleted = profile.CompletedLessonIds.Contains(t.Id);
            bool isCurrentLevel = t.Level == profile.CurrentLevel;

            string statusBadge = isCompleted
                ? "✅ ¡Misión Completada con Éxito! (⭐)"
                : isCurrentLevel
                    ? "🎯 Recomendada para tu Grado Actual"
                    : "🧭 Misión Disponible para Explorar";

            string btnLabel = isCompleted ? "🔄 Volver a Jugar" : "🚀 Entrar a Jugar";

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
                AccessibleButtonName = $"{cleanState}: {t.Title}",
                AccessibleActionName = isCompleted
                    ? $"Volver a jugar: {t.Title}"
                    : $"Comenzar la misión: {t.Title}"
            });
        }

        ItemsTopics.ItemsSource = displayList;

        if (displayList.Count > 0)
        {
            ItemsTopics.SelectedIndex = 0;
        }

        DispatcherQueue.TryEnqueue(FocusFirstMissionItem);

        int completedCount = allowedTopics.Count(t => profile.CompletedLessonIds.Contains(t.Id));
        _acc.Announce(
            $"Bienvenido a MathTutor, {profile.Name}. Menú de misiones para {GetAcademicLevelName(profile.CurrentLevel)}. " +
            $"Tienes {profile.StarsTotal} estrellas y {completedCount} de {allowedTopics.Count} misiones completadas. " +
            "Usa las flechas arriba y abajo para elegir misión y presiona Enter para comenzar.",
            speak: true,
            playTone: true);
    }

    public void FocusInitialElement() => FocusFirstMissionItem();

    private void FocusFirstMissionItem()
    {
        if (ItemsTopics.Items.Count > 0)
        {
            if (ItemsTopics.SelectedIndex < 0)
            {
                ItemsTopics.SelectedIndex = 0;
            }

            ItemsTopics.ScrollIntoView(ItemsTopics.SelectedItem);
            ItemsTopics.UpdateLayout();

            if (ItemsTopics.ContainerFromIndex(ItemsTopics.SelectedIndex) is ListViewItem firstItem)
            {
                firstItem.Focus(FocusState.Programmatic);
                return;
            }
        }

        BtnOpenRealmMap.Focus(FocusState.Programmatic);
    }

    private void ItemsTopics_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.ItemContainer is ListViewItem container && args.Item is TopicDisplayItem item)
        {
            AutomationProperties.SetName(container, item.AccessibleButtonName);
            AutomationProperties.SetHelpText(container, "Presiona Enter o Espacio para comenzar.");
        }
    }

    private void ItemsTopics_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter && e.Key != VirtualKey.Space)
            return;

        if (FocusManager.GetFocusedElement(XamlRoot) is Button or ComboBox or CheckBox or TextBox)
            return;

        if (ItemsTopics.SelectedItem is TopicDisplayItem selected && !string.IsNullOrEmpty(selected.TopicId))
        {
            e.Handled = true;
            _acc.Audio.PlayNavigationClick();
            App.Navigation.RequestTopic(selected.TopicId);
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
            App.Navigation.RequestTopic(topicId);
        }
    }

    private void BtnOpenRealmMap_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        App.Navigation.RequestMap();
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
