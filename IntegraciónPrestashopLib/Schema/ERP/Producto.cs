using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Producto
    {
        /*
         * Id dimension
         */
        public String ItemId_InventDimId { get; set; }
        /*
         * Codigo de barras
         */
        public String ItemBarCode { get; set; }
        /*
         * Nombre producto
         */
        public String ItemName { get; set; }
        /*
         * Alias del producto
         */
        public String NameAlias { get; set; }
        /*
         * Item free text
         */
        public String ItemTxt { get; set; }
        /*
         * Precio venta
         */
        public Int64 SalesPrice { get; set; }
        /*
         * Unidad de venta
         */
        public String UnitId { get; set; }
        /*
         * Link de imagen
         */
        public String ItemImg { get; set; }
        /*
         * Cantidad maxima de compra
         */
        public Decimal QtyMaxToBuy { get; set; }

        public Decimal NetWeight { get; set; }
        public Decimal GrossDepth { get; set; }
        public Decimal GrossWidth { get; set; }
        public Decimal GrossHeight { get; set; }

        public Inventario Inventario { get; set; }
    }
}
