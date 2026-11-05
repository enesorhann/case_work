namespace MiniDpp.Interfaces
{
    // Arka plan işleri için olay yayını (RabbitMQ ya da aynı süreçte çalıştırma)
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent eventMessage, CancellationToken cancellationToken = default)
            where TEvent : class;
    }
}
