using IntegracionPrestashopLib.Schema.PS;
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

namespace IntegracionPrestashopLib.Bussiness.Clientes
{
    internal class PSOrdenClient
    {
        internal static OrdenesResponse GetOrdenes(DateTime fechaIni, DateTime fechaFin, String apiKey, String prestashopUrlBase)
        {

            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(apiKey));

            String url = $"{prestashopUrlBase}orders?display=full&date=1&filter[date_add]=[{fechaIni.ToString("yyyy-MM-dd'%20'HH:mm:ss")},{fechaFin.ToString("yyyy-MM-dd'%20'HH:mm:ss")}]&output_format=JSON&ws_key={apiKey}";
            // Get the response.
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;

            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Basic {base64}");
            
            // Add a user agent header in case the
            // requested URI contains a query.

            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

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
    }

}
