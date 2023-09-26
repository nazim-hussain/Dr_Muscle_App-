using System;
using Android.Content;
using DrMuscle.Droid.Renderer;
using DrMuscle.Layout;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(DrMuscleEditor), typeof(DrMuscleEditorRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class DrMuscleEditorRenderer : EditorRenderer
    {

        public DrMuscleEditorRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            }
        }
    }
}
