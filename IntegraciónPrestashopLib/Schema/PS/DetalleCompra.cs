using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class DetalleCompra
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("product_id")]
        public String ProductId { get; set; }

        [JsonProperty("product_quantity")]
        public String Cantidad { get; set; }

        [JsonProperty("product_reference")]
        public String Reference { get; set; }


        [JsonProperty("unit_price_tax_excl")]
        public String PrecioSinImpuesto { get; set; }

        [JsonProperty("unit_price_tax_incl")]
        public String PrecioConImpuesto { get; set; }
    }
}
