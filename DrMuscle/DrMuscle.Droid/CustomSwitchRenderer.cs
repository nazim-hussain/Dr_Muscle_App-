using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using DrMuscle.Views;
using Xamarin.Facebook.Login.Widget;
using Xamarin.Facebook;
using Xamarin.Forms;
using System.Threading.Tasks;
using Xamarin.Facebook.Login;
using DrMuscle.Entity;
using DrMuscle.Droid;
using Android.Graphics;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Switch), typeof(CustomSwitchRendererDroid))]
namespace DrMuscle.Droid
{
	public class CustomSwitchRendererDroid : SwitchRenderer
	{
        public CustomSwitchRendererDroid(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Switch> e)
        {
			base.OnElementChanged(e);

			if (Control.Checked)
				Control.ThumbDrawable.SetColorFilter(new Android.Graphics.Color(97, 232, 69), PorterDuff.Mode.SrcAtop);
			else
				Control.ThumbDrawable.SetColorFilter(new Android.Graphics.Color(245, 245, 245), PorterDuff.Mode.SrcAtop);

			Control.CheckedChange += (sender, e2) =>
			{
                
				if (Control.Checked)
					Control.ThumbDrawable.SetColorFilter(new Android.Graphics.Color(97, 232, 69), PorterDuff.Mode.SrcAtop);
				else
					Control.ThumbDrawable.SetColorFilter(new Android.Graphics.Color(245, 245, 245), PorterDuff.Mode.SrcAtop);
                if (e2.IsChecked == Element.IsToggled)
                    return;
                this.Element.IsToggled = this.Control.Checked;
			};

            this.Element.Toggled += OnElementToggled;
        }
        private void OnElementToggled(object sender, EventArgs e)
        {
            try
            {
                Xamarin.Forms.Switch s = (Xamarin.Forms.Switch)sender;
                if (s.IsToggled == Element.IsToggled)
                    return;
                this.Element.IsToggled = this.Control.Checked;
            }
            catch (Exception)
            {
                // Jason of SO suggests: use logging (ie, appcenter.ms) to log this exception
                // UserDialogs.Instance.AlertAsync("Please restart Dr. Muscle at your earliest convenience. If this problem persists, please contact support.", "Android memory error");
            }
        }
    }
}