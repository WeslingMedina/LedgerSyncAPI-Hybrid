using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IReceiptRepository
    {
        Task EnsureTableExistsAsync();
        Task LogReceiptRequestAsync(ReceiptSenderRequest request, string clientId, int statusCode);
    }
}
