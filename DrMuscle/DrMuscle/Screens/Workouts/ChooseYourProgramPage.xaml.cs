using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Screens.Subscription;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;

namespace DrMuscle.Screens.Workouts
{
    public partial class ChooseYourProgramPage : DrMusclePage
    {
        private List<WorkoutTemplateGroupModel> workoutGroups;
        public ObservableCollection<WorkoutTemplateModel> workoutItems = new ObservableCollection<WorkoutTemplateModel>();
        public ObservableCollection<WorkoutTemplateGroupModel> workoutOrderItems = new ObservableCollection<WorkoutTemplateGroupModel>();

        public ChooseYourProgramPage()
        {
            InitializeComponent();
           
            Title = "Choose Featured Program";
        }
       
    }
}
