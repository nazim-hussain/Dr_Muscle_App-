using System;
using Android.Content;
using DrMuscle.Controls;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(DrMuscleEntry), typeof(DrMuscleEntryRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class DrMuscleEntryRenderer : EntryRenderer
    {
        public DrMuscleEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
                Control.SetPadding(0, 2, 0, 2);
            }
        }
    }
}
