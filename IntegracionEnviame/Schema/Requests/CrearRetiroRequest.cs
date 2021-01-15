using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Requests
{
    public class CrearRetiroRequest
    {
        public String CarrierCod { get; set; }
        public String CodigoBodega { get; set; }
        public int CantidadBultos { get; set; }
        public String NombreContactoRetiro { get; set; }
        public String TelefonoContactoRetiro { get; set; }
        public String RangoRetiro { get; set; }
        public String FechaRetiro { get; set; }
    }
}
