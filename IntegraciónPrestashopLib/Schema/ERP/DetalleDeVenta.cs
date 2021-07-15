using IntegracionPrestashopLib.Schema.PS;
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
        public String Sku { get; set; }
        //public String InventLocationId { get; set; }
        public Decimal SalesLineQty { get; set; }
        public String SalesLineCurrencyCode { get; set; }
        public Decimal SalesLineNetPrice { get; set; }
        public Decimal SalesLinePrice { get; set; }
        public Decimal SalesLineNetTotalAmount { get; set; }
        public Decimal SalesLineTotal { get; set; }
        public List<AssociatedCategory> Categories { get; internal set; }
    }
}
