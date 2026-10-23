using System;
using System.Collections.Generic;
using System.Linq;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.Core.Services;
using MathTutor.WinUI.Services;
using MathTutor.WinUI.Navigation;
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
    public string ActionHelpText { get; init; } = "Presiona Enter o Espacio para comenzar.";
}

public sealed class BadgeDisplayItem
{
    public string Id { get; init; } = string.Empty;
    public string GroupKey { get; init; } = string.Empty;
    public bool IsUnlocked { get; init; }
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
    private bool _isBuildingBadges;

    public MapPage()
    {
        InitializeComponent();
        ItemsStations.PreviewKeyDown += VerticalListNavigation.OnPreviewKeyDown;
        ItemsStations.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(ItemsStations_KeyDown), handledEventsToo: true);
        ItemsStations.ContainerContentChanging += ItemsStations_ContainerContentChanging;
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
        string? previousBadgeId = (ItemsBadges.SelectedNode?.Content as BadgeDisplayItem)?.Id;

        var profile = _acc.CurrentProfile;
        var topics = profile.Disabilities.Count > 0
            ? _curriculum.GetTopicsForAge(profile.Age, profile.Disabilities, profile.CompletedLessonIds)
            : _curriculum.GetTopicsForAge(profile.Age, profile.Disability, profile.CompletedLessonIds);
        var allowedTopicIds = topics
            .Select(t => t.Id)
            .ToHashSet();

        var stations = _rewards.GetRealmStations(profile, allowedTopicIds)
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
                cleanState = "Sendero por desbloquear";
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
                ActionButtonAccessibleName = $"Explorar la estación: {s.Name}",
                ActionHelpText = s.IsCompleted || s.IsCurrentStation
                    ? "Presiona Enter o Espacio para comenzar."
                    : "Completa la estación anterior para desbloquear este sendero."
            });
        }

        ItemsStations.ItemsSource = stationDisplayList;
        TxtEmptyStations.Visibility = stationDisplayList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

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
                Id = b.Id,
                IsUnlocked = b.IsUnlocked,
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

        var topicLevels = topics.ToDictionary(t => t.Id, t => t.Level);
        var badgeGroups = badges.GroupBy(b => topicLevels.TryGetValue(b.AssociatedTopicId, out var level)
            ? level.ToString() : "general");
        var displayById = badgeDisplayList.ToDictionary(b => b.Id);
        _isBuildingBadges = true;
        try
        {
            ItemsBadges.RootNodes.Clear();
            foreach (var group in badgeGroups)
            {
                string title = Enum.TryParse<AcademicLevel>(group.Key, out var level)
                    ? MenuPage.GetAcademicLevelName(level) : "Logros generales";
                int groupUnlocked = group.Count(b => b.IsUnlocked);
                string progress = $"Desbloqueaste {groupUnlocked} {(groupUnlocked == 1 ? "trofeo" : "trofeos")} de {group.Count()}";
                var groupNode = new TreeViewNode
                {
                    Content = new BadgeDisplayItem
                    {
                        Id = "group_" + group.Key,
                        GroupKey = group.Key,
                        Title = title,
                        UnlockStatusText = progress,
                        AccessibleBadgeName = $"{title}. {progress}",
                        BadgeBackgroundBrush = ResourceBrush("AppSurfaceBrush"),
                        BadgeBorderBrush = ResourceBrush("AppBorderBrush"),
                        UnlockStatusBrush = ResourceBrush("AppTextPrimaryBrush")
                    }
                };
                foreach (var badge in group.OrderByDescending(b => b.IsUnlocked))
                {
                    var node = new TreeViewNode { Content = displayById[badge.Id] };
                    groupNode.Children.Add(node);
                }
                groupNode.IsExpanded = profile.Preferences.BadgeGroupExpansion.TryGetValue(group.Key, out bool expanded)
                    && expanded;
                ItemsBadges.RootNodes.Add(groupNode);
                if (_isRepainting)
                {
                    var selected = groupNode.Children.FirstOrDefault(n =>
                        (n.Content as BadgeDisplayItem)?.Id == previousBadgeId);
                    if (selected is not null && groupNode.IsExpanded)
                        ItemsBadges.SelectedNode = selected;
                    else if (((BadgeDisplayItem)groupNode.Content).Id == previousBadgeId)
                        ItemsBadges.SelectedNode = groupNode;
                }
            }
        }
        finally
        {
            _isBuildingBadges = false;
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
            AutomationProperties.SetHelpText(container, item.ActionHelpText);
        }
    }

    private void ItemsBadges_Expanding(TreeView sender, TreeViewExpandingEventArgs args)
        => SaveBadgeGroupExpansion(args.Node, true);

    private void ItemsBadges_Collapsed(TreeView sender, TreeViewCollapsedEventArgs args)
        => SaveBadgeGroupExpansion(args.Node, false);

    private void SaveBadgeGroupExpansion(TreeViewNode node, bool expanded)
    {
        if (_isBuildingBadges || node.Content is not BadgeDisplayItem group || string.IsNullOrEmpty(group.GroupKey))
            return;

        _acc.CurrentProfile.Preferences.BadgeGroupExpansion[group.GroupKey] = expanded;
        _acc.SaveCurrentProfile();
    }

    private void BadgeItem_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
            return;

        var item = element.DataContext switch
        {
            TreeViewNode node => node.Content as BadgeDisplayItem,
            BadgeDisplayItem direct => direct,
            _ => null
        };
        if (item is null)
            return;

        DependencyObject? ancestor = element;
        while (ancestor is not null and not TreeViewItem)
            ancestor = VisualTreeHelper.GetParent(ancestor);

        if (ancestor is TreeViewItem container)
            AutomationProperties.SetName(container, item.AccessibleBadgeName);
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

    private void ItemsBadges_SelectionChanged(TreeView sender, TreeViewSelectionChangedEventArgs e)
    {
        if (IsLoaded && !_isRepainting && !_isBuildingBadges && ItemsBadges.SelectedNode is not null)
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

