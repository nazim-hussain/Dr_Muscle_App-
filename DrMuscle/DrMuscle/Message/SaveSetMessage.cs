using System;
using DrMuscle.Layout;

namespace DrMuscle.Message
{
    public class SaveSetMessage
    {
        public SaveSetMessage()
        {
        }
        public WorkoutLogSerieModelRef model { get; set; }
        public bool IsFinishExercise { get; set; }
    }
}
