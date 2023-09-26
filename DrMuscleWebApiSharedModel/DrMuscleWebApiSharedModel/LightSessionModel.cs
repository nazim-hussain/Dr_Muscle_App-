using System;
namespace DrMuscleWebApiSharedModel
{
    public class LightSessionModel
    {
        public long ExerciseId { get; set; }
        public bool IsLightSession { get; set; }
        public bool IsQuickMode { get; set; }
        public long BodypartId { get; set; }
        public long? WorkoutId { get; set; }
        public DateTime? MaxChallengeDate { get; set; }
    }
}
