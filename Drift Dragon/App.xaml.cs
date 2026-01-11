using Microsoft.Maui.Controls;

namespace Drift_Dragon;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new NavigationPage(new TitlePage())
        {
            BarBackgroundColor = Colors.Transparent,
            BarTextColor = Colors.Transparent
        };
    }
}
