
using System.Collections.ObjectModel;
using System.ComponentModel;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;

namespace DrMuscle.Helpers
{
    public class BodyPartSection : ObservableRangeCollection<ExerciceModel>, INotifyPropertyChanged
    {
        public BodyPartModel _bodyPart;
        public BodyPartSection()
        {
        }
        public ObservableRangeCollection<ExerciceModel> Exercises
            = new ObservableRangeCollection<ExerciceModel>();


        public BodyPartSection(BodyPartModel bodyPart, bool expanded = true)
        {
            this._bodyPart = bodyPart;
            this._expanded = expanded;
            // Continent has many countries. Once we get it, init CountryViewModel and store it in a backup variable

            foreach (ExerciceModel c in bodyPart.Exercices)
            {
                Exercises.Add(c);
            }
            // ContinentViewModel add a range with CountryViewModel
            if (expanded)
            {
                this.AddRange(Exercises);
            }
        }

        public string Name { get { return _bodyPart.Label; } }
        public long Id {
            get {
                return _bodyPart.Id; } }

        private bool _isTooltipVisible;

        public bool IsTooltipVisible
        {
            get { return _isTooltipVisible; }
            set
            {
                _isTooltipVisible = true;
                OnPropertyChanged(new PropertyChangedEventArgs("IsTooltipVisible"));
            }
        }
        private bool _expanded;
        
        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Expanded"));
                    OnPropertyChanged(new PropertyChangedEventArgs("StateIcon"));
                    if (_expanded)
                    {
                        try
                        {

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            this.AddRange(Exercises);
                        });

                        }
                        catch (System.Exception ex)
                        {

                        }
                    }
                    else
                    {
                        this.Clear();
                    }
                }
            }
        }

        public string StateIcon
        {
            get { return Expanded ? "up" : "down"; }
        }
    }

    public class ProgramGroupSection : ObservableRangeCollection<WorkoutTemplateModel>, INotifyPropertyChanged
    {
        private WorkoutTemplateGroupModel _bodyPart;
        public ProgramGroupSection()
        {
        }
        private ObservableRangeCollection<WorkoutTemplateModel> Workouts
            = new ObservableRangeCollection<WorkoutTemplateModel>();


        public ProgramGroupSection(WorkoutTemplateGroupModel bodyPart, bool expanded = true)
        {
            this._bodyPart = bodyPart;
            this._expanded = expanded;
            // Continent has many countries. Once we get it, init CountryViewModel and store it in a backup variable

            foreach (var c in bodyPart.WorkoutTemplates)
            {
                Workouts.Add(c);
            }
            // ContinentViewModel add a range with CountryViewModel
            if (expanded)
            {
                this.AddRange(Workouts);
            }
        }

        public string Name { get { return _bodyPart.Label; } }

        private bool _expanded;

        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Expanded"));
                    OnPropertyChanged(new PropertyChangedEventArgs("StateIcon"));
                    if (_expanded)
                    {
                        this.AddRange(Workouts);

                    }
                    else
                    {
                        this.Clear();
                    }
                }
            }
        }

        public string StateIcon
        {
            get { return Expanded ? "up" : "down"; }
        }
    }
}
