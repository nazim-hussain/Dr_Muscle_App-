using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class GetUserProgramInfoResponseModel
    {
        public WorkoutTemplateGroupModel RecommendedProgram { get; set; }
        public WorkoutTemplateModel NextWorkoutTemplate { get; set; }
    }
}
