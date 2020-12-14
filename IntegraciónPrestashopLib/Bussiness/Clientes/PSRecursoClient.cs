using IntegracionPrestashopLib.Schema.ERP;
using IntegracionPrestashopLib.Schema.PS.Responses;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Authentication;
using System.Text;

namespace IntegracionPrestashopLib.Bussiness.Clientes
{
    internal class PSRecursoClient<T, V>
    {
        private Hashtable resources;
        internal V GetRecurso(String recurso, String currencyId, String apiKey, String baseUrl)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (resources == null)
                resources = new Hashtable();
            if (resources[currencyId] == null)
                ObtenerRecurso(recurso, currencyId, apiKey, baseUrl);
            sw.Stop();
            Console.WriteLine($"Tiempo {recurso}: {sw.Elapsed.TotalMilliseconds} ms");
            return (V)resources[currencyId];
        }

        private void ObtenerRecurso(String recurso, string clientId, String apiKey, String baseUrl)
        {
            try
            {
                String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(apiKey));

                String url = $"{baseUrl}{recurso}/{clientId}?output_format=JSON&ws_key={apiKey}";
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
                    V resp = JsonConvert.DeserializeObject<V>(s);

                    if (resources == null)
                        resources = new Hashtable();
                    if (resources[clientId] == null)
                        resources.Add(clientId, resp);
                }
                catch (Exception)
                {

                    return;
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

    }
}
