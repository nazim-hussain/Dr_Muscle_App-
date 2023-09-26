using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class TimerPopup : PopupPage
    {
        public string RemainingSeconds { get; set; }
        public double Progress { get; set; }
        public string popupTitle { get; set; }
        public bool IsMax = false;
        decimal currentWeight = 0;
        private double barWeight = 0;
        private double StepValue = 5.0;
        private bool ShouldAnimate = false;
        public event EventHandler HidePopup;
        public TimerPopup(bool isPlate)
        {
            InitializeComponent();
            LblProgressSeconds.Text = RemainingSeconds;
            ProgressCircle.Progress = 0;
            Timer.Instance.OnTimerChange += OnTimerChange;
            Timer.Instance.OnTimerDone += OnTimerDone;
            RefreshLocalize();
            currentWeight = App.PCWeight;
            PlateStack.IsVisible = isPlate;
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DependencyService.Get<IFirebase>().SetScreenName("timer_popup");
            if (Device.RuntimePlatform.Equals(Device.iOS))
            { 
            MessagingCenter.Subscribe<EnterForegroundMessage>(this, "EnterForegroundMessage", (obj) =>
             {
                 Device.BeginInvokeOnMainThread(() => {
                     var remaining = Timer.Instance.Remaining;
                     LblProgressSeconds.Text = remaining.ToString();
                     var percentage = (float)remaining / Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value) * 100.0;
                     ProgressCircle.Progress = 100 - (float)percentage;
                 });

             });
            }
            var height = DeviceDisplay.MainDisplayInfo.Density > 1 ? DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density : DeviceDisplay.MainDisplayInfo.Height;
            if (height < 580)
            {
                PlateStack.HeightRequest = 100;
            }
            else
                PlateStack.HeightRequest = 150;
            if (PlateStack.IsVisible)
            {
                ButtonShowPlates_Clicked(new Button(), EventArgs.Empty);
            }
            if (App.IsDemoProgress)
            {
                //if (Device.RuntimePlatform.Equals(Device.iOS))
                    DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(HideButton, true);
                ShouldAnimate = true;
                animate(HideButton);

            }
            
        }

        async void animate(View grid)
        {
            try
            {
                if (Battery.EnergySaverStatus == EnergySaverStatus.On && Device.RuntimePlatform.Equals(Device.Android))
                    return;
                var a = new Animation();
                a.Add(0, 0.65, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 1.0, 0.7, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION A"); }));
                a.Add(0.65, 1, new Animation((v) =>
                {
                    grid.Scale = v;
                }, 0.7, 1.0, Easing.CubicInOut, () => { System.Diagnostics.Debug.WriteLine("ANIMATION B"); }));
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
                ShouldAnimate = false;
            }
        }

        public void SetTimerRepsSets(string repsText, bool isMax = false, bool isDumbbell = false)
        {
            this.IsMax = isMax;
            LblGetReadyFor.Text = isMax ? "Try max reps" : "Get ready for";// "Get ready for side 2";
            LblUpNextRepsSet.Text =  repsText;
            Timer.Instance.IsWorkTimer = false;
            if (string.IsNullOrEmpty(popupTitle))
            {
                LblGetReadyFor.IsVisible = true;
                LblRestFor.Text = AppResources.Restfor;
            }
            else
            {
                LblGetReadyFor.IsVisible = false;
                LblRestFor.Text = "Work for";
                Timer.Instance.IsWorkTimer = true;
            }
            if (isDumbbell)
            {
                LblPerHand.IsVisible = true;
            }
            else
            {
                LblPerHand.IsVisible = false;
            }
        }
        public void SetReadyForTitle()
        {
            LblGetReadyFor.Text =  "Get ready for side 2";
        }

        public void SetLastTimeText(string text, string lastWasText)
        {
            LblLastTime.IsVisible = true;
            LblLastTimeData.IsVisible = true;
            LblLastTime.Text =  text;
            LblLastTimeData.Text = lastWasText;

        }
        public void SetLastTimeOnlyText(string text, string lastWasText)
        {
            LblLastTime.Text = IsMax ? "" : text;
            LblLastTimeData.Text = lastWasText;

        }
        public void SetTimerText()
        {
            LblLastTime.IsVisible = false;
            LblLastTimeData.IsVisible = false;
            Timer.Instance.IsWorkTimer = false;
            if (string.IsNullOrEmpty(popupTitle))
            {
                LblGetReadyFor.IsVisible = true;
                LblRestFor.Text = AppResources.Restfor;
            }
            else
            {
                LblGetReadyFor.IsVisible = false;
                LblRestFor.Text = "Work for";
                Timer.Instance.IsWorkTimer = true;
            }
        }
        private void RefreshLocalize()
        {
            HideButton.Text = AppResources.Hide;
            //SkipButton.Text = AppResources.Skip;
            if (string.IsNullOrEmpty(popupTitle))
                LblRestFor.Text = AppResources.Restfor;
            else
                LblRestFor.Text = "Work for";
            LblSecondsText.Text = AppResources.Seconds;
            LblGetReadyFor.Text = AppResources.GetReadyFor;
        }
        async void OnTimerDone()
        {
            try
            {
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync();
                if (HidePopup != null)
                    HidePopup.Invoke(this, EventArgs.Empty);
                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.EndTimer }, "SendWatchMessage");
            }
            catch (Exception ex)
            {

            }
        }
        void OnTimerChange(int remaining)
        {
            try
            {
                LblProgressSeconds.Text = remaining.ToString();
                var percentage = (float)remaining / Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("timer_remaining").Value) * 100.0;
                ProgressCircle.Progress = 100 - (float)percentage;
            }
            catch (Exception ex)
            {

            }

        }

        public async void WeightCalculateAgain()
        {
            ButtonShowPlates_Clicked(new Button() { }, EventArgs.Empty);
        }
        async void ButtonShowPlates_Clicked(object sender, EventArgs e)
        {
            //PlateButton.IsVisible = false;
            
            PlateStack.Padding = new Thickness(0, 12, 0, 0);
            double? sliderVal = null;
            //if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("SlierValue")?.Value))
            //{
            //    sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("SlierValue")?.Value, System.Globalization.CultureInfo.InvariantCulture);
            //}

            if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
            {
                if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value))
                {
                    sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("KgBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                barWeight = sliderVal == null ? 20.0 : (double)sliderVal;
                if ((int)currentWeight < (int)barWeight)
                {
                    PlateStack.IsVisible = false;
                    return;
                }
                CalculateKgPlate();
            }
            else
            {
                if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value))
                {
                    sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                }
                barWeight = sliderVal == null ? 45.0 : (double)sliderVal;
                if ((int)currentWeight < (int)barWeight)
                {
                    PlateStack.IsVisible = false;
                    return;
                }
                CalculateLbsPlate();
            }
        }


        public void CalculateLbsPlate()
        {
            bool IsBar = false;
            var totalWeightLift = currentWeight;

            //Calculate for Bar
            if (totalWeightLift > 0)
            {
                IsBar = true;
                totalWeightLift = totalWeightLift - (decimal)barWeight;

            }

            var finals = (double)totalWeightLift;

            var platesItems = new List<PlateModel>();
            // calculating total weight and the difference
            var keyVal = LocalDBManager.Instance.GetDBSetting("PlatesLb").Value;

            if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
            {
                keyVal = LocalDBManager.Instance.GetDBSetting("PlatesLb").Value;
            }
            if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
            {
                keyVal = LocalDBManager.Instance.GetDBSetting("HomePlatesLb").Value;
            }
            if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
            {
                keyVal = LocalDBManager.Instance.GetDBSetting("OtherPlatesLb").Value;
            }

            string[] items = keyVal.Split('|');
            foreach (var item in items)
            {
                string[] pair = item.Split('_');
                var model = new PlateModel();
                if (pair.Length == 3)
                {
                    model.Key = pair[0];
                    try
                    {
                        model.Weight = Convert.ToDouble(pair[0].Replace(",","."), CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        model.Weight = 0;
                    }
                    model.Value = Int32.Parse(pair[1]);
                    model.IsSystemPlates = pair[2] == "True" ? true : false;
                    model.WeightType = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                    platesItems.Add(model);
                }
            }
            platesItems.Sort(delegate (PlateModel c1, PlateModel c2) { return c2.Weight.CompareTo(c1.Weight); });

            for (var i = 0; i < platesItems.Count; i++)
            {
                platesItems[i].CalculatedPlatesCount = 0;
                for (var a = 1; a <= platesItems[i].Value / 2; a++)
                {
                    if (finals >= (platesItems[i].Weight * 2))
                    {
                        platesItems[i].CalculatedPlatesCount++;
                        finals -= (platesItems[i].Weight * 2);
                    }
                }
            }

            for (var i = 0; i < platesItems.Count; i++)
            {
                platesItems[i].NotAvailablePlatesCount = 0;
                for (var a = 1; a <= 20; a++)
                {
                    if (finals >= (platesItems[i].Weight * 2))
                    {
                        platesItems[i].NotAvailablePlatesCount++;
                        finals -= (platesItems[i].Weight * 2);
                    }
                }
            }

            PlateStack.Children.Clear();
            if (IsBar && (int)barWeight > 0)
            {
                var image = new Image();
                image.Source = (int)barWeight < 45 ? "barBlankHalf.png" : "barHalf.png";
                PlateStack.Children.Add(image);
            }

            var platesWeight = 0.0;
            for (int i = 0; i < platesItems.Count(); i++)
            {
                for (int j = 0; j < platesItems[i].CalculatedPlatesCount; j++)
                {
                    platesWeight += platesItems[i].Weight;
                    if (platesItems[i].IsSystemPlates)
                    {
                        var image = new Image();
                        switch (platesItems[i].Weight)
                        {
                            case 45:
                                image.Source = "plate45half.png";
                                break;
                            case 35:
                                image.Source = "plate35half.png";
                                break;
                            case 25:
                                image.Source = "plate25half.png";
                                break;
                            case 10:
                                image.Source = "plate10half.png";
                                break;
                            case 5:
                                image.Source = "plate5half.png";
                                break;
                            case 2.5:
                                image.Source = "plate2halfhalf.png";
                                break;
                            case 1.25:
                                image.Source = "plateKg1half.png";
                                break;
                            default:
                                break;
                        }
                        PlateStack.Children.Add(image);
                    }
                    else
                    {
                        var image = new Image();
                        image.Source = "custom.png";
                        PlateStack.Children.Add(image);
                    }
                }

                //BarWeight.Text = $"{(int)slider.Value}\n";
                //BarWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? AppResources.Bar.ToUpper() : AppResources.Bar;
                //PlateWeight.Text = $"{platesWeight}\n";
                //PlateWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? $"{AppResources.Plates}".ToUpper() : AppResources.Plates;

            }

            for (int i = 0; i < platesItems.Count(); i++)
            {
                for (int j = 0; j < platesItems[i].NotAvailablePlatesCount; j++)
                {
                    platesWeight += platesItems[i].Weight;
                    if (platesItems[i].IsSystemPlates)
                    {
                        var image = new Image();
                        switch (platesItems[i].Weight)
                        {
                            case 45:
                                image.Source = "rplate45half.png";
                                break;
                            case 35:
                                image.Source = "rplate35half.png";
                                break;
                            case 25:
                                image.Source = "rplate25half.png";
                                break;
                            case 10:
                                image.Source = "rplate10half.png";
                                break;
                            case 5:
                                image.Source = "rplate5half.png";
                                break;
                            case 2.5:
                                image.Source = "rplate2halfhalf.png";
                                break;
                            case 1.25:
                                image.Source = "rplateKg1half.png";
                                break;
                            default:
                                break;
                        }
                        PlateStack.Children.Add(image);
                    }
                    else
                    {
                        var image = new Image();
                        image.Source = "rcustom.png";
                        PlateStack.Children.Add(image);
                    }
                }

                //BarWeight.Text = $"{(int)slider.Value}\n";
                //BarWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? AppResources.Bar.ToUpper() : AppResources.Bar;
                //PlateWeight.Text = $"{platesWeight}\n";
                //PlateWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? $"{AppResources.Plates}".ToUpper() : AppResources.Plates;

            }
        }



        public void CalculateKgPlate()
        {
            bool IsBar = false;
            var totalWeightLift = currentWeight;
            //Calculate for Bar

            if (totalWeightLift > 0)
            {
                IsBar = true;
                totalWeightLift = totalWeightLift - (decimal)barWeight;

            }

            // calculation function starts here

            var finals = (double)totalWeightLift;

            var platesItems = new List<PlateModel>();
            // calculating total weight and the difference
            var keyVal = LocalDBManager.Instance.GetDBSetting("PlatesKg").Value;

            if (LocalDBManager.Instance.GetDBSetting("GymEquipment")?.Value == "true")
            {
                keyVal = LocalDBManager.Instance.GetDBSetting("PlatesKg").Value;
            }
            if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
            {
                keyVal = LocalDBManager.Instance.GetDBSetting("HomePlatesKg").Value;
            }
            if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
            {
                keyVal = LocalDBManager.Instance.GetDBSetting("OtherPlatesKg").Value;
            }

            string[] items = keyVal.Split('|');
            foreach (var item in items)
            {
                string[] pair = item.Split('_');
                var model = new PlateModel();

                if (pair.Length == 3)
                {
                    model.Key = pair[0];
                    try
                    {
                        model.Weight = Convert.ToDouble(pair[0].Replace(",", "."), CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                        model.Weight = 0;
                    }
                    model.Value = Int32.Parse(pair[1]);
                    model.IsSystemPlates = pair[2] == "True" ? true : false;
                    model.WeightType = LocalDBManager.Instance.GetDBSetting("massunit").Value;
                    platesItems.Add(model);
                }
            }
            platesItems.Sort(delegate (PlateModel c1, PlateModel c2) { return c2.Weight.CompareTo(c1.Weight); });

            for (var i = 0; i < platesItems.Count; i++)
            {
                platesItems[i].CalculatedPlatesCount = 0;
                for (var a = 1; a <= platesItems[i].Value/2; a++)
                {
                    if (finals >= (platesItems[i].Weight * 2))
                    {
                        platesItems[i].CalculatedPlatesCount++;
                        finals -= (platesItems[i].Weight * 2);
                    }
                }
            }

            for (var i = 0; i < platesItems.Count; i++)
            {
                platesItems[i].NotAvailablePlatesCount = 0;
                for (var a = 1; a <= 20; a++)
                {
                    if (finals >= (platesItems[i].Weight * 2))
                    {
                        platesItems[i].NotAvailablePlatesCount++;
                        finals -= (platesItems[i].Weight * 2);
                    }
                }
            }

            PlateStack.Children.Clear();
            if (IsBar && (int)barWeight > 0)
            {
                var image = new Image();

                image.Source = (int)barWeight < 20 ? "barBlankHalf.png" : "barKgHalf.png" ;
                PlateStack.Children.Add(image);
            }

            var platesWeight = 0.0;
            for (int i = 0; i < platesItems.Count(); i++)
            {
                for (int j = 0; j < platesItems[i].CalculatedPlatesCount; j++)
                {
                    platesWeight += platesItems[i].Weight;
                    if (platesItems[i].IsSystemPlates)
                    {
                        var image = new Image();
                        switch (platesItems[i].Weight)
                        {
                            case 25:
                                image.Source = "plateKg25half.png";
                                break;
                            case 20:
                                image.Source = "plateKg20half.png";
                                break;
                            case 15:
                                image.Source = "plateKg15half.png";
                                break;
                            case 10:
                                image.Source = "plateKg10half.png";
                                break;
                            case 5:
                                image.Source = "plateKg5half.png";
                                break;
                            case 2.5:
                                image.Source = "plateKg2half.png";
                                break;
                            case 1.25:
                                image.Source = "plateKg1half.png";
                                break;
                            case 0.5:
                                image.Source = "plateKg05half.png";
                                break;
                            default:
                                break;
                        }
                        PlateStack.Children.Add(image);
                    }
                    else
                    {
                        var image = new Image();
                        image.Source = "custom.png";
                        PlateStack.Children.Add(image);
                    }
                }
                // LblBarWeight.Text = $"20\nBar";
                //LblPlatesWeight.Text = $"{platesWeight}\nSide";
                

            }

            for (int i = 0; i < platesItems.Count(); i++)
            {
                for (int j = 0; j < platesItems[i].NotAvailablePlatesCount; j++)
                {
                    platesWeight += platesItems[i].Weight;
                    if (platesItems[i].IsSystemPlates)
                    {
                        var image = new Image();
                        switch (platesItems[i].Weight)
                        {
                            case 25:
                                image.Source = "rplateKg25half.png";
                                break;
                            case 20:
                                image.Source = "rplateKg20half.png";
                                break;
                            case 15:
                                image.Source = "rplateKg15half.png";
                                break;
                            case 10:
                                image.Source = "rplateKg10half.png";
                                break;
                            case 5:
                                image.Source = "rplateKg5half.png";
                                break;
                            case 2.5:
                                image.Source = "rplateKg2half.png";
                                break;
                            case 1.25:
                                image.Source = "rplateKg1half.png";
                                break;
                            case 0.5:
                                image.Source = "rplateKg05half.png";
                                break;
                            default:
                                break;
                        }
                        PlateStack.Children.Add(image);
                    }
                    else
                    {
                        var image = new Image();
                        image.Source = "rcustom.png";
                        PlateStack.Children.Add(image);
                    }
                }
                // LblBarWeight.Text = $"20\nBar";
                //LblPlatesWeight.Text = $"{platesWeight}\nSide";


            }
        }

        async void ButtonHide_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                if (HideButton.Effects.Count > 0)
                {
                    HideButton.Effects.Remove(HideButton.Effects.Last());
                }
                Timer.Instance.OnTimerDone -= OnTimerDone;
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync();
                if (HidePopup != null)
                    HidePopup.Invoke(this, EventArgs.Empty);
                MessagingCenter.Send<SendWatchMessage>(new SendWatchMessage() { WatchMessageType = WatchMessageType.EndTimer }, "SendWatchMessage");
            }
            catch (Exception ex)
            {

            }
        }

    }
}
