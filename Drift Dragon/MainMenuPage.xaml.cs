using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Drift_Dragon.BusinessLogic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

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
            await LoadAndTestTips();
        }

        private async Task LoadAndTestTips()
        {
            TipLabel.Text = "Loading tips...";
            StarButton.IsEnabled = false;

            try
            {
                await Task.Delay(500);

                // Load advice from JSON
                await _adviceManager.LoadFromJsonAsync();
                _allTips = await _adviceManager.GetAllAsync();

                
                await Task.Delay(1000);

                if (_allTips.Count > 0)
                {
                    TipLabel.Text = _allTips[0].Tip; // Show first tip immediately
                    _currentTipIndex = 0;
                    StarButton.IsEnabled = true;
                    StartTipRotation();
                }
                else
                {
                    // FALLBACK: Hardcoded tips for testing
                    await LoadFallbackTips();
                }
            }
            catch (Exception ex)
            {
                TipLabel.Text = $"Error: {ex.Message}";
                await LoadFallbackTips();
            }
        }

        private async Task LoadFallbackTips()
        {

            TipLabel.Text = _allTips[0].Tip;
            StarButton.IsEnabled = true;
            StartTipRotation();
        }

        private void ShowCurrentTip()
        {
            if (_allTips.Count == 0) return;
            
            var currentTip = _allTips[_currentTipIndex];
            TipLabel.Text = currentTip.Tip;
            
            // Update star button (simplified for debug)
            StarButton.Text = _currentTipIndex % 2 == 0 ? "★" : "⭐";
        }

        private void StartTipRotation()
        {
            _tipTimer?.Stop();
            _tipTimer?.Dispose();
            
            _tipTimer = new System.Timers.Timer(3000); // Faster for testing: 3 seconds
            _tipTimer.Elapsed += (s, e) =>
            {
                _currentTipIndex = (_currentTipIndex + 1) % _allTips.Count;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ShowCurrentTip();
                });
            };
            _tipTimer.Start();
        }

        private async void OnStarClicked(object sender, EventArgs e)
        {
            if (_allTips.Count == 0) return;
    
            var currentTipId = _allTips[_currentTipIndex].adviceID;
    
            // Check actual star state using AdviceManager
            bool wasStarred = await _adviceManager.IsStarredAsync(currentTipId);
    
            // Toggle the star
            await _adviceManager.ToggleStarAsync(currentTipId);
    
            // Update button visual
            bool isNowStarred = await _adviceManager.IsStarredAsync(currentTipId);
            StarButton.Text = isNowStarred ? "★" : "⭐";
    
            // Show correct alert based on NEW state
            string message = isNowStarred ? "Tip is starred! ✨" : "Tip is unstarred.";
            await DisplayAlert("Star Status", message, "OK");
        }


        // Navigation methods
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
            // Navigate to Relaxing Listening page
            await Navigation.PushAsync(new RelaxingListeningPage());
        }
        private async void OnBackToTitleClicked(object sender, EventArgs e)
        {
            // Replace the whole navigation stack with a new TitlePage
            Application.Current.MainPage = new NavigationPage(new TitlePage());
        }



        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _tipTimer?.Stop();
            _tipTimer?.Dispose();
        }
    }
}
