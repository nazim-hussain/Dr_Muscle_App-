using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acr.UserDialogs;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Resx;
using DrMuscle.Screens.Subscription;
using DrMuscleWebApiSharedModel;
using Plugin.Connectivity;
using Xamarin.Forms;

namespace DrMuscle.Screens.Workouts
{
    public partial class FeaturedProgramPage : DrMusclePage
    {
        public List<WorkoutTemplateGroupModel> workoutGroups;
        public ObservableCollection<WorkoutTemplateGroupModel> workoutOrderItems = new ObservableCollection<WorkoutTemplateGroupModel>();

        public FeaturedProgramPage()
        {
            InitializeComponent();
           
         
        }

    }
}
