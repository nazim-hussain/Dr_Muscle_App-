using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class NewExerciceModel : BaseModel
    {
        public string Label { get; set; }
        public SetModel Set3 { get; set; }
        public SetModel Set2 { get; set; }
        public SetModel Set1 { get; set; }
    }
}
