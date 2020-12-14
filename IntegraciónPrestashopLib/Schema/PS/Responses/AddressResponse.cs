using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class AddressResponse
    {
        [JsonProperty("address")]
        internal Address Address { get; set; }
    }
}
