using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using DrMuscle.Message;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BodyweightPopup : PopupPage
    {
        public BodyweightPopup()
        {
            InitializeComponent();
            LocalDBManager.Instance.SetDBSetting("massunit", "lb");
            LbGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
        }

        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            EntryBodyWeight.Focus();
        }
        public async void BtnDoneClicked(object sender, EventArgs args)
        {
            if (string.IsNullOrEmpty(EntryBodyWeight.Text) || string.IsNullOrWhiteSpace(EntryBodyWeight.Text))
                return;
            try
            {
                var weight = int.Parse(EntryBodyWeight.Text);
                if (weight < 1)
                {
                    
                    await UserDialogs.Instance.AlertAsync(new AlertConfig()
                    {
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        Message = "Please enter valid weight",
                        Title = "Error"
                    });
                    return;
                }
            }
            catch (Exception ex)
            {

            }
            
            var bodyweight = EntryBodyWeight.Text;
            await PopupNavigation.Instance.PopAsync();

            Xamarin.Forms.MessagingCenter.Send<BodyweightMessage>(new BodyweightMessage() { BodyWeight = bodyweight }, "BodyweightMessage");
        }

        public async void BtnLbsClicked(object sender, EventArgs args)
        {
            //BtnLbs.BackgroundColor = Color.FromHex("#5CD196");
            BtnKg.BackgroundColor = Color.Transparent;
            LocalDBManager.Instance.SetDBSetting("massunit", "lb");
            KgGradient.BackgroundColor = Color.Transparent;
            LbGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            BtnKg.TextColor = Color.FromHex("#0C2432");
            BtnLbs.TextColor = Color.White;
        }

        public async void BtnKgClicked(object sender, EventArgs args)
        {
            BtnLbs.BackgroundColor = Color.Transparent; 
            //BtnKg.BackgroundColor = Color.FromHex("#5CD196");
            LocalDBManager.Instance.SetDBSetting("massunit", "kg");
            BtnKg.TextColor = Color.White;
            BtnLbs.TextColor = Color.FromHex("#0C2432");
            KgGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            LbGradient.BackgroundColor = Color.Transparent;
        }

        protected void BodyweightPopup_OnTextChanged(object obj, TextChangedEventArgs args)
        {
            try
            {

            Entry entry = (Xamarin.Forms.Entry)obj;
            const string textRegex = @"^\d+(?:[\.,]\d{0,5})?$";
            var text = entry.Text.Replace(",", ".");
            bool IsValid = Regex.IsMatch(text, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (IsValid == false && !string.IsNullOrEmpty(entry.Text))
            {
                double result;
                entry.Text = entry.Text.Substring(0, entry.Text.Length - 1);
                double.TryParse(entry.Text, out result);
                entry.Text = result.ToString();
            }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
