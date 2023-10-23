using Acr.UserDialogs;
using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Xamarin.Forms;
using DrMuscle.Dependencies;
using DrMuscle.Entity;
using DrMuscle.Layout;
using DrMuscle.Helpers;
using DrMuscle.Screens.User.OnBoarding;
using DrMuscle.Constants;
using DrMuscle.Resx;
using Plugin.Connectivity;
using Plugin.GoogleClient;
using Plugin.GoogleClient.Shared;
using DrMuscle.Services;
using DrMuscle.Screens.Demo;
using Xamarin.Essentials;
using Newtonsoft.Json;
using System.Globalization;
using DrMuscle.Models;
using Sentry;

namespace DrMuscle.Screens.User
{
    public partial class WelcomePage : DrMusclePage
    {
        IFacebookManager _manager;
        internal readonly IGoogleClientManager _googleClientManager;
        private IAlarmAndNotificationService alarmAndNotificationService;
        private IAppleSignInService appleSignInService;
        private async void CreateNewAccountButton_Clicked(object sender, EventArgs e)
        {
            //MoveToDemo();
            //await PagesFactory.PopToRootAsync();
            //await PagesFactory.PushAsync<MainOnboardingPage>();
            try
            {
                ((App)Application.Current).displayCreateNewAccount = true;
                //await PagesFactory.PushAsync<RegistrationPage>(true);
                RegistrationPage page = new RegistrationPage();
                page.OnBeforeShow();
                Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {

            }
        }

        public WelcomePage()
        {
            InitializeComponent();
            RefreshLocalized();
            HasSlideMenu = false;
            _manager = DependencyService.Get<IFacebookManager>();
            alarmAndNotificationService = new AlarmAndNotificationService();
            _googleClientManager = CrossGoogleClient.Current;
            //LoginButton.Clicked += LoginButton_Clicked;
            ResetPasswordButton.Clicked += ResetPasswordButton_Clicked;
            MadeAMistakeButton.Clicked += MadeAMistakeButton_Clicked;
            //LoginWithFBButton.Clicked += LoginWithFBButton_Clicked;
            //LoginWithGoogleButton.Clicked += LoginWithGoogleAsync;
            CreateNewAccountButton.Clicked += CreateNewAccountButton_Clicked;
            NavigationPage.SetHasNavigationBar(this, false);
            DependencyService.Get<IDrMuscleSubcription>().Init();

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
            PrivacyPolicy.GestureRecognizers.Add(tapLinkPrivacyPolicyGestureRecognizer);
            //Live
            //SendBirdClient.Init("91658003-270F-446B-BD61-0043FAA8D641");

            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });

            appleSignInService = DependencyService.Get<IAppleSignInService>();
            if (appleSignInService != null)
            {
                if (appleSignInService.IsAvailable)
                {

                    BtnAppleSignIn.IsVisible = true;
                    //BtnAppleSignIn.Clicked += LoginWithAppleAsync;

                }
            }
        }

        private void RefreshLocalized()
        {
            MadeAMistakeButton.Text = AppResources.MadeAMistakeStartOver;
            LblBackUpAutomatically.Text = AppResources.BackupAutomaticallyAccessAnywhere;
            EmailEntry.Placeholder = AppResources.TapToEnterYourEmail;
            PasswordEntry.Placeholder = AppResources.TapToEnterYourPassword;
            LblPasswordText.Text = AppResources.SixCharactersOrLonger;
            LoginButton.Text = AppResources.LogIn;
            ResetPasswordButton.Text = AppResources.ForgotPassword;
            CreateNewAccountButton.Text = AppResources.CreateNewAccount;
            ByContinueAgree.Text = AppResources.ByContinuingYouAgreeToOur;
            TermsOfUse.Text = AppResources.TermsOfUseLower;
            LblAnd.Text = AppResources.And;
            PrivacyPolicy.Text = AppResources.PrivacyPolicy;
        }
        protected override bool OnBackButtonPressed()
        {
            DBSetting dbToken = LocalDBManager.Instance.GetDBSetting("token");

            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //var result = await DisplayAlert("Exit", "Are you sure you want to Exit?", "YES", "NO");
            //if (result)
            //{
            if (dbToken == null && ((App)Application.Current).displayCreateNewAccount)
            {
                var kill = DependencyService.Get<IKillAppService>();
                kill.ExitApp();
            }
            //}
            //});
            return true;
        }

        private async void LoginWithFBButton_Clicked(object sender, EventArgs e)
        {
            App.IsDemoProgress = false;
            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
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
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = "Your Facebook account is not connected with email (or we do not have permission to access it). Please sign up with email.",
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });

                return;
            }
            LocalDBManager.Instance.SetDBSetting("FBId", FBId);
            LocalDBManager.Instance.SetDBSetting("FBEmail", FBEmail);
            LocalDBManager.Instance.SetDBSetting("FBGender", FBGender);
            LocalDBManager.Instance.SetDBSetting("FBToken", FBToken);
            var url = $"http://graph.facebook.com/{FBId}/picture?type=square";
            LocalDBManager.Instance.SetDBSetting("ProfilePic", url);
            BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = FBEmail });

            //Log in d'un compte existant avec Facebook

            try
            {
                if (existingUser != null && !existingUser.Result)
                {
                    App.IsNewUser = true;

                    if (LocalDBManager.Instance.GetDBSetting("ReadyToSignup") != null)
                    {
                        //New Register here:
                        RegisterModel registerModel = JsonConvert.DeserializeObject<RegisterModel>(LocalDBManager.Instance.GetDBSetting("ReadyRegisterModel").Value);

                        try
                        {

                            string mass = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                            string body = null;
                            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                                body = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg").Kg.ToString();
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

                                    registerModel.Firstname = firstname;
                                    registerModel.EmailAddress = FBEmail;
                                    registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                                    registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;


                                    uim = await DrMuscleRestClient.Instance.RegisterWithUser(registerModel);
                                }
                                SentrySdk.ConfigureScope(scope =>
                                {
                                    scope.User = new Sentry.User
                                    {
                                        Email = LocalDBManager.Instance.GetDBSetting("email")?.Value
                                    };
                                });
                                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                                LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                                LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                                LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
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
                                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("1By1Side",uim.Is1By1Side ? "true" : "false");

                                LocalDBManager.Instance.SetDBSetting("RecommendedReminder", uim.IsRecommendedReminder == true ? "true" : uim.IsRecommendedReminder == null ? "null" : "false");

                                if (uim.Height != null)
                                    LocalDBManager.Instance.SetDBSetting("Height", uim.Height.ToString());
                                if (uim.TargetIntake != null)
                                    LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                                else
                                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());
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
                                    if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilableDumbbell))
                                    {
                                        LocalDBManager.Instance.SetDBSetting("DumbbellKg", uim.EquipmentModel.AvilableDumbbell);
                                        LocalDBManager.Instance.SetDBSetting("HomeDumbbellKg", uim.EquipmentModel.AvilableHomeDumbbell);
                                        LocalDBManager.Instance.SetDBSetting("OtherDumbbellKg", uim.EquipmentModel.AvilableOtherDumbbell);

                                        LocalDBManager.Instance.SetDBSetting("DumbbellLb", uim.EquipmentModel.AvilableLbDumbbell);
                                        LocalDBManager.Instance.SetDBSetting("HomeDumbbellLb", uim.EquipmentModel.AvilableHomeLbDumbbell);
                                        LocalDBManager.Instance.SetDBSetting("OtherDumbbellLb", uim.EquipmentModel.AvilableHomeLbDumbbell);
                                    }
                                    else
                                    {
                                        var kgString = "50_2_True|47.5_2_True|45_2_True|42.5_2_True|40_2_True|37.5_2_True|35_2_True|32.5_2_True|30_2_True|27.5_2_True|25_2_True|22.5_2_True|20_2_True|17.5_2_True|15_2_True|12.5_2_True|10_2_True|7.5_2_True|5_2_True|2.5_2_True|1_2_True";
                                        LocalDBManager.Instance.SetDBSetting("DumbbellKg", kgString);
                                        LocalDBManager.Instance.SetDBSetting("HomeDumbbellKg", kgString);
                                        LocalDBManager.Instance.SetDBSetting("OtherDumbbellKg", kgString);

                                        var lbString = "90_2_True|85_2_True|80_2_True|75_2_True|70_2_True|65_2_True|60_2_True|55_2_True|50_2_True|45_2_True|40_2_True|35_2_True|30_2_True|25_2_True|20_2_True|15_2_True|12_2_True|10_2_True|8_2_True|5_2_True|3_2_True|2_2_True";
                                        LocalDBManager.Instance.SetDBSetting("DumbbellLb", lbString);
                                        LocalDBManager.Instance.SetDBSetting("HomeDumbbellLb", lbString);
                                        LocalDBManager.Instance.SetDBSetting("OtherDumbbellLb", lbString);
                                    }
                                    if (string.IsNullOrEmpty(uim.EquipmentModel.AvilablePlate))
                                    {
                                        if (LocalDBManager.Instance.GetDBSetting("PlatesKg") == null || LocalDBManager.Instance.GetDBSetting("PlatesLb") == null)
                                        {
                                            var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                                            LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);
                                            LocalDBManager.Instance.SetDBSetting("HomePlatesKg", kgString);
                                            LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", kgString);

                                            var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                                            LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
                                            LocalDBManager.Instance.SetDBSetting("HomePlatesLb", lbString);
                                            LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", lbString);
                                        }
                                        if (LocalDBManager.Instance.GetDBSetting("HomePlatesKg") == null || LocalDBManager.Instance.GetDBSetting("HomePlatesLb") == null)
                                        {
                                            var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                                            LocalDBManager.Instance.SetDBSetting("HomePlatesKg", kgString);
                                            LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", kgString);

                                            var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                                            LocalDBManager.Instance.SetDBSetting("HomePlatesLb", lbString);
                                            LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", lbString);
                                        }

                                        
                                    }
                                    else
                                    {
                                        
                                            var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                                            LocalDBManager.Instance.SetDBSetting("PlatesKg", uim.EquipmentModel.AvilablePlate);
                                            LocalDBManager.Instance.SetDBSetting("HomePlatesKg", uim.EquipmentModel.AvilableHomePlate);
                                            LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", uim.EquipmentModel.AvilableOtherPlate);

                                            var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                                            LocalDBManager.Instance.SetDBSetting("PlatesLb", uim.EquipmentModel.AvilableLbPlate);
                                            LocalDBManager.Instance.SetDBSetting("HomePlatesLb", uim.EquipmentModel.AvilableHomeLbPlate);
                                            LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", uim.EquipmentModel.AvilableHomeLbPlate);
                                        
                                    }
                                    if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilablePulley))
                                    {

                                        LocalDBManager.Instance.SetDBSetting("PulleyKg", uim.EquipmentModel.AvilablePulley);
                                        LocalDBManager.Instance.SetDBSetting("HomePulleyKg", uim.EquipmentModel.AvilableHomePulley);
                                        LocalDBManager.Instance.SetDBSetting("OtherPulleyKg", uim.EquipmentModel.AvilableOtherPulley);


                                        LocalDBManager.Instance.SetDBSetting("PulleyLb", uim.EquipmentModel.AvilableLbPulley);
                                        LocalDBManager.Instance.SetDBSetting("HomePulleyLb", uim.EquipmentModel.AvilableHomeLbPulley);
                                        LocalDBManager.Instance.SetDBSetting("OtherPulleyLb", uim.EquipmentModel.AvilableOtherLbPulley);
                                    }
                                    else
                                    {
                                        var kgString = "5_20_True|1.5_2_True";
                                        var lbString = "10_20_True|5_2_True|2.5_2_True";
                                        LocalDBManager.Instance.SetDBSetting("PulleyKg", kgString);
                                        LocalDBManager.Instance.SetDBSetting("HomePulleyKg", kgString);
                                        LocalDBManager.Instance.SetDBSetting("OtherPulleyKg", kgString);


                                        LocalDBManager.Instance.SetDBSetting("PulleyLb", lbString);
                                        LocalDBManager.Instance.SetDBSetting("HomePulleyLb", lbString);
                                        LocalDBManager.Instance.SetDBSetting("OtherPulleyLb", lbString);
                                    }
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
                                ((App)Application.Current).displayCreateNewAccount = true;

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
                                CancelNotification();
                                return;
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
                                Message = "We are facing problem to signup with your facebook account. Please sign up with email.",
                                Title = AppResources.Error,
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                            });

                        }
                    }
                    else
                    {
                        UserDialogs.Instance.Alert(new AlertConfig()
                        {
                            Message = $"Account not exist with {FBEmail} email, please create account or login with existing account",
                            Title = AppResources.UnableToLogIn,
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                        });
                        return;
                    }
                }
                else
                {
                    LoginSuccessResult lr = await DrMuscleRestClient.Instance.FacebookLogin(FBToken, null, null);
                    if (lr != null)
                    {
                        DateTime current = DateTime.Now;
                        CancelNotification();
                        UserInfosModel uim = await DrMuscleRestClient.Instance.GetUserInfo();

                        LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                        LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                        LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                        LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                        LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                        LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                        LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                        LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                        LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                        LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false");
                        LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                        LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                        LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                        if (uim.Age != null)
                            LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                        LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Reminder5th", uim.IsReminder ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("LastWorkoutWas", uim.LastWorkoutWas);
                        if (uim.Height != null)
                            LocalDBManager.Instance.SetDBSetting("Height", uim.Height.ToString());
                        if (uim.TargetIntake != null)
                            LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
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
                        LocalDBManager.Instance.SetDBSetting("IsEmailReminder", uim.IsReminderEmail ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("ReminderHours", uim.ReminderBeforeHours.ToString());
                        if (uim.ReminderTime != null)
                            LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                        if (uim.ReminderDays != null)
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);
                        if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                            LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                        else
                            LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());
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
                        if (uim.WarmupsValue != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                        }
                        if (uim.SetCount != null)
                        {
                            LocalDBManager.Instance.SetDBSetting("WorkSetCount", Convert.ToString(uim.SetCount));
                        }
                        ((App)Application.Current).displayCreateNewAccount = true;

                        if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                        else
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");

                        await PagesFactory.PopToRootAsync(true);
                        MessagingCenter.Send(this, "BackgroundImageUpdated");
                        await PagesFactory.PushAsync<MainAIPage>();
                        App.RegisterDeviceToken();
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
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = "We are facing problem to signup with your facebook account. Please sign up with email.",
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });

            }

        }
        public async void LoginWithAppleAsync(object sender, EventArgs ee)
        {
            //OnLoginComplete(new GoogleUser() { Email = "8h6sznjjjz@privaterelay.appleid.com", Name = "", Picture = null }, "");
            //return;
            // await SecureStorage.SetAsync("Email", "jigneshbodarya@gmail.com");
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

                    //appleSignInService.GetCredentialStateAsync("001913.30e6f5c9e107444788e562b93a1744d5.0404");
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        Message = "We haven't get email. Please login with email.",
                        Title = AppResources.Error,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });

                    return;
                }

                BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = account.Email });
                if (existingUser != null && !existingUser.Result)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        Message = "No account found with given email, Please create new account to login.",
                        Title = AppResources.Error,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });

                    return;
                }
                OnLoginComplete(new GoogleUser() { Email = account.Email, Name = account.Name, Picture = null }, "");
            }
        }
        public async void LoginWithGoogleAsync(object sender, EventArgs ee)
        {
            App.IsDemoProgress = false;
            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");


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

        private async void OnLoginComplete(GoogleUser googleUser, string message)
        {
            if (googleUser != null)
            {


                LocalDBManager.Instance.SetDBSetting("GToken", "");
                if (googleUser.Picture != null)
                    LocalDBManager.Instance.SetDBSetting("ProfilePic", googleUser.Picture.OriginalString);

                BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = googleUser.Email });
                if (existingUser != null && existingUser.Result)
                {
                    LoginSuccessResult lr = await DrMuscleRestClient.Instance.GoogleLogin("", googleUser.Email, googleUser.Name, null, null);
                    if (lr != null)
                    {
                        UserInfosModel uim = null;

                        uim = await DrMuscleRestClient.Instance.GetUserInfo();
                        CancelNotification();
                        try
                        {
                            SentrySdk.ConfigureScope(scope =>
                            {
                                scope.User = new Sentry.User
                                {
                                    Email = LocalDBManager.Instance.GetDBSetting("email")?.Value
                                };
                            });
                            
                            LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                            LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                            LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                            LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                            LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                            LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                            LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                            LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                            LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                            LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                            LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                            LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                            LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false");
                            LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                            LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                            LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");

                            LocalDBManager.Instance.SetDBSetting("DailyReset", Convert.ToString(uim.DailyExerciseCount));
                            LocalDBManager.Instance.SetDBSetting("WeeklyReset", Convert.ToString(uim.WeeklyExerciseCount));

                            LocalDBManager.Instance.SetDBSetting("IsEmailReminder", uim.IsReminderEmail ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("ReminderHours", uim.ReminderBeforeHours.ToString());
                            if (uim.ReminderTime != null)
                                LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                            if (uim.ReminderDays != null)
                                LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);
                            if (uim.Age != null)
                                LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                            LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                            LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                            LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("Reminder5th", uim.IsReminder ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("LastWorkoutWas", uim.LastWorkoutWas);
                            LocalDBManager.Instance.SetDBSetting("IsMobility", uim.IsMobility == null ? null : uim.IsMobility == false ? "false" : "true");
                            LocalDBManager.Instance.SetDBSetting("MaxWorkoutDuration", uim.WorkoutDuration.ToString());
                            LocalDBManager.Instance.SetDBSetting("IsExerciseQuickMode", uim.IsExerciseQuickMode == null ? null : uim.IsExerciseQuickMode == false ? "false" : "true");
                            LocalDBManager.Instance.SetDBSetting("MobilityLevel", uim.MobilityLevel);
                            LocalDBManager.Instance.SetDBSetting("MobilityRep", uim.MobilityRep == null ? "" : Convert.ToString(uim.MobilityRep));
                            SetupEquipment(uim);
                            if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                                LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                            else
                                LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

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
                            if (uim.WarmupsValue != null)
                            {
                                LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                            }
                            if (uim.SetCount != null)
                            {
                                LocalDBManager.Instance.SetDBSetting("WorkSetCount", Convert.ToString(uim.SetCount));
                            }

                        ((App)Application.Current).displayCreateNewAccount = true;

                            if (uim.Height != null)
                                LocalDBManager.Instance.SetDBSetting("Height", uim.Height.ToString());
                            if (uim.TargetIntake != null)
                                LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                            LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                            LocalDBManager.Instance.SetDBSetting("RecommendedReminder", uim.IsRecommendedReminder == true ? "true" : uim.IsRecommendedReminder == null ? "null" : "false");
                            await PagesFactory.PopToRootAsync(true);
                            App.RegisterDeviceToken();
                            MessagingCenter.Send(this, "BackgroundImageUpdated");
                            //await PagesFactory.PushAsync<MainAIPage>();


                        }
                        catch (Exception ex)
                        {

                        }
                        try
                        {
                            DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                            if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays < 14)
                            {
                                LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                                App.IsV1UserTrial = true;
                                SetTrialUserNotifications();
                            }
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
                    //New user
                    if (LocalDBManager.Instance.GetDBSetting("ReadyToSignup") != null)
                    {
                        string mass = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        string body = null;
                        if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            body = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), mass).Kg.ToString();
                        var token = CrossGoogleClient.Current.AccessToken;
                        LoginSuccessResult lr = await DrMuscleRestClient.Instance.GoogleLogin(token, googleUser.Email, googleUser.Name, body, mass);
                        if (lr != null)
                        {
                            UserInfosModel uim = null;
                            if (existingUser.Result)
                            {
                                uim = await DrMuscleRestClient.Instance.GetUserInfo();
                            }
                            else
                            {
                                RegisterModel registerModel = JsonConvert.DeserializeObject<RegisterModel>(LocalDBManager.Instance.GetDBSetting("ReadyRegisterModel").Value);
                                registerModel.Firstname = googleUser.Name;
                                registerModel.EmailAddress = googleUser.Email;
                                registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                                registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;

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
                                LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                                LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                                LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
                                LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
                                LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                                if (uim.Age != null)
                                    LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));

                                LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                                LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                                LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                                if (uim.Height != null)
                                    LocalDBManager.Instance.SetDBSetting("Height", uim.Height.ToString());
                                if (uim.TargetIntake != null)
                                    LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
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
                                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                                else
                                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

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
                                if (uim.WarmupsValue != null)
                                {
                                    LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
                                }
                                LocalDBManager.Instance.SetDBSetting("IsMobility", uim.IsMobility == null ? null : uim.IsMobility == false ? "false" : "true");
                                LocalDBManager.Instance.SetDBSetting("MaxWorkoutDuration", uim.WorkoutDuration.ToString());
                                LocalDBManager.Instance.SetDBSetting("IsExerciseQuickMode", uim.IsExerciseQuickMode == null ? null : uim.IsExerciseQuickMode == false ? "false" : "true");
                                LocalDBManager.Instance.SetDBSetting("MobilityLevel", uim.MobilityLevel);
                                LocalDBManager.Instance.SetDBSetting("MobilityRep", uim.MobilityRep == null ? "" : Convert.ToString(uim.MobilityRep));
                                SetupEquipment(uim);

                                ((App)Application.Current).displayCreateNewAccount = true;

                                if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                                else
                                    LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");



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
                                CancelNotification();
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
                }
            }
            else
            {
                UserDialogs.Instance.Alert(new AlertConfig()
                {
                    Message = message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });

            }
        }


        private void SetupEquipment(UserInfosModel uim)
        {
            LocalDBManager.Instance.SetDBSetting("KgBarWeight", uim.KgBarWeight == null ? "20" : Convert.ToString(uim.KgBarWeight).ReplaceWithDot());
            LocalDBManager.Instance.SetDBSetting("LbBarWeight", uim.LbBarWeight == null ? "45" : Convert.ToString(uim.LbBarWeight).ReplaceWithDot());
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

                if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilableDumbbell))
                {
                    LocalDBManager.Instance.SetDBSetting("DumbbellKg", uim.EquipmentModel.AvilableDumbbell);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellKg", uim.EquipmentModel.AvilableHomeDumbbell);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellKg", uim.EquipmentModel.AvilableOtherDumbbell);

                    LocalDBManager.Instance.SetDBSetting("DumbbellLb", uim.EquipmentModel.AvilableLbDumbbell);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellLb", uim.EquipmentModel.AvilableHomeLbDumbbell);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellLb", uim.EquipmentModel.AvilableHomeLbDumbbell);
                }
                else
                {
                    var kgString = "50_2_True|47.5_2_True|45_2_True|42.5_2_True|40_2_True|37.5_2_True|35_2_True|32.5_2_True|30_2_True|27.5_2_True|25_2_True|22.5_2_True|20_2_True|17.5_2_True|15_2_True|12.5_2_True|10_2_True|7.5_2_True|5_2_True|2.5_2_True|1_2_True";
                    LocalDBManager.Instance.SetDBSetting("DumbbellKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellKg", kgString);

                    var lbString = "90_2_True|85_2_True|80_2_True|75_2_True|70_2_True|65_2_True|60_2_True|55_2_True|50_2_True|45_2_True|40_2_True|35_2_True|30_2_True|25_2_True|20_2_True|15_2_True|12_2_True|10_2_True|8_2_True|5_2_True|3_2_True|2_2_True";
                    LocalDBManager.Instance.SetDBSetting("DumbbellLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("HomeDumbbellLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("OtherDumbbellLb", lbString);
                }
                if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilablePlate))
                {
                    LocalDBManager.Instance.SetDBSetting("PlatesKg", uim.EquipmentModel.AvilablePlate);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesKg", uim.EquipmentModel.AvilableHomePlate);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", uim.EquipmentModel.AvilableOtherPlate);

                    LocalDBManager.Instance.SetDBSetting("PlatesLb", uim.EquipmentModel.AvilableLbPlate);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesLb", uim.EquipmentModel.AvilableHomeLbPlate);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", uim.EquipmentModel.AvilableHomeLbPlate);
                }
                else
                {
                    var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                    LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", kgString);

                    var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                    LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("HomePlatesLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", lbString);
                }

                if (!string.IsNullOrEmpty(uim.EquipmentModel.AvilablePulley))
                {

                    LocalDBManager.Instance.SetDBSetting("PulleyKg", uim.EquipmentModel.AvilablePulley);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyKg", uim.EquipmentModel.AvilableHomePulley);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyKg", uim.EquipmentModel.AvilableOtherPulley);


                    LocalDBManager.Instance.SetDBSetting("PulleyLb", uim.EquipmentModel.AvilableLbPulley);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyLb", uim.EquipmentModel.AvilableHomeLbPulley);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyLb", uim.EquipmentModel.AvilableOtherLbPulley);
                }
                else
                {

                    var kgString = "5_20_True|1.5_2_True";
                    var lbString = "10_20_True|5_2_True|2.5_2_True";

                    LocalDBManager.Instance.SetDBSetting("PulleyKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyKg", kgString);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyKg", kgString);

                    
                    LocalDBManager.Instance.SetDBSetting("PulleyLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("HomePulleyLb", lbString);
                    LocalDBManager.Instance.SetDBSetting("OtherPulleyLb", lbString);
                }
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

        private void SetTrialUserNotifications()
        {
            try
            {

                CancelNotification();
                var fName = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                var dt = DateTime.Now.AddDays(2);
                var timeSpan = new TimeSpan(2, dt.Hour, dt.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(10).Day - DateTime.Now.Day, dt.Hour, dt.Minute, 0);
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"{fName}, you can do this!", timeSpan, 1451);

                var dt1 = DateTime.Now.AddDays(4);
                var timeSpan1 = new TimeSpan(4, dt1.Hour, dt1.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(15).Day - DateTime.Now.Day, dt1.Hour, dt1.Minute, 0);//// 
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", Device.RuntimePlatform.Equals(Device.Android) ? $"New users like you improve 34% in 30 days" : $"New users like you improve 34%% in 30 days", timeSpan1, 1551);

                var dt2 = DateTime.Now.AddDays(10);
                var timeSpan2 = new TimeSpan(10, dt2.Hour, dt2.Minute, 0);//  new TimeSpan(DateTime.Now.AddMinutes(20).Day - DateTime.Now.Day, dt2.Hour, dt2.Minute, 0);//// 
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"You're 12 seconds away from custom, smart workouts", timeSpan2, 1651);
            }
            catch (Exception ex)
            {

            }
        }

        private async void OnLoginCompleted(object sender, GoogleClientResultEventArgs<GoogleUser> loginEventArgs)
        {
            if (loginEventArgs.Data != null)
            {
                GoogleUser googleUser = loginEventArgs.Data;
                UserProfile user = new UserProfile();
                user.Name = googleUser.Name;
                user.Email = googleUser.Email;
                user.Picture = googleUser.Picture;
                var GivenName = googleUser.GivenName;
                var FamilyName = googleUser.FamilyName;
                var token = CrossGoogleClient.Current.AccessToken;

                OnLoginComplete(googleUser, "");

                //Token = token;
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

        private void MoveToDemo()
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
            CancelNotification();


        }

        private async void MadeAMistakeButton_Clicked(object sender, EventArgs e)
        {
            //MoveToDemo();
            LocalDBManager.Instance.Reset();
            await PagesFactory.PushAsync<MainOnboardingPage>();
        }
        private async void ResetPasswordButton_Clicked(object sender, EventArgs e)
        {
            string email = "";
            if (string.IsNullOrEmpty(EmailEntry.Text))
            {
                PromptConfig p = new PromptConfig()
                {
                    InputType = InputType.Default,
                    IsCancellable = true,
                    Title = AppResources.PasswordReset,
                    Placeholder = AppResources.EnterYourEmail,
                    OkText = AppResources.Ok,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OnAction = new Action<PromptResult>(async (PromptResult obj) =>
                    {
                        if (obj.Ok && !string.IsNullOrEmpty(obj.Text))
                        {
                            email = obj.Text;
                            await ResetPassword(email);
                        }
                    })
                };

                UserDialogs.Instance.Prompt(p);
            }
            else
            {
                email = EmailEntry.Text;
                await ResetPassword(email);
            }
        }

        private async Task ResetPassword(string email)
        {
            try
            {
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Title = AppResources.ConnectionError,
                        Message = AppResources.PleaseCheckInternetConnection,
                        OkText = AppResources.Ok
                    });
                    return;
                }
                BooleanModel response = await DrMuscleRestClient.Instance.ForgotPassword(new ForgotPasswordModel() { Email = email });
                if (response.Result)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Title = AppResources.CheckYourMail,
                        Message = string.Format("{0} {1} {2}", AppResources.PleaseCheckYourEmail, email, AppResources.ToRestYourPassword),
                        OkText = AppResources.Ok
                    });

                }
                else
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Title = AppResources.EmailNotFound,
                        Message = AppResources.CanYouTryAnotherLoginEmail,
                        OkText = AppResources.Ok
                    });

                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Title = AppResources.EmailNotFound,
                    Message = AppResources.CanYouTryAnotherLoginEmail,
                    OkText = AppResources.Ok
                });


            }
        }

        private async void LoginButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                App.IsDemoProgress = false;
                LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                if (string.IsNullOrEmpty(EmailEntry.Text) || string.IsNullOrEmpty(PasswordEntry.Text))
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.EmailPasswordEmptyError,
                        Title = AppResources.UnableToLogIn,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                    return;
                }

                if (!Emails.ValidateEmail(EmailEntry.Text?.ToLowerInvariant()))
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.InvalidEmailError,
                        Title = AppResources.InvalidEmailAddress,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });

                    return;
                }

                if (PasswordEntry.Text.Length < 6)
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.PasswordLengthError,
                        Title = AppResources.UnableToLogIn,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });

                    return;
                }

                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Title = AppResources.ConnectionError,
                        Message = AppResources.InternetConnectionProblem,
                        OkText = AppResources.Ok
                    });


                    return;
                }
                CancelNotification();
                BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = EmailEntry.Text });
                App.IsNewUser = false;
                if (existingUser != null)
                {

                    if (!existingUser.Result)
                    {
                        App.IsNewUser = true;
                        LocalDBManager.Instance.SetDBSetting("password", PasswordEntry.Text);
                        LocalDBManager.Instance.SetDBSetting("email", EmailEntry.Text);
                        if (
                            LocalDBManager.Instance.GetDBSetting("email") != null && LocalDBManager.Instance.GetDBSetting("ReadyToSignup") != null)
                        {

                            //Create a new account?
                            FinishSignup();
                            //RegisterModel registerModel = new RegisterModel();

                            //registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                            //registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
                            //registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
                            //registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                            //registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                            //registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
                            //registerModel.Password = PasswordEntry.Text;
                            //registerModel.ConfirmPassword = PasswordEntry.Text;
                            //if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null)
                            //{
                            //    var increments = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value, System.Globalization.CultureInfo.InvariantCulture);
                            //    var incrementsWeight = new MultiUnityWeight(increments, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                            //    registerModel.Increments = incrementsWeight.Kg;
                            //}

                            //BooleanModel registerResponse = await DrMuscleRestClient.Instance.RegisterUser(registerModel);
                            //if (registerResponse.Result)
                            //{
                            //DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                            //}
                            return;
                        }
                    }

                    // Log d'un compte existant avec email

                    LoginSuccessResult lr = await DrMuscleRestClient.Instance.Login(new LoginModel()
                    {
                        Username = EmailEntry.Text,
                        Password = PasswordEntry.Text
                    });

                    if (lr != null && !string.IsNullOrEmpty(lr.access_token))
                    {
                        DateTime current = DateTime.Now;

                        UserInfosModel uim = await DrMuscleRestClient.Instance.GetUserInfo();

                        LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                        SentrySdk.ConfigureScope(scope =>
                        {
                            scope.User = new Sentry.User
                            {
                                Email = LocalDBManager.Instance.GetDBSetting("email")?.Value
                            };
                        });
                        LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                        LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                        LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                        LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                        LocalDBManager.Instance.SetDBSetting("password", PasswordEntry.Text);
                        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                        LocalDBManager.Instance.SetDBSetting("token_expires_date", current.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
                        LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
                        LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
                        LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
                        LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
                        LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
                        LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
                        LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("Reminder5th", uim.IsReminder ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("LastWorkoutWas", uim.LastWorkoutWas);

                        LocalDBManager.Instance.SetDBSetting("IsMobility", uim.IsMobility == null ? null : uim.IsMobility == false ? "false" : "true");
                        LocalDBManager.Instance.SetDBSetting("MaxWorkoutDuration", uim.WorkoutDuration.ToString());
                        LocalDBManager.Instance.SetDBSetting("DailyReset", Convert.ToString(uim.DailyExerciseCount));
                        LocalDBManager.Instance.SetDBSetting("WeeklyReset", Convert.ToString(uim.WeeklyExerciseCount));
                        LocalDBManager.Instance.SetDBSetting("IsEmailReminder", uim.IsReminderEmail ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("ReminderHours", uim.ReminderBeforeHours.ToString());
                        LocalDBManager.Instance.SetDBSetting("IsExerciseQuickMode", uim.IsExerciseQuickMode == null ? null : uim.IsExerciseQuickMode == false ? "false" : "true");
                        LocalDBManager.Instance.SetDBSetting("MobilityLevel", uim.MobilityLevel);
                        LocalDBManager.Instance.SetDBSetting("MobilityRep", uim.MobilityRep == null ? "" : Convert.ToString(uim.MobilityRep));
                        SetupEquipment(uim);
                        LocalDBManager.Instance.SetDBSetting("IsEmailReminder", uim.IsReminderEmail ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("ReminderHours", uim.ReminderBeforeHours.ToString());
                        if (uim.Age != null)
                            LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
                        if (uim.ReminderTime != null)
                            LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                        if (uim.ReminderDays != null)
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);
                        LocalDBManager.Instance.SetDBSetting("RecommendedReminder", uim.IsRecommendedReminder == true ? "true" : uim.IsRecommendedReminder == null ? "null" : "false");

                        if (!string.IsNullOrEmpty(uim.SwappedJson))
                        {
                            LocalDBManager.Instance.SetDBSetting("swap_exericse_contexts", uim.SwappedJson);
                            ((App)Application.Current).swapExerciseContexts = SwapExerciseContextList.LoadContexts();
                        }
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
                                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 101, NotificationInterval.Week);
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
                                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 102, NotificationInterval.Week);
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
                                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 103, NotificationInterval.Week);
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
                                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 104, NotificationInterval.Week);
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
                                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 105, NotificationInterval.Week);
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
                                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 106, NotificationInterval.Week);
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
                                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 107, NotificationInterval.Week);
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
                        LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                        if (uim.Height != null)
                            LocalDBManager.Instance.SetDBSetting("Height", uim.Height.ToString());
                        if (uim.TargetIntake != null)
                            LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                        LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                        ((App)Application.Current).displayCreateNewAccount = true;

                        if (uim.Gender.Trim().ToLowerInvariant().Equals("man"))
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "Background2.png");
                        else
                            LocalDBManager.Instance.SetDBSetting("BackgroundImage", "BackgroundFemale.png");
                        MainAIPage._isJustAppOpen = true;
                        await PagesFactory.PopToRootAsync(true);
                        MessagingCenter.Send(this, "BackgroundImageUpdated");
                        try
                        {
                            DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                            if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays < 14)
                            {
                                LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                                App.IsV1UserTrial = true;
                                SetTrialUserNotifications();
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        App.RegisterDeviceToken();
                        //await Navigation.PushAsync(new MainPage(!existingUser.Result), false);
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
                    LoginButton_Clicked(sender, e);
                }

            }
            catch (Exception ex)
            {
                #if DEBUG
                    UserDialogs.Instance.AlertAsync(ex.Message);
                #endif  
            }
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


                LocalDBManager.Instance.SetDBSetting("firstname", response.Text);


                FinishSignup();
            }
        }

        private async void FinishSignup()
        {
            if (LocalDBManager.Instance.GetDBSetting("firstname") == null || LocalDBManager.Instance.GetDBSetting("firstname").Value == null)
            {
                GetFirstName();
                return;
            }
            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");

            //Setup Program
            RegisterModel registerModel = JsonConvert.DeserializeObject<RegisterModel>(LocalDBManager.Instance.GetDBSetting("ReadyRegisterModel").Value);
            registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.Password = PasswordEntry.Text;
            registerModel.ConfirmPassword = PasswordEntry.Text;
            BooleanModel registerResponse = await DrMuscleRestClient.Instance.RegisterUser(registerModel);
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
                LocalDBManager.Instance.SetDBSetting("reprangeType", uim.ReprangeType.ToString());
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

                LocalDBManager.Instance.SetDBSetting("IsMobility", uim.IsMobility == null ? null : uim.IsMobility == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("MaxWorkoutDuration", uim.WorkoutDuration.ToString());
                LocalDBManager.Instance.SetDBSetting("IsExerciseQuickMode", uim.IsExerciseQuickMode == null ? null : uim.IsExerciseQuickMode == false ? "false" : "true");
                LocalDBManager.Instance.SetDBSetting("MobilityLevel", uim.MobilityLevel);
                LocalDBManager.Instance.SetDBSetting("MobilityRep", uim.MobilityRep == null ? "" : Convert.ToString(uim.MobilityRep));

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
                SetupEquipment(uim);
                if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
                else
                    LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());
                LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                if (uim.Height != null)
                    LocalDBManager.Instance.SetDBSetting("Height", uim.Height.ToString());
                if (uim.TargetIntake != null)
                    LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
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
                                        Title = AppResources.ConnectionError,
                                        Message = AppResources.PleaseCheckInternetConnection,
                                        OkText = AppResources.Ok
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
                                PagesFactory.PushAsync<DemoPage>();
                                CancelNotification();
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

        public override void OnBeforeShow()
        {
            base.OnBeforeShow();

            CreateAccountStack.IsVisible = !((App)Application.Current).displayCreateNewAccount;
            CreateNewAccountButton.IsVisible = ((App)Application.Current).displayCreateNewAccount;
            LoginWithFBButton.Text = ((App)Application.Current).displayCreateNewAccount ? AppResources.LogInWithFacebook : AppResources.ConnectWithFacebook;
            LoginWithGoogleButton.Text = ((App)Application.Current).displayCreateNewAccount ? "Login with Google" : "Connect with Google";
            AppleBtnText.Text = ((App)Application.Current).displayCreateNewAccount ? "Sign in with Apple" : "Continue with Apple";
            //LblLoginText.Text = ((App)Application.Current).displayCreateNewAccount ? AppResources.LogInWithEmail : "";
            //LblLoginText.IsVisible = ((App)Application.Current).displayCreateNewAccount;
            LoginButton.Text = ((App)Application.Current).displayCreateNewAccount ? AppResources.LogInWithEmail : AppResources.CreateAccount;
            //if (((App)Application.Current).displayCreateNewAccount)
            //    SetDefaultButtonStyle(LoginButton);
            //else
            //    SetEmphasisButtonStyle(LoginButton);
            ResetPasswordButton.IsVisible = ((App)Application.Current).displayCreateNewAccount;
            MadeAMistakeButton.IsVisible = !((App)Application.Current).displayCreateNewAccount;
            LblPasswordText.IsVisible = !((App)Application.Current).displayCreateNewAccount;
            EmailEntry.Text = "";
            PasswordEntry.Text = "";
            PasswordEntry.Placeholder = ((App)Application.Current).displayCreateNewAccount ? AppResources.TapToEnterYourPassword : AppResources.TapToCreateYourPassword;
            DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");
            int? workoutId = null;
            int? programId = null;
            int? remainingWorkout = null;

            UserDialogs.Instance.HideLoading();
            if (experienceSetting != null && workoutPlaceSetting != null)
            {
                if (workoutPlaceSetting.Value == "gym")
                {

                    if (experienceSetting.Value == "less1year")
                    {
                        WorkoutInfo2.Text = "[Gym] Full-Body";
                        workoutId = 104;
                        programId = 10;
                        remainingWorkout = 18;
                    }
                    if (experienceSetting.Value == "1-3years")
                    {
                        WorkoutInfo2.Text = "[Gym] Upper-Body";
                        workoutId = 106;
                        programId = 15;
                        remainingWorkout = 32;
                    }
                    if (experienceSetting.Value == "more3years")
                    {
                        WorkoutInfo2.Text = "[Gym] Upper-Body Level 2";
                        workoutId = 424;
                        programId = 16;
                        remainingWorkout = 40;
                    }
                }
                else if (workoutPlaceSetting.Value == "home")
                {
                    if (experienceSetting.Value == "less1year")
                    {
                        WorkoutInfo2.Text = "[Home] Full-Body";
                        workoutId = 108;
                        programId = 17;
                        remainingWorkout = 18;
                    }
                    if (experienceSetting.Value == "1-3years")
                    {
                        WorkoutInfo2.Text = "[Home] Upper-Body";
                        workoutId = 109;
                        programId = 21;
                        remainingWorkout = 24;
                    }
                    if (experienceSetting.Value == "more3years")
                    {
                        WorkoutInfo2.Text = "[Home] Upper-Body Level 2";
                        workoutId = 428;
                        programId = 22;
                        remainingWorkout = 20;
                    }
                }
                else if (workoutPlaceSetting.Value == "homeBodyweightOnly")
                {
                    WorkoutInfo2.Text = "Bodyweight Level 2";
                    workoutId = 12646;
                    programId = 487;
                    remainingWorkout = 12;
                }

                if (experienceSetting.Value == "beginner")
                {
                    WorkoutInfo2.Text = "Bodyweight Level 1";
                    workoutId = 12645;
                    programId = 488;
                    remainingWorkout = 6;
                }

                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", workoutId.ToString());
                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", WorkoutInfo2.Text);
                LocalDBManager.Instance.SetDBSetting("recommendedProgramId", programId.ToString());
                LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", remainingWorkout.ToString());

                string ProgramLabel = AppResources.NotSetUp;

                switch (programId)
                {
                    case 10:
                        ProgramLabel = "[Gym] Full-Body Level 1";
                        break;
                    case 15:
                        ProgramLabel = "[Gym] Up/Low Split Level 1";
                        break;
                    case 16:
                        ProgramLabel = "[Gym] Up/Low Split Level 2";
                        break;
                    case 17:
                        ProgramLabel = "[Home] Full-Body Level 1";
                        break;
                    case 21:
                        ProgramLabel = "[Home] Up/Low Split Level 1";
                        break;
                    case 22:
                        ProgramLabel = "[Home] Up/Low Split Level 2";
                        break;
                    case 487:
                        ProgramLabel = "Bodyweight Level 2";
                        break;
                    case 488:
                        ProgramLabel = "Bodyweight Level 1";
                        break;
                }
                LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", ProgramLabel);
            }
            //else
            //{

            WorkoutInfo1.Text = AppResources.WelcomeTo;
            WorkoutInfo2.FontSize = 16;
            WorkoutInfo2.Text = AppResources.DrMuslce;
            WorkoutInfo2.FontSize = 36;
            //}
            //else
            //{
            //    WorkoutInfo1.Text = string.Format("{0} {1}!", AppResources.Congratulations, LocalDBManager.Instance.GetDBSetting("firstname")?.Value);
            //    WorkoutInfo1.FontSize = 16;
            //    WorkoutInfo2.Text = AppResources.YourProgramIsReady;
            //    WorkoutInfo2.FontSize = 20;
            //}
            //}
        }

        void SetEmphasisButtonStyle(Button btn)
        {
            btn.TextColor = Color.White;
            btn.BackgroundColor = AppThemeConstants.BlueColor;
            btn.BorderWidth = 2;
            btn.CornerRadius = 0;
            btn.FontAttributes = FontAttributes.Bold;
        }

        void SetDefaultButtonStyle(Button btn)
        {
            btn.BackgroundColor = Color.Transparent;
            btn.BorderWidth = 2;
            btn.CornerRadius = 0;
            btn.FontAttributes = FontAttributes.Bold;
            btn.BorderColor = AppThemeConstants.BlueColor;
            btn.TextColor = AppThemeConstants.BlueColor;
        }

    }
}