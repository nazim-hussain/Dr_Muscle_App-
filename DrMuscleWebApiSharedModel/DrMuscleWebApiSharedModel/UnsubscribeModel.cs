using System;
namespace DrMuscleWebApiSharedModel
{
    public class UnsubscribeModel : BaseModel
    {
        public string Key { get; set; }
        public string Email { get; set; }
        public string Validation { get; set; }
        public string Type { get; set; }
    }

    public enum UnsubscribeType
    {
         None,
         Weekly,
         Daily,
         Onboarding,
         MealPlan,
         Reminder,
         PrepareNextWorkout
    }
}
