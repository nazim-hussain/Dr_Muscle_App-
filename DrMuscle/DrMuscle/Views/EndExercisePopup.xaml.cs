using System;
using System.Collections.Generic;
using DrMuscleWebApiSharedModel;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Xamarin.Forms;
using Acr.UserDialogs;
using DrMuscle.Layout;
using DrMuscle.Helpers;
using DrMuscle.Screens.Workouts;
using DrMuscle.Resx;
using DrMuscle.Constants;
using DrMuscle.Dependencies;
using DrMuscle.Entity;
using Xamarin.Essentials;
using DrMuscle.Screens.User.OnBoarding;
using OxyPlot.Annotations;
using DrMuscle.Message;
using DrMuscle.Services;
using Rg.Plugins.Popup.Services;
using System.Threading;
using Microcharts;
using SkiaSharp;
using System.Reflection;
using Rg.Plugins.Popup.Pages;
using System.Security.Cryptography;

namespace DrMuscle.Views
{
    public partial class EndExercisePopup : PopupPage
    {
        List<OneRMModel> _lastWorkoutLog = new List<OneRMModel>();
        private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        string strFacebook = "";
        bool ShouldAnimate = false;
        bool isEstimated = false;
        private decimal _userBodyWeight = 0;

        public EndExercisePopup(OneRMModel weight0, OneRMModel weight1)
        {
            InitializeComponent();

            
            RefreshLocalized();

            NextExerciseButton.Clicked += NextExerciseButton_Clicked;
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });

            MessagingCenter.Subscribe<Message.ReceivedWatchMessage>(this, "ReceivedWatchMessage", (obj) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (obj.PhoneToWatchModel.WatchMessageType == WatchMessageType.NextExercise)
                        NextExerciseButton_Clicked(NextExerciseButton, EventArgs.Empty);
                });

            });

            LoadBeforeServerCall(weight0, weight1);
        }

        public async void LoadBeforeServerCall(OneRMModel weight1RM0, OneRMModel weight1RM1)
        {
            
            DependencyService.Get<IFirebase>().SetScreenName("end_exercise_page");
            isEstimated = false;
            if (weight1RM1.OneRM == null)
                isEstimated = true;
           // IconResultImage.Source = "up_arrow.png";
            lblResult1.IsVisible = true;
            lblResult21.IsVisible = true;
            lblResult3.IsVisible = true;
            lblResult4.IsVisible = true;

            lblResult3.Text = "";
            lblResult21.Text = "";
            lblResult4.Text = "";
            bool isTimeBased = false;// CurrentLog.Instance.ExerciseLog.Exercice.IsTimeBased;
            //plotView.Model = null;
            Title = CurrentLog.Instance.ExerciseLog.Exercice.Label;
            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NextExercise, SetModel = new WorkoutLogSerieModelRef() }, "SendWatchMessage");


            NextExerciseButton.Text = AppResources.Continue;
            _lastWorkoutLog = new List<OneRMModel>();
            _lastWorkoutLog.Add(weight1RM0);
            _lastWorkoutLog.Add(weight1RM1);

            try
            {
                try
                {

                
                if (CurrentLog.Instance.ExerciseLog.Exercice.IsWeighted)
                {
                    if (LocalDBManager.Instance.GetDBSetting("BodyWeight")?.Value != null)
                        _userBodyWeight = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture);
                    foreach (var item in _lastWorkoutLog)
                    {
                        if (item.Weight != null)
                            item.OneRM = new MultiUnityWeight(ComputeOneRM(item.Weight.Kg + _userBodyWeight, item.Reps), "kg");

                    }

                }
                }
                catch (Exception ex)
                {

                }

                //Congratulations message

                DateTime minDate = _lastWorkoutLog.Min(p => p.OneRMDate);
                DateTime maxDate = _lastWorkoutLog.Max(p => p.OneRMDate);
                OneRMModel last = _lastWorkoutLog.First(p => p.OneRMDate == maxDate);
                //OneRMModel beforeBeforeLast = _lastWorkoutLog.Where(p => p.OneRMDate > minDate && p.OneRMDate < maxDate).Skip(1).First();
                OneRMModel beforeLast = _lastWorkoutLog.Where(p => p.OneRMDate == minDate).First();

                decimal weight0 = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                                                last.OneRM.Kg :
                                                last.OneRM.Lb;
                decimal weight1 = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                                                beforeLast.OneRM?.Kg ?? (decimal)0 :
                                                beforeLast.OneRM?.Lb ?? (decimal)0;
                

                decimal reps0 = last.Reps;
                decimal reps1 = beforeLast.Reps;
                decimal progressNumb = 0;
                try
                {
                    lblResult1.Text = "Strength up";//string.Format("{0} {1}!", AppResources.Congratulations, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                    ImgName.Source = "Trophy.png";

                    if (weight0 > weight1)
                    {
                        lblResult1.Text = "Strength up";
                        ImgName.Source = "Trophy.png";
                    }

                    if (isTimeBased)
                    {
                        lblResult21.Text = string.Format("{0} {1}", _lastWorkoutLog.ElementAt(0).Reps, "Secs");
                    }
                    else
                        lblResult21.Text = string.Format("{0:0.#} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1) :
                        Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Lb, 1),
                        LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();



                    try
                    {

                        if (LocalDBManager.Instance.GetDBSetting($"WorkoutStrenth{DateTime.Now.Date}") == null)
                            LocalDBManager.Instance.SetDBSetting($"WorkoutStrenth{DateTime.Now.Date}", Convert.ToString(Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1)).ReplaceWithDot());
                        else
                        {
                            var strenthCount = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting($"WorkoutStrenth{DateTime.Now.Date}").Value.ReplaceWithDot(), System.Globalization.CultureInfo.InvariantCulture);
                            strenthCount += (double)Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1);
                            LocalDBManager.Instance.SetDBSetting($"WorkoutStrenth{DateTime.Now.Date}", $"{strenthCount}");
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {

                        if (LocalDBManager.Instance.GetDBSetting($"ExerciseStrenth{DateTime.Now.Date}") == null)
                        {
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenth{DateTime.Now.Date}", Convert.ToString(Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1)).ReplaceWithDot());
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}", CurrentLog.Instance.ExerciseLog.Exercice.Label);
                        }
                        else
                        {
                            var strenthCount = LocalDBManager.Instance.GetDBSetting($"ExerciseStrenth{DateTime.Now.Date}").Value.ReplaceWithDot();
                            //strenthCount += (double)Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1);
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenth{DateTime.Now.Date}", $"{strenthCount}|{Convert.ToString(Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1)).ReplaceWithDot()}");

                            var exerciseNames = LocalDBManager.Instance.GetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}").Value;
                            //strenthCount += (double)Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1);
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}", $"{exerciseNames}|{CurrentLog.Instance.ExerciseLog.Exercice.Label}");
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    lblResult3.Text = string.Format("{0:0.#} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        Math.Round(_lastWorkoutLog.ElementAt(1).OneRM?.Kg ?? 0, 1) :
                        Math.Round(_lastWorkoutLog.ElementAt(1).OneRM?.Lb ?? 0, 1),
                        LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();
                    lblResult3.IsVisible = true;
                    
                    
                    lblResult33.Text = "Last workout";
                    //lblResult44.Text = "Progress";

                    if (!isEstimated)
                    {
                        progressNumb = Math.Round(((Math.Round(weight0, 1) - Math.Round(weight1, 1)) * 100) / Math.Round(weight1, 1), 1);
                        lblResult4.Text = string.Format("{0}{1:0.#}%{2}", progressNumb>0 ? "+" : "", progressNumb, progressNumb > 0 ? "!" : "").ReplaceWithDot();
                        if (isTimeBased)
                        {
                            progressNumb = Math.Round(((reps0 - reps1) * 100) / reps1, 1);
                            lblResult4.Text = string.Format("{0}{1:0.#}%", progressNumb>0 ? "+" : "", progressNumb).ReplaceWithDot();
                        }

                    }
                    else
                    {
                        progressNumb = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1) :
                        Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Lb, 1);

                        lblResult4.Text = string.Format("{0}{1:0.#} {2}{3}",progressNumb>0 ? "+":"" , progressNumb,
                        LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", progressNumb>0 ? "!" : "").ReplaceWithDot();
                        lblResult3.Text = "0";
                        if (isTimeBased)
                        {
                            progressNumb = _lastWorkoutLog.ElementAt(0).Reps;
                            lblResult4.Text = string.Format("{0:0}%", progressNumb).ReplaceWithDot();
                        }
                        
                    }
                }
                catch (Exception ex)
                {

                }
                strFacebook = "";
                //If deload...
                if ( Math.Round(weight0,1) == Math.Round(weight1,1))
                {
                    ImgName.Source = "TrueState.png";
                    //IconResultImage.Source = "green.png";
                    lblResult1.Text = "Lift successful";// string.Format("{0} {1}", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                    //if (!isEstimated)
                    //{
                    //    if (Config.DownRecordPercentage > progressNumb)
                    //    {
                    //        Config.DownRecordPercentage = progressNumb;
                    //        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                    //        var newText = string.Format("{0:0.#} {1}", Math.Round(isKg ? last.OneRM.Kg : last.OneRM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                    //        var old = string.Format("{0:0.#} {1}", Math.Round(isKg ? beforeLast.OneRM.Kg : beforeLast.OneRM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();

                    //        Config.DownRecordExplainer = $"Last workout: {Title} {old}\nToday: {Title} {newText}\n\n"; ;
                    //    }
                    //}
                }
                else if (weight0 < (weight1 * (decimal)0.98) && weight0 < (weight1 - 2))
                {

                    lblResult1.IsVisible = true;

                    if (isEstimated)
                    {
                        lblResult1.Text = "Strength up";
                        ImgName.Source = "Trophy.png";
                       // IconResultImage.Source = "up_arrow.png";
                        //Set button text here:

                        lblResult1.IsVisible = true;

                    }
                    else if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsLightSession)
                    {
                        ImgName.Source = "TrueState.png";
                        lblResult1.Text = "Strength down"; //"Light session successful";// string.Format("{0} {1}!", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname").Value);

                       // IconResultImage.Source = "down_arrow.png";
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("RecoDeload").Value == "false")
                    {
                        lblResult1.Text = "Strength down";//string.Format("{0} {1}!", AppResources.Attention ,LocalDBManager.Instance.GetDBSetting("firstname").Value);
                        ImgName.Source = "alert_ic_blue.png";
                      //  IconResultImage.Source = "down_arrow.png";
                    }
                    //If 2e workout au retour de deload...
                    else
                    {
                        ImgName.Source = "TrueState.png";
                        lblResult1.Text = "Strength down"; //"Deload successful";//string.Format(AppResources.DeloadSuccessful);

                      //  IconResultImage.Source = "green.png";

                    }
                    //if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsEasy && !isEstimated)
                    //{
                    //    ImgName.Source = "TrueState.png";
                    //    lblResult1.Text = "Recovery successful";//$"{AppResources.WellDone} {LocalDBManager.Instance.GetDBSetting("firstname").Value}";

                    //}
                }
                //else if égal
                
                //else if légère diminution
                else if (weight0 >= (weight1 * (decimal)0.98) && weight0 <= weight1 || weight0 < (weight1 * (decimal)0.98) && weight0 >= (weight1 - 2))
                {
                    ImgName.Source = "TrueState.png";
                    //IconResultImage.Source = "green.png";
                    lblResult1.Text = "Lift successful";//string.Format("{0} {1}", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                    //if (Config.DownRecordPercentage > progressNumb)
                    //{
                    //    Config.DownRecordPercentage = progressNumb;
                    //    var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                    //    var newText = string.Format("{0:0.#} {1}", Math.Round(isKg ? last.OneRM.Kg : last.OneRM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                    //    var old = string.Format("{0:0.#} {1}", Math.Round(isKg ? beforeLast.OneRM.Kg : beforeLast.OneRM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();

                    //    Config.DownRecordExplainer = $"Last workout: {Title} {old}\nToday: {Title} {newText}\n\n"; ;
                    //}

                }
                //Sinon (if pas deload...)
                else
                {
                    //IconResultImage.Source = "up_arrow.png";
                    ImgName.Source = "Trophy.png";
                    //Set button text here:

                    lblResult1.IsVisible = true;

                }

                if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsEasy)
                {
                    //lblResult1.Text = "Recovery successful";//$"{AppResources.WellDone} {LocalDBManager.Instance.GetDBSetting("firstname").Value}";

                }

                NewRecordModel newModel = new NewRecordModel();
                newModel.ExerciseName = CurrentLog.Instance.ExerciseLog.Exercice.Label;
                if (!isEstimated)
                    newModel.Prev1RM = beforeLast.OneRM;
                newModel.New1RM = last.OneRM;
                newModel.IsMobility = CurrentLog.Instance.ExerciseLog.Exercice.IsFlexibility;
                newModel.ExercisePercentage = lblResult4.Text.Replace("+", "").Replace("!", "");
                newModel.ExercisePercentageNumber = progressNumb;
                ((App)Application.Current).NewRecordModelContext.NewRecordList.Add(newModel);
                ((App)Application.Current).NewRecordModelContext.SaveContexts();

                if (isEstimated)
                {
                    lblResult1.Text = "Strength up";
                    ImgName.Source = "Trophy.png";
                    //IconResultImage.Source = "up_arrow.png";
                    //Set button text here:
                    lblResult1.IsVisible = true;

                    try
                    {

                        if (LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout") == null)
                            LocalDBManager.Instance.SetDBSetting($"RecordFinishWorkout", "1");
                        else
                        {
                            var recordCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout").Value);
                            recordCount += 1;
                            LocalDBManager.Instance.SetDBSetting($"RecordFinishWorkout", $"{recordCount}");
                        }

                        //NewRecordModel newModel = new NewRecordModel();
                        //newModel.ExerciseName = CurrentLog.Instance.ExerciseLog.Exercice.Label;
                        //newModel.New1RM = last.OneRM;
                        //newModel.ExercisePercentage = lblResult4.Text.Replace("+","").Replace("!","");
                        //newModel.ExercisePercentageNumber = progressNumb;
                        //((App)Application.Current).NewRecordModelContext.NewRecordList.Add(newModel);
                        //((App)Application.Current).NewRecordModelContext.SaveContexts();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (lblResult1.Text == "Strength up" && !isEstimated)
                {
                    try
                    {

                        if (LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout") == null)
                            LocalDBManager.Instance.SetDBSetting($"RecordFinishWorkout", "1");
                        else
                        {
                            var recordCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout").Value);
                            recordCount += 1;
                            LocalDBManager.Instance.SetDBSetting($"RecordFinishWorkout", $"{recordCount}");
                        }
                        
                    }
                    catch (Exception ex)
                    {

                    }
                }
                //else if (!isEstimated)
                //{
                //    if (Config.DownRecordPercentage >= progressNumb)
                //    {
                //        Config.DownRecordPercentage = progressNumb;
                //        var isKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
                //        var newText = string.Format("{0:0.#} {1}", Math.Round(isKg ? last.OneRM.Kg : last.OneRM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();
                //        var old = string.Format("{0:0.#} {1}", Math.Round(isKg ? beforeLast.OneRM.Kg : beforeLast.OneRM.Lb, 1), (isKg ? "kg" : "lbs")).ReplaceWithDot();

                //        Config.DownRecordExplainer = $"Last workout: {Title} {old}\nToday: {Title} {newText}\n\n"; ;
                //    } 
                //}
            }
            catch (Exception e)
            {
                //await UserDialogs.Instance.AlertAsync(AppResources.PleaseCheckInternetConnection, AppResources.Error);
                //await UserDialogs.Instance.AlertAsync(new AlertConfig()
                //{
                //    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                //    Message = AppResources.PleaseCheckInternetConnection,
                //    Title = AppResources.ConnectionError
                //});
            }
            if (lblResult1.Text == "Strength up")
            {
                lblResult4.TextColor = AppThemeConstants.GreenColor;
                MyParticleCanvas.IsActive = true;
                MyParticleCanvas.IsRunning = true;
                //await Task.Delay(Device.RuntimePlatform.Equals(Device.Android) ? 9000 : 5000);
                //MyParticleCanvas.IsActive = false;
            }
            
        }

        public async void OnBeforeShow()
        {
            isEstimated = false;
            
            //lblResult3.Text = "";
            //lblResult21.Text = "";
            //lblResult4.Text = "";
            bool isTimeBased = false;// CurrentLog.Instance.ExerciseLog.Exercice.IsTimeBased;
            //plotView.Model = null;
            Title = CurrentLog.Instance.ExerciseLog.Exercice.Label;
            MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.NextExercise, SetModel = new WorkoutLogSerieModelRef() }, "SendWatchMessage");

            
            NextExerciseButton.Text = AppResources.Continue;

            
            try
            {
                if (CurrentLog.Instance.Exercise1RM.ContainsKey(CurrentLog.Instance.ExerciseLog.Exercice.Id) && CurrentLog.Instance.LastSerieModelList != null && CurrentLog.Instance.LastSerieModelList.Count > 0)
                {
                    _lastWorkoutLog = CurrentLog.Instance.Exercise1RM[CurrentLog.Instance.ExerciseLog.Exercice.Id];
                    if (_lastWorkoutLog != null)
                    {
                        List<decimal> listOf1Rm = new List<decimal>();
                        foreach (var item in CurrentLog.Instance.LastSerieModelList)
                        {
                            listOf1Rm.Add(ComputeOneRM(item.Weight.Kg, item.Reps));
                        }
                        _lastWorkoutLog.Remove(_lastWorkoutLog.Last());
                        _lastWorkoutLog.Insert(0, new OneRMModel()
                        {
                            ExerciseId = CurrentLog.Instance.ExerciseLog.Exercice.Id,
                            OneRMDate = DateTime.Now,
                            OneRM = new MultiUnityWeight(listOf1Rm.Max(), "kg"),
                            LastLogDate = DateTime.Now
                        });
                    }
                }
                else
                {

                    _lastWorkoutLog = await DrMuscleRestClient.Instance.GetOneRMForExerciseWithoutLoader(
                        new GetOneRMforExerciseModel()
                        {
                            Username = LocalDBManager.Instance.GetDBSetting("email").Value,
                            Massunit = LocalDBManager.Instance.GetDBSetting("massunit").Value,
                            ExerciseId = CurrentLog.Instance.ExerciseLog.Exercice.Id
                        }
                    );

                }
                if (CurrentLog.Instance.ExerciseLog.Exercice.IsWeighted)
                {

                    if (LocalDBManager.Instance.GetDBSetting("BodyWeight")?.Value != null)
                        _userBodyWeight = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture);
                    foreach (var item in _lastWorkoutLog)
                    {
                        item.OneRM = new MultiUnityWeight(ComputeOneRM(item.Weight.Kg + _userBodyWeight, item.Reps), "kg");
                    }
                }

                var chartSerie = new ChartSerie() { Name = AppResources.MAXSTRENGTHESTIMATELAST3WORKOUTS.ToLower().FirstCharToUpper(), Color = SKColor.Parse("#38418C") };
                List<ChartSerie> chartSeries = new List<ChartSerie>();

                List<ChartEntry> entries = new List<ChartEntry>();


                
                int i = 1;
                foreach (OneRMModel m in _lastWorkoutLog.OrderBy(w => w.OneRMDate))
                {
                    if (i == 2 && !m.IsAllowDelete)
                        isEstimated = true;
                    //if (!m.IsAllowDelete)
                    //    yAxis.Minimum = -120;
                    switch (LocalDBManager.Instance.GetDBSetting("massunit").Value)
                    {
                        default:
                        case "kg":

                            if (!m.IsAllowDelete)
                                entries.Add(new ChartEntry(0) { Label = m.OneRMDate.ToLocalTime().ToString("MMM dd"), ValueLabel = "0" });
                            else
                            {
                                var val = (float)Math.Round(m.OneRM.Kg);
                                entries.Add(new ChartEntry(val) { Label = m.OneRMDate.ToLocalTime().ToString("MMM dd"), ValueLabel = val.ToString() });
                                //var val = (float)Math.Round(inKg ? data.Average.Kg : data.Average.Lb);
                                //s1.Points.Add(new DataPoint(i, Convert.ToDouble(m.OneRM.Kg)));
                            }
                            break;
                        case "lb":
                            if (!m.IsAllowDelete)
                                entries.Add(new ChartEntry(0) { Label = m.OneRMDate.ToLocalTime().ToString("MMM dd"), ValueLabel = "0" });
                            else
                            {
                                var val = (float)Math.Round(m.OneRM.Lb);
                                entries.Add(new ChartEntry(val) { Label = m.OneRMDate.ToLocalTime().ToString("MMM dd"), ValueLabel = val.ToString() });
                            }
                            break;
                    }
                    //IndexToDateLabel.Add(i, m.OneRMDate.ToLocalTime().ToString("MMM dd"));

                    i++;
                }

                // plotModel.Series.Add(s1);
                //plotView.Model = plotModel;





                chartSerie.Entries = entries;
                chartSeries.Add(chartSerie);

                chartView.Chart = new LineChart
                {
                    LabelOrientation = Orientation.Vertical,
                    ValueLabelOrientation = Orientation.Vertical,
                    LabelTextSize = Device.RuntimePlatform == Device.iOS ? 22 : 26,
                    ValueLabelTextSize = Device.RuntimePlatform == Device.iOS ? 22 : 26,
                    SerieLabelTextSize = Device.RuntimePlatform == Device.iOS ? 20 : 24,
                    BackgroundColor = SKColors.Transparent,
                    LegendOption = SeriesLegendOption.None,
                    Series = chartSeries,
                };

                //Congratulations message

                DateTime minDate = _lastWorkoutLog.Min(p => p.OneRMDate);
                DateTime maxDate = _lastWorkoutLog.Max(p => p.OneRMDate);
                OneRMModel last = _lastWorkoutLog.First(p => p.OneRMDate == maxDate);
                OneRMModel beforeLast = _lastWorkoutLog.Where(p => p.OneRMDate > minDate && p.OneRMDate < maxDate).First();
                //OneRMModel beforeBeforeLast = _lastWorkoutLog.Where(p => p.OneRMDate > minDate && p.OneRMDate < maxDate).Skip(1).First();
                OneRMModel beforeBeforeLast = _lastWorkoutLog.Where(p => p.OneRMDate == minDate).First();

                decimal weight0 = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                                                last.OneRM.Kg :
                                                last.OneRM.Lb;
                decimal weight1 = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                                                beforeLast.OneRM.Kg :
                                                beforeLast.OneRM.Lb;
                decimal weight2 = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                                                beforeBeforeLast.OneRM.Kg :
                                                beforeBeforeLast.OneRM.Lb;

                decimal reps0 = last.Reps;
                decimal reps1 = beforeLast.Reps;
                decimal progressNumb = 0;
                try
                {
                    try
                    {

                        if (LocalDBManager.Instance.GetDBSetting($"WorkoutStrenth{DateTime.Now.Date}") == null)
                            LocalDBManager.Instance.SetDBSetting($"WorkoutStrenth{DateTime.Now.Date}", Convert.ToString(Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1)).ReplaceWithDot());
                        else
                        {
                            var strenthCount = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting($"WorkoutStrenth{DateTime.Now.Date}").Value.ReplaceWithDot(), System.Globalization.CultureInfo.InvariantCulture);
                            strenthCount += (double)Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1);
                            LocalDBManager.Instance.SetDBSetting($"WorkoutStrenth{DateTime.Now.Date}", $"{strenthCount}");
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    try
                    {

                        if (LocalDBManager.Instance.GetDBSetting($"ExerciseStrenth{DateTime.Now.Date}") == null)
                        {
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenth{DateTime.Now.Date}", Convert.ToString(Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1)).ReplaceWithDot());
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}", CurrentLog.Instance.ExerciseLog.Exercice.Label);
                        }
                        else
                        {
                            var strenthCount = LocalDBManager.Instance.GetDBSetting($"ExerciseStrenth{DateTime.Now.Date}").Value.ReplaceWithDot();
                            //strenthCount += (double)Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1);
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenth{DateTime.Now.Date}", $"{strenthCount}|{Convert.ToString(Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1)).ReplaceWithDot()}");

                            var exerciseNames = LocalDBManager.Instance.GetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}").Value;
                            //strenthCount += (double)Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1);
                            LocalDBManager.Instance.SetDBSetting($"ExerciseStrenthName{DateTime.Now.Date}", $"{exerciseNames}|{CurrentLog.Instance.ExerciseLog.Exercice.Label}");
                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    
                    //    lblResult4.Text = string.Format("{0} {1}",Math.Round(Math.Round(weight0,1) - Math.Round(weight1,1),1),
                    //(LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs")).ReplaceWithDot();
                    lblResult33.Text = "Last workout";
                    //lblResult44.Text = "Progress";

                    
                }
                catch (Exception ex)
                {

                }
                strFacebook = "";
                //If deload...

                if (weight0 < (weight1 * (decimal)0.98) && weight0 < (weight1 - 2))
                {
                    

                    if (isEstimated)
                    {
                        
                        
                        //Set button text here:

                        try
                        {

                            if (LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout") == null)
                                LocalDBManager.Instance.SetDBSetting($"RecordFinishWorkout", "1");
                            else
                            {
                                var recordCount = int.Parse(LocalDBManager.Instance.GetDBSetting($"RecordFinishWorkout").Value);
                                recordCount += 1;
                                LocalDBManager.Instance.SetDBSetting($"RecordFinishWorkout", $"{recordCount}");
                            }

                            //NewRecordModel newModel = new NewRecordModel();
                            //newModel.ExerciseName = CurrentLog.Instance.ExerciseLog.Exercice.Label;
                            //newModel.New1RM = last.OneRM;
                            //newModel.ExercisePercentageNumber = progressNumb;
                            //((App)Application.Current).NewRecordModelContext.NewRecordList.Add(newModel);
                            //((App)Application.Current).NewRecordModelContext.SaveContexts();
                        }
                        catch (Exception ex)
                        {

                        }
                        strFacebook = string.Format("{0} {1} {2} {3:f} {4}{5}", "I just smashed a new record!", CurrentLog.Instance.ExerciseLog.Exercice.Label, "is now", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                    _lastWorkoutLog.ElementAt(0).OneRM.Kg :
                    _lastWorkoutLog.ElementAt(0).OneRM.Lb, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", ". I train using Dr. Muscle. Get your invitation at:");

                    }
                    else if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsLightSession)
                    {
                        
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("RecoDeload").Value == "false")
                    {
                        
                    }
                    //If 2e workout au retour de deload...
                    else
                    {
                        

                    }
                    if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsEasy && !isEstimated)
                    {
                        
                        
                    }
                }
                //else if égal
                else if (weight0 == weight1)
                {
                    
                    
                }
                //else if légère diminution
                else if (weight0 >= (weight1 * (decimal)0.98) && weight0 <= weight1 || weight0 < (weight1 * (decimal)0.98) && weight0 >= (weight1 - 2))
                {
                    
                    
                }
                //Sinon (if pas deload...)
                else
                {
                    

                    strFacebook = string.Format("{0} {1} {2} {3:f} {4}{5}", "I just smashed a new record!", CurrentLog.Instance.ExerciseLog.Exercice.Label, "is now", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                    _lastWorkoutLog.ElementAt(0).OneRM.Kg :
                    _lastWorkoutLog.ElementAt(0).OneRM.Lb, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", ". I train using Dr. Muscle. Get your invitation at:");

                }
                
                if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsEasy)
                {
                    
                }
                
            }
            catch (Exception e)
            {
                //await UserDialogs.Instance.AlertAsync(AppResources.PleaseCheckInternetConnection, AppResources.Error);
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError
                });
            }

            if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("KenkoSingleExercisePage"))
            {
                Xamarin.Forms.MessagingCenter.Send<UpdatedWorkoutMessage>(new UpdatedWorkoutMessage() { OnlyRefresh = true }, "UpdatedWorkoutMessage");
            }
            else
            {
                // DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1352);
                var dt = DateTime.Now.AddMinutes(30);
                var timeSpan = new TimeSpan(0, dt.Hour, dt.Minute, 0);// DateTime.Now.AddMinutes(2) - DateTime.Now;////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleNotification("Dr. Muscle", "You forgot to save your workout!", timeSpan, 1352, NotificationInterval.Week, Convert.ToString(CurrentLog.Instance.CurrentWorkoutTemplate.Id));
            }

            if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("AllExercisePage") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("AllExercisesView") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("KenkoSingleExercisePage"))
            {
                
            }
            else
            {
                CurrentLog.Instance.IsFromEndExercise = false;

                CurrentLog.Instance.IsFromEndExercise = true;

                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(100);
                if (CurrentLog.Instance.IsFinishedWorkoutWithExercise)
                {
                    CurrentLog.Instance.IsFinishedWorkoutWithExercise = false;
                    MessagingCenter.Send<EndExercisePopup>(this, "EndExercisePopup");
                    return;
                }
                MessagingCenter.Send<LoadNextExercise>(new LoadNextExercise(), "LoadNextExercise");

            }
        }

        public decimal ComputeOneRM(decimal weight, int reps)
        {
            // Mayhew
            //return (100 * weight) / (decimal)(52.2 + 41.9 * Math.Exp(-0.055 * reps));
            // Epey
            return (decimal)(Constants.AppThemeConstants.Coeficent * reps) * weight + weight;
        }

        private void RefreshLocalized()
        {
            NextExerciseButton.Text = Resx.AppResources.NextExercise;
        }

        private async void NextExerciseButton_Clicked(object sender, EventArgs e)
        {
            //await PagesFactory.PopAsync();
            //await PagesFactory.PopAsync();
            //await PagesFactory.PopAsync();
            ShouldAnimate = false;
            if (!CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("DemoWorkoutPage"))
            {
                if (((App)Application.Current).UserWorkoutContexts.workouts != null) ;
                {
                    ((App)Application.Current).UserWorkoutContexts.workouts.LastWorkoutDate = DateTime.UtcNow;
                    ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                }
            }

            try
            {
                var setStyle = LocalDBManager.Instance.GetDBSetting("SetStyle").Value;
                if (LocalDBManager.Instance.GetDBReco("RReps" + CurrentLog.Instance.ExerciseLog.Exercice.Id + setStyle + "challenge")?.Value == "max" && !Config.FirstChallenge)
                {
                    Config.FirstChallenge = true;
                    var waitHandle = new EventWaitHandle(false
                        , EventResetMode.AutoReset);
                    var modalPage = new Views.GeneralPopup("EmptyStar.png", "Congratulations!", "You completed your first challenge 💪", "Continue", new Thickness(0, 0, 0, 0));
                    modalPage.Disappearing += (sender2, e2) =>
                    {
                        waitHandle.Set();
                    };
                    this.Opacity = 0.0;
                    this.BackgroundColor = Color.Transparent;
                    await PopupNavigation.Instance.PushAsync(modalPage);

                    await Task.Run(() => waitHandle.WaitOne());
                }
                else if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsDeload && !Config.FirstDeload)
                {
                    Config.FirstDeload = true;
                    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var modalPage = new Views.GeneralPopup("EmptyStar.png", "Congratulations!", "First deload done", "Continue", new Thickness(0, 0, 0, 0));
                    modalPage.Disappearing += (sender2, e2) =>
                    {
                        waitHandle.Set();
                    };
                    this.Opacity = 0.0;
                    await PopupNavigation.Instance.PushAsync(modalPage);

                    await Task.Run(() => waitHandle.WaitOne());
                }
                else if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsLightSession && !Config.FirstLightSession)
                {
                    Config.FirstLightSession = true;
                    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var modalPage = new Views.GeneralPopup("EmptyStar.png", "Congratulations!", "First light session done", "Continue", new Thickness(0, 0, 0, 0));
                    modalPage.Disappearing += (sender2, e2) =>
                    {
                        waitHandle.Set();
                    };
                    this.Opacity = 0.0;
                    await PopupNavigation.Instance.PushAsync(modalPage);

                    await Task.Run(() => waitHandle.WaitOne());
                }
            }
            catch (Exception ex)
            {

            }
            if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("AllExercisePage") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("AllExercisesView") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("KenkoSingleExercisePage"))
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await PagesFactory.PushAsyncWithoutBefore<Screens.Exercises.AllExercisePage>();
                }
                else
                    await PagesFactory.PushAsync<Screens.Exercises.AllExercisePage>();

                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync();
            }
            else { 
            
                if (PopupNavigation.Instance.PopupStack.Count>0)
                    await PopupNavigation.Instance.PopAsync();
               
            }
        }

        

        private string _formatter(double d)
        {
            return IndexToDateLabel.ContainsKey(d) ? IndexToDateLabel[d] : "";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            //if (Config.ShowWelcomePopUp5 == false)
            //{
            //    if (App.IsWelcomePopup5)
            //        return;
            //    App.IsWelcomePopup5 = true;
            //    ConfirmConfig ShowWelcomePopUp5 = new ConfirmConfig()
            //    {
            //        Message = "Your progress, new records, and key stats at a glance.",
            //        Title = "New record!",
            //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //        OkText = AppResources.GotIt,
            //        CancelText = AppResources.RemindMe,
            //        OnAction = async (bool ok) => {
            //            if (ok)
            //            {
            //                Config.ShowWelcomePopUp5 = true;
            //            }
            //            else
            //            {
            //                Config.ShowWelcomePopUp5 = false;
            //            }
            //        }
            //    };
            //    await Task.Delay(100);
            //    UserDialogs.Instance.Confirm(ShowWelcomePopUp5);
            //}
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            try
            {
                                                                                                           
                MyParticleCanvas.IsActive = false;
                MyParticleCanvas.IsRunning = false;
                MyParticleCanvas = null;

            }
            catch (Exception ex)
            {

            }
        }

        async void ShareWithFBButton_Clicked(object sender, EventArgs e)
        {
            //var imageByte = DependencyService.Get<IScreenshotService>().Capture();
            //imgCaptured.Source = ImageSource.FromStream(() => new MemoryStream(imageByte));
            // FacebookShareLinkContent linkContent = new FacebookShareLinkContent("Awesome team of developers, making the world a better place one project or plugin at the time!",
            //                                                    new Uri("http://www.github.com/crossgeeks"));
            // var ret = await CrossFacebookClient.Current.ShareAsync(linkContent);
            var _manager = DependencyService.Get<IFacebookManager>();
            var firstname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                FacebookUser result = await _manager.Login();

                if (result == null)
                {
                    UserDialogs.Instance.Alert(new AlertConfig()
                    {
                        Message = AppResources.AnErrorOccursWhenSigningIn,
                        Title = AppResources.UnableToLogIn,
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                    });
                    return;
                }
                DependencyService.Get<IFirebase>().LogEvent("fb_newrecord_share", "yes");

                _manager.ShareText(strFacebook, $"https://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=newrecord&utm_content={firstname}");
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    _manager.ShareText(strFacebook, $"https://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=newrecord&utm_content={firstname}");
                    DependencyService.Get<IFirebase>().LogEvent("fb_newrecord_share", "yes");
                });
            }
        }

        async void animate(Button grid)
        {
            try
            {
                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                var a = new Animation();
                a.Add(0, 0.5, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 1.0, 0.8, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                a.Add(0.5, 1, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 0.8, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
                a.Commit(grid, "animation", 16, 2000, null, (d, f) =>
                {
                    grid.Scale = 1.0;
                    System.Diagnostics.Debug.WriteLine("ANIMATION ALL");
                    if (ShouldAnimate)
                        animate(grid);
                });

            }
            catch (Exception ex)
            {

            }
        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            Device.OpenUri(new Uri("http://dr-muscle.com/deload/"));
        }
    }
}


