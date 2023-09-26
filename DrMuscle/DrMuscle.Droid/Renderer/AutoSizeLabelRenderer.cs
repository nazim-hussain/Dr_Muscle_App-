using System;
using System.ComponentModel;
using Android.Content;
using Android.OS;
using Android.Widget;
using DrMuscle.Controls;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(AutoSizeLabel), typeof(AutoSizeLabelRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class AutoSizeLabelRenderer : LabelRenderer
    {
        public AutoSizeLabelRenderer(Context context) : base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);
            var label = Control as TextView;
            if (label != null)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    label.SetAutoSizeTextTypeUniformWithConfiguration(15, 16, 1, (int)Android.Util.ComplexUnitType.Sp);
                }
            }
        }
        //protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    base.OnElementPropertyChanged(sender, e);
        //    var label = Control as TextView;
        //    //if (label != null)
        //    //{
        //    //    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //    //    {


        //    //        label.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
        //    //        label.SetAutoSizeTextTypeUniformWithConfiguration(8, 14, 1, (int)Android.Util.ComplexUnitType.Sp);
        //    //        label.SetMaxLines(3);
                    
        //    //    }
        //    //}
        //}
    }
}
