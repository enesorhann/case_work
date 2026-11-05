namespace MiniDpp.Helpers
{
    // GTIN-14 kuralları (GS1 kontrol basamağı)
    public static class GtinHelper
    {
        // GS1 algoritması: sağdan başlayarak rakamlar sırayla 3 ve 1 ile çarpılır.
        public static int CalculateCheckDigit(string first13Digits)
        {
            var total = 0;
            for (var index = 0; index < first13Digits.Length; index++)
            {
                var digit = first13Digits[first13Digits.Length - 1 - index] - '0';
                var weight = index % 2 == 0 ? 3 : 1;
                total += digit * weight;
            }
            return (10 - total % 10) % 10;
        }

        // GTIN için hata mesajlarını döner. Liste boşsa GTIN geçerlidir.
        public static List<string> GetErrors(string gtin)
        {
            if (gtin == string.Empty)
            {
                return new List<string> { "GTIN boş olamaz." };
            }

            if (!gtin.All(char.IsAsciiDigit))
            {
                return new List<string> { $"GTIN yalnızca rakamlardan oluşmalı (gelen: '{gtin}')." };
            }

            if (gtin.Length != 14)
            {
                var message = $"GTIN 14 haneli olmalı (gelen: {gtin.Length} hane).";
                if (gtin.Length < 14)
                {
                    message += " Baştaki sıfırlar silinmiş olabilir (ör. Excel ile kaydetme).";
                }
                return new List<string> { message };
            }

            var expected = CalculateCheckDigit(gtin[..13]);
            var actual = gtin[13] - '0';
            if (expected != actual)
            {
                return new List<string> { $"GTIN kontrol basamağı hatalı (beklenen: {expected}, gelen: {actual})." };
            }

            return new List<string>();
        }

        public static bool IsValid(string gtin) => GetErrors(gtin).Count == 0;
    }
}
