using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Drift_Dragon.BusinessLogic
{
    public class AdviceManager
    {
        private List<Advice> _advices = new();
        private readonly List<int> _starredAdviceIds = new();
        private readonly Random _random = new();

        public async Task LoadFromJsonAsync()
        {
            var data = await JsonDataService.LoadJsonAsync<List<Advice>>("advice.json");
            if (data != null)
                _advices = data;
        }

        public Task<List<Advice>> GetAllAsync()
        {
            var copy = new List<Advice>();
            foreach (var a in _advices)
            {
                copy.Add(a);
            }
            return Task.FromResult(copy);
        }

        public Task<Advice?> GetRandomAsync()
        {
            if (_advices.Count == 0)
                return Task.FromResult<Advice?>(null);

            int index = _random.Next(_advices.Count);
            return Task.FromResult<Advice?>(_advices[index]);
        }

        public Task<List<Advice>> GetStarredAsync()
        {
            var starred = new List<Advice>();
            foreach (var advice in _advices)
            {
                bool isStarred = false;
                foreach (var id in _starredAdviceIds)
                {
                    if (advice.adviceID == id)
                    {
                        isStarred = true;
                        break;
                    }
                }

                if (isStarred)
                    starred.Add(advice);
            }
            return Task.FromResult(starred);
        }

        public Task<bool> ToggleStarAsync(int adviceId)
        {
            for (int i = 0; i < _starredAdviceIds.Count; i++)
            {
                if (_starredAdviceIds[i] == adviceId)
                {
                    _starredAdviceIds.RemoveAt(i);
                    return Task.FromResult(true);
                }
            }

            _starredAdviceIds.Add(adviceId);
            return Task.FromResult(true);
        }

        public Task<bool> IsStarredAsync(int adviceId)
        {
            foreach (var id in _starredAdviceIds)
            {
                if (id == adviceId)
                    return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
