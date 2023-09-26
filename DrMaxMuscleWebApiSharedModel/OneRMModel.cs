using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class OneRMModel : BaseModel
    {
        public long ExerciseId { get; set; }
        public string UserId { get; set; }
        public DateTime OneRMDate { get; set; }
        public MultiUnityWeight OneRM { get; set; }
    }
}
