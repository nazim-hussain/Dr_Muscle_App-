using System;
using System.Collections.Generic;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;

namespace DrMuscle.Helpers
{
    public class WorkoutHistoryContext
    {
        public WorkoutHistoryContext()
        {
        }

        public List<HistoryModel> Histories = new List<HistoryModel>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static WorkoutHistoryContext FromJson(string json)
        {
            return JsonConvert.DeserializeObject<WorkoutHistoryContext>(json);
        }

        public void SaveContexts()
        {
            LocalDBManager.Instance.SetDBSetting("history_contexts", ToJson());
        }

        public static WorkoutHistoryContext LoadContexts()
        {
            DBSetting dbSwapExerciseContexts = LocalDBManager.Instance.GetDBSetting("history_contexts");

            if (dbSwapExerciseContexts == null)
            {
                WorkoutHistoryContext tmp = new WorkoutHistoryContext();
                tmp.SaveContexts();
                return tmp;
            }

            return WorkoutHistoryContext.FromJson(dbSwapExerciseContexts.Value);
        }
    }



    public class WeightsContext
    {
        public WeightsContext()
        {
        }

        public List<UserWeight> Weights = new List<UserWeight>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static WeightsContext FromJson(string json)
        {
            return JsonConvert.DeserializeObject<WeightsContext>(json);
        }

        public void SaveContexts()
        {
            LocalDBManager.Instance.SetDBSetting("weight_contexts", ToJson());
        }

        public static WeightsContext LoadContexts()
        {
            DBSetting dbSwapExerciseContexts = LocalDBManager.Instance.GetDBSetting("weight_contexts");

            if (dbSwapExerciseContexts == null)
            {
                WeightsContext tmp = new WeightsContext();
                tmp.SaveContexts();
                return tmp;
            }

            return WeightsContext.FromJson(dbSwapExerciseContexts.Value);
        }
    }


    public class WorkoutListContext
    {
        public WorkoutListContext()
        {
        }
        
        public Dictionary<long, List<ExerciceModel>> WorkoutsByExercise = new Dictionary<long, List<ExerciceModel>>();
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static WorkoutListContext FromJson(string json)
        {
            return JsonConvert.DeserializeObject<WorkoutListContext>(json);
        }

        public void SaveContexts()
        {
            LocalDBManager.Instance.SetDBSetting("workoutlist_contexts", ToJson());
        }

        public static WorkoutListContext LoadContexts()
        {
            DBSetting workoutListContexts = LocalDBManager.Instance.GetDBSetting("workoutlist_contexts");

            if (workoutListContexts == null)
            {
                WorkoutListContext tmp = new WorkoutListContext();
                tmp.SaveContexts();
                return tmp;
            }

            return WorkoutListContext.FromJson(workoutListContexts.Value);
        }
    }
}
