using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Schema.Response
{
    internal class MakeWithdrawalResponse
    {
        public String Identifier { get; set; }
        public Company Company { get; set; }
        public Address Address { get; set; }
        public Warehouse Warehouse { get; set; }
        public Contact Contact { get; set; }

        [JsonProperty("qty")]
        public String PackagesQty { get; set; }

        //weight null

        public Carrier Carrier { get; set; }
        public Response.Type Type { get; set; }
        public Status MyProperty { get; set; }
    }

    public class Carrier
    {
        public String Name { get; set; }
        public String  Code { get; set; }
        public String Country { get; set; }
    }

    public class Contact
    {
        public String Name { get; set; }
        public String Phone { get; set; }
    }

    public class Warehouse
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Code { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }

        [JsonProperty("place")]
        public String Comuna { get; set; }

        [JsonProperty("full_address")]
        public String FullAddress { get; set; }
    }

    public class Company
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public String Code { get; set; }
    }

    public class Type
    {
        public String Code { get; set; }
        public String Name { get; set; }
    }
}
