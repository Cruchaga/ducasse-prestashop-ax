using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Response
{
    public class GetTrackingResponse : MakeDeliveryResponse
    {
        public Tracking[] Tracking { get; set; }
    }

    public class Tracking
    {
        [JsonProperty("status_id")]
        public int StatusId { get; set; }
        public String Name{ get; set; }

        public String Code { get; set; }
        public String Comment { get; set; }
        [JsonProperty("created_at")]
        public DateTime Fecha { get; set; }
    }
}
