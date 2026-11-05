namespace MiniDpp.Models
{
    // CSV'den okunan ham satır. Sütun sayısı uymuyorsa Values null, RowError doludur.
    public class CsvRow
    {
        public int Line { get; set; }
        public Dictionary<string, string>? Values { get; set; }
        public string? RowError { get; set; }
    }
}
