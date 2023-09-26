using System;
using DrMuscle.iOS.Renderer;
using DrMuscle.Layout;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DrMuscleButton), typeof(DrMuscleButtonRender))]
namespace DrMuscle.iOS.Renderer
{
    public class DrMuscleButtonRender : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            { 
                //Control.LineBreakMode = UIKit.UILineBreakMode.TailTruncation;
                
                    Control.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
                    Control.TitleLabel.Lines = 0;
                    Control.TitleLabel.TextAlignment = UITextAlignment.Center;
                
            }
        }
    }
}
