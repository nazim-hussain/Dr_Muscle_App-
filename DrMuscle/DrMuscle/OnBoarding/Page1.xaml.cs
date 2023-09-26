using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DrMuscle.OnBoarding
{
    public partial class Page1 : ContentView
    {
        public Page1()
        {
            InitializeComponent();
            mainStack.Margin = new Thickness(0, 70, 0, 50);
            if (App.ScreenHeight > 668)
            {
                mainStack.Margin = new Thickness(0, 110, 0, 50);
            }
            if (App.ScreenWidth > 375)
            {
                imgLogo.WidthRequest = 170;
                imgLogo.HeightRequest = 230;
                ImgStar1.WidthRequest = 130;
                ImgStar2.WidthRequest = 130;
                LblReview1.FontSize = 17;
                LblReview2.FontSize = 17;
                LblAuthor1.FontSize = 16;
                LblAuthor2.FontSize = 16;
            }

        }
    }
}

