using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    public class CartRuleResponse
    {
        [JsonProperty("cart_rules")]
        public List<CartRule> CartRules { get; set; }
    }
}
