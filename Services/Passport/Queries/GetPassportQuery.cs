namespace MiniDpp.Services.Passport.Queries
{
    using AutoMapper;
    using MediatR;
    using Microsoft.Extensions.Options;
    using MiniDpp.Constants;
    using MiniDpp.Dtos.Passport;
    using MiniDpp.Interfaces;
    using MiniDpp.Interfaces.Product;
    using MiniDpp.Models;

    // GTIN'e göre ürün pasaportu.
    // Önce Redis önbelleğine bakılır; yoksa veritabanından okunup önbelleğe yazılır.
    // Önbellekte tam veri (karbon dahil) durur; rol filtresi her zaman EN SON uygulanır.
    public class GetPassportQuery : IRequest<PassportDto?>
    {
        public string Gtin { get; set; } = string.Empty;
        public string Role { get; set; } = Roles.Consumer;
    }

    public class GetPassportQueryHandler : IRequestHandler<GetPassportQuery, PassportDto?>
    {
        private readonly IProductRepository productRepository;
        private readonly ICacheService cacheService;
        private readonly IMapper mapper;
        private readonly DppSettings settings;

        public GetPassportQueryHandler(
            IProductRepository productRepository,
            ICacheService cacheService,
            IMapper mapper,
            IOptions<DppSettings> settings)
        {
            this.productRepository = productRepository;
            this.cacheService = cacheService;
            this.mapper = mapper;
            this.settings = settings.Value;
        }

        public async Task<PassportDto?> Handle(GetPassportQuery request, CancellationToken cancellationToken)
        {
            var key = CacheKeys.Passport(request.Gtin);
            var passport = await cacheService.GetAsync<PassportDto>(key);

            if (passport == null)
            {
                var product = await productRepository.GetByGtinAsync(request.Gtin);
                if (product == null)
                {
                    return null;
                }

                passport = mapper.Map<PassportDto>(product);
                await cacheService.SetAsync(key, passport, TimeSpan.FromSeconds(settings.CacheSeconds));
            }

            return passport.ForRole(request.Role);
        }
    }
}
