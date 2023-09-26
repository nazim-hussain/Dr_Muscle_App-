using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.User;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class WorkoutPreviewOverlay : PopupPage
    {
        #region Local Variables
        
        public static List<string> _ItemList;
        #endregion
        public WorkoutPreviewOverlay()
        {
            InitializeComponent();
           

           
            
            Setup();
            if (Device.RuntimePlatform.Equals(Device.iOS))
                SupportEmail.IsVisible = false;
            EmailSupportButton.Clicked += async (object sender, EventArgs e) =>
            {

                Device.OpenUri(new Uri("mailto:support@drmuscleapp.com?subject=Subscription%20question"));
            };

            var tapLinkTermsOfUseGestureRecognizer = new TapGestureRecognizer();
            tapLinkTermsOfUseGestureRecognizer.NumberOfTapsRequired = 1;
            tapLinkTermsOfUseGestureRecognizer.Tapped += (s, e) =>
            {
                Device.OpenUri(new Uri("http://drmuscleapp.com/news/terms/"));
            };

            TermsOfUse.GestureRecognizers.Add(tapLinkTermsOfUseGestureRecognizer);

            var tapLinkPrivacyPolicyGestureRecognizer = new TapGestureRecognizer();
            tapLinkPrivacyPolicyGestureRecognizer.NumberOfTapsRequired = 1;
            tapLinkPrivacyPolicyGestureRecognizer.Tapped += (s, e) =>
            {
                Device.OpenUri(new Uri("http://drmuscleapp.com/news/privacy/"));
            };

            PrivacyPolicy.GestureRecognizers.Add(tapLinkPrivacyPolicyGestureRecognizer);

            BuyMonthlyAccessButton.Clicked += BuyMonthlyAccessButton_Clicked;
            BuyYearlyAccessButton.Clicked += BuyYearlyAccessButton_Clicked;
            DependencyService.Get<IDrMuscleSubcription>().OnMonthlyAccessPurchased += async delegate {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
                else
                    await PopupNavigation.Instance.PopAllAsync(); MessagingCenter.Send<SubscriptionSuccessfulMessage>(new SubscriptionSuccessfulMessage(), "SubscriptionSuccessfulMessage");
            };
            DependencyService.Get<IDrMuscleSubcription>().OnYearlyAccessPurchased += async delegate {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
                else
                    await PopupNavigation.Instance.PopAllAsync(); MessagingCenter.Send<SubscriptionSuccessfulMessage>(new SubscriptionSuccessfulMessage(), "SubscriptionSuccessfulMessage");
            };

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    RestorePurchaseButton.IsVisible = true;
                    RestorePurchaseButton.Clicked += async (sender, e) => {
                        if (!CrossConnectivity.Current.IsConnected)
                        {
                            await UserDialogs.Instance.AlertAsync(new AlertConfig()
                            {
                                Message = AppResources.PleaseCheckInternetConnection,
                                Title = AppResources.ConnectionError,
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                                OkText = "Try again"
                            });
                        }
                        DependencyService.Get<IDrMuscleSubcription>().RestorePurchases();
                    };
                    break;

            }

            

          
        }


        async void BuyMonthlyAccessButton_Clicked(object sender, EventArgs e)
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
            }
            if (BuyMonthlyAccessButton.Text.Equals(AppResources.YouAlreadyHaveAccess) || BuyMonthlyAccessButton.Text.Equals("You are on the monthly plan"))
            {
                return;
            }
            await DependencyService.Get<IDrMuscleSubcription>().BuyMonthlyAccess();
        }
        async void BuyYearlyAccessButton_Clicked(object sender, EventArgs e)
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
            }
            await DependencyService.Get<IDrMuscleSubcription>().BuyYearlyAccess();
        }

        private async void Setup()
        {

            DependencyService.Get<IDrMuscleSubcription>().OnMonthlyAccessPurchased += async delegate {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
                else
                    await PopupNavigation.Instance.PopAllAsync(); MessagingCenter.Send<SubscriptionSuccessfulMessage>(new SubscriptionSuccessfulMessage(), "SubscriptionSuccessfulMessage");
            };
            DependencyService.Get<IDrMuscleSubcription>().OnYearlyAccessPurchased += async delegate {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
                else
                    await PopupNavigation.Instance.PopAllAsync(); MessagingCenter.Send<SubscriptionSuccessfulMessage>(new SubscriptionSuccessfulMessage(), "SubscriptionSuccessfulMessage");
            };

            workoutNameLabel.Text = "Our AI suggest " +  LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel")?.Value;
            int age = -1, xDays = 3;
            var reqWorkout = 18;
            try
            {
                var workoutname = "";
                var count = 3;
                
                if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                {
                    reqWorkout =  int.Parse(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout")?.Value);
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
            }
            catch (Exception ex)
            {

            }
            workoutCountLabel.Text = $"{reqWorkout} workouts • {Math.Ceiling ((decimal)reqWorkout /xDays)} weeks approx";


            //SignUpMonthly.IsVisible = true;
            //SignUpYearly.IsVisible = true;
            BuyMonthlyAccessButton.IsVisible = true;
            BuyYearlyAccessButton.IsVisible = true;
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //    LblTooExpensive.IsVisible = true;
            var monthlyText = await DependencyService.Get<IDrMuscleSubcription>().GetMonthlyButtonLabel();
            monthlyText = monthlyText.Replace("Sign up monthly", AppResources.SignUpMonthly);
            monthlyText = monthlyText.Replace("month", AppResources.Month);
            BuyMonthlyAccessButton.Text = monthlyText;

            //var monthPrice = await DependencyService.Get<IDrMuscleSubcription>().GetMonthlyPrice();
            //SignUpMonthly.Text = $"Try it for {monthPrice} for 1 month, then pay monthly:";
            //   BuyMonthlyAccessButton.Clicked += BuyMonthlyAccessButton_Clicked;

            var yearlyText = await DependencyService.Get<IDrMuscleSubcription>().GetYearlyButtonLabel();
            yearlyText = yearlyText.Replace("year", AppResources.Year);
            yearlyText = yearlyText.Replace("Sign up annual", AppResources.SignUpAnnual);
            string goal = "Start Lean Muscle";
            var result = "";
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("Demoreprange")?.Value == "BuildMuscle")
                {
                    goal = "Start Build Muscle";
                    result = "This helps you build muscle.";
                    //LblGoal.Text = "Gain muscle faster";
                }
                else if (LocalDBManager.Instance.GetDBSetting("Demoreprange")?.Value == "BuildMuscleBurnFat")
                {
                    goal = "Start Lean Muscle";//"Build muscle and burn fat";
                    result = "This helps you build muscle and burn fat.";
                    // LblGoal.Text = "Build muscle & burn fat faster";
                }
                else //if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                {
                    goal = "Start Burn Fat";
                    result = "This helps you burn fat.";
                    //LblGoal.Text = "Burn fat faster";
                }
            }
            catch (Exception ex)
            {

            }
            //try
            //{
            //    flChipView.Children.Add(CreateRandomBoxview("Bodyweight training"));
            //    flChipView.Children.Add(CreateRandomBoxview("Bands training"));
            //    flChipView.Children.Add(CreateRandomBoxview("Mobility training"));
            //    flChipView.Children.Add(CreateRandomBoxview("Cardio training"));
            //    flChipView.Children.Add(CreateRandomBoxview("Strength training"));
            //    flChipView.Children.Add(CreateRandomBoxview("Barbell training "));
            //    flChipView.Children.Add(CreateRandomBoxview("Dumbbell training "));
            //}
            //catch (Exception ex)
            //{

            //}

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

            string fname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;

            //  LblGoalDesc.Text = $"{fname}, you're doing {lowReps}-{highreps} reps. {result}";


           

            var yearPrice = await DependencyService.Get<IDrMuscleSubcription>().GetYearlyPrice();
            //SignUpYearly.Text = $"Or save on the annual plan";

            BuyYearlyAccessButton.Text = yearlyText;

           
            if (Device.RuntimePlatform.Equals(Device.Android))
                LblTooExpensive2.IsVisible = true;
            
            SignUpLabelLine3.IsVisible = true;
            SignUpLabelLine4.IsVisible = true;
            //LblExpertReview.IsVisible = true;
            //SignUpLabelHeading.IsVisible = true;

            //var review = userReviewList.ElementAt(rndm.Next(0, 9));
            //var expertReview = expertReviewList.ElementAt(rndm.Next(0, 9));
            //LblReview.Text = review.Review;
            //LblReviewerName.Text = review.ReviewerName;

            
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    SignUpLabelLine3.Text = AppResources.OnceYouConfirmYourSubscriptionPurchase;
                    SignUpLabelLine4.Text = AppResources.OnceYourSubscriptionIsActiveYourITunesAccountWill;
                    break;
                case Device.Android:
                    SignUpLabelLine3.Text = AppResources.ByTappingContinueYourPaymentWillBeChargedToYourGooglePlayAccount;
                    SignUpLabelLine4.Text = AppResources.YourSubscriptionWillRenewAutomatically;
                    break;
            }

            //GridTips1.IsVisible = true;
            //GridTips2.IsVisible = true;

            ImgJonus.IsVisible = true;
            FrmUserReview.IsVisible = true;
            FrmMKJUserReview.IsVisible = true;
            FrmPoteroUserReview.IsVisible = true;
            LblMoreUserReview.IsVisible = true;
            LblTheTestla.IsVisible = true;
            ImgBrandLogo.IsVisible = true;
            FrmExepertReview.IsVisible = true;
            FrmExepertReviewJonny.IsVisible = true;
            ImgArtin.IsVisible = true;



            LblMKJReview.Text = "\"AI is great and makes it very easy\"";
            LblMKJsubHeadingReviewer.Text = "\"Easy for me to know how many reps to do and how much weight to lift. No more guessing. This really is something different.\"";
            LblMKJReviewerName.Text = "MKJ&MKJ";

            LblPoteroReview.Text = "\"Gained 10 lbs\"";
            LblPoterosubHeadingReviewer.Text = "\"Have been in and out of the gym for a few years with modest gains, however, this app helped me gain 10 lbs and become significantly more defined. Very easy to use.\"";
            LblPoteroReviewerName.Text = "Potero2122";

            LblReview.Text = "\"Takes the brain work out of weights, reps, and sets\"";
            LblsubHeadingReviewer.Text = "\"For basic strength training this app out performs the many methods/app I have tried in my 30+ years of body/strength training.\"";
            LblReviewerName.Text = "TijFamily916";


            LblReview1.Text = "\"I haven’t skipped a workout since I started using it\"";
            LblsubHeadingReviewer1.Text = "\"This app creates new workouts automatically. They're challenging enough for me to keep building muscle without overtraining. It also keeps me motivated by showing me my progress.\"";
            LblReviewerName1.Text = "Artin Entezarjou, MD, PhD(c)";

            string cweight = "", todayweight = "", liftedweight = "";

            if (LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg")
            {
                cweight = "95 kg";
                todayweight = "120 kg";
                liftedweight = "242 kg";
            }
            else
            {
                cweight = "210 lbs";
                todayweight = "265 lbs";
                liftedweight = "535 lbs";
            }

            LblReview2.Text = $"\"I was never that heavy ({cweight})\"";
            LblsubHeadingReviewer2.Text = $"\"My strength on hip trusts exploded from something like {todayweight} to {liftedweight}. The app has scientific algorithms... simple stupid and effective wether its raining or sunshine, just follow the app and don't overthink to much.\"";
            LblReviewerName2.Text = "Jonas Notter, World Natural Bodybuilding Champion";
        }

        void WorkoutPageRecognizer_Tapped(object sender, EventArgs e)
        {

        }

        void Close_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
        }

        void HelpGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            PagesFactory.PushAsync<FAQPage>();
        }

        void MoreUserReviewGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            Device.OpenUri(new Uri("https://dr-muscle.com/reviews/"));
        }

        void NewUpdatesGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            //
            Device.OpenUri(new Uri("https://dr-muscle.com/timeline/"));
        }

        void TapMoreExperReviews_Tapped(System.Object sender, System.EventArgs e)
        {
            Device.OpenUri(new Uri("https://dr-muscle.com/reviews/"));
        }

        //private Frame CreateRandomBoxview(string items)
        //{
        //    var view = new Frame();    // Creating New View for design as chip
        //    view.BackgroundColor = Color.FromHex("#AAE1E1E1");
        //    view.BorderColor = Color.Transparent;
        //    view.Padding = new Thickness(10, 10);
        //    view.CornerRadius = 4;
        //    view.HasShadow = false;

        //    //Chip click event
        //    //var tapGestureRecognizer = new TapGestureRecognizer();
        //    //tapGestureRecognizer.Tapped += (s, e) =>
        //    //{
        //    //    var frameSender = (Frame)s;
        //    //    var labelDemo = (Label)frameSender.Content;
        //    //    if (!items.IsClicked)
        //    //    {
        //    //        view.BackgroundColor = (Color)color["White"];
        //    //        labelDemo.TextColor = (Color)color["Purple"];
        //    //        view.BorderColor = (Color)color["Purple"];
        //    //        items.IsClicked = true;
        //    //    }
        //    //    else if (items.IsClicked)
        //    //    {
        //    //        view.BackgroundColor = (Color)color["Purple"];
        //    //        labelDemo.TextColor = (Color)color["White"];
        //    //        view.BorderColor = (Color)color["White"];
        //    //        items.IsClicked = false;
        //    //    }
        //    //};
        //   // view.GestureRecognizers.Add(tapGestureRecognizer);

        //    // creating new child that holds the value of item list and add in View
        //    var label = new Label();
        //    label.Text = items;
        //    label.TextColor = Color.Black;
        //    label.HorizontalOptions = LayoutOptions.Center;
        //    label.VerticalOptions = LayoutOptions.Center;
        //    label.FontSize = 20;
        //    view.Content = label;
        //    return view;
        //}
    }
}
