using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Inventario
    {
        public int ProductoId { get; set; }
        public Decimal Stock { get; set; }
    }
}
