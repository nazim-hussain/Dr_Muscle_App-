using System;
using System.Collections.Generic;

namespace DrMuscleWebApiSharedModel
{
    public class GetUserWorkoutLogAverageResponse
    {
        public List<ExerciceModel> AverageExercises { get; set; }
        public List<ExerciceModel> AllExercises { get; set; }
        public List<OneRMAverage> Averages { get; set; }
        public List<ConsecutiveWeeksModel> ConsecutiveWeeks { get; set; }
        public List<int> Sets { get; set; }
        public List<int> ExerciseCount { get; set; }
        public List<int> WorkoutCount { get; set; }
        public List<DateTime> SetsDate { get; set; }
        public HistoryExerciseModel HistoryExerciseModel { get; set; }
        public GetUserProgramInfoResponseModel GetUserProgramInfoResponseModel { get; set; }
        public int LastMonthWorkoutCount { get; set; }
        public int LastConsecutiveWorkoutDays { get; set; }
        public int LatestVersionCode { get; set; }
        public DateTime? LastWorkoutDate { get; set; }
    }

    public class GetUserWorkoutLogDate
    {
        public List<DateTime> LogDate { get; set; }
    }
}
