using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    public class CombinationsResponse
    {
        [JsonProperty("combinations")]
        public Combination[] products { get; set; }
    }
}
