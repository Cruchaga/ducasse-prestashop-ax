using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IntegracionPrestashopLib.Schema.PS
{
    public class ProductToUpdate : Producto
    {

        public String active;
        public String additional_delivery_times;
        public String additional_shipping_cost;
        public String advanced_stock_management;
        public String available_date;
        public String available_for_order;
        public String available_later;
        public String available_now;
        public String cache_default_attribute;
        public String cache_has_attachments;
        public String cache_is_pack;
        public String condition;
        public String customizable;
        public String date_add;
        public String date_upd;
        public String delivery_in_stock;
        public String delivery_out_stock;

        public String ecotax;


        public String id_category_default;
        public String id_default_combination;
        public String id_default_image;
        public String id_shop_default;
        public String id_supplier;
        public String id_tax_rules_group;
        public String id_type_redirected;
        public String indexed;
        public String is_virtual;
        public String isbn;
        public String location;
        public String low_stock_alert;
        public String low_stock_threshold;
        public String meta_description;
        public String meta_keywords;
        public String meta_title;
        public String minimal_quantity;
        [JsonProperty("new")]
        [XmlElement("new")]
        public String isnew;
        public String on_sale;
        public String online_only;
        public String pack_stock_type;
        public String position_in_category;
        public String quantity_discount;
        public String redirect_type;
        public String show_condition;
        public String show_price;
        public String supplier_reference;
        public String text_fields;
        public String type;
        public String unit_price_ratio;
        public String unity;
        public String upc;
        public String uploadable_files;
        public String visibility;
        public String wholesale_price;

    }

    
}
