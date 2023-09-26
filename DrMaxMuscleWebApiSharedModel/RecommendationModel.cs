using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class RecommendationModel : BaseModel
    {
        public int Series { get; set; }
        public int Reps { get; set; }
        public MultiUnityWeight Weight { get; set; }
        public decimal OneRMProgress { get; set; }
    }
}
