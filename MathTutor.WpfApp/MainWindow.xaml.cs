using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Threading;
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
    private readonly RealmMapView _mapView;

    private string _lastSpokenMessage = string.Empty;
    private bool _repeatToggleFlag;

    public MainWindow()
    {
        InitializeComponent();

        _profileView = new ProfileSetupView();
        _menuView = new CurriculumMenuView();
        _lessonView = new LessonView();
        _gameView = new MiniGameView();
        _mapView = new RealmMapView();

        // Conectar el canal universal de región en vivo de UI Automation (NVDA, JAWS y Narrador)
        _acc.Speech.UiAutomationLiveRegionAnnouncer = RaiseLiveRegionAnnouncement;

        // Conectar eventos de navegación
        _profileView.ProfileSavedAndReady += () =>
        {
            UpdateTopStatus();
            NavigateTo(_menuView);
            _menuView.RefreshData();
        };

        _menuView.TopicSelected += OpenTopicLesson;

        _menuView.RealmMapRequested += () =>
        {
            NavigateTo(_mapView);
            _mapView.RefreshMap();
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
            NavigateTo(_lessonView);
            _lessonView.RestoreFocusAndAnnounce();
        };

        _gameView.MissionCompleted += (topic) =>
        {
            if (!_acc.CurrentProfile.CompletedLessonIds.Contains(topic.Id))
            {
                _acc.CurrentProfile.CompletedLessonIds.Add(topic.Id);
            }
            _acc.SaveCurrentProfile();
            UpdateTopStatus();

            // Tarea 7: Diálogo modal accesible al completar una misión
            var nextTopic = _curriculum.GetNextTopic(topic.Id, _acc.CurrentProfile);
            var dialog = new MissionCompletedDialog(topic, nextTopic)
            {
                Owner = this
            };

            bool? result = dialog.ShowDialog();
            if (result == true && dialog.ContinueToNextMission && nextTopic != null)
            {
                OpenTopicLesson(nextTopic.Id);
            }
            else
            {
                NavigateTo(_mapView);
                _mapView.RefreshMap();
            }
        };

        _mapView.BackToMenuRequested += () =>
        {
            UpdateTopStatus();
            NavigateTo(_menuView);
            _menuView.RefreshData();
        };

        _mapView.StationSelected += OpenTopicLesson;

        _acc.AnnouncementMade += Acc_AnnouncementMade;
        _acc.ThemeChanged += () =>
        {
            _acc.SaveCurrentProfile();
            UpdateTopStatus();
        };
        _acc.FontSizeChanged += () =>
        {
            _acc.SaveCurrentProfile();
            UpdateTopStatus();
        };

        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        KeyDown += MainWindow_KeyDown;
    }

    private void OpenTopicLesson(string topicId)
    {
        var topic = _curriculum.GetAllTopics().Find(t => t.Id == topicId);
        if (topic == null) return;

        var dynamicExercises = _curriculum.GenerateAdaptiveExercises(topic, _acc.CurrentProfile);
        if (dynamicExercises.Count > 0)
        {
            topic.Exercises = dynamicExercises;
        }

        NavigateTo(_lessonView);
        _lessonView.LoadTopic(topic);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Inicializar el AutomationPeer de la región en vivo desde el arranque
        _ = UIElementAutomationPeer.FromElement(TxtLiveAnnouncement)
            ?? UIElementAutomationPeer.CreatePeerForElement(TxtLiveAnnouncement);

        if (_acc.LoadSavedProfile())
        {
            // Ya existía un perfil previo guardado: restaurar, entrar al menú y posicionar foco automático (Tarea 1)
            UpdateTopStatus();
            NavigateTo(_menuView);
            _menuView.RefreshData();
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                _menuView.FocusFirstMissionItem();
            });
        }
        else
        {
            // Primera vez: aplicar estado inicial y caracterizar al alumno
            _acc.ApplyProfile(_acc.CurrentProfile);
            UpdateTopStatus();
            NavigateTo(_profileView);
        }
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
    }

    private void RaiseLiveRegionAnnouncement(string message, bool immediate)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        // Si es asertivo inmediato (ej. respuesta incorrecta o atajo F2/F3/F4), se despacha en prioridad Normal.
        // Si proviene de una transición de pantalla, se despacha en ApplicationIdle para que el foco (Keyboard.Focus)
        // se procese primero en el árbol UIA y no interrumpa la lectura en NVDA/JAWS/Narrador.
        var priority = immediate ? DispatcherPriority.Normal : DispatcherPriority.ApplicationIdle;

        Dispatcher.BeginInvoke(priority, () =>
        {
            _repeatToggleFlag = !_repeatToggleFlag;
            string liveText = _repeatToggleFlag ? message : message + "\u200B";
            string previousName = AutomationProperties.GetName(TxtLiveAnnouncement);

            TxtLiveAnnouncement.Text = liveText;
            AutomationProperties.SetName(TxtLiveAnnouncement, liveText);

            var peer = UIElementAutomationPeer.FromElement(TxtLiveAnnouncement)
                       ?? UIElementAutomationPeer.CreatePeerForElement(TxtLiveAnnouncement);

            if (peer != null)
            {
                peer.RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, previousName, liveText);
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }
        });
    }

    private void UpdateTopStatus()
    {
        TxtTopStars.Text = _acc.CurrentProfile.StarsTotal.ToString();
    }

    private void RepeatActiveViewVoice()
    {
        if (ReferenceEquals(MainContentHost.Content, _lessonView))
        {
            _lessonView.RepeatLessonVoice();
        }
        else if (ReferenceEquals(MainContentHost.Content, _gameView))
        {
            _gameView.RepeatExerciseInstruction();
        }
        else
        {
            _acc.Announce(_lastSpokenMessage, speak: true);
        }
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F1:
                e.Handled = true;
                RepeatActiveViewVoice();
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

            case Key.F6:
                e.Handled = true;
                NavigateTo(_mapView);
                _mapView.RefreshMap();
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

    private void BtnNavMap_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(_mapView);
        _mapView.RefreshMap();
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
        RepeatActiveViewVoice();
    }
}