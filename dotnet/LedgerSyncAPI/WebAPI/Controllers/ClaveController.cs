using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaveController : Controller
    {
        private readonly IMediator _mediator;

        public ClaveController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateClave([FromBody] GetClaveQuery request)
        {
            var result = await _mediator.Send(request);
            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(result.Error);
        }
    }
}
