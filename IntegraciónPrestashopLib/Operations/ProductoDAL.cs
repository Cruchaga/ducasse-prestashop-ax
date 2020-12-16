using IntegracionPrestashopLib.Bussiness.Clientes;
using IntegracionPrestashopLib.Schema.dto;
using IntegracionPrestashopLib.Schema.ERP;
using IntegracionPrestashopLib.Schema.PS.Responses;
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
        private String apiKey;
        private String baseUrl;


        public ProductoDAL(String apiKey, String prestashopUrlBase)
        {
            this.apiKey = apiKey;
            this.baseUrl = prestashopUrlBase;
        }
        /**
         * <summary>
         * Método para crear un Producto en PrestaShop
         * </summary>
         */
        public CreateResponse Create(Producto producto)
        {
            return  new CreateResponse
            {
                Creado = true
            };
        }

        /**
         * <summary>
         * Método que permite actualizar tanto los atributos del producto como el stock
         * </summary>
         */
        public UpdateResponse UpdateStock(Inventario stockProd)
        {
            PSRecursoClient<Schema.PS.StockAvailable, StockAvailableRepsonse> pSProducto = new PSRecursoClient<Schema.PS.StockAvailable, StockAvailableRepsonse>();
            var stock = pSProducto.GetRecursoWithFilter("stock_availables", stockProd.ProductoId.ToString(), apiKey, baseUrl, $"&display=full&filter[id_product]=[{stockProd.ProductoId}]");
            stock.Stock[0].Stock = stockProd.Stock.ToString();
            return PSProductoClient.UpdateStock(stock.Stock[0], apiKey, baseUrl);
        }

        public String TestDateTime(DateTime date)
        {
            return $"La fecha es: {date.ToString("dd 'de' MMMM 'de' yyyy con 'hora' HH:mm:ss FFF")}";
        }
    }
}
