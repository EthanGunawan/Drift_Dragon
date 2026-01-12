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
                _audios = data;

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
                var path = Path.Combine(FileSystem.AppDataDirectory, AudioUsageFile);
                var json = JsonSerializer.Serialize(_usageCounts);
                await File.WriteAllTextAsync(path, json);
            }
            catch
            {
            }
        }

        public Task<List<RelaxingAudio>> GetAllAsync() =>
            Task.FromResult(_audios.ToList());

        public Task<RelaxingAudio?> GetByIdAsync(int id)
        {
            var audio = _audios.FirstOrDefault(a => a.RelaxingAudioID == id);
            return Task.FromResult(audio);
        }

        public Task<List<RelaxingAudio>> GetByCategoryAsync(string category)
        {
            var filtered = _audios
                .Where(a => string.Equals(a.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Task.FromResult(filtered);
        }

        public async Task IncrementUsageAsync(int audioId)
        {
            _usageCounts[audioId] = _usageCounts.TryGetValue(audioId, out var c) ? c + 1 : 1;
            await SaveUsageAsync();
        }

        public int GetUsage(int audioId) =>
            _usageCounts.TryGetValue(audioId, out var c) ? c : 0;

        public Task<List<RelaxingAudio>> GetTopUsedAsync(int count = 5)
        {
            var ordered = _audios
                .OrderByDescending(a => GetUsage(a.RelaxingAudioID))
                .Take(count)
                .ToList();

            return Task.FromResult(ordered);
        }
    }
}
