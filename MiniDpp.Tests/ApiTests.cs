using System.Net;
using System.Text.Json;

namespace MiniDpp.Tests
{
    public class ApiTests : IDisposable
    {
        private const string Gtin = "08690000000012";
        private readonly DppAppFactory _factory = new();
        private readonly HttpClient _client;

        public ApiTests()
        {
            _client = _factory.CreateClient();
        }

        public void Dispose() => _factory.Dispose();

        private static async Task<JsonElement> JsonAsync(HttpResponseMessage response)
        {
            return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        }

        private Task<HttpResponseMessage> UploadAssignmentCsvAsync() =>
            TestData.UploadAsync(_client, TestData.ReadCsv("urunler_duzeltilmis_test.csv"));

        [Fact]
        public async Task AssignmentCsv_SavesValidRows_AndReportsErrorsWithLineNumbers()
        {
            var body = await JsonAsync(await UploadAssignmentCsvAsync());

            Assert.Equal(5, body.GetProperty("total_rows").GetInt32());
            Assert.Equal(new[] { 2, 3, 6 }, body.GetProperty("saved").EnumerateArray().Select(s => s.GetProperty("line").GetInt32()));
            Assert.Equal(new[] { "08690000000012", "08690000000029", "08690000000050" },
                body.GetProperty("saved").EnumerateArray().Select(s => s.GetProperty("gtin").GetString()));

            var errors = body.GetProperty("errors").EnumerateArray().ToList();
            Assert.Equal(4, errors[0].GetProperty("line").GetInt32());
            Assert.Equal("Wide Leg Denim 503", errors[0].GetProperty("model_name").GetString());
            Assert.Equal("Slim Denim 501", body.GetProperty("saved")[0].GetProperty("model_name").GetString());
            Assert.Equal("GTIN boş olamaz.", errors[0].GetProperty("errors")[0].GetString());
            Assert.Equal(5, errors[1].GetProperty("line").GetInt32());
            Assert.Equal("Elyaf yüzdelerinin toplamı 100 olmalı (gelen: 98).", errors[1].GetProperty("errors")[0].GetString());

            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/dpp/{Gtin}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/dpp/08690000000043")).StatusCode);
        }

        [Fact]
        public async Task InvalidSampleCsv_ReportsEveryBadRow()
        {
            var body = await JsonAsync(await TestData.UploadAsync(_client, TestData.ReadCsv("urunler_hatali_ornek.csv")));
            Assert.Equal(new[] { 2, 8 }, body.GetProperty("saved").EnumerateArray().Select(s => s.GetProperty("line").GetInt32()));
            Assert.Equal(new[] { 3, 4, 5, 6, 7 }, body.GetProperty("errors").EnumerateArray().Select(e => e.GetProperty("line").GetInt32()));
        }

        [Fact]
        public async Task MissingColumns_Returns400()
        {
            var response = await TestData.UploadAsync(_client, "gtin,model_adi\n08691234500019,X\n");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("eksik", (await JsonAsync(response)).GetProperty("detail").GetString());
        }

        [Fact]
        public async Task DuplicateGtinInSameFile_IsAnError()
        {
            var text = TestData.ReadCsv("urunler_ornek.csv");
            var body = await JsonAsync(await TestData.UploadAsync(_client, text + text.Split('\n')[1] + "\n"));
            var error = body.GetProperty("errors")[0];
            Assert.Equal(7, error.GetProperty("line").GetInt32());
            Assert.Contains("2. satırda", error.GetProperty("errors")[0].GetString());
        }

        [Fact]
        public async Task Carbon_IsHiddenForConsumer_InHtmlAndJsonLd()
        {
            await UploadAssignmentCsvAsync();

            var html = await _client.GetStringAsync($"/dpp/{Gtin}");
            var jsonLd = await _client.GetStringAsync($"/dpp/{Gtin}.jsonld");

            // Kilitli "Karbon ayak izi" kutusu görünür ama değer (12.4) sayfada hiçbir yerde yoktur.
            Assert.Contains("yalnızca yetkili görünümde", html);
            Assert.DoesNotContain("12.4", html);
            Assert.DoesNotContain("karbon_kg_co2e", jsonLd);
            var root = JsonDocument.Parse(jsonLd).RootElement;
            Assert.Equal("Product", root.GetProperty("@type").GetString());
            Assert.Equal(Gtin, root.GetProperty("gtin14").GetString());
        }

        [Fact]
        public async Task Carbon_IsVisibleForAuthorized_InHtmlAndJsonLd()
        {
            await UploadAssignmentCsvAsync();

            var html = await _client.GetStringAsync($"/dpp/{Gtin}?rol=yetkili");
            var jsonLd = JsonDocument.Parse(await _client.GetStringAsync($"/dpp/{Gtin}.jsonld?rol=yetkili")).RootElement;

            Assert.Contains("12.4 <small>kg CO₂e</small>", html);
            var carbon = jsonLd.GetProperty("additionalProperty").EnumerateArray()
                .Single(p => p.GetProperty("propertyID").GetString() == "karbon_kg_co2e");
            Assert.Equal(12.4, carbon.GetProperty("value").GetDouble());
        }

        [Fact]
        public async Task JsonLd_HasLdJsonContentType()
        {
            await UploadAssignmentCsvAsync();
            var response = await _client.GetAsync($"/dpp/{Gtin}.jsonld");
            Assert.Equal("application/ld+json", response.Content.Headers.ContentType!.MediaType);
        }

        [Fact]
        public async Task QrCode_IsPng_And404ForUnknownProduct()
        {
            await UploadAssignmentCsvAsync();
            var response = await _client.GetAsync($"/dpp/{Gtin}/qr.png");
            Assert.Equal("image/png", response.Content.Headers.ContentType!.MediaType);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.Equal(new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G' }, bytes.Take(4));
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/dpp/00000000000000/qr.png")).StatusCode);
        }

        [Fact]
        public async Task Update_CreatesVersion_AndRefreshesCache()
        {
            await UploadAssignmentCsvAsync();
            await _client.GetStringAsync($"/dpp/{Gtin}.jsonld?rol=yetkili"); // önbelleğe al

            var changed = TestData.ReadCsv("urunler_duzeltilmis_test.csv").Replace(",12.4", ",9.9");
            var body = await JsonAsync(await TestData.UploadAsync(_client, changed));
            Assert.Equal("updated", body.GetProperty("saved")[0].GetProperty("status").GetString());
            Assert.Equal("unchanged", body.GetProperty("saved")[1].GetProperty("status").GetString());

            var versions = await JsonAsync(await _client.GetAsync($"/api/products/{Gtin}/versions"));
            Assert.Equal(1, versions[0].GetProperty("version_number").GetInt32());
            Assert.Equal(12.4, versions[0].GetProperty("data").GetProperty("carbon_kg_co2e").GetDouble());

            var jsonLd = await _client.GetStringAsync($"/dpp/{Gtin}.jsonld?rol=yetkili");
            Assert.Contains("9.9", jsonLd);
            Assert.Contains("\"propertyID\": \"dpp_surum\"", jsonLd);
            Assert.Contains("Sürüm 2", await _client.GetStringAsync($"/dpp/{Gtin}?rol=yetkili"));
        }

        [Fact]
        public async Task BackgroundImportJob_FinishesWithResult()
        {
            var response = await TestData.UploadAsync(_client, TestData.ReadCsv("urunler_duzeltilmis_test.csv"), "/api/import/jobs");
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            var statusUrl = (await JsonAsync(response)).GetProperty("status_url").GetString();
            var job = await JsonAsync(await _client.GetAsync(statusUrl));
            Assert.Equal("SUCCESS", job.GetProperty("status").GetString());
            Assert.Equal(3, job.GetProperty("result").GetProperty("saved_count").GetInt32());
        }

        [Fact]
        public async Task ProductList_ReturnsSavedProducts()
        {
            await UploadAssignmentCsvAsync();
            var products = await JsonAsync(await _client.GetAsync("/api/products"));
            Assert.Equal(3, products.GetArrayLength());
            Assert.Equal("Slim Denim 501", products[0].GetProperty("model_name").GetString());
            Assert.Equal(98, products[0].GetProperty("cotton_percent").GetDouble());
            Assert.Equal("B2027-01", products[0].GetProperty("batches")[0].GetProperty("batch_number").GetString());
            Assert.Equal("Denizli", products[0].GetProperty("batches")[0].GetProperty("production_place").GetString());
        }
        [Fact]
        public async Task UnknownProduct_Returns404_ForEveryDppOutput()
        {
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/dpp/00000000000000")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/dpp/00000000000000.jsonld")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/dpp/00000000000000/qr.png")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/api/products/00000000000000/versions")).StatusCode);
        }

        [Fact]
        public async Task ImportWithoutFile_Returns400()
        {
            var response = await _client.PostAsync("/api/import", new MultipartFormDataContent());
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UnknownJob_IsPending()
        {
            var job = await JsonAsync(await _client.GetAsync("/api/import/jobs/bilinmeyen"));
            Assert.Equal("PENDING", job.GetProperty("status").GetString());
        }

        [Fact]
        public async Task Swagger_ContainsEndpointDescriptions()
        {
            var swagger = await _client.GetStringAsync("/swagger/v1/swagger.json");
            Assert.Contains("CSV'yi anında içe aktarır.", swagger);
            Assert.Contains("/dpp/{gtin}.jsonld", swagger);
        }
    }
}
