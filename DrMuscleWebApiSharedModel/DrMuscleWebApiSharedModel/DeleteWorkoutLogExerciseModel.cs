using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class DeleteWorkoutLogExerciseModel : BaseModel
    {
        public long WorkoutLogId { get; set; }
        public long WorkoutLogExerciseId { get; set; }
    }
}
