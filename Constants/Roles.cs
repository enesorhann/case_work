namespace MiniDpp.Constants
{
    // Demo amaçlı görünürlük rolleri. Karbon değerini yalnızca yetkili rol görür.
    public static class Roles
    {
        public const string Consumer = "tuketici";
        public const string Authorized = "yetkili";

        // ?rol=yetkili verildiyse yetkili, aksi her durumda tüketici döner.
        public static string Parse(string? value)
        {
            return string.Equals(value?.Trim(), Authorized, StringComparison.OrdinalIgnoreCase)
                ? Authorized
                : Consumer;
        }

        public static bool CanSeeCarbon(string role) => role == Authorized;
    }
}
