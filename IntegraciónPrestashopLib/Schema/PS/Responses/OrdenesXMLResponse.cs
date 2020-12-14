using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    [XmlRoot("prestashop")]
    public class OrdenesXMLResponse
    {
        [XmlElement("orders")]
        public OrdenDeCompraXML[] Orders { get; set; }

    }
}
