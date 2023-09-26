using Acr.UserDialogs;
using DrMuscle.Helpers;
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
using DrMuscle.Resx;
using DrMuscle.Views;
using Rg.Plugins.Popup.Services;
using System.Globalization;

namespace DrMuscle.Screens.Workouts
{
    public partial class ChooseWorkoutOrder : DrMusclePage
    {
        public ObservableListCollection<WorkoutTemplateModel> WorkoutItems = new ObservableListCollection<WorkoutTemplateModel>();

        public ChooseWorkoutOrder()
        {
            InitializeComponent();

           
        }

    }
}
