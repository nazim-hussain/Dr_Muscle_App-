using System;
using System.Collections.Generic;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Screens.User.OnBoarding;
using OxyPlot;
using Xamarin.Forms;

namespace DrMuscle.OnBoarding
{
    public partial class WalkThroughPage : DrMusclePage
    {
        private View[] _views;

        public WalkThroughPage()
        {
            InitializeComponent();
            // if (LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value == "Beta")
            // {
                carouserView.ItemsSource = new string[] { "Page1", "Page2", "Page3", "Page4" };
                indicatorView.ItemsSource = new string[] { "Page1", "Page2", "Page3", "Page4" };
            // } else
            // {
            //     carouserView.ItemsSource = new string[] { "Page1", "Page2", "Page3" };
            //     indicatorView.ItemsSource = new string[] { "Page1", "Page2", "Page3" };
            // }
            

        }

        void carouserView_PositionChanged(System.Object sender, Xamarin.Forms.PositionChangedEventArgs e)
        {
            // if (LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value == "Beta")
            // {
                if (e.PreviousPosition == 3 && e.CurrentPosition == 0)
                {
                    carouserView.Position = 3;
                    return;
                }
            // }
            // else
            // {
            //     if (e.PreviousPosition == 2 && e.CurrentPosition == 0)
            //     {
            //         carouserView.Position = 2;
            //         return;
            //     }
            // }
            
            btnContinue.IsVisible = e.CurrentPosition == 3 ? false : true;
            indicatorView.IsVisible = e.CurrentPosition == 3 ? false : true;
            indicatorView.Position = e.CurrentPosition;
            if (e.CurrentPosition == 0)
            {
                btnContinue.Text = "Get started";
            }
            else
            {
                btnContinue.Text = "Continue";
            }
            
        }

        void btnContinue_Clicked(System.Object sender, System.EventArgs e)
        {
            if (carouserView.Position == 0)
            {
                carouserView.Position = 1;
                indicatorView.Position = 1;
            }
            else if (carouserView.Position == 1)
            {
                carouserView.Position = 2;
                indicatorView.Position = 2;
            }
            else if (carouserView.Position == 2)
            {
                // if (LocalDBManager.Instance.GetDBSetting("BetaVersion")?.Value == "Beta")
                // {
                    carouserView.Position = 3;
                    indicatorView.Position = 3;
                // }
                //  else
                //     PagesFactory.PopThenPushAsync<MainOnboardingPage>(true);
            }
            else 
            {
                PagesFactory.PopThenPushAsync<MainOnboardingPage>(true); 
            }


        }
    }
}

