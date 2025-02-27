using Application.DTOs;
using Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.GenerateClave
{
    public class GetClaveQueryHandler : IRequestHandler<GetClaveQuery, Result<ClaveResponse>>
    {
        public async Task<Result<ClaveResponse>> Handle(GetClaveQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Procesar Parámetros
                var currentDate = DateTime.Now;
                string tipoDocumentoCode = GetTipoDocumentoCode(request.TipoDocumento);
                string identificacion = ProcessCedula(request.TipoCedula, request.Cedula);
                string situacionCode = GetSituacionCode(request.Situacion);
                string sucursal = ProcessSucursal(request.Sucursal);
                string terminal = ProcessTerminal(request.Terminal);
                string consecutivoPadded = request.Consecutivo.PadLeft(10, '0');
                string codigoSeguridadPadded = request.CodigoSeguridad.PadLeft(8, '0');

                // 2. Generar Clave
                string consecutivoFinal = $"{sucursal}{terminal}{tipoDocumentoCode}{consecutivoPadded}";
                string clave = $"{request.CodigoPais}{currentDate:ddMMyy}{identificacion}{consecutivoFinal}{situacionCode}{codigoSeguridadPadded}";

                return Result.Success(new ClaveResponse(clave, consecutivoFinal, clave.Length));
            }
            catch (Exception ex)
            {
                return Result.Failure<ClaveResponse>(new Error("ClaveGeneration.Error", ex.Message));
            }
        }

        private string GetTipoDocumentoCode(string tipoDocumento) => tipoDocumento switch
        {
            "FE" => "01",
            "ND" => "02",
            "NC" => "03",
            "TE" => "04",
            "CCE" => "05",
            "CPCE" => "06",
            "RCE" => "07",
            "FEC" => "08",
            "FEE" => "09",
            _ => throw new ArgumentException("TipoDocumento no válido")
        };

        private string ProcessCedula(string tipoCedula, string cedula) => tipoCedula.ToLower() switch
        {
            "fisico" or "01" => cedula.PadLeft(12, '0'),
            "juridico" or "02" => cedula.Length > 12 ? throw new ArgumentException("Cédula Jurídica inválida") : cedula.PadLeft(12, '0'),
            "dimex" or "03" => cedula.Length > 12 ? throw new ArgumentException("DIMEX inválido") : cedula.PadLeft(12, '0'),
            "nite" or "04" => cedula.PadLeft(12, '0'),
            _ => throw new ArgumentException("TipoCedula no válido")
        };

        private string GetSituacionCode(string situacion) => situacion.ToLower() switch
        {
            "normal" => "1",
            "contingencia" => "2",
            "sininternet" => "3",
            _ => throw new ArgumentException("Situación no válida")
        };

        private string ProcessSucursal(string? sucursal)
            => string.IsNullOrEmpty(sucursal) ? "001" : sucursal.PadLeft(3, '0');

        private string ProcessTerminal(string? terminal)
            => string.IsNullOrEmpty(terminal) ? "00001" : terminal.PadLeft(5, '0');
    }
}
