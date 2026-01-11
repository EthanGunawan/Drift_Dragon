using System;
using System.Collections.Generic;
using System.Linq;
using Drift_Dragon.BusinessLogic;
using Microsoft.Maui.Graphics;

public class MoodGraphDrawable : IDrawable
{
    private List<MoodJournal> _journals = new();

    public void UpdateData(List<MoodJournal> journals)
    {
        _journals = journals;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        var weekAgo = DateTime.Today.AddDays(-6);
        var weeklyData = _journals
            .Where(j => j.Date >= weekAgo)
            .OrderBy(j => j.Date)
            .Select(j => (float)(int)j.Mood)
            .DefaultIfEmpty(2f)
            .ToArray();

        if (weeklyData.Length == 0) return;

        var width = dirtyRect.Width;
        var height = dirtyRect.Height;
        var padding = 40f;
        var graphHeight = height - padding * 2;
        var graphWidth = width - padding * 2;

        // Grid lines
        canvas.StrokeColor = Colors.LightGray;
        canvas.StrokeSize = 1;
        for (int i = 0; i <= 4; i++)
        {
            var y = padding + (graphHeight * i / 4);
            canvas.DrawLine(padding, y, width - padding, y);
        }

        // Trend line
        canvas.StrokeColor = Color.FromArgb("#4A90E2");
        canvas.StrokeSize = 4;
        canvas.StrokeLineCap = LineCap.Round;
        
        for (int i = 0; i < Math.Max(1, weeklyData.Length - 1); i++)
        {
            var x1 = padding + (graphWidth * i / Math.Max(1, weeklyData.Length - 1));
            var y1 = height - padding - (weeklyData[i] / 4f * graphHeight);
            var x2 = padding + (graphWidth * (i + 1) / Math.Max(1, weeklyData.Length - 1));
            var y2 = height - padding - (weeklyData[i + 1] / 4f * graphHeight);
            canvas.DrawLine(x1, y1, x2, y2);
        }

        // Data points
        canvas.FillColor = Color.FromArgb("#4A90E2");
        foreach (var (value, index) in weeklyData.Select((v, i) => (v, i)))
        {
            var x = padding + (graphWidth * index / Math.Max(1, weeklyData.Length - 1));
            var y = height - padding - (value / 4f * graphHeight);
            canvas.FillCircle(x, y, 8);
        }
    }
}
