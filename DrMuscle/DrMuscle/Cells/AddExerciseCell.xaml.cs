﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Screens.Workouts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddExerciseCell : ViewCell
    {
        public AddExerciseCell()
        {
            InitializeComponent();
            //ToggleSwitch.SetBinding(Switch.IsToggledProperty, "IsSelected");
        }

       
    }
}