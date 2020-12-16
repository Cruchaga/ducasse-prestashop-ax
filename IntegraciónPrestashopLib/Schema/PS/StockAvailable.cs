using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class StockAvailable
    {
        [XmlElement("id")]
        [JsonProperty("id")]
        public String StockId { get; set; }
        
        [XmlElement("id_product")]
        [JsonProperty("id_product")]
        public String ProductId { get; set; }
        
        [XmlElement("id_product_attribute")]
        [JsonProperty("id_product_attribute")]
        public String ProductAttributeId { get; set; }
        
        [XmlElement("id_shop")]
        [JsonProperty("id_shop")]
        public String shopId { get; set; }
        
        [XmlElement("quantity")]
        [JsonProperty("quantity")]
        public String Stock { get; set; }
        
        [XmlElement("depends_on_stock")]
        [JsonProperty("depends_on_stock")]
        public String DependsOnStock { get; set; }
        
        [XmlElement("out_of_stock")]
        [JsonProperty("out_of_stock")]
        public String OutOfStock { get; set; }
    }
}
