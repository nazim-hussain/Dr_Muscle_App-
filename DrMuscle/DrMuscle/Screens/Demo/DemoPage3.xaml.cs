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
using DrMuscle.Screens.User.OnBoarding;
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

namespace DrMuscle.Screens.Demo
{
    public partial class DemoPage3 : DrMusclePage, INotifyPropertyChanged
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
        bool isAppear3 = false;
        private GetUserProgramInfoResponseModel upi = null;
        private bool IsSettingsChanged { get; set; }
        StackLayout contextMenuStack;
        private IFirebase _firebase;
        private bool ShouldAnimate = false;
        private bool ShouldEditAnimate = false;

        public DemoPage3()
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
            if (CurrentLog.Instance.IsDemoPopingOut)
                return;
            CurrentLog.Instance.IsFromExercise = true;
            ShouldEditAnimate = true;
            exerciseItems = new ObservableCollection<ExerciseWorkSetsModel>();
            ExerciseListView.ItemsSource = GroupedData;
            App.IsDemo1Progress = true;
            UpdateExerciseList();
            isAppear2 = false;
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
            
        }

        private void UpdateWeoghtRepsMessageTapped(WorkoutLogSerieModelRef models)
        {
            
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
            if (CurrentLog.Instance.IsDemoPopingOut)
                return;
            _firebase.SetScreenName("demo_workout");
            if ( !isAppear2 && ShouldEditAnimate)
            {
                isAppear2 = true;
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = "You're new to training. If you want instructions, tap Video next to any exercise.",
                        Title = $"Watch exercise video",
                        OkText = "Try it out"
                    });
                else
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert($"Watch exercise video", "You're new to training. If you want instructions, tap Video next to any exercise.", "Try it out");
                    });
            }

            if (isAppear2 && !ShouldAnimate && CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("ExerciseVideoPage"))
            {
                await Task.Delay(300);
                CurrentLog.Instance.IsMovingOnBording = true;
                PagesFactory.PushAsync<BoardingBotPage>();
                return;
                CurrentLog.Instance.EndExerciseActivityPage = GetType();
                await Task.Delay(300);
               
                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                long programId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);

                var upi = new GetUserProgramInfoResponseModel()
                {
                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                };
                if (upi != null)
                {
                    WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
                    if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
                    {
                        try
                        {
                            nextWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkout(workoutTemplateId);
                            //nextWorkout = w.Workouts.First(ww => ww.Id == upi.NextWorkoutTemplate.Id);
                        }
                        catch (Exception ex)
                        {
                            await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            {
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                Message = AppResources.PleaseCheckInternetConnection,
                                Title = AppResources.ConnectionError
                            });
                            return;
                        }

                    }
                    if (nextWorkout != null)
                    {
                        CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
                        CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
                        CurrentLog.Instance.WorkoutStarted = true;
                        if (Device.RuntimePlatform.Equals(Device.Android))
                        {
                            App.IsDemoProgress = false;
                            App.IsWelcomeBack = true;
                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            CurrentLog.Instance.Exercise1RM.Clear();
                            await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await PagesFactory.PopToRootAsync(true);
                            });
                        }
                        else
                        {

                            App.IsDemoProgress = false;
                            App.IsWelcomeBack = true;
                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            CurrentLog.Instance.Exercise1RM.Clear();
                            await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                            await PagesFactory.PopToRootMoveAsync(true);
                        }

                    }
                    else
                    {
                        await PagesFactory.PopToRootAsync(true);
                    }
                }
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

        private async void PlateTapped(object sender, EventArgs e)
        {
            try
            {
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
        DrMuscleButton btnVideo;
        bool IsOpen = false;
        private async void CellHeaderTapped(object sender, EventArgs e)
        {


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
                ExerciseWorkSetsModel refModel = (ExerciseWorkSetsModel)((ViewCell)sender).BindingContext;
                if (refModel != null && refModel.Id == 16731)
                {
                    refModel.IsNextExercise = true;
                    btnEdit = ((DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[5]);
                    //((DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[0]).IsVisible = true;
                    //((DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[4]).IsVisible = true;
                    //((DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[5]).IsVisible = true;
                    //btnVideo = (DrMuscleButton)((StackLayout)((StackLayout)((StackLayout)((Frame)((Grid)((ViewCell)sender).View).Children[2]).Children[0]).Children[0]).Children[5]).Children[0];
                if (ShouldEditAnimate)
                    animateEdit(btnEdit);
                }

            }
            catch (Exception ex)
            {

            }

        }

        private async void OnDeload(object sender, System.EventArgs e)
        {
            return;
            
        }

        private async void OnChallenge(object sender, System.EventArgs e)
        {
            return;
            
        }

        private async void OnVideo(object sender, System.EventArgs e)
        {
            //if (contextMenuStack != null)
            //    HideContextButton();
            CurrentLog.Instance.VideoExercise = GetExerciseModel(((ExerciseWorkSetsModel)((Button)sender).CommandParameter));
            if (Device.RuntimePlatform.Equals(Device.iOS))
                DependencyService.Get<IOrientationService>().Landscape();
            await PagesFactory.PushAsync<ExerciseVideoPage>();
            ShouldAnimate = false;
            //OnCancelClicked(sender, e);
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
            return;

            //if (contextMenuStack != null)
            //    HideContextButton();
            StackLayout s = ((StackLayout)((Button)sender).Parent);
            ExerciseWorkSetsModel m = ((ExerciseWorkSetsModel)((Button)sender).CommandParameter);
            if (m.Id == 16731 && ShouldEditAnimate)
            {
                ShouldEditAnimate = false;
                ShouldAnimate = false;
                s.Children[0].IsVisible = true;//!string.IsNullOrEmpty(m.VideoUrl);
                s.Children[1].IsVisible = false;
                s.Children[2].IsVisible = false;
                s.Children[3].IsVisible = true;
                s.Children[4].IsVisible = true;

                s.Children[5].IsVisible = false;
                s.Children[6].IsVisible = false;
                animate(s.Children[0]);
            }
            //if (m.IsNextExercise)
            //{
            //    StackLayout s1 = (StackLayout)s.Parent;
            //    s1.Children[1].IsVisible = false;
            //    s1.Children[2].IsVisible = false;

            //    string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
            //    string exId = $"{m.Id}";

            //    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload") != null)
            //    {
            //        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "Deload").Value == "deload")
            //            s.Children[3].BackgroundColor = Color.FromHex("#72DF40");
            //        else
            //            s.Children[3].BackgroundColor = Color.FromHex("#ECFF92");
            //    }

            //    if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge") != null)
            //    {
            //        if (LocalDBManager.Instance.GetDBReco("RReps" + exId + setStyle + "challenge").Value == "max")
            //            s.Children[4].BackgroundColor = Color.FromHex("#72DF40");
            //        else
            //            s.Children[4].BackgroundColor = Color.FromHex("#ECFF92");
            //    }
            //}


            //s.Children[0].IsVisible = false;//!string.IsNullOrEmpty(m.VideoUrl);
            //s.Children[1].IsVisible = false;
            //s.Children[2].IsVisible = false;
            //s.Children[3].IsVisible = true;
            //s.Children[4].IsVisible = true;

            //s.Children[5].IsVisible = false;
            //s.Children[6].IsVisible = false;
            //contextMenuStack = s;


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
                    Id = 864,
                    IsBodyweight = ee.IsBodyweight,
                    IsEasy = ee.IsEasy,
                    IsNextExercise = ee.IsNextExercise,
                    IsSwapTarget = ee.IsSwapTarget,
                    IsFinished = true,
                    IsSystemExercise = ee.IsSystemExercise,
                    IsNormalSets = ee.IsNormalSets,
                    IsUnilateral = ee.IsUnilateral,
                    IsTimeBased = ee.IsTimeBased,
                    IsMedium = ee.IsMedium,
                    BodyPartId = ee.BodyPartId,
                    Label = "Crunch",
                    VideoUrl = ee.VideoUrl,
                    WorkoutGroupId = ee.WorkoutGroupId,
                    RepsMaxValue = ee.RepsMaxValue,
                    RepsMinValue = ee.RepsMinValue,
                    Timer = ee.Timer,
                    IsSelected = false,
                    CountNo = $"1 of 1"

                };
                exercises.Add(e);

                ExerciseWorkSetsModel e1 = new ExerciseWorkSetsModel()
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
                    CountNo = $"2 of 2"

                };
                exercises.Add(e1);
                
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
                // await UserDialogs.Instance.AlertAsync(AppResources.PleaseCheckInternetConnection, AppResources.Error);
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

        void Menu_Clicked(System.Object sender, System.EventArgs e)
        {
            SlideGeneralBotDemoAction();
        }
    }
}
