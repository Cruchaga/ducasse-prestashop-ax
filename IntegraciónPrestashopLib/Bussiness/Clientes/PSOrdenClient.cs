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

            String url = $"{prestashopUrlBase}orders?display=full&date=1&filter[date_add]=[{fechaIni.ToString("yyyy-MM-dd'%20'HH:mm:ss")},{fechaFin.ToString("yyyy-MM-dd'%20'HH:mm:ss")}]&output_format=JSON";
            // Get the response.
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;

            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Basic {base64}");
            
            // Add a user agent header in case the
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            try
            {
                Stream data = client.OpenRead(url);
                StreamReader reader = new StreamReader(data);
                string s = reader.ReadToEnd();
                //Console.WriteLine(s);
                data.Close();
                reader.Close();
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
            // Get the response.
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;


            // Add a user agent header in case the
            // requested URI contains a query.
            var obj = new TrackingRequest
            {
                Tracking = orden
            };
            String track = Functions.SerializeXML(obj);
            try
            {
                HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);
                //Add these, as we're doing a POST
                req.ContentType = "application/xml";
                req.Method = "POST";
                req.Headers.Add("Authorization", $"Basic {base64}");
                //We need to count how many bytes we're sending. 
                //Post'ed Faked Forms should be name=value&
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(track);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (resp == null) return null;
                System.IO.StreamReader sr =
                      new System.IO.StreamReader(resp.GetResponseStream());
                resp.Close();
                sr.Close();
                return new UpdateResponse
                {
                    Actualizado = true
                };
            }
            catch (WebException wex)
            {
                var pageContent = new StreamReader(wex.Response.GetResponseStream())
                                      .ReadToEnd();
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
