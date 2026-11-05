using System.Text.Json;
using MiniDpp.Constants;
using MiniDpp.Interfaces;
using RabbitMQ.Client;

namespace MiniDpp.Services.RabbitMQ
{
    // Olayları RabbitMQ kuyruğuna yazar. RabbitMQ yoksa olay aynı süreçte hemen işlenir,
    // böylece uygulama RabbitMQ kurulmadan da çalışır.
    public class DppEventPublisher : IEventPublisher
    {
        private readonly RabbitMqConnection _connection;
        private readonly DppEventDispatcher _dispatcher;
        private readonly ILogger<DppEventPublisher> _logger;

        public DppEventPublisher(RabbitMqConnection connection, DppEventDispatcher dispatcher, ILogger<DppEventPublisher> logger)
        {
            _connection = connection;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
            where TEvent : class
        {
            var eventType = typeof(TEvent).Name;
            var body = JsonSerializer.SerializeToUtf8Bytes(eventMessage);

            var connection = await _connection.GetAsync(cancellationToken);
            if (connection == null)
            {
                _logger.LogInformation("RabbitMQ kullanılmıyor, {EventType} aynı süreçte işleniyor.", eventType);
                await _dispatcher.DispatchAsync(eventType, body, cancellationToken);
                return;
            }

            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: RabbitMqConstants.DppQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            // Tek kuyruk kullanıldığı için tüketicinin olayı ayırt edebilmesi adına
            // olay tipi mesaj özelliğine yazılır.
            var properties = new BasicProperties
            {
                Type = eventType,
                ContentType = "application/json",
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: RabbitMqConstants.DppQueueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}
