using System.Globalization;
using MiniDpp.Models;

namespace MiniDpp.Helpers
{
    // Bir CSV satırının doğrulanması.
    // Kurallar (ödev metninden):
    // - GTIN boş olamaz, 14 haneli sayısal metin olmalı, GS1 kontrol basamağı doğru olmalı.
    // - Pamuk ve elastan yüzdeleri sayısal, 0-100 arasında ve toplamları 100 olmalı.
    // - Model adı, batch numarası ve üretim yeri boş olamaz.
    // - Karbon değeri sayısal ve negatif olmayan bir değer olmalı.
    public static class ProductRowValidator
    {
        private const double Tolerance = 0.001; // ondalıklı toplamlar için (ör. 97.5 + 2.5)

        // Geçerliyse (satır, boş liste), değilse (null, hatalar) döner.
        public static (ProductRow? Row, List<string> Errors) Validate(Dictionary<string, string> values, int line)
        {
            var errors = new List<string>();

            // GTIN her zaman metin olarak işlenir; baştaki sıfırlar korunur.
            var gtin = Get(values, "gtin");
            errors.AddRange(GtinHelper.GetErrors(gtin));

            var modelName = Get(values, "model_adi");
            var batchNumber = Get(values, "batch_no");
            var productionPlace = Get(values, "uretim_yeri");
            if (modelName == string.Empty) errors.Add("Model adı boş olamaz.");
            if (batchNumber == string.Empty) errors.Add("Batch numarası boş olamaz.");
            if (productionPlace == string.Empty) errors.Add("Üretim yeri boş olamaz.");

            var cotton = ReadPercent(values, "elyaf_pamuk_yuzde", errors);
            var elastane = ReadPercent(values, "elyaf_elastan_yuzde", errors);
            if (cotton.HasValue && elastane.HasValue)
            {
                var total = cotton.Value + elastane.Value;
                if (Math.Abs(total - 100) > Tolerance)
                {
                    errors.Add($"Elyaf yüzdelerinin toplamı 100 olmalı (gelen: {Format(total)}).");
                }
            }

            var carbonText = Get(values, "karbon_kg_co2e");
            var carbon = ParseNumber(carbonText);
            if (carbonText == string.Empty) errors.Add("karbon_kg_co2e boş olamaz.");
            else if (carbon is null) errors.Add($"karbon_kg_co2e sayısal olmalı (gelen: '{carbonText}').");
            else if (carbon < 0) errors.Add($"karbon_kg_co2e negatif olamaz (gelen: {carbonText}).");

            if (errors.Count > 0)
            {
                return (null, errors);
            }

            var row = new ProductRow
            {
                Line = line,
                Gtin = gtin,
                ProductName = Get(values, "urun_adi"),
                ModelName = modelName,
                BatchNumber = batchNumber,
                ProductionPlace = productionPlace,
                CottonPercent = cotton!.Value,
                ElastanePercent = elastane!.Value,
                CarbonKgCo2e = carbon!.Value
            };
            return (row, errors);
        }

        // "12.5" ve "12,5" biçimlerini kabul eder. Sayı değilse null döner.
        public static double? ParseNumber(string text)
        {
            text = text.Trim();
            if (text.Contains(',') && !text.Contains('.'))
            {
                text = text.Replace(',', '.');
            }

            var styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;
            if (!double.TryParse(text, styles, CultureInfo.InvariantCulture, out var number)
                || double.IsNaN(number) || double.IsInfinity(number))
            {
                return null;
            }
            return number;
        }

        public static string Format(double number) => number.ToString("0.###", CultureInfo.InvariantCulture);

        private static string Get(Dictionary<string, string> values, string column)
        {
            return values.TryGetValue(column, out var value) ? value.Trim() : string.Empty;
        }

        private static double? ReadPercent(Dictionary<string, string> values, string column, List<string> errors)
        {
            var text = Get(values, column);
            if (text == string.Empty)
            {
                errors.Add($"{column} boş olamaz.");
                return null;
            }

            var number = ParseNumber(text);
            if (number is null)
            {
                errors.Add($"{column} sayısal olmalı (gelen: '{text}').");
                return null;
            }

            if (number < 0 || number > 100)
            {
                errors.Add($"{column} 0-100 aralığında olmalı (gelen: {text}).");
                return null;
            }
            return number;
        }
    }
}
