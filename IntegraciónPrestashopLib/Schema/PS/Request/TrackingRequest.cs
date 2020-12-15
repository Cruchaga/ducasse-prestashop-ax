using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS.Request
{
    [XmlRoot("prestashop")]
    public class TrackingRequest
    {
        [XmlElement("order_history")]
        public OrderHistory Tracking { get; set; }
    }
}
