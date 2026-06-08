using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Bussiness.Util
{
    internal class Functions
    {
        internal static String GetNumberFromRut(String rut)
        {
            return "";//TODO:IMPLEMENTAR
        }

        internal static string GetDvFromRut(string rutEmpresa)
        {
            return "";
        }

        /// <summary>
        /// Parsea un decimal usando SIEMPRE cultura invariante (punto como separador
        /// decimal, como entrega PrestaShop). Evita depender del locale de la máquina.
        /// </summary>
        internal static decimal ParseDecimal(string valor)
        {
            return decimal.Parse(valor, NumberStyles.Number, CultureInfo.InvariantCulture);
        }

        /// <summary>Formatea un decimal con cultura invariante (punto decimal).</summary>
        internal static string ToInvariant(decimal valor)
        {
            return valor.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Arma una URL agregando los parámetros como query string. Centraliza el patrón
        /// duplicado de construir "base?clave=valor&amp;clave=valor".
        /// </summary>
        internal static string BuildQueryUrl(string baseUrl, IDictionary<string, string> parametros)
        {
            if (parametros == null || parametros.Count == 0)
            {
                return baseUrl;
            }

            string query = string.Join("&", parametros.Select(p => $"{p.Key}={p.Value}").ToArray());
            return $"{baseUrl}?{query}";
        }

        internal static string SerializeXML<T>(T obj)
        {
            String xml = "";
            var serializer = new XmlSerializer(typeof(T));

            using (var sww = new StringWriter())
            {
                var enc = new UTF8Encoding(false);
                using (var memStm = new MemoryStream())
                using (var xw = XmlWriter.Create(memStm))
                {
                    serializer.Serialize(xw, obj);
                    var utf8 = memStm.ToArray();

                    xml = Encoding.UTF8.GetString(utf8).Trim();
                }
            }

            return xml;
        }
    }
}
