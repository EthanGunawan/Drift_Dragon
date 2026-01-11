using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Drift_Dragon.BusinessLogic
{
    public class BreathingExerciseManager
    {
        private List<BreathingExercise> _exercises = new();
        private readonly Dictionary<int, int> _usageCounts = new();

        public async Task LoadFromJsonAsync()
        {
            try
            {
                // SIMPLIFIED: Load the wrapper first
                var wrapper = await JsonDataService.LoadJsonAsync<Dictionary<string, JsonElement>>("breathingexercise.json");
                
                if (wrapper != null && wrapper.TryGetValue("BreathingExercises", out var exercisesElement))
                {
                    // Deserialize the array directly
                    var data = JsonSerializer.Deserialize<List<BreathingExercise>>(
                        exercisesElement.GetRawText(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (data != null)
                    {
                        _exercises = data;
                        System.Diagnostics.Debug.WriteLine($"✅ Loaded {_exercises.Count} exercises");
                        return;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("❌ Failed to parse breathingexercises.json");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ JSON Error: {ex.Message}");
            }
            
            // FALLBACK: Load as simple array if wrapper fails
            var fallback = await JsonDataService.LoadJsonAsync<List<BreathingExercise>>("breathingexercises.json");
            if (fallback != null)
            {
                _exercises = fallback;
                System.Diagnostics.Debug.WriteLine($"✅ Fallback loaded {_exercises.Count} exercises");
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
            // Simple bubble sort by usage count (unchanged)
            var indexedExercises = new List<(BreathingExercise exercise, int usage, int index)>();
            for (int i = 0; i < _exercises.Count; i++)
            {
                var usage = _usageCounts.ContainsKey(_exercises[i].BreathingExerciseID) ? _usageCounts[_exercises[i].BreathingExerciseID] : 0;
                indexedExercises.Add((_exercises[i], usage, i));
            }

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
