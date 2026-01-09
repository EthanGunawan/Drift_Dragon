namespace Drift_Dragon;

public partial class App : Application
{
    // In App.xaml.cs - Constructor
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell(); // Use Shell or NavigationPage wrapper
    }


   
}