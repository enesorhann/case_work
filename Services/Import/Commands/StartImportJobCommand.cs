namespace MiniDpp.Services.Import.Commands
{
    using MediatR;
    using MiniDpp.Constants;
    using MiniDpp.Dtos.Import;
    using MiniDpp.Interfaces;
    using MiniDpp.Models;

    // CSV içe aktarmayı arka plana (RabbitMQ) gönderir ve iş numarasını döner.
    public class StartImportJobCommand : IRequest<ImportJobCreatedDto>
    {
        public string CsvText { get; set; } = string.Empty;
    }

    public class StartImportJobCommandHandler : IRequestHandler<StartImportJobCommand, ImportJobCreatedDto>
    {
        private readonly IImportJobStore jobStore;
        private readonly IEventPublisher eventPublisher;

        public StartImportJobCommandHandler(IImportJobStore jobStore, IEventPublisher eventPublisher)
        {
            this.jobStore = jobStore;
            this.eventPublisher = eventPublisher;
        }

        public async Task<ImportJobCreatedDto> Handle(StartImportJobCommand request, CancellationToken cancellationToken)
        {
            var jobId = Guid.NewGuid().ToString();
            await jobStore.SaveAsync(new ImportJobDto { JobId = jobId, Status = ImportJobStatuses.Pending });

            await eventPublisher.PublishAsync(
                new ImportRequestedEvent { JobId = jobId, CsvText = request.CsvText },
                cancellationToken);

            return new ImportJobCreatedDto { JobId = jobId, StatusUrl = $"/api/import/jobs/{jobId}" };
        }
    }
}
