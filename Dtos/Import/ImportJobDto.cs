namespace MiniDpp.Dtos.Import
{
    // Arka plan (RabbitMQ) içe aktarma işinin durumu
    public class ImportJobDto
    {
        public string JobId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public ImportResultDto? Result { get; set; }
        public string? Error { get; set; }
    }
}
