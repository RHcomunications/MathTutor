using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WpfApp.Services;

namespace MathTutor.WpfApp.Views;

public partial class MiniGameView : UserControl
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly AdaptiveEngine _adaptive = new();

    private PedagogicalTopic? _topic;
    private int _exerciseIndex = 0;
    private Exercise? _currentExercise;
    private int _attemptsCount = 0;
    private int _hintStep = 0;
    private int _countedItemsCount = 0;
    private List<ExerciseOption> _currentOptions = new();

    public event Action? BackToLessonRequested;
    public event Action<PedagogicalTopic>? MissionCompleted;

    public MiniGameView()
    {
        InitializeComponent();
        KeyDown += MiniGameView_KeyDown;
    }

    public void StartTopicGame(PedagogicalTopic topic)
    {
        _topic = topic;
        _exerciseIndex = 0;
        LoadExerciseAtCurrentIndex();
    }

    private void LoadExerciseAtCurrentIndex()
    {
        if (_topic == null || _exerciseIndex >= _topic.Exercises.Count)
        {
            if (_topic != null) MissionCompleted?.Invoke(_topic);
            return;
        }

        _currentExercise = _topic.Exercises[_exerciseIndex];
        _attemptsCount = 0;
        _hintStep = 0;
        _countedItemsCount = 0;

        BorderSuccessBanner.Visibility = Visibility.Collapsed;
        BorderHintBanner.Visibility = Visibility.Collapsed;
        TxtHintMessage.Text = string.Empty;

        var profile = _acc.CurrentProfile;

        TxtExerciseTitle.Text = _currentExercise.Title;
        TxtProgressCounter.Text = $"Reto {_exerciseIndex + 1} de {_topic.Exercises.Count}";

        string adaptedInstruction = _adaptive.GetAdaptedInstruction(_currentExercise, profile);
        TxtInstruction.Text = adaptedInstruction;

        // Renderizar objetos manipulables
        RenderManipulableItems();

        // Renderizar opciones adaptativas
        RenderOptions();

        // Anuncio inicial accesible
        string fullSpeech = $"{_currentExercise.Title}. {adaptedInstruction}.";
        if (profile.Disability == DisabilityType.Visual && !string.IsNullOrWhiteSpace(_currentExercise.ScreenReaderDescription))
        {
            fullSpeech += $" Descripción de la escena: {_currentExercise.ScreenReaderDescription}";
        }
        fullSpeech += " Elige tu respuesta usando el teclado con los números 1, 2 o 3.";

        _acc.Announce(fullSpeech, speak: true, playTone: true);

        // Foco al contenedor para recibir atajos de teclado de inmediato
        Focus();
    }

    private void RenderManipulableItems()
    {
        PanelItemsGroupA.Children.Clear();
        PanelItemsGroupB.Children.Clear();

        if (_currentExercise == null) return;

        bool hasItemsA = _currentExercise.ItemsGroupA.Count > 0;
        bool hasItemsB = _currentExercise.ItemsGroupB.Count > 0;

        BorderManipulables.Visibility = (hasItemsA || hasItemsB) ? Visibility.Visible : Visibility.Collapsed;

        int totalItems = _currentExercise.ItemsGroupA.Count + _currentExercise.ItemsGroupB.Count;
        bool isSymbolicAdvanced = (_currentExercise.Level >= AcademicLevel.SecondaryFirstGrade ||
                                   (_currentExercise.Level == AcademicLevel.ProgrammingFoundations && _currentExercise.GameType != GamePlayType.PatternAndLoopSeed)) &&
                                  totalItems <= 2;

        TxtManipulablesPrompt.Text = isSymbolicAdvanced
            ? "Selecciona el elemento interactivo para explorar sus datos y escuchar su tono:"
            : "Toca o selecciona cada objeto para contarlo y escuchar su nota mágica:";

        if (hasItemsA)
        {
            for (int i = 0; i < _currentExercise.ItemsGroupA.Count; i++)
            {
                var item = _currentExercise.ItemsGroupA[i];
                var btn = CreateItemButton(item, i + 1, isSymbolicAdvanced);
                PanelItemsGroupA.Children.Add(btn);
            }
        }

        if (hasItemsB)
        {
            PanelGroupBSeparator.Visibility = Visibility.Visible;
            for (int i = 0; i < _currentExercise.ItemsGroupB.Count; i++)
            {
                var item = _currentExercise.ItemsGroupB[i];
                var btn = CreateItemButton(item, _currentExercise.ItemsGroupA.Count + i + 1, isSymbolicAdvanced);
                PanelItemsGroupB.Children.Add(btn);
            }
        }
        else
        {
            PanelGroupBSeparator.Visibility = Visibility.Collapsed;
        }
    }

    private Button CreateItemButton(ManipulableItem item, int displayOrder, bool isSymbolicAdvanced)
    {
        var btn = new Button
        {
            Tag = item,
            Margin = new Thickness(8),
            Padding = new Thickness(14, 10, 14, 10),
            MinHeight = 85,
            MinWidth = 95,
            Cursor = Cursors.Hand,
            Style = (Style)FindResource("AccessibleGameOptionButton")
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        var emoji = new TextBlock
        {
            Text = item.EmojiSymbol,
            FontSize = 38,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var label = new TextBlock
        {
            Text = isSymbolicAdvanced ? item.Name : $"#{displayOrder}",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)FindResource("AppTextSecondaryBrush"),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0)
        };

        stack.Children.Add(emoji);
        stack.Children.Add(label);
        btn.Content = stack;

        if (isSymbolicAdvanced)
        {
            AutomationProperties.SetName(btn, $"{item.Name}: {item.AccessibleDescription}. Selecciona para explorar.");
            AutomationProperties.SetHelpText(btn, "Presiona espacio o enter para escuchar la descripción detallada del elemento y su nota musical");
        }
        else
        {
            AutomationProperties.SetName(btn, $"{item.AccessibleDescription}. Número {displayOrder}. Toca para contar.");
            AutomationProperties.SetHelpText(btn, "Presiona espacio o enter para contar este objeto y escuchar su nota musical");
        }

        btn.Click += (s, e) =>
        {
            _countedItemsCount++;
            _acc.Audio.PlayNote(item.MusicalPitchHz, 300);

            // Resaltar visualmente que ya fue explorado/contado
            btn.Background = (Brush)FindResource("AppCardBrush");
            label.Foreground = (Brush)FindResource("AppSuccessBrush");
            label.Text = isSymbolicAdvanced ? $"✓ {item.Name}" : $"✓ #{displayOrder}";

            int totalItems = (_currentExercise?.ItemsGroupA.Count ?? 0) + (_currentExercise?.ItemsGroupB.Count ?? 0);

            string speechCount;
            if (isSymbolicAdvanced)
            {
                speechCount = $"{item.Name}: {item.AccessibleDescription}.";
            }
            else
            {
                speechCount = $"¡{displayOrder}!";
                if (_countedItemsCount == totalItems)
                {
                    speechCount += $" ¡Terminaste de contar todos! Hay {totalItems} en total. El último número es el total de la colección.";
                }
            }

            _acc.Announce(speechCount, speak: true);
        };

        return btn;
    }

    private void RenderOptions()
    {
        PanelOptions.Children.Clear();
        if (_currentExercise == null) return;

        var profile = _acc.CurrentProfile;
        _currentOptions = _adaptive.GetAdaptedOptions(_currentExercise, profile);

        for (int i = 0; i < _currentOptions.Count; i++)
        {
            var opt = _currentOptions[i];
            string shortcut = (i + 1).ToString();
            opt.AccessibleKeyShortcut = shortcut;

            var btn = new Button
            {
                Content = $"[{shortcut}]  {opt.Label}",
                Tag = opt,
                Margin = new Thickness(10),
                Padding = new Thickness(20, 14, 20, 14),
                MinHeight = 70,
                MinWidth = 160,
                Style = (Style)FindResource("AccessibleGameOptionButton")
            };

            AutomationProperties.SetName(btn, $"Opción {shortcut}: {opt.Label}");
            AutomationProperties.SetHelpText(btn, $"Presiona la tecla {shortcut} o Enter para elegir esta respuesta");

            btn.Click += OptionButton_Click;
            btn.GotFocus += (s, e) => _acc.Audio.PlayNavigationClick();

            PanelOptions.Children.Add(btn);
        }
    }

    private void OptionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ExerciseOption opt)
        {
            EvaluateAnswer(opt);
        }
    }

    private void EvaluateAnswer(ExerciseOption option)
    {
        if (_currentExercise == null) return;
        _attemptsCount++;

        var profile = _acc.CurrentProfile;

        if (option.IsCorrect)
        {
            _adaptive.RecordAttempt(profile, _currentExercise, isCorrect: true, _attemptsCount);
            _acc.Audio.PlaySuccessChord();

            BorderHintBanner.Visibility = Visibility.Collapsed;
            BorderSuccessBanner.Visibility = Visibility.Visible;

            TxtSuccessExplanation.Text = !string.IsNullOrWhiteSpace(_currentExercise.ExplanationOfSuccess)
                ? _currentExercise.ExplanationOfSuccess
                : option.SpeechFeedback;

            string announceMsg = $"¡Extraordinario! Respuesta correcta. {TxtSuccessExplanation.Text}. Presiona Enter para avanzar.";
            _acc.Announce(announceMsg, speak: true);

            BtnNextExercise.Focus();
        }
        else
        {
            _adaptive.RecordAttempt(profile, _currentExercise, isCorrect: false, _attemptsCount);
            _acc.Audio.PlayHintTone();

            // Marcar visualmente la opción incorrecta probada para evitar que el alumno repita el mismo error por accidente
            foreach (var child in PanelOptions.Children)
            {
                if (child is Button optBtn && ReferenceEquals(optBtn.Tag, option))
                {
                    string currentText = optBtn.Content?.ToString() ?? string.Empty;
                    if (!currentText.StartsWith("✗ "))
                    {
                        optBtn.Content = $"✗ {currentText}";
                    }
                    optBtn.Opacity = 0.72;
                }
            }

            BorderHintBanner.Visibility = Visibility.Visible;
            _hintStep = Math.Max(_hintStep, _attemptsCount);

            bool showDetailed = _hintStep >= 2;
            string hintLabel = showDetailed ? "Pista Paso a Paso" : "Pista Orientadora";
            string hint = showDetailed ? _currentExercise.SecondHintDetailed : _currentExercise.FirstHint;

            TxtHintMessage.Text = $"Cosmo te explica por qué no es {option.Label}:\n• {option.SpeechFeedback}\n• {hintLabel}: {hint}";

            string speechFeedback = $"{option.SpeechFeedback} {hintLabel} de Cosmo: {hint}";
            _acc.Announce(speechFeedback, speak: true);
        }
    }

    private void MiniGameView_KeyDown(object sender, KeyEventArgs e)
    {
        if (BorderSuccessBanner.Visibility == Visibility.Visible)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                e.Handled = true;
                BtnNextExercise_Click(this, new RoutedEventArgs());
                return;
            }
        }

        // F1: Repetir instrucción
        if (e.Key == Key.F1)
        {
            e.Handled = true;
            RepeatExerciseInstruction();
            return;
        }

        // Teclas numéricas 1, 2, 3, 4 (teclado regular o numpad)
        int selectedIndex = -1;
        if (e.Key == Key.D1 || e.Key == Key.NumPad1) selectedIndex = 0;
        else if (e.Key == Key.D2 || e.Key == Key.NumPad2) selectedIndex = 1;
        else if (e.Key == Key.D3 || e.Key == Key.NumPad3) selectedIndex = 2;
        else if (e.Key == Key.D4 || e.Key == Key.NumPad4) selectedIndex = 3;

        if (selectedIndex >= 0 && selectedIndex < _currentOptions.Count)
        {
            e.Handled = true;
            EvaluateAnswer(_currentOptions[selectedIndex]);
        }
    }

    public void RepeatExerciseInstruction()
    {
        if (_currentExercise == null) return;
        var profile = _acc.CurrentProfile;
        string msg = $"{_currentExercise.Title}. {_adaptive.GetAdaptedInstruction(_currentExercise, profile)}";
        if (BorderHintBanner.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(TxtHintMessage.Text))
        {
            msg += $" {TxtHintMessage.Text}";
        }
        _acc.Announce(msg, speak: true);
    }

    private void BtnRepeatInstruction_Click(object sender, RoutedEventArgs e)
    {
        RepeatExerciseInstruction();
    }

    private void BtnHint_Click(object sender, RoutedEventArgs e)
    {
        if (_currentExercise == null) return;
        _hintStep++;
        BorderHintBanner.Visibility = Visibility.Visible;

        bool showDetailed = _hintStep >= 2;
        string hintLabel = showDetailed ? "Pista 2 (Resolución Paso a Paso)" : "Pista 1 (Orientación)";
        string hint = showDetailed ? _currentExercise.SecondHintDetailed : _currentExercise.FirstHint;

        TxtHintMessage.Text = $"💡 {hintLabel} de Cosmo: {hint}";
        _acc.Audio.PlayHintTone();
        _acc.Announce($"{hintLabel} de Cosmo: {hint}", speak: true);
    }

    private void BtnNextExercise_Click(object sender, RoutedEventArgs e)
    {
        _exerciseIndex++;
        if (_topic != null && _exerciseIndex >= _topic.Exercises.Count)
        {
            _acc.Audio.PlaySuccessChord();
            _acc.Announce($"¡Misión completada con éxito! {_topic.CongratulationMessage}", speak: true);
            MissionCompleted?.Invoke(_topic);
        }
        else
        {
            LoadExerciseAtCurrentIndex();
        }
    }

    private void BtnBackToLesson_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        BackToLessonRequested?.Invoke();
    }
}
