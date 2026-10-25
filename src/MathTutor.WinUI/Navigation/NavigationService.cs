using System;
using MathTutor.Core.Models;
using Microsoft.UI.Xaml.Controls;

namespace MathTutor.WinUI.Navigation;

public sealed class NavigationService
{
    private readonly Frame _frame;
    private DateTime _lastNavigatedTime = DateTime.MinValue;
    private Type? _lastNavigatedPageType;
    private object? _lastNavigatedParameter;

    public NavigationService(Frame frame)
    {
        _frame = frame;
    }

    public event Action<string>? TopicSelected;
    public event Action? MapRequested;
    public event Action? MenuRequested;
    public event Action? ProfileCompleted;
    public event Action? ProfileCancelled;
    public event Action<PedagogicalTopic>? StartGameRequested;
    public event Action? BackToMenuRequested;
    public event Action? BackToLessonRequested;
    public event Action<PedagogicalTopic, int>? MissionCompleted;

    public Type? CurrentPageType => _frame.CurrentSourcePageType;

    public void Navigate(Type pageType, object? parameter = null)
    {
        var now = DateTime.UtcNow;
        if (_lastNavigatedPageType == pageType &&
            Equals(_lastNavigatedParameter, parameter) &&
            (now - _lastNavigatedTime).TotalMilliseconds < 300)
        {
            return;
        }

        _lastNavigatedPageType = pageType;
        _lastNavigatedParameter = parameter;
        _lastNavigatedTime = now;

        try
        {
            _frame.Navigate(pageType, parameter);
            _frame.BackStack.Clear();
        }
        catch
        {
            // Resguardo contra excepciones nativas de transición rápida
        }
    }

    public void RequestTopic(string topicId) => TopicSelected?.Invoke(topicId);
    public void RequestMap() => MapRequested?.Invoke();
    public void RequestMenu() => MenuRequested?.Invoke();
    public void CompleteProfile() => ProfileCompleted?.Invoke();
    public void CancelProfile() => ProfileCancelled?.Invoke();
    public void RequestStartGame(PedagogicalTopic topic) => StartGameRequested?.Invoke(topic);
    public void RequestBackToMenu() => BackToMenuRequested?.Invoke();
    public void RequestBackToLesson() => BackToLessonRequested?.Invoke();
    public void RequestMissionCompleted(PedagogicalTopic topic, int earnedStars) =>
        MissionCompleted?.Invoke(topic, earnedStars);
}
