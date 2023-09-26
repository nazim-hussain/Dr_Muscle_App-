using System;
using System.Collections.Generic;
using DrMuscle.Layout;
using DrMuscle.Screens.User;
using Xamarin.Forms;
using DrMuscle.Resx;
using DrMuscle.Dependencies;
using DrMuscle.Message;
using System.Collections.Specialized;
using DrMuscle.Screens.Me;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Screens.Exercises;
using Acr.UserDialogs;
using DrMuscle.Screens.Workouts;
using DrMuscleWebApiSharedModel;
using System.Linq;
using DrMuscle.Screens.Subscription;

namespace DrMuscle
{
    public partial class MainTabbedPage : TabbedPage
    {
        private Page _lastKnownPage;
        public Dictionary<int, TabData> Tabs = new Dictionary<int, TabData>();
        bool IsChatOpen = false;
        private bool IsSettingsOpen = false;
        public MainTabbedPage()
        {
            InitializeComponent();
            //SetPage();
            NavigationPage.SetHasNavigationBar(this, false);
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });
            Tabs.Add(0, new TabData() { BadgeColor = Color.Red, BadgeCaption = 0 });

            MessagingCenter.Subscribe<NavigationOnNotificationTappedMessage>(this, "NavigationOnNotificationTappedMessage", OnNotificationTapped);

            
            if (Device.RuntimePlatform.Equals(Device.Android))
                MessagingCenter.Subscribe<MoreTappedMessage>(this, "MoreTappedMessage", OnMoreTappedMessage);
            

        }
        

        private async void TabbedPage_CurrentPageChanged(object sender, System.EventArgs e)
        {
            if (App.IsFreePlan && (CurrentPage.Navigation.NavigationStack[0] is LearnPage))
            {
                ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
                {
                    Message = "Upgrading will unlock custom coaching tips based on your goals and progression.",
                    Title = "You discovered a premium feature!",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText =  "Upgrade",
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
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        //((DrMusclePage)_lastKnownPage.Navigation.NavigationStack[_lastKnownPage.Navigation.NavigationStack.Count - 1]).SlideGeneralAction();

                        CurrentPage = _lastKnownPage;

                    });
                }
                else
                {
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        ChangedTabbed();
                    CurrentPage = _lastKnownPage;

                }
                
                
                UserDialogs.Instance.Confirm(ShowWelcomePopUp2);
                
                return;
            }
            if (App.IsFreePlan && (CurrentPage.Navigation.NavigationStack[0] is ChatPage))
            {
                //ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
                //{
                //    Message = "Get free 1-on-1 support or upgrade to unlock group chat",
                //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //    OkText = "Unlock group chat",
                //    CancelText = "1-on-1 support",
                //    OnAction = async (ok) =>
                //    {
                //        if (ok)
                //            PagesFactory.PushAsync<SubscriptionPage>();
                //    }

                //};
                //UserDialogs.Instance.Confirm(ShowWelcomePopUp2);

            }
            if (this.CurrentPage.GetType() == typeof(MorePage))
            {
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    App.IsSidemenuOpen = true;
                    Device.BeginInvokeOnMainThread(async() =>
                    {
                        //((DrMusclePage)_lastKnownPage.Navigation.NavigationStack[_lastKnownPage.Navigation.NavigationStack.Count - 1]).SlideGeneralAction();
                        
                        CurrentPage = _lastKnownPage;
                        var full = new Views.FullscreenMenu();
                        await Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(full);
                        App.IsSidemenuOpen = false;

                    });
                }
                else
                {
                    ((DrMusclePage)_lastKnownPage.Navigation.NavigationStack[_lastKnownPage.Navigation.NavigationStack.Count - 1]).SlideGeneralAction();
                    if (Device.RuntimePlatform.Equals(Device.Android))
                        ChangedTabbed();
                    CurrentPage = _lastKnownPage;

                }
            }
            else
            {
                _lastKnownPage = CurrentPage;
                if (CurrentPage.Navigation.NavigationStack[0] is LearnPage)
                {
                    
                    NavigationPage.SetHasNavigationBar((LearnPage)CurrentPage.Navigation.NavigationStack[0], true);
                    if (Config.ShowLearnPopup == false)
                    {
                        if (App.IsLearnPopup)
                            return;
                        App.IsLearnPopup = true;
                        ConfirmConfig ShowPopUp = new ConfirmConfig()
                        {
                            Title = "Learn",
                            Message = "Workout and diet tips customized to your goals, fitness level, and ongoing progress.",
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            OkText = AppResources.GotIt,
                            CancelText = AppResources.RemindMe,
                            OnAction = async (bool ok) =>
                            {
                                if (ok)
                                {
                                    Config.ShowLearnPopup = true;
                                }
                                else
                                {
                                    Config.ShowLearnPopup = false;
                                }
                            }
                        };
                        try
                        {
                            DateTime creatednDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                            if ((DateTime.Now.ToUniversalTime() - creatednDate).TotalDays <= 14)
                            {
                                ShowPopUp.Message = "Workout and diet tips customized to your goals, fitness level, and ongoing progress. Users like you improve 34% in 30 days.";
                            }
                        }
                        catch (Exception)
                        {

                        }
                        await Task.Delay(100);
                        UserDialogs.Instance.Confirm(ShowPopUp);

                    }
                }

                if (CurrentPage.Navigation.NavigationStack[0] is ChatPage)
                {
                    if ( IsChatOpen == false)
                    {
                        IsChatOpen = true;
                        var loadChatMessage = new LoadChatMessage();
                        loadChatMessage.IsFromLogin = false;
                        MessagingCenter.Send(loadChatMessage, "LoadChatMessage");
                    }
                    NavigationPage.SetHasNavigationBar((ChatPage)CurrentPage.Navigation.NavigationStack[0], true);
                    //if (Config.ShowChatPopup == false)
                    //{
                    //    if (App.IsChatPopup)
                    //        return;
                    //    App.IsChatPopup = true;
                    //    ConfirmConfig ShowPopUp = new ConfirmConfig()
                    //    {
                    //        Title = "Chat",
                    //        Message = "Get 1:1 support from a human in 1 day or less.",
                    //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    //        OkText = AppResources.GotIt,
                    //        CancelText = AppResources.RemindMe,
                    //        OnAction = async (bool ok) =>
                    //        {
                    //            if (ok)
                    //            {
                    //                Config.ShowChatPopup = true;    
                    //            }
                    //            else
                    //            {
                    //                Config.ShowChatPopup = false;
                    //            }
                    //        }
                    //    };
                    //    await Task.Delay(100);
                    //    UserDialogs.Instance.Confirm(ShowPopUp);

                    //}
                }

                if (CurrentPage.Navigation.NavigationStack[0] is SettingsPage)
                {
                    
                    if (!IsSettingsOpen)
                    {
                        IsSettingsOpen = true;
                        var loadSettingsMessage = new LoadSettingsMessage();
                        loadSettingsMessage.IsFromLogin = false;
                        MessagingCenter.Send(loadSettingsMessage, "LoadSettingsMessage");
                    }
                }
                if (CurrentPage.Navigation.NavigationStack[0] is AllExercisePage)
                {
                    NavigationPage.SetHasNavigationBar((AllExercisePage)CurrentPage.Navigation.NavigationStack[0], true);
                    
                }

            }

            System.Diagnostics.Debug.WriteLine(this.CurrentPage.ToString());
        }

        async Task ChangedTabbed()
        {
            Task.Factory.StartNew(async () =>
            {
                 Device.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1500);
                    SelectedItem = _lastKnownPage;
                    this.OnPropertyChanged();
                });
            });
        }

        private async void OnMoreTappedMessage(MoreTappedMessage obj)
        {
            try
            {
                ((DrMusclePage)CurrentPage.Navigation.NavigationStack[_lastKnownPage.Navigation.NavigationStack.Count - 1]).SlideGeneralMoreAction();
            }
            catch (Exception ex)
            {

            }
        }

        private async void OnNotificationTapped(NavigationOnNotificationTappedMessage obj)
        {
            //if (string.IsNullOrEmpty(obj.PageKey.ToString()))
            //    return;

            //_analyticsService.SendNotificationsViewEvent(obj.PageKey.ToString());
            //AppPages currentPage;
            //Enum.TryParse<AppPages>(_navigationService.CurrentPageKey, out currentPage);

            //if (obj.PageKey == AppPages.MainPage && currentPage != AppPages.MainPage)
            //    _navigationService.NavigateToFirstPage();
            //else if (obj.PageKey != currentPage)
            //    _navigationService.NavigateTo(obj.PageKey);

            //_navigationService.HideSideMenu();

            try
            {
                if (obj.NotificationType == "Local")
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    return;
                }

                if (obj.NotificationType == "Workout")
                {
                    CurrentLog.Instance.IsRecoveredWorkout = true;
                    return;
                }

                if (LocalDBManager.Instance.GetDBSetting("email") == null)
                return;
                App.IsFromNotificaion = true;
                if (Device.RuntimePlatform.Equals(Device.Android))
                    this.SelectedItem = this.Children[2];
                else
                { 
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        this.SelectedItem = this.Children[2];
                    });
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void RefreshLocalized()
        {
            TabHome.Title = AppResources.Home;
            //TabLearn.Title = "Learn";
            TabExercise.Title = "Exercise";
            TabChat.Title = AppResources.ChatBeta;
            TabSettings.Title = AppResources.Settings; //"Me"; //AppResources.More;

            //MePage.Title = LocalDBManager.Instance.GetDBSetting("firstname") == null ? "Me" : string.Format("{0} {1}!", App.IsNewUser ? AppResources.Welcome : AppResources.WelcomeBack, LocalDBManager.Instance.GetDBSetting("firstname")?.Value);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshLocalized();
        }

        public virtual void OnBeforeShow()
        {
            try
            {
                foreach (var item in this.Children)
                {
                    if (item.Navigation.NavigationStack[0] is DrMusclePage)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ((DrMusclePage)item.Navigation.NavigationStack[0]).OnBeforeShow();
                        });
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        public async void PresentModalPage()
        {
            //var full = new Views.FullscreenMenu();
            //Rg.Plugins.Popup.Services.PopupNavigation.Instance.PushAsync(full);
        }

        //protected override bool OnBackButtonPressed()
        //{
        //    Device.BeginInvokeOnMainThread(async () =>
        //    {
        //        var kill = DependencyService.Get<IKillAppService>();
        //        kill.ExitApp();
        //    });
        //    return base.OnBackButtonPressed();
        //}
        public virtual void OnShow()
        {

        }
        
    }
}