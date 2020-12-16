using IntegracionPrestashopLib.Bussiness.Util;
using IntegracionPrestashopLib.Schema.Dictionary;
using IntegracionPrestashopLib.Schema.PS;
using IntegracionPrestashopLib.Schema.PS.Request;
using IntegracionPrestashopLib.Schema.PS.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Bussiness.Clientes
{
    internal class PSProductoClient
    {
        internal static UpdateResponse UpdateProduct(Producto orden, String apiKey, String prestashopUrlBase)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));

            String url = $"{prestashopUrlBase}products";
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;
            var obj = new ProductRequest
            {
                producto = orden
            };
            string xmlBody = Functions.SerializeXML(obj);

            try
            {
                HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);
                //Add these, as we're doing a POST
                req.ContentType = "application/xml";
                req.Method = "PUT";
                req.Headers.Add("Authorization", $"Basic {base64}");
                //We need to count how many bytes we're sending. 
                //Post'ed Faked Forms should be name=value&
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(xmlBody);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (resp == null) return null;
                System.IO.StreamReader sr =
                      new System.IO.StreamReader(resp.GetResponseStream());
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
                    Error = e.Message
                };
            }
        }

        internal static UpdateResponse UpdateStock(StockAvailable stockA, String apiKey, String prestashopUrlBase)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));

            String url = $"{prestashopUrlBase}stock_availables/schema=blank";
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;
            var obj = new StockRequest
            {
                Stock = stockA
            };
            string xmlBody = Functions.SerializeXML(obj);

            try
            {
                HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);
                //Add these, as we're doing a POST
                req.ContentType = "application/xml";
                req.Method = "PUT";
                req.Headers.Add("Authorization", $"Basic {base64}");
                //We need to count how many bytes we're sending. 
                //Post'ed Faked Forms should be name=value&
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(xmlBody);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                if (resp == null) return null;
                System.IO.StreamReader sr =
                      new System.IO.StreamReader(resp.GetResponseStream());
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
                    Error = e.Message
                };
            }
        }

        
    }
}
