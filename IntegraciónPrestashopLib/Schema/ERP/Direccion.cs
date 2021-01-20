using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Direccion
    {
        public String AddressId { get; set; }
        public String AddressStreet { get; set; }
        internal int StateId { get; set; }
        public String AddressCity { get; set; }
        public String AddressCounty { get; set; }
    }
}
