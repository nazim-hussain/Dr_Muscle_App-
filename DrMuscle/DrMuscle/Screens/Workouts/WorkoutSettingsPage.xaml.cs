using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;

namespace DrMuscle.Screens.Workouts
{
    public partial class WorkoutSettingsPage : DrMusclePage
    {
        bool IsLoading = false;
        int BackOff = 0;
        public WorkoutSettingsPage()
        {
            InitializeComponent();
            
            //BackOffPicker.Unfocused += BackOffPicker_Unfocused;
        }

    }
}
