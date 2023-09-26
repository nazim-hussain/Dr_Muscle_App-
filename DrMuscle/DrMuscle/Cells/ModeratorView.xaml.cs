using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using DrMuscle.Message;
using DrMuscle.Resx;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModeratorView : ViewCell
    {
        public ModeratorView()
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
                    imgInProfilePic.Source = "adminprofile.png";
                    if (message.IsFromAI)
                    {
                        imgInProfilePic.Source = "Icon2";
                        nameLabel.Text = "Dr. Muscle AI";
                    }
                    else
                    {
                        imgInProfilePic.Source = "victoriaProfile.png";// "adminprofile.png";
                        nameLabel.Text = "Victoria from Dr. Muscle";
                    }
                    
                }
                else
                {
                    if (!string.IsNullOrEmpty(message.ProfileUrl) && message.ProfileUrl.ToLower().Contains("facebook") || message.ProfileUrl.ToLower().Contains("google"))
                    {
                        imgInProfilePic.IsVisible = true;
                        FrmProfile.IsVisible = false;
                        imgInProfilePic.Source = message.ProfileUrl;
                    }
                    else
                    {
                        imgInProfilePic.Source = null;
                        imgInProfilePic.IsVisible = false;
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
                        imgInProfilePic.IsVisible = true;
                        FrmProfile.IsVisible = false;
                        imgInProfilePic.Source = "Icon2";
                        nameLabel.Text = "Dr. Muscle AI";
                    }
                }

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
            if (message.ChatType == ChannelType.Group)
                return;
            bool isMuted = App.MutedUserList.Contains(message.UserId);
            bool IsAdmin = LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("etiennejuneau@gmail.com") || LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("jorum@dr-muscle.com");
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
                    var unmuteUserMessage = new MuteUnmuteUserMessage();
                    unmuteUserMessage.IsMuted = isMuted;
                    unmuteUserMessage.UserId = message.UserId;
                    MessagingCenter.Send(unmuteUserMessage, "MuteUnmuteUserMessage");
                });
                config.Add("Delete message", () =>
                {
                    var deleteMessage = new DeleteChatMessage();
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
