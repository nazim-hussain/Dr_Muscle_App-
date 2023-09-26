using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class WorkoutTemplateModel
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Label { get; set; }
        public List<ExerciceModel> Exercises { get; set; }
        public bool IsSystemExercise { get; set; }
        public WorkoutTemplateSettingsModel WorkoutSettingsModel { get; set; }
        public override string ToString()
        {
            return $"WorkoutTemplateModel Id : {this.Id} Label : {this.Label}";
        }
    }
}
