using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Prestashop.Net.Tests
{
    /// <summary>Request tal como la recibió el servidor falso.</summary>
    internal sealed class RequestRecibida
    {
        public string Metodo = "";
        public string Ruta = "";
        public Dictionary<string, string> Cabeceras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string Cuerpo = "";
    }

    /// <summary>Respuesta programada para el servidor falso.</summary>
    internal sealed class RespuestaProgramada
    {
        public int Estado = 200;
        public string Cuerpo = "";
        public string ContentType = "application/json; charset=utf-8";
        public string Location;
        public int DemoraMs;
        /// <summary>Si se define, el cuerpo se calcula a partir de la request (para lotes).</summary>
        public Converter<RequestRecibida, string> Generador;
    }

    /// <summary>
    /// Servidor HTTP mínimo sobre TcpListener para probar el transporte de la DLL sin red ni permisos
    /// especiales: responde, en orden, las respuestas programadas y guarda las requests recibidas.
    /// </summary>
    internal sealed class ServidorFalso : IDisposable
    {
        private readonly TcpListener oyente;
        private readonly Thread hilo;
        private readonly Queue<RespuestaProgramada> cola = new Queue<RespuestaProgramada>();
        private volatile bool activo = true;

        public readonly List<RequestRecibida> Requests = new List<RequestRecibida>();

        public ServidorFalso()
        {
            oyente = new TcpListener(IPAddress.Loopback, 0);
            oyente.Start();
            hilo = new Thread(Atender);
            hilo.IsBackground = true;
            hilo.Start();
        }

        public string Url { get { return "http://127.0.0.1:" + ((IPEndPoint)oyente.LocalEndpoint).Port + "/dap-integracion/"; } }

        public ServidorFalso Responder(int estado, string cuerpo)
        {
            RespuestaProgramada r = new RespuestaProgramada();
            r.Estado = estado;
            r.Cuerpo = cuerpo;
            lock (cola) cola.Enqueue(r);
            return this;
        }

        public ServidorFalso Responder(RespuestaProgramada r)
        {
            lock (cola) cola.Enqueue(r);
            return this;
        }

        /// <summary>Sobre JSON del contrato con el código y los datos indicados.</summary>
        public static string Sobre(int errorCode, string mensaje, string datosJson)
        {
            return "{\"schemaVersion\":\"2.0\",\"executionId\":\"srv\",\"errorCode\":" + errorCode + ",\"errorMessage\":\"" + mensaje
                + "\",\"detalle\":\"\",\"reintentable\":false,\"versionModulo\":\"1.0.0\",\"duracionMs\":7,\"datos\":" + datosJson + "}";
        }

        private void Atender()
        {
            while (activo)
            {
                TcpClient cliente;
                try
                {
                    cliente = oyente.AcceptTcpClient();
                }
                catch (Exception)
                {
                    return; // servidor detenido
                }
                try
                {
                    using (cliente)
                    using (NetworkStream red = cliente.GetStream())
                    {
                        RequestRecibida req = Leer(red);
                        lock (Requests) Requests.Add(req);
                        RespuestaProgramada r;
                        lock (cola) r = cola.Count > 0 ? cola.Dequeue() : null;
                        if (r == null)
                        {
                            r = new RespuestaProgramada();
                            r.Estado = 500;
                            r.Cuerpo = "sin respuesta programada";
                            r.ContentType = "text/plain";
                        }
                        if (r.DemoraMs > 0) Thread.Sleep(r.DemoraMs);
                        string cuerpoTexto = r.Generador != null ? r.Generador(req) : r.Cuerpo;
                        byte[] cuerpo = Encoding.UTF8.GetBytes(cuerpoTexto);
                        StringBuilder cab = new StringBuilder();
                        cab.Append("HTTP/1.1 ").Append(r.Estado).Append(" X\r\n");
                        cab.Append("Content-Type: ").Append(r.ContentType).Append("\r\n");
                        cab.Append("Content-Length: ").Append(cuerpo.Length).Append("\r\n");
                        if (r.Location != null) cab.Append("Location: ").Append(r.Location).Append("\r\n");
                        cab.Append("Connection: close\r\n\r\n");
                        byte[] c = Encoding.ASCII.GetBytes(cab.ToString());
                        red.Write(c, 0, c.Length);
                        red.Write(cuerpo, 0, cuerpo.Length);
                        red.Flush();
                    }
                }
                catch (Exception)
                {
                    // El cliente cortó (p. ej. por timeout): el test lo verifica desde el lado de la DLL.
                }
            }
        }

        private static RequestRecibida Leer(NetworkStream red)
        {
            RequestRecibida req = new RequestRecibida();
            MemoryStream buf = new MemoryStream();
            int finCabeceras = -1;
            byte[] tmp = new byte[8192];
            while (finCabeceras < 0)
            {
                int n = red.Read(tmp, 0, tmp.Length);
                if (n <= 0) break;
                buf.Write(tmp, 0, n);
                finCabeceras = Encoding.ASCII.GetString(buf.ToArray()).IndexOf("\r\n\r\n", StringComparison.Ordinal);
            }
            byte[] todo = buf.ToArray();
            if (finCabeceras < 0) return req;
            string[] lineas = Encoding.ASCII.GetString(todo, 0, finCabeceras).Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string[] primera = lineas[0].Split(' ');
            req.Metodo = primera[0];
            req.Ruta = primera.Length > 1 ? primera[1] : "";
            for (int i = 1; i < lineas.Length; i++)
            {
                int dos = lineas[i].IndexOf(':');
                if (dos > 0) req.Cabeceras[lineas[i].Substring(0, dos).Trim()] = lineas[i].Substring(dos + 1).Trim();
            }
            int largo = 0;
            if (req.Cabeceras.ContainsKey("Content-Length")) int.TryParse(req.Cabeceras["Content-Length"], out largo);
            int inicio = finCabeceras + 4;
            while (todo.Length - inicio < largo)
            {
                int n = red.Read(tmp, 0, tmp.Length);
                if (n <= 0) break;
                buf.Write(tmp, 0, n);
                todo = buf.ToArray();
            }
            req.Cuerpo = Encoding.UTF8.GetString(todo, inicio, Math.Min(largo, todo.Length - inicio));
            return req;
        }

        public void Dispose()
        {
            activo = false;
            try { oyente.Stop(); } catch (Exception) { }
        }
    }
}
