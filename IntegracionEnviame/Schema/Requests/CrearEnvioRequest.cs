using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Requests
{
    public class CrearEnvioRequest
    {
        public Orden Orden { get; set; }
        public Destino Destino { get; set; }
        public String Carrier { get; set; }
    }

    public class Destino
    {
        public String NombreReceptor { get; set; }
        public String EmailReceptor { get; set; }
        public String TelefonoReceptor { get; set; }
        public String Comuna { get; set; }
        public String DireccionCompleta { get; set; }
        public String Informacion { get; set; }
    }

    public class Orden
    {
        public int CantidadBultos { get; set; }
        public String DescripcionContenido { get; set; }
        public String IdReferencia { get; set; }
        public int Precio { get; set; }
        public Decimal PesoTotal { get; set; }
        public Decimal VolumenTotal { get; set; }
        public String CodBodegaOrigen { get; set; }

    }
}
