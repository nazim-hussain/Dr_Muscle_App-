using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Acr.UserDialogs;

namespace DrMuscle.Screens.History
{
    public partial class HistoryExercisePage : ContentPage
    {
        List<HistoryModel> history = new List<HistoryModel>();
        public HistoryExercisePage()
        {
            InitializeComponent();

            Title = string.Format("{0} History", CurrentLog.Instance.ExerciseLog.Exercice.Label);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            ExerciseHistoryStackLayout.Children.Clear();
            try
            {
                
            }
            catch (Exception e)
            {
                await UserDialogs.Instance.AlertAsync("Please check your Internet connection and try again. If this problem persists, please contact support.", "Error !");
            }
        }
    }
}
