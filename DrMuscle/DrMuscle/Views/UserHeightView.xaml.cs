using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using DrMuscle.Helpers;
//using DrMuscle.Interfaces;
using DrMuscle.Message;
using DrMuscle.Services;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class UserHeightView : PopupPage
    {
        bool IsFeet = true;
        IList<int> cmList = new List<int>();
        IList<int> feetList = new List<int>();
        IList<int> inchList = new List<int>();
        public UserHeightView()
        {
            InitializeComponent();
            feetList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            PickerFeet.ItemsSource = feetList;
            inchList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            PickerInch.ItemsSource = inchList;

            for (int i = 24; i < 330; i++)
            {
                cmList.Add(i);
            }
            PickerCM.ItemsSource = cmList;
            FeetGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            FeetGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;

            if (!App.IsNUX)
                BtnCancel.IsVisible = true;
        }

        public async void BtnFeetClicked(object sender, EventArgs args)
        {
            //BtnLbs.BackgroundColor = Color.FromHex("#5CD196");
            BtnCM.BackgroundColor = Color.Transparent;
            // LocalDBManager.Instance.SetDBSetting("massunit", "lb");
            CMGradient.BackgroundColor = Color.Transparent;
            CMGradient.BackgroundColor = Color.Transparent;
            FeetGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            FeetGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            BtnCM.TextColor = Color.FromHex("#0C2432");
            PickerCM.IsVisible = false;
            FeetGrid.IsVisible = true;
            BtnFeet.TextColor = Color.White;
            IsFeet = true;
        }

        public async void BtnCMClicked(object sender, EventArgs args)
        {
            IsFeet = false;
            PickerCM.IsVisible = true;
            FeetGrid.IsVisible = false;
            BtnFeet.BackgroundColor = Color.Transparent;
            //BtnKg.BackgroundColor = Color.FromHex("#5CD196");
            // LocalDBManager.Instance.SetDBSetting("massunit", "kg");
            BtnCM.TextColor = Color.White;
            BtnFeet.TextColor = Color.FromHex("#0C2432");
            CMGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            CMGradient.BackgroundColor = Constants.AppThemeConstants.BlueColor;
            FeetGradient.BackgroundColor = Color.Transparent;
            FeetGradient.BackgroundColor = Color.Transparent;
        }


        public async void BtnDoneClicked(object sender, EventArgs args)
        {

            await PopupNavigation.Instance.PopAsync();

            var generalText = "";
            if (IsFeet)
            {
                generalText = $"{feetList[PickerFeet.SelectedIndex]}' {inchList[PickerInch.SelectedIndex]}''";
                Config.UserHeight = (float)convertFeetandInchesToCentimeter(feetList[PickerFeet.SelectedIndex], inchList[PickerInch.SelectedIndex]);
            }
            else
            {
                Config.UserHeight = cmList[PickerCM.SelectedIndex];
                generalText = $"{cmList[PickerCM.SelectedIndex]} Centimeters";
            }

            if (!App.IsNUX)
            {
                //IUserService _userService = new UserApiServices();
                
                var isupdated = await DrMuscleRestClient.Instance.SetUserHeight(new UserInfosModel() { Height = Config.UserHeight });
                LocalDBManager.Instance.SetDBSetting("Height", Config.UserHeight.ToString());
            }
            
                Xamarin.Forms.MessagingCenter.Send<GeneralMessage>(new GeneralMessage() { GeneralText = generalText, PopupEnum = Enums.GeneralPopupEnum.UserHeight }, "GeneralMessage");


            //var weigh = new MultiUnityWeight(Convert.ToDecimal(EntryBodyWeight.Text.Replace(",", "."), CultureInfo.InvariantCulture), Config.MassUnit);
            //Xamarin.Forms.MessagingCenter.Send<BodyweightMessage>(new BodyweightMessage() { BodyWeight = bodyweight, WeightType = _weightType, Weight = (double)weigh.Kg }, "BodyweightMessage");
        }

        public static double convertFeetandInchesToCentimeter(double heightInFeet, double heightInInches)
        {
            return (heightInFeet * 30.48) + (heightInInches * 2.54);
        }
        async void BtnCancel_Clicked(System.Object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            await PagesFactory.PopToRootAsync();
        }
    }
}
