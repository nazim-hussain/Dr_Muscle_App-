using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class GetExerciseRequest : BaseModel
    {
        public long ExerciseId { get; set; }
    }
}
