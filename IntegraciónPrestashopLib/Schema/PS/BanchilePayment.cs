using Newtonsoft.Json;

namespace IntegracionPrestashopLib.Schema.PS
{
    /// <summary>
    /// Respuesta del PHP custom WS/banchileinfo.php (tabla ps_payment_banchilechile).
    /// Nota: id_order en la tabla del módulo corresponde al id del carro (cart_id).
    /// </summary>
    public class BanchilePayment
    {
        [JsonProperty("id_payment")]
        public int IdPayment { get; set; }

        [JsonProperty("id_order")]
        public int IdOrder { get; set; }

        [JsonProperty("id_currency")]
        public int IdCurrency { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("reason_description")]
        public string ReasonDescription { get; set; }

        [JsonProperty("franchise")]
        public string Franchise { get; set; }

        [JsonProperty("franchise_name")]
        public string FranchiseName { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("authcode")]
        public string AuthCode { get; set; }

        [JsonProperty("receipt")]
        public string Receipt { get; set; }

        [JsonProperty("conversion")]
        public double Conversion { get; set; }

        [JsonProperty("ip_address")]
        public string IpAddress { get; set; }

        [JsonProperty("payer_email")]
        public string PayerEmail { get; set; }

        [JsonProperty("id_request")]
        public int IdRequest { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("installments")]
        public string Installments { get; set; }

        [JsonProperty("card_last_digits")]
        public string CardLastDigits { get; set; }
    }
}
