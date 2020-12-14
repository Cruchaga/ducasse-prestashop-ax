using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class Estado
    {
        [JsonProperty("id")]
        public String EstadoId { get; set; }

        [JsonProperty("name")]
        public String Nombres { get; set; }
    }
}
