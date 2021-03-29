using IntegracionPrestashopLib.Bussiness.Clientes;
using IntegracionPrestashopLib.Bussiness.Util;
using IntegracionPrestashopLib.Schema.Dictionary;
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

        private PSRecursoClient<Schema.PS.ProductToUpdate[], ProductQueryResponse> psProducto = new PSRecursoClient<Schema.PS.ProductToUpdate[], ProductQueryResponse>("IntegracionPrestashopLib.Schema.PS.Responses.ProductQueryResponse");
        private PSRecursoClient<Schema.PS.StockAvailable, StockAvailableRepsonse> pSStock = new PSRecursoClient<Schema.PS.StockAvailable, StockAvailableRepsonse>("IntegracionPrestashopLib.Schema.PS.Responses.StockAvailableRepsonse");


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
            Schema.PS.Producto psProd = new Schema.PS.Producto();
            Parser.parsePSProdFromERP(producto, ref psProd);

            var res = PSProductoClient.CreateProduct(psProd, apiKey, baseUrl);
            if (res.Creado)
            {
                if (producto.Inventario != null)
                {
                    var stockRes = UpdateStock(producto.Inventario.Stock.ToString(), ((ProductResponse)res.Objeto).Producto.ProductId);
                    if (stockRes.Actualizado)
                    {
                        return new CreateResponse
                        {
                            Creado = true
                        };
                    }
                    else
                    {
                        return new CreateResponse
                        {
                            Creado = true,
                            ErrorCode = IntegracionErrorCode.BAD_CREATED,
                            Error = $"El producto se creó pero el stock no se ingresó como corresponde. Error interno = [{stockRes.Error}]."
                        };
                    }
                }
                else
                {
                    return new CreateResponse
                    {
                        Creado = true,
                        ErrorCode = IntegracionErrorCode.BAD_CREATED,
                        Error = $"El producto se creó pero el stock no se ingresó como corresponde. Error interno = El stock se encuentra vacío."
                    };
                }
            }
            else
            {
                return res;
            }
        }

        public object GetProducto(string itemId_InventDimId)
        {
            var prodResult = psProducto.GetRecursoWithFilter("products", itemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{itemId_InventDimId}]&display=full");
            if (prodResult == null || prodResult.products == null)
            {
                return new UpdateResponse
                {
                    Actualizado = false,
                    ErrorCode = IntegracionErrorCode.NOT_FOUND,
                    Error = $"El producto con referencia {itemId_InventDimId} no fué encontrado. Objeto prestashop: {psProducto.getJson(itemId_InventDimId)}"
                };
            }
            var prod = (Schema.PS.Producto)prodResult.products[0];
            var prodERP = new Producto();
            Parser.parseERPProdFromPS(prod, ref prodERP);
            return prodERP;
        }

        private UpdateResponse Merge(Producto producto)
        {
            try
            {
                var prodResult = psProducto.GetRecursoWithFilter("products", producto.ItemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{producto.ItemId_InventDimId}]&display=full");
                if (prodResult == null || prodResult.products == null)
                {
                    return new UpdateResponse
                    {
                        Actualizado = false,
                        ErrorCode = IntegracionErrorCode.NOT_FOUND,
                        Error = $"El producto con referencia {producto.ItemId_InventDimId} no fué encontrado. Objeto prestashop: {psProducto.getJson(producto.ItemId_InventDimId)}"
                    };
                }
                var prod = (Schema.PS.Producto)prodResult.products[0];
                Parser.parsePSProdFromERP(producto, ref prod);

                return PSProductoClient.UpdateProduct((Schema.PS.ProductToUpdate)prod, apiKey, baseUrl);
            }catch(Exception e)
            {
                return new UpdateResponse
                {
                    Actualizado = false,
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = $"No se logró actualizar el producto : {e.Message} {Environment.NewLine} StackTrace: {e.StackTrace}"
                };
            }
        }

        public UpdateResponse UpdatePrice(String itemId_InventDimId,  decimal netPrice)
        {
             try
            {
                var prodResult = psProducto.GetRecursoWithFilter("products", itemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{itemId_InventDimId}]&display=full");
                if (prodResult == null || prodResult.products == null)
                {
                    return new UpdateResponse
                    {
                        Actualizado = false,
                        ErrorCode = IntegracionErrorCode.NOT_FOUND,
                        Error = $"El producto con referencia {itemId_InventDimId} no fué encontrado. Objeto prestashop: {psProducto.getJson(itemId_InventDimId)}"
                    };
                }
                var prod = (Schema.PS.Producto)prodResult.products[0];
                var urlPrice = $"{baseUrl.Replace("api/", "WS/price.php")}";
                var parametros = new Dictionary<String, String>();
                parametros.Add("price", netPrice.ToString().Replace(",","."));
                parametros.Add("id_product", prod.ProductId.ToString());
                decimal signature = ((netPrice + 1) * 7) + (prod.ProductId * 17);
                parametros.Add("signature", signature.ToString().Replace(",", "."));

                return PSProductoClient.UpdatePrice(urlPrice, parametros);
            }catch(Exception e)
            {
                return new UpdateResponse
                {
                    Actualizado = false,
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = $"No se logró actualizar el producto : {e.Message} {Environment.NewLine} StackTrace: {e.StackTrace}"
                };
            }
        }
        /**
         * <summary>
         * Método que permite actualizar tanto los atributos del producto como el stock
         * </summary>
         */
        public UpdateResponse UpdateStock(Inventario stockProd)
        {
            try
            {
                var prod = psProducto.GetRecursoWithFilter("products", stockProd.ItemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{stockProd.ItemId_InventDimId}]&display=full");
                if (prod == null || prod.products == null)
                {
                    return new UpdateResponse
                    {
                        Actualizado = false,
                        ErrorCode = IntegracionErrorCode.NOT_FOUND,
                        Error = $"El producto con referencia {stockProd.ItemId_InventDimId} no fué encontrado. respuesta de PS: {psProducto.getJson(stockProd.ItemId_InventDimId)}"
                    };
                }
                return UpdateStock(stockProd.Stock.ToString().Replace(",", "."), prod.products[0].ProductId);
            }catch(Exception e)
            {
                return new UpdateResponse
                {
                    Actualizado = false,
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = $"No se logró actualizar el producto : {e.Message} {Environment.NewLine} StackTrace: {e.StackTrace}"
                };
            }
        }

        private UpdateResponse UpdateStock(String stockProd,  int prodID)
        {
            var stock = pSStock.GetRecursoWithFilter("stock_availables", prodID.ToString(), apiKey, baseUrl, $"&display=full&filter[id_product]=[{prodID}]");
            stock.Stock[0].Stock = stockProd;
            return PSProductoClient.UpdateStock(stock.Stock[0], apiKey, baseUrl);
        }

        public String TestDateTime(DateTime date)
        {
            return $"La fecha es: {date.ToString("dd 'de' MMMM 'de' yyyy con 'hora' HH:mm:ss FFF")}";
        }
    }
}
