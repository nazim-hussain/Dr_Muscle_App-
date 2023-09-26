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

namespace DrMuscle.Screens.User.OnBoarding
{
    public partial class IntroPage3 : DrMusclePage
    {
        List<OneRMModel> _lastWorkoutLog = new List<OneRMModel>();
        private Dictionary<double, string> IndexToDateLabel = new Dictionary<double, string>();
        string strFacebook = "";
        bool ShouldAnimate = false;
        bool isEstimated = false;
        public IntroPage3()
        {
            InitializeComponent();

            LearnMoreButton.Clicked += (sender, e) => {
                Device.OpenUri(new Uri("http://drmuscleapp.com/news/deload/"));
            };
            RefreshLocalized();

            NextExerciseButton.Clicked += NextExerciseButton_Clicked;
            ShareWithFBButton.Clicked += ShareWithFBButton_Clicked;
            
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
            
            CurrentLog.Instance.IsFromEndExercise = false;
            
        }

        public override async void OnBeforeShow()
        {
            base.OnBeforeShow();
            DependencyService.Get<IFirebase>().SetScreenName("end_exercise_page");
            isEstimated = false;
            IconResultImage.Source = "up_arrow.png";
            lblResult1.IsVisible = true;
            lblResult2.IsVisible = true;
            lblResult21.IsVisible = true;
            lblResult3.IsVisible = true;
            lblResult4.IsVisible = true;
            lblResult6.IsVisible = true;
            lblLearnMore1.IsVisible = false;
            lblLearnMore2.IsVisible = false;
            plotView.Model = null;
            if (LocalDBManager.Instance.GetDBSetting("CustomExperience")?.Value != "an active, experienced lifter")
            {
                Title = "Push-up";
                _lastWorkoutLog = new List<OneRMModel>();
                OneRMModel oneRMModel1 = new OneRMModel()
                {
                    LastLogDate = DateTime.Now,
                    ExerciseId = 7673,
                    OneRM = new MultiUnityWeight((decimal)189, "kg"),
                    OneRMDate = DateTime.Now,
                    IsAllowDelete = true
                };
                OneRMModel oneRMModel2 = new OneRMModel()
                {
                    LastLogDate = DateTime.Now.AddDays(-7),
                    ExerciseId = 7673,
                    OneRM = new MultiUnityWeight((decimal)185.1, "kg"),
                    OneRMDate = DateTime.Now.AddDays(-7),
                    IsAllowDelete = true
                };
                OneRMModel oneRMModel3 = new OneRMModel()
                {
                    LastLogDate = DateTime.Now.AddDays(-14),
                    ExerciseId = 7673,
                    OneRM = new MultiUnityWeight((decimal)180.14, "kg"),
                    OneRMDate = DateTime.Now.AddDays(-14),
                    IsAllowDelete = true
                };
                _lastWorkoutLog.Add(oneRMModel1);
                _lastWorkoutLog.Add(oneRMModel2);
                _lastWorkoutLog.Add(oneRMModel3);
            }
            else
            {
                Title = "Bench Press";
                _lastWorkoutLog = new List<OneRMModel>();
                OneRMModel oneRMModel1 = new OneRMModel()
                {
                    LastLogDate = DateTime.Now,
                    ExerciseId = 11,
                    OneRM = new MultiUnityWeight((decimal)55.50, "kg"),
                    OneRMDate = DateTime.Now,
                    IsAllowDelete = true
                };
                OneRMModel oneRMModel2 = new OneRMModel()
                {
                    LastLogDate = DateTime.Now.AddDays(-7),
                    ExerciseId = 11,
                    OneRM = new MultiUnityWeight((decimal)53.68, "kg"),
                    OneRMDate = DateTime.Now.AddDays(-7),
                    IsAllowDelete = true
                };
                OneRMModel oneRMModel3 = new OneRMModel()
                {
                    LastLogDate = DateTime.Now.AddDays(-14),
                    ExerciseId = 11,
                    OneRM = new MultiUnityWeight((decimal)50, "kg"),
                    OneRMDate = DateTime.Now.AddDays(-14),
                    IsAllowDelete = true
                };
                _lastWorkoutLog.Add(oneRMModel1);
                _lastWorkoutLog.Add(oneRMModel2);
                _lastWorkoutLog.Add(oneRMModel3);
            }

            NextExerciseButton.Text = AppResources.NextExercise;
            try
            {

                var plotModel = new PlotModel
                {
                    Title = AppResources.MAXSTRENGTHESTIMATELAST3WORKOUTS.ToLower().FirstCharToUpper(),
                    //Subtitle = "for the 3 last workouts",
                    Background = OxyColors.Transparent,
                    PlotAreaBackground = OxyColors.Transparent,
                    TitleColor = OxyColor.Parse("#23253A"),
                    TitleFontSize = 15,
                    TitleFontWeight = FontWeights.Bold,
                    PlotAreaBorderColor = OxyColor.Parse("#23253A"),

                };

                double minY;
                double maxY;

                switch (LocalDBManager.Instance.GetDBSetting("massunit").Value)
                {
                    default:
                    case "kg":
                        minY = (double)(Math.Floor(_lastWorkoutLog.Min(o => o.OneRM.Kg) / 10) * 10) - 20;
                        maxY = (double)(Math.Ceiling(_lastWorkoutLog.Max(o => o.OneRM.Kg) / 10) * 10) + 20;
                        break;
                    case "lb":
                        minY = (double)(Math.Floor(_lastWorkoutLog.Min(o => o.OneRM.Lb) / 10) * 10) - 20;
                        maxY = (double)(Math.Ceiling(_lastWorkoutLog.Max(o => o.OneRM.Lb) / 10) * 10) + 20;
                        break;
                }

                LinearAxis yAxis = new LinearAxis { Position = AxisPosition.Left, Minimum = minY - 5, Maximum = maxY + 5, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColor.Parse("#23253A"), TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColor.Parse("#23253A") };
                LinearAxis xAxis = new LinearAxis { Position = AxisPosition.Bottom, AxislineColor = OxyColor.Parse("#23253A"), ExtraGridlineColor = OxyColor.Parse("#23253A"), MajorGridlineColor = OxyColor.Parse("#23253A"), MinorGridlineColor = OxyColor.Parse("#23253A"), TextColor = OxyColor.Parse("#23253A"), TicklineColor = OxyColor.Parse("#23253A"), TitleColor = OxyColor.Parse("#23253A"), MinimumMajorStep = 0.3, MinorStep = 0.5, MajorStep = 0.5 };
                xAxis.LabelFormatter = _formatter;

                xAxis.MinimumPadding = 0.05;
                xAxis.MaximumPadding = 0.1;
                xAxis.IsPanEnabled = false;
                xAxis.IsZoomEnabled = false;
                xAxis.Minimum = 0.5;
                xAxis.Maximum = 3.5;


                //xAxis.Minimum = DateTimeAxis.ToDouble(_lastWorkoutLog.Min(l => l.OneRMDate));
                //xAxis.Maximum = DateTimeAxis.ToDouble(_lastWorkoutLog.Max(l => l.OneRMDate));
                yAxis.IsAxisVisible = false;
                yAxis.IsPanEnabled = false;
                yAxis.IsZoomEnabled = false;

                IndexToDateLabel.Clear();
                IndexToDateLabel.Add(xAxis.Minimum, "");
                IndexToDateLabel.Add(xAxis.Maximum, "");

                plotModel.Axes.Add(yAxis);
                plotModel.Axes.Add(xAxis);


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
                    TextColor = OxyColor.Parse("#38418C"),

                };
                IndexToDateLabel.Clear();
                int i = 1;
                foreach (OneRMModel m in _lastWorkoutLog.OrderBy(w => w.OneRMDate))
                {
                    if (i == 2 && !m.IsAllowDelete)
                        isEstimated = true;
                    if (!m.IsAllowDelete)
                        yAxis.Minimum = -50;
                    switch (LocalDBManager.Instance.GetDBSetting("massunit").Value)
                    {
                        default:
                        case "kg":

                            if (!m.IsAllowDelete)
                                s1.Points.Add(new DataPoint(i, 0));
                            else
                                s1.Points.Add(new DataPoint(i, Convert.ToDouble(m.OneRM.Kg)));
                            break;
                        case "lb":
                            if (!m.IsAllowDelete)
                                s1.Points.Add(new DataPoint(i, 0));
                            else
                                s1.Points.Add(new DataPoint(i, Convert.ToDouble(m.OneRM.Lb)));
                            break;
                    }
                    IndexToDateLabel.Add(i, m.OneRMDate.ToLocalTime().ToString("MMM dd"));

                    i++;
                }

                plotModel.Series.Add(s1);
                plotView.Model = plotModel;

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
                    lblLearnMore2.IsVisible = false;
                    lblLearnMore1.IsVisible = false;
                    lblResult2.IsVisible = true;
                    StkResult2.IsVisible = true;
                    lblResult1.Text = AppResources.YourStrengthHasGoneUp;//string.Format("{0} {1}!", AppResources.Congratulations, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                    lblResult2.Text = "";

                    if (weight0 > weight1 && weight0 > weight2)
                    {
                        lblResult1.Text = "New strength record!";
                        lblResult2.Text = AppResources.YourStrengthHasGoneUpAndYouHaveSetaNewRecord;
                    }
                    else
                        lblResult2.IsVisible = false;
                    

                    lblResult21.Text = string.Format("{0} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1) :
                        Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Lb, 1),
                        LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();





                    try
                    {


                        lblResult3.Text = string.Format("{0} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                            Math.Round(_lastWorkoutLog.ElementAt(1).OneRM.Kg, 1) :
                            Math.Round(_lastWorkoutLog.ElementAt(1).OneRM.Lb, 1),
                            LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot();
                        lblResult3.IsVisible = true;
                        //    lblResult4.Text = string.Format("{0} {1}",Math.Round(Math.Round(weight0,1) - Math.Round(weight1,1),1),
                        //(LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs")).ReplaceWithDot();
                        lblResult33.Text = "Last workout";
                        lblResult44.Text = "Progress";
                        if (!isEstimated)
                            lblResult4.Text = string.Format(" {0}%", Math.Round(((Math.Round(weight0, 1) - Math.Round(weight1, 1)) * 100) / Math.Round(weight1, 1), 1)).ReplaceWithDot();
                        else
                        {
                            lblResult4.Text = string.Format("{0} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                            Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Kg, 1) :
                            Math.Round(_lastWorkoutLog.ElementAt(0).OneRM.Lb, 1),
                            LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs").ReplaceWithDot(); ;
                            lblResult3.Text = "0";
                            //lblResult33.Text = "";
                            //lblResult44.Text = "";
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
                        StkResult2.IsVisible = true;
                        lblResult1.IsVisible = true;

                        if (LocalDBManager.Instance.GetDBSetting("RecoDeload").Value == "false")
                        {
                            lblResult1.Text = "Your strength has gone down";//string.Format("{0} {1}!", AppResources.Attention ,LocalDBManager.Instance.GetDBSetting("firstname").Value);
                            lblResult2.Text = "";
                            lblResult2.IsVisible = false;
                            StkResult2.IsVisible = false;
                            StkResult6.IsVisible = true;

                            lblResult6.Text = string.Format("{0} {1}.", AppResources.IWillLowerYourWeightsToHelpYouRecoverTheNextTimeYou, Title);
                            lblLearnMore2.IsVisible = true;
                            lblLearnMore1.IsVisible = false;
                            LearnMoreButton.IsVisible = false;
                            IconResultImage.Source = "down_arrow.png";
                        }
                        //If 2e workout au retour de deload...
                        else
                        {
                            lblResult1.Text = "Deload successful";//string.Format(AppResources.DeloadSuccessful);
                            lblResult2.Text = $"{AppResources.IHaveLowedYourWeightsToHelpYouRecoverInTheShortTermAndProgressLongTerm}";
                            lblResult6.IsVisible = false;
                            StkResult6.IsVisible = false;
                            lblLearnMore1.IsVisible = true;
                            lblLearnMore2.IsVisible = false;
                            LearnMoreButton.IsVisible = false;
                            IconResultImage.Source = "green.png";

                        }

                    }
                    //else if égal
                    else if (weight0 == weight1)
                    {
                        IconResultImage.Source = "green.png";
                        lblResult1.Text = "Lift successful";// string.Format("{0} {1}", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                        lblResult2.Text = "Your strength has not changed, but your volume is going in the right direction. This is good.";// "Your strength has not changed, but you have done more sets. This is good.";
                        lblResult2.IsVisible = true;
                        StkResult2.IsVisible = true;
                        lblResult6.IsVisible = false;
                        StkResult6.IsVisible = false;
                        lblLearnMore2.IsVisible = false;
                        lblLearnMore1.IsVisible = false;
                        LearnMoreButton.IsVisible = false;
                    }
                    //else if légère diminution
                    else if (weight0 >= (weight1 * (decimal)0.98) && weight0 <= weight1 || weight0 < (weight1 * (decimal)0.98) && weight0 >= (weight1 - 2))
                    {
                        IconResultImage.Source = "green.png";
                        lblResult1.Text = "Lift successful";//string.Format("{0} {1}", AppResources.WellDone, LocalDBManager.Instance.GetDBSetting("firstname").Value);
                        lblResult2.Text = "Your strength has decreased slightly, but your volume is going in the right direction. Overall, this is progress.";
                        lblResult6.IsVisible = false;
                        StkResult6.IsVisible = false;
                        lblResult2.IsVisible = true;
                        StkResult2.IsVisible = true;
                        lblLearnMore2.IsVisible = false;
                        lblLearnMore1.IsVisible = false;
                        LearnMoreButton.IsVisible = false;
                    }
                    //Sinon (if pas deload...)
                    else
                    {
                        IconResultImage.Source = "up_arrow.png";
                        //Set button text here:

                        strFacebook = string.Format("{0} {1} {2} {3:f} {4}{5}", "I just smashed a new record!", Title, "is now", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ?
                        _lastWorkoutLog.ElementAt(0).OneRM.Kg :
                        _lastWorkoutLog.ElementAt(0).OneRM.Lb, LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? "kg" : "lbs", ". I train using Dr. Muscle. Get your invitation at:");
                        lblResult1.IsVisible = true;

                        lblResult6.IsVisible = false;
                        StkResult6.IsVisible = false;
                        LearnMoreButton.IsVisible = false;
                    }
                    if (strFacebook == "")
                        FbShare.IsVisible = false;
                    else
                        FbShare.IsVisible = true;
                }
                catch (Exception e)
                {
                    //await UserDialogs.Instance.AlertAsync(AppResources.PleaseCheckInternetConnection, AppResources.Error);
                    ConnectionErrorPopup();
                }

            }
            catch (Exception) { }
        }


        private string _formatter(double d)
        {
            return IndexToDateLabel.ContainsKey(d) ? IndexToDateLabel[d] : "";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (Device.RuntimePlatform.Equals(Device.iOS))
                stackBottomContent.Margin = new Thickness(3, 15, 3, App.StatusBarHeight - 5);
            else
                stackBottomContent.Margin = new Thickness(3, 15, 3, 0);
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
        


        async void MoveToBack_Tapped(System.Object sender, System.EventArgs e)
        {
            App.IsIntroBack = true;
            await Navigation.PopModalAsync(false);
            var page = PagesFactory.GetPage<MainOnboardingPage>();
            page.OnBeforeShow();
            //PagesFactory.PopToBoardingPage<MainOnboardingPage>();
        }
        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            Device.OpenUri(new Uri("http://dr-muscle.com/deload/"));
        }
    }
}
