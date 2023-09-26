using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using DrMuscle.Controls;
using DrMuscle.Renderer.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System;
[assembly: ExportRenderer(typeof(DropDownPicker), typeof(DropDownPickerRenderer))]
namespace DrMuscle.Renderer.Droid
{
	public class DropDownPickerRenderer : PickerRenderer
	{
		DropDownPicker element;

        public DropDownPickerRenderer(Context context) : base(context)
        {

        }
		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			base.OnElementChanged(e);

			element = (DropDownPicker)this.Element;

			try
			{

				this.Element.TextColor = Xamarin.Forms.Color.White;
				this.Element.BackgroundColor = Constants.AppThemeConstants.BlueColor;

				if (Control != null)
				{
					Control?.SetPadding(20, 4, 4, 8);
					Control.TextSize = 15;
					Control.SetBackgroundColor(Constants.AppThemeConstants.BlueColor.ToAndroid());
					
				}

			}
			catch (Exception ex)
			{

			}
			if (Control != null && this.Element != null && !string.IsNullOrEmpty(element.Image))
			{
                Control.Background = AddPickerStyles(element.Image);
			    Control.SetPadding(20, 10, 80, 10);
				
			}
		}

		public LayerDrawable AddPickerStyles(string imagePath)
		{
			ShapeDrawable border = new ShapeDrawable();
            border.Paint.Color = Android.Graphics.Color.Gray;
            border.SetPadding(10,10,10,10);
            border.Paint.SetStyle(Paint.Style.Stroke);
            
			Drawable[] layers = { border , GetDrawable(imagePath) };
			LayerDrawable layerDrawable = new LayerDrawable(layers);
            layerDrawable.SetLayerInset(0, 0, 0, 0, 0);
			

			return layerDrawable;
		}

		private BitmapDrawable GetDrawable(string imagePath)
		{
			imagePath = imagePath.Replace(".png", "");
			int resID = Resources.GetIdentifier(imagePath, "drawable", this.Context.PackageName);
			var drawable = ContextCompat.GetDrawable(this.Context, resID);
			var bitmap = ((BitmapDrawable)drawable).Bitmap;

			var result = new BitmapDrawable(Resources, Bitmap.CreateScaledBitmap(bitmap, 70, 37, true));
			result.Gravity = Android.Views.GravityFlags.Right;

			return result;
		}

	}
}

