namespace MiniDpp.Services.QrCode.Queries
{
    using MediatR;
    using MiniDpp.Interfaces;
    using MiniDpp.Services.QrCode.Commands;

    // Saklanan QR kod PNG dosyası. Henüz üretilmediyse null döner.
    public class GetQrCodeQuery : IRequest<byte[]?>
    {
        public string Gtin { get; set; } = string.Empty;
    }

    public class GetQrCodeQueryHandler : IRequestHandler<GetQrCodeQuery, byte[]?>
    {
        private readonly IFileStorage fileStorage;

        public GetQrCodeQueryHandler(IFileStorage fileStorage)
        {
            this.fileStorage = fileStorage;
        }

        public async Task<byte[]?> Handle(GetQrCodeQuery request, CancellationToken cancellationToken)
        {
            return await fileStorage.ReadAsync(GenerateQrCodeCommand.FileName(request.Gtin));
        }
    }
}
