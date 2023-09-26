using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Acr.UserDialogs;
using System.Collections.ObjectModel;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using DrMuscle.Screens.Exercises;
using System.Globalization;
using DrMuscle.Layout;
using DrMuscle.Resx;
using DrMuscle.Constants;
using Microsoft.AppCenter.Crashes;
using DrMuscle.Effects;
using DrMuscle.Helpers;
using DrMuscle.Screens.Workouts;
using DrMuscle.Message;

namespace DrMuscle.Screens.History
{
    public partial class HistoryPage : DrMusclePage
    {
        private Dictionary<string, long> ExercisesLabelToID;
        private Dictionary<string, TimeSpan> DurationToDays;
        ObservableCollection<HistoryItem> historyModel = new ObservableCollection<HistoryItem>();
        private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        private Dictionary<double, string> IndexToDateLabel2 = new Dictionary<double, string>();
        GetUserWorkoutLogAverageResponse mainWorkoutLog;
        bool isHistoryLoaded = false;
        double chartWidth = 0;
        public HistoryPage()
        {
            InitializeComponent();


           
        }

    }

    public class HistoryItem
    {
        public object Model { get; set; }
        public long? WorkoutLogId { get; set; }
        public long? ExerciseId { get; set; }
        public long? WorkoutLogSerieId { get; set; }
        public string Label { get; set; }
        public HistoryItemType ItemType { get; set; }
        public string TotalSets { get; set; }
        public string TotalReps { get; set; }
        public string TotalWeight { get; set; }
        public string AverageWeightPerRep { get; set; }
    }

    public class LineTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value.ToString().ToLower())
            {
                case "DATE":
                    return Color.Black;
                case "EXERCISE":
                    return Color.Gray;
                case "SET":
                    return Color.Transparent;
            }

            return Color.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // You probably don't need this, this is used to convert the other way around
            // so from color to yes no or maybe
            throw new NotImplementedException();
        }
    }

    public enum HistoryItemType
    {
        DateType,
        ExerciseType,
        SetType,
        StatisticType
    }

    public class HistoryDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HistoryDateTemplate { get; set; }
        public DataTemplate HistoryExerciseTemplate { get; set; }
        public DataTemplate HistorySetTemplate { get; set; }
        public DataTemplate HistoryStatisticTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            switch (((HistoryItem)item).ItemType)
            {
                case HistoryItemType.DateType: return HistoryDateTemplate;
                case HistoryItemType.ExerciseType: return HistoryExerciseTemplate;
                case HistoryItemType.StatisticType: return HistoryStatisticTemplate;
                default: return HistorySetTemplate;
            }
        }
    }
}
