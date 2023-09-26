using System;
using Android.Content;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ListView), typeof(DrMuscleScrollView))]
namespace DrMuscle.Droid.Renderer
{
    public class DrMuscleScrollView : ListViewRenderer
    {
        public DrMuscleScrollView(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                try
                {
                    var listview = this.Control as Android.Widget.ListView;
                    listview.NestedScrollingEnabled = true;
                }
                catch (Exception ex)
                {

                }

            }
        }
    }
}
