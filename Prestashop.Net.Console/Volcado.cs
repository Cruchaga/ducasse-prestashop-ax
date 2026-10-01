using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Prestashop.Net.Consola
{
    /// <summary>
    /// Vuelca a JSON cualquier respuesta de la DLL usando solo su API pública (propiedades y el patrón
    /// XxxCount + GetXxx(i)), tal como la recorre el X++. Sirve para archivar evidencias de las pruebas.
    /// </summary>
    internal static class Volcado
    {
        public static string Json(object o, bool incluirTraza)
        {
            StringBuilder sb = new StringBuilder();
            Escribir(sb, o, 0, incluirTraza);
            return sb.ToString();
        }

        private static void Sangria(StringBuilder sb, int n)
        {
            sb.Append('\n').Append(' ', n * 2);
        }

        private static void Escribir(StringBuilder sb, object v, int nivel, bool traza)
        {
            if (v == null) { sb.Append("null"); return; }
            if (v is string) { Texto(sb, (string)v); return; }
            if (v is bool) { sb.Append((bool)v ? "true" : "false"); return; }
            if (v is DateTime) { DateTime f = (DateTime)v; Texto(sb, f.Year < 1900 ? "" : f.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)); return; }
            if (v is IFormattable && v.GetType().IsPrimitive || v is decimal) { sb.Append(((IFormattable)v).ToString(null, CultureInfo.InvariantCulture)); return; }

            Type t = v.GetType();
            sb.Append('{');
            bool primero = true;
            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.GetIndexParameters().Length > 0) continue;
                if (p.Name == "Traza" && !traza) continue;
                if (p.Name == "Secreto" || p.Name == "BasicAuthClave" || p.Name == "Configuracion") continue;
                object valor = p.GetValue(v, null);
                if (!primero) sb.Append(',');
                primero = false;
                Sangria(sb, nivel + 1);
                if (p.Name.EndsWith("Count") && p.PropertyType == typeof(int))
                {
                    string plural = p.Name.Substring(0, p.Name.Length - 5);
                    MethodInfo get = Buscar(t, plural);
                    if (get != null)
                    {
                        Texto(sb, plural);
                        sb.Append(": [");
                        int n = (int)valor;
                        for (int i = 0; i < n; i++)
                        {
                            if (i > 0) sb.Append(',');
                            Sangria(sb, nivel + 2);
                            Escribir(sb, get.Invoke(v, new object[] { i }), nivel + 2, traza);
                        }
                        if (n > 0) Sangria(sb, nivel + 1);
                        sb.Append(']');
                        continue;
                    }
                }
                Texto(sb, p.Name);
                sb.Append(": ");
                Escribir(sb, valor, nivel + 1, traza);
            }
            Sangria(sb, nivel);
            sb.Append('}');
        }

        /// <summary>Envios → GetEnvio, Ordenes → GetOrden, Resultado → GetResultado.</summary>
        private static MethodInfo Buscar(Type t, string plural)
        {
            string[] candidatos = { plural, plural.TrimEnd('s'), plural.EndsWith("es") ? plural.Substring(0, plural.Length - 2) : plural };
            foreach (string c in candidatos)
            {
                MethodInfo m = t.GetMethod("Get" + c, new Type[] { typeof(int) });
                if (m != null) return m;
            }
            return null;
        }

        private static void Texto(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                if (c == '"') sb.Append((char)92).Append('"');
                else if (c == (char)92) sb.Append((char)92).Append((char)92);
                else if (c == '\n') sb.Append((char)92).Append('n');
                else if (c == '\r') sb.Append((char)92).Append('r');
                else if (c == '\t') sb.Append((char)92).Append('t');
                else if (c < ' ') sb.Append(' ');
                else sb.Append(c);
            }
            sb.Append('"');
        }
    }
}
