using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Responses
{
    public class CrearEnvioResponse : CreateResponse
    {
        public int Identifier { get; set; }
        public String TrackingNumber { get; set; }
        public String Carrier { get; set; }
        public String EtiquetaPdf { get; set; }
        public String EtiquetaZebra { get; set; }
        public String UrlTracking { get; set; }
    }
}
