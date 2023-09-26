using System;
using System.ComponentModel;
using DrMuscle.Controls;
using DrMuscle.iOS.Renderer;
using DrMuscle.Layout;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportRenderer(typeof(WorkoutEntry), typeof(WorkoutEntryRenderer))]
namespace DrMuscle.iOS.Renderer
{
    public class WorkoutEntryRenderer : EntryRenderer
    {
        public WorkoutEntryRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            
        }
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            try
            {
                if (Control != null)
                {
                    //Control.Layer.BorderWidth = 0;
                    // Control.Layer.BorderColor = Color.FromHex("#FFf1f1f1").ToUIColor().CGColor;
                    Control.Layer.BorderWidth = 0;
                    Control.BorderStyle = UITextBorderStyle.None;
           
                }
            }
            catch (Exception ex)
            {

            }

        }

    }
}
