using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Prestashop.Net.Consola
{
    /// <summary>
    /// Consola de escenarios de PrestaShopNet.dll: hace lo mismo que hará el X++ de AX, usando solo la API
    /// pública de la DLL. Reemplaza a la antigua consola "Test".
    /// </summary>
    /// <remarks>
    /// Sin secretos en el código: la conexión se lee de variables de entorno PSNET_* o de un archivo
    /// --env=ruta con líneas CLAVE=valor (PSNET_URL, PSNET_KEYID, PSNET_SECRETO y, para staging,
    /// PSNET_BASIC_USUARIO + PSNET_BASIC_CLAVE o PSNET_BASIC_CLAVE_ARCHIVO).
    /// Las operaciones que ESCRIBEN en la tienda piden confirmación, salvo que se pase --si.
    /// </remarks>
    internal static class Program
    {
        private static readonly List<string> Opciones = new List<string>();
        private static readonly List<string> Argumentos = new List<string>();

        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            foreach (string a in args)
            {
                if (a.StartsWith("--")) Opciones.Add(a); else Argumentos.Add(a);
            }
            if (Argumentos.Count == 0)
            {
                Ayuda();
                return 2;
            }
            try
            {
                Configuracion cfg = CargarConfiguracion();
                Cliente cliente = new Cliente(cfg);
                Console.WriteLine("PrestaShopNet " + Cliente.VersionDll + " (contrato " + Cliente.SchemaVersion + ") → " + cfg.Url);
                return Escenarios.Ejecutar(cliente, Argumentos, Opciones);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error de la consola: " + e.Message);
                return 3;
            }
        }

        internal static bool Tiene(string opcion)
        {
            return Opciones.Contains(opcion);
        }

        internal static string Valor(string opcion)
        {
            foreach (string o in Opciones)
            {
                if (o.StartsWith(opcion + "=")) return o.Substring(opcion.Length + 1);
            }
            return "";
        }

        private static Configuracion CargarConfiguracion()
        {
            Dictionary<string, string> env = new Dictionary<string, string>();
            string archivo = Valor("--env");
            if (archivo.Length > 0)
            {
                foreach (string linea in File.ReadAllLines(archivo))
                {
                    string l = linea.Trim();
                    int igual = l.IndexOf('=');
                    if (l.Length == 0 || l.StartsWith("#") || igual < 1) continue;
                    env[l.Substring(0, igual).Trim()] = l.Substring(igual + 1).Trim();
                }
            }
            Configuracion c = new Configuracion();
            c.Url = Leer(env, "PSNET_URL");
            c.KeyId = Leer(env, "PSNET_KEYID");
            c.Secreto = Leer(env, "PSNET_SECRETO");
            c.BasicAuthUsuario = Leer(env, "PSNET_BASIC_USUARIO");
            c.BasicAuthClave = Leer(env, "PSNET_BASIC_CLAVE");
            string archivoClave = Leer(env, "PSNET_BASIC_CLAVE_ARCHIVO");
            if (c.BasicAuthClave.Length == 0 && archivoClave.Length > 0 && File.Exists(archivoClave))
            {
                c.BasicAuthClave = File.ReadAllText(archivoClave).Trim();
            }
            int empleado;
            if (int.TryParse(Leer(env, "PSNET_EMPLEADO"), NumberStyles.Integer, CultureInfo.InvariantCulture, out empleado)) c.IdEmpleado = empleado;
            string log = Valor("--log").Length > 0 ? Valor("--log") : Leer(env, "PSNET_LOG");
            if (log.Length > 0) c.LogRuta = log;
            if (Tiene("--debug")) c.LogNivel = "Debug";
            int n;
            if (int.TryParse(Valor("--timeout"), out n)) c.TimeoutSegundos = n;
            if (int.TryParse(Valor("--reintentos"), out n)) c.ReintentosMax = n;
            if (Valor("--url").Length > 0) c.Url = Valor("--url");
            if (Valor("--keyid").Length > 0) c.KeyId = Valor("--keyid");
            return c;
        }

        private static string Leer(Dictionary<string, string> env, string clave)
        {
            string v;
            if (env.TryGetValue(clave, out v)) return v;
            return Environment.GetEnvironmentVariable(clave) ?? "";
        }

        private static void Ayuda()
        {
            Console.WriteLine(@"Consola de escenarios de PrestaShopNet.dll

Uso:  Prestashop.Net.Console <escenario> [argumentos] [--env=archivo] [--si] [--json=archivo] [--traza] [--debug]

Lectura (no modifican la tienda):
  ping                               conectividad, versiones y órdenes pendientes
  orden <id>                         una orden completa
  producto <sku>                     ficha de un producto

Escritura (piden confirmación; --si la da por adelantado):
  ordenes [limite]                   descarga las pendientes y las marca como entregadas
  confirmar <id>[:ventaAx] ...       confirma órdenes ya integradas en AX
  estado <idOrden> <idEstado> [idEnvio:estado:tracking:carrier ...] [--sin-correo]
  precio <sku> <precioNeto>          precio neto de un SKU
  stock <sku> <cantidad>             stock absoluto de un SKU
  activar <sku> <0|1>                activa o desactiva un producto
  lote <precios|stock|activar> <archivo>   archivo con líneas  sku;valor
  crear <sku> <nombre> <precioNeto>  crea un producto simple (inactivo)
  dte <idOrden>                      publica un DTE de prueba (reservado: responde 603)

Conexión: variables PSNET_URL, PSNET_KEYID, PSNET_SECRETO (+ PSNET_BASIC_USUARIO y PSNET_BASIC_CLAVE o
PSNET_BASIC_CLAVE_ARCHIVO en staging), del entorno o de --env=archivo. Nunca en el código.
Otras opciones: --log=carpeta  --timeout=seg  --reintentos=n  --url=...  --keyid=...");
        }
    }
}
