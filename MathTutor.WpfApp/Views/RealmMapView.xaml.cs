using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MathTutor.Core.Models;
using MathTutor.Core.Services;
using MathTutor.WpfApp.Services;

namespace MathTutor.WpfApp.Views;

public class RealmStationDisplayItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string LandmarkEmoji { get; set; } = "🏰";
    public string TopicId { get; set; } = string.Empty;
    public string StationNumberPrefix { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public Brush StatusBrush { get; set; } = Brushes.Gray;
    public string AccessibleActionName { get; set; } = string.Empty;
}

public class BadgeDisplayItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = "🏅";
    public string UnlockStatusText { get; set; } = string.Empty;
    public Brush UnlockStatusBrush { get; set; } = Brushes.Gray;
    public Brush BadgeBackgroundBrush { get; set; } = Brushes.Transparent;
    public Brush BadgeBorderBrush { get; set; } = Brushes.Gray;
    public string AccessibleBadgeName { get; set; } = string.Empty;
}

public partial class RealmMapView : UserControl
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly RewardService _rewards = new();
    private readonly MathTutor.Core.Engine.CurriculumService _curriculum = new();

    public event Action<string>? StationSelected;
    public event Action? BackToMenuRequested;

    public RealmMapView()
    {
        InitializeComponent();
    }

    public void RefreshMap()
    {
        var profile = _acc.CurrentProfile;
        var topics = profile.Disabilities.Count > 0
            ? _curriculum.GetTopicsForAge(profile.Age, profile.Disabilities)
            : _curriculum.GetTopicsForAge(profile.Age, profile.Disability);
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

        // Estaciones del mapa
        var stationDisplayList = new List<RealmStationDisplayItem>();
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
                statusBrush = (Brush)FindResource("AppSuccessBrush");
            }
            else if (s.IsCurrentStation)
            {
                status = "📍 ¡Tu Próximo Desafío Recomendado!";
                cleanState = "Misión recomendada";
                statusBrush = (Brush)FindResource("AppAccentBrush");
                recommendedIndex = i;
            }
            else
            {
                status = "🔒 Sendero por Desbloquear";
                cleanState = "Misión disponible";
                statusBrush = (Brush)FindResource("AppTextSecondaryBrush");
            }

            stationDisplayList.Add(new RealmStationDisplayItem
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                LandmarkEmoji = s.LandmarkEmoji,
                TopicId = s.TopicId,
                StationNumberPrefix = $"Paso {i + 1}:",
                StatusText = status,
                StatusBrush = statusBrush,
                // Tarea 3: Etiqueta accesible limpia sin emojis: "{Estado}: {NombreMision}"
                AccessibleActionName = $"{cleanState}: {s.Name}"
            });
        }
        ItemsStations.ItemsSource = stationDisplayList;

        if (stationDisplayList.Count > 0)
        {
            ItemsStations.SelectedIndex = recommendedIndex;
        }

        // Insignias del reino (Tarea 11: Etiqueta accesible limpia sin emojis para el lector de pantalla)
        var badgeDisplayList = new List<BadgeDisplayItem>();
        foreach (var b in badges)
        {
            string status = b.IsUnlocked ? "✨ ¡DESBLOQUEADA!" : "🔒 Bloqueada";
            string cleanBadgeState = b.IsUnlocked ? "Trofeo desbloqueado" : "Trofeo bloqueado";
            var statusBrush = b.IsUnlocked ? (Brush)FindResource("AppSuccessBrush") : (Brush)FindResource("AppTextSecondaryBrush");
            var cardBg = b.IsUnlocked ? (Brush)FindResource("AppCardBrush") : (Brush)FindResource("AppSurfaceBrush");
            var borderBrush = b.IsUnlocked ? (Brush)FindResource("AppAccentBrush") : (Brush)FindResource("AppBorderBrush");

            badgeDisplayList.Add(new BadgeDisplayItem
            {
                Title = b.Title,
                Description = b.Description,
                IconEmoji = b.IsUnlocked ? b.IconEmoji : "🔒",
                UnlockStatusText = status,
                UnlockStatusBrush = statusBrush,
                BadgeBackgroundBrush = cardBg,
                BadgeBorderBrush = borderBrush,
                AccessibleBadgeName = $"{cleanBadgeState}: {b.Title}. {b.Description}"
            });
        }
        ItemsBadges.ItemsSource = badgeDisplayList;
        if (badgeDisplayList.Count > 0)
        {
            ItemsBadges.SelectedIndex = 0;
        }

        // Tarea 1 y Tarea 2: Posicionar el foco en la estación recomendada al abrir el mapa
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, () =>
        {
            FocusSelectedStationItem();
        });

        _acc.Announce($"Mapa del Reino Matemágico. Has desbloqueado {unlockedCount} de {badges.Count} trofeos. Usa las flechas arriba y abajo para explorar las estaciones, presiona Enter para comenzar, o Tab para revisar tu sala de trofeos.", speak: true, playTone: true);
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
            if (ItemsStations.ItemContainerGenerator.ContainerFromIndex(ItemsStations.SelectedIndex) is ListBoxItem stationItem)
            {
                stationItem.Focus();
                System.Windows.Input.Keyboard.Focus(stationItem);
                return;
            }
        }

        BtnBackToMenu.Focus();
        System.Windows.Input.Keyboard.Focus(BtnBackToMenu);
    }

    private void ItemsStations_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Space)
        {
            if (ItemsStations.SelectedItem is RealmStationDisplayItem selected && !string.IsNullOrEmpty(selected.TopicId))
            {
                e.Handled = true;
                _acc.Audio.PlayNavigationClick();
                StationSelected?.Invoke(selected.TopicId);
            }
        }
    }

    private void ItemsStations_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && ItemsStations.SelectedItem is RealmStationDisplayItem)
        {
            _acc.Audio.PlayNavigationClick();
        }
    }

    private void ItemsBadges_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && ItemsBadges.SelectedItem is BadgeDisplayItem)
        {
            _acc.Audio.PlayNavigationClick();
        }
    }

    private void BtnStation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string topicId && !string.IsNullOrEmpty(topicId))
        {
            _acc.Audio.PlayNavigationClick();
            StationSelected?.Invoke(topicId);
        }
    }

    private void BtnBackToMenu_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        BackToMenuRequested?.Invoke();
    }
}
