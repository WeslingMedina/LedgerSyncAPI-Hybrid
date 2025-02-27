using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.GenerateXml
{
    public record InvoiceRequest(
        string Clave,
        string CodigoActividad,
        string Consecutivo,
        DateTime FechaEmision,
        EmisorRequest Emisor,
        ReceptorRequest Receptor,
        string CondicionVenta,
        decimal PlazoCredito,
        string MedioPago,
        List<MedioPagoRequest> MediosPago,
        string CodMoneda,
        decimal TipoCambio,
        decimal TotalServGravados,
        decimal TotalServExentos,
        decimal TotalServExonerados,
        decimal TotalMercGravada,
        decimal TotalMercExenta,
        decimal TotalMercExonerada,
        decimal TotalGravados,
        decimal TotalExento,
        decimal TotalExonerado,
        decimal TotalVentas,
        decimal TotalDescuentos,
        decimal TotalVentasNeta,
        decimal TotalImpuestos,
        decimal TotalIVADevuelto,
        decimal TotalOtrosCargos,
        decimal TotalComprobante,
        string Otros,
        string OtrosType,
        List<DetalleRequest> Detalles,
        string InfoRefeTipoDoc,
        string InfoRefeNumero,
        DateTime? InfoRefeFechaEmision,
        string InfoRefeCodigo,
        string InfoRefeRazon,
        List<OtroCargoRequest> OtrosCargos
    );

    public record EmisorRequest(
        string Nombre,
        string TipoIdentif,
        string NumIdentif,
        string? NombreComercial,
        string Provincia,
        string Canton,
        string Distrito,
        string? Barrio,
        string OtrasSenas,
        string? CodPaisTel,
        string? Tel,
        string? CodPaisFax,
        string? Fax,
        string Email
    );

    public record ReceptorRequest(
        string Nombre,
        string TipoIdentif,
        string NumIdentif,
        string? IdentifExtranjero,
        string? NombreComercial,
        string? Provincia,
        string? Canton,
        string? Distrito,
        string? Barrio,
        string? OtrasSenas,
        string? OtrasSenasExtranjero,
        string? CodPaisTel,
        string? Tel,
        string? CodPaisFax,
        string? Fax,
        string? Email
    );

    public record DetalleRequest(
        string Codigo,
        List<CodigoComercialRequest> CodigoComercial,
        decimal Cantidad,
        string UnidadMedida,
        string UnidadMedidaComercial,
        string Detalle,
        decimal PrecioUnitario,
        decimal MontoTotal,
        List<DescuentoRequest> Descuentos,
        decimal SubTotal,
        decimal? BaseImponible,
        List<ImpuestoRequest> Impuestos,
        decimal? ImpuestoNeto,
        decimal MontoTotalLinea
    );

    public record MedioPagoRequest(
        string Codigo,
        decimal Monto
    );

    public record OtroCargoRequest(
        string TipoDocumento,
        string? NumeroIdentidadTercero,
        string? NombreTercero,
        string Detalle,
        decimal? Porcentaje,
        decimal MontoCargo
    );

    public record CodigoComercialRequest(
        string Tipo,
        string Codigo
    );

    public record DescuentoRequest(
        decimal MontoDescuento,
        string NaturalezaDescuento
    );

    public record ImpuestoRequest(
        string Codigo,
        string? CodigoTarifa,
        decimal? Tarifa,
        decimal? FactorIVA,
        decimal Monto,
        ExoneracionRequest? Exoneracion
    );

    public record ExoneracionRequest(
        string TipoDocumento,
        string NumeroDocumento,
        string NombreInstitucion,
        DateTime FechaEmision,
        decimal PorcentajeExoneracion,
        decimal MontoExoneracion
    );
}
