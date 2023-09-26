using System.Collections.Generic;

namespace DrMuscleWebApiSharedModel
{
    public class GetExercisesLogResponseModel
    {
        public List<WorkoutLogSerieModel> SetLogs { get; set; }
        public List<ExerciceModel> Exercises { get; set; }
    }
}
