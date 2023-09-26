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
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.Exercises;
using DrMuscle.Screens.Me;
using DrMuscle.Screens.Workouts;
using DrMuscleWebApiSharedModel;
using Microsoft.AppCenter.Crashes;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DemoWorkoutPage : PopupPage
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
                StatusBarHeight.Height = 0;
            //double navigationBarHeight = Math.Abs(App.ScreenSize.Height - height - App.StatusBarHeight);
            // App.NavigationBarHeight = 146 + App.StatusBarHeight;// navigationBarHeight;

        }


        TimerPopup popup;
        bool isAppear = false;
        private GetUserProgramInfoResponseModel upi = null;
        private bool IsSettingsChanged { get; set; }
        StackLayout contextMenuStack;
        private IFirebase _firebase;
        private bool ShouldAnimate = false;
        private bool ShouldEditAnimate = false;
        public DemoWorkoutPage()
        {
            InitializeComponent();
            exerciseItems = new ObservableCollection<ExerciseWorkSetsModel>();
            ShouldAnimate = true;
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

            //SaveWorkoutButton.Clicked += SaveWorkoutButton_Clicked;
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });

            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            Timer.Instance.OnTimerStop += OnTimerStop;

            CurrentLog.Instance.IsFromExercise = true;
            App.IsDemo1Progress = true;
            UpdateExerciseList();
            if (LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}") == null || LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}").Value == null)
                LocalDBManager.Instance.SetDBSetting($"Time{DateTime.Now.Year}", $"{DateTime.Now.Ticks}");

            

        }

        
        private void RefreshLocalized()
        {
            Title = AppResources.ChooseExercise;
            // LblTodaysExercises.Text = AppResources.TodaYExercises;
            //  SaveWorkoutButton.Text = "Finish workout"; // AppResources.FinishAndSaveWorkout;
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
                            models.SetTitle = string.Format("Last time: {0} x {1}—for {2}{3:0.00}%, do:", item.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
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
                    if (models.IsFirstWorkSet && item.RecoModel != null)
                    {
                        var lastOneRM = ComputeOneRM(item.RecoModel.FirstWorkSetWeight.Kg, item.RecoModel.FirstWorkSetReps);
                        var currentRM = ComputeOneRM(models.Weight.Kg, models.Reps);
                        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                        var worksets = string.Format("{0} {1}", Math.Round(isKg ? item.RecoModel.FirstWorkSetWeight.Kg : item.RecoModel.FirstWorkSetWeight.Lb, 2), isKg ? "kg" : "lbs");

                        if (currentRM != 0)
                        {
                            var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                            models.SetTitle = string.Format("Last time: {0} x {1}—for {2}{3:0.00}%, do:", item.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
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
            if (Timer.Instance.State != "RUNNING")
                Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = models, IsFinishExercise = false }, "SaveSetMessage");
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
                        ((WorkoutLogSerieModelRef)item[index + 1]).SetTitle = models.RIR == 0 ? "OK, that was hard. Now do:" : "Got it! Now do:";
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
            _firebase.SetScreenName("demo_workout");
            if (!CurrentLog.Instance.IsDemoRunningStep2)
            {
                if (Device.RuntimePlatform.Equals(Device.Android))
                    UserDialogs.Instance.Alert($"Tap the exercise at the top to begin.", $"{LocalDBManager.Instance.GetDBSetting("firstname").Value}, welcome to this demo!", AppResources.GotIt);
                else
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert($"{LocalDBManager.Instance.GetDBSetting("firstname").Value}, welcome to this demo!", $"Tap the exercise at the top to begin.", AppResources.GotIt);
                    });
            }
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

            
            if (Navigation.NavigationStack.First() is MePage)
            {
                ChangeWorkout();
                return;
            }

            if (IsSettingsChanged)
            {
                try
                {

                    IsSettingsChanged = false;
                    ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
                    {
                        Message = $"Reload {CurrentLog.Instance.WorkoutTemplateCurrentExercise.Label}?",
                        Title = "Settings changed",
                        //  //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Reload",
                        CancelText = AppResources.Cancel,
                        OnAction = async (bool ok) =>
                        {
                            if (ok)
                            {
                                //Reload Settings
                                var item = exerciseItems.Where(x => x.Id == CurrentLog.Instance.WorkoutTemplateCurrentExercise.Id).FirstOrDefault();
                                if (item != null)
                                {
                                    item.RecoModel = null;
                                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(item.Id))
                                    {
                                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(item.Id);
                                        item.Clear();
                                        try
                                        {
                                            LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + item.Id + "Normal", DateTime.Now.AddDays(-1).ToString());
                                            LocalDBManager.Instance.SetDBReco("NbRepsGeneratedTime" + item.Id + "RestPause", DateTime.Now.AddDays(-1).ToString());
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                        item.RecoModel = null;
                                    }
                                    CellHeaderTapped(new Button() { BindingContext = item }, null);
                                }
                            }
                            else
                            {

                            }
                        }
                    };
                    await Task.Delay(100);
                    UserDialogs.Instance.Confirm(ShowWelcomePopUp2);

                }
                catch (Exception ex)
                {

                }
            }
            if (CurrentLog.Instance.IsAddingExerciseLocally)
            {
                await UpdateExerciseList();
                CurrentLog.Instance.IsAddingExerciseLocally = false;
            }

            try
            {
                if (CurrentLog.Instance.SwapContext != null)
                    CurrentLog.Instance.SwapContext = null;

            }
            catch (Exception ex)
            {

            }
            ExerciseListView.ItemsSource = exerciseItems;
        }

        private async void ChangeWorkout()
        {
            ConfirmConfig ShowConfirmPopUp = new ConfirmConfig()
            {
                Message = $"Change program and do {CurrentLog.Instance.CurrentWorkoutTemplate.Label} next?",
                Title = "",
                ////AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = "Select workout",
                CancelText = AppResources.Cancel,
                OnAction = async (bool ok) =>
                {
                    if (ok)
                    {
                        ChangingWorkout();
                    }
                    else
                    {
                        await PagesFactory.PopAsync();
                    }
                }
            };
            UserDialogs.Instance.Confirm(ShowConfirmPopUp);
        }

        private async void ChangingWorkout()
        {
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
            bool isSystem = false;
            BooleanModel successWorkoutLog = await DrMuscleRestClient.Instance.SaveWorkoutV2(new SaveWorkoutModel() { WorkoutId = CurrentLog.Instance.CurrentWorkoutTemplate.Id });
            try
            {
                if (successWorkoutLog.Result)
                {
                    Xamarin.Forms.MessagingCenter.Send<UpdatedWorkoutMessage>(new UpdatedWorkoutMessage(), "UpdatedWorkoutMessage");
                }
                LocalDBManager.Instance.SetDBSetting("last_workout_label", CurrentLog.Instance.CurrentWorkoutTemplate.Label);
                LocalDBManager.Instance.SetDBSetting("lastDoneProgram", CurrentLog.Instance.CurrentWorkoutTemplate.Id.ToString());
                isSystem = CurrentLog.Instance.CurrentWorkoutTemplate.IsSystemExercise;
            }
            catch (Exception ex)
            {

            }

            var nextworkoutName = CurrentLog.Instance.CurrentWorkoutTemplate.Label;
            CurrentLog.Instance.CurrentWorkoutTemplate = null;
            CurrentLog.Instance.WorkoutTemplateCurrentExercise = null;
            CurrentLog.Instance.WorkoutStarted = false;
            string fname = LocalDBManager.Instance.GetDBSetting("firstname").Value;

            try
            {
                AlertConfig p = new AlertConfig()
                {
                    Title = $"{AppResources.GotIt} {fname}!",
                    Message = $"Your next workout will be {nextworkoutName}.",
                    OkText = AppResources.Ok,
                    // //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGreen)
                };
                p.OnAction = async () =>
                {
                    await PagesFactory.PopToRootAsync();
                };
                UserDialogs.Instance.Alert(p);

            }

            catch (Exception ex)
            {
                await PagesFactory.PopToRootAsync();
            }

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
                    // //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
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

                   
                }
                model.IsNextExercise = false;
                ExerciseListView.IsCellUpdated = !ExerciseListView.IsCellUpdated;
                return;
            }
            ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
            {
                Title = $"Finish & save {model.Label}?",
                ////AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
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

                App.IsDemo1Progress = false;
                if (LocalDBManager.Instance.GetDBSetting($"Exercises{DateTime.Now.Date}") == null)
                    LocalDBManager.Instance.SetDBSetting($"Exercises{DateTime.Now.Date}", "1");
                else
                {
                    var exeCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"Exercises{DateTime.Now.Date}").Value);
                    exeCount += 1;
                    LocalDBManager.Instance.SetDBSetting($"Exercises{DateTime.Now.Date}", $"{exeCount}");
                }

            }
            catch (Exception ex)
            {

            }
            bool result = true;
            try
            {
                int? RIR = null;
                int max = 0;
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
                        if (!l.IsWarmups)
                        {
                            if (LocalDBManager.Instance.GetDBSetting($"Sets{DateTime.Now.Date}") == null)
                                LocalDBManager.Instance.SetDBSetting($"Sets{DateTime.Now.Date}", "1");
                            else
                            {
                                var setCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"Sets{DateTime.Now.Date}").Value);
                                setCount += 1;
                                LocalDBManager.Instance.SetDBSetting($"Sets{DateTime.Now.Date}", $"{setCount}");
                            }
                        }
                        
                    }
                }
                CurrentLog.Instance.BestReps = max;
                //CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].Increments
                DateTime? maxDate = null;
                try
                {
                    string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                    string exId = $"{model.Id}";
                    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                    {
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                        {
                            maxDate = DateTime.Now;
                        }
                    }

                   
                }
                catch (Exception ex)
                {

                }
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
                if (result)
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
                    if(!CurrentLog.Instance.IsDemoRunningStep2)
                        _firebase.LogEvent("demo_workout_1_done", "Done");
                    else
                        _firebase.LogEvent("demo_workout_2_done", "Done");
                    
                    if (PopupNavigation.Instance.PopupStack.Count > 0)
                        PopupNavigation.Instance.PopAsync();
                }
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


        private async void SaveWorkoutButton_Clicked(object sender, EventArgs e)
        {
            ConfirmConfig ShowConfirmPopUp = new ConfirmConfig()
            {
                Message = AppResources.AreYouSureYouAreFinishedAndWantToSaveTodaysWorkout,
                Title = AppResources.FinishAndSaveWorkoutQuestion,
                //  //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
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
        DrMuscleButton btnEdit;
        bool IsOpen = false;
        private async void CellHeaderTapped(object sender, EventArgs e)
        {

            try
            {
                
                if (CurrentLog.Instance.IsDemoRunningStep2)
                {
                    //StackLayout stk = (StackLayout)sender;
                    //StackLayout stk2 = (StackLayout)stk.Children.Last();
                    //btnEdit = (DrMuscleButton)stk2.Children.Last();


                    //Task.Factory.StartNew(async () =>
                    //{
                    //    await Task.Delay(500);
                    if (!IsOpen)
                    {
                        IsOpen = true;
                        await DisplayAlert($"{CurrentLog.Instance.BestReps} reps last time", $"Let's aim for {CurrentLog.Instance.BestReps + CurrentLog.Instance.RIR } reps today.", AppResources.GotIt);
                    }
                    //});
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

            try
            {

                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                LocalDBManager.Instance.SetDBReco("RReps" + m.Id + setStyle + "challenge", $"");

            }
            catch (Exception ex)
            {

            }
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
                    List<WorkoutLogSerieModelRef> setList = new List<WorkoutLogSerieModelRef>();

                   
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
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Exception is:{ex.ToString()}");
                    }
                }

                if (m.RecoModel == null)
                {
                   
                }
                if (m.RecoModel != null)
                {
                    
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
                        lbl3text = AppResources.AttentionTodayIsADeload;
                        iconOrange = "orange.png";
                    }
                    else if (m.RecoModel.IsMaxChallenge)
                    {
                        LocalDBManager.Instance.SetDBSetting("RecoDeload", "false");
                        lbl3text = "Today is a challenge";
                        iconOrange = "done2.png";
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("RecoDeload", "false");
                        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
                        {
                            if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
                            {
                                lbl3text = "Today is a challenge";
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
                                Id=m.Id,
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
                            SetTitle = j == 0 ? "1st work set—you got this:" : "All right! Now let's try:",
                            IsTimeBased = m.IsTimeBased,
                            IsUnilateral = m.IsUnilateral,
                            IsBodyweight = m.IsBodyweight
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
                            //rec.SetTitle = $"{dayString}: {m.RecoModel.FirstWorkSetReps} x {worksets}—let's try:";
                            var lastOneRM = ComputeOneRM(m.RecoModel.FirstWorkSetWeight.Kg, m.RecoModel.FirstWorkSetReps);
                            var currentRM = ComputeOneRM(m.RecoModel.Weight.Kg, rec.Reps);
                            if (currentRM != 0)
                            {
                                var percentage = (currentRM - lastOneRM) * 100 / currentRM;
                                rec.SetTitle = string.Format("Last time: {0} x {1}—for {2}{3:0.00}%, do:", m.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
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
                            rec.SetTitle = "1st work set—you got this:";
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
                                rec.SetTitle = string.Format("Last time: {0} x {1}—for {2}{3:0.00}%, do:", m.RecoModel.FirstWorkSetReps, worksets, percentage >= 0 ? "+" : "", percentage);
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
                        setList.First().SetTitle = $"{setList.Where(l => l.IsWarmups).ToList().Count} {warmString}, {setList.Where(l => !l.IsWarmups).ToList().Count} work sets—let's warm up:";
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
                            //  //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = AppResources.Ok,

                        };
                        UserDialogs.Instance.Alert(ShowExplainRIRPopUp);
                    }

                    if (CurrentLog.Instance.IsDemoRunningStep2 && LocalDBManager.Instance.GetDBSetting("DemoChallange") == null)
                    {
                        
                        LocalDBManager.Instance.SetDBSetting("DemoChallange", "");
                        AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                        {
                            Title = "Challenge time!",
                            Message = "Tap the more icon (upper-right) and \"Challenge\".",
                            //  //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = AppResources.GotIt,

                        };

                        if (btnEdit != null)
                        {
                            
                            await Task.Delay(1000);
                            MessagingCenter.Send<UpdateAnimationMessage>(new UpdateAnimationMessage() { ShouldAnimate = true }, "UpdateAnimationMessage");     
                        }

                        await UserDialogs.Instance.AlertAsync(ShowExplainRIRPopUp);
                        ShouldEditAnimate = true;
                        animateEdit(btnEdit);
                        
                    }
                    
                }
            }
        }

        public decimal ComputeOneRM(decimal weight, int reps)
        {
            return (decimal)(AppThemeConstants.Coeficent * reps) * weight + weight;
        }

        async void animate(View grid)
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
                    System.Diagnostics.Debug.WriteLine("ANIMATION ALL");
                    if (ShouldAnimate)
                        animate(grid);
                    
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

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            base.OnBindingContextChanged();
            try
            {

                ((ViewCell)sender).Height = 115;
                if (ShouldAnimate)
                    animate(((Grid)((ViewCell)sender).View));
                btnEdit = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[6];
                
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
                    ConfirmConfig supersetConfig = new ConfirmConfig()
                    {
                        Title = "Deload",
                        Message = "2 work sets and 5-10% less weight. Helps you recover. Deload?",
                        OkText = "Deload",
                        CancelText = AppResources.Cancel,

                    };
                    var res = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                    if (res)
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"deload");
                    else
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");

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
                FetchReco(m, null);

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

                //ConfirmConfig supersetConfig = new ConfirmConfig()
                //{
                //    Title = "Feeling strong?",
                //    Message = "We'll shoot for 10% more reps. Be safe: stop before your form breaks down.",
                //    //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //    OkText = AppResources.Challenge,
                //    CancelText = AppResources.Cancel,
                //    OnAction = async (bool ok) =>
                //    {
                //        string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                //        string exId = $"{m.Id}";
                //        if (ok)
                //        {
                //            LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"max");
                //            m.RecoModel = null;
                //            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                //                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                //            if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.ContainsKey(m.Id))
                //            {
                //                CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Remove(m.Id);
                //                m.Clear();
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
                if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") == null || LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "")
                {
                    ConfirmConfig supersetConfig = new ConfirmConfig()
                    {
                        Title = "Challenge",
                        Message = "Feeling strong? 10-15% more reps. Be safe: stop before your form breaks down.",
                        //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = AppResources.Challenge,
                        CancelText = AppResources.Cancel,

                    };
                    var res = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                    if (res)
                    {
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"max");
                        if (CurrentLog.Instance.IsDemoRunningStep2 && LocalDBManager.Instance.GetDBSetting("ChallengeTriggered") == null)
                        {
                            LocalDBManager.Instance.SetDBSetting("ChallengeTriggered", "");
                            if (ShouldEditAnimate)
                            {
                                ShouldEditAnimate = false;
                                MessagingCenter.Send<UpdateAnimationMessage>(new UpdateAnimationMessage() { ShouldAnimate = false }, "UpdateAnimationMessage");
                            }
                            AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                            {
                                Title = "You got it!",
                                Message = "You made this exercise harder. You can also make it easier with Deload.",
                                //  //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Got it",

                            };
                            UserDialogs.Instance.Alert(ShowExplainRIRPopUp);
                        }
                    }
                    else
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
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
                FetchReco(m, null);


            }
            catch (Exception ex)
            {

            }
        }

        private async void OnVideo(object sender, System.EventArgs e)
        {
            if (contextMenuStack != null)
                HideContextButton();
            CurrentLog.Instance.VideoExercise = GetExerciseModel(((ExerciseWorkSetsModel)((Button)sender).CommandParameter));
            await PagesFactory.PushAsync<ExerciseVideoPage>();
            OnCancelClicked(sender, e);
        }

        private async void OnSwap(object sender, System.EventArgs e)
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
            CurrentLog.Instance.SwapContext = context;
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
        }

        async void OnContextMenuClicked(object sender, System.EventArgs e)
        {

          
                    if (contextMenuStack != null)
                        HideContextButton();
                    StackLayout s = ((StackLayout)((Button)sender).Parent);
                    ExerciseWorkSetsModel m = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter);
            if (m.IsNextExercise)
            {
                StackLayout s1 = (StackLayout)s.Parent;
                        s1.Children[1].IsVisible = false;
                        s1.Children[2].IsVisible = false;

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
        }


        s.Children[0].IsVisible = false;//!string.IsNullOrEmpty(m.VideoUrl);
                    s.Children[1].IsVisible = false;
                    s.Children[2].IsVisible = false;
                    s.Children[3].IsVisible = true;
                    s.Children[4].IsVisible = true;

                    s.Children[5].IsVisible = false;
                    s.Children[6].IsVisible = false;
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
                //await UserDialogs.Instance.AlertAsync(AppResources.PleaseCheckInternetConnection, AppResources.Error);
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
                    // //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
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
                    decimal weight1 = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture);
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
                                return;
                            }
                            var weightText = weightResponse.Value.Replace(",", ".");
                            decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);
                            if (m.IsBodyweight)
                            {
                                LocalDBManager.Instance.SetDBSetting("BodyWeight", weight1.ToString());
                                await DrMuscleRestClient.Instance.SetUserBodyWeight(new UserInfosModel()
                                {
                                    BodyWeight = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value)
                                });
                                KenkoAskForReps(weight1, m.Label, m);
                                return;
                            }
                            SetUpCompletePopup(weight1, m.Label, m);
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
                Title = $"{m.Label}: how many can you do easily?",
                //Message = m.IsTimeBased ? $"How many seconds can you {m.Label} very easily? I'll improve on your guess after your first workout." : $"how many can you do easily?",
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

        private async void FetchNextExerciseBackgroundData(ExerciseWorkSetsModel m)
        {

            try
            {
                long? workoutId = null;


                string WeightRecommandation;
                RecommendationModel reco = null;



                string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;

                string exId = $"{m.Id}";

                long? swapedExId = null;

                if (m.RecoModel == null)
                {
                    if (LocalDBManager.Instance.GetDBSetting("SetStyle").Value == "Normal")
                    {

                        m.RecoModel = await DrMuscleRestClient.Instance.GetRecommendationNormalRIRForExerciseWithoutLoader(new GetRecommendationForExerciseModel()
                        {
                            Username = LocalDBManager.Instance.GetDBSetting("email").Value,
                            ExerciseId = m.Id,
                            IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false,
                            LightSessionDays = null,
                            WorkoutId = workoutId,
                            SwapedExId = swapedExId
                        });

                    }
                    else
                    {

                        m.RecoModel = await DrMuscleRestClient.Instance.GetRecommendationRestPauseRIRForExerciseWithoutLoader(new GetRecommendationForExerciseModel()
                        {
                            Username = LocalDBManager.Instance.GetDBSetting("email").Value,
                            ExerciseId = m.Id,
                            IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false,
                            LightSessionDays = null,
                            WorkoutId = workoutId,
                            SwapedExId = swapedExId
                        });

                    }
                }
                if (m.RecoModel != null)
                {
                    if (m.RecoModel.IsDeload)
                        m.RecoModel.IsMaxChallenge = false;
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


        protected async void SetUpCompletePopup(decimal weight1, string exerciseName, ExerciseWorkSetsModel exe, int reps = 6, bool IsBodyweight = false)
        {

            NewExerciceLogModel model = new NewExerciceLogModel();
            model.ExerciseId = (int)CurrentLog.Instance.ExerciseLog.Exercice.Id;
            model.Username = LocalDBManager.Instance.GetDBSetting("email").Value;

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
            FetchNextExerciseBackgroundData(exe);

            FetchReco(exe);
            UserDialogs.Instance.AlertAsync($"I recommend you do {reps + 1} today. Tap \"Save set\" to continue.", $"You can do {reps} crunches easily", AppResources.GotIt);

        }
    }
}
