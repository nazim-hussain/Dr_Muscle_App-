using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class HistoryModel : BaseModel
    {
        public long Id { get; set; }
        public DateTime WorkoutDate { get; set; }
        public List<HistoryExerciseModel> Exercises { get; set; }
    }
}
