using System.Text.Json;
using MediatR;
using MiniDpp.Constants;
using MiniDpp.Dtos.Import;
using MiniDpp.Interfaces;
using MiniDpp.Models;
using MiniDpp.Services.Import.Commands;
using MiniDpp.Services.QrCode.Commands;

namespace MiniDpp.Services.RabbitMQ
{
    // Gelen olayı tipine göre çözümleyip ilgili Command'ı Mediator'a gönderir.
    // Hem RabbitMQ tüketicisi hem de RabbitMQ yokken yayıncı tarafından kullanılır.
    public class DppEventDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DppEventDispatcher> _logger;

        public DppEventDispatcher(IServiceScopeFactory scopeFactory, ILogger<DppEventDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task DispatchAsync(string? eventType, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
        {
            // Her olay kendi DI kapsamında (kendi DbContext'iyle) işlenir.
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            switch (eventType)
            {
                case nameof(QrCodeRequestedEvent):
                    var qrEvent = JsonSerializer.Deserialize<QrCodeRequestedEvent>(body.Span)!;
                    await mediator.Send(new GenerateQrCodeCommand { Gtin = qrEvent.Gtin }, cancellationToken);
                    break;

                case nameof(ImportRequestedEvent):
                    var importEvent = JsonSerializer.Deserialize<ImportRequestedEvent>(body.Span)!;
                    var jobStore = scope.ServiceProvider.GetRequiredService<IImportJobStore>();
                    await RunImportAsync(importEvent, mediator, jobStore, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Bilinmeyen olay tipi '{EventType}', mesaj atlandı.", eventType);
                    break;
            }
        }

        private async Task RunImportAsync(ImportRequestedEvent importEvent, IMediator mediator,
            IImportJobStore jobStore, CancellationToken cancellationToken)
        {
            var job = new ImportJobDto { JobId = importEvent.JobId, Status = ImportJobStatuses.Started };
            await jobStore.SaveAsync(job);

            try
            {
                job.Result = await mediator.Send(new ImportProductsCommand { CsvText = importEvent.CsvText }, cancellationToken);
                job.Status = ImportJobStatuses.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İçe aktarma işi başarısız: {JobId}", importEvent.JobId);
                job.Status = ImportJobStatuses.Failure;
                job.Error = ex.Message;
            }

            await jobStore.SaveAsync(job);
        }
    }
}
