using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;

namespace IntegracionEnviame.Bussiness.Clientes
{
    internal class EnviameClient
    {
        public static String RequestWithBody(String method, String url, String apiKey, String contentType, String body)
        {
            String response = Httprequest(method, url, apiKey,contentType, body);
            return response;
        }

        public static String RequestEmpty(String method, String url, String apiKey, String contentType)
        {
            String response = Httprequest(method, url, apiKey,contentType);
            return response;
        }

        public static String RequestWithForm(String method, String url, String apiKey, String contentType,  Dictionary<String, String> formData)
        {
            NameValueCollection formCollection = new NameValueCollection();
            foreach (var pair in formData)
            {
                formCollection[pair.Key]= pair.Value;
            }

            String response = HttpFormDataRequest(method, url, apiKey, formCollection, contentType );
            return response;
        }

        public static String Request(String url, String json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            String result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }

        private static String Httprequest(String method, String url, String authKey, String contentType, String content)
        {
            string response = "";
            //const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            //const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            //ServicePointManager.SecurityProtocol = Tls12;

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.ContentType = contentType;
            req.Method = method;
            req.Headers.Add("api-key", $"{authKey}");
            
            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                streamWriter.Write(content);
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            var sr = new StreamReader(resp.GetResponseStream());
            response = sr.ReadToEnd();
            sr.Close();
            resp.Close();
            return response;
        }

        private static String Httprequest(String method, String url, String authKey, String contentType)
        {
            try
            {
                string response = "";
                //const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
                //const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
                //ServicePointManager.SecurityProtocol = Tls12;

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.ContentType = contentType;
                req.Method = method;
                req.Headers.Add("api-key", $"{authKey}");
                req.Accept = contentType;
                req.KeepAlive = true;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                var sr = new StreamReader(resp.GetResponseStream());
                response = sr.ReadToEnd();
                sr.Close();
                resp.Close();

                return response;
            }
            catch (WebException e)
            {
                var sr = new StreamReader(e.Response.GetResponseStream());
                var errorContent = sr.ReadToEnd();
                sr.Close();
                return errorContent;
            }
        }

        private static String HttpFormDataRequest(String method, String url, String authKey, NameValueCollection formData, string contentType)
        {
            try
            {
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                System.Net.WebRequest webRequest = WebRequest.Create(url);

                webRequest.Method = method;
                webRequest.ContentType = contentType + boundary;

                //webRequest.Headers.Add(HttpRequestHeader.Accept, "application/json");
                webRequest.Headers.Add("api-key", authKey);

                Stream postDataStream = GetPostStream( formData, boundary);

                webRequest.ContentLength = postDataStream.Length;
                Stream reqStream = webRequest.GetRequestStream();
                postDataStream.Position = 0;

                byte[] buffer = new byte[1024];
                int bytesRead = 0;

                while ((bytesRead = postDataStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    reqStream.Write(buffer, 0, bytesRead);
                }

                postDataStream.Close();
                reqStream.Close();

                StreamReader sr = new StreamReader(webRequest.GetResponse().GetResponseStream());
                string Result = sr.ReadToEnd();
                sr.Close();
                return Result;
            }
            catch(WebException we)
            {
                var sr = new StreamReader(we.Response.GetResponseStream());
                var errorContent = sr.ReadToEnd();
                sr.Close();
                return errorContent;
            }
        }

        private static Stream GetPostStream( NameValueCollection formData, string boundary)
        {
            Stream postDataStream = new System.IO.MemoryStream();

            //adding form data
            string formDataHeaderTemplate = Environment.NewLine + "--" + boundary + Environment.NewLine +
            "Content-Disposition: form-data; name=\"{0}\";" + Environment.NewLine + Environment.NewLine + "{1}";

            foreach (string key in formData.Keys)
            {
                byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format(formDataHeaderTemplate,
                key, formData[key]));
                postDataStream.Write(formItemBytes, 0, formItemBytes.Length);
            }

            byte[] endBoundaryBytes = System.Text.Encoding.UTF8.GetBytes(Environment.NewLine + "--" + boundary + "--");
            postDataStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            return postDataStream;
        }
    }
}
