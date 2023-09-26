using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscleWebApiSharedModel;

namespace DrMuscle.Screens.Workouts
{
    public class KenkoChooseYourWorkoutViewModel : INotifyPropertyChanged
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

    private ObservableCollection<ExerciseWorkSetsModel> _exerciseItems;
    public ObservableCollection<ExerciseWorkSetsModel> exerciseItems
    {
        get { return _exerciseItems; }
        set
        {
            _exerciseItems = value;
            OnPropertyChanged("exerciseItems");
        }
    }

    public List<ObservableGroupCollection<ExerciseWorkSetsModel, WorkoutLogSerieModel>> GroupedData { get; set; }

    public KenkoChooseYourWorkoutViewModel()
        {

        }
        public void OnAppearing()
        {

        }
    }
}
