using MiniDpp.Constants;
using MiniDpp.Dtos.Import;
using MiniDpp.Interfaces;

namespace MiniDpp.Services.ImportJobs
{
    // İş durumları önbellekte (Redis) 1 saat tutulur.
    public class ImportJobStore : IImportJobStore
    {
        private static readonly TimeSpan Expiry = TimeSpan.FromHours(1);
        private readonly ICacheService _cache;

        public ImportJobStore(ICacheService cache)
        {
            _cache = cache;
        }

        public Task SaveAsync(ImportJobDto job) => _cache.SetAsync(CacheKeys.ImportJob(job.JobId), job, Expiry);

        public Task<ImportJobDto?> GetAsync(string jobId) => _cache.GetAsync<ImportJobDto>(CacheKeys.ImportJob(jobId));
    }
}
