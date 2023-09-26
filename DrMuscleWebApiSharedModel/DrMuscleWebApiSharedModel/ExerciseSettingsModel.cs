using System;
namespace DrMuscleWebApiSharedModel
{
    public class ExerciseSettingsModel : ExerciceModel
    {
        public long ExerciseSettingsId { get; set; }
        public bool IsCustomIncrements { get; set; }
        public MultiUnityWeight Increments { get; set; }
        public MultiUnityWeight Min { get; set; }
        public MultiUnityWeight Max { get; set; }
        public string Notes { get; set; }

        public bool IsCustomReps { get; set; }
        public int? RepsMaxValue { get; set; }
        public int? RepsMinValue { get; set; }

        public bool IsCustomSets { get; set; }
        public bool? IsNormalSets { get; set; }

        public bool? IsBackOffSet { get; set; }

        public bool IsCustomWarmups { get; set; }
        public int? WarmupsValue { get; set; }
        public int? SetCount { get; set; }
        public bool? IsDefaultUnilateral { get; set; }

        public bool IsFavorite { get; set; }
    }

    public enum RepsType
    {
        BuildMuscle,
        BuildMuscleBurnFat,
        FatBurning,
        Custom
    }
}
