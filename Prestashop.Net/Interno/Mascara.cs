using System.Text.RegularExpressions;

namespace Prestashop.Net.Interno
{
    /// <summary>
    /// Enmascara datos personales antes de escribirlos en el log de nivel Debug (Ley 21.719):
    /// correo j***@d***.cl, teléfono +569****4321, RUT 12.***.***-K, y deja solo la inicial de nombres
    /// y direcciones. Nunca escribe el secreto ni la firma.
    /// </summary>
    internal static class Mascara
    {
        // [\x5c] es la barra invertida (se evita escribirla doble en el código fuente).
        private static readonly Regex ParClaveValor = new Regex(@"""([A-Za-z_]+)""\s*:\s*""((?:[^""\x5c]|[\x5c].)*)""", RegexOptions.Compiled);
        private static readonly Regex Correo = new Regex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);
        private static readonly Regex Telefono = new Regex(@"\+?56\s?9\s?\d{4}\s?\d{4}\b", RegexOptions.Compiled);
        private static readonly Regex Rut = new Regex(@"\b(\d{1,2})\.?(\d{3})\.?(\d{3})-([\dkK])\b", RegexOptions.Compiled);

        private const string Sensibles = "|secreto|signature|firma|pdfbase64|password|clave|token|cardnumber|basicauthclave|";
        private const string Persona = "|nombrecompleto|nombrecontacto|calle|complemento|direccionempresa|direccion|ipaddress|";

        public static string Texto(string texto, int maximo)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            string t = ParClaveValor.Replace(texto, delegate(Match m)
            {
                string clave = "|" + m.Groups[1].Value.ToLowerInvariant() + "|";
                if (Sensibles.IndexOf(clave) >= 0) return "\"" + m.Groups[1].Value + "\":\"***\"";
                if (Persona.IndexOf(clave) >= 0 && m.Groups[2].Value.Length > 0)
                    return "\"" + m.Groups[1].Value + "\":\"" + m.Groups[2].Value.Substring(0, 1) + "***\"";
                return m.Value;
            });
            t = Correo.Replace(t, delegate(Match m)
            {
                string[] p = m.Value.Split('@');
                string dominio = p[1];
                int punto = dominio.LastIndexOf('.');
                return p[0].Substring(0, 1) + "***@" + dominio.Substring(0, 1) + "***" + (punto >= 0 ? dominio.Substring(punto) : "");
            });
            t = Telefono.Replace(t, delegate(Match m)
            {
                string d = Regex.Replace(m.Value, @"\D", "");
                if (d.Length < 8) return "****";
                return (m.Value.StartsWith("+") ? "+" : "") + d.Substring(0, d.Length - 8) + "****" + d.Substring(d.Length - 4);
            });
            t = Rut.Replace(t, delegate(Match m) { return m.Groups[1].Value + ".***.***-" + m.Groups[4].Value.ToUpperInvariant(); });
            if (maximo > 0 && t.Length > maximo) t = t.Substring(0, maximo) + " [truncado " + (t.Length - maximo) + " caracteres]";
            return t;
        }
    }
}
