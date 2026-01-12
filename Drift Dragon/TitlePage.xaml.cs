namespace Drift_Dragon;

public partial class TitlePage : ContentPage
{
    public TitlePage()
    {
        InitializeComponent();
    }

    private async void OnPageTapped(object sender, TappedEventArgs e)
    {
        // Navigate to MainMenuPage by route
        await Shell.Current.GoToAsync(nameof(MainMenuPage));
    }
}