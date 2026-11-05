namespace MiniDpp.Models
{
    // Ürün kaydedildiğinde yayınlanır; tüketici QR kodu üretir.
    public class QrCodeRequestedEvent
    {
        public string Gtin { get; set; } = string.Empty;
    }
}
