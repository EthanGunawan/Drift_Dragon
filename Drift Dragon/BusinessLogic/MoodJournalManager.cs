using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Drift_Dragon.BusinessLogic
{
    public class MoodJournalManager
    {
        private static List<MoodJournal> _journals = new();
        private static int _nextId = 1;
        private const string MoodFileName = "mood_journals.json";

        public MoodJournalManager()
        {
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, MoodFileName);
                if (!File.Exists(path))
                    return;

                var json = await File.ReadAllTextAsync(path);
                var data = JsonSerializer.Deserialize<List<MoodJournal>>(json);

                if (data == null)
                    return;

                _journals = data;
                _nextId = _journals.Any() ? _journals.Max(j => j.MoodJournalID) + 1 : 1;
            }
            catch
            {
                // optional: log
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, MoodFileName);
                var json = JsonSerializer.Serialize(_journals);
                await File.WriteAllTextAsync(path, json);
            }
            catch
            {
                // optional: log
            }
        }

        public async Task AddEntryAsync(Mood mood, string reflection = "")
        {
            _journals.Add(new MoodJournal
            {
                MoodJournalID = _nextId++,
                Date = DateTime.Now.Date,
                Mood = mood,
                Reflection = reflection
            });

            await SaveAsync();
        }

        // Edit existing entry
        public async Task UpdateEntryAsync(MoodJournal entry)
        {
            var existing = _journals.FirstOrDefault(j => j.MoodJournalID == entry.MoodJournalID);
            if (existing == null)
                return;

            existing.Mood = entry.Mood;
            existing.Reflection = entry.Reflection;
            // keep Date as original

            await SaveAsync();
        }

        // NEW: delete by id
        public async Task DeleteEntryAsync(int moodJournalId)
        {
            var existing = _journals.FirstOrDefault(j => j.MoodJournalID == moodJournalId);
            if (existing == null)
                return;

            _journals.Remove(existing);
            await SaveAsync();
        }

        public Task<List<MoodJournal>> GetRecentAsync(int count = 10)
        {
            var recent = _journals
                .OrderByDescending(j => j.Date)
                .Take(count)
                .ToList();

            return Task.FromResult(recent);
        }

        public Task<List<MoodJournal>> GetAllAsync() =>
            Task.FromResult(_journals);

        public Task<double> GetAverageMoodAsync()
        {
            if (_journals.Count == 0)
                return Task.FromResult(2.0); // neutral baseline

            var avg = _journals.Average(j => (int)j.Mood);
            return Task.FromResult(avg);
        }

        public Task<int> GetCurrentStreakAsync()
        {
            if (_journals.Count == 0)
                return Task.FromResult(0);

            var days = _journals
                .Select(j => j.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var streak = 0;
            var today = DateTime.Today;

            foreach (var day in days)
            {
                if (day == today.AddDays(-streak))
                    streak++;
                else
                    break;
            }

            return Task.FromResult(streak);
        }
    }
}
