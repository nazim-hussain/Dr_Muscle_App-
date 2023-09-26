using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace DrMuscle.iOS.Renderer
{
    public class DrMuscleEditorRenderer : EditorRenderer
    {
        public DrMuscleEditorRenderer()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.ScrollEnabled = false;
            }
        }
    }
}
