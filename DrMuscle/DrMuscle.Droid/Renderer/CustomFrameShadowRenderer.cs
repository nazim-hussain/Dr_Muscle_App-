using System;
using Android.Content;
using Android.Graphics;
using DrMuscle.Controls;
using DrMuscle.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomFrame), typeof(CustomFrameShadowRenderer))]
namespace DrMuscle.Droid
{

    public class CustomFrameShadowRenderer : Xamarin.Forms.Platform.Android.FastRenderers.FrameRenderer
    {
        public CustomFrameShadowRenderer(Context context) : base(context)
        {
        }

        //protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        //{
        //    base.OnElementChanged(e);
        //    var element = e.NewElement as CustomFrame;


        //    if (element == null) return;
        //    if (e.NewElement != null)
        //    {
        //        ViewGroup.SetBackgroundResource(Resource.Drawable.Shadow);
        //    }
        //}


        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);
            try
            {
                CardElevation = 10;
                SetOutlineSpotShadowColor(Xamarin.Forms.Color.Gray.ToAndroid());
            }
            catch (Exception ex)
            {

            }

        }

    }
}
