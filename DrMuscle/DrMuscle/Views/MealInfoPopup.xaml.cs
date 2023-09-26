using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using DrMuscle.Message;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class MealInfoPopup : PopupPage
    {
        public MealInfoPopup()
        {
            InitializeComponent();
        }



        async void BtnSave_Clicked(System.Object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(EditorMealInfo.Text) || string.IsNullOrWhiteSpace(EditorMealInfo.Text))
                return;
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync("Please check your internet connection", "Internet error");
                return;
            }
            PopupNavigation.Instance.PopAllAsync();
            Xamarin.Forms.MessagingCenter.Send<AddedMealInfoMessage>(new AddedMealInfoMessage() { MealInfoStr = EditorMealInfo.Text }, "AddedMealInfoMessage");

        }

        void BtnCancel_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
           
            Xamarin.Forms.MessagingCenter.Send<AddedMealInfoMessage>(new AddedMealInfoMessage() { MealInfoStr = EditorMealInfo.Text, IsCanceled = true }, "AddedMealInfoMessage");
        }
    }
}
