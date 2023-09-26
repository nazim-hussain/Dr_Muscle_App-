using Acr.UserDialogs;
using DrMuscle.Screens.Exercises;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;
using SlideOverKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using DrMuscle.Screens.History;
using DrMuscle.Screens.Subscription;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Resx;

namespace DrMuscle.Screens.Workouts
{
    public partial class ChooseYourHomeWorkoutPage : DrMusclePage
    {
        private List<WorkoutTemplateModel> workouts;
        private List<WorkoutTemplateGroupModel> workoutGroups;
        public ObservableCollection<WorkoutTemplateModel> workoutItems = new ObservableCollection<WorkoutTemplateModel>();
        public ObservableCollection<WorkoutTemplateModel> workoutItems2 = new ObservableCollection<WorkoutTemplateModel>();

        public ObservableRangeCollection<ProgramGroupSection> ExeList { get; set; }
          = new ObservableRangeCollection<ProgramGroupSection>();

        GetUserWorkoutTemplateGroupResponseModel programWorkout;

        public ChooseYourHomeWorkoutPage()
        {
            InitializeComponent();

            //WorkoutListView.ItemsSource = workoutItems;
            //ProgramListView.ItemsSource = workoutItems2;

            //WorkoutListView.ItemTapped += WorkoutListView_ItemTapped;
            //ProgramListView.ItemTapped += WorkoutListView_ItemTapped;

            if (LocalDBManager.Instance.GetDBSetting("WorkoutTypeList") == null)
                LocalDBManager.Instance.SetDBSetting("WorkoutTypeList", "0");

            if (LocalDBManager.Instance.GetDBSetting("WorkoutTypeListDayPerWeek") == null)
                LocalDBManager.Instance.SetDBSetting("WorkoutTypeListDayPerWeek", "0");

            if (LocalDBManager.Instance.GetDBSetting("WorkoutOrderList") == null)
                LocalDBManager.Instance.SetDBSetting("WorkoutOrderList", "0");

        }


    }
}
