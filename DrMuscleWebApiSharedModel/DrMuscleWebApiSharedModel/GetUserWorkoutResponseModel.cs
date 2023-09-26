using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class GetUserWorkoutTemplateResponseModel : BaseModel
    {
        public List<WorkoutTemplateModel> Workouts { get; set; }
    }
}
