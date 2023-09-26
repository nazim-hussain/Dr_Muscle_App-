using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DrMuscle.OnBoarding
{
    public partial class Page2 : ContentView
    {
        public Page2()
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

                ImgStar1.WidthRequest = 130;
                ImgStar1.Margin = new Thickness(0, 25, 0, 0);
                ReviewStack.Margin = new Thickness(0, 25, 0, 0);
                LblReview1.FontSize = 17;
                LblAuthor1.FontSize = 16;
            }
        }
    }
}

