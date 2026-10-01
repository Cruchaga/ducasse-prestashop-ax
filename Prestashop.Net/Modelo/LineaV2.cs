using System.Collections.Generic;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>
    /// Línea de la orden. "Neto" = sin IVA. El descuento de cupones ya viene prorrateado:
    /// TotalNetoFinal = TotalNeto - DescuentoNeto es lo que efectivamente se cobró por la línea.
    /// </summary>
    public class LineaV2
    {
        public long IdDetalle { get; internal set; }
        /// <summary>Referencia del producto = código de artículo de AX (ItemId_InventDimId).</summary>
        public string Sku { get; internal set; }
        public long IdProducto { get; internal set; }
        public long IdCombinacion { get; internal set; }
        public string Nombre { get; internal set; }
        public int Cantidad { get; internal set; }
        /// <summary>Precio unitario de lista, antes de ofertas.</summary>
        public decimal PrecioListaNeto { get; internal set; }
        /// <summary>Precio unitario al que se vendió (con ofertas de catálogo, antes de cupones).</summary>
        public decimal PrecioNeto { get; internal set; }
        public decimal PrecioBruto { get; internal set; }
        public decimal TasaImpuesto { get; internal set; }
        public decimal TotalNeto { get; internal set; }
        public decimal TotalBruto { get; internal set; }
        /// <summary>Parte de los cupones de la orden que corresponde a esta línea.</summary>
        public decimal DescuentoNeto { get; internal set; }
        public decimal DescuentoBruto { get; internal set; }
        public decimal TotalNetoFinal { get; internal set; }
        public decimal TotalBrutoFinal { get; internal set; }
        /// <summary>Envío al que pertenece la línea (EnvioV2.IdEnvio).</summary>
        public long IdEnvio { get; internal set; }
        public long IdShipment { get; internal set; }
        /// <summary>true si la línea es un componente de un pack: el pack se entrega desglosado en sus artículos.</summary>
        public bool EsComponentePack { get; internal set; }
        /// <summary>SKU del pack del que proviene (vacío si no es componente).</summary>
        public string SkuPackPadre { get; internal set; }

        internal static LineaV2 Desde(Dictionary<string, object> d)
        {
            LineaV2 x = new LineaV2();
            x.IdDetalle = Json.Largo(d, "idDetalle");
            x.Sku = Json.Texto(d, "sku");
            x.IdProducto = Json.Largo(d, "idProducto");
            x.IdCombinacion = Json.Largo(d, "idCombinacion");
            x.Nombre = Json.Texto(d, "nombre");
            x.Cantidad = Json.Entero(d, "cantidad");
            x.PrecioListaNeto = Json.Decimal(d, "precioListaNeto");
            x.PrecioNeto = Json.Decimal(d, "precioNeto");
            x.PrecioBruto = Json.Decimal(d, "precioBruto");
            x.TasaImpuesto = Json.Decimal(d, "tasaImpuesto");
            x.TotalNeto = Json.Decimal(d, "totalNeto");
            x.TotalBruto = Json.Decimal(d, "totalBruto");
            x.DescuentoNeto = Json.Decimal(d, "descuentoNeto");
            x.DescuentoBruto = Json.Decimal(d, "descuentoBruto");
            x.TotalNetoFinal = Json.Decimal(d, "totalNetoFinal");
            x.TotalBrutoFinal = Json.Decimal(d, "totalBrutoFinal");
            x.IdEnvio = Json.Largo(d, "idEnvio");
            x.IdShipment = Json.Largo(d, "idShipment");
            x.EsComponentePack = Json.Booleano(d, "esComponentePack");
            x.SkuPackPadre = Json.Texto(d, "skuPackPadre");
            return x;
        }
    }
}
