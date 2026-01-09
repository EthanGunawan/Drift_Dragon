using System.Text.Json;
using System.Collections.Generic;

namespace Drift_Dragon.BusinessLogic
{
    public class BreathingExerciseManager
    {
        private List<BreathingExercise> _exercises = new();
        private readonly Dictionary<int, int> _usageCounts = new();

        public async Task LoadFromJsonAsync()
        {
            var wrapperJson = await JsonDataService.LoadJsonAsync<Dictionary<string, object>>("breathingexercises.json");
            if (wrapperJson?.ContainsKey("BreathingExercises") == true)
            {
                var exercisesJson = JsonSerializer.Serialize(wrapperJson["BreathingExercises"]);
                var data = JsonSerializer.Deserialize<List<BreathingExercise>>(exercisesJson, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (data != null)
                    _exercises = data;
            }
        }

        public Task<List<BreathingExercise>> GetAllAsync() => Task.FromResult(_exercises);
        
        public Task<BreathingExercise?> GetByIdAsync(int id) 
        {
            foreach (var exercise in _exercises)
            {
                if (exercise.BreathingExerciseID == id)
                    return Task.FromResult<BreathingExercise?>(exercise);
            }
            return Task.FromResult<BreathingExercise?>(null);
        }

        public Task IncrementUsageAsync(int exerciseId)
        {
            _usageCounts[exerciseId] = _usageCounts.ContainsKey(exerciseId) ? _usageCounts[exerciseId] + 1 : 1;
            return Task.CompletedTask;
        }

        public Task<List<BreathingExercise>> GetTopUsedAsync(int count = 5) 
        {
            // Simple bubble sort by usage count
            var indexedExercises = new List<(BreathingExercise exercise, int usage, int index)>();
            for (int i = 0; i < _exercises.Count; i++)
            {
                var usage = _usageCounts.ContainsKey(_exercises[i].BreathingExerciseID) ? _usageCounts[_exercises[i].BreathingExerciseID] : 0;
                indexedExercises.Add((_exercises[i], usage, i));
            }

            // Bubble sort by usage (descending)
            for (int i = 0; i < indexedExercises.Count; i++)
            {
                for (int j = 0; j < indexedExercises.Count - 1; j++)
                {
                    if (indexedExercises[j].usage < indexedExercises[j + 1].usage)
                    {
                        var temp = indexedExercises[j];
                        indexedExercises[j] = indexedExercises[j + 1];
                        indexedExercises[j + 1] = temp;
                    }
                }
            }

            var topExercises = new List<BreathingExercise>();
            for (int i = 0; i < Math.Min(count, indexedExercises.Count); i++)
            {
                topExercises.Add(indexedExercises[i].exercise);
            }
            return Task.FromResult(topExercises);
        }
    }
}
