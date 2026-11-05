namespace MiniDpp.Constants
{
    public static class CacheKeys
    {
        public static string Passport(string gtin) => $"dpp:passport:{gtin}";
        public static string ImportJob(string jobId) => $"dpp:job:{jobId}";
    }
}
