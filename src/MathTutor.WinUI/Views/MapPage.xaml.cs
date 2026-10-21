using System;
using System.Collections.Generic;
using System.Linq;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.Core.Services;
using MathTutor.WinUI.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace MathTutor.WinUI.Views;

public sealed class StationDisplayItem
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string LandmarkEmoji { get; init; } = "🏰";
    public string TopicId { get; init; } = string.Empty;
    public string StationNumberPrefix { get; init; } = string.Empty;
    public string StatusText { get; init; } = string.Empty;
    public Brush StatusBrush { get; init; } = new SolidColorBrush(Microsoft.UI.Colors.Gray);
    public string AccessibleActionName { get; init; } = string.Empty;
    public string ActionButtonAccessibleName { get; init; } = string.Empty;
}

public sealed class BadgeDisplayItem
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string IconEmoji { get; init; } = "🏅";
    public string UnlockStatusText { get; init; } = string.Empty;
    public Brush UnlockStatusBrush { get; init; } = new SolidColorBrush(Microsoft.UI.Colors.Gray);
    public Brush BadgeBackgroundBrush { get; init; } = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
    public Brush BadgeBorderBrush { get; init; } = new SolidColorBrush(Microsoft.UI.Colors.Gray);
    public string AccessibleBadgeName { get; init; } = string.Empty;
}

public sealed partial class MapPage : Page
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly RewardService _rewards = new();
    private readonly CurriculumService _curriculum = new();
    private bool _isRepainting;

    public MapPage()
    {
        InitializeComponent();
        ItemsStations.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(ItemsStations_KeyDown), handledEventsToo: true);
        ItemsStations.ContainerContentChanging += ItemsStations_ContainerContentChanging;
        ItemsBadges.ContainerContentChanging += ItemsBadges_ContainerContentChanging;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _acc.ThemeChanged += OnThemeChanged;
        RefreshMap();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _acc.ThemeChanged -= OnThemeChanged;
        base.OnNavigatedFrom(e);
    }

    private void OnThemeChanged()
    {
        if (!IsLoaded) return;

        _isRepainting = true;
        try
        {
            RefreshMap();
        }
        finally
        {
            _isRepainting = false;
        }
    }

    public void RefreshMap()
    {
        int previousStationIndex = ItemsStations.SelectedIndex;
        int previousBadgeIndex = ItemsBadges.SelectedIndex;

        var profile = _acc.CurrentProfile;
        var topics = profile.Disabilities.Count > 0
            ? _curriculum.GetTopicsForAge(profile.Age, profile.Disabilities, profile.CompletedLessonIds)
            : _curriculum.GetTopicsForAge(profile.Age, profile.Disability, profile.CompletedLessonIds);
        var allowedTopicIds = topics
            .Select(t => t.Id)
            .ToHashSet();

        var stations = _rewards.GetRealmStations(profile)
            .Where(s => allowedTopicIds.Contains(s.TopicId))
            .ToList();
        var badges = _rewards.GetAllBadges(profile)
            .Where(b => string.IsNullOrEmpty(b.AssociatedTopicId) || allowedTopicIds.Contains(b.AssociatedTopicId))
            .ToList();

        int unlockedCount = badges.Count(b => b.IsUnlocked);
        TxtBadgesUnlockedCount.Text = $"{unlockedCount}/{badges.Count}";
        AutomationProperties.SetName(
            TxtBadgesUnlockedCount,
            $"{unlockedCount} de {badges.Count} insignias desbloqueadas");

        var stationDisplayList = new List<StationDisplayItem>();
        int recommendedIndex = 0;

        for (int i = 0; i < stations.Count; i++)
        {
            var s = stations[i];
            string status;
            string cleanState;
            Brush statusBrush;

            if (s.IsCompleted)
            {
                status = "⭐ ¡Estación Completada con Éxito! (3 Estrellas)";
                cleanState = "Misión completada";
                statusBrush = ResourceBrush("AppSuccessBrush");
            }
            else if (s.IsCurrentStation)
            {
                status = "📍 ¡Tu Próximo Desafío Recomendado!";
                cleanState = "Misión recomendada";
                statusBrush = ResourceBrush("AppAccentBrush");
                recommendedIndex = i;
            }
            else
            {
                status = "🔒 Sendero por Desbloquear";
                cleanState = "Misión disponible";
                statusBrush = ResourceBrush("AppTextSecondaryBrush");
            }

            stationDisplayList.Add(new StationDisplayItem
            {
                Name = s.Name,
                Description = s.Description,
                LandmarkEmoji = s.LandmarkEmoji,
                TopicId = s.TopicId,
                StationNumberPrefix = $"Paso {i + 1}:",
                StatusText = status,
                StatusBrush = statusBrush,
                AccessibleActionName = $"{cleanState}: {s.Name}",
                ActionButtonAccessibleName = $"Explorar la estación: {s.Name}"
            });
        }

        ItemsStations.ItemsSource = stationDisplayList;

        if (stationDisplayList.Count > 0)
        {
            ItemsStations.SelectedIndex = _isRepainting && previousStationIndex >= 0 && previousStationIndex < stationDisplayList.Count
                ? previousStationIndex
                : recommendedIndex;
        }

        var badgeDisplayList = new List<BadgeDisplayItem>();
        foreach (var b in badges)
        {
            string status = b.IsUnlocked ? "✨ ¡DESBLOQUEADA!" : "🔒 Bloqueada";
            string cleanBadgeState = b.IsUnlocked ? "Trofeo desbloqueado" : "Trofeo bloqueado";

            badgeDisplayList.Add(new BadgeDisplayItem
            {
                Title = b.Title,
                Description = b.Description,
                IconEmoji = b.IsUnlocked ? b.IconEmoji : "🔒",
                UnlockStatusText = status,
                UnlockStatusBrush = b.IsUnlocked ? ResourceBrush("AppSuccessBrush") : ResourceBrush("AppTextSecondaryBrush"),
                BadgeBackgroundBrush = b.IsUnlocked ? ResourceBrush("AppCardBrush") : ResourceBrush("AppSurfaceBrush"),
                BadgeBorderBrush = b.IsUnlocked ? ResourceBrush("AppAccentBrush") : ResourceBrush("AppBorderBrush"),
                AccessibleBadgeName = $"{cleanBadgeState}: {b.Title}. {b.Description}"
            });
        }

        ItemsBadges.ItemsSource = badgeDisplayList;
        if (badgeDisplayList.Count > 0)
        {
            ItemsBadges.SelectedIndex = _isRepainting && previousBadgeIndex >= 0 && previousBadgeIndex < badgeDisplayList.Count
                ? previousBadgeIndex
                : 0;
        }

        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, FocusSelectedStationItem);

        if (!_isRepainting)
        {
            _acc.Announce(
                $"Mapa del Reino Matemágico. Has desbloqueado {unlockedCount} de {badges.Count} trofeos. " +
                "Usa las flechas arriba y abajo para explorar las estaciones, presiona Enter para comenzar, o Tab para revisar tu sala de trofeos.",
                speak: true,
                playTone: true);
        }
    }

    public void FocusSelectedStationItem()
    {
        if (ItemsStations.Items.Count > 0)
        {
            if (ItemsStations.SelectedIndex < 0)
            {
                ItemsStations.SelectedIndex = 0;
            }

            ItemsStations.UpdateLayout();
            if (ItemsStations.ContainerFromIndex(ItemsStations.SelectedIndex) is ListViewItem stationItem)
            {
                stationItem.Focus(FocusState.Programmatic);
                return;
            }
        }

        BtnBackToMenu.Focus(FocusState.Programmatic);
    }

    private void ItemsStations_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.ItemContainer is ListViewItem container && args.Item is StationDisplayItem item)
        {
            AutomationProperties.SetName(container, item.AccessibleActionName);
            AutomationProperties.SetHelpText(container, "Presiona Enter o Espacio para comenzar.");
        }
    }

    private void ItemsBadges_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
    {
        if (args.ItemContainer is ListViewItem container && args.Item is BadgeDisplayItem item)
        {
            AutomationProperties.SetName(container, item.AccessibleBadgeName);
        }
    }

    private void ItemsStations_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is not (VirtualKey.Enter or VirtualKey.Space))
            return;

        if (FocusManager.GetFocusedElement(XamlRoot) is Button or ComboBox or CheckBox or TextBox)
            return;

        if (ItemsStations.SelectedItem is StationDisplayItem selected &&
            !string.IsNullOrEmpty(selected.TopicId))
        {
            e.Handled = true;
            _acc.Audio.PlayNavigationClick();
            App.Navigation.RequestTopic(selected.TopicId);
        }
    }

    private void ItemsStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && !_isRepainting && ItemsStations.SelectedItem is StationDisplayItem)
        {
            _acc.Audio.PlayNavigationClick();
        }
    }

    private void ItemsBadges_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && !_isRepainting && ItemsBadges.SelectedItem is BadgeDisplayItem)
        {
            _acc.Audio.PlayNavigationClick();
        }
    }

    private void BtnStation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string topicId && !string.IsNullOrEmpty(topicId))
        {
            _acc.Audio.PlayNavigationClick();
            App.Navigation.RequestTopic(topicId);
        }
    }

    private void BtnBackToMenu_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        App.Navigation.RequestMenu();
    }

    private static Brush ResourceBrush(string key) =>
        Application.Current.Resources.TryGetValue(key, out object value) && value is Brush brush
            ? brush
            : new SolidColorBrush(Microsoft.UI.Colors.Gray);
}

