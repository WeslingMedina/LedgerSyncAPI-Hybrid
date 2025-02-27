using Domain.Constants;
using Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.GenerateXml
{
    public class GenerateXmlFeValidator : AbstractValidator<InvoiceRequest>
    {
        public GenerateXmlFeValidator()
        {
            // Validaciones generales
            RuleFor(x => x.Clave)
                .NotEmpty().WithMessage("Clave es requerida")
                .Length(50).WithMessage("Clave debe tener 50 caracteres");

            RuleFor(x => x.CodigoActividad)
                .NotEmpty().WithMessage("Código actividad es requerido")
                .Length(InvoiceConstants.CODIGO_ACTIVIDAD_SIZE)
                .WithMessage($"Código actividad debe tener {InvoiceConstants.CODIGO_ACTIVIDAD_SIZE} dígitos");

            RuleFor(x => x.Consecutivo)
                .NotEmpty().WithMessage("Consecutivo es requerido")
                .MaximumLength(20).WithMessage("Consecutivo máximo 20 caracteres");

            RuleFor(x => x.FechaEmision)
                .NotEmpty().WithMessage("Fecha emisión es requerida")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Fecha emisión no puede ser futura");

            // Validaciones Emisor
            RuleFor(x => x.Emisor.Nombre)
                .NotEmpty().WithMessage("Nombre emisor es requerido")
                .MaximumLength(InvoiceConstants.EMISOR_NOMBRE_MAX)
                .WithMessage($"Nombre emisor máximo {InvoiceConstants.EMISOR_NOMBRE_MAX} caracteres");

            RuleFor(x => x.Emisor.TipoIdentif)
                .NotEmpty().WithMessage("Tipo identificación emisor es requerido")
                .Must(BeValidTipoIdentificacion).WithMessage("Tipo identificación inválido. Valores permitidos: 01, 02, 03, 04, 05");

            RuleFor(x => x.Emisor.NumIdentif)
                .NotEmpty().WithMessage("Número identificación emisor es requerido")
                .MaximumLength(12).WithMessage("Número identificación emisor máximo 12 dígitos");

            RuleFor(x => x.Emisor.Email)
                .NotEmpty().WithMessage("Email emisor es requerido")
                .EmailAddress().WithMessage("Formato email emisor inválido");

            // Validaciones Receptor
            RuleFor(x => x.Receptor.Nombre)
                .NotEmpty().WithMessage("Nombre receptor es requerido")
                .MaximumLength(InvoiceConstants.RECEPTOR_NOMBRE_MAX)
                .WithMessage($"Nombre receptor máximo {InvoiceConstants.RECEPTOR_NOMBRE_MAX} caracteres");

            When(x => x.Receptor.TipoIdentif != "05", () =>
            {
                RuleFor(x => x.Receptor.NumIdentif)
                    .NotEmpty().WithMessage("Número identificación receptor es requerido")
                    .MaximumLength(12).WithMessage("Número identificación receptor máximo 12 dígitos");
            });

            RuleFor(x => x.Receptor.OtrasSenasExtranjero)
                .MaximumLength(InvoiceConstants.RECEPTOR_OTRAS_SENAS_MAX)
                .When(x => !string.IsNullOrEmpty(x.Receptor.OtrasSenasExtranjero));

            // Validaciones Detalles
            RuleFor(x => x.Detalles)
                .NotEmpty().WithMessage("Debe haber al menos un detalle")
                .Must(d => d.Count <= 15).WithMessage("Máximo 15 detalles");

            RuleForEach(x => x.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(d => d.Cantidad)
                    .GreaterThan(0).WithMessage("Cantidad debe ser mayor a 0");

                detalle.RuleFor(d => d.Impuestos)
                    .NotEmpty().WithMessage("Detalle debe tener al menos un impuesto");
            });

            // Validaciones Medios de Pago
            RuleFor(x => x.MediosPago)
                .Must(m => m == null || m.Count <= 4)
                .WithMessage("Máximo 4 medios de pago");

            // Validaciones Moneda
            When(x => x.CodMoneda != "CRC", () =>
            {
                RuleFor(x => x.TipoCambio)
                    .GreaterThan(0).WithMessage("Tipo cambio requerido para moneda diferente a CRC");
            });

            // Validaciones Resumen
            RuleFor(x => x.TotalComprobante)
                .GreaterThanOrEqualTo(0).WithMessage("Total comprobante inválido");

            RuleFor(x => x.TotalVentasNeta)
                .Equal(x => x.TotalVentas - x.TotalDescuentos)
                .WithMessage("Total venta neta inconsistente");

            // Validaciones Información Referencia
            When(x => !string.IsNullOrEmpty(x.InfoRefeTipoDoc), () =>
            {
                RuleFor(x => x.InfoRefeTipoDoc)
                    .Must(BeValidTipoDocReferencia).WithMessage("Tipo documento referencia inválido");

                RuleFor(x => x.InfoRefeCodigo)
                    .Must(BeValidCodigoReferencia).WithMessage("Código referencia inválido");
            });
        }

        private bool BeValidTipoDocReferencia(string tipoDoc)
        {
            return Enum.TryParse(tipoDoc, out TipoDocReferencia _);
        }

        private bool BeValidCodigoReferencia(string codigo)
        {
            return Enum.TryParse(codigo, out CodigoReferencia _);
        }


        private static readonly string[] ValoresTipoIdentificacion =
        GetEnumMemberValues<TipoIdentificacion>();

        private bool BeValidTipoIdentificacion(string tipoIdentif)
        {
            return ValoresTipoIdentificacion.Contains(tipoIdentif);
        }

        // Método genérico para obtener los valores reales del enum
        private static string[] GetEnumMemberValues<TEnum>() where TEnum : struct, Enum
        {
            return typeof(TEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => f.GetCustomAttribute<EnumMemberAttribute>())
                .Where(attr => attr != null)
                .Select(attr => attr!.Value)
                .ToArray();
        }
    }
}
