using MiniDpp.Constants;

namespace MiniDpp.Dtos.Passport
{
    // HTML ve JSON-LD görünümleri bu tek nesneden üretilir; böylece iki çıktı tutarlı kalır.
    public class PassportDto
    {
        public string Gtin { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ModelName { get; set; } = string.Empty;
        public double CottonPercent { get; set; }
        public double ElastanePercent { get; set; }
        public double? CarbonKgCo2e { get; set; }
        public List<BatchDto> Batches { get; set; } = new();
        public int Version { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Role { get; set; } = Roles.Consumer;

        public string DisplayName => string.IsNullOrEmpty(ProductName) ? ModelName : ProductName;
        public bool ShowCarbon => CarbonKgCo2e.HasValue;

        // Rol ayrımı tek yerde: tüketici rolünde karbon değeri gizlenir.
        public PassportDto ForRole(string role)
        {
            var copy = (PassportDto)MemberwiseClone();
            copy.Role = role;
            copy.CarbonKgCo2e = Roles.CanSeeCarbon(role) ? CarbonKgCo2e : null;
            return copy;
        }
    }
}
