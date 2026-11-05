using Microsoft.Extensions.Options;
using MiniDpp.Models;
using StackExchange.Redis;

namespace MiniDpp.Services.Caching
{
    // Redis bağlantısı ilk kullanımda bir kez denenir. Ulaşılamazsa null döner ve bellek önbelleği kullanılır.
    public class RedisConnection
    {
        private readonly Lazy<IDatabase?> _database;

        public RedisConnection(IOptions<RedisSettings> settings, ILogger<RedisConnection> logger)
        {
            _database = new Lazy<IDatabase?>(() => Connect(settings.Value, logger));
        }

        public IDatabase? Database => _database.Value;

        private static IDatabase? Connect(RedisSettings settings, ILogger logger)
        {
            if (!settings.Enabled)
            {
                logger.LogWarning("Redis kapalı: önbellek bellekte tutulacak.");
                return null;
            }

            try
            {
                var options = ConfigurationOptions.Parse(settings.ConnectionString);
                options.AbortOnConnectFail = true;
                options.ConnectTimeout = 2000;
                var connection = ConnectionMultiplexer.Connect(options);
                logger.LogInformation("Redis bağlı: {ConnectionString}", settings.ConnectionString);
                return connection.GetDatabase();
            }
            catch (Exception ex)
            {
                logger.LogWarning("Redis'e ulaşılamadı ({Message}). Önbellek bellekte tutulacak.", ex.Message);
                return null;
            }
        }
    }
}
