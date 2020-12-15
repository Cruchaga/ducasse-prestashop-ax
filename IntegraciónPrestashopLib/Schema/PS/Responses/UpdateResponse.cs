using IntegracionPrestashopLib.Schema.Dictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    public class UpdateResponse
    {
        public UpdateResponse()
        {
            this.ErrorCode = IntegracionErrorCode.NONE;
        }

        public Boolean Actualizado { get; set; }
        public int ErrorCode { get; set; }
        public String Error { get; set; }
    }
}
