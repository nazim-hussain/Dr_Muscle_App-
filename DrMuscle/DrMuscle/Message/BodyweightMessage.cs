using System;
using DrMuscle.Enums;

namespace DrMuscle.Message
{
    public class BodyweightMessage
    {
        public BodyweightMessage()
        {
        }
        public string BodyWeight { get; set; }

        public double Weight { get; set; }
        public WeightType WeightType { get; set; }


    }

    public class GoalWeightMessage
    {
        public GoalWeightMessage()
        {
        }
        public string WeightGoal { get; set; }

    }
}
