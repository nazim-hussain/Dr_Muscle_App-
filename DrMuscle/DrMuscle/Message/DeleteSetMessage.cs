using System;
using DrMuscle.Layout;

namespace DrMuscle.Message
{
    public class DeleteSetMessage
    {
        public WorkoutLogSerieModelRef model { get; set; }
        public bool isPermenantDelete { get; set; }
    }
}
