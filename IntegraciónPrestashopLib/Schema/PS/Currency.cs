using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class Currency
    {
        [JsonProperty("id")]
        public int CurrencyId { get; set; }
        [JsonProperty("name")]
        public String Nombre { get; set; }
        [JsonProperty("iso_code")]
        public String CurrencyCode { get; set; }
    }
}
