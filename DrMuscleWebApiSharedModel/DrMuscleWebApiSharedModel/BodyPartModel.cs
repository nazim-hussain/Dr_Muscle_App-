using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class BodyPartModel : BaseModel
    {
        public int Id { get; set; }
        public string Label { get; set; }

        public List<ExerciceModel> Exercices { get; set; } = new List<ExerciceModel>();
        public override string ToString()
        {
            return $"BodyPartModel Id : {this.Id} Label : {this.Label}";
        }
    }

}
