namespace MiniDpp.Dtos.Import
{
    public class ImportResultDto
    {
        public int TotalRows { get; set; }
        public int SavedCount => Saved.Count;
        public int ErrorCount => Errors.Count;
        public List<SavedRowDto> Saved { get; set; } = new();
        public List<RowErrorDto> Errors { get; set; } = new();
    }
}
