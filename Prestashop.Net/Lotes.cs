using System;
using System.Collections.Generic;
using System.Globalization;

namespace Prestashop.Net
{
    /// <summary>Base de los lotes de entrada: se arman con Agregar(...) y se envían enteros.</summary>
    public abstract class Lote
    {
        internal readonly List<Dictionary<string, object>> Items = new List<Dictionary<string, object>>();
        internal readonly List<string> Claves = new List<string>();

        /// <summary>Cantidad de ítems agregados.</summary>
        public int Count { get { return Items.Count; } }

        /// <summary>Vacía el lote para reutilizar el objeto.</summary>
        public void Limpiar()
        {
            Items.Clear();
            Claves.Clear();
        }

        internal void Sumar(string clave, Dictionary<string, object> item)
        {
            Claves.Add(clave ?? "");
            Items.Add(item);
        }
    }

    /// <summary>Órdenes que AX ya integró. Para Cliente.ConfirmarOrdenes.</summary>
    public class LoteConfirmacion : Lote
    {
        /// <param name="idOrden">OrdenDeVentaV2.IdOrden.</param>
        /// <param name="idVentaAx">Número de la venta creada en AX (SalesId). Queda registrado en la tienda.</param>
        public void Agregar(long idOrden, string idVentaAx)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["idOrden"] = idOrden;
            d["idVentaAx"] = idVentaAx ?? "";
            Sumar(idOrden.ToString(CultureInfo.InvariantCulture), d);
        }
    }

    /// <summary>Precios netos (sin IVA) por SKU. Para Cliente.ActualizarPrecios.</summary>
    public class LotePrecios : Lote
    {
        public void Agregar(string sku, decimal precioNeto)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["sku"] = sku ?? "";
            d["precioNeto"] = precioNeto;
            Sumar(sku, d);
        }
    }

    /// <summary>Stock disponible por SKU, en cantidad ABSOLUTA (no un delta). Para Cliente.ActualizarStock.</summary>
    public class LoteStock : Lote
    {
        public void Agregar(string sku, int cantidad)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["sku"] = sku ?? "";
            d["cantidad"] = cantidad;
            Sumar(sku, d);
        }
    }

    /// <summary>Productos a activar o desactivar por SKU. Para Cliente.ActivarProductos.</summary>
    public class LoteActivacion : Lote
    {
        public void Agregar(string sku, bool activo)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["sku"] = sku ?? "";
            d["activo"] = activo;
            Sumar(sku, d);
        }
    }

    /// <summary>Cambios de estado de órdenes y de sus envíos. Para Cliente.ActualizarEstados.</summary>
    public class LoteEstados : Lote
    {
        public void Agregar(CambioEstado cambio)
        {
            if (cambio == null) return;
            Sumar(cambio.IdOrden.ToString(CultureInfo.InvariantCulture), cambio.AJson());
        }
    }

    /// <summary>
    /// Cambio de estado de UNA orden: el estado del pedido (el que ve el cliente y dispara el correo)
    /// y, opcionalmente, el estado y tracking de cada envío.
    /// </summary>
    public class CambioEstado
    {
        private readonly List<Dictionary<string, object>> envios = new List<Dictionary<string, object>>();

        public CambioEstado()
        {
            EnviarCorreo = true;
        }

        public CambioEstado(long idOrden, int idEstadoOrden) : this()
        {
            IdOrden = idOrden;
            IdEstadoOrden = idEstadoOrden;
        }

        public long IdOrden { get; set; }

        /// <summary>Estado de pedido de destino (ver <see cref="Estados"/>). 0 = no cambiar el estado del pedido.</summary>
        public int IdEstadoOrden { get; set; }

        /// <summary>true (por defecto) envía al cliente el correo del estado, si el estado tiene correo.</summary>
        public bool EnviarCorreo { get; set; }

        /// <summary>Nota interna que queda en el pedido (no la ve el cliente).</summary>
        public string Comentario { get; set; }

        /// <summary>Informa el estado y el tracking de un envío de la orden.</summary>
        /// <param name="idEnvio">EnvioV2.IdEnvio.</param>
        /// <param name="estadoEnvio">Ver <see cref="EstadosEnvio"/>. Vacío = no cambiar el estado del envío.</param>
        /// <param name="tracking">Número de seguimiento del transportista. Vacío = no cambiar.</param>
        /// <param name="carrier">Nombre del transportista real (p. ej. "Chilexpress"). Vacío = no cambiar.</param>
        /// <param name="fecha">Fecha del evento. DateTime.MinValue = ahora.</param>
        public void AgregarEnvio(long idEnvio, string estadoEnvio, string tracking, string carrier, DateTime fecha)
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["idEnvio"] = idEnvio;
            d["estado"] = estadoEnvio ?? "";
            d["tracking"] = tracking ?? "";
            d["carrier"] = carrier ?? "";
            d["fecha"] = fecha.Year > 2000 ? fecha.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture) : "";
            envios.Add(d);
        }

        /// <summary>Igual que el anterior, con la fecha de ahora.</summary>
        public void AgregarEnvio(long idEnvio, string estadoEnvio, string tracking, string carrier)
        {
            AgregarEnvio(idEnvio, estadoEnvio, tracking, carrier, DateTime.MinValue);
        }

        internal Dictionary<string, object> AJson()
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["idOrden"] = IdOrden;
            d["idEstadoOrden"] = IdEstadoOrden;
            d["enviarCorreo"] = EnviarCorreo;
            d["comentario"] = Comentario ?? "";
            d["envios"] = envios;
            return d;
        }
    }
}
