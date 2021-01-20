using IntegracionPrestashopLib.Bussiness.Util;
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
        private Hashtable jsonResources = new Hashtable();
        internal V GetRecurso(String recurso, String recursoId, String apiKey, String baseUrl)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (resources == null)
                resources = new Hashtable();
            if (resources[recursoId] == null)
                ObtenerRecurso(recurso, $"/{recursoId}", apiKey, baseUrl, "");
            else
                Console.WriteLine($"Recurso {recursoId} enconctrado en lista de {recurso} con {resources.Count} recursos guardados");
            sw.Stop();
            Console.WriteLine($"Tiempo {recurso}: {sw.Elapsed.TotalMilliseconds} ms");
            return (V)resources[$"/{recursoId}"];
        }
        internal V GetRecursoWithFilter(String recurso, String recursoId, String apiKey, String baseUrl, String filtro)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (resources == null)
                resources = new Hashtable();
            if (resources[recursoId] == null)
                ObtenerRecursoWithFilter(recurso, recursoId, apiKey, baseUrl, filtro);
            else
                Console.WriteLine($"Recurso {recursoId} enconctrado en lista de {recurso} con {resources.Count} recursos guardados");
            sw.Stop();
            Console.WriteLine($"Tiempo {recurso}: {sw.Elapsed.TotalMilliseconds} ms");
            return (V)resources[recursoId];
        }

        private void ObtenerRecurso(String recurso, string recursoId, String apiKey, String baseUrl, String filtros)
        {
            String base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));
            String url = $"{baseUrl}{recurso}{recursoId}?output_format=JSON{filtros}";
                
            try
            {
                string s = HttpRequestFunctions.GetRequest(url, base64);

                V resp = JsonConvert.DeserializeObject<V>(s);

                if (resources == null)
                    resources = new Hashtable();
                if (resources[recursoId] == null)
                {
                    resources.Add(recursoId, resp);
                    Console.WriteLine($"recurso {recurso} agregado  con id {recursoId}");
                }
                if (jsonResources[recursoId] == null)
                {
                    jsonResources.Add(recursoId, s);
                }
            }catch(WebException e)
            {
                var sr = new StreamReader(e.Response.GetResponseStream());
                var pageContent = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception e)
            {
                throw new Exception($"Error detectado al intentar consumir la url {url} con apikey {base64}", e);
            }
        }

        internal String getJson(String id)
        {
            return jsonResources[id].ToString();
        }

        private void ObtenerRecursoWithFilter(String recurso, string recursoId, String apiKey, String baseUrl, String filtros)
        {
            String url = "";
            String base64 = "";
            try
            {
                base64 = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes($"{apiKey}:"));
                url = $"{baseUrl}{recurso}?output_format=JSON{filtros}";
                string s = HttpRequestFunctions.GetRequest(url, base64);
                s = s == ("0 results\r\n") ? "{}" : s;
                V resp = JsonConvert.DeserializeObject<V>(s);

                if (resources == null)
                    resources = new Hashtable();
                if (resources[recursoId] == null)
                {
                    Console.WriteLine($"Agregando {resp.ToString()} [{s}] a lista... pues se encontró vacío = {{{resources[recursoId]}}} ");
                    resources.Add(recursoId, resp);
                    Console.WriteLine($"recurso {recurso} agregado. qty de recursos: {resources.Count}");
                }

            }
            catch (TimeoutException t)
            {
                throw new Exception($"Timeout detectado al intentar consumir la url {url} con apikey {base64}", t);
            }
            catch (Exception e)
            {
                throw new Exception($"Error detectado al intentar consumir la url {url} con apikey {base64}", e);
            }
        }

    }
}
