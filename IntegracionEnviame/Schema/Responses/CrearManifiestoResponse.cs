using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Responses
{
    public class CrearManifiestoResponse: CreateResponse
    {
        internal List<Manifiesto> manifiestos { get; set; }

        public int GetSize()
        {
            return manifiestos == null ? 0 : manifiestos.Count;
        }

        public Manifiesto GetManifiesto(int index)
        {
            return manifiestos == null ? null : manifiestos[index];
        }
    }

    public class Manifiesto
    {
        public String Carrier { get; set; }
        public String SumarioPDF { get; set; }
        public String Etiqueta { get; set; }
    }
}
