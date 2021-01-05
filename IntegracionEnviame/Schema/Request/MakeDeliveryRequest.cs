using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Request
{
    public class MakeDeliveryRequest
    {
        [JsonProperty("shipping_order")]
        public ShippingOrder ShippingOrder { get; set; }

        [JsonProperty("shipping_origin")]
        public ShippingOrigin ShippingOrigin { get; set; }

        [JsonProperty("shipping_destination")]
        public ShippingDetination ShippingDestination { get; set; }

        public Carrier Carrier { get; set; }

        public MakeDeliveryRequest()
        {
            ShippingDestination = new ShippingDetination();
            ShippingOrder = new ShippingOrder();
            ShippingOrigin = new ShippingOrigin();
            Carrier = new Carrier();
        }
    }

    public class Carrier
    {
        [JsonProperty("carrier_code")]
        public String Code { get; set; }

        [JsonProperty("carrier_service")]
        public String Service { get; set; }

        [JsonProperty("tracking_number")]
        public String TrackingNumber { get; set; }

        public Carrier()
        {
            Code = "";
            Service = "";
            TrackingNumber = "";
        }
    }

    public class ShippingDetination
    {
        public Customer Customer { get; set; }
        [JsonProperty("delivery_address")]
        public DeliveryAddress DeliveryAddress { get; set; }

        public ShippingDetination()
        {
            Customer = new Customer();
            DeliveryAddress = new DeliveryAddress();
        }
    }

    public class DeliveryAddress
    {
        [JsonProperty("home_address")]
        public HomeAddress HomeAddress { get; set; }

        public DeliveryAddress()
        {
            HomeAddress = new HomeAddress();
        }
    }

    public class HomeAddress
    {
        [JsonProperty("place")]
        public String Comuna { get; set; }
        
        [JsonProperty("full_address")]
        public String FullAddress { get; set; }
        public String Information { get; set; }

        public HomeAddress()
        {
            Comuna = "";
            FullAddress = "";
            Information = "";
        }
    }

    public class Customer
    {
        public String Name { get; set; }
        public String Email { get; set; }
        public String Phone { get; set; }

        public Customer()
        {
            this.Name = "";
            this.Email = "";
            this.Phone = "";
        }
    }

    public class ShippingOrigin
    {
        [JsonProperty("warehouse_code")]
        public String WarehouseCode { get; set; }

        public ShippingOrigin()
        {
            this.WarehouseCode = "";
        }
    }

    public class ShippingOrder
    {
        [JsonProperty("n_packages")]
        public int Packages { get; set; }

        [JsonProperty("content_description")]
        public String Description { get; set; }

        [JsonProperty("imported_id")]
        public String PrestashopOrderId { get; set; }

        [JsonProperty("order_price")]
        public int TotalPrice { get; set; }

        public String Weight { get; set; }
        public String Volume { get; set; }
        public String Type { get; set; }

        public ShippingOrder()
        {
            Packages = 0;
            Description = "";
            PrestashopOrderId = "";
            TotalPrice = 0;
            Weight = "";
            Volume = "";
            Type = "";
        }
    }
}
