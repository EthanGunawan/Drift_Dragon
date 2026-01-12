using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Drift_Dragon.BusinessLogic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Drift_Dragon
{
    public partial class RelaxingListeningPage : ContentPage
    {
        private readonly RelaxingAudioManager _audioManager;
        private List<RelaxingAudio> _audioTracks = new();

        public RelaxingListeningPage()
        {
            InitializeComponent();
            _audioManager = new RelaxingAudioManager();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAudioTracks();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            AudioCollectionView.ItemsSource = null;
        }

        private async Task LoadAudioTracks()
        {
            try
            {
                await _audioManager.LoadFromJsonAsync();
                _audioTracks = await _audioManager.GetAllAsync();
                AudioCollectionView.ItemsSource = _audioTracks;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load audio: {ex.Message}", "OK");
            }
        }

        private async void OnPlayTrack(object sender, EventArgs e)
        {
            RelaxingAudio? track = null;

            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is RelaxingAudio audio1)
            {
                track = audio1;
            }
            else if (sender is Button button && button.BindingContext is RelaxingAudio audio2)
            {
                track = audio2;
            }

            if (track == null || string.IsNullOrWhiteSpace(track.AudioUrl))
                return;

            try
            {
                // Open YouTube URL in external browser/app
                await Browser.Default.OpenAsync(new Uri(track.AudioUrl), BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open {track.Title}: {ex.Message}", "OK");
            }
        }
    }
}
