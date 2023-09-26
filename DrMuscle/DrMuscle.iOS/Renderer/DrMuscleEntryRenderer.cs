using System;
using System.ComponentModel;
using DrMuscle.iOS.Renderer;
using DrMuscle.Layout;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DrEntry), typeof(DrMuscleEntryRenderer))]
namespace DrMuscle.iOS.Renderer
{
    public class DrMuscleEntryRenderer : EntryRenderer
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            try
            {
                if (Control != null)
                {
                    Control.Layer.BorderWidth = 3;
                    Control.Layer.BorderColor = Color.FromHex("#f1f1f1").ToUIColor().CGColor;
                    //Control.BorderStyle = UITextBorderStyle.None;
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}
