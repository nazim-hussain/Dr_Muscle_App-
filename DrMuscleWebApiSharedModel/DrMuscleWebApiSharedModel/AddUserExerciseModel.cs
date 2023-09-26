using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class AddUserExerciseModel : BaseModel
    {
        public string Username { get; set; }
        public string ExerciseName { get; set; }
        public long BodyPartId { get; set; }
        public bool IsEasy { get; set; }
        public bool IsBodyweight { get; set; }
        public bool IsMedium { get; set; }
        public bool IsUnilateral { get; set; }
        public bool IsTimeBased { get; set; }
        public bool IsFlexibility { get; set; }
        public bool IsWeighted { get; set; }
        public bool IsOneHanded { get; set; } = false;
        public bool  IsAssisted
        {
            get;
            set;
        }
    }
}
