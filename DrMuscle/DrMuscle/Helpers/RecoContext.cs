
using System;
using System.Collections.Generic;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;

namespace DrMuscle.Helpers
{
    public class RecoContext
    {


        public static void SaveContexts(string key, RecommendationModel reco)
        {
            string recoJson = JsonConvert.SerializeObject(reco);
            LocalDBManager.Instance.SetDBReco(key, recoJson);
        }

        public static RecommendationModel GetReco(string key)
        {
            if (LocalDBManager.Instance.GetDBReco(key) == null)
                return null;
            var json = LocalDBManager.Instance.GetDBReco(key).Value;
            return JsonConvert.DeserializeObject<RecommendationModel>(json);

        }
    }
}