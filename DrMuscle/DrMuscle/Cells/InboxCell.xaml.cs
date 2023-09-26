using System;
using System.Collections.Generic;
using System.Xml;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public partial class InboxCell : ViewCell
    {
        public InboxCell()
        {
            InitializeComponent();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            try
            {

                var message = (Messages)this.BindingContext;
                if (message == null)
                    return;
                if (message.UserId.ToLower().Equals("etiennejuneau@gmail.com"))
                {
                    imgInProfilePic.IsVisible = true;
                    FrmProfile.IsVisible = false;
                    imgInProfilePic.Source = "victoriaProfile.png";//"adminprofile.png";
                    LblName.Text = $"Victoria, {message.NormalUSerName}";
                    LblName.FontAttributes = FontAttributes.None;
                    ContentGrid.BackgroundColor = Color.Transparent;
                    if (message.IsV1User)
                    {
                        ContentGrid.BackgroundColor = AppThemeConstants.GreenTransparentColor;
                    }
                    else
                    {
                        ContentGrid.BackgroundColor = Color.Transparent;
                    }
                }
                else
                {

                    if (message.IsV1User)
                    {
                        ContentGrid.BackgroundColor = AppThemeConstants.GreenTransparentColor;
                    } else
                    {
                        ContentGrid.BackgroundColor = Color.Transparent;
                    }
                    imgInProfilePic.Source = null;
                    imgInProfilePic.IsVisible = false;
                    FrmProfile.IsVisible = true;
                    LblName.FontAttributes = FontAttributes.Bold;
                    if (message.IsBothReplied)
                        LblName.Text = $"{message.Nickname}, Victoria";
                    else
                        LblName.Text = $"{message.Nickname}";
                    LblProfileText.Text = message.Nickname.Length > 0 ? message.Nickname.Substring(0, 1).ToUpper() : "";
                    Color backColor = AppThemeConstants.RandomColor;
                    if (AppThemeConstants.ProfileColor.ContainsKey(message.UserId))
                    {
                        FrmProfile.BackgroundColor = AppThemeConstants.ProfileColor[message.UserId];
                    }
                    else
                    {
                        AppThemeConstants.ProfileColor.Add(message.UserId, backColor);
                        FrmProfile.BackgroundColor = backColor;
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
