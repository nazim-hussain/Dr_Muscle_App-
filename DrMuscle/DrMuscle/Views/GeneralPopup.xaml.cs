using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Screens.Subscription;
using DrMuscle.Screens.User;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class GeneralPopup : PopupPage
    {
        TapGestureRecognizer okGuesture;
        string buttonText = "";
        public event EventHandler OkButtonPress;
        public bool _isHide { get; set; }
        public GeneralPopup(string image, string title, string subtitle, string buttonText, Thickness? thickness = null, bool isTips = false, bool isSummary = false, string isShowLearnMore = "false", string isShowSettings="false", string ismealPlan = "false", string isNotNow = "false", string isAutoHide = "false", string isNewFeature = "false", string isNotYet = "false", bool isChatLoading = false)
        {
            InitializeComponent();
            okGuesture = new TapGestureRecognizer();
            okGuesture.Tapped += OkButton_Clicked;
            OkAction.GestureRecognizers.Add(okGuesture);
            ImgName.Source = image;
            if (thickness != null)
                ImgName.Margin = (Thickness)thickness;
            LblHeading.Text = title;
            LblSubHead.Text = subtitle;
            OkButton.Text = buttonText;
            this.buttonText = buttonText;
            LblTipText.IsVisible = false;
            if (isShowLearnMore == "true")
            {
                BtnLearnMore.IsVisible =  false;
                LblSubHead.Margin = new Thickness(15, 0, 15, 5);
               

            }
            else if (isShowSettings == "true")
            {
                BtnLearnMore.IsVisible = true;
                LblSubHead.Margin = new Thickness(15, 0, 15, 5);
                BtnLearnMore.Text = "Open Settings";

            }
            else if (ismealPlan == "true")
            {
                BtnCancel.IsVisible = true;
                //LblSubHead.Margin = new Thickness(15, 0, 15, 5);
                BtnLearnMore.IsVisible = false;
            }
            else if (isNotNow == "true")
            {
                BtnCancel.IsVisible = true;
                BtnLearnMore.IsVisible = false;
                BtnCancel.Text = "Not now";
            }
            else if (isNotYet == "true")
            {
                BtnCancel.IsVisible = true;
                BtnLearnMore.IsVisible = false;
                BtnCancel.Text = "Not yet";
            }
            else if (isNewFeature == "true")
            {
                BtnCancel.IsVisible = false;
                BtnLearnMore.IsVisible = false;
                BtnCancel.Text = "Learn more";
                LblSubHead.HorizontalTextAlignment = TextAlignment.Start;
            }
            else
            {
                BtnLearnMore.IsVisible = false;
            }

            if (isTips)
            {
                LblCountDown.IsVisible = true;
                if (buttonText == "Continue")
                {
                    SetLoadingWorkout(title);
                    //OkAction?.GestureRecognizers?.Add(okGuesture);
                } else { 
                SetLoading(title);
                SetTimerHidePopup();
                }
                //OkButton.Clicked -= OkButton_Clicked;
                //OkAction.GestureRecognizers.Remove(okGuesture);
                //OkButton.Text = "Customizing workout...";
                OkAction.IsVisible = false;
                LblCountDown.IsVisible = true;
                MessagingCenter.Subscribe<Message.WorkoutLoadedMessage>(this, "WorkoutLoadedMessage", (obj) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        LblTipText.IsVisible = false;
                        OkAction.IsVisible = true;
                        LblCountDown.IsVisible = true;
                        //OkButton.Text = "Start workout";
                        //OkButton.Clicked += OkButton_Clicked;
                        //OkAction?.GestureRecognizers?.Add(okGuesture);
                    });
                });
            }
            if (isChatLoading)
            {
                SetLoadingChat(title);
                SetTimerHidePopup();
                //OkButton.Clicked -= OkButton_Clicked;
                OkAction.GestureRecognizers.Remove(okGuesture);
                //OkButton.Text = "Customizing workout...";
                OkAction.IsVisible = false;
                //MessagingCenter.Subscribe<Message.WorkoutLoadedMessage>(this, "WorkoutLoadedMessage", (obj) =>
                //{
                //    Device.BeginInvokeOnMainThread(() =>
                //    {
                //        LblTipText.IsVisible = false;
                //        OkAction.IsVisible = true;

                //        //OkButton.Text = "Start workout";
                //        //OkButton.Clicked += OkButton_Clicked;
                //        OkAction.GestureRecognizers.Add(okGuesture);
                //    });
                //});
            }
            if (isSummary)
            {

                //OkButton.Clicked -= OkButton_Clicked;
                OkAction?.GestureRecognizers?.Remove(okGuesture);
                SetLoadingSummary(okGuesture);
                //MessagingCenter.Subscribe<Message.WorkoutLoadedMessage>(this, "WorkoutLoadedMessage", (obj) =>
                //{
                    //Device.BeginInvokeOnMainThread(() =>
                    //{
                        
                        //OkButton.Clicked += OkButton_Clicked;
                        
                    //});
                //});
            }
            if (isAutoHide == "true")
            {
                
                
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isHide = true;
            MessagingCenter.Unsubscribe<Message.WorkoutLoadedMessage>(this, "WorkoutLoadedMessage");
        }
        private async Task SetLoadingSummary(TapGestureRecognizer okGuesture)
        {
            await Task.Delay(250);

            OkButton.Text = "Loading.";

            await Task.Delay(700);
            OkButton.Text = "Loading..";

            await Task.Delay(700);
            
            OkButton.Text = "Loading...";
                await Task.Delay(700);
            OkButton.Text = this.buttonText;
            OkAction.GestureRecognizers.Add(okGuesture);

        }

        private async void SetLoadingChat(string title)
        {
            //LblHeading.FontAttributes = LblSubHead.FontAttributes;
            //LblHeading.FontSize = LblSubHead.FontSize;
            //LblHeading.TextColor = LblSubHead.TextColor;

            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                LblTipText.Text = "";
                LblTipText.IsVisible = true;
                OkAction.IsVisible = false;
                
                await Task.Factory.StartNew(async () =>
                {

                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading chat...";
                    });


                    await Task.Delay(800);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {

                        LblTipText.Text = "Loading messages...";
                    });
                    await Task.Delay(750);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LblTipText.IsVisible = false;
                        OkAction.IsVisible = true;

                        //OkButton.Text = "Start workout";
                        //OkButton.Clicked += OkButton_Clicked;
                        OkAction.GestureRecognizers.Add(okGuesture);
                    });

                });
            }
            else
            {
                LblTipText.IsVisible = true;

                //ImgLoader.IsVisible = true;
                Device.BeginInvokeOnMainThread(async () =>
                {

                    LblTipText.Text = "Loading chat...";
                    await Task.Delay(800);
                    if (LblTipText.Text == " ")
                        return;
                    LblTipText.Text = "Loading messages...";
                    await Task.Delay(750);
                    LblTipText.IsVisible = false;
                    OkAction.IsVisible = true;

                    //OkButton.Text = "Start workout";
                    //OkButton.Clicked += OkButton_Clicked;
                    OkAction.GestureRecognizers.Add(okGuesture);
                });
            }
            
        }

        private async void SetLoadingWorkout(string title)
        {
            //LblHeading.FontAttributes = LblSubHead.FontAttributes;
            //LblHeading.FontSize = LblSubHead.FontSize;
            //LblHeading.TextColor = LblSubHead.TextColor;
            try
            {

            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                LblTipText.Text = "";
                LblTipText.IsVisible = true;
                await Task.Factory.StartNew(async () =>
                {

                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading equipment...";
                    });


                    await Task.Delay(1500);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {

                        LblTipText.Text = "Loading profile...";
                    });
                    await Task.Delay(1400);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {

                        LblTipText.Text = "Loading program...";
                    });
                    await Task.Delay(1300);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading calendar...";
                    });
                    await Task.Delay(1200);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading custom tips";
                    });
                    await Task.Delay(1100);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading progression...";
                    });
                    await Task.Delay(1000);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading progression...";
                    });

                    await Task.Delay(1000);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading new records...";
                    });

                    await Task.Delay(900);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading a big pump...";
                    });
                    await Task.Delay(800);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LblTipText.IsVisible = false;
                        OkAction.IsVisible = true;

                        
                        //OkAction?.GestureRecognizers?.Add(okGuesture);
                        SetTimerForContinue();
                    });
                });
            }
            else
            {
                LblTipText.IsVisible = true;

                //ImgLoader.IsVisible = true;
                Device.BeginInvokeOnMainThread(async () =>
                {

                    LblTipText.Text = "Loading equipment...";


                    await Task.Delay(1500);
                    if (LblTipText.Text == " ")
                        return;

                        LblTipText.Text = "Loading profile...";
                    await Task.Delay(1400);
                    if (LblTipText.Text == " ")
                        return;

                        LblTipText.Text = "Loading program...";
                    await Task.Delay(1300);
                    if (LblTipText.Text == " ")
                        return;
                        LblTipText.Text = "Loading calendar...";
                    await Task.Delay(1200);
                    if (LblTipText.Text == " ")
                        return;
                        LblTipText.Text = "Loading custom tips";
                    await Task.Delay(1100);
                    if (LblTipText.Text == " ")
                        return;
                        LblTipText.Text = "Loading progression...";
                    await Task.Delay(1000);
                    if (LblTipText.Text == " ")
                        return;
                        LblTipText.Text = "Loading progression...";
                    

                    await Task.Delay(1000);
                    if (LblTipText.Text == " ")
                        return;
                    
                        LblTipText.Text = "Loading new records...";
                    

                    await Task.Delay(900);
                    if (LblTipText.Text == " ")
                        return;
                    
                        LblTipText.Text = "Loading a big pump...";
                    
                    await Task.Delay(800);
                    LblTipText.IsVisible = false;
                    OkAction.IsVisible = true;


                    
                    SetTimerForContinue();
                });
            }

            }
            catch (Exception ex)
            {

            }
        }

        private async void SetLoading(string title)
        {
            //LblHeading.FontAttributes = LblSubHead.FontAttributes;
            //LblHeading.FontSize = LblSubHead.FontSize;
            //LblHeading.TextColor = LblSubHead.TextColor;
            
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                LblTipText.Text = "";
                LblTipText.IsVisible = true;
                LblCountDown.IsVisible = true;
                await Task.Factory.StartNew(async () =>
                {

                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading sets...";
                    });

                    
                    await Task.Delay(500);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                    
                        LblTipText.Text = "Loading reps...";
                    });
                    await Task.Delay(750);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                    
                        LblTipText.Text = "Loading weights...";
                    });
                    await Task.Delay(500);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Loading a big pump...";
                    });
                    await Task.Delay(500);
                    if (LblTipText.Text == " ")
                        return;
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblTipText.Text = "Let's go!";
                    });
                });
            }
            else
            {
                 LblTipText.IsVisible = true;
                LblCountDown.IsVisible = true;
                //ImgLoader.IsVisible = true;
                Device.BeginInvokeOnMainThread(async () =>
                {

                    LblTipText.Text = "Loading sets...";
                    await Task.Delay(700);
                    if (LblTipText.Text == " ")
                        return;
                    LblTipText.Text = "Loading reps...";
                    await Task.Delay(750);
                    if (LblTipText.Text == " ")
                        return;
                    LblTipText.Text = "Loading weights...";
                    await Task.Delay(800);
                    if (LblTipText.Text == " ")
                        return;
                    if (LblTipText.Text == " ")
                        return;
                    LblTipText.Text = "Loading a big pump...";
                    await Task.Delay(800);
                    if (LblTipText.Text == " ")
                        return;
                    LblTipText.Text = "Let's go!";

                });
            }
        }

        private async void SetTimerHidePopup()
        {
            //LblHeading.FontAttributes = LblSubHead.FontAttributes;
            //LblHeading.FontSize = LblSubHead.FontSize;
            //LblHeading.TextColor = LblSubHead.TextColor;
            await Task.Delay(1000);
            OkButton.Text = $"{this.buttonText}";
            var closingText = "Closing";
            LblCountDown.Text = $"{closingText} 5";
            LblCountDown.IsVisible = true;
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                LblTipText.Text = "";
                LblTipText.IsVisible = false;
                OkButton.IsVisible = true;
                OkAction.IsVisible = true;
                await Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(1000);
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblCountDown.Text = $"{closingText} 4";
                    });


                    await Task.Delay(1000);
                    MainThread.BeginInvokeOnMainThread(() => {

                        LblCountDown.Text = $"{closingText} 3";
                    });

                    await Task.Delay(1000);
                    MainThread.BeginInvokeOnMainThread(() => {

                        LblCountDown.Text = $"{closingText} 2";
                    });
                    await Task.Delay(1000);
                    MainThread.BeginInvokeOnMainThread(() => {

                        LblCountDown.Text = $"{closingText} 1";
                    });
                    await Task.Delay(1000);
                    MainThread.BeginInvokeOnMainThread(() => {
                        LblCountDown.Text = $" ";
                    });
                    if (PopupNavigation.Instance.PopupStack.Count > 0 && !_isHide)
                            PopupNavigation.Instance.PopAsync();
                });
            }
            else
            {
                LblTipText.IsVisible = true;

                //ImgLoader.IsVisible = true;
                Device.BeginInvokeOnMainThread(async () =>
                {

                    LblCountDown.Text = $"{closingText} 5";
                    await Task.Delay(1000);
                    LblCountDown.Text = $"{closingText} 4";
                    await Task.Delay(1000);
                    LblCountDown.Text = $"{closingText} 3";
                    await Task.Delay(1000);
                    LblCountDown.Text = $"{closingText} 2";
                    await Task.Delay(1000);
                    LblCountDown.Text = $"{closingText} 1";
                    await Task.Delay(1000);
                    LblCountDown.Text = $" ";
                    if (PopupNavigation.Instance.PopupStack.Count > 0 && !_isHide)
                        PopupNavigation.Instance.PopAsync();

                });
            }
        }

        private async void SetTimerForContinue()
        {

            try
            {

                OkButton.Text = $"{this.buttonText} 5";

                if (Device.RuntimePlatform.Equals(Device.Android))
                {
                    LblTipText.Text = "";
                    LblTipText.IsVisible = false;
                    OkButton.IsVisible = true;
                    OkAction.IsVisible = true;
                    await Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(1000);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            OkButton.Text = $"{this.buttonText} 4";
                        });


                        await Task.Delay(1000);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {

                            OkButton.Text = $"{this.buttonText} 3";
                        });

                        await Task.Delay(1000);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {

                            OkButton.Text = $"{this.buttonText} 2";
                        });
                        await Task.Delay(1000);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {

                            OkButton.Text = $"{this.buttonText} 1";
                        });
                        await Task.Delay(1000);
                        MainThread.BeginInvokeOnMainThread(() =>
                        {

                            OkButton.Text = $"{this.buttonText}";
                        });
                    });
                }
                else
                {
                    LblTipText.IsVisible = false;

                    //ImgLoader.IsVisible = true;
                    Device.BeginInvokeOnMainThread(async () =>
                    {

                        OkButton.Text = $"{this.buttonText} 5";
                        await Task.Delay(1000);
                        OkButton.Text = $"{this.buttonText} 4";
                        await Task.Delay(1000);
                        OkButton.Text = $"{this.buttonText} 3";
                        await Task.Delay(1000);
                        OkButton.Text = $"{this.buttonText} 2";
                        await Task.Delay(1000);
                        OkButton.Text = $"{this.buttonText} 1";
                        await Task.Delay(1000);
                        OkButton.Text = $"{this.buttonText}";

                    });


                }
            }
            catch (Exception ex)
            {

            }
        
        }

        void OkButton_Clicked(System.Object sender, System.EventArgs e)
        {

            try
            {
                PopupNavigation.Instance.PopAllAsync();
            }
            catch (Exception ex)
            {

            }
            
            if (BtnCancel.IsVisible)
            {
                if (OkButtonPress != null)
                    OkButtonPress.Invoke(sender,EventArgs.Empty);
            }

        }

        void DrMuscleButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (LblHeading.Text.Equals("New features!"))
            {
                Device.OpenUri(new Uri("https://dr-muscle.com/timeline"));
            }
            else if (BtnLearnMore.Text.Equals("Open Settings"))
            {
                PopupNavigation.Instance.PopAsync();
                PagesFactory.PushAsync<SettingsPage>();
            }
            else if (BtnLearnMore.Text.Equals("Cancel"))
            {
                PopupNavigation.Instance.PopAsync();
               // PagesFactory.PopAsync();
            }
            else
            {
                if (CheckTrialUser())
                    return;
                PopupNavigation.Instance.PopAsync();
                PagesFactory.PushAsync<LearnPage>();
            }
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

        void DrMuscleButtonCancel_Clicked(System.Object sender, System.EventArgs e)
        {
            if (BtnCancel.Text == "Learn more")
            {
                Device.OpenUri(new Uri("https://dr-muscle.com/timeline"));
            }
            else
                PopupNavigation.Instance.PopAsync();
        }
    }
}
