using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;

namespace IntegracionPrestashopLib.Bussiness.Util
{
    internal class HttpRequestFunctions
    {

        internal static string GetRequest(string url, string base64)
        {
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;

            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Authorization, $"Basic {base64}");
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            string s = reader.ReadToEnd();
            data.Close();
            reader.Close();
            return s;
        }


        internal static string Request(string base64, string url, string xmlBody, string method)
        {
            string response = "";
            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ContentType = "application/xml";
            req.Method = method;
            req.Headers.Add("Authorization", $"Basic {base64}");
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(xmlBody);
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            var sr = new System.IO.StreamReader(resp.GetResponseStream());
            response = sr.ReadToEnd();
            sr.Close();
            resp.Close();
            return response;
        }

    }
}
