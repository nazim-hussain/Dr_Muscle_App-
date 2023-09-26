using System;
using Xamarin.Forms;
namespace DrMuscle.Controls
{
    public class DropDownPicker : Picker
    {
        public DropDownPicker()
        {
        }
        public static readonly BindableProperty ImageProperty =
            BindableProperty.Create(nameof(Image), typeof(string), typeof(DropDownPicker), string.Empty);

        public string Image
        {
            get { return (string)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
    }
}
