using System;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
namespace DrMuscle.Message
{
    public class FinishSetReceivedFromWatchOS
    {
       public WorkoutLogSerieModelRef model { get; set; }
        public WatchMessageType WatchMessageType { get; set; }
    }
}
