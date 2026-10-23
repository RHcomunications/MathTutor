using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

public sealed class GameItemView : INotifyPropertyChanged
{
    public ManipulableItem Item { get; init; } = new();
    public string Key { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public bool IsSymbolicAdvanced { get; init; }
    public bool IsCounted { get; internal set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string EmojiSymbol => Item.EmojiSymbol;
    public string Label => IsCounted
        ? (IsSymbolicAdvanced ? $"✓ {Item.Name}" : $"✓ #{DisplayOrder}")
        : (IsSymbolicAdvanced ? Item.Name : $"#{DisplayOrder}");

    public string AccessibleName => IsCounted
        ? (IsSymbolicAdvanced
            ? $"{Item.Name}: {Item.AccessibleDescription}. Contado."
            : $"{Item.AccessibleDescription}. Número {DisplayOrder}. Contado.")
        : (IsSymbolicAdvanced
            ? $"{Item.Name}: {Item.AccessibleDescription}. Selecciona para explorar."
            : $"{Item.AccessibleDescription}. Número {DisplayOrder}. Toca para contar.");

    public string HelpText => IsCounted
        ? "Ya contaste este objeto. Usa las flechas o Tab para continuar con el siguiente."
        : (IsSymbolicAdvanced
            ? "Presiona espacio o enter para escuchar la descripción detallada del elemento y su nota musical"
            : "Presiona espacio o enter para contar este objeto y escuchar su nota musical");

    public void MarkCounted()
    {
        if (IsCounted)
            return;

        IsCounted = true;
        OnPropertyChanged(nameof(IsCounted));
        OnPropertyChanged(nameof(Label));
        OnPropertyChanged(nameof(AccessibleName));
        OnPropertyChanged(nameof(HelpText));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class GameOptionView
{
    public ExerciseOption Option { get; init; } = new();
    public string Shortcut { get; init; } = string.Empty;
    public bool IsWrong { get; init; }

    public string ButtonText => IsWrong
        ? $"✗ [{Shortcut}]  {Option.Label}"
        : $"[{Shortcut}]  {Option.Label}";

    public string AccessibleName => string.IsNullOrWhiteSpace(Option.AccessibleDescription)
        ? $"Opción {Shortcut}: {Option.Label}"
        : $"Opción {Shortcut}: {Option.AccessibleDescription}";
    public string HelpText => $"Usa las flechas arriba y abajo para recorrer las respuestas. Presiona la tecla {Shortcut} o Enter para elegir esta respuesta";
}

public sealed partial class GamePage : Page
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly AdaptiveEngine _adaptive = new();

    private PedagogicalTopic? _topic;
    private int _exerciseIndex;
    private Exercise? _currentExercise;
    private int _attemptsCount;
    private int _hintStep;
    private int _countedItemsCount;
    private List<ExerciseOption> _currentOptions = new();
    private readonly HashSet<string> _countedItemKeys = new();
    private readonly HashSet<ExerciseOption> _wrongOptions = new();
    private bool _answerLocked;
    private bool _successVisible;
    private List<GameItemView> _groupAViews = new();
    private List<GameItemView> _groupBViews = new();
    private string? _lastCueKey;
    private DateTime _lastCueTime;

    public GamePage()
    {
        InitializeComponent();
        ItemsOptions.PreviewKeyDown += ItemsOptions_PreviewKeyDown;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        _topic = e.Parameter as PedagogicalTopic;
        if (_topic is null)
        {
            App.Navigation.RequestMenu();
            return;
        }

        StartTopicGame(_topic);
    }

    public void StartTopicGame(PedagogicalTopic topic)
    {
        _topic = topic;
        _exerciseIndex = 0;
        LoadExerciseAtCurrentIndex();
    }

    private void LoadExerciseAtCurrentIndex()
    {
        if (_topic is null)
        {
            App.Navigation.RequestMenu();
            return;
        }

        if (_exerciseIndex >= _topic.Exercises.Count)
        {
            int earnedStars = _attemptsCount <= 1 ? 3 : _attemptsCount == 2 ? 2 : 1;
            _acc.Audio.PlaySuccessChord();
            App.Navigation.RequestMissionCompleted(_topic, earnedStars);
            return;
        }

        _currentExercise = _topic.Exercises[_exerciseIndex];
        _attemptsCount = 0;
        _hintStep = 0;
        _countedItemsCount = 0;
        _answerLocked = false;
        _successVisible = false;
        _countedItemKeys.Clear();
        _wrongOptions.Clear();

        BorderSuccessBanner.Visibility = Visibility.Collapsed;
        BorderHintBanner.Visibility = Visibility.Collapsed;
        TxtHintMessage.Text = string.Empty;

        var profile = _acc.CurrentProfile;

        TxtExerciseTitle.Text = _currentExercise.Title;
        TxtProgressCounter.Text = $"Reto {_exerciseIndex + 1} de {_topic.Exercises.Count}";
        BarExerciseProgress.Maximum = Math.Max(1, _topic.Exercises.Count);
        BarExerciseProgress.Value = _exerciseIndex + 1;
        AutomationProperties.SetName(BarExerciseProgress, $"Progreso: reto {_exerciseIndex + 1} de {_topic.Exercises.Count}");

        string adaptedInstruction = _adaptive.GetAdaptedInstruction(_currentExercise, profile);
        TxtInstruction.Text = adaptedInstruction;
        TxtReviewExplanation.Text = _adaptive.GetAdaptedConceptExplanation(_topic, profile);

        RenderManipulableItems();
        RenderOptions();

        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, FocusInitialElement);

        string optionsSpeech = BuildOptionsAnnouncement(_currentOptions);
        string fullSpeech = $"{_currentExercise.Title}. {adaptedInstruction}.";
        if (!string.IsNullOrEmpty(optionsSpeech))
        {
            fullSpeech += $" {optionsSpeech}";
        }

        _acc.Announce(fullSpeech, speak: true, playTone: true);
        PlayPatternSequence(startDelayMs: 900);
    }

    private bool IsPatternExercise() => _topic?.Id == "kinder_patterns_01";

    private void PlayPatternSequence(int startDelayMs)
    {
        if (!IsPatternExercise() || _currentExercise is null)
            return;

        var items = _currentExercise.ItemsGroupA
            .Concat(_currentExercise.ItemsGroupB)
            .ToList();

        if (items.Count > 0)
        {
            _acc.Audio.PlayItemSequence(items, startDelayMs);
        }
    }

    private void RenderManipulableItems()
    {
        if (_currentExercise is null) return;

        var groupA = _currentExercise.ItemsGroupA;
        var groupB = _currentExercise.ItemsGroupB;
        int totalItems = groupA.Count + groupB.Count;

        BorderManipulables.Visibility = totalItems > 0 ? Visibility.Visible : Visibility.Collapsed;

        bool isSymbolicAdvanced = (_currentExercise.Level >= AcademicLevel.SecondaryFirstGrade ||
                                   (_currentExercise.Level == AcademicLevel.ProgrammingFoundations && _currentExercise.GameType != GamePlayType.PatternAndLoopSeed)) &&
                                  totalItems <= 2;

        TxtManipulablesPrompt.Text = IsPatternExercise()
            ? "Escucha la secuencia y presiona R para repetirla. Recorre cada elemento con las flechas para oír su nota:"
            : isSymbolicAdvanced
                ? "Selecciona el elemento interactivo para explorar sus datos y escuchar su tono:"
                : "Toca o selecciona cada objeto para contarlo y escuchar su nota mágica:";

        _groupAViews = BuildItemViews(groupA, 1, "A", isSymbolicAdvanced);
        _groupBViews = BuildItemViews(groupB, groupA.Count + 1, "B", isSymbolicAdvanced);

        ItemsGroupA.ItemsSource = _groupAViews;
        ItemsGroupB.ItemsSource = _groupBViews;
        PanelGroupBSeparator.Visibility = groupB.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private List<GameItemView> BuildItemViews(
        IReadOnlyList<ManipulableItem> items,
        int startOrder,
        string groupKey,
        bool isSymbolicAdvanced)
    {
        var views = new List<GameItemView>();
        for (int i = 0; i < items.Count; i++)
        {
            string key = $"{groupKey}:{i}";
            views.Add(new GameItemView
            {
                Item = items[i],
                Key = key,
                DisplayOrder = startOrder + i,
                IsSymbolicAdvanced = isSymbolicAdvanced,
                IsCounted = _countedItemKeys.Contains(key)
            });
        }
        return views;
    }

    private void Item_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not GameItemView view || _currentExercise is null)
            return;

        if (_answerLocked || _countedItemKeys.Contains(view.Key))
            return;

        _countedItemKeys.Add(view.Key);
        _countedItemsCount++;
        view.MarkCounted();
        PlayItemCueOnce(view);

        int totalItems = _currentExercise.ItemsGroupA.Count + _currentExercise.ItemsGroupB.Count;
        string speech;
        if (view.IsSymbolicAdvanced)
        {
            speech = $"{view.Item.Name}: {view.Item.AccessibleDescription}.";
        }
        else
        {
            speech = $"¡{view.DisplayOrder}!";
            if (_countedItemsCount == totalItems)
            {
                speech += $" ¡Terminaste de contar todos! Hay {totalItems} en total. El último número es el total de la colección.";
            }
        }

        _acc.Announce(speech, speak: true);
    }

    private void Item_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: GameItemView view })
        {
            PlayItemCueOnce(view);
        }
    }

    private void PlayItemCueOnce(GameItemView view)
    {
        var now = DateTime.UtcNow;
        if (_lastCueKey == view.Key && (now - _lastCueTime).TotalMilliseconds < 250)
            return;

        _lastCueKey = view.Key;
        _lastCueTime = now;
        _acc.Audio.PlayItemCue(view.Item);
    }

    private void Item_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is not (VirtualKey.Left or VirtualKey.Right or VirtualKey.Up or VirtualKey.Down))
            return;

        if (sender is not Button { Tag: GameItemView current })
            return;

        e.Handled = true;
        var items = _groupAViews.Concat(_groupBViews).ToList();
        int currentIndex = items.FindIndex(item => ReferenceEquals(item, current));
        if (currentIndex < 0)
            return;

        int nextIndex = Math.Clamp(
            currentIndex + (e.Key is VirtualKey.Right or VirtualKey.Down ? 1 : -1),
            0,
            items.Count - 1);

        if (nextIndex != currentIndex && FocusItemAt(nextIndex))
        {
            _acc.Audio.PlayNavigationClick();
        }
    }

    private bool FocusItemAt(int index)
    {
        if (index < _groupAViews.Count)
        {
            return ItemsGroupA.GetOrCreateElement(index) is Button groupAButton &&
                   groupAButton.Focus(FocusState.Programmatic);
        }

        int groupBIndex = index - _groupAViews.Count;
        if (groupBIndex >= 0 && groupBIndex < _groupBViews.Count)
        {
            return ItemsGroupB.GetOrCreateElement(groupBIndex) is Button groupBButton &&
                   groupBButton.Focus(FocusState.Programmatic);
        }

        return false;
    }

    private void RenderOptions()
    {
        if (_currentExercise is null) return;

        var profile = _acc.CurrentProfile;
        _currentOptions = _adaptive.GetAdaptedOptions(_currentExercise, profile);

        var views = new List<GameOptionView>();
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            var option = _currentOptions[i];
            string shortcut = (i + 1).ToString();
            option.AccessibleKeyShortcut = shortcut;

            views.Add(new GameOptionView
            {
                Option = option,
                Shortcut = shortcut,
                IsWrong = _wrongOptions.Contains(option)
            });
        }

        ItemsOptions.ItemsSource = views;
    }

    private void Option_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is GameOptionView view)
        {
            EvaluateAnswer(view.Option);
        }
    }

    private void EvaluateAnswer(ExerciseOption option)
    {
        if (_currentExercise is null || _answerLocked) return;

        if (_wrongOptions.Contains(option))
        {
            _acc.AnnounceAssertive($"Ya probaste esta opción. {TxtHintMessage.Text}");
            return;
        }

        _attemptsCount++;
        var profile = _acc.CurrentProfile;

        if (option.IsCorrect)
        {
            _answerLocked = true;
            _successVisible = true;
            _adaptive.RecordAttempt(profile, _currentExercise, isCorrect: true, _attemptsCount);
            _acc.Audio.PlaySuccessChord();

            BorderHintBanner.Visibility = Visibility.Collapsed;
            BorderSuccessBanner.Visibility = Visibility.Visible;

            string adaptedSuccess = _adaptive.GetAdaptedSuccess(_currentExercise, profile);
            TxtSuccessExplanation.Text = !string.IsNullOrWhiteSpace(adaptedSuccess)
                ? adaptedSuccess
                : option.SpeechFeedback;

            BtnNextExercise.Focus(FocusState.Programmatic);
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => BorderSuccessBanner.StartBringIntoView());

            _acc.AnnounceAssertive(
                $"¡Extraordinario! Respuesta correcta. {TxtSuccessExplanation.Text}. Presiona Enter para avanzar.");
            return;
        }

        _wrongOptions.Add(option);
        _adaptive.RecordAttempt(profile, _currentExercise, isCorrect: false, _attemptsCount);
        _acc.Audio.PlayErrorChord();

        BorderHintBanner.Visibility = Visibility.Visible;
        _hintStep = Math.Max(_hintStep, _attemptsCount);

        bool showDetailed = _hintStep >= 2;
        string hintLabel = showDetailed ? "Pista Paso a Paso" : "Pista Orientadora";
        string hint = _adaptive.GetAdaptedHint(_currentExercise, profile, showDetailed);

        string feedbackMessage = $"{option.SpeechFeedback} {hintLabel} de Cosmo: {hint}";
        TxtHintMessage.Text = $"Incorrecto. Cosmo te explica por qué no es {option.Label}:\n• {option.SpeechFeedback}\n• {hintLabel}: {hint}";

        RenderOptions();
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            BorderHintBanner.StartBringIntoView();
            if (ItemsOptions.TryGetElement(0) is Button firstOption)
            {
                firstOption.Focus(FocusState.Programmatic);
            }
        });

        _acc.AnnounceAssertive($"Incorrecto. {feedbackMessage}");
    }

    private void ItemsOptions_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is not (VirtualKey.Up or VirtualKey.Down or VirtualKey.Left or VirtualKey.Right))
            return;

        e.Handled = true;
        if (_answerLocked || _successVisible || e.Key is VirtualKey.Left or VirtualKey.Right)
            return;

        var focused = FocusManager.GetFocusedElement(XamlRoot);
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            if (!ReferenceEquals(ItemsOptions.TryGetElement(i), focused))
                continue;

            int next = Math.Clamp(i + (e.Key == VirtualKey.Down ? 1 : -1), 0, _currentOptions.Count - 1);
            if (next != i && ItemsOptions.GetOrCreateElement(next) is Button option &&
                option.Focus(FocusState.Keyboard))
            {
                _acc.Audio.PlayNavigationClick();
            }
            return;
        }
    }

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.F1)
        {
            e.Handled = true;
            RepeatExerciseInstruction();
            return;
        }

        if (e.Key == VirtualKey.R && IsPatternExercise())
        {
            e.Handled = true;
            PlayPatternSequence(startDelayMs: 0);
            return;
        }

        if (e.Key == VirtualKey.Escape)
        {
            e.Handled = true;
            _acc.Audio.PlayNavigationClick();
            App.Navigation.RequestBackToLesson();
            return;
        }

        if (_successVisible)
        {
            if (e.Key is VirtualKey.Enter or VirtualKey.Space &&
                FocusManager.GetFocusedElement(XamlRoot) is not Button)
            {
                e.Handled = true;
                BtnNextExercise_Click(this, new RoutedEventArgs());
            }
            return;
        }

        int selectedIndex = e.Key switch
        {
            VirtualKey.Number1 or VirtualKey.NumberPad1 => 0,
            VirtualKey.Number2 or VirtualKey.NumberPad2 => 1,
            VirtualKey.Number3 or VirtualKey.NumberPad3 => 2,
            VirtualKey.Number4 or VirtualKey.NumberPad4 => 3,
            _ => -1
        };

        if (selectedIndex >= 0 && SelectOptionByIndex(selectedIndex))
        {
            e.Handled = true;
        }
    }

    public void FocusInitialElement()
    {
        if (ShouldStartWithManipulables() && FocusItemAt(0))
        {
            return;
        }

        if (ItemsOptions.TryGetElement(0) is Button firstOption)
        {
            firstOption.Focus(FocusState.Programmatic);
        }
        else
        {
            BtnBackToLesson.Focus(FocusState.Programmatic);
        }
    }

    private bool ShouldStartWithManipulables() =>
        _currentExercise is not null &&
        (_currentExercise.GameType == GamePlayType.InteractiveCounting || IsPatternExercise());

    public bool SelectOptionByIndex(int index)
    {
        if (_answerLocked || _successVisible)
            return false;

        if (index >= 0 && index < _currentOptions.Count)
        {
            EvaluateAnswer(_currentOptions[index]);
            return true;
        }

        return false;
    }

    public void RepeatExerciseInstruction()
    {
        if (_currentExercise is null) return;

        var profile = _acc.CurrentProfile;
        string optionsSpeech = BuildOptionsAnnouncement(_currentOptions);
        string msg = $"{_currentExercise.Title}. {_adaptive.GetAdaptedInstruction(_currentExercise, profile)}.";
        if (BorderHintBanner.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(TxtHintMessage.Text))
        {
            msg += $" Pista: {TxtHintMessage.Text}.";
        }
        if (!string.IsNullOrEmpty(optionsSpeech))
        {
            msg += $" {optionsSpeech}";
        }

        _acc.Announce(msg, speak: true);
    }

    public static string BuildOptionsAnnouncement(IReadOnlyList<ExerciseOption> options) =>
        AdaptiveEngine.BuildOptionsAnnouncement(options);

    private void BtnRepeatInstruction_Click(object sender, RoutedEventArgs e)
    {
        RepeatExerciseInstruction();
    }

    private void BtnHint_Click(object sender, RoutedEventArgs e)
    {
        if (_currentExercise is null || _answerLocked) return;

        _hintStep++;
        BorderHintBanner.Visibility = Visibility.Visible;

        var profile = _acc.CurrentProfile;
        bool showDetailed = _hintStep >= 2;
        string hintLabel = showDetailed ? "Pista 2 (Resolución Paso a Paso)" : "Pista 1 (Orientación)";
        string hint = _adaptive.GetAdaptedHint(_currentExercise, profile, showDetailed);

        TxtHintMessage.Text = $"💡 {hintLabel} de Cosmo: {hint}";
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => BorderHintBanner.StartBringIntoView());

        _acc.Audio.PlayHintTone();
        _acc.Announce($"{hintLabel} de Cosmo: {hint}", speak: true);
    }

    private void BtnNextExercise_Click(object sender, RoutedEventArgs e)
    {
        _exerciseIndex++;
        LoadExerciseAtCurrentIndex();
    }

    private void BtnBackToLesson_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        App.Navigation.RequestBackToLesson();
    }
}

