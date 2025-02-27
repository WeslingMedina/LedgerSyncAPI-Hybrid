using Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public record GetClaveQuery(
        string TipoDocumento,
        string TipoCedula,
        string Cedula,
        string Situacion,
        string CodigoPais,
        string Consecutivo,
        string CodigoSeguridad,
        string? Sucursal = null,
        string? Terminal = null
    ) : IRequest<Result<ClaveResponse>>; // Add interface implementation here
}
