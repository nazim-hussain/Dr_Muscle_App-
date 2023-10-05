using DrMuscle.iOS;
using DrMuscle.Layout;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DrMuscleEntry), typeof(DrMuscleEntryRenderer))]
namespace DrMuscle.iOS
{
    public class DrMuscleEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                // Remove border
                Control.BorderStyle = UIKit.UITextBorderStyle.None;
            }
        }
    }
}