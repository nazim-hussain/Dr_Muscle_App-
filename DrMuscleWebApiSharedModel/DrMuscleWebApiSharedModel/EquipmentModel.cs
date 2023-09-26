using System;
using DrMuscleWebApiSharedModel;

namespace DrMuscleWebApiSharedModel
{
    public class EquipmentModel : BaseModel
    {
        public bool IsEquipmentEnabled { get; set; }
        public bool IsChinUpBarEnabled { get; set; }
        public bool IsPullyEnabled { get; set; }
        public bool IsPlateEnabled { get; set; }
        public bool IsDumbbellEnabled { get; set; }

        public bool IsHomeEquipmentEnabled { get; set; }
        public bool IsHomeChinupBar { get; set; }
        public bool IsHomePully { get; set; }
        public bool IsHomePlate { get; set; }
        public bool IsHomeDumbbell { get; set; }
        public bool IsOtherEquipmentEnabled { get; set; }
        public bool IsOtherChinupBar { get; set; }
        public bool IsOtherPully { get; set; }
        public bool IsOtherPlate { get; set; }
        public bool IsOtherDumbbell { get; set; }
        public string Active { get; set; }

        public string AvilablePlate { get; set; }
        public string AvilableHomePlate { get; set; }
        public string AvilableOtherPlate { get; set; }

        public string AvilableDumbbell { get; set; }
        public string AvilableHomeDumbbell { get; set; }
        public string AvilableOtherDumbbell { get; set; }

        public string AvilableLbPlate { get; set; }
        public string AvilableHomeLbPlate { get; set; }
        public string AvilableOtherLbPlate { get; set; }

        public string AvilableLbDumbbell { get; set; }
        public string AvilableHomeLbDumbbell { get; set; }
        public string AvilableOtherLbDumbbell { get; set; }


        public string AvilablePulley { get; set; }
        public string AvilableHomePulley { get; set; }
        public string AvilableOtherPulley { get; set; }

        public string AvilableLbPulley { get; set; }
        public string AvilableHomeLbPulley { get; set; }
        public string AvilableOtherLbPulley { get; set; }

    }
}
