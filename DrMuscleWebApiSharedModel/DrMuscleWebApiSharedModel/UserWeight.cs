using System;
namespace DrMuscleWebApiSharedModel
{
    public class UserWeight
    {
        public Decimal Weight { get; set; }
        public DateTime CreatedDate { get; set; }
        public Decimal? TargetIntake { get; set; }
        public long Id {get; set; }
        public string Label { get; set; }
    }
}
