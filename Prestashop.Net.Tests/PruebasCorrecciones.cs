using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Prestashop.Net.Interno;

namespace Prestashop.Net.Tests
{
    /// <summary>
    /// Correcciones de la revisión de código del 2026-10-07 (H-01 a H-07,
    /// ps9.1/dll_ax_dap004/revision-codigo-2026-10-07.md).
    /// </summary>
    [TestFixture]
    public class PruebasCorrecciones
    {
        /// <summary>JSON escrito con comillas simples, para no escapar comillas dobles en el código.</summary>
        private static string J(string s)
        {
            return s.Replace((char)39, (char)34);
        }

        private static string Fixture()
        {
            string carpeta = Path.GetDirectoryName(new Uri(typeof(PruebasCorrecciones).Assembly.CodeBase).LocalPath);
            return File.ReadAllText(Path.Combine(Path.Combine(carpeta, "Fixtures"), "ordenes.json"), Encoding.UTF8);
        }

        // ------------------------------------------------------------------ H-01 enmascarado

        [Test]
        public void H01_ElRutCompactoYLosTextosLibresSeEnmascaranPorClave()
        {
            string m = Mascara.Texto(Fixture(), 0);
            StringAssert.DoesNotContain("123456785", m);
            StringAssert.DoesNotContain("760468096", m);
            StringAssert.Contains(J("'rut':'12.***.***-5'"), m);
            StringAssert.Contains(J("'empresaRut':'76.***.***-6'"), m);
            StringAssert.Contains(J("'razonSocial':'C***'"), m);
            StringAssert.Contains(J("'texto':'L***'"), m);
            StringAssert.DoesNotContain("Llamar antes", m);
            StringAssert.Contains("14P1010001-00303099_048", m); // el SKU no es dato personal
        }

        [Test]
        public void H01_RutEnmascaradoAceptaCualquierFormato()
        {
            Assert.AreEqual("12.***.***-K", Mascara.RutEnmascarado("12.345.678-k"));
            Assert.AreEqual("12.***.***-5", Mascara.RutEnmascarado("123456785"));
            Assert.AreEqual("1.***.***-8", Mascara.RutEnmascarado("1234567-8"));
            Assert.AreEqual("***", Mascara.RutEnmascarado(""));
            StringAssert.Contains(J("'comentario':'R***'"), Mascara.Texto(J("{'comentario':'Retira Juan Pérez'}"), 0));
        }

        // ------------------------------------------------------------------ H-02 CrearProducto tras reintento

        private static ServidorFalso CreacionConTimeout(string nombreEnTienda)
        {
            ServidorFalso s = new ServidorFalso();
            RespuestaProgramada lenta = new RespuestaProgramada();
            lenta.DemoraMs = 1500; // el primer intento crea el producto, pero la respuesta llega tarde
            lenta.Cuerpo = ServidorFalso.Sobre(0, "", J("{'producto':{'sku':'N-1','idProducto':77,'nombre':'Manilla nueva'}}"));
            s.Responder(lenta);
            s.Responder(409, ServidorFalso.Sobre(500, "El SKU N-1 ya existe en PrestaShop.", "{}"));
            s.Responder(200, ServidorFalso.Sobre(0, "", J("{'producto':{'sku':'N-1','idProducto':77,'nombre':'" + nombreEnTienda + "'}}")));
            return s;
        }

        private static RespuestaProducto Crear(ServidorFalso s)
        {
            Configuracion c = Util.Config(s.Url);
            c.TimeoutSegundos = 1;
            ProductoNuevo p = new ProductoNuevo();
            p.Sku = "N-1";
            p.Nombre = " Manilla nueva ";
            return new Cliente(c).CrearProducto(p);
        }

        [Test]
        public void H02_ReintentoQueChocaConYaExisteSeConfirmaConsultandoElSku()
        {
            using (ServidorFalso s = CreacionConTimeout("Manilla nueva"))
            {
                RespuestaProducto r = Crear(s);
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage + " · " + r.Detalle);
                Assert.AreEqual(2, r.Intentos);
                Assert.AreEqual(77, r.Producto.IdProducto);
                StringAssert.Contains("intento anterior", r.Detalle);
                Assert.AreEqual(3, s.Requests.Count);
                Assert.AreEqual("GET", s.Requests[2].Metodo);
                Assert.AreEqual("/dap-integracion/producto?sku=N-1", s.Requests[2].Ruta);
            }
        }

        [Test]
        public void H02_SiElNombreNoCoincideQuedaElConflicto()
        {
            using (ServidorFalso s = CreacionConTimeout("Otro producto"))
            {
                RespuestaProducto r = Crear(s);
                Assert.AreEqual(Errores.CONFLICTO_ESTADO, r.ErrorCode);
                StringAssert.Contains("no coincide", r.Detalle);
            }
        }

        [Test]
        public void H02_ConflictoAlPrimerIntentoNoConsulta()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(409, ServidorFalso.Sobre(500, "El SKU N-1 ya existe en PrestaShop.", "{}")))
            {
                ProductoNuevo p = new ProductoNuevo();
                p.Sku = "N-1";
                p.Nombre = "Manilla nueva";
                RespuestaProducto r = new Cliente(Util.Config(s.Url)).CrearProducto(p);
                Assert.AreEqual(Errores.CONFLICTO_ESTADO, r.ErrorCode);
                Assert.AreEqual(1, r.Intentos);
                Assert.AreEqual(1, s.Requests.Count);
            }
        }

        // ------------------------------------------------------------------ H-03 tope de espera

        [Test]
        public void H03_LaEsperaEntreReintentosTieneTope()
        {
            using (ServidorFalso s = new ServidorFalso())
            {
                for (int i = 0; i < 6; i++)
                {
                    RespuestaProgramada p = new RespuestaProgramada();
                    p.Estado = 503;
                    p.Cuerpo = "<html>mantenimiento</html>";
                    p.ContentType = "text/html";
                    s.Responder(p);
                }
                Configuracion c = Util.Config(s.Url); // base de espera 1 ms
                c.ReintentosMax = 6;
                RespuestaPing r = new Cliente(c).Ping();
                Assert.AreEqual(Errores.MANTENIMIENTO, r.ErrorCode);
                Assert.AreEqual(6, r.Intentos);
                StringAssert.Contains("nuevo intento en 9 ms", r.Traza);
                StringAssert.DoesNotContain("nuevo intento en 27 ms", r.Traza);
            }
        }

        // ------------------------------------------------------------------ H-04 gzip

        private static byte[] Comprimir(string texto)
        {
            MemoryStream ms = new MemoryStream();
            using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress, true))
            {
                byte[] b = Encoding.UTF8.GetBytes(texto);
                gz.Write(b, 0, b.Length);
            }
            return ms.ToArray();
        }

        private static RespuestaProgramada Gzip(byte[] cuerpo)
        {
            RespuestaProgramada p = new RespuestaProgramada();
            p.CuerpoBytes = cuerpo;
            p.ContentEncoding = "gzip";
            return p;
        }

        [Test]
        public void H04_RespuestaGzipSeDescomprime()
        {
            using (ServidorFalso s = new ServidorFalso().Responder(Gzip(Comprimir(Util.PingOk()))))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage);
                Assert.AreEqual(5, r.OrdenesPendientes);
                StringAssert.Contains("gzip", s.Requests[0].Cabeceras["Accept-Encoding"]);
            }
        }

        [Test]
        public void H04_GzipDanadoOCortadoEs110YSeReintenta()
        {
            byte[] bueno = Comprimir(Util.PingOk());
            byte[] cortado = new byte[bueno.Length / 2];
            Array.Copy(bueno, cortado, cortado.Length);
            using (ServidorFalso s = new ServidorFalso()
                .Responder(Gzip(Encoding.ASCII.GetBytes("esto-no-es-gzip-0123456789")))
                .Responder(Gzip(cortado))
                .Responder(Gzip(bueno)))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(0, r.ErrorCode, r.ErrorMessage + " · " + r.Detalle);
                Assert.AreEqual(3, r.Intentos);
                StringAssert.Contains("110", r.Traza);
            }
        }

        [Test]
        public void H04_GzipDanadoPersistenteQuedaReintentableNoComo900()
        {
            byte[] malo = Encoding.ASCII.GetBytes("esto-no-es-gzip-0123456789");
            using (ServidorFalso s = new ServidorFalso().Responder(Gzip(malo)).Responder(Gzip(malo)).Responder(Gzip(malo)))
            {
                RespuestaPing r = new Cliente(Util.Config(s.Url)).Ping();
                Assert.AreEqual(Errores.RESPUESTA_INVALIDA, r.ErrorCode);
                Assert.IsTrue(r.Reintentable);
                Assert.AreEqual(3, r.Intentos);
                StringAssert.Contains("HTTP 200", r.Detalle);
            }
        }

        // ------------------------------------------------------------------ H-05 fechas con zona

        [Test]
        public void H05_LaFechaDelEnvioViajaConZonaSegunKind()
        {
            CambioEstado c = new CambioEstado(21, Estados.EnRuta);
            c.AgregarEnvio(1, EstadosEnvio.Despachado, "", "", new DateTime(2026, 10, 7, 15, 0, 0, DateTimeKind.Utc));
            c.AgregarEnvio(2, EstadosEnvio.Despachado, "", "", new DateTime(2026, 10, 7, 12, 0, 0, DateTimeKind.Local));
            c.AgregarEnvio(3, EstadosEnvio.Despachado, "", "", new DateTime(2026, 10, 7, 12, 0, 0));
            c.AgregarEnvioUtc(4, EstadosEnvio.Despachado, "", "", new DateTime(2026, 10, 7, 15, 0, 0));
            c.AgregarEnvioUtc(5, EstadosEnvio.Despachado, "", "", DateTime.MinValue);
            List<Dictionary<string, object>> e = (List<Dictionary<string, object>>)c.AJson()["envios"];

            Assert.AreEqual("2026-10-07T15:00:00Z", e[0]["fecha"]);
            string local = (string)e[1]["fecha"];
            StringAssert.StartsWith("2026-10-07T12:00:00", local);
            Assert.AreEqual(25, local.Length, local); // con zona: 2026-10-07T12:00:00-03:00
            Assert.AreEqual(new DateTime(2026, 10, 7, 12, 0, 0, DateTimeKind.Local).ToUniversalTime(),
                DateTimeOffset.Parse(local, CultureInfo.InvariantCulture).UtcDateTime);
            Assert.AreEqual("2026-10-07T12:00:00", e[2]["fecha"], "sin Kind: hora de Chile, como hasta ahora");
            Assert.AreEqual("2026-10-07T15:00:00Z", e[3]["fecha"]);
            Assert.AreEqual("", e[4]["fecha"], "MinValue = ahora");
        }

        [Test]
        public void H05_ElJsonEscribeLasFechasConZona()
        {
            Assert.AreEqual(J("'2026-10-07T15:00:00Z'"), Json.Escribir(new DateTime(2026, 10, 7, 15, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual(J("'2026-10-07T15:00:00'"), Json.Escribir(new DateTime(2026, 10, 7, 15, 0, 0)));
        }

        // ------------------------------------------------------------------ H-06 constantes para X++

        private static void Comparar(Type constantes, Type propiedades)
        {
            int n = 0;
            foreach (FieldInfo f in constantes.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!f.IsLiteral) continue;
                PropertyInfo p = propiedades.GetProperty(f.Name, BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(p, propiedades.Name + "." + f.Name + " no existe");
                Assert.AreEqual(f.GetValue(null), p.GetValue(null, null), propiedades.Name + "." + f.Name);
                n++;
            }
            Assert.Greater(n, 0);
            Assert.AreEqual(n, propiedades.GetProperties(BindingFlags.Public | BindingFlags.Static).Length, "sobran propiedades en " + propiedades.Name);
        }

        [Test]
        public void H06_CadaConstanteTieneSuPropiedadEstatica()
        {
            Comparar(typeof(Errores), typeof(CodigoError));
            Comparar(typeof(Estados), typeof(EstadoOrden));
            Comparar(typeof(EstadosEnvio), typeof(EstadoEnvio));
        }

        // ------------------------------------------------------------------ H-07 GetXxx fuera de rango

        [Test]
        public void H07_GetXxxFueraDeRangoDevuelveNullSinLanzar()
        {
            Assert.IsNull(new RespuestaLote().GetResultado(0));
            Assert.IsNull(new RespuestaLote().GetResultado(-1));
            Assert.IsNull(new RespuestaOrdenes().GetOrden(0));
            OrdenDeVentaV2 o = OrdenDeVentaV2.Desde(new Dictionary<string, object>());
            Assert.IsNull(o.GetEnvio(0));
            Assert.IsNull(o.GetLinea(0));
            Assert.IsNull(o.GetDescuento(0));
            Assert.IsNull(o.GetMensaje(-1));
            Assert.IsNull(EnvioV2.Desde(new Dictionary<string, object>()).GetLinea(3));
        }
    }
}
