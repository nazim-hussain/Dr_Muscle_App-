using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReviewTestimonialCell : ViewCell
    {
        public ReviewTestimonialCell()
        {
            InitializeComponent();
           
                LblTitle.IsVisible = false;
                LblReview.FontAttributes = FontAttributes.Italic;
            StackContainer.Opacity = 0;
            LblReview.Opacity = 0;
            LblReviewerName.Opacity = 0;
            ImgReivew.Opacity = 0;
        }
        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            await StackContainer.FadeTo(1, 400, Easing.CubicInOut);
            ImgReivew.FadeTo(1, 500);
             LblReview.FadeTo(1, 500);
            LblReviewerName.FadeTo(1, 500);
        }
        async void TapMoreReviews_Tapped(System.Object sender, System.EventArgs e)
        {
            //Browser.OpenAsync("https://dr-muscle.com/reviews/", BrowserLaunchMode.SystemPreferred);
            
        }

    }
}
