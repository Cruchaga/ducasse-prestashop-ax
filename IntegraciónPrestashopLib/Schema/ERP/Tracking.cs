using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Tracking
    {
        public int OrdenId { get; set; }
        public int NuevoEstadoId { get; set; }
        public Tracking(int ordenId, int estadoId)
        {
            this.OrdenId = ordenId;
            this.NuevoEstadoId = estadoId;
        }
    }
}
