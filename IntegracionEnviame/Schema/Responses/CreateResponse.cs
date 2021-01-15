using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Responses
{
    public class CreateResponse
    {
        public bool Creado { get; set; }
        public int ErrorCode { get; set; }
        public String ErrorDescription { get; set; }

    }
}
