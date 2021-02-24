using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class MessageResponse
    {
        [XmlElement("messages")]
        [JsonProperty("messages")]
        public IList<Message> OrderPayments { get; set; }
    }
}
