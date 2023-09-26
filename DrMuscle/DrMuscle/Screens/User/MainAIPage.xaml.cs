using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Controls;
using DrMuscle.Dependencies;
using DrMuscle.Effects;
using DrMuscle.Entity;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Model;
using DrMuscle.Resx;
using DrMuscle.Screens.Eve;
using DrMuscle.Screens.History;
using DrMuscle.Screens.Me;
using DrMuscle.Screens.Subscription;
using DrMuscle.Screens.Workouts;
using DrMuscle.Services;
using DrMuscle.Views;
using DrMuscleWebApiSharedModel;
using Microcharts;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Plugin.Connectivity;
using Plugin.LatestVersion;
using Plugin.Toast;
using Rg.Plugins.Popup.Services;
using SkiaSharp;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using Xamarin.Plugin.Calendar;

using static DrMuscle.Constants.AppThemeConstants;

namespace DrMuscle.Screens.User
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainAIPage : DrMusclePage
    {
        public ObservableCollection<BotModel> BotList = new ObservableCollection<BotModel>();
        private GetUserProgramInfoResponseModel upi = null;
        GetUserWorkoutLogAverageResponse workoutLogAverage;
        private IFirebase _firebase;
        private bool _isFirstDemoOpen = false;
        private bool _isSecondDemoOpen = false;
        private bool _isPurchasedOpen = false;
        private bool _isAllDone = false;
        private bool _isReconfigured = false;
        private bool ShouldAnimate = false;
        private bool _isReload = false;
        private bool _isInStrengthPhase = false;
        private bool _isAnyWorkoutFinished = false;
        private bool? _isBetaExperience = null;
        private string LblWeightGoal = "";
        private int _recommendeddays = 0;
        List<List<string>> _tipsArray = new List<List<string>>();
        DrMuscleButton btnStartWorkout;
        Label _lblWorkoutName;
        private int workoutCountForCongratulations = 0;
        private string liftedForCongratulations = "";
        private bool isHowWasSelected = false;
        private bool isAIPopupShown = false;
        private bool isAILoaded = false;
        private string AILoadedText = "";
        private string AITitle = "";
        private bool isWorkoutLoaded = false;
        public static bool _isJustAppOpen = false;
        private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        private Dictionary<double, string> IndexToDateLabel2 = new Dictionary<double, string>();
        Xamarin.Plugin.Calendar.Controls.Calendar calendar;
        Xamarin.Plugin.Calendar.Models.EventCollection eventCollection = new Xamarin.Plugin.Calendar.Models.EventCollection();
        
        public MainAIPage()
        {

            InitializeComponent();
            lstChats.ItemsSource = BotList;
            Title = "Home";//AppResources.DrMuslce;
            _firebase = DependencyService.Get<IFirebase>();

            _isJustAppOpen = true;
            CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
            MessagingCenter.Subscribe<Message.FinishWorkoutMessage>(this, "FinishWorkoutMessage", (obj) =>
            {
                FinishWorkout(obj.PopupMessage);
            });

            //
            MessagingCenter.Subscribe<Message.ShareMessage>(this, "ShareMessage", (obj) =>
            {
                BtnShareMonth_Clicked(new DrMuscleButton(), EventArgs.Empty);
            });
            //
            MessagingCenter.Subscribe<Message.HowWasWorkoutMessage>(this, "HowWasWorkoutMessage",async (obj) =>
            {
                var thatwas = "easier";
                if (obj.HowWasWorkout.Equals("good"))
                    thatwas = "similar";
                else if (obj.HowWasWorkout.Equals("too easy"))
                    thatwas = "harder";
                //var aiCard = BotList.Where(x => x.Type == BotType.AICard).FirstOrDefault();
                BotList.Insert(0, new BotModel() { Type = BotType.LastWorkoutWas,
                    Question=$"Workout {obj.HowWasWorkout}", Part1 = $"Next one will be {thatwas}.",
                    StrengthImage = "chekedGreen.png",
                    IsNewRecordAvailable = true
                });
                if (Device.RuntimePlatform.Equals(Device.Android))
                {
                    await Task.Delay(200);
                    lstChats.ScrollTo(BotList.First(), ScrollToPosition.Start, true);
                }
                isHowWasSelected = true;

                if (isWorkoutLoaded)
                {
                    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var modalPage = new Views.CongratulationsPopup("FirstWorkout.png", $"{workoutCountForCongratulations} workouts done!", $"Congrats on lifting {liftedForCongratulations}. Enjoying Dr. Muscle?", "Continue");
                    modalPage.Disappearing += (sender2, e2) =>
                    {
                        waitHandle.Set();
                    };
                    await PopupNavigation.Instance.PushAsync(modalPage);
                    await Task.Run(() => waitHandle.WaitOne());
                    if (!isAIPopupShown && isAILoaded)
                    {
                        //isAIPopupShown = true;
                        //var modalPage2 = new Views.GeneralPopup("TrueState.png", AITitle, AILoadedText, "Continue");
                        //await PopupNavigation.Instance.PushAsync(modalPage2);
                    }
                } else if (!isAIPopupShown && isAILoaded)
                {
                    //isAIPopupShown = true;
                    //var modalPage2 = new Views.GeneralPopup("TrueState.png", AITitle, AILoadedText, "Continue");
                    //await PopupNavigation.Instance.PushAsync(modalPage2);
                }
            });
            MessagingCenter.Subscribe<Message.ClosePopupMessage>(this, "ClosePopupMessage", (obj) =>
            {
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    PopupNavigation.Instance.PopAsync();
            });

            MessagingCenter.Subscribe<Message.LevelUpInfoMessage>(this, "LevelUpInfoMessage", (obj) =>
            {
                //if (BotList.Count > 2 && BotList.First().Type == BotType.Congratulations && LocalDBManager.Instance.GetDBSetting("IsSystemWorkout") != null && LocalDBManager.Instance.GetDBSetting("IsSystemWorkout").Value == "true")
                //    BotList.RemoveAt(3);


                //if (!string.IsNullOrEmpty(obj.Msg))
                //    BotList.Insert(3,new BotModel()
                //    {
                //        Question = obj.Msg,
                //        Type = BotType.ExplainerCell
                //    });
                LoadSavedWorkout();
            });

            MessagingCenter.Subscribe<Message.StartNormalWorkout>(this, "StartNormalWorkout", (obj) =>
            {
                StartTodaysWorkout();
            });


            MessagingCenter.Subscribe<Message.UpdatedWorkoutMessage>(this, "UpdatedWorkoutMessage", (obj) =>
            {
                if (obj.OnlyRefresh)
                    UpdatedMassunitWorkout();
                else if (obj.workoutChange)
                {
                    loadChangedWorkout();
                }
                else
                    UpdatedWorkout();
            });

            MessagingCenter.Subscribe<Message.RecongrationtMessage>(this, "RecongrationtMessage", (obj) =>
            {
                _isReconfigured = true;
                ReconfigrationWorkout();
            });
            MessagingCenter.Subscribe<Message.SignupFinishMessage>(this, "SignupFinishMessage", async (obj) =>
            {
                if (obj.IsRefresh)
                {
                    App.IsNUX = false;
                    UpdatedAndReload();
                }
                else
                {
                    WalkthroughPopup();
                    // if (Device.RuntimePlatform.Equals(Device.Android))
                    await StartSetup();
                    OnAppearing();
                }
            });
            MessagingCenter.Subscribe<Message.HomeOptionsMessage>(this, "HomeOptionsMessage", (obj) =>
            {
                if (obj.Options.Contains("startworkout"))
                {
                    StartTodaysWorkout();
                }
                else
                    BtnFeelingWeekShortOnTime_Clicked(new DrMuscleButton() { Text = obj.Options }, EventArgs.Empty);
            });
            MessagingCenter.Subscribe<Message.SubscriptionSuccessfulMessage>(this, "SubscriptionSuccessfulMessage", (obj) =>
            {
                SubscriptioPurchased();
            });
            MessagingCenter.Subscribe<Message.GlobalSettingsChangeMessage>(this, "GlobalSettingsChangeMessage", (obj) =>
            {
                //IsGlobalSettingsChanged = true;
                if (!obj.IsDisappear)
                    ReloadWorkout();
            });

            MessagingCenter.Subscribe<Message.BodyweightUpdateMessage>(this, "BodyweightUpdateMessage", (obj) =>
            {
                UpdateBodyWeight();
            });
            MessagingCenter.Subscribe<Message.ExerciseDeleteMessage>(this, "ExerciseDeleteMessage", (obj) =>
            {
                RefreshCurrentWorkout();
            });


            DependencyService.Get<IDrMuscleSubcription>().OnMealPlanAccessPurchased += async delegate {
                App.IsMealPlan = true;
                if (Device.RuntimePlatform.Equals(Device.Android))
                    UserDialogs.Instance.AlertAsync(new AlertConfig() { AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray), Message = "Your purchase was successful.", Title = "You're all set", OkText = "OK" });
                GetMealPlan_Clicked(btnMealPlan, EventArgs.Empty);
            };

            MessagingCenter.Subscribe<NavigationOnNotificationTappedMessage>(this, "NavigationOnNotificationTappedMessage", (obj) =>
            {
                if (obj.NotificationType == "Local")
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    PushToWorkout();
                }
                if (obj.NotificationType == "Workout")
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    CurrentLog.Instance.IsWelcomePopup = true;
                    CurrentLog.Instance.IsWelcomeMessage = true;
                    PushToUnfinishedWorkout(obj.WorkoutId);
                }
            });
            StackSteps1.IsVisible = false;
            StackSteps2.IsVisible = false;

            calendar = new Xamarin.Plugin.Calendar.Controls.Calendar()
            {
                Culture = CultureInfo.CreateSpecificCulture("en-GB")
            };
            calendar.HeaderSectionTemplate = new CalendarHeaderView();
            calendar.FooterArrowVisible = false;
            calendar.FooterSectionVisible = false;
            calendar.MonthLabelColor = Color.FromHex("#195377");
            //calendar.TodayOutlineColor = Color.FromHex("#195377");
            calendar.SelectedDateColor = Color.Black;
            calendar.SelectedDayBackgroundColor = Color.Transparent;
            calendar.SelectedDayTextColor = Color.Black;
            calendar.OtherMonthDayIsVisible = false;
            calendar.TodayOutlineColor = Color.Transparent;
            
            //calendar.Culture = CultureInfo.CreateSpecificCulture("en-US");

            calendar.EventIndicatorType = Xamarin.Plugin.Calendar.Enums.EventIndicatorType.Background;
            calendarBox2.Content = calendar;
            calendar.Events = eventCollection;
            //CurrentLog.Instance.IsRecoveredWorkout = true;
            //System.Threading.Tasks.Task.Run(async () =>
            //{
            //    //Add your code here.
            //    await Task.Delay(5000);
            //    Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage("Local"), "NavigationOnNotificationTappedMessage");
            //}).ConfigureAwait(false);
            Timer.Instance.OnTimerChange -= OnTimerChange;
            Timer.Instance.OnTimerDone -= OnTimerDone;
            Timer.Instance.OnTimerStop -= OnTimerStop;
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", ShowMoreMenu, ToolbarItemOrder.Primary, 0);
            this.ToolbarItems.Add(generalToolbarItem);
            if (LocalDBManager.Instance.GetDBSetting("StrengthPhase") == null)
                LocalDBManager.Instance.SetDBSetting("StrengthPhase", "true");
            var images = GetWorkoutCoverImageArray();
            if (Config.ShowWorkoutImagesNumber >= images.Count)
                Config.ShowWorkoutImagesNumber = 0;

            ImgWorkout.Source = images[Config.ShowWorkoutImagesNumber];
            ImgWorkout2.Source = images[Config.ShowWorkoutImagesNumber];
            Config.ShowWorkoutImagesNumber++;


        }

        public async void ShowMoreMenu()
        {
            var popUp = new FullscreenMenu();
            PopupNavigation.Instance.PushAsync(popUp);
        }

        public async void SlideGeneralHomeAction()
        {

            WalkthroughPopup();

            if (TooltipEffect.GetHasTooltip(btnStartWorkout))
            {
                TooltipEffect.SetHasTooltip(btnStartWorkout, false);
                TooltipEffect.SetHasTooltip(btnStartWorkout, true);
            }

            return;

        }
        public void SlideGeneralSummaryAction()
        {
            try
            {
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[1];
                    ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[0];
                }
                ((RightSideMasterPage)SlideMenu).ShowHomeSummaryMenu();
                if (SlideMenu.IsShown)
                {
                    HideMenu();
                }
                else
                {
                    ShowMenu();
                }

            }
            catch (Exception ex)
            {

            }
        }
        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height);
        //    if (Device.RuntimePlatform.Equals(Device.Android))
        //        StatusBarHeight.Height = 5;
        //    else
        //        StatusBarHeight.Height = App.StatusBarHeight;

        //}
        private async void WalkthroughPopup()
        {
            App.IsNUX = false;
            LocalDBManager.Instance.SetDBSetting("GoalBox2", "true");
            LocalDBManager.Instance.SetDBSetting("IsFirstMessage", "First1");
            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Tabs[0].BadgeCaption = 1;
            LocalDBManager.Instance.SetDBSetting("FirstTImeOpen", DateTime.Now.Ticks.ToString());
            _isFirstDemoOpen = true;

            var timeSpan = new TimeSpan(2, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            //new TimeSpan(2,DateTime.Now.Hour,DateTime.Now.Minute,0);
            DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Dr Muscle", "No new workout -- anything holding you back?", timeSpan, 1051, NotificationInterval.Week);
            App.IsOnboarding = true;

            //ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
            //{
            //    Title = "Custom program ready!",
            //    Message = "View your custom program?",
            //    //  //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //    OkText = "View program",
            //    CancelText = AppResources.Skip,
            //    OnAction = async (bool ok) =>
            //    {
            //        if (ok)
            //        {
            //            App.IsOnboarding = true;
            //            DemoStartTodaysWorkout();
            //        }
            //        else
            //        {
            //        }
            //    }
            //};
            //await Task.Delay(500);
            //UserDialogs.Instance.Confirm(ShowWelcomePopUp2);

            //YOu Aced the test
            
                var modalPage1 = new Views.GeneralPopup("EmptyStar.png", "Success!", "You aced the test. Welcome home.", "Continue", null, false, false);
                PopupNavigation.Instance.PushAsync(modalPage1);
            
        }

        public override void OnBeforeShow()
        {
            base.OnBeforeShow();
            //if (Device.RuntimePlatform.Equals(Device.Android))
            // MessagingCenter.Send(this, "BackgroundImageUpdated");
            _isJustAppOpen = false;
            workoutLogAverage = null;
            upi = null;
            EmptyStateStack.IsVisible = false;

            StackSteps1.IsVisible = false;
            StackSteps2.IsVisible = false;
            //TODO: changed for New UI

            lstChats.IsVisible = false;
            mainGrid.BackgroundColor = Color.FromHex("#f4f4f4");

            StartSetup();
            //LoadAB();
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

        private async void LoadAB()
        {
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("firstname") == null)
                    return;
                if (LocalDBManager.Instance.GetDBSetting("BetaLoaded")?.Value == "true")
                {
                    _isBetaExperience = true;
                    return;
                }

                    IRemoteConfigurationService remoteConfigurationService = DependencyService.Get<IRemoteConfigurationService>();
                await remoteConfigurationService.FetchAndActivateAsync();
                var configuration = await remoteConfigurationService.GetAsync<FeatureConfiguration>(Device.RuntimePlatform.Equals(Device.Android) ? "new_preview_feature" : "Features");

                if (LocalDBManager.Instance.GetDBSetting("Environment") != null && LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production")
                { }
                else
                {

                  
                }
            }
            catch (Exception ex)
            {

            }
            //ShowPlayerDetail = configuration.ShowPlayerDetail;
        }

        async void FinishWorkout(string msg)
        {
            int consecutiveWeek = 0;
            string totalStrenth = "";

            try
            {
                try
                {

                    workoutCountForCongratulations = 0;
                    liftedForCongratulations = "";
                    isHowWasSelected = false;
                    isWorkoutLoaded = false;
                    isAIPopupShown = false;
                    isAILoaded = false;
                    lstChats.IsVisible = true;
                    EmptyStateStack.IsVisible = false;
                    StackSteps1.IsVisible = false;
                    StackSteps2.IsVisible = false;
                    var wl = ((App)Application.Current).UserWorkoutContexts.workouts;

                    this.ToolbarItems.Clear();
                    if (wl != null)
                    {
                        if (wl.ConsecutiveWeeks != null && wl.ConsecutiveWeeks.Count > 0)
                        {
                            var lastTime = wl.ConsecutiveWeeks.Last();
                            var year = Convert.ToString(lastTime.MaxWeek).Substring(0, 4);
                            var weekOfYear = Convert.ToString(lastTime.MaxWeek).Substring(4, 2);
                            CultureInfo myCI = new CultureInfo("en-US");
                            Calendar cal = myCI.Calendar;
                            if (int.Parse(year) == DateTime.Now.Year)
                            {
                                var currentWeekOfYear = cal.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                                if (int.Parse(weekOfYear) == currentWeekOfYear)
                                {
                                    consecutiveWeek = (int)lastTime.ConsecutiveWeeks;
                                }
                                if (int.Parse(weekOfYear) == currentWeekOfYear - 1 && CurrentLog.Instance.IsWorkoutedOut)
                                {
                                    consecutiveWeek = (int)lastTime.ConsecutiveWeeks + 1;
                                }
                                else if (int.Parse(weekOfYear) == currentWeekOfYear - 1)
                                {
                                    consecutiveWeek = (int)lastTime.ConsecutiveWeeks;
                                }
                            }
                        }
                        else
                        {
                            if (CurrentLog.Instance.IsWorkoutedOut)
                                consecutiveWeek = 1;
                        }
                    }

                    if (wl.Sets != null)
                    {
                        if (wl.Averages.Count > 1)
                        {
                            OneRMAverage last = wl.Averages.ToList()[wl.Averages.Count - 1];
                            totalStrenth = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? $"{Math.Round(last.Average.Kg, 2)} kg" : $"{Math.Round(last.Average.Lb, 2)} lbs";
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                //lstChats.BackgroundColor = Color.White;
                //stackOptions.BackgroundColor = Color.White;
                ((App)Application.Current).UserWorkoutContexts.workouts = null;
                ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                BotList.Clear();
                stackOptions.Children.Clear();

                if (LocalDBManager.Instance.GetDBSetting("firstname") == null)
                    return;
                var welcomeNote = msg;
                Title = "Summary";// 
                int rec = 0;
                int worksetCount = 0;
                string workoutStrength = "0";
                double caloriesDouble = 0;
                welcomeNote = $"Today, you did:";
                List<string> strExercisesName = new List<string>();
                List<string> exersizeStrength = new List<string>();
                string calories = "";
                var newRecord = new BotModel()
                {
                    Type = BotType.NewRecord,
                    Question = ""
                };
                //var workoutStats = new BotModel()
                //{
                //    Type = BotType.Workout,
                //    Question = ""
                //};
                int min = 0, exerciseCount = 0;
                double totalMinutes = 0;
                
                try
                {
                    if (LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}") != null)
                    {

                        var time = LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}").Value;
                        if (time != null && time != "0")
                        {
                            var startedTime = new DateTime(long.Parse(time));
                            if ((DateTime.Now - startedTime).Minutes > 0)
                            {

                                TimeSpan span = DateTime.Now - startedTime;
                                //if (span.TotalMinutes > 120)
                                //{
                                //    startedTime = DateTime.Now.AddHours(-2);
                                //    span = DateTime.Now - startedTime;
                                //}

                                newRecord.MinuteCount = Math.Floor(span.TotalMinutes).ToString();
                                min = (int)Math.Floor(span.TotalMinutes);

                                decimal weight2 = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture);
                                caloriesDouble = (double)Math.Round(((decimal)Math.Floor((span.TotalMinutes > 120 ? 120 : span.TotalMinutes)) * (decimal)6.0 * (decimal)3.5 * weight2) / 200);
                                newRecord.CaloriesBurned = caloriesDouble.ToString();

                                totalMinutes = span.TotalMinutes;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                }
                LocalDBManager.Instance.SetDBSetting($"Time{DateTime.Now.Year}", null);

                if (LocalDBManager.Instance.GetDBSetting($"Exercises{DateTime.Now.Date}") != null)
                {
                    var exeCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"Exercises{DateTime.Now.Date}").Value);
                    newRecord.ExerciseCount = exeCount.ToString();
                    exerciseCount = exeCount;
                    //welcomeNote += exeCount < 2 ? $"\n- {exeCount} exercise" : $"\n- {exeCount} exercises";
                    LocalDBManager.Instance.SetDBSetting($"Exercises{DateTime.Now.Date}", "0");
                }
                var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                if (LocalDBManager.Instance.GetDBSetting($"WorkoutStrenth{DateTime.Now.Date}") != null)
                {
                   
                    var workstrn = double.Parse(LocalDBManager.Instance.GetDBSetting($"WorkoutStrenth{DateTime.Now.Date}").Value);
                    workoutStrength = isKg ? $"{Math.Round(workstrn, 2)} kg" : $"{Math.Round(new MultiUnityWeight((decimal)workstrn, WeightUnities.kg).Lb, 2)} lbs"; LocalDBManager.Instance.SetDBSetting($"WorkoutStrenth{DateTime.Now.Date}", "0");
                }
                try
                {

                    if (LocalDBManager.Instance.GetDBSetting($"ExerciseStrenth{DateTime.Now.Date}") != null)
                    {
                        //var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                        var arrayStrength = LocalDBManager.Instance.GetDBSetting($"ExerciseStrenth{DateTime.Now.Date}").Value;
                        var arrayExercise = LocalDBManager.Instance.GetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}").Value;
                        foreach (var item in arrayStrength.Split('|'))
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                var works = double.Parse(item);
                                var wgth = isKg ? $"{Math.Round(works, 2)} kg" : $"{Math.Round(new MultiUnityWeight((decimal)works, WeightUnities.kg).Lb, 2)} lbs";
                                exersizeStrength.Add(wgth);
                            }
                        }
                        foreach (var item in arrayExercise.Split('|'))
                        {
                            if (!string.IsNullOrEmpty(item))
                                strExercisesName.Add(item);
                        }
                        LocalDBManager.Instance.SetDBSetting($"ExerciseStrenth{DateTime.Now.Date}", "");
                        LocalDBManager.Instance.SetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}", "");
                    }

                }
                catch (Exception ex)
                {

                }


                if (LocalDBManager.Instance.GetDBSetting($"Sets{DateTime.Now.Date}") != null)
                {
                    var setCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"Sets{DateTime.Now.Date}").Value);
                    //welcomeNote += setCount < 2 ? $"\n- {setCount} work set" : $"\n- {setCount} work sets";
                    worksetCount = setCount;
                    newRecord.WorksetCount = setCount.ToString();
                    LocalDBManager.Instance.SetDBSetting($"Sets{DateTime.Now.Date}", "0");

                }
                LocalDBManager.Instance.SetDBSetting($"AnySets{DateTime.Now.Date}", "0");
                if (LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout") != null)
                {
                    var recordCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout").Value);
                    //welcomeNote += recordCount < 2 ? $"\n- {recordCount} new record" : $"\n- {recordCount} new records";
                    newRecord.RecordCount = recordCount.ToString();
                    rec = recordCount;
                    LocalDBManager.Instance.SetDBSetting($"RecordFinishWorkout", "0");
                }

                //if (exerciseCount > 0)
                //{

                //    //Get AI Analysis

                //    var name = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
                //    var goalWeight = LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value;
                //    if (LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value != null)
                //    {
                //        var goalWeights = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value.Replace(",", "."), CultureInfo.InvariantCulture);
                //        goalWeight = isKg ?  string.Format("{0:0.##} {1}", Math.Round(goalWeights, 2), "kg") : string.Format("{0:0.##} {1}", new MultiUnityWeight(goalWeights,"kg").Lb , "lbs");
                //    }


                //    var query = $"You are a fitness coach. I am your client. I just finished a workout.\nName: {name}\nGoal weight: {goalWeight}\nExercises: {exerciseCount}\nSets: {worksetCount}\nNew records: {newRecord.RecordCount}\nCalories: {caloriesDouble}\nWrite me a short text message in the style of Arnold Schwarzenegger. Highlight key progress towards goal. Include just one famous quote. Attribute it. Don't use \"I\". Don't encourage me to push to my limit. End on an upbeat note.";

                //    var newRecordList1 = ((App)Application.Current).NewRecordModelContext.NewRecordList;
                //    var newExeRecord = newRecordList1.OrderByDescending(x => x.ExercisePercentageNumber).FirstOrDefault();
                //    var exeName = "";
                //    var exeProgress = "";
                //    if (rec > 0)
                //    {
                //        var newRecordList = ((App)Application.Current).NewRecordModelContext.NewRecordList;
                //        //var isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg";
                //        var item = newRecordList.OrderByDescending(x => x.ExercisePercentageNumber).First();
                //        exeName = item.ExerciseName.Trim();


                //        if (item.Prev1RM == null)
                //        {

                //            exeProgress = string.Format("+{0:0.#} {1}", Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                //        }
                //        else
                //        {
                //            exeProgress = string.Format("+{0:0.#}{1}", item.ExercisePercentageNumber, "%").ReplaceWithDot();
                //        }

                //    }

                //    var query1 = $"You are a bodybuilding coach. I am your client. I just finished a workout.\n\nName: {name}\n\nGoal weight: {goalWeight}\n\nExercises: {exerciseCount}\n\nSets: {worksetCount}\n\nCalories burned: {caloriesDouble}\n\nWorkout week streak: {consecutiveWeek}\n\nNew records: {newRecord.RecordCount}\n\n{exeName}: {exeProgress}\n\n\n\nWrite me a poem of exactly 3 stanzas of 4 lines in the style of Arnold Schwarzenegger following these instructions:\n1. 5 words or less per line.\n2. Start by highlighting key progress towards the goal weight of {goalWeight}.\n3. Write numbers as numbers.\n4. End on an upbeat note.\n5. Include an inspirational title.";


                
                //    //TODO: Peom AI
                //    //
                //    var model2 = new BotModel()
                //    {
                //        Question = "Loading...",
                //        IsNewRecordAvailable = rec == 0,
                //        Part1 = "Loading...",
                //        StrengthImage = "Fire.png",
                //        Type = BotType.NewRecordCard
                //    };
                //    BotList.Add(model2);

                //    UserDialogs.Instance.HideLoading();
                //    AnaliesAIWithChatGPT2(query1, model2);
                //    isAILoaded = true;
                //    UserDialogs.Instance.HideLoading();


                //}

                //if (rec > 0)


                newRecord.ChainCount = Convert.ToString(consecutiveWeek);
                if (!string.IsNullOrEmpty(calories))
                    welcomeNote += calories;
                if (string.IsNullOrEmpty(newRecord.CaloriesBurned))
                    newRecord.CaloriesBurned = "N/A";
                if (string.IsNullOrEmpty(newRecord.RecordCount))
                    newRecord.RecordCount = "0";

                if (string.IsNullOrEmpty(newRecord.MinuteCount))
                    newRecord.MinuteCount = "0";
                if (string.IsNullOrEmpty(newRecord.ExerciseCount))
                    newRecord.ExerciseCount = "0";
                if (string.IsNullOrEmpty(newRecord.WorksetCount))
                    newRecord.WorksetCount = "N/A";

                //if (workoutStats.ExerciseCount != "0")
                //BotList.Add(new BotModel()
                //{
                //    Type = BotType.ExplainerCell,
                //    Question = $"{workoutStats.ExerciseCount} exercises done in {workoutStats.MinuteCount} minutes."
                //});
                //var model = new BotModel()
                //{
                //    Question = "Loading.",
                //    Type = BotType.NextWorkoutLoad
                //};
                //BotList.Add(model);
                //await Task.Delay(2000);
                //BotList.Remove(model);
                //model = new BotModel()
                //{
                //    Question = "Loading..",
                //    Type = BotType.NextWorkoutLoad
                //};
                //BotList.Add(model);
                //await Task.Delay(2000);
                //BotList.Remove(model);
                //model = new BotModel()
                //{
                //    Question = "Loading...",
                //    Type = BotType.NextWorkoutLoad
                //};
                //BotList.Add(model);
                //await Task.Delay(2000);
                //BotList.Remove(model);
                //model = new BotModel()
                //{
                //    Question = "View summary",
                //    Type = BotType.NextWorkoutLoad
                //};
                //BotList.Add(model);
                var recordText = rec > 1 ? "records" : "record";
                
                if (rec > 0)
                {
                    var newRecordList = ((App)Application.Current).NewRecordModelContext.NewRecordList.Where(x=>x.ExercisePercentageNumber>0);
                    //var isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg";

                    var info = "";
                        var item = newRecordList.OrderByDescending(x => x.ExercisePercentageNumber).First();

                        var subTitle = "";
                        if (item.Prev1RM == null)
                        {

                            subTitle = string.Format("+{0:0.#} {1}", Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                        }
                        else
                        {
                            subTitle = string.Format("+{0:0.#}{1}", item.ExercisePercentageNumber, "%").ReplaceWithDot();
                        }
                        if (rec > 1)
                            info = $"{item.ExerciseName.Trim()} & {rec - 1} more";
                        else if (rec > 0)
                            info = $"{item.ExerciseName.Trim()}";

                    
                    newRecord.Question = "";
                    newRecord.IsNewRecordAvailable = true;
                    BotList.Add(new BotModel()
                    {
                        Type = BotType.LastWorkoutWas,
                        Question = $"{newRecord.RecordCount} new {recordText}!",
                        Part1 = info,
                        StrengthImage = "starTrophy.png",
                        IsNewRecordAvailable = true
                    });
                }
                else
                {
                    newRecord.Question = "Congratulations!";
                }
                
                try
                {
                   // var isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg";
                    var newRecordList = ((App)Application.Current).NewRecordModelContext.NewRecordList.Where(x => x.ExercisePercentageNumber > 0);
                    int count = 0;
                    if (rec > 0) { 
                    foreach (var item in newRecordList.OrderByDescending(x => x.ExercisePercentageNumber))
                    {
                        var title = $"+{item.ExercisePercentage.Trim()} {item.ExerciseName}";
                        var subTitle = "";
                        if (item.Prev1RM == null)
                        {
                            subTitle = string.Format("From 0 {0} to {1:0.#} {2}.", (isKg ? "kg" : "lbs"), Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                        }
                        else
                        {
                            subTitle = string.Format("From {0:0.#} {1} to {2:0.#} {3}.", Math.Round(isKg ? item.Prev1RM.Kg : item.Prev1RM.Lb, 1), (isKg ? "kg" : "lbs"), Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                        }

                        BotList.Add(new BotModel()
                        {
                            Question = title,
                            IsNewRecordAvailable = count == 0,
                            Part1 = subTitle,
                            StrengthImage = "starTrophy.png",
                            Type = BotType.NewRecordCard
                        });
                        count = 1;
                    }
                    }	
                    try
                    {
                        if (exerciseCount > 0)
                        {
                            //Get AI Analysis

                           

                            var titleList = new String[] {
                "Keep pushing",
                "Keep grinding",
                "Keep moving forward",
                "Stronger every day",
                "Progress takes time",
                "Rise",
                "Embrace the journey",
                "One step at a time",
                "Be bold, be brave",
                "Make it happen",
                "Your inner strength",
                "Unlock your potential",
                "Choose to rise",
                "Be the change you want to see",
                "Never give up",
                "Keep rising",
                "Eyes on the goal",
                "Dream big, achieve big",
                "Success is a choice",
                "Perseverance pays off",
                "Better every day",
                "The journey is the reward",
                "Your best self",
                "It's never too late",
                "Your goal is within reach",
                "Make your dreams a reality",
                "Take the next step",
                "Always believe in yourself",
                "Eyes on the prize",
                "You're in control",
                "Stay strong, stay focused",
                "Keep climbing",
                "Trust the process",
                "It's worth It",
                "Your time is now",
                "You're in charge",
                "Believe",
                "Dream, believe, achieve",
                "Stay committed",
                "Trust your inner voice",
                "Believe in your potential",
                "Strive for greatness",
                "Your dreams within reach",
                "Perseverance, perseverance",
                "You got this"};
                            var newRecordLine = "";
                            
                            var newRecordList1 = ((App)Application.Current).NewRecordModelContext.NewRecordList;
                            if (newRecordList1.Count > newRecordList1.Count(x=>x.IsMobility))
                                newRecordList1 = newRecordList1.Except(newRecordList1.Where(x=>x.IsMobility)).ToList();
                            bool isMixed = false;
                            if (newRecordList1.Where(x => x.Prev1RM == null).ToList().Count > 0 && newRecordList1.Where(x => x.Prev1RM != null).ToList().Count > 0)
                            {
                                isMixed = true;
                            } 
                            var newExeRecord = newRecordList1.OrderByDescending(x => x.ExercisePercentageNumber).FirstOrDefault();
                            if (isMixed)
                                newExeRecord = newRecordList1.Where(x=>x.Prev1RM != null).OrderByDescending(x => x.ExercisePercentageNumber).FirstOrDefault();
                            
                            
                            var exeName = "";
                            var exeProgress = "";
                            if (newRecordList1.Count > 0)
                            {
                                var item = newExeRecord;// newRecordList.OrderByDescending(x => x.ExercisePercentageNumber).First();
                                exeName = item.ExerciseName.Trim();
                                if (item.Prev1RM == null)
                                {
                                    exeProgress = string.Format("+{0:0.#} {1}", Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                                    newRecordLine = $"Last workout: {item.ExerciseName} 1RM 0\nToday: {item.ExerciseName} 1RM {exeProgress}\n\n";
                                }
                                else
                                {
                                    exeProgress = string.Format("+{0:0.#}{1}", item.ExercisePercentageNumber, "%").ReplaceWithDot();
                                    var newText = string.Format("{0:0.#} {1}", Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                                    var old = string.Format("{0:0.#} {1}", Math.Round(isKg ? item.Prev1RM.Kg : item.Prev1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                                    newRecordLine = $"Last workout: {item.ExerciseName} 1RM {old}\nToday: {item.ExerciseName} 1RM {newText}\n\n";
                                }
                            }
                            var dropRecordLine = "";
                            //if (!string.IsNullOrEmpty(Config.DownRecordExplainer))
                            //    dropRecordLine = Config.DownRecordExplainer;
                            //else
                            if (newRecordList1.Count > 0)
                            {
                                var item = newRecordList1.OrderBy(x => x.ExercisePercentageNumber).First();
                                if (isMixed)
                                    item = newRecordList1.Where( x=>x.Prev1RM != null).OrderByDescending(x => x.ExercisePercentageNumber).FirstOrDefault();

                                if (newRecordList1.Count > 1)
                                {
                                    if (newExeRecord.ExerciseName == item.ExerciseName)
                                    {
                                        newRecordList1.Remove(item);
                                        if (isMixed && newRecordList1.Where(x=>x.Prev1RM != null).OrderByDescending(x => x.ExercisePercentageNumber ).FirstOrDefault() != null)
                                            item = newRecordList1.Where(x=>x.Prev1RM != null).OrderByDescending(x => x.ExercisePercentageNumber ).FirstOrDefault();
                                        else
                                            item = newRecordList1.OrderBy(x => x.ExercisePercentageNumber).First();
                                    }
                                }
                                exeName = item.ExerciseName.Trim();
                                if (item.Prev1RM == null)
                                {
                                    exeProgress = string.Format("+{0:0.#} {1}", Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                                    dropRecordLine = $"Last workout: {item.ExerciseName} 1RM 0\nToday: {item.ExerciseName} 1RM {exeProgress}\n\n";
                                }
                                else
                                {

                                    exeProgress = string.Format("+{0:0.#}{1}", item.ExercisePercentageNumber, "%").ReplaceWithDot();
                                    var newText = string.Format("{0:0.#} {1}", Math.Round(isKg ? item.New1RM.Kg : item.New1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                                    var old = string.Format("{0:0.#} {1}", Math.Round(isKg ? item.Prev1RM.Kg : item.Prev1RM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                                    dropRecordLine = $"Last workout: {item.ExerciseName} 1RM {old}\nToday: {item.ExerciseName} 1RM {newText}\n\n";
                                }
                            }
                            var goalWeight = LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value;
                            var firstname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
                            if (LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value != null)
                            {
                                var goalWeights = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value.Replace(",", "."), CultureInfo.InvariantCulture);
                                goalWeight = isKg ? string.Format("{0:0.##} {1}", Math.Round(goalWeights, 2), "kg") : string.Format("{0:0.##} {1}", new MultiUnityWeight(goalWeights, "kg").Lb, "lbs");
                            }

                            var query = $"My name is {firstname}, You are an AI bodybuilding coach in a mobile app. The app creates custom programs and updates them automatically every workout. I am your client. I just finished a workout.\n\nGoal weight: {goalWeight}\nExercises: {exerciseCount}\nSets: {worksetCount}\nCalories burned: {caloriesDouble}\nWorkout week streak: {consecutiveWeek}\nNew records: {newRecord.RecordCount}\n\nRecent logs:\n\n{newRecordLine}{dropRecordLine}What can you tell me about that? Write 5 paragraphs of about 45 words each in the style of John Meadows. Use numbers, but don't estimate future progress. Short sentences and short, common words. Follow this outline:\n\nParagraph 1: Congratulate and analyze workout overall.\nParagraph 2: Analyze best lift.\nParagraph 3: Analyze worst lift.\nParagraph 4: A quote from one of the following, at random: Arnold Schwarzenegger, Bruce Lee, Vince Lombardi, Winston Churchill, Mike Tyson, Muhammad Ali, Michael Jordan, Abraham Lincoln, the Dalai Lama, Nelson Mandela, Elon Musk, Steve Jobs, Alexander the Great, Julius Caesar, Marcus Aurelius, Babe Ruth. Attribute it.\nParagraph 5: Wrap up. Mention you will optimize for progress. End on an upbeat note.\n\nInclude numbers. Don't use \"I\". Don't tell me to push myself. Don't recommend or suggest more variety or new exercises. Don't use hashtags. Don't mention bodybuilding or bodybuilders. Number paragraphs.";
                           // var query = $"You are a bodybuilding coach. I am your client. I just finished a workout. Recent logs:\n\n{newRecordLine}{dropRecordLine}What can you tell me about that? Write 4 paragraphs of about 40 words each in the style of John Meadows. Use numbers, but don't estimate future progress. Short sentences and short, common words. Follow this outline:\n\nParagraph 1: Congratulate and analyze best lift.\n\nParagraph 2: Analyze worst lift.\n\nParagraph 3: One famous quote. Attribute it.\n\nParagraph 4: Wrap up. Mention you will tweak for progress. End on an upbeat note.\n\nInclude numbers. Don't use \"I\". Don't tell me to push myself. Don't recommend or suggest more variety or new exercises. Don't use hashtags. Don't mention bodybuilding or bodybuilders.";

                            var model = new BotModel()
                            {
                                Question = titleList[new Random().Next(44)],
                                IsNewRecordAvailable = rec == 0,
                                Part1 = "Loading analysis...",
                                StrengthImage = "Lamp.png",
                                Type = BotType.AICard
                            };
                            
                            //Config.DownRecordExplainer = "";
                            //Config.DownRecordPercentage = 0;
                            if (!App.IsFreePlan) {
                                BotList.Add(model);
                                AnaliesAIWithChatGPT(query, model);
                            }
                            else
                            {
                                model.Part1 = "Premium feature.";
                                model.Part2 = " Learn more.";
                                
                                BotList.Add(model);
                            }
                            //UserDialogs.Instance.HideLoading();


                            //TODO: Peom AI

                            var name = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
                            

                            var query1 = $"You are a bodybuilding coach. I am your client. I just finished a workout.\n\nName: {name}\n\nGoal weight: {goalWeight}\n\nExercises: {exerciseCount}\n\nSets: {worksetCount}\n\nCalories burned: {caloriesDouble}\n\nWorkout week streak: {consecutiveWeek}\n\nNew records: {newRecord.RecordCount}\n\n{exeName}: {exeProgress}\n\n\n\nWrite me a poem of exactly 3 stanzas of 4 lines in the style of Arnold Schwarzenegger following these instructions:\n1. 5 words or less per line.\n2. Start by highlighting key progress towards the goal weight of {goalWeight}.\n3. Write numbers as numbers.\n4. End on an upbeat note.\n5. Include an inspirational title.";

                            var model2 = new BotModel()
                            {
                                Question = "Loading...",
                                IsNewRecordAvailable = rec == 0,
                                Part1 = "Loading poem...",
                                StrengthImage = "Fire.png",
                                LevelUpText = "2",
                                Type = BotType.AICard
                            };
                            
                            if (!App.IsFreePlan)
                            {
                                BotList.Add(model2);
                                AnaliesAIWithChatGPT2(query1, model2);
                            }
                            else
                            {
                                model2.Question = "Loading poem...";
                                model2.Part1 = "Premium feature.";
                                model2.Part2 = " Learn more.";
                                BotList.Add(model2);
                            }

                            isAILoaded = true;
                            UserDialogs.Instance.HideLoading();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                ((App)Application.Current).NewRecordModelContext.NewRecordList = new List<NewRecordModel>();
                    ((App)Application.Current).NewRecordModelContext.SaveContexts();
                }
                catch (Exception ex)
                {

                }
                BotList.Add(newRecord);
                //BotList.Add(workoutStats);


                //if (BotList.Count >= 5)
                //    return;

                if (Device.RuntimePlatform.Equals(Device.iOS) && exerciseCount > 0)
                {
                    IHealthData _healthService = DependencyService.Get<IHealthData>();
                    await _healthService.GetHealthPermissionAsync(async (r) =>
                    {
                        var a = r;
                        if (r)
                        {

                            _healthService.SaveActiveMinutes(Math.Floor(totalMinutes), rec, exerciseCount, Convert.ToString(worksetCount), totalStrenth, workoutStrength, caloriesDouble, strExercisesName, exersizeStrength);
                        }
                    });
                }
                mainScroll.IsVisible = false;
                //==========Tips end
            }
            catch (Exception ex)
            {

            }


            try
            {

                await Task.Delay(1000);
                //==========Tips start

                _tipsArray = GetTipsArray();

                if (Config.ShowTipsNumber == _tipsArray.Count)
                    Config.ShowTipsNumber = 0;
                BotList.Add(new BotModel()
                {
                    Question = "Tip of the day",
                    Answer = $"{_tipsArray[Config.ShowTipsNumber][1]}",
                    Type = BotType.TipCard
                });
                if (Config.ShowTipsNumber >= _tipsArray.Count)
                    Config.ShowTipsNumber = 0;

                //BotList.Add(new BotModel()
                //{
                //    Type = BotType.ExplainerCell,
                //    Question = $"{_tipsArray[Config.ShowTipsNumber][1]}"
                //});

                Config.ShowTipsNumber += 1;

            }
            catch (Exception ex)
            {

            }
            try
            {
                BotList.Add(new BotModel()
                {
                    Question = "Loading...",
                    Type = BotType.NextWorkoutLoadingCard
                });
            }
            catch (Exception ex)
            {

            }

        }
        private async void WorkoutsGPT(BotModel botModel)
        {
            
        }

        private async void PoemGPT(BotModel botModel)
        {
            
        }

        private async Task<string> AnaliesAIWithChatGPT(string query, BotModel botModel, double temperature = 0.7, int maxTokens = 2000, double topP = 1, double frequencyPenalty = 0, double presencePenalty = 0)
        {
            return "";

            // Create an HTTP client

        }

        private async Task<string> AnaliesAIWithChatGPT2(string query, BotModel botModel, double temperature = 0.7, int maxTokens = 2000, double topP = 1, double frequencyPenalty = 0, double presencePenalty = 0)
        {
            return "";
        }

        private async void SetAILoader(BotModel model)
        {
            
            try
            {
                model.Question = "Loading.";
                if (!model.Question.StartsWith("Loading"))
                return;
            model.Question = "Loading.";
            await Task.Delay(300);
            if (!model.Question.StartsWith("Loading"))
                return;
            model.Question = "Loading..";
            await Task.Delay(300);
            
            SetAILoader(model);

            }
            catch (Exception ex)
            {

            }
        }

        private async Task SetUserEquipmentSettings(string active)
        {
            var model = new EquipmentModel()
            {
                IsEquipmentEnabled = LocalDBManager.Instance.GetDBSetting("Equipment").Value == "true",
                IsChinUpBarEnabled = LocalDBManager.Instance.GetDBSetting("ChinUp").Value == "true",
                IsDumbbellEnabled = LocalDBManager.Instance.GetDBSetting("Dumbbell").Value == "true",
                IsPlateEnabled = LocalDBManager.Instance.GetDBSetting("Plate").Value == "true",
                IsPullyEnabled = LocalDBManager.Instance.GetDBSetting("Pully").Value == "true",
                IsHomeEquipmentEnabled = LocalDBManager.Instance.GetDBSetting("HomeMainEquipment").Value == "true",
                IsHomeChinupBar = LocalDBManager.Instance.GetDBSetting("HomeChinUp").Value == "true",
                IsHomeDumbbell = LocalDBManager.Instance.GetDBSetting("HomeDumbbell").Value == "true",
                IsHomePlate = LocalDBManager.Instance.GetDBSetting("HomePlate").Value == "true",
                IsHomePully = LocalDBManager.Instance.GetDBSetting("HomePully").Value == "true",
                IsOtherEquipmentEnabled = LocalDBManager.Instance.GetDBSetting("OtherMainEquipment").Value == "true",
                IsOtherChinupBar = LocalDBManager.Instance.GetDBSetting("OtherChinUp").Value == "true",
                IsOtherDumbbell = LocalDBManager.Instance.GetDBSetting("OtherDumbbell").Value == "true",
                IsOtherPlate = LocalDBManager.Instance.GetDBSetting("OtherPlate").Value == "true",
                IsOtherPully = LocalDBManager.Instance.GetDBSetting("OtherPully").Value == "true",
            };
            if (active == "gym")
            {

                LocalDBManager.Instance.SetDBSetting("GymEquipment", "true");
                LocalDBManager.Instance.SetDBSetting("HomeEquipment", "");
                LocalDBManager.Instance.SetDBSetting("OtherEquipment", "");
            }
            if (active == "home")
            {
                LocalDBManager.Instance.SetDBSetting("GymEquipment", "");
                LocalDBManager.Instance.SetDBSetting("HomeEquipment", "true");
                LocalDBManager.Instance.SetDBSetting("OtherEquipment", "");
            }
            if (active == "other")
            {
                LocalDBManager.Instance.SetDBSetting("GymEquipment", "");
                LocalDBManager.Instance.SetDBSetting("HomeEquipment", "");
                LocalDBManager.Instance.SetDBSetting("OtherEquipment", "true");
            }

            if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                model.Active = "gym";
            if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                model.Active = "home";
            if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                model.Active = "other";

        }

        async void LoadSavedWorkout()
        {
            upi = null;
            ((App)Application.Current).UserWorkoutContexts.workouts = null;
            await Task.Delay(1300);
            await FetchMainData();
            await WorkoutLogSets();
            LocalDBManager.Instance.SetDBSetting("isNextWorkoutLoaded", "true");

            var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;

            var loader = BotList.Where(x => x.Question == "Loading..." && x.Type != BotType.AICard).FirstOrDefault();
            var index = 0;
            if (loader != null)
            {
                index = BotList.IndexOf(loader);
                BotList.Remove(loader);
            }
            
            if (isHowWasSelected && !isAIPopupShown && isAILoaded)
            {
                //isAIPopupShown = true;
                //var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                //var modalPage = new Views.GeneralPopup("TrueState.png", AITitle, AILoadedText, "Continue");
                //modalPage.Disappearing += (sender2, e2) =>
                //{
                //    waitHandle.Set();
                //};
                //await PopupNavigation.Instance.PushAsync(modalPage);
                //await Task.Run(() => waitHandle.WaitOne());
           
            }
            try
            {
                if (workouts.Sets != null)
                {
                    workoutLogAverage = workouts;
                }
                else
                {
                    workoutLogAverage = null;
                    await FetchMainDataWithSets();
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

                await FetchMainData();
                workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
            }

            // var summaryLevelUpModel = new BotModel() { Type = BotType.SummaryLevelup };
            var summaryRestModel = new BotModel()
            {
                Type = BotType.SummaryRest,
                Question = "Next workout",
            };
            try
            {
                if (workouts != null)
                {
                    if (workouts.Sets != null)
                    {

                        if (workouts.HistoryExerciseModel != null)
                        {
                            var exerciseModel = workouts.HistoryExerciseModel;
                            var inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                            var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;


                            var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();
                            summaryRestModel.WorkoutDone = exerciseModel.TotalWorkoutCompleted.ToString();
                            summaryRestModel.WorkoutDoneText = (exerciseModel.TotalWorkoutCompleted <= 1 ? $"{AppResources.WorkOut}".ToLower().FirstCharToUpper() : $"{AppResources.Workouts}").ToLower().FirstCharToUpper();
                            ///Holdinggs
                            //levelUpBotModel.WorkoutDoneText = exerciseModel.TotalWorkoutCompleted <= 1 ? $"{AppResources.WorkoutDone}".ToUpper() : $"{AppResources.WorkoutsDone}".ToUpper();
                            summaryRestModel.LevelUpMessage = "N/A";
                            summaryRestModel.LevelUpText = $"Workouts\nbefore level up".ToLower().FirstCharToUpper();

                            bool IsStrengthPhashe = false;
                            try
                            {


                                var workouts1 = ((App)Application.Current).UserWorkoutContexts.workouts;
                                int remainingWorkout = 0, totalworkout = 0;
                                var name = "";
                                if (workouts1 != null && workouts.GetUserProgramInfoResponseModel != null)
                                {
                                    if (workouts1.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                                    {
                                        name = workouts1.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label;
                                        totalworkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RequiredWorkoutToLevelUp;
                                        remainingWorkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp != null ? (int)workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp : 0;
                                    }
                                }

                                //await MakePopup("Congratulations—you have reached level 2!", "Your program now includes new and harder exercises.", AppResources.GotIt, AppResources.LearnMore, true, "https://dr-muscle.com/building-muscle");
                                IsStrengthPhashe = RecoComputation.IsInStrengthPhase(name, int.Parse(string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value) ? "40" : LocalDBManager.Instance.GetDBSetting("Age")?.Value), remainingWorkout, totalworkout);
                                if (_isInStrengthPhase == false && IsStrengthPhashe)
                                {
                                    ConfirmConfig supersetConfig = new ConfirmConfig()
                                    {
                                        Title = "Congratulations!",
                                        Message = "You will now start a strength phase.",
                                        OkText = "Learn more",
                                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                        CancelText = AppResources.GotIt,
                                    };

                                    var x = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                                    if (x)
                                    {
                                        Device.OpenUri(new Uri("https://dr-muscle.com/strength-cycles-for-hypertrophy/"));
                                    }
                                }
                                _isInStrengthPhase = IsStrengthPhashe;


                            }
                            catch (Exception ex)
                            {

                            }
                            if (workouts.GetUserProgramInfoResponseModel != null)
                            {
                                upi = workouts.GetUserProgramInfoResponseModel;
                                if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                                {
                                    summaryRestModel.LevelUpMessage = upi.RecommendedProgram.RemainingToLevelUp > 0 ? $"{(upi.RecommendedProgram.RequiredWorkoutToLevelUp - (upi.RecommendedProgram.RequiredWorkoutToLevelUp - upi.RecommendedProgram.RemainingToLevelUp)).ToString()}" : "N/A";
                                    var work = upi.RecommendedProgram.RemainingToLevelUp > 1 ? "WORKOUTS" : "WORKOUT";
                                    summaryRestModel.LevelUpText = (upi.RecommendedProgram.RemainingToLevelUp > 0 ? IsStrengthPhashe ? $"{work}\nbefore level up\n(strength phase)" : $"{work}\nbefore level up" : "CUSTOM PROGRAM").ToLower().FirstCharToUpper();
                                    //if (upi.RecommendedProgram.RequiredWorkoutToLevelUp > 0)
                                    //    strProgress += string.Format("\n- {0} {1}", upi.RecommendedProgram.RemainingToLevelUp, upi.RecommendedProgram.RemainingToLevelUp < 2 ? "workout before you level up" : AppResources.WorkoutsBeforeYouLevelUp);
                                    LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                                }

                            }
                            else if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                            {
                                summaryRestModel.LevelUpMessage = upi.RecommendedProgram.RemainingToLevelUp > 0 ? $"{(upi.RecommendedProgram.RequiredWorkoutToLevelUp - upi.RecommendedProgram.RemainingToLevelUp).ToString()}/{upi.RecommendedProgram.RequiredWorkoutToLevelUp.ToString()}" : "N/A";
                                var work = upi.RecommendedProgram.RemainingToLevelUp > 1 ? "WORKOUTS" : "WORKOUT";
                                summaryRestModel.LevelUpText = (upi.RecommendedProgram.RemainingToLevelUp > 0 ? IsStrengthPhashe ? $"{work}\nbefore level up\n(strength phase)" : $"{work}\nbefore level up" : "CUSTOM PROGRAM").ToLower().FirstCharToUpper();
                                //if (upi.RecommendedProgram.RequiredWorkoutToLevelUp > 0)
                                //    strProgress += string.Format("\n- {0} {1}", upi.RecommendedProgram.RemainingToLevelUp, upi.RecommendedProgram.RemainingToLevelUp < 2 ? "workout before you level up" : AppResources.WorkoutsBeforeYouLevelUp);
                                LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                            }
                            else
                            {
                                summaryRestModel.LevelUpMessage = summaryRestModel.WorkoutDone;
                                summaryRestModel.LevelUpText = summaryRestModel.WorkoutDoneText;
                            }
                            if (summaryRestModel.LevelUpText.ToLower().Contains("custom program"))
                            {
                                summaryRestModel.LevelUpMessage = summaryRestModel.WorkoutDone;
                                summaryRestModel.LevelUpText = summaryRestModel.WorkoutDoneText;
                            }
                            _weightLifted = SO30180672.FormatNumber((long)weightLifted);
                            _weightLiftedText = $"{unit} {AppResources.Lifted}".ToLower();
                            summaryRestModel.LbsLifted = _weightLifted;
                            summaryRestModel.LbsLiftedText = _weightLiftedText;


                            //var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                            //var unit = inKg ? "kg" : "lbs";
                            //if (!BotList.Any(x=>x.Question.Contains("Total workouts done:")))
                            //BotList.Add(new BotModel()
                            //{
                            //    Question = $"Total workouts done: {exerciseModel.TotalWorkoutCompleted}\nTotal {unit} lifted: {SO30180672.FormatNumber((long)weightLifted)}",
                            //    Type = BotType.ExplainerCell
                            //});

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            //Level up Row


            //Last Row

            if (workouts != null && workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                summaryRestModel.LbsLiftedText = workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label;
            else
                summaryRestModel.LbsLiftedText = "N/A";
            //BotList.Add(summaryLevelUpModel);
            var RequiredHours = 18;
            int hours = 0;
            if (workouts != null && workouts.LastWorkoutDate != null)
            {
                bool IsInserted = false;

                TimeSpan timeSpan;
                String dayStr = "days";
                int days = 0;
                hours = 0;
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

                

                if (workouts.LastWorkoutDate != null)
                {
                    RequiredHours = 18;
                    if (workouts != null && workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                    {
                        if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bodyweight") ||
workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("mobility") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("powerlifting") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("full-body") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bands"))
                        {

                            RequiredHours = 42;
                            if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                            {
                                if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) < 30)
                                    RequiredHours = 18;
                            }
                            if (workouts.LastConsecutiveWorkoutDays > 1 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 2)
                                RequiredHours = 42;
                        }

                        else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] legs") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] legs"))
                        {
                            RequiredHours = 18;
                            if (workouts.LastConsecutiveWorkoutDays > 5 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 3)

                            {
                                RequiredHours = 42;
                            }
                        }
                        else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("split"))
                        {
                            RequiredHours = 18;
                            if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                            {
                                if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) > 50)
                                    RequiredHours = 42;
                            }
                            if (workouts.LastConsecutiveWorkoutDays > 1 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 2)
                                RequiredHours = 42;
                        }
                    }

                    if (days > 0 && hours >= RequiredHours)
                    {
                        summaryRestModel.SinceTime = $"{days} {dayStr}";
                        summaryRestModel.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                        _isAnyWorkoutFinished = false;
                        summaryRestModel.TrainRest = "Train";
                        summaryRestModel.StrengthTextColor = AppThemeConstants.GreenColor;
                        summaryRestModel.TrainRestText = "Coach says";// "Recovered";// (days > 9 ? "I may recommend lighter weights" : "You should have recovered").ToLower().FirstCharToUpper();
                        BotList.Add(summaryRestModel);
                        //BotList.Add(restModel);
                        IsInserted = true;
                        //await AddQuestion(days > 9 ? $"{AppResources.YourLastWorkoutWas} {days} {dayStr} ago. I may recommend a light session. Start planned workout?" : $"Your last workout was {days} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                    }
                    else if (hours > 0)
                    {

                        if (hours < RequiredHours)
                        {
                            if (LocalDBManager.Instance.GetDBSetting("RecommendedReminder")?.Value == "true" )
                            {
                                var dt1 = DateTime.Now.AddHours(RequiredHours - hours);
                                var timeSpan1 = TimeSpan.FromHours(RequiredHours - hours) + DateTime.Now.TimeOfDay;
                                
                                //set Recommended days notification if user has set recommended
                                DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan1, 1111, NotificationInterval.Week);
                            }
                            else
                                DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1111);
                            summaryRestModel.StrengthTextColor = AppThemeConstants.DarkRedColor;
                            var h = RequiredHours - hours <= 1 ? "hour" : "hours";
                            //summaryRestModel.TrainRest = $"{hours}/{RequiredHours} {h}";
                            //summaryRestModel.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();

                            summaryRestModel.TrainRest = "Rest";
                            summaryRestModel.SinceTime = $"{hours}/{RequiredHours} {h}";
                            if (LocalDBManager.Instance.GetDBSetting($"WorkoutAdded{DateTime.Now.Date.AddDays(1)}")?.Value == "true")
                            {
                                summaryRestModel.SinceTime = "18 hours";
                            }
                            _isAnyWorkoutFinished = true;
                            summaryRestModel.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                            //lifted.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();
                            summaryRestModel.TrainRestText = "Coach says";// "Fatigued";// $"At least {RequiredHours - hours} {h} more to recover".ToLower().FirstCharToUpper();

                            BotList.Add(summaryRestModel);
                            //BotList.Add(restModel);
                            IsInserted = true;
                            //await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again now... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                        }
                        else
                        {
                            summaryRestModel.SinceTime = $"{hours} {dayStr}";
                            summaryRestModel.SinceTime = $"{hours}/{RequiredHours} hours";
                            if (LocalDBManager.Instance.GetDBSetting($"WorkoutAdded{DateTime.Now.Date.AddDays(1)}")?.Value == "true")
                            {
                                summaryRestModel.SinceTime = "18 hours";
                            }
                            summaryRestModel.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                            _isAnyWorkoutFinished = false;
                            summaryRestModel.TrainRest = "Train";
                            summaryRestModel.StrengthTextColor = AppThemeConstants.GreenColor;
                            summaryRestModel.TrainRestText = "Coach says";// "Recovered"; //"You should have recovered".ToLower().FirstCharToUpper();
                            BotList.Add(summaryRestModel);
                            //BotList.Add(restModel);
                            IsInserted = true;
                            // await AddQuestion($"Your last workout was {hours} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                        }

                    }
                    else
                    {
                        var h = RequiredHours - hours <= 1 ? "hour" : "hours";
                        //summaryRestModel.TrainRest = $"{RequiredHours} hours";
                        summaryRestModel.StrengthTextColor = AppThemeConstants.DarkRedColor;
                        //summaryRestModel.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();
                        summaryRestModel.TrainRest = "Rest";
                        summaryRestModel.SinceTime = $"{RequiredHours} {h}";
                        summaryRestModel.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                        summaryRestModel.TrainRestText = "Coach says";// "Fatigued";//$"At least {RequiredHours - hours} {h} more to recover".ToLower().FirstCharToUpper();
                        if (LocalDBManager.Instance.GetDBSetting($"WorkoutAdded{DateTime.Now.Date.AddDays(1)}")?.Value == "true")
                        {
                            summaryRestModel.SinceTime = "18 hours";
                        }
                        if (LocalDBManager.Instance.GetDBSetting("RecommendedReminder")?.Value == "true" )
                        {
                            var dt1 = DateTime.Now.AddHours(RequiredHours - hours);
                            var timeSpan1 = TimeSpan.FromHours(RequiredHours - hours) + DateTime.Now.TimeOfDay;

                            DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan1, 1111, NotificationInterval.Week);
                        }
                        else
                            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1111);
                        _isAnyWorkoutFinished = true;
                        if (index == 0 || index >= BotList.Count)
                            BotList.Add(summaryRestModel);
                        else
                            BotList.Insert(index, summaryRestModel);
                        //BotList.Add(restModel);
                        IsInserted = true;
                        //await AddQuestion($"Your last workout was {minutes} {dayStr} ago. I'm not sure it makes sense to work out again today... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                    }
                }

                if (!IsInserted)
                {
                    summaryRestModel.SinceTime = $"N/A";
                    summaryRestModel.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                    summaryRestModel.TrainRest = "Train";
                    _isAnyWorkoutFinished = false;
                    summaryRestModel.StrengthTextColor = AppThemeConstants.GreenColor;
                    summaryRestModel.TrainRestText = "Coach says";// "Recovered";// "No workout yet".ToLower().FirstCharToUpper();
                    if (index == 0 || index >= BotList.Count)
                        BotList.Add(summaryRestModel);
                    else
                        BotList.Insert(index, summaryRestModel);
                    //BotList.Add(summaryRestModel);

                }

            }
            else
            {
                RequiredHours = 18;
                if (workouts != null && workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram.IsSystemExercise)
                {
                    if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bodyweight") ||
workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("mobility") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("powerlifting") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("full-body") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bands"))
                    {
                        RequiredHours = 42;
                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                        {
                            if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) < 30)
                                RequiredHours = 18;
                        }
                    }
                    else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("split"))
                    {
                        RequiredHours = 18;
                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                        {
                            if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) > 50)
                                RequiredHours = 42;
                        }
                    }
                }
                //summaryRestModel.TrainRest = $"{RequiredHours} hours";
                summaryRestModel.StrengthTextColor = AppThemeConstants.DarkRedColor;
                //summaryRestModel.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();

                //summaryRestModel.TrainRest = $"{RequiredHours} hours";
                summaryRestModel.StrengthTextColor = AppThemeConstants.DarkRedColor;
                //summaryRestModel.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();
                summaryRestModel.TrainRest = "Rest";
                summaryRestModel.SinceTime = $"{RequiredHours} hours";
                summaryRestModel.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                _isAnyWorkoutFinished = true;
                summaryRestModel.TrainRestText = "Coach says";// "Fatigued";// $"At least {RequiredHours} hours more to recover".ToLower().FirstCharToUpper();


                BotList.Add(summaryRestModel);
            }
            if (summaryRestModel.TrainRest == "Rest")
            {
                //Set Reminder email if enabled
                if (LocalDBManager.Instance.GetDBSetting("IsEmailReminder") == null)
                    LocalDBManager.Instance.SetDBSetting("IsEmailReminder", "true");
                if (RequiredHours-hours>0)
                {
                    //need to - reminder hours
                    var beforeHour = 0;
                    try
                    {
                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("ReminderHours")?.Value))
                            beforeHour = int.Parse(LocalDBManager.Instance.GetDBSetting("ReminderHours")?.Value);
                    }
                    catch (Exception ex)
                    {

                    }
                    if (LocalDBManager.Instance.GetDBSetting("IsEmailReminder")?.Value == "true")
                    {
                        try
                        {

                            //Set Local Notification
                            var dt1 = DateTime.Now.AddHours(RequiredHours - hours - beforeHour);
                            var timeSpan = dt1 - DateTime.Now;

                            //var timeSpan = new TimeSpan(dt);
                            var workoutName = $"{workouts?.GetUserProgramInfoResponseModel?.NextWorkoutTemplate?.Label}";
                            var weekStreak = "";
                            if (workouts.ConsecutiveWeeks != null && workouts.ConsecutiveWeeks.Count > 0)
                            {
                                var lastTime = workouts.ConsecutiveWeeks.Last();
                                var year = Convert.ToString(lastTime.MaxWeek).Substring(0, 4);
                                var weekOfYear = Convert.ToString(lastTime.MaxWeek).Substring(4, 2);
                                CultureInfo myCI = new CultureInfo("en-US");
                                Calendar cal = myCI.Calendar;

                                if (int.Parse(year) == DateTime.Now.Year)
                                {
                                    var currentWeekOfYear = cal.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                                    if (int.Parse(weekOfYear) >= currentWeekOfYear)
                                    {
                                        if (lastTime.ConsecutiveWeeks > 0)
                                        {
                                            weekStreak = $"—{lastTime.ConsecutiveWeeks}-week streak!";
                                        }
                                    }

                                    else if (int.Parse(weekOfYear) == currentWeekOfYear - 1)
                                    {
                                        if (lastTime.ConsecutiveWeeks > 0)
                                        {
                                            weekStreak = $"—{lastTime.ConsecutiveWeeks}-week streak!";
                                        }
                                    }
                                }
                            }
                            timeSpan = TimeSpan.FromHours(RequiredHours - hours - beforeHour) + DateTime.Now.TimeOfDay;
                            DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification($"Workout in {beforeHour} {(beforeHour<2 ? "hour" : "hours")}", $"{workoutName}{weekStreak}", timeSpan, 1122, NotificationInterval.Week);
                            
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                        DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1122);
                    var date = DateTime.Now;
                    if (workouts.LastWorkoutDate != null)
                    {
                        date = ((DateTime)workouts.LastWorkoutDate).ToLocalTime();
                    }
                        
                }
            }
            try
            {
                if (CurrentLog.Instance.IsWorkoutedOut )
                {
                    if (workouts != null)
                    {
                        if (workouts.Sets != null)
                        {
                            if (workouts.HistoryExerciseModel != null)
                            {
                                var exerciseModel = workouts.HistoryExerciseModel;

                                if (exerciseModel.TotalWorkoutCompleted > 0 && (exerciseModel.TotalWorkoutCompleted % 5 == 0 || exerciseModel.TotalWorkoutCompleted == 3))
                                {
                                    var inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                                    var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                                    var unit = inKg ? "kg" : "lbs";
                                    var _lifted = $"{weightLifted.ToString("N0")} {unit}";

                                    workoutCountForCongratulations = exerciseModel.TotalWorkoutCompleted;
                                    liftedForCongratulations = _lifted;
                                    if (isHowWasSelected) {

                                        var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                                        var modalPage = new Views.CongratulationsPopup("FirstWorkout.png", $"{workoutCountForCongratulations} workouts done!", $"Congrats on lifting {liftedForCongratulations}. Enjoying Dr. Muscle?", "Continue");
                                        modalPage.Disappearing += (sender2, e2) =>
                                        {
                                            waitHandle.Set();
                                        };
                                        await PopupNavigation.Instance.PushAsync(modalPage);
                                        await Task.Run(() => waitHandle.WaitOne());
                                        //var aiCard = BotList.Where(x => x.Type == BotType.AICard).FirstOrDefault();
                                        if (!isAIPopupShown && isAILoaded) {
                                            isAIPopupShown = true;
                                            //var modalPage2 = new Views.GeneralPopup("TrueState.png", AITitle, AILoadedText, "Continue");
                                            //await PopupNavigation.Instance.PushAsync(modalPage2);
                                        }


                                    } else
                                    {
                                        isWorkoutLoaded = true;
                                    }

                                    //exerciseModel.TotalWeight} [lbs/kg]
                                    //ConfirmConfig ShowsharePopUp = new ConfirmConfig()
                                    //{
                                    //    Message = $"You've lifted {exerciseModel.TotalWorkoutCompleted} times for a total of {_lifted}! Share your stats?",
                                    //    Title = "Congratulations!",
                                    //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    //    OkText = "Share stats",
                                    //    CancelText = "Maybe later",
                                    //    //OnAction = async (bool ok) =>
                                    //    //{
                                    //    //    if (ok)
                                    //    //    {

                                    //    //    }
                                    //    //    else
                                    //    //    {
                                    //    //        await PagesFactory.PopAsync();
                                    //    //    }
                                    //    //}
                                    //};
                                    //var isShare = await UserDialogs.Instance.ConfirmAsync(ShowsharePopUp);
                                    //if (isShare)
                                    //{
                                    //    var firstname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
                                    //    if (Device.RuntimePlatform.Equals(Device.Android))
                                    //    {

                                    //        await Xamarin.Essentials.Share.RequestAsync(new Xamarin.Essentials.ShareTextRequest
                                    //        {
                                    //            Uri = $"https://dr-muscle.com",
                                    //            Subject = $"{firstname} has lifted {exerciseModel.TotalWorkoutCompleted} times for a total of {_lifted}!"
                                    //        });
                                    //    }
                                    //    else
                                    //        await Xamarin.Essentials.Share.RequestAsync($"{firstname} has lifted {exerciseModel.TotalWorkoutCompleted} times for a total of {_lifted}\nhttps://dr-muscle.com");

                                    //    //Sharing code here
                                    //}

                                    //await UserDialogs.Instance.AlertAsync($"You've worked out {exerciseModel.TotalWorkoutCompleted + 1} times!", AppResources.Congratulations,"Ok");
                                    App.IsCongratulated = true;
                                      
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            //FetchMainData();
            //GotIt_Clicked(new DrMuscleButton() { Text = "" }, EventArgs.Empty);
            QuickStatsAutoscrollOff(new DrMuscleButton() { Text = "" }, EventArgs.Empty);

            
            //var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralSummaryAction, ToolbarItemOrder.Primary, 0);
            //this.ToolbarItems.Add(generalToolbarItem);

        }

        void UpdatedMassunitWorkout()
        {
            this.ToolbarItems.Clear();
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralHomeAction, ToolbarItemOrder.Primary, 0);
            //this.ToolbarItems.Add(generalToolbarItem);

            StartSetup();
        }
        async void loadChangedWorkout()
        {
            //Popup to root
            ((App)Application.Current).UserWorkoutContexts.workouts = null;
            ((App)Application.Current).UserWorkoutContexts.SaveContexts();
            
            if (_isFirstDemoOpen)
                await FetchMainDataWithLoader();
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                await Task.Delay(300);
                await Navigation.PopToRootAsync(false);
            }
            StartSetup();
        }
        async void UpdatedWorkout()
        {
            this.ToolbarItems.Clear();
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralHomeAction, ToolbarItemOrder.Primary, 0);
            //this.ToolbarItems.Add(generalToolbarItem);
            ((App)Application.Current).UserWorkoutContexts.workouts = null;
            ((App)Application.Current).UserWorkoutContexts.SaveContexts();
            var modalPage = new Views.GeneralPopup("Lamp.png","","","");
            if (PagesFactory.GetNavigation().Navigation.NavigationStack.Last() is MainAIPage)
            {
                _tipsArray = GetTipsArray();

                //Config.ShowTipsNumber
                if (Config.ShowTipsNumber >= _tipsArray.Count)
                    Config.ShowTipsNumber = 0;
                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                 modalPage = new Views.GeneralPopup("Lamp.png", $"{_tipsArray[Config.ShowTipsNumber][0]}", $"{_tipsArray[Config.ShowTipsNumber][1]}", "Continue", null, true, false, _tipsArray[Config.ShowTipsNumber][2], _tipsArray[Config.ShowTipsNumber][3], "false", "false");
                Config.ShowTipsNumber += 1;
                PopupNavigation.Instance.PushAsync(modalPage);
            }
            
                await FetchMainData();
                await WorkoutLogSets();
            if (PopupNavigation.Instance.PopupStack.Count() > 0)
                PopupNavigation.Instance.PopAllAsync();
                StartSetup();
            
        }

        async Task UpdatedAndReload()
        {

            ((App)Application.Current).UserWorkoutContexts.workouts = null;
            ((App)Application.Current).UserWorkoutContexts.SaveContexts();

            _isReload = true;
            await StartSetup();

            _lblWorkoutName.Text = "";
            //await Task.Delay(300);
            App.IsOnboarding = true;
            _lblWorkoutName.Text = "";
            OnAppearing();
            _isReload = false;
        }

        async void
            ReconfigrationWorkout()
        {

            this.ToolbarItems.Clear();
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralHomeAction, ToolbarItemOrder.Primary, 0);
            //this.ToolbarItems.Add(generalToolbarItem);
            ((App)Application.Current).UserWorkoutContexts.workouts = null;
            ((App)Application.Current).UserWorkoutContexts.SaveContexts();
            await StartSetup();
            Xamarin.Forms.MessagingCenter.Send<BodyweightUpdateMessage>(new BodyweightUpdateMessage() { }, "BodyweightUpdateMessage");
        }

        void Current_ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            CheckReachability();
        }


        private async void CheckReachability()
        {
            if (CrossConnectivity.Current.IsConnected && Device.RuntimePlatform.Equals(Device.iOS))
            {
                var remote = await CrossConnectivity.Current.IsReachable("https://www.google.com/");
                if (remote == false)
                    CrossToastPopUp.Current.ShowToastMessage("Limited data connectivity");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //#if DEBUG
            //            LocalDBManager.Instance.SetDBSetting("IsSubscribedInApp", "true");
            //#endif
            //var dt = DateTime.Now.AddMinutes(2);
            //var timeSpan = new TimeSpan(0, dt.Hour, dt.Minute, 0);
            // DateTime.Now.AddMinutes(2) - DateTime.Now;////

            //var modalPage1 = new Views.WorkoutGeneralPopup("Medal.png", "Nice work!", "X exercises in XX minutes", "Good", null, false, true);
            //PopupNavigation.Instance.PushAsync(modalPage1);

            try
            {
                MessagingCenter.Subscribe<ReceivedWatchMessage>(this, "ReceivedWatchMessage", (obj) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (obj.PhoneToWatchModel.WatchMessageType == WatchMessageType.StartWorkout)
                            StartTodaysWorkout();
                    });
                });
                LoadSavedWeights();
                LoadWorkedOutDays();
                LoadUpcomingDays();
                if (!_isFirstDemoOpen && Device.RuntimePlatform.Equals(Device.iOS))
                {
                    if ((LocalDBManager.Instance.GetDBSetting("IsFirstMessage") != null && LocalDBManager.Instance.GetDBSetting("IsFirstMessage").Value == "First1") || (LocalDBManager.Instance.GetDBSetting("IsFirstMessageSend") != null && LocalDBManager.Instance.GetDBSetting("IsFirstMessageSend").Value == "First1"))
                    {
                        //LocalDBManage r.Instance.SetDBSetting("IsFirstMessage", null);
                        LocalDBManager.Instance.SetDBSetting("IsSecondMessage", "Second1");
                        ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Tabs[0].BadgeCaption = 1;
                    }
                }
                try
                {
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
                    if (_isReconfigured)
                    {
                        CurrentLog.Instance.IsReconfigration = false;
                        _isReconfigured = false;
                        //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                        //{
                        //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        //    Title = "Success!",
                        //    Message = "New settings saved.",
                        //    OkText = AppResources.Ok
                        //});

                        var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                        var modalPage = new Views.GeneralPopup("TrueState.png", "Success!", "New settings saved", "Start workout");
                        modalPage.Disappearing += (sender2, e2) =>
                        {
                            waitHandle.Set();
                        };
                        await PopupNavigation.Instance.PushAsync(modalPage);

                        await Task.Run(() => waitHandle.WaitOne());
                    }
                }
                catch (Exception ex)
                {

                }
                try
                {
                    var result = "";
                    int lowReps = 0;
                    int highreps = 0;
                    try
                    {
                        lowReps = int.Parse(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                        highreps = int.Parse(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                    }
                    catch (Exception)
                    {

                    }
                    var result1 = "";
                    if (lowReps >= 5 && highreps <= 12)
                    {
                        result = "Build muscle and strength";
                        result1 = "building muscle and strength";
                    }
                    else if (lowReps >= 8 && highreps <= 15)
                    {
                        result = "Build muscle and burn fat";
                        result1 = "building muscle and burn fat";
                    }
                    else if (lowReps >= 5 && highreps <= 15)
                    {
                        result = "Build muscle";
                        result1 = "building muscle";
                    }
                    else if (lowReps >= 12 && highreps <= 20)
                    {
                        result = "Burn fat";
                        result1 = "burning fat";
                    }
                    else if (highreps >= 16)
                    {
                        result = "Build muscle and burn fat";
                        result1 = "building muscle and burning fat";
                    }
                    else
                    {
                        if (LocalDBManager.Instance.GetDBSetting("Demoreprange") != null)
                        {
                            if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscle")
                            {
                                result = "Build muscle";
                                result1 = "building muscle";
                            }
                            else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscleBurnFat")
                            {
                                result = "Build muscle and burn fat";
                                result1 = "building muscle and burning fat";
                            }
                            else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                            {
                                result = "Burn fat";
                                result1 = "burning fat";
                            }
                        }
                    }
                    LblLearnGoalTitle.Text = $"{result} guide";
                    LblLearnGoal.Text = $"The Dr. Muscle 1-min guide to {result1}.";

                    LblLearnGoalTitle2.Text = $"{result} guide";
                    LblLearnGoal2.Text = $"The Dr. Muscle 1-min guide to {result1}.";

                    LblWeightGoal = $"Track your weight to get custom tip to {result.ToLower()}.";


                    if (App.IsOnboarding)
                    {
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
                        WelcomeBox.IsVisible = _isFirstDemoOpen;
                        GoalBox.IsVisible = _isFirstDemoOpen;

                        GoalBox2.IsVisible = _isFirstDemoOpen;
                        if (_isFirstDemoOpen && !CurrentLog.Instance.IsWorkoutedOut)
                        {
                            LblGoal.Text = $"This is where you'll find your workouts, key stats, and everything you need to {text.ToLowerInvariant()}.";
                        }
                        App.IsOnboarding = false;
                        //XX workouts / week
                        var workoutname = "";
                        var count = 3;
                        int age = -1, xDays = 3;
                        if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                        {

                            workoutname = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value;

                            if (LocalDBManager.Instance.GetDBSetting("Age") != null && LocalDBManager.Instance.GetDBSetting("Age").Value != null)
                            {
                                age = int.Parse(LocalDBManager.Instance.GetDBSetting("Age").Value);
                            }
                            if (age != -1)
                            {
                                if (LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("push/pull/legs"))
                                {
                                    xDays = 6;
                                }
                                else if (LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("split"))
                                {
                                    if (age < 30)
                                        xDays = 4;
                                    else if (age >= 30 && age <= 50)
                                        xDays = 4;
                                    else
                                        xDays = 3;
                                }
                                else
                                {
                                    if (age < 30)
                                        xDays = 4;
                                    else if (age >= 30 && age <= 50)
                                        xDays = 3;
                                    else
                                        xDays = 2;
                                }
                            }
                        }
                        //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                        //{
                        //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        //    Title = App.workoutPerDay>0 ? $"{App.workoutPerDay} workouts / week" : $"{xDays} workouts / week",
                        //    Message = $"Because you're {LocalDBManager.Instance.GetDBSetting("Age").Value} and you're {LocalDBManager.Instance.GetDBSetting("CustomExperience").Value}. ",
                        //    OkText = AppResources.GotIt
                        //});

                        _isAllDone = true;
                        //CurrentLog.Instance.WalkThroughCustomTipsPopup = true;
                        ////await PagesFactory.PushAsync<LearnPage>();
                        //App.IsLearnPopup = true;
                        //((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[2];
                        MessagingCenter.Send<BodyweightUpdateMessage>(new BodyweightUpdateMessage(), "BodyweightUpdateMessage");
                        //return;

                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    if (LocalDBManager.Instance.GetDBSetting($"AnySets{DateTime.Now.Date}")?.Value
                         == "1")
                    {
                        var text = BtnCardStartWorkout.Text;

                        text = text.Replace("START WORKOUT", "RESUME WORKOUT");
                        BtnCardStartWorkout.Text = text.Replace("PREVIEW NEXT WORKOUT", "RESUME WORKOUT");
                        BtnWelcomeStartWorkout.Text = BtnCardStartWorkout.Text;
                        btnstsrtWorkoutTitle.Text = BtnCardStartWorkout.Text;

                        var text1 = btnStartWorkout?.Text;
                        text1 = text1?.Replace("START WORKOUT", "RESUME WORKOUT");
                        if (btnStartWorkout != null)
                            btnStartWorkout.Text = text1?.Replace("PREVIEW NEXT WORKOUT", "RESUME WORKOUT");
                    }

                }
                catch (Exception ex)
                {

                }

                try
                {
                    if (_isAllDone)
                    {
                        _isAllDone = false;
                        var workoutname = "";
                        var count = 3;
                        if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                        {
                            workoutname = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value;
                        }

                        //AlertConfig UpdatedPopUp = new AlertConfig()
                        //{
                        //    Message = "Unlock 22 smart features.",
                        //    Title = "Smart workout ready",
                        //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        //    OkText = "Unlock"

                        //};
                        //await Task.Delay(1000);
                        // await UserDialogs.Instance.AlertAsync(UpdatedPopUp);
                        CurrentLog.Instance.IsWalkthrough = true;
                        //if (r)
                        //{
                        //CurrentLog.Instance.CurrentExercise = new ExerciceModel()
                        //{
                        //    BodyPartId = 8,
                        //    VideoUrl = "https://youtu.be/R2dMsNhN3DE",
                        //    IsBodyweight = false,
                        //    IsEasy = false,
                        //    IsFinished = false,
                        //    IsMedium = false,
                        //    IsNextExercise = false,
                        //    IsNormalSets = false,
                        //    IsSwapTarget = false,
                        //    IsSystemExercise = true,
                        //    IsTimeBased = false,
                        //    IsUnilateral = false,
                        //    Label = "Squat",
                        //    RepsMaxValue = null,
                        //    RepsMinValue = null,
                        //    Timer = null,
                        //    EquipmentId = 3,
                        //    Id = 12
                        //};
                        //PagesFactory.PushAsync<CustomDemo>();
                        //}
                        //else
                        //{
                        //    TooltipEffect.SetHasShowTooltip((Xamarin.Forms.Grid)btnStartWorkout.Parent, true);
                        //    ShouldAnimate = true;
                        //    animate((Xamarin.Forms.Grid)btnStartWorkout.Parent);

                        //}
                    }
                }
                catch (Exception ex)
                {

                }

             
                try
                {
                    if (CurrentLog.Instance.IsWalkthrough)
                    {
                        CurrentLog.Instance.IsWalkthrough = false;
                        //AlertConfig UpdatedPopUp = new AlertConfig()
                        //{
                        //    Message = "Unlock 22 smart features.",
                        //    Title = "Smart workout ready",
                        //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        //    OkText = "Unlock"

                        //};

                        if (btnStartWorkout != null)
                            TooltipEffect.SetHasShowTooltip((Xamarin.Forms.Grid)btnStartWorkout.Parent, true);
                        _lblWorkoutName.Text = "";
                        ShouldAnimate = true;
                        animate((Xamarin.Forms.Grid)btnStartWorkout.Parent);

                        //await UserDialogs.Instance.AlertAsync(UpdatedPopUp);
                        if (App.IsSleeping)
                            return;
                        var r = true;


                        if (r)
                        {
                            //try
                            //{
                            //    if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscle")
                            //    {
                            //        text = "Build muscle";
                            //    }
                            //    else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscleBurnFat")
                            //    {
                            //        text = "Build muscle and burn fat";
                            //    }
                            //    else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                            //    {
                            //        text = "Burn fat";
                            //    }
                            //}
                            //catch (Exception ex)
                            //{

                            //}
                            //var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                            //var massUnit = isKg ? "kg" : "lbs";
                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Message = "All settings fine tuned for your goals.",
                            //    Title = $"{text}",
                            //    OkText = AppResources.GotIt
                            //});

                            //AlertConfig ShowRIRPopUp = new AlertConfig()
                            //{
                            //    Title = "Smart weights, reps, and sets",
                            //    Message = "Adjusted automatically every workout based on your goals and progress.",
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    OkText = "Got it",
                            //};
                            //await UserDialogs.Instance.AlertAsync(ShowRIRPopUp);

                            //AlertConfig ShowExplainRIRPopUp = new AlertConfig()
                            //{
                            //    Title = "Smart challenges",
                            //    Message = "Trigger when you're on a roll to speed up your gains.",
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    OkText = "Got it",

                            //};

                            //await UserDialogs.Instance.AlertAsync(ShowExplainRIRPopUp);

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Daily undulating periodization",
                            //    Message = $"You progress faster when you change reps often. Your reps change (undulate) automatically every workout.",
                            //    OkText = AppResources.GotIt
                            //});



                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Exercise rotation",
                            //    Message = $"You get new exercises automatically over time—your program rotates them for you.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Overtraining protection",
                            //    Message = $"Your workouts becomes easier automatically when you show signs of overtraining.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Recovery coach",
                            //    Message = $"How long should you rest before your next workout? The app tells you on your home page.",
                            //    OkText = AppResources.GotIt
                            //});


                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Nutrition coach",
                            //    Message = $"How many calories and protein should you eat for your goals and current progress? The app tells you on the Learn tab.",
                            //    OkText = AppResources.GotIt
                            //});


                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Multiple equipment profiles",
                            //    Message = $"Training at home and on the road? You can set up multiple equipment profiles in Settings.",
                            //    OkText = AppResources.GotIt
                            //});


                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Pyramid sets",
                            //    Message = $"\"The most reliable and effective technique I’ve ever come across.\" -Martin Berkhan",
                            //    OkText = AppResources.GotIt
                            //});


                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Detailed tracking",
                            //    Message = $"Track all your stats inside the app. Get more details inside our Web app.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Favorite exercises",
                            //    Message = "Favorite an exercise to see it more often.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Plate calculator",
                            //    Message = $"Tap the plate icon (top-right) to see how to load barbell exercises. No more plate math.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Light sessions",
                            //    Message = $"Got off track? No problem: when you return after a break, you get a light session automatically.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Sets redistribution",
                            //    Message = $"Skip a workout? No problem: some of your sets are redistributed to your next workout automatically.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Strenth phase",
                            //    Message = $"Optional, automated 3-week strength phase on each program.",
                            //    OkText = AppResources.GotIt
                            //});

                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Suggest and vote",
                            //    Message = $"Something missing? Suggest and vote for new features on our feedback site.",
                            //    OkText = AppResources.GotIt
                            //});

                            //var isFull = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"And more!",
                            //    Message = $"New updates every week. View the full list?",
                            //    OkText = "Full list",
                            //    CancelText = AppResources.Skip
                            //});

                            //if (isFull)
                            //{
                            //    Device.OpenUri(new Uri("https://dr-muscle.com/discount"));
                            //    await Task.Delay(500);
                            //}

                            //await Task.Delay(500);

                            //TooltipEffect.SetHasShowTooltip((Xamarin.Forms.Grid)btnStartWorkout.Parent, true);
                            //_lblWorkoutName.Text = "";
                            //ShouldAnimate = true;
                            //animate((Xamarin.Forms.Grid)btnStartWorkout.Parent);



                        }
                        else
                        {

                            TooltipEffect.SetHasShowTooltip((Xamarin.Forms.Grid)btnStartWorkout.Parent, true);
                            _lblWorkoutName.Text = "";
                            ShouldAnimate = true;
                            animate((Xamarin.Forms.Grid)btnStartWorkout.Parent);
                            //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Title = $"Start workout {workoutname}",
                            //    Message = $"To start, tap Start {workoutname} at the bottom.",
                            //    OkText = AppResources.GotIt
                            //});
                        }

                    }
                }
                catch (Exception ex)
                {

                }

                try
                {
                    if (LocalDBManager.Instance.GetDBSetting("firstname") == null)
                        return;

                    if (LocalDBManager.Instance.GetDBSetting("VCode") == null)
                    {
                        if (Device.RuntimePlatform.Equals(Device.Android))
                            LocalDBManager.Instance.SetDBSetting("VCode", VersionTracking.CurrentBuild);
                        else
                            LocalDBManager.Instance.SetDBSetting("VCode", VersionTracking.CurrentVersion.Substring(4));
                        var newVersion = float.Parse(Device.RuntimePlatform.Equals(Device.Android) ? VersionTracking.CurrentBuild : VersionTracking.CurrentVersion.Substring(4));
                        
                    }
                    else
                    {
                        var oldVersion = float.Parse(LocalDBManager.Instance.GetDBSetting("VCode").Value);
                        if (Device.RuntimePlatform.Equals(Device.Android))
                            LocalDBManager.Instance.SetDBSetting("VCode", VersionTracking.CurrentBuild);
                        else
                            LocalDBManager.Instance.SetDBSetting("VCode", VersionTracking.CurrentVersion.Substring(4));
                        var newVersion = float.Parse(Device.RuntimePlatform.Equals(Device.Android) ? VersionTracking.CurrentBuild : VersionTracking.CurrentVersion.Substring(4));
                        
                        if (newVersion > oldVersion)
                        {
                            //App updated code here


                            //var waitHandle2 = new EventWaitHandle(false, EventResetMode.AutoReset);
                            
                            var modalPage2 = new Views.GeneralPopup("TrueState.png", "October update 2!", "✓ Assisted Exercises\n✓ Cleaner workout UI\n✓ Smarter AI chat\n⮕ Soon: New programs (PPL 7)\n", "Continue", null, false, false, "false", "false", "false", "false", "false", "true");



                            //modalPage2.Disappearing += (sender2, e2) =>
                            //{
                            //    waitHandle2.Set();
                            //};
                            await PopupNavigation.Instance.PushAsync(modalPage2);

                            //await Task.Run(() => waitHandle2.WaitOne());

                            //Old Popup
                            //ConfirmConfig UpdatedPopUp = new ConfirmConfig()
                            //{
                            //    Message = "Open release notes?",
                            //    Title = "Update successful",
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    OkText = "Open notes",
                            //    CancelText = AppResources.Cancel,
                                
                            //};


                            //await Task.Delay(100);
                            //if (App.IsSleeping)
                            //    return;
                            //var r = await UserDialogs.Instance.ConfirmAsync(UpdatedPopUp);
                            //if (r)
                            //    Device.OpenUri(new Uri("https://dr-muscle.com/timeline"));
                        }
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

                if (LocalDBManager.Instance.GetDBSetting("email") == null)
                    return;

                try
                {
                    if (Device.RuntimePlatform.Equals(Device.iOS))
                    {
                        IHealthData _healthService = DependencyService.Get<IHealthData>();
                        await _healthService.GetWeightPermissionAsync(async (r) =>
                        {
                            var a = r;
                            if (r)
                            {
                                _healthService.FetchWeight(async (double obj) => {
                                    try
                                    {
                                        Device.BeginInvokeOnMainThread(async () =>
                                        {
                                            var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                                            var weights = new MultiUnityWeight(value, "kg");

                                            var fetchvalue = obj;
                                            var fetchweights = new MultiUnityWeight((decimal)obj, LocalDBManager.Instance.GetDBSetting("massunit").Value);

                                            if (LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg")
                                            {
                                                if ((double)weights.Kg != obj)
                                                {
                                                    //Update body weight
                                                    LocalDBManager.Instance.SetDBSetting("BodyWeight", fetchweights.Kg.ToString().Replace(",", "."));

                                                    LblBodyweight.Text = string.Format("{0:0.##}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? fetchweights.Kg : fetchweights.Lb);
                                                   
                                                }
                                            }
                                            else
                                            {
                                                if (Math.Round((double)weights.Lb, 2) != obj)
                                                {
                                                    //Update body weight
                                                    LocalDBManager.Instance.SetDBSetting("BodyWeight", fetchweights.Kg.ToString().Replace(",", "."));

                                                    LblBodyweight.Text = string.Format("{0:0.##}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? fetchweights.Kg : fetchweights.Lb);
                                                   

                                                }
                                            }
                                        });
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                });
                            }
                        });
                    }
                }
                catch (Exception ex)
                {

                }

                if (BotList.Count == 0)
                {
                    StartSetup();
                    //LoadAB();
                }
                else
                {
                    if (string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("firstname").Value))
                        GetFirstName();
                }

                
               
                if (Device.RuntimePlatform.Equals(Device.iOS) && CrossConnectivity.Current.IsConnected)
                    CheckVersion();

            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<Message.ReceivedWatchMessage>(this, "ReceivedWatchMessage");
        }
        public async void CheckVersion()
        {
            try
            {

                bool isLatest = await CrossLatestVersion.Current.IsUsingLatestVersion();
                var no = await CrossLatestVersion.Current.GetLatestVersionNumber();
                if (isLatest == false)
                {
                    AskToUpdateApp();
                }

            }
            catch (Exception ex)
            {

            }
        }

        public async void AskToUpdateApp()
        {

            if (App.IsAskedLatestVersion)
                return;
            App.IsAskedLatestVersion = true;
            ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
            {
                Message = "A new update is available. Update now?",
                Title = "New update!",
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = "Update",
                CancelText = AppResources.RemindMe,
                OnAction = async (bool ok) =>
                {
                    if (ok)
                    {
                        //CrossLatestVersion.Current.CountryCode = new Xamarin.Essentials.Locale();
                        //await CrossLatestVersion.Current.OpenAppInStore(); 
                        //
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                            Device.OpenUri(new Uri("itms-apps://itunes.apple.com/app/id1073943857"));
                        else
                            await CrossLatestVersion.Current.OpenAppInStore();

                    }
                    else
                    {

                    }
                }
            };
            await Task.Delay(100);
            UserDialogs.Instance.Confirm(ShowWelcomePopUp2);
        }

        private IEnumerable<EventModel> GenerateEvents(long count, string name, bool isSystemExercise=false, bool past = false, DateTime? dateTime = null)
        {
            return Enumerable.Range(1, 1).Select(x => new EventModel
            {
                Name = $"{name}",
                Description = $"This is {name} event{x}'s description!",
                Id = count,
                IsPast = past,
                IsSystemExercise = isSystemExercise,
                Date = dateTime
            });
        }
        private void GetFirstName()
        {
            PromptConfig p = new PromptConfig()
            {
                InputType = InputType.Default,
                IsCancellable = false,
                Title = "Your first name",
                Placeholder = "Enter first name",
                OkText = AppResources.Continue,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = new Action<PromptResult>(GetFirstNameAction)
            };
            p.OnTextChanged += Name_OnTextChanged;
            UserDialogs.Instance.Prompt(p);
        }
        private async void GetFirstNameAction(PromptResult response)
        {
            if (response.Ok)
            {
                if (string.IsNullOrEmpty(response.Text))
                {
                    GetFirstName();
                    return;
                }
                await AddAnswer(response.Text);
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                }
                LocalDBManager.Instance.SetDBSetting("firstname", response.Text);
            }
        }

        public async Task FetchMainDataWithSets()
        {
            try
            {

                if (CrossConnectivity.Current.IsConnected)
                {
                    TimeZoneInfo local = TimeZoneInfo.Local;
                    if (workoutLogAverage != null)
                    {
                        var userProgram = await DrMuscleRestClient.Instance.GetUserWorkoutProgramTimeZoneInfoWithoutLoader(local);
                        if (userProgram != null)
                        {
                            workoutLogAverage.GetUserProgramInfoResponseModel = userProgram.GetUserProgramInfoResponseModel;
                            workoutLogAverage.LastWorkoutDate = userProgram.LastWorkoutDate;
                            workoutLogAverage.LastConsecutiveWorkoutDays = userProgram.LastConsecutiveWorkoutDays;
                        }
                    }
                    else
                        workoutLogAverage = await DrMuscleRestClient.Instance.GetUserWorkoutProgramTimeZoneInfo(local);
                    if (LocalDBManager.Instance.GetDBSetting("email") == null)
                        return;
                    if (workoutLogAverage != null)
                    {
                        if (workoutLogAverage.GetUserProgramInfoResponseModel != null)
                        {
                            upi = workoutLogAverage.GetUserProgramInfoResponseModel;
                            if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                            {
                                LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                            }

                        }
                        ((App)Application.Current).UserWorkoutContexts.workouts = workoutLogAverage;

                        ((App)Application.Current).UserWorkoutContexts.SaveContexts();

                        if (workoutLogAverage != null && workoutLogAverage.GetUserProgramInfoResponseModel != null)
                        {
                            if (workoutLogAverage.GetUserProgramInfoResponseModel.RecommendedProgram == null && workoutLogAverage.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
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
                                            NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value, IsSystemExercise = true },
                                            RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                                        };
                                        workoutLogAverage.GetUserProgramInfoResponseModel = upi;
                                        ((App)Application.Current).UserWorkoutContexts.workouts = workoutLogAverage;
                                        ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                                        LoadUpcomingDays();
                                        //lblProgram.Text = $"{AppResources.Program}: {upi.RecommendedProgram.Label}";
                                        //lblWorkout.Text = $"{AppResources.UpNext}: {upi.NextWorkoutTemplate.Label}";
                                        //WorkoutNowbutton.Text = $"{AppResources.StartCapitalized} {upi.NextWorkoutTemplate.Label}";
                                        LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                                        LoadCurrentWorkoutTemplate();
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                }
                            }
                        }
                    }
                    if (!App.IsV1User)
                    {
                        await CanGoFurther();
                    }

                    await WorkoutLogSets();
                }


            }
            catch (Exception ex)
            {

            }
        }
        public async Task FetchMainData()
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    TimeZoneInfo local = TimeZoneInfo.Local;
                    if (workoutLogAverage != null)
                    {
                        var userProgram = await DrMuscleRestClient.Instance.GetUserWorkoutProgramTimeZoneInfoWithoutLoader(local);
                        if (userProgram != null)
                        {

                            if (Device.RuntimePlatform.Equals(Device.Android))
                            {
                                if (workoutLogAverage.LatestVersionCode > DependencyService.Get<IVersionInfoService>().GetVersionInfo())
                                    AskToUpdateApp();
                            }
                            workoutLogAverage.GetUserProgramInfoResponseModel = userProgram.GetUserProgramInfoResponseModel;
                            workoutLogAverage.LastWorkoutDate = userProgram.LastWorkoutDate;
                            workoutLogAverage.LastConsecutiveWorkoutDays = userProgram.LastConsecutiveWorkoutDays;
                        }
                    }
                    else
                        workoutLogAverage = await DrMuscleRestClient.Instance.GetUserWorkoutProgramTimeZoneInfo(local);
                    if (LocalDBManager.Instance.GetDBSetting("email") == null)
                    {
                        workoutLogAverage = null;
                        return;
                    }
                    if (workoutLogAverage != null)
                    {
                        if (workoutLogAverage.GetUserProgramInfoResponseModel != null)
                        {
                            upi = workoutLogAverage.GetUserProgramInfoResponseModel;
                            if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                            {
                                LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                            }

                        }
                        ((App)Application.Current).UserWorkoutContexts.workouts = workoutLogAverage;

                        ((App)Application.Current).UserWorkoutContexts.SaveContexts();

                        if (workoutLogAverage != null && workoutLogAverage.GetUserProgramInfoResponseModel != null)
                        {
                            if (workoutLogAverage.GetUserProgramInfoResponseModel.RecommendedProgram == null && workoutLogAverage.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
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
                                            NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value, IsSystemExercise = true },
                                            RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                                        };
                                        workoutLogAverage.GetUserProgramInfoResponseModel = upi;
                                        ((App)Application.Current).UserWorkoutContexts.workouts = workoutLogAverage;
                                        ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                                        LoadUpcomingDays();
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
                    }
                    //if (!App.IsV1User)
                    //{
                    //    CanGoFurtherWithoughtLoader();
                    //}
                    LoadUpcomingDays();
                    WorkoutLogSets();
                }


            }
            catch (Exception ex)
            {

            }
        }

        public async Task FetchMainDataWithLoader()
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    TimeZoneInfo local = TimeZoneInfo.Local;
                    if (workoutLogAverage != null)
                    {
                        var userProgram = await DrMuscleRestClient.Instance.GetUserWorkoutProgramTimeZoneInfo(local);
                        if (userProgram != null)
                        {
                            if (Device.RuntimePlatform.Equals(Device.Android))
                            {
                                if (workoutLogAverage.LatestVersionCode > DependencyService.Get<IVersionInfoService>().GetVersionInfo())
                                    AskToUpdateApp();
                            }
                            workoutLogAverage.GetUserProgramInfoResponseModel = userProgram.GetUserProgramInfoResponseModel;
                            workoutLogAverage.LastWorkoutDate = userProgram.LastWorkoutDate;
                            workoutLogAverage.LastConsecutiveWorkoutDays = userProgram.LastConsecutiveWorkoutDays;
                        }
                    }
                    else
                        workoutLogAverage = await DrMuscleRestClient.Instance.GetUserWorkoutProgramTimeZoneInfo(local);
                    if (LocalDBManager.Instance.GetDBSetting("email") == null)
                    {
                        workoutLogAverage = null;
                        return;
                    }
                    if (workoutLogAverage != null)
                    {
                        if (workoutLogAverage.GetUserProgramInfoResponseModel != null)
                        {
                            upi = workoutLogAverage.GetUserProgramInfoResponseModel;
                            if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                            {
                                LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                            }
                            SetBottomCard();
                        }
                        ((App)Application.Current).UserWorkoutContexts.workouts = workoutLogAverage;

                        ((App)Application.Current).UserWorkoutContexts.SaveContexts();

                        if (workoutLogAverage != null && workoutLogAverage.GetUserProgramInfoResponseModel != null)
                        {
                            if (workoutLogAverage.GetUserProgramInfoResponseModel.RecommendedProgram == null && workoutLogAverage.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
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
                                            NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value, IsSystemExercise = true },
                                            RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                                        };
                                        workoutLogAverage.GetUserProgramInfoResponseModel = upi;
                                        ((App)Application.Current).UserWorkoutContexts.workouts = workoutLogAverage;
                                        ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                                        LoadUpcomingDays();
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
                    }
                    //if (!App.IsV1User)
                    //{
                    //    CanGoFurtherWithoughtLoader();
                    //}
                    LoadUpcomingDays();
                    WorkoutLogSets();
                }


            }
            catch (Exception ex)
            {

            }
        }
        private async Task WorkoutLogSets(bool IsLoader = false)
        {
            try
            {
                if (workoutLogAverage != null)
                {
                    //GetLogAverageWithSets
                    var workLog = IsLoader ? await DrMuscleRestClient.Instance.GetLogAverageWithSets() : await DrMuscleRestClient.Instance.GetLogAverageWithSetsWithoughtLoader();
                    try
                    {
                        if (workoutLogAverage == null)
                            workoutLogAverage = new GetUserWorkoutLogAverageResponse();
                        workoutLogAverage.Sets = workLog.Sets;
                        workoutLogAverage.SetsDate = workLog.SetsDate;
                        workoutLogAverage.WorkoutCount = workLog.WorkoutCount;
                        workoutLogAverage.LastMonthWorkoutCount = workLog.LastMonthWorkoutCount;
                        workoutLogAverage.Averages = workLog.Averages;
                        workoutLogAverage.AverageExercises = workLog.AverageExercises;
                        workoutLogAverage.ExerciseCount = workLog.ExerciseCount;
                        workoutLogAverage.HistoryExerciseModel = workLog.HistoryExerciseModel;
                        workoutLogAverage.ConsecutiveWeeks = workLog.ConsecutiveWeeks;


                        if (LocalDBManager.Instance.GetDBSetting("email") == null)
                        {
                            ((App)Application.Current).UserWorkoutContexts.workouts = new GetUserWorkoutLogAverageResponse();
                            ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                            workoutLogAverage = null;
                            return;
                        }
                        ((App)Application.Current).UserWorkoutContexts.workouts = workoutLogAverage;
                        ((App)Application.Current).UserWorkoutContexts.SaveContexts();

                    }
                    catch (Exception ex)
                    {

                    }
                    try
                    {
                        if (_isJustAppOpen)
                        {
                            _isJustAppOpen = false;
                            //StartSetup();
                            if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null && btnStartWorkout != null)
                            {
                                var startworkoutText = "START WORKOUT";
                                var previewWorkout = "PREVIEW NEXT WORKOUT";
                                if (LocalDBManager.Instance.GetDBSetting($"AnySets{DateTime.Now.Date}")?.Value
                         == "1")
                                {
                                    previewWorkout = "RESUME WORKOUT";
                                    startworkoutText = "RESUME WORKOUT";
                                }

                                _lblWorkoutName.Text = _isReload ? "" : $"{upi.NextWorkoutTemplate.Label}";
                                workoutNameLabel.Text = $"{upi.NextWorkoutTemplate.Label} is next";
                                workoutNameLabel2.Text = workoutNameLabel.Text;
                                btnStartWorkout.Text = _isAnyWorkoutFinished ? $"{previewWorkout}\n{upi.NextWorkoutTemplate.Label}".ToUpperInvariant() : $"{startworkoutText}\n{upi.NextWorkoutTemplate.Label}".ToUpperInvariant();
                                BtnCardStartWorkout.Text = _isAnyWorkoutFinished ? $"{previewWorkout}" : $"{startworkoutText}";
                                BtnWelcomeStartWorkout.Text = BtnCardStartWorkout.Text;
                                btnstsrtWorkoutTitle.Text = BtnCardStartWorkout.Text;
                            }
                            else
                                btnStartWorkout.Text = "Start planned workout".ToUpper();
                        }
                        //if (!DeviceInfo.Name.Equals("iPhone SE"))
                        //{ 
                        //    List <HistoryModel> history = await DrMuscleRestClient.Instance.GetHistoryAllTimeWithoutLoader(new GetUserWorkoutLogAverageForExerciseRequest() {ExerciseId = null,PeriodSinceToday = -TimeSpan.FromDays(28) });
                        //    if (history != null)
                        //    {
                        //        ((App)Application.Current).WorkoutHistoryContextList.Histories = history;
                        //        ((App)Application.Current).WorkoutHistoryContextList.SaveContexts();
                        //    }
                        //}
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

        async void SubscriptioPurchased()
        {
            if (_isPurchasedOpen)
                return;
            _isPurchasedOpen = true;
            //ConfirmConfig UpdatedPopUp = new ConfirmConfig()
            //{

            //    Title = "Welcome to Dr. Muscle!",
            //    Message = "Your access is now unlocked. Start workout?",
            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //    OkText = "Start workout",
            //    CancelText = "Cancel",
            //};
            //await Task.Delay(100);
            //var result = await UserDialogs.Instance.ConfirmAsync(UpdatedPopUp);
            //LocalDBManager.Instance.SetDBSetting("IsSubscribedInApp", "true");
            //if (result)
            //{
            //    StartTodaysWorkout();
            //}

            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var modalPage = new Views.GeneralPopup("Medal.png", "Success!", "Subscription successful—welcome to Dr. Muscle 💪", "Start workout");
            modalPage.Disappearing += (sender2, e2) =>
            {
                waitHandle.Set();
            };
            await PopupNavigation.Instance.PushAsync(modalPage);

            await Task.Run(() => waitHandle.WaitOne());
            App.IsFreePlan = false;
            App.IsV1User = true;
            App.IsTraining = true;
            App.IsV1UserTrial = true;
            StartTodaysWorkout();
            _isPurchasedOpen = false;
        }

        async void LoadCurrentWorkoutTemplate()
        {
            try
            {

                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;

                var loadWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkoutWithoutLoader(workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Id);
                workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate = loadWorkout;
                ((App)Application.Current).UserWorkoutContexts.workouts = workouts;
                ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                SetBottomCard();
            }
            catch (Exception ex)
            {

            }
        }


        async void ReloadWorkout()
        {
            try
            {

                TimeZoneInfo local = TimeZoneInfo.Local;
                var wLog = await DrMuscleRestClient.Instance.GetUserWorkoutProgramTimeZoneInfoWithoutLoader(local);
                if (wLog != null && wLog.GetUserProgramInfoResponseModel != null && wLog.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                {
                    upi = wLog.GetUserProgramInfoResponseModel;
                    var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    workouts.GetUserProgramInfoResponseModel = upi;
                    ((App)Application.Current).UserWorkoutContexts.workouts = workouts;
                    ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                    SetBottomCard();
                }

            }
            catch (Exception ex)
            {

            }
        }

        async void CheckMonthlyUser()
        {
            try
            {
                if (CurrentLog.Instance.IsMonthlyUser == null)
                {
                    var result = await DrMuscleRestClient.Instance.IsMonthlyUser(true);
                    if (result != null)
                        CurrentLog.Instance.IsMonthlyUser = result.Result;
                }
            }
            catch (Exception ex)
            {

            }
        }

        async Task StartSetup() //Is Back is added to decide show loader or not
        {
            try
            {

                if (_isFirstDemoOpen)
                {
                    StackSteps1.IsVisible = true;
                    calendarBox1.Content = calendar;
                }
                else
                {
                    StackSteps1.IsVisible = false;
                    calendarBox2.Content = calendar;
                }
                StackSteps2.IsVisible = false;
                WelcomeBox.IsVisible = true;
                GoalBox.IsVisible = true;
                //WeightBox.IsVisible = false;
                WorkoutBox.IsVisible = true;
                WeightProgress1.IsVisible = true;
                WeightProgress2.IsVisible = true;
                strengthBox.IsVisible = true;
                mainScroll.IsVisible = true;
                GoalBox2.IsVisible = LocalDBManager.Instance.GetDBSetting("GoalBox2")?.Value == "true";
                volumeBox.IsVisible = true;
                WeightBox2.IsVisible = true;
                StateBox.IsVisible = true;
                StartworkoutBox.IsVisible = true;
                SecondProgressBox.IsVisible = false;

                try
                {
                    //show each state maximum 1x every 5 days
                    var ticksStrenth = LocalDBManager.Instance.GetDBSetting($"ExtraWorkoutAskedStrenth")?.Value;
                    var ticksRecover = LocalDBManager.Instance.GetDBSetting($"ExtraWorkoutAskedRecover")?.Value;
                    if (string.IsNullOrEmpty(ticksStrenth) && string.IsNullOrEmpty(ticksRecover))
                        SecondProgressBox.IsVisible = true;
                }
                catch (Exception)
                {
                }

                try
                {
                    if (Device.RuntimePlatform.Equals(Device.Android))
                    {
                        if (LocalDBManager.Instance.GetDBSetting("FirstStepCompleted") != null && LocalDBManager.Instance.GetDBSetting("FirstStepCompleted")?.Value == "true")
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                lstChats.BackgroundColor = Color.Transparent;
                stackOptions.BackgroundColor = Color.Transparent;
                this.ToolbarItems.Clear();
                var generalToolbarItem = new ToolbarItem("Buy", "menu.png", ShowMoreMenu, ToolbarItemOrder.Primary, 0);
                this.ToolbarItems.Add(generalToolbarItem);

                BotList.Clear();
                stackOptions.Children.Clear();
                workoutLogAverage = null;
                //await Task.Delay(2000);
                //BotList.Add(new BotModel()
                //{
                //    Question = "Dr. Muscle",
                //    Type = BotType.Ques
                //});
                if (LocalDBManager.Instance.GetDBSetting("firstname") == null)
                    return;
                if (LocalDBManager.Instance.GetDBSetting("IsPurchased") != null && LocalDBManager.Instance.GetDBSetting("IsPurchased").Value == "true")
                {
                    App.IsV1UserTrial = true;
                }
                if (LocalDBManager.Instance.GetDBSetting("IsMealPlanPurchased") != null && LocalDBManager.Instance.GetDBSetting("IsMealPlanPurchased").Value == "true")
                {
                    App.IsV1UserTrial = true;
                }
                if (Device.RuntimePlatform == Device.Android)
                    LoadSavedWeights();
                var welcomeNote = "";
                string fname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                Title = "Home";
                BotList.Clear();
                var welcomemsg = "Tap Start workout to begin!";
                CheckMonthlyUser();
                //BotList.Add(new BotModel()
                //{
                //    Type = BotType.Congratulations,
                //    Question = App.IsNewUser ? $"Welcome {fname}!" : $"Welcome back {fname}!"
                //});

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
                if (_isFirstDemoOpen)
                    isBackedUser = true;
                try
                {
                    if (workouts != null && workouts.Sets != null)
                    {
                        workoutLogAverage = workouts;
                        if (workouts.Averages.Count > 1)
                        {
                            isBackedUser = true;
                            //Add Chart

                        }
                    }
                }
                catch (Exception ex)
                {

                }

                if (LocalDBManager.Instance.GetDBSetting("isWorkoutNotSync")?.Value == "true")
                {
                   
                }

                if (workouts != null)
                {
                    if (workouts.Sets != null)
                    {
                        if (workouts.Averages.Count > 1)
                        {
                            OneRMAverage last = workouts.Averages.ToList()[workouts.Averages.Count - 1];
                            OneRMAverage before = workouts.Averages.ToList()[workouts.Averages.Count - 2];
                            var cong = BotList.FirstOrDefault(x => x.Type == BotType.Congratulations);
                            if (cong == null)
                            {
                                if ((last.Average.Kg - before.Average.Kg) >= 0)
                                {
                                    welcomemsg = "Your strength is going up!";
                                    LblStrengthProgress.Text = "Your strength is up!";
                                }
                                else
                                {
                                    welcomemsg = "Your strength is going down";
                                    LblStrengthProgress.Text = "Your strength is down";
                                    try
                                    {
                                        var creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                                        if (creationDate != null && workouts.Averages.ToList()[workouts.Averages.Count - 2].Average.Kg == 0)
                                        {
                                            LblStrengthProgress.Text = "Your strength is up!";
                                            welcomemsg = "Your strength is going up!";
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                }
                                if (BotList.FirstOrDefault(x => x.Type == BotType.Congratulations) == null)
                                    BotList.Add(new BotModel()
                                    {
                                        Type = BotType.Congratulations,
                                        Question = welcomemsg
                                    });
                            }
                        }
                    }
                }
                if (isBackedUser == false)
                {
                    if (!CrossConnectivity.Current.IsConnected)
                    {
                        ConnectionErrorPopup();
                    }
                    if (LocalDBManager.Instance.GetDBSetting("isNextWorkoutLoaded")?.Value == "false")
                        BotList.Clear();
                    if (BotList.Count > 1)
                        return;
                    
                    await FetchMainDataWithLoader();
                    if (BotList.Count > 1)
                        return;
                    await WorkoutLogSets(true);
                    LocalDBManager.Instance.SetDBSetting("isNextWorkoutLoaded", "true");
                    if (workouts != null)
                    {
                        if (workouts.Sets != null)
                        {
                            if (workouts.Averages.Count > 1)
                            {
                                OneRMAverage last = workouts.Averages.ToList()[workouts.Averages.Count - 1];
                                OneRMAverage before = workouts.Averages.ToList()[workouts.Averages.Count - 2];
                                var cong = BotList.FirstOrDefault(x => x.Type == BotType.Congratulations);
                                if (cong == null)
                                {
                                    if ((last.Average.Kg - before.Average.Kg) >= 0)
                                    {
                                        welcomemsg = "Your strength is going up!";
                                    }
                                    else
                                    {
                                        welcomemsg = "Your strength is going down";
                                        try
                                        {
                                            var creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                                            if (workouts.Averages.ToList()[workouts.Averages.Count - 2].Average.Kg == 0)
                                                welcomemsg = "Your strength is going up!";
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }
                                    if (BotList.FirstOrDefault(x => x.Type == BotType.Congratulations) == null)
                                        BotList.Add(new BotModel()
                                        {
                                            Type = BotType.Congratulations,
                                            Question = welcomemsg
                                        });
                                }
                            }
                        }
                    }
                    workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    //try
                    //{
                    //    if (workouts != null && workouts.Sets != null)
                    //    {
                    //        workoutLogAverage = workouts;
                    //        if (workouts.Averages.Count > 1)
                    //        {
                    //            OneRMAverage last = workouts.Averages.OrderBy(a => a.Date).ToList()[workouts.Averages.Count - 1];
                    //            OneRMAverage before = workouts.Averages.OrderBy(a => a.Date).ToList()[workouts.Averages.Count - 2];
                    //            decimal progresskg = (last.Average.Kg - before.Average.Kg) * 100 / last.Average.Kg;
                    //            bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                    //            BotList.Clear();
                    //            //    BotList.Add(new BotModel()
                    //            //{
                    //            //    Type = BotType.Chart
                    //            //});

                    //            isBackedUser = true;
                    //            //Add Chart

                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{

                    //}
                }
                if (BotList.Count > 2)
                    return;
                if (Device.RuntimePlatform == Device.Android)
                    PagesFactory.GetPage<KenkoChooseYourWorkoutExercisePage>();
                //PagesFactory.GetPage<SubscriptionPage>();
                if (!_isFirstDemoOpen && Device.RuntimePlatform.Equals(Device.Android))
                {
                    if ((LocalDBManager.Instance.GetDBSetting("IsFirstMessage") != null && LocalDBManager.Instance.GetDBSetting("IsFirstMessage").Value == "First1") || (LocalDBManager.Instance.GetDBSetting("IsFirstMessageSend") != null && LocalDBManager.Instance.GetDBSetting("IsFirstMessageSend").Value == "First1"))
                    {
                        LocalDBManager.Instance.SetDBSetting("IsSecondMessage", "Second1");
                        ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Tabs[0].BadgeCaption = 1;
                    }
                }
                if (workouts != null && workouts.GetUserProgramInfoResponseModel == null || LocalDBManager.Instance.GetDBSetting("isNextWorkoutLoaded")?.Value == "false")
                {
                    LocalDBManager.Instance.SetDBSetting("isNextWorkoutLoaded", "true");
                    await FetchMainData();
                }
                LoadUpcomingDays();
                await SetStatsMessage($"");
                GotIt_Clicked2(new DrMuscleButton() { Text = AppResources.GotIt }, EventArgs.Empty);

                workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                //Check for popup
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
                string popupMessage = "", message = "";
                upi = workouts.GetUserProgramInfoResponseModel;

                var remainigSentence = "";

                try
                {
                    var workouts1 = ((App)Application.Current).UserWorkoutContexts.workouts;
                    int remainingWorkout = 0, totalworkout = 0;
                    var name = "";
                    if (workouts1 != null && workouts.GetUserProgramInfoResponseModel != null)
                    {
                        if (workouts1.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                        {

                            name = workouts1.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label;
                            totalworkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RequiredWorkoutToLevelUp;
                            remainingWorkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp != null ? (int)workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp : 0;
                        }
                    }


                    bool IsStrengthPhashe = RecoComputation.IsInStrengthPhase(name, int.Parse(string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value) ? "40" : LocalDBManager.Instance.GetDBSetting("Age")?.Value), remainingWorkout, totalworkout);
                    _isInStrengthPhase = IsStrengthPhashe;
                    string wkText = remainingWorkout == 1 ? "workout" : "workouts";

                    if (remainingWorkout <= 0)
                        remainigSentence = "";
                    else if (IsStrengthPhashe)
                        remainigSentence = $"You have {workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp} {wkText} left in your strength phase.";
                    else
                        remainigSentence = $"You have {remainingWorkout} {wkText} left before you level up.";


                }
                catch (Exception ex)
                {

                }

                //if (workouts?.HistoryExerciseModel != null)
                //{
                //    try
                //    {
                //        var usercreationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                //        var userWithdays = 0;
                //        if (usercreationDate != null)
                //        {
                //            userWithdays = (int)(DateTime.Now - usercreationDate).TotalDays;
                //        }
                //        var d = userWithdays > 1 ? "days" : "day";
                //        LblXXWorkout.Text = $"{workouts.HistoryExerciseModel.TotalWorkoutCompleted} {(workouts.HistoryExerciseModel.TotalWorkoutCompleted > 1 ? "workouts" : "workout")} in {userWithdays} {d}";
                //    }
                //    catch (Exception e)
                //    {
                        
                //    }
                    
                //}
                if (workouts.LastWorkoutDate != null)
                {
                    //With 1 day 16 hours of rest, I suggest you start Upper-body now.
                    var RequiredHours = 18;
                    if (workouts != null && workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                    {
                        if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bodyweight") ||
workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("mobility") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("powerlifting") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("full-body") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bands"))
                        {
                            RequiredHours = 42;
                            if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                            {
                                if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) < 30)
                                    RequiredHours = 18;
                            }
                            if (workouts.LastConsecutiveWorkoutDays > 1 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 2)
                                RequiredHours = 42;
                        }
                        else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] legs") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] legs"))
                        {
                            RequiredHours = 18;
                            if (workouts.LastConsecutiveWorkoutDays > 5 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 3)
                            {
                                RequiredHours = 42;
                            }
                        }
                        else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("split"))
                        {
                            RequiredHours = 18;
                            if (workouts.LastConsecutiveWorkoutDays > 1 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 2)
                                RequiredHours = 42;
                            if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                            {
                                if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) > 50)
                                    RequiredHours = 42;
                            }
                        }


                    }
                    if (days > 0)
                    {
                        if (days > 2)
                        {
                            var sinceday = hours / 24;
                            var sincehour = hours % 24;
                            string workoutResttime = "";
                            if (sincehour >= 1)
                                workoutResttime = string.Format("{0} {1} {2} {3}", sinceday, "d", sincehour, sincehour == 1 ? "hr" : "hrs");
                            else
                                workoutResttime = string.Format("{0} {1}", sinceday, sinceday == 1 ? "day" : "days");
                            if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                            {
                                popupMessage = $"With {workoutResttime} of rest,";
                                message = $"I suggest you start {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}. {remainigSentence}";
                            }
                            else
                            {
                                popupMessage = $"With {workoutResttime} of rest,";
                                message = $"I suggest you start work out now.";
                            }
                        }
                        else
                        {

                            if (hours < RequiredHours)
                            {
                                //var sinceday = hours / 24;
                                //var sincehour = hours % 24;

                                //var workoutResttime = string.Format("{0} {1} {2} {3}", sinceday, sinceday == 1 ? "day" : "days", sincehour, sincehour == 1 ? "hour" : "hours");
                                //if (sincehour == 0)
                                //    str = string.Format("{0} {1}", sinceday, sinceday == 1 ? "day" : "days");
                                var h = hours <= 1 ? "hour" : "hours";
                                var reqHour = RequiredHours - hours <= 1 ? "hour" : "hours";
                                var str = $"{hours} {h}";
                                if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                                {
                                    popupMessage = $"With {str} of rest,";
                                    message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}. {remainigSentence}";
                                }
                                else
                                {
                                    popupMessage = $"With {str} of rest,";
                                    message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout. {remainigSentence}";
                                }
                            }
                            else
                            {
                                var sinceday = hours / 24;
                                var sincehour = hours % 24;
                                string workoutResttime = "";
                                if (sincehour >= 1)
                                    workoutResttime = string.Format("{0} {1} {2} {3}", sinceday, "d", sincehour, sincehour == 1 ? "hr" : "hrs");
                                else
                                    workoutResttime = string.Format("{0} {1}", sinceday, sinceday == 1 ? "day" : "days");
                                // var workoutResttime = string.Format("{0} {1}, {2} {3}", sinceday, sinceday == 1 ? "day" : "days", sincehour, sincehour == 1 ? "hour" : "hours");
                                if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                                {
                                    popupMessage = $"With {workoutResttime} of rest,";
                                    message = $"I suggest you start {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}. {remainigSentence}";

                                }
                                else
                                {
                                    popupMessage = $"With {workoutResttime} of rest,";
                                    message = $"I suggest you start work out now.";
                                }
                            }
                        }
                    }
                    else
                    {
                        var reqHour = RequiredHours - hours <= 1 ? "hour" : "hours";

                        if (hours > 1 && hours < RequiredHours)
                        {
                            if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                            {
                                popupMessage = $"With {hours} {dayStr} of rest,";
                                message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}. {remainigSentence}";
                            }
                            else
                            {
                                popupMessage = $"With {hours} {dayStr} of rest,";
                                message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout. {remainigSentence}";
                            }
                            //popupMessage = $"With {hours} {dayStr} of rest, I suggest you take a day off.";
                        }
                        else if (hours >= RequiredHours)
                        {
                            if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                            {
                                popupMessage = $"With {hours} {dayStr} of rest,";
                                message = $"I suggest you start {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}. {remainigSentence}";
                            }
                            else
                            {
                                popupMessage = $"With {hours} {dayStr} of rest,";
                                message = $"I suggest you start work out now.";
                            }
                        }
                        //else if (hours > 1 && hours < 24)
                        //{
                        //    if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                        //        popupMessage = $"With {hours} {dayStr} of rest, I suggest you start {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label} now.";
                        //    else
                        //        popupMessage = $"With {hours} {dayStr} of rest, I suggest you work out now.";
                        //}
                        else if (minutes < 60)
                        {
                            if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                            {
                                popupMessage = $"With {minutes} {dayStr} of rest,";
                                message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}. {remainigSentence}";
                            }
                            else
                            {
                                popupMessage = $"With {minutes} {dayStr} of rest,";
                                message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout. {remainigSentence}";
                            }
                            //popupMessage = $"With {minutes} {dayStr} of rest, I suggest you take a day off.";
                        }
                        else
                        //popupMessage = $"With {hours} {dayStr} of rest, I suggest you take a day off.";
                        {
                            if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                            {
                                popupMessage = $"With {hours} {dayStr} of rest,";
                                message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}. {remainigSentence}";
                            }
                            else
                            {
                                popupMessage = $"With {hours} {dayStr} of rest,";
                                message = $"I suggest you recover {RequiredHours - hours} {reqHour} more before your workout. {remainigSentence}";
                            }
                        }
                    }
                }
                else
                {
                    if (workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null)
                    {
                        //Last workout: XXX, so I suggest you XXX. Today's workout: XXX.
                        if (LocalDBManager.Instance.GetDBSetting("OlderWorkoutName") != null && LocalDBManager.Instance.GetDBSetting("OlderWorkoutName").Value != null)
                        {
                            //popupMessage = $"Last workout: {LocalDBManager.Instance.GetDBSetting("OlderWorkoutName").Value}";
                            message = null;
                            popupMessage = $"I suggest you start {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label.Trim()}. {remainigSentence}";
                        }
                        else
                        {
                            popupMessage = $"Today's workout: {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label.Trim()}. Start workout?";
                            message = null;
                        }

                    }
                    else
                    {
                        popupMessage = $"Start today's workout?";
                        message = null;
                    }
                }
                if (!string.IsNullOrEmpty(popupMessage) && !App.IsDemoProgress)
                {
                    CurrentLog.Instance.IsWelcomeMessage = true;
                    var multipleEquip = 0;
                    if (LocalDBManager.Instance.GetDBSetting("Equipment")?.Value == "true")
                        multipleEquip = 1;
                    if (LocalDBManager.Instance.GetDBSetting("HomeMainEquipment")?.Value == "true")
                        multipleEquip += 1;
                    if (LocalDBManager.Instance.GetDBSetting("OtherMainEquipment")?.Value == "true")
                        multipleEquip += 1;


                    ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                    {
                        Title = $"Welcome back!",
                        Message = "How are you today?",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Good",
                        CancelText = "Tired",
                        OnAction = (ok) =>
                        {
                            if (ok)
                            {
                                askForworkout(popupMessage, message);
                            }
                            else
                            {
                                ConfirmConfig showQuickPopup = new ConfirmConfig()
                                {
                                    Title = $"Shorten workout?",
                                    Message = "Recover with a short workout (fewer sets). Just for today.",
                                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                    OkText = "Short workout",
                                    CancelText = "Normal",
                                    OnAction = (yesAction) =>
                                    {
                                        if (yesAction)
                                        {
                                            try
                                            {
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
                                                askForworkout(popupMessage, message);


                                            }
                                            catch (Exception ex)
                                            {

                                            }
                                        }
                                        else
                                        {
                                            //Normal workout
                                            askForworkout(popupMessage, message);
                                        }

                                    }
                                };
                                UserDialogs.Instance.Confirm(showQuickPopup);
                            }
                        }
                    };
                    //if (!App.IsNewUser && !CurrentLog.Instance.IsWelcomePopup && LocalDBManager.Instance.GetDBSetting("firstname") != null && !App.IsSleeping)
                    if (LocalDBManager.Instance.GetDBSetting("firstname") != null)
                        askForworkout(popupMessage, message);// 

                    CurrentLog.Instance.IsWelcomePopup = true;
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
                        await FetchMainData();
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

                    if (btnWorkoutNow.IsVisible && SecondProgressBox.IsVisible)
                    {
                        LocalDBManager.Instance.SetDBSetting($"ExtraWorkoutAskedStrenth", $"{DateTime.Now.Date.AddDays(4).Ticks}");
                    }
                    else if (btnRestNow.IsVisible && SecondProgressBox.IsVisible)
                    {
                        LocalDBManager.Instance.SetDBSetting($"ExtraWorkoutAskedRecover", $"{DateTime.Now.Date.AddDays(4).Ticks}");
                    }
                }
                catch (Exception ex)
                {
                    await FetchMainData();
                    workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                }
                finally
                {

                    Device.InvokeOnMainThreadAsync(() =>
                    {
                        if (_isFirstDemoOpen)
                        {
                            EmptyStateStack.IsVisible = false;
                            StackSteps1.IsVisible = true;
                            calendarBox1.Content = calendar;
                            LoadCurrentWorkoutTemplate();
                        }
                        else if (!EmptyStateStack.IsVisible)
                        {
                            StackSteps2.IsVisible = true;
                            calendarBox2.Content = calendar;
                        }
                        LoadChat();
#if DEBUG
                        //StackSteps1.IsVisible = true;
                        //StackSteps2.IsVisible = true;
                        //GoalBox.IsVisible = true;
                        //GoalBox2.IsVisible = true;
#endif
                        if (!CrossConnectivity.Current.IsConnected)
                        {

                            WeightBox2.IsVisible = false;
                            WeightProgress1.IsVisible = false;
                            WeightProgress2.IsVisible = false;
                            WeightCoachingCard1.IsVisible = false;
                            WeightCoachingCard2.IsVisible = false;
                        }
                        else
                        {
                            if (ImgWeight.IsVisible)
                                WeightBox2.IsVisible = false;
                            else
                                WeightBox2.IsVisible = true;
                            WeightProgress1.IsVisible = true;
                            WeightProgress2.IsVisible = true;
                            WeightCoachingCard1.IsVisible = true;
                            WeightCoachingCard2.IsVisible = true;
                        }
                    });

                }
                await CanGoFurther();
                Check10Days();
                FetchMainData();
                //await SetStatsMessage("");
                //GotIt_Clicked2(new DrMuscleButton() { Text = AppResources.GotIt }, EventArgs.Empty);

            }
            catch (Exception ex)
            {

            }
        }

        private async Task LoadChat()
        {
           // CurrentLog.Instance.GroupChats = await DrMuscleRestClient.Instance.FetchGroupMessages(new GroupChatModel() { UpdatedAt = DateTime.UtcNow });
            if (LocalDBManager.Instance.GetDBSetting("email") == null)
                return;
            // var IsAdmin = LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("etiennejuneau@gmail.com") || LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("jorum@dr-muscle.com");
            // if (!IsAdmin)
        }
        private void ShowAddRecoveryWorkout()
        {
            try
            {
                //show each state maximum 1x every 5 days
                var ticksStrenth = LocalDBManager.Instance.GetDBSetting($"ExtraWorkoutAskedStrenth")?.Value;
                var ticksRecover = LocalDBManager.Instance.GetDBSetting($"ExtraWorkoutAskedRecover")?.Value;
                if (string.IsNullOrEmpty(ticksStrenth) && string.IsNullOrEmpty(ticksRecover))
                    SecondProgressBox.IsVisible = true;
                if (!string.IsNullOrEmpty(ticksStrenth) && btnWorkoutNow.IsVisible)
                {
                    var date = new DateTime(long.Parse(ticksStrenth));
                    if (DateTime.Now.Date > date)
                    {
                        SecondProgressBox.IsVisible = true;
                    }
                }
                if (!string.IsNullOrEmpty(ticksRecover) && btnRestNow.IsVisible)
                {
                    var date = new DateTime(long.Parse(ticksRecover));
                    if (DateTime.Now.Date > date)
                    {
                        SecondProgressBox.IsVisible = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private async void PushToWorkout()
        {
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("email") == null)
                    return;

                Device.BeginInvokeOnMainThread(async () =>
                {

                    CurrentLog.Instance.WorkoutStarted = true;
                    CurrentLog.Instance.CurrentWorkoutTemplate = new WorkoutTemplateModel()
                    {
                        Id = 12645,
                        Label = "Bodyweight 1",
                        IsSystemExercise = true
                    };
                    CurrentLog.Instance.CurrentWorkoutTemplate.Exercises = new List<ExerciceModel>();
                    CurrentLog.Instance.CurrentWorkoutTemplateGroup = new WorkoutTemplateGroupModel()
                    {
                        Id = 487,
                        IsSystemExercise = true,
                        Label = "Bodyweight Level 1",
                        WorkoutTemplates = new List<WorkoutTemplateModel>() { CurrentLog.Instance.CurrentWorkoutTemplate }
                    };

                    var workoutModel = LocalDBManager.Instance.GetDBSetting($"Workout{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}")?.Value;
                    if (!string.IsNullOrEmpty(workoutModel))
                    {
                        var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkoutTemplateModel>(workoutModel);
                        CurrentLog.Instance.CurrentWorkoutTemplate = model;
                    }

                    await PagesFactory.PushAsync<KenkoChooseYourWorkoutExercisePage>();
                });


            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message);
            }
        }

        private async void SetBottomCard()
        {
            try
            {

                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                if (workouts == null || workouts.GetUserProgramInfoResponseModel == null || workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
                {
                    LblWorkoutDuration.IsVisible = false;
                    LblWorkoutDuration2.IsVisible = false;
                    return;
                }
                if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Exercises.Count > 0)
                {
                    var count = workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Exercises.Count;
                    var exe = count > 1 ? "exercises" : "exercise";
                    LblWorkoutDuration.Text = $"{count} {exe} · {count * 8} min";
                    LblWorkoutDuration2.Text = LblWorkoutDuration.Text;
                    LblWorkoutDuration.IsVisible = true;
                    LblWorkoutDuration2.IsVisible = true;
                }

            }
            catch (Exception ex)
            {

            }
        }

        private async void PushToUnfinishedWorkout(string workoutId)
        {
            try
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    long workouttemplateid = 0;
                    if (string.IsNullOrEmpty(workoutId))
                        return;
                    workouttemplateid = long.Parse(workoutId);
                    CurrentLog.Instance.IsRecoveredWorkout = false;
                    CurrentLog.Instance.IsUnFinishedWorkout = true;
                    if (workouttemplateid == 16904 || workouttemplateid == 16905 || workouttemplateid == 16906)
                        CurrentLog.Instance.IsMobilityStarted = true;
                    CurrentLog.Instance.WorkoutStarted = true;
                    CurrentLog.Instance.CurrentWorkoutTemplate = new WorkoutTemplateModel()
                    {
                        Id = workouttemplateid,
                        Label = "",
                        IsSystemExercise = true
                    };
                    CurrentLog.Instance.CurrentWorkoutTemplate.Exercises = new List<ExerciceModel>();
                    var workoutModel = LocalDBManager.Instance.GetDBSetting($"Workout{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}")?.Value;
                    if (!string.IsNullOrEmpty(workoutModel))
                    {
                        var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkoutTemplateModel>(workoutModel);
                        CurrentLog.Instance.CurrentWorkoutTemplate = model;
                    }
                    await PagesFactory.PushAsync<KenkoChooseYourWorkoutExercisePage>();
                });


            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message);
            }
        }
        private void askForworkout(string popupMessage, string message)
        {
            //if (Device.RuntimePlatform.Equals(Device.iOS))
            //    welcomePopupStack.IsVisible = true;
            //else
            //{
            //    ActionSheetConfig config = new ActionSheetConfig()
            //    {
            //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            //    };



            //    config.Add("Tired/short on time", () =>
            //    {
            //        BtnFeelingWeekShortOnTime_Clicked(new DrMuscleButton() { Text = "I'm tired or short on time" }, EventArgs.Empty);
            //    });
            //    config.Add("Good", () =>
            //    {
            //        StartTodaysWorkout();
            //    });
            //    //config.SetCancel(AppResources.Cancel, null);
            //    config.SetTitle(popupMessage);

            //    UserDialogs.Instance.ActionSheet(config);
            //}
            var firstName = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
            if (message != null)//&& message.Contains("more before your workout")
            {
                if (message.Contains("more before your workout"))
                {
                    SecondWelcomeBox.IsVisible = true;
                    LblWelcomeback.Text = $"Welcome back {firstName}!".ToUpper();

                    LblWelcomebackText.Text = $"{popupMessage} {message}";


                    if (CurrentLog.Instance.IsWorkoutedOut)
                    {
                        LblGoal.Text = $"{popupMessage} {message}";
                    }
                    btnWelcomeGotit.IsVisible = true;
                    btnWelcomeStartWorkout.IsVisible = false;


                    IsWorkoutAdded();
                    AlertConfig ShowRIRPopUp = new AlertConfig()
                    {
                        Title = $"Welcome back!",
                        Message = $"{popupMessage} {message}",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = AppResources.GotIt
                    };
                    //UserDialogs.Instance.AlertAsync(ShowRIRPopUp);
                    return;
                }
                LblWelcomeback.Text = $"Welcome back {firstName}!".ToUpper();
                LblWelcomebackText.Text = $"{popupMessage} {message}";
                if (CurrentLog.Instance.IsWorkoutedOut)
                {
                    LblGoal.Text = $"{popupMessage} {message}";
                }
                btnWelcomeGotit.IsVisible = false;
                btnWelcomeStartWorkout.IsVisible = true;
                IsWorkoutAdded();

                ConfirmConfig ShowRIRPopUp1 = new ConfirmConfig()
                {
                    Title = "Welcome back!",
                    Message = $"{popupMessage} {message}",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Start workout",
                    CancelText = "Wait",
                    OnAction = (ok) =>
                    {
                        if (ok)
                            askforEquipment();
                        else
                        {
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
                        }
                    }
                };
                //UserDialogs.Instance.Confirm(ShowRIRPopUp1);
            }
            else
            {
                LblWelcomeback.Text = $"Welcome back {firstName}!".ToUpper();
                LblWelcomebackText.Text = $"{popupMessage} {message}";
                if (CurrentLog.Instance.IsWorkoutedOut)
                {
                    LblGoal.Text = $"{popupMessage} {message}";
                }
                btnWelcomeGotit.IsVisible = false;
                btnWelcomeStartWorkout.IsVisible = true;
                IsWorkoutAdded();
                ConfirmConfig ShowRIRPopUp1 = new ConfirmConfig()
                {
                    Title = popupMessage,
                    Message = message,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Start workout",
                    CancelText = "Later",
                    OnAction = (ok) =>
                    {
                        if (ok)
                            askforEquipment();
                        else
                        {
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
                        }
                    }
                };
                // UserDialogs.Instance.Confirm(ShowRIRPopUp1);
            }

        }

        private void IsWorkoutAdded()
        {
            if (LocalDBManager.Instance.GetDBSetting($"WorkoutAdded{DateTime.Now.Date}")?.Value == "true")
            {
                LblWelcomebackText.Text = "Work out today.";
                btnWelcomeGotit.IsVisible = false;
                btnWelcomeStartWorkout.IsVisible = true;
                LblSinceTIme.Text = "18/18 hours";
                LblLastworkoutText.Text = "recovery";
                TrainRestImage.Source = "green.png";
                LblTrainRest.Text = "Train";
                LblTrainRestText.Text = $"Coach says";
                CurrentLog.Instance.IsRest = false;
                LblTrainRest.TextColor = AppThemeConstants.GreenColor;

            }
            else if (LocalDBManager.Instance.GetDBSetting($"WorkoutAdded{DateTime.Now.Date.AddDays(1)}")?.Value == "true")
            {
                LblWelcomebackText.Text = "Next workout queued 18 hours after today's workout.";
                LblSinceTIme.Text = "0/18 hours";
                LblLastworkoutText.Text = "recovery";
                //TrainRestImage.Source = lifted.TrainRest == "Train" ? "green.png" : "orange2.png";
                //LblTrainRest.Text = lifted.TrainRest;
                //LblTrainRestText.Text = lifted.TrainRestText;
                //LblTrainRest.TextColor = AppThemeConstants.GreenColor;
            }
        }
        private void askforEquipment()
        {
            var multipleEquip = 0;
            if (LocalDBManager.Instance.GetDBSetting("Equipment")?.Value == "true")
                multipleEquip = 1;
            if (LocalDBManager.Instance.GetDBSetting("HomeMainEquipment")?.Value == "true")
                multipleEquip += 1;
            if (LocalDBManager.Instance.GetDBSetting("OtherMainEquipment")?.Value == "true")
                multipleEquip += 1;

            if (multipleEquip > 1)
            {
                //Ask for Equipment
                ActionSheetConfig config = new ActionSheetConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                };

                bool isProduction = LocalDBManager.Instance.GetDBSetting("Environment") == null || LocalDBManager.Instance.GetDBSetting("Environment").Value == "Production";
                if (LocalDBManager.Instance.GetDBSetting("Equipment")?.Value == "true")
                    config.Add("Gym", async () =>
                    {
                        if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
                        {
                            StartTodaysWorkout();
                        }
                        else
                        {
                            await SetUserEquipmentSettings("gym");
                            //((App)Application.Current).UserWorkoutContexts.workouts = null;
                            //((App)Application.Current).UserWorkoutContexts.SaveContexts();

                            await FetchMainDataWithLoader();
                            WorkoutLogSets(false);
                            //BotList.Clear();
                            //await SetStatsMessage($"");
                            //GotIt_Clicked2(new DrMuscleButton() { Text = AppResources.GotIt }, EventArgs.Empty);
                            StartTodaysWorkout();
                        }
                    });
                if (LocalDBManager.Instance.GetDBSetting("HomeMainEquipment")?.Value == "true")
                    config.Add("Home", async () =>
                    {
                        if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                        {
                            StartTodaysWorkout();
                        }
                        else
                        {
                            await SetUserEquipmentSettings("home");
                            //((App)Application.Current).UserWorkoutContexts.workouts = null;
                            //((App)Application.Current).UserWorkoutContexts.SaveContexts();

                            await FetchMainDataWithLoader();
                            WorkoutLogSets(false);
                            //BotList.Clear();
                            //await SetStatsMessage($"");
                            //GotIt_Clicked2(new DrMuscleButton() { Text = AppResources.GotIt }, EventArgs.Empty);
                            StartTodaysWorkout();
                        }
                    });
                if (LocalDBManager.Instance.GetDBSetting("OtherMainEquipment")?.Value == "true")
                    config.Add("Other", async () =>
                    {
                        if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                        {
                            StartTodaysWorkout();
                        }
                        else
                        {
                            await SetUserEquipmentSettings("other");
                            //((App)Application.Current).UserWorkoutContexts.workouts = null;
                            //((App)Application.Current).UserWorkoutContexts.SaveContexts();

                            await FetchMainDataWithLoader();
                            WorkoutLogSets(false);
                            //BotList.Clear();
                            //await SetStatsMessage($"");
                            //GotIt_Clicked2(new DrMuscleButton() { Text = AppResources.GotIt }, EventArgs.Empty);
                            StartTodaysWorkout();
                        }
                    });
                config.SetTitle("Where are you training today?");

                UserDialogs.Instance.ActionSheet(config);
            }
            else
            {
                StartTodaysWorkout();
            }
        }

        private async void HelpWithGoal_Clicked(object sender, EventArgs args)
        {
            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[2];
            await Task.Delay(300);
            Xamarin.Forms.MessagingCenter.Send<HelpWithGoalChatMessage>(new HelpWithGoalChatMessage(), "HelpWithGoalChatMessage");
           
        }

        private void OpenChat_Clicked(object sender, EventArgs args)
        {
            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[2];
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
                                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value, IsSystemExercise = true },
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
                        await WorkoutLogSets(true);
                        workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    }
                }

                BotList.Add(new BotModel()
                {
                    Type = BotType.Chart
                });
                SetChartData();
                //BotList.Add(new BotModel()
                //{
                //    Type = BotType.WeightTracker
                //});
                //var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                var strProgress = "Your stats:\n";
                SetStatsMessage(strProgress);

            }
            catch (Exception ex)
            {

            }
        }



        string _weightLifted = "";
        string _weightLiftedText = "";

        async Task SetStatsMessage(string strProgress)
        {
            string welcomeMsg = "Tap Start workout to begin!";
            try
            {
                var statsModel = new BotModel()
                {
                    Type = BotType.Stats
                };
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                var levelUpBotModel = new BotModel();
                bool IsEstimated = false;
                levelUpBotModel.Type = BotType.LevelUp;

                DateTime? creationDate = null;
                try
                {
                    creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                }
                catch (Exception)
                {

                }
                EmptyStateStack.IsVisible = false;
                //TODO: changed for New UI
                //lstChats.IsVisible = true;
                lstChats.IsVisible = false;
                if (_isFirstDemoOpen)
                    {
                    StackSteps1.IsVisible = true;
                    calendarBox1.Content = calendar;
                }
                else
                {
                    StackSteps2.IsVisible = true;
                    calendarBox2.Content = calendar;
                }
                mainGrid.BackgroundColor = Color.FromHex("#f4f4f4");
                if (workouts != null)
                {
                    if (workouts.Sets != null)
                    {
                        if (workouts.Averages.Count > 1)
                        {
                            if (workouts.Averages[1].Average.Kg == 0)
                                IsEstimated = true;
                            OneRMAverage last = workouts.Averages.ToList()[workouts.Averages.Count - 1];
                            OneRMAverage before = workouts.Averages.ToList()[workouts.Averages.Count - 2];
                            decimal progresskg = (last.Average.Kg - before.Average.Kg) * 100 / (before.Average.Kg < 1 ? 1 : before.Average.Kg);
                            bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                            // strProgress += String.Format("- {0}: {1}{2} ({3}%)\n", AppResources.MaxStrength, (last.Average.Kg - before.Average.Kg) > 0 ? "+" : "", inKg ? Math.Round(last.Average.Kg - before.Average.Kg) + " kg" : Math.Round(last.Average.Lb - before.Average.Lb) + " lbs", Math.Round(progresskg)).ReplaceWithDot();
                            if ((last.Average.Kg - before.Average.Kg) >= 0)
                            {
                                statsModel.StrengthPerText = String.Format("Last 3 weeks, your strength went {0}{1}% (on average).", (last.Average.Kg - before.Average.Kg) >= 0 ? "+" : "", Math.Round(progresskg)).ReplaceWithDot();


                                //StrengthArrowText.Text =  statsModel.StrengthPerText;
                                //statsModel.StrengthMessage = String.Format(" {0}{1} {2}", (last.Average.Kg - before.Average.Kg) >= 0 ? "+ " : "", inKg ? Math.Round(last.Average.Kg - before.Average.Kg) + " kg" : Math.Round(last.Average.Lb - before.Average.Lb) + " lbs", AppResources.MaxStrength).ReplaceWithDot();
                                statsModel.StrengthImage = "up_arrow.png";
                                StrengthArrowImage.Source = "up_arrow.png";
                                //Green
                                statsModel.StrengthTextColor = Color.FromHex("#5CD196");
                                //StrengthArrowText.TextColor = Color.FromHex("#5CD196");
                                welcomeMsg = "Strength up";
                                var perceStr = "";
                                if (before.Average.Kg == 0)
                                {
                                    perceStr = String.Format("{0}!", Math.Round(inKg ? last.Average.Kg : last.Average.Lb)).ReplaceWithDot();
                                    LblStrengthProgress.Text = String.Format("{0} {1}{2}", "Strength up", (last.Average.Kg - before.Average.Kg) >= 0 ? "" : "", Math.Round(inKg ? last.Average.Kg : last.Average.Lb)).ReplaceWithDot();

                                }
                                else
                                {
                                    perceStr = String.Format("{0}%!", Math.Round(progresskg)).ReplaceWithDot();
                                    LblStrengthProgress.Text = String.Format("{0} {1}{2}%", "Strength up", (last.Average.Kg - before.Average.Kg) >= 0 ? "" : "", Math.Round(progresskg)).ReplaceWithDot();
                                }
                                LblStrengthUp.Text = "Add workout?";
                                LblStrengthUpText.Text = $"Strength up {perceStr.Replace("!", "")} in 7 days! Feeling fresh? Try adding a workout this week.";
                                btnRestNow.IsVisible = false;
                                btnWorkoutNow.IsVisible = true;
                                ShowAddRecoveryWorkout();
                            }
                            else
                            {
                                var perceStr = "";
                                statsModel.StrengthPerText = String.Format("Last 3 weeks, your strength went {0}{1}% (on average).", (last.Average.Kg - before.Average.Kg) >= 0 ? "" : "", Math.Round(progresskg)).ReplaceWithDot();
                                perceStr = String.Format("{0}%", Math.Round(progresskg)).ReplaceWithDot().Replace("-", "");
                                //LblStrengthProgress.Text = String.Format("{0} {1}{2}!", LblStrengthProgress.Text, (last.Average.Kg - before.Average.Kg) >= 0 ? "+" : "", Math.Round(progresskg)).ReplaceWithDot();
                                //statsModel.StrengthMessage = String.Format(" {0}{1} {2}", (last.Average.Kg - before.Average.Kg) >= 0 ? "+ " : "", inKg ? Math.Round(last.Average.Kg - before.Average.Kg) + " kg" : Math.Round(last.Average.Lb - before.Average.Lb) + " lbs", AppResources.MaxStrength).ReplaceWithDot();
                                statsModel.StrengthImage = "down_arrow.png";
                                StrengthArrowImage.Source = "down_arrow.png";
                                //Red
                                statsModel.StrengthTextColor = Color.FromHex("#BA1C31");
                                //StrengthArrowText.TextColor = Color.FromHex("#BA1C31");
                                welcomeMsg = "Strength down";
                                LblStrengthProgress.Text = String.Format("{0} {1}{2}%", "Strength down", (last.Average.Kg - before.Average.Kg) >= 0 ? "" : "", Math.Round(progresskg)).ReplaceWithDot().Replace("-", "");

                                LblStrengthUp.Text = "Recovery workout?";
                                LblStrengthUpText.Text = $"Strength down {perceStr.Replace("!", "")} in 7 days. Feeling tired? Try a recovery workout (only 2 work sets per exercise).";
                                btnRestNow.IsVisible = true;
                                btnWorkoutNow.IsVisible = false;

                                if (IsEstimated)
                                {
                                    welcomeMsg = "Your strength is going up!";
                                    LblStrengthProgress.Text = "Strength up";
                                    LblStrengthUp.Text = "Add workout?";
                                    LblStrengthUpText.Text = "Strength up! Feeling fresh? Try adding a workout this week.";
                                    btnRestNow.IsVisible = false;
                                    btnWorkoutNow.IsVisible = true;
                                }
                                ShowAddRecoveryWorkout();
                            }
                            //statsModel.StrengthMessage = AppResources.MaxStrength;
                            workouts.Sets.Reverse();
                            workouts.SetsDate.Reverse();

                            if (workouts.Sets.Count > 1)
                            {
                                bool isflg = false;
                                foreach (var set in workouts.Sets)
                                {
                                    if (set != 0)
                                        isflg = true;
                                }
                                if (!isflg)
                                    welcomeMsg = "Tap Start workout to begin!";
                                int firstSets = workouts.Sets[workouts.Sets.Count - 1];
                                int lastSets = workouts.Sets[workouts.Sets.Count - 2];
                                try
                                {
                                    decimal progressSets = (firstSets - lastSets) * 100 / (lastSets == 0 ? 1 : lastSets);
                                    if (firstSets == 0)
                                    {
                                        progressSets = lastSets;
                                    }
                                    // strProgress += String.Format("- {0}: {1}{2} ({3}%)\n", AppResources.WorkSetsNoColon, (firstSets - lastSets) >= 0 ? "+" : "", firstSets - lastSets, Math.Round(progressSets)).ReplaceWithDot();
                                    if ((firstSets - lastSets) >= 0)
                                    {
                                        statsModel.SetsPerText = String.Format("Last 3 weeks, your work sets went {0}{1}{2} (on average).", (firstSets - lastSets) >= 0 ? "" : "", Math.Round(progressSets), firstSets == 0 ? "" : "%").ReplaceWithDot();

                                        //statsModel.SetsMessage = String.Format(" {0}{1} {2}", (firstSets - lastSets) >= 0 ? "+ " : "", firstSets - lastSets, AppResources.WorkSetsNoColon).ReplaceWithDot();
                                        statsModel.SetsImage = "up_arrow.png";
                                        VolumeArrowImage.Source = "up_arrow.png";
                                        statsModel.SetTextColor = Color.FromHex("#5CD196");
                                        //VolumeArrowText.TextColor = Color.FromHex("#5CD196"); ;
                                        LblVolumeProgress.Text = "Volume up";
                                        if (lastSets == 0)
                                            LblVolumeProgress.Text = String.Format("{0} {1}{2}", LblVolumeProgress.Text, (firstSets - lastSets) >= 0 ? "" : "", firstSets);
                                        else
                                            LblVolumeProgress.Text = String.Format("{0} {1}{2}%", LblVolumeProgress.Text, (firstSets - lastSets) >= 0 ? "" : "", Math.Round(progressSets));
                                    }
                                    else
                                    {
                                        statsModel.SetsPerText = String.Format("Last 3 weeks, your work sets went {0}{1}{2} (on average).", (firstSets - lastSets) >= 0 ? "" : "", Math.Round(progressSets), firstSets == 0 ? "" : "%").ReplaceWithDot();
                                        //VolumeArrowText.Text = statsModel.SetsPerText;
                                        //statsModel.SetsMessage = String.Format(" {0}{1} {2}", (firstSets - lastSets) >= 0 ? "+ " : "", firstSets - lastSets, AppResources.WorkSetsNoColon).ReplaceWithDot();
                                        statsModel.SetsImage = "down_arrow.png";
                                        VolumeArrowImage.Source = "down_arrow.png";
                                        //VolumeArrowText.TextColor = Color.FromHex("#BA1C31");
                                        statsModel.SetTextColor = Color.FromHex("#BA1C31");
                                        LblVolumeProgress.Text = "Volume down";


                                        if (firstSets == 0)
                                            LblVolumeProgress.Text = String.Format("{0} {1}{2}", LblVolumeProgress.Text, (firstSets - lastSets) >= 0 ? "" : "", firstSets);
                                        else
                                            LblVolumeProgress.Text = String.Format("{0} {1}{2}%", LblVolumeProgress.Text, (firstSets - lastSets) >= 0 ? "" : "", Math.Round(progressSets)).Replace("-", "");
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
                                levelUpBotModel.Type = BotType.LevelUp;

                                //Calculate week streak
                                if (workouts.ConsecutiveWeeks != null && workouts.ConsecutiveWeeks.Count > 0)
                                {
                                    var lastTime = workouts.ConsecutiveWeeks.Last();
                                    var year = Convert.ToString(lastTime.MaxWeek).Substring(0, 4);
                                    var weekOfYear = Convert.ToString(lastTime.MaxWeek).Substring(4, 2);
                                    CultureInfo myCI = new CultureInfo("en-US");
                                    Calendar cal = myCI.Calendar;

                                    if (int.Parse(year) == DateTime.Now.Year)
                                    {
                                        var currentWeekOfYear = cal.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                                        if (int.Parse(weekOfYear) >= currentWeekOfYear)
                                        {
                                            levelUpBotModel.ChainCount = Convert.ToString(lastTime.ConsecutiveWeeks);
                                        }

                                        else if (int.Parse(weekOfYear) == currentWeekOfYear - 1)
                                        {
                                            levelUpBotModel.ChainCount = Convert.ToString(lastTime.ConsecutiveWeeks);
                                        }
                                    }
                                }

                                var exerciseModel = workouts.HistoryExerciseModel;
                                if (exerciseModel != null)
                                {
                                    var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();
                                    var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                                    levelUpBotModel.WorkoutDone = exerciseModel.TotalWorkoutCompleted.ToString();
                                    levelUpBotModel.WorkoutDoneText = (exerciseModel.TotalWorkoutCompleted <= 1 ? $"Workout".ToLower().FirstCharToUpper() : $"Workouts").ToLower().FirstCharToUpper();

                                    SendCurrentProgramWorkoutDoneEvent(Convert.ToString(exerciseModel.TotalWorkoutCompleted));
                                    levelUpBotModel.LevelUpMessage = "N/A";
                                    levelUpBotModel.LevelUpText = $"Workouts\nbefore level up".ToLower().FirstCharToUpper();
                                    bool IsStrengthPhashe = false;
                                    try
                                    {


                                        var workouts1 = ((App)Application.Current).UserWorkoutContexts.workouts;
                                        int remainingWorkout = 0, totalworkout = 0;
                                        var name = "";
                                        if (workouts1 != null && workouts.GetUserProgramInfoResponseModel != null)
                                        {
                                            if (workouts1.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                                            {
                                                name = workouts1.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label;
                                                totalworkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RequiredWorkoutToLevelUp;
                                                remainingWorkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp != null ? (int)workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp : 0;
                                            }
                                        }


                                        IsStrengthPhashe = RecoComputation.IsInStrengthPhase(name, int.Parse(string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value) ? "40" : LocalDBManager.Instance.GetDBSetting("Age")?.Value), remainingWorkout, totalworkout);


                                    }
                                    catch (Exception ex)
                                    {

                                    }
                                    if (workouts.GetUserProgramInfoResponseModel != null)
                                    {
                                        upi = workouts.GetUserProgramInfoResponseModel;
                                        if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                                        {
                                            SetBottomCard();
                                            levelUpBotModel.LevelUpMessage = upi.RecommendedProgram.RemainingToLevelUp > 0 ? $"{(upi.RecommendedProgram.RequiredWorkoutToLevelUp - (upi.RecommendedProgram.RequiredWorkoutToLevelUp - upi.RecommendedProgram.RemainingToLevelUp)).ToString()}" : "N/A";
                                            var work = upi.RecommendedProgram.RemainingToLevelUp > 1 ? "WORKOUTS" : "WORKOUT";
                                            levelUpBotModel.LevelUpText = (upi.RecommendedProgram.RemainingToLevelUp > 0 ? IsStrengthPhashe ? $"{work}\nbefore level up\n(strength phase)" : $"{work}\nbefore level up" : "CUSTOM PROGRAM").ToLower().FirstCharToUpper();

                                            LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                                        }

                                    }
                                    else if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                                    {
                                        levelUpBotModel.LevelUpMessage = upi.RecommendedProgram.RemainingToLevelUp > 0 ? $"{(upi.RecommendedProgram.RequiredWorkoutToLevelUp - (upi.RecommendedProgram.RequiredWorkoutToLevelUp - upi.RecommendedProgram.RemainingToLevelUp)).ToString()}" : "N/A";
                                        var work = upi.RecommendedProgram.RemainingToLevelUp > 1 ? "WORKOUTS" : "WORKOUT";
                                        levelUpBotModel.LevelUpText = (upi.RecommendedProgram.RemainingToLevelUp > 0 ? IsStrengthPhashe ? $"{work}\nbefore level up\n(strength phase)" : $"{work}\nbefore level up" : "CUSTOM PROGRAM").ToLower().FirstCharToUpper();
                                        SendCurrentProgramWorkoutDoneEvent(Convert.ToString(exerciseModel.TotalWorkoutCompleted));
                                        LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                                    }
                                    _weightLifted = SO30180672.FormatNumber((long)weightLifted);
                                    _weightLiftedText = $"{unit} {AppResources.Lifted}".ToLower();
                                    levelUpBotModel.LbsLifted = _weightLifted;
                                    levelUpBotModel.LbsLiftedText = _weightLiftedText;
                                    strProgress += $"- {SO30180672.FormatNumber((long)weightLifted)} {unit} {AppResources.Lifted}";
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
                                //DateTime creatednDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                                //if ((DateTime.Now.ToUniversalTime() - creatednDate).TotalDays <= 14)
                                //{
                                //    if (!_weightLiftedText.ToLower().Contains("users"))
                                //        _weightLiftedText += " (users like you improve 34% in 30 days)".ToLower().FirstCharToUpper();
                                //}
                            }
                            catch (Exception)
                            {

                            }
                            //await AddQuestion(strProgress);
                            string fname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                            BotList.Clear();
                            BotList.Add(new BotModel()
                            {
                                Type = BotType.Congratulations,
                                Question = welcomeMsg
                            });
                            if (!IsEstimated)
                                BotList.Add(statsModel);
                            if (BotList.Count < 3)
                            {
                                BotList.Add(new BotModel()
                                {
                                    Type = BotType.Chart
                                });
                                SetChartData();
                                //BotList.Add(new BotModel()
                                //{
                                //    Type = BotType.WeightTracker
                                //});

                            }


                            if (levelUpBotModel.LevelUpText.ToLower().Contains("custom program"))
                            {
                                levelUpBotModel.LevelUpMessage = levelUpBotModel.WorkoutDone;
                                levelUpBotModel.LevelUpText = levelUpBotModel.WorkoutDoneText;
                            }
                            if (string.IsNullOrEmpty(levelUpBotModel.ChainCount))
                                levelUpBotModel.ChainCount = "0";
                            if (!string.IsNullOrEmpty(levelUpBotModel.LevelUpMessage))
                                BotList.Add(levelUpBotModel);
                            lblChainCount.Text = levelUpBotModel.ChainCount;
                            if (levelUpBotModel.ChainCount == "0" || levelUpBotModel.ChainCount == "0")
                                lblResult44.Text = "Week streak";
                            LblLevelUpMessage.Text = levelUpBotModel.LevelUpMessage;
                            LblLevelUpText.Text = levelUpBotModel.LevelUpText;
                            LblLifted.Text = levelUpBotModel.LbsLifted;
                            LblLiftedText.Text = levelUpBotModel.LbsLiftedText;



                            //await AddQuestion(strProgress);
                            GotIt_Clicked2(new DrMuscleButton() { Text = "" }, EventArgs.Empty);


                        }
                        else
                        {
                            try
                            {
                                BotList.Clear();
                                EmptyStateStack.IsVisible = false;

                                strengthBox.IsVisible = false;
                                volumeBox.IsVisible = false;
                                SecondProgressBox.IsVisible = false;
                                lstChats.IsVisible = false;
                                StackSteps1.IsVisible = false;
                                StackSteps2.IsVisible = false;
                                if (_isFirstDemoOpen)
                                {
                                    EmptyStateStack.IsVisible = false;
                                    StackSteps1.IsVisible = true;
                                    calendarBox1.Content = calendar;
                                }
                                else
                                    mainGrid.BackgroundColor = Color.FromHex("#f4f4f4");
                                //if (BotList.FirstOrDefault(x => x.Type == BotType.Congratulations) == null)
                                BotList.Add(new BotModel()
                                {
                                    Type = BotType.Congratulations,
                                    Question = welcomeMsg
                                });
                                BotList.Add(new BotModel()
                                {
                                    Type = BotType.Chart
                                });
                                SetChartData();
                                var exerciseModel = workouts.HistoryExerciseModel;
                                if (exerciseModel != null)
                                {
                                    if (exerciseModel.TotalWorkoutCompleted > 0)
                                    {
                                        EmptyStateStack.IsVisible = false;
                                        if (_isFirstDemoOpen)
                                        {
                                            StackSteps1.IsVisible = true;
                                            calendarBox1.Content = calendar;
                                        }
                                        else
                                        {
                                            StackSteps2.IsVisible = true;
                                            calendarBox2.Content = calendar;
                                        }
                                        mainGrid.BackgroundColor = Color.FromHex("#f4f4f4");
                                    }
                                    bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                                    var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();

                                    var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                                    //strProgress += exerciseModel.TotalWorkoutCompleted <= 1 ? $"- {exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutDone}" : $"- {exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutsDone}";
                                    levelUpBotModel.WorkoutDone = exerciseModel.TotalWorkoutCompleted.ToString();
                                    ///Holdinggs
                                    levelUpBotModel.WorkoutDoneText = exerciseModel.TotalWorkoutCompleted <= 1 ? $"{AppResources.WorkOut}".ToLower().FirstCharToUpper() : $"{AppResources.Workouts}".ToLower().FirstCharToUpper();
                                    levelUpBotModel.LevelUpMessage = "N/A";
                                    levelUpBotModel.LevelUpText = ($"Workouts\nbefore level up").ToLower().FirstCharToUpper();

                                    if (workouts.GetUserProgramInfoResponseModel == null)
                                    {
                                        workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                                    }

                                    if (workouts.GetUserProgramInfoResponseModel != null)
                                    {

                                        upi = workouts.GetUserProgramInfoResponseModel;
                                        if (upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                                        {
                                            SetBottomCard();
                                            bool IsStrengthPhashe = false;
                                            try
                                            {


                                                var workouts1 = ((App)Application.Current).UserWorkoutContexts.workouts;
                                                int remainingWorkout = 0, totalworkout = 0;
                                                var name = "";
                                                if (workouts1 != null && workouts.GetUserProgramInfoResponseModel != null)
                                                {
                                                    if (workouts1.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                                                    {
                                                        name = workouts1.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label;
                                                        totalworkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RequiredWorkoutToLevelUp;
                                                        remainingWorkout = workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp != null ? (int)workouts1.GetUserProgramInfoResponseModel.RecommendedProgram.RemainingToLevelUp : 0;
                                                    }
                                                }


                                                IsStrengthPhashe = RecoComputation.IsInStrengthPhase(name, int.Parse(string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value) ? "40" : LocalDBManager.Instance.GetDBSetting("Age")?.Value), remainingWorkout, totalworkout);
                                                _isInStrengthPhase = IsStrengthPhashe;


                                            }
                                            catch (Exception ex)
                                            {

                                            }

                                            levelUpBotModel.LevelUpMessage = upi.RecommendedProgram.RemainingToLevelUp > 0 ? $"{(upi.RecommendedProgram.RequiredWorkoutToLevelUp - (upi.RecommendedProgram.RequiredWorkoutToLevelUp - upi.RecommendedProgram.RemainingToLevelUp)).ToString()}" : "N/A";
                                            var work = upi.RecommendedProgram.RemainingToLevelUp > 1 ? "WORKOUTS" : "WORKOUT";
                                            levelUpBotModel.LevelUpText = (upi.RecommendedProgram.RemainingToLevelUp > 0 ? IsStrengthPhashe ? $"{work}\nbefore level up\n(strength phase)" : $"{work}\nbefore level up" : "CUSTOM PROGRAM").ToLower().FirstCharToUpper();
                                            //strProgress += $"\n- {upi.RecommendedProgram.RemainingToLevelUp} {AppResources.WorkoutsBeforeYouLevelUp}";
                                            LocalDBManager.Instance.SetDBSetting("remain", upi.RecommendedProgram.RemainingToLevelUp.ToString());
                                        }
                                    }
                                    if (string.IsNullOrEmpty(levelUpBotModel.ChainCount))
                                        levelUpBotModel.ChainCount = "N/A";

                                    //strProgress += $"- {weightLifted.ToString("N0")} {unit} {AppResources.Lifted}";
                                    _weightLifted = SO30180672.FormatNumber((long)weightLifted);
                                    _weightLiftedText = $"{unit} {AppResources.Lifted}".ToLower();
                                    levelUpBotModel.LbsLifted = _weightLifted;
                                    levelUpBotModel.LbsLiftedText = _weightLiftedText;
                                    if (levelUpBotModel.LevelUpText.ToLower().Contains("custom program"))
                                    {
                                        levelUpBotModel.LevelUpMessage = levelUpBotModel.WorkoutDone;
                                        levelUpBotModel.LevelUpText = levelUpBotModel.WorkoutDoneText;
                                    }



                                    if (workouts != null && workouts.ConsecutiveWeeks != null && workouts.ConsecutiveWeeks.Count > 0)
                                    {
                                        var lastTime = workouts.ConsecutiveWeeks.Last();
                                        var year = Convert.ToString(lastTime.MaxWeek).Substring(0, 4);
                                        var weekOfYear = Convert.ToString(lastTime.MaxWeek).Substring(4, 2);
                                        CultureInfo myCI = new CultureInfo("en-US");
                                        Calendar cal = myCI.Calendar;

                                        if (int.Parse(year) == DateTime.Now.Year)
                                        {
                                            var currentWeekOfYear = cal.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                                            if (int.Parse(weekOfYear) >= currentWeekOfYear)
                                            {
                                                levelUpBotModel.ChainCount = Convert.ToString(lastTime.ConsecutiveWeeks);
                                            }

                                            else if (int.Parse(weekOfYear) == currentWeekOfYear - 1)
                                            {
                                                levelUpBotModel.ChainCount = Convert.ToString(lastTime.ConsecutiveWeeks);
                                            }
                                        }
                                    }


                                    BotList.Add(levelUpBotModel);

                                    lblChainCount.Text = levelUpBotModel.ChainCount;
                                    if (levelUpBotModel.ChainCount == "0" || levelUpBotModel.ChainCount == "0")
                                        lblResult44.Text = "Week streak";
                                    LblLevelUpMessage.Text = levelUpBotModel.LevelUpMessage;
                                    LblLevelUpText.Text = levelUpBotModel.LevelUpText;
                                    LblLifted.Text = levelUpBotModel.LbsLifted;
                                    LblLiftedText.Text = levelUpBotModel.LbsLiftedText;

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
                                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value, IsSystemExercise = true },
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
                        await WorkoutLogSets(false);
                        workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    }
                }


                stackOptions.Children.Clear();
                //var btn = new DrMuscleButton()
                //{
                //    Text = "View more stats",
                //    TextColor = Color.FromHex("#195377"),
                //    BackgroundColor = Color.Transparent,
                //};
                //btn.Clicked += GotoMePage_Click;
                //stackOptions.Children.Add(btn);
                Xamarin.Forms.MessagingCenter.Send<WorkoutLoadedMessage>(new WorkoutLoadedMessage(), "WorkoutLoadedMessage");
                await AddOptionsWithoutScroll("Workout loaded (Home)", MoveToHome);
                //_isAnyWorkoutFinished = true;
                if (BotList.Count > 0)
                {


                    if (Device.iOS == Device.RuntimePlatform)
                    {
                        //await Task.Delay(300);
                        //lstChats.ScrollTo(BotList.First(), ScrollToPosition.Start, false);

                        //await Task.Delay(1000);
                        //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, true);
                        //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, true);

                    }
                    else
                    {
                        //await Task.Delay(300);
                        //lstChats.ScrollTo(BotList.First(), ScrollToPosition.MakeVisible, false);
                        //lstChats.ScrollTo(BotList.First(), ScrollToPosition.Start, false);
                        //Device.BeginInvokeOnMainThread(async () =>
                        //{

                        //    await Task.Delay(1500);
                        //    if (BotList.Count>2)
                        //    {
                        //        lstChats.ScrollTo(BotList[BotList.Count-2], ScrollToPosition.End, false);
                        //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                        //    }

                        //});
                    }

                }



            }
            catch (Exception ex)
            {
                Xamarin.Forms.MessagingCenter.Send<WorkoutLoadedMessage>(new WorkoutLoadedMessage(), "WorkoutLoadedMessage");

            }
        }
        private void MoveToHome(object sender, EventArgs e)
        {
            this.ToolbarItems.Clear();
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralHomeAction, ToolbarItemOrder.Primary, 0);
            //this.ToolbarItems.Add(generalToolbarItem);
            //StrengthArrowText.Text = "";
            StrengthArrowImage.Source = "";
            LblStrengthProgress.Text = "";
            //VolumeArrowText.Text = "";
            VolumeArrowImage.Source = "";
            LblVolumeProgress.Text = "";
            SecondWelcomeBox.IsVisible = true;
            StartSetup();
            lstChats.IsVisible = false;
        }
        async void GotoMePage_Click(object sender, EventArgs e)
        {
            //
            await PagesFactory.PushAsync<MeCombinePage>();

        }
        private async void BtnShareMonth_Clicked(object sender, EventArgs e)
        {
            var firstname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
            if (Device.RuntimePlatform.Equals(Device.Android))
            {

                await Share.RequestAsync(new ShareTextRequest
                {
                    Uri = $"https://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=home&utm_content={firstname}",
                    Subject = $"{firstname} is inviting you to try Dr.Muscle, the app that gets you in shape faster using artificial intelligence"
                });
            }
            else
                await Xamarin.Essentials.Share.RequestAsync($"{firstname} is inviting you to try Dr.Muscle, the app that gets you in shape faster using artificial intelligence \nhttps://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=home&utm_content={firstname}");
            DependencyService.Get<IFirebase>().LogEvent("shared_home_page", "share");

        }
        async void BtnCustomize_Clicked(object sender, EventArgs e)
        {
            stackOptions.Children.Clear();
            await AddOptions("Choose another workout", BtnChooseAnother_Clicked);
            await AddOptions("Short on time or tired today", BtnFeelingWeekShortOnTime_Clicked);
            // await AddOptions("Email a human", BtnEmailAHuman_Clicked);
            await AddOptions("Back", BtnBack_Clicked);

            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Tabs[0].BadgeCaption = 0;
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
                //var btn = new DrMuscleButton()
                //{
                //    Text = "Share 1 month free",
                //    TextColor = Color.FromHex("#195377"),
                //    BackgroundColor = Color.Transparent,
                //    HeightRequest = 45
                //};
                //btn.Clicked += BtnShareMonth_Clicked;
                //stackOptions.Children.Add(btn);

                //var btn1 = new DrMuscleButton()
                //{
                //    Text = "More options",
                //    TextColor = Color.FromHex("#195377"),
                //    BackgroundColor = Color.Transparent,
                //    HeightRequest = 55,
                //    BorderWidth = 2,
                //    BorderColor = AppThemeConstants.BlueColor,
                //    Margin = new Thickness(25, 0)
                //};
                //btn1.Clicked += BtnCustomize_Clicked;
                //stackOptions.Children.Add(btn1);

                if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)

                {
                    _lblWorkoutName = new Label() { Text = _isReload ? "" : $"{upi.NextWorkoutTemplate.Label}", HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, FontSize = 17, TextColor = Color.Black, FontAttributes = FontAttributes.Bold };
                    var startworkoutText = "START WORKOUT";
                    var previewWorkout = "PREVIEW NEXT WORKOUT";
                    if (LocalDBManager.Instance.GetDBSetting($"AnySets{DateTime.Now.Date}")?.Value
                         == "1")
                    {
                        previewWorkout = "RESUME WORKOUT";
                        startworkoutText = "RESUME WORKOUT";
                    }

                    workoutNameLabel.Text = $"{upi.NextWorkoutTemplate.Label} is next";
                    workoutNameLabel2.Text = workoutNameLabel.Text;
                    await AddOptions(_isAnyWorkoutFinished ? $"{previewWorkout}\n{upi.NextWorkoutTemplate.Label}" : $"{startworkoutText}\n{upi.NextWorkoutTemplate.Label}", BtnStartTodayWorkout_Clicked);
                    BtnCardStartWorkout.Text = _isAnyWorkoutFinished ? $"{previewWorkout}" : $"{startworkoutText}";
                    BtnWelcomeStartWorkout.Text = BtnCardStartWorkout.Text;
                    btnstsrtWorkoutTitle.Text = BtnCardStartWorkout.Text;
                    //stackOptions.Children.Add(_lblWorkoutName);
                    SendCurrentProgramEvent(upi.RecommendedProgram.Label);
                    //var rawResponse = await res.Content.ReadAsStringAsync();
                }
                else
                    await AddOptions("START PLANNED WORKOUT", BtnStartTodayWorkout_Clicked);


            }
            catch (Exception ex)
            {

            }
        }

        private async void SendCurrentProgramEvent(string events)
        {
            var platfromInfo = $"{DeviceInfo.Name}_{DeviceInfo.Platform}_{DeviceInfo.VersionString}";
            //try
            //{
            //    var dict1 = new Dictionary<string, string>();
            //    dict1.Add("api_key", "NFVPvOFV83vWXyl8iqrOvmn6t3OAJMNK");
            //    dict1.Add("action", "update_contact");
            //    var contactUpdatePayload = "{\"email\":\"" + "jmxd9dkjqd@privaterelay.appleid.com" + "\",\"project_id\":1339,\"cf_billingcycle_2352\":\"" + "Monthly" + "\",\"cf_phone_2354\":\"" +  "iPhone" + "\",\"cf_paymentmethod_2353\":\"" +  "Apple" + "\"}";
            //    dict1.Add("value", contactUpdatePayload);
            //    var client1 = new HttpClient();
            //    var req1 = new HttpRequestMessage(HttpMethod.Post, "https://api.platform.ly/") { Content = new FormUrlEncodedContent(dict1) };
            //    await client1.SendAsync(req1);
            //}
            //catch (Exception ex)
            //{

            //}
            try
            {

                var dict = new Dictionary<string, string>();
                dict.Add("api_key", "NFVPvOFV83vWXyl8iqrOvmn6t3OAJMNK");
                dict.Add("action", "add_contact");
                var contactUpdatePayload = "{\"email\":\"" + LocalDBManager.Instance.GetDBSetting("email")?.Value + "\",\"first_name\":\"" + LocalDBManager.Instance.GetDBSetting("firstname")?.Value + "\",\"project_id\":1339,\"cf_userprogram_2397\":\"" + events + "\",\"cf_phonesystem_2501\":\"" + platfromInfo + "\"}";
                dict.Add("value", contactUpdatePayload);
                var client = new HttpClient();//
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.platform.ly/") { Content = new FormUrlEncodedContent(dict) };

                var res = await client.SendAsync(req);


            }
            catch (Exception ex)
            {

            }

            try
            {

                var dict = new Dictionary<string, string>();
                dict.Add("api_key", "NFVPvOFV83vWXyl8iqrOvmn6t3OAJMNK");
                dict.Add("action", "update_contact");
                var contactUpdatePayload = "{\"email\":\"" + LocalDBManager.Instance.GetDBSetting("email")?.Value + "\",\"project_id\":1339,\"cf_userprogram_2397\":\"" + events + "\",\"cf_phonesystem_2501\":\"" + platfromInfo + "\"}";
                dict.Add("value", contactUpdatePayload);
                var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.platform.ly/") { Content = new FormUrlEncodedContent(dict) };

                var res = await client.SendAsync(req);

            }
            catch (Exception ex)
            {

            }
        }

        private async void SendCurrentProgramWorkoutDoneEvent(string events)
        {
            try
            {

                var dict = new Dictionary<string, string>();
                dict.Add("api_key", "NFVPvOFV83vWXyl8iqrOvmn6t3OAJMNK");
                dict.Add("action", "update_contact");
                var contactUpdatePayload = "{\"email\":\"" + LocalDBManager.Instance.GetDBSetting("email")?.Value + "\",\"project_id\":1339,\"cf_workoutsdone_2398\":\"" + events + "\"}";
                dict.Add("value", contactUpdatePayload);
                var client = new HttpClient();
                var req = new HttpRequestMessage(HttpMethod.Post, "https://api.platform.ly/") { Content = new FormUrlEncodedContent(dict) };

                var res = await client.SendAsync(req);

            }
            catch (Exception ex)
            {

            }
        }
        private void UpdateBodyWeight()
        {
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            {
                var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                var weight1 = new MultiUnityWeight(value, "kg");
                LblBodyweight.Text = string.Format("{0:0.##}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weight1.Kg : weight1.Lb);
            }
            LoadSavedWeightFromServer();
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
                        bool IsInserted = false;
                        //await AddAnswer(AppResources.GotIt);
                        var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                        //Today workout
                        TimeSpan timeSpan;
                        String dayStr = "days";
                        int days = 0;
                        int hours = 0;
                        int minutes = 0;

                        CurrentLog.Instance.IsRest = false;
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
                        var lifted = new BotModel();
                        lifted.Type = BotType.Lifted;

                        if (!string.IsNullOrEmpty(_weightLifted))
                        {
                            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            {
                                var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                                var weight1 = new MultiUnityWeight(value, "kg");
                                lifted.LbsLifted = string.Format("{0:0.##}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weight1.Kg : weight1.Lb);
                            }
                            else
                                lifted.LbsLifted = "N/A";//_weightLifted;
                            lifted.LbsLiftedText = "Body weight";
                        }
                        //var restModel = new BotModel();
                        //restModel.Type = BotType.RestRecovered;

                        if (workouts.LastConsecutiveWorkoutDays > 1 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 2 && workouts != null && workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                        {
                            var RequiredHours = 18;
                            if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bodyweight") ||
workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("mobility") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("powerlifting") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("full-body") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bands"))
                            {
                                RequiredHours = 42;
                            }
                            else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] legs") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] legs"))

                            {
                                RequiredHours = 18;
                                if (workouts.LastConsecutiveWorkoutDays > 5 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 3)
                                {
                                    RequiredHours = 42;
                                }
                            }
                            else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("split"))
                            {
                                RequiredHours = 42;
                            }

                            if (hours < RequiredHours)
                            {
                                lifted.StrengthTextColor = AppThemeConstants.DarkRedColor;
                                var h = RequiredHours - hours <= 1 ? "hour" : "hours";
                                lifted.TrainRest = "Rest";
                                CurrentLog.Instance.IsRest = true;
                                lifted.SinceTime = $"{hours}/{RequiredHours} {h}";
                                lifted.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                                _isAnyWorkoutFinished = true;
                                //lifted.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();
                                lifted.TrainRestText = "Coach says";// "Fatigued";//$"At least {RequiredHours - hours} {h} more to recover".ToLower().FirstCharToUpper();

                            }
                            else
                            {
                                lifted.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                                _isAnyWorkoutFinished = false;
                                lifted.StrengthTextColor = AppThemeConstants.GreenColor;
                                var h = RequiredHours - hours <= 1 ? "hour" : "hours";
                                lifted.TrainRest = "Train";
                                lifted.SinceTime = $"{hours}/{RequiredHours} {h}"; //hours.ToString();
                                //lifted.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();
                                lifted.TrainRestText = $"Coach says";

                            }
                            BotList.Add(lifted);
                            //BotList.Add(restModel);
                            IsInserted = true;
                            // await AddQuestion($"{AppResources.YouHaveBeenWorkingOut} {workouts.LastConsecutiveWorkoutDays} {AppResources.DaysInARowISuggestTalkingADayOffAreYouSureYouWantToWorkOutToday} Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                        }
                        else if (workouts.LastWorkoutDate != null)
                        {
                            if (days > 0 && hours >= 42)
                            {
                                lifted.SinceTime = $"{days} {dayStr}";
                                lifted.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                                _isAnyWorkoutFinished = false;
                                lifted.TrainRest = "Train";
                                lifted.StrengthTextColor = AppThemeConstants.GreenColor;
                                lifted.TrainRestText = "Coach says";// "Recovered";// (days > 9 ? "I may recommend lighter weights" : "You should have recovered").ToLower().FirstCharToUpper();
                                BotList.Add(lifted);
                                //BotList.Add(restModel);
                                IsInserted = true;
                                //await AddQuestion(days > 9 ? $"{AppResources.YourLastWorkoutWas} {days} {dayStr} ago. I may recommend a light session. Start planned workout?" : $"Your last workout was {days} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                            }
                            else if (hours > 0)
                            {
                                var RequiredHours = 18;
                                if (workouts != null && workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                                {
                                    if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bodyweight") ||
workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("mobility") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("powerlifting") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("full-body") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bands"))
                                    {
                                        RequiredHours = 42;
                                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                                        {
                                            if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) < 30)
                                                RequiredHours = 18;
                                        }
                                        if (workouts.LastConsecutiveWorkoutDays > 1 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 2)
                                            RequiredHours = 42;
                                    }
                                    else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("split"))
                                    {
                                        RequiredHours = 18;
                                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                                        {
                                            if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) > 50)
                                                RequiredHours = 42;
                                        }
                                    }
                                }
                                if (hours < RequiredHours)
                                {

                                    lifted.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                                    lifted.StrengthTextColor = AppThemeConstants.DarkRedColor;
                                    var h = RequiredHours - hours <= 1 ? "hour" : "hours";
                                    lifted.TrainRest = "Rest";
                                    _isAnyWorkoutFinished = true;
                                    CurrentLog.Instance.IsRest = true;
                                    lifted.SinceTime = $"{hours}/{RequiredHours} {h}"; //hours.ToString();
                                    //lifted.TrainRestText = $"More to recover".ToLower().FirstCharToUpper();
                                    lifted.TrainRestText = "Coach says";// "Fatigued";//$"At least {RequiredHours - hours} {h} more to recover".ToLower().FirstCharToUpper();

                                    BotList.Add(lifted);
                                    //BotList.Add(restModel);
                                    IsInserted = true;
                                    //await AddQuestion($"Your last workout was {hours} {dayStr} ago. I'm not sure it makes sense to work out again now... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                }
                                else
                                {
                                    var h = hours <= 1 ? "hour" : "hours";
                                    lifted.SinceTime = $"{hours}/{RequiredHours} {h}";
                                    lifted.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                                    lifted.TrainRest = "Train";
                                    _isAnyWorkoutFinished = false;
                                    lifted.StrengthTextColor = AppThemeConstants.GreenColor;
                                    lifted.TrainRestText = "Coach says";// "Recovered";// "You should have recovered".ToLower().FirstCharToUpper();
                                    BotList.Add(lifted);
                                    //BotList.Add(restModel);
                                    IsInserted = true;
                                    // await AddQuestion($"Your last workout was {hours} {dayStr} ago. You should have recovered. Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                                }

                            }
                            else
                            {
                                var RequiredHours = 18;
                                if (workouts != null && workouts.GetUserProgramInfoResponseModel != null && workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                                {
                                    if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bodyweight") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("mobility") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("powerlifting") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("full-body") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("bands"))
                                    {
                                        RequiredHours = 42;
                                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                                        {
                                            if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) < 30)
                                                RequiredHours = 18;
                                        }
                                        if (workouts.LastConsecutiveWorkoutDays > 1 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 2)
                                            RequiredHours = 42;
                                    }
                                    else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[home] legs") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] push") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] pull") || workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("[gym] legs"))
                                    {
                                        RequiredHours = 18;
                                        if (workouts.LastConsecutiveWorkoutDays > 5 && workouts.LastWorkoutDate != null && (DateTime.Now - ((DateTime)workouts.LastWorkoutDate).ToLocalTime()).TotalDays < 3)

                                        {
                                            RequiredHours = 42;
                                        }
                                    }
                                    else if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label.ToLower().Contains("split"))
                                    {
                                        RequiredHours = 18;
                                        if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("Age")?.Value))
                                        {
                                            if (int.Parse(LocalDBManager.Instance.GetDBSetting("Age")?.Value) > 50)
                                                RequiredHours = 42;
                                        }

                                    }
                                }
                                lifted.SinceTime = $"0/{RequiredHours} hours";//minutes.ToString();
                                lifted.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                                lifted.TrainRest = "Rest";
                                _isAnyWorkoutFinished = true;
                                CurrentLog.Instance.IsRest = true;
                                lifted.StrengthTextColor = AppThemeConstants.DarkRedColor;
                                lifted.TrainRestText = "Coach says";// "Fatigued";//$"{RequiredHours} hours more to recover".ToLower().FirstCharToUpper();

                                BotList.Add(lifted);
                                //BotList.Add(restModel);
                                IsInserted = true;
                                //await AddQuestion($"Your last workout was {minutes} {dayStr} ago. I'm not sure it makes sense to work out again today... Up next is {workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.Label}.");
                            }
                        }

                        if (!IsInserted)
                        {

                            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            {
                                var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                                var weight1 = new MultiUnityWeight(value, "kg");
                                lifted.LbsLifted = string.Format("{0:0.##}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weight1.Kg : weight1.Lb);
                            }
                            else
                                lifted.LbsLifted = "N/A";//_weightLifted;
                            lifted.LbsLiftedText = "Body weight";

                            lifted.SinceTime = "N/A";
                            lifted.LastWorkoutText = $"recovery".ToLower().FirstCharToUpper();
                            lifted.TrainRest = "Train";
                            _isAnyWorkoutFinished = false;
                            lifted.StrengthTextColor = AppThemeConstants.GreenColor;
                            lifted.TrainRestText = "Coach says";// "Recovered";// "No workout yet".ToLower().FirstCharToUpper();
                            BotList.Add(lifted);
                            //BotList.Add(restModel);
                        }

                        LblBodyweight.Text = lifted.LbsLifted;
                        LblBodyweightText.Text = lifted.LbsLiftedText;
                        LblSinceTIme.Text = lifted.SinceTime;
                        LblLastworkoutText.Text = lifted.LastWorkoutText;
                        TrainRestImage.Source = lifted.TrainRest == "Train" ? "green.png" : "orange2.png";
                        LblTrainRest.Text = lifted.TrainRest;
                        LblTrainRestText.Text = lifted.TrainRestText;
                        LblTrainRest.TextColor = lifted.StrengthTextColor;
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
                                        NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value, IsSystemExercise = true },
                                        RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                                    };
                                    workouts.GetUserProgramInfoResponseModel = upi;
                                    ((App)Application.Current).UserWorkoutContexts.workouts = workouts;
                                    ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                                    SetBottomCard();
                                    LoadCurrentWorkoutTemplate();
                                    LoadUpcomingDays();
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

                if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                {
                    _lblWorkoutName = new Label() { Text = _isReload ? "" : $"{upi.NextWorkoutTemplate.Label}", HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, FontSize = 17, TextColor = Color.Black, FontAttributes = FontAttributes.Bold };
                    var startworkoutText = "START WORKOUT";
                    var previewWorkout = "PREVIEW NEXT WORKOUT";
                    if (LocalDBManager.Instance.GetDBSetting($"AnySets{DateTime.Now.Date}")?.Value
                         == "1")
                    {
                        previewWorkout = "RESUME WORKOUT";
                        startworkoutText = "RESUME WORKOUT";
                    }

                    workoutNameLabel.Text = $"{upi.NextWorkoutTemplate.Label} is next";
                    workoutNameLabel2.Text = workoutNameLabel.Text;
                    btnStartWorkout = await AddOptions(_isAnyWorkoutFinished ? $"{previewWorkout}\n{upi.NextWorkoutTemplate.Label}" : $"{startworkoutText}\n{upi.NextWorkoutTemplate.Label}", BtnStartTodayWorkout_Clicked);
                    BtnCardStartWorkout.Text = _isAnyWorkoutFinished ? $"{previewWorkout}" : $"{startworkoutText}";
                    BtnWelcomeStartWorkout.Text = BtnCardStartWorkout.Text;
                    btnstsrtWorkoutTitle.Text = BtnCardStartWorkout.Text;
                    //stackOptions.Children.Add(_lblWorkoutName);
                    SendCurrentProgramEvent(upi.RecommendedProgram.Label);
                }
                else
                    btnStartWorkout = await AddOptions("Start planned workout".ToUpper(), BtnStartTodayWorkout_Clicked);

                if (BotList.Count == 3)
                {
                    await Task.Delay(200);

                    lstChats.ScrollTo(BotList.First(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.First(), ScrollToPosition.End, false);
                }
                if (App.IsNewUser)
                {
                    await Task.Delay(500);
                    Grid grid = (Xamarin.Forms.Grid)btnStartWorkout.Parent;
                    TooltipEffect.SetPosition(grid, TooltipPosition.Top);
                    TooltipEffect.SetBackgroundColor(grid, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(grid, Color.White);
                    //TooltipEffect.SetText(grid, $"Tap here to {btnStartWorkout.Text}");
                    TooltipEffect.SetText(grid, $"Tap me");
                    //
                    TooltipEffect.SetHasTooltip(grid, true);
                }
                //if(_isEmpty)
                //{
                //    TooltipEffect.SetHasShowTooltip((Xamarin.Forms.Grid)btnStartWorkout.Parent, true);
                //}
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
            await AddOptions("I've changed my mind", BtnChangeMyMinde_Clicked);

        }
        private async Task LoadWorkedOutDays()
        {
            //LoadWorkedOutDays
            
           
        }
        private void RecommendedReminderMode()
        {
            

            try
            {
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;

                if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                {


                    //If Default and Age
                    int age = 40;
                    bool isApproxAge = true;
                    if (LocalDBManager.Instance.GetDBSetting("Age") != null && LocalDBManager.Instance.GetDBSetting("Age").Value != null)
                    {
                        age = int.Parse(LocalDBManager.Instance.GetDBSetting("Age").Value);
                    }

                    if (workouts.Sets != null)
                    {

                    }
                    else
                    {
                        workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    }

                    var programName = workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label;
                    var ageText = "";
                    var xDays = 0;
                    if (programName.ToLower().Contains("push/pull/legs") && age < 51)
                    {
                        xDays = 6;
                    }
                    else if (programName.ToLower().Contains("split"))
                    {
                        if (age < 30)
                            xDays = 4;
                        else if (age >= 30 && age <= 50)
                            xDays = 4;
                        else
                            xDays = 3;
                    }
                    else if (programName.ToLower().Contains("bodyweight") ||
    programName.ToLower().Contains("mobility") || programName.ToLower().Contains("full-body") || programName.ToLower().Contains("bands") || programName.ToLower().Contains("powerlifting"))
                    {
                        if (age < 30)
                            xDays = 4;
                        else if (age >= 30 && age <= 50)
                            xDays = 3;
                        else
                            xDays = 2;
                    }

                    if (xDays != 0)
                    {
                        _recommendeddays = xDays;
                        //
                        //if (xDays == 2)
                        //{
                        //    LblReminderDesc.Text = $"Monday, and Thursday.";

                        //}
                        //if (xDays == 3)
                        //{
                        //    LblReminderDesc.Text = $"Monday, Wednesday, and Friday.";
                        //}
                        //if (xDays == 4)
                        //{
                        //    LblReminderDesc.Text = $"Monday, Tuesday ,Thursday, and Friday.";
                        //}
                        //if (xDays == 5)
                        //{
                        //    LblReminderDesc.Text = $"Monday, Tuesday, Wednesday, Thursday, and Friday.";
                        //}
                        //if (xDays == 6)
                        //{
                        //    LblReminderDesc.Text = $"Monday, Tuesday, Wednesday, Friday, Saturday, and Sunday";
                        //}
                    }
                }
                else
                {
                    _recommendeddays = 7;
                }

            }
            catch (Exception ex)
            {

            }

        }

        private async Task LoadUpcomingDays()
        {
            //LoadWorkedOutDays
            try
            {
                //var logDates = await DrMuscleRestClient.Instance.GetUserWorkoutLogDate();
                foreach (var item in eventCollection.Where(x => x.Key >= DateTime.Now.Date).ToList())
                {
                    foreach (EventModel val in item.Value)
                    {
                        if (!val.IsPast)
                            eventCollection.Remove(item.Key);
                    }
                }
                if (workoutLogAverage != null)
                {
                    bool IsSunday = false;
                    bool IsMonday = false;
                    bool IsTuesday = false;
                    bool IsWednesday = false;
                    bool IsThursday = false;
                    bool IsFriday = false;
                    bool IsSaturday = false;
                    var recommended = workoutLogAverage?.GetUserProgramInfoResponseModel?.RecommendedProgram;
                    var nextWorkout = workoutLogAverage?.GetUserProgramInfoResponseModel?.NextWorkoutTemplate;
                    if (recommended == null || nextWorkout == null || recommended?.WorkoutTemplates?.Count == 0)
                        return;
                    var templadteCount = recommended.WorkoutTemplates.Count;
                    var currentIndex = recommended.WorkoutTemplates.IndexOf(recommended.WorkoutTemplates.FirstOrDefault(x => x.Id == nextWorkout.Id));
                    var todayDate = DateTime.Now.Date;
                    var currentDate = DateTime.Now.Date;
                    int remainingToLevelUp = recommended.RemainingToLevelUp != null ? (int)recommended.RemainingToLevelUp : 0;
                    if (!recommended.IsSystemExercise && recommended.RemainingToLevelUp == 0 && recommended.RequiredWorkoutToLevelUp == 0)
                        remainingToLevelUp = 30;
                    if (workoutLogAverage.LastWorkoutDate?.Date == DateTime.Now.Date)
                    {
                        todayDate = todayDate.AddDays(1);
                    }
                    if (LocalDBManager.Instance.GetDBSetting("RecommendedReminder")?.Value == "true" || LocalDBManager.Instance.GetDBSetting("RecommendedReminder")?.Value == null || LocalDBManager.Instance.GetDBSetting("ReminderDays")?.Value == null ||  LocalDBManager.Instance.GetDBSetting("ReminderDays")?.Value == "0000000")
                    {
                        RecommendedReminderMode();
                        
                        
                        int i = 0;
                        if (_recommendeddays == 2)
                        {
                            IsMonday = true;
                            IsThursday = true;
                            //LblReminderDesc.Text = $"Monday, and Thursday.";

                        }
                        if (_recommendeddays == 3)
                        {
                            IsMonday = true;
                            IsWednesday = true;
                            IsFriday = true;
                            // LblReminderDesc.Text = $"Monday, Wednesday, and Friday.";
                        }
                        if (_recommendeddays == 4)
                        {
                            IsMonday = true;
                            IsTuesday = true;
                            IsThursday = true;
                            IsFriday = true;
                            //LblReminderDesc.Text = $"Monday, Tuesday ,Thursday, and Friday.";
                        }
                        if (_recommendeddays == 5)
                        {
                            IsMonday = true;
                            IsTuesday = true;
                            IsWednesday = true;
                            IsThursday = true;
                            IsFriday = true;
                            // LblReminderDesc.Text = $"Monday, Tuesday, Wednesday, Thursday, and Friday.";
                        }
                        if (_recommendeddays == 6)
                        {
                            IsMonday = true;
                            IsTuesday = true;
                            IsWednesday = true;
                            IsFriday = true;
                            IsSaturday = true;
                            IsSunday = true;
                            //LblReminderDesc.Text = $"Monday, Tuesday, Wednesday, Friday, Saturday, and Sunday";
                        }
                        if (_recommendeddays == 7)
                        {
                            IsMonday = true;
                            IsTuesday = true;
                            IsWednesday = true;
                            IsThursday = true;
                            IsFriday = true;
                            IsSaturday = true;
                            IsSunday = true;
                        }
                        var dayofweek = todayDate.DayOfWeek;
                        while (i <= remainingToLevelUp)
                        {

                            var day = 0;

                            if (IsMonday)
                            {
                                if (DayOfWeek.Monday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                {

                                }
                                else
                                {
                                    if (DayOfWeek.Monday - todayDate.DayOfWeek < 0)
                                    {
                                        day =  (DayOfWeek.Monday - todayDate.DayOfWeek);
                                    }
                                    else
                                    {
                                        day = DayOfWeek.Monday - todayDate.DayOfWeek;
                                    }
                                    var timeSpan = new TimeSpan(day, 0, 0, 0);
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    var template = recommended.WorkoutTemplates[currentIndex];
                                    if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                    {
                                        eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                        i++;
                                        if (i >= remainingToLevelUp)
                                            break;
                                        currentIndex++;
                                    }
                                }
                                
                            }
                            if (IsTuesday)
                            {
                                if (DayOfWeek.Tuesday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                {

                                }
                                else
                                {
                                    if (DayOfWeek.Tuesday - todayDate.DayOfWeek < 0)
                                    {
                                        day =  (DayOfWeek.Tuesday - todayDate.DayOfWeek);
                                    }
                                    else
                                    {
                                        day = DayOfWeek.Tuesday - todayDate.DayOfWeek;
                                    }
                                    var timeSpan = new TimeSpan(day, 0, 0, 0);
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    var template = recommended.WorkoutTemplates[currentIndex];
                                    if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                    {
                                        eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                        i++;
                                        if (i >= remainingToLevelUp)
                                            break;
                                        currentIndex++;
                                    }
                                }
                                   
                            }
                            if (IsWednesday)
                            {
                                if (DayOfWeek.Wednesday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                {

                                }
                                else
                                {
                                    if ((DayOfWeek.Wednesday - todayDate.DayOfWeek) < 0)
                                    {
                                        day =  (DayOfWeek.Wednesday - todayDate.DayOfWeek);
                                    }
                                    else
                                    {
                                        day = DayOfWeek.Wednesday - todayDate.DayOfWeek;
                                    }
                                    var timeSpan = new TimeSpan(day, 0, 0, 0);
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    var template = recommended.WorkoutTemplates[currentIndex];
                                    if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                    {
                                        eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                        i++;
                                        if (i >= remainingToLevelUp)
                                            break;
                                        currentIndex++;
                                    }
                                }
                            }
                            if (IsThursday)
                            {
                                if (DayOfWeek.Thursday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                {

                                }
                                else
                                {
                                    if (DayOfWeek.Thursday - todayDate.DayOfWeek < 0)
                                    {
                                        day =  (DayOfWeek.Thursday - todayDate.DayOfWeek);
                                    }
                                    else
                                    {
                                        day = DayOfWeek.Thursday - todayDate.DayOfWeek;
                                    }
                                    var timeSpan = new TimeSpan(day, 0, 0, 0);
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    var template = recommended.WorkoutTemplates[currentIndex];
                                    if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                    {
                                        eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                        i++;
                                        if (i >= remainingToLevelUp)
                                            break;
                                        currentIndex++;
                                    }
                                }
                            }
                            if (IsFriday)
                            {
                                if (DayOfWeek.Friday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                {

                                }
                                else
                                {
                                    if (DayOfWeek.Friday - todayDate.DayOfWeek < 0)
                                    {
                                        day = (DayOfWeek.Friday - todayDate.DayOfWeek);
                                    }
                                    else
                                    {
                                        day = DayOfWeek.Friday - todayDate.DayOfWeek;
                                    }
                                    var timeSpan = new TimeSpan(day, 0, 0, 0);
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    var template = recommended.WorkoutTemplates[currentIndex];
                                    if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                    {
                                        eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                        i++;
                                        if (i >= remainingToLevelUp)
                                            break;
                                        currentIndex++;
                                    }
                                }
                            }
                            if (IsSaturday)
                            {
                                if (DayOfWeek.Saturday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                {

                                }
                                else
                                {
                                    if (DayOfWeek.Saturday - todayDate.DayOfWeek < 0)
                                    {
                                        day =  (DayOfWeek.Saturday - todayDate.DayOfWeek);
                                    }
                                    else
                                    {
                                        day = DayOfWeek.Saturday - todayDate.DayOfWeek;
                                    }
                                    var timeSpan = new TimeSpan(day, 0, 0, 0);
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    var template = recommended.WorkoutTemplates[currentIndex];
                                    if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                    {
                                        eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                        i++;
                                        if (i >= remainingToLevelUp)
                                            break;
                                        currentIndex++;
                                    }
                                }
                            }
                            if (IsSunday)
                            {
                                if (6 - ExtensionMethods.DayOfWeek(todayDate, DayOfWeek.Monday) < 0 && currentDate == DateTime.Now.Date)
                                {

                                }
                                else
                                {
                                    if (DayOfWeek.Sunday - todayDate.DayOfWeek < 0)
                                    {
                                        day = (6 - ExtensionMethods.DayOfWeek(todayDate, DayOfWeek.Monday)); ;
                                    }
                                    else
                                    {
                                        day = DayOfWeek.Sunday - todayDate.DayOfWeek;
                                    }
                                    var timeSpan = new TimeSpan(day, 0, 0, 0);
                                    if (templadteCount <= currentIndex)
                                        currentIndex = 0;
                                    var template = recommended.WorkoutTemplates[currentIndex];
                                    if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                    {
                                        eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                        i++;
                                        if (i >= remainingToLevelUp)
                                            break;
                                        currentIndex++;
                                    }
                                }
                            }
                            currentDate = todayDate.AddDays(7);
                            todayDate = todayDate.AddDays(7);
                        }


                        //}
                    }
                    else
                    {
                        //User choice
                        if (LocalDBManager.Instance.GetDBSetting("ReminderDays") != null && LocalDBManager.Instance.GetDBSetting("ReminderDays").Value != null)
                        {
                            var strDays = LocalDBManager.Instance.GetDBSetting("ReminderDays").Value;
                            TimeSpan timePickerSpan;
                            try
                            {
                                timePickerSpan = TimeSpan.Parse(LocalDBManager.Instance.GetDBSetting("ReminderTime").Value);
                            }
                            catch (Exception ex)
                            {
                            }
                            int i = 0;
                           //var todayDate = DateTime.Now.Date;
                            while (i <= remainingToLevelUp)
                            {
                                if (strDays.ToCharArray().Length == 7)
                                {
                                    IsSunday = strDays[0] == '1';
                                    IsMonday = strDays[1] == '1';
                                    IsTuesday = strDays[2] == '1';
                                    IsWednesday = strDays[3] == '1';
                                    IsThursday = strDays[4] == '1';
                                    IsFriday = strDays[5] == '1';
                                    IsSaturday = strDays[6] == '1';

                                    var ReminderDays = "";
                                    ReminderDays = IsSunday ? "1" : "0";
                                    ReminderDays += IsMonday ? "1" : "0";
                                    ReminderDays += IsTuesday ? "1" : "0";
                                    ReminderDays += IsWednesday ? "1" : "0";
                                    ReminderDays += IsThursday ? "1" : "0";
                                    ReminderDays += IsFriday ? "1" : "0";
                                    ReminderDays += IsSaturday ? "1" : "0";

                                    var day = 0;

                                    if (IsMonday)
                                    {
                                        if (DayOfWeek.Monday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                        {

                                        }
                                        else
                                        {
                                            if (DayOfWeek.Monday - todayDate.DayOfWeek < 0 && currentDate < DateTime.Now.Date.AddDays(7))
                                            {
                                                day = 7 + (DayOfWeek.Monday - todayDate.DayOfWeek);
                                            }
                                            else
                                            {
                                                day = DayOfWeek.Monday - todayDate.DayOfWeek;
                                            }
                                            var timeSpan = new TimeSpan(day, 0, 0, 0);
                                            if (templadteCount <= currentIndex)
                                                currentIndex = 0;
                                            var template = recommended.WorkoutTemplates[currentIndex];
                                            if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                            {
                                                eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                                i++;
                                                if (i >= remainingToLevelUp)
                                                    break;
                                                currentIndex++;
                                            }
                                        }
                                    }
                                    if (IsTuesday )
                                    {
                                        if (DayOfWeek.Tuesday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                        {

                                        }
                                        else
                                        {
                                            if (DayOfWeek.Tuesday - todayDate.DayOfWeek < 0 && currentDate < DateTime.Now.Date.AddDays(7))
                                            {
                                                day = 7 + (DayOfWeek.Tuesday - todayDate.DayOfWeek);
                                            }
                                            else
                                            {
                                                day = DayOfWeek.Tuesday - todayDate.DayOfWeek;
                                            }
                                            var timeSpan = new TimeSpan(day, 0, 0, 0);
                                            if (templadteCount <= currentIndex)
                                                currentIndex = 0;
                                            var template = recommended.WorkoutTemplates[currentIndex];
                                            if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                            {
                                                eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                                i++;
                                                if (i >= remainingToLevelUp)
                                                    break;
                                                currentIndex++;
                                            }
                                        }
                                    }
                                    if (IsWednesday)
                                    {
                                        if (DayOfWeek.Wednesday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                        {

                                        }
                                        else
                                        {
                                            if (DayOfWeek.Wednesday - todayDate.DayOfWeek < 0 && currentDate < DateTime.Now.Date.AddDays(7))
                                            {
                                                day = 7 + (DayOfWeek.Wednesday - todayDate.DayOfWeek);
                                            }
                                            else
                                            {
                                                day = DayOfWeek.Wednesday - todayDate.DayOfWeek;
                                            }
                                            var timeSpan = new TimeSpan(day, 0, 0, 0);
                                            if (templadteCount <= currentIndex)
                                                currentIndex = 0;
                                            var template = recommended.WorkoutTemplates[currentIndex];
                                            if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                            {
                                                eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                                i++;
                                                if (i >= remainingToLevelUp)
                                                    break;
                                                currentIndex++;
                                            }
                                        }
                                    }
                                    if (IsThursday)
                                    {
                                        if (DayOfWeek.Thursday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                        {

                                        }
                                        else
                                        {
                                            if (DayOfWeek.Thursday - todayDate.DayOfWeek < 0 && currentDate < DateTime.Now.Date.AddDays(7))
                                            {
                                                day = 7 + (DayOfWeek.Thursday - todayDate.DayOfWeek);
                                            }
                                            else
                                            {
                                                day = DayOfWeek.Thursday - todayDate.DayOfWeek;
                                            }
                                            var timeSpan = new TimeSpan(day, 0, 0, 0);
                                            if (templadteCount <= currentIndex)
                                                currentIndex = 0;
                                            var template = recommended.WorkoutTemplates[currentIndex];
                                            if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                            {
                                                eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                                i++;
                                                if (i >= remainingToLevelUp)
                                                    break;
                                                currentIndex++;
                                            }
                                        }
                                    }
                                    if (IsFriday )
                                    {
                                        if (DayOfWeek.Friday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                        {

                                        }
                                        else
                                        {
                                            if (DayOfWeek.Friday - todayDate.DayOfWeek < 0 && currentDate < DateTime.Now.Date.AddDays(7))
                                            {
                                                day = 7 + (DayOfWeek.Friday - todayDate.DayOfWeek);
                                            }
                                            else
                                            {
                                                day = DayOfWeek.Friday - todayDate.DayOfWeek;
                                            }
                                            var timeSpan = new TimeSpan(day, 0, 0, 0);
                                            if (templadteCount <= currentIndex)
                                                currentIndex = 0;
                                            var template = recommended.WorkoutTemplates[currentIndex];
                                            if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                            {
                                                eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                                i++;
                                                if (i >= remainingToLevelUp)
                                                    break;
                                                currentIndex++;
                                            }
                                        }
                                    }
                                    if (IsSaturday )
                                    {
                                        if (DayOfWeek.Saturday - todayDate.DayOfWeek < 0 && currentDate == DateTime.Now.Date)
                                        {

                                        }
                                        else
                                        {
                                            if (DayOfWeek.Saturday - todayDate.DayOfWeek < 0 && currentDate < DateTime.Now.Date.AddDays(7))
                                            {
                                                day = 7 + (DayOfWeek.Saturday - todayDate.DayOfWeek);
                                            }
                                            else
                                            {
                                                day = DayOfWeek.Saturday - todayDate.DayOfWeek;
                                            }
                                            var timeSpan = new TimeSpan(day, 0, 0, 0);
                                            if (templadteCount <= currentIndex)
                                                currentIndex = 0;
                                            var template = recommended.WorkoutTemplates[currentIndex];
                                            if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                            {
                                                eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                                i++;
                                                if (i >= remainingToLevelUp)
                                                    break;
                                                currentIndex++;
                                            }
                                        }
                                    }
                                    if (IsSunday)
                                    {
                                        if (6 - ExtensionMethods.DayOfWeek(todayDate, DayOfWeek.Monday) < 0 && currentDate == DateTime.Now.Date)
                                        {

                                        }
                                        else
                                        {
                                            if (DayOfWeek.Sunday - todayDate.DayOfWeek < 0)
                                            {
                                                day = (6 - ExtensionMethods.DayOfWeek(todayDate, DayOfWeek.Monday)); ;
                                            }
                                            else
                                            {
                                                day = DayOfWeek.Sunday - todayDate.DayOfWeek;
                                            }
                                            var timeSpan = new TimeSpan(day, 0, 0, 0);
                                            if (templadteCount <= currentIndex)
                                                currentIndex = 0;
                                            var template = recommended.WorkoutTemplates[currentIndex];
                                            if (!eventCollection.ContainsKey(todayDate.Add(timeSpan)))
                                            { 
                                            eventCollection.Add(todayDate.Add(timeSpan), new DayEventCollection<EventModel>(GenerateEvents(template.Id, template.Label, template.IsSystemExercise)) { EventIndicatorColor = AppThemeConstants.BlueStartGradient, EventIndicatorSelectedColor = AppThemeConstants.BlueStartGradient });
                                            i++;
                                            if (i >= remainingToLevelUp)
                                                break;
                                            currentIndex++;
                                            }
                                        }
                                    }
                                    currentDate = todayDate.AddDays(7);
                                    todayDate = todayDate.AddDays(7);
                                }

                            }
                        }
                    }
                    //calendar.Events = eventCollection;
                    calendar.DayTappedCommand = new Command<DateTime>(DaySelected);
                }
                
            }
            catch (Exception ex)
            {

            }
        }
        private async void DaySelected(DateTime obj)
        {
            var eventModel = eventCollection.Where(x => x.Key == obj).FirstOrDefault();
            if (eventModel.Value != null)
            {
                foreach (EventModel item in eventModel.Value)
                {
                    if (item.IsPast)
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        CurrentLog.Instance.PastWorkoutDate = item.Date != null ? ((DateTime)item.Date) : obj;
                        await PagesFactory.PushAsync<HistoryPage>(false);
                        return;
                    }
                    CurrentLog.Instance.CurrentWorkoutTemplate = new WorkoutTemplateModel()
                    {
                        Id = item.Id,
                        Label = item.Name,
                        IsSystemExercise = item.IsSystemExercise
                    };
                    break;
                }
                CurrentLog.Instance.CurrentWorkoutTemplate.Exercises = new List<ExerciceModel>();
                var workoutModel = LocalDBManager.Instance.GetDBSetting($"Workout{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}")?.Value;
                if (!string.IsNullOrEmpty(workoutModel))
                {
                    var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkoutTemplateModel>(workoutModel);
                    CurrentLog.Instance.CurrentWorkoutTemplate = model;
                }
                await PagesFactory.PushAsync<KenkoChooseYourWorkoutExercisePage>();
            }
        }
        private async Task LoadSavedWeights()
        {
            try
            {
                chartViewStrength.Margin = new Thickness(Device.Android == Device.RuntimePlatform ? -90 : -83, 0);
                chartViewVolume.Margin = new Thickness(Device.Android == Device.RuntimePlatform ? -90 : -83, 0);
                var chartSerie = new ChartSerie() { Name = "Weight chart", Color = SKColor.Parse("#38418C") };
                List<ChartSerie> chartSeries = new List<ChartSerie>();
                List<ChartEntry> entries = new List<ChartEntry>();
                var weightList = ((App)Application.Current).weightContext?.Weights;

                // if (weightList.Count > 0 && (DateTime.Now - weightList.First().CreatedDate).TotalDays > 9)
                // {
                //     if (StackSteps2.Children.IndexOf(volumeBox) != 3)
                //     {
                //         StackSteps2.Children.Remove(volumeBox);
                //         StackSteps2.Children.Remove(strengthBox);
                //         StackSteps2.Children.Insert(3, volumeBox);
                //         StackSteps2.Children.Insert(4, strengthBox);
                //     }
                //         
                // }
                // else if (StackSteps2.Children.IndexOf(volumeBox) == 3)
                // {
                //     StackSteps2.Children.Remove(volumeBox);
                //     StackSteps2.Children.Remove(strengthBox);
                //     StackSteps2.Children.Insert(7, volumeBox);
                //     StackSteps2.Children.Insert(8, strengthBox);
                // }                
                if (weightList != null)
                {
                    SetupWeightTracker(weightList);
                     if (weightList.Count < 2)
                    {
                        ImgWeight.IsVisible = true;
                        LblWeightGoal2.Text = LblWeightGoal;
                        CurrentLog.Instance.WeightChangedPercentage = LblWeightGoal2.Text;
                        LblWeightGoal2.FontSize = 15;
                        LblWeightGoal2.TextColor = Color.FromHex("#AA000000");
                        LblWeightGoal2.FontAttributes = FontAttributes.None;
                        LblWeightGoal2.Margin = new Thickness(20, 11, 20, 20);
                        LblTrackin2.IsVisible = true;
                        chartViewWeight.IsVisible = false;
                        WeightArrowText.IsVisible = false;
                        //WeightBox.IsVisible = false;
                        WeightBox2.IsVisible = false;
                    }
                    else
                    {
                        WeightArrowText.IsVisible = true;
                        ImgWeight.IsVisible = false;
                        chartViewWeight.IsVisible = true;
                        LblWeightGoal2.Margin = new Thickness(20, 11, 20, 0);
                        WeightArrowText.Margin = new Thickness(20, 11, 20, 20);
                        //if (weightList.Count < 4)
                        chartViewWeight.Margin = new Thickness(Device.Android == Device.RuntimePlatform ? -90 : -83, 0);

                        if (weightList.Count < 3)
                        {
                            weightList.Add(weightList.Last());
                        }


                        LblTrackin2.IsVisible = false;
                        //StackWeightProgress.IsVisible = true;
                        //Green
                        //WeightArrowText.TextColor = Color.FromHex("#5CD196");
                        WeightArrowText.Text = "0%";
                        LblWeightGoal2.FontSize = 20;
                        LblWeightGoal2.TextColor = Color.Black;
                        LblWeightGoal2.FontAttributes = FontAttributes.Bold;
                        LblWeightGoal2.Margin = new Thickness(20, 11, 20, 0);
                        WeightArrowText.Margin = new Thickness(20, 11, 20, 20);
                        WeightArrowText.Text = "Since last entry.";
                        if (Math.Round(weightList[0].Weight, 2) == Math.Round(weightList[1].Weight, 2))
                        {
                            LblWeightGoal2.Text = "Your weight is stable";
                        }
                        else if (Math.Round(weightList[0].Weight, 2) >= Math.Round(weightList[1].Weight, 2))
                        {
                            var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;

                            // WeightArrowText.Text = "Since last entry.";
                            LblWeightGoal2.Text = String.Format("Weight up {0:0.##}%", Math.Round(progress,2)).ReplaceWithDot();


                        }
                        else
                        {
                            //Red
                            //WeightArrowText.TextColor = Color.FromHex("#BA1C31");
                            var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;
                            //WeightArrowText.Text = "Since last entry.";
                            LblWeightGoal2.Text = String.Format("Weight down {0:0.##}%", Math.Round(progress,2)).ReplaceWithDot().Replace("-", "");
                        }
                        
                        //Set Weight data
                        var days = (int)((DateTime)weightList[0].CreatedDate.Date - (DateTime)weightList[1].CreatedDate.Date).TotalDays;
                        var dayStr = days > 1 ? "days" : "day";
                        WeightArrowText.Text = $"In the last {days} {dayStr}.";
                        CurrentLog.Instance.WeightChangedPercentage = $"Body {LblWeightGoal2.Text} in {days} {dayStr}";
                        //To remove unwanted zero

                        var last3points = weightList.Take(3).Reverse();
                        foreach (var weight in last3points)
                        {
                            try
                            {

                            var isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg";

                            decimal val = 0;
                            if (isKg)
                                val = Decimal.Parse(string.Format("{0:0.##}", Math.Round(weight.Weight, 2)));
                            else
                                val = Decimal.Parse(string.Format("{0:0.##}", new MultiUnityWeight((decimal)weight.Weight, "kg").Lb));  

                            entries.Add(new ChartEntry((float)val) { Label = weight.CreatedDate.ToString("MMM dd"), ValueLabel = val.ToString() });

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        chartSerie.Entries = entries;
                        chartSeries.Add(chartSerie);

                        chartViewWeight.Chart = new LineChart
                        {
                            LabelOrientation = Orientation.Vertical,
                            ValueLabelOrientation = Orientation.Vertical,
                            LabelTextSize = 20,
                            ValueLabelTextSize = 20,
                            SerieLabelTextSize = 16,
                            LegendOption = SeriesLegendOption.None,
                            Series = chartSeries,
                        };
                    }
                }
            }
            catch { }

            LoadSavedWeightFromServer();
        }

        private async Task LoadSavedWeightFromServer()
        {
            try
            {


                var chartSerie = new ChartSerie() { Name = "Weight chart", Color = SKColor.Parse("#38418C") };
                List<ChartSerie> chartSeries = new List<ChartSerie>();
                List<ChartEntry> entries = new List<ChartEntry>();

                var weightList = await DrMuscleRestClient.Instance.GetUserWeights();

                ((App)Application.Current).weightContext.Weights = weightList;
                ((App)Application.Current).weightContext.SaveContexts();
                // if (weightList.Count > 0 && (DateTime.Now - weightList.First().CreatedDate).TotalDays > 9)
                // {
                //     if (StackSteps2.Children.IndexOf(volumeBox) != 3)
                //     {
                //         StackSteps2.Children.Remove(volumeBox);
                //         StackSteps2.Children.Remove(strengthBox);
                //         StackSteps2.Children.Insert(3, volumeBox);
                //         StackSteps2.Children.Insert(4, strengthBox);
                //     }
                //
                // }
                // else if (StackSteps2.Children.IndexOf(volumeBox) == 3)
                // {
                //     StackSteps2.Children.Remove(volumeBox);
                //     StackSteps2.Children.Remove(strengthBox);
                //     StackSteps2.Children.Insert(7, volumeBox);
                //     StackSteps2.Children.Insert(8, strengthBox);
                // }
                SetupWeightTracker(weightList);
                if (weightList.Count < 2)
                {
                    ImgWeight.IsVisible = true;
                    LblWeightGoal2.Text = LblWeightGoal;
                    CurrentLog.Instance.WeightChangedPercentage = LblWeightGoal2.Text;
                    LblWeightGoal2.FontSize = 15;
                    LblWeightGoal2.TextColor = Color.FromHex("#AA000000");
                    LblWeightGoal2.FontAttributes = FontAttributes.None;
                    LblWeightGoal2.Margin = new Thickness(20, 11, 20, 20);

                    chartViewWeight.IsVisible = false;
                    WeightArrowText.IsVisible = false;

                    //WeightBox.IsVisible = false;
                    WeightBox2.IsVisible = false;
                    return;
                }
                LblWeightGoal2.Margin = new Thickness(20, 11, 20, 0);
                WeightArrowText.IsVisible = true;
                ImgWeight.IsVisible = false;
                chartViewWeight.IsVisible = true;

                //if (weightList.Count < 4)
                chartViewWeight.Margin = new Thickness(-83, 0);

                if (weightList.Count < 3)
                {
                    weightList.Add(weightList.Last());
                }
                var days = (int)((DateTime)weightList[0].CreatedDate.Date - (DateTime)weightList[1].CreatedDate.Date).TotalDays;
                var dayStr = days > 1 ? "days" : "day";
                WeightArrowText.Text = $"In the last {days} {dayStr}.";

                LblTrackin2.IsVisible = false;
                //  StackWeightProgress.IsVisible = true;
                //Green
                //WeightArrowText.TextColor = Color.FromHex("#5CD196");
                WeightArrowText.FontSize = 15;
                WeightArrowText.TextColor = Color.FromHex("#AA000000");
                WeightArrowText.FontAttributes = FontAttributes.None;
                LblWeightGoal2.FontSize = 20;
                LblWeightGoal2.TextColor = Color.Black;
                LblWeightGoal2.FontAttributes = FontAttributes.Bold;
                if (Math.Round(weightList[0].Weight, 2) == Math.Round(weightList[1].Weight, 2))
                {
                    LblWeightGoal2.Text = "Your weight is stable";
                    //WeightArrowText.Text = "Since last entry.";
                }
                else if (Math.Round(weightList[0].Weight, 2) >= Math.Round(weightList[1].Weight, 2))
                {
                    var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;

                    //WeightArrowText.Text = "Since last entry.";
                    LblWeightGoal2.Text = String.Format("Weight up {0:0.##}%", Math.Round(progress,2)).ReplaceWithDot();

                }
                else
                {
                    //Red
                    //WeightArrowText.TextColor = Color.FromHex("#BA1C31");
                    var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;
                    // WeightArrowText.Text = "Since last entry.";
                    LblWeightGoal2.Text = String.Format("Weight down {0:0.##}%", Math.Round(progress,2)).ReplaceWithDot().Replace("-", ""); ;
                }
                //Set Weight data

                CurrentLog.Instance.WeightChangedPercentage = $"Body {LblWeightGoal2.Text} in {days} {dayStr}";
                var last3points = weightList.Take(3).Reverse();
                foreach (var weight in last3points)
                {
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg";

                    decimal val = 0;
                    if (isKg)
                        val = Decimal.Parse(string.Format("{0:0.##}", Math.Round(weight.Weight, 2)));
                    else
                        val = Decimal.Parse(string.Format("{0:0.##}", new MultiUnityWeight((decimal)weight.Weight, "kg").Lb));


                    entries.Add(new ChartEntry((float)val) { Label = weight.CreatedDate.ToString("MMM dd"), ValueLabel = val.ToString() });
                }
                chartSerie.Entries = entries;
                chartSeries.Add(chartSerie);

                chartViewWeight.Chart = new LineChart
                {
                    LabelOrientation = Orientation.Vertical,
                    ValueLabelOrientation = Orientation.Vertical,
                    LabelTextSize = 20,
                    ValueLabelTextSize = 20,
                    SerieLabelTextSize = 16,
                    LegendOption = SeriesLegendOption.None,
                    Series = chartSeries,
                };
            }
            catch { }
        }


        private async void SetupWeightTracker(List<UserWeight> userWeights)
        {
            try
            {

                if (userWeights == null)
                    return;
                if (userWeights.Count == 0)
                {
                    return;
                }
                bool isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg" ? true : false;
                decimal _userBodyWeight = 0;
                if (LocalDBManager.Instance.GetDBSetting("BodyWeight")?.Value != null)
                {
                    _userBodyWeight = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture);
                    Config.CurrentWeight = _userBodyWeight.ToString();
                }

                decimal _targetIntake = 0;
                if (LocalDBManager.Instance.GetDBSetting("TargetIntake")?.Value != null)
                {
                     _targetIntake = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("TargetIntake").Value.ReplaceWithDot(), CultureInfo.InvariantCulture);
                }

                // userWeights.Reverse();
                // userWeights = userWeights.Take(15).ToList();
                // userWeights.Reverse();
                var startWeight = Convert.ToDecimal(userWeights.Last().Weight, CultureInfo.InvariantCulture);
                var fromChangePoint = startWeight;
                var weights = userWeights.Where(x => x.CreatedDate > DateTime.Now.AddDays(-10)).ToList();
                
                var averageWeight = _userBodyWeight;
                var CurrentWeight = _userBodyWeight;
                //Adding code to get average weight 
                // var fromchangePointUserObject = UserWeight();
                if (weights.Count() > 0)
                {
                    averageWeight = weights.Average(x => x.Weight);
                    //fromChangePointWeight = weights.Last().Weight;
                    //fromchangePointUserObject = weights.Last();
                }
                // else
                // {
                //     
                //     userWeights.Reverse();
                //     userWeights = userWeights.Take(2).ToList();
                //     userWeights.Reverse();
                //     
                //     startWeight = Convert.ToDecimal(userWeights.Last().Weight, CultureInfo.InvariantCulture);
                //     fromChangePoint = startWeight;
                // }
                    

                decimal goalWeight = 0;

                if (LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value != null)
                {
                    goalWeight = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    btnUpdateGoal2.IsVisible = false;
                    btnUpdateMealPlan.IsVisible = true;
                }
                else
                {
                    btnUpdateGoal2.IsVisible = true;
                    btnUpdateMealPlan.IsVisible = false;
                    LblWeightTip2.Text = "Goal weight not set";
                    LblWeightTipText2.Text = "";// "Update your goal weight to see the weight tips here.";
                }
                if (userWeights.Count == 1)
                {
                    btnLogWeight2.IsVisible = true;
                    btnUpdateMealPlan.IsVisible = false;
                    btnUpdateGoal2.IsVisible = false;
                    btnLogWeight3.IsVisible = true;
                    BtnUpdateMealPlan3.IsVisible = false;
                } else
                {
                    btnLogWeight3.IsVisible = false;
                    BtnUpdateMealPlan3.IsVisible = true;
                }
                if (_targetIntake == 0)
                {
                    var userInfo = await DrMuscleRestClient.Instance.GetTargetIntakeWithoutLoader();
                    if (userInfo?.TargetIntake != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("TargetIntake", userInfo.TargetIntake.ToString());
                        _targetIntake = (decimal)userInfo.TargetIntake;
                    }
                }
                var togoOfGoal = "";
                if (goalWeight == 0)
                {
                    if (_targetIntake != 0)
                        LblTargetIntake2.Text = $"{Math.Round(_targetIntake)} cal/day";
                    else
                        LblTargetIntake2.Text = $"Calories not set";
                    btnUpdateGoal.IsVisible = true;
                    btnMealPlan.IsVisible = false;
                }
                else
                {
                    if (CurrentWeight < goalWeight)
                        LblTargetIntake2.Text = $"{Math.Round(_targetIntake)} cal/day to build muscle";
                    else
                        LblTargetIntake2.Text = $"{Math.Round(_targetIntake)} cal/day to lose fat";
                    btnUpdateGoal.IsVisible = false;
                    btnMealPlan.IsVisible = true;
                }
                LblTargetIntake.Text = LblTargetIntake2.Text;
               
                LblCarbText.Text = LblCarbText2.Text;
                LblProteinText.Text = LblProteinText2.Text;
                LblFatText.Text = LblFatText2.Text;
                if (isKg)
                {

                    LblCurrentText1.Text = string.Format("{0:0.##} {1}", Math.Round(CurrentWeight, 2), "kg");

                    LblGoalText1.Text = goalWeight == 0 ? "?" : string.Format("{0:0.##} {1}", Math.Round(goalWeight, 2), "kg");
                    togoOfGoal = goalWeight == 0 ? "?" : string.Format("{0:0.##}", Math.Round(goalWeight, 2));
                    LblStartText1.Text = string.Format("{0:0.##} {1}", Math.Round(startWeight, 2), "kg");


                }
                else
                {

                    var truncateWeight = TruncateDecimal(CurrentWeight, 3);
                    var lbWeight = new MultiUnityWeight(truncateWeight, "kg").Lb;

                    goalWeight = TruncateDecimal(goalWeight, 3);
                    //var lbGoalWeight = new MultiUnityWeight(truncateGoalWeight, "kg").Lb;

                    LblCurrentText1.Text = string.Format("{0:0.##} {1}", Math.Round(lbWeight, 2), "lbs");

                    LblGoalText1.Text = goalWeight == 0 ? "?" : string.Format("{0:0.##} {1}", Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb, 2), "lbs");

                    togoOfGoal = goalWeight == 0 ? "?" : string.Format("{0:0.##}", Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb, 2));
                    LblStartText1.Text = string.Format("{0:0.##} {1}", Math.Round(new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2), "lbs");
                }
                LblCurrentText2.Text = LblCurrentText1.Text;
                LblGoalText2.Text = LblGoalText1.Text;
                LblStartText2.Text = LblStartText1.Text;
                bool isGain = false;
                if (CurrentWeight < goalWeight)
                {
                    isGain = true;

                }
                if (goalWeight != 0)
                {
                    string Gender = LocalDBManager.Instance.GetDBSetting("gender").Value.Trim();
                    var creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                    decimal weeks = 0;
                    if (creationDate != null)
                    {
                        weeks = (int)(DateTime.Now - creationDate).TotalDays / 7;
                    }
                    int lowReps = 0;
                    int highreps = 0;
                    try
                    {
                        lowReps = int.Parse(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                        highreps = int.Parse(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                    }
                    catch (Exception)
                    {

                    }
                    var result = "";
                    if (lowReps >= 5 && highreps <= 12)
                        result = "This helps you build muscle and strength.";
                    else if (lowReps >= 8 && highreps <= 15)
                        result = "This helps you build muscle and burn fat.";
                    else if (lowReps >= 5 && highreps <= 15)
                        result = "This helps you build muscle.";
                    else if (lowReps >= 12 && highreps <= 20)
                        result = "This helps you burn fat.";
                    else if (highreps >= 16)
                        result = "This helps you build muscle and burn fat.";
                    else
                    {
                        if (LocalDBManager.Instance.GetDBSetting("Demoreprange") != null)
                        {
                            if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscle")
                            {
                                result = "This helps you build muscle.";
                            }
                            else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscleBurnFat")
                            {
                                result = "This helps you build muscle and burn fat.";
                            }
                            else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                            {
                                result = "This helps you burn fat.";
                            }
                        }
                    }
                    decimal rate = (decimal)2.3;
                    if (result.Contains("build muscle and burn fat"))
                    {
                        rate = (decimal)2.4;
                    }
                    else if (result.Contains("build muscle"))
                    {
                        rate = (decimal)2.3;
                    }
                    else if (result.Contains("burn fat"))
                    {
                        rate = (decimal)2.4;
                    }
                    decimal gainDouble = 0;
                    if (Gender == "Man")
                    {
                        if (weeks <= 18)
                            gainDouble = ((decimal)0.015 - (decimal)0.000096899 * weeks) * averageWeight;
                        else if (weeks > 18 && weeks <= 42)
                            gainDouble = ((decimal)0.011101 - (decimal)0.000053368 * weeks) * averageWeight;
                        else if (weeks > 42)
                            gainDouble = (decimal)0.00188 * averageWeight;
                    }
                    else
                    {
                        if (weeks <= 18)
                            gainDouble = (((decimal)0.015 - (decimal)0.000096899 * weeks) * averageWeight) / 2;
                        else if (weeks > 18 && weeks <= 42)
                            gainDouble = (((decimal)0.011101 - (decimal)0.000053368 * weeks) * averageWeight) / 2;
                        else if (weeks > 42)
                            gainDouble = ((decimal)0.00188 * averageWeight) / 2;
                    }
                    //Convert to day
                    gainDouble = gainDouble / 30;


                    decimal loseDouble = ((decimal)0.01429 * averageWeight) / 30;


                    string gain = string.Format("{0:0.##}", isKg ? Math.Round(gainDouble, 2) : Math.Round(new MultiUnityWeight(gainDouble, WeightUnities.kg).Lb, 2));

                    string lose = string.Format("{0:0.##}", isKg ? Math.Round(loseDouble, 2) : Math.Round(new MultiUnityWeight(loseDouble, WeightUnities.kg).Lb, 2));
                    var weekText = weeks <= 1 ? "week" : "weeks";
                    int days = 0;

                    if (userWeights.Count > 1)
                    {
                        days = Math.Abs((int)(userWeights.Last().CreatedDate.Date - userWeights.First().CreatedDate.Date).TotalDays);
                      //  startWeight = Convert.ToDecimal(userWeights[1].Weight, CultureInfo.InvariantCulture);
                    }

                    double totalChanged = 0;
                    if (userWeights.Count > 1)
                        totalChanged = (double)(((userWeights.First().Weight - userWeights[1].Weight) * 100) / userWeights[1].Weight);
                    double dailyChanged = (double)totalChanged;

                    if (days != 0)
                        dailyChanged = totalChanged / days;
                    bool isLess = false;
                    if (days == 0)
                        days = 1;
                    if (CurrentWeight > goalWeight)
                    {
                        //Lose weight
                        if (Math.Round(CurrentWeight, 1) >= Math.Round(startWeight, 1))
                        {
                            isLess = true;
                        }
                        else
                        {
                            if (loseDouble > (Math.Abs((startWeight - CurrentWeight) / days)))
                                isLess = true;
                            else
                                isLess = false;
                        }
                    }
                    else
                    {
                        //Gain
                        if (Math.Round(CurrentWeight, 1) <= Math.Round(startWeight, 1))
                        {
                            isLess = false;
                        }
                        else
                        {
                            if (gainDouble < (Math.Abs((startWeight - CurrentWeight) / days)))
                                isLess = true;
                            else
                                isLess = false;
                        }

                    }

                    var lessMoreText = "";

                    if (CurrentWeight <= goalWeight)
                    {
                        //Gain weight
                        if (isLess)
                        {
                            lessMoreText = "so you're probably gaining fat.";//$"You're probably gaining fat. Eat less (but aim for {Math.Round(CurrentWeight * rate)} g protein / day).";
                        }
                        else
                        {

                            lessMoreText = $"so you could speed that up by eating more."; //$"You're probably leaving muscle on the table. Eat more (and aim for {Math.Round(CurrentWeight * rate)} g protein / day).";
                        }
                    }
                    else
                    {
                        //lose weight
                        if (isLess)
                        {
                            //lessMoreText = $"you could speed that up by eating less. And aim to eat {Math.Round(CurrentWeight * rate)} g of protein a day.";
                            lessMoreText = "so you could speed that up by eating less.";//$"To speed that up, eat less (but aim for {Math.Round(CurrentWeight * rate)} g protein / day).";

                        }
                        else
                        {
                            //lessMoreText = $"you're probably losing muscle mass too. Eat more to prevent that. And aim to eat {Math.Round(CurrentWeight * rate)} g of protein a day.";
                            lessMoreText = "so you're probably losing muscle mass.";//$"You're probably losing muscle mass. Eat more (and aim for {Math.Round(CurrentWeight * rate)} g protein / day).";
                        }
                    }

                    var goalGainWeight = string.Format("{0:0.##}", Math.Round(CurrentWeight * rate) / 1000, 2);


                    var gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(CurrentWeight - startWeight, 2)));
                    var gainInaMonth = Math.Round(CurrentWeight - startWeight, 2) / days;
                    var gainInaMonthText = string.Format("{0:0.##}", Math.Round(Math.Abs((CurrentWeight - startWeight)) / days, 2));
                    var gainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(goalWeight - startWeight, 2)));
                    var remainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(goalWeight - CurrentWeight, 2)));
                    var massunit = "kg";
                    if (!isKg)
                    {
                        massunit = "lbs";
                        gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2)));

                        gainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2)));

                        remainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb - new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2)));
                        goalGainWeight = string.Format("{0:0.##}", Math.Round(new MultiUnityWeight(CurrentWeight * rate, "kg").Lb / (decimal)453.59237, 2));

                        gainInaMonthText = string.Format("{0:0.##}", Math.Abs(Math.Round((new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb) / days, 2)));
                    }

                    //loseDouble and gainInaMonth // Check if https://app.startinfinity.com/b/7N8FXx54wWE/sDxhY6Rxbjh/974d2c5c-d3d3-4f73-910e-b10fb10f092c?t=comments&open=true&view=c956a7e0-0553-42e6-af24-7b6c915d3c44
                    // +-10 should lose or gain about then
                    decimal percentageDifference = 0;//
                    bool isOnTrack = false;
                    try
                    {
                        if (CurrentWeight > goalWeight)
                        { 
                            percentageDifference = ((Math.Round(Math.Abs(loseDouble),2) - Math.Round(Math.Abs(gainInaMonth),2)) / Math.Round(Math.Abs(gainInaMonth),2)) * 100;
                            if (Math.Round(Math.Abs(percentageDifference)) <= 10)
                                isOnTrack = true;
                        } else
                        {
                            
                            percentageDifference = ((Math.Round(Math.Abs(gainDouble), 2) - Math.Round(Math.Abs(gainInaMonth), 2)) / Math.Round(Math.Abs(gainInaMonth), 2)) * 100;
                            if (Math.Round(Math.Abs(percentageDifference)) <= 10)
                               isOnTrack = true;
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                    var proteinNo = "";
                    if (LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg")
                    {
                        proteinNo = Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Kg * (decimal)1.6) + "-" + Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Kg * (decimal)2.2);
                    }
                    else
                    {
                        proteinNo = Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb * (decimal)0.7) + "-" + Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb * (decimal)1.0);
                    }
                    //AI CARD
                        btnUpdateGoal3.IsVisible = false;
                        BtnHelpWithGoal.IsVisible = true;
                    
                        double userWithdays = 0;
                        try
                        {
                            userWithdays= Math.Abs((int)(userWeights.Last().CreatedDate.Date - userWeights.First().CreatedDate.Date).TotalDays);
                        }
                        catch (Exception e)
                        {
                        }
                        
                        //  startWeight = Convert.ToDecimal(userWeights[1].Weight, CultureInfo.InvariantCulture);

                        var usercreationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                        if (usercreationDate != null)
                        {
                            userWithdays = (int)(DateTime.Now - usercreationDate).TotalDays;
                        }

                        //TODO: Remove below comments when enable AI chart card
                        //try
                        //{
                        //    GetUserWorkoutLogAverageResponse res = workoutLogAverage;
                        //    var workoutCount = 0;
                        //    if (res == null)
                        //    {
                        //        res = ((App)Application.Current).UserWorkoutContexts.workouts;
                        //    }
                        //    var exerciseModel = res?.HistoryExerciseModel;
                        //    LblXXWorkout.Text = "-";
                        //    if (exerciseModel != null)
                        //    {
                        //        var d = userWithdays > 1 ? "days" : "day";
                        //        LblXXWorkout.Text = $"{exerciseModel.TotalWorkoutCompleted} {(exerciseModel.TotalWorkoutCompleted > 1 ? "workouts" : "workout")} in {userWithdays} {d}";
                        //    }
                        //}
                        //catch (Exception e)
                        //{
                            
                        //}
                        
                     
                        
                    if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1) && Math.Round(CurrentWeight, 1) == Math.Round(goalWeight, 1))
                    {
                        LblWeightTip2.Text = "Your weight is stable";
                        LblWeightTipText2.Text = $"At {LblCurrentText1.Text}, your weight is stable. Aim for {proteinNo} g protein/day.";
                        LblWeightToGo2.Text = string.Format("Success! {0} 💪", LblGoalText1.Text);
                        //TODO: Remove below comments when enable AI chart card
                        //LblXXWeightLeft.Text = "Congratulations on reaching your goal!";
                        btnUpdateGoal3.IsVisible = true;
                        BtnHelpWithGoal.IsVisible = false;
                    }
                    else if (Math.Round(CurrentWeight, 1) == Math.Round(goalWeight, 1))
                    {

                        LblWeightTip2.Text = string.Format("{0} {1} {2} in {3} {4}", gainWeight, massunit, CurrentWeight > startWeight ? "gained" : "lost", days, days > 1 ? "days" : "day");
                        LblWeightToGo2.Text = string.Format("Success! {0} 💪", LblGoalText1.Text);

                        LblWeightTipText2.Text = $"At {LblCurrentText1.Text}, you're at your goal weight. Aim for {proteinNo} g protein/day.";
                        //TODO: Remove below comments when enable AI chart card
                        //LblXXWeightLeft.Text = "Congratulations on reaching your goal!";
                        btnUpdateGoal3.IsVisible = true;
                        BtnHelpWithGoal.IsVisible = false;
                    }
                    else if (CurrentWeight < goalWeight)
                    {
                        //TODO: Remove below comments when enable AI chart card
                        //Gain weight
                        //LblXXWeightLeft.Text = $"Your weight is {LblCurrentText1.Text}, with {remainDiffernece} {massunit} left to goal. How can I help? Ask me anything.";
                       if (userWeights.Count == 1)
                        {
                            LblWeightTip2.Text = "Log weight to start";
                            LblWeightTipText2.Text = $"New users like you usually gain {gain} {massunit} a day. Log your weight to start getting custom tips to gain weight faster.";
                        }
                        else if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                        {
                            LblWeightTip2.Text = "Your weight is stable";
                            // LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a month. Currently, you're at your starting weight. So, {lessMoreText}";
                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";
                            LblWeightTipText2.Text = $"Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. your weight is stable. {lessMoreText}";
                        }
                        else if (CurrentWeight > startWeight)
                        {

                            LblWeightTip2.Text = string.Format("{0} {1} gained in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");
                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            LblWeightTipText2.Text = $"You're gaining {gainInaMonthText} {massunit} a day. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. {lessMoreText}";
                            if (isOnTrack)
                                LblWeightTipText2.Text = $"You're gaining {gainInaMonthText} {massunit} a day. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. So, you're right on track.";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //   LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a month. Currently, you're gaining {gainInaMonthText} {massunit} a month. So, {lessMoreText}";
                        }
                        else if (CurrentWeight < startWeight)
                        {
                            LblWeightTip2.Text = string.Format("{0} {1} lost in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");

                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            // LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a month. Currently, you're losing {gainInaMonthText} {massunit} a month. So, {lessMoreText}";
                            LblWeightTipText2.Text = $"You're losing {gainInaMonthText} {massunit} a day. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. {lessMoreText}";
                            //if (isOnTrack)
                            //    LblWeightTipText2.Text = $"You're losing {gainInaMonthText} {massunit} a day. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. So, you're right on track.";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                        }
                    }
                    else
                    {
                        //Loose weight
                        //TODO: Remove below comments when enable AI chart card
                        //LblXXWeightLeft.Text = $"Your weight is {LblCurrentText1.Text}, with {remainDiffernece} {massunit} left to goal. How can I help? Ask me anything.";

                        if (userWeights.Count == 1)
                        {
                            LblWeightTip2.Text = "Log weight to start";
                            LblWeightTipText2.Text = $"New users like you usually lose {lose} {massunit} a day. Log your weight to start getting custom tips to lose weight faster.";
                        }
                        else if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                        {
                            LblWeightTip2.Text = "Your weight is stable";
                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            //    LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should gain about {lose} {massunit} a month. Currently, you're at your starting weight. So, {lessMoreText}";

                            //  LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should gain about {lose} {massunit} a month. Currently, you're at your starting weight. So, {lessMoreText}";
                            LblWeightTipText2.Text = $"At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a day. your weight is stable. {lessMoreText}";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //   LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. To know if you're eating enough calories, track your waist circumference every week. If it's going up, {lessMoreText}";

                        }
                        else if (CurrentWeight > startWeight)
                        {

                            LblWeightTip2.Text = string.Format("{0} {1} gained in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");

                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a month. Currently, you're gaining {gainInaMonthText} {massunit} a month. So, {lessMoreText}";

                            LblWeightTipText2.Text = $"You're gaining {gainInaMonthText} {massunit} a day. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a day. {lessMoreText}";
                            //if (isOnTrack)
                            //    LblWeightTipText2.Text = $"You're gaining {gainInaMonthText} {massunit} a day. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a day. So, you're right on track.";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. To know if you're eating enough calories, track your waist circumference every week. If it's going up, {lessMoreText}";
                        }
                        else if (CurrentWeight < startWeight)
                        {
                            LblWeightTip2.Text = string.Format("{0} {1} lost in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");

                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            LblWeightTipText2.Text = $"You're losing {gainInaMonthText} {massunit} a day. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a day. {lessMoreText}";
                            if (isOnTrack)
                                LblWeightTipText2.Text = $"You're losing {gainInaMonthText} {massunit} a day. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a day. So, you're right on track.";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a month. Currently, you're losing {gainInaMonthText} {massunit} a month. So, {lessMoreText}";
                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. To know if you're eating enough calories, track your waist circumference every week. If it's going up, {lessMoreText}";
                        }

                    }
                }
                else
                {
                    int days = 0;
                    if (userWeights.Count > 1)
                    {
                        days = Math.Abs((int)(userWeights[1].CreatedDate.Date - userWeights.First().CreatedDate.Date).TotalDays);
                        startWeight = Convert.ToDecimal(userWeights[1].Weight, CultureInfo.InvariantCulture);
                    }


                    if (days == 0)
                        days = 1;
                    var gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(CurrentWeight - startWeight, 2)));
                    var gainInaMonth = Math.Round(CurrentWeight - startWeight, 2) / days * 30;

                    var massunit = "kg";
                    if (!isKg)
                    {
                        massunit = "lbs";
                        gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2)));

                    }
                    if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                    {
                        LblWeightToGo2.Text = "Your are at your starting weight";

                        LblWeightTipText2.Text = $"";
                    }
                    else if (CurrentWeight > startWeight)
                    {
                        LblWeightToGo2.Text = string.Format("{0} {1} gained in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");
                    }
                    else if (CurrentWeight < startWeight)
                    {
                        LblWeightToGo2.Text = string.Format("{0} {1} lost in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");
                    }
                }
                LblWeightToGo1.Text = LblWeightToGo2.Text;
                LblWeightTip1.Text = LblWeightTip2.Text;
                LblWeightTipText1.Text = LblWeightTipText2.Text;
                CurrentLog.Instance.CoachTipsText = LblWeightTipText2.Text;
                if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                {
                    LbltrackerText1.Text = $"Your are at your starting weight";
                    LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    LbltrackerText2.Text = LbltrackerText1.Text;
                }

                else if (Math.Round(CurrentWeight, 1) == Math.Round(goalWeight, 1))
                {
                    if (isKg)
                        LbltrackerText1.Text =
                            string.Format("Success! {0:0.##} kg 💪", Math.Round(CurrentWeight, 2));
                    else
                        LbltrackerText1.Text = string.Format("Success! {0:0.##} lbs 💪", Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2));
                    LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    LbltrackerText2.Text = LbltrackerText1.Text;
                }
                else if (CurrentWeight > goalWeight && goalWeight > startWeight)
                {
                    //Progress smoothly
                    LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;

                    if (isKg)
                    {
                        LbltrackerText1.Text = string.Format("Success! {0:0.##} kg above goal", Math.Round(CurrentWeight - goalWeight, 2));

                        LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                    else
                    {
                        LbltrackerText1.Text = string.Format("Success! {0:0.##} lbs above goal", Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)goalWeight, "kg").Lb, 2));
                        LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                }
                else if (CurrentWeight < goalWeight && goalWeight < startWeight)
                {
                    //Progress smoothly
                    LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;

                    if (isKg)
                    {
                        LbltrackerText1.Text = string.Format("Success! {0:0.##} kg under goal", Math.Round(goalWeight - CurrentWeight, 2));
                        LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                    else
                    {
                        LbltrackerText1.Text = string.Format("Success! {0:0.##} lbs under goal", Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb - new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2));
                        LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                }
                else if (CurrentWeight > startWeight)
                {
                    //Overweight
                    if (goalWeight < CurrentWeight)
                    {
                        LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Red;
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Red;
                    }
                    else
                    {
                        LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    }
                    if (isKg)
                    {
                        LbltrackerText1.Text = string.Format("You have gained {0:0.##} kg", Math.Round(CurrentWeight - startWeight, 2));
                    }
                    else
                    {
                        LbltrackerText1.Text = string.Format("You have gained {0:0.##} lbs", Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2));
                    }
                    LbltrackerText2.Text = LbltrackerText1.Text;
                }

                else if (CurrentWeight < startWeight)
                {
                    //Low weight
                    if (goalWeight < CurrentWeight)
                    {
                        LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    }
                    else
                    {
                        LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Red;
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Red;
                    }
                    if (isKg)
                    {
                        LbltrackerText1.Text = string.Format("You have lost {0:0.##} kg", Math.Round(startWeight - CurrentWeight, 2));
                        LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                    else
                    {
                        LbltrackerText1.Text =
                 string.Format("You have lost {0:0.##} lbs", Math.Round(new MultiUnityWeight((decimal)startWeight, "kg").Lb - new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2));

                        LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                    //if (!isGain)
                    //{
                    //    LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                    //    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;

                    //}
                }


                else if (CurrentWeight == startWeight)
                {
                    LbltrackerText1.Text = $"Your weight is stable";
                    LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    LbltrackerText2.Text = LbltrackerText1.Text;
                }

            }
            catch (Exception ex)
            {

            }
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

        public decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal tmp = Math.Truncate(step * value);
            return tmp / step;
        }

        async void BtnChangeMyMinde_Clicked(object sender, EventArgs e)
        {
            try
            {
                stackOptions.Children.Clear();
                await AddAnswer
                    (((DrMuscleButton)sender).Text);
                await AddQuestion("OK, what do you wanna do?");

                if (upi != null && upi.RecommendedProgram != null && upi.NextWorkoutTemplate != null)
                    await AddOptions($"Start workout: {upi.NextWorkoutTemplate.Label}", BtnStartTodayWorkout_Clicked);
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

            //var dt = DateTime.Now.AddMinutes(1);

            //var timeSpan = new TimeSpan(0, dt.Hour, dt.Minute, 0);            DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Dr Muscle", "You forgot to save your workout!", timeSpan, 1352, NotificationInterval.Week, Convert.ToString(104));
            //return;
            //#if DEBUG

            //            PopupNavigation.Instance.PushAsync(new Views.GeneralPopup());
            //            return;
            //#endif
            askforEquipment();
            ShouldAnimate = false;
            try
            {
                if (((Xamarin.Forms.Grid)btnStartWorkout.Parent).Effects.Count > 0)
                {
                    ((Xamarin.Forms.Grid)btnStartWorkout.Parent).Effects.Remove(((Xamarin.Forms.Grid)btnStartWorkout.Parent).Effects.Last());
                }
            }
            catch (Exception)
            {

            }


        }
        async void ChooseLevel()
        {
            ActionSheetConfig config = new ActionSheetConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            };

            bool isProduction = LocalDBManager.Instance.GetDBSetting("Environment") == null || LocalDBManager.Instance.GetDBSetting("Environment").Value == "Production";

            config.Add("Beginner", () =>
            {
                MoveToWorkout();
                SetUserMobilityLevel("Beginner");
            });
            config.Add("Intermediate", () =>
            {
                MoveToWorkout();
                SetUserMobilityLevel("Intermediate");
            });
            config.Add("Advanced", () =>
            {
                MoveToWorkout();
                SetUserMobilityLevel("Advanced");
            });
            config.SetTitle("Choose level");
            UserDialogs.Instance.ActionSheet(config);
        }

         long GetMobilityWorkoutId(string type)
        {
            var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
            var label = workouts?.GetUserProgramInfoResponseModel?.RecommendedProgram.Label;
            var workoutName = workouts?.GetUserProgramInfoResponseModel?.NextWorkoutTemplate.Label;
            if (string.IsNullOrEmpty(label))
            {
                switch (type)
                {
                    case "Beginner" :
                        return 16906;
                        break;
                    case "Intermediate" :
                        return 16905;
                        break;
                    case "Advanced" :
                        return 16904;
                        break;
                }
            }

            switch (type)
            {
                case "Beginner":
                    if (label.Contains("Full-Body") || label.Contains("Powerlifting"))
                        return 16906;
                    if (label.Contains("Push/Pull/Legs"))
                {
                    if (workoutName.Contains("Push"))
                        return 21092;
                    if (workoutName.Contains("Pull"))
                        return 21093;
                    if (workoutName.Contains("Legs"))
                        return 21094;
                }
                    if (label.Contains("Split"))
                    {
                        if (workoutName.Contains("Lower"))
                            return 21086;
                        if (workoutName.Contains("Upper"))
                            return 21087;
                    }
                    return 16906;
                    break;
                case "Intermediate":
                    if (label.Contains("Full-Body") || label.Contains("Powerlifting"))
                        return 16905;
                    if (label.Contains("Push/Pull/Legs"))
                {
                    if (workoutName.Contains("Push"))
                        return 21095;
                    if (workoutName.Contains("Pull"))
                        return 21096;
                    if (workoutName.Contains("Legs"))
                        return 21097;
                }
                    if (label.Contains("Split"))
                    {
                        if (workoutName.Contains("Lower"))
                            return 21088;
                        if (workoutName.Contains("Upper"))
                            return 21089;
                    }
                    return 16905;
                    break;
                case "Advanced":
                    if (label.Contains("Full-Body") || label.Contains("Powerlifting"))
                        return 16904;
                    if (label.Contains("Push/Pull/Legs"))
                {
                    if (workoutName.Contains("Push"))
                        return 21098;
                    if (workoutName.Contains("Pull"))
                        return 21099;
                    if (workoutName.Contains("Legs"))
                        return 21100;
                }
                    if (label.Contains("Split"))
                    {
                        if (workoutName.Contains("Lower"))
                            return 21090;
                        if (workoutName.Contains("Upper"))
                            return 21091;
                    }
                    return 16904;
                    break;
            }

            return 0;
        }
        async void MoveToWorkout()
        {
            if (App.IsV1UserTrial || App.IsFreePlan || await CanGoFurther())
            {
                CurrentLog.Instance.IsMobilityStarted = true;

                if (LocalDBManager.Instance.GetDBSetting("MobilityLevel")?.Value == "Beginner")
                    CurrentLog.Instance.CurrentWorkoutTemplate = new WorkoutTemplateModel()
                    {
                        Id = GetMobilityWorkoutId("Beginner"),
                        Label = "Flexibility & Mobility 1",
                        IsSystemExercise = true,
                        Exercises = new List<ExerciceModel>()
                    };
                else if (LocalDBManager.Instance.GetDBSetting("MobilityLevel")?.Value == "Intermediate")
                    CurrentLog.Instance.CurrentWorkoutTemplate = new WorkoutTemplateModel()
                    {
                        Id = GetMobilityWorkoutId("Intermediate"),
                        Label = "Flexibility & Mobility 2",
                        IsSystemExercise = true,
                        Exercises = new List<ExerciceModel>()
                    };
                else if (LocalDBManager.Instance.GetDBSetting("MobilityLevel")?.Value == "Advanced")
                    CurrentLog.Instance.CurrentWorkoutTemplate = new WorkoutTemplateModel()
                    {
                        Id = GetMobilityWorkoutId("Advanced"),
                        Label = "Flexibility & Mobility 3",
                        IsSystemExercise = true,
                        Exercises = new List<ExerciceModel>()
                    };
                else
                {
                    ChooseLevel();
                    return;
                }

                CurrentLog.Instance.WorkoutStarted = true;
                var workoutModel = LocalDBManager.Instance.GetDBSetting($"Workout{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}")?.Value;
                if (!string.IsNullOrEmpty(workoutModel))
                {
                    var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkoutTemplateModel>(workoutModel);
                    CurrentLog.Instance.CurrentWorkoutTemplate = model;
                }
                await PagesFactory.PushAsync<KenkoChooseYourWorkoutExercisePage>();
                SetupNextWorkout();
            }
        }

        private async void SetupNextWorkout()
        {
            //setupNextWorkout

        }
        private async void SetUserMobility(bool result)
        {
            LocalDBManager.Instance.SetDBSetting("IsMobility", result ? "true" : "false");
            
        }

        private async void SetUserMobilityLevel(string level)
        {
            LocalDBManager.Instance.SetDBSetting("MobilityLevel", level);
           
        }

        async void DemoStartTodaysWorkout()
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = AppResources.PleaseCheckInternetConnection,
                        Title = AppResources.ConnectionError,
                        OkText = "Try again"
                    });

                    return;
                }
                if (App.IsV1UserTrial || App.IsFreePlan || await CanGoFurther())
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
                                    
                                    return;
                                }

                            }
                            if (nextWorkout != null)
                            {

                                CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
                                CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
                                CurrentLog.Instance.WorkoutStarted = true;
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
                    //await PagesFactory.PushAsync<SubscriptionPage>();
                }

            }
            catch (Exception ex)
            {

            }
        }
        async void StartTodaysWorkout()
        {
#if DEBUG

            //try
            //{
            //    var waitHandle1 = new EventWaitHandle(false, EventResetMode.AutoReset);
            //    var modalPage1 = new Views.PreviewOverlay();
            //    modalPage1.Disappearing += (sender2, e2) =>
            //    {
            //        waitHandle1.Set();
            //    };
            //    await PopupNavigation.Instance.PushAsync(modalPage1);

            //    await Task.Run(() => waitHandle1.WaitOne());
            //    return;
            //}
            //catch (Exception ex)
            //{

            //}
#endif
            _tipsArray = GetTipsArray();

            //Config.ShowTipsNumber
            if (Config.ShowTipsNumber >= _tipsArray.Count)
                Config.ShowTipsNumber = 0;

            var startworkoutText = "Start workout";
            var previewworkout = "Preview workout";
            if (LocalDBManager.Instance.GetDBSetting($"AnySets{DateTime.Now.Date}")?.Value
                         == "1")
            {
                previewworkout = "Resume workout";
                startworkoutText = "Resume workout";
            }
            if (BtnCardStartWorkout.Text.ToUpper().Contains("PREVIEW NEXT WORKOUT") || LocalDBManager.Instance.GetDBSetting($"IsTodayWarmupCompleted{DateTime.Now.Date}")?.Value == "true")
            {
                CurrentLog.Instance.IsMobilityFinished = true;
                CurrentLog.Instance.IsMobilityStarted = false;
            }
            
            if ((!CurrentLog.Instance.IsFreePlanPopup && App.IsFreePlan)
                 || Config.ShowWelcomePopUp2 == false && !CurrentLog.Instance.IsMobilityStarted || (LocalDBManager.Instance.GetDBSetting("IsMobility")?.Value == "true" && !CurrentLog.Instance.IsMobilityFinished && !Config.MobilityWelcomePopup))
            { }
            else
            {
                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                System.Diagnostics.Debug.WriteLine($"Tips: {_tipsArray[Config.ShowTipsNumber]}");
                var modalPage = new Views.GeneralPopup("Lamp.png", $"{_tipsArray[Config.ShowTipsNumber][0]}", $"{_tipsArray[Config.ShowTipsNumber][1]}", _isAnyWorkoutFinished ? $"{previewworkout}" : $"{startworkoutText}", null, true, false, _tipsArray[Config.ShowTipsNumber][2], _tipsArray[Config.ShowTipsNumber][3], "false","false");
                Config.ShowTipsNumber += 1;
                if (upi != null && upi?.NextWorkoutTemplate != null)
                {
                    if ((upi?.NextWorkoutTemplate.Id >= 16336 && upi?.NextWorkoutTemplate.Id <= 16338) || (upi?.NextWorkoutTemplate.Id >= 16343 && upi?.NextWorkoutTemplate.Id <= 16351))
                    { }
                    else
                    {

                        await PopupNavigation.Instance.PushAsync(modalPage);
                        //Task.Run(async () =>
                        //{
                        //    await Task.Delay(6000);
                        //    if (PopupNavigation.Instance.PopupStack.Count > 0 && !modalPage._isHide)
                        //        PopupNavigation.Instance.PopAsync();
                        //});
                    }
                }
            }

#if DEBUG
            //try
            //{
            //    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            //    var modalPage = new Views.GeneralPopup("TrueState.png", "Boom!", "Creating your program...", "Try sample workout first");
            //    modalPage.Disappearing += (sender2, e2) =>
            //    {
            //        waitHandle.Set();
            //    };
            //    await PopupNavigation.Instance.PushAsync(modalPage);
            //    await Task.Run(() => waitHandle.WaitOne());
            //}
            //catch (Exception ex)
            //{
            //}
#endif
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {

                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = AppResources.PleaseCheckInternetConnection,
                        Title = AppResources.ConnectionError,
                        OkText = "Try again"
                    });
                    return;
                }
                if (LocalDBManager.Instance.GetDBSetting("IsMobility") == null || LocalDBManager.Instance.GetDBSetting("IsMobility")?.Value == null)
                {
                    //Consider existing user
                    ConfirmConfig exitPopUp = new ConfirmConfig()
                    {
                        Title = "Add flexibility & mobility warm-ups?",
                        Message = "Change later in Settings.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Yes, add warm-ups",
                        CancelText = AppResources.No,
                    };

                    var result = await UserDialogs.Instance.ConfirmAsync(exitPopUp);
                    SetUserMobility(result);

                    if (result)
                    {
                        ChooseLevel();
                        return;
                    }
                }
                else
                {
                    if (App.IsV1UserTrial || await CanGoFurther() || App.IsFreePlan)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("IsMobility")?.Value == "true" && !CurrentLog.Instance.IsMobilityFinished)
                        {
                            MoveToWorkout();
                            return;
                        }
                    }
                }

                if (App.IsV1UserTrial || App.IsFreePlan || await CanGoFurtherWithoughtLoader())
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
                        WorkoutTemplateGroupModel groupModel = upi.RecommendedProgram;
                        if (upi == null || upi.NextWorkoutTemplate == null)
                        {
                            await PagesFactory.PushAsync<ChooseDrMuscleOrCustomPage>();
                        }
                        else
                        {
                            if (upi.NextWorkoutTemplate.Exercises == null)
                            {
                                try
                                {
                                    nextWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkoutWithoutLoader(upi.NextWorkoutTemplate.Id);
                                }
                                catch (Exception ex)
                                {
                                    return;
                                }

                            }
                            if (nextWorkout != null)
                            {
                                CurrentLog.Instance.CurrentWorkoutTemplateGroup = groupModel;
                                CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
                                CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
                                CurrentLog.Instance.WorkoutStarted = true;
                                var workoutModel = LocalDBManager.Instance.GetDBSetting($"Workout{DateTime.Now.Date}{CurrentLog.Instance.CurrentWorkoutTemplate.Id}")?.Value;
                                if (!string.IsNullOrEmpty(workoutModel))
                                {
                                    var model = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkoutTemplateModel>(workoutModel);
                                    CurrentLog.Instance.CurrentWorkoutTemplate = model;
                                }
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
                    // await PagesFactory.PushAsync<SubscriptionPage>();
                    StartTodaysWorkout();
                }

            }
            catch (Exception ex)
            {

            }
        }

        async void RefreshCurrentWorkout()
        {
            var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
            var isBackedUser = false;
            try
            {
                if (upi != null && upi.NextWorkoutTemplate != null)
                {
                    upi.NextWorkoutTemplate = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkoutWithoutLoader(upi.NextWorkoutTemplate.Id);
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

            try
            {
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;

                if (workouts != null && workouts.LastWorkoutDate != null)
                {
                    if (LocalDBManager.Instance.GetDBSetting("Reminder5th") == null)
                        LocalDBManager.Instance.SetDBSetting("Reminder5th", "true");
                    var xDays = (DateTime.Now - (DateTime)workouts.LastWorkoutDate).TotalDays;
                    if (xDays > 4 && xDays < 6 && LocalDBManager.Instance.GetDBSetting("Reminder5th").Value == "true")
                    {
                        CurrentLog.Instance.IsRecoveredWorkout = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }

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

        async void animate(Grid grid)
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
                    if (ShouldAnimate)
                        animate(grid);
                });

            }
            catch (Exception ex)
            {
                ShouldAnimate = false;
            }
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
            if (BotList.Count > 0)
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            }
            await Task.Delay(300);


        }

        async Task<DrMuscleButton> AddOptions(string title, EventHandler handler)
        {
            var grid = new Grid() { HorizontalOptions = LayoutOptions.FillAndExpand, Margin = new Thickness(21, 0, 21, 8) };
            var btn = new DrMuscleButton()
            {
                Text = title.ToUpperInvariant(),
                TextColor = Color.Black,
                Padding = new Thickness(5, 5),
                FontSize = 13,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent
            };
            var pancakeView = new PancakeView() { HorizontalOptions = LayoutOptions.FillAndExpand, HeightRequest = title.Contains("\n") ? 59 : 45, Margin = new Thickness(0, 5), CornerRadius = 6, Content = btn, Shadow = new DropShadow() { Color = Color.FromHex("#55000000"), Offset = new Point(10, 10) } };

            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 0 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 1 });

            //pancakeView.OffsetAngle = 45;
            //pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 0 });
            //pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 1 });
            CustomFrame frame = new CustomFrame() { HorizontalOptions = LayoutOptions.FillAndExpand, HeightRequest = title.Contains("\n") ? App.ScreenHeight > 668 ? 70: 60 : App.ScreenHeight > 668 ? 66 : 45, Margin = new Thickness(0, 5), Padding = 0, CornerRadius = 6 };
            //CustomFrame frame = new CustomFrame() { HorizontalOptions = LayoutOptions.FillAndExpand, HeightRequest = title.Contains("\n") ? 59 : 45, Margin = new Thickness(0, 5), Padding = 0, CornerRadius = 6 };

            grid.Children.Add(frame);
            grid.Children.Add(pancakeView);


            btn.Clicked += handler;
            SetDefaultButtonStyle(btn);
            btn.Margin = new Thickness(0);
            btn.Padding = new Thickness(25, 10, 25, 10);
            btn.CornerRadius = 6;
            //grid.Children.Add(btn);

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    var vsg = new VisualStateGroup() { Name = "vsg" };
            //    var vs = new VisualState { Name = "Normal" };
            //    var vs2 = new VisualState { Name = "Pressed" };
            //    var vs3 = new VisualState { Name = "Focused" };
            //    vs.Setters.Add(new Setter
            //    {
            //        Property = Entry.TextColorProperty,
            //        Value = Color.White
            //    });

            //    vs.Setters.Add(new Setter
            //    {
            //        Property = Entry.BackgroundColorProperty,
            //        Value = AppThemeConstants.BlueColor
            //    });

            //    vs2.Setters.Add(new Setter
            //    {
            //        Property = Entry.TextColorProperty,
            //        Value = Color.White
            //    });
            //    vs2.Setters.Add(new Setter
            //    {
            //        Property = Entry.BackgroundColorProperty,
            //        Value = AppThemeConstants.DimBlueColor
            //    });

            //    // btn.Visual = VisualMarker.Material;

            //    vsg.States.Add(vs);
            //    vsg.States.Add(vs2);

            //    VisualStateManager.GetVisualStateGroups(btn).Add(vsg);
            //}
            stackOptions.Children.Add(grid);



            //await Task.Delay(300);
            if (BotList.Count > 0)
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            }
            return btn;
        }

        async Task<ExtendedButton> AddOptionsWithoutScroll(string title, EventHandler handler)
        {
            var grid = new Grid();
            var pancakeView = new PancakeView() { HeightRequest = 66, Margin = new Thickness(25, 5) };

            //pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            //pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 1 });
            //pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 0 });
            grid.Children.Add(pancakeView);

            var btn = new ExtendedButton()
            {
                Text = title,
                TextColor = Color.Black,
                BackgroundColor = Color.Blue,
                Padding = new Thickness(8, 0),
                FontSize = 17
            };
            btn.Clicked += handler;
            SetDefaultButtonStyle(btn);
            btn.BackgroundColor = AppThemeConstants.BlueColor;
            if (Device.RuntimePlatform == Device.iOS)
            {
                var vsg = new VisualStateGroup() { Name = "vsg" };
                var vs = new VisualState { Name = "Normal" };
                var vs2 = new VisualState { Name = "Pressed" };
                var vs3 = new VisualState { Name = "Focused" };
                vs.Setters.Add(new Setter
                {
                    Property = Entry.TextColorProperty,
                    Value = Color.White
                });

                vs.Setters.Add(new Setter
                {
                    Property = Entry.BackgroundColorProperty,
                    Value = AppThemeConstants.BlueColor
                });

                vs2.Setters.Add(new Setter
                {
                    Property = Entry.TextColorProperty,
                    Value = Color.White
                });
                vs2.Setters.Add(new Setter
                {
                    Property = Entry.BackgroundColorProperty,
                    Value = AppThemeConstants.DimBlueColor
                });

                // btn.Visual = VisualMarker.Material;

                vsg.States.Add(vs);
                vsg.States.Add(vs2);

                VisualStateManager.GetVisualStateGroups(btn).Add(vsg);
            }

            grid.Children.Add(btn);
            stackOptions.Children.Add(grid);
            await Task.Delay(300);

            //if (BotList.Count > 0)
            //{
            //    lstChats.ScrollTo(BotList.First(), ScrollToPosition.MakeVisible, false);
            //    lstChats.ScrollTo(BotList.First(), ScrollToPosition.Start, false);
            //}

            return btn;
        }

        void SetDefaultButtonStyle(Button btn)
        {
            btn.BackgroundColor = Color.Transparent;
            btn.BorderWidth = 2;
            btn.CornerRadius = 0;
            btn.Margin = new Thickness(25, 5, 25, 5);
            btn.FontAttributes = FontAttributes.Bold;
            btn.BorderColor = Color.Transparent;
            btn.TextColor = Color.White;
            btn.HeightRequest = 66;

        }

        void SetEmphasisButtonStyle(Button btn)
        {
            btn.TextColor = Color.Black;
            btn.BackgroundColor = Color.White;
            btn.Margin = new Thickness(25, 5, 25, 5);
            btn.HeightRequest = 60;
            btn.BorderWidth = 2;
            btn.CornerRadius = 0;
            btn.FontAttributes = FontAttributes.Bold;
        }
        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                PopupNavigation.Instance.PopAllAsync();
                return true;
            }
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

        async void Check10Days()
        {
            try
            {

                if (LocalDBManager.Instance.GetDBSetting("creation_date") == null)
                    return;

                DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                if (LocalDBManager.Instance.GetDBSetting("CongratulationsPopup") != null)
                {
                    var dateTime = Convert.ToDateTime(LocalDBManager.Instance.GetDBSetting("CongratulationsPopup").Value);
                    if (dateTime.Date == DateTime.Now.Date)
                        return;
                }
                var totaldays = (DateTime.Now.ToUniversalTime() - creationDate).TotalDays;
                if ((int)totaldays >= 10 && (int)totaldays <= 14)
                {
                    LocalDBManager.Instance.SetDBSetting("CongratulationsPopup", DateTime.Now.Date.ToString());
                    if (await CanGoFurtherWithoughtLoader())
                    {
                        //User is isV1 or with subscription
                        LblCongTitle.Text = $"{AppResources.GreatYouHaveBeenWorkingOutFor} {(int)totaldays} {AppResources.Days}. {AppResources.HowsYourExperienceWithDrMuscle}";
                        CongPopupStack.IsVisible = true;
                    }
                    else
                    {
                        var day = 14 - (int)totaldays > 1 ? AppResources.Days : "Day";
                        var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                        var mainTitle = AppResources.GotItExclamation;
                        var msg = $"Trial ends in {14 - (int)totaldays} {day}";
                        var okTitle = AppResources.LearnMore;
                        try
                        {
                            if (workouts.Sets != null)
                            {
                                workoutLogAverage = workouts;
                                if (workouts.Averages.Count > 1)
                                {
                                    OneRMAverage last = workouts.Averages.ToList()[workouts.Averages.Count - 1];
                                    OneRMAverage before = workouts.Averages.ToList()[workouts.Averages.Count - 2];
                                    decimal progresskg = (last.Average.Kg - before.Average.Kg) * 100 / (last.Average.Kg < 1 ? 1 : last.Average.Kg);
                                    bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                                    // var strProgress = String.Format("- {0}: {1}{2} ({3}%)\n", AppResources.MaxStrength, (last.Average.Kg - before.Average.Kg) > 0 ? "+" : "", inKg ? Math.Round(last.Average.Kg - before.Average.Kg) + " kg" : Math.Round(last.Average.Lb - before.Average.Lb) + " lbs", Math.Round(progresskg)).ReplaceWithDot();
                                    //if (last.Average.Kg - before.Average.Kg > 0)
                                    //{
                                    //    mainTitle = $"You have improved {Math.Round(progresskg)}%";
                                    //    msg = $"Most users like you improve 34% in 30 days. Your trial ends in {14 - (int)totaldays} days. Sign up to continue improving fast?";
                                    //    okTitle = "Sign up";
                                    //}
                                    //else if (last.Average.Kg - before.Average.Kg < 0)
                                    //{
                                    //    mainTitle = $"Your change is {Math.Round(progresskg)}%";
                                    //    msg = $"Most users like you improve 34% in 30 days. Your trial ends in {14 - (int)totaldays} days. Sign up to continue improving fast?";
                                    //    okTitle = "Sign up";
                                    //}
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                        ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                        {
                            Title = msg,
                            Message = "Learn more?",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = okTitle,
                            CancelText = AppResources.Cancel,
                            OnAction = async (bool ok) =>
                            {
                                if (ok)
                                {
                                    await PagesFactory.PushAsync<SubscriptionPage>();
                                }
                            }
                        };
                        if (PopupNavigation.Instance.PopupStack.Count == 0)
                            UserDialogs.Instance.Confirm(ShowRIRPopUp);
                    }

                }
                else
                {
                    if ((int)totaldays < 10)
                        return;
                    //if (!await CanGoFurtherWithoughtLoader() && !App.IsTrialExpiredPopup)
                    //{
                    //    App.IsTrialExpiredPopup = true;
                    //    ConfirmConfig ShowRIRPopUp = new ConfirmConfig()
                    //    {
                    //        Title = "Your free trial has expired",
                    //        Message = $"Sign up to continue getting in shape faster",
                    //        //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    //        OkText = AppResources.LearnMore,
                    //        CancelText = AppResources.Cancel,
                    //        OnAction = async (bool ok) =>
                    //        {
                    //            if (ok)
                    //            {
                    //                await PagesFactory.PushAsync<SubscriptionPage>();
                    //            }
                    //        }
                    //    };
                    //    UserDialogs.Instance.Confirm(ShowRIRPopUp);
                    //    return;
                    //}
                    if (DateTime.Now.ToUniversalTime().Date == creationDate.Date)
                        return;
                    // Remove comment when we move in production

                    if (Config.LastOpenCongratsPopupDate.AddMonths(1) < DateTime.Now && workoutLogAverage.LastMonthWorkoutCount >= 2 || (Config.LastOpenCongratsPopupDate == DateTime.Now.Date && GetMonthDifference(creationDate.Date, DateTime.Now.Date) > 1 && workoutLogAverage.LastMonthWorkoutCount >= 2))
                    {
                        Config.LastOpenCongratsPopupDate = DateTime.Now.Date;
                        var month = GetMonthDifference(creationDate.Date, DateTime.Now.Date);
                        CongPopupStack.IsVisible = true;
                        var monthText = month == 1 ? $"1 {AppResources.Month}" : $"{month} {AppResources.months}";
                        LblCongTitle.Text = $"{AppResources.CongYouHaveBeenWorkingOutFor} {monthText}. {AppResources.HowsYourExperienceWithDrMuscle}";
                        LocalDBManager.Instance.SetDBSetting("CongratulationsPopup", DateTime.Now.Date.ToString());
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        int GetMonthDifference(DateTime startDate, DateTime endDate)
        {
            int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }

        void GreatAction_Tapped(object sender, System.EventArgs e)
        {
            CongPopupStack.IsVisible = false;
            if (!Config.RateAlreadyGiven)
            {
                //RatePopupStack.IsVisible = true;
                ConfirmConfig ShowRateusPopUp = new ConfirmConfig()
                {
                    Title = AppResources.GreatExclamation,
                    Message = AppResources.RateUsOnStore,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.Yes,
                    CancelText = AppResources.MaybeLater,
                    OnAction = async (bool ok) =>
                    {
                        if (ok)
                        {
                            YesAction_Tapped(sender, e);
                        }
                    }
                };
                UserDialogs.Instance.Confirm(ShowRateusPopUp);

            }
            else
            {
                //Invite friend
                //
                ConfirmConfig ShowInvitePopUp = new ConfirmConfig()
                {
                    Title = AppResources.GreatExclamation,
                    Message = AppResources.InviteAFriendToTryDrMuscleForFree,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.Yes,
                    CancelText = AppResources.MaybeLater,
                    OnAction = async (bool ok) =>
                    {
                        if (ok)
                        {
                            openInviteMail(AppResources.GreatNewWorkoutApp, "Check out this new workout app...\n\niPhone download: https://itunes.apple.com/app/dr-muscle/id1073943857?mt=8\n\nAndroid: https://play.google.com/store/apps/details?id=com.drmaxmuscle.dr_max_muscle&hl=en");
                        }

                    }
                };
                UserDialogs.Instance.Confirm(ShowInvitePopUp);
            }
        }

        void GoodAction_Tapped(object sender, System.EventArgs e)
        {
            CongPopupStack.IsVisible = false;
            _firebase.LogEvent("Good, but could be improved", "rating_good");
            ConfirmConfig ShowGoodPopUp = new ConfirmConfig()
            {
                Title = AppResources.SendUsAQuickEmail,
                Message = AppResources.WeBelieveYourExperienceShouldBeSolidHowCanWeImprove,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = AppResources.SendEmail,
                CancelText = AppResources.Cancel,
                OnAction = async (bool ok) =>
                {
                    if (ok)
                    {
                        openMail("Suggestions to improve for Dr. Muscle");
                    }
                }
            };
            UserDialogs.Instance.Confirm(ShowGoodPopUp);


        }

        void BadAction_Tapped(object sender, System.EventArgs e)
        {
            CongPopupStack.IsVisible = false;
            //
            _firebase.LogEvent("rating_bad", "Bad");
            ConfirmConfig ShowSorryPopUp = new ConfirmConfig()
            {
                Title = AppResources.BadSorryToHearThat,
                Message = AppResources.WeBelieveYourExperienceShouldBeSolidSendQuickEmailHowCanWeImprove,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = AppResources.SendEmail,
                CancelText = AppResources.Cancel,
                OnAction = async (bool ok) =>
                {
                    if (ok)
                    {
                        openMail("Bad experience with Dr. Muscle");
                    }
                }
            };
            UserDialogs.Instance.Confirm(ShowSorryPopUp);
        }


        //Welcome back popup action
        void LaterAction_Tapped(object sender, System.EventArgs e)
        {
            welcomePopupStack.IsVisible = false;
            welcomeRestPopupStack.IsVisible = false;
        }
        void TiredTodayAction_Tapped(object sender, System.EventArgs e)
        {
            welcomePopupStack.IsVisible = false;
            welcomeRestPopupStack.IsVisible = false;
            BtnFeelingWeekShortOnTime_Clicked(new DrMuscleButton() { Text = "I'm tired or short on time" }, EventArgs.Empty);

        }

        void StartAction_Tapped(object sender, System.EventArgs e)
        {
            welcomeRestPopupStack.IsVisible = false;
            welcomePopupStack.IsVisible = false;
            StartTodaysWorkout();
        }

        void openMail(string subject)
        {
            Device.OpenUri(new Uri($"mailto:support@drmuscleapp.com?subject={subject}"));
        }

        void openInviteMail(string subject, string body = "")
        {
            try
            {
                Device.OpenUri(new Uri($"mailto:?subject={subject}&body={body}"));
            }
            catch (System.Exception ex)
            {

            }

        }

        void YesAction_Tapped(object sender, System.EventArgs e)
        {
            _firebase.LogEvent("rating_great", "Great!");
            //RatePopupStack.IsVisible = false;
            Config.RateAlreadyGiven = true;
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                try
                {
                    Device.OpenUri(new Uri("market://details?id=com.drmaxmuscle.dr_max_muscle"));
                }
                catch (Exception)
                {
                    Device.OpenUri(new Uri("https://play.google.com/store/apps/details?id=com.drmaxmuscle.dr_max_muscle&hl=en_IN"));
                }
            }
            else
            {
                try
                {
                    Device.OpenUri(new Uri("itms-apps://itunes.apple.com/app/id1073943857"));
                }
                catch (Exception)
                {

                    Device.OpenUri(new Uri("https://itunes.apple.com/us/app/dr-muscle/id1073943857?mt=8"));
                }
            }
        }

        void MayBeLater_Tapped(object sender, System.EventArgs e)
        {
            _firebase.LogEvent("rating_maybe_later", "Great!");
            //RatePopupStack.IsVisible = false;
        }

        void AlreadyGiven_Tapped(object sender, System.EventArgs e)
        {
            _firebase.LogEvent("rating_already_given", "Great!");
            //RatePopupStack.IsVisible = false;
            Config.RateAlreadyGiven = true;
        }

        async void DrMuscleButton_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WelcomeBox.HeightRequest;

            //await WelcomeBox.LayoutTo(new Rectangle(WelcomeBox.Bounds.X, WelcomeBox.Bounds.Y, WelcomeBox.Bounds.Width, 0), 750, Easing.Linear);
            //await Task.Delay(500);
            WelcomeBox.IsVisible = false;
            WelcomeBox.HeightRequest = height;
        }

        async void WeightProgressDismiss1_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WeightProgress1.HeightRequest;
            WeightProgress1.IsVisible = false;
            WeightProgress1.HeightRequest = height;
        }

        async void WeightProgressDismiss2_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WeightProgress2.HeightRequest;

            //await WelcomeBox.LayoutTo(new Rectangle(WelcomeBox.Bounds.X, WelcomeBox.Bounds.Y, WelcomeBox.Bounds.Width, 0), 750, Easing.Linear);
            //await Task.Delay(500);
            WeightProgress2.IsVisible = false;
            WeightProgress2.HeightRequest = height;
        }

        async void WeightCoachingDismiss2_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WeightCoachingCard2.HeightRequest;

            WeightCoachingCard2.IsVisible = false;
            WeightCoachingCard2.HeightRequest = height;
        }

        async void Later_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = GoalBox.HeightRequest;
            //await GoalBox.LayoutTo(new Rectangle(GoalBox.Bounds.X, GoalBox.Bounds.Y, GoalBox.Bounds.Width, 0), 750, Easing.Linear);
            GoalBox.IsVisible = false;
            GoalBox.HeightRequest = height;
            LocalDBManager.Instance.SetDBSetting("GoalBox2", "true");
        }

        async void Learn_Clicked(System.Object sender, System.EventArgs e)
        {
            if (CheckTrialUser())
                return;
            //GoalBox.IsVisible = false;
            await PagesFactory.PushAsync<LearnPage>();
            var height = GoalBox.HeightRequest;
            //await GoalBox.LayoutTo(new Rectangle(GoalBox.Bounds.X, GoalBox.Bounds.Y, GoalBox.Bounds.Width, 0), 750, Easing.Linear);
            GoalBox.IsVisible = false;
            GoalBox.HeightRequest = height;
            LocalDBManager.Instance.SetDBSetting("GoalBox2", "false");
        }

        async void Later_Clicked2(System.Object sender, System.EventArgs e)
        {
            var height = GoalBox2.HeightRequest;
            //await GoalBox2.LayoutTo(new Rectangle(GoalBox2.Bounds.X, GoalBox2.Bounds.Y, GoalBox2.Bounds.Width, 0), 750, Easing.Linear);
            GoalBox2.IsVisible = false;
            GoalBox2.HeightRequest = height;
            LocalDBManager.Instance.SetDBSetting("GoalBox2", "true");
        }

        async void Later_dismiss_Clicked2(System.Object sender, System.EventArgs e)
        {
            var height = GoalBox2.HeightRequest;
            //await GoalBox2.LayoutTo(new Rectangle(GoalBox2.Bounds.X, GoalBox2.Bounds.Y, GoalBox2.Bounds.Width, 0), 750, Easing.Linear);
            GoalBox2.IsVisible = false;
            GoalBox2.HeightRequest = height;
            LocalDBManager.Instance.SetDBSetting("GoalBox2", "false");
        }

        async void Learn_Clicked2(System.Object sender, System.EventArgs e)
        {
            if (CheckTrialUser())
                return;
            //GoalBox.IsVisible = false;
            await PagesFactory.PushAsync<LearnPage>();
            var height = GoalBox2.HeightRequest;
            //await GoalBox2.LayoutTo(new Rectangle(GoalBox2.Bounds.X, GoalBox2.Bounds.Y, GoalBox2.Bounds.Width, 0), 750, Easing.Linear);
            GoalBox2.IsVisible = false;
            GoalBox2.HeightRequest = height;
            LocalDBManager.Instance.SetDBSetting("GoalBox2", "false");
        }
        async void LaterWeight_Clicked(System.Object sender, System.EventArgs e)
        {
            //var height = WeightBox.HeightRequest;

            //WeightBox.IsVisible = false;
            //WeightBox.HeightRequest = height;
        }
        //
        bool isMealPlan = false;
        async void GetMealPlan_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Try again"
                });
                return;
            }
            if (isMealPlan)
                return;
            isMealPlan = true;
            CheckReachability();


            //string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
            //if (!string.IsNullOrEmpty(val))
            //{
            //    if (val == "Beta")
            //    {
            //        App.IsMealPlan = false;
            //        await PagesFactory.PushAsync<MealInfoPage>();
            //        isMealPlan = false;
            //        return;
            //    }
            //    else
            //    {
            //        _isBetaExperience = null;
            //    }
            //}
            if (!App.IsMealPlan)
                await CanGoFurtherForMealPlan();
            if (App.IsMealPlan )
            {
                await PagesFactory.PushAsync<MealInfoPage>();
            }
            else
            {
                
                    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var modalPage = new Views.GeneralPopup("Lists.png", "New! Meal plans—limited-time discount", "Get in shape faster with meal plans that update on autopilot. Discounted for a limited time to celebrate.", "Get meal plan add-on", null, false, false, "false", "false", "true");
                    modalPage.Disappearing += (sender2, e2) =>
                    {
                        waitHandle.Set();
                    };
                    modalPage.OkButtonPress += ModalPage_OkButtonPress;
                    await PopupNavigation.Instance.PushAsync(modalPage);
                    await Task.Run(() => waitHandle.WaitOne());
            }
                isMealPlan = false;

        }

        private async void ModalPage_OkButtonPress(object sender, EventArgs e)
        {
            await Task.Delay(300);
            
            await DependencyService.Get<IDrMuscleSubcription>().BuyMealPlanAccess();
            
        }
        async void EnterWeight_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Try again"
                });
                return;
            }

            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = "Update body weight",
                MaxLength = 7,
                Placeholder = "Tap to enter your weight",
                OkText = AppResources.Ok,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (weightResponse.Ok)
                    {
                        

                       
                        return;
                    }
                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);

        }


        private bool CheckTrialUser()
        {
            if (App.IsFreePlan)
            {
                ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
                {
                    Message = "Upgrading will unlock custom coaching tips based on your goals and progression.",
                    Title = "You discovered a premium feature!",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Upgrade",
                    CancelText = "Maybe later",
                    OnAction = async (bool ok) =>
                    {
                        if (ok)
                        {
                            PagesFactory.PushAsync<SubscriptionPage>();
                        }
                        else
                        {

                        }
                    }
                };
                UserDialogs.Instance.Confirm(ShowWelcomePopUp2);
            }
            return App.IsFreePlan;
        }
        //TODO: Chart
        private async void SetChartData()
        {
            //Settings value

            if (LocalDBManager.Instance.GetDBSetting("email") == null)
                return;
            if (LocalDBManager.Instance.GetDBSetting("massunit") == null)
                return;
            //Setting Stats of workout done and total weight lifted
            try
            {
                var exerciseModel = workoutLogAverage.HistoryExerciseModel;
                if (exerciseModel != null)
                {
                    bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                    var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();
                    //lblWorkoutsDone.IsVisible = true;
                    //lblLiftedCount.IsVisible = true;
                    var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                    //lblWorkoutsDone.Text = exerciseModel.TotalWorkoutCompleted <= 1 ? $"{exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutDone}" : $"{exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutsDone}";
                    //lblLiftedCount.Text = $"{weightLifted.ToString("N0")} {unit} {AppResources.Lifted}";
                }
            }
            catch (Exception ex)
            {

            }

            //Drawing Chart
            try
            {

                //LblProgress.Text = "";
                //if (workoutLogAverage == null)
                //return;
                if (workoutLogAverage == null || !workoutLogAverage.Averages.Any())
                {
                    //NoDataLabel.IsVisible = true;







                    //LblSetsProgress.Text = "";
                    //LblProgress.Text = "";
                    //lblLastWorkout.Text = "";
                }
                else
                {
                    // NoDataLabel.IsVisible = false;



                    bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";


                    var chartSerie = new ChartSerie() { Name = "Strength chart", Color = SKColor.Parse("#38418C") };
                    List<ChartSerie> chartSeries = new List<ChartSerie>();

                    List<ChartEntry> entries = new List<ChartEntry>();

                    int index = 1;

                    DateTime? creationDate = null;
                    bool isestimated = true;
                    try
                    {
                        creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                    }
                    catch (Exception)
                    {

                    }
                    foreach (var sets in workoutLogAverage.Sets)
                    {
                        if (sets != 0)
                            isestimated = false;
                    }
                    if (workoutLogAverage.Averages.Count > 2)
                    {
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {

                            var val = (float)Math.Round(inKg ? data.Average.Kg : data.Average.Lb);
                            entries.Add(new ChartEntry(val) { Label = data.Date.ToLocalTime().ToString("MMM dd"), ValueLabel = val.ToString() });
                            index++;
                        }
                    }
                    else if (workoutLogAverage.Averages.Count == 2)
                    {
                        index = 2;
                        foreach (var data in workoutLogAverage.Averages.Take(2))
                        {

                            var val = (float)Math.Round(inKg ? data.Average.Kg : data.Average.Lb);
                            entries.Add(new ChartEntry(val) { Label = data.Date.ToLocalTime().ToString("MMM dd"), ValueLabel = val.ToString() });
                            index++;
                        }

                    }
                    else
                    {
                        index = 3;
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {

                            var val = (float)Math.Round(inKg ? data.Average.Kg : data.Average.Lb);
                            entries.Add(new ChartEntry(val) { Label = data.Date.ToLocalTime().ToString("MMM dd"), ValueLabel = val.ToString() });
                            index--;
                        }
                        if (index > 0)
                        {
                            for (int i = index; i > 0; i--)
                            {
                                entries.Add(new ChartEntry(0) { Label = workoutLogAverage.SetsDate[index].ToLocalTime().ToString("MMM dd"), ValueLabel = "0" });
                                //IndexToDateLabel.Add(i, "");
                            }
                        }
                    }



                    index = 1;
                    var s2 = new LineSeries()
                    {
                        Color = OxyColor.Parse("#5DD397"),//Green
                        LabelFormatString = "{1:0}",
                        FontSize = 15,
                        TextColor = OxyColor.Parse("#5DD397"),
                        LineStyle = LineStyle.Dash,
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 6,
                        MarkerStroke = OxyColor.Parse("#5DD397"),
                        MarkerFill = OxyColor.Parse("#5DD397"),
                        MarkerStrokeThickness = 1,

                    };

                    if (workoutLogAverage.Sets != null)
                    {
                        workoutLogAverage.Sets.Reverse();
                        workoutLogAverage.SetsDate.Reverse();
                        IndexToDateLabel.Clear();

                        foreach (var sets in workoutLogAverage.Sets)
                        {
                            s2.Points.Add(new DataPoint(index, Convert.ToDouble(sets)));
                            IndexToDateLabel.Add(index, workoutLogAverage.SetsDate[index - 1].ToLocalTime().ToString("MMM dd"));
                            entries[index - 1].Label = workoutLogAverage.SetsDate[index - 1].ToLocalTime().ToString("MMM dd");
                            index++;
                        }

                        chartSerie.Entries = entries;
                        chartSeries.Add(chartSerie);

                    }

                    chartViewStrength.Chart = new LineChart
                    {
                        LabelOrientation = Orientation.Vertical,
                        ValueLabelOrientation = Orientation.Vertical,
                        LabelTextSize = 20,
                        ValueLabelTextSize = 20,
                        SerieLabelTextSize = 16,
                        LegendOption = SeriesLegendOption.None,
                        Series = chartSeries,

                    };

                    //Second Chart
                    var plotModel2 = new PlotModel
                    {
                        Title = AppResources.WorkSetsCapital.ToLower().FirstCharToUpper(),
                        TitleFontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 16,
                        TitleFontWeight = FontWeights.Normal,
                        TitleColor = OxyColor.Parse("#23253A"),
                        Background = OxyColors.Transparent,
                        PlotAreaBackground = OxyColors.Transparent,
                        PlotAreaBorderColor = OxyColor.Parse("#23253A"),
                        IsLegendVisible = true
                    };


                    try
                    {

                        var minVal = (double)workoutLogAverage.Sets.Min();
                        var maxVal = (double)workoutLogAverage.Sets.Max();


                        var min = minVal - (maxVal - minVal) * 0.20;
                        var max = maxVal + (maxVal - minVal) * 0.5;
                        if (min == 0 && max == 0)
                        {
                            min = -30;
                            max = 50;
                        }


                        LinearAxis yAxis = new LinearAxis { Position = AxisPosition.Left, Minimum = min, Maximum = max, MinimumPadding = 50, AxislineColor = OxyColors.Blue, ExtraGridlineColor = OxyColors.Blue, MajorGridlineColor = OxyColors.Blue, MinorGridlineColor = OxyColors.Blue, TextColor = OxyColors.Blue, TicklineColor = OxyColors.Blue, TitleColor = OxyColors.Blue, TickStyle = TickStyle.None };
                        yAxis.IsAxisVisible = false;
                        LinearAxis xAxis = new LinearAxis { Position = AxisPosition.Bottom, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColor.Parse("#23253A"), TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColors.Blue, MinimumMajorStep = 0.3, MinorStep = 0.5, MajorStep = 0.5 };

                        xAxis.LabelFormatter = _formatter;
                        xAxis.MinimumPadding = 1;
                        xAxis.IsPanEnabled = false;
                        xAxis.IsZoomEnabled = false;
                        xAxis.Minimum = 0.5;
                        xAxis.Maximum = 3.5;

                        IndexToDateLabel2.Clear();
                        IndexToDateLabel2.Add(xAxis.Minimum, "");
                        IndexToDateLabel2.Add(xAxis.Maximum, "");

                        yAxis.IsPanEnabled = false;
                        yAxis.IsZoomEnabled = false;
                        plotModel2.Axes.Add(yAxis);
                        plotModel2.Axes.Add(xAxis);
                    }
                    catch (Exception)
                    {

                    }
                    var s12 = new LineSeries()
                    {
                        Color = OxyColor.Parse("#38418C"),
                        TextColor = OxyColor.Parse("#38418C"),
                        LabelFormatString = "{1:0}",
                        FontSize = 15,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 6,
                        MarkerStroke = OxyColor.Parse("#38418C"),
                        MarkerFill = OxyColor.Parse("#38418C"),
                        MarkerStrokeThickness = 1,
                    };
                    index = 1;
                    if (workoutLogAverage.Averages.Count > 2)
                    {
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {
                            s12.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));
                            index++;
                        }
                    }
                    else if (workoutLogAverage.Averages.Count == 2)
                    {
                        index = 2;
                        s12.Points.Add(new DataPoint(1, 0));
                        foreach (var data in workoutLogAverage.Averages.Take(2))
                        {
                            s12.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));
                            //IndexToDateLabel.Add(index, data.Date.ToLocalTime().ToString("MM/dd", CultureInfo.InvariantCulture));
                            index++;
                        }

                    }
                    else
                    {
                        index = 3;
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {
                            s12.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));

                            index--;
                        }
                        if (index > 0)
                        {
                            for (int i = index; i > 0; i--)
                            {
                                s12.Points.Add(new DataPoint(i, 0));
                            }
                        }
                    }


                    index = 1;


                    var chartSerie2 = new ChartSerie() { Name = "Workset", Color = SKColor.Parse("#5DD397") };
                    List<ChartSerie> chartSeries2 = new List<ChartSerie>();
                    List<ChartEntry> entries2 = new List<ChartEntry>();


                    if (workoutLogAverage.Sets != null)
                    {
                        IndexToDateLabel2.Clear();
                        foreach (var sets in workoutLogAverage.Sets)
                        {
                            IndexToDateLabel2.Add(index, workoutLogAverage.SetsDate[index - 1].ToLocalTime().ToString("MM/dd", CultureInfo.InvariantCulture));
                            //entries2[0].Label = workoutLogAverage.SetsDate[index - 1].ToLocalTime().ToString("MMM dd");

                            entries2.Add(new ChartEntry(sets) { Label = workoutLogAverage.SetsDate[index - 1].ToLocalTime().ToString("MMM dd", CultureInfo.InvariantCulture), ValueLabel = sets.ToString() });
                            index++;
                        }


                    }

                    chartSerie2.Entries = entries2;
                    chartSeries2.Add(chartSerie2);

                    chartViewVolume.Chart = new LineChart
                    {
                        LabelOrientation = Orientation.Vertical,
                        ValueLabelOrientation = Orientation.Vertical,
                        LabelTextSize = 20,
                        ValueLabelTextSize = 20,
                        SerieLabelTextSize = 16,
                        LegendOption = SeriesLegendOption.None,
                        Series = chartSeries2,
                    };
                    //plotModel2.Series.Add(s12);

                    if (workoutLogAverage.Sets != null)
                    {
                        workoutLogAverage.Sets.Reverse();
                        workoutLogAverage.SetsDate.Reverse();
                    }


                }
            }
            catch (Exception e)
            {
                var properties = new Dictionary<string, string>
                    {
                        { "AIPage_ChartCell", $"{e.StackTrace}" }
                    };
                Crashes.TrackError(e, properties);
            }
        }
        private string _formatter(double d)
        {
            return IndexToDateLabel.ContainsKey(d) ? IndexToDateLabel[d] : "";
        }

        void StrenthChart_Tapped(System.Object sender, System.EventArgs e)
        {
            UserDialogs.Instance.Alert(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = "Your max strength for all exercises done recently.",
                Title = "Total strength"
            });
        }

        void SetsChart_Tapped(System.Object sender, System.EventArgs e)
        {
            UserDialogs.Instance.Alert(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = "Your volume in the last 3 weeks.",
                Title = "Work sets"
            });
        }

        async void BtnStrengthGotit_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = strengthBox.HeightRequest;

            //await strengthBox.LayoutTo(new Rectangle(strengthBox.Bounds.X, strengthBox.Bounds.Y, strengthBox.Bounds.Width, 0), 750, Easing.Linear);

            strengthBox.IsVisible = false;
            strengthBox.HeightRequest = height;
        }
        async void BtnVolumeGotit_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = volumeBox.HeightRequest;
            //await volumeBox.LayoutTo(new Rectangle(volumeBox.Bounds.X, volumeBox.Bounds.Y, volumeBox.Bounds.Width, 0), 750, Easing.Linear);
            volumeBox.IsVisible = false;
            volumeBox.HeightRequest = height;
        }

        async void BtnWeightGotit_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WeightBox2.HeightRequest;
            //await WeightBox2.LayoutTo(new Rectangle(WeightBox2.Bounds.X, WeightBox2.Bounds.Y, WeightBox2.Bounds.Width, 0), 750, Easing.Linear);
            WeightBox2.IsVisible = false;
            WeightBox2.HeightRequest = height;
        }

        async void BtnStartWorkoutNow_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WeightBox2.HeightRequest;
            WeightBox2.IsVisible = false;
            WeightBox2.HeightRequest = height;
        }

        async void BtnRestNow_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WeightBox2.HeightRequest;
            WeightBox2.IsVisible = false;
            WeightBox2.HeightRequest = height;
        }

        async void BtnStatsGotit_Clicked(object sender, System.EventArgs e)
        {
            var height = StateBox.HeightRequest;
            //await StateBox.LayoutTo(new Rectangle(StateBox.Bounds.X, StateBox.Bounds.Y, StateBox.Bounds.Width, 0), 750, Easing.Linear);
            StateBox.IsVisible = false;
            StateBox.HeightRequest = height;
        }

        async void btnWelcomeStartWorkout_Clicked(object sender, System.EventArgs e)
        {
            BtnStartTodayWorkout_Clicked(sender, e);

            btnWelcomeGotit_Clicked(sender, e);
        }

        async void btnWelcomeGotit_Clicked(object sender, System.EventArgs e)
        {
            var height = SecondWelcomeBox.HeightRequest;
            //await SecondWelcomeBox.LayoutTo(new Rectangle(SecondWelcomeBox.Bounds.X, SecondWelcomeBox.Bounds.Y, SecondWelcomeBox.Bounds.Width, 0), 750, Easing.Linear);
            SecondWelcomeBox.IsVisible = false;
            SecondWelcomeBox.HeightRequest = height;


        }

        async void btnWelcomeProgress_Clicked(object sender, System.EventArgs e)
        {
            var height = SecondProgressBox.HeightRequest;

            SecondProgressBox.IsVisible = false;
            SecondProgressBox.HeightRequest = height;


        }

        async void BtnWeightHistory_Clicked(System.Object sender, System.EventArgs e)
        {
            await PagesFactory.PushAsync<HistortWeightPage>();
        }
        async void BtnLearnMore_Clicked(System.Object sender, System.EventArgs e)
        {
            if (CheckTrialUser())
                return;
            await PagesFactory.PushAsync<LearnPage>();
        }

        async void btnRecoveryWorkout_Clicked(object sender, System.EventArgs e)
        {

            ConfirmConfig ShowsharePopUp = new ConfirmConfig()
            {
                Message = $"All exercises will have only 2 work sets today.",
                Title = "Are you sure?",
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OkText = "2 work sets",
                CancelText = AppResources.Cancel,
            };
            var isConfirm = await UserDialogs.Instance.ConfirmAsync(ShowsharePopUp);
            if (isConfirm)
            {

                LocalDBManager.Instance.SetDBSetting("OlderQuickMode", LocalDBManager.Instance.GetDBSetting("QuickMode").Value);
                LocalDBManager.Instance.SetDBSetting("QuickMode", "true");
                try
                {
                    LocalDBManager.Instance.ResetReco();

                }
                catch (Exception ex)
                {

                }
                App.IsHowHardAsked = true;
                var startworkoutText = "Start workout";
                if (LocalDBManager.Instance.GetDBSetting($"AnySets{DateTime.Now.Date}")?.Value
                         == "1")
                    startworkoutText = "Resume workout";
                var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                var modalPage = new Views.GeneralPopup("TrueState.png", "Success!", "2 work sets per exercise today", $"{startworkoutText}");
                modalPage.Disappearing += (sender2, e2) =>
                {
                    waitHandle.Set();
                };
                await PopupNavigation.Instance.PushAsync(modalPage);

                await Task.Run(() => waitHandle.WaitOne());
                StartTodaysWorkout();
            }

        }


        async void btnChangeWokoutNow_Clicked(object sender, System.EventArgs e)
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var msg = "";
            if (btnWelcomeStartWorkout.IsVisible)
                msg = "Next workout queued 18 hours after today's workout.";
            else
            {
                msg = "Work out today.";
            }

            var startworkoutText = "Start workout";
            if (LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}") != null && LocalDBManager.Instance.GetDBSetting($"Time{DateTime.Now.Year}").Value != null && LocalDBManager.Instance.GetDBSetting($"Exercises{DateTime.Now.Date}") != null && LocalDBManager.Instance.GetDBSetting($"Exercises{DateTime.Now.Date}").Value != null)
                startworkoutText = "Start workout";
            LocalDBManager.Instance.SetDBSetting($"WorkoutAdded{DateTime.Now.Date.AddDays(1)}", "true");
            var modalPage = new Views.GeneralPopup("TrueState.png", "Success!", msg, startworkoutText);
            modalPage.Disappearing += (sender2, e2) =>
            {
                waitHandle.Set();
            };
            await PopupNavigation.Instance.PushAsync(modalPage);

            await Task.Run(() => waitHandle.WaitOne());
            if (!btnWelcomeStartWorkout.IsVisible)
            {
                //LblWelcomebackText.Text = "Do your next workout today // Next workout queued 18 hours after today's workout.";
            }
            btnWelcomeStartWorkout.IsVisible = true;
            btnWelcomeGotit.IsVisible = false;
            StartTodaysWorkout();
        }

        async void btnGetSupport_Clicked(System.Object sender, System.EventArgs e)
        {

            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[2];
        }
        async void btnUpdateGoal_Clicked(System.Object sender, System.EventArgs e)
        {

            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Try again"
                });
                return;
            }

            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = "Update goal weight",
                MaxLength = 7,
                Placeholder = "Tap to enter your goal weight",
                OkText = AppResources.Ok,
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

                        LocalDBManager.Instance.SetDBSetting("WeightGoal", new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().Replace(",", "."));
                        var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                        var weights = new MultiUnityWeight(value, "kg");
                        LblBodyweight.Text = string.Format("{0:0.##}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weights.Kg : weights.Lb);
                        await DrMuscleRestClient.Instance.SetUserWeightGoal(new UserInfosModel()
                        {
                            WeightGoal = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value)
                        });
                        btnUpdateGoal.IsVisible = false;
                        btnMealPlan.IsVisible = true;

                        var userInfo = await DrMuscleRestClient.Instance.GetTargetIntake();
                        if (userInfo.TargetIntake != null)
                            LocalDBManager.Instance.SetDBSetting("TargetIntake", userInfo.TargetIntake.ToString());
                        LoadSavedWeightFromServer();
                        return;
                    }
                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);

        }

        void WeightCoachingDismiss1_Clicked(System.Object sender, System.EventArgs e)
        {
            var height = WeightCoachingCard1.HeightRequest;

            WeightCoachingCard1.IsVisible = false;
            WeightCoachingCard1.HeightRequest = height;
        }

        async void btnChart_Clicked(System.Object sender, System.EventArgs e)
        {
            await PagesFactory.PushAsync<MeCombinePage>();
        }

        async void btnHistory_Clicked(System.Object sender, System.EventArgs e)
        {
            CurrentLog.Instance.PastWorkoutDate = null;
            await PagesFactory.PushAsync<HistoryPage>();
        }
    }
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns an zero-based index where firstDayOfWeek = 0 and lastDayOfWeek = 6
        /// </summary>
        /// <param name="value"></param>
        /// <param name="firstDayOfWeek"></param>
        /// <returns>int between 0 and 6</returns>
        public static int DayOfWeek(this DateTime value, DayOfWeek firstDayOfWeek)
        {
            var idx = 7 + (int)value.DayOfWeek - (int)firstDayOfWeek;
            if (idx > 6) // week ends at 6, because Enum.DayOfWeek is zero-based
            {
                idx -= 7;
            }
            return idx;
        }
    }


}
