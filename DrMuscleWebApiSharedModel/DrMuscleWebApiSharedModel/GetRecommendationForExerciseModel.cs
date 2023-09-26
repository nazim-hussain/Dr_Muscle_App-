using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class GetRecommendationForExerciseModel : BaseModel
    {
        public string Username { get; set; }
        public long ExerciseId { get; set; }
        public long? WorkoutId { get; set; }
        public bool? IsQuickMode { get; set; }
        public int? LightSessionDays { get; set; }
        public long? SwapedExId { get; set; }
        public bool IsStrengthPhashe { get; set; }
        public bool IsFreePlan { get; set; }
    }
}
