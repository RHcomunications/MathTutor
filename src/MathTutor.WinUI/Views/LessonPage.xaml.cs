using System;
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

public sealed partial class LessonPage : Page
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly AdaptiveEngine _adaptive = new();
    private PedagogicalTopic? _topic;
    private int _currentStep = 1;
    private const int TotalSteps = 3;

    public LessonPage()
    {
        InitializeComponent();
        Loaded += LessonPage_Loaded;
    }

    private void LessonPage_Loaded(object sender, RoutedEventArgs e)
    {
        BtnNextOrStart.Focus(FocusState.Programmatic);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var args = e.Parameter as LessonNavigationArgs;
        _topic = args?.Topic ?? e.Parameter as PedagogicalTopic;
        if (_topic is null)
        {
            App.Navigation.RequestMenu();
            return;
        }

        LoadTopic(_topic, args?.StartStep ?? 1);
    }

    private void LoadTopic(PedagogicalTopic topic, int startStep = 1)
    {
        _topic = topic;
        _currentStep = Math.Clamp(startStep, 1, TotalSteps);

        var profile = _acc.CurrentProfile;
        TxtLessonTitle.Text = topic.Title;
        TxtNarrativeStory.Text = _adaptive.GetAdaptedTopicIntro(topic, profile);
        TxtCoreConcept.Text = _adaptive.GetAdaptedConceptExplanation(topic, profile);
        TxtGuidedExample.Text = BuildGuidedExampleText(topic, profile);
        ItemsPrinciples.ItemsSource = topic.PrinciplesAddressed;

        UpdateStepViewAndAnnounce(playTone: true);
    }

    public void RepeatLessonVoice()
    {
        AnnounceCurrentStep(playTone: false);
    }

    private void UpdateStepViewAndAnnounce(bool playTone)
    {
        if (_topic is null) return;

        CardStep1.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
        CardStep2.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
        CardStep3.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

        BtnPreviousStep.IsEnabled = _currentStep > 1;

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

        TxtStepIndicator.Text = _currentStep switch
        {
            1 => $"Paso 1 de {TotalSteps}: Historia de la Misión",
            2 => $"Paso 2 de {TotalSteps}: El Secreto Matemático",
            3 => $"Paso 3 de {TotalSteps}: Ejemplo Guiado y Reto",
            _ => $"Paso {_currentStep} de {TotalSteps}"
        };

        AnnounceCurrentStep(playTone);
    }

    private void AnnounceCurrentStep(bool playTone)
    {
        if (_topic is null) return;

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
        if (_topic is null) return;

        if (_currentStep < TotalSteps)
        {
            _currentStep++;
            _acc.Audio.PlayNavigationClick();
            UpdateStepViewAndAnnounce(playTone: false);
        }
        else
        {
            _acc.Audio.PlayNavigationClick();
            App.Navigation.RequestStartGame(_topic);
        }
    }

    private void GoToPreviousStep()
    {
        if (_topic is null || _currentStep <= 1) return;

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

    private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.F1)
        {
            e.Handled = true;
            RepeatLessonVoice();
            return;
        }

        if (e.Key == VirtualKey.Escape)
        {
            e.Handled = true;
            _acc.Audio.PlayNavigationClick();
            App.Navigation.RequestMenu();
            return;
        }

        if (e.Key is VirtualKey.Back or VirtualKey.Up)
        {
            if (_currentStep > 1)
            {
                e.Handled = true;
                GoToPreviousStep();
            }
            return;
        }

        if (e.Key == VirtualKey.Down)
        {
            if (_currentStep < TotalSteps)
            {
                e.Handled = true;
                AdvanceStepOrStartGame();
            }
            return;
        }

        if (e.Key is VirtualKey.Enter or VirtualKey.Space)
        {
            if (FocusManager.GetFocusedElement(XamlRoot) is Button)
            {
                return;
            }

            e.Handled = true;
            AdvanceStepOrStartGame();
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
        App.Navigation.RequestMenu();
    }
}

public sealed record LessonNavigationArgs(PedagogicalTopic Topic, int StartStep = 1);
