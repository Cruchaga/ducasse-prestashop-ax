using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class CartRule
    {
        private Int64 Id { get; set; }
        private String IdCustomer { get; set; }
        private String DateFrom { get; set; }
        private String DateTo { get; set; }
        private String Description { get; set; }
        private String Quantity { get; set; }
        private String QuantityPerUser { get; set; }
        private String Priority { get; set; }
        private String PartialUse { get; set; }
        private String Code { get; set; }
        private String MinimumAmount { get; set; }
        private String MinimumAmountTax { get; set; }
        private String MinimumAmountCurrency { get; set; }
        private String MinimumAmountShipping { get; set; }
        private String CountryRestriction { get; set; }
        private String CarrierRestriction { get; set; }
        private String GroupRestriction { get; set; }
        private String CartRuleRestriction { get; set; }
        private String ProductRestriction { get; set; }
        private String ShopRestriction { get; set; }
        private String FreeShipping { get; set; }
        private String ReductionPercent { get; set; }
        private String ReductionAmount { get; set; }
        private String ReductionTax { get; set; }
        private String ReductionCurrency { get; set; }
        private String ReductionProduct { get; set; }
        private String ReductionExcludeSpecial { get; set; }
        private String GiftProduct { get; set; }
        private String GiftProductAttribute { get; set; }
        private String Highlight { get; set; }
        private String Active { get; set; }
        private String DateAdd { get; set; }
        private String DateUpd { get; set; }
        private String Name { get; set; }
    }
}
