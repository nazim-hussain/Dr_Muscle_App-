using System;
using DrMuscle.Layout;

namespace DrMuscle.Message
{
    public class CellUpdateMessage
    {
        public WorkoutLogSerieModelRef model { get; set; }
        public CellUpdateMessage()
        {

        }
    }
}
