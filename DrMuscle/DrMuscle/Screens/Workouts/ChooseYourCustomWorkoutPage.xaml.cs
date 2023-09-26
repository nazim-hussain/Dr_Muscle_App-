using Acr.UserDialogs;
using DrMuscle.Screens.Exercises;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using DrMuscle.Screens.History;
using DrMuscle.Screens.Subscription;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Resx;
using DrMuscle.Constants;
using Microsoft.AppCenter.Crashes;
using System.Globalization;
using DrMuscle.Views;
using Rg.Plugins.Popup.Services;
using DrMuscle.Screens.Me;
using DrMuscle.Effects;
using DrMuscle.Screens.User;

namespace DrMuscle.Screens.Workouts
{
    public partial class ChooseYourCustomWorkoutPage : DrMusclePage
    {
        public List<WorkoutTemplateModel> workouts;
        public List<WorkoutTemplateGroupModel> workoutGroups;
        public ObservableCollection<WorkoutTemplateModel> workoutItems = new ObservableCollection<WorkoutTemplateModel>();
        public ObservableCollection<WorkoutTemplateGroupModel> workoutOrderItems = new ObservableCollection<WorkoutTemplateGroupModel>();
        ProgramListPopup popup;
        public ChooseYourCustomWorkoutPage()
        {
            InitializeComponent();

       

           
        }

    }
}
