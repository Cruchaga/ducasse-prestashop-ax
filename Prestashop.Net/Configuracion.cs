using System;

namespace Prestashop.Net
{
    /// <summary>
    /// Datos de conexión de prestashop-net.dll con la tienda. AX los guarda en su tabla de parámetros
    /// y los entrega al crear el <see cref="Cliente"/>.
    /// </summary>
    /// <remarks>
    /// Desde X++ las propiedades se asignan con set_Xxx(valor): config.set_Url("https://...").
    /// </remarks>
    public class Configuracion
    {
        public Configuracion()
        {
            TimeoutSegundos = 120;
            ReintentosMax = 3;
            LogNivel = "Info";
            LogDias = 30;
        }

        /// <summary>URL base de la integración. Obligatoria y con https://. Ej.: https://dapducasse.cl/dap-integracion/</summary>
        public string Url { get; set; }

        /// <summary>Identificador público de la clave (BO › Dap Ducasse · Técnico › Integración AX).</summary>
        public string KeyId { get; set; }

        /// <summary>Secreto de la clave. Nunca viaja por la red: solo se usa para firmar cada llamada.</summary>
        public string Secreto { get; set; }

        /// <summary>Id del empleado de PrestaShop que firma los cambios de estado (0 = el configurado en la tienda).</summary>
        public int IdEmpleado { get; set; }

        /// <summary>Tiempo máximo de espera por llamada, en segundos. Por defecto 120.</summary>
        public int TimeoutSegundos { get; set; }

        /// <summary>Intentos por llamada ante errores transitorios (red, timeout, 5xx). Por defecto 3; 1 = sin reintentos; máximo 10. Esperas entre intentos: 1 s, 3 s y luego 9 s.</summary>
        public int ReintentosMax { get; set; }

        /// <summary>Carpeta del log local. Vacío = carpeta "prestashop-net/log" dentro de ProgramData. "-" = no escribir archivo.</summary>
        public string LogRuta { get; set; }

        /// <summary>"Info" (por defecto) o "Debug" (agrega request y response con datos personales enmascarados).</summary>
        public string LogNivel { get; set; }

        /// <summary>Días que se conservan los archivos de log. Por defecto 30.</summary>
        public int LogDias { get; set; }

        /// <summary>Solo staging: usuario del basic-auth de Apache que protege el ambiente de pruebas. Vacío en producción.</summary>
        public string BasicAuthUsuario { get; set; }

        /// <summary>Solo staging: clave del basic-auth de Apache. Vacío en producción.</summary>
        public string BasicAuthClave { get; set; }

        /// <summary>true = no usar el proxy del sistema (evita la demora de autodetección de proxy en algunos servidores).</summary>
        public bool SinProxy { get; set; }

        /// <summary>
        /// Valida la configuración sin llamar a la tienda. Devuelve "" si es válida, o el motivo.
        /// HTTPS es obligatorio: con http:// un proxy (Cloudflare) responde 301 y las credenciales se pierden
        /// en la redirección, que fue la causa del incidente de agosto de 2026.
        /// </summary>
        public string Validar()
        {
            if (string.IsNullOrEmpty(Url) || Url.Trim().Length == 0) return "Url es obligatoria.";
            Uri uri;
            if (!Uri.TryCreate(Url.Trim(), UriKind.Absolute, out uri)) return "Url no es una dirección válida: " + Url;
            if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase) && !PermitirHttpEnPruebas)
                return "Url debe empezar con https:// (se recibió " + uri.Scheme + "://).";
            if (string.IsNullOrEmpty(KeyId) || KeyId.Trim().Length == 0) return "KeyId es obligatorio.";
            if (string.IsNullOrEmpty(Secreto) || Secreto.Trim().Length == 0) return "Secreto es obligatorio.";
            if (TimeoutSegundos < 1 || TimeoutSegundos > 3600) return "TimeoutSegundos debe estar entre 1 y 3600.";
            if (ReintentosMax < 1 || ReintentosMax > 10) return "ReintentosMax debe estar entre 1 y 10.";
            if (!string.IsNullOrEmpty(BasicAuthUsuario) && string.IsNullOrEmpty(BasicAuthClave))
                return "BasicAuthClave es obligatoria cuando se informa BasicAuthUsuario.";
            return "";
        }

        /// <summary>Solo para los tests unitarios (servidor falso local sin TLS). No es accesible desde AX.</summary>
        internal static bool PermitirHttpEnPruebas;

        /// <summary>URL base normalizada, siempre terminada en "/".</summary>
        internal string UrlBase()
        {
            string u = (Url ?? "").Trim();
            return u.EndsWith("/") ? u : u + "/";
        }
    }
}
