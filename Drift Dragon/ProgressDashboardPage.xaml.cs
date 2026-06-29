using System;
using System.Collections.Generic;
using Drift_Dragon.BusinessLogic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Drift_Dragon
{
    public partial class ProgressDashboardPage : ContentPage
    {
        private readonly MoodJournalManager _moodManager = new();
        private readonly BreathingExerciseManager _exerciseManager = new();
        private readonly RelaxingAudioManager _audioManager = new();

        public ProgressDashboardPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            try
            {
                // Load main data (no Task.WhenAll to keep it simple)
                await _exerciseManager.LoadFromJsonAsync();
                await _audioManager.LoadFromJsonAsync();

                // Update streak
                int streak = await _moodManager.GetCurrentStreakAsync();
                StreakLabel.Text = streak.ToString() + " days 🔥";

                // Update mood trend label + graph
                await UpdateMoodTrendGraph();

                // Update top breathing exercises
                await UpdateTopBreathing();

                // Update top audio
                await UpdateTopAudio();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load dashboard: " + ex.Message, "OK");
            }
        }

        private async Task UpdateMoodTrendGraph()
        {
            var journals = await _moodManager.GetAllAsync();

            // Filter last 7 days (today and previous 6)
            DateTime weekAgo = DateTime.Today.AddDays(-6);

            // Calculate average mood for all journals
            double avgMood = 0;
            if (journals.Count > 0)
            {
                double sum = 0;
                foreach (var j in journals)
                {
                    sum += (int)j.Mood;
                }
                avgMood = sum / journals.Count;
            }
            MoodTrendLabel.Text = "Weekly avg: " + avgMood.ToString("F1") + "/4";

            // Just invalidate; drawing happens in OnMoodGraphDraw
            MoodTrendGraph.Invalidate();
        }

        private async Task<List<float>> GetWeeklyMoodValuesAsync()
        {
            var journals = await _moodManager.GetAllAsync();
            DateTime weekAgo = DateTime.Today.AddDays(-6);

            // Filter last 7 days and order by date (simple sort)
            var filtered = new List<MoodJournal>();
            foreach (var j in journals)
            {
                if (j.Date >= weekAgo)
                {
                    filtered.Add(j);
                }
            }

            // Sort filtered by Date ascending
            for (int i = 0; i < filtered.Count - 1; i++)
            {
                for (int j = i + 1; j < filtered.Count; j++)
                {
                    if (filtered[j].Date < filtered[i].Date)
                    {
                        var temp = filtered[i];
                        filtered[i] = filtered[j];
                        filtered[j] = temp;
                    }
                }
            }

            // Convert to list of float mood values
            var result = new List<float>();
            foreach (var j in filtered)
            {
                result.Add((float)(int)j.Mood);
            }

            return result;
        }

        private async void OnMoodGraphDraw(object sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Get weekly data (no LINQ)
            var weeklyData = await GetWeeklyMoodValuesAsync();
            if (weeklyData.Count == 0)
                return;

            var info = e.Info;
            float width = info.Width;
            float height = info.Height;
            float padding = 40f;

            float maxMood = 4f;
            float graphHeight = height - padding * 2;
            float graphWidth = width - padding * 2;

            // Grid lines
            using (var gridPaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1, IsStroke = true })
            {
                for (int i = 0; i <= 4; i++)
                {
                    float y = padding + (graphHeight * i / 4f);
                    canvas.DrawLine(padding, y, width - padding, y, gridPaint);
                }
            }

            // Trend line
            using (var linePaint = new SKPaint
            {
                Color = SKColor.Parse("#4A90E2"),
                StrokeWidth = 4,
                IsStroke = true,
                StrokeCap = SKStrokeCap.Round
            })
            {
                for (int i = 0; i < weeklyData.Count - 1; i++)
                {
                    float x1 = padding + (graphWidth * i / (weeklyData.Count - 1));
                    float y1 = height - padding - (weeklyData[i] / maxMood * graphHeight);
                    float x2 = padding + (graphWidth * (i + 1) / (weeklyData.Count - 1));
                    float y2 = height - padding - (weeklyData[i + 1] / maxMood * graphHeight);

                    canvas.DrawLine(x1, y1, x2, y2, linePaint);
                }
            }

            // Data points
            using (var pointPaint = new SKPaint { Color = SKColor.Parse("#4A90E2"), Style = SKPaintStyle.Fill })
            {
                for (int i = 0; i < weeklyData.Count; i++)
                {
                    float value = weeklyData[i];
                    float x = padding + (graphWidth * i / (weeklyData.Count - 1));
                    float y = height - padding - (value / maxMood * graphHeight);
                    canvas.DrawCircle(x, y, 8f, pointPaint);
                }
            }
        }

        private async Task UpdateTopBreathing()
        {
            var topExercises = await _exerciseManager.GetTopUsedAsync(5);

           
            var ranked = new List<object>();
            int index = 1;
            foreach (var ex in topExercises)
            {
                ranked.Add(new
                {
                    Rank = index,
                    Name = ex.Name,
                    UsageCount = "Used recently"
                });
                index++;
            }

            TopBreathingView.ItemsSource = ranked;
        }

        private async Task UpdateTopAudio()
        {
            var audios = await _audioManager.GetAllAsync();

            // Take first 5 items manually
            var ranked = new List<object>();
            int index = 1;
            for (int i = 0; i < audios.Count && i < 5; i++)
            {
                var audio = audios[i];
                ranked.Add(new
                {
                    Rank = index,
                    Title = audio.Title,
                    UsageCount = "Popular"
                });
                index++;
            }

            TopAudioView.ItemsSource = ranked;
        }
    }
}
