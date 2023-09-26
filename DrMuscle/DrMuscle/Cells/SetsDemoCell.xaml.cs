using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.Exercises;
using DrMuscle.Views;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetsDemoCell : ViewCell
    {
        private decimal currentWeight = 0;
        private int currentReps = 0;
        public static event Action ViewCellSizeChangedEvent;

        bool ShouldAnimate = false;
        bool ShouldFinishAnimate = false;
        bool DemoSecondSteps = false;
        private decimal weightStep = LocalDBManager.Instance.GetDBSetting("massunit") == null ? 1 : LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (decimal)1 : (decimal)2.5;
        public SetsDemoCell()
        {
            InitializeComponent();

            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            Timer.Instance.OnTimerStop += OnTimerDone;
            WeightEntry.Unfocused += WeightEntry_Unfocused;
            if (Device.RuntimePlatform.Equals(Device.iOS))
                this.ForceUpdateSize();
            if (App.IsDemoProgress)
            {

                if (CurrentLog.Instance.IsDemoRunningStep2)
                {
                    MessagingCenter.Subscribe<Message.UpdateAnimationMessage>(this, "UpdateAnimationMessage", (obj) =>
                    {
                        UpdateAnimation(obj.ShouldAnimate);
                    });
                    DemoSecondSteps = false;
                    ShouldAnimate = false;
                    ShouldFinishAnimate = true;
                    SetFinishAnimation();
                }
                //else
                //{
                //    ShouldFinishAnimate = true;
                //    ShouldAnimate = true;
                //    SetAnimation();
                //    SetFinishAnimation();
                //}

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
            if (App.IsDemoProgress)
                animateFinish(FinishExercise);

        }

        async void animate(View grid)
        {
            try
            {
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
            if (workout != null)
            {
                workout.OnSizeChanged += Item_OnSizeChanged;

                if (!string.IsNullOrEmpty(workout.HeaderTitle) && workout.IsHeaderCell)
                    HeaderDescStack.IsVisible = true;
                else
                    HeaderDescStack.IsVisible = false;

                var isKg = false;
                try
                {
                    isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                }
                catch (Exception ex)
                {

                }

                currentWeight = isKg ? workout.Weight.Kg : workout.Weight.Lb;
                currentReps = workout.Reps;
                //RepsEntry.Text = string.Format("{0}", currentReps);
                //LblReps.Text = string.Format("{0}", currentReps);

                //WeightEntry.Text = isKg ?
                //string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Kg, 1),2)) :
                //string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Lb, (decimal)2.5),2));
                WeightText.Text = workout.IsBodyweight ? "Bodyweight" : isKg ? "kg" : "lbs";
                WeightText.FontSize = workout.IsBodyweight ? 16 : 14;
                if (workout.Id == 16508)
                {
                    WeightText.Text = workout.IsWarmups ? "Brisk" : "Fast";
                    WeightText.FontSize = 26;
                }

                //LblWeight.Text = isKg ?
                //                                                    string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Kg, 1), 1)) :
                //                                                    string.Format("{0}", Math.Round(SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Lb, (decimal)2.5), 1));
                LblMassUnit.Text = workout.IsBodyweight ? "" : isKg ? "kg" : "lbs";
                if (workout.IsEditing)
                    BtnFinishSet.Text = "Save";
                else
                    BtnFinishSet.Text = "Save set";
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
                        weightStep = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (decimal)1 : (decimal)2.5;
                }
                catch (Exception ex)
                {

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

        public async void UpdateAnimation(bool IsAnimate)
        {
            DemoSecondSteps = IsAnimate;
            if (!IsAnimate)
            {
                ShouldFinishAnimate = true;
                ShouldAnimate = true;
                SetAnimation();
               // SetFinishAnimation();
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
                    if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                        Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                }
            }



        }

        void OnTimerChange(int remaining)
        {
            try
            {
                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;

                if (workout != null && !workout.IsEditing && workout.IsActive)
                {
                    if (Timer.Instance.State == "RUNNING")
                    {
                        BtnFinishSet.Text = $" Save set (rest left: {remaining.ToString()} s)";
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
                if (workout.IsEditing)
                    BtnFinishSet.Text = "Save";
                else
                    BtnFinishSet.Text = "Save set";
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
            //BtnSaveSet.Clicked -= SaveSet_Clicked;
            WeightEntry.Unfocus();
            ShouldAnimate = false;

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {
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
                        Timer.Instance.Remaining = int.Parse(popup.RemainingSeconds);
                        PopupNavigation.Instance.PushAsync(popup);
                        if (Timer.Instance.State == "STOPPED" && Timer.Instance.stopRequest == true)
                        {
                            //await Timer.Instance.StopTimer();
                            Timer.Instance.stopRequest = false;
                            //await Task.Delay(1000);
                        }
                        Timer.Instance.StartTimer();
                        return;
                    }

                }
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                if (Device.RuntimePlatform.Equals(Device.Android))
                {
                    workout.Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb, false);
                    if (isKg && workout.Weight.Kg < 1)
                        workout.Weight = new MultiUnityWeight(1, "kg");
                    else if (!isKg && workout.Weight.Lb < 1)
                        workout.Weight = new MultiUnityWeight(1, "lb");
                    if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                        if (!workout.IsUnilateral || (workout.IsUnilateral && workout.IsFirstSide))
                            Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                }
                if (workout.Reps <= 0)
                    workout.Reps = 1;
                if (isKg && workout.Weight.Kg < 1)
                    workout.Weight = new MultiUnityWeight(1, "kg");
                else if (!isKg && workout.Weight.Lb < 1)
                    workout.Weight = new MultiUnityWeight(1, "lb");
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
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                            this.ForceUpdateSize();
                        System.Diagnostics.Debug.WriteLine($"View Width: {this.View.Width}, Height {this.View.Height}");
                        SizeRequest field = this.View.Measure(double.PositiveInfinity, double.PositiveInfinity);
                        System.Diagnostics.Debug.WriteLine($"infinity Width: {field.Request.Width}, Height {field.Request.Height}");
                    });
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
                        var time = workout.IsLastWarmupSet ? "55" : CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsBodyweight ? "30" : "40";
                        if (workout.Id == 16508)
                            time = "120";
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", time);

                    }
                    else
                    {
                        if (LocalDBManager.Instance.GetDBSetting("timer_autoset").Value == "true")
                        {
                            LocalDBManager.Instance.SetDBSetting("timer_remaining", CurrentLog.Instance.GetRecommendationRestTime(workout.Id, false, workout.IsNormalset, CurrentLog.Instance.RecommendationsByExercise[workout.Id].IsPyramid).ToString());
                            if (workout.IsNextBackOffSet)
                            {
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", "50");
                                //Back-off popup
                                if (Config.ShowBackoffPopup == false)
                                {
                                    if (App.IsShowBackOffPopup)
                                        return;
                                    App.IsShowBackOffPopup = true;
                                    ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
                                    {
                                        Message = "Scientists have found that doing more reps with less weight on your last set builds strength and endurance faster (Goto et al. 2004). Let's try it out.",
                                        Title = "Get strong faster",
                                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                        OkText = AppResources.GotIt,
                                        CancelText = AppResources.RemindMe,
                                        OnAction = async (bool ok) =>
                                        {
                                            if (ok)
                                            {
                                                Config.ShowBackoffPopup = true;
                                            }
                                            else
                                            {
                                                Config.ShowBackoffPopup = false;
                                            }
                                        }
                                    };
                                    await Task.Delay(100);
                                    UserDialogs.Instance.Confirm(ShowWelcomePopUp2);
                                }

                            }
                        }
                        else
                        {
                            LocalDBManager.Instance.SetDBSetting("timer_remaining", App.globalTime.ToString());
                        }
                    }
                    if (Timer.Instance.State == "STOPPED" && Timer.Instance.stopRequest == true)
                    {
                        //await Timer.Instance.StopTimer();
                        Timer.Instance.stopRequest = false;
                        //await Task.Delay(1000);
                    }

                    Timer.Instance.StartTimer();
                    if (!workout.IsWarmups && workout.IsFirstWorkSet)
                    {
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
                        }

                    }

                    //}
                }
                catch
                {

                }
                //BtnSaveSet.Clicked += SaveSet_Clicked;

            }

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

        private async void UnFinishedExercise_Clicked(object sender, EventArgs e)
        {
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null)
            {
                ShouldFinishAnimate = false;
                ShouldAnimate = false;
                Xamarin.Forms.MessagingCenter.Send<SaveSetMessage>(new SaveSetMessage() { model = workout, IsFinishExercise = true }, "SaveSetMessage");
            }
        }
        private void RepsMore_Clicked(object sender, EventArgs e)
        {
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
                if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");

            }
            RepsEntry.TextChanged += RepsEntry_TextChanged;
        }

        private void RepsLess_Clicked(object sender, EventArgs e)
        {
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
                if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
            }
            RepsEntry.TextChanged += RepsEntry_TextChanged;
        }


        private async void WeightLess_Clicked(object sender, EventArgs e)
        {
            if (currentWeight - weightStep <= 0)
                return;

            WeightEntry.TextChanged -= WeightEntry_TextChanged;
            currentWeight -= weightStep;
            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null && workout.IsNext)
            {
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

                //LblWeight.Text = WeightEntry.Text = string.Format("{0:0.0}", currentWeight).ReplaceWithDot();
                App.PCWeight = currentWeight;
                var unit = isKg ? WeightUnities.kg : WeightUnities.lb;
                workout.ShouldUpdateIncrement = false;
                workout.Weight = new MultiUnityWeight(currentWeight, unit, false);
                if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");

            }
            WeightEntry.TextChanged += WeightEntry_TextChanged;
        }

        private async void WeightMore_Clicked(object sender, EventArgs e)
        {
            WeightEntry.TextChanged -= WeightEntry_TextChanged;

            currentWeight += weightStep;

            WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
            if (workout != null && workout.IsNext)
            {
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                App.PCWeight = currentWeight;
                //LblWeight.Text = WeightEntry.Text = string.Format("{0:0.0}", currentWeight).ReplaceWithDot();
                var unit = isKg ? WeightUnities.kg : WeightUnities.lb;
                workout.ShouldUpdateIncrement = false;
                workout.Weight = new MultiUnityWeight(currentWeight, unit, false);
                if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                    Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
            }
            WeightEntry.TextChanged += WeightEntry_TextChanged;
        }


        void WeightEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (WeightEntry.Text.EndsWith(",") || WeightEntry.Text.EndsWith("."))
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
                            App.PCWeight = currentWeight;
                            if (workout.IsFirstWorkSet)
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
                    LblWeight.Text = string.Format("{0:0.0}", weight1);
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
                        LblReps.Text = $"{reps}";
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
                    if (!workout.IsBackOffSet && !workout.IsWarmups && !workout.IsFinished && !workout.IsEditing)
                        Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = workout }, "WeightRepsUpdatedMessage");
                }
            }
        }
        void RepsEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {

                if (RepsEntry.Text.EndsWith(",") || RepsEntry.Text.EndsWith(".") || string.IsNullOrEmpty(RepsEntry.Text))
                    return;

                currentReps = Convert.ToInt32(RepsEntry.Text.Replace(",", "").Replace(".", ""));

                WorkoutLogSerieModelRef workout = (WorkoutLogSerieModelRef)this.BindingContext;
                if (workout != null)
                {
                    //RepsEntry.Text = string.Format("{0}", currentReps);
                    //LblReps.Text = string.Format("{0}", currentReps);
                    workout.Reps = currentReps;
                    if (workout.IsFirstWorkSet)
                        Xamarin.Forms.MessagingCenter.Send<OneRMChangedMessage>(new OneRMChangedMessage() { model = workout, Weight = App.PCWeight, Reps = currentReps }, "OneRMChangedMessage"); ;
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

    }
}
