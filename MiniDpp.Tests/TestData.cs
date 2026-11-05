using System.Net.Http.Headers;

namespace MiniDpp.Tests
{
    public static class TestData
    {
        public static string DataFolder
        {
            get
            {
                var folder = new DirectoryInfo(AppContext.BaseDirectory);
                while (folder != null && !Directory.Exists(Path.Combine(folder.FullName, "Data")))
                {
                    folder = folder.Parent;
                }
                return Path.Combine(folder!.FullName, "Data");
            }
        }

        public static string ReadCsv(string fileName) => File.ReadAllText(Path.Combine(DataFolder, fileName));

        public static async Task<HttpResponseMessage> UploadAsync(HttpClient client, string csvText, string url = "/api/import")
        {
            var content = new MultipartFormDataContent();
            var file = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(csvText));
            file.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            content.Add(file, "file", "urunler.csv");
            return await client.PostAsync(url, content);
        }
    }
}
