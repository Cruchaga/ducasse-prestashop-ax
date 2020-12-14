using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Facturacion
    {
        internal String CustomerId { get; set; }
        public String InvoiceAccount { get; set; }
        public String DigitMod11 { get; set; }
        public String CustNameInvoice { get; set; }
        public String LiteralBusiness { get; set; }
        public Direccion Direccion { get; set; }
        public String EmailInvoice { get; set; }
    }
}
