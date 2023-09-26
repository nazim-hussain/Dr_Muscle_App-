using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class HistoryModel : BaseModel
    {
        public DateTime WorkoutDate { get; set; }
        public List<HistoryExerciseModel> Exercises { get; set; }
    }
}
