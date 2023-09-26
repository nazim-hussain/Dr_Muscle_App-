using Microsoft.AppCenter.Analytics;
using System;
using Xamarin.Forms;

namespace DrMuscle.Layout
{
	public class OnBoardingPage : DrMusclePage  
	{
		private string _backgroundImageResource;

		protected ContentView ClickBox;

		public OnBoardingPage(string backgroundImageResource = "OnBoarding1.png")
		{
			NavigationPage.SetHasNavigationBar(this, false);
            HasSlideMenu = false;
			BackgroundColor = Color.Black;
			_backgroundImageResource = backgroundImageResource;

			Image image = new Image
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Aspect = Aspect.AspectFill,
				Source = _backgroundImageResource
			};

			ClickBox = new ContentView
			{
				Content = image
			};

			AbsoluteLayout absoluteLayout = new AbsoluteLayout
			{
				BackgroundColor = Color.Black
			};



			AbsoluteLayout.SetLayoutFlags(ClickBox, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(ClickBox, new Rectangle(0f, 0f, 1f, 1f));
			absoluteLayout.Children.Add(ClickBox);

	
			Content = absoluteLayout;
		}

		public void FullScreenImagePage(String ImageName, string DescriptionText, int index, int count)
		{
			NavigationPage.SetHasNavigationBar(this, false);


		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Analytics.TrackEvent(GetType().FullName + ".OnAppearing");


		}
	}
}

