using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class NewExerciceLogModel : BaseModel
    {
        public int ExerciseId { get; set; }
        public MultiUnityWeight Weight4 { get; set; }
        public MultiUnityWeight Weight3 { get; set; }
        public MultiUnityWeight Weight2 { get; set; }
        public MultiUnityWeight Weight1 { get; set; }
        public string Reps4 { get; set; }
        public string Reps3 { get; set; }
        public string Reps2 { get; set; }
        public string Reps1 { get; set; }
        public string Username { get; set; }
        public int? RIR { get; set; }

    }
}
