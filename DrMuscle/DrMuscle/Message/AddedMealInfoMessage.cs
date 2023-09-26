using System;
namespace DrMuscle.Message
{
    public class AddedMealInfoMessage
    {
        public string MealInfoStr { get; set; }
        public bool IsCanceled { get; set; }
        public AddedMealInfoMessage()
        {
        }
    }
}
