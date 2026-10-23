using System;
using System.Runtime.InteropServices;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WinUI.Navigation;
using MathTutor.WinUI.Services;
using MathTutor.WinUI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using Windows.UI.ViewManagement;

namespace MathTutor.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly CurriculumService _curriculum = new();

    private PedagogicalTopic? _currentTopic;
    private string _lastSpokenMessage = string.Empty;
    private bool _repeatToggleFlag;
    private bool _isClosing;
    private AccessibilitySettings? _systemAccessibility;

    public NavigationService Navigation { get; }

    public MainWindow()
    {
        InitializeComponent();

        Navigation = new NavigationService(RootFrame);
        App.Navigation = Navigation;

        _acc.Speech.UiAutomationAnnouncer = RaiseScreenReaderAnnouncement;

        Navigation.TopicSelected += OpenTopicLesson;
        Navigation.MapRequested += OpenMap;
        Navigation.MenuRequested += OpenMenu;
        Navigation.ProfileCompleted += OnProfileCompleted;
        Navigation.ProfileCancelled += OnProfileCancelled;
        Navigation.StartGameRequested += OpenGame;
        Navigation.BackToMenuRequested += OpenMenu;
        Navigation.BackToLessonRequested += BackToLesson;
        Navigation.MissionCompleted += OnMissionCompleted;

        _acc.AnnouncementMade += Acc_AnnouncementMade;
        _acc.ThemeChanged += OnThemeChanged;
        _acc.FontSizeChanged += OnFontSizeChanged;

        try
        {
            _systemAccessibility = new AccessibilitySettings();
            _systemAccessibility.HighContrastChanged += OnSystemHighContrastChanged;
        }
        catch
        {
            _systemAccessibility = null;
        }

        RootGrid.Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RootGrid.Loaded -= MainWindow_Loaded;
        ApplyInitialWindowSize();

        bool hasSavedProfile = _acc.LoadSavedProfile();
        if (!hasSavedProfile)
        {
            _acc.ApplyProfile(_acc.CurrentProfile);
        }

        UpdateTopStatus();
        ApplySystemHighContrast();

        if (hasSavedProfile)
        {
            OpenMenu();
        }
        else
        {
            OpenProfile();
        }

        // Request foreground once, after the initial layout and navigation are ready.
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_isClosing) return;
            AppWindow.MoveInZOrderAtTop();
            Activate();
            SetForegroundWindow(WinRT.Interop.WindowNative.GetWindowHandle(this));
        });
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    private void ApplyInitialWindowSize()
    {
        var scale = RootGrid.XamlRoot?.RasterizationScale ?? 1.0;
        var display = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary);
        var workArea = display?.WorkArea;

        var width = (int)Math.Round(1100 * scale);
        var height = (int)Math.Round(820 * scale);

        if (workArea is { } area)
        {
            width = Math.Min(width, area.Width);
            height = Math.Min(height, area.Height);
        }

        AppWindow.Resize(new SizeInt32(width, height));

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = Math.Min((int)Math.Round(800 * scale), width);
            presenter.PreferredMinimumHeight = Math.Min((int)Math.Round(600 * scale), height);
        }

        if (workArea is { } work)
        {
            var size = AppWindow.Size;
            var x = work.X + Math.Max(0, (work.Width - size.Width) / 2);
            var y = work.Y + Math.Max(0, (work.Height - size.Height) / 2);
            AppWindow.Move(new PointInt32(x, y));
        }
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        _isClosing = true;
        if (_systemAccessibility is not null)
        {
            _systemAccessibility.HighContrastChanged -= OnSystemHighContrastChanged;
        }
        _acc.Speech.Dispose();
    }

    private void OnSystemHighContrastChanged(AccessibilitySettings sender, object args)
    {
        DispatcherQueue.TryEnqueue(ApplySystemHighContrast);
    }

    private void ApplySystemHighContrast()
    {
        if (_systemAccessibility is null)
            return;

        bool highContrast = _systemAccessibility.HighContrast;
        string scheme = _systemAccessibility.HighContrastScheme ?? string.Empty;
        bool useDarkScheme = highContrast &&
            !scheme.Contains("White", StringComparison.OrdinalIgnoreCase) &&
            !scheme.Contains("#2", StringComparison.OrdinalIgnoreCase);

        _acc.ApplySystemHighContrast(highContrast, useDarkScheme);
    }

    private void OnProfileCompleted()
    {
        _acc.SaveCurrentProfile();
        UpdateTopStatus();
        OpenMenu();
    }

    private void OnProfileCancelled()
    {
        UpdateTopStatus();
        OpenMenu();
    }

    private void OpenProfile()
    {
        Navigation.Navigate(typeof(ProfilePage));
    }

    private void OpenMenu()
    {
        UpdateTopStatus();

        if (Navigation.CurrentPageType == typeof(MenuPage) &&
            RootFrame.Content is MenuPage menuPage)
        {
            menuPage.RefreshData();
            return;
        }

        Navigation.Navigate(typeof(MenuPage));
    }

    private void OpenMap()
    {
        Navigation.Navigate(typeof(MapPage));
    }

    private void OpenTopicLesson(string topicId)
    {
        var topic = _curriculum.GetAllTopics().Find(t => t.Id == topicId);
        if (topic is null)
            return;

        var dynamicExercises = _curriculum.GenerateAdaptiveExercises(topic, _acc.CurrentProfile);
        if (dynamicExercises.Count > 0)
        {
            topic.Exercises = dynamicExercises;
        }

        _currentTopic = topic;
        Navigation.Navigate(typeof(LessonPage), new LessonNavigationArgs(topic));
    }

    private void OpenGame(PedagogicalTopic topic)
    {
        _currentTopic = topic;
        Navigation.Navigate(typeof(GamePage), topic);
    }

    private void BackToLesson()
    {
        if (_currentTopic is null)
        {
            OpenMenu();
            return;
        }

        Navigation.Navigate(typeof(LessonPage), new LessonNavigationArgs(_currentTopic, 3));
    }

    private async void OnMissionCompleted(PedagogicalTopic topic, int earnedStars)
    {
        if (!_acc.CurrentProfile.CompletedLessonIds.Contains(topic.Id))
        {
            _acc.CurrentProfile.CompletedLessonIds.Add(topic.Id);
        }

        _acc.SaveCurrentProfile();
        UpdateTopStatus();
        _acc.Audio.PlaySuccessChord();

        var nextTopic = _curriculum.GetNextTopic(topic.Id, _acc.CurrentProfile);
        var textBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AppTextPrimaryBrush"];
        var secondaryBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AppTextSecondaryBrush"];
        var successBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AppSuccessBrush"];
        var onSuccessBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AppOnSuccessBrush"];
        var surfaceBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AppSurfaceBrush"];
        var backgroundBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AppBackgroundBrush"];
        var baseFontSize = (double)Application.Current.Resources["AppBaseFontSize"];
        var subHeaderFontSize = (double)Application.Current.Resources["AppSubHeaderFontSize"];
        var headerFontSize = (double)Application.Current.Resources["AppHeaderFontSize"];

        string congratulation = !string.IsNullOrWhiteSpace(topic.CongratulationMessage)
            ? topic.CongratulationMessage
            : "¡Excelente trabajo! Dominaste todos los principios de esta misión.";
        string transition = nextTopic is not null
            ? "Presiona Enter para jugar la siguiente misión o Escape para volver al Mapa del Reino."
            : "¡Completaste todas las misiones de esta etapa! Presiona Enter o Escape para volver al Mapa del Reino.";

        var titlePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12
        };
        titlePanel.Children.Add(new TextBlock
        {
            Text = "🏆",
            FontSize = 40,
            VerticalAlignment = VerticalAlignment.Center
        });
        titlePanel.Children.Add(new TextBlock
        {
            Text = $"¡Misión Superada: {topic.Title}!",
            Foreground = successBrush,
            FontSize = headerFontSize,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        });

        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(new TextBlock
        {
            Text = $"Has completado: {topic.Title}",
            TextWrapping = TextWrapping.Wrap,
            Foreground = successBrush,
            FontSize = subHeaderFontSize,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold
        });
        content.Children.Add(new TextBlock
        {
            Text = congratulation,
            TextWrapping = TextWrapping.Wrap,
            Foreground = textBrush,
            FontSize = baseFontSize
        });
        content.Children.Add(new TextBlock
        {
            Text = $"Ganaste {earnedStars} de 3 estrellas",
            TextWrapping = TextWrapping.Wrap,
            Foreground = successBrush,
            FontSize = subHeaderFontSize,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold
        });

        if (nextTopic is not null)
        {
            content.Children.Add(new TextBlock
            {
                Text = $"Siguiente misión: {nextTopic.Title}",
                TextWrapping = TextWrapping.Wrap,
                Foreground = textBrush,
                FontSize = baseFontSize
            });
        }

        content.Children.Add(new TextBlock
        {
            Text = transition,
            TextWrapping = TextWrapping.Wrap,
            Foreground = secondaryBrush,
            FontSize = baseFontSize,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
        });

        var card = new Border
        {
            Background = surfaceBrush,
            BorderBrush = successBrush,
            BorderThickness = new Thickness(4),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = content
        };

        string spoken = nextTopic is not null
            ? $"¡Misión Superada: {topic.Title}! Ganaste {earnedStars} de 3 estrellas. Siguiente misión: {nextTopic.Title}. Presiona Enter para jugar la siguiente misión o Escape para volver al mapa."
            : $"¡Misión Superada: {topic.Title}! Ganaste {earnedStars} de 3 estrellas. Completaste todas las misiones de esta etapa. Presiona Enter o Escape para volver al mapa.";

        void AnnounceInDialog()
        {
            _acc.AnnounceAssertive(spoken);
        }

        var dialog = new ContentDialog
        {
            XamlRoot = RootGrid.XamlRoot,
            Title = titlePanel,
            Content = card,
            Background = backgroundBrush,
            BorderBrush = successBrush,
            BorderThickness = new Thickness(4),
            PrimaryButtonText = nextTopic is not null ? $"Siguiente: {nextTopic.Title}" : "Ver mapa",
            CloseButtonText = nextTopic is not null ? "Ver mapa" : "Cerrar",
            DefaultButton = ContentDialogButton.Primary
        };
        AutomationProperties.SetName(dialog, "Diálogo de Misión Completada");

        if (Application.Current.Resources["AccessibleActionButton"] is Style baseButtonStyle)
        {
            var primaryButtonStyle = new Style(typeof(Button)) { BasedOn = baseButtonStyle };
            primaryButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, successBrush));
            primaryButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, onSuccessBrush));
            primaryButtonStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 64.0));
            primaryButtonStyle.Setters.Add(new Setter(Control.FontSizeProperty, subHeaderFontSize));
            dialog.PrimaryButtonStyle = primaryButtonStyle;

            var closeButtonStyle = new Style(typeof(Button)) { BasedOn = baseButtonStyle };
            closeButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, surfaceBrush));
            closeButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, textBrush));
            closeButtonStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 54.0));
            dialog.CloseButtonStyle = closeButtonStyle;
        }

        var repeatAccelerator = new KeyboardAccelerator { Key = Windows.System.VirtualKey.F1 };
        repeatAccelerator.Invoked += (_, args) =>
        {
            args.Handled = true;
            AnnounceInDialog();
        };
        dialog.KeyboardAccelerators.Add(repeatAccelerator);
        dialog.Opened += (_, _) => DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, AnnounceInDialog);

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && nextTopic is not null)
        {
            OpenTopicLesson(nextTopic.Id);
        }
        else
        {
            OpenMap();
        }
    }

    private void OnThemeChanged()
    {
        if (_acc.Storage.Exists())
        {
            _acc.SaveCurrentProfile();
        }
        UpdateTopStatus();
        RefreshThemeResources();
    }

    private void OnFontSizeChanged()
    {
        if (_acc.Storage.Exists())
        {
            _acc.SaveCurrentProfile();
        }
        UpdateTopStatus();
        RefreshThemeResources();
    }

    private void RefreshThemeResources()
    {
        var target = _acc.CurrentProfile.Preferences.Theme == HighContrastTheme.HighContrastDark
            ? ElementTheme.Dark
            : ElementTheme.Light;

        if (RootGrid.RequestedTheme == target)
        {
            RootGrid.RequestedTheme = target == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        }

        RootGrid.RequestedTheme = target;
    }

    private void Acc_AnnouncementMade(string message)
    {
        _lastSpokenMessage = message;
        TxtLiveAnnouncement.Text = message;
        AutomationProperties.SetName(TxtLiveAnnouncement, message);
    }

    private void RaiseScreenReaderAnnouncement(string message, bool immediate)
    {
        if (string.IsNullOrWhiteSpace(message) || _isClosing)
            return;

        var priority = immediate ? DispatcherQueuePriority.Normal : DispatcherQueuePriority.Low;

        DispatcherQueue.TryEnqueue(priority, () =>
        {
            _repeatToggleFlag = !_repeatToggleFlag;
            string liveText = _repeatToggleFlag ? message : message + "\u200B";

            TxtLiveAnnouncement.Text = liveText;
            AutomationProperties.SetName(TxtLiveAnnouncement, liveText);

            var peer = FrameworkElementAutomationPeer.FromElement(TxtLiveAnnouncement)
                       ?? FrameworkElementAutomationPeer.CreatePeerForElement(TxtLiveAnnouncement);

            peer?.RaiseNotificationEvent(
                AutomationNotificationKind.ActionCompleted,
                AutomationNotificationProcessing.ImportantMostRecent,
                message,
                "MathTutor");
        });
    }

    private void UpdateTopStatus()
    {
        int stars = _acc.CurrentProfile.StarsTotal;
        TxtTopStars.Text = stars.ToString();
        AutomationProperties.SetName(TxtTopStars, stars == 1 ? "1 estrella" : $"{stars} estrellas");
    }

    private void RepeatActiveViewVoice()
    {
        if (RootFrame.Content is LessonPage lessonPage)
        {
            lessonPage.RepeatLessonVoice();
        }
        else if (RootFrame.Content is GamePage gamePage)
        {
            gamePage.RepeatExerciseInstruction();
        }
        else
        {
            _acc.Announce(_lastSpokenMessage, speak: true);
        }
    }

    private void OnHelpAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        RepeatActiveViewVoice();
    }

    private void OnSoundsAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        _acc.ToggleSpeech();
    }

    private void OnContrastAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        _acc.CycleTheme();
    }

    private void OnFontAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        _acc.CycleFontSize();
    }

    private void OnProfileAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        OpenProfile();
    }

    private void OnMapAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        args.Handled = true;
        OpenMap();
    }

    private void OnEscapeAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (Navigation.CurrentPageType == typeof(MenuPage))
            return;

        args.Handled = true;
        OpenMenu();
    }

    private void MenuItem_GotFocus(object sender, RoutedEventArgs e)
    {
        if (ReferenceEquals(sender, e.OriginalSource))
        {
            _acc.Audio.PlayNavigationClick();
        }
    }

    private void BtnNavMenu_Click(object sender, RoutedEventArgs e) => OpenMenu();

    private void BtnNavMap_Click(object sender, RoutedEventArgs e) => OpenMap();

    private void BtnNavProfile_Click(object sender, RoutedEventArgs e) => OpenProfile();

    private void BtnToggleContrast_Click(object sender, RoutedEventArgs e) => _acc.CycleTheme();

    private void BtnToggleFont_Click(object sender, RoutedEventArgs e) => _acc.CycleFontSize();

    private void BtnToggleSpeech_Click(object sender, RoutedEventArgs e) => _acc.ToggleSpeech();

    private void BtnHelpVoice_Click(object sender, RoutedEventArgs e) => RepeatActiveViewVoice();
}
