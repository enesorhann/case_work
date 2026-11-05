using System.Text.Json;
using MiniDpp.Interfaces;
using StackExchange.Redis;

namespace MiniDpp.Services.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IDatabase database)
        {
            _database = database;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value.ToString()) : null;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class
        {
            await _database.StringSetAsync(key, JsonSerializer.Serialize(value), expiry);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
