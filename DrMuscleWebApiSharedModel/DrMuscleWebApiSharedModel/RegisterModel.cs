using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class RegisterModel : BaseModel
    {
        public string Firstname { get; set; }
        
        public string Lastname { get; set; }
        
        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string SelectedGender { get; set; }

        public string MassUnit { get; set; }

        public int RepsMinimum { get; set; }

        public int RepsMaximum { get; set; }

        public int? Age { get; set; }

        public decimal? Increments { get; set; }

        public decimal? Min { get; set; }

        public decimal? Max { get; set; }

        public bool? IsQuickMode { get; set; }

        public bool? SetStyle { get; set; } = false;

        public bool IsHumanSupport { get; set; }

        public bool IsPyramid { get; set; }

        public bool IsCardio { get; set; }

        public string MainGoal { get; set; }

        public string BodyPartPrioriy { get; set; }

        public string AIMessage { get; set; }
        
        public string ReminderDays { get; set; }
        public TimeSpan? ReminderTime { get; set; }

        public int? TimeBeforeWorkout { get; set; }
        public bool IsReminderEmail { get; set; }

        public MultiUnityWeight BodyWeight { get; set; }
        public MultiUnityWeight WeightGoal { get; set; }

        public LearnMore LearnMoreDetails { get; set; }

        public EquipmentModel EquipmentModel { get; set; }

        public long? ProgramId { get; set; }

        public bool? IsMobility { get; set; }

        public string MobilityLevel { get; set; }

        public bool? IsExerciseQuickMode { get; set; }

        public bool? IsRecommendedReminder { get; set; }

        public bool isDing { get; set; }
        
        public int WorkoutDuration { get; set; }
        

        public float? Height { get; set; }
    }
    public class LearnMore
    {
        public int Age { get; set; }
        public string AgeDesc { get; set; }

        public string Focus { get; set; }
        public string FocusDesc { get; set; }

        public string Exp { get; set; }
        public string ExpDesc { get; set; }
    }
}
