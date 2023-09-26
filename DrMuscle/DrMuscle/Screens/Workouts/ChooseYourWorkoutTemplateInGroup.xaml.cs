using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;
using DrMuscle.Resx;
using DrMuscle.Screens.Me;

namespace DrMuscle.Screens.Workouts
{
    public partial class ChooseYourWorkoutTemplateInGroup : DrMusclePage
    {
        private List<WorkoutTemplateModel> workouts;
        public ObservableCollection<WorkoutTemplateModel> workoutItems = new ObservableCollection<WorkoutTemplateModel>();
        bool isAppliedSettings = false;

        public ChooseYourWorkoutTemplateInGroup()
        {
            InitializeComponent();

          
        }

    }
}
