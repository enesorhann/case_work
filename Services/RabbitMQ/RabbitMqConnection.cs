using Microsoft.Extensions.Options;
using MiniDpp.Models;
using RabbitMQ.Client;

namespace MiniDpp.Services.RabbitMQ
{
    // Uygulama boyunca tek RabbitMQ bağlantısı kullanılır.
    // Bağlantı kurulamazsa null döner; RetryDelaySeconds geçmeden tekrar denenmez.
    public sealed class RabbitMqConnection : IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly SemaphoreSlim _lock = new(1, 1);
        private IConnection? _connection;
        private DateTime _retryAfter = DateTime.MinValue;

        public RabbitMqConnection(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqConnection> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public bool Enabled => _settings.Enabled;

        public async Task<IConnection?> GetAsync(CancellationToken cancellationToken = default)
        {
            if (!_settings.Enabled) return null;
            if (_connection is { IsOpen: true }) return _connection;
            if (DateTime.UtcNow < _retryAfter) return null;

            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_connection is { IsOpen: true }) return _connection;

                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(3)
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _logger.LogInformation("RabbitMQ bağlı: {Host}:{Port}", _settings.HostName, _settings.Port);
                return _connection;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _retryAfter = DateTime.UtcNow.AddSeconds(_settings.RetryDelaySeconds);
                _logger.LogWarning("RabbitMQ'ya ulaşılamadı ({Message}).", ex.Message);
                return null;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
