
using System;
using System.ComponentModel;
using Android.Content;
using DrMuscle.Controls;
using DrMuscle.Droid.Renderer;
using DrMuscle.Layout;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(WorkoutEntry), typeof(WorkoutEntryRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class WorkoutEntryRenderer : EntryRenderer
    {
        public WorkoutEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
                Control.SetPadding(0, 8, 0, 8);
                Control.InputType = Android.Text.InputTypes.ClassNumber | Android.Text.InputTypes.NumberFlagDecimal;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (Control != null)
            {
                Control.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            }

            
        }
        
       
        
    }
}