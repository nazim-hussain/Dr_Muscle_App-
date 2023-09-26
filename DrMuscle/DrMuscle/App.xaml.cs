using Acr.UserDialogs;
using DrMuscle.Screens.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using Xamarin.Forms;
using DrMuscle.Layout;
using System.Threading.Tasks;
using System.Diagnostics;
using DrMuscle.Screens.User.OnBoarding;
using DrMuscle.Helpers;
using DrMuscleWebApiSharedModel;
using System.Globalization;
using DrMuscle.Localize;
using DrMuscle.Dependencies;
using DrMuscle.Screens.Demo;
using DrMuscle.Constants;
using DrMuscle.Services;
using Plugin.FirebasePushNotification;
using DrMuscle.Message;
using DrMuscle.OnBoarding;
using Sentry;

namespace DrMuscle
{
    public partial class App : Application
    {
        private bool isLoading = false;
        private bool isLoadingDisplay = false;
        public SwapExerciseContextList swapExerciseContexts;
        private UserWorkoutContext userWorkoutContext;
        private WorkoutListContext workoutListContext;
        private WorkoutHistoryContext workoutHistoryContext;
        private WorkoutLogContext workoutLogContext;
        private NewRecordModelContext newRecordModelContext;
        public WeightsContext weightContext;
        public static bool IsNUX = false;
        public static bool IsIntro = false;
        public static bool IsIntroBack = false;
        public static bool IsConnectedToWatch = false; 
        public bool displayCreateNewAccount = true;
        public static bool IsEquipmentOpen = false;

        public static int workoutPerDay = 0;
        public static bool IsWelcomePopup1 = false;
        public static bool IsShowTooltip = false;
        public static bool IsWelcomePopup2 = false;
        public static bool IsExercisePopup = false;
        public static bool IsGymPopup = false;
        public static bool IsHomeGymPopup = false;
        public static bool IsBodyweightPopup = false;
        public static bool IsCustomPopup = false;
        public static bool IsLearnPopup = false;
        public static bool IsChatPopup = false;
        public static bool IsCongratulated = false;
        public static bool IsSettingsPopup = false;
        public static bool IsAskedLatestVersion = false;
        public static bool IsWelcomePopup3 = false;
        public static bool IsWelcomePopup4 = false;
        public static bool IsWelcomePopup5 = false;
        public static bool IsShowBackOffPopup = false;
        public static bool ShowAllSetPopup = false;
        public static bool MobilityWelcomePopup = false;
        public static bool IsPlatePopup = false;
        public static bool IsSupersetPopup = false;
        public static bool IsTrialExpiredPopup = false;
        public static bool IsSaveSetClicked = false;
        public static bool IsAddExercisesPopUp = false; 
        public static bool IsWelcomeBack = false;
        public static bool IsTakeTimeOff = false;
        public static bool IsOnboarding = false;
        public static bool IsSidemenuOpen = false;
        public static bool IsHowHardAsked = false;
        public static bool IsShowBackOffTooltip = false;
        public static bool IsFeaturedPopup = false;
        public static bool IsSurprisePopup = false;
        public static bool IsFreePlan = false;
        public static bool IsDisplayPopup = false;

        public static bool IsResizeScreen = false;
        public static bool IsNewUser = false;
        public static bool IsNewFirstTime = false;
        public static bool IsSleeping = false;
        public static bool ShowEasyExercisePopUp = false;

        public static bool IsFromNotificaion = false;

        public static double NavigationBarHeight = 44;
        public static double StatusBarHeight = 20;
        public static bool IsV1User = false;
        public static bool IsMealPlan = false;
        public static bool IsTraining = false;
        public static bool IsV1UserTrial = false;
        public static long WorkoutId = 0;
        public static long BodypartId = 0;
        public static int Days = 0;
        public static int globalTime = 0;
        public static decimal PCWeight= 0;
        public static bool IsDemoProgress = false;
        public static bool IsDemo1Progress = false;
        public static int IsMainPage = 0;
        public static List<bool> WelcomeTooltop { get; set; }
        public static List<bool> BackoffTooltop { get; set; }
        public static List<string> MutedUserList = new List<string>();
        public static bool IsStopEvent { get; set; }
        public static bool IsGoogle { get; set; }
        public static bool IsApple { get; set; }
        public static bool IsWebsite { get; set; }

        public static double ScreenWidth { get; set; }
        public static double ScreenHeight { get; set; }

        public App()
        {
            InitializeComponent();
            FinishedExercices = new List<ExerciceModel>();
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                CrossFirebasePushNotification.Current.OnNotificationOpened += Current_OnNotificationOpened;

        }

        private async void Current_OnNotificationOpened(object source, FirebasePushNotificationResponseEventArgs e)
        {
            try
            {
                try
                {
                    await Task.Delay(13000);                    Xamarin.Forms.MessagingCenter.Send<NavigationOnNotificationTappedMessage>(new NavigationOnNotificationTappedMessage(""), "NavigationOnNotificationTappedMessage");
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        public NavigationPage BuildNavigationPage(Page rootPage)
        {
            NoAnimationNavigationPage NavPage = new NoAnimationNavigationPage(rootPage);
            ((NavigationPage)NavPage).BarTextColor = Color.White;
            ((NavigationPage)NavPage).BackgroundImage = "nav.png";
            return NavPage;
        }

        private async void DrMaxMuscleRestClient_EndPost()
        {
            isLoading = false;
            await Task.Delay(500);
            bool isLoadingAfter = isLoading;
            if (isLoadingAfter)
                return;

            if (isLoadingDisplay)
            {
                Debug.WriteLine("HideLoading");
                UserDialogs.Instance.HideLoading();
                isLoadingDisplay = false;
                isLoading = false;
            }
        }

        private async void DrMaxMuscleRestClient_StartPost()
        {
            isLoading = true;
            if (!isLoadingDisplay)
            {
                Debug.WriteLine("ShowLoading");
                try
                {
                    await Task.Delay(50);
                    UserDialogs.Instance.ShowLoading("Loading...");
                    isLoadingDisplay = true;
                }
                catch (Exception ex)
                {
                    isLoading = false;
                }
            }
        }

        public SwapExerciseContextList SwapExericesContexts
        {
            get { return swapExerciseContexts; }
        }

        public WeightsContext WeightsContextList
        {
            get { return weightContext; }
        }

        public UserWorkoutContext UserWorkoutContexts
        {
            get { return userWorkoutContext; }
        }

        public WorkoutListContext WorkoutListContexts
        {
            get { return workoutListContext; }
        }

        public WorkoutHistoryContext WorkoutHistoryContextList
        {
            get { return workoutHistoryContext; }
        }

        public WorkoutLogContext WorkoutLogContext
        {
            get { return workoutLogContext; }
        }

        public NewRecordModelContext NewRecordModelContext
        {
            get { return newRecordModelContext; }
        }

        public IList<ExerciceModel> FinishedExercices { get; set; }

        protected override async void OnStart()
        {
            // Handle when your app starts
            
            AppCenter.Start("ios=a49dc22e-26b8-4d56-b1c1-2165a2e57107;android=8da898d0-1363-42ae-9495-98589b14d49b;", typeof(Analytics), typeof(Crashes));

            Crashes.GetErrorAttachments = (ErrorReport report) =>
            {
                string userMail = "not set";
                DBSetting mailSetting = LocalDBManager.Instance.GetDBSetting("email");
                if (mailSetting != null)
                    userMail = mailSetting.Value;

                return new ErrorAttachmentLog[]
                {
                    ErrorAttachmentLog.AttachmentWithText(userMail, "")
                };
            };

            SentrySdk.ConfigureScope(scope =>
            {
                scope.User = new User
                {
                    Email = LocalDBManager.Instance.GetDBSetting("email")?.Value
                };
            });

            AppCenter.LogLevel = LogLevel.None;
            CrossFirebasePushNotification.Current.OnTokenRefresh += Current_OnTokenRefresh;
            CrossFirebasePushNotification.Current.OnNotificationReceived += Current_OnNotificationReceived;


            CurrentLog.Instance.ShowWelcomePopUp = false;
            DrMuscleRestClient.Instance.StartPost += DrMaxMuscleRestClient_StartPost;
            DrMuscleRestClient.Instance.EndPost += DrMaxMuscleRestClient_EndPost;
            DBSetting dbOnBoardingSeen = LocalDBManager.Instance.GetDBSetting("onboarding_seen");
            DBSetting dbToken = LocalDBManager.Instance.GetDBSetting("token");
            DBSetting dbTokenExpirationDate = LocalDBManager.Instance.GetDBSetting("token_expires_date");
            DBSetting dbFirstname = LocalDBManager.Instance.GetDBSetting("firstname");
            if (LocalDBManager.Instance.GetDBSetting("timer_count") != null)
                LocalDBManager.Instance.SetDBSetting("timer_remaining", LocalDBManager.Instance.GetDBSetting("timer_count").Value);
            else
                LocalDBManager.Instance.SetDBSetting("timer_count", "60");
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
            swapExerciseContexts = SwapExerciseContextList.LoadContexts();
            userWorkoutContext = UserWorkoutContext.LoadContexts();
            workoutListContext = WorkoutListContext.LoadContexts();
            workoutHistoryContext = WorkoutHistoryContext.LoadContexts();
            workoutLogContext = WorkoutLogContext.LoadContexts();
            weightContext = WeightsContext.LoadContexts();
            newRecordModelContext = NewRecordModelContext.LoadContexts();
            Page startPage = PagesFactory.GetNavigation();



//#if DEBUG
//            IsDemoProgress = true;
//            PagesFactory.PushAsync<MainOnboardingPage>();
//            MainPage = startPage;
//            return;
//#endif

            if (LocalDBManager.Instance.GetDBSetting("AppLanguage") != null)
            {
                var localize = DependencyService.Get<ILocalize>();
                if (localize != null)
                {
                    ResourceLoader.Instance.SetCultureInfo(new CultureInfo(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value));
                    localize.SetLocale(new CultureInfo(LocalDBManager.Instance.GetDBSetting("AppLanguage").Value));
                }
            }

            if (dbOnBoardingSeen == null || dbOnBoardingSeen.Value == "false")
            {
                displayCreateNewAccount = false;
                PagesFactory.PopThenPushAsync<WalkThroughPage>();

                // string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
                // if (string.IsNullOrEmpty(val))
                // {
                //     if (DateTime.Now.Minute % 2 == 0)
                //     {
                //         LocalDBManager.Instance.SetDBSetting("BetaVersion", "Beta");
                //         PagesFactory.PopThenPushAsync<WalkThroughPage>();
                //         
                //     }
                //     else
                //     {
                //         LocalDBManager.Instance.SetDBSetting("BetaVersion", "Normal");
                //         PagesFactory.PopThenPushAsync<WalkThroughPage>();
                //         
                //     }
                // }
                // else
                // {
                //     if (val.Equals("Beta"))
                //     {
                //         LocalDBManager.Instance.SetDBSetting("BetaVersion", "Beta");
                //         PagesFactory.PopThenPushAsync<WalkThroughPage>();
                //         
                //     }
                //     else
                //     {
                //         PagesFactory.PopThenPushAsync<WalkThroughPage>();
                //     }
                // }

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
            }
            else
            {
                if (dbToken != null && dbTokenExpirationDate != null && DateTime.Now < new DateTime(Convert.ToInt64(dbTokenExpirationDate.Value)))
                {
                    displayCreateNewAccount = true;
                    if (LocalDBManager.Instance.GetDBSetting("SetStyle") == null)
                        LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    DrMuscleRestClient.Instance.SetToken(LocalDBManager.Instance.GetDBSetting("token").Value);
                    //startPage = BuildNavigationPage(new MainPage());
                    PagesFactory.PushMainTabbed<MainTabbedPage>();


                    if (LocalDBManager.Instance.GetDBSetting("FirstStepCompleted") != null && LocalDBManager.Instance.GetDBSetting("FirstStepCompleted").Value == "true")
                    {
                        IsDemoProgress = true;
                        PagesFactory.PushAsync<MainOnboardingPage>();
                        

                    }
                    else
                        RefreshUsersSettings();
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
                }
                else
                {
                    //startPage = BuildNavigationPage(new WelcomePage());
                    displayCreateNewAccount = true;
                    PagesFactory.PopThenPushAsync<WelcomePage>();
                    //PagesFactory.GetNavigation().PushAsync(new WalkThroughPage());
                }

            }

            //startPage = BuildNavigationPage(new OnboardingPage1Welcome());
            MainPage = startPage;

            if (Config.SecondOpenEventTrack == 1)
            {
                //Send event
                DependencyService.Get<IFirebase>().LogEvent("second_open", "Open");
                Config.SecondOpenEventTrack = 2;
            }
            WelcomeTooltop = new List<bool>();
            WelcomeTooltop.Add(false);
            WelcomeTooltop.Add(false);
            WelcomeTooltop.Add(false);

            BackoffTooltop = new List<bool>();
            BackoffTooltop.Add(false);
            BackoffTooltop.Add(false);
            BackoffTooltop.Add(false);
            if (Config.ShowWelcomePopUp2 || Config.ViewWebHistoryPopup || Config.ShowWelcomePopUp3 || LocalDBManager.Instance.GetDBSetting("email") == null)
                return;
            if (Config.SecondOpenEventTrack == 0)
                Config.SecondOpenEventTrack = 1;




        }

        private void Current_OnNotificationReceived(object source, FirebasePushNotificationDataEventArgs e)
        {
            try
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Tabs[0].BadgeCaption = 1;
                });
            }
            catch (Exception ex)
            {

            }
        }

        private void Current_OnTokenRefresh(object source, FirebasePushNotificationTokenEventArgs e)
        {
            Config.RegisteredDeviceToken = e.Token;
            RegisterDeviceToken();
        }

        protected override void OnSleep()
        {
            IsSleeping = true;
            // Handle when your app sleeps
            
           // Timer.Instance.StopTimer();
        }

        protected override void OnResume()
        {
            IsSleeping = false;
            // Handle when your app resumes
        }

        private async void RefreshUsersSettings()
        {
            decimal? sliderVal = null;
            if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("SlierValue")?.Value))
            {
                sliderVal = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("SlierValue")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                await DrMuscleRestClient.Instance.SetUserBarWeight(new UserInfosModel()
                {
                    KgBarWeight = sliderVal,
                    LbBarWeight = sliderVal,
                    MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lb"
                });
                LocalDBManager.Instance.SetDBSetting("SlierValue", null);
            }
            var uim = await DrMuscleRestClient.Instance.GetUserInfoWithoutLoader();
            try
            {
                if (uim == null)
                    return;
                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Reminder5th", uim.IsReminder ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("MaxWorkoutDuration", uim.WorkoutDuration.ToString());
                LocalDBManager.Instance.SetDBSetting("LastWorkoutWas", uim.LastWorkoutWas);
                LocalDBManager.Instance.SetDBSetting("RecommendedReminder", uim.IsRecommendedReminder == true ? "true" : uim.IsRecommendedReminder == null ? "null" : "false");
                if (uim.Height != null)
                    LocalDBManager.Instance.SetDBSetting("Height", uim.Height.ToString());
                if (uim.TargetIntake != null)
                    LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                SetupEquipment(uim);
                if (uim.Age != null)
                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                if (uim.ReminderTime != null)
                    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                if (uim.ReminderDays != null)
                    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);
                if (!string.IsNullOrEmpty(uim.SwappedJson))
                {
                    LocalDBManager.Instance.SetDBSetting("swap_exericse_contexts", uim.SwappedJson);
                    ((App)Application.Current).swapExerciseContexts = SwapExerciseContextList.LoadContexts();
                }

                LocalDBManager.Instance.SetDBSetting("DailyReset", Convert.ToString(uim.DailyExerciseCount));
                LocalDBManager.Instance.SetDBSetting("WeeklyReset", Convert.ToString(uim.WeeklyExerciseCount));


                LocalDBManager.Instance.SetDBSetting("IsEmailReminder", uim.IsReminderEmail ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("ReminderHours",uim.ReminderBeforeHours.ToString());

                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());
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
                        return;
                    }
                    if (strDays.ToCharArray().Length == 7)
                    {
                        var IsSunday = strDays[0] == '1';
                        var IsMonday = strDays[1] == '1';
                        var IsTuesday = strDays[2] == '1';
                        var IsWednesday = strDays[3] == '1';
                        var IsThursday = strDays[4] == '1';
                        var IsFriday = strDays[5] == '1';
                        var IsSaturday = strDays[6] == '1';

                        var ReminderDays = "";
                        ReminderDays = IsSunday ? "1" : "0";
                        ReminderDays += IsMonday ? "1" : "0";
                        ReminderDays += IsTuesday ? "1" : "0";
                        ReminderDays += IsWednesday ? "1" : "0";
                        ReminderDays += IsThursday ? "1" : "0";
                        ReminderDays += IsFriday ? "1" : "0";
                        ReminderDays += IsSaturday ? "1" : "0";
                        IAlarmAndNotificationService alarmAndNotificationService = new AlarmAndNotificationService();
                        alarmAndNotificationService.CancelNotification(101);
                        alarmAndNotificationService.CancelNotification(102);
                        alarmAndNotificationService.CancelNotification(103);
                        alarmAndNotificationService.CancelNotification(104);
                        alarmAndNotificationService.CancelNotification(105);
                        alarmAndNotificationService.CancelNotification(106);
                        alarmAndNotificationService.CancelNotification(107);
                        alarmAndNotificationService.CancelNotification(108);

                        var day = 0;
                        if (IsSunday)
                        {
                            if (DayOfWeek.Sunday - DateTime.Now.DayOfWeek < 0)
                            {
                                day = 7 + (DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
                            }
                            else
                            {
                                day = DayOfWeek.Sunday - DateTime.Now.DayOfWeek;
                            }
                            var timeSpan = new TimeSpan(day, timePickerSpan.Hours, timePickerSpan.Minutes, timePickerSpan.Seconds);
                            alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 101, NotificationInterval.Week);
                        }
                        if (IsMonday)
                        {
                            if (DayOfWeek.Monday - DateTime.Now.DayOfWeek < 0)
                            {
                                day = 7 + (DayOfWeek.Monday - DateTime.Now.DayOfWeek);
                            }
                            else
                            {
                                day = DayOfWeek.Monday - DateTime.Now.DayOfWeek;
                            }
                            var timeSpan = new TimeSpan(day, timePickerSpan.Hours, timePickerSpan.Minutes, timePickerSpan.Seconds);
                            alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 102, NotificationInterval.Week);
                        }
                        if (IsTuesday)
                        {
                            if (DayOfWeek.Tuesday - DateTime.Now.DayOfWeek < 0)
                            {
                                day = 7 + (DayOfWeek.Tuesday - DateTime.Now.DayOfWeek);
                            }
                            else
                            {
                                day = DayOfWeek.Tuesday - DateTime.Now.DayOfWeek;
                            }
                            var timeSpan = new TimeSpan(day, timePickerSpan.Hours, timePickerSpan.Minutes, timePickerSpan.Seconds);
                            alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 103, NotificationInterval.Week);
                        }
                        if (IsWednesday)
                        {
                            if (DayOfWeek.Wednesday - DateTime.Now.DayOfWeek < 0)
                            {
                                day = 7 + (DayOfWeek.Wednesday - DateTime.Now.DayOfWeek);
                            }
                            else
                            {
                                day = DayOfWeek.Wednesday - DateTime.Now.DayOfWeek;
                            }
                            var timeSpan = new TimeSpan(day, timePickerSpan.Hours, timePickerSpan.Minutes, timePickerSpan.Seconds);
                            alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 104, NotificationInterval.Week);
                        }
                        if (IsThursday)
                        {
                            if (DayOfWeek.Thursday - DateTime.Now.DayOfWeek < 0)
                            {
                                day = 7 + (DayOfWeek.Thursday - DateTime.Now.DayOfWeek);
                            }
                            else
                            {
                                day = DayOfWeek.Thursday - DateTime.Now.DayOfWeek;
                            }
                            var timeSpan = new TimeSpan(day, timePickerSpan.Hours, timePickerSpan.Minutes, timePickerSpan.Seconds);
                            alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 105, NotificationInterval.Week);
                        }
                        if (IsFriday)
                        {
                            if (DayOfWeek.Friday - DateTime.Now.DayOfWeek < 0)
                            {
                                day = 7 + (DayOfWeek.Friday - DateTime.Now.DayOfWeek);
                            }
                            else
                            {
                                day = DayOfWeek.Friday - DateTime.Now.DayOfWeek;
                            }
                            var timeSpan = new TimeSpan(day, timePickerSpan.Hours, timePickerSpan.Minutes, timePickerSpan.Seconds);
                            alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 106, NotificationInterval.Week);
                        }
                        if (IsSaturday)
                        {
                            if (DayOfWeek.Saturday - DateTime.Now.DayOfWeek < 0)
                            {
                                day = 7 + (DayOfWeek.Saturday - DateTime.Now.DayOfWeek);
                            }
                            else
                            {
                                day = DayOfWeek.Saturday - DateTime.Now.DayOfWeek;
                            }
                            var timeSpan = new TimeSpan(day, timePickerSpan.Hours, timePickerSpan.Minutes, timePickerSpan.Seconds);
                            alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout? You got this!", timeSpan, 107, NotificationInterval.Week);
                        }
                    }
                }
                if (uim.IsPyramid)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsRPyramid", "true");
                }
                else if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
                }
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false");

                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                }
                if (uim.SetCount != null)
                {
                    LocalDBManager.Instance.SetDBSetting("WorkSetCount", Convert.ToString(uim.SetCount));
                }
                if (uim.Increments != null)
                    LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
                if (uim.Max != null)
                    LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
                if (uim.Min != null)
                    LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
                if (uim.BodyWeight != null)
                {
                    LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
                }
                if (uim.WeightGoal != null)
                {
                    LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                }
                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                ((App)Application.Current).displayCreateNewAccount = true;

                
            }
            catch (Exception ex)
            {

            }

           // RegisterDeviceToken();
        }

        public static async void RegisterDeviceToken()
        {
            if (string.IsNullOrEmpty(Config.RegisteredDeviceToken))
                return;
            
           
        }

        private void SetupEquipment(UserInfosModel uim)
        {
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("email")?.Value == null)
                    return;
                LocalDBManager.Instance.SetDBSetting("KgBarWeight", uim.KgBarWeight == null ? "20" : Convert.ToString(uim.KgBarWeight).ReplaceWithDot());
                LocalDBManager.Instance.SetDBSetting("LBBarWeight", uim.LbBarWeight == null ? "45" : Convert.ToString(uim.LbBarWeight).ReplaceWithDot());

                if (uim.EquipmentModel != null)
            {
                LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("HomeMainEquipment", uim.EquipmentModel.IsHomeEquipmentEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomeChinUp", uim.EquipmentModel.IsHomeChinupBar ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomeDumbbell", uim.EquipmentModel.IsHomeDumbbell ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomePlate", uim.EquipmentModel.IsHomePlate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("HomePully", uim.EquipmentModel.IsHomePully ? "true" : "false");


                LocalDBManager.Instance.SetDBSetting("OtherMainEquipment", uim.EquipmentModel.IsOtherEquipmentEnabled ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherChinUp", uim.EquipmentModel.IsOtherChinupBar ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherDumbbell", uim.EquipmentModel.IsOtherDumbbell ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherPlate", uim.EquipmentModel.IsOtherPlate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("OtherPully", uim.EquipmentModel.IsOtherPully ? "true" : "false");

                    if (uim.EquipmentModel.Active == "gym")
                    LocalDBManager.Instance.SetDBSetting("GymEquipment", "true");
                if (uim.EquipmentModel.Active == "home")
                    LocalDBManager.Instance.SetDBSetting("HomeEquipment", "true");
                if (uim.EquipmentModel.Active == "other")
                    LocalDBManager.Instance.SetDBSetting("OtherEquipment", "true");
            }
            else
            {
                LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                LocalDBManager.Instance.SetDBSetting("Plate", "true");
                LocalDBManager.Instance.SetDBSetting("Pully", "true");

                LocalDBManager.Instance.SetDBSetting("HomeMainEquipment", "false");
                LocalDBManager.Instance.SetDBSetting("HomeChinUp", "true");
                LocalDBManager.Instance.SetDBSetting("HomeDumbbell", "true");
                LocalDBManager.Instance.SetDBSetting("HomePlate", "true");
                LocalDBManager.Instance.SetDBSetting("HomePully", "true");

                LocalDBManager.Instance.SetDBSetting("OtherEquipment", "false");
                LocalDBManager.Instance.SetDBSetting("OtherChinUp", "true");
                LocalDBManager.Instance.SetDBSetting("OtherDumbbell", "true");
                LocalDBManager.Instance.SetDBSetting("OtherPlate", "true");
                LocalDBManager.Instance.SetDBSetting("OtherPully", "true");

                }


            }
            catch (Exception ex)
            {

            }
        }
    }
}
