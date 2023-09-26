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
using Xamarin.Forms.PancakeView;
using DrMuscle.Controls;
using DrMuscle.Entity;
using Plugin.GoogleClient;
using DrMuscle.Dependencies;
using Plugin.GoogleClient.Shared;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;
using Xamarin.Essentials;
using Plugin.Connectivity;
using DrMuscle.Screens.Demo;
using DrMuscle.Services;
using Newtonsoft.Json;

namespace DrMuscle.Screens.User.OnBoarding
{
    public partial class BoardingBotPage : DrMusclePage
    {
        public ObservableCollection<BotModel> BotList = new ObservableCollection<BotModel>();
        public LearnMore learnMore = new LearnMore();
        bool ManMoreMuscle = false;
        bool ManLessFat = false;
        bool ManBetterHealth = false;
        bool ManStorngerSexDrive = false;
        private readonly IGoogleClientManager _googleClientManager;
        IFacebookManager _manager;
        private IAppleSignInService appleSignInService;
        bool FemaleMoreEnergy = false;
        bool FemaleToned = false;
        bool IsHumanSupport = false;
        bool ShouldAnimate = false;
        bool IsBodyweightPopup = false;
        bool IsIncludeCardio = false;
        string focusText = "", mainGoal = "";
        private IFirebase _firebase;
        Picker AgePicker;
        Picker BodyweightPicker;
        public static bool IsMovedToLogin = false;

        bool IsEquipment = false;
        bool IsPully = false;
        bool isDumbbells = false;
        bool IsPlates = false;
        bool IsChinupBar = false;
        bool isProcessing = false;
        string bodypartName = "";
        CustomImageButton bodypart1, bodypart2, bodypart3, bodypartBalanced;
        public BoardingBotPage()
        {
            InitializeComponent();

            lstChats.ItemsSource = BotList;
            NavigationPage.SetHasBackButton(this, false);
            Title = AppResources.DrMuslce;
            _firebase = DependencyService.Get<IFirebase>();
            this.ToolbarItems.Clear();
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralBotAction, ToolbarItemOrder.Primary, 0);
            this.ToolbarItems.Add(generalToolbarItem);
            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "DrMuscleLogo.png");
            MessagingCenter.Send(this, "BackgroundImageUpdated");
            var tapLinkTermsOfUseGestureRecognizer = new TapGestureRecognizer();
            tapLinkTermsOfUseGestureRecognizer.Tapped += (s, e) =>
            {
                //Device.OpenUri(new Uri("http://drmuscleapp.com/news/terms/"));
                Browser.OpenAsync("http://drmuscleapp.com/news/terms/", BrowserLaunchMode.SystemPreferred);
            };
            TermsOfUse.GestureRecognizers.Add(tapLinkTermsOfUseGestureRecognizer);

            var tapLinkPrivacyPolicyGestureRecognizer = new TapGestureRecognizer();
            tapLinkPrivacyPolicyGestureRecognizer.Tapped += (s, e) =>
            {
                //Device.OpenUri(new Uri("http://drmuscleapp.com/news/privacy/"));
                Browser.OpenAsync("http://drmuscleapp.com/news/privacy/", BrowserLaunchMode.SystemPreferred);
            };

            MessagingCenter.Subscribe<Message.BodyweightMessage>(this, "BodyweightMessage", (obj) =>
            {
                IsBodyweightPopup = false;
                BodyWeightMassUnitMessage(obj.BodyWeight);
            });
            PrivacyPolicy.GestureRecognizers.Add(tapLinkPrivacyPolicyGestureRecognizer);
            _googleClientManager = CrossGoogleClient.Current;
            _manager = DependencyService.Get<IFacebookManager>();
            LoginWithFBButton.Clicked += LoginWithFBButton_Clicked;
            LoginWithGoogleButton.Clicked += LoginWithGoogleAsync;
            LoginWithEmailButton.Clicked += ConnectWithEmail;
            
            LoginButton.HeightRequest = 170;
            BtnAppleSignIn.IsVisible = false;
           // BtnAppleSignIn2.IsVisible = false;

            appleSignInService = DependencyService.Get<IAppleSignInService>();
            if (appleSignInService != null)
            {
                if (appleSignInService.IsAvailable)
                {
                    LoginButton.HeightRequest = 220;
                    BtnAppleSignIn.IsVisible = true;
                   // BtnAppleSignIn2.IsVisible = true;
                    BtnAppleSignIn.Clicked += LoginWithAppleAsync;
                }
            }
        }

        private async void BodyWeightMassUnitMessage(string bodyWeight)
        {
            try
            {

                LocalDBManager.Instance.SetDBSetting("BodyWeight", new MultiUnityWeight(Convert.ToDecimal(bodyWeight, CultureInfo.InvariantCulture), LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().ReplaceWithDot());
                await AddAnswer(bodyWeight);


                //await AddQuestion("Are you a man or a woman?");
                //await Task.Delay(300);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                //SetupGender();

                BotList.Add(new BotModel()
                {
                    Question = "Sign in to:\n- Save your program\n- Work out from any device\n- Never lose your history",
                    Type = BotType.Ques
                });
                await Task.Delay(1500);
                Device.BeginInvokeOnMainThread(() =>
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                });
                //SignupCode here:

                SetMenu();

            }
            catch (Exception ex)
            {

            }
        }
        private async void BodyweightPicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                int age = Convert.ToInt32(BodyweightPicker.SelectedItem, CultureInfo.InvariantCulture);
                LocalDBManager.Instance.SetDBSetting("BodyWeight", Convert.ToString(age));
                await AddAnswer(Convert.ToString(age));

                if (LocalDBManager.Instance.GetDBSetting("ExLevel").Value == "Exp")
                {
                    LocalDBManager.Instance.SetDBSetting("workout_place", "gym");
                    LocalDBManager.Instance.SetDBSetting("experience", "more3years");
                    NoAdvancedClicked(sender, e);
                    return;
                }
                LocalDBManager.Instance.SetDBSetting("experience", "less1year");
                // await AddQuestion("Are you training at home with no equipment?");
                Device.BeginInvokeOnMainThread(() =>
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                });
                BeginnerSetup();

            }
            catch (Exception ex)
            {

            }
        }

        private async void AgePicker_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private async void AgePicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                int age = Convert.ToInt32(AgePicker.SelectedItem, CultureInfo.InvariantCulture);
                LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(age));
                //        await AddAnswer(Convert.ToString(age));

                await AddAnswer($"{age}");
                if (age > 50)
                    learnMore.AgeDesc = $"Recovery is slower at {age}. So, I added easy days to your program.";
                else if (age > 30)
                    learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I'm updating your program to make sure you train each muscle max 2x a week.";
                else
                    learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";
                //await AddQuestion(learnMore.AgeDesc);
                if (LocalDBManager.Instance.GetDBSetting("experience").Value == "beginner")
                    AddCardio();// SetupQuickMode();
                else
                    workoutPlace();

            }
            catch (Exception ex)
            {

            }
        }
        public override void OnBeforeShow()
        {
            base.OnBeforeShow();
            try
            {
                App.IsNUX = false;
                CurrentLog.Instance.IsMovingOnBording = false;
                this.ToolbarItems.Clear();
                var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralBotAction, ToolbarItemOrder.Primary, 0);
                this.ToolbarItems.Add(generalToolbarItem);
                IsMovedToLogin = false;
                List<string> age = new List<string>();
                List<string> bodyweight = new List<string>();
                for (int i = 10; i < 125; i++)
                {
                    age.Add($"{i}");
                }


                if (AgePicker != null)
                    AgePicker.SelectedIndexChanged -= AgePicker_SelectedIndexChanged;

                AgePicker = new Picker()
                {

                    Title = "How old are you?"
                };
                AgePicker.ItemsSource = age;
                AgePicker.SelectedItem = "35";
                AgePicker.Unfocused += AgePicker_Unfocused;
                AgePicker.SelectedIndexChanged += AgePicker_SelectedIndexChanged;

                if (BodyweightPicker != null)
                    BodyweightPicker.Unfocused -= BodyweightPicker_Unfocused;
                BodyweightPicker = new Picker()
                {
                    Title = "what is your body weight?"
                };
                BodyweightPicker.ItemsSource = bodyweight;
                BodyweightPicker.SelectedItem = "160";
                BodyweightPicker.Unfocused += BodyweightPicker_Unfocused;
                MainGrid.Children.Insert(0, AgePicker);
                MainGrid.Children.Insert(0, BodyweightPicker);

                if (!App.IsDemoProgress)
                    StartSetup();
                else
                {
                    SetUpRestOnboarding();
                }

            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            try
            {

                DependencyService.Get<IFirebase>().SetScreenName("onboarding_account");
                if (Device.RuntimePlatform.Equals(Device.Android)) 
                {
                    DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1351);
                    var dt = DateTime.Now.AddMinutes(5);
                    var timeSpan = new TimeSpan(0, dt.Hour, dt.Minute, 0);// DateTime.Now.AddMinutes(2) - DateTime.Now;////
                    DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Dr. Muscle", "Oops! You're 12 seconds away from custom, smart workouts", timeSpan, 1351, NotificationInterval.Week);
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected override bool OnBackButtonPressed()
        {

            if (IsBodyweightPopup)
                return true;
            ((App)Application.Current).displayCreateNewAccount = true;
            PagesFactory.PushAsync<WelcomePage>();
            return true;

        }

        private async Task ClearOptions()
        {
            var count = stackOptions.Children.Count;
            for (var i = 0; i < count; i++)
            {
                stackOptions.Children.RemoveAt(0);
            }
            BottomViewHeight.Height = 65;
        }

        void Handle_ItemAppearing(object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {

        }

        async Task StartSetup()
        {
            try
            {
                StackSignupMenu.IsVisible = false;
                BotList.Clear();
                await ClearOptions();

                IsPlates = false;
                isDumbbells = false;
                IsPully = false;
                IsEquipment = false;
                IsChinupBar = false;
                bodypartName = "";
                var welcomeNote = "";
                var time = DateTime.Now.Hour;
                if (time < 12)
                    welcomeNote = AppResources.GoodMorning;
                else if (time < 18)
                    welcomeNote = AppResources.GoodAfternoon;
                else
                    welcomeNote = AppResources.GoodEvening;
                ///Full demo after account creation-------Start
                /*
                BotList.Add(new BotModel()
                {
                    Question = "Welcome to Dr. Muscle! I'm Dr. Carl Juneau and you're 30 sec away from smart, custom workouts. ",
                    Type = BotType.Ques
                });

                await Task.Delay(2500);

                BotList.Add(new BotModel()
                {
                    Question = "Why trust me? I've been a coach for 17 years and a trainer for the Canadian Forces.",
                    Type = BotType.Ques
                });
                await Task.Delay(2500);
                BotList.Add(new BotModel()
                {
                    Question = $"",
                    Answer = "",
                    Type = BotType.Photo
                });

                await ClearOptions();


               
                await Task.Delay(2500);
               
                SetUpRestOnboarding();
                */
                ///Full demo after account creation-------End
                BotList.Add(new BotModel()
                {
                    Question = "Welcome to Dr. Muscle! I'm Dr. Carl Juneau and I'll help you get in shape fast with smart, custom workouts.",
                    Type = BotType.Ques
                });

                ////
                await Task.Delay(2500);

                BotList.Add(new BotModel()
                {
                    Question = "Why trust me? I've been a coach for 17 years and a trainer for the Canadian Forces.",
                    Type = BotType.Ques
                });
                await Task.Delay(2500);
                BotList.Add(new BotModel()
                {
                    Question = $"",
                    Answer = "",
                    Type = BotType.Photo
                });

                await ClearOptions();

                await Task.Delay(2500);
                if (IsMovedToLogin)
                    return;
                BotList.Add(new BotModel()
                {
                    Question = "Are you...",
                    Type = BotType.Ques
                });
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                await Task.Delay(1000);

                SetNotifications();

                var btn = await AddOptions("New to training", async (ss, ee) =>
                {
                    SetNotifications();
                    ShouldAnimate = false;
                    _firebase.LogEvent("start_onboarding", "new_to_training");
                    _firebase.LogEvent("new_to_training", "");
                    LocalDBManager.Instance.SetDBSetting("CustomExperience", "new to training");
                    await AddAnswer("New to training");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);

                    await AddQuestion("Congrats on getting started! You'll love seeing your performance improve automatically, simple and effective exercises in your program, and more.");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await ClearOptions();
                    LocalDBManager.Instance.SetDBSetting("ExLevel", "New");
                    LocalDBManager.Instance.SetDBSetting("NewLevel", "All");

                    await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);


                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });
                var btn1 = await AddOptions("Returning after a break", async (ss, ee) =>
                {
                    SetNotifications();
                    ShouldAnimate = false;
                    LocalDBManager.Instance.SetDBSetting("CustomExperience", "returning from a break");
                    _firebase.LogEvent("start_onboarding", "returning_after_a_break");
                    _firebase.LogEvent("returning_after_a_break", "");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddAnswer("Returning after a break");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddQuestion("Nice—you'll ramp up fast! You'll love seeing your performance improve automatically, simple and effective exercises in your program, and more.");
                    LocalDBManager.Instance.SetDBSetting("ExLevel", "Return");
                    ClearOptions();
                    await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);
                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn1.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });

                var btn2 = await AddOptions("Active, experienced lifter", async (ss, ee) =>
                {
                    SetNotifications();
                    ShouldAnimate = false;
                    LocalDBManager.Instance.SetDBSetting("CustomExperience", "an active, experienced lifter");
                    _firebase.LogEvent("start_onboarding", "Active_experienced_lifter");
                    _firebase.LogEvent("Active_experienced_lifter", "");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddAnswer("Active, experienced lifter");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddQuestion("Nice! You'll love the advanced features of your program like rest-pause and back-off sets, daily undulating periodization, deloads, overtraining protection, and more.");
                    ClearOptions();
                    LocalDBManager.Instance.SetDBSetting("ExLevel", "Exp");
                    await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);
                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn2.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });
                //SetupMassUnit();
            }
            catch (Exception ex)
            {

            }
        }

        private async void SetUpRestOnboarding()
        {
            ClearOptions();
            //BotList.Add(new BotModel()
            //{
            //    Question = "Congratulations—you smashed the demo!",
            //    Type = BotType.Ques
            //});
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            //await Task.Delay(1000);
            BotList.Add(new BotModel()
            {
                Question = "Congratulations—you smashed the demo!",
                Type = BotType.Ques
            });
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            await Task.Delay(1000);
            BotList.Add(new BotModel()
            {
                Question = "This app is new. Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we release new features every month and we'll treat your feedback like gold.",
                Type = BotType.Ques
            });
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            await Task.Delay(2500);

            GotItAfterImage(new DrMuscleButton(), EventArgs.Empty);

        }

        async void GotItAfterExperienceLevel(object sender, EventArgs e)
        {

            await AddAnswer(AppResources.GotIt);
            ClearOptions();
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //SetupGender();
            SetupMassUnit();
            //GotItAfterImage(sender, e);
            
        }

        private void SetNotifications()
        {
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                CancelNotification();
                var dt = DateTime.Now.AddMinutes(5);
                var timeSpan = new TimeSpan(DateTime.Now.AddMinutes(5).Day-DateTime.Now.Day, dt.Hour, dt.Minute, 0);// DateTime.Now.AddMinutes(2) - DateTime.Now;////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Dr. Muscle", "Oops! You're 12 seconds away from custom, smart workouts", timeSpan, 1351, NotificationInterval.Week);
            }
        }

        private void SetTrialUserNotifications()
        {
            try
            {

            CancelNotification();
            var fName = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            var dt = DateTime.Now.AddDays(2);
            var timeSpan = new TimeSpan(2, dt.Hour, dt.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(10).Day - DateTime.Now.Day, dt.Hour, dt.Minute, 0);////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"{fName}, you can do this!", timeSpan, 1451);

            var dt1 = DateTime.Now.AddDays(4);
            var timeSpan1 =  new TimeSpan(4, dt1.Hour, dt1.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(15).Day - DateTime.Now.Day, dt1.Hour, dt1.Minute, 0);////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", Device.RuntimePlatform.Equals(Device.Android) ? $"New users like you improve 34% in 30 days" : $"New users like you improve 34%% in 30 days", timeSpan1, 1551);

            var dt2 = DateTime.Now.AddDays(10);
            var timeSpan2 =  new TimeSpan(10, dt2.Hour, dt2.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(20).Day - DateTime.Now.Day, dt2.Hour, dt2.Minute, 0);////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"You're 12 seconds away from custom, smart workouts", timeSpan2, 1651);

            }
            catch (Exception ex)
            {

            }
        }

        private void CancelNotification()
        {
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1051);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1151);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1251);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1351);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1451);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1551);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1651);

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

        async void GotItAfterImage(object sender, EventArgs e)
        {
            //await AddAnswer("Hi Carl");
            await AddQuestion("Are you a man or a woman?");
            await Task.Delay(300);
            SetupGender();

        }
        void SetDefaultButtonStyle(Button btn)
        {
            btn.BackgroundColor = Color.Transparent;
            btn.BorderWidth = 0;
            btn.CornerRadius = 6;
            btn.Margin = new Thickness(25, 5, 25, 5);
            btn.FontAttributes = FontAttributes.Bold;
            btn.BorderColor = Color.Transparent;
            btn.TextColor = Color.White;
            btn.HeightRequest = 50;

        }

        void SetEmphasisButtonStyle(Button btn)
        {
            btn.TextColor = Color.White;
            btn.BackgroundColor = Color.Transparent;
            btn.Margin = new Thickness(25, 5, 25, 5);
            btn.HeightRequest = 50;
            btn.BorderWidth = 6;
            btn.CornerRadius = 5;
            btn.FontAttributes = FontAttributes.Bold;
        }

        async void YesButton_Clicked(object sender, EventArgs e)
        {

            await AddAnswer(AppResources.Yes);

            await AddQuestion(AppResources.DoYouUseLbsOrKgs, false);

            //Yes-No Button
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.Man, ManButton_Clicked);
            await AddOptions(AppResources.Woman, WomanButton_Clicked);


        }

        async void NoButton_Clicked(object sender, EventArgs e)
        {
            //Move back
            ((App)Application.Current).displayCreateNewAccount = true;
            await PagesFactory.PushAsync<WelcomePage>();
        }

        async void ManButton_Clicked(object sender, EventArgs e)
        {

            BotList.Add(new BotModel()
            {
                Answer = AppResources.Man,
                Type = BotType.Ans
            });

            await ClearOptions();


            LocalDBManager.Instance.SetDBSetting("gender", "Man");
            await Task.Delay(300);
            //await AddQuestion("Men often want (tap all that apply):");
            //await Task.Delay(300);

            //await AddCheckbox("More muscle", Man_MoreMuscle_Clicked);
            //await AddCheckbox("Less fat", Man_LessFat_Clicked);
            //await AddCheckbox("Better health", Man_BetterHealth_Clicked);
            //await AddCheckbox("Stronger sex drive", Man_StorngerSexDrive_Clicked);

            //await AddOptions("Continue", ManTakeActionBasedOnInput);
            SetupMainGoal();
            return;

        }

        private async void SetupMainGoal()
        {

            //await Task.Delay(300);

            

            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
            if (IsWoman)
            {

                await AddQuestion("Thanks—would you also like:");

                await AddCheckbox("Less fat", Man_LessFat_Clicked);
                await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
                await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

                await AddOptions("Continue", WomanTakeActionBasedOnInput);
            }
            else
            {
                await AddQuestion("Thanks—would you also like:");
                await Task.Delay(300);

                await AddCheckbox("More muscle", Man_MoreMuscle_Clicked);
                await AddCheckbox("Less fat", Man_LessFat_Clicked);
                await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                await AddCheckbox("Stronger sex drive", Man_StorngerSexDrive_Clicked);

                await AddOptions("Continue", ManTakeActionBasedOnInput);
            }

        }
        private async void GetMainGoalAction(PromptResult response)
        {
            try
            {

                mainGoal = null;

                if (string.IsNullOrEmpty(response.Text))
                {
                    SetupMainGoal();
                    return;
                }
                else
                {
                    mainGoal = response.Text.ToLower();
                    await AddAnswer(response.Text);
                    LocalDBManager.Instance.SetDBSetting("PopupMainGoal", mainGoal);
                }
                //GotItAfterImage(new Button(), EventArgs.Empty);
                var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
                if (IsWoman)
                {

                    await AddQuestion("Thanks—would you also like:");

                    await AddCheckbox("Less fat", Man_LessFat_Clicked);
                    await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                    await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
                    await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

                    await AddOptions("Continue", WomanTakeActionBasedOnInput);
                }
                else
                {
                    await AddQuestion("Thanks—would you also like:");
                    await Task.Delay(300);

                    await AddCheckbox("More muscle", Man_MoreMuscle_Clicked);
                    await AddCheckbox("Less fat", Man_LessFat_Clicked);
                    await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                    await AddCheckbox("Stronger sex drive", Man_StorngerSexDrive_Clicked);

                    await AddOptions("Continue", ManTakeActionBasedOnInput);
                }

            }
            catch (Exception ex)
            {

            }

        }

        void Man_MoreMuscle_Clicked(object sender, EventArgs e)
        {
            ManMoreMuscle = !ManMoreMuscle;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManMoreMuscle ? "done.png" : "Undone.png";
        }

        void WoMan_MoreEnergy_Clicked(object sender, EventArgs e)
        {
            FemaleMoreEnergy = !FemaleMoreEnergy;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = FemaleMoreEnergy ? "done.png" : "Undone.png";
        }

        void WoMan_FemaleToned_Clicked(object sender, EventArgs e)
        {
            FemaleToned = !FemaleToned;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = FemaleToned ? "done.png" : "Undone.png";
        }

        void Man_LessFat_Clicked(object sender, EventArgs e)
        {
            ManLessFat = !ManLessFat;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManLessFat ? "done.png" : "Undone.png";
        }

        void Man_BetterHealth_Clicked(object sender, EventArgs e)
        {
            ManBetterHealth = !ManBetterHealth;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManBetterHealth ? "done.png" : "Undone.png";
        }

        void Man_StorngerSexDrive_Clicked(object sender, EventArgs e)
        {
            ManStorngerSexDrive = !ManStorngerSexDrive;
            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
            img.Source = ManStorngerSexDrive ? "done.png" : "Undone.png";
        }
        async void ManTakeActionBasedOnInput(object sender, EventArgs e)
        {
            try
            {
                var count = 0;
                count += ManMoreMuscle ? 1 : 0;
                count += ManLessFat ? 1 : 0;
                count += ManBetterHealth ? 1 : 0;
                count += ManStorngerSexDrive ? 1 : 0;
                var responseText = "";
                if (ManMoreMuscle)
                    responseText = "More muscle";
                if (ManLessFat)
                    responseText += responseText == "" ? "Less fat" : "\nLess fat";
                if (ManBetterHealth)
                    responseText += responseText == "" ? "Better health" : "\nBetter health";
                if (ManStorngerSexDrive)
                    responseText += responseText == "" ? "Stronger sex drive" : "\nStronger sex drive";
                if (responseText != "")
                    await AddAnswer(responseText);
                focusText = responseText;
                _firebase.LogEvent("chose_goals", focusText);
                if (ManMoreMuscle && ManLessFat)//&& count > 2
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
                    await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. I'm adding them to your program. High reps burn more fat.");


                }
                else if (ManMoreMuscle)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
                    await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. Adding them to your program...");

                }
                else if (ManLessFat)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");

                    await AddQuestion("OK. High reps burn more fat. I'm setting yours at 12-20.");

                }
                else if (ManBetterHealth || ManStorngerSexDrive)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");

                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
                    await AddQuestion("Got it.");

                }
                else
                    return;
                if (ManLessFat && ManMoreMuscle)
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");

                //if (ManLessFat)
                //    FatLossOption();
                //else
                    AskForHumanSupport();

            }
            catch (Exception ex)
            {

            }

        }

        async void TakeBodyWeight()
        {

            try
            {
                //BotList.Add(new BotModel()
                //{ Type = BotType.Empty });
                await AddQuestion("What is your body weight?");
                var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";

                if (IsWoman)
                {

                    BodyweightPicker.SelectedItem = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "140" : "65";
                }
                else
                    BodyweightPicker.SelectedItem = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "180" : "80";
                BodyweightPicker.IsVisible = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                });
                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        BodyweightPicker.Focus();
                    });
                }
                catch (Exception ex)
                {

                }

            }
            catch (Exception ex)
            {

            }
            //PromptConfig firsttimeExercisePopup = new PromptConfig()
            //{
            //    InputType = InputType.Number,
            //    IsCancellable = false,
            //    Title = "What is your body weight?",
            //    Message = $"",
            //    Placeholder = "Enter your bodyweight here",
            //    OkText = AppResources.Ok,
            //    //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogFirstTimeExercise),
            //    OnAction = async (ageResponse) =>
            //    {
            //        if (string.IsNullOrWhiteSpace(ageResponse.Value) || Convert.ToDecimal(ageResponse.Value, CultureInfo.InvariantCulture) < 1)
            //        {
            //            BotList.Remove(BotList.Last());
            //            TakeBodyWeight();
            //            return;
            //        }
            //        BotList.Remove(BotList.Last());
            //        int age = Convert.ToInt32(ageResponse.Value, CultureInfo.InvariantCulture);
            //        LocalDBManager.Instance.SetDBSetting("BodyWeight", Convert.ToString(age));
            //        await AddAnswer(Convert.ToString(age));



            //        SetupMassUnit();
            //    }
            //};
            //firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            //UserDialogs.Instance.Prompt(firsttimeExercisePopup);

        }

        async void WomanTakeActionBasedOnInput(object sender, EventArgs e)
        {
            try
            {
                var count = 0;
                count += FemaleMoreEnergy ? 1 : 0;
                count += ManLessFat ? 1 : 0;
                count += ManBetterHealth ? 1 : 0;
                count += FemaleToned ? 1 : 0;

                var responseText = "";
                if (ManLessFat)
                    responseText = "Less fat";
                if (ManBetterHealth)
                    responseText += responseText == "" ? "Better health" : "\nBetter health";
                if (FemaleMoreEnergy)
                    responseText += responseText == "" ? "More energy" : "\nMore energy";
                if (FemaleToned)
                    responseText += responseText == "" ? "Toned muscles" : "\nToned muscles";
                if (responseText != "")
                    await AddAnswer(responseText);
                focusText = responseText;
                _firebase.LogEvent("chose_goals", focusText);
                if (FemaleToned && ManLessFat) //&& count > 2
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");

                    await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. I'm adding them to your program. High reps burn more fat.");

                }
                else if (FemaleToned)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
                    await AddQuestion("Got it. You can tone up 59% faster with rest-pause sets. Adding them to your program...");

                    //await AddQuestion("Got it. You can tone up 59% faster with rest-pause sets. I'm adding them to your program. For that loss, your diet is important. Would you also like help with that?");

                }
                else if (ManLessFat)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");
                    await AddQuestion("OK. High reps burn more fat. I'm setting yours at 12-20.");

                }
                else if (ManBetterHealth || FemaleMoreEnergy)
                {
                    LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
                    await AddQuestion("Got it.");

                }
                else
                    return;
                if (ManLessFat && FemaleToned)
                    LocalDBManager.Instance.SetDBSetting("Demoreprange", "BuildMuscleBurnFat");
                //if (ManLessFat)
                //    FatLossOption();
                //else
                    AskForHumanSupport();

            }
            catch (Exception ex)
            {

            }
        }

        async void FatLossOption()
        {
            try
            {
                await ClearOptions();
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                await AddOptions("Yes", async (o, ee) =>
                {
                    await AddAnswer("Yes");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);

                //await AddQuestion("According to the International Society of Sports Nutrition, \"A wide range of dietary approaches can be similarly effective for improving body composition.\" In other words, you don’t need to follow a specific diet (e.g. low fat or low carb). Instead, the key is finding the one that works best for you, and that you can stick to long-term.");
                await AddQuestion("Great! Look for an email from my assistant Victoria Kingsley within one business day.");
                    await AddOptions(AppResources.GotIt, async (oo, eee) =>
                    {
                        await AddAnswer(AppResources.GotIt);
                        if (Device.RuntimePlatform.Equals(Device.Android))
                            await Task.Delay(300);
                        IsHumanSupport = true;
                        SetupEpeBegginer();
                    });
                });

                await AddOptions("No", async (o, ee) =>
                {
                    await AddAnswer("No");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    if (ManLessFat)
                        SetupEpeBegginer();
                    else
                        AskForHumanSupport();
                });

            }
            catch (Exception ex)
            {

            }
        }
        async void AskForHumanSupport()
        {
            //try
            //{
            //    await AddQuestion("Would you like a human to get in touch about your goals?");
            //    await AddOptions("Yes", async (o, ee) =>
            //    {
            //        IsHumanSupport = true;
            //        await AddAnswer("Yes");
            //        ClearOptions();
            //        if (Device.RuntimePlatform.Equals(Device.Android))
            //            await Task.Delay(300);

            //        await AddQuestion("Great! Look for an email from my assistant Victoria Kingsley within one business day.");

            //        await ClearOptions();

            //    SetupEpeBegginer();
            //    });

            //    await AddOptions("No", async (o, ee) =>
            //    {
            //        IsHumanSupport = false;
            //        await AddAnswer("No");
            //        ClearOptions();
            //        if (Device.RuntimePlatform.Equals(Device.Android))
            //            await Task.Delay(300);
            //        await AddQuestion(AppResources.GotIt);

            //        await ClearOptions();

            //    SetupEpeBegginer();


            //    });

            //}
            //catch (Exception ex)
            //{

            //}
            IsHumanSupport = false;
            SetupEpeBegginer();
        }

        async void SetupEpeBegginer()
        {
            if (LocalDBManager.Instance.GetDBSetting("ExLevel").Value == "Exp")
            {
                LocalDBManager.Instance.SetDBSetting("workout_place", "gym");
                LocalDBManager.Instance.SetDBSetting("experience", "more3years");
                NoAdvancedClicked(new DrMuscleButton(), EventArgs.Empty);
                return;
            }
            LocalDBManager.Instance.SetDBSetting("experience", "less1year");
            // await AddQuestion("Are you training at home with no equipment?");
            Device.BeginInvokeOnMainThread(() =>
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            });
            BeginnerSetup();
        }
        async void MenNext_Clicked(object sender, EventArgs e)
        {

        }

        async void WomanButton_Clicked(object sender, EventArgs e)
        {
            BotList.Add(new BotModel()
            {
                Answer = AppResources.Woman,
                Type = BotType.Ans
            });
            await Task.Delay(300);
            await ClearOptions();
            LocalDBManager.Instance.SetDBSetting("gender", "Woman");

            //await AddQuestion("Woman often want (tap all that apply):");

            //await AddCheckbox("Less fat", Man_LessFat_Clicked);
            //await AddCheckbox("Better health", Man_BetterHealth_Clicked);
            //await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
            //await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

            //await AddOptions("Continue", WomanTakeActionBasedOnInput);
            SetupMainGoal();

        }

        async void WomanThinClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.ThinWomenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitAndStrongWhileMaintaingLeanPhysiqueAddSizeToLegsBootyDenseLookingMuscleOverall, false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.Yes, WomanYesSkinnyClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, WomanOtherGoalClicked);
        }

        async void WomanYesSkinnyClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.GotItAreYouABeginnerWithNoEquipment);

            BeginnerSetup();
        }

        async void WomanOtherGoalClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.PleaseChooseAGoal);
            await AddQuestion(AppResources.DontWorryLiftingWightsWontMakeyouBulky);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.FocusOnToningUp, WomanFocusMuscleClicked);
            await AddOptions(AppResources.ToneUpAndSlimDown, WomanFocusBothClicked);
            await AddOptions(AppResources.FocusOnSlimmingDown, WomanFocusBurnFatClicked);
        }

        async void WomanFocusMuscleClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");
            await AddAnswer(((DrMuscleButton)sender).Text);

            BeginnerSetup();
        }

        async void WomanFocusBothClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            await AddAnswer(((DrMuscleButton)sender).Text);
            BeginnerSetup();
        }

        async void WomanFocusBurnFatClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");
            await AddAnswer(((DrMuscleButton)sender).Text);
            BeginnerSetup();
        }

        async void WomanMidsizeClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.MidsizeWomenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitAndStrongLeanerAndComfortableInMyBody);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.Yes, WomanYesMidsizeClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, WomanOtherGoalClicked);
        }

        //Start from here
        async void WomanYesMidsizeClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion(AppResources.GotItAreYouABeginnerWithNoEquipment);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            BeginnerSetup();
        }


        async void WomanFullFClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.FullFiguredOften);
            await AddQuestion(AppResources.HaveAHardTimeLosingWeightGetFatLookingAtFood);

            await AddOptions(AppResources.YesICanGainWeightEasily, FullFClicked);
            await AddOptions(AppResources.NoIDontGainWeightThatEasily, FullFClicked);
        }

        async void FullFClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.ThankYouTitle);
            await AddQuestion(AppResources.FullFiguredWomenAlsoOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitAndStrongWhileDroppingBodyFatShapeArms);

            await AddOptions(AppResources.Yes, WomanYesBigClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, WomanOtherGoalClicked);
        }

        async void WomanYesBigClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");

            await AddQuestion(AppResources.BurningFatGotItAreYouBegginerWithNoEquipment);
            BeginnerSetup();
        }

        async void SkinnyManButton_Clicked(object sender, EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.SkinnyMenOften);
            await AddQuestion(AppResources.HaveAHardTimeGainingWeightSomeSayIEatConstantlyAndWorkMyButtOff);

            await ClearOptions();

            await AddOptions(AppResources.YesIHaveAHardTimeGaining, ManBodyTypeClicked);
            await AddOptions(AppResources.NoIDontHaveAHardTime, ManBodyTypeClicked);
            await AddOptions(AppResources.NotSureIveNeverLiftedBefore, ManBodyTypeClicked);

        }
        async void BigManButton_Clicked(object sender, EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.BigMenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetRidOfThisBodyFatAndLoseMyGut);

            await AddOptions(AppResources.Yes, ManYesBigClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, ManOtherGoalClicked);

        }

        async void ManYesBigClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");
            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.BurningFatGotItAreYouBegginerWithNoEquipment);
            BeginnerSetup();
        }
        async void MidsizeManButton_Clicked(object sender, EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);
            await AddQuestion(AppResources.MidsizeMenOftenSay);
            await AddQuestion(AppResources.TheyWantToGetFitStrongAndMoreMuscularGainLeanMassAndHaveAVisibleSetOf);

            await AddOptions(AppResources.Yes, ManYesMidsizeClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, ManOtherGoalClicked);
        }

        async void ManYesMidsizeClicked(object sender, System.EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            await AddQuestion(AppResources.BuildingMuscleBuriningFatGotItAreYouBeginner);
            BeginnerSetup();
        }


        async void ManBodyTypeClicked(object sender, System.EventArgs e)
        {

            await AddAnswer(((DrMuscleButton)sender).Text);

            await AddQuestion(AppResources.GotIt);
            await AddQuestion(AppResources.SkinnyMenAlsoOftenSay);
            await AddQuestion(AppResources.TheyWantToPutOnLeanMassWhileKeepingmyAbsDefinedGainHealthy);

            await AddOptions(AppResources.Yes, ManYesSkinnyClicked);
            await AddOptions(AppResources.NoChooseOtherGoal, ManOtherGoalClicked);

        }

        async void ManYesSkinnyClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");

            await AddAnswer(((DrMuscleButton)sender).Text);
            await AddQuestion(AppResources.BuildingMuscleGotItAreYouABeginnerWithNoEquipment);
            BeginnerSetup();
        }

        async void ManOtherGoalClicked(object sender, System.EventArgs e)
        {
            try
            {
                await AddAnswer(((DrMuscleButton)sender).Text);

                BotList.Add(new BotModel()
                {
                    Question = AppResources.DontWorryYouCanCustomizeLater,
                    Type = BotType.Ques
                });
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                await Task.Delay(300);
                BotList.Add(new BotModel()
                {
                    Question = AppResources.PleaseChooseAGoal,
                    Type = BotType.Ques
                });

                await Task.Delay(300);

                await ClearOptions();
                var manBurnFatButton = new DrMuscleButton()
                {
                    Text = AppResources.FocusOnBurningFat,
                    TextColor = Color.White
                };
                manBurnFatButton.Clicked += ManFocusBurnFatClicked;
                SetDefaultButtonStyle(manBurnFatButton);
                var grid = new Grid();
                var pancakeView = new PancakeView() {  HeightRequest = 50, Margin = new Thickness(25, 5) };

                pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
                pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#5CD196"), Offset = 1 });
                pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#40A076"), Offset = 0 });

                grid.Children.Add(pancakeView);
                grid.Children.Add(manBurnFatButton);

                stackOptions.Children.Add(grid);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                await Task.Delay(300);

                var manBuildMuscleBurnFatButton = new DrMuscleButton()
                {
                    Text = AppResources.BuildMuscleAndBurnFat,
                    TextColor = Color.White
                };
                manBuildMuscleBurnFatButton.Clicked += ManFocusBothClicked;
                SetDefaultButtonStyle(manBuildMuscleBurnFatButton);
                stackOptions.Children.Add(manBuildMuscleBurnFatButton);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                await Task.Delay(300);

                var manBuildMuscleButton = new DrMuscleButton()
                {
                    Text = AppResources.FocusOnBuildingMuscle,
                    TextColor = Color.White,
                };
                manBuildMuscleButton.Clicked += ManFocusMuscleClicked;
                SetDefaultButtonStyle(manBuildMuscleButton);

                stackOptions.Children.Add(manBuildMuscleButton);
                await Task.Delay(300);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);

            }
            catch (Exception ex)
            {

            }
        }

        async void BeginnerSetup()
        {
            try
            {
                await ClearOptions();

                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                //await AddOptions("At home, no equipment", YesBeginnerClicked);
                //await AddOptions("Gym or home gym", async (sender, e) => {

                LocalDBManager.Instance.SetDBSetting("experience", "less1year");

                //await AddAnswer(((DrMuscleButton)sender).Text);
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                await AddQuestion("How old are you?");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);

                GetAge();
                //});

            }
            catch (Exception ex)
            {

            }
        }

        async void ManFocusMuscleClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "12");

            await AddAnswer(AppResources.FocusOnBuildingMuscle);

            BeginnerSetup();

        }

        async void ManFocusBothClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");

            await AddAnswer(AppResources.BuildMuscleAndBurnFat);
            BeginnerSetup();

        }

        async void ManFocusBurnFatClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("reprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "20");

            await AddAnswer(AppResources.FocusOnBurningFat);
            BeginnerSetup();
        }

        async void YesBeginnerClicked(object sender, System.EventArgs e)
        {
            await ClearOptions();
            LocalDBManager.Instance.SetDBSetting("reprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("repsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("repsmaximum", "15");
            LocalDBManager.Instance.SetDBSetting("experience", "beginner");
            LocalDBManager.Instance.SetDBSetting("workout_place", "homeBodyweightOnly");


            await AddAnswer(((DrMuscleButton)sender).Text);

            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion("How old are you?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
        }


        async void AddGotItBeforeAge()
        {
            try
            {
                await ClearOptions();
                await AddOptions(AppResources.GotIt, GotIt_Clicked);
                async void GotIt_Clicked(object sender, EventArgs e)
                {

                    await AddAnswer(((DrMuscleButton)sender).Text);
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddQuestion("How old are you?");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    GetAge();
                }

            }
            catch (Exception ex)
            {

            }
        }
        async void NoAdvancedClicked(object sender, System.EventArgs e)
        {
            //await AddAnswer(((DrMuscleButton)sender).Text);
            //if (Device.RuntimePlatform.Equals(Device.Android))

            //    await Task.Delay(300);
            await AddQuestion("How long have you been working out for?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await ClearOptions();
            await AddOptions(AppResources.LessThan1Year, LessOneYearClicked);
            await AddOptions(AppResources.OneToThreeYears, OneThreeYearsClicked);
            await AddOptions(AppResources.MoreThan3Years, More3YearsClicked);
        }


        async void LessOneYearClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("experience", "less1year");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddQuestion("OK, good news! You can expect fast progress. Most new lifters get their best results by training their full body 3x/week. I'm setting up your program that way.");

            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //AddGotItBeforeAge();
            await AddQuestion("How old are you?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
        }

        async void OneThreeYearsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("experience", "1-3years");

            await AddAnswer(((DrMuscleButton)sender).Text);
            //await AddQuestion("OK, great. At your level, most lifters get their best results by training their upper and their lower body 2x/week. I'm setting up your program that way.");
            //AddGotItBeforeAge();
            await AddQuestion("How old are you?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
        }

        async void More3YearsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("experience", "more3years");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddQuestion("OK, great. At your level, many lifters get great results by training their upper and their lower body 2x/week with different exercises variations. I'm setting up your program that way.");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //AddGotItBeforeAge();
            await AddQuestion("How old are you?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
        }

        async void workoutPlace()
        {
            await AddQuestion("Where do you work out?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.Gym, GymClicked);
            await AddOptions(AppResources.HomeGymBasicEqipment, HomeClicked);
            await AddOptions(AppResources.HomeBodtweightOnly, BodyweightClicked);

        }

        async void GymClicked(object senders, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("workout_place", "gym");

            await AddAnswer(((DrMuscleButton)senders).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion("Confirm equipment");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //SetupQuickMode();
            IsPully = true;
            await AddCheckbox("Pulley", (sender, ev) =>
            {
                IsPully = !IsPully;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPully ? "done.png" : "Undone.png";
            }, true);
            IsPlates = true;
            await AddCheckbox("Plates", (sender, ev) =>
            {
                IsPlates = !IsPlates;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPlates ? "done.png" : "Undone.png";
            }, true);
            IsChinupBar = true;
            await AddCheckbox("Chin-up bar", (sender, ev) =>
            {
                IsChinupBar = !IsChinupBar;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsChinupBar ? "done.png" : "Undone.png";
            }, true);

            isDumbbells = true;
            await AddCheckbox("Dumbbells", (sender, ev) =>
            {
                isDumbbells = !isDumbbells;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = isDumbbells ? "done.png" : "Undone.png";
            }, true);
            IsEquipment = true;
            
            await AddOptions("Continue", async (sender, ee) =>
            {
                await AddAnswer("Continue");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                //SetupQuickMode();
                AskforBodypartPriority();
            });
        }

        async void HomeClicked(object senders, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("workout_place", "home");

            await AddAnswer(((DrMuscleButton)senders).Text);
            ClearOptions();
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion("What equipment do you have?");

            await AddCheckbox("Pulley", (sender, ev) =>
            {
                IsPully = !IsPully;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPully ? "done.png" : "Undone.png";
            });

            await AddCheckbox("Plates", (sender, ev) =>
            {
                IsPlates = !IsPlates;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsPlates ? "done.png" : "Undone.png";
            });
            await AddCheckbox("Chin-up bar", (sender, ev) =>
            {
                IsChinupBar = !IsChinupBar;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = IsChinupBar ? "done.png" : "Undone.png";
            });
            
            isDumbbells = true;
            await AddCheckbox("Dumbbells", (sender, ev) =>
            {
                isDumbbells = !isDumbbells;
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = isDumbbells ? "done.png" : "Undone.png";
            }, true);

            IsEquipment = true;

            await AddOptions("Continue", async (sender, ee) =>
            {
                await AddAnswer("Continue");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                //SetupQuickMode();
                AskforBodypartPriority();
            });


        }

        async void BodyweightClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("workout_place", "homeBodyweightOnly");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddQuestion("OK, bodyweight exercises only. No problem.");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //SetupQuickMode();
            AddCardio();
        }

        

        async void AskforBodypartPriority()
        {
            bodypartName = "balanced";
            await AddQuestion("Wanna prioritize a body part?");
            if (LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd() == "Man")
            {
                bodypart1 = await AddCheckbox("Biceps", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "Biceps";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";


                });
                bodypart2 = await AddCheckbox("Chest", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "Chest";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";
                });
                bodypart3 = await AddCheckbox("Abs", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "Abs";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";

                });
                bodypartBalanced = await AddCheckbox("Balanced", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
                });
            }
            else
            {
                bodypart1 = await AddCheckbox("Abs", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "Abs";

                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";

                });
                bodypart2 = await AddCheckbox("Legs", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "Legs";
                   
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";
                });
                bodypart3 = await AddCheckbox("Glutes", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "Glutes";

                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";
                });
                bodypartBalanced = await AddCheckbox("Balanced", (sender, ev) =>
                {
                    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                    img.Source = "done.png";
                    bodypartName = "";
              
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
                    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";

                });
            }
            await AddOptions("Continue", async (sender, ee) =>
            {
                if (bodypartName.Equals("balanced"))
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Title = "Error",
                        Message = $"Please select one body part.",
                        OkText = AppResources.Ok
                    });
                    return;
                }
                await AddAnswer("Continue");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                //SetupQuickMode();
                AddCardio();
            });
        }

        async void AddCardio()
        {
            await AddQuestion("Include cardio?", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions("Yes", async (sender, e) =>
            {
                await AddAnswer("Yes");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                IsIncludeCardio = true;
                SetupQuickMode();
            });
            await AddOptions("No", async (sender, e) =>
            {
                IsIncludeCardio = false;
                await AddAnswer("No");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                SetupQuickMode();
            });
        }

        async void SetupQuickMode()
        {

            if (LocalDBManager.Instance.GetDBSetting("NewLevel") != null && LocalDBManager.Instance.GetDBSetting("ExLevel") != null && LocalDBManager.Instance.GetDBSetting("NewLevel").Value == "Streamline" && LocalDBManager.Instance.GetDBSetting("ExLevel").Value == "New")
            {
                LocalDBManager.Instance.SetDBSetting("QuickMode", "false");
                ProgramReadyInstruction();
                return;
            }
            await AddQuestion("How long do you want to work out?", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions("30 min", QuickmodeOnClicked);
            await AddOptions("45 min", QuickmodeMediumClicked);
            await AddOptions("1+ hour", QuickmodeOffClicked);
        }
        async void QuickmodeOnClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("QuickMode", "true");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            LearnMoreTimeline();


        }

        async void LearnMoreTimeline()
        {
            //BotList.Add(new BotModel()
            //{
            //    Question = "Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we'll treat your feedback like gold. Got a suggestion? Get in touch. We release new features every month.",
            //    Type = BotType.Ques
            //});
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            //await AddQuestion("More features are coming! Got a suggestion? Get in touch. We release new features every week.", false);

            ProgramReadyInstruction();
        }

        async void QuickmodeOffClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("QuickMode", "false");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            LearnMoreTimeline();

        }

        async void QuickmodeMediumClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("QuickMode", "null");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            LearnMoreTimeline();

        }

        async void SetupMassUnit()
        {
            ClearOptions();
            IsBodyweightPopup = true;
            await AddQuestion("What is your body weight?");
            await PopupNavigation.Instance.PushAsync(new BodyweightPopup());
            //await AddQuestion(AppResources.DoYouUseLbsOrKgs, false);

            //await AddOptions(AppResources.Lbs, LbsClicked);
            //await AddOptions(AppResources.Kg, KgClicked);


        }

        async void SetupGender()
        {
            ManBetterHealth = false;
            ManLessFat = false;
            ManStorngerSexDrive = false;
            ManMoreMuscle = false;
            FemaleMoreEnergy = false;
            FemaleToned = false;

            await AddOptions(AppResources.Man, ManButton_Clicked);
            await AddOptions(AppResources.Woman, WomanButton_Clicked);


        }

        async void GetAge()
        {
            BotList.Add(new BotModel()
            { Type = BotType.Empty });
            AgePicker.IsVisible = true;
            Device.BeginInvokeOnMainThread(() =>
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            });
            try
            {

                Device.BeginInvokeOnMainThread(() =>
                {
                    AgePicker.Focus();
                });

            }
            catch (Exception ex)
            {

            }
            //PromptConfig firsttimeExercisePopup = new PromptConfig()
            //{
            //    InputType = InputType.Number,
            //    IsCancellable = false,
            //    Title = "How old are you?",
            //    Message = $"Your age influences how often you should train.",
            //    Placeholder = "Enter your age here",
            //    OkText = AppResources.Ok,
            //    //AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogFirstTimeExercise),
            //    OnAction = async (ageResponse) =>
            //    {
            //        if (string.IsNullOrWhiteSpace(ageResponse.Value) || Convert.ToDecimal(ageResponse.Value, CultureInfo.InvariantCulture) < 1)
            //        {
            //            BotList.Remove(BotList.Last());
            //            GetAge();
            //            return;
            //        }
            //        BotList.Remove(BotList.Last());
            //        int age = Convert.ToInt32(ageResponse.Value, CultureInfo.InvariantCulture);
            //        LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(age));
            //        await AddAnswer(Convert.ToString(age));


            //        if (age > 50)
            //            learnMore.AgeDesc = $"Recovery is slower at {age}. So, I added easy days to your program.";
            //        else if (age > 30)
            //            learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I'm updating your program to make sure you train each muscle max 2x a week.";
            //        else
            //            learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";
            //        await AddQuestion(learnMore.AgeDesc);
            //        if (LocalDBManager.Instance.GetDBSetting("experience").Value == "beginner")
            //            SetupQuickMode();
            //        else
            //            workoutPlace();
            //    }
            //};
            //firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
            //UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        async void LbsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("massunit", "lb");

            await AddAnswer(((DrMuscleButton)sender).Text);
            TakeBodyWeight();
        }

        async void KgClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("massunit", "kg");

            await AddAnswer(((DrMuscleButton)sender).Text);


            TakeBodyWeight();

        }

        async void GotoLevelUp()
        {



            await AddQuestion($"All right! Your custom program is ready. Learn more?");
            await AddOptions(AppResources.LearnMore, LearnMoreButton_Clicked);
            await AddOptions(AppResources.Skip, LearnMoreSkipButton_Clicked);

        }
        private void GetFirstName()
        {
            StackSignupMenu.IsVisible = false;
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
                LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                CurrentLog.Instance.ShowWelcomePopUp = true;
                ((App)Application.Current).displayCreateNewAccount = false;

                GetPassword();
            }
        }

        private void GetEmail()
        {
            PromptConfig p = new PromptConfig()
            {
                InputType = InputType.Email,
                IsCancellable = false,
                Title = "Your email",
                Placeholder = "Enter your email",
                OkText = AppResources.Continue,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = new Action<PromptResult>(GetEmailAction)
            };

            UserDialogs.Instance.Prompt(p);
        }
        private async void GetEmailAction(PromptResult response)
        {

            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Message = AppResources.PleaseCheckInternetConnection,
                            //    Title = AppResources.ConnectionError
                            //});
                GetEmailAction(response);
                return;
            }
            if (response.Ok)
            {
                if (!Emails.ValidateEmail(response.Text))
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        Message = AppResources.InvalidEmailError,
                        Title = AppResources.InvalidEmailAddress,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                    GetEmail();
                    return;
                }
                BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = response.Text });
                if (existingUser != null)
                {
                    if (existingUser.Result)
                    {

                        ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
                        {
                            Title = "Email already in use",
                            Message = "Use another email or log into your existing account.",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = "Use another email",
                            CancelText = AppResources.LogIn,

                        };
                        var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
                        if (actionOk)
                        {
                            GetEmail();
                        }
                        else
                        {
                            ((App)Application.Current).displayCreateNewAccount = true;
                            await PagesFactory.PushAsync<WelcomePage>();
                        }

                        return;
                    }

                }
                else
                    GetEmailAction(response);

                App.IsNewUser = true;

                LocalDBManager.Instance.SetDBSetting("email", response.Text);
                await AddAnswer(response.Text);
                await AddQuestion("Enter first name");
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                }

                GetFirstName();
            }
        }

        private void GetPassword()
        {
            PromptConfig p = new PromptConfig()
            {
                InputType = InputType.Password,
                IsCancellable = false,
                Title = "Create password",
                Message = "At least 6 characters",
                Placeholder = "Create password",
                OkText = AppResources.Create,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = new Action<PromptResult>(GetPasswordAction)
            };

            UserDialogs.Instance.Prompt(p);
        }
        private async void GetPasswordAction(PromptResult response)
        {
            if (response.Ok)
            {

                if (response.Text.Length < 6)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        Message = AppResources.PasswordLengthError,
                        Title = AppResources.Error,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                    GetPassword();
                    return;
                }
                await AddAnswer(response.Text);
                LocalDBManager.Instance.SetDBSetting("password", response.Text);
                await Task.Delay(100);
                CreateAccountBeforeDemoButton_Clicked();
                //CreateAccountButton_Clicked();
            }

        }
        async void CreateAccountButton_Clicked()
        {
            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;
            var WorkoutInfo2 = "";
            //Setup Program
            if (experienceSetting != null && workoutPlaceSetting != null)
            {
                if (workoutPlaceSetting.Value == "gym")
                {

                    if (experienceSetting.Value == "less1year")
                    {
                        WorkoutInfo2 = "[Gym] Full-Body";
                        workoutId = 104;
                        programId = 10;
                        remainingWorkout = 18;
                    }
                    if (experienceSetting.Value == "1-3years")
                    {
                        WorkoutInfo2 = "[Gym] Upper-Body";
                        workoutId = 106;
                        programId = 15;
                        remainingWorkout = 32;
                    }
                    if (experienceSetting.Value == "more3years")
                    {
                        WorkoutInfo2 = "[Gym] Upper-Body Level 2";
                        workoutId = 424;
                        programId = 16;
                        remainingWorkout = 40;
                    }
                }
                else if (workoutPlaceSetting.Value == "home")
                {
                    if (experienceSetting.Value == "less1year")
                    {
                        WorkoutInfo2 = "[Home] Full-Body";
                        workoutId = 108;
                        programId = 17;
                        remainingWorkout = 18;
                    }
                    if (experienceSetting.Value == "1-3years")
                    {
                        WorkoutInfo2 = "[Home] Upper-Body";
                        workoutId = 109;
                        programId = 21;
                        remainingWorkout = 24;
                    }
                    if (experienceSetting.Value == "more3years")
                    {
                        WorkoutInfo2 = "[Home] Upper-Body Level 2";
                        workoutId = 428;
                        programId = 22;
                        remainingWorkout = 40;
                    }
                }
                else if (workoutPlaceSetting.Value == "homeBodyweightOnly")
                {
                    WorkoutInfo2 = "Bodyweight Level 2";
                    workoutId = 12646;
                    programId = 487;
                    remainingWorkout = 12;
                }

                if (experienceSetting.Value == "beginner")
                {
                    WorkoutInfo2 = "Bodyweight Level 1";
                    workoutId = 12645;
                    programId = 488;
                    remainingWorkout = 6;
                }

                string ProgramLabel = AppResources.NotSetUp;
                int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                switch (programId)
                {
                    case 10:
                        ProgramLabel = "[Gym] Full-Body Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Gym] Full-Body Level 6";
                            programId = 395;
                            WorkoutInfo2 = "[Gym] Full-Body 6A (easy)";
                            workoutId = 2312;
                        }
                        else if (age > 30)
                        {
                            ProgramLabel = "[Gym] Up/Low Split Level 1";
                            programId = 15;
                            WorkoutInfo2 = "[Gym] Lower Body";
                            workoutId = 107;
                        }
                        break;
                    case 15:
                        ProgramLabel = "[Gym] Up/Low Split Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Gym] Up/Low Split Level 6";
                            programId = 401;
                            WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                            workoutId = 2337;
                        }
                        break;
                    case 16:
                        ProgramLabel = "[Gym] Up/Low Split Level 2";
                        if (age > 50)
                        {
                            ProgramLabel = "[Gym] Up/Low Split Level 6";
                            programId = 401;
                            WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                            workoutId = 2337;
                        }
                        break;
                    case 17:
                        ProgramLabel = "[Home] Full-Body Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Home] Full-Body Level 6";
                            programId = 398;
                            WorkoutInfo2 = "[Home] Full-Body 6A (easy)";
                            workoutId = 2325;
                        }
                        else if (age > 30)
                        {
                            ProgramLabel = "[Home] Up/Low Split Level 1";
                            programId = 21;
                            WorkoutInfo2 = "[Home] Lower Body";
                            workoutId = 110;
                        }
                        break;
                    case 21:
                        ProgramLabel = "[Home] Up/Low Split Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Home] Up/Low Split Level 6";
                            programId = 404;
                            WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                            workoutId = 2361;
                        }
                        break;
                    case 22:
                        ProgramLabel = "[Home] Up/Low Split Level 2";
                        if (age > 50)
                        {
                            ProgramLabel = "[Home] Up/Low Split Level 6";
                            programId = 404;
                            WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                            workoutId = 2361;
                        }
                        break;
                    case 487:
                        ProgramLabel = "Bodyweight Level 2";
                        break;
                    case 488:
                        ProgramLabel = "Bodyweight Level 1";
                        break;
                }
                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", workoutId.ToString());
                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", WorkoutInfo2);
                LocalDBManager.Instance.SetDBSetting("recommendedProgramId", programId.ToString());
                LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", remainingWorkout.ToString());

                LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", ProgramLabel);
            }
            //SignUp here
            RegisterModel registerModel = new RegisterModel();

            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                registerModel.IsQuickMode = false;
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                    registerModel.IsQuickMode = null;
                else
                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            }
            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            registerModel.Password = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.ConfirmPassword = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.LearnMoreDetails = learnMore;
            registerModel.IsHumanSupport = IsHumanSupport;
            registerModel.IsCardio = IsIncludeCardio;
            registerModel.BodyPartPrioriy = bodypartName;

            registerModel.MainGoal = mainGoal;
            if (IsEquipment)
            {
                registerModel.EquipmentModel = new EquipmentModel()
                {
                    IsEquipmentEnabled = true,
                    IsDumbbellEnabled = isDumbbells,
                    IsPlateEnabled = IsPlates,
                    IsPullyEnabled = IsPully,
                    IsChinUpBarEnabled = IsChinupBar
                };
            }
            if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null)
            {
                var increments = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value, System.Globalization.CultureInfo.InvariantCulture);
                var incrementsWeight = new MultiUnityWeight(increments, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                registerModel.Increments = incrementsWeight.Kg;
            }
            try
            {


                BooleanModel registerResponse = await DrMuscleRestClient.Instance.RegisterUser(registerModel);
                if (registerResponse.Result)
                {
                    DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                }
            }
            catch (Exception ex)
            {

            }
            //Login
            LoginSuccessResult lr = await DrMuscleRestClient.Instance.Login(new LoginModel()
            {
                Username = registerModel.EmailAddress,
                Password = registerModel.Password
            });

            if (lr != null && !string.IsNullOrEmpty(lr.access_token))
            {
                DateTime current = DateTime.Now;

                UserInfosModel uim = await DrMuscleRestClient.Instance.GetUserInfo();

                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("password", registerModel.Password);
                LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());

                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); if (uim.Age != null)
                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
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
                if (uim.EquipmentModel != null)
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                    LocalDBManager.Instance.SetDBSetting("Plate", "true");
                    LocalDBManager.Instance.SetDBSetting("Pully", "true");
                }

                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                ((App)Application.Current).displayCreateNewAccount = true;

                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                else
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");
                App.IsNewFirstTime = false;
                if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                        LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                {
                    try
                    {
                        long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                        long pId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                        var upi = new GetUserProgramInfoResponseModel()
                        {
                            NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                            RecommendedProgram = new WorkoutTemplateGroupModel() { Id = pId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                        };
                        if (upi != null)
                        {
                            WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
                            if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
                            {
                                try
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

                                    //GetUserWorkoutTemplateResponseModel w = await DrMuscleRestClient.Instance.GetUserWorkout();
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

                            //await PagesFactory.PopToRootAsync(true);
                            //App.IsDemoProgress = false;
                            //App.IsWelcomeBack = true;
                            //LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                            try
                            {
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
                                await PagesFactory.PushAsync<DemoPage>();
                                //CancelNotification();
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



                MessagingCenter.Send(this, "BackgroundImageUpdated");

            }
            else
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = AppResources.EmailAndPasswordDoNotMatch,
                    Title = AppResources.UnableToLogIn,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
        }
        async void CreateAccountBeforeDemoButton_Clicked()
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
            LocalDBManager.Instance.SetDBSetting("LoginType", "Email");

            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;
            var WorkoutInfo2 = "";
            //Setup Program

            //SignUp here
            RegisterModel registerModel = new RegisterModel();

            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;

            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            registerModel.Password = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.ConfirmPassword = LocalDBManager.Instance.GetDBSetting("password").Value;

            try
            {

                BooleanModel registerResponse = await DrMuscleRestClient.Instance.RegisterUserBeforeDemo(registerModel);
                if (registerResponse.Result)
                {
                    DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                }
            }
            catch (Exception ex)
            {

            }
            //Login
            LoginSuccessResult lr = await DrMuscleRestClient.Instance.Login(new LoginModel()
            {
                Username = registerModel.EmailAddress,
                Password = registerModel.Password
            });

            if (lr != null && !string.IsNullOrEmpty(lr.access_token))
            {
                DateTime current = DateTime.Now;

                UserInfosModel uim = await DrMuscleRestClient.Instance.GetUserInfo();

                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("password", registerModel.Password);
                LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false");                //if (uim.ReminderTime != null)
                //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                //if (uim.ReminderDays != null)
                //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);
                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
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
                if (uim.EquipmentModel != null)
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                    LocalDBManager.Instance.SetDBSetting("Plate", "true");
                    LocalDBManager.Instance.SetDBSetting("Pully", "true");
                }
                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());
                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                ((App)Application.Current).displayCreateNewAccount = true;

                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                else
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");
                App.IsNewFirstTime = false;




                try
                {
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
                    PagesFactory.PushAsync<DemoPage>();
                    //CancelNotification();
                }
                catch (Exception ex)
                {

                }

            }
            else
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = AppResources.EmailAndPasswordDoNotMatch,
                    Title = AppResources.UnableToLogIn,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
        }
        async void CreateAccountAfterDemoButton_Clicked()
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            if (isProcessing)
                return;
            isProcessing = true;

            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;
            var WorkoutInfo2 = "";
            //Setup Program
            if (experienceSetting != null && workoutPlaceSetting != null)
            {
                if (workoutPlaceSetting.Value == "gym")
                {

                    if (experienceSetting.Value == "less1year")
                    {
                        WorkoutInfo2 = "[Gym] Full-Body";
                        workoutId = 104;
                        programId = 10;
                        remainingWorkout = 18;
                    }
                    if (experienceSetting.Value == "1-3years")
                    {
                        WorkoutInfo2 = "[Gym] Upper-Body";
                        workoutId = 106;
                        programId = 15;
                        remainingWorkout = 32;
                    }
                    if (experienceSetting.Value == "more3years")
                    {
                        WorkoutInfo2 = "[Gym] Upper-Body Level 2";
                        workoutId = 424;
                        programId = 16;
                        remainingWorkout = 40;
                    }
                }
                else if (workoutPlaceSetting.Value == "home")
                {
                    if (experienceSetting.Value == "less1year")
                    {
                        WorkoutInfo2 = "[Home] Full-Body";
                        workoutId = 108;
                        programId = 17;
                        remainingWorkout = 18;
                    }
                    if (experienceSetting.Value == "1-3years")
                    {
                        WorkoutInfo2 = "[Home] Upper-Body";
                        workoutId = 109;
                        programId = 21;
                        remainingWorkout = 24;
                    }
                    if (experienceSetting.Value == "more3years")
                    {
                        WorkoutInfo2 = "[Home] Upper-Body Level 2";
                        workoutId = 428;
                        programId = 22;
                        remainingWorkout = 40;
                    }
                }
                else if (workoutPlaceSetting.Value == "homeBodyweightOnly")
                {
                    WorkoutInfo2 = "Bodyweight Level 2";
                    workoutId = 12646;
                    programId = 487;
                    remainingWorkout = 12;
                }

                if (experienceSetting.Value == "beginner")
                {
                    WorkoutInfo2 = "Bodyweight Level 1";
                    workoutId = 12645;
                    programId = 488;
                    remainingWorkout = 6;
                }

                string ProgramLabel = AppResources.NotSetUp;
                int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                switch (programId)
                {
                    case 10:
                        ProgramLabel = "[Gym] Full-Body Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Gym] Full-Body Level 6";
                            programId = 395;
                            WorkoutInfo2 = "[Gym] Full-Body 6A (easy)";
                            workoutId = 2312;
                        }
                        else if (age > 30)
                        {
                            ProgramLabel = "[Gym] Up/Low Split Level 1";
                            programId = 15;
                            WorkoutInfo2 = "[Gym] Lower Body";
                            workoutId = 107;
                        }
                        break;
                    case 15:
                        ProgramLabel = "[Gym] Up/Low Split Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Gym] Up/Low Split Level 6";
                            programId = 401;
                            WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                            workoutId = 2337;
                        }
                        break;
                    case 16:
                        ProgramLabel = "[Gym] Up/Low Split Level 2";
                        if (age > 50)
                        {
                            ProgramLabel = "[Gym] Up/Low Split Level 6";
                            programId = 401;
                            WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                            workoutId = 2337;
                        }
                        break;
                    case 17:
                        ProgramLabel = "[Home] Full-Body Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Home] Full-Body Level 6";
                            programId = 398;
                            WorkoutInfo2 = "[Home] Full-Body 6A (easy)";
                            workoutId = 2325;
                        }
                        else if (age > 30)
                        {
                            ProgramLabel = "[Home] Up/Low Split Level 1";
                            programId = 21;
                            WorkoutInfo2 = "[Home] Lower Body";
                            workoutId = 110;
                        }
                        break;
                    case 21:
                        ProgramLabel = "[Home] Up/Low Split Level 1";
                        if (age > 50)
                        {
                            ProgramLabel = "[Home] Up/Low Split Level 6";
                            programId = 404;
                            WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                            workoutId = 2361;
                        }
                        break;
                    case 22:
                        ProgramLabel = "[Home] Up/Low Split Level 2";
                        if (age > 50)
                        {
                            ProgramLabel = "[Home] Up/Low Split Level 6";
                            programId = 404;
                            WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                            workoutId = 2361;
                        }
                        break;
                    case 487:
                        ProgramLabel = "Bodyweight Level 2";
                        break;
                    case 488:
                        ProgramLabel = "Bodyweight Level 1";
                        break;
                }
                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", workoutId.ToString());
                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", WorkoutInfo2);
                LocalDBManager.Instance.SetDBSetting("recommendedProgramId", programId.ToString());
                LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", remainingWorkout.ToString());

                LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", ProgramLabel);
            }
            //SignUp here
            RegisterModel registerModel = new RegisterModel();

            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                registerModel.IsQuickMode = false;
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                    registerModel.IsQuickMode = null;
                else
                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            }
            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            registerModel.Password = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.ConfirmPassword = LocalDBManager.Instance.GetDBSetting("password").Value;
            registerModel.LearnMoreDetails = learnMore;
            registerModel.IsHumanSupport = IsHumanSupport;
            registerModel.IsCardio = IsIncludeCardio;
            registerModel.BodyPartPrioriy = bodypartName;

            registerModel.MainGoal = mainGoal;
            if (IsEquipment)
            {
                registerModel.EquipmentModel = new EquipmentModel()
                {
                    IsEquipmentEnabled = true,
                    IsDumbbellEnabled = isDumbbells,
                    IsPlateEnabled = IsPlates,
                    IsPullyEnabled = IsPully,
                    IsChinUpBarEnabled = IsChinupBar
                };
            }
            if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null)
            {
                var increments = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value, System.Globalization.CultureInfo.InvariantCulture);
                var incrementsWeight = new MultiUnityWeight(increments, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                registerModel.Increments = incrementsWeight.Kg;
            }

            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            UserInfosModel registerResponse = await DrMuscleRestClient.Instance.RegisterUserAfterDemo(registerModel);

            if (registerResponse != null)
            {
                CancelNotification();
                SetTrialUserNotifications();
                DateTime current = DateTime.Now;

                UserInfosModel uim = registerResponse;

                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("password", registerModel.Password);
                //LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                //LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());

                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); if (uim.Age != null)
                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
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
                if (uim.EquipmentModel != null)
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                    LocalDBManager.Instance.SetDBSetting("Plate", "true");
                    LocalDBManager.Instance.SetDBSetting("Pully", "true");
                }

                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                ((App)Application.Current).displayCreateNewAccount = true;

                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                else
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");
                App.IsNewFirstTime = false;
                
                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                long pId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                var upi = new GetUserProgramInfoResponseModel()
                {
                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = pId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
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
                            // await UserDialogs.Instance.AlertAsync(new AlertConfig()
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
                        CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
                        CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
                        CurrentLog.Instance.WorkoutStarted = true;
                        if (Device.RuntimePlatform.Equals(Device.Android))
                        {
                            App.IsDemoProgress = false;
                            App.IsWelcomeBack = true;
                            App.IsNewUser = true;
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
                            App.IsNewUser = true;
                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            CurrentLog.Instance.Exercise1RM.Clear();
                            await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                            await PagesFactory.PopToRootMoveAsync(true);
                        }

                    }
                    else
                    {
                        await PagesFactory.PopToRootAsync(true);
                        App.IsDemoProgress = false;
                        App.IsWelcomeBack = true;
                        App.IsNewUser = true;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                        await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                    }
                }


                //await PagesFactory.PopToRootAsync(true);

            }
            else
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = "Something went wrong, please try again.",
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            isProcessing = false;
        }
        async void LearnMoreButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            await AddAnswer(((DrMuscleButton)sender).Text);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            await ClearOptions();
            await Task.Delay(300);



            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            learnMore.Exp = "";
            learnMore.ExpDesc = "";
            if (experienceSetting != null)
            {
                if (experienceSetting.Value == "less1year")
                {
                    learnMore.Exp = $"Less than 1 year";
                    learnMore.ExpDesc = "You're new to lifting, so I recommend you train each muscle 3x a week on a full-body program. You will progress faster that way.";
                }
                if (experienceSetting.Value == "1-3years")
                {
                    learnMore.Exp = $"1-3 years";
                    learnMore.ExpDesc = "You have been lifting for over a year, so I recommend you train each muscle 2x a week on a split-body program. This gives you more time to recover between workouts for each muscle.";
                }
                if (experienceSetting.Value == "more3years")
                {
                    learnMore.Exp = $"More than 3 years";
                    learnMore.ExpDesc = "You have been lifting for 3+ years, so I recommend you train each muscle 2x a week on a split-body program with A and B days. This gives you more time to recover between workouts for each muscle and different exercises on A and B days. At your level, this is almost always needed to continue making progress.";
                }
                if (!string.IsNullOrEmpty(learnMore.Exp))
                {
                    await AddQuestion($"Your experience: {learnMore.Exp}");
                    await AddQuestion(learnMore.ExpDesc);
                    await AddOptions(AppResources.GotIt, LearnMoreSteps2);
                    return;
                }
            }

            LearnMoreSteps2(sender, e);

        }

        async void LearnMoreSteps2(object sender, EventArgs e)
        {
            await ClearOptions();
            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
            if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscle")
            {
                learnMore.Focus = IsWoman ? "Getting stronger" : "Building muscle";
                learnMore.FocusDesc = IsWoman ? "To get stronger, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range." : "To build muscle, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscleBurnFat")
            {
                learnMore.Focus = IsWoman ? "Overall fitness" : "Building muscle and burning fat";
                learnMore.FocusDesc = IsWoman ? "For overall fitness, I recommend you repeat each exercise 8-15 times." : "To build muscle and burn fat, I recommend you repeat each exercise 8-15 times.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "FatBurning")
            {
                learnMore.Focus = "Burning fat";
                learnMore.FocusDesc = "To burn fat, I recommend you repeat each exercise 12-20 times. You will burn more calories by lifting in that range.";
            }
            await AddQuestion($"Your focus: {learnMore.Focus}");
            await AddQuestion(learnMore.FocusDesc);
            await AddOptions(AppResources.GotIt, LearnMoreSteps3);

        }
        async void LearnMoreSteps3(object sender, EventArgs e)
        {
            await ClearOptions();
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            learnMore.Age = age;
            await AddQuestion($"Your age: {age}");
            if (age > 50)
                learnMore.AgeDesc = $"Recovery is slower at {age}. So, I added easy days to your program.";
            else if (age > 30)
                learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I'm updating your program to make sure you train each muscle max 2x a week.";
            else
                learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";
            //await AddQuestion(learnMore.AgeDesc);
            //await Task.Delay(100);
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            }
            //await AddOptions(AppResources.GotIt, LearnMoreComplete
            LearnMoreComplete(sender, e);
        }

        async void LearnMoreComplete(object sender, EventArgs e)
        {
            await ClearOptions();
            await ProgramReadyInstruction();

            await AddOptions(AppResources.GotIt, GotItButton_Clicked);

            stackOptions.Children.Add(TermsConditionStack);
            TermsConditionStack.IsVisible = true;

        }
        async Task ProgramReadyInstruction()
        {


            string goalLabel = "";
            try
            {

                if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscle")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBuild;
                }
                else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscleBurnFat")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBuildNBuildFat;
                }
                else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "FatBurning")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBurnFatFaster;
                }


            }
            catch (Exception ex)
            { }

            try
            {
                await ClearOptions();
                DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
                DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
                var programId = 0;
                if (experienceSetting != null && workoutPlaceSetting != null)
                {
                    if (workoutPlaceSetting.Value == "gym")
                    {

                        if (experienceSetting.Value == "less1year")
                        {
                            programId = 10;
                        }
                        if (experienceSetting.Value == "1-3years")
                        {
                            programId = 15;
                        }
                        if (experienceSetting.Value == "more3years")
                        {
                            programId = 16;
                        }
                    }
                    else if (workoutPlaceSetting.Value == "home")
                    {
                        if (experienceSetting.Value == "less1year")
                        {
                            programId = 17;
                        }
                        if (experienceSetting.Value == "1-3years")
                        {
                            programId = 21;
                        }
                        if (experienceSetting.Value == "more3years")
                        {
                            programId = 22;
                        }
                    }
                    else if (workoutPlaceSetting.Value == "homeBodyweightOnly")
                    {
                        programId = 487;
                    }

                    if (experienceSetting.Value == "beginner")
                    {
                        programId = 488;
                    }
                    var ProgramLabel = "";
                    int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                    learnMore.Age = age;
                    switch (programId)
                    {
                        case 10:
                            ProgramLabel = "[Gym] Full-Body Level 1";
                            if (age > 50)
                            {

                                ProgramLabel = "[Gym] Full-Body Level 6";
                                programId = 395;
                            }
                            else if (age > 30)
                            {
                                ProgramLabel = "[Gym] Up/Low Split Level 1";
                                programId = 15;
                            }

                            break;
                        case 15:
                            ProgramLabel = "[Gym] Up/Low Split Level 1";
                            if (age > 50)
                            {

                                ProgramLabel = "[Gym] Up/Low Split Level 6";
                                programId = 401;
                            }
                            break;
                        case 16:
                            ProgramLabel = "[Gym] Up/Low Split Level 2";
                            if (age > 50)
                            {

                                ProgramLabel = "[Gym] Up/Low Split Level 6";
                                programId = 401;
                            }
                            break;
                        case 17:
                            ProgramLabel = "[Home] Full-Body Level 1";

                            if (age > 50)
                            {
                                ProgramLabel = "[Home] Full-Body Level 6";
                                programId = 398;
                            }
                            else if (age > 30)
                            {
                                ProgramLabel = "[Home] Up/Low Split Level 1";
                                programId = 21;
                            }
                            break;
                        case 21:
                            ProgramLabel = "[Home] Up/Low Split Level 1";

                            if (age > 50)
                            {
                                ProgramLabel = "[Home] Up/Low Split Level 6";
                                programId = 404;
                            }
                            break;
                        case 22:
                            ProgramLabel = "[Home] Up/Low Split Level 2";
                            if (age > 50)
                            {
                                ProgramLabel = "[Home] Up/Low Split Level 6";
                                programId = 404;
                            }
                            break;
                        case 487:
                            ProgramLabel = "Bodyweight Level 2";
                            break;
                        case 488:
                            ProgramLabel = "Bodyweight Level 1";
                            break;
                    }

                    if (age > 51)
                    {

                    }
                    var weekX = 0;
                    var dayText = "";
                    var instructionText = "";
                    instructionText += "- This template is flexible\n";
                    instructionText += AppResources.YouCanChangeWorkoutDays + "\n";
                    if (LocalDBManager.Instance.GetDBSetting("experience").Value == "more3years" || LocalDBManager.Instance.GetDBSetting("experience").Value == "1-3years" || ProgramLabel.ToLower().Contains("split"))
                    {
                        weekX = 4;
                        dayText += "Your week should look like this:" + "\n";
                        dayText += AppResources.MondayUpperBody1More1Year + "\n";
                        dayText += AppResources.TuesdayLowerBodyMore1Year + "\n";
                        dayText += AppResources.WednesdayOffMore1Year + "\n";
                        dayText += AppResources.ThursdayUpperBodyMore1Year + "\n";
                        dayText += AppResources.FridayOrSaturdayLowerBodyMore1Year + "\n";
                        dayText += AppResources.SundayOffMore1Year;
                        instructionText += AppResources.WorkOutYourUpperAndYourLowerBody2xWeekForBestResultsMore1Year + "\n";
                        instructionText += "- Don't worry: you can change everything later.";
                    }
                    else
                    {
                        weekX = 3;
                        dayText += "Your week should look like this:" + "\n";
                        dayText += AppResources.MondayFullBody + "\n";
                        dayText += AppResources.TuesdayOff + "\n";
                        dayText += AppResources.WednesdayFullBody + "\n";
                        dayText += AppResources.ThursdayOff + "\n";
                        dayText += AppResources.FridayOrSaturdayFullBody + "\n";
                        dayText += AppResources.SundayOff;
                        instructionText += AppResources.WorkOutYourFullBody3xWeekForBestResults + "\n";
                        instructionText += "- Don't worry: you can change everything later.";
                    }

                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);


                    LocalDBManager.Instance.SetDBSetting("ReadyToSignup", "true");
                    try
                    {
                        
                        int? workoutId = null;
                        
                        int? remainingWorkout = null;
                        var WorkoutInfo2 = "";
                        //Setup Program
                        if (experienceSetting != null && workoutPlaceSetting != null)
                        {
                            if (workoutPlaceSetting.Value == "gym")
                            {

                                if (experienceSetting.Value == "less1year")
                                {
                                    WorkoutInfo2 = "[Gym] Full-Body";
                                    workoutId = 104;
                                    programId = 10;
                                    remainingWorkout = 18;
                                }
                                if (experienceSetting.Value == "1-3years")
                                {
                                    WorkoutInfo2 = "[Gym] Upper-Body";
                                    workoutId = 106;
                                    programId = 15;
                                    remainingWorkout = 32;
                                }
                                if (experienceSetting.Value == "more3years")
                                {
                                    WorkoutInfo2 = "[Gym] Upper-Body Level 2";
                                    workoutId = 424;
                                    programId = 16;
                                    remainingWorkout = 40;
                                }
                            }
                            else if (workoutPlaceSetting.Value == "home")
                            {
                                if (experienceSetting.Value == "less1year")
                                {
                                    WorkoutInfo2 = "[Home] Full-Body";
                                    workoutId = 108;
                                    programId = 17;
                                    remainingWorkout = 18;
                                }
                                if (experienceSetting.Value == "1-3years")
                                {
                                    WorkoutInfo2 = "[Home] Upper-Body";
                                    workoutId = 109;
                                    programId = 21;
                                    remainingWorkout = 24;
                                }
                                if (experienceSetting.Value == "more3years")
                                {
                                    WorkoutInfo2 = "[Home] Upper-Body Level 2";
                                    workoutId = 428;
                                    programId = 22;
                                    remainingWorkout = 40;
                                }
                            }
                            else if (workoutPlaceSetting.Value == "homeBodyweightOnly")
                            {
                                WorkoutInfo2 = "Bodyweight Level 2";
                                workoutId = 12646;
                                programId = 487;
                                remainingWorkout = 12;
                            }

                            if (experienceSetting.Value == "beginner")
                            {
                                WorkoutInfo2 = "Bodyweight Level 1";
                                workoutId = 12645;
                                programId = 488;
                                remainingWorkout = 6;
                            }

                           
                            switch (programId)
                            {
                                case 10:
                                    ProgramLabel = "[Gym] Full-Body Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Gym] Full-Body Level 6";
                                        programId = 395;
                                        WorkoutInfo2 = "[Gym] Full-Body 6A (easy)";
                                        workoutId = 2312;
                                    }
                                    else if (age > 30)
                                    {
                                        ProgramLabel = "[Gym] Up/Low Split Level 1";
                                        programId = 15;
                                        WorkoutInfo2 = "[Gym] Lower Body";
                                        workoutId = 107;
                                    }
                                    break;
                                case 15:
                                    ProgramLabel = "[Gym] Up/Low Split Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Gym] Up/Low Split Level 6";
                                        programId = 401;
                                        WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                                        workoutId = 2337;
                                    }
                                    break;
                                case 16:
                                    ProgramLabel = "[Gym] Up/Low Split Level 2";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Gym] Up/Low Split Level 6";
                                        programId = 401;
                                        WorkoutInfo2 = "[Gym] Lower Body 6A (easy)";
                                        workoutId = 2337;
                                    }
                                    break;
                                case 17:
                                    ProgramLabel = "[Home] Full-Body Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Home] Full-Body Level 6";
                                        programId = 398;
                                        WorkoutInfo2 = "[Home] Full-Body 6A (easy)";
                                        workoutId = 2325;
                                    }
                                    else if (age > 30)
                                    {
                                        ProgramLabel = "[Home] Up/Low Split Level 1";
                                        programId = 21;
                                        WorkoutInfo2 = "[Home] Lower Body";
                                        workoutId = 110;
                                    }
                                    break;
                                case 21:
                                    ProgramLabel = "[Home] Up/Low Split Level 1";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Home] Up/Low Split Level 6";
                                        programId = 404;
                                        WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                                        workoutId = 2361;
                                    }
                                    break;
                                case 22:
                                    ProgramLabel = "[Home] Up/Low Split Level 2";
                                    if (age > 50)
                                    {
                                        ProgramLabel = "[Home] Up/Low Split Level 6";
                                        programId = 404;
                                        WorkoutInfo2 = "[Home] Lower Body 6A (easy)";
                                        workoutId = 2361;
                                    }
                                    break;
                                case 487:
                                    ProgramLabel = "Bodyweight Level 2";
                                    break;
                                case 488:
                                    ProgramLabel = "Bodyweight Level 1";
                                    break;
                            }
                            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", workoutId.ToString());
                            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", WorkoutInfo2);
                            LocalDBManager.Instance.SetDBSetting("recommendedProgramId", programId.ToString());
                            LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", remainingWorkout.ToString());

                            LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", ProgramLabel);
                        }
                        //SignUp here
                        RegisterModel registerModel = new RegisterModel();

                        registerModel.Firstname = "";
                        registerModel.EmailAddress = "";
                        registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                            registerModel.IsQuickMode = false;
                        else
                        {
                            if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                                registerModel.IsQuickMode = null;
                            else
                                registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
                        }
                        if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                            registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                        registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                        registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                        if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";
                        registerModel.LearnMoreDetails = learnMore;
                        registerModel.IsHumanSupport = IsHumanSupport;
                        registerModel.IsCardio = IsIncludeCardio;
                        registerModel.BodyPartPrioriy = bodypartName;

                        registerModel.MainGoal = mainGoal;
                        if (IsEquipment)
                        {
                            registerModel.EquipmentModel = new EquipmentModel()
                            {
                                IsEquipmentEnabled = true,
                                IsDumbbellEnabled = isDumbbells,
                                IsPlateEnabled = IsPlates,
                                IsPullyEnabled = IsPully,
                                IsChinUpBarEnabled = IsChinupBar
                            };
                        }
                        if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null)
                        {
                            var increments = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value, System.Globalization.CultureInfo.InvariantCulture);
                            var incrementsWeight = new MultiUnityWeight(increments, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                            registerModel.Increments = incrementsWeight.Kg;
                        }
                        LocalDBManager.Instance.SetDBSetting("ReadyRegisterModel", JsonConvert.SerializeObject(registerModel));
                    }
                    catch (Exception ex)
                    {

                    }
                    await AddQuestion("Congratulations! Your custom, smart program is ready. Learn more?");
                    await AddOptions("Learn more", async (sender, esc) =>
                    {
                        await AddAnswer(((DrMuscleButton)sender).Text, false);

                        await AddQuestion($"Based on your age, experience, and preferences, I recommend you work out {weekX} times a week on this program: '{ProgramLabel}'.");
                        _firebase.LogEvent("got_program", ProgramLabel);
                        await ClearOptions();
                        await AddOptions(AppResources.GotIt, BtnGotItProgram);
                        //Day

                        if (Device.RuntimePlatform.Equals(Device.iOS))
                        {
                            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                        }
                        async void BtnGotItProgram(object sse, EventArgs evr)
                        {
                            //await AddAnswer(((DrMuscleButton)sse).Text, false);

                        //    ((DrMuscleButton)sse).Clicked -= BtnGotItProgram;
                        //    ((DrMuscleButton)sse).Clicked += BtnGotItNext;
                        //    await AddQuestion($"Your smart program:\n- Updates in real time\n- Matches your progress\n- Speeds up future progress");

                        //    if (Device.RuntimePlatform.Equals(Device.iOS))
                        //    {
                        //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                        //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                        //    }

                        //    async void BtnGotItNext(object s, EventArgs e)
                        //{
                        //    _firebase.LogEvent("gotten_program", ProgramLabel);
                        //    if (Device.RuntimePlatform.Equals(Device.Android))
                        //        await Task.Delay(300);
                                LearnMoreSkipButton_Clicked(sse, evr);
                                //await AddQuestion(AppResources.WarningIMNOtLikeOtherAppsIGuideYouInRealTimeBased, false);
                                //if (Device.RuntimePlatform.Equals(Device.iOS))
                                //{
                                //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                                //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                                //}
                                //    ((DrMuscleButton)s).Clicked -= BtnGotItNext;
                                //((DrMuscleButton)s).Clicked += btnGotIt4_Clicked;
                                //async void btnGotIt4_Clicked(object ob, EventArgs ar)
                                //{
                                //    await AddAnswer(((DrMuscleButton)ob).Text, false);
                                //    await ClearOptions();
                                //    //
                                //    await AddQuestion("This app uses the latest science, but it can't correct your form, or allow for a medical condition. It may be wrong at times. When in doubt, trust your judgment. Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we'll treat your feedback like gold. Got a suggestion? Get in touch. We release new features every month.");
                                //    if (Device.RuntimePlatform.Equals(Device.Android))
                                //        await Task.Delay(300);
                                //    await AddOptions("View latest features", async (sand, ees) =>
                                //    {
                                //        if (Device.RuntimePlatform.Equals(Device.Android))
                                //            await Task.Delay(300);
                                //        //Device.OpenUri(new Uri("https://dr-muscle.com/timeline"));
                                //        await Browser.OpenAsync("https://dr-muscle.com/timeline", BrowserLaunchMode.SystemPreferred);
                                //        LearnMoreSkipButton_Clicked(sand, ees);
                                //    });
                                //    await AddOptions("Continue", async (sende, eee) =>
                                //    {
                                //        if (Device.RuntimePlatform.Equals(Device.Android))
                                //            await Task.Delay(300);
                                //        LearnMoreSkipButton_Clicked(sende, eee);
                                //    });
                                //    stackOptions.Children.Add(TermsConditionStack);
                                //    TermsConditionStack.IsVisible = true;
                                //}
                            }
                        //}
                    });
                    await AddOptions("Skip", async (sender, esc) => {
                        //await AddAnswer(((DrMuscleButton)sender).Text, false);
                        _firebase.LogEvent("got_program", ProgramLabel);

                        LearnMoreSkipButton_Clicked(sender, esc);
                    });

                }
            }
            catch (Exception ex)
            {

            }

        }

        async void LearnMoreSkipButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            await AddAnswer(((DrMuscleButton)sender).Text);
            await ClearOptions();
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            learnMore.Age = age;

            if (age > 50)
                learnMore.AgeDesc = $"Recovery is slower at {age}.";
            else if (age > 30)
                learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I recommend you train each muscle 2x a week (instead of 3x a week).";
            else
                learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";



            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value == "Woman";
            if (!string.IsNullOrEmpty(focusText))
            {
                focusText = focusText.Replace("\nStronger sex drive", "");
                focusText = focusText.Replace("Stronger sex drive", "");
            }
            if (string.IsNullOrEmpty(focusText))
                focusText = "Better health";

            if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscle")
            {
                learnMore.Focus = focusText.Replace("\n", ", ").ToLower();
                if (learnMore.Focus.Contains(","))
                {
                    int ind = learnMore.Focus.LastIndexOf(",");
                    var subStr = learnMore.Focus.Substring(ind);
                    var newStr = subStr.Replace(",", " and");
                    learnMore.Focus = learnMore.Focus.Replace(subStr, newStr);
                }

                learnMore.FocusDesc = IsWoman ? "To get stronger, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range." : "To build muscle, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "BuildMuscleBurnFat")
            {
                learnMore.Focus = focusText.Replace("\n", ", ").ToLower();
                if (learnMore.Focus.Contains(","))
                {
                    int ind = learnMore.Focus.LastIndexOf(",");
                    var subStr = learnMore.Focus.Substring(ind);
                    var newStr = subStr.Replace(",", " and");
                    learnMore.Focus = learnMore.Focus.Replace(subStr, newStr);
                }
                learnMore.FocusDesc = IsWoman ? "For overall fitness, I recommend you repeat each exercise 8-15 times." : "To build muscle and burn fat, I recommend you repeat each exercise 8-15 times.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("reprange").Value == "FatBurning")
            {
                learnMore.Focus = focusText.Replace("\n", ", ").ToLower();
                if (learnMore.Focus.Contains(","))
                {
                    int ind = learnMore.Focus.LastIndexOf(",");
                    var subStr = learnMore.Focus.Substring(ind);
                    var newStr = subStr.Replace(",", " and");
                    learnMore.Focus = learnMore.Focus.Replace(subStr, newStr);
                }
                learnMore.FocusDesc = "To burn fat, I recommend you repeat each exercise 12-20 times. You will burn more calories by lifting in that range.";
            }
            LocalDBManager.Instance.SetDBSetting("DBFocus", learnMore.Focus);

            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            learnMore.Exp = "";
            learnMore.ExpDesc = "";
            if (experienceSetting != null)
            {
                if (experienceSetting.Value == "less1year")
                {
                    learnMore.Exp = $"Less than 1 year";
                    learnMore.ExpDesc = "You're new to lifting, so I recommend you train each muscle 3x a week on a full-body program. You will progress faster that way.";
                }
                if (experienceSetting.Value == "1-3years")
                {
                    learnMore.Exp = $"1-3 years";
                    learnMore.ExpDesc = "You have been lifting for over a year, so I recommend you train each muscle 2x a week on a split-body program. This gives you more time to recover between working out each muscle.";
                }
                if (experienceSetting.Value == "more3years")
                {
                    learnMore.Exp = $"More than 3 years";
                    learnMore.ExpDesc = "You have been lifting for 3+ years, so I recommend you train each muscle 2x a week on a split-body program with A and B days. This gives you more time to recover between working out each muscle and more exercise variation. At your level, it's important. ";
                }

            }

            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            await ClearOptions();



            //BotList.Add(new BotModel()
            //{
            //    Question = "Sign in to:\n- Save your program\n- Work out from any device\n- Never lose your history",
            //    Type = BotType.Ques
            //});
            //await Task.Delay(300);
            //Device.BeginInvokeOnMainThread(() =>
            //{
            //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            //});
            //SignupCode here:
            //await Task.Delay(1000);
            //SetMenu();
            if (LocalDBManager.Instance.GetDBSetting("LoginType") != null && LocalDBManager.Instance.GetDBSetting("LoginType").Value == "Social")
                GoogleFbLoginAfterDemo();
            else
                CreateAccountAfterDemoButton_Clicked();
        }

        async void SetMenu()
        {
            stackOptions.Children.Add(StackSignupMenu);
            BottomViewHeight.Height = GridLength.Auto;
            StackSignupMenu.IsVisible = true;
            if (Device.RuntimePlatform.Equals(Device.iOS))
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            }
            stackOptions.Children.Add(TermsConditionStack);
            TermsConditionStack.IsVisible = true;
        }
        async void GotItButton_Clicked(object sender, EventArgs e)
        {
            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            await ClearOptions();
            await Task.Delay(300);
            GetEmail();
        }

        async void ConnectWithEmail(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Message = AppResources.PleaseCheckInternetConnection,
                            //    Title = AppResources.ConnectionError
                            //});
                return;
            }
            await ClearOptions();
            //GetFirstName();
            GetEmail();
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

        async Task AddAnswer(string answer, bool isClearOptions = true)
        {
            BotList.Add(new BotModel()
            {
                Answer = answer,
                Type = BotType.Ans
            });
            if (isClearOptions)
                await ClearOptions();
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            await Task.Delay(300);
        }

        async Task<CustomImageButton> AddCheckbox(string title, EventHandler handler, bool ischecked = false)
        {
            CustomImageButton imgBtn = new CustomImageButton()
            {
                Text = title,
                Source = ischecked ? "done.png" : "Undone.png",
                BackgroundColor = Color.White,
                TextFontColor = AppThemeConstants.OffBlackColor,
                Margin = new Thickness(25, 1),
                Padding = new Thickness(2)
            };
            imgBtn.Clicked += handler;
            stackOptions.Children.Add(imgBtn);
            return imgBtn;
        }

        async Task<DrMuscleButton> AddOptions(string title, EventHandler handler)
        {
            var grid = new Grid();
            var pancakeView = new PancakeView() {  HeightRequest = 50, Margin = new Thickness(25, 5) };
            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#5CD196"), Offset = 1 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#40A076"), Offset = 0 });

            grid.Children.Add(pancakeView);


            var btn = new DrMuscleButton()
            {
                Text = title,
                TextColor = Color.Black,
                BackgroundColor = Color.White,
                FontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 17
            };
            btn.Clicked += handler;
            SetDefaultButtonStyle(btn);
            grid.Children.Add(btn);
            stackOptions.Children.Add(grid);

            BottomViewHeight.Height = GridLength.Auto;
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            return btn;
        }

        public async void LoginWithAppleAsync(object sender, EventArgs ee)
        {
            var account = await appleSignInService.SignInAsync();
            if (account != null)
            {
                if (!string.IsNullOrEmpty(account.Email))
                {
                    await SecureStorage.SetAsync("Email", account.Email);
                    if (!string.IsNullOrEmpty(account.Name))
                        await SecureStorage.SetAsync("Name", account.Name);
                    else if (!string.IsNullOrEmpty(account.GivenName))
                        await SecureStorage.SetAsync("Name", account.GivenName);
                    else if (!string.IsNullOrEmpty(account.FamilyName))
                        await SecureStorage.SetAsync("Name", account.FamilyName);
                    else
                        await SecureStorage.SetAsync("Name", "  ");
                }
                else
                {
                    string email = await SecureStorage.GetAsync("Email");
                    string name = await SecureStorage.GetAsync("Name");
                    account.Email = email;
                    account.Name = name;
                }
                if (string.IsNullOrEmpty(account.Email))
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = "We haven't get email. Please login with email.",
                        Title = AppResources.Error
                    });
                    
                    return;
                }
                OnLoginCompleted(null, new GoogleClientResultEventArgs<GoogleUser>(new GoogleUser() { Email = account.Email, Name = account.Name }, GoogleActionStatus.Completed));
            }
            
        }
        //Google Login
        public async void LoginWithGoogleAsync(object sender, EventArgs ee)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Message = AppResources.PleaseCheckInternetConnection,
                            //    Title = AppResources.ConnectionError
                            //});
                return;
            }
            _googleClientManager.OnLogin += OnLoginCompleted;
            try
            {
                await _googleClientManager.LoginAsync();
            }
            catch (GoogleClientSignInNetworkErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInCanceledErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInInvalidAccountErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInInternalErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientNotInitializedErrorException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientBaseException e)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = e.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }

        }


        private async void OnLoginCompleted(object sender, GoogleClientResultEventArgs<GoogleUser> loginEventArgs)
        {
            _googleClientManager.OnLogin -= OnLoginCompleted;
            if (loginEventArgs.Data != null)
            {
                GoogleUser googleUser = loginEventArgs.Data;
                UserProfile user = new UserProfile();
                user.Name = googleUser.Name;
                user.Email = googleUser.Email;
                if (user.Picture != null)
                    user.Picture = googleUser.Picture;
                //var token = CrossGoogleClient.Current.ActiveToken;
                LocalDBManager.Instance.SetDBSetting("LoginType", "Social");
                LocalDBManager.Instance.SetDBSetting("GToken", "");
                if (user.Picture != null)
                    LocalDBManager.Instance.SetDBSetting("ProfilePic", user.Picture.OriginalString);

                //IsLoggedIn = true;
                bool IsExistingUser = false;
                BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = user.Email });
                if (existingUser != null)
                {
                    if (existingUser.Result)
                    {

                        ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
                        {
                            Title = "You are already registered",
                            Message = "Use another account or log into your existing account.",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = "Use another account",
                            CancelText = AppResources.LogIn,

                        };
                        var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
                        if (actionOk)
                        {
                            return;
                        }
                        else
                        {
                            IsExistingUser = true;
                            //((App)Application.Current).displayCreateNewAccount = true;
                            //await PagesFactory.PushAsync<WelcomePage>();
                        }

                        //return;
                    }

                }


                string mass = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                string body = null;
                if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                    body = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), mass).Kg.ToString();

                LoginSuccessResult lr = await DrMuscleRestClient.Instance.GoogleLogin("", user.Email, user.Name, body, mass);
                if (lr != null)
                {
                    UserInfosModel uim = null;
                    if (existingUser.Result)
                    {
                        uim = await DrMuscleRestClient.Instance.GetUserInfo();
                    }
                    else
                    {
                        //RegisterModel registerModel = new RegisterModel();
                        //registerModel.Firstname = user.Name;
                        //registerModel.EmailAddress = user.Email;
                        //registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                        //registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        //if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                        //    registerModel.IsQuickMode = false;
                        //else
                        //{
                        //    if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                        //        registerModel.IsQuickMode = null;
                        //    else
                        //        registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
                        //}
                        //if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                        //    registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                        //registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                        //registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                        //registerModel.Password = "";
                        //registerModel.ConfirmPassword = "";
                        //registerModel.LearnMoreDetails = learnMore;
                        //registerModel.IsHumanSupport = IsHumanSupport;
                        //registerModel.IsCardio = IsIncludeCardio;
                        //registerModel.BodyPartPrioriy = bodypartName;

                        //registerModel.MainGoal = mainGoal;
                        //if (IsEquipment)
                        //{
                        //    registerModel.EquipmentModel = new EquipmentModel()
                        //    {
                        //        IsEquipmentEnabled = true,
                        //        IsDumbbellEnabled = isDumbbells,
                        //        IsPlateEnabled = IsPlates,
                        //        IsPullyEnabled = IsPully,
                        //        IsChinUpBarEnabled = IsChinupBar,

                        //    };
                        //}
                        //if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                        //    registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        RegisterModel registerModel = new RegisterModel();
                        registerModel.Firstname = user.Name;
                        registerModel.EmailAddress = user.Email;

                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";
                        if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        uim = await DrMuscleRestClient.Instance.RegisterWithUser(registerModel);
                        
                    }
                    try
                    {
                        LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                        LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                        LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                        LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                        LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                        LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                        LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                        LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                        LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                        LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                        LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                        if (uim.Age != null)
                            LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                        //if (uim.ReminderTime != null)
                        //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                        //if (uim.ReminderDays != null)
                        //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

                        LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                        LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                        if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                        {
                            LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                            LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                        }
                        else
                        {
                            LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                            LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
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
                        if (uim.WarmupsValue != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                        }

                        if (uim.EquipmentModel != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                        }
                        else
                        {
                            LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                            LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                            LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                            LocalDBManager.Instance.SetDBSetting("Plate", "true");
                            LocalDBManager.Instance.SetDBSetting("Pully", "true");
                        }
                        if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                            LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                        else
                            LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                        ((App)Application.Current).displayCreateNewAccount = true;

                        if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                        else
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");

                        //await PagesFactory.PushAsync<MainAIPage>();

                        //App.IsWelcomeBack = true;
                        //App.IsDemoProgress = false;
                        //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                        if (IsExistingUser)
                        {
                            App.IsDemoProgress = false;
                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            await PagesFactory.PopToRootAsync(true);
                            return;
                        }

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
                        PagesFactory.PushAsync<DemoPage>();
                       // CancelNotification();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.EmailAndPasswordDoNotMatch,
                        Title = AppResources.UnableToLogIn,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                }
            }
            else
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = loginEventArgs.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                
            }

            _googleClientManager.OnLogin -= OnLoginCompleted;

        }


        private async void GoogleFbLoginAfterDemo()
        {
            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            RegisterModel registerModel = new RegisterModel();
            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                registerModel.IsQuickMode = false;
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                    registerModel.IsQuickMode = null;
                else
                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            }
            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            registerModel.Password = "";
            registerModel.ConfirmPassword = "";
            registerModel.LearnMoreDetails = learnMore;
            registerModel.IsHumanSupport = IsHumanSupport;
            registerModel.IsCardio = IsIncludeCardio;
            registerModel.BodyPartPrioriy = bodypartName;

            registerModel.MainGoal = mainGoal;
            if (IsEquipment)
            {
                registerModel.EquipmentModel = new EquipmentModel()
                {
                    IsEquipmentEnabled = true,
                    IsDumbbellEnabled = isDumbbells,
                    IsPlateEnabled = IsPlates,
                    IsPullyEnabled = IsPully,
                    IsChinUpBarEnabled = IsChinupBar
                };
            }
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            var uim = await DrMuscleRestClient.Instance.RegisterWithUser(registerModel);
            try
            {
                CancelNotification();
                SetTrialUserNotifications();
                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                if (uim.Age != null)
                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                //if (uim.ReminderTime != null)
                //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                //if (uim.ReminderDays != null)
                //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
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
                if (uim.WarmupsValue != null)
                {
                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                }

                if (uim.EquipmentModel != null)
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                    LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                    LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                    LocalDBManager.Instance.SetDBSetting("Plate", "true");
                    LocalDBManager.Instance.SetDBSetting("Pully", "true");
                }
                    ((App)Application.Current).displayCreateNewAccount = true;
                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());
                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                else
                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");




                //await PagesFactory.PopToRootAsync(true);

                //await PagesFactory.PushAsync<MainAIPage>();

                //App.IsDemoProgress = false;
                //LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                //App.IsWelcomeBack = true;
                //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                long pId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
                var upi = new GetUserProgramInfoResponseModel()
                {
                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = pId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
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
                            App.IsNewUser = true;
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
                            App.IsNewUser = true;
                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            CurrentLog.Instance.Exercise1RM.Clear();
                            await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                            await PagesFactory.PopToRootMoveAsync(true);
                        }

                    }
                    else
                    {
                        await PagesFactory.PopToRootAsync(true);
                        App.IsDemoProgress = false;
                        App.IsWelcomeBack = true;
                        App.IsNewUser = true;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");

                        await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                    }
                }


            }
            catch (Exception ex)
            {

            }
        }

        public void Logout()
        {
            _googleClientManager.OnLogout += OnLogoutCompleted;
            _googleClientManager.Logout();
        }

        private void OnLogoutCompleted(object sender, EventArgs loginEventArgs)
        {

        }

        //Facebook Login
        private async void LoginWithFBButton_Clicked(object sender, EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            //{
                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            //    Message = AppResources.PleaseCheckInternetConnection,
                            //    Title = AppResources.ConnectionError
                            //});
                return;
            }
            FacebookUser result = await _manager.Login();
            if (result == null)
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = AppResources.AnErrorOccursWhenSigningIn,
                    Title = AppResources.UnableToLogIn,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                await WelcomePage_OnFBLoginSucceded(result.Id, result.Email, "", result.Token, result.FirstName);
            });
        }

        private async Task WelcomePage_OnFBLoginSucceded(string FBId, string FBEmail, string FBGender, string FBToken, string firstname)
        {
            if (string.IsNullOrEmpty(FBEmail))
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = "Your Facebook account is not connected with email (or we do not have permission to access it). Please sign up with email.",
                    Title = AppResources.Error
                });
                
                return;
            }
            LocalDBManager.Instance.SetDBSetting("LoginType", "Social");
            LocalDBManager.Instance.SetDBSetting("FBId", FBId);
            LocalDBManager.Instance.SetDBSetting("FBEmail", FBEmail);
            LocalDBManager.Instance.SetDBSetting("FBGender", FBGender);
            LocalDBManager.Instance.SetDBSetting("FBToken", FBToken);
            var url = $"http://graph.facebook.com/{FBId}/picture?type=square";
            LocalDBManager.Instance.SetDBSetting("ProfilePic", url);



            BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = FBEmail });
            bool IsExistingUser = false;
            if (existingUser != null)
            {
                if (existingUser.Result)
                {

                    ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
                    {
                        Title = "You are already registered",
                        Message = "Use another account or log into your existing account.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Use another account",
                        CancelText = AppResources.LogIn,

                    };
                    var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
                    if (actionOk)
                    {
                        return;
                    }
                    else
                    {
                        //((App)Application.Current).displayCreateNewAccount = true;
                        //await PagesFactory.PushAsync<WelcomePage>();
                        IsExistingUser = true;
                    }

                    //return;
                }

            }
            //Log in d'un compte existant avec Facebook
            string mass = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            string body = null;
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                body = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg").Kg.ToString();
            try
            {


                LoginSuccessResult lr = await DrMuscleRestClient.Instance.FacebookLogin(FBToken, body, mass);
                if (lr != null)
                {
                    DateTime current = DateTime.Now;
                    UserInfosModel uim = null;
                    if (existingUser.Result)
                    {
                        uim = await DrMuscleRestClient.Instance.GetUserInfo();
                    }
                    else
                    {
                        //RegisterModel registerModel = new RegisterModel();
                        //registerModel.Firstname = firstname;
                        //registerModel.EmailAddress = FBEmail;
                        //registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                        //registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        //if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
                        //    registerModel.IsQuickMode = false;
                        //else
                        //{
                        //    if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
                        //        registerModel.IsQuickMode = null;
                        //    else
                        //        registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
                        //}
                        //if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                        //    registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                        //registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                        //registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                        //if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                        //    registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        //registerModel.Password = "";
                        //registerModel.ConfirmPassword = "";
                        //registerModel.LearnMoreDetails = learnMore;
                        //registerModel.IsHumanSupport = IsHumanSupport;
                        //registerModel.IsCardio = IsIncludeCardio;
                        //registerModel.BodyPartPrioriy = bodypartName;

                        //registerModel.MainGoal = mainGoal;
                        //if (IsEquipment)
                        //{
                        //    registerModel.EquipmentModel = new EquipmentModel()
                        //    {
                        //        IsEquipmentEnabled = true,
                        //        IsDumbbellEnabled = isDumbbells,
                        //        IsPlateEnabled = IsPlates,
                        //        IsPullyEnabled = IsPully,
                        //        IsChinUpBarEnabled = IsChinupBar
                        //    };
                        //}
                        RegisterModel registerModel = new RegisterModel();
                        registerModel.Firstname = firstname;
                        registerModel.EmailAddress = FBEmail;

                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;

                        if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                            registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
                        if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";

                        uim = await DrMuscleRestClient.Instance.RegisterWithUser(registerModel);
                    }
                    LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                    LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                    LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                    LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                    LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                    LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                    LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                    LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                    LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                    LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                    LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                    LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                    LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                    LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                    if (uim.Age != null)
                        LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                    //if (uim.ReminderTime != null)
                    //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                    //if (uim.ReminderDays != null)
                    //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

                    LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                    LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                    LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                    LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                    if (uim.IsNormalSet == null || uim.IsNormalSet == true)
                    {
                        LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
                        LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                        LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
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
                    if (uim.WarmupsValue != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                    }
                    if (uim.EquipmentModel != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                    }
                    else
                    {
                        LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                        LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                        LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                        LocalDBManager.Instance.SetDBSetting("Plate", "true");
                        LocalDBManager.Instance.SetDBSetting("Pully", "true");
                    }
                    ((App)Application.Current).displayCreateNewAccount = true;

                    if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                        LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                    else
                        LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

                    //await PagesFactory.PopToRootAsync(true);
                    //await PagesFactory.PushAsync<MainAIPage>();
                    //    App.IsWelcomeBack = true;
                    //    App.IsDemoProgress = false;
                    //LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                    //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                    if (IsExistingUser)
                    {
                        App.IsDemoProgress = false;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                        await PagesFactory.PopToRootAsync(true);
                        return;
                    }
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
                    PagesFactory.PushAsync<DemoPage>();
                }
                else
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.EmailAndPasswordDoNotMatch,
                        Title = AppResources.UnableToLogIn,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                }
            }
            catch (Exception ex)
            {
                
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = "We are facing problem to signup with your facebook account. Please sign up with email.",
                    Title = AppResources.Error
                });

            }
        }

    }
}
