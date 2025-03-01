using Application.DTOs;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SigningController : Controller
    {
        private readonly IXmlSignerService _signService;
        private readonly ILogger<SigningController> _logger;

        public SigningController(IXmlSignerService signService, ILogger<SigningController> logger)
        {
            _signService = signService;
            _logger = logger;
        }

        [HttpPost("signFE")]
        public async Task<IActionResult> SignDocument([FromBody] SigningRequest request)
        {
            try
            {
                var result = await _signService.SignDocument(request);
                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Error signing document");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
