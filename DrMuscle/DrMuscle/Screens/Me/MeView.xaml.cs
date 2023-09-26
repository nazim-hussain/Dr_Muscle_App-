using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using DrMuscle.Resx;
using DrMuscle.Screens.History;
using DrMuscleWebApiSharedModel;
using Microsoft.AppCenter.Crashes;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Plugin.Connectivity;
using System.Text.RegularExpressions;
using Xamarin.Forms;
using DrMuscle.Screens.Workouts;

namespace DrMuscle.Screens.Me
{
    public partial class MeView : ContentView
    {
        //private bool ShowStartWorkoutPopup { get; set; }
        //private IFirebase _firebase;
        //private GetUserProgramInfoResponseModel upi = null;
        //GetUserWorkoutLogAverageResponse workoutLogAverage = null;
        //private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        //private Dictionary<double, string> IndexToDateLabel2 = new Dictionary<double, string>();
        //bool IsLaunched = false;
        //bool isHistoryLoaded = false;
        //double chartWidth = 0;
        //private Dictionary<string, long> ExercisesLabelToID;
        //private Dictionary<string, TimeSpan> DurationToDays;
        //ObservableCollection<HistoryItem> historyModel = new ObservableCollection<HistoryItem>();
        //GetUserWorkoutLogAverageResponse mainWorkoutLog;

        private Dictionary<string, long> ExercisesLabelToID;
        private Dictionary<string, TimeSpan> DurationToDays;
        ObservableCollection<HistoryItem> historyModel = new ObservableCollection<HistoryItem>();
        private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        private Dictionary<double, string> IndexToDateLabel2 = new Dictionary<double, string>();
        GetUserWorkoutLogAverageResponse mainWorkoutLog;
        bool isHistoryLoaded = false;
        double chartWidth = 0;

        public MeView()
        {
            InitializeComponent();


        }


    }
}

