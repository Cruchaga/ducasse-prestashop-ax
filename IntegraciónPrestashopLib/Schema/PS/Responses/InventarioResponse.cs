using IntegracionPrestashopLib.Schema.ERP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    public class InventarioResponse
    {
        public Inventario Inventario { get; internal set; }
        public bool HayError { get; internal set; }
        public int ErrorCode { get; internal set; }
        public string Error { get; internal set; }
    }
}
