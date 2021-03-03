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
        public String CarrierService { get; set; }
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
        private String pesoTotal;
        public String Get_PesoTotal()
        {
            return pesoTotal;
        }
        public void Set_PesoTotal(int parteEntera, int parteDecimal)
        {
            pesoTotal = $"{parteEntera}.{parteDecimal}";
        }
        private String volumenTotal;
        public String Get_VolumenTotal()
        {
            return volumenTotal;
        }
        public void Set_VolumenTotal(int parteEntera, int parteDecimal)
        {
            volumenTotal = $"{parteEntera}.{parteDecimal}";
        }
        public String CodBodegaOrigen { get; set; }

    }
}
