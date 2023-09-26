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
    public partial class NextWorkoutLoadingCardCell : ViewCell
    {
        public NextWorkoutLoadingCardCell()
        {
            InitializeComponent();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            LblDescription.Text = "Next workout...";
            UpdateLoadingDescription();
        }

        private async void UpdateLoadingDescription()
        {
            await Task.Delay(3500);

            LblDescription.Text = "Next workout sets...";
            await Task.Delay(2500);
            LblDescription.Text = "Next workout reps...";
            await Task.Delay(2500);
            LblDescription.Text = "Next workout rests...";
            await Task.Delay(2000);
            LblDescription.Text = "Next workout weights...";
            await Task.Delay(1500);
            LblDescription.Text = "Next workout exercises...";
            
        }
             
    }
}
