using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Prestashop.Net.Interno;

namespace Prestashop.Net.Tests
{
    /// <summary>ReabrirOrdenes (2.1.0): AX devuelve a pendiente órdenes ya confirmadas para volver a descargarlas.</summary>
    [TestFixture]
    public class PruebasReapertura
    {
        /// <summary>Como el módulo: 33 reabierta, 34 sin cambio (no estaba confirmada), 35 cancelada (500).</summary>
        private static string Tienda(RequestRecibida q)
        {
            List<Dictionary<string, object>> items = Json.Lista(Json.Objeto(Json.LeerObjeto(q.Cuerpo), "datos"), "items");
            StringBuilder sb = new StringBuilder();
            int malos = 0;
            foreach (Dictionary<string, object> it in items)
            {
                string id = Json.Texto(it, "idOrden");
                int codigo = id == "35" ? 500 : 0;
                if (codigo != 0) malos++;
                if (sb.Length > 0) sb.Append(',');
                sb.Append("{\"clave\":\"" + id + "\",\"errorCode\":" + codigo + ",\"errorMessage\":\""
                    + (codigo != 0 ? "La orden 35 está en el estado \\\"Cancelado\\\" (6), que no se entrega a AX." : "")
                    + "\",\"resultado\":\"" + (id == "34" ? "sin_cambio" : "reabierta") + "\"}");
            }
            return ServidorFalso.Sobre(malos > 0 ? 501 : 0, "", "{\"total\":" + items.Count + ",\"ok\":" + (items.Count - malos) + ",\"fallidos\":" + malos
                + ",\"resultados\":[" + sb + "]}");
        }

        private static RespuestaProgramada Programada()
        {
            RespuestaProgramada r = new RespuestaProgramada();
            r.Generador = Tienda;
            return r;
        }

        [Test]
        public void ReabrirOrdenesEnviaIdYMotivoAOrdenesReabrir()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Programada()))
            {
                LoteReapertura l = new LoteReapertura();
                l.Agregar(33, "Borrada del monitor por error");
                l.Agregar(34, "Reproceso");
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ReabrirOrdenes(l);
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual("POST", s.Requests[0].Metodo);
                Assert.AreEqual("/dap-integracion/ordenes-reabrir", s.Requests[0].Ruta);
                StringAssert.Contains("\"idOrden\":33", s.Requests[0].Cuerpo);
                StringAssert.Contains("\"motivo\":\"Borrada del monitor por error\"", s.Requests[0].Cuerpo);
                Assert.AreEqual(2, r.Correctos);
                Assert.AreEqual("33", r.GetResultado(0).Clave);
                Assert.AreEqual("reabierta", r.GetResultado(0).Resultado);
                Assert.AreEqual("sin_cambio", r.GetResultado(1).Resultado, "no estaba confirmada: igual vuelve a llegar, no es error");
            }
        }

        [Test]
        public void OrdenQueNoSeEntregaDaConflictoYResultadoParcial()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Programada()))
            {
                LoteReapertura l = new LoteReapertura();
                l.Agregar(33, "Borrada del monitor por error");
                l.Agregar(35, "Borrada del monitor por error");
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ReabrirOrdenes(l);
                Assert.AreEqual(Errores.RESULTADO_PARCIAL, r.ErrorCode);
                Assert.AreEqual(1, r.Fallidos);
                Assert.AreEqual(Errores.CONFLICTO_ESTADO, r.GetResultado(1).ErrorCode);
                StringAssert.Contains("Cancelado", r.GetResultado(1).ErrorMessage);
                Assert.AreEqual("", r.GetResultado(1).Resultado, "un ítem con error no informa resultado");
            }
        }

        [Test]
        public void ReabrirOrdenEsUnLoteDeUno()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Programada()))
            {
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ReabrirOrden(33, "Borrada del monitor por error");
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual(1, r.Total);
                Assert.AreEqual("reabierta", r.GetResultado(0).Resultado);
            }
        }

        [Test]
        public void MotivoNuloViajaVacioYLoValidaLaTienda()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Programada()))
            {
                LoteReapertura l = new LoteReapertura();
                l.Agregar(33, null);
                new Cliente(Util.Config(s.Url)).ReabrirOrdenes(l);
                StringAssert.Contains("\"motivo\":\"\"", s.Requests[0].Cuerpo);
            }
        }

        [Test]
        public void PorReferenciaEnviaLaReferenciaSinIdOrden()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, ServidorFalso.Sobre(0, "",
                "{\"total\":1,\"ok\":1,\"fallidos\":0,\"resultados\":[{\"clave\":\"CQCGMZSXH\",\"errorCode\":0,\"errorMessage\":\"\",\"resultado\":\"reabierta\"}]}")))
            {
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ReabrirOrdenPorReferencia("CQCGMZSXH", "Borrada del monitor por error");
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual("/dap-integracion/ordenes-reabrir", s.Requests[0].Ruta);
                StringAssert.Contains("\"referencia\":\"CQCGMZSXH\"", s.Requests[0].Cuerpo);
                StringAssert.Contains("\"motivo\":\"Borrada del monitor por error\"", s.Requests[0].Cuerpo);
                StringAssert.DoesNotContain("idOrden", s.Requests[0].Cuerpo, "por referencia no viaja idOrden");
                Assert.AreEqual("CQCGMZSXH", r.GetResultado(0).Clave, "la clave del resultado es la referencia");
                Assert.AreEqual("reabierta", r.GetResultado(0).Resultado);
            }
        }

        [Test]
        public void LoteMixtoDeIdYReferenciaHeredaLaClaveSiElTramoFalla()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, ServidorFalso.Sobre(Errores.AUTH_CLAVE, "La clave de integración no existe o fue revocada.", "{}")))
            {
                LoteReapertura l = new LoteReapertura();
                l.Agregar(33, "Motivo");
                l.AgregarPorReferencia("CQCGMZSXH", "Motivo");
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ReabrirOrdenes(l);
                Assert.AreEqual(Errores.AUTH_CLAVE, r.ErrorCode);
                Assert.AreEqual("33", r.GetResultado(0).Clave);
                Assert.AreEqual("CQCGMZSXH", r.GetResultado(1).Clave);
            }
        }

        [Test]
        public void LoteVacioNoLlamaALaTienda()
        {
            using (ServidorFalso s = new ServidorFalso())
            {
                RespuestaLote r = new Cliente(Util.Config(s.Url)).ReabrirOrdenes(new LoteReapertura());
                Assert.AreEqual(0, r.ErrorCode);
                Assert.AreEqual(0, s.Requests.Count);
            }
        }
    }
}
