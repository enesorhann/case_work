using MiniDpp.Constants.Enums;

namespace MiniDpp.Dtos.Import
{
    public class SavedRowDto
    {
        public int Line { get; set; }
        public string Gtin { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public RowSaveStatus Status { get; set; }
    }
}
