using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class GetUserWorkoutLogAverageForExerciseRequest
    {
        public long? ExerciseId { get; set; }
        public TimeSpan? PeriodSinceToday { get; set; }
        public string userId { get; set; }
        public DateTime? Date { get; set; }
    }
}
