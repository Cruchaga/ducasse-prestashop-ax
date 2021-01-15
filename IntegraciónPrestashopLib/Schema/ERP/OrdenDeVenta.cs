using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class OrdenDeVenta
    {
        public Int64 SalesIdPrestaShop { get; set; }
        internal String EstadoId { get; set; }
        internal String IdCart { get; set; }
        public String Estado { get; set; }
        public DateTime SalesDate { get; set; }
        public Facturacion Facturacion { get; set; }
        public Despacho Despacho { get; set; }
        public String PaymMode { get; set; }
        public Boolean Factura { get; set; }
        public Decimal TotalWeight { get; set; }
        internal String CurrencyId { get; set; }
        public String CurrencyCode { get; set; }
        public Decimal TotalNetAmount { get; set; }
        public Decimal TotalShippingAmount { get; set; }
        public Decimal TaxAmount { get; set; }
        public Decimal TotalAmount { get; set; }
        public DetalleDeVenta DetalleDeVentas { get; set; }
    }
}
