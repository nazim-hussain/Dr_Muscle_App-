using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Screens.User;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LinkGestureCell : ViewCell
    {
        public LinkGestureCell()
        {
            InitializeComponent();
            FrmContainer.Opacity = 0;
            FrmContainer.Opacity = 0;
            //if (Device.RuntimePlatform.Equals(Device.iOS))
            //    LblQuestion.TextColor = Color.FromHex("#5063EE");
        }
        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            await FrmContainer.FadeTo(1, 500,Easing.CubicInOut);
            await LblQuestion.FadeTo(1, 500);

        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            PagesFactory.PushAsync<FAQPage>();
        }
    }
}
