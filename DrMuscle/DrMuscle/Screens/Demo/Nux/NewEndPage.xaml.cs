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
using DrMuscle.Screens.Exercises;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;
using OxyPlot.Annotations;
using DrMuscle.Screens.User;
using DrMuscle.Message;
using System.Threading;
using Microcharts;
using SkiaSharp;

namespace DrMuscle.Screens.Demo
{
    public partial class NewEndPage : DrMusclePage
    {
        List<OneRMModel> _lastWorkoutLog = new List<OneRMModel>();
        private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        string strFacebook = "";
        bool ShouldAnimate = false;
        bool isEstimated = false;
        public NewEndPage()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
            LearnMoreButton.Clicked += (sender, e) => {
                Device.OpenUri(new Uri("http://drmuscleapp.com/news/deload/"));
            };
            RefreshLocalized();

            NextExerciseButton.Clicked += NextExerciseButton_Clicked;
           // ShareWithFBButton.Clicked += ShareWithFBButton_Clicked;
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) => {
                RefreshLocalized();
            });
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                ConfirmConfig exitPopUp = new ConfirmConfig()
                {

                    Title = AppResources.Exit,
                    Message = AppResources.AreYouSureYouWantToExit,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.Yes,
                    CancelText = AppResources.No,
                };

                var result = await UserDialogs.Instance.ConfirmAsync(exitPopUp);
                if (result)
                {
                    var kill = DependencyService.Get<IKillAppService>();
                    kill.ExitApp();
                }
            });
            return true;
        }

        public decimal ComputeOneRM(decimal weight, int reps)
        {
            // Mayhew
            //return (100 * weight) / (decimal)(52.2 + 41.9 * Math.Exp(-0.055 * reps));
            // Epey
            return (decimal)(AppThemeConstants.Coeficent * reps) * weight + weight;
        }

        private void RefreshLocalized()
        {
            LearnMoreButton.Text = AppResources.LearnMoreAboutDeloads;
            NextExerciseButton.Text = AppResources.NextExercise;
        }

        private async void NextExerciseButton_Clicked(object sender, EventArgs e)
        {
            //await PagesFactory.PopAsync();
            //await PagesFactory.PopAsync();
            //await PagesFactory.PopAsync();
            ShouldAnimate = false;
            NextExerciseButton.Effects.Clear();
            

            var modalPage1 = new Views.WelcomeAIOverlay();
            PopupNavigation.Instance.PushAsync(modalPage1);
            modalPage1.SetDetails("", CurrentLog.Instance.AiDescription);
        }

        public override async void OnBeforeShow()
        {
            base.OnBeforeShow();
            if (CurrentLog.Instance.IsDemoPopingOut)
                return;

            
                DependencyService.Get<IFirebase>().SetScreenName("end_demo_exercise_page");
                isEstimated = false;
                IconResultImage.Source = "up_arrow.png";
                lblResult1.IsVisible = true;
                lblResult2.IsVisible = true;
                lblResult21.IsVisible = true;
                lblResult3.IsVisible = true;
                lblResult4.IsVisible = true;
                lblResult6.IsVisible = true;
                
                DrMuscle.Effects.TooltipEffect.SetText(NextExerciseButton, "Tap here to continue");
                DrMuscle.Effects.TooltipEffect.SetBackgroundColor(NextExerciseButton, AppThemeConstants.BlueColor);
                DrMuscle.Effects.TooltipEffect.SetTextColor(NextExerciseButton, Color.White);
                DrMuscle.Effects.TooltipEffect.SetPosition(NextExerciseButton, DrMuscle.Effects.TooltipPosition.Top);
                DrMuscle.Effects.TooltipEffect.SetHasTooltip(NextExerciseButton, true);
                
                Title = CurrentLog.Instance.ExerciseLog.Exercice.Label;
            if (Device.RuntimePlatform.Equals(Device.Android))
                await Task.Delay(400);

            if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("NewDemoPage") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("DemoChallengePage"))
                {
                    NextExerciseButton.Text = "Next";
                    //if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("NewDemoPage2"))
                    //    NextExerciseButton.Text = AppResources.NextExercise;
                    ShouldAnimate = true;
                    animate(NextExerciseButton);
                }
                else
                    NextExerciseButton.Text = AppResources.NextExercise;
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
                                LastLogDate = DateTime.Now,
                                IsAllowDelete = true
                            });
                        }
                    }

                //var plotModel = new PlotModel
                //{
                //    Title = AppResources.MAXSTRENGTHESTIMATELAST3WORKOUTS.ToLower().FirstCharToUpper(),
                //    //Subtitle = "for the 3 last workouts",
                //    Background = OxyColors.Transparent,
                //    PlotAreaBackground = OxyColors.Transparent,
                //    TitleColor = OxyColor.Parse("#23253A"),
                //    TitleFontSize = 14,
                //    TitleFontWeight = FontWeights.Bold,
                //    PlotAreaBorderColor = OxyColor.Parse("#23253A"),
                //    LegendLineSpacing = 5,
                //};

                //double minY;
                //double maxY;

                var chartSerie = new ChartSerie() { Name = AppResources.MAXSTRENGTHESTIMATELAST3WORKOUTS.ToLower().FirstCharToUpper(), Color = SKColor.Parse("#38418C") };
                List<ChartSerie> chartSeries = new List<ChartSerie>();

                List<ChartEntry> entries = new List<ChartEntry>();


               

                    var s1 = new LineSeries()
                    {
                        Color = OxyColor.Parse("#38418C"),
                        MarkerType = MarkerType.Circle,
                        MarkerSize = 6,
                        MarkerStroke = OxyColor.Parse("#38418C"),
                        MarkerFill = OxyColor.Parse("#38418C"),
                        MarkerStrokeThickness = 1,
                        LabelFormatString = "{1:0}",
                        FontSize = 15,
                        TextColor = OxyColor.Parse("#38418C")
                    };

                    int i = 1;
                    IndexToDateLabel.Clear();
                    foreach (OneRMModel m in _lastWorkoutLog.OrderBy(w => w.OneRMDate))
                    {
                        if (i == 2 && !m.IsAllowDelete)
                            isEstimated = true;
                        
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
                        IndexToDateLabel.Add(i, m.OneRMDate.ToLocalTime().ToString("MMM dd"));

                        i++;
                    }

                    //plotModel.Series.Add(s1);
                    //plotView.Model = plotModel;


                chartSerie.Entries = entries;
                chartSeries.Add(chartSerie);

                chartView.Chart = new LineChart
                {
                    LabelOrientation = Orientation.Vertical,
                    ValueLabelOrientation = Orientation.Vertical,
                    LabelTextSize = 18,
                    ValueLabelTextSize = 18,
                    SerieLabelTextSize = 12,
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
                    try
                    {


                        lblResult2.IsVisible = true;
                        lblResult1.Text = AppResources.YourStrengthHasGoneUp;//string.Format("{0} {1}!", AppResources.Congratulations, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                        lblResult2.Text = "";

                        if (weight0 > weight1 && weight0 > weight2)
                        {
                            lblResult1.Text = "New strength record!";
                            lblResult2.Text = AppResources.YourStrengthHasGoneUpAndYouHaveSetaNewRecord;
                        }
                        else
                            lblResult2.IsVisible = false;
                        //lblResult21.Text = string.Format("{0} {1:f} {2}", AppResources.TodaysMaxEstimate, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        //    _lastWorkoutLog.ElementAt(0).OneRM.Kg :
                        //    _lastWorkoutLog.ElementAt(0).OneRM.Lb,
                        //    LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();
                        //lblResult3.Text = string.Format("{0} {1:f} {2}", AppResources.PreviousMaxEstimate, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        //    _lastWorkoutLog.ElementAt(1).OneRM.Kg :
                        //    _lastWorkoutLog.ElementAt(1).OneRM.Lb,
                        //    LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();
                        //lblResult3.IsVisible = true;
                        //lblResult4.Text = string.Format("{0}: {1:f} {2}", AppResources.Progress, weight0 - weight1,
                        //    (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs")).ReplaceWithDot();

                        lblResult21.Text = string.Format("{0} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                            Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1) :
                            Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Lb, 1),
                            LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();
                        lblResult3.Text = string.Format("{0} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                            Math.Round(_lastWorkoutLog.ElementAt(1).OneRM.Kg, 1) :
                            Math.Round(_lastWorkoutLog.ElementAt(1).OneRM.Lb, 1),
                            LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();
                        lblResult3.IsVisible = true;
                        //    lblResult4.Text = string.Format("{0} {1}", Math.Round(Math.Round(weight0,1) - Math.Round(weight1,1),1),
                        //(LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs")).ReplaceWithDot();

                        if (!isEstimated)
                            lblResult4.Text = string.Format(" {0}%", Math.Round(((Math.Round(weight0, 1) - Math.Round(weight1, 1)) * 100) / Math.Round(weight1, 1), 1)).ReplaceWithDot();
                        else
                        {
                            lblResult4.Text = "N/A";
                            lblResult3.Text = "N/A";
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    strFacebook = "";
                    //If deload...

                    if (weight0 < (weight1 * (decimal)0.98) && weight0 < (weight1 - 2))
                    {
                        lblResult2.IsVisible = true;
                        if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsLightSession)
                        {
                            lblResult1.Text = string.Format("{0} {1}", AppResources.WellDone, "");
                            lblResult2.Text = "Your light session went as planned";
                            lblResult6.Text = string.Format("{0} {1}", "Your weights will go up next time you", CurrentLog.Instance.ExerciseLog.Exercice.Label);
                            LearnMoreButton.IsVisible = true;
                            IconResultImage.Source = "down_arrow.png";
                        }
                        else if (LocalDBManager.Instance.GetDBSetting("RecoDeload").Value == "false")
                        {
                            lblResult1.Text = "Your strength has gone down";//string.Format("{0} {1}!", AppResources.Attention ,LocalDBManager.Instance.GetDBSetting("firstname").Value);
                            lblResult2.Text = "";
                            lblResult6.Text = string.Format("{0} {1}", AppResources.IWillLowerYourWeightsToHelpYouRecoverTheNextTimeYou, CurrentLog.Instance.ExerciseLog.Exercice.Label);
                            LearnMoreButton.IsVisible = true;
                            lblResult2.IsVisible = false;
                            IconResultImage.Source = "down_arrow.png";
                        }
                        //If 2e workout au retour de deload...
                        else
                        {
                            lblResult1.Text = "Deload successful";//string.Format(AppResources.DeloadSuccessful);
                            lblResult2.Text = AppResources.IHaveLowedYourWeightsToHelpYouRecoverInTheShortTermAndProgressLongTerm;
                            lblResult6.IsVisible = false;
                            IconResultImage.Source = "green.png";

                            LearnMoreButton.IsVisible = true;
                        }
                        if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsEasy)
                        {
                            lblResult1.Text = $"{AppResources.WellDone} {""}";
                            lblResult2.Text = AppResources.IMadeThisExericseEasyToHelpYouRecoverTheNextTimeYouTrain; //"I made this exericse easy to help you recover. The next time you train, you'll be in a great position to smash a new record.";
                        }
                    }
                    //else if égal
                    else if (weight0 == weight1)
                    {
                        IconResultImage.Source = "green.png";
                        lblResult1.Text = "Lift successful";// string.Format("{0} {1}", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                        lblResult2.Text = "Your strength has not changed, but your volume is going in the right direction. This is good.";// "Your strength has not changed, but you have done more sets. This is good.";
                        lblResult6.IsVisible = false;
                        lblResult2.IsVisible = true;
                        LearnMoreButton.IsVisible = false;
                    }
                    //else if légère diminution
                    else if (weight0 >= (weight1 * (decimal)0.98) && weight0 <= weight1 || weight0 < (weight1 * (decimal)0.98) && weight0 >= (weight1 - 2))
                    {
                        IconResultImage.Source = "green.png";
                        lblResult1.Text = "Lift successful";//string.Format("{0} {1}", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                        lblResult2.Text = "Your strength has decreased slightly, but your volume is going in the right direction. Overall, this is progress.";
                        lblResult6.IsVisible = false;
                        lblResult2.IsVisible = true;
                        LearnMoreButton.IsVisible = false;
                    }
                    //Sinon (if pas deload...)
                    else
                    {
                        IconResultImage.Source = "up_arrow.png";
                        //Set button text here:

                        strFacebook = string.Format("{0} {1} {2} {3:f} {4}{5}", "I just smashed a new record!", CurrentLog.Instance.ExerciseLog.Exercice.Label, "is now", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        _lastWorkoutLog.ElementAt(0).OneRM.Kg :
                        _lastWorkoutLog.ElementAt(0).OneRM.Lb, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", ". I train using Dr. Muscle. Get your invitation at:");
                        lblResult1.IsVisible = true;
                        //lblResult2.IsVisible = true;
                        lblResult6.IsVisible = false;
                        LearnMoreButton.IsVisible = false;
                    }
                    //if (strFacebook == "")
                    //    FbShare.IsVisible = false;
                    //else
                    //    FbShare.IsVisible = true;
                    if (CurrentLog.Instance.RecommendationsByExercise[CurrentLog.Instance.ExerciseLog.Exercice.Id].IsEasy)
                    {
                        lblResult1.Text = $"{AppResources.WellDone} {""}";
                        lblResult2.Text = AppResources.IMadeThisExericseEasyToHelpYouRecoverTheNextTimeYouTrain;
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
           
        }

        private string _formatter(double d)
        {
            return IndexToDateLabel.ContainsKey(d) ? IndexToDateLabel[d] : "";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (CurrentLog.Instance.IsDemoPopingOut)
                return;
            //if (Device.RuntimePlatform.Equals(Device.iOS))
                DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(NextExerciseButton, true);
            if (CurrentLog.Instance.IsSettingsVisited)
            {
                CurrentLog.Instance.IsSettingsVisited = false;
                MoveToHomepage();
                return;
            }
            if (Config.ShowWelcomePopUp5 == false)
            {
                if (App.IsWelcomePopup5)
                    return;
                App.IsWelcomePopup5 = true;
                ConfirmConfig ShowWelcomePopUp5 = new ConfirmConfig()
                {
                    Message = "Your progress, new records, and key stats at a glance.",
                    Title = "New record!",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = AppResources.GotIt,
                    CancelText = AppResources.RemindMe,
                    OnAction = async (bool ok) => {
                        if (ok)
                        {
                            Config.ShowWelcomePopUp5 = true;
                        }
                        else
                        {
                            Config.ShowWelcomePopUp5 = false;
                        }
                    }
                };
                await Task.Delay(100);
                UserDialogs.Instance.Confirm(ShowWelcomePopUp5);
            }
        }

        private async void MoveToHomepage()
        {
            await Task.Delay(300);
            if (Device.RuntimePlatform.Equals(Device.Android))
            {
                //App.IsSettingsPopup = true;
                //CurrentLog.Instance.IsFromExperienceLifter = true;
                //await PagesFactory.PopToRootThenPushAsync<SettingsPage>(true);
                App.IsDemoProgress = false;
                App.IsWelcomeBack = true;
                App.IsNewUser = true;
                LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                CurrentLog.Instance.Exercise1RM.Clear();
                //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await PagesFactory.PopToRootAsync(true);
                });
                MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            }
            else
            {

                App.IsDemoProgress = false;
                App.IsWelcomeBack = true;
                App.IsNewUser = true;
                LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                CurrentLog.Instance.Exercise1RM.Clear();
                //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                await PagesFactory.PopToRootMoveAsync(true);
                //App.IsSettingsPopup = true;
                //CurrentLog.Instance.IsFromExperienceLifter = true;
                //await PagesFactory.PushMoveAsync<SettingsPage>(true);
                MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            }
            return;
            long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
            long programId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);

            var upi = new GetUserProgramInfoResponseModel()
            {
                NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
            };
            if (upi != null)
            {
                WorkoutTemplateModel nextWorkout = upi.NextWorkoutTemplate;
                if (upi.NextWorkoutTemplate.Exercises == null || upi.NextWorkoutTemplate.Exercises.Count() == 0)
                {
                    try
                    {
                        nextWorkout = await DrMuscleRestClient.Instance.GetUserCustomizedCurrentWorkout(workoutTemplateId);
                        //nextWorkout = w.Workouts.First(ww => ww.Id == upi.NextWorkoutTemplate.Id);
                    }
                    catch (Exception ex)
                    {
                        await UserDialogs.Instance.AlertAsync(new AlertConfig()
                        {
                            AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            Message = AppResources.PleaseCheckInternetConnection,
                            Title = AppResources.ConnectionError
                        });
                        return;
                    }

                }
                if (nextWorkout != null)
                {
                    CurrentLog.Instance.CurrentWorkoutTemplate = nextWorkout;
                    CurrentLog.Instance.WorkoutTemplateCurrentExercise = nextWorkout.Exercises.First();
                    CurrentLog.Instance.WorkoutStarted = true;
                    if (Device.RuntimePlatform.Equals(Device.Android))
                    {
                        App.IsDemoProgress = false;
                        App.IsWelcomeBack = true;
                        App.IsNewUser = true;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                        CurrentLog.Instance.Exercise1RM.Clear();
                        //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await PagesFactory.PopToRootAsync(true);
                        });
                        MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
                    }
                    else
                    {

                        App.IsDemoProgress = false;
                        App.IsWelcomeBack = true;
                        App.IsNewUser = true;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                        CurrentLog.Instance.Exercise1RM.Clear();
                        //await PopupNavigation.Instance.PushAsync(new ReminderPopup());
                        await PagesFactory.PopToRootMoveAsync(true);
                        MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
                    }

                }
                else
                {
                    await PagesFactory.PopToRootAsync(true);
                }
                App.IsSettingsPopup = false;
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
                _manager.ShareText(strFacebook, $"https://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=newrecord&utm_content={firstname}");
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    _manager.ShareText(strFacebook, $"https://dr-muscle.com/discount/?utm_source=app&utm_medium=share&utm_campaign=newrecord&utm_content={firstname}");
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

    }
}
