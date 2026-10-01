using System.Collections.Generic;
using Prestashop.Net.Interno;

namespace Prestashop.Net
{
    /// <summary>
    /// Detalle del pago de la orden. Los nombres de las propiedades son los de PaymentInfo de
    /// IntegracionPrestashopLib 1.46 (los de Transbank como canónicos), para reutilizar el mapeo de AX.
    /// </summary>
    /// <remarks>
    /// MedioPago: "Webpay Plus", "Banchile Pagos" u otro medio de la tienda. Si TieneDetalle es false
    /// la tienda no registró una transacción aprobada para el pedido y el resto viene vacío.
    /// AuthorizationCode es texto: puede empezar con ceros, no convertir a número.
    /// </remarks>
    public class PagoV2
    {
        public string MedioPago { get; internal set; }
        /// <summary>Nombre técnico del módulo de pago de PrestaShop (webpay, banchilechile…).</summary>
        public string Modulo { get; internal set; }
        public bool TieneDetalle { get; internal set; }

        public string Token { get; internal set; }
        public string Vci { get; internal set; }
        public int Amount { get; internal set; }
        public string Status { get; internal set; }
        public string BuyOrder { get; internal set; }
        public string SessionId { get; internal set; }
        public string Card_number { get; internal set; }
        public string AccountingDate { get; internal set; }
        public string TransactionDate { get; internal set; }
        public string AuthorizationCode { get; internal set; }
        public string PaymentTypeCode { get; internal set; }
        /// <summary>0 = aprobado. En Banchile, -1 si el banco informó un motivo no numérico.</summary>
        public int ResponseCode { get; internal set; }
        public int InstallmentsAmount { get; internal set; }
        public int InstallmentsNumber { get; internal set; }
        public int Balance { get; internal set; }

        // Exclusivos de Banchile Pagos (vacíos o 0 en Webpay).
        public int IdPayment { get; internal set; }
        public int CartId { get; internal set; }
        public int CurrencyId { get; internal set; }
        public string Franchise { get; internal set; }
        public string FranchiseName { get; internal set; }
        public string Bank { get; internal set; }
        public string Receipt { get; internal set; }
        public double Conversion { get; internal set; }
        public string IpAddress { get; internal set; }
        public string PayerEmail { get; internal set; }
        public int RequestId { get; internal set; }
        /// <summary>Identificador del pago en Banchile: sirve para conciliar contra el portal del banco.</summary>
        public string Reference { get; internal set; }

        internal static PagoV2 Desde(Dictionary<string, object> d)
        {
            PagoV2 p = new PagoV2();
            p.MedioPago = Json.Texto(d, "medioPago");
            p.Modulo = Json.Texto(d, "modulo");
            p.TieneDetalle = Json.Booleano(d, "tieneDetalle");
            p.Token = Json.Texto(d, "token");
            p.Vci = Json.Texto(d, "vci");
            p.Amount = Json.Entero(d, "amount");
            p.Status = Json.Texto(d, "status");
            p.BuyOrder = Json.Texto(d, "buyOrder");
            p.SessionId = Json.Texto(d, "sessionId");
            p.Card_number = Json.Texto(d, "cardNumber");
            p.AccountingDate = Json.Texto(d, "accountingDate");
            p.TransactionDate = Json.Texto(d, "transactionDate");
            p.AuthorizationCode = Json.Texto(d, "authorizationCode");
            p.PaymentTypeCode = Json.Texto(d, "paymentTypeCode");
            p.ResponseCode = Json.Entero(d, "responseCode");
            p.InstallmentsAmount = Json.Entero(d, "installmentsAmount");
            p.InstallmentsNumber = Json.Entero(d, "installmentsNumber");
            p.Balance = Json.Entero(d, "balance");
            p.IdPayment = Json.Entero(d, "idPayment");
            p.CartId = Json.Entero(d, "cartId");
            p.CurrencyId = Json.Entero(d, "currencyId");
            p.Franchise = Json.Texto(d, "franchise");
            p.FranchiseName = Json.Texto(d, "franchiseName");
            p.Bank = Json.Texto(d, "bank");
            p.Receipt = Json.Texto(d, "receipt");
            p.Conversion = (double)Json.Decimal(d, "conversion");
            p.IpAddress = Json.Texto(d, "ipAddress");
            p.PayerEmail = Json.Texto(d, "payerEmail");
            p.RequestId = Json.Entero(d, "requestId");
            p.Reference = Json.Texto(d, "reference");
            return p;
        }
    }
}
