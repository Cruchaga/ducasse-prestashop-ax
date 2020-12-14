using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class Address
    {
        [JsonProperty("id")]
        public String AddressId { get; set; }
        [JsonProperty("address1")]
        public String Direccion1 { get; set; }
        [JsonProperty("address2")]
        public String Direccion2 { get; set; }

        [JsonProperty("alias")]
        public String Alias { get; set; }

        [JsonProperty("city")]
        public String Ciudad { get; set; }

        public String Company { get; set; }
        [JsonProperty("date_add")]
        public String DateAdd { get; set; }
        [JsonProperty("date_upd")]
        public String DateUpd { get; set; }
        public String Deleted { get; set; }
        public String Dni { get; set; }
        public String Firstname { get; set; }
        [JsonProperty("id_country")]
        public String CountryId { get; set; }
        [JsonProperty("id_customer")]
        public String CustomerId { get; set; }
        //[JsonProperty("id_manufacturer")]
        [JsonProperty("id_state")]
        public String StateId { get; set; }
        //[JsonProperty("id_supplier")]
        //[JsonProperty("id_warehouse")]
        public String Lastname { get; set; }
        public String Other { get; set; }
        public String Phone { get; set; }
        //[JsonProperty("phone_mobile")]
        //[JsonProperty("postcode")]
        [JsonProperty("vat_number")]
        public String RutEmpresa { get; set; }

    }
}
