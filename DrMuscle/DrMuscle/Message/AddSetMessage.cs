using System;
using DrMuscle.Layout;

namespace DrMuscle.Message
{
    public class AddSetMessage
    {
        public WorkoutLogSerieModelRef model { get; set; }
        public bool hasFinished { get; set; }
    }
}
