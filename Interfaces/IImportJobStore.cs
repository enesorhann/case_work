namespace MiniDpp.Interfaces
{
    using MiniDpp.Dtos.Import;

    // Arka plan içe aktarma işlerinin durumunu saklar (Redis).
    public interface IImportJobStore
    {
        Task SaveAsync(ImportJobDto job);
        Task<ImportJobDto?> GetAsync(string jobId);
    }
}
