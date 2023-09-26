using System;
using DrMuscle.Layout;

namespace DrMuscle.Message
{
    public class OneRMChangedMessage
    {
        public WorkoutLogSerieModelRef model { get; set; }
        public decimal Weight { get; set; }
        public int Reps { get; set; }
        public OneRMChangedMessage()
        {
        }
    }
}
