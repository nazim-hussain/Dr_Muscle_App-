using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class SetModel : BaseModel
    {
        public decimal WeightInKg { get; set; }
        public int Reps { get; set; }
    }
}
