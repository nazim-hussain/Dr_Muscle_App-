using System;
using DrMuscle.Controls;
using DrMuscle.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AutoSizeLabel), typeof(AutoSizeLabelRenderer))]

namespace DrMuscle.iOS.Renderer
{
    public class AutoSizeLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            var label = Control as UILabel;
            if (label != null)
            {
                label.AdjustsFontSizeToFitWidth = true;
                label.BaselineAdjustment = UIBaselineAdjustment.AlignCenters;
                label.MinimumFontSize = 10;
                label.Lines = 3;
                label.LineBreakMode = UILineBreakMode.Clip;
            }
        }
    }
}
