using Drift_Dragon.BusinessLogic;

namespace Drift_Dragon;

public partial class MoodJournalPage : ContentPage
{
    private readonly MoodJournalManager _moodManager = new();

    public MoodJournalPage()
    {
        InitializeComponent();
    
        
        MoodSlider.ValueChanged += MoodSlider_ValueChanged;
    
        MoodSlider.Value = 2; // Default "Ok"
        UpdateEmoji();
        LoadHistory();
    
        
        MoodHistoryCollectionView.ItemsSource = null;
    }


    private void MoodSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        UpdateEmoji();
    }

    private void UpdateEmoji()
    {
        int moodValue = (int)MoodSlider.Value;
        MoodEmojiLabel.Text = moodValue switch
        {
            0 => "😢",  // Terrible
            1 => "🙁",  // Bad
            2 => "😐",  // Ok
            3 => "🙂",  // Good
            4 => "😊",  // Great
            _ => "😐"
        };
    }

    private async void OnSaveMoodClicked(object sender, EventArgs e)
    {
        int moodValue = (int)MoodSlider.Value;
        Mood mood = (Mood)moodValue; // Convert slider to your enum
        
        await _moodManager.AddEntryAsync(mood, ReflectionEntry.Text ?? string.Empty);
        
        // Reset form
        MoodSlider.Value = 2;
        ReflectionEntry.Text = string.Empty;
        UpdateEmoji();
        
        LoadHistory();
        await DisplayAlert("Saved! ✨", "Your mood is logged.", "OK");
    }

    private async void LoadHistory()
    {
        var recent = await _moodManager.GetRecentAsync(10);
        MoodHistoryCollectionView.ItemsSource = recent.Select(j => new
        {
            j.Date,
            j.Reflection,
            Emoji = GetEmoji(j.Mood),
            MoodScore = $"{j.Mood}/4"
        }).ToList();
    }

    private string GetEmoji(Mood mood) => mood switch
    {
        Mood.Terrible => "😢",
        Mood.Bad => "🙁",
        Mood.Ok => "😐",
        Mood.Good => "🙂",
        Mood.Great => "😊",
        _ => "😐"
    };

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
       
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // ONLY reload if empty (first time or data cleared)
        if (MoodHistoryCollectionView.ItemsSource == null)
        {
            LoadHistory();
        }
    }

}
