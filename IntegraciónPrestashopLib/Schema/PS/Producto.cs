using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class Producto
    {
        [JsonProperty("id")]
        [XmlElement("id")]
        public Int32 ProductId { get; set; }
        //[JsonProperty("active")]
        //[JsonProperty("additional_delivery_times")]
        //[JsonProperty("additional_shipping_cost")]
        //[JsonProperty("advanced_stock_management")]
        //[JsonProperty("available_date")]
        //[JsonProperty("available_for_order")]
        //[JsonProperty("available_later")]
        //[JsonProperty("available_now")]
        //[JsonProperty("cache_default_attribute")]
        //[JsonProperty("cache_has_attachments")]
        //[JsonProperty("cache_is_pack")]
        //[JsonProperty("condition")]
        //[JsonProperty("customizable")]
        //[JsonProperty("date_add")]
        //[JsonProperty("date_upd")]
        //[JsonProperty("delivery_in_stock")]
        //[JsonProperty("delivery_out_stock")]
        [JsonProperty("ean13")]
        [XmlElement("ean13")]
        public String CodeBarEan13 { get; set; }
        //[JsonProperty("ecotax")]
        [JsonProperty("height")]
        [XmlElement("height")]
        public String Alto { get; set; }
        
        [JsonProperty("depth")]
        [XmlElement("depth")]
        public String Profundidad { get; set; }
        
        [JsonProperty("width")]
        [XmlElement("width")]
        public String Ancho { get; set; }
        //[JsonProperty("id_category_default")]
        //[JsonProperty("id_default_combination")]
        //[JsonProperty("id_default_image")]
        [JsonProperty("id_manufacturer")]
        [XmlElement("id_manufacturer")]
        public int ManufacturerId { get; set; }
        //[JsonProperty("id_shop_default")]
        //[JsonProperty("id_supplier")]
        //[JsonProperty("id_tax_rules_group")]
        //[JsonProperty("id_type_redirected")]
        //[JsonProperty("indexed")]
        //[JsonProperty("is_virtual")]
        //[JsonProperty("isbn")]
        //[JsonProperty("link_rewrite")]
        //[JsonProperty("location")]
        //[JsonProperty("low_stock_alert")]
        //[JsonProperty("low_stock_threshold")]
        //[JsonProperty("manufacturer_name")]
        //[JsonProperty("meta_description")]
        //[JsonProperty("meta_keywords")]
        //[JsonProperty("meta_title")]
        //[JsonProperty("minimal_quantity")]
        //[JsonProperty("new")]
        //[JsonProperty("on_sale")]
        //[JsonProperty("online_only")]
        //[JsonProperty("pack_stock_type")]
        //[JsonProperty("position_in_category")]
        [JsonProperty("price")]
        [XmlElement("price")]
        public String Precio { get; set; }
        //[JsonProperty("quantity")]
        //[JsonProperty("quantity_discount")]
        //[JsonProperty("redirect_type")]

        [JsonProperty("reference")]
        [XmlElement("reference")]
        public String Reference { get; set; }
        //[JsonProperty("show_condition")]
        //[JsonProperty("show_price")]
        [JsonProperty("state")]
        [XmlElement("state")]
        public String State { get; set; } = "1";
        //[JsonProperty("supplier_reference")]
        //[JsonProperty("text_fields")]
        //[JsonProperty("type")]
        //[JsonProperty("unit_price_ratio")]
        //[JsonProperty("unity")]
        //[JsonProperty("upc")]
        //[JsonProperty("uploadable_files")]
        //[JsonProperty("visibility")]
        [JsonProperty("weight")]
        [XmlElement("weight")]
        public String Weight { get; set; }
        //[JsonProperty("wholesale_price")]

        [JsonIgnore]
        [XmlElement("link_rewrite")]
        public LangDescriptions UrlAmigable { get; set; }

        [JsonIgnore]
        [XmlElement("name")]
        public LangDescriptions Name { get; set; }

        [JsonIgnore]
        [XmlElement("description")]
        public LangDescriptions Description { get; set; }

        [JsonIgnore]
        [XmlElement("description_short")]
        public LangDescriptions ShortDescription { get; set; }
        
        [XmlIgnore]
        [JsonProperty("link_rewrite")]
        public String UrlAmigableString { get; set; }

        [XmlIgnore]
        [JsonProperty("name")]
        public String NameString { get; set; }

        [XmlIgnore]
        [JsonProperty("description")]
        public String DescriptionString { get; set; }

        [XmlIgnore]
        [JsonProperty("description_short")]
        public String ShortDescriptionString { get; set; }

        public ProductAssociations Associations { get; set; }
        public bool ShouldSerializeProductId()
        {
            return (this.ProductId != 0);
        }
    }       
    
    public class LangDescriptions
    {
        [XmlElement("language")]
        public Lang Languages { get; set; }
    }

    public class Lang
    {
        [XmlAttribute("id")]
        public String LangId { get; set; }
        [XmlText]
        public String Language { get; set; }
    }

}
