using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using MiniDpp.Dtos.Passport;

namespace MiniDpp.Helpers
{
    // PassportDto -> schema.org/Product JSON-LD.
    // HTML sayfası da aynı çıktıyı sayfaya gömer; böylece iki çıktı birebir tutarlıdır.
    // Karbon alanı yalnızca PassportDto içinde değer varsa (yetkili rol) eklenir.
    public static class JsonLdBuilder
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // Türkçe karakterler okunur kalır; <, >, & yine kaçırılır (HTML'e gömmek için güvenli).
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        public static ProductJsonLdDto Build(PassportDto passport, string baseUrl)
        {
            var pageUrl = $"{baseUrl.TrimEnd('/')}/dpp/{passport.Gtin}";

            var properties = new List<PropertyValueDto>
            {
                Property("elyaf_pamuk_yuzde", "Pamuk oranı", passport.CottonPercent, "%"),
                Property("elyaf_elastan_yuzde", "Elastan oranı", passport.ElastanePercent, "%")
            };

            foreach (var batch in passport.Batches)
            {
                properties.Add(Property("batch_no", "Batch/Lot numarası", batch.BatchNumber));
                properties.Add(Property("uretim_yeri", $"Üretim yeri ({batch.BatchNumber})", batch.ProductionPlace));
            }

            if (passport.ShowCarbon)
            {
                properties.Add(Property("karbon_kg_co2e", "Karbon ayak izi", passport.CarbonKgCo2e!.Value, "kg CO2e"));
            }

            properties.Add(Property("dpp_surum", "DPP sürümü", passport.Version));

            return new ProductJsonLdDto
            {
                Id = pageUrl,
                Url = pageUrl,
                Name = passport.DisplayName,
                Gtin14 = passport.Gtin,
                Model = new ProductModelJsonLdDto { Name = passport.ModelName },
                Material = $"Pamuk %{ProductRowValidator.Format(passport.CottonPercent)}, Elastan %{ProductRowValidator.Format(passport.ElastanePercent)}",
                AdditionalProperty = properties,
                DateModified = passport.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        public static string Serialize(ProductJsonLdDto jsonLd) => JsonSerializer.Serialize(jsonLd, SerializerOptions);

        private static PropertyValueDto Property(string id, string name, object value, string? unit = null)
        {
            return new PropertyValueDto { PropertyId = id, Name = name, Value = value, UnitText = unit };
        }
    }
}
