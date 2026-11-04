using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Prestashop.Net.Interno;

namespace Prestashop.Net.Tests
{
    [TestFixture]
    public class PruebasOrdenes
    {
        private static string Fixture()
        {
            string carpeta = Path.GetDirectoryName(new Uri(typeof(PruebasOrdenes).Assembly.CodeBase).LocalPath);
            return File.ReadAllText(Path.Combine(Path.Combine(carpeta, "Fixtures"), "ordenes.json"), Encoding.UTF8);
        }

        private static RespuestaOrdenes Obtener(int errorCodeGlobal)
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, ServidorFalso.Sobre(errorCodeGlobal, "1 de 2 órdenes no se pudieron armar", Fixture())))
            {
                return new Cliente(Util.Config(s.Url)).ObtenerOrdenes();
            }
        }

        [Test]
        public void ResultadoParcialEsVisibleYLasOrdenesSanasVienenCompletas()
        {
            RespuestaOrdenes r = Obtener(Errores.RESULTADO_PARCIAL);
            Assert.AreEqual(Errores.RESULTADO_PARCIAL, r.ErrorCode, "el parcial nunca es ErrorCode 0");
            Assert.AreEqual(2, r.OrdenesCount);
            Assert.AreEqual(1, r.OrdenesConError);
            Assert.AreEqual(3, r.PendientesRestantes);
            Assert.AreEqual(0, r.GetOrden(0).ErrorCode);
            Assert.AreEqual(400, r.GetOrden(1).ErrorCode);
            StringAssert.Contains("dirección de entrega", r.GetOrden(1).ErrorMessage);
            Assert.AreEqual(0, r.GetOrden(1).LineasCount, "la orden con error llega sin datos, no lanza");
            Assert.IsNotNull(r.GetOrden(1).Cliente);
        }

        [Test]
        public void MapeaCabeceraClienteDocumentoYDirecciones()
        {
            OrdenDeVentaV2 o = Obtener(0).GetOrden(0);
            Assert.AreEqual(21L, o.IdOrden);
            Assert.AreEqual("WDXJJQGAZ", o.Referencia);
            Assert.AreEqual(new DateTime(2026, 9, 8, 0, 24, 18), o.FechaCreacion, "hora de la tienda");
            Assert.AreEqual(new DateTime(2026, 9, 8, 3, 24, 18), o.FechaCreacionUtc);
            Assert.AreEqual(Estados.PagoAceptado, o.IdEstado);
            Assert.IsTrue(o.RevisarDireccion);
            Assert.AreEqual("María José Pérez Soto", o.Cliente.NombreCompleto, "nombre en un solo campo, con tildes");
            Assert.AreEqual("+56961234567", o.Cliente.Telefono);
            Assert.AreEqual("123456785", o.Cliente.Rut);
            Assert.IsTrue(o.EsFactura);
            Assert.AreEqual("Constructora Peña Ltda.", o.Documento.RazonSocial);
            Assert.AreEqual("Construcción", o.Documento.Giro);
            Assert.AreEqual("Providencia", o.DireccionEntrega.Comuna);
            Assert.AreEqual("13123", o.DireccionEntrega.IdComuna, "IdComuna lleva el código AX, no el id_state (470) de la tienda");
            Assert.AreEqual("13123", o.DireccionEntrega.ComunaCodAx, "código INE de la comuna para AX");
            Assert.AreEqual("", o.DireccionEntrega.Ciudad);
            Assert.AreEqual("13101", o.Documento.ComunaCodAxEmpresa);
            Assert.AreEqual("", o.DireccionFacturacion.ComunaCodAx, "una tienda sin el campo (módulo 1.1.0) lo deja vacío");
            Assert.AreEqual("", o.DireccionFacturacion.IdComuna, "sin código AX, IdComuna vacío (nunca el id_state)");
            Assert.AreEqual("", o.DireccionFacturacion.Ciudad);
            Assert.AreEqual("Av. O'Higgins 1460", o.DireccionFacturacion.Calle);
            Assert.AreEqual("entregada", o.EstadoIntegracion);
            Assert.AreEqual(2, o.Entregas);
        }

        [Test]
        public void MapeaEnviosConTiendaCentroYTracking()
        {
            OrdenDeVentaV2 o = Obtener(0).GetOrden(0);
            Assert.AreEqual(2, o.EnviosCount);
            Assert.IsTrue(o.TieneRetiroEnTienda);
            EnvioV2 retiro = o.GetEnvio(0);
            Assert.IsTrue(retiro.EsRetiro);
            Assert.AreEqual("LAMPA", retiro.CentroDistribucion);
            Assert.AreEqual("T05", retiro.Tienda.CodigoAx);
            Assert.AreEqual("Santiago Centro", retiro.Tienda.Nombre);
            Assert.AreEqual(EstadosEnvio.EnTienda, retiro.Estado);
            Assert.AreEqual(new DateTime(2026, 9, 9, 10, 0, 0), retiro.FechaEstado);
            EnvioV2 flete = o.GetEnvio(1);
            Assert.AreEqual("flete", flete.Servicio);
            Assert.AreEqual("PUDAHUEL", flete.CentroDistribucion);
            Assert.AreEqual(0, flete.Tienda.IdTienda);
            Assert.AreEqual(10535m, flete.CostoBruto);
            Assert.AreEqual("CHX123", flete.Tracking);
            Assert.AreEqual(DateTime.MinValue, flete.FechaEstado);
            Assert.AreEqual(1, flete.LineasCount);
            Assert.AreEqual(2, flete.GetLinea(0).Cantidad);
        }

        [Test]
        public void MapeaLineasDescuentosPacksYTotales()
        {
            OrdenDeVentaV2 o = Obtener(0).GetOrden(0);
            Assert.AreEqual(3, o.LineasCount);
            LineaV2 a = o.GetLinea(0);
            Assert.IsTrue(a.EsComponentePack);
            Assert.AreEqual("KIT-01", a.SkuPackPadre);
            Assert.AreEqual(7L, a.IdEnvio);
            LineaV2 p = o.GetLinea(2);
            Assert.AreEqual("14P1010001-00303099_048", p.Sku);
            Assert.AreEqual(44525.210084m, p.PrecioNeto, "seis decimales sin pérdida");
            Assert.AreEqual(87298.8m, p.TotalNetoFinal);
            Assert.AreEqual(8L, p.IdEnvio);
            decimal descuento = 0m;
            for (int i = 0; i < o.LineasCount; i++) descuento += o.GetLinea(i).DescuentoNeto;
            Assert.AreEqual(o.Totales.DescuentoNeto, descuento, "el descuento prorrateado cuadra con el total");
            Assert.AreEqual(1, o.DescuentosCount);
            Assert.AreEqual("BIENVENIDA", o.GetDescuento(0).Codigo);
            Assert.AreEqual(187297m, o.Totales.TotalPagado);
            Assert.AreEqual(43.992m, o.Totales.PesoKg);
            Assert.AreEqual(1, o.MensajesCount);
            Assert.AreEqual("Llamar antes de despachar", o.GetMensaje(0).Texto);
        }

        [Test]
        public void MapeaElPagoConLosNombresDeLaDll146()
        {
            PagoV2 p = Obtener(0).GetOrden(0).Pago;
            Assert.AreEqual("Webpay Plus", p.MedioPago);
            Assert.IsTrue(p.TieneDetalle);
            Assert.AreEqual("001213", p.AuthorizationCode, "texto: conserva los ceros a la izquierda");
            Assert.AreEqual(187297, p.Amount);
            Assert.AreEqual("6623", p.Card_number);
            Assert.AreEqual(0, p.ResponseCode);
        }
    }
}
