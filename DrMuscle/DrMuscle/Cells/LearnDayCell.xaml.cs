using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Screens.User;
using DrMuscle.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LearnDayCell : ViewCell
    {
        public LearnDayCell()
        {
            InitializeComponent();
            FrmContainer.Opacity = 0;
            FrmContainer.Opacity = 0;

        }
        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            await FrmContainer.FadeTo(1, 500, Easing.CubicInOut);
            await LblAnswer.FadeTo(1, 500);

        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            PopupNavigation.Instance.PushAsync(new ReminderPopup());
            //PagesFactory.PushAsync<FAQPage>();
        }
    }
}
