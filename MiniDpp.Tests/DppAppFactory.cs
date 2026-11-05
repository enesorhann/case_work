using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MiniDpp.Tests
{
    // Her test kendi geçici SQLite veritabanıyla, Redis ve RabbitMQ olmadan çalışır.
    public class DppAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _folder = Path.Combine(Path.GetTempPath(), "minidpp-test-" + Guid.NewGuid());

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Directory.CreateDirectory(_folder);
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={Path.Combine(_folder, "test.db")}");
            builder.UseSetting("Redis:Enabled", "false");
            builder.UseSetting("RabbitMq:Enabled", "false");
            builder.UseSetting("Dpp:QrFolder", Path.Combine(_folder, "qr"));
            builder.UseSetting("Dpp:BaseUrl", "http://testserver");
            builder.UseSetting("Logging:LogLevel:Default", "Warning");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try { Directory.Delete(_folder, recursive: true); } catch { /* geçici klasör */ }
        }
    }
}
