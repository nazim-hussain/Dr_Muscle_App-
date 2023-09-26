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
using DrMuscle.Screens.Workouts;

namespace DrMuscle.Screens.User.OnBoarding
{
    public partial class IntroPage1 : DrMusclePage, INotifyPropertyChanged
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
            base.OnSizeAllocated(width, height);
            if (Device.RuntimePlatform.Equals(Device.Android))
                StatusBarHeight.Height = 5;
            else
                StatusBarHeight.Height = App.StatusBarHeight;
            if (Device.RuntimePlatform.Equals(Device.iOS))
                stackBottomContent.Margin = new Thickness(3, 15, 3, App.StatusBarHeight - 20);
            else
                stackBottomContent.Margin = new Thickness(3, 15, 3, 0);
            //double navigationBarHeight = Math.Abs(App.ScreenSize.Height - height - App.StatusBarHeight);
            // App.NavigationBarHeight = 146 + App.StatusBarHeight;// navigationBarHeight;
        }


        TimerPopup popup;
        bool isAppear = false;
        bool IsPowerlifting;
        private GetUserProgramInfoResponseModel upi = null;
        private bool IsSettingsChanged { get; set; }
        private bool IsGlobalSettingsChanged { get; set; }
        List<long> OpenExercises = new List<long>();
        StackLayout contextMenuStack;
        private bool _isAskedforLightSession, _isAskedforDeload;
        private WorkoutLogSerieModelRef _backOffSet;
        private bool _isHideTimerCall = false;
        private bool _superSetRunning = false;
        private bool _tryChallenge = false;
        private bool _tryDeload = false;
        private bool _trySwap = false;
        private bool _trySwapMenu = false;
        private bool _isAskedforSwipe = false;
        private bool _areExercisesUnfnished = false;
        private decimal _userBodyWeight = 0;
        private IFirebase _firebase;
        View vHeader;
        DrMuscleButton btnChallenge, btnDeload, btnSwap, btnMore;
        Dictionary<long, View> vHeaders = new Dictionary<long, View>();
        public IntroPage1()
        {
            InitializeComponent();
            try
            {
                exerciseItems = new ObservableCollection<ExerciseWorkSetsModel>();

                ExerciseListView.ItemsSource = GroupedData;
                ExerciseListView.ItemTapped += ExerciseListView_ItemTapped;
                if (Device.RuntimePlatform.Equals(Device.Android))
                    ExerciseListView.ItemAppearing += ExerciseListView_ItemAppearing; ;

                var gesture = new TapGestureRecognizer();
                gesture.Tapped += ListTapped;
                ExerciseListView.GestureRecognizers.Add(gesture);

                _firebase = DependencyService.Get<IFirebase>();

                if (LocalDBManager.Instance.GetDBSetting("PlatesKg") == null || LocalDBManager.Instance.GetDBSetting("PlatesLb") == null)
                {
                    var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                    LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);

                    var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                    LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
                }


                MessagingCenter.Subscribe<Message.ExerciseDeleteMessage>(this, "ExerciseDeleteMessage", (obj) =>
                {
                    IsSettingsChanged = false;
                });
                MessagingCenter.Subscribe<Message.GlobalSettingsChangeMessage>(this, "GlobalSettingsChangeMessage", (obj) =>
                {
                    if (obj.IsDisappear)
                        IsGlobalSettingsChanged = true;
                });

                Timer.Instance.OnTimerChange += OnTimerChange;
                Timer.Instance.OnTimerDone += OnTimerDone;
                Timer.Instance.OnTimerStop += OnTimerStop;
            }
            catch (Exception ex)
            {

            }
        }

        private void ExerciseListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                HideContextButton();
            });
        }

        private void ExerciseListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }

        private void RefreshLocalized()
        {

        }

        bool TimerBased = false;
        string timeRemain = "0";
        async void OnTimerDone()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    BtnTimer.Text = null;
                    BtnTimer.Image = "stopwatch.png";
                });
                try
                {
                    if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen") == null)
                        LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");
                    if (TimerBased && LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true" && isAppear)
                    {
                        await Task.Delay(100);
                        TimerBased = false;
                        popup = new TimerPopup(false);
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", timeRemain);
                        popup.popupTitle = "Work";
                        popup?.SetTimerRepsSets("");
                        popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                        Timer.Instance.Remaining = int.Parse(popup.RemainingSeconds);
                        PopupNavigation.Instance.PushAsync(popup);
                        Timer.Instance.StartTimer();

                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }

        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        void OnTimerStop()
        {
            try
            {
                BtnTimer.Text = null;
                BtnTimer.Image = "stopwatch.png";
            }
            catch (Exception ex)
            {

            }
        }

        void OnTimerChange(int remaining)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Timer.Instance.State == "RUNNING")
                {
                    BtnTimer.Text = remaining.ToString();
                    BtnTimer.Image = null;
                }
                else
                //if (remaining.ToString().Equals("0"))
                {
                    BtnTimer.Text = null;
                    BtnTimer.Image = "stopwatch.png";
                }
            });
        }

        private void UpdateOneRM(WorkoutLogSerieModelRef models, decimal weight, int reps)
        {
            if (contextMenuStack != null)
                HideContextButton();
            try
            {
                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                        continue;
                    if (item.IsFinished)
                    {
                        item.RecoModel = null;
                        return;
                    }
                    //Update rest of sets from this update model
                    var index = item.IndexOf(models);
                    //                        if (models.IsBodyweight)
                    //weight = models.Weight.Kg;

                    if (models.IsFirstWorkSet && item.RecoModel != null && item.RecoModel.FirstWorkSetWeight != null && item.RecoModel.FirstWorkSetWeight.Entered != 0)
                    {
                        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                        if (models.IsBodyweight)
                            weight = isKg ? item.RecoModel.FirstWorkSetWeight.Kg : Convert.ToDecimal(item.RecoModel.FirstWorkSetWeight.Lb, CultureInfo.InvariantCulture);
                        else
                        {
                            weight = Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                        }
                        var lastOneRM = item.RecoModel.FirstWorkSet1RM.Kg;//
                        lastOneRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), item.RecoModel.FirstWorkSetReps);

                        var newWeight = Math.Round(new MultiUnityWeight(weight, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), 2);

                        var currentRM = TruncateDecimal(ComputeOneRM(newWeight, reps), 2);
                        lastOneRM = Math.Round(isKg ? new MultiUnityWeight(lastOneRM, "kg").Kg : new MultiUnityWeight(lastOneRM, "kg").Lb, 1);

                        currentRM = Math.Round(isKg ? new MultiUnityWeight(currentRM, "kg").Kg : new MultiUnityWeight(currentRM, "kg").Lb, 1);




                        var worksets = string.Format("{0:0.##} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");
                        if (models.IsBodyweight)
                        {
                            worksets = "body";
                            if (models.Id == 16508)
                            {
                                worksets = "fast";
                            }
                            if (models.Id >= 16897 && models.Id <= 16907 || models.Id == 14279 || models.Id >= 21508 && models.Id <= 21514)
                            {
                                worksets = "bands";
                            }
                        }
                        if (currentRM != 0)
                        {
                            var percentage = (currentRM - lastOneRM) * 100 / lastOneRM;
                            if (item.RecoModel.FirstWorkSetWeight.Kg == TruncateDecimal(new MultiUnityWeight(weight + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), isKg ? "kg" : "lb").Kg, 2) && item.RecoModel.FirstWorkSetReps == reps)
                                percentage = 0;
                            if (models.IsMaxChallenge)
                            {
                                models.LastTimeSet = string.Format("Last time: {0} x {1}", item.RecoModel.FirstWorkSetReps, worksets);
                                models.SetTitle = string.Format("Try max reps today:", item.RecoModel.FirstWorkSetReps, worksets);
                            }

                            else
                            {
                                models.LastTimeSet = string.Format("Last time: {0:0.##} x {1}", item.RecoModel.FirstWorkSetReps, worksets);
                                models.SetTitle = string.Format("For {0}{1:0.00}% do:", percentage >= 0 ? "+" : "", percentage);
                            }
                            popup?.SetLastTimeOnlyText(string.Format("{0}{1:0.00}%:", percentage >= 0 ? "+" : "", percentage), string.Format("Last time: {0} x {1}", item.RecoModel.FirstWorkSetReps, worksets));
                        }
                    }

                    break;
                }


            }
            catch (Exception ex)
            {

            }
        }


        private void UpdateWeoghtRepsMessageTapped(WorkoutLogSerieModelRef models)
        {
            if (contextMenuStack != null)
                HideContextButton();

            try
            {
                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                        continue;
                    if (item.IsFinished)
                    {
                        item.RecoModel = null;
                        return;
                    }
                    //Update rest of sets from this update model

                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

                    var index = item.IndexOf(models);
                    if (item.IsReversePyramid)
                    {

                        if (models.IsFirstWorkSet && item.RecoModel != null && item.RecoModel.FirstWorkSetWeight != null && item.RecoModel.FirstWorkSetWeight.Entered != 0)
                        {
                            var lastOneRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), item.RecoModel.FirstWorkSetReps);

                            decimal currentRM = 0;
                            if (models.IsBodyweight)
                                currentRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : Convert.ToDecimal(item.RecoModel.FirstWorkSetWeight.Lb, CultureInfo.InvariantCulture), isKg ? "kg" : "lb").Kg, models.Reps);
                            else
                            {
                                var w = isKg ? models.Weight.Kg : Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                var newWeight = Math.Round(new MultiUnityWeight(w, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), 2);
                                currentRM = TruncateDecimal(ComputeOneRM(newWeight, models.Reps), 2);
                            }


                            //=====
                            lastOneRM = Math.Round(isKg ? new MultiUnityWeight(lastOneRM, "kg").Kg : new MultiUnityWeight(lastOneRM, "kg").Lb, 1);
                            currentRM = Math.Round(isKg ? new MultiUnityWeight(currentRM, "kg").Kg : new MultiUnityWeight(currentRM, "kg").Lb, 1);
                            //=====



                            var worksets = string.Format("{0:0.##} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");

                            if (currentRM != 0)
                            {
                                var percentage = (currentRM - lastOneRM) * 100 / lastOneRM;

                                var newWeigh = new MultiUnityWeight(isKg ? models.Weight.Kg : Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb");
                                if (item.RecoModel.FirstWorkSetWeight.Kg == TruncateDecimal(newWeigh.Kg, 2) && item.RecoModel.FirstWorkSetReps == models.Reps)
                                    percentage = 0;

                                models.LastTimeSet = string.Format("Last time: {0} x {1}", item.RecoModel.FirstWorkSetReps, worksets);
                                models.SetTitle = string.Format("For {0}{1:0.00}% do:", percentage >= 0 ? "+" : "", percentage);
                            }
                        }


                        decimal weightdif = 0;
                        int repsdif = 0;
                        for (int j = index; j < item.Count; j++)
                        {

                            WorkoutLogSerieModelRef updatingItem = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[j];
                            if (j == index)
                            {
                                updatingItem.Weight = models.Weight;
                                updatingItem.Reps = models.Reps;

                                if (updatingItem.WeightDouble != updatingItem.PreviousWeightDouble)
                                {
                                    var currentWeigght = Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                    var prevWeigght = Convert.ToDecimal(models.PreviousWeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                    weightdif = prevWeigght - currentWeigght;
                                }

                                if (models.PreviousReps != models.Reps)
                                {
                                    repsdif = updatingItem.PreviousReps - updatingItem.Reps;
                                }
                                continue;
                            }

                            if (!updatingItem.IsFinished && !updatingItem.IsWarmups)
                            {
                                //var count = item.Where(x => x.IsWarmups == true).Count();
                                //var last = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[j - 1];
                                //var rec = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[index];

                                //decimal weight = RecoComputation.RoundToNearestIncrement(last.Weight.Kg + (last.Weight.Kg * ((decimal)0.1)), item.RecoModel.Increments == null ? (decimal)2.0 : item.RecoModel.Increments.Kg, item.RecoModel.Min?.Kg, item.RecoModel.Max?.Kg);
                                decimal weight = 1;
                                //if (weightdif != 0)
                                //{
                                var weigh = Convert.ToDecimal(updatingItem.PreviousWeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                if (weightdif > 0)
                                    weight = weigh - Math.Abs(weightdif);
                                else
                                    weight = weigh + Math.Abs(weightdif);
                                updatingItem.Weight = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(weight, item.RecoModel.Increments == null ? isKg ? (decimal)2.0 : (decimal)5.0 : isKg ? item.RecoModel.Increments.Kg : item.RecoModel.Increments.Lb, isKg ? item.RecoModel.Min?.Kg : item.RecoModel.Min?.Lb, isKg ? item.RecoModel.Max?.Kg : item.RecoModel.Max?.Lb), isKg ? "kg" : "lb");
                                //}
                                //if (repsdif != 0)
                                //{
                                if (repsdif > 0)
                                    updatingItem.Reps = updatingItem.PreviousReps - Math.Abs(repsdif);
                                else
                                    updatingItem.Reps = updatingItem.PreviousReps + Math.Abs(repsdif);
                                //}

                                //if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
                                //{
                                //    if (weight >= last.Weight.Kg)
                                //    {
                                //        weight = RecoComputation.RoundToNearestIncrement(last.Weight.Kg + (item.RecoModel.Increments != null ? item.RecoModel.Increments.Kg : (last.Weight.Kg * ( (decimal)0.1))), item.RecoModel.Increments == null ? (decimal)2.0 : item.RecoModel.Increments.Kg, item.RecoModel.Min?.Kg, item.RecoModel.Max?.Kg);
                                //        //if (weight == last.Weight.Kg)
                                //        //{
                                //        //    updatingItem.Reps = last.Reps; 
                                //        //}
                                //        //else
                                //            updatingItem.Reps = last.Reps - (int)Math.Floor(last.Reps*0.4);
                                //    }
                                //    updatingItem.Weight = new MultiUnityWeight(weight, "kg");
                                //    if (updatingItem.Reps < 1)
                                //        updatingItem.Reps = 1;
                                //}
                                //else
                                //{
                                //    var inc = updatingItem.Increments != null ? updatingItem.Increments.Lb : (decimal)5;
                                //    if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc) >= SaveSetPage.RoundDownToNearestIncrement(last.Weight.Lb, inc))
                                //    {
                                //        weight = RecoComputation.RoundToNearestIncrement(last.Weight.Kg + (item.RecoModel.Increments != null ? item.RecoModel.Increments.Kg : (last.Weight.Kg * ( (decimal)0.1))), item.RecoModel.Increments == null ? (decimal)2 : item.RecoModel.Increments.Kg, item.RecoModel.Min?.Kg, item.RecoModel.Max?.Kg);
                                //        //if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc) == SaveSetPage.RoundDownToNearestIncrement(last.Weight.Lb, inc))
                                //        //{
                                //        //    updatingItem.Reps = last.Reps;
                                //        //}
                                //        //else
                                //            updatingItem.Reps = last.Reps - (int)Math.Ceiling(last.Reps * 0.4);
                                //    }
                                //    updatingItem.Weight = new MultiUnityWeight(SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc), "lb");
                                //}


                                if (weight <= 0)
                                {

                                    weight = updatingItem.Increments != null ? updatingItem.Increments.Kg : (decimal)2; ;

                                    updatingItem.Weight = new MultiUnityWeight(weight, "kg");
                                }
                                if (updatingItem.Reps < 1)
                                    updatingItem.Reps = 1;

                            }
                        }
                        //==========
                    }
                    else if (!item.IsPyramid)
                    {
                        var reps = models.Reps;
                        if (item.IsNormalSets)
                            reps = models.Reps;
                        else
                        {
                            if (models.IsFirstWorkSet)
                                reps = models.Reps <= 5 ? (int)Math.Ceiling((decimal)models.Reps / (decimal)3.0) : (int)models.Reps / 3;

                            if (reps <= 0)
                                reps = 1;
                        }
                        if (models.IsFirstWorkSet && item.RecoModel != null && item.RecoModel.FirstWorkSetWeight != null && item.RecoModel.FirstWorkSetWeight.Entered != 0)
                        {
                            var lastOneRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), item.RecoModel.FirstWorkSetReps);

                            decimal currentRM = 0;
                            if (models.IsBodyweight)
                                currentRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : Convert.ToDecimal(item.RecoModel.FirstWorkSetWeight.Lb, CultureInfo.InvariantCulture), isKg ? "kg" : "lb").Kg, models.Reps);
                            else
                            {
                                //Older implementaion
                                //currentRM = ComputeOneRM(new MultiUnityWeight(isKg ? models.Weight.Kg : Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), models.Reps);

                                ////New implementaion
                                var w = isKg ? models.Weight.Kg : Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);

                                var newWeight = Math.Round(new MultiUnityWeight(w, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), 2);

                                currentRM = TruncateDecimal(ComputeOneRM(newWeight, models.Reps), 2);



                            }

                            //=====
                            lastOneRM = Math.Round(isKg ? new MultiUnityWeight(lastOneRM, "kg").Kg : new MultiUnityWeight(lastOneRM, "kg").Lb, 1);
                            currentRM = Math.Round(isKg ? new MultiUnityWeight(currentRM, "kg").Kg : new MultiUnityWeight(currentRM, "kg").Lb, 1);
                            //=====



                            var worksets = string.Format("{0:0.##} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");


                            if (models.IsBodyweight)
                            {
                                worksets = "body";
                                if (models.Id == 16508)
                                {
                                    worksets = "fast";
                                }
                                if (models.Id >= 16897 && models.Id <= 16907 || models.Id == 14279 || models.Id >= 21508 && models.Id <= 21514)
                                {
                                    worksets = "bands";
                                }
                            }

                            if (currentRM != 0)
                            {
                                var percentage = (currentRM - lastOneRM) * 100 / lastOneRM;

                                var newWeigh = new MultiUnityWeight(isKg ? models.Weight.Kg : Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb");
                                if (item.RecoModel.FirstWorkSetWeight.Kg == TruncateDecimal(newWeigh.Kg, 2) && item.RecoModel.FirstWorkSetReps == models.Reps)
                                    percentage = 0;

                                models.LastTimeSet = string.Format("Last time: {0} x {1}", item.RecoModel.FirstWorkSetReps, worksets);
                                models.SetTitle = string.Format("For {0}{1:0.00}% do:", percentage >= 0 ? "+" : "", percentage);
                            }
                        }

                        for (int i = index; i < item.Count; i++)
                        {
                            //if (item[i] == models)
                            //    continue;
                            WorkoutLogSerieModelRef updatingItem = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[i];
                            if (updatingItem.IsBackOffSet && !updatingItem.IsFinished && !updatingItem.IsWarmups && !updatingItem.IsFirstWorkSet)
                            {

                                var mod = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(isKg ? models.Weight.Kg - (models.Weight.Kg * (decimal)0.3) : models.Weight.Lb - (models.Weight.Lb * (decimal)0.3), item.RecoModel.Increments == null ? (decimal)1.0 : isKg ? item.RecoModel.Increments.Kg : TruncateDecimal(item.RecoModel.Increments.Lb, 5), isKg ? item.RecoModel.Min?.Kg : item.RecoModel.Min?.Lb, isKg ? item.RecoModel.Max?.Kg : item.RecoModel.Max?.Lb), isKg ? "kg" : "lb");



                                updatingItem.Weight = mod;
                                if (Math.Abs(updatingItem.Weight.Kg - models.Weight.Kg) > 0)
                                {
                                    var ob = ((Math.Abs(models.Weight.Kg - updatingItem.Weight.Kg) / models.Weight.Kg) > (decimal)0.3 ? (decimal)0.3 * (decimal)3 : Math.Abs(models.Weight.Kg - updatingItem.Weight.Kg) / models.Weight.Kg * (decimal)3);// Changed 3.66 to 3 after Bruno issue
                                    updatingItem.Reps = (int)reps + (int)Math.Ceiling(reps * ob);
                                }
                                else
                                {
                                    updatingItem.Reps = (int)(reps + Math.Ceiling(reps * 0.2));
                                }
                                continue;
                            }
                            if (!updatingItem.IsFinished && !updatingItem.IsWarmups && !updatingItem.IsFirstWorkSet)
                            {
                                updatingItem.Weight = models.Weight;
                                if (reps != 0)
                                    updatingItem.Reps = reps;
                            }
                        }
                    }
                    else
                    {
                        //Reverse pyramid
                        if (models.IsFirstWorkSet && item.RecoModel != null && item.RecoModel.FirstWorkSetWeight != null && item.RecoModel.FirstWorkSetWeight.Entered != 0)
                        {
                            //var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                            var lastOneRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), item.RecoModel.FirstWorkSetReps);

                            decimal currentRM = 0;
                            if (models.IsBodyweight)
                                currentRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : Convert.ToDecimal(item.RecoModel.FirstWorkSetWeight.Lb, CultureInfo.InvariantCulture), isKg ? "kg" : "lb").Kg, models.Reps);
                            else
                            {
                                var w = isKg ? models.Weight.Kg : Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                var newWeight = Math.Round(new MultiUnityWeight(w, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(item.Id) ? _userBodyWeight : 0), 2);
                                currentRM = TruncateDecimal(ComputeOneRM(newWeight, models.Reps), 2);

                            }

                            //=====
                            lastOneRM = Math.Round(isKg ? new MultiUnityWeight(lastOneRM, "kg").Kg : new MultiUnityWeight(lastOneRM, "kg").Lb, 1);
                            currentRM = Math.Round(isKg ? new MultiUnityWeight(currentRM, "kg").Kg : new MultiUnityWeight(currentRM, "kg").Lb, 1);
                            //=====

                            var worksets = string.Format("{0:0.##} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");

                            if (currentRM != 0)
                            {
                                var percentage = (currentRM - lastOneRM) * 100 / lastOneRM;

                                var newWeigh = new MultiUnityWeight(isKg ? models.Weight.Kg : Convert.ToDecimal(models.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb");
                                if (item.RecoModel.FirstWorkSetWeight.Kg == TruncateDecimal(newWeigh.Kg, 2) && item.RecoModel.FirstWorkSetReps == models.Reps)
                                    percentage = 0;

                                models.LastTimeSet = string.Format("Last time: {0} x {1}", item.RecoModel.FirstWorkSetReps, worksets);
                                models.SetTitle = string.Format("For {0}{1:0.00}% do:", percentage >= 0 ? "+" : "", percentage);
                            }
                        }
                        for (int j = index; j < item.Count; j++)
                        {

                            WorkoutLogSerieModelRef updatingItem = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[j];
                            if (j == index)
                            {
                                updatingItem.Weight = models.Weight;
                                updatingItem.Reps = models.Reps;
                                continue;
                            }

                            if (!updatingItem.IsFinished && !updatingItem.IsWarmups && !updatingItem.IsFirstWorkSet)
                            {
                                var count = item.Where(x => x.IsWarmups == true).Count();
                                var rec = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[j - 1];
                                var last = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[index];
                                var reps = rec.Reps + (j - count) + 1;
                                var lstWeight = Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                decimal weight = RecoComputation.RoundToNearestIncrementPyramid(lstWeight - (lstWeight * ((decimal)0.1)), item.RecoModel.Increments == null ? (decimal)2.0 : item.RecoModel.Increments.Kg, item.RecoModel.Min?.Kg, item.RecoModel.Max?.Kg);
                                if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
                                {
                                    if (weight >= last.Weight.Kg)
                                    {
                                        weight = RecoComputation.RoundToNearestIncrementPyramid(rec.Weight.Kg - (item.RecoModel.Increments != null ? item.RecoModel.Increments.Kg : (rec.Weight.Kg * (decimal)0.1)), item.RecoModel.Increments == null ? (decimal)2.0 : item.RecoModel.Increments.Kg, item.RecoModel.Min?.Kg, item.RecoModel.Max?.Kg);
                                        if (weight == last.Weight.Kg)
                                        {
                                            updatingItem.Reps = rec.Reps;
                                        }
                                        else
                                            updatingItem.Reps = reps;
                                    }
                                    else
                                        updatingItem.Reps = reps;
                                    updatingItem.Weight = new MultiUnityWeight(weight, "kg");
                                }
                                else
                                {
                                    var inc = updatingItem.Increments != null ? Math.Round(updatingItem.Increments.Lb, 2) : (decimal)5;

                                    weight = RecoComputation.RoundToNearestIncrementPyramid(lstWeight - lstWeight * ((decimal)0.1), inc, item.RecoModel.Min?.Lb, item.RecoModel.Max?.Lb);

                                    if (SaveSetPage.RoundDownToNearestIncrement(weight, inc, item.RecoModel.Min?.Lb, item.RecoModel.Max?.Lb) >= SaveSetPage.RoundDownToNearestIncrement(rec.Weight.Lb, inc, item.RecoModel.Min?.Lb, item.RecoModel.Max?.Lb))
                                    {
                                        weight = RecoComputation.RoundToNearestIncrementPyramid(lstWeight - (item.RecoModel.Increments != null ? item.RecoModel.Increments.Lb : (lstWeight * ((decimal)0.1))), item.RecoModel.Increments == null ? (decimal)5 : item.RecoModel.Increments.Lb, item.RecoModel.Min?.Lb, item.RecoModel.Max?.Lb);
                                        if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "lb").Lb, inc, item.RecoModel.Min?.Lb, item.RecoModel.Max?.Lb) == SaveSetPage.RoundDownToNearestIncrement(rec.Weight.Lb, inc, item.RecoModel.Min?.Lb, item.RecoModel.Max?.Lb))
                                        {
                                            updatingItem.Reps = rec.Reps; //
                                        }
                                        else
                                        {
                                            updatingItem.Reps = reps;
                                        }
                                        weight = new MultiUnityWeight(weight, "lb").Lb;
                                    }
                                    else
                                        updatingItem.Reps = reps;
                                    updatingItem.Weight = new MultiUnityWeight(SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "lb").Lb, inc, item.RecoModel.Min?.Lb, item.RecoModel.Max?.Lb), "lb");
                                }
                                if (weight <= 0)
                                {
                                    updatingItem.Reps = last.Reps;
                                    weight = updatingItem.Increments != null ? isKg ? updatingItem.Increments.Kg : updatingItem.Increments.Lb : isKg ? 2 : 5;
                                    if (last.Weight.Kg > (decimal)1.15)
                                        updatingItem.Reps = last.Reps + j + 1;
                                    updatingItem.Weight = new MultiUnityWeight(weight, isKg ? "kg" : "lb");
                                }
                                if (updatingItem.WeightDouble.ReplaceWithDot() == rec.WeightDouble.ReplaceWithDot())
                                    updatingItem.Reps = rec.Reps;
                            }
                        }
                    }
                    break;
                }


            }
            catch (Exception ex)
            {

            }
        }
        public decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }
        private async void Load1RM(long id)
        {
            try
            {

                if (CurrentLog.Instance.Exercise1RM.ContainsKey(id))
                {

                }
                else
                {
                    //var _rm = await DrMuscleRestClient.Instance.GetOneRMForExerciseWithoutLoader(
                    //    new GetOneRMforExerciseModel()
                    //    {
                    //        Username = LocalDBManager.Instance.GetDBSetting("email").Value,
                    //        Massunit = LocalDBManager.Instance.GetDBSetting("massunit").Value,
                    //        ExerciseId = id
                    //    });
                    //if (_rm != null)
                    //    CurrentLog.Instance.Exercise1RM.Add(id, _rm);
                }


            }
            catch (Exception ex)
            {

            }
        }

        private void AddSetMessageTapped(WorkoutLogSerieModelRef models)
        {
            try
            {
                if (contextMenuStack != null)
                    HideContextButton();
                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                        continue;

                    var newSet = new WorkoutLogSerieModelRef()
                    {
                        Id = item.Id,
                        IsLastSet = true,
                        IsFinished = false,
                        Weight = models.Weight,
                        Reps = models.Reps,
                        IsNext = true,
                        SetNo = $"SET {item.Count + 1}/{item.Count + 1}",
                        IsFirstSide = item.IsFirstSide,
                        ExerciseName = item.Label,
                        EquipmentId = item.EquipmentId,
                        Increments = models.Increments,
                        SetTitle = "Last set—you can do this!",
                        IsTimeBased = models.IsTimeBased,
                        IsUnilateral = models.IsUnilateral,
                        IsBodyweight = models.IsBodyweight,
                        IsBackOffSet = models.IsBackOffSet,
                        IsFlexibility = models.IsFlexibility,
                        IsNormalset = models.IsNormalset,
                        BodypartId = models.BodypartId,
                        Min = models.Min,
                        Max = models.Max
                    };
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(item.Id))
                    {
                        var listOfSets = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[item.Id];
                        listOfSets.Add(newSet);
                    }
                    item.Add(newSet);
                    for (var i = 0; i < item.Count; i++)
                        ((WorkoutLogSerieModelRef)item[i]).SetNo = $"SET {i + 1}/{item.Count}";
                    if (item.First().IsWarmups)
                    {
                        var warmString = item.Where(l => l.IsWarmups).ToList().Count < 2 ? "warm-up" : "warm-ups";
                        ((WorkoutLogSerieModelRef)item.First()).SetTitle = $"{item.Where(l => l.IsWarmups).ToList().Count} {warmString}, {item.Where(l => !l.IsWarmups).ToList().Count} work sets\nLet's warm up:";
                    }
                    ScrollToActiveSet(newSet, item);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Task.Delay(200);
                        ExerciseListView.ScrollTo(models, ScrollToPosition.MakeVisible, true);
                    });
                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                    break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void DeleteSetMessageTapped(WorkoutLogSerieModelRef models)
        {
            try
            {
                if (contextMenuStack != null)
                    HideContextButton();

                models.IsFinished = false;
                models.IsEditing = true;
                models.IsNext = true;
                //if (Timer.Instance.State != "RUNNING" && !Device.RuntimePlatform.Equals(Device.iOS))
                //    Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = models, IsFinishExercise = false }, "SaveSetMessage");
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                        continue;
                    models.IsFinished = false;

                    foreach (WorkoutLogSerieModelRef sets in item)
                    {
                        sets.IsEditing = false;
                        if (sets.IsNext)
                        {
                            sets.IsNext = false;
                            //if (Device.RuntimePlatform.Equals(Device.iOS))
                            //    sets.IsSizeChanged = !sets.IsSizeChanged;
                        }
                        sets.IsNext = false;
                    }
                    models.IsEditing = false;
                    models.IsNext = true;
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        models.IsSizeChanged = !models.IsSizeChanged;

                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        ExerciseListView.ItemsSource = exerciseItems;
                    break;
                }
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //ExerciseListView.BeginRefresh();
                        //ExerciseListView.EndRefresh();
                    });
                }
                return;

                //TO rmeove complete set
                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                        continue;
                    models.IsFinished = false;
                    if (item.Count == 1)
                        break;
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(item.Id))
                    {
                        var listOfSets = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[item.Id];
                        listOfSets.Remove(models);
                    }

                    item.Remove(models);
                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UpdateSetTitleMessageTapped(WorkoutLogSerieModelRef models)
        {
            try
            {
                foreach (var item in exerciseItems)
                {
                    try
                    {

                        if (!item.Contains(models))
                            continue;

                        var index = item.IndexOf(models);
                        bool isRepsChanged = false;
                        if (item.Count > (index + 1))
                        {
                            ((WorkoutLogSerieModelRef)item[index + 1]).SetTitle = models.RIR == 0 ? "Ok, that was hard. \nNow let's try:" : "Got it! Now let's try:";
                            if (item.IsUnilateral && !item.IsFirstSide)
                                continue;
                            //if (models.RIR==0 && !item.IsBodyweight)
                            //{
                            //    if (!item.IsPyramid)
                            //    { 
                            //        for (int i = index+1; i < item.Count; i++)
                            //        {
                            //            var rec = (WorkoutLogSerieModelRef)item[i];
                            //            decimal weight = RecoComputation.RoundToNearestIncrement(rec.Weight.Kg - (rec.Weight.Kg * (decimal)0.1), item.RecoModel.Increments == null ? (decimal)1.0 : item.RecoModel.Increments.Kg, item.RecoModel.Min?.Kg, item.RecoModel.Max?.Kg);
                            //            decimal oldWeight = RecoComputation.RoundToNearestIncrement(rec.Weight.Kg, item.RecoModel.Increments == null ? (decimal)1.0 : item.RecoModel.Increments.Kg, item.RecoModel.Min?.Kg, item.RecoModel.Max?.Kg);
                            //            if (oldWeight == weight && i==index+1)
                            //            {
                            //                isRepsChanged = true;
                            //            }
                            //            if (isRepsChanged)
                            //                ((WorkoutLogSerieModelRef)item[i]).Reps -= ((WorkoutLogSerieModelRef)item[i]).Reps > 1 ? 1 : 0;
                            //            else
                            //                ((WorkoutLogSerieModelRef)item[i]).Weight = new MultiUnityWeight(weight, "kg");

                            //            if (i == index + 1)
                            //            {

                            //                popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1} ", rec.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false,rec.EquipmentId == 4);
                            //                App.PCWeight = Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                            //                popup?.WeightCalculateAgain();
                            //            }
                            //        }
                            //    }
                            //    else
                            //    {
                            //        for (int j = index+1; j < item.Count; j++)
                            //        {
                            //            //if (item[i] == models)
                            //            //    continue;
                            //            WorkoutLogSerieModelRef updatingItem = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[j];

                            //            if (!updatingItem.IsFinished && !updatingItem.IsWarmups && !updatingItem.IsFirstWorkSet)
                            //            {
                            //                var count = item.Where(x => x.IsWarmups == true).Count();
                            //                var rec = updatingItem;

                            //                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

                            //                decimal weight = RecoComputation.RoundToNearestIncrement(isKg ? rec.Weight.Kg - (rec.Weight.Kg * (decimal)0.1) : rec.Weight.Lb - (rec.Weight.Lb * (decimal)0.1), item.RecoModel.Increments == null ? (decimal)1.0 : isKg ? item.RecoModel.Increments.Kg : TruncateDecimal(item.RecoModel.Increments.Lb, 5), isKg ? item.RecoModel.Min?.Kg : item.RecoModel.Min?.Lb, isKg ? item.RecoModel.Max?.Kg : item.RecoModel.Max?.Lb);

                            //                var newWeight = new MultiUnityWeight(weight, isKg ? "kg" : "lb");
                            //                var oldWeight = isKg ? updatingItem.Weight.Kg : updatingItem.Weight.Lb;
                            //                if (oldWeight == weight && j == index + 1)
                            //                {
                            //                    isRepsChanged = true;
                            //                }
                            //                if (isRepsChanged)
                            //                    updatingItem.Reps -= updatingItem.Reps > 1 ? 1 : 0;
                            //                else                                                
                            //                    updatingItem.Weight = newWeight;
                            //                if (j == index + 1)
                            //                {

                            //popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1} ", updatingItem.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", updatingItem.IsFirstWorkSet && updatingItem.IsMaxChallenge ? "Max" : updatingItem.Reps.ToString()).ReplaceWithDot(), updatingItem.IsFirstWorkSet && updatingItem.IsMaxChallenge ? true : false, updatingItem.EquipmentId == 4);
                            //App.PCWeight = Convert.ToDecimal(updatingItem.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                            //popup?.WeightCalculateAgain();
                            //                }

                            //            }
                            //        }
                            //    }
                            //}
                            //else
                            if (models.RIR == 0)
                            {
                                for (int i = index + 1; i < item.Count; i++)
                                {
                                    var rec = (WorkoutLogSerieModelRef)item[i];

                                    int reps = (int)Math.Floor(rec.Reps - (rec.Reps * (decimal)0.1));
                                    if (reps < 1)
                                        reps = 1;
                                    ((WorkoutLogSerieModelRef)item[i]).Reps = reps;
                                    if (i == index + 1)
                                    {
                                        if (!item.IsBodyweight)
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1} ", rec.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false, rec.EquipmentId == 4);
                                            App.PCWeight = Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                            popup?.WeightCalculateAgain();
                                        }
                                        else if (rec.Id == 16508)
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{1} x {0} ", "Fast", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false);
                                        }
                                        else if (rec.BodypartId == 12)
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{1} x {0} ", "Cooldown", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false);
                                        }
                                        else if (rec.Id >= 16897 && rec.Id <= 16907 || rec.Id == 14279 || rec.Id >= 21508 && rec.Id <= 21514)//
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{0} x {1} ", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString(), "Bands").ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false, rec.EquipmentId == 4);
                                        }
                                        else
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{1} x {0} ", "Body", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false, rec.EquipmentId == 4);
                                        }
                                    }
                                }
                            }
                            if (item.IsNormalSets == false && item.IsPyramid == false && (models.RIR == 4 || models.RIR == 3))
                            {
                                for (int i = index + 1; i < item.Count; i++)
                                {
                                    var rec = (WorkoutLogSerieModelRef)item[i];
                                    if (rec.IsFinished)
                                        continue;
                                    ((WorkoutLogSerieModelRef)item[i]).Reps += 1;
                                    if (i == index + 1)
                                    {
                                        if (item.IsBodyweight)
                                        {
                                            if (rec.Id == 16508)
                                            {
                                                popup?.SetTimerRepsSets(string.Format("{1} x {0} ", "Fast", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false);
                                            }
                                            else if (rec.BodypartId == 12)
                                            {
                                                popup?.SetTimerRepsSets(string.Format("{1} x {0} ", "Cooldown", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false);
                                            }
                                            else if (rec.Id >= 16897 && rec.Id <= 16907 || rec.Id == 14279 || rec.Id >= 21508 && rec.Id <= 21514)//
                                            {
                                                popup?.SetTimerRepsSets(string.Format("{0} x {1} ", rec.Reps.ToString(), "Bands").ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false);
                                            }
                                            else
                                            {
                                                popup?.SetTimerRepsSets(string.Format("{1} x {0} ", "Body", rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false);
                                            }
                                        }
                                        else
                                            popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1} ", rec.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", rec.IsFirstWorkSet && rec.IsMaxChallenge ? "Max" : rec.Reps.ToString()).ReplaceWithDot(), rec.IsFirstWorkSet && rec.IsMaxChallenge ? true : false, rec.EquipmentId == 4);
                                        App.PCWeight = Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)

                    {

                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void UpdatedQuickModeList(WorkoutLogSerieModelRef models)
        {
            foreach (var item in exerciseItems)
            {
                try
                {

                    if (!item.Contains(models))
                        continue;

                    var index = item.IndexOf(models);
                    bool isRepsChanged = false;
                    if (item.Count > (index + 1))
                    {
                        ((WorkoutLogSerieModelRef)item[index + 1]).SetTitle = models.RIR == 0 ? "Ok, that was hard. \nNow let's try:" : "Got it! Now let's try:";
                        if (item.IsUnilateral && !item.IsFirstSide)
                            continue;

                        if (models.RIR == 0)
                        {
                            if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true")
                            {


                                if (item.Where(x => x.IsWarmups == false).Count() > 2)
                                {
                                    List<WorkoutLogSerieModelRef> lstItems = new List<WorkoutLogSerieModelRef>();
                                    int cnt = 0;
                                    foreach (WorkoutLogSerieModelRef it in item)
                                    {
                                        it.IsNextBackOffSet = false;
                                        if (!it.IsWarmups)
                                            cnt += 1;
                                        lstItems.Add(it);
                                        if (cnt == 2)
                                        {
                                            it.IsLastSet = true;
                                            break;
                                        }
                                    }
                                    item.Clear();
                                    foreach (var i in lstItems)
                                    {
                                        i.IsNextBackOffSet = false;
                                        if (lstItems.Last() == i)
                                            i.IsLastSet = true;
                                        item.Add(i);
                                    }
                                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[item.Id] = new ObservableCollection<WorkoutLogSerieModelRef>(lstItems);

                                }
                                if (item.RecoModel.IsNormalSets && !item.RecoModel.IsPyramid)
                                    item.RecoModel.Series = 2;
                                else if (!item.RecoModel.IsNormalSets && !item.RecoModel.IsPyramid)
                                {
                                    item.RecoModel.Series = 1;
                                    item.RecoModel.NbRepsPauses = 1;

                                }

                                if (CurrentLog.Instance.RecommendationsByExercise.ContainsKey(CurrentLog.Instance.ExerciseLog.Exercice.Id))
                                    CurrentLog.Instance.RecommendationsByExercise[item.Id] = item.RecoModel;
                                else
                                    CurrentLog.Instance.RecommendationsByExercise.Add(item.Id, item.RecoModel);

                                item.IsNextExercise = false;
                                FetchReco(item);
                            }
                        }

                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        private async void SaveSetFromWatchMessageTapped(PhoneToWatchModel models)
        {
            try
            {

                if (models.WatchMessageType == WatchMessageType.EndTimer)
                {
                    if (PopupNavigation.PopupStack.Count > 0)
                        PopupNavigation.Instance.PopAllAsync();
                    return;
                }
                if (models.WatchMessageType == WatchMessageType.FinishSaveWorkout)
                {
                    SavingExcercise();
                    return;
                }
                var model = exerciseItems.FirstOrDefault(m => m.Id == models.Id);
                if (models.WatchMessageType == WatchMessageType.FinishExercise)
                {
                    PushToDataServer(model);
                    return;
                }
                if (models.WatchMessageType == WatchMessageType.FinishSide1)
                {
                    Finished_Clicked(model);
                    return;
                }
                if (models.WatchMessageType == WatchMessageType.RIR)
                {
                    foreach (var set in model.Where(x => x.IsWarmups == false))
                    {
                        set.RIR = models.RIR;
                    }
                    return;
                }
                foreach (WorkoutLogSerieModelRef item in model)
                {
                    if (item.IsFinished)
                        continue;

                    MessagingCenter.Send<FinishSetReceivedFromWatchOS>(new FinishSetReceivedFromWatchOS() { model = item, WatchMessageType = models.WatchMessageType }, "FinishSetReceivedFromWatchOS");
                    break;
                }

            }
            catch (Exception ex)
            {

            }
        }
        private async void SaveSetMessageTapped(WorkoutLogSerieModelRef models, bool IsFinished)
        {
            try
            {
                if (contextMenuStack != null)
                    HideContextButton();
                if (Device.RuntimePlatform.Equals(Device.Android))
                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;

                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                    {
                        foreach (WorkoutLogSerieModelRef subItem in item)
                        {
                            subItem.IsActive = false;
                        }
                        continue;
                    }

                    if (IsFinished)
                    {
                        //Called SaveSet
                        if (item.IsFinished)
                        {
                            Finished_Clicked(item);
                            return;
                        }
                        if (!((WorkoutLogSerieModelRef)item.LastOrDefault()).IsFirstSetFinished)
                            PushToDataServer(item);
                        else
                            Finished_Clicked(item);
                        return;
                    }
                    if (item.FirstOrDefault() != null)
                    {
                        WorkoutLogSerieModelRef header = (WorkoutLogSerieModelRef)item.FirstOrDefault();
                        WorkoutLogSerieModelRef last = (WorkoutLogSerieModelRef)item.LastOrDefault();
                        last.IsFirstSetFinished = header.IsFinished;

                    }
                    if (models.IsWarmups)
                    {
                        foreach (WorkoutLogSerieModelRef set in item)
                        {
                            var warmUponeRM = ComputeOneRM(set.Weight.Kg, set.Reps);
                            var firstSet = (WorkoutLogSerieModelRef)item.ElementAt(item.Count(x => x.IsWarmups));
                            var computeRM = ComputeOneRM(firstSet.Weight.Kg, firstSet.Reps);

                            if (!firstSet.IsFinished && warmUponeRM > computeRM && !item.IsReversePyramid)
                            {
                                foreach (WorkoutLogSerieModelRef it in item.Where(x => x.IsWarmups))
                                {
                                    if (!it.IsFinished)
                                    {
                                        it.Reps = set.Reps;
                                        it.Weight = set.Weight;
                                    }
                                }
                                firstSet.Weight = set.Weight;
                                firstSet.Reps = set.Reps;
                                UpdateWeoghtRepsMessageTapped(firstSet);
                            }
                            if (set.IsLastWarmupSet)
                                break;
                        }

                    }
                    if (models.IsLastSet && models.IsUnilateral && models.IsFirstSide)
                    {
                        WorkoutLogSerieModelRef first = (DrMuscle.Layout.WorkoutLogSerieModelRef)item.First();
                        App.PCWeight = Convert.ToDecimal(first.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                        try
                        {
                            if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen") == null)
                                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");

                            first.IsActive = true;

                        }
                        catch (Exception ex)
                        {

                        }

                        if (item.Count > 1)
                        {
                            if (Device.RuntimePlatform.Equals(Device.Android))
                            {
                                var index = item.IndexOf(models);
                                WorkoutLogSerieModelRef before = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[index > 1 ? index - 1 : index];
                                ScrollToActiveSet(before, item);
                            }
                        }

                        ScrollToActiveSet(models, item);
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Task.Delay(200);
                            ExerciseListView.ScrollTo(models, ScrollToPosition.MakeVisible, true);
                        });
                    }
                    else if (models.IsLastSet)
                    {
                        if (item.Count > 1)
                        {
                            if (Device.RuntimePlatform.Equals(Device.Android))
                            {
                                var index = item.IndexOf(models);
                                WorkoutLogSerieModelRef before = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[index > 1 ? index - 1 : index];
                                ScrollToActiveSet(before, item);
                            }
                        }

                        ScrollToActiveSet(models, item);
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Task.Delay(200);
                            ExerciseListView.ScrollTo(models, ScrollToPosition.MakeVisible, true);
                        });
                    }

                    bool isAllSetfinished = true;
                    foreach (WorkoutLogSerieModelRef logSerieModel in item)
                    {
                        if (logSerieModel.IsFinished)
                            continue;
                        isAllSetfinished = false;
                        if (!logSerieModel.IsNext)
                        {
                            logSerieModel.IsNext = true;
                            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSetBehind, SetModel = logSerieModel, Label = item.Label }, "SendWatchMessage");
                            App.PCWeight = Convert.ToDecimal(logSerieModel.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                            try
                            {
                                if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen") == null)
                                    LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");
                                if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                                {
                                    _backOffSet = logSerieModel;
                                    popup = new TimerPopup(item.IsPlate);
                                    popup.HidePopup += HidePopup;
                                    popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                                    popup.popupTitle = "";
                                    if (logSerieModel.IsBodyweight)
                                    {
                                        if (logSerieModel.Id == 16508)
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{0} x {1} ", logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? "Max" : logSerieModel.Reps.ToString(), logSerieModel.IsWarmups ? "Brisk" : "Fast").ReplaceWithDot(), logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? true : false);
                                        }
                                        else if (item.BodyPartId == 12)
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{0} x {1} ", logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? "Max" : logSerieModel.Reps.ToString(), logSerieModel.IsWarmups ? "Brisk" : logSerieModel.IsFirstWorkSet ? "Fast" : "Cooldown").ReplaceWithDot(), logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? true : false);
                                            TimerBased = false;
                                            if (logSerieModel.IsWarmups == false)
                                                popup.popupTitle = "Work";
                                        }
                                        else if (logSerieModel.Id >= 16897 && logSerieModel.Id <= 16907 || logSerieModel.Id == 14279 || logSerieModel.Id >= 21508 && logSerieModel.Id <= 21514)// 
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{0} x {1} ", logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? "Max" : logSerieModel.Reps.ToString(), "Bands").ReplaceWithDot(), logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? true : false);
                                        }
                                        else
                                        {
                                            popup?.SetTimerRepsSets(string.Format("{0} x Body ", logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? "Max" : logSerieModel.Reps.ToString()).ReplaceWithDot(), logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? true : false);
                                        }
                                    }
                                    else
                                    {
                                        popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1} ", logSerieModel.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? "Max" : logSerieModel.Reps.ToString()).ReplaceWithDot(), logSerieModel.IsFirstWorkSet && logSerieModel.IsMaxChallenge ? true : false, logSerieModel.EquipmentId == 4);
                                    }
                                    popup.SetTimerText();
                                    if (logSerieModel.IsFirstWorkSet && item.RecoModel != null && item.RecoModel.FirstWorkSetWeight != null && item.RecoModel.FirstWorkSetWeight.Entered != 0)
                                    {
                                        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                                        var lastOneRM = ComputeOneRM(new MultiUnityWeight(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, isKg ? "kg" : "lb").Kg, item.RecoModel.FirstWorkSetReps);
                                        var currentRM = ComputeOneRM(new MultiUnityWeight(isKg ? logSerieModel.Weight.Kg : logSerieModel.Weight.Lb, isKg ? "kg" : "lb").Kg, logSerieModel.Reps);
                                        var worksets = string.Format("{0:0.##} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");

                                        if (logSerieModel.IsBodyweight)
                                        {
                                            worksets = "body";
                                            if (logSerieModel.Id == 16508)
                                            {
                                                worksets = "fast";
                                            }
                                            else if (logSerieModel.BodypartId == 12)
                                            {
                                                worksets = "fast";
                                                TimerBased = false;
                                                popup.popupTitle = "Work";
                                            }
                                            if (logSerieModel.Id >= 16897 && logSerieModel.Id <= 16907 || logSerieModel.Id == 14279 || logSerieModel.Id >= 21508 && logSerieModel.Id <= 21514)
                                            {
                                                worksets = "bands";
                                            }
                                        }
                                        if (currentRM != 0)
                                        {
                                            var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                                            popup.SetLastTimeText(string.Format("{0}{1:0.0}%:", percentage >= 0 ? "+" : "", percentage), string.Format("Last time: {0} x {1}", item.RecoModel.FirstWorkSetReps, worksets));
                                        }

                                    }
                                    if (item.IsTimeBased)
                                    {
                                        timeRemain = Convert.ToString(logSerieModel.Reps);
                                        TimerBased = true;
                                        if (logSerieModel.BodypartId == 12 && logSerieModel.Id != 16508 && logSerieModel.IsWarmups == false)
                                            TimerBased = false;
                                    }
                                    else
                                        TimerBased = false;
                                    if (Config.ShowSupersetPopup == 0 && !App.IsSupersetPopup || _superSetRunning)
                                        ;
                                    else
                                        PopupNavigation.Instance.PushAsync(popup);

                                }
                                logSerieModel.IsActive = true;

                                if (item.IndexOf(logSerieModel) > 0)
                                {

                                    // ScrollToActiveSet(old, item);

                                    if (Device.RuntimePlatform.Equals(Device.Android))
                                    {
                                        var index = item.IndexOf(logSerieModel);
                                        WorkoutLogSerieModelRef old = (WorkoutLogSerieModelRef)item[index - 1];
                                        Device.BeginInvokeOnMainThread(async () =>
                                        {
                                            ExerciseListView.ScrollTo(old, ScrollToPosition.End, false);
                                            ScrollToActiveSet(logSerieModel, item);

                                            await Task.Delay(200);
                                            ExerciseListView.ScrollTo(logSerieModel, ScrollToPosition.MakeVisible, true);

                                        });
                                    }
                                    else
                                        ScrollToActiveSet(logSerieModel, item);


                                }
                                else
                                {
                                    ScrollToActiveSet(logSerieModel, item);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            if (Device.RuntimePlatform.Equals(Device.iOS))
                                ExerciseListView.ScrollTo(logSerieModel, ScrollToPosition.MakeVisible, true);
                            //else
                            //    ExerciseListView.ScrollTo(logSerieModel, ScrollToPosition.MakeVisible, true);

                            break;
                        }
                    }
                    //
                    if (_areExercisesUnfnished)
                        item.IsFirstSide = false;
                    if (isAllSetfinished && item.IsFirstSide == false && !item.IsReversePyramid)
                    {
                        if (!App.IsConnectedToWatch)
                        {
                            ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                            {
                                Title = $"All sets done—finish exercise?",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Finish exercise",
                                CancelText = AppResources.Cancel,

                            };
                            var x = await UserDialogs.Instance.ConfirmAsync(ShowRIRPopUp);
                            if (x)
                            {
                                PushToDataServer(item);
                            }
                        }
                        else
                            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.FinishExercise, SetModel = models }, "SendWatchMessage");
                    }

                    if (isAllSetfinished && item.IsFirstSide == true)
                    {
                        if (!App.IsConnectedToWatch)
                        {
                            ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                            {
                                Title = $"All sets done for side 1—start side 2?",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Side 2",
                                CancelText = AppResources.Cancel,

                            };
                            var x = await UserDialogs.Instance.ConfirmAsync(ShowRIRPopUp);
                            if (x)
                            {
                                FinishedSide1_Clicked(item);
                            }
                        }
                        else
                            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.FinishExercise, SetModel = models }, "SendWatchMessage");
                    }
                    if (isAllSetfinished && item.IsFirstSide == true && App.IsConnectedToWatch)
                        MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.FinishSide1, SetModel = models }, "SendWatchMessage");
                }
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;

                try
                {
                    ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef;
                    ((App)Application.Current).WorkoutLogContext.SaveContexts();
                }
                catch (Exception ex)
                {

                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (DrMuscle.Effects.TooltipEffect.GetHasShowTooltip(StackHeader))
                    StackHeader.Effects.Clear();
                if (_superSetRunning)
                {
                    _superSetRunning = false;
                    var ind = 0;
                    foreach (var item in exerciseItems)
                    {
                        if (item.Contains(models))
                        {
                            ind = exerciseItems.IndexOf(item);
                            break;
                        }
                    }

                    //if (exerciseItems.Count - 2 != ind)
                    //{
                    //if (Device.RuntimePlatform.Equals(Device.iOS))
                    //{
                    //    ExerciseListView.ItemPosition = ind - 1;
                    //    ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;
                    //    ExerciseListView.ScrollTo(exerciseItems[ind-1].First(), ScrollToPosition.Start, false);

                    //    ScrollToActiveSet((WorkoutLogSerieModelRef)exerciseItems[ind - 1].First(), exerciseItems[ind - 1]);
                    //}
                    //else
                    //{
                    //    ExerciseListView.ItemPosition = ind - 1;
                    //    ExerciseListView.ScrollTo(exerciseItems[ind].First(), ScrollToPosition.Start, false);
                    //    ScrollToActiveSet((WorkoutLogSerieModelRef)exerciseItems[ind - 1].First(), exerciseItems[ind - 1]);
                    //    ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;
                    //}
                    try
                    {

                        await Task.Delay(1500);
                        if (exerciseItems.Count > 0)
                        {
                            ((WorkoutLogSerieModelRef)exerciseItems[ind][0]).ShowSuperSet3 = false;
                            ((WorkoutLogSerieModelRef)exerciseItems[ind][1]).ShowSuperSet3 = false;
                            ((WorkoutLogSerieModelRef)exerciseItems[ind][1]).ShowSuperSet2 = false;

                            if (((WorkoutLogSerieModelRef)exerciseItems[ind][1]).IsFinished == false)
                                DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(StackHeader, true);
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                    //}
                }
                try
                {

                    if (Config.ShowSupersetPopup == 0 && !App.IsSupersetPopup)
                    {

                        App.IsSupersetPopup = true;
                        bool isAccepted = false;
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                        {
                            isAccepted = await DisplayAlert($"Save time", $"Alternate between exercises to save time.", AppResources.GotIt, AppResources.RemindMe);
                        }
                        else
                        {
                            isAccepted = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                            {
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                Message = $"Alternate between exercises to save time.",
                                Title = $"Save time",
                                OkText = AppResources.GotIt,
                                CancelText = AppResources.RemindMe
                            });

                        }
                        if (isAccepted)
                            Config.ShowSupersetPopup = 3;
                        else
                            Config.ShowSupersetPopup = 0;
                        int ind = 0, index = 0;
                        foreach (var item in exerciseItems.Where(x => x.Count() > 0))
                        {
                            if (item.Count() > 1)
                            {
                                index = item.Count();
                                ind = exerciseItems.IndexOf(item);
                                break;
                            }
                        }
                        if (exerciseItems.Count - 2 != ind)
                        {
                            if (Device.RuntimePlatform.Equals(Device.iOS))
                            {
                                ExerciseListView.ItemPosition = ind + 1;
                                ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;
                                ExerciseListView.ScrollTo(exerciseItems[ind].Last(), ScrollToPosition.Start, false);

                            }
                            else
                            {
                                ExerciseListView.ItemPosition = index + ind + 1;
                                ExerciseListView.ScrollTo(exerciseItems[ind].Last(), ScrollToPosition.Start, false);
                                ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;
                            }

                            await Task.Delay(800);

                            TooltipEffect.SetPosition(vHeaders[exerciseItems[ind + 1].Id], TooltipPosition.Bottom);
                            TooltipEffect.SetBackgroundColor(vHeaders[exerciseItems[ind + 1].Id], AppThemeConstants.BlueColor);
                            TooltipEffect.SetTextColor(vHeaders[exerciseItems[ind + 1].Id], Color.White);
                            TooltipEffect.SetText(vHeaders[exerciseItems[ind + 1].Id], $"Tap on another exercise");
                            TooltipEffect.SetHasTooltip(vHeaders[exerciseItems[ind + 1].Id], true);
                            TooltipEffect.SetHasShowTooltip(vHeaders[exerciseItems[ind + 1].Id], true);

                            _superSetRunning = true;
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }

        public async void HidePopup(object sender, EventArgs e)
        {

            if (Config.ShowBackoffPopup && App.IsShowBackOffPopup)
            {
                if (_isHideTimerCall)
                    return;
                _isHideTimerCall = true;
                App.WelcomeTooltop[0] = true;
                App.IsShowBackOffPopup = false;
                App.IsShowBackOffTooltip = true;
                BackoffSetCallout();
            }
        }

        private async void AskForFinishPyramidExercise(WorkoutLogSerieModelRef models)
        {
            try
            {
                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                    {
                        foreach (WorkoutLogSerieModelRef subItem in item)
                        {
                            subItem.IsActive = false;
                        }
                        continue;
                    }
                    if (!item.IsReversePyramid)
                        break;
                    if (!App.IsConnectedToWatch)
                    {
                        ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                        {
                            Title = $"All sets done—finish exercise?",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = "Finish exercise",
                            CancelText = AppResources.Cancel,

                        };
                        var x = await UserDialogs.Instance.ConfirmAsync(ShowRIRPopUp);
                        if (x)
                        {
                            PushToDataServer(item);
                        }
                    }
                    else
                        MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.FinishExercise, SetModel = models }, "SendWatchMessage");
                }
            }
            catch (Exception)
            {

            }
        }

        public async void BackoffSetCallout()
        {

            if (_backOffSet != null)
            {
                _backOffSet.ISbackOffSetTooltip = true;
                await Task.Delay(4000);
                _backOffSet.ISbackOffSetTooltip = false;
            }
            else
            {
                WorkoutLogSerieModelRef i = null;
                foreach (WorkoutLogSerieModelRef item in exerciseItems.First())
                {
                    if (item.IsFinished)
                        continue;
                    i = item;
                    item.ISbackOffSetTooltip = true;
                    break;

                }
                await Task.Delay(4000);
                if (i != null)
                    i.ISbackOffSetTooltip = false;
            }
        }

        public override async void OnBeforeShow()
        {
            base.OnBeforeShow();
            try
            {
                if (App.IsIntroBack)
                    return;
                vHeaders = new Dictionary<long, View>();
                UpdateExerciseList();
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                isAppear = false;
                try
                {
                    DependencyService.Get<IKeyboardHelper>().HideKeyboard();
                }
                catch (Exception ex)
                {

                }

                //
            }
            catch (Exception ex)
            {

            }
        }

        private bool IsFromMePage()
        {
            var isMePage = false;
            foreach (var item in Navigation.NavigationStack)
            {

                if (item is DrMuscle.Screens.User.SettingsPage)
                {
                    if (Navigation.NavigationStack.Last() == item)
                        continue;
                    isMePage = true;
                    break;
                }
            }
            return isMePage;
        }
        //DrMusclePage page;
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            NavigationPage.SetHasNavigationBar(this, false);
            //page = PagesFactory.GetPage<IntroPage2>();
        }


        private async void FinishedSide1_Clicked(ExerciseWorkSetsModel model)
        {
            try
            {

                if (model.IsFirstSide && model.IsUnilateral)
                {

                    model.IsFirstSide = false;
                    List<WorkoutLogSerieModelRef> setList = new List<WorkoutLogSerieModelRef>();
                    foreach (WorkoutLogSerieModelRef item in model)
                    {
                        item.IsFirstSide = false;
                        item.IsFinished = false;
                        item.IsNext = false;
                        item.IsActive = false;
                        item.IsEditing = false;
                        item.IsFirstSetFinished = false;
                        setList.Add(item);
                    }
                    WorkoutLogSerieModelRef workout = ((WorkoutLogSerieModelRef)model.First());
                    workout.IsNext = true;
                    workout.IsActive = true;
                    App.PCWeight = Convert.ToDecimal(workout.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                    MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout, Label = model.Label }, "SendWatchMessage");
                    if (LocalDBManager.Instance.GetDBSetting("timer_autoset").Value == "true")
                    {

                        if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true" && Timer.Instance.State == "RUNNING")
                        {

                            popup = new TimerPopup(model.IsPlate);
                            popup.RemainingSeconds = Convert.ToString(Timer.Instance.Remaining);
                            popup.popupTitle = "";
                            if (workout.IsBodyweight)
                            {

                                popup?.SetTimerRepsSets(string.Format("{0} x Body", workout.IsFirstWorkSet && workout.IsMaxChallenge ? "Max" : workout.Reps.ToString()).ReplaceWithDot(), workout.IsFirstWorkSet && workout.IsMaxChallenge ? true : false);
                                popup?.SetReadyForTitle();
                            }
                            else
                            {

                                popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1} ", workout.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", workout.IsFirstWorkSet && workout.IsMaxChallenge ? "Max" : workout.Reps.ToString()).ReplaceWithDot(), workout.IsFirstWorkSet && workout.IsMaxChallenge ? true : false, workout.EquipmentId == 4);
                                popup?.SetReadyForTitle();
                            }
                            popup.SetTimerText();

                            TimerBased = false;
                            PopupNavigation.Instance.PushAsync(popup);

                        }
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", App.globalTime.ToString());
                        //timeRemain = TimerEntry;
                    }



                    ScrollToSnap(setList, model);
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                    {

                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ExerciseListView.ScrollTo(setList.First(), ScrollToPosition.Start, false);
                        });
                    }
                    if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value != "true" && !App.IsConnectedToWatch)
                    {

                    }
                    try
                    {
                        ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef;
                        ((App)Application.Current).WorkoutLogContext.SaveContexts();
                    }
                    catch (Exception ex)
                    {

                    }

                    return;
                }

            }
            catch (Exception ex)
            {

            }
        }

        private async void Finished_Clicked(ExerciseWorkSetsModel model)
        {
            try
            {

                if (model.IsFirstSide && model.IsUnilateral)
                {

                    model.IsFirstSide = false;
                    List<WorkoutLogSerieModelRef> setList = new List<WorkoutLogSerieModelRef>();
                    foreach (WorkoutLogSerieModelRef item in model)
                    {
                        item.IsFirstSide = false;
                        item.IsFinished = false;
                        item.IsNext = false;
                        item.IsActive = false;
                        item.IsEditing = false;
                        item.IsFirstSetFinished = false;
                        setList.Add(item);
                    }
                    WorkoutLogSerieModelRef workout = ((WorkoutLogSerieModelRef)model.First());
                    workout.IsNext = true;
                    workout.IsActive = true;
                    App.PCWeight = Convert.ToDecimal(workout.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                    MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout, Label = model.Label }, "SendWatchMessage");
                    if (LocalDBManager.Instance.GetDBSetting("timer_autoset").Value == "true")
                    {

                        if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true" && Timer.Instance.State == "RUNNING")
                        {

                            popup = new TimerPopup(model.IsPlate);
                            popup.RemainingSeconds = Convert.ToString(Timer.Instance.Remaining);
                            popup.popupTitle = "";
                            if (workout.IsBodyweight)
                            {

                                popup?.SetTimerRepsSets(string.Format("{0} x Body", workout.IsFirstWorkSet && workout.IsMaxChallenge ? "Max" : workout.Reps.ToString()).ReplaceWithDot(), workout.IsFirstWorkSet && workout.IsMaxChallenge ? true : false);
                                popup?.SetReadyForTitle();
                            }
                            else
                            {

                                popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1} ", workout.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", workout.IsFirstWorkSet && workout.IsMaxChallenge ? "Max" : workout.Reps.ToString()).ReplaceWithDot(), workout.IsFirstWorkSet && workout.IsMaxChallenge ? true : false, workout.EquipmentId == 4);
                                popup?.SetReadyForTitle();
                            }
                            popup.SetTimerText();
                            //if (item.IsTimeBased)
                            //{
                            //    timeRemain = Convert.ToString(first.Reps);
                            //    TimerBased = true;
                            //}
                            //else
                            TimerBased = false;
                            PopupNavigation.Instance.PushAsync(popup);

                        }
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", App.globalTime.ToString());
                        //timeRemain = TimerEntry;
                    }
                    //}
                    //Timer.Instance.StopTimer();
                    //Timer.Instance.stopRequest = false;
                    //Timer.Instance.StartTimer();
                    ////Open fullscreen timer
                    //if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                    //{

                    //    cellhepopup = new TimerPopup();
                    //    popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                    //    popup.popupTitle = "";
                    //    popup?.SetTimerRepsSets(string.Format("{0:0.00} {1} x {2} ", workout.WeightDouble.ReplaceWithDot(), LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", workout.Reps).ReplaceWithDot());
                    //    popup.SetTimerText();
                    //    if (model.IsTimeBased)
                    //    {
                    //        timeRemain = Convert.ToString(workout.Reps);
                    //        TimerBased = true;
                    //    }
                    //    else
                    //        TimerBased = false;
                    //    PopupNavigation.Instance.PushAsync(popup);

                    //}



                    ScrollToSnap(setList, model);
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                    {
                        //setList.First().IsSizeChanged = !setList.First().IsSizeChanged;
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ExerciseListView.ScrollTo(setList.First(), ScrollToPosition.Start, false);
                        });
                    }
                    if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value != "true" && !App.IsConnectedToWatch)
                    {
                        //AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                        //{
                        //    Title = "Well done! Now do all sets for side 2",
                        //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        //    OkText = AppResources.Ok,

                        //};
                        //UserDialogs.Instance.Alert(ShowExplainRIRPopUp);

                    }
                    try
                    {
                        ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef;
                        ((App)Application.Current).WorkoutLogContext.SaveContexts();
                    }
                    catch (Exception ex)
                    {

                    }
                    //if (Config.ShowAllSetPopup == false)
                    //{
                    //    if (App.ShowAllSetPopup)
                    //        return;
                    //    App.ShowAllSetPopup = true;
                    //    ConfirmConfig ShowWelcomePopUp4 = new ConfirmConfig()
                    //    {

                    //        Title = "Well done! Now do all sets for side 2",
                    //        //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    //        OkText = AppResources.GotIt,
                    //        CancelText = AppResources.RemindMe,
                    //        OnAction = async (bool ok) =>
                    //        {
                    //            if (ok)
                    //            {
                    //                Config.ShowAllSetPopup = true;
                    //            }
                    //            else
                    //            {
                    //                Config.ShowAllSetPopup = false;
                    //            }
                    //        }
                    //    };
                    //    await Task.Delay(100);
                    //    UserDialogs.Instance.Confirm(ShowWelcomePopUp4);
                    //}
                    return;
                }

                if (model.IsFinished)
                {
                    foreach (WorkoutLogSerieModelRef item in model)
                    {

                        var newWorkOutLog = new WorkoutLogSerieModel()
                        {
                            Id = item.Id,
                            Reps = item.Reps,
                            Weight = item.Weight,
                            Exercice = GetExerciseModel(model)
                        };

                    }
                    model.IsNextExercise = false;
                    model.Clear();
                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                    return;
                }
                ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                {
                    Title = $"Save {model.Label}?",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Save",
                    CancelText = AppResources.Cancel,
                    OnAction = async (bool ok) =>
                    {
                        if (ok)
                        {
                            PushToDataServer(model);
                        }
                        else
                        {
                        }
                    }
                };
                UserDialogs.Instance.Confirm(ShowRIRPopUp);

            }
            catch (Exception ex)
            {

            }
        }

        private async void PushToDataServer(ExerciseWorkSetsModel model)
        {
           
        }

        private async void AskForFinishWorkoutOnSkip()
        {
            try
            {

                //Finish workout button
                var allFinished = exerciseItems.Where(x => x.IsFinished == false && x.IsFinishWorkoutExe == false).FirstOrDefault();
                if (allFinished == null && exerciseItems.Count > 0)
                {
                    if (exerciseItems.Where(x => x.IsFinishWorkoutExe == true).FirstOrDefault() != null)
                    {

                        exerciseItems.Where(x => x.IsFinishWorkoutExe == true).FirstOrDefault().IsFinished = true;
                        //Nice workpopup
                        try
                        {
                            ConfirmConfig ShowConfirmPopUp = new ConfirmConfig()
                            {

                                Title = "All exercises finished—save workout?",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Save workout",
                                CancelText = AppResources.Cancel,
                                OnAction = async (bool ok) =>
                                {
                                    if (ok)
                                    {
                                        SavingExcercise();
                                    }
                                }
                            };
                            if (!App.IsConnectedToWatch)
                                UserDialogs.Instance.Confirm(ShowConfirmPopUp);
                            else
                                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.FinishSaveWorkout, SetModel = new WorkoutLogSerieModelRef() }, "SendWatchMessage");

                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void FetchNextExerciseBackgroundData(ExerciseWorkSetsModel m)
        {
            try
            {
                if (m.Id == 0 && m.Id == -1)
                    return;
                NewExerciseLogResponseModel newExercise = null;//await DrMuscleRestClient.Instance.IsNewExerciseWithSessionInfoWithoutLoader(new ExerciceModel() { Id = m.Id });
                if (newExercise != null)
                {
                    if (!newExercise.IsNewExercise)
                    {
                        try
                        {
                            long? workoutId = null;
                            try
                            {
                                //if (!CurrentLog.Instance.IsFromExercise)
                                workoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id;
                            }
                            catch (Exception)
                            {

                            }

                            DateTime? lastLogDate = newExercise.LastLogDate;
                            int? sessionDays = null;


                            string WeightRecommandation;
                            RecommendationModel reco = null;

                            if (lastLogDate != null)
                            {
                                var days = (int)(DateTime.Now - (DateTime)lastLogDate).TotalDays;
                                if (days >= 5 && days <= 9)
                                    sessionDays = days;
                                if (days > 9)
                                {
                                    return;
                                }
                            }

                            string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;

                            string exId = $"{m.Id}";
                            var lastTime = LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle);

                            if (lastTime != null)
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value))
                                    {
                                        var LastRecoPlus1Day = Convert.ToDateTime(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value);
                                        if (LastRecoPlus1Day > DateTime.Now)
                                        {
                                            var recommendation = RecoContext.GetReco("Reco" + exId + setStyle);
                                            if (recommendation != null)
                                                m.RecoModel = recommendation;
                                        }
                                        else
                                        {
                                            //Removed saved set if available
                                            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef != null && CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);

                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Exception is:{ex.ToString()}");
                                }
                            }
                            long? swapedExId = null;
                            try
                            {
                                if (m.IsSwapTarget)
                                {
                                    bool isSwapped = ((App)Application.Current).SwapExericesContexts.Swaps.Any(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.TargetExerciseId == m.Id);
                                    swapedExId = (long)((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.TargetExerciseId == m.Id).SourceExerciseId;
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            if (LocalDBManager.Instance.GetDBSetting("IsPyramid") == null)
                                LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                            bool? isQuick = false;
                            if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                                isQuick = null;
                            else
                                isQuick = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
                            bool IsStrengthPhashe = false;
                            try
                            {

                                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                                int remainingWorkout = 0, totalworkout = 0;
                                if (workouts != null && workouts.GetUserProgramInfoResponseModel != null)
                                {
                                    if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                                    {
                                        totalworkout = workouts.GetUserProgramInfoResponseModel.RecommendedProgram.RequiredWorkoutToLevelUp;
                                        remainingWorkout = workouts.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp != null ? (int)workouts.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp : 0;
                                    }
                                }
                                if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label.Equals(CurrentLog.Instance.CurrentWorkoutTemplate.Label))

                                    IsStrengthPhashe = RecoComputation.IsInStrengthPhase(CurrentLog.Instance.CurrentWorkoutTemplate.Label, int.Parse(string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value) ? "40" : LocalDBManager.Instance.GetDBSetting("Age")?.Value), remainingWorkout, totalworkout);


                            }
                            catch (Exception ex)
                            {

                            }
                            if (m.RecoModel == null)
                            {
                                

                                if (m.RecoModel != null)
                                {
                                    if (m.RecoModel.HistorySet != null)
                                        m.RecoModel.HistorySet.Reverse();
                                }
                            }
                            if (m.RecoModel != null)
                            {
                                if (m.RecoModel.IsDeload)
                                    return;
                                m.RecoModel.IsLightSession = false;
                                if (m.RecoModel.Reps <= 0)
                                    m.RecoModel.Reps = 1;
                                if (m.RecoModel.NbRepsPauses <= 0)
                                    m.RecoModel.NbRepsPauses = 1;

                                RecoContext.SaveContexts("Reco" + exId + setStyle, m.RecoModel);
                                LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + exId + setStyle, DateTime.Now.AddDays(1).ToString());


                            }
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                }

            }
            catch (Exception ex)
            {

            }
        }

        private async void SaveWorkoutButton_Clicked(object sender, EventArgs e)
        {
            try
            {

                ConfirmConfig ShowConfirmPopUp = new ConfirmConfig()
                {
                    Title = $"Save {CurrentLog.Instance.CurrentWorkoutTemplate.Label}?",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Save",
                    CancelText = AppResources.Cancel,
                    OnAction = async (bool ok) =>
                    {
                        if (ok)
                        {
                            SavingExcercise();
                        }
                    }
                };
                UserDialogs.Instance.Confirm(ShowConfirmPopUp);

            }
            catch (Exception ex)
            {

            }
        }

        private bool SaveUnfinishedExercise()
        {
            try
            {
                bool isAnySetfinshed = false;
                ExerciseWorkSetsModel m = null;
                foreach (var item in exerciseItems)
                {
                    if (!item.IsFinished && item.Id != 0)
                    {
                        if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(item.Id))
                        {
                            var sets = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[item.Id];
                            var isfinshed = sets.Where(x => x.IsFinished == true).FirstOrDefault();
                            if (isfinshed != null)
                            {
                                m = item;
                                if (item.Count == 0)
                                {
                                    foreach (var insideset in sets)
                                    {
                                        m.IsFirstSide = false;
                                        m.Add(insideset);

                                    }
                                    CurrentLog.Instance.ExerciseLog = m.FirstOrDefault();
                                    CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(item);
                                }
                                isAnySetfinshed = true;
                                _areExercisesUnfnished = true;

                                break;
                            }
                        }
                    }
                }
                if (isAnySetfinshed)
                {
                    ConfirmConfig finishShowPopUp = new ConfirmConfig()
                    {
                        Message = $"Save {m.Label}?",
                        Title = $"{m.Label} not saved",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Save",
                        CancelText = AppResources.Skip,
                        OnAction = async (bool ok) =>
                        {
                            if (ok)
                            {
                                PushToDataServer(m);
                            }
                            else
                            {
                                _areExercisesUnfnished = false;
                                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                                ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef;
                                ((App)Application.Current).WorkoutLogContext.SaveContexts();
                                SavingExcercise();
                            }
                        }
                    };
                    UserDialogs.Instance.Confirm(finishShowPopUp);
                    return true;
                }
                else
                {
                    _areExercisesUnfnished = false;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        private async void SavingExcercise()
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = AppResources.PleaseCheckInternetConnection,
                        Title = AppResources.ConnectionError
                    });
                    return;
                }
                CurrentLog.Instance.IsWorkoutedOut = false;
                //Check for unifnished exercise
                if (SaveUnfinishedExercise())
                    return;

                try
                {
                    if (exerciseItems.Where(x => x.IsFinished).FirstOrDefault() != null)
                        CurrentLog.Instance.IsWorkoutedOut = true;
                    foreach (var item in exerciseItems)
                    {
                        LocalDBManager.Instance.SetDBSetting($"workout{CurrentLog.Instance.CurrentWorkoutTemplate.Id}exercise{item.Id}", "false");
                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    foreach (var item in exerciseItems)
                    {
                        if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(item.Id)) CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(item.Id);
                        item.Clear();
                    }
                    ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef;
                    ((App)Application.Current).WorkoutLogContext.SaveContexts();
                }
                catch (Exception ex)
                {

                }
                try
                {
                    if (LocalDBManager.Instance.GetDBSetting("OlderQuickMode") != null)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("OlderQuickMode").Value != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("QuickMode", LocalDBManager.Instance.GetDBSetting("OlderQuickMode").Value);
                            LocalDBManager.Instance.SetDBSetting("OlderQuickMode", null);
                            LocalDBManager.Instance.ResetReco();
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                if (LocalDBManager.Instance.GetDBSetting("FinishWorkoutCounter") == null)
                    LocalDBManager.Instance.SetDBSetting("FinishWorkoutCounter", "1");

                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                if (workouts != null)
                {
                    if (workouts.Sets != null)
                    {
                        if (workouts.HistoryExerciseModel != null)
                        {
                            var exerciseModel = workouts.HistoryExerciseModel;
                            if (exerciseModel.TotalWorkoutCompleted < 5)
                            {
                                _firebase.LogEvent($"Finish_workout_{exerciseModel.TotalWorkoutCompleted + 1}", $"{exerciseModel.TotalWorkoutCompleted + 1}");
                            }
                        }
                    }
                }

                //if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("FinishWorkoutCounter").Value))
                //{
                //    var count = int.Parse(LocalDBManager.Instance.GetDBSetting("FinishWorkoutCounter").Value);
                //    if (count<=5)
                //    {
                //        if (count == 2)
                //            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1051);
                //        _firebase.LogEvent($"Finish_workout_{count}", $"{count}");
                //        count += 1;
                //        LocalDBManager.Instance.SetDBSetting("FinishWorkoutCounter", $"{count}");
                //    }
                //}
                foreach (ExerciceModel exerciceModel in CurrentLog.Instance.CurrentWorkoutTemplate.Exercises)
                {
                    LocalDBManager.Instance.SetDBSetting($"workout{CurrentLog.Instance.CurrentWorkoutTemplate.Id}exercise{exerciceModel.Id}", "false");
                }

                try
                {
                    if (CurrentLog.Instance.IsMobilityStarted || CurrentLog.Instance.CurrentWorkoutTemplateGroup.ProgramId == 757 || CurrentLog.Instance.CurrentWorkoutTemplateGroup.Id == 1922 || CurrentLog.Instance.CurrentWorkoutTemplateGroup.Id == 1923 || CurrentLog.Instance.CurrentWorkoutTemplateGroup.Id == 1924)
                    {

                        foreach (ExerciceModel exerciceModel in CurrentLog.Instance.CurrentWorkoutTemplate.Exercises)
                        {
                            LocalDBManager.Instance.SetDBSetting($"workout{CurrentLog.Instance.CurrentWorkoutTemplate.Id}exercise{exerciceModel.Id}", "false");
                        }




                        if (CurrentLog.Instance.IsMobilityStarted)
                        {
                            StartTodaysWorkout();
                            return;
                        }

                        Xamarin.Forms.MessagingCenter.Send<FinishWorkoutMessage>(new FinishWorkoutMessage() { PopupMessage = "" }, "FinishWorkoutMessage");

                        try
                        {

                            if (App.Current.MainPage.Navigation.NavigationStack.First() is MainTabbedPage)
                                ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[0];

                        }
                        catch (Exception ex)
                        {

                        }
                        DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1352);
                        Navigation.PopToRootAsync();
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Xamarin.Forms.MessagingCenter.Send<LevelUpInfoMessage>(new LevelUpInfoMessage() { Msg = "" }, "LevelUpInfoMessage");
                        });
                        return;
                    }
                }
                catch (Exception ex)
                {

                }

                LocalDBManager.Instance.SetDBSetting("OlderWorkoutName", CurrentLog.Instance.CurrentWorkoutTemplate.Label);
                if (CurrentLog.Instance.IsRecoveredWorkout && CurrentLog.Instance.CurrentWorkoutTemplate.Id == 12645)
                {
                    try
                    {
                        DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1352);
                        ConfirmConfig ShowConfirmPopUp = new ConfirmConfig()
                        {
                            Message = $"You're back on track—nice! Want to switch to a bodyweight program for the time being?",
                            Title = "",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = "Change workout",
                            CancelText = "No thanks",
                            //OnAction = async (bool ok) =>
                            //{
                            //    if (ok)
                            //    {

                            //    }
                            //    else
                            //    {
                            //        await PagesFactory.PopAsync();
                            //    }
                            //}
                        };
                        var response = await UserDialogs.Instance.ConfirmAsync(ShowConfirmPopUp);
                        if (response)
                        {

                            return;
                        }

                        foreach (ExerciceModel exerciceModel in CurrentLog.Instance.CurrentWorkoutTemplate.Exercises)
                        {
                            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(exerciceModel.Id))
                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(exerciceModel.Id);

                            LocalDBManager.Instance.SetDBSetting($"workout{CurrentLog.Instance.CurrentWorkoutTemplate.Id}exercise{exerciceModel.Id}", "false");
                            try
                            {
                                bool isSwapped = ((App)Application.Current).SwapExericesContexts.Swaps.Any(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == exerciceModel.Id);
                                if (isSwapped)
                                {
                                    long targetExerciseId = (long)((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == exerciceModel.Id).TargetExerciseId;

                                    var obj = (Application.Current as App)?.FinishedExercices.FirstOrDefault(x => x.Id == targetExerciseId);
                                    if (obj != null)
                                    {
                                        LocalDBManager.Instance.SetDBSetting($"workout{CurrentLog.Instance.CurrentWorkoutTemplate.Id}exercise{targetExerciseId}", "false");
                                        (Application.Current as App)?.FinishedExercices.Remove(obj);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                        }
                        await PagesFactory.PopToRootAsync();
                        return;
                    }
                    catch (Exception ex)
                    {

                    }
                    return;
                }
                LocalDBManager.Instance.SetDBSetting("last_workout_label", CurrentLog.Instance.CurrentWorkoutTemplate.Label);
                LocalDBManager.Instance.SetDBSetting("lastDoneProgram", CurrentLog.Instance.CurrentWorkoutTemplate.Id.ToString());
                LocalDBManager.Instance.SetDBSetting("IsSystemWorkout", CurrentLog.Instance.CurrentWorkoutTemplate.IsSystemExercise == true ? "true" : "false");
                bool isSystem = CurrentLog.Instance.CurrentWorkoutTemplate.IsSystemExercise;
                foreach (ExerciceModel exerciceModel in CurrentLog.Instance.CurrentWorkoutTemplate.Exercises)
                {
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(exerciceModel.Id))
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(exerciceModel.Id);

                    LocalDBManager.Instance.SetDBSetting($"workout{CurrentLog.Instance.CurrentWorkoutTemplate.Id}exercise{exerciceModel.Id}", "false");
                    try
                    {
                        bool isSwapped = ((App)Application.Current).SwapExericesContexts.Swaps.Any(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == exerciceModel.Id);
                        if (isSwapped)
                        {
                            long targetExerciseId = (long)((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == exerciceModel.Id).TargetExerciseId;

                            var obj = (Application.Current as App)?.FinishedExercices.FirstOrDefault(x => x.Id == targetExerciseId);
                            if (obj != null)
                            {
                                LocalDBManager.Instance.SetDBSetting($"workout{CurrentLog.Instance.CurrentWorkoutTemplate.Id}exercise{targetExerciseId}", "false");
                                (Application.Current as App)?.FinishedExercices.Remove(obj);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
                Xamarin.Forms.MessagingCenter.Send<FinishWorkoutMessage>(new FinishWorkoutMessage() { PopupMessage = "" }, "FinishWorkoutMessage");


                try
                {

                    if (App.Current.MainPage.Navigation.NavigationStack.First() is MainTabbedPage)
                        ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[0];

                }
                catch (Exception ex)
                {

                }

                try
                {
                    string jsonFileName = "ProgramList.json";
                    //Load local workouts
                    ExerciceModel exerciseList = new ExerciceModel();
                    var assembly = typeof(KenkoChooseYourWorkoutExercisePage).GetTypeInfo().Assembly;
                    Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{jsonFileName}");
                    GetUserWorkoutTemplateGroupResponseModel getUserWorkoutTemplate;
                    using (var reader = new System.IO.StreamReader(stream))
                    {
                        var jsonString = reader.ReadToEnd();

                        //Converting JSON Array Objects into generic list    
                        getUserWorkoutTemplate = JsonConvert.DeserializeObject<GetUserWorkoutTemplateGroupResponseModel>(jsonString);
                        if (getUserWorkoutTemplate != null)
                        {
                            WorkoutTemplateGroupModel getUserWorkout = null;
                            foreach (var item in getUserWorkoutTemplate.WorkoutOrders)
                            {
                                var m = item.WorkoutTemplates.Where(x => x.Id == CurrentLog.Instance.CurrentWorkoutTemplate.Id).FirstOrDefault();
                                if (m != null)
                                {
                                    getUserWorkout = item;
                                    break;
                                }
                            }
                            if (getUserWorkout != null)
                            {
                                var index = getUserWorkout.WorkoutTemplates.IndexOf(getUserWorkout.WorkoutTemplates.First(x => x.Id == CurrentLog.Instance.CurrentWorkoutTemplate.Id));
                                var nextWorkoutindex = 0;
                                if (index == getUserWorkout.WorkoutTemplates.Count - 1)
                                    nextWorkoutindex = 0;
                                else
                                    nextWorkoutindex = index + 1;
                                var nextworkout = getUserWorkout.WorkoutTemplates[nextWorkoutindex];

                                var upi = new GetUserProgramInfoResponseModel()
                                {
                                    NextWorkoutTemplate = new WorkoutTemplateModel()
                                    {
                                        Id = nextworkout.Id,
                                        Label = nextworkout.Label,
                                        IsSystemExercise = true
                                    },
                                    RecommendedProgram = new WorkoutTemplateGroupModel()
                                    {
                                        Id = getUserWorkout.Id,
                                        Label = getUserWorkout.Label
                                    }
                                };
                                var usWorkout = ((App)Application.Current).UserWorkoutContexts.workouts;

                                usWorkout = new GetUserWorkoutLogAverageResponse();
                                if (usWorkout.GetUserProgramInfoResponseModel != null)
                                {
                                    usWorkout.GetUserProgramInfoResponseModel.NextWorkoutTemplate = upi.NextWorkoutTemplate;
                                    if (usWorkout.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                                    {
                                        usWorkout.GetUserProgramInfoResponseModel.RecommendedProgram.Id = upi.RecommendedProgram.Id;
                                        usWorkout.GetUserProgramInfoResponseModel.RecommendedProgram.Label = upi.RecommendedProgram.Label;
                                    }
                                    else
                                    {
                                        usWorkout.GetUserProgramInfoResponseModel.RecommendedProgram = upi.RecommendedProgram;
                                    }
                                }
                                else
                                {
                                    usWorkout.GetUserProgramInfoResponseModel = upi;
                                }
                                    ((App)Application.Current).UserWorkoutContexts.workouts = usWorkout;
                                ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                            }

                        }

                    }
                }
                catch (Exception ex)
                {

                }
                Navigation.PopToRootAsync();
                DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1352);

                var responseLog = await DrMuscleRestClient.Instance.SaveGetWorkoutInfo(new SaveWorkoutModel() { WorkoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id, WorkoutTemplateId = CurrentLog.Instance.CurrentWorkoutTemplateGroup != null ? CurrentLog.Instance.CurrentWorkoutTemplateGroup.Id : 0 });
                Device.BeginInvokeOnMainThread(() =>
                {
                    Xamarin.Forms.MessagingCenter.Send<LevelUpInfoMessage>(new LevelUpInfoMessage() { Msg = "" }, "LevelUpInfoMessage");
                });

                if (responseLog != null && responseLog.RecommendedProgram != null)
                {
                    if (responseLog.RecommendedProgram.Level != null)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("WorkoutLevel") != null && LocalDBManager.Instance.GetDBSetting("WorkoutLevel").Value != null)
                        {
                            if (responseLog.RecommendedProgram.Level > int.Parse(LocalDBManager.Instance.GetDBSetting("WorkoutLevel").Value))
                            {
                                if (responseLog.RecommendedProgram.Level == (int.Parse(LocalDBManager.Instance.GetDBSetting("WorkoutLevel").Value)) + 1)
                                {
                                    //Level messages
                                    if (responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight") && responseLog.RecommendedProgram.Level == 2)
                                    {
                                        await MakePopup("Congratulations—you have reached level 2!", "Your program now includes new and harder exercises.", AppResources.GotIt, AppResources.LearnMore, true, "https://dr-muscle.com/building-muscle");
                                    }
                                    if (LocalDBManager.Instance.GetDBSetting("last_workout_label").Value.Contains("bands") && responseLog.RecommendedProgram.Level == 4 && !responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight"))
                                    {
                                        await MakePopup("Congratulations—you have reached level 4!", "Your program now includes easy workouts to help you recover.", AppResources.GotIt, AppResources.LearnMore, true, "http://dr-muscle.com/easy");
                                    }
                                    if (responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight") && responseLog.RecommendedProgram.Level == 3)
                                    {
                                        await MakePopup("Congratulations—you have reached level 3!", "Your program now includes new and harder exercises.", AppResources.GotIt, AppResources.LearnMore, true, "https://dr-muscle.com/building-muscle");
                                    }

                                    if (responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight") && responseLog.RecommendedProgram.Level == 4)
                                    {
                                        await MakePopup("Congratulations—you have reached level 4!", "Your program now includes new and harder exercises.", AppResources.GotIt, AppResources.LearnMore, true, "https://dr-muscle.com/building-muscle");
                                    }
                                    else if (responseLog.RecommendedProgram.Level == 2 && !responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight"))
                                    {
                                        await MakePopup("Congratulations—you have reached level 2!", "Your program now includes A/B workouts with new exercises in rotation.", AppResources.GotIt, AppResources.LearnMore, true, "https://dr-muscle.com/build-muscle-faster/#3");
                                    }
                                    if (responseLog.RecommendedProgram.Level == 3 && !responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight"))
                                    {
                                        await MakePopup("Congratulations—you have reached level 3!", "Your program now includes A/B/C workouts with new exercises in rotation.", AppResources.GotIt, AppResources.LearnMore, true, "https://dr-muscle.com/build-muscle-faster/#3");
                                    }

                                    if (LocalDBManager.Instance.GetDBSetting("last_workout_label").Value.Contains("bands") && responseLog.RecommendedProgram.Level == 4 && !responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight"))
                                    {
                                        await MakePopup("Congratulations—you have reached level 4!", "Your program now includes easy workouts to help you recover.", AppResources.GotIt, AppResources.LearnMore, true, "http://dr-muscle.com/easy");
                                    }


                                    else if (responseLog.RecommendedProgram.Level == 4 && !responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight"))
                                    {
                                        await MakePopup("Congratulations—you have reached level 4!", "Your program now includes easy workouts to help you recover.", AppResources.GotIt, AppResources.LearnMore, true, "http://dr-muscle.com/easy");
                                    }
                                    if (responseLog.RecommendedProgram.Level == 5)
                                    {
                                        await MakePopup("Congratulations—you have reached level 5!", "Your program now includes A/B easy workouts to help you recover.", AppResources.GotIt, AppResources.LearnMore, true, "http://dr-muscle.com/easy");
                                    }
                                    if (responseLog.RecommendedProgram.Level == 6)
                                    {
                                        await MakePopup("Congratulations—you have reached level 6!", "Your program now includes A/B/C easy workouts to help you recover.", AppResources.GotIt, AppResources.LearnMore, true, "http://dr-muscle.com/easy");
                                    }

                                    if (responseLog.RecommendedProgram.Level == 7)
                                    {
                                        await MakePopup("Congratulations—you have reached level 7!", "Your program now includes A/B/C medium workouts to prep you for new records on your normal workouts.", AppResources.GotIt, AppResources.LearnMore, true, "http://dr-muscle.com/easy");
                                    }
                                }
                                else if (responseLog.RecommendedProgram.Level == (int.Parse(LocalDBManager.Instance.GetDBSetting("WorkoutLevel").Value)) + 2)
                                {
                                    if (LocalDBManager.Instance.GetDBSetting("last_workout_label").Value.Contains("Bands") && responseLog.RecommendedProgram.Level == 4 && !responseLog.RecommendedProgram.Label.ToLower().Contains("bodyweight"))
                                    {
                                        await MakePopup("Congratulations—you have reached level 4!", "Your program now includes easy workouts to help you recover.", AppResources.GotIt, AppResources.LearnMore, true, "http://dr-muscle.com/easy");
                                    }
                                }
                            }
                        }


                        LocalDBManager.Instance.SetDBSetting("WorkoutLevel", responseLog.RecommendedProgram.Level.ToString());
                    }
                }

                CurrentLog.Instance.CurrentWorkoutTemplate = null;
                CurrentLog.Instance.WorkoutTemplateCurrentExercise = null;
                CurrentLog.Instance.WorkoutStarted = false;
                LocalDBManager.Instance.ResetReco();
                string fname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                string Msg = "";// $"{AppResources.Congratulations} {fname}!";
                try
                {
                    if (responseLog != null && responseLog.RecommendedProgram != null)
                    {

                        if (responseLog.RecommendedProgram.RemainingToLevelUp > 0)
                        {
                            //OLD
                            var workoutText = responseLog.RecommendedProgram.RemainingToLevelUp > 1 ? AppResources.WorkoutsFullStop : $"{AppResources.WorkOut.ToLower() }.";
                            Msg += $"{AppResources.WellDone}! {AppResources.YouAre1WorkoutCloserToNewExercisesYourProgramWillLevelUpIn} {responseLog.RecommendedProgram.RemainingToLevelUp} {workoutText} {AppResources.ISuggestForRecovery}";
                            //NEW
                            //Msg = $"Well done! I suggest at least 24 hours for recovery. You can take time off now :)";
                            //

                        }

                    }
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //Xamarin.Forms.MessagingCenter.Send<LevelUpInfoMessage>(new LevelUpInfoMessage() { Msg = Msg }, "LevelUpInfoMessage");
                    });
                }
                catch (Exception ex)
                {
                    //await PagesFactory.PopToRootAsync();
                }

            }
            catch (Exception ex)
            {

            }
        }

        private async Task SetWarmupsWorkout()
        {
            if (LocalDBManager.Instance.GetDBSetting("MobilityLevel")?.Value == "Beginner")
                CurrentLog.Instance.CurrentWorkoutTemplate.Id = 16906;
            else if (LocalDBManager.Instance.GetDBSetting("MobilityLevel")?.Value == "Intermediate")
                CurrentLog.Instance.CurrentWorkoutTemplate.Id = 16905;
            else if (LocalDBManager.Instance.GetDBSetting("MobilityLevel")?.Value == "Advanced")
                CurrentLog.Instance.CurrentWorkoutTemplate.Id = 16904;
            else
                CurrentLog.Instance.CurrentWorkoutTemplate.Id = 16906;
        }

        private async void StartTodaysWorkout()
        {
            CurrentLog.Instance.IsMobilityStarted = false;
            CurrentLog.Instance.IsMobilityFinished = true;
            try
            {

                if (App.Current.MainPage.Navigation.NavigationStack.First() is MainTabbedPage)
                    ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[0];

            }
            catch (Exception ex)
            {

            }
            await Navigation.PopToRootAsync();
            Device.BeginInvokeOnMainThread(() =>
            {
                Xamarin.Forms.MessagingCenter.Send<StartNormalWorkout>(new StartNormalWorkout() { }, "StartNormalWorkout");
            });
        }

        private async Task MakePopup(string title, string message, string cancelTitle, string OkTitle, bool isLink = false, string linkUrl = "")
        {
            ConfirmConfig supersetConfig = new ConfirmConfig()
            {
                Title = title,
                Message = message,
                OkText = OkTitle,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                CancelText = AppResources.GotIt,
                //OnAction = async (bool ok) =>
                //{
                //    if (ok)
                //    {

                //    }
                //    else
                //    {

                //    }
                //}


            };

            var x = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
            if (x)
            {
                if (isLink)
                    Device.OpenUri(new Uri(linkUrl));
            }
        }

        private async void PlateTapped(object sender, EventArgs e)
        {
            try
            {
                ImgPlate.Effects.Clear();
                DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            }
            catch (Exception ex)
            {

            }


            CurrentLog.Instance.CurrentWeight = App.PCWeight;

            var page = new PlateCalculatorPopup();
            await PopupNavigation.Instance.PushAsync(page);
            //Xamarin.Forms.MessagingCenter.Send<PlateCalulatorMessage>(new PlateCalulatorMessage(), "PlateCalulatorMessage");

        }
        private async void TimerTapped(object sender, EventArgs e)
        {
            if (Config.ShowTimer)
                BtnTimer.Effects.Clear();
            SlideTimerAction();
        }


        private async void ExerciseListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (contextMenuStack != null)
                HideContextButton();

            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //ExerciseListView.BeginRefresh();
                    //ExerciseListView.EndRefresh();
                });
            }

            try
            {

                if (ExerciseListView.SelectedItem == null)
                    return;

                Device.BeginInvokeOnMainThread(() =>
                {
                    WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)e.Item;
                    if (workout != null)
                    {
                        if (!workout.IsFinished)
                        {
                            if (ExerciseListView.SelectedItem != null)
                                ExerciseListView.SelectedItem = null;
                            return;
                        }
                        workout.IsFinished = true;
                        App.PCWeight = decimal.Parse(workout.WeightDouble.ReplaceWithDot());
                        UpdateOneRM(workout, Convert.ToDecimal(workout.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), workout.Reps);
                        workout.IsEditing = true;

                        workout.IsNext = true;

                        if (Device.RuntimePlatform.Equals(Device.iOS))
                        {
                            workout.IsSizeChanged = !workout.IsSizeChanged;
                            ExerciseListView.ItemsSource = exerciseItems;
                        }
                        else
                        {
                            ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                        }
                    }
                });
                if (ExerciseListView.SelectedItem != null)
                    ExerciseListView.SelectedItem = null;

            }
            catch (Exception ex)
            {

            }
            //
        }
        private async void PicktorialTapped(object sender, EventArgs e)
        {
            try
            {

                if (contextMenuStack != null)
                    HideContextButton();
                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
                if (!string.IsNullOrEmpty(m.VideoUrl))
                {
                    CurrentLog.Instance.VideoExercise = GetExerciseModel(m);
                    await PagesFactory.PushAsync<ExerciseVideoPage>();
                }

            }
            catch (Exception ex)
            {

            }
            // OnCancelClicked(sender, e);
        }
        //class LifeCycle
        //{
        //    [JsonProperty("lifecycle stage")]
        //    public string Life_Cycle { get; set; }
        //}
        private async void CellHeaderTapped(object sender, EventArgs e)
        {
            try
            {
                if (vHeader != null)
                    vHeader.Effects.Clear();

                _isAskedforLightSession = false;
                _isAskedforDeload = false;
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = AppResources.PleaseCheckInternetConnection,
                        Title = AppResources.ConnectionError
                    });
                    return;
                }
                if (contextMenuStack != null)
                    HideContextButton();


                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
                try
                {

                    foreach (var item in vHeaders)
                    {
                        vHeaders[item.Key].Effects.Clear();
                    }

                }
                catch (Exception ex)
                {

                }
                if (CurrentLog.Instance.IsMobilityStarted)
                {

                }
                else if (App.IsSupersetPopup && !_superSetRunning && m.IsPlate && !Config.ShowPlateTooltip)
                {
                    Config.ShowPlateTooltip = true;
                    DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(ImgPlate, true);
                }
                else if (Config.ShowSupersetPopup == 3 && !Config.ShowPlateTooltip && !App.IsSupersetPopup)
                {
                    Config.ShowPlateTooltip = true;
                    DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(ImgPlate, true);
                }
                else if (App.IsSupersetPopup && !_superSetRunning && !m.IsPlate && !Config.ShowPlateTooltip && !Config.ShowChallenge)
                {
                    btnMore = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)(vHeaders[m.Id])).Children[2]).Children[0]).Children[0]).Children[5]).Children[6];

                    Config.ShowChallenge = true;
                    TooltipEffect.SetPosition(btnMore, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(btnMore, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(btnMore, Color.White);
                    TooltipEffect.SetText(btnMore, $"Try a challenge");
                    TooltipEffect.SetHasTooltip(btnMore, true);
                    TooltipEffect.SetHasShowTooltip(btnMore, true);
                    _tryChallenge = true;

                }
                else if (Config.ShowSupersetPopup == 3 && !Config.ShowPlateTooltip && !App.IsSupersetPopup && !Config.ShowChallenge)
                {
                    btnMore = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)(vHeaders[m.Id])).Children[2]).Children[0]).Children[0]).Children[5]).Children[6];

                    Config.ShowChallenge = true;
                    TooltipEffect.SetPosition(btnMore, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(btnMore, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(btnMore, Color.White);
                    TooltipEffect.SetText(btnMore, $"Try a challenge");
                    TooltipEffect.SetHasTooltip(btnMore, true);
                    TooltipEffect.SetHasShowTooltip(btnMore, true);
                    _tryChallenge = true;
                }
                else if (Config.ShowPlateTooltip && !Config.ShowChallenge)
                {
                    try
                    {

                        btnMore = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)(vHeaders[m.Id])).Children[2]).Children[0]).Children[0]).Children[5]).Children[6];
                        //btnMore = (DrMuscleButton)((StackLayout)((StackLayout)sender).Children[5]).Children[6];

                        //btnMore = (DrMuscleButton)((StackLayout)((StackLayout)sender).Children[5]).Children[6];
                        Config.ShowChallenge = true;
                        TooltipEffect.SetPosition(btnMore, TooltipPosition.Bottom);
                        TooltipEffect.SetBackgroundColor(btnMore, AppThemeConstants.BlueColor);
                        TooltipEffect.SetTextColor(btnMore, Color.White);
                        TooltipEffect.SetText(btnMore, $"Try a challenge");
                        TooltipEffect.SetHasTooltip(btnMore, true);
                        TooltipEffect.SetHasShowTooltip(btnMore, true);
                        _tryChallenge = true;


                    }
                    catch (Exception ex)
                    {

                    }
                }
                else if (Config.ShowDeload && !Config.ShowTimer)
                {
                    TooltipEffect.SetPosition(BtnTimer, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(BtnTimer, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(BtnTimer, Color.White);
                    TooltipEffect.SetText(BtnTimer, $"Try timer");
                    TooltipEffect.SetHasTooltip(BtnTimer, true);
                    TooltipEffect.SetHasShowTooltip(BtnTimer, true);
                    CurrentLog.Instance.ShowTimerOptions = true;
                    Config.ShowTimer = true;
                }
                else if (Config.ShowTimer && !Config.ShowEditWorkout)
                {
                    TooltipEffect.SetPosition(BtnEditWorkout, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(BtnEditWorkout, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(BtnEditWorkout, Color.White);
                    TooltipEffect.SetText(BtnEditWorkout, $"Try editing workout");
                    TooltipEffect.SetHasTooltip(BtnEditWorkout, true);
                    TooltipEffect.SetHasShowTooltip(BtnEditWorkout, true);
                    CurrentLog.Instance.ShowEditWorkouts = true;
                    Config.ShowEditWorkout = true;
                }

                if (m.Id == 0)
                    return;

                if (m.IsNextExercise && m.Count > 0)
                {
                    m.Clear();
                    m.IsNextExercise = false;
                    return;
                }
                m.IsNextExercise = true;// !m.IsNextExercise;

                if (m.IsFinished)
                {
                    try
                    {
                        if (!CrossConnectivity.Current.IsConnected)
                        {
                            await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            {
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                Message = AppResources.PleaseCheckInternetConnection,
                                Title = AppResources.ConnectionError
                            });
                            return;
                        }
                        List<WorkoutLogSerieModelRef> setList = new List<WorkoutLogSerieModelRef>();
                        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                        List<HistoryModel> historyModel = new List<HistoryModel>();
                        if (historyModel.Count > 0)
                        {
                            var logSerie = historyModel.First().Exercises[0].Sets.OrderBy(x => x.LogDate).ToList();
                            var warmupList = logSerie.Where(l => l.IsWarmups).ToList();
                            for (int i = 0; i < warmupList.Count; i++)
                            {
                                warmupList[i].Weight.IsRound = false;
                                setList.Add(new WorkoutLogSerieModelRef()
                                {
                                    Weight = isKg ? warmupList[i].Weight : new MultiUnityWeight(warmupList[i].Weight.Lb, "lb", true),
                                    IsWarmups = warmupList[i].IsWarmups,
                                    Reps = warmupList[i].Reps,
                                    SetNo = $"SET {setList.Count + 1}",
                                    IsLastWarmupSet = i == warmupList.Where(l => l.IsWarmups).ToList().Count - 1 ? true : false,
                                    IsHeaderCell = i == 0 ? true : false,
                                    HeaderImage = "",
                                    HeaderTitle = "",
                                    ExerciseName = m.Label,
                                    EquipmentId = m.EquipmentId,
                                    SetTitle = i == 0 ? "Let's warm up:" : "",
                                    IsFinished = true,
                                    IsExerciseFinished = true,
                                    Id = logSerie[i].Id,
                                    IsTimeBased = m.IsTimeBased,
                                    IsBodyweight = m.IsBodyweight,
                                    IsNormalset = m.IsNormalSets,
                                    ShouldUpdateIncrement = true

                                });

                            }
                            if (setList.Count > 1)
                            {
                                setList.Last().SetTitle = "Last warm-up set:";
                            }

                            var worksetsList = logSerie.Where(l => l.IsWarmups == false).ToList();
                            for (int j = 0; j < worksetsList.Count; j++)
                            {
                                worksetsList[j].Weight.IsRound = false;
                                var rec = new WorkoutLogSerieModelRef()
                                {
                                    Weight = isKg ? worksetsList[j].Weight : new MultiUnityWeight(worksetsList[j].Weight.Lb, "lb", true),
                                    IsWarmups = worksetsList[j].IsWarmups,
                                    Reps = worksetsList[j].Reps,
                                    SetNo = $"SET {setList.Count + 1}",
                                    ExerciseName = m.Label,
                                    EquipmentId = m.EquipmentId,
                                    IsFirstWorkSet = j == 0 ? true : false,
                                    SetTitle = j == 0 ? "1st work set—you got this:" : "All right! Now let's try:",
                                    IsFinished = true,
                                    IsExerciseFinished = true,
                                    Id = worksetsList[j].Id,
                                    IsTimeBased = m.IsTimeBased,
                                    IsBodyweight = m.IsBodyweight,
                                    IsNormalset = m.IsNormalSets,
                                    ShouldUpdateIncrement = true
                                };
                                if (setList.Count == 0)
                                {
                                    rec.IsHeaderCell = true;
                                    rec.HeaderImage = "";
                                    rec.HeaderTitle = "";
                                }
                                setList.Add(rec);
                            }


                            if ((setList.Count - logSerie.Where(l => l.IsWarmups).ToList().Count) > 3)
                            {
                                setList[setList.Count - 2].SetTitle = "Almost done—keep it up!";
                                setList.Last().SetTitle = "Last set—you can do this!";
                            }
                            else if ((setList.Count - logSerie.Where(l => l.IsWarmups).ToList().Count) > 2)
                            {
                                setList.Last().SetTitle = "Last set—you can do this!";
                            }

                            if (setList.First().IsWarmups)
                            {
                                var warmString = setList.Where(l => l.IsWarmups).ToList().Count < 2 ? "warm-up" : "warm-ups";
                                setList.First().SetTitle = $"{setList.Where(l => l.IsWarmups).ToList().Count} {warmString}, {setList.Where(l => !l.IsWarmups).ToList().Count} work sets\nLet's warm up:";
                            }
                            if (setList.Count > 0)
                            {
                                setList.Last().IsLastSet = true;
                                //if (m.IsFirstSide)
                                //    setList.Last().IsFirstSide = true;
                            }
                            for (var i = 0; i < setList.Count; i++)
                                ((WorkoutLogSerieModelRef)setList[i]).SetNo = $"SET {i + 1}/{setList.Count}";
                            foreach (var item in setList)
                            {
                                m.Add(item);
                            }
                            ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                            {
                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[m.Id] = new ObservableCollection<WorkoutLogSerieModelRef>(setList);
                            }
                            else
                            {
                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Add(m.Id, new ObservableCollection<WorkoutLogSerieModelRef>(setList));
                            }
                            ScrollToSnap(setList, m);

                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    return;
                }

                if (m.RecoModel != null)
                {
                    FetchReco(m, null);
                    return;
                }

                NewExerciseLogResponseModel newExercise = null;//await DrMuscleRestClient.Instance.IsNewExerciseWithSessionInfo(new ExerciceModel() { Id = m.Id });
                if (newExercise != null)
                {
                    if (!newExercise.IsNewExercise)
                    {
                        try
                        {
                            DateTime? lastLogDate = newExercise.LastLogDate;
                            int? sessionDays = null;


                            string WeightRecommandation;
                            RecommendationModel reco = null;

                            //Todo: clean up on 2019 01 18
                            if (LocalDBManager.Instance.GetDBSetting("SetStyle") == null)
                                LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                                LocalDBManager.Instance.SetDBSetting("QuickMode", "false");
                            var bodyPartname = "";


                            switch (m.BodyPartId)
                            {
                                case 1:
                                    break;
                                case 2:
                                    bodyPartname = "Shoulders";
                                    break;
                                case 3:
                                    bodyPartname = "Chest";
                                    break;
                                case 4:
                                    bodyPartname = "Back";
                                    break;
                                case 5:
                                    bodyPartname = "Biceps";
                                    break;
                                case 6:
                                    bodyPartname = "Triceps";
                                    break;
                                case 7:
                                    bodyPartname = "Abs";
                                    break;
                                case 8:
                                    bodyPartname = "Legs";
                                    break;
                                case 9:
                                    bodyPartname = "Calves";
                                    break;
                                case 10:
                                    bodyPartname = "Neck";
                                    break;
                                case 11:
                                    bodyPartname = "Forearm";
                                    break;
                                default:
                                    //
                                    break;
                            }
                            if (LocalDBManager.Instance.GetDBSetting($"IsAskedLightSession{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}") != null && LocalDBManager.Instance.GetDBSetting($"IsAskedLightSession{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}").Value == "true")
                                CurrentLog.Instance.IsAskedLightSession = true;
                            else
                                CurrentLog.Instance.IsAskedLightSession = false;
                            if (lastLogDate != null && !CurrentLog.Instance.IsAskedLightSession)
                            {
                                var days = (int)(DateTime.Now - (DateTime)lastLogDate).TotalDays;
                                if (days >= 5 && days <= 9)
                                    sessionDays = days;
                                if (days > 9)
                                {
                                    ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                                    {
                                        Title = "Light session?",
                                        Message = string.IsNullOrEmpty(bodyPartname) == false ? $"The last time you trained {bodyPartname.ToLower()} was {days} {AppResources.DaysAgoDoALightSessionToRecover}" : string.Format("{0} {1} {2} {3} {4}", "The last time you trained", m.Label, AppResources.was, days, AppResources.DaysAgoDoALightSessionToRecover),
                                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                        OkText = AppResources.LightSession,
                                        CancelText = AppResources.Cancel,
                                    };
                                    try
                                    {
                                        LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + CurrentLog.Instance.WorkoutTemplateCurrentExercise.Id + "Normal", DateTime.Now.AddDays(-1).ToString());
                                        LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + CurrentLog.Instance.WorkoutTemplateCurrentExercise.Id + "RestPause", DateTime.Now.AddDays(-1).ToString());
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
                                    CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(m);
                                    bool isConfirm = true;
                                    if (!App.IsConnectedToWatch)
                                        isConfirm = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                                    if (isConfirm)
                                    {
                                        if (days > 50)
                                            days = 50;
                                        sessionDays = days;
                                        App.WorkoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id;
                                        App.BodypartId = (int)CurrentLog.Instance.ExerciseLog.Exercice.BodyPartId;
                                        App.Days = days;
                                        if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                                            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                                        if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                                        {
                                            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                                            m.RecoModel = null;
                                        }
                                        //reco.Weight = new MultiUnityWeight(reco.Weight.Kg - ((reco.Weight.Kg * (decimal)1.5 * days) / 100), "kg");
                                        //reco.WarmUpWeightSet1 = new MultiUnityWeight(reco.WarmUpWeightSet1.Kg - ((reco.WarmUpWeightSet1.Kg * (decimal)1.5 * days) / 100), "kg");
                                        //reco.WarmUpWeightSet2 = new MultiUnityWeight(reco.WarmUpWeightSet2.Kg - ((reco.WarmUpWeightSet2.Kg * (decimal)1.5 * days) / 100), "kg");
                                    }
                                    else
                                    {
                                        //CurrentLog.Instance.IsAskedLightSession = true;
                                        LocalDBManager.Instance.SetDBSetting($"IsAskedLightSession{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}", "true");

                                        _isAskedforLightSession = true;
                                        sessionDays = null;
                                        App.Days = 0;
                                        if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                                            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                                        if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                                        {
                                            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                                            m.RecoModel = null;
                                        }
                                    }
                                }
                            }
                            try
                            {
                                if (App.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && App.BodypartId == (int)m.BodyPartId && App.Days > 9)
                                {
                                    sessionDays = App.Days;
                                }

                            }
                            catch (Exception ex)
                            {

                            }

                            await FetchReco(m, sessionDays);
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    else
                    {
                        if (m.Id != 0)
                            RunExercise(m);
                    }
                }
                else
                    await FetchReco(m);

            }
            catch (Exception ex)
            {

            }
        }

        private async Task FetchReco(ExerciseWorkSetsModel m, int? sessionDays = null)
        {
            try
            {
                if (m.IsNextExercise)
                {
                    long? workoutId = null;
                    try
                    {
                        //if (!CurrentLog.Instance.IsFromExercise)
                        workoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id;
                    }
                    catch (Exception)
                    {

                    }

                    if (m.Count > 0)
                    {
                        m.Clear();
                        return;
                    }

                    string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;

                    string exId = $"{m.Id}";
                    var lastTime = LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle);


                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                    {
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                    }
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                    {
                        var sets = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[m.Id];


                        if (m.RecoModel == null)
                        {
                            if (lastTime != null)
                            {
                                try
                                {
                                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value))
                                    {
                                        var LastRecoPlus1Day = Convert.ToDateTime(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value);
                                        if (LastRecoPlus1Day > DateTime.Now)
                                        {
                                            var recommendation = RecoContext.GetReco("Reco" + exId + setStyle);
                                            if (recommendation != null)
                                            {
                                                m.RecoModel = recommendation;
                                                m.RecoModel.IsDeload = false;
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Exception is:{ex.ToString()}");
                                }
                            }
                        }

                        if (m.RecoModel != null)
                        {
                            m.IsPyramid = m.RecoModel.IsPyramid;
                            m.IsReversePyramid = m.RecoModel.IsReversePyramid;
                            if (CurrentLog.Instance.ExerciseLog == null)
                            {
                                CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
                            }
                            CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(m);
                            if (CurrentLog.Instance.RecommendationsByExercise.ContainsKey(CurrentLog.Instance.ExerciseLog.Exercice.Id))
                                CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id] = m.RecoModel;
                            else
                                CurrentLog.Instance.RecommendationsByExercise.Add(CurrentLog.Instance.ExerciseLog.Exercice.Id, m.RecoModel);

                        }
                        foreach (var cacheSet in sets)
                        {
                            m.Add(cacheSet);
                        }
                        if (sets.Any(x => x.IsFinished == false))
                        {
                            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = sets.FirstOrDefault(x => x.IsFinished == false), Label = m.Label }, "SendWatchMessage");
                            App.PCWeight = Convert.ToDecimal(sets.FirstOrDefault(x => x.IsFinished == false).WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            // App.PCWeight = Convert.ToDecimal(sets.Last().WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.FinishExercise, SetModel = sets.Last(), Label = m.Label }, "SendWatchMessage");
                        }
                        if (m.IsUnilateral && sets.Where(x => x.IsFinished).FirstOrDefault() == null && sets.Where(x => x.IsFirstSide == false).FirstOrDefault() == null && !App.IsConnectedToWatch && !_areExercisesUnfnished)
                        {
                            m.IsFirstSide = true;

                            AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                            {
                                Title = "Do all sets for side 1",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "OK",
                            };

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                UserDialogs.Instance.Alert(ShowExplainRIRPopUp);
                            });


                        }
                        if (sets.Where(x => x.IsFirstSide == true).FirstOrDefault() != null)
                            m.IsFirstSide = true;
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                            ExerciseListView.ScrollTo(sets.First(), ScrollToPosition.Start, true);
                        await Task.Delay(300);

                        //ExerciseListView.ItemPosition = exerciseItems.IndexOf(m);
                        //ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;

                        //Scroll To position

                        if (m.RecoModel != null)
                        {
                            ScrollToSnap(sets.ToList(), m);

                            return;
                        }
                    }


                    if (lastTime != null)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value))
                            {
                                var LastRecoPlus1Day = Convert.ToDateTime(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value);
                                if (LastRecoPlus1Day > DateTime.Now)
                                {
                                    var recommendation = RecoContext.GetReco("Reco" + exId + setStyle);
                                    if (recommendation != null)
                                    {
                                        m.RecoModel = recommendation;
                                        m.RecoModel.IsDeload = false;


                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Exception is:{ex.ToString()}");
                        }
                    }
                    long? swapedExId = null;
                    try
                    {
                        if (m.IsSwapTarget)
                        {
                            bool isSwapped = ((App)Application.Current).SwapExericesContexts.Swaps.Any(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.TargetExerciseId == m.Id);
                            swapedExId = (long)((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.TargetExerciseId == m.Id).SourceExerciseId;
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                    if (LocalDBManager.Instance.GetDBSetting("IsPyramid") == null)
                        LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");

                    bool? isQuick = false;
                    if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                        isQuick = null;
                    else
                        isQuick = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
                    bool IsStrengthPhashe = false;
                    try
                    {
                        var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                        int remainingWorkout = 0, totalworkout = 0;
                        if (workouts != null && workouts.GetUserProgramInfoResponseModel != null)
                        {
                            if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                            {
                                totalworkout = workouts.GetUserProgramInfoResponseModel.RecommendedProgram.RequiredWorkoutToLevelUp;
                                remainingWorkout = workouts.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp != null ? (int)workouts.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp : 0;
                            }
                        }
                        if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label.Equals(CurrentLog.Instance.CurrentWorkoutTemplate.Label))

                            IsStrengthPhashe = RecoComputation.IsInStrengthPhase(CurrentLog.Instance.CurrentWorkoutTemplate.Label, int.Parse(string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value) ? "40" : LocalDBManager.Instance.GetDBSetting("Age")?.Value), remainingWorkout, totalworkout);
                    }
                    catch (Exception ex)
                    {

                    }
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                    if (m.RecoModel == null)
                    {
                        
                        if (m.RecoModel != null)
                        {

                            if (m.RecoModel.HistorySet != null)
                                m.RecoModel.HistorySet.Reverse();
                        }
                    }
                    var ShowWarmups = false;
                    if (m.RecoModel != null)
                    {

                        var weights = LocalDBManager.Instance.GetDBSetting($"SetupWeight{m.Id}")?.Value;
                        if (!string.IsNullOrEmpty(weights))
                        {
                            if (!m.IsBodyweight)
                            {
                                m.RecoModel.Weight = new MultiUnityWeight(Convert.ToDecimal(weights, CultureInfo.InvariantCulture), "kg", true);
                                m.RecoModel.Reps = 6;
                                m.RecoModel.NbRepsPauses = 2;
                            }
                            ShowWarmups = true;

                            LocalDBManager.Instance.SetDBReco("RReps" + m.Id + setStyle + "challenge", $"max");
                            AlertConfig challengeConfig = new AlertConfig()
                            {
                                Title = $"{m.Label}: ready for a new record?",
                                Message = $"Try a challenge. Do as many reps as you can on your first work set. Stop before your form breaks down.",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Try a challenge",
                            };
                            await UserDialogs.Instance.AlertAsync(challengeConfig);
                        }


                        //m.RecoModel.Weight = new MultiUnityWeight((decimal)100, WeightUnities.kg);
                        m.RecoModel.Max = null;
                        RecoComputation.ComputeWarmups(m.RecoModel, m.Id, m);
                        bool IsMaxChallenge = false;

                        var challengeacceptedValue = LocalDBManager.Instance.GetDBSetting($"{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}")?.Value;
                        if (!string.IsNullOrEmpty(challengeacceptedValue))
                        {
                            var challengeCount = int.Parse(challengeacceptedValue);
                            if (challengeCount == 2)
                                m.RecoModel.IsMaxChallenge = false;
                        }
                        m.RecoModel.IsLightSession = sessionDays == null ? false : sessionDays > 9 ? true : false;
                        if (m.RecoModel.IsLightSession)
                            m.RecoModel.IsDeload = false;
                        if (m.RecoModel.IsDeload || _isAskedforLightSession)
                        {
                            m.RecoModel.IsMaxChallenge = false;
                            _isAskedforLightSession = false;
                        }
                        if (m.IsBodyweight)
                            m.RecoModel.IsPyramid = false;
                        if (m.RecoModel.IsPyramid)
                        {
                            m.IsPyramid = true;
                            m.RecoModel.IsBackOffSet = false;
                            m.RecoModel.BackOffSetWeight = null;
                        }
                        if (m.RecoModel.IsReversePyramid)
                        {
                            m.IsReversePyramid = true;
                            m.RecoModel.NbPauses = m.RecoModel.NbPauses + m.RecoModel.Series;
                            m.RecoModel.Series = 0;
                        }
                        if (m.RecoModel.Reps <= 0)
                            m.RecoModel.Reps = 1;

                        if (m.RecoModel.NbRepsPauses <= 0)
                            m.RecoModel.NbRepsPauses = 1;

                        RecoContext.SaveContexts("Reco" + exId + setStyle, m.RecoModel);
                        LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + exId + setStyle, DateTime.Now.AddDays(1).ToString());
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") != null)
                        {
                            if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "deload")
                            {
                                m.RecoModel.IsDeload = true;
                                m.RecoModel.RecommendationInKg = m.RecoModel.Weight.Kg - (m.RecoModel.Weight.Kg * (decimal)0.1);
                                if (m.RecoModel.IsNormalSets || m.RecoModel.IsReversePyramid)
                                {
                                    m.RecoModel = RecoComputation.GetNormalDeload(m.RecoModel);
                                }
                                else
                                {
                                    m.RecoModel = RecoComputation.GetRestPauseDeload(m.RecoModel);
                                }
                                m.RecoModel.IsLightSession = true;
                                m.RecoModel.IsMaxChallenge = false;
                            }
                        }
                        if (LocalDBManager.Instance.GetDBSetting($"IsAskedDeload{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}") != null && LocalDBManager.Instance.GetDBSetting($"IsAskedDeload{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}").Value == "true")
                            CurrentLog.Instance.IsAskedDeload = true;
                        else
                            CurrentLog.Instance.IsAskedDeload = false;
                        if (CurrentLog.Instance.IsAskedDeload)
                            m.RecoModel.IsDeload = false;
                        if (m.RecoModel.IsDeload && !m.RecoModel.IsLightSession && !CurrentLog.Instance.IsAskedDeload)
                        {
                            var per = string.Format("{0:0.00}%", m.RecoModel.OneRMPercentage * 100);
                            bool isConfirm = true;
                            ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                            {
                                Title = $"Deload {m.Label}?",
                                Message = $"Strength {per} last time—deload to recover?",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Deload",
                                CancelText = AppResources.Cancel,
                            };
                            _isAskedforDeload = true;
                            if (!App.IsConnectedToWatch)
                                isConfirm = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                            if (isConfirm)
                            {
                                LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"deload");
                                //Create new Reco for deload
                                if (m.RecoModel.IsNormalSets)
                                {
                                    m.RecoModel = RecoComputation.GetNormalDeload(m.RecoModel);
                                }
                                else
                                {
                                    m.RecoModel = RecoComputation.GetRestPauseDeload(m.RecoModel);
                                }
                            }
                            else
                            {
                                LocalDBManager.Instance.SetDBSetting($"IsAskedDeload{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}", "true");
                                m.RecoModel.IsDeload = false;
                                RecoContext.SaveContexts("Reco" + exId + setStyle, m.RecoModel);
                            }
                        }

                        m.IsNormalSets = m.RecoModel.IsNormalSets;

                        string lbl3text = "";
                        string iconOrange = "";

                        lbl3text = "";

                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                        {
                            if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                            {
                                m.RecoModel.Reps = (int)Math.Ceiling((decimal)m.RecoModel.Reps + ((decimal)m.RecoModel.Reps * (decimal)0.1));
                                m.RecoModel.NbRepsPauses = (int)Math.Ceiling((decimal)m.RecoModel.NbRepsPauses + ((decimal)m.RecoModel.NbRepsPauses * (decimal)0.1));
                                m.RecoModel.IsMaxChallenge = false;
                                IsMaxChallenge = true;
                            }
                        }

                        try
                        {
                            if (LocalDBManager.Instance.GetDBSetting($"{CurrentLog.Instance.CurrentWorkoutTemplate.Id}Challenge{m.BodyPartId}") != null && LocalDBManager.Instance.GetDBSetting($"{CurrentLog.Instance.CurrentWorkoutTemplate.Id}Challenge{m.BodyPartId}").Value == $"{m.BodyPartId}")
                            {
                                m.RecoModel.IsMaxChallenge = false;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        if (LocalDBManager.Instance.GetDBSetting($"IsAskedChallenge{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}") != null && LocalDBManager.Instance.GetDBSetting($"IsAskedChallenge{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}").Value == "true")
                            CurrentLog.Instance.IsAskedChallenge = true;
                        else
                            CurrentLog.Instance.IsAskedChallenge = false;
                        if (CurrentLog.Instance.IsAskedChallenge)
                            m.RecoModel.IsMaxChallenge = false;
                        if (_isAskedforLightSession)
                            m.RecoModel.IsMaxChallenge = false;
                        if (m.RecoModel.IsMaxChallenge)
                        {
                            bool isMaxChallenge = true;
                            ConfirmConfig supersetConfig = new ConfirmConfig()
                            {
                                Title = $"{m.Label}: ready for a new record?",
                                Message = $"Try a challenge. Do as many reps as you can on your first work set. Stop before your form breaks down.",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Try a challenge",
                                CancelText = AppResources.Cancel,
                            };
                            if (!App.IsConnectedToWatch)
                                isMaxChallenge = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                            if (isMaxChallenge)
                            {
                                LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"max");
                                if (LocalDBManager.Instance.GetDBSetting($"{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}") == null)
                                    LocalDBManager.Instance.SetDBSetting($"{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}", "1");
                                else
                                {
                                    var cnt = LocalDBManager.Instance.GetDBSetting($"{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}")?.Value;
                                    if (!string.IsNullOrEmpty(cnt))
                                    {
                                        var challengeCount = int.Parse(cnt);

                                        if (challengeCount == 1)
                                            LocalDBManager.Instance.SetDBSetting($"{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}", "2");
                                        if (challengeCount == 0)
                                            LocalDBManager.Instance.SetDBSetting($"{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}", "1");
                                    }
                                }

                                LocalDBManager.Instance.SetDBSetting($"{DateTime.Now.Date}{m.Id}", "1");
                                //LocalDBManager.Instance.SetDBSetting("RReps" + exId + setStyle, $"{reco.Reps}");
                                try
                                {

                                    m.RecoModel.Reps = (int)Math.Ceiling((decimal)m.RecoModel.Reps + ((decimal)m.RecoModel.Reps * (decimal)0.1));
                                    m.RecoModel.NbRepsPauses = (int)Math.Ceiling((decimal)m.RecoModel.NbRepsPauses + ((decimal)m.RecoModel.NbRepsPauses * (decimal)0.1));
                                    IsMaxChallenge = true;


                                    LocalDBManager.Instance.SetDBSetting($"{CurrentLog.Instance.CurrentWorkoutTemplate.Id}Challenge{m.BodyPartId}", $"{m.BodyPartId}");
                                }
                                catch (Exception ex)
                                {

                                }
                                _firebase.LogEvent("challenge_time", "Yes");
                            }
                            else
                            {
                                LocalDBManager.Instance.SetDBSetting($"IsAskedChallenge{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}", "true");
                                IsMaxChallenge = false;
                                m.RecoModel.IsMaxChallenge = false;
                                RecoContext.SaveContexts("Reco" + exId + setStyle, m.RecoModel);
                                LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
                                if (LocalDBManager.Instance.GetDBSetting("timer_autoset") == null)
                                    LocalDBManager.Instance.SetDBSetting("timer_autoset", "true");

                            }

                        }
                        if (sessionDays != null)
                        {
                            if (LocalDBManager.Instance.GetDBSetting($"IsAskedAddExtraSet{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}") != null && LocalDBManager.Instance.GetDBSetting($"IsAskedAddExtraSet{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}").Value == "true")
                                CurrentLog.Instance.IsAskedAddExtraSet = true;
                            else
                                CurrentLog.Instance.IsAskedAddExtraSet = false;
                            if (sessionDays >= 5 && sessionDays <= 9 && !_isAskedforDeload && !CurrentLog.Instance.IsAskedAddExtraSet)
                            {
                                var bodyPartname = m.BodyPartId == 1 ? "" : Constants.AppThemeConstants.GetBodyPartName(m.BodyPartId);
                                bool isAddMoreSet = true;
                                ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                                {
                                    Message = string.Format("{0} {1} {2} {3} {4}", AppResources.TheLastTimeYouDid, string.IsNullOrEmpty(bodyPartname) == false ? bodyPartname.ToLower() : m.Label, AppResources.was, sessionDays, AppResources.DaysAgoYouShouldBeFullyRecoveredDoExtraSet),
                                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    OkText = "+1 set",
                                    CancelText = AppResources.Cancel,
                                };
                                if (!App.IsConnectedToWatch)
                                    isAddMoreSet = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                                if (isAddMoreSet)
                                {
                                    if (!m.RecoModel.IsReversePyramid && m.RecoModel.NbPauses == 0)
                                        m.RecoModel.Series += 1;
                                    else
                                        m.RecoModel.NbPauses += 1;
                                }
                                else
                                    LocalDBManager.Instance.SetDBSetting($"IsAskedAddExtraSet{CurrentLog.Instance.CurrentWorkoutTemplate.Id}_{m.BodyPartId}", "true");
                            }
                        }
                        if (CurrentLog.Instance.ExerciseLog == null)
                        {
                            CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
                        }
                        CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(m);
                        if (CurrentLog.Instance.RecommendationsByExercise.ContainsKey(CurrentLog.Instance.ExerciseLog.Exercice.Id))
                            CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id] = m.RecoModel;
                        else
                            CurrentLog.Instance.RecommendationsByExercise.Add(CurrentLog.Instance.ExerciseLog.Exercice.Id, m.RecoModel);

                        if (sessionDays > 9)
                        {
                            lbl3text = "Light session";
                            iconOrange = "orange.png";
                        }
                        else
                            iconOrange = "";

                        if (m.RecoModel.IsDeload)
                        {
                            LocalDBManager.Instance.SetDBSetting("RecoDeload", "true");
                            lbl3text = "Deload";
                            iconOrange = "orange.png";
                        }
                        else if (m.RecoModel.IsMaxChallenge)
                        {
                            LocalDBManager.Instance.SetDBSetting("RecoDeload", "false");
                            lbl3text = "Challenge";
                            iconOrange = "done2.png";
                        }
                        else
                        {
                            LocalDBManager.Instance.SetDBSetting("RecoDeload", "false");
                            if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                            {
                                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                                {
                                    lbl3text = "Challenge";
                                    iconOrange = "done2.png";
                                }
                            }
                        }
                        if (m.Count > 0)
                        {
                            m.Clear();
                            return;
                        }
                        bool isLowerWeightNotPossible = false;

                        List<WorkoutLogSerieModelRef> setList = new List<WorkoutLogSerieModelRef>();


                        if (m.IsUnilateral)
                            m.IsFirstSide = true;

                        if (m.RecoModel.WarmUpsList.Count > 0)
                        {

                            for (int i = 0; i < m.RecoModel.WarmUpsList.Count; i++)
                            {

                                setList.Add(new WorkoutLogSerieModelRef()
                                {
                                    Id = m.Id,
                                    Weight = !RecoComputation.IsWeightedExercise(m.Id) && m.RecoModel.WarmUpsList[i].WarmUpWeightSet.Kg < 2 ? new MultiUnityWeight(RecoComputation.RoundToNearestIncrement((decimal)2 * (m.RecoModel.Increments == null ? 2 : m.RecoModel.Increments.Kg), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg), "kg") : m.RecoModel.WarmUpsList[i].WarmUpWeightSet,
                                    IsWarmups = true,
                                    Reps = m.RecoModel.WarmUpsList[i].WarmUpReps,
                                    SetNo = $"SET {setList.Count + 1}",
                                    IsLastWarmupSet = i == m.RecoModel.WarmUpsList.Count - 1 ? true : false,
                                    IsHeaderCell = i == 0 ? true : false,
                                    ShowWorkTimer = i == 0 && m.IsTimeBased ? true : false,
                                    HeaderImage = iconOrange,
                                    HeaderTitle = lbl3text,
                                    ExerciseName = m.Label,
                                    IsFlexibility = m.IsFlexibility,
                                    EquipmentId = m.EquipmentId,
                                    Increments = m.RecoModel.Increments,
                                    Min = m.RecoModel.Min,
                                    Max = m.RecoModel.Max,
                                    SetTitle = i == 0 ? "Let's warm up:" : "",
                                    IsTimeBased = m.IsTimeBased,
                                    IsUnilateral = m.IsUnilateral,
                                    IsBodyweight = m.IsBodyweight,
                                    IsNormalset = m.IsNormalSets,
                                    BodypartId = m.BodyPartId,
                                    IsJustSetup = ShowWarmups,
                                    VideoUrl = i == 0 ? m.LocalVideo : null// i == 0 ? string.IsNullOrEmpty(m.LocalVideo) ? null :  FormsVideoLibrary.VideoSource.FromResource(m.LocalVideo) : null
                                });
                                setList.Last().Weight = new MultiUnityWeight(Convert.ToDecimal(setList.Last().WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb");
                                if (m.RecoModel.HistorySet != null)
                                {
                                    if (m.RecoModel.HistorySet.Where(x => x.IsWarmups).Count() > i)
                                    {
                                        var r = m.RecoModel.HistorySet.Where(x => x.IsWarmups).ToList()[i];
                                        var WeightText = m.IsBodyweight ? "body" : $"{ string.Format("{0:0.##}", Math.Round((isKg ? r.Weight.Kg : r.Weight.Lb), 2))} {(isKg ? "kg" : "lbs")}";
                                        if (m.Id == 16508)
                                        {
                                            WeightText = setList.Last().IsWarmups ? "brisk" : "fast";
                                        }
                                        else if (m.BodyPartId == 12)
                                        {
                                            WeightText = setList.Last().IsWarmups ? "brisk" : "cooldown";

                                        }
                                        if (m.Id >= 16897 && m.Id <= 16907 || m.Id == 14279 || m.Id >= 21508 && m.Id <= 21514)
                                        {
                                            WeightText = "bands";
                                        }



                                        if (r.IsWarmups)
                                        {
                                            setList.Last().LastTimeSet = $"Last time: {r.Reps} x {WeightText}";
                                            //TODO:
                                        }
                                    }
                                }

                            }
                            if (setList.Count > 1)
                            {
                                setList.Last().SetTitle = "Last warm-up set:";
                            }
                        }
                        bool isMarkFirstSet = false;
                        for (int j = 0; j < m.RecoModel.Series; j++)
                        {

                            isMarkFirstSet = true;
                            var rec = new WorkoutLogSerieModelRef()
                            {
                                Id = m.Id,
                                Weight = m.RecoModel.Weight.Kg < 2 ? new MultiUnityWeight(RecoComputation.RoundToNearestIncrement((decimal)2 * (m.RecoModel.Increments == null ? 2 : m.RecoModel.Increments.Kg), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg), "kg") : m.RecoModel.Weight,
                                IsWarmups = false,
                                Reps = m.RecoModel.Reps,
                                SetNo = $"SET {setList.Count + 1}",
                                ExerciseName = m.Label,
                                EquipmentId = m.EquipmentId,
                                IsFirstWorkSet = j == 0 ? true : false,
                                Increments = m.RecoModel.Increments,
                                SetTitle = j == 0 ? IsMaxChallenge ? "Try max reps today:" : "1st work set—you got this:" : "All right! Now let's try:",
                                IsTimeBased = m.IsTimeBased,
                                IsUnilateral = m.IsUnilateral,
                                IsFlexibility = m.IsFlexibility,
                                IsBodyweight = m.IsBodyweight,
                                IsNormalset = m.IsNormalSets,
                                IsFirstSide = m.IsFirstSide,
                                Min = m.RecoModel.Min,
                                Max = m.RecoModel.Max,
                                IsMaxChallenge = j == 0 ? IsMaxChallenge : false,
                                BodypartId = m.BodyPartId
                            };
                            rec.Weight = new MultiUnityWeight(Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb");
                            if (j == 0 && m.RecoModel.IsReversePyramid)
                            {
                                rec.SetTitle = IsMaxChallenge ? "Try max reps today:" : "Last work set—you got this:";
                            }
                            if (j == 0 && m.RecoModel.FirstWorkSetWeight != null)
                            {
                                var worksets = string.Format("{0:0.##} {1}", Math.Round(isKg ? m.RecoModel.FirstWorkSetWeight.Kg : m.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");
                                var days = 0;
                                if (m.RecoModel.LastLogDate != null)
                                    days = (DateTime.Now - ((DateTime)m.RecoModel.LastLogDate).ToLocalTime()).Days;
                                var dayString = days == 0 ? "Today" : days == 1 ? "1 day ago" : $"{days} days ago";
                                if (m.RecoModel.IsBodyweight)
                                    worksets = "body";





                                var lastOneRM = m.RecoModel.FirstWorkSetWeight.Kg;//
                                lastOneRM = ComputeOneRM(new MultiUnityWeight(isKg ? m.RecoModel.FirstWorkSetWeight.Kg : m.RecoModel.FirstWorkSetWeight.Lb, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(m.Id) ? _userBodyWeight : 0), m.RecoModel.FirstWorkSetReps);






                                if (!m.RecoModel.IsBodyweight && RecoComputation.IsWeightedExercise(m.Id) && !m.RecoModel.IsDeload)
                                {
                                    m.RecoModel.Reps = RecoComputation.ComputeReps(new MultiUnityWeight(new MultiUnityWeight(Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb").Kg + _userBodyWeight, "kg"), m.RecoModel.FirstWorkSet1RM.Kg) + 1;
                                    if (m.RecoModel.Reps <= 0)
                                        m.RecoModel.Reps = 1;
                                    rec.Reps = m.RecoModel.Reps;
                                }
                                else if (!m.RecoModel.IsBodyweight && !m.RecoModel.IsDeload)
                                {
                                    m.RecoModel.Reps = RecoComputation.ComputeReps(new MultiUnityWeight(Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb"), m.RecoModel.FirstWorkSet1RM.Kg) + 1;
                                    if (m.RecoModel.Reps <= 0)
                                        m.RecoModel.Reps = 1;
                                    rec.Reps = m.RecoModel.Reps;
                                }

                                decimal weight = 0;

                                if (m.IsBodyweight)
                                    weight = isKg ? m.RecoModel.FirstWorkSetWeight.Kg : Convert.ToDecimal(m.RecoModel.FirstWorkSetWeight.Lb, CultureInfo.InvariantCulture);
                                else
                                {
                                    weight = Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                }


                                var newWeight = Math.Round(new MultiUnityWeight(weight, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(m.Id) ? _userBodyWeight : 0), 2);
                                var currentRM = TruncateDecimal(ComputeOneRM(newWeight, m.RecoModel.Reps), 2);
                                lastOneRM = Math.Round(isKg ? new MultiUnityWeight(lastOneRM, "kg").Kg : new MultiUnityWeight(lastOneRM, "kg").Lb, 1);
                                currentRM = Math.Round(isKg ? new MultiUnityWeight(currentRM, "kg").Kg : new MultiUnityWeight(currentRM, "kg").Lb, 1);


                                if (currentRM != 0)
                                {
                                    var percentage = (currentRM - lastOneRM) * 100 / lastOneRM;
                                    rec.LastTimeSet = string.Format("Last time: {0} x {1}", m.RecoModel.FirstWorkSetReps, worksets);
                                    rec.SetTitle = string.Format("For {0}{1:0.0}% do:", percentage >= 0 ? "+" : "", percentage);
                                }
                            }
                            if (j > 0 && m.RecoModel.IsPyramid)
                            {
                                rec.Reps = setList.Last().Reps + j + 1;

                                var lstkgWeight = Convert.ToDecimal(setList.Last().WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                decimal weight = RecoComputation.RoundToNearestIncrementPyramid(lstkgWeight - (lstkgWeight * ((decimal)0.1)), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);

                                if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
                                {
                                    var inc = m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg;
                                    if (SaveSetPage.RoundDownToNearestIncrement(weight, inc, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg) >= SaveSetPage.RoundDownToNearestIncrement(setList.Last().Weight.Kg, inc, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg))
                                    {
                                        weight = RecoComputation.RoundToNearestIncrementPyramid(setList.Last().Weight.Kg - (m.RecoModel.Increments != null ? m.RecoModel.Increments.Kg : (setList.Last().Weight.Kg * (decimal)0.1)), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);
                                        if (weight == setList.Last().Weight.Kg)
                                        {
                                            rec.Reps = setList.Last().Reps;
                                            isLowerWeightNotPossible = true;
                                        }
                                        //else
                                        //    updatingItem.Reps = reps;
                                    }
                                    //else
                                    //updatingItem.Reps = reps;
                                    rec.Weight = new MultiUnityWeight(weight, "kg");
                                }
                                else
                                {
                                    var lstWeight = Convert.ToDecimal(setList.Last().WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                    var inc = m.RecoModel.Increments != null ? Math.Round(m.RecoModel.Increments.Lb, 2) : (decimal)5;
                                    weight = RecoComputation.RoundToNearestIncrementPyramid(lstWeight - lstWeight * ((decimal)0.1), inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb);


                                    if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "lb").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb) >= SaveSetPage.RoundDownToNearestIncrement(setList.Last().Weight.Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb))
                                    {

                                        weight = RecoComputation.RoundToNearestIncrementPyramid(setList.Last().Weight.Lb - (m.RecoModel.Increments != null ? Math.Round(m.RecoModel.Increments.Lb, 2) : (setList.Last().Weight.Lb * ((decimal)0.1))), inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb);
                                        if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "lb").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb) == SaveSetPage.RoundDownToNearestIncrement(rec.Weight.Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb))
                                        {
                                            rec.Reps = setList.Last().Reps; //
                                            isLowerWeightNotPossible = true;
                                        }
                                        //else
                                        //{
                                        //    updatingItem.Reps = reps;
                                        //}
                                        weight = new MultiUnityWeight(weight, "lb").Kg;
                                        if (weight == 0)
                                            weight = isKg ? 2 : 5;
                                        rec.Weight = new MultiUnityWeight(weight, isKg ? "kg" : "lb");
                                        if (rec.WeightDouble == setList.Last().WeightDouble)
                                        {
                                            rec.Reps = setList.Last().Reps;
                                            isLowerWeightNotPossible = true;
                                        }
                                    }
                                    else
                                        rec.Weight = new MultiUnityWeight(SaveSetPage.RoundDownToNearestIncrementLb(weight, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb), "lb");

                                    if (rec.WeightDouble == setList.Last().WeightDouble)
                                    {
                                        rec.Reps = setList.Last().Reps;
                                        isLowerWeightNotPossible = true;
                                    }
                                }
                                if (weight <= 0)
                                {
                                    rec.Reps = setList.Last().Reps;
                                    weight = m.RecoModel.Increments != null ? m.RecoModel.Increments.Kg : (decimal)2; ;
                                    if (setList.Last().Weight.Kg > (decimal)1.15)
                                        rec.Reps = setList.Last().Reps + j + 1;
                                    rec.Weight = new MultiUnityWeight(weight, "kg");
                                    if (rec.WeightDouble == setList.Last().WeightDouble)
                                        rec.Reps = setList.Last().Reps;
                                    isLowerWeightNotPossible = true;
                                }

                            }

                            if (setList.Count == 0)
                            {
                                rec.IsHeaderCell = true;
                                rec.ShowWorkTimer = m.IsTimeBased ? true : false;
                                rec.HeaderImage = iconOrange;
                                rec.HeaderTitle = lbl3text;
                                if (!string.IsNullOrEmpty(m.LocalVideo))
                                    rec.VideoUrl = m.LocalVideo;// FormsVideoLibrary.VideoSource.FromResource(m.VideoUrl);
                            }
                            if (!rec.IsFirstWorkSet)
                            {
                                if (m.RecoModel.HistorySet != null)
                                {
                                    var workcount = m.RecoModel.HistorySet.Where(x => x.IsWarmups == false).ToList();
                                    if (workcount.Count() > j)
                                    {
                                        var r = workcount[j];
                                        var WeightText = m.IsBodyweight ? "body" : $"{string.Format("{0:0.##}", Math.Round((isKg ? r.Weight.Kg : r.Weight.Lb), 2))} {(isKg ? "kg" : "lbs")}";


                                        if (m.Id == 16508)
                                        {
                                            WeightText = rec.IsWarmups ? "brisk" : "fast";
                                        }
                                        else if (m.BodyPartId == 12)
                                        {
                                            WeightText = rec.IsWarmups ? "brisk" : "cooldown";

                                        }
                                        if (m.Id >= 16897 && m.Id <= 16907 || m.Id == 14279 || m.Id >= 21508 && m.Id <= 21514)
                                        {
                                            WeightText = "bands";
                                        }
                                        if (!r.IsWarmups)
                                        {
                                            rec.LastTimeSet = $"Last time: {r.Reps} x {WeightText}";
                                        }
                                    }
                                }
                            }
                            setList.Add(rec);
                        }

                        for (int j = 0; j < m.RecoModel.NbPauses; j++)
                        {

                            var rec = new WorkoutLogSerieModelRef()
                            {
                                Id = m.Id,
                                Weight = m.RecoModel.Weight.Kg < 2 ? new MultiUnityWeight(RecoComputation.RoundToNearestIncrement((decimal)2 * (m.RecoModel.Increments == null ? 2 : m.RecoModel.Increments.Kg), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg), "kg") : m.RecoModel.Weight,
                                IsWarmups = false,
                                Reps = m.RecoModel.NbRepsPauses,
                                SetNo = $"SET {setList.Count + 1}",
                                ExerciseName = m.Label,
                                EquipmentId = m.EquipmentId,
                                IsFlexibility = m.IsFlexibility,
                                Increments = m.RecoModel.Increments,
                                SetTitle = "All right! Now let's try:",
                                IsTimeBased = m.IsTimeBased,
                                IsUnilateral = m.IsUnilateral,
                                IsBodyweight = m.IsBodyweight,
                                IsNormalset = m.IsNormalSets,
                                IsFirstSide = m.IsFirstSide,
                                Min = m.RecoModel.Min,
                                Max = m.RecoModel.Max,
                                BodypartId = m.BodyPartId

                            };
                            rec.Weight = new MultiUnityWeight(Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb");

                            if (!isMarkFirstSet && j == 0)
                            {
                                rec.IsFirstWorkSet = true;
                                rec.SetTitle = IsMaxChallenge ? "Try max reps today:" : "1st work set—you got this:";
                                rec.IsMaxChallenge = j == 0 ? IsMaxChallenge : false;
                            }

                            if (!isMarkFirstSet && j == 0 && m.RecoModel.FirstWorkSetWeight != null)
                            {

                                var worksets = string.Format("{0:0.##} {1}", Math.Round(isKg ? m.RecoModel.FirstWorkSetWeight.Kg : m.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");
                                var days = 0;
                                if (m.RecoModel.LastLogDate != null)
                                    days = (DateTime.Now - ((DateTime)m.RecoModel.LastLogDate).ToLocalTime()).Days;
                                var dayString = days == 0 ? "Today" : days == 1 ? "1 day ago" : $"{days} days ago";
                                if (m.RecoModel.IsBodyweight)
                                    worksets = "body";
                                rec.SetTitle = $"{dayString}: {m.RecoModel.FirstWorkSetReps} x {worksets}\nLet's try:";

                                var lastOneRM = ComputeOneRM(new MultiUnityWeight(isKg ? m.RecoModel.FirstWorkSetWeight.Kg : m.RecoModel.FirstWorkSetWeight.Lb, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(m.Id) ? _userBodyWeight : 0), m.RecoModel.FirstWorkSetReps);
                                if (!m.RecoModel.IsBodyweight && RecoComputation.IsWeightedExercise(m.Id))
                                {
                                    m.RecoModel.Reps = RecoComputation.ComputeReps(new MultiUnityWeight(new MultiUnityWeight(Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb").Kg + _userBodyWeight, "kg"), m.RecoModel.FirstWorkSet1RM.Kg) + 1;
                                    if (m.RecoModel.Reps <= 0)
                                        m.RecoModel.Reps = 1;
                                    rec.Reps = m.RecoModel.Reps;
                                }
                                else if (!m.RecoModel.IsBodyweight)
                                {
                                    m.RecoModel.Reps = RecoComputation.ComputeReps(new MultiUnityWeight(Convert.ToDecimal(rec.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture), isKg ? "kg" : "lb"), m.RecoModel.FirstWorkSet1RM.Kg) + 1;
                                    if (m.RecoModel.Reps <= 0)
                                        m.RecoModel.Reps = 1;
                                    rec.Reps = m.RecoModel.Reps;
                                }

                                var currentRM = ComputeOneRM(new MultiUnityWeight(isKg ? m.RecoModel.Weight.Kg : m.RecoModel.Weight.Lb, isKg ? "kg" : "lb").Kg + (RecoComputation.IsWeightedExercise(m.Id) ? _userBodyWeight : 0), rec.Reps);

                                if (currentRM != 0)
                                {
                                    var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                                    rec.LastTimeSet = string.Format("Last time: {0} x {1}", m.RecoModel.FirstWorkSetReps, worksets);
                                    rec.SetTitle = string.Format("For {0}{1:0.0}% do:", percentage >= 0 ? "+" : "", percentage);
                                }
                            }


                            if (m.RecoModel.IsReversePyramid)
                            {
                                ////TODO: Reverse Pyramid

                                rec.Reps = j == 0 ? m.RecoModel.Reps : (int)Math.Ceiling(setList.First(x => x.IsWarmups == false).Reps + (setList.First(x => x.IsWarmups == false).Reps * 0.4));

                                if (j > 0)
                                {
                                    var first = setList.First(x => x.IsWarmups == false);
                                    decimal weight = RecoComputation.RoundToNearestIncrementPyramid(rec.Weight.Kg - (rec.Weight.Kg * ((decimal)j * (m.RecoModel.Increments == null ? (decimal)0.1 : (decimal)0.1))), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);
                                    if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
                                    {
                                        if (weight >= setList.First(x => x.IsWarmups == false).Weight.Kg)
                                        {
                                            weight = RecoComputation.RoundToNearestIncrementPyramid(setList.First(x => x.IsWarmups == false).Weight.Kg - (m.RecoModel.Increments != null ? m.RecoModel.Increments.Kg : (rec.Weight.Kg * ((decimal)j * (decimal)0.1))), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);
                                        }
                                        rec.Weight = new MultiUnityWeight(weight, "kg");
                                        if (rec.Reps < 1)
                                            rec.Reps = 1;
                                    }
                                    else
                                    {
                                        var inc = rec.Increments != null ? Math.Round(rec.Increments.Lb, 2) : (decimal)5;
                                        if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb) >= SaveSetPage.RoundDownToNearestIncrement(setList.First(x => x.IsWarmups == false).Weight.Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb))
                                        {
                                            weight = RecoComputation.RoundToNearestIncrementPyramid(setList.First(x => x.IsWarmups == false).Weight.Kg - (m.RecoModel.Increments != null ? m.RecoModel.Increments.Kg : (rec.Weight.Kg * ((decimal)j * (decimal)0.1))), m.RecoModel.Increments == null ? (decimal)2 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);
                                            if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb) == SaveSetPage.RoundDownToNearestIncrement(setList.First(x => x.IsWarmups == false).Weight.Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb))
                                            {
                                                rec.Reps = setList.Last().Reps;
                                                isLowerWeightNotPossible = true;
                                            }
                                        }
                                        rec.Weight = new MultiUnityWeight(SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb), "lb");
                                    }
                                    if (weight == 0)
                                    {
                                        //rec.Reps = setList.First(x => x.IsWarmups == false).Reps;
                                        weight = rec.Increments != null ? rec.Increments.Kg : (decimal)2;

                                        rec.Weight = new MultiUnityWeight(weight, "kg");
                                    }
                                    if (first.WeightDouble == rec.WeightDouble)
                                    {
                                        rec.Reps = first.Reps;
                                        isLowerWeightNotPossible = true;
                                    }

                                    if (rec.Reps < 1)
                                        rec.Reps = 1;
                                }

                                ////TODO: Reverse Pyramid

                            }
                            else
                            {


                                if (j > 0 && m.RecoModel.IsPyramid)
                                {
                                    rec.Reps = setList.Last().Reps + j + 1;
                                    decimal weight = RecoComputation.RoundToNearestIncrementPyramid(rec.Weight.Kg - (rec.Weight.Kg * ((decimal)j * (m.RecoModel.Increments == null ? (decimal)0.1 : (decimal)0.1))), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);
                                    if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
                                    {
                                        if (weight >= setList.Last().Weight.Kg)
                                        {
                                            weight = RecoComputation.RoundToNearestIncrementPyramid(setList.Last().Weight.Kg - (m.RecoModel.Increments != null ? m.RecoModel.Increments.Kg : (rec.Weight.Kg * ((decimal)j * (decimal)0.1))), m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);
                                            if (weight == setList.Last().Weight.Kg)
                                            {
                                                rec.Reps = setList.Last().Reps;
                                                isLowerWeightNotPossible = true;
                                            }
                                        }
                                        rec.Weight = new MultiUnityWeight(weight, "kg");
                                    }
                                    else
                                    {
                                        var inc = rec.Increments != null ? rec.Increments.Lb : (decimal)5;
                                        if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb) >= SaveSetPage.RoundDownToNearestIncrement(setList.Last().Weight.Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb))
                                        {
                                            weight = RecoComputation.RoundToNearestIncrementPyramid(setList.Last().Weight.Kg - (m.RecoModel.Increments != null ? m.RecoModel.Increments.Kg : (rec.Weight.Kg * ((decimal)j * (decimal)0.1))), m.RecoModel.Increments == null ? (decimal)2 : m.RecoModel.Increments.Kg, m.RecoModel.Min?.Kg, m.RecoModel.Max?.Kg);
                                            if (SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb) == SaveSetPage.RoundDownToNearestIncrement(setList.Last().Weight.Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb))
                                            {
                                                rec.Reps = setList.Last().Reps;
                                                isLowerWeightNotPossible = true;
                                            }
                                        }
                                        rec.Weight = new MultiUnityWeight(SaveSetPage.RoundDownToNearestIncrement(new MultiUnityWeight(weight, "kg").Lb, inc, m.RecoModel.Min?.Lb, m.RecoModel.Max?.Lb), "lb");
                                    }
                                    if (weight <= 0)
                                    {
                                        rec.Reps = setList.Last().Reps;
                                        weight = rec.Increments != null ? rec.Increments.Kg : (decimal)1; ; ;
                                        if (setList.Last().Weight.Kg > (decimal)1.15)
                                            rec.Reps = setList.Last().Reps + j + 1;
                                        rec.Weight = new MultiUnityWeight(weight, "kg");
                                    }
                                }
                            }
                            if (setList.Count == 0)
                            {
                                rec.IsHeaderCell = true;
                                rec.ShowWorkTimer = m.IsTimeBased ? true : false;
                                rec.HeaderImage = iconOrange;
                                rec.HeaderTitle = lbl3text;
                                if (!string.IsNullOrEmpty(m.LocalVideo) && !m.RecoModel.IsReversePyramid)
                                    rec.VideoUrl = m.LocalVideo;// FormsVideoLibrary.VideoSource.FromResource(m.VideoUrl);
                            }
                            if (!rec.IsFirstWorkSet)
                            {
                                if (m.RecoModel.HistorySet != null)
                                {
                                    var workcount = m.RecoModel.HistorySet.Where(x => x.IsWarmups == false).ToList();
                                    if (workcount.Count() > j)
                                    {
                                        var r = workcount[j];
                                        var WeightText = m.IsBodyweight ? "body" : $"{string.Format("{0:0.##}", Math.Round((isKg ? r.Weight.Kg : r.Weight.Lb), 2))} {(isKg ? "kg" : "lbs")}";


                                        if (m.Id == 16508)
                                        {
                                            WeightText = rec.IsWarmups ? "brisk" : "fast";
                                        }
                                        else if (m.BodyPartId == 12)
                                        {
                                            WeightText = rec.IsWarmups ? "brisk" : "cooldown";
                                        }
                                        if (m.Id >= 16897 && m.Id <= 16907 || m.Id == 14279 || m.Id >= 21508 && m.Id <= 21514)
                                        {
                                            WeightText = "bands";
                                        }
                                        if (!r.IsWarmups)
                                        {
                                            rec.LastTimeSet = $"Last time: {r.Reps} x {WeightText}";
                                            //TODO:
                                        }
                                    }
                                }
                            }

                            rec.PreviousReps = rec.Reps;
                            rec.PreviousWeight = rec.Weight;
                            if (m.RecoModel.IsReversePyramid)
                                setList.Insert(setList.Count(x => x.IsWarmups), rec);
                            else
                                setList.Add(rec);
                        }
                        if (m.RecoModel.IsReversePyramid && setList.Count(x => x.IsWarmups) == 0)
                        {
                            if (!string.IsNullOrEmpty(m.LocalVideo) && setList.FirstOrDefault() != null)
                                setList.First().VideoUrl = m.LocalVideo;
                            setList.Last().IsHeaderCell = false;
                            setList.First().IsHeaderCell = true;
                            setList.First().HeaderImage = iconOrange;
                            setList.First().HeaderTitle = lbl3text;
                        }
                        var worksetcount = (setList.Count - m.RecoModel.WarmUpsList.Count);
                        if (worksetcount > 2 && !m.IsBodyweight)
                        {
                            if (m.RecoModel.BackOffSetWeight != null && !m.RecoModel.IsPyramid)
                            {
                                decimal wei = Convert.ToDecimal(setList.Last().WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                                var inc = isKg ? m.RecoModel.Increments == null ? (decimal)2.0 : m.RecoModel.Increments.Kg : m.RecoModel.Increments == null ? (decimal)5.0 : Math.Round(m.RecoModel.Increments.Lb);

                                m.RecoModel.BackOffSetWeight = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement((decimal)wei - wei * (decimal)0.3, inc, isKg ? m.RecoModel.Min?.Kg : m.RecoModel.Min?.Lb, isKg ? m.RecoModel.Max?.Kg : m.RecoModel.Max?.Lb), isKg ? "kg" : "lb");

                                setList.Last().Weight = m.RecoModel.BackOffSetWeight.Kg < 2 ? new MultiUnityWeight(RecoComputation.RoundToNearestIncrement((decimal)2 * inc, inc, isKg ? m.RecoModel.Min.Kg : m.RecoModel.Min?.Lb, isKg ? m.RecoModel.Max?.Kg : m.RecoModel.Max?.Lb), isKg ? "kg" : "lb") : m.RecoModel.BackOffSetWeight;

                                //if (m.RecoModel.Weight.Kg != m.RecoModel.BackOffSetWeight.Kg)
                                //{
                                if (Math.Abs(m.RecoModel.Weight.Kg - m.RecoModel.BackOffSetWeight.Kg) > 0)
                                {
                                    var ob = ((Math.Abs(m.RecoModel.Weight.Kg - m.RecoModel.BackOffSetWeight.Kg) / (m.RecoModel.Weight.Kg == 0 ? 2 : m.RecoModel.Weight.Kg)) > (decimal)0.3 ? (decimal)0.3 * (decimal)3 : Math.Abs(m.RecoModel.Weight.Kg - m.RecoModel.BackOffSetWeight.Kg) / (m.RecoModel.Weight.Kg == 0 ? 2 : m.RecoModel.Weight.Kg) * (decimal)3);
                                    setList.Last().Reps = (int)setList.Last().Reps + (int)Math.Ceiling(setList.Last().Reps * ob);
                                }
                                else
                                {
                                    setList.Last().Reps = (int)(setList.Last().Reps + Math.Ceiling(setList.Last().Reps * 0.2));
                                }
                                //setList.Last().Reps = (int)(setList.Last().Reps + Math.Ceiling(setList.Last().Reps * 1.1));
                                //}
                                //else
                                //{
                                //    //
                                //}
                                setList.Last().IsBackOffSet = true;
                                setList[setList.Count - 2].IsNextBackOffSet = true;

                            }
                        }
                        if (worksetcount > 3)
                        {
                            setList[setList.Count - 2].SetTitle = "Almost done—keep it up!";
                            setList.Last().SetTitle = "Last set—you can do this!";
                        }
                        else if (worksetcount > 2)
                        {
                            setList.Last().SetTitle = "Last set—you can do this!";
                        }
                        if (setList.First().IsWarmups)
                        {
                            var warmString = setList.Where(l => l.IsWarmups).ToList().Count < 2 ? "warm-up" : "warm-ups";
                            setList.First().SetTitle = $"{setList.Where(l => l.IsWarmups).ToList().Count} {warmString}, {setList.Where(l => !l.IsWarmups).ToList().Count} work sets\nLet's warm up:";
                        }
                        var selected = setList.Where(x => x.IsNext == true).FirstOrDefault();

                        if (selected == null && setList.Count > 0)
                        {
                            setList.First().IsNext = true;
                            App.PCWeight = Convert.ToDecimal(setList.First().WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                            try
                            {
                                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = setList.First(), Label = m.Label }, "SendWatchMessage");
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        else
                        {
                            //Get index and set
                        }
                        if (setList.Count > 0)
                        {
                            setList.Last().IsLastSet = true;
                            if (m.IsFirstSide)
                                setList.Last().IsFirstSide = true;
                        }
                        for (var i = 0; i < setList.Count; i++)
                            ((WorkoutLogSerieModelRef)setList[i]).SetNo = $"SET {i + 1}/{setList.Count}";
                        foreach (var item in setList)
                        {
                            m.Add(item);
                        }
                        ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                        if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                        {
                            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[m.Id] = new ObservableCollection<WorkoutLogSerieModelRef>(setList);
                        }
                        else
                        {
                            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Add(m.Id, new ObservableCollection<WorkoutLogSerieModelRef>(setList));
                        }
                        //TODO: When Exercise quick mode on fetch reco of next exercise
                        if (LocalDBManager.Instance.GetDBSetting("IsExerciseQuickMode")?.Value == "true")
                        {
                            var exModel = exerciseItems.FirstOrDefault(x => !x.IsNextExercise && x.Id > 0 && !x.IsFinished);
                            if (exModel != null)
                                FetchNextExerciseBackgroundData(exModel);
                        }

                        //lblResult4.Text = string.Format("Do {0} {1} for {2} sets of {3} reps ({4} rest)", WeightRecommandation, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", reco.Series, reco.Reps, restTime);

                        if (m.RecoModel.IsDeload)
                        {
                            LocalDBManager.Instance.SetDBSetting("RecoDeload", "true");
                        }
                        else
                            LocalDBManager.Instance.SetDBSetting("RecoDeload", "false");
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            if (setList.Count > 0)
                            {
                                ScrollToSnap(setList, m);
                            }
                        });
                        if (ShowWarmups && !Config.ShowWarmups)
                        {
                            ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                            {
                                Title = $"Start by warming up",
                                Message = "Repeat a few times—slow and steady—and tap Save set.",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = AppResources.GotIt,
                                CancelText = AppResources.RemindMe,

                            };
                            var x = await UserDialogs.Instance.ConfirmAsync(ShowRIRPopUp);
                            if (x)
                            {
                                Config.ShowWarmups = true;
                            }
                        }
                        if (isLowerWeightNotPossible && (m.IsPyramid || m.IsReversePyramid))
                        {
                            var msg = "";
                            if (m.RecoModel.Min != null)
                            {
                                var incr = string.Format("{0:0.##}", isKg ? Math.Round(m.RecoModel.Min.Kg, 2) : Math.Round(m.RecoModel.Min.Lb, 2));

                                msg = $"You have a min weight of {(isKg ? incr + " kg" : incr + " lbs")}, so you cannot do {(m.IsReversePyramid ? "pyramid" : "reverse pyramid")} sets. Edit weights or try rest-pause sets today.";
                            }
                            else
                            {

                                msg = $"Your weights cannot go below {(isKg ? "2 kg" : "5 lbs")}, so you cannot do {(m.IsReversePyramid ? "pyramid" : "reverse pyramid")} sets. Edit weights or try rest-pause sets today.";
                            }
                            ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                            {
                                Title = $"Cannot lower weights",
                                Message = msg,
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Try rest-pause",
                                CancelText = "Edit weights",

                            };
                            var x = await UserDialogs.Instance.ConfirmAsync(ShowRIRPopUp);
                            if (!x)
                            {
                                CurrentLog.Instance.AutoEnableIncrements = true;
                                CurrentLog.Instance.WorkoutTemplateCurrentExercise = GetExerciseModel(m);
                                
                                IsSettingsChanged = true;
                                return;
                            }
                            else
                            {
                                m.RecoModel.IsReversePyramid = false;
                                m.RecoModel.IsPyramid = false;
                                m.IsPyramid = m.IsReversePyramid = false;
                                if (m.RecoModel.Series == 0)
                                {
                                    m.RecoModel.Series = 1;
                                    m.RecoModel.NbPauses = m.RecoModel.NbPauses - 1;
                                    m.RecoModel.NbRepsPauses = m.RecoModel.Reps / 3;
                                }
                                else
                                {
                                    var left = m.RecoModel.Series - 1;
                                    m.RecoModel.Series = 1;
                                    m.RecoModel.NbPauses = left;
                                    m.RecoModel.NbRepsPauses = m.RecoModel.Reps / 3;
                                }
                                RecoContext.SaveContexts("Reco" + exId + setStyle, m.RecoModel);
                                LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + exId + setStyle, DateTime.Now.AddDays(1).ToString());
                                m.Clear();
                                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                                FetchReco(m);
                                return;
                            }
                        }

                        //Reps outside your range
                        if (m.RecoModel.Reps < m.RecoModel.MinReps || m.RecoModel.Reps > m.RecoModel.MaxReps)
                        {
                            if (!Config.RepRangeOutsidePopup)
                            {
                                var incr = string.Format("{0:0.#}", m.RecoModel.Increments == null ? (isKg ? 2 : 5) : (isKg ? Math.Round(m.RecoModel.Increments.Kg, 2) : Math.Round(m.RecoModel.Increments.Lb, 2)));
                                var msg = "";
                                if (m.RecoModel.Reps < m.RecoModel.MinReps)
                                    msg = $"Reps of {m.RecoModel.Reps} are lower than your min of {m.RecoModel.MinReps} to match your {incr} {(isKg ? "kg" : "lbs")} increments.";
                                else
                                    msg = $"Reps of {m.RecoModel.Reps} are higher than your max of {m.RecoModel.MaxReps} to match your {incr} {(isKg ? "kg" : "lbs")} increments.";
                                ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                                {
                                    Title = $"Weights and reps adjusted",
                                    Message = msg,
                                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    OkText = AppResources.GotIt,
                                    CancelText = AppResources.RemindMe,

                                };
                                var x = await UserDialogs.Instance.ConfirmAsync(ShowRIRPopUp);
                                if (x)
                                {
                                    Config.RepRangeOutsidePopup = true;
                                }
                            }
                        }

                        if (m.IsFirstSide)
                        {
                            if (!m.IsPopup && !App.IsConnectedToWatch)
                            {
                                AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                                {
                                    Title = "Do all sets for side 1",
                                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    OkText = "OK",
                                };
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    UserDialogs.Instance.Alert(ShowExplainRIRPopUp);
                                });

                                m.IsPopup = true;
                            }
                        }


                    }
                }



            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (App.IsShowTooltip)
                {
                    if (App.WelcomeTooltop[0] == false)
                    {
                        await Task.Delay(2000);
                        WorkoutLogSerieModelRef i = null;
                        foreach (WorkoutLogSerieModelRef item in m)
                        {
                            if (item.IsFinished)
                                continue;
                            i = item;
                            item.ShowPlusTooltip = true;
                            App.WelcomeTooltop[0] = true;
                            break;

                        }
                        await Task.Delay(4000);
                        if (i != null)
                            i.ShowPlusTooltip = false;
                    }
                }

                if (_superSetRunning)
                {
                    await Task.Delay(2100);
                    WorkoutLogSerieModelRef i = null;
                    foreach (WorkoutLogSerieModelRef item in m)
                    {
                        //if (item.IsFinished)
                        //    continue;
                        i = item;
                        item.ShowSuperSet3 = true;
                        break;

                    }
                    await Task.Delay(4000);
                    i.ShowSuperSet3 = false;
                }

            }
        }

        public decimal ComputeOneRM(decimal weight, int reps)
        {
            return (decimal)(AppThemeConstants.Coeficent * reps) * weight + weight;
        }

        private async void ScrollToActiveSet(WorkoutLogSerieModelRef set, ExerciseWorkSetsModel m)
        {
            try
            {

                if (set != null)
                {
                    //if (Device.RuntimePlatform.Equals(Device.iOS))
                    //Device.BeginInvokeOnMainThread(async () =>
                    //    {
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        ExerciseListView.SelectedItem = set;

                    ExerciseListView.ScrollTo(set, ScrollToPosition.Start, true);

                    await Task.Delay(300);

                    ExerciseListView.ScrollTo(set, ScrollToPosition.MakeVisible, true);

                    //});
                    //return;
                    //await Task.Delay(300);
                    //int position = 0;
                    //foreach (var itemGood in exerciseItems)
                    //{
                    //    if (itemGood == m)
                    //        break;
                    //    position += 1;
                    //    position += itemGood.Count;
                    //}
                    //var index = m.IndexOf(set);

                    //ExerciseListView.ScrollTo(set, ScrollToPosition.MakeVisible, true);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void ScrollToSnap(List<WorkoutLogSerieModelRef> setList, ExerciseWorkSetsModel m)
        {
            try
            {

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(700);
                    if (setList.FirstOrDefault().IsNext && Device.RuntimePlatform == Device.Android)
                    {
                        //int position = 0;
                        //var prev = m;
                        //foreach (var itemGood in exerciseItems)
                        //{
                        //    if (itemGood == m)
                        //        break;
                        //    prev = itemGood;
                        //    position += 1;
                        //    position += itemGood.Count;
                        //}
                        //ExerciseListView.ItemPosition = position;
                        //ExerciseListView.ScrollToTop = !ExerciseListView.ScrollToTop;
                        ExerciseListView.ScrollTo(setList.First(), m, ScrollToPosition.Center, false);
                    }
                    //
                    else
                        ExerciseListView.ScrollTo(setList.First(), m, ScrollToPosition.Start, false);
                });
            }
            catch (Exception ex)
            {

            }
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();
            try
            {

                ((ViewCell)sender).Height = 115;


                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
                if (m != null && m.IsFinishWorkoutExe)
                {
                    var btn = (DrMuscleButton)((Frame)((Grid)((ViewCell)sender).View).Children[2]).FindByName("BtnFinishWorkout");
                    if (CurrentLog.Instance.IsMobilityStarted && btn != null)
                        btn.Text = "Finish & save warm-up";
                    ((ViewCell)sender).Height = 90;
                }
                var btnVideo = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[5];
                if (m != null && string.IsNullOrEmpty(m.VideoUrl))
                    btnVideo.IsVisible = false;
                else
                    btnVideo.IsVisible = true;
                if (exerciseItems.IndexOf(m) == 0 && !CurrentLog.Instance.IsMobilityStarted)
                {
                    vHeader = (Grid)((ViewCell)sender).View;
                    TooltipEffect.SetPosition(vHeader, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(vHeader, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(vHeader, Color.White);
                    TooltipEffect.SetText(vHeader, $"Tap here to start");
                    TooltipEffect.SetHasTooltip(vHeader, true);
                }
                if (m != null && vHeaders.ContainsKey(m.Id))
                {
                    vHeaders[m.Id] = (Grid)((ViewCell)sender).View;
                }
                else if (m != null)
                    vHeaders.Add(m.Id, (Grid)((ViewCell)sender).View);
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    ((ViewCell)sender).ForceUpdateSize();
                }
            }
            catch (Exception ex)
            {

            }
            //Image swapImage = (Image)((StackLayout)((StackLayout)((ViewCell)sender).View).Children[0]).Children[2];
            //if (m.IsSwapTarget)
            //{
            //    swapImage.IsVisible = true;
            //}
            //else
            //{
            //    swapImage.IsVisible = false;
            //}
        }

        private async void OnDeload(object sender, System.EventArgs e)
        {
            try
            {
                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
                bool isAccepted = true;
                if (btnDeload != null)
                {
                    TooltipEffect.SetHasTooltip(btnDeload, false);
                    TooltipEffect.SetHasShowTooltip(btnDeload, false);
                    btnDeload = null;
                    _tryDeload = false;
                    _trySwap = true;
                }

                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                string exId = $"{m.Id}";
                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") == null || LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "")
                {
                    ConfirmConfig supersetConfig = new ConfirmConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Title = "Deload",
                        Message = m.IsBodyweight ? $"2 work sets and 15-20% fewer reps. Helps you recover. Deload?" : "2 work sets and 5-10% less weight. Helps you recover. Deload?",
                        OkText = "Deload",
                        CancelText = AppResources.Cancel,

                    };
                    var res = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                    if (res)
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"deload");
                    else
                    {
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                        isAccepted = false;
                    }

                }
                else
                {
                    m.RecoModel.IsDeload = false;
                    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                }
                if (isAccepted)
                {
                    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                    {
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                        m.Clear();
                    }
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") != null)
                    {
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "deload")
                            contextMenuStack.Children[2].BackgroundColor = Color.FromHex("#72DF40");
                        else
                            contextMenuStack.Children[2].BackgroundColor = Color.FromHex("#ECFF92");
                    }

                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                    {
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                            contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#72DF40");
                        else
                            contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#ECFF92");
                    }
                    Timer.Instance.StopTimer();
                    FetchReco(m, null);
                }
                if (_trySwap && !_isAskedforSwipe)
                {
                    _isAskedforSwipe = true;
                    _trySwapMenu = true;
                    HideContextButton();
                    await Task.Delay(500);
                    //AlertConfig ShowExplainDeloadPopUp = new AlertConfig()
                    //{
                    //    Title = "Smart swap",
                    //    Message = "Don't like an exercise? Swap it for another. Tap More and Swap.",
                    //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    //    OkText = "Got it",
                    //};
                    //await UserDialogs.Instance.AlertAsync(ShowExplainDeloadPopUp);

                    try
                    {
                        btnMore = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)(vHeaders[m.Id])).Children[2]).Children[0]).Children[0]).Children[5]).Children[6];
                    }
                    catch (Exception ex)
                    {

                    }
                    TooltipEffect.SetPosition(btnMore, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(btnMore, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(btnMore, Color.White);
                    TooltipEffect.SetText(btnMore, $"Try a swap");
                    TooltipEffect.SetHasTooltip(btnMore, true);
                    TooltipEffect.SetHasShowTooltip(btnMore, true);

                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void OnChallenge(object sender, System.EventArgs e)
        {
            try
            {
                bool isAccepted = true;
                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
                if (btnChallenge != null)
                {
                    TooltipEffect.SetHasTooltip(btnChallenge, false);
                    TooltipEffect.SetHasShowTooltip(btnChallenge, false);
                    btnChallenge = null;
                    _tryDeload = true;
                }


                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                string exId = $"{m.Id}";
                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") == null || LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "")
                {
                    ConfirmConfig supersetConfig = new ConfirmConfig()
                    {
                        Title = $"Ready for a new record?",
                        Message = "Do as many reps as you can on your first work set. Stop before your form breaks down.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = AppResources.Challenge,
                        CancelText = AppResources.Cancel,

                    };
                    var res = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                    if (res)
                    {
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"max");
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
                        isAccepted = false;
                    }

                }
                else
                    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
                if (isAccepted)
                {
                    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                    {
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                        m.Clear();
                    }
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") != null)
                    {
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "deload")
                            contextMenuStack.Children[2].BackgroundColor = Color.FromHex("#72DF40");
                        else
                            contextMenuStack.Children[2].BackgroundColor = Color.FromHex("#ECFF92");
                    }

                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                    {
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                            contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#72DF40");
                        else
                            contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#ECFF92");
                    }
                    Timer.Instance.StopTimer();
                    FetchReco(m, null);
                }
                if (_tryDeload)
                {
                    HideContextButton();

                    await Task.Delay(500);
                    try
                    {
                        btnMore = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)(vHeaders[m.Id])).Children[2]).Children[0]).Children[0]).Children[5]).Children[6];

                    }
                    catch (Exception ex)
                    {

                    }
                    TooltipEffect.SetPosition(btnMore, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(btnMore, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(btnMore, Color.White);
                    TooltipEffect.SetText(btnMore, $"Try a deload");
                    TooltipEffect.SetHasTooltip(btnMore, true);
                    TooltipEffect.SetHasShowTooltip(btnMore, true);
                }

            }
            catch (Exception ex)
            {

            }
        }

        private async void OnVideo(object sender, System.EventArgs e)
        {
            //if (contextMenuStack != null)
            //    HideContextButton();
            CurrentLog.Instance.VideoExercise = GetExerciseModel(((ExerciseWorkSetsModel)((Button)sender).CommandParameter));
            if (Device.RuntimePlatform.Equals(Device.iOS))
                DependencyService.Get<IOrientationService>().Landscape();
            await PagesFactory.PushAsync<ExerciseVideoPage>(true);
            OnCancelClicked(sender, e);
        }
        private async void OnContextVideo(object sender, System.EventArgs e)
        {
            //if (contextMenuStack != null)
            //    HideContextButton();
            CurrentLog.Instance.VideoExercise = GetExerciseModel(((ExerciseWorkSetsModel)((Button)sender).CommandParameter));
            if (Device.RuntimePlatform.Equals(Device.iOS))
                DependencyService.Get<IOrientationService>().Landscape();
            await PagesFactory.PushAsync<ExerciseVideoPage>(true);

        }

        private async void OnSwap(object sender, System.EventArgs e)
        {
            try
            {
                SwapExerciseContext context = new SwapExerciseContext();
                context.WorkoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id;
                context.SourceExerciseId = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter).Id;
                context.SourceBodyPartId = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter).BodyPartId;
                ExerciseWorkSetsModel model = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter);
                context.Label = model.Label;
                context.IsBodyweight = model.IsBodyweight;
                context.IsSystemExercise = model.IsSystemExercise;
                context.IsEasy = model.IsEasy;
                context.VideoUrl = model.VideoUrl;
                context.BodyPartId = model.BodyPartId;
                context.IsUnilateral = model.IsUnilateral;
                context.IsTimeBased = model.IsTimeBased;
                CurrentLog.Instance.SwapContext = context;
                if (_trySwap)
                {
                    _trySwap = false;
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                }
                OnCancelClicked(sender, e);


            }
            catch (Exception ex)
            {

            }
        }

        private async void OnRestore(object sender, System.EventArgs e)
        {
            try
            {

                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
                SwapExerciseContext sec = ((App)Application.Current).SwapExericesContexts.Swaps.First(s => s.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && s.TargetExerciseId == m.Id);
                ((App)Application.Current).SwapExericesContexts.Swaps.Remove(sec);
                ((App)Application.Current).SwapExericesContexts.SaveContexts();
                OnCancelClicked(sender, e);

                
                await UpdateExerciseList();

            }
            catch (Exception ex)
            {

            }
        }

        public async void ResetExercisesAction(ExerciceModel model)
        {
            BooleanModel result = await DrMuscleRestClient.Instance.ResetExercise(model);
            LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + model.Id + "Normal", DateTime.Now.AddDays(-1).ToString());
            LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + model.Id + "RestPause", DateTime.Now.AddDays(-1).ToString());
        }

        public async void OnReset(object sender, EventArgs e)
        {
            var mi = ((Button)sender);

            ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)mi.CommandParameter;
            CurrentLog.Instance.WorkoutTemplateCurrentExercise = GetExerciseModel(m);
           
            IsSettingsChanged = true;
            OnCancelClicked(sender, e);
            
        }

        void HideContextButton()
        {
            try
            {

                StackLayout s1 = (StackLayout)contextMenuStack.Parent;
                s1.Children[1].IsVisible = true;
                s1.Children[2].IsVisible = true;

                ExerciseWorkSetsModel m = ((ExerciseWorkSetsModel)((Button)contextMenuStack.Children[5]).CommandParameter);
                contextMenuStack.Children[0].IsVisible = false;
                contextMenuStack.Children[1].IsVisible = false;
                contextMenuStack.Children[2].IsVisible = false;
                contextMenuStack.Children[3].IsVisible = false;
                contextMenuStack.Children[4].IsVisible = false;
                contextMenuStack.Children[5].IsVisible = !string.IsNullOrEmpty(m.VideoUrl);
                contextMenuStack.Children[6].IsVisible = true;
                contextMenuStack = null;
            }
            catch (Exception ex)
            {

            }
        }

        void OnCancelClicked(object sender, System.EventArgs e)
        {
            try
            {

                StackLayout s = ((StackLayout)((Button)sender).Parent);
                ExerciseWorkSetsModel m = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter);

                StackLayout s1 = (StackLayout)s.Parent;
                s1.Children[1].IsVisible = true;
                s1.Children[2].IsVisible = true;

                s.Children[0].IsVisible = false;
                s.Children[1].IsVisible = false;
                s.Children[2].IsVisible = false;
                s.Children[3].IsVisible = false;
                s.Children[4].IsVisible = false;
                s.Children[5].IsVisible = !string.IsNullOrEmpty(m.VideoUrl);
                s.Children[6].IsVisible = true;


            }
            catch (Exception ex)
            {

            }
        }

        async void OnContextMenuClicked(object sender, System.EventArgs e)
        {
            if (contextMenuStack != null)
                HideContextButton();
            StackLayout s = ((StackLayout)((Button)sender).Parent);
            ExerciseWorkSetsModel m = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter);
            if (m.IsNextExercise && m.Count > 0)
            {
                StackLayout s1 = (StackLayout)s.Parent;
                s1.Children[1].IsVisible = false;
                s1.Children[2].IsVisible = false;

                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                string exId = $"{ m.Id}";

                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") != null)
                {
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "deload")
                        s.Children[2].BackgroundColor = Color.FromHex("#72DF40");
                    else
                        s.Children[2].BackgroundColor = Color.FromHex("#ECFF92");
                }

                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                {
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                        s.Children[3].BackgroundColor = Color.FromHex("#72DF40");
                    else
                        s.Children[3].BackgroundColor = Color.FromHex("#ECFF92");
                }

            }


            //s.Children[0].IsVisible = !string.IsNullOrEmpty(m.VideoUrl);
            s.Children[0].IsVisible = !m.IsSwapTarget;
            s.Children[1].IsVisible = m.IsSwapTarget;
            s.Children[2].IsVisible = m.IsNextExercise;
            s.Children[3].IsVisible = m.IsNextExercise;

            s.Children[4].IsVisible = true;
            s.Children[5].IsVisible = false;
            s.Children[6].IsVisible = false;
            if (btnMore != null && _tryChallenge && s.Children[3].IsVisible)
            {
                //btnDeload = (DrMuscleButton)s.Children[2];

                btnChallenge = (DrMuscleButton)s.Children[3];
                await Task.Delay(500);
                TooltipEffect.SetHasTooltip(btnMore, false);
                TooltipEffect.SetHasShowTooltip(btnMore, false);
                TooltipEffect.SetPosition(s.Children[3], TooltipPosition.Bottom);
                TooltipEffect.SetBackgroundColor(s.Children[3], AppThemeConstants.BlueColor);
                TooltipEffect.SetTextColor(s.Children[3], Color.White);
                TooltipEffect.SetText(s.Children[3], $"Tap Challenge");
                TooltipEffect.SetHasTooltip(s.Children[3], true);
                TooltipEffect.SetHasShowTooltip(s.Children[3], true);
                _tryChallenge = false;

            }
            if (btnMore != null && _tryDeload)
            {
                Config.ShowDeload = true;
                btnDeload = (DrMuscleButton)s.Children[2];
                await Task.Delay(500);
                TooltipEffect.SetHasTooltip(btnMore, false);
                TooltipEffect.SetHasShowTooltip(btnMore, false);
                TooltipEffect.SetPosition(s.Children[2], TooltipPosition.Bottom);
                TooltipEffect.SetBackgroundColor(s.Children[2], AppThemeConstants.BlueColor);
                TooltipEffect.SetTextColor(s.Children[2], Color.White);
                TooltipEffect.SetText(s.Children[2], $"Tap Deload");
                TooltipEffect.SetHasTooltip(s.Children[2], true);
                TooltipEffect.SetHasShowTooltip(s.Children[2], true);
                _tryDeload = false;
            }
            if (btnMore != null && _trySwapMenu)
            {

                btnSwap = (DrMuscleButton)s.Children[0];
                await Task.Delay(500);
                TooltipEffect.SetHasTooltip(btnMore, false);
                TooltipEffect.SetHasShowTooltip(btnMore, false);
                TooltipEffect.SetPosition(s.Children[0], TooltipPosition.Bottom);
                TooltipEffect.SetBackgroundColor(s.Children[0], AppThemeConstants.BlueColor);
                TooltipEffect.SetTextColor(s.Children[0], Color.White);
                TooltipEffect.SetText(s.Children[0], $"Tap Swap");
                TooltipEffect.SetHasTooltip(s.Children[0], true);
                TooltipEffect.SetHasShowTooltip(s.Children[0], true);
                _trySwapMenu = false;
            }
            contextMenuStack = s;
        }

        private ExerciceModel GetSwappedExercise(long id)
        {
            try
            {

                SwapExerciseContext context = ((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.TargetExerciseId == id);
                if (!string.IsNullOrEmpty(context.Label))
                {
                    ExerciceModel model = new ExerciceModel()
                    {
                        Id = (long)context.TargetExerciseId,
                        Label = context.Label,
                        IsBodyweight = context.IsBodyweight,
                        IsSwapTarget = true,
                        IsSystemExercise = context.IsSystemExercise,
                        VideoUrl = context.VideoUrl,
                        IsEasy = context.IsEasy,
                        BodyPartId = context.BodyPartId,
                        IsUnilateral = context.IsUnilateral,
                        IsTimeBased = context.IsTimeBased,
                        IsPlate = context.IsPlate,
                        IsFlexibility = context.IsFlexibility
                    };
                    model.IsSwapTarget = true;
                    //if ((Application.Current as App)?.FinishedExercices.FirstOrDefault(x => x.Id == model.Id) != null)
                    //{
                    //    model.IsFinished = true;
                    //    model.IsNextExercise = false;
                    //}

                    return model;
                }

            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private async Task UpdateExerciseList()
        {
            var exercises = new ObservableCollection<ExerciseWorkSetsModel>();
            exerciseItems = new ObservableCollection<ExerciseWorkSetsModel>();
            var exerList = new List<ExerciceModel>();
            try
            {

                string jsonFileName = "Exercises.json";
                ExerciceModel exerciseList = new ExerciceModel();
                var assembly = typeof(AllExercisePage).GetTypeInfo().Assembly;
                Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{jsonFileName}");
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var jsonString = reader.ReadToEnd();

                    //Converting JSON Array Objects into generic list    
                    

                }

            }
            catch (Exception ex)
            {

            }

            WorkoutTemplateModel workoutTemplate = GetWorkout();
            try
            {

                LblWorkoutName.Text = workoutTemplate.Label;

                var count = 1;
                foreach (ExerciceModel ee in workoutTemplate.Exercises)
                {
                    var localVideo = "";
                    try
                    {
                        var localex = exerList.FirstOrDefault(x => x.Id == ee.Id);
                        if (localex != null)
                            localVideo = localex.LocalVideo;
                    }
                    catch (Exception ex)
                    {

                    }
                    ExerciseWorkSetsModel e = new ExerciseWorkSetsModel()
                    {
                        Id = ee.Id,
                        IsBodyweight = ee.IsBodyweight,
                        IsEasy = ee.IsEasy,
                        IsNextExercise = OpenExercises.Contains(ee.Id) ? true : false,
                        IsSwapTarget = ee.IsSwapTarget,
                        IsFinished = ee.IsFinished,
                        IsSystemExercise = ee.IsSystemExercise,
                        IsNormalSets = ee.IsNormalSets,
                        IsUnilateral = ee.IsUnilateral,
                        IsTimeBased = ee.IsTimeBased,
                        IsMedium = ee.IsMedium,
                        BodyPartId = ee.BodyPartId,
                        Label = ee.Label,
                        VideoUrl = string.IsNullOrEmpty(localVideo) ? ee.VideoUrl : "",
                        WorkoutGroupId = ee.WorkoutGroupId,
                        RepsMaxValue = ee.RepsMaxValue,
                        RepsMinValue = ee.RepsMinValue,
                        IsPlate = ee.IsPlate,
                        Timer = ee.Timer,
                        IsSelected = false,
                        EquipmentId = ee.Id == 865 || ee.Id == 3211 || ee.Id == 17105 || ee.Id == 13454 || ee.Id == 15929 || ee.Id == 15930 || ee.Id == 15931 || ee.Id == 15947 || ee.Id == 15948 || ee.Id == 15949 || ee.Id == 18845 || ee.Id == 6998
                        ? null : ee.EquipmentId,
                        CountNo = $"{count} of {workoutTemplate.Exercises.Count}",
                        LocalVideo = localVideo,
                        IsFlexibility = ee.IsFlexibility

                    };
                    count++;
                    exercises.Add(e);
                }
                exerciseItems = exercises;
                ExerciseListView.ItemsSource = exerciseItems;
            }
            catch (Exception e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });

            }
        }

        private WorkoutTemplateModel GetWorkout()
        {
            WorkoutTemplateModel workoutTemplate = new WorkoutTemplateModel();
            List<ExerciceModel> exerList = new List<ExerciceModel>();
            if (LocalDBManager.Instance.GetDBSetting("CustomExperience")?.Value != "an active, experienced lifter")
            {
                exerList.Add(new ExerciceModel() { Label = "Push-up", Id = 7673, IsBodyweight = true, BodyPartId = 3});

            }
            else
            {
                exerList.Add(new ExerciceModel() { Label = "Bench Press", Id = 11, BodyPartId = 3});
            }

            exerList.Add(new ExerciceModel() { Label = "Squat", Id = 12, BodyPartId = 8});
            exerList.Add(new ExerciceModel() { Label = "Dumbbell Row", Id = 838,BodyPartId = 4 });
            exerList.Add(new ExerciceModel() { Label = "Deadlift", Id = 13, BodyPartId = 14 });
            exerList.Add(new ExerciceModel() { Label = "Dumbbell Shoulder Press", Id = 839, BodyPartId = 2});
            exerList.Add(new ExerciceModel() { Label = "Lat Pulldown", Id = 840, BodyPartId = 4});
            exerList.Add(new ExerciceModel() { Label = "Cable Crunch", Id = 841, BodyPartId = 7});
            exerList.Add(new ExerciceModel() { Label = "Triceps Pushdown", Id = 842, BodyPartId = 6 });
            exerList.Add(new ExerciceModel() { Label = "Biceps Curl", Id = 843, BodyPartId = 5});
            
            workoutTemplate.Exercises = exerList;
            workoutTemplate.Id = 104;
            workoutTemplate.Label = "[Gym] Full-body";
            workoutTemplate.IsSystemExercise = true;

            return workoutTemplate;

        }


        public ExerciseWorkSetsModel GetExerciseWorkSetModel(ExerciceModel ee)
        {
            return new ExerciseWorkSetsModel()
            {
                Id = ee.Id,
                IsBodyweight = ee.IsBodyweight,
                IsEasy = ee.IsEasy,
                IsNextExercise = ee.IsNextExercise,
                IsSwapTarget = ee.IsSwapTarget,
                IsFinished = ee.IsFinished,
                IsSystemExercise = ee.IsSystemExercise,
                IsNormalSets = ee.IsNormalSets,
                IsUnilateral = ee.IsUnilateral,
                IsTimeBased = ee.IsTimeBased,
                IsMedium = ee.IsMedium,
                BodyPartId = ee.BodyPartId,
                Label = ee.Label,
                VideoUrl = ee.VideoUrl,
                WorkoutGroupId = ee.WorkoutGroupId,
                RepsMaxValue = ee.RepsMaxValue,
                RepsMinValue = ee.RepsMinValue,
                Timer = ee.Timer,
                IsPlate = ee.IsPlate,
                IsSelected = false,
                EquipmentId = ee.EquipmentId,
                LocalVideo = ee.LocalVideo,
                IsFlexibility = ee.IsFlexibility
            };
        }

        public ExerciceModel GetExerciseModel(ExerciseWorkSetsModel ee)
        {
            return new ExerciceModel()
            {
                Id = ee.Id,
                IsBodyweight = ee.IsBodyweight,
                IsEasy = ee.IsEasy,
                IsNextExercise = ee.IsNextExercise,
                IsSwapTarget = ee.IsSwapTarget,
                IsFinished = ee.IsFinished,
                IsSystemExercise = ee.IsSystemExercise,
                IsNormalSets = ee.IsNormalSets,
                IsUnilateral = ee.IsUnilateral,
                IsTimeBased = ee.IsTimeBased,
                IsMedium = ee.IsMedium,
                BodyPartId = ee.BodyPartId,
                Label = ee.Label,
                VideoUrl = ee.VideoUrl,
                WorkoutGroupId = ee.WorkoutGroupId,
                RepsMaxValue = ee.RepsMaxValue,
                RepsMinValue = ee.RepsMinValue,
                Timer = ee.Timer,
                IsPlate = ee.IsPlate,
                EquipmentId = ee.EquipmentId,
                LocalVideo = ee.LocalVideo,
                IsFlexibility = ee.IsFlexibility
            };
        }


        private async void ListTapped(object sender, EventArgs args)
        {
            if (contextMenuStack != null)
                HideContextButton();
        }

        private async void NewTapped(object sender, EventArgs args)
        {
            try
            {
                if (CurrentLog.Instance.ShowEditWorkouts)
                {
                    BtnEditWorkout.Effects.Clear();
                    CurrentLog.Instance.ShowEditWorkouts = true;
                    Config.ShowEditWorkout = true;
                }
                if (Config.AddExercisesPopUp == false)
                {
                    if (App.IsAddExercisesPopUp)
                    {
                        AddExercises();
                        return;
                    }
                    App.IsAddExercisesPopUp = true;
                    ConfirmConfig ShowAddExePopUp = new ConfirmConfig()
                    {
                        Message = "Add or remove exercises and reorder on the fly.",
                        Title = "Edit workout",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = AppResources.GotIt,
                        CancelText = AppResources.RemindMe,
                        OnAction = async (bool ok) =>
                        {
                            if (ok)
                            {
                                Config.AddExercisesPopUp = true;
                                AddExercises();
                            }
                            else
                            {
                                Config.AddExercisesPopUp = false;
                                AddExercises();
                            }
                        }
                    };
                    await Task.Delay(100);
                    UserDialogs.Instance.Confirm(ShowAddExePopUp);
                }
                else
                {
                    CurrentLog.Instance.IsAddingExerciseLocally = true;
                    await PagesFactory.PushAsync<AddExercisesToWorkoutPage>();
                }

            }
            catch (Exception ex)
            {

            }
        }

        private async void AddExercises()
        {
            CurrentLog.Instance.IsAddingExerciseLocally = true;
            await PagesFactory.PushAsync<AddExercisesToWorkoutPage>();
        }

        private void ResetButtons()
        {
            //SaveWorkoutButton.TextColor = Color.White;
            //SaveWorkoutButton.BackgroundColor = Color.Transparent;
            //SaveWorkoutButton.FontAttributes = FontAttributes.None;
        }

        private void ChangeButtonsEmphasis()
        {
            //SaveWorkoutButton.TextColor = Color.Black;
            //SaveWorkoutButton.BackgroundColor = Color.White;
            //SaveWorkoutButton.FontAttributes = FontAttributes.Bold;
        }

        //New Exercise Setup
        protected async Task RunExercise(ExerciseWorkSetsModel m)
        {
            if (m.Id == 0)
                return;
            CurrentLog.Instance.EndExerciseActivityPage = this.GetType();
            CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
            CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(m);

            try
            {
                //if (!string.IsNullOrEmpty(CurrentLog.Instance.ExerciseLog.Exercice.LocalVideo))
                //{
                //    m.Add(new WorkoutLogSerieModelRef()
                //    {
                //        IsHeaderCell = true,
                //        Weight = new MultiUnityWeight(0, "kg"),
                //        Reps = 0,
                //        VideoUrl = CurrentLog.Instance.ExerciseLog.Exercice.LocalVideo,
                //        IsSetupNotCompleted = true
                //    });
                //    await Task.Delay(1500);
                //}

                if (m.IsBodyweight)
                {
                    decimal weight1 = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture);
                    if (m.Id == 16508)
                    {
                        m.Clear();
                        SetUpCompletePopup(weight1, m.Label, m, 29, true);

                        return;
                    }
                    KenkoAskForReps(weight1, m.Label, m);
                    return;
                }
                NormalExercisePopup exPopup = new NormalExercisePopup(CurrentLog.Instance.ExerciseLog.Exercice.LocalVideo, string.Format("{0}", CurrentLog.Instance.ExerciseLog.Exercice.Label), m.IsTimeBased ? m.EquipmentId == 4 ? "How many seconds can you lift easily? Enter the weight for 1 hand." : "How many seconds can you lift easily?" : m.EquipmentId == 4 ? "How much can you lift easily? Enter the weight for 1 hand." : "How much can you lift easily?.", "Enter starting weight here", m, false);
                exPopup._kenkoPage = this;
                await PopupNavigation.Instance.PushAsync(exPopup);
                return;

                PromptConfig firsttimeExercisePopup = new PromptConfig()
                {
                    InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                    IsCancellable = true,
                    Title = string.Format("{0} {1}", CurrentLog.Instance.ExerciseLog.Exercice.Label, AppResources.Setup),
                    
                    Message = m.IsTimeBased ? m.EquipmentId == 4 ? "How many seconds can you lift easily? Enter the weight for 1 hand. I'll improve on your guess as you train." : "How many seconds can you lift easily? I'll improve on your guess as you train." : m.EquipmentId == 4 ? "How much can you lift easily? Enter the weight for 1 hand. I'll improve on your guess as you train." : "How much can you lift easily? I'll improve on your guess as you train.",

                    Placeholder = "Enter weight here",
                    OkText = AppResources.Continue,
                    MaxLength = 4,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OnAction = async (weightResponse) =>
                    {
                        m.Clear();
                        if (weightResponse.Ok)
                        {
                            if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                            {
                                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                                {
                                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    Message = m.IsTimeBased ? "Please enter valid seconds." : "Please enter valid reps.",
                                    Title = AppResources.Error
                                });

                                return;
                            }
                            var weightText = weightResponse.Value.Replace(",", ".");
                            decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);
                            if (m.IsBodyweight)
                            {
                                LocalDBManager.Instance.SetDBSetting("BodyWeight", new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString());

                                await DrMuscleRestClient.Instance.SetUserBodyWeight(new UserInfosModel()
                                {
                                    BodyWeight = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value)
                                });
                                KenkoAskForReps(new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg, m.Label, m);
                                return;
                            }
                            SetUpCompletePopup(new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg, m.Label, m);
                        }
                        else
                            m.IsNextExercise = false;
                    }
                };

                firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
                UserDialogs.Instance.Prompt(firsttimeExercisePopup);

            }
            catch (Exception ex)
            {
                //try
                //{
                //    if (!CurrentLog.Instance.WorkoutLogSeriesByExercise.ContainsKey(CurrentLog.Instance.ExerciseLog.Exercice.Id))
                //    {
                //        await PagesFactory.PushAsync<ExerciseChartPage>();
                //    }
                //    else
                //    {
                //        if (LocalDBManager.Instance.GetDBSetting("timer_autoset").Value == "true")
                //            LocalDBManager.Instance.SetDBSetting("timer_remaining", CurrentLog.Instance.GetRecommendationRestTime(CurrentLog.Instance.ExerciseLog.Exercice.Id).ToString());
                //        await PagesFactory.PushAsync<SaveSetPage>();
                //    }
                //}
                //catch (Exception e)
                //{
                //    var properties = new Dictionary<string, string>
                //    {
                //        { "DrMusclePage_RunExercise", $"{e.StackTrace}" }
                //    };
                //    Crashes.TrackError(e, properties);
                //    await UserDialogs.Instance.AlertAsync(AppResources.PleaseCheckInternetConnection, AppResources.Error);
                //}

            }
        }

        protected async void KenkoAskForReps(decimal weight1, string exerciseName, ExerciseWorkSetsModel m)
        {
            try
            {
                if (m.Id == 16508)
                {
                    SetUpCompletePopup(weight1, exerciseName, m, 29, true);
                    return;
                }

                string desc = "";
                if (m.Label.ToLower().Contains("bands"))
                    desc = $"How many can you do easily? Aim for {(LocalDBManager.Instance.GetDBSetting("repsminimum").Value == "0" ? "5" : LocalDBManager.Instance.GetDBSetting("repsminimum").Value)}+ reps. Choose your band accordingly.";
                else
                    desc = "How many can you do easily?";
                var popup = new NormalExercisePopup(CurrentLog.Instance.ExerciseLog.Exercice.LocalVideo, $"{m.Label}", "How many can you do easily?", "Enter how many here", m, true);
                popup._kenkoPage = this;
                PopupNavigation.Instance.PushAsync(popup);

                return;
                PromptConfig firsttimeExercisePopup = new PromptConfig()
                {
                    InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                    IsCancellable = true,
                    Title = $"{m.Label}",
                    Message = "How many can you do easily?",
                    Placeholder = "Enter how many here",
                    OkText = AppResources.Continue,
                    MaxLength = 4,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OnAction = async (weightResponse) =>
                    {
                        m.Clear();
                        if (weightResponse.Ok)
                        {
                            if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                            {
                                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                                {
                                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    Message = m.IsTimeBased ? "Please enter valid seconds." : "Please enter valid reps.",
                                    Title = AppResources.Error
                                });

                                return;
                            }
                            int reps = Convert.ToInt32(weightResponse.Value, CultureInfo.InvariantCulture);
                            SetUpCompletePopup(weight1, exerciseName, m, reps, true);
                        }
                        else
                            m.IsNextExercise = false;
                    }
                };
                if (m.Label.ToLower().Contains("bands"))
                    firsttimeExercisePopup.Message = $"How many can you do easily? Aim for {(LocalDBManager.Instance.GetDBSetting("repsminimum").Value == "0" ? "5" : LocalDBManager.Instance.GetDBSetting("repsminimum").Value)}+ reps. Choose your band accordingly.";
                firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
                UserDialogs.Instance.Prompt(firsttimeExercisePopup);

            }
            catch (Exception ex)
            {

            }
        }

        async void ViewCell_PropertyChanged(System.Object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
            if (m == null)
                return;
            if (string.IsNullOrEmpty(m.LocalVideo))
                return;
            if (m != null && m.IsNextExercise && !string.IsNullOrEmpty(m.LocalVideo))
            {
                ((ViewCell)sender).Height = 315;
                ((ViewCell)sender).ForceUpdateSize();
            }
            else if (m != null && !m.IsNextExercise && !m.IsFinishWorkoutExe)
            {
                ((ViewCell)sender).Height = 115;
                ((ViewCell)sender).ForceUpdateSize();
            }

        }

        public async void FinishSetup(ExerciseWorkSetsModel m, string userWeight, bool isBodyweight)
        {
            if (isBodyweight)
            {

                decimal weight1 = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.ReplaceWithDot(), CultureInfo.InvariantCulture);

                int reps = Convert.ToInt32(userWeight, CultureInfo.InvariantCulture);
                SetUpCompletePopup(weight1, m.Label, m, reps, true);

            }
            else
            {
                var weightText = userWeight.ReplaceWithDot();
                decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);

                SetUpCompletePopup(new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg, m.Label, m);

            }
        }

        //HIIT Cardio Id: #16508
        protected async void SetUpCompletePopup(decimal weight1, string exerciseName, ExerciseWorkSetsModel exe, int reps = 6, bool IsBodyweight = false)
        {
            try
            {
                NewExerciceLogModel model = new NewExerciceLogModel();
                model.ExerciseId = (int)CurrentLog.Instance.ExerciseLog.Exercice.Id;
                model.Username = LocalDBManager.Instance.GetDBSetting("email").Value;
                model.RIR = 1;
                if (IsBodyweight)
                {
                    if (exe.IsEasy)
                        reps = reps + 2;
                    else if (exe.IsMedium)
                        reps = reps + 1;
                    else
                        reps = reps - 1;

                    model.Weight1 = new MultiUnityWeight(weight1, "kg");
                    model.Reps1 = reps.ToString();
                    model.Weight2 = new MultiUnityWeight(weight1, "kg");
                    model.Reps2 = (reps - 1).ToString();
                    model.Weight3 = new MultiUnityWeight(weight1, "kg");
                    model.Reps3 = (reps - 2).ToString();
                    model.Weight4 = new MultiUnityWeight(weight1, "kg");
                    model.Reps4 = (reps - 3).ToString();
                    LocalDBManager.Instance.SetDBSetting($"SetupWeight{model.ExerciseId}", Convert.ToString(weight1).ReplaceWithDot());
                }
                else
                {
                    //weight1 = weight1 + (weight1 / 100);
                    decimal weight2 = weight1 - (2 * weight1 / 100);
                    decimal weight3 = weight2 - (2 * weight2 / 100);
                    decimal weight4 = weight3 - (2 * weight3 / 100);
                    model.Weight1 = new MultiUnityWeight(weight1, "kg");
                    model.Reps1 = reps.ToString();
                    model.Weight2 = new MultiUnityWeight(weight2, "kg");
                    model.Reps2 = reps.ToString();
                    model.Weight3 = new MultiUnityWeight(weight3, "kg");
                    model.Reps3 = reps.ToString();
                    model.Weight4 = new MultiUnityWeight(weight4, "kg");
                    model.Reps4 = reps.ToString();
                    LocalDBManager.Instance.SetDBSetting($"SetupWeight{model.ExerciseId}", Convert.ToString(weight1).ReplaceWithDot());

                }

                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                LocalDBManager.Instance.SetDBReco("RReps" + model.ExerciseId + setStyle + "challenge", $"max");

                FetchReco(exe);
                //ConfirmConfig confirmExercise = new ConfirmConfig()
                //{
                //    Title = AppResources.SetupComplete,
                //    Message = string.Format("{0} {1}", exerciseName, AppResources.SetupCompleteExerciseNow),
                //    OkText = string.Format("{0}", exerciseName),
                //    CancelText = AppResources.Cancel,
                //    //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomRed),
                //    OnAction = async (bool obj) => {
                //        if (obj)
                //        {
                //            //await PagesFactory.PushAsync<ExerciseChartPage>();

                //        }
                //    }
                //};

                //UserDialogs.Instance.Confirm(confirmExercise);

            }
            catch (Exception ex)
            {

            }
        }

        void MoveToPage2_Tapped(System.Object sender, System.EventArgs e)
        {
            App.IsIntro = true;
            IntroPage2 page = new IntroPage2();
            page.OnBeforeShow();
            Navigation.PushAsync(page);
                //PagesFactory.PushAsync<IntroPage2>();
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