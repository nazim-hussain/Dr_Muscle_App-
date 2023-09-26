using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class GetOneRMforExerciseModel : BaseModel
    {
        public string Username { get; set; }
        public string Massunit { get; set; }
        public long ExerciseId { get; set; }
    }
}
