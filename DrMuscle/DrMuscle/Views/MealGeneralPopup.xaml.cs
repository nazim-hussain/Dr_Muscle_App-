using System;
using System.Collections.Generic;
using DrMuscle.Message;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class MealGeneralPopup : PopupPage
    {
        public string Title { get; set; }
        public Enums.GeneralPopupEnum GeneralEnum { get; set; }
        public MealGeneralPopup()
        {
            InitializeComponent();
        }

        public void SetPopupTitle(string title, Enums.GeneralPopupEnum popupEnum, string subTitle = "", string placeholder = "")
        {
            LblTitle.Text = title;
            LblSubTitle.Text = subTitle;
            GeneralEnum = popupEnum;
            EditorMealInfo.Placeholder = placeholder;
        }
        void BtnSave_Clicked(System.Object sender, System.EventArgs e)
        {
            if (string.IsNullOrEmpty(EditorMealInfo.Text) || string.IsNullOrWhiteSpace(EditorMealInfo.Text))
                return;
            PopupNavigation.Instance.PopAllAsync();
            Xamarin.Forms.MessagingCenter.Send<GeneralMessage>(new GeneralMessage() { GeneralText = EditorMealInfo.Text, PopupEnum = GeneralEnum }, "GeneralMessage");

        }

        void BtnCancel_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAllAsync();
            Xamarin.Forms.MessagingCenter.Send<GeneralMessage>(new GeneralMessage() { GeneralText = EditorMealInfo.Text, PopupEnum = GeneralEnum, IsCanceled = true }, "GeneralMessage");
        }
        }
}
