using Application.Interfaces;
using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class InvoiceSearchService : IInvoiceSearchService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceSearchService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceSearchResponse> GetInvoiceByKeyAsync(InvoiceSearchRequest request)
        {
            if (string.IsNullOrEmpty(request.Clave))
            {
                return new InvoiceSearchResponse
                {
                    Status = 400,
                    Data = "La clave no puede ser en blanco"
                };
            }

            return await _invoiceRepository.GetInvoiceByKeyAsync(request);
        }
    }
}
