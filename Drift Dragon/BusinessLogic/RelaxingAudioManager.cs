using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Drift_Dragon.BusinessLogic
{
    public class RelaxingAudioManager
    {
        private List<RelaxingAudio> _audios = new();
        private readonly Dictionary<int, int> _usageCounts = new();

        private const string AudioUsageFile = "audio_usage.json";

        public async Task LoadFromJsonAsync()
        {
            var data = await JsonDataService.LoadJsonAsync<List<RelaxingAudio>>("audio.json");
            if (data != null)
            {
                _audios = data;
            }

            _ = LoadUsageAsync();
        }

        private async Task LoadUsageAsync()
        {
            try
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, AudioUsageFile);
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
                var path = Path.Combine(FileSystem.AppDataDirectory, AudioUsageFile);
                var json = JsonSerializer.Serialize(_usageCounts);
                await File.WriteAllTextAsync(path, json);
            }
            catch
            {
            }
        }

        public Task<List<RelaxingAudio>> GetAllAsync()
        {
            var copy = new List<RelaxingAudio>();
            foreach (var a in _audios)
            {
                copy.Add(a);
            }
            return Task.FromResult(copy);
        }

        public Task<RelaxingAudio?> GetByIdAsync(int id)
        {
            RelaxingAudio? found = null;
            foreach (var a in _audios)
            {
                if (a.RelaxingAudioID == id)
                {
                    found = a;
                    break;
                }
            }
            return Task.FromResult(found);
        }

        public Task<List<RelaxingAudio>> GetByCategoryAsync(string category)
        {
            var result = new List<RelaxingAudio>();
            foreach (var a in _audios)
            {
                if (a.Category != null && a.Category.ToLower() == category.ToLower())
                {
                    result.Add(a);
                }
            }
            return Task.FromResult(result);
        }

        public async Task IncrementUsageAsync(int audioId)
        {
            if (_usageCounts.ContainsKey(audioId))
            {
                _usageCounts[audioId] = _usageCounts[audioId] + 1;
            }
            else
            {
                _usageCounts[audioId] = 1;
            }

            await SaveUsageAsync();
        }

        public int GetUsage(int audioId)
        {
            if (_usageCounts.ContainsKey(audioId))
                return _usageCounts[audioId];

            return 0;
        }

        public Task<List<RelaxingAudio>> GetTopUsedAsync(int count = 5)
        {
            var ordered = new List<RelaxingAudio>();
            foreach (var a in _audios)
            {
                ordered.Add(a);
            }

            for (int i = 0; i < ordered.Count - 1; i++)
            {
                for (int j = i + 1; j < ordered.Count; j++)
                {
                    int usageI = GetUsage(ordered[i].RelaxingAudioID);
                    int usageJ = GetUsage(ordered[j].RelaxingAudioID);

                    if (usageJ > usageI)
                    {
                        var temp = ordered[i];
                        ordered[i] = ordered[j];
                        ordered[j] = temp;
                    }
                }
            }

            var result = new List<RelaxingAudio>();
            for (int i = 0; i < ordered.Count && i < count; i++)
            {
                result.Add(ordered[i]);
            }

            return Task.FromResult(result);
        }
    }
}
