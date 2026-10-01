using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Prestashop.Net.Consola
{
    /// <summary>Los escenarios de la consola. Cada uno imprime ErrorCode, ExecutionId y duración.</summary>
    internal static class Escenarios
    {
        public static int Ejecutar(Cliente c, List<string> a, List<string> opciones)
        {
            switch (a[0].ToLowerInvariant())
            {
                case "ping":
                    RespuestaPing p = c.Ping();
                    Cabecera("Ping", p);
                    if (p.Ok) Console.WriteLine("  PrestaShop " + p.VersionPs + " · módulo " + p.VersionModulo + " · hora servidor " + p.HoraServidorUtc.ToString("u")
                        + " · órdenes pendientes " + p.OrdenesPendientes + " · clave \"" + p.ClaveNombre + "\"");
                    return Cerrar(p);
                case "orden":
                    RespuestaOrden o = c.ObtenerOrden(long.Parse(a[1], CultureInfo.InvariantCulture));
                    Cabecera("ObtenerOrden", o);
                    if (o.Orden != null) Orden(o.Orden, true);
                    return Cerrar(o);
                case "producto":
                    RespuestaProducto pr = c.ObtenerProducto(a[1]);
                    Cabecera("ObtenerProducto", pr);
                    if (pr.Producto != null) Producto(pr.Producto);
                    return Cerrar(pr);
                case "ordenes":
                    if (!Confirmar("Descargar las órdenes pendientes (quedan marcadas como entregadas a AX)")) return 2;
                    RespuestaOrdenes os = c.ObtenerOrdenes(a.Count > 1 ? int.Parse(a[1], CultureInfo.InvariantCulture) : 50);
                    Cabecera("ObtenerOrdenes", os);
                    Console.WriteLine("  " + os.OrdenesCount + " órdenes · " + os.OrdenesConError + " con error · " + os.PendientesRestantes + " pendientes restantes");
                    for (int i = 0; i < os.OrdenesCount; i++) Orden(os.GetOrden(i), false);
                    return Cerrar(os);
                case "confirmar":
                    LoteConfirmacion lc = new LoteConfirmacion();
                    for (int i = 1; i < a.Count; i++)
                    {
                        string[] par = a[i].Split(':');
                        lc.Agregar(long.Parse(par[0], CultureInfo.InvariantCulture), par.Length > 1 ? par[1] : "");
                    }
                    if (!Confirmar("Confirmar " + lc.Count + " órdenes como integradas en AX")) return 2;
                    return Lote("ConfirmarOrdenes", c.ConfirmarOrdenes(lc));
                case "estado":
                    CambioEstado ce = new CambioEstado(long.Parse(a[1], CultureInfo.InvariantCulture), int.Parse(a[2], CultureInfo.InvariantCulture));
                    ce.EnviarCorreo = !opciones.Contains("--sin-correo");
                    for (int i = 3; i < a.Count; i++)
                    {
                        string[] e = (a[i] + ":::").Split(':');
                        ce.AgregarEnvio(long.Parse(e[0], CultureInfo.InvariantCulture), e[1], e[2], e[3]);
                    }
                    if (!Confirmar("Cambiar el estado de la orden " + ce.IdOrden + (ce.EnviarCorreo && ce.IdEstadoOrden > 0 ? " (ENVÍA CORREO al cliente)" : " (sin correo)"))) return 2;
                    return Lote("ActualizarEstados", c.ActualizarEstado(ce));
                case "precio":
                    LotePrecios lp = new LotePrecios();
                    lp.Agregar(a[1], decimal.Parse(a[2], CultureInfo.InvariantCulture));
                    if (!Confirmar("Cambiar el precio neto de " + a[1] + " a " + a[2])) return 2;
                    return Lote("ActualizarPrecios", c.ActualizarPrecios(lp));
                case "stock":
                    LoteStock ls = new LoteStock();
                    ls.Agregar(a[1], int.Parse(a[2], CultureInfo.InvariantCulture));
                    if (!Confirmar("Fijar el stock de " + a[1] + " en " + a[2])) return 2;
                    return Lote("ActualizarStock", c.ActualizarStock(ls));
                case "activar":
                    LoteActivacion la = new LoteActivacion();
                    la.Agregar(a[1], a[2] == "1" || a[2].ToLowerInvariant() == "true");
                    if (!Confirmar((a[2] == "1" ? "Activar " : "Desactivar ") + a[1])) return 2;
                    return Lote("ActivarProductos", c.ActivarProductos(la));
                case "lote":
                    return LoteDesdeArchivo(c, a[1].ToLowerInvariant(), a[2]);
                case "crear":
                    ProductoNuevo pn = new ProductoNuevo();
                    pn.Sku = a[1];
                    pn.Nombre = a[2];
                    pn.PrecioNeto = decimal.Parse(a[3], CultureInfo.InvariantCulture);
                    if (!Confirmar("Crear el producto " + pn.Sku + " (inactivo)")) return 2;
                    RespuestaProducto cr = c.CrearProducto(pn);
                    Cabecera("CrearProducto", cr);
                    if (cr.Producto != null) Producto(cr.Producto);
                    return Cerrar(cr);
                case "dte":
                    Dte d = new Dte();
                    d.IdOrden = long.Parse(a[1], CultureInfo.InvariantCulture);
                    d.Tipo = "boleta";
                    d.Folio = "0";
                    d.FechaEmision = DateTime.Today;
                    d.NombreArchivo = "prueba.pdf";
                    d.PdfBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes("%PDF-1.4 prueba"));
                    RespuestaDte rd = c.PublicarDte(d);
                    Cabecera("PublicarDte", rd);
                    return Cerrar(rd);
                default:
                    Console.Error.WriteLine("Escenario desconocido: " + a[0]);
                    return 2;
            }
        }

        private static bool Confirmar(string que)
        {
            if (Program.Tiene("--si")) return true;
            Console.Write(que + ". ¿Continuar? [s/N] ");
            string r = Console.ReadLine();
            return r != null && r.Trim().ToLowerInvariant() == "s";
        }

        private static void Cabecera(string operacion, Respuesta r)
        {
            Console.WriteLine("[" + operacion + "] ErrorCode " + r.ErrorCode + " " + Errores.Nombre(r.ErrorCode) + " · " + r.DuracionMs + " ms · intentos "
                + r.Intentos + " · ExecutionId " + r.ExecutionId + (r.Reintentable ? " · reintentable" : ""));
            if (r.ErrorMessage.Length > 0) Console.WriteLine("  Mensaje: " + r.ErrorMessage);
            if (r.Detalle.Length > 0) Console.WriteLine("  Detalle: " + r.Detalle);
        }

        /// <summary>Archiva el JSON y la traza si se pidieron, y traduce el resultado a código de salida.</summary>
        private static int Cerrar(Respuesta r)
        {
            string archivo = Program.Valor("--json");
            if (archivo.Length > 0)
            {
                File.WriteAllText(archivo, Volcado.Json(r, Program.Tiene("--traza")), new UTF8Encoding(false));
                Console.WriteLine("  JSON guardado en " + archivo);
            }
            else if (Program.Tiene("--traza"))
            {
                Console.WriteLine("--- traza ---");
                Console.Write(r.Traza);
            }
            return r.ErrorCode == 0 ? 0 : 1;
        }

        private static int Lote(string operacion, RespuestaLote r)
        {
            Cabecera(operacion, r);
            Console.WriteLine("  total " + r.Total + " · correctos " + r.Correctos + " · fallidos " + r.Fallidos);
            int mostrar = Math.Min(r.ResultadoCount, Program.Tiene("--todo") ? int.MaxValue : 12);
            for (int i = 0; i < mostrar; i++)
            {
                ResultadoItem it = r.GetResultado(i);
                Console.WriteLine("    " + it.Clave + " → " + (it.ErrorCode == 0 ? it.Resultado : "ERROR " + it.ErrorCode + " " + it.ErrorMessage));
            }
            if (r.ResultadoCount > mostrar) Console.WriteLine("    … " + (r.ResultadoCount - mostrar) + " más (--todo para verlos)");
            return Cerrar(r);
        }

        /// <summary>Lote desde un archivo de texto con líneas "sku;valor" (precio neto, cantidad, o 0/1).</summary>
        private static int LoteDesdeArchivo(Cliente c, string tipo, string archivo)
        {
            LotePrecios lp = new LotePrecios();
            LoteStock ls = new LoteStock();
            LoteActivacion la = new LoteActivacion();
            foreach (string linea in File.ReadAllLines(archivo))
            {
                string[] p = linea.Split(';');
                if (p.Length < 2 || p[0].Trim().Length == 0 || p[0].StartsWith("#")) continue;
                string sku = p[0].Trim();
                string v = p[1].Trim();
                if (tipo == "precios") lp.Agregar(sku, decimal.Parse(v, CultureInfo.InvariantCulture));
                else if (tipo == "stock") ls.Agregar(sku, int.Parse(v, CultureInfo.InvariantCulture));
                else if (tipo == "activar") la.Agregar(sku, v == "1");
            }
            int n = tipo == "precios" ? lp.Count : (tipo == "stock" ? ls.Count : la.Count);
            if (n == 0)
            {
                Console.Error.WriteLine("El archivo no tiene ítems, o el tipo no es precios, stock ni activar.");
                return 2;
            }
            if (!Confirmar("Enviar un lote de " + tipo + " con " + n + " ítems")) return 2;
            if (tipo == "precios") return Lote("ActualizarPrecios", c.ActualizarPrecios(lp));
            if (tipo == "stock") return Lote("ActualizarStock", c.ActualizarStock(ls));
            return Lote("ActivarProductos", c.ActivarProductos(la));
        }

        private static void Producto(ProductoV2 p)
        {
            Console.WriteLine("  " + p.Sku + " · " + p.Nombre + " · id " + p.IdProducto + (p.IdCombinacion > 0 ? "/" + p.IdCombinacion : "")
                + " · precio neto " + p.PrecioNeto.ToString(CultureInfo.InvariantCulture) + " · stock " + p.Stock + " · " + (p.Activo ? "activo" : "INACTIVO")
                + (p.EsPack ? " · pack" : "") + (p.TieneCombinaciones ? " · con combinaciones" : "") + " · " + p.PesoKg.ToString(CultureInfo.InvariantCulture) + " kg");
        }

        private static void Orden(OrdenDeVentaV2 o, bool detalle)
        {
            Console.WriteLine("  Orden " + o.IdOrden + " " + o.Referencia + " · " + o.FechaCreacion.ToString("yyyy-MM-dd HH:mm") + " · estado " + o.IdEstado + " " + o.Estado
                + " · integración " + o.EstadoIntegracion + (o.IdVentaAx.Length > 0 ? " (" + o.IdVentaAx + ")" : ""));
            if (o.ErrorCode != 0)
            {
                Console.WriteLine("    ERROR " + o.ErrorCode + ": " + o.ErrorMessage);
                return;
            }
            Console.WriteLine("    " + o.Cliente.NombreCompleto + " · RUT " + o.Cliente.Rut + " · " + o.Cliente.Telefono + " · " + o.Documento.Tipo
                + (o.EsFactura ? " a " + o.Documento.RazonSocial + " (" + o.Documento.EmpresaRut + ", giro " + o.Documento.Giro + ")" : "")
                + " · " + o.DireccionEntrega.Calle + ", " + o.DireccionEntrega.Comuna + ", " + o.DireccionEntrega.Region);
            Console.WriteLine("    Pago: " + o.Pago.MedioPago + (o.Pago.TieneDetalle ? " · autorización " + o.Pago.AuthorizationCode + " · monto " + o.Pago.Amount
                + (o.Pago.Reference.Length > 0 ? " · ref " + o.Pago.Reference : "") : " · sin detalle") + " · total " + o.Totales.TotalPagado.ToString(CultureInfo.InvariantCulture)
                + " (productos " + o.Totales.ProductosBruto.ToString(CultureInfo.InvariantCulture) + " + envío " + o.Totales.EnvioBruto.ToString(CultureInfo.InvariantCulture)
                + " − descuento " + o.Totales.DescuentoBruto.ToString(CultureInfo.InvariantCulture) + ")");
            for (int i = 0; i < o.EnviosCount; i++)
            {
                EnvioV2 e = o.GetEnvio(i);
                Console.WriteLine("    Envío " + e.Secuencia + "/" + o.EnviosCount + " (id " + e.IdEnvio + ") " + e.Servicio + " desde " + e.CentroDistribucion
                    + (e.EsRetiro ? " · retiro en " + e.Tienda.Nombre + " [AX " + (e.Tienda.CodigoAx.Length > 0 ? e.Tienda.CodigoAx : "sin código") + "]" : "")
                    + " · " + e.PesoKg.ToString(CultureInfo.InvariantCulture) + " kg · flete " + e.CostoBruto.ToString(CultureInfo.InvariantCulture)
                    + " · " + e.Estado + (e.Tracking.Length > 0 ? " · tracking " + e.Tracking : "") + " · " + e.LineasCount + " líneas");
            }
            if (!detalle) return;
            for (int i = 0; i < o.LineasCount; i++)
            {
                LineaV2 l = o.GetLinea(i);
                Console.WriteLine("    Línea " + l.Sku + " x" + l.Cantidad + " · neto " + l.TotalNeto.ToString(CultureInfo.InvariantCulture) + " − desc "
                    + l.DescuentoNeto.ToString(CultureInfo.InvariantCulture) + " = " + l.TotalNetoFinal.ToString(CultureInfo.InvariantCulture)
                    + " · envío " + l.IdEnvio + (l.EsComponentePack ? " · de pack " + l.SkuPackPadre : "") + " · " + l.Nombre);
            }
        }
    }
}
