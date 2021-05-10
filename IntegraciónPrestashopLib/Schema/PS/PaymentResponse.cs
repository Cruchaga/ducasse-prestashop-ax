using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class PaymentResponse
    {
        public WPPayment transbank_response { get; set; }
        public String token { get; set; }

    }
}
