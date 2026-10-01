using System;
using System.IO;
using NUnit.Framework;
using Prestashop.Net.Interno;

namespace Prestashop.Net.Tests
{
    [TestFixture]
    public class PruebasLog
    {
        [Test]
        public void EscribeElArchivoDiarioConElExecutionId()
        {
            string carpeta = Path.Combine(Path.GetTempPath(), "psnet-" + Guid.NewGuid().ToString("N"));
            try
            {
                using (ServidorFalso s = new ServidorFalso().Responder(200, Util.PingOk()))
                {
                    Configuracion c = Util.Config(s.Url);
                    c.LogRuta = carpeta;
                    RespuestaPing r = new Cliente(c).Ping();
                    string[] archivos = Directory.GetFiles(carpeta, "prestashop-net-*.log");
                    Assert.AreEqual(1, archivos.Length);
                    string log = File.ReadAllText(archivos[0]);
                    StringAssert.Contains(r.ExecutionId, log);
                    StringAssert.Contains("| Ping | inicio |", log);
                    StringAssert.Contains("errorCode 0 OK", log);
                    Assert.AreEqual(r.Traza.Trim(), log.Trim(), "la traza de la respuesta es lo que quedó en el archivo");
                }
            }
            finally
            {
                if (Directory.Exists(carpeta)) Directory.Delete(carpeta, true);
            }
        }

        [Test]
        public void EnDebugRegistraElPayloadEnmascaradoYNuncaElSecreto()
        {
            string datos = "{\"producto\":{\"sku\":\"A-1\"},\"email\":\"ivan@hedera.cl\",\"telefono\":\"+56987654321\",\"nombreCompleto\":\"María Pérez\"}";
            using (ServidorFalso s = new ServidorFalso().Responder(200, ServidorFalso.Sobre(0, "", datos)))
            {
                Configuracion c = Util.Config(s.Url);
                c.LogNivel = "Debug";
                RespuestaProducto r = new Cliente(c).ObtenerProducto("A-1");
                StringAssert.Contains("DEBUG", r.Traza);
                StringAssert.Contains("i***@h***.cl", r.Traza);
                Assert.IsFalse(r.Traza.Contains("ivan@hedera.cl"));
                Assert.IsFalse(r.Traza.Contains("87654321"));
                Assert.IsFalse(r.Traza.Contains("Pérez"));
                Assert.IsFalse(r.Traza.Contains(Util.Secreto));
            }
        }

        [Test]
        public void EnInfoNoRegistraPayloads()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, ServidorFalso.Sobre(0, "", "{\"email\":\"ivan@hedera.cl\"}")))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.IsFalse(r.Traza.Contains("DEBUG"));
                Assert.IsFalse(r.Traza.Contains("hedera"));
            }
        }

        [Test]
        public void SiNoSePuedeEscribirElLogLaOperacionSigue()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(200, Util.PingOk()))
            {
                Configuracion c = Util.Config(s.Url);
                c.LogRuta = "Z:|ruta<invalida>";
                RespuestaPing r = new Cliente(c).Ping();
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage + " " + r.Detalle);
                StringAssert.Contains("no se pudo escribir el log", r.Traza);
            }
        }
    }

    [TestFixture]
    public class PruebasMascaraYConfig
    {
        [Test]
        public void EnmascaraCorreoTelefonoRutYDatosPersonales()
        {
            string m = Mascara.Texto("{\"email\":\"jenifer.ortega@dapducasse.cl\",\"rut\":\"12.345.678-K\",\"tel\":\"+56912344321\",\"calle\":\"Av. Siempre Viva 742\",\"secreto\":\"abc\",\"sku\":\"053108001500-00348609_048\"}", 0);
            StringAssert.Contains("j***@d***.cl", m);
            StringAssert.Contains("12.***.***-K", m);
            StringAssert.Contains("+569****4321", m);
            StringAssert.Contains("\"calle\":\"A***\"", m);
            StringAssert.Contains("\"secreto\":\"***\"", m);
            StringAssert.Contains("053108001500-00348609_048", m);
        }

        [Test]
        public void ValidarAceptaUnaConfiguracionCompleta()
        {
            Configuracion c = new Configuracion();
            c.Url = "https://dapducasse.cl/dap-integracion";
            c.KeyId = "dap_x";
            c.Secreto = "s";
            Assert.AreEqual("", c.Validar());
            Assert.AreEqual("https://dapducasse.cl/dap-integracion/", c.UrlBase());
            Assert.AreEqual(120, c.TimeoutSegundos);
            Assert.AreEqual(3, c.ReintentosMax);
        }

        [Test]
        public void ValidarExigeClaveDeBasicAuthSiHayUsuario()
        {
            Configuracion c = Util.Config("https://v3.dapducasse.cl/dap-integracion/");
            c.BasicAuthUsuario = "axdll";
            StringAssert.Contains("BasicAuthClave", c.Validar());
        }

        [Test]
        public void CatalogoDeErrores()
        {
            Assert.AreEqual("RESULTADO_PARCIAL", Errores.Nombre(501));
            Assert.IsTrue(Errores.EsReintentable(Errores.TIMEOUT));
            Assert.IsFalse(Errores.EsReintentable(Errores.VALIDACION));
            Assert.AreEqual(24, Estados.EnTienda);
            Assert.AreEqual(25, Estados.ErrorEnDespacho);
        }
    }
}
