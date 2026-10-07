using System;
using System.Text;
using NUnit.Framework;
using Prestashop.Net.Interno;

namespace Prestashop.Net.Tests
{
    internal static class Util
    {
        public const string KeyId = "dap_0123456789abcdef";
        public const string Secreto = "secreto-de-prueba-0123456789";

        public static Configuracion Config(string url)
        {
            Configuracion.PermitirHttpEnPruebas = true; // el servidor falso no tiene TLS
            Transporte.EsperaBaseMs = 1;                // reintentos sin esperar 1 s / 3 s / 9 s
            Configuracion c = new Configuracion();
            c.Url = url;
            c.KeyId = KeyId;
            c.Secreto = Secreto;
            c.LogRuta = "-";
            c.SinProxy = true;
            return c;
        }

        public static string PingOk()
        {
            return ServidorFalso.Sobre(0, "", "{\"versionPs\":\"9.1.5\",\"horaServidor\":\"2026-10-01T03:37:50Z\",\"ordenesPendientes\":5,\"claveNombre\":\"pruebas\"}");
        }
    }

    [TestFixture]
    public class PruebasTransporte
    {
        [Test]
        public void PingCorrectoMapeaLosDatosYElSobre()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, Util.PingOk()))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage + " " + r.Detalle);
                Assert.IsTrue(r.Ok);
                Assert.AreEqual("9.1.5", r.VersionPs);
                Assert.AreEqual(5, r.OrdenesPendientes);
                Assert.AreEqual(new DateTime(2026, 10, 1, 3, 37, 50), r.HoraServidorUtc);
                Assert.AreEqual("1.0.0", r.VersionModulo);
                Assert.AreEqual("2.0", r.SchemaVersion);
                Assert.AreEqual(1, r.Intentos);
                Assert.AreEqual(36, r.ExecutionId.Length, "ExecutionId es un GUID");
                StringAssert.Contains(r.ExecutionId, r.Traza);
            }
        }

        [Test]
        public void CadaRequestVaFirmadaYConLasCabecerasDelContrato()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, ServidorFalso.Sobre(0, "", "{\"ordenes\":[],\"pendientesRestantes\":0}")))
            {
                Configuracion c = Util.Config(s.Url);
                c.BasicAuthUsuario = "axdll";
                c.BasicAuthClave = "clave-staging";
                RespuestaOrdenes r = new Cliente(c).ObtenerOrdenes(25);
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                RequestRecibida q = s.Requests[0];
                Assert.AreEqual("POST", q.Metodo);
                Assert.AreEqual("/dap-integracion/ordenes", q.Ruta);
                Assert.AreEqual(Util.KeyId, q.Cabeceras["X-Dap-KeyId"]);
                Assert.AreEqual("2.0", q.Cabeceras["X-Dap-Schema"]);
                Assert.AreEqual("prestashop-net/" + Cliente.VersionDll, q.Cabeceras["X-Dap-Client"]);
                Assert.AreEqual(r.ExecutionId, q.Cabeceras["X-Dap-ExecutionId"]);
                Assert.AreEqual("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("axdll:clave-staging")), q.Cabeceras["Authorization"]);
                StringAssert.Contains("\"limite\":25", q.Cuerpo);
                StringAssert.Contains("\"executionId\":\"" + r.ExecutionId + "\"", q.Cuerpo);
                // La firma se puede recalcular con el secreto: es la del método, la ruta, el timestamp, el KeyId y el cuerpo.
                string esperada = Firma.Firmar(Util.Secreto, Firma.Cadena("POST", q.Ruta, q.Cabeceras["X-Dap-Timestamp"], Util.KeyId, Encoding.UTF8.GetBytes(q.Cuerpo)));
                Assert.AreEqual(esperada, q.Cabeceras["X-Dap-Signature"]);
                Assert.IsFalse(q.Cuerpo.Contains(Util.Secreto), "el secreto nunca viaja");
            }
        }

        [Test]
        public void ReintentaErroresTransitoriosYTerminaBien()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(500, "<html>error</html>").Responder(502, "bad gateway").Responder(200, Util.PingOk()))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual(3, r.Intentos);
                Assert.AreEqual(3, s.Requests.Count);
                StringAssert.Contains("reintento", r.Traza);
            }
        }

        [Test]
        public void AgotaLosReintentosYDevuelveElErrorComoReintentable()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(503, "<h1>mantenimiento</h1>").Responder(503, "x").Responder(503, "x"))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(Errores.MANTENIMIENTO, r.ErrorCode);
                Assert.IsTrue(r.Reintentable);
                Assert.AreEqual(3, r.Intentos);
                StringAssert.Contains("HTTP 503", r.Detalle);
            }
        }

        [Test]
        public void NoReintentaErroresDeAutenticacion()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(401, ServidorFalso.Sobre(200, "La firma de la request no es válida.", "{}")))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(Errores.AUTH_FIRMA, r.ErrorCode);
                Assert.AreEqual("La firma de la request no es válida.", r.ErrorMessage);
                Assert.IsFalse(r.Reintentable);
                Assert.AreEqual(1, s.Requests.Count);
            }
        }

        [Test]
        public void UnaRedireccionNoSeSigueYSeReportaComo103()
        {
            RespuestaProgramada redir = new RespuestaProgramada();
            redir.Estado = 301;
            redir.Location = "https://otro.sitio/dap-integracion/ping";
            redir.ContentType = "text/html";
            using (ServidorFalso s = new ServidorFalso().Responder(redir).Responder(200, Util.PingOk()))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(Errores.REDIRECCION, r.ErrorCode);
                StringAssert.Contains("otro.sitio", r.Detalle);
                Assert.AreEqual(1, s.Requests.Count, "no se siguió la redirección ni se reintentó");
            }
        }

        [Test]
        public void RespuestaQueNoEsElJsonDelContratoEs110()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, "<html>pagina de un proxy</html>"))
            {
                Configuracion c = Util.Config(s.Url);
                c.ReintentosMax = 1;
                RespuestaPing r = new Cliente(c).Ping();
                Assert.AreEqual(Errores.RESPUESTA_INVALIDA, r.ErrorCode);
                StringAssert.Contains("pagina de un proxy", r.Detalle);
            }
        }

        [Test]
        public void El401DeApacheSinSobreApuntaAlBasicAuth()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(401, "<html>401 Unauthorized</html>"))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(Errores.AUTH_FIRMA, r.ErrorCode);
                StringAssert.Contains("BasicAuth", r.ErrorMessage);
                Assert.AreEqual(1, s.Requests.Count);
            }
        }

        [Test]
        public void El404SinSobreIndicaUrlOModuloIncorrecto()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(404, "<html>404</html>"))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(Errores.NO_ENCONTRADO, r.ErrorCode);
                Assert.IsFalse(r.Reintentable);
            }
        }

        [Test]
        public void TimeoutEs101()
        {
            RespuestaProgramada lenta = new RespuestaProgramada();
            lenta.DemoraMs = 3000;
            lenta.Cuerpo = Util.PingOk();
            using (ServidorFalso s = new ServidorFalso().Responder(lenta))
            {
                Configuracion c = Util.Config(s.Url);
                c.TimeoutSegundos = 1;
                c.ReintentosMax = 1;
                RespuestaPing r = new Cliente(c).Ping();
                Assert.AreEqual(Errores.TIMEOUT, r.ErrorCode, r.Detalle);
                Assert.IsTrue(r.Reintentable);
            }
        }

        [Test]
        public void ConexionRechazadaEs100YNoLanza()
        {
            string url;
            using (ServidorFalso s = new ServidorFalso())
            {
                url = s.Url; // al salir del using el puerto queda cerrado
            }
            Configuracion c = Util.Config(url);
            c.ReintentosMax = 2;
            RespuestaOrdenes r = new Cliente(c).ObtenerOrdenes();
            Assert.AreEqual(Errores.CONEXION, r.ErrorCode, r.Detalle);
            Assert.IsTrue(r.Reintentable);
            Assert.AreEqual(2, r.Intentos);
            Assert.AreEqual(0, r.OrdenesCount, "sin órdenes Y con error: nunca 'cero órdenes con ErrorCode 0'");
        }

        [Test]
        public void ConfiguracionInvalidaEs301SinLlamarALaTienda()
        {
            using (ServidorFalso s = new ServidorFalso())
            {
                Configuracion c = Util.Config(s.Url);
                c.Secreto = "";
                RespuestaPing r = new Cliente(c).Ping();
                Assert.AreEqual(Errores.CONFIG_INVALIDA, r.ErrorCode);
                StringAssert.Contains("Secreto", r.ErrorMessage);
                Assert.AreEqual(0, s.Requests.Count);
            }
        }

        [Test]
        public void HttpSinTlsSeRechazaEnLaConfiguracion()
        {
            Configuracion c = Util.Config("http://dapducasse.cl/dap-integracion/");
            Configuracion.PermitirHttpEnPruebas = false;
            try
            {
                RespuestaPing r = new Cliente(c).Ping();
                Assert.AreEqual(Errores.CONFIG_INVALIDA, r.ErrorCode);
                StringAssert.Contains("https://", r.ErrorMessage);
            }
            finally
            {
                Configuracion.PermitirHttpEnPruebas = true;
            }
        }

        [Test]
        public void ClienteSinConfiguracionNoLanza()
        {
            RespuestaPing r = new Cliente((Configuracion)null).Ping();
            Assert.AreEqual(Errores.CONFIG_INVALIDA, r.ErrorCode);
            Assert.IsFalse(r.Ok);
        }

        [Test]
        public void ErrorDeNegocioDelServidorLlegaConSuCodigoYMensaje()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(404, ServidorFalso.Sobre(400, "La orden 999 no existe.", "{}")))
            {
                RespuestaOrden r = new Cliente(Util.Config(s.Url)).ObtenerOrden(999);
                Assert.AreEqual(Errores.NO_ENCONTRADO, r.ErrorCode);
                Assert.AreEqual("La orden 999 no existe.", r.ErrorMessage);
                Assert.IsNull(r.Orden);
                Assert.AreEqual("/dap-integracion/orden?id=999", s.Requests[0].Ruta);
                Assert.AreEqual("GET", s.Requests[0].Metodo);
            }
        }

        [Test]
        public void VersionDeContratoIncompatibleEs700()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(409, ServidorFalso.Sobre(700, "Versión de contrato no soportada.", "{}")))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(Errores.VERSION_CONTRATO, r.ErrorCode);
                Assert.AreEqual(1, s.Requests.Count);
            }
        }

        [Test]
        public void ElSkuViajaEscapadoEnLaQueryYLaFirmaLoCubre()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, ServidorFalso.Sobre(0, "", "{\"producto\":{\"sku\":\"A B/1\",\"precioNeto\":10.5,\"stock\":3,\"activo\":true}}")))
            {
                RespuestaProducto r = new Cliente(Util.Config(s.Url)).ObtenerProducto("A B/1");
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual(10.5m, r.Producto.PrecioNeto);
                Assert.IsTrue(r.Producto.Activo);
                RequestRecibida q = s.Requests[0];
                Assert.AreEqual("/dap-integracion/producto?sku=A%20B%2F1", q.Ruta);
                Assert.AreEqual(Firma.Firmar(Util.Secreto, Firma.Cadena("GET", q.Ruta, q.Cabeceras["X-Dap-Timestamp"], Util.KeyId, new byte[0])), q.Cabeceras["X-Dap-Signature"]);
            }
        }
    }
}
