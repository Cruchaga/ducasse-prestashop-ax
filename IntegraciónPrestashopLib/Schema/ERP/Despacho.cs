using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Despacho
    {
        public String AccountNum { get; set; }
        public String DigitMod11 { get; set; }
        public String CustName { get; set; }
        public String LiteralBusiness { get; set; }
        public Direccion Direccion { get; set; }
        public String ContactTelephoneNumber { get; set; }
        public String ContactEmail { get; set; }

    }
}
