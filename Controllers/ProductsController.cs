using MediatR;
using Microsoft.AspNetCore.Mvc;
using MiniDpp.Dtos.Product;
using MiniDpp.Services.Product.Queries;

namespace MiniDpp.Controllers
{
    /// <summary>Ürün listesi ve sürüm geçmişi API'si.</summary>
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Kayıtlı tüm ürünlerin kısa listesini döner.</summary>
        /// <remarks>Ürünler modelleriyle birlikte, GTIN sırasına göre gelir. Ürün adı boşsa model adı kullanılır.</remarks>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetProductsQuery());
            return Ok(result);
        }

        /// <summary>Ürünün önceki sürümlerini döner (bonus).</summary>
        /// <remarks>
        /// Ürün her güncellendiğinde değişiklikten önceki hali sürüm numarası ve tarihle saklanır.
        /// Ürün hiç güncellenmediyse boş liste döner.
        /// </remarks>
        /// <param name="gtin">14 haneli GTIN</param>
        /// <response code="200">Sürüm listesi</response>
        /// <response code="404">Ürün bulunamadı</response>
        [HttpGet("{gtin}/versions")]
        [ProducesResponseType(typeof(List<ProductVersionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVersions([FromRoute] string gtin)
        {
            var result = await _mediator.Send(new GetProductVersionsQuery { Gtin = gtin });
            if (result == null)
            {
                return Problem(detail: $"GTIN {gtin} için ürün bulunamadı.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(result);
        }
    }
}
