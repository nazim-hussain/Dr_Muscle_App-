using System.Collections.Generic;

namespace DrMuscleWebApiSharedModel
{
    public class WorkoutTemplateGroupModel
    {
        public long Id { get; set; }
        public bool IsFeaturedProgram { get; set; }
        public string UserId { get; set; }
        public string Label { get; set; }
        public List<WorkoutTemplateModel> WorkoutTemplates { get; set; }
        public bool IsSystemExercise { get; set; }
        public int RequiredWorkoutToLevelUp { get; set; }
        public int? Level { get; set; }
        public int? RemainingToLevelUp { get; set; }
        public int? NextProgramId { get; set; }
        public long ProgramId { get; set; }

        public override string ToString()
        {
            return $"WorkoutTemplateGroupModel Id : {this.Id} Label : {this.Label}";
        }
    }

    public class AverageImprovement : BaseModel
    {
        public double ImprovePercent { get; set; }
    }
}
