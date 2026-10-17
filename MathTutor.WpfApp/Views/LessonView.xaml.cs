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

        ItemsPrinciples.ItemsSource = topic.PrinciplesAddressed;

        // Anunciar por voz la lección
        string speechMessage = $"{topic.Title}. Cosmo dice: {narrative}. {concept}. Presiona Enter para jugar el reto lúdico o F1 para escuchar de nuevo.";
        _acc.Announce(speechMessage, speak: true, playTone: true);

        // Foco inicial accesible
        BtnStartGame.Focus();
    }

    private void BtnRepeatVoice_Click(object sender, RoutedEventArgs e)
    {
        if (_topic == null) return;
        var profile = _acc.CurrentProfile;
        string speechMessage = $"{_topic.Title}. Cosmo dice: {_adaptive.GetAdaptedTopicIntro(_topic, profile)}. {_adaptive.GetAdaptedConceptExplanation(_topic, profile)}";
        _acc.Announce(speechMessage, speak: true, playTone: false);
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
