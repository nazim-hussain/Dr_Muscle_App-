using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using DrMuscle.Enums;
using DrMuscle.Message;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class MealBodyweightPopup : PopupPage
    {

        WeightType _weightType;
        bool IsLb = true;
        public MealBodyweightPopup()
        {
            InitializeComponent();
            //LocalDBManager.Instance.SetDBSetting("massunit", "lb");


            IsLb = Config.MassUnit == "lb";
            if (IsLb)
            {
                LbGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
                LbGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;

            }
            else
            {
                BtnKgClicked(BtnKg, EventArgs.Empty);
            }
        }

        public void SetBodyWeightProperty(WeightType weightType)
        {
            _weightType = weightType;
            BtnCancel.IsVisible = false;
            if (weightType == WeightType.PreviousMonth)
            {

                LblTitle.Text = "Your body weight a month ago?";
            }
            else if (weightType == WeightType.CurrentMonth)
            {
                if (string.IsNullOrEmpty(Config.UserEmail))
                    BtnCancel.IsVisible = true;
                LblTitle.Text = "Welcome! What is your body weight today?";
            }
            else if (weightType == WeightType.PredictedMonth)
            {
                LblTitle.Text = "Your target body weight?";
            }
            else if (weightType == WeightType.UpdateBodyWeight)
            {
                BtnCancel.IsVisible = true;
                LblTitle.Text = "Update your body weight";
            }
            else if (weightType == WeightType.UpdatePredictedWeight)
            {
                BtnCancel.IsVisible = true;
                LblTitle.Text = "Your target body weight in 1 month";
            }
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
                var weight = Convert.ToDouble(EntryBodyWeight.Text.Replace(",", "."), CultureInfo.InvariantCulture);
                if (weight < 1)
                {
                    UserDialogs.Instance.Alert("Please enter valid weight", "Error", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {

            }
            Config.MassUnit = IsLb ? "lb" : "kg";
            var bodyweight = EntryBodyWeight.Text;
            await PopupNavigation.Instance.PopAsync();
            var weigh = new DrMuscleWebApiSharedModel.MultiUnityWeight(Convert.ToDecimal(EntryBodyWeight.Text.Replace(",", "."), CultureInfo.InvariantCulture), Config.MassUnit);
            Xamarin.Forms.MessagingCenter.Send<BodyweightMessage>(new BodyweightMessage() { BodyWeight = bodyweight, WeightType = _weightType, Weight = (double)weigh.Kg }, "BodyweightMessage");
        }

        public async void BtnLbsClicked(object sender, EventArgs args)
        {
            //BtnLbs.BackgroundColor = Color.FromHex("#5CD196");
            BtnKg.BackgroundColor = Color.Transparent;
            // LocalDBManager.Instance.SetDBSetting("massunit", "lb");
            KgGradient.BackgroundColor = Color.Transparent;
            KgGradient.BackgroundColor = Color.Transparent;
            LbGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            LbGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            BtnKg.TextColor = Color.FromHex("#0C2432");
            BtnLbs.TextColor = Color.White;
            IsLb = true;
        }

        public async void BtnKgClicked(object sender, EventArgs args)
        {
            IsLb = false;
            BtnLbs.BackgroundColor = Color.Transparent;
            //BtnKg.BackgroundColor = Color.FromHex("#5CD196");
            // LocalDBManager.Instance.SetDBSetting("massunit", "kg");
            BtnKg.TextColor = Color.White;
            BtnLbs.TextColor = Color.FromHex("#0C2432");
            KgGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            KgGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            LbGradient.BackgroundColor = Color.Transparent;
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

        async void BtnCancel_Clicked(System.Object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
