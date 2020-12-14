using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class State
    {
        //[JsonProperty("active")]
        [JsonProperty("id")]
        public int StateId { get; set; }
        //[JsonProperty("id_country")]
        //[JsonProperty("id_zone")]
        [JsonProperty("iso_code")]
        public String StateCode { get; set; }
        [JsonProperty("name")]
        public String Nombre { get; set; }
    }
}
