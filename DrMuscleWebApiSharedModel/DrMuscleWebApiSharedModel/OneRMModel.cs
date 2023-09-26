using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class OneRMModel : BaseModel
    {
        public long ExerciseId { get; set; }
        public string UserId { get; set; }
        public DateTime OneRMDate { get; set; }
        public bool IsAllowDelete { get; set; }
        public MultiUnityWeight OneRM { get; set; }
        public DateTime? LastLogDate { get; set; }
        public int Reps { get; set; }
        public MultiUnityWeight Weight { get; set; }
    }

    public class OneRMModel2 : OneRMModel
    {
        public long BodypartId { get; set; }
    }
}
