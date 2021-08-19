using IntegracionPrestashopLib.Bussiness.Util;
using IntegracionPrestashopLib.Schema.Dictionary;
using IntegracionPrestashopLib.Schema.PS;
using IntegracionPrestashopLib.Schema.PS.Request;
using IntegracionPrestashopLib.Schema.PS.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Bussiness.Clientes
{
    internal class PSOrdenClient
    {
        internal static OrdenesResponse GetOrdenes(DateTime fechaIni, DateTime fechaFin, String apiKey, String prestashopUrlBase, ref StringBuilder sbTraza)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));
            sbTraza.Append($"Consultando ordenes desde {fechaIni.ToString("dd/MM/yyyy HH:mm:ss")} hasta {fechaFin.ToString("dd/MM/yyyy HH:mm:ss")} {Environment.NewLine}");

            String url = $"{prestashopUrlBase}orders?display=full&date=1&filter[current_state]=[3]&filter[date_add]=[{fechaIni.ToString("yyyy-MM-dd'%20'HH:mm:ss")},{fechaFin.ToString("yyyy-MM-dd'%20'HH:mm:ss")}]&output_format=JSON";
            
            try
            {

                string s = HttpRequestFunctions.GetRequest(url, base64);
                OrdenesResponse resp = new OrdenesResponse
                {
                    Orders = new List<OrdenDeCompra>()
                };
                if (!s.Equals("[]"))
                    resp = JsonConvert.DeserializeObject<OrdenesResponse>(s);

                resp.Json = s;

                return resp;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Excepcion: {e.Message}");
                sbTraza.Append($"Error al obtener ordenes [{e.Message}]{Environment.NewLine}");
                return new OrdenesResponse
                {
                    Orders = new List<OrdenDeCompra>(),
                    
                };
            }
        }

        internal static PaymentResponse GetOrderPayment(String prestashopUrl, Dictionary<String, String> parametros)
        {

            string method = "GET";

            StringBuilder sb = new StringBuilder(prestashopUrl);
            int count = 0;
            foreach(var dic in parametros)
            {
                if (count == 0)
                    sb.Append("?");
                else
                    sb.Append("&");

                sb.Append($"{dic.Key}={dic.Value}");
                count++;
            }
            string response = HttpRequestFunctions.Request(sb.ToString(), "", method);
            
            var payment =(response != "0 results\r\n")? JsonConvert.DeserializeObject<PaymentResponse>(response): null;
            return payment;
            
        }

        internal static CartRulesResponse[] GetCartRules(String prestashopUrl, Dictionary<String, String> parametros)
        {

            string method = "GET";

            StringBuilder sb = new StringBuilder(prestashopUrl);
            int count = 0;
            foreach(var dic in parametros)
            {
                if (count == 0)
                    sb.Append("?");
                else
                    sb.Append("&");

                sb.Append($"{dic.Key}={dic.Value}");
                count++;
            }
            string response = HttpRequestFunctions.Request(sb.ToString(), "", method);
            if (response.StartsWith("["))
            {
                var cartRules = JsonConvert.DeserializeObject<CartRulesResponse[]>(response);
                return cartRules;
            }
            else
                return null;
            
        }
        internal static UpdateResponse UpdateTracking(OrderHistory orden, String apiKey, String prestashopUrlBase)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));
            String sendmail = orden.Informar ? "1" : "0";
            String url = $"{prestashopUrlBase}order_histories/{orden.OrderId}?sendemail={sendmail}";
            string method = "POST";

            var obj = new TrackingRequest
            {
                Tracking = orden
            };
            String track = Functions.SerializeXML(obj);
            try
            {
                var response = HttpRequestFunctions.Request(base64, url, track, method);
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
                    Error = $"Error al actualizar tracking : {e.Message}"
                };
            }
        }

    }

}
