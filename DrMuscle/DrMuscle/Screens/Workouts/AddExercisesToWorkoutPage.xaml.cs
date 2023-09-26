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
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using DrMuscle.Constants;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Newtonsoft.Json;
using DrMuscle.Screens.Exercises;
using System.Reflection;
using DrMuscle.Effects;
using ViewCell = Xamarin.Forms.ViewCell;

namespace DrMuscle.Screens.Workouts
{
    public partial class AddExercisesToWorkoutPage : DrMusclePage
    {
        //public ObservableCollection<SelectableExerciceModel> exerciseItems = new ObservableCollection<SelectableExerciceModel>();
        //public ObservableCollection<SelectableExerciceModel> exerciseItemsResult = new ObservableCollection<SelectableExerciceModel>();

        
        public AddExercisesToWorkoutPage()
        {
            InitializeComponent();

            
        }

        

    }

    public class SelectableExerciceModel : ExerciceModel, INotifyPropertyChanged
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
        private bool _isSelected;
        public bool IsSelected
        {
            get
            { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        
    }
}
