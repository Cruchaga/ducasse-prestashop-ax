using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Prestashop.Net.Interno
{
    /// <summary>
    /// Firma HMAC-SHA256 de cada request. Debe coincidir bit a bit con src/Service/Firma.php del módulo
    /// dapintegracion (hay un vector de prueba compartido en los tests de ambos lados).
    /// </summary>
    /// <remarks>
    ///   cadena = METODO "\n" RUTA "\n" TIMESTAMP "\n" KEYID "\n" SHA256HEX(cuerpo)
    ///   firma  = HEX( HMAC-SHA256( secreto en UTF-8, cadena en UTF-8 ) )
    /// RUTA es el path más la query tal como viajan ("/dap-integracion/orden?id=21").
    /// </remarks>
    internal static class Firma
    {
        public static string Hex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        public static string Sha256Hex(byte[] datos)
        {
            using (SHA256 sha = new SHA256Managed())
            {
                return Hex(sha.ComputeHash(datos ?? new byte[0]));
            }
        }

        public static string Cadena(string metodo, string ruta, string timestamp, string keyId, byte[] cuerpo)
        {
            return (metodo ?? "").ToUpperInvariant() + "\n" + ruta + "\n" + timestamp + "\n" + keyId + "\n" + Sha256Hex(cuerpo);
        }

        public static string Firmar(string secreto, string cadena)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secreto ?? "")))
            {
                return Hex(hmac.ComputeHash(Encoding.UTF8.GetBytes(cadena)));
            }
        }

        /// <summary>Segundos desde 1970-01-01 UTC.</summary>
        public static string TimestampAhora()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return ((long)t.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }
    }
}
