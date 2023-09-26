using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Microsoft.AppCenter.Crashes;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using Xamarin.Forms;

namespace DrMuscle.Cells
{
    public partial class ChartCell : ViewCell
    {
        GetUserWorkoutLogAverageResponse workoutLogAverage = null;
        private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        private Dictionary<double, string> IndexToDateLabel2 = new Dictionary<double, string>();

        public ChartCell()
        {
            InitializeComponent();
            try
            {

                plotView.Model = null;
                plotView2.Model = null;

            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            try
            {
                var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                if (workouts.Sets != null)
                {
                    workoutLogAverage = workouts;
                    plotView.Model = null;
                    plotView2.Model = null;
                    SetChartData();
                }
                else
                    workoutLogAverage = null;

            }
            catch (Exception ex)
            {

            }
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.ForceUpdateSize();
                });
            }
        }
        private async void SetChartData()
        {
            //Settings value

            if (LocalDBManager.Instance.GetDBSetting("email") == null)
                return;
            if (LocalDBManager.Instance.GetDBSetting("massunit") == null)
                return;
            //Setting Stats of workout done and total weight lifted
            try
            {
                var exerciseModel = workoutLogAverage.HistoryExerciseModel;
                if (exerciseModel != null)
                {
                    bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                    var unit = inKg ? AppResources.Kg.ToLower() : AppResources.Lbs.ToLower();
                    //lblWorkoutsDone.IsVisible = true;
                    //lblLiftedCount.IsVisible = true;
                    var weightLifted = inKg ? exerciseModel.TotalWeight.Kg : exerciseModel.TotalWeight.Lb;
                    //lblWorkoutsDone.Text = exerciseModel.TotalWorkoutCompleted <= 1 ? $"{exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutDone}" : $"{exerciseModel.TotalWorkoutCompleted} {AppResources.WorkoutsDone}";
                    //lblLiftedCount.Text = $"{weightLifted.ToString("N0")} {unit} {AppResources.Lifted}";
                }
            }
            catch (Exception ex)
            {

            }

            //Drawing Chart
            try
            {
                plotView.Model = null;
                plotView2.Model = null;
                //LblProgress.Text = "";
                //if (workoutLogAverage == null)
                //return;
                if (workoutLogAverage == null || !workoutLogAverage.Averages.Any())
                {
                    //NoDataLabel.IsVisible = true;
                    var plotModel = new PlotModel
                    {
                        Title = AppResources.TryaWorkoutToSee.ToLower().FirstCharToUpper(),
                        TitleFontSize = Device.RuntimePlatform.Equals(Device.Android) ? 14 : 14,
                        TitleFontWeight = FontWeights.Normal,
                        TitleColor = OxyColor.Parse("#23253A"),
                        Background = OxyColors.Transparent,
                        PlotAreaBackground = OxyColors.Transparent,
                        PlotAreaBorderColor = OxyColor.Parse("#23253A"),
                        LegendTextColor = OxyColor.Parse("#23253A"),
                        LegendPosition = LegendPosition.RightMiddle,
                        LegendOrientation = LegendOrientation.Vertical,
                        LegendPadding = 20,
                        //LegendLineSpacing = 20,
                        //LegendItemSpacing = 20,
                    };

                    LinearAxis yAxis = new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 50, Minimum = 10, Maximum = 20, AxislineColor = OxyColors.Blue, ExtraGridlineColor = OxyColors.Blue, MajorGridlineColor = OxyColors.Blue, MinorGridlineColor = OxyColors.Blue, TextColor = OxyColors.Blue, TicklineColor = OxyColors.Blue, TitleColor = OxyColors.Blue, TickStyle = TickStyle.None };
                    yAxis.IsAxisVisible = false;
                    LinearAxis xAxis = new LinearAxis { Position = AxisPosition.Bottom, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColors.Blue, TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColors.Blue };

                    xAxis.LabelFormatter = _formatter;
                    xAxis.MinimumPadding = 1;
                    xAxis.IsPanEnabled = false;
                    xAxis.IsZoomEnabled = false;
                    xAxis.Minimum = 0.5;
                    xAxis.Maximum = 3.5;

                    IndexToDateLabel.Clear();
                    IndexToDateLabel.Add(xAxis.Minimum, "");
                    IndexToDateLabel.Add(xAxis.Maximum, "");

                    yAxis.IsPanEnabled = false;
                    yAxis.IsZoomEnabled = false;
                    plotModel.Axes.Add(yAxis);
                    plotModel.Axes.Add(xAxis);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        plotView.Model = plotModel;
                    });
                    var plotModel2 = new PlotModel
                    {
                        Title = AppResources.YourProgressInThisChart.ToLower().FirstCharToUpper(),//AppResources.TryAWorkoutToSeeYourProgressInThisChart,
                        TitleFontSize = Device.RuntimePlatform.Equals(Device.Android) ? 14 : 14,
                        TitleFontWeight = FontWeights.Normal,
                        TitleColor = OxyColor.Parse("#23253A"),
                        Background = OxyColors.Transparent,
                        PlotAreaBackground = OxyColors.Transparent,
                        PlotAreaBorderColor = OxyColor.Parse("#23253A"),
                        LegendTextColor = OxyColor.Parse("#23253A"),
                        LegendPosition = LegendPosition.RightMiddle,
                        LegendOrientation = LegendOrientation.Vertical,
                        LegendPadding = 20,
                        //LegendLineSpacing = 20,
                        //LegendItemSpacing = 20,
                    };

                    LinearAxis yAxis2 = new LinearAxis { Position = AxisPosition.Left, MinimumPadding = 50, Minimum = 10, Maximum = 20, AxislineColor = OxyColors.Blue, ExtraGridlineColor = OxyColors.Blue, MajorGridlineColor = OxyColors.Blue, MinorGridlineColor = OxyColors.Blue, TextColor = OxyColors.Blue, TicklineColor = OxyColors.Blue, TitleColor = OxyColors.Blue, TickStyle = TickStyle.None };
                    yAxis2.IsAxisVisible = false;
                    LinearAxis xAxis2 = new LinearAxis { Position = AxisPosition.Bottom, AxislineColor = OxyColors.Blue, ExtraGridlineColor = OxyColors.Blue, MajorGridlineColor = OxyColors.Blue, MinorGridlineColor = OxyColors.Blue, TextColor = OxyColors.Blue, TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColors.Blue };

                    xAxis2.LabelFormatter = _formatter;
                    xAxis2.MinimumPadding = 1;
                    xAxis2.IsPanEnabled = false;
                    xAxis2.IsZoomEnabled = false;
                    xAxis2.Minimum = 0.5;
                    xAxis2.Maximum = 3.5;

                    IndexToDateLabel.Clear();
                    IndexToDateLabel.Add(xAxis2.Minimum, "");
                    IndexToDateLabel.Add(xAxis2.Maximum, "");

                    yAxis2.IsPanEnabled = false;
                    yAxis2.IsZoomEnabled = false;
                    plotModel2.Axes.Add(yAxis2);
                    plotModel2.Axes.Add(xAxis2);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        
                        plotView2.Model = plotModel2;
                    });
                    try
                    {

                    plotModel.PlotView.InvalidatePlot(true);
                    plotModel2.PlotView.InvalidatePlot(true);

                    }
                    catch (Exception ex)
                    {

                    }
                    //LblSetsProgress.Text = "";
                    //LblProgress.Text = "";
                    //lblLastWorkout.Text = "";
                }
                else
                {
                    // NoDataLabel.IsVisible = false;

                    var plotModel = new PlotModel
                    {
                        Title = "Total strength",//AppResources.MaxStrengthCapital.ToLower().FirstCharToUpper(),
                        TitleFontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 16,
                        TitleFontWeight = FontWeights.Normal,
                        TitleColor = OxyColor.Parse("#23253A"),
                        Background = OxyColors.Transparent,
                        PlotAreaBackground = OxyColors.Transparent,
                        PlotAreaBorderColor = OxyColor.Parse("#23253A"),
                        //LegendPlacement = LegendPlacement.Outside,
                        //LegendTextColor = OxyColor.Parse("#23253A"),
                        //LegendPosition = LegendPosition.BottomCenter,
                        //LegendOrientation = LegendOrientation.Horizontal,
                        //LegendLineSpacing = 5,
                        IsLegendVisible = true
                        //LegendItemSpacing = 20,
                    };
                    
                    bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";

                    try
                    {
                        
                        var minVal = (double)workoutLogAverage.Averages.Min(a => inKg ? a.Average.Kg : a.Average.Lb);
                        var maxVal = (double)workoutLogAverage.Averages.Max(a => inKg ? a.Average.Kg : a.Average.Lb);
                        var min = minVal - (maxVal - minVal) * 0.20;
                        var max = maxVal + (maxVal - minVal) * 0.5;

                        //var max = maxVal + ((6.0 / 100.0) * maxVal); //maxVal + (1.42*(maxVal)) *maxVal/19.0;
                        //19:27 // 

                        //if (workoutLogAverage.Sets != null && workoutLogAverage.Sets.Count > 0)
                        //{
                        //    min = Math.Min((double)workoutLogAverage.Averages.Min(a => inKg ? a.Average.Kg : a.Average.Lb) - 100, workoutLogAverage.Sets.Min() - 100);
                        //    max = Math.Max((double)workoutLogAverage.Averages.Max(a => inKg ? a.Average.Kg : a.Average.Lb) + (inKg ? 600 : 1200), workoutLogAverage.Sets.Max() + (inKg ? 600 : 1200));
                        //}

                        LinearAxis yAxis = new LinearAxis { Position = AxisPosition.Left, MinimumPadding=50, Minimum = min, Maximum = max, AxislineColor = OxyColors.Blue, ExtraGridlineColor = OxyColors.Blue, MajorGridlineColor = OxyColors.Blue, MinorGridlineColor = OxyColors.Blue, TextColor = OxyColors.Blue, TicklineColor = OxyColors.Blue, TitleColor = OxyColors.Blue, TickStyle = TickStyle.None };
                        yAxis.IsAxisVisible = false;
                        LinearAxis xAxis = new LinearAxis { Position = AxisPosition.Bottom, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColor.Parse("#23253A"), TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColors.Blue, FontSize = 12, MinimumMajorStep = 0.3, MinorStep = 0.5, MajorStep = 0.5 };

                        xAxis.LabelFormatter = _formatter;
                        xAxis.MinimumPadding = 0.05;
                        xAxis.MaximumPadding = 0.1;
                        xAxis.IsPanEnabled = false;
                        xAxis.IsZoomEnabled = false;
                        xAxis.Minimum = 0.5;
                        xAxis.Maximum = 3.5;

                        IndexToDateLabel.Clear();
                        IndexToDateLabel.Add(xAxis.Minimum, "");
                        IndexToDateLabel.Add(xAxis.Maximum, "");

                        yAxis.IsPanEnabled = false;
                        yAxis.IsZoomEnabled = false;
                        plotModel.Axes.Add(yAxis);
                        plotModel.Axes.Add(xAxis);
                    }
                    catch (Exception)
                    {

                    }
                    var s1 = new LineSeries()
                    {
                        //Title = AppResources.MaxStrength,
                        Color = OxyColor.Parse("#38418C"),
                        TextColor = OxyColor.Parse("#38418C"),
                        LabelFormatString = "{1:0}",
                        //LabelMargin = -26,
                        FontSize = 15,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 6,
                        MarkerStroke = OxyColor.Parse("#38418C"),
                        MarkerFill = OxyColor.Parse("#38418C"),
                        MarkerStrokeThickness = 1,
                    };
                    int index = 1;

                    DateTime? creationDate = null;
                    bool isestimated = true;
                    try
                    {
                        creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                    }
                    catch (Exception)
                    {

                    }
                    foreach (var sets in workoutLogAverage.Sets)
                    {
                        if (sets != 0)
                            isestimated = false;
                    }
                        if (workoutLogAverage.Averages.Count > 2)
                    {
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {
                            //if (workoutLogAverage.Averages[1].Average.Kg == 0 || isestimated)
                            //    s1.Points.Add(new DataPoint(index, 0));
                            //else
                                s1.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));
                            // plotModel.Annotations.Add(new TextAnnotation() { Text = "Estimate", TextVerticalAlignment = VerticalAlignment.Top, StrokeThickness = 0, Stroke = OxyColors.Transparent, TextPosition = new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)-20) });

                            index++;
                        }
                    }
                    else if (workoutLogAverage.Averages.Count == 2)
                    {
                        index = 2;
                        s1.Points.Add(new DataPoint(1, 0));
                        foreach (var data in workoutLogAverage.Averages.Take(2))
                        {
                            //if (creationDate != null && data.Date < ((DateTime)creationDate).Date || isestimated)
                            //    s1.Points.Add(new DataPoint(index, 0));
                            //else
                                s1.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));
                            //plotModel.Annotations.Add(new TextAnnotation() { Text = "Estimate", TextVerticalAlignment = VerticalAlignment.Top, StrokeThickness = 0, Stroke = OxyColors.Transparent, TextPosition = new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)-20) });
                            index++;
                        }

                    }
                    else
                    {
                        index = 3;
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {
                            //if (creationDate != null && data.Date < ((DateTime)creationDate).Date || isestimated)
                            //    s1.Points.Add(new DataPoint(index, 0));
                            //else
                                s1.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));

                            index--;
                        }
                        if (index > 0)
                        {
                            for (int i = index; i > 0; i--)
                            {
                                s1.Points.Add(new DataPoint(i, 0));
                                //IndexToDateLabel.Add(i, "");
                            }
                        }
                    }

                    /*
                     double[,] pointavg = new double[10,3] {
                                            { 320,310,330},
                                            { 150,106,270},
                                            { 260,260,250},
                                            { 300,302,301},
                                            { 440,460,480},
                                            { 205,203,204},
                                            { 104,102,101},
                                            { 170,190,200},
                                            { 310,310,350},
                                            { 170,180,180}
                                        };

                                        for (int i = 0; i < 10; i++)
                                        {
                                            s1 = new LineSeries()
                                            {
                                                //Title = AppResources.MaxStrength,
                                                Color = OxyColor.Parse("#38418C"),
                                                TextColor = OxyColor.Parse("#38418C"),
                                                LabelFormatString = "{1:0}",
                                                //LabelMargin = -26,
                                                FontSize = 15,
                                                MarkerType = MarkerType.Circle,
                                                MarkerSize = 6,
                                                MarkerStroke = OxyColor.Parse("#38418C"),
                                                MarkerFill = OxyColor.Parse("#38418C"),
                                                MarkerStrokeThickness = 1,
                                            };
                                            for (int j = 0; j < 3; j++)
                                            {
                                                s1.Points.Add(new DataPoint(j+1, Convert.ToDouble(pointavg[i,j])));

                                            }
                                            plotModel.Series.Add(s1);

                                        }
                     */
                    index = 1;
                    var s2 = new LineSeries()
                    {
                        //Title = AppResources.WorkSetsNoColon,
                        Color = OxyColor.Parse("#5DD397"),//Green
                        LabelFormatString = "{1:0}",
                        FontSize = 15,
                        TextColor = OxyColor.Parse("#5DD397"),
                        LineStyle = LineStyle.Dash,
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 6,
                        MarkerStroke = OxyColor.Parse("#5DD397"),
                        MarkerFill = OxyColor.Parse("#5DD397"),
                        MarkerStrokeThickness = 1,
                        
                    };

                    if (workoutLogAverage.Sets != null)
                    {
                        workoutLogAverage.Sets.Reverse();
                        workoutLogAverage.SetsDate.Reverse();
                        IndexToDateLabel.Clear();

                        foreach (var sets in workoutLogAverage.Sets)
                        {
                            s2.Points.Add(new DataPoint(index, Convert.ToDouble(sets)));
                            IndexToDateLabel.Add(index, workoutLogAverage.SetsDate[index - 1].ToLocalTime().ToString("MMM dd"));

                            index++;
                        }

                        //if (workoutLogAverage.Sets.Count > 1)
                        //{

                        //    int firstSets = workoutLogAverage.Sets[workoutLogAverage.Sets.Count - 1];
                        //    int lastSets = workoutLogAverage.Sets[workoutLogAverage.Sets.Count - 2];
                        //    try
                        //    {
                        //        decimal progressSets = (firstSets - lastSets) * 100 / firstSets;
                        //        LblSetsProgress.Text = String.Format("{0}: {1}{2} ({3}%)", AppResources.WorkSetsNoColon, (firstSets - lastSets) > 0 ? "+" : "", firstSets - lastSets, progressSets.ToString("0.00")).ReplaceWithDot();
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        LblSetsProgress.Text = "";
                        //    }
                        //}
                    }

                    plotModel.Series.Add(s1);
                    //plotModel.Series.Add(s2);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        plotView.Model = plotModel;
                    });
                    //Second Chart

                    var plotModel2 = new PlotModel
                    {
                        Title = AppResources.WorkSetsCapital.ToLower().FirstCharToUpper(),
                        TitleFontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 16,
                        TitleFontWeight = FontWeights.Normal,
                        TitleColor = OxyColor.Parse("#23253A"),
                        Background = OxyColors.Transparent,
                        PlotAreaBackground = OxyColors.Transparent,
                        PlotAreaBorderColor = OxyColor.Parse("#23253A"),
                        //LegendPlacement = LegendPlacement.Outside,
                        //LegendTextColor = OxyColor.Parse("#23253A"),
                        //LegendPosition = LegendPosition.BottomCenter,
                        //LegendOrientation = LegendOrientation.Horizontal,
                        //LegendLineSpacing = 5,
                        IsLegendVisible = true
                        //LegendItemSpacing = 20,
                    };

                    //bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";

                    try
                    {
                        
                        var minVal = (double)workoutLogAverage.Sets.Min();
                        var maxVal = (double)workoutLogAverage.Sets.Max();
                        //var change = maxVal * 0.2;
                        //var min = minVal - change;
                        //var max = 28 * maxVal / 19;

                        var min = minVal - (maxVal - minVal) * 0.20;
                        var max = maxVal + (maxVal - minVal) * 0.5;
                        if(min == 0 && max == 0)
                        {
                            min = -30;
                            max = 50;
                        }
                        //if (workoutLogAverage.Sets != null && workoutLogAverage.Sets.Count > 0)
                        //{
                        //    min = Math.Min((double)workoutLogAverage.Averages.Min(a => inKg ? a.Average.Kg : a.Average.Lb) - 300, workoutLogAverage.Sets.Min() - 300);
                        //    max = Math.Max((double)workoutLogAverage.Averages.Max(a => inKg ? a.Average.Kg : a.Average.Lb) + 300, workoutLogAverage.Sets.Max() + 300);
                        //}

                        LinearAxis yAxis = new LinearAxis { Position = AxisPosition.Left, Minimum = min, Maximum = max, MinimumPadding = 50, AxislineColor = OxyColors.Blue, ExtraGridlineColor = OxyColors.Blue, MajorGridlineColor = OxyColors.Blue, MinorGridlineColor = OxyColors.Blue, TextColor = OxyColors.Blue, TicklineColor = OxyColors.Blue, TitleColor = OxyColors.Blue, TickStyle = TickStyle.None };
                        yAxis.IsAxisVisible = false;
                        LinearAxis xAxis = new LinearAxis { Position = AxisPosition.Bottom, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColor.Parse("#23253A"), TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColors.Blue, MinimumMajorStep = 0.3, MinorStep = 0.5, MajorStep = 0.5 };

                        xAxis.LabelFormatter = _formatter;
                        xAxis.MinimumPadding = 1;
                        xAxis.IsPanEnabled = false;
                        xAxis.IsZoomEnabled = false;
                        xAxis.Minimum = 0.5;
                        xAxis.Maximum = 3.5;

                        IndexToDateLabel2.Clear();
                        IndexToDateLabel2.Add(xAxis.Minimum, "");
                        IndexToDateLabel2.Add(xAxis.Maximum, "");

                        yAxis.IsPanEnabled = false;
                        yAxis.IsZoomEnabled = false;
                        plotModel2.Axes.Add(yAxis);
                        plotModel2.Axes.Add(xAxis);
                    }
                    catch (Exception)
                    {

                    }
                    var s12 = new LineSeries()
                    {
                        //Title = AppResources.MaxStrength,
                        Color = OxyColor.Parse("#38418C"),
                        TextColor = OxyColor.Parse("#38418C"),
                        LabelFormatString = "{1:0}",
                        //LabelMargin = -26,
                        FontSize = 15,
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 6,
                        MarkerStroke = OxyColor.Parse("#38418C"),
                        MarkerFill = OxyColor.Parse("#38418C"),
                        MarkerStrokeThickness = 1,
                    };
                    index = 1;
                    if (workoutLogAverage.Averages.Count > 2)
                    {
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {
                            s12.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));
                            index++;
                        }
                    }
                    else if (workoutLogAverage.Averages.Count == 2)
                    {
                        index = 2;
                        s12.Points.Add(new DataPoint(1, 0));
                        foreach (var data in workoutLogAverage.Averages.Take(2))
                        {
                            s12.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));
                            //IndexToDateLabel.Add(index, data.Date.ToLocalTime().ToString("MM/dd", CultureInfo.InvariantCulture));
                            index++;
                        }

                    }
                    else
                    {
                        index = 3;
                        foreach (var data in workoutLogAverage.Averages.Take(3))
                        {
                            s12.Points.Add(new DataPoint(index, Convert.ToDouble(inKg ? data.Average.Kg : data.Average.Lb)));
                            //IndexToDateLabel.Add(index, data.Date.ToLocalTime().ToString("MM/dd", CultureInfo.InvariantCulture));
                            index--;
                        }
                        if (index > 0)
                        {
                            for (int i = index; i > 0; i--)
                            {
                                s12.Points.Add(new DataPoint(i, 0));
                                //IndexToDateLabel.Add(i, "");
                            }
                        }
                    }


                    index = 1;
                    var s22 = new LineSeries()
                    {
                        //Title = AppResources.WorkSetsNoColon,
                        Color = OxyColor.Parse("#5DD397"),
                        LabelFormatString = "{1:0}",
                        FontSize = 15,
                        TextColor = OxyColor.Parse("#5DD397"),
                        LineStyle = LineStyle.Dash,
                        MarkerType = MarkerType.Diamond,
                        MarkerSize = 6,
                        MarkerStroke = OxyColor.Parse("#5DD397"),
                        MarkerFill = OxyColor.Parse("#5DD397"),
                        MarkerStrokeThickness = 1,
                    };

                    if (workoutLogAverage.Sets != null)
                    {
                        IndexToDateLabel2.Clear();
                        foreach (var sets in workoutLogAverage.Sets)
                        {
                            s22.Points.Add(new DataPoint(index, Convert.ToDouble(sets)));
                            IndexToDateLabel2.Add(index, workoutLogAverage.SetsDate[index - 1].ToLocalTime().ToString("MM/dd", CultureInfo.InvariantCulture));

                            index++;
                        }

                       
                    }

                    //plotModel2.Series.Add(s12);
                    plotModel2.Series.Add(s22);
                    Device.BeginInvokeOnMainThread(() => { 
                        plotView2.Model = plotModel2;
                    });
                    if (workoutLogAverage.Sets != null)
                    {
                        workoutLogAverage.Sets.Reverse();
                        workoutLogAverage.SetsDate.Reverse();
                    }
                    OneRMAverage last = workoutLogAverage.Averages.ToList()[workoutLogAverage.Averages.Count - 1];

                    
                }
            }
            catch (Exception e)
            {
                var properties = new Dictionary<string, string>
                    {
                        { "AIPage_ChartCell", $"{e.StackTrace}" }
                    };
                Crashes.TrackError(e, properties);
            }
        }
        private string _formatter(double d)
        {
            return IndexToDateLabel.ContainsKey(d) ? IndexToDateLabel[d] : "";
        }

        void StrenthChart_Tapped(System.Object sender, System.EventArgs e)
        {
             UserDialogs.Instance.Alert(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = "Your max strength for all exercises done recently.",
                Title = "Total strength"
             });
        }

        void SetsChart_Tapped(System.Object sender, System.EventArgs e)
        {
            UserDialogs.Instance.Alert(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = "Your volume in the last 3 weeks.",
                Title = "Work sets"
            });
        }
    }
}
