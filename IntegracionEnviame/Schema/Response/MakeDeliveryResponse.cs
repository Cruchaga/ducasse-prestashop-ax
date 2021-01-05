using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Response
{
    public class MakeDeliveryResponse
    {
        public int identifier { get; set; }
        
        [JsonProperty("imported_id")]
        public String PrestashopOrderId { get; set; }

        [JsonProperty("tracking_number")]
        public String TrackingNumber { get; set; }
        public Status Status { get; set; }
        public Customer Customer { get; set; }

        [JsonProperty("shipping_address")]
        public ShippingAddress ShippingAddress { get; set; }
        public String Country{ get; set; }
        public String Carrier { get; set; }
        public String Service { get; set; }
        public Label Label { get; set; }

    }
    public class Customer
    {
        [JsonProperty("full_name")]
        public String FullName { get; set; }
        public String Email { get; set; }
        public String Phone { get; set; }

    }

    public class Label
    {
        [JsonProperty("PDF")]
        public String Pdf { get; set; }

        [JsonProperty("ZPL")]
        public ZPL Zpl { get; set; }

    }

    public class ZPL
    {
        public String Raw { get; set; }
        public String url { get; set; }
    }

    public class ShippingAddress
    {
        [JsonProperty("full_address")]
        public String FullName { get; set; }

        [JsonProperty("place")]
        public String Comuna { get; set; }
        public String Home { get; set; }
    }

    public class Status
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Code { get; set; }
        public String Info { get; set; }
        
        [JsonProperty("created_at")]
        public DateTime Fecha { get; set; }
    }
}
