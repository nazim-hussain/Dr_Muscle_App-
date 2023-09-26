using System;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using Xamarin.Forms;

namespace DrMuscle
{
	public class OBPage2 : OnBoardingPage
	{
		public OBPage2() : base("OnBoarding2.png")
		{
			TapGestureRecognizer tap = new TapGestureRecognizer();
			tap.Tapped += HandleTapped;
			ClickBox.GestureRecognizers.Add(tap);
		}

		public async void HandleTapped(object sender, EventArgs e)
		{
            await PagesFactory.PushAsync<OBPage3>();
        }
	}
}

