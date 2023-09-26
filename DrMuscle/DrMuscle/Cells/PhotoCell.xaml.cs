using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public partial class PhotoCell : ViewCell
    {
        public PhotoCell()
        {
            InitializeComponent();
            FrmContainer.Opacity = 0;
            ImgPhoto.Opacity = 0;
        }

        protected async override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            await FrmContainer.FadeTo(1, 500, Easing.CubicInOut);
            await ImgPhoto.FadeTo(1, 500);
        }
    }
}
