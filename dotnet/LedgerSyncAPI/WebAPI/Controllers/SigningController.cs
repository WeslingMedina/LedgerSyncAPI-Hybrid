using Application.DTOs;
using Application.Features.SignDocument;
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
        private readonly IMediator _mediator;
        private readonly IFileRepository _fileRepository;
        private readonly IFileStorage _fileStorage;

        public SigningController(IMediator mediator, IFileRepository fileRepository, IFileStorage fileStorage)
        {
            _mediator = mediator;
            _fileRepository = fileRepository;
            _fileStorage = fileStorage;
        }

        [HttpPost("signFE")]
        public async Task<IActionResult> SignDocument([FromBody] SignRequestDto request)
        {
            var command = new SignElectronicDocumentCommand
            {
                P12Url = _fileStorage.GetFilePath(await _fileRepository.GetByDownloadCodeAsync(request.P12Url)),
                P12Pin = request.P12Pin,
                InXml = request.InXml,
                DocumentType = request.DocumentType
            };

            var signedXml = await _mediator.Send(command);
            return Ok(new { SignedXml = Convert.ToBase64String(Encoding.UTF8.GetBytes(signedXml)) });
        }
    }
}
