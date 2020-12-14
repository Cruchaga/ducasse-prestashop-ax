using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class Customer
    {
        [JsonProperty("id")]
        public int CustomerId { get; set; }

        public String GetCustomerStringId()
        {
            return CustomerId.ToString();
        }
        [JsonProperty("company")]
        public String Company { get; set; }
        [JsonProperty("firstname")]
        public String Nombre { get; set; }
        [JsonProperty("lastname")]
        public String Apellido { get; set; }
        [JsonProperty("email")]
        public String Email { get; set; }
    }
}
