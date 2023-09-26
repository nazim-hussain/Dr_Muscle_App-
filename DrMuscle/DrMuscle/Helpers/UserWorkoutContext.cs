using System;
using System.Collections.Generic;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;

namespace DrMuscle.Helpers
{
    public class UserWorkoutContext
    {
        public UserWorkoutContext()
        {
        }
        public GetUserWorkoutLogAverageResponse workouts = new GetUserWorkoutLogAverageResponse();

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static UserWorkoutContext FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UserWorkoutContext>(json);
        }

        public void SaveContexts()
        {
            LocalDBManager.Instance.SetDBSetting("user_workout_contexts", ToJson());
        }

        public static UserWorkoutContext LoadContexts()
        {
            DBSetting dbUserWorkoutContexts = LocalDBManager.Instance.GetDBSetting("user_workout_contexts");

            if (dbUserWorkoutContexts == null)
            {
                UserWorkoutContext tmp = new UserWorkoutContext();
                tmp.SaveContexts();
                return tmp;
            }

            return UserWorkoutContext.FromJson(dbUserWorkoutContexts.Value);
        }               
    }
}
