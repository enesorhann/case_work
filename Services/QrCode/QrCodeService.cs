using MiniDpp.Interfaces;
using QRCoder;

namespace MiniDpp.Services.QrCode
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] CreatePng(string text)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            return new PngByteQRCode(data).GetGraphic(8);
        }
    }
}
