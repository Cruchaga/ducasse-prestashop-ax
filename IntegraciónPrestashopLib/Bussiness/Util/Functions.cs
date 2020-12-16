using System;
using System.Collections.Generic;
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
