using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class EstadoResponse
    {
        [JsonProperty("order_state")]
        internal Estado estado { get; set; }

    }
}
