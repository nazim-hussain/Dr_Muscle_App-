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
using DrMuscle.Effects;
using System.Reflection;
using System.IO;
using System.Net.Http;
using DrMuscle.Screens.Subscription;
using System.Threading;
using DrMuscle.Screens.User;
using Xamarin.Forms.Xaml;
using OxyPlot;
using SQLite;
using DrMuscle.Controls;
using Xamarin.Essentials;
using static Xamarin.Forms.Internals.GIFBitmap;

namespace DrMuscle.Screens.Workouts
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KenkoChooseYourWorkoutExercisePage : DrMusclePage, INotifyPropertyChanged
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

        private ObservableCollection<ExerciseWorkSetsModel> _exerciseItems;
        public ObservableCollection<ExerciseWorkSetsModel> exerciseItems
        {
            get { return _exerciseItems; }
            set
            {
                _exerciseItems = value;
                OnPropertyChanged("exerciseItems");
            }
        }
        
        public List<ObservableGroupCollection<ExerciseWorkSetsModel, WorkoutLogSerieModel>> GroupedData { get; set; }

        protected override void OnSizeAllocated(double width, double height)
        {
           
        }


        
        public KenkoChooseYourWorkoutExercisePage()
        {
            InitializeComponent();
            
        }

        private void ExerciseListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            


        }

        private void ExerciseListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }

        private void RefreshLocalized()
        {
            //Title = AppResources.ChooseExercise;
            // LblTodaysExercises.Text = AppResources.TodaYExercises;
            //  SaveWorkoutButton.Text = "Finish workout"; // AppResources.FinishAndSaveWorkout;
        }

        bool TimerBased = false;
        string timeRemain = "0";
        async void OnTimerDone()
        {
            

        }

        void OnTimerStop()
        {
           
        }

        void OnTimerChange(int remaining)
        {
          
        }

       
        private string GetKeyValue(bool isPlate, bool isDumbbell, bool isPulley, bool isKg)
        {
            var keyVal = "";
            if (isPlate)
            {

                if (isKg)
                {
                    keyVal = LocalDBManager.Instance.GetDBSetting("PlatesKg").Value;

                    if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("PlatesKg").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("HomePlatesKg").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("OtherPlatesKg").Value;
                    }
                }
                else
                {
                    keyVal = LocalDBManager.Instance.GetDBSetting("PlatesLb").Value;
                    if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("PlatesLb").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("HomePlatesLb").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("OtherPlatesLb").Value;
                    }
                }

            }
            if (isDumbbell)
            {

                if (isKg)
                {
                    keyVal = LocalDBManager.Instance.GetDBSetting("DumbbellKg").Value;

                    if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("DumbbellKg").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("HomeDumbbellKg").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("OtherDumbbellKg").Value;
                    }
                }
                else
                {
                    keyVal = LocalDBManager.Instance.GetDBSetting("DumbbellLb").Value;
                    if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("DumbbellLb").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("HomeDumbbellLb").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("OtherDumbbellLb").Value;
                    }
                }
            }
            if (isPulley)
            {

                if (isKg)
                {
                    keyVal = LocalDBManager.Instance.GetDBSetting("PulleyKg").Value;

                    if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("PulleyKg").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("HomePulleyKg").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("OtherPulleyKg").Value;
                    }
                }
                else
                {
                    keyVal = LocalDBManager.Instance.GetDBSetting("PulleyLb").Value;
                    if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("PulleyLb").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("HomePulleyLb").Value;
                    }
                    if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                    {
                        keyVal = LocalDBManager.Instance.GetDBSetting("OtherPulleyLb").Value;
                    }
                }
            }
            return keyVal;
        }

     
        public override async void OnBeforeShow()
        {
            base.OnBeforeShow();
           
        }

       
        //HIIT Cardio Id: #16508
        protected async void SetUpCompletePopup(decimal weight1, string exerciseName, ExerciseWorkSetsModel exe, int reps = 2, bool IsBodyweight = false)
        {
          
        }

    }

    public enum KenkoHeaderItemType
    {
        Regular,
        Footer

    }

    public class KenkoHeaderDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RegularDateTemplate { get; set; }
        public DataTemplate FooterExerciseTemplate { get; set; }
        public DataTemplate SetsTemplate { get; set; }

        public KenkoHeaderDataTemplateSelector()
        {

            this.SetsTemplate = new DataTemplate(typeof(SetsCell));
        }
        //public KenkoHeaderDataTemplateSelector()
        //{
        //    // Retain instances!
        //    this.RegularDateTemplate = RegularTemplate;// new DataTemplate(typeof(SetDisplayCell));
        //    this.setNextCell = new DataTemplate(typeof(SetsCell));
        //    this.setCompletedCell = new DataTemplate(typeof(WelcomeCell));
        //}
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item == null)
                return FooterExerciseTemplate;
            if (item is WorkoutLogSerieModelRef)
                return this.SetsTemplate;

            if (((ExerciseWorkSetsModel)item).IsFinishWorkoutExe)
            {
                return RegularDateTemplate;
            }
            else
                return RegularDateTemplate;

        }
    }
}