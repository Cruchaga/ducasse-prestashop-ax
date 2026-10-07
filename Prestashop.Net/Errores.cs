namespace Prestashop.Net
{
    /// <summary>
    /// Catálogo público de códigos de error (el mismo del módulo dapintegracion de PrestaShop).
    /// Toda respuesta trae ErrorCode: 0 es OK; cualquier otro valor, ErrorMessage dice qué pasó.
    /// </summary>
    /// <remarks>
    /// 1xx conectividad · 2xx autenticación · 3xx validación · 4xx no encontrado · 5xx conflicto de
    /// estado y resultado parcial · 6xx servidor PrestaShop · 7xx versión de contrato · 9xx no controlado.
    /// Un código nunca cambia de significado; los nuevos se agregan.
    /// </remarks>
    public static class Errores
    {
        /// <summary>Operación correcta.</summary>
        public const int OK = 0;

        /// <summary>No se pudo conectar con la tienda (DNS, red, conexión cerrada).</summary>
        public const int CONEXION = 100;
        /// <summary>La tienda no respondió dentro de TimeoutSegundos.</summary>
        public const int TIMEOUT = 101;
        /// <summary>Certificado TLS inválido o canal seguro rechazado.</summary>
        public const int TLS = 102;
        /// <summary>La tienda respondió con una redirección (301/302). La URL configurada es incorrecta; la DLL no sigue redirecciones.</summary>
        public const int REDIRECCION = 103;
        /// <summary>La respuesta no es el JSON del contrato (p. ej. una página HTML de un proxy).</summary>
        public const int RESPUESTA_INVALIDA = 110;

        /// <summary>Firma inválida, o basic-auth de staging incorrecto.</summary>
        public const int AUTH_FIRMA = 200;
        /// <summary>KeyId desconocido o revocado.</summary>
        public const int AUTH_CLAVE = 201;
        /// <summary>Reloj del servidor de AX desfasado más de 5 minutos.</summary>
        public const int AUTH_TIMESTAMP = 202;
        /// <summary>IP no autorizada.</summary>
        public const int IP_NO_AUTORIZADA = 203;

        /// <summary>Dato de entrada inválido (campo obligatorio, tipo, formato).</summary>
        public const int VALIDACION = 300;
        /// <summary>Configuración inválida (URL sin https, KeyId o secreto vacíos). No se llegó a llamar.</summary>
        public const int CONFIG_INVALIDA = 301;

        /// <summary>La orden, el SKU, el estado o el envío no existe.</summary>
        public const int NO_ENCONTRADO = 400;

        /// <summary>Conflicto: orden cancelada, envío de otra orden, SKU repetido, orden ya confirmada con otra venta.</summary>
        public const int CONFLICTO_ESTADO = 500;
        /// <summary>Resultado parcial: algunos ítems u órdenes tienen error propio. Revise cada uno.</summary>
        public const int RESULTADO_PARCIAL = 501;

        /// <summary>Error del servidor PrestaShop (HTTP 5xx o excepción).</summary>
        public const int ERROR_SERVIDOR_PS = 600;
        /// <summary>La tienda respondió 503 (mantenimiento u origen caído).</summary>
        public const int MANTENIMIENTO = 601;
        /// <summary>Límite excedido: demasiados ítems en el lote o demasiadas llamadas (HTTP 429).</summary>
        public const int LIMITE = 602;
        /// <summary>Operación reservada en el contrato, aún no habilitada en el servidor.</summary>
        public const int NO_IMPLEMENTADO = 603;

        /// <summary>La versión del contrato de la DLL no es compatible con la del servidor.</summary>
        public const int VERSION_CONTRATO = 700;

        /// <summary>Error no controlado dentro de la DLL. Detalle trae la excepción.</summary>
        public const int NO_CONTROLADO = 900;

        /// <summary>Nombre simbólico del código, para logs y mensajes.</summary>
        public static string Nombre(int codigo)
        {
            switch (codigo)
            {
                case OK: return "OK";
                case CONEXION: return "CONEXION";
                case TIMEOUT: return "TIMEOUT";
                case TLS: return "TLS";
                case REDIRECCION: return "REDIRECCION";
                case RESPUESTA_INVALIDA: return "RESPUESTA_INVALIDA";
                case AUTH_FIRMA: return "AUTH_FIRMA";
                case AUTH_CLAVE: return "AUTH_CLAVE";
                case AUTH_TIMESTAMP: return "AUTH_TIMESTAMP";
                case IP_NO_AUTORIZADA: return "IP_NO_AUTORIZADA";
                case VALIDACION: return "VALIDACION";
                case CONFIG_INVALIDA: return "CONFIG_INVALIDA";
                case NO_ENCONTRADO: return "NO_ENCONTRADO";
                case CONFLICTO_ESTADO: return "CONFLICTO_ESTADO";
                case RESULTADO_PARCIAL: return "RESULTADO_PARCIAL";
                case ERROR_SERVIDOR_PS: return "ERROR_SERVIDOR_PS";
                case MANTENIMIENTO: return "MANTENIMIENTO";
                case LIMITE: return "LIMITE";
                case NO_IMPLEMENTADO: return "NO_IMPLEMENTADO";
                case VERSION_CONTRATO: return "VERSION_CONTRATO";
                case NO_CONTROLADO: return "NO_CONTROLADO";
                default: return "DESCONOCIDO";
            }
        }

        /// <summary>true si tiene sentido volver a intentar la misma llamada más tarde.</summary>
        public static bool EsReintentable(int codigo)
        {
            return codigo == CONEXION || codigo == TIMEOUT || codigo == RESPUESTA_INVALIDA
                || codigo == ERROR_SERVIDOR_PS || codigo == MANTENIMIENTO;
        }
    }

    /// <summary>Ids de los estados de pedido de la tienda Dap Ducasse (PS 9.1). Para ActualizarEstados.</summary>
    public static class Estados
    {
        public const int PagoAceptado = 2;
        public const int PreparacionEnCurso = 3;
        public const int EnRuta = 4;
        public const int Entregado = 5;
        public const int Cancelado = 6;
        public const int Reembolsado = 7;
        public const int ErrorEnPago = 8;
        public const int EnTienda = 24;
        public const int ErrorEnDespacho = 25;
    }

    /// <summary>Estados de un envío (una orden tiene uno o más envíos). Para CambioEstado.AgregarEnvio.</summary>
    public static class EstadosEnvio
    {
        public const string Pendiente = "pendiente";
        public const string Preparacion = "preparacion";
        public const string Despachado = "despachado";
        public const string EnTienda = "en_tienda";
        public const string Entregado = "entregado";
        public const string Error = "error";
    }

    /// <summary>
    /// Catálogo de códigos de error como propiedades estáticas.
    /// Desde X++: Prestashop.Net.CodigoError::get_OK(). Los valores son los de <see cref="Errores"/>,
    /// cuyos campos const X++ no puede leer (revisión 2026-10-07, H-06).
    /// </summary>
    public static class CodigoError
    {
        public static int OK { get { return Errores.OK; } }
        public static int CONEXION { get { return Errores.CONEXION; } }
        public static int TIMEOUT { get { return Errores.TIMEOUT; } }
        public static int TLS { get { return Errores.TLS; } }
        public static int REDIRECCION { get { return Errores.REDIRECCION; } }
        public static int RESPUESTA_INVALIDA { get { return Errores.RESPUESTA_INVALIDA; } }
        public static int AUTH_FIRMA { get { return Errores.AUTH_FIRMA; } }
        public static int AUTH_CLAVE { get { return Errores.AUTH_CLAVE; } }
        public static int AUTH_TIMESTAMP { get { return Errores.AUTH_TIMESTAMP; } }
        public static int IP_NO_AUTORIZADA { get { return Errores.IP_NO_AUTORIZADA; } }
        public static int VALIDACION { get { return Errores.VALIDACION; } }
        public static int CONFIG_INVALIDA { get { return Errores.CONFIG_INVALIDA; } }
        public static int NO_ENCONTRADO { get { return Errores.NO_ENCONTRADO; } }
        public static int CONFLICTO_ESTADO { get { return Errores.CONFLICTO_ESTADO; } }
        public static int RESULTADO_PARCIAL { get { return Errores.RESULTADO_PARCIAL; } }
        public static int ERROR_SERVIDOR_PS { get { return Errores.ERROR_SERVIDOR_PS; } }
        public static int MANTENIMIENTO { get { return Errores.MANTENIMIENTO; } }
        public static int LIMITE { get { return Errores.LIMITE; } }
        public static int NO_IMPLEMENTADO { get { return Errores.NO_IMPLEMENTADO; } }
        public static int VERSION_CONTRATO { get { return Errores.VERSION_CONTRATO; } }
        public static int NO_CONTROLADO { get { return Errores.NO_CONTROLADO; } }
    }

    /// <summary>
    /// Ids de los estados de pedido como propiedades estáticas.
    /// Desde X++: Prestashop.Net.EstadoOrden::get_PagoAceptado(). Los valores son los de <see cref="Estados"/>,
    /// cuyos campos const X++ no puede leer (revisión 2026-10-07, H-06).
    /// </summary>
    public static class EstadoOrden
    {
        public static int PagoAceptado { get { return Estados.PagoAceptado; } }
        public static int PreparacionEnCurso { get { return Estados.PreparacionEnCurso; } }
        public static int EnRuta { get { return Estados.EnRuta; } }
        public static int Entregado { get { return Estados.Entregado; } }
        public static int Cancelado { get { return Estados.Cancelado; } }
        public static int Reembolsado { get { return Estados.Reembolsado; } }
        public static int ErrorEnPago { get { return Estados.ErrorEnPago; } }
        public static int EnTienda { get { return Estados.EnTienda; } }
        public static int ErrorEnDespacho { get { return Estados.ErrorEnDespacho; } }
    }

    /// <summary>
    /// Estados de envío como propiedades estáticas.
    /// Desde X++: Prestashop.Net.EstadoEnvio::get_Pendiente(). Los valores son los de <see cref="EstadosEnvio"/>,
    /// cuyos campos const X++ no puede leer (revisión 2026-10-07, H-06).
    /// </summary>
    public static class EstadoEnvio
    {
        public static string Pendiente { get { return EstadosEnvio.Pendiente; } }
        public static string Preparacion { get { return EstadosEnvio.Preparacion; } }
        public static string Despachado { get { return EstadosEnvio.Despachado; } }
        public static string EnTienda { get { return EstadosEnvio.EnTienda; } }
        public static string Entregado { get { return EstadosEnvio.Entregado; } }
        public static string Error { get { return EstadosEnvio.Error; } }
    }
}
