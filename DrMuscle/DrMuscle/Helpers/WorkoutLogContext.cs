using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;

namespace DrMuscle.Helpers
{
    public class WorkoutLogContext
    {
        public WorkoutLogContext()
        {
        }

        public Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>> WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static WorkoutLogContext FromJson(string json)
        {
            return JsonConvert.DeserializeObject<WorkoutLogContext>(json);
        }

        public void SaveContexts()
        {
            LocalDBManager.Instance.SetDBSetting("workoutlog_contexts", ToJson());
        }

        public static WorkoutLogContext LoadContexts()
        {
            DBSetting workoutListContexts = LocalDBManager.Instance.GetDBSetting("workoutlog_contexts");

            if (workoutListContexts == null)
            {
                WorkoutLogContext tmp = new WorkoutLogContext();
                tmp.SaveContexts();
                return tmp;
            }

            return WorkoutLogContext.FromJson(workoutListContexts.Value);
        }

    }
}
