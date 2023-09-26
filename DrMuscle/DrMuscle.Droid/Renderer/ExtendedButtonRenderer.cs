using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DrMuscle.Droid;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Android.Graphics.Drawables;
using System.ComponentModel;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(DrMuscle.Controls.ExtendedButton), typeof(ExtendedButtonRenderer))]
namespace DrMuscle.Droid
{
    public class ExtendedButtonRenderer : Xamarin.Forms.Platform.Android.ButtonRenderer
    {
        private GradientDrawable _normal, _pressed;

        public ExtendedButtonRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                try
                {

                var button = e.NewElement;
                Control.TransformationMethod = null;
                
                // Create a drawable for the button's normal state
                _normal = new Android.Graphics.Drawables.GradientDrawable();
                if (button.BackgroundColor.R == -1.0 && button.BackgroundColor.G == -1.0 && button.BackgroundColor.B == -1.0)
                    _normal.SetColor(Android.Graphics.Color.ParseColor("#ff2c2e2f"));
                else
                    _normal.SetColor(button.BackgroundColor.ToAndroid());

                _normal.SetStroke((int)button.BorderWidth, button.BorderColor.ToAndroid());
                _normal.SetCornerRadius(button.BorderRadius);

                // Create a drawable for the button's pressed state
                _pressed = new Android.Graphics.Drawables.GradientDrawable();
                var highlight = Context.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.ColorActivatedHighlight }).GetColor(0, Android.Graphics.Color.Gray);
                _pressed.SetColor(highlight);
                _pressed.SetStroke((int)button.BorderWidth, button.BorderColor.ToAndroid());
                _pressed.SetCornerRadius(button.BorderRadius);
                    
                this.TextAlignment = Android.Views.TextAlignment.Center;
                // Add the drawables to a state list and assign the state list to the button
                //var sld = new StateListDrawable();
                //sld.AddState(new int[] { Android.Resource.Attribute.StatePressed }, _pressed);
                //sld.AddState(new int[] { }, _normal);
                    //Control.SetBackgroundDrawable(sld);

                    var shape = new PaintDrawable(button.BackgroundColor.ToAndroid());
                    shape.SetCornerRadius(button.CornerRadius * 4);

                    var ripple = new RippleDrawable(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.CadetBlue), shape, null);

                    //set the ripple as the native button's background 
                    Control.SetBackground(ripple);



                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Control.SetAutoSizeTextTypeUniformWithConfiguration(8, (int)button.FontSize, 1, (int)Android.Util.ComplexUnitType.Sp);
                }
                    Control.SetLines(1);
                    
                }
                catch (Exception ex)
                {

                }
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            var button = (Xamarin.Forms.Button)sender;

            if (_normal != null && _pressed != null)
            {
                if (e.PropertyName == "BorderRadius")
                {
                    _normal.SetCornerRadius(button.BorderRadius);
                    _pressed.SetCornerRadius(button.BorderRadius);
                }
                if (e.PropertyName == "BorderWidth" || e.PropertyName == "BorderColor")
                {
                    _normal.SetStroke((int)button.BorderWidth, button.BorderColor.ToAndroid());
                    _pressed.SetStroke((int)button.BorderWidth, button.BorderColor.ToAndroid());
                }
            }
        }
    }
}