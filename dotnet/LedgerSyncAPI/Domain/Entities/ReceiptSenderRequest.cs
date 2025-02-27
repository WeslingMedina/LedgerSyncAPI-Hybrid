using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ReceiptSenderRequest
    {
        public string clave { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public Emisor emisor { get; set; } = new();
        public Receptor? receptor { get; set; }
        public string comprobanteXml { get; set; } = string.Empty;
        public string? callbackUrl { get; set; }
    }
}
