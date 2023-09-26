using System;
using Android.Content;
using DrMuscle.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Picker), typeof(CustomPickerRenderer))]
namespace DrMuscle.Droid
{
    public class CustomPickerRenderer : PickerRenderer
    {
        public CustomPickerRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);
            try
            {

            this.Element.TextColor = Color.White;
            this.Element.BackgroundColor = Constants.AppThemeConstants.BlueColor;
                
            if (Control != null)
            {
                Control?.SetPadding(20, 4, 0, 70);
                Control.TextSize = 15;
                Control.SetBackgroundColor(Constants.AppThemeConstants.BlueColor.ToAndroid());
            }

            }
            catch (Exception ex)
            {

            }
        }
    }
}
