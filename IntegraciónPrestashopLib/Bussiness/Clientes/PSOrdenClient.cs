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
        private const string NoResults = "0 results\r\n";

        /// <summary>Arma el header Basic (apiKey como usuario, sin password) en base64.</summary>
        private static string BuildAuthHeader(string apiKey)
        {
            return Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));
        }

        internal static OrdenesResponse GetOrdenes(DateTime fechaIni, DateTime fechaFin, String apiKey, String prestashopUrlBase, Traza sbTraza)
        {
            string base64 = BuildAuthHeader(apiKey);
            sbTraza.Append($"Consultando ordenes desde {fechaIni.ToString("dd/MM/yyyy HH:mm:ss")} hasta {fechaFin.ToString("dd/MM/yyyy HH:mm:ss")} {Environment.NewLine}");

            string url = $"{prestashopUrlBase}orders?display=full&date=1&filter[current_state]=[2|3]&filter[date_add]=[{fechaIni.ToString("yyyy-MM-dd'%20'HH:mm:ss")},{fechaFin.ToString("yyyy-MM-dd'%20'HH:mm:ss")}]&output_format=JSON";
            sbTraza.Append($"URL:  {url}  {Environment.NewLine}");

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
                sbTraza.Log($"Excepcion al obtener ordenes: {e.Message}");
                return new OrdenesResponse
                {
                    Orders = new List<OrdenDeCompra>(),
                    HuboError = true,
                    MensajeError = e.Message
                };
            }
        }

        internal static PaymentResponse GetOrderPayment(String prestashopUrl, Dictionary<String, String> parametros)
        {
            string url = Functions.BuildQueryUrl(prestashopUrl, parametros);
            string response = HttpRequestFunctions.Request(url, "", "GET");

            PaymentResponse payment = (response != NoResults)
                ? JsonConvert.DeserializeObject<PaymentResponse>(response)
                : null;
            return payment;
        }

        internal static BanchilePayment GetBanchilePayment(String prestashopUrl, Dictionary<String, String> parametros)
        {
            string url = Functions.BuildQueryUrl(prestashopUrl, parametros);
            string response = HttpRequestFunctions.Request(url, "", "GET");

            BanchilePayment payment = (response != NoResults && response.TrimStart().StartsWith("{"))
                ? JsonConvert.DeserializeObject<BanchilePayment>(response)
                : null;
            return payment;
        }

        internal static CartRulesResponse[] GetCartRules(String prestashopUrl, Dictionary<String, String> parametros)
        {
            string url = Functions.BuildQueryUrl(prestashopUrl, parametros);
            string response = HttpRequestFunctions.Request(url, "", "GET");

            if (response.StartsWith("["))
            {
                CartRulesResponse[] cartRules = JsonConvert.DeserializeObject<CartRulesResponse[]>(response);
                return cartRules;
            }

            return null;
        }

        internal static UpdateResponse UpdateTracking(OrderHistory orden, String apiKey, String prestashopUrlBase)
        {
            string base64 = BuildAuthHeader(apiKey);
            string sendmail = orden.Informar ? "1" : "0";
            string url = $"{prestashopUrlBase}order_histories/{orden.OrderId}?sendemail={sendmail}";
            string method = "POST";

            TrackingRequest obj = new TrackingRequest
            {
                Tracking = orden
            };
            string track = Functions.SerializeXML(obj);
            try
            {
                string response = HttpRequestFunctions.Request(base64, url, track, method);
                return new UpdateResponse
                {
                    Actualizado = true
                };
            }
            catch (WebException wex)
            {
                StreamReader sr = new StreamReader(wex.Response.GetResponseStream());
                string pageContent = sr.ReadToEnd();
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
