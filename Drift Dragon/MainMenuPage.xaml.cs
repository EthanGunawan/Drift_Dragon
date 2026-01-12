using Drift_Dragon.BusinessLogic;

namespace Drift_Dragon
{
    public partial class MainMenuPage : ContentPage
    {
        private readonly AdviceManager _adviceManager;
        private List<Advice> _allTips = new();
        private int _currentTipIndex = 0;
        private System.Timers.Timer? _tipTimer;

        public MainMenuPage()
        {
            InitializeComponent();
            _adviceManager = new AdviceManager();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAndStartTips();
        }

        private async Task LoadAndStartTips()
        {
            TipLabel.Text = "Loading tips...";
            StarButton.IsEnabled = false;

            try
            {
                await _adviceManager.LoadFromJsonAsync();
                _allTips = await _adviceManager.GetAllAsync();

                if (_allTips.Count > 0)
                {
                    _currentTipIndex = 0;
                    ShowCurrentTip();
                    StarButton.IsEnabled = true;
                    StartTipRotation();
                }
                else
                {
                    TipLabel.Text = "No tips available.";
                }
            }
            catch (Exception ex)
            {
                TipLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void ShowCurrentTip()
        {
            if (_allTips.Count == 0) return;

            var currentTip = _allTips[_currentTipIndex];
            TipLabel.Text = currentTip.Tip;

            // Simple star visual (you can wire this to AdviceManager starred state later)
            StarButton.Text = "⭐";
        }

        private void StartTipRotation()
        {
            _tipTimer?.Stop();
            _tipTimer?.Dispose();

            _tipTimer = new System.Timers.Timer(8000); // 8s between tips
            _tipTimer.Elapsed += (s, e) =>
            {
                if (_allTips.Count == 0) return;

                _currentTipIndex = (_currentTipIndex + 1) % _allTips.Count;
                MainThread.BeginInvokeOnMainThread(ShowCurrentTip);
            };
            _tipTimer.Start();
        }

        private async void OnStarClicked(object sender, EventArgs e)
        {
            if (_allTips.Count == 0) return;

            var currentTipId = _allTips[_currentTipIndex].adviceID;

            // Toggle star in manager
            await _adviceManager.ToggleStarAsync(currentTipId);
            bool isStarred = await _adviceManager.IsStarredAsync(currentTipId);

            StarButton.Text = isStarred ? "★" : "⭐";
            await DisplayAlert("Star Status",
                isStarred ? "Tip is starred! ✨" : "Tip is unstarred.",
                "OK");
        }

        private async void OnBreathingClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BreathingExerciseLibraryPage());
        }

        private async void OnMoodJournalClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MoodJournalPage());
        }

        private async void OnDashboardClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProgressDashboardPage());
        }

        private async void OnListeningClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RelaxingListeningPage());
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _tipTimer?.Stop();
            _tipTimer?.Dispose();
        }
    }
}
