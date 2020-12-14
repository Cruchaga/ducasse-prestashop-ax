using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Direccion
    {
        internal String AddressId { get; set; }
        public String AddressStreet { get; set; }
        public String AddressCountry { get; set; }
        internal int CountryId { get; set; }
        public String AddressState { get; set; }
        internal int StateId { get; set; }
        public String AddressCity { get; set; }
        public String AddressCounty { get; set; }
    }
}
