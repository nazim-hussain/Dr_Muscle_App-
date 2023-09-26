using DrMuscle.Layout;
using Xamarin.Forms;

using DrMuscle.Resx;
using System;
using DrMuscle.Message;
using DrMuscle.Dependencies;
using System.Collections.ObjectModel;
using DrMuscle.Helpers;
using DrMuscleWebApiSharedModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;

namespace DrMuscle.Screens.User
{
    public partial class ChatPage : DrMusclePage, IActiveAware
    {
        public event EventHandler IsActiveChanged;

        bool _isActive;
        public bool IsActive
        { 
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    IsActiveChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }



        public ObservableCollection<Messages> messageList = new ObservableCollection<Messages>();


        public ChatPage()
        {
            InitializeComponent();
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });
            //Live
            //SendBirdClient.Init("91658003-270F-446B-BD61-0043FAA8D641");

            //Test
            //SendBirdClient.Init("05F82C36-1159-4179-8C49-5910C7F51D7D");

        }
        void RefreshLocalized()
        {
            Title = AppResources.GroupChatBeta;
        }

        public override void OnBeforeShow()
        {
           
            var loadChatMessage = new LoadChatMessage();
            loadChatMessage.IsFromLogin = true;
            MessagingCenter.Send(loadChatMessage, "LoadChatMessage");
           
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (!App.IsSidemenuOpen)
            { 
            var loadChatMessage = new LoadChatMessage();
            loadChatMessage.IsFromLogin = false;
            MessagingCenter.Send(loadChatMessage, "LoadChatMessage");
            }
            try
            {
                
                var isAdmin = LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("etiennejuneau@gmail.com") || LocalDBManager.Instance.GetDBSetting("email").Value.ToLower().Equals("jorum@dr-muscle.com");
                if (isAdmin) { 
                    var timerToolbarItem = new ToolbarItem("1:1 support", "", Support_Tapped, ToolbarItemOrder.Primary, 0);
                    ToolbarItems.Clear();
                    ToolbarItems.Add(timerToolbarItem);
                } else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }
        public void MovetoSupport()
        {
         //   Support_Tapped(null, EventArgs.Empty);
        }

        async void Support_Tapped()
        {
            await PagesFactory.PushAsync<InboxPage>();  
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var unLoadChat = new UnLoadChatMessage();
            MessagingCenter.Send(unLoadChat, "UnLoadChatMessage");

        }


        protected override bool OnBackButtonPressed()
        {
            if (PopupNavigation.Instance.PopupStack.Count > 0)
            {
                PopupNavigation.Instance.PopAllAsync();
                return true;
            }
            Device.BeginInvokeOnMainThread(async () =>
            {
                ConfirmConfig exitPopUp = new ConfirmConfig()
                {
                    Title = AppResources.Exit,
                    Message = AppResources.AreYouSureYouWantToExit,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.Yes,
                    CancelText = AppResources.No,
                };

                var result = await UserDialogs.Instance.ConfirmAsync(exitPopUp);
                if (result)
                {
                    var kill = DependencyService.Get<IKillAppService>();
                    kill.ExitApp();
                }
            });
            return true;
        }
    }
}
