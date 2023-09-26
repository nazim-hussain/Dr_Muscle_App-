using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugin.Settings;
using Xamarin.Essentials;

namespace DrMuscle.Helpers
{
    public class SwapExerciseContext
    {
        public long WorkoutId { get; set; }
        public long? SourceExerciseId { get; set; }
        public long? SourceBodyPartId { get; set; }
        public long? TargetExerciseId { get; set; }
        public long? BodyPartId { get; set; }

        public string Label { get; set; }
        public bool IsSystemExercise { get; set; }
        public bool IsSwapTarget { get; set; }
        public bool IsFinished { get; set; }

        public bool IsUnilateral { get; set; }
        public bool IsTimeBased { get; set; }
        public bool IsPlate { get; set; }

        public bool IsEasy { get; set; }
        public bool IsBodyweight { get; set; }
        public string VideoUrl { get; set; }
        public bool IsNextExercise { get; set; }
        public bool IsFlexibility { get; set; }
        public bool IsWeighted { get; set; }
        public bool IsOneHanded { get; set; }
        public bool IsMobility { get; set; }

        public bool IsAssisted { get; set; }
    }

    public class SwapExerciseContextList
    {
        public List<SwapExerciseContext> Swaps = new List<SwapExerciseContext>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static SwapExerciseContextList FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SwapExerciseContextList>(json);
        }

        public void SaveContexts()
        {
            //if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("email")?.Value))
            //    CrossSettings.Current.AddOrUpdateValue(LocalDBManager.Instance.GetDBSetting("email").Value, ToJson());
            LocalDBManager.Instance.SetDBSetting("swap_exericse_contexts", ToJson());
        }

        public static SwapExerciseContextList LoadContexts()
        {
            DBSetting dbSwapExerciseContexts = LocalDBManager.Instance.GetDBSetting("swap_exericse_contexts");
            
            if (dbSwapExerciseContexts == null)
            {
                //if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("email")?.Value))
                //{
                //    return SwapExerciseContextList.FromJson(CrossSettings.Current.GetValueOrDefault(LocalDBManager.Instance.GetDBSetting("email").Value, ""));
                //}
                SwapExerciseContextList tmp = new SwapExerciseContextList();
                tmp.SaveContexts();
                return tmp;
            }

            return SwapExerciseContextList.FromJson(dbSwapExerciseContexts.Value);
        }
    }

    
}
