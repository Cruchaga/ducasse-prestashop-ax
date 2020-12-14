using Newtonsoft.Json;
using System.Collections.Generic;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class CustomerResponse
    {
        [JsonProperty("customer")]
        public Customer Clientes { get; set; }
    }
}