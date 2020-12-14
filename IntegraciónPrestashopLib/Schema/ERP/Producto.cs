using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Producto
    {
        /*
         * Codigo de producto
         */
        public String ItemId { get; set; }
        /*
         * Id dimension
         */
        public String InventDimId { get; set; }
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
         * Id Configuracion 
         */
        public String ConfigId { get; set; }
        /*
         * Id Tamaño
         */
        public String SizeId { get; set; }
        /*
         * Id Color 
         */
        public String ColorId { get; set; }
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
        /*
         * Cantidad por embalaje
         */
        public String TaxPackagingQty { get; set; }

        public Inventario Inventario { get; set; }
    }
}
