using System;
using Android.Content;
using Android.Content.Res;
using Android.Text.Method;
using Android.Util;
using Android.Widget;
using DrMuscle.Controls;
using DrMuscle.Droid.Renderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedLabel), typeof(ExtendedLabelRenderer))]
namespace DrMuscle.Droid.Renderer
{
    public class ExtendedLabelRenderer : LabelRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
        {
            base.OnElementChanged(e);

            var view = (ExtendedLabel)Element;
            if (view == null) return;
            
            //TextView textView = new TextView(Forms.Context);
            //textView.LayoutParameters = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
            //textView.SetTextColor(view.TextColor.ToAndroid());

            //// Setting the auto link mask to capture all types of link-able data

            Control.AutoLinkMask = Android.Text.Util.MatchOptions.All;
            Control.Clickable = true;

            Control.LongClick += (sender, eee) =>
            {
                ClipboardManager clipboard = (ClipboardManager)MainActivity._currentActivity.GetSystemService(Context.ClipboardService);
                ClipData clip = ClipData.NewPlainText("label", Control.Text);
                clipboard.PrimaryClip = clip;
                Toast.MakeText(MainActivity._currentActivity, "Text copied to clipboard", ToastLength.Short).Show();
            };
            try
            {
                Control.MovementMethod = LinkMovementMethod.Instance;
            }
            catch (Exception ex)
            {

            }
            int[][] states = new int[][]
          {
            new int[] { Android.Resource.Attribute.StateEnabled }, // enabled
            new int[] {-Android.Resource.Attribute.StateEnabled }, // disabled
            new int[] { Android.Resource.Attribute.StateFocused }, // enabled
            new int[] { Android.Resource.Attribute.StatePressed }, // enabled
            new int[] { Android.Resource.Attribute.StateSelected }  // pressed
          };


            //int[] colors = new int[]
            //{
            //    Color.FromHex("#0000EE").ToAndroid(),
            //    Color.FromHex("#0000EE").ToAndroid(),
            //    Color.FromHex("#551A8B").ToAndroid(),
            //    Color.FromHex("#551A8B").ToAndroid(),
            //    Color.FromHex("#551A8B").ToAndroid()
            //};

           // ColorStateList myList = new ColorStateList(states, colors);

            //Control.SetLinkTextColor(myList);
            //Color.FromHex("#c0d8f2")
            Control.SetLinkTextColor(Constants.AppThemeConstants.BlueLightColor.ToAndroid());
            //// Make sure to set text after setting the mask
            //textView.Text = view.Text;
            //textView.SetTextSize(ComplexUnitType.Dip, (float)view.FontSize);

            //// overriding Xamarin Forms Label and replace with our native control
            //SetNativeControl(textView);
        }
    
        public ExtendedLabelRenderer(Context context) : base(context)
        {

        }
    }
}
