using Microsoft.UI.Xaml;
using MathTutor.WinUI.Navigation;

namespace MathTutor.WinUI;

public partial class App : Application
{
    public static NavigationService Navigation { get; internal set; } = null!;
    public static Window? MainAppWindow { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        MainAppWindow = window;
        window.Activate();
    }
}
