using System;
using System.Collections.Generic;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>Comprador del pedido.</summary>
    public class ClienteV2
    {
        public long IdCliente { get; internal set; }
        /// <summary>Nombre y apellidos en UN solo campo, tal como los escribió el cliente en el checkout.</summary>
        public string NombreCompleto { get; internal set; }
        public string Email { get; internal set; }
        /// <summary>RUT del comprador, sin puntos ni guion (dígito verificador al final).</summary>
        public string Rut { get; internal set; }
        /// <summary>Teléfono en formato E.164: +56 seguido de 9 dígitos, sin separadores.</summary>
        public string Telefono { get; internal set; }

        internal static ClienteV2 Desde(Dictionary<string, object> d)
        {
            ClienteV2 c = new ClienteV2();
            c.IdCliente = Json.Largo(d, "idCliente");
            c.NombreCompleto = Json.Texto(d, "nombreCompleto");
            c.Email = Json.Texto(d, "email");
            c.Rut = Json.Texto(d, "rut");
            c.Telefono = Json.Texto(d, "telefono");
            return c;
        }
    }

    /// <summary>Documento tributario pedido por el cliente. La emisión la hace AX.</summary>
    public class DocumentoV2
    {
        /// <summary>"boleta" o "factura".</summary>
        public string Tipo { get; internal set; }
        public string EmpresaRut { get; internal set; }
        public string RazonSocial { get; internal set; }
        public string Giro { get; internal set; }
        public string NombreFantasia { get; internal set; }
        public string DireccionEmpresa { get; internal set; }
        public string ComunaEmpresa { get; internal set; }
        public string RegionEmpresa { get; internal set; }

        internal static DocumentoV2 Desde(Dictionary<string, object> d)
        {
            DocumentoV2 x = new DocumentoV2();
            x.Tipo = Json.Texto(d, "tipo");
            if (x.Tipo.Length == 0) x.Tipo = "boleta";
            x.EmpresaRut = Json.Texto(d, "empresaRut");
            x.RazonSocial = Json.Texto(d, "razonSocial");
            x.Giro = Json.Texto(d, "giro");
            x.NombreFantasia = Json.Texto(d, "nombreFantasia");
            x.DireccionEmpresa = Json.Texto(d, "direccionEmpresa");
            x.ComunaEmpresa = Json.Texto(d, "comunaEmpresa");
            x.RegionEmpresa = Json.Texto(d, "regionEmpresa");
            return x;
        }
    }

    /// <summary>Dirección chilena: calle, complemento, comuna y región (no hay campo ciudad).</summary>
    public class DireccionV2
    {
        public long IdDireccion { get; internal set; }
        public string NombreContacto { get; internal set; }
        public string Calle { get; internal set; }
        public string Complemento { get; internal set; }
        public string Comuna { get; internal set; }
        /// <summary>Id de la comuna en PrestaShop (id_state).</summary>
        public int IdComuna { get; internal set; }
        public string Region { get; internal set; }
        public string Telefono { get; internal set; }

        internal static DireccionV2 Desde(Dictionary<string, object> d)
        {
            DireccionV2 x = new DireccionV2();
            x.IdDireccion = Json.Largo(d, "idDireccion");
            x.NombreContacto = Json.Texto(d, "nombreContacto");
            x.Calle = Json.Texto(d, "calle");
            x.Complemento = Json.Texto(d, "complemento");
            x.Comuna = Json.Texto(d, "comuna");
            x.IdComuna = Json.Entero(d, "idComuna");
            x.Region = Json.Texto(d, "region");
            x.Telefono = Json.Texto(d, "telefono");
            return x;
        }
    }

    /// <summary>Totales de la orden. "Neto" = sin IVA; "Bruto" = con IVA.</summary>
    public class TotalesV2
    {
        public decimal ProductosNeto { get; internal set; }
        public decimal ProductosBruto { get; internal set; }
        /// <summary>Suma del flete de todos los envíos.</summary>
        public decimal EnvioNeto { get; internal set; }
        public decimal EnvioBruto { get; internal set; }
        public decimal DescuentoNeto { get; internal set; }
        public decimal DescuentoBruto { get; internal set; }
        public decimal Impuesto { get; internal set; }
        public decimal TotalPagadoNeto { get; internal set; }
        /// <summary>Total pagado por el cliente, con IVA (lo que cobró la pasarela).</summary>
        public decimal TotalPagado { get; internal set; }
        public decimal PesoKg { get; internal set; }

        internal static TotalesV2 Desde(Dictionary<string, object> d)
        {
            TotalesV2 x = new TotalesV2();
            x.ProductosNeto = Json.Decimal(d, "productosNeto");
            x.ProductosBruto = Json.Decimal(d, "productosBruto");
            x.EnvioNeto = Json.Decimal(d, "envioNeto");
            x.EnvioBruto = Json.Decimal(d, "envioBruto");
            x.DescuentoNeto = Json.Decimal(d, "descuentoNeto");
            x.DescuentoBruto = Json.Decimal(d, "descuentoBruto");
            x.Impuesto = Json.Decimal(d, "impuesto");
            x.TotalPagadoNeto = Json.Decimal(d, "totalPagadoNeto");
            x.TotalPagado = Json.Decimal(d, "totalPagado");
            x.PesoKg = Json.Decimal(d, "pesoKg");
            return x;
        }
    }

    /// <summary>Regla de descuento (cupón) aplicada a la orden. El monto ya viene prorrateado en cada línea.</summary>
    public class DescuentoV2
    {
        public string Codigo { get; internal set; }
        public string Nombre { get; internal set; }
        public decimal MontoNeto { get; internal set; }
        public decimal MontoBruto { get; internal set; }
        /// <summary>true si la regla regala el despacho (su monto incluye el flete).</summary>
        public bool EnvioGratis { get; internal set; }

        internal static DescuentoV2 Desde(Dictionary<string, object> d)
        {
            DescuentoV2 x = new DescuentoV2();
            x.Codigo = Json.Texto(d, "codigo");
            x.Nombre = Json.Texto(d, "nombre");
            x.MontoNeto = Json.Decimal(d, "montoNeto");
            x.MontoBruto = Json.Decimal(d, "montoBruto");
            x.EnvioGratis = Json.Booleano(d, "envioGratis");
            return x;
        }
    }

    /// <summary>Comentario que el cliente dejó en el pedido.</summary>
    public class MensajeV2
    {
        public DateTime Fecha { get; internal set; }
        public string Texto { get; internal set; }

        internal static MensajeV2 Desde(Dictionary<string, object> d)
        {
            MensajeV2 x = new MensajeV2();
            x.Fecha = Json.Fecha(d, "fecha");
            x.Texto = Json.Texto(d, "texto");
            return x;
        }
    }
}
