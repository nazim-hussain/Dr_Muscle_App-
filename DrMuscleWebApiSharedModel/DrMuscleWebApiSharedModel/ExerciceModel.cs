using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class ExerciceModel : BaseModel
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public bool IsSystemExercise { get; set; }
        public bool IsSwapTarget { get; set; }
        public bool IsFinished { get; set; }
        public long? BodyPartId { get; set; }

        public bool IsUnilateral { get; set; }
        public bool IsTimeBased { get; set; }

        public long? EquipmentId { get; set; }
        public bool IsEasy { get; set; }
        public bool IsMedium { get; set; }
        public bool IsBodyweight { get; set; }
        public string VideoUrl { get; set; }
        public bool IsNextExercise { get; set; }

        public bool IsPlate { get; set; }
        public bool IsWeighted { get; set; }

        public bool IsPyramid { get; set; }
        //FeaturedProgram
        public int? RepsMaxValue { get; set; }
        public int? RepsMinValue { get; set; }
        public int? Timer { get; set; }
        public bool IsNormalSets { get; set; }

        public long? WorkoutGroupId { get; set; }
        public bool IsBodypartPriority { get; set; }

        public bool IsFlexibility { get; set; }
        public bool IsOneHanded { get; set; }
        public string LocalVideo { get; set; }

        public bool IsAssisted { get; set; }
        //public int BodyPartId { get; set; }
        //public BodyPartModel BodyPart { get;set; }
        public override string ToString()
        {
            return $"ExerciseModel Id : {this.Id} Label : {this.Label}";
        }
    }

    public class ExerciceModelWithReco : ExerciceModel
    {
        public RecommendationModel RecoModel { get; set; }
        public bool IsSelected { get; set; }
    }

}
