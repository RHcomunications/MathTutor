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
}

public partial class RealmMapView : UserControl
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly RewardService _rewards = new();

    public event Action<string>? StationSelected;
    public event Action? BackToMenuRequested;

    public RealmMapView()
    {
        InitializeComponent();
        Loaded += RealmMapView_Loaded;
    }

    private void RealmMapView_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        var profile = _acc.CurrentProfile;
        var stations = _rewards.GetRealmStations(profile);
        var badges = _rewards.GetAllBadges(profile);

        int unlockedCount = badges.Count(b => b.IsUnlocked);
        TxtBadgesUnlockedCount.Text = $"{unlockedCount}/{badges.Count}";

        // Estaciones del mapa
        var stationDisplayList = new List<RealmStationDisplayItem>();
        for (int i = 0; i < stations.Count; i++)
        {
            var s = stations[i];
            string status;
            Brush statusBrush;

            if (s.IsCompleted)
            {
                status = "⭐ ¡Estación Completada con Éxito! (3 Estrellas)";
                statusBrush = (Brush)FindResource("AppSuccessBrush");
            }
            else if (s.IsCurrentStation)
            {
                status = "📍 ¡Tu Próximo Desafío Recomendado!";
                statusBrush = (Brush)FindResource("AppAccentBrush");
            }
            else
            {
                status = "🔒 Sendero por Desbloquear";
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
                AccessibleActionName = $"Entrar a la estación {s.Name}. {status}"
            });
        }
        ItemsStations.ItemsSource = stationDisplayList;

        // Insignias del reino
        var badgeDisplayList = new List<BadgeDisplayItem>();
        foreach (var b in badges)
        {
            string status = b.IsUnlocked ? "✨ ¡DESBLOQUEADA!" : "🔒 Bloqueada";
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
                BadgeBorderBrush = borderBrush
            });
        }
        ItemsBadges.ItemsSource = badgeDisplayList;

        _acc.Announce($"Mapa del Reino Matemágico. Has desbloqueado {unlockedCount} de {badges.Count} trofeos. Explora el sendero de aventuras.", speak: true, playTone: true);
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
