using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class PaymentsResponse
    {
        [XmlElement("order_payments")]
        [JsonProperty("order_payments")]
        public IList<Payment> OrderPayments { get; set; }
    }
}
