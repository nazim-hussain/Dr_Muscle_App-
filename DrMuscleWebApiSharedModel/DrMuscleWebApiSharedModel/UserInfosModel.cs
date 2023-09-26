using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class UserInfosModel : BaseModel
    {
        public string Email { get; set; }
        public string UId { get; set; }

        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Gender { get; set; }
        public string MassUnit { get; set; }
        public bool? IsNormalSet { get; set; }
        public string Password { get; set; }
        public string ReminderDays { get; set; }
        public string TimeZone { get; set; }
        public TimeSpan? ReminderTime { get; set; }
		public DateTime CreationDate { get; set; }
        public int RepsMinimum { get; set; }
        public int RepsMaximum { get; set; }
        public int ReprangeType { get; set; }
        public bool? IsQuickMode { get; set; }
        public bool IsReminderEmail { get; set; }
        public int ReminderBeforeHours { get; set; }

        public int? WarmupsValue { get; set; }
        public int? Age { get; set; }
        public float? Height { get; set; }
        public DateTime? LastChallengeDate { get; set; }
        public MultiUnityWeight BodyWeight { get; set; }
        public MultiUnityWeight WeightGoal { get; set; }
        public MultiUnityWeight Increments { get; set; }
        public MultiUnityWeight Min { get; set; }
        public MultiUnityWeight Max { get; set; }
        public bool IsVibrate { get; set; }
        public bool IsSound { get; set; }
        public bool IsRepsSound { get; set; }
        public bool IsTimer321 { get; set; }
        public bool IsFullscreen { get; set; }
        public bool IsAutoStart { get; set; }
        public bool IsAutomatchReps { get; set; }
        public int TimeCount { get; set; }
        public bool IsBackOffSet { get; set; }
        public bool IsCardio { get; set; }
        public bool IsReminder { get; set; }
        public int? SetCount { get; set; }
        public bool IsStrength { get; set; }
        public string BodyPartPrioriy { get; set; }
        public string SwappedJson { get; set; }
        public EquipmentModel EquipmentModel { get; set; }
        public bool? IsMobility { get; set; }
        public bool? IsExerciseQuickMode { get; set; }
        public bool Is1By1Side { get; set; } = true;
        public int? MobilityRep { get; set; }

        public int? WeeklyExerciseCount { get; set; }
        public int? DailyExerciseCount { get; set; }

        public int WorkoutDuration { get; set; }
        
        public string MobilityLevel { get; set; }
        public string MainGoal { get; set; }
        public bool IsPyramid { get; set; }
        public string LastWorkoutWas { get; set; }
        public DateTime LastActiveDate { get; set; }

        public bool? IsRecommendedReminder { get; set; }

        public decimal? TargetIntake { get; set; }
        public decimal? KgBarWeight { get; set; }
        public decimal? LbBarWeight { get; set; }
    }

    public class UserModel
    {
        public UserModel()
        {
        }
        public double? LastMonthWeight { get; set; }
        public double? CurrentWeight { get; set; }
        public double? PredictedWeight { get; set; }
        public double? TargetIntake { get; set; }


        public Guid Id { get; set; }
        public string Firstname { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string WeightUnit { get; set; }
    

        public DateTime? LastMealPlanOrderDate { get; set; }
        public double? Height { get; set; }
        public int? Age { get; set; }
        public string Gender { get; set; }

        
        public int? MealCount { get; set; }
        public DateTime LastActiveDate { get; set; }

        public virtual ICollection<DmmMeal> DmmMeal { get; set; }
        public virtual ICollection<MealPlanModel> DmmMealPlan { get; set; }

       
    }

    //public class MealModel
    //{
    //    public Guid Id { get; set; }
    //    public Guid UserId { get; set; }
    //    public string MealInfo { get; set; }
    //    public DateTime? CreatedDate { get; set; }
    //    public int MealCount { get; set; }
    //    public MealModel()
    //    {
    //    }
    //}
}
