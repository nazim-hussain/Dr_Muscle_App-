using System;
namespace DrMuscle.Models
{
    public class WeightModel
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public double Weight { get; set; }
        public double TargetWeight { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
