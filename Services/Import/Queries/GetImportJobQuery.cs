namespace MiniDpp.Services.Import.Queries
{
    using MediatR;
    using MiniDpp.Constants;
    using MiniDpp.Dtos.Import;
    using MiniDpp.Interfaces;

    public class GetImportJobQuery : IRequest<ImportJobDto>
    {
        public string JobId { get; set; } = string.Empty;
    }

    public class GetImportJobQueryHandler : IRequestHandler<GetImportJobQuery, ImportJobDto>
    {
        private readonly IImportJobStore jobStore;

        public GetImportJobQueryHandler(IImportJobStore jobStore)
        {
            this.jobStore = jobStore;
        }

        public async Task<ImportJobDto> Handle(GetImportJobQuery request, CancellationToken cancellationToken)
        {
            return await jobStore.GetAsync(request.JobId)
                ?? new ImportJobDto { JobId = request.JobId, Status = ImportJobStatuses.Pending };
        }
    }
}
