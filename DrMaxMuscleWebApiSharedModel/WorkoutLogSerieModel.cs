using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class WorkoutLogSerieModel : BaseModel
    {
        public ExerciceModel Exercice { get; set; }
        public string UserId { get; set; }
        public DateTime LogDate { get; set; }
        public int Reps { get; set; }
        public MultiUnityWeight Weight { get; set; }

        public MultiUnityWeight OneRM { get; set; }
    }
}
