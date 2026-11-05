namespace MiniDpp.Helpers
{
    // GitHub Codespaces içinde çalışırken dışarıdan erişilen adresi bulur.
    // Örnek: https://{CODESPACE_NAME}-8080.app.github.dev
    public static class CodespacesHelper
    {
        public static string? GetForwardedUrl(int port)
        {
            var name = Environment.GetEnvironmentVariable("CODESPACE_NAME");
            var domain = Environment.GetEnvironmentVariable("GITHUB_CODESPACES_PORT_FORWARDING_DOMAIN");

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(domain))
            {
                return null;
            }
            return $"https://{name}-{port}.{domain}";
        }
    }
}
