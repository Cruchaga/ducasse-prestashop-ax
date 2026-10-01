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

        /// <summary>
        /// True cuando la consulta a PrestaShop falló (red, 401, 500, …). Permite
        /// distinguir "no hubo órdenes" de "no se pudo consultar", que antes se
        /// reportaban igual y dejaban pasar la falla en silencio.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool HuboError { get; set; }

        /// <summary>Detalle del fallo cuando <see cref="HuboError"/> es true.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public String MensajeError { get; set; }
    }
}
