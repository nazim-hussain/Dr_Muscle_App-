using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DrMuscle.Message;
using DrMuscle.Screens.Exercises;
using DrMuscleWebApiSharedModel;
using Xamarin.Forms;
using static FFImageLoading.Helpers.Gif.GifFrame;

namespace DrMuscle.Layout
{

    public class WorkoutLogModel : BaseModel, INotifyPropertyChanged
    {
        public long Id { get; set; }
        public ExerciceModel Exercice { get; set; }
        public string UserId { get; set; }
        public DateTime LogDate { get; set; }
        public int Reps { get; set; }
        public MultiUnityWeight Weight { get; set; }

        public MultiUnityWeight OneRM { get; set; }
        public bool IsWarmups { get; set; }
        public bool isNext { get; set; }
        public bool IsFinished { get; set; }
        //Todo NbPause au niveau du WorkoutLogSerieModel possiblement inutile
        public int? NbPause { get; set; }
        public int? RIR { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExerciseWorkSetsModel : ObservableCollection<WorkoutLogSerieModel>, INotifyPropertyChanged
    {

        private bool _isFinished { get; set; }
        public long Id { get; set; }
        public string Label { get; set; }
        public bool IsSystemExercise { get; set; }
        public bool IsSwapTarget { get; set; }
        public bool IsPlate { get; set; }
        public bool IsPyramid {
            get;
            set;
        }
        public int ChangeCount { get; set; }
        public bool IsReversePyramid { get; set; }
        public bool IsFinished
        {
            get { return _isFinished; }
            set
            {
                _isFinished = value;
                OnPropertyChanged(nameof(IsFinished));
            }
        }
        public long? BodyPartId { get; set; }
        public long? EquipmentId { get; set; }

        public bool IsUnilateral { get; set; }
        public bool IsTimeBased { get; set; }

        public bool IsEasy { get; set; }
        public bool IsMedium { get; set; }
        public bool IsBodyweight { get; set; }
        public string VideoUrl { get; set; }
       // public string LocalVideo { get; set; }

        //FeaturedProgram
        public int? RepsMaxValue { get; set; }
        public int? RepsMinValue { get; set; }
        public int? Timer { get; set; }
        public bool IsNormalSets { get; set; }
        public long? WorkoutGroupId { get; set; }
        public bool IsFirstSide { get; set; }
        public bool IsPopup { get; set; }
        public bool IsFlexibility { get; set; }
        public bool IsWeighted { get; set; }
        public bool IsOneHanded { get; set; }
        //public int BodyPartId { get; set; }
        //public BodyPartModel BodyPart { get;set; }
        public bool IsAssisted { get; set; }
        
        private string _countNo;
        public string CountNo
        {
            get { return _countNo; }
            set
            {
                _countNo = value;
                OnPropertyChanged(nameof(CountNo));
            }
        }

        public RecommendationModel RecoModel { get; set; }
        public bool IsSelected { get; set; }

        private BodyPartModel _bodyPart;
        public ExerciseWorkSetsModel()
        {
        }

        private bool _isFinishWorkoutExe;
        public bool IsFinishWorkoutExe
        {
            get { return _isFinishWorkoutExe; }
            set
            {
                _isFinishWorkoutExe = value;
                OnPropertyChanged("IsFinishWorkoutExe");
            }
        }

        public bool IsFrameBackground
        {
            get
            {
                return IsFinishWorkoutExe || IsAddExercise;
            }
        }
        private bool _isAddExercise;
        public bool IsAddExercise
        {
            get { return _isAddExercise; }
            set
            {
                _isAddExercise = value;
                OnPropertyChanged("IsAddExercise");
            }
        }

        private bool _isNextExercise;
        public bool IsNextExercise
        {
            get { return _isNextExercise; }
            set
            {
                _isNextExercise = value;
                OnPropertyChanged("IsNextExercise");
                OnPropertyChanged("IsVideoUrlAvailable");
            }
        }
        private string _localVideo;
        public string LocalVideo
        {
            get { return _localVideo; }
            set
            {
                _localVideo = value;
                OnPropertyChanged("LocalVideo");
                OnPropertyChanged("IsVideoUrlAvailable");
            }
        }

        public bool IsVideoUrlAvailable
        {
            get
            {

                if (!string.IsNullOrEmpty(LocalVideo))
                    return true;
                return false;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WorkoutLogSerieModelRef : WorkoutLogSerieModel, INotifyPropertyChanged, IDisposable
    {
        private bool _isNext;
        private bool _isFinished;
        private string _setNo;
        private bool _isLastSet;
        private bool _isFirstSide;
        private bool _isHeaderCell;
        private bool _isTimerOff;
        private bool _isEditing;
        //To display timer on button
        private bool _isActive;
        private bool _isBodyweight;
        public event EventHandler OnSizeChanged;

        private string _headerImage;
        private string _headerTitle;
        private string _videoUrl;
        private string _setTitle;
        private string _lastTimeSet;
        public string ExerciseName { get; set; }
        public bool IsFlexibility { get; set; }
        public long? EquipmentId { get; set; }
        private bool _isExerciseFinished;
        private bool _isTimeBased;
        private bool _isUnilateral;
        private bool _isSizeChanged;
        private bool _isMaxChallenge;
        private bool disposed = false;

        public WorkoutLogSerieModelRef()
        {

            //WeightChangedCommand = new Command<TextChangedEventArgs>((newText) =>
            //{
            //    System.Diagnostics.Debug.WriteLine($"TextChanged : {newText}");
            //    string entryText = newText.NewTextValue.Replace(",", ".");
            //    entryText = entryText.Replace(" ", "");
            //    if (!string.IsNullOrEmpty(entryText))
            //    {
            //        var currentWeight = Convert.ToDecimal(entryText, CultureInfo.InvariantCulture);
            //        App.PCWeight = currentWeight;
            //        if (IsFirstWorkSet)
            //            Xamarin.Forms.MessagingCenter.Send<OneRMChangedMessage>(new OneRMChangedMessage() { model = this, Weight = currentWeight, Reps = Reps }, "OneRMChangedMessage");
            //    }
                
            //});
            //RepsChangedCommand = new Command<TextChangedEventArgs>((newText) =>
            //{
            //    if (IsFirstWorkSet)
            //    {
            //        var reps = Convert.ToInt32(newText.NewTextValue.Replace(",", "").Replace(".", ""));
            //        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;

            //        Xamarin.Forms.MessagingCenter.Send<OneRMChangedMessage>(new OneRMChangedMessage() { model = this, Weight = isKg? Weight.Kg : Weight.Lb, Reps = Reps }, "OneRMChangedMessage");
            //    }
                    
            //});
            //WeightUnfocusedCommand = new Command<object>((newText) => {
            //    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                //string entryText = newText.Replace(",", ".");
                //entryText = entryText.Replace(" ", "");
                //if (!string.IsNullOrEmpty(entryText))
                //{
                //    var currentWeight = Convert.ToDecimal(entryText, CultureInfo.InvariantCulture);
                //    App.PCWeight = currentWeight;
                //    Weight = new MultiUnityWeight(currentWeight, isKg ? WeightUnities.kg : WeightUnities.lb, false);
                //    if (!IsBackOffSet && !IsWarmups && !IsFinished && !IsEditing)
                //        Xamarin.Forms.MessagingCenter.Send<WeightRepsUpdatedMessage>(new WeightRepsUpdatedMessage() { model = this }, "WeightRepsUpdatedMessage");
                //}
            //});
        }
        //public ICommand WeightChangedCommand { get; set; }
        //public ICommand RepsChangedCommand { get; set; }
        //public ICommand WeightUnfocusedCommand { get; set; }

            public bool IsMaxChallenge
        {
            get { return _isMaxChallenge; }

            set
            {
                _isMaxChallenge = value;
                OnPropertyChanged(nameof(IsMaxChallenge));
            }
        }
            public bool IsAssisted { get; set; }
        public bool IsJustSetup { get; set; }
        public bool IsSizeChanged
        {
            get { return _isSizeChanged; }
            set
            {
                _isSizeChanged = value;
                
                OnSizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public bool IsUnilateral
        {
            get { return _isUnilateral; }
            set
            {
                _isUnilateral = value;
            }
        }
        public bool IsTimeBased
        {
            get { return _isTimeBased; }
            set
            {
                _isTimeBased = value;
                OnPropertyChanged(nameof(IsTimeBased));
            }
        }

        public string LastTimeSet
        {
            get { 
                    //if (IsNext)
                    return _lastTimeSet;
                if (string.IsNullOrEmpty(_lastTimeSet))
                    return "";
                return _lastTimeSet.Replace("Last time: ", "");
            }
            set
            {
                _lastTimeSet = value;
                OnPropertyChanged(nameof(LastTimeSet));
            }
        }
 private bool _isFirstSetFinished;
        public bool IsFirstSetFinished
        {
            get { return _isFirstSetFinished; }
            set
            {
                _isFirstSetFinished = value;
                OnPropertyChanged(nameof(IsFirstSetFinished));
            }
        }
        public bool ShouldUpdateIncrement { get; set; }
        public bool IsBackOffSet { get; set; }
        public bool IsNextBackOffSet { get; set; }
        public bool IsExerciseFinished
        {
            get { return _isExerciseFinished; }
            set
            {
                _isExerciseFinished = value;
                OnPropertyChanged("IsExerciseFinished");
            }
        }
        private MultiUnityWeight _weight;

        public MultiUnityWeight Increments { get; set; }
        public MultiUnityWeight Min { get; set; }
        public MultiUnityWeight Max { get; set; }

        public MultiUnityWeight PreviousWeight { get; set; }
        

        public MultiUnityWeight Weight
        {
            get

            { return _weight; }

            set
            {
                _weight = value;
                OnPropertyChanged("Weight");
                OnPropertyChanged("WeightSingal");
                OnPropertyChanged("WeightDouble");
            }
        }

        private bool _ShowPlusTooltip;
        public bool ShowPlusTooltip
        {
            get { return _ShowPlusTooltip; }
            set
            {
                _ShowPlusTooltip = value;
                OnPropertyChanged(nameof(ShowPlusTooltip));
            }
        }
        private bool _showSuperSet3;
        public bool ShowSuperSet3
        {
            get { return _showSuperSet3; }
            set
            {
                _showSuperSet3 = value;
                OnPropertyChanged(nameof(ShowSuperSet3));
            }
        }


        private bool _ShowSuperSet2;
        public bool ShowSuperSet2
        {
            get { return _ShowSuperSet2; }
            set
            {
                _ShowSuperSet2 = value;
                OnPropertyChanged(nameof(ShowSuperSet2));
            }
        }

        private bool _iSbackOffSetTooltip;
        public bool ISbackOffSetTooltip
        {
            get { return _iSbackOffSetTooltip; }
            set
            {
                _iSbackOffSetTooltip = value;
                OnPropertyChanged(nameof(ISbackOffSetTooltip));
            }
        }

        public bool IsBodyweight
        {
            get { return _isBodyweight; }
            set
            {
                _isBodyweight = value;
                OnPropertyChanged(nameof(IsBodyweight));
            }
        }
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged("IsActive");

            }
        }
        public string WeightSingal
        {
            get
            {
                if (IsBodyweight && Id == 16508)
                    return IsWarmups ? "Brisk" : "Fast";
                if (IsBodyweight && BodypartId == 12)
                    return IsWarmups ? "Brisk" : IsFirstWorkSet ? "Fast" : "Cooldown";
                if (Id >= 16897 && Id <= 16907 || Id == 14279 || Id >= 21508 && Id <= 21514)
                    return "Bands";
                if (IsBodyweight)
                    return "Body";
                
                if (Weight != null && LocalDBManager.Instance.GetDBSetting("massunit") != null)
                {
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                    if (IsPlateAvailable || IsDumbbellAvailable || IsPulleyAvailable)
                    {
                        decimal w1 = isKg ? Weight.Kg : Weight.Lb;

                        decimal result1 = w1 - Math.Truncate(w1);
                        return result1 > (decimal)0.001 ?
                                                                        string.Format("{0}", Math.Round(w1, 2)).TrimEnd('0') :
                                                                        string.Format("{0}", Math.Round(w1, 0)); //
                    }
                    decimal w = isKg ?
                                                                     ShouldUpdateIncrement == true ? Weight.Kg : SaveSetPage.RoundToNearestIncrement(Weight.Kg,Increments == null ? 2 : Increments.Kg,Min?.Kg,Max?.Kg):
                                                                    ShouldUpdateIncrement == true ? Weight.Lb : SaveSetPage.RoundToNearestIncrement(Weight.Lb, Increments == null ? (decimal)5 : Increments.Lb, Min?.Lb, Max?.Lb);
                    var result = w - Math.Truncate(w);

                    return result > (decimal)0.001 ?
                                                                    string.Format("{0}", Math.Round(w, 2)).TrimEnd('0') :
                                                                    string.Format("{0}", Math.Round(w, 0));//
                }
                return "";

            }
        }

        public string WeightDouble
        {
            get
            {
                if (Weight != null && LocalDBManager.Instance.GetDBSetting("massunit") != null)
                {
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                    

                    if (IsBodyweight)
                        return string.Format("{0}", isKg ? Math.Round(Weight.Kg,2) : Math.Round(Weight.Lb,2));
                    if (IsPlateAvailable || IsDumbbellAvailable || IsPulleyAvailable)
                    {
                        decimal w1 = isKg ? Weight.Kg : Weight.Lb; 

                        decimal result1 = w1 - Math.Truncate(w1);
                        return result1 > (decimal)0.001 ?
                                                                        string.Format("{0}", Math.Round(w1, 2)).TrimEnd('0') :
                                                                        string.Format("{0}", Math.Round(w1, 0)); //
                    }
                    decimal w = isKg ?
                                                                     ShouldUpdateIncrement == true ? Weight.Kg : SaveSetPage.RoundToNearestIncrement(Weight.Kg, Increments == null ? 2 : Increments.Kg, Min?.Kg, Max?.Kg) :
                                                                    ShouldUpdateIncrement == true ? Weight.Lb : SaveSetPage.RoundToNearestIncrement(Weight.Lb, Increments == null ? (decimal)5 : Increments.Lb, Min?.Lb, Max?.Lb);

                    decimal result = w - Math.Truncate(w);
                    return result > (decimal)0.001 ?
                                                                    string.Format("{0}", Math.Round(w, 2)).TrimEnd('0') :
                                                                    string.Format("{0}", Math.Round(w, 0)); //
                }
                return "";

            }
        }

        public string PreviousWeightDouble
        {
            get
            {
                if (PreviousWeight != null && LocalDBManager.Instance.GetDBSetting("massunit") != null)
                {
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? true : false;
                    decimal w = isKg ?
                                                                     ShouldUpdateIncrement == true ? Weight.Kg : SaveSetPage.RoundToNearestIncrement(PreviousWeight.Kg, Increments == null ? 2 : Increments.Kg, Min?.Kg, Max?.Kg) :
                                                                    ShouldUpdateIncrement == true ? Weight.Lb : SaveSetPage.RoundToNearestIncrement(PreviousWeight.Lb, Increments == null ? (decimal)5 : Increments.Lb, Min?.Lb, Max?.Lb);

                    decimal result = w - Math.Truncate(w);
                    return result > (decimal)0.001 ?
                                                                    string.Format("{0}", Math.Round(w, 2)).TrimEnd('0') :
                                                                    string.Format("{0}", Math.Round(w, 0)); //
                }
                return "";

            }
        }

        public int PreviousReps { get; set;}
        public bool IsExtraSet { get; set; }

        private int _reps;
        public new int Reps
        {
            get { return _reps; }
            set
            {
                _reps = value;
                OnPropertyChanged("Reps");
            }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set
            {
                _isEditing = value;
                OnPropertyChanged("IsEditing");
            }

        }

        public string HeaderTitle
        {
            get { return _headerTitle; }
            set
            {
                _headerTitle = value;
                OnPropertyChanged("HeaderTitle");
            }
        }

        public string VideoUrl
        {
            get { return _videoUrl; }
            set
            {
                _videoUrl = value;
                OnPropertyChanged("VideoUrl");
                OnPropertyChanged("IsVideoUrlAvailable");
            }
        }

        public bool IsVideoUrlAvailable
        {
             get {

                if (!string.IsNullOrEmpty(VideoUrl) && IsHeaderCell)
                    return  true;
                return false;
            }
        }



        public string SetTitle
        {
            get { return _setTitle; }
            set
            {
                _setTitle = value;
                OnPropertyChanged("SetTitle");
            }
        }

        public bool IsFirstSide
        {
            get { return _isFirstSide; }
            set
            {
                _isFirstSide = value;
                OnPropertyChanged("IsFirstSide");
            }
        }

        public string HeaderImage
        {
            get { return _headerImage; }
            set
            {
                _headerImage = value;
                OnPropertyChanged("HeaderImage");
            }
        }
        //    private bool _isSetupNotCompleted;

        public bool IsSetupNotCompleted { get; set; }
    //{
    //        get { return _isSetupNotCompleted; }
    //        set
    //        {
    //            _isSetupNotCompleted = value;
    //            OnPropertyChanged("IsSetupNotCompleted");
    //        }
    //    }

        public bool IsHeaderCell
        {
            get { return _isHeaderCell; }
            set
            {
                _isHeaderCell = value;
                OnPropertyChanged("IsHeaderCell");
            }
        }

        public bool IsLastSet
        {
            get { return _isLastSet; }
            set
            {
                _isLastSet = value;
                OnPropertyChanged("IsLastSet");
            }
        }


        public bool IsNext
        {
            get { return _isNext; }
            set
            {
                _isNext = value;
                OnPropertyChanged("IsNext");
                if (Device.RuntimePlatform.Equals(Device.Android)) { 
                    OnPropertyChanged(nameof(SetNo));
                    OnPropertyChanged(nameof(LastTimeSet));
                }
            }
        }

        public Color BackColor
        {
            get
            {
                if (IsFinished || IsNext)
                    return Device.RuntimePlatform == Device.Android ? Color.FromHex("#4D0C2432") : Color.FromHex("#660C2432");//195276
                else
                   return Color.Transparent;
            }
        }
        public bool IsFinished
        {
            get { return _isFinished; }
            set
            {
                _isFinished = value;
                OnPropertyChanged("IsFinished");
                OnPropertyChanged(nameof(SetNo));
                OnPropertyChanged("BackColor");
            }
        }
        public bool IsNormalset { get; set; }
        

        public string SetNo
        {
            get
            {
                if (_setNo == null)
                    return _setNo;
                
                    return _setNo.Replace("SET ", "");
                //else if (IsNext && !_setNo.Contains("SET "))
                //            return "SET " + _setNo;
                //return _setNo;
            }
            set
            {
                _setNo = value;
                OnPropertyChanged("SetNo");
            }
        }

        public bool IsLastWarmupSet { get; set; }
        public bool IsFirstWorkSet { get; set; }
        public bool ShowWorkTimer { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose of managed resources here
                    //eventSource.SomeEvent -= OnSomeEvent;
                }

                // Dispose of unmanaged resources here

                disposed = true;
            }
        }
    }
}
