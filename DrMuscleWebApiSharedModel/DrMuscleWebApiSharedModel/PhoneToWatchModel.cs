using System;
namespace DrMuscleWebApiSharedModel
{
    public class PhoneToWatchModel
    {
        public PhoneToWatchModel()
        {
        }

        public int Reps { get; set; }
        public string Weight { get; set; }
        public string Label { get; set; }
        public long Id { get; set; }
        public int Seconds { get; set; }
        public int RIR { get; set; }
        public WatchMessageType WatchMessageType { get; set; }
        public Platform SenderPlatform { get; set; }
    }

    public enum WatchMessageType
    {
        SaveSet,
        FinishExercise,
        FinishSide1,
        FinishSet,
        StartTimer,
        EndTimer,
        NewSet,
        NewSetBehind,
        EditSet,
        WeightMore,
        WeightLess,
        RepsMore,
        RepsLess,
        NextExercise,
        FinishSaveWorkout,
        RIR,
        StartWorkout

    }
    public enum Platform
    {
        Phone,
        Watch
    }
}
