using MiniDpp.Interfaces;

namespace MiniDpp.Services.Storage
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _folder;

        public LocalFileStorage(string folder)
        {
            _folder = folder;
            Directory.CreateDirectory(_folder);
        }

        public async Task SaveAsync(string name, byte[] data)
        {
            var path = Path.Combine(_folder, name);
            var tempPath = path + ".tmp";
            await File.WriteAllBytesAsync(tempPath, data);
            File.Move(tempPath, path, overwrite: true); // yarım dosya okunmasın
        }

        public async Task<byte[]?> ReadAsync(string name)
        {
            var path = Path.Combine(_folder, name);
            return File.Exists(path) ? await File.ReadAllBytesAsync(path) : null;
        }
    }
}
