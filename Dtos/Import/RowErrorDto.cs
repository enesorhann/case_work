namespace MiniDpp.Dtos.Import
{
    public class RowErrorDto
    {
        public int Line { get; set; }
        public string Gtin { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}
