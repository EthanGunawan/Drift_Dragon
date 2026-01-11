using System;
using Drift_Dragon.BusinessLogic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Drift_Dragon;

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
            // Load all managers
            await Task.WhenAll(
                _moodManager.GetAllAsync(),
                _exerciseManager.LoadFromJsonAsync(),
                _audioManager.LoadFromJsonAsync()
            );

            // Update streak
            var streak = await _moodManager.GetCurrentStreakAsync();
            StreakLabel.Text = $"{streak} days 🔥";
            
            var journals = await _moodManager.GetAllAsync();
            if (MoodTrendGraph.Drawable is MoodGraphDrawable drawable)
            {
                drawable.UpdateData(journals.ToList());
                MoodTrendGraph.Invalidate();
            }

            // Update mood trend graph
            await UpdateMoodTrendGraph();

            // Update top breathing exercises
            await UpdateTopBreathing();

            // Update top audio (using fallback counts since RelaxingAudioManager lacks usage tracking)
            await UpdateTopAudio();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load dashboard: {ex.Message}", "OK");
        }
    }

    private async Task UpdateMoodTrendGraph()
    {
        var journals = await _moodManager.GetAllAsync();
        var weekAgo = DateTime.Today.AddDays(-6);
        var weeklyMoods = journals
            .Where(j => j.Date >= weekAgo)
            .GroupBy(j => j.Date.DayOfWeek)
            .ToDictionary(g => g.Key, g => g.Average(j => (int)j.Mood));

        // ✅ CORRECT - Calculate average first
        double avgMood = journals.Any() ? journals.Average(j => (int)j.Mood) : 0;
        MoodTrendLabel.Text = $"Weekly avg: {avgMood:F1}/4";

        MoodTrendGraph.Invalidate();
    }

    private void OnMoodGraphDraw(object sender, SKPaintSurfaceEventArgs e)
    {
        var surface = e.Surface;
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        var journals = _moodManager.GetAllAsync().Result;
        var weekAgo = DateTime.Today.AddDays(-6);
        var weeklyData = journals
            .Where(j => j.Date >= weekAgo)
            .OrderBy(j => j.Date)
            .Select(j => (float)(int)j.Mood)
            .ToList();

        if (weeklyData.Count == 0) return;

        var info = e.Info;
        var width = info.Width;
        var height = info.Height;
        var padding = 40f;

        // Scale data to fit graph
        var maxMood = 4f;
        var graphHeight = height - padding * 2;
        var graphWidth = width - padding * 2;

        // Draw grid lines
        using var gridPaint = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1, IsStroke = true };
        for (int i = 0; i <= 4; i++)
        {
            var y = padding + (graphHeight * i / 4);
            canvas.DrawLine(padding, y, width - padding, y, gridPaint);
        }

        // Draw trend line
        using var linePaint = new SKPaint 
        { 
            Color = SKColor.Parse("#4A90E2"), 
            StrokeWidth = 4, 
            IsStroke = true,
            StrokeCap = SKStrokeCap.Round
        };

        for (int i = 0; i < weeklyData.Count - 1; i++)
        {
            var x1 = padding + (graphWidth * i / (weeklyData.Count - 1));
            var y1 = height - padding - (weeklyData[i] / maxMood * graphHeight);
            var x2 = padding + (graphWidth * (i + 1) / (weeklyData.Count - 1));
            var y2 = height - padding - (weeklyData[i + 1] / maxMood * graphHeight);
            
            canvas.DrawLine(x1, y1, x2, y2, linePaint);
        }

        // Draw data points
        using var pointPaint = new SKPaint { Color = SKColor.Parse("#4A90E2"), Style = SKPaintStyle.Fill };
        foreach (var (value, index) in weeklyData.Select((v, i) => (v, i)))
        {
            var x = padding + (graphWidth * index / (weeklyData.Count - 1));
            var y = height - padding - (value / maxMood * graphHeight);
            canvas.DrawCircle(x, y, 8f, pointPaint);
        }
    }

    private async Task UpdateTopBreathing()
    {
        var topExercises = await _exerciseManager.GetTopUsedAsync(5);
        var ranked = topExercises.Select((ex, i) => new
        {
            Rank = i + 1,
            ex.Name,
            UsageCount = "Used recently"
        }).ToList();
        TopBreathingView.ItemsSource = ranked;
    }

    private async Task UpdateTopAudio()
    {
        var audios = await _audioManager.GetAllAsync();
        var ranked = audios.Take(5).Select((audio, i) => new
        {
            Rank = i + 1,
            audio.Title,
            UsageCount = "Popular"
        }).ToList();
        TopAudioView.ItemsSource = ranked;
    }
}
