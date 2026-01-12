namespace Drift_Dragon;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register pages for routing
        Routing.RegisterRoute(nameof(MainMenuPage), typeof(MainMenuPage));
    }
}