using System;
using Newtonsoft.Json;

namespace DrMuscleWebApiSharedModel
{
    public class MealPlanModel
    {
        public MealPlanModel()
        {
        }
        public DateTime Date { get; set; }

        public string Height { get; set; }

        public string Email { get; set; }

        public string Weight { get; set; }

        public string WeightGoal { get; set; }

        public int? Age { get; set; }

        public decimal? BodyFat { get; set; }

        public string Job { get; set; }

        public string HowMuchExercise { get; set; }

        public string FoodPreferences { get; set; }

        public string AnyAllergies { get; set; }

        public string UnlikeFood { get; set; }

        public string FavouriteFood { get; set; }

        public string MealSize { get; set; }

        public bool IsBudgetConcern { get; set; }

        public string HowMuchMeal { get; set; }

        public string CookBreakfastTime { get; set; }

        public string CookLunchTime { get; set; }

        public string CookSupperTime { get; set; }

        public string CookingSkill { get; set; }

        public bool SkipBreakfast { get; set; }
        public bool SkipLunch { get; set; }
        public bool SkipSupper { get; set; }
        public string MainGoal { get; set; }

    }

    public class DmmMealPlan
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string FavoriteDiet { get; set; }
        public bool IsAnyAllergies { get; set; }
        public bool IsSimpleMeal { get; set; }
        public string Allergies { get; set; }
        public string ExercisePerWeek { get; set; }
        public string ActiveOnJob { get; set; }
        public string VegetarianOptions { get; set; }
        public DateTime? CreatedDate { get; set; }

        public decimal? TargetIntake { get; set; }
        public double? DaysOnPlan { get; set; }
        public decimal? WeightChange { get; set; }
        public decimal? DailyWeightChangePercent { get; set; }
        public double? CurrentBodyWeight { get; set; }
        public double? TargetBodyWeight { get; set; }


        public virtual UserInfosModel User { get; set; }
    }

    public partial class DmmMeal : BaseModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string MealInfo { get; set; }
        public DateTime? CreatedDate { get; set; }

        public int MealCount { get; set; }
        [JsonIgnore]
        public virtual UserInfosModel User { get; set; }
    }
}
