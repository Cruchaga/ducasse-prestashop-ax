using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class OrderHistory
    {
        [XmlElement("id_employee")]
        public int EmployeeId { get; set; }

        [XmlElement("id_order_state")]
        public int EstadoId { get; set; }

        [XmlElement("id_order")]
        public int OrderId { get; set; }
    }
}
