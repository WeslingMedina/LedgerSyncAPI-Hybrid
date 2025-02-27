using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.GenerateXml
{
    public record InvoiceResponse(
        string Clave,
        string Xml
    );
}
