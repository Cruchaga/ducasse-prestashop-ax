using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class ProductResponse
    {
        [JsonProperty("product")]
        public Producto Producto { get; set; }
    }
}
