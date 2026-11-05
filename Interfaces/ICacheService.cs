namespace MiniDpp.Interfaces
{
    // Anahtar -> nesne önbelleği (Redis ya da bellek)
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan expiry) where T : class;
        Task RemoveAsync(string key);
    }
}
