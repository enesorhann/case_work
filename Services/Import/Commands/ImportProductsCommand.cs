namespace MiniDpp.Services.Import.Commands
{
    using System.Text.Json;
    using AutoMapper;
    using MediatR;
    using MiniDpp.Constants;
    using MiniDpp.Constants.Enums;
    using MiniDpp.Domain.Entities;
    using MiniDpp.Dtos.Import;
    using MiniDpp.Dtos.Product;
    using MiniDpp.Helpers;
    using MiniDpp.Interfaces;
    using MiniDpp.Interfaces.Product;
    using MiniDpp.Models;

    // CSV içe aktarma.
    // Akış: CSV oku -> her satırı doğrula -> geçerli satırları kaydet -> önbelleği temizle
    // -> her kaydedilen ürün için QR kod olayını yayınla (RabbitMQ).
    // Hatalı satırlar kaydedilmez ama diğer satırların kaydı devam eder.
    public class ImportProductsCommand : IRequest<ImportResultDto>
    {
        public string CsvText { get; set; } = string.Empty;
    }

    public class ImportProductsCommandHandler : IRequestHandler<ImportProductsCommand, ImportResultDto>
    {
        private readonly IProductRepository productRepository;
        private readonly ICacheService cacheService;
        private readonly IEventPublisher eventPublisher;
        private readonly IMapper mapper;
        private readonly ILogger<ImportProductsCommandHandler> logger;

        public ImportProductsCommandHandler(
            IProductRepository productRepository,
            ICacheService cacheService,
            IEventPublisher eventPublisher,
            IMapper mapper,
            ILogger<ImportProductsCommandHandler> logger)
        {
            this.productRepository = productRepository;
            this.cacheService = cacheService;
            this.eventPublisher = eventPublisher;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<ImportResultDto> Handle(ImportProductsCommand request, CancellationToken cancellationToken)
        {
            var result = new ImportResultDto();
            var validRows = new List<ProductRow>();
            var firstLineOfGtin = new Dictionary<string, int>();

            // 1) Doğrulama
            foreach (var csvRow in CsvFileReader.ReadRows(request.CsvText))
            {
                result.TotalRows++;

                if (csvRow.Values == null)
                {
                    result.Errors.Add(new RowErrorDto { Line = csvRow.Line, Errors = { csvRow.RowError! } });
                    continue;
                }

                var (row, errors) = ProductRowValidator.Validate(csvRow.Values, csvRow.Line);
                var gtin = csvRow.Values["gtin"].Trim();
                var modelName = csvRow.Values["model_adi"].Trim();

                if (row != null && firstLineOfGtin.TryGetValue(gtin, out var firstLine))
                {
                    errors = new List<string> { $"Bu GTIN dosyada {firstLine}. satırda zaten var." };
                    row = null;
                }

                if (row == null)
                {
                    result.Errors.Add(new RowErrorDto { Line = csvRow.Line, Gtin = gtin, ModelName = modelName, Errors = errors });
                    continue;
                }

                firstLineOfGtin[gtin] = csvRow.Line;
                validRows.Add(row);
            }

            // 2) Kayıt (tek transaction)
            foreach (var row in validRows)
            {
                var status = await SaveRowAsync(row);
                result.Saved.Add(new SavedRowDto { Line = row.Line, Gtin = row.Gtin, ModelName = row.ModelName, Status = status });
            }
            await productRepository.SaveChangesAsync();

            // 3) Kayıt sonrası: önbelleği temizle, QR kodlarını arka planda üret
            foreach (var saved in result.Saved)
            {
                await cacheService.RemoveAsync(CacheKeys.Passport(saved.Gtin));
                try
                {
                    await eventPublisher.PublishAsync(new QrCodeRequestedEvent { Gtin = saved.Gtin }, cancellationToken);
                }
                catch (Exception ex)
                {
                    // QR kod istendiğinde de üretilebildiği için bu hata içe aktarmayı bozmaz.
                    logger.LogError(ex, "QR kod olayı yayınlanamadı: {Gtin}", saved.Gtin);
                }
            }

            return result;
        }

        private async Task<RowSaveStatus> SaveRowAsync(ProductRow row)
        {
            var now = DateTime.UtcNow;
            var model = await productRepository.GetOrCreateModelAsync(row.ModelName);
            var product = await productRepository.GetByGtinAsync(row.Gtin, tracking: true);

            if (product == null)
            {
                product = new Product
                {
                    Gtin = row.Gtin,
                    Name = row.ProductName,
                    Model = model,
                    CottonPercent = row.CottonPercent,
                    ElastanePercent = row.ElastanePercent,
                    CarbonKgCo2e = row.CarbonKgCo2e,
                    Version = 1,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                product.Batches.Add(new Batch { BatchNumber = row.BatchNumber, ProductionPlace = row.ProductionPlace, CreatedAt = now });
                await productRepository.AddAsync(product);
                return RowSaveStatus.Created;
            }

            // Var olan ürün: değişiklik varsa eski hali sürüm olarak saklanır (bonus).
            var oldSnapshot = TakeSnapshot(product);

            product.Name = row.ProductName;
            product.Model = model;
            product.CottonPercent = row.CottonPercent;
            product.ElastanePercent = row.ElastanePercent;
            product.CarbonKgCo2e = row.CarbonKgCo2e;

            var batch = product.Batches.FirstOrDefault(b => b.BatchNumber == row.BatchNumber);
            if (batch == null)
            {
                product.Batches.Add(new Batch { BatchNumber = row.BatchNumber, ProductionPlace = row.ProductionPlace, CreatedAt = now });
            }
            else
            {
                batch.ProductionPlace = row.ProductionPlace;
            }

            if (TakeSnapshot(product) == oldSnapshot)
            {
                return RowSaveStatus.Unchanged;
            }

            await productRepository.AddVersionAsync(new ProductVersion
            {
                Product = product,
                VersionNumber = product.Version,
                SnapshotJson = oldSnapshot,
                SavedAt = now
            });
            product.Version++;
            product.UpdatedAt = now;
            return RowSaveStatus.Updated;
        }

        private string TakeSnapshot(Product product)
        {
            return JsonSerializer.Serialize(mapper.Map<ProductSnapshotDto>(product), JsonOptions.Default);
        }
    }
}
