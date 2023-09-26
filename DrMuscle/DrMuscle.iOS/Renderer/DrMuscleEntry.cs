using System;
using System.ComponentModel;
using DrMuscle.Controls;
using DrMuscle.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(DrMuscleEntry), typeof(DrMuscleEntryRender))]
namespace DrMuscle.iOS.Renderer
{
    public class DrMuscleEntryRender : EntryRenderer
    {
        public DrMuscleEntryRender()
        {
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            try
            {
                if (Control != null)
                {
                    Control.Layer.BorderWidth = 0;
                   // Control.Layer.BorderColor = Color.FromHex("#FFf1f1f1").ToUIColor().CGColor;
                    Control.BorderStyle = UITextBorderStyle.None;
                }
            }
            catch (Exception ex)
            {

            }

        }
        
    }
}
