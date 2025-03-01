using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SigningResponse
    {
        public Respuesta Resp { get; set; }

        public class Respuesta
        {
            public string XmlFirmado { get; set; }
        }
    }
}
