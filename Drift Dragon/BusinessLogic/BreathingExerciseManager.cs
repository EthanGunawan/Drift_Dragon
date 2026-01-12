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
           

            _exercises.Clear();
            _exercises.AddRange(data);
            
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
                    _usageCounts[kvp.Key] = kvp.Value;
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

        public Task<List<BreathingExercise>> GetAllAsync() =>
            Task.FromResult(_exercises.ToList());

        public Task<BreathingExercise?> GetByIdAsync(int id)
        {
            var exercise = _exercises.FirstOrDefault(e => e.BreathingExerciseID == id);
            return Task.FromResult(exercise);
        }

        public async Task IncrementUsageAsync(int exerciseId)
        {
            _usageCounts[exerciseId] = _usageCounts.TryGetValue(exerciseId, out var c) ? c + 1 : 1;
            await SaveUsageAsync();
        }

        public int GetUsage(int exerciseId) =>
            _usageCounts.TryGetValue(exerciseId, out var c) ? c : 0;

        public Task<List<BreathingExercise>> GetTopUsedAsync(int count = 5)
        {
            var ordered = _exercises
                .OrderByDescending(e => GetUsage(e.BreathingExerciseID))
                .Take(count)
                .ToList();

            return Task.FromResult(ordered);
        }
    }
}
