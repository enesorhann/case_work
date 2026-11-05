using MiniDpp.Constants;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MiniDpp.Services.RabbitMQ
{
    // Kuyruğu dinleyen arka plan servisi (Celery worker'ın .NET karşılığı).
    public class DppEventConsumer : BackgroundService
    {
        private readonly RabbitMqConnection _connection;
        private readonly DppEventDispatcher _dispatcher;
        private readonly ILogger<DppEventConsumer> _logger;

        public DppEventConsumer(RabbitMqConnection connection, DppEventDispatcher dispatcher, ILogger<DppEventConsumer> logger)
        {
            _connection = connection;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_connection.Enabled)
            {
                return;
            }

            // RabbitMQ, API'den sonra ayağa kalkabilir; bu yüzden bağlanana kadar tekrar denenir.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("RabbitMQ tüketicisi çalışamadı ({Message}). 5 sn sonra tekrar denenecek.", ex.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        private async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            var connection = await _connection.GetAsync(stoppingToken);
            if (connection == null)
            {
                return;
            }

            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: RabbitMqConstants.DppQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, args) =>
            {
                var eventType = args.BasicProperties.Type;
                _logger.LogInformation("Olay alındı: {EventType}", eventType);

                try
                {
                    await _dispatcher.DispatchAsync(eventType, args.Body.ToArray(), stoppingToken);
                }
                catch (Exception ex)
                {
                    // Tek bir hatalı mesaj yüzünden tüketici durmasın.
                    _logger.LogError(ex, "Olay işlenemedi: {EventType}", eventType);
                }
            };

            await channel.BasicConsumeAsync(
                queue: RabbitMqConstants.DppQueueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation("RabbitMQ kuyruğu dinleniyor: {Queue}", RabbitMqConstants.DppQueueName);

            // Bağlantı açık kaldığı sürece bekle; koparsa dış döngü yeniden bağlanır.
            while (connection.IsOpen && channel.IsOpen)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}
