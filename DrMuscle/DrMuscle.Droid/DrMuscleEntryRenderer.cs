using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DrMuscle.Droid;
using DrMuscle.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(DrMuscleEntry), typeof(DrMuscleEntryRenderer))]
namespace DrMuscle.Droid
{
    internal class DrMuscleEntryRenderer : EntryRenderer
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
            }
        }
    }
}