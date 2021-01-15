using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Response
{
    public class MakeManifestResponse
    {
        public String Carrier { get; set; }
        public String Url { get; set; }
        public String Labels { get; set; }
        public String Summary { get; set; }
    }
}
