using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class FeedbackView : PopupPage
    {
        public FeedbackView()
        {
            InitializeComponent();
        }

        
        async void SolidButton_Clicked(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var page = new FullReview();
            await PopupNavigation.Instance.PushAsync(page);
        }

        async void Feedback_Clicked(System.Object sender, System.EventArgs e)
        {

            PopupNavigation.Instance.PopAsync();
            await Task.Delay(500);
            Device.OpenUri(new Uri("mailto:support@drmuscleapp.com?subject=Feedback%20about%20Dr.%20Muscle"));
        }
    }
}
