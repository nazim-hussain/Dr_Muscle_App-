using Android.Content;
using DrMuscle.Droid;
using DrMuscle.Layout;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(DrEntry), typeof(ExtendedEntry))]
namespace DrMuscle.Droid
{
    class ExtendedEntry : EntryRenderer
    {
        public ExtendedEntry(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                Control.SetTextColor(Android.Graphics.Color.Black);
                Control.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            }
        }
    }
}