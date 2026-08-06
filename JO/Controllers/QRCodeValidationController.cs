using JO.Models;
using JO.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JO.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeValidationController : ControllerBase
    {
        private readonly IQRCodeService _qrCodeService;

        public QRCodeValidationController(IQRCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        [HttpGet("validate")]
        public async Task<IActionResult> Validate([FromQuery] string qrCode)
        {
            if (string.IsNullOrWhiteSpace(qrCode))
            {
                return BadRequest("Le code QR ne peut pas être vide.");
            }

            var result = await _qrCodeService.ValidateQRCodeAsync(qrCode);

            return result switch
            {
                QRCodeValidationResult.Valid => Ok("✅ Le code QR est valide."),
                QRCodeValidationResult.AlreadyScanned => Conflict("⚠️ Ce code QR a déjà été scanné."),
                QRCodeValidationResult.NotFound => NotFound("❌ Le code QR est invalide ou introuvable."),
                _ => StatusCode(500, "⚠️ Une erreur inconnue est survenue lors de la validation du code QR.")
            };
        }
    }
}
