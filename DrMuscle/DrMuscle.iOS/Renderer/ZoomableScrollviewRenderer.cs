using DrMuscle.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Linq;

[assembly: ExportRenderer(typeof(DrMuscle.Controls.ZoomableScrollview), typeof(ZoomableScrollviewRenderer))]
namespace DrMuscle.iOS.Renderers
{
    public class ZoomableScrollviewRenderer : ScrollViewRenderer
    {
        // bool zoomEnabled = false;
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            MaximumZoomScale = 3f;
            MinimumZoomScale = 1.0f;

        }
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (Subviews.Length > 0)
            {
                ViewForZoomingInScrollView += GetViewForZooming;
            }
            else
            {
                ViewForZoomingInScrollView -= GetViewForZooming;
            }

        }
        public UIView GetViewForZooming(UIScrollView sv)
        {
            return this.Subviews.FirstOrDefault();
        }

    }
}