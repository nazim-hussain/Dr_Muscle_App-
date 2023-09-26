using System;
using System.Collections.Generic;
using Xamarin.Forms;
using DrMuscle.Resx;
using DrMuscle.Helpers;
using DrMuscle.Screens.User;

namespace DrMuscle.Cells
{
    public partial class WelcomeCell : ViewCell
    {
        public WelcomeCell()
        {
            InitializeComponent();
            LblWelcome.Text = AppResources.ThisIsBeginningWithSupport;
            if (App.IsV1User)
                LblGroupChat.IsVisible = false;
            else
                LblGroupChat.IsVisible = false;
        }

        private async void GroupChatTapped(object sender, EventArgs args)
        {
            await PagesFactory.PushAsync<GroupChatPage>();
        }
    }
}
