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
using DrMuscle.Message;

namespace DrMuscle.Screens.User.OnBoarding
{
    public partial class ReconfigureBoardingPage : DrMusclePage
    {
        public ObservableCollection<BotModel> BotList = new ObservableCollection<BotModel>();
        public LearnMore learnMore = new LearnMore();
        bool ManMoreMuscle = false;
        bool ManLessFat = false;
        bool ManBetterHealth = false;
        bool ManStorngerSexDrive = false;

        bool FemaleMoreEnergy = false;
        bool FemaleToned = false;
        bool IsHumanSupport = false;
        bool ShouldAnimate = false;
        bool IsBodyweightPopup = false;
        bool IsIncludeCardio = false;
        bool? SetStyle = false;
        string focusText = "", mainGoal = "";
        private IFirebase _firebase;
        Picker AgePicker;
        Picker BodyweightPicker;
        Picker LevelPicker;
        Picker BodyPartPicker;
        public static bool IsMovedToLogin = false;
        MultiUnityWeight IncrementUnit = null;
        bool IsEquipment = false;
        bool IsPully = false;
        bool isDumbbells = false;
        bool IsPlates = false;
        bool IsChinupBar = false;
        bool isProcessing = false;
        bool IsPyramid = false;
        string bodypartName = "";
        CustomImageButton bodypart1, bodypart2, bodypart3, bodypartBalanced;
        public ReconfigureBoardingPage()
        {
            InitializeComponent();

            lstChats.ItemsSource = BotList;
            NavigationPage.SetHasBackButton(this, false);
            Title = AppResources.DrMuslce;
            _firebase = DependencyService.Get<IFirebase>();
            this.ToolbarItems.Clear();
            var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralBotAction, ToolbarItemOrder.Primary, 0);
            this.ToolbarItems.Add(generalToolbarItem);
            LocalDBManager.Instance.SetDBSetting("RBackgroundImage", "DrMuscleLogo.png");
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
            
            LoginButton.HeightRequest = 170;
            BtnAppleSignIn.IsVisible = false;
            // BtnAppleSignIn2.IsVisible = false;

            
        }

        private async void BodyWeightMassUnitMessage(string bodyWeight)
        {
            try
            {

                LocalDBManager.Instance.SetDBSetting("RBodyWeight", new MultiUnityWeight(Convert.ToDecimal(bodyWeight, CultureInfo.InvariantCulture), LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().ReplaceWithDot());
                await AddAnswer(bodyWeight);


                //await AddQuestion("Are you a man or a woman?");
                //await Task.Delay(300);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                //SetupGender();

                //BotList.Add(new BotModel()
                //{
                //    Question = "Sign in to:\n- Save your program\n- Work out from any device\n- Never lose your history",
                //    Type = BotType.Ques
                //});
                //await Task.Delay(1500);
                //Device.BeginInvokeOnMainThread(() =>
                //{
                //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                //});
                //SignupCode here:

                SetupMainGoal();
                //SetMenu();

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
                LocalDBManager.Instance.SetDBSetting("RBodyWeight", Convert.ToString(age));
                await AddAnswer(Convert.ToString(age));

                if (LocalDBManager.Instance.GetDBSetting("RExLevel").Value == "Exp")
                {
                    LocalDBManager.Instance.SetDBSetting("Rworkout_place", "gym");
                    LocalDBManager.Instance.SetDBSetting("Rexperience", "more3years");

                    if (LocalDBManager.Instance.GetDBSetting("Rexperience").Value == "beginner")
                        AddCardio();// SetupQuickMode();
                    else
                        workoutPlace();
                    return;
                }
                LocalDBManager.Instance.SetDBSetting("Rexperience", "less1year");
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

        private async void LevelPicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                if (LevelPicker.SelectedIndex == -1)
                    LevelPicker.SelectedIndex = 0;
                int level = LevelPicker.SelectedIndex + 1;
                LocalDBManager.Instance.SetDBSetting("RMainLevel", level.ToString());
                LocalDBManager.Instance.SetDBSetting("RCustomMainLevel", level.ToString());
                await AddAnswer($"Level {level}");
                //SetupMassUnit();
                if (LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "returning from a break")
                {
                    AskSetStyle();
                }
                else
                {
                    AskSetStyle();
                }
            }
            catch (Exception ex)
            {

            }
        }


        private async void AgePicker_Unfocused(object sender, FocusEventArgs e)
        {
            try
            {
                int age = Convert.ToInt32(AgePicker.SelectedItem, CultureInfo.InvariantCulture);
                LocalDBManager.Instance.SetDBSetting("RAge", Convert.ToString(age));
                //        await AddAnswer(Convert.ToString(age));

                await AddAnswer($"{age}");


                //[Choose schedule][medium emphasis button][insert their frequency] a week[high emphasis button]
                var gender = LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd() == "Man" ? "Men" : "Women";
                var xDays = 0;
                if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Split"))
                {
                    if (age < 30)
                        xDays = 5;
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
                await AddQuestion($"{gender} aged {age} like you usually progress the fastest on that program when they work out {xDays} times a week.");

                //if (LocalDBManager.Instance.GetDBSetting("Rexperience").Value == "beginner")
                //    AddCardio();// SetupQuickMode();
                //else
                //    workoutPlace();

                //await AddOptions("Choose schedule", (o, ev) => {
                //    AddAnswer("Choose schedule");
                //    ShowWorkoutReminder();
                //});

                var btn1 = new DrMuscleButton()
                {
                    Text = "Another schedule",
                    TextColor = Color.FromHex("#195377"),
                    BackgroundColor = Color.Transparent,
                    HeightRequest = 55,
                    BorderWidth = 2,
                    BorderColor = AppThemeConstants.BlueColor,
                    Margin = new Thickness(25, 2),
                    CornerRadius=0,
                };
                btn1.Clicked += (o, ev) => {
                    AddAnswer("Another schedule");
                    ShowWorkoutReminder();
                };
                stackOptions.Children.Add(btn1);
                await AddOptions($"Recommended {xDays}x/week", (o, ev) => {
                    AddAnswer($"Recommended {xDays}x/week");
                    ShowWorkoutReminder();
                });

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
                
                CurrentLog.Instance.IsMovingOnBording = false;
                this.ToolbarItems.Clear();
                var generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralBotConfigureAction, ToolbarItemOrder.Primary, 0);
                this.ToolbarItems.Add(generalToolbarItem);
                IsMovedToLogin = false;
                List<string> age = new List<string>();
                List<string> bodyweight = new List<string>();
                for (int i = 10; i < 125; i++)
                {
                    age.Add($"{i}");
                }
                List<string> level = new List<string>();
                for (int i = 1; i < 7; i++)
                {
                    level.Add($"Level {i}");
                }

                if (AgePicker != null)
                    AgePicker.SelectedIndexChanged -= AgePicker_SelectedIndexChanged;

                AgePicker = new Picker()
                {
                    Title = "Age?"
                };
                AgePicker.ItemsSource = age;
                AgePicker.SelectedItem = "35";
                AgePicker.Unfocused += AgePicker_Unfocused;
                AgePicker.SelectedIndexChanged += AgePicker_SelectedIndexChanged;


                if (LevelPicker != null)
                    LevelPicker.Unfocused -= LevelPicker_Unfocused;

                LevelPicker = new Picker()
                {
                    Title = "Select level"
                };
                LevelPicker.ItemsSource = level;
                LevelPicker.SelectedIndex = 0;
                LevelPicker.Unfocused += LevelPicker_Unfocused;


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
                MainGrid.Children.Insert(0, LevelPicker);
                
                    StartSetup();
                

            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
        }

        protected override bool OnBackButtonPressed()
        {

            
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
        bool Isrestarted = false;
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
                SetStyle = false;
                IncrementUnit = null;
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


                //await Task.Delay(2500);
                //if (BotList.Count == 1)
                //{
                //    BotList.Add(new BotModel()
                //    {
                //        Question = "Why trust me? I've been a coach for 17 years and a trainer for the Canadian Forces.",
                //        Type = BotType.Ques
                //    });
                //}
                //else
                //    return;
                //if (BotList.Count == 1 || BotList.Count > 2)
                //    return;

                //await Task.Delay(2500);
                //if (BotList.Count == 2)
                //{
                //    BotList.Add(new BotModel()
                //    {
                //        Question = $"",
                //        Answer = "",
                //        Type = BotType.Photo
                //    });
                //}
                //else
                //    return;

                await ClearOptions();

                await Task.Delay(2500);

                if (IsMovedToLogin)
                    return;
                if (BotList.Count == 1)
                {
                    BotList.Add(new BotModel()
                    {
                        Question = "Are you...",
                        Type = BotType.Ques
                    });
                }
                else
                    return;

                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                await Task.Delay(1000);
                if (BotList.Count < 2)
                    return;
                await ClearOptions();
                var btn = await AddOptions("New to lifting weights", async (ss, ee) =>
                {
                ShouldAnimate = false;
                //_firebase.LogEvent("start_onboarding", "new_to_training");
                //_firebase.LogEvent("new_to_training", "");
                LocalDBManager.Instance.SetDBSetting("RCustomExperience", "new to training");
                await AddAnswer("New to lifting weights");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);

                //await AddQuestion("Congrats! Getting started is the hardest part, and this app makes it easier. New users like you get 34% stronger in 30 days. Start demo to see how.");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                BotList.Add(new BotModel()
                {
                    Part1 = "User reviews",
                    Part2 = "\"When I first trialed the app, I wasn't sure I'd like it. But the AI is great and makes it very easy for me to know how many reps to do and how much weight to lift. No more guessing. This really is something different.\"",
                    Part3 = "MKJ&MKJ",
                    Type = BotType.Review
                });
                if (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value != "")
                    LocalDBManager.Instance.SetDBSetting("Rexperience", "");
                    await ClearOptions();
                    LocalDBManager.Instance.SetDBSetting("RExLevel", "New");
                    LocalDBManager.Instance.SetDBSetting("RNewLevel", "All");
                    //BotList.Add(new BotModel()
                    //{
                    //    Part1 = "User reviews",
                    //    Part2 = "When I first trialed the app, I wasn’t sure I’d like it, but after having stuck with it for a couple of months, I’m sold. The AI is great and makes it very easy for me to know how many reps to do and how much weight to lift. No more guessing. This really is something different.",
                    //    Part3 = "MKJ&MKJ",
                    //    Type = BotType.Review
                    //});
                    //await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);
                    await Task.Delay(1000);
                    Features_Clicked();

                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });
                var btn1 = await AddOptions("Returning after a break", async (ss, ee) =>
                {
                    ShouldAnimate = false;
                    LocalDBManager.Instance.SetDBSetting("RCustomExperience", "returning from a break");
                    //_firebase.LogEvent("start_onboarding", "returning_after_a_break");
                    //_firebase.LogEvent("returning_after_a_break", "");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddAnswer("Returning after a break");
                    if (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value != "")
                        LocalDBManager.Instance.SetDBSetting("Rexperience", "");
                    await Task.Delay(300);
                    //await AddQuestion("Congrats on your return! New users like you get 34% stronger in 30 days. Start demo to see why.");
                    BotList.Add(new BotModel()
                    {
                        Part1 = "User reviews",
                        Part2 = "\"I have been in and out of the gym for a few years with some light progress and modest gains, however, this app helped me gain 10 lbs and become significantly more defined. Very easy to use.\"",
                        Part3 = "Potero2122",
                        Type = BotType.ReviewTestimonial
                    });
                    LocalDBManager.Instance.SetDBSetting("RExLevel", "Return");
                    ClearOptions();
                    // await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);

                    //BotList.Add(new BotModel()
                    //{
                    //    Part1 = "User reviews",
                    //    Part2 = "I have been in and out of the gym for a few years with some light progress every time and modest gains, however, the implementation of this app helped me gain 10 lbs and become significantly more defined in the first 6 weeks. Very easy to use.",
                    //    Part3 = "Potero2122",
                    //    Type = BotType.Review
                    //});
                    await Task.Delay(1000);
                    Features_Clicked();
                });
                Device.BeginInvokeOnMainThread(async () =>
                {
                    Grid grid = (Xamarin.Forms.Grid)btn1.Parent;
                    ShouldAnimate = true;
                    animate(grid);

                });

                var btn2 = await AddOptions("Already lifting weights", async (ss, ee) =>
                {
                    ShouldAnimate = false;
                    LocalDBManager.Instance.SetDBSetting("RCustomExperience", "an active, experienced lifter");
                    //_firebase.LogEvent("start_onboarding", "Active_experienced_lifter");
                    //_firebase.LogEvent("Active_experienced_lifter", "");
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        await Task.Delay(300);
                    await AddAnswer("Already lifting weights");
                        await Task.Delay(300);
                    //await AddQuestion("Nice! This app can help you break through plateaus and reach your genetic potential.");
                    ClearOptions();
                    BotList.Add(new BotModel()
                    {
                        Part1 = "User reviews",
                        Part2 = "\"For basic strength training this app out performs the many methods/app I have tried in my 30+ years of body/strength training. What I like the most is that it take the brain work out of weights, reps, and sets.\"",
                        Part3 = "TijFamily916",
                        Type = BotType.ReviewTestimonial
                    });
                    LocalDBManager.Instance.SetDBSetting("RExLevel", "Exp");
                    //await AddOptions(AppResources.GotIt, GotItAfterExperienceLevel);
                    //BotList.Add(new BotModel()
                    //{
                    //    Part1 = "User reviews",
                    //    Part2 = "For basic strength training this app out performs the many methods/apps I have tried in my 30+ years of body/strength training. What I like the most is that it take the brain work out of weights, reps, and sets (if you follow a structured workout).",
                    //    Part3 = "TijFamily916",
                    //    Type = BotType.Review
                    //});
                    await Task.Delay(1000);
                    Features_Clicked();
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

        private async void Features_Clicked()
        {
            ClearOptions();
            SetUpRestOnboarding();
            //var btn = await AddOptions("Start demo", GotItAfterExperienceLevel);
            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    Grid grid = (Xamarin.Forms.Grid)btn.Parent;
            //    ShouldAnimate = true;
            //    animate(grid);

            //});
            //stackOptions.Children.Add(TermsConditionStack);
            //TermsConditionStack.IsVisible = true;
        }
        private async void SetUpRestOnboarding()
        {
            ClearOptions();
            
            //BotList.Add(new BotModel()
            //{
            //    Question = "Congrats—you smashed the demo!",
            //    Type = BotType.Ques
            //});
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            //await Task.Delay(1000);

            if (LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "returning from a break")
            {
                //Since you are [new to training / returning after a break], I recommend you train your full body 2-4 times a week for best results.
                var msgText = "";
                if (LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "new to training")
                    msgText = "Since you are new to training, I recommend you train your full body 2-4 times a week for best results.";
                else
                    msgText = "Since you are returning after a break, I recommend you train your full body 2-4 times a week for best results.";
                BotList.Add(new BotModel()
                {
                    Question = msgText,
                    Type = BotType.Ques
                });
                AskForProgram();
            }
            else
            {
                NoAdvancedClicked(new DrMuscleButton(), EventArgs.Empty);
            }
            //    BotList.Add(new BotModel()
            //{
            //    Question = "This app is new. Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we release new features every month and we'll treat your feedback like gold.",
            //    Type = BotType.Ques
            //});
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            await Task.Delay(2500);

            //GotItAfterImage(new DrMuscleButton(), EventArgs.Empty);

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
            await AddQuestion("Man or woman?");
            await Task.Delay(300);
            SetupGender();

        }
        void SetDefaultButtonStyle(Button btn)
        {
            btn.BackgroundColor = Color.Transparent;
            btn.BorderWidth = 0;
            btn.CornerRadius = 0;
            btn.Margin = new Thickness(25, 2, 25, 2);
            btn.FontAttributes = FontAttributes.Bold;
            btn.BorderColor = Color.Transparent;
            btn.TextColor = Color.White;
            btn.HeightRequest = 55;

        }

        void SetEmphasisButtonStyle(Button btn)
        {
            btn.TextColor = Color.White;
            btn.BackgroundColor = Color.Transparent;
            btn.Margin = new Thickness(25, 2, 25, 2);
            btn.HeightRequest = 55;
            btn.BorderWidth = 6;
            btn.CornerRadius = 0;
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


            LocalDBManager.Instance.SetDBSetting("Rgender", "Man");
            //await AddQuestion("Men often want (tap all that apply):");
            //await Task.Delay(300);

            //await AddCheckbox("More muscle", Man_MoreMuscle_Clicked);
            //await AddCheckbox("Less fat", Man_LessFat_Clicked);
            //await AddCheckbox("Better health", Man_BetterHealth_Clicked);
            //await AddCheckbox("Stronger sex drive", Man_StorngerSexDrive_Clicked);

            //await AddOptions("Continue", ManTakeActionBasedOnInput);
            //SetupMassUnit();
            //SetupMainGoal();
            await AddQuestion("Age?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
            return;

        }

        private async void SetupMainGoal()
        {

            //await Task.Delay(300);
            ManLessFat = false;
            ManBetterHealth = false;
            FemaleMoreEnergy = false;
            FemaleToned = false;
            ManMoreMuscle = false;
            ManStorngerSexDrive = false;
            


            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd() == "Woman";
            if (IsWoman)
            {

                await AddQuestion("What are your goals?");

                await AddCheckbox("Less fat", Man_LessFat_Clicked);
                await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
                await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

                await AddOptions("Continue", WomanTakeActionBasedOnInput);
            }
            else
            {
                await AddQuestion("What are your goals?");
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
                    LocalDBManager.Instance.SetDBSetting("RPopupMainGoal", mainGoal);
                }
                var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd() == "Woman";
                if (IsWoman)
                {

                    await AddQuestion("What are your goals?");

                    await AddCheckbox("Less fat", Man_LessFat_Clicked);
                    await AddCheckbox("Better health", Man_BetterHealth_Clicked);
                    await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
                    await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

                    await AddOptions("Continue", WomanTakeActionBasedOnInput);
                }
                else
                {
                    await AddQuestion("What are your goals?");
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
               // _firebase.LogEvent("chose_goals", focusText);
                if (ManMoreMuscle && ManLessFat)//&& count > 2
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");
                    //await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. I'm adding them to your program. High reps burn more fat.");


                }
                else if (ManMoreMuscle)
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "5");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "12");
                    // await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. Adding them to your program...");

                }
                else if (ManLessFat)
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "12");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "20");

                    // await AddQuestion("OK. High reps burn more fat. I'm setting yours at 12-20.");

                }
                else if (ManBetterHealth || ManStorngerSexDrive)
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscleBurnFat");

                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");
                    //await AddQuestion("Got it.");

                }
                else
                    return;
                if (ManLessFat && ManMoreMuscle)
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscleBurnFat");

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
                var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd() == "Woman";

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
            //        LocalDBManager.Instance.SetDBSetting("RBodyWeight", Convert.ToString(age));
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
               // _firebase.LogEvent("chose_goals", focusText);
                if (FemaleToned && ManLessFat) //&& count > 2
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");

                    // await AddQuestion("Got it. You can build muscle 59% faster with rest-pause sets. I'm adding them to your program. High reps burn more fat.");

                }
                else if (FemaleToned)
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscle");
                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "5");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "12");
                    //await AddQuestion("Got it. You can tone up 59% faster with rest-pause sets. Adding them to your program...");


                }
                else if (ManLessFat)
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "FatBurning");
                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "12");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "20");
                    //await AddQuestion("OK. High reps burn more fat. I'm setting yours at 12-20.");

                }
                else if (ManBetterHealth || FemaleMoreEnergy)
                {
                    LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscleBurnFat");
                    LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
                    LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");
                    //await AddQuestion("Got it.");

                }
                else
                    return;
                if (ManLessFat && FemaleToned)
                    LocalDBManager.Instance.SetDBSetting("RDemoreprange", "BuildMuscleBurnFat");
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
            if (LocalDBManager.Instance.GetDBSetting("RExLevel").Value == "Exp")
            {
                LocalDBManager.Instance.SetDBSetting("Rworkout_place", "gym");
                LocalDBManager.Instance.SetDBSetting("Rexperience", "more3years");
                //NoAdvancedClicked(new DrMuscleButton(), EventArgs.Empty);
                //await AddQuestion("How old are you?");
                //if (Device.RuntimePlatform.Equals(Device.Android))
                //    await Task.Delay(300);
                //GetAge();
                if (LocalDBManager.Instance.GetDBSetting("Rexperience").Value == "beginner")
                    AddCardio();// SetupQuickMode();
                else
                    workoutPlace();
                return;
            }
            LocalDBManager.Instance.SetDBSetting("Rexperience", "less1year");
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
            LocalDBManager.Instance.SetDBSetting("Rgender", "Woman");

            //await AddQuestion("Woman often want (tap all that apply):");

            //await AddCheckbox("Less fat", Man_LessFat_Clicked);
            //await AddCheckbox("Better health", Man_BetterHealth_Clicked);
            //await AddCheckbox("More energy", WoMan_MoreEnergy_Clicked);
            //await AddCheckbox("Toned muscles", WoMan_FemaleToned_Clicked);

            //await AddOptions("Continue", WomanTakeActionBasedOnInput);
            //SetupMassUnit();
            await AddQuestion("Age?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
            //SetupMainGoal();

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
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "12");
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
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "12");
            await AddAnswer(((DrMuscleButton)sender).Text);

            BeginnerSetup();
        }

        async void WomanFocusBothClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");
            await AddAnswer(((DrMuscleButton)sender).Text);
            BeginnerSetup();
        }

        async void WomanFocusBurnFatClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rreprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "20");
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
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");
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
            LocalDBManager.Instance.SetDBSetting("Rreprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "20");

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
            LocalDBManager.Instance.SetDBSetting("Rreprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "20");
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
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");
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
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "12");

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
                var pancakeView = new PancakeView() { CornerRadius = 0, HeightRequest = 50, Margin = new Thickness(25, 2) };
                pancakeView.OffsetAngle = 45;
                pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 0 });
                pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 1 });
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

                LocalDBManager.Instance.SetDBSetting("Rexperience", "less1year");

                //await AddAnswer(((DrMuscleButton)sender).Text);
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                if (LocalDBManager.Instance.GetDBSetting("Rexperience").Value == "beginner")
                    AddCardio();// SetupQuickMode();
                else
                    workoutPlace();
                //});

            }
            catch (Exception ex)
            {

            }
        }

        async void ManFocusMuscleClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscle");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "5");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "12");

            await AddAnswer(AppResources.FocusOnBuildingMuscle);

            BeginnerSetup();

        }

        async void ManFocusBothClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");

            await AddAnswer(AppResources.BuildMuscleAndBurnFat);
            BeginnerSetup();

        }

        async void ManFocusBurnFatClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rreprange", "FatBurning");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "12");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "20");

            await AddAnswer(AppResources.FocusOnBurningFat);
            BeginnerSetup();
        }

        async void YesBeginnerClicked(object sender, System.EventArgs e)
        {
            await ClearOptions();
            LocalDBManager.Instance.SetDBSetting("Rreprange", "BuildMuscleBurnFat");
            LocalDBManager.Instance.SetDBSetting("Rrepsminimum", "8");
            LocalDBManager.Instance.SetDBSetting("Rrepsmaximum", "15");
            LocalDBManager.Instance.SetDBSetting("Rexperience", "beginner");
            LocalDBManager.Instance.SetDBSetting("Rworkout_place", "homeBodyweightOnly");


            await AddAnswer(((DrMuscleButton)sender).Text);

            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddQuestion("Age?");
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
                    await AddQuestion("Age?");
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
            await AddQuestion("How long have you been lifting weights for?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await ClearOptions();
            await AddOptions(AppResources.LessThan1Year, LessOneYearClicked);
            await AddOptions(AppResources.OneToThreeYears, OneThreeYearsClicked);
            await AddOptions(AppResources.MoreThan3Years, More3YearsClicked);
        }

        async void AskForProgram()
        {
            ClearOptions();
            var btn1 = new DrMuscleButton()
            {
                Text = "Another program",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0,
            };
            btn1.Clicked += AnotherProgram_clicked;
            stackOptions.Children.Add(btn1);

            await AddOptions("Recommended program", RecommendedProgram_clicked);
        }
        async void AnotherProgram_clicked(object sender, EventArgs args)
        {
            //[Full body, 2-4x/week]
            //[Split body, 3-5x/week]
            await AddAnswer("Another program");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            ClearOptions();
            await AddOptions("Full body, 2-4x/week", (o, e) => {
                LocalDBManager.Instance.SetDBSetting("RMainProgram", "Full body");
                //SetupGetAge();
                GetGender();
            });

            await AddOptions("Split body, 3-4x/week", (o, e) => {
                LocalDBManager.Instance.SetDBSetting("RMainProgram", "Split body");
                GetGender();
            });

            await AddOptions("Powerlifting, 2-4x/week", (o, e) =>
            {
                LocalDBManager.Instance.SetDBSetting("RMainProgram", "Powerlifting");
                GetGender();
            });

            await AddOptions("Bands only, 2-4x/week", (o, e) =>
            {
                LocalDBManager.Instance.SetDBSetting("RMainProgram", "Bands only");
                GetGender();
            });

            await AddOptions("Bodyweight, 2-4x/week", (o, e) =>
            {
                LocalDBManager.Instance.SetDBSetting("RMainProgram", "Bodyweight");
                GetGender();
            });

        }

        async void ShowWorkoutReminder()
        {
            await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            ClearOptions();

            //[Another level][Recommended level]



            //Setup Program




            string ProgramLabel = "";
            string programInfo = "";
            int level = 0;
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("RAge").Value);


            if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Split"))
            {

                if (age > 50)
                {
                    ProgramLabel = "Up/Low Split Level 6";
                    programInfo = "This level includes A/B/C easy workouts to help you recover";
                    level = 6;
                }
                else
                //if (age > 30)
                {
                    if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "more3years"))
                    {
                        ProgramLabel = "Up/Low Split Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Up/Low Split Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }
                    else
                    {
                        ProgramLabel = "Full-Body Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }

                }
            }


            else if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Powerlifting"))
            {

                if (age > 50)
                {
                    ProgramLabel = "Powerlifting Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 1;
                }
                else
                //if (age > 30)
                {
                    if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "more3years"))
                    {
                        ProgramLabel = "Powerlifting Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Powerlifting Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }
                    else
                    {
                        ProgramLabel = "Powerlifting Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }

                }
            }
            else if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Bands only"))
            {

                if (age > 50)
                {
                    ProgramLabel = "Buffed w/ Bands Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 2;
                }
                else
                {
                    if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "more3years"))
                    {
                        ProgramLabel = "Buffed w/ Bands Level 2";
                        programInfo = "This level includes A/B workouts to help you recover.";
                        level = 2;
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Buffed w/ Bands Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }
                    else
                    {
                        ProgramLabel = "Buffed w/ Bands Level 2";
                        programInfo = "This level includes A/B workouts to help you recover.";
                        level = 2;
                    }

                }

            }
            else if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Bodyweight"))
            {

                if (age > 50)
                {
                    ProgramLabel = "Bodyweight Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 2;
                }
                else
                //if (age > 30)
                {
                    if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "more3years"))
                    {
                        ProgramLabel = "Bodyweight Level 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "1-3years"))
                    {
                        ProgramLabel = "Bodyweight Level 1";
                        programInfo = "This level includes simple and effective exercises.";
                        level = 1;
                    }
                    else
                    {
                        ProgramLabel = "Bodyweight 2";
                        programInfo = "This level includes new and harder exercises.";
                        level = 2;
                    }
                }
            }
            else
            {
                if (age > 50)
                {
                    ProgramLabel = "Full-Body Level 6";
                    programInfo = "This level includes A/B/C easy workouts to help you recover";
                    level = 6;
                }
                else
                //if (age > 30)
                {
                    ProgramLabel = "Full-Body Level 1";
                    programInfo = "This level includes simple and effective exercises.";
                    level = 1;
                }
            }
            // await AddQuestion($"Based on your age and experience, I recommend you start on level {level}. {programInfo} Higher levels have more exercise variations.");


            await AddQuestion($"Based on your age and experience, I recommend you start on level {level}. {programInfo} Higher levels have more exercise variations.");

            var btn1 = new DrMuscleButton()
            {
                Text = "Another level",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0,
            };
            btn1.Clicked += async (s, e) => {

                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                //{
                //    Message = "Higher levels have more recovery and exercise variations. You can change this later.",
                //    OkText = AppResources.GotIt,
                //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                //});
                List<string> levels = new List<string>();
                int lvl = 7;
                if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Split"))
                {
                    lvl = 7;
                }
                else if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Powerlifting"))
                {
                    lvl = 4;
                }
                else if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Bands only"))
                {
                    lvl = 2;
                }
                else if (LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Bodyweight"))
                {
                    lvl = 4;
                }


                for (int i = 1; i <= lvl; i++)
                {
                    levels.Add($"Level {i}");
                }
                LevelPicker.ItemsSource = levels;

                await AddAnswer("Another level");
                LevelPicker.IsVisible = true;
                Device.BeginInvokeOnMainThread(() =>
                {
                    LevelPicker.Focus();
                });
            };
            stackOptions.Children.Add(btn1);

            await AddOptions($"Recommended level {level}", async (s, e) => {
                await AddAnswer($"Recommended level {level}");
                LocalDBManager.Instance.SetDBSetting("RMainLevel", level.ToString());
                //SetupMassUnit();
                if (LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "returning from a break")
                {
                    AskSetStyle();
                    //SetupMassUnit();
                }
                else
                {
                    AskSetStyle();
                }
            });
        }

        private async void AskSetStyle()
        {

            await ClearOptions();
            await AddQuestion("Try rest-pause sets? They're harder, but make your workouts 59% faster.");


            var btn1 = new DrMuscleButton()
            {
                Text = "Normal sets",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0,
            };
            btn1.Clicked += (o, ev) => {
                AddAnswer("Normal sets");
                SetStyle = true;
                SetupMassUnit();
            };
            stackOptions.Children.Add(btn1);
            if (LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "new to training" || LocalDBManager.Instance.GetDBSetting("RCustomExperience").Value == "returning from a break")
            { }
            else
            {
                var btnPyramid = new DrMuscleButton()
                {
                    Text = "Pyramid sets",
                    TextColor = Color.FromHex("#195377"),
                    BackgroundColor = Color.Transparent,
                    HeightRequest = 55,
                    BorderWidth = 2,
                    BorderColor = AppThemeConstants.BlueColor,
                    Margin = new Thickness(25, 2),
                    CornerRadius = 0
                };
                btnPyramid.Clicked += (o, ev) => {
                    AddAnswer("Pyramid sets");
                    SetStyle = false;
                    IsPyramid = true;
                    SetupMassUnit();
                };
                stackOptions.Children.Add(btnPyramid);

                var btn2 = new DrMuscleButton()
                {
                    Text = "Reverse pyramid sets",
                    TextColor = Color.FromHex("#195377"),
                    BackgroundColor = Color.Transparent,
                    HeightRequest = 55,
                    BorderWidth = 2,
                    BorderColor = AppThemeConstants.BlueColor,
                    Margin = new Thickness(25, 2),
                    CornerRadius = 0,
                };
                btn2.Clicked += (o, ev) => {
                    AddAnswer("Reverse pyramid sets");
                    SetStyle = null;
                    SetupMassUnit();
                };
                stackOptions.Children.Add(btn2);
            }
            await AddOptions("Rest-pause sets", (ss, ee) =>
            {
                AddAnswer("Rest-pause sets");
                SetStyle = false;
                SetupMassUnit();
            });


        }

        async void GetGender()
        {

            ClearOptions();
            GotItAfterImage(new DrMuscleButton(), EventArgs.Empty);
        }

        async void SetupGetAge()
        {
            ClearOptions();
            await AddQuestion("Age?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            GetAge();
        }
        async void RecommendedProgram_clicked(object sender, EventArgs args)
        {

            try
            {

                if (LocalDBManager.Instance.GetDBSetting("Rexperience") != null && (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "more3years") || (LocalDBManager.Instance.GetDBSetting("Rexperience")?.Value == "1-3years"))//
                    LocalDBManager.Instance.SetDBSetting("RMainProgram", "Split body");
                else
                    LocalDBManager.Instance.SetDBSetting("RMainProgram", "Full body");

            }
            catch (Exception ex)
            {
                LocalDBManager.Instance.SetDBSetting("RMainProgram", "Full body");
            }
            GetGender();
        }

        async void LessOneYearClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rexperience", "less1year");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);

            await AddQuestion("Since you have been working out for less than 1 year, I recommend you train your full body 2-4 times a week for best results.");
            AskForProgram();
            LocalDBManager.Instance.SetDBSetting("RMainProgram", "Full body");
            //await AddQuestion("How old are you?");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //GetAge();
        }

        async void OneThreeYearsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rexperience", "1-3years");

            await AddAnswer(((DrMuscleButton)sender).Text);
            await AddQuestion("Since you have been working out for 1-3 years, I recommend you train 3-4 times a week on an upper/lower-body split for best results.");
            LocalDBManager.Instance.SetDBSetting("RMainProgram", "Split body");
            AskForProgram();
            //await AddQuestion("How old are you?");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //GetAge();
        }

        async void More3YearsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rexperience", "more3years");

            await AddAnswer(((DrMuscleButton)sender).Text);
            await AddQuestion("Since you have been working out for 3+ years, I recommend you train 3-5 times a week on an upper/lower-body split for best results.");
            LocalDBManager.Instance.SetDBSetting("RMainProgram", "Split body");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            AskForProgram();
            //await AddQuestion("How old are you?");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //GetAge();
        }

        async void workoutPlace()
        {
            await AddQuestion("Where do you work out?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions(AppResources.HomeBodtweightOnly, BodyweightClicked);
            await AddOptions("Home (bodyweight & bands)", BodyweightBandsClicked);
            await AddOptions(AppResources.HomeGymBasicEqipment, HomeClicked);
            await AddOptions(AppResources.Gym, GymClicked);

        }

        async void GymClicked(object senders, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rworkout_place", "gym");

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
                //AskforBodypartPriority();
                AskForIncrements();
            });
        }

        async void HomeClicked(object senders, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rworkout_place", "home");

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
                //AskforBodypartPriority();
                AskForIncrements();

            });
        }

        async void AskForIncrements()
        {
            ClearOptions();
            await AddQuestion("Your dumbbells and plates go up in what increments?");
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
            await AddOptions(isKg ? "1 kg" : "2.5 lbs", async (s, e) => {
                await AddAnswer(isKg ? "1 kg" : "2.5 lbs");
                IncrementUnit = new MultiUnityWeight(isKg ? (decimal)1 : (decimal)2.5, isKg ? "kg" : "lb");
                AskforBodypartPriority();
            });
            await AddOptions(isKg ? "2.5 kg" : "5 lbs", async (s, e) => {
                await AddAnswer(isKg ? "2.5 kg" : "5 lbs");
                IncrementUnit = new MultiUnityWeight(isKg ? (decimal)2.5 : (decimal)5, isKg ? "kg" : "lb");
                AskforBodypartPriority();
            });
            await AddOptions("Not sure (set up later in Settings)", async (s, e) => {
                await AddAnswer("Not sure (set up later in Settings)");
                IncrementUnit = null;
                AskforBodypartPriority();

            });
        }

        async void BodyweightClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rworkout_place", "homeBodyweightOnly");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddQuestion("OK, bodyweight exercises only. No problem.");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //SetupQuickMode();
            AskforBodypartPriority();
        }
        async void BodyweightBandsClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("Rworkout_place", "homeBodyweightBandsOnly");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddQuestion("OK, bodyweight exercises only. No problem.");
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    await Task.Delay(300);
            //SetupQuickMode();
            //AddCardio();
            AskforBodypartPriority();
        }

        private async void BodypartPicker_Unfocused(object sender, FocusEventArgs e)
        {
            bodypartName = (sender as Picker).SelectedIndex == -1 || (sender as Picker).SelectedIndex == 0 ? "" : (string)(sender as Picker).SelectedItem;
            if ((sender as Picker).SelectedIndex != -1)
                await AddAnswer((string)(sender as Picker).SelectedItem);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //SetupQuickMode();
            AddCardio();
        }

        async void AskforBodypartPriority()
        {
            bodypartName = "";
            await AddQuestion("Wanna prioritize a body part?");
            if (BodyPartPicker != null)
            {
                BodyPartPicker.Unfocused -= BodypartPicker_Unfocused;
            }
            BodyPartPicker = new Picker();
            List<string> bodyParts = new List<string>();
            //bodyParts.Add("");
            bodyParts.Add("No thanks");
            bodyParts.Add("Biceps");
            bodyParts.Add("Chest");
            bodyParts.Add("Abs");
            bodyParts.Add("Legs");
            bodyParts.Add("Glutes");

            BodyPartPicker.ItemsSource = bodyParts;
            MainGrid.Children.Insert(0, BodyPartPicker);
            BodyPartPicker.Unfocused += BodypartPicker_Unfocused;
            BodyPartPicker.Focus();
            
        }

        //async void AskforBodypartPriority()
        //{
        //    bodypartName = "";
        //    await AddQuestion("Wanna prioritize a body part?");
        //    if (LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd().TrimEnd() == "Man")
        //    {
        //        bodypart1 = await AddCheckbox("Biceps", (sender, ev) =>
        //        {
        //            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
        //            if (bodypartName != "Biceps")
        //            {
        //                img.Source = "done.png";
        //                bodypartName = "Biceps";
        //            }
        //            else
        //            {
        //                bodypartName = "";
        //                img.Source = "Undone.png";
        //            }
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
        //            //((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";


        //        });
        //        bodypart2 = await AddCheckbox("Chest", (sender, ev) =>
        //        {
        //            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];

        //            if (bodypartName != "Chest")
        //            {
        //                img.Source = "done.png";
        //                bodypartName = "Chest";
        //            }
        //            else
        //            {
        //                bodypartName = "";
        //                img.Source = "Undone.png";
        //            }
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
        //            //((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";
        //        });
        //        bodypart3 = await AddCheckbox("Abs", (sender, ev) =>
        //        {
        //            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];

        //            if (bodypartName != "Abs")
        //            {
        //                img.Source = "done.png";
        //                bodypartName = "Abs";
        //            }
        //            else
        //            {
        //                bodypartName = "";
        //                img.Source = "Undone.png";
        //            }
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
        //            //((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";

        //        });
        //        //bodypartBalanced = await AddCheckbox("Balanced", (sender, ev) =>
        //        //{
        //        //    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
        //        //    img.Source = "done.png";
        //        //    bodypartName = "";
        //        //    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
        //        //    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
        //        //    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
        //        //});
        //    }
        //    else
        //    {
        //        bodypart1 = await AddCheckbox("Abs", (sender, ev) =>
        //        {
        //            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];

        //            if (bodypartName != "Abs")
        //            {
        //                img.Source = "done.png";
        //                bodypartName = "Abs";
        //            }
        //            else
        //            {
        //                bodypartName = "";
        //                img.Source = "Undone.png";
        //            }
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
        //            //((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";

        //        });
        //        bodypart2 = await AddCheckbox("Legs", (sender, ev) =>
        //        {
        //            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];

        //            if (bodypartName != "Legs")
        //            {
        //                img.Source = "done.png";
        //                bodypartName = "Legs";
        //            }
        //            else
        //            {
        //                bodypartName = "";
        //                img.Source = "Undone.png";
        //            }
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
        //            //((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";
        //        });
        //        bodypart3 = await AddCheckbox("Glutes", (sender, ev) =>
        //        {
        //            Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];

        //            if (bodypartName != "Glutes")
        //            {
        //                img.Source = "done.png";
        //                bodypartName = "Glutes";
        //            }
        //            else
        //            {
        //                bodypartName = "";
        //                img.Source = "Undone.png";
        //            }
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";
        //            ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
        //            //((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypartBalanced).Content).Children[0]).Source = "Undone.png";
        //        });
        //        //bodypartBalanced = await AddCheckbox("Balanced", (sender, ev) =>
        //        //{
        //        //    Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
        //        //    img.Source = "done.png";
        //        //    bodypartName = "";

        //        //    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart2).Content).Children[0]).Source = "Undone.png";
        //        //    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart3).Content).Children[0]).Source = "Undone.png";
        //        //    ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)bodypart1).Content).Children[0]).Source = "Undone.png";

        //        //});
        //    }
        //    await AddOptions("Continue", async (sender, ee) =>
        //    {
        //        if (bodypartName.Equals("balanced"))
        //        {
        //            await UserDialogs.Instance.AlertAsync(new AlertConfig()
        //            {
        //                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
        //                Title = "Error",
        //                Message = $"Please select one body part.",
        //                OkText = AppResources.Ok
        //            });
        //            return;
        //        }
        //        await AddAnswer("Continue");
        //        if (Device.RuntimePlatform.Equals(Device.Android))
        //            await Task.Delay(300);
        //        //SetupQuickMode();
        //        AddCardio();
        //    });
        //}

        async void AddCardio()
        {
            await AddQuestion("Include cardio?", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            await AddOptions("No cardio", async (sender, e) =>
            {
                IsIncludeCardio = false;
                await AddAnswer("No cardio");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                SetupQuickMode();
            });

            await AddOptions("Include cardio", async (sender, e) =>
            {
                await AddAnswer("Include cardio");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                IsIncludeCardio = true;
                SetupQuickMode();
            });

        }

        async void SetupQuickMode()
        {

            if (LocalDBManager.Instance.GetDBSetting("RNewLevel") != null && LocalDBManager.Instance.GetDBSetting("RExLevel") != null && LocalDBManager.Instance.GetDBSetting("RNewLevel").Value == "Streamline" && LocalDBManager.Instance.GetDBSetting("RExLevel").Value == "New")
            {
                LocalDBManager.Instance.SetDBSetting("RQuickMode", "false");
                ProgramReadyInstruction();
                return;
            }
            await AddQuestion("How long do you want to work out?", false);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            //await AddOptions("30 min", QuickmodeOnClicked);
            //await AddOptions("45 min", QuickmodeMediumClicked);

            var btn1 = new DrMuscleButton()
            {
                Text = "30 min",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0,
            };
            btn1.Clicked += QuickmodeOnClicked;
            stackOptions.Children.Add(btn1);

            var btn2 = new DrMuscleButton()
            {
                Text = "45 min",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius=0
            };
            btn2.Clicked += QuickmodeMediumClicked;
            stackOptions.Children.Add(btn2);


            await AddOptions("Flexible (based on progress)", QuickmodeOffClicked);
        }
        async void QuickmodeOnClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("RQuickMode", "true");

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
            LocalDBManager.Instance.SetDBSetting("RQuickMode", "false");

            await AddAnswer(((DrMuscleButton)sender).Text);
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(300);
            LearnMoreTimeline();

        }

        async void QuickmodeMediumClicked(object sender, System.EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("RQuickMode", "null");

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
            //        LocalDBManager.Instance.SetDBSetting("RAge", Convert.ToString(age));
            //        await AddAnswer(Convert.ToString(age));


            //        if (age > 50)
            //            learnMore.AgeDesc = $"Recovery is slower at {age}. So, I added easy days to your program.";
            //        else if (age > 30)
            //            learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I'm updating your program to make sure you train each muscle max 2x a week.";
            //        else
            //            learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";
            //        await AddQuestion(learnMore.AgeDesc);
            //        if (LocalDBManager.Instance.GetDBSetting("Rexperience").Value == "beginner")
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


            if (!CrossConnectivity.Current.IsConnected)
            {
                ConnectionErrorPopup();

                GotoLevelUp();
                return;
            }
            //await AddQuestion($"All right! Your custom program is ready. Learn more?");
            //await AddOptions(AppResources.LearnMore, LearnMoreButton_Clicked);
            //await AddOptions(AppResources.Skip, LearnMoreSkipButton_Clicked);
            LearnMoreSkipButton_Clicked(new DrMuscleButton(), EventArgs.Empty);

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
                LocalDBManager.Instance.SetDBSetting("Rfirstname", response.Text);
                LocalDBManager.Instance.SetDBSetting("RSetStyle", "RestPause");
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

                LocalDBManager.Instance.SetDBSetting("Remail", response.Text);
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
                LocalDBManager.Instance.SetDBSetting("Rpassword", response.Text);
                await Task.Delay(100);
                //CreateAccountBeforeDemoButton_Clicked();
            }

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



            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("Rexperience");
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
            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd() == "Woman";
            if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "BuildMuscle")
            {
                learnMore.Focus = IsWoman ? "Getting stronger" : "Building muscle";
                learnMore.FocusDesc = IsWoman ? "To get stronger, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range." : "To build muscle, I recommend you repeat each exercise 5-12 times. You will also get stronger by lifting in that range.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "BuildMuscleBurnFat")
            {
                learnMore.Focus = IsWoman ? "Overall fitness" : "Building muscle and burning fat";
                learnMore.FocusDesc = IsWoman ? "For overall fitness, I recommend you repeat each exercise 8-15 times." : "To build muscle and burn fat, I recommend you repeat each exercise 8-15 times.";
            }
            else if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "FatBurning")
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
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("RAge").Value);
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

                if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "BuildMuscle")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBuild;
                }
                else if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "BuildMuscleBurnFat")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBuildNBuildFat;
                }
                else if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "FatBurning")
                {
                    goalLabel = AppResources.IUpdateItEveryTimeYouWorkOutBurnFatFaster;
                }


            }
            catch (Exception ex)
            { }

            try
            {
                await ClearOptions();
                DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("Rexperience");
                DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("Rworkout_place");
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
                    int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("RAge").Value);
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
                    

                    

                    LearnMoreSkipButton_Clicked(new DrMuscleButton(), EventArgs.Empty);
                    //await AddQuestion("Congratulations! Your custom, smart program is ready. Learn more?");
                    //await AddOptions("Learn more", async (sender, esc) =>
                    //{
                    //    await AddAnswer(((DrMuscleButton)sender).Text, false);

                    //    await AddQuestion($"Based on your age, experience, and preferences, I recommend you work out {weekX} times a week on this program: '{ProgramLabel}'.");
                    //    _firebase.LogEvent("got_program", ProgramLabel);
                    //    await ClearOptions();
                    //    await AddOptions(AppResources.GotIt, BtnGotItProgram);
                    //    //Day

                    //    if (Device.RuntimePlatform.Equals(Device.iOS))
                    //    {
                    //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    //    }
                    //    async void BtnGotItProgram(object sse, EventArgs evr)
                    //    {
                    //        //await AddAnswer(((DrMuscleButton)sse).Text, false);

                    //        //    ((DrMuscleButton)sse).Clicked -= BtnGotItProgram;
                    //        //    ((DrMuscleButton)sse).Clicked += BtnGotItNext;
                    //        //    await AddQuestion($"Your smart program:\n- Updates in real time\n- Matches your progress\n- Speeds up future progress");

                    //        //    if (Device.RuntimePlatform.Equals(Device.iOS))
                    //        //    {
                    //        //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //        //        lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    //        //    }

                    //        //    async void BtnGotItNext(object s, EventArgs e)
                    //        //{
                    //        //    _firebase.LogEvent("gotten_program", ProgramLabel);
                    //        //    if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //        await Task.Delay(300);
                    //        LearnMoreSkipButton_Clicked(sse, evr);
                    //        //await AddQuestion(AppResources.WarningIMNOtLikeOtherAppsIGuideYouInRealTimeBased, false);
                    //        //if (Device.RuntimePlatform.Equals(Device.iOS))
                    //        //{
                    //        //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                    //        //    lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
                    //        //}
                    //        //    ((DrMuscleButton)s).Clicked -= BtnGotItNext;
                    //        //((DrMuscleButton)s).Clicked += btnGotIt4_Clicked;
                    //        //async void btnGotIt4_Clicked(object ob, EventArgs ar)
                    //        //{
                    //        //    await AddAnswer(((DrMuscleButton)ob).Text, false);
                    //        //    await ClearOptions();
                    //        //    //
                    //        //    await AddQuestion("This app uses the latest science, but it can't correct your form, or allow for a medical condition. It may be wrong at times. When in doubt, trust your judgment. Features like smart watch integration and calendar view are not yet available. But if you’re an early adopter who wants to get in shape fast, you'll love your new custom workouts. Give us a shot: we'll treat your feedback like gold. Got a suggestion? Get in touch. We release new features every month.");
                    //        //    if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //        await Task.Delay(300);
                    //        //    await AddOptions("View latest features", async (sand, ees) =>
                    //        //    {
                    //        //        if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //            await Task.Delay(300);
                    //        //        //Device.OpenUri(new Uri("https://dr-muscle.com/timeline"));
                    //        //        await Browser.OpenAsync("https://dr-muscle.com/timeline", BrowserLaunchMode.SystemPreferred);
                    //        //        LearnMoreSkipButton_Clicked(sand, ees);
                    //        //    });
                    //        //    await AddOptions("Continue", async (sende, eee) =>
                    //        //    {
                    //        //        if (Device.RuntimePlatform.Equals(Device.Android))
                    //        //            await Task.Delay(300);
                    //        //        LearnMoreSkipButton_Clicked(sende, eee);
                    //        //    });
                    //        //    stackOptions.Children.Add(TermsConditionStack);
                    //        //    TermsConditionStack.IsVisible = true;
                    //        //}
                    //    }
                    //    //}
                    //});
                    //await AddOptions("Skip", async (sender, esc) => {
                    //    //await AddAnswer(((DrMuscleButton)sender).Text, false);
                    //    _firebase.LogEvent("got_program", ProgramLabel);

                    //    LearnMoreSkipButton_Clicked(sender, esc);
                    //});

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
            //await AddAnswer(((DrMuscleButton)sender).Text);
            await ClearOptions();
            int age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("RAge").Value);
            learnMore.Age = age;

            if (age > 50)
                learnMore.AgeDesc = $"Recovery is slower at {age}.";
            else if (age > 30)
                learnMore.AgeDesc = $"Recovery is a bit slower at {age}. So, I recommend you train each muscle 2x a week (instead of 3x a week).";
            else
                learnMore.AgeDesc = "Recovery is optimal at your age. You can train each muscle as often as 3x a week.";



            var IsWoman = LocalDBManager.Instance.GetDBSetting("gender").Value.TrimEnd() == "Woman";
            if (!string.IsNullOrEmpty(focusText))
            {
                focusText = focusText.Replace("\nStronger sex drive", "");
                focusText = focusText.Replace("Stronger sex drive", "");
            }
            if (string.IsNullOrEmpty(focusText))
                focusText = "Better health";

            if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "BuildMuscle")
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
            else if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "BuildMuscleBurnFat")
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
            else if (LocalDBManager.Instance.GetDBSetting("Rreprange").Value == "FatBurning")
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
            LocalDBManager.Instance.SetDBSetting("RDBFocus", learnMore.Focus);

            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("Rexperience");
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

            var changesText = "I will apply the following changes:";

            changesText += $"\n\n- New rep range: {LocalDBManager.Instance.GetDBSetting("Rrepsminimum").Value}-{LocalDBManager.Instance.GetDBSetting("Rrepsmaximum").Value}";
            var quickMode = "";
            if (LocalDBManager.Instance.GetDBSetting("RQuickMode").Value == "true")
                quickMode = "30 min";
            else if (LocalDBManager.Instance.GetDBSetting("RQuickMode").Value == "null")
                quickMode = "45 min";
            else
                quickMode = "Flexible (based on progress)";

            changesText += $"\n- Workout duration: {quickMode}";

            var setStyle = this.SetStyle == null ? "Reverse pyramide" : this.SetStyle == false ? "Rest-pause" : "Normal";
            if (IsPyramid)
                setStyle = "Pyramid";
            changesText += $"\n- Set style: {setStyle}";
            var isCardio = IsIncludeCardio ? "Yes" : "No";
            changesText += $"\n- Include Cardio: {isCardio}";

            //DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("Rexperience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("Rworkout_place");

            var level = 0;
            if (LocalDBManager.Instance.GetDBSetting("RMainLevel") != null)
                level = int.Parse(LocalDBManager.Instance.GetDBSetting("RMainLevel").Value);
            bool isSplit = LocalDBManager.Instance.GetDBSetting("RMainProgram").Value.Contains("Split");
            bool isGym = workoutPlaceSetting?.Value == "gym";

            var mo = AppThemeConstants.GetLevelLastProgram(level, isGym, !isSplit, LocalDBManager.Instance.GetDBSetting("RMainProgram").Value);

            if (workoutPlaceSetting?.Value == "homeBodyweightOnly")
            {
                if (LocalDBManager.Instance.GetDBSetting("RCustomMainLevel") != null && LocalDBManager.Instance.GetDBSetting("RCustomMainLevel")?.Value == "1")
                {
                    mo.workoutName = "Bodyweight 1";
                    mo.workoutid = 12645;
                    mo.programid = 487;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 1";
                }
                else if (level <= 1)
                {
                    mo.workoutName = "Bodyweight 1";
                    mo.workoutid = 12645;
                    mo.programid = 487;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 1";
                }
                else if (level == 2)
                {
                    mo.workoutName = "Bodyweight 2";
                    mo.workoutid = 12646;
                    mo.programid = 488;
                    mo.reqWorkout = 12;
                    mo.programName = "Bodyweight Level 2";
                }
                else if (level == 3)
                {
                    mo.workoutName = "Bodyweight 3";
                    mo.workoutid = 14017;
                    mo.programid = 923;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 3";
                }
                else if (level >= 4)
                {
                    mo.workoutName = "Bodyweight 4";
                    mo.workoutid = 14019;
                    mo.programid = 924;
                    mo.reqWorkout = 15;
                    mo.programName = "Bodyweight Level 4";
                }

            }
            else if (workoutPlaceSetting?.Value == "homeBodyweightBandsOnly")
            {
                mo.workoutName = "[Home] Buffed w/ Bands";
                mo.programName = "[Home] Buffed w/ Bands Level 1";
                mo.workoutid = 15377;
                mo.programid = 1339;
                mo.reqWorkout = 15;

                if (experienceSetting?.Value == "more3years")
                {
                    mo.workoutName = "[Home] Buffed w/ Bands 2A";
                    mo.programName = "[Home] Buffed w/ Bands Level 2";
                    mo.workoutid = 15376;
                    mo.programid = 1338;
                    mo.reqWorkout = 18;
                }
            }


            LocalDBManager.Instance.SetDBSetting("RrecommendedWorkoutId", mo.workoutid.ToString());
            LocalDBManager.Instance.SetDBSetting("RrecommendedWorkoutLabel", mo.workoutName);
            LocalDBManager.Instance.SetDBSetting("RrecommendedProgramId", mo.programid.ToString());
            LocalDBManager.Instance.SetDBSetting("RrecommendedRemainingWorkout", mo.reqWorkout.ToString());

            LocalDBManager.Instance.SetDBSetting("RrecommendedProgramLabel", mo.programName);

            changesText += $"\n- New program: {mo.programName}";

            BotList.Add(new BotModel()
            {
                Question = changesText,
                Type = BotType.Ques
            });
            BotList.Add(new BotModel()
            {
                Type = BotType.Empty
            });
            //SignupCode here:

            var btn1 = new DrMuscleButton()
            {
                Text = "Restart",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0,
            };
            btn1.Clicked += (o, ev) => {
                BotList.Clear();
                if (Device.RuntimePlatform.Equals(Device.Android))
                { 
                List<string> ages = new List<string>();
                List<string> bodyweight = new List<string>();
                for (int i = 10; i < 125; i++)
                {
                    ages.Add($"{i}");
                }
                List<string> levels = new List<string>();
                for (int i = 1; i < 7; i++)
                {
                    levels.Add($"Level {i}");
                }

                if (AgePicker != null)
                    AgePicker.SelectedIndexChanged -= AgePicker_SelectedIndexChanged;

                AgePicker = new Picker()
                {
                    Title = "Age?"
                };
                AgePicker.ItemsSource = ages;
                AgePicker.SelectedItem = "35";
                AgePicker.Unfocused += AgePicker_Unfocused;
                AgePicker.SelectedIndexChanged += AgePicker_SelectedIndexChanged;


                if (LevelPicker != null)
                    LevelPicker.Unfocused -= LevelPicker_Unfocused;

                LevelPicker = new Picker()
                {
                    Title = "Select level"
                };
                LevelPicker.ItemsSource = levels;
                LevelPicker.SelectedIndex = 0;
                LevelPicker.Unfocused += LevelPicker_Unfocused;
                MainGrid.Children.Insert(0, AgePicker);
                MainGrid.Children.Insert(0, LevelPicker);
                }
                StartSetup();
            };
            stackOptions.Children.Add(btn1);
            await AddOptions($"Save", (o, ev) => {
                AddAnswer($"Save");
                UpdateReconfigration(mo.workoutid);
            });
            await Task.Delay(300);
            Device.BeginInvokeOnMainThread(() =>
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            });
            //SetMenu();
        }

        private async void UpdateReconfigration(int workoutId)
        {

            RegisterModel registerModel = new RegisterModel();

            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("Rgender").Value.TrimEnd();
            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            if (LocalDBManager.Instance.GetDBSetting("RQuickMode") == null)
                registerModel.IsQuickMode = false;
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("RQuickMode").Value == "null")
                    registerModel.IsQuickMode = null;
                else
                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("RQuickMode").Value == "true" ? true : false;
            }
            if (LocalDBManager.Instance.GetDBSetting("RAge") != null)
                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("RAge").Value);
            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Rrepsminimum").Value);
            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Rrepsmaximum").Value);
            if (LocalDBManager.Instance.GetDBSetting("RBodyWeight") != null)
                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("RBodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            
            registerModel.LearnMoreDetails = learnMore;
            registerModel.IsHumanSupport = IsHumanSupport;
            registerModel.IsCardio = IsIncludeCardio;
            registerModel.BodyPartPrioriy = bodypartName;
            registerModel.SetStyle = SetStyle;
            registerModel.IsPyramid = IsPyramid;
            registerModel.MainGoal = mainGoal;


            
            registerModel.MobilityLevel = LocalDBManager.Instance.GetDBSetting("MobilityLevel")?.Value;
            if (LocalDBManager.Instance.GetDBSetting("IsMobility")?.Value == null)
                registerModel.IsMobility = null;
            else
                registerModel.IsMobility =  LocalDBManager.Instance.GetDBSetting("IsMobility")?.Value == "false" ? false : true;

        
            if (LocalDBManager.Instance.GetDBSetting("ReminderDays") != null && LocalDBManager.Instance.GetDBSetting("ReminderTime") != null)
            {
                try
                {
                    registerModel.ReminderTime = TimeSpan.Parse(LocalDBManager.Instance.GetDBSetting("ReminderTime").Value);
                    registerModel.ReminderDays = LocalDBManager.Instance.GetDBSetting("ReminderDays")?.Value;
                }
                catch (Exception ex)
                {

                }

            }
            if (IsEquipment)
            {
                if (isDumbbells == true || IsPlates == true || IsPully == true || IsChinupBar == true)
                {
                    if (IsEquipment)
                    {
                        var model = new EquipmentModel();

                        if (LocalDBManager.Instance.GetDBSetting("Rworkout_place")?.Value == "gym")
                        {
                            model.IsEquipmentEnabled = true;
                            model.IsDumbbellEnabled = isDumbbells;
                            model.IsPlateEnabled = IsPlates;
                            model.IsPullyEnabled = IsPully;
                            model.IsChinUpBarEnabled = IsChinupBar;
                            model.Active = "gym";

                            model.IsHomeEquipmentEnabled = false;
                            model.IsHomeDumbbell = true;
                            model.IsHomePlate = true;
                            model.IsHomePully = true;
                            model.IsHomeChinupBar = true;

                        }
                        else
                        {
                            model.IsEquipmentEnabled = false;
                            model.IsDumbbellEnabled = true;
                            model.IsPlateEnabled = true;
                            model.IsPullyEnabled = true;
                            model.IsChinUpBarEnabled = true;

                            model.IsOtherEquipmentEnabled = false;
                            model.IsOtherDumbbell = true;
                            model.IsOtherPlate = true;
                            model.IsOtherPully = true;
                            model.IsOtherChinupBar = true;

                            model.IsHomeEquipmentEnabled = true;
                            model.IsHomeDumbbell = isDumbbells;
                            model.IsHomePlate = IsPlates;
                            model.IsHomePully = IsPully;
                            model.IsHomeChinupBar = IsChinupBar;
                            model.Active = "home";
                        }
                        registerModel.EquipmentModel = model;
                    }
                }
                else
                {
                    var model = new EquipmentModel();

                    model.IsEquipmentEnabled = false;
                    model.IsDumbbellEnabled = true;
                    model.IsPlateEnabled = true;
                    model.IsPullyEnabled = true;
                    model.IsChinUpBarEnabled = true;

                    model.IsOtherEquipmentEnabled = false;
                    model.IsOtherDumbbell = true;
                    model.IsOtherPlate = true;
                    model.IsOtherPully = true;
                    model.IsOtherChinupBar = true;

                    model.IsHomeEquipmentEnabled = false;
                    model.IsHomeDumbbell = true;
                    model.IsHomePlate = true;
                    model.IsHomePully = true;
                    model.IsHomeChinupBar = true;
                    model.Active = "gym";
                    registerModel.EquipmentModel = model;
                }
            }
            if (IncrementUnit != null)
            {
                registerModel.Increments = IncrementUnit.Kg;
            }

            //API call
            var uim = await DrMuscleRestClient.Instance.RegisterWithUser(registerModel);
            if (uim != null)
            {
                DBSetting dbToken = LocalDBManager.Instance.GetDBSetting("token");
                DBSetting dbTokenExpirationDate = LocalDBManager.Instance.GetDBSetting("token_expires_date");
                DBSetting dbOnBoardingSeen = LocalDBManager.Instance.GetDBSetting("onboarding_seen");
                LocalDBManager.Instance.Reset();
                LocalDBManager.Instance.SetDBSetting("token", dbToken.Value);
                LocalDBManager.Instance.SetDBSetting("token_expires_date", dbTokenExpirationDate.Value);
                LocalDBManager.Instance.SetDBSetting("onboarding_seen", dbOnBoardingSeen.Value);
                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
            LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
            LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
            LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
            LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
            LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
            LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
            LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
            LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
            LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
            LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
            if (uim.Age != null)
                LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                if (uim.ReminderTime != null)
                    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                if (uim.ReminderDays != null)
                    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
            LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
            LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
            LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
            LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
            LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

                LocalDBManager.Instance.SetDBSetting("IsMobility", uim.IsMobility == null ? null : uim.IsMobility == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("IsExerciseQuickMode", uim.IsExerciseQuickMode == null ? null : uim.IsExerciseQuickMode == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("MobilityLevel", uim.MobilityLevel);

                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                if (uim.IsPyramid)
                {
                    LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
                    LocalDBManager.Instance.SetDBSetting("IsRPyramid", "true");
                    LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
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

                //if (uim.EquipmentModel != null)
                //{
                //    LocalDBManager.Instance.SetDBSetting("Equipment", uim.EquipmentModel.IsEquipmentEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("ChinUp", uim.EquipmentModel.IsChinUpBarEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("Dumbbell", uim.EquipmentModel.IsDumbbellEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("Plate", uim.EquipmentModel.IsPlateEnabled ? "true" : "false");
                //    LocalDBManager.Instance.SetDBSetting("Pully", uim.EquipmentModel.IsPullyEnabled ? "true" : "false");
                //}
                //else
                //{
                //    LocalDBManager.Instance.SetDBSetting("Equipment", "false");
                //    LocalDBManager.Instance.SetDBSetting("ChinUp", "true");
                //    LocalDBManager.Instance.SetDBSetting("Dumbbell", "true");
                //    LocalDBManager.Instance.SetDBSetting("Plate", "true");
                //    LocalDBManager.Instance.SetDBSetting("Pully", "true");
                //}
                SetupEquipment(uim);
            if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
            else
                LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

            ((App)Application.Current).displayCreateNewAccount = true;

            if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
            else
                LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");
                await DrMuscleRestClient.Instance.SaveWorkoutV3(new SaveWorkoutModel() { WorkoutId = workoutId });
            }
            CurrentLog.Instance.IsReconfigration = true;
            Xamarin.Forms.MessagingCenter.Send<RecongrationtMessage>(new RecongrationtMessage(), "RecongrationtMessage");
            
            await Navigation.PopAsync();
            
            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[0];
            await PagesFactory.PopToRootAsync();
            
        }

        private void SetupEquipment(UserInfosModel uim)
        {
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
            var pancakeView = new PancakeView() {  HeightRequest = 50, Margin = new Thickness(25, 2) };
            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 0 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 1 });

            grid.Children.Add(pancakeView);


            var btn = new DrMuscleButton()
            {
                Text = title,
                TextColor = Color.Black,
                BackgroundColor = Color.White,
                FontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 17,
                CornerRadius = 0,
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
    }
}
