using System;
using Newtonsoft.Json;

namespace DrMuscleWebApiSharedModel
{
    public class NewUserSubscriptionModel
    {
        [JsonProperty("email_address")]
        public string email_address { get; set; }

        [JsonProperty("emailaddress")]
        public string emailaddress { get; set; }
    }
}
