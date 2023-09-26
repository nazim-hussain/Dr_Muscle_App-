using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Widget;
using DrMuscle.Controls;
using DrMuscle.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ExtendedEditorControl), typeof(CustomEditorRenderer))]
namespace DrMuscle.Droid.Renderers
{
    public class CustomEditorRenderer : EditorRenderer
    {
        bool initial = true;
        Drawable originalBackground;
        Context _context;
        public CustomEditorRenderer(Context context) : base(context)
        {
            _context = context;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Editor> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                if (initial)
                {
                    originalBackground = Control.Background;
                    initial = false;
                }
                Control.SetTextColor(Android.Graphics.Color.Black);
                //Control.SetScrollContainer(true);
                //Control.HorizontalScrollBarEnabled = false;
                Control.SetMaxLines(3);
                var customControl = (ExtendedEditorControl)Element;

                LinearLayout.LayoutParams linearLayoutParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.FillParent,
            LinearLayout.LayoutParams.WrapContent);
                Control.LayoutParameters = new LayoutParams(linearLayoutParams);
                customControl.IsTextPredictionEnabled = true;
                //Android.Widget.RelativeLayout.LayoutParams layout = new Android.Widget.RelativeLayout.LayoutParams(Android.Widget.RelativeLayout.LayoutParams.WrapContent, Android.Widget.RelativeLayout.LayoutParams.WrapContent);
                //Control.LayoutParameters = new LayoutParams(layout);
                //Control.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagMultiLine | Android.Text.InputTypes.TextFlagCapSentences;
            }

            if (e.NewElement != null)
            {
                var customControl = (ExtendedEditorControl)Element;
                //customControl.Keyboard = Keyboard.Create(KeyboardFlags.CapitalizeSentence);
                customControl.IsTextPredictionEnabled = true;
                customControl.IsSpellCheckEnabled = true;
                
                if (customControl.HasRoundedCorner)
                {
                    ApplyBorder();
                }

                //if (!string.IsNullOrEmpty(customControl.Placeholder))
                //{
                //    Control.Hint = customControl.Placeholder;
                    //Control.SetHintTextColor(customControl.PlaceholderColor.ToAndroid());
                //}
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            var customControl = (ExtendedEditorControl)Element;

            //if (ExtendedEditorControl.PlaceholderProperty.PropertyName == e.PropertyName)
            //{
            //    Control.Hint = customControl.Placeholder;
            //}
            //else if (ExtendedEditorControl.PlaceholderColorProperty.PropertyName == e.PropertyName)
            //{
            //    Control.SetHintTextColor(customControl.PlaceholderColor.ToAndroid());
            //}
            //else
            if (ExtendedEditorControl.HasRoundedCornerProperty.PropertyName == e.PropertyName)
            {
                if (customControl.HasRoundedCorner)
                {
                    ApplyBorder();
                }
                else
                {
                    this.Control.Background = originalBackground;
                }
            }
        }

        void ApplyBorder()
        {
            GradientDrawable gd = new GradientDrawable();
            gd.SetCornerRadius(10);
            gd.SetStroke(2, Color.Black.ToAndroid());
            this.Control.Background = gd;
        }
    }
}
