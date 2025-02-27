using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SignRequestDto
    {
        public string P12Url { get; set; }
        public string P12Pin { get; set; }
        public string InXml { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}
