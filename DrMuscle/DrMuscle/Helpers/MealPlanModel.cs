using System;
namespace DrMuscle.Models
{
    public class MealPlanModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FavoriteDiet { get; set; }
        public bool IsAnyAllergies { get; set; }
        public string Allergies { get; set; }
        public string ExercisePerWeek { get; set; }
        public string ActiveOnJob { get; set; }
        public string VegetarianOptions { get; set; }
        public DateTime? CreatedDate { get; set; }


        public string TargetIntake { get; set; }
        public string DaysOnPlan { get; set; }
        public string WeightChange { get; set; }
        public string DailyWeightChangePercent { get; set; }
        public float? TargetBodyweight { get; set; }

        public MealPlanModel()
        {
        }
    }
}
