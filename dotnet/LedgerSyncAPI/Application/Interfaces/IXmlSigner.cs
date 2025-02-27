using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IXmlSigner
    {
        Task<SigningResult> SignXmlAsync(string certificatePath, string certificatePassword, string xmlContent, string documentType);
    }
}
