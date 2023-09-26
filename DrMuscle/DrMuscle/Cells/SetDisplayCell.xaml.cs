using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Screens.Exercises;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DrMuscle.Cells
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetDisplayCell : ViewCell
    {
        public SetDisplayCell()
        {
            InitializeComponent();
        }
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            WorkoutLogSerieModel workout = (DrMuscleWebApiSharedModel.WorkoutLogSerieModel)this.BindingContext;
            if (workout != null)
            {
                LblReps.Text = $"{workout.Reps}";
                LblWeight.Text = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                                                                    string.Format("{0:0.00}", SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Kg, 1, null, null)) :
                                                                    string.Format("{0:0.00}", SaveSetPage.RoundDownToNearestIncrement(workout.Weight.Lb, (decimal)2.5, null, null));
                LblMassUnit.Text = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs";
            }
            
        }
    }
}
