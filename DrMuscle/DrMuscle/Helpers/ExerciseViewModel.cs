using System;
using DrMuscleWebApiSharedModel;

namespace DrMuscle.Helpers
{
    public class ExerciseViewModel
    {
        public ExerciseViewModel()
        {
        }
        private ExerciceModel _exercise;

        public ExerciseViewModel(ExerciceModel exercise)
        {
            this._exercise = exercise;
        }

    }
}
