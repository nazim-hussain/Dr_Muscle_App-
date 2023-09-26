using System;
using DrMuscle.Layout;
using SlideOverKit;
using Xamarin.Forms;
using System.Diagnostics;
using Microsoft.AppCenter.Analytics;
using Acr.UserDialogs;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrMuscleWebApiSharedModel;
using DrMuscle.Dependencies;
using DrMuscle.Screens.Exercises;
using DrMuscle.Helpers;
using System.Globalization;
using DrMuscle.Resx;
using Microsoft.AppCenter.Crashes;
using System.Collections.Generic;
using System.Linq;
using DrMuscle.Screens.User;
using DrMuscle.Services;
using Rg.Plugins.Popup.Services;
using DrMuscle.Views;
using DrMuscle.Constants;
using System.Net.NetworkInformation;
using Plugin.Connectivity;

namespace DrMuscle.Layout
{
    public class DrMusclePage : MenuContainerPage
    {
        public ToolbarItem timerToolbarItem;
        private ToolbarItem generalToolbarItem;
        protected bool HasSlideMenu = true;
        public DrMusclePage()
        {


            BackgroundColor = Color.White;

            timerToolbarItem = new ToolbarItem(Timer.Instance.State == "STOPPED" ? "" : Timer.Instance.Remaining.ToString(), Timer.Instance.State == "STOPPED" ? "stopwatch.png" : "", SlideTimerAction, ToolbarItemOrder.Primary, 0);
            generalToolbarItem = new ToolbarItem("Buy", "menu.png", SlideGeneralAction, ToolbarItemOrder.Primary, 0);

            if (HasSlideMenu)
            {

                //if (App.IsMainPage < 4 && Device.RuntimePlatform.Equals(Device.Android))
                //{
                //    NavigationPage.SetHasNavigationBar(this, false);
                    App.IsMainPage += 1;
                //}
                SlideMenu = new RightSideMasterPage();
                
                // this.ToolbarItems.Add(timerToolbarItem);
                //if (LocalDBManager.Instance.GetDBSetting("email") == null)
                //    this.ToolbarItems.Add(generalToolbarItem);
            }

            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            Timer.Instance.OnTimerStop += OnTimerStop;

        }

        protected void OnTimerDone()
        {
            try
            {
                try
                {

                    var navigation = (((MainTabbedPage)((NoAnimationNavigationPage)Application.Current.MainPage).CurrentPage).CurrentPage.Navigation);
                    if (navigation.NavigationStack.Last() is ChatPage || navigation.NavigationStack.Last() is GroupChatPage || navigation.NavigationStack.Last() is SupportPage)
                       return;

                }
                catch (Exception ex)
                {

                }
                if (ToolbarItems.Count > 0)
                {
                    var index = 0;
                    if (this.ToolbarItems.Count == 2)
                    {
                        index = 1;
                    }
                    this.ToolbarItems.RemoveAt(index);
                    timerToolbarItem = new ToolbarItem("", "stopwatch.png", SlideTimerAction, ToolbarItemOrder.Primary, 0);
                    this.ToolbarItems.Insert(index, timerToolbarItem);
                }
            }
            catch (Exception ex)
            {

            }

        }

        protected async void ConnectionErrorPopup()
        {
            await UserDialogs.Instance.AlertAsync(new AlertConfig()
            {
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                Message = AppResources.PleaseCheckInternetConnection,
                Title = AppResources.ConnectionError,
                OkText = "Try again"
            });
        }
        

        public async Task<bool> IsGoogleReachable()
        {
            try
            {
                if (Device.RuntimePlatform.Equals(Device.iOS))
                    return await CrossConnectivity.Current.IsReachable("https://www.google.com/");

                return true;
            }
            catch (Exception)
            {
                // An exception occurred (e.g., no internet connection)
                return false;
            }
        }

        protected void OnTimerStop()
        {
            try
            {
                
                if (ToolbarItems.Count > 0)
                {
                    var index = 0;
                    if (this.ToolbarItems.Count == 2)
                    {
                        index = 1;
                    }
                    this.ToolbarItems.RemoveAt(index);
                    timerToolbarItem = new ToolbarItem("", "stopwatch.png", SlideTimerAction, ToolbarItemOrder.Primary, 0);
                    this.ToolbarItems.Insert(index, timerToolbarItem);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public List<string> GetWorkoutCoverImageArray()
        {
            List<String> strList = new List<string>();
            //Add cover image name here
            strList.Add("WorkoutBackground");
            strList.Add("top");
            strList.Add("top2");
            strList.Add("SettingsBackground");
            strList.Add("middle");
            strList.Add("middle2");
            strList.Add("ExerciseBackground");
            strList.Add("bottom");
            strList.Add("bottom2");
            return strList;
        }
        public List<List<string>> GetTipsArray()
        {
            List<List<string>> _tipsArray = new List<List<string>>();
            int lowReps = 0;
            int highreps = 0;
            try
            {
                lowReps = int.Parse(LocalDBManager.Instance.GetDBSetting("repsminimum").Value);
                highreps = int.Parse(LocalDBManager.Instance.GetDBSetting("repsmaximum").Value);
            }
            catch (Exception)
            {

            }
            var result = "";
            if (lowReps >= 5 && highreps <= 12)
                result = "This helps you build muscle and strength.";
            else if (lowReps >= 8 && highreps <= 15)
                result = "This helps you build muscle and burn fat.";
            else if (lowReps >= 5 && highreps <= 15)
                result = "This helps you build muscle.";
            else if (lowReps >= 12 && highreps <= 20)
                result = "This helps you burn fat.";
            else if (highreps >= 16)
                result = "This helps you build muscle and burn fat.";
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("Demoreprange") != null)
                {
                    if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscle")
                    {
                        result = "This helps you build muscle.";
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscleBurnFat")
                    {
                        result = "This helps you build muscle and burn fat.";
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                    {
                        result = "This helps you burn fat.";
                    }
                }
            }
            _tipsArray = new List<List<string>>();

            //Second tip
            bool IsKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";
            decimal weeks = 0;
            string Gender = LocalDBManager.Instance.GetDBSetting("gender").Value.Trim();
            var creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
            if (creationDate != null)
            {
                weeks = (int)(DateTime.Now - creationDate).TotalDays / 7;
            }
            decimal gainDouble = 0;
            decimal weight1 = 0;
            if (LocalDBManager.Instance.GetDBSetting("BodyWeight") != null)
            {
                weight1 = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture);
                //grams = Math.Round(weight1 * (decimal)2.4);
            }
            string weightText = "";
            try
            {//string.Format("{0:0.#}"
                var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight")?.Value?.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                var weights = new MultiUnityWeight(value, "kg");
                weightText = string.Format("{0:0.#} {1}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? Math.Round(weights.Kg, 2) : Math.Round(weights.Lb, 2), IsKg ? "kg" : "lbs");
            }
            catch (Exception ex)
            {

            }
            if (Gender == "Man")
            {
                if (weeks <= 18)
                    gainDouble = ((decimal)0.015 - (decimal)0.000096899 * weeks) * weight1;
                else if (weeks > 18 && weeks <= 42)
                    gainDouble = ((decimal)0.011101 - (decimal)0.000053368 * weeks) * weight1;
                else if (weeks > 42)
                    gainDouble = (decimal)0.00188 * weight1;
            }
            else
            {
                if (weeks <= 18)
                    gainDouble = (((decimal)0.015 - (decimal)0.000096899 * weeks) * weight1) / 2;
                else if (weeks > 18 && weeks <= 42)
                    gainDouble = (((decimal)0.011101 - (decimal)0.000053368 * weeks) * weight1) / 2;
                else if (weeks > 42)
                    gainDouble = ((decimal)0.00188 * weight1) / 2;
            }
            string gain = IsKg ? $"{Math.Round(gainDouble, 2)} kg" : $"{Math.Round(new MultiUnityWeight(gainDouble, WeightUnities.kg).Lb, 2)} lbs";
            var weekText = weeks <= 1 ? "week" : "weeks";
            var menText = Gender == "Man" ? "Men" : "Women";



            var dict = new Dictionary<string, string>() { };
            dict.Add("", "");




            _tipsArray = new List<List<string>>();
            //first
            var lst = new List<string>();
            lst.Add("Equipment profiles");
            lst.Add("Training in multiple locations? Create equipment profiles (in Settings). You'll get to choose when you open the app.");
            lst.Add("false");
            lst.Add("false");
            //_tipsArray.Add("Equipment profiles" "Training in multiple locations? Create equipment profiles (in Settings). You'll get to choose when you open the app." );
            _tipsArray.Add(lst);
            //Second
            if (highreps - lowReps > 5)
            {
                lst = new List<string>();
                lst.Add("Rep range");
                lst.Add($"Your rep range is set to {lowReps}-{highreps}. Nice work! Scientists found you progress faster when you change reps often.");
                lst.Add("false");
                lst.Add("false");
                _tipsArray.Add(lst);
                //_tipsArray.Add("Rep range", "Your rep range is set to 12-18. Nice work! Scientists found you progress faster when you change reps often.");
            }
            else if (highreps - lowReps <= 5)
            {
                lst = new List<string>();
                lst.Add("Rep range");
                lst.Add($"Your rep range is set to {lowReps}-{highreps}. Studies have shown you progress faster when you change reps often. Consider increasing your range.");
                lst.Add("false");
                lst.Add("false");
                _tipsArray.Add(lst);
                //_tipsArray.Add("Rep range", "Your rep range is set to 6-8. Consider increasing your range. Studies have shown you progress faster when you change reps often.");
            }
            //third
            //_tipsArray.Add($"{gain} lbs of muscle", $"You've been working out with Dr. Muscle for {weeks} {weekText}. So, you should be gaining about {gain} of muscle a month. If you're gaining less than that, eat more. If you're gaining more, eat less. You're probably gaining fat.");

            lst = new List<string>();
            lst.Add($"{gain} of muscle");
            lst.Add($"You've been working out for {weeks} {weekText}. So, you should be gaining ~{gain} of muscle a month.");
            lst.Add("true");
            lst.Add("false");
            _tipsArray.Add(lst);
            //Fourth
            var protein = "";
            if (LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg")
            {
                if (result.Contains("build muscle and burn fat") || result.Contains("burn fat"))
                    protein = Math.Round(new MultiUnityWeight(weight1, "kg").Kg * (decimal)1.8) + "" ;
                else
                    protein = Math.Round(new MultiUnityWeight(weight1, "kg").Kg * (decimal)1.6) + "";
            }
            else
            {
                if (result.Contains("build muscle and burn fat") || result.Contains("burn fat"))
                    protein = Math.Round(new MultiUnityWeight(weight1, "kg").Lb * (decimal)0.8) + "";
                else
                    protein = Math.Round(new MultiUnityWeight(weight1, "kg").Lb * (decimal)0.7) + "";
            }

            if (result.Contains("build muscle and burn fat"))
            {
                lst = new List<string>();
                lst.Add($"{protein} g of protein");
                lst.Add($"You weigh {weightText}—eat {protein} grams of protein a day for faster results.");
                lst.Add("true");
                lst.Add("false");
                _tipsArray.Add(lst);
            }
                //_tipsArray.Add($"{Math.Round(weight1 * (decimal)2.4)} g of protein", $"To build muscle and burn fat faster, scientists recommend eating plenty of protein. You weigh {weightText}, so {Math.Round(weight1 * (decimal)2.4)} grams a day should be optimal for you. Good sources are beef, chicken, fish, and eggs. Vegetarians options include tofu, beans, quinoa, and nuts.");

            else if (result.Contains("build muscle"))
            {
                lst = new List<string>();
                lst.Add($"{protein} g of protein");
                lst.Add($"You weigh {weightText}—eat {protein} grams of protein a day for faster results.");
                lst.Add("true");
                lst.Add("false");
                _tipsArray.Add(lst);
            }
                //_tipsArray.Add($"{Math.Round(weight1 * (decimal)2.3)} g of protein", $"To build muscle faster, scientists recommend eating plenty of protein. You weigh {weightText}, so {Math.Round(weight1 * (decimal)2.3)} grams a day should be optimal for you. Good sources are beef, chicken, fish, and eggs.");
            else if (result.Contains("burn fat"))
            {
                lst = new List<string>();
                lst.Add($"{protein} g of protein");
                lst.Add($"You weigh {weightText}—eat {protein} grams of protein a day for faster results. Visit Learn tab for more.");
                lst.Add("true");
                lst.Add("false");
                _tipsArray.Add(lst);
            }
                //_tipsArray.Add($"{Math.Round(weight1 * (decimal)2.4)} g of protein",$"To burn fat faster, scientists recommend eating plenty of protein. You weigh {weightText}, so {Math.Round(weight1 * (decimal)2.4)} grams a day should be optimal for you. Good sources are beef, chicken, fish, and eggs.");

            //Fifth
            var mobilitySettings = LocalDBManager.Instance.GetDBSetting("IsMobility")?.Value == "true" ? "on" : "off";
            //_tipsArray.Add($"Better stretching", $"Studies have shown it's best to warm up with dynamic stretching and mobility exercises. Turn them on in Settings (currently: {mobilitySettings}).");
            lst = new List<string>();
            lst.Add($"Better warm-ups");
            lst.Add($"Warm up with mobility exercises to increase performance and lower injuries. Currently: {mobilitySettings}.");
            lst.Add("false");
            lst.Add("true");
            _tipsArray.Add(lst);
            //Sixth
            var settings = LocalDBManager.Instance.GetDBSetting("BackOffSet")?.Value == "true" ? "on" : "off";
            
           // _tipsArray.Add("Back-off sets", $"Scientists found back-off sets build muscle faster (do more reps with less weight on your last set). Turn them on in Settings (currently: {settings}).");
            lst = new List<string>();
            lst.Add("Back-off sets");
            lst.Add($"Build muscle faster by doing more reps with less weight on your last set. Currently: {settings}.");
            lst.Add("false");
            lst.Add("true");
            _tipsArray.Add(lst);

            //Seventh
            var strenghthPhaseSettings = LocalDBManager.Instance.GetDBSetting("StrengthPhase")?.Value == "true" ? "on" : "off";
            //_tipsArray.Add("Strength cycles", $"In one study, lifters who trained for strength for 3 weeks and hypertrophy for 5 weeks gained more muscle and strength. Automate strength cycles like that in Settings (currently: {strenghthPhaseSettings}.");
            lst = new List<string>();
            lst.Add("Get stronger");
            lst.Add($"Train with low reps at the end of every program to gain muscle and strength faster. Currently: {strenghthPhaseSettings}.");
            lst.Add("false");
            lst.Add("true");
            _tipsArray.Add(lst);

            //Eight
            bool? isProgress = null;
            var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
            if (workouts != null)
            {
                var Question = "";
                var progressText = "";
                if (workouts.Sets != null)
                {
                    if (workouts.Averages.Count > 1)
                    {
                        OneRMAverage last = workouts.Averages.ToList()[workouts.Averages.Count - 1];
                        OneRMAverage before = workouts.Averages.ToList()[workouts.Averages.Count - 2];
                        decimal progresskg = (last.Average.Kg - before.Average.Kg) * 100 / (before.Average.Kg < 1 ? 1 : before.Average.Kg);

                        bool inKg = LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg";

                        string progress = "";
                        string worksets = "";
                        if ((last.Average.Kg - before.Average.Kg) >= 0)
                        {
                            progress = String.Format("{0}{1}%", (last.Average.Kg - before.Average.Kg) >= 0 ? "+" : "", Math.Round(progresskg)).ReplaceWithDot();
                            isProgress = true;
                        }
                        else
                        {
                            progress = String.Format("{0}{1}%", (last.Average.Kg - before.Average.Kg) >= 0 ? "+" : "", Math.Round(progresskg)).ReplaceWithDot();
                            isProgress = false;

                        }
                        var progText = isProgress==true ? "Keep it up!" : "Consider getting in touch if your strength continues to go down.";
                        progressText =  $"Build muscle faster by getting stronger. Last 3 weeks, your strength went {progress}. {progText}";
                        //statsModel.StrengthMessage = AppResources.MaxStrength;
                        workouts.Sets.Reverse();
                        workouts.SetsDate.Reverse();

                        if (workouts.Sets.Count > 1)
                        {

                            int firstSets = workouts.Sets[workouts.Sets.Count - 1];
                            int lastSets = workouts.Sets[workouts.Sets.Count - 2];
                            try
                            {
                                decimal progressSets = (firstSets - lastSets) * 100 / (lastSets == 0 ? 1 : lastSets);
                                // strProgress += String.Format("- {0}: {1}{2} ({3}%)\n", AppResources.WorkSetsNoColon, (firstSets - lastSets) >= 0 ? "+" : "", firstSets - lastSets, Math.Round(progressSets)).ReplaceWithDot();
                                bool isWorksetProgress = false;
                                if (lastSets == 0)
                                    progressSets = firstSets;
                                if ((firstSets - lastSets) >= 0)
                                {
                                    isWorksetProgress = true;
                                    worksets = String.Format("{0}{1}{2}", (firstSets - lastSets) >= 0 ? "+" : "", Math.Round(progressSets), lastSets == 0 ? "" : "%").ReplaceWithDot();
                                }
                                else
                                {
                                    isWorksetProgress = false;
                                    worksets = String.Format("{0}{1}{2}", (firstSets - lastSets) >= 0 ? "+" : "", Math.Round(progressSets), lastSets == 0 ? "" : "%").ReplaceWithDot();
                                }

                                Question = $"Last 3 weeks, your strength went {progress} and your work sets went {worksets}.";
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        workouts.Sets.Reverse();
                        workouts.SetsDate.Reverse();

                    }
                    else
                    {

                    }

                }
            
            var adjusttext = "";
            if (isProgress == null)
            {
                adjusttext = "So if you feel tired, you can skip a workout this week.";

            }
            else if (isProgress == true)
            {
                adjusttext = "So if you feel fresh, you can add a workout this week.";
            }
            else
            {
                adjusttext = "So if you feel tired, you can skip a workout this week.";
            }
                var prg = isProgress==true ? "up" : "down";
               // _tipsArray.Add($"Your strength is going {prg}", $"{Question} {adjusttext}");
               if (!string.IsNullOrEmpty(Question) && !string.IsNullOrEmpty(adjusttext))
                { 
                    lst = new List<string>();
                    lst.Add($"Strength going {prg}");
                    lst.Add($"{Question} {adjusttext}");
                    lst.Add("false");
                    lst.Add("false");
                    _tipsArray.Add(lst);
                }
                //Nineth
                //if (!string.IsNullOrEmpty(progressText))
                //    _tipsArray.Add("Build muscle", progressText);
                if (!string.IsNullOrEmpty(progressText))
                { 
                    lst = new List<string>();
                    lst.Add("Build muscle");
                    lst.Add(progressText);
                    lst.Add("false");
                    lst.Add("false");
                    _tipsArray.Add(lst);
                }
            }
            //Tenth

            List<string> setstyles = new List<string>();
            setstyles.Add("Rest-pause");
            setstyles.Add("Normal");
            setstyles.Add("Pyramid");
            setstyles.Add("Reverse pyramid");
            var repStyle = "";
            if (LocalDBManager.Instance.GetDBSetting("IsRPyramid") != null && LocalDBManager.Instance.GetDBSetting("IsRPyramid").Value == "true")
            {
                repStyle = setstyles[2];
            }
            else if (LocalDBManager.Instance.GetDBSetting("SetStyle").Value == "Normal")
            {
                if (LocalDBManager.Instance.GetDBSetting("IsPyramid") != null && LocalDBManager.Instance.GetDBSetting("IsPyramid").Value == "true")
                {
                    repStyle = setstyles[3];
                }
                else
                {
                    repStyle = setstyles[1];
                    
                }
            }
            else if (LocalDBManager.Instance.GetDBSetting("SetStyle").Value == "RestPause")
            {
                repStyle = setstyles[0];
            }

           // _tipsArray.Add($"Rest-pause sets",$"Build muscle in less time using rest-pause sets. In one study, lifters who did 1 rest-pause set gained as much muscle as lifters who did 3 normal sets. Automate them in Settings (currently: {repStyle}).");

            lst = new List<string>();
            lst.Add("Rest-pause sets");
            lst.Add($"Build muscle in less time using rest-pause sets. They're as effective as 3 normal sets. Currently: {repStyle}.");
            lst.Add("false");
            lst.Add("true");
            _tipsArray.Add(lst);
            
            return _tipsArray;
        }

        public void OnTimerChange(int remaining)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                timerToolbarItem.Text = remaining.ToString();
            });
        }

        public void SlideGeneralAction()
        {
            try
            {
                if (SlideMenu == null)
                    return;

                //    ((RightSideMasterPage)SlideMenu).ShowGeneral();
                //if (SlideMenu.IsShown)
                //{
                //    HideMenu();
                //}
                //else
                //{
                //    ShowMenu();
                //}
                PopupNavigation.Instance.PushAsync(new FullscreenMenu());
                
            }
            catch (Exception ex)
            {

            }
        }

        public async void SlideGeneralMoreAction()
        {
            try
            {
                 Device.BeginInvokeOnMainThread(async () =>
                {
                    //if (SlideMenu == null)
                    //    return;
                    //((RightSideMasterPage)SlideMenu).ShowGeneral();

                    //if (SlideMenu.IsShown)
                    //{
                    //    HideMenu();
                    //}
                    //else
                    //{
                    //    ShowMenu();
                    //}
                    if (PopupNavigation.Instance.PopupStack.Count > 0)
                        await PopupNavigation.Instance.PopAsync();
                    PopupNavigation.Instance.PushAsync(new FullscreenMenu());
                    
                });
            }
            catch (Exception ex)
            {

            }
        }

        public void SlideGeneralBotAction()
        {
            try
            {

            ((RightSideMasterPage)SlideMenu).ShowAutoBotMenu();
            if (SlideMenu.IsShown)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }

            }
            catch (Exception ex)
            {

            }
        }
        public void SlideGeneralBotConfigureAction()
        {
            try
            {

                ((RightSideMasterPage)SlideMenu).ShowAutoBotReconfigureMenu();
                if (SlideMenu.IsShown)
                {
                    HideMenu();
                }
                else
                {
                    ShowMenu();
                }

            }
            catch (Exception ex)
            {

            }
        }




        public void SlideGeneralBotDemoAction()
        {
            try
            {
                
                ((RightSideMasterPage)SlideMenu).ShowAutoBotDemoMenu();
                if (SlideMenu.IsShown)
                {
                        HideMenu();
                }
                else
                {
                        ShowMenu();
                }

            }
            catch (Exception ex)
            {

            }
        }

        public void SetFeaturedTimer()
        {
            ((RightSideMasterPage)SlideMenu).SetFeatuedTimer();
        }

        public void Back_Clicked(object sender, EventArgs e)
        {
            PagesFactory.PopAsync();
        }

        public void SlideTimerAction()
        {
            var button = new Button();
            try
            {

            ((RightSideMasterPage)SlideMenu).ShowTimer();
            if (SlideMenu.IsShown)
            {
                HideMenu();
            }
            else
            {
                ShowMenu();
            }

            }
            catch (Exception ex)
            {

            }
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (Device.RuntimePlatform.Equals(Device.Android))
                MessagingCenter.Unsubscribe<RightSideMasterPage>(this, "BackgroundImageUpdated");
        }

        public void HideTimerIcon()
        {
            if (ToolbarItems.Count > 0)
            {
                var index = 0;
                if (this.ToolbarItems.Count == 2)
                {
                    index = 1;
                }
                this.ToolbarItems.RemoveAt(index);
                timerToolbarItem = new ToolbarItem(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value, "", SlideTimerAction, ToolbarItemOrder.Primary, 0);
                this.ToolbarItems.Insert(index, timerToolbarItem);
            }
        }

        protected void SetTimerText(string timerText)
        {
            timerToolbarItem.Text = timerText;
        }

        public virtual void OnShow()
        {
            Analytics.TrackEvent(GetType().FullName + ".OnShow");
        }

        public virtual void OnBeforeShow()
        {
            
        }

        private void CancelNotification()
        {
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1451);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1551);
            DependencyService.Get<IAlarmAndNotificationService>().CancelNotification(1651);
        }

        private void SetTrialUserNotifications()
        {
            try
            {

                CancelNotification();
                var fName = LocalDBManager.Instance.GetDBSetting("firstname").Value;
                var dt = DateTime.Now.AddDays(2);
                var timeSpan = new TimeSpan(2, dt.Hour, dt.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(10).Day - DateTime.Now.Day, dt.Hour, dt.Minute, 0);////
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"{fName}, you can do this!", timeSpan, 1451);

                var dt1 = DateTime.Now.AddDays(4);
                var timeSpan1 = new TimeSpan(4, dt1.Hour, dt1.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(15).Day - DateTime.Now.Day, dt1.Hour, dt1.Minute, 0);//// 
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", Device.RuntimePlatform.Equals(Device.Android) ? $"New users like you improve 34% in 30 days" : $"New users like you improve 34%% in 30 days", timeSpan1, 1551);

                var dt2 = DateTime.Now.AddDays(10);
                var timeSpan2 = new TimeSpan(10, dt2.Hour, dt2.Minute, 0);// new TimeSpan(DateTime.Now.AddMinutes(20).Day - DateTime.Now.Day, dt2.Hour, dt2.Minute, 0);//// 
                DependencyService.Get<IAlarmAndNotificationService>().ScheduleOnceNotification("Dr. Muscle", $"You're 12 seconds away from custom, smart workouts", timeSpan2, 1651);
            }
            catch (Exception ex)
            {

            }
        }
        public async Task<bool> CanGoFurther(bool isLoader = false)
        {
            try
            {
                if (string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("creation_date").Value))
                {
                    DateTime setDate = DateTime.Now.ToUniversalTime();
                    LocalDBManager.Instance.SetDBSetting("creation_date", setDate.Ticks.ToString());
                }
                try
                {
                    DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                    if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays < 14)
                    {
                        LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                        App.IsV1UserTrial = true;
                        App.IsFreePlan = false;
                        SetTrialUserNotifications();
                        return true;
                    }
                }
                catch (Exception ex)
                {

                }
                BooleanModel isV1User = isLoader ? await DrMuscleRestClient.Instance.IsV1User() : await DrMuscleRestClient.Instance.IsV1UserWithoutLoader();

                if (isV1User != null)
                {
                    App.IsMealPlan = isV1User.IsMealPlan;
                    DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                    if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays < 14)
                    {
                        App.IsV1UserTrial = true;
                        App.IsFreePlan = false;
                        App.IsMealPlan = isV1User.IsMealPlan;


                        LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", isV1User.IsMealPlan ? "true" : "false");

                        App.IsTraining = true;
                        return true;
                    }
                    else if (!isV1User.IsTraining)
                    {
                        App.IsFreePlan = true;
                        App.IsV1UserTrial = false;
                        App.IsV1User = false;
                        App.IsTraining = false;
                        App.IsMealPlan = isV1User.IsMealPlan;
                        LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", isV1User.IsMealPlan ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("IsPurchased", "false");
                    }
                    if (isV1User.Result)
                    {
                        App.IsV1UserTrial = isV1User.IsTraining;
                        //App.IsV1User = isV1User.IsTraining;
                        App.IsV1User = isV1User.IsTraining;
                        App.IsMealPlan = isV1User.IsMealPlan;
                        App.IsFreePlan = !isV1User.IsTraining;
                        LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", isV1User.IsMealPlan ? "true" : "false");
                        LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");

                        if (App.IsV1User)
                            return true;
                    }


                }
            }
            catch (Exception ex)
            {

            }
            finally
            {

                if (DependencyService.Get<IDrMuscleSubcription>().IsActiveSubscriptions())
                {
                    App.IsV1UserTrial = true;
                    App.IsV1User = true;
                    App.IsFreePlan = false;
                    App.IsTraining = true;
                    LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                    if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                        App.IsMealPlan = true;
                }
                if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                {
                    App.IsMealPlan = true;
                    LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", "true");
                }
            }
            return false;
        }

        public async Task<bool> CanGoFurtherForMealPlan()
        {
            try
            {

                BooleanModel isV1User =  await DrMuscleRestClient.Instance.IsV1User(); 

                if (isV1User != null)
                {
                    App.IsMealPlan = isV1User.IsMealPlan;
                    
                    if (DependencyService.Get<IDrMuscleSubcription>().IsActiveSubscriptions())
                    {
                        App.IsV1UserTrial = true;
                        App.IsV1User = true;
                        App.IsFreePlan = false;
                        App.IsTraining = true;
                        LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                        if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                            App.IsMealPlan = true;
                        return true;
                    }
                    if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                    {
                        App.IsMealPlan = true;
                        LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", "true");
                    }
                   
                }

            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<bool> CanGoFurtherWithoughtLoader()
        {
            
            if (LocalDBManager.Instance.GetDBSetting("creation_date") == null)
                return false;
            if (string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("creation_date").Value))
            {
                DateTime setDate = DateTime.Now.ToUniversalTime();
                LocalDBManager.Instance.SetDBSetting("creation_date", setDate.Ticks.ToString());
            }
            try
            {
                DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays < 14)
                {

                    App.IsV1UserTrial = true;
                    App.IsFreePlan = false;
                    LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                    SetTrialUserNotifications();
                }
            }
            catch (Exception ex)
            {

            }
            try
            {

            
            BooleanModel isV1User = await DrMuscleRestClient.Instance.IsV1UserWithoutLoaderQuick();

            if (isV1User != null)
            {

                bool isTrail = false;
                DateTime creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                App.IsMealPlan = isV1User.IsMealPlan;
                LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", isV1User.IsMealPlan ? "true" : "false");
                if ((DateTime.Now.ToUniversalTime() - creationDate).TotalDays < 14)
                {
                    isTrail = true;
                    App.IsV1UserTrial = true;
                    App.IsMealPlan = isV1User.IsMealPlan;
                }
                else if (!isV1User.IsTraining)
                {
                    App.IsFreePlan = true;
                    App.IsV1UserTrial = false;
                    App.IsV1User = false;
                    App.IsTraining = isV1User.IsTraining;
                    App.IsMealPlan = isV1User.IsMealPlan;

                    LocalDBManager.Instance.SetDBSetting("IsPurchased", "false");
                }
                if (isV1User.Result)
                {
                    App.IsV1UserTrial = isV1User.IsTraining;
                    //App.IsV1User = isV1User.IsTraining;
                    App.IsV1User = isV1User.IsTraining;
                    App.IsMealPlan = isV1User.IsMealPlan;
                    App.IsFreePlan = !isV1User.IsTraining;

                    LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                    return true;
                }

                if (DependencyService.Get<IDrMuscleSubcription>().IsActiveSubscriptions())
                {
                    App.IsV1UserTrial = true;
                    App.IsV1User = true;
                    App.IsFreePlan = false;
                    App.IsTraining = true;
                    LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                    if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                        App.IsMealPlan = true;
                        return true;
                }
                if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                {
                    App.IsMealPlan = true;
                    LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", "true");
                }
                
                if (!isTrail)
                { 
                    LocalDBManager.Instance.SetDBSetting("IsPurchased", "false");
                    App.IsV1UserTrial = false;
                    App.IsFreePlan = true;
                }
            }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (DependencyService.Get<IDrMuscleSubcription>().IsActiveSubscriptions())
                {
                    App.IsV1UserTrial = true;
                    App.IsV1User = true;
                    App.IsFreePlan = false;
                    App.IsTraining = true;
                    LocalDBManager.Instance.SetDBSetting("IsPurchased", "true");
                    if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                        App.IsMealPlan = true;
                }
                if (DependencyService.Get<IDrMuscleSubcription>().IsActiveMealPlan())
                {
                    App.IsMealPlan = true;
                    LocalDBManager.Instance.SetDBSetting("IsMealPlanPurchased", "true");
                }
            }
            return false;
            }

        protected void FirsttimeExercisePopup_OnTextChanged(PromptTextChangedArgs obj)
        {

            const string textRegex = @"^\d+(?:[\.,]\d{0,5})?$";
            var text = obj.Value.Replace(",", ".");
            bool IsValid = Regex.IsMatch(text, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (IsValid == false && !string.IsNullOrEmpty(obj.Value))
            {
                double result;
                obj.Value = obj.Value.Substring(0, obj.Value.Length - 1);
                double.TryParse(obj.Value, out result);
                obj.Value = result.ToString();
            }
        }

        protected void ExerciseRepsPopup_OnTextChanged(PromptTextChangedArgs obj)
        {
            const string textRegex = @"^\d+(?:)?$";
            bool IsValid = Regex.IsMatch(obj.Value, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (IsValid == false && !string.IsNullOrEmpty(obj.Value))
            {
                double result;
                obj.Value = obj.Value.Substring(0, obj.Value.Length - 1);
                double.TryParse(obj.Value, out result);
                obj.Value = result.ToString();
            }
        }

        protected void Name_OnTextChanged(PromptTextChangedArgs obj)
        {

            if (!string.IsNullOrEmpty(obj.Value))
            {
                if (obj.Value.Length == 1)
                    obj.Value = char.ToUpper(obj.Value[0]) + "";
                else if (obj.Value.Length > 1)
                    obj.Value = char.ToUpper(obj.Value[0]) + obj.Value.Substring(1);
            }
        }

        protected async Task RunExercise(ExerciceModel m)
        {
            CurrentLog.Instance.EndExerciseActivityPage = this.GetType();
            CurrentLog.Instance.ExerciseLog = new WorkoutLogSerieModel();
            CurrentLog.Instance.ExerciseLog.Exercice = m;

          
        }

        protected async void AskForReps(decimal weight1, string exerciseName, ExerciceModel m)
        {
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title =  string.Format("{0} {1}", CurrentLog.Instance.ExerciseLog.Exercice.Label, AppResources.Setup),
                Message = m.IsTimeBased ? $"How many seconds can you {m.Label} very easily? I'll improve on your guess after your first workout." : $"{AppResources.HowMany} {exerciseName}s can you do easily?",                
                Placeholder = AppResources.TapToEnterHowMany,
                OkText = AppResources.Ok,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (weightResponse.Ok)
                    {
                        if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                        {
                            return;
                        }
                        int reps = Convert.ToInt32(weightResponse.Value, CultureInfo.InvariantCulture);
                        SetUpCompletePopup(weight1, exerciseName, reps, true);
                    }
                }
            };
            firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        protected async void SetUpCompletePopup(decimal weight1, string exerciseName, int reps = 6, bool IsBodyweight = false)
        {

            NewExerciceLogModel model = new NewExerciceLogModel();
            model.ExerciseId = (int)CurrentLog.Instance.ExerciseLog.Exercice.Id;
            model.Username = LocalDBManager.Instance.GetDBSetting("email").Value;

            if (IsBodyweight)
            {
                model.Weight1 = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                model.Reps1 = reps.ToString();
                model.Weight2 = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                model.Reps2 = (reps - 1).ToString();
                model.Weight3 = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                model.Reps3 = (reps - 2).ToString();
            }
            else
            {
                weight1 = weight1 + (weight1 / 100);
                decimal weight2 = weight1 - (2 * weight1 / 100);
                decimal weight3 = weight2 - (2 * weight2 / 100);
                model.Weight1 = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                model.Reps1 = reps.ToString();
                model.Weight2 = new MultiUnityWeight(weight2, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                model.Reps2 = reps.ToString();
                model.Weight3 = new MultiUnityWeight(weight3, LocalDBManager.Instance.GetDBSetting("massunit").Value);
                model.Reps3 = reps.ToString();
            }

            ConfirmConfig confirmExercise = new ConfirmConfig()
            {
                Title = AppResources.SetupComplete,
                Message = string.Format("{0} {1}", exerciseName, AppResources.SetupCompleteExerciseNow),
                OkText = string.Format("{0}", exerciseName),
                CancelText = AppResources.Cancel,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (bool obj) => {
                    if (obj)
                    {
                    }
                }
            };

            UserDialogs.Instance.Confirm(confirmExercise);
        }

        public Color GetTransparentGradient()
        {
            return Color.Transparent;
            //return new Xamarin.Forms.PancakeView.GradientStopCollection() { new Xamarin.Forms.PancakeView.GradientStop() { Color = Color.Transparent, Offset = 0 }, new Xamarin.Forms.PancakeView.GradientStop() { Color = Color.Transparent, Offset = 1 } };
        }

        public Color GetBlueGradient()
        {
            return Constants.AppThemeConstants.BlueColor;
            //return new Xamarin.Forms.PancakeView.GradientStopCollection() { new Xamarin.Forms.PancakeView.GradientStop() { Color = Constants.AppThemeConstants.BlueColor, Offset = 0 }, new Xamarin.Forms.PancakeView.GradientStop() { Color = Constants.AppThemeConstants.BlueColor, Offset = 1 } };
        }

        void BackgroundImageChanged(RightSideMasterPage obj)
        {
            //if (Device.RuntimePlatform.Equals(Device.Android))
            //{
                //if (LocalDBManager.Instance.GetDBSetting("BackgroundImage") != null)
                //    BackgroundImage = LocalDBManager.Instance.GetDBSetting("BackgroundImage").Value;
                //else
                    //BackgroundImage = "Backgroundblack.png";
            //}
        }
    }

}


