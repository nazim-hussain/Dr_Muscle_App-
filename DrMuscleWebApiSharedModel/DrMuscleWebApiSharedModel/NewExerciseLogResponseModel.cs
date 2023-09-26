using System;
namespace DrMuscleWebApiSharedModel
{
    public class NewExerciseLogResponseModel : BaseModel
    {
        public bool IsNewExercise { get; set; }
        public DateTime? LastLogDate { get; set; }

    }
}
