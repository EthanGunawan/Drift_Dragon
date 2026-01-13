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

                int maxId = 0;
                foreach (var j in _journals)
                {
                    if (j.MoodJournalID > maxId)
                        maxId = j.MoodJournalID;
                }
                _nextId = maxId + 1;
            }
            catch
            {
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
            }
        }

        public async Task AddEntryAsync(Mood mood, string reflection = "")
        {
            var entry = new MoodJournal
            {
                MoodJournalID = _nextId++,
                Date = DateTime.Now.Date,
                Mood = mood,
                Reflection = reflection
            };

            _journals.Add(entry);
            await SaveAsync();
        }

        public async Task UpdateEntryAsync(MoodJournal entry)
        {
            MoodJournal? existing = null;
            foreach (var j in _journals)
            {
                if (j.MoodJournalID == entry.MoodJournalID)
                {
                    existing = j;
                    break;
                }
            }

            if (existing == null)
                return;

            existing.Mood = entry.Mood;
            existing.Reflection = entry.Reflection;

            await SaveAsync();
        }

        public async Task DeleteEntryAsync(int moodJournalId)
        {
            MoodJournal? existing = null;
            foreach (var j in _journals)
            {
                if (j.MoodJournalID == moodJournalId)
                {
                    existing = j;
                    break;
                }
            }

            if (existing == null)
                return;

            _journals.Remove(existing);
            await SaveAsync();
        }

        public Task<List<MoodJournal>> GetRecentAsync(int count = 10)
        {
            // Sort by Date descending using simple sort
            var ordered = new List<MoodJournal>();
            foreach (var j in _journals)
            {
                ordered.Add(j);
            }

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                for (int j = i + 1; j < ordered.Count; j++)
                {
                    if (ordered[j].Date > ordered[i].Date)
                    {
                        var temp = ordered[i];
                        ordered[i] = ordered[j];
                        ordered[j] = temp;
                    }
                }
            }

            var result = new List<MoodJournal>();
            for (int i = 0; i < ordered.Count && i < count; i++)
            {
                result.Add(ordered[i]);
            }

            return Task.FromResult(result);
        }

        public Task<List<MoodJournal>> GetAllAsync()
        {
            var copy = new List<MoodJournal>();
            foreach (var j in _journals)
            {
                copy.Add(j);
            }
            return Task.FromResult(copy);
        }

        public Task<double> GetAverageMoodAsync()
        {
            if (_journals.Count == 0)
                return Task.FromResult(2.0);

            double sum = 0;
            foreach (var j in _journals)
            {
                sum += (int)j.Mood;
            }

            double avg = sum / _journals.Count;
            return Task.FromResult(avg);
        }

        public Task<int> GetCurrentStreakAsync()
        {
            if (_journals.Count == 0)
                return Task.FromResult(0);

            // Distinct dates
            var dates = new List<DateTime>();
            foreach (var j in _journals)
            {
                var d = j.Date.Date;
                bool exists = false;
                foreach (var existing in dates)
                {
                    if (existing == d)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                    dates.Add(d);
            }

            // Sort dates descending
            for (int i = 0; i < dates.Count - 1; i++)
            {
                for (int j = i + 1; j < dates.Count; j++)
                {
                    if (dates[j] > dates[i])
                    {
                        var temp = dates[i];
                        dates[i] = dates[j];
                        dates[j] = temp;
                    }
                }
            }

            int streak = 0;
            DateTime today = DateTime.Today;

            foreach (var day in dates)
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
