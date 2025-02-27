using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record ReceiptSendDto(
        string Token,
        string Clave,
        string Fecha,
        string EmiTipoIdentificacion,
        string EmiNumeroIdentificacion,
        string? RecpTipoIdentificacion,
        string? RecpNumeroIdentificacion,
        string ComprobanteXml,
        string? CallbackUrl,
        string ClientId
    );
}
