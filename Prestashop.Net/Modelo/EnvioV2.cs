using System;
using System.Collections.Generic;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>
    /// Un envío de la orden: los productos de un centro de distribución que viajan (o se retiran) juntos.
    /// </summary>
    public class EnvioV2
    {
        private readonly List<LineaEnvioV2> lineas = new List<LineaEnvioV2>();

        /// <summary>Id del envío. Es el que se informa en ActualizarEstados para el tracking y el estado.</summary>
        public long IdEnvio { get; internal set; }
        /// <summary>Id del envío nativo de PrestaShop (ps_shipment).</summary>
        public long IdShipment { get; internal set; }
        /// <summary>Número del envío dentro de la orden: 1, 2, 3…</summary>
        public int Secuencia { get; internal set; }
        /// <summary>"despacho" (a domicilio) o "retiro" (en tienda).</summary>
        public string Tipo { get; internal set; }
        /// <summary>"paqueteria", "flete" o "retiro".</summary>
        public string Servicio { get; internal set; }
        /// <summary>Centro de distribución de origen: LAMPA, PUDAHUEL o RENCA.</summary>
        public string CentroDistribucion { get; internal set; }
        public int IdCarrier { get; internal set; }
        public string Carrier { get; internal set; }
        /// <summary>Tienda de retiro (IdTienda = 0 si el envío es despacho a domicilio).</summary>
        public TiendaV2 Tienda { get; internal set; }
        public decimal PesoKg { get; internal set; }
        /// <summary>Flete de este envío, sin IVA.</summary>
        public decimal CostoNeto { get; internal set; }
        public decimal CostoBruto { get; internal set; }
        /// <summary>Estado del envío (ver <see cref="EstadosEnvio"/>).</summary>
        public string Estado { get; internal set; }
        public string Tracking { get; internal set; }
        public DateTime FechaEstado { get; internal set; }

        public int LineasCount { get { return lineas.Count; } }
        /// <summary>Elemento i (desde 0), o null si i está fuera de rango (no lanza hacia AX).</summary>
        public LineaEnvioV2 GetLinea(int i) { return i >= 0 && i < lineas.Count ? lineas[i] : null; }

        public bool EsRetiro { get { return Tipo == "retiro"; } }

        internal static EnvioV2 Desde(Dictionary<string, object> d)
        {
            EnvioV2 x = new EnvioV2();
            x.IdEnvio = Json.Largo(d, "idEnvio");
            x.IdShipment = Json.Largo(d, "idShipment");
            x.Secuencia = Json.Entero(d, "secuencia");
            x.Tipo = Json.Texto(d, "tipo");
            x.Servicio = Json.Texto(d, "servicio");
            x.CentroDistribucion = Json.Texto(d, "centroDistribucion");
            x.IdCarrier = Json.Entero(d, "idCarrier");
            x.Carrier = Json.Texto(d, "carrier");
            x.Tienda = TiendaV2.Desde(Json.Objeto(d, "tienda"));
            x.PesoKg = Json.Decimal(d, "pesoKg");
            x.CostoNeto = Json.Decimal(d, "costoNeto");
            x.CostoBruto = Json.Decimal(d, "costoBruto");
            x.Estado = Json.Texto(d, "estado");
            x.Tracking = Json.Texto(d, "tracking");
            x.FechaEstado = Json.Fecha(d, "fechaEstado");
            foreach (Dictionary<string, object> l in Json.Lista(d, "lineas"))
            {
                LineaEnvioV2 le = new LineaEnvioV2();
                le.IdDetalle = Json.Largo(l, "idDetalle");
                le.Sku = Json.Texto(l, "sku");
                le.Cantidad = Json.Entero(l, "cantidad");
                x.lineas.Add(le);
            }
            return x;
        }
    }

    /// <summary>Tienda de retiro de un envío.</summary>
    public class TiendaV2
    {
        public int IdTienda { get; internal set; }
        /// <summary>Código de la tienda en AX (se configura en el BO de PrestaShop; vacío si aún no se cargó).</summary>
        public string CodigoAx { get; internal set; }
        public string Nombre { get; internal set; }
        public string Direccion { get; internal set; }

        internal static TiendaV2 Desde(Dictionary<string, object> d)
        {
            TiendaV2 x = new TiendaV2();
            x.IdTienda = Json.Entero(d, "idTienda");
            x.CodigoAx = Json.Texto(d, "codigoAx");
            x.Nombre = Json.Texto(d, "nombre");
            x.Direccion = Json.Texto(d, "direccion");
            return x;
        }
    }

    /// <summary>Producto y cantidad que viajan en un envío.</summary>
    public class LineaEnvioV2
    {
        public long IdDetalle { get; internal set; }
        public string Sku { get; internal set; }
        public int Cantidad { get; internal set; }
    }
}
