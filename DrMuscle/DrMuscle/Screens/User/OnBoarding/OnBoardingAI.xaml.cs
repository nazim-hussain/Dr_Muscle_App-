using System;
using System.Collections.Generic;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using Xamarin.Forms;
using DrMuscle.Resx;

namespace DrMuscle.Screens.User.OnBoarding
{
    public partial class OnBoardingAI : DrMusclePage
    {
        public OnBoardingAI()
        {
            InitializeComponent();
            HasSlideMenu = false;
            this.ToolbarItems.Clear();
            RefreshLocalized();

            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) => {
                RefreshLocalized();
            });
        }

        private void RefreshLocalized()
        {
            LblIAmNotLike.Text = AppResources.ImNotLikeOtherApps;
            BtnGotIt.Text = AppResources.GotIt;
        }

        void GotItClicked(object sender, System.EventArgs e)
        {
           
        }
    }
}
