using DrMuscle.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using DrMuscle.Cells;

namespace DrMuscle.Cells
{
    public class ChatDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate incomingDataTemplate;
        private readonly DataTemplate outgoingDataTemplate;
        private readonly DataTemplate welcomeDataTemplate;
        private readonly DataTemplate moderatorDataTemplate;
        private readonly DataTemplate ModeratoroutgoingDataTemplate;
        public ChatDataTemplateSelector()
        {

            // Retain instances!
            this.incomingDataTemplate = new DataTemplate(typeof(IncommingViewCell));
            this.outgoingDataTemplate = new DataTemplate(typeof(UserOutgoingCell));
            this.ModeratoroutgoingDataTemplate = new DataTemplate(typeof(OutgoingViewCell));
            this.welcomeDataTemplate = new DataTemplate(typeof(WelcomeCell));
            this.moderatorDataTemplate = new DataTemplate(typeof(ModeratorView));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as Messages;
            if (messageVm == null)
                return null;
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("email") == null)
                    return this.outgoingDataTemplate;
                if (string.IsNullOrEmpty(Convert.ToString(messageVm.UserId)))
                    return this.welcomeDataTemplate;
                if (LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("jorum@dr-muscle.com") || LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("etiennejuneau@gmail.com"))
                    return messageVm.UserId.Equals("etiennejuneau@gmail.com") ? this.ModeratoroutgoingDataTemplate : this.incomingDataTemplate;
                

                return messageVm.UserId.Equals(LocalDBManager.Instance.GetDBSetting("email").Value) ? this.outgoingDataTemplate : messageVm.UserId.Equals("etiennejuneau@gmail.com") ? this.moderatorDataTemplate :  this.incomingDataTemplate;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}