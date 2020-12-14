using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    public class OrdenesResponse
    {
        [XmlElement("orders")]
        [JsonProperty("orders")]
        public IList<OrdenDeCompra> Orders { get; set; }

        public String Json { get; set; }
    }
}
