using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class InvoiceSearchResponse
    {
        public int Status { get; set; }
        public string? To { get; set; }
        public object? Data { get; set; }
    }
}
