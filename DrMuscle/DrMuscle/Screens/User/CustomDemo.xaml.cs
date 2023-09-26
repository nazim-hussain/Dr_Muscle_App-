using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.Exercises;
using DrMuscle.Screens.Me;
using DrMuscle.Screens.Workouts;
using DrMuscle.Views;
using DrMuscleWebApiSharedModel;
using Microsoft.AppCenter.Crashes;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Screens.User
{
    public partial class CustomDemo : DrMusclePage, INotifyPropertyChanged
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
                StatusBarHeight.Height = 44;
            //double navigationBarHeight = Math.Abs(App.ScreenSize.Height - height - App.StatusBarHeight);
            // App.NavigationBarHeight = 146 + App.StatusBarHeight;// navigationBarHeight;

        }

        TimerPopup popup;
        bool isAppear = false;
        bool isAppear2 = false;
        private GetUserProgramInfoResponseModel upi = null;
        private bool IsSettingsChanged { get; set; }
        StackLayout contextMenuStack;
        private IFirebase _firebase;
        private bool ShouldAnimate = false;
        private bool ShouldEditAnimate = false;
        private bool IsRestarted = false;

        private bool ShouldChallengeAnimate = false;
        private bool IsAnimated = false;

        private bool ShouldDeloadAnimate = false;
        private bool IsDeloadAnimated = false;

        private bool ShouldSwapAnimate = false;
        private bool IsSwapAnimated = false;
        private bool IsWeightAsked = false, isDeloadAsk = false;

        public CustomDemo()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            exerciseItems = new ObservableCollection<ExerciseWorkSetsModel>();
            ExerciseListView.ItemsSource = GroupedData;
            NavigationPage.SetHasNavigationBar(this, false);
            ExerciseListView.ItemTapped += ExerciseListView_ItemTapped;
            _firebase = DependencyService.Get<IFirebase>();
            if (LocalDBManager.Instance.GetDBSetting("PlatesKg") == null || LocalDBManager.Instance.GetDBSetting("PlatesLb") == null)
            {
                var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);

                var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
            }

            //var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralBotAction, ToolbarItemOrder.Primary, 0);
            //this.ToolbarItems.Add(generalToolbarItem);
            this.ToolbarItems.Clear();
            //SaveWorkoutButton.Clicked += SaveWorkoutButton_Clicked;
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });

            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            Timer.Instance.OnTimerStop += OnTimerStop;



        }

        private void RefreshLocalized()
        {
            Title = AppResources.ChooseExercise;
            // LblTodaysExercises.Text = AppResources.TodaYExercises;
            //  SaveWorkoutButton.Text = "Finish workout"; // AppResources.FinishAndSaveWorkout;
        }

        public override void OnBeforeShow()
        {
            base.OnBeforeShow();
            CurrentLog.Instance.IsDemoRunningStep1 = false;
            CurrentLog.Instance.IsDemoRunningStep2 = false;
            IsWeightAsked = false;
            isDeloadAsk = false;
            CurrentLog.Instance.IsFromExercise = true;
            ShouldAnimate = true;
            IsAnimated = false;

            exerciseItems = new ObservableCollection<ExerciseWorkSetsModel>();
            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();

            App.IsDemo1Progress = true;
            if (GroupedData != null)
                GroupedData.Clear();
            ExerciseListView.ItemsSource = GroupedData;
            isAppear2 = false;

            try
            {
                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                string exId = $"{CurrentLog.Instance.CurrentExercise.Id}";

                LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
            }
            catch (Exception ex)
            {

            }
            UpdateExerciseList();
            if (LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}") == null || LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}").Value == null)
                LocalDBManager.Instance.SetDBSetting($"Time{DateTime.Now.Year}", $"{DateTime.Now.Ticks}");

        }
        bool TimerBased = false;
        string timeRemain = "0";
        async void OnTimerDone()
        {
            try
            {

                BtnTimer.Text = null;
                BtnTimer.Image = "stopwatch.png";

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

        void OnTimerStop()
        {
            try
            {
                //if (ToolbarItems.Count > 0)
                //{
                //    var index = 0;
                //    if (this.ToolbarItems.Count == 2)
                //    {
                //        index = 1;
                //    }
                //    this.ToolbarItems.RemoveAt(index);
                //    timerToolbarItem = new ToolbarItem("", "stopwatch.png", SlideTimerAction, ToolbarItemOrder.Primary, 0);
                //    this.ToolbarItems.Insert(index, timerToolbarItem);
                //}
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
                    BtnTimer.Image = "";
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
                    //Update rest of sets from this update model
                    var index = item.IndexOf(models);

                    if (models.IsFirstWorkSet && item.RecoModel != null && item.RecoModel.FirstWorkSetWeight != null)
                    {
                        var lastOneRM = ComputeOneRM(item.RecoModel.FirstWorkSetWeight.Kg, item.RecoModel.FirstWorkSetReps);
                        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                        var currentRM = ComputeOneRM(new MultiUnityWeight(weight, isKg ? "kg" : "lb").Kg, reps);
                        var worksets = string.Format("{0} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");

                        if (currentRM != 0)
                        {
                            var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                            models.SetTitle = string.Format("Last time: {0} x {1}\nFor {2}{3:0.00}% do:", item.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
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
            try
            {
                foreach (var item in exerciseItems)
                {
                    if (!item.Contains(models))
                        continue;
                    //Update rest of sets from this update model
                    var index = item.IndexOf(models);
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
                    if (models.IsFirstWorkSet && item.RecoModel != null && item.RecoModel.FirstWorkSetWeight != null)
                    {
                        var lastOneRM = ComputeOneRM(item.RecoModel.FirstWorkSetWeight.Kg, item.RecoModel.FirstWorkSetReps);
                        var currentRM = ComputeOneRM(models.Weight.Kg, models.Reps);
                        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                        var worksets = string.Format("{0} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");

                        if (currentRM != 0)
                        {
                            var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                            models.SetTitle = string.Format("Last time: {0} x {1}\nFor {2}{3:0.00}% do:", item.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
                        }
                    }
                    for (int i = index; i < item.Count; i++)
                    {
                        //if (item[i] == models)
                        //    continue;
                        WorkoutLogSerieModelRef updatingItem = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[i];
                        if (updatingItem.IsBackOffSet && !updatingItem.IsFinished && !updatingItem.IsWarmups && !updatingItem.IsFirstWorkSet)
                        {
                            updatingItem.Reps = (int)(reps + Math.Ceiling(reps * 0.75));
                            continue;
                        }
                        if (!updatingItem.IsFinished && !updatingItem.IsWarmups && !updatingItem.IsFirstWorkSet)
                        {
                            updatingItem.Weight = models.Weight;
                            if (reps != 0)
                                updatingItem.Reps = reps;
                        }
                    }

                    break;
                }
            }
            catch (Exception ex)
            {

            }
        }


        private void AddSetMessageTapped(WorkoutLogSerieModelRef models)
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
                    SetNo = $"SET {item.Count + 1}",
                    IsFirstSide = item.IsFirstSide,
                    ExerciseName = item.Label,
                    Increments = models.Increments,
                    SetTitle = "Last set—you can do this!",
                    IsTimeBased = models.IsTimeBased,
                    IsUnilateral = models.IsUnilateral,
                    IsBodyweight = models.IsBodyweight,
                    IsBackOffSet = models.IsBackOffSet
                };
                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(item.Id))
                {
                    var listOfSets = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[item.Id];
                    listOfSets.Add(newSet);
                }
                item.Add(newSet);
                for (var i = 0; i < item.Count; i++)
                    ((WorkoutLogSerieModelRef)item[i]).SetNo = $"SET {i + 1}/{item.Count}";
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                ExerciseListView.ScrollTo(newSet, ScrollToPosition.MakeVisible, true);

                break;
            }
        }
        private void DeleteSetMessageTapped(WorkoutLogSerieModelRef models)
        {
            if (contextMenuStack != null)
                HideContextButton();
            models.IsFinished = false;
            models.IsEditing = true;
            models.IsNext = true;
            //if (Timer.Instance.State != "RUNNING")
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
                    sets.IsNext = false;
                }
                models.IsEditing = false;
                models.IsNext = true;
                //if (item.Count == 1)
                //    break;
                //if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(item.Id))
                //{
                //    var listOfSets = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[item.Id];
                //    listOfSets.Remove(models);
                //}

                //item.Remove(models);
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                break;
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

        private void UpdateSetTitleMessageTapped(WorkoutLogSerieModelRef models)
        {
            foreach (var item in exerciseItems)
            {
                try
                {

                    if (!item.Contains(models))
                        continue;

                    var index = item.IndexOf(models);
                    if (item.Count > (index + 1))
                        ((WorkoutLogSerieModelRef)item[index + 1]).SetTitle = models.RIR == 0 ? "OK, that was hard. \nNow do:" : "Got it! Now do:";
                    if (models.RIR == 0 && item.IsBodyweight)
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
                                popup?.SetTimerRepsSets(string.Format("{1} x {0} ", "Body", rec.Reps).ReplaceWithDot());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }




        private async void ScrollToActiveSet(WorkoutLogSerieModelRef set, ExerciseWorkSetsModel m)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (set != null)
                {
                    //if (Device.RuntimePlatform.Equals(Device.iOS))
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                            ExerciseListView.SelectedItem = set;
                        ExerciseListView.ScrollTo(set, ScrollToPosition.End, true);
                        await Task.Delay(100);
                        ExerciseListView.ScrollTo(set, ScrollToPosition.MakeVisible, true);

                    });


                }
            });
        }
        private void SaveSetMessageTapped(WorkoutLogSerieModelRef models, bool IsFinished)
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
                    Finished_Clicked(item);
                    return;
                }

                //if (models.IsLastSet && models.IsUnilateral && models.IsFirstSide)
                //{
                //    WorkoutLogSerieModelRef first = (DrMuscle.Layout.WorkoutLogSerieModelRef)item.First();
                //    App.PCWeight = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? first.Weight.Kg : first.Weight.Lb;
                //    try
                //    {
                //        if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen") == null)
                //            LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");
                //        if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                //        {

                //            popup = new TimerPopup();
                //            popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                //            popup.popupTitle = "";
                //            if (models.IsBodyweight)
                //                popup?.SetTimerRepsSets(string.Format("Body x {0} ", first.Reps).ReplaceWithDot());
                //            else
                //                popup?.SetTimerRepsSets(string.Format("{0:0.00} {1} x {2} ", first.WeightDouble, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", first.Reps).ReplaceWithDot());
                //            popup.SetTimerText();
                //            //if (item.IsTimeBased)
                //            //{
                //            //    timeRemain = Convert.ToString(first.Reps);
                //            //    TimerBased = true;
                //            //}
                //            //else
                //            TimerBased = false;
                //            PopupNavigation.Instance.PushAsync(popup);

                //        }
                //        first.IsActive = true;
                //        ExerciseListView.ScrollTo(first, ScrollToPosition.MakeVisible, true);
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //    if (item.Count > 1)
                //    {
                //        if (Device.RuntimePlatform.Equals(Device.Android))
                //        {
                //            var index = item.IndexOf(models);
                //            WorkoutLogSerieModelRef before = (DrMuscle.Layout.WorkoutLogSerieModelRef)item[index > 1 ? index - 1 : index];
                //            ScrollToActiveSet(before, item);
                //        }
                //    }


                //    ScrollToActiveSet(models, item);

                //}

                foreach (WorkoutLogSerieModelRef logSerieModel in item)
                {
                    if (logSerieModel.IsFinished)
                        continue;
                    if (!logSerieModel.IsNext)
                    {
                        logSerieModel.IsNext = true;
                        App.PCWeight = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? logSerieModel.Weight.Kg : logSerieModel.Weight.Lb;
                        try
                        {
                            if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen") == null)
                                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");
                            if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                            {

                                popup = new TimerPopup(item.IsPlate);
                                popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                                popup.popupTitle = "";
                                if (logSerieModel.IsBodyweight)
                                    popup?.SetTimerRepsSets(string.Format("{0} x Body", logSerieModel.Reps).ReplaceWithDot());
                                else
                                    popup?.SetTimerRepsSets(string.Format("{2} x {0:0.00} {1}", logSerieModel.WeightDouble, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", logSerieModel.Reps).ReplaceWithDot());
                                popup.SetTimerText();
                                if (item.IsTimeBased)
                                {
                                    timeRemain = Convert.ToString(logSerieModel.Reps);
                                    TimerBased = true;
                                }
                                else
                                    TimerBased = false;
                                PopupNavigation.Instance.PushAsync(popup);
                                if (LocalDBManager.Instance.GetDBSetting("FirstSet") == null)
                                {
                                    LocalDBManager.Instance.SetDBSetting("FirstSet", "false");

                                    //DisplayAlert("Nice work!", "Tap \"Hide\" at the bottom to hide the timer and continue saving sets.", AppResources.GotIt);
                                }

                            }
                            logSerieModel.IsActive = true;
                            ExerciseListView.ScrollTo(logSerieModel, ScrollToPosition.MakeVisible, true);

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

            }
            ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;


        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            CurrentLog.Instance.IsDemoRunningStep2 = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            isAppear = false;
            ShouldAnimate = false;
            try
            {
                DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            }
            catch (Exception ex)
            {

            }
            MessagingCenter.Unsubscribe<Message.SaveSetMessage>(this, "SaveSetMessage");
            MessagingCenter.Unsubscribe<Message.UpdateSetTitleMessage>(this, "UpdateSetTitleMessage");
            MessagingCenter.Unsubscribe<Message.DeleteSetMessage>(this, "DeleteSetMessage");
            MessagingCenter.Unsubscribe<Message.CellUpdateMessage>(this, "CellUpdateMessage");
            MessagingCenter.Unsubscribe<Message.WeightRepsUpdatedMessage>(this, "WeightRepsUpdatedMessage");
            MessagingCenter.Unsubscribe<Message.AddSetMessage>(this, "AddSetMessage");

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!isAppear2 && !App.IsSleeping && !CurrentLog.Instance.IsMovingOnBording)
            {
                isAppear2 = true;
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = $"Tell the app how much you think you can Squat very easily for 6 reps.",
                        Title = $"Smart weight",
                        OkText = AppResources.Ok

                    });

                else
                {
                    await DisplayAlert($"Smart weight", $"Tell the app how much you think you can Squat very easily for 6 reps.", AppResources.Ok);
                }
                CellHeaderTapped(new Button() { BindingContext = exerciseItems.First() }, null);

            }
            _firebase.SetScreenName("demo_workout");

            MessagingCenter.Subscribe<Message.SaveSetMessage>(this, "SaveSetMessage", (obj) =>
            {
                SaveSetMessageTapped(obj.model, obj.IsFinishExercise);
            });

            MessagingCenter.Subscribe<Message.UpdateSetTitleMessage>(this, "UpdateSetTitleMessage", (obj) =>
            {
                UpdateSetTitleMessageTapped(obj.model);
            });

            MessagingCenter.Subscribe<Message.DeleteSetMessage>(this, "DeleteSetMessage", (obj) =>
            {
                DeleteSetMessageTapped(obj.model);
            });

            MessagingCenter.Subscribe<Message.OneRMChangedMessage>(this, "OneRMChangedMessage", (obj) =>
            {
                UpdateOneRM(obj.model, obj.Weight, obj.Reps);
            });

            MessagingCenter.Subscribe<Message.CellUpdateMessage>(this, "CellUpdateMessage", (obj) =>
            {
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
            });

            //
            MessagingCenter.Subscribe<Message.WeightRepsUpdatedMessage>(this, "WeightRepsUpdatedMessage", (obj) =>
            {
                UpdateWeoghtRepsMessageTapped(obj.model);
            });

            MessagingCenter.Subscribe<Message.AddSetMessage>(this, "AddSetMessage", (obj) =>
            {
                AddSetMessageTapped(obj.model);
            });
            ExerciseListView.ItemsSource = exerciseItems;
        }

        

        private async void Finished_Clicked(ExerciseWorkSetsModel model)
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
                    setList.Add(item);
                }
                WorkoutLogSerieModelRef workout = ((WorkoutLogSerieModelRef)model.First());
                workout.IsNext = true;
                workout.IsActive = true;
                App.PCWeight = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? workout.Weight.Kg : workout.Weight.Lb;
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                if (workout.IsWarmups)
                {
                    var time = workout.IsLastWarmupSet ? "55" : CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsBodyweight ? "30" : "40";
                    LocalDBManager.Instance.SetDBSetting("timer_remaining", time);

                }
                else
                {
                    if (LocalDBManager.Instance.GetDBSetting("timer_autoset").Value == "true")
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", CurrentLog.Instance.GetRecommendationRestTime(CurrentLog.Instance.ExerciseLog.Exercice.Id, false, CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsNormalSets).ToString());
                        //LocalDBManager.Instance.SetDBSetting("timer_remaining", "25");
                        //timeRemain = CurrentLog.Instance.GetRecommendationRestTime(CurrentLog.Instance.ExerciseLog.Exercice.Id, false, CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsNormalSets).ToString();
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", App.globalTime.ToString());
                        //timeRemain = TimerEntry;
                    }
                }
                //Timer.Instance.StopTimer();
                //Timer.Instance.stopRequest = false;
                //Timer.Instance.StartTimer();
                //Open fullscreen timer
                //if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                //{

                //    popup = new TimerPopup();
                //    popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                //    popup.popupTitle = "";
                //    popup?.SetTimerRepsSets(string.Format("{0:0.00} {1} x {2} ", workout.WeightDouble, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", workout.Reps).ReplaceWithDot());
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

                AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                {
                    Title = "Well done! Now do all sets for side 2",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.Ok,

                };

                ScrollToSnap(setList, model);
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    setList.First().IsSizeChanged = !setList.First().IsSizeChanged;
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ExerciseListView.ScrollTo(setList.First(), ScrollToPosition.Start, false);
                    });
                }
                UserDialogs.Instance.Alert(ShowExplainRIRPopUp);
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
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                return;
            }
            ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
            {
                Title = $"Finish & save {model.Label}?",
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = "Finish & save",
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

        private void ScrollToSnap(List<WorkoutLogSerieModelRef> setList, ExerciseWorkSetsModel m)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (setList.Count > 0)
                {
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        ExerciseListView.ScrollTo(setList.First(), ScrollToPosition.Start, true);
                    int position = 0;
                    foreach (var itemGood in exerciseItems)
                    {
                        if (itemGood == m)
                            break;
                        position += 1;
                        position += itemGood.Count;
                    }
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        ExerciseListView.ItemPosition = exerciseItems.IndexOf(m);
                    else
                        ExerciseListView.ItemPosition = position;
                    ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;
                }
            });
        }

        private async void PushToDataServer(ExerciseWorkSetsModel model)
        {
            CurrentLog.Instance.IsWalkthrough = true;
            PagesFactory.PopToRootAsync();

            
            CurrentLog.Instance.RecommendationsByExercise.Clear();
            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Clear();

            App.IsDemo1Progress = false;
            return;
            CurrentLog.Instance.IsDemoPopingOut = false;
            bool result = true;
            try
            {
                int? RIR = null;
                int max = 0;
                List<WorkoutLogSerieModel> serieModelList = new List<WorkoutLogSerieModel>();
                foreach (WorkoutLogSerieModelRef l in model)
                {
                    if (l.IsFinished)
                    {
                        if (l.IsFirstWorkSet)
                            RIR = l.RIR;
                        WorkoutLogSerieModelEx serieModel = new WorkoutLogSerieModelEx()
                        {
                            Exercice = new ExerciceModel() { Id = model.Id },
                            Reps = l.Reps,
                            UserId = CurrentLog.Instance.ExerciseLog.UserId,
                            Weight = l.Weight,
                            RIR = RIR,
                            IsWarmups = l.IsWarmups
                        };
                        CurrentLog.Instance.RIR = RIR == null ? 0 : (int)RIR;
                        if (l.Reps > max)
                        {
                            max = l.Reps;

                        }
                        serieModelList.Add(serieModel);
                        //BooleanModel b = await DrMuscleRestClient.Instance.AddWorkoutLogSerie(serieModel);
                        //result = result && b.Result;

                    }
                }
                if (serieModelList.Count == 0)
                {
                    await UserDialogs.Instance.AlertAsync("Save atleast one set", AppResources.Error);
                    return;
                }

                CurrentLog.Instance.LastSerieModelList = serieModelList;
                CurrentLog.Instance.BestReps = max;
                //CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].Increments

            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    { "FinishExercise", $"{ex.StackTrace}" }
                };
                Crashes.TrackError(ex, properties);
            }
            try
            {

                if (Timer.Instance.State == "RUNNING")
                {
                    await Timer.Instance.StopTimer();
                }

                if ((Application.Current as App)?.FinishedExercices.FirstOrDefault(x => x.Id == model.Id) == null)
                    (Application.Current as App)?.FinishedExercices.Add(GetExerciseModel(model));

                LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + model.Id + "Normal", DateTime.Now.AddDays(-1).ToString());
                LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + model.Id + "RestPause", DateTime.Now.AddDays(-1).ToString());
                DependencyService.Get<IFirebase>().LogEvent("finished_exercise", "");
                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(model.Id))
                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(model.Id);
                //foreach (var item in items)
                //{
                //    item.IsExerciseFinished = true;
                //}
                //items.First().IsNext = true;

                CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(model);
                CurrentLog.Instance.IsFromExercise = true;

                CurrentLog.Instance.EndExerciseActivityPage = GetType();
                _firebase.LogEvent("demo_workout_1_done", "Done");
                //await PagesFactory.PushAsync<EndPage>();


            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                    {
                        { "FinishExerciseResultHandle", $"{ex.StackTrace}" }
                    };
                Crashes.TrackError(ex, properties);
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                ConfirmConfig exitPopUp = new ConfirmConfig()
                {

                    Title = AppResources.Exit,
                    Message = AppResources.AreYouSureYouWantToExit,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.Yes,
                    CancelText = AppResources.No,
                };

                var result = await UserDialogs.Instance.ConfirmAsync(exitPopUp);
                if (result)
                {
                    var kill = DependencyService.Get<IKillAppService>();
                    kill.ExitApp();
                }
            });
            return true;
        }

        private async void SaveWorkoutButton_Clicked(object sender, EventArgs e)
        {
            ConfirmConfig ShowConfirmPopUp = new ConfirmConfig()
            {
                Message = AppResources.AreYouSureYouAreFinishedAndWantToSaveTodaysWorkout,
                Title = AppResources.FinishAndSaveWorkoutQuestion,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = AppResources.FinishAndSave,
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

        private async void SavingExcercise()
        {
            //if (!await CanGoFurther())
            //{
            //    return;
            //}
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("OlderQuickMode") != null)
                {
                    if (LocalDBManager.Instance.GetDBSetting("OlderQuickMode").Value != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("QuickMode", LocalDBManager.Instance.GetDBSetting("OlderQuickMode").Value);
                        LocalDBManager.Instance.SetDBSetting("OlderQuickMode", null);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            try
            {
                if (CurrentLog.Instance.CurrentWorkoutTemplateGroup.RequiredWorkoutToLevelUp == 999)
                {
                    foreach (ExerciceModel exerciceModel in CurrentLog.Instance.CurrentWorkoutTemplate.Exercises)
                    {
                        LocalDBManager.Instance.SetDBSetting($"workout{exerciceModel.Id}", "false");
                    }
                    await PagesFactory.PopToRootAsync();
                    return;
                }
            }
            catch (Exception ex)
            {

            }
            var responseLog = await DrMuscleRestClient.Instance.SaveGetWorkoutInfo(new SaveWorkoutModel() { WorkoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id });

            LocalDBManager.Instance.SetDBSetting("last_workout_label", CurrentLog.Instance.CurrentWorkoutTemplate.Label);
            LocalDBManager.Instance.SetDBSetting("lastDoneProgram", CurrentLog.Instance.CurrentWorkoutTemplate.Id.ToString());
            bool isSystem = CurrentLog.Instance.CurrentWorkoutTemplate.IsSystemExercise;
            foreach (ExerciceModel exerciceModel in CurrentLog.Instance.CurrentWorkoutTemplate.Exercises)
            {
                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(exerciceModel.Id))
                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(exerciceModel.Id);

                LocalDBManager.Instance.SetDBSetting($"workout{exerciceModel.Id}", "false");
                try
                {
                    bool isSwapped = ((App)Application.Current).SwapExericesContexts.Swaps.Any(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == exerciceModel.Id);
                    if (isSwapped)
                    {
                        long targetExerciseId = (long)((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == exerciceModel.Id).TargetExerciseId;

                        var obj = (Application.Current as App)?.FinishedExercices.FirstOrDefault(x => x.Id == targetExerciseId);
                        if (obj != null)
                        {
                            LocalDBManager.Instance.SetDBSetting($"workout{targetExerciseId}", "false");
                            (Application.Current as App)?.FinishedExercices.Remove(obj);
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
            CurrentLog.Instance.CurrentWorkoutTemplate = null;
            CurrentLog.Instance.WorkoutTemplateCurrentExercise = null;
            CurrentLog.Instance.WorkoutStarted = false;
            string fname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            string Msg = $"{AppResources.Congratulations} {fname}!";
            try
            {
                if (responseLog != null && responseLog.RecommendedProgram != null)
                {

                    if (responseLog.RecommendedProgram.RemainingToLevelUp > 0)
                        Msg += $" {AppResources.YouAre1WorkoutCloserToNewExercisesYourProgramWillLevelUpIn} {responseLog.RecommendedProgram.RemainingToLevelUp} {AppResources.WorkoutsFullStop}";

                }

                if (responseLog != null)
                {
                    Xamarin.Forms.MessagingCenter.Send<FinishWorkoutMessage>(new FinishWorkoutMessage() { PopupMessage = Msg }, "FinishWorkoutMessage");
                }
                await PagesFactory.PopToRootAsync();
            }
            catch (Exception ex)
            {
                await PagesFactory.PopToRootAsync();
            }

        }


        private async void PlateTapped(object sender, EventArgs e)
        {
            try
            {
                DependencyService.Get<IKeyboardHelper>().HideKeyboard();
            }
            catch (Exception ex)
            {

            }
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //{

            //    var items = exerciseItems.Where(x => x.IsNextExercise).ToList();
            //    var isOpen = false;
            //        if (items != null && items.Count>0)
            //    {
            //        foreach (ExerciseWorkSetsModel item in items)
            //        {
            //            if (item.Count > 0)
            //            {
            //                foreach (WorkoutLogSerieModelRef model in item)
            //                {
            //                    if(model.IsNext && !model.IsFinished)
            //                    {
            //                        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

            //                        CurrentLog.Instance.CurrentWeight = isKg ? model.Weight.Kg : model.Weight.Lb;

            //                        var page = new PlateCalculatorPopup();
            //                        await PopupNavigation.Instance.PushAsync(page);
            //                    }
            //                    isOpen = true;
            //                    break;
            //                } 
            //            }
            //            if (isOpen)
            //                break;
            //        }


            //    }

            //}
            //else

            CurrentLog.Instance.CurrentWeight = App.PCWeight;

            var page = new PlateCalculatorPopup();
            await PopupNavigation.Instance.PushAsync(page);
            //Xamarin.Forms.MessagingCenter.Send<PlateCalulatorMessage>(new PlateCalulatorMessage(), "PlateCalulatorMessage");

        }
        //private async void TimerTapped(object sender, EventArgs e)
        //{
        //    SlideTimerAction();
        //}

        private async void ExerciseListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {


                if (contextMenuStack != null)
                    HideContextButton();
                if (ExerciseListView.SelectedItem == null)
                    return;
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
                    workout.IsEditing = true;
                    workout.IsNext = true;
                    //if (Timer.Instance.State != "RUNNING")
                    //    Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = false }, "SaveSetMessage");
                    ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        workout.IsSizeChanged = !workout.IsSizeChanged;

                }
                if (ExerciseListView.SelectedItem != null)
                    ExerciseListView.SelectedItem = null;
            }
            catch (Exception ex)
            {
                ExerciseListView.BeginRefresh();
                ExerciseListView.EndRefresh();
            }

        }
        private async void PicktorialTapped(object sender, EventArgs e)
        {
            if (contextMenuStack != null)
                HideContextButton();
            ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
            if (!string.IsNullOrEmpty(m.VideoUrl))
            {
                CurrentLog.Instance.VideoExercise = GetExerciseModel(m);
                await PagesFactory.PushAsync<ExerciseVideoPage>();
            }

            // OnCancelClicked(sender, e);
        }
        DrMuscleButton btnEdit, btnChallenge, btnDeload, btnSwap;
        bool IsOpen = false;
        private async void CellHeaderTapped(object sender, EventArgs e)
        {

            try
            {

                if (CurrentLog.Instance.IsDemoRunningStep2)
                {
                    
                    if (!IsOpen)
                    {
                        IsOpen = true;
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                            await DisplayAlert($"{CurrentLog.Instance.BestReps} reps last time", $"Let's aim for {CurrentLog.Instance.BestReps + CurrentLog.Instance.RIR } reps today.", AppResources.GotIt);
                        else
                        {
                            AlertConfig alertConfig = new AlertConfig()
                            {
                                Message = $"Let's aim for {CurrentLog.Instance.BestReps + CurrentLog.Instance.RIR } reps today.",
                                Title = $"{CurrentLog.Instance.BestReps} reps last time",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = AppResources.GotIt,

                            };
                            await UserDialogs.Instance.AlertAsync(alertConfig);
                        }
                    }
                    
                }


            }
            catch (Exception ex)
            {

            }
            ShouldAnimate = false;
            if (contextMenuStack != null)
                HideContextButton();
            var currentOpenExer = exerciseItems.Where(x => x.IsNextExercise).FirstOrDefault();

            ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;

            if (m.IsNextExercise && m.Count > 0)
            {
                m.Clear();
                m.IsNextExercise = false;
                return;
            }



            if (m.RecoModel != null)
            {
                m.IsNextExercise = true;
                FetchReco(m, null);
                return;
            }
            if (!CurrentLog.Instance.IsDemoRunningStep2)
            {
                RunExercise(m);

                return;
            }
            m.IsNextExercise = true;
            await FetchReco(m);

        }

        private async Task FetchReco(ExerciseWorkSetsModel m, int? sessionDays = null)
        {
            if (m.IsNextExercise)
            {
                long? workoutId = null;


                if (m.Count > 0)
                {
                    m.Clear();
                    return;
                }

                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                {
                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                }
                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                {
                    var sets = CurrentLog.Instance.WorkoutLogSeriesByExerciseRef[m.Id];
                    foreach (var cacheSet in sets)
                    {
                        m.Add(cacheSet);
                    }
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        ExerciseListView.ScrollTo(sets.First(), ScrollToPosition.Start, true);
                    await Task.Delay(300);

                    ExerciseListView.ItemPosition = exerciseItems.IndexOf(m);
                    ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;
                    return;
                }

                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value; ;

                string exId = $"{m.Id}";
                var lastTime = LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle);



                if (m.RecoModel == null)
                {
                    if (LocalDBManager.Instance.GetDBSetting("SetStyle").Value == "Normal")
                    {

                        m.RecoModel = await DrMuscleRestClient.Instance.GetRecommendationNormalRIRForExercise(new GetRecommendationForExerciseModel()
                        {
                            Username = LocalDBManager.Instance.GetDBSetting("email").Value,
                            ExerciseId = m.Id,
                            IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false,
                            LightSessionDays = sessionDays != null ? sessionDays > 9 ? sessionDays : null : null,
                            WorkoutId = workoutId
                        });

                    }
                    else
                    {
                        var bdyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "lb").Kg;
                        m.RecoModel = GetRecommendationRestPauseRIRWithoutDeload(5, bdyWeight,
                bdyWeight,
                bdyWeight,
                CurrentLog.Instance.BestReps + CurrentLog.Instance.RIR,
             CurrentLog.Instance.BestReps,
            CurrentLog.Instance.BestReps - 1, 12, 16, 0, false, false, true,
            bdyWeight,
            null, null, null,
            false, 0, null, false, false, null);

                    }
                }
                if (m.RecoModel != null)
                {
                    bool IsMaxChallenge = false;
                    if (m.RecoModel.IsDeload)
                        m.RecoModel.IsMaxChallenge = false;
                    m.RecoModel.IsLightSession = sessionDays == null ? false : sessionDays > 9 ? true : false;
                    if (m.RecoModel.Reps <= 0)
                        m.RecoModel.Reps = 1;
                    if (m.RecoModel.NbRepsPauses <= 0)
                        m.RecoModel.NbRepsPauses = 1;
                    RecoContext.SaveContexts("Reco" + exId + setStyle, m.RecoModel);
                    LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + exId + setStyle, DateTime.Now.AddDays(1).ToString());

                    m.IsNormalSets = m.RecoModel.IsNormalSets;

                    string lbl3text = "";
                    string iconOrange = "";
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") != null)
                    {
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "deload")
                        {
                            m.RecoModel.IsDeload = true;
                            m.RecoModel.RecommendationInKg = m.RecoModel.Weight.Kg - (m.RecoModel.Weight.Kg * (decimal)0.1);
                            if (m.RecoModel.IsNormalSets)
                            {
                                m.RecoModel = RecoComputation.GetNormalDeload(m.RecoModel);
                            }
                            else
                            {
                                m.RecoModel = RecoComputation.GetRestPauseDeload(m.RecoModel);
                            }
                            m.RecoModel.NbPauses = 0;
                            m.RecoModel.IsLightSession = true;
                            m.RecoModel.IsMaxChallenge = false;
                        }
                    }
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

                    CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
                    CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(m);
                    if (CurrentLog.Instance.RecommendationsByExercise.ContainsKey(CurrentLog.Instance.ExerciseLog.Exercice.Id))
                        CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id] = m.RecoModel;
                    else
                        CurrentLog.Instance.RecommendationsByExercise.Add(CurrentLog.Instance.ExerciseLog.Exercice.Id, m.RecoModel);




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
                                Weight = m.RecoModel.WarmUpsList[i].WarmUpWeightSet,
                                IsWarmups = true,
                                Reps = m.RecoModel.WarmUpsList[i].WarmUpReps,

                                SetNo = $"SET {setList.Count + 1}",
                                IsLastWarmupSet = i == m.RecoModel.WarmUpsList.Count - 1 ? true : false,
                                IsHeaderCell = i == 0 ? true : false,
                                HeaderImage = iconOrange,
                                HeaderTitle = lbl3text,
                                ExerciseName = m.Label,
                                Increments = m.RecoModel.Increments,
                                SetTitle = i == 0 ? "Let's warm up:" : "",
                                IsTimeBased = m.IsTimeBased,
                                IsUnilateral = m.IsUnilateral,
                                IsBodyweight = m.IsBodyweight
                            });

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
                            Weight = m.RecoModel.Weight,
                            IsWarmups = false,
                            Reps = m.RecoModel.Reps,
                            SetNo = $"SET {setList.Count + 1}",
                            ExerciseName = m.Label,
                            IsFirstWorkSet = j == 0 ? true : false,
                            Increments = m.RecoModel.Increments,
                            SetTitle = j == 0 ? IsMaxChallenge ? "Try max reps today:" : "1st work set—You got this:" : "All right! Now let's try:",
                            IsTimeBased = m.IsTimeBased,
                            IsUnilateral = m.IsUnilateral,
                            IsBodyweight = m.IsBodyweight,
                            IsMaxChallenge = j == 0 ? IsMaxChallenge : false
                        };
                        if (j == 0 && m.RecoModel.FirstWorkSetWeight != null)
                        {
                            var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                            var worksets = string.Format("{0} {1}", Math.Round(isKg ? m.RecoModel.FirstWorkSetWeight.Kg : m.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");
                            var days = 0;
                            if (m.RecoModel.LastLogDate != null)
                                days = (DateTime.Now - ((DateTime)m.RecoModel.LastLogDate).ToLocalTime()).Days;
                            var dayString = days == 0 ? "Today" : days == 1 ? "1 day ago" : $"{days} days ago";
                            if (m.RecoModel.IsBodyweight)
                                worksets = "body";
                            
                            var lastOneRM = ComputeOneRM(m.RecoModel.FirstWorkSetWeight.Kg, m.RecoModel.FirstWorkSetReps);
                            var currentRM = ComputeOneRM(m.RecoModel.Weight.Kg, rec.Reps);
                            if (currentRM != 0)
                            {
                                var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                                rec.SetTitle = string.Format("Last time: {0} x {1}\nFor {2}{3:0.00}% do:", m.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
                            }
                        }
                        if (setList.Count == 0)
                        {
                            rec.IsHeaderCell = true;
                            rec.HeaderImage = iconOrange;
                            rec.HeaderTitle = lbl3text;
                        }
                        setList.Add(rec);
                    }

                    for (int j = 0; j < m.RecoModel.NbPauses; j++)
                    {

                        var rec = new WorkoutLogSerieModelRef()
                        {
                            Id = m.Id,
                            Weight = m.RecoModel.Weight,
                            IsWarmups = false,
                            Reps = m.RecoModel.NbRepsPauses,
                            SetNo = $"SET {setList.Count + 1}",
                            ExerciseName = m.Label,
                            Increments = m.RecoModel.Increments,
                            SetTitle = "All right! Now let's try:",
                            IsTimeBased = m.IsTimeBased,
                            IsUnilateral = m.IsUnilateral,
                            IsBodyweight = m.IsBodyweight

                        };
                        if (!isMarkFirstSet && j == 0)
                        {
                            rec.IsFirstWorkSet = true;
                            rec.SetTitle = IsMaxChallenge ? "Try max reps today:" : "1st work set—you got this:";
                            rec.IsMaxChallenge = IsMaxChallenge;
                        }
                        if (!isMarkFirstSet && j == 0 && m.RecoModel.FirstWorkSetWeight != null)
                        {
                            var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                            var worksets = string.Format("{0} {1}", Math.Round(isKg ? m.RecoModel.FirstWorkSetWeight.Kg : m.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");
                            var days = 0;
                            if (m.RecoModel.LastLogDate != null)
                                days = (DateTime.Now - ((DateTime)m.RecoModel.LastLogDate).ToLocalTime()).Days;
                            var dayString = days == 0 ? "Today" : days == 1 ? "1 day ago" : $"{days} days ago";
                            if (m.RecoModel.IsBodyweight)
                                worksets = "body";
                            //rec.SetTitle = $"{dayString}: {m.RecoModel.FirstWorkSetReps} x {worksets}—let's try:";
                            var lastOneRM = ComputeOneRM(m.RecoModel.FirstWorkSetWeight.Kg, m.RecoModel.FirstWorkSetReps);
                            var currentRM = ComputeOneRM(m.RecoModel.Weight.Kg, rec.Reps);
                            if (currentRM != 0)
                            {
                                var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                                rec.SetTitle = string.Format("Last time: {0} x {1}\nFor {2}{3:0.00}% do:", m.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
                            }
                        }
                        if (setList.Count == 0)
                        {
                            rec.IsHeaderCell = true;
                            rec.HeaderImage = iconOrange;
                            rec.HeaderTitle = lbl3text;
                        }
                        setList.Add(rec);
                    }
                    var worksetcount = (setList.Count - m.RecoModel.WarmUpsList.Count);
                    if (worksetcount > 2 && !m.IsBodyweight)
                    {
                        if (m.RecoModel.BackOffSetWeight != null)
                        {
                            setList.Last().Reps = (int)(setList.Last().Reps + Math.Ceiling(setList.Last().Reps * 0.75));
                            setList.Last().IsBackOffSet = true;
                            setList.Last().Weight = m.RecoModel.BackOffSetWeight;
                            setList[setList.Count - 2].IsNextBackOffSet = true;
                        }
                    }
                    if (worksetcount > 3)
                    {
                        setList[setList.Count - 2].SetTitle = "Almost done—Keep it up!";
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
                        setList.First().IsNext = true;
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
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Add(CurrentLog.Instance.ExerciseLog.Exercice.Id, new ObservableCollection<WorkoutLogSerieModelRef>(setList));
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
                            if (Device.RuntimePlatform.Equals(Device.iOS))
                                ExerciseListView.ScrollTo(setList.First(), ScrollToPosition.Start, true);
                            await Task.Delay(300);
                            int position = 0;
                            foreach (var itemGood in exerciseItems)
                            {
                                if (itemGood == m)
                                    break;
                                position += 1;
                                position += itemGood.Count;
                            }
                            ExerciseListView.ItemPosition = position;
                            ExerciseListView.IsScrolled = !ExerciseListView.IsScrolled;
                        }
                    });
                    if (m.IsFirstSide)
                    {
                        AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                        {
                            Title = "Do all sets for side 1 now",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = AppResources.Ok,

                        };
                        UserDialogs.Instance.Alert(ShowExplainRIRPopUp);
                    }


                }
            }
        }

        public static decimal ComputeOneRM(decimal weight, int reps)
        {
            return (decimal)(AppThemeConstants.Coeficent * reps) * weight + weight;
        }

        public static decimal ComputeWeightforOneRM(decimal oneRM, int reps)
        {
            return  (decimal) oneRM / (decimal)((AppThemeConstants.Coeficent * reps) + 1);
        }

        async void animate(View grid)
        {
            try
            {
                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                Device.BeginInvokeOnMainThread(() =>
                {
                    var a = new Animation();
                    a.Add(0, 0.5, new Animation((v) =>
                    {
                        grid.Scale = v;
                    }, 1.0, 0.8, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                    a.Add(0.5, 1, new Animation((v) =>
                    {
                        grid.Scale = v;
                    }, 0.8, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
                    a.Commit(grid, "animation", 16, 2000, null, (d, f) =>
                    {
                        grid.Scale = 1.0;
                        if (ShouldAnimate)
                            animate(grid);

                    });
                });
            }
            catch (Exception ex)
            {

            }
        }

        async void animateEdit(View grid)
        {
            try
            {


                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                Device.BeginInvokeOnMainThread(() =>
                {
                    var a = new Animation();
                    a.Add(0, 0.5, new Animation((v) =>
                    {
                        grid.Scale = v;
                    }, 1.0, 0.8, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                    a.Add(0.5, 1, new Animation((v) =>
                    {
                        grid.Scale = v;
                    }, 0.8, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
                    a.Commit(grid, "animation", 16, 2000, null, (d, f) =>
                    {
                        grid.Scale = 1.0;
                        System.Diagnostics.Debug.WriteLine("ANIMATION Edit");
                        if (ShouldEditAnimate)
                            animateEdit(grid);
                    });

                });
            }
            catch (Exception ex)
            {

            }
        }

        async void animateChallenge(View grid)
        {
            try
            {
                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                var a = new Animation();
                a.Add(0, 0.5, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 1.0, 0.8, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                a.Add(0.5, 1, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 0.8, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
                a.Commit(grid, "animation", 16, 2000, null, (d, f) =>
                {
                    grid.Scale = 1.0;
                    if (ShouldChallengeAnimate)
                        animateChallenge(grid);

                });

            }
            catch (Exception ex)
            {

            }
        }

        async void animateDeload(View grid)
        {
            try
            {
                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                var a = new Animation();
                a.Add(0, 0.5, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 1.0, 0.8, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                a.Add(0.5, 1, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 0.8, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
                a.Commit(grid, "animation", 16, 2000, null, (d, f) =>
                {
                    grid.Scale = 1.0;
                    if (ShouldDeloadAnimate)
                        animateDeload(grid);

                });
            }
            catch (Exception ex)
            {

            }
        }

        async void animateSwap(View grid)
        {
            try
            {
                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                var a = new Animation();
                a.Add(0, 0.5, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 1.0, 0.8, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                a.Add(0.5, 1, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 0.8, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
                a.Commit(grid, "animation", 16, 2000, null, (d, f) =>
                {
                    grid.Scale = 1.0;
                    if (ShouldSwapAnimate)
                        animateSwap(grid);

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

                var btnVideo = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[6];
                if (string.IsNullOrEmpty(m.VideoUrl))
                    btnVideo.IsVisible = false;
                else
                    btnVideo.IsVisible = true;
               
                btnEdit = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[7];

                //btnEdit = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[7];
                btnSwap = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[1];
                btnChallenge = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[4];
                
                btnDeload = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[3];

            }
            catch (Exception ex)
            {

            }

        }

        private async void OnDeload(object sender, System.EventArgs e)
        {
            try
            {
                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
                ShouldDeloadAnimate = false;
                ShouldChallengeAnimate = false;
                ShouldEditAnimate = false;
                IsDeloadAnimated = false;
                
                //ConfirmConfig supersetConfig = new ConfirmConfig()
                //{
                //    Title = "Deload?",
                //    Message = "2 work sets and 5-10% less weight. Helps to recover.",
                //    OkText = "Deload",
                //    CancelText = AppResources.Cancel,
                //    OnAction = async (bool ok) =>
                //    {
                //        string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                //        string exId = $"{m.Id}";
                //        if (ok)
                //        {
                //            LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"deload");
                //            m.RecoModel = null;
                //            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                //                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                //            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                //            {
                //                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                //                m.Clear();
                //                try
                //                {

                //                }
                //                catch (Exception ex)
                //                {

                //                }
                //            }
                //            FetchReco(m, null);
                //        }
                //        else
                //            LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
                //    }
                //};
                //UserDialogs.Instance.Confirm(supersetConfig);

                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                string exId = $"{m.Id}";
                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") == null || LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "")
                {
                    AlertConfig supersetConfig = new AlertConfig()
                    {
                        Title = "Deload",
                        Message = m.IsBodyweight ? $"2 work sets and 15-20% fewer reps. Helps you recover. Deload?" : "2 work sets and 5-10% less weight. Helps you recover. Deload?",
                        OkText = "Deload",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),

                    };
                     await UserDialogs.Instance.AlertAsync(supersetConfig);
                    
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"deload");
                    //else
                    //{
                    //    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                    //    if (Device.RuntimePlatform.Equals(Device.Android))
                    //    HideContextButton();
                    //    return;
                    //}

                }
                else
                {
                    m.RecoModel.IsDeload = false;
                    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                }
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
                        contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#72DF40");
                    else
                        contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#ECFF92");
                }

                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                {
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                        contextMenuStack.Children[4].BackgroundColor = Color.FromHex("#72DF40");
                    else
                        contextMenuStack.Children[4].BackgroundColor = Color.FromHex("#ECFF92");
                }
                await FetchReco(m, null);

                AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                {
                    Title = "Smart challenges",
                    Message = "To keep you progressing fast.",//"The app will also suggest challenges and deloads automatically to keep you progressing fast.",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Got it",

                };

                await UserDialogs.Instance.AlertAsync(ShowExplainRIRPopUp);

                if (!isDeloadAsk)
                {
                    isDeloadAsk = true;
                    AlertConfig ShowExplainDeloadPopUp = new AlertConfig()
                    {
                        Title = "Smart swap",
                        Message = "Don't like an exercise? Swap it for another. Tap More and Swap.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Got it",

                    };

                    await UserDialogs.Instance.AlertAsync(ShowExplainDeloadPopUp);
                    ShouldEditAnimate = true;
                    IsSwapAnimated = true;
                    ShouldSwapAnimate = false;
                    animateEdit(btnEdit);
                }
                if (Device.RuntimePlatform.Equals(Device.Android))
                    HideContextButton();
                IsSwapAnimated = true;
            }
            catch (Exception ex)
            {

            }
        }

        private async void OnChallenge(object sender, System.EventArgs e)
        {
            try
            {

                ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;

                ShouldChallengeAnimate = false;
                IsDeloadAnimated = false;
                ShouldEditAnimate = false;
                IsAnimated = true;
                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                string exId = $"{m.Id}";
                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") == null || LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "")
                {
                    AlertConfig supersetConfig = new AlertConfig()
                    {
                        Title = $"{m.Label} challenge!",
                        Message = "Ready to smash a new record? Do as many reps are you can on your first work set. Be safe: stop before your form breaks down.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = AppResources.Challenge,
                    };
                    await UserDialogs.Instance.AlertAsync(supersetConfig);
                        if (m != null && m.RecoModel != null)
                            m.RecoModel.IsDeload = false;
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"max");
                       
                    //}
                    //else
                    //{
                    //    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
                    //    if (Device.RuntimePlatform.Equals(Device.Android))
                    //        HideContextButton();
                    //    return;
                    //}
                }
                else
                    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
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
                        contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#72DF40");
                    else
                        contextMenuStack.Children[3].BackgroundColor = Color.FromHex("#ECFF92");
                }

                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                {
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                        contextMenuStack.Children[4].BackgroundColor = Color.FromHex("#72DF40");
                    else
                        contextMenuStack.Children[4].BackgroundColor = Color.FromHex("#ECFF92");
                }
                await FetchReco(m, null);
                if (!IsWeightAsked)
                {
                    IsWeightAsked = true;
                    AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                    {
                        Title = "Weight too heavy?",
                        Message = "Try a deload. Tap More and Deload.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Got it",
                    };

                    await UserDialogs.Instance.AlertAsync(ShowExplainRIRPopUp);
                    ShouldEditAnimate = true;
                    IsDeloadAnimated = true;

                    animateEdit(btnEdit);
                }
                if (Device.RuntimePlatform.Equals(Device.Android))
                    HideContextButton();
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
            await PagesFactory.PushAsync<ExerciseVideoPage>();
            //OnCancelClicked(sender, e);
        }

        private async void OnSwap(object sender, System.EventArgs e)
        {
            
            IsSwapAnimated = false;
            ShouldSwapAnimate = false;
            SwapExerciseContext context = new SwapExerciseContext();
            context.WorkoutId = 105;
            context.SourceExerciseId = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter).Id;
            context.SourceBodyPartId = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter).BodyPartId;
            ExerciseWorkSetsModel model = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter);
            context.Label = model.Label;
            context.IsBodyweight = model.IsBodyweight;
            context.IsSystemExercise = model.IsSystemExercise;
            context.IsEasy = model.IsEasy;
            context.VideoUrl = model.VideoUrl;
            context.BodyPartId = model.BodyPartId;
            CurrentLog.Instance.SwapContext = context;

            CurrentLog.Instance.IsRecoveredWorkout = true;

            try
            {
                CurrentLog.Instance.RecommendationsByExercise.Clear();
                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Clear();
            }
            catch (Exception ex)
            {

            }
            OnCancelClicked(sender, e);

        }

        private async void OnRestore(object sender, System.EventArgs e)
        {
            ExerciseWorkSetsModel m = (ExerciseWorkSetsModel)((BindableObject)sender).BindingContext;
            SwapExerciseContext sec = ((App)Application.Current).SwapExericesContexts.Swaps.First(s => s.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && s.TargetExerciseId == m.Id);
            ((App)Application.Current).SwapExericesContexts.Swaps.Remove(sec);
            ((App)Application.Current).SwapExericesContexts.SaveContexts();
            OnCancelClicked(sender, e);
            await UpdateExerciseList();
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
            //ConfirmConfig p = new ConfirmConfig()
            //{
            //    Title = AppResources.ResetExercise, 
            //    Message = AppResources.AreYouSureYouWantToResetThisExerciseAndDeleteAllItsHistoryThisCannotBeUndone,// string.Format("Are you sure you want to reset this exercise and delete all its history? This cannot be undone.", m.Label),
            //    OkText = AppResources.Reset,
            //    CancelText = AppResources.Cancel,
            //    //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomRed)
            //};
            //p.OnAction = (obj) =>
            //{
            //    if (obj)
            //    {
            //        ResetExercisesAction(m);
            //        OnCancelClicked(sender, e);
            //    }
            //};
            //UserDialogs.Instance.Confirm(p);
        }

        void HideContextButton()
        {
            try
            {

                StackLayout s1 = (StackLayout)contextMenuStack.Parent;
                s1.Children[1].IsVisible = true;
                s1.Children[2].IsVisible = true;

                contextMenuStack.Children[0].IsVisible = false;
                contextMenuStack.Children[1].IsVisible = false;
                contextMenuStack.Children[2].IsVisible = false;
                contextMenuStack.Children[3].IsVisible = false;
                contextMenuStack.Children[4].IsVisible = false;
                contextMenuStack.Children[5].IsVisible = false;
                contextMenuStack.Children[6].IsVisible = true;
                contextMenuStack.Children[7].IsVisible = true;
                contextMenuStack = null;
            }
            catch (Exception ex)
            {

            }
        }

        void OnCancelClicked(object sender, System.EventArgs e)
        {
            StackLayout s = ((StackLayout)((Button)sender).Parent);

            StackLayout s1 = (StackLayout)s.Parent;
            s1.Children[1].IsVisible = true;
            s1.Children[2].IsVisible = true;
            s.Children[0].IsVisible = false;
            s.Children[1].IsVisible = false;
            s.Children[2].IsVisible = false;
            s.Children[3].IsVisible = false;
            s.Children[4].IsVisible = false;
            s.Children[5].IsVisible = false;
            s.Children[6].IsVisible = true;
            s.Children[7].IsVisible = true;
        }

        async void OnContextMenuClicked(object sender, System.EventArgs e)
        {


            if (contextMenuStack != null)
                HideContextButton();
            ShouldEditAnimate = false;

            

            StackLayout s = ((StackLayout)((Button)sender).Parent);
            ExerciseWorkSetsModel m = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter);
            if (m.IsNextExercise)
            {

                if (!IsAnimated)
                {
                    IsAnimated = true;
                    ShouldChallengeAnimate = true;
                    animateChallenge(btnChallenge);
                }

                if (IsDeloadAnimated)
                {
                    ShouldDeloadAnimate = true;
                    animateDeload(btnDeload);
                }
                if (IsSwapAnimated)
                {
                    if (!ShouldSwapAnimate)
                    { 
                        ShouldSwapAnimate = true;
                        animateSwap(btnSwap);
                    }
                }
                StackLayout s1 = (StackLayout)s.Parent;
                s1.Children[1].IsVisible = false;
                s1.Children[2].IsVisible = false;
            }
            string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
            string exId = $"{m.Id}";

            if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") != null)
            {
                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "deload")
                    s.Children[3].BackgroundColor = Color.FromHex("#72DF40");
                else
                    s.Children[3].BackgroundColor = Color.FromHex("#ECFF92");
            }

            if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
            {
                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                    s.Children[4].BackgroundColor = Color.FromHex("#72DF40");
                else
                    s.Children[4].BackgroundColor = Color.FromHex("#ECFF92");
            }



            s.Children[0].IsVisible = false;//!string.IsNullOrEmpty(m.VideoUrl);
            s.Children[1].IsVisible = IsSwapAnimated;
            s.Children[2].IsVisible = false;
            s.Children[3].IsVisible = true;
            s.Children[4].IsVisible = true;

            s.Children[5].IsVisible = false;
            s.Children[6].IsVisible = false;
            s.Children[7].IsVisible = false;
            contextMenuStack = s;


        }


        private async Task UpdateExerciseList()
        {
            var exercises = new ObservableCollection<ExerciseWorkSetsModel>();
            exerciseItems = new ObservableCollection<ExerciseWorkSetsModel>();
            try
            {

                var ee = CurrentLog.Instance.CurrentExercise;
                LblWorkoutName.Text = ee.Label;

                ExerciseWorkSetsModel e = new ExerciseWorkSetsModel()
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
                    IsSelected = false,
                    CountNo = $"1 of 1"

                };
                exercises.Add(e);
                //try
                //{
                //    long targetExerciseId = (long)((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == e.Id).TargetExerciseId;
                //    GetExerciseRequest req = new GetExerciseRequest();
                //    req.ExerciseId = targetExerciseId;
                //    ExerciceModel emm = await DrMuscleRestClient.Instance.GetExercise(req);

                //    var seriParent = JsonConvert.SerializeObject(emm);
                //    ExerciseWorkSetsModel em = JsonConvert.DeserializeObject<ExerciseWorkSetsModel>(seriParent);

                //    if ((Application.Current as App)?.FinishedExercices.FirstOrDefault(x => x.Id == em.Id) != null)
                //    {
                //        em.IsFinished = true;
                //        em.IsNextExercise = false;
                //    }
                //    em.IsSwapTarget = true;
                //    exercises.Add(em);
                //}
                //catch (Exception ex)
                //{
                //    SwapExerciseContext context = ((App)Application.Current).SwapExericesContexts.Swaps.First(c => c.WorkoutId == CurrentLog.Instance.CurrentWorkoutTemplate.Id && c.SourceExerciseId == e.Id);
                //    if (!string.IsNullOrEmpty(context.Label))
                //    {
                //        ExerciseWorkSetsModel model = new ExerciseWorkSetsModel()
                //        {
                //            Id = (long)context.TargetExerciseId,
                //            Label = context.Label,
                //            IsBodyweight = context.IsBodyweight,
                //            IsSwapTarget = true,
                //            IsSystemExercise = context.IsSystemExercise,
                //            VideoUrl = context.VideoUrl,
                //            IsEasy = context.IsEasy,
                //            BodyPartId = context.BodyPartId,
                //            CountNo = $"1 of 1"

                //        };
                //        if ((Application.Current as App)?.FinishedExercices.FirstOrDefault(x => x.Id == model.Id) != null)
                //        {
                //            model.IsFinished = true;
                //            model.IsNextExercise = false;
                //        }
                //        exercises.Add(model);

                //    }

                //}



                //var exModel = exercises.Where(x => x.IsFinished == false).FirstOrDefault();
                //if (exModel != null)
                //{
                //    exModel.IsNextExercise = true;
                //    ResetButtons();
                //}
                //else
                //    ChangeButtonsEmphasis();

                exerciseItems = exercises;
                //try
                //{
                //    if (CurrentLog.Instance.WorkoutsByExercise == null)
                //        CurrentLog.Instance.WorkoutsByExercise = new Dictionary<long, List<ExerciceModel>>();
                //    if (CurrentLog.Instance.WorkoutsByExercise.ContainsKey(CurrentLog.Instance.CurrentWorkoutTemplate.Id))
                //        CurrentLog.Instance.WorkoutsByExercise[CurrentLog.Instance.CurrentWorkoutTemplate.Id] = exercises.ToList();
                //    else
                //        CurrentLog.Instance.WorkoutsByExercise.Add(CurrentLog.Instance.CurrentWorkoutTemplate.Id, exercises.ToList());

                //}
                //catch (Exception ex)
                //{

                //}


                //var nextExer = exerciseItems.First();
                //nextExer.IsNextExercise = true;
                //if (nextExer != null)
                //{
                //    NewExerciseLogResponseModel newExercise = await DrMuscleRestClient.Instance.IsNewExerciseWithSessionInfo(new ExerciceModel() { Id = nextExer.Id });
                //    if (newExercise != null)
                //    {
                //        if (!newExercise.IsNewExercise)
                //        {
                //            // await FetchReco(nextExer);
                //            try
                //            {
                //                DateTime? lastLogDate = newExercise.LastLogDate;
                //                int? sessionDays = null;


                //                string WeightRecommandation;
                //                RecommendationModel reco = null;

                //                //Todo: clean up on 2019 01 18
                //                if (LocalDBManager.Instance.GetDBSetting("SetStyle") == null)
                //                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                //                if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                //                    LocalDBManager.Instance.SetDBSetting("QuickMode", "false");
                //                var bodyPartname = "";


                //                switch (nextExer.BodyPartId)
                //                {
                //                    case 1:

                //                        break;
                //                    case 2:
                //                        bodyPartname = "Shoulders";
                //                        break;
                //                    case 3:
                //                        bodyPartname = "Chest";
                //                        break;
                //                    case 4:
                //                        bodyPartname = "Back";
                //                        break;
                //                    case 5:
                //                        bodyPartname = "Biceps";
                //                        break;
                //                    case 6:
                //                        bodyPartname = "Triceps";
                //                        break;
                //                    case 7:
                //                        bodyPartname = "Abs";
                //                        break;
                //                    case 8:
                //                        bodyPartname = "Legs";
                //                        break;
                //                    case 9:
                //                        bodyPartname = "Calves";
                //                        break;
                //                    case 10:
                //                        bodyPartname = "Neck";
                //                        break;
                //                    case 11:
                //                        bodyPartname = "Forearm";
                //                        break;
                //                    default:
                //                        //
                //                        break;
                //                }
                //                if (CurrentLog.Instance.ExerciseLog == null)
                //                {
                //                    CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
                //                    CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(nextExer);
                //                }
                //                if (lastLogDate != null)
                //                {
                //                    var days = (int)(DateTime.Now - (DateTime)lastLogDate).TotalDays;
                //                    if (days >= 5 && days <= 9)
                //                        sessionDays = days;
                //                    if (days > 9)
                //                    {
                //                        ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                //                        {
                //                            Message = string.IsNullOrEmpty(bodyPartname) == false ? $"The last time you trained {bodyPartname.ToLower()} was {days} {AppResources.DaysAgoDoALightSessionToRecover}" : string.Format("{0} {1} {2} {3} {4}", "The last time you trained", CurrentLog.Instance.ExerciseLog.Exercice.Label, AppResources.was, days, AppResources.DaysAgoDoALightSessionToRecover),
                //                            // //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //                            OkText = AppResources.LightSession,
                //                            CancelText = AppResources.Cancel,
                //                        };
                //                        try
                //                        {
                //                            LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + CurrentLog.Instance.WorkoutTemplateCurrentExercise.Id + "Normal", DateTime.Now.AddDays(-1).ToString());
                //                            LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + CurrentLog.Instance.WorkoutTemplateCurrentExercise.Id + "RestPause", DateTime.Now.AddDays(-1).ToString());
                //                        }
                //                        catch (Exception ex)
                //                        {

                //                        }
                //                        var isConfirm = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                //                        if (isConfirm)
                //                        {
                //                            if (days > 50)
                //                                days = 50;
                //                            sessionDays = days;
                //                            //App.WorkoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id;
                //                            App.BodypartId = (int)CurrentLog.Instance.ExerciseLog.Exercice.BodyPartId;
                //                            App.Days = days;
                //                            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                //                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                //                            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(nextExer.Id))
                //                            {
                //                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(nextExer.Id);
                //                                nextExer.RecoModel = null;
                //                            }
                //                            //reco.Weight = new MultiUnityWeight(reco.Weight.Kg - ((reco.Weight.Kg * (decimal)1.5 * days) / 100), "kg");
                //                            //reco.WarmUpWeightSet1 = new MultiUnityWeight(reco.WarmUpWeightSet1.Kg - ((reco.WarmUpWeightSet1.Kg * (decimal)1.5 * days) / 100), "kg");
                //                            //reco.WarmUpWeightSet2 = new MultiUnityWeight(reco.WarmUpWeightSet2.Kg - ((reco.WarmUpWeightSet2.Kg * (decimal)1.5 * days) / 100), "kg");
                //                        }
                //                        else
                //                        {
                //                            sessionDays = null;
                //                            App.Days = 0;
                //                            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(nextExer.Id))
                //                            {
                //                                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(nextExer.Id);
                //                                nextExer.RecoModel = null;
                //                            }
                //                        }
                //                    }
                //                }


                //                await FetchReco(nextExer, sessionDays);
                //            }
                //            catch (Exception ex)
                //            {
                //                await FetchReco(nextExer, null);
                //            }

                //        }
                //        else
                //        {
                //            RunExercise(nextExer);
                //        }
                //    }
                //}
                //if (exerciseItems.Where(x => x.IsFinishWorkoutExe == true).FirstOrDefault() == null)
                //    exerciseItems.Add(new ExerciseWorkSetsModel() { IsFinishWorkoutExe = true });
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
                IsSelected = false,
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
            };
        }

        private async void ListTapped(object sender, EventArgs args)
        {
            if (contextMenuStack != null)
                HideContextButton();
        }


        private async void NewTapped(object sender, EventArgs args)
        {
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
                    Message = "Add exercises to and reorder any workout on the fly.",
                    Title = "Add exercises",
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
            CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
            CurrentLog.Instance.ExerciseLog.Exercice = GetExerciseModel(m);

            try
            {

                if (m.IsBodyweight && LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                {
                    decimal weight1 = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "lb").Kg;
                    KenkoAskForReps(weight1, m.Label, m);
                    return;
                }
                PromptConfig firsttimeExercisePopup = new PromptConfig()
                {
                    InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                    IsCancellable = true,
                    Title = string.Format("{0} {1}", CurrentLog.Instance.ExerciseLog.Exercice.Label, AppResources.Setup),
                    //Message = m.IsBodyweight ? string.Format("{0} ({1} {2})?", AppResources.WhatsYourBodyWeight, AppResources._in,
                    //LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "lbs" : "kg") :
                    //m.IsEasy ?
                    //string.Format("{0} {1} {2}",
                    //AppResources.HowMuchCanYou, m.Label, AppResources.VeryVeryVeryEasily6TimesIwillImproveOnYourGuessAfterYourFirstWorkout) :
                    Message = m.IsBodyweight ? m.IsTimeBased ? $"How many seconds can you {m.Label} very easily? I'll improve on your guess after your first workout." : $"How many {m.Label} can you do easily?" : m.IsTimeBased ? $"How many seconds can you {m.Label} very easily? I'll improve on your guess after your first workout." :
                        string.Format("{0} {1} {2}",
                          AppResources.HowMuchCanYou, m.Label, AppResources.VeryEasily6TimesIWillImproveOnYourGuessAfterYourFirstWorkout),
                    Placeholder = "Tap to enter your weight",
                    OkText = AppResources.Ok,
                    MaxLength = 4,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OnAction = async (weightResponse) =>
                    {
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


            }
        }

        protected void FirsttimeExercisePopup_OnTextChanged(PromptTextChangedArgs obj)
        {

            const string textRegex = @"^\d+(?:[\.,]\d{0,5})?$";
            var text = obj.Value.Replace(",", ".");
            bool IsValid = Regex.IsMatch(text, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (IsValid == false && !string.IsNullOrEmpty(obj.Value))
            {
                double result;
                obj.Value = obj.Value.Substring(0, obj.Value.Length - 1);
                double.TryParse(obj.Value, out result);
                obj.Value = result.ToString();
            }
        }

        protected void ExerciseRepsPopup_OnTextChanged(PromptTextChangedArgs obj)
        {
            const string textRegex = @"^\d+(?:)?$";
            bool IsValid = Regex.IsMatch(obj.Value, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (IsValid == false && !string.IsNullOrEmpty(obj.Value))
            {
                double result;
                obj.Value = obj.Value.Substring(0, obj.Value.Length - 1);
                double.TryParse(obj.Value, out result);
                obj.Value = result.ToString();
            }
        }

        protected async void KenkoAskForReps(decimal weight1, string exerciseName, ExerciseWorkSetsModel m)
        {
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = $"{m.Label}",
                Message = "How many can you do easily?",
                Placeholder = "Enter how many",
                OkText = AppResources.Save,
                MaxLength = 4,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (weightResponse.Ok)
                    {
                        if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                        {
                            return;
                        }
                        int reps = Convert.ToInt32(weightResponse.Value, CultureInfo.InvariantCulture);
                        SetUpCompletePopup(weight1, exerciseName, m, reps, true);
                    }
                    else
                        m.IsNextExercise = false;
                }
            };
            firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        //private async void FetchNextExerciseBackgroundData(ExerciseWorkSetsModel m)
        //{

        //    try
        //    {
        //        long? workoutId = null;


        //        string WeightRecommandation;
        //        RecommendationModel reco = null;



        //        string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;

        //        string exId = $"{m.Id}";

        //        long? swapedExId = null;

        //        if (m.RecoModel == null)
        //        {
        //            if (LocalDBManager.Instance.GetDBSetting("SetStyle").Value == "Normal")
        //            {

        //                m.RecoModel = await DrMuscleRestClient.Instance.GetRecommendationNormalRIRForExerciseWithoutLoader(new GetRecommendationForExerciseModel()
        //                {
        //                    Username = LocalDBManager.Instance.GetDBSetting("email").Value,
        //                    ExerciseId = m.Id,
        //                    IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false,
        //                    LightSessionDays = null,
        //                    WorkoutId = workoutId,
        //                    SwapedExId = swapedExId
        //                });

        //            }
        //            else
        //            {

        //                m.RecoModel = await DrMuscleRestClient.Instance.GetRecommendationRestPauseRIRForExerciseWithoutLoader(new GetRecommendationForExerciseModel()
        //                {
        //                    Username = LocalDBManager.Instance.GetDBSetting("email").Value,
        //                    ExerciseId = m.Id,
        //                    IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false,
        //                    LightSessionDays = null,
        //                    WorkoutId = workoutId,
        //                    SwapedExId = swapedExId
        //                });

        //            }
        //        }
        //        if (m.RecoModel != null)
        //        {
        //            if (m.RecoModel.IsDeload)
        //                m.RecoModel.IsMaxChallenge = false;
        //            m.RecoModel.IsLightSession = false;
        //            if (m.RecoModel.Reps <= 0)
        //                m.RecoModel.Reps = 1;
        //            if (m.RecoModel.NbRepsPauses <= 0)
        //                m.RecoModel.NbRepsPauses = 1;

        //            RecoContext.SaveContexts("Reco" + exId + setStyle, m.RecoModel);
        //            LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + exId + setStyle, DateTime.Now.AddDays(1).ToString());
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //}


        protected async void SetUpCompletePopup(decimal weight1, string exerciseName, ExerciseWorkSetsModel exe, int reps = 6, bool IsBodyweight = false)
        {

            NewExerciceLogModel model = new NewExerciceLogModel();
            model.ExerciseId = (int)CurrentLog.Instance.ExerciseLog.Exercice.Id;
            model.Username = "";
            reps += 1;
            if (IsBodyweight)
            {
                model.Weight1 = new MultiUnityWeight(weight1, "kg");
                model.Reps1 = reps.ToString();
                model.Weight2 = new MultiUnityWeight(weight1, "kg");
                model.Reps2 = (reps - 1).ToString();
                model.Weight3 = new MultiUnityWeight(weight1, "kg");
                model.Reps3 = (reps - 2).ToString();
                model.Weight4 = new MultiUnityWeight(weight1, "kg");
                model.Reps4 = (reps - 3).ToString();
            }
            else
            {
                weight1 = weight1 + (weight1 / 100);
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
            }

            //await DrMuscleRestClient.Instance.AddNewExerciseLogWithMoreSet(model);

            //ConfirmConfig confirmExercise = new ConfirmConfig()
            //{

            //    Title = $"Great! {exerciseName} is set up",
            //    Message = $"You said you can do Crunch {reps} times easily so I recommend you do {reps+2} today. Your first set is a warm-up. Do it, and tap \"Save set\".",
            //    OkText = AppResources.GotIt,
            //    CancelText = AppResources.Cancel,

            //    //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomRed),
            //    OnAction = async (bool obj) =>
            //    {

            //        //if (obj)
            //        //{
            //        //    //await PagesFactory.PushAsync<ExerciseChartPage>();

            //        //}
            //    }
            //};

            //UserDialogs.Instance.Confirm(confirmExercise);
            //FetchNextExerciseBackgroundData(exe);



            exe.RecoModel = GetRecommendationRestPauseRIRWithoutDeload(4, model.Weight1.Kg,
                model.Weight2.Kg,
                model.Weight3.Kg,
                int.Parse(model.Reps1),
             int.Parse(model.Reps2),
            int.Parse(model.Reps3), 12, 16, 0, false, false,false,
            model.Weight1.Kg,
            null, null, null,
            false, 0, null, false, false, 3);
            exe.IsNextExercise = true;
            await FetchReco(exe);

            var oneRMList = new List<OneRMModel>();
            OneRMModel oneRMModel1 = new OneRMModel()
            {
                ExerciseId = exe.Id,
                OneRM = new MultiUnityWeight(ComputeOneRM(weight1, reps - 1), "kg"),
                OneRMDate = DateTime.Now.AddDays(-7),
            };
            OneRMModel oneRMModel2 = new OneRMModel()
            {
                ExerciseId = exe.Id,
                OneRM = new MultiUnityWeight(ComputeOneRM(weight1, reps - 2), "kg"),
                OneRMDate = DateTime.Now.AddDays(-14),
            };
            OneRMModel oneRMModel3 = new OneRMModel()
            {
                ExerciseId = exe.Id,
                OneRM = new MultiUnityWeight(ComputeOneRM(weight1, reps - 3), "kg"),
                OneRMDate = DateTime.Now.AddDays(-21),
            };
            oneRMList.Add(oneRMModel1);
            oneRMList.Add(oneRMModel2);
            oneRMList.Add(oneRMModel3);
            CurrentLog.Instance.Exercise1RM.Clear();
            CurrentLog.Instance.Exercise1RM.Add(exe.Id, oneRMList);

            var min = LocalDBManager.Instance.GetDBSetting("repsminimum").Value;
            var max = LocalDBManager.Instance.GetDBSetting("repsmaximum").Value;
            var text = "";
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscle")
                {
                    text = "Build muscle";
                }
                else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscleBurnFat")
                {
                    text = "Build muscle and burn fat";
                }
                else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                {
                    text = "Burn fat";
                }
            }
            catch (Exception ex)
            {

            }
            var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
            var weightText = Math.Round(isKg ? exe.RecoModel.Weight.Kg : exe.RecoModel.Weight.Lb) ;
            var massUnit = isKg ? "kg" : "lbs";
            await UserDialogs.Instance.AlertAsync(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = "All settings fine tuned for your goals.",//"$"For your goal, you should do {min} reps with {weightText} {massUnit} today.",
                Title = $"{text}",
                OkText = AppResources.GotIt
            });
            var onerm1 = ComputeOneRM(exe.RecoModel.Weight.Kg, reps);

            decimal percent = (decimal)(52.2 + 41.9 * Math.Exp(-0.055 * int.Parse(min)));
            decimal recommendationInKg189 = onerm1 * percent / 100;

            var increased1RM189 = ComputeWeightforOneRM(onerm1 + (decimal)1.89, int.Parse(min));
            var increased1RM179 = ComputeWeightforOneRM(onerm1 + (decimal)1.89 + (decimal)1.78, int.Parse(max));
            var weightText1 = new MultiUnityWeight(increased1RM189, LocalDBManager.Instance.GetDBSetting("massunit").Value);
            var weightText2 = new MultiUnityWeight(increased1RM179, LocalDBManager.Instance.GetDBSetting("massunit").Value);

            var weight1Unit = Math.Round(weightText + (weightText * (decimal)0.0189), 2); //Math.Round(isKg ? weightText1.Kg : weightText1.Lb, 2);
            var weight2Unit = Math.Round(weight1Unit + (weight1Unit * (decimal)0.0189),2);//Math.Round(isKg ? weightText2.Kg : weightText2.Lb, 2);
            await UserDialogs.Instance.AlertAsync(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = $"Let's say you do more: {weight1Unit}(+1.89%) {massUnit}. The next time you squat, the app will recommend {weight2Unit} {massUnit} for {max} reps.",
                Title = $"You smashed it!",
                OkText = "Why?"
            });

            await UserDialogs.Instance.AlertAsync(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = $"Taking your reps into account, you progressed 1.89%. The next time your squat, I predict you'll progress 1.78%.",
                Title = $"Why?",
                OkText = "Got it"
            });

            await UserDialogs.Instance.AlertAsync(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = $"Let's say that was super easy. On the next popup, select \"I could have done 6 - 7 more\".",
                Title = $"How hard was it?",
                OkText = "Got it"
            });


            
                    AskRIR();

            var RIR = 0;
                async void AskRIR()
                {
                    var isTimeBased = CurrentLog.Instance.ExerciseLog.Exercice.IsTimeBased;

                    string DoneMore = null;
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                    {
                        while (DoneMore == null)
                        {
                            
                            DoneMore = await App.Current.MainPage.DisplayActionSheet(null, null, null,
                                AppResources.ThatWasVeryVeryHard,
                                isTimeBased ? "I could have done 1-2 more secs" : AppResources.ICouldHaveDone12MoreRep,
                                isTimeBased ? "I could have done 3-4 more secs" : AppResources.ICouldHaveDone34MoreReps,
                                isTimeBased ? "I could have done 5-6 more secs" : AppResources.IcouldHaveDone56MoreReps,
                                isTimeBased ? "I could have done 6-7 more secs" : "I could have done 6-7 more",
                                isTimeBased ? "I could have done 7+ more secs" : AppResources.ICouldHaveDone7PMoreReps);
                            CurrentLog.Instance.LastSetWas = DoneMore;
                            if (DoneMore == AppResources.ThatWasVeryVeryHard)
                            {
                                //Debug.WriteLine(DoneMore);
                                RIR = 0;

                                ProcessRIR(DoneMore);
                            }
                            else if (DoneMore == AppResources.ICouldHaveDone12MoreRep || DoneMore == "I could have done 1-2 more secs")
                            {
                                //Debug.WriteLine(DoneMore);
                                RIR = 1;

                                ProcessRIR(DoneMore);
                            }
                            else if (DoneMore == AppResources.ICouldHaveDone34MoreReps || DoneMore == "I could have done 3-4 more secs")
                            {
                                //Debug.WriteLine(DoneMore);
                                RIR = 2;

                                ProcessRIR(DoneMore);
                            }
                            else if (DoneMore == AppResources.IcouldHaveDone56MoreReps || DoneMore == "I could have done 5-6 more secs")
                            {
                                //Debug.WriteLine(DoneMore);
                                RIR = 3;
                                ProcessRIR(DoneMore);
                            }
                        else if (DoneMore == "I could have done 6-7 more" || DoneMore == "I could have done 6-7 more secs")
                        {
                            //Debug.WriteLine(DoneMore);
                            RIR = 4;
                            ProcessRIR(DoneMore);
                        }
                        else if (DoneMore == AppResources.ICouldHaveDone7PMoreReps || DoneMore == "I could have done 7+ more secs")
                            {
                                //Debug.WriteLine(DoneMore);
                                RIR = 5;
                                ProcessRIR(DoneMore);
                            }
                            else if (DoneMore == null)
                            {
                                if (Device.RuntimePlatform.Equals(Device.iOS))
                                    await App.Current.MainPage.DisplayAlert(AppResources.PleaseAnswer, AppResources.ImSorryIDidNotGetYourAnswerINeedToKnow, AppResources.TryAgain);
                                else
                                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                                    {
                                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                        Message = AppResources.ImSorryIDidNotGetYourAnswerINeedToKnow,
                                        Title = AppResources.PleaseAnswer,
                                        OkText = AppResources.TryAgain,
                                    });
                            }
                        }
                    }
                    else
                    {
                        ActionSheetConfig actionSheetConfig = new ActionSheetConfig();
                        actionSheetConfig.AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray);
                        actionSheetConfig.Add(AppResources.ThatWasVeryVeryHard, () =>
                        {
                            //Debug.WriteLine(DoneMore);
                            RIR = 0;

                            ProcessRIR(AppResources.ThatWasVeryVeryHard);
                        });
                        actionSheetConfig.Add(isTimeBased ? "I could have done 1-2 more secs" : AppResources.ICouldHaveDone12MoreRep, () =>
                        {
                            //Debug.WriteLine(DoneMore);
                            RIR = 1;
                            ProcessRIR(isTimeBased ? "I could have done 1-2 more secs" : AppResources.ICouldHaveDone12MoreRep);
                        });
                        actionSheetConfig.Add(isTimeBased ? "I could have done 3-4 more secs" : AppResources.ICouldHaveDone34MoreReps, () =>
                        {
                            //Debug.WriteLine(DoneMore);
                            RIR = 2;
                            ProcessRIR(isTimeBased ? "I could have done 3-4 more secs" : AppResources.ICouldHaveDone34MoreReps);
                        });
                        actionSheetConfig.Add(isTimeBased ? "I could have done 5-6 more secs" : AppResources.IcouldHaveDone56MoreReps, () =>
                        {
                            //Debug.WriteLine(DoneMore);
                            RIR = 3;
                            ProcessRIR(isTimeBased ? "I could have done 5-6 more secs" : AppResources.IcouldHaveDone56MoreReps);
                        });
                    actionSheetConfig.Add(isTimeBased ? "I could have done 6-7 more secs" : "I could have done 6-7 more", () =>
                    {
                        //Debug.WriteLine(DoneMore);
                        RIR = 4;
                        ProcessRIR(isTimeBased ? "I could have done 6-7 more secs" : "I could have done 6-7 more");
                    });
                    actionSheetConfig.Add(isTimeBased ? "I could have done 7+ more secs" : AppResources.ICouldHaveDone7PMoreReps, () =>
                        {
                            //Debug.WriteLine(DoneMore);
                            RIR = 5;
                            ProcessRIR(isTimeBased ? "I could have done 7+ more secs" : AppResources.ICouldHaveDone7PMoreReps);
                        });
                        UserDialogs.Instance.ActionSheet(actionSheetConfig);
                    }
                    //ActionSheetConfig actionSheetConfig = new ActionSheetConfig();
                    //actionSheetConfig.//AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGreen);
                    //actionSheetConfig.Add(AppResources.ThatWasVeryVeryHard, () =>
                    //{
                    //    Debug.WriteLine(DoneMore);
                    //    RIR = 0;
                    //    ProcessRIR(AppResources.ThatWasVeryVeryHard);
                    //});
                    //actionSheetConfig.Add(AppResources.ICouldHaveDone12MoreRep, () =>
                    //{
                    //    Debug.WriteLine(DoneMore);
                    //    RIR = 1;
                    //    ProcessRIR(AppResources.ICouldHaveDone12MoreRep);
                    //});
                    //actionSheetConfig.Add(AppResources.ICouldHaveDone34MoreReps, () =>
                    //{
                    //    Debug.WriteLine(DoneMore);
                    //    RIR = 2;
                    //    ProcessRIR(AppResources.ICouldHaveDone34MoreReps);
                    //});
                    //actionSheetConfig.Add(AppResources.IcouldHaveDone56MoreReps, () =>
                    //{
                    //    Debug.WriteLine(DoneMore);
                    //    RIR = 3;
                    //    ProcessRIR(AppResources.IcouldHaveDone56MoreReps);
                    //});
                    //actionSheetConfig.Add(AppResources.ICouldHaveDone7PMoreReps, () =>
                    //{
                    //    Debug.WriteLine(DoneMore);
                    //    RIR = 4;
                    //    ProcessRIR(AppResources.ICouldHaveDone7PMoreReps);
                    //});
                    //UserDialogs.Instance.ActionSheet(actionSheetConfig);





                    //};

                }
            
            async void ProcessRIR(string DoneMore)
                {
                    if (string.IsNullOrEmpty(DoneMore))
                    {
                        AskRIR();
                        return;
                    }
                    CurrentLog.Instance.LastSetWas = DoneMore;

                
                //decimal recommendationInKg = (onerm1 + (decimal)1.89 + (decimal)1.78) / (decimal)(1 + ((decimal)int.Parse(max) + RIR) / (decimal)30.0);
                var weight3Unit = Math.Round(weight2Unit + (weight2Unit * (decimal)0.0357), 2);
                var weightText3 = new MultiUnityWeight(weight3Unit, LocalDBManager.Instance.GetDBSetting("massunit").Value);

                //var weight3Unit = Math.Round(isKg ? weightText3.Kg : weightText3.Lb, 2);
                AlertConfig ShowRIRPopUp = new AlertConfig()
                        {
                            Title = "Weight adjusted automatically",
                            Message = "Every time you work out.",// $"Instead of {weight2Unit} {massUnit}, the app will recommend {weight3Unit} {massUnit} the next time you squat because you said {DoneMore.Replace("I ","you ")} today.",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = "Got it",
                        };
                        await UserDialogs.Instance.AlertAsync(ShowRIRPopUp);

                AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                {
                    Title = "Want fast progress?",
                    Message = "Try a Challenge. Tap More (right side) and Challenge.",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.GotIt,
                };

                await UserDialogs.Instance.AlertAsync(ShowExplainRIRPopUp);

                foreach (var item in exerciseItems)
                {
                    exe.RecoModel.Weight = weightText3;
                    exe.RecoModel.Reps = int.Parse(max) + RIR;
                    item.First().Weight = weightText3;
                    item.First().Reps = int.Parse(max) + RIR;
                    break;
                }

                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(exe.Id))
                {
                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(exe.Id);
                    exe.Clear();
                }
                await FetchReco(exe);
                ShouldEditAnimate = true;
                animateEdit(btnEdit);
                return;
                    
                }

            
        }



        public static RecommendationModel GetRecommendationRestPauseRIRWithoutDeload(int LastWorkoutNbSeriesForExercise,
                                          decimal Weight0,
                                          decimal Weight1,
                                          decimal Weight2,
                                          int Reps0,
                                          int Reps1,
                                          int Reps2,
                                          int RepsMinimum,
                                          int RepsMaximum,
                                          int? RIR,
                                          bool IsEasy,
                                          bool IsMedium,
                                          bool IsBodyweight,
                                          decimal? BodyWeight,
                                          decimal? Increments,
                                          decimal? Min,
                                          decimal? Max,
                                          bool isQuickMode,
                                          int? WarmupsValue,
                                          int? LightSessionDays,
                                          bool IsLastLightSession,
                                          bool? IsBackOffSet,
                                          int? SetCount
                                          )
        {

            // Calcul du 1RM pour les 3 entraînements précédents

            decimal OneRM0 = ComputeOneRM(Weight0, Reps0);
            decimal OneRM1 = ComputeOneRM(Weight1, Reps1);
            decimal OneRM2 = ComputeOneRM(Weight2, Reps2);


            // Fin calcul du 1RM pour les 3 entraînements précédents

            RecommendationModel result = new RecommendationModel();

            if (Increments != null)
                result.Increments = new MultiUnityWeight((decimal)Increments, "kg");
            if (Min != null)
                result.Min = new MultiUnityWeight((decimal)Min, "kg");
            if (Max != null)
                result.Max = new MultiUnityWeight((decimal)Max, "kg");

            // Calcul du progres en % entre les 2 derniers entraînements
            if (OneRM1 < 1)
                OneRM1 = 1;
            result.OneRMProgress = (OneRM0 - OneRM1) * 100 / OneRM1;

            // Fin calcul du progres en % entre les 2 derniers entraînements

            // FIN CALCULS PRÉALABLES            

            // RECOMMENDATION DU JOUR

            // NOMBRE DE REPS en fonction du nombre de reps au dernier workout

            int nbReps = 0;
            int spreadReps = RepsMaximum - RepsMinimum;
            int halfSpreadReps = spreadReps / 2;

            // Si en bas
            if (Reps0 <= (RepsMaximum - halfSpreadReps))
                // On va en haut
                nbReps = new Random().Next((RepsMaximum - halfSpreadReps), (RepsMaximum + 1));
            // Sinon
            else
                // On va en bas
                nbReps = new Random().Next(RepsMinimum, (RepsMinimum + halfSpreadReps + 1));

            // Reps for bodyweight exercises

            if (IsBodyweight)
            {
                // Reps = reps last workout + a number of reps based on last workout RIR
                if (IsEasy)
                {
                    if (RIR == 0)
                        nbReps = Reps0 - 3;
                    if (RIR == 1)
                        nbReps = Reps0 - 2;
                    if (RIR == 2)
                        nbReps = Reps0 - 1;
                    if (RIR == 3)
                        nbReps = Reps0;
                    if (RIR == 4 || RIR == null)
                        nbReps = Reps0 + 1;
                }
                else if (IsMedium)
                {
                    if (RIR == 0)
                        nbReps = Reps0 - 2;
                    if (RIR == 1)
                        nbReps = Reps0 - 1;
                    if (RIR == 2 || RIR == null)
                        nbReps = Reps0;
                    if (RIR == 3)
                        nbReps = Reps0 + 1;
                    if (RIR == 4)
                        nbReps = Reps0 + 2;
                }
                else
                {
                    if (RIR == 0)
                        nbReps = Reps0;
                    if (RIR == 1 || RIR == null)
                        nbReps = Reps0 + 1;
                    if (RIR == 2)
                        nbReps = Reps0 + 2;
                    if (RIR == 3)
                        nbReps = Reps0 + 4;
                    if (RIR == 4)
                        nbReps = Reps0 + 6;
                }
            }

            result.Reps = nbReps;

            // FIN NOMBRE DE REPS

            // POIDS en fonction des RIR

            decimal oneRMProgressRIR = 0;

            // oneRMProgressRIR en fonction des RIR

            if (IsEasy)
            {
                if (RIR == 0)
                    oneRMProgressRIR = (decimal)-0.03;
                if (RIR == 1)
                    oneRMProgressRIR = (decimal)-0.015;
                if (RIR == 2 || RIR == null)
                    oneRMProgressRIR = (decimal)-0.005;
                if (RIR == 3)
                    oneRMProgressRIR = 0;
                if (RIR == 4)
                    oneRMProgressRIR = (decimal)0.01;
            }
            else if (IsMedium)
            {
                if (RIR == 0)
                    oneRMProgressRIR = (decimal)-0.015;
                if (RIR == 1)
                    oneRMProgressRIR = (decimal)-0.005;
                if (RIR == 2 || RIR == null)
                    oneRMProgressRIR = 0;
                if (RIR == 3)
                    oneRMProgressRIR = (decimal)0.01;
                if (RIR == 4)
                    oneRMProgressRIR = (decimal)0.035;
            }
            else
            {
                if (RIR == 0)
                    oneRMProgressRIR = (decimal)-0.005;
                if (RIR == 1)
                    oneRMProgressRIR = 0;
                if (RIR == 2 || RIR == null)
                    oneRMProgressRIR = (decimal)0.01;
                if (RIR == 3)
                    oneRMProgressRIR = (decimal)0.035;
                if (RIR == 4)
                    oneRMProgressRIR = (decimal)0.05;
            }

            // On calcule l'augmentation (increase) prévue pour le 1RM aujourd'hui
            decimal OneRMIncreaseRIR = OneRM0 * oneRMProgressRIR;

            // On calcule le 1RM prévu pour aujourd'hui (OneRM00) en additionnant le 1RM le plus récent (OneRM0) et l'augmentation (increase) prévue pour le 1RM aujourd'hui
            decimal OneRM00 = OneRM0 + OneRMIncreaseRIR;

            // On calcule le poids recommandé aujourd'hui à partir du 1RM prévu pour aujourd'hui (OneRM00)            
            // Mayhew
            // decimal percent = (decimal)(52.2 + 41.9 * Math.Exp(-0.055 * nbReps));
            // decimal recommendationInKg = OneRM00 * percent / 100;

            // Epley
            // w 
            decimal recommendationInKg = OneRM00 / (decimal)(1 + ((decimal)nbReps / (decimal)30.0));

            // Ajustement increments
            if (Increments != null || Max != null || Min != null)
            {
                //Put recommendationInKg through RoundToNearestIncrement and return that result as result.Weight 
                var incrementValue = Increments != null ? (decimal)Increments : 1;
                result.Weight = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(recommendationInKg, incrementValue, Min, Max), WeightUnities.kg);

                //Compute new reps for new result.Weight (+1 because too easy)
                try
                {
                    if (!IsBodyweight)
                    {
                        result.Reps = RecoComputation.ComputeReps(result.Weight, OneRM0) + 1;
                        if (result.Reps < 1)
                            result.Reps = 1;
                    }
                }
                catch (Exception ex)
                {

                    System.Diagnostics.Debug.WriteLine($"{ex}");
                }

            }
            else
            {
                recommendationInKg = Math.Ceiling(recommendationInKg);
                result.Weight = new MultiUnityWeight(recommendationInKg, WeightUnities.kg);
            }

            // FIN POIDS

            // SÉRIES

            result.Series = 1;

            // FIN SÉRIES

            // REST-PAUSE SETS

            // NOMBRE DE PAUSES
            // En fonction du nombre de pauses au dernier workout (+1 par workout)
            // calculé comme total des séries - 2, pcq 2 séries w-u + 1 normal set à chaque entraînement

            int nbPauses = LastWorkoutNbSeriesForExercise - 2;

            // Fin en fonction du nombre de pauses au dernier workout (+1 par workout)

            // Ajustement des pauses en fonction du progrès réalisé entre les 2 derniers workouts

            if ((double)result.OneRMProgress >= 5)
            {
                nbPauses = nbPauses + 1;
            }
            else
            {
                if ((double)result.OneRMProgress >= 2 && (double)result.OneRMProgress < 5)
                {
                    nbPauses = nbPauses;
                }
                else
                {
                    if ((double)result.OneRMProgress >= -1 && result.OneRMProgress < 2)
                    {
                        nbPauses = nbPauses - 1;
                    }
                    else
                    {
                        // DELOAD
                        if ((double)result.OneRMProgress < -2 && OneRM0 < (OneRM1 - 2) && !IsLastLightSession)
                        {

                            // DELOAD PAUSES
                            result.IsDeload = true;
                            // nbPauses = 1;

                            // FIN DELOAD PAUSES

                            // DELOAD POIDS
                            // Si le 1RM est inférieur à 98% du 1RM précédent, je mets 90%.
                            // S'il est inférieur ainsi depuis 2 workouts, je mets 101% (de l'augmentation prévue par les RIR).
                            if (OneRM0 < (OneRM2 * (decimal)0.98) && OneRM1 < (OneRM2 * (decimal)0.98))
                            {
                                result.RecommendationInKg = recommendationInKg * (decimal)1.01;
                            }
                            else
                            {
                                result.RecommendationInKg = recommendationInKg * (decimal)0.90;
                            }

                            result.OneRMPercentage = (OneRM0 - OneRM1) / OneRM1;
                            // Ajustement increments
                            //if (Increments != null || Max != null || Min != null)
                            //{
                            //    //Put recommendationInKg through RoundToNearestIncrement and return that result as result.Weight 
                            //    var incrementValue = Increments != null ? (decimal)Increments : 1;
                            //    result.Weight = new MultiUnityWeight(RoundToNearestIncrement(recommendationInKg, incrementValue, Min, Max), WeightUnities.kg);
                            //}
                            //else
                            //    result.Weight = new MultiUnityWeight(recommendationInKg, WeightUnities.kg);

                            // FIN DELOAD POIDS
                        }
                        // FIN DELOAD
                    }
                }
            }

            // Fin ajustement des pauses en fonction du progrès réalisé entre les 2 derniers workouts

            // Protection nombre de pauses < 1 et > 5
            if (nbPauses < 1)
                nbPauses = 1;
            if (nbPauses > 5)
                nbPauses = 5;
            // Fin protection nombre de pauses < 2 et > 5

            // nombre de pauses pour les exercices (easy)
            if (IsEasy)
                nbPauses = 1;

            if (IsMedium)
            {
                //if (result.IsDeload)
                //    nbPauses = 1;
                //else
                nbPauses = 2;
            }


            nbPauses = 2;



            result.NbPauses = nbPauses;

            // FIN NOMBRE DE PAUSES

            // TEMPS DE REPOS ENTRE CHAQUE PAUSES EN SECONDES (géré dans l'app)

            // NOMBRE DE REPS DANS LES PAUSES

            int nbRepsPauses = result.Reps <= 5 ? (int)Math.Ceiling((decimal)result.Reps / (decimal)3) : (int)result.Reps / 3;
            result.NbRepsPauses = nbRepsPauses;

            if (result.NbRepsPauses <= 0)
                result.NbRepsPauses = 0;
            // FIN NOMBRE DE REPS DANS LES PAUSES

            // POIDS DES PAUSES
            // On veut le même poids que la série, soit "result.Weight"
            // FIN POIDS DES PAUSES

            // FIN REST-PAUSE SETS

            // WARM-UP SETS (IL Y EN A 2)

            // Poids des séries d'échauffement
            decimal wuWeight1 = recommendationInKg * (decimal)0.50;
            decimal wuWeight2 = recommendationInKg * (decimal)0.75;

            // Ajustement increments
            if (Increments != null || Max != null || Min != null)
            {
                //Put recommendationInKg through RoundToNearestIncrement and return that result as warm-up weights (wuWeight1 and wuWeight2)
                var incrementValue = Increments != null ? (decimal)Increments : 1;
                result.WarmUpWeightSet1 = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(wuWeight1, incrementValue, Min, Max), WeightUnities.kg);
                result.WarmUpWeightSet2 = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(wuWeight2, incrementValue, Min, Max), WeightUnities.kg);
            }
            else
            {
                result.WarmUpWeightSet1 = new MultiUnityWeight(wuWeight1, WeightUnities.kg);
                result.WarmUpWeightSet2 = new MultiUnityWeight(wuWeight2, WeightUnities.kg);
            }

            // Répétitions des séries d'échauffement
            int wuReps1 = result.Reps * 3 / 4;

            if (wuReps1 < 5)
                wuReps1 = 5;

            int wuReps2 = result.Reps / 2;

            if (wuReps2 < 3)
                wuReps2 = 3;

            result.WarmUpReps1 = wuReps1;
            result.WarmUpReps2 = wuReps2;

            // Temps de repos entre les séries d'échauffement en secondes (géré dans l'app)
            //Todo: Remove WarmUpRest1 et WarmUpRest2 du code (ici et reco)

            // FIN WARM-UP SETS

            // BODYWEIGHT EXERCISES

            if (IsBodyweight)
            {
                // Weight for bodyweight exercises
                result.Weight = new MultiUnityWeight(Convert.ToDecimal(BodyWeight), WeightUnities.kg);

                // Poids des séries d'échauffement for bodyweight exercises
                result.WarmUpWeightSet1 = new MultiUnityWeight(Convert.ToDecimal(BodyWeight), WeightUnities.kg);
                result.WarmUpWeightSet2 = new MultiUnityWeight(Convert.ToDecimal(BodyWeight), WeightUnities.kg);
                //result.WarmUpWeightSet2 = new MultiUnityWeight(wuWeight2, WeightUnities.kg);

                // Répétitions des séries d'échauffement for bodyweight exercises
                result.WarmUpReps1 = nbReps * 1 / 3;
                result.WarmUpReps2 = nbReps * 2 / 3;
            }

            // END BODYWEIGHT EXERCISES
            //if (LightSessionDays != null && LightSessionDays > 9)
            //{
            //    var incrementValue = Increments != null ? (decimal)Increments : 1;

            //    if (IsBodyweight)
            //        result.Reps = (int)Math.Ceiling((double)result.Reps * (3.0 / 4.0));
            //    else
            //        result.Weight = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(result.Weight.Kg - ((result.Weight.Kg * (decimal)1.5 * (int)LightSessionDays) / 100), incrementValue, Min, Max), "kg");
            //    result.NbPauses = 2;
            //}
            if (IsBackOffSet == true)
                result.BackOffSetWeight = new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(result.Weight.Kg - ((result.Weight.Kg * (decimal)30) / 100), Increments != null ? (decimal)Increments : 1, Min, Max), "kg");
            //START CUSTOM WARMUPS
            result.WarmUpsList = new List<WarmUps>();
            if (WarmupsValue != null)
            {
                if (WarmupsValue > 0)
                {

                    try
                    {


                        var incrementValue = Increments != null ? (decimal)Increments : 1;

                        var intialWeight = result.Weight.Kg / 2;
                        var newWarmup = WarmupsValue > 1 ? WarmupsValue - 1 : WarmupsValue;
                        var weightIncrement = (((result.Weight.Kg * (decimal)0.85) - (result.Weight.Kg * (decimal)0.5)) / (int)newWarmup);
                        var initialReps = (decimal)result.Reps * (decimal)0.75;

                        if (IsBodyweight && WarmupsValue == 1)
                            initialReps = (decimal)result.Reps * (decimal)0.675;
                        var repsIncrement = IsBodyweight ? (WarmupsValue == 1 ? result.Reps * (decimal)0.675 : result.Reps * (decimal)0.75 - result.Reps * (decimal)0.5) / newWarmup : (result.Reps * (decimal)0.75 - result.Reps * (decimal)0.4) / newWarmup;

                        if (IsBodyweight == false && initialReps < (decimal)5.01)
                        {
                            initialReps = 6;
                        }
                        var warmupCount = 0;
                        while (warmupCount < (int)WarmupsValue)
                        {
                            var newWarmsup = new WarmUps()
                            {
                                WarmUpWeightSet = IsBodyweight ? new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(result.Weight.Kg, incrementValue, Min, Max), WeightUnities.kg) : new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(intialWeight + (weightIncrement * warmupCount), incrementValue, Min, Max), WeightUnities.kg),
                                WarmUpReps = IsBodyweight ? (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * ((decimal)WarmupsValue - (decimal)(warmupCount + 1)))) : (int)Math.Ceiling(initialReps - ((decimal)repsIncrement * (decimal)warmupCount))
                            };
                            if (!IsBodyweight)
                                newWarmsup.WarmUpReps = newWarmsup.WarmUpReps < 3 ? 3 : newWarmsup.WarmUpReps;
                            result.WarmUpsList.Add(newWarmsup);
                            warmupCount++;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
            {
                //Adding computed warmups => Older way
                var incrementValue = Increments != null ? (decimal)Increments : 1;
                var warmups1 = new WarmUps()
                {
                    WarmUpReps = result.WarmUpReps1,
                    WarmUpWeightSet = LightSessionDays == null ? result.WarmUpWeightSet1 : new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(result.WarmUpWeightSet1.Kg - ((result.WarmUpWeightSet1.Kg * (decimal)1.5 * (int)LightSessionDays) / 100), incrementValue, Min, Max), "kg")
                };
                var warmups2 = new WarmUps()
                {
                    WarmUpReps = result.WarmUpReps2,
                    WarmUpWeightSet = LightSessionDays == null ? result.WarmUpWeightSet2 : new MultiUnityWeight(RecoComputation.RoundToNearestIncrement(result.WarmUpWeightSet2.Kg - ((result.WarmUpWeightSet2.Kg * (decimal)1.5 * (int)LightSessionDays) / 100), incrementValue, Min, Max), "kg")
                };
                warmups1.WarmUpReps = warmups1.WarmUpReps == 0 ? 1 : warmups1.WarmUpReps;
                warmups2.WarmUpReps = warmups2.WarmUpReps == 0 ? 1 : warmups2.WarmUpReps;
                if (IsEasy)
                {
                    var initialReps = (decimal)result.Reps * (decimal)0.75;
                    if (IsBodyweight == false && initialReps < (decimal)5.01)
                        initialReps = 6;
                    warmups1.WarmUpReps = (int)Math.Ceiling(initialReps);
                    warmups1.WarmUpWeightSet = LightSessionDays == null ? new MultiUnityWeight(RecoComputation.RoundToNearestIncrement((result.Weight.Kg / 2), incrementValue, Min, Max), "kg") : new MultiUnityWeight(RecoComputation.RoundToNearestIncrement((result.Weight.Kg / 2) - (((result.Weight.Kg / 2) * (decimal)1.5 * (int)LightSessionDays) / 100), incrementValue, Min, Max), "kg");
                    result.WarmUpsList.Add(warmups1);
                }
                else
                {
                    result.WarmUpsList.Add(warmups1);
                    result.WarmUpsList.Add(warmups2);
                }
            }
            //END CUSTOM WARMUPS
            result.IsEasy = IsEasy;
            result.IsMedium = IsMedium;

            result.IsBodyweight = IsBodyweight;

            result.IsNormalSets = false;

            if (result.Reps <= 0)
                result.Reps = 1;
            return result;
        }

        void Menu_Clicked(System.Object sender, System.EventArgs e)
        {
            SlideGeneralBotDemoAction();
        }
    }
}
