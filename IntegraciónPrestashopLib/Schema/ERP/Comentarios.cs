using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class Comentarios
    {
        public List<Comentario> Mensajes { get; internal set; }

        public Comentarios()
        {
               
        }

        public Comentarios(List<Comentario> mensajes)
        {
            this.Mensajes = mensajes;
        }
        public int GetSize()
        {
            return Mensajes == null ? 0 : Mensajes.Count;
        }

        public Comentario GetMensaje(int index)
        {
            return Mensajes[index];
        }
    }
}
