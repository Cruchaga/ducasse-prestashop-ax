using System;
using System.Text;

namespace IntegracionPrestashopLib.Bussiness.Util
{
    /// <summary>
    /// Acumulador de traza thread-safe. Reemplaza el uso directo de <see cref="StringBuilder"/>,
    /// que NO es seguro cuando varias tareas (Task.Run) escriben en paralelo sobre la misma
    /// instancia (como ocurre al enriquecer una orden).
    /// <para>
    /// <see cref="Log"/> envuelve la escritura a consola + traza en un solo método (antes
    /// estaban dispersos como pares Console.WriteLine + Append).
    /// </para>
    /// </summary>
    internal class Traza
    {
        private readonly StringBuilder sb = new StringBuilder();
        private readonly object candado = new object();

        /// <summary>Agrega texto a la traza (sin salto de línea automático).</summary>
        internal Traza Append(string texto)
        {
            lock (candado)
            {
                sb.Append(texto);
            }
            return this;
        }

        /// <summary>Escribe el mensaje a la consola y a la traza (con salto de línea).</summary>
        internal void Log(string mensaje)
        {
            lock (candado)
            {
                Console.WriteLine(mensaje);
                sb.Append(mensaje).Append(Environment.NewLine);
            }
        }

        public override string ToString()
        {
            lock (candado)
            {
                return sb.ToString();
            }
        }
    }
}
