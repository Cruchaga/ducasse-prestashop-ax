using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Facturacion
    {
        public String Rut { get; set; }
        public String CustNameInvoice { get; set; }
        public String LiteralBusiness { get; set; }
        public Direccion Direccion { get; set; }
        public String EmailInvoice { get; set; }
        public string PhoneNumber { get; set; }
    }
}
