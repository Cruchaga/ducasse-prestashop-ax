using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class Message
    {
        [XmlElement("message")]
        [JsonProperty("message")]
        public String Msg { get; set; }

        [XmlElement("date_add")]
        [JsonProperty("date_add")]
        public  DateTime fechaHora { get; set; }
    }
}
