using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Request
{
    public class MakeManifestRequest
    {
        [JsonProperty("deliveries")]
        public int[] Deliveries { get; set; }
    }
}
