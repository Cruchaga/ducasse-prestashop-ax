using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class ProductAssociations
    { 
        public List<AssociatedCategory> categories { get; set; }
        public List<ProductBundle> product_bundle { get; set; }

    }
}
