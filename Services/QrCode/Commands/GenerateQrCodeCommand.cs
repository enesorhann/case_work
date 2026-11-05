namespace MiniDpp.Services.QrCode.Commands
{
    using MediatR;
    using Microsoft.Extensions.Options;
    using MiniDpp.Interfaces;
    using MiniDpp.Interfaces.Product;
    using MiniDpp.Models;

    // Ürünün HTML DPP adresine yönlenen QR kodu PNG olarak üretir ve saklar.
    // Normalde RabbitMQ tüketicisi tarafından çalıştırılır. Ürün yoksa false döner.
    public class GenerateQrCodeCommand : IRequest<bool>
    {
        public string Gtin { get; set; } = string.Empty;

        public static string FileName(string gtin) => $"{gtin}.png";
    }

    public class GenerateQrCodeCommandHandler : IRequestHandler<GenerateQrCodeCommand, bool>
    {
        private readonly IProductRepository productRepository;
        private readonly IQrCodeService qrCodeService;
        private readonly IFileStorage fileStorage;
        private readonly DppSettings settings;

        public GenerateQrCodeCommandHandler(
            IProductRepository productRepository,
            IQrCodeService qrCodeService,
            IFileStorage fileStorage,
            IOptions<DppSettings> settings)
        {
            this.productRepository = productRepository;
            this.qrCodeService = qrCodeService;
            this.fileStorage = fileStorage;
            this.settings = settings.Value;
        }

        public async Task<bool> Handle(GenerateQrCodeCommand request, CancellationToken cancellationToken)
        {
            if (!await productRepository.ExistsAsync(request.Gtin))
            {
                return false;
            }

            var url = $"{settings.BaseUrl.TrimEnd('/')}/dpp/{request.Gtin}";
            var png = qrCodeService.CreatePng(url);
            await fileStorage.SaveAsync(GenerateQrCodeCommand.FileName(request.Gtin), png);
            return true;
        }
    }
}
