using DrMuscleWebApiSharedModel;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Acr.UserDialogs;
using System.Diagnostics;
using DrMuscle.Screens.Exercises;
using DrMuscle.Layout;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using DrMuscle.Screens.Workouts;
using DrMuscle.Helpers;
using System.Threading.Tasks;
using DrMuscle.Resx;
using DrMuscle.Constants;
using Microsoft.AppCenter.Crashes;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;

namespace DrMuscle.Screens.Exercises
{
    public partial class SaveSetPage : DrMusclePage
    {
        private ObservableCollection<WorkoutLogSerieModelEx> workoutLogSerieModel;
        private ObservableCollection<WorkoutLogSerieModelEx> Side1SetworkoutLogSerieModel;
        private decimal currentWeight = 0;
        private int currentReps = 0;
        private int? RIR = 0;
        private decimal weightStep = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (decimal)1 : (decimal)2.5;
        TimerPopup popup;
        bool isFirstSide = false;
        bool isSecondSide = false;
        public SaveSetPage()
        {
            
        }

        public static decimal RoundToNearestIncrement(decimal numToRound, decimal step, decimal? min, decimal? max)
        {
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null && LocalDBManager.Instance.GetDBSetting("workout_increments")?.Value != null && CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].Increments != null)
                {
                    return numToRound;
                }
                else
                {
                    if (step == 0)
                        return numToRound;

                    if (min != null)
                    {
                        var numAdjustedForMin = (decimal)min;
                        while (numAdjustedForMin < numToRound)
                        {
                            numAdjustedForMin += step;
                        }
                        var numRounded = numAdjustedForMin;

                        if (max != null)
                        {
                            if (numRounded > max)
                                numRounded = (decimal)max;
                        }

                        return numRounded;
                    }
                    else
                    {
                        //Calc the floor value of numToRound
                        decimal floor = ((long)(numToRound / step)) * step;

                        //round up if more than 60% way of step
                        decimal round = floor;
                        decimal remainder = numToRound - floor;
                        if (remainder > (step * (decimal)0.40))
                            round += step;
                        if (max != null && round > max)
                        {
                            round = (decimal)max;
                            var steps = round % step;
                            round = round - steps;
                        }
                        return round;
                    }
                }
            }
            catch (Exception ex)
            {
                return numToRound;
            }

        }
        public static decimal RoundDownToNearestIncrement(decimal numToRound, decimal step, decimal? min, decimal? max)
        {
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null && LocalDBManager.Instance.GetDBSetting("workout_increments")?.Value != null && CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].Increments != null)
                {
                    return numToRound;
                }
                else
                {
                    if (step == 0)
                        return numToRound;

                    if (min != null)
                    {
                        var numAdjustedForMin = (decimal)min;
                        while (numAdjustedForMin < numToRound)
                        {
                            numAdjustedForMin += step;
                        }
                        var numRounded = numAdjustedForMin;

                        if (max != null)
                        {
                            if (numRounded > max)
                                numRounded = (decimal)max;
                        }

                        return numRounded;
                    }
                    else
                    {
                        if (step == 1 || step == (decimal)2.5)
                        {
                            if (numToRound == 1 ||
                                numToRound == 2 ||
                                numToRound == 3 ||
                                numToRound == 8 ||
                                numToRound == 12)
                            {
                                return numToRound;
                            }
                        }
                        //Calc the floor value of numToRound
                        decimal floor = ((long)(numToRound / step)) * step;

                        //round up if more than 60% way of step
                        decimal round = floor;
                        decimal remainder = numToRound - floor;
                        if (remainder > (step * (decimal)0.60))
                            round += step;
                        if (max != null && round > max)
                            round = (decimal)max;
                        return round;
                    }
                }
            }
            catch (Exception ex)
            {
                return numToRound;
            }
            
        }

        public static decimal RoundDownToNearestIncrementLb(decimal numToRound, decimal step, decimal? min, decimal? max)
        {
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null && LocalDBManager.Instance.GetDBSetting("workout_increments")?.Value != null && CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].Increments != null)
                {
                    return numToRound;
                }
                else
                {
                    if (step == 0)
                        return numToRound;

                    if (min != null)
                    {
                        var numAdjustedForMin = (decimal)min;
                        while (numAdjustedForMin < numToRound)
                        {
                            numAdjustedForMin += step;
                        }
                        var numRounded = numAdjustedForMin;

                        if (max != null)
                        {
                            if (numRounded > max)
                                numRounded = (decimal)max;
                        }

                        return numRounded;
                    }
                    else
                    {
                        if (step == 1 || step == (decimal)2.5)
                        {
                            if (numToRound == 1 ||
                                numToRound == 2 ||
                                numToRound == 3 ||
                                numToRound == 8 ||
                                numToRound == 12)
                            {
                                return numToRound;
                            }
                        }
                        //Calc the floor value of numToRound
                        decimal floor = ((long)(numToRound / step)) * step;

                        //round up if more than 60% way of step
                        decimal round = floor;
                        //decimal remainder = numToRound - floor;
                        //if (remainder > (step * (decimal)0.60))
                        //    round += step;
                        if (max != null && round > max)
                            round = (decimal)max;
                        return round;

                    }
                }
            }
            catch (Exception ex)
            {
                return numToRound;
            }

        }
        public static decimal RoundDownToNearest(decimal numToRound, decimal step)
        {
            try
            {

            
            if (LocalDBManager.Instance.GetDBSetting("workout_increments") != null && LocalDBManager.Instance.GetDBSetting("workout_increments")?.Value != null)
            {
                return numToRound;
            }
            else
            {
                if (step == 0)
                    return numToRound;

                if (step == 1 || step == (decimal)2.5)
                {
                    if (numToRound == 1 ||
                        numToRound == 2 ||
                        numToRound == 3 ||
                        numToRound == 8 ||
                        numToRound == 12 )
                    {
                        return numToRound;
                    }
                }
                //Calc the floor value of numToRound
                decimal floor = ((long)(numToRound / step)) * step;

                //round up if more than 60% way of step
                decimal round = floor;
                decimal remainder = numToRound - floor;
                if (remainder > (step * (decimal)0.60))
                    round += step;

                return round;
            }
            }
            catch (Exception ex)
            {
                return numToRound;
            }
        }
    }

    public class PositionConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
            /*
            ListBoxItem item = value as ListBoxItem;
            ListBox view = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
            int index = view.ItemContainerGenerator.IndexFromContainer(item);
            return index.ToString();*/
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}