using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class Tupla
    {
        [JsonProperty("id")]
        public String Id { get; set; }
        [JsonProperty("value")]
        public String Valor { get; set; }
    }
}
