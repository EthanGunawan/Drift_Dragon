using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Drift_Dragon.BusinessLogic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Drift_Dragon
{
    public partial class BreathingExerciseLibraryPage : ContentPage
    {
        private readonly BreathingExerciseManager _exerciseManager;
        private List<BreathingExercise> _exercises = new();

        public BreathingExerciseLibraryPage()
        {
            InitializeComponent();
            _exerciseManager = new BreathingExerciseManager();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadExercises();
        }

        private async Task LoadExercises()
        {
            try
            {
                PositionLabel.Text = "Loading...";
                
                await _exerciseManager.LoadFromJsonAsync();
                _exercises = await _exerciseManager.GetAllAsync();
                
                if (_exercises.Count == 0)
                {
                    PositionLabel.Text = "No exercises found";
                    return;
                }
                
                ExercisesCarousel.ItemsSource = _exercises;
                ExercisesCarousel.PositionChanged += ExercisesCarousel_PositionChanged;
                UpdatePositionLabel();
            }
            catch (Exception ex)
            {
                PositionLabel.Text = $"Load failed: {ex.Message}";
            }
        }

        private void ExercisesCarousel_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            UpdatePositionLabel();
        }

        private void UpdatePositionLabel()
        {
            var position = ExercisesCarousel.Position + 1;
            PositionLabel.Text = $"{position} of {_exercises.Count}";
        }

        private async void OnStartExercise(object sender, EventArgs e)
        {
            var exercise = _exercises[ExercisesCarousel.Position];
    
            // Track usage
            await _exerciseManager.IncrementUsageAsync(exercise.BreathingExerciseID);
    
            // 1. Vibrate phone (grab attention!)
            if (Vibration.Default.IsSupported)
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
    
            // 2. Screen flashes breathing colors (immersive!)
            await AnimateBreathingColors();
    
            // 3. Success confetti effect
            await DisplayAlert("🌟 BREATHING COMPLETE!", 
                $"You just mastered:\n🫁 {exercise.Name}", "Next Exercise!");
        }

        private async Task AnimateBreathingColors()
        {
            var originalBackground = this.BackgroundColor;
    
            // Blue → Calm blue → White flash → Success green (SLOWER)
            var colors = new Color[]
            {
                Color.FromArgb("#4A90E2"),  // Blue inhale (2.5s)
                Color.FromArgb("#3498DB"),  // Lighter blue hold (2.5s) 
                Colors.White,               // Bright flash exhale (2s)
                Color.FromArgb("#27AE60")   // Green success (1.5s)
            };
    
            var delays = new[] { 2500, 2500, 2000, 1500 }; // Much slower breathing pace
    
            for (int i = 0; i < colors.Length; i++)
            {
                this.BackgroundColor = colors[i];
                await Task.Delay(delays[i]);
            }
    
            // Reset to original
            this.BackgroundColor = originalBackground;
        }



        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (ExercisesCarousel != null)
                ExercisesCarousel.PositionChanged -= ExercisesCarousel_PositionChanged;
        }
    }
}
