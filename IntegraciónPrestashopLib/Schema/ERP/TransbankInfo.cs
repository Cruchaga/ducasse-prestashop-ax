using System;

namespace IntegracionPrestashopLib.Schema.ERP
{
    public class TransbankInfo
    {
        public TransbankInfo()
        {
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
        }
        public string  Token { get; set; }
        public string Vci { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; }
        public string BuyOrder { get; set; }
        public string SessionId { get; set; }
        public string Card_number { get; set; }
        public string AccountingDate { get; set; }
        public string TransactionDate { get; set; }
        public string AuthorizationCode { get; set; }
        public string PaymentTypeCode { get; set; }
        public int ResponseCode { get; set; }
        public string InstallmentsAmount { get; set; }
        public string InstallmentsNumber { get; set; }
        public string Balance { get; set; }
    }
}