using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class ProductQueryResponse
    {
        [JsonProperty("products")]
        public ProductToUpdate[] products { get; set; }

        [JsonIgnore]
        public String Json { get; set; }
    }
}
