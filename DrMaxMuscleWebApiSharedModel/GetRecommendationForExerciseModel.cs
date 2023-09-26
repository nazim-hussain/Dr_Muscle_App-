using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class GetRecommendationForExerciseModel : BaseModel
    {
        public string Username { get; set; }
        public long ExerciseId { get; set; }
    }
}
