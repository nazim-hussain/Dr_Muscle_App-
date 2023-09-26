using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using DrMuscle.Resx;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserOutgoingCell : ViewCell
    {
        public UserOutgoingCell()
        {
            InitializeComponent();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var message = (Messages)this.BindingContext;
                    if (message == null)
                        return;
                    if (message.UserId.ToLower().Equals("etiennejuneau@gmail.com"))
                    {
                        imgOutProfilePic.IsVisible = true;
                        FrmProfile.IsVisible = false;
                        if (message.IsFromAI)
                        {
                            imgOutProfilePic.Source = "Icon2";
                            nameLabel.Text = "Dr. Muscle AI";
                        }
                        else
                        {
                            imgOutProfilePic.Source = "victoriaProfile.png";//"adminprofile.png";
                            nameLabel.Text = "Victoria from Dr. Muscle";
                        }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(message.ProfileUrl) && message.ProfileUrl.ToLower().Contains("facebook") || message.ProfileUrl.ToLower().Contains("google"))
                        {
                            imgOutProfilePic.IsVisible = true;
                            FrmProfile.IsVisible = false;
                            imgOutProfilePic.Source = message.ProfileUrl;
                        }
                        else
                        {
                            imgOutProfilePic.Source = null;
                            imgOutProfilePic.IsVisible = false;
                            FrmProfile.IsVisible = true;
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
                        if (message.IsFromAI)
                        {
                            imgOutProfilePic.IsVisible = true;
                            FrmProfile.IsVisible = false;
                            imgOutProfilePic.Source = "Icon2";
                            nameLabel.Text = "Dr. Muscle AI";
                        }
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }

        async void Username_Tapped(object sender, EventArgs e)
        {
            var message = (Messages)this.BindingContext;
            if (message == null)
                return;
            bool IsAdmin = LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("etiennejuneau@gmail.com") || LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("jorum@dr-muscle.com");

            if (!message.UserId.ToLower().Equals("etiennejuneau@gmail.com") && IsAdmin)
            {
                //Clipboard.SetTextAsync(message.UserId);
                //Plugin.Toast.CrossToastPopUp.Current.ShowToastMessage("Copied to clipboard", Plugin.Toast.Abstractions.ToastLength.Short);
                bool isMuted = App.MutedUserList.Contains(message.UserId);
                if (!message.UserId.ToLower().Equals("etiennejuneau@gmail.com") && IsAdmin)
                {

                    ActionSheetConfig config = new ActionSheetConfig();
                    config.AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray);
                    config.Add($"Get {message.UserId}", () =>
                    {
                        Clipboard.SetTextAsync(message.UserId);
                        Plugin.Toast.CrossToastPopUp.Current.ShowToastMessage("Email id copied to clipboard", Plugin.Toast.Abstractions.ToastLength.Short);
                    });
                    config.Add(isMuted ? $"Unmute {message.UserId}" : $"Mute {message.UserId}", () =>
                    {
                        var unmuteUserMessage = new Message.MuteUnmuteUserMessage();
                        unmuteUserMessage.IsMuted = isMuted;
                        unmuteUserMessage.UserId = message.UserId;
                        MessagingCenter.Send(unmuteUserMessage, "MuteUnmuteUserMessage");
                    });
                    config.Add("Delete message", () =>
                    {
                        var deleteMessage = new Message.DeleteChatMessage();
                        deleteMessage.FullMessage = message;
                        MessagingCenter.Send(deleteMessage, "DeleteChatMessage");

                    });
                    config.SetCancel(AppResources.Cancel, null);

                    config.SetTitle(isMuted ? $"Get email or unmute {message.UserId}?" : $"Get email or mute {message.UserId}?");
                    UserDialogs.Instance.ActionSheet(config);

                }
            }
        }

    }
}
