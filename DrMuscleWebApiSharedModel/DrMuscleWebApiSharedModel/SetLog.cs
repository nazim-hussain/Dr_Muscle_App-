using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class SetLog : BaseModel
    {
        public ExerciceModel Exercice { get; set; }
        public SetModel Set { get; set; }
    }
}
