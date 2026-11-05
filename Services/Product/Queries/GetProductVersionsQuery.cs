namespace MiniDpp.Services.Product.Queries
{
    using AutoMapper;
    using MediatR;
    using MiniDpp.Dtos.Product;
    using MiniDpp.Interfaces.Product;

    // Ürünün önceki sürümleri (bonus). Ürün yoksa null döner.
    public class GetProductVersionsQuery : IRequest<List<ProductVersionDto>?>
    {
        public string Gtin { get; set; } = string.Empty;
    }

    public class GetProductVersionsQueryHandler : IRequestHandler<GetProductVersionsQuery, List<ProductVersionDto>?>
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;

        public GetProductVersionsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
        }

        public async Task<List<ProductVersionDto>?> Handle(GetProductVersionsQuery request, CancellationToken cancellationToken)
        {
            var product = await productRepository.GetByGtinAsync(request.Gtin);
            if (product == null)
            {
                return null;
            }
            return mapper.Map<List<ProductVersionDto>>(product.Versions.OrderBy(v => v.VersionNumber));
        }
    }
}
