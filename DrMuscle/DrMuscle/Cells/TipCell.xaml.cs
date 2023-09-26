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
    public partial class TipCell : ViewCell
    {
        public TipCell()
        {
            InitializeComponent();
        }

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            
        }

        void LblStrengthUpText_BindingContextChanged(System.Object sender, System.EventArgs e)
        {
            ForceUpdateSize();
        }
    }
}
