using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/token")]
    public class TokenController : Controller
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> GetToken([FromBody] TokenRequest request)
        {
            var result = await _tokenService.GetTokenAsync(request);
            return result.Status == 200 ? Ok(result) : BadRequest(result);
        }
    }
}
