using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    public class ExerciceModel : BaseModel
    {
        public long Id { get; set; }
        public string Label { get; set; }
        //public int BodyPartId { get; set; }
        //public BodyPartModel BodyPart { get;set; }
    }
}
