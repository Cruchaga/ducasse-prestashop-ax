using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Prestashop.Net.Interno;

namespace Prestashop.Net.Tests
{
    [TestFixture]
    public class PruebasLotes
    {
        /// <summary>Responde un resultado OK por cada ítem recibido, salvo los SKU que empiezan con "MALO".</summary>
        private static string Eco(RequestRecibida q)
        {
            List<Dictionary<string, object>> items = Json.Lista(Json.Objeto(Json.LeerObjeto(q.Cuerpo), "datos"), "items");
            StringBuilder sb = new StringBuilder();
            int malos = 0;
            foreach (Dictionary<string, object> it in items)
            {
                string clave = Json.Texto(it, "sku").Length > 0 ? Json.Texto(it, "sku") : Json.Texto(it, "idOrden");
                bool malo = clave.StartsWith("MALO");
                if (malo) malos++;
                if (sb.Length > 0) sb.Append(',');
                sb.Append("{\"clave\":\"" + clave + "\",\"errorCode\":" + (malo ? 400 : 0) + ",\"errorMessage\":\"" + (malo ? "El SKU no existe." : "")
                    + "\",\"resultado\":\"actualizado\"}");
            }
            return ServidorFalso.Sobre(malos > 0 ? 501 : 0, "", "{\"total\":" + items.Count + ",\"ok\":" + (items.Count - malos) + ",\"fallidos\":" + malos
                + ",\"resultados\":[" + sb + "]}");
        }

        private static RespuestaProgramada Generada()
        {
            RespuestaProgramada r = new RespuestaProgramada();
            r.Generador = Eco;
            return r;
        }

        [Test]
        public void LoteCorrectoDevuelveUnResultadoPorItem()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Generada()))
            {
                LotePrecios l = new LotePrecios();
                l.Agregar("A-1", 1000.5m);
                l.Agregar("B-2", 2000m);
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ActualizarPrecios(l);
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual(2, r.Total);
                Assert.AreEqual(2, r.Correctos);
                Assert.AreEqual("A-1", r.GetResultado(0).Clave);
                Assert.AreEqual("actualizado", r.GetResultado(0).Resultado);
                StringAssert.Contains("\"precioNeto\":1000.5", s.Requests[0].Cuerpo);
                Assert.AreEqual("/dap-integracion/precios", s.Requests[0].Ruta);
            }
        }

        [Test]
        public void ItemConErrorDaResultadoParcial501()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Generada()))
            {
                LoteStock l = new LoteStock();
                l.Agregar("A-1", 5);
                l.Agregar("MALO-9", 1);
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ActualizarStock(l);
                Assert.AreEqual(Errores.RESULTADO_PARCIAL, r.ErrorCode);
                Assert.AreEqual(1, r.Fallidos);
                Assert.AreEqual(1, r.Correctos);
                Assert.AreEqual(400, r.GetResultado(1).ErrorCode);
                Assert.AreEqual("", r.GetResultado(1).Resultado);
            }
        }

        [Test]
        public void LoteGrandeSeParteEnTramosDe500YSeConsolida()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Generada()).Responder(Generada()).Responder(Generada()))
            {
                LoteActivacion l = new LoteActivacion();
                for (int i = 0; i < 1200; i++) l.Agregar("SKU-" + i, i % 2 == 0);
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ActivarProductos(l);
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual(1200, r.Total);
                Assert.AreEqual(3, s.Requests.Count, "500 + 500 + 200");
                Assert.AreEqual("SKU-0", r.GetResultado(0).Clave);
                Assert.AreEqual("SKU-1199", r.GetResultado(1199).Clave);
                Assert.AreEqual(200, Json.Lista(Json.Objeto(Json.LeerObjeto(s.Requests[2].Cuerpo), "datos"), "items").Count);
            }
        }

        [Test]
        public void SiUnTramoFallaSusItemsHeredanElErrorYElGlobalEsParcial()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Generada()).Responder(401, ServidorFalso.Sobre(201, "La clave fue revocada.", "{}")))
            {
                LoteStock l = new LoteStock();
                for (int i = 0; i < 600; i++) l.Agregar("SKU-" + i, i);
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ActualizarStock(l);
                Assert.AreEqual(Errores.RESULTADO_PARCIAL, r.ErrorCode);
                Assert.AreEqual(500, r.Correctos);
                Assert.AreEqual(100, r.Fallidos);
                Assert.AreEqual(Errores.AUTH_CLAVE, r.GetResultado(599).ErrorCode);
                Assert.AreEqual("SKU-599", r.GetResultado(599).Clave);
            }
        }

        [Test]
        public void SiNadaSeProcesaElGlobalEsLaCausaRealNoParcial()
        {
            string url;
            using (ServidorFalso s = new ServidorFalso()) { url = s.Url; }
            Configuracion c = Util.Config(url);
            c.ReintentosMax = 1;
            LotePrecios l = new LotePrecios();
            l.Agregar("A-1", 1m);
            RespuestaLote r = new Cliente(c).ActualizarPrecios(l);
            Assert.AreEqual(Errores.CONEXION, r.ErrorCode);
            Assert.IsTrue(r.Reintentable);
            Assert.AreEqual(1, r.Fallidos);
            Assert.AreEqual(Errores.CONEXION, r.GetResultado(0).ErrorCode);
        }

        [Test]
        public void LoteVacioEsOkSinLlamarALaTienda()
        {
            using (ServidorFalso s = new ServidorFalso())
            {
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ConfirmarOrdenes(new LoteConfirmacion());
                Assert.AreEqual(0, r.ErrorCode);
                Assert.AreEqual(0, r.Total);
                Assert.AreEqual(0, s.Requests.Count);
            }
        }

        [Test]
        public void CambioDeEstadoEnviaEmpleadoEstadoYEnvios()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Generada()))
            {
                Configuracion c = Util.Config(s.Url);
                c.IdEmpleado = 8;
                CambioEstado ce = new CambioEstado(21, Estados.EnRuta);
                ce.EnviarCorreo = false;
                ce.AgregarEnvio(8, EstadosEnvio.Despachado, "CHX123", "Chilexpress", new DateTime(2026, 10, 1, 9, 30, 0));
                RespuestaLote r = new Cliente(c).ActualizarEstado(ce);
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Dictionary<string, object> datos = Json.Objeto(Json.LeerObjeto(s.Requests[0].Cuerpo), "datos");
                Assert.AreEqual(8, Json.Entero(datos, "idEmpleado"));
                Dictionary<string, object> item = Json.Lista(datos, "items")[0];
                Assert.AreEqual(21, Json.Entero(item, "idOrden"));
                Assert.AreEqual(4, Json.Entero(item, "idEstadoOrden"));
                Assert.IsFalse(Json.Booleano(item, "enviarCorreo"));
                Dictionary<string, object> envio = Json.Lista(item, "envios")[0];
                Assert.AreEqual("despachado", Json.Texto(envio, "estado"));
                Assert.AreEqual("CHX123", Json.Texto(envio, "tracking"));
                Assert.AreEqual("2026-10-01T09:30:00", Json.Texto(envio, "fecha"));
            }
        }
    }
}
