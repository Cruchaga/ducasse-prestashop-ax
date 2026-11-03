using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Prestashop.Net.Interno
{
    /// <summary>
    /// Log de una ejecución: archivo diario en el servidor de AX + la traza que viaja en la respuesta.
    /// </summary>
    /// <remarks>
    /// Regla de oro: el log NUNCA hace fallar una operación. Si no se puede escribir el archivo (permisos,
    /// disco lleno), la ejecución sigue y todo queda igualmente en Respuesta.Traza.
    /// Línea: fecha | nivel | ExecutionId | operación | fase | mensaje
    /// </remarks>
    internal sealed class Registro
    {
        private static readonly object Candado = new object();
        private static DateTime ultimaPurga = DateTime.MinValue;

        private readonly StringBuilder traza = new StringBuilder();
        private readonly string carpeta;
        private readonly bool debug;
        private readonly int dias;
        private readonly string executionId;
        private readonly string operacion;
        private bool archivoFallo;

        public Registro(Configuracion c, string executionId, string operacion)
        {
            this.executionId = executionId;
            this.operacion = operacion;
            debug = string.Equals(c.LogNivel, "Debug", StringComparison.OrdinalIgnoreCase);
            dias = c.LogDias > 0 ? c.LogDias : 30;
            string ruta = (c.LogRuta ?? "").Trim();
            if (ruta == "-")
            {
                carpeta = null;
            }
            else if (ruta.Length > 0)
            {
                carpeta = ruta;
            }
            else
            {
                carpeta = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PrestaShopNet"), "log");
            }
        }

        public bool EsDebug { get { return debug; } }

        public string Traza { get { return traza.ToString(); } }

        public void Info(string fase, string mensaje)
        {
            Escribir("INFO ", fase, mensaje);
        }

        public void Error(string fase, string mensaje)
        {
            Escribir("ERROR", fase, mensaje);
        }

        /// <summary>Solo con LogNivel = Debug. El texto se enmascara antes de escribirse.</summary>
        public void Debug(string fase, string textoSensible)
        {
            if (debug) Escribir("DEBUG", fase, Mascara.Texto(textoSensible, 20000));
        }

        private void Escribir(string nivel, string fase, string mensaje)
        {
            string linea = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) + " | " + nivel + " | " + executionId
                + " | " + operacion + " | " + fase + " | " + (mensaje ?? "").Replace("\r", " ").Replace("\n", " ");
            traza.AppendLine(linea);
            if (carpeta == null || archivoFallo) return;
            try
            {
                lock (Candado)
                {
                    if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
                    string archivo = Path.Combine(carpeta, "PrestaShopNet-" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".log");
                    // FileShare.ReadWrite: varios procesos de AX pueden escribir el mismo archivo.
                    using (FileStream fs = new FileStream(archivo, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (StreamWriter w = new StreamWriter(fs, new UTF8Encoding(false)))
                    {
                        w.WriteLine(linea);
                    }
                    Purgar();
                }
            }
            catch (Exception e)
            {
                archivoFallo = true;
                traza.AppendLine("(no se pudo escribir el log en " + carpeta + ": " + e.Message + "; la ejecución continúa)");
            }
        }

        /// <summary>Borra los archivos de log más antiguos que LogDias. A lo más una vez al día por proceso.</summary>
        private void Purgar()
        {
            if (ultimaPurga.Date == DateTime.Today) return;
            ultimaPurga = DateTime.Today;
            try
            {
                foreach (string f in Directory.GetFiles(carpeta, "PrestaShopNet-*.log"))
                {
                    if (File.GetLastWriteTime(f) < DateTime.Today.AddDays(-dias)) File.Delete(f);
                }
            }
            catch (Exception)
            {
                // La purga es accesoria.
            }
        }
    }
}
