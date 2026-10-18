using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WpfApp.Services;

namespace MathTutor.WpfApp.Views;

public partial class LessonView : UserControl
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly AdaptiveEngine _adaptive = new();
    private PedagogicalTopic? _topic;
    private int _currentStep = 1;
    private const int TotalSteps = 3;

    public event Action<PedagogicalTopic>? StartGameRequested;
    public event Action? BackToMenuRequested;

    public LessonView()
    {
        InitializeComponent();
    }

    public void LoadTopic(PedagogicalTopic topic)
    {
        _topic = topic;
        _currentStep = 1;
        var profile = _acc.CurrentProfile;

        TxtLessonTitle.Text = topic.Title;

        string narrative = _adaptive.GetAdaptedTopicIntro(topic, profile);
        TxtNarrativeStory.Text = narrative;

        string concept = _adaptive.GetAdaptedConceptExplanation(topic, profile);
        TxtCoreConcept.Text = concept;

        string guidedExample = BuildGuidedExampleText(topic, profile);
        TxtGuidedExample.Text = guidedExample;

        ItemsPrinciples.ItemsSource = topic.PrinciplesAddressed;

        UpdateStepViewAndAnnounce(playTone: true);
    }

    public void RestoreFocusAndAnnounce()
    {
        UpdateStepViewAndAnnounce(playTone: true);
    }

    public void RepeatLessonVoice()
    {
        AnnounceCurrentStep(playTone: false);
    }

    private void UpdateStepViewAndAnnounce(bool playTone)
    {
        if (_topic == null) return;

        // Mostrar únicamente la tarjeta del paso actual (Mejora 1)
        CardStep1.Visibility = (_currentStep == 1) ? Visibility.Visible : Visibility.Collapsed;
        CardStep2.Visibility = (_currentStep == 2) ? Visibility.Visible : Visibility.Collapsed;
        CardStep3.Visibility = (_currentStep == 3) ? Visibility.Visible : Visibility.Collapsed;

        // Configurar botón anterior
        BtnPreviousStep.IsEnabled = (_currentStep > 1);

        // Configurar botón siguiente / comenzar
        if (_currentStep < TotalSteps)
        {
            BtnNextOrStart.Content = "➡ Siguiente Paso (Enter)";
            AutomationProperties.SetName(BtnNextOrStart, $"Avanzar al paso {_currentStep + 1} de {TotalSteps}");
            AutomationProperties.SetHelpText(BtnNextOrStart, "Presiona Enter o Espacio para continuar.");
        }
        else
        {
            BtnNextOrStart.Content = "🎮 ¡Comenzar el Reto Lúdico! (Enter)";
            AutomationProperties.SetName(BtnNextOrStart, "Comenzar el juego y los ejercicios");
            AutomationProperties.SetHelpText(BtnNextOrStart, "Presiona Enter o Espacio para jugar.");
        }

        // Indicador de encabezado
        TxtStepIndicator.Text = _currentStep switch
        {
            1 => $"Paso 1 de {TotalSteps}: Historia de la Misión",
            2 => $"Paso 2 de {TotalSteps}: El Secreto Matemático",
            3 => $"Paso 3 de {TotalSteps}: Ejemplo Guiado y Reto",
            _ => $"Paso {_currentStep} de {TotalSteps}"
        };

        // Foco de teclado en el botón de acción principal
        BtnNextOrStart.Focus();
        Keyboard.Focus(BtnNextOrStart);

        // Anunciar por voz única y exclusivamente el paso actual
        AnnounceCurrentStep(playTone);
    }

    private void AnnounceCurrentStep(bool playTone)
    {
        if (_topic == null) return;
        var profile = _acc.CurrentProfile;

        string speechMessage = _currentStep switch
        {
            1 => $"Paso 1 de 3. Historia de la misión: {_topic.Title}. Cosmo dice: {TxtNarrativeStory.Text}. Presiona Enter o Espacio para continuar al Secreto Matemático.",
            2 => $"Paso 2 de 3. El Secreto Matemático: {TxtCoreConcept.Text}. Presiona Enter o Espacio para ver el ejemplo guiado, o Flecha Izquierda para volver a la historia.",
            3 => $"Paso 3 de 3. Ejemplo guiado con Cosmo: {TxtGuidedExample.Text}. ¡Lección completada! Presiona Enter para comenzar el reto lúdico, o Flecha Izquierda para repasar.",
            _ => string.Empty
        };

        _acc.Announce(speechMessage, speak: true, playTone: playTone);
    }

    private void AdvanceStepOrStartGame()
    {
        if (_topic == null) return;

        if (_currentStep < TotalSteps)
        {
            _currentStep++;
            _acc.Audio.PlayNavigationClick();
            UpdateStepViewAndAnnounce(playTone: false);
        }
        else
        {
            _acc.Audio.PlayNavigationClick();
            StartGameRequested?.Invoke(_topic);
        }
    }

    private void GoToPreviousStep()
    {
        if (_topic == null || _currentStep <= 1) return;

        _currentStep--;
        _acc.Audio.PlayNavigationClick();
        UpdateStepViewAndAnnounce(playTone: false);
    }

    private string BuildGuidedExampleText(PedagogicalTopic topic, UserProfile profile)
    {
        if (topic.Exercises.Count == 0)
        {
            return "Explora los objetos interactivos y aplica el principio de la misión para encontrar la respuesta.";
        }

        var sample = topic.Exercises[0];
        string problem = _adaptive.GetAdaptedInstruction(sample, profile);
        return $"Situación: {problem}. Paso 1: {sample.FirstHint}. Paso 2: {sample.SecondHintDetailed}. Conclusión: {sample.ExplanationOfSuccess}";
    }

    private void LessonView_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Space)
        {
            AdvanceStepOrStartGame();
            e.Handled = true;
        }
        else if (e.Key is Key.Left or Key.Back or Key.Up)
        {
            if (_currentStep > 1)
            {
                GoToPreviousStep();
                e.Handled = true;
            }
        }
        else if (e.Key is Key.Right or Key.Down)
        {
            if (_currentStep < TotalSteps)
            {
                AdvanceStepOrStartGame();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.F1)
        {
            RepeatLessonVoice();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            _acc.Audio.PlayNavigationClick();
            BackToMenuRequested?.Invoke();
            e.Handled = true;
        }
    }

    private void BtnPreviousStep_Click(object sender, RoutedEventArgs e)
    {
        GoToPreviousStep();
    }

    private void BtnNextOrStart_Click(object sender, RoutedEventArgs e)
    {
        AdvanceStepOrStartGame();
    }

    private void BtnRepeatVoice_Click(object sender, RoutedEventArgs e)
    {
        RepeatLessonVoice();
    }

    private void BtnBackToMenu_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        BackToMenuRequested?.Invoke();
    }
}
