using System;
using System.ComponentModel;
using DrMuscle.Controls;
using DrMuscle.Controls.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContextMenuButton), typeof(ContextMenuButtonRenderer))]
namespace DrMuscle.Controls.Renderers
{
    public class ContextMenuButtonRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (Element is ContextMenuButton contextButton)
            {
                contextButton.GetCoordinates = GetCoordinatesNative;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (Element is ContextMenuButton contextButton)
            {
                contextButton.GetCoordinates = GetCoordinatesNative;
            }   
        }

            private (int x, int y) GetCoordinatesNative()
        {

            //let frame = yourButton.superview.convertRect(yourButton.frame, toView: self.view)
            var frme = Control.Superview.ConvertRectToView(Frame, null);
            return ((int)Control.Superview.Superview.Frame.Left+82, (int)frme.Top-20);

        }
    }
}
