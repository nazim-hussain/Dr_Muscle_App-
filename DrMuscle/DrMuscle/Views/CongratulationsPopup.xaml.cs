using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Message;
using DrMuscle.Screens.Subscription;
using DrMuscle.Screens.User;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class CongratulationsPopup : PopupPage
    {
        TapGestureRecognizer okGuesture;
        string buttonText = "";
        public event EventHandler OkButtonPress;
        public bool _isHide { get; set; }
        public int workoutCount = 0;
        public string lifted = "";
        public CongratulationsPopup(string image, string title, string subtitle, string buttonText)
        {
            InitializeComponent();
            okGuesture = new TapGestureRecognizer();
            okGuesture.Tapped += DrMuscleButtonCancel_Clicked;
            OkAction.GestureRecognizers.Add(okGuesture);
            ImgName.Source = image;
            
            LblHeading.Text = title;
            LblSubHead.Text = $"{subtitle}";
            OkButton.Text = buttonText;
            this.buttonText = buttonText;
            LblTipText.IsVisible = false;
            

            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            MyParticleCanvas.IsActive = true;
            MyParticleCanvas.IsRunning = true;
            //await Task.Delay(Device.RuntimePlatform.Equals(Device.Android) ? 9000 : 5000);
            //MyParticleCanvas.IsActive = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isHide = true;
            MessagingCenter.Unsubscribe<Message.WorkoutLoadedMessage>(this, "WorkoutLoadedMessage");
        }
        private async Task SetLoadingSummary(TapGestureRecognizer okGuesture)
        {
            //await Task.Delay(250);

            //OkButton.Text = "Loading.";

            //await Task.Delay(700);
            //OkButton.Text = "Loading..";

            //await Task.Delay(700);

            //OkButton.Text = "Loading...";
            //await Task.Delay(700);
            OkButton.Text = this.buttonText;
            OkAction.GestureRecognizers.Add(okGuesture);

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

        void OkButton_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();

            if (OkButtonPress != null)
                OkButtonPress.Invoke(sender, EventArgs.Empty);


        }

        void DrMuscleButton_Clicked(System.Object sender, System.EventArgs e)
        {
            
                if (CheckTrialUser())
                    return;
                PopupNavigation.Instance.PopAsync();
                PagesFactory.PushAsync<LearnPage>();
            
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
            PopupNavigation.Instance.PopAsync();
        }

        async void DrMuscleButtonShareTrial_Clicked(System.Object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
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
            DependencyService.Get<IFirebase>().LogEvent("popup_share_free_trial", "share");
        }

        async void DrMuscleButtonFeedback_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();

            var page = new FeedbackView();
            await PopupNavigation.Instance.PushAsync(page);

        }

        private async void SetupLastWorkoutWas(string workoutwas)
        {
           
        }
    }
}
