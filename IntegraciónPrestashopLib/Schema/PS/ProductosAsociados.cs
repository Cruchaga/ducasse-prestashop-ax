using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    
    public  class ProductosAsociados
    {
        [JsonProperty("order_rows")]
        public IList<DetalleCompra> productos { get; set; }
        

    }
}
