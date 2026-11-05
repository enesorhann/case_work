namespace MiniDpp.Exceptions
{
    // Dosyanın tamamını etkileyen hata (boş dosya, eksik sütun vb.).
    public class CsvFormatException : Exception
    {
        public CsvFormatException(string message) : base(message)
        {
        }
    }
}
