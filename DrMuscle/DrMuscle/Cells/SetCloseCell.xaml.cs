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
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Views;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetCloseCell : ViewCell
    {
        private decimal currentWeight = 0;
        private int currentReps = 0;
        public static event Action ViewCellSizeChangedEvent;

        bool ShouldAnimate = false;
        bool ShouldFinishAnimate = false;
        bool DemoSecondSteps = false;

        private decimal weightStep = LocalDBManager.Instance.GetDBSetting("massunit") == null ? 2 : LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (decimal)2 : (decimal)5;
        public SetCloseCell()
        {
            InitializeComponent();

            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            Timer.Instance.OnTimerStop += OnTimerDone;
            //WeightEntry.Unfocused += WeightEntry_Unfocused;
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                RepsEntry.HeightRequest = 33;
                WeightEntry.HeightRequest = 33;
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
                // animate(BtnSaveSet);

            }
        }

        private async void SetFinishAnimation()
        {

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
                    
                    var isKg = false;
                    try
                    {
                        isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg" ? true : false;
                    }
                    catch (Exception ex)
                    {

                    }

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


                    //LblWeight.Text = isKg ?
                    //                                                    string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Kg, 1), 1)) :
                    //                                                    string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Lb, (decimal)2.5), 1));

                   
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
                //if (m.WatchMessageType == WatchMessageType.SaveSet)
                //    this.SaveSet_Clicked(BtnSaveSet, EventArgs.Empty);
                //else if (m.WatchMessageType == WatchMessageType.RepsLess)
                //    this.RepsLess_Clicked(new DrMuscleButton(), EventArgs.Empty);
                //else if (m.WatchMessageType == WatchMessageType.RepsMore)
                //    this.RepsMore_Clicked(new DrMuscleButton(), EventArgs.Empty);
                //else if (m.WatchMessageType == WatchMessageType.WeightLess)
                //    this.WeightLess_Clicked(new DrMuscleButton(), EventArgs.Empty);
                //else if (m.WatchMessageType == WatchMessageType.WeightMore)
                //    WeightMore_Clicked(new DrMuscleButton(), EventArgs.Empty);

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
                    //LblCoachTips.Effects.Clear();
                    //DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(StackReps, true);
                    App.WelcomeTooltop[1] = true;
                    return true;
                }

                if (App.WelcomeTooltop[2] == false)
                {
                    //StackReps.Effects.Clear();
                    //DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(BtnSaveSet, true);
                    App.WelcomeTooltop[2] = true;
                    return true;
                }
            }
            if (App.IsShowBackOffTooltip)
            {

                if (App.BackoffTooltop[1] == false)
                {
                    //StackRepsConatiner.Effects.Clear();
                    // DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(StackWeight, true);
                    App.BackoffTooltop[1] = true;
                    return true;
                }

                if (App.BackoffTooltop[2] == false)
                {
                    //StackWeight.Effects.Clear();
                    //DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(BtnFinishSet, true);
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
                        //BtnFinishSet.Text = $" Save set (rest left: {remaining.ToString()} s)";
                        //  UnFinishedExercises.Text = "Finish exercise";
                    }
                    //else
                    //  BtnFinishSet.Text =  $"Save set";
                }
                else if (workout.IsEditing)
                {
                    // BtnFinishSet.Text = $"Save";
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
                //if (workout.IsEditing)
                //    BtnFinishSet.Text = "Save";
                //    else if (workout.IsLastSet && !workout.IsEditing && workout.IsNext && !workout.IsFinished)
                //    {
                //        BtnFinishSet.Text = workout.IsFirstSide ? "Save set & side 1" : "Save set & exercise";
                //        UnFinishedExercises.Text = "Save set & do 1 more";
                //        FinishExercise.IsVisible = false;
                //    }
                //    else
                //    {
                //        BtnFinishSet.Text = "Save set";
                //        UnFinishedExercises.Text = "Finish exercise";
                //    }
            }
            catch (Exception ex)
            {

            }
        }

        private async void AddSet_Clicked(object sender, EventArgs e)
        {

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null && workout.IsFinished)
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
            else
            {

                workout.IsNext = false;

                Xamarin.Forms.MessagingCenter.Send<AddSetMessage>(new AddSetMessage() { model = workout }, "AddSetMessage");

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


                App.IsSaveSetClicked = true;
                //BtnSaveSet.Clicked -= SaveSet_Clicked;

                ShouldAnimate = false;
                //string saveandFinish = BtnFinishSet.Text;
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    workout.ShowSuperSet3 = false;
                    workout.ShowSuperSet2 = false;
                    //if (workout.IsTimeBased && workout.IsHeaderCell)
                    if (workout.IsTimeBased && workout.IsHeaderCell && workout.ShowWorkTimer)
                    {
                        //  BtnFinishSet.Text = "Save set";
                        workout.ShowWorkTimer = false;
                        if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                        {
                            await Task.Delay(100);

                            var popup = new TimerPopup(false);
                            LocalDBManager.Instance.SetDBSetting("timer_remaining", workout.Reps.ToString());
                            popup.popupTitle = "Work";
                            popup?.SetTimerRepsSets("");
                            popup.RemainingSeconds = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
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

                    //if (!workout.IsEditing && BtnFinishSet.Text == "Save set & exercise")
                    //{
                    //    if (!CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsReversePyramid || (CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsReversePyramid && !workout.IsFirstWorkSet))
                    //    {
                    //        App.IsSaveSetClicked = false;
                    //        FinishedExercise_Clicked(sender, e);

                    //        return;
                    //    }

                    //}
                    //else if (!workout.IsEditing && workout.IsLastSet && BtnFinishSet.Text != "Save set & exercise")
                    //{
                    //    FinishExercise.IsVisible = true;
                    //}





                    //BtnSaveSet.Clicked += SaveSet_Clicked;

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

                Xamarin.Forms.MessagingCenter.Send<DeleteSetMessage>(new DeleteSetMessage() { model = workout }, "DeleteSetMessage");

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
                    workout.Reps = currentReps;
                }
            }
            catch (Exception ex)
            {
                //RepsEntry.Text = $"{currentReps}";
            }
        }

        void WeightEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!WeightEntry.IsFocused && Device.RuntimePlatform.Equals(Device.iOS))
                    return;
                if (Device.RuntimePlatform.Equals(Device.Android))
                {
                    var keyboardService = Xamarin.Forms.DependencyService.Get<Dependencies.IKeyboardService>();
                    if (!keyboardService.isCurrentlyShowing())
                        return;
                }

                string txt = WeightEntry.Text == null ? "" : WeightEntry.Text;
                if (string.IsNullOrEmpty(txt))//|| txt.EndsWith(",") || txt.EndsWith(".")
                {

                }
                else
                {

                    if (!string.IsNullOrEmpty(WeightEntry.Text.Trim()) && !WeightEntry.Text.EndsWith(".") && !WeightEntry.Text.EndsWith(","))
                    {
                        string entryText = WeightEntry.Text.Replace(",", ".");
                        entryText = entryText.Replace(" ", "");
                        currentWeight = Convert.ToDecimal(entryText, CultureInfo.InvariantCulture);
                        WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;

                        if (workout != null  && !entryText.EndsWith("."))
                        {
                            var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                            workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb, false);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //currentWeight = 0;
                // WeightEntry.Text = WeightEntry.Text = string.Format("{0:0.00}", currentWeight).ReplaceWithDot();
            }
        }


        private async void UnFinishedExercise_Clicked(object sender, EventArgs e)
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
                    //LblWeight.Text = string.Format("{0:0.0}", weight1);
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
    }
}
