using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.History;
using DrMuscle.Screens.Me;
using DrMuscle.Screens.Subscription;
using DrMuscle.Screens.User;
using DrMuscle.Screens.User.OnBoarding;
using DrMuscle.Services;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class FullscreenMenu : PopupPage
    {
        public List<ReviewsModel> reviewList = new List<ReviewsModel>();
        public FullscreenMenu()
        {
            InitializeComponent();
            App.IsSidemenuOpen = true;
            VersionInfoLabel.Text = DependencyService.Get<IDrMuscleSubcription>().GetBuildVersion().Replace("Version", AppResources.Version).Replace("Build", AppResources.Build);
            reviewList = GetReviews();
            var rndm = new Random();
            var review = reviewList.ElementAt(rndm.Next(0, 9));
            LblReview.Text = review.Review;
            LblReviewerName.Text = review.ReviewerName;

            var h = DeviceDisplay.MainDisplayInfo.Density > 1 ? DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density : DeviceDisplay.MainDisplayInfo.Height; ;
            if (h<650)
            {
                LblReview.MaxLines = 3;
            }
            else if (h >= 650 && h <= 700)
            {
                LblReview.MaxLines = 4;
            }
            else if (h > 700 && h <= 730)
            {
                LblReview.MaxLines = 5;
            }
            else if (h > 730 && h <= 760)
            {
                LblReview.MaxLines = 6;
            }
            else if (h > 760 && h <= 790)
            {
                LblReview.MaxLines = 6;
            }
            else if (h > 790)
                LblReview.MaxLines = 7;
            
            MeGesture.Tapped += async (sender, e) =>
            {
                await HideWithoutAnimations();
                await PagesFactory.PushAsync<SettingsPage>();
            };
            //NewNUXGestures.Tapped += async (sender, e) =>
            //{
            //    await HideWithoutAnimations();
            //    await PagesFactory.PushAsync<MainOnboardingPage>(true);
            //};
            //ChartsGesture.Tapped += async (sender, e) =>
            //{
            //    await HideWithoutAnimations();
            //    await PagesFactory.PushAsync<MeCombinePage>();
            //};
            SettingGesture.Tapped += async (object sender, EventArgs e) =>
            {
                await HideWithoutAnimations();
                await PagesFactory.PushAsync<SettingsPage>();
            };

            //EquipmentGesture.Tapped += async (object sender, EventArgs e) =>
            //{
            //    await HideWithoutAnimations();
            //    await PagesFactory.PushAsync<EquipmentSettingsPage>();
            //};

            HistoryGesture.Tapped += async (object sender, EventArgs e) =>
            {
                await HideWithoutAnimations();
                //CurrentLog.Instance.PastWorkoutDate = null;
                await PagesFactory.PushAsync<HistoryPage>();
            };

            SubscriptionGesture.Tapped += async (object sender, EventArgs e) =>
            {
                await HideWithoutAnimations();
                await PagesFactory.PushAsync<SubscriptionPage>();
            };
            FeedbackGesture.Tapped += async (sender, e) =>
            {
                await HideWithoutAnimations();
                var page = new FeedbackView();
                await PopupNavigation.Instance.PushAsync(page);
            };
            FAQGesture.Tapped += async (object sender, EventArgs e) =>
            {
                await HideWithoutAnimations();
                await PagesFactory.PushAsync<FAQPage>();
            };

            LearnGesture.Tapped += async (object sender, EventArgs e) =>
            {
                await HideWithoutAnimations();

                if (CheckTrialUser())
                    return;
                await PagesFactory.PushAsync<LearnPage>();
            };
            //WebGestures.Tapped += (sender, e) =>
            //{
            //    Device.OpenUri(new Uri("https://dr-muscle.com/reviews/"));
            //    //Device.OpenUri(new Uri("https://my.dr-muscle.com"));
            //};
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
                DependencyService.Get<IFirebase>().LogEvent("told_a_friend", "share");
                DependencyService.Get<IFirebase>().LogEvent("menu_share_free_trial", "share");
            };
            //EmailUsButton.Clicked += (object sender, EventArgs e) =>
            //{
            //    HideWithoutAnimations();
            //    Device.OpenUri(new Uri("mailto:support@drmuscleapp.com"));
            //};

            LogoutGesture.Tapped += async (object sender, EventArgs e) =>
            {
                //

                ConfirmConfig supersetConfig = new ConfirmConfig()
                {
                    Title = "Are you sure?",
                    OkText = "Log out",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    CancelText = "Cancel",
                };
                if (LocalDBManager.Instance.GetDBSetting($"AnySets{DateTime.Now.Date}")?.Value
                         == "1")
                    supersetConfig.Message = "Your unsaved sets will be lost.";

                var x = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                if (x)
                {
                    if (Timer.Instance.State == "RUNNING")
                    {
                        await Timer.Instance.StopTimer();
                    }

                    //Config.DownRecordExplainer = "";
                    //Config.DownRecordPercentage = 0;
                    await HideWithoutAnimations();
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
                    ((App)Application.Current).NewRecordModelContext.NewRecordList = new List<NewRecordModel>();
                    ((App)Application.Current).NewRecordModelContext.SaveContexts();
                    ((App)Application.Current).WeightsContextList.Weights = new List<UserWeight>();
                    ((App)Application.Current).WeightsContextList.SaveContexts();

                    try
                    {
                        if (((global::DrMuscle.MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage.Navigation.NavigationStack[0] is LearnPage)
                            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).SelectedItem = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[0];
                    }
                    catch (Exception ex)
                    {

                    }
                    await PagesFactory.PopToRootAsync(true);
                    await PagesFactory.PopToRootAsync();
                    ((App)Application.Current).displayCreateNewAccount = true;
                    await PagesFactory.PushAsync<WelcomePage>(true);
                }
            };

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
                LblEmail.Text = LocalDBManager.Instance.GetDBSetting("email")?.Value;
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                if (workouts != null)
                {
                    if (workouts.Sets != null)
                    {
                         
                            bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                            var exerciseModel = workouts.HistoryExerciseModel;
                            if (exerciseModel != null)
                            {
                                var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();
                                var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                                LblDoneWorkout.Text = exerciseModel.TotalWorkoutCompleted <= 1 ? $"{exerciseModel.TotalWorkoutCompleted} workout" : $"{exerciseModel.TotalWorkoutCompleted} workouts";
                            }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            RefreshLocalized();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
        protected override void OnDisappearing()
        {
            App.IsSidemenuOpen = false;
            base.OnDisappearing();
        }
        private void RefreshLocalized()
        {
            //MeButton.Text = "More";
            //ChartsButton.Text = "Charts";
            //HistoryButton.Text = AppResources.History;
            SubscriptionInfosButton.Text = "Subscription"; //AppResources.SubscriptionInfo;
            LogOutButton.Text = AppResources.LogOut;
            
            
            VersionInfoLabel.Text = DependencyService.Get<IDrMuscleSubcription>().GetBuildVersion().Replace("Version", AppResources.Version).Replace("Build", AppResources.Build);
            
            FAQButton.Text = "Help";
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

        private async void RemoveToken()
        {
            
        }

        private void CancelNotification()
        {
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1251);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1351);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1451);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1551);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1651);
        }

        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height);

        //    if (Device.RuntimePlatform.Equals(Device.iOS))
        //    {
        //        GeneralStack.Margin = new Thickness(0, 0, 0, 0);
                
        //        PancakeContainer.Margin = new Thickness(0);
        //        PancakeContainer.Padding = new Thickness(0, App.StatusBarHeight + 10, 0, 0);
        //    }
        //}
        async void TapMoreReviews_Tapped(System.Object sender, System.EventArgs e)
        {
            await HideWithoutAnimations();
            //Browser.OpenAsync("https://dr-muscle.com/reviews/", BrowserLaunchMode.SystemPreferred);
            Device.OpenUri(new Uri("https://dr-muscle.com/reviews/"));
        }

        private async Task HideWithoutAnimations()
        {
            try
            {
                    if (PopupNavigation.Instance.PopupStack.Count > 0)
                        await PopupNavigation.Instance.PopAsync();
                    
                
            }
            catch (Exception ex)
            {

            }
        }

        void Handle_BuildVersionTapped(object sender, System.EventArgs e)
        {
            ActionSheetConfig config = new ActionSheetConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            };

            bool isProduction = LocalDBManager.Instance.GetDBSetting("Environment") == null || LocalDBManager.Instance.GetDBSetting("Environment").Value == "Production";

            config.Add(isProduction ? $"Production (active)" : $"Production", () =>
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

        private async void LogOut()
        {
            await HideWithoutAnimations();
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
            ((App)Application.Current).NewRecordModelContext.NewRecordList = new List<NewRecordModel>();
            ((App)Application.Current).NewRecordModelContext.SaveContexts();
            ((App)Application.Current).WorkoutHistoryContextList.Histories = new List<HistoryModel>();
            ((App)Application.Current).WorkoutHistoryContextList.SaveContexts();
            ((App)Application.Current).WorkoutLogContext.WorkoutLogSeriesByExerciseRef = new Dictionary<long, ObservableCollection<WorkoutLogSerieModelRef>>();
            ((App)Application.Current).WorkoutLogContext.SaveContexts();
            await PagesFactory.PopToRootAsync(true);
            await PagesFactory.PopToRootAsync();
            ((App)Application.Current).displayCreateNewAccount = true;
            await PagesFactory.PushAsync<WelcomePage>(true);

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

        async void ImageButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread( async () =>
            {
                await PopupNavigation.Instance.PopAsync();   
            });
        }
    }
}
