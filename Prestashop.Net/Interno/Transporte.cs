using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Prestashop.Net.Interno
{
    /// <summary>Sobre de respuesta del contrato, más los datos de transporte de la llamada.</summary>
    internal sealed class Sobre
    {
        public int ErrorCode;
        public string ErrorMessage = "";
        public string Detalle = "";
        public bool Reintentable;
        public string SchemaVersion = "";
        public string VersionModulo = "";
        public int DuracionServidorMs;
        public Dictionary<string, object> Datos = new Dictionary<string, object>();
        public int HttpStatus;
        public int BytesEnviados;
        public int BytesRecibidos;
        public int Intentos;
    }

    /// <summary>
    /// Cliente HTTP de la integración: HTTPS obligatorio, firma HMAC por request, sin seguir redirecciones,
    /// reintentos acotados para errores transitorios. Síncrono y de un hilo (DISEÑO.md §9): el servidor
    /// resuelve cada lote en una sola llamada, así que la DLL no necesita paralelizar.
    /// </summary>
    internal sealed class Transporte
    {
        /// <summary>Espera base entre reintentos: 1 s, 3 s, 9 s. Los tests la reducen.</summary>
        internal static int EsperaBaseMs = 1000;

        /// <summary>TLS 1.2 = SslProtocols 0x00000C00, el mismo valor que fuerza la DLL 1.46 para PS 1.7.</summary>
        internal const SecurityProtocolType Tls12 = (SecurityProtocolType)0x00000C00;

        private readonly Configuracion cfg;
        private readonly Registro log;

        public Transporte(Configuracion cfg, Registro log)
        {
            this.cfg = cfg;
            this.log = log;
        }

        /// <summary>Ejecuta la llamada con reintentos. Nunca lanza: todo error vuelve dentro del sobre.</summary>
        /// <param name="rutaRelativa">Operación y query, p. ej. "ordenes" o "orden?id=21".</param>
        public Sobre Ejecutar(string metodo, string rutaRelativa, Dictionary<string, object> datos, string executionId)
        {
            Uri uri = new Uri(cfg.UrlBase() + rutaRelativa);
            byte[] cuerpo = new byte[0];
            if (metodo == "POST")
            {
                Dictionary<string, object> req = new Dictionary<string, object>();
                req["schemaVersion"] = Cliente.SchemaVersion;
                req["executionId"] = executionId;
                req["datos"] = datos ?? new Dictionary<string, object>();
                string json = Json.Escribir(req);
                cuerpo = Encoding.UTF8.GetBytes(json);
                log.Debug("request", json);
            }

            int maximo = Math.Max(1, cfg.ReintentosMax);
            Sobre s = null;
            for (int intento = 1; intento <= maximo; intento++)
            {
                s = Intentar(metodo, uri, cuerpo, executionId);
                s.Intentos = intento;
                bool transitorio = s.ErrorCode == Errores.CONEXION || s.ErrorCode == Errores.TIMEOUT || s.ErrorCode == Errores.RESPUESTA_INVALIDA
                    || s.ErrorCode == Errores.ERROR_SERVIDOR_PS || s.ErrorCode == Errores.MANTENIMIENTO
                    || (s.ErrorCode == Errores.LIMITE && s.HttpStatus == 429);
                if (!transitorio || intento == maximo) break;
                int espera = EsperaBaseMs * (int)Math.Pow(3, intento - 1);
                log.Info("reintento", "intento " + intento + " de " + maximo + " falló con " + s.ErrorCode + " " + Errores.Nombre(s.ErrorCode)
                    + " (" + s.ErrorMessage + "); nuevo intento en " + espera + " ms");
                Thread.Sleep(espera);
            }
            return s;
        }

        private Sobre Intentar(string metodo, Uri uri, byte[] cuerpo, string executionId)
        {
            Sobre s = new Sobre();
            s.BytesEnviados = cuerpo.Length;
            HttpWebResponse resp = null;
            try
            {
                // Solo TLS 1.2, igual que IntegracionPrestashopLib 1.46 (HttpRequestFunctions): se asigna (no se agrega)
                // antes de CADA request, porque ServicePointManager es global al proceso de AX y otra librería puede
                // haberlo cambiado. La enumeración Tls12 no existe en .NET 3.5: se usa el valor numérico.
                try
                {
                    ServicePointManager.SecurityProtocol = Tls12;
                }
                catch (NotSupportedException e)
                {
                    // .NET 3.5 sin el soporte de TLS 1.2 (falta la actualización KB3154518 o .NET 4.5+ en la máquina).
                    s.ErrorCode = Errores.TLS;
                    s.ErrorMessage = "El .NET Framework de este servidor no soporta TLS 1.2, que la tienda exige. Instale la actualización de TLS 1.2 para .NET 3.5 (KB3154518) o .NET Framework 4.5 o superior.";
                    s.Detalle = "NotSupportedException: " + e.Message;
                    return s;
                }

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.Method = metodo;
                req.AllowAutoRedirect = false; // una redirección perdería las credenciales: se reporta como error 103
                req.Timeout = cfg.TimeoutSegundos * 1000;
                req.ReadWriteTimeout = cfg.TimeoutSegundos * 1000;
                req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                req.UserAgent = Cliente.AgenteUsuario;
                req.Accept = "application/json";
                req.KeepAlive = true;
                req.ServicePoint.Expect100Continue = false;
                if (cfg.SinProxy) req.Proxy = null;

                string keyId = (cfg.KeyId ?? "").Trim();
                string ts = Firma.TimestampAhora();
                req.Headers["X-Dap-KeyId"] = keyId;
                req.Headers["X-Dap-Timestamp"] = ts;
                req.Headers["X-Dap-Signature"] = Firma.Firmar((cfg.Secreto ?? "").Trim(), Firma.Cadena(metodo, uri.PathAndQuery, ts, keyId, cuerpo));
                req.Headers["X-Dap-ExecutionId"] = executionId;
                req.Headers["X-Dap-Schema"] = Cliente.SchemaVersion;
                req.Headers["X-Dap-Client"] = Cliente.AgenteUsuario;
                if (!string.IsNullOrEmpty(cfg.BasicAuthUsuario))
                {
                    // Solo staging. Se envía de entrada (sin esperar el desafío 401).
                    req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(cfg.BasicAuthUsuario + ":" + cfg.BasicAuthClave));
                }
                if (metodo == "POST")
                {
                    req.ContentType = "application/json; charset=utf-8";
                    req.ContentLength = cuerpo.Length;
                    using (Stream salida = req.GetRequestStream())
                    {
                        salida.Write(cuerpo, 0, cuerpo.Length);
                    }
                }

                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException we)
                {
                    resp = we.Response as HttpWebResponse; // 4xx y 5xx llegan como excepción con respuesta
                    if (resp == null)
                    {
                        MapearFallaDeRed(s, we);
                        return s;
                    }
                }

                s.HttpStatus = (int)resp.StatusCode;
                string texto;
                using (Stream entrada = resp.GetResponseStream())
                using (StreamReader lector = new StreamReader(entrada, Encoding.UTF8))
                {
                    texto = lector.ReadToEnd();
                }
                s.BytesRecibidos = Encoding.UTF8.GetByteCount(texto);
                log.Debug("response", "HTTP " + s.HttpStatus + " " + texto);
                Interpretar(s, texto, resp.Headers["Location"]);
            }
            catch (WebException we)
            {
                MapearFallaDeRed(s, we);
            }
            catch (IOException e)
            {
                s.ErrorCode = Errores.CONEXION;
                s.ErrorMessage = "La conexión con la tienda se cortó mientras se leía la respuesta.";
                s.Detalle = e.GetType().Name + ": " + e.Message;
            }
            finally
            {
                if (resp != null) resp.Close();
            }
            s.Reintentable = s.Reintentable || Errores.EsReintentable(s.ErrorCode);
            return s;
        }

        /// <summary>Convierte la respuesta HTTP en el sobre. Si no es el JSON del contrato, clasifica por el estado HTTP.</summary>
        internal static void Interpretar(Sobre s, string texto, string location)
        {
            int st = s.HttpStatus;
            if (st >= 300 && st < 400)
            {
                s.ErrorCode = Errores.REDIRECCION;
                s.ErrorMessage = "La tienda respondió una redirección (HTTP " + st + "). Revise la Url configurada: debe ser https y la dirección definitiva.";
                s.Detalle = "Location: " + (location ?? "");
                return;
            }

            Dictionary<string, object> j = null;
            try
            {
                j = Json.LeerObjeto(texto);
            }
            catch (FormatException)
            {
                // No es JSON: se clasifica más abajo.
            }
            if (j != null && j.ContainsKey("errorCode"))
            {
                s.ErrorCode = Json.Entero(j, "errorCode");
                s.ErrorMessage = Json.Texto(j, "errorMessage");
                s.Detalle = Json.Texto(j, "detalle");
                s.Reintentable = Json.Booleano(j, "reintentable");
                s.SchemaVersion = Json.Texto(j, "schemaVersion");
                s.VersionModulo = Json.Texto(j, "versionModulo");
                s.DuracionServidorMs = Json.Entero(j, "duracionMs");
                s.Datos = Json.Objeto(j, "datos");
                if (s.ErrorCode == 0 && st >= 400)
                {
                    s.ErrorCode = Errores.RESPUESTA_INVALIDA;
                    s.ErrorMessage = "La tienda respondió HTTP " + st + " con un sobre sin error.";
                }
                return;
            }

            // La respuesta no es el sobre del contrato: una página de Apache, de Cloudflare o de mantenimiento.
            string inicio = (texto ?? "").Trim();
            if (inicio.Length > 600) inicio = inicio.Substring(0, 600) + "…";
            s.Detalle = "HTTP " + st + " · " + inicio.Replace("\r", " ").Replace("\n", " ");
            if (st == 401)
            {
                s.ErrorCode = Errores.AUTH_FIRMA;
                s.ErrorMessage = "La tienda respondió 401 antes de llegar a la integración. En el ambiente de pruebas revise BasicAuthUsuario y BasicAuthClave.";
            }
            else if (st == 403)
            {
                s.ErrorCode = Errores.IP_NO_AUTORIZADA;
                s.ErrorMessage = "La tienda rechazó la conexión (HTTP 403): la IP de este servidor no está autorizada en el firewall o en Apache.";
            }
            else if (st == 404)
            {
                s.ErrorCode = Errores.NO_ENCONTRADO;
                s.ErrorMessage = "La URL de la integración no existe en la tienda (HTTP 404). Revise Url y que el módulo dapintegracion esté instalado.";
            }
            else if (st == 429)
            {
                s.ErrorCode = Errores.LIMITE;
                s.ErrorMessage = "La tienda limitó las llamadas (HTTP 429).";
            }
            else if (st == 503)
            {
                s.ErrorCode = Errores.MANTENIMIENTO;
                s.ErrorMessage = "La tienda no está disponible (HTTP 503: mantenimiento u origen caído).";
            }
            else if (st >= 500)
            {
                s.ErrorCode = Errores.ERROR_SERVIDOR_PS;
                s.ErrorMessage = "Error del servidor de la tienda (HTTP " + st + ").";
            }
            else
            {
                s.ErrorCode = Errores.RESPUESTA_INVALIDA;
                s.ErrorMessage = "La respuesta de la tienda no es el JSON de la integración (HTTP " + st + ").";
            }
        }

        private static void MapearFallaDeRed(Sobre s, WebException we)
        {
            switch (we.Status)
            {
                case WebExceptionStatus.Timeout:
                    s.ErrorCode = Errores.TIMEOUT;
                    s.ErrorMessage = "La tienda no respondió dentro del tiempo de espera.";
                    break;
                case WebExceptionStatus.TrustFailure:
                case WebExceptionStatus.SecureChannelFailure:
                    s.ErrorCode = Errores.TLS;
                    s.ErrorMessage = "No se pudo establecer una conexión segura con la tienda (certificado o TLS).";
                    break;
                case WebExceptionStatus.NameResolutionFailure:
                    s.ErrorCode = Errores.CONEXION;
                    s.ErrorMessage = "No se pudo resolver el nombre de la tienda (DNS).";
                    break;
                default:
                    s.ErrorCode = Errores.CONEXION;
                    s.ErrorMessage = "No se pudo conectar con la tienda.";
                    break;
            }
            s.Detalle = we.Status + ": " + we.Message + (we.InnerException != null ? " · " + we.InnerException.Message : "");
            s.Reintentable = Errores.EsReintentable(s.ErrorCode);
        }
    }
}
