using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Prestashop.Net.Interno
{
    /// <summary>
    /// JSON mínimo y sin dependencias (lector y escritor) para el protocolo de dapintegracion.
    /// </summary>
    /// <remarks>
    /// prestashop-net.dll no referencia Newtonsoft.Json a propósito: AX carga varias DLL en el mismo
    /// proceso y una sola carpeta no puede tener dos versiones de Newtonsoft.Json.dll (la DLL 1.46 usa
    /// la 12.0.3, que además tiene una vulnerabilidad conocida de desbordamiento por anidamiento).
    /// Este lector limita la profundidad y solo produce: Dictionary&lt;string, object&gt;, List&lt;object&gt;,
    /// string, decimal (o double si no cabe), bool y null.
    /// </remarks>
    internal static class Json
    {
        private const int ProfundidadMaxima = 64;
        private const char Barra = (char)92; // barra invertida

        // ------------------------------------------------------------------ lectura

        public static object Leer(string texto)
        {
            if (texto == null) throw new FormatException("JSON vacío.");
            int i = 0;
            object valor = LeerValor(texto, ref i, 0);
            Saltar(texto, ref i);
            if (i != texto.Length) throw new FormatException("JSON con contenido extra en la posición " + i + ".");
            return valor;
        }

        public static Dictionary<string, object> LeerObjeto(string texto)
        {
            Dictionary<string, object> d = Leer(texto) as Dictionary<string, object>;
            if (d == null) throw new FormatException("Se esperaba un objeto JSON.");
            return d;
        }

        private static void Saltar(string s, ref int i)
        {
            while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r')) i++;
        }

        private static object LeerValor(string s, ref int i, int nivel)
        {
            if (nivel > ProfundidadMaxima) throw new FormatException("JSON demasiado anidado.");
            Saltar(s, ref i);
            if (i >= s.Length) throw new FormatException("JSON truncado.");
            char c = s[i];
            if (c == '{') return LeerObjeto(s, ref i, nivel);
            if (c == '[') return LeerLista(s, ref i, nivel);
            if (c == '"') return LeerTexto(s, ref i);
            if (c == 't' && Sigue(s, i, "true")) { i += 4; return true; }
            if (c == 'f' && Sigue(s, i, "false")) { i += 5; return false; }
            if (c == 'n' && Sigue(s, i, "null")) { i += 4; return null; }
            if (c == '-' || (c >= '0' && c <= '9')) return LeerNumero(s, ref i);
            throw new FormatException("Carácter inesperado '" + c + "' en la posición " + i + ".");
        }

        private static bool Sigue(string s, int i, string palabra)
        {
            return string.CompareOrdinal(s, i, palabra, 0, palabra.Length) == 0;
        }

        private static Dictionary<string, object> LeerObjeto(string s, ref int i, int nivel)
        {
            Dictionary<string, object> d = new Dictionary<string, object>(StringComparer.Ordinal);
            i++; // {
            Saltar(s, ref i);
            if (i < s.Length && s[i] == '}') { i++; return d; }
            while (true)
            {
                Saltar(s, ref i);
                if (i >= s.Length || s[i] != '"') throw new FormatException("Se esperaba una clave en la posición " + i + ".");
                string clave = LeerTexto(s, ref i);
                Saltar(s, ref i);
                if (i >= s.Length || s[i] != ':') throw new FormatException("Se esperaba ':' en la posición " + i + ".");
                i++;
                d[clave] = LeerValor(s, ref i, nivel + 1);
                Saltar(s, ref i);
                if (i >= s.Length) throw new FormatException("Objeto JSON sin cerrar.");
                if (s[i] == ',') { i++; continue; }
                if (s[i] == '}') { i++; return d; }
                throw new FormatException("Se esperaba ',' o '}' en la posición " + i + ".");
            }
        }

        private static List<object> LeerLista(string s, ref int i, int nivel)
        {
            List<object> l = new List<object>();
            i++; // [
            Saltar(s, ref i);
            if (i < s.Length && s[i] == ']') { i++; return l; }
            while (true)
            {
                l.Add(LeerValor(s, ref i, nivel + 1));
                Saltar(s, ref i);
                if (i >= s.Length) throw new FormatException("Lista JSON sin cerrar.");
                if (s[i] == ',') { i++; continue; }
                if (s[i] == ']') { i++; return l; }
                throw new FormatException("Se esperaba ',' o ']' en la posición " + i + ".");
            }
        }

        private static string LeerTexto(string s, ref int i)
        {
            StringBuilder sb = new StringBuilder();
            i++; // comilla de apertura
            while (i < s.Length)
            {
                char c = s[i++];
                if (c == '"') return sb.ToString();
                if (c != Barra) { sb.Append(c); continue; }
                if (i >= s.Length) break;
                char e = s[i++];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        if (i + 4 > s.Length) throw new FormatException("Escape unicode truncado.");
                        sb.Append((char)int.Parse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        i += 4;
                        break;
                    default: sb.Append(e); break; // comilla, barra, barra invertida
                }
            }
            throw new FormatException("Texto JSON sin cerrar.");
        }

        private static object LeerNumero(string s, ref int i)
        {
            int inicio = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            string n = s.Substring(inicio, i - inicio);
            decimal dec;
            if (decimal.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out dec)) return dec;
            double dob;
            if (double.TryParse(n, NumberStyles.Float, CultureInfo.InvariantCulture, out dob)) return dob;
            throw new FormatException("Número inválido '" + n + "'.");
        }

        // ------------------------------------------------------------------ escritura

        public static string Escribir(object valor)
        {
            StringBuilder sb = new StringBuilder();
            EscribirValor(sb, valor);
            return sb.ToString();
        }

        private static void EscribirValor(StringBuilder sb, object v)
        {
            if (v == null) { sb.Append("null"); return; }
            string texto = v as string;
            if (texto != null) { EscribirTexto(sb, texto); return; }
            if (v is bool) { sb.Append((bool)v ? "true" : "false"); return; }
            if (v is int || v is long || v is short || v is byte) { sb.Append(Convert.ToInt64(v, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture)); return; }
            if (v is decimal) { sb.Append(((decimal)v).ToString(CultureInfo.InvariantCulture)); return; }
            if (v is double || v is float) { sb.Append(Convert.ToDouble(v, CultureInfo.InvariantCulture).ToString("R", CultureInfo.InvariantCulture)); return; }
            if (v is DateTime) { EscribirTexto(sb, ((DateTime)v).ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)); return; }
            Dictionary<string, object> d = v as Dictionary<string, object>;
            if (d != null)
            {
                sb.Append('{');
                bool primero = true;
                foreach (KeyValuePair<string, object> par in d)
                {
                    if (!primero) sb.Append(',');
                    primero = false;
                    EscribirTexto(sb, par.Key);
                    sb.Append(':');
                    EscribirValor(sb, par.Value);
                }
                sb.Append('}');
                return;
            }
            System.Collections.IEnumerable lista = v as System.Collections.IEnumerable;
            if (lista != null)
            {
                sb.Append('[');
                bool primero = true;
                foreach (object item in lista)
                {
                    if (!primero) sb.Append(',');
                    primero = false;
                    EscribirValor(sb, item);
                }
                sb.Append(']');
                return;
            }
            EscribirTexto(sb, Convert.ToString(v, CultureInfo.InvariantCulture));
        }

        private static void EscribirTexto(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                if (c == '"') { sb.Append(Barra).Append('"'); }
                else if (c == Barra) { sb.Append(Barra).Append(Barra); }
                else if (c == '\n') { sb.Append(Barra).Append('n'); }
                else if (c == '\r') { sb.Append(Barra).Append('r'); }
                else if (c == '\t') { sb.Append(Barra).Append('t'); }
                else if (c < ' ') { sb.Append(Barra).Append('u').Append(((int)c).ToString("x4", CultureInfo.InvariantCulture)); }
                else sb.Append(c);
            }
            sb.Append('"');
        }

        // ------------------------------------------------------------------ acceso tolerante

        // Los getters nunca lanzan: una clave ausente o de otro tipo devuelve el valor por defecto
        // (cambios aditivos del contrato no rompen una DLL más antigua).

        public static object Valor(Dictionary<string, object> d, string clave)
        {
            object v;
            return d != null && d.TryGetValue(clave, out v) ? v : null;
        }

        public static string Texto(Dictionary<string, object> d, string clave)
        {
            object v = Valor(d, clave);
            if (v == null) return "";
            if (v is bool) return (bool)v ? "true" : "false";
            return Convert.ToString(v, CultureInfo.InvariantCulture);
        }

        public static decimal Decimal(Dictionary<string, object> d, string clave)
        {
            object v = Valor(d, clave);
            if (v is decimal) return (decimal)v;
            if (v is double) { try { return Convert.ToDecimal((double)v); } catch (OverflowException) { return 0m; } }
            if (v is bool) return (bool)v ? 1m : 0m;
            decimal r;
            string s = v as string;
            return s != null && decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out r) ? r : 0m;
        }

        public static long Largo(Dictionary<string, object> d, string clave)
        {
            decimal v = Decimal(d, clave);
            try { return (long)Math.Round(v, MidpointRounding.AwayFromZero); } catch (OverflowException) { return 0; }
        }

        public static int Entero(Dictionary<string, object> d, string clave)
        {
            long v = Largo(d, clave);
            return v > int.MaxValue || v < int.MinValue ? 0 : (int)v;
        }

        public static bool Booleano(Dictionary<string, object> d, string clave)
        {
            object v = Valor(d, clave);
            if (v is bool) return (bool)v;
            if (v is decimal) return (decimal)v != 0m;
            string s = v as string;
            return s == "1" || string.Equals(s, "true", StringComparison.OrdinalIgnoreCase);
        }

        public static Dictionary<string, object> Objeto(Dictionary<string, object> d, string clave)
        {
            Dictionary<string, object> o = Valor(d, clave) as Dictionary<string, object>;
            return o ?? new Dictionary<string, object>();
        }

        /// <summary>Lista de objetos (los elementos que no son objeto se ignoran).</summary>
        public static List<Dictionary<string, object>> Lista(Dictionary<string, object> d, string clave)
        {
            List<Dictionary<string, object>> r = new List<Dictionary<string, object>>();
            List<object> l = Valor(d, clave) as List<object>;
            if (l == null) return r;
            foreach (object o in l)
            {
                Dictionary<string, object> x = o as Dictionary<string, object>;
                if (x != null) r.Add(x);
            }
            return r;
        }

        /// <summary>Fecha ISO 8601 con zona ("2026-09-08T00:24:18-03:00") → hora local de la tienda. Vacío → DateTime.MinValue.</summary>
        public static DateTime Fecha(Dictionary<string, object> d, string clave)
        {
            DateTimeOffset f;
            return DateTimeOffset.TryParse(Texto(d, clave), CultureInfo.InvariantCulture, DateTimeStyles.None, out f) ? f.DateTime : DateTime.MinValue;
        }

        public static DateTime FechaUtc(Dictionary<string, object> d, string clave)
        {
            DateTimeOffset f;
            return DateTimeOffset.TryParse(Texto(d, clave), CultureInfo.InvariantCulture, DateTimeStyles.None, out f) ? f.UtcDateTime : DateTime.MinValue;
        }
    }
}
