using Acr.UserDialogs;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrMuscle.Resx;
using Xamarin.Forms;

namespace DrMuscle.Screens.Workouts
{
    public partial class AddWorkoutToWorkoutOrderPage : DrMusclePage
    {
        public ObservableCollection<SelectableWorkoutTemplateModel> workoutItems = new ObservableCollection<SelectableWorkoutTemplateModel>();

        public AddWorkoutToWorkoutOrderPage()
        {
            InitializeComponent();
        }

    }

    public class SelectableWorkoutTemplateModel : WorkoutTemplateModel
    {
        public bool IsSelected { get; set; }
    }
}
