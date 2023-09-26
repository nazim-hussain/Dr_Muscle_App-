using System;
using System.Collections.Generic;

namespace DrMuscleWebApiSharedModel
{
    public class ProgramExerciseModel : BaseModel
    {
        public string ProgramName { get; set; }
        public int? RequiredToLevelUp { get; set; }
        public int RequiredWorkoutToLevelUp { get; set; }
        public string PinCode { get; set; }
        public int? RemainingToLevelUp { get; set; }
        public int? NextProgramId { get; set; }
        public int? Timer { get; set; }
        public List<WorkoutsModel> workoutList { get; set; }
        public List<ExerciceModel> UserExercises { get; set; }
    }

    public class WorkoutsModel
    {
        public string WorkoutName { get; set; }
        public List<ExerciceModel> exercices { get; set; }
    }
}
