using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace MiniDpp.Helpers
{
    // API yanıtlarında ve veritabanındaki sürüm kayıtlarında kullanılan ortak JSON ayarı:
    // alan adları snake_case (ör. total_rows, model_name), Türkçe karakterler okunur.
    public static class JsonOptions
    {
        public static void Apply(JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        }

        public static readonly JsonSerializerOptions Default = Create();

        private static JsonSerializerOptions Create()
        {
            var options = new JsonSerializerOptions();
            Apply(options);
            return options;
        }
    }
}
