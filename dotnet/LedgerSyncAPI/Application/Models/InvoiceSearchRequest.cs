using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class InvoiceSearchRequest
    {
        public string Clave { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
    }
}
