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
        private PSRecursoClient<Schema.PS.Combination, CombinationsResponse> psCombinacion = new PSRecursoClient<Schema.PS.Combination, CombinationsResponse>("IntegracionPrestashopLib.Schema.PS.Responses.CombinationsResponse");
        private PSRecursoClient<Schema.PS.StockAvailable, StockAvailableRepsonse> pSStock = new PSRecursoClient<Schema.PS.StockAvailable, StockAvailableRepsonse>("IntegracionPrestashopLib.Schema.PS.Responses.StockAvailableRepsonse");


        public ProductoDAL(String apiKey, String prestashopUrlBase)
        {
            this.apiKey = apiKey;
            this.baseUrl = Functions.NormalizarUrlBase(prestashopUrlBase);
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

            CreateResponse res = PSProductoClient.CreateProduct(psProd, apiKey, baseUrl);
            if (res.Creado)
            {
                if (producto.Inventario != null)
                {
                    UpdateResponse stockRes = UpdateStock(producto.Inventario.Stock.ToString(), ((ProductResponse)res.Objeto).Product.ProductId);
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
            Traza sbTraza = new Traza();
            ProductQueryResponse prodResult = psProducto.GetRecursoWithFilter("products", itemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{itemId_InventDimId}]&display=full", sbTraza);
            if (prodResult == null || prodResult.products == null)
            {
                return new UpdateResponse
                {
                    Actualizado = false,
                    ErrorCode = IntegracionErrorCode.NOT_FOUND,
                    Error = $"El producto con referencia {itemId_InventDimId} no fué encontrado. Objeto prestashop: {psProducto.getJson(itemId_InventDimId)}"
                };
            }
            Schema.PS.Producto prod = (Schema.PS.Producto)prodResult.products[0];
            Producto prodERP = new Producto();
            Parser.parseERPProdFromPS(prod, ref prodERP);
            return prodERP;
        }

        private UpdateResponse Merge(Producto producto)
        {
            try
            {
                Traza sbTraza = new Traza();
                ProductQueryResponse prodResult = psProducto.GetRecursoWithFilter("products", producto.ItemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{producto.ItemId_InventDimId}]&display=full", sbTraza);
                if (prodResult == null || prodResult.products == null)
                {
                    return new UpdateResponse
                    {
                        Actualizado = false,
                        ErrorCode = IntegracionErrorCode.NOT_FOUND,
                        Error = $"El producto con referencia {producto.ItemId_InventDimId} no fué encontrado. Objeto prestashop: {psProducto.getJson(producto.ItemId_InventDimId)}"
                    };
                }
                Schema.PS.Producto prod = (Schema.PS.Producto)prodResult.products[0];
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
                Schema.PS.Producto prod;
                String urlPrice = "";
                Dictionary<String, String> parametros = new Dictionary<String, String>();

                Traza sbTraza = new Traza();
                ProductQueryResponse prodResult = psProducto.GetRecursoWithFilter("products", itemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{itemId_InventDimId}]&display=full", sbTraza);
                if (prodResult == null || prodResult.products == null)
                {
                    CombinationsResponse comb = psCombinacion.GetRecursoWithFilter("combinations", itemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{itemId_InventDimId}]&display=full", sbTraza);
                    if (comb == null || comb.products == null)
                    {

                        return new UpdateResponse
                        {
                            Actualizado = false,
                            ErrorCode = IntegracionErrorCode.NOT_FOUND,
                            Error = $"El producto con referencia {itemId_InventDimId} no fué encontrado. Objeto prestashop: {psProducto.getJson(itemId_InventDimId)}"
                        };
                    }
                    else
                    {
                        Schema.PS.Combination combProd = comb.products[0];
                        urlPrice = $"{baseUrl.Replace("api/", "WS/combination_price.php")}";

                        parametros.Add("price", Functions.ToInvariant(netPrice));
                        parametros.Add("id_product", combProd.ProductId.ToString());
                        parametros.Add("id_prod_attribute", combProd.CombinationId.ToString());
                        decimal signature = ((netPrice + 1) * 6) + (int.Parse(combProd.ProductId) * 17);
                        parametros.Add("signature", Functions.ToInvariant(signature));
                    }
                }
                else
                {
                    prod = prodResult.products[0];
                    urlPrice = $"{baseUrl.Replace("api/", "WS/price.php")}";

                    parametros.Add("price", Functions.ToInvariant(netPrice));
                    parametros.Add("id_product", prod.ProductId.ToString());
                    decimal signature = ((netPrice + 1) * 7) + (prod.ProductId * 17);
                    parametros.Add("signature", Functions.ToInvariant(signature));
                }
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
                Traza sbTraza = new Traza();

                ProductQueryResponse prod = psProducto.GetRecursoWithFilter("products", stockProd.ItemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{stockProd.ItemId_InventDimId}]&display=full", sbTraza);
                if (prod == null || prod.products == null)
                {
                    CombinationsResponse comb = psCombinacion.GetRecursoWithFilter("combinations", stockProd.ItemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{stockProd.ItemId_InventDimId}]&display=full", sbTraza);
                    if (comb == null || comb.products == null) {
                        return new UpdateResponse
                        {
                            Actualizado = false,
                            ErrorCode = IntegracionErrorCode.NOT_FOUND,
                            Error = $"El producto con referencia {stockProd.ItemId_InventDimId} no fué encontrado. respuesta de PS: {psProducto.getJson(stockProd.ItemId_InventDimId)}"
                        };
                    }
                    return UpdateStock(stockProd.Stock.ToString().Replace(",", "."), comb.products[0].ProductId, comb.products[0].CombinationId);
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

        private UpdateResponse UpdateStock(String stockProd, int prodID)
        {
            Traza sbTraza = new Traza();

            StockAvailableRepsonse stock = pSStock.GetRecursoWithFilter("stock_availables", prodID.ToString(), apiKey, baseUrl, $"&display=full&filter[id_product]=[{prodID}]", sbTraza);
            stock.Stock[0].Stock = stockProd;
            return PSProductoClient.UpdateStock(stock.Stock[0], apiKey, baseUrl);
        }
        private UpdateResponse UpdateStock(String stockProd,  String prodID, int combinationId)
        {
            Traza sbTraza = new Traza();

            StockAvailableRepsonse stock = pSStock.GetRecursoWithFilter("stock_availables", $"{prodID.ToString()}-{combinationId}", apiKey, baseUrl, $"&display=full&filter[id_product]=[{prodID}]&filter[id_product_attribute]=[{combinationId}]", sbTraza);
            stock.Stock[0].Stock = stockProd;
            return PSProductoClient.UpdateStock(stock.Stock[0], apiKey, baseUrl);
        }

        public InventarioResponse GetStock(String itemId_InventDimId)
        {
            try
            {
                Traza sbTraza = new Traza();
                ProductQueryResponse prod = psProducto.GetRecursoWithFilter("products", itemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{itemId_InventDimId}]&display=full", sbTraza);

                StockAvailableRepsonse stock = new StockAvailableRepsonse();
                int stockSalida = 0;
                if (prod == null || prod.products == null)
                {
                    CombinationsResponse comb = psCombinacion.GetRecursoWithFilter("combinations", itemId_InventDimId, apiKey, baseUrl, $"&filter[reference]=[{itemId_InventDimId}]&display=full", sbTraza);
                    if (comb == null || comb.products == null)
                    {
                        return new InventarioResponse
                        {
                            HayError = false,
                            ErrorCode = IntegracionErrorCode.NOT_FOUND,
                            Error = $"El producto con referencia {itemId_InventDimId} no fué encontrado. respuesta de PS: {psProducto.getJson(itemId_InventDimId)}",
                            Inventario = null
                        };
                    }
                    String prodID = comb.products[0].ProductId;
                    int combinationId = comb.products[0].CombinationId;
                    stock = pSStock.GetRecursoWithFilter("stock_availables", $"{prodID.ToString()}-{combinationId}", apiKey, baseUrl, $"&display=full&filter[id_product]=[{prodID}]&filter[id_product_attribute]=[{combinationId}]", sbTraza);
                }
                else
                {
                    int prodID = prod.products[0].ProductId;
                    stock = pSStock.GetRecursoWithFilter("stock_availables", prodID.ToString(), apiKey, baseUrl, $"&display=full&filter[id_product]=[{prodID}]", sbTraza);
                }

                if (int.TryParse(stock.Stock[0].Stock, out stockSalida))
                {
                    return new InventarioResponse
                    {
                        HayError = false,
                        Inventario = new Inventario
                        {
                            Stock = stockSalida,
                            ItemId_InventDimId = itemId_InventDimId
                        }
                    };
                }
                else
                {
                    return new InventarioResponse
                    {
                        HayError = true,
                        Error = $"El stock registrado no es un número entero. [Valor = '{stock.Stock[0].Stock}']",
                        ErrorCode = IntegracionErrorCode.BAD_ATTRIBUTE
                    };
                }
            }
            catch (Exception e)
            {
                return new InventarioResponse
                {
                    HayError = true,
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = $"No se logró obtener el stock : {e.Message} {Environment.NewLine} StackTrace: {e.StackTrace}"
                };
            }
        }

        public String TestDateTime(DateTime date)
        {
            return $"La fecha es: {date.ToString("dd 'de' MMMM 'de' yyyy con 'hora' HH:mm:ss FFF")}";
        }
    }
}
