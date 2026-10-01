using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Prestashop.Net.Interno;

namespace Prestashop.Net.Tests
{
    [TestFixture]
    public class PruebasJson
    {
        [Test]
        public void LeeObjetosListasYTiposBasicos()
        {
            Dictionary<string, object> d = Json.LeerObjeto("{ \"a\": 1, \"b\": -2.5, \"c\": \"x\", \"d\": true, \"e\": null, \"f\": [1, {\"g\": \"h\"}], \"i\": {} }");
            Assert.AreEqual(1, Json.Entero(d, "a"));
            Assert.AreEqual(-2.5m, Json.Decimal(d, "b"));
            Assert.AreEqual("x", Json.Texto(d, "c"));
            Assert.IsTrue(Json.Booleano(d, "d"));
            Assert.AreEqual("", Json.Texto(d, "e"));
            Assert.AreEqual(1, Json.Lista(d, "f").Count, "solo los objetos de la lista");
            Assert.AreEqual(0, Json.Objeto(d, "i").Count);
        }

        [Test]
        public void ClaveAusenteDevuelveValorPorDefectoSinLanzar()
        {
            Dictionary<string, object> d = Json.LeerObjeto("{}");
            Assert.AreEqual("", Json.Texto(d, "x"));
            Assert.AreEqual(0, Json.Entero(d, "x"));
            Assert.AreEqual(0m, Json.Decimal(d, "x"));
            Assert.IsFalse(Json.Booleano(d, "x"));
            Assert.AreEqual(0, Json.Lista(d, "x").Count);
            Assert.AreEqual(0, Json.Objeto(d, "x").Count);
            Assert.AreEqual(DateTime.MinValue, Json.Fecha(d, "x"));
        }

        [Test]
        public void LeeEscapesYUnicode()
        {
            string b = ((char)92).ToString(); // barra invertida
            Dictionary<string, object> d = Json.LeerObjeto("{\"t\":\"l" + b + "u00ednea " + b + "\"1" + b + "\"" + b + "nPe" + b + "u00f1a " + b + "/ O'Higgins" + b + b + "\"}");
            Assert.AreEqual("línea \"1\"\nPeña / O'Higgins" + b, Json.Texto(d, "t"));
        }

        [Test]
        public void NumerosConDecimalesYCultura()
        {
            Dictionary<string, object> d = Json.LeerObjeto("{\"p\":53550.420168,\"e\":1.5e3,\"n\":187297}");
            Assert.AreEqual(53550.420168m, Json.Decimal(d, "p"));
            Assert.AreEqual(1500m, Json.Decimal(d, "e"));
            Assert.AreEqual(187297L, Json.Largo(d, "n"));
        }

        [Test]
        public void RechazaJsonInvalidoODemasiadoAnidado()
        {
            Assert.Throws<FormatException>(delegate { Json.Leer("<html>503</html>"); });
            Assert.Throws<FormatException>(delegate { Json.Leer("{\"a\":1"); });
            Assert.Throws<FormatException>(delegate { Json.Leer("{\"a\":1} basura"); });
            Assert.Throws<FormatException>(delegate { Json.Leer(new string('[', 200) + new string(']', 200)); });
        }

        [Test]
        public void EscribeYVuelveALeerLoMismo()
        {
            Dictionary<string, object> d = new Dictionary<string, object>();
            d["sku"] = "A \"1\"\nñ";
            d["precio"] = 1234.5m;
            d["n"] = 7;
            d["ok"] = true;
            List<object> l = new List<object>();
            l.Add(new Dictionary<string, object>());
            d["items"] = l;
            string json = Json.Escribir(d);
            StringAssert.Contains("\"precio\":1234.5", json, "decimales con punto, sin importar la cultura de la máquina");
            Dictionary<string, object> v = Json.LeerObjeto(json);
            Assert.AreEqual("A \"1\"\nñ", Json.Texto(v, "sku"));
            Assert.AreEqual(1234.5m, Json.Decimal(v, "precio"));
            Assert.AreEqual(1, Json.Lista(v, "items").Count);
        }

        [Test]
        public void FechaIsoConZonaSeLeeEnHoraDeLaTiendaYEnUtc()
        {
            Dictionary<string, object> d = Json.LeerObjeto("{\"f\":\"2026-09-08T00:24:18-03:00\"}");
            Assert.AreEqual(new DateTime(2026, 9, 8, 0, 24, 18), Json.Fecha(d, "f"));
            Assert.AreEqual(new DateTime(2026, 9, 8, 3, 24, 18), Json.FechaUtc(d, "f"));
        }
    }

    [TestFixture]
    public class PruebasFirma
    {
        // Vector COMPARTIDO con tests/unit_test.php del módulo dapintegracion. Si un lado cambia la firma, falla acá o allá.
        private const string Secreto = "secreto-de-prueba-0123456789";
        private const string Cuerpo = "{\"schemaVersion\":\"2.0\",\"executionId\":\"t\",\"datos\":{\"limite\":50}}";

        [Test]
        public void Sha256DelCuerpoCoincideConPhp()
        {
            Assert.AreEqual("f97e1b76370d90aaff1211769bd6020b52322b354046f39aca72577f0d1c7cb2", Firma.Sha256Hex(Encoding.UTF8.GetBytes(Cuerpo)));
        }

        [Test]
        public void FirmaPostCoincideConPhp()
        {
            string cadena = Firma.Cadena("post", "/dap-integracion/ordenes", "1790000000", "dap_0123456789abcdef", Encoding.UTF8.GetBytes(Cuerpo));
            Assert.AreEqual("3e1e8995b0635d7f2a5fa19d1f1e80ffa977a303a122c7cce676534f19a5586f", Firma.Firmar(Secreto, cadena));
        }

        [Test]
        public void FirmaGetConQueryYCuerpoVacioCoincideConPhp()
        {
            string cadena = Firma.Cadena("GET", "/dap-integracion/orden?id=21", "1790000000", "dap_0123456789abcdef", new byte[0]);
            Assert.AreEqual("404a152a248ec28045137ed35393ae6c72ba2b411057ea70a6dbbf3dce52e50f", Firma.Firmar(Secreto, cadena));
        }

        [Test]
        public void TimestampEsEpochEnSegundos()
        {
            long t = long.Parse(Firma.TimestampAhora());
            Assert.Greater(t, 1700000000L);
            Assert.Less(t, 4000000000L);
        }
    }
}
