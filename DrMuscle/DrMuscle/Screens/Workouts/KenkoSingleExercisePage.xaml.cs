using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Screens.Exercises;
using DrMuscleWebApiSharedModel;
using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using System.Globalization;
using DrMuscle.Helpers;
using DrMuscle.Resx;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DrMuscle.Message;
using DrMuscle.Screens.Me;
using DrMuscle.Layout;
using Newtonsoft.Json;
using Microsoft.AppCenter.Crashes;
using DrMuscle.Views;
using Rg.Plugins.Popup.Services;
using DrMuscle.Constants;
using DrMuscle.Cells;
using Plugin.Connectivity;
using DrMuscle.Services;
using System.Reflection;
using System.IO;
using DrMuscle.Screens.Subscription;
using Xamarin.Forms.Xaml;
using DrMuscle.Controls;

namespace DrMuscle.Screens.Workouts
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KenkoSingleExercisePage : DrMusclePage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetObservableProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

       
        public KenkoSingleExercisePage()
        {
            InitializeComponent();
            
         
        }

        private void RefreshLocalized()
        {
            Title = AppResources.ChooseExercise;
          
        }

        
        private async void NewTapped(object sender, EventArgs args)
        {
            
        }

       
        public async void CancelClick(ExerciseWorkSetsModel m)
        {
            
        }

        public async void FinishSetup(ExerciseWorkSetsModel m, string userWeight, bool isBodyweight)
        {
            
        }

        protected async void SetUpCompletePopup(decimal weight1, string exerciseName, ExerciseWorkSetsModel exe, int reps = 2, bool IsBodyweight = false)
        {

            
        }

    }

}