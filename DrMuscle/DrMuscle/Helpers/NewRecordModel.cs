using System;
using System.Collections.Generic;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;

namespace DrMuscle.Helpers
{
    public class NewRecordModel
    {
        public NewRecordModel()
        {
        }

        public string ExerciseName { get; set; }
        public string ExerciseId { get; set; }
        public string ExercisePercentage { get; set; }
        public decimal ExercisePercentageNumber { get; set; }
        
        public bool IsMobility { get; set; }
        public MultiUnityWeight Prev1RM { get; set; }
        public MultiUnityWeight New1RM { get; set; }
    }

    public class NewRecordModelContext
    {
        public NewRecordModelContext()
        {
        }

        public List<NewRecordModel> NewRecordList = new List<NewRecordModel>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static NewRecordModelContext FromJson(string json)
        {
            return JsonConvert.DeserializeObject<NewRecordModelContext>(json);
        }

        public void SaveContexts()
        {
            LocalDBManager.Instance.SetDBSetting("newrecord_contexts", ToJson());
        }

        public static NewRecordModelContext LoadContexts()
        {
            DBSetting dbSwapExerciseContexts = LocalDBManager.Instance.GetDBSetting("newrecord_contexts");

            if (dbSwapExerciseContexts == null)
            {
                NewRecordModelContext tmp = new NewRecordModelContext();
                tmp.SaveContexts();
                return tmp;
            }

            return NewRecordModelContext.FromJson(dbSwapExerciseContexts.Value);
        }
    }

}
