using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS.Responses
{
    internal class CartRulesResponse
    {
        public int id_cart_rule { get; set; }
        public String reduction_percent { get; set; }
        public String reduction_amount { get; set; }
        public String TipoRegla { get; set; }
        public String free_shipping { get; set; }
        public String id_item { get; set; }

        public Decimal getAmountToDiscount()
        {
            Decimal amount;
            if (Decimal.TryParse(reduction_amount.Replace(".", ","), out amount))
                return amount;
            else
                return 0;
        }
        public Decimal getPercentToDiscount()
        {
            Decimal percent;
            if (Decimal.TryParse(reduction_percent.Replace(".", ","), out percent))
                return percent;
            else
                return 0;
        }

        public Boolean isPercent()
        {
            return getPercentToDiscount() != 0;
        }
    }
}
