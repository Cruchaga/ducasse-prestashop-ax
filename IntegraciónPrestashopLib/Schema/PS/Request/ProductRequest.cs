using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS.Request
{
    [XmlRoot("prestashop")]
    public class ProductRequest
    {
        [XmlElement("product")]
        public Producto producto { get; set; }
    }
}
