using System;
using System.Collections.Generic;
using DrMuscle.Dependencies;
using DrMuscle.Layout;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DrMuscle.Screens.Workouts
{
    public partial class ExerciseVideoPage : DrMusclePage
    {
        public ExerciseVideoPage()
        {
            InitializeComponent();             
            this.ToolbarItems.Clear();
            if (Device.RuntimePlatform.Equals(Device.Android))
                NavigationPage.SetHasNavigationBar(this, false);
        }

    }
}
