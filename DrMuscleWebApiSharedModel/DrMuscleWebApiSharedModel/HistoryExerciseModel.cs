using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class HistoryExerciseModel : BaseModel
    {
        public ExerciceModel Exercise { get; set; }
        public List<WorkoutLogSerieModel> Sets { get; set; }
        public int Series { get; set; }
        public int Reps { get; set; }
        public int BodypartId { get; set; }
        public MultiUnityWeight TotalWeight { get; set; }
        public MultiUnityWeight BestSerie1RM { get; set; }
        public MultiUnityWeight AverageWeightByRep { get; set; }
        public int TotalWorkoutCompleted { get; set; }
    }
}
