using IntegracionPrestashopLib.Schema.Dictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    public class CreateResponse
    {
        public CreateResponse()
        {
            this.ErrorCode = IntegracionErrorCode.NONE;
        }
        internal object Objeto { get; set; }
        public Boolean Creado { get; set; }
        public int ErrorCode { get; set; }
        public String Error { get; set; }
    }
}
