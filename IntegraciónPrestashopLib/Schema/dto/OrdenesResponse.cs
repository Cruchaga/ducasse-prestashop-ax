using IntegracionPrestashopLib.Schema.Dictionary;
using IntegracionPrestashopLib.Schema.ERP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace IntegracionPrestashopLib.Schema.dto
{
    public class OrdenesResponse
    {

        private List<OrdenDeVenta> ordenesDeVenta;
        public String JSon { get; set; }
        public int ErrorCode { get; set; }
        public String  Error { get; set; }
        public String Traza { get; set; }

        public OrdenesResponse()
        {
            this.ErrorCode = IntegracionErrorCode.NONE;
        }
        internal void SetOrdenesDeVenta(List<OrdenDeVenta> ordenes)
        {
            this.ordenesDeVenta = ordenes;
        }

        public int GetSize()
        {
            if (ordenesDeVenta == null)
                return 0;
            else
                return ordenesDeVenta.Count();
        }

        public OrdenDeVenta GetOrden(int index)
        {
            return ordenesDeVenta[index];
        }

    }
}
