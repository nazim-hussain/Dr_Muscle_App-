using System;
using DrMuscle.Enums;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;

namespace DrMuscle.Message
{
    public class SendWatchMessage
    {
        public WatchMessageType WatchMessageType
        {
            get; set;
        }
        public WorkoutLogSerieModelRef SetModel { get; set; }
        public int Seconds { get; set; }
        public string Label { get; set; }
        public SendWatchMessage()
        {
        }
    }
}
