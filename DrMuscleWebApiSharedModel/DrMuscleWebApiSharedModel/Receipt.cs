using System;
using Newtonsoft.Json;

namespace DrMuscleWebApiSharedModel
{
    public class Receipt
    {
        [JsonProperty("receipt-data")]
        public string ReceiptData { get; set; }

        public string password { get; set; }
    }
}
