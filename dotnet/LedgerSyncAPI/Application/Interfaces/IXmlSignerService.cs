using Application.DTOs;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IXmlSignerService
    {
        string SignXml(string P12Url, string pin, string xmlContent, DocumentType documentType);
    }
}
