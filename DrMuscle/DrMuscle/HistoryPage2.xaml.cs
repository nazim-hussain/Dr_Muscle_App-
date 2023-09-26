using Acr.UserDialogs; 
using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace DrMuscle
{
    public partial class HistoryPage2 : ContentPage
    {
        public ObservableCollection<HistoryModelEx> history = new ObservableCollection<HistoryModelEx>();
        public HistoryPage2()
        {
            InitializeComponent();

            
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            
           
        }
    }

    public class HistoryModelEx : HistoryModel
    {
        public string WorkoutDateString
        {
            get
            {
                return WorkoutDate.ToLocalTime().ToString("dddd, MMMM dd | hh:mm tt");
            }
        }
    }
}
