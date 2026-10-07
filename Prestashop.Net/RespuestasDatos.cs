using System.Collections.Generic;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>
    /// Respuesta de ObtenerOrdenes. ErrorCode 0 = todas completas; 501 = algunas órdenes traen error propio
    /// (OrdenDeVentaV2.ErrorCode): integre y confirme las sanas, no confirme las que tienen error.
    /// Cualquier otro código significa que NO se pudo consultar (no es lo mismo que "no hay órdenes").
    /// </summary>
    public class RespuestaOrdenes : Respuesta
    {
        private readonly List<OrdenDeVentaV2> ordenes = new List<OrdenDeVentaV2>();

        public int OrdenesCount { get { return ordenes.Count; } }
        /// <summary>Elemento i (desde 0), o null si i está fuera de rango (no lanza hacia AX).</summary>
        public OrdenDeVentaV2 GetOrden(int i) { return i >= 0 && i < ordenes.Count ? ordenes[i] : null; }

        /// <summary>Órdenes pendientes que quedaron fuera de esta respuesta por el límite. Si es mayor que 0, confirme y vuelva a llamar.</summary>
        public int PendientesRestantes { get; internal set; }

        /// <summary>Cuántas de las órdenes devueltas traen error propio.</summary>
        public int OrdenesConError
        {
            get
            {
                int n = 0;
                foreach (OrdenDeVentaV2 o in ordenes) if (o.ErrorCode != 0) n++;
                return n;
            }
        }

        internal void Cargar(Dictionary<string, object> datos)
        {
            foreach (Dictionary<string, object> o in Json.Lista(datos, "ordenes")) ordenes.Add(OrdenDeVentaV2.Desde(o));
            PendientesRestantes = Json.Entero(datos, "pendientesRestantes");
        }
    }

    /// <summary>Respuesta de ObtenerOrden (una orden puntual, en cualquier estado).</summary>
    public class RespuestaOrden : Respuesta
    {
        /// <summary>La orden, o null si no existe o no se pudo consultar.</summary>
        public OrdenDeVentaV2 Orden { get; internal set; }
    }

    /// <summary>Respuesta de ObtenerProducto y CrearProducto.</summary>
    public class RespuestaProducto : Respuesta
    {
        /// <summary>El producto, o null si no existe o hubo error.</summary>
        public ProductoV2 Producto { get; internal set; }
    }

    /// <summary>Respuesta de PublicarDte.</summary>
    public class RespuestaDte : Respuesta
    {
        public RespuestaDte()
        {
            Url = "";
        }

        /// <summary>Dirección donde el cliente ve el documento (con su sesión iniciada).</summary>
        public string Url { get; internal set; }
    }
}
