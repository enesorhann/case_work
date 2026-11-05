namespace MiniDpp.Services.Product.Queries
{
    using AutoMapper;
    using MediatR;
    using MiniDpp.Dtos.Product;
    using MiniDpp.Interfaces.Product;

    public class GetProductsQuery : IRequest<List<ProductListDto>>
    {
    }

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, List<ProductListDto>>
    {
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;

        public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
        }

        public async Task<List<ProductListDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await productRepository.GetAllAsync();
            return mapper.Map<List<ProductListDto>>(products);
        }
    }
}
