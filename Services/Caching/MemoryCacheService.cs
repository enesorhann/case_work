using System.Collections.Concurrent;
using System.Text.Json;
using MiniDpp.Interfaces;

namespace MiniDpp.Services.Caching
{
    // Redis yokken kullanılan basit, süreç içi önbellek.
    // Değerler JSON olarak tutulur; böylece Redis ile aynı davranır (kopya döner).
    public class MemoryCacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, (DateTime ExpiresAt, string Json)> _items = new();

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (_items.TryGetValue(key, out var item))
            {
                if (DateTime.UtcNow < item.ExpiresAt)
                {
                    return Task.FromResult(JsonSerializer.Deserialize<T>(item.Json));
                }
                _items.TryRemove(key, out _);
            }
            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class
        {
            _items[key] = (DateTime.UtcNow + expiry, JsonSerializer.Serialize(value));
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _items.TryRemove(key, out _);
            return Task.CompletedTask;
        }
    }
}
