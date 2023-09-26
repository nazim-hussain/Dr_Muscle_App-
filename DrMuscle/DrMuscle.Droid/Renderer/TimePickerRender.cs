using System;
using Android.Content;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.TimePicker), typeof(TimePickerRender))]
namespace DrMuscle.Droid.Renderer 
{
    public class TimePickerRender : TimePickerRenderer
    {

    public TimePickerRender(Context context) : base(context)
    {
    }
    protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
    {
        base.OnElementChanged(e);
        try
        {

            this.Element.TextColor = Color.White;
            this.Element.BackgroundColor = Color.Transparent;

            if (Control != null)
            {
                Control?.SetPadding(20, 4, 0, 8);
                Control.TextSize = 20;
                    Control.SetTextColor(Color.Black.ToAndroid());
                Control.SetBackgroundColor(Color.Transparent.ToAndroid());
            }

        }
        catch (Exception ex)
        {

        }
    }
}
}
