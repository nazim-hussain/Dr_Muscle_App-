using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Message;
using DrMuscle.Screens.Subscription;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AIAnalysisCell : ViewCell
	{
		public AIAnalysisCell ()
		{
			InitializeComponent ();
		}

		private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
		{
			PagesFactory.PushAsync<SubscriptionPage>();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			BotModel model = this.BindingContext as BotModel;

			if (model != null && !string.IsNullOrEmpty(model.Part2) && model.Part2.Contains("Learn more."))
			{
				gridChatButtons.IsVisible = false;
			}
			else
			{
				if (model != null && !string.IsNullOrEmpty(model.LevelUpText) && model.LevelUpText.Contains("2"))
				{
					gridChatButtons.IsVisible = false;
					return;
				}
				gridChatButtons.IsVisible = true;
			}
		}

		private async void HelpWithGoal_Clicked(object sender, EventArgs args)
		{
			((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[2];
			await Task.Delay(300);
			Xamarin.Forms.MessagingCenter.Send<HelpWithGoalChatMessage>(new HelpWithGoalChatMessage(), "HelpWithGoalChatMessage");
			
		}

		private void OpenChat_Clicked(object sender, EventArgs args)
		{
			((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[2];
		}
	}
}
