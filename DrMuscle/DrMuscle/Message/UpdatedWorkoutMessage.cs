using System;
namespace DrMuscle.Message
{
    public class UpdatedWorkoutMessage
    {
        public bool OnlyRefresh { get; set; }
        public bool workoutChange { get; set; }
        public UpdatedWorkoutMessage()
        {
        }
    }

    public class RecongrationtMessage
    {
        public RecongrationtMessage()
        {
        }
    }
}
