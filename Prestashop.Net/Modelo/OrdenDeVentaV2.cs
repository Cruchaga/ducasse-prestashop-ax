using System;
using System.Collections.Generic;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>
    /// Orden de venta de PrestaShop 9.1 enriquecida para AX (contrato 2.0).
    /// Una orden tiene N envíos (uno por centro de distribución o retiro en tienda) y cada línea
    /// indica a qué envío pertenece.
    /// </summary>
    /// <remarks>
    /// Las listas se recorren con XxxCount y GetXxx(i). Si ErrorCode no es 0 la orden NO se pudo armar
    /// completa: no debe integrarse ni confirmarse; volverá en la próxima descarga.
    /// </remarks>
    public class OrdenDeVentaV2
    {
        private readonly List<EnvioV2> envios = new List<EnvioV2>();
        private readonly List<LineaV2> lineas = new List<LineaV2>();
        private readonly List<DescuentoV2> descuentos = new List<DescuentoV2>();
        private readonly List<MensajeV2> mensajes = new List<MensajeV2>();

        /// <summary>Id del pedido en PrestaShop (id_order).</summary>
        public long IdOrden { get; internal set; }
        /// <summary>Referencia que ve el cliente (9 letras).</summary>
        public string Referencia { get; internal set; }
        /// <summary>Fecha de creación en hora de la tienda (Chile).</summary>
        public DateTime FechaCreacion { get; internal set; }
        public DateTime FechaCreacionUtc { get; internal set; }
        /// <summary>Id del estado actual del pedido (ver <see cref="Estados"/>).</summary>
        public int IdEstado { get; internal set; }
        public string Estado { get; internal set; }
        /// <summary>Código ISO de la moneda (CLP).</summary>
        public string Moneda { get; internal set; }
        public long IdCarrito { get; internal set; }
        /// <summary>true si la validación de dirección marcó el pedido para revisión manual.</summary>
        public bool RevisarDireccion { get; internal set; }

        /// <summary>0 = orden completa. Otro valor: no se pudo armar; ver ErrorMessage.</summary>
        public int ErrorCode { get; internal set; }
        public string ErrorMessage { get; internal set; }

        public ClienteV2 Cliente { get; internal set; }
        public DocumentoV2 Documento { get; internal set; }
        /// <summary>Dirección del cliente. Con retiro en tienda sigue siendo la del cliente; la tienda va en el envío.</summary>
        public DireccionV2 DireccionEntrega { get; internal set; }
        public DireccionV2 DireccionFacturacion { get; internal set; }
        public PagoV2 Pago { get; internal set; }
        public TotalesV2 Totales { get; internal set; }

        /// <summary>Estado de integración en la tienda: pendiente, entregada, confirmada o error.</summary>
        public string EstadoIntegracion { get; internal set; }
        /// <summary>Veces que esta orden se entregó a AX sin ser confirmada.</summary>
        public int Entregas { get; internal set; }
        /// <summary>Número de venta de AX informado al confirmar (vacío si aún no se confirma).</summary>
        public string IdVentaAx { get; internal set; }

        public int EnviosCount { get { return envios.Count; } }
        public EnvioV2 GetEnvio(int i) { return envios[i]; }

        public int LineasCount { get { return lineas.Count; } }
        public LineaV2 GetLinea(int i) { return lineas[i]; }

        public int DescuentosCount { get { return descuentos.Count; } }
        public DescuentoV2 GetDescuento(int i) { return descuentos[i]; }

        public int MensajesCount { get { return mensajes.Count; } }
        public MensajeV2 GetMensaje(int i) { return mensajes[i]; }

        /// <summary>true si la orden pide factura (false = boleta).</summary>
        public bool EsFactura { get { return Documento != null && Documento.Tipo == "factura"; } }

        /// <summary>true si algún envío es retiro en tienda.</summary>
        public bool TieneRetiroEnTienda
        {
            get
            {
                foreach (EnvioV2 e in envios) if (e.Tipo == "retiro") return true;
                return false;
            }
        }

        internal static OrdenDeVentaV2 Desde(Dictionary<string, object> d)
        {
            OrdenDeVentaV2 o = new OrdenDeVentaV2();
            o.IdOrden = Json.Largo(d, "idOrden");
            o.Referencia = Json.Texto(d, "referencia");
            o.FechaCreacion = Json.Fecha(d, "fechaCreacion");
            o.FechaCreacionUtc = Json.FechaUtc(d, "fechaCreacion");
            o.IdEstado = Json.Entero(d, "idEstado");
            o.Estado = Json.Texto(d, "estado");
            o.Moneda = Json.Texto(d, "moneda");
            o.IdCarrito = Json.Largo(d, "idCarrito");
            o.RevisarDireccion = Json.Booleano(d, "revisarDireccion");
            o.ErrorCode = Json.Entero(d, "errorCode");
            o.ErrorMessage = Json.Texto(d, "errorMessage");
            o.Cliente = ClienteV2.Desde(Json.Objeto(d, "cliente"));
            o.Documento = DocumentoV2.Desde(Json.Objeto(d, "documento"));
            o.DireccionEntrega = DireccionV2.Desde(Json.Objeto(d, "direccionEntrega"));
            o.DireccionFacturacion = DireccionV2.Desde(Json.Objeto(d, "direccionFacturacion"));
            o.Pago = PagoV2.Desde(Json.Objeto(d, "pago"));
            o.Totales = TotalesV2.Desde(Json.Objeto(d, "totales"));
            Dictionary<string, object> i = Json.Objeto(d, "integracion");
            o.EstadoIntegracion = Json.Texto(i, "estado");
            o.Entregas = Json.Entero(i, "entregas");
            o.IdVentaAx = Json.Texto(i, "idVentaAx");
            foreach (Dictionary<string, object> x in Json.Lista(d, "envios")) o.envios.Add(EnvioV2.Desde(x));
            foreach (Dictionary<string, object> x in Json.Lista(d, "lineas")) o.lineas.Add(LineaV2.Desde(x));
            foreach (Dictionary<string, object> x in Json.Lista(d, "descuentos")) o.descuentos.Add(DescuentoV2.Desde(x));
            foreach (Dictionary<string, object> x in Json.Lista(d, "mensajes")) o.mensajes.Add(MensajeV2.Desde(x));
            return o;
        }
    }
}
