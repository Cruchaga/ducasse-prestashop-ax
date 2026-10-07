using System.Collections.Generic;

namespace Prestashop.Net
{
    /// <summary>
    /// Base de TODA respuesta de la DLL. La DLL nunca lanza excepciones hacia AX: el resultado de cada
    /// llamada se lee en ErrorCode (0 = OK) y ErrorMessage.
    /// </summary>
    public class Respuesta
    {
        public Respuesta()
        {
            ErrorMessage = "";
            Detalle = "";
            ExecutionId = "";
            SchemaVersion = "";
            VersionModulo = "";
            Traza = "";
        }

        /// <summary>0 = OK. Ver <see cref="Errores"/>.</summary>
        public int ErrorCode { get; set; }

        /// <summary>Mensaje legible en español, apto para el log o la alerta de AX.</summary>
        public string ErrorMessage { get; set; }

        /// <summary>Detalle técnico (estado HTTP, excepción, inicio de la respuesta) para diagnóstico.</summary>
        public string Detalle { get; set; }

        /// <summary>true si tiene sentido reintentar más tarde (problema transitorio).</summary>
        public bool Reintentable { get; set; }

        /// <summary>Identificador único de la ejecución. Es el mismo en el log local y en el registro de la tienda.</summary>
        public string ExecutionId { get; set; }

        /// <summary>Versión del contrato que habla el servidor.</summary>
        public string SchemaVersion { get; set; }

        /// <summary>Versión del módulo dapintegracion del servidor.</summary>
        public string VersionModulo { get; set; }

        /// <summary>Duración total de la llamada, en milisegundos (incluye reintentos).</summary>
        public int DuracionMs { get; set; }

        /// <summary>Intentos realizados (1 = respondió a la primera).</summary>
        public int Intentos { get; set; }

        /// <summary>Bitácora de la ejecución: lo mismo que queda en el archivo de log.</summary>
        public string Traza { get; set; }

        /// <summary>true si ErrorCode es 0.</summary>
        public bool Ok { get { return ErrorCode == 0; } }
    }

    /// <summary>Resultado de un ítem dentro de un lote.</summary>
    public class ResultadoItem
    {
        public ResultadoItem()
        {
            Clave = "";
            ErrorMessage = "";
            Resultado = "";
        }

        /// <summary>SKU o id de orden del ítem.</summary>
        public string Clave { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>"actualizado", "sin_cambio", "confirmada" (vacío si el ítem falló).</summary>
        public string Resultado { get; set; }
    }

    /// <summary>
    /// Respuesta de una operación por lote. Cada ítem es independiente: ErrorCode global 0 = todos OK,
    /// 501 = algunos fallaron (ver cada resultado), otro código = el lote no llegó a procesarse.
    /// </summary>
    public class RespuestaLote : Respuesta
    {
        internal readonly List<ResultadoItem> Resultados = new List<ResultadoItem>();

        public int Total { get; set; }
        public int Correctos { get; set; }
        public int Fallidos { get; set; }

        public int ResultadoCount { get { return Resultados.Count; } }

        /// <summary>Elemento i (desde 0), o null si i está fuera de rango (no lanza hacia AX).</summary>
        public ResultadoItem GetResultado(int i) { return i >= 0 && i < Resultados.Count ? Resultados[i] : null; }
    }

    /// <summary>Respuesta de Ping: conectividad, versiones y hora del servidor.</summary>
    public class RespuestaPing : Respuesta
    {
        public RespuestaPing()
        {
            VersionPs = "";
            ClaveNombre = "";
        }

        public string VersionPs { get; set; }
        public System.DateTime HoraServidorUtc { get; set; }
        public int OrdenesPendientes { get; set; }

        /// <summary>Nombre de la clave con la que se autenticó la llamada.</summary>
        public string ClaveNombre { get; set; }
    }
}
