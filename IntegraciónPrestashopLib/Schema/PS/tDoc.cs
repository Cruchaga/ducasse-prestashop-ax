using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class tDoc
    {
        [JsonProperty("cart_id")]
        public int IdCart { get; set; }
        public int Tipo { get; set; }
        public String Rut { get; set; }

        [JsonProperty("razsoc")]
        public String RazonSocial { get; set; }
        public String Giro { get; set; }

    }
}
