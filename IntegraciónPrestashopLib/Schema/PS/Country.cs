using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    internal class Country
    {
        [JsonProperty("id")]
        public int CountryId{ get; set; }
        //[JsonProperty("active")]
        //[JsonProperty("call_prefix")]
        //[JsonProperty("contains_states")]
        //[JsonProperty("display_tax_label")]
        //[JsonProperty("id_currency")]
        //[JsonProperty("id_zone")]
        //[JsonProperty("iso_code")]
        [JsonProperty("name")]
        public String Nombre { get; set; }
        //[JsonProperty("need_identification_number")]
        //[JsonProperty("need_zip_code")]
        //[JsonProperty("zip_code_format")]
    }
}
