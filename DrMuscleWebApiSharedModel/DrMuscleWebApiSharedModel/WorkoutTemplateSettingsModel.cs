using System;
namespace DrMuscleWebApiSharedModel
{
    public class WorkoutTemplateSettingsModel : WorkoutTemplateModel
    {
        public string Notes { get; set; }

        public bool IsCustomReps { get; set; }
        public int? RepsMaxValue { get; set; }
        public int? RepsMinValue { get; set; }
        public bool? IsBackOffSet { get; set; }
        public bool IsCustomWorkSets { get; set; }
        public int? SetCount { get; set; }
    }
}
