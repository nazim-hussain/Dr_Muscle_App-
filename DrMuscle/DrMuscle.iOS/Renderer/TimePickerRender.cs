using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace DrMuscle.iOS.Renderer
{
    public class TimePickerRender : TimePickerRenderer
    {
        public TimePickerRender()
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                Control.BackgroundColor = Color.Transparent.ToUIColor();
                Control.TextColor = Color.White.ToUIColor();
                

            }
        }
    }
}
