using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Threading;
using MathTutor.Core.Models;
using MathTutor.WpfApp.Services;

namespace MathTutor.WpfApp.Views;

/// <summary>
/// Diálogo modal accesible al completar una misión (Tarea 7).
/// Permite avanzar inmediatamente a la siguiente misión con Enter o regresar al Mapa del Reino con Escape.
/// </summary>
public partial class MissionCompletedDialog : Window
{
    private readonly AccessibilityManager _acc = AccessibilityManager.Instance;
    private readonly PedagogicalTopic _completedTopic;
    private readonly PedagogicalTopic? _nextTopic;
    private string _spokenAnnouncement = string.Empty;

    public bool ContinueToNextMission { get; private set; }

    public MissionCompletedDialog(PedagogicalTopic completedTopic, PedagogicalTopic? nextTopic)
    {
        InitializeComponent();

        _completedTopic = completedTopic;
        _nextTopic = nextTopic;

        TxtCompletedHeader.Text = $"¡Misión Superada: {completedTopic.Title}!";
        TxtCongratulationMessage.Text = !string.IsNullOrWhiteSpace(completedTopic.CongratulationMessage)
            ? completedTopic.CongratulationMessage
            : "¡Excelente trabajo! Dominaste todos los principios de esta misión.";

        if (_nextTopic != null)
        {
            string nextLabel = $"Siguiente Misión: {_nextTopic.Title}";
            BtnNextMission.Content = nextLabel;
            AutomationProperties.SetName(BtnNextMission, nextLabel);
        }
        else
        {
            const string finishLabel = "Volver al Menú de Misiones";
            BtnNextMission.Content = finishLabel;
            AutomationProperties.SetName(BtnNextMission, finishLabel);
            TxtTransitionPrompt.Text = "¡Completaste todas las misiones de esta etapa! Presiona Enter o Escape para volver al Mapa del Reino.";
        }

        Loaded += MissionCompletedDialog_Loaded;
        PreviewKeyDown += MissionCompletedDialog_PreviewKeyDown;
    }

    private void MissionCompletedDialog_Loaded(object sender, RoutedEventArgs e)
    {
        // Tarea 1 y Tarea 7: Asignar el foco automáticamente al botón primario ("Siguiente Misión: [Nombre]")
        BtnNextMission.Focus();
        Keyboard.Focus(BtnNextMission);

        _spokenAnnouncement = _nextTopic != null
            ? $"¡Felicidades! Misión completada. {_completedTopic.CongratulationMessage} Siguiente misión: {_nextTopic.Title}. Presiona Enter para jugar la siguiente misión o Escape para volver al mapa."
            : $"¡Felicidades! Misión completada. {_completedTopic.CongratulationMessage} Has superado todas las misiones de tu etapa actual. Presiona Enter o Escape para volver al mapa.";

        // Anunciar después de que el foco del diálogo se estabilice en el árbol UIA
        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
        {
            AnnounceInModal(_spokenAnnouncement);
        });
    }

    private void AnnounceInModal(string message)
    {
        TxtDialogLiveRegion.Text = message;
        AutomationProperties.SetName(TxtDialogLiveRegion, message);

        var peer = UIElementAutomationPeer.FromElement(TxtDialogLiveRegion)
                   ?? UIElementAutomationPeer.CreatePeerForElement(TxtDialogLiveRegion);
        if (peer != null)
        {
            peer.RaisePropertyChangedEvent(AutomationElementIdentifiers.NameProperty, string.Empty, message);
            peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }

        _acc.AnnounceAssertive(message);
    }

    private void MissionCompletedDialog_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F1)
        {
            e.Handled = true;
            AnnounceInModal(_spokenAnnouncement);
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            ContinueToNextMission = false;
            DialogResult = false;
            Close();
        }
    }

    private void BtnNextMission_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        ContinueToNextMission = true;
        DialogResult = true;
        Close();
    }

    private void BtnBackToMap_Click(object sender, RoutedEventArgs e)
    {
        _acc.Audio.PlayNavigationClick();
        ContinueToNextMission = false;
        DialogResult = false;
        Close();
    }
}
