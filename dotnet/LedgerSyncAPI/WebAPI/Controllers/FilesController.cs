using Application.Features.Files;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IFileRepository _fileRepository;
        private readonly IFileStorage _fileStorage;

        public FilesController(IMediator mediator, IFileRepository fileRepository, IFileStorage fileStorage)
        {
            _mediator = mediator;
            _fileRepository = fileRepository;
            _fileStorage = fileStorage;
        }

        [HttpPost("upload-cert")]
        public async Task<IActionResult> UploadCert(
            [FromForm] IFormFile fileToUpload,
            [FromHeader] string sessionKey,
            [FromQuery] string iam)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _mediator.Send(new UploadCertCommand
            {
                File = fileToUpload,
                UserId = userId,
                Iam = iam
            });

            return Ok(result);
        }

        [HttpGet("download/{code}")]
        public async Task<IActionResult> DownloadFile(string code)
        {
            var file = await _fileRepository.GetByDownloadCodeAsync(code);
            if (file == null) return NotFound();

            var filePath = _fileStorage.GetFilePath(file);
            return PhysicalFile(Path.GetFullPath(filePath), "application/octet-stream", file.Name);
        }
    }
}
