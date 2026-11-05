namespace MiniDpp.Models
{
    // CSV'nin arka planda içe aktarılması istendiğinde yayınlanır.
    public class ImportRequestedEvent
    {
        public string JobId { get; set; } = string.Empty;
        public string CsvText { get; set; } = string.Empty;
    }
}
