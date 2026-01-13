using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Drift_Dragon.BusinessLogic
{
    public class BreathingExerciseManager
    {
        private readonly List<BreathingExercise> _exercises = new();
        private readonly Dictionary<int, int> _usageCounts = new();

        private const string UsageFileName = "breathing_usage.json";

        public BreathingExerciseManager()
        {
            _ = LoadUsageAsync();
        }

        public async Task LoadFromJsonAsync()
        {
            var data = await JsonDataService.LoadJsonAsync<List<BreathingExercise>>("breathingexercise.json");
            if (data == null)
                return;

            _exercises.Clear();
            foreach (var e in data)
            {
                _exercises.Add(e);
            }
        }

        private async Task LoadUsageAsync()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, UsageFileName);
                if (!File.Exists(path))
                    return;

                var json = await File.ReadAllTextAsync(path);
                var data = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
                if (data == null)
                    return;

                _usageCounts.Clear();
                foreach (var kvp in data)
                {
                    _usageCounts[kvp.Key] = kvp.Value;
                }
            }
            catch
            {
            }
        }

        private async Task SaveUsageAsync()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, UsageFileName);
                var json = JsonSerializer.Serialize(_usageCounts);
                await File.WriteAllTextAsync(path, json);
            }
            catch
            {
            }
        }

        public Task<List<BreathingExercise>> GetAllAsync()
        {
            var copy = new List<BreathingExercise>();
            foreach (var e in _exercises)
            {
                copy.Add(e);
            }
            return Task.FromResult(copy);
        }

        public Task<BreathingExercise?> GetByIdAsync(int id)
        {
            BreathingExercise? found = null;
            foreach (var e in _exercises)
            {
                if (e.BreathingExerciseID == id)
                {
                    found = e;
                    break;
                }
            }
            return Task.FromResult(found);
        }

        public async Task IncrementUsageAsync(int exerciseId)
        {
            if (_usageCounts.ContainsKey(exerciseId))
            {
                _usageCounts[exerciseId] = _usageCounts[exerciseId] + 1;
            }
            else
            {
                _usageCounts[exerciseId] = 1;
            }

            await SaveUsageAsync();
        }

        public int GetUsage(int exerciseId)
        {
            if (_usageCounts.ContainsKey(exerciseId))
                return _usageCounts[exerciseId];

            return 0;
        }

        public Task<List<BreathingExercise>> GetTopUsedAsync(int count = 5)
        {
            // Simple bubble-sort style ordering by usage (no LINQ)
            var ordered = new List<BreathingExercise>();
            foreach (var e in _exercises)
            {
                ordered.Add(e);
            }

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                for (int j = i + 1; j < ordered.Count; j++)
                {
                    int usageI = GetUsage(ordered[i].BreathingExerciseID);
                    int usageJ = GetUsage(ordered[j].BreathingExerciseID);

                    if (usageJ > usageI)
                    {
                        var temp = ordered[i];
                        ordered[i] = ordered[j];
                        ordered[j] = temp;
                    }
                }
            }

            var result = new List<BreathingExercise>();
            for (int i = 0; i < ordered.Count && i < count; i++)
            {
                result.Add(ordered[i]);
            }

            return Task.FromResult(result);
        }
    }
}
