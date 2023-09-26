using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using Xamarin.Forms;

namespace DrMuscle.Screens.User
{
    public partial class FAQPage : DrMusclePage
    {
        string supportUrl = "";
        public FAQPage()
        {
            InitializeComponent();
            Title = "Help";
            //SendBirdClient.Init("91658003-270F-446B-BD61-0043FAA8D641");
        }

        

        //async void HandleGroupChannelCreateHandler(GroupChannel channel, SendBirdException e)
        //{
        //    if (e != null)
        //    {
        //        return;
        //    }

        //    Device.BeginInvokeOnMainThread(async () =>
        //    {
        //        supportUrl = channel.Url;
        //        UserDialogs.Instance.HideLoading();
        //        CurrentLog.Instance.ChannelUrl = channel.Url;
        //        await PagesFactory.PushAsync<SupportPage>();
        //    });
        //}

    }
}
