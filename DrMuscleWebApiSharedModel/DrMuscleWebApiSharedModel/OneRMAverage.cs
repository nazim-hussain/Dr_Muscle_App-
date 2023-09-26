using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class OneRMAverage
    {
        public DateTime Date { get; set; }
        public MultiUnityWeight Average { get; set; }
        public int Worksets { get; set; }
        public bool IsAllow { get; set; }
    }
}
