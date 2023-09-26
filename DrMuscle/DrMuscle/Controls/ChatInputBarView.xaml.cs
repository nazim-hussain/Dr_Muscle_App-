using System;
using System.Collections.Generic;
using DrMuscle.Screens.User;
using Plugin.Connectivity;
using Xamarin.Forms;
using DrMuscle.Resx;

namespace DrMuscle.Controls
{
    public partial class ChatInputBarView : ContentView
    {
        public event EventHandler Tapped;

        public ChatInputBarView()
        {
            InitializeComponent();
            chatTextInput.BindingContext = this;
            
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) => {
                RefreshLocalized();
            });
        }
        void RefreshLocalized()
        {
            BtnSend.Text = AppResources.Send;
        }
        //public void Handle_Completed(object sender, EventArgs e)
        //{
            //if (Tapped != null)
            //{
            //    Tapped.Invoke(this, e);
            //}
            //chatTextInput.Focus();
        //}

        public void UnFocusEntry()
        {
            chatTextInput?.Unfocus();
        }

        void Handle_Completed(object sender, System.EventArgs e)
        {
            if (Tapped != null)
            {
                Tapped.Invoke(this, e);
            }
            chatTextInput.Text = "";
            //chatTextInput.Focus();
        }

        public static readonly BindableProperty MessageTextProperty = BindableProperty.Create("MessageText", typeof(string), typeof(ChatInputBarView), string.Empty, BindingMode.TwoWay, null, (bindable, oldValue, newValue) =>
        {
            ((ChatInputBarView)bindable).SetMessageText();
        });

        private void SetMessageText()
        {
            this.MessageText = chatTextInput.Text;
            BtnSend.IsEnabled = MessageText.Length > 0 && CrossConnectivity.Current.IsConnected;
        }

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }

            set
            {
                SetValue(MessageTextProperty, value);
            }
        }

        void chatTextInput_TextChanged(System.Object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(chatTextInput.Text))
                    { 
                    if (chatTextInput.Text.Length == 1)
                    chatTextInput.Text = char.ToUpper(chatTextInput.Text[0]) + "";
            else if (chatTextInput.Text.Length > 1)
                    chatTextInput.Text = char.ToUpper(chatTextInput.Text[0]) + chatTextInput.Text.Substring(1);
            }
        }
    }
}
