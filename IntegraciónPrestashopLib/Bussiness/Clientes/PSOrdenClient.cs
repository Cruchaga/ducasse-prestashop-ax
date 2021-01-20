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
        internal static OrdenesResponse GetOrdenes(DateTime fechaIni, DateTime fechaFin, String apiKey, String prestashopUrlBase)
        {

            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));

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
                return new OrdenesResponse
                {
                    Orders = new List<OrdenDeCompra>()
                };
            }
        }

        internal static UpdateResponse UpdateTracking(OrderHistory orden, String apiKey, String prestashopUrlBase)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));

            String url = $"{prestashopUrlBase}order_histories/{orden.OrderId}";
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
