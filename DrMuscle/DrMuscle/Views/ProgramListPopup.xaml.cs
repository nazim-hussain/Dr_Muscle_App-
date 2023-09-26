using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DrMuscle.Helpers;
using DrMuscle.Screens.Workouts;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class ProgramListPopup : PopupPage
    {
        public ObservableCollection<WorkoutTemplateGroupModel> workoutOrderItems = new ObservableCollection<WorkoutTemplateGroupModel>();

        public object _chooseYourCustomWorkout;
        public string ProgramName { get; set; }
        public ProgramListPopup()
        {
            InitializeComponent();
            ProgramListView.ItemsSource = workoutOrderItems;

            ProgramListView.ItemTapped += WorkoutListView_ItemTapped;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DependencyService.Get<IFirebase>().SetScreenName("program_list_popup");

        }
        public void setDataSource()
        {

            WorkoutTemplateGroupModel addWorkoutOrderItem = new WorkoutTemplateGroupModel();
            addWorkoutOrderItem.Id = -1;
            addWorkoutOrderItem.IsSystemExercise = false;
            addWorkoutOrderItem.Label = "None";
            if (workoutOrderItems.Where(x => x.Id == -1).FirstOrDefault() == null)
                workoutOrderItems.Insert(0, addWorkoutOrderItem);

            ProgramListView.ItemsSource = workoutOrderItems;
            ProgramListView.HeightRequest = workoutOrderItems.Count * 50;
        }

        private async void WorkoutListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;
            if (PopupNavigation.Instance.PopupStack.Count > 0)
                await PopupNavigation.Instance.PopAsync();

           
        }
    }
}
