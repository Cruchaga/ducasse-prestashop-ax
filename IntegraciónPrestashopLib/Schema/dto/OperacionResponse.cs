using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.dto
{
    internal class OperacionResponse<T>
    {
        public String Descripcion { get; set; }
        public Boolean Exitosa { get; set; }
        public String Error { get; set; }
        public List<T> obj { get; set; }
    }
}
