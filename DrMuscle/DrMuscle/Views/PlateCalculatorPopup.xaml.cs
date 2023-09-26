using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrMuscle.Constants;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DrMuscle.Views
{
    public partial class PlateCalculatorPopup : PopupPage
    {
        decimal currentWeight = 0;
        private int barWeight = 0;
        private double StepValue = 0.5;
        private bool IsAppeared = false;


        public PlateCalculatorPopup()
        {
            InitializeComponent();
            currentWeight = CurrentLog.Instance.CurrentWeight;
            if (LocalDBManager.Instance.GetDBSetting("PlatesKg") == null || LocalDBManager.Instance.GetDBSetting("PlatesLb") == null)
            {
                var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);
                LocalDBManager.Instance.SetDBSetting("HomePlatesKg", kgString);
                LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", kgString);

                var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
                LocalDBManager.Instance.SetDBSetting("HomePlatesLb", lbString);
                LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", lbString);
            }
            if (LocalDBManager.Instance.GetDBSetting("HomePlatesKg") == null || LocalDBManager.Instance.GetDBSetting("HomePlatesLb") == null)
            {
                var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                LocalDBManager.Instance.SetDBSetting("HomePlatesKg", kgString);
                LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", kgString);

                var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                LocalDBManager.Instance.SetDBSetting("HomePlatesLb", lbString);
                LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", lbString);
            }
            refreshLocalize();
        }

        private void refreshLocalize()
        {
            EditButton.Text = $"{AppResources.Edit} ";
            LblSlideToAdjust.Text = AppResources.SlideToAdjustBarWeight;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DependencyService.Get<IFirebase>().SetScreenName("plate_calulator_popup");
            try
            {
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
                    slider.Maximum = 20.0;
                    slider.Value = sliderVal == null  ? 20.0 : (double)sliderVal;
                    slider.Minimum = 0;
                    LblMinimum.Text = "0";
                    
                    CalculateKgPlate();
            }
            else
            {
                    if (!string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value))
                    {
                        sliderVal = Convert.ToDouble(LocalDBManager.Instance.GetDBSetting("LbBarWeight")?.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    slider.Maximum = 45.0;
                    slider.Minimum =0;
                    slider.Value = sliderVal == null ? 45.0 : (double)sliderVal;
                    LblMinimum.Text = "0";
                CalculateLbsPlate();
            }
            LblSlider.Text = $"{slider.Value}";
            }
            catch (Exception ex)
            {

            }
            if (Config.ShowPlateTooltip && !Config.ShowBarSliderTooltip && !CurrentLog.Instance.IsMobilityStarted)
            {
                await Task.Delay(1000);
                Config.ShowBarSliderTooltip = true;
                DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(slider, true);
            }
            IsAppeared = true;
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
            {
                
                StepValue = 0.5;
                var newStep = Math.Round(e.NewValue / StepValue);

                slider.Value = newStep * StepValue;
                CalculateKgPlate();
                // DrMuscleRestClient.Instance.SetUserBarWeight(new UserInfosModel()
                //{
                //    KgBarWeight = (decimal)slider.Value,
                //    LbBarWeight = (decimal)slider.Value,
                //    MassUnit = "kg"
                //});
                //LocalDBManager.Instance.SetDBSetting("KgBarWeight", $"{slider.Value}".ReplaceWithDot());
            }
            else
            {

                StepValue = 1.0;

                var newStep = Math.Round(e.NewValue / StepValue);
                slider.Value = newStep * StepValue;
                CalculateLbsPlate();
                //DrMuscleRestClient.Instance.SetUserBarWeight(new UserInfosModel()
                //{
                //    KgBarWeight = (decimal)slider.Value,
                //    LbBarWeight = (decimal)slider.Value,
                //    MassUnit = "lb"
                //});
                //LocalDBManager.Instance.SetDBSetting("LbBarWeight", $"{slider.Value}".ReplaceWithDot());
            }
            
            if (IsAppeared)
            TapGestureRecognizer_Tapped(null, EventArgs.Empty);
            LblSlider.Text = $"{slider.Value}";

        }


        public void CalculateLbsPlate()
        {
            bool IsBar = false;
            var totalWeightLift = currentWeight;

            //Calculate for Bar
            if (totalWeightLift > 0)
            {
                IsBar = true;
                totalWeightLift = totalWeightLift - (decimal)slider.Value;

            }

            var finals = (decimal)totalWeightLift;

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
                        model.Weight = double.Parse(pair[0]);
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
                    if (finals >= (decimal)(platesItems[i].Weight * 2))
                    {
                        platesItems[i].CalculatedPlatesCount++;
                        platesItems[i].isAvailable = true;
                        finals -= (decimal)(platesItems[i].Weight * 2);
                    }
                }
            }
            
            for (var i = 0; i < platesItems.Count; i++)
            {
                platesItems[i].NotAvailablePlatesCount = 0;
                for (var a = 1; a <= 20; a++)
                {
                    if (finals >= (decimal)(platesItems[i].Weight * 2))
                    {
                        platesItems[i].NotAvailablePlatesCount++;
                        platesItems[i].isAvailable = false;
                        finals -= (decimal)(platesItems[i].Weight * 2);
                    }
                }
            }

            PlateStack.Children.Clear();
            if (IsBar && (int)slider.Value > 0)
            {
                var image = new Image();
                image.Source = (int)slider.Value == 45 ? "bar.png" : "barBlank.png";
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
                                image.Source = "plate45.png";
                                break;
                            case 35:
                                image.Source = "plate35.png";
                                break;
                            case 25:
                                image.Source = "plate25.png";
                                break;
                            case 10:
                                image.Source = "plate10.png";
                                break;
                            case 5:
                                image.Source = "plate5.png";
                                break;
                            case 2.5:
                                image.Source = "plate2half.png";
                                break;
                            case 1.25:
                                image.Source = "plateKg1.png";
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

                BarWeight.Text = $"{slider.Value}\n";
                BarWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? AppResources.Bar.ToUpper() : AppResources.Bar;
                if (platesWeight == 0)
                    PlateWeight.Text = $"{platesWeight}\n";
                else
                    PlateWeight.Text = $"{platesWeight} X 2\n";
                PlateWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? $"{AppResources.Plates}".ToUpper() : AppResources.Plates;

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
                                image.Source = "rplate45.png";
                                break;
                            case 35:
                                image.Source = "rplate35.png";
                                break;
                            case 25:
                                image.Source = "rplate25.png";
                                break;
                            case 10:
                                image.Source = "rplate10.png";
                                break;
                            case 5:
                                image.Source = "rplate5.png";
                                break;
                            case 2.5:
                                image.Source = "rplate2half.png";
                                break;
                            case 1.25:
                                image.Source = "rplateKg1.png";
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
            }

                BarWeight.Text = $"{slider.Value}\n";
                BarWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? AppResources.Bar.ToUpper() : AppResources.Bar;
            if (platesWeight == 0)
                PlateWeight.Text = $"{platesWeight}\n";
            else
                PlateWeight.Text = $"{platesWeight} X 2\n";
            PlateWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? $"{AppResources.Plates}".ToUpper() : AppResources.Plates;

            
        }



        public void CalculateKgPlate()
        {
            bool IsBar = false;
            var totalWeightLift = currentWeight;
            //Calculate for Bar

            if (totalWeightLift > 0)
            {
                IsBar = true;
                totalWeightLift = totalWeightLift - (decimal)slider.Value;

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
                        model.Weight = double.Parse(pair[0]);
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
            if (IsBar && (int)slider.Value > 0)
            {
                var image = new Image();
                image.Source = (int)slider.Value == 20 ? "barKg.png" : "barBlank.png";
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
                                image.Source = "plateKg25.png";
                                break;
                            case 20:
                                image.Source = "plateKg20.png";
                                break;
                            case 15:
                                image.Source = "plateKg15.png";
                                break;
                            case 10:
                                image.Source = "plateKg10.png";
                                break;
                            case 5:
                                image.Source = "plateKg5.png";
                                break;
                            case 2.5:
                                image.Source = "plateKg2.png";
                                break;
                            case 1.25:
                                image.Source = "plateKg1.png";
                                break;
                            case 0.5:
                                image.Source = "plateKg05.png";
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
                                image.Source = "rplateKg25.png";
                                break;
                            case 20:
                                image.Source = "rplateKg20.png";
                                break;
                            case 15:
                                image.Source = "rplateKg15.png";
                                break;
                            case 10:
                                image.Source = "rplateKg10.png";
                                break;
                            case 5:
                                image.Source = "rplateKg5.png";
                                break;
                            case 2.5:
                                image.Source = "rplateKg2.png";
                                break;
                            case 1.25:
                                image.Source = "rplateKg1.png";
                                break;
                            case 0.5:
                                image.Source = "rplateKg05.png";
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
            }
                    // LblBarWeight.Text = $"20\nBar";
                    //LblPlatesWeight.Text = $"{platesWeight}\nSide";
                    BarWeight.Text = $"{slider.Value}\n";
                BarWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? AppResources.Bar.ToUpper() : AppResources.Bar;
            if (platesWeight == 0)
                PlateWeight.Text = $"{platesWeight}\n";
            else
                PlateWeight.Text = $"{platesWeight} X 2\n";
            PlateWeightText.Text = Device.RuntimePlatform.Equals(Device.Android) == true ? $"{AppResources.Plates}".ToUpper() : $"{AppResources.Plates}";

            
        }

        async void Edit_Clicked(object sender, System.EventArgs e)
        {
            await PagesFactory.PushAsync<EquipmentPage>();
            try
            {
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAllAsync();
            }
            catch (Exception ex)
            {

            }
        }

        async void ButtonPlateHide_Clicked(object sender, System.EventArgs e)
        {
            //NavigationPage.SetHasNavigationBar(this, true);
            //PlateView.IsVisible = false;
            try
            {
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync();
            }
            catch (Exception ex)
            {

            }

        }

        void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            if (!CurrentLog.Instance.IsMobilityStarted)
            { 
            slider.Effects.Clear();
            if (Config.ShowPlateTooltip && Config.ShowBarSliderTooltip && !Config.ShowBarPlatesTooltip )
            {
                Config.ShowBarPlatesTooltip = true;
                DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(SliderBar, true);
                DrMuscle.Effects.TooltipEffect.SetHasShowTooltip(PlateStack, true);
            }
            else
            {
                if (Config.ShowBarPlatesTooltip )
                { 
                    SliderBar.Effects.Clear();
                    PlateStack.Effects.Clear();
                }
            }
            }
        }

        void slider_DragCompleted(System.Object sender, System.EventArgs e)
        {
            if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
            {
                DrMuscleRestClient.Instance.SetUserBarWeight(new UserInfosModel()
                {
                    KgBarWeight = (decimal)slider.Value,
                    LbBarWeight = (decimal)slider.Value,
                    MassUnit = "kg"
                });
                LocalDBManager.Instance.SetDBSetting("KgBarWeight", $"{slider.Value}".ReplaceWithDot());
            }
            else
            {

                
                DrMuscleRestClient.Instance.SetUserBarWeight(new UserInfosModel()
                {
                    KgBarWeight = (decimal)slider.Value,
                    LbBarWeight = (decimal)slider.Value,
                    MassUnit = "lb"
                });
                LocalDBManager.Instance.SetDBSetting("LbBarWeight", $"{slider.Value}".ReplaceWithDot());
            }

        }
    }
}
