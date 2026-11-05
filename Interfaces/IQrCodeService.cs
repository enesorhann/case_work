namespace MiniDpp.Interfaces
{
    public interface IQrCodeService
    {
        byte[] CreatePng(string text);
    }
}
