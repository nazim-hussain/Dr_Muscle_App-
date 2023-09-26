using DrMuscle.Dependencies;
using DrMuscle.Screens.Subscription;
using DrMuscle.Screens.User;
using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acr.UserDialogs;
using Xamarin.Forms;
using System.Diagnostics;
using DrMuscle.Screens.History;
using DrMuscle.Helpers;
using System.Text.RegularExpressions;
using DrMuscleWebApiSharedModel;
using DrMuscle.Resx;
using DrMuscle.Screens.Workouts;
using DrMuscle.Screens.User.OnBoarding;
using DrMuscle.Screens.Me;
using System.Collections.ObjectModel;
using DrMuscle.Message;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;
using Splat;
using DrMuscle.Screens.Demo;
using DrMuscle.Services;
using Xamarin.Essentials;
using DrMuscle.Effects;
using DrMuscle.Constants;
using System.Threading;

namespace DrMuscle.Layout
{
    public partial class RightSideMasterPage : SlideMenuView
    {
        public List<ReviewsModel> reviewList = new List<ReviewsModel>();
        private bool _isTimerTooltip = false;
        private bool _isTimerSettingsUpdating = false;
        public RightSideMasterPage()
        {
            InitializeComponent();
            // You must set IsFullScreen in this case, 
            // otherwise you need to set HeightRequest, 
            // just like the QuickInnerMenu sample
            this.IsFullScreen = true;
            // You must set WidthRequest in this case
            //if (App.IsMainPage == 5)
            //    this.WidthRequest = DeviceDisplay.MainDisplayInfo.Density > 1 ? DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density : DeviceDisplay.MainDisplayInfo.Width;
            //else
                this.WidthRequest = 260;// DeviceDisplay.MainDisplayInfo.Density > 1 ? DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density : DeviceDisplay.MainDisplayInfo.Width;
            this.MenuOrientations = MenuOrientation.RightToLeft;
            
            // You must set BackgroundColor, 
            // and you cannot put another layout with background color cover the whole View
            // otherwise, it cannot be dragged on Android
            this.BackgroundColor = Color.White;
            this.Opacity = 1.0;
            // This is shadow view color, you can set a transparent color
            this.BackgroundViewColor = Color.FromHex("#99000000");
            
            VersionInfoLabel.Text = DependencyService.Get<IDrMuscleSubcription>().GetBuildVersion().Replace("Version", AppResources.Version).Replace("Build", AppResources.Build);
            reviewList = GetReviews();
            //HomeButton.Clicked += async (object sender, EventArgs e) =>
            //{
            //    HideWithoutAnimations();
            //    //await PagesFactory.PushAsync<MainPage>();
            //    try
            //    {
            //        var navigation = (((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).CurrentPage.Navigation);
            //        if ((DrMusclePage)navigation.NavigationStack[0] is MainAIPage)
            //        {
            //            await PagesFactory.PushAsync<MainAIPage>();
            //        }
            //        else
            //        {
            //            ((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).CurrentPage = ((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).Children[0];
            //            await PagesFactory.PushAsync<MainAIPage>();
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //};
            ShortOnTimeButton.Clicked += async (sender, e) => {
                HideWithoutAnimations();
                MessagingCenter.Send<HomeOptionsMessage>(new HomeOptionsMessage() { Options= ShortOnTimeButton.Text }, "HomeOptionsMessage");
            };
            TiredTodayButton.Clicked += async (sender, e) => {
                HideWithoutAnimations();
                MessagingCenter.Send<HomeOptionsMessage>(new HomeOptionsMessage() { Options = TiredTodayButton.Text }, "HomeOptionsMessage");
            };
            MoreStatsButton.Clicked += async (sender, e) => {
                HideWithoutAnimations();
                await PagesFactory.PushAsync<MeCombinePage>();
            };


            //NewNUXButton.Clicked += async (sender, e) => {
            //    HideWithoutAnimations();
            //    CurrentLog.Instance.IsDemoRunningStep2 = false;
            //    await PagesFactory.PushAsync<MainOnboardingPage>();
            //};
            //
            //StartWorkoutButton.Clicked += async (sender, e) => {
            //    HideWithoutAnimations();
            //    MessagingCenter.Send<HomeOptionsMessage>(new HomeOptionsMessage() { Options = "startworkout" }, "HomeOptionsMessage");
            //};
            SharefreeMonthButton.Clicked += async (sender, e) => {
                HideWithoutAnimations();
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
            };

            MeGesture.Tapped += async (sender, e) =>
            {
                HideWithoutAnimations();
                await PagesFactory.PushAsync<SettingsPage>();
            };

            ChartsGesture.Tapped += async (sender, e) =>
            {
                HideWithoutAnimations();
                await PagesFactory.PushAsync<MeCombinePage>();
            };
            SettingGesture.Tapped += async (object sender, EventArgs e) =>
            {
                HideWithoutAnimations();
                await PagesFactory.PushAsync<SettingsPage>();
            };

            //WorkoutsButton.Clicked += async (sender, e) =>
            //{
            //    HideWithoutAnimations();
            //    await PagesFactory.PushAsync<ChooseDrMuscleOrCustomPage>();
            //};
            //ChatButton.Clicked += async (object sender, EventArgs e) =>
            //{
            //    HideWithoutAnimations();
            //    await PagesFactory.PushAsync<ChatPage>();
            //};

            //ManageExercise.Clicked += async (object sender, EventArgs e) =>
            //{
            //    HideWithoutAnimations();
            //    await PagesFactory.PushAsync<ChooseDrMuscleOrCustomExercisePage>();
            //};

            HistoryGesture.Tapped += async (object sender, EventArgs e) =>
            {
                HideWithoutAnimations();
                CurrentLog.Instance.PastWorkoutDate = null; 
                await PagesFactory.PushAsync<HistoryPage>();
            };

            SubscriptionGesture.Tapped += async (object sender, EventArgs e) =>
            {
                HideWithoutAnimations();
                await PagesFactory.PushAsync<SubscriptionPage>();
            };
            //WebGesture.Tapped += (sender, e) =>
            //{
            //    Device.OpenUri(new Uri("https://my.dr-muscle.com"));
            //};

            WebGestures.Tapped += (sender, e) =>
            {
                Device.OpenUri(new Uri("https://my.dr-muscle.com"));
            };
            TellAFriendGesture.Tapped += (sender, e) =>
            {
                var firstname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
                if (Device.RuntimePlatform.Equals(Device.Android))
                {

                    Xamarin.Essentials.Share.RequestAsync(new Xamarin.Essentials.ShareTextRequest
                    {
                        Uri = $"https://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=sidebar&utm_content={firstname}",
                        Subject = $"{firstname} is inviting you to try Dr.Muscle, the app that gets you in shape faster using artificial intelligence"
                    });
                }
                else
                    Xamarin.Essentials.Share.RequestAsync($"{firstname} is inviting you to try Dr.Muscle, the app that gets you in shape faster using artificial intelligence \nhttps://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=sidebar&utm_content={firstname}");
                //if (Device.RuntimePlatform.Equals(Device.Android))
                //Xamarin.Essentials.Share.RequestAsync("Check out this new app! For your fitness. \n\n\"Dr.Muscle gets you in shape fast like a personal trainer\" \nhttps://play.google.com/store/apps/details?id=com.drmaxmuscle.dr_max_muscle&hl=en");
                //else
                //    Xamarin.Essentials.Share.RequestAsync("Check out this new app! For your fitness. \n\n\"Dr.Muscle gets you in shape fast like a personal trainer\" \nhttps://itunes.apple.com/app/dr-muscle/id1073943857?mt=8");
                DependencyService.Get<IFirebase>().LogEvent("told_a_friend","share");
            };
            //EmailUsButton.Clicked += (object sender, EventArgs e) =>
            //{
            //    HideWithoutAnimations();
            //    Device.OpenUri(new Uri("mailto:support@drmuscleapp.com"));
            //};

            LogoutGesture.Tapped += async (object sender, EventArgs e) =>
            {
                HideWithoutAnimations();
                RemoveToken();
                CancelNotification();
                LocalDBManager.Instance.Reset();
                CurrentLog.Instance.Reset();
                App.IsV1User = false;
                App.IsV1UserTrial = false;
                App.IsFreePlan = false;
                App.IsCongratulated = false;
                App.IsSupersetPopup = false;
                ((App)Application.Current).UserWorkoutContexts.workouts = new GetUserWorkoutLogAverageResponse();
                ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                ((App)Application.Current).WorkoutHistoryContextList.Histories = new List<HistoryModel>();
                ((App)Application.Current).WorkoutHistoryContextList.SaveContexts();
                ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
                ((App)Application.Current).WorkoutLogContext.SaveContexts();
                try
                {
                    if (((global::DrMuscle.MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage.Navigation.NavigationStack[0] is LearnPage)
                        ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).SelectedItem = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[0];
                }
                catch (Exception ex)
                {

                }
                await PagesFactory.PopToRootAsync();
                ((App)Application.Current).displayCreateNewAccount = true;
                await PagesFactory.PushAsync<WelcomePage>(true);

            };

            SignInButton.Clicked += async (object sender, EventArgs e) =>
            {
                ((App)Application.Current).displayCreateNewAccount = true;
                MainOnboardingPage.IsMovedToLogin = true;
                HideWithoutAnimations();
                PagesFactory.PopThenPushAsync<WelcomePage>(true);
            };

            SkipDemoButton.Clicked += async (object sender, EventArgs e) =>
            {
                
                HideWithoutAnimations();
                CurrentLog.Instance.EndExerciseActivityPage = GetType();
                //await Task.Delay(300);


                //if (Device.RuntimePlatform.Equals(Device.Android))
                //{
                //    App.IsDemoProgress = false;
                //    App.IsWelcomeBack = true;
                //    App.IsNewUser = true;
                //    LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                //    CurrentLog.Instance.Exercise1RM.Clear();
                //    Device.BeginInvokeOnMainThread(async () =>
                //    {
                //        await PagesFactory.PopToRootAsync(true);
                //        await Task.Delay(1000);
                //        MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
                //    });
                //}
                //else
                //{

                //    App.IsDemoProgress = false;
                //    App.IsWelcomeBack = true;
                //    App.IsNewUser = true;
                //    LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                //    CurrentLog.Instance.Exercise1RM.Clear();

                //      await PagesFactory.PopToRootMoveAsync(true);

                //    await Task.Delay(1000);
                //    MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");


                //}
                var modalPage1 = new Views.WelcomeAIOverlay();
                PopupNavigation.Instance.PushAsync(modalPage1);
                modalPage1.SetDetails("", CurrentLog.Instance.AiDescription);

            };

            //OldHomeButton.Clicked += async (object sender, EventArgs e) =>
            //{
            //    HideWithoutAnimations();
            //    await PagesFactory.PushAsync<MainPage>();
            //};
            FAQGesture.Tapped += async (object sender, EventArgs e) =>
            {
                HideWithoutAnimations();
                await PagesFactory.PushAsync<FAQPage>();
            };

            CancelButton.Clicked += async (sender, e) =>
            {
                HideWithoutAnimations();
                await PagesFactory.PopAsync(true);
            };

                LanguageButton.Clicked += (sender, e) =>
            {
                HideWithoutAnimations();
                Device.BeginInvokeOnMainThread(() =>
                {
                    var p = new LanguagesPage();
                    p.OnBeforeShow();
                    Navigation.PushModalAsync(p);
                });
            };

            RestartSetupButton.Clicked += (sender, e) =>
            {
                HideWithoutAnimations();
                App.IsDemoProgress = false;
                CurrentLog.Instance.IsDemoRunningStep2 = false;
                PagesFactory.PopToPage<MainOnboardingPage>(true);
            };
            RestartDemoButton.Clicked += async (sender, e) =>
            {
                HideWithoutAnimations();
                App.IsDemoProgress = false;
                App.IsDemo1Progress = false;
                CurrentLog.Instance.IsDemoRunningStep2 = false;
                CurrentLog.Instance.IsDemoRunningStep1 = true;
                CurrentLog.Instance.IsDemoPopingOut = true;
                CurrentLog.Instance.IsRestarted = true;
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
                App.IsDemoProgress = true;
                LocalDBManager.Instance.SetDBSetting("DemoProgress", "true");
                CurrentLog.Instance.Exercise1RM.Clear();
                await PagesFactory.PopToPage<NewDemoPage>();
                CurrentLog.Instance.IsDemoPopingOut = false;
            };
            if (LocalDBManager.Instance.GetDBSetting("timer_vibrate") == null)
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_sound") == null)
                LocalDBManager.Instance.SetDBSetting("timer_sound", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_reps_sound") == null)
            {
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", "true");
                LocalDBManager.Instance.SetDBSetting("timer_sound", "false");
            }

            if (LocalDBManager.Instance.GetDBSetting("timer_123_sound") == null)
                LocalDBManager.Instance.SetDBSetting("timer_123_sound", "true");
                
            

            if (LocalDBManager.Instance.GetDBSetting("timer_autostart") == null)
                LocalDBManager.Instance.SetDBSetting("timer_autostart", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_autoset") == null)
                LocalDBManager.Instance.SetDBSetting("timer_autoset", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen") == null)
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_remaining") == null)
                LocalDBManager.Instance.SetDBSetting("timer_remaining", "60");

            if (LocalDBManager.Instance.GetDBSetting("reprange") == null)
                LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");


            TimerEntry.Text = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
            _isTimerSettingsUpdating = true;
            VibrateSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_vibrate").Value);
            SoundSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_sound").Value);
            RepsSoundSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_reps_sound").Value);
            Timer123Switch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_123_sound").Value);
            if (SoundSwitch.IsToggled && RepsSoundSwitch.IsToggled)
            {
                SoundSwitch.IsToggled = false;
                LocalDBManager.Instance.SetDBSetting("timer_sound", "false");
            }
            AutostartSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_autostart").Value);
            AutosetSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_autoset").Value);
            FullscreenSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value);
            _isTimerSettingsUpdating = false;
            TimerEntry.Text = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
            if (LocalDBManager.Instance.GetDBSetting("timer_remaining").Value != null)
            {
                try
                {
                    App.globalTime = int.Parse(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value);

                }
                catch (Exception ex)
                {
                    LocalDBManager.Instance.SetDBSetting("timer_remaining", "60");
                    App.globalTime = 60;
                    TimerEntry.Text = "60";
                }
            }

            TimerStartButton.Clicked += async (sender, e) =>
            {
                if (TimerEntry.Text.Length == 0)
                    return;
                Debug.WriteLine(Timer.Instance.Remaining.ToString());

                Timer.Instance.Remaining = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value);
                if (Timer.Instance.Remaining == 0)
                    return;
                // 
                try
                {

                    if (((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage is DrMusclePage)
                        ((DrMusclePage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).HideTimerIcon();
                    else
                    {
                        var navigation = (((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).CurrentPage.Navigation);
                        ((DrMusclePage)navigation.NavigationStack[navigation.NavigationStack.Count - 1]).HideTimerIcon();
                        //((DrMusclePage)((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).CurrentPage.Navigation.NavigationStack[0]).HideTimerIcon();
                    }

                }
                catch (Exception ex)
                {

                }
                if (Timer.Instance.State != "RUNNING")
                {
                    Timer.Instance.StartTimer();
                    HideWithoutAnimations();
                    TimerStartButton.Text = "STOP";
                    if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value == "true")
                        await PagesFactory.PushAsync<TimerOverlay>(true);

                }
                else
                {
                    HideWithoutAnimations();
                    TimerStartButton.Text = "START";
                    await Timer.Instance.StopTimer();
                }

            };

            TimerLess.Clicked += (sender, e) =>
            {
                if (TimerEntry.Text.Length == 0)
                    return;
                TimerLess.Unfocus();
                int current = Convert.ToInt32(TimerEntry.Text);
                if (current >= 5)
                    current = current - 5;
                TimerEntry.Text = current.ToString();
                Timer.Instance.Remaining = current;
                UpdateTimeCountWithoutLoader(current);
                App.globalTime = current;

                if (_isTimerTooltip)
                {
                    _isTimerTooltip = false;
                    TooltipEffect.SetPosition(TimerStartButton, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(TimerStartButton, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(TimerStartButton, Color.White);
                    TooltipEffect.SetText(TimerStartButton, $"Start timer");
                    TooltipEffect.SetHasTooltip(TimerStartButton, true);
                    TooltipEffect.SetHasShowTooltip(TimerStartButton, true);
                }

            };
            
            TimerMore.Clicked += (sender, e) =>
            {
                if (TimerEntry.Text.Length == 0)
                    return;
                TimerLess.Unfocus();
                int current = Convert.ToInt32(TimerEntry.Text);
                current = current + 5;
                TimerEntry.Text = current.ToString();
                UpdateTimeCountWithoutLoader(current);
                Timer.Instance.Remaining = current;
                App.globalTime = current;

                if (_isTimerTooltip)
                {
                    TooltipEffect.SetPosition(TimerLess, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(TimerLess, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(TimerLess, Color.White);
                    TooltipEffect.SetText(TimerLess, $"Remove seconds");
                    TooltipEffect.SetHasTooltip(TimerLess, true);
                    TooltipEffect.SetHasShowTooltip(TimerLess, true);
                }
            };

            TimerEntry.TextChanged += (sender, e) =>
            {
                try
                {
                    if (TimerEntry.Text.Length == 0)
                    {
                        return;
                    }
                    const string textRegex = @"^\d+(?:)?$";
                bool IsValid = Regex.IsMatch(TimerEntry.Text, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
                if (IsValid == false)
                    TimerEntry.Text = e.OldTextValue;
                if (TimerEntry.Text.Length == 0)
                {
                    return;
                }

                if (",.".Contains(TimerEntry.Text[TimerEntry.Text.Length - 1].ToString()))
                    TimerEntry.Text = TimerEntry.Text.Substring(0, TimerEntry.Text.Length - 1);
                LocalDBManager.Instance.SetDBSetting("timer_remaining", TimerEntry.Text);
                UpdateTimeCountWithoutLoader(int.Parse(TimerEntry.Text));
                App.globalTime = Convert.ToInt32(TimerEntry.Text);

                }
                catch (Exception ex)
                {

                }
            };

            VibrateSwitch.Toggled += (sender, e) =>
            {
                if (VibrateSwitch.IsToggled)
                    LocalDBManager.Instance.SetDBSetting("timer_vibrate", "true");
                else
                    LocalDBManager.Instance.SetDBSetting("timer_vibrate", "false");
                UpdateTimerSettings();

            };

            SoundSwitch.Toggled += (sender, e) =>
            {
                if (!_isTimerSettingsUpdating)
                { 
                if (SoundSwitch.IsToggled)
                    {
                        RepsSoundSwitch.IsToggled = false;
                        LocalDBManager.Instance.SetDBSetting("timer_reps_sound", "false");
                        LocalDBManager.Instance.SetDBSetting("timer_sound", "true");
                    }
                else
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_sound", "false");
                    }
                UpdateTimerSettings();
                }

            };

            RepsSoundSwitch.Toggled += (sender, e) =>
            {
                if (!_isTimerSettingsUpdating)
                {
                    if (RepsSoundSwitch.IsToggled)
                    {
                        SoundSwitch.IsToggled = false;
                        LocalDBManager.Instance.SetDBSetting("timer_sound", "false");
                        LocalDBManager.Instance.SetDBSetting("timer_reps_sound", "true");
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_reps_sound", "false");
                    }
                    UpdateTimerSettings();
                }
            };
            //
            Timer123Switch.Toggled += (sender, e) =>
            {
                if (!_isTimerSettingsUpdating)
                {
                    if (Timer123Switch.IsToggled)
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_123_sound", "true");
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("timer_123_sound", "false");
                    }
                    UpdateTimerSettings();
                }
            };
            AutostartSwitch.Toggled += (sender, e) =>
            {
                if (AutostartSwitch.IsToggled)
                    LocalDBManager.Instance.SetDBSetting("timer_autostart", "true");
                else
                    LocalDBManager.Instance.SetDBSetting("timer_autostart", "false");
                UpdateTimerSettings();

            };

            AutosetSwitch.Toggled += (sender, e) =>
            {
                if (AutosetSwitch.IsToggled)
                    LocalDBManager.Instance.SetDBSetting("timer_autoset", "true");
                else
                    LocalDBManager.Instance.SetDBSetting("timer_autoset", "false");
                UpdateTimerSettings();
                if (_isTimerTooltip)
                {
                    TooltipEffect.SetPosition(TimerMore, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(TimerMore, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(TimerMore, Color.White);
                    TooltipEffect.SetText(TimerMore, $"Add seconds");
                    TooltipEffect.SetHasTooltip(TimerMore, true);
                    TooltipEffect.SetHasShowTooltip(TimerMore, true);
                }
            };

            FullscreenSwitch.Toggled += (sender, e) =>
            {
                if (FullscreenSwitch.IsToggled)
                    LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");
                else
                    LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "false");
                UpdateTimerSettings();
            };

            Timer.Instance.OnTimerDone += () => { TimerStartButton.Text = "START"; };
            Timer.Instance.OnTimerStop += () => { TimerStartButton.Text = "START"; };

            var tapLinkGestureRecognizer = new TapGestureRecognizer();
            tapLinkGestureRecognizer.Tapped += (s, e) =>
            {
                Device.OpenUri(new Uri("http://drmuscleapp.com/news/between-sets/"));
            };

            LearnMoreLink.GestureRecognizers.Add(tapLinkGestureRecognizer);
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });
        }

        private void CancelNotification()
        {
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1251);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1351);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1451);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1551);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1651);
        }

        private void RefreshLocalized()
        {
            //MeButton.Text = "More";
            ChartsButton.Text = "Charts";
            HistoryButton.Text = AppResources.History;
            SubscriptionInfosButton.Text = "Subscription"; //AppResources.SubscriptionInfo;
            //SettingsButton.Text = AppResources.Settings;
            //EmailUsButton.Text = AppResources.EmailSupport;
            LogOutButton.Text = AppResources.LogOut;
            LblVibrate.Text = AppResources.VIBRATE;
            //LblSound.Text = AppResources.SOUND;
            LblAutoStart.Text = AppResources.AUTOSTART;
            LblAutomatchReps.Text = AppResources.AUTOMATCHREPS;
            TimerStartButton.Text = AppResources.START;
            LearnMoreLink.Text = AppResources.LearnMore;
            //ManageExercise.Text = AppResources.ManageExercises;
            LblAutomaticallyChangeTimer.Text = AppResources.AutomaticallyChangeTimerDurationToMatchRecommendedRepsAndOptimizeMuscleHypertrophy;
            VersionInfoLabel.Text = DependencyService.Get<IDrMuscleSubcription>().GetBuildVersion().Replace("Version", AppResources.Version).Replace("Build", AppResources.Build);
            VersionInfoLabel1.Text = VersionInfoLabel.Text;
            VersionInfoLabel2.Text = VersionInfoLabel.Text;
            LblFullScreen.Text = AppResources.FullscreenUppercase;
            //WebButton.Text = AppResources.WebApp;
            //OldHomeButton.Text = "Old Home";
            FAQButton.Text = "Help";
        }

        private List<ReviewsModel> GetReviews()
        {
            List<ReviewsModel> reviews = new List<ReviewsModel>();
            reviews.Add(new ReviewsModel()
            {
                Review = "For basic strength training this app out performs the many methods/apps I have tried in my 30+ years of body/strength training. What I like the most is that it take the brain work out of weights, reps, and sets (if you follow a structured workout). What I like even more is the exceptional customer engagement.",
                ReviewerName = "TijFamily916"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "Let me just say, I was thinking of being an online personal trainer but after using and seeing the power of this app, I sincerely can’t charge people the rates I had in mind when this app does it at a fraction of the cost. The man behind it, Dr. Juneau is the real deal too.",
                ReviewerName = "Rajib Ghosh"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "love seeing my progress on my 1 RM while varying my weight and rep count. Also feel like I am getting more results in a shorter time utilizing the rest pause method. Loving the workouts and the feedback from the app",
                ReviewerName = "Randall Duke"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "Maximizing the time in the gym takes preparation. This app eliminates that and does a better job then I did with hours of preparation. I've seen amazing gains with less work.",
                ReviewerName = "Raymond Backers"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "Great alternative to an actual human personal trainer if your schedule is always dynamic. The charts and graphs and many various options are outstanding.",
                ReviewerName = "Daniel Quick"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "Dr Carl has used science and experience to create an app that will continually push you to the limits. I've been using this app for about a month now, and am moving weight that I didn't think was possible in this short amount of time. I've been lifting for years, but this app would be just as affective for a beginner. One of the best things about it, is Dr Carl listens to the users and their feedback, and is constantly making improvements.",
                ReviewerName = "DeeBee78"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "This app is absolutely amazing. I have been in and out of the gym for a few years with some light progress every time and modest gains, however, the implementation of this app helped me gain 10 lbs and become significantly more defined in the first 6 weeks. Very easy to use, and the customer service is incredible. This app is really great for anyone from beginners to experts.",
                ReviewerName = "Potero2122"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "When I first trialed the app, I wasn’t sure I’d like it, but after having stuck with it for a couple of months, I’m sold. The AI is great and makes it very easy for me to know how many reps to do and how much weight to lift. No more guessing. He brings all the science of lifting to this app, and I’d been lifting regularly for two years. This really is something different than any other app out there.",
                ReviewerName = "MKJ&MKJ"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "This is a very good app to invest in. It's already a good design and has great workouts that will help you continually build muscle and break through plateaus, but they are constantly working to improve it based on customer feedback. The most important thing about this app is the customer service. Christelle and Carl are always available to assist you in anyway they can in a very timely manner, most of the time within an hour of submitting your question or issue. I would recommend this app to everyone serious about building muscle.",
                ReviewerName = "David Fechter"
            });
            reviews.Add(new ReviewsModel()
            {
                Review = "I have been using Dr. Muscle for two years now and this app gives me confidence and provides structure to my workouts. I love that the app adapts to you and is quite \"forgiving\" when you do fail while encouraging you to push harder each time. It has really demystified all the elements of training for hypertrophy so I can get straight to lifting after a hard day at work without having to think about everything! I look forward to the analysis of my \"performance\" after every exercise and love to see those green check marks indicating progress. I have recently subscribed to \"Eve\" the dietary equivalent to this app and while it's in its early stages of development I'm looking forward to similarly great things.",
                ReviewerName = "Remone Mundle"
            });

            return reviews;
        }

        


        private async void UpdateTimeCountWithoutLoader(int timeCount)
        {
            //SetUserTimeCount
            LocalDBManager.Instance.SetDBSetting("timer_count", timeCount.ToString());
            
        }
        private async void UpdateTimeCount(int timeCount)
        {
            //SetUserTimeCount
            LocalDBManager.Instance.SetDBSetting("timer_count", timeCount.ToString());
          
        }
        private async void UpdateMassUnit(string unit)
        {
          
        }

        private async void UpdateTimerSettings()
        {
           
        }


        public void ResignedField()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (TimerEntry != null)
                TimerEntry.Unfocus();
            });
        }

        public async void ShowGeneral()
        {
            //this.WidthRequest = 250;
            var rndm = new Random();
            var review = reviewList.ElementAt(rndm.Next(0, 9));
            LblReview.Text = review.Review;
            LblReviewerName.Text = review.ReviewerName;
            if (TimerEntry.IsFocused)
                TimerEntry.Unfocus();
            
            Device.BeginInvokeOnMainThread(async () =>
            {
                GeneralStack.IsVisible = true;
                TimerStack.IsVisible = false;
                BotStack.IsVisible = false;
                HomeStack.IsVisible = false;
                
            });
            try
            {
                
                if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("ProfilePic")?.Value))
                    ImgProfile.Source = LocalDBManager.Instance.GetDBSetting("ProfilePic")?.Value;
                else
                    ImgProfile.Source = "me_tab.png";
            }
            catch (Exception ex)
            {

            }
            try
            {
                LblDoneWorkout.Text = "";
                LblNmae.Text = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
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
                            var exerciseModel = workouts.HistoryExerciseModel;
                            if (exerciseModel != null)
                            {
                                var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();
                                var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                                LblDoneWorkout.Text = exerciseModel.TotalWorkoutCompleted <= 1 ? $"{exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutDone}" : $"{exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutsDone}";

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
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
        public void ShowHomeMenu()
        {
            if (TimerEntry.IsFocused)
                TimerEntry.Unfocus();
            GeneralStack.IsVisible = false;
            BotStack.IsVisible = false;
            HomeStack.IsVisible = true;
            TimerStack.IsVisible = false;
            HomeMainStack.IsVisible = true;
            SummaryMainStack.IsVisible = false;

            
        }
        public void ShowHomeSummaryMenu()
        {
            this.WidthRequest = 250;
            if (TimerEntry.IsFocused)
                TimerEntry.Unfocus();
            GeneralStack.IsVisible = false;
            BotStack.IsVisible = false;
            HomeStack.IsVisible = true;
            TimerStack.IsVisible = false;
            HomeMainStack.IsVisible = false;
            SummaryMainStack.IsVisible = true;


        }

        public void ShowAutoBotMenu()
        {
            this.WidthRequest = 250;
            if (TimerEntry.IsFocused)
                TimerEntry.Unfocus();
            GeneralStack.IsVisible = false;
            RestartSetupButton.IsVisible = true;
            SignInButton.IsVisible = true;
            RestartDemoButton.IsVisible = false;
            BoxDemoBorder.IsVisible = false;
            SkipDemoButton.IsVisible = false;
            BoxSetupBorder.IsVisible = true;
            HomeStack.IsVisible = false;
            BotStack.IsVisible = true;
            TimerStack.IsVisible = false;

            LanguageButton.IsVisible = true;
            BoxLanguageBorder.IsVisible = true;
            CancelButton.IsVisible = false;

            string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
            if (val == "Beta")
            {
                ModeLbl.Text = "Beta experience - load normal";
            }
            else
            {
                ModeLbl.Text = "Normal experience - load beta";
            }
        }

        public void ShowAutoBotReconfigureMenu()
        {
            this.WidthRequest = 250;
            if (TimerEntry.IsFocused)
                TimerEntry.Unfocus();
            GeneralStack.IsVisible = false;
            RestartSetupButton.IsVisible = false;
            SignInButton.IsVisible = false;
            RestartDemoButton.IsVisible = false;
            BoxDemoBorder.IsVisible = false;
            SkipDemoButton.IsVisible = false;
            BoxSetupBorder.IsVisible = false;
            HomeStack.IsVisible = false;
            BotStack.IsVisible = true;
            TimerStack.IsVisible = false;

            LanguageButton.IsVisible = false;
            BoxLanguageBorder.IsVisible = false;
            CancelButton.IsVisible = true;
        }

        public void ShowAutoBotDemoMenu()
        {
            this.WidthRequest = 250;
            if (TimerEntry.IsFocused)
                TimerEntry.Unfocus();
            GeneralStack.IsVisible = false;
            SkipDemoButton.IsVisible = true;
            SignInButton.IsVisible = false;
            RestartSetupButton.IsVisible = false;
            BoxSetupBorder.IsVisible = false;
            RestartDemoButton.IsVisible = true;
            BoxDemoBorder.IsVisible = true;
            BotStack.IsVisible = true;
            HomeStack.IsVisible = false;
            BotStack.VerticalOptions = LayoutOptions.FillAndExpand;
            TimerStack.IsVisible = false;
            LanguageButton.IsVisible = true;
            BoxLanguageBorder.IsVisible = true;
            CancelButton.IsVisible = false;
        }

        public void SetFeatuedTimer()
        {
            if (TimerEntry.IsFocused)
                TimerEntry.Unfocus();
            TimerEntry.Text = LocalDBManager.Instance.GetDBSetting("timer_remaining").Value;
            AutosetSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_autoset").Value);
        }
        public async void ShowTimer()
        {
            this.WidthRequest = 250;
            Device.BeginInvokeOnMainThread(async () =>
            {
                GeneralStack.IsVisible = false;
                BotStack.IsVisible = false;
                HomeStack.IsVisible = false;
                SetTimerSettings();
                TimerStack.IsVisible = true;
                if (CurrentLog.Instance.ShowTimerOptions)
                {
                    await Task.Delay(500);
                   
                    CurrentLog.Instance.ShowTimerOptions = false;
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        _isTimerTooltip = true;
                        TooltipEffect.SetPosition(StackAutoMatch, TooltipPosition.Bottom);
                    TooltipEffect.SetBackgroundColor(StackAutoMatch, AppThemeConstants.BlueColor);
                    TooltipEffect.SetTextColor(StackAutoMatch, Color.White);
                    TooltipEffect.SetText(StackAutoMatch, $"Toggle off to customize");
                    TooltipEffect.SetHasTooltip(StackAutoMatch, true);
                    TooltipEffect.SetHasShowTooltip(StackAutoMatch, true);
                    }
                    else
                    {
                       // UserDialogs.Instance.Alert("Toggle off to customize");
                    }
                }
                if (Timer.Instance.State == "RUNNING")
                {
                    TimerStartButton.Text = "STOP";
                }
            });
        }

        void SetTimerSettings()
        {

            if (LocalDBManager.Instance.GetDBSetting("timer_vibrate") == null)
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_sound") == null)
                LocalDBManager.Instance.SetDBSetting("timer_sound", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_reps_sound") == null)
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_autostart") == null)
                LocalDBManager.Instance.SetDBSetting("timer_autostart", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_autoset") == null)
                LocalDBManager.Instance.SetDBSetting("timer_autoset", "true");

            if (LocalDBManager.Instance.GetDBSetting("timer_fullscreen") == null)
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", "true");

            try
            {
                _isTimerSettingsUpdating = true;
                VibrateSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_vibrate").Value);
                SoundSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_sound").Value);
                Timer123Switch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_123_sound").Value);
                RepsSoundSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_reps_sound").Value);
                if (RepsSoundSwitch.IsToggled && RepsSoundSwitch.IsToggled)
                {
                    SoundSwitch.IsToggled = false;
                    LocalDBManager.Instance.SetDBSetting("timer_sound", "false");
                }
                AutostartSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_autostart").Value);
                AutosetSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_autoset").Value);
                FullscreenSwitch.IsToggled = Convert.ToBoolean(LocalDBManager.Instance.GetDBSetting("timer_fullscreen").Value);
                TimerEntry.Text = LocalDBManager.Instance.GetDBSetting("timer_count").Value;
                _isTimerSettingsUpdating = false;
            }
            catch (Exception ex)
            {
                _isTimerSettingsUpdating = false;
            }
        }

        void Handle_BuildVersionTapped(object sender, System.EventArgs e)
        {
            ActionSheetConfig config = new ActionSheetConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            };

            bool isProduction = LocalDBManager.Instance.GetDBSetting("Environment") == null || LocalDBManager.Instance.GetDBSetting("Environment").Value == "Production";

            config.Add(isProduction ? $"Production (active)": $"Production" , () =>
            {
                if (LocalDBManager.Instance.GetDBSetting("Environment") == null)
                {
                    SetProduction();
                    return;
                }
                if (LocalDBManager.Instance.GetDBSetting("Environment").Value != "Production")
                {
                    SetProduction();
                    LogOut();
                }
            });
            config.Add(isProduction ? "Staging" : "Staging (active)", () =>
            {
                if (LocalDBManager.Instance.GetDBSetting("Environment") == null)
                {
                    SetStaging();
                    LogOut();

                    return;
                }
                if (LocalDBManager.Instance.GetDBSetting("Environment").Value != "Staging")
                {
                    SetStaging();
                    LogOut();
                }
            });
            config.Add("Crash", () =>
            {
                
                var kill = DependencyService.Get<IKillAppService>();
                kill.ExitApp();
                
            });
            config.SetCancel(AppResources.Cancel, null);
            config.SetTitle(AppResources.ChooseEnvironment);
            //config.Options = new List<Acr.UserDialogs.ActionSheetOption>() { "Production API", "Staging (test) API" };
            UserDialogs.Instance.ActionSheet(config);
        }

        private void SetProduction()
        {
            LocalDBManager.Instance.SetDBSetting("Environment", "Production");
            DrMuscleRestClient.Instance.ResetBaseUrl();
        }
        private void SetStaging()
        {
            LocalDBManager.Instance.SetDBSetting("Environment", "Staging");
            DrMuscleRestClient.Instance.ResetBaseUrl();
        }
        private async void LogOut()
        {
            HideWithoutAnimations();
            RemoveToken();
            CancelNotification();
            LocalDBManager.Instance.Reset();
            CurrentLog.Instance.Reset();
            App.IsV1User = false;
            App.IsV1UserTrial = false;
            App.IsCongratulated = false;
            App.IsSupersetPopup = false;
            App.IsFreePlan = false;
            ((App)Application.Current).UserWorkoutContexts.workouts = new GetUserWorkoutLogAverageResponse();
            ((App)Application.Current).UserWorkoutContexts.SaveContexts();
            ((App)Application.Current).WorkoutHistoryContextList.Histories = new List<HistoryModel>();
            ((App)Application.Current).WorkoutHistoryContextList.SaveContexts();
            ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
            ((App)Application.Current).WorkoutLogContext.SaveContexts();
            await PagesFactory.PopToRootAsync();
            ((App)Application.Current).displayCreateNewAccount = true;
            await PagesFactory.PushAsync<WelcomePage>(true);

        }

        private async void RemoveToken()
        {
            try
            {
                //string email = LocalDBManager.Instance.GetDBSetting("email").Value;
                //await Task.Delay(1000);
                ////Live
                //SendBirdClient.Init("91658003-270F-446B-BD61-0043FAA8D641");
                ////Test
                ////SendBirdClient.Init("05F82C36-1159-4179-8C49-5910C7F51D7D");
                //if (!email.ToLower().Equals("etiennejuneau@gmail.com"))
                //    SendBirdClient.Connect(email, Connect_Handler);
                //else
                //    SendBirdClient.Connect(email, "8e5bcba7c5b339da28c39f7b315d7b6d3e3a80c0", Connect_Handler);
               
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            try
            {

            
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
              GeneralStack.Margin = new Thickness(0, 0, 0, 0);
                BotStack.Margin = new Thickness(0, App.StatusBarHeight, 0, 0);
                HomeStack.Margin = new Thickness(0, App.StatusBarHeight, 0, 0);
                TimerStack.Margin = new Thickness(0, App.StatusBarHeight, 0, 0);
                PancakeContainer.Margin = new Thickness(0);
                PancakeContainer.Padding = new Thickness(0, App.StatusBarHeight + 10, 0, 0);
            }
            else
                {
                    if (TimerStack != null)
                    TimerStack.Margin = new Thickness(0, 20, 0, 0);
                }
                    
            }
            catch (Exception ex)
            {

            }
        }

       

        void TapMoreReviews_Tapped(System.Object sender, System.EventArgs e)
        {
            HideWithoutAnimations();
            Browser.OpenAsync("https://dr-muscle.com/reviews/", BrowserLaunchMode.SystemPreferred);
        }

        void Handle_ModeChange(System.Object sender, System.EventArgs e)
        {
            string val = LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value;
            if (val == "Beta")
            {
                ModeLbl.Text = "";
                LocalDBManager.Instance.SetDBSetting("BetaVersion", "Normal");
            }
            else
            {
                ModeLbl.Text = "";
                LocalDBManager.Instance.SetDBSetting("BetaVersion", "Beta");
            }
            HideWithoutAnimations();


        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            TimerEntry.Unfocus();
        }

        async void TimerEntry_Focused(System.Object sender, Xamarin.Forms.FocusEventArgs e)
        {
            await Task.Delay(300);
            ShowTimer();
        }
    }
}
