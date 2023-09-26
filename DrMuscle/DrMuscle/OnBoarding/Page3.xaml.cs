using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DrMuscle.OnBoarding
{
    public partial class Page3 : ContentView
    {
        public Page3()
        {
            InitializeComponent();
            mainView.Margin = new Thickness(0, 70, 0, 50);
            if (App.ScreenHeight > 668)
            {
                mainView.Margin = new Thickness(0, 110, 0, 50);
            }

            if (App.ScreenWidth > 375)
            {
                ImgLogo.WidthRequest = 170;
                ImgLogo.HeightRequest = 150;

                LblTitle.FontSize = 22;
                LblLine1.FontSize = 19;
                LblLine2.FontSize = 19;
                LblLine3.FontSize = 19;
                //LblLine4.FontSize = 19;

                LblSubHeading.FontSize = 20;
                ImgLeave1.WidthRequest = 55;
                ImgLeave2.WidthRequest = 55;
                LeavesStack.Margin = new Thickness(0, 50,0,0);

                LblAuthor1.FontSize = 16;
            }
        }
    }
}

