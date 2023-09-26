using System;
using DrMuscle.Layout;

namespace DrMuscle.Message
{
    public class FinishExerciseMessage
    {
        public FinishExerciseMessage()
        {
        }
        public WorkoutLogSerieModelRef model { get; set; }
    }
}
