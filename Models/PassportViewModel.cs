using MiniDpp.Dtos.Passport;

namespace MiniDpp.Models
{
    // Views/Dpp/Passport.cshtml sayfasının modeli
    public class PassportViewModel
    {
        public PassportDto Passport { get; set; } = null!;
        public string JsonLd { get; set; } = string.Empty;
        public bool IsAuthorized { get; set; }
        public string RoleQuery => IsAuthorized ? "?rol=yetkili" : string.Empty;
    }
}
