using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscleWebApiSharedModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DrMuscle.Layout
{
    public partial class EquipmentPage : DrMusclePage
    {
        string[] keyValues = new string[] { };
        public ObservableCollection<PlateModel> platesItems = new ObservableCollection<PlateModel>();
        public ObservableCollection<DumbellModel> dumbellsItems = new ObservableCollection<DumbellModel>();
        public EquipmentPage()
        {
            InitializeComponent();
            PlatesListView.ItemsSource = platesItems;
            PlatesListView.ItemTapped += PlatesListView_ItemTapped;
            RefreshLocalized();
            MessagingCenter.Subscribe<Message.LanguageChangeMessage>(this, "LocalizeUpdated", (obj) =>
            {
                RefreshLocalized();
            });
        }

        private void RefreshLocalized()
        {
            Title = AppResources.Equipment;
            LblPlates.Text = AppResources.PlatesCapital;
        }

        void PlatesListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {

            if (e.Item == null)
                return;

            if (((PlateModel)e.Item).Id == -1)
            {
                //Add new plates here
                NewPlates();
            }
            PlatesListView.SelectedItem = null;

            }
            catch (Exception ex)
            {

            }
        }

        void DumbbellsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            //if (DumbbellsListView.SelectedItem == null)
            //    return;

            //if (((DumbellModel)e.Item).Id == -1)
            //{
            //    //Add new plates here
            //    //NewPlates();
            //}
            //DumbbellsListView.SelectedItem = null;
        }

        public override void OnBeforeShow()
        {
            base.OnBeforeShow();

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (LocalDBManager.Instance.GetDBSetting("PlatesKg") == null || LocalDBManager.Instance.GetDBSetting("PlatesLb") == null)
            {
                var kgString = "25_20_True|20_20_True|15_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True|0.5_20_True";
                LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);

                var lbString = "45_20_True|35_20_True|25_20_True|10_20_True|5_20_True|2.5_20_True|1.25_20_True";
                LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
            }

            if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
            {
                
                
                if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                {
                    GeneratePlatesArray("HomePlatesKg");
                }
                else if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                {
                    GeneratePlatesArray("OtherPlatesKg");
                }
                else
                {
                    GeneratePlatesArray("PlatesKg");
                }
            }
            else
            {
                if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                {
                    GeneratePlatesArray("HomePlatesLb");
                }
                else if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                {
                    GeneratePlatesArray("OtherPlatesLb");
                }
                else
                {
                    GeneratePlatesArray("PlatesLb");
                }
                
            }
        }

        private async void GeneratePlatesArray(string weightType)
        {
            var keyVal = LocalDBManager.Instance.GetDBSetting(weightType).Value;
            platesItems.Clear();
            dumbellsItems.Clear();
            string[] items = keyVal.Split('|');
            foreach (var item in items)
            {
                string[] pair = item.Split('_');
                var model = new PlateModel();
                if (pair.Length == 3)
                {
                    model.Key = pair[0];
                    model.Value = Int32.Parse(pair[1]);
                    model.IsSystemPlates = pair[2] == "True" ? true : false;
                    model.WeightType = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "lbs" : "kg";
                    platesItems.Add(model);
                }
            }
            platesItems.Add(new PlateModel()
            { Key = AppResources.TapToEnterNewPlates, Id = -1 });

            dumbellsItems.Add(new DumbellModel() { Key = "Tap to enter new dumbbell", Id = -1 });
            //PlatesListView.HeightRequest = platesItems.Count * 45;
        }

        void OnCancelClicked(object sender, System.EventArgs e)
        {
            StackLayout s = ((StackLayout)((Button)sender).Parent);
            s.Children[0].IsVisible = false;
            s.Children[1].IsVisible = false;
            s.Children[2].IsVisible = false;
            s.Children[3].IsVisible = true;
        }

        void OnContextMenuClicked(object sender, System.EventArgs e)
        {
            StackLayout s = ((StackLayout)((Button)sender).Parent);
            var mi = ((Button)sender);
            PlateModel m = (PlateModel)mi.CommandParameter;
            if (m.IsSystemPlates)
            {
                s.Children[0].IsVisible = true;
                s.Children[1].IsVisible = true;
                s.Children[2].IsVisible = false;
                s.Children[3].IsVisible = false;
            }
            else
            {
                s.Children[0].IsVisible = true;
                s.Children[1].IsVisible = true;
                s.Children[2].IsVisible = true;
                s.Children[3].IsVisible = false;
            }
        }
        public void OnEdit(object sender, EventArgs e)
        {
            var mi = ((Button)sender);
            PlateModel m = (PlateModel)mi.CommandParameter;
            OnCancelClicked(sender, e);

            if (m.IsSystemPlates)
            {
                AskForEditCounts(m);
                return;
            }
            //Edit workout log
            var massUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "lbs" : "kg";
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = $"{AppResources.EditPlateWeight}",
                Message = $"{AppResources.EnterWeights} {AppResources._in} {massUnit}",
                Text = m.Key.ToString().ReplaceWithDot(),
                OkText = AppResources.Edit,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = (weightResponse) =>
                {
                    if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 0)
                    {
                        return;
                    }
                    var weightText = weightResponse.Value.Replace(",", ".");
                    decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);
                    m.Key = weight1.ToString().ReplaceWithDot();
                    AskForEditCounts(m);

                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }
        public void OnDelete(object sender, EventArgs e)
        {
            var mi = ((Button)sender);
            PlateModel m = (PlateModel)mi.CommandParameter;
            ConfirmConfig p = new ConfirmConfig()
            {
                Title = AppResources.DeletePlates,
                Message = string.Format("{0} \"{1}\"?", AppResources.PermanentlyDelete, m.Label),
                OkText = AppResources.Delete,
                CancelText = AppResources.Cancel,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
            };
            p.OnAction = (obj) =>
            {
                if (obj)
                {
                    //Delete Here
                    if (!m.IsSystemPlates)
                    {
                        platesItems.Remove(m);
                        PlatesListView.HeightRequest = platesItems.Count * 45;
                        SaveEquipments();
                    }
                    //Save
                }
            };
            UserDialogs.Instance.Confirm(p);
        }


        void AskForEditCounts(PlateModel m)
        {
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = InputType.Number,
                IsCancellable = true,
                Title = string.Format("{0}", AppResources.EditPlateCount),
                Message = AppResources.EnterNewCount,
                Placeholder = AppResources.TapToEnterHowMany,
                Text = m.Value.ToString(),
                OkText = AppResources.Edit,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = (weightResponse) =>
                {
                    if (!weightResponse.Ok || string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 0)
                    {
                        return;
                    }
                    try
                    {
                        int plateCount = Convert.ToInt32(weightResponse.Value, CultureInfo.InvariantCulture);
                        m.Value = plateCount;
                        //Save new value
                        SaveEquipments();

                    }
                    catch (Exception ex)
                    {

                    }

                }
            };
            firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        private async void NewPlates()
        {
            PlateModel m = new PlateModel();
            var massUnit = LocalDBManager.Instance.GetDBSetting("massunit").Value == "lb" ? "lbs" : "kg";
            m.WeightType = massUnit;
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = $"{AppResources.AddPlateWeight}",
                Message = $"{AppResources.EnterWeights} {AppResources._in} {massUnit}",
                Placeholder = AppResources.EnterWeights,
                OkText = AppResources.Add,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = (weightResponse) =>
                {
                    if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 0)
                    {
                        return;
                    }
                    var weightText = weightResponse.Value.Replace(",", ".");
                    decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);
                    m.Key = weight1.ToString().ReplaceWithDot();
                    AddPlatesCounts(m);
                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        private async void AddPlatesCounts(PlateModel m)
        {
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = InputType.Number,
                IsCancellable = true,
                Title = string.Format("{0}", AppResources.AddPlateCount),
                Message = AppResources.EnterNewCount,
                Placeholder = AppResources.TapToEnterHowMany,
                OkText = AppResources.Add,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) < 1)
                    {
                        return;
                    }
                    try
                    {
                        int plateCount = Convert.ToInt32(weightResponse.Value, CultureInfo.InvariantCulture);
                        m.Value = plateCount;
                        m.IsSystemPlates = false;
                        platesItems.Insert(platesItems.Count - 1, m);
                        PlatesListView.HeightRequest = platesItems.Count * 45;
                        SaveEquipments();
                    }
                    catch (Exception ex)
                    {

                    }

                }
            };
            firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        private async void SaveEquipments()
        {
            if (LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg")
            {
                var kgString = "";
                foreach (var item in platesItems)
                {
                    if (item.Id == -1)
                        continue;
                    if (kgString != null)
                        kgString += "|";
                    kgString += $"{item.Key}_{item.Value}_{item.IsSystemPlates}";
                }
                //try
                //{
                //    var zeroPlates = platesItems.Where(x => x.Key == "0.5").FirstOrDefault();
                //    var onePlates = platesItems.Where(x => x.Key == "1.25").FirstOrDefault();

                //    decimal value = 0;
                //    if (LocalDBManager.Instance.GetDBSetting("workout_increments")?.Value != null)
                //    {
                //        value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                //    }
                //    var unit = new MultiUnityWeight(value, "kg");
                //        if (zeroPlates.Value == 0 && onePlates.Value != 0 && Math.Round(unit.Kg,2) != (decimal)2.5)
                //        {
                //            ConfirmConfig p = new ConfirmConfig()
                //            {
                //                Title = "No 0.5-kg plates",
                //                Message = "Update recommendations to match your plates?",
                //                OkText = "Update",
                //                CancelText = AppResources.Cancel,
                //                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                //            };
                            
                //            var response = await UserDialogs.Instance.ConfirmAsync(p);
                //            if (response)
                //            {
                //                LocalDBManager.Instance.SetDBSetting("workout_increments","2.5");
                //                MessagingCenter.Send<GlobalSettingsChangeMessage>(new GlobalSettingsChangeMessage() { IsDisappear = true }, "GlobalSettingsChangeMessage");
                //                await DrMuscleRestClient.Instance.SetUserIncrementsOnly(new UserInfosModel()
                //                {
                //                    Increments = new MultiUnityWeight((decimal)2.5, "kg")
                //                });
                //            }
                            
                //        }
                    

                //}
                //catch (Exception ex)
                //{

                //}
                
                if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                {
                    LocalDBManager.Instance.SetDBSetting("HomePlatesKg", kgString);
                }
                else if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                {
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesKg", kgString);
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("PlatesKg", kgString);
                }
                try
                {
                    await DrMuscleRestClient.Instance.SetUserEquipmentPlateSettings(new EquipmentModel()
                    {
                        AvilablePlate = LocalDBManager.Instance.GetDBSetting($"PlatesKg")?.Value,
                        AvilableHomePlate = LocalDBManager.Instance.GetDBSetting($"HomePlatesKg")?.Value,
                        AvilableOtherPlate = LocalDBManager.Instance.GetDBSetting($"OtherPlatesKg")?.Value,
                        AvilableLbPlate = LocalDBManager.Instance.GetDBSetting($"PlatesLb")?.Value,
                        AvilableHomeLbPlate = LocalDBManager.Instance.GetDBSetting($"HomePlatesLb")?.Value,
                        AvilableOtherLbPlate = LocalDBManager.Instance.GetDBSetting($"OtherPlatesLb")?.Value,

                    });
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                var lbString = "";
                foreach (var item in platesItems)
                {
                    if (item.Id == -1)
                        continue;
                    if (lbString != null)
                        lbString += "|";
                    lbString += $"{item.Key}_{item.Value}_{item.IsSystemPlates}";
                }
                //try
                //{
                //    var zeroPlates = platesItems.Where(x => x.Key == "2.5").FirstOrDefault();
                //    var onePlates = platesItems.Where(x => x.Key == "5").FirstOrDefault();
                //    decimal value = 0;
                //    if (LocalDBManager.Instance.GetDBSetting("workout_increments")?.Value != null)
                //    {
                //        value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("workout_increments").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                //    }
                //    var unit = new MultiUnityWeight(value, "kg");
                //        if (zeroPlates.Value == 0 && onePlates.Value != 0 && Math.Round(unit.Lb, 2) != 10)
                //        {
                //            ConfirmConfig p = new ConfirmConfig()
                //            {
                //                Title = "No 2.5-lbs plates",
                //                Message = "Update recommendations to match your plates?",
                //                OkText = "Update",
                //                CancelText = AppResources.Cancel,
                //                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                //            };

                //            var response = await UserDialogs.Instance.ConfirmAsync(p);
                //            if (response)
                //            {
                //                LocalDBManager.Instance.SetDBSetting("workout_increments", new MultiUnityWeight((decimal)10, "lb").Kg.ToString().ReplaceWithDot());
                //                MessagingCenter.Send<GlobalSettingsChangeMessage>(new GlobalSettingsChangeMessage() { IsDisappear = true }, "GlobalSettingsChangeMessage");

                //                await DrMuscleRestClient.Instance.SetUserIncrementsOnly(new UserInfosModel()
                //                {
                //                    Increments = new MultiUnityWeight((decimal)10, "lb")
                //                });
                //            }
                //        }
                    

                //}
                //catch (Exception ex)
                //{

                //}
                if (LocalDBManager.Instance.GetDBSetting("HomeEquipment")?.Value == "true")
                {
                    LocalDBManager.Instance.SetDBSetting("HomePlatesLb", lbString);
                }
                else if (LocalDBManager.Instance.GetDBSetting("OtherEquipment")?.Value == "true")
                {
                    LocalDBManager.Instance.SetDBSetting("OtherPlatesLb", lbString);
                }
                else
                {
                    LocalDBManager.Instance.SetDBSetting("PlatesLb", lbString);
                }
                try
                {
                    await DrMuscleRestClient.Instance.SetUserEquipmentPlateSettings(new EquipmentModel()
                    {
                        AvilablePlate = LocalDBManager.Instance.GetDBSetting($"PlatesKg")?.Value,
                        AvilableHomePlate = LocalDBManager.Instance.GetDBSetting($"HomePlatesKg")?.Value,
                        AvilableOtherPlate = LocalDBManager.Instance.GetDBSetting($"OtherPlatesKg")?.Value,
                        AvilableLbPlate = LocalDBManager.Instance.GetDBSetting($"PlatesLb")?.Value,
                        AvilableHomeLbPlate = LocalDBManager.Instance.GetDBSetting($"HomePlatesLb")?.Value,
                        AvilableOtherLbPlate = LocalDBManager.Instance.GetDBSetting($"OtherPlatesLb")?.Value,

                    });
                }
                catch (Exception ex)
                {

                }
            }
        }

        //TODO: New Dumbells

    }
}
