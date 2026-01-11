namespace Drift_Dragon;

public partial class TitlePage : ContentPage
{
    public TitlePage()
    {
        InitializeComponent();
    }

    private async void OnPageTapped(object sender, TappedEventArgs e)
    {
        // Simple fade effect
        await RootGrid.FadeTo(0.0, 300, Easing.CubicOut);
        // Navigate to your existing main menu
        await Navigation.PushAsync(new MainMenuPage());
        // Remove this page from the back stack
        Navigation.RemovePage(this);
    }
}