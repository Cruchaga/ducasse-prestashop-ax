using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class Payment
    {
        [JsonProperty("transaction_id")]
        [XmlElement("transaction_id")]
        public String TransactionId { get; set; }
    }
}
