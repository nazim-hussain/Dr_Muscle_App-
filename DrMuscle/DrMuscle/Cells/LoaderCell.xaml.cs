using System;
using System.Collections.Generic;
using FFImageLoading;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public partial class LoaderCell : ViewCell
    {
        public LoaderCell()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            try
            {
                if (Device.RuntimePlatform.Equals(Device.iOS))
                {
                    ImageService.Instance.InvalidateMemoryCache();
                    ImgLoader.Source = "resource://DrMuscle.Image.typing_loader.gif";
                }
            }
            catch (Exception ex)
            {

            }



        }
    }
}
