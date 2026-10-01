using System;
using System.Collections.Generic;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>Ficha de un producto (o combinación) de la tienda, identificado por su SKU.</summary>
    public class ProductoV2
    {
        public string Sku { get; internal set; }
        public long IdProducto { get; internal set; }
        /// <summary>0 si el SKU es de un producto simple.</summary>
        public long IdCombinacion { get; internal set; }
        public string Nombre { get; internal set; }
        public string NombreCorto { get; internal set; }
        public string CodigoBarras { get; internal set; }
        public decimal PrecioNeto { get; internal set; }
        public string Unidad { get; internal set; }
        public decimal PesoKg { get; internal set; }
        public decimal AltoCm { get; internal set; }
        public decimal AnchoCm { get; internal set; }
        public decimal ProfundidadCm { get; internal set; }
        public int Stock { get; internal set; }
        public bool Activo { get; internal set; }
        public bool EsPack { get; internal set; }
        public bool TieneCombinaciones { get; internal set; }

        internal static ProductoV2 Desde(Dictionary<string, object> d)
        {
            ProductoV2 p = new ProductoV2();
            p.Sku = Json.Texto(d, "sku");
            p.IdProducto = Json.Largo(d, "idProducto");
            p.IdCombinacion = Json.Largo(d, "idCombinacion");
            p.Nombre = Json.Texto(d, "nombre");
            p.NombreCorto = Json.Texto(d, "nombreCorto");
            p.CodigoBarras = Json.Texto(d, "codigoBarras");
            p.PrecioNeto = Json.Decimal(d, "precioNeto");
            p.Unidad = Json.Texto(d, "unidad");
            p.PesoKg = Json.Decimal(d, "pesoKg");
            p.AltoCm = Json.Decimal(d, "altoCm");
            p.AnchoCm = Json.Decimal(d, "anchoCm");
            p.ProfundidadCm = Json.Decimal(d, "profundidadCm");
            p.Stock = Json.Entero(d, "stock");
            p.Activo = Json.Booleano(d, "activo");
            p.EsPack = Json.Booleano(d, "esPack");
            p.TieneCombinaciones = Json.Booleano(d, "tieneCombinaciones");
            return p;
        }
    }

    /// <summary>
    /// Producto simple a crear desde AX. Nace inactivo (salvo Activo = true) y en la categoría Inicio:
    /// fotos, categoría y descripción las completa el equipo de eCommerce en el back office.
    /// </summary>
    public class ProductoNuevo
    {
        /// <summary>Obligatorio. Código de artículo de AX (ItemId_InventDimId).</summary>
        public string Sku { get; set; }
        /// <summary>Obligatorio.</summary>
        public string Nombre { get; set; }
        public string NombreCorto { get; set; }
        public string Descripcion { get; set; }
        public string CodigoBarras { get; set; }
        public decimal PrecioNeto { get; set; }
        public string Unidad { get; set; }
        public decimal PesoKg { get; set; }
        public decimal AltoCm { get; set; }
        public decimal AnchoCm { get; set; }
        public decimal ProfundidadCm { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }

        internal Dictionary<string, object> AJson()
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["sku"] = Sku ?? "";
            d["nombre"] = Nombre ?? "";
            d["nombreCorto"] = NombreCorto ?? "";
            d["descripcion"] = Descripcion ?? "";
            d["codigoBarras"] = CodigoBarras ?? "";
            d["precioNeto"] = PrecioNeto;
            d["unidad"] = Unidad ?? "";
            d["pesoKg"] = PesoKg;
            d["altoCm"] = AltoCm;
            d["anchoCm"] = AnchoCm;
            d["profundidadCm"] = ProfundidadCm;
            d["stock"] = Stock;
            d["activo"] = Activo;
            return d;
        }
    }

    /// <summary>
    /// Documento tributario (boleta o factura) emitido por AX para publicar en la cuenta del cliente.
    /// Reservado en el contrato 2.0: el servidor responde 603 hasta que se habilite (post go-live).
    /// </summary>
    public class Dte
    {
        public long IdOrden { get; set; }
        /// <summary>"boleta" o "factura".</summary>
        public string Tipo { get; set; }
        public string Folio { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal MontoTotal { get; set; }
        public string NombreArchivo { get; set; }
        /// <summary>PDF en base64 (máximo 2 MB).</summary>
        public string PdfBase64 { get; set; }

        internal Dictionary<string, object> AJson()
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["idOrden"] = IdOrden;
            d["tipo"] = Tipo ?? "";
            d["folio"] = Folio ?? "";
            d["fechaEmision"] = FechaEmision;
            d["montoTotal"] = MontoTotal;
            d["nombreArchivo"] = NombreArchivo ?? "";
            d["pdfBase64"] = PdfBase64 ?? "";
            return d;
        }
    }
}
