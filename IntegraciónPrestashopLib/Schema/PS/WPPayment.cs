using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class WPPayment
    {
        public String vci { get; set; }
        public int amount { get; set; }
        public String status { get; set; }
        public String buyOrder { get; set; }
        public String sessionId { get; set; }
        public String accountingDate { get; set; }
        public String transactionDate { get; set; }
        public String authorizationCode { get; set; }
        public String paymentTypeCode { get; set; }
        public TbkCardDetail cardDetail { get; set; }
        public int responseCode { get; set; }
        public Object installmentsAmount { get; set; }
        public Object installmentsNumber { get; set; }
        public Object balance { get; set; }
    }
}
