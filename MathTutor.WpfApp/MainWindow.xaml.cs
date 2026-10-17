using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WpfApp.Services;
using MathTutor.WpfApp.Views;

namespace MathTutor.WpfApp;

public partial class MainWindow : Window
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly CurriculumService _curriculum = new();

    private readonly ProfileSetupView _profileView;
    private readonly CurriculumMenuView _menuView;
    private readonly LessonView _lessonView;
    private readonly MiniGameView _gameView;

    private string _lastSpokenMessage = string.Empty;

    public MainWindow()
    {
        InitializeComponent();

        _profileView = new ProfileSetupView();
        _menuView = new CurriculumMenuView();
        _lessonView = new LessonView();
        _gameView = new MiniGameView();

        // Conectar eventos de navegación
        _profileView.ProfileSavedAndReady += () =>
        {
            UpdateTopStatus();
            NavigateTo(_menuView);
            _menuView.RefreshData();
        };

        _menuView.TopicSelected += (topicId) =>
        {
            var topic = _curriculum.GetAllTopics().Find(t => t.Id == topicId);
            if (topic != null)
            {
                NavigateTo(_lessonView);
                _lessonView.LoadTopic(topic);
            }
        };

        _lessonView.StartGameRequested += (topic) =>
        {
            NavigateTo(_gameView);
            _gameView.StartTopicGame(topic);
        };

        _lessonView.BackToMenuRequested += () =>
        {
            UpdateTopStatus();
            NavigateTo(_menuView);
            _menuView.RefreshData();
        };

        _gameView.BackToLessonRequested += () =>
        {
            UpdateTopStatus();
            NavigateTo(_menuView);
            _menuView.RefreshData();
        };

        _gameView.MissionCompleted += (topic) =>
        {
            UpdateTopStatus();
            NavigateTo(_menuView);
            _menuView.RefreshData();
        };

        _acc.AnnouncementMade += Acc_AnnouncementMade;
        _acc.ThemeChanged += () => UpdateTopStatus();
        _acc.FontSizeChanged += () => UpdateTopStatus();

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Aplicar estado inicial de accesibilidad
        _acc.ApplyProfile(_acc.CurrentProfile);
        UpdateTopStatus();

        // Iniciar en la pantalla de caracterización del alumno
        NavigateTo(_profileView);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _acc.Speech.Dispose();
    }

    private void NavigateTo(UIElement view)
    {
        MainContentHost.Content = view;
    }

    private void Acc_AnnouncementMade(string message)
    {
        _lastSpokenMessage = message;
        TxtLiveAnnouncement.Text = message;
        System.Windows.Automation.AutomationProperties.SetName(TxtLiveAnnouncement, message);
    }

    private void UpdateTopStatus()
    {
        TxtTopStars.Text = _acc.CurrentProfile.StarsTotal.ToString();
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F1:
                e.Handled = true;
                _acc.Announce(_lastSpokenMessage, speak: true);
                break;

            case Key.F2:
                e.Handled = true;
                _acc.ToggleSpeech();
                break;

            case Key.F3:
                e.Handled = true;
                _acc.CycleTheme();
                break;

            case Key.F4:
                e.Handled = true;
                _acc.CycleFontSize();
                break;

            case Key.F5:
                e.Handled = true;
                NavigateTo(_profileView);
                break;

            case Key.Escape:
                if (MainContentHost.Content != _menuView)
                {
                    e.Handled = true;
                    UpdateTopStatus();
                    NavigateTo(_menuView);
                    _menuView.RefreshData();
                }
                break;
        }
    }

    private void BtnNavMenu_Click(object sender, RoutedEventArgs e)
    {
        UpdateTopStatus();
        NavigateTo(_menuView);
        _menuView.RefreshData();
    }

    private void BtnNavProfile_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(_profileView);
    }

    private void BtnToggleContrast_Click(object sender, RoutedEventArgs e)
    {
        _acc.CycleTheme();
    }

    private void BtnToggleFont_Click(object sender, RoutedEventArgs e)
    {
        _acc.CycleFontSize();
    }

    private void BtnToggleSpeech_Click(object sender, RoutedEventArgs e)
    {
        _acc.ToggleSpeech();
    }

    private void BtnHelpVoice_Click(object sender, RoutedEventArgs e)
    {
        _acc.Announce(_lastSpokenMessage, speak: true);
    }
}