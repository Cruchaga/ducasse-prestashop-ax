using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class Combination
    {

        [XmlElement("id")]
        [JsonProperty("id")]
        public int CombinationId { get; set; }

        [XmlElement("id_product")]
        [JsonProperty("id_product")]
        public String ProductId { get; set; }

        [XmlElement("reference")]
        [JsonProperty("reference")]
        public String InventDimId { get; set; }
    }
}
