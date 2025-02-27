using Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class GetClaveQueryValidator : AbstractValidator<GetClaveQuery>
    {
        public GetClaveQueryValidator()
        {
            // Validaciones similares al código PHP
            RuleFor(x => x.TipoDocumento)
                .NotEmpty().WithMessage("TipoDocumento requerido")
                .Must(BeValidTipoDocumento).WithMessage("TipoDocumento no válido");

            RuleFor(x => x.TipoCedula)
                .NotEmpty().WithMessage("TipoCedula requerido")
                .Must(BeValidTipoCedula).WithMessage("TipoCedula no válido");

            RuleFor(x => x.Cedula)
                .NotEmpty().WithMessage("Cedula requerida")
                .Must(BeNumeric).WithMessage("Cedula debe ser numérica");

            RuleFor(x => x.CodigoPais)
                .NotEmpty().WithMessage("CodigoPais requerido")
                .Length(3).WithMessage("CodigoPais debe tener 3 dígitos")
                .Must(BeNumeric).WithMessage("CodigoPais debe ser numérico");

            RuleFor(x => x.Consecutivo)
                .NotEmpty().WithMessage("Consecutivo requerido")
                .Must(BeNumeric).WithMessage("Consecutivo debe ser numérico");

            RuleFor(x => x.Situacion)
                .NotEmpty().WithMessage("Situacion requerida")
                .Must(BeValidSituacion).WithMessage("Situacion no válida");

            RuleFor(x => x.CodigoSeguridad)
                .NotEmpty().WithMessage("CodigoSeguridad requerido")
                .Must(BeNumeric).WithMessage("CodigoSeguridad debe ser numérico");

            When(x => !string.IsNullOrEmpty(x.Sucursal), () =>
            {
                RuleFor(x => x.Sucursal)
                    .Must(BeNumeric).WithMessage("Sucursal debe ser numérica");
            });

            When(x => !string.IsNullOrEmpty(x.Terminal), () =>
            {
                RuleFor(x => x.Terminal)
                    .Must(BeNumeric).WithMessage("Terminal debe ser numérica");
            });
        }

        private bool BeValidTipoDocumento(string tipoDocumento)
            => new[] { "FE", "ND", "NC", "TE", "CCE", "CPCE", "RCE", "FEC", "FEE" }.Contains(tipoDocumento);

        private bool BeValidTipoCedula(string tipoCedula)
            => new[] { "fisico", "juridico", "dimex", "nite", "01", "02", "03", "04" }.Contains(tipoCedula.ToLower());

        private bool BeValidSituacion(string situacion)
            => new[] { "normal", "contingencia", "sininternet" }.Contains(situacion.ToLower());

        private bool BeNumeric(string value)
            => long.TryParse(value, out _);
    }
}
