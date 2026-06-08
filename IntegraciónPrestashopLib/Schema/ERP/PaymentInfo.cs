using System;

namespace IntegracionPrestashopLib.Schema.ERP
{
    /// <summary>
    /// Información del pago de una orden, unificada para ambos medios de pago
    /// (Webpay Plus y Banchile Pagos). Los campos con concepto equivalente entre
    /// ambos medios usan el nombre proveniente de Transbank para no duplicar.
    /// El campo MedioPago indica de qué medio provienen los datos.
    /// </summary>
    public class PaymentInfo
    {
        public PaymentInfo()
        {
            this.MedioPago = "";
            // Campos comunes / Transbank
            this.Token = "";
            this.Vci = "";
            this.Status = "";
            this.BuyOrder = "";
            this.SessionId = "";
            this.Card_number = "";
            this.AccountingDate = "";
            this.TransactionDate = "";
            this.AuthorizationCode = "";
            this.PaymentTypeCode = "";
            this.InstallmentsAmount = "";
            this.InstallmentsNumber = "";
            this.Balance = "";
            // Campos propios de Banco de Chile
            this.Franchise = "";
            this.FranchiseName = "";
            this.Bank = "";
            this.Receipt = "";
            this.IpAddress = "";
            this.PayerEmail = "";
            this.Reference = "";
        }

        /// <summary>Medio de pago: "Webpay Plus" o "Banchile Pagos".</summary>
        public string MedioPago { get; set; }

        // ---- Campos comunes / Transbank (nombres preferidos) ----
        public string Token { get; set; }
        public string Vci { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }                 // BCH: reason_description ("Aprobada")
        public string BuyOrder { get; set; }
        public string SessionId { get; set; }
        public string Card_number { get; set; }            // BCH: card_last_digits
        public string AccountingDate { get; set; }
        public string TransactionDate { get; set; }        // BCH: date
        public string AuthorizationCode { get; set; }      // BCH: authcode
        public string PaymentTypeCode { get; set; }
        public int ResponseCode { get; set; }              // BCH: reason ("00" -> 0)
        public string InstallmentsAmount { get; set; }
        public string InstallmentsNumber { get; set; }     // BCH: installments
        public string Balance { get; set; }

        // ---- Campos propios de Banco de Chile ----
        public int IdPayment { get; set; }
        public int CartId { get; set; }
        public int CurrencyId { get; set; }
        public string Franchise { get; set; }
        public string FranchiseName { get; set; }
        public string Bank { get; set; }
        public string Receipt { get; set; }
        public double Conversion { get; set; }
        public string IpAddress { get; set; }
        public string PayerEmail { get; set; }
        public int RequestId { get; set; }
        public string Reference { get; set; }
    }
}
