using System;
using Newtonsoft.Json;

namespace DrMuscleWebApiSharedModel
{
    public class LifeCycle
    {
        [JsonProperty("lifecycle stage")]
        public string Life_Cycle { get; set; }
    }
}

