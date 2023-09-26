using System;
using DrMuscle.Layout;
using DrMuscle.Screens.User.TellMeMore;
using Xamarin.Forms;

namespace DrMuscle
{
	public class OBPage3 : OnBoardingPage
	{
		public OBPage3() : base("OnBoarding3.png")
		{
			TapGestureRecognizer tap = new TapGestureRecognizer();
			tap.Tapped += HandleTapped;
			ClickBox.GestureRecognizers.Add(tap);
		}

		public void HandleTapped(object sender, EventArgs e)
		{
			LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
			Application.Current.MainPage = ((App)Application.Current).BuildNavigationPage(new TwoChoices("Tell me more about yourself", "Man", "Woman", "gender", "Man", "Woman"));
		}
	}
}

