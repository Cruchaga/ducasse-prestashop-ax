using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS.Request
{
    [XmlRoot("prestashop")]
    public class ProductUpdateRequest
    {
        [XmlElement("product")]
        public ProductToUpdate producto { get; set; }
    }
}
