using Drift_Dragon.BusinessLogic;

namespace Drift_Dragon
{
    public partial class RelaxingListeningPage : ContentPage
    {
        private readonly RelaxingAudioManager _audioManager;
        private List<RelaxingAudio> _audioTracks = new();
        private int _currentTrackIndex = -1;

        public RelaxingListeningPage()
        {
            InitializeComponent();
            _audioManager = new RelaxingAudioManager();
        }
        
        

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
    
            // CRASH FIX: Force CollectionView cleanup
            AudioCollectionView.ItemsSource = null;
            GC.Collect(); // Force garbage collection
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Rebind on return
            LoadAudioTracks();
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
            
            if (track != null)
            {
                _currentTrackIndex = _audioTracks.IndexOf(track);
                await PlayYouTubeTrack(track);
            }
        }

        private async Task PlayYouTubeTrack(RelaxingAudio track)
        {
            try
            {
                // Open YouTube URL in external browser/app
                await Browser.Default.OpenAsync(new Uri(track.AudioUrl), BrowserLaunchMode.SystemPreferred);
                
                // Update now playing
                UpdateNowPlaying(track);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open {track.Title}: {ex.Message}", "OK");
            }
        }

        private void UpdateNowPlaying(RelaxingAudio track)
        {
            NowPlayingLabel.Text = "Now Playing:";
            CurrentTrackLabel.Text = track.Title;
            PlayPauseButton.Text = "⏸️";
        }

        private async void OnPlayPause(object sender, EventArgs e)
        {
            if (_currentTrackIndex >= 0 && _currentTrackIndex < _audioTracks.Count)
            {
                var track = _audioTracks[_currentTrackIndex];
                await PlayYouTubeTrack(track); // Re-open if paused
            }
        }
        
    }
}
