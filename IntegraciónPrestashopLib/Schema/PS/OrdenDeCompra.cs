using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class OrdenDeCompra
    {
        public int Id { get; set; }

        [XmlElement("date_add")]
        [JsonProperty("date_add")]
        public DateTime DateAdd { get; set; }

        public String Payment { get; set; }         //Metodo de pago


        [XmlElement("id_customer")]
        [JsonProperty("id_customer")]
        public String CustomerId { get; set; }

        [XmlElement("current_state")]
        [JsonProperty("current_state")]
        public String CurrentState { get; set; }

        [XmlElement("id_address_delivery")]
        [JsonProperty("id_address_delivery")]
        public String AddressDeliveryId { get; set; }

        [XmlElement("id_address_invoice")]
        [JsonProperty("id_address_invoice")]
        public String InvoiceDeliveryId { get; set; }

        [XmlElement("id_currency")]
        [JsonProperty("id_currency")]
        public String CurrencyId { get; set; }

        [XmlElement("total_paid")]
        [JsonProperty("total_paid")]
        public String TotalPaid { get; set; }

        [XmlElement("total_paid_tax_excl")]
        [JsonProperty("total_paid_tax_excl")]
        public String TotalNeto { get; set; }

        [XmlElement("total_paid_tax_incl")]
        [JsonProperty("total_paid_tax_incl")]
        public String TotalFinal { get; set; }

        [XmlElement("total_discounts")]
        [JsonProperty("total_discounts")]
        public Decimal TotalDiscounts { get; set; }

        [XmlElement("associations")]
        [JsonProperty("associations")]
        public ProductosAsociados Asociados { get; set; }

        public String Json { get; set; }

    }
}
