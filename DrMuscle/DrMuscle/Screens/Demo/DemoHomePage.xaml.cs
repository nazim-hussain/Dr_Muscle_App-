using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using Xamarin.Forms;
using DrMuscle.Resx;
using DrMuscle.Constants;
using System.Linq;
using Acr.UserDialogs;
using DrMuscleWebApiSharedModel;
using DrMuscle.Screens.Workouts;
using System.Globalization;
using Plugin.Connectivity;
using DrMuscle.Screens.Subscription;
using DrMuscle.Screens.History;
using Plugin.Toast;
using DrMuscle.Dependencies;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms.PancakeView;
using DrMuscle.Screens.Me;
using Plugin.GoogleClient;
using Plugin.GoogleClient.Shared;
using DrMuscle.Entity;
using Plugin.LatestVersion;
using Xamarin.Essentials;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;
using DrMuscle.Screens.User.OnBoarding;

namespace DrMuscle.Screens.Demo
{
    public partial class DemoHomePage : DrMusclePage
    {
        public ObservableCollection<BotModel> BotList = new ObservableCollection<BotModel>();
        private GetUserProgramInfoResponseModel upi = null;
        GetUserWorkoutLogAverageResponse workoutLogAverage;
        private IFirebase _firebase;
        private bool _isFirstDemoOpen = false;
        private bool _isSecondDemoOpen = false;

        public DemoHomePage()
        {
            InitializeComponent();
            lstChats.ItemsSource = BotList;
            Title = AppResources.DrMuslce;
            _firebase = DependencyService.Get<IFirebase>();

            NavigationPage.SetHasBackButton(this, false);
            Timer.Instance.OnTimerChange -= OnTimerChange;
            Timer.Instance.OnTimerDone -= OnTimerDone;
            Timer.Instance.OnTimerStop -= OnTimerStop;
        }

        public override void OnBeforeShow()
        {
            base.OnBeforeShow();
            //if (Device.RuntimePlatform.Equals(Device.Android))
            if (CurrentLog.Instance.IsDemoPopingOut)
                return;
            MessagingCenter.Send(this, "BackgroundImageUpdated");
            StartSetup();
            try
            {


                if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null)
                {
                    if (LocalDBManager.Instance.GetDBSetting("firstname") != null)
                        CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef;
                }
            }
            catch (Exception ex)
            {

            }

        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                if (CurrentLog.Instance.IsDemoPopingOut)
                    return;
                try
                {
                    if (_isFirstDemoOpen || _isSecondDemoOpen)
                        return;
                    if (PopupNavigation.Instance.PopupStack.Count > 1)
                        return;

                    if (CurrentLog.Instance.WorkoutLogSeriesByExerciseRef == null || CurrentLog.Instance.WorkoutLogSeriesByExerciseRef.Count == 0)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("firstname") != null)
                            CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef;
                    }
                }
                catch (Exception ex)
                {

                }

               

                try
                {
                    _firebase.SetScreenName("home_page");

                    if (LocalDBManager.Instance.GetDBSetting("timer_remaining") == null)
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", "40");

                }
                catch (Exception ex)
                {

                }
                if (CurrentLog.Instance.IsDemoRunningStep2)
                {
                    CurrentLog.Instance.IsDemoRunningStep1 = false;
                    CurrentLog.Instance.IsDemoRunningStep2 = false;

                    BotList.Clear();
                    //await AddQuestion("Congratulations, you finished the demo!");
                    //await AddQuestion("Ready for a real workout? Tap the big start button at the bottom.");
                    //ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
                    //{
                    //    Message = "You finished the demo! Now you see how Dr. Muscle helps you get in shape faster by automating your custom program. Congrats on getting started! You'll be using 22 advanced features automatically. Learn more?",
                    //    Title = "Nice work!",
                    //    //  //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    //    OkText = AppResources.LearnMore,
                    //    CancelText = AppResources.Cancel,
                    //    OnAction = async (bool ok) =>
                    //    {
                    //        if (ok)
                    //        {
                    //            PagesFactory.PushAsync<FAQPage>();
                    //        }
                    //        else
                    //        {

                    //        }
                    //    }
                    //};
                    //await Task.Delay(100);
                    //UserDialogs.Instance.Confirm(ShowWelcomePopUp2);
                    //Device.BeginInvokeOnMainThread( async () =>
                    //{
                    //    await PagesFactory.PushAsync<BoardingBotPage>(true);
                    //});

                }
                //CurrentLog.Instance.IsDemoRunningStep1 = true;
                if (CurrentLog.Instance.IsDemoRunningStep1)
                {
                    CurrentLog.Instance.IsDemoRunningStep1 = false;
                    CurrentLog.Instance.IsDemoRunningStep2 = true;
                    BotList.Clear();
                    await AddQuestion("Congratulations! You have just finished demo workout1.");
                    _isFirstDemoOpen = true;
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Title = $"You did {CurrentLog.Instance.BestReps} reps—nice work!",
                        Message = $"You said \"{CurrentLog.Instance.LastSetWas}\". Next workout, the app will push you to do {CurrentLog.Instance.BestReps + CurrentLog.Instance.RIR} reps.",
                        OkText = "Demo workout 2"
                    });
                    _isFirstDemoOpen = false;
                    CurrentLog.Instance.CurrentExercise = new ExerciceModel()
                    {
                        BodyPartId = 7,
                        VideoUrl = "https://youtu.be/Plh1CyiPE_Y",
                        IsBodyweight = true,
                        IsEasy = false,
                        IsFinished = false,
                        IsMedium = false,
                        IsNextExercise = false,
                        IsNormalSets = false,
                        IsSwapTarget = false,
                        IsSystemExercise = true,
                        IsTimeBased = false,
                        IsUnilateral = false,
                        Label = "Crunch",
                        RepsMaxValue = null,
                        RepsMinValue = null,
                        Timer = null,
                        Id = 864
                    };
                    await PagesFactory.PushAsync<DemoPage2>();
                }



                if (BotList.Count == 0)
                    StartSetup();
                else
                {
                    //var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    //try
                    //{

                    //    if (workouts.Sets == null)
                    //    {
                    //        StartSetup();
                    //    }
                    //}
                    //catch { }
                }

               

            }
            catch (Exception ex)
            {

            }
        }
       

     
        async Task StartSetup()
        {
            try
            {

                BotList.Clear();
                stackOptions.Children.Clear();
                //await Task.Delay(2000);
                //BotList.Add(new BotModel()
                //{
                //    Question = "Dr. Muscle",
                //    Type = BotType.Ques
                //});
                if (LocalDBManager.Instance.GetDBSetting("firstname") == null)
                    return;
                var welcomeNote = "";
                string fname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                Title = $"Welcome back {fname}!";
                //var time = DateTime.Now.Hour;
                //if (time < 12)
                //    welcomeNote = AppResources.GoodMorning;
                //else if (time < 18)
                //    welcomeNote = AppResources.GoodAfternoon;
                //else
                //    welcomeNote = AppResources.GoodEvening;

                //var welcomeMsg = $"{welcomeNote} {fname}—welcome back!";

                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                var isBackedUser = false;
                try
                {
                    if (workouts != null && workouts.Sets != null)
                    {
                        workoutLogAverage = workouts;
                        if (workouts.Averages.Count > 1)
                        {
                            OneRMAverage last = workouts.Averages.OrderBy(a => a.Date).ToList()[workouts.Averages.Count - 1];
                            OneRMAverage before = workouts.Averages.OrderBy(a => a.Date).ToList()[workouts.Averages.Count - 2];
                            decimal progresskg = (last.Average.Kg - before.Average.Kg) * 100 / last.Average.Kg;
                            bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                            BotList.Clear();
                            //    BotList.Add(new BotModel()
                            //{
                            //    Type = BotType.Chart
                            //});

                            isBackedUser = false;
                            //Add Chart

                        }
                    }
                }
                catch (Exception ex)
                {

                }
                if (isBackedUser == false)
                {
                    //BotList.Add(new BotModel()
                    //{
                    //    Question = welcomeMsg,
                    //    Type = BotType.Ques
                    //});

                }
                if (BotList.Count > 1)
                    return;
                if (isBackedUser)
                {

                    await SetStatsMessage($"");
                    GotIt_Clicked2(new DrMuscleButton() { Text = AppResources.GotIt }, EventArgs.Empty);
                }
                await Task.Delay(1000);


                try
                {



                    if (workouts.Sets != null)
                    {
                        workoutLogAverage = workouts;
                    }
                    else
                    {
                        workoutLogAverage = null;
                       
                        workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    }
                    if (workouts.GetUserProgramInfoResponseModel != null)
                    {
                        upi = workouts.GetUserProgramInfoResponseModel;
                        if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                        }

                    }
                }
                catch (Exception ex)
                {
                  
                    workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                }
               
                GotIt_Clicked(new DrMuscleButton() { Text = AppResources.GotIt }, EventArgs.Empty);


            }
            catch (Exception ex)
            {

            }
        }

        async void QuickStats(object sender, EventArgs args)
        {
            try
            {
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                if (workouts != null && workouts.GetUserProgramInfoResponseModel != null)
                {
                    if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram == null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                        {
                            try
                            {
                                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                                long programId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                                upi = new GetUserProgramInfoResponseModel()
                                {
                                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                                };
                                workouts.GetUserProgramInfoResponseModel = upi;
                                ((App)Application.Current).UserWorkoutContexts.workouts = workouts;
                                ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                                //lblProgram.Text = $"{AppResources.Program}: {upi.RecommendedProgram.Label}";
                                //lblWorkout.Text = $"{AppResources.UpNext}: {upi.NextWorkoutTemplate.Label}";
                                //WorkoutNowbutton.Text = $"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}";
                                LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                            }
                            catch (Exception ex)
                            {

                            }

                        }
                    }
                }

                await AddQuestion("Here's a snapshot of your recent progress:");

                await Task.Delay(1000);
                if (workouts != null)
                {
                    if (workouts.Sets == null)
                    {
                        
                        workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    }
                }

                //var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                var strProgress = "Your stats:\n";
                SetStatsMessage(strProgress);

            }
            catch (Exception ex)
            {

            }
        }

        async void SetReturnUserView()
        {
            try
            {
                BotList.Add(new BotModel()
                {
                    Type = BotType.Chart
                });

            }
            catch (Exception ex)
            {

            }
        }

        async Task SetStatsMessage(string strProgress)
        {
            try
            {
                var statsModel = new BotModel()
                {
                    Type = BotType.Stats
                };
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                if (workouts != null)
                {
                    if (workouts.Sets != null)
                    {
                        if (workouts.Averages.Count > 1)
                        {
                            OneRMAverage last = workouts.Averages.OrderBy(a => a.Date).ToList()[workouts.Averages.Count - 1];
                            OneRMAverage before = workouts.Averages.OrderBy(a => a.Date).ToList()[workouts.Averages.Count - 2];
                            decimal progresskg = (last.Average.Kg - before.Average.Kg) * 100 / last.Average.Kg;
                            bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                            // strProgress += String.Format("- {0}: {1}{2} ({3}%)\n", AppResources.MaxStrength, (last.Average.Kg - before.Average.Kg) > 0 ? "+" : "", inKg ? Math.Round(last.Average.Kg - before.Average.Kg) + " kg" : Math.Round(last.Average.Lb - before.Average.Lb) + " lbs", Math.Round(progresskg)).ReplaceWithDot();
                            if ((last.Average.Kg - before.Average.Kg) >= 0)
                            {
                                statsModel.StrengthPerText = String.Format(" {0}{1}%", (last.Average.Kg - before.Average.Kg) >= 0 ? "+" : "", Math.Round(progresskg)).ReplaceWithDot();
                                //statsModel.StrengthMessage = String.Format(" {0}{1} {2}", (last.Average.Kg - before.Average.Kg) >= 0 ? "+ " : "", inKg ? Math.Round(last.Average.Kg - before.Average.Kg) + " kg" : Math.Round(last.Average.Lb - before.Average.Lb) + " lbs", AppResources.MaxStrength).ReplaceWithDot();
                                statsModel.StrengthImage = "up_arrow.png";
                                statsModel.StrengthTextColor = Color.FromHex("#5CD196");
                            }
                            else
                            {
                                statsModel.StrengthPerText = String.Format(" {0}{1}%", (last.Average.Kg - before.Average.Kg) >= 0 ? "+" : "", Math.Round(progresskg)).ReplaceWithDot();
                                //statsModel.StrengthMessage = String.Format(" {0}{1} {2}", (last.Average.Kg - before.Average.Kg) >= 0 ? "+ " : "", inKg ? Math.Round(last.Average.Kg - before.Average.Kg) + " kg" : Math.Round(last.Average.Lb - before.Average.Lb) + " lbs", AppResources.MaxStrength).ReplaceWithDot();
                                statsModel.StrengthImage = "down_arrow.png";
                                statsModel.StrengthTextColor = Color.FromHex("#BA1C31");

                            }
                            //statsModel.StrengthMessage = AppResources.MaxStrength;
                            workouts.Sets.Reverse();
                            workouts.SetsDate.Reverse();

                            if (workouts.Sets.Count > 1)
                            {

                                int firstSets = workouts.Sets[workouts.Sets.Count - 1];
                                int lastSets = workouts.Sets[workouts.Sets.Count - 2];
                                try
                                {
                                    decimal progressSets = (firstSets - lastSets) * 100 / (lastSets == 0 ? 1 : lastSets);
                                    // strProgress += String.Format("- {0}: {1}{2} ({3}%)\n", AppResources.WorkSetsNoColon, (firstSets - lastSets) >= 0 ? "+" : "", firstSets - lastSets, Math.Round(progressSets)).ReplaceWithDot();
                                    if ((firstSets - lastSets) >= 0)
                                    {
                                        statsModel.SetsPerText = String.Format(" {0}{1}%", (firstSets - lastSets) >= 0 ? "+" : "", Math.Round(progressSets)).ReplaceWithDot();
                                        //statsModel.SetsMessage = String.Format(" {0}{1} {2}", (firstSets - lastSets) >= 0 ? "+ " : "", firstSets - lastSets, AppResources.WorkSetsNoColon).ReplaceWithDot();
                                        statsModel.SetsImage = "up_arrow.png";
                                        statsModel.SetTextColor = Color.FromHex("#5CD196");
                                    }
                                    else
                                    {
                                        statsModel.SetsPerText = String.Format(" {0}{1}%", (firstSets - lastSets) >= 0 ? "+" : "", Math.Round(progressSets)).ReplaceWithDot();
                                        //statsModel.SetsMessage = String.Format(" {0}{1} {2}", (firstSets - lastSets) >= 0 ? "+ " : "", firstSets - lastSets, AppResources.WorkSetsNoColon).ReplaceWithDot();
                                        statsModel.SetsImage = "down_arrow.png";
                                        statsModel.SetTextColor = Color.FromHex("#BA1C31");

                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }

                            workouts.Sets.Reverse();
                            workouts.SetsDate.Reverse();

                            try
                            {
                                var exerciseModel = workouts.HistoryExerciseModel;
                                if (exerciseModel != null)
                                {
                                    var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();
                                    var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                                    strProgress += exerciseModel.TotalWorkoutCompleted <= 1 ? $"- {exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutDone}" : $"- {exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutsDone}";
                                    if (workouts.GetUserProgramInfoResponseModel != null)
                                    {
                                        upi = workouts.GetUserProgramInfoResponseModel;
                                        if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                                        {
                                            if (upi.RecommendedProgram.RequiredWorkoutToLevelUp > 0)
                                                strProgress += string.Format("\n- {0} {1}", upi.RecommendedProgram.RemainingToLevelUp, upi.RecommendedProgram.RemainingToLevelUp < 2 ? "workout before you level up" : AppResources.WorkoutsBeforeYouLevelUp);
                                            LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                                        }

                                    }
                                    strProgress += $"\n- {weightLifted.ToString("N0")} {unit} {AppResources.Lifted}";
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                            //Add workout before you level up

                            await Task.Delay(1000);
                            //TODO: Carl: Above strProgress construct user stats based on available data:

                            try
                            {
                                DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                                if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays <= 14)
                                    strProgress += "\n- Most users like you improve 34% in 30 days";
                            }
                            catch (Exception)
                            {

                            }
                            //await AddQuestion(strProgress);
                            BotList.Clear();
                            BotList.Add(statsModel);
                            if (BotList.Count < 2)
                                BotList.Add(new BotModel()
                                {
                                    Type = BotType.Chart
                                });
                            await AddQuestion(strProgress);
                            //await AddOptions(AppResources.GotIt, GotIt_Clicked);
                            GotIt_Clicked2(new DrMuscleButton() { Text = "" }, EventArgs.Empty);


                        }
                        else
                        {
                            try
                            {
                                var exerciseModel = workouts.HistoryExerciseModel;
                                if (exerciseModel != null)
                                {
                                    bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                                    var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();

                                    var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                                    strProgress += exerciseModel.TotalWorkoutCompleted <= 1 ? $"- {exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutDone}" : $"- {exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutsDone}";
                                    if (workouts.GetUserProgramInfoResponseModel != null)
                                    {
                                        if (workouts.Averages.Count > 1)
                                            BotList.Add(new BotModel()
                                            {
                                                Type = BotType.Chart
                                            });
                                        upi = workouts.GetUserProgramInfoResponseModel;
                                        if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                                        {
                                            if (upi.RecommendedProgram.RequiredWorkoutToLevelUp > 0)
                                                strProgress += string.Format("- {0} {1}", upi.RecommendedProgram.RemainingToLevelUp, upi.RecommendedProgram.RemainingToLevelUp < 2 ? "workout before you level up" : AppResources.WorkoutsBeforeYouLevelUp);
                                            //strProgress += $"\n- {upi.RecommendedProgram.RemainingToLevelUp} {AppResources.WorkoutsBeforeYouLevelUp}";
                                            LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                                        }
                                    }
                                    strProgress += $"\n- {weightLifted.ToString("N0")} {unit} {AppResources.Lifted}";
                                    try
                                    {
                                        DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                                        if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays <= 14)
                                            strProgress += "\n- Most users like you improve 34% in 30 days";
                                    }
                                    catch (Exception)
                                    {

                                    }
                                    await AddQuestion(strProgress);

                                    GotIt_Clicked2(new DrMuscleButton() { Text = "" }, EventArgs.Empty);
                                }
                            }
                            catch (Exception ex)
                            {
                                GotIt_Clicked2(new DrMuscleButton() { Text = "" }, EventArgs.Empty);
                            }
                            //Today workout

                        }

                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        async void QuickStatsAutoscrollOff(object sender, EventArgs args)
        {
            try
            {
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                if (workouts != null && workouts.GetUserProgramInfoResponseModel != null)
                {
                    if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram == null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                        {
                            try
                            {
                                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                                long programId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                                upi = new GetUserProgramInfoResponseModel()
                                {
                                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                                };
                                workouts.GetUserProgramInfoResponseModel = upi;
                                ((App)Application.Current).UserWorkoutContexts.workouts = workouts;
                                ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                                //lblProgram.Text = $"{AppResources.Program}: {upi.RecommendedProgram.Label}";
                                //lblWorkout.Text = $"{AppResources.UpNext}: {upi.NextWorkoutTemplate.Label}";
                                //WorkoutNowbutton.Text = $"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}";
                                LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                            }
                            catch (Exception ex)
                            {

                            }

                        }
                    }
                }

                //  await AddQuestion("Here's a snapshot of your recent progress:");

                //await Task.Delay(1000);
                if (workouts != null)
                {
                    if (workouts.Sets == null)
                    {
                        workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    }
                }


                stackOptions.Children.Clear();
                var btn = new DrMuscleButton()
                {
                    Text = "View more stats",
                    TextColor = Color.FromHex("#195377"),
                    BackgroundColor = Color.Transparent,
                };
                btn.Clicked += GotoMePage_Click;
                stackOptions.Children.Add(btn);
                await AddOptionsWithoutScroll("Next workout", BtnStartTodayWorkout_Clicked);
                await Task.Delay(200);
                lstChats.ScrollTo(BotList.First(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.First(), ScrollToPosition.End, false);
                ToolbarItems.Clear();
                //var next = new ToolbarItem("Next workout", null, StartTodaysWorkout);
                //ToolbarItems.Add(next);

            }
            catch (Exception ex)
            {

            }
        }
        async void GotoMePage_Click(object sender, EventArgs e)
        {
            //
            await PagesFactory.PushAsync<MeCombinePage>();

        }
        async void BtnCustomize_Clicked(object sender, EventArgs e)
        {
            stackOptions.Children.Clear();
            //if (((DrMuscleButton)sender).Text == "Something else")
            //    await AddAnswer(((DrMuscleButton)sender).Text);
            await AddOptions("Choose another workout", BtnChooseAnother_Clicked);
            await AddOptions("Short on time or tired today", BtnFeelingWeekShortOnTime_Clicked);
            //await AddOptions("I'm short on time", BtnFeelingWeekShortOnTime_Clicked);
            //await AddOptions("Something else", BtnChangeMyMinde_Clicked);
            //await AddOptions("Check my stats", BtnCheckMyStats_Clicked);
            await AddOptions("Email a human", BtnEmailAHuman_Clicked);

            await AddOptions("Back", BtnBack_Clicked);


            /*
             * if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                await AddOptions($"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}", BtnStartTodayWorkout_Clicked);
            else
                await AddOptions("Start planned workout", BtnStartTodayWorkout_Clicked);
            */
        }

        async void GotIt_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (BotList.Count >= 1)
                    return;
                if (((DrMuscleButton)sender).Text == AppResources.GotIt)
                {
                    //await AddAnswer(AppResources.GotIt);
                    var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    //Today workout
                    TimeSpan timeSpan;
                    String dayStr = "days";
                    int days = 0;
                    int hours = 0;
                    int minutes = 0;
                    try
                    {

                        if (workouts.Averages.Count > 1)
                        {
                            timeSpan = DateTime.Now.ToLocalTime().Subtract(workouts.Averages[0].Date.ToLocalTime());
                            days = timeSpan.Days;
                            hours = (int)timeSpan.TotalHours;
                            minutes = (int)timeSpan.TotalMinutes;
                            dayStr = timeSpan.Days == 1 ? "day" : "days";
                        }

                        if (workouts.LastWorkoutDate != null)
                        {
                            days = (int)(DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays;
                            hours = (int)(DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalHours;
                            minutes = (int)(DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalMinutes;
                            if (days > 0)
                                dayStr = days == 1 ? "day" : "days";
                            else if (hours > 0 && hours < 72)
                                dayStr = hours <= 1 ? "hour" : "hours";
                            else if (minutes < 60)
                                dayStr = minutes <= 1 ? "minute" : "minutes";
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {


                        if (workouts.GetUserProgramInfoResponseModel != null)
                        {
                            upi = workouts.GetUserProgramInfoResponseModel;
                            if (workouts.LastConsecutiveWorkoutDays > 1)
                            {
                                await AddQuestion($"{AppResources.YouHaveBeenWorkingOut} {workouts.LastConsecutiveWorkoutDays} {AppResources.DaysInARowISuggestTalkingADayOffAreYouSureYouWantToWorkOutToday} Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                            }
                            else if (workouts.LastWorkoutDate != null)
                            {
                                if (days > 0)
                                {
                                    if (days > 9)
                                        await AddQuestion($"{AppResources.YourLastWorkoutWas} {days} {dayStr} ago. Your strength may have gone down, so I may recommend a light session. Start planned workout?");
                                    else if (days > 5)
                                        await AddQuestion($"{AppResources.YourLastWorkoutWas} {days} {dayStr} ago. You should be fully recovered. I may suggest extra sets. Start planned workout?");
                                    else
                                    {
                                        if (hours < 18)
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again now... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                        else if (hours < 24)
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                        else if (hours < 72)
                                        {
                                            var sinceday = hours / 24;
                                            var sincehour = hours % 24;

                                            var str = string.Format("{0} {1} {2} {3}", sinceday, sinceday == 1 ? "day" : "days", sincehour, sincehour == 1 ? "hour" : "hours");

                                            await AddQuestion($"Your last workout was {str} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                        }
                                        else
                                            await AddQuestion($"Your last workout was {days} {dayStr} ago. I suggest working out a bit more often for best results. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                    }
                                }
                                else
                                {
                                    if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                                    {
                                        if (minutes < 60)
                                            await AddQuestion($"Your last workout was {minutes} {dayStr} ago. I'm not sure it makes sense to work out again now... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                        else
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again now... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                    }
                                    else
                                    {
                                        if (minutes < 60)
                                            await AddQuestion($"Your last workout was {minutes} {dayStr} ago. I'm not sure it makes sense to work out again today... Start another workout anyway?");
                                        else
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again today... Start another workout anyway?");

                                    }

                                }
                            }
                        }
                        else
                        {
                            if (workouts.LastConsecutiveWorkoutDays > 1)
                            {
                                await AddQuestion($"{AppResources.YouHaveBeenWorkingOut} {workouts.LastConsecutiveWorkoutDays} {AppResources.DaysInARowISuggestTalkingADayOffAreYouSureYouWantToWorkOutToday}");
                            }
                            else if (workouts.LastWorkoutDate != null)
                            {
                                var d = 0;
                                if (days > 0)
                                    d = days;
                                else
                                {
                                    d = timeSpan.Days;
                                    //hours = (int)timeSpan.TotalHours;
                                    //minutes = (int)timeSpan.TotalMinutes;
                                    if (days > 0)
                                        dayStr = d == 1 ? "day" : "days";
                                    else if (hours > 0 && hours < 72)
                                        dayStr = hours <= 1 ? "hour" : "hours";
                                    else if (minutes < 60)
                                        dayStr = minutes <= 1 ? "minute" : "minutes";

                                }
                                if (d > 0)
                                {
                                    if (days > 9)
                                        await AddQuestion($"{AppResources.YourLastWorkoutWas} {d} {dayStr} ago. Your strength may have gone down, so I may recommend a light session. Start planned workout?");
                                    else if (days > 5)
                                        await AddQuestion($"{AppResources.YourLastWorkoutWas} {d} {dayStr} ago. You should be fully recovered. I may suggest extra sets. Start planned workout?");
                                    else
                                    {
                                        if (hours < 18)
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again now... Start another workout anyway?");
                                        else if (hours < 24)
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. You should have recovered. Start planned workout?");
                                        else if (hours < 72)
                                        {
                                            var sinceday = hours / 24;
                                            var sincehour = hours % 24;

                                            var str = string.Format("{0} {1} {2} {3}", sinceday, sinceday == 1 ? "day" : "days", sincehour, sincehour == 1 ? "hour" : "hours");

                                            await AddQuestion($"Your last workout was {str} ago. You should have recovered. Start planned workout?");
                                        }
                                        else
                                            await AddQuestion($"Your last workout was {d} {dayStr} ago. I suggest working out a bit more often for best results. Start planned workout?");
                                    }
                                }
                                else
                                {
                                    if (minutes < 60)
                                        await AddQuestion($"Your last workout was {minutes} {dayStr} ago. I'm not sure it makes sense to work out again today... Start another workout anyway?");
                                    else
                                    {
                                        if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again today... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                        else
                                            await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again today... Start another workout anyway?");
                                    }

                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (BotList.Count == 0)
                {
                    if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                        await AddQuestion($"Looks like you have not worked out yet. Your first workout is {upi.NextWorkoutTemplate.Label}.");
                    else
                        await AddQuestion($"Looks like you have not worked out yet. Start planned workout?");
                }
                stackOptions.Children.Clear();
                var btn = new DrMuscleButton()
                {
                    Text = "More options",
                    TextColor = Color.FromHex("#195377"),
                    BackgroundColor = Color.Transparent,
                };
                btn.Clicked += BtnCustomize_Clicked;
                stackOptions.Children.Add(btn);


                if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                    await AddOptions($"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}", BtnStartTodayWorkout_Clicked);
                else
                    await AddOptions("Start planned workout", BtnStartTodayWorkout_Clicked);

                //await AddOptions("Quick stats",QuickStats);

                //await AddOptions("Start planned workout", BtnStartTodayWorkout_Clicked);
                //await AddOptions("More options", BtnCustomize_Clicked);

            }
            catch (Exception ex)
            {

            }
        }

        async void GotIt_Clicked2(object sender, EventArgs e)
        {
            try
            {
                stackOptions.Children.Clear();
                try
                {


                    if (((DrMuscleButton)sender).Text == AppResources.GotIt)
                    {
                        //await AddAnswer(AppResources.GotIt);
                        var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                        //Today workout
                        TimeSpan timeSpan;
                        String dayStr = "days";
                        int days = 0;
                        int hours = 0;
                        int minutes = 0;


                        if (workouts.LastWorkoutDate != null)
                        {

                            days = (int)(DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays;
                            hours = (int)(DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalHours;
                            minutes = (int)(DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalMinutes;
                            if (days > 0)
                                dayStr = days == 1 ? "day" : "days";
                            else if (hours > 0 && hours < 72)
                                dayStr = hours <= 1 ? "hour" : "hours";
                            else if (minutes < 60)
                                dayStr = minutes <= 1 ? "minute" : "minutes";

                            var d = 0;
                            if (days > 0)
                                d = days;
                            else
                            {
                                d = timeSpan.Days;
                                //hours = (int)timeSpan.TotalHours;
                                //minutes = (int)timeSpan.TotalMinutes;
                                if (days > 0)
                                    dayStr = d == 1 ? "day" : "days";
                                else if (hours > 0 && hours < 72)
                                    dayStr = hours <= 1 ? "hour" : "hours";
                                else if (minutes < 60)
                                    dayStr = minutes <= 1 ? "minute" : "minutes";


                            }
                        }
                        else if (workouts.Averages.Count > 1)
                        {
                            timeSpan = DateTime.Now.ToLocalTime().Subtract(workouts.Averages[0].Date.ToLocalTime());
                            days = timeSpan.Days;
                            dayStr = timeSpan.Days == 1 ? "day" : "days";
                        }

                        if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                        {
                            upi = workouts.GetUserProgramInfoResponseModel;
                            if (workouts.LastConsecutiveWorkoutDays > 1)
                            {
                                await AddQuestion($"{AppResources.YouHaveBeenWorkingOut} {workouts.LastConsecutiveWorkoutDays} {AppResources.DaysInARowISuggestTalkingADayOffAreYouSureYouWantToWorkOutToday} Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                            }
                            else if (workouts.LastWorkoutDate != null)
                            {
                                if (days > 0)
                                    await AddQuestion(days > 9 ? $"{AppResources.YourLastWorkoutWas} {days} {dayStr} ago. I may recommend a light session. Start planned workout?" : $"Your last workout was {days} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                else if (hours > 0)
                                {
                                    if (hours < 18)
                                        await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again now... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                    else if (hours < 24)
                                        await AddQuestion($"Your last workout was {hours} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");

                                }
                                else
                                    await AddQuestion($"Your last workout was {minutes} {dayStr} ago. I'm not sure it makes sense to work out again today... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                            }
                        }
                        else
                        {
                            if (workouts.LastConsecutiveWorkoutDays > 1)
                            {
                                await AddQuestion($"{AppResources.YouHaveBeenWorkingOut} {workouts.LastConsecutiveWorkoutDays} {AppResources.DaysInARowISuggestTalkingADayOffAreYouSureYouWantToWorkOutToday}");
                            }
                            else if (workouts.LastWorkoutDate != null)
                            {
                                if (timeSpan.Days > 0)
                                    await AddQuestion(days > 9 ? $"{AppResources.YourLastWorkoutWas} {timeSpan.Days} {dayStr} ago. I may recommend a light session. Start planned workout?" : $"Your last workout was {timeSpan.Days} {dayStr} ago. You should have recovered. Start planned workout?");
                                else if (hours > 0)
                                {
                                    if (hours < 18)
                                        await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again today... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                    else if (hours < 24)
                                        await AddQuestion($"Your last workout was {hours} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");

                                    //await AddQuestion($"Your last workout was {hours} {dayStr}. I'm not sure it makes sense to work out again today... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                }
                                else
                                    await AddQuestion($"Your last workout was {minutes} {dayStr} ago. I'm not sure it makes sense to work out again today... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                            }

                        }
                    }

                }
                catch (Exception ex)
                {
                    try
                    {
                        var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                        if (workouts != null && workouts.GetUserProgramInfoResponseModel != null)
                        {
                            if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram == null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
                            {
                                if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                                {

                                    long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                                    long programId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                                    upi = new GetUserProgramInfoResponseModel()
                                    {
                                        NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                                        RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                                    };
                                    workouts.GetUserProgramInfoResponseModel = upi;
                                    ((App)Application.Current).UserWorkoutContexts.workouts = workouts;
                                    ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                                    //lblProgram.Text = $"{AppResources.Program}: {upi.RecommendedProgram.Label}";
                                    //lblWorkout.Text = $"{AppResources.UpNext}: {upi.NextWorkoutTemplate.Label}";
                                    //WorkoutNowbutton.Text = $"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}";
                                    LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());

                                }
                            }

                        }
                    }
                    catch (Exception exw)
                    {

                    }

                }
                stackOptions.Children.Clear();
                //await AddOptions(, BtnCustomize_Clicked);
                var btn = new DrMuscleButton()
                {
                    Text = "More options",
                    TextColor = Color.FromHex("#195377"),
                    BackgroundColor = Color.Transparent,
                };
                btn.Clicked += BtnCustomize_Clicked;
                stackOptions.Children.Add(btn);
                if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                    await AddOptions($"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}", BtnStartTodayWorkout_Clicked);
                else
                    await AddOptions("Start planned workout", BtnStartTodayWorkout_Clicked);
                if (BotList.Count == 3)
                {
                    await Task.Delay(200);
                    lstChats.ScrollTo(BotList.First(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.First(), ScrollToPosition.End, false);
                }
                //await AddOptions("Start planned workout", BtnStartTodayWorkout_Clicked);

            }
            catch (Exception ex)
            {

            }
        }
        async void BtnFeelingWeekShortOnTime_Clicked(object sender, EventArgs e)
        {
            stackOptions.Children.Clear();
            await AddAnswer(((DrMuscleButton)sender).Text);
            await AddQuestion("OK then, I suggest we take it easy (you'll do fewer sets, and your workout will be shorter). Sounds good?");

            await AddOptions("Start shorter workout", BtnQuickMode_Clicked);
            await AddOptions("I've changed my mind ", BtnChangeMyMinde_Clicked);

        }

        async void BtnQuickMode_Clicked(object sender, EventArgs e)
        {
            try
            {
                await AddAnswer(((DrMuscleButton)sender).Text);

                //LocalDBManager.Instance.SetDBSetting("QuickMode", "true");
                if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                    LocalDBManager.Instance.SetDBSetting("QuickMode", "false");
                LocalDBManager.Instance.SetDBSetting("OlderQuickMode", LocalDBManager.Instance.GetDBSetting("QuickMode").Value);
                LocalDBManager.Instance.SetDBSetting("QuickMode", "true");
                try
                {
                    LocalDBManager.Instance.ResetReco();
                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                }
                catch (Exception ex)
                {

                }
                StartTodaysWorkout();

            }
            catch (Exception ex)
            {

            }
            //await DrMuscleRestClient.Instance.SetUserQuickMode(new UserInfosModel()
            //{
            //    IsQuickMode = true
            //});
        }

        async void BtnChangeMyMinde_Clicked(object sender, EventArgs e)
        {
            try
            {
                stackOptions.Children.Clear();
                await AddAnswer(((DrMuscleButton)sender).Text);
                await AddQuestion("OK, what do you wanna do?");

                if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                    await AddOptions($"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}", BtnStartTodayWorkout_Clicked);
                else
                    await AddOptions("Start planned workout", BtnStartTodayWorkout_Clicked);
                await AddOptions("Choose another workout", BtnChooseAnother_Clicked);
                await AddOptions("Check my stats", BtnCheckMyStats_Clicked);
                await AddOptions("Email a human", BtnEmailAHuman_Clicked);
                await AddOptions("Back", BtnBack2_Clicked);

                LocalDBManager.Instance.ResetReco();
                if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                    LocalDBManager.Instance.SetDBSetting("QuickMode", "false");
                if (LocalDBManager.Instance.GetDBSetting("OlderQuickMode") != null && LocalDBManager.Instance.GetDBSetting("OlderQuickMode").Value != null)
                    LocalDBManager.Instance.SetDBSetting("QuickMode", LocalDBManager.Instance.GetDBSetting("OlderQuickMode").Value);
                try
                {
                    CurrentLog.Instance.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                }
                catch (Exception ex)
                {

                }

            }
            catch (Exception ex)
            {

            }
        }

        async void BtnStartTodayWorkout_Clicked(object sender, EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            StartTodaysWorkout();
            //PopupNavigation.Instance.PushAsync(new ReminderPopup());
        }



        async void StartTodaysWorkout()
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
                if (App.IsV1UserTrial || await CanGoFurther())
                {
                    if (upi != null)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("Equipment") == null)
                            LocalDBManager.Instance.SetDBSetting("Equipment", "false");

                        if (LocalDBManager.Instance.GetDBSetting("ChinUp") == null)
                            LocalDBManager.Instance.SetDBSetting("ChinUp", "true");

                        if (LocalDBManager.Instance.GetDBSetting("Dumbbell") == null)
                            LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");

                        if (LocalDBManager.Instance.GetDBSetting("Plate") == null)
                            LocalDBManager.Instance.SetDBSetting("Plate", "true");

                        if (LocalDBManager.Instance.GetDBSetting("Pully") == null)
                            LocalDBManager.Instance.SetDBSetting("Pully", "true");

                        WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
                        if (upi == null || upi.NextWorkoutTemplate == null)
                        {
                            await PagesFactory.PushAsync<ChooseDrMuscleOrCustomPage>();
                        }
                        else
                        {
                            if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
                            {
                                try
                                {
                                    GetUserWorkoutTemplateResponseModel w;
                                    //if (LocalDBManager.Instance.GetDBSetting("Equipment").Value == "true")
                                    //{
                                    //    w = await DrMuscleRestClient.Instance.GetCustomizedUserWorkout(new EquipmentModel()
                                    //    {
                                    //        IsEquipmentEnabled = LocalDBManager.Instance.GetDBSetting("Equipment").Value == "true",
                                    //        IsChinUpBarEnabled = LocalDBManager.Instance.GetDBSetting("ChinUp").Value == "true",
                                    //        IsPullyEnabled = LocalDBManager.Instance.GetDBSetting("Pully").Value == "true",
                                    //        IsPlateEnabled = LocalDBManager.Instance.GetDBSetting("Plate").Value == "true"
                                    //    });
                                    //}
                                    //else
                                    w = await DrMuscleRestClient.Instance.GetUserWorkout();
                                    nextWorkout = w.Workouts.First(ww => ww.Id == upi.NextWorkoutTemplate.Id);
                                }
                                catch (Exception ex)
                                {
                                    //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Message = AppResources.PleaseCheckInternetConnection,
                            //    Title = AppResources.ConnectionError
                            //});
                                    return;
                                }

                            }
                            if (nextWorkout != null)
                            {
                                if (CrossConnectivity.Current.IsConnected)
                                {
                                    try
                                    {
                                        GetUserWorkoutTemplateResponseModel w;
                                        System.Diagnostics.Debug.WriteLine($"LTE connected/nStart time:{DateTime.Now.ToString("0:mm: ss.fff")}");
                                        //if (LocalDBManager.Instance.GetDBSetting("Equipment").Value == "true")
                                        //{
                                        //    w = await DrMuscleRestClient.Instance.GetCustomizedUserWorkout(new EquipmentModel()
                                        //    {
                                        //        IsEquipmentEnabled = LocalDBManager.Instance.GetDBSetting("Equipment").Value == "true",
                                        //        IsChinUpBarEnabled = LocalDBManager.Instance.GetDBSetting("ChinUp").Value == "true",
                                        //        IsPullyEnabled = LocalDBManager.Instance.GetDBSetting("Pully").Value == "true",
                                        //        IsPlateEnabled = LocalDBManager.Instance.GetDBSetting("Plate").Value == "true"
                                        //    });
                                        //    nextWorkout = w.Workouts.First(ww => ww.Id == upi.NextWorkoutTemplate.Id);
                                        //}
                                        System.Diagnostics.Debug.WriteLine($"End time:{DateTime.Now.ToString("0:mm:ss.fff")}");
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
                                CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
                                CurrentLog.Instance.WorkoutStarted = true;
                                await PagesFactory.PushAsync<KenkoChooseYourWorkoutExercisePage>();
                            }
                            else
                            {
                                await PagesFactory.PushAsync<ChooseDrMuscleOrCustomPage>();
                            }
                        }
                    }
                    else
                    {
                        upi = await DrMuscleRestClient.Instance.GetUserProgramInfo();
                        if (upi != null)
                        {
                            StartTodaysWorkout();
                        }
                        else
                        {
                            //LocalDBManager.Instance.SetDBSetting("remain","1");
                            await PagesFactory.PushAsync<ChooseDrMuscleOrCustomPage>();
                        }
                    }
                }
                else
                {
                    await PagesFactory.PushAsync<SubscriptionPage>();
                }

            }
            catch (Exception ex)
            {

            }
        }



        async void BtnChooseAnother_Clicked(object sender, EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            await PagesFactory.PushAsync<ChooseDrMuscleOrCustomPage>();
        }

        async void BtnCheckMyStats_Clicked(object sender, EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            CurrentLog.Instance.PastWorkoutDate = null;
            await PagesFactory.PushAsync<HistoryPage>();
        }

        async void BtnEmailAHuman_Clicked(object sender, EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            Device.OpenUri(new Uri("mailto:support@drmuscleapp.com?subject="));

        }

        async void BtnBack2_Clicked(object sender, EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            stackOptions.Children.Clear();
            BtnCustomize_Clicked(new DrMuscleButton(), EventArgs.Empty);

        }


        async void BtnBack_Clicked(object sender, EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            stackOptions.Children.Clear();
            GotIt_Clicked2(new DrMuscleButton(), EventArgs.Empty);

        }

        async Task AddQuestion(string question, bool isAnimated = true)
        {
            BotList.Add(new BotModel()
            {
                Question = question,
                Type = BotType.Ques
            });
            if (isAnimated)
            {
                await Task.Delay(300);
            }
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

        }

        async Task AddAnswer(string answer)
        {
            BotList.Add(new BotModel()
            {
                Answer = answer,
                Type = BotType.Ans
            });

            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            await Task.Delay(300);


        }

        async Task<DrMuscleButton> AddOptions(string title, EventHandler handler)
        {
            var grid = new Grid();
            var pancakeView = new PancakeView() { HeightRequest = 50, Margin = new Thickness(25, 5) };
            //pancakeView.BackgroundGradientStops = new GradientStopCollection() { new GradientStop() { Color=, Offset=0 },
            //new GradientStop() { Color = Color.FromHex("#53C28C"), Offset = 0.65f },  new GradientStop() { Color =  ,Offset=1} };
            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#5CD196"), Offset = 1 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#40A076"), Offset = 0 });
            grid.Children.Add(pancakeView);

            var btn = new DrMuscleButton()
            {
                Text = title,
                TextColor = Color.Black,
                BackgroundColor = Color.White,
            };

            btn.Clicked += handler;
            SetDefaultButtonStyle(btn);

            grid.Children.Add(btn);

            stackOptions.Children.Add(grid);
            //await Task.Delay(300);
            if (BotList.Count > 0)
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            }
            return btn;
        }

        async Task<DrMuscleButton> AddOptionsWithoutScroll(string title, EventHandler handler)
        {
            var grid = new Grid();
            var pancakeView = new PancakeView() { HeightRequest = 50, Margin = new Thickness(25, 5) };
            //pancakeView.BackgroundGradientStops = new GradientStopCollection() { new GradientStop() { Color=Color.FromHex("#40A076"), Offset=0 },
            //new GradientStop() { Color = Color.FromHex("#53C28C"), Offset = 0.65f },  new GradientStop() { Color = Color.FromHex("#68E6A3") ,Offset=1} };
            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#5CD196"), Offset = 1 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#40A076"), Offset = 0 });

            grid.Children.Add(pancakeView);

            var btn = new DrMuscleButton()
            {
                Text = title,
                TextColor = Color.Black,
                BackgroundColor = Color.White,
            };
            btn.Clicked += handler;
            SetDefaultButtonStyle(btn);

            grid.Children.Add(btn);
            stackOptions.Children.Add(grid);
            //await Task.Delay(300);


            return btn;
        }

        void SetDefaultButtonStyle(Button btn)
        {
            btn.BackgroundColor = Color.Transparent;
            btn.BorderWidth = 2;
            btn.CornerRadius = 5;
            btn.Margin = new Thickness(25, 5, 25, 5);
            btn.FontAttributes = FontAttributes.Bold;
            btn.BorderColor = Color.Transparent;
            btn.TextColor = Color.White;
            btn.HeightRequest = 50;

        }

        void SetEmphasisButtonStyle(Button btn)
        {
            btn.TextColor = Color.Black;
            btn.BackgroundColor = Color.White;
            btn.Margin = new Thickness(25, 5, 25, 5);
            btn.HeightRequest = 50;
            btn.BorderWidth = 2;
            btn.CornerRadius = 5;
            btn.FontAttributes = FontAttributes.Bold;
        }
        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await DisplayAlert(AppResources.Exit, AppResources.AreYouSureYouWantToExit, AppResources.Yes, AppResources.No);
                if (result)
                {
                    var kill = DependencyService.Get<IKillAppService>();
                    kill.ExitApp();
                }
            });
            return true;
        }

        


    }
}
