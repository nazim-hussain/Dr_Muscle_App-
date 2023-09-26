using System;
using System.Collections.Generic;
using DrMuscle.Helpers;
using DrMuscle.Screens.User;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
namespace DrMuscle.Cells
{
    public partial class HeaderCell : ViewCell
    {
        public HeaderCell()
        {
            InitializeComponent();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            try
            {

                var message = (GroupChannelType)this.BindingContext;
                if (message == null)
                    return;

                //if (message.Type == ChannelType.Open)
                    //ViewAlldButton.IsVisible = true;
                //else
                    ViewAlldButton.IsVisible = false;
            }
            catch(Exception ex)
            {

            }
        }

        async void ViewAll_Clicked(object sender, System.EventArgs e)
        {
           await PagesFactory.PushAsync<ChatPage>();
        }
    }
}
