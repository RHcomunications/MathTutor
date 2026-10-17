using System;
using System.Windows;
using System.Windows.Controls;
using MathTutor.Core.Engine;
using MathTutor.Core.Models;
using MathTutor.WpfApp.Services;

namespace MathTutor.WpfApp.Views;

public partial class LessonView : UserControl
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly AdaptiveEngine _adaptive = new();
    private PedagogicalTopic? _topic;

    public event Action<PedagogicalTopic>? StartGameRequested;
    public event Action? BackToMenuRequested;

    public LessonView()
    {
        InitializeComponent();
    }

    public void LoadTopic(PedagogicalTopic topic)
    {
        _topic = topic;
        var profile = _acc.CurrentProfile;

        TxtLessonTitle.Text = topic.Title;

        string narrative = _adaptive.GetAdaptedTopicIntro(topic, profile);
        TxtNarrativeStory.Text = narrative;

        string concept = _adaptive.GetAdaptedConceptExplanation(topic, profile);
        TxtCoreConcept.Text = concept;

        string guidedExample = BuildGuidedExampleText(topic, profile);
        TxtGuidedExample.Text = guidedExample;

        ItemsPrinciples.ItemsSource = topic.PrinciplesAddressed;

        RestoreFocusAndAnnounce();
    }

    public void RestoreFocusAndAnnounce()
    {
        if (_topic == null) return;
        var profile = _acc.CurrentProfile;
        string narrative = _adaptive.GetAdaptedTopicIntro(_topic, profile);
        string concept = _adaptive.GetAdaptedConceptExplanation(_topic, profile);

        string speechMessage = $"{_topic.Title}. Cosmo dice: {narrative}. {concept}. {TxtGuidedExample.Text} Presiona Enter para jugar el reto lúdico o F1 para escuchar de nuevo.";
        _acc.Announce(speechMessage, speak: true, playTone: true);

        BtnStartGame.Focus();
    }

    public void RepeatLessonVoice()
    {
        if (_topic == null) return;
        var profile = _acc.CurrentProfile;
        string speechMessage = $"{_topic.Title}. Cosmo dice: {_adaptive.GetAdaptedTopicIntro(_topic, profile)}. {_adaptive.GetAdaptedConceptExplanation(_topic, profile)}. {TxtGuidedExample.Text}";
        _acc.Announce(speechMessage, speak: true, playTone: false);
    }

    private string BuildGuidedExampleText(PedagogicalTopic topic, UserProfile profile)
    {
        if (topic.Exercises.Count == 0)
        {
            return "Explora los objetos interactivos y aplica el principio de la misión para encontrar la respuesta.";
        }

        var sample = topic.Exercises[0];
        string problem = _adaptive.GetAdaptedInstruction(sample, profile);
        return $"• Situación: {problem}\n" +
               $"• Paso 1 (Estrategia): {sample.FirstHint}\n" +
               $"• Paso 2 (Resolución): {sample.SecondHintDetailed}\n" +
               $"• Conclusión: {sample.ExplanationOfSuccess}";
    }

    private void BtnRepeatVoice_Click(object sender, RoutedEventArgs e)
    {
        RepeatLessonVoice();
    }

    private void BtnStartGame_Click(object sender, RoutedEventArgs e)
    {
        if (_topic != null)
        {
            _acc.Audio.PlayNavigationClick();
            StartGameRequested?.Invoke(_topic);
        }
    }

    private void BtnBackToMenu_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        BackToMenuRequested?.Invoke();
    }
}
