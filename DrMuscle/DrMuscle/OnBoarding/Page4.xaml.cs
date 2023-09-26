using System;
using System.Collections.Generic;
using DrMuscle.Screens.User.OnBoarding;
using Xamarin.Forms;

namespace DrMuscle.OnBoarding
{
    public partial class Page4 : ContentView
    {
        public Page4()
        {
            InitializeComponent();
            mainView.Margin = new Thickness(0, 70, 0, 0);
            indicatorView.ItemsSource = new string[] { "Page1", "Page2", "Page3", "Page4" };
            indicatorView.Position = 3;
            if (App.ScreenHeight > 668)
            {
                mainView.Margin = new Thickness(0, 110, 0, 0);
            }

            if (App.ScreenWidth > 375)
            {
                //ImgLogo.WidthRequest = 170;
                //ImgLogo.HeightRequest = 150;

                LblTitle.FontSize = 22;
                //LblLine1.FontSize = 19;
                //LblLine2.FontSize = 19;
                //LblLine3.FontSize = 19;
                //LblLine4.FontSize = 19;

                //LblSubHeading.FontSize = 20;
                //ImgLeave1.WidthRequest = 55;
                //ImgLeave2.WidthRequest = 55;
                //LeavesStack.Margin = new Thickness(0, 50, 0, 0);

                LblAuthor1.FontSize = 16;
            }
        }
        void btnContinue_Clicked(System.Object sender, System.EventArgs e)
        {
            Helpers.PagesFactory.PopThenPushAsync<MainOnboardingPage>(true);
        }
    }
}

