using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Message;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class FullReview : PopupPage
    {
        public FullReview()
        {
            InitializeComponent();
            
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //if (App.IsV1User)
            //{
            //    //LastSaperator.IsVisible = false;
            //    Upgrade.IsVisible = false;
            //}

            if (CurrentLog.Instance.IsMonthlyUser == null)
            {
                var result = await DrMuscleRestClient.Instance.IsMonthlyUser();
                if (result != null)
                {
                    CurrentLog.Instance.IsMonthlyUser = result.Result;
                    Upgrade.IsVisible = result.Result;
                }
            }
            else
                Upgrade.IsVisible = (bool)CurrentLog.Instance.IsMonthlyUser;
        }

        async void MonthFree_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            await Task.Delay(1000);
            Xamarin.Forms.MessagingCenter.Send<ShareMessage>(new ShareMessage() { }, "ShareMessage");

        }

        async void Upgrade_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            //await PagesFactory.PushAsync<DrMuscle.Screens.Subscription.SubscriptionPage>();
            //
            Device.OpenUri(new Uri("mailto:support@drmuscleapp.com?subject=Upgrade%20to%20annual%20(4%20months%20free)&body=I%20would%20like%20to%20upgrade%20to%20the%20annual%20plan%20and%20get%204%20months%20free.%20Please%20tell%20me%20more."));
        }

        async void ReviewButton_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            await Task.Delay(500);
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                try
                {
                    Device.OpenUri(new Uri("market://details?id=com.drmaxmuscle.dr_max_muscle"));
                }
                catch (Exception)
                {
                    Device.OpenUri(new Uri("https://play.google.com/store/apps/details?id=com.drmaxmuscle.dr_max_muscle&hl=en_IN"));
                }
            }
            else
            {
                try
                {
                    Device.OpenUri(new Uri("itms-apps://itunes.apple.com/app/id1073943857"));
                }
                catch (Exception)
                {

                    Device.OpenUri(new Uri("https://itunes.apple.com/us/app/dr-muscle/id1073943857?mt=8"));
                }
            }
        }
    }
}
