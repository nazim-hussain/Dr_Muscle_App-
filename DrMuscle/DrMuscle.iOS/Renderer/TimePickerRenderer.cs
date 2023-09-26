using System;
using DrMuscle.iOS.Renderer;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
[assembly: ExportRenderer(typeof(TimePicker), typeof(CustomTimePickerRenderer))]

namespace DrMuscle.iOS.Renderer
{
    public class CustomTimePickerRenderer : TimePickerRenderer
    {
        public CustomTimePickerRenderer()
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(14, 0))
                { 
                    UITextField entry = Control;
                    UIDatePicker picker = (UIDatePicker)entry.InputView;
                    picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
                }
                
            }
        }
    }
}
