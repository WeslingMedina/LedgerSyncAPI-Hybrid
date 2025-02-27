using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.SignDocument
{
    public class SignElectronicDocumentHandler : IRequestHandler<SignElectronicDocumentCommand, string>
    {
        private readonly IXmlSignerService _signerService;

        public SignElectronicDocumentHandler(IXmlSignerService signerService)
        {
            _signerService = signerService;
        }

        public Task<string> Handle(SignElectronicDocumentCommand request, CancellationToken cancellationToken)
        {
            var signedXml = _signerService.SignXml(
                request.P12Url,
                request.P12Pin,
                request.InXml,
                request.DocumentType);

            return Task.FromResult(signedXml);
        }
    }
}