using IntegracionEnviame.Schema.Response;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntegracionEnviame.Bussiness.Util
{
    public class Parser
    {
        internal static MakeWithdrawalResponse ParseWithdrawalResponseFromJson(string response)
        {
            MakeWithdrawalResponse retiroResponse;
            JObject jo = JObject.Parse(response);
            JToken data = jo.Root.First;
            var root = data.Path;
            if (root.Equals("data"))
            {
                data = jo.SelectToken("data");
            }
            else if (root.Equals("pickup_date"))
            {
                data = jo.SelectToken("");
            }
            retiroResponse = data.ToObject<MakeWithdrawalResponse>();

            return retiroResponse;
        }
        internal static MakeDeliveryResponse ParseDeliveryResponseFromJson(string response)
        {
            MakeDeliveryResponse deliveryResponse;
            JObject jo = JObject.Parse(response);
            JToken data = jo.Root.First;
            var root = data.Path;
            data = jo.SelectToken("data");
            deliveryResponse = data.ToObject<MakeDeliveryResponse>();

            return deliveryResponse;
        }

        internal static MakeManifestResponse[] ParseManifestResponseFromJson(string json)
        {
            MakeManifestResponse[] manifestResponse;
            JObject jo = JObject.Parse(json);
            JToken data = jo.Root.First;
            var root = data.Path;
            data = jo.SelectToken("data");
            manifestResponse = data.ToObject<MakeManifestResponse[]>();

            return manifestResponse;
        }

        internal static GetTrackingResponse ParseTrackingResponseFromJson(string response)
        {
            GetTrackingResponse trackingResponse;
            JObject jo = JObject.Parse(response);
            JToken data = jo.Root.First;
            var root = data.Path;
            data = jo.SelectToken("data");
            trackingResponse = data.ToObject<GetTrackingResponse>();

            return trackingResponse;
        }
    }
}
