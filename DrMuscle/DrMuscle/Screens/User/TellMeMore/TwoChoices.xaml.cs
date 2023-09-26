using DrMuscle.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DrMuscle.Screens.User.TellMeMore
{
    public partial class TwoChoices
    {
		private string SettingKey { get; set; }
		private string ChooseText { get; set; }
		private string Choice1ValueText { get; set; }
		private string Choice2ValueText { get; set; }

        public EventHandler OnNext { get; set; }

        public TwoChoices(string chooseText, string choice1Text, string choice2Text, string settingKey, string choice1ValueText, string choice2ValueText) : base()
        {
            InitializeComponent();
            BackgroundImage = "Background2.png";
            SetTextAndSetting(chooseText, choice1Text, choice2Text, settingKey, choice1ValueText, choice2ValueText);
        }

		private void SetTextAndSetting(string chooseText, string choice1Text, string choice2Text, string settingKey, string choice1ValueText, string choice2ValueText)
		{
			ChooseText = chooseText;
			SettingKey = settingKey;
			Choice1ValueText = choice1ValueText;
			Choice2ValueText = choice2ValueText;

			ChoiceLabel.Text = chooseText;

			Choice1Button.Text = choice1Text;
			Choice2Button.Text = choice2Text;

			Choice1Button.Clicked += Choice1Button_Clicked;
			Choice2Button.Clicked += Choice2Button_Clicked;
		}

        private async void Choice2Button_Clicked(object sender, EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting(SettingKey, Choice2ValueText);
			GoNextPage(sender, e);
            
        }

		private async void GoNextPage(object sender, EventArgs e)
		{
            if (OnNext != null)
                OnNext(sender, e);
			
			switch (ChooseText)
			{
				default:
				case "Tell me more about yourself":
					await Navigation.PushAsync(new TwoChoices("Choose one: lbs or kg?", "lbs", "kg", "massunit", "lb", "kg"), false);
                    break;
				case "Choose one: lbs or kg?":
					await Navigation.PushAsync(new TwoChoices("I've been working out for...", "Less than 1 year", "More than 1 year", "experience", "less1year", "more1year"), false);
					break;
				case "I've been working out for...":
					await Navigation.PushAsync(new TwoChoices("I work out at...", "Home", "Gym", "workout_place", "home", "gym"), false);
					break;
				case "I work out at...":
					await Navigation.PushAsync(new OneEntry("What's your firstname?", "Tap to enter your firstname", "firstname"), false);
					break;
			}
		}

		private async void Choice1Button_Clicked(object sender, EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting(SettingKey, Choice1ValueText);
			GoNextPage(sender ,e);
        }
    }
}
