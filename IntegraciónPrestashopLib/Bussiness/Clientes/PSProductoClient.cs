using IntegracionPrestashopLib.Bussiness.Util;
using IntegracionPrestashopLib.Schema.Dictionary;
using IntegracionPrestashopLib.Schema.PS;
using IntegracionPrestashopLib.Schema.PS.Request;
using IntegracionPrestashopLib.Schema.PS.Responses;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Security.Authentication;

namespace IntegracionPrestashopLib.Bussiness.Clientes
{
    internal class PSProductoClient
    {

        internal static CreateResponse CreateProduct(Producto orden, String apiKey, String prestashopUrlBase)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));
            String url = $"{prestashopUrlBase}products?output_format=JSON";
            var obj = new ProductRequest
            {
                producto = orden
            };
            string xmlBody = Functions.SerializeXML(obj);
            string method = "POST";
            try
            {
                string response = HttpRequestFunctions.Request(base64, url, xmlBody, method);
                return new CreateResponse
                {
                    Creado = true,
                    Objeto = JsonConvert.DeserializeObject<ProductResponse>(response)
                };
            }
            catch (WebException wex)
            {
                var sr = new StreamReader(wex.Response.GetResponseStream());
                var pageContent = sr.ReadToEnd();
                sr.Close();
                return new CreateResponse
                {
                    Creado = false,
                    Error = pageContent,
                    ErrorCode = IntegracionErrorCode.PRESTASHOP_INTERNAL_ERROR
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Excepcion: {e.Message}");
                return new CreateResponse
                {
                    Creado = false,
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = e.Message
                };
            }
        }

        internal static UpdateResponse UpdateProduct(ProductToUpdate orden, String apiKey, String prestashopUrlBase)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));
            String url = $"{prestashopUrlBase}products";
            string method = "PUT";
            var obj = new ProductUpdateRequest
            {
                producto = orden
            };
            string xmlBody = Functions.SerializeXML(obj);

            try
            {
                string response = HttpRequestFunctions.Request(base64, url, xmlBody, method);

                return new UpdateResponse
                {
                    Actualizado = true
                };
            }
            catch (WebException wex)
            {
                var sr = new StreamReader(wex.Response.GetResponseStream());
                var pageContent = sr.ReadToEnd();
                sr.Close();
                return new UpdateResponse
                {
                    Actualizado = false,
                    Error = pageContent,
                    ErrorCode = IntegracionErrorCode.PRESTASHOP_INTERNAL_ERROR
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Excepcion: {e.Message}");
                return new UpdateResponse
                {
                    Actualizado = false,
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = e.Message
                };
            }
        }

        internal static UpdateResponse UpdateStock(StockAvailable stockA, String apiKey, String prestashopUrlBase)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));

            String url = $"{prestashopUrlBase}stock_availables/schema=blank";
            var obj = new StockRequest
            {
                Stock = stockA
            };
            string xmlBody = Functions.SerializeXML(obj);
            string method = "PUT";
            try
            {
                string response = HttpRequestFunctions.Request(base64, url, xmlBody, method);

                return new UpdateResponse
                {
                    Actualizado = true
                };
            }
            catch (WebException wex)
            {
                var sr = new StreamReader(wex.Response.GetResponseStream());
                var pageContent = sr.ReadToEnd();
                sr.Close();
                return new UpdateResponse
                {
                    Actualizado = false,
                    Error = pageContent,
                    ErrorCode = IntegracionErrorCode.PRESTASHOP_INTERNAL_ERROR
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"Excepcion: {e.Message}");
                return new UpdateResponse
                {
                    Actualizado = false,
                    ErrorCode = IntegracionErrorCode.UNCONTROLATED,
                    Error = $"Error: {e.Message} {Environment.NewLine} {e.StackTrace}"
                };
            }
        }

        
    }
}
