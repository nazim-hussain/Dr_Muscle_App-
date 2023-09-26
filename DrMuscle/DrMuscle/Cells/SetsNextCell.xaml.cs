using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.Exercises;
using DrMuscle.Screens.User;
using DrMuscle.Views;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetsNextCell : ViewCell
    {
        private decimal currentWeight = 0;
        private int currentReps = 0;
        public static event Action ViewCellSizeChangedEvent;

        bool ShouldAnimate = false;
        bool ShouldFinishAnimate = false;
        bool DemoSecondSteps = false;

        public decimal weightStep = LocalDBManager.Instance.GetDBSetting("massunit") == null ? 2 : LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (decimal)2 : (decimal)5;
        public SetsNextCell()
        {
            InitializeComponent();

            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            Timer.Instance.OnTimerStop += OnTimerDone;
            WeightEntry.Unfocused += WeightEntry_Unfocused;
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                //RepsEntry.HeightRequest = 50;
                //WeightEntry.HeightRequest = 33;
                //RepsMore.HeightRequest = 33;
                //RepsLess.HeightRequest = 33;
                this.ForceUpdateSize();
            }

            MessagingCenter.Subscribe<Message.FinishSetReceivedFromWatchOS>(this, "FinishSetReceivedFromWatchOS", (obj) =>
            {
                SaveSetFromWatchTapped(obj);
            });

            if (App.IsDemoProgress)
            {

                if (CurrentLog.Instance.IsDemoRunningStep2)
                {
                    MessagingCenter.Subscribe<Message.UpdateAnimationMessage>(this, "UpdateAnimationMessage", (obj) =>
                    {
                        UpdateAnimation(obj.ShouldAnimate);
                    });
                    DemoSecondSteps = false;
                    ShouldFinishAnimate = false;
                    ShouldAnimate = false;
                }
                else
                {
                    if (!CurrentLog.Instance.IsRestarted)
                    {
                        ShouldFinishAnimate = true;
                        ShouldAnimate = true;
                        SetAnimation();
                        SetFinishAnimation();
                    }
                }
            }
        }

        private async void SetAnimation()
        {
            if (App.IsDemoProgress)
            {
                animate(BtnSaveSet);

            }
        }

        private async void SetFinishAnimation()
        {
            //if (App.IsDemoProgress)
            //    animateFinish(FinishExercise);

        }

        async void animate(View grid)
        {
            try
            {
                if (CurrentLog.Instance.IsRestarted)
                {
                    ShouldAnimate = false;
                    return;
                }
                if (!App.IsDemoProgress)
                {
                    ShouldAnimate = false;
                    return;
                }
                if (!App.IsDemo1Progress)
                {
                    ShouldAnimate = false;
                    return;
                }
                if (DemoSecondSteps)
                {
                    await Task.Delay(2000);
                    animate(grid);
                }
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
                    if (ShouldAnimate)
                        animate(grid);
                });

            }
            catch (Exception ex)
            {
                ShouldAnimate = false;
            }
        }

        async void animateFinish(View grid)
        {
            try
            {
                if (CurrentLog.Instance.IsRestarted)
                {
                    ShouldFinishAnimate = false;
                    return;
                }
                if (!App.IsDemoProgress)
                    ShouldFinishAnimate = false;
                if (!App.IsDemo1Progress)
                {
                    ShouldAnimate = false;
                    return;
                }
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
                    if (ShouldFinishAnimate)
                        animateFinish(grid);
                });

            }
            catch (Exception ex)
            {
                ShouldFinishAnimate = false;
            }
        }

        private void Item_OnSizeChanged(object sender, EventArgs args)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    this.ForceUpdateSize();
                    ViewCellSizeChangedEvent?.Invoke();
                }
            });
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            try
            {

                if (workout != null)
                {
                    workout.OnSizeChanged += Item_OnSizeChanged;
                    //videoPlayer.Source = string.IsNullOrEmpty(workout.VideoUrl) ? null : FormsVideoLibrary.VideoSource.FromResource(workout.VideoUrl);
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                    {
                        if (workout.IsVideoUrlAvailable)
                        {
                            videoPlayer.Source = null;
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                videoPlayer.Source = workout.VideoUrl;   
                            });
                        }
                    }
                    var isKg = false;
                    try
                    {
                        isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg" ? true : false;
                    }
                    catch (Exception ex)
                    {

                    }
                    //if (!string.IsNullOrEmpty(workout.HeaderTitle) && workout.IsHeaderCell)
                    //{
                    //    HeaderDescStack.IsVisible = true;
                        
                        
                    //}
                    //else
                    //    HeaderDescStack.IsVisible = false;
                    repsTypeLabel.Text = workout.IsTimeBased ? "SEC" : "REPS";
                    massUnitLabel.Text = isKg ? "KG" : "LBS";

                    if (App.IsShowTooltip)
                    {
                        if (workout.IsHeaderCell)
                        {

                        }
                    }

                    currentWeight = isKg ? workout.Weight.Kg : Convert.ToDecimal(workout.WeightDouble.ReplaceWithDot(), CultureInfo.InvariantCulture);
                    currentReps = workout.Reps;
                    //RepsEntry.Text = string.Format("{0}", currentReps);
                    //LblReps.Text = string.Format("{0}", currentReps);

                    //WeightEntry.Text = isKg ?
                    //string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Kg, 1),2)) :
                    //string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Lb, (decimal)2.5),2));
                    //WeightText.Text = workout.IsBodyweight ? "Bodyweight" : isKg ? "kg" : "lbs";
                    if (workout.EquipmentId == 4 || workout.EquipmentId == 3)
                    {
                        WeightText.IsVisible = true;
                        //WeightEntry.HorizontalTextAlignment = TextAlignment.Center;
                        WeightText.Text = workout.EquipmentId == 4 ? workout.IsOneHanded ? "" : "per hand" : "bar + plates";
                    }
                    
                    else
                        WeightText.IsVisible = false;

                    PerSideText.IsVisible = workout.Id == 12983 || workout.Id == 14289 || workout.Id == 14312 || workout.Id == 22123 || workout.Id == 12985;
                    //WeightText.FontSize = workout.IsBodyweight ? 16 : 14;
                    if (workout.Id == 16508)
                    {
                        //WeightText.Text = workout.IsWarmups ? "Brisk" : "Fast";
                        //WeightText.FontSize = 26;
                    }
                    else if (workout.BodypartId == 12)
                    {
                        //WeightText.Text = workout.IsWarmups ? "Brisk" : workout.IsFirstWorkSet ? "Fast" : "Cooldown";
                        //WeightText.FontSize = 26;
                        //if (WeightText.Text == "Cooldown")
                        //    WeightText.FontSize = 18;
                    }
                    if (workout.Id >= 16897 && workout.Id <= 16907 || workout.Id == 14279 || workout.Id >= 21508 && workout.Id <= 21514)
                    {
                        //WeightText.Text = "Bands";
                        //WeightText.FontSize = 26;
                    }

                    //LblWeight.Text = isKg ?
                    //                                                    string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Kg, 1), 1)) :
                    //                                                    string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Lb, (decimal)2.5), 1));
                    //LblMassUnit.Text = workout.IsBodyweight ? "" : isKg ? "kg" : "lbs";
                    if (workout.IsEditing)
                        BtnFinishSet.Text = "Save";
                    else if (workout.IsExtraSet && !workout.IsFinished)
                    {
                        BtnFinishSet.Text = workout.IsFirstSide ? "Save set & side 1" : "Save set & exercise";
                        UnFinishedExercises.Text = "Save set & do 1 more";
                    }
                    else
                        BtnFinishSet.Text = "Save set";
                    //if (!workout.IsLastSet)
                    //    FinishExercise.IsVisible = false;
                    if (workout.IsTimeBased && workout.IsHeaderCell && workout.ShowWorkTimer)
                        BtnFinishSet.Text = "Start set";
                    try
                    {
                        if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null || workout.Increments != null)
                        {
                            if (workout.Increments != null)
                            {
                                var unit = workout.Increments;
                                var resultLb = unit.Lb;
                                if (!isKg)
                                {
                                    var result = unit.Lb - Math.Truncate(unit.Lb);

                                    resultLb = result > (decimal)0.001 ?
                                                                                     Math.Round(unit.Lb, 2) :
                                                                                     Math.Round(unit.Lb, 0);
                                }
                                weightStep = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? unit.Kg : resultLb;
                            }
                            else
                            {
                                var increment = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value.ReplaceWithDot(), System.Globalization.CultureInfo.InvariantCulture);
                                var unit = new MultiUnityWeight(increment, "kg", false);
                                weightStep = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? unit.Kg : unit.Lb;
                            }
                        }
                        else
                            weightStep = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (decimal)2 : (decimal)5;
                    }
                    catch (Exception ex)
                    {
                        try
                        {

                            weightStep = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (decimal)2 : (decimal)5;

                        }
                        catch (Exception)
                        {

                        }
                    }
                    //Device.BeginInvokeOnMainThread(() => {
                    //    if (Device.RuntimePlatform.Equals(Device.iOS))
                    //        System.Diagnostics.Debug.WriteLine($"View Width: {this.View.Width}, Height {this.View.Height}");
                    //    this.ForceUpdateSize();
                    //    SizeRequest field = this.View.Measure(double.PositiveInfinity, double.PositiveInfinity);
                    //    System.Diagnostics.Debug.WriteLine($"infinity Width: {field.Request.Width}, Height {field.Request.Height}");
                    //});
                }

            }
            catch (Exception ex)
            {

            }
        }

        public async void UpdateAnimation(bool IsAnimate)
        {
            DemoSecondSteps = IsAnimate;
            if (!IsAnimate)
            {
                ShouldFinishAnimate = true;
                ShouldAnimate = true;
                SetAnimation();
                SetFinishAnimation();
            }
        }

        private async void SaveSetFromWatchTapped(FinishSetReceivedFromWatchOS m)
        {
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null && workout == m.model)
            {
                if (m.WatchMessageType == WatchMessageType.SaveSet)
                    this.SaveSet_Clicked(BtnSaveSet, EventArgs.Empty);
                else if (m.WatchMessageType == WatchMessageType.RepsLess)
                    this.RepsLess_Clicked(new DrMuscleButton(), EventArgs.Empty);
                else if (m.WatchMessageType == WatchMessageType.RepsMore)
                    this.RepsMore_Clicked(new DrMuscleButton(), EventArgs.Empty);
                else if (m.WatchMessageType == WatchMessageType.WeightLess)
                    this.WeightLess_Clicked(new DrMuscleButton(), EventArgs.Empty);
                else if (m.WatchMessageType == WatchMessageType.WeightMore)
                    WeightMore_Clicked(new DrMuscleButton(), EventArgs.Empty);

            }
        }
        async void WeightEntry_Unfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            //if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsBodyweight && currentWeight != 0)
            //{
            //    await DrMuscleRestClient.Instance.SetUserBodyWeight(new UserInfosModel()
            //    {
            //        BodyWeight = new MultiUnityWeight(currentWeight, LocalDBManager.Instance.GetDBSetting("massunit").Value)
            //    });
            //}

            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                    workout.ShouldUpdateIncrement = true;
                    workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb, false);
                    MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout }, "SendWatchMessage");
                    if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                        Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                }
            }

        }

        private bool CheckWelcomeTooltip()
        {
            if (App.IsShowTooltip)
            {
                if (App.WelcomeTooltop[1] == false)
                {
                    LblCoachTips.Effects.Clear();
                    DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(StackReps, true);
                    App.WelcomeTooltop[1] = true;
                    return true;
                }

                if (App.WelcomeTooltop[2] == false)
                {
                    StackReps.Effects.Clear();
                    DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(BtnSaveSet, true);
                    App.WelcomeTooltop[2] = true;
                    return true;
                }
            }
            if (App.IsShowBackOffTooltip)
            {

                if (App.BackoffTooltop[1] == false)
                {
                    //StackRepsConatiner.Effects.Clear();
                    DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(StackWeight, true);
                    App.BackoffTooltop[1] = true;
                    return true;
                }

                if (App.BackoffTooltop[2] == false)
                {
                    StackWeight.Effects.Clear();
                    DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(BtnFinishSet, true);
                    App.BackoffTooltop[2] = true;
                    App.IsShowBackOffTooltip = false;
                    return true;
                }
            }
            return false;
        }

        void OnTimerChange(int remaining)
        {
            try
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout == null)
                    return;
                if (workout != null && !workout.IsEditing && workout.IsActive)
                {
                    if (Timer.Instance.State == "RUNNING")
                    {
                        BtnFinishSet.Text = $" Save set (rest {remaining.ToString()})";
                        UnFinishedExercises.Text = workout.IsFirstSide ? "Finish side 1" : "Finish exercise";
                    }
                    else
                        BtnFinishSet.Text = $"Save set";
                }
                else if (workout.IsEditing)
                {
                    BtnFinishSet.Text = $"Save";
                }

                //var percentage = (float)remaining / Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value) * 100.0;
                //ProgressCircle.Progress = 100 - (float)percentage;
            }
            catch (Exception ex)
            {

            }

        }

        async void OnTimerDone()
        {
            try
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout == null)
                    return;
                if (workout.IsEditing)
                    BtnFinishSet.Text = "Save";
                else if (workout.IsLastSet && !workout.IsEditing && workout.IsNext && !workout.IsFinished)
                {
                    BtnFinishSet.Text = workout.IsFirstSide ? "Save set & side 1" : "Save set & exercise";
                    btnAddSet.IsVisible = false;
                    UnFinishedExercises.Text = "Save set & do 1 more";
                    //TO hide space of btnaddset
                    
                    
                    //FinishExercise.IsVisible = false;
                }
                else
                {
                    BtnFinishSet.Text = "Save set";
                    UnFinishedExercises.Text = workout.IsFirstSide ? "Finish side 1" : "Finish exercise";
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void AddSet_Clicked(object sender, EventArgs e)
        {

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {

                workout.IsFinished = true;
                workout.IsNext = false;
                workout.IsLastSet = false;
                workout.IsFirstWorkSet = false;
                Xamarin.Forms.MessagingCenter.Send<AddSetMessage>(new AddSetMessage() { model = workout }, "AddSetMessage");

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        this.ForceUpdateSize();

                });
            }

        }

        void btnAddSet_Clicked(System.Object sender, System.EventArgs e)
        {
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {

                workout.IsLastSet = false;
                workout.IsFirstWorkSet = false;
                
                Xamarin.Forms.MessagingCenter.Send<AddSetMessage>(new AddSetMessage() { model = workout, hasFinished = true }, "AddSetMessage");

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                        this.ForceUpdateSize();
                });
            }
        }
        string TimerEntry = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;


        private async void SaveSet_Clicked(object sender, EventArgs e)
        {
            //new Thread(async () =>
            //{
            //    Device.BeginInvokeOnMainThread(async () =>
            //    {
            try
            {
                if (CheckWelcomeTooltip())
                    return;
                if (App.IsSaveSetClicked)
                    return;
                try
                {
                    DependencyService.Get<IKeyboardHelper>().HideKeyboard();
                }
                catch (Exception ex)
                {

                }
                LocalDBManager.Instance.SetDBSetting($"AnySets{DateTime.Now.Date}", "1");
                LblSuperSet.Effects.Clear();
                if (App.IsSupersetPopup)
                    LblBackoffset.Effects.Clear();

                App.IsSaveSetClicked = true;
                //BtnSaveSet.Clicked -= SaveSet_Clicked;
                WeightEntry.Unfocus();
                ShouldAnimate = false;
                string saveandFinish = BtnFinishSet.Text;
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    workout.ShowSuperSet3 = false;
                    workout.ShowSuperSet2 = false;
                    //if (workout.IsTimeBased && workout.IsHeaderCell)
                    if (workout.IsTimeBased && workout.IsHeaderCell && workout.ShowWorkTimer)
                    {
                        BtnFinishSet.Text = "Save set";
                        workout.ShowWorkTimer = false;
                        if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                        {
                            await Task.Delay(100);

                            var popup = new TimerPopup(false);
                            LocalDBManager.Instance.SetDBSetting("timer_remaining", workout.Reps.ToString());
                            popup.popupTitle = "Work";
                            popup?.SetTimerRepsSets("");
                            popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
                            if (Timer.Instance.State == "RUNNING")
                            {
                                Timer.Instance.Remaining = 0;
                                Timer.Instance.StopTimer();
                            }
                            Timer.Instance.Remaining = int.Parse(popup.RemainingSeconds);
                            PopupNavigation.Instance.PushAsync(popup);
                            
                            if (Timer.Instance.State == "STOPPED" && Timer.Instance.stopRequest == true)
                            {
                                //await Timer.Instance.StopTimer();
                                Timer.Instance.stopRequest = false;
                                //await Task.Delay(1000);
                            }
                            Timer.Instance.StartTimer();
                            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { SetModel = workout, WatchMessageType = WatchMessageType.StartTimer, Seconds = int.Parse(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value) - 1 }, "SendWatchMessage");
                            App.IsSaveSetClicked = false;
                            return;
                        }

                    }
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                    if (Device.RuntimePlatform.Equals(Device.Android) && !workout.IsBodyweight)
                    {
                        workout.ShouldUpdateIncrement = true;
                        var doubleWEIGHT = workout.WeightDouble;
                        workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb, false);
                        if (isKg && workout.Weight.Kg < 1)
                            workout.Weight = new MultiUnityWeight(1, "kg");
                        else if (!isKg && workout.Weight.Lb < 1)
                            workout.Weight = new MultiUnityWeight(1, "lb");
                        if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                            if (!workout.IsUnilateral || (workout.IsUnilateral && workout.IsFirstSide))
                                if (workout.WeightDouble != doubleWEIGHT || workout.Reps != workout.PreviousReps)
                                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                    }
                    if (workout.Reps <= 0)
                        workout.Reps = 1;
                    if (isKg && workout.Weight.Kg < 1)
                        workout.Weight = new MultiUnityWeight(1, "kg");
                    else if (!isKg && workout.Weight.Lb < 1)
                        workout.Weight = new MultiUnityWeight(1, "lb");

                    if (!workout.IsEditing && BtnFinishSet.Text == "Save set & exercise")
                    {
                        if (!CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsReversePyramid || (CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsReversePyramid && !workout.IsFirstWorkSet))
                        {
                            App.IsSaveSetClicked = false;
                            FinishedExercise_Clicked(sender, e);

                            return;
                        }

                    }
                    else if (!workout.IsEditing && workout.IsLastSet && BtnFinishSet.Text != "Save set & exercise")
                    {
                        //FinishExercise.IsVisible = true;
                    }

                    if (workout.IsEditing)
                    {
                        workout.IsNext = false;
                        workout.IsEditing = false;
                        workout.IsFinished = true;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (Device.RuntimePlatform.Equals(Device.iOS))
                                this.ForceUpdateSize();
                        });
                        Xamarin.Forms.MessagingCenter.Send<CellUpdateMessage>(new CellUpdateMessage() { model = workout }, "CellUpdateMessage");
                        //BtnSaveSet.Clicked += SaveSet_Clicked;
                        App.IsSaveSetClicked = false;
                        if (!workout.IsExerciseFinished && workout.IsFirstWorkSet)
                            Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = false }, "SaveSetMessage");
                        return;
                    }
                    try
                    {
                        if (Timer.Instance.State == "RUNNING")
                        {
                            Timer.Instance.Remaining = 0;
                            Timer.Instance.StopTimer();
                        }
                        workout.IsFinished = true;
                        workout.IsNext = false;
                        BtnFinishSet.Text = "Save set";
                       
                        Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = false }, "SaveSetMessage");

                        try
                        {
                            if (Device.RuntimePlatform.Equals(Device.iOS))
                                this.ForceUpdateSize();
                        }
                        catch (Exception ex)
                        {

                        }
                        BtnFinishSet.Text = "Save set";
                        //if (!workout.IsLastSet)
                        //{
                        if (Timer.Instance.State == "RUNNING")
                        {
                            await Timer.Instance.StopTimer();
                            //await Task.Delay(1000);
                        }

                        if (workout.IsWarmups)
                        {
                            if (LocalDBManager.Instance.GetDBSetting("timer_autoset").Value == "true")
                            {
                                var time = workout.IsLastWarmupSet ? "55" : CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsBodyweight ? "30" : "40";
                                if (workout.Id == 16508)
                                    time = "120";
                                if (workout.IsFlexibility)
                                    time = "30";
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", time);
                            }
                            else
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", App.globalTime.ToString());
                        }
                        else
                        {
                            if (LocalDBManager.Instance.GetDBSetting("timer_autoset").Value == "true")
                            {
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", CurrentLog.Instance.GetRecommendationRestTime(workout.Id, false, workout.IsNormalset, CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsPyramid, workout.IsFlexibility, CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsReversePyramid, workout.IsMaxChallenge,workout.Reps).ToString());
                                if (workout.IsNextBackOffSet)
                                {
                                    LocalDBManager.Instance.SetDBSetting("timer_remaining", "50");
                                    //Back-off popup
                                    if (Config.ShowBackoffPopup == false)
                                    {
                                        if (App.IsShowBackOffPopup)
                                        {
                                            App.IsSaveSetClicked = false;
                                            return;
                                        }
                                        App.IsShowBackOffPopup = true;
                                        AlertConfig ShowWelcomePopUp2 = new AlertConfig()
                                        {
                                            Message = "Do more reps with less weight on your last set. Scientists found it gets you in shape faster.",
                                            Title = "Back-off set",
                                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                            OkText = "Try it out",
                                        };
                                        await Task.Delay(100);
                                        Config.ShowBackoffPopup = true;
                                        UserDialogs.Instance.Alert(ShowWelcomePopUp2);
                                    }

                                }
                            }
                            else
                            {
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", App.globalTime.ToString());
                            }

                            if (workout.BodypartId == 12 && workout.Id != 16508 && !workout.IsWarmups)
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", workout.Reps.ToString());
                        }
                        if (Timer.Instance.State == "STOPPED" && Timer.Instance.stopRequest == true)
                        {
                            //await Timer.Instance.StopTimer();
                            Timer.Instance.stopRequest = false;
                            //await Task.Delay(1000);
                        }

                        Timer.Instance.StartTimer();
                        if (workout.IsLastWarmupSet && workout.IsJustSetup)
                        {
                            //Show workset popup

                            AlertConfig challengeConfig = new AlertConfig()
                            {
                                Title = workout.IsBodyweight ? "Test your starting reps" : "Test your starting weight",
                                Message = $"Repeat as many times as you can with good form. Stop when it gets hard. Enter how many you did.",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Got it",
                            };
                            if ((workout.IsUnilateral && workout.IsFirstSide) || !workout.IsUnilateral)
                            await UserDialogs.Instance.AlertAsync(challengeConfig);

                        }
                        if (!workout.IsLastSet)
                            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { SetModel = workout, WatchMessageType = WatchMessageType.StartTimer, Seconds = int.Parse(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value) - 1 }, "SendWatchMessage");
                        if (!workout.IsWarmups && workout.IsFirstWorkSet && (workout.IsFirstSide && workout.IsUnilateral || !workout.IsUnilateral ) )
                        {
                            try
                            {

                            if (workout.IsMaxChallenge)
                            {
                                if (CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsEasy)
                                    workout.RIR = 3;
                                else if (CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsMedium)
                                    workout.RIR = 2;
                                else
                                    workout.RIR = 1;
                                return;
                            }


                            }
                            catch (Exception ex)
                            {

                            }
                            if (App.IsConnectedToWatch)
                            {
                                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { SetModel = workout, WatchMessageType = WatchMessageType.RIR }, "SendWatchMessage");
                                return;
                            }
                            if (Config.ShowExplainRIRPopUp == false)
                            {
                                if (Device.RuntimePlatform.Equals(Device.Android))
                                {
                                    ConfirmConfig ShowExplainRIRPopUp = new ConfirmConfig()
                                    {
                                        Message = AppResources.NowPleaseTellMeHowHardThatWas,
                                        Title = string.Format("{0} {1}!", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname")?.Value),
                                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                        OkText = AppResources.Ok,
                                        CancelText = AppResources.RemindMe,
                                        OnAction = async (bool ok) =>
                                        {
                                            if (ok)
                                            {
                                                Config.ShowExplainRIRPopUp = true;
                                                //LocalDBManager.Instance.SetDBSetting("ShowExplainRIRPopUp", "false");
                                                AskRIR();
                                            }
                                            else
                                            {
                                                Config.ShowExplainRIRPopUp = false;
                                                //LocalDBManager.Instance.SetDBSetting("ShowExplainRIRPopUp", "true");
                                                AskRIR();
                                            }
                                        }
                                    };
                                    UserDialogs.Instance.Confirm(ShowExplainRIRPopUp);
                                }
                                else
                                {
                                    bool IsAsk = false;
                                    if (Device.RuntimePlatform.Equals(Device.iOS))
                                        IsAsk = await App.Current.MainPage.DisplayAlert(string.Format("{0} {1}!", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname")?.Value), AppResources.NowPleaseTellMeHowHardThatWas, AppResources.Ok, AppResources.RemindMe);
                                    else
                                        IsAsk = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                                        {
                                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                            Message = AppResources.NowPleaseTellMeHowHardThatWas,
                                            Title = string.Format("{0} {1}!", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname")?.Value),
                                            OkText = AppResources.Ok,
                                            CancelText = AppResources.RemindMe
                                        });
                                    if (IsAsk)
                                    {
                                        Config.ShowExplainRIRPopUp = true;
                                        //LocalDBManager.Instance.SetDBSetting("ShowExplainRIRPopUp", "false");
                                        AskRIR();
                                    }
                                    else
                                    {
                                        Config.ShowExplainRIRPopUp = false;
                                        //LocalDBManager.Instance.SetDBSetting("ShowExplainRIRPopUp", "true");
                                        AskRIR();
                                    }
                                }
                            }
                            else
                            {
                                AskRIR();
                            }

                            async void AskRIR()
                            {
                                var isTimeBased = CurrentLog.Instance.ExerciseLog.Exercice.IsTimeBased;

                                string DoneMore = null;
                                if (Device.RuntimePlatform.Equals(Device.iOS))
                                {
                                    while (DoneMore == null)
                                    {
                                        int RIR;
                                        DoneMore = await App.Current.MainPage.DisplayActionSheet(null, null, null,
                                            AppResources.ThatWasVeryVeryHard,
                                            isTimeBased ? "I could have done 1-2 more secs" : AppResources.ICouldHaveDone12MoreRep,
                                            isTimeBased ? "I could have done 3-4 more secs" : AppResources.ICouldHaveDone34MoreReps,
                                            isTimeBased ? "I could have done 5-6 more secs" : AppResources.IcouldHaveDone56MoreReps,
                                            isTimeBased ? "I could have done 7+ more secs" : AppResources.ICouldHaveDone7PMoreReps);
                                        CurrentLog.Instance.LastSetWas = DoneMore;
                                        if (DoneMore == AppResources.ThatWasVeryVeryHard)
                                        {
                                            //Debug.WriteLine(DoneMore);
                                            workout.RIR = 0;

                                            ProcessRIR(DoneMore);
                                        }
                                        else if (DoneMore == AppResources.ICouldHaveDone12MoreRep || DoneMore == "I could have done 1-2 more secs")
                                        {
                                            //Debug.WriteLine(DoneMore);
                                            workout.RIR = 1;

                                            ProcessRIR(DoneMore);
                                        }
                                        else if (DoneMore == AppResources.ICouldHaveDone34MoreReps || DoneMore == "I could have done 3-4 more secs")
                                        {
                                            //Debug.WriteLine(DoneMore);
                                            workout.RIR = 2;

                                            ProcessRIR(DoneMore);
                                        }
                                        else if (DoneMore == AppResources.IcouldHaveDone56MoreReps || DoneMore == "I could have done 5-6 more secs")
                                        {
                                            //Debug.WriteLine(DoneMore);
                                            workout.RIR = 3;
                                            ProcessRIR(DoneMore);
                                        }
                                        else if (DoneMore == AppResources.ICouldHaveDone7PMoreReps || DoneMore == "I could have done 7+ more secs")
                                        {
                                            //Debug.WriteLine(DoneMore);
                                            workout.RIR = 4;
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
                                        workout.RIR = 0;

                                        ProcessRIR(AppResources.ThatWasVeryVeryHard);
                                    });
                                    actionSheetConfig.Add(isTimeBased ? "I could have done 1-2 more secs" : AppResources.ICouldHaveDone12MoreRep, () =>
                                    {
                                        //Debug.WriteLine(DoneMore);
                                        workout.RIR = 1;
                                        ProcessRIR(isTimeBased ? "I could have done 1-2 more secs" : AppResources.ICouldHaveDone12MoreRep);
                                    });
                                    actionSheetConfig.Add(isTimeBased ? "I could have done 3-4 more secs" : AppResources.ICouldHaveDone34MoreReps, () =>
                                    {
                                        //Debug.WriteLine(DoneMore);
                                        workout.RIR = 2;
                                        ProcessRIR(isTimeBased ? "I could have done 3-4 more secs" : AppResources.ICouldHaveDone34MoreReps);
                                    });
                                    actionSheetConfig.Add(isTimeBased ? "I could have done 5-6 more secs" : AppResources.IcouldHaveDone56MoreReps, () =>
                                    {
                                        //Debug.WriteLine(DoneMore);
                                        workout.RIR = 3;
                                        ProcessRIR(isTimeBased ? "I could have done 5-6 more secs" : AppResources.IcouldHaveDone56MoreReps);
                                    });
                                    actionSheetConfig.Add(isTimeBased ? "I could have done 7+ more secs" : AppResources.ICouldHaveDone7PMoreReps, () =>
                                    {
                                        //Debug.WriteLine(DoneMore);
                                        workout.RIR = 4;
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

                                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value != "true" && App.IsHowHardAsked == false && DoneMore.Contains(AppResources.ThatWasVeryVeryHard) && !workout.IsMaxChallenge)
                                {
                                    //ConfirmConfig ShowsharePopUp = new ConfirmConfig()
                                    //{
                                    //    Message = $"Do only 2 work sets on remaining exercises to recover.",
                                    //    Title = "Tired today?",
                                    //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    //    OkText = "2 work sets",
                                    //    CancelText = AppResources.Cancel,
                                    //};
                                    //var isShare = await UserDialogs.Instance.ConfirmAsync(ShowsharePopUp);
                                    //if (isShare)
                                    //{

                                    //    LocalDBManager.Instance.SetDBSetting("OlderQuickMode", LocalDBManager.Instance.GetDBSetting("QuickMode").Value);
                                    //    LocalDBManager.Instance.SetDBSetting("QuickMode", "true");
                                    //    try
                                    //    {
                                    //        LocalDBManager.Instance.ResetReco();
                                    //        Xamarin.Forms.MessagingCenter.Send<QuickTimeMessage>(new QuickTimeMessage() { model = workout }, "QuickTimeMessage");
                                    //    }
                                    //    catch (Exception ex)
                                    //    {

                                    //    }
                                    //}
                                    //App.IsHowHardAsked = true;
                                }
                                Xamarin.Forms.MessagingCenter.Send<UpdateSetTitleMessage>(new UpdateSetTitleMessage() { model = workout }, "UpdateSetTitleMessage");
                                CurrentLog.Instance.LastSetWas = DoneMore;

                                if (Config.ShowRIRPopUp == false)
                                {
                                    ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                                    {
                                        Title = AppResources.GotItExclamation,
                                        Message = string.Format("{0} \"{1}\". {2}", AppResources.YouSaid, DoneMore, AppResources.IWillAdjustAccordingly),
                                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                        OkText = AppResources.GotIt,
                                        CancelText = AppResources.RemindMe,
                                        OnAction = async (bool ok) =>
                                        {
                                            if (ok)
                                            {
                                                Config.ShowRIRPopUp = true;
                                            }
                                            else
                                            {
                                                Config.ShowRIRPopUp = false;
                                            }
                                        }
                                    };
                                    UserDialogs.Instance.Confirm(ShowRIRPopUp);
                                    return;
                                }
                                else
                                {
                                    //Ask for finish exercise:
                                    //  
                                    if (!workout.IsEditing && saveandFinish == "Save set & exercise")
                                    {
                                        App.IsSaveSetClicked = false;
                                        FinishedExercise_Clicked(sender, e);
                                    }
                                    else
                                        Xamarin.Forms.MessagingCenter.Send<FinishExerciseMessage>(new FinishExerciseMessage() { model = workout }, "FinishExerciseMessage");
                                }
                            }

                        }

                        //}
                    }
                    catch
                    {

                    }
                    finally
                    {
                        await Task.Delay(2000);
                        App.IsSaveSetClicked = false;
                    }
                    //BtnSaveSet.Clicked += SaveSet_Clicked;

                }

                LblCoachTips.Effects.Clear();
                StackReps.Effects.Clear();
                BtnSaveSet.Effects.Clear();
                if (Config.ShowBackoffPopup)
                {
                    BtnFinishSet.Effects.Clear();
                    //StackRepsConatiner.Effects.Clear();
                    StackWeight.Effects.Clear();
                }

            }
            finally
            {

            }
            //    });
            //}).Start();
        }

        private async void FinishedExercise_Clicked(object sender, EventArgs e)
        {

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {

                workout.IsFinished = true;
                workout.IsNext = false;
                ShouldFinishAnimate = false;
                ShouldAnimate = false;
                Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = true }, "SaveSetMessage");
            }

        }

        private async void DeleteSet_Clicked(object sender, EventArgs e)
        {


            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {
                BtnFinishSet.Text = "Save set";
                Xamarin.Forms.MessagingCenter.Send<DeleteSetMessage>(new DeleteSetMessage() { model = workout }, "DeleteSetMessage");

            }
        }

        private async void UnFinishedExercise_Clicked(object sender, EventArgs e)
        {


            if (UnFinishedExercises.Text == "Finish exercise")
                UnFinishedExercise_Clicked1(sender, e);
            else if (UnFinishedExercises.Text == "Finish side 1")
                SaveSet_Clicked(BtnSaveSet, e);
            else
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    workout.IsFinished = true;
                    workout.IsNext = false;
                    workout.IsLastSet = false;

                    Xamarin.Forms.MessagingCenter.Send<AddSetMessage>(new AddSetMessage() { model = workout }, "AddSetMessage");

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                            this.ForceUpdateSize();

                    });
                }
            }
            //WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            //if (workout != null)
            //{
            //    ShouldFinishAnimate = false;
            //    ShouldAnimate = false;

            //    Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = true }, "SaveSetMessage");
            //}
        }
        private async void UnFinishedExercise_Clicked1(object sender, EventArgs e)
        {

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {
                //workout.IsFirstSide = false;
                ShouldFinishAnimate = false;
                ShouldAnimate = false;

                Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = true }, "SaveSetMessage");
            }
        }
        private async void SkipExercise_Clicked(object sender, EventArgs e)
        {
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {
                ShouldFinishAnimate = false;
                ShouldAnimate = false;

                Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = true }, "SaveSetMessage");
            }
        }
        private async void RepsMore_Clicked(object sender, EventArgs e)
        {
            if (CheckWelcomeTooltip())
                return;
            RepsEntry.TextChanged -= RepsEntry_TextChanged;
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;

            if (workout != null && workout.IsNext)
            {
                currentReps += 1;
                //RepsEntry.Text = string.Format("{0}", currentReps);
                //LblReps.Text = string.Format("{0}", currentReps);
                Device.BeginInvokeOnMainThread(() =>
                {
                    workout.Reps = currentReps;
                });
                workout.Reps = currentReps;
                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout }, "SendWatchMessage");
                if (!workout.IsBackOffSet && !workout.IsWarmups ) //&& !workout.IsFinished && !workout.IsEditing
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                else if (!workout.IsExerciseFinished && workout.IsFirstWorkSet && workout.IsFinished && workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");

            }

            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                workout.ShouldUpdateIncrement = true;
                workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb, false);
                if (isKg && workout.Weight.Kg < 1)
                    workout.Weight = new MultiUnityWeight(1, "kg");
                else if (!isKg && workout.Weight.Lb < 1)
                    workout.Weight = new MultiUnityWeight(1, "lb");
                App.PCWeight = currentWeight;

            }
            RepsEntry.TextChanged += RepsEntry_TextChanged;
        }

        private void RepsLess_Clicked(object sender, EventArgs e)
        {
            if (CheckWelcomeTooltip())
                return;
            if (currentReps <= 1)
                return;
            RepsEntry.TextChanged -= RepsEntry_TextChanged;
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;

            if (workout != null && workout.IsNext)
            {

                currentReps -= 1;
                //RepsEntry.Text = string.Format("{0}", currentReps);
                //LblReps.Text = string.Format("{0}", currentReps);
                workout.Reps = currentReps;

                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout }, "SendWatchMessage");
                if (!workout.IsBackOffSet && !workout.IsWarmups ) //&& !workout.IsFinished && !workout.IsEditing
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                else if (!workout.IsExerciseFinished && workout.IsFirstWorkSet && workout.IsFinished && workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
            }
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                workout.ShouldUpdateIncrement = true;
                workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb, false);
                if (isKg && workout.Weight.Kg < 1)
                    workout.Weight = new MultiUnityWeight(1, "kg");
                else if (!isKg && workout.Weight.Lb < 1)
                    workout.Weight = new MultiUnityWeight(1, "lb");
                App.PCWeight = currentWeight;

            }
            RepsEntry.TextChanged += RepsEntry_TextChanged;
        }


        private async void WeightLess_Clicked(object sender, EventArgs e)
        {
            if (CheckWelcomeTooltip())
                return;
            if (currentWeight - weightStep <= 0)
                return;

            WeightEntry.TextChanged -= WeightEntry_TextChanged;

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;

            if (workout != null && workout.IsNext && !workout.IsBodyweight)
            {
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

                currentWeight -= weightStep;
                if (workout.Min != null && currentWeight < (isKg ? workout.Min.Kg : workout.Min.Lb))
                {
                    currentWeight = Math.Round((isKg ? workout.Min.Kg : workout.Min.Lb), 2);
                    return;
                }
                //LblWeight.Text = WeightEntry.Text = string.Format("{0:0.0}", currentWeight).ReplaceWithDot();
                App.PCWeight = currentWeight;
                var unit = isKg ? WeightUnities.kg : WeightUnities.lb;
                workout.ShouldUpdateIncrement = false;
                workout.Weight = new MultiUnityWeight(currentWeight, unit, false);
                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout }, "SendWatchMessage");
                if (!workout.IsBackOffSet && !workout.IsWarmups ) //&& !workout.IsFinished && !workout.IsEditing
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                else if (!workout.IsExerciseFinished && workout.IsFirstWorkSet && workout.IsFinished && workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
            }
            WeightEntry.TextChanged += WeightEntry_TextChanged;
        }

        private async void WeightMore_Clicked(object sender, EventArgs e)
        {
            if (CheckWelcomeTooltip())
                return;
            WeightEntry.TextChanged -= WeightEntry_TextChanged;



            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null && workout.IsNext && !workout.IsBodyweight)
            {
                currentWeight += weightStep;
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                App.PCWeight = currentWeight;
                //LblWeight.Text = WeightEntry.Text = string.Format("{0:0.0}", currentWeight).ReplaceWithDot();
                var unit = isKg ? WeightUnities.kg : WeightUnities.lb;
                workout.ShouldUpdateIncrement = false;
                workout.Weight = new MultiUnityWeight(currentWeight, unit, false);
                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout }, "SendWatchMessage");
                if (!workout.IsBackOffSet && !workout.IsWarmups )//&& !workout.IsFinished && !workout.IsEditing
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                else if (!workout.IsExerciseFinished && workout.IsFirstWorkSet && workout.IsFinished && workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
            }
            WeightEntry.TextChanged += WeightEntry_TextChanged;
        }


        void WeightEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                //if (!WeightEntry.IsFocused && Device.RuntimePlatform.Equals(Device.iOS))
                //    return;
                //if (Device.RuntimePlatform.Equals(Device.Android))
                //{
                //    var keyboardService = Xamarin.Forms.DependencyService.Get<Dependencies.IKeyboardService>();
                //    if (!keyboardService.isCurrentlyShowing())
                //        return;
                //}
                string txt = WeightEntry.Text == null ? "" : WeightEntry.Text;
                if (string.IsNullOrEmpty(txt))//|| txt.EndsWith(",") || txt.EndsWith(".")
                {

                }
                else
                {
                    if (!string.IsNullOrEmpty(WeightEntry.Text.Trim()))
                    {
                        string entryText = WeightEntry.Text.Replace(",", ".");
                        entryText = entryText.Replace(" ", "");
                        currentWeight = Convert.ToDecimal(entryText, CultureInfo.InvariantCulture);
                        WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                        if (workout.IsNext || workout.IsEditing)
                        {
                            //if (Device.RuntimePlatform.Equals(Device.Android) && !WeightEntry.IsFocused)
                            //    return;
                            try
                            {
                                if (( Device.RuntimePlatform.Equals(Device.iOS)) && workout.Id != CurrentLog.Instance.ExerciseLog.Exercice.Id)
                                    return;
                                if (Device.RuntimePlatform.Equals(Device.Android))
                                {
                                    var keyboardService = Xamarin.Forms.DependencyService.Get<Dependencies.IKeyboardService>();
                                    if (!keyboardService.isCurrentlyShowing() && workout.Id != CurrentLog.Instance.ExerciseLog.Exercice.Id)
                                        return;
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            
                            App.PCWeight = currentWeight;
                            if (workout.IsFirstWorkSet && WeightEntry.IsFocused)
                                Xamarin.Forms.MessagingCenter.Send<OneRMChangedMessage>(new OneRMChangedMessage() { model = workout, Weight = currentWeight, Reps = workout.Reps }, "OneRMChangedMessage"); ;
                        }
                        //if (Device.RuntimePlatform.Equals(Device.Android))
                        //{
                        //    WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                        //    if (workout != null)
                        //    {
                        //        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

                        //        workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb);
                        //        if (!workout.IsFirstWorkSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                        //            Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                        //    }

                        //}
                        //WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                        //{
                        //    if (workout != null)
                        //    {
                        //        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

                        //        //WeightEntry.Text = string.Format("{0}", currentWeight).ReplaceWithDot();
                        //        LblWeight.Text = string.Format("{0:0.0}", currentWeight).ReplaceWithDot();
                        //       // workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb);

                        //    }
                        //}

                    }
                    else
                        currentWeight = 0;
                }
            }
            catch (Exception ex)
            {
                //currentWeight = 0;
                // WeightEntry.Text = WeightEntry.Text = string.Format("{0:0.00}", currentWeight).ReplaceWithDot();
            }
        }

        //Edit Sets

        public void OnEdit(object sender, EventArgs e)
        {
            var mi = ((Button)sender);
            WorkoutLogSerieModelRef m = (WorkoutLogSerieModelRef)mi.CommandParameter;
            OnCancelClicked(sender, e);
            //Edit workout log
            var massUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "lbs" : "kg";
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = $"{AppResources.Edit} {m.ExerciseName}",
                Message = $"{AppResources.EnterWeights} {AppResources._in} {massUnit}",
                Text = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? Math.Round(m.Weight.Lb, 2).ToString().ReplaceWithDot() : Math.Round(m.Weight.Kg, 2).ToString().ReplaceWithDot(),
                OkText = AppResources.Edit,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                    {
                        return;
                    }
                    var weightText = weightResponse.Value.Replace(",", ".");
                    decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);
                    currentWeight = weight1;
                    m.Weight = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value, false);
                   // LblWeight.Text = string.Format("{0:0.0}", weight1);
                    AskForEditReps(m);

                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
            //}
        }

        async void AskForEditReps(WorkoutLogSerieModelRef m)
        {
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = InputType.Number,
                IsCancellable = true,
                Title = string.Format("{0} {1}", AppResources.Edit, m.ExerciseName),
                Message = AppResources.EnterNewReps,
                Placeholder = AppResources.TapToEnterHowMany,
                Text = m.Reps.ToString(),
                OkText = AppResources.Edit,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                    {
                        return;
                    }
                    try
                    {
                        int reps = Convert.ToInt32(weightResponse.Value, CultureInfo.InvariantCulture);
                        currentReps = reps;
                        m.Reps = reps;
                        //LblReps.Text = $"{reps}";
                        //Device.BeginInvokeOnMainThread(() =>
                        //{
                        //    foreach (WorkoutLogSerieModelRef wlsme in workoutLogSerieModel)
                        //        wlsme.OnPropertyChanged("SetLabel");
                        //});
                    }
                    catch (Exception ex)
                    {

                    }

                }
            };
            firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        public void OnDelete(object sender, EventArgs e)
        {
            //var mi = ((Button)sender);
            //WorkoutLogSerieModelRef m = (WorkoutLogSerieModelRef)mi.CommandParameter;
            //workoutLogSerieModel.RemoveAt(workoutLogSerieModel.IndexOf(m));
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    foreach (WorkoutLogSerieModelRef wlsme in workoutLogSerieModel)
            //        wlsme.OnPropertyChanged("SetLabel");
            //});

            //UpdateTopLabels();

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {
                workout.IsFinished = false;
                workout.IsNext = false;
                if (Timer.Instance.State != "RUNNING")
                    Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = false }, "SaveSetMessage");
            }
            OnCancelClicked(sender, e);
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

        void RepsEntry_Unfocused(object sender, TextChangedEventArgs e)
        {
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NewSet, SetModel = workout }, "SendWatchMessage");
                    if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                        Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                }
            }
        }
        void RepsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!RepsEntry.IsFocused && Device.RuntimePlatform.Equals(Device.iOS))
                    return;
                if (Device.RuntimePlatform.Equals(Device.Android))
                {
                    var keyboardService = Xamarin.Forms.DependencyService.Get<Dependencies.IKeyboardService>();
                    if (!keyboardService.isCurrentlyShowing())
                        return;
                }
                string txt = RepsEntry.Text == null ? "" : RepsEntry.Text;
                if (txt.EndsWith(",") || txt.EndsWith(".") || string.IsNullOrEmpty(RepsEntry.Text))
                    return;

                currentReps = Convert.ToInt32(RepsEntry.Text.Replace(",", "").Replace(".", ""));

                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    //RepsEntry.Text = string.Format("{0}", currentReps);
                    //LblReps.Text = string.Format("{0}", currentReps);
                    workout.Reps = currentReps;
                    if (workout.IsFirstWorkSet )
                    {
                        Xamarin.Forms.MessagingCenter.Send<OneRMChangedMessage>(new OneRMChangedMessage() { model = workout, Weight = App.PCWeight, Reps = currentReps }, "OneRMChangedMessage"); ;
                        Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                    }
                    else if(!workout.IsWarmups && !workout.IsFinished)
                    {
                        if (Device.RuntimePlatform.Equals(Device.Android))
                        {
                            var keyboardService = Xamarin.Forms.DependencyService.Get<Dependencies.IKeyboardService>();
                            if (keyboardService.isCurrentlyShowing()  && !workout.IsLastSet )
                                Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                        } else if (RepsEntry.IsFocused && !workout.IsLastSet)
                            Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                    }
                }


            }
            catch (Exception ex)
            {
                RepsEntry.Text = $"{currentReps}";
            }
        }

        void OnCancelClicked(object sender, System.EventArgs e)
        {
            StackLayout s = ((StackLayout)((Button)sender).Parent);
            s.Children[0].IsVisible = false;
            s.Children[1].IsVisible = false;
            s.Children[2].IsVisible = false;
            s.Children[3].IsVisible = true;
        }

        void OnContextMenuClicked(object sender, System.EventArgs e)
        {
            StackLayout s = ((StackLayout)((Button)sender).Parent);
            s.Children[0].IsVisible = true;
            s.Children[1].IsVisible = true;
            s.Children[2].IsVisible = true;
            s.Children[3].IsVisible = false;
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            CheckWelcomeTooltip();
        }

        void TapGestureRecognizer_Tapped_1(System.Object sender, System.EventArgs e)
        {
            try
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                videoPlayer.Source = workout.VideoUrl;
            }
            catch (Exception ex)
            {

            }
        }

        async void DeleteSetTapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            ConfirmConfig supersetConfig = new ConfirmConfig()
            {
                Title = "Delete set? ",
                OkText = "Delete",
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                CancelText = AppResources.Cancel,
            };

            var x = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
            if (x)
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    Xamarin.Forms.MessagingCenter.Send<DeleteSetMessage>(new DeleteSetMessage() { model = workout, isPermenantDelete = true }, "DeleteSetMessage");
                }
            }

        }

        async void ShowAlert_Tapped(System.Object sender, System.EventArgs e)
        {
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;

            string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;

            string exId = $"{workout.Id}";
            var lastTime = LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle);
            RecommendationModel reco = null; 
            if (lastTime != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value))
                    {
                        var LastRecoPlus1Day = Convert.ToDateTime(LocalDBManager.Instance.GetDBReco("NbRepsGeneratedTime" + exId + setStyle).Value);
                        if (LastRecoPlus1Day > DateTime.Now)
                        {
                            reco = RecoContext.GetReco("Reco" + exId + setStyle);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception is:{ex.ToString()}");
                }
            }

            if (workout.IsMaxChallenge)
            {
                ConfirmConfig maxConfig = new ConfirmConfig()
                {
                    Title = $"Challenge",
                    Message = $"Do as many reps as you can with good form. Stop before your form breaks down.",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Continue",
                    CancelText = "Cancel challenge",
                };

                var isMaxChallenge = await UserDialogs.Instance.ConfirmAsync(maxConfig);
                if (!isMaxChallenge)
                {
                    //load normal
                    LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "challenge", $"");
                    Xamarin.Forms.MessagingCenter.Send<LoadNormalExercise>(new LoadNormalExercise() { exerciseId = workout.Id }, "LoadNormalExercise");
                }
                return;
            }
                
            if (reco == null)
                return;

            if (reco.IsDeload || workout.SetTitle.Contains("Deload"))
                {
                    var per = string.Format("{0:0.##}%", reco.OneRMPercentage * 100);
                    bool isConfirm = true;
                if (per.Equals("0%"))
                    per = "+0%";
                    ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                    {
                        Title = $"Deload",
                        Message = $"Strength {per} last time. Deloading to recover.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Continue",
                        CancelText = "Cancel deload",
                    };
                    var isDeload = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                    if (!isDeload)
                    {
                    //load normal
                    if (!reco.IsDeload)
                    {
                        //string setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                        //string exId = $"{m.Id}";
                        LocalDBManager.Instance.SetDBReco("RReps" + exId + setStyle + "Deload", $"");
                    }
                    Xamarin.Forms.MessagingCenter.Send<LoadNormalExercise>(new LoadNormalExercise() { exerciseId = workout.Id }, "LoadNormalExercise");
                }
                    return;
                }
            if (reco.IsLightSession)
            {
                var bodyPartname = AppThemeConstants.GetBodyPartName(workout.BodypartId);
                try
                {
                    bodyPartname = AppThemeConstants.GetBodyPartName(workout.BodypartId);
                }
                catch (Exception ex)
                {

                }
                
                ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                {
                    Title = "Light session",
                    Message = string.IsNullOrEmpty(bodyPartname) == false ? $"Last {bodyPartname.ToLower()} workout {reco.days} days ago. Light session enabled." : $"Last {workout.ExerciseName} workout {reco.days} days ago. Light session enabled.",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Continue",
                    CancelText = "Cancel light session",
                };
                var isLightSession = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                if (!isLightSession)
                {
                    //load normal
                    Xamarin.Forms.MessagingCenter.Send<LoadNormalExercise>(new LoadNormalExercise() { exerciseId = workout.Id, isReloadReco = true }, "LoadNormalExercise");
                }
                return;
            }

            if (reco.days >= 5 && reco.days <= 9)
            {
                var bodyPartname =  AppThemeConstants.GetBodyPartName(workout.BodypartId);

                ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                {
                    Title = $"{(string.IsNullOrEmpty(bodyPartname) == false ? bodyPartname.ToLower().FirstCharToUpper() : workout.ExerciseName)} fully recovered",
                    Message = $"Last trained: {reco.days} days ago. Added 1 set.",//string.Format("{0} {1} {2} {3} {4}", AppResources.TheLastTimeYouDid, string.IsNullOrEmpty(bodyPartname) == false ? bodyPartname.ToLower() : m.Label, AppResources.was, sessionDays, AppResources.DaysAgoYouShouldBeFullyRecoveredDoExtraSet),
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Continue",
                    CancelText = "Remove 1 set",
                };
                
                var isAddMoreSet = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                
                if (!isAddMoreSet)
                {
                    //load normal
                    Xamarin.Forms.MessagingCenter.Send<LoadNormalExercise>(new LoadNormalExercise() { exerciseId = workout.Id }, "LoadNormalExercise");
                }
                return;
            }

            var msg = "";
            if (reco.Reps < reco.MinReps)
                msg = workout.IsBodyweight ? $"Reps of {reco.Reps} are lower than your min of {reco.MinReps} to give you a smooth progression." : $"Your preferred reps are {reco.MinReps}-{reco.MaxReps}, but your weight increments are too large to progress in that range today.";
            else
                msg = workout.IsBodyweight ? $"Reps of {reco.Reps} are higher than your max of {reco.MaxReps} to give you a smooth progression." : $"Your preferred reps are {reco.MinReps}-{reco.MaxReps}, but your weight increments are too large to progress in that range today.";
            msg = workout.IsBodyweight ? msg : $"Your preferred reps are {reco.MinReps}-{reco.MaxReps}, but your weight increments are too large to progress in that range today.";
            //                                    msg = m.IsBodyweight ? $"Reps of {m.RecoModel.Reps} are higher than your max of {m.RecoModel.MaxReps} to give you a smooth progression." : $"Reps of {m.RecoModel.Reps} are higher than your max of {m.RecoModel.MaxReps} to match your {incr} {(isKg ? "kg" : "lbs")} increments.";
            ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
            {
                Title = workout.IsBodyweight ? "Reps outside range" : $"Reps outside range",
                Message = msg,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = "Got it",
                CancelText = "Edit equipment"
            };
            var result = await UserDialogs.Instance.ConfirmAsync(ShowRIRPopUp);
            if (!result)
            {
                PagesFactory.PushAsync<EquipmentSettingsPage>();
            
            }
            
        }
    }
    }
