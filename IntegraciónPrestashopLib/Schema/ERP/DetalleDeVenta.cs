using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class DetalleDeVenta
    {
        internal List<Detalle> itemsVendidos;

        public DetalleDeVenta() { }
        internal DetalleDeVenta(List<Detalle> detalles)
        {
            this.itemsVendidos = detalles;
        }
        internal void SetDetalleDeVenta(List<Detalle> detalles)
        {
            this.itemsVendidos = detalles;
        }
        internal List<Detalle> GetDetalleDeVenta()
        {
            return itemsVendidos;
        }
        public int GetSize()
        {
            return itemsVendidos.Count;
        }

        public Detalle GetItemDetalle(int index)
        {
            return itemsVendidos[index];
        }
    }

    public class Detalle
    {
        public String ItemId { get; set; }
        public String InventDimId { get; set; }
        //public String InventLocationId { get; set; }
        public Decimal SalesLineQty { get; set; }
        public String SalesLineCurrencyCode { get; set; }
        public Decimal SalesLinePriceList { get; set; }
        public Decimal SalesLineDiscountPercent { get; set; }
        public Decimal SalesLineNetTotal { get; set; }
        public Decimal SalesLineTotalAmount { get; set; }
    }
}
