using DrMuscle.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using DrMuscle.Helpers;

namespace DrMuscle.Screens.User.TellMeMore
{
    public partial class OneEntry
    {
        private string SettingKey { get; set; }

        public OneEntry(string questionText, string entryPlaceholder, string settingKey) : base()
        {
            InitializeComponent();
            BackgroundImage = "Background2.png";
            QuestionLabel.Text = questionText;
            FirstEntry.Placeholder = entryPlaceholder;
            NextButton.Clicked += NextButton_Clicked;
            SettingKey = settingKey;
        }

        private async void NextButton_Clicked(object sender, EventArgs e)
        {
            LocalDBManager.Instance.SetDBSetting("onboarding_seen", "true");
            LocalDBManager.Instance.SetDBSetting(SettingKey, FirstEntry.Text);
            await PagesFactory.PushAsync<WelcomePage>();
        }
    }
}
