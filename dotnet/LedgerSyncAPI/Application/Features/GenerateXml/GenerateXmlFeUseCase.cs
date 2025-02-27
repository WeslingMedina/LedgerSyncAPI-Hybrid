using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Features.GenerateXml
{
    public class GenerateXmlFeUseCase
    {
        private readonly IValidator<InvoiceRequest> _validator;
        private static readonly XNamespace ns = "https://cdn.comprobanteselectronicos.go.cr/xml-schemas/v4.4/facturaElectronica";

        public GenerateXmlFeUseCase(IValidator<InvoiceRequest> validator)
        {
            _validator = validator;
        }

        // Método Execute faltante
        public async Task<InvoiceResponse> Execute(InvoiceRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var xml = GenerateXml(request);
            return new InvoiceResponse(request.Clave, Convert.ToBase64String(Encoding.UTF8.GetBytes(xml)));
        }

        private string GenerateXml(InvoiceRequest request)
        {
            var xml = new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "FacturaElectronica",
                    new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),

                    // Sección principal
                    new XElement(ns + "Clave", request.Clave),
                    new XElement(ns + "CodigoActividad", request.CodigoActividad.PadLeft(6, '0')),
                    new XElement(ns + "NumeroConsecutivo", request.Consecutivo),
                    new XElement(ns + "FechaEmision", request.FechaEmision.ToString("yyyy-MM-ddTHH:mm:ss")),

                    // Emisor
                    BuildEmisor(request.Emisor),

                    // Receptor
                    BuildReceptor(request.Receptor),

                    // Sección de condiciones de pago
                    new XElement(ns + "CondicionVenta", request.CondicionVenta),
                    request.PlazoCredito > 0 ? new XElement(ns + "PlazoCredito", request.PlazoCredito) : null,
                    !string.IsNullOrEmpty(request.MedioPago) ? new XElement(ns + "MedioPago", request.MedioPago) : null,
                    request.MediosPago?.Take(4).Select(mp => new XElement(ns + "MedioPago", mp.Codigo)),

                    // Detalles del servicio
                    new XElement(ns + "DetalleServicio",
                        request.Detalles.Select((d, index) => BuildDetalle(d, index + 1))),

                    // Otros cargos
                    request.OtrosCargos?.Take(15).Select(oc => BuildOtroCargo(oc)),

                    // Resumen Factura
                    new XElement(ns + "ResumenFactura",
                        (request.CodMoneda != "CRC" && request.TipoCambio > 0) ?
                            new XElement(ns + "CodigoTipoMoneda",
                                new XElement(ns + "CodigoMoneda", request.CodMoneda),
                                new XElement(ns + "TipoCambio", request.TipoCambio)) : null,
                        new XElement(ns + "TotalServGravados", request.TotalServGravados),
                        new XElement(ns + "TotalServExentos", request.TotalServExentos),
                        new XElement(ns + "TotalServExonerado", request.TotalServExonerados),
                        new XElement(ns + "TotalMercanciasGravadas", request.TotalMercGravada),
                        new XElement(ns + "TotalMercanciasExentas", request.TotalMercExenta),
                        new XElement(ns + "TotalMercExonerada", request.TotalMercExonerada),
                        new XElement(ns + "TotalGravado", request.TotalGravados),
                        new XElement(ns + "TotalExento", request.TotalExento),
                        new XElement(ns + "TotalExonerado", request.TotalExonerado),
                        new XElement(ns + "TotalVenta", request.TotalVentas),
                        new XElement(ns + "TotalDescuentos", request.TotalDescuentos),
                        new XElement(ns + "TotalVentaNeta", request.TotalVentasNeta),
                        new XElement(ns + "TotalImpuesto", request.TotalImpuestos),
                        new XElement(ns + "TotalIVADevuelto", request.TotalIVADevuelto),
                        new XElement(ns + "TotalOtrosCargos", request.TotalOtrosCargos),
                        new XElement(ns + "TotalComprobante", request.TotalComprobante)),

                    // Información de referencia
                    BuildInformacionReferencia(request),

                    // Otros
                    !string.IsNullOrEmpty(request.Otros) ?
                        new XElement(ns + "Otros",
                            new XElement(ns + request.OtrosType, request.Otros)) : null
                )
            );

            return xml.ToString();
        }

        private XElement BuildEmisor(EmisorRequest emisor)
        {
            var emisorElement = new XElement(ns + "Emisor",
                new XElement(ns + "Nombre", emisor.Nombre),
                new XElement(ns + "Identificacion",
                    new XElement(ns + "Tipo", emisor.TipoIdentif),
                    new XElement(ns + "Numero", emisor.NumIdentif)),
                !string.IsNullOrEmpty(emisor.NombreComercial) ?
                    new XElement(ns + "NombreComercial", emisor.NombreComercial) : null,
                new XElement(ns + "Ubicacion",
                    new XElement(ns + "Provincia", emisor.Provincia),
                    new XElement(ns + "Canton", emisor.Canton),
                    new XElement(ns + "Distrito", emisor.Distrito),
                    !string.IsNullOrEmpty(emisor.Barrio) ?
                        new XElement(ns + "Barrio", emisor.Barrio) : null,
                    new XElement(ns + "OtrasSenas", emisor.OtrasSenas)),
                (!string.IsNullOrEmpty(emisor.CodPaisTel) && !string.IsNullOrEmpty(emisor.Tel)) ?
                    new XElement(ns + "Telefono",
                        new XElement(ns + "CodigoPais", emisor.CodPaisTel),
                        new XElement(ns + "NumTelefono", emisor.Tel)) : null,
                (!string.IsNullOrEmpty(emisor.CodPaisFax) && !string.IsNullOrEmpty(emisor.Fax)) ?
                    new XElement(ns + "Fax",
                        new XElement(ns + "CodigoPais", emisor.CodPaisFax),
                        new XElement(ns + "NumTelefono", emisor.Fax)) : null,
                new XElement(ns + "CorreoElectronico", emisor.Email));

            return emisorElement;
        }

        private XElement BuildReceptor(ReceptorRequest receptor)
        {
            var receptorElement = new XElement(ns + "Receptor",
                new XElement(ns + "Nombre", receptor.Nombre),
                new XElement(ns + "Identificacion",
                    new XElement(ns + "Tipo", receptor.TipoIdentif),
                    new XElement(ns + "Numero", receptor.NumIdentif)),
                !string.IsNullOrEmpty(receptor.IdentifExtranjero) ?
                    new XElement(ns + "IdentificacionExtranjero", receptor.IdentifExtranjero) : null,
                !string.IsNullOrEmpty(receptor.NombreComercial) ?
                    new XElement(ns + "NombreComercial", receptor.NombreComercial) : null,
                (receptor.Provincia != null && receptor.Canton != null && receptor.Distrito != null) ?
                    new XElement(ns + "Ubicacion",
                        new XElement(ns + "Provincia", receptor.Provincia),
                        new XElement(ns + "Canton", receptor.Canton),
                        new XElement(ns + "Distrito", receptor.Distrito),
                        !string.IsNullOrEmpty(receptor.Barrio) ?
                            new XElement(ns + "Barrio", receptor.Barrio) : null,
                        new XElement(ns + "OtrasSenas", receptor.OtrasSenas)) : null,
                !string.IsNullOrEmpty(receptor.OtrasSenasExtranjero) ?
                    new XElement(ns + "OtrasSenasExtranjero", receptor.OtrasSenasExtranjero) : null,
                (!string.IsNullOrEmpty(receptor.CodPaisTel) && !string.IsNullOrEmpty(receptor.Tel)) ?
                    new XElement(ns + "Telefono",
                        new XElement(ns + "CodigoPais", receptor.CodPaisTel),
                        new XElement(ns + "NumTelefono", receptor.Tel)) : null,
                (!string.IsNullOrEmpty(receptor.CodPaisFax) && !string.IsNullOrEmpty(receptor.Fax)) ?
                    new XElement(ns + "Fax",
                        new XElement(ns + "CodigoPais", receptor.CodPaisFax),
                        new XElement(ns + "NumTelefono", receptor.Fax)) : null,
                !string.IsNullOrEmpty(receptor.Email) ?
                    new XElement(ns + "CorreoElectronico", receptor.Email) : null);

            return receptorElement;
        }

        private XElement BuildDetalle(DetalleRequest detalle, int numeroLinea)
        {
            return new XElement(ns + "LineaDetalle",
                new XElement(ns + "NumeroLinea", numeroLinea),
                !string.IsNullOrEmpty(detalle.Codigo) ?
                    new XElement(ns + "Codigo", detalle.Codigo) : null,
                detalle.CodigoComercial?.Take(5).Select(cc =>
                    new XElement(ns + "CodigoComercial",
                        new XElement(ns + "Tipo", cc.Tipo),
                        new XElement(ns + "Codigo", cc.Codigo))),
                new XElement(ns + "Cantidad", detalle.Cantidad),
                new XElement(ns + "UnidadMedida", detalle.UnidadMedida),
                !string.IsNullOrEmpty(detalle.UnidadMedidaComercial) ?
                    new XElement(ns + "UnidadMedidaComercial", detalle.UnidadMedidaComercial) : null,
                new XElement(ns + "Detalle", detalle.Detalle),
                new XElement(ns + "PrecioUnitario", detalle.PrecioUnitario),
                new XElement(ns + "MontoTotal", detalle.MontoTotal),
                detalle.Descuentos?.Take(5).Select(d =>
                    new XElement(ns + "Descuento",
                        new XElement(ns + "MontoDescuento", d.MontoDescuento),
                        new XElement(ns + "NaturalezaDescuento", d.NaturalezaDescuento))),
                new XElement(ns + "SubTotal", detalle.SubTotal),
                detalle.BaseImponible.HasValue ?
                    new XElement(ns + "BaseImponible", detalle.BaseImponible.Value) : null,
                detalle.Impuestos?.Select(i =>
                    new XElement(ns + "Impuesto",
                        new XElement(ns + "Codigo", i.Codigo),
                        !string.IsNullOrEmpty(i.CodigoTarifa) ?
                            new XElement(ns + "CodigoTarifa", i.CodigoTarifa) : null,
                        i.Tarifa.HasValue ?
                            new XElement(ns + "Tarifa", i.Tarifa.Value) : null,
                        i.FactorIVA.HasValue ?
                            new XElement(ns + "FactorIVA", i.FactorIVA.Value) : null,
                        new XElement(ns + "Monto", i.Monto),
                        i.Exoneracion != null ?
                            new XElement(ns + "Exoneracion",
                                new XElement(ns + "TipoDocumento", i.Exoneracion.TipoDocumento),
                                new XElement(ns + "NumeroDocumento", i.Exoneracion.NumeroDocumento),
                                new XElement(ns + "NombreInstitucion", i.Exoneracion.NombreInstitucion),
                                new XElement(ns + "FechaEmision", i.Exoneracion.FechaEmision.ToString("yyyy-MM-dd")),
                                new XElement(ns + "PorcentajeExoneracion", i.Exoneracion.PorcentajeExoneracion),
                                new XElement(ns + "MontoExoneracion", i.Exoneracion.MontoExoneracion)) : null)),
                new XElement(ns + "MontoTotalLinea", detalle.MontoTotalLinea));
        }

        private XElement BuildInformacionReferencia(InvoiceRequest request)
        {
            if (string.IsNullOrEmpty(request.InfoRefeTipoDoc)) return null;

            return new XElement(ns + "InformacionReferencia",
                new XElement(ns + "TipoDoc", request.InfoRefeTipoDoc),
                !string.IsNullOrEmpty(request.InfoRefeNumero) ?
                    new XElement(ns + "Numero", request.InfoRefeNumero) : null,
                request.InfoRefeFechaEmision.HasValue ?
                    new XElement(ns + "FechaEmision", request.InfoRefeFechaEmision.Value.ToString("yyyy-MM-dd")) : null,
                !string.IsNullOrEmpty(request.InfoRefeCodigo) ?
                    new XElement(ns + "Codigo", request.InfoRefeCodigo) : null,
                !string.IsNullOrEmpty(request.InfoRefeRazon) ?
                    new XElement(ns + "Razon", request.InfoRefeRazon) : null);
        }

        private XElement BuildOtroCargo(OtroCargoRequest cargo)
        {
            return new XElement(ns + "OtrosCargos",
                new XElement(ns + "TipoDocumento", cargo.TipoDocumento),
                !string.IsNullOrEmpty(cargo.NumeroIdentidadTercero) ?
                    new XElement(ns + "NumeroIdentidadTercero", cargo.NumeroIdentidadTercero) : null,
                !string.IsNullOrEmpty(cargo.NombreTercero) ?
                    new XElement(ns + "NombreTercero", cargo.NombreTercero) : null,
                new XElement(ns + "Detalle", cargo.Detalle),
                cargo.Porcentaje.HasValue ?
                    new XElement(ns + "Porcentaje", cargo.Porcentaje.Value) : null,
                new XElement(ns + "MontoCargo", cargo.MontoCargo));
        }
    }
}
