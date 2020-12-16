using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class StockAvailableRepsonse
    {
        [JsonProperty("stock_availables")]
        public StockAvailable[] Stock { get; set; }
    }
}
