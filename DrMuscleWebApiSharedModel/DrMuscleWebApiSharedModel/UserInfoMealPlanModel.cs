using System;
namespace DrMuscleWebApiSharedModel
{
    public class UserInfoMealPlanModel : BaseModel
    {
        public MealPlanModel MealPlan { get; set; }
        public UserInfosModel UserInfos { get; set; }
        public string BillingCycle { get; set; }
        public string PaymentMethod { get; set; }
    }
}
