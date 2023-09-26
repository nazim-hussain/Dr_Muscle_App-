using DrMuscle;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Screens.Exercises;
using DrMuscleWebApiSharedModel;
using System;
using Xamarin.Forms;
using DrMuscle.Layout;
using DrMuscle.Message;
using Acr.UserDialogs;
using System.Text.RegularExpressions;
using System.Globalization;
using DrMuscle.Localize;
using DrMuscle.Resx;
using System.Collections.ObjectModel;
using DrMuscle.Constants;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;
using System.Collections.Generic;
using Plugin.Connectivity;
using DrMuscle.Screens.Workouts;
using System.Linq;
using DrMuscle.Screens.User.OnBoarding;
using System.Threading.Tasks;
using DrMuscle.Services;
using System.Threading;
using DrMuscle.Screens.Subscription;
using System.Net.Http;

namespace DrMuscle.Screens.User
{
    public partial class EquipmentSettingsPage : DrMusclePage
    {
        public event EventHandler IsActiveChanged;

        public EquipmentSettingsPage()
        {
            InitializeComponent();
            Title = "Equipment";
        }
    }
}


