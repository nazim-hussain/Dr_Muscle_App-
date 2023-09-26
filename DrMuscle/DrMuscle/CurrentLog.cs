using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;
using DrMuscle.Helpers;
using DrMuscle.Layout;

namespace DrMuscle
{
    public class CurrentLog
    {
        private static CurrentLog _instance;
        public WorkoutLogSerieModel ExerciseLog { get; set; }
        public ExerciceModel RunningExercise { get; set; }
        public List<GroupChatModel> GroupChats { get; set; }
        public List<ChatModel> SupportChats { get; set; }
        public MultiUnityWeight Weight1 { get; set; }
        public MultiUnityWeight Weight2 { get; set; }
        public MultiUnityWeight Weight3 { get; set; }
        public string Reps1 { get; set; }
        public string Reps2 { get; set; }
        public string Reps3 { get; set; }
        public string LastSetWas { get; set; }
        public int BestReps { get; set; }
        public int RIR { get; set; }
        public bool IsNewExercise { get; set; }
        public bool IsFromExercise { get; set; }
        public bool WalkThroughCustomTipsPopup { get; set; }
        public bool IsFromExperienceLifter { get; set; }
        public bool IsHeightPopup { get; set; }
        public Dictionary<long, RecommendationModel> RecommendationsByExercise { get; set; }
        public Dictionary<long, List<ExerciceModel>> WorkoutsByExercise { get; set; }
        public Dictionary<long, List<OneRMModel>> Exercise1RM { get; set; }
        
        public WorkoutTemplateModel CurrentWorkoutTemplate { get; set; }
        public WorkoutTemplateModel WorkoutTemplateSettings { get; set; }
        public WorkoutTemplateGroupModel CurrentWorkoutTemplateGroup { get; set; }
        public ExerciceModel WorkoutTemplateCurrentExercise { get; set; }
        public ExerciceModel CurrentExercise { get; set; }
        public ObservableCollection<WorkoutTemplateGroupModel> workoutOrderItems { get; set; }
        public ExerciceModel VideoExercise { get; set; }
        public List<WorkoutLogSerieModel> LastSerieModelList { get; set; }
        public string ChannelUrl { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverName { get; set; }
        public string AiDescription { get; set; }
        public string AnalysisGPTText { get; set; }
        public bool LoadingGPTText { get; set; }
        public string PoemGPTText { get; set; }
        public string PoemGPTTitle { get; set; }
        public long RoomId { get; set; }
        public bool IsSendOne { get; set; }
        public bool IsAddingExerciseLocally { get; set; }
        public bool IsWalkthrough { get; set; }
        public bool IsMinRepsWarning { get; set; }
        public bool IsMaxRepsWarning { get; set; }
        public bool IsAddedExercises { get; set; }
        public bool IsAddingExerciseFromWorkout { get; set; }
        public bool IsFreePlanPopup { get; set; }
        public bool IsRecommendationLeft { get; set; }
        public bool IsExerciseDeleted { get; set; }
        public bool? IsMonthlyUser { get; set; }
        public bool getUserWorkoutLogLoaded { get; set; }
        public bool IsWelcomePopup { get; set; }
        public bool WorkoutStarted { get; set; }
        public bool IsBodyweight { get; set; }
        public bool IsBodyPartUpdated { get; set; }
        public bool IsFavouriteUpdated { get; set; }
        public bool IsAddedNewExercise { get; set; }
        public bool AutoEnableIncrements { get; set; }
        public bool IsFromEndExercise { get; set; }
        public bool IsDemoRunningStep1 { get; set; }
        public bool IsDemoRunningStep2 { get; set; }
        public bool IsDemoPopingOut { get; set; }
        public bool IsSettingsVisited { get; set; }
        public bool IsRestarted { get; set; }
        public bool IsRecoveredWorkout { get; set; }
        public bool IsUnFinishedWorkout { get; set; }
        public bool IsMobilityStarted { get; set; }
        public bool IsMobilityFinished { get; set; }
        public bool IsFinishedWorkoutWithExercise { get; set; }
        public bool IsWorkoutedOut { get; set; }
        public bool IsReconfigration { get; set; }
        public bool IsWelcomeMessage { get; set; }
        public DateTime? PastWorkoutDate { get; set; }
        public bool ShowCurentExercise { get; set; }
        public bool IsMovingOnBording { get; set; }
        public bool ShowTimerOptions { get; set; }
        public bool ShowEditWorkouts { get; set; }
        public bool IsRepsOutsidePopup { get; set; }
        public bool IsAskedLightSession { get; set; }
        public bool IsAskedDeload { get; set; }
        public bool IsAskedAddExtraSet { get; set; }
        public bool IsAskedChallenge { get; set; }
        public bool IsRest { get; set; }
        public Type EndExerciseActivityPage { get; set; }
        public Type ExerciseSettingsPage { get; set; }
        public decimal CurrentWeightLb { get; set; }
        public decimal CurrentWeightKg { get; set; }
        
        public string WeightChangedPercentage { get; set; }
        public string CoachTipsText { get; set; }
        
        public decimal CurrentWeight { get; set; }
        public Dictionary<long, ObservableCollection<WorkoutLogSerieModelEx>> WorkoutLogSeriesByExercise { get; set; }
        public Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>> WorkoutLogSeriesByExerciseRef { get; set; }
        public int GetRecommendationRestTime(long exerciseId, bool inSuperset = false, bool isNormalSet = false, bool isPyramid = false, bool isFlexibility = false, bool isReversePyramid = false, bool isMaxSet = false, int reps=0) 
		{
            if (CurrentLog.Instance.IsRepsOutsidePopup && (isReversePyramid || isPyramid))
            {
                isNormalSet = true;
                isReversePyramid = false;
                isPyramid = false;
            }
            var addExtra = isMaxSet ? 20 : 0;
            if (exerciseId == 16508)
                return 120 + addExtra;
            if (isFlexibility)
                return 30 + addExtra;
            if (isPyramid)
                return 180 + addExtra;
            if (isReversePyramid)
                return 120 + addExtra;
                if (!isNormalSet)
            {
                return 25 + addExtra;
            }
            
     //       else if (LocalDBManager.Instance.GetDBSetting("SetStyle").Value == "RestPause")
					//return 25;
            if (!RecommendationsByExercise.ContainsKey(exerciseId))
					return inSuperset ? 20 : 60 + addExtra;
            if (reps == 0)
                reps = RecommendationsByExercise[exerciseId].Reps;
            switch (reps)
			{
				default:
					return inSuperset ? 20 : 60 + addExtra;
				case 1:
				case 2:
				case 3:
				case 4:
					return inSuperset ? 70 : 210 + addExtra;
                case 5:
                case 6:
				case 7:
					return inSuperset ? 60 : 180 + addExtra;
                case 8:
                case 9:
				case 10:
					return inSuperset ? 50 : 150 + addExtra;
				case 11:
				case 12:
                case 13:
                case 14:
                case 15:
                    return inSuperset ? 40 : 120 + addExtra;
				case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    return inSuperset ? 30 : 90 + addExtra;
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                    return inSuperset ? 20 : 60 + addExtra;
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                    return inSuperset ? 20 : 60 + addExtra;
			}
		}		

        private CurrentLog()
        {
            WorkoutStarted = false;
            WorkoutLogSeriesByExercise = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelEx>>();
            RecommendationsByExercise = new Dictionary<long, RecommendationModel>();
            WorkoutsByExercise = new Dictionary<long, List<ExerciceModel>>();
            Exercise1RM = new Dictionary<long, List<OneRMModel>>();
            LastSerieModelList = new List<WorkoutLogSerieModel>();
        }

        public static CurrentLog Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CurrentLog();
                    _instance.WorkoutLogSeriesByExercise = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelEx>>();
                    _instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                    _instance.RecommendationsByExercise = new Dictionary<long, RecommendationModel>();
                }
                return _instance;
            }
        }

        public SwapExerciseContext SwapContext { get; set; }
        public bool ShowWelcomePopUp { get; set; }

        internal void Reset()
        {
            _instance = new DrMuscle.CurrentLog();
        }
    }
}