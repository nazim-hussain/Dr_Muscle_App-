using System;
using System.ComponentModel;
using DrMuscle.Controls;
using DrMuscle.iOS.Renderer;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(AutoBotListView), typeof(AutoBotListViewRenderer))]

namespace DrMuscle.iOS.Renderer
{
    public class AutoBotListViewRenderer : ListViewRenderer
    {
        public AutoBotListViewRenderer()
        {
        }

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
