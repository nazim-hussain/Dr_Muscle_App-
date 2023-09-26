using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewRecordCell : ViewCell
    {
        public NewRecordCell()
        {
            InitializeComponent();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            try
            {
            var botModel = (BotModel)this.BindingContext;
            if (string.IsNullOrEmpty(botModel?.Question))
                LblAnswer.IsVisible = false;

            }
            catch (Exception ex)
            {

            }
        }
    }
}
