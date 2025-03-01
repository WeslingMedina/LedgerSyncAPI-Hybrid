using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SigningRequest
    {
        public required string Class { get; set; }
        public required string Method { get; set; }
        public required string P12Url { get; set; }
        public required string InXml { get; set; }
        public required string PinP12  { get; set; }
        public required string DocType { get; set; }
    }
}
