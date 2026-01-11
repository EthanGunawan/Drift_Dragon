using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Drift_Dragon.BusinessLogic
{
    public class RelaxingAudioManager
    {
        private List<RelaxingAudio> _audios = new();

        public async Task LoadFromJsonAsync()
        {
            var data = await JsonDataService.LoadJsonAsync<List<RelaxingAudio>>("audio.json");
            if (data != null)
                _audios = data;
        }

        public Task<List<RelaxingAudio>> GetAllAsync() => Task.FromResult(_audios);
        
        public Task<RelaxingAudio?> GetByIdAsync(int id) 
        {
            foreach (var audio in _audios)
            {
                if (audio.RelaxingAudioID == id)
                    return Task.FromResult<RelaxingAudio?>(audio);
            }
            return Task.FromResult<RelaxingAudio?>(null);
        }

        public Task<List<RelaxingAudio>> GetByCategoryAsync(string category) 
        {
            var filtered = new List<RelaxingAudio>();
            foreach (var audio in _audios)
            {
                if (string.Equals(audio.Category, category, StringComparison.OrdinalIgnoreCase))
                {
                    filtered.Add(audio);
                }
            }
            return Task.FromResult(filtered);
        }
    }
}