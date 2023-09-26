using System;
using DrMuscle.iOS.Renderer;
using DrMuscle.Layout;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
[assembly: ExportRenderer(typeof(DrMuscleListView), typeof(DrMuscleListViewRenderer))]
namespace DrMuscle.iOS.Renderer
{
    public class DrMuscleListViewRenderer : ListViewRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control != null)
                {
                    if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
                    {
                        Control.SectionHeaderTopPadding = 0;
                    }
                }
            }
        }
    }
}
