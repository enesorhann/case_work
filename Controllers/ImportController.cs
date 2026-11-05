using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MiniDpp.Dtos.Import;
using MiniDpp.Exceptions;
using MiniDpp.Helpers;
using MiniDpp.Models;
using MiniDpp.Services.Import.Commands;
using MiniDpp.Services.Import.Queries;

namespace MiniDpp.Controllers
{
    /// <summary>CSV içe aktarma API'si: anında ya da RabbitMQ ile arka planda.</summary>
    [Route("api/import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly DppSettings _settings;

        public ImportController(IMediator mediator, IOptions<DppSettings> settings)
        {
            _mediator = mediator;
            _settings = settings.Value;
        }

        /// <summary>CSV'yi anında içe aktarır.</summary>
        /// <remarks>
        /// Her satırı doğrular; geçerli satırları tek transaction'da kaydeder, hatalı satırları satır numarası
        /// (başlık = 1. satır) ve nedenleriyle döner. Hatalı satırlar diğerlerinin kaydını engellemez.
        /// Kaydedilen her ürün için Redis önbelleği silinir ve QR kod üretimi RabbitMQ'ya gönderilir.
        /// </remarks>
        /// <param name="file">CSV dosyası (UTF-8 ya da Windows-1254, ayraç , veya ;)</param>
        /// <response code="200">Kaydedilen satırlar ve satır numaralı hatalar</response>
        /// <response code="400">Dosya boş ya da zorunlu sütun eksik</response>
        /// <response code="413">Dosya 5 MB'tan büyük</response>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file.Length > _settings.MaxUploadBytes)
            {
                return Problem(detail: "Dosya çok büyük.", statusCode: StatusCodes.Status413PayloadTooLarge);
            }

            try
            {
                var result = await _mediator.Send(new ImportProductsCommand { CsvText = await ReadAsync(file) });
                return Ok(result);
            }
            catch (CsvFormatException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        /// <summary>CSV'yi arka planda içe aktarmak için RabbitMQ kuyruğuna koyar.</summary>
        /// <remarks>
        /// İş durumunu PENDING olarak Redis'e yazar, ImportRequestedEvent yayınlar ve hemen bir job_id döner.
        /// Sonuç GET /api/import/jobs/{jobId} ile sorgulanır. Dosya biçimi hataları burada değil,
        /// iş durumunda FAILURE olarak görünür.
        /// </remarks>
        /// <param name="file">CSV dosyası</param>
        /// <response code="202">İş kuyruğa alındı; Location başlığı durum adresidir</response>
        /// <response code="413">Dosya 5 MB'tan büyük</response>
        [HttpPost("jobs")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImportJobCreatedDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        public async Task<IActionResult> StartJob(IFormFile file)
        {
            if (file.Length > _settings.MaxUploadBytes)
            {
                return Problem(detail: "Dosya çok büyük.", statusCode: StatusCodes.Status413PayloadTooLarge);
            }

            var result = await _mediator.Send(new StartImportJobCommand { CsvText = await ReadAsync(file) });
            return Accepted(result.StatusUrl, result);
        }

        /// <summary>Arka plan içe aktarma işinin durumunu ve sonucunu döner.</summary>
        /// <remarks>
        /// Durumlar: PENDING (kuyrukta ya da bilinmeyen numara), STARTED, SUCCESS (result dolu),
        /// FAILURE (error dolu). Kayıtlar Redis'te 1 saat tutulur.
        /// </remarks>
        /// <param name="jobId">POST /api/import/jobs yanıtındaki job_id</param>
        [HttpGet("jobs/{jobId}")]
        [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJob([FromRoute] string jobId)
        {
            var result = await _mediator.Send(new GetImportJobQuery { JobId = jobId });
            return Ok(result);
        }

        private static async Task<string> ReadAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            return CsvFileReader.Decode(stream.ToArray());
        }
    }
}
