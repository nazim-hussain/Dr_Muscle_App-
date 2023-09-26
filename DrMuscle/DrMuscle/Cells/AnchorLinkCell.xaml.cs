using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnchorLinkCell : ViewCell
    {
        public AnchorLinkCell()
        {
            InitializeComponent();
            FrmContainer.Opacity = 0;
            FrmContainer.Opacity = 0;

        }
        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            await FrmContainer.FadeTo(1, 500, Easing.CubicInOut);
            await LblAnswer.FadeTo(1, 500);

        }
    }
}
