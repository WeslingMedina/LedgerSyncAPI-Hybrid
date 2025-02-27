using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptsController : Controller
    {
        private readonly IReceiptSender _receiptSender;

        public ReceiptsController(IReceiptSender receiptSender)
        {
            _receiptSender = receiptSender;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ReceiptSendDto request)
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(request.Token) ||
                string.IsNullOrEmpty(request.Clave) ||
                string.IsNullOrEmpty(request.Fecha) ||
                string.IsNullOrEmpty(request.EmiTipoIdentificacion) ||
                string.IsNullOrEmpty(request.EmiNumeroIdentificacion) ||
                string.IsNullOrEmpty(request.ComprobanteXml) ||
                string.IsNullOrEmpty(request.ClientId))
            {
                return BadRequest("Missing required parameters");
            }

            var result = await _receiptSender.SendReceiptAsync(request);
            return StatusCode(result.Status, result);
        }
    }
}
