using System;
using System.Collections.Generic;

namespace Drift_Dragon.BusinessLogic
{
    public class MoodJournalManager
    {
        private List<MoodJournal> _journals = new();
        private int _nextId = 1;

        public async Task AddEntryAsync(Mood mood, string reflection = "")
        {
            _journals.Add(new MoodJournal
            {
                MoodJournalID = _nextId++,
                Date = DateTime.Now.Date,
                Mood = mood,
                Reflection = reflection
            });
            await Task.CompletedTask;
        }

        public Task<List<MoodJournal>> GetRecentAsync(int count = 10) 
        {
            // Sort by date descending using bubble sort
            for (int i = 0; i < _journals.Count; i++)
            {
                for (int j = 0; j < _journals.Count - 1; j++)
                {
                    if (_journals[j].Date < _journals[j + 1].Date)
                    {
                        var temp = _journals[j];
                        _journals[j] = _journals[j + 1];
                        _journals[j + 1] = temp;
                    }
                }
            }

            var recent = new List<MoodJournal>();
            for (int i = 0; i < Math.Min(count, _journals.Count); i++)
            {
                recent.Add(_journals[i]);
            }
            return Task.FromResult(recent);
        }

        public Task<List<MoodJournal>> GetAllAsync() => Task.FromResult(_journals);

        public Task<double> GetAverageMoodAsync() 
        {
            if (_journals.Count == 0) return Task.FromResult(2.0);
            
            double sum = 0;
            foreach (var journal in _journals)
            {
                sum += (int)journal.Mood;
            }
            return Task.FromResult(sum / _journals.Count);
        }

        public Task<int> GetCurrentStreakAsync()
        {
            if (_journals.Count == 0) return Task.FromResult(0);
            
            var today = DateTime.Today;
            var streak = 0;
            
            for (int i = 0; i < _journals.Count; i++)
            {
                if (_journals[i].Date.Date == today.AddDays(-streak))
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }
            return Task.FromResult(streak);
        }
    }
}
