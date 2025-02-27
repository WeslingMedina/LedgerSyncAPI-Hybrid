using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SigningRequest
    {
        public string P12Url { get; set; } = string.Empty;
        public string PinP12 { get; set; } = string.Empty;
        public string InXml { get; set; } = string.Empty;
        public string TipoDoc { get; set; } = string.Empty;
    }
}
