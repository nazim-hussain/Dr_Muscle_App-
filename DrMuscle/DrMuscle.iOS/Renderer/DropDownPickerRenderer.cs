using DrMuscle.Controls;
using DrMuscle.Renderer.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(DropDownPicker), typeof(DropDownPickerRenderer))]
namespace DrMuscle.Renderer.iOS
{
	public class DropDownPickerRenderer : PickerRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			base.OnElementChanged(e);

			var element = (DropDownPicker)this.Element;

            if (this.Control != null && this.Element != null && !string.IsNullOrEmpty(element.Image)){
                var downarrow = UIImage.FromBundle(element.Image);
				Control.RightViewMode = UITextFieldViewMode.Always;
				Control.RightView = new UIImageView(downarrow);
            }
		}
	}
}

