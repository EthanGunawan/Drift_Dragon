using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Drift_Dragon.BusinessLogic;
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

        private List<MoodJournal> _allJournals = new();

        public ProgressDashboardPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await LoadDashboardData();
            }
            catch (Exception ex)
            {
                await DisplayAlert("OnAppearing error", ex.ToString(), "OK");
            }
        }

        private async Task LoadDashboardData()
        {
            try
            {
                // Load JSON-based data
                await _exerciseManager.LoadFromJsonAsync();
                await _audioManager.LoadFromJsonAsync();

                // Load journals once
                var journals = await _moodManager.GetAllAsync();
                if (journals == null)
                    _allJournals = new List<MoodJournal>();
                else
                    _allJournals = journals;

                // Streak (guard exceptions)
                int streak = 0;
                try
                {
                    streak = await _moodManager.GetCurrentStreakAsync();
                }
                catch
                {
                    streak = 0;
                }
                StreakLabel.Text = streak.ToString() + " days 🔥";

                // Mood label + graph
                UpdateMoodTrendLabel();
                MoodTrendGraph.InvalidateSurface();

                // Top breathing + audio
                await UpdateTopBreathing();
                await UpdateTopAudio();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Dashboard load error", ex.ToString(), "OK");
            }
        }

        private void UpdateMoodTrendLabel()
        {
            double avgMood = 0;
            if (_allJournals.Count > 0)
            {
                double sum = 0;
                foreach (var j in _allJournals)
                {
                    sum += (int)j.Mood;
                }
                avgMood = sum / _allJournals.Count;
            }

            MoodTrendLabel.Text = "Weekly avg: " + avgMood.ToString("F1") + "/4";
        }

        private List<float> GetWeeklyMoodValues()
        {
            DateTime weekAgo = DateTime.Today.AddDays(-6);

            var filtered = new List<MoodJournal>();
            foreach (var j in _allJournals)
            {
                if (j.Date >= weekAgo)
                {
                    filtered.Add(j);
                }
            }

            // sort by date ascending
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

            var result = new List<float>();
            foreach (var j in filtered)
            {
                result.Add((float)(int)j.Mood);
            }

            return result;
        }

        private void OnMoodGraphDraw(object sender, SKPaintSurfaceEventArgs e)
        {
            try
            {
                var surface = e.Surface;
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                var weeklyData = GetWeeklyMoodValues();
                if (weeklyData.Count == 0)
                    return;

                var info = e.Info;
                float width = info.Width;
                float height = info.Height;
                float padding = 40f;

                float maxMood = 4f;
                float graphHeight = height - padding * 2;
                float graphWidth = width - padding * 2;

                // grid
                using (var gridPaint = new SKPaint
                {
                    Color = SKColors.LightGray,
                    StrokeWidth = 1,
                    IsStroke = true
                })
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        float y = padding + (graphHeight * i / 4f);
                        canvas.DrawLine(padding, y, width - padding, y, gridPaint);
                    }
                }

                // line
                using (var linePaint = new SKPaint
                {
                    Color = SKColor.Parse("#4A90E2"),
                    StrokeWidth = 4,
                    StrokeCap = SKStrokeCap.Round,
                    IsStroke = true
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

                // points
                using (var pointPaint = new SKPaint
                {
                    Color = SKColor.Parse("#4A90E2"),
                    Style = SKPaintStyle.Fill
                })
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
            catch
            {
                // swallow draw errors so they don't crash the page
            }
        }

        private async Task UpdateTopBreathing()
        {
            var topExercises = await _exerciseManager.GetTopUsedAsync(5);
            if (topExercises == null)
                topExercises = new List<BreathingExercise>();

            var ranked = new List<object>();
            int index = 1;
            foreach (var ex in topExercises)
            {
                string name = ex != null ? ex.Name : "Unknown";
                ranked.Add(new
                {
                    Rank = index,
                    Name = name,
                    UsageCount = "Used recently"
                });
                index++;
            }

            TopBreathingView.ItemsSource = ranked;
        }

        private async Task UpdateTopAudio()
        {
            var audios = await _audioManager.GetAllAsync();
            if (audios == null)
                audios = new List<RelaxingAudio>();

            var ranked = new List<object>();
            int index = 1;
            for (int i = 0; i < audios.Count && i < 5; i++)
            {
                var audio = audios[i];
                string title = audio != null ? audio.Title : "Unknown";
                ranked.Add(new
                {
                    Rank = index,
                    Title = title,
                    UsageCount = "Popular"
                });
                index++;
            }

            TopAudioView.ItemsSource = ranked;
        }
    }
}
