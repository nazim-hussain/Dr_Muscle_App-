using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMuscleWebApiSharedModel
{
    public class RecommendationModel : BaseModel
    {
        public int Series { get; set; }
        public int Reps { get; set; }
        public MultiUnityWeight Weight { get; set; }
        public decimal OneRMProgress { get; set; }
        public decimal RecommendationInKg { get; set; }
        public decimal OneRMPercentage { get; set; }
        public int WarmUpReps1 { get; set; }
        public int WarmUpReps2 { get; set; }
        public MultiUnityWeight WarmUpWeightSet1 { get; set; }
        public MultiUnityWeight WarmUpWeightSet2 { get; set; }
        public List<WarmUps> WarmUpsList { get; set; }
        public int WarmupsCount { get; set; }
        public int RpRest { get; set; }
        //public int WarmUpRest1 { get; set; }
        //public int WarmUpRest2 { get; set; }
        public int NbPauses { get; set; }
        public int NbRepsPauses { get; set; }
        public bool IsEasy { get; set; }
        public bool IsMedium { get; set; }
        public bool IsBodyweight { get; set; }
        public MultiUnityWeight Increments { get; set; }
        public MultiUnityWeight Max { get; set; }
        public MultiUnityWeight Min { get; set; }
        public bool IsNormalSets { get; set; }
        public bool IsDeload { get; set; }
        public bool IsBackOffSet { get; set; }
        public bool? IsDefaultUnilateral { get; set; }
        public MultiUnityWeight BackOffSetWeight { get; set; }
        public bool IsMaxChallenge { get; set; }
        public bool IsLightSession { get; set; }
        public DateTime? LastLogDate { get; set; }
        public int FirstWorkSetReps { get; set; }
        public MultiUnityWeight FirstWorkSetWeight { get; set; }
        public MultiUnityWeight FirstWorkSet1RM { get; set; }
        public bool IsPyramid { get; set; }
        public bool IsReversePyramid { get; set; }
        public List<WorkoutLogSerieModel> HistorySet { get; set; }
        public int MinReps { get; set; }
        public int MaxReps { get; set; }
        public bool isPlateAvailable { get; set; }
        public bool isDumbbellAvailable { get; set; }
        public bool isPulleyAvailable { get; set; }
        public int? days { get; set; }
    }

    public class WarmUps
    {
        public int WarmUpReps { get; set; }
        public MultiUnityWeight WarmUpWeightSet { get; set; }
    }
}
