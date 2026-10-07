using System;
using System.Collections.Generic;
using System.Globalization;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>
    /// Punto de entrada de prestashop-net.dll: todo lo que Dynamics AX hace contra la tienda PrestaShop 9.1.
    /// </summary>
    /// <remarks>
    /// Reglas del contrato:
    ///  - Ningún método lanza excepciones: el resultado está en ErrorCode (0 = OK) y ErrorMessage.
    ///  - "No se pudo consultar" y "no hay datos" son siempre distinguibles.
    ///  - Repetir una llamada no duplica nada: ACK, cantidades y estados son absolutos. CrearProducto no es
    ///    idempotente en el servidor (un SKU repetido da 500); si un reintento choca con eso, la DLL consulta
    ///    el SKU y confirma la creación del intento anterior (ver CrearProducto).
    ///  - Cada llamada lleva un ExecutionId que aparece en el log local y en el registro de la tienda.
    /// </remarks>
    public class Cliente
    {
        /// <summary>Máximo de ítems por request; un lote mayor se parte solo en varias llamadas.</summary>
        internal const int MaxPorRequest = 500;

        private readonly Configuracion cfg;

        public Cliente(Configuracion config)
        {
            cfg = config ?? new Configuracion();
        }

        /// <summary>Forma corta para producción: URL, KeyId y secreto; el resto con valores por defecto.</summary>
        public Cliente(string url, string keyId, string secreto)
        {
            cfg = new Configuracion();
            cfg.Url = url;
            cfg.KeyId = keyId;
            cfg.Secreto = secreto;
        }

        /// <summary>Versión de la DLL.</summary>
        public static string VersionDll { get { return "2.0.1"; } }

        /// <summary>Versión del contrato que habla esta DLL.</summary>
        public static string SchemaVersion { get { return "2.0"; } }

        internal static string AgenteUsuario { get { return "prestashop-net/" + VersionDll; } }

        /// <summary>Configuración en uso (para ajustar timeouts o el log después de crear el cliente).</summary>
        public Configuracion Configuracion { get { return cfg; } }

        private delegate void Mapeador(Dictionary<string, object> datos);

        /// <summary>Ciclo común de toda llamada: validar, llamar, copiar el sobre, mapear los datos y registrar.</summary>
        private void Ejecutar(Respuesta r, string nombre, string metodo, string ruta, Dictionary<string, object> datos, Mapeador mapear)
        {
            DateTime inicio = DateTime.UtcNow;
            r.ExecutionId = Guid.NewGuid().ToString();
            Registro log = null;
            try
            {
                log = new Registro(cfg, r.ExecutionId, nombre);
                string invalida = cfg.Validar();
                if (invalida.Length > 0)
                {
                    r.ErrorCode = Errores.CONFIG_INVALIDA;
                    r.ErrorMessage = "Configuración inválida: " + invalida;
                    log.Error("config", r.ErrorMessage);
                    return;
                }
                log.Info("inicio", metodo + " " + cfg.UrlBase() + ruta + " · " + AgenteUsuario);
                Sobre s = new Transporte(cfg, log).Ejecutar(metodo, ruta, datos, r.ExecutionId);
                r.ErrorCode = s.ErrorCode;
                r.ErrorMessage = s.ErrorMessage;
                r.Detalle = s.Detalle;
                r.Reintentable = s.Reintentable;
                r.SchemaVersion = s.SchemaVersion;
                r.VersionModulo = s.VersionModulo;
                r.Intentos = s.Intentos;
                if (mapear != null && s.Datos.Count > 0)
                {
                    try
                    {
                        mapear(s.Datos);
                    }
                    catch (Exception e)
                    {
                        r.ErrorCode = Errores.RESPUESTA_INVALIDA;
                        r.ErrorMessage = "La respuesta de la tienda no tiene el formato esperado.";
                        r.Detalle = e.GetType().Name + ": " + e.Message;
                        r.Reintentable = false;
                    }
                }
                string fin = "errorCode " + r.ErrorCode + " " + Errores.Nombre(r.ErrorCode) + " · HTTP " + s.HttpStatus + " · intentos " + s.Intentos
                    + " · enviados " + s.BytesEnviados + " B · recibidos " + s.BytesRecibidos + " B · servidor " + s.DuracionServidorMs + " ms · total "
                    + (int)(DateTime.UtcNow - inicio).TotalMilliseconds + " ms" + (r.ErrorCode != 0 ? " · " + r.ErrorMessage + (r.Detalle.Length > 0 ? " · " + r.Detalle : "") : "");
                if (r.ErrorCode == 0) log.Info("fin", fin); else log.Error("fin", fin);
            }
            catch (Exception e)
            {
                r.ErrorCode = Errores.NO_CONTROLADO;
                r.ErrorMessage = "Error no controlado en prestashop-net.dll: " + e.Message;
                r.Detalle = e.ToString();
                r.Reintentable = false;
                try
                {
                    if (log != null) log.Error("excepcion", e.ToString());
                }
                catch (Exception)
                {
                    // Ni siquiera el log puede hacer que una excepción llegue a AX.
                }
            }
            finally
            {
                r.DuracionMs = (int)(DateTime.UtcNow - inicio).TotalMilliseconds;
                if (log != null) r.Traza = log.Traza;
            }
        }

        /// <summary>Ciclo de un lote: lo parte en tramos de MaxPorRequest, llama por tramo y consolida los resultados.</summary>
        private RespuestaLote EjecutarLote(string nombre, string ruta, Lote lote, Dictionary<string, object> extra)
        {
            RespuestaLote r = new RespuestaLote();
            DateTime inicio = DateTime.UtcNow;
            if (lote == null || lote.Count == 0)
            {
                r.ExecutionId = Guid.NewGuid().ToString();
                return r; // lote vacío: nada que hacer, OK sin llamar a la tienda
            }
            int duro = 0;
            string duroMensaje = "";
            string duroDetalle = "";
            bool duroReintentable = false;
            System.Text.StringBuilder traza = new System.Text.StringBuilder();

            for (int desde = 0; desde < lote.Count; desde += MaxPorRequest)
            {
                int n = Math.Min(MaxPorRequest, lote.Count - desde);
                Dictionary<string, object> datos = extra != null ? new Dictionary<string, object>(extra) : new Dictionary<string, object>();
                datos["items"] = lote.Items.GetRange(desde, n);
                Respuesta tramo = new Respuesta();
                List<ResultadoItem> res = new List<ResultadoItem>();
                Ejecutar(tramo, nombre, "POST", ruta, datos, delegate(Dictionary<string, object> d)
                {
                    foreach (Dictionary<string, object> x in Json.Lista(d, "resultados"))
                    {
                        ResultadoItem it = new ResultadoItem();
                        it.Clave = Json.Texto(x, "clave");
                        it.ErrorCode = Json.Entero(x, "errorCode");
                        it.ErrorMessage = Json.Texto(x, "errorMessage");
                        it.Resultado = it.ErrorCode == 0 ? Json.Texto(x, "resultado") : "";
                        res.Add(it);
                    }
                });
                if (res.Count != n)
                {
                    // El tramo no se procesó (red, autenticación, validación global): cada ítem hereda ese error.
                    int codigo = tramo.ErrorCode != 0 && tramo.ErrorCode != Errores.RESULTADO_PARCIAL ? tramo.ErrorCode : Errores.RESPUESTA_INVALIDA;
                    string mensaje = tramo.ErrorMessage.Length > 0 ? tramo.ErrorMessage : "La tienda no devolvió un resultado por cada ítem del lote.";
                    if (duro == 0)
                    {
                        duro = codigo;
                        duroMensaje = mensaje;
                        duroDetalle = tramo.Detalle;
                        duroReintentable = tramo.Reintentable || Errores.EsReintentable(codigo);
                    }
                    res.Clear();
                    for (int i = desde; i < desde + n; i++)
                    {
                        ResultadoItem it = new ResultadoItem();
                        it.Clave = lote.Claves[i];
                        it.ErrorCode = codigo;
                        it.ErrorMessage = mensaje;
                        res.Add(it);
                    }
                }
                r.Resultados.AddRange(res);
                if (r.ExecutionId.Length == 0) r.ExecutionId = tramo.ExecutionId;
                if (tramo.SchemaVersion.Length > 0) r.SchemaVersion = tramo.SchemaVersion;
                if (tramo.VersionModulo.Length > 0) r.VersionModulo = tramo.VersionModulo;
                r.Intentos += tramo.Intentos;
                traza.Append(tramo.Traza);
            }

            r.Total = r.Resultados.Count;
            foreach (ResultadoItem it in r.Resultados) if (it.ErrorCode != 0) r.Fallidos++;
            r.Correctos = r.Total - r.Fallidos;
            if (r.Fallidos > 0)
            {
                if (duro != 0 && r.Correctos == 0)
                {
                    // Nada se procesó: se informa la causa real (p. ej. 100 CONEXION), no un "parcial".
                    r.ErrorCode = duro;
                    r.ErrorMessage = duroMensaje;
                    r.Detalle = duroDetalle;
                    r.Reintentable = duroReintentable;
                }
                else
                {
                    r.ErrorCode = Errores.RESULTADO_PARCIAL;
                    r.ErrorMessage = r.Fallidos + " de " + r.Total + " ítems con error; revise el resultado de cada uno.";
                    r.Reintentable = duroReintentable;
                }
            }
            r.DuracionMs = (int)(DateTime.UtcNow - inicio).TotalMilliseconds;
            r.Traza = traza.ToString();
            return r;
        }

        // ------------------------------------------------------------------ operaciones

        /// <summary>Prueba de conectividad y credenciales. No modifica nada.</summary>
        public RespuestaPing Ping()
        {
            RespuestaPing r = new RespuestaPing();
            Ejecutar(r, "Ping", "GET", "ping", null, delegate(Dictionary<string, object> d)
            {
                r.VersionPs = Json.Texto(d, "versionPs");
                r.HoraServidorUtc = Json.FechaUtc(d, "horaServidor");
                r.OrdenesPendientes = Json.Entero(d, "ordenesPendientes");
                r.ClaveNombre = Json.Texto(d, "claveNombre");
            });
            return r;
        }

        /// <summary>Órdenes pendientes de integrar (hasta 50). Ver <see cref="ObtenerOrdenes(int)"/>.</summary>
        public RespuestaOrdenes ObtenerOrdenes()
        {
            return ObtenerOrdenes(50);
        }

        /// <summary>
        /// Órdenes pagadas que AX aún no confirma, de la más antigua a la más nueva. Una orden se sigue
        /// entregando en cada llamada hasta que AX la confirma con <see cref="ConfirmarOrdenes"/>.
        /// </summary>
        /// <param name="limite">Máximo de órdenes por llamada (1 a 200).</param>
        public RespuestaOrdenes ObtenerOrdenes(int limite)
        {
            RespuestaOrdenes r = new RespuestaOrdenes();
            Dictionary<string, object> datos = new Dictionary<string, object>();
            datos["limite"] = limite < 1 ? 50 : Math.Min(limite, 200);
            Ejecutar(r, "ObtenerOrdenes", "POST", "ordenes", datos, r.Cargar);
            return r;
        }

        /// <summary>Una orden puntual, en cualquier estado (soporte o reproceso). No cambia su estado de integración.</summary>
        public RespuestaOrden ObtenerOrden(long idOrden)
        {
            RespuestaOrden r = new RespuestaOrden();
            Ejecutar(r, "ObtenerOrden", "GET", "orden?id=" + idOrden.ToString(CultureInfo.InvariantCulture), null, delegate(Dictionary<string, object> d)
            {
                Dictionary<string, object> o = Json.Objeto(d, "orden");
                if (o.Count > 0) r.Orden = OrdenDeVentaV2.Desde(o);
            });
            return r;
        }

        /// <summary>
        /// Confirma las órdenes que AX ya integró: dejan de entregarse en ObtenerOrdenes.
        /// Confirmar dos veces la misma orden es inocuo.
        /// </summary>
        public RespuestaLote ConfirmarOrdenes(LoteConfirmacion lote)
        {
            return EjecutarLote("ConfirmarOrdenes", "ordenes-confirmar", lote, null);
        }

        /// <summary>Confirma una sola orden.</summary>
        public RespuestaLote ConfirmarOrden(long idOrden, string idVentaAx)
        {
            LoteConfirmacion l = new LoteConfirmacion();
            l.Agregar(idOrden, idVentaAx);
            return ConfirmarOrdenes(l);
        }

        /// <summary>
        /// Cambia el estado de órdenes y el estado/tracking de sus envíos. Pedir el estado que la orden ya
        /// tiene es inocuo (no repite el correo).
        /// </summary>
        public RespuestaLote ActualizarEstados(LoteEstados lote)
        {
            Dictionary<string, object> extra = new Dictionary<string, object>();
            extra["idEmpleado"] = cfg.IdEmpleado;
            return EjecutarLote("ActualizarEstados", "estados", lote, extra);
        }

        /// <summary>Cambia el estado de una sola orden (con o sin envíos).</summary>
        public RespuestaLote ActualizarEstado(CambioEstado cambio)
        {
            LoteEstados l = new LoteEstados();
            l.Agregar(cambio);
            return ActualizarEstados(l);
        }

        /// <summary>Actualiza precios netos por SKU. Solo se escribe lo que cambió.</summary>
        public RespuestaLote ActualizarPrecios(LotePrecios lote)
        {
            return EjecutarLote("ActualizarPrecios", "precios", lote, null);
        }

        /// <summary>Actualiza el stock disponible por SKU (cantidad absoluta). Solo se escribe lo que cambió.</summary>
        public RespuestaLote ActualizarStock(LoteStock lote)
        {
            return EjecutarLote("ActualizarStock", "stock", lote, null);
        }

        /// <summary>Activa o desactiva productos por SKU (el artículo desmarcado en AX deja de venderse en la tienda).</summary>
        public RespuestaLote ActivarProductos(LoteActivacion lote)
        {
            return EjecutarLote("ActivarProductos", "activar", lote, null);
        }

        /// <summary>Ficha de un producto o combinación por SKU. ErrorCode 400 si no existe.</summary>
        public RespuestaProducto ObtenerProducto(string sku)
        {
            RespuestaProducto r = new RespuestaProducto();
            Ejecutar(r, "ObtenerProducto", "GET", "producto?sku=" + Uri.EscapeDataString(sku ?? ""), null, delegate(Dictionary<string, object> d)
            {
                Dictionary<string, object> p = Json.Objeto(d, "producto");
                if (p.Count > 0) r.Producto = ProductoV2.Desde(p);
            });
            return r;
        }

        /// <summary>
        /// Crea un producto simple. ErrorCode 500 si el SKU ya existe. Si el primer intento se cortó (timeout, red)
        /// y el reintento recibe "ya existe", la DLL consulta el SKU: si el nombre coincide con el enviado lo da por
        /// creado por el intento anterior (ErrorCode 0, Detalle lo explica); si no, deja el 500.
        /// </summary>
        public RespuestaProducto CrearProducto(ProductoNuevo producto)
        {
            ProductoNuevo pn = producto ?? new ProductoNuevo();
            RespuestaProducto r = new RespuestaProducto();
            Dictionary<string, object> datos = new Dictionary<string, object>();
            datos["producto"] = pn.AJson();
            Ejecutar(r, "CrearProducto", "POST", "producto", datos, delegate(Dictionary<string, object> d)
            {
                Dictionary<string, object> p = Json.Objeto(d, "producto");
                if (p.Count > 0) r.Producto = ProductoV2.Desde(p);
            });
            if (r.ErrorCode == Errores.CONFLICTO_ESTADO && r.Intentos > 1) ConfirmarCreacionTrasReintento(r, pn);
            return r;
        }

        /// <summary>
        /// El servidor no es idempotente al crear: si el intento anterior creó el producto pero su respuesta se perdió,
        /// el reintento recibe 500 "ya existe" (revisión 2026-10-07, H-02). Se consulta el SKU para distinguirlo de un
        /// SKU que ya existía antes.
        /// </summary>
        private void ConfirmarCreacionTrasReintento(RespuestaProducto r, ProductoNuevo pn)
        {
            RespuestaProducto c = ObtenerProducto(pn.Sku);
            r.Traza += c.Traza;
            r.DuracionMs += c.DuracionMs;
            string esperado = (pn.Nombre ?? "").Trim();
            if (c.ErrorCode == 0 && c.Producto != null && esperado.Length > 0 && string.Equals((c.Producto.Nombre ?? "").Trim(), esperado, StringComparison.Ordinal))
            {
                r.ErrorCode = Errores.OK;
                r.ErrorMessage = "";
                r.Reintentable = false;
                r.Producto = c.Producto;
                r.Detalle = "El reintento encontró el SKU ya creado con el mismo nombre: se toma como creado por el intento anterior"
                    + " (consulta " + c.ExecutionId + ").";
                return;
            }
            r.Detalle = (r.Detalle.Length > 0 ? r.Detalle + " · " : "") + "Tras un reintento el SKU ya existía y no se pudo confirmar que lo creó el intento anterior"
                + (c.ErrorCode != 0 ? " (la consulta respondió " + c.ErrorCode + ")" : " (el nombre no coincide)") + ".";
        }

        /// <summary>
        /// Publica el documento tributario de una orden para el cliente. Reservado: el servidor responde
        /// ErrorCode 603 hasta que la función se habilite en la tienda.
        /// </summary>
        public RespuestaDte PublicarDte(Dte dte)
        {
            RespuestaDte r = new RespuestaDte();
            Dictionary<string, object> datos = new Dictionary<string, object>();
            datos["dte"] = (dte ?? new Dte()).AJson();
            Ejecutar(r, "PublicarDte", "POST", "dte", datos, delegate(Dictionary<string, object> d)
            {
                r.Url = Json.Texto(d, "url");
            });
            return r;
        }
    }
}
