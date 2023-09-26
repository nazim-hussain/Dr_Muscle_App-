using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class BooleanModel : BaseModel
    {
        public bool Result { get; set; }
        public bool IsFreePlan { get; set; }
        public bool IsTraining { get; set; }
        public bool IsMealPlan { get; set; }
    }

    public class LongModel : BaseModel
    {
        public long Result { get; set; }
    }
}
