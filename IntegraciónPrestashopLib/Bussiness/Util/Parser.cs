using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Bussiness.Util
{
    internal class Parser
    {
        public Schema.PS.Producto GetPSProductoFromERP(Schema.ERP.Producto producto)
        {
            try
            {
                return new Schema.PS.Producto
                {
                    //TODO: Implementar
                };
            }
            catch(Exception e)
            {
                //TODO: aplicar logging
                return null;
            }
        }

        public Schema.ERP.OrdenDeVenta GetOVFromPSOC(Schema.PS.OrdenDeCompra orden)
        {
            try
            {
                return new Schema.ERP.OrdenDeVenta
                {
                    //TODO: Implementar
                };
            }
            catch (Exception e) {
                //TODO: implementar logging
                return null;
            }
        }
    }
}
