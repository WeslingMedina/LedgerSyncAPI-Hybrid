using Application.Features.GenerateXml;
using Application.Interfaces;
using Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : Controller
    {
        private readonly GenerateXmlFeUseCase _useCase;
        private readonly IInvoiceSearchService _invoiceSearchService;

        public InvoiceController(GenerateXmlFeUseCase useCase, IInvoiceSearchService invoiceSearchService)
        {
            _useCase = useCase;
            _invoiceSearchService = invoiceSearchService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] InvoiceRequest request)
        {
            try
            {
                var result = await _useCase.Execute(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
        }

        [HttpGet("consultar")]
        public async Task<IActionResult> ConsultarComprobante(
            [FromQuery] string clave,
            [FromQuery] string token,
            [FromQuery] string client_id)
        {
            var request = new InvoiceSearchRequest
            {
                Clave = clave,
                Token = token,
                ClientId = client_id
            };

            var result = await _invoiceSearchService.GetInvoiceByKeyAsync(request);

            if (result.Status >= 400)
            {
                return StatusCode(result.Status, result.Data);
            }

            return Ok(result.Data);
        }
    }
}
