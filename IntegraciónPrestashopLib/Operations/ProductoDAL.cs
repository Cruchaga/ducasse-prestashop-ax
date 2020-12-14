using IntegracionPrestashopLib.Schema.dto;
using IntegracionPrestashopLib.Schema.ERP;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Operations
{
    public class ProductoDAL
    {
        /**
         * <summary>
         * Método para crear un Producto en PrestaShop
         * </summary>
         */
        public Boolean Create(Producto o)
        {
            return  true ;
        }

        /**
         * <summary>
         * Método que permite obtener un producto según filtros.
         * </summary>
         * <remarks>
         * Filtros aceptados: [id(string), nombre_exacto(string), nombre_coincide(string)]
         * </remarks>
        public Producto Get()
        {
            
            var ret = new OperacionResponse<Producto>
            {
                Descripcion = "Producto obtenido",
                Error = "0",
                Exitosa = true,
                obj = new List<Producto>
                {
                    new Producto
                    {
                        ColorId = "dum",
                        ConfigId = "",
                        InventDimId = "270006004-0054",
                        ItemBarCode = "7899842544560032",
                        ItemId = "13",
                        ItemImg = "/root/img",
                        ItemName = "Producto dummy Blanco dum",
                        ItemTxt = "Producto dummy creado con las intenciones de realizar pruebas de concepto con la dll que integrará los sistemas.",
                        NameAlias = "Dummy",
                        QtyMaxToBuy = 10,
                        SalesPrice = 17500,
                        SizeId = "Negro",
                        TaxPackagingQty = "1",
                        UnitId = "mt2",
                        Stock = 350
                    }
                }
            };
            var json = JsonConvert.SerializeObject(ret);
            ret.Descripcion = json;
            return ret;
        }
        
         */
        /**
         * <summary>
         * Método que permite actualizar tanto los atributos del producto como el stock
         * </summary>
         */
        public Boolean Update(Producto o)
        {
            return true;
        }

        public String TestDateTime(DateTime date)
        {
            return $"La fecha es: {date.ToString("dd 'de' MMMM 'de' yyyy con 'hora' HH:mm:ss FFF")}";
        }
    }
}
