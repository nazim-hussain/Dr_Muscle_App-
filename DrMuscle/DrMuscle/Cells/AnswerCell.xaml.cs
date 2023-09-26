using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public partial class AnswerCell : ViewCell
    {
        public AnswerCell()
        {
            InitializeComponent();
            FrmContainer.Opacity = 0;
            FrmContainer.Opacity = 0;
        }

        protected override async void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            await FrmContainer.FadeTo(1, 500,Easing.CubicInOut);
            await LblAnswer.FadeTo(1, 500);
        }
    }
}
