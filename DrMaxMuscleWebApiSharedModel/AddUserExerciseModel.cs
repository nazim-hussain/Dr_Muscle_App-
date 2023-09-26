using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class AddUserExerciseModel : BaseModel
    {
        public string Username { get; set; }
        public string ExerciseName { get; set; }
    }
}
