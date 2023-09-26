using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class GetUserExerciseResponseModel : BaseModel
    {
        public List<ExerciceModel> Exercises { get; set; }
    }
}
