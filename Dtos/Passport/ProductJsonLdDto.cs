using System.Text.Json.Serialization;

namespace MiniDpp.Dtos.Passport
{
    // schema.org/Product yapısındaki JSON-LD çıktısı. Alan adları schema.org ile birebir aynıdır.
    public class ProductJsonLdDto
    {
        [JsonPropertyName("@context")] public string Context { get; set; } = "https://schema.org";
        [JsonPropertyName("@type")] public string Type { get; set; } = "Product";
        [JsonPropertyName("@id")] public string Id { get; set; } = string.Empty;
        [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("gtin14")] public string Gtin14 { get; set; } = string.Empty;
        [JsonPropertyName("model")] public ProductModelJsonLdDto Model { get; set; } = new();
        [JsonPropertyName("material")] public string Material { get; set; } = string.Empty;
        [JsonPropertyName("additionalProperty")] public List<PropertyValueDto> AdditionalProperty { get; set; } = new();
        [JsonPropertyName("dateModified")] public string DateModified { get; set; } = string.Empty;
    }

    public class ProductModelJsonLdDto
    {
        [JsonPropertyName("@type")] public string Type { get; set; } = "ProductModel";
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    }

    public class PropertyValueDto
    {
        [JsonPropertyName("@type")] public string Type { get; set; } = "PropertyValue";
        [JsonPropertyName("propertyID")] public string PropertyId { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("value")] public object Value { get; set; } = string.Empty;
        [JsonPropertyName("unitText")] public string? UnitText { get; set; }
    }
}
