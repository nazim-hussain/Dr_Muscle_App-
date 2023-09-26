using Acr.UserDialogs;
using DrMuscle.Dependencies;
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
using System.Globalization;
using DrMuscle.Helpers;
using DrMuscle.Resx;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DrMuscle.Message;
using DrMuscle.Screens.Me;

namespace DrMuscle.Screens.Workouts
{
    public partial class ChooseYourWorkoutExercisePage : DrMusclePage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetObservableProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var changed = PropertyChanged;
            if (changed != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<ExerciceModel> _exerciseItems;
        public ObservableCollection<ExerciceModel> exerciseItems
        {
            get { return _exerciseItems; }
            set
            {
                _exerciseItems = value;
                OnPropertyChanged("exerciseItems");
            }
        }

         
        private GetUserProgramInfoResponseModel upi = null;

        public ChooseYourWorkoutExercisePage()
        {
            InitializeComponent();
            exerciseItems = new ObservableCollection<ExerciceModel>();
            
        }

    }
}
