namespace MiniDpp.Interfaces
{
    public interface IFileStorage
    {
        Task SaveAsync(string name, byte[] data);
        Task<byte[]?> ReadAsync(string name);
    }
}
