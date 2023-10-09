using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Resx;
using DrMuscle.Screens.User.OnBoarding;
using DrMuscle.Views;
using DrMuscleWebApiSharedModel;
using Plugin.Connectivity;
using Plugin.GoogleClient.Shared;
using Plugin.GoogleClient;
using Sentry.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.Mime.MediaTypeNames;
using Application = Xamarin.Forms.Application;
using Browser = Xamarin.Essentials.Browser;
using DrMuscle.Entity;
using System.Globalization;
using Rg.Plugins.Popup.Services;
using System.Threading;
using DrMuscle.Dependencies;
using Device = Xamarin.Forms.Device;
using DrMuscle.Services;

namespace DrMuscle.Screens.User
{
	public partial class RegistrationPage : DrMusclePage
	{
        private bool isPasswordVisible = false;
        private  IGoogleClientManager _googleClientManager;
        IFacebookManager _manager;
        private IAppleSignInService appleSignInService;
        public RegistrationPage ()
		{
			InitializeComponent ();
            ClearFormValues();
            SetUIForSmallDensityDevices();
            //HasSlideMenu = false;
        }

        private void ClearFormValues()
        {
            EmailEntry.Text = "";
            PasswordEntry.Text = "";
            EmailValidator.IsVisible = false;
            PasswordValidator.IsVisible = false;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            ClearFormValues();
            
        }

        private void SetUIForSmallDensityDevices()
        {
            var dpi = DependencyService.Get<IDpiService>().GetDpi();
            
            if (dpi <= 401)
            {
                if (dpi < 350)
                {
                    AppLogoImage.HeightRequest = 105;
                    AppLogoImage.WidthRequest = 105;
                    LblHeader1.FontSize = 16;
                    LblHeader2.FontSize = 12;
                    LblHeader3.FontSize = 12;
                    EmailFrame.Padding = (Device.RuntimePlatform == Device.Android) ? 4 : 10;
                    PasswordFrame.Padding = (Device.RuntimePlatform == Device.Android) ? 4 : 10;
                    EmailBtnFrame.Padding = 11;
                    GoogleBtnFrame.Padding = 11;
                    FacebookBtnFrame.Padding = 11;
                    AppleBtnFrame.Padding = 11;
                    EmailValidator.Margin = (Device.RuntimePlatform == Device.Android) ? new Thickness(0, -2, 0, 0) : new Thickness(0, -6, 0, 0);
                    PasswordValidator.Margin = (Device.RuntimePlatform == Device.Android) ? new Thickness(0, -2, 0, 0) : new Thickness(0, -6, 0, 0);
                }
                else
                {
                    //AppLogoImage.HeightRequest = 120;
                    //AppLogoImage.WidthRequest = 120;
                    LblHeader1.FontSize = 18;
                    LblHeader2.FontSize = 14;
                    LblHeader3.FontSize = 14;
                    EmailFrame.Padding = (Device.RuntimePlatform == Device.Android) ? 5 : 11;
                    PasswordFrame.Padding = (Device.RuntimePlatform == Device.Android) ? 5 : 11;
                    EmailBtnFrame.Padding = 12;
                    GoogleBtnFrame.Padding = 12;
                    FacebookBtnFrame.Padding = 12;
                    AppleBtnFrame.Padding = 12;
                }

            }
            
        }

        public override void OnBeforeShow()
        {
            base.OnBeforeShow();
        }
        private async void Login_btn_clicked(object sender, EventArgs e)
        {
            ((App)Application.Current).displayCreateNewAccount = true;
            PagesFactory.PushAsync<WelcomePage>();
        }

        private void TermsClicked(object sender, EventArgs e)
        {
            Browser.OpenAsync("http://drmuscleapp.com/news/terms/", BrowserLaunchMode.SystemPreferred);
        }

        private void PrivacyClicked(object sender, EventArgs e)
        {
            Browser.OpenAsync("http://drmuscleapp.com/news/privacy/", BrowserLaunchMode.SystemPreferred);
        }
        protected override bool OnBackButtonPressed()
        {           
            ((App)Application.Current).displayCreateNewAccount = true;
            PagesFactory.PushAsync<WelcomePage>();
            return true;
        }

        private async void CreateAccountByEmail(object sender, EventArgs e)
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
            if (DataValidation())
            {
                bool isEmailExist =await CheckEmailExist(EmailEntry.Text);
                if (!isEmailExist)
                {
                    App.IsNewUser = true;
                    LocalDBManager.Instance.SetDBSetting("email", EmailEntry.Text);
                    CreateAccountBeforeDemoButton_Clicked();
                }
            }
        }

        private bool DataValidation()
        {
            try
            {
                EmailValidator.IsVisible = false;
                PasswordValidator.IsVisible = false;
                if (string.IsNullOrEmpty(EmailEntry.Text) && string.IsNullOrEmpty(PasswordEntry.Text))
                {
                    EmailValidator.IsVisible = true;
                    EmailValidator.Text = AppResources.EnterYourEmail;
                    PasswordValidator.IsVisible = true;
                    PasswordValidator.Text = "Enter your password.";
                    return false;
                }
                else if (string.IsNullOrEmpty(EmailEntry.Text))
                {
                    EmailValidator.IsVisible = true;
                    EmailValidator.Text = AppResources.EnterYourEmail;
                    return false;
                }
                else if (string.IsNullOrEmpty(PasswordEntry.Text))
                {
                    PasswordValidator.IsVisible = true;
                    PasswordValidator.Text = "Enter your password.";
                    return false;
                }
                else
                {
                    bool isEmailValid = CheckEmailValidity(EmailEntry.Text);
                    bool isPasswordValid = CheckPasswordValidity(PasswordEntry.Text);
                    if (isEmailValid && isPasswordValid)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool CheckPasswordValidity(string text)
        {
            try
            {
                if (text.Length < 6)
                {
                    PasswordValidator.IsVisible = true;
                    PasswordValidator.Text = "At least 6 characters";
                    return false;
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("password", text);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool CheckEmailValidity(string email)
        {
            var text = email;
            if (!string.IsNullOrEmpty(email) && text.Contains("@"))
            {
                var newEmail = email.Substring(0, email.IndexOf('@'));
                if (newEmail.Length == 1)
                    text = $"a{text}";
            }
            if (!Emails.ValidateEmail(text))
            {
                EmailValidator.IsVisible = true;
                EmailValidator.Text = AppResources.InvalidEmailError;
                return false;
            }
            if (email.Contains("#") || email.Contains("%") || email.Contains("{") || email.Contains("}") || email.Contains("(") || email.Contains("}") || email.Contains("$") || email.Contains("^") || email.Contains("&") || email.Contains("=") || email.Contains("`") || email.Contains("'") || email.Contains("\"") || email.Contains(",") || email.Contains("?") || email.Contains("/") || email.Contains("\\") || email.Contains("<") || email.Contains(">") || email.Contains(":") || email.Contains(";") || email.Contains("|") || email.Contains("[") || email.Contains("]") || email.Contains("*") || email.Contains("*") || email.Contains("!") || email.Contains("~") || email.Count(t => t == '@') > 1)
            {
                EmailValidator.IsVisible = true;
                EmailValidator.Text = AppResources.InvalidEmailError;
                return false;
            }
            try
            {
                var domain = email.Substring(email.IndexOf('@'));
                var extension = email.Substring(email.IndexOf('.') + 1).ToUpper();
                if (domain.Contains("gnail") || domain.Contains("gmaill") || domain.Contains(".cam"))
                {
                    EmailValidator.IsVisible = true;
                    EmailValidator.Text = AppResources.InvalidEmailError;
                    return false;
                }
                else
                {
                    EmailValidator.IsVisible = false;
                    EmailValidator.Text = "";
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }

        private async Task<bool> CheckEmailExist(string email)
        {
            BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExistWithoutLoader(new IsEmailAlreadyExistModel() { email = email });
            if (existingUser != null)
            {
                if (existingUser.Result)
                {
                    
                    //try
                    //{
                    //    if (firstnameDisposible != null)
                    //        firstnameDisposible.Dispose();
                    //    if (passwordDisposible != null)
                    //        passwordDisposible.Dispose();

                    //}
                    //catch (Exception ex)
                    //{

                    //}
                    //finally
                    //{
                    //    await Task.Delay(500);
                    //}

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
                        //GetEmail();
                    }
                    else
                    {
                        ((App)Xamarin.Forms.Application.Current).displayCreateNewAccount = true;
                        await PagesFactory.PushAsync<WelcomePage>();
                    }

                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
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

            //registerModel.Firstname = LocalDBManager.Instance.GetDBSetting("firstname").Value;

            //Revert it
            string FirstName = "";
            //try
            //{
            //    if (EmailEntry.Text.Contains("@"))
            //    {
            //        string[] parts = EmailEntry.Text.Split('@');
            //        FirstName = parts[0];
            //    }
            //}
            //catch (Exception ex)
            //{
            //    FirstName = "";
            //}
            //LocalDBManager.Instance.SetDBSetting("firstname", FirstName);
            ////Revert it

            registerModel.Firstname = FirstName;
            registerModel.EmailAddress = LocalDBManager.Instance.GetDBSetting("email").Value;
            registerModel.MassUnit = "lb";
            registerModel.BodyWeight = new MultiUnityWeight(150, "lb");
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
            LoginSuccessResult lr = await DrMuscleRestClient.Instance.LoginWithoutLoader(new LoginModel()
            {
                //Username = "nazimtest11@gmail.com",
                //Password = "Nazim123"
                ////Revert it
                Username = registerModel.EmailAddress,
                Password = registerModel.Password
            });

            if (lr != null && !string.IsNullOrEmpty(lr.access_token))
            {
                DateTime current = DateTime.Now;

                UserInfosModel uim = await DrMuscleRestClient.Instance.GetUserInfoWithoutLoader();

                LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
                LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
                //LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
                LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
                //LocalDBManager.Instance.SetDBSetting("password", "Nazim123");
                //Revert it
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
                LocalDBManager.Instance.SetDBSetting("timer_123_sound", uim.IsTimer321 ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_reps_sound", uim.IsRepsSound ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
                LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false");                //if (uim.ReminderTime != null)

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
                if (uim.WeightGoal != null)
                {
                    LocalDBManager.Instance.SetDBSetting("WeightGoal", uim.WeightGoal.Kg.ToString().ReplaceWithDot());
                }
                LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");
                //await AccountCreatedPopup();
                //SetUpRestOnboarding();

                // New code
                App.IsIntroBack = true;
                // Navigation.PopModalAsync(false);
               //await PagesFactory.PopThenPushAsync<RegistrationPage>(true);
                //var page = PagesFactory.GetPage<MainOnboardingPage>();
                //page.OnBeforeShow();

                MainOnboardingPage page = new MainOnboardingPage();
                page.OnBeforeShow();
                Navigation.PushAsync(page);
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
        private async void CreateAccountByGmail(object sender, EventArgs e)
        {

            _googleClientManager = CrossGoogleClient.Current;
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
            catch (GoogleClientSignInNetworkErrorException ex)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = ex.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInCanceledErrorException ex)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = ex.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInInvalidAccountErrorException ex)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = ex.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientSignInInternalErrorException ex)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = ex.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientNotInitializedErrorException ex)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = ex.Message,
                    Title = AppResources.Error,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
            }
            catch (GoogleClientBaseException ex)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = ex.Message,
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
                else
                    body = "60";


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
                        LocalDBManager.Instance.SetDBSetting("email", user.Email);
                        LocalDBManager.Instance.SetDBSetting("firstname", user.Name);
                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";
                        if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
                            registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
                        if (LocalDBManager.Instance.GetDBSetting("WeightGoal") != null)
                            registerModel.WeightGoal = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value, CultureInfo.InvariantCulture), "kg");

                        //await DrMuscleRestClient.Instance.RegisterUserBeforeDemo(registerModel);
                        DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
                        LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
                        await AccountCreatedPopup();
                        //SetUpRestOnboarding();
                        LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");

                        MainOnboardingPage page = new MainOnboardingPage();
                        page.OnBeforeShow();
                        Navigation.PushAsync(page);
                    }
                    try
                    {
                        //LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                        //LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
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
                        if (uim.TargetIntake != null && uim.TargetIntake != 0)
                            LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                        //if (uim.ReminderTime != null)
                        //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                        //if (uim.ReminderDays != null)
                        //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

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

                        LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
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

                        if (IsExistingUser)
                        {
                            App.IsDemoProgress = false;
                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                            await PagesFactory.PopToRootAsync(true);
                            return;
                        }
                        await AccountCreatedPopup();
                        //SetUpRestOnboarding();
                        LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");

                        MainOnboardingPage page = new MainOnboardingPage();
                        page.OnBeforeShow();
                        Navigation.PushAsync(page);
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
        private async Task AccountCreatedPopup()
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var modalPage = new Views.GeneralPopup("TrueState.png", "Success!", "Account created", "Customize program");
            modalPage.Disappearing += (sender2, e2) =>
            {
                waitHandle.Set();
            };
            await PopupNavigation.Instance.PushAsync(modalPage);

            await Task.Run(() => waitHandle.WaitOne());

        }
        private async void CreateAccountByFacebook(object sender, EventArgs e)
        {
            _manager = DependencyService.Get<IFacebookManager>();
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
            LocalDBManager.Instance.SetDBSetting("firstname", firstname);
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
            string mass = "lb";
            string body = null;
            body = new MultiUnityWeight(150, "lb").Kg.ToString();
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
                        RegisterModel registerModel = new RegisterModel();
                        registerModel.Firstname = firstname;
                        registerModel.EmailAddress = FBEmail;

                        registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;

                        if (LocalDBManager.Instance.GetDBSetting("Age") != null)
                            registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);

                        registerModel.BodyWeight = new MultiUnityWeight(150, "lb");
                        registerModel.Password = "";
                        registerModel.ConfirmPassword = "";

                        //await DrMuscleRestClient.Instance.RegisterUserBeforeDemo(registerModel);
                        await AccountCreatedPopup();
                        //SetUpRestOnboarding();
                        LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");
                        DependencyService.Get<IFirebase>().LogEvent("account_created", "");
                        //New Code
                        MainOnboardingPage page1 = new MainOnboardingPage();
                        page1.OnBeforeShow();
                        Navigation.PushAsync(page1);
                        //New Code
                        return;
                    }
                    LocalDBManager.Instance.SetDBSetting("email", uim.Email);
                    if (!string.IsNullOrEmpty(uim.Firstname))
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
                    if (uim.TargetIntake != null && uim.TargetIntake != 0)
                        LocalDBManager.Instance.SetDBSetting("TargetIntake", uim.TargetIntake.ToString());
                    //if (uim.ReminderTime != null)
                    //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
                    //if (uim.ReminderDays != null)
                    //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

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

                    LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("1By1Side", uim.Is1By1Side ? "true" : "false");
                    LocalDBManager.Instance.SetDBSetting("StrengthPhase", uim.IsStrength ? "true" : "false");
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
                    await AccountCreatedPopup();
                    //SetUpRestOnboarding();
                    LocalDBManager.Instance.SetDBSetting("FirstStepCompleted", "true");

                    MainOnboardingPage page = new MainOnboardingPage();
                    page.OnBeforeShow();
                    Navigation.PushAsync(page);
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
            //if (string.IsNullOrEmpty(FBEmail))
            //{
            //    await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //    {
            //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //        Message = "Your Facebook account is not connected with email (or we do not have permission to access it). Please sign up with email.",
            //        Title = AppResources.Error
            //    });

            //    return;
            //}
            //LocalDBManager.Instance.SetDBSetting("LoginType", "Social");
            //LocalDBManager.Instance.SetDBSetting("FBId", FBId);
            //LocalDBManager.Instance.SetDBSetting("FBEmail", FBEmail);
            //LocalDBManager.Instance.SetDBSetting("FBGender", FBGender);
            //LocalDBManager.Instance.SetDBSetting("FBToken", FBToken);
            //var url = $"http://graph.facebook.com/{FBId}/picture?type=square";
            //LocalDBManager.Instance.SetDBSetting("ProfilePic", url);



            //BooleanModel existingUser = await DrMuscleRestClient.Instance.IsEmailAlreadyExist(new IsEmailAlreadyExistModel() { email = FBEmail });
            //bool IsExistingUser = false;
            //if (existingUser != null)
            //{
            //    if (existingUser.Result)
            //    {

            //        ConfirmConfig ShowAlertPopUp = new ConfirmConfig()
            //        {
            //            Title = "You are already registered",
            //            Message = "Use another account or log into your existing account.",
            //            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //            OkText = "Use another account",
            //            CancelText = AppResources.LogIn,

            //        };
            //        var actionOk = await UserDialogs.Instance.ConfirmAsync(ShowAlertPopUp);
            //        if (actionOk)
            //        {
            //            return;
            //        }
            //        else
            //        {
            //            //((App)Application.Current).displayCreateNewAccount = true;
            //            //await PagesFactory.PushAsync<WelcomePage>();
            //            IsExistingUser = true;
            //        }

            //        //return;
            //    }

            //}
            ////Log in d'un compte existant avec Facebook
            //string mass = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            //string body = null;
            //if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            //    body = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg").Kg.ToString();
            //try
            //{


            //    LoginSuccessResult lr = await DrMuscleRestClient.Instance.FacebookLogin(FBToken, body, mass);
            //    if (lr != null)
            //    {
            //        DateTime current = DateTime.Now;
            //        UserInfosModel uim = null;
            //        if (existingUser.Result)
            //        {
            //            uim = await DrMuscleRestClient.Instance.GetUserInfo();
            //        }
            //        else
            //        {
            //            RegisterModel registerModel = new RegisterModel();
            //            registerModel.Firstname = firstname;
            //            registerModel.EmailAddress = FBEmail;
            //            registerModel.SelectedGender = LocalDBManager.Instance.GetDBSetting("gender").Value;
            //            registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;
            //            if (LocalDBManager.Instance.GetDBSetting("QuickMode") == null)
            //                registerModel.IsQuickMode = false;
            //            else
            //            {
            //                if (LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "null")
            //                    registerModel.IsQuickMode = null;
            //                else
            //                    registerModel.IsQuickMode = LocalDBManager.Instance.GetDBSetting("QuickMode").Value == "true" ? true : false;
            //            }
            //            if (LocalDBManager.Instance.GetDBSetting("Age") != null)
            //                registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            //            registerModel.RepsMinimum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
            //            registerModel.RepsMaximum = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            //            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            //                registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            //            registerModel.Password = "";
            //            registerModel.ConfirmPassword = "";
            //            registerModel.LearnMoreDetails = learnMore;
            //            registerModel.IsHumanSupport = IsHumanSupport;
            //            registerModel.IsCardio = IsIncludeCardio;
            //            registerModel.BodyPartPrioriy = bodypartName;
            //            registerModel.SetStyle = SetStyle;
            //            if (IncrementUnit != null)
            //                registerModel.Increments = IncrementUnit.Kg;
            //            registerModel.MainGoal = mainGoal;
            //            if (IsEquipment)
            //            {
            //                var model = new EquipmentModel();

            //                if (LocalDBManager.Instance.GetDBSetting("workout_place")?.Value == "gym")
            //                {
            //                    model.IsEquipmentEnabled = true;
            //                    model.IsDumbbellEnabled = isDumbbells;
            //                    model.IsPlateEnabled = IsPlates;
            //                    model.IsPullyEnabled = IsPully;
            //                    model.IsChinUpBarEnabled = IsChinupBar;
            //                    model.Active = "gym";
            //                }
            //                else
            //                {
            //                    model.IsHomeEquipmentEnabled = true;
            //                    model.IsHomeDumbbell = isDumbbells;
            //                    model.IsHomePlate = IsPlates;
            //                    model.IsHomePully = IsPully;
            //                    model.IsHomeChinupBar = IsChinupBar;
            //                    model.Active = "home";
            //                }
            //                registerModel.EquipmentModel = model;

            //            }
            //            //RegisterModel registerModel = new RegisterModel();
            //            //registerModel.Firstname = firstname;
            //            //registerModel.EmailAddress = FBEmail;

            //            //registerModel.MassUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value;

            //            //if (LocalDBManager.Instance.GetDBSetting("Age") != null)
            //            //    registerModel.Age = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("Age").Value);
            //            //if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            //            //    registerModel.BodyWeight = new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg");
            //            //registerModel.Password = "";
            //            //registerModel.ConfirmPassword = "";

            //            uim = await DrMuscleRestClient.Instance.RegisterWithUser(registerModel);
            //            try
            //            {
            //                DBSetting experienceSetting = LocalDBManager.Instance.GetDBSetting("experience");
            //                DBSetting workoutPlaceSetting = LocalDBManager.Instance.GetDBSetting("workout_place");

            //                var level = 0;
            //                if (LocalDBManager.Instance.GetDBSetting("MainLevel") != null)
            //                    level = int.Parse(LocalDBManager.Instance.GetDBSetting("MainLevel").Value);
            //                bool isSplit = LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split");
            //                bool isGym = workoutPlaceSetting?.Value == "gym";
            //                var mo = AppThemeConstants.GetLevelProgram(level, isGym, !isSplit);
            //                if (workoutPlaceSetting?.Value == "homeBodyweightOnly")
            //                {
            //                    if (LocalDBManager.Instance.GetDBSetting("CustomMainLevel") != null && LocalDBManager.Instance.GetDBSetting("CustomMainLevel")?.Value == "1")
            //                    {
            //                        mo.workoutName = "Bodyweight 1";
            //                        mo.workoutid = 12645;
            //                        mo.programid = 487;
            //                        mo.reqWorkout = 12;
            //                        mo.programName = "Bodyweight Level 1";
            //                    }
            //                    else if (level <= 1)
            //                    {
            //                        mo.workoutName = "Bodyweight 2";
            //                        mo.workoutid = 12646;
            //                        mo.programid = 488;
            //                        mo.reqWorkout = 12;
            //                        mo.programName = "Bodyweight Level 2";
            //                    }
            //                    else if (level == 2)
            //                    {
            //                        mo.workoutName = "Bodyweight 2";
            //                        mo.workoutid = 12646;
            //                        mo.programid = 488;
            //                        mo.reqWorkout = 12;
            //                        mo.programName = "Bodyweight Level 2";
            //                    }
            //                    else if (level == 3)
            //                    {
            //                        mo.workoutName = "Bodyweight 3";
            //                        mo.workoutid = 14017;
            //                        mo.programid = 923;
            //                        mo.reqWorkout = 15;
            //                        mo.programName = "Bodyweight Level 3";
            //                    }
            //                    else if (level >= 4)
            //                    {
            //                        mo.workoutName = "Bodyweight 4";
            //                        mo.workoutid = 14019;
            //                        mo.programid = 924;
            //                        mo.reqWorkout = 15;
            //                        mo.programName = "Bodyweight Level 4";
            //                    }

            //                }
            //                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", mo.workoutid.ToString());
            //                LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", mo.workoutName);
            //                LocalDBManager.Instance.SetDBSetting("recommendedProgramId", mo.programid.ToString());
            //                LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", mo.reqWorkout.ToString());

            //                LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", mo.programName);


            //            }
            //            catch (Exception ex)
            //            {

            //            }
            //        }
            //        LocalDBManager.Instance.SetDBSetting("email", uim.Email);
            //        LocalDBManager.Instance.SetDBSetting("firstname", uim.Firstname);
            //        LocalDBManager.Instance.SetDBSetting("lastname", uim.Lastname);
            //        LocalDBManager.Instance.SetDBSetting("gender", uim.Gender);
            //        LocalDBManager.Instance.SetDBSetting("massunit", uim.MassUnit);
            //        LocalDBManager.Instance.SetDBSetting("token", lr.access_token);
            //        LocalDBManager.Instance.SetDBSetting("token_expires_date", DateTime.Now.Add(TimeSpan.FromSeconds((double)lr.expires_in + 1)).Ticks.ToString());
            //        LocalDBManager.Instance.SetDBSetting("creation_date", uim.CreationDate.Ticks.ToString());
            //        LocalDBManager.Instance.SetDBSetting("reprange", "Custom");
            //        LocalDBManager.Instance.SetDBSetting("repsminimum", Convert.ToString(uim.RepsMinimum));
            //        LocalDBManager.Instance.SetDBSetting("repsmaximum", Convert.ToString(uim.RepsMaximum));
            //        LocalDBManager.Instance.SetDBSetting("QuickMode", uim.IsQuickMode == true ? "true" : uim.IsQuickMode == null ? "null" : "false"); LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");
            //        LocalDBManager.Instance.SetDBSetting("ExerciseTypeList", "0");
            //        LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
            //        if (uim.Age != null)
            //            LocalDBManager.Instance.SetDBSetting("Age", Convert.ToString(uim.Age));
            //        //if (uim.ReminderTime != null)
            //        //    LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
            //        //if (uim.ReminderDays != null)
            //        //    LocalDBManager.Instance.SetDBSetting("ReminderDays", uim.ReminderDays);

            //        LocalDBManager.Instance.SetDBSetting("timer_vibrate", uim.IsVibrate ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_sound", uim.IsSound ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_autostart", uim.IsAutoStart ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_autoset", uim.IsAutomatchReps ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_fullscreen", uim.IsFullscreen ? "true" : "false");
            //        LocalDBManager.Instance.SetDBSetting("timer_count", uim.TimeCount.ToString());
            //        LocalDBManager.Instance.SetDBSetting("timer_remaining", uim.TimeCount.ToString());
            //        LocalDBManager.Instance.SetDBSetting("Cardio", uim.IsCardio ? "true" : "false");

            //        LocalDBManager.Instance.SetDBSetting("BackOffSet", uim.IsBackOffSet ? "true" : "false");
            //        if (uim.IsNormalSet == null || uim.IsNormalSet == true)
            //        {
            //            LocalDBManager.Instance.SetDBSetting("SetStyle", "Normal");
            //            LocalDBManager.Instance.SetDBSetting("IsPyramid", uim.IsNormalSet == null ? "true" : "false");
            //        }
            //        else
            //        {
            //            LocalDBManager.Instance.SetDBSetting("SetStyle", "RestPause");
            //            LocalDBManager.Instance.SetDBSetting("IsPyramid", "false");
            //        }
            //        if (uim.Increments != null)
            //            LocalDBManager.Instance.SetDBSetting("workout_increments", uim.Increments.Kg.ToString().ReplaceWithDot());
            //        if (uim.Max != null)
            //            LocalDBManager.Instance.SetDBSetting("workout_max", uim.Max.Kg.ToString().ReplaceWithDot());
            //        if (uim.Min != null)
            //            LocalDBManager.Instance.SetDBSetting("workout_min", uim.Min.Kg.ToString().ReplaceWithDot());
            //        if (uim.BodyWeight != null)
            //        {
            //            LocalDBManager.Instance.SetDBSetting("BodyWeight", uim.BodyWeight.Kg.ToString().ReplaceWithDot());
            //        }
            //        if (uim.WarmupsValue != null)
            //        {
            //            LocalDBManager.Instance.SetDBSetting("warmups", Convert.ToString(uim.WarmupsValue));
            //        }
            //        SetupEquipment(uim);
            //        ((App)Application.Current).displayCreateNewAccount = true;

            //        if (string.IsNullOrEmpty(uim.BodyPartPrioriy))
            //            LocalDBManager.Instance.SetDBSetting("BodypartPriority", "");
            //        else
            //            LocalDBManager.Instance.SetDBSetting("BodypartPriority", uim.BodyPartPrioriy.Trim());

            //        //await PagesFactory.PopToRootAsync(true);
            //        //await PagesFactory.PushAsync<MainAIPage>();
            //        //    App.IsWelcomeBack = true;
            //        //    App.IsDemoProgress = false;
            //        //LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //        //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            //        if (IsExistingUser)
            //        {
            //            App.IsNUX = false;
            //            App.IsDemoProgress = false;
            //            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //            await PagesFactory.PopToRootAsync(true);
            //            return;
            //        }
            //        if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
            //            LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
            //        {
            //            try
            //            {
            //                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
            //                long pId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);
            //                var upi = new GetUserProgramInfoResponseModel()
            //                {
            //                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
            //                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = pId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
            //                };
            //                if (upi != null)
            //                {
            //                    WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
            //                    if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
            //                    {
            //                        try
            //                        {
            //                            nextWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkout(workoutTemplateId);
            //                            //nextWorkout = w.Workouts.First(ww => ww.Id == upi.NextWorkoutTemplate.Id);
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //                            {
            //                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //                                Message = AppResources.PleaseCheckInternetConnection,
            //                                Title = AppResources.ConnectionError
            //                            });
            //                            // await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //                            //{
            //                            //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //                            //    Message = AppResources.PleaseCheckInternetConnection,
            //                            //    Title = AppResources.ConnectionError
            //                            //});
            //                            return;
            //                        }

            //                    }
            //                    App.IsNUX = false;
            //                    if (nextWorkout != null)
            //                    {
            //                        CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
            //                        CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
            //                        CurrentLog.Instance.WorkoutStarted = true;
            //                        if (Device.RuntimePlatform.Equals(Device.Android))
            //                        {
            //                            await PagesFactory.PopToRootThenPushAsync<KenkoDemoWorkoutExercisePage>(true);
            //                            App.IsDemoProgress = false;
            //                            App.IsWelcomeBack = true;
            //                            App.IsNewUser = true;
            //                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //                            CurrentLog.Instance.Exercise1RM.Clear();
            //                            //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            //                            Device.BeginInvokeOnMainThread(async () =>
            //                            {
            //                                await PagesFactory.PopToRootAsync(true);
            //                            });
            //                            MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            //                        }
            //                        else
            //                        {

            //                            App.IsDemoProgress = false;
            //                            App.IsWelcomeBack = true;
            //                            App.IsNewUser = true;
            //                            LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
            //                            CurrentLog.Instance.Exercise1RM.Clear();
            //                            //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
            //                            await PagesFactory.PopToRootMoveAsync(true);
            //                            await PagesFactory.PushMoveAsync<KenkoDemoWorkoutExercisePage>();
            //                            MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            //                        }

            //                    }
            //                    else
            //                    {
            //                        await PagesFactory.PopToRootAsync(true);
            //                        App.IsDemoProgress = false;
            //                        App.IsWelcomeBack = true;
            //                        App.IsNewUser = true;
            //                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");

            //                    }

            //                }
            //            }
            //            catch (Exception ex)
            //            {

            //            }

            //        }
            //    }
            //    else
            //    {
            //        UserDialogs.Instance.Alert(new AlertConfig()
            //        {
            //            Message = AppResources.EmailAndPasswordDoNotMatch,
            //            Title = AppResources.UnableToLogIn,
            //            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            //        });
            //    }
            //}
            //catch (Exception ex)
            //{

            //    await UserDialogs.Instance.AlertAsync(new AlertConfig()
            //    {
            //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //        Message = "We are facing problem to signup with your facebook account. Please sign up with email.",
            //        Title = AppResources.Error
            //    });

            //}
        }

        private async void CreateAccountByApple(object sender, EventArgs e)
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

        private void EmailTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue as string;
            if (string.IsNullOrEmpty(text))
            {
                EmailValidator.IsVisible = true;
                EmailValidator.Text = AppResources.EnterYourEmail;
            }
            else
            {
                EmailValidator.IsVisible = false;
            }
        }

        private void PasswordTextChanged(object sender, TextChangedEventArgs e)
        {
            var text = e.NewTextValue as string;
            if (string.IsNullOrEmpty(text))
            {
                PasswordValidator.IsVisible = true;
                PasswordValidator.Text = "Enter your password.";
            }
            else if(text.Length < 6)
            {
                PasswordValidator.IsVisible = true;
                PasswordValidator.Text = "At least 6 characters";
            }
            else
            {
                PasswordValidator.IsVisible = false;
            }
        }
    }
}