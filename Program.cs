using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MiniDpp.Data;
using MiniDpp.Helpers;
using MiniDpp.Interfaces;
using MiniDpp.Interfaces.Product;
using MiniDpp.Models;
using MiniDpp.Repositories;
using MiniDpp.Services.Caching;
using MiniDpp.Services.ImportJobs;
using MiniDpp.Services.QrCode;
using MiniDpp.Services.RabbitMQ;
using MiniDpp.Services.Storage;

namespace MiniDpp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Sayılar her makinede "12.4" biçiminde gösterilsin (Türkçe Windows'ta "12,4" olmasın).
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            // Windows-1254 (Türkçe) kodlu CSV'leri okuyabilmek için
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var builder = WebApplication.CreateBuilder(args);

            // Ayarlar (appsettings.json ya da ortam değişkenleri)
            builder.Services.Configure<DppSettings>(builder.Configuration.GetSection(DppSettings.SectionName));
            // Codespaces'te BaseUrl elle verilmediyse QR kodlar Codespaces adresini göstersin.
            builder.Services.PostConfigure<DppSettings>(settings =>
            {
                var codespacesUrl = CodespacesHelper.GetForwardedUrl(8080);
                if (codespacesUrl != null && settings.BaseUrl == DppSettings.DefaultBaseUrl)
                {
                    settings.BaseUrl = codespacesUrl;
                }
            });
            builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection(RedisSettings.SectionName));
            builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

            // Veritabanı (SQLite + EF Core)
            builder.Services.AddDbContext<DataContext>((services, options) =>
                options.UseSqlite(services.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")));

            // Repository
            builder.Services.AddScoped<IProductRepository, ProductRepository>();

            // Redis önbelleği (Redis'e ulaşılamazsa bellek önbelleği)
            builder.Services.AddSingleton<RedisConnection>();
            builder.Services.AddSingleton<ICacheService>(services =>
            {
                var database = services.GetRequiredService<RedisConnection>().Database;
                return database != null ? new RedisCacheService(database) : new MemoryCacheService();
            });
            builder.Services.AddSingleton<IImportJobStore, ImportJobStore>();

            // QR kod ve dosya saklama
            builder.Services.AddSingleton<IQrCodeService, QrCodeService>();
            builder.Services.AddSingleton<IFileStorage>(services =>
                new LocalFileStorage(services.GetRequiredService<IOptions<DppSettings>>().Value.QrFolder));

            // RabbitMQ (yayıncı + arka planda kuyruğu dinleyen tüketici)
            builder.Services.AddSingleton<RabbitMqConnection>();
            builder.Services.AddSingleton<DppEventDispatcher>();
            builder.Services.AddSingleton<IEventPublisher, DppEventPublisher>();
            builder.Services.AddHostedService<DppEventConsumer>();

            // CQRS (MediatR) ve AutoMapper
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
                cfg.AddMaps(typeof(MappingProfiles).Assembly);
            });

            // MVC: API controller'ları + HTML pasaport için Razor View
            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options => JsonOptions.Apply(options.JsonSerializerOptions));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "Mini DPP API", Version = "v1" });
                // Controller'lardaki /// <summary> açıklamaları Swagger'da gösterilir.
                var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
                if (File.Exists(xmlFile))
                {
                    options.IncludeXmlComments(xmlFile);
                }
            });

            var app = builder.Build();

            ApplyMigrations(app);

            app.UseSwagger();
            app.UseSwaggerUI();

            // React arayüzü (frontend -> npm run build -> wwwroot)
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapControllers();

            if (!app.Environment.WebRootFileProvider.GetFileInfo("index.html").Exists)
            {
                app.MapGet("/", () => Results.Content(
                    "<h1>Mini DPP API çalışıyor</h1><p>React arayüzü derlenmemiş: <code>cd frontend && npm install && npm run build</code></p>"
                    + "<p><a href=\"/swagger\">Swagger</a></p>",
                    "text/html; charset=utf-8"));
            }

            app.Run();
        }

        private static void ApplyMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            // SQLite dosyasının klasörü yoksa oluştur.
            var dataSource = new SqliteConnectionStringBuilder(context.Database.GetConnectionString()).DataSource;
            var folder = Path.GetDirectoryName(Path.GetFullPath(dataSource));
            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            context.Database.Migrate();
            // API ve arka plan işleri aynı anda okuyup yazabilsin diye
            context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }
    }
}
