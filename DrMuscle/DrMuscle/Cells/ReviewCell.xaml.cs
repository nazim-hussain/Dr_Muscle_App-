using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Screens.Eve;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReviewCell : ViewCell
    {
        public ReviewCell()
        {
            InitializeComponent();
            DBSetting dbToken = LocalDBManager.Instance.GetDBSetting("token");
            if (dbToken == null && dbToken?.Value == null)
            {
                LblTitle.IsVisible = false;
                LblReview.FontAttributes = FontAttributes.Italic;
            }
            else
            {
                LblTitle.IsVisible = true;
                LblReview.FontAttributes = FontAttributes.None;
            }
        }

        void TapMoreReviews_Tapped(System.Object sender, System.EventArgs e)
        {
            //Browser.OpenAsync("https://dr-muscle.com/reviews/", BrowserLaunchMode.SystemPreferred);
            DBSetting dbToken = LocalDBManager.Instance.GetDBSetting("token");
            if (dbToken != null && dbToken?.Value != null)
            {
                Device.OpenUri(new Uri("https://dr-muscle.com/reviews/"));
                //PagesFactory.PushAsync<MealInfoPage>();
            }
            
        }

    }
}
