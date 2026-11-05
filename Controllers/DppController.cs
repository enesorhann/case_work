using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MiniDpp.Constants;
using MiniDpp.Dtos.Passport;
using MiniDpp.Helpers;
using MiniDpp.Models;
using MiniDpp.Services.Passport.Queries;
using MiniDpp.Services.QrCode.Commands;
using MiniDpp.Services.QrCode.Queries;

namespace MiniDpp.Controllers
{
    /// <summary>Dijital Ürün Pasaportu çıktıları: HTML (Razor View), JSON-LD ve QR kod.</summary>
    /// <remarks>HTML ve JSON-LD aynı Query'den ve aynı JsonLdBuilder'dan beslenir; rol kuralı ikisinde de aynıdır.</remarks>
    public class DppController : Controller
    {
        private readonly IMediator _mediator;
        private readonly DppSettings _settings;

        public DppController(IMediator mediator, IOptions<DppSettings> settings)
        {
            _mediator = mediator;
            _settings = settings.Value;
        }

        /// <summary>İnsanların okuyacağı HTML ürün pasaportu (QR kod bu adrese yönlenir).</summary>
        /// <remarks>
        /// Varsayılan tüketici görünümünde karbon değeri gösterilmez; ?rol=yetkili ile gösterilir.
        /// Sayfa aynı JSON-LD'yi script etiketi içinde de taşır.
        /// </remarks>
        /// <param name="gtin">14 haneli GTIN</param>
        /// <param name="rol">"yetkili" ise karbon değeri gösterilir</param>
        /// <response code="200">HTML sayfa</response>
        /// <response code="404">Ürün bulunamadı</response>
        [HttpGet("dpp/{gtin}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Passport([FromRoute] string gtin, [FromQuery] string? rol)
        {
            var passport = await GetPassportAsync(gtin, rol);
            if (passport == null)
            {
                return NotFound($"GTIN {gtin} için ürün bulunamadı.");
            }

            var model = new PassportViewModel
            {
                Passport = passport,
                JsonLd = JsonLdBuilder.Serialize(JsonLdBuilder.Build(passport, _settings.BaseUrl)),
                IsAuthorized = passport.Role == Roles.Authorized
            };
            return View(model);
        }

        /// <summary>Makine tarafından okunabilir pasaport (schema.org/Product JSON-LD).</summary>
        /// <remarks>Karbon değeri yalnızca ?rol=yetkili ile additionalProperty listesine eklenir.</remarks>
        /// <param name="gtin">14 haneli GTIN</param>
        /// <param name="rol">"yetkili" ise karbon değeri eklenir</param>
        /// <response code="200">application/ld+json</response>
        /// <response code="404">Ürün bulunamadı</response>
        [HttpGet("dpp/{gtin}.jsonld")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JsonLd([FromRoute] string gtin, [FromQuery] string? rol)
        {
            var passport = await GetPassportAsync(gtin, rol);
            if (passport == null)
            {
                return NotFound($"GTIN {gtin} için ürün bulunamadı.");
            }

            var json = JsonLdBuilder.Serialize(JsonLdBuilder.Build(passport, _settings.BaseUrl));
            return Content(json, "application/ld+json; charset=utf-8");
        }

        /// <summary>Ürünün HTML pasaport adresine yönlenen QR kod (PNG).</summary>
        /// <remarks>QR kod normalde içe aktarmadan sonra arka planda üretilir; henüz yoksa bu istekte üretilir.</remarks>
        /// <param name="gtin">14 haneli GTIN</param>
        /// <response code="200">image/png</response>
        /// <response code="404">Ürün bulunamadı</response>
        [HttpGet("dpp/{gtin}/qr.png")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> QrCode([FromRoute] string gtin)
        {
            var png = await _mediator.Send(new GetQrCodeQuery { Gtin = gtin });
            if (png == null)
            {
                // Arka plan işi henüz bitmediyse QR kod burada üretilir.
                if (!await _mediator.Send(new GenerateQrCodeCommand { Gtin = gtin }))
                {
                    return NotFound($"GTIN {gtin} için ürün bulunamadı.");
                }
                png = await _mediator.Send(new GetQrCodeQuery { Gtin = gtin });
            }
            return File(png!, "image/png");
        }

        private Task<PassportDto?> GetPassportAsync(string gtin, string? rol)
        {
            return _mediator.Send(new GetPassportQuery { Gtin = gtin, Role = Roles.Parse(rol) });
        }
    }
}
